using System;
using System.Diagnostics;
using System.Text.Json;

namespace DotGram.Cli;

/// <summary>A <c>Using</c> item: what the SDK writes into <c>GlobalUsings.g.cs</c>.</summary>
readonly record struct GlobalUsing(string Name, string? Alias, bool Static);

/// <summary>
/// What one evaluation of a project says for one target framework: its C# files, global usings,
/// grammars and settings.
/// </summary>
/// <remarks>
/// <para>
/// An evaluation reads the project file and everything it imports — <c>Directory.Build.props</c>,
/// the SDK's own defaults. Nothing is restored, built or written.
/// </para>
/// <para>
/// No target of the project runs. The symbols a build adds for the framework and platform
/// (<c>NET8_0_OR_GREATER</c>, <c>WINDOWS</c>, <c>WINDOWS7_0_OR_GREATER</c>, …) are worked out by the SDK's
/// own targets in a project of the tool's, in a directory of its own (<see cref="ImplicitDefinesAsync"/>),
/// from what the evaluation says, so that they are the build's and not a copy of its rules.
/// </para>
/// </remarks>
sealed class ProjectEvaluation
{
	public required string             Project          { get; init; }
	public required string?            TargetFramework  { get; init; }
	public required string             TargetFrameworks { get; init; }
	public required string             AssemblyName     { get; init; }
	public required string?            LanguageVersion  { get; init; }
	public required List<string>       DefineConstants  { get; init; }
	public required string?            Nullable         { get; init; }
	public required List<string>       Compile          { get; init; }
	public required List<GlobalUsing>  Usings           { get; init; }
	public required List<string>       Grammars         { get; init; }

	/// <summary>Where restore wrote what it resolved: packages, versions, their files.</summary>
	public required string?            AssetsFile       { get; init; }

	/// <summary>The SDK's folder of targeting packs, and the version of .NET it bundles.</summary>
	public required string?            TargetingPacks   { get; init; }
	public required string?            BundledVersion   { get; init; }

	/// <summary>What the framework is: <c>.NETCoreApp</c> and <c>v10.0</c>, say.</summary>
	public required string?            FrameworkIdentifier { get; init; }
	public required string?            FrameworkVersion    { get; init; }

	/// <summary>Project references the build runs as analyzers, which the tool does not.</summary>
	public required List<string>       AnalyzerProjects { get; init; }

	/// <summary>How long working out the SDK's symbols took.</summary>
	public required TimeSpan           DefinesElapsed   { get; init; }

	/// <summary>
	/// The build properties generators are shown, by name: the two DotGram reads, and every
	/// <c>CompilerVisibleProperty</c> the project's packages declare for theirs.
	/// </summary>
	public required Dictionary<string, string> BuildProperties { get; init; }

	/// <summary>The properties a build passes to the generator that change what it writes.</summary>
	public static readonly string[] GeneratorProperties = ["DotGramTrace", "DotGramPositionalFollow"];

	/// <summary>The frameworks the project targets, in its order.</summary>
	public IReadOnlyList<string> Frameworks()
	{
		var several = TargetFrameworks.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		return several.Length > 0 ? several : TargetFramework is { Length: > 0 } one ? [one] : [];
	}

	/// <summary>The properties the SDK's define targets read, passed to the project that runs them.</summary>
	static readonly string[] DefineInputs =
	[
		"DisableImplicitFrameworkDefines", "DisableDiagnosticTracing", "TargetPlatformIdentifier", "TargetPlatformVersion",
		"SupportedOSPlatformVersion",
	];

