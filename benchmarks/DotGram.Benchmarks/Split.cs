using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotGram.Parsers.Sql;

using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// `--split [path] [version] [shown]`: where this grammar cuts a file into statements, held
/// against where ScriptDom does.
/// </summary>
/// <remarks>
/// <para>
/// The corpus is cut into statements by ScriptDom, and it stays so: a file this grammar does
/// not read whole would otherwise vanish from every count with all its statements, and the
/// statements it cannot read are the work list. ScriptDom is the knife here and not the
/// authority, and this is what checks the knife against `ParseSql`: every file ScriptDom
/// reads whole is read here whole as well, and the two lists of where a statement begins
/// are compared.
/// </para>
/// <para>
/// `GO` is not T-SQL: it is the line a client cuts a script into batches at, and neither the
/// server nor `ParseSql` ever sees it. So the file is cut the way a client cuts it — at a
/// line that is `GO`, with a count or a comment after it — and each batch is read on its
/// own, its offsets moved back to the file's.
/// </para>
/// </remarks>
static class Split
{
	internal static void Run(string? path, string version, int shown)
	{
		var root     = path ?? Corpus.Checked();
		var files    = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var versions = Corpus.Versions(root, files);
		var readers  = new Dictionary<string, TSqlParser>(StringComparer.Ordinal);

		var theirs     = 0;
		var statements = 0;
		var same       = 0;
		var apart      = 0;
		var refused    = 0;
		var lost       = 0;
		var cuts       = new List<string>();
		var stopped    = new Dictionary<string, (int Count, List<string> Like)>(StringComparer.Ordinal);

		foreach (var file in files)
		{
			var text = File.ReadAllText(file);

			using var reader = new StringReader(text);

			if (Kinds.Reader(readers, version, versions.GetValueOrDefault(file)).Parse(reader, out var errors)
					is not TSqlScript script
				|| errors.Count > 0)
				continue;

			theirs++;

			var there = script.Batches
				.SelectMany(static batch => batch.Statements)
				.Select(static statement => (At: statement.StartOffset, Length: statement.FragmentLength))
				.ToArray();

			statements += there.Length;

			var (here, at) = Read(text);

			if (here is null)
			{
				refused++;
				lost += there.Length;

				var why       = Corpus.Stopped(text, at);
				var statement = there.FirstOrDefault(one => one.At <= at && at < one.At + one.Length);
				var example   = statement.Length > 0
					? Corpus.One(text.Substring(statement.At, statement.Length))
					: Corpus.One(text[at..Math.Min(text.Length, at + 120)]);

				var (count, like) = stopped.TryGetValue(why, out var before) ? before : (0, new List<string>());

				if (like.Count < shown)
					like.Add(example);

				stopped[why] = (count + 1, like);
				continue;
			}

			if (here.SequenceEqual(there.Select(static one => one.At)))
			{
				same++;
				continue;
			}

			apart++;

			if (cuts.Count < shown * 10)
				cuts.Add(Apart(file, text, here, there.Select(static one => one.At).ToArray()));
		}

		Console.WriteLine();
		Console.WriteLine($"where a statement begins, against ScriptDom, TSql{version}Parser");
		Console.WriteLine();
		Console.WriteLine($"  {files.Length} files, {theirs} read whole by ScriptDom, holding {statements} statements");
		Console.WriteLine($"    {same,5}  read here too, and cut the same");
		Console.WriteLine($"    {apart,5}  read here too, and cut differently");
		Console.WriteLine($"    {refused,5}  not read here whole — {lost} statements in them");

		if (cuts.Count > 0)
		{
			Console.WriteLine();
			Console.WriteLine("  cut differently, at the first place the two disagree:");

			foreach (var cut in cuts)
				Console.WriteLine(cut);
		}

		Console.WriteLine();
		Console.WriteLine("  where the files not read here stopped:");
		Console.WriteLine();

		foreach (var (why, one) in stopped.OrderByDescending(static one => one.Value.Count).ThenBy(static one => one.Key))
		{
			Console.WriteLine($"  {one.Count,5}  stops at {why}");

			foreach (var example in one.Like)
				Console.WriteLine($"           {example}");
		}
	}

	/// <summary>Where each statement begins, batch by batch — or, where the text is not read, where it stopped.</summary>
	static (int[]? Starts, int At) Read(string text)
	{
		var starts = new List<int>();

		foreach (var (at, batch) in Batches(text))
		{
			var match = TransactSql.Located.TryParseSql(batch);

			if (!match.IsSuccess)
				return (null, at + (int)match.Position);

			starts.AddRange(match.Value.Select(statement => at + statement.Span.At));
		}

		return ([.. starts], 0);
	}

	/// <summary>A line a client cuts a script at: `GO`, and a count or a comment after it.</summary>
	static readonly Regex Go = new(
		@"^[ \t]*GO(?:[ \t]+\d+)?[ \t]*(?:--[^\r\n]*)?\r?$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

	/// <summary>The batches a client would send, each with where it begins in the file.</summary>
	static IEnumerable<(int At, string Text)> Batches(string text)
	{
		var at = 0;

		foreach (Match go in Go.Matches(text))
		{
			yield return (at, text[at..go.Index]);

			at = go.Index + go.Length;
		}

		yield return (at, text[at..]);
	}

	/// <summary>The first statement the two lists begin at different places, from both sides.</summary>
	static string Apart(string file, string text, int[] here, int[] there)
	{
		var i = 0;

		while (i < here.Length && i < there.Length && here[i] == there[i])
			i++;

		string From(int[] starts) =>
			i < starts.Length ? Corpus.One(text[starts[i]..Math.Min(text.Length, starts[i] + 100)]) : "(nothing)";

		return
			$"      {Path.GetFileName(file)}: {here.Length} here, {there.Length} there\n" +
			$"           here   {From(here)}\n" +
			$"           there  {From(there)}";
	}
}
