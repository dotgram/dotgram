using System;

using DotGram.Parsers;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The SQL-92 expression parser, read.
/// </summary>
/// <remarks>
/// <para>
/// It had none, which came to light the moment it was worth knowing: the grammar was about
/// to be switched to the lexical split (<c>Lexical = true</c>) and "the suite is green" said
/// only that it compiled. A parser that nothing reads with is a parser nothing knows about.
/// </para>
/// <para>
/// Verdicts and not values — the grammar builds none, and what it is for is answering
/// whether a string is a <c>&lt;search condition&gt;</c>. The inputs are the corpus the
/// split was measured against, so the two say the same thing about the same strings and
/// this one says it where CI can hear.
/// </para>
/// </remarks>
public sealed class SqlStandard92Tests
{
	[Theory]
	[InlineData("a = 1")]
	[InlineData("salary BETWEEN 1000 AND 2000")]
	[InlineData("name LIKE 'A%' ESCAPE '\\'")]
	[InlineData("x IN (1, 2, 3) AND y IS NOT NULL")]
	[InlineData("(a + b) * c - d / e > f AND NOT g < h")]
	[InlineData("CAST(x AS INTEGER) = 5 OR SUBSTRING(s FROM 1 FOR 3) = 'abc'")]
	[InlineData("amount * 1.05 + tax >= total AND status <> 'CLOSED' AND created IS NOT NULL")]
	[InlineData("warehouse.zip_code = 'X' AND vendor_key IS NOT NULL AND quota > 0")]
	[InlineData("((((a + 1) * 2) - 3) / 4) + b > 0")]
	[InlineData("EXTRACT(YEAR FROM created) = 2020")]
	[InlineData("COALESCE(a, b, c) IS NOT NULL AND NULLIF(d, 0) > 1")]
	[InlineData("AVG(x) > 1 AND COUNT(*) < 100 AND SUM(DISTINCT y) = 0")]
	[InlineData("CAST(x AS NUMERIC(10, 2)) > 0")]
	[InlineData("CAST(x AS VARCHAR(20)) = 'a'")]
	public void A_search_condition_reads(string input)
	{
		Assert.True(SqlStandard92.TryParseSearchCondition(input).IsSuccess, input);
	}

	// ── §7, the query level ──────────────────────────────────────────────────────

	/// <summary>A query reads, clauses and all.</summary>
	/// <remarks>
	/// The level the grammar used to hold a place for: a subquery was a balanced run of
	/// characters kept as text, and is a query now. Every join the standard writes, the
	/// three set operators, a derived table, a scalar subquery in the select list, and the
	/// two table constructors that are not a <c>SELECT</c> at all.
	/// </remarks>
	[Theory]
	[InlineData("SELECT * FROM t")]
	[InlineData("SELECT DISTINCT a, b FROM t WHERE a > 1")]
	[InlineData("SELECT t.* FROM t")]
	[InlineData("SELECT a AS x, b y FROM t")]
	[InlineData("SELECT a FROM t, u")]
	[InlineData("SELECT a FROM t JOIN u ON t.id = u.id")]
	[InlineData("SELECT a FROM t LEFT OUTER JOIN u ON t.id = u.id")]
	[InlineData("SELECT a FROM t NATURAL FULL JOIN u")]
	[InlineData("SELECT a FROM t CROSS JOIN u")]
	[InlineData("SELECT a FROM t INNER JOIN u USING (id, k)")]
	[InlineData("SELECT a FROM (SELECT b FROM u) AS d (c)")]
	[InlineData("SELECT a FROM t WHERE a IN (SELECT b FROM u)")]
	[InlineData("SELECT a FROM t WHERE EXISTS (SELECT * FROM u WHERE u.id = t.id)")]
	[InlineData("SELECT COUNT(*) FROM t GROUP BY a, b HAVING COUNT(*) > 1")]
	[InlineData("SELECT (SELECT MAX(b) FROM u) FROM t")]
	[InlineData("SELECT a FROM t UNION ALL SELECT b FROM u EXCEPT SELECT c FROM v")]
	[InlineData("SELECT a FROM t ORDER BY a DESC, 2 ASC")]
	[InlineData("VALUES (1, 2), (3, 4)")]
	[InlineData("TABLE t")]
	[InlineData("(SELECT a FROM t)")]
	// §6.11 <character factor>: a collate clause says how a value is compared rather than
	// what it is, so it is read and dropped. Found by ScriptDom's corpus (`--corpus`).
	[InlineData("SELECT a COLLATE SQL_Latin1_General_CP1_CI_AS FROM t")]
	[InlineData("SELECT a FROM t WHERE b COLLATE X = c")]
	public void A_query_reads(string input) =>
		Assert.True(SqlStandard92.TryParseSelect(input).IsSuccess, input);

