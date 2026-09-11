using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Parsers.Sql;

using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// `--coverage sql-docs [output]`: how much of Microsoft's Transact-SQL reference this grammar
/// reads — every example on every page, asked of the engine and of this grammar at every
/// compatibility level, and written up page by page.
/// </summary>
/// <remarks>
/// <para>
/// The corpus is what ScriptDom's authors thought to test, and a statement nobody tested is
/// not in it: `--engine` says how much of the corpus is read — of the kinds this grammar has
/// a rule for — and nothing about the rest of the language. The reference is the other half:
/// a page per statement, per function, per clause, and on most of them examples written by
/// the people who wrote the engine. Asked of the engine, an example says what the page's
/// language is; asked of this grammar beside it, it says whether the page is covered.
/// </para>
/// <para>
/// An example is a `sql` block anywhere but under a heading that says Syntax, which is where
/// `--syntax` looks, so the two never take the same block. It is cut into statements by
/// ScriptDom, and a block ScriptDom cannot cut is asked whole, one batch at a time. Each
/// statement goes to every level from 100 to 170 under `SET PARSEONLY ON`, and to this
/// grammar's parser for the same level. Anything that names `PARSEONLY` is left out: the
/// reference has `SET PARSEONLY OFF` on its page, and after it the rest of the reference would
/// be run on this machine rather than read. And a connection a `USE` moved to another database
/// is moved back before the next statement, since a level is a database's.
/// </para>
/// <para>
/// The latest level decides the row, as `--engine` decides it: read by both, the work list, a
/// defect here, another product's — by the product range the page or the example is marked
/// for, or by the engine's own answer — or neither. A statement both read at 170 whose answers
/// part at an older level is counted apart, as the work the version axis still has. Beside the
/// pages, the whole reference's work list by the statement it begins with, and what the
/// engine answered the defects, message by message.
/// </para>
/// </remarks>
static class Coverage
{
	static readonly string[] Asked = ["100", "110", "120", "130", "140", "150", "160", "170"];

	/// <summary>A line that ends a batch: `GO`, a count after it, a semicolon.</summary>
	static readonly Regex Go = new(@"^\s*GO(\s+\d+)?\s*;?\s*$", RegexOptions.IgnoreCase);

	/// <summary>A comment, of either kind, which is not what a statement begins with.</summary>
	static readonly Regex Comment = new(@"--[^\n]*|/\*.*?\*/", RegexOptions.Singleline);

	/// <summary>
	/// A statement that sets an option of the session — `SET QUOTED_IDENTIFIER OFF` — which the
	/// engine honours under `PARSEONLY` too, and which would change how everything after it is
	/// read: the reference's page sets it off to show a statement failing.
	/// </summary>
	static readonly Regex SetsOption = new(@"(^|\n)\s*SET\s+(?!@)", RegexOptions.IgnoreCase);

	/// <summary>A word after `CREATE`, `ALTER` or `DROP` that is half of what is created.</summary>
	static readonly HashSet<string> Compound = new(StringComparer.Ordinal)
	{
		"ASYMMETRIC", "AVAILABILITY", "BROKER", "COLUMN", "CRYPTOGRAPHIC", "DATABASE", "EVENT",
		"EXTERNAL", "FULLTEXT", "MASTER", "MESSAGE", "PARTITION", "REMOTE", "RESOURCE", "SEARCH",
		"SECURITY", "SELECTIVE", "SERVER", "SPATIAL", "SYMMETRIC", "WORKLOAD", "XML",
	};

	sealed class Tally
	{
		public int Statements, Both, Work, Defects, Levels, Other, Neither, Skipped;

		public string? FirstWork, FirstDefect, FirstLevel;

		public void Add(Tally one)
		{
			Statements += one.Statements;
			Both       += one.Both;
			Work       += one.Work;
			Defects    += one.Defects;
			Levels     += one.Levels;
			Other      += one.Other;
			Neither    += one.Neither;
			Skipped    += one.Skipped;
		}

		/// <summary>The share of what the engine reads, or this grammar reads for it, that both read.</summary>
		public string Read => Both + Work + Defects == 0 ? "—" : $"{100.0 * Both / (Both + Work + Defects):0.0}%";
	}

