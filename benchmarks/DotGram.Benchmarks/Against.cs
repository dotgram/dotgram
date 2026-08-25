using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotGram.Benchmarks;

/// <summary>
/// The URL comparison of <see cref="UrlBenchmarks"/>, measured round-robin instead of one
/// method at a time.
/// </summary>
/// <remarks>
/// <para>
/// Not a replacement for the benchmark, and not more accurate than it. It answers a
/// narrower question the benchmark's own design cannot: what the <i>ratio</i> between two
/// engines is, on a machine that is not idle.
/// </para>
/// <para>
/// BenchmarkDotNet runs each case in a process of its own, one after another. That is the
/// right way to get a trustworthy absolute number, and it means <c>.Gram</c> is measured
/// at one minute and <c>Regex</c> at another. A ratio between them is then only as good as
/// the assumption that nothing about the machine changed in between — and on a developer's
/// machine, where the numbers in <c>benchmarks/README.md</c> come from, something usually
/// did. Two runs during this file's own writing had to be thrown away for exactly that:
/// <c>.Gram</c> and <c>.Gram, every part</c>, which do the same work and had agreed to
/// within 1%, came out 21% apart.
/// </para>
/// <para>
/// Here every method is measured once per round, adjacent in time and in the same process,
/// and the rounds repeat. Whatever the machine does to one measurement it does to the five
/// beside it, so the ratio survives what the absolute number does not. The absolute number
/// is the one to distrust here: read it as scale, and read the benchmark for it.
/// </para>
/// <para>
/// Each method's own spread across rounds is printed beside its median, because a reader
/// cannot otherwise tell a quiet machine from a busy one, and this exists to be run on a
/// busy one.
/// </para>
/// </remarks>
static class Against
{
	/// <summary>Called through <see cref="UrlBenchmarks"/> itself, so the work is the same work.</summary>
	static readonly (string Name, Func<UrlBenchmarks, int> Measure)[] Methods =
	[
		(".Gram",                      benchmark => benchmark.Grammar()?.Length ?? 0),
		("Regex",                      benchmark => benchmark.RegexInterpreted()?.Length ?? 0),
		("Regex, compiled",            benchmark => benchmark.RegexCompiled()?.Length ?? 0),
		(".Gram, every part",          benchmark => benchmark.GrammarEveryPart()),
		("Regex, every part",          benchmark => benchmark.RegexInterpretedEveryPart()),
		("Regex compiled, every part", benchmark => benchmark.RegexCompiledEveryPart()),
	];

	/// <summary>
	/// What the loop and the indirect call cost with no parsing under them, subtracted from
	/// every median beside it.
	/// </summary>
	/// <remarks>
	/// Not a nicety. A constant added to both sides of a ratio drags the ratio towards one,
	/// so a comparison that leaves it in flatters whichever engine is slower. It is measured
	/// in the rotation like everything else and through the same delegate, so what it removes
	/// is what it measured rather than an estimate.
	/// </remarks>
	static readonly Func<UrlBenchmarks, int> Overhead = static _ => 0;

	/// <summary>Kept assigned so that nothing measured here can be optimized away.</summary>
	static volatile int _sink;

	public static void Run(int rounds, int iterations)
	{
		foreach (var input in UrlBenchmarks.Inputs)
		{
			var benchmark = new UrlBenchmarks { Input = input };

			// The same refusal to measure two things that do not do the same work.
			benchmark.CheckTheyAgree();

			var taken = new List<double>[Methods.Length];
			var costs = new List<double>();

			for (var i = 0; i < Methods.Length; i++)
				taken[i] = [];

			// Warm every one of them before any is timed, not each before itself: a method
			// still at tier zero in the middle of a round would be measured against
			// neighbours that were not. Twice and at full size, because what has to be left
			// behind is the tiering, and a short pass does not reach it.
			for (var warm = 0; warm < 2; warm++)
			{
				Time(benchmark, Overhead, iterations);

				for (var i = 0; i < Methods.Length; i++)
					Time(benchmark, Methods[i].Measure, iterations);
			}

			for (var round = 0; round < rounds; round++)
			{
				costs.Add(Time(benchmark, Overhead, iterations));

				for (var i = 0; i < Methods.Length; i++)
					taken[i].Add(Time(benchmark, Methods[i].Measure, iterations));
			}

			Report(input, taken, Median(costs));
		}
	}

	static void Report(string input, IReadOnlyList<List<double>> taken, double overhead)
	{
		var medians  = taken.Select(times => Median(times) - overhead).ToArray();
		var baseline = medians[0];

		Console.WriteLine();
		Console.WriteLine($"== {input} ==");
		Console.WriteLine();
		Console.WriteLine($"   {"",-28} {"median",10} {"ratio",7} {"spread",8}");

		for (var i = 0; i < medians.Length; i++)
		{
			var ordered = taken[i].Order().ToArray();

			// Over the middle of the rounds rather than all of them. One round interrupted
			// by something else on the machine says nothing about the measurement, and a
			// spread that reports it is a spread nobody can read.
			var low  = ordered[ordered.Length / 4];
			var high = ordered[^(1 + ordered.Length / 4)];

			Console.WriteLine(
				$"   {Methods[i].Name,-28} {medians[i],10:F1} {medians[i] / baseline,7:F2} " +
				$"{100.0 * (high - low) / low,7:F1}%");
		}

		Console.WriteLine();
		Console.WriteLine($"   {"(loop and call, removed)",-28} {overhead,10:F1}");
	}

	static double Median(List<double> values)
	{
		var ordered = values.Order().ToArray();

		return ordered.Length % 2 == 1
			? ordered[ordered.Length / 2]
			: (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2;
	}

	/// <summary>Nanoseconds for one call, from a fixed amount of work rather than a fixed time.</summary>
	static double Time(UrlBenchmarks benchmark, Func<UrlBenchmarks, int> measure, int iterations)
	{
		var watch = Stopwatch.StartNew();
		var sink  = 0;

		for (var i = 0; i < iterations; i++)
			sink += measure(benchmark);

		watch.Stop();

		_sink = sink;

		return watch.Elapsed.TotalMilliseconds * 1_000_000.0 / iterations;
	}
}
