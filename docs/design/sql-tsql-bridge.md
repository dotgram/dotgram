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

Every place the two trees differ is then collected. A difference inside one of thirteen subtrees, found by its
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
- `EXECUTE p`: a procedure call in T-SQL, a prepared statement in the standard. The whole statement, where it is
  an `ExecuteModule` with the keyword `EXECUTE`, a one-part name and nothing else, and the standard's `Execute`
  of the same name with nothing else.

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
- `Statement.ExecuteAs.Exec`: `EXEC AS` against `EXECUTE AS`.
- `RoutineDefinition.Unbracketed`: a procedure's parameters without their brackets, `CREATE PROCEDURE p @a INT AS …`.
- `RoutineDefinition.AsOmitted`: a function's body or inline RETURN with no AS before it.

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

## Known writer defects

None. The six this bridge found — FETCH without INTO, DROP … IF EXISTS, DROP PROCEDURE x, y, DROP TRIGGER … ON, a bare RETURN, and `- -1` written as a comment — were fixed on main in c044587.

## The counters

| Counter | Count |
| --- | ---: |
| ByCase Clause.ColumnDefinition.Type | 1691 |
| ByCase Clause.Dropped.Tail | 5 |
| ByCase Clause.For.Kind | 20 |
| ByCase Clause.For.Options | 44 |
| ByCase Clause.GroupBy.With | 2 |
| ByCase Clause.Hint.Text | 33 |
| ByCase Clause.JsonColumn.Type | 54 |
| ByCase Clause.Option.Name | 9 |
| ByCase Clause.Option.Partitions | 3 |
| ByCase Clause.ParameterDeclaration.Type | 134 |
| ByCase Clause.SystemTime.Kind | 0 |
| ByCase Clause.TableSample.Unit | 4 |
| ByCase Clause.Top.With | 0 |
| ByCase Clause.VariableDeclaration.Type | 84 |
| ByCase Clause.Window.Frame | 7 |
| ByCase Expression.ColumnReference.Text of a date part | 112 |
| ByCase Expression.ColumnReference.Text under Statement.DropIndex | 13 |
| ByCase Expression.ColumnReference.Text under Statement.DropSignature | 5 |
| ByCase Expression.GraphMatch.Pattern | 114 |
| ByCase Expression.Hinted.Words | 5 |
| ByCase Expression.Literal.Text | 26 |
| ByCase Expression.Match.Qualifier | 0 |
| ByCase Expression.NamedArgument.Name | 0 |
| ByCase Expression.OdbcEscape.Kind | 1 |
| ByCase Expression.Prefixed.Word | 23 |
| ByCase Expression.Quantified.Quantifier | 0 |
| ByCase Expression.RoutineInvocation.Name | 120 |
| ByCase Expression.RoutineInvocation.Word | 117 |
| ByCase Expression.WindowFunction.Within | 16 |
| ByCase Query.Specification.Quantifier | 5 |
| ByCase SetExpression.Statistics.Report.Kind | 2 |
| ByCase SetExpression.Transactions.IsolationLevel.Level | 0 |
| ByCase Statement.AddSignature.To | 3 |
| ByCase Statement.AggregateDefinition.Tail | 13 |
| ByCase Statement.AlterAsymmetricKey.Tail | 6 |
| ByCase Statement.AlterCertificate.Tail | 5 |
| ByCase Statement.AlterColumnEncryptionKey.Tail | 4 |
| ByCase Statement.AlterFullTextIndex.Tail | 29 |
| ByCase Statement.AlterMasterKey.Tail | 6 |
| ByCase Statement.AlterRole.Tail | 3 |
| ByCase Statement.AlterSchema.Tail | 7 |
| ByCase Statement.AlterServerRole.Tail | 3 |
| ByCase Statement.AlterServiceMasterKey.Tail | 2 |
| ByCase Statement.AlterSymmetricKey.Tail | 2 |
| ByCase Statement.AlterTable.Tail | 54 |
| ByCase Statement.AlterUser.Tail | 5 |
| ByCase Statement.AssemblyDefinition.Tail | 3 |
| ByCase Statement.AsymmetricKeyDefinition.Tail | 18 |
| ByCase Statement.CertificateDefinition.Tail | 1 |
| ByCase Statement.ColumnEncryptionKeyDefinition.Tail | 4 |
| ByCase Statement.ColumnMasterKeyDefinition.Tail | 5 |
| ByCase Statement.CreateFunction.Returns | 57 |
| ByCase Statement.CreateIndex.Using | 6 |
| ByCase Statement.CreateLogin.Tail | 16 |
| ByCase Statement.CreateRole.Tail | 1 |
| ByCase Statement.CreateServerRole.Tail | 1 |
| ByCase Statement.CreateUser.Tail | 9 |
| ByCase Statement.CredentialDefinition.Tail | 4 |
| ByCase Statement.CursorAction.Verb | 0 |
| ByCase Statement.DefaultDefinition.Tail | 2 |
| ByCase Statement.Deny.Privileges | 6 |
| ByCase Statement.DropAssembly.Tail | 1 |
| ByCase Statement.DropAsymmetricKey.Tail | 1 |
| ByCase Statement.DropEventNotification.Tail | 2 |
| ByCase Statement.DropEventSession.Tail | 2 |
| ByCase Statement.DropSignature.From | 1 |
| ByCase Statement.DropSymmetricKey.Tail | 1 |
| ByCase Statement.DropTrigger.Tail | 2 |
| ByCase Statement.Execute.Tail | 8 |
| ByCase Statement.Fetch.Orientation | 0 |
| ByCase Statement.FullTextIndexDefinition.Tail | 7 |
| ByCase Statement.Grant.On | 2 |
| ByCase Statement.Grant.Privileges | 6 |
| ByCase Statement.Insert.Into | 7 |
| ByCase Statement.MasterKeyDefinition.Tail | 3 |
| ByCase Statement.OpenSymmetricKey.Tail | 7 |
| ByCase Statement.RaiseError.Tail | 0 |
| ByCase Statement.Revoke.Privileges | 10 |
| ByCase Statement.RuleDefinition.Tail | 1 |
| ByCase Statement.SchemaDefinition.Tail | 12 |
| ByCase Statement.SequenceDefinition.Tail | 1 |
| ByCase Statement.StatisticsDefinition.Tail | 26 |
| ByCase Statement.SymmetricKeyDefinition.Tail | 8 |
| ByCase Statement.SynonymDefinition.Tail | 18 |
| ByCase Statement.TableDefinition.Kind | 8 |
| ByCase Statement.Transaction.Kind | 0 |
| ByCase Statement.Transaction.Tail | 15 |
| ByCase Statement.Transaction.Word | 0 |
| ByCase Statement.TypeDefinition.Tail | 133 |
| ByCase Statement.UpdateStatistics.Tail | 8 |
| ByCase TableReference.Joined.Hint | 0 |
| Meaning CAST (a AS money): a type of T-SQL's own in T-SQL, a user-defined type in the standard | 43 |
| Meaning DATEADD (day, ...): a date part in T-SQL, a column in the standard | 6 |
| Meaning EXECUTE p: a procedure call in T-SQL, a prepared statement in the standard | 0 |
| Meaning FREETEXT ((c), ...): a column list in T-SQL, a value in brackets in the standard | 7 |
| Meaning FROM t OPTION (h): a hint in T-SQL, an alias in the standard | 10 |
| Meaning GLOBAL c: a cursor of the connection in T-SQL, an extended name in the standard | 6 |
| Meaning GROUP BY ALL a: the empty groups kept in T-SQL, a quantifier in the standard | 7 |
| Meaning SELECT a INTO t: a new table in T-SQL, a target in the standard | 3 |
| Meaning SELECT x = a: an alias in T-SQL, a comparison in the standard | 1 |
| Meaning SET t.c: a qualified column in T-SQL, a mutated set clause in the standard | 15 |
| Meaning TOP (n) a: a row count in T-SQL, a call to TOP in the standard | 2 |
| Meaning UPDATE ONLY (t): a call to a function ONLY in T-SQL, the table t alone in the standard | 0 |
| Meaning c t DEFAULT v COLLATE x: the default's collation in T-SQL, the column's in the standard | 1 |
| Projected a T-SQL column's options as the standard's members | 234 |
| Projected a key's T-SQL columns as the standard's names | 6 |
| Recovered Alias.AsKeyword of MERGE's target | 19 |
| Recovered Alias.AsKeyword of a table source | 948 |
| Recovered Clause.DerivedColumn alias = expression | 408 |
| Recovered Expression.Invocation without brackets | 43 |
| Recovered Expression.Trim.FromKeyword | 0 |
| Recovered Limit.FetchRowWord | 23 |
| Recovered Limit.OffsetRowWord | 28 |
| Recovered Limit.Position | 23 |
| Recovered ParameterDefinition.AsKeyword | 273 |
| Recovered ReferencesSpecification.DeleteRuleFirst | 44 |
| Recovered RoutineDefinition.AsOmitted | 174 |
| Recovered RoutineDefinition.Proc | 246 |
| Recovered RoutineDefinition.Unbracketed | 70 |
| Recovered SelectColumn.ExpressionItem.AsKeyword | 403 |
| Recovered Statement.Delete.FromOmitted | 85 |
| Recovered Statement.DropRoutine.Proc | 24 |
| Recovered Statement.ExecuteAs.Exec | 20 |
| Recovered Statement.ExecuteModule.Keyword | 88 |
| Recovered Statement.ExecuteString.Exec | 24 |
| Recovered Statement.SetTransaction.Tran | 23 |
| Recovered WindowReference.NameRef against (name) | 20 |

