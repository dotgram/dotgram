using System;
using System.Globalization;

using DotGram.Examples.Expressions;

using Xunit;

namespace DotGram.Tests.Calculators;

/// <summary>
/// The same arithmetic, written the other ways.
/// </summary>
/// <remarks>
/// <para>
/// CalculatorExample.cs is the one to read: one rule, strengths, and three publications.
/// The grammars beside this file are the same language spelled differently — precedence
/// as a stack of rules, a publication over two numbers instead of three, a tree built by
/// one rule rather than five — and what is worth having of them is the check that they
/// agree, which is a test rather than something to copy.
/// </para>
/// <para>
/// They are here rather than under examples/ for that reason: an example is a thing to
/// take, and a second spelling of a language somebody already has is not one.
/// </para>
/// </remarks>
public sealed class CalculatorFormsTests
{
	// ── One grammar, two calculators ─────────────────────────────────────────────

	/// <summary>The same four rules, published twice over two different numbers.</summary>
	/// <remarks>
	/// What separates the two is one rule and one word: `parse Sum with (Value = …)`. The
	/// arithmetic is written once and its `=&gt;` bodies name no type, so `left + right` is
	/// C#'s `+` on whichever number arrived — and `Sum : Value` carries that type out to the
	/// published method, which is why one of these returns `int` and the other `decimal`.
	/// </remarks>
	[Theory]
	[InlineData("1+2*3",     7)]
	[InlineData("(1+2)*3",   9)]
	[InlineData("7/2",       3)]     // integer division, because this one works in int
	public void One_grammar_publishes_an_int_calculator(string expression, int expected) =>
		Assert.Equal(expected, TwoCalculators.EvaluateInt(expression));

	[Theory]
	[InlineData("1+2*3",     "7")]
	[InlineData("(1+2)*3",   "9")]
	[InlineData("7/2",       "3.5")]  // and this one in double, from the same four rules
	[InlineData("1.5*2",     "3")]    // double, so no trailing zero where decimal would keep one
	public void And_a_double_one_beside_it(string expression, string expected) =>
		Assert.Equal(
			expected,
			TwoCalculators.EvaluateDouble(expression).ToString(CultureInfo.InvariantCulture));

	/// <summary>And the two really are two types, not one with a conversion.</summary>
	[Fact]
	public void And_each_hands_back_its_own_type()
	{
		Assert.IsType<int>(TwoCalculators.EvaluateInt("1"));
		Assert.IsType<double>(TwoCalculators.EvaluateDouble("1"));

		// The int one has never heard of a decimal point: `Value` is `IntNumber` there.
		Assert.False(TwoCalculators.TryEvaluateInt("1.5").IsSuccess);
		Assert.True(TwoCalculators.TryEvaluateDouble("1.5").IsSuccess);
	}

	// ── The calculator that recurses both ways ───────────────────────────────────

	static string Decimal(string expression) =>
		DecimalCalculator.Evaluate(expression).ToString(CultureInfo.InvariantCulture);

	[Theory]
	[InlineData("1-2-3",         "-4")]     // (1-2)-3 — Sum takes its left operand at its own level
	[InlineData("100/5/2",       "10")]     // (100/5)/2, the same reason
	[InlineData("2^3^2",        "512")]     // 2^(3^2) — Power takes its right one there instead
	[InlineData("(2^3)^2",       "64")]     // and this is what the other grouping means
	public void Each_operator_groups_by_which_side_is_parsed_at_its_own_level(
		string expression, string expected) =>
		Assert.Equal(expected, Decimal(expression));

	[Theory]
	[InlineData("1/8",        "0.125")]     // `: @decimal`, so not the 0 the int one gives
	[InlineData("1.5*2",        "3.0")]
	[InlineData("2^-2",        "0.25")]
	[InlineData("-2^2",          "-4")]     // -(2^2): `^` binds tighter than unary minus
	[InlineData("2*3^2",         "18")]     // and looser than `*`
	public void And_reckons_in_decimal(string expression, string expected) =>
		Assert.Equal(expected, Decimal(expression));

