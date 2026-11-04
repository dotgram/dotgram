using System;

using DotGram.Sql.Ast;

namespace DotGram.Sql.PostgreSql;

// Inside the namespace, where they are asked before DotGram.Sql's own Expression, Query and Statement.
using Expression = DotGram.Sql.Ast.Expression;

/// <summary>
/// How <c>PostgreSql.gram</c> makes the common tree out of what it read: the words and lists a rule
/// captured, turned into nodes. Pure factories: they read no host state, so a construction run for a
/// derivation the reading then gives up leaves nothing behind.
/// </summary>
static class Nodes
{
	// ── Names ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// A Unicode name, <c>U&amp;"a"</c>, and the escape character its <c>UESCAPE 'c'</c> named, where one did:
	/// the name as written up to its closing quote, and the character out of the string's quotes.
	/// </summary>
	public static Identifier UnicodeIdentifier(string text, string? escape)
	{
		return new Identifier(text, IdentifierStyle.UnicodeDelimited, EscapeCharacter(escape));
	}

	/// <summary>A Unicode string, <c>U&amp;'a'</c>, and the escape character its <c>UESCAPE 'c'</c> named: the text from its first quote.</summary>
	public static LiteralValue UnicodeString(string text, string? escape)
	{
		return new LiteralValue.String(Quoted(text), StringLiteralKind.Unicode, null, EscapeCharacter(escape));
	}

	/// <summary>A string of a kind its first letter says, <c>E'a'</c> or <c>N'a'</c>: the text from its first quote, as the standard's reading keeps it.</summary>
	public static LiteralValue StringLiteral(string text, StringLiteralKind kind)
	{
		return new LiteralValue.String(Quoted(text), kind);
	}

	/// <summary>A string of bits, <c>B'0101'</c> or <c>X'1FF'</c>: the text from its first quote, and which letter began it.</summary>
	public static LiteralValue BitStringLiteral(string text)
	{
		return new LiteralValue.BitString(Quoted(text)) { Hexadecimal = (text[0] | 0x20) == 'x' };
	}

	/// <summary>The text from its first quote on.</summary>
	static string Quoted(string text)
	{
		return text.Substring(text.IndexOf('\''));
	}

	/// <summary>The character inside a one-character string, <c>'!'</c> or <c>E'!'</c>, or null where no string was written.</summary>
	static char? EscapeCharacter(string? quoted)
	{
		return quoted is null ? null : quoted[quoted.IndexOf('\'') + 1];
	}

	/// <summary>
	/// Whether a string names an escape character as the server allows one: exactly one character, and none of a hexit,
	/// <c>+</c>, a quote of either kind or a space.
	/// </summary>
	public static bool IsEscape(string quoted)
	{
		var body = quoted.Substring(quoted.IndexOf('\'') + 1, quoted.Length - quoted.IndexOf('\'') - 2);

		if (body.Length != 1)
			return false;

		var c = body[0];

		return !Uri.IsHexDigit(c) && c != '+' && c != '\'' && c != '"' && !char.IsWhiteSpace(c);
	}

	/// <summary>A name and the parts after it, as many as were read.</summary>
	public static QualifiedName Chain(Identifier first, Identifier[]? rest)
	{
		return new QualifiedName(first, rest);
	}

	// ── Literals ───────────────────────────────────────────────────────────────

	/// <summary>
	/// A number as written, its kind read from its text: an integer in one of four radixes, a decimal with a
	/// point, or an approximate one with an exponent.
	/// </summary>
	public static LiteralValue Number(string text)
	{
		var kind = text.Length > 1 && text[0] == '0'
			? text[1] switch
			{
				'x' or 'X' => NumericLiteralKind.HexInteger,
				'o' or 'O' => NumericLiteralKind.OctalInteger,
				'b' or 'B' => NumericLiteralKind.BinaryInteger,
				_          => Decimal(text),
			}
			: Decimal(text);

		return new LiteralValue.Numeric(text, kind);
	}

	/// <summary>A decimal number's kind: approximate with an exponent, decimal with a point, an integer otherwise.</summary>
	static NumericLiteralKind Decimal(string text)
	{
		if (text.IndexOfAny(['e', 'E']) >= 0)
			return NumericLiteralKind.Approximate;

		return text.IndexOf('.') >= 0 ? NumericLiteralKind.Decimal : NumericLiteralKind.DecimalInteger;
	}

	/// <summary>
	/// A positional parameter, <c>$1</c>: its number, read from the text after the sign. Leading zeros are the number's,
	/// not the tree's — <c>$01</c> is parameter 1 and is written back as <c>$1</c>, the one normalisation this reading makes.
	/// </summary>
	public static Expression Parameter(string text)
	{
		return new Expression.Parameter(ParameterKind.Positional) { Ordinal = int.Parse(text.AsSpan(1), System.Globalization.CultureInfo.InvariantCulture) };
	}

	/// <summary>Whether a parameter's number is one the server can hold ("parameter number too large" otherwise).</summary>
	public static bool FitsParameter(string text)
	{
		return int.TryParse(text.AsSpan(1), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out _);
	}
}
