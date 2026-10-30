using System;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Language.Tests;

public sealed class CSharpStringMapTests
{
	/// <summary>A caret in a <c>[Gram]</c> literal lands where it stands in the grammar, however the literal is spelled.</summary>
	[Theory]
	[InlineData("\"A = 'a' => @(par|)\"")]
	[InlineData("\"A = '\\n' => @(par|)\"")]
	[InlineData("@\"A = \"\"x\"\" => @(par|)\"")]
	[InlineData("\"\"\"\nA = 'a' => @(par|)\n\"\"\"")]
	public void DecodesALiteralAndTheCaretInIt(string marked)
	{
		var caret = marked.IndexOf('|');
		var literal = marked.Remove(caret, 1);

		Assert.True(CSharpStringMap.TryDecode(literal, caret, out var grammar, out var position));
		Assert.EndsWith("@(par", grammar.Substring(0, position), StringComparison.Ordinal);
		Assert.StartsWith(")", grammar.Substring(position), StringComparison.Ordinal);
	}

	[Fact]
	public void MapsRegularEscapesToTheirWholeSpelling()
	{
		const string literal = "\"a\\n\\u0042\"";
		var map = Map(literal, out var value);

		Assert.Equal("a\nB", value);
		Assert.Equal("\\n", Slice(literal, map, 1, 1));
		Assert.Equal("\\u0042", Slice(literal, map, 2, 1));
	}

	[Theory]
	[InlineData("\"\\U00000041\"", "A")]
	[InlineData("\"\\U0001F600\"", "😀")]
	public void MapsEightDigitUnicodeEscape(string literal, string value)
	{
		var map = Map(literal, out var decoded);

		Assert.Equal(value, decoded);
		Assert.Equal("\\U" + literal.Substring(3, 8), Slice(literal, map, 0, decoded.Length));
	}

	[Fact]
	public void MapsVerbatimDoubledQuoteToItsWholeSpelling()
	{
		const string literal = "@\"a\"\"b\"";
		var map = Map(literal, out var value);

		Assert.Equal("a\"b", value);
		Assert.Equal("\"\"", Slice(literal, map, 1, 1));
	}

	[Fact]
	public void MapsIndentedRawStringWithoutIncludingIndentation()
	{
		const string literal = "\"\"\"\r\n\t\tRule = 'a'\r\n\t\tparse Rule\r\n\t\t\"\"\"";
		var map = Map(literal, out var value);
		var at  = value.IndexOf("parse", StringComparison.Ordinal);

		Assert.Equal("Rule = 'a'\r\nparse Rule", value);
		Assert.Equal("parse", Slice(literal, map, at, "parse".Length));
	}

	[Fact]
	public void MapsIndentedRawStringWithUnindentedBlankLine()
	{
		const string literal = "\"\"\"\r\n\t\tRule = 'a'\r\n\r\n\t\tparse Rule\r\n\t\t\"\"\"";
		var map = Map(literal, out var value);
		var at  = value.IndexOf("parse", StringComparison.Ordinal);

		Assert.Equal("Rule = 'a'\r\n\r\nparse Rule", value);
		Assert.Equal("parse", Slice(literal, map, at, "parse".Length));
	}

	static CSharpStringMap Map(string literal, out string value)
	{
		var expression = Assert.IsType<LiteralExpressionSyntax>(SyntaxFactory.ParseExpression(literal));
		var token      = expression.Token;

		value = token.ValueText;

		Assert.True(CSharpStringMap.TryCreate(token, out var map));

		return Assert.IsType<CSharpStringMap>(map);
	}

	static string Slice(string source, CSharpStringMap map, int start, int length)
	{
		Assert.True(map.TryMap(start, length, out var span));

		return source.Substring(span.Start, span.Length);
	}
}
