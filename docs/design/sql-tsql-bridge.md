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

1. `TransactSqlParser.Located` reads a statement into the old tree, through the internal entries that still build
   it (`ParseSyntax…`);
2. `Bridge.ToAst` carries it onto the new tree, or throws a `BridgeGap` naming the old record and why;
3. `TransactSqlWriter.Write` prints the new tree, refuses it, or crashes;
4. `TransactSqlParser` reads the text back, and the old tree it builds is held to the first.

The bridge is `src/DotGram.Sql/TransactSql/Bridge`, internal: the parser's public entries carry what they read
across with it, and a statement it has no place for is handed back as an extension of the new tree.

Every statement rule now builds the new tree in its action, its parts still carried by the bridge: such a
statement is read into the old tree as a `Statement.Built`, which holds what was built (or the gap found) and
is counted under the record the rule built before. Its round trip is the new tree's own: the statement is
written, read back, and the two new trees are held to each other. A query statement is built where it is
carried, since the WITH in front of it is read by the rule around it; until then it holds what its rule
read, and its round trip holds that too: its order, FOR, hints and WITH, which are still the old tree's
records, compared as the old tree's are.

The query and table source rules build the new tree in their actions as well, a query's and a source's parts
still carried by the bridge: such a query or source stands where the old tree holds one as a `Query.Built` or a
`TableReference.Built`, which holds what was built (or the gap found), what the rules that hold one ask of it,
and a specification's select list and condition as they were read; it is carried as what it holds, and counted
under the record its rule built before.

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
- `FREETEXT ((c), …)`, `CONTAINS ((c), …)`: T-SQL's full-text predicate with a column list, a call with a value
  in brackets in the standard. The `FullTextPredicate`, where the standard's call has the same search and nothing
  else.
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
- `Statement.Classification.Options`: the names of ADD SENSITIVITY CLASSIFICATION's options and the RANK word, as written (the old tree upper-cases them).

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
| ByCase Clause.ColumnDefinition.Type | 0 |
| ByCase Clause.Dropped.Tail | 0 |
| ByCase Clause.EventPiece.Where | 0 |
| ByCase Clause.For.Kind | 20 |
| ByCase Clause.For.Options | 44 |
| ByCase Clause.GroupBy.With | 0 |
| ByCase Clause.Hint.Text | 23 |
| ByCase Clause.JsonColumn.Type | 0 |
| ByCase Clause.Option.Name | 4 |
| ByCase Clause.Option.Partitions | 0 |
| ByCase Clause.ParameterDeclaration.Type | 0 |
| ByCase Clause.SystemTime.Kind | 0 |
| ByCase Clause.TableSample.Unit | 0 |
| ByCase Clause.Top.With | 0 |
| ByCase Clause.VariableDeclaration.Type | 0 |
| ByCase Clause.Window.Frame | 0 |
| ByCase Expression.ColumnReference.Text of EXECUTE AS | 0 |
| ByCase Expression.ColumnReference.Text of a date part | 0 |
| ByCase Expression.ColumnReference.Text under Statement.DropIndex | 0 |
| ByCase Expression.ColumnReference.Text under Statement.DropSignature | 0 |
| ByCase Expression.GraphMatch.Pattern | 0 |
| ByCase Expression.Hinted.Words | 0 |
| ByCase Expression.Literal.Text | 2 |
| ByCase Expression.Match.Qualifier | 0 |
| ByCase Expression.NamedArgument.Name | 0 |
| ByCase Expression.OdbcEscape.Kind | 0 |
| ByCase Expression.Prefixed.Word | 0 |
| ByCase Expression.Quantified.Quantifier | 0 |
| ByCase Expression.RoutineInvocation.Name | 0 |
| ByCase Expression.RoutineInvocation.Word | 0 |
| ByCase Expression.WindowFunction.Within | 0 |
| ByCase Query.Specification.Quantifier | 0 |
| ByCase SetExpression.Statistics.Report.Kind | 0 |
| ByCase SetExpression.Transactions.IsolationLevel.Level | 0 |
| ByCase Statement.AddSignature.To | 0 |
| ByCase Statement.AggregateDefinition.Tail | 0 |
| ByCase Statement.AlterAsymmetricKey.Tail | 0 |
| ByCase Statement.AlterCertificate.Tail | 0 |
| ByCase Statement.AlterColumnEncryptionKey.Tail | 0 |
| ByCase Statement.AlterDatabaseModifyFileGroup.Tail | 0 |
| ByCase Statement.AlterDatabaseRebuildLog.Tail | 0 |
| ByCase Statement.AlterFullTextCatalog.Tail | 0 |
| ByCase Statement.AlterFullTextIndex.Tail | 0 |
| ByCase Statement.AlterIndex.Namespaces | 0 |
| ByCase Statement.AlterIndex.Paths | 0 |
| ByCase Statement.AlterMasterKey.Tail | 0 |
| ByCase Statement.AlterPartitionFunction.Tail | 0 |
| ByCase Statement.AlterPartitionScheme.Tail | 0 |
| ByCase Statement.AlterResourceGovernor.Tail | 0 |
| ByCase Statement.AlterRole.Tail | 0 |
| ByCase Statement.AlterSchema.Tail | 0 |
| ByCase Statement.AlterSearchPropertyList.Tail | 0 |
| ByCase Statement.AlterServerConfiguration.Tail | 0 |
| ByCase Statement.AlterServerRole.Tail | 0 |
| ByCase Statement.AlterServiceMasterKey.Tail | 0 |
| ByCase Statement.AlterSymmetricKey.Tail | 0 |
| ByCase Statement.AlterTable.Tail | 0 |
| ByCase Statement.AlterUser.Tail | 0 |
| ByCase Statement.AssemblyDefinition.Tail | 0 |
| ByCase Statement.AsymmetricKeyDefinition.Tail | 0 |
| ByCase Statement.AuditSpecificationDefinition.Tail | 0 |
| ByCase Statement.AvailabilityGroupDefinition.Tail | 0 |
| ByCase Statement.BackupCertificate.Tail | 0 |
| ByCase Statement.BackupDatabase.Tail | 0 |
| ByCase Statement.BackupServiceMasterKey.Tail | 0 |
| ByCase Statement.BackupTransactionLog.Tail | 0 |
| ByCase Statement.BrokerPriorityDefinition.Tail | 0 |
| ByCase Statement.CertificateDefinition.Tail | 0 |
| ByCase Statement.ColumnEncryptionKeyDefinition.Tail | 0 |
| ByCase Statement.ColumnMasterKeyDefinition.Tail | 0 |
| ByCase Statement.ContractDefinition.Tail | 0 |
| ByCase Statement.CreateDatabase.Containment | 0 |
| ByCase Statement.CreateDatabase.Tail | 0 |
| ByCase Statement.CreateFunction.Returns | 0 |
| ByCase Statement.CreateIndex.Using | 0 |
| ByCase Statement.CreateLogin.Tail | 0 |
| ByCase Statement.CreateRole.Tail | 0 |
| ByCase Statement.CreateServerRole.Tail | 0 |
| ByCase Statement.CreateUser.Tail | 0 |
| ByCase Statement.CredentialDefinition.Tail | 0 |
| ByCase Statement.CursorAction.Verb | 0 |
| ByCase Statement.DatabaseAuditSpecificationDefinition.Tail | 0 |
| ByCase Statement.DefaultDefinition.Tail | 0 |
| ByCase Statement.Deny.Privileges | 0 |
| ByCase Statement.DropAssembly.Tail | 0 |
| ByCase Statement.DropAsymmetricKey.Tail | 0 |
| ByCase Statement.DropEventNotification.Tail | 0 |
| ByCase Statement.DropEventSession.Tail | 0 |
| ByCase Statement.DropSignature.From | 0 |
| ByCase Statement.DropSymmetricKey.Tail | 0 |
| ByCase Statement.DropTrigger.Tail | 0 |
| ByCase Statement.EndpointDefinition.Payload | 0 |
| ByCase Statement.EndpointDefinition.Protocol | 0 |
| ByCase Statement.EventNotificationDefinition.Tail | 0 |
| ByCase Statement.EventSessionDefinition.State | 0 |
| ByCase Statement.Execute.Tail | 0 |
| ByCase Statement.ExternalDataSourceDefinition.Tail | 0 |
| ByCase Statement.ExternalFileFormatDefinition.Tail | 0 |
| ByCase Statement.ExternalLanguageDefinition.Tail | 0 |
| ByCase Statement.ExternalLibraryDefinition.Tail | 0 |
| ByCase Statement.ExternalModelDefinition.Tail | 0 |
| ByCase Statement.ExternalResourcePoolDefinition.Tail | 0 |
| ByCase Statement.Fetch.Orientation | 0 |
| ByCase Statement.FullTextCatalogDefinition.Tail | 0 |
| ByCase Statement.FullTextIndexDefinition.Tail | 0 |
| ByCase Statement.Grant.On | 0 |
| ByCase Statement.Grant.Privileges | 0 |
| ByCase Statement.Insert.Into | 0 |
| ByCase Statement.MasterKeyDefinition.Tail | 0 |
| ByCase Statement.MessageTypeDefinition.Tail | 0 |
| ByCase Statement.OpenSymmetricKey.Tail | 0 |
| ByCase Statement.PartitionFunctionDefinition.Tail | 0 |
| ByCase Statement.PartitionSchemeDefinition.Tail | 0 |
| ByCase Statement.QueueDefinition.Tail | 0 |
| ByCase Statement.RaiseError.Tail | 0 |
| ByCase Statement.RemoteServiceBindingDefinition.Tail | 0 |
| ByCase Statement.ResourcePoolDefinition.Tail | 0 |
| ByCase Statement.RestoreDatabase.Tail | 0 |
| ByCase Statement.RestoreFileListOnly.Tail | 0 |
| ByCase Statement.RestoreLabelOnly.Tail | 0 |
| ByCase Statement.RestoreLog.Tail | 0 |
| ByCase Statement.RestoreMasterKey.Tail | 0 |
| ByCase Statement.RestoreRewindOnly.Tail | 0 |
| ByCase Statement.RestoreServiceMasterKey.Tail | 0 |
| ByCase Statement.RestoreVerifyOnly.Tail | 0 |
| ByCase Statement.Revoke.Privileges | 0 |
| ByCase Statement.RouteDefinition.Tail | 0 |
| ByCase Statement.RuleDefinition.Tail | 0 |
| ByCase Statement.SchemaDefinition.Tail | 0 |
| ByCase Statement.SequenceDefinition.Tail | 0 |
| ByCase Statement.ServerAuditDefinition.Tail | 0 |
| ByCase Statement.ServiceDefinition.Tail | 0 |
| ByCase Statement.StatisticsDefinition.Tail | 0 |
| ByCase Statement.SymmetricKeyDefinition.Tail | 0 |
| ByCase Statement.SynonymDefinition.Tail | 0 |
| ByCase Statement.TableDefinition.Kind | 0 |
| ByCase Statement.Transaction.Kind | 0 |
| ByCase Statement.Transaction.Name | 0 |
| ByCase Statement.Transaction.Tail | 0 |
| ByCase Statement.Transaction.Word | 0 |
| ByCase Statement.TypeDefinition.Tail | 0 |
| ByCase Statement.UpdateStatistics.Tail | 0 |
| ByCase Statement.WorkloadGroupDefinition.Tail | 0 |
| ByCase TableReference.Joined.Hint | 0 |
| Meaning CAST (a AS money): a type of T-SQL's own in T-SQL, a user-defined type in the standard | 43 |
| Meaning DATEADD (day, ...): a date part in T-SQL, a column in the standard | 6 |
| Meaning EXECUTE p: a procedure call in T-SQL, a prepared statement in the standard | 0 |
| Meaning FREETEXT ((c), ...): T-SQL's full-text predicate, a call with a value in brackets in the standard | 12 |
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
| Recovered Alias.AsKeyword of MERGE's target | 21 |
| Recovered Alias.AsKeyword of a table source | 1018 |
| Recovered Clause.DerivedColumn alias = expression | 505 |
| Recovered Expression.Invocation without brackets | 49 |
| Recovered Expression.Trim.FromKeyword | 0 |
| Recovered Limit.FetchRowWord | 23 |
| Recovered Limit.OffsetRowWord | 28 |
| Recovered Limit.Position | 23 |
| Recovered ParameterDefinition.AsKeyword | 300 |
| Recovered ReferencesSpecification.DeleteRuleFirst | 44 |
| Recovered RoutineDefinition.AsOmitted | 184 |
| Recovered RoutineDefinition.Proc | 288 |
| Recovered RoutineDefinition.Unbracketed | 94 |
| Recovered SelectColumn.ExpressionItem.AsKeyword | 498 |
| Recovered Statement.Classification.Options | 19 |
| Recovered Statement.Delete.FromOmitted | 89 |
| Recovered Statement.ExecuteAs.Exec | 21 |
| Recovered Statement.ExecuteModule.Keyword | 90 |
| Recovered Statement.ExecuteString.Exec | 25 |
| Recovered Statement.SetTransaction.Tran | 23 |
| Recovered WindowReference.NameRef against (name) | 20 |

