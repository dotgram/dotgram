using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Handwritten;

// The six levels SqlStandardParser publishes — a value, a data type, an expression, a search condition, a
// statement and a text of statements — read the way the grammar reads them, so that the generated parser's
// entries have a yardstick of their own. A data type and a search condition are the productions of the
// same names, published with the rest.
partial class HandSqlStandard
{
	// ── The levels ─────────────────────────────────────────────────────────────

	/// <summary>Reads the whole input as a value: a <c>&lt;literal&gt;</c>, or <c>NULL</c>.</summary>
	public static LiteralValue ParseValue(string input)
	{
		return TryParseValue(input, out var value) ? value : throw Refused(input, "value");
	}

	/// <summary>Reads the whole input as a value, answering rather than throwing.</summary>
	public static bool TryParseValue(string input, out LiteralValue value)
	{
		if (TryParseLiteral(input, out value))
			return true;

		var cursor = new SqlCursor(input);

		if (cursor.Take(SqlWord.Null) && cursor.AtEnd)
		{
			value = new LiteralValue.Null();

			return true;
		}

		value = null!;

		return false;
	}

	/// <summary>Reads the whole input as an expression, a <c>&lt;value expression&gt;</c>.</summary>
	public static Expression ParseExpression(string input)
	{
		return TryParseExpression(input, out var value) ? value : throw Refused(input, "expression");
	}

	/// <summary>Reads the whole input as an expression, answering rather than throwing.</summary>
	public static bool TryParseExpression(string input, out Expression value)
	{
		return TryParseValueExpression(input, out value);
	}

	/// <summary>Reads the whole input as one statement of any kind, its semicolon optional.</summary>
	public static Statement ParseStatement(string input)
	{
		return TryParseStatement(input, out var value) ? value : throw Refused(input, "statement");
	}

	/// <summary>Reads the whole input as one statement of any kind, answering rather than throwing.</summary>
	public static bool TryParseStatement(string input, out Statement value)
	{
		var cursor = new SqlCursor(input);

		if (SqlStatement(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	/// <summary>Reads the whole input as statements, each ended by a semicolon or by the end of the text.</summary>
	public static Statement[] ParseSql(string input)
	{
		return TryParseSql(input, out var value) ? value : throw Refused(input, "SQL text");
	}

	/// <summary>Reads the whole input as statements, answering rather than throwing.</summary>
	public static bool TryParseSql(string input, out Statement[] value)
	{
		var cursor     = new SqlCursor(input);
		var statements = new List<Statement>();

		while (!cursor.AtEnd)
		{
			if (!SqlStatement(ref cursor, out var statement))
			{
				value = null!;

				return false;
			}

			statements.Add(statement);
		}

		value = [.. statements];

		return true;
	}

	// ── A statement of any kind ────────────────────────────────────────────────

	/// <summary>A statement, ended by its semicolon or by the end of the text.</summary>
	static bool SqlStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		if (SqlStatementBody(ref cursor, out statement) && (cursor.Take(SqlTokenKind.Semicolon) || cursor.AtEnd))
			return true;

		cursor    = save;
		statement = null!;

		return false;
	}

	/// <summary>
	/// A query, as a cursor specification, then the executable statements, then a temporary table's
	/// declaration: the order of the grammar, which asks the query before the single-row <c>SELECT … INTO</c>.
	/// </summary>
	static bool SqlStatementBody(ref SqlCursor cursor, out Statement statement)
	{
		if (CursorSpecification(ref cursor, out var query))
		{
			statement = query;

			return true;
		}

		if (SQLExecutableStatement(ref cursor, out statement))
			return true;

		if (TemporaryTableDeclaration(ref cursor, out var table))
		{
			statement = table;

			return true;
		}

		statement = null!;

		return false;
	}
}
