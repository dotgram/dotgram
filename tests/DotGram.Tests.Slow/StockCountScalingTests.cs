using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Examples.Feeds;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A recovering repetition costs the same per line however far into its input the line is: a
/// count ten times as long, with the same share of broken lines, takes about ten times as long
/// to read, not a hundred.
/// </summary>
/// <remarks>
/// <para>
/// Every tenth line of these counts is broken, and the grammar asks for each broken line's
/// number (<c>parserLine</c>). That number used to be counted from the start of the input for
/// each of them, which made a count with rejections quadratic in its length. The bound, thirty
/// against ten, leaves room for noise and none for that (<see cref="Linear"/>'s own remarks say
/// how). Timed alone, the best of several runs.
/// </para>
/// <para>
/// <b>Since 2026-10-03 a control gates the class, rather than the two tests failing CI on its own
/// noise.</b> <c>And_from_a_reader</c> read ratios as high as 33.8 and 35.6 against this same
/// bound of 30 on a shared Linux runner (2026-10-02; the lower one on commit c230688c, run
/// 37050539448), where a quiet machine reads about 10.5. The control is a count with no broken
/// lines at all: it never asks the grammar to recover, so it cannot be made quadratic by the
/// defect above, and a reading of it over bound is a statement about the run and not about the
/// reader — the same argument <c>ReaderScalingTests</c> makes for its own "no block at all" row.
/// It is measured once per process, cached, and retried up to <see cref="ScalingClock.Attempts"/> times if a
/// reading exceeds <see cref="Linear"/>; the first clean reading is trusted. If none of those is
/// clean, both tests of the class are skipped as inconclusive, naming the control's readings: a
/// control that only ever says "the machine was noisy" should not then fail CI on exactly that
/// noise, and the two real readings of that same noisy run are not trustworthy either.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class StockCountScalingTests
{
	/// <summary>What ten times the lines may take, as a multiple of what one tenth of them takes.</summary>
	/// <remarks>
	/// <b>Derived, not chosen.</b> Three numbers fix it. The reading is linear and measures
	/// 10.2 and 10.8 for ten times the input on a quiet machine (2026-09-24, both forms). The
	/// defect this guards is quadratic — <c>parserLine</c> counted from the start of the input
	/// for every broken line — which is about a hundred at ten times the input. And CI is a
	/// shared runner, where the large size is inflated more than the small one, which is what
	/// the previous bound of 15 could not survive: it left 1.4x over a measurement of 10.5.
	/// <para>
	/// Thirty sits near the geometric middle of 10.5 and 100: 2.9x of room above what the
	/// reading costs, and 3.3x of margin below what the defect costs. Both margins are stated
	/// because a bound with only one of them is either flaky or decorative.
	/// </para>
	/// </remarks>
	const double Linear = 30;

	/// <summary>How many times the control is measured before a reading over its bound is believed.</summary>
	[Fact]
	public void A_count_with_broken_lines_reads_in_time_linear_in_its_length_from_a_string()
	{
		var control = Control.Value;

		if (!control.Within)
		{
			Assert.Skip(InconclusiveMessage(control));

			return;
		}

		var shorter = Count(1_000);
		var longer  = Count(10_000);

		CheckBroken(shorter);
		CheckBroken(longer);

		AssertLinear(ScalingClock.Settle(() => StockCountReader.Read(shorter), () => StockCountReader.Read(longer), Linear));
	}

	[Fact]
	public void And_from_a_reader()
	{
		var control = Control.Value;

		if (!control.Within)
		{
			Assert.Skip(InconclusiveMessage(control));

			return;
		}

		var shorter = Count(1_000);
		var longer  = Count(10_000);

		CheckBroken(shorter);
		CheckBroken(longer);

		AssertLinear(ScalingClock.Settle(
			() => StockCountReader.Read(new StringReader(shorter)),
			() => StockCountReader.Read(new StringReader(longer)),
			Linear));
	}

	/// <summary>What to tell xunit when the control never read clean; see the class remarks.</summary>
	static string InconclusiveMessage(ScalingClock.Reading control)
	{
		return
			$"The control row (a count with no broken lines) read {control.Readings} against a bound " +
			$"of {Linear} on every one of {ScalingClock.Attempts} measurements; the machine is noisy and neither of " +
			"this class's tests can be trusted this run.";
	}

	static void AssertLinear(ScalingClock.Reading reading)
	{
		Assert.True(
			reading.Within,
			$"Ten times the lines took {reading.Ratio:F1} times as long " +
			$"({reading.Shorter:F0} µs against {reading.Longer:F0} µs), against a bound of {Linear}, " +
			$"on every one of {ScalingClock.Attempts} measurements ({reading.Readings}).");
	}

	/// <summary>
	/// The control's own reading for the whole process: measured once and shared by both tests so
	/// that a noisy control can skip the class instead of failing it.
	/// </summary>
	static readonly Lazy<ScalingClock.Reading> Control = new(
		static () =>
		{
			var shorter = CountAllGood(1_000);
			var longer  = CountAllGood(10_000);

			CheckAllGood(shorter);
			CheckAllGood(longer);

			return ScalingClock.Settle(() => StockCountReader.Read(shorter), () => StockCountReader.Read(longer), Linear);
		});

	static void CheckBroken(string text)
	{
		var count = StockCountReader.Read(text);

		Assert.Equal(count.Lines.Count / 10, count.Lines.Count - count.Total);
	}

	static void CheckAllGood(string text)
	{
		var count = StockCountReader.Read(text);

		Assert.Equal(count.Lines.Count, count.Total);
	}

	static string Count(int lines)
	{
		var text = new StringBuilder();
		var good = 0;

		for (var line = 0; line < lines; line++)
		{
			if (line % 10 == 9)
				text.Append("x 1\n");
			else
			{
				text.Append(Item(line)).Append(": ").Append(line % 100).Append('\n');
				good++;
			}
		}

		return text.Append("END ").Append(good).Append('\n').ToString();
	}

	/// <summary>
	/// A count of so many lines, none of them broken: the control, linear by construction because
	/// it never asks the grammar to recover (see the class remarks).
	/// </summary>
	static string CountAllGood(int lines)
	{
		var text = new StringBuilder();

		for (var line = 0; line < lines; line++)
			text.Append(Item(line)).Append(": ").Append(line % 100).Append('\n');

		return text.Append("END ").Append(lines).Append('\n').ToString();
	}

	/// <summary>An item named in letters only, as the grammar's names are: item a, b, … z, ba, ….</summary>
	static string Item(int number)
	{
		var name = "";

		do
		{
			name   = (char)('a' + number % 26) + name;
			number /= 26;
		}
		while (number > 0);

		return "item" + name;
	}
}
