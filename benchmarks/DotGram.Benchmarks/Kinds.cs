using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Sql;
using DotGram.Sql.TransactSql;

using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// What ScriptDom and this repository's dialect make of the same statements.
/// </summary>
/// <remarks>
/// <para>
/// <c>--corpus</c> reads query-shaped fragments cut out by hand, because splitting T-SQL
/// properly needs a T-SQL parser. This has one. ScriptDom is asked to read each file and
/// hand back its statements, and every statement it hands back is one it understood — so
/// the denominator here is not a guess about where a statement ended, and every row of the
/// table is a thing somebody's parser has a name for.
/// </para>
/// <para>
/// <b>What the numbers are and are not.</b> The share is what fraction of the statements of
/// one kind this dialect reads, and today it reads queries only, so most rows are zero and
/// are meant to be: the table is a work list ordered by how often the corpus needs the
/// thing. It says nothing about speed and nothing about the trees either side builds —
/// ScriptDom builds a full AST with token positions and error recovery, and this grammar
/// builds what the round trip proves it builds, with its positions asked for rather than
/// always there. Those are separate questions and get separate harnesses.
/// </para>
/// <para>
/// The other direction is the one nothing else asks: a statement ScriptDom calls something
/// other than a query, read here as a query. That is over-acceptance, it is a defect on
/// this side whatever the corpus says, and it is counted at the bottom.
/// </para>
/// </remarks>
static class Kinds
{
	/// <summary>
	/// Microsoft's parser per version of the language, which is also the partition the
	/// corpus's own directory names use.
	/// </summary>
	/// <remarks>
	/// Flat, and that is Microsoft's arrangement rather than an accident: every one of
	/// these derives from <c>TSqlParser</c> and carries a whole grammar of its own, where
	/// the twelve differ by what one version added to the one before it.
	/// </remarks>
	internal static TSqlParser? Version(string named) => named switch
	{
		"80"  => new TSql80Parser (true),
		"90"  => new TSql90Parser (true),
		"100" => new TSql100Parser(true),
		"110" => new TSql110Parser(true),
		"120" => new TSql120Parser(true),
		"130" => new TSql130Parser(true),
		"140" => new TSql140Parser(true),
		"150" => new TSql150Parser(true),
		"160" => new TSql160Parser(true),
		"170" => new TSql170Parser(true),
		"180" => new TSql180Parser(true),
		_     => null,
	};

	/// <summary>The parser for one file: the version the corpus gives it, never a later one than asked for.</summary>
	/// <remarks>
	/// The argument is a ceiling and not a choice. <c>--kinds 130</c> still means "the corpus
	/// as of 130", so a file the corpus marks 180 is refused there as it was before; what
	/// changes is that a file marked 80 is read by the parser that can read it rather than by
	/// one from which its syntax was removed.
	/// </remarks>
	internal static TSqlParser Reader(
		Dictionary<string, TSqlParser> made, string ceiling, string? version)
	{
		var named = version is not null && int.Parse(version) < int.Parse(ceiling) ? version : ceiling;

		if (!made.TryGetValue(named, out var parser))
			made[named] = parser = Version(named)!;

		return parser;
	}

	// There was a catalogue here of the kinds this grammar had a rule for, and `--engine` and
	// this report asked about those alone. The grammar reads every kind of statement the
	// corpus has now, so the catalogue was a list of what had been done by one date — and it
	// hid from `--engine`'s work list every statement of a kind it did not name: `SHUTDOWN`,
	// `SEND ON CONVERSATION`, `CREATE SYNONYM`, `SET DISABLE_DEF_CNST_CHK ON`, which the
	// engine reads and this grammar did not, were never counted. Every statement is asked
	// about now, 2026-09-13.

	public static void Run(string? root, string version, int shown)
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

		var files    = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var versions = Corpus.Versions(root, files);
		var readers  = new Dictionary<string, TSqlParser>(StringComparer.Ordinal);
		var whole    = 0;
		var refused  = 0;
		var counted  = new Dictionary<string, (int Total, int Read)>(StringComparer.Ordinal);
		var missed   = new Dictionary<string, List<string>>(StringComparer.Ordinal);
		var stopped  = new Dictionary<string, (int Count, List<string> Like)>(StringComparer.Ordinal);