	[Fact]
	public void And_says_where_a_decimal_expression_stops_being_one()
	{
		Assert.Equal("1/8 = 0.125", DecimalCalculator.Explain("1/8"));
		Assert.StartsWith("Expected ['-' | '(' | '0'..'9'].", DecimalCalculator.Explain("1/"));
	}

	[Fact]
	public void And_a_number_is_where_a_space_still_means_something()
	{
		// `Number` is declared in a namespace that shadows trivia with `none`, so the spaces
		// this grammar ignores everywhere else are not ignored between digits.
		Assert.Equal("1.5", Decimal(" 1.5 "));
		Assert.Throws<FormatException>(static () => DecimalCalculator.Evaluate("1 . 5"));
	}

	// ── The calculator of one rule ───────────────────────────────────────────────

	static string Strength(string expression) =>
		StrengthCalculator.Evaluate(expression).ToString(CultureInfo.InvariantCulture);

	[Theory]
	[InlineData("1-2-3",         "-4")]     // `<< 1` reads its right operand at 2 → groups left
	[InlineData("2^3^2",        "512")]     // `>> 3` reads it at 3 → groups right
	[InlineData("-1-2",          "-3")]
	[InlineData("1/8",        "0.125")]
	[InlineData(" 1 + 2 * 3 ",    "7")]
	public void One_rule_holds_a_whole_expression_language(string expression, string expected) =>
		Assert.Equal(expected, Strength(expression));

	[Fact]
	public void And_says_where_a_one_rule_expression_stops_being_one()
	{
		Assert.Equal("-1-2 = -3", StrengthCalculator.Explain("-1-2"));

		// A strength refuses nothing by itself: what fails here is that `+` needs a right
		// operand, and the position named is where one stopped being available.
		Assert.StartsWith("Expected ['-' | '(' | '0'..'9'].", StrengthCalculator.Explain("1+"));
	}

	[Theory]
	[MemberData(nameof(ExampleTests.Expressions), MemberType = typeof(ExampleTests))]
	public void One_rule_of_strengths_means_what_the_five_rules_of_levels_mean(string expression) =>
		// The two calculators, expression by expression. The last pair is the one that
		// looked as though it could not translate: levels say the asymmetry by naming two
		// different rules either side of `^`, and a strength is one number.
		//
		// One number says both, because a strength is not symmetric to begin with — it is
		// the strength the operand to the *right* is read at, and nothing else. A prefix
		// has no left operand, so it is a base, and a base is entered whatever strength was
		// asked for: there is nothing to its left for anything to bind more tightly than.
		// So `^` and unary minus at the same 3 gives -(2^2) on one side and 2^(-2) on the
		// other.
		Assert.Equal(Decimal(expression), Strength(expression));

	[Theory]
	[MemberData(nameof(ExampleTests.Expressions), MemberType = typeof(ExampleTests))]
	public void And_one_rule_of_strengths_builds_the_very_same_tree(string expression) =>
		// Not "the same answer" — the same tree, node for node, by record equality. Five
		// rules of levels and one rule of strengths are two ways of writing one language,
		// and this is as close as a test can get to saying so.
		Assert.Equal(ExpressionParser.Read(expression), OneRuleParser.Read(expression));

	[Fact]
	public void And_the_walks_do_not_know_which_grammar_built_it() =>
		// `Evaluate` and `Print` are on the tree and mention no parser, which is why one
		// set of them serves both grammars — and why this line says nothing about which
		// one built the node it is calling.
		Assert.Equal("((1 - 2) - -3)", OneRuleParser.Read("1-2--3").Print());

	[Theory]
	[MemberData(nameof(ExampleTests.Expressions), MemberType = typeof(ExampleTests))]
	public void And_a_walk_over_it_gets_the_same_answers_as_the_calculator(string expression) =>
		// The tree is built by a grammar of levels, like DecimalCalculatorExample and
		// unlike StrengthCalculatorExample. All three answer the same, which is the point:
		// the two conventions are two ways of saying one language, and what a `=>` builds
		// — a number or a node — is a separate question from how the grammar is written.
		Assert.Equal(
			Decimal(expression),
			ExpressionParser.Read(expression).Evaluate().ToString(CultureInfo.InvariantCulture));
}
