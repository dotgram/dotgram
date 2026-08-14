using System;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;

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

		Assert.Empty(result.Diagnostics);

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
