using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using DotGram.Sql.PostgreSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// What grows costs what its size does, never a power of it: a dollar-quoted body twice as long and a comment
/// nested twice as deep each allocate at most a bounded multiple of the one before, and each is read long before a
/// doubling reading could finish.
/// </summary>
/// <remarks>
/// <para>
/// The shapes are PostgreSQL's own. A dollar quote is closed by the tag it opened with, matched again at every
/// <c>$</c> inside the body (docs/syntax.md §3.5.1), so a body of <c>$</c> signs is the costliest; a comment holds
/// others to any depth, where T-SQL's stop at six, and the server's lexer counts them with an integer, so a hundred
/// thousand of them are read here too, through every door. The depths and lengths go up in order and the first one
/// out of bounds ends the test, so a defect fails it rather than hanging it.
/// </para>
/// </remarks>
public sealed class PostgreSqlNestingCostTests
{
	/// <summary>How much more a reading four times as big may allocate: a linear shape's four, with room; a quadratic's sixteen fails.</summary>
	const double Slack = 8;

	/// <summary>How much more a reading twice as deep may allocate: a linear shape's two, with room; a quadratic's four fails.</summary>
	const double DepthSlack = 3;

	/// <summary>How long a reading of any size here may take before it is taken for a blow-up.</summary>
	static readonly TimeSpan Watchdog = TimeSpan.FromSeconds(10);

	[Theory]
	[InlineData("dollar quote of dollars, closed",   "$t$", "$", "$t$",  true)]
	[InlineData("dollar quote of dollars, unclosed", "$t$", "$", "$t",   false)]
	[InlineData("dollar quote of other tags, closed", "$t$", "$x", "$t$", true)]
	[InlineData("dollar quote of text, closed",      "$t$", "a", "$t$",  true)]
	[InlineData("dollar quote of a long tag, near misses in the body", "$abcdefghij$", "$abcdefghi$", "$abcdefghij$", true)]
	[InlineData("comments nested, closed",           "",    "/*", "*/",  true)]
	[InlineData("comments nested, unclosed",         "",    "/*", "",    false)]
	public void A_run_costs_what_its_length_does(string what, string head, string unit, string tail, bool accepted)
	{
		// The shape once at a small size and on this thread, so that the first size measured is not the one that
		// pays for compiling the reader.
		PostgreSqlParser.TryParseExpression(Text(head, unit, tail, 4, accepted));

		var bytes = new List<long>();

		foreach (var size in new[] { 1 << 10, 1 << 12, 1 << 14, 1 << 16 })
		{
			var text = Text(head, unit, tail, size, accepted);
			var (finished, read, allocated) = Read(text, static t => PostgreSqlParser.TryParseExpression(t).IsSuccess);

			Assert.True(
				finished,
				$"{what}: {size} units were not read in {Watchdog.TotalSeconds:F0} s, where the sizes before allocated " +
				$"{string.Join(", ", bytes.Select(static one => one.ToString("N0")))} bytes.");
			Assert.Equal(accepted, read);

			if (bytes.Count > 0)
			{
				Assert.True(
					allocated <= bytes[^1] * Slack,
					$"{what}: {size / 4} units allocated {bytes[^1]:N0} bytes and {size} allocated {allocated:N0}, " +
					$"{(double)allocated / bytes[^1]:F1} times as much. Four times the size may cost a constant or a " +
					"multiple of it, never a power of it.");
			}

			bytes.Add(allocated);
		}
	}

