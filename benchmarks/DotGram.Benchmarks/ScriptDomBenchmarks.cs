using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram.Sql;
using DotGram.Sql.TransactSql;

using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// <c>TransactSql.gram</c> against Microsoft's ScriptDom, over ScriptDom's own corpus.
/// </summary>
/// <remarks>
/// <para>
/// <c>--speed</c> asks the same question and answers it round-robin, which is the right
/// shape for a machine that is not idle: every method measured once per round, adjacent in
/// time, so whatever the machine does to one it does to the others. What that design cannot
/// give is an absolute number anybody should quote. This is the other half — one case per
/// process, warmed and iterated until the distribution settles, with the confidence
/// intervals printed beside it. Run it on a quiet machine and it is the number for the
/// README; run <c>--speed</c> while working and it is the number for the day.
/// </para>
/// <para>
/// <b>Agreement first.</b> Only the statements both parsers read are timed. A parser that
/// quietly reads a smaller language is faster for a reason that says nothing about how it is
/// built. How many that is is printed by the run and not written here: it was written here
/// once, as 6,861 of 8,397, and the grammar has read more of the corpus every week since.
/// </para>
/// <para>
/// <b>And both build a tree of the whole statement.</b> That was not true until the round
/// trip said so: <c>--roundtrip</c> takes every one of these statements, prints the tree
/// back out, and hands it to ScriptDom, which reads it into the tree it made of the
/// original — all 6865 of them. What is left of the difference is where each part of the
/// statement was. ScriptDom carries that always; here it is asked for, so
/// <see cref="Located"/> is the row to hold against <see cref="Tree"/> and
/// <see cref="Grammar"/> says what dropping it saves.
/// </para>
/// <para>
/// <b>One operation is one statement, and the report says so itself.</b> It used to be the
/// whole corpus with a note to divide by a property BenchmarkDotNet does not print -- so a
/// short <c>SELECT</c> and a three-hundred-line procedure were averaged into one number and
/// nobody, us included, could take them apart again. Now each row reads a sample of
/// <see cref="Sample"/> statements from one length bucket and declares that count as
/// <c>OperationsPerInvoke</c>, which is what makes the printed figure per statement without
/// anybody dividing anything.
/// </para>
/// <para>
/// <b>Three buckets, by the length of the statement</b> -- under 100 characters, under 300,
/// and the rest -- because the mechanism changes with the scale of the input and one average
/// hides which end of the corpus moved. The boundaries are fixed here rather than taken as
/// terciles of the corpus, which would shift whenever it grew and stop two runs being
/// comparable; they are where they are because the corpus says its median statement is 71
/// characters and its 95th percentile 251, which the setup prints on every run so that the
/// next person can see the boundaries stop fitting rather than find out from a bucket that
/// will not fill.
/// </para>
/// <para>
/// <b><see cref="Tokens"/> has no partner, deliberately.</b> It times ScriptDom's lexer
/// alone; ours is internal to the generated parser, and exposing it to make a pair would be
/// a change to what we ship for the sake of a row. So the row stands alone, and this is the
/// sentence that keeps a reader from setting their lexing beside our whole parse.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ScriptDomBenchmarks
{
	/// <summary>One statement, and the parser the corpus says is the one to read it with.</summary>
	public readonly record struct Timed(string Text, TSqlParser By);

	/// <summary>How many statements one row reads, and what its time is divided by.</summary>
	/// <remarks>
	/// A constant, because <c>OperationsPerInvoke</c> is one, and the same for every bucket so
	/// that the three rows are three readings of one measurement rather than three of their own.
	/// </remarks>
	public const int Sample = 200;

	/// <summary>The length buckets, by what a statement's text measures.</summary>
	public static string[] Sizes { get; } = ["short", "medium", "long"];

	[ParamsSource(nameof(Sizes))]
	public string Size { get; set; } = "short";

	/// <summary>What <see cref="TsqlLoop"/> passes to read the corpus whole.</summary>
	internal const string Every = "every";

	Timed[] _sample = null!;

	/// <summary>How many statements one round of this class reads, for a harness that says so.</summary>
	internal int Statements => _sample.Length;

	/// <summary>
	/// Every statement of the corpus both parsers read, cut out by ScriptDom, each with the
	/// parser its file's version names — which is <c>--speed</c>'s corpus exactly, so that
	/// the two harnesses answer one question and not two.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		var root    = Corpus.Checked();
		var files   = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var named   = Corpus.Versions(root, files);
		var readers = new Dictionary<string, TSqlParser>(StringComparer.Ordinal);
		var kept    = new List<Timed>();

		foreach (var file in files)
		{
			var text = File.ReadAllText(file);
			var by   = Kinds.Reader(readers, "170", named.GetValueOrDefault(file));

			using var reader = new StringReader(text);

			if (by.Parse(reader, out var errors) is not TSqlScript script || errors.Count > 0)
				continue;

			foreach (var statement in script.Batches.SelectMany(static batch => batch.Statements))
			{
				var one = text.Substring(statement.StartOffset, statement.FragmentLength).TrimEnd();

				try
				{
					if (TransactSqlParser.TryParseStatement(one).IsSuccess)
						kept.Add(new Timed(one, by));
				}
				catch (Exception)
				{
					// A parser that throws is a defect worth seeing in `--kinds`, which names
					// it. Here it is simply not a statement both of them read.
				}
			}
		}

		// What each side is, in the log a run keeps: the package version read from the assembly
		// that was loaded rather than from the one the project asks for, since those are two facts
		// and only the first is what ran.
		Console.WriteLine(
			"ScriptDom " + Kinds.ScriptDomVersion + ", quoted identifiers on, " +
			kept.Count + " statements both parsers read, " +
			kept.Count(one => Bucket(one.Text.Length) == "short")  + " short, " +
			kept.Count(one => Bucket(one.Text.Length) == "medium") + " medium, " +
			kept.Count(one => Bucket(one.Text.Length) == "long")   + " long");

		// The lengths themselves, since the boundaries above are only as good as the shape they cut.
		var lengths = kept.Select(one => one.Text.Length).OrderBy(one => one).ToArray();

		Console.WriteLine(
			"statement length: p50 " + lengths[lengths.Length / 2] +
			", p75 " + lengths[lengths.Length * 3 / 4] +
			", p90 " + lengths[lengths.Length * 9 / 10] +
			", p95 " + lengths[lengths.Length * 95 / 100] +
			", p99 " + lengths[lengths.Length * 99 / 100] +
			", max " + lengths[lengths.Length - 1]);

		// "every" is not one of the buckets a run reads; it is how TsqlLoop asks for the corpus
		// entire, the way this class read it before the buckets existed.
		Timed[] bucket = Size == Every
			? [.. kept]
			: [.. kept.Where(one => Bucket(one.Text.Length) == Size)];

		if (Size != Every && bucket.Length < Sample)
			throw new InvalidOperationException(
				$"The '{Size}' bucket holds {bucket.Length} statements and a row reads {Sample}. " +
				"Move the boundary or lower the sample; a row that reads one twice is not a row.");

		// Evenly spaced rather than the first N: the corpus is ordered by file, so the first two
		// hundred of a bucket are two hundred statements of whatever that one file was about.
		_sample = Size == Every
			? bucket
			:
			[
				.. Enumerable
					.Range(0, Sample)
					.Select(at => bucket[(int)((long)at * bucket.Length / Sample)]),
			];
	}

	/// <summary>Which bucket a statement of that many characters belongs to.</summary>
	static string Bucket(int length)
	{
		return length < 100 ? "short" : length < 300 ? "medium" : "long";
	}

	/// <summary>ScriptDom's lexer on its own, so that lexing can be told from the rest.</summary>
	[Benchmark(OperationsPerInvoke = Sample)]
	public int Tokens()
	{
		var sink = 0;

		foreach (var (one, by) in _sample)
		{
			using var reader = new StringReader(one);

			sink += by.GetTokenStream(reader, out _).Count;
		}

		return sink;
	}

	[Benchmark(Baseline = true, OperationsPerInvoke = Sample)]
	public int Tree()
	{
		var sink = 0;

		foreach (var (one, by) in _sample)
		{
			using var reader = new StringReader(one);

			sink += by.Parse(reader, out _) is null ? 0 : 1;
		}

		return sink;
	}

	/// <summary>The baseline again, under another name, and it is not a spare row.</summary>
	/// <remarks>
	/// BenchmarkDotNet is strong on an absolute number and weak on a ratio. Whatever this row
	/// reads away from 1.00 is the resolution of every other ratio in the table: a difference
	/// smaller than it is not a difference, and two of ours were paid for this week before that
	/// was written down anywhere.
	/// </remarks>
	[Benchmark(OperationsPerInvoke = Sample, Description = "Tree again (A/A)")]
	public int TreeAgain()
	{
		return Tree();
	}

	/// <summary>The reading that carries where each part of the statement was, as ScriptDom does.</summary>
	[Benchmark(OperationsPerInvoke = Sample)]
	public int Located()
	{
		var sink = 0;

		foreach (var (one, _) in _sample)
			sink += TransactSqlParser.Located.TryParseStatement(one).IsSuccess ? 1 : 0;

		return sink;
	}

	[Benchmark(OperationsPerInvoke = Sample)]
	public int Grammar()
	{
		var sink = 0;

		foreach (var (one, _) in _sample)
			sink += TransactSqlParser.TryParseStatement(one).IsSuccess ? 1 : 0;

		return sink;
	}
}
