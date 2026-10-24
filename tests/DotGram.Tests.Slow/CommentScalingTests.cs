using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A text of block comments, closed or not, is read or refused in time linear in its length.
/// </summary>
/// <remarks>
/// <para>
/// A comment nests at any depth, so the counter that reads one reads to the end of the text before
/// it knows the comment is unclosed. The tokenizer once took the <c>/</c> and <c>*</c> of the opening
/// for tokens and asked the trivia again at the next opening inside it, which read to the end again:
/// the square of the length, five seconds for forty thousand openings. Now a comment the text ends
/// inside of ends the tokens where it opened, and is refused there; the text is read once.
/// </para>
/// <para>
/// A ratio and not a bound in seconds. Four times the openings take about four times as long when
/// the read is linear and sixteen times as long when it is quadratic, and ten separates them with
/// room on both sides; the two sizes are timed in one run (<see cref="ScalingClock"/>), so a slow
/// machine is slow for both. The texts are made before any clock runs: a text of a hundred thousand
/// openings is a megabyte on the large-object heap, and building one inside the timing was the
/// likeliest reason four times the depth once took thirteen times as long on a small runner.
/// </para>
/// <para>
/// Every depth's time is written down, passing or not, in <c>CommentTimes.txt</c> beside the assembly
/// and in the message of a failure, so that a figure for a runner has a source: the ratio of one pair
/// says little about a curve, and the whole ladder says where it bends.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class CommentScalingTests
{
	/// <summary>The ratio of the large size's time to the small one's above which the read is taken for superlinear.</summary>
	const double Limit = 10;

	/// <summary>The openings of the pair that is held to <see cref="Limit"/>: four times as many in the large one.</summary>
	const int Small = 25_000;

	const int Large = 100_000;

	/// <summary>The depths written down: the pair above, one size under it and one between, and two beyond.</summary>
	static readonly int[] Ladder = [12_500, 25_000, 50_000, 100_000, 200_000, 400_000];

	static readonly object Record = new();

	static readonly Dictionary<string, Func<int, string>> Shapes = new()
	{
		["unclosed"]    = static n => string.Concat(Enumerable.Repeat("/* ", n)),
		["half-closed"] = static n => string.Concat(Enumerable.Repeat("/* /* */ ", n)),
		["balanced"]    = static n => string.Concat(Enumerable.Repeat("/* ", n)) + string.Concat(Enumerable.Repeat("*/ ", n)),
	};

	/// <summary>Each reader, what it is given with a comment of a shape, and whether it takes it.</summary>
	static readonly (string Name, Func<string, string> Text, Func<string, bool> Reads)[] Readers =
	[
		("T-SQL statement, comment after",  static c => "SELECT 1 " + c, static t => TransactSqlParser.TryParseStatement(t).IsSuccess),
		("T-SQL statement, comment before", static c => c + "SELECT 1",  static t => TransactSqlParser.TryParseStatement(t).IsSuccess),
		("T-SQL located statement",         static c => "SELECT 1 " + c, static t => TransactSqlParser.Located.TryParseStatement(t).IsSuccess),
		("T-SQL script",                    static c => "SELECT 1\nGO\nSELECT 2 " + c, static t => TransactSqlParser.TryParseScript(t).IsSuccess),
		("SQL:2023 statement, comment after", static c => "SELECT 1 FROM t " + c, static t => SqlStandardParser.TryParseStatement(t).IsSuccess),
	];

	public static TheoryData<string, string> Rows()
	{
		var rows = new TheoryData<string, string>();

		foreach (var shape in Shapes.Keys)
			foreach (var (name, _, _) in Readers)
				rows.Add(shape, name);

		return rows;
	}

	[Theory]
	[MemberData(nameof(Rows))]
	public void Four_times_the_openings_do_not_cost_ten_times_as_long(string shape, string reader)
	{
		var (_, text, reads) = Readers.Single(one => one.Name == reader);

		// What a comment is put beside is read without one, so that a refusal is the comment's.
		Assert.True(reads(text("")), $"{reader}: the text beside the comment is not read without one.");

		// Made first, all of them, and read once each for what they should answer.
		var texts = Ladder.ToDictionary(static n => n, n => text(Shapes[shape](n)));

		foreach (var (size, one) in texts)
			Assert.True(reads(one) == (shape == "balanced"), $"{reader}, {shape}, {size:N0} openings: " + (shape == "balanced" ? "should be read" : "should be refused"));

		var small   = texts[Small];
		var large   = texts[Large];
		var reading = ScalingClock.Settle(() => reads(small), () => reads(large), Limit);

		var ladder = string.Join(", ", Ladder.Select(n => $"{n:N0}: {Best(() => reads(texts[n])):F2} ms"));
		var told   = string.Create(
			CultureInfo.InvariantCulture,
			$"{reader}, {shape}: four times the openings took {reading.Ratio:F1} times as long " +
			$"({reading.Shorter:F0} µs against {reading.Longer:F0} µs); every depth, best of 7: {ladder}.");

		lock (Record)
			File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "CommentTimes.txt"), told + Environment.NewLine);

		Console.WriteLine("comment depth | " + told);
		TestContext.Current.SendDiagnosticMessage(told);

		Assert.True(reading.Within, told + $" Measured {ScalingClock.Attempts} times ({reading.Readings}).");
	}

	/// <summary>The least time, in milliseconds, of seven reads.</summary>
	static double Best(Func<bool> read)
	{
		var least = double.MaxValue;

		for (var round = 0; round < 7; round++)
		{
			var watch = Stopwatch.StartNew();

			_ = read();

			least = Math.Min(least, watch.Elapsed.TotalMilliseconds);
		}

		return least;
	}
}
