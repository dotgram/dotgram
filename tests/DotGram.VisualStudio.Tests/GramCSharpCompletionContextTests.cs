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
	public void FindsCSharpNamePrefixAfterTransition(string text, string expected)
	{
		Assert.True(GramCSharpCompletionContext.TryGetPrefix(text, text.Length, out var prefix));
		Assert.Equal(expected, prefix);
	}

	[Theory]
	[InlineData("Rule = Other")]
	[InlineData("Rule = when @(value)")]
	[InlineData("Rule = when @(value.Me")]
	public void DoesNotTakeOverGrammarOrParenthesizedExpressions(string text)
	{
		Assert.False(GramCSharpCompletionContext.TryGetPrefix(text, text.Length, out _));
	}
}
