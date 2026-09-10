using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Parsers.Sql;

namespace DotGram.Benchmarks;

/// <summary>
/// What <see cref="SqlStandard92"/> makes of somebody else's corpus.
/// </summary>
/// <remarks>
/// <para>
/// Every test in this repository was written here, which is the one thing wrong with all of
/// them: a grammar and its tests written by the same hand agree about what the language is.
/// Microsoft's ScriptDom ships a T-SQL parser with about eleven hundred <c>.sql</c> files
/// behind it, written by people who had never heard of this one, and what they hold is real
/// SQL with real corners in it.
/// </para>
/// <para>
/// <b>It is not an oracle and this is not an agreement test.</b> The corpus is T-SQL and
/// this grammar is standard SQL-92, so a statement ScriptDom reads and this one refuses is
/// usually a dialect and not a defect. What the harness gives is the other direction: a
/// refusal grouped by what was expected where it stopped, which sorts the dialect from the
/// hole. `TOP 10` and `[bracketed]` are T-SQL; a missing `§7` production is ours.
/// </para>
/// <para>
/// Statements are cut out by hand — batches at <c>GO</c>, statements at <c>;</c> — because
/// splitting T-SQL properly needs a T-SQL parser, which is the thing being compared against.
/// What comes out is query-shaped fragments and nothing else claims to be exact.
/// </para>
/// </remarks>
static class Corpus
{
	public static void Run(string? root, int shown)
	{
		root ??= Checked();

		if (!Directory.Exists(root))
		{
			Console.WriteLine($"No corpus at {root}.");

			return;
		}

		var statements = new List<string>();

		var files = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);

		foreach (var file in files)
			statements.AddRange(Queries(File.ReadAllText(file)));

		// Both, and in that order: the standard is what the dialect is a dialect of,
		// and the only number worth anything here is the difference between them.
		Read("SQL-92", files.Length, statements, shown, static one =>
		{
			var match = SqlStandard92.TryParseSelect(one);

			return match.IsSuccess ? -1 : (int)match.Position;
		});