## The records

Category is the inventory's (`sql-tsql-tree-inventory.md`). Counted is what became of the statements counted
under the record — those whose first gap it is, or whose root it is. Gaps are every gap the record has, a
statement counted once for each record and reason it holds, with the commonest reasons. Occurrences are how
many nodes of the record the statements hold, at any depth. All three are over the corpus and the tests' inputs.

| Old record | Family | Category | Counted | Gaps | Occurrences |
| --- | --- | --- | --- | --- | ---: |
| `Expression.Literal` | Values | B | — | — | 9361 |
| `Expression.ColumnReference` | Values | B | — | 139: needs decision: inventory cross-cutting gaps, identifier styles (a name part `CERTIFICATE c`) (7); needs decision: inventory cross-cutting gaps, identifier styles (a name part `READ COMMITTED`) (6) | 9237 |
| `Clause.Option` | Tables | C | gap 144 | 144: needs decision: inventory Clause.Option (a bracketed list of values) (34); needs decision: inventory Clause.Option (a bracketed list after =) (21) | 6517 |
| `Clause.ColumnDefinition` | Tables | B | gap 116 | 116: needs decision: inventory DataType (`decimal(max)`: a word in the brackets of a type) (24); needs decision: inventory Clause.ConstraintDefinition (a default added WITH VALUES) (11) | 3528 |
| `Query.Specification` | Queries | B | — | — | 3369 |
| `Clause.ColumnOption` | Tables | B | gap 107 | 107: needs decision: inventory Statement.AlterTable (ALTER COLUMN with more than a collation and a nullability) (64); needs decision: inventory Clause.ColumnOption (COLUMN_SET FOR ALL_SPARSE_COLUMNS) (43) | 2995 |
| `Statement.Select` | Queries | B | converted 2371 | — | 2987 |
| `Clause.DerivedColumn` | Queries | A | — | — | 2978 |
| `TableReference.Named` | Tables | A | gap 5 | 5: needs decision: inventory Clause.Hint (a hint written against the sample) (4); needs decision: inventory cross-cutting gaps, identifier styles (a name part `db `) (1) | 2944 |
| `Expression.RoutineInvocation` | Values | B | gap 56 | 71: needs decision: inventory cross-cutting gaps, identifier styles (a name part `GROUPING SETS`) (12); needs decision: inventory Expression.RoutineInvocation (the word `INT`) (8) | 2310 |
| `Statement.TableDefinition` | Tables | B | converted 1610, gap 79 | 79: needs decision: inventory Statement.TableDefinition (CREATE EXTERNAL TABLE, which another task owns) (79) | 1906 |
| `Clause.ConstraintDefinition` | Tables | B | gap 36 | 36: needs decision: inventory Clause.ConstraintDefinition (an index's options without brackets, which IndexDefinition has no flag for) (25); needs decision: inventory Clause.ConstraintDefinition (PRIMARY KEY with HASH, COLUMNSTORE, ORDER, INCLUDE or a filter) (7) | 1841 |
| `Clause.SortSpecification` | Queries | A | — | — | 1303 |
| `Clause.QualifiedAsterisk` | Queries | A | — | — | 1221 |
| `Expression.Comparison` | Values | A | — | — | 841 |
| `Statement.AlterTable` | Tables | B | converted 540, gap 19 | 19: needs decision: inventory Statement.AlterTable (MERGE) (7); needs decision: inventory Statement.AlterTable (SPLIT) (5) | 644 |
| `Clause.EventPiece` | Monitoring | C | — | — | 514 |
| `Expression.NamedArgument` | Values | A | gap 43 | 179: needs decision: inventory Expression.NamedArgument (179) | 506 |
| `Statement.CreateIndex` | Tables | C | converted 358, gap 57 | 57: needs decision: inventory Statement.CreateIndex (SELECTIVE XML INDEX, which no record holds) (31); needs decision: inventory Statement.CreateIndex (a secondary XML index FOR a selective index's path) (13) | 495 |
| `TableReference.FunctionCall` | Tables | A | gap 6 | 6: needs decision: inventory TableReference.FunctionCall (a PREDICT that is not MODEL, DATA AS and a schema) (6) | 487 |
| `Expression.Parenthesized` | Values | A | — | — | 438 |
| `TableReference.Joined` | Tables | A | — | — | 382 |
| `Clause.VariableDeclaration` | Procedural | C | gap 31 | 31: needs decision: inventory DataType (`VECTOR(2, FLOAT32)`: a word in the brackets of a type) (2); needs decision: inventory DataType (`VECTOR(2, float16)`: a word in the brackets of a type) (2) | 357 |
| `Statement.Declare` | Procedural | C | converted 263 | — | 347 |
| `Statement.Compound` | Procedural | C | converted 17 | — | 338 |
| `Statement.EventSessionDefinition` | Monitoring | C | gap 323 | 323: no target record: Statement.CreateEventSession (323) | 323 |
| `Clause.ParameterDeclaration` | Routines | B | — | — | 298 |
| `Clause.Hint` | Queries | C | gap 5 | 5: needs decision: inventory Clause.Hint (the old WITH (n)) (4); needs decision: inventory Clause.Hint (a hint of a shape the family does not hold) (1) | 294 |
| `Statement.CreateProcedure` | Routines | B | converted 246, gap 19 | 19: needs decision: inventory DataType (`VECTOR(3, float32)`: a word in the brackets of a type) (1); needs decision: inventory DataType (`bit(max)`: a word in the brackets of a type) (1) | 285 |
| `Clause.Connection` | Tables | C | — | — | 270 |
| `Statement.EndpointDefinition` | Server | C | gap 249 | 249: no target record: Statement.CreateEndpoint (249) | 249 |
| `Statement.Insert` | Dml | A | converted 192, gap 9 | 9: needs decision: inventory Statement.Insert (a qualified or prefixed column `..a1`, where the column list holds identifiers) (4); needs decision: inventory Clause.MergeWhen (an INSERT that is not one row) (1) | 242 |
| `Expression.Measured` | Values | C | — | 190: target exists, no writer arm: Expression.Measured (190) | 234 |
| `Statement.AlterDatabaseSet` | Database | C | converted 218 | — | 218 |
| `Expression.Add` | Values | A | — | — | 198 |
| `Expression.RowValueConstructor` | Values | A | — | — | 186 |
| `Expression.WindowFunction` | Values | B | gap 4 | 4: needs decision: inventory Expression.WindowFunction (a window over something that is not a call) (3); needs decision: inventory Expression.WindowFunction (a JSON aggregate with WITHIN GROUP or a null treatment, which its record has no place for) (1) | 184 |
| `Statement.CreateFunction` | Routines | B | converted 174, gap 2 | 2: needs decision: inventory DataType (`VECTOR(3, float32)`: a word in the brackets of a type) (1); needs decision: inventory DataType (`int(max)`: a word in the brackets of a type) (1) | 182 |
| `Clause.Window` | Values | A | — | — | 181 |
| `Expression.Prefixed` | Values | C | gap 51 | 181: no target record: a typed T-SQL rowset table source (proposal 7) (181) | 181 |
| `Clause.OrderBy` | Queries | B | — | — | 177 |
| `Clause.Placement` | Tables | C | — | — | 177 |
| `Statement.TypeDefinition` | Tables | B | converted 150, gap 19 | 19: needs decision: inventory DataType (`VECTOR(3, float32)`: a word in the brackets of a type) (1); needs decision: inventory DataType (`bit(max)`: a word in the brackets of a type) (1) | 174 |
| `Expression.Negate` | Values | A | — | — | 173 |
| `Statement.SetStatement` | Procedural | C | converted 154 | — | 165 |
| `TableReference.Parenthesized` | Tables | A | — | — | 162 |
| `Clause.GroupBy` | Queries | B | — | — | 158 |
| `Clause.JsonColumn` | Tables | C | gap 1 | 1: no target record: RowsetColumn.Ordinal is a number, and keeps no spelling of it such as `00` (1) | 155 |
| `Statement.WorkloadGroupDefinition` | Server | C | gap 155 | 155: no target record: Statement.CreateWorkloadGroup (155) | 155 |
| `Statement.CreateDatabase` | Database | C | converted 146, gap 8 | 8: needs decision: Trees.FreeText (Statement.CreateDatabase.Containment or Tail, written otherwise than the writer prints it) (8) | 154 |
| `Clause.Set` | Dml | B | — | — | 151 |
| `Statement.AlterIndex` | Tables | C | converted 134, gap 14 | 14: needs decision: inventory Statement.AlterIndex (a selective XML index's paths) (14) | 148 |
| `Statement.Execute` | Routines | B | converted 135, shape 1 | — | 147 |
| `Statement.Return` | Procedural | A | converted 3 | — | 146 |
| `Clause.References` | Tables | A | — | — | 145 |
| `Statement.Update` | Dml | A | converted 114 | — | 140 |
| `Expression.GraphMatch` | Values | C | — | — | 131 |
| `Expression.Hinted` | Values | C | gap 37 | 127: needs decision: inventory Expression.Hinted (127) | 127 |
| `Statement.Transaction` | Procedural | B | converted 87, gap 28 | 28: no target record: TransactionName (a number and a name of two parts, `-5:a.b`) (28) | 125 |
| `Expression.And` | Values | A | — | — | 122 |
| `Statement.ServerAuditDefinition` | Monitoring | C | gap 118 | 118: no target record: Statement.CreateServerAudit (118) | 118 |
| `Statement.Delete` | Dml | A | converted 64 | — | 114 |
| `Expression.Subquery` | Values | A | — | — | 113 |
| `Statement.Grant` | Security | B | converted 108 | — | 108 |
| `Statement.ExternalFileFormatDefinition` | External | C | gap 101 | 101: no target record: Statement.CreateExternalFileFormat (101) | 101 |
| `Statement.FullTextIndexDefinition` | Tables | C | converted 45, gap 3 | 3: needs decision: inventory Statement.FullTextIndexDefinition (a catalog after its filegroup, or a place of other words) (2); needs decision: inventory Statement.FullTextIndexDefinition (ON (catalog) in brackets without a filegroup) (1) | 99 |
| `Expression.Member` | Values | A | — | — | 98 |
| `Clause.Output` | Dml | C | — | — | 97 |
| `TableReference.Derived` | Tables | A | — | — | 97 |
| `TableReference.Pivot` | Tables | C | — | — | 93 |
| `Statement.AlterFullTextIndex` | Tables | C | converted 70, gap 22 | 22: needs decision: inventory Statement.AlterFullTextIndex (WITH NO POPULATION, which no Option of one name says) (22) | 92 |
| `Statement.ExternalDataSourceDefinition` | External | C | gap 92 | 92: no target record: Statement.CreateExternalDataSource (92) | 92 |
| `Statement.Print` | Procedural | C | converted 38 | — | 89 |
| `Statement.SetVariable` | Procedural | C | converted 78 | — | 89 |
| `Clause.DatabaseFile` | Database | C | — | — | 88 |
| `Statement.ResourcePoolDefinition` | Server | C | gap 86 | 86: no target record: Statement.CreateResourcePool (86) | 86 |
| `Clause.Top` | Queries | C | — | — | 84 |
| `Query.TableValueConstructor` | Queries | A | — | — | 84 |
| `Statement.ViewDefinition` | Tables | B | converted 43, gap 37 | 37: needs decision: inventory Statement.ViewDefinition (CREATE MATERIALIZED VIEW, Synapse's) (37) | 84 |
| `Query.FromFile` | Dml | C | — | — | 83 |
| `Clause.Dropped` | Tables | B | gap 2 | 2: needs decision: inventory Clause.Dropped (a period or an index, which no action holds) (2) | 82 |
| `Clause.For` | Queries | C | — | — | 82 |
| `Statement.ExternalResourcePoolDefinition` | External | C | gap 79 | 79: no target record: Statement.CreateExternalResourcePool (79) | 79 |
| `Statement.CreateTrigger` | Routines | B | converted 61, gap 10 | 10: no target record: Statement.CreateTrigger (statement kinds and event names in one list, or the wrong kind for its target) (10) | 76 |
| `Statement.Dbcc` | Server | C | gap 73 | 73: no target record: Statement.Dbcc (73) | 73 |
| `Expression.Not` | Values | A | — | — | 71 |
| `Statement.CreateTableAsSelect` | Tables | B | converted 22, gap 31 | 31: needs decision: inventory Statement.CreateTableAsSelect (CREATE EXTERNAL TABLE AS SELECT, which another task owns) (30); needs decision: inventory Statement.CreateTableAsSelect (a query with OPTION, which TableContents.AsQuery cannot hold) (1) | 70 |
| `Statement.AlterServerConfiguration` | Server | C | gap 68 | 68: no target record: Statement.AlterServerConfiguration (68) | 68 |
| `Clause.SystemTime` | Tables | A | — | — | 67 |
| `Clause.MergeWhen` | Dml | B | — | — | 65 |
| `Query.Parenthesized` | Queries | B | — | — | 64 |
| `Statement.ExternalLibraryDefinition` | External | C | gap 59 | 59: no target record: Statement.CreateExternalLibrary (59) | 59 |
| `Clause.CommonTableExpression` | Queries | A | — | — | 57 |
| `Statement.CreateLogin` | Security | C | converted 57 | — | 57 |
| `Statement.UpdateText` | Dml | C | converted 57 | — | 57 |
| `Expression.OdbcEscape` | Values | C | — | — | 56 |
| `Statement.DatabaseAuditSpecificationDefinition` | Monitoring | C | gap 56 | 56: no target record: Statement.CreateDatabaseAuditSpecification (56) | 56 |
| `Expression.Collated` | Values | A | — | — | 55 |
| `Statement.DropIndex` | Drop | C | converted 47 | — | 53 |
| `Statement.Merge` | Dml | A | converted 46 | — | 53 |
| `Expression.IsNull` | Values | A | — | — | 52 |
| `Statement.AvailabilityGroupDefinition` | Server | C | gap 51 | 51: no target record: Statement.CreateAvailabilityGroup (51) | 51 |
| `Statement.TriggerSwitch` | Routines | C | converted 44 | — | 51 |
| `Statement.QueueDefinition` | Broker | C | gap 48 | 48: no target record: Statement.CreateQueue (48) | 48 |
| `Statement.If` | Procedural | C | converted 23 | — | 47 |
| `Statement.SequenceDefinition` | Tables | A | converted 46, gap 1 | 1: needs decision: inventory cross-cutting gaps, identifier styles (a name part ` TestSeq`) (1) | 47 |
| `Statement.AlterFullTextStopList` | Database | C | converted 46 | — | 46 |
| `Statement.RestoreDatabase` | Database | C | converted 26, gap 20 | 20: needs decision: Trees.FreeText (Statement.RestoreDatabase.Tail, written otherwise than the writer prints it) (11); no target record: Option (RESTORE … MOVE 'file' TO 'path': two values) (5) | 46 |
| `Statement.AuditSpecificationDefinition` | Monitoring | C | gap 45 | 45: no target record: Statement.CreateServerAuditSpecification (45) | 45 |
| `Statement.AssemblyDefinition` | Routines | C | converted 44 | — | 44 |
| `Statement.AsymmetricKeyDefinition` | Security | C | converted 43 | — | 43 |
| `Statement.StatisticsDefinition` | Tables | C | converted 43 | — | 43 |
| `Statement.UpdateStatistics` | Tables | C | converted 41 | — | 43 |
| `Clause.Into` | Queries | C | — | — | 41 |
| `Expression.Multiply` | Values | A | — | — | 41 |
| `Statement.EndConversation` | Broker | C | gap 34 | 34: no target record: Statement.EndConversation (34) | 41 |
| `Statement.ExternalLanguageDefinition` | External | C | gap 41 | 41: no target record: Statement.CreateExternalLanguage (41) | 41 |
| `Expression.Or` | Values | A | — | — | 40 |
| `Clause.WindowDefinition` | Values | A | — | — | 39 |
| `Query.Ordered` | Queries | B | — | — | 39 |
| `Expression.JsonPair` | Values | A | — | 31: needs decision: inventory Expression.JsonPair (31) | 38 |
| `Expression.IsDistinctFrom` | Values | A | — | — | 37 |
| `Statement.BackupDatabase` | Database | C | converted 24, gap 13 | 13: needs decision: Trees.FreeText (Statement.BackupDatabase.Tail, written otherwise than the writer prints it) (13) | 37 |
| `Statement.CreateUser` | Security | C | converted 37 | — | 37 |
| `Statement.SecurityPolicyDefinition` | Security | C | converted 37 | — | 37 |
| `Statement.Label` | Procedural | C | converted 30 | — | 36 |
| `Statement.Revoke` | Security | B | converted 36 | — | 36 |
| `SetExpression.Miscellaneous.Offset` | Procedural | C | — | — | 35 |
| `Statement.WriteText` | Dml | C | converted 35 | — | 35 |
| `Query.Union` | Queries | B | — | — | 34 |
| `Statement.Deny` | Security | C | converted 34 | — | 34 |
| `Statement.ExternalModelDefinition` | External | C | gap 34 | 34: no target record: Statement.CreateExternalModel (34) | 34 |
| `Statement.Fetch` | Procedural | A | converted 34 | — | 34 |
| `Clause.CursorDefinition` | Procedural | B | — | — | 32 |
| `Clause.TableSample` | Tables | A | — | — | 31 |
| `Clause.When` | Values | A | — | — | 30 |
| `Expression.Case` | Values | A | — | — | 30 |
| `Expression.ShiftLeft` | Values | C | — | — | 30 |
| `Statement.AlterDatabaseModify` | Database | C | converted 30 | — | 30 |
| `Statement.AlterDatabaseModifyFileGroup` | Database | C | converted 11, gap 19 | 19: needs decision: Trees.FreeText (Statement.AlterDatabaseModifyFileGroup.Tail, written otherwise than the writer prints it) (10); no target record: Statement.AlterDatabase (a termination after an action other than SET) (9) | 30 |
| `Statement.Send` | Broker | C | gap 30 | 30: no target record: Statement.Send (30) | 30 |
| `Expression.Aliased` | Values | A | — | 29: needs decision: inventory Expression.Aliased (29) | 29 |
| `Expression.In` | Values | A | — | — | 29 |
| `Statement.AlterResourceGovernor` | Server | C | gap 29 | 29: no target record: Statement.AlterResourceGovernor (29) | 29 |
| `Statement.SynonymDefinition` | Tables | C | converted 29 | — | 29 |
| `TableReference.Changed` | Dml | A | — | — | 29 |
| `Query.DefaultValues` | Dml | A | — | — | 28 |
| `Statement.Kill` | Server | C | gap 28 | 28: no target record: Statement.Kill (28) | 28 |
| `SetExpression.QueryExecution.Switch` | Procedural | C | — | — | 27 |
| `Statement.SchemaDefinition` | Security | A | converted 27 | — | 27 |
| `Statement.SymmetricKeyDefinition` | Security | C | converted 27 | — | 27 |
| `Clause.VariableAssignment` | Queries | C | — | — | 26 |
| `Statement.DeclareCursor` | Procedural | B | converted 22, gap 4 | 4: not kept by the old tree: Clause.CursorDefinition (the ISO form's words in another order, or twice) (2); needs decision: inventory cross-cutting gaps, identifier styles (a qualified name `dbo.t.a` where one name stands) (1) | 26 |
| `Expression.Like` | Values | A | — | — | 25 |
| `SetExpression.IsoSettings.Switch` | Procedural | C | — | — | 25 |
| `Statement.CursorAction` | Procedural | A | converted 25 | — | 25 |
| `Statement.Truncate` | Dml | A | converted 25 | — | 25 |
| `SetExpression.Statistics.Report` | Procedural | C | — | — | 24 |
| `Statement.DropProcedure` | Drop | B | converted 24 | — | 24 |
| `Statement.ReadText` | Dml | C | converted 24 | — | 24 |
| `SetExpression.Transactions.IsolationLevel` | Procedural | A | — | — | 23 |
| `Statement.BeginDialog` | Broker | C | gap 23 | 23: no target record: Statement.BeginDialog (23) | 23 |
| `Statement.BrokerPriorityDefinition` | Broker | C | gap 23 | 23: no target record: Statement.CreateBrokerPriority (23) | 23 |
| `Statement.DropTrigger` | Drop | B | converted 19 | — | 23 |
| `SetExpression.Miscellaneous.Switch` | Procedural | C | — | — | 22 |
| `Statement.Break` | Procedural | C | converted 3 | — | 22 |
| `Statement.OpenSymmetricKey` | Security | C | converted 22 | — | 22 |
| `Statement.AlterAsymmetricKey` | Security | C | converted 21 | — | 21 |
| `Statement.PartitionFunctionDefinition` | Database | C | converted 14, gap 7 | 7: needs decision: Trees.FreeText (Statement.PartitionFunctionDefinition.Tail, written otherwise than the writer prints it) (7) | 21 |
| `TableReference.Unpivot` | Tables | C | — | — | 21 |
| `Statement.CredentialDefinition` | Security | C | converted 20 | — | 20 |
| `Statement.ExecuteAs` | Procedural | C | converted 20 | — | 20 |
| `Statement.FullTextCatalogDefinition` | Database | C | converted 15, gap 5 | 5: needs decision: Trees.FreeText (Statement.FullTextCatalogDefinition.Tail, written otherwise than the writer prints it) (5) | 20 |
| `Statement.RaiseError` | Procedural | C | converted 12 | — | 20 |
| `Statement.RuleDefinition` | Routines | C | converted 20 | — | 20 |
| `Statement.WaitFor` | Procedural | C | converted 20 | — | 20 |
| `Expression.BitwiseNot` | Values | C | — | — | 19 |
| `Statement.CertificateDefinition` | Security | C | converted 19 | — | 19 |
| `Statement.DropDatabase` | Drop | C | converted 19 | — | 19 |
| `Statement.FullTextStopListDefinition` | Database | C | converted 19 | — | 19 |
| `Statement.TryCatch` | Procedural | C | converted 11 | — | 19 |
| `Statement.AggregateDefinition` | Routines | C | converted 18 | — | 18 |
| `Statement.AlterLogin` | Security | C | converted 18 | — | 18 |
| `Statement.DropAssembly` | Drop | C | converted 18 | — | 18 |
| `Statement.MessageTypeDefinition` | Broker | C | gap 18 | 18: no target record: Statement.CreateMessageType (18) | 18 |
| `Statement.ServiceDefinition` | Broker | C | gap 18 | 18: no target record: Statement.CreateService (18) | 18 |
| `Expression.Plus` | Values | A | — | — | 17 |
| `Statement.AlterPartitionFunction` | Database | C | converted 6, gap 11 | 11: needs decision: Trees.FreeText (Statement.AlterPartitionFunction.Tail, written otherwise than the writer prints it) (11) | 17 |
| `Statement.BackupCertificate` | Database | C | converted 14, gap 3 | 3: needs decision: Trees.FreeText (Statement.BackupCertificate.Tail, written otherwise than the writer prints it) (3) | 17 |
| `Statement.Classification` | Security | C | converted 17 | — | 17 |
| `Statement.DropTable` | Drop | B | converted 16 | — | 17 |
| `Statement.EventNotificationDefinition` | Monitoring | C | gap 17 | 17: no target record: Statement.CreateEventNotification (17) | 17 |
| `Statement.Receive` | Broker | C | gap 13 | 17: no target record: Statement.Receive (17) | 17 |
| `Statement.RemoteServiceBindingDefinition` | Broker | C | gap 17 | 17: no target record: Statement.CreateRemoteServiceBinding (17) | 17 |
| `Statement.AddSignature` | Security | C | converted 16 | — | 16 |
| `Statement.AlterMasterKey` | Security | C | converted 16 | — | 16 |
| `Statement.DefaultDefinition` | Routines | C | converted 16 | — | 16 |
| `Statement.DropFunction` | Drop | B | converted 16 | — | 16 |
| `Clause.PartitionRange` | Dml | C | — | — | 15 |
| `Statement.BackupTransactionLog` | Database | C | converted 7, gap 8 | 8: needs decision: Trees.FreeText (Statement.BackupTransactionLog.Tail, written otherwise than the writer prints it) (8) | 15 |
| `Statement.DropAggregate` | Drop | C | converted 15 | — | 15 |
| `Statement.DropView` | Drop | B | converted 15 | — | 15 |
| `Clause.FileGroup` | Database | C | — | — | 14 |
| `Expression.Concatenate` | Values | A | — | — | 14 |
| `Query.FromStream` | Dml | C | — | — | 14 |
| `Statement.AlterCertificate` | Security | C | converted 14 | — | 14 |
| `Statement.DropSensitivityClassification` | Drop | C | converted 14 | — | 14 |
| `Statement.DropSequence` | Drop | B | converted 14 | — | 14 |
| `Statement.DropType` | Drop | B | converted 14 | — | 14 |
| `Statement.RestoreLog` | Database | C | converted 12, gap 2 | 2: needs decision: Trees.FreeText (Statement.RestoreLog.Tail, written otherwise than the writer prints it) (1); no target record: Option (RESTORE … STOPATMARK = mark AFTER datetime: two values) (1) | 14 |
| `Expression.ShiftRight` | Values | C | — | — | 13 |
| `Statement.AlterDatabaseScopedConfiguration` | Database | C | converted 13 | — | 13 |
| `Statement.AlterFullTextCatalog` | Database | C | converted 8, gap 5 | 5: needs decision: Trees.FreeText (Statement.AlterFullTextCatalog.Tail, written otherwise than the writer prints it) (5) | 13 |
| `Statement.AlterServiceMasterKey` | Security | C | converted 13 | — | 13 |
| `Statement.AlterUser` | Security | C | converted 13 | — | 13 |
| `Statement.CryptographicProviderDefinition` | Security | C | converted 13 | — | 13 |
| `Statement.DropDefault` | Drop | C | converted 13 | — | 13 |
| `Statement.DropRule` | Drop | C | converted 13 | — | 13 |
| `Statement.DropSynonym` | Drop | C | converted 13 | — | 13 |
| `Statement.SetUser` | Procedural | C | converted 13 | — | 13 |
| `Clause.ExecutionContext` | Routines | C | — | — | 12 |
| `Expression.Divide` | Values | A | — | — | 12 |
| `Expression.RowsetOrder` | Values | C | — | 12: no target record: a typed T-SQL rowset table source (proposal 7) (12) | 12 |
| `Statement.DropSignature` | Drop | C | converted 12 | — | 12 |
| `Statement.DropStatistics` | Drop | C | converted 12 | — | 12 |
| `Expression.Exists` | Values | A | — | — | 11 |
| `Statement.AlterAuthorization` | Security | C | converted 11 | — | 11 |
| `Statement.AlterDatabaseRebuildLog` | Database | C | converted 5, gap 6 | 6: no target record: AlterDatabaseAction.RebuildLog (REBUILD LOG ON a file) (6) | 11 |
| `Statement.AlterSchema` | Security | C | converted 11 | — | 11 |
| `Statement.AlterSearchPropertyList` | Database | C | converted 5, gap 6 | 6: needs decision: Trees.FreeText (Statement.AlterSearchPropertyList.Tail, written otherwise than the writer prints it) (6) | 11 |
| `Statement.DropEventNotification` | Drop | C | converted 9, gap 2 | 2: needs decision: inventory Statement.DropEventNotification (`on queue dbo.q1` kept as written, against `ON QUEUE dbo.q1` as the writer prints it, which the old tree compares exactly) (1); needs decision: inventory Statement.DropEventNotification (`on queue q1` kept as written, against `ON QUEUE q1` as the writer prints it, which the old tree compares exactly) (1) | 11 |
| `Statement.DropQueue` | Drop | C | converted 11 | — | 11 |
| `Statement.DropSecurityPolicy` | Drop | C | converted 11 | — | 11 |
| `Statement.PartitionSchemeDefinition` | Database | C | converted 9, gap 2 | 2: needs decision: Trees.FreeText (Statement.PartitionSchemeDefinition.Tail, written otherwise than the writer prints it) (2) | 11 |
| `Statement.Revert` | Procedural | C | converted 11 | — | 11 |
| `Statement.XmlSchemaCollectionDefinition` | Routines | C | converted 11 | — | 11 |
| `Expression.Between` | Values | A | — | — | 10 |
| `Statement.AlterPartitionScheme` | Database | C | converted 7, gap 3 | 3: needs decision: Trees.FreeText (Statement.AlterPartitionScheme.Tail, written otherwise than the writer prints it) (3) | 10 |
| `Statement.DatabaseScopedCredentialDefinition` | Security | C | converted 10 | — | 10 |
| `Statement.DropEventSession` | Drop | C | converted 10 | — | 10 |
| `Statement.GetConversationGroup` | Broker | C | gap 4 | 10: no target record: Statement.GetConversationGroup (10) | 10 |
| `Statement.WaitForStatement` | Broker | C | gap 10 | 10: no target record: Statement.WaitForReceive (10) | 10 |
| `Clause.CursorFor` | Procedural | A | — | — | 9 |
| `Expression.Subtract` | Values | A | — | — | 9 |
| `Statement.AlterDatabaseCollate` | Database | C | converted 9 | — | 9 |
| `Statement.DropXmlSchemaCollection` | Drop | C | converted 9 | — | 9 |
| `Statement.MasterKeyDefinition` | Security | C | converted 9 | — | 9 |
| `Statement.Reconfigure` | Server | C | gap 9 | 9: no target record: Statement.Reconfigure (9) | 9 |
| `Statement.Throw` | Procedural | C | converted 7 | — | 9 |
| `Statement.Use` | Procedural | C | converted 9 | — | 9 |
| `TableReference.OdbcJoin` | Tables | C | — | — | 9 |
| `SetExpression.DateAndTime.DateFirst` | Procedural | C | — | — | 8 |
| `SetExpression.QueryExecution.TextSize` | Procedural | C | — | — | 8 |
| `SetExpression.Statistics.Switch` | Procedural | C | — | — | 8 |
| `Statement.AlterDatabaseAddFileGroup` | Database | C | converted 8 | — | 8 |
| `Statement.AlterDatabaseAddLogFile` | Database | C | converted 4, gap 4 | 4: no target record: AlterDatabaseAction.AddLogFile (TO FILEGROUP after a log file) (4) | 8 |
| `Statement.AlterRole` | Security | C | converted 8 | — | 8 |
| `Statement.AlterXmlSchemaCollection` | Routines | C | converted 8 | — | 8 |
| `Statement.ColumnMasterKeyDefinition` | Security | C | converted 8 | — | 8 |
| `Statement.DatabaseEncryptionKeyDefinition` | Security | C | converted 8 | — | 8 |
| `Statement.DropExternalLanguage` | Drop | C | converted 8 | — | 8 |
| `Statement.DropExternalLibrary` | Drop | C | converted 8 | — | 8 |
| `Statement.DropRole` | Drop | B | converted 8 | — | 8 |
| `Statement.DropUser` | Drop | C | converted 6 | — | 8 |
| `Statement.GoTo` | Procedural | C | converted 3 | — | 8 |
| `SetExpression.Miscellaneous.IdentityInsert` | Procedural | C | — | — | 7 |
| `Statement.AlterDatabaseAddFile` | Database | C | converted 7 | — | 7 |
| `Statement.AlterServerRole` | Security | C | converted 7 | — | 7 |
| `Statement.Checkpoint` | Database | C | converted 7 | — | 7 |
| `Statement.Continue` | Procedural | C | converted 2 | — | 7 |
| `Statement.DropAsymmetricKey` | Drop | C | converted 7 | — | 7 |
| `Statement.DropContract` | Drop | C | converted 7 | — | 7 |
| `Statement.DropMessageType` | Drop | C | converted 7 | — | 7 |
| `Statement.DropRemoteServiceBinding` | Drop | C | converted 7 | — | 7 |
| `Statement.DropRoute` | Drop | C | converted 7 | — | 7 |
| `Statement.DropService` | Drop | C | converted 7 | — | 7 |
| `Statement.DropSymmetricKey` | Drop | C | converted 7 | — | 7 |
| `Statement.RestoreMasterKey` | Database | C | converted 6, gap 1 | 1: needs decision: Trees.FreeText (Statement.RestoreMasterKey.Tail, written otherwise than the writer prints it) (1) | 7 |
| `Statement.RouteDefinition` | Broker | C | gap 7 | 7: no target record: Statement.CreateRoute (7) | 7 |
| `Statement.SearchPropertyListDefinition` | Database | C | converted 7 | — | 7 |
| `Query.Except` | Queries | B | — | — | 6 |
| `SetExpression.QueryExecution.RowCount` | Procedural | C | — | — | 6 |
| `Statement.AlterSymmetricKey` | Security | C | converted 6 | — | 6 |
| `Statement.ColumnEncryptionKeyDefinition` | Security | C | converted 6 | — | 6 |
| `Statement.ContractDefinition` | Broker | C | gap 6 | 6: no target record: Statement.CreateContract (6) | 6 |
| `Statement.CreateApplicationRole` | Security | C | converted 6 | — | 6 |
| `Statement.CreateRole` | Security | A | converted 6 | — | 6 |
| `Statement.DropDatabaseAuditSpecification` | Drop | C | converted 6 | — | 6 |
| `Statement.DropFulltextIndex` | Drop | C | converted 6 | — | 6 |
| `Statement.DropSchema` | Drop | B | converted 6 | — | 6 |
| `Statement.DropServerAudit` | Drop | C | converted 6 | — | 6 |
| `Statement.DropServerAuditSpecification` | Drop | C | converted 6 | — | 6 |
| `Statement.DropWorkloadClassifier` | Drop | C | converted 6 | — | 6 |
| `Statement.RestoreServiceMasterKey` | Database | C | converted 4, gap 2 | 2: needs decision: Trees.FreeText (Statement.RestoreServiceMasterKey.Tail, written otherwise than the writer prints it) (2) | 6 |
| `Statement.SetCursor` | Procedural | C | converted 6 | — | 6 |
| `Statement.Shutdown` | Server | C | gap 6 | 6: no target record: Statement.Shutdown (6) | 6 |
| `Statement.While` | Procedural | C | converted 6 | — | 6 |
| `Query.FromExecute` | Routines | C | — | 5: needs decision: inventory Query.FromExecute (outside an INSERT) (5) | 5 |
| `SetExpression.Transactions.Switch` | Procedural | C | — | — | 5 |
| `Statement.AlterColumnEncryptionKey` | Security | C | converted 5 | — | 5 |
| `Statement.AlterDatabaseModifyFile` | Database | C | converted 5 | — | 5 |
| `Statement.CloseSymmetricKey` | Security | C | converted 5 | — | 5 |
| `Statement.ConversationTimer` | Broker | C | gap 5 | 5: no target record: Statement.BeginConversationTimer (5) | 5 |
| `Statement.CreateServerRole` | Security | C | converted 5 | — | 5 |
| `Statement.DropExternalTable` | Drop | C | converted 5 | — | 5 |
| `Statement.MoveConversation` | Broker | C | gap 5 | 5: no target record: Statement.MoveConversation (5) | 5 |
| `Expression.Modulo` | Values | C | — | — | 4 |
| `Query.Intersect` | Queries | B | — | — | 4 |
| `SetExpression.Miscellaneous.FipsFlagger` | Procedural | C | — | — | 4 |
| `SetExpression.QueryExecution.QueryGovernorCostLimit` | Procedural | C | — | — | 4 |
| `Statement.AlterApplicationRole` | Security | C | converted 4 | — | 4 |
| `Statement.AlterDatabaseEncryptionKey` | Security | C | converted 4 | — | 4 |
| `Statement.AlterDatabaseModifyName` | Database | C | converted 4 | — | 4 |
| `Statement.BackupMasterKey` | Database | C | converted 4 | — | 4 |
| `Statement.DropAvailabilityGroup` | Drop | C | converted 4 | — | 4 |
| `Statement.DropCryptographicProvider` | Drop | C | converted 4 | — | 4 |
| `Statement.DropDatabaseScopedCredential` | Drop | C | converted 4 | — | 4 |
| `Statement.DropExternalModel` | Drop | C | converted 4 | — | 4 |
| `Statement.DropSearchPropertyList` | Drop | C | converted 4 | — | 4 |
| `Statement.DropServerRole` | Drop | C | converted 4 | — | 4 |
| `Statement.OpenMasterKey` | Security | C | converted 4 | — | 4 |
| `Statement.RestoreRewindOnly` | Database | C | converted 2, gap 2 | 2: needs decision: Trees.FreeText (Statement.RestoreRewindOnly.Tail, written otherwise than the writer prints it) (2) | 4 |
| `Expression.BitwiseAnd` | Values | C | — | — | 3 |
| `SetExpression.DateAndTime.DateFormat` | Procedural | C | — | — | 3 |
| `SetExpression.Locking.DeadlockPriority` | Procedural | C | — | — | 3 |
| `SetExpression.Miscellaneous.ErrorLevel` | Procedural | C | — | — | 3 |
| `SetExpression.Miscellaneous.Language` | Procedural | C | — | — | 3 |
| `Statement.AlterDatabaseRemoveFile` | Database | C | converted 3 | — | 3 |
| `Statement.BackupServiceMasterKey` | Database | C | converted 2, gap 1 | 1: needs decision: Trees.FreeText (Statement.BackupServiceMasterKey.Tail, written otherwise than the writer prints it) (1) | 3 |
| `Statement.CloseAllSymmetricKeys` | Security | C | converted 3 | — | 3 |
| `Statement.CloseMasterKey` | Security | C | converted 3 | — | 3 |
| `Statement.DropApplicationRole` | Drop | C | converted 3 | — | 3 |
| `Statement.DropBrokerPriority` | Drop | C | converted 3 | — | 3 |
| `Statement.DropCertificate` | Drop | C | converted 3 | — | 3 |
| `Statement.DropColumnEncryptionKey` | Drop | C | converted 3 | — | 3 |
| `Statement.DropColumnMasterKey` | Drop | C | converted 3 | — | 3 |
| `Statement.DropCredential` | Drop | C | converted 3 | — | 3 |
| `Statement.DropDatabaseEncryptionKey` | Drop | C | converted 3 | — | 3 |
| `Statement.DropEndpoint` | Drop | C | converted 3 | — | 3 |
| `Statement.DropFulltextCatalog` | Drop | C | converted 3 | — | 3 |
| `Statement.DropFulltextStoplist` | Drop | C | converted 3 | — | 3 |
| `Statement.DropLogin` | Drop | C | converted 3 | — | 3 |
| `Statement.DropMasterKey` | Drop | C | converted 3 | — | 3 |
| `Statement.DropPartitionFunction` | Drop | C | converted 3 | — | 3 |
| `Statement.DropPartitionScheme` | Drop | C | converted 3 | — | 3 |
| `Statement.DropResourcePool` | Drop | C | converted 3 | — | 3 |
| `Statement.DropWorkloadGroup` | Drop | C | converted 3 | — | 3 |
| `Statement.LineNumber` | Procedural | C | converted 3 | — | 3 |
| `Statement.RestoreFileListOnly` | Database | C | converted 2, gap 1 | 1: needs decision: Trees.FreeText (Statement.RestoreFileListOnly.Tail, written otherwise than the writer prints it) (1) | 3 |
| `Statement.RestoreHeaderOnly` | Database | C | converted 2, gap 1 | 1: no target record: Option (RESTORE … MOVE 'file' TO 'path': two values) (1) | 3 |
| `Statement.RestoreVerifyOnly` | Database | C | converted 2, gap 1 | 1: needs decision: Trees.FreeText (Statement.RestoreVerifyOnly.Tail, written otherwise than the writer prints it) (1) | 3 |
| `Expression.BitwiseOr` | Values | C | — | — | 2 |
| `Expression.BitwiseXor` | Values | C | — | — | 2 |
| `SetExpression.Locking.LockTimeout` | Procedural | C | — | — | 2 |
| `SetExpression.Miscellaneous.ContextInfo` | Procedural | C | — | — | 2 |
| `Statement.AlterDatabaseModifyBackupStorageRedundancy` | Database | C | converted 2 | — | 2 |
| `Statement.AlterDatabasePerformCutover` | Database | C | converted 2 | — | 2 |
| `Statement.AlterDatabaseRemoveFileGroup` | Database | C | converted 2 | — | 2 |
| `Statement.DropExternalDataSource` | Drop | C | converted 2 | — | 2 |
| `Statement.DropExternalFileFormat` | Drop | C | converted 2 | — | 2 |
| `Statement.DropExternalResourcePool` | Drop | C | converted 2 | — | 2 |
| `Statement.RestoreLabelOnly` | Database | C | converted 1, gap 1 | 1: needs decision: Trees.FreeText (Statement.RestoreLabelOnly.Tail, written otherwise than the writer prints it) (1) | 2 |
| `Statement.RestoreSymmetricKey` | Database | C | converted 2 | — | 2 |
| `Expression.Pieced` | Values | C | — | 1: no target record: a typed T-SQL rowset table source (proposal 7) (1) | 1 |
| `Expression.Quantified` | Values | A | — | — | 1 |
| `Query.ExplicitTable` | Queries | A | — | — | 1 |
| `Statement.BackupGroup` | Database | C | converted 1 | — | 1 |
| `Statement.BackupSymmetricKey` | Database | C | converted 1 | — | 1 |
| `Expression.IsTruth` | Values | A | — | — | 0 |
| `Expression.Match` | Values | A | — | — | 0 |
| `Expression.Overlaps` | Values | A | — | — | 0 |
| `Expression.Unique` | Values | A | — | — | 0 |
| `Statement.BackupServer` | Database | C | — | — | 0 |
