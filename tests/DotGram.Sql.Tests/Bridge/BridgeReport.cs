using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGram.Sql.Tests;

/// <summary>
/// <c>docs/design/sql-tsql-bridge.md</c>: what the bridge is for and how it is run, the closed lists the oracle
/// reads by, the known writer defects, and a table written by <c>BridgeTests</c> of every old record — its
/// inventory category, what became of the statements counted under it, every gap it has, and how often it
/// occurs — sorted by how often, so that the next task starts with what matters.
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
		`src/DotGram.Sql/Ast/SqlAst*.cs`, the writer first (`sql-parsers.md`, `sql-tsql-tree.md`). Before the
		grammar is switched, every construct T-SQL reads has to have a place in the new tree and be printed back by
		`Sql2023Writer`. The bridge proves that family by family, without touching the grammar:

		1. `TransactSqlParser.Located` reads a statement into the old tree;
		2. `Bridge.ToAst` carries it onto the new tree, or throws a `BridgeGap` naming the old record and why;
		3. `TransactSqlWriter.Write` prints the new tree, refuses it, or crashes;
		4. `TransactSqlParser` reads the text back, and the old tree it builds is held to the first.

		Each statement is **converted** (the same tree came back), a **gap**, **refused** by the writer (it said no:
		`NotSupportedException`, a rejected argument or operation), **crashed** (anything else the writer threw),
		**unread** (the text written does not read), or **different** (it reads as another tree). Where
		`SqlStandardParser` reads the same text, the bridge's tree is also held to the one the standard's parser
		builds. The statements are those of `tests/Corpus/ScriptDom`, enumerated as `TreeEqualityTests` does, and every
		input `TransactSqlTests` expects to read (gathered by reflection over its `[InlineData]`, leaving out the
		theories whose name says they refuse and the rows whose `bool` says not read).

		A gap is one of five reasons: **no target record**, **target exists, no writer arm**, **needs decision**
		(naming the inventory row), **not kept by the old tree**, and **bridge arm missing** (the target is there and
		written, and the bridge has no arm yet). Whether the writer has an arm for a declared target is asked of the
		writer itself.

		## The shape check, and the dialects' different meanings

		Before the two trees are compared, the bridge's is projected where T-SQL holds a thing its own way and the
		standard another (`BridgeOracle.Projection`, written once there), and these two projections are the only
		comparisons that are not plain equality:

		- A T-SQL column's options, in the order written, onto the standard's members: a `COLLATE` right after a
		  character type into the type, as the standard's grammar reads it there; then a `DEFAULT` into `Generation`,
		  as `GENERATED ALWAYS AS ROW START` and `ROW END` are; the constraints in order into `Constraints`; a last
		  `COLLATE` into `Collation`. Only where the options stand in that order and each has its counterpart there.
		- A PRIMARY KEY's or a UNIQUE's `Keys` onto its `Columns`, where each is a one-part name with no direction and
		  the key has no other T-SQL part.

		A column or a key the projection does not apply to is left as it is, so that it differs, and a projected
		column with one option wrong still differs: either is a shape mismatch. Each projection made is counted
		(`Projected`).

		The projection makes the standard's form out of T-SQL's, so the bridge must have built T-SQL's: before
		anything is compared the oracle asks every column of the bridge's tree for empty `Generation`, `Constraints`
		and `Collation` and no collation inside its type, and every PRIMARY KEY and UNIQUE for empty `Columns`
		(`BridgeOracle.Unprojected`). A tree in the standard's form there is counted **different**.

		Every place the two trees differ is then collected. A difference inside one of twelve subtrees, found by its
		construct and never by the statement's text, is the two dialects reading the text with different meanings,
		and is exempted (`BridgeOracle.Meaning`); any other difference makes the statement a **shape** mismatch.

		- `TOP (n) a`: a row count in T-SQL, a call to `TOP` aliased `a` in the standard. The `Query.Select`'s `Top`
		  and its first column.
		- `SET t.c = …`: a qualified column in T-SQL, a mutated set clause on `t` in the standard. The
		  `AssignmentTarget` whose name has more than one part.
		- `GLOBAL c`: a cursor of the connection in T-SQL, an extended name in the standard. The `CursorReference`
		  that is `Global`.
		- `FREETEXT ((c), …)`, `CONTAINS ((c), …)`: a column list in T-SQL, a value in brackets in the standard. The
		  first argument of an invocation of either.
		- `DATEADD (day, …)`: a date part in T-SQL, a column in the standard. The first argument of a date part
		  function, a `DatePart`, where the standard has a column of one part whose text is that part's word.
		- `CAST (a AS money)`: one of T-SQL's system types in T-SQL, a user-defined type in the standard. A system
		  type with no arguments — a `DataType.Named` of one part, or `sys` and a name, on the list of system types, a
		  standard record written as a name (`WrittenName`), or an `nvarchar` with no length — where the standard has
		  a user-defined type of the same name, or, for `timestamp` with or without a length, its own TIMESTAMP.
		- `GROUP BY ALL a`: the empty groups kept in T-SQL, the quantifier that changes nothing in the standard. The
		  `Quantifier` and `AllKeyword` of a `GroupByClause` that is `AllKeyword`.
		- `SELECT a INTO t`: a new table in T-SQL, a target in the standard. A `Query.Select`'s `IntoTable` with no
		  filegroup, and its `Into` where the standard's one target names the same table.
		- `FROM t OPTION (h)`: a hint in T-SQL, an alias with columns in the standard. A statement's `Hints`, and an
		  `Alias` the standard reads the word OPTION as.
		- `SELECT x = a`: an alias in T-SQL, a comparison in the standard. A `SelectColumn.ExpressionItem` that is
		  `AliasFirst`.
		- `UPDATE ONLY (t)`, `DELETE FROM ONLY (t)`: a call to a function `ONLY` in T-SQL, the table `t` alone in the
		  standard. The statement's `Target`, a `TableSource.Function` of the one-part name `ONLY`. The standard's
		  `INSERT` reads no `ONLY`, and the writer refuses it there.
		- `c t DEFAULT v COLLATE x`: the default value's collation in T-SQL, the column's in the standard. The
		  `ColumnDefinition` whose default is an `Expression.Collate`: the default's value against the standard's,
		  which must be the collated value, and the column's `Collation`, which must be the value's collation. Only
		  both together are exempted; either alone is a shape mismatch.

		## Words read from the text

		The old tree does not keep every word the new tree records. The bridge reads these, and only these, from the
		statement's text through the old nodes' spans (`Bridge.Recoverable`); each reading is counted, the shape check
		still applies to it, and only the gate for switching the grammar leaves them out:

		- `SelectColumn.ExpressionItem.AsKeyword`: `a AS x` against `a x`.
		- `Clause.DerivedColumn alias = expression`: `x = a`, which is `SelectColumn.ExpressionItem.AliasFirst`.
		- `Alias.AsKeyword of a table source`: `t AS x` against `t x`; a word AS, never a name `"as"` or `[as]`, and
		  not the AS of `FOR SYSTEM_TIME AS OF`.
		- `Alias.AsKeyword of MERGE's target`.
		- `Limit.OffsetRowWord`, `Limit.Position`, `Limit.FetchRowWord`: `ROW` against `ROWS`, `FIRST` against `NEXT`.
		- `WindowReference.NameRef against (name)`: `OVER w` against `OVER (w)`.
		- `Expression.Invocation without brackets`: a call with no arguments written without its brackets.
		- `Expression.Trim.FromKeyword`: `TRIM (LEADING FROM s)` against `TRIM (LEADING s)`.
		- `Statement.DropRoutine.Proc`: `PROC` against `PROCEDURE`.
		- `Statement.SetTransaction.Tran`: `TRAN` against `TRANSACTION`.
		- `Statement.Delete.FromOmitted`: `DELETE t` against `DELETE FROM t`.
		- `ReferencesSpecification.DeleteRuleFirst`: `ON DELETE … ON UPDATE …` against the other order, where both
		  are written; a delete rule alone is first, as the standard's parser has it.
		- `Statement.ExecuteModule.Keyword`: `EXEC` against `EXECUTE` before a module; a bare call has neither.
		- `Statement.ExecuteString.Exec`: `EXEC` against `EXECUTE` before a string run.
		- `RoutineDefinition.Proc`: `PROC` against `PROCEDURE` after CREATE or ALTER.
		- `ParameterDefinition.AsKeyword`: `@a AS INT` against `@a INT`.

		## Prefixed pivot and unpivot names

		`TableSource.Pivot` and `TableSource.Unpivot` hold their names as `QualifiedName`. A PIVOT's FOR column and an
		UNPIVOT's IN list name existing columns, and SQL Server runs them qualified. A PIVOT's IN list and an UNPIVOT's
		value and FOR columns name new columns: the parser reads a prefix there, and compilation refuses it (Msg 254 and
		Msg 275). The corpora hold 4 prefixed statements, all of the first kind:

		- `SELECT * FROM st PIVOT (AVG (.st.StandardCost) FOR st.DaysToManufacture IN ([0], [1])) AS PivotTable;`, in two
		  spellings.
		- `SELECT * FROM t1 CROSS APPLY t2 UNPIVOT (q FOR n IN (t1.c0, c1)) AS a;`, in two spellings.

		## Key words kept as written

		The old tree keeps some key words in the case they were written in (`distinct`, `current_user`, a cast's
		`varchar (10)`), which the new tree keeps as a meaning and prints in capitals. The members that hold such text
		are a closed list (`Trees.FreeText`) and compare token by token: blanks only between tokens, a word without
		case only where both are in the closed list of key words the writer capitalises (`Trees.KeyWords`), a
		national string's `N` without case, and every other token — a name in brackets or quotes, a string, a
		number, any other word — exactly. A statement that passes only so is converted, and counted per member.

		## Definitions kept as text

		The old tree keeps CREATE STATISTICS, UPDATE STATISTICS, CREATE TYPE, CREATE SYNONYM, CREATE and ALTER
		FULLTEXT INDEX, a sequence and the action of ALTER TABLE … ALTER INDEX as a name and the text after it. The
		bridge cuts that text at its brackets and its commas outside them, and reads each part again by the grammar's
		own rule for it: a value as a select list's, a condition as a WHERE, a list of options as a table's, a list of
		partitions as TRUNCATE TABLE's, a table type's body as a table's. Each such text is in the closed list above.

		A view's verb is null for the plain CREATE, which is the standard's and what its parser builds; `ALTER` and
		`CREATE OR ALTER` are values of it, and `DefinitionVerb.Create`, the same text as null, is refused.

		## Checks for the grammar switch

		Where the new tree reads a text differently from the old one, the T-SQL grammar has to build the new form
		when it is switched:

		- `((SELECT 1))` as a value is `Subquery(Query.Parenthesized(…))`: a subquery's brackets are its own, and only
		  the second pair is a node. The old tree has `Parenthesized(Subquery(…))`.
		- `DATEADD (day, 1, d)` and the other date part functions take a `DatePart` first, where the old tree has a
		  column.
		- `{fn CONVERT (a, SQL_INTEGER)}` holds a plain `Invocation`: the call inside an ODBC escape is ODBC's, never
		  T-SQL's `Convert`.
		- `CAST (DISTINCT 1)` and `PARSE (ALL a)` are calls with a quantifier, never a `Cast` or a `Parse`: the old
		  tree keeps the quantifier where the type would stand.

		## The ratchet

		`Bridge/Baseline.<Family>.txt` holds, for every old record a family owns,
		`converted/gap/refused/crashed/unread/different/shape`: a statement is counted under the record its first gap
		names, or else under its own root. `Bridge/Baseline.Counters.txt` holds the debts: per member, how many
		statements passed only with key words' case aside (`ByCase`) and how many readings came from the text
		(`Recovered`); per pattern, how many subtrees it exempted (`Meaning`); per projection, how many columns and
		keys the shape check projected (`Projected`). `BridgeTests` fails where `converted`
		falls or anything else rises, where a statement fails the round trip and is not one of its `Expected`, and
		prints a gain without failing. A plain run writes nothing.

		The `ByCase` and `Recovered` counters rise as more statements convert, since each statement converted reads
		its words as the others do: a rise there that comes with more conversions is expected, and is recorded
		deliberately with the rest.

		A statement converts only where the written text, read back located and carried again with that text as its
		source, gives the same new tree, spans aside: the words read from the text are not in the old tree, so the
		old trees agreeing says nothing of them. Where it does not, the statement is different, its detail starting
		`new tree:`.
		""";

	public static void Write(
		IReadOnlyDictionary<string, string> families,
		IReadOnlyDictionary<string, Baselines.Counts> counts,
		IReadOnlyDictionary<string, Dictionary<string, int>> gaps,
		IReadOnlyDictionary<string, int> occurrences,
		IReadOnlyDictionary<string, int> counters,
		IReadOnlyDictionary<string, BridgeOracle.Status> expected)
	{
		var text = new StringBuilder();

		void Line(string line = "")
		{
			text.Append(line).Append("\r\n");
		}

		foreach (var line in Head.Split('\n'))
			Line(line.TrimEnd('\r'));

		Line();
		Line("## Known writer defects");
		Line();

		if (expected.Count == 0)
			Line("None. The six this bridge found — FETCH without INTO, DROP … IF EXISTS, DROP PROCEDURE x, y, DROP TRIGGER … ON, a bare RETURN, and `- -1` written as a comment — were fixed on main in c044587.");
		else
			foreach (var (statement, status) in expected.OrderBy(static one => one.Key, StringComparer.Ordinal))
				Line($"- {status}: `{statement}`");

		Line();
		Line("## The counters");
		Line();
		Line("| Counter | Count |");
		Line("| --- | ---: |");

		foreach (var (key, count) in counters.OrderBy(static one => one.Key, StringComparer.Ordinal))
			Line($"| {key.Replace("|", "\\|", StringComparison.Ordinal)} | {count} |");

		Line();
		Line("## The records");
		Line();
		Line("Category is the inventory's (`sql-tsql-tree-inventory.md`). Counted is what became of the statements counted");
		Line("under the record — those whose first gap it is, or whose root it is. Gaps are every gap the record has, a");
		Line("statement counted once for each record and reason it holds, with the commonest reasons. Occurrences are how");
		Line("many nodes of the record the statements hold, at any depth. All three are over the corpus and the tests' inputs.");
		Line();
		Line("| Old record | Family | Category | Counted | Gaps | Occurrences |");
		Line("| --- | --- | --- | --- | --- | ---: |");

		foreach (var (record, family) in families
			.OrderByDescending(one => occurrences.TryGetValue(one.Key, out var many) ? many : 0)
			.ThenBy(static one => one.Key, StringComparer.Ordinal))
		{
			var category = Inventory.Of(record)?.Category ?? "";
			var count    = counts.TryGetValue(record, out var found) ? found : Baselines.None;
			var seen     = occurrences.TryGetValue(record, out var many) ? many : 0;

			Line($"| `{record}` | {family} | {category} | {Status(count)} | {Reasons(gaps.TryGetValue(record, out var why) ? why : null)} | {seen} |");
		}

		File.WriteAllText(Page(), text.ToString());
	}

	static string Status(Baselines.Counts count)
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
		Add(count.Crashed, "crashed");
		Add(count.Unread, "unread");
		Add(count.Different, "different");
		Add(count.Shape, "shape");

		return parts.Count > 0 ? string.Join(", ", parts) : "—";
	}

	static string Reasons(Dictionary<string, int>? reasons)
	{
		if (reasons is not { Count: > 0 })
			return "—";

		return $"{reasons.Values.Sum()}: " + string.Join("; ", reasons
			.OrderByDescending(static one => one.Value)
			.ThenBy(static one => one.Key, StringComparer.Ordinal)
			.Take(2)
			.Select(static one => $"{one.Key.Replace("|", "\\|", StringComparison.Ordinal)} ({one.Value})"));
	}
}
