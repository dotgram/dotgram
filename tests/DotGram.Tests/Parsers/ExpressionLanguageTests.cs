using System;
using System.Collections.Generic;
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

	/// <summary>What could have stood here, each thing named once.</summary>
	/// <remarks>
	/// A failure keeps every expectation recorded against the furthest position, and a
	/// language this size has several sites wanting the same character — the `&lt;` that
	/// opens a type argument list is written in more than one rule. Joined as they came,
	/// the message said "Expected '&lt;' or '&lt;'.", which reads like a defect because it
	/// is one: what a reader is owed is the set of what could have stood there.
	/// Written as a property rather than against the exact text, which is a separate
	/// question and one §7.5 is still the authority on.
	/// </remarks>
	[Fact]
	public void What_could_have_stood_here_is_named_once_each()
	{
		var error = ExpressionLanguage.TryParseLambda(
			"(int x) => x + return", new ExpressionLanguage.State()).Error!;

		Assert.StartsWith("Expected ", error, StringComparison.Ordinal);

		var terms = error
			.Substring("Expected ".Length)
			.TrimEnd('.')
			.Split([", ", " or "], StringSplitOptions.None);

		Assert.Equal(terms.Length, terms.Distinct().Count());
	}

	// ── Nested initializers: the three the API has and one syntax says ───────────

	/// <summary>A collection whose `Add` takes two things, which is what `ElementInit` is for.</summary>
	/// <remarks>
	/// A list of values could not have said this: `Add` is not obliged to take one thing,
	/// and a dictionary's takes two. C# writes each call's arguments in braces of their
	/// own and so does this — the grammar reads a braced group as one element and a bare
	/// expression as an element of one.
	/// </remarks>
	[Fact]
	public void A_collection_initializer_may_call_Add_with_more_than_one_thing()
	{
		ExpressionLanguage.Using("System.Collections.Generic");

		var made = ExpressionLanguage.Compile<Func<Dictionary<int, string>>>(
			"() => new Dictionary<int, string>() { { 1, \"one\" }, { 2, \"two\" } }")();

		Assert.Equal(["one", "two"], new[] { made[1], made[2] });
	}

	[Fact]
	public void And_one_whose_Add_takes_one_is_written_without_them()
	{
		ExpressionLanguage.Using("System.Collections.Generic");

		Assert.Equal(
			[10, 20],
			ExpressionLanguage.Compile<Func<List<int>>>("() => new List<int>() { 10, 20 }")());
	}

	/// <summary>`Inner = { X = 1 }` sets what is already there rather than replacing it.</summary>
	/// <remarks>
	/// `MemberBind`, and the difference from an assignment is the whole point: no `new`
	/// stands after the `=`, so the object the member already holds is the one initialized.
	/// Which member's type the nested braces are read against is not known where they are
	/// read — it is one step further in than the name — so the settings travel as text and
	/// a value, and are bound where the member is in hand.
	/// </remarks>
	[Fact]
	public void A_member_initializer_may_nest()
	{
		ExpressionLanguage.Using("DotGram.Tests.Parsers");

		var made = ExpressionLanguage.Compile<Func<Holder>>(
			"() => new Holder() { Inner = { Count = 7 } }")();

		Assert.Equal(7, made.Inner.Count);
	}

	[Fact]
	public void And_a_nested_one_may_be_a_collection()
	{
		// `ListBind`: the list the member already holds is added to, not replaced.
		ExpressionLanguage.Using("DotGram.Tests.Parsers");

		Assert.Equal(
			[3, 4],
			ExpressionLanguage.Compile<Func<Holder>>("() => new Holder() { Items = { 3, 4 } }")().Items);
	}

	[Fact]
	public void And_the_three_forms_stand_side_by_side()
	{
		ExpressionLanguage.Using("DotGram.Tests.Parsers");

		var made = ExpressionLanguage.Compile<Func<Holder>>(
			"() => new Holder() { Name = \"a\", Inner = { Count = 1 }, Items = { 5 } }")();

		Assert.Equal("a", made.Name);
		Assert.Equal(1,   made.Inner.Count);
		Assert.Equal([5], made.Items);
	}

	[Fact]
	public void And_a_collection_with_no_such_Add_says_so()
	{
		ExpressionLanguage.Using("System.Collections.Generic");

		Assert.Contains(
			"no 'Add' taking",
			Assert.Throws<FormatException>(
				() => ExpressionLanguage.Compile<Func<List<int>>>(
					"() => new List<int>() { { 1, 2 } }")).Message);
	}

	// ── checked and unchecked: §7.8's marks, in the language they were built for ──

	/// <summary>
	/// The same arithmetic, read the same way, building two different trees.
	/// </summary>
	/// <remarks>
	/// `Additive` is one rule and has one alternative for `+`. What differs between these
	/// two is not what was read — same characters, same route — but which of
	/// <c>Expression.Add</c> and <c>Expression.AddChecked</c> the host was asked for, which
	/// it decides from the mark standing over the construction (§7.8).
	/// </remarks>
	[Fact]
	public void Overflow_wraps_where_nothing_says_otherwise() =>
		Assert.Equal(
			int.MinValue,
			ExpressionLanguage.Compile<Func<int, int>>("(int x) => x + 1")(int.MaxValue));

	[Fact]
	public void And_throws_inside_checked() =>
		Assert.Throws<OverflowException>(
			() => ExpressionLanguage.Compile<Func<int, int>>("(int x) => checked(x + 1)")(int.MaxValue));

	[Theory]
	[InlineData("(int x) => checked(x - 1)",  "Subtract")]
	[InlineData("(int x) => checked(x * 2)",  "Multiply")]
	[InlineData("(int x) => checked(-x)",     "Negate")]
	public void And_every_arithmetic_node_that_has_a_checked_form_uses_it(string text, string _) =>
		Assert.Throws<OverflowException>(
			() => ExpressionLanguage.Compile<Func<int, int>>(text)(int.MinValue));

	[Fact]
	public void And_a_cast_is_where_the_difference_shows_most() =>
		// `(byte)300` is 44 unchecked and throws checked, and neither is something the C#
		// compiler could have said anything about here: the value is not a constant until
		// the tree is compiled.
		Assert.Equal(44, ExpressionLanguage.Compile<Func<int, byte>>("(int x) => (byte)x")(300));

	[Fact]
	public void And_the_same_cast_throws_inside_checked() =>
		Assert.Throws<OverflowException>(
			() => ExpressionLanguage.Compile<Func<int, byte>>("(int x) => checked((byte)x)")(300));

	[Fact]
	public void And_a_compound_assignment_is_marked_like_the_operator_it_stands_for() =>
		Assert.Throws<OverflowException>(
			() => ExpressionLanguage.Compile<Func<int, int>>(
				"(int x) => { int a = x; return checked(a += 1); }")(int.MaxValue));

	/// <summary>The shape a pair of flags cancelling each other cannot express.</summary>
	/// <remarks>
	/// `unchecked` does not turn `checked` off — it stands over its own protraction and the
	/// outer mark is in force again after it. So the inner sum wraps and the outer one, over
	/// the same reading of the same rule, throws.
	/// </remarks>
	[Fact]
	public void A_mark_nests_rather_than_cancelling()
	{
		var wraps = ExpressionLanguage.Compile<Func<int, int>>("(int x) => unchecked(x + 1)");

		Assert.Equal(int.MinValue, wraps(int.MaxValue));

		var outer = ExpressionLanguage.Compile<Func<int, int>>(
			"(int x) => checked(unchecked(x + 1) + 1)");

		// The inner `x + 1` wrapped to int.MinValue without complaint; the outer `+ 1` is
		// under `checked` again and has nothing to overflow, so this is an ordinary answer
		// and the point is that it is one.
		Assert.Equal(int.MinValue + 1, outer(int.MaxValue));

		Assert.Throws<OverflowException>(
			() => ExpressionLanguage.Compile<Func<int, int>>(
				"(int x) => checked(unchecked(x + 0) + 1)")(int.MaxValue));
	}

	[Fact]
	public void And_a_mark_reaches_through_a_call_to_another_rule() =>
		// The mark is over `Expression`, and everything under it — a parenthesized group,
		// a whole other level of the ladder — is inside. Nothing about that is written at
		// the sites: they say what they are, and the extent does the rest.
		Assert.Throws<OverflowException>(
			() => ExpressionLanguage.Compile<Func<int, int>>("(int x) => checked((x + 0) * 2 + x)")(
				int.MaxValue));

	[Fact]
	public void And_checked_is_not_a_name() =>
		// §4.6: a keyword does not match inside a word, so a variable may be called
		// `checkedTotal` without the reading stopping after seven characters.
		Assert.Equal(
			7,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int x) => { int checkedTotal = x + 2; return checkedTotal; }")(5));

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

	// ── A block is an expression, and it holds a scope ──────────────────────────

	[Fact]
	public void A_block_is_worth_its_last_expression() =>
		// Which is what `Expression.Block` is worth, so the language says it too: no
		// `return`, no label, just the value the block ends on.
		Assert.Equal(
			25,
			ExpressionLanguage.Compile<Func<int, int, int>>(
				"(int x, int y) => { int sum = x + y; sum * sum }")(2, 3));

	[Fact]
	public void And_being_an_expression_it_stands_wherever_one_does() =>
		Assert.Equal(
			10,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int x) => { int doubled = { int half = x; half * 2 }; doubled }")(5));

	[Fact]
	public void And_it_stands_where_a_value_is_expected_rather_than_anywhere_at_all() =>
		// An initializer, a `return`, a branch, the last thing in a block. Not an operand in
		// the middle of an expression: a construct reachable both as a statement and as a
		// primary is read once as each, at every level of a nest of them, and that is what
		// made a chain of three `else if`s take 1.6 seconds.
		Assert.Equal(
			[7, 7],
			new[]
			{
				ExpressionLanguage.Compile<Func<int, int>>("(int x) => { int t = { x * 2 }; t + 1 }")(3),
				ExpressionLanguage.Compile<Func<int, int>>("(int x) => { return { x * 2 + 1 }; }")(3),
			});

	[Fact]
	public void Two_blocks_beside_each_other_may_each_declare_the_same_name() =>
		// The case a table by name gets wrong and the reason a name is looked up by where
		// it is written: these are two variables, and legal C#. The second `t` is 100, and
		// an answer of 200 would mean the first block's `t` had been overwritten.
		Assert.Equal(
			101,
			ExpressionLanguage.Compile<Func<int>>(
				"() => { int a = { int t = 1; t }; int b = { int t = 100; t }; a + b }")());

	[Fact]
	public void And_an_inner_block_shadows_the_name_around_it() =>
		// C# refuses this outright (CS0136); reading the nearer name is the more permissive
		// of the two answers and turns no valid C# into something else.
		Assert.Equal(
			2,
			ExpressionLanguage.Compile<Func<int>>("() => { int t = 1; { int t = 2; t } }")());

	[Fact]
	public void And_a_name_the_block_beside_it_declared_is_not_in_scope() =>
		Assert.Contains(
			"nothing named 't'",
			Assert.Throws<FormatException>(
				() => ExpressionLanguage.Parse("() => { int a = { int t = 1; t }; t }")).Message);

	[Fact]
	public void And_a_block_with_nothing_in_it_says_so() =>
		// Nothing else refuses a block: `Expression.Block` is worth its last expression
		// whatever that is, so `{ int a = x; }` is worth the assignment and reads. Only a
		// block with no expressions at all has nothing to be worth, and the tree has no
		// such node.
		Assert.Contains(
			"has to hold something",
			Assert.Throws<FormatException>(
				() => ExpressionLanguage.Parse("(int x) => { }")).Message);

	[Fact]
	public void A_return_still_leaves_the_whole_lambda() =>
		// The C# form, unchanged — and from a block inside a block, which is what a label
		// on the lambda rather than on a block is for.
		Assert.Equal(
			9,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int x) => { int t = x; { return t * 3; } }")(3));

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
		Assert.False(ExpressionLanguage.TryParse("(Widget w) => w").IsSuccess);

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
		Assert.False(ExpressionLanguage.TryParse("() => 1 L").IsSuccess);

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
		Assert.False(ExpressionLanguage.TryParse("() => \"abc").IsSuccess);

	[Fact]
	public void A_comparison_answers_bool() =>
		Assert.True(ExpressionLanguage.Compile<Func<int, bool>>("(int x) => x > 2 && x < 10")(5));

	[Fact]
	public void And_the_word_true_is_a_value_rather_than_a_name() =>
		// §4.6 weaves a boundary round a keyword, so `trueish` would be a name — which is
		// the whole reason this grammar declares a `wordboundary`.
		Assert.True(ExpressionLanguage.Compile<Func<int, bool>>("(int x) => true")(0));

	// ── Types by name, and everything a name in metadata unlocks ────────────────

	[Fact]
	public void A_name_is_a_type_where_the_namespaces_say_it_is() =>
		// The keywords are the grammar's, written as `typeof(int)` where C# reads them. A
		// name is the host's, because what `Exception` means is a question about which
		// namespaces to look in — and no grammar can carry that for an API it has not been
		// pointed at yet.
		Assert.Equal(
			[typeof(Exception), typeof(string), typeof(long)],
			new[] { "(object o) => o as Exception", "(object o) => o as String", "(int x) => (long)x" }
				.Select(text => ExpressionLanguage.Parse(text).Body.Type));

	[Fact]
	public void And_a_name_that_is_no_type_leaves_the_parenthesis_a_parenthesis() =>
		// The cast ambiguity C# needs a rule of its own for: `(Foo)x` is a cast where `Foo`
		// names a type and an expression where it does not, and the guard answering no is
		// what sends the parse to the other reading.
		Assert.Equal(
			[6L, 8],
			new object[]
			{
				ExpressionLanguage.Compile<Func<int, long>>("(int x) => (long)x * 2L")(3),
				ExpressionLanguage.Compile<Func<int, int>>("(int x) => (x + 1) * 2")(3),
			});

	[Fact]
	public void An_instance_member_is_read_by_the_API_own_lookup() =>
		// `Expression.PropertyOrField` answers this, and `Expression.Call` chooses the
		// overload — so almost none of it is written in the host, and what cannot be found
		// is reported in the API's own words.
		Assert.Equal(
			[3, 2, "ABC"],
			new object[]
			{
				ExpressionLanguage.Compile<Func<string, int>>("(string s) => s.Length")("abc"),
				ExpressionLanguage.Compile<Func<string, int>>("(string s) => s.IndexOf(\"c\")")("abc"),
				ExpressionLanguage.Compile<Func<string, string>>("(string s) => s.ToUpperInvariant()")("abc"),
			});

	[Fact]
	public void A_static_member_names_its_type_first() =>
		Assert.Equal(
			[7, 3.0, ""],
			new object[]
			{
				ExpressionLanguage.Compile<Func<int, int>>("(int x) => Math.Max(x, 7)")(3),
				ExpressionLanguage.Compile<Func<double>>("() => Math.Floor(3.7)")(),
				ExpressionLanguage.Compile<Func<string>>("() => String.Empty")(),
			});

	[Fact]
	public void And_a_dotted_name_is_a_type_only_as_far_as_it_resolves() =>
		// `System.Math.Max` resolves whole; `s.Length` does not resolve at all and is read
		// as a name and a member of it. The greedy name gives back a part at a time.
		Assert.Equal(7, ExpressionLanguage.Compile<Func<int, int>>("(int x) => System.Math.Max(x, 7)")(3));

	[Fact]
	public void A_constructor_is_chosen_by_what_fits_its_arguments() =>
		Assert.Equal(
			"boom",
			ExpressionLanguage.Compile<Func<string, string>>(
				"(string s) => new Exception(s).Message")("boom"));

	[Fact]
	public void An_array_is_made_by_size_or_by_what_is_in_it() =>
		Assert.Equal(
			[4, 3, 20],
			new[]
			{
				ExpressionLanguage.Compile<Func<int>>("() => new int[4].Length")(),
				ExpressionLanguage.Compile<Func<int>>("() => new int[] { 10, 20, 30 }.Length")(),
				ExpressionLanguage.Compile<Func<int>>("() => new int[] { 10, 20, 30 }[1]")(),
			});

	[Fact]
	public void And_an_index_is_the_array_node_or_the_indexer_by_what_it_reads() =>
		// Two factories and one syntax, told apart by the operand rather than by the text:
		// an array's element is a node of this tree, anything else's is a property.
		Assert.Equal(
			['b', 20],
			new object[]
			{
				ExpressionLanguage.Compile<Func<string, char>>("(string s) => s[1]")("abc"),
				ExpressionLanguage.Compile<Func<int>>("() => new int[] { 10, 20 }[1]")(),
			});

	[Theory]
	[InlineData("(object o) => o is string",  "abc", true)]
	[InlineData("(object o) => o is string",  42,    false)]
	public void Is_asks_the_type_and_as_answers_null(string text, object argument, bool expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<object, bool>>(text)(argument));

	[Fact]
	public void And_as_answers_null_where_it_is_not_that() =>
		Assert.Null(ExpressionLanguage.Compile<Func<object, string>>("(object o) => o as string")(42));

	[Fact]
	public void A_generic_type_is_named_with_what_it_is_over() =>
		// Metadata names it by arity — `Func`2` — which is one more thing about the runtime
		// rather than about the language, and so is the host's. The grammar reads a name, a
		// `<`, some types and a `>`, which is what every language calls a generic type.
		Assert.Equal(
			8,
			ExpressionLanguage.Compile<Func<Func<int, int>, int, int>>(
				"(Func<int, int> f, int x) => f(x) * 2")(n => n + 3, 1));

	[Fact]
	public void And_a_type_may_be_an_array_of_one() =>
		Assert.Equal(
			[3, 20],
			new[]
			{
				ExpressionLanguage.Compile<Func<int[], int>>("(int[] a) => a.Length")([1, 2, 3]),
				ExpressionLanguage.Compile<Func<int[], int>>("(int[] a) => a[1] * 10")([1, 2, 3]),
			});

	[Fact]
	public void An_initializer_sets_what_it_names() =>
		// Which member a name is cannot be known where the name is read — the type is a
		// sibling of the braces rather than something above them — so the pair travels as
		// text and a value, and the member is found where the type is in hand.
		Assert.Equal(
			"boom",
			ExpressionLanguage.Compile<Func<string, string>>(
				"(string s) => new Exception() { Source = s }.Source")("boom"));

	[Fact]
	public void And_a_collection_initializer_adds_what_it_lists()
	{
		// `List` is in a namespace the host was not told about, which is what a `using` is
		// for — and the host is where it belongs, because a grammar cannot carry one for an
		// API it has not been pointed at yet.
		ExpressionLanguage.Using("System.Collections.Generic");

		Assert.Equal(
			3,
			ExpressionLanguage.Compile<Func<int>>("() => new List<int>() { 10, 20, 30 }.Count")());
	}

	[Fact]
	public void A_member_and_an_element_may_be_written_to() =>
		// The API keeps reading an element apart from writing one — `ArrayIndex` answers
		// with a value and `ArrayAccess` with the element — and which is meant is decided by
		// which side of the `=` it stands on, which the grammar knows and the API cannot.
		Assert.Equal(
			[7, "set"],
			new object[]
			{
				ExpressionLanguage.Compile<Func<int[], int>>("(int[] a) => { a[1] = 7; a[1] }")([1, 2, 3]),
				ExpressionLanguage.Compile<Func<string>>(
					"() => { Exception e = new Exception(); e.Source = \"set\"; e.Source }")(),
			});

	[Fact]
	public void And_a_compound_assignment_writes_to_a_name_or_a_member() =>
		// Not to an element, and that is a measurement rather than a taste: an index is an
		// expression, and eleven alternatives reading it eleven times before finding out
		// which operator they are made `a[a[a[a[0]]]] = 1` take most of a second.
		Assert.False(ExpressionLanguage.TryParse("(int[] a) => { a[1] += 7; a[1] }").IsSuccess);

	// ── try, catch, finally, throw ──────────────────────────────────────────────

	[Theory]
	[InlineData(2, 5)]
	[InlineData(0, -1)]
	public void A_try_catches_what_the_runtime_throws(int argument, int expected) =>
		Assert.Equal(
			expected,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int n) => { int r = 0; try { r = 10 / n; } catch (DivideByZeroException e) { r = -1; } r }")
				(argument));

	[Fact]
	public void And_a_finally_runs_either_way() =>
		// Three shapes and three factories, and the grammar says which by what is written.
		// A `finally` with no `catch` does not swallow what it runs after, so the last of
		// these leaves by the exception with its `r += 1` already done.
		Assert.Equal(
			[11, 0, 6],
			new[]
			{
				ExpressionLanguage.Compile<Func<int, int>>(
					"(int n) => { int r = 0; try { r = 10 / n; } catch (Exception e) { r = -1; } finally { r += 1; } r }")(1),
				ExpressionLanguage.Compile<Func<int, int>>(
					"(int n) => { int r = 0; try { r = 10 / n; } catch (Exception e) { r = -1; } finally { r += 1; } r }")(0),
				ExpressionLanguage.Compile<Func<int, int>>(
					"(int n) => { int r = 0; try { r = 10 / n; } finally { r += 1; } r }")(2),
			});

	[Fact]
	public void And_the_caught_variable_belongs_to_its_handler() =>
		Assert.Equal(
			"Attempted to divide by zero.",
			ExpressionLanguage.Compile<Func<string>>(
				"() => { string m = \"\"; int z = 0; try { m = (10 / z).ToString(); } catch (Exception e) { m = e.Message; } m }")());

	[Fact]
	public void And_a_throw_is_a_statement_that_never_comes_back() =>
		Assert.Equal(
			"no",
			Assert.Throws<InvalidOperationException>(
				() => ExpressionLanguage.Compile<Func<int, int>>(
					"(int n) => { if (n < 0) throw new InvalidOperationException(\"no\"); n }")(-1))
				.Message);

	// ── Statements ──────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("(int x) => { int a = 0; if (x > 0) a = 1; else a = 2; a }",  3, 1)]
	[InlineData("(int x) => { int a = 0; if (x > 0) a = 1; else a = 2; a }", -3, 2)]
	[InlineData("(int x) => { int a = 5; if (x > 0) a = 1; a }",             -3, 5)]
	public void An_if_reads_as_a_statement(string text, int argument, int expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<int, int>>(text)(argument));

	[Fact]
	public void And_with_an_else_it_is_worth_what_its_branches_are() =>
		// `Expression.Condition` with both branches the same type is a value, so the `if`
		// is one — which `?:` cannot stand in for, because a branch of `?:` is an
		// expression and this one takes a statement, blocks and declarations included.
		Assert.Equal(
			[1, 12],
			new[]
			{
				ExpressionLanguage.Compile<Func<int, int>>("(int x) => if (x > 0) 1 else -1")(3),
				ExpressionLanguage.Compile<Func<int, int>>(
					"(int x) => { int n = if (x > 0) { int t = x; t * 4 } else 0; n }")(3),
			});

	[Theory]
	[InlineData( 3, 1)]
	[InlineData(-3, 0)]
	public void And_branches_worth_different_things_make_it_worth_nothing(int argument, int expected) =>
		// `Expression.Condition` is one factory with two answers, and which one an `if` meant
		// is a question about this API rather than about the language — so the host answers
		// it and the grammar stays the shape every language writes. Here the branches have
		// no type in common, one of them being a `return`, so the `if` is worth nothing and
		// the block is worth what follows it.
		Assert.Equal(
			expected,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int x) => { int a = 0; if (x > 0) a = 1; else { return 0; } a }")(argument));

	[Fact]
	public void A_while_loop_runs_until_its_test_says_otherwise() =>
		Assert.Equal(
			120,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int n) => { int i = 0; int f = 1; while (i < n) { i++; f *= i; } f }")(5));

	[Fact]
	public void A_do_loop_runs_once_before_it_asks() =>
		Assert.Equal(
			[6, 1],
			new[]
			{
				ExpressionLanguage.Compile<Func<int, int>>(
					"(int n) => { int i = 0; int c = 0; do { i++; c += i; } while (i < n); c }")(3),
				ExpressionLanguage.Compile<Func<int, int>>(
					"(int n) => { int i = 0; int c = 0; do { i++; c += i; } while (i < n); c }")(0),
			});

	[Fact]
	public void A_for_loop_holds_its_own_variable() =>
		// The initializer's `i` belongs to the loop and not to what is around it, which is
		// the same scope machinery a block uses — a `for` records an extent of its own.
		Assert.Equal(
			10,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { sum += i; } sum }")(5));

	[Fact]
	public void And_a_name_the_loop_declared_is_not_in_scope_after_it() =>
		Assert.Contains(
			"nothing named 'i'",
			Assert.Throws<FormatException>(
				() => ExpressionLanguage.Parse("(int n) => { for (int i = 0; i < n; i++) { n += 1; } i }"))
				.Message);

	[Fact]
	public void Break_and_continue_name_the_loop_they_are_written_in() =>
		Assert.Equal(
			25,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { if (i % 2 == 0) continue; sum += i; } sum }")(10));

	[Fact]
	public void And_a_break_leaves_the_innermost_one() =>
		Assert.Equal(
			3,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int n) => { int c = 0; for (int i = 0; i < n; i++) { for (int j = 0; j < n; j++) { if (j == 1) break; c++; } } c }")(3));

	[Fact]
	public void And_a_break_outside_every_loop_is_refused() =>
		Assert.Contains(
			"inside no loop and no switch",
			Assert.Throws<FormatException>(
				() => ExpressionLanguage.Parse("(int x) => { break; x }")).Message);

	[Theory]
	[InlineData(1, 10)]
	[InlineData(2, 20)]
	[InlineData(9, -1)]
	public void A_switch_chooses_by_value(int argument, int expected) =>
		Assert.Equal(
			expected,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int n) => { int r = 0; switch (n) { case 1: r = 10; break; case 2: r = 20; break; default: r = -1; } r }")
				(argument));

	[Fact]
	public void And_a_break_in_a_case_leaves_the_switch_and_not_the_loop() =>
		// C#'s rule, and the reason a switch records an extent of its own: were the jump to
		// name the loop, this would stop at 2 and answer 1 rather than 8.
		Assert.Equal(
			8,
			ExpressionLanguage.Compile<Func<int, int>>(
				"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { switch (i) { case 2: break; default: sum += i; } } sum }")(5));

	// ── Assignment, which is where a statement gets its work done ───────────────

	[Theory]
	[InlineData("(int x) => { int a = x; a += 5; a *= 2; a }",  1, 12)]
	[InlineData("(int x) => { int a = x; a -= 1; a }",          5,  4)]
	[InlineData("(int x) => { int a = x; a /= 2; a }",          9,  4)]
	[InlineData("(int x) => { int a = x; a %= 3; a }",          8,  2)]
	[InlineData("(int x) => { int a = x; a <<= 2; a }",         3, 12)]
	[InlineData("(int x) => { int a = x; a >>= 1; a }",         8,  4)]
	[InlineData("(int x) => { int a = x; a &= 6; a }",          3,  2)]
	[InlineData("(int x) => { int a = x; a |= 4; a }",          3,  7)]
	[InlineData("(int x) => { int a = x; a ^= 1; a }",          3,  2)]
	public void Every_compound_assignment_C_sharp_writes_is_here(string text, int argument, int expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<int, int>>(text)(argument));

	[Fact]
	public void And_an_assignment_is_worth_what_it_assigned() =>
		// Which is C#'s rule and the API's both, and is what makes `a = b = c` read.
		Assert.Equal(
			7,
			ExpressionLanguage.Compile<Func<int>>("() => { int a = 0; int b = 0; a = b = 7; a }")());

	[Theory]
	[InlineData("(int x) => { int a = x; int b = a++; b * 10 + a }", 1, 12)]
	[InlineData("(int x) => { int a = x; int b = ++a; b * 10 + a }", 1, 22)]
	[InlineData("(int x) => { int a = x; int b = a--; b * 10 + a }", 1, 10)]
	[InlineData("(int x) => { int a = x; int b = --a; b * 10 + a }", 1,  0)]
	public void And_increment_says_which_value_it_is_worth_by_where_it_stands(
		string text, int argument, int expected) =>
		Assert.Equal(expected, ExpressionLanguage.Compile<Func<int, int>>(text)(argument));

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
		var match = ExpressionLanguage.TryParse(text);

		Assert.False(match.IsSuccess);
		Assert.NotNull(match.Error);
	}
}

/// <summary>What the nested-initializer tests are written against.</summary>
/// <remarks>
/// Top level rather than nested in the test class, because the parser resolves a name
/// against namespaces the way a `using` does and a nested type is not reachable that way.
/// Its two initializable members are get-only and already populated, which is what tells
/// `MemberBind` and `ListBind` from an assignment: the object is the one already there.
/// </remarks>
public sealed class Holder
{
	public string    Name  { get; set; } = "";
	public Counter   Inner { get; }      = new();
	public List<int> Items { get; }      = [];
}

public sealed class Counter
{
	public int Count { get; set; }
}
