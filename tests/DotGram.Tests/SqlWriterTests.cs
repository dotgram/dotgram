using System;

using DotGram.Parsers.Sql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <see cref="SqlWriter"/>: the tree back as SQL.
/// </summary>
/// <remarks>
/// Two claims, and the second is the one that matters. The first is that a handful of shapes
/// come out written the way they are meant to be — brackets where precedence needs them and
/// nowhere else, and the calls whose syntax is their own. The second is <b>idempotence</b>:
/// what the writer prints, the parser reads back into a tree the writer prints the same way.
/// A parser and a printer that disagree will show it here without a second parser being
/// asked; what only a second parser can say — whether the tree matches the <em>text</em> —
/// is <c>benchmarks --roundtrip</c>'s business.
/// </remarks>
public sealed class SqlWriterTests
{
	/// <summary>Every bracket somebody wrote comes back, and none that they did not.</summary>
	/// <remarks>
	/// The tree records the brackets rather than deducing them, so that a formatter does
	/// not rewrite <c>(a) + b</c> into <c>a + b</c> behind its author's back. Precedence is
	/// still the rule where a tree was built rather than read: nothing here adds a bracket
	/// that changes nothing.
	/// </remarks>
	[Theory]
	[InlineData("a + b * c",        "a + b * c")]
	[InlineData("(a + b) * c",      "(a + b) * c")]
	[InlineData("a * b + c",        "a * b + c")]
	[InlineData("a - (b - c)",      "a - (b - c)")]
	[InlineData("(a) + b",          "(a) + b")]
	[InlineData("- a * b",          "-a * b")]
	[InlineData("- (a * b)",        "-(a * b)")]
	public void A_bracket_is_written_where_precedence_needs_it(string input, string printed) =>
		Assert.Equal(printed, SqlWriter.Write(TransactSql.ParseValueExpression(input)));

	/// <summary>And the same for the tower of conditions.</summary>
	[Theory]
	[InlineData("a = 1 AND b = 2 OR c = 3",   "a = 1 AND b = 2 OR c = 3")]
	[InlineData("a = 1 AND (b = 2 OR c = 3)", "a = 1 AND (b = 2 OR c = 3)")]
	[InlineData("NOT (a = 1 AND b = 2)",      "NOT (a = 1 AND b = 2)")]
	[InlineData("NOT a = 1 AND b = 2",        "NOT a = 1 AND b = 2")]
	[InlineData("(a = 1)",                    "(a = 1)")]
	[InlineData("a NOT BETWEEN 1 AND 2",      "a NOT BETWEEN 1 AND 2")]
	[InlineData("a NOT IN (1, 2)",            "a NOT IN (1, 2)")]
	[InlineData("a LIKE 'x%' ESCAPE '\\'",    "a LIKE 'x%' ESCAPE '\\'")]
	[InlineData("a IS NOT NULL",              "a IS NOT NULL")]
	public void And_where_a_condition_needs_it(string input, string printed) =>
		Assert.Equal(printed, SqlWriter.Write(TransactSql.ParseSearchCondition(input)));

	/// <summary>The calls whose syntax is their own, and not an argument list.</summary>
	[Theory]
	[InlineData("CAST(a AS INT)",              "CAST(a AS INT)")]
	[InlineData("TRY_CAST(a AS INT)",          "TRY_CAST(a AS INT)")]
	[InlineData("CONVERT(INT, a)",             "CONVERT(INT, a)")]
	[InlineData("CONVERT(INT, a, 101)",        "CONVERT(INT, a, 101)")]
	[InlineData("PARSE(a AS INT)",             "PARSE(a AS INT)")]
	[InlineData("COUNT(*)",                    "COUNT(*)")]
	[InlineData("COUNT(DISTINCT a)",           "COUNT(DISTINCT a)")]
	[InlineData("NEXT VALUE FOR dbo.s",        "NEXT VALUE FOR dbo.s")]
	[InlineData("a AT TIME ZONE 'UTC'",        "a AT TIME ZONE 'UTC'")]
	[InlineData("dbo.f(a, 1)",                 "dbo.f(a, 1)")]
	[InlineData("CASE WHEN a > 1 THEN 2 ELSE 3 END", "CASE WHEN a > 1 THEN 2 ELSE 3 END")]
	public void A_call_with_a_syntax_of_its_own_comes_back_in_it(string input, string printed) =>
		Assert.Equal(printed, SqlWriter.Write(TransactSql.ParseValueExpression(input)));

