using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Benchmarks;
using DotGram.ExpressionLanguage;
using DotGram.Handwritten;
using DotGram.Sql;
using DotGram.Sql.Ast;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

// g1lab loop <row> <variant> [seconds]  : one reading of one row in a loop; ns and bytes a call (one process per variant)
// g1lab agree <repo-root> <out-dir>      : shipped vs the lab copy on the tape vs the lab copy carried immediately
// g1lab size                             : IL by family of the SQL:2023 readings and EL's two carriers
// g1lab carriers                         : which store each lab class compiled to (ImmediateValues / DirectValues)

var ElImmediate = typeof(ExpressionParser).GetNestedType("Immediate", BindingFlags.NonPublic)!;
string Select20 = "SELECT " + string.Join(", ", Enumerable.Range(0, 20).Select(i => "a" + i)) + " FROM t WHERE a0 = 1";

var texts = new Dictionary<string, (string Entry, string Text)>
{
	["sql/literal"]    = ("Literal", "1"),
	["sql/column"]     = ("ColumnReference", "a.b.c"),
	["sql/arithmetic"] = ("ValueExpression", "(a + b) * c - d / 5"),
	["sql/nest8"]      = ("ValueExpression", "((((((((a))))))))"),
	["sql/condition"]  = ("SearchCondition", "x = 1 AND y IS NOT NULL OR z BETWEEN 1 AND 2"),
	["sql/select1"]    = ("QueryExpression", "SELECT a FROM t"),
	["sql/select20"]   = ("QueryExpression", Select20),
	["sql/values"]     = ("QueryExpression", "VALUES (1)"),
};

Func<int> Reader(string variant, string entry, string text)
{
	return (variant, entry) switch
	{
		("shipped", "Literal")         => () => SqlStandardParser.TryParseLiteral(text).IsSuccess ? 1 : 0,
		("shipped", "ColumnReference") => () => SqlStandardParser.TryParseColumnReference(text).IsSuccess ? 1 : 0,
		("shipped", "ValueExpression") => () => SqlStandardParser.TryParseValueExpression(text).IsSuccess ? 1 : 0,
		("shipped", "SearchCondition") => () => SqlStandardParser.TryParseSearchCondition(text).IsSuccess ? 1 : 0,
		("shipped", "QueryExpression") => () => SqlStandardParser.TryParseQueryExpression(text).IsSuccess ? 1 : 0,
		("tape", "Literal")         => () => CarriedSqlStandardTape.TryParseLiteral(text).IsSuccess ? 1 : 0,
		("tape", "ColumnReference") => () => CarriedSqlStandardTape.TryParseColumnReference(text).IsSuccess ? 1 : 0,
		("tape", "ValueExpression") => () => CarriedSqlStandardTape.TryParseValueExpression(text).IsSuccess ? 1 : 0,
		("tape", "SearchCondition") => () => CarriedSqlStandardTape.TryParseSearchCondition(text).IsSuccess ? 1 : 0,
		("tape", "QueryExpression") => () => CarriedSqlStandardTape.TryParseQueryExpression(text).IsSuccess ? 1 : 0,
		("carried", "Literal")         => () => CarriedSqlStandard.TryParseLiteral(text).IsSuccess ? 1 : 0,
		("carried", "ColumnReference") => () => CarriedSqlStandard.TryParseColumnReference(text).IsSuccess ? 1 : 0,
		("carried", "ValueExpression") => () => CarriedSqlStandard.TryParseValueExpression(text).IsSuccess ? 1 : 0,
		("carried", "SearchCondition") => () => CarriedSqlStandard.TryParseSearchCondition(text).IsSuccess ? 1 : 0,
		("carried", "QueryExpression") => () => CarriedSqlStandard.TryParseQueryExpression(text).IsSuccess ? 1 : 0,
		("hand", "Literal")         => () => HandSqlStandard.TryParseLiteral(text, out _) ? 1 : 0,
		("hand", "ColumnReference") => () => HandSqlStandard.TryParseColumnReference(text, out _) ? 1 : 0,
		("hand", "ValueExpression") => () => HandSqlStandard.TryParseValueExpression(text, out _) ? 1 : 0,
		("hand", "SearchCondition") => () => HandSqlStandard.TryParseSearchCondition(text, out _) ? 1 : 0,
		("hand", "QueryExpression") => () => HandSqlStandard.TryParseQueryExpression(text, out _) ? 1 : 0,
		_ => throw new ArgumentException($"{variant} {entry}"),
	};
}

