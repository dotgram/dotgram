using System;
using System.Diagnostics;
using System.Linq;

using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A script read one statement at a time costs the same per statement however many there are:
/// forty times the statements take about forty times as long, and not sixteen hundred.
/// </summary>
/// <remarks>
/// <para>
/// A reading that begins where it is told cuts the whole input into tokens, so a host reading
/// a script statement by statement cut the whole script once for every statement — the text
/// once per statement, which is the square of the text. The stand read 10, 100 and 400
/// statements at 56 µs, 1.9 ms and 30.3 ms, an exponent of 1.99 over a linear allocation.
/// The cutting is kept for the next reading of the same string now, so the script is cut once.
/// </para>
/// <para>
/// The bound is on the exponent and not on a time: ten times the statements may take up to
/// about thirteen times as long (1.1), which leaves room for noise and none for the square.
/// Timed alone, the best of several runs.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class ScriptScalingTests
{
	[Fact]
	public void A_script_read_one_statement_at_a_time_costs_the_same_for_each()
	{
		var shorter = Best(10);
		var longer  = Best(400);

		var exponent = Math.Log(longer / shorter) / Math.Log(40.0);

		Assert.True(
			exponent <= 1.1,
			$"Forty times the statements took {longer / shorter:F1} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs), an exponent of {exponent:F2}.");
	}

	/// <summary>The fastest of several readings of a script of <paramref name="statements"/>, in microseconds.</summary>
	static double Best(int statements)
	{
		var text = Script(statements);

		Assert.Equal(statements, Read(text));

		GC.Collect();
		GC.WaitForPendingFinalizers();

		var best = double.MaxValue;

		for (var run = 0; run < 7; run++)
		{
			var watch = Stopwatch.StartNew();

			Read(text);

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}

	/// <summary>How many statements a reading from a position finds, one after another.</summary>
	static int Read(string text)
	{
		var read = 0;
		var at   = 0;

		while (at < text.Length)
		{
			while (at < text.Length && (text[at] == ';' || char.IsWhiteSpace(text[at])))
				at++;

			if (at == text.Length)
				break;

			if (!TransactSqlParser.TryParseStatement(text, ref at, out _))
				throw new InvalidOperationException($"The script is not read at {at}.");

			read++;
		}

		return read;
	}

	static string Script(int statements) =>
		string.Concat(Enumerable.Repeat("SELECT a, b FROM t WHERE a = 1;\r\n", statements));
}
