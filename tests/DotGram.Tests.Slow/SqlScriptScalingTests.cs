using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using DotGram.Sql;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Cutting a script into batches costs the same per line however long the script, on the shapes
/// that could make a scanner go back over what it has passed.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SqlScript"/> is one forward pass: it looks at a line's start once, jumps from one
/// character that matters to the next, and keeps nothing that grows with what is behind it. These
/// are the inputs that would show it if that stopped being true — a separator on every other line,
/// one string ten characters to a repeat, a block comment that never closes after one that opens on
/// every line, one batch of many lines, a reference on every line, and a comment that opens every
/// line before a separator — and one more, the whole reading of a script of many batches, each read
/// through a window of the one string.
/// </para>
/// <para>
/// The bound is on the exponent of four times the input, and it is the 2.0 of
/// <see cref="ReaderScalingTests"/> for the reason given there: a shared runner's spread is wider
/// than anything tighter could tell from it. The square of the input is what a defect would read.
/// Measured on this machine at 50,000 to 800,000 repeats, the best of fifteen, under no other load:
/// 0.1 to 0.3 µs a line on every shape, the cost a line rising while the batches already built
/// outgrow the cache and flat from 400,000 to 800,000 — so a short pair of sizes reads an exponent
/// anywhere from 0.2 to 1.6 on the shapes that build a batch a line, and the bound is what catches
/// the square through that.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class SqlScriptScalingTests
{
	public static TheoryData<string> Shapes()
	{
		return [.. Make.Keys];
	}

	static readonly Dictionary<string, Func<int, string>> Make = new()
	{
		["GO lines"]               = static n => string.Concat(Enumerable.Repeat("SELECT 1\nGO\n", n)),
		["one long string"]        = static n => "SELECT '" + new string('x', n * 10) + "'\n",
		["unclosed comments"]      = static n => string.Concat(Enumerable.Repeat("/* \n", n)) + "GO\n",
		["one batch of lines"]     = static n => string.Concat(Enumerable.Repeat("SELECT 1\n", n)),
		["a reference a line"]     = static n => ":setvar x 1\n" + string.Concat(Enumerable.Repeat("SELECT $(x)\n", n)),
		["a comment before GO"]    = static n => string.Concat(Enumerable.Repeat("/* c */ GO\n", n)),
	};

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Four_times_the_script_does_not_cost_sixteen(string shape)
	{
		var small = Make[shape](25_000);
		var large = Make[shape](100_000);

		var shorter = Best(() => SqlScript.Read(small));
		var longer  = Best(() => SqlScript.Read(large));

		var exponent = Math.Log(longer / shorter) / Math.Log(4.0);

		Assert.True(
			exponent <= 2.0,
			$"With {shape}, four times the script took {longer / shorter:F1} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs), an exponent of {exponent:F2}.");
	}

	[Fact]
	public void Four_times_the_batches_do_not_cost_sixteen_to_read()
	{
		var small = Batches(500);
		var large = Batches(2_000);

		var shorter = Best(() => TransactSqlParser.TryParseScript(small));
		var longer  = Best(() => TransactSqlParser.TryParseScript(large));

		var exponent = Math.Log(longer / shorter) / Math.Log(4.0);

		Assert.True(
			exponent <= 2.0,
			$"Four times the batches took {longer / shorter:F1} times as long to read " +
			$"({shorter:F0} µs against {longer:F0} µs), an exponent of {exponent:F2}.");
	}

	static string Batches(int count)
	{
		var text = string.Concat(Enumerable.Repeat("SELECT a, b FROM t WHERE a = 1\r\nGO\r\n", count));

		Assert.Equal(count, TransactSqlParser.ParseScript(text).Length);

		return text;
	}

	/// <summary>The fastest of several runs of <paramref name="read"/>, in microseconds.</summary>
	static double Best(Func<object> read)
	{
		read();

		GC.Collect();
		GC.WaitForPendingFinalizers();

		var best = double.MaxValue;

		for (var run = 0; run < 7; run++)
		{
			var watch = Stopwatch.StartNew();

			read();

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}
}
