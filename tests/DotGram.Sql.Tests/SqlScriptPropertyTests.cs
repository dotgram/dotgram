using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <see cref="SqlScript"/> held to what must hold of any script, whatever sqlcmd makes of it: scripts
/// built at random from the pieces the reader turns on, and every reading checked against itself.
/// </summary>
/// <remarks>
/// The oracle rows say what the reader answers; these say that the answer is one answer — that a
/// batch is the text it claims to be a window of, that every position maps back to where it was
/// written, and that the readings of a script in its several forms agree.
/// </remarks>
public sealed class SqlScriptPropertyTests
{
	static readonly string[] Pieces =
	[
		"PRINT 1", "SELECT 'a", "b'", "'", "\"", "[", "]", "]]", "''", "/*", "*/", "/* c */", "--", "-- c", "-", "/",
		"GO", "go", "Go 2", "GO 0", "GO 3", "GO -1", "GO x", "GO;", " ", "\t", "\r", "\n", "\n", "\n", "\r\n", "\r\n",
		"$(x)", "$(y)", "$(e)", "$(nope)", "$(", "$", ":setvar x 1", ":setvar y \"a b\"", ":setvar x", ":setvar x $(y)",
		":setvar e \"\"", "/* c */ GO", " GO", "x", "SELECT 1", ";", ":on error exit", "exit", "exit(SELECT 1)",
		":r i.sql", ":r nope.sql", "GO $(n)", ":setvar n 2", "﻿", "é", "GOTO", "GO--c", "*", ":", "(", ")",
	];

	static readonly Dictionary<string, string> Files = new(StringComparer.Ordinal)
	{
		["i.sql"] = "SELECT 9\nGO\nSELECT 'z\n",
	};

	public static TheoryData<int> Seeds()
	{
		return [1, 2, 3, 4];
	}

	[Theory]
	[MemberData(nameof(Seeds))]
	public void A_random_script_reads_consistently(int seed)
	{
		var random = new Random(seed);

		for (var i = 0; i < 500; i++)
		{
			var text = Script(random);

			foreach (var ssms in new[] { false, true })
			{
				var problem = Check(text, ssms);

				Assert.True(problem is null, $"{problem} (ssms: {ssms}) in {System.Text.Json.JsonSerializer.Serialize(text)}");
			}
		}
	}

	[Fact]
	public void Reading_in_parallel_answers_as_reading_alone()
	{
		var texts = Enumerable.Range(0, 64)
			.Select(static i => $":setvar v{i} {i}\nSELECT $(v{i})\nGO {i % 3}\n" + string.Concat(Enumerable.Repeat("SELECT 'x\nGO\ny'\n/* c */ GO\n", i % 7)))
			.ToArray();
		var expected = texts.Select(Sent).ToArray();
		var wrong    = 0;

		Parallel.For(0, 4096, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
		{
			if (Sent(texts[i % texts.Length]) != expected[i % texts.Length])
				Interlocked.Increment(ref wrong);
		});

		Assert.Equal(0, wrong);
	}

	static string Sent(string text)
	{
		return string.Join("\u0001", SqlScript.Read(text).Batches.Select(static one => one.ToString() + "\u0002" + one.Count));
	}

	static string Script(Random random)
	{
		var builder = new StringBuilder();
		var count   = random.Next(1, 25);

		for (var i = 0; i < count; i++)
		{
			if (random.Next(3) == 0)
				builder.Append('\n');

			builder.Append(Pieces[random.Next(Pieces.Length)]);
		}

		return builder.ToString();
	}

	static readonly Regex Reference = new(@"^\$\([A-Za-z_][A-Za-z0-9_-]*\)$", RegexOptions.CultureInvariant);

	static string? Check(string text, bool ssms)
	{
		var options = new ScriptOptions
		{
			Profile        = ssms ? ScriptProfile.Ssms() : ScriptProfile.SqlCmd(),
			SourceName     = "main",
			Variables      = new Dictionary<string, string> { ["y"] = "YY" },
			ResolveInclude = static include => Files.TryGetValue(include.Path, out var file) ? new ScriptSource(include.Path, file) : null,
		};
		var script = SqlScript.Read(text, options);
		var last   = -1;

		foreach (var batch in script.Batches)
		{
			if (batch.Count < 0)
				return "a negative count";

			if (batch.IsVerbatim)
			{
				if (batch.At < 0 || batch.At + batch.Length > batch.Text.Length)
					return "a window outside its text";

				var location = batch.Locate(batch.At, batch.Length);

				if (location.Span != new SqlSpan(batch.At, batch.Length))
					return "a verbatim batch that Locate moves";

				if (ReferenceEquals(batch.Text, text))
				{
					if (batch.At < last)
						return "batches out of order";

					last = batch.At + batch.Length;
				}

				continue;
			}

			if (batch.At != 0 || batch.Length != batch.Text.Length)
				return "an assembled batch that is not the whole of its text";

			var previous       = -1;
			var previousSource = default(string);

			for (var at = 0; at < batch.Length; at++)
			{
				var location = batch.Locate(at, 1);
				var source   = location.Source == "main" ? text : location.Source is { } name && Files.TryGetValue(name, out var file) ? file : null;

				if (source is null)
					return "a position in no source: " + location.Source;

				if (location.Span.At < 0 || location.Span.End > source.Length)
					return $"Locate({at}, 1) outside its source";

				var written = source.Substring(location.Span.At, location.Span.Length);

				// A line break the tool adds where a file ends in a string or a comment was written
				// nowhere, and stands at the end of that file.
				var added = written.Length == 0 && batch.Text[at] == '\n' && location.Span.At == source.Length;

				if (written != batch.Text.Substring(at, 1) && !Reference.IsMatch(written) && !added)
					return $"Locate({at}, 1) maps '{batch.Text[at]}' to '{written}'";

				if (location.Source == previousSource && location.Span.At < previous)
					return $"Locate goes back at {at}";

				previous       = location.Span.At;
				previousSource = location.Source;
			}
		}

		if (ssms)
			return null;

		var match        = TransactSqlParser.TryParseScript(text);
		var quiet        = TransactSqlParser.TryParseScript(text, out var value);
		var located      = TransactSqlParser.Located.TryParseScript(text);
		var locatedQuiet = TransactSqlParser.Located.TryParseScript(text, out _);
		var threw        = false;

		try
		{
			TransactSqlParser.ParseScript(text);
		}
		catch (FormatException)
		{
			threw = true;
		}

		if (match.IsSuccess != quiet || match.IsSuccess != located.IsSuccess || match.IsSuccess != locatedQuiet || match.IsSuccess == threw)
			return $"the forms disagree: Match {match.IsSuccess}, bool {quiet}, Located {located.IsSuccess}, Located bool {locatedQuiet}, threw {threw}";

		if (!match.IsSuccess)
			return located.Position == match.Position ? null : $"refusals at {match.Position} and, located, {located.Position}";

		if (match.Value.Length != value.Length || match.Value.Length != located.Value.Length)
			return "the forms disagree on the batches";

		for (var i = 0; i < value.Length; i++)
		{
			var statements = match.Value[i].Statements.Length;

			if (statements != value[i].Statements.Length || statements != located.Value[i].Statements.Length)
				return $"the forms disagree on the statements of batch {i}";

			var whole = TransactSqlParser.TryParseSql(match.Value[i].Source!.ToString());

			if (whole.IsSuccess && whole.Value.Length != statements)
				return $"batch {i} read through a window has {statements} statements, and alone {whole.Value.Length}";
		}

		return null;
	}
}
