using System;
using System.Collections.Immutable;
using System.Diagnostics;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotGram.Cli;

/// <summary>What one run of the generator wrote and said, for one target framework.</summary>
sealed class GeneratorOutput
{
	/// <summary>Each file by the name a build gives it (its hint name), with the text the generator wrote.</summary>
	public required SortedDictionary<string, string> Files { get; init; }

	/// <summary>Each file's tree, as the driver parsed it, by the same name.</summary>
	public required Dictionary<string, SyntaxTree> Trees { get; init; }

	/// <summary>What the generator reported: GRAM errors, warnings, information.</summary>
	public required ImmutableArray<Diagnostic> Diagnostics { get; init; }

	/// <summary>The generator's own one-line summary of each parser, as a build prints it.</summary>
	public required ImmutableArray<string> Summaries { get; init; }

	/// <summary>The compilation with every generator's files in it, DotGram's among them.</summary>
	public required Compilation Updated { get; init; }

	/// <summary>
	/// The compilation as the build has it before DotGram's files are added: what the user wrote, and
	/// what the project's other generators add to it.
	/// </summary>
	public required Compilation Checked { get; init; }

	/// <summary>The other generators' trees, which <see cref="Checked"/> holds.</summary>
	public required ImmutableArray<SyntaxTree> OtherTrees { get; init; }

	/// <summary>The errors the other generators reported: the build fails on them.</summary>
	public required ImmutableArray<Diagnostic> OtherErrors { get; init; }

	public required TimeSpan Elapsed { get; init; }

	public bool HasErrors => Diagnostics.Any(static diagnostic => diagnostic is { Severity: DiagnosticSeverity.Error, IsSuppressed: false });
}

/// <summary>The generator, run as a build runs it, outside a build.</summary>
static class Generator
{
	/// <summary>
	/// What every build that reads the summary asks for. It only adds the summary files, which the
	/// tool reads for its progress lines and does not write.
	/// </summary>
	const string ReportProperty = "DotGramReportGeneration";

	const string ReportSuffix       = ".DotGramReport.g.cs";
	const string ReportDetailSuffix = ".DotGramReportDetail.g.cs";

	/// <summary>
	/// The failure of DotGram's own generator as the user reads it: what failed in a line, and the stack trace
	/// for <c>--verbose</c>. Two sources that declare the same generated file make Roslyn throw from
	/// <c>AddSource</c>, which is what a nested project that the outer one did not exclude does.
	/// </summary>
	static ToolException Failed(Exception exception, Compilation compilation)
	{
		var detail = exception.ToString();

		if (exception is ArgumentException { ParamName: "hintName" } && HintOf(exception.Message) is { } hint)
		{
			var text = $"two sources declare the generated file '{hint}': a class generated twice. A nested project that the outer project's files include does this; exclude it with Compile Remove.";

			if (DeclaredAs(compilation, hint) is { Length: > 0 } files)
				text += " Declared in: " + files + ".";

			return new ToolException(text, ExitCode.Failure) { Detail = detail };
		}

		return new ToolException($"the generator failed: {exception.GetType().Name}: {exception.Message}", ExitCode.Failure) { Detail = detail };
	}

	/// <summary>The name Roslyn quotes in "The hintName '...' of the added source file must be unique".</summary>
	static string? HintOf(string message)
	{
		const string Start = "hintName '";

		var from = message.IndexOf(Start, StringComparison.Ordinal);

		if (from < 0)
			return null;

		from += Start.Length;

		var to = message.IndexOf('\'', from);

		return to < 0 ? null : message.Substring(from, to - from);
	}

	/// <summary>The files of the classes whose generated file is called <paramref name="hint"/>, where they can be told.</summary>
	static string DeclaredAs(Compilation compilation, string hint)
	{
		var name = hint.EndsWith(".g.cs", StringComparison.Ordinal) ? hint.Substring(0, hint.Length - ".g.cs".Length) : hint;

		try
		{
			var paths = new SortedSet<string>(StringComparer.Ordinal);

			foreach (var symbol in compilation.GetSymbolsWithName(static _ => true, SymbolFilter.Type))
				if (symbol is INamedTypeSymbol type && type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) == name)
					foreach (var location in type.Locations)
						if (location.SourceTree?.FilePath is { Length: > 0 } path)
							paths.Add(path);

			return string.Join(", ", paths);
		}
		catch (Exception)
		{
			// The files are a help to the reader; the message is complete without them.
			return "";
		}
	}

	/// <param name="others">The project's other generators, run beside DotGram's as the build runs them.</param>
	public static GeneratorOutput Run(GenerationInputs inputs, ImmutableArray<SyntaxTree> extraSources, IReadOnlyList<ISourceGenerator> others)
	{
		var clock = Stopwatch.StartNew();

		var compilation = inputs.Compile();

		if (!extraSources.IsDefaultOrEmpty)
			compilation = compilation.AddSyntaxTrees(extraSources);

		var gram = new GramGenerator().AsSourceGenerator();

		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			[gram, .. others],
			inputs.AdditionalFiles,
			inputs.ParseOptions,
			inputs.Options.WithGlobal("build_property." + ReportProperty, "summary"),
			new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None));

		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out _);

		var results = driver.GetRunResult().Results;
		var result  = results.Single(one => ReferenceEquals(one.Generator, gram));

		if (result.Exception is { } exception)
			throw Failed(exception, compilation);

		var files     = new SortedDictionary<string, string>(StringComparer.Ordinal);
		var trees     = new Dictionary<string, SyntaxTree>(StringComparer.Ordinal);
		var summaries = ImmutableArray.CreateBuilder<string>();

		foreach (var source in result.GeneratedSources)
		{
			var text = source.SourceText.ToString();

			if (source.HintName.EndsWith(ReportSuffix, StringComparison.Ordinal))
				summaries.Add(text.TrimStart('/', ' ').TrimEnd());
			else if (!source.HintName.EndsWith(ReportDetailSuffix, StringComparison.Ordinal))
			{
				files.Add(source.HintName, text);
				trees.Add(source.HintName, source.SyntaxTree);
			}
		}

		// A generator that throws adds nothing, and the build says so as an error (CS8785): the run stops.
		if (results.FirstOrDefault(one => !ReferenceEquals(one.Generator, gram) && one.Exception is not null) is { Exception: { } failure } failed)
		{
			var type = failed.Generator.GetGeneratorType();

			throw new ToolException($"the generator {type.FullName} in {type.Assembly.Location} failed: {failure.GetType().Name}: {failure.Message}", ExitCode.Failure) { Detail = failure.ToString() };
		}

		var otherTrees = results
			.Where(one => !ReferenceEquals(one.Generator, gram))
			.SelectMany(static one => one.GeneratedSources.Select(static source => source.SyntaxTree))
			.ToImmutableArray();

		return new GeneratorOutput
		{
			Files       = files,
			Trees       = trees,
			Diagnostics = Severities.Apply(result.Diagnostics, inputs.Options, inputs.CompilationOptions),
			Summaries   = summaries.ToImmutable(),
			Updated     = updated,
			Checked     = compilation.AddSyntaxTrees(otherTrees),
			OtherTrees  = otherTrees,
			OtherErrors = [.. results.Where(one => !ReferenceEquals(one.Generator, gram))
				.SelectMany(static one => one.Diagnostics)
				.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)],
			Elapsed     = clock.Elapsed,
		};
	}
}
