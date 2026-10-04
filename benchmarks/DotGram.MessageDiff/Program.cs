using System.Text;

using DotGram.Sql;
using DotGram.Sql.Ast;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

// Usage: DotGram.MessageDiff <corpus-directory> <output-file>
//
// One line per reading: the file (relative to the corpus), what was read, the outcome, where it
// stopped, the whole refusal message or the tree written back. Statements are cut as the corpus
// benchmark cuts them (batches at GO, statements at ';'), because splitting T-SQL properly needs
// the parser being measured. Each text is read whole and as its first 60%; the refusals of a
// prefix are where the messages are most varied.
if (args.Length != 2)
	throw new ArgumentException("Usage: DotGram.MessageDiff <corpus-directory> <output-file>");

var root  = Path.GetFullPath(args[0]);
var files = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories)
	.Select(file => Path.GetRelativePath(root, file).Replace('\\', '/'))
	.OrderBy(static file => file, StringComparer.Ordinal)
	.ToList();

using var output = new StreamWriter(args[1], false, new UTF8Encoding(false)) { NewLine = "\n" };

var lines = 0;

foreach (var file in files)
{
	var script = File.ReadAllText(Path.Combine(root, file));

	foreach (var (mode, text) in Cuts(script))
		Report(file, "tsql-script/" + mode, text, static one => Script(one));

	var at = 0;

	foreach (var statement in Statements(script))
	{
		at++;

		foreach (var (mode, text) in Cuts(statement))
		{
			Report(file, $"tsql-statement#{at}/{mode}", text, static one => TSql(one));
			Report(file, $"sql2023-direct#{at}/{mode}", text + ";", static one => Standard(one));
		}
	}
}

Console.WriteLine($"{lines} readings of {files.Count} files");

return;

IEnumerable<(string Mode, string Text)> Cuts(string text)
{
	yield return ("whole", text);
	yield return ("60%", text[..(text.Length * 6 / 10)]);
}

void Report(string file, string what, string text, Func<string, string> read)
{
	string said;

	try
	{
		said = read(text);
	}
	catch (Exception exception)
	{
		said = "threw " + exception.GetType().Name;
	}

	output.Write(file);
	output.Write('\t');
	output.Write(what);
	output.Write('\t');
	output.WriteLine(Escape(said));

	lines++;
}

static string Script(string text)
{
	var read = TransactSqlParser.TryParseScript(text);

	if (!read.IsSuccess)
		return Refused(read.Position, read.Error);

	var written = new StringBuilder("ok ");

	foreach (var batch in read.Value!)
	{
		foreach (var statement in batch.Statements)
			written.Append(SqlWriter.Write(statement)).Append(" ;; ");

		written.Append("GO ").Append(batch.Go).Append(" || ");
	}

	return written.ToString();
}

static string TSql(string text)
{
	var read = TransactSqlParser.TryParseStatement(text);

	return read.IsSuccess ? "ok " + SqlWriter.Write(read.Value!) : Refused(read.Position, read.Error);
}

static string Standard(string text)
{
	var read = SqlStandardParser.TryParseDirectSQLStatement(text);

	return read.IsSuccess ? "ok " + Sql2023Writer.Write(read.Value!) : Refused(read.Position, read.Error);
}

static string Refused(long position, string? error)
{
	return $"refused at {position}: {error}";
}

static string Escape(string text)
{
	return text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\t", "\\t", StringComparison.Ordinal);
}

static IEnumerable<string> Statements(string script)
{
	var batch = new StringBuilder();

	foreach (var line in script.Split('\n'))
	{
		var one = line.TrimEnd('\r');

		if (one.TrimStart().StartsWith("--", StringComparison.Ordinal))
			continue;

		if (string.Equals(one.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
		{
			foreach (var statement in Split(batch.ToString()))
				yield return statement;

			batch.Clear();

			continue;
		}

		batch.Append(one).Append('\n');
	}

	foreach (var statement in Split(batch.ToString()))
		yield return statement;

	static IEnumerable<string> Split(string batch)
	{
		foreach (var statement in batch.Split(';'))
			if (statement.Trim() is { Length: > 0 } one)
				yield return one;
	}
}
