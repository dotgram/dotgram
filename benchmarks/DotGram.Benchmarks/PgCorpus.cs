using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Sql.Ast;
using DotGram.Sql.PostgreSql;

namespace DotGram.Benchmarks;

/// <summary>
/// What <see cref="PostgreSqlParser"/> makes of PostgreSQL's own regression suite (<c>src/test/regress/sql</c>):
/// the statements cut as psql cuts them, each read, written back by <see cref="PostgreSqlWriter"/> and read again.
/// </summary>
/// <remarks>
/// <para>
/// The suite is a script for psql, not text a program sends: statements end at <c>;</c> outside strings, dollar
/// quotes, comments and parentheses; a line of <c>\…</c> is psql's and not the server's; <c>COPY … FROM stdin;</c> is
/// followed by data lines up to <c>\.</c> that belong to no statement; a <c>BEGIN ATOMIC … END</c> body holds
/// semicolons psql counts past; and <c>:name</c>, <c>:'name'</c> and <c>:"name"</c> are psql's variables, which make
/// a statement no SQL until psql has substituted them. The cutter here is psqlscan.l's reading of those, and the
/// statements with variables are counted apart and read by nobody.
/// </para>
/// <para>
/// <b>It is not an oracle.</b> The suite holds every kind of statement the server has, so a refusal is a construct not
/// written yet far more often than a defect; what the harness gives is the work list — refusals by the statement's
/// first words — and the four cells per file that the slow test holds to a baseline, never worse.
/// </para>
/// </remarks>
internal static class PgCorpus
{
	/// <summary>One file's four cells: read, refused, the writer threw or printed what does not read back, came back the same.</summary>
	internal sealed record Counts(int Read, int Refused, int Threw, int Same);

	/// <summary>A statement as psql would send it, and the line it begins on.</summary>
	internal sealed record Statement(string Text, int Line, bool HasVariable);

	/// <summary>What the cutter made of a script.</summary>
	internal sealed record Cut(IReadOnlyList<Statement> Statements, int Directives, int DataLines);

	internal sealed record Trouble(string File, int Line, string Kind, string Statement, string Why);

	internal sealed record Result(Dictionary<string, Counts> ByFile, Dictionary<string, int> RefusedByKind, Dictionary<string, int> ReadByKind, int Variables);

