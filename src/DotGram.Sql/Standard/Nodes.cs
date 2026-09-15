using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Sql.Standard;

/// <summary>
/// How <c>SqlStandard.gram</c> makes the SQL:2023 tree out of what it read: the words and lists a
/// rule captured, turned into nodes (docs/design/sql-ast.md).
/// </summary>
static class Nodes
{
	// ── Names ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// A Unicode delimited identifier, <c>U&amp;"a"</c> or <c>U&amp;"a!0041" UESCAPE '!'</c>: the name as written
	/// up to its closing quote, and the escape character where one was named.
	/// </summary>
	public static Identifier UnicodeIdentifier(string text)
	{
		var close = text.LastIndexOf('"');
		var name  = text.Substring(0, close + 1);
		var rest  = text.Substring(close + 1);
		var quote = rest.IndexOf('\'');

		return new Identifier(name, IdentifierStyle.UnicodeDelimited, quote >= 0 && quote + 1 < rest.Length ? rest[quote + 1] : null);
	}

	/// <summary>A name made of a word read as a key word, <c>MODULE</c>, and the names after it.</summary>
	public static QualifiedName Named(string word, params Identifier[] rest)
	{
		var parts = new Identifier[rest.Length + 1];

		parts[0] = new Identifier(word);
		Array.Copy(rest, 0, parts, 1, rest.Length);

		return new QualifiedName(parts);
	}

	/// <summary>A name and the parts after it, as many as were read.</summary>
	public static QualifiedName Chain(Identifier first, Identifier[]? rest)
	{
		if (rest is not { Length: > 0 })
			return new QualifiedName([first]);

		var parts = new Identifier[rest.Length + 1];

		parts[0] = first;
		Array.Copy(rest, 0, parts, 1, rest.Length);

		return new QualifiedName(parts);
	}

	/// <summary>A name and one optional part after it.</summary>
	public static QualifiedName Chain(Identifier first, Identifier? second) =>
		second is null ? new QualifiedName([first]) : new QualifiedName([first, second]);

	/// <summary>A character set's name: its qualifiers, and the SQL language identifier after them.</summary>
	public static CharacterSetName CharacterSet(Identifier[]? qualifiers, string name)
	{
		var parts = new Identifier[(qualifiers?.Length ?? 0) + 1];

		qualifiers?.CopyTo(parts, 0);
		parts[parts.Length - 1] = new Identifier(name);

		return new CharacterSetName(new QualifiedName(parts));
	}

	// ── Lists ──────────────────────────────────────────────────────────────────

	/// <summary>The first of a list and the rest of it, as one list in the order written.</summary>
	public static IReadOnlyList<T> List<T>(T first, T[]? rest)
	{
		if (rest is not { Length: > 0 })
			return [first];

		var all = new T[rest.Length + 1];

		all[0] = first;
		Array.Copy(rest, 0, all, 1, rest.Length);

		return all;
	}

	// ── §5.3 Literals ──────────────────────────────────────────────────────────

	/// <summary>A number as written, its sign in front where one was, and which of the numeric literals it is.</summary>
	public static LiteralValue NumericLiteral(string text)
	{
		var body = text.Length > 0 && (text[0] == '+' || text[0] == '-') ? text.Substring(1) : text;
		var kind =
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'x' ? NumericLiteralKind.HexInteger :
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'o' ? NumericLiteralKind.OctalInteger :
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'b' ? NumericLiteralKind.BinaryInteger :
			body.IndexOf('e') >= 0 || body.IndexOf('E') >= 0 ? NumericLiteralKind.Approximate :
			body.IndexOf('.') >= 0 ? NumericLiteralKind.Decimal :
			NumericLiteralKind.DecimalInteger;

		return new LiteralValue.Numeric(text, kind);
	}

	/// <summary>
	/// A character string literal of any kind: its introducer's character set, what was written from
	/// its first quote to its last, and a Unicode literal's escape character.
	/// </summary>
	public static LiteralValue StringLiteral(string text, StringLiteralKind kind)
	{
		var quote = text.IndexOf('\'');
		var start = text[0] == '_' ? 1 : -1;

		CharacterSetName? characterSet = null;

		if (start == 1)
		{
			var introduced = text.Substring(1, quote - 1);

			if (kind == StringLiteralKind.Unicode)
				introduced = introduced.Substring(0, introduced.Length - 2);

			characterSet = CharacterSet(introduced);
		}

		var escaped = kind == StringLiteralKind.Unicode && text.EndsWith(DefaultEscape, StringComparison.OrdinalIgnoreCase);
		var end     = escaped ? text.Length - DefaultEscape.Length : text.Length;
		var literal = text.Substring(quote, end - quote);

		return new LiteralValue.String(literal, kind, characterSet, escaped ? '\\' : null);
	}

