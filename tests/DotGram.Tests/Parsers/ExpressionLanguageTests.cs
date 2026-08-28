using System;
using System.Linq.Expressions;

using DotGram.Parsers;

using Xunit;

namespace DotGram.Tests.Parsers;

/// <summary>
/// The expression language of <c>DotGram.Parsers</c>, read and run.
/// </summary>
/// <remarks>
/// A parser that ships is held to what it does, not to what it emits: every test here
/// compiles the tree and calls it, because a lambda that builds and answers wrongly is
/// the failure a snapshot cannot see.
/// </remarks>
public sealed class ExpressionLanguageTests
{
	[Theory]
	[InlineData("(int x) => x",                 3, 3)]
	[InlineData("(int x) => x + 1",             3, 4)]
	[InlineData("(int x) => x * x - 1",         3, 8)]
	[InlineData("(int x) => -x",                3, -3)]
	[InlineData("(int x) => (x + 1) * 2",       3, 8)]
	[InlineData("(int x) => x % 2",             3, 1)]
	public void An_expression_body_reads_and_runs(string text, int argument, int expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<int, int>>(text)(argument));

	[Fact]
	public void Precedence_is_C_sharp_precedence() =>
		// One rule per level (§4.3's default), so `*` binds tighter than `+` and the
		// grammar says which by which rule calls which.
		Assert.Equal(7, ExpressionLanguage.Compile<Func<int, int>>("(int x) => 1 + x * 2")(3));

	[Fact]
	public void And_the_same_operator_groups_to_the_left() =>
		// Left recursion is where the associativity is: `10 - 3 - 2` is `(10 - 3) - 2`,
		// which is 5 and not 9.
		Assert.Equal(5, ExpressionLanguage.Compile<Func<int, int>>("(int x) => 10 - 3 - x")(2));

	// ── Blocks: locals, and a return that is a jump ─────────────────────────────

	[Fact]
	public void A_block_declares_locals_and_returns() =>
		Assert.Equal(
			25,
			ExpressionLanguage.Compile<Func<int, int, int>>(
				"(int x, int y) => { int sum = x + y; return sum * sum; }")(2, 3));

	[Fact]
	public void And_a_local_may_be_read_by_the_next_one() =>
		Assert.Equal(
			12,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int x) => { int doubled = x * 2; int more = doubled + 2; return more; }")(5));

	[Fact]
	public void And_a_local_says_its_type_because_the_API_asks_where_it_is_read() =>
		// `Expression.Variable` wants a type at the declaration, and the initializer is
		// not built until long after — so the language says `int sum`, not `var sum`.
		// The grammar shaped to the API, which is what wiring one up looks like.
		Assert.Equal(
			2.5,
			ExpressionLanguage.Compile<Func<int, double>>(
				"(int x) => { double half = x / 2.0; return half; }")(5));

	[Fact]
	public void And_a_name_nothing_declares_is_refused_by_the_binder() =>
		// The grammar reads it — a name is a word — and the binder is what knows there is
		// no such variable, which is where docs/syntax.md §7 puts a question about scope.
		Assert.Contains(
			"nothing named 'y'",
			Assert.Throws<FormatException>(
				() => ExpressionLanguage.Parse("(int x) => x + y")).Message);

	[Fact]
	public void And_a_type_the_language_does_not_have_is_not_a_declaration() =>
		// The guard answers rather than throws — it runs during the match, on readings
		// the parse may abandon — so a word that is not a type simply does not read as a
		// declaration, and the text is refused as text.
		Assert.False(ExpressionLanguage.TryParseLambda("(Widget w) => w").IsSuccess);

	// ── Types, and what mixing them means ───────────────────────────────────────

	[Fact]
	public void An_int_and_a_double_meet_as_a_double() =>
		Assert.Equal(3.5, ExpressionLanguage.Compile<Func<int, double>>("(int x) => x + 1.5")(2));

	[Fact]
	public void A_comparison_answers_bool() =>
		Assert.True(ExpressionLanguage.Compile<Func<int, bool>>("(int x) => x > 2 && x < 10")(5));

	[Fact]
	public void And_the_word_true_is_a_value_rather_than_a_name() =>
		// §4.6 weaves a boundary round a keyword, so `trueish` would be a name — which is
		// the whole reason this grammar declares a `wordboundary`.
		Assert.True(ExpressionLanguage.Compile<Func<int, bool>>("(int x) => true")(0));

	// ── What it refuses to read at all ──────────────────────────────────────────

	[Theory]
	[InlineData("(int x) => ")]
	[InlineData("(int x) => x +")]
	[InlineData("(int x) => { return x }")]
	[InlineData("int x => x")]
	public void A_text_that_is_not_this_language_is_refused_with_a_position(string text)
	{
		var match = ExpressionLanguage.TryParseLambda(text);

		Assert.False(match.IsSuccess);
		Assert.NotNull(match.Error);
	}
}
