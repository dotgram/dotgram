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
	static bool Matches(string grammar, string input) => Parsed(grammar, input).IsSuccess;

	static (bool IsSuccess, object? Value, string? Error, int Position) Parsed(string grammar, string input)
	{
		var result = Compile(grammar + "\nparse Start");

		Assert.Empty(result.Diagnostics);

		return EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input);
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

	// ── Backtracking (§11) ───────────────────────────────────────────────────────

	// A greedy operand that took too much has to give it back when what follows fails.
	// Every one of these matched nothing before the recognizer became a machine with a
	// stack of the points it could have gone another way.

	[Fact] public void Optional_gives_back()  => Assert.True(Matches("Start = 'a'? & 'a'",     "a"));
	[Fact] public void Star_gives_back()      => Assert.True(Matches("Start = 'a'* & 'a'",     "a"));
	[Fact] public void Plus_gives_back()      => Assert.True(Matches("Start = 'a'+ & 'a'",     "aa"));
	[Fact] public void Counted_gives_back()   => Assert.True(Matches("Start = 'a'{1,2} & 'a'", "aa"));

	[Fact]
	public void A_choice_gives_back_across_the_rest_of_the_sequence() =>
		// The specification's own counterexample: §11 rests the rule that alternatives
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

	// ── Where a failure is reported ─────────────────────────────────────────────

	/// <summary>The message and the position a grammar refuses an input with.</summary>
	static (string Error, int Position) Refusal(string grammar, string input)
	{
		var (isSuccess, _, error, position) = Parsed(grammar, input);

		Assert.False(isSuccess, input);

		return (error!, position);
	}

	[Fact]
	public void A_refusal_names_the_position_the_input_stopped_making_sense_at() =>
		Assert.Equal(2, Refusal("""Start = "ab" & ['c'] & ['d']""", "abXY").Position);

	[Fact]
	public void The_position_is_where_the_failing_operand_began_not_where_it_gave_up() =>
		// `"abcd"` is one operand and it starts at 0, so that is what is named, though the
		// character that did not fit is at 2. Sharpening this means recording the offset
		// at each failing test rather than one position at the point of giving up — a
		// refinement of what is here, not a different shape.
		Assert.Equal(0, Refusal("""Start = "abcd" """, "abXY").Position);

	[Fact]
	public void It_is_the_furthest_reached_and_not_the_last_tried() =>
		// The first alternative fails at 0 and the second gets to 2 before failing. What
		// is worth reporting is how far the input could be followed, so the position only
		// ever rises — a later, shallower failure does not overwrite a deeper one.
		Assert.Equal(2, Refusal("""Start = ("abc" | "ab") & 'z' """, "abq").Position);

	[Fact]
	public void A_failure_inside_a_rule_is_the_caller_s_failure_too() =>
		// The state is threaded through the call rather than returned, so a rule boundary
		// does not flatten the position back to where the call was made.
		Assert.Equal(
			2,
			Refusal("Inner = ['a'] & ['b'] & ['c']\nStart = Inner", "abq").Position);

	[Fact]
	public void A_lookahead_does_not_report_how_far_it_looked() =>
		// It reached position 2 inside itself and consumed nothing. Naming 2 would point
		// at input the match never needed; what failed is the lookahead, at 0.
		Assert.Equal(
			0,
			Refusal("Start = ?=(['a'] & ['b'] & ['z']) & ['a']", "abq").Position);

	// ── Captures, and the value they build (§7.3) ───────────────────────────────

	/// <summary>Compiles and runs as <see cref="Matches"/> does, and hands back the value.</summary>
	static object? Built(string grammar, string input)
	{
		var (isSuccess, value, _, _) = Parsed(grammar, input);

		Assert.True(isSuccess, input);

		return value;
	}

	/// <summary>A member of a built value, by name — the type did not exist to compile against.</summary>
	static object? Read(object? value, params string[] path)
	{
		foreach (var name in path)
			value = value?.GetType().GetProperty(name)?.GetValue(value);

		return value;
	}

	[Fact]
	public void Each_capture_is_a_member_named_after_it()
	{
		var value = Built("Start = scheme: \"ab\" & rest: 'c'", "abc");

		Assert.Equal("ab", Read(value, "Scheme"));
		Assert.Equal("c",  Read(value, "Rest"));
	}

	[Fact]
	public void A_repeated_capture_is_the_text_of_the_whole_run() =>
		// §10 binds a capture tighter than a quantifier, so this is one capture repeated.
		// §7.3 gives it the text joined, which is the run rather than its last iteration.
		Assert.Equal("8080", Read(Built("Start = digits: ['0'..'9']+", "8080"), "Digits"));

	[Fact]
	public void A_run_that_matched_nothing_is_empty_rather_than_absent() =>
		Assert.Equal("", Read(Built("Start = digits: ['0'..'9']* & 'x'", "x"), "Digits"));

	[Fact]
	public void An_option_that_was_not_taken_is_absent_rather_than_empty() =>
		Assert.Null(Read(Built("Start = (sign: '-')? & 'x'", "x"), "Sign"));

	[Fact]
	public void A_capture_of_a_rule_that_builds_holds_its_value() =>
		Assert.Equal(
			"x",
			Read(Built("Inner = letter: 'x'\nStart = inner: Inner", "x"), "Inner", "Letter"));

	[Fact]
	public void A_capture_the_match_gave_back_is_not_in_the_value()
	{
		// The first alternative matches 'a' and then fails on 'c', so the match resumes in
		// the second — where nothing was captured, and `a` must be as unwritten as if the
		// first alternative had never been tried.
		var value = Built("Start = (a: 'x' & 'y' | 'x' & b: 'z')", "xz");

		Assert.Null(Read(value, "A"));
		Assert.Equal("z", Read(value, "B"));
	}

	[Fact]
	public void The_same_name_in_two_alternatives_is_one_member() =>
		Assert.Equal("y", Read(Built("Start = (v: \"xy\" | v: 'y')", "y"), "V"));

	[Fact]
	public void A_repeated_capture_of_a_rule_is_a_sequence_of_its_values()
	{
		var value = Built("Item = letter: ['a'..'z']\nStart = items: Item+", "abc");
		var items = (Array)Read(value, "Items")!;

		Assert.Equal(3, items.Length);
		Assert.Equal(["a", "b", "c"], items.Cast<object>().Select(item => Read(item, "Letter")));
	}

	[Fact]
	public void An_empty_run_is_an_empty_sequence_rather_than_null() =>
		Assert.Empty((Array)Read(
			Built("Item = letter: 'x'\nStart = items: Item* & 'y'", "y"), "Items")!);

	[Fact]
	public void A_sequence_gives_back_what_an_abandoned_attempt_appended()
	{
		// `Item+` takes three, `'z'` fails, and the repetition hands one back at a time
		// until `'c' & 'z'` fits. What it collected has to shrink with it — the length at
		// the moment of the push is on the backtracking frame, and the resume truncates
		// to it.
		var items = (Array)Read(
			Built("Item = letter: ['a'..'z']\nStart = items: Item+ & 'c' & 'z'", "abcz"), "Items")!;

		Assert.Equal(["a", "b"], items.Cast<object>().Select(item => Read(item, "Letter")));
	}

	[Fact]
	public void And_a_repetition_inside_a_repetition_gives_back_only_its_own_iteration()
	{
		var items = (Array)Read(
			Built("Item = letter: ['a'..'z']\nStart = (items: Item+ & '.')* & 'x'", "ab.cd.x"),
			"Items")!;

		Assert.Equal(["a", "b", "c", "d"], items.Cast<object>().Select(item => Read(item, "Letter")));
	}

	[Fact]
	public void A_capture_of_text_inside_a_repetition_is_still_the_run_it_matched() =>
		// §7.3 keeps the two apart: a quantifier over text joins it, a quantifier over a
		// rule collects. Only the second is a sequence.
		Assert.Equal("8080", Read(Built("Start = digits: ['0'..'9']+", "8080"), "Digits"));

	[Fact]
	public void But_text_captured_around_something_else_is_still_refused() =>
		Refused(GrammarNormalizer.UnbuiltCapture, "Start = (a: 'x' & 'y')+");

	[Fact]
	public void Nor_is_one_inside_a_lookahead() =>
		Refused(GrammarNormalizer.UnbuiltCapture, "Start = ?=(a: 'x') & 'x'");

	[Fact]
	public void One_name_cannot_hold_two_different_things() =>
		Refused(
			GrammarNormalizer.CaptureTypeMismatch,
			"Item = a: 'x'\nStart = (v: Item | v: 'y')");

	// ── A rule that declares its own type and builds it (§7.3) ──────────────────

	[Fact]
	public void A_rule_may_name_a_C_sharp_type_and_say_how_to_build_it() =>
		Assert.Equal(
			42,
			Built(
				"""
				@using System.Globalization;

				Start : @int = ['0'..'9']+ => @int.Parse(text, @CultureInfo.InvariantCulture)
				""",
				"42"));

	[Fact]
	public void The_matched_text_is_supplied_under_the_name_text() =>
		Assert.Equal(
			3,
			Built("Start : @int = ['a'..'z']+ => @(text.Length)", "abc"));

	[Fact]
	public void Captures_reach_the_expression_by_their_own_names() =>
		Assert.Equal(
			"b-a",
			Built("""Start : @string = a: 'a' & b: 'b' => @(b + "-" + a)""", "ab"));

	// ── `where` guards (§8.1) ───────────────────────────────────────────────────

	[Theory]
	[InlineData("12",  true)]
	[InlineData("123", false)]
	public void A_guard_asks_a_question_of_the_text_so_far(string input, bool expected) =>
		Assert.Equal(expected, Matches("Start = ['0'..'9']+ & where @(text.Length < 3)", input));

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void And_of_the_captures_written_before_it(string input, bool expected) =>
		Assert.Equal(
			expected,
			Matches("""Start = a: 'a' & b: ['a'..'z'] & where @(b == "b")""", input));

	[Fact]
	public void A_failing_guard_is_a_non_match_and_a_sibling_is_tried() =>
		// Recognition, not a value failure: saying no sends the match back into the choice
		// rather than ending it. §8.1 is where the two are told apart.
		Assert.True(Matches(
			"""Start = (a: "ab" & where @(a == "xy") | a: "ab") & 'c'""",
			"abc"));

	[Fact]
	public void A_guard_may_stand_where_nothing_has_been_captured() =>
		Assert.True(Matches("Start = ['0'..'9']+ & where @(true)", "7"));

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
