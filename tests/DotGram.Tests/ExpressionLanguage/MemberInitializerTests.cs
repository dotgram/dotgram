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
	/// that the elements' reading goes on into the next level rather than stopping at a name. Four depths a doubling
	/// apart, and judged as <see cref="ExpressionBlowUpTests"/> judges a nesting: by how the
	/// increments grow, so that what every reading costs whatever its depth cannot read as a slope.
	/// Linear is 1; the reading twice a level reads 8 at the last doubling, and the quiet reading
	/// inside the building one reads 2.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("a new object's members",           "",                  "new Box { Next = new Box { Last = Next }, Next = ", " }", "")]
	[InlineData("a new object's members, unclosed", "",                  "new Box { Next = new Box { Last = Next }, Next = ", "",   "")]
	[InlineData("a member's own members",           "new Box { Next = ", "{ Last = Next, Next = ",                            " }", " }")]
	[InlineData("a member's own members, unclosed", "new Box { Next = ", "{ Last = Next, Next = ",                            "",   "")]
	public void Nested_member_initializers_are_read_once_a_level(
		string what, string head, string opener, string closer, string tail)
	{
		var generated = new long[Depths.Length];
		var hand      = new long[Depths.Length];

		for (var at = 0; at < Depths.Length; at++)
		{
			var text = Text + head + Repeated(opener, Depths[at]) + "Next" + Repeated(closer, Depths[at]) + tail;

			generated[at] = Places(text, ExpressionParser.TryParseLambda, closer.Length > 0);
			hand[at]      = Places(text, HandExpression.TryParseLambda,   closer.Length > 0);
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
	public void An_element_after_a_member_initializer_is_refused(string text, string element)
	{
		var match = Both.TryParse(text, typeof(Box).Assembly);

		Assert.Equal(ExpressionParser.Outcome.NoMatch, match.Outcome);
		Assert.Equal(text.LastIndexOf(element, StringComparison.Ordinal), match.Position);
		Assert.Equal(
			"An initializer that sets members cannot add elements as well; every entry in it has to be 'Name = ...'.",
			match.Error);
	}

	/// <summary>Cut short where the element could still turn out to be a member, the text is only short.</summary>
	[Theory]
	[InlineData(Text + "new Box { Next = null, Next")]
	[InlineData(Text + "new Box { Next = null, ")]
	public void But_not_before_the_element_is_one(string text)
	{
		var match = Both.TryParse(text, typeof(Box).Assembly);

		Assert.Equal(ExpressionParser.Outcome.Starved, match.Outcome);
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
