using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// The generated SQL recognizer against <see cref="HandSqlTokens"/>, measured round-robin.
/// </summary>
/// <remarks>
/// <para>
/// The ratio this prints is the one every claim in <c>docs/next.md</c> about the generated
/// parser is stated in. It was measured for a week from a program outside the repository,
/// and then the directory holding it was cleared; this is that program, put where the
/// solution builds it.
/// </para>
/// <para>
/// Round-robin and not one method at a time, for <see cref="Against"/>'s reason: a ratio
/// between two numbers taken a minute apart is only as good as the machine having stayed
/// the same, and on a developer's machine it does not. Here the two are measured adjacent
/// in time, in one process, and the rounds repeat.
/// </para>
/// <para>
/// <b>Agreement first.</b> A hand-written parser that quietly reads a smaller language is
/// faster for a reason that says nothing about how the generated one is built, so nothing
/// is measured until the two have answered the same on every input — the benchmark inputs
/// and the corpus of shapes below, which is the test suite's, refusals included. The first
/// day's parser is the exception and is shown as one: it is held only to the benchmark
/// inputs, and where it parts from the language is printed rather than thrown.
/// </para>
/// </remarks>
static class SqlAgainst
{
	public static readonly string[] Inputs =
	[
		"a = 1",
		"(a + b) * c > d",
		"((((a + 1) * 2) - 3) / 4) + b > 0",
		"x = 1 AND y IS NOT NULL",
		string.Join(" AND ", Enumerable.Range(0, 64).Select(i => "a" + i + " = 1")),
		string.Join(" + ", Enumerable.Range(0, 64).Select(i => "a" + i)) + " > 0",
		"(a + b) * c >",
	];

	/// <summary>
	/// Shapes the two must agree on before either is timed. The accepted half is the test
	/// suite's own corpus; the refused half is there because agreeing about what matches
	/// is only half of reading the same language.
	/// </summary>
	static readonly string[] Corpus =
	[
		"a = 1",
		"salary BETWEEN 1000 AND 2000",
		"name LIKE 'A%' ESCAPE '\\'",
		"x IN (1, 2, 3) AND y IS NOT NULL",
		"(a + b) * c - d / e > f AND NOT g < h",
		"CAST(x AS INTEGER) = 5 OR SUBSTRING(s FROM 1 FOR 3) = 'abc'",
		"amount * 1.05 + tax >= total AND status <> 'CLOSED' AND created IS NOT NULL",
		"warehouse.zip_code = 'X' AND vendor_key IS NOT NULL AND quota > 0",
		"((((a + 1) * 2) - 3) / 4) + b > 0",
		"EXTRACT(YEAR FROM created) = 2020",
		"COALESCE(a, b, c) IS NOT NULL AND NULLIF(d, 0) > 1",
		"AVG(x) > 1 AND COUNT(*) < 100 AND SUM(DISTINCT y) = 0",
		"CAST(x AS NUMERIC(10, 2)) > 0",
		"CAST(x AS VARCHAR(20)) = 'a'",
		"CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'small' ELSE 'none' END = label",
		"CASE a WHEN 1 THEN 2 WHEN 3 THEN 4 END = label",
		"a IS TRUE AND b IS NOT FALSE",
		"x NOT BETWEEN 1 AND 2",
		"x NOT IN (1, 2)",
		"a = 1 -- a comment\n  AND b = 2",
		"a /* and one of the other kind */ = 1",
		"\"quoted name\" = 1",
		"1E5 > 1.5 AND .5 < 2.",
		"a = ",
		"BETWEEN 1 AND 2",
		"a AND",
		"(a + b",
		"a = 1 OR",
		"x IN",
		"a + b * c",
		"CAST(x AS INTEGER)",
		"'a' || 'b'",
		"",
		"()",
		"a = = 1",
	];

	/// <summary>Kept assigned so that nothing measured here can be optimized away.</summary>
	static volatile int _sink;

