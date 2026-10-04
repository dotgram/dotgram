using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotGram.Tests;

/// <summary>
/// How the scaling classes read a ratio of two timings so that one scheduler hiccup cannot decide it.
/// </summary>
/// <remarks>
/// A row used to be the best of seven runs of a few milliseconds each at either size, the small size
/// timed completely before the large one: a pause of a millisecond or two on a loaded machine moved
/// the exponent by a quarter, and a pause that outlasted all seven runs of one size decided the row.
/// Here every timing covers at least <see cref="FloorMilliseconds"/> of work (a short read is
/// repeated inside the clock), the two sizes are timed in alternating rounds so that a slow stretch
/// of the machine falls on both, the least of the rounds is kept for each, and a ratio over its bound
/// is measured again, up to <see cref="Attempts"/> times, the least ratio counting: a defect that
/// makes a read quadratic shows in every attempt, a pause in one at most.
/// </remarks>
static class ScalingClock
{
	/// <summary>How many times a ratio over its bound is measured before it is believed.</summary>
	public const int Attempts = 3;

	/// <summary>The least work, in milliseconds, one timing covers.</summary>
	const double FloorMilliseconds = 25;

	const int Rounds = 11;

	/// <summary>Rounds always taken, however slow the reads: three are enough to tell a square from a pause.</summary>
	const int LeastRounds = 3;

	/// <summary>
	/// Past this many seconds of rounds in one attempt no more are started once <see cref="LeastRounds"/>
	/// are in, so that a read that has gone quadratic fails in minutes and not in hours.
	/// </summary>
	const double BudgetSeconds = 5;

	/// <summary>What <see cref="Settle"/> found.</summary>
	public sealed class Reading
	{
		public Reading(double ratio, double shorter, double longer, bool within, List<double> attempts)
		{
			Ratio    = ratio;
			Shorter  = shorter;
			Longer   = longer;
			Within   = within;
			Readings = string.Join(", ", attempts.Select(static one => one.ToString("F1")));
		}

		/// <summary>The least ratio of the large size's time to the small one's over the attempts.</summary>
		public double Ratio { get; }

		/// <summary>The small size's time, in microseconds, in the attempt that gave <see cref="Ratio"/>.</summary>
		public double Shorter { get; }

		/// <summary>The large size's time, in microseconds, in the attempt that gave <see cref="Ratio"/>.</summary>
		public double Longer { get; }

		/// <summary>Whether <see cref="Ratio"/> is within the bound.</summary>
		public bool Within { get; }

		/// <summary>Every attempt's ratio, for a message.</summary>
		public string Readings { get; }

		/// <summary>The exponent of <see cref="Ratio"/> for an input <paramref name="factor"/> times as large.</summary>
		public double Exponent(double factor)
		{
			return Math.Log(Ratio) / Math.Log(factor);
		}
	}

	/// <summary>
	/// Times <paramref name="small"/> and <paramref name="large"/> until their ratio is no more than
	/// <paramref name="bound"/> or <see cref="Attempts"/> attempts have been spent.
	/// </summary>
	public static Reading Settle(Action small, Action large, double bound)
	{
		small();
		large();

		var attempts = new List<double>();
		var best     = (Ratio: double.MaxValue, Shorter: 0.0, Longer: 0.0);

		for (var attempt = 1; attempt <= Attempts; attempt++)
		{
			var (shorter, longer) = Pair(small, large);
			var ratio             = longer / shorter;

			attempts.Add(ratio);

			if (ratio < best.Ratio)
				best = (ratio, shorter, longer);

			if (ratio <= bound)
				break;
		}

		return new Reading(best.Ratio, best.Shorter, best.Longer, best.Ratio <= bound, attempts);
	}

	/// <summary>The least time of one run of each, in microseconds, over rounds that alternate between the two.</summary>
	static (double Shorter, double Longer) Pair(Action small, Action large)
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();

		var smallRuns = Runs(small);
		var largeRuns = Runs(large);
		var shorter   = double.MaxValue;
		var longer    = double.MaxValue;

		var clock = Stopwatch.StartNew();

		for (var round = 0; round < Rounds && (round < LeastRounds || clock.Elapsed.TotalSeconds < BudgetSeconds); round++)
		{
			shorter = Math.Min(shorter, Time(small, smallRuns));
			longer  = Math.Min(longer,  Time(large, largeRuns));
		}

		return (shorter, longer);
	}

	/// <summary>How many runs make a timing cover <see cref="FloorMilliseconds"/>, judged by the fastest of up to three.</summary>
	static int Runs(Action run)
	{
		var least = double.MaxValue;

		for (var one = 0; one < 3; one++)
		{
			var watch = Stopwatch.StartNew();

			run();

			least = Math.Min(least, watch.Elapsed.TotalMilliseconds);

			if (least >= FloorMilliseconds)
				break;
		}

		return least >= FloorMilliseconds ? 1 : (int)Math.Min(1000, Math.Ceiling(FloorMilliseconds / Math.Max(least, 0.005)));
	}

	/// <summary>One run's time in microseconds, from <paramref name="runs"/> back to back.</summary>
	static double Time(Action run, int runs)
	{
		var watch = Stopwatch.StartNew();

		for (var one = 0; one < runs; one++)
			run();

		return watch.Elapsed.TotalMilliseconds * 1000 / runs;
	}
}
