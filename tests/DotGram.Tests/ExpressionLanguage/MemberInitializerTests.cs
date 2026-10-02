using System;
using System.Linq.Expressions;

using DotGram.ExpressionLanguage;
using DotGram.Handwritten;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Braces that begin with a name and <c>=</c> hold member initializers and nothing else, which is
/// C#'s rule: it decides on those two tokens, and so does this language.
/// </summary>
public sealed class MemberInitializerTests
{
	/// <summary>
	/// A level of nested member initializers is read once by the generated parser and once by the
	/// hand-written one, accepted or refused.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The generated parser read <c>{ Next = …</c> as members first and, where that failed, again as
	/// elements — an element is an expression, and <c>Next = …</c> is an assignment where a variable
	/// of that name is in scope. Each level refused twice what the level inside it refused, and
	/// seventeen of them never closed took three quarters of a second. The hand-written parser read
	/// accepted members twice, quietly and then building, and nested that read inside itself.
	/// </para>
	/// <para>
	/// Counted by the places a name is looked up at (<c>State.Places</c>), which both readers share,
	/// and which every level here adds one to by reading <c>Last = Next</c>. The first entry of a
	/// level is a member that an element would read as well, an assignment to the parameter, so
	/// that the elements' reading goes on into the next level rather than stopping at a name; the
	/// level inside is the second entry's, <c>Last</c>, since a member set twice is refused (CS1912). Four depths a doubling
	/// apart, and judged as <see cref="ExpressionBlowUpTests"/> judges a nesting: by how the
	/// increments grow, so that what every reading costs whatever its depth cannot read as a slope.
	/// Linear is 1; the reading twice a level reads 8 at the last doubling, and the quiet reading
	/// inside the building one reads 2.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("a new object's members",           "",                  "new Box { Next = new Box { Last = Next }, Last = ", "Next",        " }", "",   true)]
	[InlineData("a new object's members, unclosed", "",                  "new Box { Next = new Box { Last = Next }, Last = ", "Next",        "",   "",   false)]
	[InlineData("a member's own members",           "new Box { Next = ", "{ Last = Next, Next = ",                            "Next",        " }", " }", true)]
	[InlineData("a member's own members, unclosed", "new Box { Next = ", "{ Last = Next, Next = ",                            "Next",        "",   "",   false)]
	[InlineData("elements, then a member",          "",                  "new List<object> { Next, ",                         "Next = null", " }", "",   false)]
	[InlineData("elements, then a member, unclosed", "",                 "new List<object> { Next, ",                         "Next = null", "",   "",   false)]
	[InlineData("an element assigned to",            "",                 "new List<object> { Next, ",                         "Next.Next = null", " }", "", false)]
	[InlineData("an indexer's own members, unclosed", "",                "new Box { [Next] = { Last = Next, [Next] = ",       "Next",        "",   "",   false)]
	public void Nested_member_initializers_are_read_once_a_level(
		string what, string head, string opener, string middle, string closer, string tail, bool accepted)
	{
		var generated = new long[Depths.Length];
		var hand      = new long[Depths.Length];

		for (var at = 0; at < Depths.Length; at++)
		{
			var text = Text + head + Repeated(opener, Depths[at]) + middle + Repeated(closer, Depths[at]) + tail;

			generated[at] = Places(text, ExpressionParser.TryParseLambda, accepted);
			hand[at]      = Places(text, HandExpression.TryParseLambda,   accepted);
		}

		Assert.True(
			Linearly(generated) && Linearly(hand),
			$"{what}: the generated parser looked at {string.Join(", ", generated)} places and the hand-written " +
			$"one at {string.Join(", ", hand)}, at {string.Join(", ", Depths)} levels. A level is to cost what " +
			"the one before it did, not what everything inside it did.");
	}