		foreach (var file in files)
		{
			var text = File.ReadAllText(file);

			using var reader = new StringReader(text);

			var parser = Reader(readers, version, versions.GetValueOrDefault(file));
			var read   = parser.Parse(reader, out var errors);

			// A file ScriptDom cannot read whole is left out rather than read in part: what
			// makes this table worth anything is that its denominator is somebody else's
			// certainty about where a statement begins and ends.
			if (errors.Count > 0 || read is not TSqlScript script)
			{
				refused++;

				continue;
			}

			whole++;

			foreach (var statement in script.Batches.SelectMany(static batch => batch.Statements))
			{
				var kind = statement.GetType().Name;

				// The fragment carries its terminator, and it is kept: `A MERGE statement must
				// be terminated by a semi-colon` is the engine's own answer, so the separator
				// is part of the statement for at least one of them and the grammar reads it.
				var one  = text.Substring(statement.StartOffset, statement.FragmentLength).TrimEnd();

				// Caught and named rather than thrown: a harness that dies on statement three
				// thousand says less than one that reads the rest and reports what it hit,
				// and a parser that throws is a defect worth seeing beside the ones it
				// merely refuses.
				bool ours;

				try
				{
					ours = TransactSqlParser.TryParseStatement(one).IsSuccess;
				}
				catch (Exception thrown)
				{
					Console.WriteLine($"threw {thrown.GetType().Name}: {Corpus.One(one)}");

					ours = false;
				}

				var (total, already) = counted.TryGetValue(kind, out var seen) ? seen : (0, 0);

				counted[kind] = (total + 1, already + (ours ? 1 : 0));

				if (!ours)
				{
					if (!missed.TryGetValue(kind, out var some))
						missed[kind] = some = [];

					if (some.Count < shown)
						some.Add(Corpus.One(one));

					// And where it stopped — which names the feature, where the kind only
					// names the statement.
					var why = Corpus.Stopped(one, (int)TransactSqlParser.TryParseStatement(one).Position);

					var (count, like) = stopped.TryGetValue(why, out var before)
						? before
						: (0, new List<string>());

					if (like.Count < shown)
						like.Add(Corpus.One(one));

					stopped[why] = (count + 1, like);
				}
			}
		}

		Report(version, files.Length, whole, refused, counted, missed, shown);

		Console.WriteLine();
		Console.WriteLine($"  where the {stopped.Values.Sum(static one => one.Count)} refusals stopped");
		Console.WriteLine();

		foreach (var (why, one) in stopped.OrderByDescending(one => one.Value.Count).ThenBy(one => one.Key))
		{
			Console.WriteLine($"  {one.Count,5}  stops at {why}");

			foreach (var example in one.Like.Take(shown))
				Console.WriteLine($"           {example}");
		}
	}

	static void Report(
		string version, int files, int whole, int refused,
		Dictionary<string, (int Total, int Read)> counted,
		Dictionary<string, List<string>> missed,
		int shown)
	{
		var statements = counted.Values.Sum(static one => one.Total);
		var ours       = counted.Values.Sum(static one => one.Read);

		Console.WriteLine();
		Console.WriteLine($"against ScriptDom, TSql{version}Parser");
		Console.WriteLine();
		Console.WriteLine(
			$"  {files} files, {whole} read whole and {refused} not; " +
			$"{statements} statements of {counted.Count} kinds");
		Console.WriteLine();
		Console.WriteLine($"  {"kind",-42}{"count",7}{"read",8}{"share",9}");
		Console.WriteLine($"  {new string('-', 42 + 7 + 8 + 9)}");

		foreach (var (kind, one) in counted.OrderByDescending(one => one.Value.Total).ThenBy(one => one.Key))
		{
			Console.WriteLine(
				$"  {kind,-42}{one.Total,7}{one.Read,8}{Share(one.Read, one.Total),9}");

			foreach (var example in missed.TryGetValue(kind, out var some) ? some.Take(shown) : [])
				Console.WriteLine($"      {example}");
		}

		Console.WriteLine($"  {new string('-', 42 + 7 + 8 + 9)}");
		Console.WriteLine($"  {"",-42}{statements,7}{ours,8}{Share(ours, statements),9}");
	}

	static string Share(int part, int whole) =>
		whole == 0 ? "—" : $"{100.0 * part / whole:F1}%";
}
