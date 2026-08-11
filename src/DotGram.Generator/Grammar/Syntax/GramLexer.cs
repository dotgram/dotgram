using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Grammar.Syntax;

/// <summary>
/// Turns `.gram` source into tokens.
/// </summary>
/// <remarks>
/// <para>
/// Whitespace and comments are dropped rather than emitted: layout carries no meaning
/// in `.gram`, so nothing downstream can need them. Rule separation works out of the
/// grammar alone — every operand needs an explicit connector, so an expression can
/// never be continued by what looks like the next rule.
/// </para>
/// <para>
/// The one place adjacency matters is <c>@(</c>: the parenthesis must follow the
/// <c>@</c> with nothing between. Everywhere else whitespace is free, so
/// <c>@ int . Parse ( text )</c> lexes the same as the tight spelling — but
/// <c>@ (a + b)</c> would be indistinguishable from a call on a missing name.
/// </para>
/// </remarks>
public static class GramLexer
{
	public const string UnterminatedCharacter  = "GRAM1001";
	public const string UnterminatedString     = "GRAM1002";
	public const string UnterminatedComment    = "GRAM1003";
	public const string InvalidEscape          = "GRAM1004";
	public const string UnexpectedCharacter    = "GRAM1005";
	public const string UnterminatedExpression = "GRAM1006";
	public const string ExpressionNeedsScanner = "GRAM1007";
	public const string MalformedCategory      = "GRAM1008";

	/// <param name="scanner">
	/// Finds where an inline <c>@(...)</c> ends. Null means the grammar may not use
	/// one — reported as a diagnostic rather than a crash, since a caller that only
	/// exercises the grammar side legitimately has no C# lexer to offer.
	/// </param>
	public static TokenList Tokenize(string text, ICSharpScanner? scanner = null)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var tokens      = new List<Token>();
		var diagnostics = new List<GramDiagnostic>();
		var position    = 0;

		void Report(string id, string message, int start, int length) =>
			diagnostics.Add(new GramDiagnostic(id, message, start, length, GramSeverity.Error));

		void Add(TokenKind kind, int start, int length, string? value = null) =>
			tokens.Add(new Token(kind, start, length, value));

		while (position < text.Length)
		{
			var start   = position;
			var current = text[position];

			if (char.IsWhiteSpace(current))
			{
				position++;
				continue;
			}

			if (current == '/' && position + 1 < text.Length)
			{
				if (text[position + 1] == '/')
				{
					while (position < text.Length && text[position] is not ('\n' or '\r'))
						position++;

					continue;
				}

				if (text[position + 1] == '*')
				{
					position += 2;

					while (position + 1 < text.Length && !(text[position] == '*' && text[position + 1] == '/'))
						position++;

					if (position + 1 >= text.Length)
					{
						Report(UnterminatedComment, "Unterminated comment.", start, text.Length - start);
						position = text.Length;
					}
					else
					{
						position += 2;
					}

					continue;
				}
			}

			if (current == '_' || char.IsLetter(current))
			{
				while (position < text.Length && (text[position] == '_' || char.IsLetterOrDigit(text[position])))
					position++;

				Add(TokenKind.Identifier, start, position - start, text.Substring(start, position - start));
				continue;
			}

			if (char.IsDigit(current))
			{
				while (position < text.Length && char.IsDigit(text[position]))
					position++;

				Add(TokenKind.Integer, start, position - start, text.Substring(start, position - start));
				continue;
			}

			switch (current)
			{
				case '\'':
				case '"':
					position = ReadQuoted(text, position, current, diagnostics, out var value);

					Add(current == '\'' ? TokenKind.Character : TokenKind.String, start, position - start, value);
					continue;

				case '\\':
					position = ReadUnicodeCategory(text, position, diagnostics, out var category);

					if (category is not null)
						Add(TokenKind.UnicodeCategory, start, position - start, category);

					continue;

				case '@' when position + 1 < text.Length && text[position + 1] == '(':
					position = ReadCSharpExpression(text, position, scanner, diagnostics, out var expression);

					if (expression is not null)
						Add(TokenKind.CSharpExpression, start, position - start, expression);

					continue;
			}

			var kind = ReadOperator(text, ref position);

			if (kind is null)
			{
				Report(UnexpectedCharacter, $"Unexpected character '{current}'.", start, 1);
				Add(TokenKind.Unknown, start, 1, current.ToString());
				position++;

				continue;
			}

			Add(kind.Value, start, position - start);
		}

		tokens.Add(new Token(TokenKind.EndOfFile, text.Length, 0, null));