	/// <summary>And what it reads, it builds.</summary>
	[Fact]
	public void And_a_query_says_what_it_selects_and_where_from()
	{
		var query = Assert.IsType<SqlNode.Query>(
			SqlStandard92.TryParseSelect("SELECT DISTINCT a, t.* FROM u AS t WHERE a > 1").Value);

		Assert.True(query.Distinct);
		Assert.Equal(2, query.Columns.Length);
		Assert.Equal("t", Assert.IsType<SqlNode.Star>(query.Columns[1]).Qualifier);

		var source = Assert.IsType<SqlNode.Source>(Assert.Single(query.From));

		Assert.Equal("u", source.Table);
		Assert.Equal("t", source.Name);
		Assert.NotNull(query.Where);
		Assert.Empty(query.GroupBy);
		Assert.Null(query.Having);
	}

	/// <summary>A join says which it is and what it joins on.</summary>
	[Theory]
	[InlineData("SELECT a FROM t JOIN u ON t.id = u.id",        "Inner", false, true,  false)]
	[InlineData("SELECT a FROM t LEFT OUTER JOIN u ON x = y",   "Left",  false, true,  false)]
	[InlineData("SELECT a FROM t RIGHT JOIN u ON x = y",        "Right", false, true,  false)]
	[InlineData("SELECT a FROM t NATURAL FULL JOIN u",          "Full",  true,  false, false)]
	[InlineData("SELECT a FROM t CROSS JOIN u",                 "Cross", false, false, false)]
	[InlineData("SELECT a FROM t JOIN u USING (id)",            "Inner", false, false, true)]
	public void A_join_says_which_it_is(
		string input, string kind, bool natural, bool on, bool over)
	{
		var query = Assert.IsType<SqlNode.Query>(SqlStandard92.TryParseSelect(input).Value);
		var join  = Assert.IsType<SqlNode.Join>(Assert.Single(query.From));

		Assert.Equal(kind, join.Kind.ToString());
		Assert.Equal(natural, join.Natural);
		Assert.Equal(on, join.On is not null);
		Assert.Equal(over, join.Using is not null);
	}

	/// <summary>
	/// And the set operators group as §7.10 says: <c>INTERSECT</c> tighter than the two
	/// beside it, and all three to the left.
	/// </summary>
	[Theory]
	[InlineData("SELECT a FROM t UNION SELECT b FROM u INTERSECT SELECT c FROM v", "Union", "Intersect")]
	[InlineData("SELECT a FROM t INTERSECT SELECT b FROM u UNION SELECT c FROM v", "Union", "Intersect")]
	[InlineData("SELECT a FROM t UNION SELECT b FROM u EXCEPT SELECT c FROM v",    "Except", "Union")]
	public void Set_operators_group_as_the_standard_says(string input, string outer, string inner)
	{
		var read = Assert.IsType<SqlNode.Binary>(SqlStandard92.TryParseQuery(input).Value);

		Assert.Equal(outer, read.Operator.ToString());
		Assert.Contains(
			inner,
			new[] { read.Left, read.Right }
				.OfType<SqlNode.Binary>()
				.Select(static one => one.Operator.ToString()));
	}

	/// <summary>A subquery is a query now, and no longer the text between brackets.</summary>
	[Fact]
	public void A_subquery_is_read_rather_than_kept()
	{
		var query = Assert.IsType<SqlNode.Query>(
			SqlStandard92.TryParseSelect("SELECT a FROM (SELECT b FROM u) AS d (c)").Value);

		var source = Assert.IsType<SqlNode.Source>(Assert.Single(query.From));

		Assert.Null(source.Table);
		Assert.Equal("d", source.Name);
		Assert.Equal(new[] { "c" }, source.Columns);

		var inner = Assert.IsType<SqlNode.Query>(source.Derived);

		Assert.Equal("u", Assert.IsType<SqlNode.Source>(Assert.Single(inner.From)).Table);
	}

