using System;
using System.Diagnostics;
using System.IO;
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
/// Every tenth line of these counts is broken, and the grammar asks for each broken line's
/// number (<c>parserLine</c>). That number used to be counted from the start of the input for
/// each of them, which made a count with rejections quadratic in its length. The bound, fifteen
/// against ten, leaves room for noise and none for that. Timed alone, the best of several runs.
/// </remarks>
[Collection(nameof(Alone))]
public sealed class StockCountScalingTests
{
	[Fact]
	public void A_count_with_broken_lines_reads_in_time_linear_in_its_length_from_a_string()
	{
		var shorter = Count(1_000);
		var longer  = Count(10_000);

		AssertLinear(Best(() => StockCountReader.Read(shorter)), Best(() => StockCountReader.Read(longer)));
	}

	[Fact]
	public void And_from_a_reader()
	{
		var shorter = Count(1_000);
		var longer  = Count(10_000);

		AssertLinear(
			Best(() => StockCountReader.Read(new StringReader(shorter))),
			Best(() => StockCountReader.Read(new StringReader(longer))));
	}

	static void AssertLinear(double shorter, double longer)
	{
		Assert.True(longer / shorter < 15, $"Ten times the lines took {longer / shorter:F1} times as long ({shorter:F0} µs against {longer:F0} µs).");
	}

	/// <summary>The fastest of several reads, in microseconds, after one to compile it.</summary>
	static double Best(Func<StockCount> read)
	{
		var count = read();

		Assert.Equal(count.Lines.Count / 10, count.Lines.Count - count.Total);

		var best = double.MaxValue;

		for (var run = 0; run < 7; run++)
		{
			var watch = Stopwatch.StartNew();

			read();

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}

	/// <summary>A count of so many lines, every tenth of them broken, and its closing line.</summary>
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