	/// <summary>
	/// A nest costs what its depth does: a call inside a call with a typed literal's string or a bad tail after the
	/// outermost (the call read once, the string after its bracket deciding), a value or a subquery in brackets inside
	/// brackets, a subscript inside a subscript (the first value read once, the colon after it deciding), and a
	/// bracketed join inside a bracketed join — accepted and refused alike.
	/// </summary>
	[Theory]
	[InlineData("calls, then a typed literal",       "SELECT ", "f(",  "a", ")", " 'x'",      true)]
	[InlineData("calls, then a bad tail",            "SELECT ", "f(",  "a", ")", " 'x' 'y'",  false)]
	[InlineData("calls, unclosed",                   "SELECT ", "f(",  "a", "",  "",          false)]
	[InlineData("brackets around a subquery, closed", "SELECT ", "(",   "SELECT 1", ")", "",   true)]
	[InlineData("brackets around a subquery, unclosed", "SELECT ", "(", "SELECT 1", "", "",    false)]
	[InlineData("brackets around a value, closed",    "SELECT ", "(",   "a", ")", "",          true)]
	[InlineData("brackets around a value, unclosed",  "SELECT ", "(",   "a", "",  "",          false)]
	[InlineData("subscripts nested, closed",         "SELECT ", "a[",  "1", "]", "",          true)]
	[InlineData("subscripts nested, unclosed",       "SELECT ", "a[",  "1", "",  "",          false)]
	[InlineData("slices nested, closed",             "SELECT ", "a[1:", "1", "]", "",         true)]
	[InlineData("tests after a bracket, nested",     "SELECT ", "(",   "a", ") IS NULL", "",   true)]
	[InlineData("rollups nested, closed",            "SELECT a FROM t GROUP BY ", "ROLLUP(", "a", ")", "", true)]
	[InlineData("rollups nested, unclosed",          "SELECT a FROM t GROUP BY ", "ROLLUP(", "a", "",  "", false)]
	[InlineData("derived tables nested, closed",     "SELECT * FROM ", "(", "(SELECT 1) x", " JOIN y ON true) z", "", true)]
	[InlineData("derived tables nested, unclosed",   "SELECT * FROM ", "(", "(SELECT 1) x", "", "", false)]
	[InlineData("insert parentheses nested, closed", "INSERT INTO t ", "(", "SELECT 1", ")", "", true)]
	[InlineData("insert parentheses nested, unclosed", "INSERT INTO t ", "(", "SELECT 1", "", "", false)]
	[InlineData("data change bodies nested, closed", "", "WITH x AS (", "INSERT INTO t DEFAULT VALUES", ") INSERT INTO t DEFAULT VALUES", "", true)]
	[InlineData("data change bodies nested, unclosed", "", "WITH x AS (", "INSERT INTO t DEFAULT VALUES", "", "", false)]
	[InlineData("bracketed joins, closed",           "SELECT * FROM ", "(", "a JOIN b ON x", " JOIN c ON y)", "", true)]
	[InlineData("explains nested",                   "", "EXPLAIN ", "SELECT 1", "", "", false)]
	[InlineData("explains with options nested",      "", "EXPLAIN (VERBOSE) ", "SELECT 1", "", "", false)]
	[InlineData("prepares nested",                   "", "PREPARE p AS ", "SELECT 1", "", "", false)]
	[InlineData("bracketed joins, unclosed",         "SELECT * FROM ", "(", "a JOIN b ON x", "", "", false)]
	[InlineData("a table made of bracketed queries, closed", "CREATE TABLE t AS ", "(", "SELECT 1", ")", " WITH DATA", true)]
	[InlineData("a table made of bracketed queries, unclosed", "CREATE TABLE t AS ", "(", "SELECT 1", "", "", false)]
	[InlineData("checks of brackets in a column, closed", "CREATE TABLE t (a int CHECK ", "(", "a > 0", ")", ")", true)]
	public void A_nest_costs_what_its_depth_does(string what, string head, string opener, string core, string closer, string tail, bool accepted)
	{
		PostgreSqlParser.TryParseStatement(Nest(head, opener, core, closer, tail, 2));

		var bytes = new List<long>();

		foreach (var depth in new[] { 8, 16, 32, 64 })
		{
			var text = Nest(head, opener, core, closer, tail, depth);
			var (finished, read, allocated) = Read(text, static t => PostgreSqlParser.TryParseStatement(t).IsSuccess);

			Assert.True(finished, $"{what}: {depth} levels were not read in {Watchdog.TotalSeconds:F0} s, where the depths before allocated {string.Join(", ", bytes.Select(static one => one.ToString("N0")))} bytes.");
			Assert.Equal(accepted, read);

			if (bytes.Count > 0)
				Assert.True(allocated <= bytes[^1] * DepthSlack, $"{what}: {depth / 2} levels allocated {bytes[^1]:N0} bytes and {depth} allocated {allocated:N0}, {(double)allocated / bytes[^1]:F1} times as much.");

			bytes.Add(allocated);
		}
	}

