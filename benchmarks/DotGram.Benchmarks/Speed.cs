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
/// <b>And what each side builds is the same thing now.</b> It was not: the caveat printed
/// under this table used to say that hints, options and output clauses were read and
/// dropped, and while that was true the ratio measured this grammar building a tenth of a
/// tree against ScriptDom building all of one. It is no longer true, and the oracle is what
/// says so rather than an opinion — every statement of the corpus that both parsers read
/// comes back through <c>--roundtrip</c> as the same statement, which is a claim about the
/// tree and not about the text.
/// </para>
/// <para>
/// What is left of the difference is where each part of the statement was. ScriptDom
/// carries that always; here it is asked for — <c>[Gram(LocationType = …)]</c> — so both
/// readings are timed, and the one to compare against ScriptDom is the one that carries
/// what ScriptDom carries. ScriptDom's lexer is timed on its own beside them, so that the
/// part of the difference which is <em>lexing</em> can be told from the rest.
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

		if (Version(version) is null)
		{
			Console.WriteLine($"No such version as {version}. One of 80 90 100 … 180.");

			return;
		}

		var (kept, all, bytes) = Gathered(root, version);

		if (kept.Count == 0)
		{
			Console.WriteLine("Nothing both parsers read.");

			return;
		}

		var methods = new (string Name, Func<IReadOnlyList<Timed>, int> Measure)[]
		{
			("ScriptDom, tokens",  Tokens),
			("ScriptDom, tree",    Tree),
			(".Gram, located",     Located),
			(".Gram",              Grammar),
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

	/// <summary>One statement, and the parser the corpus says is the one to read it with.</summary>
	readonly record struct Timed(string Text, TSqlParser By);

	static void Report(
		(string Name, Func<IReadOnlyList<Timed>, int> Measure)[] methods,
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
			"  Both build a tree of the whole statement: --roundtrip says every one of these");
		Console.WriteLine(
			"  comes back through ScriptDom as the statement it was read from. What differs is");
		Console.WriteLine(
			"  where each part of it was — ScriptDom carries that always, and here it is asked");
		Console.WriteLine(
			"  for, so the row to hold against ScriptDom's tree is the located one.");
	}

	/// <summary>
	/// Every statement of the corpus both parsers read, and how much text that is.
	/// </summary>
	/// <remarks>
	/// Each file by the parser its version names, capped at the one asked for, which is what
	/// <c>--kinds</c> and <c>--roundtrip</c> do and for the same reason: a file whose syntax
	/// was taken out of the language is read by the parser that still has it, and timing it
	/// with a later one would be timing error recovery.
	/// </remarks>
	static (List<Timed> Kept, int All, long Bytes) Gathered(string root, string version)
	{
		var kept    = new List<Timed>();
		var all     = 0;
		var bytes   = 0L;
		var files   = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var named   = Corpus.Versions(root, files);
		var readers = new Dictionary<string, TSqlParser>(StringComparer.Ordinal);

		foreach (var file in files)
		{
			var text   = File.ReadAllText(file);
			var parser = Kinds.Reader(readers, version, named.GetValueOrDefault(file));

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

				kept.Add(new Timed(one, parser));
				bytes += one.Length;
			}
		}

		return (kept, all, bytes);
	}

	static int Tokens(IReadOnlyList<Timed> statements)
	{
		var sink = 0;

		foreach (var (one, by) in statements)
		{
			using var reader = new StringReader(one);

			sink += by.GetTokenStream(reader, out _).Count;
		}

		return sink;
	}

	static int Tree(IReadOnlyList<Timed> statements)
	{
		var sink = 0;

		foreach (var (one, by) in statements)
		{
			using var reader = new StringReader(one);

			sink += by.Parse(reader, out _) is null ? 0 : 1;
		}

		return sink;
	}

	static int Grammar(IReadOnlyList<Timed> statements)
	{
		var sink = 0;

		foreach (var (one, _) in statements)
			sink += TransactSql.TryParseStatement(one).IsSuccess ? 1 : 0;

		return sink;
	}

	/// <summary>The same reading, with where each part of the statement was.</summary>
	static int Located(IReadOnlyList<Timed> statements)
	{
		var sink = 0;

		foreach (var (one, _) in statements)
			sink += TransactSql.Located.TryParseStatement(one).IsSuccess ? 1 : 0;

		return sink;
	}

	/// <summary>
	/// What the loop costs with no parsing under it, subtracted from every median beside it.
	/// </summary>
	/// <remarks>
	/// A constant added to both sides of a ratio drags the ratio towards one, so a
	/// comparison that leaves it in flatters whichever parser is slower.
	/// </remarks>
	static int Nothing(IReadOnlyList<Timed> statements)
	{
		var sink = 0;

		foreach (var (one, _) in statements)
			sink += one.Length;

		return sink;
	}

	/// <summary>What one pass over every statement allocates.</summary>
	static long Allocated(IReadOnlyList<Timed> statements, Func<IReadOnlyList<Timed>, int> measure)
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
	static double Time(IReadOnlyList<Timed> statements, Func<IReadOnlyList<Timed>, int> measure)
	{
		var watch = Stopwatch.StartNew();
		var sink  = measure(statements);

		watch.Stop();

		_sink = sink;

		return watch.Elapsed.TotalMilliseconds * 1_000_000.0;
	}
}
