using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Level 2: the generator driven in memory, so what it produced can be read back as
/// text and inspected as code.
/// </summary>
public sealed class GeneratorDriverTests
{
	[Fact]
	public void Emits_the_marker_attributes_into_every_compilation()
	{
		var source = GetGeneratedSource(RunGenerator(""), "DotGram.Attributes.g.cs");

		Assert.Contains("internal sealed class GramAttribute", source, StringComparison.Ordinal);
		Assert.Contains("internal sealed class GramRuntimeAttribute", source, StringComparison.Ordinal);
	}

	[Fact]
	public void Emits_support_types_internal_by_default()
	{
		var source = GetGeneratedSource(RunGenerator(""), "DotGram.Support.g.cs");

		Assert.Contains("internal enum Outcome", source, StringComparison.Ordinal);
		Assert.Contains("internal readonly struct SourceSpan", source, StringComparison.Ordinal);
		Assert.DoesNotContain("public enum Outcome", source, StringComparison.Ordinal);
	}

	[Fact]
	public void Emits_support_types_public_when_the_assembly_publishes_them()
	{
		var source = GetGeneratedSource(
			RunGenerator("[assembly: DotGram.GramRuntime]"),
			"DotGram.Support.g.cs");

		Assert.Contains("public enum Outcome", source, StringComparison.Ordinal);
		Assert.Contains("public readonly struct SourceSpan", source, StringComparison.Ordinal);
	}

	[Fact]
	public void Generated_sources_compile()
	{
		RunGenerator("", out var output);

		var errors = output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.Empty(errors);
	}

	// ── The host class (§1) ──────────────────────────────────────────────────────

	const string Digits = "Digits = ['0'..'9']+\nparse Digits";

	/// <summary>The grammar as a C# string literal, for writing it into the attribute.</summary>
	const string DigitsLiteral = "\"Digits = ['0'..'9']+\\nparse Digits\"";

	[Fact]
	public void A_grammar_written_into_the_attribute_needs_no_file()
	{
		var source = GetGeneratedSource(
			RunGenerator(
				"namespace My.App;\n" +
				"[DotGram.Gram(" + DigitsLiteral + ")]\n" +
				"public partial class Numbers;"),
			"My.App.Numbers.g.cs");

		Assert.Contains("namespace My.App",       source, StringComparison.Ordinal);
		Assert.Contains("partial class Numbers",  source, StringComparison.Ordinal);
		Assert.Contains("ParseDigits",            source, StringComparison.Ordinal);
	}

	[Fact]
	public void With_no_argument_the_file_is_the_one_named_after_the_class()
	{
		var source = GetGeneratedSource(
			RunGenerator(
				"[DotGram.Gram] public partial class Numbers;",
				("/proj/Numbers.gram", Digits)),
			"Numbers.g.cs");

		Assert.Contains("ParseDigits", source, StringComparison.Ordinal);
	}

