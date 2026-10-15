using System;

using DotGram.Examples.Expressions;
using DotGram.Examples.Formats;
using DotGram.Examples.Languages;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The lexemes of the examples are whole: trivia stands between their tokens and not inside
/// them (§4.5), and a string keeps the space just inside its quotes.
/// </summary>
public sealed class ExampleLexemeTests
{
	[Theory]
	[InlineData("1 .5")]
	[InlineData("1. 5")]
	[InlineData("1 . 5")]
	[InlineData("2 * 1 .5")]
	public void A_number_of_the_calculator_has_no_space_inside_it(string expression)
	{
		Assert.False(Calculator.TryEvaluateDecimal(expression).IsSuccess, expression);
	}

	[Fact]
	public void And_a_number_beside_a_space_is_still_a_number()
	{
		Assert.Equal(3.0m, Calculator.EvaluateDecimal(" 1.5 * 2 "));
	}

	[Theory]
	[InlineData("Price > 0 .75")]
	[InlineData("Price > 0. 75")]
	[InlineData("Price IN (1, 2 .5, 3)")]
	public void A_number_of_a_filter_has_no_space_inside_it(string text)
	{
		Assert.ThrowsAny<Exception>(() => Filter.Read(text));
	}

	[Theory]
	[InlineData("Note = ' a'", " a")]
	[InlineData("Note = 'a '", "a ")]
	[InlineData("Note = ' '", " ")]
	[InlineData("Note = ''", "")]
	public void A_string_of_a_filter_keeps_its_spaces(string text, string expected)
	{
		Assert.Equal(expected, Assert.IsType<Compare>(Filter.Read(text)).Value);
	}

	[Theory]
	[InlineData("[1 .5]")]
	[InlineData("[1. 5]")]
	[InlineData("[1 . 5]")]
	public void A_number_of_JSON_has_no_space_inside_it(string text)
	{
		Assert.False(JsonParser.TryParseJson(text).IsSuccess, text);
	}

	[Fact]
	public void And_a_number_beside_spaces_is_still_a_number_in_JSON()
	{
		Assert.True(JsonParser.TryParseJson("[ 1.5 , 2 ]").IsSuccess);
	}

	[Theory]
	[InlineData("[\" a\"]", " a")]
	[InlineData("[\"a \"]", "a ")]
	[InlineData("[\" \"]", " ")]
	public void A_string_of_JSON_keeps_its_spaces(string text, string expected)
	{
		var read = (JsonArray)JsonParser.Read(text);

		Assert.Equal(expected, ((JsonText)Assert.Single(read.Items)).Text);
	}
}