	/// <summary>
	/// Evaluates the project once, for a framework — with the SDK's implicit symbols worked out beside
	/// it — or (null) as it stands, which says only what frameworks it targets.
	/// </summary>
	/// <exception cref="ToolException">The SDK cannot be run, or the evaluation fails.</exception>
	public static async Task<ProjectEvaluation> RunAsync(string project, string? framework, string configuration)
	{
		var arguments = new List<string> { "-p:Configuration=" + configuration };

		if (framework is not null)
			arguments.Add("-p:TargetFramework=" + framework);

		arguments.Add("-getProperty:" + string.Join(",",
		[
			"TargetFrameworks", "TargetFramework", "AssemblyName", "LangVersion", "DefineConstants", "Nullable",
			"ProjectAssetsFile", "NetCoreTargetingPackRoot", "BundledNETCoreAppPackageVersion",
			"TargetFrameworkIdentifier", "TargetFrameworkVersion",
			.. DefineInputs, .. GeneratorProperties,
		]));
		arguments.Add("-getItem:Compile,Using,AdditionalFiles,ProjectReference,CompilerVisibleProperty");

		using var json = await EvaluateAsync(project, arguments, framework).ConfigureAwait(false);

		var properties = json.RootElement.GetProperty("Properties");
		var items      = json.RootElement.GetProperty("Items");

		string? Property(string name)
		{
			return properties.TryGetProperty(name, out var value) && value.GetString() is { Length: > 0 } text ? text : null;
		}

		IEnumerable<JsonElement> Items(string name)
		{
			return items.TryGetProperty(name, out var value) ? value.EnumerateArray() : [];
		}

		static string? Metadata(JsonElement item, string name)
		{
			return item.TryGetProperty(name, out var value) && value.GetString() is { Length: > 0 } text ? text : null;
		}

		var buildProperties = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (var name in GeneratorProperties)
			if (Property(name) is { } value)
				buildProperties[name] = value;

		var defines = new List<string>((Property("DefineConstants") ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
		var elapsed = TimeSpan.Zero;

		if (framework is not null)
		{
			var visible = Items("CompilerVisibleProperty")
				.Select(static item => item.GetProperty("Identity").GetString()!)
				.Where(name => !buildProperties.ContainsKey(name))
				.Distinct(StringComparer.Ordinal)
				.ToList();

			var symbols = ImplicitDefinesAsync(project, framework, configuration, defines, DefineInputs.ToDictionary(static name => name, Property));
			var values  = visible.Count == 0 ? Task.FromResult(new Dictionary<string, string>()) : PropertiesAsync(project, framework, configuration, visible);

			var (computed, took) = await symbols.ConfigureAwait(false);

			defines = computed;
			elapsed = took;

			foreach (var (name, value) in await values.ConfigureAwait(false))
				buildProperties[name] = value;
		}

		return new ProjectEvaluation
		{
			Project             = project,
			TargetFramework     = Property("TargetFramework"),
			TargetFrameworks    = Property("TargetFrameworks") ?? "",
			AssemblyName        = Property("AssemblyName") ?? Path.GetFileNameWithoutExtension(project),
			LanguageVersion     = Property("LangVersion"),
			DefineConstants     = defines,
			Nullable            = Property("Nullable"),
			Compile             = [.. Items("Compile").Select(static item => Path.GetFullPath(item.GetProperty("FullPath").GetString()!))],
			Usings              = [.. Items("Using").Select(static item => new GlobalUsing(
				item.GetProperty("Identity").GetString()!,
				Metadata(item, "Alias"),
				string.Equals(Metadata(item, "Static"), "true", StringComparison.OrdinalIgnoreCase)))],
			Grammars            = [.. Items("AdditionalFiles")
				.Select(static item => item.GetProperty("FullPath").GetString()!)
				.Where(static path => path.EndsWith(".gram", StringComparison.OrdinalIgnoreCase))],
			AssetsFile          = Property("ProjectAssetsFile"),
			TargetingPacks      = Property("NetCoreTargetingPackRoot"),
			BundledVersion      = Property("BundledNETCoreAppPackageVersion"),
			FrameworkIdentifier = Property("TargetFrameworkIdentifier"),
			FrameworkVersion    = Property("TargetFrameworkVersion"),
			AnalyzerProjects    = [.. Items("ProjectReference")
				.Where(static item => string.Equals(Metadata(item, "OutputItemType"), "Analyzer", StringComparison.OrdinalIgnoreCase))
				.Select(static item => item.GetProperty("FullPath").GetString()!)
				.Distinct(StringComparer.Ordinal)],
			BuildProperties     = buildProperties,
			DefinesElapsed      = elapsed,
		};
	}

	/// <summary>The values of the named properties, by a second evaluation.</summary>
	static async Task<Dictionary<string, string>> PropertiesAsync(string project, string framework, string configuration, IReadOnlyList<string> names)
	{
		using var json = await EvaluateAsync(
			project,
			["-p:Configuration=" + configuration, "-p:TargetFramework=" + framework, "-getProperty:" + string.Join(",", names) + ",TargetFramework"],
			framework).ConfigureAwait(false);

		var values = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (var property in json.RootElement.GetProperty("Properties").EnumerateObject())
			if (names.Contains(property.Name) && property.Value.GetString() is { Length: > 0 } value)
				values[property.Name] = value;

		return values;
	}

	/// <summary>
	/// The symbols a build compiles with, worked out by the SDK's own targets — and nothing else — in a
	/// project of the tool's, in a temporary directory: the project's evaluated symbols and the
	/// properties those targets read are handed to it, and both targets that add and remove symbols are
	/// named, so that <c>TRACE</c> goes where <c>DisableDiagnosticTracing</c> sends it even with the
	/// framework's symbols disabled. The user's project is only evaluated; none of its targets runs.
	/// </summary>
	public static async Task<(List<string> Defines, TimeSpan Elapsed)> ImplicitDefinesAsync(
		string project, string framework, string configuration, IReadOnlyList<string> defines, IReadOnlyDictionary<string, string?> inputs)
	{
		var clock     = System.Diagnostics.Stopwatch.StartNew();
		var directory = Path.Combine(Path.GetTempPath(), "dotgram-cli-" + Guid.NewGuid().ToString("N"));

		Directory.CreateDirectory(directory);

		try
		{
			// The SDK the project builds with, which a global.json above it may pin.
			if (GlobalJson(Path.GetDirectoryName(project)!) is { } pin)
				File.Copy(pin, Path.Combine(directory, "global.json"));

			var properties = new System.Text.StringBuilder();

			void Add(string name, string? value)
			{
				if (value is not null)
					properties.Append("    <").Append(name).Append('>').Append(System.Security.SecurityElement.Escape(value)).Append("</").Append(name).Append(">\n");
			}

			Add("TargetFramework", framework);
			Add("Configuration", configuration);
			Add("EnableWindowsTargeting", "true");

			// The project's evaluated symbols, its configuration's among them already: the SDK adds that
			// one again in evaluation unless told not to.
			Add("DefineConstants", string.Join(";", defines));
			Add("DisableImplicitConfigurationDefines", "true");

			foreach (var (name, input) in inputs)
				Add(name, input);

			var wrapper = Path.Combine(directory, "defines.csproj");

			File.WriteAllText(wrapper, "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n" + properties + "  </PropertyGroup>\n</Project>\n");

			using var json = await EvaluateAsync(
				wrapper,
				[
					"-p:ImportDirectoryBuildProps=false", "-p:ImportDirectoryBuildTargets=false", "-p:ImportDirectoryPackagesProps=false",
					"-t:_DisableDiagnosticTracing;AddImplicitDefineConstants", "-getProperty:DefineConstants,TargetFramework",
				],
				framework).ConfigureAwait(false);

			var value = json.RootElement.GetProperty("Properties").GetProperty("DefineConstants").GetString() ?? "";

			return ([.. value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)], clock.Elapsed);
		}
		finally
		{
			try
			{
				Directory.Delete(directory, recursive: true);
			}
			catch (IOException)
			{
				// The temporary folder is the system's to clear; nothing in it is anyone's.
			}
		}
	}

	/// <summary>The nearest <c>global.json</c> at or above a directory.</summary>
	static string? GlobalJson(string directory)
	{
		for (var at = directory; at is not null; at = Path.GetDirectoryName(at))
			if (File.Exists(Path.Combine(at, "global.json")))
				return Path.Combine(at, "global.json");

		return null;
	}

	/// <summary>One <c>dotnet msbuild</c> of a project, its JSON answer.</summary>
	static async Task<JsonDocument> EvaluateAsync(string project, IEnumerable<string> arguments, string? framework)
	{
		var (exit, output, error) = await RunAsync(["msbuild", project, "-nologo", "-nodeReuse:false", .. arguments]).ConfigureAwait(false);

		var start = output.IndexOf('{');

		if (exit != 0 || start < 0)
			throw ToolException.Failure(
				$"evaluating {project}{(framework is null ? "" : " for " + framework)} failed (dotnet msbuild exited {exit}):" +
				Environment.NewLine + (output + error).Trim());

		return JsonDocument.Parse(output.Substring(start));
	}

	static async Task<(int Exit, string Output, string Error)> RunAsync(IEnumerable<string> arguments)
	{
		var start = new ProcessStartInfo(DotnetHost())
		{
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			UseShellExecute        = false,
		};

		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		// An evaluation of our own, not a build: no MSBuild node left behind, and no telemetry line in
		// the output the JSON is read from.
		start.Environment["MSBUILDDISABLENODEREUSE"]     = "1";
		start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
		start.Environment["DOTNET_NOLOGO"]               = "1";

		Process process;

		try
		{
			process = Process.Start(start)!;
		}
		catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
		{
			throw ToolException.Failure($"cannot run '{start.FileName}' to evaluate the project: {exception.Message}");
		}

		using (process)
		{
			var output = process.StandardOutput.ReadToEndAsync();
			var error  = process.StandardError.ReadToEndAsync();

			await process.WaitForExitAsync().ConfigureAwait(false);

			return (process.ExitCode, await output.ConfigureAwait(false), await error.ConfigureAwait(false));
		}
	}

	/// <summary>
	/// The <c>dotnet</c> that runs this tool, where it can be told; otherwise the one on the path.
	/// </summary>
	static string DotnetHost()
	{
		if (Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } host && File.Exists(host))
			return host;

		if (Environment.ProcessPath is { } process &&
			Path.GetFileNameWithoutExtension(process).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
			return process;

		return "dotnet";
	}
}
