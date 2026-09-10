using System;

using BenchmarkDotNet.Running;

using DotGram.Parsers.Sql;

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
		// `--big` is not a benchmark either: it reads one search condition at five sizes,
		// up to a few megabytes, and prints what each parse took and allocated. What it is
		// for is the shape of the curve rather than the numbers — a ratio that holds at
		// sixty-four predicates and does not at a hundred thousand is a different fact
		// about the parser than either measurement alone. See benchmarks/README.md.
		// `--feed` is not a benchmark either: it reads one feed at two sizes and prints what
		// the process held while it did. What it is for is the claim that a streamed parse
		// costs what a record costs rather than what the file does, which is a statement
		// about the difference between two runs and not about either number.
		//
		// The input is made as it is read and never held, so what is measured is the parse
		// and not somebody's ability to produce twenty gigabytes of disk. See
		// benchmarks/README.md.
		if (args.Length >= 1 && args[0] == "--feed")
		{
			var sizes = args.Length > 1
				? args.Skip(1).Select(long.Parse).ToArray()
				: [40_000_000L, 600_000_000L];

			Console.WriteLine($"{"rows",15}  {"read",10}  {"seconds",8}  {"managed peak",13}  {"working set",12}");

			foreach (var rows in sizes)
			{
				var clock = System.Diagnostics.Stopwatch.StartNew();
				var made  = new MadeFeed(rows);
				var seen  = 0L;
				var peak  = 0L;

				foreach (var part in Feed.ParseFeed(made))
				{
					seen++;

					// Sampled rather than watched: asking for it costs more than the parse
					// does at this rate, and the high-water mark is what the claim is about.
					if ((seen & 0xFFFFF) == 0)
						peak = Math.Max(peak, GC.GetTotalMemory(false));
				}

				Console.WriteLine(
					$"{seen,15:N0}  {made.Characters / 1024.0 / 1024.0 / 1024.0,7:N2} GiB  {clock.Elapsed.TotalSeconds,8:N1}  " +
					$"{peak / 1024.0 / 1024.0,9:N1} MiB  {Environment.WorkingSet / 1024.0 / 1024.0,8:N1} MiB");
			}

			return;
		}

		if (args.Length > 0 && args[0] == "--big")
		{
			foreach (var terms in new[] { 1_000, 10_000, 50_000, 100_000, 200_000 })
			{
				var text = string.Join(
					" AND ", Enumerable.Range(0, terms).Select(i => "a" + i + " = " + i));

				Console.WriteLine($"{terms:N0} predicates, {text.Length / 1024.0 / 1024.0:N2} MB of text");

				foreach (var (name, what) in new (string Name, Func<string, bool> What)[]
				{
					("generated", static one => SqlStandard92.TryParseSearchCondition(one).IsSuccess),
					("by hand",   static one => HandSqlTokens.Parse(one)),
				})
				{
					// Warmed, because the first parse on a thread builds the buffers it will
					// then keep, and the best of several, because a collection lands where it
					// lands.
					for (var warm = 0; warm < 3; warm++)
						what(text);

					var best   = double.MaxValue;
					var bytes  = 0L;
					var passes = 0;
					var read   = false;

					for (var round = 0; round < 5; round++)
					{
						var collected = GC.CollectionCount(0);
						var before    = GC.GetAllocatedBytesForCurrentThread();
						var watch     = System.Diagnostics.Stopwatch.StartNew();

						read = what(text);

						watch.Stop();

						if (watch.Elapsed.TotalMilliseconds >= best)
							continue;

						best   = watch.Elapsed.TotalMilliseconds;
						bytes  = GC.GetAllocatedBytesForCurrentThread() - before;
						passes = GC.CollectionCount(0) - collected;
					}

					Console.WriteLine(
						$"  {name,-10} {best,8:N1} ms  {bytes / 1024.0 / 1024.0,8:N1} MB  " +
						$"{passes,3} gen0  {(read ? "read" : "REFUSED")}");
				}
			}

			return;
		}

		// `--bytes [iterations]` is what each parse allocates, which a ratio of times
		// cannot say. See SqlAgainst.Bytes.
		if (args.Length > 0 && args[0] == "--bytes")
		{
			SqlAgainst.Bytes(args.Length > 1 && int.TryParse(args[1], out var runs0) ? runs0 : 2000);

			return;
		}

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
			var immediately = args.Length > 3 && args[3] == "immediate";
			var text    = SqlAgainst.Inputs[which];
			var until   = DateTime.UtcNow.AddSeconds(seconds);
			var read    = 0;

			while (DateTime.UtcNow < until)
				for (var i = 0; i < 2000; i++)
					read += byHand  ? HandSqlTokens.Parse(text) ? 1 : 0 :
					        immediately ? ImmediateSql.TryParseSearchCondition(text).IsSuccess ? 1 : 0 :
					                  SqlStandard92.TryParseSearchCondition(text).IsSuccess ? 1 : 0;

			Console.WriteLine($"{read:N0} parses of \"{text}\"");

			return;
		}

		// `--elspin [seconds] [input] [hand|immediate]` is `--spin` for the expression
		// language: one input read over and over, long enough for a profiler to sample.
		// See ExpressionAgainst.cs for the inputs, by index.
		if (args.Length > 0 && args[0] == "--elspin")
		{
			var seconds = args.Length > 1 && int.TryParse(args[1], out var given) ? given : 20;
			var which   = args.Length > 2 && int.TryParse(args[2], out var index) ? index : 7;

			ExpressionAgainst.Spin(seconds, which, args.Length > 3 ? args[3] : "tape");

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

		// `--slope [parses]` is not a benchmark either, and is the one that answers "why":
		// one shape of predicate at two lengths, timed in a loop per reading, with the
		// difference over the count being what one more of that shape costs. A ratio over a
		// whole input mixes a fixed cost, a shape mixture and a size; a slope is one shape.
		// Run it under DOTNET_TieredCompilation=0 — with tiering on, three hundred warm-up
		// parses still leave part of the reading at tier 0 and the rows move by half between
		// runs. See SqlSlope.cs.
		// `--ladders [parses]` is not a benchmark either: one arithmetic language written
		// twice — a rule a level, and one rule with binding powers (§4.3.1) — over the same
		// inputs, so that what climbing is worth is known before a grammar is rewritten to
		// use it. See Ladders.cs.
		// `--engine [path] [version] [shown]` puts the same corpus to SQL Server itself,
		// which is the only source here that is not somebody's reading of the language.
		// `SET PARSEONLY ON` asks it whether a statement is syntax, compiling nothing and
		// needing no schema. Four cells come out and three of them say something: the work
		// list with an authority behind it, a defect here where this reads what the engine
		// will not, and a finding about ScriptDom where it reads what the engine will not.
		// Needs a server on the machine, and says so where there is none. See Engine.cs.
		// `--levels [path] [shown]` asks every compatibility level from 100 to 170 about every
		// statement and keeps the ones whose answer moves: what a level gates on a server
		// that has one parser. See CompatibilityLevels.cs.
		if (args.Length >= 1 && args[0] == "--levels")
		{
			var rest  = args.Skip(1).ToArray();
			var named = rest.Length >= 1 && (rest[0].Contains('/') || rest[0].Contains('\\'));
			var first = named ? 1 : 0;

			CompatibilityLevels.Run(
				named ? rest[0] : null,
				rest.Length > first && int.TryParse(rest[first], out var few) ? few : 2);

			return;
		}

		// `--syntax sql-docs [output]` gathers every syntax block of Microsoft's T-SQL reference
		// from a clone of MicrosoftDocs/sql-docs into the file beside TransactSql.gram. Not a
		// measurement: the specification the grammar is written from, kept where the grammar
		// is. See SyntaxBlocks.cs.
		if (args.Length >= 2 && args[0] == "--syntax")
		{
			SyntaxBlocks.Run(args[1], args.Length >= 3 ? args[2] : null);
			return;
		}

		if (args.Length >= 1 && args[0] == "--engine")
		{
			var rest  = args.Skip(1).ToArray();
			var named = rest.Length >= 1 && (rest[0].Contains('/') || rest[0].Contains('\\'));
			var first = named ? 1 : 0;

			Engine.Run(
				named ? rest[0] : null,
				rest.Length > first ? rest[first] : "170",
				rest.Length > first + 1 && int.TryParse(rest[first + 1], out var few) ? few : 1);

			return;
		}

		// `--speed [path] [version] [rounds]` is the other half of the comparison `--kinds`
		// makes: not what the two parsers read but how long they take about it, over the
		// statements both of them read and round-robin so that the ratio survives a machine
		// that is not idle. Three rows, because ScriptDom's lexer and ScriptDom's tree are
		// not the same work and neither is what this grammar builds. See Speed.cs.
		// `--roundtrip [path] [version]` asks whether the tree says what the text said: the
		// original through ScriptDom's printer, and the original through this grammar, this
		// writer and then ScriptDom's printer, held against each other. See RoundTrip.cs.
		if (args.Length >= 1 && args[0] == "--roundtrip")
		{
			var rest  = args.Skip(1).ToArray();
			var named = rest.Length >= 1 && (rest[0].Contains('/') || rest[0].Contains('\\'));
			var first = named ? 1 : 0;

			RoundTrip.Run(
				named ? rest[0] : null,
				rest.Length > first ? rest[first] : "180",
				rest.Length > first + 1 ? rest[first + 1] : null,
				rest.Length > first + 2 && int.TryParse(rest[first + 2], out var shown) ? shown : 2);

			return;
		}

		// `--prepare [name] [rounds]` asks what a parse costs before it has read anything:
		// the shortest input each grammar accepts against one worth parsing, and the first
		// call of all beside them. See Preparation.cs.
		if (args.Length >= 1 && args[0] == "--prepare")
		{
			var rest = args.Skip(1).ToArray();

			Preparation.Run(rest.Length >= 1 ? rest[0] : null);

			return;
		}

		if (args.Length >= 1 && args[0] == "--speed")
		{
			var rest  = args.Skip(1).ToArray();
			var named = rest.Length >= 1 && (rest[0].Contains('/') || rest[0].Contains('\\'));
			var first = named ? 1 : 0;

			Speed.Run(
				named ? rest[0] : null,
				rest.Length > first ? rest[first] : "170",
				rest.Length > first + 1 && int.TryParse(rest[first + 1], out var many) ? many : 7);

			return;
		}

		// `--kinds [path] [version] [shown]` is the same corpus read the other way round: it
		// asks ScriptDom to split each file into statements, which is the one thing only a
		// T-SQL parser can do, and tallies what this dialect makes of each kind. What comes
		// out is a work list ordered by how often the corpus needs the thing, and a count of
		// the opposite defect — a statement read here as a query and called something else
		// there. See Kinds.cs.
		if (args.Length >= 1 && args[0] == "--kinds")
		{
			var rest  = args.Skip(1).ToArray();
			var named = rest.Length >= 1 && (rest[0].Contains('/') || rest[0].Contains('\\'));
			var first = named ? 1 : 0;

			Kinds.Run(
				named ? rest[0] : null,
				rest.Length > first ? rest[first] : "170",
				rest.Length > first + 1 && int.TryParse(rest[first + 1], out var few) ? few : 1);

			return;
		}

		// `--corpus [path] [shown]` is not a benchmark at all: it reads somebody else's
		// `.sql` files, cuts the query-shaped statements out of them and says what each
		// parser makes of each, grouping the refusals by what stood where the reading
		// stopped. Every other test here was written beside the grammar it tests; this one
		// was not. Without a path it reads the copy in `tests/Corpus/ScriptDom`, which is
		// ScriptDom's own suite kept byte for byte. See Corpus.cs.
		if (args.Length >= 1 && args[0] == "--corpus")
		{
			var named = args.Length >= 2 && !int.TryParse(args[1], out _);
			var at    = named ? 2 : 1;

			Corpus.Run(
				named ? args[1] : null,
				args.Length > at && int.TryParse(args[at], out var some) ? some : 2);

			return;
		}

		if (args.Length >= 1 && args[0] == "--ladders")
		{
			Ladders.Run(args.Length >= 2 && int.TryParse(args[1], out var over) ? over : 401);

			return;
		}

		if (args.Length >= 1 && args[0] == "--slope")
		{
			SqlSlope.Run(args.Length >= 2 && int.TryParse(args[1], out var reads) ? reads : 201);

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

		// `--el [rounds] [iterations]` is the same for the expression language: the two
		// readings of its grammar against the hand-written parser in HandExpression.cs,
		// round-robin, after checking that all three read the same language. See
		// ExpressionAgainst.cs.
		if (args.Length >= 1 && args[0] == "--el")
		{
			var rounds     = args.Length >= 2 && int.TryParse(args[1], out var turns) ? turns : 7;
			var iterations = args.Length >= 3 && int.TryParse(args[2], out var runs)  ? runs  : 20_000;

			ExpressionAgainst.Run(rounds, iterations);

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
