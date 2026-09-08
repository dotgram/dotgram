using System;

using DotGram.Parsers;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// T-SQL as a dialect of SQL-92, read.
/// </summary>
/// <remarks>
/// <para>
/// What is under test is not that T-SQL reads. <see cref="SqlStandard92Tests"/> already asks
/// that of the rules these ones stand in for, and asking it again of a dialect would say
/// nothing new. What is under test is that <em>a dialect is five rules and a header</em>:
/// <c>TransactSql.gram</c> replaces an identifier, a value specification, a value primary, a
/// query specification and a table primary, and §5.1's rebinding is what carries the
/// replacement into every rule that leads to one.
/// </para>
/// <para>
/// So the theories come in pairs. What T-SQL adds reads here and is refused by the class
/// this one derives from — which is what says the dialect did not leak into its base. And it
/// reads in positions the header never names — a bracketed name inside a join condition, a
/// variable inside a <c>HAVING</c> — which is the only way to tell a substitution that
/// reached from one written out at the top that stopped there.
/// </para>
/// </remarks>
public sealed class TransactSqlTests
{
	// ── The five seams, each read through the rule that replaced it ──────────────

	/// <summary>
	/// What T-SQL has and the standard does not: it reads here, and there it does not.
	/// </summary>
	/// <remarks>
	/// Both halves are the claim. The first says the replacement happened; the second says
	/// it happened <em>here</em> — a base and a derived class are two whole parsers, and the
	/// base still reads the language it always read.
	/// </remarks>
	[Theory]

	// §5.2's identifier, widened: brackets, `]]` for a bracket inside, and a temporary name.
	[InlineData("SELECT [a] FROM t")]
	[InlineData("SELECT [a]]b] FROM t")]
	[InlineData("SELECT [select] FROM [group]")]
	[InlineData("SELECT a FROM #tmp")]
	[InlineData("SELECT a FROM ##shared")]

	// §6.3's general value specification, widened: a variable stands where a parameter does.
	[InlineData("SELECT @v")]
	[InlineData("SELECT @@ROWCOUNT")]
	[InlineData("SELECT a FROM t WHERE b = @v")]

	// §6.11's value primary, widened: a call on a name this grammar never heard of, and
	// the window a call may be computed over.
	[InlineData("SELECT dbo.f(1, a) FROM t")]
	[InlineData("SELECT ISNULL(a, 0) FROM t")]
	[InlineData("SELECT COUNT(*) OVER () FROM t")]
	[InlineData("SELECT SUM(a) OVER (PARTITION BY b ORDER BY c) FROM t")]

	// §7.9's query specification, widened: `FROM` is optional and `TOP` stands before the
	// select list.
	[InlineData("SELECT 1")]
	[InlineData("SELECT TOP 10 a FROM t")]
	[InlineData("SELECT TOP (10) PERCENT a FROM t")]
	[InlineData("SELECT TOP 5 WITH TIES a FROM t ORDER BY a")]
	[InlineData("SELECT DISTINCT TOP 3 a FROM t")]

	// §7.4's table primary, widened: a table-valued function and a table variable.
	[InlineData("SELECT a FROM dbo.f(1) AS x")]
	[InlineData("SELECT a FROM @rows")]
	[InlineData("SELECT a FROM @rows AS r")]
	public void What_the_dialect_adds_reads_here_and_not_in_the_standard(string input)
	{
		Assert.True(TransactSql.TryParseSelect(input).IsSuccess, input);
		Assert.False(SqlStandard92.TryParseSelect(input).IsSuccess, input);
	}

	// ── That the substitution reached ────────────────────────────────────────────

	/// <summary>And it reads where the header never names it.</summary>
	/// <remarks>
	/// <c>namespace Dialect with (...)</c> names five rules. None of these positions is one
	/// of them: a join condition, a subquery's select list, a <c>CASE</c> arm, a
	/// <c>GROUP BY</c> key, a sort key, a set operand. Each is some other rule of the
	/// standard that leads to a replaced one, and reading here is the whole of what §5.1
	/// claims to do.
	/// </remarks>
	[Theory]
	[InlineData("SELECT a FROM t JOIN u ON t.[id] = u.[id]")]
	[InlineData("SELECT a FROM t WHERE b IN (SELECT [c] FROM u)")]
	[InlineData("SELECT CASE WHEN [a] > 1 THEN @v ELSE dbo.f([b]) END FROM t")]
	[InlineData("SELECT a FROM t GROUP BY [b] HAVING COUNT(*) > @n")]
	[InlineData("SELECT a FROM t ORDER BY [b] DESC")]
	[InlineData("SELECT a FROM t UNION SELECT [b] FROM u")]
	[InlineData("SELECT a FROM (SELECT TOP 1 [b] FROM u) AS d")]
	[InlineData("SELECT a FROM t WHERE b > (SELECT MAX([c]) FROM #tmp)")]
	[InlineData("SELECT a FROM t WHERE b BETWEEN @lo AND @hi")]
	[InlineData("SELECT a FROM t WHERE CAST([b] AS INTEGER) = dbo.f(@v)")]
	[InlineData("SELECT a FROM t JOIN dbo.f(@v) AS x ON t.id = x.id")]
	[InlineData("SELECT a FROM t WHERE EXISTS (SELECT 1 FROM u WHERE u.[id] = t.[id])")]
	public void The_replacement_reaches_where_it_is_never_named(string input) =>
		Assert.True(TransactSql.TryParseSelect(input).IsSuccess, input);

	// ── And that nothing was traded for it ───────────────────────────────────────

	/// <summary>What the standard reads, the dialect reads too.</summary>
	/// <remarks>
	/// A replacement restates the alternatives of the rule it replaces rather than calling
	/// it — a rule that named the rule it replaces would be rewritten into itself — so what
	/// the standard said is said again by hand, and this is what asks whether all of it was.
	/// </remarks>
	[Theory]
	[InlineData("SELECT * FROM t")]
	[InlineData("SELECT DISTINCT a, b FROM t WHERE a > 1")]
	[InlineData("SELECT t.* FROM t")]
	[InlineData("SELECT a AS x, b y FROM t")]
	[InlineData("SELECT a FROM t, u")]
	[InlineData("SELECT a FROM t LEFT OUTER JOIN u ON t.id = u.id")]
	[InlineData("SELECT a FROM t CROSS JOIN u")]
	[InlineData("SELECT a FROM (SELECT b FROM u) AS d (c)")]
	[InlineData("SELECT COUNT(*) FROM t GROUP BY a, b HAVING COUNT(*) > 1")]
	[InlineData("SELECT (SELECT MAX(b) FROM u) FROM t")]
	[InlineData("SELECT a FROM t UNION ALL SELECT b FROM u EXCEPT SELECT c FROM v")]
	[InlineData("SELECT a FROM t ORDER BY a DESC, 2 ASC")]
	[InlineData("SELECT \"select\" FROM t")]
	[InlineData("SELECT a COLLATE SQL_Latin1_General_CP1_CI_AS FROM t")]
	[InlineData("VALUES (1, 2), (3, 4)")]
	[InlineData("TABLE t")]
	[InlineData("(SELECT a FROM t)")]
	public void What_the_standard_reads_the_dialect_reads_too(string input)
	{
		Assert.True(SqlStandard92.TryParseSelect(input).IsSuccess, input);
		Assert.True(TransactSql  .TryParseSelect(input).IsSuccess, input);
	}

	// ── The value layer ─────────────────────────────────────────────────────────

	/// <summary>
	/// What T-SQL puts where a value goes, and the standard does not.
	/// </summary>
	/// <remarks>
	/// Every one of these came off the list <c>--kinds</c> makes: of the statements
	/// Microsoft's parser calls a query, what stood where this one stopped. So the theory is
	/// the work list, and a row of it that stops failing is a row that stops appearing.
	/// </remarks>
	[Theory]

	// The types a cast may name, and `MAX` where a length stands.
	[InlineData("SELECT CAST (a AS VARCHAR (MAX)) FROM t")]
	[InlineData("SELECT CAST (a AS NVARCHAR (300)) FROM t")]
	[InlineData("SELECT CAST (a AS VARBINARY (20)) FROM t")]
	[InlineData("SELECT CAST (a AS DATETIME2) FROM t")]
	[InlineData("SELECT CAST (a AS JSON) FROM t")]
	[InlineData("SELECT CAST (a AS VECTOR (3)) FROM t")]
	[InlineData("SELECT CAST (a AS MONEY) FROM t")]
	[InlineData("SELECT CAST (a AS dbo.MyType) FROM t")]

	// And the casts that are casts under another name.
	[InlineData("SELECT TRY_CAST ('12345' AS INT)")]
	[InlineData("SELECT CONVERT (INT, a) FROM t")]
	[InlineData("SELECT CONVERT (VARCHAR (10), a, 121) FROM t")]
	[InlineData("SELECT TRY_CONVERT (INT, a) FROM t")]

	// The literals.
	[InlineData("SELECT $492050157978986.2129")]
	[InlineData("SELECT 0xabcdef")]
	[InlineData("SELECT SET_BIT (0x00, 2) AS VARBIN1")]

	// A condition where a value stands, which is what `IIF` needs and a value cannot be.
	[InlineData("SELECT IIF (3 > 4, 'A', 'B')")]
	[InlineData("SELECT IIF (NOT a > 1, 1, 0) FROM t")]
	[InlineData("SELECT CHOOSE (2, 'a', 'b', 'c')")]

