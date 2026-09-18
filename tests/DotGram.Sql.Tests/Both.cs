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
using Statement = DotGram.Sql.Ast.Statement;

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

	// ── §6.28 Value expressions, §8 predicates ─────────────────────────────────

	public static SqlStandardParser.Match<Expression> TryParseValueExpression(string input)
	{
		var read = SqlStandardParser.TryParseValueExpression(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseValueExpression(input, out var hand), hand);

		return read;
	}

	public static Expression ParseValueExpression(string input) => TryParseValueExpression(input).IsSuccess
		? SqlStandardParser.ParseValueExpression(input)
		: throw Refused(input, "value expression");

	public static SqlStandardParser.Match<Expression> TryParseSearchCondition(string input)
	{
		var read = SqlStandardParser.TryParseSearchCondition(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSearchCondition(input, out var hand), hand);

		return read;
	}

	public static Expression ParseSearchCondition(string input) => TryParseSearchCondition(input).IsSuccess
		? SqlStandardParser.ParseSearchCondition(input)
		: throw Refused(input, "search condition");

	// ── §7 Query expressions ───────────────────────────────────────────────────

	public static SqlStandardParser.Match<Statement.Select> TryParseQueryExpression(string input)
	{
		var read = SqlStandardParser.TryParseQueryExpression(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseQueryExpression(input, out var hand), hand);

		return read;
	}

	public static Statement.Select ParseQueryExpression(string input) => TryParseQueryExpression(input).IsSuccess
		? SqlStandardParser.ParseQueryExpression(input)
		: throw Refused(input, "query expression");

	public static SqlStandardParser.Match<TableSource> TryParseTableReference(string input)
	{
		var read = SqlStandardParser.TryParseTableReference(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseTableReference(input, out var hand), hand);

		return read;
	}

	public static TableSource ParseTableReference(string input) => TryParseTableReference(input).IsSuccess
		? SqlStandardParser.ParseTableReference(input)
		: throw Refused(input, "table reference");

	// ── §14 Data change statements ─────────────────────────────────────────────

	public static SqlStandardParser.Match<Statement.Insert> TryParseInsertStatement(string input)
	{
		var read = SqlStandardParser.TryParseInsertStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseInsertStatement(input, out var hand), hand);

		return read;
	}

	public static Statement.Insert ParseInsertStatement(string input) => TryParseInsertStatement(input).IsSuccess
		? SqlStandardParser.ParseInsertStatement(input)
		: throw Refused(input, "insert statement");

	public static SqlStandardParser.Match<Statement.Update> TryParseUpdateStatementSearched(string input)
	{
		var read = SqlStandardParser.TryParseUpdateStatementSearched(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseUpdateStatementSearched(input, out var hand), hand);

		return read;
	}

	public static Statement.Update ParseUpdateStatementSearched(string input) => TryParseUpdateStatementSearched(input).IsSuccess
		? SqlStandardParser.ParseUpdateStatementSearched(input)
		: throw Refused(input, "update statement: searched");

	public static SqlStandardParser.Match<Statement.Update> TryParseUpdateStatementPositioned(string input)
	{
		var read = SqlStandardParser.TryParseUpdateStatementPositioned(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseUpdateStatementPositioned(input, out var hand), hand);

		return read;
	}

	public static Statement.Update ParseUpdateStatementPositioned(string input) => TryParseUpdateStatementPositioned(input).IsSuccess
		? SqlStandardParser.ParseUpdateStatementPositioned(input)
		: throw Refused(input, "update statement: positioned");

	public static SqlStandardParser.Match<Statement.Delete> TryParseDeleteStatementSearched(string input)
	{
		var read = SqlStandardParser.TryParseDeleteStatementSearched(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDeleteStatementSearched(input, out var hand), hand);

		return read;
	}

	public static Statement.Delete ParseDeleteStatementSearched(string input) => TryParseDeleteStatementSearched(input).IsSuccess
		? SqlStandardParser.ParseDeleteStatementSearched(input)
		: throw Refused(input, "delete statement: searched");

	public static SqlStandardParser.Match<Statement.Delete> TryParseDeleteStatementPositioned(string input)
	{
		var read = SqlStandardParser.TryParseDeleteStatementPositioned(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDeleteStatementPositioned(input, out var hand), hand);

		return read;
	}

	public static Statement.Delete ParseDeleteStatementPositioned(string input) => TryParseDeleteStatementPositioned(input).IsSuccess
		? SqlStandardParser.ParseDeleteStatementPositioned(input)
		: throw Refused(input, "delete statement: positioned");

	public static SqlStandardParser.Match<Statement.Merge> TryParseMergeStatement(string input)
	{
		var read = SqlStandardParser.TryParseMergeStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseMergeStatement(input, out var hand), hand);

		return read;
	}

	public static Statement.Merge ParseMergeStatement(string input) => TryParseMergeStatement(input).IsSuccess
		? SqlStandardParser.ParseMergeStatement(input)
		: throw Refused(input, "merge statement");

	public static SqlStandardParser.Match<Statement.TruncateTable> TryParseTruncateTableStatement(string input)
	{
		var read = SqlStandardParser.TryParseTruncateTableStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseTruncateTableStatement(input, out var hand), hand);

		return read;
	}

	public static Statement.TruncateTable ParseTruncateTableStatement(string input) => TryParseTruncateTableStatement(input).IsSuccess
		? SqlStandardParser.ParseTruncateTableStatement(input)
		: throw Refused(input, "truncate table statement");

	// ── §11, §12 and §14 to §23: the statements ────────────────────────────────

	public static SqlStandardParser.Match<Statement> TryParseSQLSchemaStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLSchemaStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLSchemaStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseSQLSchemaStatement(string input) => TryParseSQLSchemaStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLSchemaStatement(input)
		: throw Refused(input, "SQL schema statement");

	public static SqlStandardParser.Match<Statement> TryParseDirectSQLStatement(string input)
	{
		var read = SqlStandardParser.TryParseDirectSQLStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDirectSQLStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseDirectSQLStatement(string input) => TryParseDirectSQLStatement(input).IsSuccess
		? SqlStandardParser.ParseDirectSQLStatement(input)
		: throw Refused(input, "direct SQL statement");

	public static SqlStandardParser.Match<Statement> TryParseDirectSQLDataStatement(string input)
	{
		var read = SqlStandardParser.TryParseDirectSQLDataStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDirectSQLDataStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseDirectSQLDataStatement(string input) => TryParseDirectSQLDataStatement(input).IsSuccess
		? SqlStandardParser.ParseDirectSQLDataStatement(input)
		: throw Refused(input, "direct SQL data statement");

	public static SqlStandardParser.Match<Statement> TryParseSQLDataStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLDataStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLDataStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseSQLDataStatement(string input) => TryParseSQLDataStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLDataStatement(input)
		: throw Refused(input, "SQL data statement");

	public static SqlStandardParser.Match<Statement> TryParseSQLControlStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLControlStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLControlStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseSQLControlStatement(string input) => TryParseSQLControlStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLControlStatement(input)
		: throw Refused(input, "SQL control statement");

	public static SqlStandardParser.Match<Statement> TryParseSQLTransactionStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLTransactionStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLTransactionStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseSQLTransactionStatement(string input) => TryParseSQLTransactionStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLTransactionStatement(input)
		: throw Refused(input, "SQL transaction statement");

	public static SqlStandardParser.Match<Statement> TryParseSQLConnectionStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLConnectionStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLConnectionStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseSQLConnectionStatement(string input) => TryParseSQLConnectionStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLConnectionStatement(input)
		: throw Refused(input, "SQL connection statement");

	public static SqlStandardParser.Match<Statement> TryParseSQLSessionStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLSessionStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLSessionStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseSQLSessionStatement(string input) => TryParseSQLSessionStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLSessionStatement(input)
		: throw Refused(input, "SQL session statement");

	public static SqlStandardParser.Match<Statement.GetDiagnostics> TryParseSQLDiagnosticsStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLDiagnosticsStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLDiagnosticsStatement(input, out var hand), hand);

		return read;
	}

	public static Statement.GetDiagnostics ParseSQLDiagnosticsStatement(string input) => TryParseSQLDiagnosticsStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLDiagnosticsStatement(input)
		: throw Refused(input, "SQL diagnostics statement");

	public static SqlStandardParser.Match<Statement> TryParseSQLDynamicStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLDynamicStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLDynamicStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseSQLDynamicStatement(string input) => TryParseSQLDynamicStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLDynamicStatement(input)
		: throw Refused(input, "SQL dynamic statement");

	public static SqlStandardParser.Match<Statement> TryParseSQLProcedureStatement(string input)
	{
		var read = SqlStandardParser.TryParseSQLProcedureStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLProcedureStatement(input, out var hand), hand);

		return read;
	}

	public static Statement ParseSQLProcedureStatement(string input) => TryParseSQLProcedureStatement(input).IsSuccess
		? SqlStandardParser.ParseSQLProcedureStatement(input)
		: throw Refused(input, "SQL procedure statement");

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
