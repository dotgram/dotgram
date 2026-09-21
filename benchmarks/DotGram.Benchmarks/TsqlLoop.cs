using System;
using System.Diagnostics;
using System.Globalization;

namespace DotGram.Benchmarks;

/// <summary>
/// The corpus of <see cref="ScriptDomBenchmarks"/> read over and over by one T-SQL reading alone, so
/// that a profiler sees that reading and nothing else:
/// <c>tsql-loop plain|located [every|short|medium|long] [rounds]</c>.
/// </summary>
/// <remarks>
/// The word has no dashes because dotTrace takes an argument that begins with one for its own; start
/// it on the apphost by full path (.claude/rules/profiling.md §4). The corpus is cut out by ScriptDom
/// first, which is in the profile as setup and outside the timed rounds; the rounds are warmed, the
/// heap collected, and then timed with what they allocated.
/// </remarks>
static class TsqlLoop
{
	public static void Run(string which, string size, int rounds)
	{
		var benchmark = new ScriptDomBenchmarks { Size = size };

		benchmark.Setup();

		Func<int> read = which switch
		{
			"plain"   => benchmark.Grammar,
			"located" => benchmark.Located,
			_         => throw new ArgumentException($"'{which}' is not plain or located."),
		};

		var sink = 0;

		for (var round = 0; round < 20; round++)
			sink += read();

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var allocated = GC.GetTotalAllocatedBytes(true);
		var pause     = GC.GetTotalPauseDuration();
		var counts    = (GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
		var watch     = Stopwatch.StartNew();

		for (var round = 0; round < rounds; round++)
			sink += read();

		watch.Stop();

		allocated = GC.GetTotalAllocatedBytes(true) - allocated;
		pause     = GC.GetTotalPauseDuration() - pause;

		Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
			$"{which} {size}: {rounds} rounds of {benchmark.Statements} statements in {watch.Elapsed.TotalSeconds:F2} s, {watch.Elapsed.TotalMilliseconds / rounds:F2} ms a round, {allocated / (double)rounds / 1048576:F2} MB a round; " +
			$"collections gen0 {GC.CollectionCount(0) - counts.Item1}, gen1 {GC.CollectionCount(1) - counts.Item2}, gen2 {GC.CollectionCount(2) - counts.Item3}, paused {pause.TotalMilliseconds:F0} ms ({pause.TotalMilliseconds / watch.Elapsed.TotalMilliseconds:P1}) (sink {sink})"));
	}
}
