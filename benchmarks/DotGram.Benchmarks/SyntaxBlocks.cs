using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotGram.Benchmarks;

/// <summary>
/// `--syntax sql-docs [output]`: every syntax block of Microsoft's Transact-SQL reference,
/// gathered into one file beside the grammar written from them.
/// </summary>
/// <remarks>
/// <para>
/// Microsoft publishes no grammar for T-SQL. What it publishes is a reference page per
/// statement, and in each page a syntax block or several — one per product, or one per part
/// of the statement. Gathered in one place they are the nearest thing there is to a grammar,
/// and they are what `TransactSql.gram` is written from; with them in the repository, the
/// answer to "why is this rule written so" is a line in a file rather than a page that has
/// since changed.
/// </para>
/// <para>
/// The input is a clone of MicrosoftDocs/sql-docs; `docs/t-sql` and `docs/includes` are what
/// is read, so a sparse one is enough. A block is a `syntaxsql` one, or a plain or `sql` one
/// under a heading that says Syntax — about one page in six writes its syntax that way, and
/// the examples further down are never under such a heading. The documentation's includes
/// are expanded where they stand, whether a line of their own or a product's name inside a
/// sentence.
/// </para>
/// <para>
/// What is kept beside a block is what says which block it is: its page, the heading above
/// it, the sentence before it (which is where a page says "Syntax for SQL Server" or "for
/// Azure Synapse Analytics"), and the product range it is marked for where the page marks
/// one. The commit the clone stood at is written into the header, so that two harvests can
/// be told apart and a later one diffed against this.
/// </para>
/// </remarks>
static class SyntaxBlocks
{
	internal static void Run(string docs, string? output)
	{
		var root = Path.Combine(docs, "docs", "t-sql");

		if (!Directory.Exists(root))
		{
			Console.WriteLine($"{root} does not exist: the first argument is a clone of MicrosoftDocs/sql-docs");
			return;
		}

		var target = output ?? Default();
		var pages  = Directory
			.GetFiles(root, "*.md", SearchOption.AllDirectories)
			.Select(path => (Path: path, Name: Path.GetRelativePath(docs, path).Replace('\\', '/')))
			.OrderBy(static page => page.Name, StringComparer.Ordinal)
			.ToArray();

		var text   = new StringBuilder();
		var read   = 0;
		var blocks = 0;
		var plain  = 0;

		Header(text, Commit(docs));

		foreach (var (path, name) in pages)
		{
			var (title, found) = Read(path);

			if (found.Count == 0)
				continue;

			read++;
			blocks += found.Count;
			plain  += found.Count(static block => !block.Fence.Equals("syntaxsql", StringComparison.OrdinalIgnoreCase));

			text.Append("\r\n## ").Append(title ?? name).Append("\r\n\r\n");
			text.Append('`').Append(name).Append("`\r\n");

			string? heading = null;

			foreach (var block in found)
			{
				if (block.Heading != heading && block.Heading is not null)
					text.Append("\r\n### ").Append(block.Heading).Append("\r\n");

				heading = block.Heading;
				text.Append("\r\n");

				if (block.Moniker is not null)
					text.Append("Marked for `").Append(block.Moniker).Append("`.\r\n\r\n");

				if (block.Lead is not null)
					text.Append("> ").Append(block.Lead).Append("\r\n\r\n");

				text.Append("```").Append(block.Fence).Append("\r\n");

				foreach (var line in block.Lines)
					text.Append(line).Append("\r\n");

				text.Append("```\r\n");
			}
		}

		File.WriteAllText(target, text.ToString(), new UTF8Encoding(false));
		Console.WriteLine(
			$"{blocks} syntax blocks ({plain} of them not marked syntaxsql) from {read} of {pages.Length} pages, into {target}");
	}

	sealed record Block(string? Heading, string? Lead, string? Moniker, string Fence, List<string> Lines);

	/// <summary>An include on a line of its own, or inside a sentence.</summary>
	static readonly Regex Include = new(@"\[!INCLUDE\s*\[[^\]]*\]\(([^)]+)\)\]", RegexOptions.IgnoreCase);

	/// <summary>A page's title and its syntax blocks, each with what stood around it.</summary>
	static (string? Title, List<Block> Blocks) Read(string path)
	{
		var found   = new List<Block>();
		var all     = File.ReadAllLines(path);
		var title   = Title(all);
		var lines   = Expanded(path, all, 0);
		var folder  = Path.GetDirectoryName(path)!;

		string? heading = null;
		string? lead    = null;
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
				var syntax = fence.Equals("syntaxsql", StringComparison.OrdinalIgnoreCase)
					|| (fence.Length == 0 || fence.Equals("sql", StringComparison.OrdinalIgnoreCase))
					   && heading is not null && heading.StartsWith("Syntax", StringComparison.OrdinalIgnoreCase);

				for (at++; at < lines.Count && lines[at].Trim() != "```"; at++)
					body.Add(Unindented(lines[at], indent).TrimEnd());

				if (syntax)
				{
					while (body.Count > 0 && body[0].Length == 0)
						body.RemoveAt(0);

					while (body.Count > 0 && body[^1].Length == 0)
						body.RemoveAt(body.Count - 1);

					if (body.Count > 0)
						found.Add(new Block(heading, lead, moniker, fence, body));
				}

				lead = null;
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
			{
				heading = Inline(trimmed.TrimStart('#').Trim(), folder, 0);
				lead    = null;
				continue;
			}

			if (trimmed.Length > 0)
				lead = Inline(trimmed, folder, 0);
		}

