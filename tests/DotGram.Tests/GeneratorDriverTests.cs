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
	public void And_a_grammar_in_the_attribute_points_inside_its_string()
	{
		// It used to point at the whole attribute — the right place to look, and not the
		// right character. The offset is into the grammar, and what the author sees is the
		// spelling of it, so the two have to be lined up.
		var source =
			"[DotGram.Gram(\"\"\"\n" +
			"	Digits = ['0'..'9']+\n" +
			"	Start  = Missing\n" +
			"	\"\"\")]\n" +
			"public partial class Numbers;";

		var diagnostic = Assert.Single(RunGenerator(source).Diagnostics.Where(d => d.Id == "GRAM3002"));
		var at         = diagnostic.Location.GetLineSpan();

		// The third line of the file, where `Missing` is written — not the first, where the
		// attribute begins.
		Assert.Equal(2, at.StartLinePosition.Line);

		Assert.Equal(
			"Missing",
			source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
	}

	[Fact]
	public void But_falls_back_to_the_attribute_when_it_cannot_be_placed()
	{
		// The line is written one way and decodes to another, so looking for it in the
		// spelling finds nothing. Silence beats a squiggle in the wrong place.
		var run = RunGenerator("[DotGram.Gram(\"Start\\u0020= Missing\")] public partial class Numbers;");

		var diagnostic = Assert.Single(run.Diagnostics.Where(d => d.Id == "GRAM3002"));

		Assert.Contains("DotGram.Gram", diagnostic.Location.SourceTree!.ToString()
			.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length),
			StringComparison.Ordinal);
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

	// ── A C# name as an operand (§7.1) ───────────────────────────────────────────

	[Fact]
	public void A_C_sharp_predicate_tests_one_input_item()
	{
		// The brackets establish the contract: both C# names test exactly one item, just as
		// the range beside IsVowel does.
		var parse = Build("""
			[DotGram.Gram("Start = ([@IsVowel] | ['0'..'9'])+ & [@IsStop]\nparse Start")]
			public partial class Predicates
			{
				static bool IsVowel(char c) => "aeiou".IndexOf(c) >= 0;
				static bool IsStop(char c)  => c == '.';
			}
			""")
			.GetType("Predicates")!
			.GetMethod("ParseStart", [typeof(string)])!;

		Assert.Equal("ae1i.", parse.Invoke(null, ["ae1i."]));
		Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, ["aexi."]));
	}

	[Fact]
	public void A_C_sharp_method_may_read_the_input_itself()
	{
		// §7.1's second row. `Blob` reads a length-prefixed run the grammar has no way of
		// spelling — the count decides how much to take — and moves the parser's own
		// position to say how much it took.
		var parse = Build("""
			[DotGram.Gram("Start = 'b' & @Blob & '.'\nparse Start")]
			public partial class Blobs
			{
				static bool Blob(System.ReadOnlySpan<char> input, ref int pos)
				{
					var at = pos;

					var size = 0;

					while (at < input.Length && input[at] >= '0' && input[at] <= '9')
						size = size * 10 + (input[at++] - '0');

					if (at == pos || at + size > input.Length)
						return false;

					pos = at + size;

					return true;
				}
			}
			""")
			.GetType("Blobs")!
			.GetMethod("ParseStart", [typeof(string)])!;

		// `3abc` is three characters of payload after the count, and the `.` follows it.
		Assert.Equal("b3abc.", parse.Invoke(null, ["b3abc."]));

		// Saying no is an ordinary non-match, so the rule simply does not match.
		Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, ["b3ab."]));
	}

	[Fact]
	public void Syntactic_position_selects_between_C_sharp_overloads()
	{
		var parse = Build("""
			[DotGram.Gram("Start = [@Foo] & @Foo & eof\nparse Start")]
			public partial class Overloaded
			{
				static bool Foo(char c) => c == 'a';

				static bool Foo(System.ReadOnlySpan<char> input, ref int pos)
				{
					if (pos + 2 > input.Length || input[pos] != 'b' || input[pos + 1] != 'c')
						return false;

					pos += 2;
					return true;
				}
			}
			""")
			.GetType("Overloaded")!
			.GetMethod("ParseStart", [typeof(string)])!;

		Assert.Equal("abc", parse.Invoke(null, ["abc"]));
		Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, ["abb"]));
	}

	[Fact]
	public void And_a_grammar_that_reads_its_own_input_is_not_streamed()
	{
		// The recognizer is handed a span and told nothing about where it came from, so it
		// cannot tell the end of a window from the end of the input — and, unlike a
		// literal, it has no way to say which it hit. Over a window it would read a record
		// cut in half as a record that ended.
		var source = GetGeneratedSource(
			RunGenerator("""
				[DotGram.Gram("Feed : @object[] = Row* recover eol\nRow : @object = @Blob & eol => @(new object())\nparse Feed")]
				public partial class Reading
				{
					static bool Blob(System.ReadOnlySpan<char> input, ref int pos) => false;
				}
				"""),
			"Reading.g.cs");

		Assert.DoesNotContain("TextReader", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_bare_C_sharp_operand_uses_the_external_recognizer_contract()
	{
		var run = RunGenerator("""
			[DotGram.Gram("Start = @Convert & 'x'\nparse Start")]
			public partial class Converting
			{
				static int Convert(string text) => text.Length;
			}
			""", out var output);

		var source = GetGeneratedSource(run, "Converting.g.cs");
		var errors = output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.Contains("Convert(text, ref p)", source, StringComparison.Ordinal);
		Assert.NotEmpty(errors);
		Assert.All(errors, error => Assert.StartsWith("CS", error.Id, StringComparison.Ordinal));
	}

	[Fact]
	public void A_transformation_is_emitted_as_written()
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
		Assert.Contains("Always(digits);", source, StringComparison.Ordinal);
	}

	// ── A sequence result (§4.1 case 2) ──────────────────────────────────────────

	/// <summary>
	/// A rule whose result is a sequence of a type its operands share.
	/// </summary>
	/// <remarks>
	/// The envelope and the records in one result, in the order they were read, which is
	/// what §4.1 case 2 is for — and what makes a streamed <c>parse</c> possible later,
	/// since a sequence is the only shape that can be handed over one element at a time.
	/// </remarks>
	const string Items = """
		[DotGram.Gram("Feed : @Item[] = Header & Row* & Trailer & eof\nHeader : @Item = 'H' & eol => @(new Head())\nRow : @Item = name: ['a'..'z']+ & eol => @(new Line(name))\nTrailer : @Item = 'T' & eol => @(new Tail())\nparse Feed")]
		public partial class Items { }

		public abstract class Item { }
		public sealed class Head : Item { }
		public sealed class Tail : Item { }
		public sealed class Line : Item
		{
			public Line(string name) { Name = name; }
			public string Name { get; }
		}
		""";

	[Fact]
	public void A_rule_of_a_sequence_type_collects_the_operands_that_fit()
	{
		var items = (Array)Build(Items)
			.GetType("Items")!
			.GetMethod("ParseFeed", [typeof(string)])!
			.Invoke(null, ["H\naa\nbb\nT\n"])!;

		// Header, two rows and the trailer, in the order the grammar reads them. Nothing
		// says `=>` anywhere in `Feed` — what it is made of is the shape of the rule.
		Assert.Equal(
			["Head", "Line:aa", "Line:bb", "Tail"],
			items.Cast<object>().Select(static item => item.GetType().Name + Named(item)));
	}

	static string Named(object item) =>
		item.GetType().GetProperty("Name") is { } name ? ":" + name.GetValue(item) : "";

	[Fact]
	public void An_operand_that_does_not_fit_the_element_type_is_left_out()
	{
		// `Sep` builds no value at all, so there is nothing of it to collect — and that is
		// not an error, it is what "every operand whose value is assignable" means.
		var items = (Array)Build(Items.Replace(
				"Row : @Item = name: ['a'..'z']+ & eol",
				"Sep = '-' & eol\\nRow : @Item = name: ['a'..'z']+ & eol",
				StringComparison.Ordinal)
			.Replace("= Header & Row*", "= Header & Sep & Row*", StringComparison.Ordinal))
			.GetType("Items")!
			.GetMethod("ParseFeed", [typeof(string)])!
			.Invoke(null, ["H\n-\naa\nT\n"])!;

		Assert.Equal(["Head", "Line:aa", "Tail"], items.Cast<object>().Select(static item => item.GetType().Name + Named(item)));
	}

	[Fact]
	public void A_sequence_of_nothing_that_fits_is_refused()
	{
		// The grammar says its result is a sequence and then nothing joins it. Left alone
		// it would generate a method returning an always-empty array, which is a rule that
		// compiles and means nothing.
		var run = RunGenerator("""
			[DotGram.Gram("Feed : @Item[] = 'H' & eol & eof\nparse Feed")]
			public partial class Empty { }

			public abstract class Item { }
			""");

		Assert.Contains(run.Diagnostics, diagnostic => diagnostic.Id == "GRAM4008");
	}

	// ── parse over a reader (§6.3) ───────────────────────────────────────────────

	/// <summary>A feed whose records are read one at a time.</summary>
	/// <remarks>
	/// `recover` is what makes it streamable and not decoration: handing an element to the
	/// caller cannot be undone, so the parse may only read what the grammar says it will
	/// not go back past (§8.2).
	/// </remarks>
	const string Stream = """
		[DotGram.Gram("Feed : @Item[] = Header & Row* recover eol & Trailer & eof\nHeader : @Item = 'H' & eol => @(new Head())\nRow : @Item = name: ['a'..'z']+ & eol => @(new Line(name))\nTrailer : @Item = 'T' & eol => @(new Tail())\nparse Feed")]
		public partial class Streamed { }
		""" + Shapes;

	const string Shapes = """

		public abstract class Item { }
		public sealed class Head : Item { }
		public sealed class Tail : Item { }
		public sealed class Line : Item
		{
			public Line(string name) { Name = name; }
			public string Name { get; }
		}
		""";

	static string[] Read(Assembly assembly, string type, string method, object input) =>
		[.. ((System.Collections.IEnumerable)assembly
				.GetType(type)!
				.GetMethod(method, [input.GetType() == typeof(string) ? typeof(string) : typeof(TextReader)])!
				.Invoke(null, [input])!)
			.Cast<object>()
			.Select(static item => item.GetType().Name + Named(item))];

	[Fact]
	public void A_parse_over_a_reader_reads_the_same_things()
	{
		var assembly = Build(Stream);
		var text     = "H\naa\nbb\nT\n";

		// The same elements in the same order, out of input that was never all there.
		Assert.Equal(
			["Head", "Line:aa", "Line:bb", "Tail"],
			Read(assembly, "Streamed", "ParseFeed", new StringReader(text)));
	}

	[Fact]
	public void And_reads_more_records_than_the_window_holds()
	{
		// The claim streaming exists to make. Four thousand records is well past the 4096
		// characters the window starts at, so the buffer is reused many times over.
		var records = string.Concat(Enumerable.Repeat("aa\n", 4000));

		Assert.Equal(
			4002,
			Read(Build(Stream), "Streamed", "ParseFeed", new StringReader("H\n" + records + "T\n")).Length);
	}

	[Fact]
	public void A_broken_record_is_stepped_over_in_a_stream_too()
	{
		// The two overloads have to agree, and this is where they most easily would not:
		// over a string the repetition backtracks out of a bad element and the machine
		// steps over it, while in a stream the driver has to do the stepping itself.
		var assembly = Build(Stream);
		var text     = "H\naa\nb1b\ncc\nT\n";

		var whole = ((Array)assembly
			.GetType("Streamed")!
			.GetMethod("ParseFeed", [typeof(string)])!
			.Invoke(null, [text])!)
			.Cast<object>()
			.Select(static item => item.GetType().Name + Named(item));

		// Four and not five: there is no `=>` on the recovery, so the rejection is dropped
		// rather than taking its place in the sequence (§8.3).
		Assert.Equal(["Head", "Line:aa", "Line:cc", "Tail"], whole);

		Assert.Equal(whole, Read(assembly, "Streamed", "ParseFeed", new StringReader(text)));
	}

	[Fact]
	public void A_rejection_can_take_its_place_in_the_stream()
	{
		// §8.2's other form: with a `=>` the rejection is an element of the sequence, and
		// in a stream it arrives between the records it sat between.
		var assembly = Build(Stream.Replace(
			"Row* recover eol",
			"Row* recover eol => @(new Bad(parserLine, parserText))",
			StringComparison.Ordinal)
			.Replace(
				"public sealed class Tail : Item { }",
				"public sealed class Tail : Item { }\n" +
				"public sealed class Bad : Item\n" +
				"{\n" +
				"	public Bad(int line, string text) { Line = line; Text = text; }\n" +
				"	public int Line { get; }\n" +
				"	public string Text { get; }\n" +
				"}",
				StringComparison.Ordinal));

		var read = (System.Collections.IEnumerable)assembly
			.GetType("Streamed")!
			.GetMethod("ParseFeed", [typeof(TextReader)])!
			.Invoke(null, [new StringReader("H\naa\nb1b\ncc\nT\n")])!;

		var bad = read.Cast<object>().Single(static item => item.GetType().Name == "Bad");

		// The line is counted from the start of the file, not from the start of the window
		// — the third line, which is where a person would open it.
		Assert.Equal(3, bad.GetType().GetProperty("Line")!.GetValue(bad));

		// Without the terminator: `eol` separates the elements and is not part of one, so
		// the rejected extent stops where the synchronization point begins (§8.2).
		Assert.Equal("b1b", bad.GetType().GetProperty("Text")!.GetValue(bad));
	}

	[Fact]
	public void And_is_placed_by_a_line_number_that_survives_the_window_moving()
	{
		// The defect this is here for: a line number counted inside the buffer restarts
		// every time the buffer moves, so a bad record deep in a large feed would be
		// reported near the top of the file. Two thousand records is well past the 4096
		// characters the window starts at.
		var assembly = Build(Stream.Replace(
			"Row* recover eol",
			"Row* recover eol => @(new Bad(parserLine, parserText))",
			StringComparison.Ordinal)
			.Replace(
				"public sealed class Tail : Item { }",
				"public sealed class Tail : Item { }\n" +
				"public sealed class Bad : Item\n" +
				"{\n" +
				"	public Bad(int line, string text) { Line = line; Text = text; }\n" +
				"	public int Line { get; }\n" +
				"	public string Text { get; }\n" +
				"}",
				StringComparison.Ordinal));

		var text = "H\n" + string.Concat(Enumerable.Repeat("aa\n", 2000)) + "b1b\ncc\nT\n";

		var read = (System.Collections.IEnumerable)assembly
			.GetType("Streamed")!
			.GetMethod("ParseFeed", [typeof(TextReader)])!
			.Invoke(null, [new StringReader(text)])!;

		var bad = read.Cast<object>().Single(static item => item.GetType().Name == "Bad");

		// One header plus two thousand records, so the bad line is 2002.
		Assert.Equal(2002, bad.GetType().GetProperty("Line")!.GetValue(bad));
	}

	[Fact]
	public void A_repetition_that_cannot_tell_its_own_end_is_reported()
	{
		// The grammar that started this: `Trailer` reads as a `Row` too, so where the
		// repetition ends is decided by backtracking rather than by the grammar. It parses,
		// and the author never learns their rule had two readings.
		var run = RunGenerator(Stream.Replace(
			"Trailer : @Item = 'T' & eol",
			"Trailer : @Item = 't' & eol",
			StringComparison.Ordinal));

		var told = Assert.Single(run.Diagnostics.Where(d => d.Id == "GRAM5002"));

		Assert.Contains("backtracking", told.GetMessage(), StringComparison.Ordinal);
		Assert.Equal(DiagnosticSeverity.Info, told.Severity);
	}

	[Fact]
	public void And_one_that_can_is_not()
	{
		// `T` is not a lowercase letter, so the repetition stops at the trailer because the
		// grammar says so and not because the engine tried both ways.
		Assert.Empty(RunGenerator(Stream).Diagnostics.Where(d => d.Id == "GRAM5002"));
	}

	[Fact]
	public void And_a_grammar_that_leans_on_backtracking_is_left_alone()
	{
		// `'a'+ & 'a'` is the same overlap and is perfectly good .Gram: §11 makes
		// backtracking total and a rule is entitled to lean on it. What makes the case
		// above different is that the rule asks to be read in parts, and a part handed over
		// cannot be taken back.
		Assert.Empty(
			RunGenerator("""
				[DotGram.Gram("Start = 'a'+ & 'a'\nparse Start")]
				public partial class Leaning { }
				""").Diagnostics);
	}

	[Fact]
	public void An_unambiguous_grammar_needs_no_mark_to_be_streamed()
	{
		// What the mark is for is surviving a bad record, not permitting a stream. Where
		// the grammar itself says where the repetition ends, nothing has to commit it: no
		// element handed over would ever have been wanted back.
		var assembly = Build(Stream.Replace(" recover eol", "", StringComparison.Ordinal));

		Assert.Equal(
			["Head", "Line:aa", "Line:bb", "Tail"],
			Read(assembly, "Streamed", "ParseFeed", new StringReader("H\naa\nbb\nT\n")));
	}

	[Fact]
	public void But_one_whose_repetition_has_no_end_of_its_own_does()
	{
		// `t` reads as a record too, so where the repetition stops is settled by
		// backtracking — and a stream has nothing to backtrack with.
		var ambiguous = Stream
			.Replace(" recover eol", "", StringComparison.Ordinal)
			.Replace("Trailer : @Item = 'T' & eol", "Trailer : @Item = 't' & eol", StringComparison.Ordinal);

		var told = Assert.Single(RunGenerator(ambiguous).Diagnostics.Where(d => d.Id == "GRAM5001"));

		Assert.Contains("settled by backtracking", told.GetMessage(), StringComparison.Ordinal);
		Assert.Contains("recover",                 told.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void And_the_mark_settles_it_where_the_grammar_does_not()
	{
		// The same ambiguous grammar with the repetition marked. It is possessive, so where
		// it ends is decided rather than searched for, and the overload comes back — which
		// is not to say the grammar is a good one, and GRAM5002 still says so.
		var marked = Stream.Replace(
			"Trailer : @Item = 'T' & eol",
			"Trailer : @Item = 't' & eol",
			StringComparison.Ordinal);

		var run = RunGenerator(marked);

		Assert.Empty(run.Diagnostics.Where(d => d.Id == "GRAM5001"));
		Assert.NotEmpty(run.Diagnostics.Where(d => d.Id == "GRAM5002"));
	}

	[Fact]
	public void And_a_grammar_that_never_asked_for_one_is_told_nothing()
	{
		// A result that comes in parts is what says the author had a stream in mind, and
		// this one does not declare one. Most grammars are not feeds; telling every one of
		// them what it did not get would be noise on every build.
		var run = RunGenerator("""
			[DotGram.Gram("Feed = rows: Row* recover eol\nRow = name: ['a'..'z']+ & eol\nparse Feed")]
			public partial class NotASequence { }
			""");

		Assert.Empty(run.Diagnostics.Where(d => d.Id == "GRAM5001"));
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
			.GetMethod("ParseFeed", [typeof(string)])!;

		// The middle line begins a Row and breaks inside one, which is what recovery is
		// for — a line that never began one would simply end the repetition (§8.2).
		var thrown = Assert.Throws<TargetInvocationException>(
			() => parse.Invoke(null, ["aa\nb1b\ncc\n"]));

		Assert.IsType<InvalidOperationException>(thrown.InnerException);
		Assert.Equal("a defect, not a rejection", thrown.InnerException!.Message);
	}

	[Fact]
	public void Generated_code_carries_no_value_refusal_state()
	{
		var source = GetGeneratedSource(
			RunGenerator("""
				[DotGram.Gram("Row = name: ['a'..'z']+ & eol\nFeed = rows: Row* recover eol => @(parserText) & eof\nparse Feed")]
				public partial class PlainFeed { }
				"""),
			"PlainFeed.g.cs");

		Assert.DoesNotContain("Refused", source, StringComparison.Ordinal);
		Assert.Contains("failure.Reach", source, StringComparison.Ordinal);
	}

	// ── Captures matched to a constructor (§7.3) ─────────────────────────────────

	[Fact]
	public void A_declared_type_can_be_built_from_its_constructor()
	{
		// §7.3's first way of filling a result in, and the one that needs the host: which
		// constructors a type has is not something a grammar can see. No `=>` anywhere —
		// the captures are the arguments, matched by name.
		var built = Build("""
			public sealed class Row(string name, string amount)
			{
				public string Name   { get; } = name;
				public string Amount { get; } = amount;
			}

			[DotGram.Gram("Row : @Row = name: ['a'..'z']+ & ',' & amount: ['0'..'9']+\nparse Row")]
			public partial class Rows;
			""")
			.GetType("Rows")!
			.GetMethod("ParseRow", [typeof(string)])!
			.Invoke(null, ["ab,12"])!;

		Assert.Equal("ab", built.GetType().GetProperty("Name")!.GetValue(built));
		Assert.Equal("12", built.GetType().GetProperty("Amount")!.GetValue(built));
	}

	[Fact]
	public void The_match_is_by_name_and_ignores_case()
	{
		// The mechanical transform §7.3 describes: the capture `symbol` fits the parameter
		// `symbol` and the property `Symbol`. A constructor written the way records write
		// theirs takes captures written the way grammars write theirs.
		var built = Build("""
			public sealed class Pair(string left, string right)
			{
				public string Left  { get; } = left;
				public string Right { get; } = right;
			}

			[DotGram.Gram("Pair : @Pair = Left: ['a'..'z']+ & '-' & Right: ['a'..'z']+\nparse Pair")]
			public partial class Pairs;
			""")
			.GetType("Pairs")!
			.GetMethod("ParsePair", [typeof(string)])!
			.Invoke(null, ["ab-cd"])!;

		Assert.Equal("ab", built.GetType().GetProperty("Left")!.GetValue(built));
	}

	[Fact]
	public void A_rule_that_says_how_to_build_its_value_still_says_it()
	{
		// The constructor match is what happens when the grammar left it unsaid. Writing
		// `=> @(new Row(...))` by hand goes on meaning exactly that — here with the
		// arguments swapped, which a match by name would never produce, so the assertion
		// can tell the two apart.
		var built = Build("""
			public sealed class Swapped(string name, string amount)
			{
				public string Name   { get; } = name;
				public string Amount { get; } = amount;
			}

			[DotGram.Gram("Row : @Swapped = name: ['a'..'z']+ & ',' & amount: ['0'..'9']+ => @(new Swapped(amount, name))\nparse Row")]
			public partial class Handwritten;
			""")
			.GetType("Handwritten")!
			.GetMethod("ParseRow", [typeof(string)])!
			.Invoke(null, ["ab,12"])!;

		Assert.Equal("12", built.GetType().GetProperty("Name")!.GetValue(built));
		Assert.Equal("ab", built.GetType().GetProperty("Amount")!.GetValue(built));
	}

	[Fact]
	public void A_type_beside_the_grammar_is_found_by_its_short_name()
	{
		// `@Method` beside a grammar has always meant the host's own method. `@Row` beside
		// it meant a top-level `Row`, and a type nested in the host — which is where a type
		// written for one grammar belongs — could not be named at all without writing out
		// the chain the author never writes anywhere else.
		var built = Build("""
			[DotGram.Gram("Row : @Line = name: ['a'..'z']+ & ',' & amount: ['0'..'9']+\nparse Row")]
			public partial class Nested
			{
				public sealed class Line(string name, string amount)
				{
					public string Name   { get; } = name;
					public string Amount { get; } = amount;
				}
			}
			""")
			.GetType("Nested")!
			.GetMethod("ParseRow", [typeof(string)])!
			.Invoke(null, ["ab,12"])!;

		Assert.Equal("ab", built.GetType().GetProperty("Name")!.GetValue(built));
		Assert.Equal("12", built.GetType().GetProperty("Amount")!.GetValue(built));
	}

	[Fact]
	public void And_the_nearer_one_wins_the_way_it_does_in_C_sharp()
	{
		// Both exist. C# inside the host class binds the short name to the nested type, and
		// the generated code *is* inside the host class — so resolving to the outer one
		// would check the constructors of a type other than the one that gets called, and
		// the mismatch would surface as a C# error in a file the author did not write.
		var built = Build("""
			public sealed class Line(string name, string amount)
			{
				public string Name   { get; } = name;
				public string Amount { get; } = amount;
				public string Which  { get; } = "outer";
			}

			[DotGram.Gram("Row : @Line = name: ['a'..'z']+ & ',' & amount: ['0'..'9']+\nparse Row")]
			public partial class Shadowing
			{
				public sealed class Line(string name, string amount)
				{
					public string Name   { get; } = name;
					public string Amount { get; } = amount;
					public string Which  { get; } = "nested";
				}
			}
			""")
			.GetType("Shadowing")!
			.GetMethod("ParseRow", [typeof(string)])!
			.Invoke(null, ["ab,12"])!;

		Assert.Equal("nested", built.GetType().GetProperty("Which")!.GetValue(built));
	}

	[Fact]
	public void Half_an_answer_is_refused_and_says_which_half()
	{
		// One alternative builds and the other does not. §7.3's constructor is matched
		// against the rule rather than against an alternative — the captures that fill it
		// are the rule's — so a rule that has begun answering has to finish. Refused rather
		// than completed for it: a `=>` on one alternative and not the next is as likely to
		// be an omission as an intention, and the silent version of that guess builds the
		// wrong value.
		var diagnostic = Assert.Single(
			RunGenerator(
				"public sealed class Two(string a, string b)\n" +
				"{\n" +
				"	public string A { get; } = a;\n" +
				"	public string B { get; } = b;\n" +
				"}\n" +
				"[DotGram.Gram(\"R : @Two = a: ['a'..'z']+ & b: ['0'..'9']+ => @(new Two(a, b))" +
				"\\n | a: ['A'..'Z']+ & b: ['0'..'9']+\\nparse R\")]\n" +
				"public partial class Mixed;")
				.Diagnostics
				.Where(d => d.Id == "GRAM4008"));

		Assert.Contains("on 1 of its 2 alternatives", diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void A_type_with_no_constructor_to_fill_is_written_into_instead()
	{
		// §7.3's second way, reached when the first cannot answer: the value is made and
		// its properties written from the captures. `required` is what makes this a
		// definite answer rather than a guess — the type says which of them it insists on.
		var built = Build("""
			public sealed class Entry
			{
				public required string Name   { get; init; }
				public required string Amount { get; init; }
				public          string Note   { get; init; } = "";
			}

			[DotGram.Gram("Row : @Entry = name: ['a'..'z']+ & ',' & amount: ['0'..'9']+\nparse Row")]
			public partial class Entries;
			""")
			.GetType("Entries")!
			.GetMethod("ParseRow", [typeof(string)])!
			.Invoke(null, ["ab,12"])!;

		Assert.Equal("ab", built.GetType().GetProperty("Name")!.GetValue(built));
		Assert.Equal("12", built.GetType().GetProperty("Amount")!.GetValue(built));

		// Not covered, not required, and so not written: it keeps the default the type gave it.
		Assert.Equal("", built.GetType().GetProperty("Note")!.GetValue(built));
	}

	[Fact]
	public void A_required_property_the_captures_cannot_fill_is_not_a_way_to_build_it()
	{
		// The type insists on `Amount` and the rule captures no such thing, so writing into
		// it would not compile. Reported as unbuilt rather than emitted and left to fail in
		// the consumer's build.
		var diagnostic = Assert.Single(
			RunGenerator("""
				public sealed class Insisting
				{
					public required string Name   { get; init; }
					public required string Amount { get; init; }
				}

				[DotGram.Gram("Row : @Insisting = name: ['a'..'z']+\nparse Row")]
				public partial class Short;
				""")
				.Diagnostics
				.Where(d => d.Id == "GRAM4008"));

		Assert.Contains("does not say how to build it", diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void A_constructor_is_preferred_to_writing_into_the_value()
	{
		// §7.3 lists the constructor first, and the difference is visible: the constructor
		// here records that it ran.
		var built = Build("""
			public sealed class Either
			{
				public Either() { }

				public Either(string name) { Name = name; How = "constructor"; }

				public string Name { get; init; } = "";
				public string How  { get; init; } = "initializer";
			}

			[DotGram.Gram("Row : @Either = name: ['a'..'z']+\nparse Row")]
			public partial class Preferred;
			""")
			.GetType("Preferred")!
			.GetMethod("ParseRow", [typeof(string)])!
			.Invoke(null, ["ab"])!;

		Assert.Equal("constructor", built.GetType().GetProperty("How")!.GetValue(built));
	}

	[Fact]
	public void A_type_no_constructor_of_which_the_captures_cover_is_reported()
	{
		// Turned down rather than half-built. The message names what was matched against,
		// because "declares a type and does not say how to build it" is true of a rule
		// whose captures very nearly fit and does not say which one is missing.
		var diagnostic = Assert.Single(
			RunGenerator("""
				public sealed class Priced(string name, int amount)
				{
					public string Name   { get; } = name;
					public int    Amount { get; } = amount;
				}

				[DotGram.Gram("Row : @Priced = name: ['a'..'z']+\nparse Row")]
				public partial class Missing;
				""")
				.Diagnostics
				.Where(d => d.Id == "GRAM4008"));

		Assert.Contains("No constructor of", diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	// ── C# names belong to the consumer's compiler ───────────────────────────────

	[Fact]
	public void A_missing_transformation_is_emitted_and_reported_by_C_sharp()
	{
		RunGenerator(
			"[DotGram.Gram(\"Start : @int = digits: ['0'..'9']+ => @Tini(digits)\\nparse Start\")]\n" +
			"public partial class Misspelled { static int Tiny(string digits) => digits.Length; }",
			out var output);

		var error = Assert.Single(output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

		Assert.Equal("CS0103", error.Id);
		Assert.Contains("Tini", error.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void A_missing_guard_is_emitted_and_reported_by_C_sharp()
	{
		RunGenerator(
			"[DotGram.Gram(\"Start = digits: ['0'..'9']+ & where @Fit(digits)\\nparse Start\")]\n" +
			"public partial class Misspelled { static bool Fits(string digits) => true; }",
			out var output);

		var errors = output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.NotEmpty(errors);
		Assert.All(errors, error =>
		{
			Assert.Equal("CS0103", error.Id);
			Assert.Contains("Fit", error.GetMessage(), StringComparison.Ordinal);
		});
	}

	[Fact]
	public void A_missing_bare_recognizer_is_reported_by_C_sharp()
	{
		RunGenerator(
			"[DotGram.Gram(\"Start = @Unknown & eol\\nparse Start\")]\n" +
			"public partial class Bare;",
			out var output);

		var errors = output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.NotEmpty(errors);
		Assert.All(errors, error =>
		{
			Assert.Equal("CS0103", error.Id);
			Assert.Contains("Unknown", error.GetMessage(), StringComparison.Ordinal);
		});
	}

	// ── Where a C# error lands (§7.6) ────────────────────────────────────────────

	[Fact]
	public void An_error_in_the_grammars_own_C_sharp_is_reported_in_the_authors_file()
	{
		// The point of the whole mechanism, asked of the thing that actually reports it.
		// `Missing` does not exist, so C# has something to say; without a `#line` it says
		// it inside a generated file the author did not write and is told not to edit.
		RunGenerator(
			""""
			using DotGram;

			[Gram("""
				Start : @int = digits: ['0'..'9']+ => @(Missing(digits))
				parse Start
				""")]
			public partial class Broken;
			"""",
			out var output);

		var error = Assert.Single(
			output
				.GetDiagnostics(TestContext.Current.CancellationToken)
				.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
				.ToArray());

		// The mapped span, which is the one every tool shows. The raw one still points into
		// the generated file, and the whole of a `#line` is the difference between the two.
		var at = error.Location.GetMappedLineSpan();

		Assert.Equal("GeneratorDriverTest.cs", at.Path);

		// The fourth line of the source above, 0-based here, and the column `Missing` is
		// written at — not the start of the line, and not the start of the expression,
		// which is where the `@` is.
		Assert.Equal(3,  at.StartLinePosition.Line);
		Assert.Equal(41, at.StartLinePosition.Character);

		Assert.Contains("Missing", error.GetMessage(), StringComparison.Ordinal);
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

		// Anything but information. A grammar that is told what it did not get is still a
		// grammar worth building — §6.3's "no reader overload" is exactly that.
		Assert.Empty(run.Diagnostics.Where(
			static diagnostic => diagnostic.Severity != DiagnosticSeverity.Info));

		using var stream = new MemoryStream();

		var emitted = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		// The generated source with the message, because the message names a line in a file
		// that exists only in this process — without it there is nothing to look at.
		Assert.True(
			emitted.Success,
			string.Join("\n", emitted.Diagnostics.Where(
				static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)) +
			"\n\n" + string.Join(
				"\n",
				run.Results.SelectMany(static r => r.GeneratedSources)
					.Where(static s => !s.HintName.StartsWith("DotGram.", StringComparison.Ordinal))
					.Select(static s => s.SourceText.ToString())));

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

	// ── §8.3 over a streamed parse ───────────────────────────────────────────────

	[Fact]
	public void A_streamed_parse_reports_a_bad_record_on_the_hook()
	{
		// §8.3 where it matters most: a million records, one of them wrong, and the reader
		// needs to know which. The bad line begins the way a good one does and breaks part
		// way through, which is what recovery steps over (§8.2).
		var assembly = Build(
			"using System.Collections.Generic;\n"
			+ "[DotGram.Gram(\"Feed : @string[] = Row* recover eol & eof\\n"
			+ "Row : @string = name: [\'a\'..\'z\']+ & eol => @(name)\\n"
			+ "parse Feed\")]\n"
			+ "public partial class Streamed\n"
			+ "{\n"
			+ "	public static List<string> Bad = new();\n"
			+ "	static partial void OnRecovered(string element, string text, long position,\n"
			+ "		int line, int column, int ordinal, string message) => Bad.Add(text.Trim());\n"
			+ "}");

		var type = assembly.GetType("Streamed")!;
		var read = ((System.Collections.IEnumerable)type
			.GetMethod("ParseFeed", [typeof(TextReader)])!
			.Invoke(null, [new StringReader("aa\nab1\ncc\n")])!)
			.Cast<string>()
			.ToArray();

		// The good records arrive; the bad one is dropped and reported instead.
		Assert.Equal(["aa", "cc"], read);
		Assert.Equal(["ab1"], (List<string>)type.GetField("Bad")!.GetValue(null)!);
	}

	[Fact]
	public void A_missing_predicate_inside_an_element_set_is_reported_by_C_sharp()
	{
		RunGenerator(
			"[DotGram.Gram(\"Start = [@IsVowel | \'0\'..\'9\']+\\nparse Start\")]\n"
			+ "public partial class Sets;",
			out var output);

		var errors = output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.NotEmpty(errors);
		Assert.All(errors, error =>
		{
			Assert.Equal("CS0103", error.Id);
			Assert.Contains("IsVowel", error.GetMessage(), StringComparison.Ordinal);
		});
	}
}