switch (args.Length > 0 ? args[0] : "")
{
	case "loop":
	{
		var (entry, text) = texts[args[1]];
		var read    = Reader(args[2], entry, text);
		var seconds = args.Length > 3 ? double.Parse(args[3], CultureInfo.InvariantCulture) : 3.0;
		var warmFor = args.Length > 4 ? double.Parse(args[4], CultureInfo.InvariantCulture) : 2.0;
		var sink    = read();

		if (sink != 1)
			throw new InvalidOperationException($"{args[1]} {args[2]} does not read");

		var warm = Stopwatch.StartNew();

		while (warm.Elapsed.TotalSeconds < warmFor)
			sink += read();

		GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
		var allocated = GC.GetTotalAllocatedBytes(true);
		var watch     = Stopwatch.StartNew();
		long calls    = 0;

		while (watch.Elapsed.TotalSeconds < seconds)
		{
			for (var i = 0; i < 1000; i++)
				sink += read();
			calls += 1000;
		}

		watch.Stop();
		allocated = GC.GetTotalAllocatedBytes(true) - allocated;
		Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
			$"{args[1]} {args[2]}: {calls} calls, {watch.Elapsed.TotalMilliseconds * 1e6 / calls:F1} ns a call, {allocated / (double)calls:F0} B a call (sink {sink})"));
		return;
	}

	case "agree":
		Agree(args[1], args[2]);
		return;

	case "size":
		Sizes();
		return;

	case "size-machines":
		foreach (var root in new[] { typeof(SqlStandardParser), typeof(CarriedSqlStandardTape), typeof(CarriedSqlStandard), typeof(ExpressionParser), ElImmediate })
		{
			var machines = new SortedDictionary<string, long[]>(StringComparer.Ordinal);
			foreach (var type in Within(root, root == typeof(ExpressionParser) ? ElImmediate : null))
				foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Cast<MethodBase>())
				{
					int il;
					try { il = m.GetMethodBody()?.GetILAsByteArray()?.Length ?? 0; } catch { il = 0; }
					string? machine = null; var slot = 0;
					if (type.Name.StartsWith("Reader_DotGram_")) { machine = type.Name["Reader_DotGram_".Length..]; slot = m.Name.Contains("_Part") ? 1 : 0; }
					else if (m.Name.StartsWith("Materialize_DotGram_"))
					{
						var rest = m.Name["Materialize_DotGram_".Length..];
						var cut = rest.IndexOf('_');
						machine = cut < 0 ? rest : rest[..cut];
						slot = 2;
					}
					if (machine is null) continue;
					if (!machines.TryGetValue(machine, out var c)) machines[machine] = c = new long[4];
					c[slot] += il; c[3]++;
				}
			Console.WriteLine($"== {root.FullName}");
			foreach (var (k, v) in machines)
				Console.WriteLine($"   {k,-40} reader {v[0],10:N0}  parts {v[1],10:N0}  materialiser {v[2],10:N0}  total {v[0] + v[1] + v[2],10:N0}  ({v[3]} methods)");
		}
		return;

	case "carriers":
		foreach (var type in new[] { typeof(SqlStandardParser), typeof(CarriedSqlStandard), typeof(CarriedSqlStandardTape), typeof(ImmediateSqlStandard), typeof(ImmediateSql), typeof(ExpressionParser), ElImmediate })
		{
			var nested = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Select(static one => one.Name).ToList();
			Console.WriteLine($"{type.FullName}: ImmediateValues {nested.Any(static n => n.Contains("ImmediateValues"))}, DirectValues {nested.Any(static n => n.Contains("DirectValues"))}");
		}
		return;

	default:
		Console.WriteLine(string.Join(", ", texts.Keys));
		return;
}

// ── agreement ──────────────────────────────────────────────────────────────