		return (title, found);
	}

	static string? Title(string[] lines)
	{
		if (lines.Length == 0 || lines[0].Trim() != "---")
			return null;

		for (var at = 1; at < lines.Length && lines[at].Trim() != "---"; at++)
		{
			if (lines[at].StartsWith("title:", StringComparison.Ordinal))
				return lines[at]["title:".Length..].Trim().Trim('"', '\'');
		}

		return null;
	}

	/// <summary>A file's lines without its front matter, with includes on a line of their own spliced in.</summary>
	static List<string> Expanded(string path, string[] lines, int depth)
	{
		var result = new List<string>();
		var at     = 0;

		if (lines.Length > 0 && lines[0].Trim() == "---")
		{
			for (at = 1; at < lines.Length && lines[at].Trim() != "---"; at++)
			{
			}

			at++;
		}

		for (; at < lines.Length; at++)
		{
			var match = Include.Match(lines[at].Trim());

			if (depth < 4 && match.Success && match.Length == lines[at].Trim().Length
				&& Resolve(Path.GetDirectoryName(path)!, match.Groups[1].Value) is { } included)
			{
				result.AddRange(Expanded(included, File.ReadAllLines(included), depth + 1));
				continue;
			}

			result.Add(lines[at]);
		}

		return result;
	}

	/// <summary>A sentence with the includes inside it replaced by what they say.</summary>
	static string Inline(string sentence, string folder, int depth) =>
		depth >= 4 ? sentence : Include.Replace(sentence, match =>
		{
			if (Resolve(folder, match.Groups[1].Value) is not { } included)
				return match.Value;

			var said = Expanded(included, File.ReadAllLines(included), depth + 1)
				.Select(static line => line.Trim())
				.FirstOrDefault(static line => line.Length > 0);

			return said is null ? match.Value : Inline(said, Path.GetDirectoryName(included)!, depth + 1);
		});

	static string? Resolve(string folder, string link)
	{
		var file = Path.GetFullPath(Path.Combine(folder, link.Split('#')[0]));

		return File.Exists(file) ? file : null;
	}

	static string Unindented(string line, int indent)
	{
		var spaces = 0;

		while (spaces < indent && spaces < line.Length && line[spaces] == ' ')
			spaces++;

		return line[spaces..];
	}

	static void Header(StringBuilder text, string commit)
	{
		text.Append("# Transact-SQL syntax, as Microsoft publishes it\r\n\r\n");
		text.Append("Every syntax block of the Transact-SQL reference in\r\n");
		text.Append("[MicrosoftDocs/sql-docs](https://github.com/MicrosoftDocs/sql-docs), `docs/t-sql`, at\r\n");
		text.Append(commit).Append(", gathered by `--syntax`\r\n");
		text.Append("(`benchmarks/DotGram.Benchmarks/SyntaxBlocks.cs`): the `syntaxsql` blocks, and a plain or\r\n");
		text.Append("`sql` one under a heading that says Syntax.\r\n\r\n");
		text.Append("© Microsoft Corporation. The documentation is licensed under\r\n");
		text.Append("[Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/).\r\n");
		text.Append("Changed from the original: only the syntax blocks are kept, each with its page's title and\r\n");
		text.Append("path, the heading above it, the sentence before it, and the product range the page marks it\r\n");
		text.Append("for; the documentation's includes are expanded in place, and trailing whitespace is\r\n");
		text.Append("removed. Nothing inside a block is otherwise altered.\r\n\r\n");
		text.Append("Microsoft publishes no grammar for T-SQL, and this is the nearest thing to one. It is what\r\n");
		text.Append("`TransactSql.gram` is written from — and where a block and SQL Server disagree, the engine\r\n");
		text.Append("is what the grammar follows.\r\n");
	}

	/// <summary>The commit the clone stands at and its date, or a line saying it is not known.</summary>
	static string Commit(string docs)
	{
		try
		{
			using var git = Process.Start(new ProcessStartInfo("git", $"-C \"{docs}\" log -1 --format=%H%x20%cs")
			{
				RedirectStandardOutput = true,
				UseShellExecute        = false,
			})!;

			var said = git.StandardOutput.ReadToEnd().Trim().Split(' ');
			git.WaitForExit();

			return said.Length == 2 ? $"commit `{said[0]}` ({said[1]})" : "a commit not recorded";
		}
		catch (System.ComponentModel.Win32Exception)
		{
			return "a commit not recorded";
		}
	}

	/// <summary>Beside the grammar, found by walking up to the solution.</summary>
	static string Default()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		var root = at?.FullName ?? ".";

		return Path.Combine(root, "src", "DotGram.Parsers", "Sql", "TransactSql", "Specification", "syntax.md");
	}
}
