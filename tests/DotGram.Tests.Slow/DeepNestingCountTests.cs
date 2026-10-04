using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A nest whose every level builds a value while it is read costs the walks that build them its
/// depth, not the square of it: counted in the records the walks read, at four depths a doubling apart.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it holds.</b> A guard that is handed a value has the walk build it from the log while the
/// text is still being read, and SQL:2023 has a guard at nearly every level of an expression, each
/// asking for the level's operand: in a nest, everything below. The walk listed the log from the
/// guard's rule to its end and stepped back over every record to find what the operand reached, so
/// each level read the whole nest below it, nearly all of it built already for the guards inside.
/// The reader's own work was flat — its rule entries grew by the same amount at every doubling —
/// while the records the walks read grew four times a doubling: 1,391,000, 5,532,050, 22,064,150 and
/// 88,128,350 at 100 to 800 levels of <c>CASE … ELSE CASE …</c>, and ten thousand levels of it took
/// twenty-five seconds. T-SQL's nested subqueries did the same with one walk a level: 2,246,807 at
/// 800. Now a walk follows the references down from what it was asked for and stops at what is
/// built, over an index of the log kept from one walk to the next.
/// </para>
/// <para>
/// <b>Counted, not timed</b> (D144). The materializer counts what it indexes and what it reaches where
/// the compilation defines <c>DOTGRAM_COUNTS</c>; the grammars are compiled here with the symbol set,
/// as they ship (<c>SqlVariants</c>). The counters are static, and the other count tests compile the
/// same variants, so this runs alone. What each doubling adds may be at most twice what the one before
/// added, with a tenth to spare for the ends, and each depth is held under a bound a level, so that a
/// constant creeping up is seen too. Counted at 50, 100, 200 and 400 levels:
/// </para>
/// <code>
///   SQL:2023 CASE in ELSE     5,564    11,064    22,064    44,064
///   SQL:2023 CASE in THEN     3,964     7,864    15,664    31,264
///   SQL:2023 CASE operand     3,364     6,664    13,264    26,464
///   SQL:2023 COALESCE         3,064     6,064    12,064    24,064
///   SQL:2023 IN (SELECT …)    5,012     9,912    19,712    39,312
///   SQL:2023 subqueries       3,164     6,264    12,464    24,864
///   T-SQL subqueries            716     1,416     2,816     5,616
///   T-SQL CASE in ELSE        1,516     3,016     6,016    12,016
///   T-SQL COALESCE              616     1,216     2,416     4,816
///   T-SQL IN (SELECT …)       1,534     3,034     6,034    12,034
/// </code>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class DeepNestingCountTests(ITestOutputHelper output)
{
	/// <summary>How much more than twice a doubling may add, for what the ends of the text cost.</summary>
	const double Slack = 1.1;

	static readonly int[] Depths = [50, 100, 200, 400];

	[Theory]
	[InlineData("CASE in ELSE",  "SELECT ",                "CASE WHEN 1 = 1 THEN 1 ELSE ",  "1",     " END",              " FROM t", 140)]
	[InlineData("CASE in THEN",  "SELECT ",                "CASE WHEN 1 = 1 THEN ",         "1",     " END",              " FROM t", 100)]
	[InlineData("CASE operand",  "SELECT ",                "CASE ",                         "1",     " WHEN 1 THEN 1 END", " FROM t", 85)]
	[InlineData("COALESCE",      "SELECT ",                "COALESCE(1, ",                  "1",     ")",                 " FROM t", 75)]
	[InlineData("IN (SELECT …)", "SELECT 1 FROM t WHERE ", "1 IN (SELECT 1 FROM t WHERE ",  "1 = 1", ")",                 "",        125)]
	[InlineData("subqueries",    "SELECT ",                "(SELECT ",                      "1",     " FROM t)",          " FROM t", 80)]
	public void A_SQL_2023_nest_is_walked_once(string shape, string prefix, string open, string middle, string close, string suffix, int perLevel)
	{
		Holds(SqlVariants.Parser(SqlVariants.Standard, counts: true), "TryParseQueryExpression", "SQL:2023 " + shape, depth => Nested(prefix, open, middle, close, suffix, depth), perLevel);
	}

	[Theory]
	[InlineData("subqueries",    "SELECT ",         "(SELECT ",                     "1",     ")",                  "", 20)]
	[InlineData("CASE in ELSE",  "SELECT ",         "CASE WHEN 1 = 1 THEN 1 ELSE ", "1",     " END",               "", 40)]
	[InlineData("COALESCE",      "SELECT ",         "COALESCE(1, ",                 "1",     ")",                  "", 16)]
	[InlineData("IN (SELECT …)", "SELECT 1 WHERE ", "1 IN (SELECT 1 WHERE ",        "1 = 1", ")",                  "", 40)]
	public void A_TSql_nest_is_walked_once(string shape, string prefix, string open, string middle, string close, string suffix, int perLevel)
	{
		Holds(SqlVariants.Parser(SqlVariants.TransactSql, counts: true), "TryParseStatement", "T-SQL " + shape, depth => Nested(prefix, open, middle, close, suffix, depth), perLevel);
	}

	void Holds(Type parser, string publication, string shape, Func<int, string> text, int perLevel)
	{
		var counts = new List<long>();

		foreach (var depth in Depths)
		{
			counts.Add(Count(parser, publication, text(depth)));
			output.WriteLine($"{shape}: {counts[^1]:N0} records read by the walks at {depth} levels");

			// Or the counters are not being written and everything below passes on nothing.
			Assert.True(counts[0] > 0, $"{shape}: nothing was counted at {Depths[0]} levels.");

			Assert.True(
				counts[^1] <= (long)perLevel * depth + 500,
				$"{shape}: the walks read {counts[^1]:N0} records at {depth} levels, over the bound of {perLevel} a level.");

			if (counts.Count < 3)
				continue;

			var earlier = counts[^2] - counts[^3];
			var later   = counts[^1] - counts[^2];

			Assert.True(
				earlier > 0 && later <= earlier * 2 * Slack,
				$"{shape}: the walks read {string.Join(", ", counts)} records at {string.Join(", ", Depths.Take(counts.Count))} " +
				$"levels, so a doubling added {later} where the one before added {earlier}, " +
				$"{(double)later / earlier:F2} times as much where the depth is held to twice.");
		}
	}

	/// <summary>The records the walks of one reading listed and reached, from a fresh zero.</summary>
	static long Count(Type parser, string publication, string text)
	{
		var counter = parser.GetNestedType("Ways", BindingFlags.NonPublic)!.GetField("CountListed", BindingFlags.NonPublic | BindingFlags.Static)
			?? throw new InvalidOperationException("the generated Ways has no CountListed; is it still emitted under DOTGRAM_COUNTS?");

		counter.SetValue(null, 0L);

		var read = parser.GetMethod(
			publication,
			BindingFlags.Public | BindingFlags.Static,
			binder: null,
			[typeof(string)],
			modifiers: null)!;

		object?    match = null;
		Exception? error = null;

		// On a stack with room for the nest, so that no reading is carried onto another thread.
		var thread = new Thread(() =>
		{
			try { match = read.Invoke(null, [text]); }
			catch (Exception e) { error = e; }
		}, 256 * 1024 * 1024);

		thread.Start();
		thread.Join();

		if (error is not null)
			throw new InvalidOperationException("the counted reading threw", error);

		Assert.True((bool)match!.GetType().GetProperty("IsSuccess")!.GetValue(match)!, "the nest was refused.");

		return (long)counter.GetValue(null)!;
	}

	static string Nested(string prefix, string open, string middle, string close, string suffix, int depth)
	{
		return prefix + string.Concat(Enumerable.Repeat(open, depth)) + middle + string.Concat(Enumerable.Repeat(close, depth)) + suffix;
	}
}
