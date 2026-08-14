using System;
using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The emitter checked by compiling what it wrote and running it.
/// </summary>
/// <remarks>
/// Asserting on the text would only say the generator is consistent with itself.
/// Compiling it says it is valid C#, and running it says the grammar recognizes what
/// it claims to — which is the only claim worth making at this stage.
/// </remarks>
public sealed class CSharpEmitterTests
{
	/// <summary>Compiles a grammar the way the generator does, and returns the C#.</summary>
	static string Emit(string grammar)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		// Anything but information. A grammar is allowed to be told what it did not get and
		// still be a grammar the emitter should be asked about — that is what `Info` is
		// for, and §6.3's "no reader overload" is one.
		Assert.DoesNotContain(
			result.Diagnostics,
			static diagnostic => diagnostic.Severity != GramSeverity.Info);

		return Assert.Single(result.Sources).Text;
	}

	/// <summary>Compiles the emitted source and calls the asking half of a publication.</summary>
	static (bool Matched, object? Value) Invoke(string grammar, string method, string input)
	{
		var (isSuccess, value, _, _) =
			EmittedCode.Match(EmittedCode.Compile(Emit(grammar)), "Grammar", "Try" + method, input);

		return (isSuccess, value);
	}

	/// <summary>The common case: one rule, published with <c>parse</c>.</summary>
	static (bool Matched, string Value) Run(string grammar, string input)
	{
		var (matched, value) = Invoke(grammar + "\nparse Start", "ParseStart", input);

		return (matched, (string)(value ?? ""));
	}

	[Theory]
	[InlineData("abc",  true)]
	[InlineData("abd",  false)]
	[InlineData("ab",   false)]
	[InlineData("abcd", false)]     // parse requires the whole input
	public void Literals(string input, bool expected) =>
		Assert.Equal(expected, Run("""Start = 'a' & 'b' & 'c'""", input).Matched);

	[Theory]
	[InlineData("42",    true)]
	[InlineData("0",     true)]
	[InlineData("",      false)]
	[InlineData("4x",    false)]
	public void Element_sets_and_repetition(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = ['0'..'9']+", input).Matched);

	[Theory]
	[InlineData("cat", true)]
	[InlineData("dog", true)]
	[InlineData("cow", false)]
	public void Ordered_choice(string input, bool expected) =>
		Assert.Equal(expected, Run("""Start = "cat" | "dog" """, input).Matched);

	[Fact]
	public void Ordered_choice_backtracks_across_a_shared_prefix()
	{
		// The case a commit point would have broken: both alternatives begin with the
		// same rule and diverge only after it (docs/syntax.md §11).
		var grammar = """
			Start = Call | Index
			Call  = Name & '(' & ')'
			Index = Name & '[' & ']'
			Name  = ['a'..'z']+
			""";

		Assert.True(Run(grammar, "foo()").Matched);
		Assert.True(Run(grammar, "foo[]").Matched);
		Assert.False(Run(grammar, "foo{}").Matched);
	}

	[Theory]
	[InlineData("ab",   true)]
	[InlineData("b",    true)]
	[InlineData("aab",  false)]
	public void Optional(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a'? & 'b'", input).Matched);

	[Theory]
	[InlineData("aaa", true)]
	[InlineData("aa",  false)]
	[InlineData("aaaa", false)]
	public void Counted_repetition(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a'{3}", input).Matched);

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void Lookahead_consumes_nothing(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a' & ?='b' & 'b'", input).Matched);

	[Fact]
	public void The_value_is_the_matched_text() =>
		Assert.Equal("hello", Run("Start = ['a'..'z']+", "hello").Value);

	[Fact]
	public void Rules_call_each_other()
	{
		var grammar = """
			Start  = Digits & '-' & Digits
			Digits = ['0'..'9']+
			""";

		Assert.True (Run(grammar, "12-34").Matched);
		Assert.False(Run(grammar, "12-").Matched);
	}

	// ── The standard library (§3.1) ──────────────────────────────────────────────

	[Theory]
	[InlineData("a\n",   true)]
	[InlineData("a\r\n", true)]
	[InlineData("a\r",   true)]
	[InlineData("a",     false)]
	public void Eol(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a' & eol", input).Matched);

	[Theory]
	[InlineData("ab",  true)]
	[InlineData("a\n", true)]
	[InlineData("a",   false)]
	public void Any_is_one_of_whatever(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a' & any", input).Matched);

	[Fact]
	public void Eof_consumes_nothing_and_is_only_true_at_the_end() =>
		Assert.True(Run("Start = 'a' & eof", "a").Matched);

	[Fact]
	public void None_matches_the_empty_sequence() =>
		Assert.True(Run("Start = 'a' & none", "a").Matched);

	// ── Publication (§6) ─────────────────────────────────────────────────────────

	const string Digits = """
		Start = ['0'..'9']+

		""";

	/// <summary>Compiles the emitted source and walks a <c>find</c>.</summary>
	static object?[] Occurrences(string grammar, string method, string input) =>
		EmittedCode.Found(EmittedCode.Compile(Emit(grammar)), "Grammar", method, input);

	[Fact]
	public void Parse_requires_the_input_to_end() =>
		Assert.False(Run("Start = ['0'..'9']+", "12ab").Matched);

	[Fact]
	public void Find_takes_every_occurrence() =>
		Assert.Equal(
			new object?[] { "12", "34" },
			Occurrences(Digits + "find Start", "FindStart", "ab12cd34"));

	[Fact]
	public void And_the_first_one_is_LINQ_s_business_rather_than_a_directive_s() =>
		Assert.Equal("12", Occurrences(Digits + "find Start", "FindStart", "ab12cd34")[0]);

	[Fact]
	public void Find_yields_nothing_rather_than_matching_nothing() =>
		Assert.Empty(Occurrences(Digits + "find Start", "FindStart", "abc"));

	[Fact]
	public void As_renames_the_pair() =>
		Assert.True(Invoke(Digits + "parse Start as ReadDigits", "ReadDigits", "12").Matched);

	[Fact]
	public void One_grammar_can_publish_the_same_rule_both_ways()
	{
		var grammar = Digits + """
			parse Start
			find Start
			""";

		Assert.True(Invoke(grammar, "ParseStart", "12").Matched);
		Assert.Single(Occurrences(grammar, "FindStart", "ab12"));
	}

	[Fact]
	public void Two_directives_wanting_one_name_is_a_diagnostic()
	{
		var result = GramCompiler.Compile("""
			Start = 'a'
			parse Start
			parse Start
			""");

		Assert.Equal(GrammarBinder.DuplicatePublication, Assert.Single(result.Diagnostics).Id);
		Assert.Empty(result.Sources);
	}

	/// <summary>
	/// A rule that named its own type gets a method to build it with, and the captures
	/// are its parameters — which is what lets a <c>=&gt;</c> use their names without
	/// dodging every local the recognizer has.
	/// </summary>
	[Fact]
	public void A_declared_type_is_built_by_a_method_of_its_own()
	{
		var source = Emit("""
			@using System.Globalization;

			Start : @int = digits: ['0'..'9']+ => @int.Parse(digits, @CultureInfo.InvariantCulture)
			parse Start
			""");

		Assert.Contains("using System.Globalization;", source);
		Assert.Contains(
			"static int Construct_Start(string parserText, string digits) =>",
			source);
		Assert.Contains("int.Parse(digits, CultureInfo.InvariantCulture);", source);

		// And the publication hands back that type rather than the matched text.
		Assert.Contains("public static int ParseStart(string input)", source);
	}

	[Fact]
	public void A_broken_grammar_emits_nothing()
	{
		var result = GramCompiler.Compile("Start = Missing");

		Assert.NotEmpty(result.Diagnostics);
		Assert.Empty(result.Sources);
	}

	[Fact]
	public void Blocks_written_in_one_piece_land_at_the_depth_they_are_written_at()
	{
		// `Match<T>`, `Failure` and `Grow` are raw string literals in the emitter, which
		// carry whatever line endings the emitter's own file was saved with. Read as one
		// line — which is what happens when those endings are not the ones the writer
		// splits on — they arrive indented once and flat after that. The code still
		// compiles, so nothing but this notices.
		// In a namespace, so the depth every one of them sits at is two and not "whatever
		// the class happened to be nested at".
		var source = Assert.Single(GramCompiler.Compile(
			"Start = 'a'+ & 'b'\nparse Start",
			new GramCompilerOptions { ClassName = "Grammar", Namespace = "My.App" }).Sources).Text;

		Assert.Contains("\t\tpublic readonly struct Match<T>\r\n\t\t{\r\n\t\t\tprivate Match(", source);
		Assert.Contains("\t\tstruct Failure\r\n\t\t{\r\n\t\t\t/// <summary>",                    source);
		Assert.Contains("\t\tstatic int[] Grow(global::System.Span<int> from)\r\n\t\t{\r\n",     source);
	}

	[Fact]
	public void Only_a_rule_of_strengths_takes_one()
	{
		var source = Emit("""
			E : @int = left: E & '+' & right: E << 1 => @(left + right)
			         | digits: D+                    => @int.Parse(digits)
			D        = ['0'..'9']
			parse E
			""");

		// The rule that climbs takes the strength; the one beside it is untouched, which is
		// the whole of what variant A buys.
		Assert.Contains("static int Recognize_E(global::System.ReadOnlySpan<char> text, int pos, int power, ref Failure failure, out int value)", source);
		Assert.Contains("static int Recognize_D(global::System.ReadOnlySpan<char> text, int pos, ref Failure failure)", source);

		// `<< 1` reads its right operand at 2 — one tighter, so a `+` cannot appear in it.
		Assert.Contains("Recognize_E(text, p, 2, ref failure, out v1);", source);

		// And publication asks at 0, which admits everything.
		Assert.Contains("Recognize_E_Whole(text, 0, 0, ref failure, out var recognized);", source);
	}

	// ── Streaming (§6.3) ─────────────────────────────────────────────────────────

	/// <summary>What the emitted window holds before it has to grow.</summary>
	/// <remarks>
	/// The generator's own constant, and the tests below are about what happens at its
	/// edge — so they read it rather than restating it, and a change to it moves them.
	/// </remarks>
	const int Buffer = 4096;

	/// <summary>An input whose only occurrence straddles the end of the first window.</summary>
	static string AcrossTheBoundary(string occurrence, int before = 6) =>
		new string('x', Buffer - before) + occurrence + new string('x', 16);

	[Fact]
	public void A_find_over_a_reader_takes_the_same_occurrences()
	{
		var assembly = EmittedCode.Compile(Emit(Digits + "find Start"));

		Assert.Equal(
			EmittedCode.Found(assembly, "Grammar", "FindStart", "ab12cd34"),
			EmittedCode.Found(assembly, "Grammar", "FindStart", "ab12cd34", typeof(TextReader)));
	}

	[Fact]
	public void An_occurrence_that_straddles_the_buffer_is_one_occurrence()
	{
		// The defect a windowed parse has if it believes what a full buffer tells it: the
		// digits stop at the edge of what is held, which looks exactly like the input
		// stopping. Read naively this is "123456" and "7890" — two occurrences where the
		// input has one.
		var input = AcrossTheBoundary("1234567890");

		Assert.Equal(
			new object?[] { "1234567890" },
			EmittedCode.Found(
				EmittedCode.Compile(Emit(Digits + "find Start")),
				"Grammar",
				"FindStart",
				input,
				typeof(TextReader)));
	}

	[Fact]
	public void And_is_reported_at_its_place_in_the_whole_input()
	{
		// The offset is into the input and not into the buffer, which is the whole reason
		// §6.3 makes it a `long`. This one is past the first window, so a position measured
		// from the buffer would be a small number that means nothing.
		Assert.Equal(
			[Buffer - 6],
			EmittedCode.FoundAt(
				EmittedCode.Compile(Emit(Digits + "find Start")),
				"Grammar",
				"FindStart",
				AcrossTheBoundary("1234567890"),
				typeof(TextReader)));
	}

	[Fact]
	public void An_occurrence_longer_than_the_window_grows_it()
	{
		// The analysis bounds retention by the grammar, not by a constant: an element that
		// does not fit is a long record rather than a runaway, and the window grows for it.
		var digits = new string('7', Buffer * 3);

		Assert.Equal(
			new object?[] { digits },
			EmittedCode.Found(
				EmittedCode.Compile(Emit(Digits + "find Start")),
				"Grammar",
				"FindStart",
				"ab" + digits + "cd",
				typeof(TextReader)));
	}

	[Fact]
	public void A_reader_that_gives_nothing_yields_nothing() =>
		Assert.Empty(
			EmittedCode.Found(
				EmittedCode.Compile(Emit(Digits + "find Start")),
				"Grammar",
				"FindStart",
				"",
				typeof(TextReader)));

	[Fact]
	public void Only_find_gets_a_reader_so_far()
	{
		// `parse` needs the decomposition of Retention.PlanFor — what lets its window move
		// is a committed repetition inside it — and that is not built yet. Pinned so that
		// the day it is, this test is what says so.
		var source = Emit(Digits + "parse Start\nfind Start");

		Assert.Contains(
			"IEnumerable<Match<string>> FindStart(global::System.IO.TextReader input)",
			source,
			StringComparison.Ordinal);

		Assert.DoesNotContain("TryParseStart(global::System.IO.TextReader", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_rule_that_would_hold_the_input_gets_no_reader_overload()
	{
		// `any*` reads to the end of the file and can give any of it back, so the window
		// would be the input and streaming would be a word rather than a property (§6.3).
		var source = Emit("Start = any* & 'z'\nfind Start");

		Assert.DoesNotContain("TextReader", source, StringComparison.Ordinal);
		Assert.DoesNotContain("class Window", source, StringComparison.Ordinal);
	}

	[Fact]
	public void And_is_told_so_where_it_asked()
	{
		// The alternative is what the author actually meets: a call that does not bind,
		// naming neither the rule responsible nor anything they could do about it.
		var result = GramCompiler.Compile(
			"Start = any* & 'z'\nfind Start",
			new GramCompilerOptions { ClassName = "Grammar" });

		var told = Assert.Single(result.Diagnostics);

		Assert.Equal(Retention.NotStreamable, told.Id);
		Assert.Equal(GramSeverity.Info,       told.Severity);

		// The innermost part that does not fit, rather than the whole body it sits in.
		Assert.Contains("any*",   told.Message, StringComparison.Ordinal);
		Assert.Contains("§6.3",   told.Message, StringComparison.Ordinal);
		Assert.Contains("FindStart", told.Message, StringComparison.Ordinal);

		// Information, so the parser is still generated: what it did not get is an
		// overload, not a method.
		Assert.NotEmpty(result.Sources);
	}

	[Fact]
	public void A_rule_that_streams_is_told_nothing() =>
		Assert.Empty(GramCompiler.Compile(
			"Start = ['0'..'9']+\nfind Start",
			new GramCompilerOptions { ClassName = "Grammar" }).Diagnostics);

	[Fact]
	public void And_neither_is_a_parse_that_has_no_reader_overload_yet() =>
		// The reason there is a fact about this compiler rather than about the grammar in
		// front of it. Saying it on every build of every grammar would be noise, and
		// docs/status.md is where it belongs.
		Assert.Empty(GramCompiler.Compile(
			"Start = any* & 'z'\nparse Start",
			new GramCompilerOptions { ClassName = "Grammar" }).Diagnostics);

	[Fact]
	public void The_class_goes_where_it_was_asked_to()
	{
		var source = GramCompiler.Compile(
			"Start = 'a'",
			new GramCompilerOptions { ClassName = "Feed", Namespace = "My.App" });

		Assert.Contains("namespace My.App",  Assert.Single(source.Sources).Text);
		Assert.Contains("partial class Feed", source.Sources[0].Text);
	}
}
