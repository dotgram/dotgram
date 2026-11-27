using System.Diagnostics;
using System.Text;

using DotGram.Benchmarks;
using DotGram.Sql;
using DotGram.Sql.PostgreSql;
using DotGram.Sql.TransactSql;

// `spans <corpus-dir>... --out <file>`: one line per node of every statement the located reading reads.
// `time <rounds> <corpus-dir>...`: the plain and the located reading over the same statements, interleaved.
var mode  = args[0];
var dirs  = new List<string>();
string? output = null;
var rounds = 0;

for (var at = 1; at < args.Length; at++)
{
	if (args[at] == "--out")
	{
		output = args[++at];
	}
	else if (mode is "time" or "ttime" && rounds == 0)
	{
		rounds = int.Parse(args[at]);
	}
	else
	{
		dirs.Add(args[at]);
	}
}

var statements = new List<(string Where, string Text)>();

foreach (var dir in mode is "tspans" or "tplain" or "ttime" ? [] : dirs)
{
	foreach (var file in Directory.GetFiles(dir, "*.sql", SearchOption.AllDirectories).OrderBy(static one => one, StringComparer.Ordinal))
	{
		var key = Path.GetFileName(dir.TrimEnd('/')) + "/" + Path.GetRelativePath(dir, file).Replace('\\', '/');

		foreach (var statement in PgCorpus.CutScript(File.ReadAllText(file)).Statements)
		{
			if (!statement.HasVariable)
				statements.Add((key + ":" + statement.Line, statement.Text));
		}
	}
}

if (mode is "spans" or "plain" or "tspans" or "tplain")
{
	using var writer = new StreamWriter(output!, false, new UTF8Encoding(false));
	var read    = 0;
	var refused = 0;
	var nodes   = 0;
	var tsql    = mode is "tspans" or "tplain";
	var located = mode.EndsWith("spans", StringComparison.Ordinal);
	var inputs  = tsql ? Scripts() : statements;

	foreach (var (where, text) in inputs)
	{
		IEnumerable<ISqlSpan> roots;

		if (tsql)
		{
			var made = located ? TransactSqlParser.Located.TryParseScript(text) : TransactSqlParser.TryParseScript(text);

			if (!made.IsSuccess)
			{
				refused++;
				writer.Write($"{where}\trefused\t{made.Position}\t{made.Error}\n");
				continue;
			}

			roots = made.Value.SelectMany(static batch => batch.Statements).Cast<ISqlSpan>();
		}
		else
		{
			var made = located ? PostgreSqlParser.Located.TryParseSql(text) : PostgreSqlParser.TryParseSql(text);

			if (!made.IsSuccess)
			{
				refused++;
				writer.Write($"{where}\trefused\t{made.Position}\t{made.Error}\n");
				continue;
			}

			roots = made.Value.Cast<ISqlSpan>();
		}

		read++;

		var index = 0;

		foreach (var root in roots)
		{
			// The whole tree as its record prints it, hashed stably: equal trees across builds and processes.
			writer.Write($"{where}\troot\t{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(root.ToString()!)))}\n");

			SqlWalker.Walk(root, node =>
			{
				var span = node.Span;

				writer.Write(where);
				writer.Write('\t');
				writer.Write(index++);
				writer.Write('\t');
				writer.Write(TypeName(node.GetType()));
				writer.Write('\t');
				writer.Write(span.At);
				writer.Write('\t');
				writer.Write(span.Length);
				writer.Write('\t');
				writer.Write(span.GapStart);
				writer.Write('\t');
				writer.Write(span.Known ? Excerpt(text, span) : "-");
				writer.Write('\t');
				writer.Write(node.GetHashCode());
				writer.Write('\n');
				nodes++;

				return true;
			});
		}
	}

	Console.WriteLine($"inputs {inputs.Count}, read {read}, refused {refused}, nodes {nodes}");

	return 0;
}