	public static void Run(int rounds, int iterations)
	{
		Agree();

		Console.WriteLine();
		Console.Write($"{"",-36}");

		foreach (var (name, _) in Methods)
			Console.Write($" {name,11}");

		// Both readings of the grammar against the hand-written parser: what the tape costs
		// over it, and what building as you read costs over it.
		Console.WriteLine("   tape/hand  eager/hand");

		foreach (var input in Inputs)
		{
			var taken = new List<double>[Methods.Length];

			for (var i = 0; i < Methods.Length; i++)
				taken[i] = [];

			var costs = new List<double>();

			// Warmed together and to full size, so that none is measured at tier zero
			// beside a neighbour that is not.
			for (var warm = 0; warm < 2; warm++)
			{
				Time(input, Nothing, iterations);

				foreach (var (_, measure) in Methods)
					Time(input, measure, iterations);
			}

			for (var round = 0; round < rounds; round++)
			{
				costs.Add(Time(input, Nothing, iterations));

				for (var i = 0; i < Methods.Length; i++)
					taken[i].Add(Time(input, Methods[i].Measure, iterations));
			}

			var overhead = Median(costs);

			Report(input, [.. taken.Select(times => Median(times) - overhead)]);
		}
	}

	/// <summary>
	/// The two readings of the full language, the hand-written lexer alone, and the first
	/// day's parser.
	/// </summary>
	/// <remarks>
	/// The first two are the comparison: both tokenize, read tokens and build the same
	/// tree, so what is between them is the reader and the building. The third is there so
	/// the lexer's share can be taken off both. The fourth reads a fraction of the
	/// language and builds nothing — it is what the first day's ratios were divided by,
	/// kept so a reader can see what they were made of, and it is not a yardstick for
	/// anything measured now.
	/// </remarks>
	static readonly (string Name, Func<string, int> Measure)[] Methods =
	[
		("generated", static input => SqlStandard92.TryParseSearchCondition(input).IsSuccess ? 1 : 0),
		("eager",     static input => EagerSql.TryParseSearchCondition(input).IsSuccess ? 1 : 0),
		("by hand",   static input => HandSqlTokens.Parse(input) ? 1 : 0),
		("its lexer", static input => HandSqlTokens.LexOnly(input)),
		("day one",   static input => HandSqlOriginal.Parse(input) ? 1 : 0),
	];

	/// <summary>
	/// The two answering the same about every shape, before anything is timed. Throws
	/// rather than warns: a ratio measured against a parser that reads a different
	/// language is worse than no ratio.
	/// </summary>
	public static void Agree()
	{
		var parted = new List<string>();

		foreach (var text in Corpus.Concat(Inputs))
		{
			var made      = SqlStandard92.TryParseSearchCondition(text);
			var built     = HandSqlTokens.Build(text);
			var generated = made.IsSuccess;
			var handed    = built is not null;
			var original  = HandSqlOriginal.Parse(text);

			if (generated != handed)
			{
				throw new InvalidOperationException(
					$"About \"{text}\": the generated parser says {Said(generated)} and the hand-written " +
					$"one says {Said(handed)}. They do not read the same language, and a ratio between " +
					"them would be a fiction.");
			}

			// And the same tree, not merely the same answer: both build now, and building
			// is most of what is being measured.
			if (generated && SqlTree.Show(made.Value) is var one && SqlTree.Show(built) is var other &&
				one != other)
			{
				throw new InvalidOperationException(
					$"About \"{text}\": the two parsers read it the same and build it differently.\n" +
					$"  generated {one}\n" +
					$"  by hand   {other}");
			}

			// And the eager carrier: the same grammar, the same tree, built as it is read.
			var eager = EagerSql.TryParseSearchCondition(text);

			if (eager.IsSuccess != generated)
				throw new InvalidOperationException(
					$"About \"{text}\": the tape says {Said(generated)} and the eager carrier says " +
					$"{Said(eager.IsSuccess)}.");

			if (generated && SqlTree.Show(made.Value) != SqlTree.Show(eager.Value))
				throw new InvalidOperationException(
					$"About \"{text}\": the two carriers read it the same and build it differently.\n" +
					$"  tape  {SqlTree.Show(made.Value)}\n" +
					$"  eager {SqlTree.Show(eager.Value)}");

			// The first day's parser is held only to what it was ever checked against — the
			// benchmark inputs — and its departures over the corpus are shown, because they
			// are what the old ratio was made of.
			if (original != generated)
			{
				if (Array.IndexOf(Inputs, text) >= 0)
					throw new InvalidOperationException(
						$"The first day's parser says {Said(original)} about the benchmark input \"{text}\" " +
						$"and the generated one says {Said(generated)}; it did not on the day.");

				parted.Add($"  {Said(original),-3} instead of {Said(generated),-3} about \"{text.Replace('\n', ' ')}\"");
			}
		}

		Console.WriteLine(
			$"Both read the same language and build the same tree over {Corpus.Length + Inputs.Length} shapes.");
		Console.WriteLine($"The first day's parser reads the {Inputs.Length} benchmark inputs and parts from them on {parted.Count}:");

		foreach (var line in parted)
			Console.WriteLine(line);

		static string Said(bool yes) => yes ? "yes" : "no";
	}

