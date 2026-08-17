using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The lexer compared against its own dump, one token per line.
/// </summary>
/// <remarks>
/// Asserting on the rendered stage output rather than on hand-built token arrays is
/// deliberate: a regression reads as a line-level diff instead of an index mismatch,
/// and the expected text doubles as documentation of what the notation lexes to.
/// </remarks>
public sealed class GramLexerTests
{
	[Fact]
	public void Lexes_a_rule()
	{
		Assert.Equal(
			"""
			Identifier "Row"
			Equals
			String "D"
			Ampersand
			Character "|"
			Ampersand
			Identifier "symbol"
			Colon
			Identifier "Text"
			""",
			GramLexer.Tokenize("""Row = "D" & '|' & symbol: Text""").ToString());
	}

	[Fact]
	public void Takes_the_longest_operator()
	{
		// Every one of these would lex as two tokens under a naive single-character pass.
		Assert.Equal(
			"""
			DotDot
			Arrow
			PositiveLookahead
			NegativeLookahead
			Dot
			Equals
			Question
			""",
			GramLexer.Tokenize(".. => ?= ?! . = ?").ToString());
	}

	[Fact]
	public void Lexes_quantifiers_and_element_sets()
	{
		Assert.Equal(
			"""
			OpenBracket
			Character "a"
			DotDot
			Character "z"
			CloseBracket
			Plus
			OpenBrace
			Integer "2"
			Comma
			Integer "4"
			CloseBrace
			""",
			GramLexer.Tokenize("['a'..'z']+{2,4}").ToString());
	}

	[Fact]
	public void Lexes_a_unicode_category()
	{
		Assert.Equal(
			"""
			OpenBracket
			UnicodeCategory "Lu"
			Bar
			UnicodeCategory "Nd"
			CloseBracket
			""",
			GramLexer.Tokenize(@"[\p{Lu} | \p{Nd}]").ToString());
	}

	[Fact]
	public void Resolves_escape_sequences()
	{
		var tokens = GramLexer.Tokenize(@"'\n' '\'' ""a\""b"" 'A'");

		Assert.Equal(["\n", "'", "a\"b", "A"], tokens.Tokens
			.Where(token => token.Kind is TokenKind.Character or TokenKind.String)
			.Select(token => token.Value));
	}

	[Fact]
	public void Drops_comments_and_whitespace()
	{
		Assert.Equal(
			"""
			Identifier "A"
			Equals
			Identifier "B"
			""",
			GramLexer.Tokenize("""
				// leading
				A = /* inline */ B   // trailing
				""").ToString());
	}

	[Fact]
	public void Reads_an_inline_expression_whole()
	{
		Assert.Equal(
			"""
			Identifier "when"
			CSharpExpression "qty > 0 && s == \"a)b\""
			Ampersand
			Identifier "rest"
			""",
			GramLexer.Tokenize(
				"""when @(qty > 0 && s == "a)b") & rest""",
				RoslynCSharpScanner.Instance).ToString());
	}

	[Fact]
	public void An_at_not_followed_by_a_parenthesis_is_just_an_at()
	{
		// Adjacency is the whole rule: `@ (` is a name that never came.
		Assert.Equal(
			"""
			At
			Identifier "int"
			Dot
			Identifier "Parse"
			OpenParen
			Identifier "text"
			CloseParen
			""",
			GramLexer.Tokenize("@ int . Parse ( text )").ToString());
	}

	[Theory]
	[InlineData("'a",           GramLexer.UnterminatedCharacter)]
	[InlineData("\"a",          GramLexer.UnterminatedString)]
	[InlineData("A /* b",       GramLexer.UnterminatedComment)]
	[InlineData(@"'\q'",        GramLexer.InvalidEscape)]
	[InlineData("A ` B",        GramLexer.UnexpectedCharacter)]
	[InlineData(@"[\x{Lu}]",    GramLexer.MalformedCategory)]
	public void Reports(string source, string expectedId)
	{
		var tokens = GramLexer.Tokenize(source);

		Assert.Contains(expectedId, tokens.Diagnostics.Select(diagnostic => diagnostic.Id));
	}

	[Fact]
	public void An_inline_expression_without_a_scanner_is_a_diagnostic_not_a_crash()
	{
		var tokens = GramLexer.Tokenize("=> @(a + b)");

		Assert.Contains(GramLexer.ExpressionNeedsScanner, tokens.Diagnostics.Select(d => d.Id));
	}

	[Fact]
	public void Positions_point_back_at_the_source()
	{
		var source = """A = "bc"?""";
		var tokens = GramLexer.Tokenize(source);

		foreach (var token in tokens.Tokens.Where(t => t.Kind != TokenKind.EndOfFile))
			Assert.Equal(token.Length, source.Substring(token.Position, token.Length).Length);

		var literal = tokens.Tokens.Single(t => t.Kind == TokenKind.String);

		Assert.Equal(@"""bc""", source.Substring(literal.Position, literal.Length));
	}
}
