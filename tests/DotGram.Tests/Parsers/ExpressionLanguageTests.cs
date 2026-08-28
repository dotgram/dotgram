using System;
using System.Linq;
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
		// not built until long after — so the language says `double half`, not `var half`.
		// The grammar shaped to the API, which is what wiring one up looks like.
		Assert.Equal(
			2.5,
			ExpressionLanguage.Compile<Func<double, double>>(
				"(double x) => { double half = x / 2.0; return half; }")(5));

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
	public void Nothing_widens_on_its_own_and_the_API_is_what_says_so() =>
		// The language holds no opinion the API does not hold. Adding an `int` to a
		// `double` is refused by `Expression.Add` itself, in its own words — which is
		// better than a message this could invent, and is the whole reason every `=>` in
		// the grammar names a factory instead of dispatching on the operator's text.
		Assert.Contains(
			"not defined for the types",
			Assert.Throws<InvalidOperationException>(
				() => ExpressionLanguage.Parse("(int x) => x + 1.5")).Message);

	[Fact]
	public void And_two_doubles_add_as_doubles() =>
		Assert.Equal(3.5, ExpressionLanguage.Compile<Func<double, double>>("(double x) => x + 1.5")(2));

	// ── Constants say their type the way C# does ────────────────────────────────

	[Fact]
	public void A_suffix_says_which_type_a_constant_is() =>
		Assert.Equal(
			[typeof(int), typeof(long), typeof(decimal), typeof(double), typeof(double), typeof(decimal)],
			new[] { "1", "1L", "1m", "1d", "1.5", "1.5m" }
				.Select(text => ExpressionLanguage.Parse($"() => {text}").Body.Type));

	[Theory]
	[InlineData("(long x) => x + 1L",        3L,    4L)]
	[InlineData("(long x) => x * 2L",        3L,    6L)]
	public void And_a_suffixed_constant_computes_in_its_own_type(string text, long argument, long expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<long, long>>(text)(argument));

	[Fact]
	public void And_a_decimal_stays_a_decimal() =>
		Assert.Equal(
			7.5m,
			ExpressionLanguage.Compile<Func<decimal, decimal>>("(decimal x) => x * 1.5m")(5m));

	[Fact]
	public void And_the_suffix_belongs_to_the_number_rather_than_standing_beside_it() =>
		// Lexical, so nothing may come between: `1 L` is a constant and then a name, and
		// a lambda made of those two is not this language.
		Assert.False(ExpressionLanguage.TryParseLambda("() => 1 L").IsSuccess);

	[Fact]
	public void And_a_name_may_still_begin_with_a_suffix_letter() =>
		// The suffix is a character set rather than a word literal, so it carries no
		// §4.6 boundary of its own — and `m` and `L` remain perfectly good names.
		Assert.Equal(4, ExpressionLanguage.Compile<Func<int, int>>("(int m) => m + 1")(3));

	// ── Bases, text and characters ──────────────────────────────────────────────

	[Theory]
	[InlineData("0x1F",   31)]
	[InlineData("0xff",   255)]
	[InlineData("0B1010", 10)]
	[InlineData("0b1",    1)]
	public void A_base_is_a_prefix_and_the_digits_are_its_own(string text, int expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<int>>($"() => {text}")());

	[Fact]
	public void And_a_base_takes_the_long_suffix_too() =>
		Assert.Equal(255L, ExpressionLanguage.Compile<Func<long>>("() => 0xFFL")());

	[Theory]
	[InlineData("\"\"",          "")]
	[InlineData("\"abc\"",       "abc")]
	[InlineData("\"a b\"",       "a b")]
	[InlineData("\"a\\tb\"",     "a\tb")]
	[InlineData("\"a\\\\b\"",    "a\\b")]
	[InlineData("\"say \\\"x\\\"\"", "say \"x\"")]
	[InlineData("\"\\u0041\"",   "A")]
	public void A_string_is_its_parts_joined(string text, string expected) =>
		// An escape is an alternative of the grammar naming the character it stands for,
		// and a run that needs none is one part — so the decoding is the grammar's, and
		// `string.Concat` is what puts them back together.
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<string>>($"() => {text}")());

	[Theory]
	[InlineData("'a'",      'a')]
	[InlineData("'\\n'",    '\n')]
	[InlineData("'\\''",    '\'')]
	[InlineData("'\\\\'",   '\\')]
	[InlineData("'\\u0041'", 'A')]
	public void A_character_is_one_part_of_the_same_kind(string text, char expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<char>>($"() => {text}")());

	[Fact]
	public void And_text_compares_as_text() =>
		Assert.True(ExpressionLanguage.Compile<Func<string, bool>>("(string s) => s == \"yes\"")("yes"));

	[Fact]
	public void And_an_unterminated_string_is_not_this_language() =>
		Assert.False(ExpressionLanguage.TryParseLambda("() => \"abc").IsSuccess);

	[Fact]
	public void A_comparison_answers_bool() =>
		Assert.True(ExpressionLanguage.Compile<Func<int, bool>>("(int x) => x > 2 && x < 10")(5));

	[Fact]
	public void And_the_word_true_is_a_value_rather_than_a_name() =>
		// §4.6 weaves a boundary round a keyword, so `trueish` would be a name — which is
		// the whole reason this grammar declares a `wordboundary`.
		Assert.True(ExpressionLanguage.Compile<Func<int, bool>>("(int x) => true")(0));

	// ── The rest of C#'s ladder ─────────────────────────────────────────────────

	[Theory]
	[InlineData("(int x) => x & 6",   3,  2)]
	[InlineData("(int x) => x | 4",   3,  7)]
	[InlineData("(int x) => x ^ 1",   3,  2)]
	[InlineData("(int x) => ~x",      3, -4)]
	[InlineData("(int x) => x << 2",  3, 12)]
	[InlineData("(int x) => x >> 1",  6,  3)]
	[InlineData("(int x) => +x",      3,  3)]
	public void The_bitwise_operators_and_the_shifts_are_there(string text, int argument, int expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<int, int>>(text)(argument));

	[Theory]
	[InlineData("() => 1 | 2 ^ 3 & 4",  3)]    // 1 | (2 ^ (3 & 4))
	[InlineData("() => 1 << 2 + 3",    32)]    // 1 << (2 + 3): additive binds tighter
	[InlineData("() => 6 >> 1 + 1",     1)]    // 6 >> 2
	[InlineData("() => 1 + 2 << 1",     6)]    // (1 + 2) << 1
	public void And_they_sit_where_C_sharp_puts_them(string text, int expected) =>
		// The ladder is the point, not the operators: each level is a rule that calls the
		// next, so the order in the file is the order in the spec and reads as one.
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<int>>(text)());

	[Fact]
	public void And_a_single_character_operator_does_not_eat_half_of_a_double_one() =>
		// `?!'|'` is what keeps `||` out of `|`'s reach — and `1 | 0` still reads as the
		// bitwise one, which is the half that would be lost by refusing `|` outright.
		Assert.Equal(
			[false, true, 1],
			new object[]
			{
				ExpressionLanguage.Compile<Func<bool>>("() => false || false")(),
				ExpressionLanguage.Compile<Func<bool>>("() => false || true")(),
				ExpressionLanguage.Compile<Func<int>>("() => 1 | 0")(),
			});

	[Fact]
	public void And_a_shift_is_told_from_a_comparison_the_same_way() =>
		Assert.True(ExpressionLanguage.Compile<Func<int, bool>>("(int x) => x >> 1 > 2")(6));

	[Theory]
	[InlineData("(int x) => x > 2 ? 1 : 0",           3, 1)]
	[InlineData("(int x) => x > 2 ? 1 : 0",           1, 0)]
	[InlineData("(int x) => x > 2 ? 1 : x > 0 ? 2 : 3", 1, 2)]   // groups to the right
	public void A_conditional_chooses_and_groups_to_the_right(string text, int argument, int expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<int, int>>(text)(argument));

	[Theory]
	[InlineData("yes",  "yes")]
	[InlineData(null,   "none")]
	public void And_a_coalesce_answers_the_left_where_it_has_one(string? argument, string expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<string?, string>>("(string s) => s ?? \"none\"")(argument));

	[Fact]
	public void And_null_is_a_word_like_any_other() =>
		Assert.True(ExpressionLanguage.Compile<Func<string?, bool>>("(string s) => s == null")(null));

	[Fact]
	public void A_cast_is_a_type_in_parentheses_and_a_parenthesis_is_still_a_parenthesis() =>
		// The two readings differ by what stands inside: a type is a keyword, and a
		// keyword is no name — so no rule of the C# kind is needed to choose.
		Assert.Equal(
			[6L, 8],
			new object[]
			{
				ExpressionLanguage.Compile<Func<int, long>>("(int x) => (long)x * 2L")(3),
				ExpressionLanguage.Compile<Func<int, int>>("(int x) => (x + 1) * 2")(3),
			});

	[Fact]
	public void And_a_cast_is_the_only_place_a_conversion_comes_from() =>
		// Nothing widens on its own, so `x + 1.5` over an `int` is refused — and the cast
		// is how the author says they meant it.
		Assert.Equal(4.5, ExpressionLanguage.Compile<Func<int, double>>("(int x) => (double)x + 1.5")(3));

	// ── The literal forms, down to the ones that are easy to forget ─────────────

	[Fact]
	public void A_separator_stands_between_digits_and_is_no_part_of_the_value() =>
		Assert.Equal(
			[1000000, 0xFFFF, 0b1010],
			new[]
			{
				ExpressionLanguage.Compile<Func<int>>("() => 1_000_000")(),
				ExpressionLanguage.Compile<Func<int>>("() => 0xFF_FF")(),
				ExpressionLanguage.Compile<Func<int>>("() => 0b1_010")(),
			});

	[Fact]
	public void An_exponent_and_a_leading_point_are_reals_too() =>
		Assert.Equal(
			[1500.0, 0.5, 0.0015],
			new[]
			{
				ExpressionLanguage.Compile<Func<double>>("() => 1.5e3")(),
				ExpressionLanguage.Compile<Func<double>>("() => .5")(),
				ExpressionLanguage.Compile<Func<double>>("() => 1.5E-3")(),
			});

	[Fact]
	public void And_every_suffix_C_sharp_writes_says_its_type() =>
		Assert.Equal(
			[
				typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(ulong),
				typeof(float), typeof(double), typeof(decimal), typeof(long),
			],
			new[] { "1", "1u", "1L", "1UL", "1lu", "1f", "1d", "1m", "0xFFL" }
				.Select(text => ExpressionLanguage.Parse($"() => {text}").Body.Type));

	[Fact]
	public void And_an_integer_too_wide_for_an_int_is_a_long_as_it_is_in_C_sharp() =>
		// Two readings of the same digits rather than a helper: `int.TryParse` is asked
		// while the text is read, and the alternative it turns down is the `long` one.
		Assert.Equal(
			[typeof(int), typeof(long)],
			new[] { "2147483647", "2147483648" }
				.Select(text => ExpressionLanguage.Parse($"() => {text}").Body.Type));

	[Theory]
	[InlineData("\"\\x41\"",         "A")]
	[InlineData("\"\\U00000041\"",   "A")]
	[InlineData("\"a\\vb\"",         "a\vb")]
	[InlineData("@\"a\\b\"",         "a\\b")]
	[InlineData("@\"say \"\"x\"\"\"", "say \"x\"")]
	public void And_the_escapes_and_the_verbatim_form_read_as_C_sharp_reads_them(string text, string expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<string>>($"() => {text}")());

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
