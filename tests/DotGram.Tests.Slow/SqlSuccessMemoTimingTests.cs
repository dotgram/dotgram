using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the memo of successes costs in time, and what the two rewrites of the SQL-92 grammar would:
/// the median of five rounds over the statements each parser accepts. The only part of
/// <see cref="SqlSuccessMemoTests"/> that is Alone, so that the readings are not taken beside another
/// class's; it prints and asserts nothing about the numbers.
/// </summary>
[Collection(typeof(Alone))]
public sealed class SqlSuccessMemoTimingTests(ITestOutputHelper output)
{
	/// <summary>How many of the accepted statements a timing reads.</summary>
	const int Timed = 1500;

	void Log(string line)
	{
		output.WriteLine(line);

		if (Environment.GetEnvironmentVariable("SUCCESSMEMO_OUT") is { Length: > 0 } file)
			File.AppendAllText(file, line + "\n");
	}

	[Fact]
	public void Sql92_times_with_the_memo()
	{
		var unkept = SqlSuccessMemoTests.Immediate(SqlVariants.Sql92, keeps: false);
		var kept   = SqlSuccessMemoTests.Immediate(SqlVariants.Sql92);
		var cover  = SqlSuccessMemoTests.Immediate(SqlVariants.Sql92Cover);
		var split  = SqlSuccessMemoTests.Immediate(SqlVariants.Sql92Split);

		var accepted = SqlSuccessMemoTests.Accepted(unkept, "TryParseSelect", SqlSuccessMemoTests.Generated(3000, seed: 92), Timed);

		Timing("TryParseSelect", accepted, [("shipped, no memo", unkept), ("shipped + memo", kept), ("cover", cover), ("split", split)]);
		Timing("TryParseSelect", accepted, [("shipped, no memo", unkept), ("shipped + memo", kept), ("cover", cover), ("split", split)]);
	}

	[Fact]
	public void TSql_times_with_the_memo()
	{
		var unkept    = SqlSuccessMemoTests.Immediate(SqlVariants.TransactSql, keeps: false);
		var kept      = SqlSuccessMemoTests.Immediate(SqlVariants.TransactSql);
		var statement = SqlVariants.Entry(SqlVariants.TransactSql, "TryParseStatement");

		var accepted = SqlSuccessMemoTests.Accepted(unkept, statement, SqlSuccessMemoTests.Statements(), Timed);

		Timing(statement, accepted, [("shipped, no memo", unkept), ("shipped + memo", kept)]);
		Timing(statement, accepted, [("shipped, no memo", unkept), ("shipped + memo", kept)]);
	}

	[Fact]
	public void Sql2023_times_with_the_memo()
	{
		var unkept = SqlSuccessMemoTests.Immediate(SqlVariants.Standard, keeps: false);
		var kept   = SqlSuccessMemoTests.Immediate(SqlVariants.Standard);

		var accepted = SqlSuccessMemoTests.Accepted(unkept, "TryParseStatement", SqlSuccessMemoTests.Statements(), Timed);

		Timing("TryParseStatement", accepted, [("shipped, no memo", unkept), ("shipped + memo", kept)]);
	}

	void Timing(string publication, List<string> accepted, (string Name, Type Parser)[] parsers)
	{
		var texts   = accepted.Take(Timed).ToList();
		var methods = parsers.Select(one => one.Parser.GetMethod(publication, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null, [typeof(string)], modifiers: null)!).ToArray();
		var medians = new double[parsers.Length];
		var rounds  = new List<double>[parsers.Length];

		for (var i = 0; i < parsers.Length; i++)
			rounds[i] = [];

		// Warm up each, then five rounds, the parsers interleaved so that the machine's state is shared.
		for (var round = 0; round < 6; round++)
			for (var i = 0; i < parsers.Length; i++)
			{
				var clock = Stopwatch.StartNew();

				foreach (var text in texts)
					methods[i].Invoke(null, [text]);

				clock.Stop();

				if (round > 0)
					rounds[i].Add(clock.Elapsed.TotalMilliseconds * 1000.0 / texts.Count);
			}

		for (var i = 0; i < parsers.Length; i++)
		{
			var sorted = rounds[i].OrderBy(static one => one).ToList();

			medians[i] = sorted[sorted.Count / 2];
		}

		Log($"time per accepted generated statement ({texts.Count} texts, median of 5 rounds, directional on a shared machine):");

		for (var i = 0; i < parsers.Length; i++)
			Log($"  {parsers[i].Name,-16} {medians[i],8:F2} us  x{medians[i] / medians[0]:F3} of the first  (rounds: {string.Join(" ", rounds[i].Select(static one => one.ToString("F2")))})");
	}
}
