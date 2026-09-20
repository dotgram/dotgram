using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Every example of the package's two shipped pages, run rather than read (D37).
/// </summary>
/// <remarks>
/// <para>
/// An example on a page a package ships is prose: nothing holds it to the package, and a change to
/// what a call does need not touch the page that shows it. DotGram.Finance shipped a first call
/// that threw, in both of its pages, for a day — so these compile and run what README.md and
/// SKILL.md tell a reader to write.
/// </para>
/// <para>
/// And they hold the values the comments claim, not merely that the calls compile: a comment
/// saying <c>8</c> beside a call that answers <c>9</c> misleads a reader as far as one that
/// throws. Where a page shows a fragment rather than a program — a caller's own type, a text from
/// somewhere else — the test writes the smallest whole thing of that shape.
/// </para>
/// </remarks>
public sealed class ExpressionShippedExampleTests
{
	// ── README.md ───────────────────────────────────────────────────────────────

	[Fact]
	public void The_first_example_of_the_readme()
	{
		var square = ExpressionParser.Compile<Func<int, int>>("(int x) => x * x - 1");

		Assert.Equal(8, square(3));
	}

	[Fact]
	public void A_body_of_statements()
	{
		var calculate = ExpressionParser.Compile<Func<int, int, int>>(
			"""
			(int x, int y) =>
			{
			    int sum = x + y;
			    return sum * sum;
			}
			""");

		Assert.Equal(25, calculate(2, 3));
	}

	[Fact]
	public void The_tree_and_how_it_prints()
	{
		var expression = ExpressionParser.Parse("(double x) => x / 2.0");

		Assert.Equal("x => (x / 2)", expression.ToString());
	}

	[Fact]
	public void A_refusal_answers_instead_of_throwing()
	{
		var match = ExpressionParser.TryParse("(string s) => s - 1");

		Assert.False(match.IsSuccess);

		// The page says the message is what Expression.Subtract said about the two types, so the
		// test holds it to naming both rather than to the sentence, which is the BCL's to word.
		Assert.Contains("String", match.Error);
		Assert.Contains("Int32", match.Error);
	}

	[Fact]
	public void A_using_inside_the_text()
	{
		var count = ExpressionParser.Compile<Func<IList<int>, int>>(
			"""
			using System.Collections.Generic;

			(IList<int> l) => l.Count
			""");

		Assert.Equal(3, count(new List<int> { 1, 2, 3 }));
	}

	// ── SKILL.md ────────────────────────────────────────────────────────────────

	[Fact]
	public void The_three_calls_the_skill_opens_with()
	{
		var price = ExpressionParser.Compile<Func<decimal, int, decimal>>(
			"(decimal unit, int count) => unit * count * (count > 10 ? 0.9m : 1m)");

		Assert.Equal(20m, price(2m, 10));
		Assert.Equal(19.8m, price(2m, 11));

		LambdaExpression tree = ExpressionParser.Parse("(double x) => x / 2.0");

		Assert.Equal(typeof(double), Assert.Single(tree.Parameters).Type);

		var match = ExpressionParser.TryParse("(string s) => s - 1");

		Assert.False(match.IsSuccess);

		// What the page prints on a refusal: a position and a message, both of them there.
		Assert.NotNull(match.Error);
		Assert.True(match.Position >= 0);
	}

	static readonly Assembly Caller = typeof(ExpressionShippedExampleTests).Assembly;

	[Fact]
	public void A_rule_over_a_type_of_the_calling_assembly()
	{
		// The page shows the two lines and leaves the type to the reader. A name is resolved
		// from what the text says and the assembly it is handed, and nothing else, so the
		// smallest whole thing of that shape names its namespace as a consumer would.
		const string text = """
			using DotGram.Tests.ExpressionLanguage;

			(Order o) => o.Quantity > 10
			""";

		var rule = ExpressionParser.Compile<Func<Order, bool>>(text, Caller);

		Assert.True(rule(new Order { Quantity = 11 }));
		Assert.False(rule(new Order { Quantity = 10 }));
	}

	[Fact]
	public void A_query_over_an_array()
	{
		var total = ExpressionParser.Compile<Func<int[], int>>(
			"""
			using System.Linq;

			(int[] a) => a.Where(n => n > 1).Sum()
			""");

		Assert.Equal(5, total([1, 2, 3]));
	}
}

/// <summary>The type the skill's fragment stands for: a consumer's own, in their own assembly.</summary>
public sealed class Order
{
	public int Quantity { get; set; }
}
