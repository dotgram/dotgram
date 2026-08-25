using DotGram.VisualStudio;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class GramCSharpCompletionContextTests
{
	[Theory]
	[InlineData("Rule : @", "")]
	[InlineData("Rule : @dec", "dec")]
	[InlineData("Rule = x => @decimal.Pa", "decimal.Pa")]
	[InlineData("Rule : @global::System.String", "global::System.String")]
	[InlineData("Rule = when @(", "")]
	[InlineData("Rule = when @(decimal.Pa", "decimal.Pa")]
	[InlineData("Rule = when @(Call(value, dec", "Call(value, dec")]
	public void FindsCSharpNamePrefixAfterTransition(string text, string expected)
	{
		Assert.True(GramCSharpCompletionContext.TryGetPrefix(text, text.Length, out var prefix));
		Assert.Equal(expected, prefix);
	}

	[Theory]
	[InlineData("Rule = Other")]
	[InlineData("Rule = when @(value)")]
	public void DoesNotTakeOverGrammarOrClosedParenthesizedExpressions(string text)
	{
		Assert.False(GramCSharpCompletionContext.TryGetPrefix(text, text.Length, out _));
	}

	[Fact]
	public void FindsHoveredSymbolInsideMemberAccess()
	{
		const string text = "Rule = value => @decimal.Parse";
		var position = text.IndexOf("Parse", System.StringComparison.Ordinal) + 2;

		Assert.True(GramCSharpCompletionContext.TryGetExpression(
			text, position, out var expression, out var expressionStart, out var symbolStart, out var symbolLength));
		Assert.Equal("decimal.Parse", expression);
		Assert.Equal(text.IndexOf("decimal", System.StringComparison.Ordinal), expressionStart);
		Assert.Equal("Parse", text.Substring(symbolStart, symbolLength));
	}

	[Fact]
	public void FindsHoveredSymbolInsideParenthesizedExpression()
	{
		const string text = "Rule = when @(decimal.Parse(value))";
		var position = text.IndexOf("Parse", System.StringComparison.Ordinal) + 2;

		Assert.True(GramCSharpCompletionContext.TryGetExpression(
			text, position, out var expression, out var expressionStart, out var symbolStart, out var symbolLength));
		Assert.Equal("decimal.Parse(value)", expression);
		Assert.Equal(text.IndexOf("decimal", System.StringComparison.Ordinal), expressionStart);
		Assert.Equal("Parse", text.Substring(symbolStart, symbolLength));
	}
}