	/// <summary>
	/// And it groups the way §6.11 says, which the tower states as three strengths
	/// rather than three rules (§4.3.1).
	/// </summary>
	/// <remarks>
	/// Reading a thing and building the right thing are two questions, and the theory
	/// above asks only the first. What binding powers put at risk is the second: a sign
	/// that took the whole product instead of the operand beside it would still read.
	/// </remarks>
	[Theory]
	[InlineData("a + b * c",   "(a Add (b Multiply c))")]
	[InlineData("a * b + c",   "((a Multiply b) Add c)")]
	[InlineData("a - b - c",   "((a Subtract b) Subtract c)")]
	[InlineData("a / b / c",   "((a Divide b) Divide c)")]
	[InlineData("(a + b) * c", "((a Add b) Multiply c)")]
	[InlineData("-a * b",      "((Negate a) Multiply b)")]
	[InlineData("-a + b",      "((Negate a) Add b)")]
	[InlineData("a * -b",      "(a Multiply (Negate b))")]
	[InlineData("a || b || c", "((a Concatenate b) Concatenate c)")]
	public void And_groups_the_way_the_standard_says(string input, string shape)
	{
		var read = SqlStandard92.TryParseValueExpression(input);

		Assert.True(read.IsSuccess, input);
		Assert.Equal(shape, Shape(read.Value!));
	}

	/// <summary>A value expression as parentheses and operator names, for comparing.</summary>
	static string Shape(SqlNode node) => node switch
	{
		SqlNode.Binary(var op, var left, var right) => $"({Shape(left)} {op} {Shape(right)})",
		SqlNode.Unary (var op, var operand)         => $"({op} {Shape(operand)})",
		SqlNode.Column(var text)                    => text,
		SqlNode.Literal(_, var text)                => text,
		_                                           => node.GetType().Name,
	};

	/// <summary>
	/// A word the standard does not reserve is a name, and a word it reserves is not.
	/// </summary>
	/// <remarks>
	/// §5.2 is a list of what an identifier may <em>not</em> be, and everything outside it is
	/// fair game — <c>zone</c> is a word of SQL-92, from <c>WITH TIME ZONE</c>, and a column
	/// may be called it. This is the case that the split got wrong first: over tokens the
	/// word arrives as its own kind and never reaches the identifier rule unless the kind
	/// says it is one too.
	/// </remarks>
	[Theory]
	[InlineData("zone = 1",       true)]
	[InlineData("year > month",   true)]
	[InlineData("having = 1",     false)]
	[InlineData("select = 1",     false)]
	[InlineData("\"select\" = 1", true)]

	// And one that is reserved and reads anyway, which is not an exception to the rule but
	// a different rule: §6.3 makes `VALUE` a niladic function, so `value = 1` is a value
	// specification compared with one rather than a column called `value`.
	[InlineData("value = 1",      true)]
	public void A_non_reserved_word_may_be_a_name(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandard92.TryParseSearchCondition(input).IsSuccess);
	}

	/// <summary>
	/// A `CASE` with more than one `WHEN`, which is what a woven word boundary used to refuse.
	/// </summary>
	[Theory]
	[InlineData("CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'small' ELSE 'none' END = label")]
	[InlineData("CASE a WHEN 1 THEN 2 WHEN 3 THEN 4 END = label")]
	public void A_case_may_have_more_than_one_when(string input)
	{
		Assert.True(SqlStandard92.TryParseSearchCondition(input).IsSuccess, input);
	}

	/// <summary>What is refused, and where.</summary>
	/// <remarks>
	/// The position is a character of the input whichever machine read it — over tokens it is
	/// mapped back through the extents, and getting that wrong is invisible until something
	/// asks. So it is asked here: the refusal is at the end of what could be read, not at the
	/// beginning of what could not.
	/// </remarks>
	[Theory]
	[InlineData("a = ")]
	[InlineData("BETWEEN 1 AND 2")]
	[InlineData("a AND")]
	[InlineData("(a + b")]
	[InlineData("a = 1 OR")]
	[InlineData("x IN")]
	public void What_is_not_a_search_condition_is_refused(string input)
	{
		var match = SqlStandard92.TryParseSearchCondition(input);

		Assert.False(match.IsSuccess, input);
		Assert.InRange(match.Position, 0, input.Length);
	}

	/// <summary>A refusal is reported where the reading stopped, not where it started.</summary>
	[Fact]
	public void A_refusal_says_how_far_it_got()
	{
		var early = SqlStandard92.TryParseSearchCondition("= 1");
		var late  = SqlStandard92.TryParseSearchCondition("a = 1 AND b = 2 AND c =");

		Assert.False(early.IsSuccess);
		Assert.False(late.IsSuccess);

		Assert.True(late.Position > early.Position, $"{late.Position} should be past {early.Position}");
	}

	/// <summary>And a value expression is its own publication.</summary>
	[Theory]
	[InlineData("a + b * c")]
	[InlineData("CAST(x AS INTEGER)")]
	[InlineData("'a' || 'b'")]
	public void A_value_expression_reads(string input)
	{
		Assert.True(SqlStandard92.TryParseValueExpression(input).IsSuccess, input);
	}
}