	// A truth where a condition stands, which is the same trade on the other side.
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'foo')")]
	[InlineData("SELECT a FROM t WHERE FREETEXT ((b), 'vital safety')")]
	[InlineData("SELECT 1 WHERE REGEXP_LIKE ('abc', '^a')")]
	[InlineData("SELECT CASE WHEN REGEXP_LIKE ('abc', '^a') THEN 1 ELSE 0 END")]
	[InlineData("SELECT a FROM t WHERE b IS NOT DISTINCT FROM 1")]
	[InlineData("SELECT a FROM t WHERE b IS DISTINCT FROM 1")]

	// A member of whatever was just read.
	[InlineData("SELECT (c1).SomeProperty FROM t")]
	[InlineData("SELECT t::a FROM u")]
	[InlineData("SELECT dbo.[type 1]::[Property] FROM t")]
	[InlineData("SELECT c1.f1().SomeProperty FROM t")]

	// A time zone, and a sequence.
	[InlineData("SELECT a AT TIME ZONE 'UTC' FROM t")]
	[InlineData("SELECT CAST ('1212-12-12 12:12:12' AS DATETIME2) AT TIME ZONE @tz")]
	[InlineData("SELECT NEXT VALUE FOR seq1")]
	[InlineData("SELECT NEXT VALUE FOR seq1 OVER (ORDER BY a) FROM t")]

	// And everything a window may say.
	[InlineData("SELECT COUNT (a) OVER (ORDER BY b) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ROWS UNBOUNDED PRECEDING) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ROWS 1 PRECEDING) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (RANGE CURRENT ROW) FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) OVER () FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) RESPECT NULLS FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) RESPECT NULLS OVER Win1 FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) IGNORE NULLS OVER () FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) WITHIN GROUP (ORDER BY a) OVER () FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) RESPECT NULLS OVER () FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ORDER BY b ROWS 1 PRECEDING) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ORDER BY b ROWS BETWEEN 1 FOLLOWING AND 1 FOLLOWING) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ORDER BY b RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FROM t")]
	[InlineData("SELECT SUM (a) OVER Win1 FROM t")]
	[InlineData("SELECT STRING_AGG (a, ',') WITHIN GROUP (ORDER BY a) FROM t")]
	public void The_value_layer_reads(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	// ── The query layer ─────────────────────────────────────────────────────────

	/// <summary>What T-SQL puts around a query, and the standard does not.</summary>
	/// <remarks>
	/// The clauses that belong to the statement rather than to the query — how many rows to
	/// skip, what shape to return them in, how to run it — and the three places inside one
	/// where T-SQL says more than §7 does: a select list that may assign, a group by that
	/// groups by sets, and a from clause that may apply, hint and pivot.
	/// </remarks>
	[Theory]

	// What the query returns, and where it puts it.
	[InlineData("SELECT @a += 1")]
	[InlineData("SELECT @a = b FROM t")]
	[InlineData("SELECT alias = b FROM t")]
	[InlineData("SELECT b AS 'Original string' FROM t")]
	[InlineData("SELECT c1 INTO t2 FROM t1")]
	[InlineData("SELECT c1 INTO t2 ON fg FROM t1")]
	[InlineData("SELECT TOP 10 WITH APPROXIMATE a FROM t ORDER BY a")]

	// The windows a select list may name rather than write out.
	[InlineData("SELECT SUM (c1) OVER Win1 FROM t1 WINDOW Win1 AS (PARTITION BY c1)")]

	// Grouping by sets of columns rather than by columns.
	[InlineData("SELECT a FROM t GROUP BY CUBE (a)")]
	[InlineData("SELECT a FROM t GROUP BY ROLLUP (a, b)")]
	[InlineData("SELECT a FROM t GROUP BY GROUPING SETS ((CUBE (a), ROLLUP (b), c), (a), ())")]
	[InlineData("SELECT a FROM t GROUP BY ALL ()")]

	// Applying, hinting, sampling and pivoting a source.
	[InlineData("SELECT * FROM t1 CROSS APPLY (SELECT * FROM u) AS x")]
	[InlineData("SELECT * FROM t1 OUTER APPLY dbo.f (t1.a) AS x")]
	[InlineData("SELECT * FROM t1 INNER HASH JOIN t10 ON t1.c1 = t10.c1")]
	[InlineData("SELECT * FROM t1 INNER REMOTE JOIN t10 ON t1.c1 = t10.c1")]
	[InlineData("SELECT c1 FROM t1 AS table1 WITH (INDEX (0, 1, ind2), HOLDLOCK, NOLOCK)")]
	[InlineData("SELECT * FROM t WITH (FORCESEEK (i134 (c1, c3, c4)))")]
	[InlineData("SELECT TOP (5) * FROM r WITH (SPATIAL_WINDOW_MAX_CELLS = 512)")]
	[InlineData("SELECT * FROM t1 TABLESAMPLE (12 ROWS)")]
	[InlineData("SELECT * FROM fun2 TABLESAMPLE SYSTEM (12 PERCENT) REPEATABLE (100)")]
	[InlineData("SELECT VendorID, [164] AS Emp1 FROM t PIVOT (SUM (a) FOR b IN ([164])) AS pvt")]
	[InlineData("SELECT a, b FROM (SELECT * FROM t) AS d UNPIVOT (v FOR k IN (x, y)) AS u")]

	// How many rows, what shape, and how to run it.
	[InlineData("SELECT * FROM T ORDER BY a OFFSET 5 ROWS FETCH NEXT 2 ROWS ONLY")]
	[InlineData("SELECT a FROM t ORDER BY a OFFSET 5 ROWS")]
	[InlineData("SELECT * FROM t1 FOR JSON AUTO")]
	[InlineData("SELECT * FROM t1 FOR JSON PATH, ROOT ('r')")]
	[InlineData("SELECT * FROM t1 FOR XML AUTO, ELEMENTS")]
	[InlineData("SELECT * FROM t1 FOR BROWSE")]
	[InlineData("SELECT a FROM t OPTION (RECOMPILE)")]
	[InlineData("SELECT * FROM t1 OPTION (MERGE UNION, HASH JOIN)")]
	[InlineData("SELECT * FROM t1 OPTION (HASH GROUP, CONCAT UNION, LOOP JOIN, FAST 10, FORCE ORDER)")]
	[InlineData("SELECT * FROM t1 OPTION (MAX_GRANT_PERCENT = 50)")]
	[InlineData("SELECT * FROM t1 OPTION (OPTIMIZE FOR (@v1 = 20, @v2 = UNKNOWN))")]
	[InlineData("SELECT * FROM t1 OPTION (USE PLAN N'zzz')")]
	[InlineData("SELECT * FROM t1 OPTION (USE HINT ('DISABLE_OPTIMIZED_NESTED_LOOP'))")]
	[InlineData("SELECT * FROM t WHERE c1 = 3 OPTION (TABLE HINT (t, FORCESCAN))")]
	[InlineData("SELECT * FROM t1 OPTION (PARAMETERIZATION SIMPLE, RECOMPILE, EXPAND VIEWS)")]
	public void The_query_layer_reads(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And what the standard reads, the dialect is right to refuse.</summary>
	/// <remarks>
	/// <para>
	/// A dialect narrows as well as widens, and nothing here was measuring that: a refusal
	/// count sees only what is not read, never what is read and should not be. These came
	/// out of writing the `FROM` clause from the published syntax rather than from the
	/// corpus, and every one was checked against SQL Server 2025 itself — `SET PARSEONLY
	/// ON` and the statement, which asks the engine the one question this is about.
	/// </para>
	/// <para>
	/// `NATURAL JOIN` and `USING` are SQL-92 and are not T-SQL at all; the engine reads the
	/// first as a correlation name and refuses what follows, and the second as an old-style
	/// table hint list. A sample stands before the hints and not after. And a bare
	/// parenthesised list after a table name is a hint list, not a column alias list —
	/// which is why the standard's spelling of a correlation may not be written here.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("SELECT a FROM t NATURAL FULL JOIN u")]
	[InlineData("SELECT a FROM t INNER JOIN u USING (id, k)")]
	[InlineData("SELECT a FROM t EXCEPT ALL SELECT b FROM u")]
	[InlineData("SELECT a FROM t INTERSECT ALL SELECT b FROM u")]
	[InlineData("SELECT a FROM t UNION CORRESPONDING SELECT b FROM u")]
	[InlineData("SELECT a FROM t JOIN u USING (id)")]
	public void What_is_the_standard_and_not_the_dialect_is_refused(string input)
	{
		Assert.True (SqlStandard92.TryParseSelect(input).IsSuccess, input);
		Assert.False(TransactSql  .TryParseSelect(input).IsSuccess, input);
	}

	/// <summary>What the engine refuses, this refuses too.</summary>
	/// <remarks>
	/// <para>
	/// Each of these was written into the grammar from the corpus or from a guess about the
	/// order of two clauses, and each was taken back out after SQL Server 2025 was asked —
	/// `SET PARSEONLY ON` and the statement, which is the one question a syntax has an
	/// answer to and needs no schema to answer.
	/// </para>
	/// <para>
	/// The first is a finding rather than a correction: <c>INNER LOCAL MERGE JOIN</c> is in
	/// ScriptDom's own test corpus, and the engine answers
	/// <c>'LOCAL' is not a recognized join option</c>. The other parser reads something the
	/// server does not, which is what comparing three sources is for and what comparing two
	/// could never have shown.
	/// </para>
	/// <para>
	/// One the engine refuses and this does not, deliberately: <c>FROM t1 AS a (c2)</c> is
	/// answered with <c>"c2" is not a recognized table hints option</c>, which settles that
	/// the parentheses are the old spelling of a hint list and not a column alias list — but
	/// refusing it is a check against the list of hint names, and this grammar reads a hint
	/// name as an identifier on purpose (see <c>TableHint</c>). So the shape is right here
	/// and the vocabulary is not checked, which is a looseness and not a defect.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("SELECT * FROM t1 INNER LOCAL MERGE JOIN t10 ON t1.c1 = t10.c1")]
	[InlineData("SELECT JSON_OBJECT (NULL ON NULL)")]
	[InlineData("SELECT JSON_ARRAY (NULL ON NULL)")]
	[InlineData("SELECT a FROM t ORDER BY a FETCH NEXT 2 ROWS ONLY")]
	[InlineData("SELECT trim(*) FROM t")]
	[InlineData("SELECT c1 FROM t1 AS a WITH (NOLOCK) TABLESAMPLE (10 PERCENT)")]
	public void What_the_engine_refuses_this_refuses_too(string input) =>
		Assert.False(TransactSql.TryParseSelect(input).IsSuccess, input);

	// ── The temporal and grouping clauses ───────────────────────────────────────

	/// <summary>What the published `FROM` and `GROUP BY` say and the examples did not.</summary>
	[Theory]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME AS OF '01/02/03'")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME FROM '2013-01-01' TO '2014-01-01'")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME BETWEEN '2013-01-01' AND '2014-01-01'")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME CONTAINED IN ('2013-01-01', '2014-01-01')")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME ALL WHERE ManagerID = 5")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME AS OF @at AS d WHERE d.a = 1")]
	[InlineData("SELECT c1 FROM t1 GROUP BY () WITH CUBE")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1 WITH ROLLUP")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1, c2 WITH CUBE")]
	[InlineData("SELECT c1 FROM t1 GROUP BY ROLLUP ((c1, c2), c3)")]
	[InlineData("SELECT c1 FROM t1 GROUP BY GROUPING SETS ((CUBE (c1), ROLLUP (c1), c1), (c1), ())")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1 WITH (DISTRIBUTED_AGG)")]
	public void The_published_clauses_read(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The JSON constructors, a named query, and three smaller things.</summary>
	/// <remarks>
	/// All written from the published syntax. `JSON_OBJECT` is the one place in T-SQL where
	/// a colon joins two values instead of introducing a parameter, and it carries two
	/// clauses an argument list has not — what a null does, and what type comes back.
	/// </remarks>
	[Theory]
	[InlineData("SELECT JSON_OBJECT ()")]
	[InlineData("SELECT JSON_OBJECT ('name':'value', 'type':1)")]
	[InlineData("SELECT JSON_OBJECT ('name':'value', 'type':NULL ABSENT ON NULL)")]
	[InlineData("SELECT JSON_OBJECT ('a':1 RETURNING json)")]
	[InlineData("SELECT JSON_OBJECT ('name':JSON_ARRAY (1, 2))")]
	[InlineData("SELECT JSON_OBJECT ('name':JSON_OBJECT ('id':1))")]
	[InlineData("SELECT JSON_ARRAY (1, 2 NULL ON NULL)")]
	[InlineData("SELECT JSON_ARRAYAGG (name ABSENT ON NULL) OVER (PARTITION BY dept) FROM t")]
	[InlineData("SELECT JSON_VALUE ('c', '$' RETURNING INT)")]

	// A query given a name in front of the statement that uses it.
	[InlineData("WITH cte AS (SELECT * FROM t) SELECT * FROM cte")]
	[InlineData("WITH cte (a, b) AS (SELECT x, y FROM t) SELECT a FROM cte")]
	[InlineData("WITH a AS (SELECT 1 AS x), b AS (SELECT 2 AS y) SELECT * FROM a, b")]
	[InlineData("WITH r (n) AS (SELECT 1 UNION ALL SELECT n + 1 FROM r) SELECT n FROM r OPTION (MAXRECURSION 2)")]
	[InlineData("WITH c AS (SELECT TOP 1 a FROM t ORDER BY a) SELECT * FROM c")]

	// And three smaller ones the work list named.
	[InlineData("SELECT * FROM t1 OPTION (OPTIMIZE FOR (@v1 = 20, @v2 = NULL))")]
	[InlineData("SELECT SUM (c1) OVER (Win1 ORDER BY c1) FROM t1 WINDOW Win1 AS (PARTITION BY c1)")]
	[InlineData("SELECT c1 INTO myDb..t2 FROM t1")]
	[InlineData("SELECT * FROM myDb..t1")]
	[InlineData("SELECT t.* FROM myDb..t1 AS t")]
	public void The_json_and_named_query_layer_reads(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The smaller shapes the work list named, one line of grammar each.</summary>
	[Theory]
	[InlineData("SELECT CASE a WHEN NULL THEN 1 ELSE 2 END FROM t")]
	[InlineData("SELECT CASE tz WHEN NULL THEN d AT TIME ZONE 'UTC' ELSE d END FROM t")]
	[InlineData("SELECT * FROM .[MyDb].dbo.t1")]
	[InlineData("SELECT [db].$PARTITION.f1 (@a) FROM t")]
	[InlineData("SELECT $PARTITION.f1 (12 + 1) FROM t")]
	[InlineData("SELECT * FROM master..sysprocesses AS p CROSS APPLY ::fn_get_sql (p.sql_handle) AS s")]
	[InlineData("WITH XMLNAMESPACES (DEFAULT 'u') SELECT c1 FROM t1")]
	[InlineData("WITH XMLNAMESPACES ('u' AS ns) SELECT c1 FROM t1 FOR XML AUTO")]
	[InlineData("SELECT a FROM t WHERE a IS DISTINCT FROM ANY (SELECT b FROM u)")]
	[InlineData("SELECT a FROM t WHERE a IS NOT DISTINCT FROM ALL (SELECT b FROM u)")]
	public void The_smaller_shapes_read(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The graph pattern, which is a drawing rather than an expression.</summary>
	/// <remarks>
	/// Every one of these is from the published `MATCH` syntax or its examples. The arrows
	/// are two characters read as two: a `&lt;-` lexeme would be produced everywhere, and
	/// `a &lt;-1` would stop being a comparison with a negative number.
	/// </remarks>
	[Theory]
	[InlineData("SELECT p2.name FROM Person p1, friend, Person p2 WHERE MATCH (p1-(friend)->p2)")]
	[InlineData("SELECT p2.name FROM Person p1, friend, Person p2 WHERE MATCH (p2<-(friend)-p1)")]
	[InlineData("SELECT a FROM P p1, f f1, P p2, f f2, P p0 WHERE MATCH (p1-(f1)->p0<-(f2)-p2)")]
	[InlineData("SELECT a FROM P p1, f f1, P p2, f f2, P p0 WHERE MATCH (p1-(f1)->p0 AND p2-(f2)->p0)")]
	[InlineData("SELECT a FROM P p1, f, P p2, P p3 WHERE MATCH (p1-(f)->p2-(f)->p3)")]
	[InlineData("SELECT a FROM P p1, f FOR PATH AS fo, P FOR PATH AS p2 WHERE MATCH (SHORTEST_PATH (p1(-(fo)->p2){1,3}))")]
	[InlineData("SELECT a FROM P p1, f FOR PATH AS fo, P FOR PATH AS p2 WHERE MATCH (SHORTEST_PATH (p1(-(fo)->p2)+))")]
	[InlineData("SELECT a FROM N n1, E e, N n2 WHERE MATCH (LAST_NODE (n1)-(e)->n2)")]
	[InlineData("SELECT a FROM N n1, N n2 WHERE MATCH (LAST_NODE (n1) = LAST_NODE (n2))")]
	[InlineData("SELECT a FROM P p1, f, P p2 WHERE MATCH (p1-(f)->p2) AND p1.name = 'Alice'")]
	[InlineData("SELECT STRING_AGG (p2.name, '->') WITHIN GROUP (GRAPH PATH) FROM P p2")]
	[InlineData("SELECT $node_id FROM Person")]
	[InlineData("SELECT p.$edge_id, p.$from_id, p.$to_id FROM friend AS p")]

	// And the comparison the arrows must not have taken away.
	[InlineData("SELECT a FROM t WHERE a < -1")]
	[InlineData("SELECT a FROM t WHERE a <-1")]
	public void The_graph_pattern_reads(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The rowset functions, and the escapes ODBC left behind.</summary>
	/// <remarks>
	/// A dozen small languages rather than one: `OPENJSON` takes a schema, `OPENROWSET` a
	/// file and a list of options, `CHANGETABLE` a word and then a table, and the full-text
	/// four a column list where a value would stand. What they share is the shape around
	/// them, so it is written once and the option names are left to the catalogue — the
	/// same trade the table hints make, and for the same reason.
	/// </remarks>
	[Theory]

	// The schema a rowset function may declare, which `OPENJSON` and `OPENXML` share.
	[InlineData("SELECT c1 FROM OPENJSON (@j) WITH (c1 INT '$.a.b[2]')")]
	[InlineData("SELECT * FROM OPENJSON (@j, 'lax $.location') WITH (street VARCHAR (500), lon INT '$.geo.longitude') AS location")]
	[InlineData("SELECT * FROM OPENJSON (@j) WITH ([Order] NVARCHAR (MAX) AS JSON)")]
	[InlineData("SELECT * FROM OPENJSON (@var)")]
	[InlineData("SELECT * FROM OPENXML (@idoc, '/ROOT/Customer', 1) WITH (CustomerID VARCHAR (10) '@id')")]
	[InlineData("SELECT * FROM OPENXML (@idoc, '/root', 1) WITH Customers")]

	// A bulk source, its options, and the older spelling that joins a provider by semicolons.
	[InlineData("SELECT * FROM OPENROWSET (BULK 'df1', SINGLE_NCLOB) AS a")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'df1', SINGLE_NCLOB, ORDER (c1 ASC)) AS a")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f', FORMATFILE = 'x', FIRSTROW = 2, ORDER (c1 ASC, c2 DESC) UNIQUE) AS a")]
	[InlineData("SELECT a.* FROM OPENROWSET ('SQLOLEDB', N'seattle1'; 'manager'; 'MyPass', 'SELECT 1') AS a")]
	[InlineData("SELECT * FROM OPENQUERY (OracleSvr, 'SELECT name FROM joe.titles') AS a")]

	// The tracking and full-text four, which take a word and then a table.
	[InlineData("SELECT * FROM CHANGETABLE (CHANGES t1, 10) AS a")]
	[InlineData("SELECT * FROM CHANGETABLE (VERSION s1.d1.dbo.t1, (c1), (1)) AS a")]
	[InlineData("SELECT * FROM CHANGETABLE (VERSION z..t1, (c1, c2), ('a', 'b')) AS a (z1, z2)")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t1, PROPERTY (c1, 'p'), 'foo', LANGUAGE 1033, 5) AS k")]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE (db1.s1.t1, (c1, c2, c3), -10) AS k")]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE (t1, *) AS t_alias")]
	[InlineData("SELECT * FROM STRING_SPLIT (NULL, N',')")]

	// What a model is asked for, and the escapes every driver has understood since before
	// this language had a standard.
	[InlineData("SELECT AI_GENERATE_EMBEDDINGS ('text' USE MODEL MyDefaultModel)")]
	[InlineData("SELECT { FN convert (@a, sql_int) }, { FN database () }")]
	[InlineData("SELECT { d '2020-01-01' }, { ts '2020-01-01 00:00:00' }")]
	[InlineData("SELECT * FROM { oj t LEFT OUTER JOIN u ON t.a = u.a }")]
	[InlineData("SELECT @a ||= 1")]
	[InlineData("SELECT dbo.f (DEFAULT, 1)")]
	public void The_rowset_functions_read(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	// ── The statements that change rows ─────────────────────────────────────────

	/// <summary>The four statements that write, read through the one entry point.</summary>
	/// <remarks>
	/// The query level was the whole of this grammar until now, and these are written on top
	/// of it rather than beside it: an `INSERT` takes a query, an `UPDATE` takes a `FROM`
	/// clause, a `MERGE` takes a table source and two search conditions. Almost nothing is
	/// new — what is new is the frame around what was already read.
	/// </remarks>
	[Theory]
	[InlineData("INSERT INTO t VALUES (1, 2), (DEFAULT, 0), (NULL, NULL)")]
	[InlineData("INSERT t (a, b) SELECT x, y FROM u")]
	[InlineData("INSERT INTO t DEFAULT VALUES")]
	[InlineData("INSERT TOP (10) INTO t (a) SELECT x FROM u")]
	[InlineData("INSERT INTO t (a) OUTPUT INSERTED.a INTO @v VALUES (1)")]
	[InlineData("INSERT INTO t WITH (TABLOCK) (a) VALUES (1)")]

	[InlineData("UPDATE t SET a = 1")]
	[InlineData("UPDATE t SET a = 1, b = DEFAULT, c = NULL WHERE d > 0")]
	[InlineData("UPDATE TOP (10) t SET a = a * 1.25, b = GETDATE ()")]
	[InlineData("UPDATE t SET a += 1, b -= 2, c ||= 'x'")]
	[InlineData("UPDATE t SET @v = a = 1")]
	[InlineData("UPDATE t SET @v += 1")]
	[InlineData("UPDATE t SET s.DocumentSummary.WRITE (N'features', 28, 10)")]
	[InlineData("UPDATE t SET a = 1 OUTPUT DELETED.a, INSERTED.a INTO @v FROM t AS x JOIN u ON x.id = u.id")]
	[InlineData("UPDATE t SET a = 1 WHERE CURRENT OF GLOBAL c1")]
	[InlineData("UPDATE @rows SET a = 1")]
	[InlineData("WITH c AS (SELECT * FROM t) UPDATE c SET a = 1")]

	[InlineData("DELETE FROM t")]
	[InlineData("DELETE t WHERE a > 1")]
	[InlineData("DELETE TOP (20) FROM t WHERE d < '20020701'")]
	[InlineData("DELETE t OUTPUT DELETED.* WHERE a = 1")]
	[InlineData("DELETE x FROM t AS x INNER JOIN u ON x.id = u.id WHERE u.a > 1")]
	[InlineData("DELETE t WHERE CURRENT OF c1")]
	[InlineData("DELETE OPENQUERY (srv, 'SELECT a FROM t')")]

	[InlineData("MERGE t AS d USING u AS s ON d.id = s.id WHEN MATCHED THEN UPDATE SET d.a = s.a")]
	[InlineData("MERGE INTO t USING u ON t.id = u.id WHEN MATCHED THEN DELETE")]
	[InlineData("MERGE t USING u ON t.id = u.id WHEN NOT MATCHED THEN INSERT (a) VALUES (1)")]
	[InlineData("MERGE t USING u ON t.id = u.id WHEN NOT MATCHED BY TARGET THEN INSERT DEFAULT VALUES")]
	[InlineData("MERGE t USING u ON t.id = u.id WHEN NOT MATCHED BY SOURCE THEN DELETE")]
	[InlineData("MERGE t USING (SELECT a, b FROM v) AS s (a, b) ON t.a = s.a WHEN MATCHED AND t.q <= 0 THEN DELETE WHEN MATCHED THEN UPDATE SET t.q = s.b")]
	[InlineData("MERGE TOP (5) t USING u ON t.id = u.id WHEN MATCHED THEN DELETE OUTPUT $ACTION, DELETED.a")]
	public void The_statements_that_write_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And a statement holds a query, and a query holds a statement.</summary>
	/// <remarks>
	/// Which is why they are one ADT and not two: `INSERT … SELECT` one way, and a merge
	/// standing where a derived table does the other.
	/// </remarks>
	[Fact]
	public void A_statement_and_a_query_hold_each_other()
	{
		var written = Assert.IsType<SqlNode.Insert>(
			TransactSql.TryParseStatement("INSERT INTO t (a) SELECT b FROM u WHERE b > 1").Value);

		Assert.Equal(new[] { "a" }, written.Columns);

		var query = Assert.IsType<SqlNode.Query>(written.Rows);

		Assert.Equal("u", Assert.IsType<SqlNode.Source>(Assert.Single(query.From)).Table);
		Assert.NotNull(query.Where);

		var merged = Assert.IsType<SqlNode.Merge>(
			TransactSql.TryParseStatement(
				"MERGE t USING u ON t.id = u.id WHEN MATCHED THEN UPDATE SET a = 1").Value);

		var arm = Assert.IsType<SqlNode.MergeWhen>(Assert.Single(merged.Whens));

		Assert.True(arm.OnMatch);
		Assert.Null(arm.Condition);

		var change = Assert.IsType<SqlNode.Update>(arm.Action);

		Assert.Equal("a", Assert.IsType<SqlNode.Assign>(Assert.Single(change.Set)).Target);
	}

	/// <summary>And the query entry point still reads only queries.</summary>
	[Theory]
	[InlineData("INSERT INTO t VALUES (1)")]
	[InlineData("UPDATE t SET a = 1")]
	[InlineData("DELETE FROM t")]
	[InlineData("MERGE t USING u ON t.id = u.id WHEN MATCHED THEN DELETE")]
	public void What_writes_is_not_a_query(string input)
	{
		Assert.False(TransactSql.TryParseSelect   (input).IsSuccess, input);
		Assert.True (TransactSql.TryParseStatement(input).IsSuccess, input);
	}

	/// <summary>The procedural level: the frame everything else stands inside.</summary>
	/// <remarks>
	/// Written before the rest of the statement language and not after, because
	/// `CREATE PROCEDURE`, `CREATE TRIGGER` and `CREATE FUNCTION` are a header and a body and
	/// the body is this — so none of the three can be read at all until it exists, and
	/// `INSERT … EXEC` cannot either.
	/// </remarks>
	[Theory]
	[InlineData("BEGIN SELECT 1 END")]
	[InlineData("BEGIN SELECT 1; SELECT 2; END")]
	[InlineData("IF @a > 1 SELECT 1")]
	[InlineData("IF @a > 1 BEGIN SELECT 1 END ELSE BEGIN SELECT 2 END")]
	[InlineData("WHILE @a > 1 BEGIN SET @a = @a - 1; IF @a = 5 BREAK; ELSE CONTINUE; END")]
	[InlineData("BEGIN TRY SELECT 1 END TRY BEGIN CATCH THROW END CATCH")]

	[InlineData("DECLARE @a INT")]
	[InlineData("DECLARE @a AS REAL = 1.2")]
	[InlineData("DECLARE @a INT, @b NVARCHAR (50) = N'x', @c AS TABLE (id INT NOT NULL, n VARCHAR (30))")]
	[InlineData("DECLARE @t TABLE (a INT IDENTITY (1, 5) NOT NULL, b AS a * 2, c MONEY DEFAULT 0)")]
	[InlineData("DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT a FROM t")]
	[InlineData("DECLARE @c AS CURSOR")]

	[InlineData("SET @a = 1")]
	[InlineData("SET @a += 1")]
	[InlineData("SET @a = (SELECT MAX (b) FROM t)")]
	[InlineData("SET ANSI_NULLS ON")]
	[InlineData("SET ANSI_NULLS, QUOTED_IDENTIFIER OFF")]
	[InlineData("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED")]
	[InlineData("SET IDENTITY_INSERT dbo.t ON")]
	[InlineData("SET ROWCOUNT 100")]
	[InlineData("SET TEXTSIZE 2048")]

	[InlineData("EXEC dbo.p")]
	[InlineData("EXECUTE dbo.p @a = 1, @b = DEFAULT, @c OUTPUT")]
	[InlineData("EXEC @rc = dbo.p 1, 2")]
	[InlineData("EXEC (N'SELECT 1')")]
	[InlineData("EXEC dbo.p WITH RECOMPILE")]
	[InlineData("EXEC dbo.p AT linked")]
	[InlineData("INSERT INTO t (a) EXEC dbo.p")]

	[InlineData("BEGIN TRANSACTION")]
	[InlineData("BEGIN TRAN t1 WITH MARK N'x'")]
	[InlineData("BEGIN DISTRIBUTED TRANSACTION")]
	[InlineData("COMMIT")]
	[InlineData("COMMIT TRANSACTION t1")]
	[InlineData("ROLLBACK TRAN @v")]
	[InlineData("SAVE TRANSACTION s1")]

	[InlineData("PRINT 'Number of rows is ' + CAST (@@ROWCOUNT AS CHAR (3))")]
	[InlineData("RETURN")]
	[InlineData("RETURN 1")]
	[InlineData("GOTO label1")]
	[InlineData("THROW 51000, 'x', 1")]
	[InlineData("RAISERROR ('x', 16, 1) WITH NOWAIT")]
	[InlineData("WAITFOR DELAY '00:00:02'")]
	[InlineData("USE master")]
	[InlineData("CHECKPOINT")]

	[InlineData("BULK INSERT t FROM 'f.dat'")]
	[InlineData("BULK INSERT dbo.t FROM 'f.dat' WITH (FIELDTERMINATOR = ',', FIRSTROW = 2, TABLOCK)")]
	public void The_procedural_level_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Tables declared and tables changed, which share their column definitions.</summary>
	/// <remarks>
	/// The largest single kind in the corpus and the one that pulls the most behind it: a
	/// column definition is what `ALTER TABLE … ADD` adds and what `ALTER TABLE … ALTER
	/// COLUMN` changes, and the table body a variable declares is the same thing smaller.
	/// The option lists are written as a shape with an open vocabulary, as the table hints
	/// are and for the same reason.
	/// </remarks>
	[Theory]
	[InlineData("CREATE TABLE t (a INT)")]
	[InlineData("CREATE TABLE dbo.t (a INT NOT NULL, b NVARCHAR (50) NULL)")]
	[InlineData("CREATE TABLE t (a INT IDENTITY (1, 5) NOT NULL, b AS a * 2 PERSISTED)")]
	[InlineData("CREATE TABLE t (a INT SPARSE, b NCHAR (10) COLLATE Albanian_BIN, c VARBINARY (MAX) FILESTREAM)")]
	[InlineData("CREATE TABLE t (a INT CONSTRAINT pk PRIMARY KEY CLUSTERED)")]
	[InlineData("CREATE TABLE t (a INT, CONSTRAINT pk PRIMARY KEY (a ASC, b DESC) WITH (FILLFACTOR = 80) ON [PRIMARY])")]
	[InlineData("CREATE TABLE t (a INT, b INT, FOREIGN KEY (a) REFERENCES u (id) ON DELETE CASCADE ON UPDATE NO ACTION)")]
	[InlineData("CREATE TABLE t (a INT REFERENCES u (id) NOT FOR REPLICATION)")]
	[InlineData("CREATE TABLE t (a INT, CHECK (a > 0))")]
	[InlineData("CREATE TABLE t (a INT DEFAULT 0, b INT CONSTRAINT df DEFAULT 1)")]
	[InlineData("CREATE TABLE t (a INT, INDEX ix NONCLUSTERED (a) WHERE a > 0 WITH (DATA_COMPRESSION = PAGE))")]
	[InlineData("CREATE TABLE t (a INT, INDEX ix CLUSTERED COLUMNSTORE ORDER (a))")]
	[InlineData("CREATE TABLE t (a INT, s DATETIME2 GENERATED ALWAYS AS ROW START, e DATETIME2 GENERATED ALWAYS AS ROW END, PERIOD FOR SYSTEM_TIME (s, e)) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.h))")]
	[InlineData("CREATE TABLE t (a INT) ON ps (a) TEXTIMAGE_ON [PRIMARY]")]
	[InlineData("CREATE TABLE t (a XML COLUMN_SET FOR ALL_SPARSE_COLUMNS)")]
	[InlineData("CREATE TABLE n1 (c1 INT) AS NODE")]
	[InlineData("CREATE TABLE e01 (c1 INT, CONSTRAINT cnst CONNECTION (N1 TO N2)) AS EDGE")]
	[InlineData("CREATE TABLE t AS FILETABLE WITH (FILETABLE_DIRECTORY = 'd')")]
	[InlineData("CREATE TABLE t (c1 INT, INDEX idx NONCLUSTERED ($NODE_ID))")]
	[InlineData("CREATE TABLE t (i INT NOT NULL INDEX ix NONCLUSTERED HASH WITH (BUCKET_COUNT = 16))")]
	[InlineData("ALTER TABLE t REBUILD WITH (ONLINE = ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1440 MINUTES, ABORT_AFTER_WAIT = NONE)))")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1 WITH (DISTRIBUTED_AGG), c2")]
	[InlineData("CREATE TABLE t (a INT) WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1 TO 4, 6))")]

	[InlineData("ALTER TABLE t ADD c5 VARBINARY (MAX) FILESTREAM")]
	[InlineData("ALTER TABLE t ADD CONSTRAINT pk PRIMARY KEY (a)")]
	[InlineData("ALTER TABLE t WITH NOCHECK ADD CHECK (a > 0)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 INT NOT NULL")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 ADD SPARSE")]
	[InlineData("ALTER TABLE t DROP COLUMN a, CONSTRAINT c")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT IF EXISTS c")]
	[InlineData("ALTER TABLE t NOCHECK CONSTRAINT ALL")]
	[InlineData("ALTER TABLE t ENABLE TRIGGER tr1, tr2")]
	[InlineData("ALTER TABLE t SET (SYSTEM_VERSIONING = OFF)")]
	[InlineData("ALTER TABLE t REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = ROW)")]
	[InlineData("ALTER TABLE t SWITCH PARTITION 1 TO u PARTITION 2")]

	// And the table a variable declares, which is the same body read the same way.
	[InlineData("DECLARE @t TABLE (a INT PRIMARY KEY, b AS a * 2, CHECK (a > 0))")]
	public void The_table_statements_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And a table says what is in it.</summary>
	[Fact]
	public void A_created_table_says_its_columns()
	{
		var made = Assert.IsType<SqlNode.CreateTable>(
			TransactSql.TryParseStatement(
				"CREATE TABLE dbo.t (a INT NOT NULL, b AS a * 2, CONSTRAINT pk PRIMARY KEY (a))").Value);

		Assert.Equal("dbo.t", made.Name);
		Assert.Equal(3, made.Elements.Length);

		var first = Assert.IsType<SqlNode.Field>(made.Elements[0]);

		Assert.Equal("a", first.Name);
		Assert.Equal("INT", first.Type);
		Assert.Null(first.Computed);

		var second = Assert.IsType<SqlNode.Field>(made.Elements[1]);

		Assert.Equal("b", second.Name);
		Assert.Null(second.Type);
		Assert.NotNull(second.Computed);

		var third = Assert.IsType<SqlNode.Constraint>(made.Elements[2]);

		Assert.Equal("pk", third.Name);
		Assert.Equal("PRIMARY KEY", third.Kind);
		Assert.Equal(new[] { "a" }, third.Columns);
	}

	/// <summary>The four that are a header and a body.</summary>
	/// <remarks>
	/// None of them could be read at all until the procedural level existed, because each is
	/// a header and then whatever T-SQL somebody put inside it — 341 statements of the corpus
	/// between them, and the body was the part already done.
	/// </remarks>
	[Theory]
	[InlineData("CREATE PROCEDURE p AS SELECT 1")]
	[InlineData("CREATE PROC dbo.p @a INT, @b NVARCHAR (50) = N'x' OUTPUT AS BEGIN SELECT @a END")]
	[InlineData("CREATE PROCEDURE p (@a INT = 0 READONLY) WITH RECOMPILE, ENCRYPTION AS SELECT 1")]
	[InlineData("CREATE OR ALTER PROCEDURE p AS SELECT 1")]
	[InlineData("ALTER PROCEDURE p AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p WITH EXECUTE AS OWNER AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p AS EXTERNAL NAME asm.cls.method")]
	[InlineData("CREATE PROCEDURE p AS BEGIN INSERT INTO t1 VALUES (1, 2), (DEFAULT, 0); RETURN 0 END")]

	[InlineData("CREATE FUNCTION f (@a INT) RETURNS INT AS BEGIN RETURN @a * 2 END")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE AS RETURN (SELECT a FROM t)")]
	[InlineData("CREATE FUNCTION f (@a INT) RETURNS TABLE RETURN SELECT a FROM t WHERE b = @a")]
	[InlineData("CREATE FUNCTION f () RETURNS @r TABLE (a INT NOT NULL) AS BEGIN INSERT INTO @r VALUES (1); RETURN END")]
	[InlineData("CREATE FUNCTION f (@a INT) RETURNS INT WITH SCHEMABINDING, RETURNS NULL ON NULL INPUT AS BEGIN RETURN 1 END")]

	[InlineData("CREATE TRIGGER tr ON Sales.Customer AFTER INSERT, UPDATE AS RAISERROR ('x', 16, 10)")]
	[InlineData("CREATE TRIGGER tr ON t INSTEAD OF DELETE AS SELECT 1")]
	[InlineData("CREATE TRIGGER tr ON t FOR INSERT NOT FOR REPLICATION AS SELECT 1")]
	[InlineData("CREATE TRIGGER safety ON DATABASE FOR DROP_SYNONYM AS RAISERROR ('x', 10, 1)")]
	[InlineData("CREATE TRIGGER tr ON ALL SERVER FOR CREATE_DATABASE AS PRINT 'made'")]
	[InlineData("CREATE TRIGGER tr ON ALL SERVER WITH EXECUTE AS 'login_test' FOR LOGON AS BEGIN ROLLBACK END")]

	[InlineData("CREATE VIEW v AS SELECT a FROM t")]
	[InlineData("CREATE VIEW dbo.v (a, b) WITH SCHEMABINDING AS SELECT x, y FROM t WITH CHECK OPTION")]
	[InlineData("CREATE OR ALTER VIEW v AS SELECT a FROM t UNION SELECT b FROM u")]
	public void The_routines_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Indexes and permissions, which share almost everything with what came before.</summary>
	/// <remarks>
	/// An index inside a `CREATE TABLE` is the same thing said in the same words, so only the
	/// header is new. And a permission's *name* is made of words T-SQL reserves — `ALTER ANY
	/// DATABASE`, `VIEW SERVER STATE`, `BACKUP LOG` — which an identifier cannot be, so they
	/// are named the way the option names are.
	/// </remarks>
	[Theory]
	[InlineData("CREATE INDEX ix ON t (a)")]
	[InlineData("CREATE UNIQUE CLUSTERED INDEX ix ON dbo.t (a ASC, b DESC) INCLUDE (c) WHERE a > 0")]
	[InlineData("CREATE INDEX ix ON t (a) WITH (PAD_INDEX = ON, FILLFACTOR = 80, ONLINE = ON) ON ps (a)")]
	[InlineData("CREATE INDEX ix ON t (a) WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1 TO 4))")]
	[InlineData("CREATE CLUSTERED COLUMNSTORE INDEX cix ON t")]
	[InlineData("CREATE NONCLUSTERED COLUMNSTORE INDEX cix ON t (a, b) WHERE a > 0")]
	[InlineData("ALTER INDEX ALL ON t REBUILD PARTITION = ALL")]
	[InlineData("ALTER INDEX ix ON t REORGANIZE WITH (LOB_COMPACTION = ON)")]
	[InlineData("ALTER INDEX ix ON t DISABLE")]
	[InlineData("ALTER INDEX ix ON t SET (ALLOW_PAGE_LOCKS = OFF)")]
	[InlineData("ALTER INDEX ix ON t RESUME WITH (MAXDOP = 2)")]

	[InlineData("GRANT SELECT ON t TO u")]
	[InlineData("GRANT SELECT (a, b), UPDATE ON dbo.t TO u, v WITH GRANT OPTION")]
	[InlineData("GRANT alter ON SERVER ROLE::serverRole1 TO serverRole2")]
	[InlineData("GRANT EXECUTE ON OBJECT::dbo.p TO PUBLIC AS dbo")]
	[InlineData("GRANT VIEW SERVER STATE TO login_test")]
	[InlineData("GRANT ALTER ANY DATABASE DDL TRIGGER TO u")]
	[InlineData("GRANT ALL PRIVILEGES ON t TO u")]
	[InlineData("DENY VIEW DEFINITION ON SCHEMA::s TO u CASCADE")]
	[InlineData("REVOKE GRANT OPTION FOR SELECT ON t FROM u CASCADE AS dbo")]
	[InlineData("REVOKE CREATE TABLE FROM u")]
	public void The_indexes_and_permissions_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The database, which is a header and an option list.</summary>
	/// <remarks>
	/// The settings are not enumerated and the rule is not about which of them exist: a
	/// setting is a run of words and then, sometimes, what it is set to. What the syntax
	/// does say is where the run ends — `ON` and `OFF` are values, and `WITH` begins the
	/// termination clause — and that is what the lookahead in `DatabaseOptionName` is.
	/// </remarks>
	[Theory]
	[InlineData("CREATE DATABASE d1")]
	[InlineData("CREATE DATABASE d1 CONTAINMENT = PARTIAL")]
	[InlineData("CREATE DATABASE d1 COLLATE Estonian_CS_AS")]
	[InlineData("CREATE DATABASE d1 ON PRIMARY (NAME = a, FILENAME = 'a.mdf', SIZE = 10 MB, " +
		"MAXSIZE = 1 GB, FILEGROWTH = 10 %) LOG ON (NAME = b, FILENAME = 'b.ldf')")]
	[InlineData("CREATE DATABASE d1 ON (FILENAME = 'a.mdf'), (FILENAME = 'b.ldf') FOR ATTACH WITH ENABLE_BROKER")]
	[InlineData("CREATE DATABASE d1 ON PRIMARY (NAME = a, FILENAME = 'a'), " +
		"FILEGROUP fg CONTAINS FILESTREAM (NAME = b, FILENAME = 'b')")]
	[InlineData("CREATE DATABASE s1 ON (NAME = a, FILENAME = 'a.ss') AS SNAPSHOT OF d1")]

	[InlineData("ALTER DATABASE d1 SET SINGLE_USER")]
	[InlineData("ALTER DATABASE d1 SET SINGLE_USER WITH ROLLBACK IMMEDIATE")]
	[InlineData("ALTER DATABASE d1 SET READ_ONLY WITH ROLLBACK AFTER 10 SECONDS")]
	[InlineData("ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 160")]
	[InlineData("ALTER DATABASE d1 SET ENCRYPTION ON, ENCRYPTION OFF")]
	[InlineData("ALTER DATABASE d1 SET HADR AVAILABILITY GROUP = g1")]
	[InlineData("ALTER DATABASE d1 SET HADR SUSPEND")]
	[InlineData("ALTER DATABASE d1 SET TARGET_RECOVERY_TIME = 42 SECONDS")]
	[InlineData("ALTER DATABASE d1 SET CHANGE_TRACKING (CHANGE_RETENTION = 3 DAYS, AUTO_CLEANUP = OFF)")]
	[InlineData("ALTER DATABASE d1 SET QUERY_STORE = ON (DESIRED_STATE = READ_ONLY, MAX_PLANS_PER_QUERY = 200)")]
	[InlineData("ALTER DATABASE d1 SET QUERY_STORE CLEAR ALL")]
	[InlineData("ALTER DATABASE d1 SET AUTO_CREATE_STATISTICS ON (INCREMENTAL = ON), AUTO_UPDATE_STATISTICS ON")]
	[InlineData("ALTER DATABASE d1 COLLATE Estonian_CS_AS")]
	[InlineData("ALTER DATABASE d1 MODIFY NAME = d2")]
	[InlineData("ALTER DATABASE d1 MODIFY (MAXSIZE = 1 GB, EDITION = 'basic')")]
	[InlineData("ALTER DATABASE d1 MODIFY FILEGROUP fg1 AUTOGROW_ALL_FILES")]
	[InlineData("ALTER DATABASE d1 MODIFY FILEGROUP fg1 READ_ONLY WITH ROLLBACK AFTER 10 SECONDS")]
	[InlineData("ALTER DATABASE d1 ADD FILEGROUP fg1 CONTAINS MEMORY_OPTIMIZED_DATA")]
	[InlineData("ALTER DATABASE d1 ADD FILE (FILENAME = 'a', NAME = b) TO FILEGROUP [MY FILEGROUP]")]
	[InlineData("ALTER DATABASE d1 ADD LOG FILE (FILENAME = 'log'), (FILENAME = 'log2')")]
	[InlineData("ALTER DATABASE d1 REMOVE FILE a")]
	[InlineData("ALTER DATABASE d1 REBUILD LOG")]
	[InlineData("ALTER DATABASE SCOPED COLLATE Estonian_CS_AS")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 1")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE")]
	public void The_database_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// And a statement that only begins like one of these is left for whoever reads it.
	/// </summary>
	/// <remarks>
	/// Each is a statement kind of its own — an audit specification, an encryption key —
	/// and the rules here must not take them for a database called `AUDIT` doing something
	/// called `SPECIFICATION`. That is why the two word-only actions are written out and
	/// not read as `word`: a rule wide enough for `REBUILD LOG` is wide enough for these.
	/// </remarks>
	[Theory]
	[InlineData("ALTER DATABASE d1 AUDIT SPECIFICATION s1")]
	[InlineData("ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = AES_256")]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_128 ENCRYPTION BY SERVER CERTIFICATE c1")]
	public void And_what_only_begins_like_a_database_is_left_alone(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>What a second reading of the published syntax found missing.</summary>
	/// <remarks>
	/// Written clause by clause against Microsoft's own blocks rather than from the corpus:
	/// each of these is a line of `<column_definition>`, `<column_constraint>`,
	/// `ALTER TABLE`, `CREATE PROCEDURE` or `<data_type>` that the grammar did not have.
	/// The corpus is what noticed; the syntax is what said what to write.
	/// </remarks>
	[Theory]
	// `<data_type>`: xml takes a schema collection where everything else takes a precision.
	[InlineData("CREATE TABLE t (c1 XML (CONTENT dbo.sc), c2 XML (sc), c3 XML)")]

	// `[ [ CONSTRAINT constraint_name ] { NULL | NOT NULL } ]` — a nullability may be named.
	[InlineData("CREATE TABLE t (c1 INT CONSTRAINT nn NOT NULL)")]

	// `GENERATED ALWAYS AS { ROW | TRANSACTION_ID | SEQUENCE_NUMBER } { START | END } [ HIDDEN ]`,
	// and the two words the ledger adds to that list.
	[InlineData("CREATE TABLE t (a DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 VARBINARY (85) GENERATED ALWAYS AS SUSER_SID START")]

	// `HIDDEN` stands with `SPARSE` and the rest, after `ADD` and `DROP` and on its own.
	[InlineData("ALTER TABLE t ALTER COLUMN c1 INT HIDDEN NULL")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 ADD HIDDEN")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 DROP MASKED")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 ADD ROWGUIDCOL WITH (ONLINE = ON)")]

	// The published `ALTER TABLE` actions that were not there.
	[InlineData("ALTER TABLE t ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)")]
	[InlineData("ALTER TABLE t DISABLE FILETABLE_NAMESPACE")]
	[InlineData("ALTER TABLE t ALTER INDEX ix REBUILD WITH (BUCKET_COUNT = 1)")]
	[InlineData("ALTER TABLE t MERGE RANGE (NULL)")]
	[InlineData("ALTER TABLE t SPLIT RANGE (10)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO fg, ONLINE = ON)")]

	// `DECLARE` gives a variable a nullability, which nothing about a variable suggests.
	[InlineData("DECLARE @v AS INT NOT NULL = 4")]
	[InlineData("DECLARE @v AS INT NULL")]

	// `[ NULL | NOT NULL ] [ = default ]`, in that order, from the natively compiled form.
	[InlineData("CREATE PROCEDURE p @p1 INT, @p2 INT NULL = NULL, @p3 INT NOT NULL AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS " +
		"BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'us_english') SELECT 1 END")]

	// An option's name is a run of words in four places at once, and its value may be a list.
	[InlineData("ALTER DATABASE db SET QUERY_STORE (CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 367))")]
	[InlineData("CREATE TABLE t (a INT) WITH (CLUSTERED COLUMNSTORE INDEX, DISTRIBUTION = HASH(a))")]

	// The selective XML index, which is the one index whose shape is its own.
	[InlineData("CREATE SELECTIVE XML INDEX sxi ON t (c) FOR (path1 = '/a/b')")]
	[InlineData("CREATE SELECTIVE XML INDEX sxi ON t (c) WITH XMLNAMESPACES ('urn:a' AS ns) " +
		"FOR (p1 = '/a/b' AS XQUERY 'xs:double', p2 = '/a/c' AS XQUERY 'xs:string' MAXLENGTH (200) SINGLETON, " +
		"p3 = '/a/d' AS SQL NVARCHAR (100)) WITH (PAD_INDEX = ON)")]
	[InlineData("ALTER INDEX sxi ON t FOR (REMOVE path1)")]
	[InlineData("ALTER INDEX sxi ON t WITH XMLNAMESPACES ('urn:a' AS ns) FOR (ADD p9 = '/a/e')")]
	[InlineData("CREATE PRIMARY XML INDEX pxi ON t (c)")]
	[InlineData("CREATE XML INDEX xi ON t (c) USING XML INDEX pxi FOR PATH")]

	// A warehouse's table and view, which say the distribution before the query.
	[InlineData("CREATE TABLE dbo.t1 (c1, c2) WITH (DISTRIBUTION = ROUND_ROBIN) AS SELECT a, b FROM u")]
	[InlineData("CREATE MATERIALIZED VIEW v WITH (DISTRIBUTION = HASH(c5)) AS SELECT c5 FROM t")]

	// `<collate clause>` follows a character expression and not only a bare column.
	[InlineData("SELECT a COLLATE Albanian_BIN")]
	[InlineData("SELECT (a) COLLATE Albanian_BIN")]
	[InlineData("SELECT t.a COLLATE Albanian_BIN")]

	// `WITH CHANGE_TRACKING_CONTEXT ( context )` in front of the statement, on its own.
	[InlineData("WITH CHANGE_TRACKING_CONTEXT (0xff) INSERT INTO t (a) VALUES (1)")]

	// `SET LANGUAGE us_english` — a setting and one value, which a third of them take.
	[InlineData("SET LANGUAGE us_english")]
	[InlineData("SET DATEFORMAT mdy")]
	[InlineData("COMMIT TRANSACTION WITH (DELAYED_DURABILITY = ON)")]
	public void The_published_syntax_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And where the engine and ScriptDom disagree, the engine wins above 90.</summary>
	/// <remarks>
	/// ScriptDom reads a cursor variable with a default and a `CHANGE_TRACKING_CONTEXT`
	/// joined to a named query by a comma. The engine answers `Incorrect syntax` to both,
	/// and the published syntax gives neither — a cursor has no default, and the context
	/// stands in front of the statement on its own.
	/// </remarks>
	[Theory]
	[InlineData("DECLARE @c AS CURSOR = 'x'")]
	[InlineData("WITH CHANGE_TRACKING_CONTEXT (0xff), c (a) AS (SELECT a FROM t) INSERT INTO u (a) SELECT a FROM c")]
	public void And_what_only_ScriptDom_reads_is_refused(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>`NULL` is a constant of this dialect and stands wherever a value does.</summary>
	/// <remarks>
	/// The standard's is a <c>&lt;null specification&gt;</c> and stands in a handful of
	/// named places — a row constructor, a `CAST` operand, a `SET`. Microsoft's expression
	/// reference lists it among the constants, which is a different language: `1 + NULL` is
	/// an expression, `trim('[]' FROM NULL)` is a call, and `DEFAULT NULL` is a constraint
	/// whose value is one. Found by the corpus refusing all three.
	/// </remarks>
	[Theory]
	[InlineData("SELECT NULL")]
	[InlineData("SELECT 1 + NULL")]
	[InlineData("SELECT TRIM('[]' FROM NULL)")]
	[InlineData("SELECT COALESCE(NULL, 1)")]
	[InlineData("INSERT INTO t (a) VALUES (NULL)")]
	[InlineData("CREATE TABLE t (a INT CONSTRAINT d DEFAULT NULL)")]

	// A table-valued function written in the CLR declares its columns where an inline one
	// would say `RETURN`, and `ORDER` says what the assembly promises about them.
	[InlineData("CREATE FUNCTION f () RETURNS TABLE (c1 INT) AS EXTERNAL NAME a.b.c")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE (c1 INT) ORDER (c1 ASC) AS EXTERNAL NAME a.b.c")]

	// And a table-valued method on a variable of a user-defined type.
	[InlineData("SELECT c1 FROM @v.f(1) AS t (c)")]
	[InlineData("SELECT c1 FROM @v.f() AS t")]
	[InlineData("SELECT c1 FROM @rows")]
	public void And_the_rest_of_the_second_reading(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A member reached through `::`, and everything that may follow one.</summary>
	/// <remarks>
	/// `SELECT t::a` read and `SELECT t::a + 1` did not, which was not the grammar:
	/// <c>("::" | '.') &amp; Identifier</c> was being taken for a set of two literals and
	/// every call site became that range, with the identifier after it gone. See
	/// <see cref="SemanticTests.A_choice_that_reads_more_than_its_literals_is_not_a_set"/>.
	/// </remarks>
	[Theory]
	[InlineData("SELECT t::a")]
	[InlineData("SELECT t::a + 1")]
	[InlineData("SELECT t::a AS q")]
	[InlineData("SELECT t::f()")]
	[InlineData("SELECT t::a COLLATE Albanian_BIN")]
	[InlineData("SELECT t::a, t2::f(), dbo.[type 1]::[Property]")]
	[InlineData("SELECT (c1).SomeProperty")]
	public void A_member_reads_and_the_value_goes_on(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>`DROP`, which is sixty-six statements and one shape.</summary>
	/// <remarks>
	/// Written from the sixty-six published blocks, which agree about everything but the
	/// words in the middle. Those are written out and here they are the syntax rather than a
	/// catalogue: `DROP TABLE` and `DROP VIEW` are different statements, `DROP MASTER KEY`
	/// names no object at all, and a rule wide enough for any word would read `DROP FOO x`
	/// and call it one of them.
	/// </remarks>
	[Theory]
	[InlineData("DROP TABLE t")]
	[InlineData("DROP TABLE IF EXISTS dbo.t, dbo.u")]
	[InlineData("DROP VIEW IF EXISTS v")]
	[InlineData("DROP PROC p")]
	[InlineData("DROP PROCEDURE IF EXISTS dbo.p, dbo.q")]
	[InlineData("DROP FUNCTION IF EXISTS f")]
	[InlineData("DROP DATABASE IF EXISTS d1, d2")]
	[InlineData("DROP DATABASE AUDIT SPECIFICATION s")]
	[InlineData("DROP DATABASE SCOPED CREDENTIAL c")]
	[InlineData("DROP SERVER AUDIT SPECIFICATION s")]
	[InlineData("DROP SERVER AUDIT a")]
	[InlineData("DROP SERVER ROLE r")]
	[InlineData("DROP XML SCHEMA COLLECTION dbo.sc")]
	[InlineData("DROP SEARCH PROPERTY LIST pl")]
	[InlineData("DROP CRYPTOGRAPHIC PROVIDER p")]
	[InlineData("DROP EXTERNAL DATA SOURCE ds")]
	[InlineData("DROP EXTERNAL FILE FORMAT ff")]
	[InlineData("DROP EXTERNAL RESOURCE POOL rp")]
	[InlineData("DROP WORKLOAD CLASSIFIER wc")]

	// The five that say something after the names.
	[InlineData("DROP ASYMMETRIC KEY k REMOVE PROVIDER KEY")]
	[InlineData("DROP SYMMETRIC KEY k")]
	[InlineData("DROP ASSEMBLY IF EXISTS a1, a2 WITH NO DEPENDENTS")]
	[InlineData("DROP EXTERNAL LIBRARY l AUTHORIZATION dbo")]
	[InlineData("DROP EVENT SESSION s ON SERVER")]
	[InlineData("DROP EVENT NOTIFICATION n1, n2 ON QUEUE dbo.q")]

	// The two that name nothing: there is one of each per database.
	[InlineData("DROP MASTER KEY")]
	[InlineData("DROP DATABASE ENCRYPTION KEY")]

	// An index is named twice, and the older spelling puts both in one qualified name.
	[InlineData("DROP INDEX ix ON dbo.t")]
	[InlineData("DROP INDEX IF EXISTS ix ON t WITH (ONLINE = ON, MOVE TO fg)")]
	[InlineData("DROP INDEX dbo.t.ix")]
	[InlineData("DROP INDEX ix1 ON t1, ix2 ON t2")]
	[InlineData("DROP FULLTEXT INDEX ON dbo.t")]

	// A signature comes off a module and a classification off a column.
	[InlineData("DROP SIGNATURE FROM dbo.p BY CERTIFICATE c")]
	[InlineData("DROP COUNTER SIGNATURE FROM dbo.p BY ASYMMETRIC KEY k, CERTIFICATE c")]
	[InlineData("DROP SENSITIVITY CLASSIFICATION FROM dbo.t.c1, dbo.t.c2")]

	// And a trigger says which of the three kinds it is by what it is on.
	[InlineData("DROP TRIGGER IF EXISTS tr1, tr2")]
	[InlineData("DROP TRIGGER tr ON DATABASE")]
	[InlineData("DROP TRIGGER tr ON ALL SERVER")]
	public void The_drop_family_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And a word that names no object of this language is not one.</summary>
	[Theory]
	[InlineData("DROP FOO x")]
	[InlineData("DROP TABLE")]
	[InlineData("DROP MASTER KEY k")]
	public void And_a_drop_of_nothing_is_refused(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	// ── And builds the standard's tree ───────────────────────────────────────────

	/// <summary>
	/// A call says what it calls and with what, which is the one node the dialect adds.
	/// </summary>
	[Fact]
	public void A_call_says_its_name_and_its_arguments()
	{
		var query = Assert.IsType<SqlNode.Query>(
			TransactSql.TryParseSelect("SELECT dbo.f(a, 1) FROM t").Value);

		var call = Assert.IsType<SqlNode.Call>(
			Assert.IsType<SqlNode.Selected>(Assert.Single(query.Columns)).Value);

		Assert.Equal("dbo.f", call.Name);
		Assert.Equal(2, call.Arguments.Length);
		Assert.Equal("a", Assert.IsType<SqlNode.Column>(call.Arguments[0]).Text);
	}

	/// <summary>A variable is a parameter, which is the rule it was widened into.</summary>
	[Theory]
	[InlineData("SELECT @v",         "@v")]
	[InlineData("SELECT @@ROWCOUNT", "@@ROWCOUNT")]
	public void A_variable_stands_where_a_parameter_does(string input, string text)
	{
		var query = Assert.IsType<SqlNode.Query>(TransactSql.TryParseSelect(input).Value);

		var literal = Assert.IsType<SqlNode.Literal>(
			Assert.IsType<SqlNode.Selected>(Assert.Single(query.Columns)).Value);

		Assert.Equal(SqlLiteralKind.Parameter, literal.Kind);
		Assert.Equal(text, literal.Text);
	}

	/// <summary>
	/// And what is read and dropped is dropped: the tree a dialect builds is the standard's.
	/// </summary>
	/// <remarks>
	/// <c>TOP</c> says how many rows come back and <c>OVER</c> says which rows a call sees;
	/// neither says what the rows or the call are. So both read and neither reaches the
	/// tree, and what comes back is the same query as without them.
	/// </remarks>
	[Fact]
	public void A_count_and_a_window_are_read_and_dropped()
	{
		// Node by node rather than query by query: a `Query` holds arrays, and a record
		// compares those by reference, so two readings of the same text are never equal.
		var topped = Query("SELECT TOP 10 PERCENT a FROM t");
		var plain  = Query("SELECT a FROM t");

		Assert.Equal(Assert.Single(plain.Columns), Assert.Single(topped.Columns));
		Assert.Equal(Assert.Single(plain.From),    Assert.Single(topped.From));

		Assert.Equal(
			Assert.Single(Query("SELECT COUNT(*) FROM t")                      .Columns),
			Assert.Single(Query("SELECT COUNT(*) OVER (PARTITION BY b) FROM t").Columns));
	}

	/// <summary>What the dialect read, where it was a query.</summary>
	static SqlNode.Query Query(string input) =>
		Assert.IsType<SqlNode.Query>(TransactSql.TryParseSelect(input).Value);

	/// <summary>A bracketed name may be a reserved word, which is what brackets are for.</summary>
	/// <remarks>
	/// The lookahead that refuses <c>SELECT</c> as a name stands in front of the regular
	/// spelling only, exactly as it does in the standard.
	/// </remarks>
	[Fact]
	public void A_bracketed_name_may_be_a_reserved_word()
	{
		var query = Assert.IsType<SqlNode.Query>(
			TransactSql.TryParseSelect("SELECT [select] FROM t").Value);

		Assert.Equal(
			"[select]",
			Assert.IsType<SqlNode.Column>(
				Assert.IsType<SqlNode.Selected>(Assert.Single(query.Columns)).Value).Text);

		Assert.False(TransactSql.TryParseSelect("SELECT select FROM t").IsSuccess);
	}

	/// <summary>A table variable is where the rows come from.</summary>
	[Fact]
	public void A_table_variable_is_a_source()
	{
		var query = Assert.IsType<SqlNode.Query>(
			TransactSql.TryParseSelect("SELECT a FROM @rows AS r").Value);

		var source = Assert.IsType<SqlNode.Source>(Assert.Single(query.From));

		Assert.Equal("@rows", source.Table);
		Assert.Equal("r", source.Name);
	}

	// ── And the other three publications came across ─────────────────────────────

	/// <summary>
	/// A dialect publishes four entry points, and each is the standard's rule read through
	/// the substitution rather than a rule of its own.
	/// </summary>
	[Fact]
	public void Every_entry_point_reads_the_dialect()
	{
		Assert.True(TransactSql.TryParseQuery          ("SELECT [a] FROM t UNION SELECT @v").IsSuccess);
		Assert.True(TransactSql.TryParseSearchCondition("[a] > dbo.f(@v)")                  .IsSuccess);
		Assert.True(TransactSql.TryParseValueExpression("dbo.f(@v) + [b]")                  .IsSuccess);

		Assert.False(SqlStandard92.TryParseSearchCondition("[a] > dbo.f(@v)").IsSuccess);
		Assert.False(SqlStandard92.TryParseValueExpression("dbo.f(@v) + [b]").IsSuccess);
	}

	/// <summary>And what is not read yet is refused, where the reading stopped.</summary>
	[Theory]
	[InlineData("SELECT TOP")]
	[InlineData("SELECT [a")]
	[InlineData("SELECT @")]
	public void What_is_not_read_yet_is_refused(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.False(match.IsSuccess, input);
		Assert.InRange(match.Position, 0, input.Length);
	}
}
