using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using DotGram.Generation;

using Microsoft.CodeAnalysis;

namespace DotGram.Cli;

/// <summary><c>dotnet dotgram</c>: the commands, and what each exits with.</summary>
static class Cli
{
	public static int Main(string[] arguments)
	{
		return Run(arguments, Console.Out, Console.Error, Directory.GetCurrentDirectory());
	}

	/// <summary>Runs one command line, writing to the two writers given; what <c>Main</c> runs, and what the tests run.</summary>
	public static int Run(IReadOnlyList<string> arguments, TextWriter output, TextWriter error, string currentDirectory)
	{
		try
		{
			switch (CommandLine.Parse(arguments, currentDirectory))
			{
				case Command.Usage:
					error.WriteLine(Help.Usage);
					return ExitCode.Failure;

				case Command.Help:
					output.WriteLine(Help.Text);
					return ExitCode.Done;

				case Command.GenerateHelp:
					output.WriteLine(Help.Generate);
					return ExitCode.Done;

				case Command.Version:
					output.WriteLine($"dotnet-dotgram {ToolVersion} (generator {GeneratorVersion})");
					return ExitCode.Done;

				case Command.Skill:
					output.Write(Help.Skill());
					return ExitCode.Done;

				case Command.Generate(var options):
					return Generate(options, output);

				default:
					throw new InvalidOperationException("A command the dispatch does not know.");
			}
		}
		catch (ToolException exception)
		{
			error.WriteLine("dotnet dotgram: error: " + exception.Message);

			if (exception.IsUsage)
				error.WriteLine("Run 'dotnet dotgram --help' for the commands and options.");

			return exception.ExitCode;
		}
	}

	/// <summary>The tool's version, without the commit a build may append to it.</summary>
	public static string ToolVersion => VersionOf(typeof(Cli).Assembly);

	/// <summary>The generator's version: the DotGram package this tool carries.</summary>
	public static string GeneratorVersion => VersionOf(typeof(GramGenerator).Assembly);

	static string VersionOf(Assembly assembly)
	{
		var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
			?? assembly.GetName().Version?.ToString()
			?? "0.0.0";

		var plus = version.IndexOf('+');

		return plus < 0 ? version : version.Substring(0, plus);
	}

	static int Generate(GenerateOptions options, TextWriter output)
	{
		var clock = Stopwatch.StartNew();

		output.WriteLine("dotnet dotgram " + options.Describe());

		var runs = options.Kind == InputKind.Project ? Projects(options) : [Grammar(options)];

		var outputs = new List<(Pass Run, GeneratorOutput Output)>();

		// Each framework's generation at once: a large grammar takes seconds of one core for each, and
		// the frameworks of one project share nothing while they are generated.
		var generations = runs.Select(static run => Task.Run(() => Generator.Run(run.Inputs, run.ExtraSources))).ToArray();

		// And beside them what a project's own files cannot resolve, which needs nothing generated.
		var unresolved = runs.Select(static run => run.CompileOutput
			? null
			: Task.Run(() => Unresolved.Diagnostics(run.Inputs.Compile(), run.Inputs.Sources))).ToArray();

		for (var index = 0; index < runs.Count; index++)
		{
			var run       = runs[index];
			var generated = generations[index].GetAwaiter().GetResult();

			outputs.Add((run, generated));

			foreach (var summary in generated.Summaries)
				output.WriteLine($"  {run.Inputs.Framework.Moniker}: {summary}");

			output.WriteLine(string.Create(
				CultureInfo.InvariantCulture,
				$"  {run.Inputs.Framework.Moniker}: {generated.Files.Count} files, {generated.Files.Values.Sum(static text => (long)text.Length):N0} characters in {generated.Elapsed.TotalSeconds:F1} s"));
		}

		// Errors and warnings, as a build shows them; the generator's information is counted only.
		var reported = outputs.SelectMany(static one => one.Output.Diagnostics).ToList();

		Report.Write(output, reported, static diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning);

		var information = reported
			.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Info)
			.Select(Report.Format)
			.Distinct(StringComparer.Ordinal)
			.Count();

		if (information > 0)
			output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  {information} information messages of the generator not shown (a build does not show them either)."));

