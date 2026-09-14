using System;

using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// The SQL:2023 grammar, read: every row here is answered the same way by the standard's BNF.
/// </summary>
/// <remarks>
/// <para>
/// The verdicts are not this suite's own. Each row was put to `--standard` in DotGram.Benchmarks,
/// which reads the published BNF with an Earley recognizer and asks the grammar's rule of the same
/// name beside it, and the two agreed. A row that is added here is added there first.
/// </para>
/// <para>
/// Verdicts and not trees — the grammar builds none yet.
/// </para>
/// </remarks>
public sealed class SqlStandardParserTests
{
	// ── §5.3 Literals ────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("1")]
	[InlineData("-1")]
	[InlineData("+ 1.5")]
	[InlineData(".5")]
	[InlineData("1.")]
	[InlineData("1E5")]
	[InlineData("1e5")]
	[InlineData("1.5E-3")]
	[InlineData("1.e5")]
	[InlineData("1E+5")]
	[InlineData("0X1F")]
	[InlineData("0x1f")]
	[InlineData("0X_1")]
	[InlineData("0O17")]
	[InlineData("0B101")]
	[InlineData("1_000")]
	[InlineData("'abc'")]
	[InlineData("''")]
	[InlineData("'it''s'")]
	[InlineData("'a' 'b'")]
	[InlineData("'a'/* c */'b'")]
	[InlineData("'a' -- c")]
	[InlineData("N'abc'")]
	[InlineData("n'abc'")]
	[InlineData("N'a' 'b'")]
	[InlineData("X'0A1b'")]
	[InlineData("X'0 A'")]
	[InlineData("X'0A' 'B1'")]
	[InlineData("U&'\\0041'")]
	[InlineData("U&'a' 'b'")]
	[InlineData("_latin1'abc'")]
	[InlineData("_s.latin1'abc'")]
	[InlineData("_\"s\".latin1'a'")]
	[InlineData("_absolute.latin1'a'")]
	[InlineData("DATE '2020-01-01'")]
	[InlineData("DATE '2020-1-1'")]
	[InlineData("date'2020-01-01'")]
	[InlineData("TIME '12:00:00'")]
	[InlineData("TIME '12:00:00.'")]
	[InlineData("TIME '1:2:3'")]
	[InlineData("TIME '12:00:00+01:00'")]
	[InlineData("TIMESTAMP '2020-01-01 12:00:00.5'")]
	[InlineData("INTERVAL '1' DAY")]
	[InlineData("INTERVAL -'1-2' YEAR TO MONTH")]
	[InlineData("INTERVAL '-1' YEAR")]
	[InlineData("INTERVAL - '1' YEAR")]
	[InlineData("INTERVAL '1 2:3:4.5' DAY TO SECOND(3)")]
	[InlineData("INTERVAL '1 2' DAY TO HOUR")]
	[InlineData("INTERVAL '1:2' HOUR TO MINUTE")]
	[InlineData("INTERVAL '5.5' SECOND")]
	[InlineData("INTERVAL '5' SECOND(2, 3)")]
	[InlineData("INTERVAL '1' YEAR(2) TO MONTH")]
	[InlineData("INTERVAL '5' MONTH TO YEAR")]
	[InlineData("TRUE")]
	[InlineData("FALSE")]
	[InlineData("unknown")]
	[InlineData("- /* c */ 1")]
	[InlineData("/* a /* b */ c */ 1")]
	public void A_literal_reads(string input)
	{
		Assert.True(SqlStandardParser.TryParseLiteral(input).IsSuccess, input);
	}

	/// <summary>What the BNF refuses, and why it does.</summary>
	/// <remarks>
	/// A digit follows one underscore and not two; hexits come in pairs; SQL:2023 has no bit
	/// string; the escape character a `UESCAPE` names is not read; nothing stands between an
	/// introducer and its name, or its name and the quote, and the name is no reserved word; a
	/// date string is a date; a timestamp's two halves have one space between them; a second
	/// is not a field to end at; `NULL` is no literal; `2K` is a large object's length.
	/// </remarks>
	[Theory]
	[InlineData("1__0")]
	[InlineData("0X")]
	[InlineData(".E5")]
	[InlineData("X'0'")]
	[InlineData("B'101'")]
	[InlineData("U&'#0041' UESCAPE '#'")]
	[InlineData("_ latin1 'abc'")]
	[InlineData("_latin1 'a'")]
	[InlineData("_select.latin1'a'")]
	[InlineData("_a.b.c.latin1'a'")]
	[InlineData("DATE 'x'")]
	[InlineData("TIMESTAMP '2020-01-01  12:00:00'")]
	[InlineData("INTERVAL '1' SECOND TO SECOND")]
	[InlineData("NULL")]
	[InlineData("abc")]
	[InlineData("'abc")]
	[InlineData("1 2")]
	[InlineData("2K")]
	public void What_is_not_a_literal_is_refused(string input)
	{
		Assert.False(SqlStandardParser.TryParseLiteral(input).IsSuccess, input);
	}

	// ── §5.4 Names and identifiers ───────────────────────────────────────────────