	/// <summary>What is gathered across pages: the work by statement, the defects by message.</summary>
	sealed class Findings
	{
		public readonly Dictionary<string, (int Count, HashSet<string> Pages, string Like)> Missing = new(StringComparer.Ordinal);

		public readonly Dictionary<int, (int Count, string Like)> Refused = new();

		public int Moved, Reset;
	}

	sealed record Example(string? Moniker, string Text);

	sealed record Page(string Section, string Title, string Name, Tally Tally);

	internal static void Run(string docs, string? output)
	{
		var root = Path.Combine(docs, "docs", "t-sql");

		if (!Directory.Exists(root))
		{
			Console.WriteLine($"{root} does not exist: the first argument is a clone of MicrosoftDocs/sql-docs");
			return;
		}

		var connections = new List<SqlConnection>();

		try
		{
			foreach (var level in Asked)
			{
				if (Engine.Connected(level) is not { } connection)
					return;

				Engine.ParseOnly(connection);
				connections.Add(connection);
			}

			Measure(docs, root, connections, output ?? Default());
		}
		finally
		{
			foreach (var connection in connections)
				connection.Dispose();
		}
	}

	static void Measure(string docs, string root, List<SqlConnection> connections, string target)
	{
		var splitter  = Kinds.Version("170")!;
		var databases = connections.Select(static connection => connection.Database).ToArray();
		var findings  = new Findings();
		var pages     = new List<Page>();
		var files     = Directory
			.GetFiles(root, "*.md", SearchOption.AllDirectories)
			.Select(path => (Path: path, Name: Path.GetRelativePath(docs, path).Replace('\\', '/')))
			.OrderBy(static page => page.Name, StringComparer.Ordinal)
			.ToArray();

		foreach (var (path, name) in files)
		{
			var (title, examples) = Examples(path);

			if (examples.Count == 0)
				continue;

			var tally = new Tally();

			foreach (var example in examples)
				foreach (var (text, whole) in Statements(splitter, example.Text))
					Ask(text, whole, Other(example.Moniker), connections, databases, tally, findings, name);

			if (tally.Statements + tally.Skipped > 0)
				pages.Add(new Page(Section(name), title ?? name, name, tally));

			Console.Write($"\r  {pages.Count} pages");
		}

		Console.WriteLine();
		Write(target, SyntaxBlocks.Commit(docs), files.Length, pages, findings);
	}

	/// <summary>A page's title and its examples, each with the product range it is marked for.</summary>
	static (string? Title, List<Example> Examples) Examples(string path)
	{
		var found = new List<Example>();
		var all   = File.ReadAllLines(path);
		var title = SyntaxBlocks.Title(all);
		var page  = Range(all);
		var lines = SyntaxBlocks.Expanded(path, all, 0);

		string? heading = null;
		string? moniker = null;

		for (var at = 0; at < lines.Count; at++)
		{
			var line    = lines[at];
			var trimmed = line.Trim();

			if (trimmed.StartsWith("```", StringComparison.Ordinal))
			{
				var indent = line.Length - line.TrimStart().Length;
				var fence  = trimmed[3..].Trim();
				var body   = new List<string>();

				for (at++; at < lines.Count && lines[at].Trim() != "```"; at++)
					body.Add(SyntaxBlocks.Unindented(lines[at], indent).TrimEnd());

				var sql    = fence is "sql" or "SQL" or "tsql" or "t-sql" or "TSQL";
				var syntax = heading is not null && heading.StartsWith("Syntax", StringComparison.OrdinalIgnoreCase);

				if (sql && !syntax && body.Any(static one => one.Trim().Length > 0))
					found.Add(new Example(moniker ?? page, string.Join("\n", body)));

				continue;
			}

			if (trimmed.StartsWith("::: moniker range=", StringComparison.Ordinal))
			{
				var open  = trimmed.IndexOf('"');
				var close = trimmed.LastIndexOf('"');

				moniker = open >= 0 && close > open ? trimmed[(open + 1)..close] : trimmed;
				continue;
			}

			if (trimmed.StartsWith(":::", StringComparison.Ordinal))
			{
				moniker = null;
				continue;
			}

			if (trimmed.StartsWith('#'))
				heading = trimmed.TrimStart('#').Trim();
		}

		return (title, found);
	}