if (mode == "ttime")
{
	var scripts = Scripts();

	for (var warm = 0; warm < int.Parse(Environment.GetEnvironmentVariable("PROBE_WARM") ?? "5"); warm++)
	{
		TPlain(scripts);
		TLocated(scripts);
	}

	var plain   = new List<double>();
	var located = new List<double>();

	for (var round = 0; round < rounds; round++)
	{
		var clock = Stopwatch.StartNew();

		TPlain(scripts);
		plain.Add(clock.Elapsed.TotalMilliseconds);
		clock.Restart();
		TLocated(scripts);
		located.Add(clock.Elapsed.TotalMilliseconds);
	}

	Console.WriteLine("plain   " + string.Join(" ", plain.Select(static one => one.ToString("F0"))));
	Console.WriteLine("located " + string.Join(" ", located.Select(static one => one.ToString("F0"))));
	plain.Sort();
	located.Sort();
	Console.WriteLine($"scripts {scripts.Count}; plain median {plain[plain.Count / 2]:F1} ms min {plain[0]:F1}; located median {located[located.Count / 2]:F1} ms min {located[0]:F1}");

	return 0;
}

if (mode == "time")
{
	// Warm both readings first, then interleave them round by round.
	for (var warm = 0; warm < int.Parse(Environment.GetEnvironmentVariable("PROBE_WARM") ?? "5"); warm++)
	{
		Plain();
		Located();
	}

	var plain   = new List<double>();
	var located = new List<double>();

	for (var round = 0; round < rounds; round++)
	{
		var clock = Stopwatch.StartNew();

		Plain();
		plain.Add(clock.Elapsed.TotalMilliseconds);
		clock.Restart();
		Located();
		located.Add(clock.Elapsed.TotalMilliseconds);
	}

	var before = GC.GetAllocatedBytesForCurrentThread();

	Plain();

	var plainBytes = GC.GetAllocatedBytesForCurrentThread() - before;

	before = GC.GetAllocatedBytesForCurrentThread();
	Located();

	var locatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;

	Console.WriteLine("plain   " + string.Join(" ", plain.Select(static one => one.ToString("F0"))));
	Console.WriteLine("located " + string.Join(" ", located.Select(static one => one.ToString("F0"))));
	plain.Sort();
	located.Sort();
	Console.WriteLine($"statements {statements.Count}; plain median {plain[plain.Count / 2]:F1} ms min {plain[0]:F1}, {plainBytes / 1048576.0:F1} MB; located median {located[located.Count / 2]:F1} ms min {located[0]:F1}, {locatedBytes / 1048576.0:F1} MB");

	return 0;
}

if (mode == "cold")
{
	// The first plain and the first located reading of every statement of the first file, in a fresh process: what
	// the JIT costs before either reading has run.
	var first = statements.TakeWhile(one => one.Where.Split(':')[0] == statements[0].Where.Split(':')[0]).ToList();
	var clock = Stopwatch.StartNew();

	foreach (var (_, text) in first)
		PostgreSqlParser.TryParseSql(text);

	var plainCold = clock.Elapsed.TotalMilliseconds;

	clock.Restart();

	foreach (var (_, text) in first)
		PostgreSqlParser.Located.TryParseSql(text);

	Console.WriteLine($"cold: {first.Count} statements, plain first {plainCold:F0} ms, located after {clock.Elapsed.TotalMilliseconds:F0} ms");

	return 0;
}

Console.Error.WriteLine("modes: spans, time, cold");

return 2;

void Plain()
{
	foreach (var (_, text) in statements)
		PostgreSqlParser.TryParseSql(text);
}

void Located()
{
	foreach (var (_, text) in statements)
		PostgreSqlParser.Located.TryParseSql(text);
}

List<(string Where, string Text)> Scripts()
{
	var scripts = new List<(string Where, string Text)>();

	foreach (var dir in dirs)
		foreach (var file in Directory.GetFiles(dir, "*.sql", SearchOption.AllDirectories).OrderBy(static one => one, StringComparer.Ordinal))
			scripts.Add((Path.GetRelativePath(dir, file).Replace('\\', '/'), File.ReadAllText(file)));

	return scripts;
}

static void TPlain(List<(string Where, string Text)> scripts)
{
	foreach (var (_, text) in scripts)
		TransactSqlParser.TryParseScript(text);
}

static void TLocated(List<(string Where, string Text)> scripts)
{
	foreach (var (_, text) in scripts)
		TransactSqlParser.Located.TryParseScript(text);
}

static string TypeName(Type type)
{
	return type.FullName!.Replace("DotGram.Sql.Ast.", "");
}

static string Excerpt(string text, SqlSpan span)
{
	if (span.At < 0 || span.End > text.Length)
		return "<out of range>";

	var piece = text.Substring(span.At, Math.Min(span.Length, 60)).Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');

	return span.Length > 60 ? piece + "…" : piece;
}
