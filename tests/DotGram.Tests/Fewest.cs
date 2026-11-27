using System;
using System.Diagnostics;

namespace DotGram.Tests;

/// <summary>
/// The fewest milliseconds of repeated readings, for a guard against a cost that grows with the
/// input.
/// </summary>
/// <remarks>
/// One reading beside other tests, which compile and collect all the while, can fall behind a
/// stalled scheduler or a collection and cost several times what it costs alone, and the guard then
/// fails on time that is not the regression. A cost that grows with the input is in every reading,
/// so the fewest of enough of them is the figure the code itself costs, and the limit stays as
/// strict as it was.
/// </remarks>
static class Fewest
{
	/// <summary>
	/// The fewest time of rounds of <paramref name="run"/>: it stops as soon as a round is within
	/// <paramref name="limit"/> (no later round can make that worse), or when three rounds in a row
	/// bring nothing more than 1% lower, or after <see cref="MaxRounds"/>.
	/// </summary>
	public static TimeSpan Of(Action run, TimeSpan limit)
	{
		var best   = TimeSpan.MaxValue;
		var steady = 0;

		for (var round = 0; round < MaxRounds && steady < 3; round++)
		{
			var watch = Stopwatch.StartNew();

			run();

			var took = watch.Elapsed;

			if (took < best - best / 100)
				steady = 0;
			else
				steady++;

			if (took < best)
				best = took;

			if (best < limit)
				break;
		}

		return best;
	}

	public const int MaxRounds = 20;
}
