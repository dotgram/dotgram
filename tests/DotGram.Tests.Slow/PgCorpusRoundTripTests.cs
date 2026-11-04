using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Benchmarks;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The psql cutter, held to psqlscan.l's rules case by case; and PostgreSQL's regression suite, cut by it and read,
/// written back and read again, every file's four cells held to <c>PgCorpusBaseline.txt</c> and never worse.
/// </summary>
/// <remarks>
/// The suite is <c>tests/Corpus/PostgreSql/sql</c> (or <c>DOTGRAM_PG_CORPUS</c>); where it is absent the corpus test
/// has nothing to hold and passes, as the engine tests do without a server. <c>DOTGRAM_RECORD_PG_CORPUS_BASELINE=1</c>
/// rewrites the baseline from the run.
/// </remarks>
public sealed class PgCorpusRoundTripTests
{
	static string Corpus([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Environment.GetEnvironmentVariable("DOTGRAM_PG_CORPUS") is { Length: > 0 } given
			? given
			: Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Corpus", "PostgreSql", "sql"));
	}

	static string Baseline([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.Combine(Path.GetDirectoryName(here)!, "PgCorpusBaseline.txt");
	}

	// ── The cutter ─────────────────────────────────────────────────────────────

	[Fact]
	public void A_semicolon_ends_a_statement_outside_strings_comments_dollar_quotes_and_brackets()
	{
		var cut = PgCorpus.CutScript("SELECT ';'; SELECT \";\"; SELECT $$;$$; SELECT $t$;$t$; SELECT 1 -- ;\n; SELECT /* ; */ 2; SELECT (1;2); SELECT E'\\';'");

		Assert.Equal(["SELECT ';'", "SELECT \";\"", "SELECT $$;$$", "SELECT $t$;$t$", "SELECT 1 -- ;", "SELECT /* ; */ 2", "SELECT (1;2)", "SELECT E'\\';'"], cut.Statements.Select(static one => one.Text).ToArray());
	}

	[Fact]
	public void A_backslash_command_is_psqls_and_the_ones_that_send_the_buffer_send_it()
	{
		var cut = PgCorpus.CutScript("\\set x 1\nSELECT 1;\n\\d t\nSELECT 2 \\gset\nSELECT 3\n\\echo hi\n;");

		Assert.Equal(["SELECT 1", "SELECT 2", "SELECT 3"], cut.Statements.Select(static one => one.Text).ToArray());
		Assert.Equal(4, cut.Directives);
		Assert.Equal([2, 4, 5], cut.Statements.Select(static one => one.Line).ToArray());
	}

	[Fact]
	public void Copy_from_stdin_is_followed_by_data_that_belongs_to_no_statement()
	{
		var cut = PgCorpus.CutScript("COPY t FROM stdin;\n1\tone;\n2\ttwo\n\\.\nSELECT 1;\ncopy t (a) from STDIN with (format csv);\nx;\n\\.\n");

		Assert.Equal(["COPY t FROM stdin", "SELECT 1", "copy t (a) from STDIN with (format csv)"], cut.Statements.Select(static one => one.Text).ToArray());
		Assert.Equal(3, cut.DataLines);
	}

	[Fact]
	public void A_begin_atomic_body_holds_semicolons_and_its_case_ends()
	{
		var cut = PgCorpus.CutScript("CREATE FUNCTION f() RETURNS int BEGIN ATOMIC SELECT CASE WHEN true THEN 1 END; RETURN 1; END; SELECT 2;");

		Assert.Equal(["CREATE FUNCTION f() RETURNS int BEGIN ATOMIC SELECT CASE WHEN true THEN 1 END; RETURN 1; END", "SELECT 2"], cut.Statements.Select(static one => one.Text).ToArray());
	}

	[Fact]
	public void A_statement_with_a_psql_variable_is_marked_and_a_cast_is_not()
	{
		var cut = PgCorpus.CutScript("SELECT :x; SELECT :'x'; SELECT :\"x\"; SELECT 1::int; SELECT ':x'; SELECT a[1:2];");

		Assert.Equal([true, true, true, false, false, false], cut.Statements.Select(static one => one.HasVariable).ToArray());
	}

	[Fact]
	public void A_statements_kind_is_its_first_words()
	{
		Assert.Equal("SELECT", PgCorpus.Kind("select 1"));
		Assert.Equal("CREATE TABLE", PgCorpus.Kind("CREATE TEMP TABLE t (a int)"));
		Assert.Equal("CREATE INDEX", PgCorpus.Kind("create unique index i on t (a)"));
		Assert.Equal("ALTER TABLE", PgCorpus.Kind("ALTER TABLE t ADD a int"));
		Assert.Equal("INSERT", PgCorpus.Kind("INSERT INTO t VALUES (1)"));
		Assert.Equal("SELECT", PgCorpus.Kind("-- a comment\n/* and */ SELECT 1"));
	}

	// ── The corpus ─────────────────────────────────────────────────────────────

	/// <summary>Every file's four counts, no worse than the baseline's, and the corpus no smaller.</summary>
	[Fact]
	public void No_file_of_the_corpus_reads_or_writes_back_worse_than_its_baseline()
	{
		var root = Corpus();

		if (!Directory.Exists(root))
			return;

		var counts = PgCorpus.Run(root).ByFile;

		if (Environment.GetEnvironmentVariable("DOTGRAM_RECORD_PG_CORPUS_BASELINE") is { Length: > 0 })
		{
			Record(counts);

			return;
		}

		var baseline = Read();
		var faults   = new List<string>();

		foreach (var (file, now) in counts.OrderBy(static one => one.Key, StringComparer.Ordinal))
		{
			if (!baseline.TryGetValue(file, out var then))
			{
				faults.Add($"{file}: not in PgCorpusBaseline.txt. Add the line `{Line(file, now)}`.");
				continue;
			}

			if (now.Read < then.Read)
				faults.Add($"{file}: read {now.Read} statements where the baseline read {then.Read}.");

			if (now.Same < then.Same)
				faults.Add($"{file}: {now.Same} statements came back whole where the baseline had {then.Same}.");

			if (now.Threw > then.Threw)
				faults.Add($"{file}: the writer failed on {now.Threw} statements against {then.Threw}.");
		}

		foreach (var file in baseline.Keys.Where(one => !counts.ContainsKey(one)).OrderBy(static one => one, StringComparer.Ordinal))
			faults.Add($"{file}: in PgCorpusBaseline.txt and not on disk. A corpus that shrank is not an improvement.");

		Assert.Empty(faults);
	}

	static string Line(string file, PgCorpus.Counts counts)
	{
		return $"{file} = {counts.Read}/{counts.Refused}/{counts.Threw}/{counts.Same}";
	}

	static void Record(Dictionary<string, PgCorpus.Counts> counts)
	{
		var lines = new List<string>
		{
			"# What each file of tests/Corpus/PostgreSql/sql reads and writes back: `path = read/refused/threw/same`.",
			"#   read    - statements the cutter cut and this grammar read (statements with psql variables are read by nobody and not counted)",
			"#   refused - statements this grammar did not read",
			"#   threw   - read, and then the writer threw, or printed what does not read back as the same tree",
			"#   same    - read, printed, and read back as the same tree",
			"# Written by DOTGRAM_RECORD_PG_CORPUS_BASELINE=1 and by nothing else; read may not go down, same may not go down, threw may not go up.",
		};

		foreach (var (file, now) in counts.OrderBy(static one => one.Key, StringComparer.Ordinal))
			lines.Add(Line(file, now));

		File.WriteAllText(Baseline(), string.Join("\r\n", lines) + "\r\n");
	}

	static Dictionary<string, PgCorpus.Counts> Read()
	{
		var baseline = new Dictionary<string, PgCorpus.Counts>(StringComparer.Ordinal);

		if (!File.Exists(Baseline()))
			return baseline;

		foreach (var raw in File.ReadAllLines(Baseline()))
		{
			var line = raw.Trim();

			if (line.Length == 0 || line.StartsWith('#'))
				continue;

			var at = line.IndexOf(" = ", StringComparison.Ordinal);

			Assert.True(at > 0, $"PgCorpusBaseline.txt: `{line}` is not `path = read/refused/threw/same`.");

			var cells = line.Substring(at + 3).Split('/');

			Assert.True(cells.Length == 4, $"PgCorpusBaseline.txt: `{line}` has {cells.Length} cells, not four.");

			baseline[line.Substring(0, at)] = new PgCorpus.Counts(int.Parse(cells[0]), int.Parse(cells[1]), int.Parse(cells[2]), int.Parse(cells[3]));
		}

		return baseline;
	}
}