void Agree(string repo, string outDir)
{
	Directory.CreateDirectory(outDir);

	var cases = new List<(string Source, string Entry, string Text)>();
	var corpus = Path.Combine(repo, "tests", "Corpus", "ScriptDom");
	var files  = Directory.GetFiles(corpus, "*.sql", SearchOption.AllDirectories).OrderBy(static f => f, StringComparer.Ordinal).ToList();

	// (1) The corpus as DotGram.MessageDiff reads it for SQL:2023: statements cut at ';' and GO,
	// whole and their first 60%, each read as a direct SQL statement with ';' after it.
	foreach (var file in files)
		foreach (var statement in Statements(File.ReadAllText(file)))
			foreach (var cut in Cuts(statement))
				cases.Add(("corpus-direct", "DirectSQLStatement", cut + ";"));

	// (2) What the corpus holds that SQL:2023 can read: T-SQL's queries written back by the
	// SQL:2023 writer, whole and their first 60%, read as query expressions.
	foreach (var file in files)
		foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
		{
			var first = TransactSqlParser.TryParseSql(batch);

			if (!first.IsSuccess)
				continue;

			foreach (var statement in first.Value)
			{
				string written;

				try
				{
					written = SqlWriter.Write(statement);
				}
				catch (Exception)
				{
					continue;
				}

				var entry = statement is DotGram.Sql.Statement.Select ? "QueryExpression" : "DirectSQLStatement";

				foreach (var cut in Cuts(written))
					cases.Add(("corpus-written", entry, entry == "DirectSQLStatement" ? cut + ";" : cut));
			}
		}

	// (3) The strings of the SQL:2023 test files, each read four ways.
	var literal = new Regex("\"((?:[^\"\\\\]|\\\\.)*)\"", RegexOptions.Compiled);

	foreach (var file in Directory.GetFiles(Path.Combine(repo, "tests", "DotGram.Sql.Tests"), "*.cs")
		.Concat(Directory.GetFiles(Path.Combine(repo, "tests", "DotGram.Tests.Slow"), "Sql*.cs"))
		.Concat(Directory.GetFiles(Path.Combine(repo, "benchmarks", "DotGram.Benchmarks"), "*.cs")))
		foreach (var line in File.ReadLines(file))
		{

			foreach (Match m in literal.Matches(line))
			{
				var text = Unescape(m.Groups[1].Value);

				if (text.Length == 0)
					continue;

				foreach (var entry in new[] { "DirectSQLStatement", "QueryExpression", "ValueExpression", "SearchCondition" })
					cases.Add(("tests", entry, text));
			}
		}

	// (4) The stand's rows.
	foreach (var (_, (entry, text)) in texts)
		cases.Add(("stand", entry, text));

	cases = cases.Distinct().ToList();

	// (5) Every text the shipped parser accepts, cut at each boundary between words and signs, and
	// with a stray token after it: refusals late in a text, where a construction may already have
	// run on a well-formed prefix.
	var derived = new List<(string Source, string Entry, string Text)>();

	foreach (var (_, entry, text) in cases)
	{
		if (Read("shipped", entry, text).Kind != "ok")
			continue;

		var cuts = 0;
		var body = entry == "DirectSQLStatement" && text.EndsWith(";") ? text[..^1] : text;
		var end  = entry == "DirectSQLStatement" ? ";" : "";

		for (var i = 1; i < body.Length && cuts < 300; i++)
			if (char.IsLetterOrDigit(body[i - 1]) != char.IsLetterOrDigit(body[i]))
			{
				derived.Add(("prefixes", entry, body[..i] + end));
				cuts++;
			}

		foreach (var stray in new[] { " )", " ,", " x", " AND", " (" })
			derived.Add(("stray", entry, body + stray + end));
	}

	cases.AddRange(derived);
	cases = cases.Distinct().ToList();
	Console.WriteLine($"{cases.Count} readings");

	var tallies = new SortedDictionary<string, int>(StringComparer.Ordinal);
	using var detail = new StreamWriter(Path.Combine(outDir, "agreement-differences.tsv"), false, new UTF8Encoding(false)) { NewLine = "\n" };
	using var throwers = new StreamWriter(Path.Combine(outDir, "agreement-throwers.tsv"), false, new UTF8Encoding(false)) { NewLine = "\n" };

	void Count(string key)
	{
		tallies[key] = tallies.GetValueOrDefault(key) + 1;
	}

	foreach (var (source, entry, text) in cases)
	{
		var shipped = Read("shipped", entry, text);
		var tape    = Read("tape", entry, text);
		var carried = Read("carried", entry, text);

		Count($"{source}\tshipped {shipped.Kind}");
		Count($"all\tshipped {shipped.Kind}");

		// The grammar edit: the copy on the tape against the shipped parser.
		var edit = Compare(shipped, tape);
		Count($"{source}\tedit: {edit}");
		Count($"all\tedit: {edit}");

		// The carrier: the copy carried immediately against the shipped parser.
		var carrier = Compare(shipped, carried);
		Count($"{source}\tcarried: {carrier}");
		Count($"all\tcarried: {carrier}");

		if (edit != "same tree" && edit != "same refusal")
			detail.WriteLine($"edit\t{source}\t{entry}\t{edit}\t{Escape(text)}\t{Escape(shipped.Said)}\t{Escape(tape.Said)}");

		if (carrier != "same tree" && carrier != "same refusal")
		{
			if (carrier == "tape refuses, carried throws")
				throwers.WriteLine($"{source}\t{entry}\t{Escape(text)}\t{Escape(shipped.Said)}\t{Escape(carried.Said)}");
			else
				detail.WriteLine($"carried\t{source}\t{entry}\t{carrier}\t{Escape(text)}\t{Escape(shipped.Said)}\t{Escape(carried.Said)}");
		}
	}

	using var summary = new StreamWriter(Path.Combine(outDir, "agreement-summary.tsv"), false, new UTF8Encoding(false)) { NewLine = "\n" };

	foreach (var (key, count) in tallies)
	{
		summary.WriteLine($"{key}\t{count}");
		Console.WriteLine($"{key}\t{count}");
	}
}

