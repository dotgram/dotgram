using System;
using System.Linq;
using System.Reflection;

using DotGram.Handwritten;
using DotGram.Sql.Ast;
using DotGram.Sql.Productions;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside the namespace, where it is asked before DotGram.Sql's own Expression and Statement — the tree
// T-SQL builds, which a using directive above the namespace would lose to.
using Expression = DotGram.Sql.Ast.Expression;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// What <see cref="SqlStandardParser"/> publishes: six levels — a value, a data type, an expression, a
/// search condition, a statement and a text of statements — and nothing below them.
/// </summary>
/// <remarks>
/// The productions are read one by one through <see cref="SqlStandardProductions"/>, the grammar
/// included by a test fixture, which <see cref="Both"/> holds to these levels on every row. These are
/// the levels' own: what each reads that no production does, and that the fixture, a parser compiled
/// apart from this one, answers as this one does.
/// </remarks>
public sealed class SqlStandardEntryTests
{
	// ── The surface ────────────────────────────────────────────────────────────

	/// <summary>Six entries, each in the forms every publication has, and no other public method.</summary>
	[Fact]
	public void The_parser_publishes_six_levels()
	{
		var publications = typeof(SqlStandardParser).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static one => one.Name.StartsWith("Parse", StringComparison.Ordinal) || one.Name.StartsWith("TryParse", StringComparison.Ordinal))
			.ToList();
		var entries = publications
			.Select(static one => one.Name.StartsWith("TryParse", StringComparison.Ordinal) ? one.Name["TryParse".Length..] : one.Name["Parse".Length..])
			.Distinct()
			.OrderBy(static one => one, StringComparer.Ordinal);

