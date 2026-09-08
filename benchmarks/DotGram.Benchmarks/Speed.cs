using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DotGram.Parsers;

using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// This grammar against Microsoft's ScriptDom, in time rather than in coverage.
/// </summary>
/// <remarks>
/// <para>
/// <c>--kinds</c> asks the two parsers what they read; this asks how long they take about
/// it. Round-robin for <see cref="Against"/>'s reason: a ratio between two numbers taken a
/// minute apart is only as good as the machine having stayed the same, and on a
/// developer's machine it does not.
/// </para>
/// <para>
/// <b>Agreement first.</b> Only the statements both parsers read are timed. A parser that
/// quietly reads a smaller language is faster for a reason that says nothing about how it
/// is built, and this grammar reads three quarters of the corpus — so timing it over the
/// whole of it would be timing three quarters of the work against all of it. The count of
/// what was kept is printed, because that is the caveat and it belongs beside the number
/// rather than under it.
/// </para>
/// <para>
/// <b>And what each side builds is not the same thing.</b> ScriptDom returns a complete
/// syntax tree: every clause a node, every node carrying its first and last token, and the
/// token stream itself kept beside it — enough to print the statement back out and to say
/// where in the text each part of it was. This grammar returns <c>SqlNode</c>, which is
/// the standard's shape and about a tenth of that: hints, options and output clauses are
/// read and dropped, and nothing carries a position. Three rows are printed for that
/// reason — ScriptDom's lexer alone, ScriptDom whole, and this — so that the part of the
/// difference which is <em>lexing</em> can be told from the part which is building a tree
/// nobody asked for.
/// </para>
/// </remarks>
static class Speed
{
	/// <summary>Kept assigned so that nothing measured here can be optimized away.</summary>
	static volatile int _sink;

	public static void Run(string? root, string version, int rounds)
	{
		root ??= Corpus.Checked();

		if (!Directory.Exists(root))
		{
			Console.WriteLine($"No corpus at {root}.");

			return;
		}

		if (Version(version) is not { } parser)
		{
			Console.WriteLine($"No such version as {version}. One of 80 90 100 … 180.");

			return;
		}

		var (kept, all, bytes) = Gathered(root, parser);

		if (kept.Count == 0)
		{
			Console.WriteLine("Nothing both parsers read.");

			return;
		}

		var methods = new (string Name, Func<IReadOnlyList<string>, int> Measure)[]
		{
			("ScriptDom, tokens", statements => Tokens(parser, statements)),
			("ScriptDom, tree",   statements => Tree(parser, statements)),
			(".Gram",             Grammar),
		};

		var taken = new List<double>[methods.Length];
		var costs = new List<double>();

		for (var i = 0; i < methods.Length; i++)
			taken[i] = [];

		// Warm every one of them before any is timed, not each before itself: a method still
		// at tier zero in the middle of a round would be measured against neighbours that
		// were not.
		for (var warm = 0; warm < 2; warm++)
		{
			Time(kept, Nothing);

			foreach (var method in methods)
				Time(kept, method.Measure);
		}

		for (var round = 0; round < rounds; round++)
		{
			costs.Add(Time(kept, Nothing));

			for (var i = 0; i < methods.Length; i++)
				taken[i].Add(Time(kept, methods[i].Measure));
		}

		// What each pass allocates, taken once and outside the timing: a parser's answer is
		// as much a thing it made as a thing it did, and the ratio of times says nothing
		// about the ratio of garbage.
		var made = new long[methods.Length];

		for (var i = 0; i < methods.Length; i++)
			made[i] = Allocated(kept, methods[i].Measure) - Allocated(kept, Nothing);

		Report(methods, taken, made, Median(costs), kept.Count, all, bytes, version, rounds);
	}

