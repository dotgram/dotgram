using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Parsers;

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
/// ScriptDom builds a full AST with token positions and error recovery, and what this
/// grammar builds is a tenth of that. Those are separate questions and get separate
/// harnesses.
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
	static TSqlParser? Version(string named) => named switch
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

	public static void Run(string? root, string version, int shown)
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

		var files    = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var whole    = 0;
		var refused  = 0;
		var counted  = new Dictionary<string, (int Total, int Read)>(StringComparer.Ordinal);
		var missed   = new Dictionary<string, List<string>>(StringComparer.Ordinal);
		var overRead = new List<string>();

		foreach (var file in files)
		{
			var text = File.ReadAllText(file);

			using var reader = new StringReader(text);

			var read = parser.Parse(reader, out var errors);

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

				// The fragment carries its terminator, and a publication reads the whole of
				// what it is given: a statement separator is the script's punctuation and not
				// the statement's, so it comes off here rather than being read as a refusal.
				var one  = text.Substring(statement.StartOffset, statement.FragmentLength)
					.TrimEnd()
					.TrimEnd(';')
					.TrimEnd();

				var ours = TransactSql.TryParseSelect(one).IsSuccess;

				var (total, already) = counted.TryGetValue(kind, out var seen) ? seen : (0, 0);

				counted[kind] = (total + 1, already + (ours ? 1 : 0));

				if (ours && kind != nameof(SelectStatement))
					overRead.Add(Corpus.One(one));

				if (!ours)
				{
					if (!missed.TryGetValue(kind, out var some))
						missed[kind] = some = [];

					if (some.Count < shown)
						some.Add(Corpus.One(one));
				}
			}
		}

		Report(version, files.Length, whole, refused, counted, missed, overRead, shown);
	}

	static void Report(
		string version, int files, int whole, int refused,
		Dictionary<string, (int Total, int Read)> counted,
		Dictionary<string, List<string>> missed,
		List<string> overRead,
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
		Console.WriteLine();

		Console.WriteLine(
			overRead.Count == 0
				? "  Nothing ScriptDom calls something other than a query reads as one here."
				: $"  {overRead.Count} read here as a query and called something else there — a defect on this side:");

		foreach (var one in overRead.Take(shown * 4))
			Console.WriteLine($"      {one}");
	}

	static string Share(int part, int whole) =>
		whole == 0 ? "—" : $"{100.0 * part / whole:F1}%";
}