static string Compare(Outcome a, Outcome b)
{
	return (a.Kind, b.Kind) switch
	{
		("ok", "ok") => Equals(a.Value, b.Value) ? (a.Said == b.Said ? "same tree" : "equal tree, other text") : (a.Said == b.Said ? "same text, unequal tree" : "different tree"),
		("refused", "refused") => a.Said == b.Said ? "same refusal" : a.Position == b.Position ? "refused at the same place, other message" : "refused elsewhere",
		("refused", "threw") => "tape refuses, carried throws",
		("threw", "threw") => a.Said == b.Said ? "same exception" : "both throw, differently",
		_ => $"{a.Kind} vs {b.Kind}",
	};
}

Outcome Read(string variant, string entry, string text)
{
	try
	{
		dynamic match = (variant, entry) switch
		{
			("shipped", "DirectSQLStatement") => SqlStandardParser.TryParseDirectSQLStatement(text),
			("shipped", "QueryExpression")    => SqlStandardParser.TryParseQueryExpression(text),
			("shipped", "ValueExpression")    => SqlStandardParser.TryParseValueExpression(text),
			("shipped", "SearchCondition")    => SqlStandardParser.TryParseSearchCondition(text),
			("shipped", "Literal")            => SqlStandardParser.TryParseLiteral(text),
			("shipped", "ColumnReference")    => SqlStandardParser.TryParseColumnReference(text),
			("tape", "DirectSQLStatement")    => CarriedSqlStandardTape.TryParseDirectSQLStatement(text),
			("tape", "QueryExpression")       => CarriedSqlStandardTape.TryParseQueryExpression(text),
			("tape", "ValueExpression")       => CarriedSqlStandardTape.TryParseValueExpression(text),
			("tape", "SearchCondition")       => CarriedSqlStandardTape.TryParseSearchCondition(text),
			("tape", "Literal")               => CarriedSqlStandardTape.TryParseLiteral(text),
			("tape", "ColumnReference")       => CarriedSqlStandardTape.TryParseColumnReference(text),
			("carried", "DirectSQLStatement") => CarriedSqlStandard.TryParseDirectSQLStatement(text),
			("carried", "QueryExpression")    => CarriedSqlStandard.TryParseQueryExpression(text),
			("carried", "ValueExpression")    => CarriedSqlStandard.TryParseValueExpression(text),
			("carried", "SearchCondition")    => CarriedSqlStandard.TryParseSearchCondition(text),
			("carried", "Literal")            => CarriedSqlStandard.TryParseLiteral(text),
			("carried", "ColumnReference")    => CarriedSqlStandard.TryParseColumnReference(text),
			_ => throw new ArgumentException(entry),
		};

		if ((bool)match.IsSuccess)
		{
			object value = match.Value;
			string written;

			try
			{
				written = value switch
				{
					ISqlNode n => Sql2023Writer.Write(n),
					_ => value?.ToString() ?? "null",
				};
			}
			catch (Exception exception)
			{
				written = "writer threw " + exception.GetType().Name;
			}

			return new Outcome("ok", value, "ok " + written, -1);
		}

		long position = match.Position;
		string? error = match.Error;

		return new Outcome("refused", null, $"refused at {position}: {error}", position);
	}
	catch (Exception exception) when (exception is not Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
	{
		return new Outcome("threw", null, $"threw {exception.GetType().Name}: {exception.Message}", -1);
	}
}

static string Unescape(string text)
{
	var b = new StringBuilder();
	for (var i = 0; i < text.Length; i++)
	{
		if (text[i] != '\\' || i + 1 == text.Length) { b.Append(text[i]); continue; }
		var c = text[++i];
		b.Append(c switch { 'n' => '\n', 'r' => '\r', 't' => '\t', '0' => '\0', _ => c });
	}
	return b.ToString();
}

static string Escape(string text)
{
	return text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
}

static IEnumerable<string> Cuts(string text)
{
	yield return text;
	yield return text[..(text.Length * 6 / 10)];
}

// As DotGram.MessageDiff cuts them: batches at GO, statements at ';' outside quotes and comments.
static IEnumerable<string> Statements(string script)
{
	foreach (var batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
	{
		var current = new StringBuilder();
		var quote   = '\0';

		for (var i = 0; i < batch.Length; i++)
		{
			var c = batch[i];

			if (quote != '\0')
			{
				current.Append(c);

				if (c == quote)
					quote = '\0';

				continue;
			}

			if (c == '-' && i + 1 < batch.Length && batch[i + 1] == '-')
			{
				while (i < batch.Length && batch[i] != '\n')
					i++;

				current.Append('\n');
				continue;
			}

			if (c is '\'' or '"' or '[')
			{
				quote = c == '[' ? ']' : c;
				current.Append(c);
				continue;
			}

			if (c == ';')
			{
				var one = current.ToString().Trim();

				if (one.Length > 0)
					yield return one;

				current.Clear();
				continue;
			}

			current.Append(c);
		}

		var last = current.ToString().Trim();

		if (last.Length > 0)
			yield return last;
	}
}

// ── size ───────────────────────────────────────────────────────────────────

void Sizes()
{
	foreach (var (name, roots) in new (string, Type[])[]
	{
		("SqlStandardParser (shipped, tape)", new[] { typeof(SqlStandardParser) }),
		("ImmediateSqlStandard (fallback tape)", new[] { typeof(ImmediateSqlStandard) }),
		("CarriedSqlStandardTape (lab copy, tape)", new[] { typeof(CarriedSqlStandardTape) }),
		("CarriedSqlStandard (lab copy, immediate)", new[] { typeof(CarriedSqlStandard) }),
		("ExpressionParser (tape, without Immediate)", new[] { typeof(ExpressionParser) }),
		("ExpressionParser.Immediate (immediate)", new[] { ElImmediate }),
		("HandSqlStandard+SqlTowers+SqlCursor+SqlWords", typeof(HandSqlStandard).Assembly.GetTypes().Where(static t => t.DeclaringType is null && t.Name is "HandSqlStandard" or "SqlTowers" or "SqlCursor" or "SqlWords").ToArray()),
	})
	{
		var families = new SortedDictionary<string, (long Il, int Methods)>(StringComparer.Ordinal);
		long total = 0;
		var count  = 0;

		foreach (var root in roots)
			foreach (var type in Within(root, root == typeof(ExpressionParser) ? ElImmediate : null))
				foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Cast<MethodBase>()
					.Concat(type.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)))
				{
					int il;
					try { il = m.GetMethodBody()?.GetILAsByteArray()?.Length ?? 0; } catch { il = 0; }
					var f = Family(m.Name, type.Name);
					families[f] = (families.GetValueOrDefault(f).Il + il, families.GetValueOrDefault(f).Methods + 1);
					total += il;
					count++;
				}

		Console.WriteLine($"== {name}: {total:N0} IL bytes in {count:N0} methods");
		foreach (var (k, v) in families.OrderByDescending(static e => e.Value.Il))
			Console.WriteLine($"   {v.Il,10:N0}  {v.Methods,6}  {k}");
	}
}

