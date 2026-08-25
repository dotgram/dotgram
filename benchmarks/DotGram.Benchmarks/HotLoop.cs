using System;
using System.Diagnostics;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// Not a benchmark: a process a profiler can attach to and get a line-by-line breakdown
/// from, rather than a single number.
/// </summary>
/// <remarks>
/// The two inputs are <see cref="UrlBenchmarks"/>'s own losing cases against
/// <c>RegexOptions.Compiled</c> — the refusal, and the URL with every part present
/// (<c>benchmarks/README.md</c>, "Current result" and "What captures cost"). Alternated
/// rather than run one at a time: both go through the same <c>TryParseUrl</c> entry
/// point, so a sampling profiler tells the two apart by which lines under it got hot, not
/// by which process ran — one run covers both failure modes at once.
/// </remarks>
static class HotLoop
{
	const string NoMatch   = "https://exa mple.com/";
	const string EveryPart = "https://user@example.com:8080/a/b/c?q=1&r=2#top";

	public static void Run(int seconds)
	{
		// Warm: the first parse of a thread builds the parser it will then keep, and the
		// first call of a method jits it. Neither is what steady-state costs, and neither
		// is what the profiler should spend its samples on.
		for (var i = 0; i < 10_000; i++)
		{
			Urls.TryParseUrl(NoMatch);
			Urls.TryParseUrl(EveryPart);
		}

		Console.WriteLine($"Attach the profiler now. Running for {seconds}s...");

		var deadline = Stopwatch.StartNew();
		var runs     = 0L;

		while (deadline.Elapsed.TotalSeconds < seconds)
		{
			Urls.TryParseUrl(NoMatch);
			Urls.TryParseUrl(EveryPart);

			runs += 2;
		}

		Console.WriteLine($"{runs} parses in {deadline.Elapsed.TotalSeconds:0.0}s.");
	}
}
