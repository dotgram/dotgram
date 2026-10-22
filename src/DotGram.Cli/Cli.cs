using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

		// Each framework's generation starts as its inputs are known, while MSBuild is asked for the
		// next framework's: a large grammar takes seconds of one core for each, and the frameworks of one
		// project share nothing while they are generated.
		var runs = options.Kind == InputKind.Project ? Projects(options) : [Grammar(options)];

		foreach (var warning in runs.SelectMany(static run => run.Warnings).Distinct(StringComparer.Ordinal))
			output.WriteLine("warning: " + warning);

		var outputs = new List<(Pass Run, Generation Generation)>();

		foreach (var run in runs)
		{
			var generation = run.Generated.GetAwaiter().GetResult();
			var generated  = generation.Output;

			outputs.Add((run, generation));

			foreach (var note in run.Notes)
				output.WriteLine($"  {run.Inputs.Moniker}: {note}");

			foreach (var summary in generated.Summaries)
				output.WriteLine($"  {run.Inputs.Moniker}: {summary}");

			output.WriteLine(string.Create(
				CultureInfo.InvariantCulture,
				$"  {run.Inputs.Moniker}: {generated.Files.Count} files, {generated.Files.Values.Sum(static text => (long)text.Length):N0} characters in {generated.Elapsed.TotalSeconds:F1} s"));
		}

		// Errors and warnings, as a build shows them; the generator's information is counted only.
		var reported = outputs.SelectMany(static one => one.Generation.Output.Diagnostics).ToList();

		Report.Write(output, reported, static diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning);

		var information = reported
			.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Info)
			.Select(Report.Format)
			.Distinct(StringComparer.Ordinal)
			.Count();

		if (information > 0)
			output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  {information} information messages of the generator not shown (a build does not show them either)."));

		if (outputs.Any(static one => one.Generation.Output.HasErrors))
		{
			var errors = reported.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

			// A C# type the grammar names that is not in view: a typo, or a type the tool was not given.
			// Where that is every error, it is the second as much as the first, and said so.
			if (errors.Any(static diagnostic => diagnostic.Id == NotInView))
				output.WriteLine(
					"A C# type the grammar names is not in view. If it is declared elsewhere, " +
					(options.Kind == InputKind.Grammar
						? "add the file that declares it with --sources (or --reference its assembly)."
						: "reference the project or assembly that declares it, or declare it in the project."));

			output.WriteLine("Grammar errors: nothing written.");

			return errors.All(static diagnostic => diagnostic.Id == NotInView) ? ExitCode.Failure : ExitCode.GrammarErrors;
		}

		foreach (var (run, generation) in outputs)
		{
			var exit = Check(run, generation, options, output);

			if (exit != ExitCode.Done)
				return exit;
		}

		var files = Files([.. outputs.Select(static one => (one.Run, one.Generation.Output))], options);

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

	/// <summary>
	/// One generation to run: its inputs, and in <c>.gram</c> mode the host it is given. It is started
	/// when it is made, and <see cref="Generated"/> is what it comes to.
	/// </summary>
	/// <param name="Others">The project's other generators, run beside DotGram's.</param>
	/// <param name="Warnings">What the tool could not do as the build does, or does differently.</param>
	/// <param name="Notes">What to say of the pass beside its results.</param>
	/// <param name="OtherDotGram">The version of DotGram the project has where it is not the tool's: its output is then checked in the project.</param>
	sealed record Pass(
		GenerationInputs                 Inputs,
		ImmutableArray<SyntaxTree>       ExtraSources,
		bool                             CompileOutput,
		string?                          HostFile,
		string?                          HostText,
		IReadOnlyList<ISourceGenerator>  Others,
		IReadOnlyList<string>            Warnings,
		IReadOnlyList<string>            Notes,
		string?                          OtherDotGram)
	{
		public Task<Generation> Generated { get; } = Task.Run(() => Generation.Of(Inputs, ExtraSources, Others, OtherDotGram is not null));
	}

	/// <summary>What one pass came to: what the generator wrote, and the errors the build would report.</summary>
	/// <param name="Errors">Errors in the project's own code and its other generators' output.</param>
	/// <param name="OutputErrors">Errors in DotGram's output, looked for only where the project's DotGram is another.</param>
	sealed record Generation(GeneratorOutput Output, List<Diagnostic> Errors, TimeSpan Checked, List<Diagnostic> OutputErrors)
	{
		public static Generation Of(GenerationInputs inputs, ImmutableArray<SyntaxTree> extraSources, IReadOnlyList<ISourceGenerator> others, bool checkOutput)
		{
			var generated = Generator.Run(inputs, extraSources, others);
			var written   = inputs.Sources.AddRange(extraSources.IsDefault ? [] : extraSources);

			// Not where the generator has refused: nothing is written then, and what it says is the answer.
			if (generated.HasErrors)
				return new Generation(generated, [], TimeSpan.Zero, []);

			var (errors, elapsed) = Refusal.Errors(written, generated);

			return new Generation(generated, errors, elapsed, checkOutput ? Refusal.OutputErrors(generated) : []);
		}
	}

	/// <summary>The project's frameworks, each asked of MSBuild after the one before, and generated as soon as it is asked.</summary>
	static List<Pass> Projects(GenerateOptions options)
	{
		if (!File.Exists(options.Input))
			throw ToolException.Failure($"project '{options.Input}' does not exist.");

		var sdk        = MSBuild.SdkOf(options.Input);
		var frameworks = options.Framework is not null ? [options.Framework] : MSBuild.Frameworks(options.Input, options.Configuration, sdk);

		if (frameworks.Count == 0)
			throw ToolException.Failure($"{options.Input} names no target framework.");

		var listed = OutputFolder.Listed(options.Output);

		return [.. frameworks.Select(framework => PassFor(CompilerInputs.Read(options.Input, framework, options.Configuration, sdk), options, listed))];
	}

	/// <summary>One framework of a project: what its build gives the compiler, and the generators among its analyzers.</summary>
	static Pass PassFor(CompilerInputs inputs, GenerateOptions options, IReadOnlyList<string> listed)
	{
		var arguments = inputs.Arguments;
		var analyzers = AnalyzerGenerators.Load(arguments.AnalyzerReferences.Select(reference => Path.GetFullPath(reference.FilePath, inputs.ProjectDirectory)));
		var warnings  = new List<string>(analyzers.Warnings);
		var notes     = new List<string>
		{
			string.Create(
				CultureInfo.InvariantCulture,
				$"compiler inputs from MSBuild in {(inputs.ReferencesElapsed + inputs.InputsElapsed).TotalSeconds:F1} s (referenced projects {inputs.ReferencesElapsed.TotalSeconds:F1} s, arguments {inputs.InputsElapsed.TotalSeconds:F1} s)"),
		};

		string? other = null;

		if (!analyzers.HasDotGram)
			notes.Add($"the project lists no DotGram analyzer; generating with DotGram {GeneratorVersion}");
		else if (analyzers.DotGramVersion is { } version && version != GeneratorVersion)
		{
			warnings.Add($"the project references DotGram {version}; generating with {GeneratorVersion}");
			other = version;
		}

		// What the build compiles. The output folder's own files, as the last run wrote them, are not
		// among them: a generator in a build never sees its own output, and a parser that saw the last
		// one could answer otherwise. Another generator's checked-in file beside them is compiled.
		var output  = Path.GetFullPath(options.Output);
		var sources = arguments.SourceFiles
			.Select(static file => file.Path)
			.Where(path => !IsOurs(path, output, listed))
			.ToList();

		var parsing = arguments.ParseOptions;
		var trees   = GenerationInputs.Parse(sources, parsing);
		var files   = arguments.AdditionalFiles.Select(static file => (AdditionalText)new FileText(file.Path, GenerationInputs.Read(file.Path))).ToImmutableArray();

		var generation = new GenerationInputs
		{
			Moniker            = inputs.Moniker,
			AssemblyName       = arguments.CompilationName ?? Path.GetFileNameWithoutExtension(options.Input),
			ParseOptions       = parsing,
			CompilationOptions = arguments.CompilationOptions,
			Sources            = trees,
			AdditionalFiles    = files,
			References         = [.. arguments.ResolveMetadataReferences(new ReferenceFiles(inputs.ProjectDirectory, arguments.ReferencePaths))],
			Options            = ConfiguredOptions.Read(arguments.AnalyzerConfigPaths, trees, files),
		};

		return new Pass(generation, default, false, null, null, analyzers.Generators, warnings, notes, other);
	}

	/// <summary>
	/// A file in the output folder that the last run wrote, as its manifest says: nothing else, so that
	/// another generator's checked-in <c>.g.cs</c> beside it is compiled as the build compiles it.
	/// </summary>
	static bool IsOurs(string path, string output, IReadOnlyList<string> listed)
	{
		var names = FileSystem.NamesIn(output);

		return names.Equals(Path.GetDirectoryName(path), Path.TrimEndingDirectorySeparator(output)) &&
			listed.Contains(Path.GetFileName(path), names);
	}

	/// <summary>The references the compiler was given, found as it finds them: as written, against the project's directory, then in <c>/lib:</c> folders.</summary>
	sealed class ReferenceFiles(string baseDirectory, ImmutableArray<string> searchPaths) : MetadataReferenceResolver
	{
		public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties)
		{
			foreach (var candidate in new[] { Path.GetFullPath(reference, baseDirectory) }.Concat(searchPaths.Select(folder => Path.Combine(folder, reference))))
				if (File.Exists(candidate))
					return [MetadataReference.CreateFromFile(candidate, properties)];

			// The build says CS0006 for it, and fails.
			throw ToolException.Failure($"the reference '{reference}' does not exist: the project's build reports CS0006 for it; build the project that makes it, or restore.");
		}

		public override bool Equals(object? other)
		{
			return ReferenceEquals(this, other);
		}

		public override int GetHashCode()
		{
			return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
		}
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
			Moniker            = framework.Moniker,
			AssemblyName       = className,
			ParseOptions       = parsing,
			CompilationOptions = new CSharpCompilationOptions(
				OutputKind.DynamicallyLinkedLibrary,
				nullableContextOptions: NullableContextOptions.Enable,
				allowUnsafe: true,
				concurrentBuild: true),
			Sources            = sources,
			AdditionalFiles    = [new FileText(options.Input, GenerationInputs.Read(options.Input))],
			References         = GenerationInputs.ReferencesFor(framework, options.References, options.Packages),
			Options            = ConfiguredOptions.Empty,
		};

		return new Pass(inputs, [host], true, hostFile, hostText, [], [], [], null);
	}

	/// <summary>
	/// What stops a run between generating and writing: an error the project's build would report in
	/// its own code, DotGram's output of another version not compiling in the project, and in
	/// <c>.gram</c> mode a parser that does not compile.
	/// </summary>
	static int Check(Pass run, Generation generation, GenerateOptions options, TextWriter output)
	{
		var mode    = run.CompileOutput;
		var moniker = run.Inputs.Moniker;
		var errors  = generation.Errors;

		if (errors.Count > 0)
		{
			var advice = mode
				? "add the file that declares it with --sources, or --reference its assembly"
				: "your project's build reports this too; if package references changed, run `dotnet restore`";

			foreach (var error in errors)
				output.WriteLine(Report.Format(error, options.AllowUnresolved ? "warning" : "error"));

			if (!options.AllowUnresolved)
			{
				output.WriteLine(
					$"{errors.Count} errors in {moniker} stop generation as they stop the project's build — a half-written file does too: {advice}. " +
					"Nothing written; --allow-unresolved generates anyway.");

				return ExitCode.Failure;
			}
		}

		output.WriteLine(string.Create(
			CultureInfo.InvariantCulture,
			$"  {moniker}: sources checked in {generation.Checked.TotalSeconds:F1} s"));

		if (run.OtherDotGram is { } version && generation.OutputErrors.Count > 0)
			throw ToolException.Failure(
				$"the project references DotGram {version}, and the output of the tool's DotGram {GeneratorVersion} does not compile in it for {moniker}:{Environment.NewLine}" +
				string.Join(Environment.NewLine, generation.OutputErrors.Select(Report.Format)));

		if (mode)
		{
			var generated   = generation.Output;
			var diagnostics = Refusal.Diagnostics(generated.Updated, generated.Updated.SyntaxTrees);
			var failures    = diagnostics
				.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
				.OrderBy(static diagnostic => Report.Format(diagnostic), StringComparer.Ordinal)
				.ToList();

			if (failures.Count > 0)
			{
				Report.Write(output, failures, static _ => true);
				output.WriteLine("The generated parser does not compile with its host and sources: nothing written.");
				return ExitCode.GrammarErrors;
			}
		}

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
				firstFramework = run.Inputs.Moniker;
				continue;
			}

			var differs = first.Keys.Union(files.Keys)
				.FirstOrDefault(name => !first.TryGetValue(name, out var a) || !files.TryGetValue(name, out var b) || a != b);

			if (differs is not null)
				throw ToolException.Failure(
					$"{differs}: the parser generated for {firstFramework} differs from the one for {run.Inputs.Moniker}, " +
					"and the folder is compiled by both. Generate each framework into a folder of its own with --framework.");
		}

		return first ?? [];
	}
}
