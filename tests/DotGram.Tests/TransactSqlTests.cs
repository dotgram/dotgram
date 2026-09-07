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
	[InlineData("SELECT a FROM t NATURAL FULL JOIN u")]
	[InlineData("SELECT a FROM t CROSS JOIN u")]
	[InlineData("SELECT a FROM t INNER JOIN u USING (id, k)")]
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
	[InlineData("SELECT * FROM t1 CROSS APPLY (SELECT * FROM u)")]
	[InlineData("SELECT * FROM t1 FOR JSON AUTO")]
	[InlineData("SELECT a FROM t OPTION (RECOMPILE)")]
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