	/// <summary>The product range a page's front matter marks the whole page for.</summary>
	static string? Range(string[] lines)
	{
		if (lines.Length == 0 || lines[0].Trim() != "---")
			return null;

		for (var at = 1; at < lines.Length && lines[at].Trim() != "---"; at++)
		{
			if (lines[at].StartsWith("monikerRange:", StringComparison.OrdinalIgnoreCase))
				return lines[at]["monikerRange:".Length..].Trim().Trim('"', '\'');
		}

		return null;
	}

	/// <summary>Whether a range leaves SQL Server out, which makes what it marks another product's.</summary>
	static bool Other(string? moniker) =>
		moniker is not null && moniker.IndexOf("sql-server", StringComparison.OrdinalIgnoreCase) < 0;

	/// <summary>The directory of the reference a page is in: `statements`, `functions`, `queries`.</summary>
	static string Section(string name)
	{
		var parts = name.Split('/');

		return parts.Length > 3 ? parts[2] : "reference";
	}

	/// <summary>An example as statements: ScriptDom's cut where it can make one, and batches where not.</summary>
	static List<(string Text, bool Whole)> Statements(TSqlParser splitter, string text)
	{
		var cut = new List<(string, bool)>();

		using (var reader = new StringReader(text))
		{
			if (splitter.Parse(reader, out var errors) is TSqlScript script && errors.Count == 0)
			{
				foreach (var statement in script.Batches.SelectMany(static batch => batch.Statements))
				{
					var one = text.Substring(statement.StartOffset, statement.FragmentLength).TrimEnd();

					if (one.Length > 0)
						cut.Add((one, false));
				}

				return cut;
			}
		}

		var batch = new StringBuilder();

		foreach (var line in text.Split('\n'))
		{
			if (Go.IsMatch(line))
			{
				if (batch.ToString().Trim() is { Length: > 0 } done)
					cut.Add((done, true));

				batch.Clear();
				continue;
			}

			batch.Append(line.TrimEnd('\r')).Append('\n');
		}

		if (batch.ToString().Trim() is { Length: > 0 } last)
			cut.Add((last, true));

		return cut;
	}

	/// <summary>One statement asked of every level and of this grammar at each, and counted.</summary>
	static void Ask(
		string text, bool whole, bool other, List<SqlConnection> connections, string[] databases,
		Tally tally, Findings findings, string page)
	{
		if (text.Contains("PARSEONLY", StringComparison.OrdinalIgnoreCase))
		{
			tally.Skipped++;
			return;
		}

		tally.Statements++;

		var engine  = new bool[Asked.Length];
		var here    = new bool[Asked.Length];
		var message = 0;
		var at      = 0;

		for (var level = 0; level < Asked.Length; level++)
		{
			message       = Engine.Answer(connections[level], text);
			engine[level] = message == 0 || Engine.AboutNames(message);

			if (connections[level].Database != databases[level])
			{
				connections[level].ChangeDatabase(databases[level]);
				findings.Moved++;
			}

			var (read, stopped) = whole ? Engine.ParseText(Asked[level], text) : Engine.Parse(Asked[level], text);

			here[level] = read;
			at          = stopped;
		}

		// An option set stays set on the connection, and the next statement would be asked as the
		// page left the session rather than as a statement on its own. A connection opened again
		// comes back from the pool with its options reset.
		if (SetsOption.IsMatch(text))
		{
			foreach (var connection in connections)
			{
				connection.Close();
				connection.Open();
				Engine.ParseOnly(connection);
			}

			findings.Reset++;
		}

		var last = Asked.Length - 1;

		if (engine[last] && here[last])
		{
			tally.Both++;

			if (!engine.SequenceEqual(here))
			{
				tally.Levels++;
				tally.FirstLevel ??= $"levels {Shape(engine, here)} — `{Code(text)}`";
			}
		}
		else if (engine[last])
		{
			var stops = $"stops at {Corpus.Stopped(text, at)} — `{Code(text)}`";

			tally.Work++;
			tally.FirstWork ??= stops;

			var family = Family(text);
			var (count, of, like) = findings.Missing.TryGetValue(family, out var before)
				? before
				: (0, new HashSet<string>(StringComparer.Ordinal), stops);

			of.Add(page);
			findings.Missing[family] = (count + 1, of, like);
		}
		else if (here[last] && (other || Engine.Elsewhere(text, message)))
		{
			tally.Other++;
		}
		else if (here[last])
		{
			tally.Defects++;
			tally.FirstDefect ??= $"read here, Msg {message} there — `{Code(text)}`";

			findings.Refused[message] = findings.Refused.TryGetValue(message, out var seen)
				? (seen.Count + 1, seen.Like)
				: (1, $"`{Code(text)}`");
		}
		else
		{
			tally.Neither++;
		}
	}

