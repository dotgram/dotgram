using System;

using DotGram.Grammar;

using Microsoft.CodeAnalysis.CSharp;

namespace DotGram.Generation;

/// <summary>
/// Finds the end of an inline <c>@(...)</c> expression using Roslyn's own C# lexer.
/// </summary>
/// <remarks>
/// <para>
/// The lexer, not the parser. <c>SyntaxFactory.ParseExpression</c> looks like the right
/// tool and is not: it is greedy and knows nothing about where a grammar expression
/// ends, while <c>&amp;</c>, <c>|</c>, <c>*</c>, <c>+</c>, <c>?</c>, <c>[</c> and
/// <c>..</c> are all valid C# operators. On <c>where @(qty &gt; 0) &amp; b: Y</c> it
/// would happily consume <c>&amp; b</c> and stop only at the colon.
/// </para>
/// <para>
/// Tokens are what make this correct: a string, verbatim, interpolated or raw literal,
/// a character literal and a comment each arrive as a single token, so a <c>)</c>
/// inside one cannot change the nesting depth.
/// </para>
/// </remarks>
public sealed class RoslynCSharpScanner : ICSharpScanner
{
	public static readonly RoslynCSharpScanner Instance = new();

	public bool TryFindClosingParenthesis(string text, int openParenthesisIndex, out int closeParenthesisIndex)
	{
		closeParenthesisIndex = -1;

		if (text is null || openParenthesisIndex < 0 || openParenthesisIndex >= text.Length || text[openParenthesisIndex] != '(')
			return false;

		var depth = 0;

		foreach (var token in SyntaxFactory.ParseTokens(text, offset: openParenthesisIndex))
		{
			switch (token.Kind())
			{
				case SyntaxKind.OpenParenToken:
					depth++;
					break;

				case SyntaxKind.CloseParenToken:
					if (--depth == 0)
					{
						closeParenthesisIndex = token.SpanStart;
						return true;
					}

					break;

				case SyntaxKind.EndOfFileToken:
					return false;
			}
		}

		return false;
	}
}
