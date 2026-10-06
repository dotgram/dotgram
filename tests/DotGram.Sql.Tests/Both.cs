using System;
using System.Reflection;
using System.Text;

using DotGram.Handwritten;
using DotGram.Sql.Ast;
using DotGram.Sql.Productions;
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
/// A production is read by <see cref="SqlStandardProductions"/>, the grammar included and published
/// production by production, since the shipped class publishes six levels and no production; every
/// reading is also put to <see cref="SqlStandardParser"/> at the level that holds it (<see cref="Shipped"/>),
/// so that a row tests the binary that ships as well as the grammar.
/// </para>
/// <para>
/// Chapter by chapter: a production the handwritten parser does not read yet is still called
/// through <see cref="SqlStandardProductions"/> directly, and moves here when it does.
/// </para>
/// </remarks>
static class Both
{
	// ── §5.3 Literals ──────────────────────────────────────────────────────────

	public static SqlStandardProductions.Match<LiteralValue> TryParseLiteral(string input)
	{
		var read = SqlStandardProductions.TryParseLiteral(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseLiteral(input, out var hand), hand);

		Shipped.Value(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static LiteralValue ParseLiteral(string input)
	{
		return TryParseLiteral(input).IsSuccess
		? SqlStandardProductions.ParseLiteral(input)
		: throw Refused(input, "literal");
	}

	public static SqlStandardProductions.Match<LiteralValue> TryParseUnsignedLiteral(string input)
	{
		var read = SqlStandardProductions.TryParseUnsignedLiteral(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseUnsignedLiteral(input, out var hand), hand);

		return read;
	}

	// ── §5.4 Names and identifiers ─────────────────────────────────────────────

	public static SqlStandardProductions.Match<Identifier> TryParseIdentifier(string input)
	{
		var read = SqlStandardProductions.TryParseIdentifier(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseIdentifier(input, out var hand), hand);

		return read;
	}

	public static Identifier ParseIdentifier(string input)
	{
		return TryParseIdentifier(input).IsSuccess
		? SqlStandardProductions.ParseIdentifier(input)
		: throw Refused(input, "identifier");
	}

	public static SqlStandardProductions.Match<QualifiedName> TryParseIdentifierChain(string input)
	{
		var read = SqlStandardProductions.TryParseIdentifierChain(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseIdentifierChain(input, out var hand), hand);

		return read;
	}

	public static QualifiedName ParseIdentifierChain(string input)
	{
		return TryParseIdentifierChain(input).IsSuccess
		? SqlStandardProductions.ParseIdentifierChain(input)
		: throw Refused(input, "identifier chain");
	}

	public static SqlStandardProductions.Match<Expression> TryParseColumnReference(string input)
	{
		var read = SqlStandardProductions.TryParseColumnReference(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseColumnReference(input, out var hand), hand);

		return read;
	}

	public static Expression ParseColumnReference(string input)
	{
		return TryParseColumnReference(input).IsSuccess
		? SqlStandardProductions.ParseColumnReference(input)
		: throw Refused(input, "column reference");
	}

	public static SqlStandardProductions.Match<QualifiedName> TryParseTableName(string input)
	{
		var read = SqlStandardProductions.TryParseTableName(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseTableName(input, out var hand), hand);

		return read;
	}

	public static QualifiedName ParseTableName(string input)
	{
		return TryParseTableName(input).IsSuccess
		? SqlStandardProductions.ParseTableName(input)
		: throw Refused(input, "table name");
	}

	public static SqlStandardProductions.Match<QualifiedName> TryParseSchemaName(string input)
	{
		var read = SqlStandardProductions.TryParseSchemaName(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSchemaName(input, out var hand), hand);

		return read;
	}

	// ── §6.1 Data types ────────────────────────────────────────────────────────

	public static SqlStandardProductions.Match<DataType> TryParseDataType(string input)
	{
		var read = SqlStandardProductions.TryParseDataType(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDataType(input, out var hand), hand);

		Shipped.Same(input, read.IsSuccess, read.Position, read.Error, read.IsSuccess ? read.Value : null, SqlStandardParser.TryParseDataType(input));

		return read;
	}

	public static DataType ParseDataType(string input)
	{
		return TryParseDataType(input).IsSuccess
		? SqlStandardProductions.ParseDataType(input)
		: throw Refused(input, "data type");
	}

	// ── §6.28 Value expressions, §8 predicates ─────────────────────────────────

	public static SqlStandardProductions.Match<Expression> TryParseValueExpression(string input)
	{
		var read = SqlStandardProductions.TryParseValueExpression(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseValueExpression(input, out var hand), hand);

		Shipped.Same(input, read.IsSuccess, read.Position, read.Error, read.IsSuccess ? read.Value : null, SqlStandardParser.TryParseExpression(input));

		return read;
	}

	public static Expression ParseValueExpression(string input)
	{
		return TryParseValueExpression(input).IsSuccess
		? SqlStandardProductions.ParseValueExpression(input)
		: throw Refused(input, "value expression");
	}

	public static SqlStandardProductions.Match<Expression> TryParseRowValuePredicand(string input)
	{
		var read = SqlStandardProductions.TryParseRowValuePredicand(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseRowValuePredicand(input, out var hand), hand);

		return read;
	}

	public static SqlStandardProductions.Match<Expression> TryParseSearchCondition(string input)
	{
		var read = SqlStandardProductions.TryParseSearchCondition(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSearchCondition(input, out var hand), hand);

		Shipped.Same(input, read.IsSuccess, read.Position, read.Error, read.IsSuccess ? read.Value : null, SqlStandardParser.TryParseSearchCondition(input));

		return read;
	}

	public static Expression ParseSearchCondition(string input)
	{
		return TryParseSearchCondition(input).IsSuccess
		? SqlStandardProductions.ParseSearchCondition(input)
		: throw Refused(input, "search condition");
	}

	// ── §7 Query expressions ───────────────────────────────────────────────────

	public static SqlStandardProductions.Match<Statement.Select> TryParseQueryExpression(string input)
	{
		var read = SqlStandardProductions.TryParseQueryExpression(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseQueryExpression(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.Select ParseQueryExpression(string input)
	{
		return TryParseQueryExpression(input).IsSuccess
		? SqlStandardProductions.ParseQueryExpression(input)
		: throw Refused(input, "query expression");
	}

	public static SqlStandardProductions.Match<TableSource> TryParseTableReference(string input)
	{
		var read = SqlStandardProductions.TryParseTableReference(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseTableReference(input, out var hand), hand);

		return read;
	}

	public static TableSource ParseTableReference(string input)
	{
		return TryParseTableReference(input).IsSuccess
		? SqlStandardProductions.ParseTableReference(input)
		: throw Refused(input, "table reference");
	}

	// ── §14 Data change statements ─────────────────────────────────────────────

	public static SqlStandardProductions.Match<Statement.Insert> TryParseInsertStatement(string input)
	{
		var read = SqlStandardProductions.TryParseInsertStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseInsertStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.Insert ParseInsertStatement(string input)
	{
		return TryParseInsertStatement(input).IsSuccess
		? SqlStandardProductions.ParseInsertStatement(input)
		: throw Refused(input, "insert statement");
	}

	public static SqlStandardProductions.Match<Statement.Update> TryParseUpdateStatementSearched(string input)
	{
		var read = SqlStandardProductions.TryParseUpdateStatementSearched(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseUpdateStatementSearched(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.Update ParseUpdateStatementSearched(string input)
	{
		return TryParseUpdateStatementSearched(input).IsSuccess
		? SqlStandardProductions.ParseUpdateStatementSearched(input)
		: throw Refused(input, "update statement: searched");
	}

	public static SqlStandardProductions.Match<Statement.Update> TryParseUpdateStatementPositioned(string input)
	{
		var read = SqlStandardProductions.TryParseUpdateStatementPositioned(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseUpdateStatementPositioned(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.Update ParseUpdateStatementPositioned(string input)
	{
		return TryParseUpdateStatementPositioned(input).IsSuccess
		? SqlStandardProductions.ParseUpdateStatementPositioned(input)
		: throw Refused(input, "update statement: positioned");
	}

	public static SqlStandardProductions.Match<Statement.Delete> TryParseDeleteStatementSearched(string input)
	{
		var read = SqlStandardProductions.TryParseDeleteStatementSearched(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDeleteStatementSearched(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.Delete ParseDeleteStatementSearched(string input)
	{
		return TryParseDeleteStatementSearched(input).IsSuccess
		? SqlStandardProductions.ParseDeleteStatementSearched(input)
		: throw Refused(input, "delete statement: searched");
	}

	public static SqlStandardProductions.Match<Statement.Delete> TryParseDeleteStatementPositioned(string input)
	{
		var read = SqlStandardProductions.TryParseDeleteStatementPositioned(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDeleteStatementPositioned(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.Delete ParseDeleteStatementPositioned(string input)
	{
		return TryParseDeleteStatementPositioned(input).IsSuccess
		? SqlStandardProductions.ParseDeleteStatementPositioned(input)
		: throw Refused(input, "delete statement: positioned");
	}

	public static SqlStandardProductions.Match<Statement.Merge> TryParseMergeStatement(string input)
	{
		var read = SqlStandardProductions.TryParseMergeStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseMergeStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.Merge ParseMergeStatement(string input)
	{
		return TryParseMergeStatement(input).IsSuccess
		? SqlStandardProductions.ParseMergeStatement(input)
		: throw Refused(input, "merge statement");
	}

	public static SqlStandardProductions.Match<Statement.TruncateTable> TryParseTruncateTableStatement(string input)
	{
		var read = SqlStandardProductions.TryParseTruncateTableStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseTruncateTableStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.TruncateTable ParseTruncateTableStatement(string input)
	{
		return TryParseTruncateTableStatement(input).IsSuccess
		? SqlStandardProductions.ParseTruncateTableStatement(input)
		: throw Refused(input, "truncate table statement");
	}

	// ── §11, §12 and §14 to §23: the statements ────────────────────────────────

	public static SqlStandardProductions.Match<Statement> TryParseSQLSchemaStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLSchemaStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLSchemaStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseSQLSchemaStatement(string input)
	{
		return TryParseSQLSchemaStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLSchemaStatement(input)
		: throw Refused(input, "SQL schema statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseDirectSQLStatement(string input)
	{
		var read = SqlStandardProductions.TryParseDirectSQLStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDirectSQLStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseDirectSQLStatement(string input)
	{
		return TryParseDirectSQLStatement(input).IsSuccess
		? SqlStandardProductions.ParseDirectSQLStatement(input)
		: throw Refused(input, "direct SQL statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseDirectSQLDataStatement(string input)
	{
		var read = SqlStandardProductions.TryParseDirectSQLDataStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseDirectSQLDataStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseDirectSQLDataStatement(string input)
	{
		return TryParseDirectSQLDataStatement(input).IsSuccess
		? SqlStandardProductions.ParseDirectSQLDataStatement(input)
		: throw Refused(input, "direct SQL data statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseSQLDataStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLDataStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLDataStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseSQLDataStatement(string input)
	{
		return TryParseSQLDataStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLDataStatement(input)
		: throw Refused(input, "SQL data statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseSQLControlStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLControlStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLControlStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseSQLControlStatement(string input)
	{
		return TryParseSQLControlStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLControlStatement(input)
		: throw Refused(input, "SQL control statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseSQLTransactionStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLTransactionStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLTransactionStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseSQLTransactionStatement(string input)
	{
		return TryParseSQLTransactionStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLTransactionStatement(input)
		: throw Refused(input, "SQL transaction statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseSQLConnectionStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLConnectionStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLConnectionStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseSQLConnectionStatement(string input)
	{
		return TryParseSQLConnectionStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLConnectionStatement(input)
		: throw Refused(input, "SQL connection statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseSQLSessionStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLSessionStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLSessionStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseSQLSessionStatement(string input)
	{
		return TryParseSQLSessionStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLSessionStatement(input)
		: throw Refused(input, "SQL session statement");
	}

	public static SqlStandardProductions.Match<Statement.GetDiagnostics> TryParseSQLDiagnosticsStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLDiagnosticsStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLDiagnosticsStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement.GetDiagnostics ParseSQLDiagnosticsStatement(string input)
	{
		return TryParseSQLDiagnosticsStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLDiagnosticsStatement(input)
		: throw Refused(input, "SQL diagnostics statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseSQLDynamicStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLDynamicStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLDynamicStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseSQLDynamicStatement(string input)
	{
		return TryParseSQLDynamicStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLDynamicStatement(input)
		: throw Refused(input, "SQL dynamic statement");
	}

	public static SqlStandardProductions.Match<Statement> TryParseSQLProcedureStatement(string input)
	{
		var read = SqlStandardProductions.TryParseSQLProcedureStatement(input);

		Agree(input, read.IsSuccess, read.IsSuccess ? read.Value : null, HandSqlStandard.TryParseSQLProcedureStatement(input, out var hand), hand);

		Shipped.Statement(input, read.IsSuccess, read.IsSuccess ? read.Value : null);

		return read;
	}

	public static Statement ParseSQLProcedureStatement(string input)
	{
		return TryParseSQLProcedureStatement(input).IsSuccess
		? SqlStandardProductions.ParseSQLProcedureStatement(input)
		: throw Refused(input, "SQL procedure statement");
	}

	// ── Holding the productions to the shipped parser ─────────────────────────

	/// <summary>
	/// The shipped <see cref="SqlStandardParser"/> beside the productions: what a production reads, the
	/// level that holds it reads to the same tree.
	/// </summary>
	/// <remarks>
	/// The productions are read by <see cref="SqlStandardProductions"/>, which includes the shipped grammar
	/// and is a parser of its own; the shipped class publishes six levels and no production. So every
	/// reading here is put to the shipped class as well, at the level it belongs to, and the rows written
	/// for one production test the binary that ships too.
	/// </remarks>
	static class Shipped
	{
		/// <summary>A statement or a query the production read is one statement to the shipped parser, the same one.</summary>
		public static void Statement(string input, bool read, object? tree)
		{
			if (!read)
				return;

			var shipped = SqlStandardParser.TryParseStatement(input);

			Assert.True(shipped.IsSuccess, "The shipped parser refuses as a statement what a production reads: " + input + "\n  " + shipped.Error);
			Same(input, tree, shipped.Value);
		}

		/// <summary>A literal is a value to the shipped parser; a value that is no literal is <c>NULL</c>.</summary>
		public static void Value(string input, bool read, object? tree)
		{
			var shipped = SqlStandardParser.TryParseValue(input);

			if (read)
			{
				Assert.True(shipped.IsSuccess, "The shipped parser refuses as a value what is a literal: " + input + "\n  " + shipped.Error);
				Same(input, tree, shipped.Value);
			}
			else if (shipped.IsSuccess)
				Assert.IsType<LiteralValue.Null>(shipped.Value);
		}

		/// <summary>A production the shipped parser publishes as a level of its own answers the same there, refusals and their messages too.</summary>
		public static void Same<T>(string input, bool read, long position, string? error, object? tree, SqlStandardParser.Match<T> shipped)
		{
			Assert.True(read == shipped.IsSuccess, (read ? "The shipped parser refuses what the production reads: " : "The shipped parser reads what the production refuses: ") + input);

			if (read)
			{
				Same(input, tree, shipped.Value);

				return;
			}

			Assert.Equal(position, shipped.Position);
			Assert.Equal(error, shipped.Error);
		}

		static void Same(string input, object? tree, object? shipped)
		{
			var expected = Dump(tree);
			var actual   = Dump(shipped);

			Assert.True(expected == actual, "The production and the shipped parser built different trees for: " + input + "\n  production: " + expected + "\n  shipped:    " + actual);
			Assert.Equal(tree, shipped);
		}
	}

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

		// Two trees built apart, by two parsers, are equal as records too: the tree compares by content.
		Assert.Equal(tree, handTree);
		Assert.Equal(tree!.GetHashCode(), handTree!.GetHashCode());
	}

	static FormatException Refused(string input, string production)
	{
		return new("Input does not match '" + production + "': " + input);
	}

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