	[Theory]
	[InlineData("a")]
	[InlineData("a1")]
	[InlineData("é")]
	[InlineData("a·b")]
	[InlineData("\"select\"")]
	[InlineData("\"a\"\"b\"")]
	[InlineData("absolute")]
	[InlineData("absolutely")]
	[InlineData("selectx")]
	[InlineData("x_select")]
	[InlineData("U&\"a\\0041\"")]
	[InlineData("U&\"\\+000041\"")]
	[InlineData("u&\"a\"")]
	[InlineData("U&\"a\"UESCAPE'\\'")]
	public void An_identifier_reads(string input)
	{
		Assert.True(SqlStandardParser.TryParseIdentifier(input).IsSuccess, input);
	}

	/// <summary>A reserved word in any case, an empty delimited identifier, and what no identifier begins or holds.</summary>
	[Theory]
	[InlineData("select")]
	[InlineData("SeLeCt")]
	[InlineData("abs")]
	[InlineData("absent")]
	[InlineData("END-EXEC")]
	[InlineData("\"\"")]
	[InlineData("_x")]
	[InlineData("1a")]
	[InlineData("a$")]
	[InlineData("a.b")]
	public void What_is_not_an_identifier_is_refused(string input)
	{
		Assert.False(SqlStandardParser.TryParseIdentifier(input).IsSuccess, input);
	}

