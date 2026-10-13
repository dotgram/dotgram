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
3. `Sql2023Writer.Write` prints the new tree, refuses it, or crashes;
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

Every place the two trees differ is collected. A difference inside one of ten subtrees, found by its
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
  a user-defined type of the same name, or, for `timestamp`, its own TIMESTAMP.
- `GROUP BY ALL a`: the empty groups kept in T-SQL, the quantifier that changes nothing in the standard. The
  `Quantifier` and `AllKeyword` of a `GroupByClause` that is `AllKeyword`.
- `SELECT a INTO t`: a new table in T-SQL, a target in the standard. A `Query.Select`'s `IntoTable` with no
  filegroup, and its `Into` where the standard's one target names the same table.
- `FROM t OPTION (h)`: a hint in T-SQL, an alias with columns in the standard. A statement's `Hints`, and an
  `Alias` the standard reads the word OPTION as.
- `SELECT x = a`: an alias in T-SQL, a comparison in the standard. A `SelectColumn.ExpressionItem` that is
  `AliasFirst`.

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
(`Recovered`); per pattern, how many subtrees it exempted (`Meaning`). `BridgeTests` fails where `converted`
falls or anything else rises, where a statement fails the round trip and is not one of its `Expected`, and
prints a gain without failing. A plain run writes nothing.

The `ByCase` and `Recovered` counters rise as more statements convert, since each statement converted reads
its words as the others do: a rise there that comes with more conversions is expected, and is recorded
deliberately with the rest.

A statement converts only where the written text, read back located and carried again with that text as its
source, gives the same new tree, spans aside: the words read from the text are not in the old tree, so the
old trees agreeing says nothing of them. Where it does not, the statement is different, its detail starting
`new tree:`.

## Known writer defects

None. The six this bridge found — FETCH without INTO, DROP … IF EXISTS, DROP PROCEDURE x, y, DROP TRIGGER … ON, a bare RETURN, and `- -1` written as a comment — were fixed on main in c044587.

## The counters

| Counter | Count |
| --- | ---: |
| ByCase Clause.For.Kind | 20 |
| ByCase Clause.For.Options | 44 |
| ByCase Clause.GroupBy.With | 2 |
| ByCase Clause.Hint.Text | 28 |
| ByCase Clause.JsonColumn.Type | 54 |
| ByCase Clause.Option.Name | 4 |
| ByCase Clause.SystemTime.Kind | 0 |
| ByCase Clause.TableSample.Unit | 4 |
| ByCase Clause.Top.With | 0 |
| ByCase Clause.Window.Frame | 7 |
| ByCase Expression.ColumnReference.Text of a date part | 110 |
| ByCase Expression.GraphMatch.Pattern | 114 |
| ByCase Expression.Hinted.Words | 4 |
| ByCase Expression.Literal.Text | 26 |
| ByCase Expression.Match.Qualifier | 0 |
| ByCase Expression.NamedArgument.Name | 0 |
| ByCase Expression.OdbcEscape.Kind | 1 |
| ByCase Expression.Prefixed.Word | 23 |
| ByCase Expression.Quantified.Quantifier | 0 |
| ByCase Expression.RoutineInvocation.Name | 117 |
| ByCase Expression.RoutineInvocation.Word | 110 |
| ByCase Expression.WindowFunction.Within | 16 |
| ByCase Query.Specification.Quantifier | 5 |
| ByCase SetExpression.Transactions.IsolationLevel.Level | 0 |
| ByCase Statement.CursorAction.Verb | 0 |
| ByCase Statement.Fetch.Orientation | 0 |
| ByCase Statement.Insert.Into | 1 |
| ByCase Statement.SchemaDefinition.Tail | 2 |
| ByCase Statement.SequenceDefinition.Tail | 1 |
| ByCase Statement.Transaction.Kind | 0 |
| ByCase Statement.Transaction.Tail | 8 |
| ByCase Statement.Transaction.Word | 0 |
| ByCase TableReference.Joined.Hint | 0 |
| Meaning CAST (a AS money): a type of T-SQL's own in T-SQL, a user-defined type in the standard | 2 |
| Meaning DATEADD (day, ...): a date part in T-SQL, a column in the standard | 6 |
| Meaning FREETEXT ((c), ...): a column list in T-SQL, a value in brackets in the standard | 7 |
| Meaning FROM t OPTION (h): a hint in T-SQL, an alias in the standard | 10 |
| Meaning GLOBAL c: a cursor of the connection in T-SQL, an extended name in the standard | 6 |
| Meaning GROUP BY ALL a: the empty groups kept in T-SQL, a quantifier in the standard | 7 |
| Meaning SELECT a INTO t: a new table in T-SQL, a target in the standard | 3 |
| Meaning SELECT x = a: an alias in T-SQL, a comparison in the standard | 1 |
| Meaning SET t.c: a qualified column in T-SQL, a mutated set clause in the standard | 15 |
| Meaning TOP (n) a: a row count in T-SQL, a call to TOP in the standard | 2 |
| Recovered Alias.AsKeyword of MERGE's target | 7 |
| Recovered Alias.AsKeyword of a table source | 857 |
| Recovered Clause.DerivedColumn alias = expression | 350 |
| Recovered Expression.Invocation without brackets | 33 |
| Recovered Expression.Trim.FromKeyword | 0 |
| Recovered Limit.FetchRowWord | 23 |
| Recovered Limit.OffsetRowWord | 26 |
| Recovered Limit.Position | 23 |
| Recovered SelectColumn.ExpressionItem.AsKeyword | 345 |
| Recovered Statement.DropRoutine.Proc | 24 |
| Recovered Statement.SetTransaction.Tran | 23 |
| Recovered WindowReference.NameRef against (name) | 14 |

## The records

Category is the inventory's (`sql-tsql-tree-inventory.md`). Counted is what became of the statements counted
under the record — those whose first gap it is, or whose root it is. Gaps are every gap the record has, a
statement counted once for each record and reason it holds, with the commonest reasons. Occurrences are how
many nodes of the record the statements hold, at any depth. All three are over the corpus and the tests' inputs.