	/// <summary>What a statement begins with, which names what it is: `DBCC`, `CREATE PARTITION FUNCTION`.</summary>
	static string Family(string text)
	{
		var words = Regex.Matches(Comment.Replace(text, " "), "[A-Za-z_]+")
			.Select(static match => match.Value.ToUpperInvariant())
			.Take(3)
			.ToArray();

		if (words.Length == 0)
			return "(no word)";

		var take = words[0] is "CREATE" or "ALTER" or "DROP"
			? words.Length > 2 && Compound.Contains(words[1]) ? 3 : 2
			: 1;

		return string.Join(' ', words.Take(Math.Min(take, words.Length)));
	}

	/// <summary>Which levels the two part at: the engine's answer and this grammar's, level by level.</summary>
	static string Shape(bool[] engine, bool[] here)
	{
		var parted = new List<string>();

		for (var level = 0; level < Asked.Length; level++)
		{
			if (engine[level] != here[level])
				parted.Add(Asked[level] + (engine[level] ? " engine" : " here"));
		}

		return string.Join(", ", parted);
	}

	/// <summary>A statement on one line, fit for a code span in a table.</summary>
	static string Code(string text) => Corpus.One(text).Replace('`', '\'').Replace("|", "\\|");

	static string Cell(string text) => text.Replace("|", "\\|");