		if (outputs.Any(static one => one.Output.HasErrors))
		{
			var errors = reported.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

			// A C# type the grammar names that is not in view: a typo, or a type the tool was not given.
			// Where that is every error, it is the second as much as the first, and said so.
			if (errors.Any(static diagnostic => diagnostic.Id == NotInView))
				output.WriteLine(
					"A C# type the grammar names is not in view. If it is declared elsewhere, " +
					(options.Kind == InputKind.Grammar
						? "add the file that declares it with --sources (or --reference its assembly)."
						: "pass the assembly that declares it with --reference <file.dll|dir> or --package <Name>@<Version>."));

			output.WriteLine("Grammar errors: nothing written.");

			return errors.All(static diagnostic => diagnostic.Id == NotInView) ? ExitCode.Failure : ExitCode.GrammarErrors;
		}

		for (var index = 0; index < outputs.Count; index++)
		{
			var (run, generated) = outputs[index];

			var exit = Check(run, generated, unresolved[index]?.GetAwaiter().GetResult(), options, output);

			if (exit != ExitCode.Done)
				return exit;
		}

		var files = Files(outputs, options);

		var manifest = OutputFolder.Manifest(files, ToolVersion, GeneratorVersion);

		var (written, unchanged, deleted) = OutputFolder.Write(options.Output, files, manifest);

		output.WriteLine(string.Create(
			CultureInfo.InvariantCulture,
			$"Wrote {written} files into {options.Output} ({unchanged} unchanged, {deleted} deleted) in {clock.Elapsed.TotalSeconds:F1} s."));

		output.WriteLine(
			$"The generator writes CRLF line ends: '{Relative(options.Output)}/** -text' in .gitattributes keeps them as written.");

		return ExitCode.Done;