	/// <summary>And the statements the tree holds whole.</summary>
	[Theory]
	[InlineData("SELECT a FROM t",                        "SELECT a FROM t")]
	[InlineData("SELECT DISTINCT a, b FROM t WHERE a > 1", "SELECT DISTINCT a, b FROM t WHERE a > 1")]
	[InlineData("SELECT * FROM t ORDER BY a DESC",        "SELECT * FROM t ORDER BY a DESC")]
	[InlineData("SELECT a FROM t JOIN u ON t.a = u.a",    "SELECT a FROM t JOIN u ON t.a = u.a")]
	[InlineData("SELECT a FROM t INNER JOIN u ON t.a = u.a", "SELECT a FROM t INNER JOIN u ON t.a = u.a")]
	[InlineData("SELECT a FROM t LEFT OUTER JOIN u ON t.a = u.a",
		"SELECT a FROM t LEFT OUTER JOIN u ON t.a = u.a")]
	[InlineData("SELECT a FROM t LEFT JOIN u ON t.a = u.a", "SELECT a FROM t LEFT JOIN u ON t.a = u.a")]
	[InlineData("SELECT a FROM (SELECT b FROM u) AS d",   "SELECT a FROM (SELECT b FROM u) AS d")]
	[InlineData("SELECT a FROM t UNION ALL SELECT b FROM u",
		"SELECT a FROM t UNION ALL SELECT b FROM u")]
	[InlineData("INSERT INTO t (a) VALUES (1)",           "INSERT INTO t (a) VALUES (1)")]
	[InlineData("UPDATE t SET a = 1 WHERE b = 2",         "UPDATE t SET a = 1 WHERE b = 2")]
	[InlineData("DELETE FROM t WHERE a = 1",              "DELETE FROM t WHERE a = 1")]
	[InlineData("PRINT 1",                                "PRINT 1")]
	[InlineData("DROP TABLE a, b",                        "DROP TABLE a, b")]
	[InlineData("DROP XML SCHEMA COLLECTION c",           "DROP XML SCHEMA COLLECTION c")]
	[InlineData("CREATE SCHEMA s",                        "CREATE SCHEMA s")]
	[InlineData("ALTER DATABASE d SET COMPATIBILITY_LEVEL = 150", "ALTER DATABASE d SET COMPATIBILITY_LEVEL = 150")]
	[InlineData("INSERT INTO t VALUES (1) OPTION (RECOMPILE)", "INSERT INTO t VALUES (1) OPTION (RECOMPILE)")]
	public void A_statement_comes_back_as_what_it_said(string input, string printed) =>
		Assert.Equal(printed, SqlWriter.Write(Read(input)));

