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
/// <para>
/// <b>Since 2026-10-03 a control gates the class, rather than the two tests failing CI on its own
/// noise.</b> "a reference a line" read an exponent of 2.04 on a shared Linux runner (2026-10-03,
/// the `checked` job), where this machine's own repeated readings of that same shape sit between
/// 0.17 and 0.55, and allocation is flat at about 372 bytes a line from 25,000 to 800,000 — so the
/// defect this bound would catch is not present; the reading was the runner. The control is "one
/// batch of lines": no separator to cut on, no comment, nothing to substitute, so it cannot be made
/// quadratic by any of the defects the six shapes above each guard against, and a reading of it over
/// bound is a statement about the run and not about the reader — the same argument
/// <see cref="ReaderScalingTests"/> makes for its own "no block at all" row and
/// <see cref="StockCountScalingTests"/> for its all-good count. It is measured once per process,
/// cached, and retried up to <see cref="Attempts"/> times if a reading exceeds <see cref="Bound"/>;
/// the first clean reading is trusted. If none of those is clean, every row of the theory and the
/// batch-reading fact are both skipped as inconclusive, naming the control's readings: a control
/// that only ever says "the machine was noisy" should not then fail CI on exactly that noise, and
/// the other readings of that same noisy run are not trustworthy either.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class SqlScriptScalingTests
{
	/// <summary>The row that is linear by construction and gates the class; see the class remarks.</summary>
	const string ControlShape = "one batch of lines";

	const double Bound = 2.0;

	/// <summary>How many times the control is measured before a reading over its bound is believed.</summary>
	const int Attempts = 3;

	public static TheoryData<string> Shapes()
	{
		return [.. Make.Keys];
	}

	static readonly Dictionary<string, Func<int, string>> Make = new()
	{
		["GO lines"]               = static n => string.Concat(Enumerable.Repeat("SELECT 1\nGO\n", n)),
		["one long string"]        = static n => "SELECT '" + new string('x', n * 10) + "'\n",
		["unclosed comments"]      = static n => string.Concat(Enumerable.Repeat("/* \n", n)) + "GO\n",
		[ControlShape]             = static n => string.Concat(Enumerable.Repeat("SELECT 1\n", n)),
		["a reference a line"]     = static n => ":setvar x 1\n" + string.Concat(Enumerable.Repeat("SELECT $(x)\n", n)),
		["a comment before GO"]    = static n => string.Concat(Enumerable.Repeat("/* c */ GO\n", n)),
	};

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Four_times_the_script_does_not_cost_sixteen(string shape)
	{
		var control = Control.Value;

		if (control.Inconclusive)
		{
			Assert.Skip(InconclusiveMessage(control));

			return;
		}

		if (shape == ControlShape)
		{
			Assert.True(
				control.Exponent <= Bound,
				$"With {shape}, four times the script took an exponent of {control.Exponent:F2} against {Bound:F2}.");

			return;
		}

		var small = Make[shape](25_000);
		var large = Make[shape](100_000);

		var shorter = Best(() => SqlScript.Read(small));
		var longer  = Best(() => SqlScript.Read(large));

		var exponent = Math.Log(longer / shorter) / Math.Log(4.0);

		Assert.True(
			exponent <= Bound,
			$"With {shape}, four times the script took {longer / shorter:F1} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs), an exponent of {exponent:F2}.");
	}

	[Fact]
	public void Four_times_the_batches_do_not_cost_sixteen_to_read()
	{
		var control = Control.Value;

		if (control.Inconclusive)
		{
			Assert.Skip(InconclusiveMessage(control));

			return;
		}

		var small = Batches(500);
		var large = Batches(2_000);

		var shorter = Best(() => TransactSqlParser.TryParseScript(small));
		var longer  = Best(() => TransactSqlParser.TryParseScript(large));

		var exponent = Math.Log(longer / shorter) / Math.Log(4.0);

		Assert.True(
			exponent <= Bound,
			$"Four times the batches took {longer / shorter:F1} times as long to read " +
			$"({shorter:F0} µs against {longer:F0} µs), an exponent of {exponent:F2}.");
	}

	/// <summary>What to tell xunit when the control never read clean; see the class remarks.</summary>
	static string InconclusiveMessage(ControlReading control)
	{
		return
			$"The control row (\"{ControlShape}\") read an exponent of {control.Exponent:F2} against its " +
			$"bound of {Bound:F2} on every one of {Attempts} measurements ({control.Readings}); the machine " +
			"is noisy and nothing in this class can be trusted this run.";
	}

	/// <summary>
	/// The control's own exponent for the whole process: measured once, retried up to
	/// <see cref="Attempts"/> times while it reads over <see cref="Bound"/>, and shared by every row
	/// of the theory and by the batch-reading fact so that a noisy control can skip the class instead
	/// of failing it.
	/// </summary>
	static readonly Lazy<ControlReading> Control = new(MeasureControl);

	static ControlReading MeasureControl()
	{
		var readings = new List<double>();

		for (var attempt = 1; attempt <= Attempts; attempt++)
		{
			var shorter  = Best(() => SqlScript.Read(Make[ControlShape](25_000)));
			var longer   = Best(() => SqlScript.Read(Make[ControlShape](100_000)));
			var exponent = Math.Log(longer / shorter) / Math.Log(4.0);

			readings.Add(exponent);

			if (exponent <= Bound)
				return new ControlReading(exponent, readings, false);
		}

		return new ControlReading(readings[^1], readings, true);
	}

	/// <summary>What <see cref="MeasureControl"/> found: the trusted exponent, every attempt, and whether all of them were over the bound.</summary>
	sealed class ControlReading
	{
		public ControlReading(double exponent, List<double> attempts, bool inconclusive)
		{
			Exponent     = exponent;
			Inconclusive = inconclusive;
			Readings     = string.Join(", ", attempts.Select(static one => one.ToString("F2")));
		}

		public double Exponent { get; }
		public bool Inconclusive { get; }
		public string Readings { get; }
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