	/// <summary>
	/// A DDL list costs what its length does: a table's columns and a column's constraints, read once from the parenthesis
	/// and each element decided by its first word; a partition's bound values and its keys; a routine's argument types and a
	/// list of routines; operators with their operands' types, each a type with modifiers and brackets — accepted and
	/// refused at the end alike.
	/// </summary>
	[Theory]
	[InlineData("table columns, closed",                  "CREATE TABLE t (", "c int, ", "z int)", true)]
	[InlineData("table columns, unclosed",                "CREATE TABLE t (", "c int, ", "z int", false)]
	[InlineData("table columns, a bad last one",          "CREATE TABLE t (", "c int, ", "z)", false)]
	[InlineData("table constraints, closed",              "CREATE TABLE t (a int", ", CONSTRAINT k UNIQUE (a) DEFERRABLE", ")", true)]
	[InlineData("a column's constraints, closed",         "CREATE TABLE t (a int", " NOT NULL CHECK (a > 0)", ")", true)]
	[InlineData("a column's constraints, a bad last one", "CREATE TABLE t (a int", " NOT NULL CHECK (a > 0)", " CHECK)", false)]
	[InlineData("partition bound values, closed",         "CREATE TABLE p PARTITION OF t FOR VALUES IN (", "1, ", "1)", true)]
	[InlineData("partition bound values, unclosed",       "CREATE TABLE p PARTITION OF t FOR VALUES IN (", "1, ", "1", false)]
	[InlineData("partition keys, closed",                 "CREATE TABLE t (a int) PARTITION BY RANGE (", "lower(a) COLLATE \"C\" text_ops, ", "a)", true)]
	[InlineData("argument types, closed",                 "DROP FUNCTION f(", "numeric(10, 2)[], ", "int)", true)]
	[InlineData("argument types, unclosed",               "DROP FUNCTION f(", "numeric(10, 2)[], ", "int", false)]
	[InlineData("routines dropped, the last one unclosed", "DROP FUNCTION ", "f(int, text), ", "f(int", false)]
	[InlineData("comment argument types, closed",         "COMMENT ON FUNCTION f(", "IN a character varying(3), ", "int) IS 'x'", true)]
	[InlineData("operators dropped, closed",              "DROP OPERATOR ", "+ (character varying(3), int[]), ", "+ (int, int)", true)]
	[InlineData("operators dropped, the last one unclosed", "DROP OPERATOR ", "+ (character varying(3), int[]), ", "+ (int, ", false)]
	[InlineData("tables dropped, closed",                 "DROP TABLE IF EXISTS ", "s.t, ", "u CASCADE", true)]
	public void A_DDL_list_costs_what_its_length_does(string what, string head, string unit, string tail, bool accepted)
	{
		PostgreSqlParser.TryParseStatement(head + unit + tail);

		var bytes = new List<long>();

		foreach (var size in new[] { 64, 256, 1024, 4096 })
		{
			var text = head + string.Concat(Enumerable.Repeat(unit, size)) + tail;
			var (finished, read, allocated) = Read(text, static t => PostgreSqlParser.TryParseStatement(t).IsSuccess);

			Assert.True(finished, $"{what}: {size} items were not read in {Watchdog.TotalSeconds:F0} s, where the sizes before allocated {string.Join(", ", bytes.Select(static one => one.ToString("N0")))} bytes.");
			Assert.Equal(accepted, read);

			if (bytes.Count > 0)
				Assert.True(allocated <= bytes[^1] * Slack, $"{what}: {size / 4} items allocated {bytes[^1]:N0} bytes and {size} allocated {allocated:N0}, {(double)allocated / bytes[^1]:F1} times as much.");

			bytes.Add(allocated);
		}
	}

	static string Nest(string head, string opener, string core, string closer, string tail, int depth)
	{
		return head + string.Concat(Enumerable.Repeat(opener, depth)) + core + string.Concat(Enumerable.Repeat(closer, depth)) + tail;
	}

	/// <summary>A comment a hundred thousand deep is read through every door: plain and located.</summary>
	[Fact]
	public void A_comment_nested_a_hundred_thousand_deep_is_read_through_every_door()
	{
		const int depth = 100_000;

		var text = string.Concat(Enumerable.Repeat("/*", depth)) + " 1 " + string.Concat(Enumerable.Repeat("*/", depth)) + " 1";

		Assert.True(PostgreSqlParser.TryParseExpression(text).IsSuccess);
		Assert.True(PostgreSqlParser.Located.TryParseExpression(text).IsSuccess);
		Assert.False(PostgreSqlParser.TryParseExpression(text[..^(2 * depth + 3)]).IsSuccess);
	}

	/// <summary>The text: the head, the unit as often as asked, the tail, and a value where the whole is a comment.</summary>
	static string Text(string head, string unit, string tail, int count, bool accepted)
	{
		var text = new StringBuilder(head);

		for (var i = 0; i < count; i++)
			text.Append(unit);

		if (unit == "/*")
		{
			text.Append(" 1 ");

			for (var i = 0; i < count && accepted; i++)
				text.Append(tail);

			return text.Append(" 1").ToString();
		}

		return text.Append(tail).ToString();
	}

	/// <summary>
	/// Reads the text on a thread of its own, so that the parser's stores start empty; whether it finished within the
	/// watchdog, whether it was accepted, and the bytes the thread allocated.
	/// </summary>
	static (bool Finished, bool Read, long Allocated) Read(string text, Func<string, bool> parse)
	{
		var read      = false;
		var allocated = 0L;
		var thread    = new Thread(
			() =>
			{
				var before = GC.GetAllocatedBytesForCurrentThread();

				read      = parse(text);
				allocated = GC.GetAllocatedBytesForCurrentThread() - before;
			},
			64 << 20)
		{
			// A reading the watchdog gave up on runs on until the process ends; it must not keep it alive.
			IsBackground = true,
		};

		thread.Start();

		var finished = thread.Join(Watchdog);

		return (finished, read, allocated);
	}
}
