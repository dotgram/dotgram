using System;
using System.Collections.Immutable;
using System.Diagnostics;

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
				"-p:NonExistentFile=" + absent, "-getItem:CscCommandLineArgs",
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
		};
	}
}
