using System;
using System.Collections.Generic;
using System.Globalization;

using DotGram.Sql.Ast;

namespace DotGram.Handwritten;

/// <summary>
/// ISO/IEC 9075-2:2023, SQL/Foundation, read by hand: the parser <c>SqlStandardParser</c> would be
/// if a person had written it instead of a grammar.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it is for.</b> It is the yardstick the generated parser is measured against, and the
/// target it is optimized towards: a number that says the generated parser costs so much is only
/// worth having if the other side of the ratio did the same work. So this reads the same language,
/// builds the same tree — <c>DotGram.Sql.Ast</c>, node for node — and refuses what the generated
/// parser refuses. Where the two disagree, one of them is wrong, and the comparison harness says
/// which input told them apart before anything is timed.
/// </para>
/// <para>
/// <b>The same work, not the same shape.</b> It reads characters where the generated parser reads
/// characters: there is no array of tokens, and nothing of what was passed is kept
/// (<see cref="SqlCursor"/>). What separates the two numbers is how the reading is arranged, which
/// is the thing being measured.
/// </para>
/// <para>
/// <b>The rules are the standard's.</b> A method is named after the production it reads —
/// <c>&lt;query expression&gt;</c> is <c>QueryExpression</c> — so that the grammar and this can be
/// read side by side, and a difference between them is a difference in one line rather than in the
/// shape of a file. A rule that fails leaves the reading where it found it.
/// </para>
/// </remarks>
public static partial class HandSqlStandard
{
	// ── §5.3 Literals ──────────────────────────────────────────────────────────

	/// <summary>Reads the whole input as a <c>&lt;literal&gt;</c>.</summary>
	public static LiteralValue ParseLiteral(string input)
	{
		if (TryParseLiteral(input, out var value))
			return value;

		throw Refused(input, "literal");
	}

