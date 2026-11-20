using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Xunit;

namespace DotGram.Tests.SlowShapes;

/// <summary>What one input cost at each size it was read at.</summary>
/// <param name="Sizes">The sizes reached, in characters of input.</param>
/// <param name="Entries">The rule entries of one reading at each size.</param>
/// <param name="Milliseconds">The time of that reading at each size: directional, since it is taken beside the counters.</param>
sealed record SlowLadder(int[] Sizes, long[] Entries, double[] Milliseconds)
{
	/// <summary>The exponent of the entries between the first size and the last: 1 is linear, 2 is quadratic.</summary>
	public double Exponent
	{
		get
		{
			return Math.Log((double)Entries[^1] / Entries[0]) / Math.Log((double)Sizes[^1] / Sizes[0]);
		}
	}

	/// <summary>The exponent of the time over the last three sizes, where the early ones are too short to read off a clock.</summary>
	public double TimeExponent
	{
		get
		{
			var last = Sizes.Length - 1;

			return Math.Log(Milliseconds[last] / Milliseconds[last - 2]) / Math.Log((double)Sizes[last] / Sizes[last - 2]);
		}
	}
}

static class SlowShapeLadder
{
	/// <summary>The exponent up to which a shape that has been given the generator work it waited for is held.</summary>
	public const double LinearBound = 1.2;

	/// <summary>How far the exponent may be from the one recorded for a shape not yet fixed before the record is out of date, in either direction.</summary>
	public const double Tolerance = 0.25;

	/// <summary>The same for a shape held by its time, which a clock reads less closely.</summary>
	public const double TimeTolerance = 0.5;

	/// <summary>The sizes a ladder takes from its first: six, each twice the one before, so that 2,048 gives 16, 32 and 64 thousand characters at its top.</summary>
	const int Doublings = 6;

	/// <summary>A reading this long, in milliseconds, ends the ladder: the next would take four times as long and a refusal that costs a minute is no row.</summary>
	const double GiveUpMilliseconds = 3000;

	/// <summary>The fewest sizes a ladder must reach to have an exponent.</summary>
	const int LeastSizes = 4;

	public static SlowLadder Climb(Type parser, string entry, SlowInput input)
	{
		var sizes   = new List<int>();
		var entries = new List<long>();
		var times   = new List<double>();

		for (var i = 0; i < Doublings; i++)
		{
			var size = input.First << i;

			var (count, milliseconds, _) = Count(parser, entry, input.Text(size), input.Accepted);

			sizes.Add(size);
			entries.Add(count);
			times.Add(milliseconds);

			if (milliseconds > GiveUpMilliseconds)
				break;
		}

		return new SlowLadder([.. sizes], [.. entries], [.. times]);
	}

	/// <summary>Checks a ladder against what is held of its input, and says what it found.</summary>
	public static void Hold(SlowShape shape, SlowInput input, SlowLadder ladder, Action<string> write)
	{
		var label    = $"{shape.Id} | {input.Name}";
		var exponent = input.ByTime ? ladder.TimeExponent : ladder.Exponent;

		write($"{label}: {(input.ByTime ? "time exponent" : "exponent")} {exponent.ToString("F2", CultureInfo.InvariantCulture)} over {ladder.Sizes[0]:N0} to {ladder.Sizes[^1]:N0} characters ({input.Item ?? shape.Item})");

		for (var i = 0; i < ladder.Sizes.Length; i++)
			write($"    {ladder.Sizes[i],7:N0}  {ladder.Entries[i],14:N0} rule entries  {ladder.Milliseconds[i],9:F1} ms");

		Assert.True(ladder.Sizes.Length >= LeastSizes, $"{label}: only {ladder.Sizes.Length} sizes reached before a reading took over {GiveUpMilliseconds:N0} ms; give the input a lower First.");

		if (input.Linear)
		{
			Assert.True(exponent <= LinearBound, $"{label}: exponent {exponent:F2} is over {LinearBound}, and this shape is held to a linear refusal.");

			return;
		}

		// A shape not yet fixed is held to what it is. A refusal that has become cheaper is the news the record waits for:
		// it fails, and the fix is to set Linear on the input.
		Assert.True(
			Math.Abs(exponent - input.Today) <= (input.ByTime ? TimeTolerance : Tolerance),
			$"{label}: exponent {exponent:F2}, recorded {input.Today:F1}. If the generator work for {input.Item ?? shape.Item} has landed and this is now " +
			$"at most {LinearBound}, set Linear on this input; otherwise the shape changed and the record is wrong.");
	}

	/// <summary>Writes the table of every ladder where <c>SLOWSHAPES_REPORT</c> names a directory.</summary>
	public static void Report(IEnumerable<string> lines)
	{
		if (Environment.GetEnvironmentVariable("SLOWSHAPES_REPORT") is not { Length: > 0 } directory)
			return;

		Directory.CreateDirectory(directory);
		File.AppendAllLines(Path.Combine(directory, "slow-shapes.txt"), lines);
	}

	/// <summary>Operations of one reading by a counted parser's entry from a fresh zero (every rule entry, and the records the materialising walks listed), and its time.</summary>
	static (long Entries, double Milliseconds, bool Accepted) Count(Type parser, string entry, string text, bool accepted)
	{
		var counters = parser.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
			.SelectMany(static type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
			.Where(static field => field.Name.StartsWith("Count", StringComparison.Ordinal) && field.FieldType == typeof(long))
			.ToList();

		Assert.NotEmpty(counters);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var read = parser.GetMethod(entry, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null, [typeof(string)], modifiers: null)
			?? throw new InvalidOperationException($"{parser.FullName} has no {entry}(string).");

		object?    match   = null;
		Exception? error   = null;
		var        elapsed = TimeSpan.Zero;

		// On a stack with room for a nest thousands deep, so that no reading is carried onto another thread.
		var thread = new Thread(() =>
		{
			try
			{
				var watch = Stopwatch.StartNew();

				match   = read.Invoke(null, [text]);
				elapsed = watch.Elapsed;
			}
			catch (Exception e)
			{
				error = e;
			}
		}, 512 * 1024 * 1024);

		thread.Start();
		thread.Join();

		if (error is not null)
			throw new InvalidOperationException("the counted reading threw", error);

		var ok = (bool)match!.GetType().GetProperty("IsSuccess")!.GetValue(match)!;

		Assert.Equal(accepted, ok);

		return (counters.Sum(static counter => (long)counter.GetValue(null)!), elapsed.TotalMilliseconds, ok);
	}
}
