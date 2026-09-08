using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Parsers;
using DotGram.Parsers.Sql;

using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// Whether the tree says what the text said, checked against ScriptDom rather than against
/// ourselves.
/// </summary>
/// <remarks>
/// <para>
/// A refusal count cannot see the worst kind of defect: the parser reads the text, answers
/// yes, and builds something else. Nothing in this repository could have caught that, because
/// the only thing that knows what the tree should hold is the tree.
/// </para>
/// <para>
/// So a second parser is asked. For each statement:
/// </para>
/// <list type="number">
/// <item>ScriptDom parses the original and prints it — call that <b>A</b>;</item>
/// <item>this grammar parses the original, <see cref="SqlWriter"/> prints it, ScriptDom
/// parses <em>that</em> and prints it — call that <b>B</b>.</item>
/// </list>
/// <para>
/// <b>ScriptDom's generator is the normal form on both sides</b>, which is the whole trick:
/// keyword casing, line breaks, redundant brackets, `INNER` against nothing and every other
/// way of writing the same statement are erased before the comparison. What survives a
/// difference between A and B is a difference in meaning.
/// </para>
/// <para>
/// <b>The number this reports is completeness, not correctness alone.</b> Everything the tree
/// reads and drops — `TOP`, `OVER`, the hints, `OUTPUT`, a named query's `WITH`, the option
/// lists of the DDL — cannot be printed back, so it lands here as a difference. That is the
/// point: this is the first measurement of how much the tree throws away, statement by
/// statement, and the per-kind table is the list of work to make it lossless.
/// </para>
/// </remarks>
static class RoundTrip
{
	public static void Run(string? root, string version, string? wanted = null, int most = 2)
	{
		root ??= Corpus.Checked();

		if (!Directory.Exists(root))
		{
			Console.WriteLine($"No corpus at {root}.");

			return;
		}

		if (Kinds.Version(version) is not { } parser)
		{
			Console.WriteLine($"No such version as {version}. One of 80 90 100 … 180.");

			return;
		}

		var generator = Generator(version);
		var counts    = new Dictionary<string, Tally>(StringComparer.Ordinal);
		var shown     = new List<string>();

		var read = 0;
		var same = 0;
		var lost = 0;
		var bad  = 0;

		foreach (var file in Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories))
		{
			var text = File.ReadAllText(file);

			using var whole = new StringReader(text);

			if (parser.Parse(whole, out var errors) is not TSqlScript script || errors.Count > 0)
				continue;

			foreach (var statement in script.Batches.SelectMany(static batch => batch.Statements))
			{
				var original = text.Substring(statement.StartOffset, statement.FragmentLength).TrimEnd();
				var kind     = statement.GetType().Name;

				Statement? made;

				try
				{
					var match = TransactSql.TryParseStatement(original);

					made = match.IsSuccess ? match.Value as Statement : null;
				}
				catch (Exception)
				{
					made = null;
				}

				// Only what both read: what this grammar refuses is `--kinds`' business, and
				// counting it here would say the same thing twice.
				if (made is null)
					continue;

				read++;

				var tally = Counted(counts, kind);

				tally.All++;

				string printed;

				try
				{
					printed = SqlWriter.Write(made);
				}
				catch (Exception failure)
				{
					bad++;
					tally.Broken++;
					Show(shown, wanted, most, kind, original, "the writer threw: " + failure.GetType().Name);

					continue;
				}

				generator.GenerateScript(statement, out var a);

				using var again = new StringReader(printed);

				if (parser.Parse(again, out var refused) is not TSqlScript back || refused.Count > 0 ||
					back.Batches.SelectMany(static batch => batch.Statements).ToArray()
						is not [var only])
				{
					bad++;
					tally.Broken++;
					Show(shown, wanted, most, kind, original, "printed as: " + printed);

					continue;
				}

				generator.GenerateScript(only, out var b);

				if (Normalized(a) == Normalized(b))
				{
					same++;
					tally.Same++;
				}
				else
				{
					lost++;
					Show(shown, wanted, most, kind, original, "A: " + One(a) + "\n     B: " + One(b));
				}
			}
		}