	/// <summary>
	/// An element after a member initializer is refused, and said to be: C# refuses it too
	/// (CS0747), since the braces were settled as members by their first two tokens.
	/// </summary>
	[Theory]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { x = 1, 2 }",  "2 }")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int>() { x = 1, x }", "x }")]
	[InlineData(Text + "new Box { Next = null, Next }",                                     "Next }")]
	[InlineData(Text + "new Box { Next = { Next = null, Next } }",                          "Next } }")]
	// An indexer's entry settles the braces as members too.
	[InlineData("using System.Collections.Generic; (int x) => new Dictionary<int, int> { [0] = 1, x }", "x }")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { [0] = 1, 2 }",            "2 }")]
	public void An_element_after_a_member_initializer_is_refused(string text, string element)
	{
		var match = Both.TryParse(text, typeof(Box).Assembly);

		Assert.Equal(ExpressionParser.Outcome.NoMatch, match.Outcome);
		Assert.Equal(text.LastIndexOf(element, StringComparison.Ordinal), match.Position);
		Assert.Equal(
			"An initializer that sets members cannot add elements as well; every entry in it has to be 'Name = ...' or '[index] = ...'.",
			match.Error);
	}

	/// <summary>Cut short where the element could still turn out to be a member, the text is only short.</summary>
	[Theory]
	[InlineData(Text + "new Box { Next = null, Next")]
	[InlineData(Text + "new Box { Next = null, ")]
	[InlineData(Text + "new Box { Next = null, [")]
	[InlineData(Text + "new Box { [Next] = ")]
	public void But_not_before_the_element_is_one(string text)
	{
		var match = Both.TryParse(text, typeof(Box).Assembly);

		Assert.Equal(ExpressionParser.Outcome.Starved, match.Outcome);
	}

	/// <summary>
	/// A text cut short after a member's <c>=</c> wants an expression, and says so as it did
	/// before the braces were settled on their first two tokens.
	/// </summary>
	/// <remarks>
	/// What stands after the <c>=</c> may be a value, a member initializer or a collection one, and
	/// all three begin with what an expression or a brace begins with. The parser's own list of
	/// what could have stood there names the lexer's token kinds one by one (<c>RawInterpolated3</c>,
	/// <c>Decimals</c>, ...), which says nothing to whoever wrote the text.
	/// </remarks>
	[Theory]
	[InlineData(Text + "new Box { Next =")]
	[InlineData(Text + "new Box() { Next = ")]
	[InlineData(Text + "new Box { Next = null, Last =")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { x =")]
	public void A_value_cut_short_after_a_member_wants_an_expression(string text)
	{
		var match = Both.TryParse(text, typeof(Box).Assembly);

		Assert.Equal(ExpressionParser.Outcome.Starved, match.Outcome);
		Assert.Equal("Expected an expression.", match.Error);
	}

	/// <summary>
	/// A comma after the last member adds no element, so it is not refused as one: C# accepts
	/// <c>new Box { Next = null, }</c>, and so does this language.
	/// </summary>
	[Theory]
	[InlineData(Text + "new Box { Next = null, }")]
	[InlineData(Text + "new Box { Next = null, Last = null, }")]
	public void A_trailing_comma_after_members_is_no_element(string text)
	{
		var match = Both.TryParse(text, typeof(Box).Assembly);

		Assert.True(match.IsSuccess, match.Error);
	}

	/// <summary>
	/// Braces that begin with an element are elements, as they always were — a comparison among
	/// them too, whose `==` is no `=`.
	/// </summary>
	[Fact]
	public void Braces_that_begin_with_an_element_are_elements()
	{
		Assert.Equal(
			[1, 2],
			Both.Compile<Func<int, System.Collections.Generic.List<int>>>(
				"using System.Collections.Generic; (int x) => new List<int> { x, 2 }")(1));
		Assert.Equal(
			[true, false],
			Both.Compile<Func<int, System.Collections.Generic.List<bool>>>(
				"using System.Collections.Generic; (int x) => new List<bool> { x == 1, x == 2 }")(1));
	}

	/// <summary>
	/// A member initializer after elements is refused, and said to be: the mirror of the above, and
	/// CS0747 in C# as well. Wherever the elements are, and whatever their form.
	/// </summary>
	[Theory]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { 1, x = 2 }",                           "x = 2")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { x, x = 2 }",                           "x = 2")]
	[InlineData("using DotGram.Tests.ExpressionLanguage; (int x) => new Holder { Items = { 1, x = 2 } }",           "x = 2")]
	[InlineData("using System.Collections.Generic; (int x) => new Dictionary<int, int> { { 1, 2 }, x = 3 }",       "x = 3")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { 1, x = }",                            "x = }")]
	// Any other assignment is no element either, first or after others: its operator is where.
	[InlineData("using System.Collections.Generic; using System.Text; (StringBuilder b) => new List<int> { 1, b.Capacity = 2 }", "= 2")]
	[InlineData("using System.Collections.Generic; using System.Text; (StringBuilder b) => new List<int> { b.Capacity = 2 }",    "= 2")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { 1, x += 1 }",                          "+= 1")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { x += 1 }",                             "+= 1")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { x, x <<= 1, 2 }",                      "<<= 1")]
	// An indexer's `[0] =`, which no element begins with.
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { 1, [0] = 2 }",                         "[0] = 2")]
	[InlineData("using System.Collections.Generic; (int x) => new List<int> { x, [x] = 2, 3 }",                      "[x] = 2")]
	public void A_member_initializer_after_elements_is_refused(string text, string member)
	{
		var match = Both.TryParse(text, typeof(Box).Assembly);

		Assert.Equal(ExpressionParser.Outcome.NoMatch, match.Outcome);
		Assert.Equal(text.LastIndexOf(member, StringComparison.Ordinal), match.Position);
		Assert.Equal(
			"An element of a collection initializer cannot be an assignment, and members cannot be set among elements.",
			match.Error);
	}

	/// <summary>Cut short after the operator, an element may still be a comparison or a lambda.</summary>
	[Theory]
	[InlineData("using System.Collections.Generic; (int x) => new List<bool> { true, x =")]
	[InlineData("using System.Collections.Generic; (int x) => new List<bool> { true, x += ")]
	[InlineData("using System.Collections.Generic; (int x) => new List<bool> { x =")]
	[InlineData("using System.Collections.Generic; (int x) => new List<bool> { true, [0] =")]
	public void But_not_before_the_assignment_is_one(string text)
	{
		Assert.Equal(ExpressionParser.Outcome.Starved, Both.TryParse(text, typeof(Box).Assembly).Outcome);
	}

	/// <summary>A comma after a comma is no entry, and is refused as the parser refuses it.</summary>
	[Theory]
	[InlineData(Text + "new Box { Next = null,, }")]
	[InlineData(Text + "new Box { Next = null, , Last = null }")]
	public void A_doubled_comma_is_no_element_after_members(string text)
	{
		var match = Both.TryParse(text, typeof(Box).Assembly);

		Assert.False(match.IsSuccess);
		Assert.Equal("Expected a name.", match.Error);
	}

	/// <summary>An assignment in parentheses is an element, and one after a member's `=` is its value.</summary>
	[Fact]
	public void An_assignment_that_is_no_member_initializer_still_reads()
	{
		Assert.Equal(
			[1, 2],
			Both.Compile<Func<int, System.Collections.Generic.List<int>>>(
				"using System.Collections.Generic; (int x) => new List<int> { (x = 1), 2 }")(0));
		Assert.Equal(
			[true, false],
			Both.Compile<Func<int, System.Collections.Generic.List<bool>>>(
				"using System.Collections.Generic; (int x) => new List<bool> { x == 0, x != 0 }")(0));
		Assert.Equal(
			1,
			Both.Compile<Func<System.Collections.Generic.List<Func<int, int>>>>(
				"using System; using System.Collections.Generic; () => new List<Func<int, int>> { z => z + 1 }")()[0](0));
		Assert.Equal(
			3,
			Both.Compile<Func<int, Counter>>(
				"using DotGram.Tests.ExpressionLanguage; (int x) => new Counter { Count = x = 3 }")(0).Count);
	}

	/// <summary>A comma after the last entry of any initializer, which C# allows.</summary>
	[Fact]
	public void A_trailing_comma_is_accepted_in_every_initializer()
	{
		Assert.Null(Both.Compile<Func<Box?, Box>>(Text + "new Box { Next = null, }")(null).Next);
		Assert.NotNull(Both.Compile<Func<Box?, Box>>(Text + "new Box { Next = null, Last = new Box { Next = null, }, }")(null).Last);
		Assert.Equal(
			[1, 2],
			Both.Compile<Func<System.Collections.Generic.List<int>>>(
				"using System.Collections.Generic; () => new List<int> { 1, 2, }")());
		Assert.Equal(
			[3, 4],
			Both.Compile<Func<Holder>>(
				"using DotGram.Tests.ExpressionLanguage; () => new Holder { Items = { 3, 4, }, }")().Items);
		Assert.Equal(
			2,
			Both.Compile<Func<System.Collections.Generic.Dictionary<int, int>>>(
				"using System.Collections.Generic; () => new Dictionary<int, int> { { 1, 2 }, }")()[1]);
		Assert.Equal([1, 2], Both.Compile<Func<int[]>>("() => new int[] { 1, 2, }")());
	}

	/// <summary>A comma alone is no initializer, as in C#.</summary>
	[Theory]
	[InlineData("using System.Collections.Generic; () => new List<int> { , }")]
	[InlineData("() => new int[] { , }")]
	[InlineData(Text + "new Box { Next = null,, }")]
	[InlineData("using System.Collections.Generic; () => new List<int> { 1,, }")]
	public void A_comma_alone_is_refused(string text)
	{
		Assert.False(Both.TryParse(text, typeof(Box).Assembly).IsSuccess);
	}

	const string Text = "using DotGram.Tests.ExpressionLanguage; (Box Next) => ";

	static readonly int[] Depths = [2, 4, 8, 16];

	/// <summary>The places one reading looked up names at, having said whether it read the text.</summary>
	static long Places(
		string text,
		Func<string, ExpressionParser.State, ExpressionParser.Match<LambdaExpression>> read,
		bool accepted)
	{
		var state = new ExpressionParser.State(typeof(Box).Assembly) { Text = text };

		Assert.Equal(accepted, read(text, state).IsSuccess);

		return state.Places;
	}

	/// <summary>Whether no increment of these counts grew faster than linearly on the one before it.</summary>
	static bool Linearly(long[] counts)
	{
		for (var at = 0; at + 2 < counts.Length; at++)
		{
			var earlier = counts[at + 1] - counts[at];
			var later   = counts[at + 2] - counts[at + 1];

			if (later > 0 && (earlier <= 0 || Math.Log2((double)later / earlier) > 1.6))
				return false;
		}

		return true;
	}

	static string Repeated(string piece, int times)
	{
		var built = new System.Text.StringBuilder(piece.Length * times);

		for (var at = 0; at < times; at++)
			built.Append(piece);

		return built.ToString();
	}
}

/// <summary>What the nesting member initializers are written against: members of its own type.</summary>
public sealed class Box
{
	public Box? Next { get; set; }

	public Box? Last { get; set; }
}