	static void Report(
		(string Name, Func<IReadOnlyList<string>, int> Measure)[] methods,
		IReadOnlyList<List<double>> taken,
		IReadOnlyList<long> made,
		double overhead,
		int kept,
		int all,
		long bytes,
		string version,
		int rounds)
	{
		var medians = taken.Select(times => Median(times) - overhead).ToArray();
		var whole   = medians[1];

		Console.WriteLine();
		Console.WriteLine($"against ScriptDom, TSql{version}Parser, {rounds} rounds");
		Console.WriteLine();
		Console.WriteLine(
			$"  {kept} of {all} statements — the ones both parsers read, which is what may be timed");
		Console.WriteLine($"  {bytes / 1024.0:F0} KB of T-SQL a round");
		Console.WriteLine();
		Console.WriteLine(
			$"  {"",-20}{"per statement",15}{"MB/s",9}{"ratio",8}{"spread",9}{"allocated",13}");
		Console.WriteLine($"  {new string('-', 20 + 15 + 9 + 8 + 9 + 13)}");

		for (var i = 0; i < medians.Length; i++)
		{
			var ordered = taken[i].Order().ToArray();
			var low     = ordered[ordered.Length / 4];
			var high    = ordered[^(1 + ordered.Length / 4)];

			Console.WriteLine(
				$"  {methods[i].Name,-20}{medians[i] / kept,12:F0} ns" +
				$"{bytes / (medians[i] / 1_000_000_000.0) / 1_048_576.0,9:F1}" +
				$"{whole / medians[i],8:F2}" +
				$"{100.0 * (high - low) / low,7:F1}%" +
				$"{made[i] / (double)kept,10:F0} B");
		}

		Console.WriteLine();
		Console.WriteLine($"  {"(loop, removed)",-20}{overhead / kept,12:F0} ns");
		Console.WriteLine();
		Console.WriteLine(
			"  ScriptDom returns every clause as a node, each carrying its first and last");
		Console.WriteLine(
			"  token, with the token stream kept beside it. This returns the standard's shape:");
		Console.WriteLine(
			"  hints, options and output clauses are read and dropped, and nothing carries a");
		Console.WriteLine(
			"  position. The two are not the same answer, and the ratio is not a like for like.");
	}

	/// <summary>
	/// Every statement of the corpus both parsers read, and how much text that is.
	/// </summary>
	static (List<string> Kept, int All, long Bytes) Gathered(string root, TSqlParser parser)
	{
		var kept  = new List<string>();
		var all   = 0;
		var bytes = 0L;

		foreach (var file in Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories))
		{
			var text = File.ReadAllText(file);

			using var reader = new StringReader(text);

			if (parser.Parse(reader, out var errors) is not TSqlScript script || errors.Count > 0)
				continue;

			foreach (var statement in script.Batches.SelectMany(static batch => batch.Statements))
			{
				all++;

				// The fragment carries its terminator, and it is kept: `A MERGE statement
				// must be terminated by a semi-colon` is the engine's own answer, so the
				// separator is part of the statement for at least one of them.
				var one = text.Substring(statement.StartOffset, statement.FragmentLength).TrimEnd();

				try
				{
					if (!TransactSql.TryParseStatement(one).IsSuccess)
						continue;
				}
				catch (Exception)
				{
					continue;
				}

				kept.Add(one);
				bytes += one.Length;
			}
		}

		return (kept, all, bytes);
	}

	static int Tokens(TSqlParser parser, IReadOnlyList<string> statements)
	{
		var sink = 0;

		foreach (var one in statements)
		{
			using var reader = new StringReader(one);

			sink += parser.GetTokenStream(reader, out _).Count;
		}

		return sink;
	}

	static int Tree(TSqlParser parser, IReadOnlyList<string> statements)
	{
		var sink = 0;

		foreach (var one in statements)
		{
			using var reader = new StringReader(one);

			sink += parser.Parse(reader, out _) is null ? 0 : 1;
		}

		return sink;
	}

	static int Grammar(IReadOnlyList<string> statements)
	{
		var sink = 0;

		foreach (var one in statements)
			sink += TransactSql.TryParseStatement(one).IsSuccess ? 1 : 0;

		return sink;
	}

	/// <summary>
	/// What the loop costs with no parsing under it, subtracted from every median beside it.
	/// </summary>
	/// <remarks>
	/// A constant added to both sides of a ratio drags the ratio towards one, so a
	/// comparison that leaves it in flatters whichever parser is slower.
	/// </remarks>
	static int Nothing(IReadOnlyList<string> statements)
	{
		var sink = 0;

		foreach (var one in statements)
			sink += one.Length;

		return sink;
	}

	/// <summary>What one pass over every statement allocates.</summary>
	static long Allocated(IReadOnlyList<string> statements, Func<IReadOnlyList<string>, int> measure)
	{
		// Twice, and the second is the one reported: the first pass through a delegate this
		// process has not called yet allocates the machinery for calling it.
		measure(statements);

		var before = GC.GetAllocatedBytesForCurrentThread();

		_sink = measure(statements);

		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	static TSqlParser? Version(string named) => Kinds.Version(named);

	static double Median(List<double> values)
	{
		var ordered = values.Order().ToArray();

		return ordered.Length % 2 == 1
			? ordered[ordered.Length / 2]
			: (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2;
	}

	/// <summary>Milliseconds for one pass over every statement.</summary>
	static double Time(IReadOnlyList<string> statements, Func<IReadOnlyList<string>, int> measure)
	{
		var watch = Stopwatch.StartNew();
		var sink  = measure(statements);

		watch.Stop();

		_sink = sink;

		return watch.Elapsed.TotalMilliseconds * 1_000_000.0;
	}
}
