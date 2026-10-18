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
/// <b>The baseline is the old T-SQL tree's, which brought every statement back.</b> The parser now hands
/// back <c>DotGram.Sql.Ast</c>, printed by <c>TransactSqlWriter</c>, and this gate is what that swap has
/// to pass — a failure here is the swap losing a statement, not noise to be waved through
/// (<c>docs/design/sql-tsql-tree.md</c>). What it does not bring back yet is listed, statement by
/// statement, in <see cref="Known"/>; a key word the writer prints in capitals where ScriptDom keeps
/// what was written is no difference (<c>CorpusRoundTrip.KeyWordCaseOnly</c>).
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
		var root     = Corpus();
		var troubles = new List<CorpusRoundTrip.Trouble>();
		var counts   = CorpusRoundTrip.Run(root, show: troubles.Add).ByFile;

		if (Environment.GetEnvironmentVariable("DOTGRAM_RECORD_CORPUS_BASELINE") is { Length: > 0 })
		{
			Record(counts);

			return;
		}

		var baseline = Read();
		var faults   = new List<string>();
		var better   = new List<string>();
		var excused  = new Dictionary<string, (int Threw, int Different)>(StringComparer.Ordinal);
		var met      = new HashSet<int>();
		var unlisted = new List<string>();

		foreach (var trouble in troubles)
		{
			var at = Enumerable.Range(0, Known.Length).FirstOrDefault(
				one => !met.Contains(one) && Known[one].File == trouble.File && trouble.Original.Contains(Known[one].Statement, StringComparison.Ordinal),
				-1);

			if (at < 0)
			{
				unlisted.Add($"{trouble.File}: `{trouble.Original}` did not come back: {trouble.Why ?? trouble.Theirs + " | " + trouble.Ours}");

				continue;
			}

			met.Add(at);

			if (!FailsAsListed(trouble, Known[at].Expected))
			{
				faults.Add($"{trouble.File}: `{Known[at].Statement}` is listed as {Known[at].Expected} and failed otherwise: {trouble.Why ?? trouble.Theirs + " | " + trouble.Ours}{(trouble.Gap is { } gap ? ", the gap " + gap : "")}.");

				continue;
			}

			var (threw, different) = excused.GetValueOrDefault(trouble.File);

			excused[trouble.File] = trouble.Why is null ? (threw, different + 1) : (threw + 1, different);
		}

		for (var at = 0; at < Known.Length; at++)
			if (!met.Contains(at))
				faults.Add($"{Known[at].File}: `{Known[at].Statement}` comes back now. Take it out of Known.");

		foreach (var (file, now) in counts.OrderBy(static one => one.Key, StringComparer.Ordinal))
		{
			if (!baseline.TryGetValue(file, out var then))
			{
				faults.Add($"{file}: not in CorpusBaseline.txt. Add the line `{Line(file, now)}`.");

				continue;
			}

			var (threw, different) = excused.GetValueOrDefault(file);

			if (now.Read < then.Read)
				faults.Add($"{file}: read {now.Read} statements where the baseline read {then.Read}.");

			if (now.Same + threw + different < then.Same)
				faults.Add($"{file}: {now.Same} statements came back whole where the baseline had {then.Same}, {threw + different} of them known differences.");

			if (now.Threw - threw > then.Threw)
				faults.Add($"{file}: the writer failed on {now.Threw} statements against {then.Threw}, {threw} of them known differences.");

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

		// What did not come back and is not listed is printed in full only where a count fell: the assertion's own
		// message cuts each line short.
		if (faults.Count > 0)
			foreach (var line in faults.Concat(unlisted))
				output.WriteLine(line);

		Assert.Equal([], faults);
	}

	/// <summary>
	/// The statements of the corpus that read and do not come back while the T-SQL entries carry what the grammar
	/// reads onto the common tree: the file, a piece of the statement's text as the corpus writes it, and how it fails.
	/// </summary>
	/// <remarks>
	/// The baseline is the old tree's, which brought every one of them back. Each is excused in its own file only,
	/// once, and only while it fails as its row says: a gap of the reason named, which the writer refuses. A row that
	/// no longer happens fails, and so does one that fails otherwise — another gap, a writer exception, a printing —
	/// so that the list shrinks with the gaps it names and never covers a new one. The counts are what the baseline
	/// holds the rest of the corpus to.
	/// </remarks>
	static readonly (string File, string Statement, Expected Expected)[] Known =
	[
		("TestScripts/ScalarDataTypeTests.sql",      "c30 national text not null",            Gap("a type `national text` of several words")),
		("TestScripts/CreateTableTests90.sql",       "c2 national sys.text",                  Gap("a name part `national sys`")),
		("TestScripts/ScalarDataTypeTests90.sql",    "c39a Double Precision(max)",            Gap("a type `Double Precision(max)` of several words")),
		("TestScripts/FromClauseTests90.sql",        "tablesample (1000 rows)(nolock)",       Gap("a hint written against the sample")),
		("TestScripts/FromClauseTests.sql",          "select c1 from t1 with (0)",            Gap("the old WITH (n)")),
		("TestScripts/InsertStatementTests.sql",     "Insert ..t1 (c1, a.b.c.d",              Gap("a qualified or prefixed column `a.b.c.d`")),
		("BaselinesCommon/InsertStatementTests.sql", "INSERT ..t1 (c1, a.b.c.d",              Gap("a qualified or prefixed column `a.b.c.d`")),
		("TestScripts/InsertStatementTests.sql",     "Insert @v1 (..a1)",                     Gap("a qualified or prefixed column `..a1`")),
		("BaselinesCommon/InsertStatementTests.sql", "INSERT @v1 (..a1)",                     Gap("a qualified or prefixed column `..a1`")),
		("TestScripts/BackupStatementTests.sql",     "file = ('f2',@var4)",                   Gap("a name `('f2',@var4)`")),
		("BaselinesCommon/BackupStatementTests.sql", "FILE = ('f2', @var4)",                  Gap("a name `('f2', @var4)`")),
		("TestScripts/BackupStatementTests.sql",     "filegroup = (@var5, @var6)",            Gap("a name `(@var5, @var6)`")),
		("BaselinesCommon/BackupStatementTests.sql", "FILEGROUP = (@var5, @var6)",            Gap("a name `(@var5, @var6)`")),
		("TestScripts/ExpressionTests.sql",          "{fn extract(hour from getdate())}",     Gap("(the word `hour`)")),
		("BaselinesCommon/ExpressionTests.sql",      "{ FN extract ( hour FROM getdate()) }", Gap("(the word `hour`)")),
		("TestScripts/JsonFunctionTests170.sql",     "JSON_OBJECTAGG()",                      Gap("JSON_OBJECTAGG of no pair")),
		("Baselines170/JsonFunctionTests170.sql",    "JSON_OBJECTAGG()",                      Gap("JSON_OBJECTAGG of no pair")),
		("TestScripts/RowsetsInSelectTests.sql",     ". 'Something' AS Z",                    Gap("a name part `'Something'` that is no name")),
		("BaselinesCommon/RowsetsInSelectTests.sql", ".'Something' AS Z",                     Gap("a name part `'Something'` that is no name")),
	];

	/// <summary>How a known difference fails, which it must go on doing to be excused.</summary>
	/// <param name="How">The bridge's gap, the writer's refusal of a tree, or a printing ScriptDom reads otherwise.</param>
	/// <param name="Says">
	/// A piece of the gap's reason, the name of the writer's exception, or a piece of ScriptDom's printing of what this
	/// grammar read.
	/// </param>
	sealed record Expected(Failure How, string Says);

	enum Failure { Gap, Refused, Printed }

	/// <summary>An extension of the bridge's, whose reason says <paramref name="reason"/>, which the writer refuses.</summary>
	static Expected Gap(string reason)
	{
		return new Expected(Failure.Gap, reason);
	}

	/// <summary>Whether a statement that did not come back failed the way its row says.</summary>
	static bool FailsAsListed(CorpusRoundTrip.Trouble trouble, Expected expected)
	{
		return expected.How switch
		{
			Failure.Gap     => trouble.Gap is { } gap && gap.Contains(expected.Says, StringComparison.Ordinal) && trouble.Why == "the writer threw: NotSupportedException",
			Failure.Refused => trouble.Gap is null && trouble.Why == "the writer threw: " + expected.Says,
			Failure.Printed => trouble.Why is null && trouble.Ours is { } ours && ours.Contains(expected.Says, StringComparison.Ordinal),
			_               => false,
		};
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

		var printed = DotGram.Sql.Ast.TransactSqlWriter.Write(read.Value!);

		Assert.Equal(Theirs(version, statement), Theirs(version, printed));
	}

	/// <summary>
	/// A name spelled like a key word, in any capitals, bare or bracketed, comes back exactly as it was written.
	/// </summary>
	/// <remarks>
	/// The writer prints key words in capitals, and the corpus gate excuses that where the tree holds a key word;
	/// a name is held as written, so here the two printings are compared with no case excused at all.
	/// </remarks>
	[Theory]
	[InlineData("SELECT Wk, [Trim], Privileges, [ALL] FROM t")]
	[InlineData("select wK from [Wk] as Trim where [trim] = 1")]
	[InlineData("SELECT a AS Privileges, b AS [Wk], c AS [all] FROM [Trim].Wk")]
	[InlineData("GRANT SELECT ON Trim TO Privileges")]
	[InlineData("UPDATE [All] SET Wk = trim WHERE [Privileges] = 1")]
	public void A_name_spelled_like_a_key_word_comes_back_as_written(string statement)
	{
		var printed = DotGram.Sql.Ast.TransactSqlWriter.Write(TransactSqlParser.ParseStatement(statement));

		Assert.Equal(Theirs("180", statement), Theirs("180", printed));
	}

	/// <summary>
	/// That the corpus gate excuses no difference of case in a name, a quoted name or a string.
	/// </summary>
	[Theory]
	[InlineData("SELECT Wk FROM t", "SELECT WK FROM t")]
	[InlineData("SELECT [Trim] FROM t", "SELECT [TRIM] FROM t")]
	[InlineData("SELECT a AS Privileges FROM t", "SELECT a AS PRIVILEGES FROM t")]
	[InlineData("SELECT [ALL] FROM t", "SELECT [all] FROM t")]
	[InlineData("SELECT 'wk' FROM t", "SELECT 'WK' FROM t")]
	[InlineData("SELECT DATEPART(wk, Wk) FROM t", "SELECT DATEPART(WK, WK) FROM t")]
	public void A_name_in_other_capitals_is_not_excused(string written, string printed)
	{
		Assert.False(CorpusRoundTrip.KeyWordCaseOnly(new TSql180Parser(true), written, printed, TransactSqlParser.ParseStatement(written)));
	}

	/// <summary>
	/// That it excuses the case of a word the tree holds as a key word or an enum, which the writer prints in capitals.
	/// </summary>
	[Theory]
	[InlineData("SELECT DATEPART(wk, d) FROM t", "SELECT DATEPART(WK, d) FROM t")]
	[InlineData("SELECT trim(a) FROM t", "SELECT TRIM(a) FROM t")]
	[InlineData("grant all privileges on t to u", "grant ALL PRIVILEGES on t to u")]
	public void A_key_word_in_other_capitals_is_excused(string written, string printed)
	{
		Assert.True(CorpusRoundTrip.KeyWordCaseOnly(new TSql180Parser(true), written, printed, TransactSqlParser.ParseStatement(written)));
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