## The records

Category is the inventory's (`sql-tsql-tree-inventory.md`). Counted is what became of the statements counted
under the record — those whose first gap it is, or whose root it is. Gaps are every gap the record has, a
statement counted once for each record and reason it holds, with the commonest reasons. Occurrences are how
many nodes of the record the statements hold, at any depth. All three are over the corpus and the tests' inputs.
A statement, a query or a source whose rule builds the new tree holds no node of the old tree, and what was read
under it is in neither of the last two.

| Old record | Family | Category | Counted | Gaps | Occurrences |
| --- | --- | --- | --- | --- | ---: |
| `Statement.Select` | Queries | B | converted 2534 | — | 2562 |
| `Statement.TableDefinition` | Tables | B | converted 1807 | — | 1829 |
| `Statement.AlterTable` | Tables | B | converted 648, gap 1 | 1: needs decision: inventory Statement.AlterTable (a constraint added with a FOR the old tree drops) (1) | 649 |
| `Statement.CreateIndex` | Tables | C | converted 493, gap 2 | 2: needs decision: inventory Statement.CreateIndex (a spatial index USING FOO) (2) | 495 |
| `Statement.EventSessionDefinition` | Monitoring | C | converted 321, gap 2 | 2: not kept by the old tree: a number written with its unit against it (`1MB`): the text is compared by tokens, and the tree writes the unit after a blank (2) | 323 |
| `Statement.Declare` | Procedural | C | converted 292 | — | 301 |
| `Statement.CreateProcedure` | Routines | B | converted 288, gap 2 | 2: needs decision: inventory DataType (a type `double precision(max)` of several words) (1); target exists, no writer arm: Clause.ParameterDeclaration.Type, which the writer refuses (ArgumentException) (1) | 290 |
| `Statement.EndpointDefinition` | Server | C | converted 251, gap 1 | 1: needs decision: inventory Statement.EndpointDefinition (`CREATE ENDPOINT e AS TCP (LISTENER_PORT = 4022) FOR SERVICE_BROKER (AUTHENTICATION = WINDOWS) ; ;` after the name) (1) | 252 |
| `Statement.AlterDatabaseSet` | Database | C | converted 218 | — | 218 |
| `Statement.Insert` | Dml | A | converted 197, gap 9 | 9: needs decision: inventory Statement.Insert (a qualified or prefixed column `..a1`, where the column list holds identifiers) (4); needs decision: inventory Clause.MergeWhen (an INSERT that is not one row) (1) | 205 |
| `Statement.CreateFunction` | Routines | B | converted 184 | — | 184 |
| `Statement.TypeDefinition` | Tables | B | converted 173, gap 1 | 1: needs decision: inventory DataType (a type `double precision(max)` of several words) (1) | 174 |
| `Statement.WorkloadGroupDefinition` | Server | C | converted 155 | — | 155 |
| `Statement.CreateDatabase` | Database | C | converted 154 | — | 154 |
| `Statement.SetStatement` | Procedural | C | converted 154 | — | 154 |
| `Statement.AlterIndex` | Tables | C | converted 148 | — | 148 |
| `Statement.Execute` | Routines | B | converted 138, gap 4, shape 1 | 4: needs decision: inventory Statement.Execute (a result set `a INT , CHECK ( a > 0 )` with more than columns each with its type) (1); needs decision: inventory Statement.Execute (a result set `a INT , CONSTRAINT k PRIMARY KEY ( a )` with more than columns each with its type) (1) | 142 |
| `Statement.ServerAuditDefinition` | Monitoring | C | converted 117, gap 2 | 2: not kept by the old tree: a number written with its unit against it (`1MB`): the text is compared by tokens, and the tree writes the unit after a blank (2) | 119 |
| `Statement.Update` | Dml | A | converted 116 | — | 116 |
| `Statement.Transaction` | Procedural | B | converted 115 | — | 115 |
| `Statement.Grant` | Security | B | converted 108 | — | 108 |
| `Statement.ExternalFileFormatDefinition` | External | C | converted 97, gap 4 | 4: no target record: a stray comma in a bracketed list of the PolyBase objects, which the tree keeps nothing for (4) | 101 |
| `Statement.FullTextIndexDefinition` | Tables | C | converted 99, gap 1 | 1: needs decision: inventory Statement.FullTextIndexDefinition ((a, --c b) KEY INDEX ix) (1) | 100 |
| `Statement.AlterFullTextIndex` | Tables | C | converted 92 | — | 92 |
| `Statement.ExternalDataSourceDefinition` | External | C | converted 85, gap 7 | 7: no target record: a stray comma in a bracketed list of the PolyBase objects, which the tree keeps nothing for (7) | 92 |
| `Statement.ResourcePoolDefinition` | Server | C | converted 86 | — | 86 |
| `Statement.ViewDefinition` | Tables | B | converted 84 | — | 84 |
| `Statement.ExternalResourcePoolDefinition` | External | C | converted 79 | — | 79 |
| `Statement.SetVariable` | Procedural | C | converted 78 | — | 78 |
| `Statement.CreateTrigger` | Routines | B | converted 76 | — | 76 |
| `Statement.Dbcc` | Server | C | converted 73 | — | 73 |
| `Statement.AlterServerConfiguration` | Server | C | converted 68 | — | 68 |
| `Statement.Delete` | Dml | A | converted 68 | — | 68 |
| `Statement.CreateTableAsSelect` | Tables | B | converted 64 | — | 64 |
| `Statement.ExternalLibraryDefinition` | External | C | converted 59 | — | 59 |
| `Statement.CreateLogin` | Security | C | converted 57 | — | 57 |
| `Statement.UpdateText` | Dml | C | converted 57 | — | 57 |
| `Statement.DatabaseAuditSpecificationDefinition` | Monitoring | C | converted 56 | — | 56 |
| `Statement.AvailabilityGroupDefinition` | Server | C | converted 51 | — | 51 |
| `Statement.Merge` | Dml | A | converted 48 | — | 49 |
| `Statement.QueueDefinition` | Broker | C | converted 48 | — | 48 |
| `Statement.DropIndex` | Drop | C | converted 47 | — | 47 |
| `Statement.RestoreDatabase` | Database | C | converted 47 | — | 47 |
| `Statement.SequenceDefinition` | Tables | A | converted 46, gap 1 | 1: needs decision: inventory cross-cutting gaps, identifier styles (a name part ` TestSeq`) (1) | 47 |
| `Statement.AlterFullTextStopList` | Database | C | converted 46 | — | 46 |
| `Statement.AuditSpecificationDefinition` | Monitoring | C | converted 45 | — | 45 |
| `Statement.AssemblyDefinition` | Routines | C | converted 44 | — | 44 |
| `Statement.StatisticsDefinition` | Tables | C | converted 44 | — | 44 |
| `Statement.TriggerSwitch` | Routines | C | converted 44 | — | 44 |
| `Statement.AsymmetricKeyDefinition` | Security | C | converted 43 | — | 43 |
| `Statement.ExternalLanguageDefinition` | External | C | converted 42 | — | 42 |
| `Statement.UpdateStatistics` | Tables | C | converted 41 | — | 41 |
| `Statement.BackupDatabase` | Database | C | converted 38 | — | 38 |
| `Statement.Print` | Procedural | C | converted 38 | — | 38 |
| `Statement.SecurityPolicyDefinition` | Security | C | converted 38 | — | 38 |
| `Statement.CreateUser` | Security | C | converted 37 | — | 37 |
| `Statement.Revoke` | Security | B | converted 36 | — | 36 |
| `Statement.WriteText` | Dml | C | converted 35 | — | 35 |
| `Statement.Deny` | Security | C | converted 34 | — | 34 |
| `Statement.ExternalModelDefinition` | External | C | converted 34 | — | 34 |
| `Statement.Fetch` | Procedural | A | converted 34 | — | 34 |
| `Statement.AlterDatabaseModify` | Database | C | converted 31 | — | 31 |
| `Statement.AlterDatabaseModifyFileGroup` | Database | C | converted 30 | — | 30 |
| `Statement.Label` | Procedural | C | converted 30 | — | 30 |
| `Statement.Send` | Broker | C | converted 30 | — | 30 |
| `Statement.AlterResourceGovernor` | Server | C | converted 29 | — | 29 |
| `Statement.SynonymDefinition` | Tables | C | converted 29 | — | 29 |
| `Statement.Kill` | Server | C | converted 28 | — | 28 |
| `Statement.EndConversation` | Broker | C | converted 27 | — | 27 |
| `Statement.SchemaDefinition` | Security | A | converted 27 | — | 27 |
| `Statement.SymmetricKeyDefinition` | Security | C | converted 27 | — | 27 |
| `Statement.DeclareCursor` | Procedural | B | converted 22, gap 4 | 4: not kept by the old tree: Clause.CursorDefinition (the ISO form's words in another order, or twice) (2); needs decision: inventory cross-cutting gaps, identifier styles (a qualified name `dbo.t.a` where one name stands) (1) | 26 |
| `Statement.CursorAction` | Procedural | A | converted 25 | — | 25 |
| `Statement.Truncate` | Dml | A | converted 25 | — | 25 |
| `Statement.DropProcedure` | Drop | B | converted 24 | — | 24 |
| `Statement.ReadText` | Dml | C | converted 24 | — | 24 |
| `Statement.BeginDialog` | Broker | C | converted 23 | — | 23 |
| `Statement.BrokerPriorityDefinition` | Broker | C | converted 23 | — | 23 |
| `Statement.If` | Procedural | C | converted 23 | — | 23 |
| `Statement.OpenSymmetricKey` | Security | C | converted 22 | — | 22 |
| `Statement.AlterAsymmetricKey` | Security | C | converted 21 | — | 21 |
| `Statement.ExecuteAs` | Procedural | C | converted 21 | — | 21 |
| `Statement.PartitionFunctionDefinition` | Database | C | converted 21 | — | 21 |
| `Statement.Classification` | Security | C | converted 19, gap 1 | 1: needs decision: inventory Statement.Classification (`ADD SENSITIVITY CLASSIFICATION TO t.c WITH (LABEL = 'x') ; ;` after the name) (1) | 20 |
| `Statement.CredentialDefinition` | Security | C | converted 20 | — | 20 |
| `Statement.FullTextCatalogDefinition` | Database | C | converted 20 | — | 20 |
| `Statement.RuleDefinition` | Routines | C | converted 20 | — | 20 |
| `Statement.WaitFor` | Procedural | C | converted 20 | — | 20 |
| `Statement.CertificateDefinition` | Security | C | converted 19 | — | 19 |
| `Statement.DropDatabase` | Drop | C | converted 19 | — | 19 |
| `Statement.DropTrigger` | Drop | B | converted 19 | — | 19 |
| `Statement.FullTextStopListDefinition` | Database | C | converted 19 | — | 19 |
| `Statement.AggregateDefinition` | Routines | C | converted 18 | — | 18 |
| `Statement.AlterLogin` | Security | C | converted 18 | — | 18 |
| `Statement.DropAssembly` | Drop | C | converted 18 | — | 18 |
| `Statement.MessageTypeDefinition` | Broker | C | converted 18 | — | 18 |
| `Statement.ServiceDefinition` | Broker | C | converted 18 | — | 18 |
| `Statement.AlterPartitionFunction` | Database | C | converted 17 | — | 17 |
| `Statement.BackupCertificate` | Database | C | converted 17 | — | 17 |
| `Statement.Compound` | Procedural | C | converted 17 | — | 17 |
| `Statement.EventNotificationDefinition` | Monitoring | C | converted 17 | — | 17 |
| `Statement.RemoteServiceBindingDefinition` | Broker | C | converted 17 | — | 17 |
| `Statement.AddSignature` | Security | C | converted 16 | — | 16 |
| `Statement.AlterMasterKey` | Security | C | converted 16 | — | 16 |
| `Statement.DefaultDefinition` | Routines | C | converted 16 | — | 16 |
| `Statement.DropFunction` | Drop | B | converted 16 | — | 16 |
| `Statement.DropTable` | Drop | B | converted 16 | — | 16 |
| `Statement.TryCatch` | Procedural | C | converted 16 | — | 16 |
| `Statement.BackupTransactionLog` | Database | C | converted 15 | — | 15 |
| `Statement.DropAggregate` | Drop | C | converted 15 | — | 15 |
| `Statement.DropView` | Drop | B | converted 15 | — | 15 |
| `Statement.AlterCertificate` | Security | C | converted 14 | — | 14 |
| `Statement.DropSensitivityClassification` | Drop | C | converted 14 | — | 14 |
| `Statement.DropSequence` | Drop | B | converted 14 | — | 14 |
| `Statement.DropType` | Drop | B | converted 14 | — | 14 |
| `Statement.RestoreLog` | Database | C | converted 14 | — | 14 |
| `Statement.AlterDatabaseScopedConfiguration` | Database | C | converted 13 | — | 13 |
| `Statement.AlterFullTextCatalog` | Database | C | converted 13 | — | 13 |
| `Statement.AlterServiceMasterKey` | Security | C | converted 13 | — | 13 |
| `Statement.AlterUser` | Security | C | converted 13 | — | 13 |
| `Statement.CryptographicProviderDefinition` | Security | C | converted 13 | — | 13 |
| `Statement.DropDefault` | Drop | C | converted 13 | — | 13 |
| `Statement.DropRule` | Drop | C | converted 13 | — | 13 |
| `Statement.DropSynonym` | Drop | C | converted 13 | — | 13 |
| `Statement.RaiseError` | Procedural | C | converted 13 | — | 13 |
| `Statement.Receive` | Broker | C | converted 13 | — | 13 |
| `Statement.SetUser` | Procedural | C | converted 13 | — | 13 |
| `Statement.DropSignature` | Drop | C | converted 12 | — | 12 |
| `Statement.DropStatistics` | Drop | C | converted 12 | — | 12 |
| `Statement.AlterAuthorization` | Security | C | converted 11 | — | 11 |
| `Statement.AlterDatabaseRebuildLog` | Database | C | converted 11 | — | 11 |
| `Statement.AlterSchema` | Security | C | converted 11 | — | 11 |
| `Statement.AlterSearchPropertyList` | Database | C | converted 11 | — | 11 |
| `Statement.DropEventNotification` | Drop | C | converted 11 | — | 11 |
| `Statement.DropQueue` | Drop | C | converted 11 | — | 11 |
| `Statement.DropSecurityPolicy` | Drop | C | converted 11 | — | 11 |
| `Statement.PartitionSchemeDefinition` | Database | C | converted 11 | — | 11 |
| `Statement.Revert` | Procedural | C | converted 11 | — | 11 |
| `Statement.XmlSchemaCollectionDefinition` | Routines | C | converted 11 | — | 11 |
| `Statement.AlterPartitionScheme` | Database | C | converted 10 | — | 10 |
| `Statement.DatabaseScopedCredentialDefinition` | Security | C | converted 10 | — | 10 |
| `Statement.DropEventSession` | Drop | C | converted 10 | — | 10 |
| `Statement.WaitForStatement` | Broker | C | converted 10 | — | 10 |
| `Statement.AlterDatabaseCollate` | Database | C | converted 9 | — | 9 |
| `Statement.DropXmlSchemaCollection` | Drop | C | converted 9 | — | 9 |
| `Statement.MasterKeyDefinition` | Security | C | converted 9 | — | 9 |
| `Statement.Reconfigure` | Server | C | converted 9 | — | 9 |
| `Statement.Use` | Procedural | C | converted 9 | — | 9 |
| `Statement.AlterDatabaseAddFileGroup` | Database | C | converted 8 | — | 8 |
| `Statement.AlterDatabaseAddLogFile` | Database | C | converted 8 | — | 8 |
| `Statement.AlterRole` | Security | C | converted 8 | — | 8 |
| `Statement.AlterXmlSchemaCollection` | Routines | C | converted 8 | — | 8 |
| `Statement.ColumnMasterKeyDefinition` | Security | C | converted 8 | — | 8 |
| `Statement.DatabaseEncryptionKeyDefinition` | Security | C | converted 8 | — | 8 |
| `Statement.DropExternalLanguage` | Drop | C | converted 8 | — | 8 |
| `Statement.DropExternalLibrary` | Drop | C | converted 8 | — | 8 |
| `Statement.DropRole` | Drop | B | converted 8 | — | 8 |
| `Statement.SearchPropertyListDefinition` | Database | C | converted 7, gap 1 | 1: needs decision: inventory Statement.SearchPropertyListDefinition (AUTHORIZATION --c a) (1) | 8 |
| `Statement.AlterDatabaseAddFile` | Database | C | converted 7 | — | 7 |
| `Statement.AlterServerRole` | Security | C | converted 7 | — | 7 |
| `Statement.Checkpoint` | Database | C | converted 7 | — | 7 |
| `Statement.DropAsymmetricKey` | Drop | C | converted 7 | — | 7 |
| `Statement.DropContract` | Drop | C | converted 7 | — | 7 |
| `Statement.DropMessageType` | Drop | C | converted 7 | — | 7 |
| `Statement.DropRemoteServiceBinding` | Drop | C | converted 7 | — | 7 |
| `Statement.DropRoute` | Drop | C | converted 7 | — | 7 |
| `Statement.DropService` | Drop | C | converted 7 | — | 7 |
| `Statement.DropSymmetricKey` | Drop | C | converted 7 | — | 7 |
| `Statement.RestoreMasterKey` | Database | C | converted 7 | — | 7 |
| `Statement.RouteDefinition` | Broker | C | converted 7 | — | 7 |
| `Statement.Throw` | Procedural | C | converted 7 | — | 7 |
| `Statement.AlterSymmetricKey` | Security | C | converted 6 | — | 6 |
| `Statement.ColumnEncryptionKeyDefinition` | Security | C | converted 6 | — | 6 |
| `Statement.ContractDefinition` | Broker | C | converted 6 | — | 6 |
| `Statement.CreateApplicationRole` | Security | C | converted 6 | — | 6 |
| `Statement.CreateRole` | Security | A | converted 6 | — | 6 |
| `Statement.DropDatabaseAuditSpecification` | Drop | C | converted 6 | — | 6 |
| `Statement.DropFulltextIndex` | Drop | C | converted 6 | — | 6 |
| `Statement.DropSchema` | Drop | B | converted 6 | — | 6 |
| `Statement.DropServerAudit` | Drop | C | converted 6 | — | 6 |
| `Statement.DropServerAuditSpecification` | Drop | C | converted 6 | — | 6 |
| `Statement.DropUser` | Drop | C | converted 6 | — | 6 |
| `Statement.DropWorkloadClassifier` | Drop | C | converted 6 | — | 6 |
| `Statement.RestoreServiceMasterKey` | Database | C | converted 6 | — | 6 |
| `Statement.SetCursor` | Procedural | C | converted 6 | — | 6 |
| `Statement.Shutdown` | Server | C | converted 6 | — | 6 |
| `Statement.While` | Procedural | C | converted 6 | — | 6 |
| `Statement.AlterColumnEncryptionKey` | Security | C | converted 5 | — | 5 |
| `Statement.AlterDatabaseModifyFile` | Database | C | converted 5 | — | 5 |
| `Statement.CloseSymmetricKey` | Security | C | converted 5 | — | 5 |
| `Statement.ConversationTimer` | Broker | C | converted 5 | — | 5 |
| `Statement.CreateServerRole` | Security | C | converted 5 | — | 5 |
| `Statement.DropExternalTable` | Drop | C | converted 5 | — | 5 |
| `Statement.MoveConversation` | Broker | C | converted 5 | — | 5 |
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
| `Statement.GetConversationGroup` | Broker | C | converted 4 | — | 4 |
| `Statement.OpenMasterKey` | Security | C | converted 4 | — | 4 |
| `Statement.RestoreRewindOnly` | Database | C | converted 4 | — | 4 |
| `Statement.AlterDatabaseRemoveFile` | Database | C | converted 3 | — | 3 |
| `Statement.BackupServiceMasterKey` | Database | C | converted 3 | — | 3 |
| `Statement.Break` | Procedural | C | converted 3 | — | 3 |
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
| `Statement.GoTo` | Procedural | C | converted 3 | — | 3 |
| `Statement.LineNumber` | Procedural | C | converted 3 | — | 3 |
| `Statement.RestoreFileListOnly` | Database | C | converted 3 | — | 3 |
| `Statement.RestoreHeaderOnly` | Database | C | converted 3 | — | 3 |
| `Statement.RestoreVerifyOnly` | Database | C | converted 3 | — | 3 |
| `Statement.Return` | Procedural | A | converted 3 | — | 3 |
| `Statement.AlterDatabaseModifyBackupStorageRedundancy` | Database | C | converted 2 | — | 2 |
| `Statement.AlterDatabasePerformCutover` | Database | C | converted 2 | — | 2 |
| `Statement.AlterDatabaseRemoveFileGroup` | Database | C | converted 2 | — | 2 |
| `Statement.Continue` | Procedural | C | converted 2 | — | 2 |
| `Statement.DropExternalDataSource` | Drop | C | converted 2 | — | 2 |
| `Statement.DropExternalFileFormat` | Drop | C | converted 2 | — | 2 |
| `Statement.DropExternalResourcePool` | Drop | C | converted 2 | — | 2 |
| `Statement.RestoreLabelOnly` | Database | C | converted 2 | — | 2 |
| `Statement.RestoreSymmetricKey` | Database | C | converted 2 | — | 2 |
| `Statement.BackupGroup` | Database | C | converted 1 | — | 1 |
| `Statement.BackupSymmetricKey` | Database | C | converted 1 | — | 1 |
| `Clause.ColumnDefinition` | Tables | B | gap 22 | 22: needs decision: inventory DataType (a type `national text` of several words) (4); needs decision: inventory cross-cutting gaps, identifier styles (a name part `national sys`) (4) | 0 |
| `Clause.ColumnOption` | Tables | B | — | — | 0 |
| `Clause.CommonTableExpression` | Queries | A | — | — | 0 |
| `Clause.Connection` | Tables | C | — | — | 0 |
| `Clause.ConstraintDefinition` | Tables | B | — | — | 0 |
| `Clause.CursorDefinition` | Procedural | B | — | — | 0 |
| `Clause.CursorFor` | Procedural | A | — | — | 0 |
| `Clause.DatabaseFile` | Database | C | — | — | 0 |
| `Clause.DerivedColumn` | Queries | A | — | — | 0 |
| `Clause.Dropped` | Tables | B | — | — | 0 |
| `Clause.EventPiece` | Monitoring | C | — | — | 0 |
| `Clause.ExecutionContext` | Routines | C | — | — | 0 |
| `Clause.FileGroup` | Database | C | — | — | 0 |
| `Clause.For` | Queries | C | — | — | 0 |
| `Clause.GroupBy` | Queries | B | — | — | 0 |
| `Clause.Hint` | Queries | C | gap 5 | 5: needs decision: inventory Clause.Hint (the old WITH (n)) (4); needs decision: inventory Clause.Hint (a hint of a shape the family does not hold) (1) | 0 |
| `Clause.Into` | Queries | C | — | — | 0 |
| `Clause.JsonColumn` | Tables | C | gap 1 | 1: no target record: RowsetColumn.Ordinal is a number, and keeps no spelling of it such as `00` (1) | 0 |
| `Clause.MergeWhen` | Dml | B | — | — | 0 |
| `Clause.Option` | Tables | C | — | — | 0 |
| `Clause.OrderBy` | Queries | B | — | — | 0 |
| `Clause.Output` | Dml | C | — | — | 0 |
| `Clause.ParameterDeclaration` | Routines | B | — | — | 0 |
| `Clause.PartitionRange` | Dml | C | — | — | 0 |
| `Clause.Placement` | Tables | C | — | — | 0 |
| `Clause.QualifiedAsterisk` | Queries | A | — | — | 0 |
| `Clause.References` | Tables | A | — | — | 0 |
| `Clause.Set` | Dml | B | — | — | 0 |
| `Clause.SortSpecification` | Queries | A | — | — | 0 |
| `Clause.SystemTime` | Tables | A | — | — | 0 |
| `Clause.TableSample` | Tables | A | — | — | 0 |
| `Clause.Top` | Queries | C | — | — | 0 |
| `Clause.VariableAssignment` | Queries | C | — | — | 0 |
| `Clause.VariableDeclaration` | Procedural | C | gap 9 | 9: needs decision: inventory DataType (a type `"ncharacter" varying` of several words) (1); needs decision: inventory DataType (a type `[ncharacter] varying` of several words) (1) | 0 |
| `Clause.When` | Values | A | — | — | 0 |
| `Clause.Window` | Values | A | — | — | 0 |
| `Clause.WindowDefinition` | Values | A | — | — | 0 |
| `Expression.Add` | Values | A | — | — | 0 |
| `Expression.Aliased` | Values | A | — | — | 0 |
| `Expression.And` | Values | A | — | — | 0 |
| `Expression.Between` | Values | A | — | — | 0 |
| `Expression.BitwiseAnd` | Values | C | — | — | 0 |
| `Expression.BitwiseNot` | Values | C | — | — | 0 |
| `Expression.BitwiseOr` | Values | C | — | — | 0 |
| `Expression.BitwiseXor` | Values | C | — | — | 0 |
| `Expression.Case` | Values | A | — | — | 0 |
| `Expression.Collated` | Values | A | — | — | 0 |
| `Expression.ColumnReference` | Values | B | — | — | 0 |
| `Expression.Comparison` | Values | A | — | — | 0 |
| `Expression.Concatenate` | Values | A | — | — | 0 |
| `Expression.Divide` | Values | A | — | — | 0 |
| `Expression.Exists` | Values | A | — | — | 0 |
| `Expression.GraphMatch` | Values | C | — | — | 0 |
| `Expression.Hinted` | Values | C | — | — | 0 |
| `Expression.In` | Values | A | — | — | 0 |
| `Expression.IsDistinctFrom` | Values | A | — | — | 0 |
| `Expression.IsNull` | Values | A | — | — | 0 |
| `Expression.IsTruth` | Values | A | — | — | 0 |
| `Expression.JsonPair` | Values | A | — | — | 0 |
| `Expression.Like` | Values | A | — | — | 0 |
| `Expression.Literal` | Values | B | — | — | 0 |
| `Expression.Match` | Values | A | — | — | 0 |
| `Expression.Measured` | Values | C | — | — | 0 |
| `Expression.Member` | Values | A | — | — | 0 |
| `Expression.Modulo` | Values | C | — | — | 0 |
| `Expression.Multiply` | Values | A | — | — | 0 |
| `Expression.NamedArgument` | Values | A | — | — | 0 |
| `Expression.Negate` | Values | A | — | — | 0 |
| `Expression.Not` | Values | A | — | — | 0 |
| `Expression.OdbcEscape` | Values | C | — | — | 0 |
| `Expression.Or` | Values | A | — | — | 0 |
| `Expression.Overlaps` | Values | A | — | — | 0 |
| `Expression.Parenthesized` | Values | A | — | — | 0 |
| `Expression.Pieced` | Values | C | — | — | 0 |
| `Expression.Plus` | Values | A | — | — | 0 |
| `Expression.Prefixed` | Values | C | — | — | 0 |
| `Expression.Quantified` | Values | A | — | — | 0 |
| `Expression.RoutineInvocation` | Values | B | gap 8 | 8: needs decision: inventory Expression.RoutineInvocation (JSON_ARRAYAGG of other than one value, where the record holds one) (2); needs decision: inventory DataType (`VECTOR(3, 4)`: a vector of other than dimensions, MAX and a base type of the page's) (1) | 0 |
| `Expression.RowValueConstructor` | Values | A | — | — | 0 |
| `Expression.RowsetOrder` | Values | C | — | — | 0 |
| `Expression.ShiftLeft` | Values | C | — | — | 0 |
| `Expression.ShiftRight` | Values | C | — | — | 0 |
| `Expression.Subquery` | Values | A | — | — | 0 |
| `Expression.Subtract` | Values | A | — | — | 0 |
| `Expression.Unique` | Values | A | — | — | 0 |
| `Expression.WindowFunction` | Values | B | gap 4 | 4: needs decision: inventory Expression.WindowFunction (a window over something that is not a call) (3); needs decision: inventory Expression.WindowFunction (a JSON aggregate with WITHIN GROUP or a null treatment, which its record has no place for) (1) | 0 |
| `Query.Built` | Built |  | — | — | 0 |
| `Query.DefaultValues` | Dml | A | — | — | 0 |
| `Query.Except` | Queries | B | — | — | 0 |
| `Query.ExplicitTable` | Queries | A | — | — | 0 |
| `Query.FromExecute` | Routines | C | — | — | 0 |
| `Query.FromFile` | Dml | C | — | — | 0 |
| `Query.FromStream` | Dml | C | — | — | 0 |
| `Query.Intersect` | Queries | B | — | — | 0 |
| `Query.Ordered` | Queries | B | — | — | 0 |
| `Query.Parenthesized` | Queries | B | — | — | 0 |
| `Query.Specification` | Queries | B | — | — | 0 |
| `Query.TableValueConstructor` | Queries | A | — | — | 0 |
| `Query.Union` | Queries | B | — | — | 0 |
| `SetExpression.DateAndTime.DateFirst` | Procedural | C | — | — | 0 |
| `SetExpression.DateAndTime.DateFormat` | Procedural | C | — | — | 0 |
| `SetExpression.IsoSettings.Switch` | Procedural | C | — | — | 0 |
| `SetExpression.Locking.DeadlockPriority` | Procedural | C | — | — | 0 |
| `SetExpression.Locking.LockTimeout` | Procedural | C | — | — | 0 |
| `SetExpression.Miscellaneous.ContextInfo` | Procedural | C | — | — | 0 |
| `SetExpression.Miscellaneous.ErrorLevel` | Procedural | C | — | — | 0 |
| `SetExpression.Miscellaneous.FipsFlagger` | Procedural | C | — | — | 0 |
| `SetExpression.Miscellaneous.IdentityInsert` | Procedural | C | — | — | 0 |
| `SetExpression.Miscellaneous.Language` | Procedural | C | — | — | 0 |
| `SetExpression.Miscellaneous.Offset` | Procedural | C | — | — | 0 |
| `SetExpression.Miscellaneous.Switch` | Procedural | C | — | — | 0 |
| `SetExpression.QueryExecution.QueryGovernorCostLimit` | Procedural | C | — | — | 0 |
| `SetExpression.QueryExecution.RowCount` | Procedural | C | — | — | 0 |
| `SetExpression.QueryExecution.Switch` | Procedural | C | — | — | 0 |
| `SetExpression.QueryExecution.TextSize` | Procedural | C | — | — | 0 |
| `SetExpression.Statistics.Report` | Procedural | C | — | — | 0 |
| `SetExpression.Statistics.Switch` | Procedural | C | — | — | 0 |
| `SetExpression.Transactions.IsolationLevel` | Procedural | A | — | — | 0 |
| `SetExpression.Transactions.Switch` | Procedural | C | — | — | 0 |
| `Statement.BackupServer` | Database | C | — | — | 0 |
| `Statement.Built` | Built |  | — | — | 0 |
| `TableReference.Built` | Built |  | — | — | 0 |
| `TableReference.Changed` | Dml | A | — | — | 0 |
| `TableReference.Derived` | Tables | A | — | — | 0 |
| `TableReference.FunctionCall` | Tables | A | gap 2 | 2: needs decision: inventory TableReference.FunctionCall (a PREDICT that is not MODEL, DATA AS and a schema) (2) | 0 |
| `TableReference.Joined` | Tables | A | — | — | 0 |
| `TableReference.Named` | Tables | A | gap 8 | 8: needs decision: inventory Clause.Hint (a hint written against the sample) (4); needs decision: inventory cross-cutting gaps, identifier styles (a name part `'Something'` that is no name) (3) | 0 |
| `TableReference.OdbcJoin` | Tables | C | — | — | 0 |
| `TableReference.Parenthesized` | Tables | A | — | — | 0 |
| `TableReference.Pivot` | Tables | C | — | — | 0 |
| `TableReference.Unpivot` | Tables | C | — | — | 0 |
