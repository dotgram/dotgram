using System;

using BenchmarkDotNet.Running;

namespace DotGram.Benchmarks;

/// <summary>
/// Runs the benchmarks. <c>dotnet run -c Release --project benchmarks/DotGram.Benchmarks</c>.
/// </summary>
/// <remarks>
/// A separate project from the tests, and not run by CI. A number from a shared runner is
/// a number about the runner, and a test suite that fails when a machine is busy is a
/// test suite people learn to ignore.
/// </remarks>
static class Program
{
	static void Main(string[] args)
	{
		// `--depth N` is not a benchmark: it is one run that either prints `ok` or takes
		// the process with it, so that a caller can walk N up and find where nesting stops
		// being possible. See Nesting.cs.
		if (args.Length == 2 && args[0] == "--depth" && int.TryParse(args[1], out var depth))
		{
			Console.WriteLine(Nesting.Reads(depth) ? "ok" : "no match");

			return;
		}

		// `--alloc` is not a benchmark either: it asks the runtime what a parse allocates
		// and prints the answer. See Allocation.cs.
		if (args.Length == 1 && args[0] == "--alloc")
		{
			Allocation.Report();

			return;
		}

		// `--hot [seconds] [input]` is not a benchmark either: it runs the URL grammar in
		// a loop long enough for a profiler to attach to and get a line-by-line breakdown
		// from. Named alone, one input is what a profile about that input has to run. See
		// HotLoop.cs.
		if (args.Length >= 1 && args[0] == "--hot")
		{
			var seconds = args.Length >= 2 && int.TryParse(args[1], out var given) ? given : 10;
			var which   = args.Length >= 3 ? args[2] : "both";

			HotLoop.Run(seconds, which);

			return;
		}

		// `--against [rounds] [iterations]` is not a benchmark either: it measures the URL
		// comparison round-robin instead of one method at a time, so that the ratios hold
		// on a machine that is not idle. See Against.cs.
		if (args.Length >= 1 && args[0] == "--against")
		{
			var rounds     = args.Length >= 2 && int.TryParse(args[1], out var many) ? many : 9;
			var iterations = args.Length >= 3 && int.TryParse(args[2], out var each) ? each : 200_000;

			Against.Run(rounds, iterations);

			return;
		}

		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
	}
}
