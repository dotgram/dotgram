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

	/// <summary>Compiles the emitted source and calls the Try- half of a publication.</summary>
	static (bool Matched, object Value) Invoke(string grammar, string method, string input)
	{
		var type      = EmittedCode.Compile(Emit(grammar)).GetType("Grammar")!;
		var arguments = new object?[] { input, null, null, null };
		var matched   = (bool)type.GetMethod("Try" + method)!.Invoke(null, arguments)!;

		return (matched, arguments[1]!);
	}

	/// <summary>The common case: one rule, published with <c>parse</c>.</summary>
	static (bool Matched, string Value) Run(string grammar, string input)
	{
		var (matched, value) = Invoke(grammar + "\nparse Start", "ParseStart", input);

		return (matched, (string)value);
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
		// same rule and diverge only after it (docs/syntax.md §10).
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

	[Fact]
	public void Match_does_not_require_the_input_to_end()
	{
		var (matched, value) = Invoke(Digits + "match Start", "MatchStart", "12ab");

		Assert.True(matched);
		Assert.Equal("12", value);
	}

	[Fact]
	public void Parse_does()
	{
		Assert.False(Run("Start = ['0'..'9']+", "12ab").Matched);
	}

	[Fact]
	public void Find_takes_the_first_occurrence()
	{
		var (matched, value) = Invoke(Digits + "find Start", "FindStart", "ab12cd34");

		Assert.True(matched);
		Assert.Equal("12", value);
	}

	[Fact]
	public void Find_all_takes_every_occurrence()
	{
		var (matched, value) = Invoke(Digits + "find all Start", "FindAllStart", "ab12cd34");

		Assert.True(matched);
		Assert.Equal(new[] { "12", "34" }, value);
	}

	[Fact]
	public void Find_reports_no_occurrence_rather_than_matching_nothing()
	{
		Assert.False(Invoke(Digits + "find Start", "FindStart", "abc").Matched);
	}

	[Fact]
	public void As_renames_the_pair() =>
		Assert.True(Invoke(Digits + "parse Start as ReadDigits", "ReadDigits", "12").Matched);

	[Fact]
	public void One_grammar_can_publish_the_same_rule_several_ways()
	{
		var grammar = Digits + """
			parse Start
			find all Start
			""";

		Assert.True(Invoke(grammar, "ParseStart",   "12").Matched);
		Assert.True(Invoke(grammar, "FindAllStart", "ab12").Matched);
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

	[Fact]
	public void A_broken_grammar_emits_nothing()
	{
		var result = GramCompiler.Compile("Start = Missing");

		Assert.NotEmpty(result.Diagnostics);
		Assert.Empty(result.Sources);
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
