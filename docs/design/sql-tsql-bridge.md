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
| `Expression.ColumnReference` | Values | B | gap 13: needs decision: inventory Expression.ColumnReference (`t.*` standing where a value does) | 6170 |
| `Expression.Literal` | Values | B | gap 153: no target record: Expression.Variable for `@x` and `@@x` (decided 2026-09-15, not declared); no target record: LiteralValue.Binary has no flag for T-SQL's `0x` spelling (proposal 11) | 5490 |
| `Clause.Option` | Tables | C | gap 9: no target record: WithClause has no list for WITH XMLNAMESPACES (proposal 1) | 3877 |
| `Query.Specification` | Queries | B | gap 41: no target record: Query.Select has no member for SelectIntoTable (proposal 1) | 2032 |
| `Clause.DerivedColumn` | Queries | A | gap 20: needs decision: inventory Clause.DerivedColumn (a string alias); needs decision: inventory Clause.DerivedColumn (alias = expression) | 1931 |
| `TableReference.Named` | Tables | A | gap 120: no target record: TableSource.Named has no table hints (proposal 9); no target record: TableSource.Named has no OPENDATASOURCE server (inventory TableReference.Named) | 1842 |
| `Statement.Select` | Queries | B | converted 941, unread 3 | 1811 |
| `Clause.ColumnDefinition` | Tables | B | not seen | 1702 |
| `Expression.RoutineInvocation` | Values | B | gap 267: needs decision: inventory Expression.JsonPair (T-SQL's JSON functions as the standard's JSON nodes); no target record: a typed node for CONVERT (proposal 12) | 1637 |
| `Clause.ColumnOption` | Tables | B | not seen | 1076 |
| `Clause.ConstraintDefinition` | Tables | B | not seen | 1026 |
| `Clause.SortSpecification` | Queries | A | not seen | 986 |
| `Clause.QualifiedAsterisk` | Queries | A | not seen | 781 |
| `Statement.TableDefinition` | Tables | B | gap 1829: needs decision: inventory Statement.TableDefinition | 611 |
| `Expression.Comparison` | Values | A | not seen | 496 |
| `Clause.EventPiece` | Server | C | not seen | 368 |
| `Expression.NamedArgument` | Values | A | not seen | 352 |
| `Expression.Parenthesized` | Values | A | not seen | 346 |
| `TableReference.FunctionCall` | Tables | A | gap 380: no target record: a typed T-SQL rowset table source (proposal 7); target exists, no writer arm: TableSource.Function | 319 |
| `Statement.CreateIndex` | Tables | C | gap 495: no target record: Statement.CreateIndex | 294 |
| `Statement.AlterTable` | Tables | B | gap 644: needs decision: inventory Statement.AlterTable | 291 |
| `Clause.Connection` | Tables | C | not seen | 266 |
| `Clause.ParameterDeclaration` | Routines | B | not seen | 231 |
| `Clause.Hint` | Queries | C | gap 72: target exists, no writer arm: Statement.Select.Hints (QueryHint.Named) | 222 |
| `Statement.Compound` | Procedural | C | gap 17: no target record: Statement.Compound | 216 |
| `Statement.EventSessionDefinition` | Server | C | gap 323: no target record: Statement.CreateEventSession | 193 |
| `Statement.AlterDatabaseSet` | Database | C | gap 218: no target record: Statement.AlterDatabase | 190 |
| `Expression.Measured` | Values | C | not seen | 187 |
| `TableReference.Joined` | Tables | A | gap 89: target exists, no writer arm: JoinKind.CrossApply and JoinKind.OuterApply; target exists, no writer arm: TableSource.Join.Hint (JoinHint) | 167 |
| `Expression.Add` | Values | A | not seen | 158 |
| `Clause.VariableDeclaration` | Procedural | C | not seen | 150 |
| `Clause.Placement` | Tables | C | not seen | 148 |
| `Statement.CreateProcedure` | Routines | B | gap 279: needs decision: inventory Statement.CreateProcedure | 147 |
| `Expression.RowValueConstructor` | Values | A | not seen | 144 |
| `Statement.Declare` | Procedural | C | gap 300: no target record: Statement.DeclareVariables | 142 |
| `Clause.Window` | Values | A | gap 29: needs decision: inventory Clause.Window (the frame kept as text) | 140 |
| `Expression.WindowFunction` | Values | B | gap 48: needs decision: inventory Expression.WindowFunction (WITHIN GROUP kept as text) | 140 |
| `Statement.EndpointDefinition` | Server | C | gap 249: no target record: Statement.CreateEndpoint | 138 |
| `Clause.GroupBy` | Queries | B | gap 20: no target record: GroupByClause has no WITH CUBE or WITH ROLLUP (proposal 5); no target record: GroupByClause has no T-SQL ALL flag (proposal 5) | 137 |
| `Expression.Prefixed` | Values | C | gap 22: no target record: a typed T-SQL rowset table source (proposal 7) | 137 |
| `Statement.AlterIndex` | Tables | C | gap 148: no target record: Statement.AlterIndex | 132 |
| `Clause.OrderBy` | Queries | B | not seen | 130 |
| `Expression.Negate` | Values | A | not seen | 124 |
| `Statement.CreateFunction` | Routines | B | gap 181: needs decision: inventory Statement.CreateFunction | 121 |
| `Statement.SetStatement` | Procedural | C | converted 23, gap 131: no target record: Statement.SetOptions | 118 |
| `Statement.CreateDatabase` | Database | C | gap 154: no target record: Statement.CreateDatabase | 112 |
| `Expression.And` | Values | A | not seen | 111 |
| `Statement.Return` | Procedural | A | converted 1, different 2 | 111 |
| `Expression.GraphMatch` | Values | C | gap 80: no target record: Expression.GraphMatch, a graph pattern family (proposal 15) | 106 |
| `Clause.Set` | Dml | B | gap 45: no target record: Assignment has no compound operator (proposal 4); no target record: Assignment has no Through for `@v = column = value` (proposal 4) | 98 |
| `Statement.Update` | Dml | A | converted 31, gap 29: no target record: Statement.Update has no second FROM (proposal 2); no target record: Statement.Update has no OutputClause in the new tree (proposal 2) | 94 |
| `Statement.Insert` | Dml | A | converted 15, gap 49: no target record: Statement.Insert has no member for whether INTO was written (proposal 2); no target record: Statement.Insert has no OutputClause in the new tree (proposal 2) | 90 |
| `TableReference.Parenthesized` | Tables | A | not seen | 90 |
| `Statement.Transaction` | Procedural | B | converted 49, gap 60: target exists, no writer arm: Statement.BeginTransaction; target exists, no writer arm: Statement.SaveTransaction | 86 |
| `Statement.WorkloadGroupDefinition` | Server | C | gap 155: no target record: Statement.CreateWorkloadGroup | 84 |
| `Expression.Subquery` | Values | A | not seen | 80 |
| `Statement.Grant` | Security | B | gap 108: needs decision: inventory Statement.Grant | 80 |
| `Statement.ViewDefinition` | Tables | B | gap 84: needs decision: inventory Statement.ViewDefinition | 80 |
| `Expression.Member` | Values | A | not seen | 78 |
| `Expression.Hinted` | Values | C | gap 23: needs decision: inventory Expression.Hinted | 76 |
| `Clause.JsonColumn` | Tables | C | not seen | 73 |
| `TableReference.Derived` | Tables | A | gap 8: no target record: TableSource.Subquery has no FOR PATH (inventory TableReference.Derived) | 72 |
| `Statement.AlterFullTextIndex` | Tables | C | gap 92: no target record: Statement.AlterFullTextIndex | 68 |
| `Clause.For` | Queries | C | gap 74: target exists, no writer arm: Query.For (ForClause) | 64 |
| `Statement.SetVariable` | Procedural | C | gap 78: no target record: Statement.SetVariable | 62 |
| `Clause.DatabaseFile` | Database | C | not seen | 60 |
| `Clause.SystemTime` | Tables | A | gap 10: no target record: SystemTimeSpecification has no `ALL` (inventory Clause.SystemTime); no target record: SystemTimeSpecification has no `CONTAINED IN` (inventory Clause.SystemTime) | 60 |
| `Statement.ExternalResourcePoolDefinition` | Server | C | gap 79: no target record: Statement.CreateExternalResourcePool | 60 |
| `Clause.Dropped` | Tables | B | not seen | 58 |
| `Clause.Output` | Dml | C | not seen | 58 |
| `Statement.Delete` | Dml | A | converted 11, gap 45: no target record: Statement.Delete has no OutputClause in the new tree (proposal 2); no target record: Statement.Delete has no second FROM (proposal 2) | 58 |
| `Statement.FullTextIndexDefinition` | Tables | C | gap 99: no target record: Statement.CreateFullTextIndex | 56 |
| `Statement.CreateTrigger` | Routines | B | gap 62: needs decision: inventory Statement.CreateTrigger | 54 |
| `Statement.ExternalFileFormatDefinition` | Server | C | gap 101: no target record: Statement.CreateExternalFileFormat | 54 |
| `Clause.Top` | Queries | C | gap 7: needs decision: inventory Clause.Top (WITH APPROXIMATE) | 47 |
| `Expression.IsNull` | Values | A | not seen | 47 |
| `Statement.Execute` | Routines | B | gap 135: needs decision: inventory Statement.Execute | 46 |
| `Statement.ExternalDataSourceDefinition` | Server | C | gap 92: no target record: Statement.CreateExternalDataSource | 44 |
| `Statement.ResourcePoolDefinition` | Server | C | gap 86: no target record: Statement.CreateResourcePool | 44 |
| `Query.Parenthesized` | Queries | B | not seen | 41 |
| `Clause.MergeWhen` | Dml | B | gap 8: no target record: MergeClause.NotMatchedBySource and the BY TARGET flag (proposal 3) | 40 |
| `Expression.Collated` | Values | A | not seen | 40 |
| `Statement.CreateLogin` | Security | C | gap 57: no target record: Statement.CreateLogin | 40 |
| `Clause.References` | Tables | A | not seen | 39 |
| `Expression.Or` | Values | A | not seen | 38 |
| `Query.TableValueConstructor` | Queries | A | not seen | 38 |
| `Statement.CreateTableAsSelect` | Tables | B | gap 64: needs decision: inventory Statement.CreateTableAsSelect | 38 |
| `Statement.If` | Procedural | C | gap 23: no target record: Statement.If | 38 |
| `Expression.Not` | Values | A | not seen | 36 |
| `Query.FromFile` | Dml | C | gap 83: no target record: Statement.BulkInsert (inventory Query.FromFile) | 36 |
| `Statement.AsymmetricKeyDefinition` | Security | C | gap 43: no target record: Statement.CreateAsymmetricKey | 36 |
| `Statement.RestoreDatabase` | Database | C | gap 46: no target record: Statement.RestoreDatabase | 35 |
| `Clause.WindowDefinition` | Values | A | not seen | 34 |
| `Statement.Print` | Procedural | C | gap 38: no target record: Statement.Print | 34 |
| `Statement.StatisticsDefinition` | Tables | C | gap 43: no target record: Statement.CreateStatistics | 34 |
| `Expression.IsDistinctFrom` | Values | A | not seen | 33 |
| `Statement.AuditSpecificationDefinition` | Server | C | gap 45: no target record: Statement.CreateServerAuditSpecification | 32 |
| `Statement.DatabaseAuditSpecificationDefinition` | Database | C | gap 56: no target record: Statement.CreateDatabaseAuditSpecification | 32 |
| `Statement.DropIndex` | Drop | C | gap 47: target exists, no writer arm: Statement.DropIndex | 32 |
| `Statement.Deny` | Security | C | gap 34: needs decision: inventory Statement.Deny (target Statement.Deny declared, no arm yet) | 31 |
| `Statement.Revoke` | Security | B | gap 36: needs decision: inventory Statement.Revoke | 31 |
| `Clause.CommonTableExpression` | Queries | A | not seen | 30 |
| `Statement.Dbcc` | Server | C | gap 73: no target record: Statement.Dbcc | 30 |
| `Statement.Merge` | Dml | A | converted 9, gap 29: no target record: Statement.Merge has no member for whether INTO was written (proposal 2); no target record: Statement.Merge has no OutputClause in the new tree (proposal 2) | 30 |
| `Clause.Into` | Queries | C | not seen | 28 |
| `Expression.Multiply` | Values | A | not seen | 28 |
| `Statement.AlterDatabaseModifyFileGroup` | Database | C | gap 30: needs decision: inventory Statement.AlterDatabaseModifyFileGroup (target AlterDatabaseAction.ModifyFileGroup declared, no arm yet) | 28 |
| `Statement.ExternalLibraryDefinition` | Server | C | gap 59: no target record: Statement.CreateExternalLibrary | 28 |
| `Statement.UpdateStatistics` | Tables | C | gap 41: no target record: Statement.UpdateStatistics | 28 |
| `Expression.JsonPair` | Values | A | not seen | 27 |
| `Statement.AssemblyDefinition` | Routines | C | gap 44: no target record: Statement.CreateAssembly | 26 |
| `Statement.CreateUser` | Security | C | gap 37: no target record: Statement.CreateUser | 26 |
| `Statement.TypeDefinition` | Routines | B | gap 174: needs decision: inventory Statement.TypeDefinition | 26 |
| `Clause.TableSample` | Tables | A | gap 9: no target record: SampleClause.Method is required and T-SQL's SYSTEM may be left out (inventory Clause.TableSample); no target record: SampleClause has no unit, PERCENT or ROWS (inventory Clause.TableSample) | 24 |
| `Clause.When` | Values | A | not seen | 24 |
| `Expression.Case` | Values | A | not seen | 24 |
| `Expression.In` | Values | A | not seen | 24 |
| `SetExpression.Miscellaneous.Offset` | Procedural | C | not seen | 24 |
| `Statement.QueueDefinition` | Server | C | gap 48: no target record: Statement.CreateQueue | 24 |
| `Statement.ServerAuditDefinition` | Server | C | gap 118: no target record: Statement.CreateServerAudit | 24 |
| `Statement.SchemaDefinition` | Security | A | converted 12, gap 15: needs decision: inventory Statement.SchemaDefinition (elements kept as text) | 23 |
| `Clause.VariableAssignment` | Queries | C | gap 23: no target record: SelectColumn.VariableAssignment (inventory Clause.VariableAssignment) | 22 |
| `SetExpression.QueryExecution.Switch` | Procedural | C | not seen | 22 |
| `Statement.AlterDatabaseModify` | Database | C | gap 30: needs decision: inventory Statement.AlterDatabaseModify (target AlterDatabaseAction.Modify declared, no arm yet) | 22 |
| `Statement.BackupDatabase` | Database | C | gap 37: no target record: Statement.BackupDatabase | 22 |
| `Query.Union` | Queries | B | not seen | 21 |
| `Query.Ordered` | Queries | B | not seen | 20 |
| `SetExpression.IsoSettings.Switch` | Procedural | C | not seen | 20 |
| `SetExpression.Statistics.Report` | Procedural | C | not seen | 20 |
| `SetExpression.Transactions.IsolationLevel` | Procedural | A | not seen | 20 |
| `Statement.ExternalLanguageDefinition` | Server | C | gap 41: no target record: Statement.CreateExternalLanguage | 20 |
| `Statement.TriggerSwitch` | Routines | C | gap 44: no target record: Statement.EnableTrigger | 20 |
| `Statement.UpdateText` | Dml | C | gap 57: no target record: Statement.UpdateText | 20 |
| `Statement.Break` | Procedural | C | gap 3: no target record: Statement.Break | 19 |
| `Query.DefaultValues` | Dml | A | not seen | 18 |
| `Statement.AlterAsymmetricKey` | Security | C | gap 21: no target record: Statement.AlterAsymmetricKey | 18 |
| `Statement.BrokerPriorityDefinition` | Server | C | gap 23: no target record: Statement.CreateBrokerPriority | 18 |
| `Statement.ExternalModelDefinition` | Server | C | gap 34: no target record: Statement.CreateExternalModel | 18 |
| `SetExpression.Miscellaneous.Switch` | Procedural | C | not seen | 16 |
| `Statement.AlterFullTextStopList` | Database | C | gap 46: no target record: Statement.AlterFullTextStopList | 16 |
| `Statement.AlterServerConfiguration` | Server | C | gap 68: no target record: Statement.AlterServerConfiguration | 16 |
| `Statement.CredentialDefinition` | Security | C | gap 20: no target record: Statement.CreateCredential | 16 |
| `Statement.EventNotificationDefinition` | Server | C | gap 17: no target record: Statement.CreateEventNotification | 16 |
| `Statement.Fetch` | Procedural | A | refused 24 | 16 |
| `Statement.FullTextStopListDefinition` | Database | C | gap 19: no target record: Statement.CreateFullTextStopList | 16 |
| `Statement.Kill` | Server | C | gap 28: no target record: Statement.Kill | 16 |
| `Expression.Concatenate` | Values | A | not seen | 14 |
| `Statement.AggregateDefinition` | Routines | C | gap 18: no target record: Statement.CreateAggregate | 14 |
| `Statement.DropDatabase` | Drop | C | gap 19: target exists, no writer arm: Statement.DropObject | 14 |
| `Statement.RaiseError` | Procedural | C | gap 10: no target record: Statement.RaiseError | 14 |
| `Statement.SymmetricKeyDefinition` | Security | C | gap 27: no target record: Statement.CreateSymmetricKey | 14 |
| `Statement.Truncate` | Dml | A | converted 13, gap 12: no target record: Statement.TruncateTable has no partitions (PartitionRange is declared, no member holds it) | 14 |
| `Clause.FileGroup` | Database | C | not seen | 12 |
| `Expression.Divide` | Values | A | not seen | 12 |
| `Expression.Like` | Values | A | not seen | 12 |
| `Expression.Plus` | Values | A | not seen | 12 |
| `Statement.AlterMasterKey` | Security | C | gap 16: no target record: Statement.AlterMasterKey | 12 |
| `Statement.BackupTransactionLog` | Database | C | gap 15: no target record: Statement.BackupLog | 12 |
| `Statement.BeginDialog` | Server | C | gap 23: no target record: Statement.BeginDialog | 12 |
| `Statement.CertificateDefinition` | Security | C | gap 19: no target record: Statement.CreateCertificate | 12 |
| `Statement.Classification` | Security | C | gap 17: no target record: Statement.AddSensitivityClassification | 12 |
| `Statement.DropSensitivityClassification` | Drop | C | gap 14: target exists, no writer arm: Statement.DropSensitivityClassification | 12 |
| `Statement.ExecuteAs` | Procedural | C | gap 20: no target record: Statement.ExecuteAs | 12 |
| `Statement.FullTextCatalogDefinition` | Database | C | gap 20: no target record: Statement.CreateFullTextCatalog | 12 |
| `Statement.MessageTypeDefinition` | Server | C | gap 18: no target record: Statement.CreateMessageType | 12 |
| `Statement.OpenSymmetricKey` | Security | C | gap 22: no target record: Statement.OpenSymmetricKey | 12 |
| `Statement.RemoteServiceBindingDefinition` | Server | C | gap 17: no target record: Statement.CreateRemoteServiceBinding | 12 |
| `Statement.SequenceDefinition` | Routines | A | converted 28, gap 19: no target record: SequenceOption has no CACHE (inventory Statement.SequenceDefinition); needs decision: inventory cross-cutting gaps, identifier styles (a name part ` TestSeq`) | 12 |
| `Statement.ServiceDefinition` | Server | C | gap 18: no target record: Statement.CreateService | 12 |
| `Statement.WaitFor` | Procedural | C | gap 20: no target record: Statement.WaitFor | 12 |
| `Statement.WriteText` | Dml | C | gap 35: no target record: Statement.WriteText | 12 |
| `Statement.Send` | Server | C | gap 30: no target record: Statement.Send | 11 |
| `Clause.CursorDefinition` | Procedural | B | not seen | 10 |
| `Expression.RowsetOrder` | Values | C | not seen | 10 |
| `Statement.AlterCertificate` | Security | C | gap 14: no target record: Statement.AlterCertificate | 10 |
| `Statement.AlterFullTextCatalog` | Database | C | gap 13: no target record: Statement.AlterFullTextCatalog | 10 |
| `Statement.AlterResourceGovernor` | Server | C | gap 29: no target record: Statement.AlterResourceGovernor | 10 |
| `Statement.AlterUser` | Security | C | gap 13: no target record: Statement.AlterUser | 10 |
| `Statement.DropTrigger` | Drop | B | converted 14, different 5 | 10 |
| `Statement.RestoreLog` | Database | C | gap 14: no target record: Statement.RestoreLog | 10 |
| `TableReference.Pivot` | Tables | C | gap 76: no target record: TableSource.Pivot and TableSource.Unpivot (proposal 6) | 10 |
| `Statement.TryCatch` | Procedural | C | gap 16: no target record: Statement.TryCatch | 9 |
| `Clause.PartitionRange` | Dml | C | not seen | 8 |
| `Expression.Between` | Values | A | not seen | 8 |
| `Expression.BitwiseNot` | Values | C | not seen | 8 |
| `SetExpression.Statistics.Switch` | Procedural | C | not seen | 8 |
| `Statement.AlterPartitionFunction` | Database | C | gap 17: no target record: Statement.AlterPartitionFunction | 8 |
| `Statement.AlterSchema` | Security | C | gap 11: no target record: Statement.AlterSchema | 8 |
| `Statement.AlterSearchPropertyList` | Database | C | gap 11: no target record: Statement.AlterSearchPropertyList | 8 |
| `Statement.AlterServiceMasterKey` | Security | C | gap 13: no target record: Statement.AlterServiceMasterKey | 8 |
| `Statement.AvailabilityGroupDefinition` | Server | C | gap 51: no target record: Statement.CreateAvailabilityGroup | 8 |
| `Statement.CryptographicProviderDefinition` | Security | C | gap 13: no target record: Statement.CreateCryptographicProvider | 8 |
| `Statement.DatabaseScopedCredentialDefinition` | Security | C | gap 10: no target record: Statement.CreateDatabaseScopedCredential | 8 |
| `Statement.DeclareCursor` | Procedural | B | gap 26: needs decision: inventory Statement.DeclareCursor | 8 |
| `Statement.DropAggregate` | Drop | C | gap 15: target exists, no writer arm: Statement.DropObject | 8 |
| `Statement.DropAssembly` | Drop | C | gap 18: target exists, no writer arm: Statement.DropAssembly | 8 |
| `Statement.DropEventNotification` | Drop | C | gap 11: target exists, no writer arm: Statement.DropEventNotification | 8 |
| `Statement.DropFunction` | Drop | B | converted 11, unread 1, different 4 | 8 |
| `Statement.DropProcedure` | Drop | B | converted 17, unread 3, different 4 | 8 |
| `Statement.DropType` | Drop | B | converted 14 | 8 |
| `Statement.MasterKeyDefinition` | Security | C | gap 9: no target record: Statement.CreateMasterKey | 8 |
| `Statement.SetUser` | Procedural | C | gap 13: no target record: Statement.SetUser | 8 |
| `Statement.SynonymDefinition` | Routines | C | gap 29: no target record: Statement.CreateSynonym | 8 |
| `TableReference.Changed` | Tables | A | gap 25: needs decision: inventory TableReference.Changed | 8 |
| `Statement.BackupCertificate` | Database | C | gap 17: no target record: Statement.BackupCertificate | 7 |
| `Statement.EndConversation` | Server | C | gap 27: no target record: Statement.EndConversation | 7 |
| `Expression.OdbcEscape` | Values | C | gap 50: target exists, no writer arm: Expression.OdbcEscape | 6 |
| `Expression.ShiftLeft` | Values | C | not seen | 6 |
| `Expression.ShiftRight` | Values | C | not seen | 6 |
| `Expression.Subtract` | Values | A | not seen | 6 |
| `SetExpression.QueryExecution.TextSize` | Procedural | C | not seen | 6 |
| `Statement.AddSignature` | Security | C | gap 16: no target record: Statement.AddSignature | 6 |
| `Statement.AlterAuthorization` | Security | C | gap 11: no target record: Statement.AlterAuthorization | 6 |
| `Statement.AlterDatabaseAddFile` | Database | C | gap 7: needs decision: inventory Statement.AlterDatabaseAddFile (target AlterDatabaseAction.AddFile declared, no arm yet) | 6 |
| `Statement.AlterDatabaseAddFileGroup` | Database | C | gap 8: needs decision: inventory Statement.AlterDatabaseAddFileGroup (target AlterDatabaseAction.AddFileGroup declared, no arm yet) | 6 |
| `Statement.AlterDatabaseCollate` | Database | C | gap 9: needs decision: inventory Statement.AlterDatabaseCollate (target AlterDatabaseAction.Collate declared, no arm yet) | 6 |
| `Statement.AlterPartitionScheme` | Database | C | gap 10: no target record: Statement.AlterPartitionScheme | 6 |
| `Statement.AlterRole` | Security | C | gap 8: no target record: Statement.AlterRole | 6 |
| `Statement.AlterServerRole` | Security | C | gap 7: no target record: Statement.AlterServerRole | 6 |
| `Statement.ColumnMasterKeyDefinition` | Security | C | gap 8: no target record: Statement.CreateColumnMasterKey | 6 |
| `Statement.Continue` | Procedural | C | gap 2: no target record: Statement.Continue | 6 |
| `Statement.CursorAction` | Procedural | A | converted 16, gap 6: target exists, no writer arm: Statement.DeallocateCursor | 6 |
| `Statement.DefaultDefinition` | Routines | C | gap 16: no target record: Statement.CreateDefault | 6 |
| `Statement.DropDefault` | Drop | C | gap 13: target exists, no writer arm: Statement.DropObject | 6 |
| `Statement.DropEventSession` | Drop | C | gap 10: target exists, no writer arm: Statement.DropEventSession | 6 |
| `Statement.DropQueue` | Drop | C | gap 11: target exists, no writer arm: Statement.DropObject | 6 |
| `Statement.DropRole` | Drop | B | converted 8 | 6 |
| `Statement.DropRule` | Drop | C | gap 13: target exists, no writer arm: Statement.DropObject | 6 |
| `Statement.DropSequence` | Drop | B | converted 14 | 6 |
| `Statement.DropSynonym` | Drop | C | gap 13: target exists, no writer arm: Statement.DropObject | 6 |
| `Statement.DropTable` | Drop | B | converted 16 | 6 |
| `Statement.DropUser` | Drop | C | gap 6: target exists, no writer arm: Statement.DropObject | 6 |
| `Statement.DropView` | Drop | B | converted 15 | 6 |
| `Statement.Label` | Procedural | C | gap 30: no target record: Statement.Label | 6 |
| `Statement.ReadText` | Dml | C | gap 24: no target record: Statement.ReadText | 6 |
| `Statement.Revert` | Procedural | C | gap 11: no target record: Statement.Revert | 6 |
| `Statement.SearchPropertyListDefinition` | Database | C | gap 7: no target record: Statement.CreateSearchPropertyList | 6 |
| `Statement.Use` | Procedural | C | gap 7: no target record: Statement.Use | 6 |
| `TableReference.OdbcJoin` | Tables | C | gap 7: no target record: TableSource.OdbcJoin (proposal 15) | 6 |
| `TableReference.Unpivot` | Tables | C | gap 14: no target record: TableSource.Pivot and TableSource.Unpivot (proposal 6) | 6 |
| `Statement.RestoreMasterKey` | Database | C | gap 7: no target record: Statement.RestoreMasterKey | 5 |
| `Statement.RestoreServiceMasterKey` | Database | C | gap 6: no target record: Statement.RestoreServiceMasterKey | 5 |
| `Expression.Exists` | Values | A | not seen | 4 |
| `Query.Except` | Queries | B | not seen | 4 |
| `Query.Intersect` | Queries | B | not seen | 4 |
| `SetExpression.Miscellaneous.IdentityInsert` | Procedural | C | not seen | 4 |
| `SetExpression.QueryExecution.RowCount` | Procedural | C | not seen | 4 |
| `SetExpression.Transactions.Switch` | Procedural | C | not seen | 4 |
| `Statement.AlterColumnEncryptionKey` | Security | C | gap 5: no target record: Statement.AlterColumnEncryptionKey | 4 |
| `Statement.AlterDatabaseAddLogFile` | Database | C | gap 8: needs decision: inventory Statement.AlterDatabaseAddLogFile (target AlterDatabaseAction.AddLogFile declared, no arm yet) | 4 |
| `Statement.AlterDatabaseModifyFile` | Database | C | gap 5: needs decision: inventory Statement.AlterDatabaseModifyFile (target AlterDatabaseAction.ModifyFile declared, no arm yet) | 4 |
| `Statement.AlterDatabaseRebuildLog` | Database | C | gap 11: needs decision: inventory Statement.AlterDatabaseRebuildLog (target AlterDatabaseAction.RebuildLog declared, no arm yet) | 4 |
| `Statement.AlterDatabaseScopedConfiguration` | Database | C | gap 13: no target record: Statement.AlterDatabaseScopedConfiguration | 4 |
| `Statement.AlterLogin` | Security | C | gap 18: no target record: Statement.AlterLogin | 4 |
| `Statement.AlterSymmetricKey` | Security | C | gap 6: no target record: Statement.AlterSymmetricKey | 4 |
| `Statement.AlterXmlSchemaCollection` | Routines | C | gap 8: no target record: Statement.AlterXmlSchemaCollection | 4 |
| `Statement.Checkpoint` | Database | C | gap 7: no target record: Statement.Checkpoint | 4 |
| `Statement.ColumnEncryptionKeyDefinition` | Security | C | gap 6: no target record: Statement.CreateColumnEncryptionKey | 4 |
| `Statement.ContractDefinition` | Server | C | gap 6: no target record: Statement.CreateContract | 4 |
| `Statement.CreateApplicationRole` | Security | C | gap 6: no target record: Statement.CreateApplicationRole | 4 |
| `Statement.CreateRole` | Security | A | converted 3, gap 3: no target record: Statement.CreateRole has no AUTHORIZATION owner (inventory Statement.CreateRole) | 4 |
| `Statement.CreateServerRole` | Security | C | gap 5: no target record: Statement.CreateServerRole | 4 |
| `Statement.DatabaseEncryptionKeyDefinition` | Security | C | gap 8: no target record: Statement.CreateDatabaseEncryptionKey | 4 |
| `Statement.DropAsymmetricKey` | Drop | C | gap 7: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.DropDatabaseAuditSpecification` | Drop | C | gap 6: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.DropExternalLanguage` | Drop | C | gap 8: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.DropExternalLibrary` | Drop | C | gap 8: target exists, no writer arm: Statement.DropExternalLibrary | 4 |
| `Statement.DropFulltextIndex` | Drop | C | gap 6: target exists, no writer arm: Statement.DropFulltextIndex | 4 |
| `Statement.DropSchema` | Drop | B | converted 6 | 4 |
| `Statement.DropSecurityPolicy` | Drop | C | gap 11: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.DropServerAudit` | Drop | C | gap 6: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.DropServerAuditSpecification` | Drop | C | gap 6: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.DropSignature` | Drop | C | gap 12: target exists, no writer arm: Statement.DropSignature | 4 |
| `Statement.DropStatistics` | Drop | C | gap 12: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.DropSymmetricKey` | Drop | C | gap 7: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.DropXmlSchemaCollection` | Drop | C | gap 9: target exists, no writer arm: Statement.DropObject | 4 |
| `Statement.GetConversationGroup` | Server | C | gap 4: no target record: Statement.GetConversationGroup | 4 |
| `Statement.GoTo` | Procedural | C | gap 3: no target record: Statement.GoTo | 4 |
| `Statement.PartitionFunctionDefinition` | Database | C | gap 21: no target record: Statement.CreatePartitionFunction | 4 |
| `Statement.PartitionSchemeDefinition` | Database | C | gap 11: no target record: Statement.CreatePartitionScheme | 4 |
| `Statement.Reconfigure` | Server | C | gap 9: no target record: Statement.Reconfigure | 4 |
| `Statement.RestoreRewindOnly` | Database | C | gap 4: no target record: Statement.RestoreRewindOnly | 4 |
| `Statement.Shutdown` | Server | C | gap 6: no target record: Statement.Shutdown | 4 |
| `Statement.WaitForStatement` | Procedural | C | gap 10: no target record: Statement.WaitForReceive | 4 |
| `Statement.While` | Procedural | C | gap 6: no target record: Statement.While | 4 |
| `Statement.XmlSchemaCollectionDefinition` | Routines | C | gap 11: no target record: Statement.CreateXmlSchemaCollection | 4 |
| `Statement.BackupServiceMasterKey` | Database | C | gap 3: no target record: Statement.BackupServiceMasterKey | 3 |
| `Statement.RuleDefinition` | Routines | C | gap 20: no target record: Statement.CreateRule | 3 |
| `Statement.Throw` | Procedural | C | gap 5: no target record: Statement.Throw | 3 |
| `Clause.ExecutionContext` | Routines | C | not seen | 2 |
| `Expression.Modulo` | Values | C | not seen | 2 |
| `Query.FromExecute` | Dml | C | gap 5: no target record: InsertSource.Execute (inventory Query.FromExecute) | 2 |
| `SetExpression.Miscellaneous.ErrorLevel` | Procedural | C | not seen | 2 |
| `Statement.AlterApplicationRole` | Security | C | gap 4: no target record: Statement.AlterApplicationRole | 2 |
| `Statement.AlterDatabaseModifyName` | Database | C | gap 4: needs decision: inventory Statement.AlterDatabaseModifyName (target AlterDatabaseAction.ModifyName declared, no arm yet) | 2 |
| `Statement.AlterDatabasePerformCutover` | Database | C | gap 2: needs decision: inventory Statement.AlterDatabasePerformCutover (target AlterDatabaseAction.PerformCutover declared, no arm yet) | 2 |
| `Statement.AlterDatabaseRemoveFile` | Database | C | gap 3: needs decision: inventory Statement.AlterDatabaseRemoveFile (target AlterDatabaseAction.RemoveFile declared, no arm yet) | 2 |
| `Statement.AlterDatabaseRemoveFileGroup` | Database | C | gap 2: needs decision: inventory Statement.AlterDatabaseRemoveFileGroup (target AlterDatabaseAction.RemoveFileGroup declared, no arm yet) | 2 |
| `Statement.BackupMasterKey` | Database | C | gap 4: no target record: Statement.BackupMasterKey | 2 |
| `Statement.CloseAllSymmetricKeys` | Security | C | gap 3: no target record: Statement.CloseAllSymmetricKeys | 2 |
| `Statement.CloseMasterKey` | Security | C | gap 3: no target record: Statement.CloseMasterKey | 2 |
| `Statement.CloseSymmetricKey` | Security | C | gap 5: no target record: Statement.CloseSymmetricKey | 2 |
| `Statement.ConversationTimer` | Server | C | gap 5: no target record: Statement.BeginConversationTimer | 2 |
| `Statement.DropApplicationRole` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropAvailabilityGroup` | Drop | C | gap 4: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropBrokerPriority` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropCertificate` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropColumnEncryptionKey` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropColumnMasterKey` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropContract` | Drop | C | gap 7: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropCredential` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropCryptographicProvider` | Drop | C | gap 4: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropDatabaseEncryptionKey` | Drop | C | gap 3: target exists, no writer arm: Statement.DropDatabaseEncryptionKey | 2 |
| `Statement.DropDatabaseScopedCredential` | Drop | C | gap 4: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropEndpoint` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropExternalModel` | Drop | C | gap 4: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropFulltextCatalog` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropFulltextStoplist` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropLogin` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropMasterKey` | Drop | C | gap 3: target exists, no writer arm: Statement.DropMasterKey | 2 |
| `Statement.DropMessageType` | Drop | C | gap 7: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropPartitionFunction` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropPartitionScheme` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropRemoteServiceBinding` | Drop | C | gap 7: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropResourcePool` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropRoute` | Drop | C | gap 7: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropSearchPropertyList` | Drop | C | gap 4: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropServerRole` | Drop | C | gap 4: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropService` | Drop | C | gap 7: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.DropWorkloadGroup` | Drop | C | gap 3: target exists, no writer arm: Statement.DropObject | 2 |
| `Statement.LineNumber` | Procedural | C | gap 3: no target record: Statement.LineNumber | 2 |
| `Statement.MoveConversation` | Server | C | gap 5: no target record: Statement.MoveConversation | 2 |
| `Statement.OpenMasterKey` | Security | C | gap 4: no target record: Statement.OpenMasterKey | 2 |
| `Statement.Receive` | Server | C | gap 13: no target record: Statement.Receive | 2 |
| `Statement.RestoreFileListOnly` | Database | C | gap 3: no target record: Statement.RestoreFileListOnly | 2 |
| `Statement.RestoreLabelOnly` | Database | C | gap 2: no target record: Statement.RestoreLabelOnly | 2 |
| `Statement.RestoreVerifyOnly` | Database | C | gap 3: no target record: Statement.RestoreVerifyOnly | 2 |
| `Statement.RouteDefinition` | Server | C | gap 7: no target record: Statement.CreateRoute | 2 |
| `Statement.SetCursor` | Procedural | C | gap 6: target exists, no writer arm: Statement.SetCursorVariable | 2 |
| `Clause.CursorFor` | Procedural | A | not seen | 0 |
| `Expression.Aliased` | Values | A | not seen | 0 |
| `Expression.BitwiseAnd` | Values | C | not seen | 0 |
| `Expression.BitwiseOr` | Values | C | not seen | 0 |
| `Expression.BitwiseXor` | Values | C | not seen | 0 |
| `Expression.IsTruth` | Values | A | not seen | 0 |
| `Expression.Match` | Values | A | not seen | 0 |
| `Expression.Overlaps` | Values | A | not seen | 0 |
| `Expression.Pieced` | Values | C | not seen | 0 |
| `Expression.Quantified` | Values | A | not seen | 0 |
| `Expression.Unique` | Values | A | not seen | 0 |
| `Query.ExplicitTable` | Queries | A | not seen | 0 |
| `Query.FromStream` | Dml | C | gap 14: no target record: Statement.InsertBulk (inventory Query.FromStream) | 0 |
| `SetExpression.DateAndTime.DateFirst` | Procedural | C | not seen | 0 |
| `SetExpression.DateAndTime.DateFormat` | Procedural | C | not seen | 0 |
| `SetExpression.Locking.DeadlockPriority` | Procedural | C | not seen | 0 |
| `SetExpression.Locking.LockTimeout` | Procedural | C | not seen | 0 |
| `SetExpression.Miscellaneous.ContextInfo` | Procedural | C | not seen | 0 |
| `SetExpression.Miscellaneous.FipsFlagger` | Procedural | C | not seen | 0 |
| `SetExpression.Miscellaneous.Language` | Procedural | C | not seen | 0 |
| `SetExpression.QueryExecution.QueryGovernorCostLimit` | Procedural | C | not seen | 0 |
| `Statement.AlterDatabaseEncryptionKey` | Security | C | gap 4: no target record: Statement.AlterDatabaseEncryptionKey | 0 |
| `Statement.AlterDatabaseModifyBackupStorageRedundancy` | Database | C | gap 2: needs decision: inventory Statement.AlterDatabaseModifyBackupStorageRedundancy (target AlterDatabaseAction.ModifyBackupStorageRedundancy declared, no arm yet) | 0 |
| `Statement.BackupGroup` | Database | C | gap 1: no target record: Statement.BackupGroup | 0 |
| `Statement.BackupServer` | Database | C | not seen | 0 |
| `Statement.BackupSymmetricKey` | Database | C | gap 1: no target record: Statement.BackupSymmetricKey | 0 |
| `Statement.DropExternalDataSource` | Drop | C | gap 2: target exists, no writer arm: Statement.DropObject | 0 |
| `Statement.DropExternalFileFormat` | Drop | C | gap 2: target exists, no writer arm: Statement.DropObject | 0 |
| `Statement.DropExternalResourcePool` | Drop | C | gap 2: target exists, no writer arm: Statement.DropObject | 0 |
| `Statement.DropExternalTable` | Drop | C | gap 5: target exists, no writer arm: Statement.DropObject | 0 |
| `Statement.DropWorkloadClassifier` | Drop | C | gap 6: target exists, no writer arm: Statement.DropObject | 0 |
| `Statement.RestoreHeaderOnly` | Database | C | gap 3: no target record: Statement.RestoreHeaderOnly | 0 |
| `Statement.RestoreSymmetricKey` | Database | C | gap 2: no target record: Statement.RestoreSymmetricKey | 0 |
| `Statement.SecurityPolicyDefinition` | Security | C | gap 37: no target record: Statement.CreateSecurityPolicy | 0 |
