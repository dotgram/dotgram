using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// The tuple literal: `(a, b)`, which is a <c>ValueTuple</c> and nothing else.
/// </summary>
/// <remarks>
/// <para>
/// It was refused before 2026-09-23, and the generated text that wanted one wrote
/// <c>ValueTuple.Create(…)</c> instead. What was refused was not the elements in it: `(1, 2)`
/// was refused too, because `( … )` read as a parenthesised expression and the comma was
/// simply where the grammar stopped.
/// </para>
/// <para>
/// Element NAMES are refused, and that is a decision rather than an omission: in C# a name is
/// metadata beside the type and is erased from the value, so a compiled expression tree — which
/// carries the type alone — could hand it to nobody. The refusal says so, in its own words, so
/// that a text written with one is not told about a colon or about a name nothing declares.
/// </para>
/// </remarks>
public sealed class TupleTests
{
	delegate object Reading(int tag, string value);

	const string Using = "using System; using DotGram.Tests.ExpressionLanguage; ";

	[Theory]
	// The two shapes the report came from, and a third element to show it is not a pair rule.
	[InlineData("(int tag, string value) => (true, value)", "(True, 1.25)")]
	[InlineData("(int tag, string value) => (1, 2)", "(1, 2)")]
	[InlineData("(int tag, string value) => (tag, value, tag > 0)", "(25010, 1.25, True)")]
	// A tuple inside a tuple, which is an element like any other.
	[InlineData("(int tag, string value) => ((1, 2), 3)", "((1, 2), 3)")]
	// Past seven the eighth element holds the rest, which is what C# builds and what
	// `ValueTuple` is shaped for. Nothing caps the count.
	[InlineData("(int tag, string value) => (1, 2, 3, 4, 5, 6, 7, 8, 9)", "(1, 2, 3, 4, 5, 6, 7, 8, 9)")]
	// Read back by position, and handed to a method that takes one.
	[InlineData("(int tag, string value) => (tag, 2).Item1", "25010")]
	[InlineData("(int tag, string value) => Tuples.Took((true, value))", "True:1.25")]
	// A single parenthesised expression is still that, which is the whole of what tells the
	// two apart.
	[InlineData("(int tag, string value) => (tag)", "25010")]
	public void A_tuple_reads_and_is_worth_what_C_sharp_makes_of_it(string text, string expected)
	{
		var made = ExpressionParser.Compile<Reading>(Using + text, typeof(TupleTests).Assembly);

		Assert.Equal(expected, made(25010, "1.25")!.ToString());
	}

	[Theory]
	// A name is refused whether or not anything of that name is declared — the second would
	// otherwise be read as far as the name and refused in words about the colon.
	[InlineData("(int tag, string value) => (Valid: true, Value: 1)", "Valid:")]
	[InlineData("(int tag, string value) => (tag: 1, other: 2)", "tag:")]
	// A name on any element, not only the first.
	[InlineData("(int tag, string value) => (1, Value: 2)", "Value:")]
	public void A_named_element_is_refused_and_the_refusal_says_which(string text, string named)
	{
		var thrown = Assert.Throws<InvalidOperationException>(
			() => ExpressionParser.Compile<Reading>(Using + text, typeof(TupleTests).Assembly));

		Assert.Contains(named, thrown.Message, StringComparison.Ordinal);
		Assert.Contains("Item1", thrown.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void An_element_with_no_type_of_its_own_leaves_the_tuple_with_none()
	{
		var thrown = Assert.Throws<InvalidOperationException>(
			() => ExpressionParser.Compile<Reading>(
				Using + "(int tag, string value) => (null, 1)", typeof(TupleTests).Assembly));

		Assert.Contains("element 1", thrown.Message, StringComparison.Ordinal);
	}
}

/// <summary>What the texts above hand a tuple to, kept beside the witness.</summary>
static class Tuples
{
	/// <summary>A method taking a tuple, which is how one is asked to be built for a parameter.</summary>
	public static string Took((bool Valid, string Value) pair)
	{
		return pair.Valid + ":" + pair.Value;
	}
}