	[Theory]
	[InlineData("a.b.c", true)]
	[InlineData("a . b", true)]
	[InlineData("a./* c */b", true)]
	[InlineData("\"a\".b", true)]
	[InlineData("a.b.c.d", true)]
	[InlineData("a.select", false)]
	[InlineData("a.", false)]
	[InlineData(".a", false)]
	[InlineData("MODULE.a.b", false)]
	public void An_identifier_chain(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseIdentifierChain(input).IsSuccess);
	}

	/// <summary>
	/// A column reference is a chain, or a module's own: `MODULE.a.b`, where `MODULE` is reserved
	/// and no chain begins.
	/// </summary>
	[Theory]
	[InlineData("a.b.c", true)]
	[InlineData("MODULE.a.b", true)]
	[InlineData("MODULE.a", false)]
	public void A_column_reference(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseColumnReference(input).IsSuccess);
	}

	/// <summary>
	/// A table's name has a schema's before it and a catalog's before that, and no more; the
	/// qualifier is not taken so greedily that no name is left for the end.
	/// </summary>
	[Theory]
	[InlineData("a", true)]
	[InlineData("a.b", true)]
	[InlineData("a.b.c", true)]
	[InlineData("MODULE.a", true)]
	[InlineData("a.b.c.d", false)]
	[InlineData("MODULE.a.b", false)]
	public void A_table_name(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseTableName(input).IsSuccess);
	}
	// ── §6 Scalar expressions, §8 Predicates ─────────────────────────────────────

	/// <summary>
	/// A value expression, whose towers — numeric, character, datetime, interval and more — meet only in
	/// a primary: `a || b + c` is refused, `x + y AT LOCAL` read and `x - y AT LOCAL` refused.
	/// </summary>
	[Theory]
	[InlineData("a + b * c", true)]
	[InlineData("a || b + c", false)]
	[InlineData("a + b || c", false)]
	[InlineData("x + y AT LOCAL", true)]
	[InlineData("x - y AT LOCAL", false)]
	[InlineData("x + y + z AT LOCAL", true)]
	[InlineData("x AT LOCAL + y - z * 2", true)]
	[InlineData("a DAY * 2", true)]
	[InlineData("2 * a DAY", true)]
	[InlineData("2 / a DAY", false)]
	[InlineData("a DAY * b DAY", false)]
	[InlineData("-a DAY", true)]
	[InlineData("-a AT LOCAL", false)]
	[InlineData("- -a", false)]
	[InlineData("a COLLATE c || b", true)]
	[InlineData("a COLLATE c + 1", false)]
	[InlineData("a COLLATE c", true)]
	[InlineData("TRIM_ARRAY(a, 1) || SUBSTRING(b FROM 1)", false)]
	[InlineData("TRIM_ARRAY(a, 1) || b", true)]
	[InlineData("SUBSTRING(b FROM 1) || c COLLATE d", true)]
	[InlineData("ABS(a DAY)", true)]
	[InlineData("ABS(a || b)", false)]
	[InlineData("(a = 1) + 2", true)]
	[InlineData("a = 1", true)]
	[InlineData("NOT a", true)]
	[InlineData("a IS TRUE", true)]
	[InlineData("(a, b)", true)]
	[InlineData("ROW(a)", true)]
	[InlineData("a MULTISET UNION b MULTISET INTERSECT ALL c", true)]
	[InlineData("a MULTISET UNION b || c", false)]
	[InlineData("a.b.c(1, 2)", true)]
	[InlineData("a[1][2]", true)]
	[InlineData("a[b || c]", false)]
	[InlineData("f(x => 1)", true)]
	[InlineData("s.t::m(1)", true)]
	[InlineData("NEW t(1)", true)]
	[InlineData("CAST(a AS INTEGER ARRAY[3] MULTISET)", true)]
	[InlineData("CASE WHEN a THEN 1 ELSE NULL END", true)]
	[InlineData("CASE a WHEN 1, > 2 THEN 3 END", true)]
	[InlineData(":h INDICATOR :i", true)]
	[InlineData("?", true)]
	[InlineData("CURRENT_DATE - x DAY", true)]
	[InlineData("EXTRACT(YEAR FROM a || b)", false)]
	[InlineData("POSITION(a + 1 IN b)", false)]
	[InlineData("a -> b", true)]
	[InlineData("DEREF(a) -> b(1)", true)]
	[InlineData("a AT TIME ZONE b HOUR", true)]
	[InlineData("(a - b) DAY TO SECOND", true)]
	[InlineData("CASE WHEN a THEN 1 WHEN b THEN 2 END", true)]
	[InlineData("CASE a WHEN 1 THEN 2 WHEN 3 THEN 4 ELSE 5 END", true)]
	[InlineData("a [1] [2]", true)]
	[InlineData("(a) .b", true)]
	[InlineData("a.b(1) .c(2)", true)]
	[InlineData("TRIM_ARRAY(a, 1) [1] .b", true)]
	[InlineData("CAST(NULL AS INTEGER)", true)]
	[InlineData("CAST(ARRAY[] AS INTEGER ARRAY)", true)]
	[InlineData("a + b * c - d / e", true)]
	[InlineData("a * b DAY / c", true)]
	[InlineData("a DAY * b / c * d", true)]
	[InlineData("2 * 3 * a DAY", true)]
	[InlineData("2 / 3 * a DAY", true)]
	[InlineData("a + b DAY - c", true)]
	[InlineData("a DAY + b", true)]
	[InlineData("d AT LOCAL - x DAY + y DAY", true)]
	[InlineData("x DAY + d AT LOCAL + y DAY", true)]
	[InlineData("x DAY - d AT LOCAL", false)]
	[InlineData("SUBSTRING(a FROM 1 FOR 2 USING OCTETS)", true)]
	[InlineData("SUBSTRING(a SIMILAR b ESCAPE c)", true)]
	[InlineData("TRIM(LEADING 'x' FROM a)", true)]
	[InlineData("TRIM(FROM a)", true)]
	[InlineData("TRIM(a)", true)]
	[InlineData("TRIM(LEADING a)", false)]
	[InlineData("BTRIM(a, 'x')", true)]
	[InlineData("OVERLAY(a PLACING b FROM 1 FOR 2)", true)]
	[InlineData("POSITION(a IN b USING CHARACTERS)", true)]
	[InlineData("CHAR_LENGTH(a USING OCTETS)", true)]
	[InlineData("OCTET_LENGTH(a)", true)]
	[InlineData("EXTRACT(TIMEZONE_HOUR FROM a)", true)]
	[InlineData("MOD(a, b) + LN(c)", true)]
	[InlineData("UPPER(a) || LOWER(b)", true)]
	[InlineData("CURRENT_TIMESTAMP(3) + x HOUR", true)]
	[InlineData("LOCALTIME", true)]
	[InlineData("CARDINALITY(a)", true)]
	[InlineData("WIDTH_BUCKET(a, 1, 2, 3)", true)]
	[InlineData("NORMALIZE(a, NFC, 10 CHARACTERS)", true)]
	[InlineData("LPAD(a, 3, ' ')", true)]
	[InlineData("TRANSLATE_REGEX('a' IN b WITH 'c' OCCURRENCE ALL)", true)]
	[InlineData("POSITION_REGEX(START 'a' IN b OCCURRENCE 2)", true)]
	[InlineData("a.b.SPECIFICTYPE", true)]
	[InlineData("a.SPECIFICTYPE() || 'x'", true)]
	[InlineData("COALESCE(a, b, c)", true)]
	[InlineData("NULLIF(a, b)", true)]
	[InlineData("GREATEST(a, b)", true)]
	[InlineData("NEXT VALUE FOR s", true)]
	[InlineData("TREAT(a AS REF(t))", true)]
	[InlineData("ELEMENT(a)", true)]
	[InlineData("SET(a) MULTISET EXCEPT DISTINCT b", true)]
	[InlineData("MULTISET[1, 2]", true)]
	[InlineData("COLLATION FOR (a)", true)]
	[InlineData("CURRENT_TRANSFORM_GROUP_FOR_TYPE t", true)]
	[InlineData("f(a AS t, NULL, DEFAULT, x => ARRAY[])", true)]
	[InlineData("(a AS t).m(1)", true)]
	[InlineData("a.b.c.d(1)", true)]
	[InlineData("a.b.c.d", true)]
	[InlineData("MODULE.a.b", true)]
	[InlineData("+a", true)]
	[InlineData("-a || b", false)]
	[InlineData("a IS NOT TRUE AND b", true)]
	[InlineData("NOT (a + 1)", false)]
	[InlineData("(a + 1) IS TRUE", false)]
	[InlineData("((a)) IS TRUE", true)]
	[InlineData("-1 IS TRUE", false)]
	[InlineData("a = 1 OR b", true)]
	[InlineData("a + 1 AND b", false)]
	public void A_value_expression(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseValueExpression(input).IsSuccess);
	}

	/// <summary>A search condition: `x IN (a + 1)` is refused, since an in value list holds row value expressions.</summary>
	[Theory]
	[InlineData("x IN (a + 1)", false)]
	[InlineData("x IN (1, 2)", true)]
	[InlineData("x IN (a)", true)]
	[InlineData("x LIKE a + b", false)]
	[InlineData("x LIKE 'a' ESCAPE '\\'", true)]
	[InlineData("x BETWEEN 1 AND 2 AND y", true)]
	[InlineData("a = b IS TRUE", true)]
	[InlineData("a = NOT b", false)]
	[InlineData("NOT a = b OR c", true)]
	[InlineData("(a, b) = (1, 2)", true)]
	[InlineData("a IS NOT DISTINCT FROM b", true)]
	[InlineData("a IS NFC NORMALIZED", true)]
	[InlineData("a OVERLAPS b", true)]
	[InlineData("PERIOD(a, b) CONTAINS c", true)]
	[InlineData("a SIMILAR TO 'x'", true)]
	[InlineData("a LIKE_REGEX 'x' FLAG 'i'", true)]
	[InlineData("a IS NOT A SET", true)]
	[InlineData("a MEMBER OF b", true)]
	[InlineData("a IS OF (ONLY t)", true)]
	[InlineData("a IS JSON OBJECT WITH UNIQUE KEYS", true)]
	[InlineData("(a) IS UNKNOWN", true)]
	[InlineData("a = 1 = 2", false)]
	[InlineData("a < > b", false)]
	[InlineData("a <> b", true)]
	public void A_search_condition(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseSearchCondition(input).IsSuccess);
	}

	// ── §7 Query expressions ─────────────────────────────────────────────────────

	/// <summary>
	/// A query expression, with the joins, subqueries and clauses of §7 and the predicates that hold a
	/// query. A derived table needs a correlation name; a partitioned join table stands before a
	/// qualified or a natural join and not a cross join; a query in brackets has no `WITH`.
	/// </summary>
	[Theory]
	[InlineData("SELECT a FROM t", true)]
	[InlineData("SELECT * FROM t", true)]
	[InlineData("SELECT a, b AS c, d e FROM s.t AS x WHERE a = 1 GROUP BY a HAVING b > 2", true)]
	[InlineData("SELECT DISTINCT t.* FROM t", true)]
	[InlineData("SELECT a FROM t UNION SELECT b FROM u ORDER BY 1", true)]
	[InlineData("(SELECT a FROM t) EXCEPT ALL (SELECT a FROM u) INTERSECT SELECT a FROM v", true)]
	[InlineData("VALUES (1, 2), (3, 4)", true)]
	[InlineData("TABLE t", true)]
	[InlineData("WITH x (a) AS (SELECT 1 FROM t) SELECT a FROM x", true)]
	[InlineData("SELECT a FROM t JOIN u ON t.a = u.a LEFT OUTER JOIN v USING (a)", true)]
	[InlineData("SELECT a FROM t CROSS JOIN u NATURAL JOIN v", true)]
	[InlineData("SELECT a FROM (SELECT a FROM t) AS s", true)]
	[InlineData("SELECT a FROM t OFFSET 1 ROWS FETCH FIRST 2 ROWS ONLY", true)]
	[InlineData("SELECT a FROM t WHERE EXISTS (SELECT 1 FROM u)", true)]
	[InlineData("SELECT a FROM t WHERE a IN (SELECT a FROM u)", true)]
	[InlineData("SELECT a FROM t WHERE a > ALL (SELECT a FROM u)", true)]
	[InlineData("SELECT (SELECT 1 FROM u) FROM t", true)]
	[InlineData("SELECT a FROM t, u", true)]
	[InlineData("SELECT a FROM (t JOIN u ON a = b)", true)]
	[InlineData("SELECT a FROM t GROUP BY ROLLUP (a, b), ()", true)]
	[InlineData("SELECT a FROM t WINDOW w AS (PARTITION BY a ORDER BY b ROWS UNBOUNDED PRECEDING)", true)]
	[InlineData("SELECT a", false)]
	[InlineData("SELECT a FROM t ORDER BY a DESC NULLS LAST", true)]
	[InlineData("SELECT a FROM (SELECT a FROM t)", false)]
	[InlineData("SELECT a FROM (t)", false)]
	[InlineData("SELECT a FROM ((t JOIN u ON a = b))", true)]
	[InlineData("SELECT a FROM ((t JOIN u ON a = b) JOIN v ON c = d)", true)]
	[InlineData("SELECT a FROM ((t JOIN u ON a = b) TABLESAMPLE SYSTEM (5))", false)]
	[InlineData("SELECT a FROM ((SELECT a FROM t) s JOIN u ON x = y)", true)]
	[InlineData("SELECT a FROM t JOIN u JOIN v ON a = b ON c = d", true)]
	[InlineData("SELECT a FROM t JOIN u JOIN v ON a = b", false)]
	[InlineData("SELECT a FROM t JOIN u CROSS JOIN v ON a = b", true)]
	[InlineData("SELECT a FROM t JOIN u NATURAL JOIN v ON a = b", true)]
	[InlineData("SELECT a FROM t NATURAL JOIN u JOIN v ON a = b", true)]
	[InlineData("SELECT a FROM t PARTITION BY (a) JOIN u ON x = y", true)]
	[InlineData("SELECT a FROM t PARTITION BY (a) CROSS JOIN u", false)]
	[InlineData("SELECT a FROM t JOIN u PARTITION BY (a) ON x = y", true)]
	[InlineData("SELECT a FROM t JOIN u PARTITION BY (a) JOIN v ON x = y ON c = d", true)]
	[InlineData("SELECT a FROM t NATURAL FULL JOIN u PARTITION BY (a, b)", true)]
	[InlineData("SELECT a FROM t PARTITION BY (a)", false)]
	[InlineData("SELECT a FROM t CROSS JOIN u PARTITION BY (a) JOIN v ON x = y", false)]
	[InlineData("SELECT a FROM t TABLESAMPLE BERNOULLI (10) REPEATABLE (1) AS x", false)]
	[InlineData("SELECT a FROM t FOR SYSTEM_TIME AS OF CURRENT_TIMESTAMP x", true)]
	[InlineData("SELECT a FROM t FOR SYSTEM_TIME BETWEEN SYMMETRIC a AND b", true)]
	[InlineData("SELECT a FROM t FOR SYSTEM_TIME FROM a TO b AS x (c, d)", true)]
	[InlineData("SELECT a FROM LATERAL (SELECT a FROM t) AS x", true)]
	[InlineData("SELECT a FROM UNNEST (a, b) WITH ORDINALITY AS x", true)]
	[InlineData("SELECT a FROM UNNEST (a)", false)]
	[InlineData("SELECT a FROM TABLE (f(x)) x", true)]
	[InlineData("SELECT a FROM TABLE (f(x))", true)]
	[InlineData("SELECT a FROM TABLE (a || b)", false)]
	[InlineData("SELECT a FROM ONLY (t) x", true)]
	[InlineData("SELECT a FROM s.t.u.v", false)]
	[InlineData("SELECT a FROM MODULE.t", true)]
	[InlineData("SELECT t.*, u.* AS (x, y), (a).*, f(a).* FROM t", true)]
	[InlineData("SELECT (a, b).* FROM t", false)]
	[InlineData("SELECT a || b[1].* FROM t", true)]
	[InlineData("SELECT *, a FROM t", false)]
	[InlineData("SELECT ALL a b FROM t", true)]
	[InlineData("SELECT a AS FROM t", false)]
	[InlineData("SELECT a FROM t GROUP BY DISTINCT a COLLATE c, (b, c), CUBE ((a), b)", true)]
	[InlineData("SELECT a FROM t GROUP BY GROUPING SETS (ROLLUP (a), GROUPING SETS (()))", true)]
	[InlineData("SELECT a FROM t GROUP BY ROLLUP (())", false)]
	[InlineData("SELECT a FROM t GROUP BY a + 1", false)]
	[InlineData("SELECT a FROM t WINDOW w AS (w2), v AS ()", true)]
	[InlineData("SELECT a FROM t WINDOW w AS (RANGE BETWEEN 1 PRECEDING AND UNBOUNDED FOLLOWING EXCLUDE NO OTHERS)", true)]
	[InlineData("SELECT a FROM t WINDOW w AS (GROUPS BETWEEN CURRENT ROW AND :x FOLLOWING EXCLUDE TIES)", true)]
	[InlineData("SELECT a FROM t WINDOW w AS (ROWS UNBOUNDED FOLLOWING)", false)]
	[InlineData("SELECT a FROM t WINDOW w AS (ROWS p PRECEDING)", true)]
	[InlineData("SELECT a FROM t HAVING a WINDOW w AS (ORDER BY a)", true)]
	[InlineData("SELECT a FROM t WHERE a GROUP BY a", true)]
	[InlineData("SELECT a FROM t ORDER BY a ASC, b NULLS FIRST", true)]
	[InlineData("SELECT a FROM t OFFSET :x ROW", true)]
	[InlineData("SELECT a FROM t FETCH NEXT ROW WITH TIES", true)]
	[InlineData("SELECT a FROM t FETCH FIRST 10 PERCENT ROWS ONLY", true)]
	[InlineData("SELECT a FROM t FETCH FIRST -1 ROWS ONLY", true)]
	[InlineData("SELECT a FROM t OFFSET a.b ROWS", true)]
	[InlineData("SELECT a FROM t OFFSET 1 + 1 ROWS", false)]
	[InlineData("SELECT a FROM t FETCH FIRST ROWS ONLY OFFSET 1 ROWS", false)]
	[InlineData("(SELECT a FROM t ORDER BY a OFFSET 1 ROWS) UNION CORRESPONDING BY (a) SELECT a FROM u", true)]
	[InlineData("(WITH x AS (SELECT a FROM t) SELECT a FROM x) UNION SELECT a FROM u", false)]
	[InlineData("((SELECT a FROM t))", true)]
	[InlineData("(((SELECT a FROM t) UNION (SELECT b FROM u)))", true)]
	[InlineData("SELECT a FROM t INTERSECT DISTINCT SELECT a FROM u UNION ALL TABLE v EXCEPT VALUES 1", true)]
	[InlineData("VALUES 1, a = b, (1, 2), ROW (1)", true)]
	[InlineData("VALUES (SELECT a FROM t)", true)]
	[InlineData("VALUES DEFAULT", false)]
	[InlineData("TABLE s.t ORDER BY a", true)]
	[InlineData("WITH RECURSIVE x (a, b) AS (SELECT a FROM t) SEARCH DEPTH FIRST BY a SET s CYCLE a, b SET c TO 1 DEFAULT 0 USING p SELECT a FROM x", true)]
	[InlineData("WITH x AS (SELECT a FROM t) CYCLE a SET c USING p, y AS (TABLE x) SELECT a FROM y", true)]
	[InlineData("WITH x AS (SELECT a FROM t) CYCLE a SET c USING p SEARCH BREADTH FIRST BY a SET s SELECT a FROM x", false)]
	[InlineData("WITH x AS SELECT a FROM t SELECT a FROM x", false)]
	[InlineData("SELECT a FROM t WHERE UNIQUE NULLS NOT DISTINCT (SELECT a FROM u)", true)]
	[InlineData("SELECT a FROM t WHERE (a, b) MATCH UNIQUE PARTIAL (SELECT a, b FROM u)", true)]
	[InlineData("SELECT a FROM t WHERE a NOT IN ((SELECT a FROM u))", true)]
	[InlineData("SELECT a FROM t WHERE a IN (1, ((SELECT a FROM u)))", true)]
	[InlineData("SELECT a FROM t WHERE a IN ((SELECT a FROM u) + 1)", false)]
	[InlineData("SELECT a FROM t WHERE a = SOME (VALUES 1)", true)]
	[InlineData("SELECT a FROM t WHERE ((SELECT a FROM u)) IS TRUE", true)]
	[InlineData("SELECT a FROM t WHERE (SELECT a FROM u) IS JSON", true)]
	[InlineData("SELECT a FROM t WHERE EXISTS (SELECT a FROM u) AND NOT EXISTS (TABLE v)", true)]
	[InlineData("SELECT a FROM t WHERE (SELECT a, b FROM u) = (1, 2)", true)]
	[InlineData("SELECT ARRAY (SELECT a FROM t), MULTISET (SELECT a FROM t), TABLE (SELECT a FROM t) FROM t", true)]
	[InlineData("SELECT (SELECT a FROM t)[1] FROM t", true)]
	[InlineData("SELECT CASE a WHEN = ANY (SELECT a FROM t) THEN 1 WHEN MATCH (TABLE u) THEN 2 END FROM t", true)]
	[InlineData("SELECT ((SELECT a FROM t) + 1) * 2 FROM t", true)]
	[InlineData("SELECT a FROM t WHERE x = (((((SELECT a FROM t)))))", true)]
	[InlineData("SELECT a FROM (((((SELECT a FROM t) s JOIN u ON a = b) JOIN v ON c = d) JOIN w ON e = f))", true)]
	[InlineData("SELECT (SELECT (SELECT (SELECT (SELECT (SELECT (SELECT (SELECT a FROM t) FROM t) FROM t) FROM t) FROM t) FROM t) FROM t) FROM t", true)]
	[InlineData("SELECT a FROM (SELECT a FROM (SELECT a FROM (SELECT a FROM (SELECT a FROM (SELECT a FROM (SELECT a FROM t) s) s) s) s) s) s", true)]
	[InlineData("SELECT a FROM t JOIN u USING (a) AS j", true)]
	[InlineData("SELECT a FROM t INNER JOIN u ON a", true)]
	[InlineData("SELECT a FROM t LEFT JOIN u ON a = b RIGHT OUTER JOIN v ON c", true)]
	[InlineData("SELECT a FROM t a, LATERAL (SELECT a FROM u WHERE u.a = t.a) b", true)]
	[InlineData("SELECT DISTINCT FROM t", false)]
	[InlineData("SELECT a, FROM t", false)]
	[InlineData("SELECT a FROM t WHERE", false)]
	[InlineData("SELECT a FROM t ORDER BY", false)]
	[InlineData("SELECT a FROM t UNION", false)]
	[InlineData("SELECT a FROM t t2 t3", false)]
	public void A_query_expression(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseQueryExpression(input).IsSuccess);
	}

	// ── §6.10 Window functions, §10.9 Aggregates ─────────────────────────────────

	/// <summary>An aggregate, a window function, a nested window function or a row pattern navigation, as a value.</summary>
	[Theory]
	[InlineData("COUNT(*)", true)]
	[InlineData("COUNT(DISTINCT a)", true)]
	[InlineData("SUM(a) FILTER (WHERE a > 1)", true)]
	[InlineData("AVG(ALL a + 1)", true)]
	[InlineData("ANY(a)", true)]
	[InlineData("a = ANY (1)", true)]
	[InlineData("EVERY(a) AND b", true)]
	[InlineData("COVAR_POP(a, b)", true)]
	[InlineData("RANK(1, 2) WITHIN GROUP (ORDER BY a)", true)]
	[InlineData("PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY a DESC)", true)]
	[InlineData("LISTAGG(DISTINCT a, ', ' ON OVERFLOW TRUNCATE '...' WITH COUNT) WITHIN GROUP (ORDER BY a)", true)]
	[InlineData("ARRAY_AGG(a ORDER BY b)", true)]
	[InlineData("COUNT(v.*)", true)]
	[InlineData("GROUPING(a, b.c)", true)]
	[InlineData("RUNNING SUM(a)", true)]
	[InlineData("FINAL COUNT(*)", true)]
	[InlineData("ROW_NUMBER() OVER w", true)]
	[InlineData("RANK() OVER (PARTITION BY a ORDER BY b)", true)]
	[InlineData("SUM(a) OVER (w ROWS UNBOUNDED PRECEDING)", true)]
	[InlineData("NTILE(4) OVER w", true)]
	[InlineData("LEAD(a, 1, 0) IGNORE NULLS OVER w", true)]
	[InlineData("FIRST_VALUE(a) RESPECT NULLS OVER w", true)]
	[InlineData("NTH_VALUE(a, 2) FROM LAST OVER w", true)]
	[InlineData("m OVER w", true)]
	[InlineData("ROW_NUMBER(CURRENT_ROW)", true)]
	[InlineData("VALUE_OF(a AT BEGIN_PARTITION + 1, 0)", true)]
	[InlineData("SUM(a) + 1", true)]
	[InlineData("COUNT(*) FILTER (WHERE a) OVER w", true)]
	[InlineData("SUM(a) || 'x'", true)]
	[InlineData("PREV(a)", true)]
	[InlineData("FIRST(a.b, 1)", true)]
	[InlineData("PREV(FIRST(a, 1), 2)", true)]
	[InlineData("MATCH_NUMBER()", true)]
	[InlineData("CLASSIFIER(a)", true)]
	[InlineData("ROW_NUMBER()", false)]
	[InlineData("RANK(1)", false)]
	[InlineData("SUM(a)[1]", true)]
	[InlineData("COUNT(DISTINCT *)", false)]
	public void An_aggregate_or_a_window_function(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseValueExpression(input).IsSuccess);
	}

	/// <summary>
	/// The same in a query. `ANY (SELECT …)` is a quantifier's and nothing more, since an aggregate's
	/// brackets hold a value expression; `ANY ((SELECT …))` is an aggregate's too, and may go on.
	/// </summary>
	[Theory]
	[InlineData("SELECT COUNT(*), SUM(a) OVER w FROM t GROUP BY a HAVING COUNT(*) > 1 WINDOW w AS (ORDER BY a)", true)]
	[InlineData("SELECT a FROM t WHERE a = ANY (SELECT a FROM t) + 1", false)]
	[InlineData("SELECT a FROM t WHERE a = ALL (SELECT a FROM t) + 1", false)]
	[InlineData("SELECT a FROM t WHERE a = SOME (SELECT a FROM t) FILTER (WHERE b)", false)]
	[InlineData("SELECT m OVER (PARTITION BY a), m.n OVER w FROM t", false)]
	[InlineData("SELECT RANK(1) WITHIN GROUP (ORDER BY a) OVER w FROM t", true)]
	[InlineData("SELECT RANK(1) WITHIN GROUP (ORDER BY a) FILTER (WHERE b) FROM t", true)]
	[InlineData("SELECT RUNNING SUM(a) OVER w FROM t", false)]
	[InlineData("SELECT FINAL FIRST(a) FROM t", true)]
	[InlineData("SELECT PREV(RUNNING FIRST(a, 1), 2) FROM t", true)]
	[InlineData("SELECT PREV(FIRST(a) + 1) FROM t", true)]
	[InlineData("SELECT PREV(FIRST(a, b + 1)) FROM t", true)]
	[InlineData("SELECT FIRST(a, b + 1) FROM t", true)]
	[InlineData("SELECT RUNNING FIRST(a, b + 1) FROM t", false)]
	[InlineData("SELECT NEXT(FINAL LAST(a), ?) FROM t", true)]
	[InlineData("SELECT NEXT VALUE FOR s FROM t", true)]
	[InlineData("SELECT COUNT(ALL *) FROM t", false)]
	[InlineData("SELECT COUNT(a.*) OVER w FROM t", true)]
	[InlineData("SELECT ARRAY_AGG(a ORDER BY b DESC, c) FILTER (WHERE d) FROM t", true)]
	[InlineData("SELECT LISTAGG(a, ',') WITHIN GROUP (ORDER BY a) FROM t", true)]
	[InlineData("SELECT LISTAGG(a, b) WITHIN GROUP (ORDER BY a) FROM t", false)]
	[InlineData("SELECT LISTAGG(a, ',' ON OVERFLOW ERROR) FROM t", false)]
	[InlineData("SELECT PERCENTILE_DISC(a + 1) WITHIN GROUP (ORDER BY a) FROM t", true)]
	[InlineData("SELECT PERCENTILE_DISC('x') WITHIN GROUP (ORDER BY a) FROM t", true)]
	[InlineData("SELECT REGR_COUNT(a, 'x') FROM t", true)]
	[InlineData("SELECT LEAD(a) OVER w, LAG(a, 2) RESPECT NULLS OVER (ORDER BY b) FROM t", true)]
	[InlineData("SELECT LEAD(a, 1 + 1) OVER w FROM t", false)]
	[InlineData("SELECT LEAD(a, -1) OVER w FROM t", false)]
	[InlineData("SELECT NTILE(:n) OVER w, NTILE(?) OVER w FROM t", true)]
	[InlineData("SELECT NTILE(a + 1) OVER w FROM t", false)]
	[InlineData("SELECT NTH_VALUE(a, 1) FROM FIRST IGNORE NULLS OVER w FROM t", true)]
	[InlineData("SELECT FIRST_VALUE(a) OVER w FROM t", true)]
	[InlineData("SELECT FIRST_VALUE(a) FROM t", false)]
	[InlineData("SELECT ROW_NUMBER() OVER () FROM t", true)]
	[InlineData("SELECT VALUE_OF(a AT CURRENT_ROW - :k) FROM t", true)]
	[InlineData("SELECT VALUE_OF(a + 1 AT END_FRAME, 0) FROM t", true)]
	[InlineData("SELECT GROUPING(a) FROM t GROUP BY ROLLUP (a)", true)]
	[InlineData("SELECT GROUPING(a + 1) FROM t", false)]
	[InlineData("SELECT SUM(a) OVER w.x FROM t", true)]
	[InlineData("SELECT (SUM(a)) OVER w FROM t", false)]
	[InlineData("SELECT SUM(a) OVER w OVER v FROM t", false)]
	[InlineData("SELECT EVERY(a = 1) IS TRUE FROM t", true)]
	[InlineData("SELECT ANY_VALUE(DISTINCT a) FROM t", true)]
	[InlineData("SELECT CUME_DIST() OVER w * 100 FROM t", true)]
	[InlineData("SELECT INTERSECTION(a) MULTISET UNION FUSION(b) FROM t", true)]
	[InlineData("SELECT COLLECT(a)[1] FROM t", true)]
	[InlineData("SELECT a FROM t WINDOW w AS (ORDER BY SUM(a))", true)]
	[InlineData("SELECT a FROM t ORDER BY COUNT(*) DESC", true)]
	[InlineData("SELECT CASE WHEN COUNT(*) > 1 THEN MAX(a) END FROM t", true)]
	[InlineData("SELECT a FROM t WHERE a = ANY ((SELECT a FROM t)) + 1", true)]
	[InlineData("SELECT a FROM t WHERE a = SOME ((SELECT a FROM t)) FILTER (WHERE b)", true)]
	[InlineData("SELECT a FROM t WHERE a = ALL ((SELECT a FROM t)) + 1", false)]
	[InlineData("SELECT a FROM t WHERE a = ANY ((SELECT a FROM t))", true)]
	public void An_aggregate_in_a_query(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseQueryExpression(input).IsSuccess);
	}

	// ── §6.1 Data types ──────────────────────────────────────────────────────────

	[Theory]
	[InlineData("INTEGER", true)]
	[InlineData("INTEGER ARRAY", true)]
	[InlineData("INTEGER ARRAY[3]", true)]
	[InlineData("INTEGER MULTISET", true)]
	[InlineData("INTEGER ARRAY MULTISET", true)]
	[InlineData("INTEGER ARRAY[3] MULTISET", true)]
	[InlineData("CHARACTER VARYING(10)", true)]
	[InlineData("CHAR(10 CHARACTERS) CHARACTER SET latin1 COLLATE c", true)]
	[InlineData("NATIONAL CHAR LARGE OBJECT", false)]
	[InlineData("NCHAR LARGE OBJECT(2K)", true)]
	[InlineData("BLOB(2 M)", true)]
	[InlineData("BLOB(2Mx)", false)]
	[InlineData("DECIMAL(10, 2)", true)]
	[InlineData("DOUBLE PRECISION", true)]
	[InlineData("TIME(3) WITH TIME ZONE", true)]
	[InlineData("TIMESTAMP WITHOUT TIME ZONE", true)]
	[InlineData("INTERVAL DAY TO SECOND(3)", true)]
	[InlineData("ROW(a INTEGER, b ROW(c DATE))", true)]
	[InlineData("REF(t) SCOPE s.u", true)]
	[InlineData("s.t", true)]
	[InlineData("BOOLEAN ARRAY", true)]
	[InlineData("JSON", true)]
	[InlineData("INTEGER ARRAY[3] ARRAY", true)]
	[InlineData("INTEGER ARRAY[3]MULTISET", true)]
	[InlineData("INTEGER ARRAY ??(3??) MULTISET", true)]
	[InlineData("INTEGER ARRAY[3] ARRAY[4]", true)]
	[InlineData("INTEGER MULTISET ARRAY[3]", true)]
	[InlineData("INTEGER MULTISET MULTISET", true)]
	[InlineData("INTEGER ARRAY[3]  MULTISET", true)]
	public void A_data_type(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseDataType(input).IsSuccess);
	}
}
