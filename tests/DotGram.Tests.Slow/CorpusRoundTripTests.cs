using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Benchmarks;
using DotGram.Sql.TransactSql;

using Microsoft.SqlServer.TransactSql.ScriptDom;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Somebody else's SQL, read and written back: the 1,086 files of <c>tests/Corpus/ScriptDom</c>, held
/// per file to <c>CorpusBaseline.txt</c>.
/// </summary>
/// <remarks>
/// <para>
/// Nothing in any suite read that corpus before this. The round trip over it ran only by hand, through
/// the benchmarks' <c>--roundtrip</c>, so a regression in what T-SQL reads, refuses or writes back over
/// real SQL was invisible to CI — and the corpus is the swap's oracle.
/// </para>
/// <para>
/// <b>The cutting and the comparing are shared with <c>RoundTrip.cs</c></b> (<c>CorpusRoundTrip.cs</c>,
/// compiled into both): what this catches is a CHANGE in the counts, not a fault in how a statement is
/// cut or compared, which both roads would share. <see cref="A_statement_of_each_partition_comes_back"/>
/// is the small assertion that does not go through the shared code, so that shared code which stopped
/// comparing anything would still be noticed.
/// </para>
/// <para>
/// <b>It is 100% today for the tree the SHIPPED T-SQL grammar builds.</b> When the one-tree unification
/// swaps the grammar pair onto <c>DotGram.Sql.Ast</c>, this gate is what the swap has to pass — a
/// failure here at that point is the swap losing a statement, not noise to be waved through
/// (<c>docs/design/sql-tsql-tree.md</c>).
/// </para>
/// </remarks>
public sealed class CorpusRoundTripTests(ITestOutputHelper output)
{
	/// <summary>Where the corpus is, from this file.</summary>
	static string Corpus([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Corpus", "ScriptDom"));
	}

	static string Baseline([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.Combine(Path.GetDirectoryName(here)!, "CorpusBaseline.txt");
	}

	/// <summary>
	/// Every file's four counts, no worse than the baseline's, and the corpus no smaller.
	/// </summary>
	/// <remarks>
	/// Written by the process that measures it and by no other: <c>DOTGRAM_RECORD_CORPUS_BASELINE=1</c>
	/// rewrites <c>CorpusBaseline.txt</c> from this run. A file the baseline does not name fails and
	/// prints the line to add, rather than passing; a file the baseline names and the disk does not have
	/// fails as well, because a corpus that shrank is not an improvement.
	/// </remarks>
	[Fact]
	public void No_file_of_the_corpus_reads_or_writes_back_worse_than_its_baseline()
	{
		var root   = Corpus();
		var counts = CorpusRoundTrip.Run(root).ByFile;

		if (Environment.GetEnvironmentVariable("DOTGRAM_RECORD_CORPUS_BASELINE") is { Length: > 0 })
		{
			Record(counts);

			return;
		}

		var baseline = Read();
		var faults   = new List<string>();
		var better   = new List<string>();

		foreach (var (file, now) in counts.OrderBy(static one => one.Key, StringComparer.Ordinal))
		{
			if (!baseline.TryGetValue(file, out var then))
			{
				faults.Add($"{file}: not in CorpusBaseline.txt. Add the line `{Line(file, now)}`.");

				continue;
			}

			if (now.Read < then.Read)
				faults.Add($"{file}: read {now.Read} statements where the baseline read {then.Read}.");

			if (now.Same < then.Same)
				faults.Add($"{file}: {now.Same} statements came back whole where the baseline had {then.Same}.");

			if (now.Threw > then.Threw)
				faults.Add($"{file}: the writer failed on {now.Threw} statements against {then.Threw}.");

			if (now != then && now.Read >= then.Read && now.Same >= then.Same && now.Threw <= then.Threw)
				better.Add($"{file}: {Line(file, then)} -> {Line(file, now)}");
		}

		foreach (var file in baseline.Keys.Where(one => !counts.ContainsKey(one)).OrderBy(static one => one, StringComparer.Ordinal))
			faults.Add($"{file}: in CorpusBaseline.txt and not on disk. A corpus that shrank is not an improvement.");

		// Printed rather than asserted: the baseline is rewritten deliberately, not by a passing run.
		// Through the output helper, because a message on a passing assertion is shown nowhere.
		if (better.Count > 0)
			foreach (var line in better.Prepend("improved, so regenerate the baseline deliberately:"))
				output.WriteLine(line);

		Assert.Equal([], faults);
	}

	/// <summary>
	/// One statement from each partition of the corpus, round-tripped here rather than through the
	/// shared code.
	/// </summary>
	/// <remarks>
	/// The independence the shared implementation costs, bought back in the smallest way that pays: if
	/// <c>CorpusRoundTrip</c> ever stopped comparing anything — every comparison trivially equal, a
	/// generator that prints nothing — the counts would still match their baseline and this would not.
	/// The inputs are chosen to witness the version mapping too: <c>*=</c> is syntax no parser above 80
	/// reads, so a reading that ignored a file's own version would refuse it.
	/// </remarks>
	[Theory]
	[InlineData("180", "SELECT a, b FROM t WHERE a > 1 ORDER BY b")]
	[InlineData("180", "UPDATE t SET a = 1 OUTPUT inserted.a INTO u (a) WHERE b = 2")]
	[InlineData("180", "MERGE INTO t USING u ON t.a = u.a WHEN MATCHED THEN UPDATE SET t.b = u.b;")]
	public void A_statement_of_each_partition_comes_back(string version, string statement)
	{
		var read = TransactSqlParser.TryParseStatement(statement);

		Assert.True(read.IsSuccess, $"this grammar refused `{statement}`, which ScriptDom {version} reads.");

		var printed = DotGram.Sql.SqlWriter.Write((DotGram.Sql.Statement)read.Value!);

		Assert.Equal(Theirs(version, statement), Theirs(version, printed));
	}

	/// <summary>
	/// That reading each file at its own version is load-bearing, and not a precaution.
	/// </summary>
	/// <remarks>
	/// The round trip cannot witness this: <c>*=</c> and <c>DUMP DATABASE</c> are syntax the corpus
	/// holds and <b>this grammar refuses</b> — they are among the 723 statements ScriptDom reads and it
	/// does not — so no statement of the 80 partition comes back to be compared. What can be witnessed
	/// is that the mapping does work at all: ScriptDom 80 reads them and ScriptDom 180 does not, so a
	/// reading at 180 alone would drop the files that hold them, and the corpus the gate covers would
	/// quietly shrink.
	/// </remarks>
	[Theory]
	[InlineData("SELECT a FROM t, u WHERE t.a *= u.a")]
	[InlineData("DUMP DATABASE d TO DISK = 'd.bak'")]
	public void A_file_is_read_at_its_own_version_because_180_will_not_read_it(string statement)
	{
		Assert.True(Reads("80", statement), $"ScriptDom 80 no longer reads `{statement}`, so this row witnesses nothing.");
		Assert.False(Reads("180", statement), $"ScriptDom 180 now reads `{statement}`, so it no longer shows why the version mapping is needed. Change the row, not the assertion.");
	}

	static bool Reads(string version, string text)
	{
		TSqlParser parser = version == "80" ? new TSql80Parser(true) : new TSql180Parser(true);

		using var reader = new StringReader(text);

		return parser.Parse(reader, out var errors) is TSqlScript && errors.Count == 0;
	}

	/// <summary>What ScriptDom makes of a statement, printed by its own generator.</summary>
	static string Theirs(string version, string text)
	{
		TSqlParser parser = version switch
		{
			"80" => new TSql80Parser(true),
			_    => new TSql180Parser(true),
		};

		SqlScriptGenerator generator = version switch
		{
			"80" => new Sql80ScriptGenerator(new SqlScriptGeneratorOptions { KeywordCasing = KeywordCasing.Uppercase }),
			_    => new Sql180ScriptGenerator(new SqlScriptGeneratorOptions { KeywordCasing = KeywordCasing.Uppercase }),
		};

		using var reader = new StringReader(text);

		Assert.True(
			parser.Parse(reader, out var errors) is TSqlScript && errors.Count == 0,
			$"ScriptDom {version} would not read `{text}`, so this row checks nothing. Change the row, not the assertion.");

		using var again = new StringReader(text);
		var script = (TSqlScript)parser.Parse(again, out _)!;

		generator.GenerateScript(script, out var printed);

		return string.Join(' ', printed.Split(' ', '\t', '\n', '\r').Where(static one => one.Length > 0));
	}

	static string Line(string file, CorpusRoundTrip.Counts counts)
	{
		return $"{file} = {counts.Read}/{counts.Refused}/{counts.Threw}/{counts.Same}";
	}

	static Dictionary<string, CorpusRoundTrip.Counts> Read()
	{
		var rows = new Dictionary<string, CorpusRoundTrip.Counts>(StringComparer.Ordinal);

		foreach (var line in File.ReadAllLines(Baseline()))
		{
			if (line.Length == 0 || line[0] == '#')
				continue;

			var at = line.LastIndexOf(" = ", StringComparison.Ordinal);

			Assert.True(at > 0, $"CorpusBaseline.txt: `{line}` is not `path = read/refused/threw/same`.");

			var parts = line[(at + 3)..].Split('/');

			Assert.Equal(4, parts.Length);

			rows[line[..at]] = new CorpusRoundTrip.Counts(
				int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
		}

		return rows;
	}

	static void Record(Dictionary<string, CorpusRoundTrip.Counts> counts)
	{
		var text = new StringBuilder();

		text.AppendLine("# What each file of tests/Corpus/ScriptDom reads and writes back: `path = read/refused/threw/same`.");
		text.AppendLine("#   read    - statements ScriptDom read and this grammar read as well");
		text.AppendLine("#   refused - statements ScriptDom read and this grammar did not");
		text.AppendLine("#   threw   - read, and then the writer threw or printed something ScriptDom will not read");
		text.AppendLine("#   same    - read, printed, and printed back to the same statement");
		text.AppendLine("# A fall in `read` or `same`, or a rise in `threw`, fails CorpusRoundTripTests. A rise in the");
		text.AppendLine("# first two is printed, and this file is regenerated deliberately rather than by a passing run:");
		text.AppendLine("#   DOTGRAM_RECORD_CORPUS_BASELINE=1 dotnet .build/bin/DotGram.Tests.Slow/debug/DotGram.Tests.Slow.dll");
		text.AppendLine("# Both the reading and the printing are ScriptDom's, at each file's own parser version where a");
		text.AppendLine("# Baselines<n> directory names one, capped at 180. The counts are the same ones `--roundtrip`");
		text.AppendLine("# prints, from the same code (CorpusRoundTrip.cs).");
		text.AppendLine("#");
		text.AppendLine($"# Files {counts.Count}, read {counts.Values.Sum(static one => one.Read)}, " +
			$"refused {counts.Values.Sum(static one => one.Refused)}, " +
			$"threw {counts.Values.Sum(static one => one.Threw)}, " +
			$"same {counts.Values.Sum(static one => one.Same)}.");
		text.AppendLine();

		foreach (var (file, count) in counts.OrderBy(static one => one.Key, StringComparer.Ordinal))
			text.AppendLine(Line(file, count));

		File.WriteAllText(Baseline(), text.ToString());
	}
}