	/// <summary>
	/// What the writer prints, the parser reads into a tree the writer prints the same way.
	/// </summary>
	/// <remarks>
	/// The claim that keeps the two honest without a second parser. A statement whose printed
	/// form this grammar cannot read at all fails here too, which is the other half of it.
	/// </remarks>
	[Theory]
	[InlineData("SELECT a, t.*, dbo.f(b) AS c FROM t AS x JOIN u ON x.a = u.a WHERE a IN (1, 2)")]
	[InlineData("SELECT COUNT(*) FROM t GROUP BY a HAVING COUNT(*) > 1")]
	[InlineData("SELECT a FROM t WHERE EXISTS (SELECT 1 FROM u WHERE u.a = t.a)")]
	[InlineData("SELECT CASE a WHEN 1 THEN 'x' ELSE 'y' END FROM t")]
	[InlineData("SELECT a FROM t UNION SELECT b FROM u EXCEPT SELECT c FROM v")]
	[InlineData("INSERT INTO t (a, b) SELECT c, d FROM u")]
	[InlineData("UPDATE t SET a = a + 1, b = 'x' FROM t JOIN u ON t.a = u.a WHERE t.b IS NULL")]
	[InlineData("DELETE FROM t FROM t JOIN u ON t.a = u.a WHERE u.b = 1")]
	[InlineData("BEGIN PRINT 1; PRINT 2; END")]
	[InlineData("IF a = 1 PRINT 1 ELSE PRINT 2")]
	[InlineData("WHILE a = 1 BEGIN PRINT 1; END")]
	[InlineData("BEGIN TRY PRINT 1; END TRY BEGIN CATCH PRINT 2; END CATCH")]
	[InlineData("DECLARE @a INT = 1, @b VARCHAR (10)")]
	[InlineData("CREATE TABLE t (a INT NOT NULL, b AS a + 1, PRIMARY KEY (a))")]
	[InlineData("CREATE TABLE t (a INT SPARSE NULL, b NCHAR (10) COLLATE Latin1_General_BIN, c INT IDENTITY (1, 5) NOT NULL, d VARBINARY (MAX) FILESTREAM)")]
	[InlineData("CREATE TABLE t (a INT MASKED WITH (FUNCTION = 'default()'), b INT CONSTRAINT nn NOT NULL, c INT CONSTRAINT df DEFAULT (0))")]
	[InlineData("CREATE TABLE t (a INT, CONSTRAINT pk PRIMARY KEY CLUSTERED (a ASC, b DESC) WITH (FILLFACTOR = 80) ON [PRIMARY]) ON ps (a) TEXTIMAGE_ON fg WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1, 3 TO 5))")]
	[InlineData("CREATE TABLE t (a INT, b INT, FOREIGN KEY (a) REFERENCES u (id) ON DELETE CASCADE ON UPDATE NO ACTION NOT FOR REPLICATION)")]
	[InlineData("CREATE TABLE t (a INT, INDEX ix UNIQUE NONCLUSTERED (a) INCLUDE (b) WHERE a > 0 WITH (DATA_COMPRESSION = PAGE), INDEX cci CLUSTERED COLUMNSTORE WITH (COMPRESSION_DELAY = 10 MINUTES))")]
	[InlineData("CREATE TABLE t AS FILETABLE WITH (FILETABLE_DIRECTORY = 'd')")]
	[InlineData("CREATE TABLE t (a DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL, b DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL, PERIOD FOR SYSTEM_TIME (a, b)) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.h))")]
	[InlineData("CREATE TABLE t WITH (DISTRIBUTION = HASH (a), CLUSTERED INDEX (a)) AS SELECT a FROM u")]
	[InlineData("ALTER TABLE t ADD c INT NOT NULL INDEX ix NONCLUSTERED HASH (c) WITH (BUCKET_COUNT = 256)")]
	[InlineData("ALTER TABLE t ALTER COLUMN a ADD SPARSE WITH (ONLINE = ON)")]
	[InlineData("DECLARE @t TABLE (a INT PRIMARY KEY, b INT SPARSE)")]
	[InlineData("CREATE UNIQUE NONCLUSTERED INDEX ix ON t (a, b DESC) INCLUDE (c) WHERE a > 0 WITH (PAD_INDEX = ON) ON ps (a)")]
	[InlineData("CREATE VIEW v AS SELECT a FROM t")]
	[InlineData("CREATE PROCEDURE p (@a INT = 1) AS SELECT a FROM t;")]
	[InlineData("CREATE FUNCTION f (@a INT) RETURNS INT AS RETURN 1;")]
	[InlineData("CREATE INDEX ix ON t (a, b)")]
	[InlineData("GRANT SELECT TO u")]
	[InlineData("SET @a = 1")]
	[InlineData("EXECUTE dbo.p 1, 2")]
	[InlineData("SELECT a FROM t WHERE b LIKE 'x%' AND NOT (c = 1 OR d = 2)")]
	public void What_the_writer_prints_reads_back_the_same(string input)
	{
		var once  = SqlWriter.Write(Read(input));
		var twice = SqlWriter.Write(Read(once));

		Assert.Equal(once, twice);
	}

	/// <summary>Every node says where it was written.</summary>
	/// <remarks>
	/// What <c>[GramOptions(LocationType = typeof(ISqlSpan), Suffix = "Located")]</c> asks for: the
	/// reader offers each construction the range it was read over, and the value keeps the
	/// last offer. The span is the node's own text without the trivia around it, which is
	/// what lets a comment fall between two of them rather than inside one.
	/// <para>
	/// Read by the second parser and not the first: locations cost fourteen per cent of a
	/// parse, and one grammar compiles to both so that only whoever wants them pays.
	/// </para>
	/// </remarks>
	[Fact]
	public void Every_node_says_where_it_was_written()
	{
		const string input = "SELECT a + 1 FROM t WHERE b = 2";

		var match = TransactSql.Located.TryParseStatement(input);

		Assert.True(match.IsSuccess, input);

		var read  = Assert.IsAssignableFrom<Statement>(match.Value);
		var query = Assert.IsType<Query.Specification>(Assert.IsType<Statement.Select>(read).Of);

		// And the reading that was not asked for locations keeps none, which is the whole
		// point of there being two.
		Assert.False(Read(input).Span.Known);

		Assert.Equal(input, Cut(input, read.Span));
		Assert.Equal(input, Cut(input, query.Span));

		var column = Assert.IsType<Clause.DerivedColumn>(Assert.Single(query.Columns));

		Assert.Equal("a + 1", Cut(input, column.Span));
		Assert.Equal("a + 1", Cut(input, column.Value.Span));
		Assert.Equal("t",     Cut(input, Assert.Single(query.From).Span));

		// A rule that hands back a value another rule made lends it its own range, and
		// `WhereClause` is `"WHERE"i & c: SearchCondition => @(c)`. The keyword is in the
		// condition's span, which is the safe direction — see `ISqlSpan`.
		Assert.Equal("WHERE b = 2", Cut(input, query.Where!.Span));
	}

	/// <summary>And a span is where it says, in the text it was measured against.</summary>
	static string Cut(string input, SqlSpan span) => input.Substring(span.At, span.Length);

	static Statement Read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));

		return Assert.IsAssignableFrom<Statement>(match.Value);
	}
}