	static readonly Regex CopyFromStdin = new(@"^\s*COPY\b[\s\S]*?\bFROM\s+STDIN\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	/// <summary>The comments a statement begins with, which say nothing of its kind.</summary>
	static readonly Regex LeadingCommentsStripped = new(@"^(\s*(--[^\n]*\n?|/\*[\s\S]*?\*/))+", RegexOptions.Compiled);

	// ── The cutter ─────────────────────────────────────────────────────────────

	/// <summary>The backslash commands that send the query buffer, by their exact names (psql's command.c).</summary>
	static readonly HashSet<string> Sending = new(StringComparer.Ordinal) { "g", "gx", "gset", "gexec", "gdesc", "bind", "bind_named", "sendpipeline", "parse", "crosstabview", "watch" };

	/// <summary>The commands that discard the query buffer.</summary>
	static readonly HashSet<string> Discarding = new(StringComparer.Ordinal) { "r", "reset" };

	/// <summary>psql's own variables, defined without a <c>\set</c>.</summary>
	static readonly HashSet<string> BuiltIn = new(StringComparer.Ordinal)
	{
		"DBNAME", "ENCODING", "ERROR", "HOST", "LAST_ERROR_MESSAGE", "LAST_ERROR_SQLSTATE", "PORT", "ROW_COUNT", "SERVER_VERSION_NAME",
		"SERVER_VERSION_NUM", "SHELL_ERROR", "SHELL_EXIT_CODE", "SQLSTATE", "USER", "VERSION", "VERSION_NAME", "VERSION_NUM",
	};

	static readonly Regex StandardConformingStrings = new(@"^\s*SET\s+(SESSION\s+|LOCAL\s+)?standard_conforming_strings\s*(=|TO)\s*'?(?<value>\w+)'?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	static readonly Regex ResetConformingStrings    = new(@"^\s*RESET\s+standard_conforming_strings\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	static readonly Regex CopyCommandFromStdin      = new(@"^\\copy\b[\s\S]*?\bfrom\s+p?stdin\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	/// <summary>
	/// Cuts a psql script into the statements psql would send, as psqlscan.l reads it: statements end at <c>;</c> (or
	/// <c>\;</c>) outside strings, quoted names, dollar quotes, comments and parentheses, and outside a <c>BEGIN ATOMIC</c>
	/// body; a backslash command runs to the end of its line, or to a <c>\\</c>; the ones that send the buffer send it,
	/// <c>\r</c> drops it, <c>\set</c> defines a variable, <c>\copy … from stdin</c> swallows its data to <c>\.</c>, as
	/// <c>COPY … FROM stdin;</c> does; and a plain string reads backslashes as escapes while
	/// <c>standard_conforming_strings</c> is off.
	/// </summary>
	internal static Cut CutScript(string text)
	{
		var statements = new List<Statement>();
		var buffer     = new StringBuilder();
		var defined    = new HashSet<string>(BuiltIn, StringComparer.Ordinal);
		var directives = 0;
		var dataLines  = 0;
		var line       = 1;
		var startLine  = 1;
		var parens     = 0;
		var begins     = 0;      // BEGIN ATOMIC … END nesting, with CASE … END inside it
		var lastWord   = "";
		var variable   = false;
		var blank      = true;   // nothing but spacing in the buffer so far, so the next content begins the statement
		var escapes    = false;  // standard_conforming_strings = off: a plain string reads backslash escapes
		var at         = 0;

		void Take(int from, int length)
		{
			if (blank)
			{
				for (var i = from; i < from + length; i++)
				{
					if (!char.IsWhiteSpace(text[i]))
					{
						blank     = false;
						startLine = line;
						break;
					}
				}
			}

			buffer.Append(text, from, length);
		}

		void SkipCopyData()
		{
			var end = text.IndexOf('\n', at);

			at = end < 0 ? text.Length : end + 1;

			if (end >= 0)
				line++;

			while (at < text.Length)
			{
				var next = text.IndexOf('\n', at);
				var row  = text.Substring(at, (next < 0 ? text.Length : next) - at).TrimEnd('\r');

				at = next < 0 ? text.Length : next + 1;
				line++;

				if (row == "\\.")
					break;

				dataLines++;
			}
		}

		void Flush()
		{
			var statement = buffer.ToString().Trim();

			buffer.Clear();

			if (statement.Length > 0)
			{
				statements.Add(new Statement(statement, startLine, variable));

				if (StandardConformingStrings.Match(statement) is { Success: true } setting)
					escapes = setting.Groups["value"].Value.ToLowerInvariant() is "off" or "false" or "0" or "no";
				else if (ResetConformingStrings.IsMatch(statement))
					escapes = false;
			}

			variable = false;
			blank    = true;
			parens   = 0;
			begins   = 0;
			lastWord = "";
		}

		while (at < text.Length)
		{
			var c = text[at];

			// A line comment: dropped while nothing of a statement has begun, kept inside one; a block comment
			// (nested), a string, a quoted name, a dollar quote: copied whole.
			if (c == '-' && at + 1 < text.Length && text[at + 1] == '-')
			{
				var end = text.IndexOf('\n', at);

				if (end < 0)
					end = text.Length;

				if (!blank)
					Take(at, end - at);

				at = end;
				continue;
			}

			if (c == '/' && at + 1 < text.Length && text[at + 1] == '*')
			{
				var depth = 0;
				var from  = at;

				do
				{
					if (at + 1 < text.Length && text[at] == '/' && text[at + 1] == '*')
					{
						depth++;
						at += 2;
					}
					else if (at + 1 < text.Length && text[at] == '*' && text[at + 1] == '/')
					{
						depth--;
						at += 2;
					}
					else
					{
						if (text[at] == '\n')
							line++;

						at++;
					}
				}
				while (depth > 0 && at < text.Length);

				if (!blank)
					Take(from, at - from);

				continue;
			}

			if (c == '\'' || c == '"')
			{
				var escaped = c == '\'' && (escapes || at > 0 && (text[at - 1] | 0x20) == 'e' && (at < 2 || !IsWordChar(text[at - 2])));
				var from    = at;

				at++;

				while (at < text.Length)
				{
					if (text[at] == '\n')
						line++;

					if (escaped && text[at] == '\\' && at + 1 < text.Length)
					{
						if (text[at + 1] == '\n')
							line++;

						at += 2;
						continue;
					}

					if (text[at] == c)
					{
						if (at + 1 < text.Length && text[at + 1] == c)
						{
							at += 2;
							continue;
						}

						at++;
						break;
					}

					at++;
				}

				Take(from, at - from);
				continue;
			}

			if (c == '$' && DollarTag(text, at) is { } tag)
			{
				var close = text.IndexOf(tag, at + tag.Length, StringComparison.Ordinal);
				var end   = close < 0 ? text.Length : close + tag.Length;

				for (var i = at; i < end; i++)
					if (text[i] == '\n')
						line++;

				Take(at, end - at);
				at = end;
				continue;
			}

			// psql's own: `\;` is a semicolon the server never sees, `\\` ends a command on a line, and `\name` runs
			// to the end of the line. The commands that send the buffer send it, `\r` drops it, `\set` defines a
			// variable, `\copy … from stdin` has its data to `\.`; every other one leaves the buffer as it is.
			if (c == '\\')
			{
				if (at + 1 < text.Length && text[at + 1] == ';')
				{
					at += 2;

					if (parens == 0 && begins == 0)
						Flush();

					continue;
				}

				if (at + 1 < text.Length && text[at + 1] == '\\')
				{
					at += 2;
					continue;
				}

				var end       = text.IndexOf('\n', at);
				var separator = text.IndexOf("\\\\", at + 1, StringComparison.Ordinal);

				if (end < 0)
					end = text.Length;

				if (separator >= 0 && separator < end)
					end = separator;

				var command = text.Substring(at, end - at).TrimEnd('\r');
				var nameEnd = 1;

				while (nameEnd < command.Length && (char.IsLetterOrDigit(command[nameEnd]) || command[nameEnd] == '_' || command[nameEnd] == '?'))
					nameEnd++;

				var name = command.Substring(1, nameEnd - 1);

				directives++;
				at = end;

				if (Sending.Contains(name))
					Flush();
				else if (Discarding.Contains(name))
				{
					buffer.Clear();
					variable = false;
					blank    = true;
					parens   = 0;
					begins   = 0;
					lastWord = "";
				}
				else if (name == "set")
				{
					var words = command.Substring(nameEnd).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

					if (words.Length > 0)
						defined.Add(words[0]);
				}
				else if (name == "unset")
				{
					var words = command.Substring(nameEnd).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

					if (words.Length > 0)
						defined.Remove(words[0]);
				}
				else if (name == "copy" && CopyCommandFromStdin.IsMatch(command))
				{
					at = end < text.Length ? end : text.Length;
					SkipCopyData();
					continue;
				}

				continue;
			}

			if (c == '\n')
				line++;

			if (c == '(')
				parens++;
			else if (c == ')' && parens > 0)
				parens--;

			// psql's variables: `:name`, `:'name'`, `:"name"`, `:{?name}` — of a name psql has, by `\set` or its own; not
			// the `::` of a cast, and not a slice's `:NULL` or `:n` of a name nobody set.
			if (c == ':' && (at == 0 || text[at - 1] != ':') && at + 1 < text.Length && text[at + 1] != ':')
			{
				var from = at + 1;

				if (from < text.Length && (text[from] == '\'' || text[from] == '"'))
					from++;
				else if (from + 1 < text.Length && text[from] == '{' && text[from + 1] == '?')
					from += 2;

				var to = from;

				while (to < text.Length && IsWordChar(text[to]) && text[to] != '$')
					to++;

				if (to > from && defined.Contains(text.Substring(from, to - from)))
					variable = true;
			}

			// The words psqlscan.l counts: BEGIN ATOMIC opens a body, CASE inside it opens one more, END closes one.
			if (IsWordStart(c))
			{
				var from = at;

				while (at < text.Length && IsWordChar(text[at]))
					at++;

				var word = text.Substring(from, at - from);

				if (lastWord.Equals("begin", StringComparison.OrdinalIgnoreCase) && word.Equals("atomic", StringComparison.OrdinalIgnoreCase))
					begins++;
				else if (begins > 0 && word.Equals("case", StringComparison.OrdinalIgnoreCase))
					begins++;
				else if (begins > 0 && word.Equals("end", StringComparison.OrdinalIgnoreCase))
					begins--;

				lastWord = word;
				Take(from, at - from);
				continue;
			}

			if (c == ';' && parens == 0 && begins == 0)
			{
				at++;

				var statement = buffer.ToString().Trim();
				var copy      = CopyFromStdin.IsMatch(LeadingCommentsStripped.Replace(statement, ""));

				Flush();

				// COPY … FROM stdin: the lines up to `\.` are the data, and belong to no statement.
				if (copy)
					SkipCopyData();

				continue;
			}

			Take(at, 1);
			at++;
		}

		Flush();

		return new Cut(statements, directives, dataLines);
	}

	static bool IsWordStart(char c)
	{
		return char.IsLetter(c) || c == '_';
	}

	static bool IsWordChar(char c)
	{
		return char.IsLetterOrDigit(c) || c == '_' || c == '$';
	}

	/// <summary>The dollar-quote tag beginning at <paramref name="at"/>, `$$` or `$tag$`; null where a `$` is not one (a parameter, `$1`).</summary>
	static string? DollarTag(string text, int at)
	{
		var end = at + 1;

		if (end < text.Length && (char.IsLetter(text[end]) || text[end] == '_'))
		{
			while (end < text.Length && IsWordChar(text[end]) && text[end] != '$')
				end++;
		}

		return end < text.Length && text[end] == '$' ? text.Substring(at, end + 1 - at) : null;
	}

	// ── The reading ────────────────────────────────────────────────────────────

	/// <summary>The statement's kind for the work list: its first word, and the second after CREATE, ALTER and DROP.</summary>
	internal static string Kind(string statement)
	{
		var words = Regex.Matches(LeadingCommentsStripped.Replace(statement, ""), @"[A-Za-z_]+").Select(static one => one.Value.ToUpperInvariant()).Take(3).ToList();

		if (words.Count == 0)
			return "?";

		if (words[0] is "CREATE" or "ALTER" or "DROP" && words.Count > 1)
			return words[0] + " " + (words[1] is "OR" or "UNIQUE" or "TEMP" or "TEMPORARY" or "UNLOGGED" or "MATERIALIZED" or "GLOBAL" or "LOCAL" && words.Count > 2 ? words[2] : words[1]);

		return words[0];
	}

	internal static Result Run(string root, Action<Trouble>? show = null)
	{
		var byFile  = new Dictionary<string, Counts>(StringComparer.Ordinal);
		var refused = new Dictionary<string, int>(StringComparer.Ordinal);
		var kinds   = new Dictionary<string, int>(StringComparer.Ordinal);
		var variables = 0;

		foreach (var file in Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories).OrderBy(static one => one, StringComparer.Ordinal))
		{
			var key = Path.GetRelativePath(root, file).Replace('\\', '/');
			var cut = CutScript(File.ReadAllText(file));
			var read = 0;
			var no   = 0;
			var threw = 0;
			var same  = 0;

			foreach (var statement in cut.Statements)
			{
				if (statement.HasVariable)
				{
					variables++;
					continue;
				}

				var kind = Kind(statement.Text);
				var made = PostgreSqlParser.TryParseSql(statement.Text);

				if (!made.IsSuccess || made.Value is not [var only])
				{
					no++;
					refused[kind] = refused.GetValueOrDefault(kind) + 1;
					show?.Invoke(new Trouble(key, statement.Line, kind, statement.Text, made.Error ?? "more than one statement"));
					continue;
				}

				read++;
				kinds[kind] = kinds.GetValueOrDefault(kind) + 1;

				string printed;

				try
				{
					printed = PostgreSqlWriter.Write(only);
				}
				catch (Exception failure)
				{
					threw++;
					show?.Invoke(new Trouble(key, statement.Line, kind, statement.Text, "the writer threw " + failure.GetType().Name + ": " + failure.Message));
					continue;
				}

				var back = PostgreSqlParser.TryParseSql(printed);

				if (!back.IsSuccess || back.Value is not [var again])
				{
					threw++;
					show?.Invoke(new Trouble(key, statement.Line, kind, statement.Text, "written as `" + printed + "`, which does not read back"));
					continue;
				}

				if (!only.Equals(again))
				{
					threw++;
					show?.Invoke(new Trouble(key, statement.Line, kind, statement.Text, "written as `" + printed + "`, which reads back as another tree"));
					continue;
				}

				same++;
			}

			byFile[key] = new Counts(read, no, threw, same);
		}

		return new Result(byFile, refused, kinds, variables);
	}

	/// <summary>`--pg-corpus <dir> [groups] [each]`: the four cells per file and in all, the work list of refusals by kind, and `each` refusals of the first `groups` kinds.</summary>
	internal static void Report(string root, int shown, int each)
	{
		if (!Directory.Exists(root))
		{
			Console.WriteLine($"No corpus at {root}.");
			return;
		}

		var troubles = new List<Trouble>();
		var result   = Run(root, troubles.Add);
		var read     = 0;
		var refused  = 0;
		var threw    = 0;
		var same     = 0;

		foreach (var (file, counts) in result.ByFile)
		{
			Console.WriteLine($"{file} = {counts.Read}/{counts.Refused}/{counts.Threw}/{counts.Same}");
			read    += counts.Read;
			refused += counts.Refused;
			threw   += counts.Threw;
			same    += counts.Same;
		}

		Console.WriteLine();
		Console.WriteLine($"{result.ByFile.Count} files: read {read}, refused {refused}, threw {threw}, same {same}; {result.Variables} statements with psql variables, read by nobody.");
		Console.WriteLine();
		Console.WriteLine("Refused, by the statement's first words (the work list):");

		foreach (var (kind, count) in result.RefusedByKind.OrderByDescending(static one => one.Value).ThenBy(static one => one.Key, StringComparer.Ordinal))
			Console.WriteLine($"  {count,6}  {kind}  (read {result.ReadByKind.GetValueOrDefault(kind)})");

		Console.WriteLine();
		Console.WriteLine($"Read, by kind:");

		foreach (var (kind, count) in result.ReadByKind.OrderByDescending(static one => one.Value).ThenBy(static one => one.Key, StringComparer.Ordinal))
			Console.WriteLine($"  {count,6}  {kind}");

		foreach (var group in troubles.Where(static one => !one.Why.StartsWith("the writer", StringComparison.Ordinal) && !one.Why.StartsWith("written as", StringComparison.Ordinal)).GroupBy(static one => one.Kind).OrderByDescending(static one => one.Count()).Take(shown))
		{
			Console.WriteLine();
			Console.WriteLine($"── {group.Key}: {group.Count()} refused ──");

			foreach (var trouble in group.Take(each))
				Console.WriteLine($"  {trouble.File}:{trouble.Line}: {Short(trouble.Statement)}\n    {trouble.Why}");
		}

		var broken = troubles.Where(static one => one.Why.StartsWith("the writer", StringComparison.Ordinal) || one.Why.StartsWith("written as", StringComparison.Ordinal)).ToList();

		if (broken.Count > 0)
		{
			Console.WriteLine();
			Console.WriteLine($"── the round trip: {broken.Count} did not come back ──");

			foreach (var trouble in broken.Take(each * 4))
				Console.WriteLine($"  {trouble.File}:{trouble.Line}: {Short(trouble.Statement)}\n    {trouble.Why}");
		}
	}

	static string Short(string statement)
	{
		var one = Regex.Replace(statement, @"\s+", " ");

		return one.Length > 160 ? one.Substring(0, 157) + "..." : one;
	}
}