		return new TokenList(tokens, diagnostics);
	}

	/// <summary>Longest match first, so <c>..</c> never lexes as two dots.</summary>
	static TokenKind? ReadOperator(string text, ref int position)
	{
		// Over a span rather than a two-character Substring: the old spelling allocated
		// a string per operator token purely to switch on it.
		var kind = text.AsSpan(position) switch
		{
			['.', '.', ..] => TokenKind.DotDot,
			['=', '>', ..] => TokenKind.Arrow,
			['?', '=', ..] => TokenKind.PositiveLookahead,
			['?', '!', ..] => TokenKind.NegativeLookahead,
			_              => (TokenKind?)null,
		};

		if (kind is not null)
		{
			position += 2;
			return kind;
		}

		kind = text[position] switch
		{
			'&' => TokenKind.Ampersand,
			'|' => TokenKind.Bar,
			'(' => TokenKind.OpenParen,
			')' => TokenKind.CloseParen,
			'[' => TokenKind.OpenBracket,
			']' => TokenKind.CloseBracket,
			'{' => TokenKind.OpenBrace,
			'}' => TokenKind.CloseBrace,
			',' => TokenKind.Comma,
			';' => TokenKind.Semicolon,
			':' => TokenKind.Colon,
			'=' => TokenKind.Equals,
			'?' => TokenKind.Question,
			'*' => TokenKind.Star,
			'+' => TokenKind.Plus,
			'^' => TokenKind.Caret,
			'.' => TokenKind.Dot,
			'<' => TokenKind.Less,
			'>' => TokenKind.Greater,
			'@' => TokenKind.At,
			_   => null,
		};

		if (kind is not null)
			position++;

		return kind;
	}

	/// <summary>Reads a character or string literal, resolving escapes.</summary>
	static int ReadQuoted(string text, int position, char quote, List<GramDiagnostic> diagnostics, out string value)
	{
		var start   = position++;
		var decoded = new StringBuilder();

		while (position < text.Length && text[position] != quote)
		{
			if (text[position] is '\n' or '\r')
				break;

			if (text[position] != '\\')
			{
				decoded.Append(text[position++]);
				continue;
			}

			var escapeStart = position++;

			if (position >= text.Length)
				break;

			var escaped = text[position++];

			switch (escaped)
			{
				case '\'': decoded.Append('\''); break;
				case '"':  decoded.Append('"');  break;
				case '\\': decoded.Append('\\'); break;
				case '0':  decoded.Append('\0'); break;
				case 'a':  decoded.Append('\a'); break;
				case 'b':  decoded.Append('\b'); break;
				case 'f':  decoded.Append('\f'); break;
				case 'n':  decoded.Append('\n'); break;
				case 'r':  decoded.Append('\r'); break;
				case 't':  decoded.Append('\t'); break;
				case 'v':  decoded.Append('\v'); break;

				case 'u' when TryReadHex4(text, position, out var scalar):

					decoded.Append(scalar);
					position += 4;
					break;

				default:
					diagnostics.Add(new GramDiagnostic(
						InvalidEscape,
						$"Unrecognized escape sequence '\\{escaped}'.",
						escapeStart,
						position - escapeStart,
						GramSeverity.Error));
					break;
			}
		}

		value = decoded.ToString();

		if (position < text.Length && text[position] == quote)
			return position + 1;

		diagnostics.Add(new GramDiagnostic(
			quote == '\'' ? UnterminatedCharacter : UnterminatedString,
			quote == '\'' ? "Unterminated character literal." : "Unterminated string literal.",
			start,
			position - start,
			GramSeverity.Error));

		return position;
	}

	/// <summary>
	/// Four hex digits of a <c>\uXXXX</c> escape.
	/// </summary>
	/// <remarks>
	/// By hand rather than through <c>int.TryParse</c>: the span overload does not
	/// exist on netstandard2.0, and the string overload would allocate for four
	/// characters. Four digits are hardly worth a parser.
	/// </remarks>
	static bool TryReadHex4(string text, int position, out char value)
	{
		value = '\0';

		if (position + 4 > text.Length)
			return false;

		var scalar = 0;

		for (var i = position; i < position + 4; i++)
		{
			var digit = text[i] switch
			{
				>= '0' and <= '9' => text[i] - '0',
				>= 'a' and <= 'f' => text[i] - 'a' + 10,
				>= 'A' and <= 'F' => text[i] - 'A' + 10,
				_                 => -1,
			};

			if (digit < 0)
				return false;

			scalar = scalar * 16 + digit;
		}

		value = (char)scalar;

		return true;
	}

	/// <summary>Reads <c>\p{Category}</c>, the .NET regex spelling.</summary>
	static int ReadUnicodeCategory(string text, int position, List<GramDiagnostic> diagnostics, out string? category)
	{
		var start = position;
		var close = position + 2 < text.Length && text[position + 1] == 'p' && text[position + 2] == '{'
			? text.IndexOf('}', position + 3)
			: -1;

		if (close < 0)
		{
			category = null;

			diagnostics.Add(new GramDiagnostic(
				MalformedCategory,
				@"Expected a Unicode category in the form \p{Lu}.",
				start,
				1,
				GramSeverity.Error));

			return position + 1;
		}

		category = text.Substring(position + 3, close - position - 3);

		return close + 1;
	}

	/// <summary>
	/// Reads <c>@(…)</c> whole. The scan is a lexical question — a <c>)</c> can hide
	/// inside a string, an interpolation or a comment — so it belongs here rather than
	/// leaving the parser to switch the lexer's mode mid-stream.
	/// </summary>
	static int ReadCSharpExpression(
		string                text,
		int                   position,
		ICSharpScanner?       scanner,
		List<GramDiagnostic>  diagnostics,
		out string?           expression)
	{
		expression = null;

		var openParen = position + 1;

		if (scanner is null)
		{
			diagnostics.Add(new GramDiagnostic(
				ExpressionNeedsScanner,
				"An inline @(...) expression needs a C# scanner, and none was supplied.",
				position,
				2,
				GramSeverity.Error));

			return position + 2;
		}

		if (!scanner.TryFindClosingParenthesis(text, openParen, out var closeParen))
		{
			diagnostics.Add(new GramDiagnostic(
				UnterminatedExpression,
				"Unterminated inline @(...) expression.",
				position,
				text.Length - position,
				GramSeverity.Error));

			return text.Length;
		}

		expression = text.Substring(openParen + 1, closeParen - openParen - 1);

		return closeParen + 1;
	}
}
