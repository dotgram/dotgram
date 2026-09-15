using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// `--standard production [file]`: each line of a file put to the BNF of ISO/IEC 9075-2:2023, as
/// <see cref="StandardOracle"/> answers it — whether the line is the production named, how many
/// tokens it is, and the token the reading could not go past.
/// </summary>
/// <remarks>
/// The standard's authority, as `--engine` is T-SQL's. A line that begins `--` is a comment and
/// is skipped; without a file the lines are read from the console.
/// </remarks>
static class Standard
{
	public static void Run(string start, string? path)
	{
		var watch  = Stopwatch.StartNew();
		var rules  = Bnf.Read(File.ReadAllText(Specification()));
		var oracle = new StandardOracle(rules);
		var (lexical, reserved) = oracle.Counts;

		Console.WriteLine($"{rules.Count} productions, {lexical} of them lexical, {reserved} reserved words; read in {watch.ElapsedMilliseconds} ms");
		Console.WriteLine();

		// `!` names a production instead of a file: what of the BNF reads as empty, and which named
		// productions that production reaches derive nothing.
		if (start == "!" && path is not null)
		{
			foreach (var (name, body) in rules.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
				if (HasEmpty(body))
					Console.WriteLine($"empty piece in <{name}>: {body}");

			Console.WriteLine();
			Console.WriteLine($"derive nothing, reached from <{path}>: {string.Join(", ", oracle.Empty(path))}");
			return;
		}

		// `=` before the production asks the grammar alone, and times it: the recognizer takes seconds
		// over a long line, which is no way to look for what makes the grammar slow.
		if (start.StartsWith('=') && path is not null)
		{
			var alone = typeof(SqlStandardParser).GetMethod("TryParse" + Bnf.RuleName(start[1..]), [typeof(string)])
				?? throw new ArgumentException($"SqlStandardParser publishes no rule for <{start[1..]}>");

			foreach (var line in File.ReadLines(path))
			{
				if (line.Trim().Length == 0 || line.TrimStart().StartsWith("--", StringComparison.Ordinal))
					continue;

				alone.Invoke(null, [line]);

				watch.Restart();
				var result = alone.Invoke(null, [line])!;
				var ticks  = watch.Elapsed.Ticks;
				var ours   = (bool)result.GetType().GetProperty("IsSuccess")!.GetValue(result)!;

				Console.WriteLine($"{(ours ? "ok" : "no"),3} {TimeSpan.FromTicks(ticks).TotalMilliseconds,9:0.000} ms  {line}");
			}

			return;
		}

		// `~` before the production puts the grammar's tree to the writer and back: each line the rule
		// reads is built, written, read again and built again, and the two trees must be one, and the two
		// texts written from them one text. The BNF is not asked.
		if (start.StartsWith('~') && path is not null)
		{
			RoundTrip(start[1..], path);

			return;
		}

		var lines = path is null
			? Enumerable.Repeat(0, int.MaxValue).Select(_ => Console.ReadLine()).TakeWhile(static one => one is not null).Select(static one => one!)
			: File.ReadLines(path);

		// The grammar's rule of the same name, where one is published: `<column reference>` is
		// asked of `SqlStandardParser.TryParseColumnReference` beside the BNF.
		var parser = typeof(SqlStandardParser).GetMethod("TryParse" + Bnf.RuleName(start), [typeof(string)]);
		var (agree, differ) = (0, 0);
		var grammar = new Stopwatch();
		var slowest = (Ticks: 0L, Line: "");

		foreach (var line in lines)
		{
			if (line.Trim().Length == 0 || line.TrimStart().StartsWith("--", StringComparison.Ordinal))
				continue;

			// `?` asks which lexical productions derive each word of the line, whole.
			if (start == "?")
			{
				foreach (var word in line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
					Console.WriteLine($"{word}: {string.Join(", ", oracle.LexicalNames.Where(name => oracle.Derives(name, word)).OrderBy(static name => name, StringComparer.Ordinal))}");

				continue;
			}

			watch.Restart();

			// A line is a text that ends: a simple comment is closed by a newline (§5.2), and the
			// last line of a file has none of its own.
			var (read, stopped, count) = oracle.Reads(start, line + "\n");
			var where = read ? "" : stopped < 0 ? "  (no token here)" : $"  (stops at token {stopped + 1} of {count}{At(oracle, line, stopped)})";

			if (parser is null)
			{
				Console.WriteLine($"{(read ? "ok" : "no"),3} {count,4} {watch.ElapsedMilliseconds,6} ms  {line}{where}");
				continue;
			}

			// What the grammar says, and a mark where it does not say what the BNF does. The grammar's
			// time is kept apart from the BNF's, which is most of a line's, and taken on a second
			// reading: the first compiles whatever part of the parser the line is the first to reach.
			var before = grammar.Elapsed.Ticks;

			parser.Invoke(null, [line]);

			grammar.Start();
			var result = parser.Invoke(null, [line])!;
			grammar.Stop();

			if (grammar.Elapsed.Ticks - before > slowest.Ticks)
				slowest = (grammar.Elapsed.Ticks - before, line);

			var ours = (bool)result.GetType().GetProperty("IsSuccess")!.GetValue(result)!;

			if (ours == read)
				agree++;
			else
				differ++;

			Console.WriteLine($"{(read ? "ok" : "no"),3} {(ours ? "ok" : "no"),3} {(ours == read ? "" : "≠"),1} {count,4} {watch.ElapsedMilliseconds,6} ms  {line}{where}");
		}

		if (parser is not null)
		{
			Console.WriteLine($"\nthe BNF and {parser.Name}: {agree} agree, {differ} differ");
			Console.WriteLine($"the grammar alone: {grammar.ElapsedMilliseconds} ms, the slowest line {TimeSpan.FromTicks(slowest.Ticks).TotalMilliseconds:0.0} ms: {slowest.Line}");
		}
	}

	/// <summary>
	/// Each line the rule reads, built, written by <see cref="Sql2023Writer"/>, read and built again: the
	/// trees must be one, and so must the texts written from them. What does not hold is shown, the first
	/// of them whole.
	/// </summary>
	static void RoundTrip(string production, string path)
	{
		var parser = typeof(SqlStandardParser).GetMethod("TryParse" + Bnf.RuleName(production), [typeof(string)])
			?? throw new ArgumentException($"SqlStandardParser publishes no rule for <{production}>");
		var (read, same, shown) = (0, 0, 0);

		// A direct SQL statement ends in its semicolon, which is the production's and no part of the tree.
		var terminator = production == "direct SQL statement" ? ";" : "";

		foreach (var line in File.ReadLines(path))
		{
			if (line.Trim().Length == 0 || line.TrimStart().StartsWith("--", StringComparison.Ordinal))
				continue;

			if (Tree(parser, line) is not { } first)
				continue;

			read++;

			string written, again = "";
			ISqlNode? second = null;
			string? failure = null;

			try
			{
				written = Sql2023Writer.Write(first) + terminator;
				second  = Tree(parser, written);

				if (second is not null)
					again = Sql2023Writer.Write(second) + terminator;
			}
			catch (Exception exception)
			{
				written = "";
				failure = exception.GetType().Name + ": " + exception.Message;
			}

			var (before, after) = (Dump(first), second is null ? "" : Dump(second));

			if (failure is null && second is not null && before == after && written == again)
			{
				same++;
				continue;
			}

			if (shown++ >= 40)
				continue;

			Console.WriteLine($"  {line}");
			Console.WriteLine($"  → {(failure ?? (second is null ? "not read back: " + written : before != after ? "another tree: " + written : "written otherwise: " + again))}");

			if (failure is null && second is not null && before != after)
			{
				var at = 0;

				while (at < before.Length && at < after.Length && before[at] == after[at])
					at++;

				Console.WriteLine($"    was  …{before.Substring(Math.Max(0, at - 60), Math.Min(160, before.Length - Math.Max(0, at - 60)))}");
				Console.WriteLine($"    now  …{after.Substring(Math.Max(0, at - 60), Math.Min(160, after.Length - Math.Max(0, at - 60)))}");
			}
		}

		Console.WriteLine($"\n{read} lines read, {same} written and read back to the same tree, {read - same} not");
	}

	static ISqlNode? Tree(MethodInfo parser, string text)
	{
		var result = parser.Invoke(null, [text])!;
		var type   = result.GetType();

		return (bool)type.GetProperty("IsSuccess")!.GetValue(result)! ? type.GetProperty("Value")!.GetValue(result) as ISqlNode : null;
	}

	/// <summary>A tree as text, every property but where it was written: two trees are one where their dumps are.</summary>
	static string Dump(object? node)
	{
		var text = new StringBuilder();

		Dump(text, node);

		return text.ToString();
	}

	static void Dump(StringBuilder text, object? node)
	{
		switch (node)
		{
			case null:
				text.Append("null");
				return;

			case string value:
				text.Append('"').Append(value).Append('"');
				return;

			case System.Collections.IEnumerable list:
				text.Append('[');

				foreach (var one in list)
				{
					Dump(text, one);
					text.Append(',');
				}

				text.Append(']');
				return;
		}

		var type = node.GetType();

		if (type.IsPrimitive || type.IsEnum)
		{
			text.Append(node);
			return;
		}

		text.Append(type.Name).Append('(');

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (property.Name is "Span" or "EqualityContract" || property.GetIndexParameters().Length > 0)
				continue;

			text.Append(property.Name).Append('=');
			Dump(text, property.GetValue(node));
			text.Append(';');
		}

		text.Append(')');
	}

	static bool HasEmpty(BnfNode node) => node switch
	{
		BnfSequence sequence => sequence.Items.Length == 0 || sequence.Items.Any(HasEmpty),
		BnfChoice choice     => choice.Options.Any(static option => option is BnfSequence { Items.Length: 0 } || HasEmpty(option)),
		BnfOptional optional => HasEmpty(optional.Body),
		BnfRepeated repeated => HasEmpty(repeated.Body),
		_                    => false,
	};

	static string At(StandardOracle oracle, string line, int stopped)
	{
		var tokens = oracle.Tokens(line + "\n");

		return tokens is not null && stopped >= 0 && stopped < tokens.Count ? $": {tokens[stopped]}" : "";
	}

	static string Specification()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return Path.Combine(at?.FullName ?? ".", "src", "DotGram.Sql", "Standard", "Specification", "ISO_IEC_9075-2(E)_Foundation.bnf.txt");
	}
}
