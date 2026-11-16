using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DotGram.LanguageServer;

/// <summary>
/// What MSBuild makes of one project: the C# files it compiles, the grammars among its additional
/// files, the projects whose assemblies it references, and its global usings — after every glob,
/// <c>Remove</c>, condition and import, <c>Directory.Build.props</c> and a restored package's build
/// files among them.
/// </summary>
sealed record ProjectEvaluation(
	string Project,
	IReadOnlyList<string> Compile,
	IReadOnlyList<string> Grammars,
	IReadOnlyList<string> References,
	IReadOnlyList<string> Usings)
{
	// Injected after Microsoft.Common.targets: a target that runs nothing and returns the items, each with
	// what it is and which project it is of. Only this target is built — and the project's InitialTargets,
	// which MSBuild runs before any — so nothing is compiled, restored or written.
	const string Targets = """
		<Project>
		  <Target Name="DotGramEditorItems" Returns="@(_DotGramEditorItem)">
		    <ItemGroup>
		      <_DotGramEditorItem Include="@(Compile->'%(FullPath)')" DotGramKind="Compile" DotGramProject="$(MSBuildProjectFullPath)" />
		      <_DotGramEditorItem Include="@(AdditionalFiles->'%(FullPath)')" DotGramKind="AdditionalFiles" DotGramProject="$(MSBuildProjectFullPath)" />
		      <_DotGramEditorItem Include="@(ProjectReference->'%(FullPath)')" DotGramKind="ProjectReference" DotGramProject="$(MSBuildProjectFullPath)" />
		      <_DotGramEditorItem Include="@(Using)" DotGramKind="Using" DotGramProject="$(MSBuildProjectFullPath)" />
		    </ItemGroup>
		  </Target>
		</Project>
		""";

	// Injected after the cross-targeting targets of a project with several frameworks, whose outer
	// evaluation has no C# of its own: the same target, asked of the project once for each framework.
	const string CrossTargets = """
		<Project>
		  <Target Name="DotGramEditorItems" Returns="@(_DotGramEditorItem)">
		    <ItemGroup>
		      <_DotGramEditorFramework Include="$(TargetFrameworks)" />
		    </ItemGroup>
		    <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="DotGramEditorItems" BuildInParallel="true"
		             Properties="TargetFramework=%(_DotGramEditorFramework.Identity)">
		      <Output TaskParameter="TargetOutputs" ItemName="_DotGramEditorItem" />
		    </MSBuild>
		  </Target>
		</Project>
		""";

	/// <summary>
	/// Evaluates the projects with the <c>dotnet</c> on the path, in one MSBuild run in
	/// <paramref name="directory"/>, where a <c>global.json</c> may choose the SDK: every project at once,
	/// in parallel. A project MSBuild cannot evaluate is missing from the answer.
	/// </summary>
	/// <exception cref="InvalidOperationException">MSBuild could not be run, or said nothing that can be read.</exception>
	public static IReadOnlyDictionary<string, ProjectEvaluation> Evaluate(
		IReadOnlyList<string> projects,
		string directory,
		StringComparer paths,
		CancellationToken cancellationToken)
	{
		var result = new Dictionary<string, ProjectEvaluation>(paths);

		if (projects.Count == 0)
			return result;

		var scratch = Path.Combine(Path.GetTempPath(), "dotgram-editor-" + Guid.NewGuid().ToString("N"));

		Directory.CreateDirectory(scratch);

		try
		{
			var targets = Path.Combine(scratch, "Items.targets");
			var cross   = Path.Combine(scratch, "CrossItems.targets");
			var all     = Path.Combine(scratch, "All.proj");
			var answer  = Path.Combine(scratch, "answer.json");

			File.WriteAllText(targets, Targets);
			File.WriteAllText(cross, CrossTargets);
			File.WriteAllText(all, Traversal(projects, targets, cross));

			var output = Run("dotnet", directory, ["msbuild", all, "-nologo", "-nodeReuse:false", "-v:q", "-t:Run", "-getTargetResult:Run", "-getResultOutputFile:" + answer], cancellationToken);

			using var json = File.Exists(answer)
				? JsonDocument.Parse(File.ReadAllText(answer, Encoding.UTF8))
				: Answer(output) ?? throw new InvalidOperationException("MSBuild gave no answer: " + output.Trim());

			var items = json.RootElement.GetProperty("TargetResults").GetProperty("Run").GetProperty("Items");
			var found = new Dictionary<string, (List<string> Compile, List<string> Grammars, List<string> References, List<string> Usings)>(paths);

			foreach (var item in items.EnumerateArray())
			{
				var project = Text(item, "DotGramProject");

				if (project.Length == 0)
					continue;

				if (!found.TryGetValue(project, out var lists))
					found[project] = lists = ([], [], [], []);

				var identity = Text(item, "Identity");

				switch (Text(item, "DotGramKind"))
				{
					case "Compile" when identity.EndsWith(".cs", StringComparison.OrdinalIgnoreCase):
						lists.Compile.Add(identity);
						break;

					case "AdditionalFiles" when identity.EndsWith(DotGram.Language.GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase):
						lists.Grammars.Add(identity);
						break;

					// A reference to a project's assembly; not one to an analyzer, or one that takes only its build order.
					case "ProjectReference" when
						!string.Equals(Text(item, "ReferenceOutputAssembly"), "false", StringComparison.OrdinalIgnoreCase) &&
						!string.Equals(Text(item, "OutputItemType"), "Analyzer", StringComparison.OrdinalIgnoreCase):
						lists.References.Add(identity);
						break;

					case "Using":
						lists.Usings.Add(GlobalUsing(identity, Text(item, "Alias"), string.Equals(Text(item, "Static"), "true", StringComparison.OrdinalIgnoreCase)));
						break;
				}
			}

			// A project of several frameworks answers once for each: its items are those of all of them, once.
			foreach (var (project, lists) in found)
				result[project] = new ProjectEvaluation(
					project,
					[.. lists.Compile.Distinct(paths)],
					[.. lists.Grammars.Distinct(paths)],
					[.. lists.References.Distinct(paths)],
					[.. lists.Usings.Distinct(StringComparer.Ordinal)]);

			return result;
		}
		finally
		{
			try
			{
				Directory.Delete(scratch, recursive: true);
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
			{
				// The temporary folder is the system's to clear.
			}
		}
	}

	/// <summary>A <c>global using</c> as the SDK writes one for a <c>Using</c> item.</summary>
	public static string GlobalUsing(string name, string? alias, bool isStatic)
	{
		return alias is { Length: > 0 }
			? $"global using {alias} = global::{name};"
			: isStatic ? $"global using static global::{name};" : $"global using global::{name};";
	}

	static string Traversal(IReadOnlyList<string> projects, string targets, string cross)
	{
		var result = new StringBuilder();

		result.Append("<Project>\n  <ItemGroup>\n");

		foreach (var project in projects)
			result.Append("    <_DotGramProject Include=\"").Append(Escape(project)).Append("\" />\n");

		// ContinueOnError: a project that cannot be evaluated is left out of the answer, not the run.
		result.Append($"""
			  </ItemGroup>
			  <Target Name="Run" Returns="@(_DotGramItem)">
			    <MSBuild Projects="@(_DotGramProject)" Targets="DotGramEditorItems" BuildInParallel="true" SkipNonexistentTargets="true" ContinueOnError="true"
			             Properties="CustomAfterMicrosoftCommonTargets={Escape(targets)};CustomAfterMicrosoftCommonCrossTargetingTargets={Escape(cross)};DesignTimeBuild=true">
			      <Output TaskParameter="TargetOutputs" ItemName="_DotGramItem" />
			    </MSBuild>
			  </Target>
			</Project>

			""");

		return result.ToString();
	}

	/// <summary>A path as an MSBuild item or property holds it: its special characters escaped.</summary>
	static string Escape(string path)
	{
		var result = new StringBuilder(path.Length);

		foreach (var character in path)
		{
			if (character is '%' or '$' or '@' or '\'' or ';' or '?' or '*' or '"' or '&' or '<' or '>')
				result.Append('%').Append(((int)character).ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
			else
				result.Append(character);
		}

		return result.ToString();
	}

	static string Text(JsonElement item, string name)
	{
		return item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";
	}

	/// <summary>The JSON answer in what MSBuild printed, where it wrote no file: from a line that begins with <c>{</c> to the end.</summary>
	static JsonDocument? Answer(string output)
	{
		for (var start = output.IndexOf('{'); start >= 0; start = output.IndexOf('{', start + 1))
		{
			if (start > 0 && output[start - 1] != '\n')
				continue;

			try
			{
				return JsonDocument.Parse(output.Substring(start));
			}
			catch (JsonException)
			{
				// Not the answer: a line of something else that begins with a brace.
			}
		}

		return null;
	}

	/// <summary>Runs a program; what it printed. A cancellation kills it.</summary>
	internal static string Run(string program, string directory, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
	{
		var start = new ProcessStartInfo(program)
		{
			WorkingDirectory       = directory,
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			UseShellExecute        = false,
			CreateNoWindow         = true,
		};

		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		// The SDK's own, not this process's: a server run by `dotnet` from the .NET Install Tool has its
		// own DOTNET_ROOT, which may hold no SDK.
		start.Environment.Remove("MSBuildExtensionsPath");
		start.Environment.Remove("MSBuildSDKsPath");
		start.Environment.Remove("MSBUILD_EXE_PATH");
		start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
		start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
		start.Environment["DOTNET_NOLOGO"] = "1";

		using var process = Process.Start(start) ?? throw new InvalidOperationException(program + " could not be started.");

		var output = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
		var error  = process.StandardError.ReadToEndAsync(CancellationToken.None);

		using (cancellationToken.Register(() =>
		{
			try
			{
				process.Kill(entireProcessTree: true);
			}
			catch (InvalidOperationException)
			{
				// It has ended.
			}
		}))
		{
			process.WaitForExit();

			// What it printed, read to the end while a cancellation still kills and stops the wait: a child
			// it left running may hold the output open after it has exited.
			Task.WaitAll([output, error], cancellationToken);
		}

		cancellationToken.ThrowIfCancellationRequested();

		if (process.ExitCode != 0)
			throw new InvalidOperationException($"{program} exited with {process.ExitCode}: {(output.Result + error.Result).Trim()}");

		return output.Result;
	}
}