	/// <summary>
	/// The one escape specifier the BNF reads, a part of the token with nothing inside it: the Syntax
	/// Rules choose another character, and a separator ends the token.
	/// </summary>
	const string DefaultEscape = "UESCAPE'\\'";

	/// <summary>A binary string literal: what was written from its first quote to its last.</summary>
	public static LiteralValue BinaryLiteral(string text) =>
		new LiteralValue.Binary(text.Substring(text.IndexOf('\'')));

	public static LiteralValue BooleanLiteral(string word) =>
		new LiteralValue.Boolean((word[0] | 0x20) switch
		{
			't' => Ast.BooleanLiteral.True,
			'f' => Ast.BooleanLiteral.False,
			_   => Ast.BooleanLiteral.Unknown,
		});

	public static UnaryOperator? SignOf(string? sign) =>
		sign switch
		{
			"+" => UnaryOperator.Plus,
			"-" => UnaryOperator.Minus,
			_   => null,
		};

	/// <summary>A character set's name as an introducer writes it, the dots between its parts.</summary>
	static CharacterSetName CharacterSet(string dotted)
	{
		var parts = dotted.Split('.');
		var names = new Identifier[parts.Length];

		for (var at = 0; at < parts.Length; at++)
			names[at] = new Identifier(parts[at], parts[at].Length > 0 && parts[at][0] == '"' ? IdentifierStyle.Delimited : IdentifierStyle.Regular);

		return new CharacterSetName(new QualifiedName(names));
	}

	// ── Numbers inside a production ────────────────────────────────────────────

	/// <summary>An unsigned integer as the BNF writes one — in any radix, with underscores — or null.</summary>
	public static int? Integer(string? text) =>
		text is null ? null : (int)Long(text);

	static long Long(string text)
	{
		text = text.Replace("_", "");

		if (text.Length > 2 && text[0] == '0')
		{
			switch (text[1] | 0x20)
			{
				case 'x': return Convert.ToInt64(text.Substring(2), 16);
				case 'o': return Convert.ToInt64(text.Substring(2), 8);
				case 'b': return Convert.ToInt64(text.Substring(2), 2);
			}
		}

		return long.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
	}

	// ── §10.1 Interval qualifier ───────────────────────────────────────────────

	public static DateTimeField Field(string word) =>
		word.ToUpperInvariant() switch
		{
			"YEAR"   => DateTimeField.Year,
			"MONTH"  => DateTimeField.Month,
			"DAY"    => DateTimeField.Day,
			"HOUR"   => DateTimeField.Hour,
			"MINUTE" => DateTimeField.Minute,
			_        => DateTimeField.Second,
		};

	/// <summary>A start field and an end field: the start's precision, and the end's fractional one.</summary>
	public static IntervalQualifier Qualifier(IntervalQualifier start, IntervalQualifier end) =>
		new(start.Start, start.LeadingPrecision, end.Start, end.FractionalPrecision);

	// ── §6.1 Data types ────────────────────────────────────────────────────────

	/// <summary>A collection type's suffix: `ARRAY` with its cardinality and its brackets, or `MULTISET`.</summary>
	public readonly record struct Suffix(bool Multiset, int? Cardinality, bool Trigraphs);

	public static Suffix ArraySuffix(string? bracket, string? cardinality) =>
		new(false, Integer(cardinality), bracket == "??(");

	public static Suffix MultisetSuffix() => new(true, null, false);

	/// <summary>A type and the collection suffixes after it, each wrapping what stands before it.</summary>
	public static DataType Collected(DataType type, Suffix[]? suffixes)
	{
		foreach (var suffix in suffixes ?? [])
			type = suffix.Multiset ? new DataType.Multiset(type) : new DataType.Array(type, suffix.Cardinality, suffix.Trigraphs);

		return type;
	}

	/// <summary>A length as a character string type writes it: a number or a large object's size, and its units.</summary>
	public readonly record struct Length(int? Characters, LargeObjectSize? LargeObject, LengthUnit? Unit);

