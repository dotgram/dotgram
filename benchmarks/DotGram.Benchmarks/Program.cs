using System;

using BenchmarkDotNet.Running;

using DotGram.Parsers;

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
		// `--lexers [rounds] [iterations]` is the two lexers alone, the generated one
		// measured by refusing the parse at its first token. See SqlAgainst.Lexers.
		if (args.Length > 0 && args[0] == "--lexers")
		{
			SqlAgainst.Lexers(
				args.Length > 1 && int.TryParse(args[1], out var rounds) ? rounds : 7,
				args.Length > 2 && int.TryParse(args[2], out var runs) ? runs : 300_000);

			return;
		}

		// `--spin [seconds] [input] [hand]` is not a benchmark either: it reads one SQL
		// input over and over, long enough for a profiler to attach and sample. Which
		// input is an index into SqlAgainst.Inputs, and `hand` runs the hand-written
		// parser instead of the generated one, so the two profiles can be read against
		// each other — where the generated one spends time the other has no line for is
		// where the generator's own machinery is.
		if (args.Length > 0 && args[0] == "--spin")
		{
			var seconds = args.Length > 1 && int.TryParse(args[1], out var given) ? given : 20;
			var which   = args.Length > 2 && int.TryParse(args[2], out var index) ? index : 4;
			var byHand  = args.Length > 3 && args[3] == "hand";
			var text    = SqlAgainst.Inputs[which];
			var until   = DateTime.UtcNow.AddSeconds(seconds);
			var read    = 0;

			while (DateTime.UtcNow < until)
				for (var i = 0; i < 2000; i++)
					read += byHand
						? HandSqlTokens.Parse(text) ? 1 : 0
						: SqlStandard92.TryParseSearchCondition(text).IsSuccess ? 1 : 0;

			Console.WriteLine($"{read:N0} parses of \"{text}\"");

			return;
		}

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

		// `--hand [rounds] [iterations]` is not a benchmark either: it measures the SQL
		// recognizer against the hand-written one in HandSqlTokens.cs, round-robin, after
		// checking that the two read the same language. See SqlAgainst.cs.
		if (args.Length >= 1 && args[0] == "--hand")
		{
			var rounds     = args.Length >= 2 && int.TryParse(args[1], out var turns) ? turns : 7;
			var iterations = args.Length >= 3 && int.TryParse(args[2], out var runs)  ? runs  : 20_000;

			SqlAgainst.Run(rounds, iterations);

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