		Assert.Equal(["DataType", "Expression", "SearchCondition", "Sql", "Statement", "Value"], entries);
		Assert.Equal(42, publications.Count);
	}

	// ── A value ────────────────────────────────────────────────────────────────

	/// <summary>
	/// A sign and a number are one value however they are separated — the sign and the number are two
	/// tokens, as the standard's <c>&lt;signed numeric literal&gt;</c> has them.
	/// </summary>
	[Theory]
	[InlineData("-1")]
	[InlineData("- 1")]
	[InlineData("- /*c*/ 1")]
	public void A_value_is_a_signed_number(string input)
	{
		var value = Assert.IsType<LiteralValue.Numeric>(SqlStandardParser.ParseValue(input));

		Assert.Equal("-1", value.Text);
		Assert.True(HandSqlStandard.TryParseValue(input, out var hand));
		Assert.Equal(value, hand);
	}

	/// <summary>
	/// <c>NULL</c> is no <c>&lt;literal&gt;</c> in the standard but a <c>&lt;null specification&gt;</c>; it
	/// is a value here, the one a caller cannot write as a literal.
	/// </summary>
	[Theory]
	[InlineData("NULL")]
	[InlineData("null")]
	[InlineData(" NULL /* none */")]
	public void A_value_is_NULL(string input)
	{
		Assert.IsType<LiteralValue.Null>(SqlStandardParser.ParseValue(input));
		Assert.True(HandSqlStandard.TryParseValue(input, out var hand));
		Assert.IsType<LiteralValue.Null>(hand);
	}

	[Theory]
	[InlineData("'it''s'")]
	[InlineData("N'abc'")]
	[InlineData("X'0A1B'")]
	[InlineData("DATE '2024-01-31'")]
	[InlineData("INTERVAL -'1:30' HOUR TO MINUTE")]
	[InlineData("TRUE")]
	[InlineData("UNKNOWN")]
	public void A_value_is_any_literal(string input)
	{
		Assert.Equal(SqlStandardProductions.ParseLiteral(input), SqlStandardParser.ParseValue(input));
	}

	[Theory]
	[InlineData("DEFAULT")]
	[InlineData("a")]
	[InlineData("1 + 1")]
	[InlineData("(1)")]
	[InlineData("NULL NULL")]
	[InlineData("")]
	public void What_is_no_value_is_refused(string input)
	{
		Assert.False(SqlStandardParser.TryParseValue(input).IsSuccess);
		Assert.False(HandSqlStandard.TryParseValue(input, out _));
	}

	/// <summary>
	/// A value and an expression read <c>-1</c> differently: a value is one literal whose text has the
	/// sign, an expression a sign applied to the literal <c>1</c>, as the standard's <c>&lt;factor&gt;</c> is.
	/// </summary>
	[Fact]
	public void A_signed_value_is_not_the_signed_expression()
	{
		var value      = SqlStandardParser.ParseValue("-1");
		var expression = SqlStandardParser.ParseExpression("-1");

		Assert.IsType<LiteralValue.Numeric>(value);
		Assert.False(expression is Expression.Literal { Value: LiteralValue.Numeric { Text: "-1" } });
		Assert.Equal("-1", Sql2023Writer.Write(expression));
	}

	// ── A statement ────────────────────────────────────────────────────────────

	/// <summary>Every kind of statement through the one entry; the kind is the type of the node.</summary>
	[Theory]
	[InlineData("SELECT a FROM t", typeof(Statement.Select))]
	[InlineData("VALUES (1), (2)", typeof(Statement.Select))]
	[InlineData("TABLE t", typeof(Statement.Select))]
	[InlineData("WITH q AS (SELECT 1 FROM t) SELECT * FROM q ORDER BY 1 FETCH FIRST 1 ROW ONLY", typeof(Statement.Select))]
	[InlineData("INSERT INTO t (a) VALUES (1)", typeof(Statement.Insert))]
	[InlineData("UPDATE t SET a = 1 WHERE b = 2", typeof(Statement.Update))]
	[InlineData("UPDATE t SET a = 1 WHERE CURRENT OF c", typeof(Statement.Update))]
	[InlineData("DELETE FROM t WHERE a = 1", typeof(Statement.Delete))]
	[InlineData("DELETE FROM t WHERE CURRENT OF c", typeof(Statement.Delete))]
	[InlineData("MERGE INTO t USING u ON t.a = u.a WHEN MATCHED THEN DELETE", typeof(Statement.Merge))]
	[InlineData("TRUNCATE TABLE t", typeof(Statement.TruncateTable))]
	[InlineData("CREATE TABLE t (a INT NOT NULL, PRIMARY KEY (a))", typeof(Statement.CreateTable))]
	[InlineData("DECLARE LOCAL TEMPORARY TABLE t (a INT)", typeof(Statement.DeclareLocalTemporaryTable))]
	[InlineData("SELECT a INTO :x FROM t", typeof(Statement.Select))]
	[InlineData("COMMIT", typeof(Statement.Commit))]
	[InlineData("GET DIAGNOSTICS n = NUMBER", typeof(Statement.GetDiagnostics))]
	public void A_statement_is_any_statement(string input, Type kind)
	{
		Assert.IsType(kind, SqlStandardParser.ParseStatement(input));
		Assert.IsType(kind, SqlStandardParser.ParseStatement(input + ";"));
		Assert.Equal(SqlStandardParser.ParseStatement(input), SqlStandardParser.ParseStatement(input + " ; "));

		Assert.True(HandSqlStandard.TryParseStatement(input, out var hand));
		Assert.Equal(SqlStandardParser.ParseStatement(input), hand);
	}

	/// <summary>
	/// A query read as a statement is the node a query is, and a cursor's updatability is read with it:
	/// a caller that wants a query and nothing else asks for <see cref="Statement.Select"/> without one.
	/// </summary>
	[Fact]
	public void A_query_is_a_statement_and_may_be_updatable()
	{
		var query = Assert.IsType<Statement.Select>(SqlStandardParser.ParseStatement("SELECT a FROM t"));
		var cursor = Assert.IsType<Statement.Select>(SqlStandardParser.ParseStatement("SELECT a FROM t FOR UPDATE OF a"));

		Assert.Null(query.Updatability);
		Assert.NotNull(cursor.Updatability);
		Assert.Equal(query, cursor with { Updatability = null });
	}

	[Theory]
	[InlineData("")]
	[InlineData(";")]
	[InlineData("SELECT a FROM t;;")]
	[InlineData("SELECT a FROM t; COMMIT")]
	[InlineData("SELECT TOP 1 a FROM t")]
	public void What_is_not_one_statement_is_refused(string input)
	{
		Assert.False(SqlStandardParser.TryParseStatement(input).IsSuccess);
		Assert.False(HandSqlStandard.TryParseStatement(input, out _));
	}

	// ── A text ─────────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("", 0)]
	[InlineData("  \r\n\t", 0)]
	[InlineData("/* nothing */ -- at all\n", 0)]
	[InlineData("SELECT a FROM t", 1)]
	[InlineData("SELECT a FROM t;", 1)]
	[InlineData("SELECT a FROM t -- the last, and no semicolon", 1)]
	[InlineData("SELECT a FROM t; DELETE FROM t", 2)]
	[InlineData("SELECT a FROM t; DELETE FROM t;", 2)]
	[InlineData("SELECT a FROM t;\nDELETE FROM t;\n-- done\n", 2)]
	[InlineData("CREATE TRIGGER g AFTER INSERT ON t FOR EACH ROW BEGIN ATOMIC DELETE FROM u; INSERT INTO u VALUES (1); END; COMMIT", 2)]
	public void A_text_is_statements_each_ended_by_a_semicolon_or_the_end(string input, int count)
	{
		var statements = SqlStandardParser.ParseSql(input);

		Assert.Equal(count, statements.Length);
		Assert.True(HandSqlStandard.TryParseSql(input, out var hand));
		Assert.Equal(statements, hand);
	}

	/// <summary>A text's statements are the statements read one by one.</summary>
	[Fact]
	public void A_text_reads_what_its_statements_read()
	{
		var statements = SqlStandardParser.ParseSql("SELECT a FROM t; INSERT INTO t VALUES (1); CREATE TRIGGER g AFTER INSERT ON t FOR EACH ROW BEGIN ATOMIC DELETE FROM u; END");

		Assert.Equal(SqlStandardParser.ParseStatement("SELECT a FROM t"), statements[0]);
		Assert.Equal(SqlStandardParser.ParseStatement("INSERT INTO t VALUES (1)"), statements[1]);
		Assert.Equal(SqlStandardParser.ParseStatement("CREATE TRIGGER g AFTER INSERT ON t FOR EACH ROW BEGIN ATOMIC DELETE FROM u; END"), statements[2]);
	}

	/// <summary>
	/// An empty statement is none: a semicolon ends a statement and is not one. T-SQL's <c>ParseSql</c>
	/// reads both, as the engine does; the standard has no empty statement.
	/// </summary>
	[Theory]
	[InlineData(";")]
	[InlineData(";;")]
	[InlineData("; SELECT a FROM t")]
	[InlineData("SELECT a FROM t;;")]
	[InlineData("SELECT a FROM t; ; DELETE FROM t")]
	public void An_empty_statement_is_refused(string input)
	{
		Assert.False(SqlStandardParser.TryParseSql(input).IsSuccess);
		Assert.False(HandSqlStandard.TryParseSql(input, out _));
	}

	/// <summary>Two statements with nothing between them: refused where the second begins, a semicolon expected there.</summary>
	[Fact]
	public void Two_statements_need_a_semicolon_between_them()
	{
		var read = SqlStandardParser.TryParseSql("COMMIT ROLLBACK");

		Assert.False(read.IsSuccess);
		Assert.Equal(7, read.Position);
		Assert.Contains("';'", read.Error, StringComparison.Ordinal);
		Assert.False(HandSqlStandard.TryParseSql("COMMIT ROLLBACK", out _));
	}

	/// <summary>A refusal inside the second statement is said where it is, not where the second statement begins.</summary>
	[Fact]
	public void A_refusal_in_a_later_statement_points_into_it()
	{
		const string text = "SELECT a FROM t; SELECT b FROM WHERE b = 1";

		var read = SqlStandardParser.TryParseSql(text);

		Assert.False(read.IsSuccess);
		Assert.Equal(text.IndexOf("WHERE", StringComparison.Ordinal), read.Position);
		Assert.Equal(SqlStandardParser.TryParseStatement("SELECT b FROM WHERE b = 1").Error, read.Error);
	}

	// ── The fixture, held to the shipped parser ────────────────────────────────

	public static TheoryData<string> Inputs =>
	[
		"1", "-1", "NULL", "'x'", "DATE '2024-01-31'", "a", "a + b * 2", "(a + b) * c - d / 5", "a = 1 AND b IS NOT NULL",
		"x BETWEEN 1 AND 2 OR EXISTS (SELECT 1 FROM t)", "CHAR VARYING(20)", "DECIMAL(10, 2)", "INTERVAL DAY TO SECOND(3)",
		"SELECT a FROM t", "SELECT a FROM t;", "SELECT a FROM t; DELETE FROM t", "COMMIT ROLLBACK", "SELECT a FROM", "SELECT TOP 1 a FROM t",
		"INSERT INTO t VALUES (1", "CREATE TABLE t (a INT", "UPDATE t SET a = 1 WHERE CURRENT OF c", "", ";", "a +", "CASE WHEN",
	];

	/// <summary>
	/// The fixture includes the grammar and is a parser of its own — its own kinds, machines and memo — and
	/// answers each level as the shipped class does: the same verdict, tree, position and message. A
	/// difference is a defect of the include.
	/// </summary>
	[Theory]
	[MemberData(nameof(Inputs))]
	public void The_fixture_reads_each_level_as_the_shipped_parser_does(string input)
	{
		Same(SqlStandardParser.TryParseValue(input), SqlStandardProductions.TryParseValue(input));
		Same(SqlStandardParser.TryParseDataType(input), SqlStandardProductions.TryParseDataType(input));
		Same(SqlStandardParser.TryParseExpression(input), SqlStandardProductions.TryParseExpression(input));
		Same(SqlStandardParser.TryParseSearchCondition(input), SqlStandardProductions.TryParseSearchCondition(input));
		Same(SqlStandardParser.TryParseStatement(input), SqlStandardProductions.TryParseStatement(input));
		Same(SqlStandardParser.TryParseSql(input), SqlStandardProductions.TryParseSql(input));
	}

	static void Same<T>(SqlStandardParser.Match<T> shipped, SqlStandardProductions.Match<T> fixture)
	{
		Assert.Equal(shipped.IsSuccess, fixture.IsSuccess);

		if (shipped.IsSuccess)
		{
			Assert.Equal(shipped.Value, fixture.Value);

			return;
		}

		Assert.Equal(shipped.Position, fixture.Position);
		Assert.Equal(shipped.Error, fixture.Error);
	}
}
