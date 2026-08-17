using System;

using DotGram.Generation;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Level 1: the grammar half called directly, no Roslyn driver involved.
/// </summary>
/// <remarks>
/// These cases are exactly why the scanner uses Roslyn's lexer rather than a
/// parenthesis counter — every one of them hides a <c>)</c> somewhere a counter would
/// trip over. A fake scanner that passed these by accident would be worse than none.
/// </remarks>
public sealed class CSharpScannerTests
{
	[Theory]
	// The two easy cases, then one per way a bare parenthesis counter goes wrong:
	// each of the rest hides a ')' where counting would stop early.
	[InlineData("(a + b) & rest",             6)]   // (a + b)
	[InlineData("(f(x)) & rest",              5)]   // (f(x))
	[InlineData("(\"a\\\")b\") & rest",       8)]   // ("a\")b")   — escaped quote
	[InlineData("(@\"a\"\"b)\") & rest",      9)]   // (@"a""b)")  — verbatim, doubled quote
	[InlineData("($\"a) b\") & rest",         8)]   // ($"a) b")   — interpolated text
	[InlineData("($\"{ F(a) }\") & rest",    12)]   // ($"{ F(a) }")
	[InlineData("(')') & rest",               4)]   // (')')
	[InlineData("(x /* ) */ + y) & rest",    14)]   // (x /* ) */ + y)
	[InlineData("(x // )\n + y) & rest",     12)]   // (x // )\n + y)
	public void Finds_the_matching_parenthesis(string text, int expected)
	{
		var found = RoslynCSharpScanner.Instance.TryFindClosingParenthesis(text, 0, out var index);

		Assert.True(found, $"No closing parenthesis found in {text}");
		Assert.Equal(expected, index);
		Assert.Equal(')', text[index]);
	}

	[Theory]
	// Positions must be absolute. Every case above starts the expression at index 0,
	// where a relative result is indistinguishable from an absolute one — which is
	// exactly how an off-by-offset bug survived here until the lexer used it.
	[InlineData("when @(a + b) & rest",        6, 12)]
	[InlineData("=> @(\"a)b\") & rest",         4, 10)]
	public void Reports_positions_relative_to_the_whole_text(string text, int open, int expected)
	{
		Assert.True(RoslynCSharpScanner.Instance.TryFindClosingParenthesis(text, open, out var index));
		Assert.Equal(expected, index);
		Assert.Equal(')', text[index]);
	}

	[Fact]
	public void Reports_an_unterminated_expression()
	{
		var found = RoslynCSharpScanner.Instance.TryFindClosingParenthesis("(a + b", 0, out var index);

		Assert.False(found);
		Assert.Equal(-1, index);
	}

	[Fact]
	public void Refuses_a_position_that_is_not_an_opening_parenthesis()
	{
		Assert.False(RoslynCSharpScanner.Instance.TryFindClosingParenthesis("a + b", 0, out _));
	}
}