	/// <summary>
	/// The two lexers alone, which the generated one can be asked for after all.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A generated parser tokenizes the whole input before it reads a token of it, and it
	/// does so whether or not the reading gets anywhere. So an input the reader refuses at
	/// its first token costs the lexer and nothing else — put a <c>)</c> in front, where a
	/// search condition cannot begin, and what is left over the same input without it is
	/// the tokenizing.
	/// </para>
	/// <para>
	/// Which retires the assumption the totals used to rest on. The reader's share was had
	/// by subtracting the hand-written lexer from both sides and supposing two lexers doing
	/// the same work cost about the same. They do not, and now the numbers say by how much.
	/// </para>
	/// </remarks>
	public static void Lexers(int rounds, int iterations)
	{
		Agree();

		Console.WriteLine();
		Console.WriteLine($"{"",-36} {"generated",11} {"by hand",11}   ratio");

		foreach (var input in Inputs)
		{
			// The `)` is refused at the first token, so what a parse of it costs is the
			// entry and one refusal — the same for every input, and subtracted from both.
			var refused = ")" + input;
			var alone   = ")";

			var taken = new List<double>[4];

			for (var i = 0; i < taken.Length; i++)
				taken[i] = [];

			var measures = new Func<string, int>[]
			{
				static one => SqlStandard92.TryParseSearchCondition(one).IsSuccess ? 1 : 0,
				static one => HandSqlTokens.LexOnly(one),
			};

			for (var warm = 0; warm < 2; warm++)
				foreach (var measure in measures)
				{
					Time(refused, measure, iterations);
					Time(alone, measure, iterations);
				}

			for (var round = 0; round < rounds; round++)
				for (var i = 0; i < measures.Length; i++)
				{
					taken[i * 2].Add(Time(refused, measures[i], iterations));
					taken[i * 2 + 1].Add(Time(alone, measures[i], iterations));
				}

			var made = Median(taken[0]) - Median(taken[1]);
			var hand = Median(taken[2]) - Median(taken[3]);
			var shown = input.Length <= 34 ? input : input.Substring(0, 31) + "...";

			Console.WriteLine($"{shown,-36} {made,8:N1} ns {hand,8:N1} ns   {made / hand,5:N2}x");
		}
	}

	/// <summary>What the loop and the indirect call cost with no parsing under them.</summary>
	static int Nothing(string input) => input.Length & 1;

	static double Time(string input, Func<string, int> measure, int iterations)
	{
		var watch = Stopwatch.StartNew();
		var sink  = 0;

		for (var i = 0; i < iterations; i++)
			sink += measure(input);

		watch.Stop();
		_sink = sink;

		return watch.Elapsed.TotalMilliseconds * 1e6 / iterations;
	}

	static void Report(string input, IReadOnlyList<double> medians)
	{
		var shown = input.Length <= 34 ? input : input.Substring(0, 31) + "...";

		Console.Write($"{shown,-36}");

		foreach (var median in medians)
			Console.Write($" {median,8:N1} ns");

		Console.WriteLine($"   {medians[0] / medians[2],8:N2}x {medians[1] / medians[2],9:N2}x");
	}

	static double Median(List<double> times)
	{
		var ordered = times.Order().ToArray();

		return ordered.Length % 2 == 1
			? ordered[ordered.Length / 2]
			: (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2;
	}
}