		string Relative(string path)
		{
			return Path.GetFileName(path.TrimEnd('/', '\\'));
		}
	}

	/// <summary>The generator's "no C# type of this name is in view".</summary>
	const string NotInView = "GRAM3004";

	/// <summary>One generation to run: its inputs, and in <c>.gram</c> mode the host it is given.</summary>
	sealed record Pass(GenerationInputs Inputs, ImmutableArray<SyntaxTree> ExtraSources, bool CompileOutput, string? HostFile, string? HostText);

	/// <summary>The project evaluated for each of its frameworks, in parallel, and the inputs of each.</summary>
	static List<Pass> Projects(GenerateOptions options)
	{
		if (!File.Exists(options.Input))
			throw ToolException.Failure($"project '{options.Input}' does not exist.");

		var evaluations = Task.Run(async () =>
		{
			if (options.Framework is not null)
				return new[] { await ProjectEvaluation.RunAsync(options.Input, options.Framework, options.Configuration).ConfigureAwait(false) };

			var outer      = await ProjectEvaluation.RunAsync(options.Input, null, options.Configuration).ConfigureAwait(false);
			var frameworks = outer.Frameworks();

			if (frameworks.Count == 0)
				throw ToolException.Failure($"{options.Input} names no target framework.");

			return await Task.WhenAll(frameworks.Select(framework => ProjectEvaluation.RunAsync(options.Input, framework, options.Configuration))).ConfigureAwait(false);
		}).GetAwaiter().GetResult();

		var listed = OutputFolder.Listed(options.Output);

		return [.. evaluations.Select(evaluation => new Pass(Inputs(evaluation, options, listed), default, false, null, null))];
	}

	static GenerationInputs Inputs(ProjectEvaluation evaluation, GenerateOptions options, IReadOnlyList<string> listed)
	{
		var moniker   = evaluation.TargetFramework ?? options.Framework ?? "";
		var framework = Framework.Of(moniker)
			?? throw ToolException.Failure(
				$"{evaluation.Project} targets '{moniker}', which the tool carries no reference assemblies for ({Framework.Supported}). " +
				"Pass --framework with one it does, into an output folder of its own.");

		// The project's symbols are the build's already, the framework's and platform's among them
		// (ProjectEvaluation); only what the command line adds goes beside them.
		var parsing = GenerationInputs.Parsing(
			framework,
			options.LanguageVersion ?? evaluation.LanguageVersion,
			[.. evaluation.DefineConstants, .. options.Defines]);

		// What this tool wrote into the folder, where the project compiles it: the generator in a
		// build never sees its own output, and a parser that saw the last one could answer otherwise.
		var output = Path.GetFullPath(options.Output);
		var sources = evaluation.Compile
			.Where(path => !IsOurs(path, output, listed))
			.Distinct(FileSystem.Names)
			.ToList();

		var trees = GenerationInputs.Parse(sources, parsing);

		var usings = GenerationInputs.GlobalUsings(
			evaluation.Usings,
			parsing,
			Path.Combine(Path.GetDirectoryName(evaluation.Project)!, "obj", evaluation.AssemblyName + ".GlobalUsings.g.cs"));

		return new GenerationInputs
		{
			Framework       = framework,
			AssemblyName    = evaluation.AssemblyName,
			ParseOptions    = parsing,
			Nullable        = GenerationInputs.NullableOf(evaluation.Nullable),
			Sources         = usings is null ? trees : trees.Add(usings),
			Grammars        = [.. evaluation.Grammars.Distinct(StringComparer.Ordinal).Select(static path => (AdditionalText)new GrammarText(path, GenerationInputs.Read(path)))],
			References      = GenerationInputs.ReferencesFor(framework, options.References, options.Packages),
			BuildProperties = evaluation.BuildProperties.ToImmutableDictionary(),
		};
	}

	/// <summary>
	/// A file in the output folder that the last run wrote, as its manifest says: nothing else, so that
	/// another generator's checked-in <c>.g.cs</c> beside it is compiled as the build compiles it.
	/// </summary>
	static bool IsOurs(string path, string output, IReadOnlyList<string> listed)
	{
		return FileSystem.Names.Equals(Path.GetDirectoryName(path), Path.TrimEndingDirectorySeparator(output)) &&
			listed.Contains(Path.GetFileName(path), FileSystem.Names);
	}

	/// <summary>A <c>.gram</c> input: its host synthesized, its options from the command line.</summary>
	static Pass Grammar(GenerateOptions options)
	{
		if (!File.Exists(options.Input))
			throw ToolException.Failure($"grammar '{options.Input}' does not exist.");

		var moniker = options.Framework ?? "net10.0";

		// A platform's symbols and version come from the SDK's data for it, which a project asks for and
		// a lone grammar has nobody to ask.
		if (moniker.Contains('-', StringComparison.Ordinal))
			throw ToolException.Usage($"--framework '{moniker}' names a platform, which only a project can say how to compile for; generate the project.");

		var framework = Framework.Of(moniker)
			?? throw ToolException.Usage($"--framework '{moniker}' is not one the tool carries references for ({Framework.Supported}).");

		var gramOptions = GrammarHost.Options();
		var arguments   = options.GramOptions.Select(option => GrammarHost.Argument(option.Name, option.Value, gramOptions)).ToList();
		var className   = GrammarHost.ClassName(options);
		var hostText    = GrammarHost.Text(options, className, arguments);
		var hostFile    = className + ".Host.g.cs";

		var parsing = GenerationInputs.Parsing(
			framework,
			options.LanguageVersion,
			["TRACE", options.Configuration.Equals("Debug", StringComparison.OrdinalIgnoreCase) ? "DEBUG" : "RELEASE", .. framework.ImplicitSymbols, .. options.Defines]);

		var sources = GenerationInputs.Parse(GrammarHost.Sources(options.Sources), parsing);

		if (GrammarHost.SecondHost(sources, Path.GetFileName(options.Input)) is { } other)
			throw ToolException.Failure(
				$"{other}: a [Gram] in the sources already claims {Path.GetFileName(options.Input)}; generate that host's project instead, " +
				"or leave the attribute off and keep the partial class for the members its hooks call.");

		var host = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
			hostText, parsing, Path.Combine(Path.GetFullPath(options.Output), hostFile), System.Text.Encoding.UTF8);

		var inputs = new GenerationInputs
		{
			Framework       = framework,
			AssemblyName    = className,
			ParseOptions    = parsing,
			Nullable        = Microsoft.CodeAnalysis.NullableContextOptions.Enable,
			Sources         = sources,
			Grammars        = [new GrammarText(options.Input, GenerationInputs.Read(options.Input))],
			References      = GenerationInputs.ReferencesFor(framework, options.References, options.Packages),
			BuildProperties = ImmutableDictionary<string, string>.Empty,
		};

		return new Pass(inputs, [host], true, hostFile, hostText);
	}

	/// <summary>
	/// What stops a run between generating and writing: a type a grammar asks about that the
	/// compilation cannot resolve, and in <c>.gram</c> mode a parser that does not compile.
	/// </summary>
	/// <param name="sourceDiagnostics">A project's own files' diagnostics, found while it was generated; null in <c>.gram</c> mode.</param>
	static int Check(Pass run, GeneratorOutput generated, List<Diagnostic>? sourceDiagnostics, GenerateOptions options, TextWriter output)
	{
		var mode  = run.CompileOutput;
		var clock = Stopwatch.StartNew();

		// In `.gram` mode the host and the parser are compiled with the sources, since nothing else
		// would compile them before they are written. A project's are not: what is looked for there
		// is the names its own files cannot resolve, and those are found in the compilation the
		// generator was handed — binding them against the generated files as well costs seconds on a
		// large grammar — with every name the generated files declare taken back out.
		var diagnostics = mode
			? Unresolved.Diagnostics(generated.Updated, generated.Updated.SyntaxTrees)
			: sourceDiagnostics!;

		var unresolved = Unresolved.Of(diagnostics)
			.Where(name => mode || !(generated.GeneratedTypeNames.Contains(name.Name) || generated.GeneratedTypeNames.Contains(name.Name + "Attribute")))
			.ToList();

		var dependencies   = Unresolved.Dependencies(run.Inputs.Sources.AddRange(run.ExtraSources.IsDefault ? [] : run.ExtraSources), generated.AskedTypeNames);
		var (asked, other) = Unresolved.Split(unresolved, generated.AskedTypeNames, dependencies);
		var moniker        = run.Inputs.Framework.Moniker;

		if (other.Count > 0)
			output.WriteLine(
				$"  {moniker}: {other.Count} unresolved names no grammar asks about, which change nothing generated: " +
				string.Join(", ", other.Select(static name => name.Name).Distinct(StringComparer.Ordinal).Take(10)) +
				(other.Select(static name => name.Name).Distinct(StringComparer.Ordinal).Count() > 10 ? ", …" : ""));

		if (asked.Count > 0)
		{
			var advice = mode
				? "add the file that declares it with --sources (or --reference its assembly)"
				: "pass the assembly that declares it with --reference <file.dll|dir> or --package <Name>@<Version>";

			foreach (var name in asked.DistinctBy(static name => name.Name))
				output.WriteLine(
					$"{name.Where}: {(options.AllowUnresolved ? "warning" : "error")}: '{name.Name}' cannot be resolved for {moniker} ({name.Diagnostic.Id}), and " +
					(name.Through is null ? "a grammar asks about it" : $"the declaration of '{name.Through}', which a grammar's answer depends on, names it") +
					$": {advice}.");

			if (!options.AllowUnresolved)
			{
				output.WriteLine("A type a grammar asks about is missing, and the parser could differ from a build's: nothing written. --allow-unresolved generates anyway.");
				return ExitCode.Failure;
			}
		}

		if (mode)
		{
			var errors = diagnostics
				.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
				.OrderBy(static diagnostic => Report.Format(diagnostic), StringComparer.Ordinal)
				.ToList();

			if (errors.Count > 0)
			{
				Report.Write(output, errors, static _ => true);
				output.WriteLine("The generated parser does not compile with its host and sources: nothing written.");
				return ExitCode.GrammarErrors;
			}
		}

		output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  {moniker}: names checked in {clock.Elapsed.TotalSeconds:F1} s"));

		return ExitCode.Done;
	}

	/// <summary>
	/// The files to write: each framework's, which must be the same, with the <c>#line</c> paths made
	/// relative where they are to be.
	/// </summary>
	static SortedDictionary<string, string> Files(List<(Pass Run, GeneratorOutput Output)> outputs, GenerateOptions options)
	{
		var directory = Path.GetFullPath(options.Output);
		SortedDictionary<string, string>? first = null;
		string? firstFramework = null;

		foreach (var (run, generated) in outputs)
		{
			var inputs = run.Inputs.InputPaths().ToHashSet(StringComparer.Ordinal);
			var files  = new SortedDictionary<string, string>(StringComparer.Ordinal);

			foreach (var (name, text) in generated.Files)
				files[name] = options.LinePaths == LinePaths.Relative ? LineDirectives.Relative(text, inputs, directory, generated.Trees[name]) : text;

			if (run.HostFile is not null)
				files[run.HostFile] = run.HostText!;

			if (first is null)
			{
				first          = files;
				firstFramework = run.Inputs.Framework.Moniker;
				continue;
			}

			var differs = first.Keys.Union(files.Keys)
				.FirstOrDefault(name => !first.TryGetValue(name, out var a) || !files.TryGetValue(name, out var b) || a != b);

			if (differs is not null)
				throw ToolException.Failure(
					$"{differs}: the parser generated for {firstFramework} differs from the one for {run.Inputs.Framework.Moniker}, " +
					"and the folder is compiled by both. Generate each framework into a folder of its own with --framework.");
		}

		return first ?? [];
	}
}
