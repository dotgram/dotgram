using System;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Cli;

/// <summary>What one run of the generator wrote and said, for one target framework.</summary>
sealed class GeneratorOutput
{
	/// <summary>Each file by the name a build gives it (its hint name), with the text the generator wrote.</summary>
	public required SortedDictionary<string, string> Files { get; init; }

	/// <summary>What the generator reported: GRAM errors, warnings, information.</summary>
	public required ImmutableArray<Diagnostic> Diagnostics { get; init; }

	/// <summary>The generator's own one-line summary of each parser, as a build prints it.</summary>
	public required ImmutableArray<string> Summaries { get; init; }

	/// <summary>The compilation the generator was handed: what the user wrote, and nothing generated.</summary>
	public required Compilation Input { get; init; }

	/// <summary>The compilation with the generated files in it.</summary>
	public required Compilation Updated { get; init; }

	/// <summary>The name of every type the generated files declare, nested ones included.</summary>
	public required ImmutableHashSet<string> GeneratedTypeNames { get; init; }

	/// <summary>Every identifier in a type name some grammar asks the compilation about.</summary>
	public required ImmutableHashSet<string> AskedTypeNames { get; init; }

	public required TimeSpan Elapsed { get; init; }

	public bool HasErrors => Diagnostics.Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
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

	public static GeneratorOutput Run(GenerationInputs inputs, ImmutableArray<SyntaxTree> extraSources = default)
	{
		var clock = Stopwatch.StartNew();

		var compilation = inputs.Compile();

		if (!extraSources.IsDefaultOrEmpty)
			compilation = compilation.AddSyntaxTrees(extraSources);

		var properties = inputs.BuildProperties.SetItem(ReportProperty, "summary");

		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			[new GramGenerator().AsSourceGenerator()],
			inputs.Grammars,
			inputs.ParseOptions,
			new BuildPropertiesProvider(properties),
			new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var driverDiagnostics);

		var result = driver.GetRunResult().Results.Single();

		if (result.Exception is { } exception)
			throw ToolException.Failure($"the generator failed: {exception}");

		var files     = new SortedDictionary<string, string>(StringComparer.Ordinal);
		var summaries = ImmutableArray.CreateBuilder<string>();

		foreach (var source in result.GeneratedSources)
		{
			var text = source.SourceText.ToString();

			if (source.HintName.EndsWith(ReportSuffix, StringComparison.Ordinal))
				summaries.Add(text.TrimStart('/', ' ').TrimEnd());
			else if (!source.HintName.EndsWith(ReportDetailSuffix, StringComparison.Ordinal))
				files.Add(source.HintName, text);
		}

		return new GeneratorOutput
		{
			Files          = files,
			Diagnostics    = driverDiagnostics,
			Summaries      = summaries.ToImmutable(),
			Input          = compilation,
			Updated        = updated,
			GeneratedTypeNames = DeclaredTypeNames(result.GeneratedSources.Select(static source => source.SyntaxTree)),
			AskedTypeNames = AskedTypeNames(result),
			Elapsed        = clock.Elapsed,
		};
	}

	/// <summary>
	/// The identifiers of every type a grammar asks the compilation about — constructors, settable
	/// properties, whether it exists, what it fits — read from the generator's first stage.
	/// </summary>
	/// <remarks>
	/// The questions are the generator's own and not public, so they are read from the values the
	/// driver tracks for the stage the generator names. A generator that changes their shape fails
	/// here, loudly, and a test of the tool says where; it never silently asks nothing.
	/// </remarks>
	static ImmutableHashSet<string> AskedTypeNames(GeneratorRunResult result)
	{
		var names = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

		if (!result.TrackedSteps.TryGetValue(GramGenerator.AskedStage, out var steps))
			return names.ToImmutable();

		foreach (var step in steps)
			foreach (var (value, _) in step.Outputs)
			{
				var questions = Member(value, "Questions");
				var items     = Member(questions, "Items") as IEnumerable
					?? throw Unreadable("Questions.Items is not a sequence");

				foreach (var question in items)
				{
					var kind = (int)(Member(question, "Kind") ?? throw Unreadable("a question has no Kind"));

					// The two that ask about methods name a method, not a type; what the others name,
					// and what any of them is asked against, is a type.
					if (kind is not (ExternalValueKind or ExternalMethodKind))
						Identifiers((string?)Member(question, "Name"), names);

					Identifiers((string?)Member(question, "Against"), names);
				}
			}

		return names.ToImmutable();
	}

	/// <summary>Every type a tree declares, read from its declarations alone: no member body is looked into.</summary>
	static ImmutableHashSet<string> DeclaredTypeNames(IEnumerable<SyntaxTree> trees)
	{
		var names = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

		foreach (var tree in trees)
			foreach (var node in tree.GetRoot().DescendantNodes(static node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax))
				switch (node)
				{
					case BaseTypeDeclarationSyntax type:
						names.Add(type.Identifier.ValueText);
						break;

					case DelegateDeclarationSyntax @delegate:
						names.Add(@delegate.Identifier.ValueText);
						break;
				}

		return names.ToImmutable();
	}

	const int ExternalValueKind  = -5;
	const int ExternalMethodKind = -6;

	static object? Member(object? instance, string name)
	{
		if (instance is null)
			throw Unreadable($"no value to read {name} from");

		var property = instance.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw Unreadable($"{instance.GetType().Name} has no {name}");

		return property.GetValue(instance);
	}

	static InvalidOperationException Unreadable(string what)
	{
		return new InvalidOperationException(
			$"The generator's questions could not be read ({what}). This is a defect in DotGram.Cli, not in the grammar.");
	}

	/// <summary>The identifiers in a C# type name: <c>List&lt;Ast.Node&gt;[]</c> gives List, Ast and Node.</summary>
	public static void Identifiers(string? name, ImmutableHashSet<string>.Builder into)
	{
		if (name is null)
			return;

		var start = -1;

		for (var at = 0; at <= name.Length; at++)
		{
			var part = at < name.Length && (char.IsLetterOrDigit(name[at]) || name[at] == '_');

			if (part && start < 0)
				start = at;
			else if (!part && start >= 0)
			{
				into.Add(name.Substring(start, at - start));
				start = -1;
			}
		}
	}
}