	static void Write(string target, string commit, int files, List<Page> pages, Findings findings)
	{
		var all = new Tally();

		foreach (var page in pages)
			all.Add(page.Tally);

		var sections = pages
			.GroupBy(static page => page.Section)
			.OrderBy(static section => section.Key, StringComparer.Ordinal)
			.ToArray();

		var text = new StringBuilder();

		text.Append("# How much of the T-SQL reference the grammar reads\r\n\r\n");
		text.Append("Every example of every page of the Transact-SQL reference in\r\n");
		text.Append("[MicrosoftDocs/sql-docs](https://github.com/MicrosoftDocs/sql-docs), `docs/t-sql`, at\r\n");
		text.Append(commit).Append(", cut into statements and asked of SQL Server 2025 under\r\n");
		text.Append("`SET PARSEONLY ON` at every compatibility level from 100 to 170, and of\r\n");
		text.Append("`TransactSql.gram`'s parser for the same level. Written by `--coverage`\r\n");
		text.Append("(`benchmarks/DotGram.Benchmarks/Coverage.cs`); run again rather than edited.\r\n\r\n");
		text.Append("© Microsoft Corporation. The documentation is licensed under\r\n");
		text.Append("[Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/).\r\n");
		text.Append("Changed from the original: of each page only its title, its path and one statement of its\r\n");
		text.Append("examples, flattened to a line, are kept.\r\n\r\n");
		text.Append("The latest level decides a statement's column. **Both** read it. **Work**: the engine\r\n");
		text.Append("reads it and the grammar does not. **Defects**: the grammar reads what the engine refuses.\r\n");
		text.Append("**Levels**: both read it at 170 and part at an older level. **Other**: the grammar reads\r\n");
		text.Append("it, and it is another product's, by the range the page or the example is marked for or\r\n");
		text.Append("by the engine's answer. **Neither** reads it. **Read** is both over both, work and\r\n");
		text.Append("defects. What names `PARSEONLY` is left out, since `SET PARSEONLY OFF` would have the\r\n");
		text.Append("rest run rather than read.\r\n\r\n");

		text.Append($"{pages.Count} of {files} pages have examples.");

		if (findings.Moved > 0)
			text.Append($" A `USE` moved a connection to another database {findings.Moved} times, and it was moved back.");

		if (findings.Reset > 0)
			text.Append($" {findings.Reset} statements set an option of the session, and the connections were opened again after each.");

		text.Append("\r\n\r\n");
		text.Append("| Section | pages | statements | read | both | work | defects | levels | other | neither | left out |\r\n");
		text.Append("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |\r\n");
		Total(text, "**all**", pages.Count, all);

		foreach (var section in sections)
		{
			var tally = new Tally();

			foreach (var page in section)
				tally.Add(page.Tally);

			Total(text, $"[{section.Key}](#{section.Key})", section.Count(), tally);
		}

		text.Append("\r\n## The work list, by what a statement begins with\r\n\r\n");
		text.Append("| Statement | statements | pages | for example |\r\n");
		text.Append("| --- | ---: | ---: | --- |\r\n");

		foreach (var (family, (count, of, like)) in findings.Missing
			.OrderByDescending(static one => one.Value.Count)
			.ThenBy(static one => one.Key, StringComparer.Ordinal))
		{
			text.Append("| ").Append(Cell(family)).Append(" | ")
				.Append(count).Append(" | ")
				.Append(of.Count).Append(" | ")
				.Append(like).Append(" |\r\n");
		}

		text.Append("\r\n## What the engine answered the defects\r\n\r\n");
		text.Append("| Message | statements | for example |\r\n");
		text.Append("| --- | ---: | --- |\r\n");

		foreach (var (message, (count, like)) in findings.Refused
			.OrderByDescending(static one => one.Value.Count)
			.ThenBy(static one => one.Key))
		{
			text.Append("| ").Append(message).Append(" | ")
				.Append(count).Append(" | ")
				.Append(like).Append(" |\r\n");
		}

		foreach (var section in sections)
		{
			text.Append("\r\n## ").Append(section.Key).Append("\r\n\r\n");
			text.Append("| Page | statements | read | work | defects | levels | other | first thing to do |\r\n");
			text.Append("| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |\r\n");

			foreach (var page in section
				.OrderByDescending(static page => page.Tally.Work + page.Tally.Defects + page.Tally.Levels)
				.ThenBy(static page => page.Name, StringComparer.Ordinal))
			{
				var tally = page.Tally;
				var first = tally.FirstWork ?? tally.FirstDefect ?? tally.FirstLevel ?? "";

				text.Append("| ").Append(Cell(page.Title)).Append(" | ")
					.Append(tally.Statements).Append(" | ")
					.Append(tally.Read).Append(" | ")
					.Append(tally.Work).Append(" | ")
					.Append(tally.Defects).Append(" | ")
					.Append(tally.Levels).Append(" | ")
					.Append(tally.Other).Append(" | ")
					.Append(first).Append(" |\r\n");
			}
		}

		File.WriteAllText(target, text.ToString(), new UTF8Encoding(false));
		Console.WriteLine(
			$"{all.Statements} statements from {pages.Count} pages: {all.Both} read by both ({all.Read}), " +
			$"{all.Work} work, {all.Defects} defects, {all.Levels} parting at a level, {all.Other} another product's, " +
			$"{all.Neither} neither, {all.Skipped} left out, {findings.Moved} moved back, {findings.Reset} options reset; into {target}");
	}

	static void Total(StringBuilder text, string name, int pages, Tally tally) =>
		text.Append("| ").Append(name).Append(" | ")
			.Append(pages).Append(" | ")
			.Append(tally.Statements).Append(" | ")
			.Append(tally.Read).Append(" | ")
			.Append(tally.Both).Append(" | ")
			.Append(tally.Work).Append(" | ")
			.Append(tally.Defects).Append(" | ")
			.Append(tally.Levels).Append(" | ")
			.Append(tally.Other).Append(" | ")
			.Append(tally.Neither).Append(" | ")
			.Append(tally.Skipped).Append(" |\r\n");

	/// <summary>Beside the grammar's documentation, found by walking up to the solution.</summary>
	static string Default()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return Path.Combine(at?.FullName ?? ".", "docs", "coverage.md");
	}
}
