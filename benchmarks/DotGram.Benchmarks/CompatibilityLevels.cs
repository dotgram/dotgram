using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Parsers;

using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// Which statements the engine's answer depends on the compatibility level for — the part of
/// T-SQL a version can be asked about on a server that has one parser.
/// </summary>
/// <remarks>
/// <para>
/// <c>--engine</c> asks one level at a time, and a level on its own cannot say what it gates:
/// a refusal at 130 means nothing about versions unless 160 reads the same statement. So this
/// asks every level about every statement and keeps only the statements whose answer moves.
/// What comes out is the engine's own partition of the language by level, with no parser's
/// opinion in it.
/// </para>
/// <para>
/// A message about names counts as read, as it does in <c>--engine</c>: the question is
/// whether the level changes the syntax, and <c>Must declare the scalar variable</c> at every
/// level is the same answer every time.
/// </para>
/// </remarks>
static class CompatibilityLevels
{
	static readonly string[] Asked = ["100", "110", "120", "130", "140", "150", "160", "170"];

	public static void Run(string? root, int shown)
	{
		root ??= Corpus.Checked();

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

			if (File.Exists(root))
				Probe(root, connections);
			else
				Ask(root, connections, shown);
		}
		finally
		{
			foreach (var connection in connections)
				connection.Dispose();
		}
	}

	/// <summary>A file of statements, one a line, each asked at every level.</summary>
	/// <remarks>
	/// Where a boundary is in doubt the corpus is the wrong instrument — it holds what somebody
	/// thought to test — and a line written to ask exactly the question is the right one: is it
	/// all of <c>OPENJSON</c> the level gates, or only its <c>WITH</c>?
	/// </remarks>
	static void Probe(string file, List<SqlConnection> connections)
	{
		Console.WriteLine();
		Console.WriteLine($"  {string.Join(" ", Asked.Select(static level => $"{level,5}"))}");

		foreach (var line in File.ReadAllLines(file))
		{
			if (line.Trim().Length == 0)
				continue;

			var answers = connections
				.Select(connection => Engine.Answer(connection, line))
				.Select(static message => Engine.AboutNames(message) ? 0 : message)
				.ToArray();

			// And this grammar's parser for each level beside the engine's answer, so that a
			// row where the two differ is a row that is plainly wrong.
			var ours = Asked.Select(level => Engine.Parse(level, line).Read ? "   ok" : "   no");

			Console.WriteLine($"  {Shape(answers)}   engine  {line}");
			Console.WriteLine($"  {string.Join(" ", ours)}   here");
		}
	}

	static void Ask(string root, List<SqlConnection> connections, int shown)
	{
		var files    = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var versions = Corpus.Versions(root, files);
		var readers  = new Dictionary<string, TSqlParser>(StringComparer.Ordinal);
		var seen     = new HashSet<string>(StringComparer.Ordinal);
		var asked    = 0;
		var moved    = new Dictionary<string, (int Count, int Here, List<string> Like)>(StringComparer.Ordinal);

		foreach (var file in files)
		{
			var text   = File.ReadAllText(file);
			var reader = Kinds.Reader(readers, "180", versions.GetValueOrDefault(file));

			using var input = new StringReader(text);

			if (reader.Parse(input, out var errors) is not TSqlScript script || errors.Count > 0)
				continue;

			foreach (var statement in script.Batches.SelectMany(static batch => batch.Statements))
			{
				var one = text.Substring(statement.StartOffset, statement.FragmentLength).TrimEnd();

				// The corpus repeats itself across its version directories, and a statement
				// asked twice would be counted twice.
				if (!seen.Add(one))
					continue;

				asked++;

				var answers = connections
					.Select(connection => Engine.Answer(connection, one))
					.Select(static message => Engine.AboutNames(message) ? 0 : message)
					.ToArray();

				if (answers.All(message => message == answers[0]))
					continue;

				var key  = $"{statement.GetType().Name,-40} {Shape(answers)}";
				var here = TransactSql.TryParseStatement(one).IsSuccess;

				var (count, read, like) = moved.TryGetValue(key, out var before) ? before : (0, 0, new List<string>());

				if (like.Count < shown)
					like.Add((here ? "  read here  " : "  refused    ") + Corpus.One(one));

				moved[key] = (count + 1, read + (here ? 1 : 0), like);
			}
		}

		Console.WriteLine();
		Console.WriteLine($"every compatibility level from {Asked[0]} to {Asked[^1]}, asked about each statement once");
		Console.WriteLine();
		Console.WriteLine($"  {asked} statements, {moved.Values.Sum(static one => one.Count)} of them answered differently by level");
		Console.WriteLine();
		Console.WriteLine($"  {"kind",-40} {string.Join(" ", Asked.Select(static level => $"{level,5}"))}   count  read here");
		Console.WriteLine();

		foreach (var (key, (count, read, like)) in moved.OrderBy(static one => one.Key, StringComparer.Ordinal))
		{
			Console.WriteLine($"  {key}   {count,5}  {read,9}");

			foreach (var example in like)
				Console.WriteLine($"        {example}");
		}
	}

	/// <summary>One column a level: <c>ok</c> where it reads, the message number where it does not.</summary>
	static string Shape(int[] answers) =>
		string.Join(" ", answers.Select(static message => message == 0 ? "   ok" : $"{message,5}"));
}