	public static Length LengthOf(string number, string? units) => new(Integer(number), null, Units(units));

	public static Length LengthOf(LargeObjectSize size, string? units) => new(null, size, Units(units));

	public static LargeObjectSize Size(string number, string? multiplier) =>
		new(Long(number), multiplier is null ? null : multiplier[0]);

	static LengthUnit? Units(string? units) =>
		units is null ? null : (units[0] | 0x20) == 'c' ? LengthUnit.Characters : LengthUnit.Octets;

	/// <summary>A character string type by its spelling — the words written, one space between them — and its length.</summary>
	public static DataType Character(string spelling, Length? length) =>
		new DataType.Character(
			string.Join(" ", spelling.ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)) switch
			{
				"CHARACTER"                        => CharacterTypeKind.Character,
				"CHAR"                             => CharacterTypeKind.Char,
				"CHARACTER VARYING"                => CharacterTypeKind.CharacterVarying,
				"CHAR VARYING"                     => CharacterTypeKind.CharVarying,
				"VARCHAR"                          => CharacterTypeKind.Varchar,
				"CHARACTER LARGE OBJECT"           => CharacterTypeKind.CharacterLargeObject,
				"CHAR LARGE OBJECT"                => CharacterTypeKind.CharLargeObject,
				"CLOB"                             => CharacterTypeKind.Clob,
				"NATIONAL CHARACTER"               => CharacterTypeKind.NationalCharacter,
				"NATIONAL CHAR"                    => CharacterTypeKind.NationalChar,
				"NCHAR"                            => CharacterTypeKind.Nchar,
				"NATIONAL CHARACTER VARYING"       => CharacterTypeKind.NationalCharacterVarying,
				"NATIONAL CHAR VARYING"            => CharacterTypeKind.NationalCharVarying,
				"NCHAR VARYING"                    => CharacterTypeKind.NcharVarying,
				"NATIONAL CHARACTER LARGE OBJECT"  => CharacterTypeKind.NationalCharacterLargeObject,
				"NCHAR LARGE OBJECT"               => CharacterTypeKind.NcharLargeObject,
				_                                  => CharacterTypeKind.Nclob,
			},
			length?.Characters, length?.Unit, LargeObject: length?.LargeObject);

	/// <summary>A character string type with the character set and the collation written after it.</summary>
	public static DataType Characters(DataType type, CharacterSetName? characterSet, CollationName? collation) =>
		characterSet is null && collation is null ? type : ((DataType.Character)type) with { CharacterSet = characterSet, Collation = collation };

	public static DataType Binary(string spelling, string? length, LargeObjectSize? size) =>
		new DataType.Binary(
			spelling switch
			{
				"BINARY"              => BinaryTypeKind.Binary,
				"BINARY VARYING"      => BinaryTypeKind.BinaryVarying,
				"VARBINARY"           => BinaryTypeKind.Varbinary,
				"BINARY LARGE OBJECT" => BinaryTypeKind.BinaryLargeObject,
				_                     => BinaryTypeKind.Blob,
			},
			Integer(length), size);

	public static DataType NumericType(string spelling, string? precision, string? scale) =>
		new DataType.Numeric(
			spelling.ToUpperInvariant() switch
			{
				"NUMERIC"          => NumericTypeKind.Numeric,
				"DECIMAL"          => NumericTypeKind.Decimal,
				"DEC"              => NumericTypeKind.Dec,
				"SMALLINT"         => NumericTypeKind.SmallInt,
				"INTEGER"          => NumericTypeKind.Integer,
				"INT"              => NumericTypeKind.Int,
				"BIGINT"           => NumericTypeKind.BigInt,
				"FLOAT"            => NumericTypeKind.Float,
				"REAL"             => NumericTypeKind.Real,
				"DOUBLE PRECISION" => NumericTypeKind.DoublePrecision,
				_                  => NumericTypeKind.DecFloat,
			},
			Integer(precision), Integer(scale));

	public static DataType DateTime(string word, string? precision, string? zone) =>
		new DataType.DateTime(
			(word.Length > 4 ? DateTimeTypeKind.Timestamp : DateTimeTypeKind.Time),
			Integer(precision),
			zone is null ? null : (zone.Length > 4 ? TimeZoneMode.Without : TimeZoneMode.With));
}