static IEnumerable<Type> Within(Type root, Type? except)
{
	yield return root;

	foreach (var nested in root.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
	{
		if (nested == except)
			continue;

		foreach (var one in Within(nested, except))
			yield return one;
	}
}

static string Family(string method, string type)
{
	if (type.StartsWith("Reader_") || method.StartsWith("Read_")) return method.Contains("_Part") ? "Read_ parts" : "Read_ (reader methods)";
	if (method.StartsWith("Recognize_") && method.Contains("_Expected")) return "expected sets";
	if (method.StartsWith("get_") && method.Contains("Expected")) return "expected sets";
	if (method.StartsWith("Recognize_")) return "Recognize_ (engine/flat/tables)";
	if (method.StartsWith("Materialize_")) return method.Contains("_Arm") ? "Materialize_ arms" : "Materialize_ walk/parts";
	if (method.StartsWith("Construct_")) return "Construct_ factories";
	if (method.StartsWith("Scan") || method.StartsWith("Tokenize")) return "lexer/scanners";
	if (method.StartsWith("Parse") || method.StartsWith("TryParse")) return "public entries";
	if (type.Contains("Ways") || type.Contains("DirectValues") || type.Contains("ImmediateValues") || type.Contains("Failure")) return "support (Ways/values/failure)";
	return "other";
}

record Outcome(string Kind, object? Value, string Said, long Position);
