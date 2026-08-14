using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
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
	public void Emits_the_attribute_and_the_one_support_type_into_every_compilation()
	{
		var source = GetGeneratedSource(RunGenerator(""), "DotGram.Attributes.g.cs");

		Assert.Contains("internal sealed class GramAttribute",   source, StringComparison.Ordinal);
		Assert.Contains("internal readonly struct SourceSpan",   source, StringComparison.Ordinal);
	}

	[Fact]
	public void And_nothing_public_that_two_assemblies_would_have_to_agree_about()
	{
		// Internal is what makes .Gram need no runtime assembly and no way of finding one:
		// two assemblies each emitting `DotGram.SourceSpan` cannot see each other's, so
		// there is nothing to collide, discover or version. There used to be a public mode
		// for this, and it brought the version skew back with it.
		var source = GetGeneratedSource(RunGenerator(""), "DotGram.Attributes.g.cs");

		Assert.DoesNotContain("public sealed class",   source, StringComparison.Ordinal);
		Assert.DoesNotContain("public readonly struct", source, StringComparison.Ordinal);
		Assert.DoesNotContain("public enum",            source, StringComparison.Ordinal);
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

	[Fact]
	public void A_partial_void_with_a_ref_parameter_vanishes_when_nobody_implements_it()
	{
		// What a two-phase generator would rest on: the editor gets the API, whose body
		// hands off to a `partial void` the build phase implements. Where the second half
		// is absent — which in the editor it always is — the call goes with the
		// declaration and the value is left as it was, so it compiles and answers `default`
		// rather than failing to build.
		//
		// `ref` and not `out`: a classic partial method may not take `out`, precisely
		// because an erased call would leave it unassigned.
		var assembly = EmittedCode.Compile("""
			public partial class Grammar
			{
				public static int Handed()
				{
					var value = 7;

					Recognize(ref value);

					return value;
				}

				static partial void Recognize(ref int value);
			}
			""");

		var handed = assembly.GetType("Grammar")!.GetMethod("Handed")!;

		Assert.Equal(7, handed.Invoke(null, null));
		Assert.Null(assembly.GetType("Grammar")!.GetMethod(
			"Recognize",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
	}

	// ── Value failures (§8.1) ────────────────────────────────────────────────────

	/// <summary>
	/// A grammar whose <c>=&gt;</c> may refuse, and the C# it names.
	/// </summary>
	/// <remarks>
	/// Driven through the generator rather than the compiler because §8.1 is decided by
	/// the shape of a C# signature, and only a real compilation can be asked what that
	/// shape is. The permissive resolver the grammar half falls back to cannot tell a
	/// guard from a conversion that refuses.
	/// </remarks>
	const string Refusing = """
		[DotGram.Gram("Start : @int = digits: ['0'..'9']+ => @TryTiny(digits)\nparse Start")]
		public partial class Numbers
		{
			static bool TryTiny(string digits, out int value) =>
				int.TryParse(digits, out value) && value < 100;
		}
		""";

	[Fact]
	public void A_transformation_that_may_refuse_is_recognized_by_its_shape()
	{
		var run = RunGenerator(Refusing);

		Assert.Empty(run.Diagnostics);

		var source = GetGeneratedSource(run, "Numbers.g.cs");

		// The factory answers whether it produced a value, rather than producing one.
		Assert.Contains(
			"static bool Construct_Start(string parserText, string digits, out int value) =>",
			source,
			StringComparison.Ordinal);

		Assert.Contains("TryTiny(digits, out value);", source, StringComparison.Ordinal);

		// And "no" is a failure of the match, which is what makes it a *value* failure
		// rather than an exception: the rule simply does not match here.
		Assert.Contains(
			"if (!Construct_Start(text.Slice(pos, p - pos).ToString(), ",
			source,
			StringComparison.Ordinal);
	}

	[Fact]
	public void And_an_ordinary_transformation_still_produces_one()
	{
		var source = GetGeneratedSource(
			RunGenerator("""
				[DotGram.Gram("Start : @int = digits: ['0'..'9']+ => @Always(digits)\nparse Start")]
				public partial class Numbers
				{
					static int Always(string digits) => int.Parse(digits);
				}
				"""),
			"Numbers.g.cs");

		Assert.Contains("static int Construct_Start(", source, StringComparison.Ordinal);
		Assert.DoesNotContain("out int value) =>",       source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_grammar_whose_construction_may_refuse_compiles()
	{
		RunGenerator(Refusing, out var output);

		Assert.Empty(output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
	}

	/// <summary>
	/// An element that spans two lines and whose value may be refused, in a repetition
	/// that recovers at one line.
	/// </summary>
	/// <remarks>
	/// The shape that tells the two recoveries apart, and the reason §8.2 calls one of
	/// them cheaper. A recognition failure has to be scanned forward from where the
	/// element began, and the first <c>eol</c> that scan finds is the one *inside* the
	/// element — so the parse picks up in the middle of it and every pair after that is
	/// read off by one. A value failure needs no scan: the element matched, so the parse
	/// is already past it and where the next one begins is known.
	/// </remarks>
	const string Pairs = """
		[DotGram.Gram("Pair : @string = a: ['0'..'9']+ & eol & b: ['0'..'9']+ & eol => @TryJoin(a, b)\nFeed = pairs: Pair* recover eol => @(parserText) & eof\nparse Feed")]
		public partial class PairFeed
		{
			static bool TryJoin(string a, string b, out string value)
			{
				value = a + "+" + b;

				return int.Parse(a) + int.Parse(b) < 10;
			}
		}
		""";

	[Fact]
	public void A_refused_value_resumes_past_the_element_rather_than_scanning_it_again()
	{
		var feed = Build(Pairs)
			.GetType("PairFeed")!
			.GetMethod("ParseFeed")!
			.Invoke(null, ["1\n2\n5\n6\n3\n4\n"])!;

		var parsed = (string[])feed.GetType().GetProperty("Pairs")!.GetValue(feed)!;

		// The middle pair is refused whole — both its lines are what was rejected — and
		// the pair after it is read as a pair rather than as the tail of the one before.
		Assert.Equal(["1+2", "5\n6\n", "3+4"], parsed);
	}

	[Fact]
	public void An_exception_out_of_a_factory_leaves_the_parse()
	{
		// §8.2 decided this rather than left it: catching would mean catching `Exception`,
		// there being no type that tells "this record's quantity is not a number" from
		// `NullReferenceException`, and a parser that reports a bug in the author's own C#
		// as "row 400 was malformed" is worse than one that stops. A conversion that can
		// fail says so by its shape instead (§8.1) — that is the difference between a
		// rejection and a defect, and this is the test that the defect still gets out.
		var parse = Build("""
			[DotGram.Gram("Row = name: ['a'..'z']+ & eol\nFeed = rows: Row* recover eol => @Boom(parserText) & eof\nparse Feed")]
			public partial class BoomingFeed
			{
				static Row Boom(string parserText) =>
					throw new System.InvalidOperationException("a defect, not a rejection");
			}
			""")
			.GetType("BoomingFeed")!
			.GetMethod("ParseFeed")!;

		// The middle line begins a Row and breaks inside one, which is what recovery is
		// for — a line that never began one would simply end the repetition (§8.2).
		var thrown = Assert.Throws<TargetInvocationException>(
			() => parse.Invoke(null, ["aa\nb1b\ncc\n"]));

		Assert.IsType<InvalidOperationException>(thrown.InnerException);
		Assert.Equal("a defect, not a rejection", thrown.InnerException!.Message);
	}

	[Fact]
	public void A_grammar_that_cannot_refuse_a_value_carries_nothing_that_tells_one()
	{
		// The field would always be -1, the branch reading it never taken and the message's
		// condition always false. Emitted code is read in someone else's build, where the
		// shortest version of what it does is the one that belongs.
		var refusing = GetGeneratedSource(RunGenerator(Pairs), "PairFeed.g.cs");

		var recognizing = GetGeneratedSource(
			RunGenerator("""
				[DotGram.Gram("Row = name: ['a'..'z']+ & eol\nFeed = rows: Row* recover eol => @(parserText) & eof\nparse Feed")]
				public partial class PlainFeed { }
				"""),
			"PlainFeed.g.cs");

		Assert.Contains("public int Refused;",    refusing,    StringComparison.Ordinal);
		Assert.DoesNotContain("Refused",          recognizing, StringComparison.Ordinal);

		// And the one that keeps the difference honest: both still recover.
		Assert.Contains("failure.Reach", refusing,    StringComparison.Ordinal);
		Assert.Contains("failure.Reach", recognizing, StringComparison.Ordinal);
	}

	[Fact]
	public void And_says_so_rather_than_calling_a_record_that_matched_malformed()
	{
		// §8.1 makes the two failures different things, so the message has to be a
		// different message. "Does not match" of a record that matched perfectly well
		// sends a reader looking at the shape, which is the half that was fine.
		var reported = Pairs.Replace("@(parserText)", "@(parserMessage)", StringComparison.Ordinal);

		var feed = Build(reported)
			.GetType("PairFeed")!
			.GetMethod("ParseFeed")!
			.Invoke(null, ["5\n6\n"])!;

		Assert.Equal(
			["'Pair' at 0 was recognized and its value was not accepted."],
			(string[])feed.GetType().GetProperty("Pairs")!.GetValue(feed)!);
	}

	// ── What re-runs, and when ───────────────────────────────────────────────────

	/// <summary>
	/// The grammar a host claims, small enough that what is measured is the pipeline
	/// rather than the grammar.
	/// </summary>
	const string Claimed = """
		[DotGram.Gram("Start = 'a'+\nparse Start")]
		public static partial class Claimed { }
		""";

	[Fact]
	public void Editing_something_else_does_not_regenerate_the_parser()
	{
		// The claim an incremental generator exists to make. It is checked rather than
		// argued because the failure is invisible: a generator that regenerates everything
		// on every keystroke produces exactly the right code, just at the wrong time and
		// as often as the author can type.
		var run = AfterEditingAnUnrelatedFile(Claimed);

		// The middle stage re-runs — it holds the compilation and cannot not — and answers
		// the same, which is what stops everything below it.
		Cached(run, GramGenerator.AskedStage);
		Cached(run, GramGenerator.CompiledStage);

		Assert.All(
			run.Results.SelectMany(result => result.TrackedOutputSteps).SelectMany(step => step.Value),
			step => Assert.All(
				step.Outputs,
				output => Assert.True(
					output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
					$"Editing an unrelated file re-ran the output step ({output.Reason}).")));
	}

	[Fact]
	public void But_editing_the_grammar_does()
	{
		// The other half, and the one that keeps the first honest: "nothing re-ran" is also
		// what a generator that does nothing at all would report.
		var run = AfterEditingAnUnrelatedFile(Claimed, alsoEditTheGrammar: true);

		Assert.Contains(
			Runs(run, GramGenerator.CompiledStage),
			output => output.Reason is IncrementalStepRunReason.Modified or IncrementalStepRunReason.New);
	}

	[Theory]
	[InlineData("Start : @int = digits: ['0'..'9']+ => @int.Parse(digits)\nparse Start")]
	[InlineData("@using System.Globalization;\nStart : @decimal = d: ['0'..'9']+ => @(decimal.Parse(d, CultureInfo.InvariantCulture))\nparse Start")]
	[InlineData("Start = d: ['0'..'9'] & where @(d == \"1\")\nparse Start")]
	[InlineData("Start : @System.Text.StringBuilder = t: ['a'..'z']+ => @(new System.Text.StringBuilder(t))\nparse Start")]
	public void The_question_collector_foresees_what_binding_asks(string grammar)
	{
		// The one thing this design can get wrong. Questions are collected from the
		// grammar's syntax and answered before binding runs, so a question binding asks
		// that nobody foresaw is answered "no" without the host ever being consulted — a
		// wrong answer, not a slow one. The generator records those; here there must be
		// none, over grammars that reach C# every way the notation allows.
		var result = RunGenerator($"[DotGram.Gram(@\"{grammar.Replace("\"", "\"\"")}\")] public partial class Probe;");

		Assert.DoesNotContain(
			result.Diagnostics,
			diagnostic => diagnostic.Id == "CS8785" || diagnostic.Severity == DiagnosticSeverity.Error);
	}

	static void Cached(GeneratorDriverRunResult run, string stage)
	{
		var runs = Runs(run, stage);

		// Before the assertion, because `Assert.All` over nothing passes — and a stage
		// renamed or removed would report nothing rather than fail.
		Assert.True(runs.Length > 0, $"No stage named '{stage}' ran at all.");

		Assert.All(
			runs,
			output => Assert.True(
				output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
				$"Editing an unrelated file re-ran '{stage}' ({output.Reason}). Something it " +
				"depends on compares unequal across compilations."));
	}

	static ImmutableArray<(object Value, IncrementalStepRunReason Reason)> Runs(
		GeneratorDriverRunResult run, string stage) =>
	[
		.. run.Results
			.SelectMany(result => result.TrackedSteps.TryGetValue(stage, out var steps)
				? steps
				: [])
			.SelectMany(step => step.Outputs)
	];

	/// <summary>
	/// Runs the generator twice over the same host, changing only a file that has nothing
	/// to do with any grammar, and reports how the output step of the second run was
	/// reached.
	/// </summary>
	/// <param name="alsoEditTheGrammar">
	/// Change the host's own grammar too, which must have the opposite effect.
	/// </param>
	static GeneratorDriverRunResult AfterEditingAnUnrelatedFile(
		string source,
		bool   alsoEditTheGrammar = false)
	{
		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

		var host      = CSharpSyntaxTree.ParseText(source, parseOptions, "Host.cs");
		var unrelated = CSharpSyntaxTree.ParseText("class Unrelated { int One; }", parseOptions, "Other.cs");

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Incremental",
			[host, unrelated],
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var driver = (GeneratorDriver)CSharpGeneratorDriver.Create(
			[new GramGenerator().AsSourceGenerator()],
			parseOptions: parseOptions,
			driverOptions: new GeneratorDriverOptions(
				IncrementalGeneratorOutputKind.None,
				trackIncrementalGeneratorSteps: true));

		driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

		// One edit, in a file no grammar and no host has anything to do with.
		var edited = compilation.ReplaceSyntaxTree(
			unrelated,
			CSharpSyntaxTree.ParseText("class Unrelated { int One; int Two; }", parseOptions, "Other.cs"));

		if (alsoEditTheGrammar)
			edited = edited.ReplaceSyntaxTree(
				host,
				CSharpSyntaxTree.ParseText(
					source.Replace("Start = 'a'+", "Start = 'b'+", StringComparison.Ordinal),
					parseOptions,
					"Host.cs"));

		return driver.RunGenerators(edited, TestContext.Current.CancellationToken).GetRunResult();
	}

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

	/// <summary>
	/// Runs the generator and loads what it and the host wrote together.
	/// </summary>
	/// <remarks>
	/// For the claims that are about a parser running rather than about the text of one.
	/// Through the driver and not <c>EmittedCode</c> because these grammars name C# in the
	/// host class, and what that C# is — a guard, a transformation, one that may refuse —
	/// is a question only a real compilation answers (§8.1).
	/// </remarks>
	static Assembly Build(string source)
	{
		var run = RunGenerator(source, out var output);

		Assert.Empty(run.Diagnostics);

		using var stream = new MemoryStream();

		var emitted = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(
			emitted.Success,
			string.Join("\n", emitted.Diagnostics.Where(
				static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));

		return Assembly.Load(stream.ToArray());
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
