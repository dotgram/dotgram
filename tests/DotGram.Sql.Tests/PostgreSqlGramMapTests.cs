using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <c>PostgreSql/Specification/gram-map.csv</c> says where every nonterminal of PostgreSQL's grammar goes, and has to
/// keep saying it: a rule of <c>PostgreSql.gram</c> that exists, <c>token</c>, <c>erased</c>, <c>refused</c> with a
/// note, or <c>unwritten</c> while it waits.
/// </summary>
/// <remarks>
/// The rows are written by a script over the server's <c>gram.y</c>, so the inventory is complete from the start and
/// what is left is what the <c>unwritten</c> rows say. A rule renamed in the grammar leaves a row that fails here.
/// </remarks>
public sealed class PostgreSqlGramMapTests
{
	static readonly string[] Words = ["unwritten", "token", "erased", "refused"];

	[Fact]
	public void Every_nonterminal_goes_somewhere_that_exists()
	{
		var folder  = Path.Combine(SourceRoot.Directory, "src", "DotGram.Sql", "PostgreSql");
		var grammar = File.ReadAllText(Path.Combine(folder, "PostgreSql.gram"));
		var rules   = Regex.Matches(grammar, @"^\t?([A-Za-z][A-Za-z0-9]*)(\([a-z]+\))?\s*(:|=)", RegexOptions.Multiline)
			.Select(static one => one.Groups[1].Value)
			.ToHashSet(StringComparer.Ordinal);
		var rows    = File.ReadAllLines(Path.Combine(folder, "Specification", "gram-map.csv")).Skip(1)
			.Where(static line => line.Length > 0)
			.Select(static line => line.Split(',', 4))
			.ToList();

		Assert.True(rows.Count > 700, $"{rows.Count} rows: the inventory is not the whole grammar");

		var faults = rows
			.Where(row => row.Length < 3 || !(Words.Contains(row[2]) || row[2].Split(';').All(rules.Contains)))
			.Select(static row => string.Join(",", row))
			.ToList();
		var refusedWithoutReason = rows.Where(static row => row.Length >= 4 && row[2] == "refused" && row[3].Length == 0).Select(static row => row[0]).ToList();

		Assert.Empty(faults);
		Assert.Empty(refusedWithoutReason);
	}

	[Fact]
	public void The_nonterminals_are_listed_once()
	{
		var names = File.ReadAllLines(Path.Combine(SourceRoot.Directory, "src", "DotGram.Sql", "PostgreSql", "Specification", "gram-map.csv")).Skip(1)
			.Where(static line => line.Length > 0)
			.Select(static line => line.Split(',')[0])
			.ToList();

		Assert.Equal(names.Count, names.Distinct(StringComparer.Ordinal).Count());
	}
}
