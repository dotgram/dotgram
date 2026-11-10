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

	/// <summary>
	/// The second sample beside the suite, <c>tests/Corpus/PostgreSql/dump</c>: what pg_dump writes, which the suite does not
	/// hold. Its files are keyed under <c>dump/</c> in the same baseline.
	/// </summary>
	static string Dump([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Corpus", "PostgreSql", "dump"));
	}

	/// <summary>Every file's counts: the suite's by their own names, the dump sample's under <c>dump/</c> where it is on disk.</summary>
	static Dictionary<string, PgCorpus.Counts> Counted(string root)
	{
		var counts = PgCorpus.Run(root).ByFile;

		if (Environment.GetEnvironmentVariable("DOTGRAM_PG_CORPUS") is not { Length: > 0 } && Directory.Exists(Dump()))
		{
			foreach (var (file, now) in PgCorpus.Run(Dump()).ByFile)
				counts["dump/" + file] = now;
		}

		return counts;
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
		var cut = PgCorpus.CutScript("COPY t FROM stdin;\n1\tone;\n2\ttwo\n\\.\nSELECT 1;\ncopy t (a) from STDIN with (format csv);\nx;\n\\.\n-- the data\nCOPY u FROM stdin;\n3\n\\.\n\\copy v from stdin\n4;\n\\.\nSELECT 2;");

		Assert.Equal(["COPY t FROM stdin", "SELECT 1", "copy t (a) from STDIN with (format csv)", "COPY u FROM stdin", "SELECT 2"], cut.Statements.Select(static one => one.Text).ToArray());
		Assert.Equal(5, cut.DataLines);
	}

	[Fact]
	public void A_comment_before_a_statement_is_not_the_statement()
	{
		var cut = PgCorpus.CutScript("-- one\n/* two */\nSELECT 1; -- three\nSELECT /* four */ 2;\n-- five");

		Assert.Equal(["SELECT 1", "SELECT /* four */ 2"], cut.Statements.Select(static one => one.Text).ToArray());
		Assert.Equal([3, 4], cut.Statements.Select(static one => one.Line).ToArray());
	}

	[Fact]
	public void A_backslash_semicolon_ends_a_statement_and_the_commands_are_known_by_name()
	{
		var cut = PgCorpus.CutScript("SELECT 1 \\; SELECT 2;\nSELECT 3 \\getenv x PATH\n;\nSELECT 4 \\r\nSELECT 5;\nSELECT 6 \\bind 1 \\g\nSELECT 7 \\parse p\n\\if true \\\\ SELECT 8;\n\\endif");

		Assert.Equal(["SELECT 1", "SELECT 2", "SELECT 3", "SELECT 5", "SELECT 6", "SELECT 7", "SELECT 8"], cut.Statements.Select(static one => one.Text).ToArray());
	}

	[Fact]
	public void A_plain_string_reads_escapes_while_standard_conforming_strings_is_off()
	{
		var cut = PgCorpus.CutScript("SET standard_conforming_strings = off;\nSELECT 'a\\'b';\nSELECT 2;\nSELECT U&'\\', b'\\', x'\\';\nSELECT ab'\\'c';\nRESET standard_conforming_strings;\nSELECT 'c\\';\nSELECT 3;");

		Assert.Equal(["SET standard_conforming_strings = off", "SELECT 'a\\'b'", "SELECT 2", "SELECT U&'\\', b'\\', x'\\'", "SELECT ab'\\'c'", "RESET standard_conforming_strings", "SELECT 'c\\'", "SELECT 3"], cut.Statements.Select(static one => one.Text).ToArray());
	}

	[Fact]
	public void A_commands_quoted_argument_may_hold_a_double_backslash()
	{
		var cut = PgCorpus.CutScript("\\qecho '\\\\.'\nSELECT 1;\nSELECT 2;");

		Assert.Equal(["SELECT 1", "SELECT 2"], cut.Statements.Select(static one => one.Text).ToArray());
	}

	[Fact]
	public void Sendpipeline_sends_and_a_backslash_semicolon_inside_parentheses_is_kept()
	{
		var cut = PgCorpus.CutScript("SELECT 1 \\bind \\sendpipeline\nSELECT (1 \\; 2), '\\:'; SELECT a \\: b;");

		Assert.Equal(["SELECT 1", "SELECT (1 ; 2), '\\:'", "SELECT a : b"], cut.Statements.Select(static one => one.Text).ToArray());
	}

	[Fact]
	public void Gset_and_getenv_define_variables()
	{
		var cut = PgCorpus.CutScript("\\getenv home HOME\nSELECT :home; SELECT :other; SELECT 1 \\gset\nSELECT :other; SELECT a[1:NULL], a[1:2];");

		Assert.Equal([true, false, true, false], cut.Statements.Where(static one => one.Text.StartsWith("SELECT :", StringComparison.Ordinal) || one.Text.StartsWith("SELECT a[", StringComparison.Ordinal)).Select(static one => one.HasVariable).ToArray());
	}

	[Fact]
	public void A_begin_atomic_body_holds_semicolons_and_its_case_ends()
	{
		var cut = PgCorpus.CutScript("CREATE FUNCTION f() RETURNS int BEGIN ATOMIC SELECT CASE WHEN true THEN 1 END; RETURN 1; END; SELECT 2;");

		Assert.Equal(["CREATE FUNCTION f() RETURNS int BEGIN ATOMIC SELECT CASE WHEN true THEN 1 END; RETURN 1; END", "SELECT 2"], cut.Statements.Select(static one => one.Text).ToArray());
	}

	[Fact]
	public void A_statement_with_a_psql_variable_is_marked_and_a_cast_or_an_unset_name_is_not()
	{
		var cut = PgCorpus.CutScript("\\set x 1\nSELECT :x; SELECT :'x'; SELECT :\"x\"; SELECT :{?x}; SELECT 1::int; SELECT ':x'; SELECT a[1:2]; SELECT a[1:NULL]; SELECT :y; SELECT :VERSION_NUM;");

		Assert.Equal([true, true, true, true, false, false, false, false, false, true], cut.Statements.Select(static one => one.HasVariable).ToArray());
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

		var counts = Counted(root);

		if (Environment.GetEnvironmentVariable("DOTGRAM_RECORD_PG_CORPUS_BASELINE") is { Length: > 0 })
		{
			// A re-record never hides a file that reads less: a drop must be allowed by name, in
			// DOTGRAM_PG_CORPUS_ALLOW_DROP (file keys apart by `;`), and is then written with that said.
			var allowed = (Environment.GetEnvironmentVariable("DOTGRAM_PG_CORPUS_ALLOW_DROP") ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.Ordinal);
			var dropped = Read()
				.Where(then => counts.TryGetValue(then.Key, out var now) && now.Read < then.Value.Read && !allowed.Contains(then.Key))
				.Select(then => $"{then.Key}: read {counts[then.Key].Read} where the baseline read {then.Value.Read}; name it in DOTGRAM_PG_CORPUS_ALLOW_DROP to record the drop.")
				.ToList();

			Assert.Empty(dropped);
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
			"# What each file of tests/Corpus/PostgreSql/sql, and of dump/ beside it under its own prefix, reads and writes back: `path = read/refused/threw/same`.",
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
