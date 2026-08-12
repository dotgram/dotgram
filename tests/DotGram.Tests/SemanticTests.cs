using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Small grammars run against real input, and grammars that must be refused.
/// </summary>
/// <remarks>
/// A corpus rather than a test per feature. What breaks a grammar engine is almost
/// never <c>X*</c> on its own — it is <c>X*</c> next to something else, or a construct
/// that parses and then quietly means nothing. Several of these were written from a
/// review that found exactly that: syntax accepted at the front of the pipeline and
/// lost somewhere before the back of it.
/// </remarks>
public sealed class SemanticTests
{
	/// <summary>Compiles, compiles the result, and runs it. Fails if the grammar does not compile.</summary>
	static bool Matches(string grammar, string input)
	{
		var result = Compile(grammar + "\nparse Start");

		Assert.Empty(result.Diagnostics);

		var type      = EmittedCode.Compile(result.Sources[0].Text).GetType("Grammar")!;
		var arguments = new object?[] { input, null, null, null };

		return (bool)type.GetMethod("TryParseStart")!.Invoke(null, arguments)!;
	}

	static GramCompilation Compile(string grammar) => GramCompiler.Compile(
		grammar,
		new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

	/// <summary>The one diagnostic a grammar must be refused with.</summary>
	static void Refused(string id, string grammar)
	{
		var diagnostics = Compile(grammar).Diagnostics;

		Assert.True(
			diagnostics.Any(diagnostic => diagnostic.Id == id),
			$"Expected {id}; got " + (diagnostics.Count == 0
				? "no diagnostics at all"
				: string.Join(", ", diagnostics.Select(diagnostic => diagnostic.ToString()))));
	}

	// ── Backtracking (§10) ───────────────────────────────────────────────────────

	// A greedy operand that took too much has to give it back when what follows fails.
	// Every one of these matched nothing before the recognizer became a machine with a
	// stack of the points it could have gone another way.

	[Fact] public void Optional_gives_back()  => Assert.True(Matches("Start = 'a'? & 'a'",     "a"));
	[Fact] public void Star_gives_back()      => Assert.True(Matches("Start = 'a'* & 'a'",     "a"));
	[Fact] public void Plus_gives_back()      => Assert.True(Matches("Start = 'a'+ & 'a'",     "aa"));
	[Fact] public void Counted_gives_back()   => Assert.True(Matches("Start = 'a'{1,2} & 'a'", "aa"));

	[Fact]
	public void A_choice_gives_back_across_the_rest_of_the_sequence() =>
		// The specification's own counterexample: §10 rests the rule that alternatives
		// may never be reordered on this working.
		Assert.True(Matches("""Start = ("x" | "xy") & 'y'""", "xy"));

	[Fact]
	public void And_keeps_giving_back_until_something_fits() =>
		Assert.True(Matches("Start = ['a'..'z']* & 'c' & ['a'..'z']*", "abcde"));

	[Theory]
	[InlineData("aaab", true)]
	[InlineData("aaa",  false)]
	public void Nested_repetition_backtracks(string input, bool expected) =>
		Assert.Equal(expected, Matches("Start = ('a'+)+ & 'b'", input));

	[Fact]
	public void Backtracking_does_not_make_a_failing_match_succeed() =>
		Assert.False(Matches("Start = 'a'* & 'b'", "aaa"));

	[Fact]
	public void A_lookahead_leaves_nothing_behind_to_backtrack_into() =>
		Assert.True(Matches("Start = ?=('a' | 'b') & 'a' & 'b'", "ab"));

	[Fact]
	public void Repetition_longer_than_the_first_stack_page() =>
		// The stack starts at 48 ints and grows; this needs far more frames than that.
		Assert.True(Matches("Start = ['a'..'z']* & 'z'", new string('a', 500) + "z"));

	// ── Unicode categories (§3.1) ────────────────────────────────────────────────

	[Theory]
	[InlineData("AB",  true)]
	[InlineData("Ab",  false)]
	public void Category_by_abbreviation(string input, bool expected) =>
		Assert.Equal(expected, Matches(@"Start = [\p{Lu}]+", input));

	[Theory]
	[InlineData("abc", true)]
	[InlineData("ab1", false)]
	public void A_category_group_is_every_category_in_it(string input, bool expected) =>
		Assert.Equal(expected, Matches(@"Start = [\p{L}]+", input));

	[Fact]
	public void Digits_by_category() => Assert.True(Matches(@"Start = [\p{Nd}]+", "2026"));

	[Fact]
	public void An_unknown_category_is_refused() =>
		Refused(GramLexer.UnknownCategory, @"Start = [\p{Zz}]");

	// ── References inside an element set (§3.1) ──────────────────────────────────

	[Theory]
	[InlineData("ab",  true)]
	[InlineData("a1",  false)]
	public void A_set_may_name_an_elementary_rule(string input, bool expected) =>
		Assert.Equal(expected, Matches("Letter = ['a'..'z']\nStart = [Letter]+", input));

	[Theory]
	[InlineData("a1",  true)]
	[InlineData("a-",  false)]
	public void And_is_merged_with_the_rest_of_the_set(string input, bool expected) =>
		Assert.Equal(expected, Matches("Letter = ['a'..'z']\nStart = [Letter | '0'..'9']+", input));

	[Fact]
	public void A_rule_declared_after_the_set_that_names_it_still_merges() =>
		Assert.True(Matches("Start = [Letter]+\nLetter = ['a'..'z']", "ab"));

	[Fact]
	public void Complementing_a_set_complements_what_was_merged_into_it() =>
		Assert.False(Matches("Letter = ['a'..'z']\nStart = [^ Letter]+", "ab"));

	[Fact]
	public void A_rule_that_is_not_one_element_cannot_be_in_a_set() =>
		Refused(GrammarNormalizer.UnsupportedElement, "Pair = 'a' & 'b'\nStart = [Pair]");

	// ── Literals ────────────────────────────────────────────────────────────────

	[Fact]
	public void A_character_literal_holds_one_character() =>
		Refused(GramLexer.MalformedCharacter, "Start = 'ab'");

	[Fact]
	public void An_empty_character_literal_is_refused() =>
		Refused(GramLexer.MalformedCharacter, "Start = ''");

	[Theory]
	[InlineData(@"'\0'",     "\0")]
	[InlineData(@"'\a'",     "\a")]
	[InlineData(@"'\v'",     "\v")]
	[InlineData(@"'é'", "é")]
	[InlineData("'\u2028'", "\u2028")]     // a line separator: legal in a grammar, not in C# source
	public void Control_and_non_ascii_characters_survive_emission(string literal, string input) =>
		Assert.True(Matches($"Start = {literal}", input));

	// ── Repetition counts ───────────────────────────────────────────────────────

	[Fact]
	public void A_count_too_large_for_an_int_is_a_diagnostic_and_not_a_crash() =>
		Refused(GramParser.InvalidCount, "Start = 'a'{999999999999999999999999999}");

	[Fact]
	public void A_range_that_can_never_match_is_a_diagnostic() =>
		Refused(GramParser.InvalidCount, "Start = 'a'{5,2}");

	[Theory]
	[InlineData("aa",   true)]
	[InlineData("aaa",  true)]
	[InlineData("a",    false)]
	[InlineData("aaaa", false)]
	public void Bounded_repetition(string input, bool expected) =>
		Assert.Equal(expected, Matches("Start = 'a'{2,3}", input));
}