		Read("T-SQL", files.Length, statements, shown, static one =>
		{
			var match = TransactSql.TryParseSelect(one);

			return match.IsSuccess ? -1 : (int)match.Position;
		});
	}

	/// <summary>What one parser made of the statements, and where it stopped.</summary>
	static void Read(
		string name, int files, List<string> statements, int shown, Func<string, int> reader)
	{
		var read    = 0;
		var counted = new Dictionary<string, int>(StringComparer.Ordinal);
		var like    = new Dictionary<string, List<string>>(StringComparer.Ordinal);

		foreach (var one in statements)
		{
			var stopped = reader(one);

			if (stopped < 0)
			{
				read++;

				continue;
			}

			// What stood where the reading stopped, which names the feature: `TOP`,
			// `APPLY`, `OVER`, `[`. The message cannot — over kinds an expectation is a
			// token kind, and a kind is a character nobody wrote.
			var why = Stopped(one, stopped);

			counted[why] = counted.TryGetValue(why, out var already) ? already + 1 : 1;

			if (!like.TryGetValue(why, out var some))
				like[why] = some = [];

			if (some.Count < shown)
				some.Add(One(one));
		}

		Console.WriteLine();
		Console.WriteLine(
			$"{name,-6}  {files} files, {statements.Count} query-shaped statements, " +
			$"{read} read ({(statements.Count == 0 ? 0 : 100.0 * read / statements.Count):F1}%)");
		Console.WriteLine();

		foreach (var (why, count) in counted.OrderByDescending(one => one.Value).ThenBy(one => one.Key))
		{
			Console.WriteLine($"  {count,5}  stops at {why}");

			foreach (var one in like[why].Take(shown))
				Console.WriteLine($"           {one}");
		}
	}

	/// <summary>
	/// The query-shaped statements of one script: batches at <c>GO</c>, statements at
	/// <c>;</c>, and what is left that begins the way a query does.
	/// </summary>
	static IEnumerable<string> Queries(string script)
	{
		foreach (var batch in Batches(script))
			foreach (var statement in batch.Split(';'))
			{
				var one = statement.Trim();

				if (one.Length == 0)
					continue;

				var word = Word(one);

				if (word is "SELECT" or "VALUES" or "TABLE" || one[0] == '(')
					yield return one;
			}
	}

	/// <summary>A script's batches, with the comment lines taken out of them.</summary>
	/// <remarks>
	/// Line comments only. A block comment can hold a <c>GO</c> or a semicolon and cutting
	/// one out properly is reading it, which is what the parser being measured does — so a
	/// statement with one in it is measured with it, and the parser reads it or says so.
	/// </remarks>
	static IEnumerable<string> Batches(string script)
	{
		var batch = new StringBuilder();

		foreach (var line in script.Split('\n'))
		{
			var one = line.TrimEnd('\r');

			if (one.TrimStart().StartsWith("--", StringComparison.Ordinal))
				continue;

			if (string.Equals(one.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
			{
				yield return batch.ToString();

				batch.Clear();

				continue;
			}

			batch.Append(one).Append('\n');
		}

		yield return batch.ToString();
	}

	/// <summary>What stands where the reading stopped: a word, or the character itself.</summary>
	internal static string Stopped(string statement, int at)
	{
		while (at < statement.Length && char.IsWhiteSpace(statement[at]))
			at++;

		if (at >= statement.Length)
			return "the end";

		if (!char.IsLetter(statement[at]) && statement[at] != '_')
			return "'" + statement[at] + "'";

		var to = at;

		while (to < statement.Length && (char.IsLetterOrDigit(statement[to]) || statement[to] == '_'))
			to++;

		return statement.Substring(at, to - at).ToUpperInvariant();
	}

	/// <summary>One statement on one line, short enough to read in a list.</summary>
	internal static string One(string statement)
	{
		var flat = string.Join(" ", statement.Split('\n').Select(one => one.Trim()));

		return flat.Length <= 86 ? flat : flat.Substring(0, 83) + "...";
	}

	/// <summary>The first word of a statement, upper-cased, or null where it has none.</summary>
	static string? Word(string statement)
	{
		var at = 0;

		while (at < statement.Length && char.IsLetter(statement[at]))
			at++;

		return at == 0 ? null : statement.Substring(0, at).ToUpperInvariant();
	}

	/// <summary>Which version of T-SQL each file is written in, where the corpus says so.</summary>
	/// <remarks>
	/// <para>
	/// The <c>Baselines&lt;n&gt;</c> directories are Microsoft's own partition of the language
	/// by parser version, and reading the whole corpus with one parser throws that away in
	/// both directions: <c>*=</c>, <c>DUMP</c> and <c>DISABLE_DEF_CNST_CHK</c> were taken out
	/// of the language and only an old parser reads them, while
	/// <c>AUTOMATIC_INDEX_COMPACTION</c> arrived in 180 and only a new one does. Fifteen
	/// files were being called unreadable for no better reason than that.
	/// </para>
	/// <para>
	/// A file outside those directories takes the version of the file of the same name that
	/// is inside one — <c>TestScripts/</c> is upstream's union and every version-sensitive
	/// file in it has its twin — and anything still unnamed takes the version asked for.
	/// </para>
	/// </remarks>
	internal static Dictionary<string, string> Versions(string root, IEnumerable<string> files)
	{
		var byFile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var byName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		foreach (var file in files)
			if (Partition(root, file) is { } version)
			{
				byFile[file] = version;
				byName[Path.GetFileName(file)] = version;
			}

		foreach (var file in files)
			if (!byFile.ContainsKey(file) && byName.TryGetValue(Path.GetFileName(file), out var twin))
				byFile[file] = twin;

		return byFile;
	}

	/// <summary>The version a <c>Baselines&lt;n&gt;</c> directory names, or null for anything else.</summary>
	static string? Partition(string root, string file)
	{
		var inside = file.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		var first  = inside.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];

		return first.StartsWith("Baselines", StringComparison.Ordinal) &&
			first["Baselines".Length..] is { Length: > 0 } named &&
			named.All(char.IsAsciiDigit)
				? named
				: null;
	}

	/// <summary>The copy kept in the repository, which is what `--corpus` reads by default.</summary>
	/// <remarks>
	/// Found by walking up to the solution rather than by a path relative to the binary:
	/// the working directory a benchmark is launched from is whatever the launcher felt
	/// like, and `bin/Release/net10.0` is not where the corpus lives.
	/// </remarks>
	internal static string Checked()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at is null
			? Path.Combine("tests", "Corpus", "ScriptDom")
			: Path.Combine(at.FullName, "tests", "Corpus", "ScriptDom");
	}
}