		Report(read, same, lost, bad, counts, shown);
	}

	sealed class Tally
	{
		public int All;
		public int Same;
		public int Broken;
	}

	static Tally Counted(Dictionary<string, Tally> counts, string kind)
	{
		if (!counts.TryGetValue(kind, out var tally))
			counts[kind] = tally = new Tally();

		return tally;
	}

	static void Report(
		int read, int same, int lost, int bad,
		Dictionary<string, Tally> counts, List<string> shown)
	{
		Console.WriteLine();
		Console.WriteLine($"  {read} statements read by both, printed back and put to ScriptDom again");
		Console.WriteLine();
		Console.WriteLine($"  {same,6}  the same statement          {Share(same, read)}");
		Console.WriteLine($"  {lost,6}  read, printed, and different {Share(lost, read)}");
		Console.WriteLine($"  {bad,6}  printed into something ScriptDom will not read {Share(bad, read)}");
		Console.WriteLine();
		Console.WriteLine($"  {"kind",-46}{"count",7}{"same",8}{"share",9}");

		foreach (var (kind, tally) in counts.OrderByDescending(static one => one.Value.All - one.Value.Same)
			.ThenByDescending(static one => one.Value.All))
		{
			if (tally.All == tally.Same)
				continue;

			Console.WriteLine($"  {kind,-46}{tally.All,7}{tally.Same,8}{Share(tally.Same, tally.All),9}");
		}

		var whole = counts.Values.Count(static one => one.All == one.Same);

		Console.WriteLine();
		Console.WriteLine($"  and {whole} kinds that come back whole");

		if (shown.Count == 0)
			return;

		Console.WriteLine();

		foreach (var one in shown)
			Console.WriteLine(one);
	}

	static string Share(int part, int all) =>
		all == 0 ? "" : $"{100.0 * part / all,7:0.0}%";

	/// <summary>
	/// A few examples of each kind, so that the list stays readable — or a great many of one
	/// kind, which is what a wave of work on that kind needs.
	/// </summary>
	static void Show(
		List<string> shown, string? wanted, int most, string kind, string original, string how)
	{
		if (wanted is not null && !string.Equals(kind, wanted, StringComparison.OrdinalIgnoreCase))
			return;

		var already = shown.Count(one => one.StartsWith("  " + kind + " ", StringComparison.Ordinal));

		if (already >= most)
			return;

		shown.Add($"  {kind} — {One(original)}\n     {how}");
	}

	static string One(string text)
	{
		var line = string.Join(' ', text.Split('\n', '\r').Select(static one => one.Trim())
			.Where(static one => one.Length > 0));

		return line.Length > 140 ? line[..140] + "…" : line;
	}

	/// <summary>
	/// Whitespace is not meaning. Both sides come out of the same generator, so the only
	/// difference this erases is the one a line break makes to a string comparison.
	/// </summary>
	static string Normalized(string text) =>
		string.Join(' ', text.Split(' ', '\t', '\n', '\r')
			.Where(static one => one.Length > 0));

	static SqlScriptGenerator Generator(string version)
	{
		var options = new SqlScriptGeneratorOptions
		{
			KeywordCasing              = KeywordCasing.Uppercase,
			IncludeSemicolons          = true,
			AlignClauseBodies          = false,
			NewLineBeforeFromClause    = false,
			NewLineBeforeWhereClause   = false,
			NewLineBeforeGroupByClause = false,
			NewLineBeforeHavingClause  = false,
			NewLineBeforeOrderByClause = false,
			NewLineBeforeJoinClause    = false,
			NewLineBeforeOnClause      = false,
			NewLineBeforeOffsetClause  = false,
			NewLineBeforeOutputClause  = false,
		};

		return version switch
		{
			"80"  => new Sql80ScriptGenerator (options),
			"90"  => new Sql90ScriptGenerator (options),
			"100" => new Sql100ScriptGenerator(options),
			"110" => new Sql110ScriptGenerator(options),
			"120" => new Sql120ScriptGenerator(options),
			"130" => new Sql130ScriptGenerator(options),
			"140" => new Sql140ScriptGenerator(options),
			"150" => new Sql150ScriptGenerator(options),
			"160" => new Sql160ScriptGenerator(options),
			"170" => new Sql170ScriptGenerator(options),
			_     => new Sql180ScriptGenerator(options),
		};
	}
}