| Old record | Family | Category | Counted | Gaps | Occurrences |
| --- | --- | --- | --- | --- | ---: |
| `Expression.Literal` | Values | B | — | — | 9322 |
| `Expression.ColumnReference` | Values | B | — | 139: needs decision: inventory cross-cutting gaps, identifier styles (a name part `CERTIFICATE c`) (7); needs decision: inventory cross-cutting gaps, identifier styles (a name part `READ COMMITTED`) (6) | 9235 |
| `Clause.Option` | Tables | C | — | — | 6517 |
| `Clause.ColumnDefinition` | Tables | B | — | — | 3528 |
| `Query.Specification` | Queries | B | — | — | 3358 |
| `Clause.ColumnOption` | Tables | B | — | — | 2995 |
| `Statement.Select` | Queries | B | converted 2363 | — | 2976 |
| `Clause.DerivedColumn` | Queries | A | — | — | 2967 |
| `TableReference.Named` | Tables | A | gap 5 | 5: needs decision: inventory Clause.Hint (a hint written against the sample) (4); needs decision: inventory cross-cutting gaps, identifier styles (a name part `db `) (1) | 2944 |
| `Expression.RoutineInvocation` | Values | B | gap 56 | 71: needs decision: inventory cross-cutting gaps, identifier styles (a name part `GROUPING SETS`) (12); needs decision: inventory Expression.RoutineInvocation (the word `INT`) (8) | 2310 |
| `Statement.TableDefinition` | Tables | B | gap 1829 | 1878: needs decision: inventory Statement.TableDefinition (1878) | 1906 |
| `Clause.ConstraintDefinition` | Tables | B | — | — | 1841 |
| `Clause.SortSpecification` | Queries | A | — | — | 1303 |
| `Clause.QualifiedAsterisk` | Queries | A | — | — | 1221 |
| `Expression.Comparison` | Values | A | — | — | 841 |
| `Statement.AlterTable` | Tables | B | gap 644 | 644: needs decision: inventory Statement.AlterTable (644) | 644 |
| `Clause.EventPiece` | Server | C | — | — | 514 |
| `Expression.NamedArgument` | Values | A | gap 39 | 179: needs decision: inventory Expression.NamedArgument (179) | 506 |
| `Statement.CreateIndex` | Tables | C | gap 495 | 495: no target record: Statement.CreateIndex (495) | 495 |
| `TableReference.FunctionCall` | Tables | A | gap 6 | 6: needs decision: inventory TableReference.FunctionCall (a PREDICT that is not MODEL, DATA AS and a schema) (6) | 487 |
| `Expression.Parenthesized` | Values | A | — | — | 438 |
| `TableReference.Joined` | Tables | A | — | — | 382 |
| `Clause.VariableDeclaration` | Procedural | C | — | — | 357 |
| `Statement.Declare` | Procedural | C | gap 300 | 335: no target record: Statement.DeclareVariables (335) | 347 |
| `Statement.Compound` | Procedural | C | gap 17 | 328: no target record: Statement.Compound (328) | 337 |
| `Statement.EventSessionDefinition` | Server | C | gap 323 | 323: no target record: Statement.CreateEventSession (323) | 323 |
| `Clause.Hint` | Queries | C | gap 5 | 5: needs decision: inventory Clause.Hint (the old WITH (n)) (4); needs decision: inventory Clause.Hint (a hint of a shape the family does not hold) (1) | 294 |
| `Clause.ParameterDeclaration` | Routines | B | — | — | 290 |
| `Statement.CreateProcedure` | Routines | B | gap 279 | 279: needs decision: inventory Statement.CreateProcedure (279) | 279 |
| `Clause.Connection` | Tables | C | — | — | 270 |
| `Statement.EndpointDefinition` | Server | C | gap 249 | 249: no target record: Statement.CreateEndpoint (249) | 249 |
| `Statement.Insert` | Dml | A | converted 27, gap 50 | 72: no target record: Statement.Insert has no member for whether INTO was written (proposal 2) (28); needs decision: inventory Statement.Insert (no target) (16) | 242 |
| `Expression.Measured` | Values | C | — | 190: target exists, no writer arm: Expression.Measured (190) | 234 |
| `Statement.AlterDatabaseSet` | Database | C | gap 218 | 218: no target record: Statement.AlterDatabase (218) | 218 |
| `Expression.Add` | Values | A | — | — | 198 |
| `Expression.RowValueConstructor` | Values | A | — | — | 186 |
| `Expression.WindowFunction` | Values | B | gap 4 | 4: needs decision: inventory Expression.WindowFunction (a window over something that is not a call) (3); needs decision: inventory Expression.WindowFunction (a JSON aggregate with WITHIN GROUP or a null treatment, which its record has no place for) (1) | 184 |
| `Clause.Window` | Values | A | — | — | 181 |
| `Expression.Prefixed` | Values | C | gap 51 | 181: no target record: a typed T-SQL rowset table source (proposal 7) (181) | 181 |
| `Statement.CreateFunction` | Routines | B | gap 181 | 181: needs decision: inventory Statement.CreateFunction (181) | 181 |
| `Clause.OrderBy` | Queries | B | — | — | 177 |
| `Clause.Placement` | Tables | C | — | — | 177 |
| `Statement.TypeDefinition` | Routines | B | gap 174 | 174: needs decision: inventory Statement.TypeDefinition (174) | 174 |
| `Expression.Negate` | Values | A | — | — | 173 |
| `Statement.SetStatement` | Procedural | C | converted 23, gap 131 | 142: no target record: Statement.SetOptions (142) | 165 |
| `TableReference.Parenthesized` | Tables | A | — | — | 162 |
| `Clause.GroupBy` | Queries | B | — | — | 158 |
| `Clause.JsonColumn` | Tables | C | gap 4 | 4: needs decision: inventory Clause.ColumnOption (an explicit NULL, decided with CREATE TABLE's column options) (3); no target record: RowsetColumn.Ordinal is a number, and keeps no spelling of it such as `00` (1) | 155 |
| `Statement.WorkloadGroupDefinition` | Server | C | gap 155 | 155: no target record: Statement.CreateWorkloadGroup (155) | 155 |
| `Statement.CreateDatabase` | Database | C | gap 154 | 154: no target record: Statement.CreateDatabase (154) | 154 |
| `Clause.Set` | Dml | B | gap 47 | 47: no target record: Assignment has no compound operator (proposal 4) (27); no target record: Assignment has no Through for `@v = column = value` (proposal 4) (13) | 151 |
| `Statement.AlterIndex` | Tables | C | gap 148 | 148: no target record: Statement.AlterIndex (148) | 148 |
| `Statement.Execute` | Routines | B | gap 135 | 147: needs decision: inventory Statement.Execute (147) | 147 |
| `Clause.References` | Tables | A | — | — | 145 |
| `Statement.Return` | Procedural | A | converted 3 | — | 145 |
| `Statement.Update` | Dml | A | converted 40, gap 29 | 53: needs decision: inventory Statement.Update (no target) (20); no target record: Statement.Update has no OutputClause in the new tree (proposal 2) (13) | 140 |
| `Expression.GraphMatch` | Values | C | — | — | 131 |
| `Expression.Hinted` | Values | C | gap 23 | 127: needs decision: inventory Expression.Hinted (127) | 127 |
| `Expression.And` | Values | A | — | — | 122 |
| `Statement.Transaction` | Procedural | B | converted 45, gap 64 | 67: target exists, no writer arm: Statement.BeginTransaction (33); target exists, no writer arm: Statement.SaveTransaction (21) | 119 |
| `Statement.ServerAuditDefinition` | Server | C | gap 118 | 118: no target record: Statement.CreateServerAudit (118) | 118 |
| `Statement.Delete` | Dml | A | converted 23, gap 45 | 89: no target record: Statement.Delete has no OutputClause in the new tree (proposal 2) (48); needs decision: inventory Statement.Delete (no target) (25) | 114 |
| `Expression.Subquery` | Values | A | — | — | 113 |
| `Statement.Grant` | Security | B | gap 108 | 108: needs decision: inventory Statement.Grant (108) | 108 |
| `Statement.ExternalFileFormatDefinition` | Server | C | gap 101 | 101: no target record: Statement.CreateExternalFileFormat (101) | 101 |
| `Statement.FullTextIndexDefinition` | Tables | C | gap 99 | 99: no target record: Statement.CreateFullTextIndex (99) | 99 |
| `Expression.Member` | Values | A | — | — | 98 |
| `Clause.Output` | Dml | C | — | — | 97 |
| `TableReference.Derived` | Tables | A | — | — | 97 |
| `TableReference.Pivot` | Tables | C | — | — | 93 |
| `Statement.AlterFullTextIndex` | Tables | C | gap 92 | 92: no target record: Statement.AlterFullTextIndex (92) | 92 |
| `Statement.ExternalDataSourceDefinition` | Server | C | gap 92 | 92: no target record: Statement.CreateExternalDataSource (92) | 92 |
| `Statement.SetVariable` | Procedural | C | gap 78 | 89: no target record: Statement.SetVariable (89) | 89 |
| `Clause.DatabaseFile` | Database | C | — | — | 88 |
| `Statement.ResourcePoolDefinition` | Server | C | gap 86 | 86: no target record: Statement.CreateResourcePool (86) | 86 |
| `Clause.Top` | Queries | C | — | — | 84 |
| `Query.TableValueConstructor` | Queries | A | — | — | 84 |
| `Statement.ViewDefinition` | Tables | B | gap 84 | 84: needs decision: inventory Statement.ViewDefinition (84) | 84 |
| `Query.FromFile` | Dml | C | gap 83 | 83: no target record: Statement.BulkInsert (inventory Query.FromFile) (83) | 83 |
| `Clause.Dropped` | Tables | B | — | — | 82 |
| `Clause.For` | Queries | C | — | — | 82 |
| `Statement.ExternalResourcePoolDefinition` | Server | C | gap 79 | 79: no target record: Statement.CreateExternalResourcePool (79) | 79 |
| `Statement.Print` | Procedural | C | gap 38 | 70: no target record: Statement.Print (70) | 75 |
| `Statement.Dbcc` | Server | C | gap 73 | 73: no target record: Statement.Dbcc (73) | 73 |
| `Expression.Not` | Values | A | — | — | 71 |
| `Statement.CreateTableAsSelect` | Tables | B | gap 64 | 70: needs decision: inventory Statement.CreateTableAsSelect (70) | 70 |
| `Statement.AlterServerConfiguration` | Server | C | gap 68 | 68: no target record: Statement.AlterServerConfiguration (68) | 68 |
| `Clause.SystemTime` | Tables | A | — | — | 67 |
| `Clause.MergeWhen` | Dml | B | gap 9 | 9: no target record: MergeClause.NotMatchedBySource and the BY TARGET flag (proposal 3) (9) | 65 |
| `Query.Parenthesized` | Queries | B | — | — | 64 |
| `Statement.CreateTrigger` | Routines | B | gap 62 | 62: needs decision: inventory Statement.CreateTrigger (62) | 62 |
| `Statement.ExternalLibraryDefinition` | Server | C | gap 59 | 59: no target record: Statement.CreateExternalLibrary (59) | 59 |
| `Clause.CommonTableExpression` | Queries | A | — | — | 57 |
| `Statement.CreateLogin` | Security | C | gap 57 | 57: no target record: Statement.CreateLogin (57) | 57 |
| `Statement.UpdateText` | Dml | C | gap 57 | 57: no target record: Statement.UpdateText (57) | 57 |
| `Expression.OdbcEscape` | Values | C | — | — | 56 |
| `Statement.DatabaseAuditSpecificationDefinition` | Database | C | gap 56 | 56: no target record: Statement.CreateDatabaseAuditSpecification (56) | 56 |
| `Expression.Collated` | Values | A | — | — | 55 |
| `Statement.DropIndex` | Drop | C | gap 47 | 49: target exists, no writer arm: Statement.DropIndex (49) | 53 |
| `Statement.Merge` | Dml | A | converted 11, gap 29 | 33: no target record: Statement.Merge has no member for whether INTO was written (proposal 2) (17); no target record: Statement.Merge has no OutputClause in the new tree (proposal 2) (12) | 53 |
| `Expression.IsNull` | Values | A | — | — | 52 |
| `Statement.AvailabilityGroupDefinition` | Server | C | gap 51 | 51: no target record: Statement.CreateAvailabilityGroup (51) | 51 |
| `Statement.TriggerSwitch` | Routines | C | gap 44 | 47: no target record: Statement.EnableTrigger (47) | 51 |
| `Statement.QueueDefinition` | Server | C | gap 48 | 48: no target record: Statement.CreateQueue (48) | 48 |
| `Statement.If` | Procedural | C | gap 23 | 37: no target record: Statement.If (37) | 47 |
| `Statement.SequenceDefinition` | Routines | A | converted 29, gap 18 | 18: no target record: SequenceOption has no CACHE (inventory Statement.SequenceDefinition) (17); needs decision: inventory cross-cutting gaps, identifier styles (a name part ` TestSeq`) (1) | 47 |
| `Statement.AlterFullTextStopList` | Database | C | gap 46 | 46: no target record: Statement.AlterFullTextStopList (46) | 46 |
| `Statement.RestoreDatabase` | Database | C | gap 46 | 46: no target record: Statement.RestoreDatabase (46) | 46 |
| `Statement.AuditSpecificationDefinition` | Server | C | gap 45 | 45: no target record: Statement.CreateServerAuditSpecification (45) | 45 |
| `Statement.AssemblyDefinition` | Routines | C | gap 44 | 44: no target record: Statement.CreateAssembly (44) | 44 |
| `Statement.AsymmetricKeyDefinition` | Security | C | gap 43 | 43: no target record: Statement.CreateAsymmetricKey (43) | 43 |
| `Statement.StatisticsDefinition` | Tables | C | gap 43 | 43: no target record: Statement.CreateStatistics (43) | 43 |
| `Statement.UpdateStatistics` | Tables | C | gap 41 | 43: no target record: Statement.UpdateStatistics (43) | 43 |
| `Clause.Into` | Queries | C | — | — | 41 |
| `Expression.Multiply` | Values | A | — | — | 41 |
| `Statement.EndConversation` | Server | C | gap 27 | 34: no target record: Statement.EndConversation (34) | 41 |
| `Statement.ExternalLanguageDefinition` | Server | C | gap 41 | 41: no target record: Statement.CreateExternalLanguage (41) | 41 |
| `Expression.Or` | Values | A | — | — | 40 |
| `Clause.WindowDefinition` | Values | A | — | — | 39 |
| `Query.Ordered` | Queries | B | — | — | 39 |
| `Expression.JsonPair` | Values | A | — | 31: needs decision: inventory Expression.JsonPair (31) | 38 |
| `Expression.IsDistinctFrom` | Values | A | — | — | 37 |
| `Statement.BackupDatabase` | Database | C | gap 37 | 37: no target record: Statement.BackupDatabase (37) | 37 |
| `Statement.CreateUser` | Security | C | gap 37 | 37: no target record: Statement.CreateUser (37) | 37 |
| `Statement.SecurityPolicyDefinition` | Security | C | gap 37 | 37: no target record: Statement.CreateSecurityPolicy (37) | 37 |
| `Statement.Label` | Procedural | C | gap 30 | 36: no target record: Statement.Label (36) | 36 |
| `Statement.Revoke` | Security | B | gap 36 | 36: needs decision: inventory Statement.Revoke (36) | 36 |
| `SetExpression.Miscellaneous.Offset` | Procedural | C | — | — | 35 |
| `Statement.WriteText` | Dml | C | gap 35 | 35: no target record: Statement.WriteText (35) | 35 |
| `Query.Union` | Queries | B | — | — | 34 |
| `Statement.Deny` | Security | C | gap 34 | 34: bridge arm missing: Statement.Deny (34) | 34 |
| `Statement.ExternalModelDefinition` | Server | C | gap 34 | 34: no target record: Statement.CreateExternalModel (34) | 34 |
| `Statement.Fetch` | Procedural | A | converted 34 | — | 34 |
| `Clause.CursorDefinition` | Procedural | B | — | — | 32 |
| `Clause.TableSample` | Tables | A | — | — | 31 |
| `Clause.When` | Values | A | — | — | 30 |
| `Expression.Case` | Values | A | — | — | 30 |
| `Expression.ShiftLeft` | Values | C | — | — | 30 |
| `Statement.AlterDatabaseModify` | Database | C | gap 30 | 30: target exists, no writer arm: AlterDatabaseAction.Modify (30) | 30 |
| `Statement.AlterDatabaseModifyFileGroup` | Database | C | gap 30 | 30: target exists, no writer arm: AlterDatabaseAction.ModifyFileGroup (30) | 30 |
| `Statement.Send` | Server | C | gap 30 | 30: no target record: Statement.Send (30) | 30 |
| `Expression.Aliased` | Values | A | — | 29: needs decision: inventory Expression.Aliased (29) | 29 |
| `Expression.In` | Values | A | — | — | 29 |
| `Statement.AlterResourceGovernor` | Server | C | gap 29 | 29: no target record: Statement.AlterResourceGovernor (29) | 29 |
| `Statement.SynonymDefinition` | Routines | C | gap 29 | 29: no target record: Statement.CreateSynonym (29) | 29 |
| `TableReference.Changed` | Tables | A | gap 25 | 29: needs decision: inventory TableReference.Changed (29) | 29 |
| `Query.DefaultValues` | Dml | A | — | 26: needs decision: inventory Query.DefaultValues (outside an INSERT) (26) | 28 |
| `Statement.Kill` | Server | C | gap 28 | 28: no target record: Statement.Kill (28) | 28 |
| `SetExpression.QueryExecution.Switch` | Procedural | C | — | — | 27 |
| `Statement.SchemaDefinition` | Security | A | converted 12, gap 15 | 15: needs decision: inventory Statement.SchemaDefinition (elements kept as text) (15) | 27 |
| `Statement.SymmetricKeyDefinition` | Security | C | gap 27 | 27: no target record: Statement.CreateSymmetricKey (27) | 27 |
| `Clause.VariableAssignment` | Queries | C | — | — | 26 |
| `Statement.DeclareCursor` | Procedural | B | gap 26 | 26: needs decision: inventory Statement.DeclareCursor (26) | 26 |
| `Expression.Like` | Values | A | — | — | 25 |
| `SetExpression.IsoSettings.Switch` | Procedural | C | — | — | 25 |
| `Statement.CursorAction` | Procedural | A | converted 19, gap 6 | 6: target exists, no writer arm: Statement.DeallocateCursor (6) | 25 |
| `Statement.Truncate` | Dml | A | converted 13, gap 12 | 12: no target record: Statement.TruncateTable has no partitions (PartitionRange is declared, no member holds it) (12) | 25 |
| `SetExpression.Statistics.Report` | Procedural | C | — | — | 24 |
| `Statement.DropProcedure` | Drop | B | converted 24 | — | 24 |
| `Statement.ReadText` | Dml | C | gap 24 | 24: no target record: Statement.ReadText (24) | 24 |
| `SetExpression.Transactions.IsolationLevel` | Procedural | A | — | — | 23 |
| `Statement.BeginDialog` | Server | C | gap 23 | 23: no target record: Statement.BeginDialog (23) | 23 |
| `Statement.BrokerPriorityDefinition` | Server | C | gap 23 | 23: no target record: Statement.CreateBrokerPriority (23) | 23 |
| `Statement.DropTrigger` | Drop | B | converted 19 | — | 23 |
| `SetExpression.Miscellaneous.Switch` | Procedural | C | — | — | 22 |
| `Statement.Break` | Procedural | C | gap 3 | 22: no target record: Statement.Break (22) | 22 |
| `Statement.OpenSymmetricKey` | Security | C | gap 22 | 22: no target record: Statement.OpenSymmetricKey (22) | 22 |
| `Statement.AlterAsymmetricKey` | Security | C | gap 21 | 21: no target record: Statement.AlterAsymmetricKey (21) | 21 |
| `Statement.PartitionFunctionDefinition` | Database | C | gap 21 | 21: no target record: Statement.CreatePartitionFunction (21) | 21 |
| `TableReference.Unpivot` | Tables | C | — | — | 21 |
| `Statement.CredentialDefinition` | Security | C | gap 20 | 20: no target record: Statement.CreateCredential (20) | 20 |
| `Statement.ExecuteAs` | Procedural | C | gap 20 | 20: no target record: Statement.ExecuteAs (20) | 20 |
| `Statement.FullTextCatalogDefinition` | Database | C | gap 20 | 20: no target record: Statement.CreateFullTextCatalog (20) | 20 |
| `Statement.RuleDefinition` | Routines | C | gap 20 | 20: no target record: Statement.CreateRule (20) | 20 |
| `Statement.WaitFor` | Procedural | C | gap 20 | 20: no target record: Statement.WaitFor (20) | 20 |
| `Expression.BitwiseNot` | Values | C | — | — | 19 |
| `Statement.CertificateDefinition` | Security | C | gap 19 | 19: no target record: Statement.CreateCertificate (19) | 19 |
| `Statement.DropDatabase` | Drop | C | gap 19 | 19: no target record: Statement.DropDatabase (one record per kind, not declared yet) (19) | 19 |
| `Statement.FullTextStopListDefinition` | Database | C | gap 19 | 19: no target record: Statement.CreateFullTextStopList (19) | 19 |
| `Statement.TryCatch` | Procedural | C | gap 16 | 18: no target record: Statement.TryCatch (18) | 19 |
| `Statement.AggregateDefinition` | Routines | C | gap 18 | 18: no target record: Statement.CreateAggregate (18) | 18 |
| `Statement.AlterLogin` | Security | C | gap 18 | 18: no target record: Statement.AlterLogin (18) | 18 |
| `Statement.DropAssembly` | Drop | C | gap 18 | 18: target exists, no writer arm: Statement.DropAssembly (18) | 18 |
| `Statement.MessageTypeDefinition` | Server | C | gap 18 | 18: no target record: Statement.CreateMessageType (18) | 18 |
| `Statement.RaiseError` | Procedural | C | gap 10 | 18: no target record: Statement.RaiseError (18) | 18 |
| `Statement.ServiceDefinition` | Server | C | gap 18 | 18: no target record: Statement.CreateService (18) | 18 |
| `Expression.Plus` | Values | A | — | — | 17 |
| `Statement.AlterPartitionFunction` | Database | C | gap 17 | 17: no target record: Statement.AlterPartitionFunction (17) | 17 |
| `Statement.BackupCertificate` | Database | C | gap 17 | 17: no target record: Statement.BackupCertificate (17) | 17 |
| `Statement.Classification` | Security | C | gap 17 | 17: no target record: Statement.AddSensitivityClassification (17) | 17 |
| `Statement.DropTable` | Drop | B | converted 16 | — | 17 |
| `Statement.EventNotificationDefinition` | Server | C | gap 17 | 17: no target record: Statement.CreateEventNotification (17) | 17 |
| `Statement.Receive` | Server | C | gap 13 | 17: no target record: Statement.Receive (17) | 17 |
| `Statement.RemoteServiceBindingDefinition` | Server | C | gap 17 | 17: no target record: Statement.CreateRemoteServiceBinding (17) | 17 |
| `Statement.AddSignature` | Security | C | gap 16 | 16: no target record: Statement.AddSignature (16) | 16 |
| `Statement.AlterMasterKey` | Security | C | gap 16 | 16: no target record: Statement.AlterMasterKey (16) | 16 |
| `Statement.DefaultDefinition` | Routines | C | gap 16 | 16: no target record: Statement.CreateDefault (16) | 16 |
| `Statement.DropFunction` | Drop | B | converted 16 | — | 16 |
| `Clause.PartitionRange` | Dml | C | — | — | 15 |
| `Statement.BackupTransactionLog` | Database | C | gap 15 | 15: no target record: Statement.BackupLog (15) | 15 |
| `Statement.DropAggregate` | Drop | C | gap 15 | 15: no target record: Statement.DropAggregate (one record per kind, not declared yet) (15) | 15 |
| `Statement.DropView` | Drop | B | converted 15 | — | 15 |
| `Clause.FileGroup` | Database | C | — | — | 14 |
| `Expression.Concatenate` | Values | A | — | — | 14 |
| `Query.FromStream` | Dml | C | gap 14 | 14: no target record: Statement.InsertBulk (inventory Query.FromStream) (14) | 14 |
| `Statement.AlterCertificate` | Security | C | gap 14 | 14: no target record: Statement.AlterCertificate (14) | 14 |
| `Statement.DropSensitivityClassification` | Drop | C | gap 14 | 14: target exists, no writer arm: Statement.DropSensitivityClassification (14) | 14 |
| `Statement.DropSequence` | Drop | B | converted 14 | — | 14 |
| `Statement.DropType` | Drop | B | converted 14 | — | 14 |
| `Statement.RestoreLog` | Database | C | gap 14 | 14: no target record: Statement.RestoreLog (14) | 14 |
| `Expression.ShiftRight` | Values | C | — | — | 13 |
| `Statement.AlterDatabaseScopedConfiguration` | Database | C | gap 13 | 13: no target record: Statement.AlterDatabaseScopedConfiguration (13) | 13 |
| `Statement.AlterFullTextCatalog` | Database | C | gap 13 | 13: no target record: Statement.AlterFullTextCatalog (13) | 13 |
| `Statement.AlterServiceMasterKey` | Security | C | gap 13 | 13: no target record: Statement.AlterServiceMasterKey (13) | 13 |
| `Statement.AlterUser` | Security | C | gap 13 | 13: no target record: Statement.AlterUser (13) | 13 |
| `Statement.CryptographicProviderDefinition` | Security | C | gap 13 | 13: no target record: Statement.CreateCryptographicProvider (13) | 13 |
| `Statement.DropDefault` | Drop | C | gap 13 | 13: no target record: Statement.DropDefault (one record per kind, not declared yet) (13) | 13 |
| `Statement.DropRule` | Drop | C | gap 13 | 13: no target record: Statement.DropRule (one record per kind, not declared yet) (13) | 13 |
| `Statement.DropSynonym` | Drop | C | gap 13 | 13: no target record: Statement.DropSynonym (one record per kind, not declared yet) (13) | 13 |
| `Statement.SetUser` | Procedural | C | gap 13 | 13: no target record: Statement.SetUser (13) | 13 |
| `Clause.ExecutionContext` | Routines | C | — | — | 12 |
| `Expression.Divide` | Values | A | — | — | 12 |
| `Expression.RowsetOrder` | Values | C | — | 12: no target record: a typed T-SQL rowset table source (proposal 7) (12) | 12 |
| `Statement.DropSignature` | Drop | C | gap 12 | 12: target exists, no writer arm: Statement.DropSignature (12) | 12 |
| `Statement.DropStatistics` | Drop | C | gap 12 | 12: no target record: Statement.DropStatistics (one record per kind, not declared yet) (12) | 12 |
| `Expression.Exists` | Values | A | — | — | 11 |
| `Statement.AlterAuthorization` | Security | C | gap 11 | 11: no target record: Statement.AlterAuthorization (11) | 11 |
| `Statement.AlterDatabaseRebuildLog` | Database | C | gap 11 | 11: target exists, no writer arm: AlterDatabaseAction.RebuildLog (11) | 11 |
| `Statement.AlterSchema` | Security | C | gap 11 | 11: no target record: Statement.AlterSchema (11) | 11 |
| `Statement.AlterSearchPropertyList` | Database | C | gap 11 | 11: no target record: Statement.AlterSearchPropertyList (11) | 11 |
| `Statement.DropEventNotification` | Drop | C | gap 11 | 11: target exists, no writer arm: Statement.DropEventNotification (11) | 11 |
| `Statement.DropQueue` | Drop | C | gap 11 | 11: no target record: Statement.DropQueue (one record per kind, not declared yet) (11) | 11 |
| `Statement.DropSecurityPolicy` | Drop | C | gap 11 | 11: no target record: Statement.DropSecurityPolicy (one record per kind, not declared yet) (11) | 11 |
| `Statement.PartitionSchemeDefinition` | Database | C | gap 11 | 11: no target record: Statement.CreatePartitionScheme (11) | 11 |
| `Statement.Revert` | Procedural | C | gap 11 | 11: no target record: Statement.Revert (11) | 11 |
| `Statement.XmlSchemaCollectionDefinition` | Routines | C | gap 11 | 11: no target record: Statement.CreateXmlSchemaCollection (11) | 11 |
| `Expression.Between` | Values | A | — | — | 10 |
| `Statement.AlterPartitionScheme` | Database | C | gap 10 | 10: no target record: Statement.AlterPartitionScheme (10) | 10 |
| `Statement.DatabaseScopedCredentialDefinition` | Security | C | gap 10 | 10: no target record: Statement.CreateDatabaseScopedCredential (10) | 10 |
| `Statement.DropEventSession` | Drop | C | gap 10 | 10: target exists, no writer arm: Statement.DropEventSession (10) | 10 |
| `Statement.GetConversationGroup` | Server | C | gap 4 | 10: no target record: Statement.GetConversationGroup (10) | 10 |
| `Statement.WaitForStatement` | Procedural | C | gap 10 | 10: no target record: Statement.WaitForReceive (10) | 10 |
| `Clause.CursorFor` | Procedural | A | — | — | 9 |
| `Expression.Subtract` | Values | A | — | — | 9 |
| `Statement.AlterDatabaseCollate` | Database | C | gap 9 | 9: target exists, no writer arm: AlterDatabaseAction.Collate (9) | 9 |
| `Statement.DropXmlSchemaCollection` | Drop | C | gap 9 | 9: no target record: Statement.DropXmlSchemaCollection (one record per kind, not declared yet) (9) | 9 |
| `Statement.MasterKeyDefinition` | Security | C | gap 9 | 9: no target record: Statement.CreateMasterKey (9) | 9 |
| `Statement.Reconfigure` | Server | C | gap 9 | 9: no target record: Statement.Reconfigure (9) | 9 |
| `TableReference.OdbcJoin` | Tables | C | — | — | 9 |
| `SetExpression.DateAndTime.DateFirst` | Procedural | C | — | — | 8 |
| `SetExpression.QueryExecution.TextSize` | Procedural | C | — | — | 8 |
| `SetExpression.Statistics.Switch` | Procedural | C | — | — | 8 |
| `Statement.AlterDatabaseAddFileGroup` | Database | C | gap 8 | 8: target exists, no writer arm: AlterDatabaseAction.AddFileGroup (8) | 8 |
| `Statement.AlterDatabaseAddLogFile` | Database | C | gap 8 | 8: target exists, no writer arm: AlterDatabaseAction.AddLogFile (8) | 8 |
| `Statement.AlterRole` | Security | C | gap 8 | 8: no target record: Statement.AlterRole (8) | 8 |
| `Statement.AlterXmlSchemaCollection` | Routines | C | gap 8 | 8: no target record: Statement.AlterXmlSchemaCollection (8) | 8 |
| `Statement.ColumnMasterKeyDefinition` | Security | C | gap 8 | 8: no target record: Statement.CreateColumnMasterKey (8) | 8 |
| `Statement.DatabaseEncryptionKeyDefinition` | Security | C | gap 8 | 8: no target record: Statement.CreateDatabaseEncryptionKey (8) | 8 |
| `Statement.DropExternalLanguage` | Drop | C | gap 8 | 8: no target record: Statement.DropExternalLanguage (one record per kind, not declared yet) (8) | 8 |
| `Statement.DropExternalLibrary` | Drop | C | gap 8 | 8: target exists, no writer arm: Statement.DropExternalLibrary (8) | 8 |
| `Statement.DropRole` | Drop | B | converted 8 | — | 8 |
| `Statement.DropUser` | Drop | C | gap 6 | 8: no target record: Statement.DropUser (one record per kind, not declared yet) (8) | 8 |
| `Statement.GoTo` | Procedural | C | gap 3 | 6: no target record: Statement.GoTo (6) | 8 |
| `SetExpression.Miscellaneous.IdentityInsert` | Procedural | C | — | — | 7 |
| `Statement.AlterDatabaseAddFile` | Database | C | gap 7 | 7: target exists, no writer arm: AlterDatabaseAction.AddFile (7) | 7 |
| `Statement.AlterServerRole` | Security | C | gap 7 | 7: no target record: Statement.AlterServerRole (7) | 7 |
| `Statement.Checkpoint` | Database | C | gap 7 | 7: no target record: Statement.Checkpoint (7) | 7 |
| `Statement.Continue` | Procedural | C | gap 2 | 7: no target record: Statement.Continue (7) | 7 |
| `Statement.DropAsymmetricKey` | Drop | C | gap 7 | 7: no target record: Statement.DropAsymmetricKey (one record per kind, not declared yet) (7) | 7 |
| `Statement.DropContract` | Drop | C | gap 7 | 7: no target record: Statement.DropContract (one record per kind, not declared yet) (7) | 7 |
| `Statement.DropMessageType` | Drop | C | gap 7 | 7: no target record: Statement.DropMessageType (one record per kind, not declared yet) (7) | 7 |
| `Statement.DropRemoteServiceBinding` | Drop | C | gap 7 | 7: no target record: Statement.DropRemoteServiceBinding (one record per kind, not declared yet) (7) | 7 |
| `Statement.DropRoute` | Drop | C | gap 7 | 7: no target record: Statement.DropRoute (one record per kind, not declared yet) (7) | 7 |
| `Statement.DropService` | Drop | C | gap 7 | 7: no target record: Statement.DropService (one record per kind, not declared yet) (7) | 7 |
| `Statement.DropSymmetricKey` | Drop | C | gap 7 | 7: no target record: Statement.DropSymmetricKey (one record per kind, not declared yet) (7) | 7 |
| `Statement.RestoreMasterKey` | Database | C | gap 7 | 7: no target record: Statement.RestoreMasterKey (7) | 7 |
| `Statement.RouteDefinition` | Server | C | gap 7 | 7: no target record: Statement.CreateRoute (7) | 7 |
| `Statement.SearchPropertyListDefinition` | Database | C | gap 7 | 7: no target record: Statement.CreateSearchPropertyList (7) | 7 |
| `Statement.Throw` | Procedural | C | gap 5 | 7: no target record: Statement.Throw (7) | 7 |
| `Statement.Use` | Procedural | C | gap 7 | 7: no target record: Statement.Use (7) | 7 |
| `Query.Except` | Queries | B | — | — | 6 |
| `SetExpression.QueryExecution.RowCount` | Procedural | C | — | — | 6 |
| `Statement.AlterSymmetricKey` | Security | C | gap 6 | 6: no target record: Statement.AlterSymmetricKey (6) | 6 |
| `Statement.ColumnEncryptionKeyDefinition` | Security | C | gap 6 | 6: no target record: Statement.CreateColumnEncryptionKey (6) | 6 |
| `Statement.ContractDefinition` | Server | C | gap 6 | 6: no target record: Statement.CreateContract (6) | 6 |
| `Statement.CreateApplicationRole` | Security | C | gap 6 | 6: no target record: Statement.CreateApplicationRole (6) | 6 |
| `Statement.CreateRole` | Security | A | converted 3, gap 3 | 3: no target record: Statement.CreateRole has no AUTHORIZATION owner (inventory Statement.CreateRole) (3) | 6 |
| `Statement.DropDatabaseAuditSpecification` | Drop | C | gap 6 | 6: no target record: Statement.DropDatabaseAuditSpecification (one record per kind, not declared yet) (6) | 6 |
| `Statement.DropFulltextIndex` | Drop | C | gap 6 | 6: target exists, no writer arm: Statement.DropFulltextIndex (6) | 6 |
| `Statement.DropSchema` | Drop | B | converted 6 | — | 6 |
| `Statement.DropServerAudit` | Drop | C | gap 6 | 6: no target record: Statement.DropServerAudit (one record per kind, not declared yet) (6) | 6 |
| `Statement.DropServerAuditSpecification` | Drop | C | gap 6 | 6: no target record: Statement.DropServerAuditSpecification (one record per kind, not declared yet) (6) | 6 |
| `Statement.DropWorkloadClassifier` | Drop | C | gap 6 | 6: no target record: Statement.DropWorkloadClassifier (one record per kind, not declared yet) (6) | 6 |
| `Statement.RestoreServiceMasterKey` | Database | C | gap 6 | 6: no target record: Statement.RestoreServiceMasterKey (6) | 6 |
| `Statement.SetCursor` | Procedural | C | gap 6 | 6: target exists, no writer arm: Statement.SetCursorVariable (6) | 6 |
| `Statement.Shutdown` | Server | C | gap 6 | 6: no target record: Statement.Shutdown (6) | 6 |
| `Statement.While` | Procedural | C | gap 6 | 6: no target record: Statement.While (6) | 6 |
| `Query.FromExecute` | Dml | C | gap 5 | 5: no target record: InsertSource.Execute (inventory Query.FromExecute) (5) | 5 |
| `SetExpression.Transactions.Switch` | Procedural | C | — | — | 5 |
| `Statement.AlterColumnEncryptionKey` | Security | C | gap 5 | 5: no target record: Statement.AlterColumnEncryptionKey (5) | 5 |
| `Statement.AlterDatabaseModifyFile` | Database | C | gap 5 | 5: target exists, no writer arm: AlterDatabaseAction.ModifyFile (5) | 5 |
| `Statement.CloseSymmetricKey` | Security | C | gap 5 | 5: no target record: Statement.CloseSymmetricKey (5) | 5 |
| `Statement.ConversationTimer` | Server | C | gap 5 | 5: no target record: Statement.BeginConversationTimer (5) | 5 |
| `Statement.CreateServerRole` | Security | C | gap 5 | 5: no target record: Statement.CreateServerRole (5) | 5 |
| `Statement.DropExternalTable` | Drop | C | gap 5 | 5: no target record: Statement.DropExternalTable (one record per kind, not declared yet) (5) | 5 |
| `Statement.MoveConversation` | Server | C | gap 5 | 5: no target record: Statement.MoveConversation (5) | 5 |
| `Expression.Modulo` | Values | C | — | — | 4 |
| `Query.Intersect` | Queries | B | — | — | 4 |
| `SetExpression.Miscellaneous.FipsFlagger` | Procedural | C | — | — | 4 |
| `SetExpression.QueryExecution.QueryGovernorCostLimit` | Procedural | C | — | — | 4 |
| `Statement.AlterApplicationRole` | Security | C | gap 4 | 4: no target record: Statement.AlterApplicationRole (4) | 4 |
| `Statement.AlterDatabaseEncryptionKey` | Security | C | gap 4 | 4: no target record: Statement.AlterDatabaseEncryptionKey (4) | 4 |
| `Statement.AlterDatabaseModifyName` | Database | C | gap 4 | 4: target exists, no writer arm: AlterDatabaseAction.ModifyName (4) | 4 |
| `Statement.BackupMasterKey` | Database | C | gap 4 | 4: no target record: Statement.BackupMasterKey (4) | 4 |
| `Statement.DropAvailabilityGroup` | Drop | C | gap 4 | 4: no target record: Statement.DropAvailabilityGroup (one record per kind, not declared yet) (4) | 4 |
| `Statement.DropCryptographicProvider` | Drop | C | gap 4 | 4: no target record: Statement.DropCryptographicProvider (one record per kind, not declared yet) (4) | 4 |
| `Statement.DropDatabaseScopedCredential` | Drop | C | gap 4 | 4: no target record: Statement.DropDatabaseScopedCredential (one record per kind, not declared yet) (4) | 4 |
| `Statement.DropExternalModel` | Drop | C | gap 4 | 4: no target record: Statement.DropExternalModel (one record per kind, not declared yet) (4) | 4 |
| `Statement.DropSearchPropertyList` | Drop | C | gap 4 | 4: no target record: Statement.DropSearchPropertyList (one record per kind, not declared yet) (4) | 4 |
| `Statement.DropServerRole` | Drop | C | gap 4 | 4: no target record: Statement.DropServerRole (one record per kind, not declared yet) (4) | 4 |
| `Statement.OpenMasterKey` | Security | C | gap 4 | 4: no target record: Statement.OpenMasterKey (4) | 4 |
| `Statement.RestoreRewindOnly` | Database | C | gap 4 | 4: no target record: Statement.RestoreRewindOnly (4) | 4 |
| `Expression.BitwiseAnd` | Values | C | — | — | 3 |
| `SetExpression.DateAndTime.DateFormat` | Procedural | C | — | — | 3 |
| `SetExpression.Locking.DeadlockPriority` | Procedural | C | — | — | 3 |
| `SetExpression.Miscellaneous.ErrorLevel` | Procedural | C | — | — | 3 |
| `SetExpression.Miscellaneous.Language` | Procedural | C | — | — | 3 |
| `Statement.AlterDatabaseRemoveFile` | Database | C | gap 3 | 3: target exists, no writer arm: AlterDatabaseAction.RemoveFile (3) | 3 |
| `Statement.BackupServiceMasterKey` | Database | C | gap 3 | 3: no target record: Statement.BackupServiceMasterKey (3) | 3 |
| `Statement.CloseAllSymmetricKeys` | Security | C | gap 3 | 3: no target record: Statement.CloseAllSymmetricKeys (3) | 3 |
| `Statement.CloseMasterKey` | Security | C | gap 3 | 3: no target record: Statement.CloseMasterKey (3) | 3 |
| `Statement.DropApplicationRole` | Drop | C | gap 3 | 3: no target record: Statement.DropApplicationRole (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropBrokerPriority` | Drop | C | gap 3 | 3: no target record: Statement.DropBrokerPriority (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropCertificate` | Drop | C | gap 3 | 3: no target record: Statement.DropCertificate (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropColumnEncryptionKey` | Drop | C | gap 3 | 3: no target record: Statement.DropColumnEncryptionKey (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropColumnMasterKey` | Drop | C | gap 3 | 3: no target record: Statement.DropColumnMasterKey (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropCredential` | Drop | C | gap 3 | 3: no target record: Statement.DropCredential (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropDatabaseEncryptionKey` | Drop | C | gap 3 | 3: target exists, no writer arm: Statement.DropDatabaseEncryptionKey (3) | 3 |
| `Statement.DropEndpoint` | Drop | C | gap 3 | 3: no target record: Statement.DropEndpoint (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropFulltextCatalog` | Drop | C | gap 3 | 3: no target record: Statement.DropFulltextCatalog (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropFulltextStoplist` | Drop | C | gap 3 | 3: no target record: Statement.DropFulltextStoplist (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropLogin` | Drop | C | gap 3 | 3: no target record: Statement.DropLogin (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropMasterKey` | Drop | C | gap 3 | 3: target exists, no writer arm: Statement.DropMasterKey (3) | 3 |
| `Statement.DropPartitionFunction` | Drop | C | gap 3 | 3: no target record: Statement.DropPartitionFunction (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropPartitionScheme` | Drop | C | gap 3 | 3: no target record: Statement.DropPartitionScheme (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropResourcePool` | Drop | C | gap 3 | 3: no target record: Statement.DropResourcePool (one record per kind, not declared yet) (3) | 3 |
| `Statement.DropWorkloadGroup` | Drop | C | gap 3 | 3: no target record: Statement.DropWorkloadGroup (one record per kind, not declared yet) (3) | 3 |
| `Statement.LineNumber` | Procedural | C | gap 3 | 3: no target record: Statement.LineNumber (3) | 3 |
| `Statement.RestoreFileListOnly` | Database | C | gap 3 | 3: no target record: Statement.RestoreFileListOnly (3) | 3 |
| `Statement.RestoreHeaderOnly` | Database | C | gap 3 | 3: no target record: Statement.RestoreHeaderOnly (3) | 3 |
| `Statement.RestoreVerifyOnly` | Database | C | gap 3 | 3: no target record: Statement.RestoreVerifyOnly (3) | 3 |
| `Expression.BitwiseOr` | Values | C | — | — | 2 |
| `Expression.BitwiseXor` | Values | C | — | — | 2 |
| `SetExpression.Locking.LockTimeout` | Procedural | C | — | — | 2 |
| `SetExpression.Miscellaneous.ContextInfo` | Procedural | C | — | — | 2 |
| `Statement.AlterDatabaseModifyBackupStorageRedundancy` | Database | C | gap 2 | 2: target exists, no writer arm: AlterDatabaseAction.ModifyBackupStorageRedundancy (2) | 2 |
| `Statement.AlterDatabasePerformCutover` | Database | C | gap 2 | 2: target exists, no writer arm: AlterDatabaseAction.PerformCutover (2) | 2 |
| `Statement.AlterDatabaseRemoveFileGroup` | Database | C | gap 2 | 2: target exists, no writer arm: AlterDatabaseAction.RemoveFileGroup (2) | 2 |
| `Statement.DropExternalDataSource` | Drop | C | gap 2 | 2: no target record: Statement.DropExternalDataSource (one record per kind, not declared yet) (2) | 2 |
| `Statement.DropExternalFileFormat` | Drop | C | gap 2 | 2: no target record: Statement.DropExternalFileFormat (one record per kind, not declared yet) (2) | 2 |
| `Statement.DropExternalResourcePool` | Drop | C | gap 2 | 2: no target record: Statement.DropExternalResourcePool (one record per kind, not declared yet) (2) | 2 |
| `Statement.RestoreLabelOnly` | Database | C | gap 2 | 2: no target record: Statement.RestoreLabelOnly (2) | 2 |
| `Statement.RestoreSymmetricKey` | Database | C | gap 2 | 2: no target record: Statement.RestoreSymmetricKey (2) | 2 |
| `Expression.Pieced` | Values | C | — | 1: no target record: a typed T-SQL rowset table source (proposal 7) (1) | 1 |
| `Expression.Quantified` | Values | A | — | — | 1 |
| `Query.ExplicitTable` | Queries | A | — | — | 1 |
| `Statement.BackupGroup` | Database | C | gap 1 | 1: no target record: Statement.BackupGroup (1) | 1 |
| `Statement.BackupSymmetricKey` | Database | C | gap 1 | 1: no target record: Statement.BackupSymmetricKey (1) | 1 |
| `Expression.IsTruth` | Values | A | — | — | 0 |
| `Expression.Match` | Values | A | — | — | 0 |
| `Expression.Overlaps` | Values | A | — | — | 0 |
| `Expression.Unique` | Values | A | — | — | 0 |
| `Statement.BackupServer` | Database | C | — | — | 0 |