	/// <summary>Reads the whole input as a <c>&lt;literal&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseLiteral(string input, out LiteralValue value)
	{
		var cursor = new SqlCursor(input);

		if (Literal(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	/// <summary>Reads the whole input as an <c>&lt;unsigned literal&gt;</c>.</summary>
	public static LiteralValue ParseUnsignedLiteral(string input)
	{
		if (TryParseUnsignedLiteral(input, out var value))
			return value;

		throw Refused(input, "unsigned literal");
	}

	/// <summary>Reads the whole input as an <c>&lt;unsigned literal&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseUnsignedLiteral(string input, out LiteralValue value)
	{
		var cursor = new SqlCursor(input);

		if (UnsignedLiteral(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	// ── §5.4 Names and identifiers ─────────────────────────────────────────────

	/// <summary>Reads the whole input as an <c>&lt;identifier&gt;</c>.</summary>
	public static Identifier ParseIdentifier(string input)
	{
		if (TryParseIdentifier(input, out var value))
			return value;

		throw Refused(input, "identifier");
	}

	/// <summary>Reads the whole input as an <c>&lt;identifier&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseIdentifier(string input, out Identifier value)
	{
		var cursor = new SqlCursor(input);

		if (Identifier(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	/// <summary>Reads the whole input as an <c>&lt;identifier chain&gt;</c>.</summary>
	public static QualifiedName ParseIdentifierChain(string input)
	{
		if (TryParseIdentifierChain(input, out var value))
			return value;

		throw Refused(input, "identifier chain");
	}

	/// <summary>Reads the whole input as an <c>&lt;identifier chain&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseIdentifierChain(string input, out QualifiedName value)
	{
		var cursor = new SqlCursor(input);

		if (IdentifierChain(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	/// <summary>Reads the whole input as a <c>&lt;column reference&gt;</c>.</summary>
	public static Expression ParseColumnReference(string input)
	{
		if (TryParseColumnReference(input, out var value))
			return value;

		throw Refused(input, "column reference");
	}

	/// <summary>Reads the whole input as a <c>&lt;column reference&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseColumnReference(string input, out Expression value)
	{
		var cursor = new SqlCursor(input);

		if (ColumnReference(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	/// <summary>Reads the whole input as a <c>&lt;table name&gt;</c>.</summary>
	public static QualifiedName ParseTableName(string input)
	{
		if (TryParseTableName(input, out var value))
			return value;

		throw Refused(input, "table name");
	}

	/// <summary>Reads the whole input as a <c>&lt;table name&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseTableName(string input, out QualifiedName value)
	{
		var cursor = new SqlCursor(input);

		if (TableName(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	/// <summary>Reads the whole input as a <c>&lt;schema name&gt;</c>.</summary>
	public static QualifiedName ParseSchemaName(string input)
	{
		if (TryParseSchemaName(input, out var value))
			return value;

		throw Refused(input, "schema name");
	}

	/// <summary>Reads the whole input as a <c>&lt;schema name&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseSchemaName(string input, out QualifiedName value)
	{
		var cursor = new SqlCursor(input);

		if (SchemaName(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	// ── §6.1 Data types ────────────────────────────────────────────────────────

	/// <summary>Reads the whole input as a <c>&lt;data type&gt;</c>.</summary>
	public static DataType ParseDataType(string input)
	{
		if (TryParseDataType(input, out var value))
			return value;

		throw Refused(input, "data type");
	}

	/// <summary>Reads the whole input as a <c>&lt;data type&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseDataType(string input, out DataType value)
	{
		var cursor = new SqlCursor(input);

		if (DataType(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	static FormatException Refused(string input, string production) =>
		new("Input does not match '" + production + "': " + input);

	// ── §5.4 Names and identifiers ─────────────────────────────────────────────

	/// <summary>
	/// <c>&lt;identifier&gt;</c>. §5.2 cuts a text into its longest tokens, so a name is one token
	/// and no part of one is a name of its own.
	/// </summary>
	static bool Identifier(ref SqlCursor cursor, out Identifier name)
	{
		switch (cursor.Kind)
		{
			case SqlTokenKind.Word:
				// §5.4: a regular identifier is no reserved word.
				if (cursor.Word != SqlWord.Name)
					break;

				name = new Identifier(cursor.TextOf(cursor.Token));
				cursor.Take();

				return true;

			case SqlTokenKind.Delimited:
				name = new Identifier(cursor.TextOf(cursor.Token), IdentifierStyle.Delimited);
				cursor.Take();

				return true;

			case SqlTokenKind.UnicodeName:
			{
				// The name as written up to its closing quote, and the escape where one was named.
				var token = cursor.Token;

				name = new Identifier(
					cursor.Text.Substring(token.Start, token.Body - token.Start),
					IdentifierStyle.UnicodeDelimited,
					token.End > token.Body ? '\\' : null);

				cursor.Take();

				return true;
			}
		}

		name = null!;

		return false;
	}

	/// <summary><c>&lt;identifier chain&gt;</c>: a name, and the names after it.</summary>
	static bool IdentifierChain(ref SqlCursor cursor, out QualifiedName name)
	{
		name = null!;

		if (!Identifier(ref cursor, out var first))
			return false;

		List<Identifier>? rest = null;

		while (true)
		{
			var save = cursor;

			if (!cursor.Take(SqlTokenKind.Dot))
				break;

			if (!Identifier(ref cursor, out var next))
			{
				cursor = save;

				break;
			}

			(rest ??= []).Add(next);
		}

		name = Chain(first, rest);

		return true;
	}

	/// <summary>
	/// <c>&lt;table name&gt;</c>, which is a <c>&lt;local or schema qualified name&gt;</c>: a name,
	/// a schema's before it and a catalog's before that, or <c>MODULE</c> for a module's own.
	/// </summary>
	static bool TableName(ref SqlCursor cursor, out QualifiedName name)
	{
		name = null!;

		if (cursor.Word == SqlWord.Module)
		{
			var save   = cursor;
			var module = cursor.TextOf(cursor.Token);

			cursor.Take();

			if (cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var qualified))
			{
				name = new QualifiedName([new Identifier(module), qualified]);

				return true;
			}

			cursor = save;

			return false;
		}

		return Names(ref cursor, 3, out name);
	}

	/// <summary><c>&lt;schema name&gt;</c>: a catalog's name may stand before it.</summary>
	static bool SchemaName(ref SqlCursor cursor, out QualifiedName name) => Names(ref cursor, 2, out name);

	/// <summary>
	/// <c>&lt;schema qualified name&gt;</c>, and every name the BNF nests the same way: at most
	/// <paramref name="most"/> parts, and no more even where another period follows.
	/// </summary>
	static bool Names(ref SqlCursor cursor, int most, out QualifiedName name)
	{
		name = null!;

		if (!Identifier(ref cursor, out var first))
			return false;

		List<Identifier>? rest = null;

		for (var taken = 1; taken < most; taken++)
		{
			var save = cursor;

			if (!cursor.Take(SqlTokenKind.Dot))
				break;

			if (!Identifier(ref cursor, out var next))
			{
				cursor = save;

				break;
			}

			(rest ??= []).Add(next);
		}

		name = Chain(first, rest);

		return true;
	}

	/// <summary><c>&lt;column reference&gt;</c>: a chain of names, or a module's column.</summary>
	static bool ColumnReference(ref SqlCursor cursor, out Expression value)
	{
		value = null!;

		if (cursor.Word == SqlWord.Module)
		{
			var save   = cursor;
			var module = cursor.TextOf(cursor.Token);

			cursor.Take();

			if (cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var qualified) &&
				cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var column))
			{
				value = new Expression.Reference(new QualifiedName([new Identifier(module), qualified, column]));

				return true;
			}

			cursor = save;

			return false;
		}

		if (!IdentifierChain(ref cursor, out var name))
			return false;

		value = new Expression.Reference(name);

		return true;
	}

	static QualifiedName Chain(Identifier first, List<Identifier>? rest)
	{
		if (rest is not { Count: > 0 })
			return new QualifiedName([first]);

		var parts = new Identifier[rest.Count + 1];

		parts[0] = first;
		rest.CopyTo(parts, 1);

		return new QualifiedName(parts);
	}

	// ── The numbers a production writes out ────────────────────────────────────

	/// <summary>An <c>&lt;unsigned integer&gt;</c> as the BNF writes one: in any radix, with underscores.</summary>
	static long Long(ReadOnlySpan<char> text)
	{
		var radix = 10;

		if (text.Length > 2 && text[0] == '0')
		{
			switch (text[1] | 0x20)
			{
				case 'x': radix = 16; text = text[2..]; break;
				case 'o': radix = 8;  text = text[2..]; break;
				case 'b': radix = 2;  text = text[2..]; break;
			}
		}

		var value = 0L;

		foreach (var digit in text)
		{
			if (digit == '_')
				continue;

			value = value * radix + ((uint)(digit - '0') <= 9 ? digit - '0' : (digit | 0x20) - 'a' + 10);
		}

		return value;
	}

	static int Integer(ReadOnlySpan<char> text) => (int)Long(text);
}
