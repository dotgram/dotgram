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
/// One target runs: the SDK's <c>AddImplicitDefineConstants</c>, which only works out the symbols a
/// build adds for the framework and platform (<c>NET8_0_OR_GREATER</c>, <c>WINDOWS</c>,
/// <c>WINDOWS7_0_OR_GREATER</c>, …), by the SDK's own rules and properties
/// (<c>DisableImplicitFrameworkDefines</c>, <c>DisableDiagnosticTracing</c>), so that the defines are
/// the build's and not a copy of its rules. It reads properties and writes nothing.
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

	/// <summary>The build properties the generator reads, by name, where the project sets them.</summary>
	public required Dictionary<string, string> BuildProperties { get; init; }

	/// <summary>The properties a build passes to the generator that change what it writes.</summary>
	public static readonly string[] GeneratorProperties = ["DotGramTrace", "DotGramPositionalFollow"];

	/// <summary>The frameworks the project targets, in its order.</summary>
	public IReadOnlyList<string> Frameworks()
	{
		var several = TargetFrameworks.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		return several.Length > 0 ? several : TargetFramework is { Length: > 0 } one ? [one] : [];
	}

	/// <summary>
	/// Evaluates the project once, for a framework — with the SDK's implicit symbols worked out — or
	/// (null) as it stands, which says only what frameworks it targets.
	/// </summary>
	/// <exception cref="ToolException">The SDK cannot be run, or the evaluation fails.</exception>
	public static async Task<ProjectEvaluation> RunAsync(string project, string? framework, string configuration)
	{
		var arguments = new List<string>
		{
			"msbuild", project, "-nologo", "-nodeReuse:false",
			"-p:Configuration=" + configuration,
		};

		if (framework is not null)
			arguments.Add("-p:TargetFramework=" + framework);

		// The target is the inner build's: a project of several frameworks has none outside them.
		if (framework is not null)
			arguments.Add("-t:AddImplicitDefineConstants");

		arguments.Add("-getProperty:" + string.Join(",", ["TargetFrameworks", "TargetFramework", "AssemblyName", "LangVersion", "DefineConstants", "Nullable", .. GeneratorProperties]));
		arguments.Add("-getItem:Compile,Using,AdditionalFiles");

		var (exit, output, error) = await RunAsync(arguments).ConfigureAwait(false);

		var start = output.IndexOf('{');

		if (exit != 0 || start < 0)
			throw ToolException.Failure(
				$"evaluating {project}{(framework is null ? "" : " for " + framework)} failed (dotnet msbuild exited {exit}):" +
				Environment.NewLine + (output + error).Trim());

		using var json = JsonDocument.Parse(output.Substring(start));

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

		var buildProperties = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (var name in GeneratorProperties)
			if (Property(name) is { } value)
				buildProperties[name] = value;

		return new ProjectEvaluation
		{
			Project          = project,
			TargetFramework  = Property("TargetFramework"),
			TargetFrameworks = Property("TargetFrameworks") ?? "",
			AssemblyName     = Property("AssemblyName") ?? Path.GetFileNameWithoutExtension(project),
			LanguageVersion  = Property("LangVersion"),
			DefineConstants  = [.. (Property("DefineConstants") ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)],
			Nullable         = Property("Nullable"),
			Compile          = [.. Items("Compile").Select(static item => item.GetProperty("FullPath").GetString()!)],
			Usings           = [.. Items("Using").Select(static item => new GlobalUsing(
				item.GetProperty("Identity").GetString()!,
				item.TryGetProperty("Alias", out var alias) && alias.GetString() is { Length: > 0 } text ? text : null,
				item.TryGetProperty("Static", out var isStatic) && string.Equals(isStatic.GetString(), "true", StringComparison.OrdinalIgnoreCase)))],
			Grammars         = [.. Items("AdditionalFiles")
				.Select(static item => item.GetProperty("FullPath").GetString()!)
				.Where(static path => path.EndsWith(".gram", StringComparison.OrdinalIgnoreCase))],
			BuildProperties  = buildProperties,
		};
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
