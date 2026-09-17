using System;
using System.Reflection;
using System.Text;

using DotGram.Handwritten;
using DotGram.Sql.Ast;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside the namespace, where it is asked before DotGram.Sql's own Expression — the tree T-SQL
// builds, which a using directive above the namespace would lose to.
using Expression = DotGram.Sql.Ast.Expression;

/// <summary>
/// Both readings of ISO/IEC 9075-2:2023 at once: the generated <see cref="SqlStandardParser"/> and
/// the handwritten <see cref="HandSqlStandard"/>, which must answer the same.
/// </summary>
/// <remarks>
/// <para>
/// The handwritten parser is what the generated one is measured against, and a ratio between two
/// parsers that read different languages says nothing. So the suite reads through here: every row
/// already written for the generated parser is put to both, and a row that tells them apart fails
/// where it stands rather than in a benchmark nobody ran.
/// </para>
/// <para>
/// What comes back is the generated parser's own answer, so the assertions after the call are
/// about the parser the tests were written for. The handwritten one is held to it, not the other
/// way round — it is the generated parser that the BNF oracle has been asking, line by line.
/// </para>
/// <para>
/// Chapter by chapter: a production the handwritten parser does not read yet is still called
/// through <see cref="SqlStandardParser"/> directly, and moves here when it does.
/// </para>
/// </remarks>
static class Both
{
	// ── §5.3 Literals ──────────────────────────────────────────────────────────

	public static SqlStandardParser.Match<LiteralValue> TryParseLiteral(string input)
	{
		var read = SqlStandardParser.TryParseLiteral(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseLiteral(input, out var hand), hand);

		return read;
	}

	public static LiteralValue ParseLiteral(string input) => TryParseLiteral(input).IsSuccess
		? SqlStandardParser.ParseLiteral(input)
		: throw Refused(input, "literal");

	public static SqlStandardParser.Match<LiteralValue> TryParseUnsignedLiteral(string input)
	{
		var read = SqlStandardParser.TryParseUnsignedLiteral(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseUnsignedLiteral(input, out var hand), hand);

		return read;
	}

	// ── §5.4 Names and identifiers ─────────────────────────────────────────────

	public static SqlStandardParser.Match<Identifier> TryParseIdentifier(string input)
	{
		var read = SqlStandardParser.TryParseIdentifier(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseIdentifier(input, out var hand), hand);

		return read;
	}

	public static Identifier ParseIdentifier(string input) => TryParseIdentifier(input).IsSuccess
		? SqlStandardParser.ParseIdentifier(input)
		: throw Refused(input, "identifier");

	public static SqlStandardParser.Match<QualifiedName> TryParseIdentifierChain(string input)
	{
		var read = SqlStandardParser.TryParseIdentifierChain(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseIdentifierChain(input, out var hand), hand);

		return read;
	}

	public static QualifiedName ParseIdentifierChain(string input) => TryParseIdentifierChain(input).IsSuccess
		? SqlStandardParser.ParseIdentifierChain(input)
		: throw Refused(input, "identifier chain");

	public static SqlStandardParser.Match<Expression> TryParseColumnReference(string input)
	{
		var read = SqlStandardParser.TryParseColumnReference(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseColumnReference(input, out var hand), hand);

		return read;
	}

	public static Expression ParseColumnReference(string input) => TryParseColumnReference(input).IsSuccess
		? SqlStandardParser.ParseColumnReference(input)
		: throw Refused(input, "column reference");

	public static SqlStandardParser.Match<QualifiedName> TryParseTableName(string input)
	{
		var read = SqlStandardParser.TryParseTableName(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseTableName(input, out var hand), hand);

		return read;
	}

	public static QualifiedName ParseTableName(string input) => TryParseTableName(input).IsSuccess
		? SqlStandardParser.ParseTableName(input)
		: throw Refused(input, "table name");

	public static SqlStandardParser.Match<QualifiedName> TryParseSchemaName(string input)
	{
		var read = SqlStandardParser.TryParseSchemaName(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSchemaName(input, out var hand), hand);

		return read;
	}

	// ── §6.1 Data types ────────────────────────────────────────────────────────

	public static SqlStandardParser.Match<DataType> TryParseDataType(string input)
	{
		var read = SqlStandardParser.TryParseDataType(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDataType(input, out var hand), hand);

		return read;
	}

	public static DataType ParseDataType(string input) => TryParseDataType(input).IsSuccess
		? SqlStandardParser.ParseDataType(input)
		: throw Refused(input, "data type");

	// ── Holding one to the other ───────────────────────────────────────────────

	/// <summary>
	/// That the two read the same input the same way: the same verdict, and where both read it,
	/// the same tree down to the last property.
	/// </summary>
	static void Agree(string input, bool read, object? tree, bool hand, object? handTree)
	{
		Assert.True(
			read == hand,
			(read ? "The generated parser reads what the handwritten one refuses: " : "The handwritten parser reads what the generated one refuses: ") + input);

		if (!read)
			return;

		var expected = Dump(tree);
		var actual   = Dump(handTree);

		Assert.True(expected == actual, "The two parsers built different trees for: " + input + "\n  generated: " + expected + "\n  by hand:   " + actual);
	}

	static FormatException Refused(string input, string production) =>
		new("Input does not match '" + production + "': " + input);

	/// <summary>A tree as its properties, which is what two trees are compared as.</summary>
	static string Dump(object? node)
	{
		var text = new StringBuilder();

		Dump(text, node);

		return text.ToString();
	}

	static void Dump(StringBuilder text, object? node)
	{
		switch (node)
		{
			case null:
				text.Append("null");

				return;

			case string value:
				text.Append('"').Append(value).Append('"');

				return;

			case System.Collections.IEnumerable list:
				text.Append('[');

				foreach (var one in list)
				{
					Dump(text, one);
					text.Append(',');
				}

				text.Append(']');

				return;
		}

		var type = node.GetType();

		if (type.IsPrimitive || type.IsEnum)
		{
			text.Append(node);

			return;
		}

		text.Append(type.Name).Append('(');

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			// Where a node was written is no part of what it says, and only one of the two
			// parsers is asked to keep it.
			if (property.Name is "Span" or "EqualityContract" || property.GetIndexParameters().Length > 0)
				continue;

			text.Append(property.Name).Append('=');
			Dump(text, property.GetValue(node));
			text.Append(';');
		}

		text.Append(')');
	}
}
