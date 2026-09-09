using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram.Parsers;

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
/// <b>Agreement first.</b> Only the statements both parsers read are timed, which is 6861
/// of the 8397 ScriptDom finds. A parser that quietly reads a smaller language is faster
/// for a reason that says nothing about how it is built.
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
/// One operation is the whole corpus, because the setup cost of cutting it up dwarfs a
/// single statement and BenchmarkDotNet may not be told a count it learns at run time.
/// Divide by <see cref="Statements"/> for the per-statement figure the README prints.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ScriptDomBenchmarks
{
	/// <summary>One statement, and the parser the corpus says is the one to read it with.</summary>
	public readonly record struct Timed(string Text, TSqlParser By);

	Timed[] _kept = null!;

	/// <summary>How many statements one operation reads, for dividing the result by.</summary>
	public int Statements => _kept.Length;

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
					if (TransactSql.TryParseStatement(one).IsSuccess)
						kept.Add(new Timed(one, by));
				}
				catch (Exception)
				{
					// A parser that throws is a defect worth seeing in `--kinds`, which names
					// it. Here it is simply not a statement both of them read.
				}
			}
		}

		_kept = [.. kept];
	}

	/// <summary>ScriptDom's lexer on its own, so that lexing can be told from the rest.</summary>
	[Benchmark]
	public int Tokens()
	{
		var sink = 0;

		foreach (var (one, by) in _kept)
		{
			using var reader = new StringReader(one);

			sink += by.GetTokenStream(reader, out _).Count;
		}

		return sink;
	}

	[Benchmark(Baseline = true)]
	public int Tree()
	{
		var sink = 0;

		foreach (var (one, by) in _kept)
		{
			using var reader = new StringReader(one);

			sink += by.Parse(reader, out _) is null ? 0 : 1;
		}

		return sink;
	}

	/// <summary>The reading that carries where each part of the statement was, as ScriptDom does.</summary>
	[Benchmark]
	public int Located()
	{
		var sink = 0;

		foreach (var (one, _) in _kept)
			sink += TransactSql.Located.TryParseStatement(one).IsSuccess ? 1 : 0;

		return sink;
	}

	[Benchmark]
	public int Grammar()
	{
		var sink = 0;

		foreach (var (one, _) in _kept)
			sink += TransactSql.TryParseStatement(one).IsSuccess ? 1 : 0;

		return sink;
	}
}