	[Fact]
	public void An_argument_ending_in_gram_is_a_path()
	{
		var source = GetGeneratedSource(
			RunGenerator(
				"""[DotGram.Gram("formats/feed.gram")] public partial class Feed;""",
				("/proj/formats/feed.gram", Digits),
				("/proj/other.gram",        "Other = 'x'")),
			"Feed.g.cs");

		Assert.Contains("ParseDigits", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_nested_host_is_written_back_out_nested()
	{
		var source = GetGeneratedSource(
			RunGenerator(
				"public partial class Outer\n" +
				"{\n" +
				"	[DotGram.Gram(" + DigitsLiteral + ")]\n" +
				"	public partial class Inner;\n" +
				"}"),
			"Outer.Inner.g.cs");

		Assert.Contains("partial class Outer", source, StringComparison.Ordinal);
		Assert.Contains("partial class Inner", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_generic_host_keeps_its_type_parameters()
	{
		// Without them the generated half declares a different type, and the consumer
		// gets an error about a partial class they did write correctly.
		var source = GetGeneratedSource(
			RunGenerator(
				"public partial class Outer<T>\n" +
				"{\n" +
				"	[DotGram.Gram(" + DigitsLiteral + ")]\n" +
				"	public partial class Inner<U>;\n" +
				"}"),
			"Outer_T_.Inner_U_.g.cs");

		Assert.Contains("partial class Outer<T>", source, StringComparison.Ordinal);
		Assert.Contains("partial class Inner<U>", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_generic_host_looks_for_the_file_named_after_the_class_alone()
	{
		var source = GetGeneratedSource(
			RunGenerator(
				"[DotGram.Gram] public partial class Numbers<T>;",
				("/proj/Numbers.gram", Digits)),
			"Numbers_T_.g.cs");

		Assert.Contains("ParseDigits", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_host_that_is_not_partial_is_told_so() =>
		AssertDiagnostic("GRAM0002", RunGenerator(
			"[DotGram.Gram] public class Numbers { }",
			("/proj/Numbers.gram", Digits)));

	[Fact]
	public void A_host_whose_enclosing_class_is_not_partial_is_told_so_too() =>
		AssertDiagnostic("GRAM0002", RunGenerator(
			"public class Outer { [DotGram.Gram] public partial class Inner { } }",
			("/proj/Inner.gram", Digits)));

	[Fact]
	public void A_missing_grammar_file_is_a_diagnostic_and_not_a_crash() =>
		AssertDiagnostic("GRAM0003", RunGenerator("[DotGram.Gram] public partial class Numbers;"));

	[Fact]
	public void Two_files_matching_one_path_is_a_diagnostic() =>
		AssertDiagnostic("GRAM0004", RunGenerator(
			"[DotGram.Gram] public partial class Numbers;",
			("/a/Numbers.gram", Digits),
			("/b/Numbers.gram", Digits)));

	[Fact]
	public void A_gram_file_no_class_claims_generates_nothing()
	{
		var result = RunGenerator("", ("/proj/Orphan.gram", Digits));

		Assert.DoesNotContain(
			result.Results.SelectMany(static r => r.GeneratedSources),
			static source => source.HintName.EndsWith(".g.cs", StringComparison.Ordinal) &&
				source.SourceText.ToString().Contains("ParseDigits"));
	}

	[Fact]
	public void A_grammar_error_points_into_the_grammar_file()
	{
		var result = RunGenerator(
			"[DotGram.Gram] public partial class Numbers;",
			("/proj/Numbers.gram", "Digits = ['0'..'9']+\nStart  = Missing"));

		var diagnostic = Assert.Single(result.Diagnostics.Where(d => d.Id == "GRAM3002"));
		var position   = diagnostic.Location.GetLineSpan();

		Assert.Equal("/proj/Numbers.gram", position.Path);
		Assert.Equal(1, position.StartLinePosition.Line);          // the second line
	}

	[Fact]
	public void A_generated_parser_compiles()
	{
		RunGenerator(
			"[DotGram.Gram] public partial class Numbers;",
			out var output,
			("/proj/Numbers.gram", Digits));

		Assert.Empty(output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
	}

	// ── Driving it ───────────────────────────────────────────────────────────────

	static void AssertDiagnostic(string id, GeneratorDriverRunResult result) =>
		Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Id == id);

	static GeneratorDriverRunResult RunGenerator(string source, params (string Path, string Text)[] additionalFiles) =>
		RunGenerator(source, out _, additionalFiles);

	static GeneratorDriverRunResult RunGenerator(
		string source,
		out Compilation output,
		params (string Path, string Text)[] additionalFiles)
	{
		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.GeneratorDriver",
			[CSharpSyntaxTree.ParseText(source, parseOptions, "GeneratorDriverTest.cs")],
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var driver = CSharpGeneratorDriver
			.Create(
				[new GramGenerator().AsSourceGenerator()],
				additionalTexts: additionalFiles.Select(static file => (AdditionalText)new InMemoryFile(file.Path, file.Text)),
				parseOptions: parseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out output, out _);

		return driver.GetRunResult();
	}

	/// <summary>A .gram file that never touched a disk.</summary>
	sealed class InMemoryFile(string path, string text) : AdditionalText
	{
		public override string Path { get; } = path;

		public override SourceText GetText(CancellationToken cancellationToken = default) =>
			SourceText.From(text);
	}

	static string GetGeneratedSource(GeneratorDriverRunResult result, string hintName)
	{
		var sources = result.Results
			.SelectMany(static r => r.GeneratedSources)
			.Where(s => string.Equals(s.HintName, hintName, StringComparison.Ordinal))
			.ToArray();

		Assert.True(
			sources.Length == 1,
			$"Expected exactly one '{hintName}'; got {sources.Length}. Generated: " +
			string.Join(", ", result.Results.SelectMany(static r => r.GeneratedSources).Select(static s => s.HintName)));

		return sources[0].SourceText.ToString();
	}

	static ImmutableArray<MetadataReference> GetMetadataReferences() =>
	[
		.. AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
			.Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)),
	];
}
