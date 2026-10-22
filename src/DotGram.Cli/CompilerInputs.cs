using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

using Microsoft.CodeAnalysis.CSharp;

namespace DotGram.Cli;

/// <summary>
/// What a project's build gives the C# compiler for one target framework, asked of MSBuild: the
/// compiler's own command line, parsed by the compiler's own parser.
/// </summary>
/// <remarks>
/// <para>
/// MSBuild is asked as an IDE asks it when it opens a project: for the items of the target that
/// would call the compiler, with the call itself skipped. That runs the project's
/// <c>InitialTargets</c> and the targets that run before <c>CoreCompile</c> — the ones that write
/// <c>AssemblyInfo</c>, global usings and the analyzer configuration under <c>obj</c>, and any the
/// project hooks there — and nothing after it: not <c>Build</c>, not the assembly.
/// </para>
/// <para>
/// Two calls, one after the other and each framework after the one before it: two processes that
/// build one referenced project, or write one file of the project's, would collide. The first builds
/// the referenced projects whose outputs are missing or out of date, so that the references exist;
/// the second reads the compiler's arguments.
/// </para>
/// </remarks>
sealed class CompilerInputs
{
	public required string                     Moniker           { get; init; }
	public required string                     ProjectDirectory  { get; init; }
	public required CSharpCommandLineArguments Arguments         { get; init; }

	/// <summary>How long the referenced projects took to be up to date, and how long the compiler's arguments took to get.</summary>
	public required TimeSpan                   ReferencesElapsed { get; init; }
	public required TimeSpan                   InputsElapsed     { get; init; }

	/// <summary>The version of the compiler this project's build runs, or null where it could not be told.</summary>
	public required Version?                   CompilerVersion   { get; init; }

	/// <summary>What the tool noticed about the project that its user should know, said once per project.</summary>
	public required IReadOnlyList<string>      Warnings          { get; init; }

	/// <exception cref="ToolException">MSBuild fails, the project is not restored, or its arguments do not parse.</exception>
	public static CompilerInputs Read(string project, string moniker, string configuration, Version sdk)
	{
		var directory  = Path.GetDirectoryName(project)!;
		var properties = new[] { "-p:TargetFramework=" + moniker, "-p:Configuration=" + configuration };
		var clock      = Stopwatch.StartNew();

		MSBuild.Build(project, [.. properties, "-t:ResolveProjectReferences"], "MSBuild failed for " + moniker);

		var references = clock.Elapsed;

		// A file that does not exist among the compiler's inputs keeps CoreCompile from being skipped as
		// up to date: on a project that has been built, MSBuild would hand over no command line at all.
		var absent = Path.Combine(Path.GetTempPath(), "dotgram-cli-" + Guid.NewGuid().ToString("N") + ".absent");

		using var json = MSBuild.Ask(
			project,
			[
				.. properties, "-t:Compile", "-p:SkipCompilerExecution=true", "-p:ProvideCommandLineArgs=true", "-p:BuildProjectReferences=false",
				"-p:DesignTimeBuild=true", "-p:NonExistentFile=" + absent, "-getItem:CscCommandLineArgs",
				"-getProperty:RoslynTargetsPath,CscToolPath,ProjectAssetsFile,MSBuildAllProjects",
			],
			sdk,
			"MSBuild failed for " + moniker);

		// Each item's identity is one argument of the compiler, as it is: a path with a blank in it is not
		// quoted, and a quoted /features value is the compiler's parser to unquote.
		var arguments = new List<string>();

		if (json.RootElement.TryGetProperty("Items", out var items) && items.TryGetProperty("CscCommandLineArgs", out var args))
			foreach (var item in args.EnumerateArray())
				arguments.Add(item.GetProperty("Identity").GetString()!);

		if (arguments.Count == 0)
			throw ToolException.Failure($"MSBuild gave no compiler arguments for {moniker} of {project}: is it a C# project, with a Compile target?");

		var inputs = clock.Elapsed - references;
		var parsed = CSharpCommandLineParser.Default.Parse(arguments, directory, sdkDirectory: null);

		if (parsed.Errors.Length > 0)
			throw ToolException.Failure(
				$"the compiler arguments MSBuild gave for {moniker} do not parse:{Environment.NewLine}" +
				string.Join(Environment.NewLine, parsed.Errors.Select(static error => error.ToString())));

		return new CompilerInputs
		{
			Moniker           = moniker,
			ProjectDirectory  = directory,
			Arguments         = parsed,
			ReferencesElapsed = references,
			InputsElapsed     = inputs,
			CompilerVersion   = CompilerVersionOf(json.RootElement),
			Warnings          = StaleRestore(project, json.RootElement),
		};
	}

	static string Property(JsonElement root, string name)
	{
		return root.TryGetProperty("Properties", out var properties) && properties.TryGetProperty(name, out var value) ? value.GetString() ?? "" : "";
	}

	/// <summary>
	/// The version of the compiler the build runs, which is the one a generator is held to: the
	/// <c>Microsoft.CodeAnalysis</c> beside the compiler of the SDK, or of the toolset package that
	/// replaces it. Null where it cannot be found, and then the tool's own is the measure.
	/// </summary>
	static Version? CompilerVersionOf(JsonElement root)
	{
		var folders = new List<string>();

		if (Property(root, "CscToolPath") is { Length: > 0 } tool)
		{
			folders.Add(tool);
			folders.Add(Path.Combine(tool, "bincore"));
		}

		if (Property(root, "RoslynTargetsPath") is { Length: > 0 } roslyn)
			folders.Add(Path.Combine(roslyn, "bincore"));

		foreach (var folder in folders)
		{
			var assembly = Path.Combine(folder, "Microsoft.CodeAnalysis.dll");

			try
			{
				if (File.Exists(assembly))
					return AssemblyName.GetAssemblyName(assembly).Version;
			}
			catch (Exception exception) when (exception is BadImageFormatException or IOException or UnauthorizedAccessException)
			{
				// Not a compiler we can read; the next folder, or the tool's own measure.
			}
		}

		return null;
	}

	/// <summary>
	/// A warning where the restore the project was last given is older than a file that can change
	/// it: the project file, and the <c>Directory.Packages.props</c> and <c>Directory.Build.*</c> files
	/// it imports. The tool does not restore; the build would.
	/// </summary>
	static IReadOnlyList<string> StaleRestore(string project, JsonElement root)
	{
		var assets = Property(root, "ProjectAssetsFile");

		if (assets.Length == 0 || !File.Exists(assets))
			return [];

		var restored = File.GetLastWriteTimeUtc(assets);
		var warnings = new List<string>();

		var files = Property(root, "MSBuildAllProjects")
			.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Where(static file =>
			{
				var name = Path.GetFileName(file);

				return name.Equals("Directory.Packages.props", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Directory.Build.", StringComparison.OrdinalIgnoreCase);
			})
			.Prepend(project)
			.Distinct(StringComparer.Ordinal);

		foreach (var file in files)
			if (File.Exists(file) && File.GetLastWriteTimeUtc(file) > restored)
				warnings.Add($"restore may be stale: {file} is newer than {assets}; if package references changed, run `dotnet restore` (the tool never restores).");

		return warnings;
	}
}
