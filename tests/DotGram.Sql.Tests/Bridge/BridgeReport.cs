using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGram.Sql.Tests;

/// <summary>
/// <c>docs/design/sql-tsql-bridge.md</c>: what the bridge is for and how it is run, and a table written by
/// <c>BridgeTests</c> of every old record — its inventory category, what became of the statements counted
/// under it, and how often the corpus holds it — sorted by how often, so that the next task starts with
/// what matters.
/// </summary>
static class BridgeReport
{
	public static string Page([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "..", "docs", "design", "sql-tsql-bridge.md"));
	}

	const string Head =
		"""
		# The bridge from the old T-SQL tree to the new one

		Written by `BridgeTests` (`tests/DotGram.Sql.Tests/Bridge`), not edited by hand. Rewritten with the baselines:

		```text
		DOTGRAM_RECORD_BRIDGE_BASELINE=1 dotnet test --project tests/DotGram.Sql.Tests -c Release --filter-class DotGram.Sql.Tests.BridgeTests
		```

		## What it is for

		T-SQL moves from the tree in `src/DotGram.Sql/SqlSyntax.cs` onto the SQL:2023 tree in
		`src/DotGram.Sql/Standard/Sql2023Ast*.cs`, the writer first (`sql-parsers.md`, `sql-tsql-tree.md`). Before the
		grammar is switched, every construct T-SQL reads has to have a place in the new tree and be printed back by
		`Sql2023Writer`. The bridge proves that family by family, without touching the grammar:

		1. `TransactSqlParser.Located` reads a statement into the old tree;
		2. `Bridge.ToAst` carries it onto the new tree, or throws a `BridgeGap` naming the old record and why;
		3. `Sql2023Writer.Write` prints the new tree, or refuses;
		4. `TransactSqlParser` reads the text back, and the old tree it builds is held to the first.

		Each statement is **converted** (the same tree came back), a **gap**, **refused** by the writer, **unread**
		(the text written does not read), or **different** (it reads as another tree). Where `SqlStandardParser`
		reads the same text, the bridge's tree is also held to the one the standard's parser builds, and one that
		is not equal is a **shape** mismatch — unless the two dialects give the text different meanings, as T-SQL's
		`TOP (n) a` is a row count and the standard's a call to `TOP` aliased `a`; those are listed apart
		(`BridgeOracle.Meaning`) and are not counted. The statements are those of `tests/Corpus/ScriptDom`,
		enumerated as `TreeEqualityTests` does, and every input `TransactSqlTests` expects to read (gathered by
		reflection over its `[InlineData]`, leaving out the theories that refuse).

		## How it reads what the old tree drops

		The old tree keeps some words as text and drops others the new tree records: `AS` before an alias, `ROW`
		against `ROWS`, `FIRST` against `NEXT`, `PROC` against `PROCEDURE`, `OVER w` against `OVER (w)`. The bridge
		reads those from the statement's text through the old nodes' spans. And the old tree keeps some key words in
		the case they were written in (`distinct`, `current_user`, a cast's `varchar (10)`), which the new tree keeps
		as a meaning and prints in capitals: the comparison of old trees takes those members without case, and only
		those (`Trees.KeyWords`).

		## The ratchet

		`Bridge/Baseline.<Family>.txt` holds, for every old record a family owns, `converted/gap/refused/unread/
		different/shape`: a statement is counted under the record its gap names, or else under its own root.
		`BridgeTests` fails where `converted` falls or any of the last four rises, and prints a gain without
		failing. A plain run writes nothing.

		## The records

		Category is the inventory's (`sql-tsql-tree-inventory.md`). Status is what became of the statements counted
		under the record, with the commonest gap reasons. Occurrences are how many nodes of the record the corpus
		holds, at any depth.

		| Old record | Family | Category | Status | Occurrences |
		| --- | --- | --- | --- | ---: |
		""";

	public static void Write(
		IReadOnlyDictionary<string, string> families,
		IReadOnlyDictionary<string, Baselines.Counts> counts,
		IReadOnlyDictionary<string, Dictionary<string, int>> reasons,
		IReadOnlyDictionary<string, int> occurrences)
	{
		var text = new StringBuilder();

		foreach (var line in Head.Split('\n'))
			text.Append(line.TrimEnd('\r')).Append("\r\n");

		foreach (var (record, family) in families
			.OrderByDescending(one => occurrences.TryGetValue(one.Key, out var many) ? many : 0)
			.ThenBy(static one => one.Key, StringComparer.Ordinal))
		{
			var category = Inventory.Of(record)?.Category ?? "";
			var count    = counts.TryGetValue(record, out var found) ? found : Baselines.None;
			var seen     = occurrences.TryGetValue(record, out var many) ? many : 0;

			text.Append("| `").Append(record).Append("` | ").Append(family).Append(" | ").Append(category).Append(" | ")
				.Append(Status(count, reasons.TryGetValue(record, out var why) ? why : null)).Append(" | ")
				.Append(seen).Append(" |\r\n");
		}

		File.WriteAllText(Page(), text.ToString());
	}

	static string Status(Baselines.Counts count, Dictionary<string, int>? reasons)
	{
		var parts = new List<string>();

		void Add(int value, string word)
		{
			if (value > 0)
				parts.Add($"{word} {value}");
		}

		Add(count.Converted, "converted");
		Add(count.Gap, "gap");
		Add(count.Refused, "refused");
		Add(count.Unread, "unread");
		Add(count.Different, "different");
		Add(count.Shape, "shape");

		var status = parts.Count > 0 ? string.Join(", ", parts) : "not seen";

		if (reasons is { Count: > 0 })
			status += ": " + string.Join("; ", reasons.OrderByDescending(static one => one.Value).ThenBy(static one => one.Key, StringComparer.Ordinal)
				.Take(2).Select(static one => one.Key.Replace("|", "\\|", StringComparison.Ordinal)));

		return status;
	}
}
