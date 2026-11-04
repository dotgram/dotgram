using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using DotGram.Sql.PostgreSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// The PostgreSQL grammar held to the server's own parser: every row of <c>Scripts/pg-oracle.json</c> carries the
/// verdict PostgreSQL's grammar gave the text — read or refused — and <see cref="PostgreSqlParser"/> must answer the
/// same.
/// </summary>
/// <remarks>
/// <para>
/// The verdicts are recorded, so that this runs with nothing but the fixture: a private script puts each row's
/// <c>text</c> to libpg_query, which is the server's <c>gram.y</c> and <c>scan.l</c> compiled apart from the server —
/// syntax alone, no catalogue — and writes what it said, with its version. Where the live server answers otherwise
/// (<c>$1x</c>: the server's lexer refuses the junk, libpg_query 18.6 reads a label), the row carries the server's
/// verdict as <c>server</c>, and the server wins. A row's <c>expression</c> is the part of
/// the text after <c>SELECT</c>, read here through <c>ParseExpression</c> while that is the level published; a row
/// with no <c>expression</c> is a statement, read through <c>ParseStatement</c> once that level exists.
/// </para>
/// <para>
/// Both cells are faults: what PostgreSQL reads and this refuses is the work list, what this reads and PostgreSQL
/// refuses is a defect. A row the grammar knowingly reads otherwise says why in <c>known</c>, and is held to that.
/// </para>
/// </remarks>
public sealed class PostgreSqlOracleTests
{
	sealed record Row(string Name, string Text, string? Expression, bool Accepted, string? Known);

	/// <summary>Whether the text is read, by the server where it was asked and disagreed, by libpg_query otherwise.</summary>
	static bool Read(JsonElement row)
	{
		return row.TryGetProperty("server", out var server) ? server.GetBoolean() : row.GetProperty("accepted").GetBoolean();
	}

	static IReadOnlyList<Row> Rows()
	{
		var text = File.ReadAllText(Path.Combine(SourceRoot.Directory, "tests", "DotGram.Sql.Tests", "Scripts", "pg-oracle.json"));

		using var document = JsonDocument.Parse(text);

		var oracle = document.RootElement.GetProperty("oracle").GetString();

		Assert.False(string.IsNullOrEmpty(oracle), "the fixture records no oracle version: its verdicts were never recorded");

		return document.RootElement.GetProperty("rows").EnumerateArray()
			.Select(static row => new Row(
				row.GetProperty("name").GetString()!,
				row.GetProperty("text").GetString()!,
				row.TryGetProperty("expression", out var expression) ? expression.GetString() : null,
				Read(row),
				row.TryGetProperty("known", out var known) ? known.GetString() : null))
			.ToList();
	}

	[Fact]
	public void The_grammar_answers_every_recorded_row_as_the_server_does()
	{
		var faults = new List<string>();

		foreach (var row in Rows())
		{
			var read = row.Expression is { } expression
				? PostgreSqlParser.TryParseExpression(expression).IsSuccess
				: throw new NotSupportedException($"{row.Name}: a statement row, which no published level reads yet");
			var expected = row.Known is null ? row.Accepted : !row.Accepted;

			if (read != expected)
				faults.Add($"{row.Name}: PostgreSQL {(row.Accepted ? "reads" : "refuses")} `{row.Text}`, this {(read ? "reads" : "refuses")} it{(row.Known is null ? "" : $" (known: {row.Known})")}");
		}

		Assert.Empty(faults);
	}

	/// <summary>A known deviation is a row the server and the grammar answer differently; one they agree on is stale.</summary>
	[Fact]
	public void Every_known_deviation_is_still_one()
	{
		var stale = Rows()
			.Where(static row => row.Known is not null && row.Expression is not null)
			.Where(static row => PostgreSqlParser.TryParseExpression(row.Expression!).IsSuccess == row.Accepted)
			.Select(static row => row.Name)
			.ToList();

		Assert.Empty(stale);
	}
}
