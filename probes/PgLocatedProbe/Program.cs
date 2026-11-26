using System.Diagnostics;
using System.Text;

using DotGram.Benchmarks;
using DotGram.Sql;
using DotGram.Sql.PostgreSql;

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
	else if (mode == "time" && rounds == 0)
	{
		rounds = int.Parse(args[at]);
	}
	else
	{
		dirs.Add(args[at]);
	}
}

var statements = new List<(string Where, string Text)>();

foreach (var dir in dirs)
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

if (mode == "spans")
{
	using var writer = new StreamWriter(output!, false, new UTF8Encoding(false));
	var read    = 0;
	var refused = 0;
	var nodes   = 0;

	foreach (var (where, text) in statements)
	{
		var made = PostgreSqlParser.Located.TryParseSql(text);

		if (!made.IsSuccess)
		{
			refused++;
			continue;
		}

		read++;

		var index = 0;

		foreach (var root in made.Value)
		{
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
				writer.Write('\n');
				nodes++;

				return true;
			});
		}
	}

	Console.WriteLine($"statements {statements.Count}, read {read}, refused {refused}, nodes {nodes}");

	return 0;
}

if (mode == "time")
{
	// Warm both readings first, then interleave them round by round.
	for (var warm = 0; warm < 2; warm++)
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

	plain.Sort();
	located.Sort();
	Console.WriteLine($"statements {statements.Count}; plain median {plain[plain.Count / 2]:F1} ms min {plain[0]:F1}; located median {located[located.Count / 2]:F1} ms min {located[0]:F1}");

	return 0;
}

Console.Error.WriteLine("modes: spans, time");

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
