# T-SQL tree inventory: `SqlSyntax.cs` against `Sql2023Ast.cs`

Every nested record and every enum of `src/DotGram.Sql/SqlSyntax.cs` (namespace `DotGram.Sql`),
held against `src/DotGram.Sql/Standard/Sql2023Ast.cs` (namespace `DotGram.Sql.Ast`). Sources are
`docs/ast.md`'s. Both files were read in full; every target named below exists in `Sql2023Ast.cs`
under that name (checked by reading the file and by grep), and a target that is a guess says
"uncertain".

Categories:

- **A**: the same concept exists in the new tree. The target is named, and so is whatever the old
  node holds that the target has no place for.
- **B**: a standard concept whose shape differs between the trees in a way that needs a decision.
- **C**: T-SQL only (or SQL/PSM, which the new tree does not cover). No counterpart, so a family
  and a name are proposed.

Two facts apply to nearly every row and are not repeated in each:

1. **Old names are strings, new names are `Identifier`/`QualifiedName`.** Every `string Name`,
   `string Table`, `string[] Columns`, `Expression[] Names` and `Expression.ColumnReference(Text)`
   has to be split into parts with an `IdentifierStyle`, and that enum has no value for `[x]`
   (see Cross-cutting gaps).
2. **Old types are strings, new types are `DataType`.** Every `string? Type`, `string? Returns`
   and a cast's `Word` has to become a `DataType`. The old tree has no data type node at all.

Abbreviations: "Def+Tail" means the old record is `Statement.Definition(Name)`, with `Tail`
(words after the name as text), `Options` (`Clause[]?`) and `Verb` (`CREATE`/`ALTER`/`CREATE OR
ALTER`). "Removal" means `Statement.Removal(Expression[] Names)`, with `Tail` and `IfExists`.

---

## Statement (246 records)

The old `Statement` has an abstract `StatementCategory Category`. The new `Statement` has
nothing like it (see the enums).

### Data statements

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Statement.Select` | SQL-92 | B | `Statement.Select` | The old one wraps: `With Clause[]`, `Of Query`, `OrderBy Clause?`, `For Clause[]`, `Options Clause[]`. The new one *is* the query: its properties are the specification's, plus `SetOperations`, `Body`, `Parentheses`, `OrderBy`/`Offset`/`Fetch`. No place for `FOR XML/JSON/BROWSE`, `OPTION (…)`, or `WITH XMLNAMESPACES` (old `With` holds it as a `Clause.Option` beside the CTEs). `With` → `WithClause`, but `Recursive` is not a T-SQL word. |
| `Statement.Insert` | SQL-92 | A | `Statement.Insert` | `Target` is a `TableReference` in the old tree, a `QualifiedName` in the new, but T-SQL writes a table variable, an alias, `OPENQUERY(…)`, or a table with hints `WITH (TABLOCK)`. No place for `With` (a CTE before INSERT), `Top`, `Output`, `Options` (OPTION), or the `Into` word (`INTO`/`OVER`/omitted). `InsertSource` has no `EXECUTE` alternative (`Query.FromExecute`). `Override` is unused by T-SQL. |
| `Statement.Update` | SQL-92 | A | `Statement.Update` | `Target` is `TableTarget(QualifiedName)`, so an alias, variable, rowset or hinted table does not fit. No place for T-SQL's second `From TableReference[]`, `With`, `Top`, `Output`, `Options`. `Set Clause[]` → `Assignment` (see `Clause.Set`). `CurrentOf` fits `WHERE CURRENT OF` (T-SQL also writes `GLOBAL c`, which is `CursorReference.Global`). |
| `Statement.Delete` | SQL-92 | A | `Statement.Delete` | Same as Update: the target shape, plus no `From`, `With`, `Top`, `Output`, `Options`. `DELETE t FROM t JOIN …` has a target that is an alias. |
| `Statement.Merge` | SQL:2003 | A | `Statement.Merge` | `Target TableTarget` (T-SQL writes hints and a CTE name). `Using TableReference` → `SourceTable TableSource`. No place for `With`, `Top`, `Output`, `Options`, or `Into` (bool: `MERGE t` against `MERGE INTO t`). `Whens` → `MergeClause` (see `Clause.MergeWhen`). |
| `Statement.Truncate` | T-SQL | A | `Statement.TruncateTable` | No place for `Partitions` (`WITH (PARTITIONS (1, 3 TO 5))`). `Identity` is unused by T-SQL. |
| `Statement.ReadText` | T-SQL | C | `Statement.ReadText` | Column (2–4 part name), pointer, offset, size, `HOLDLOCK`. |
| `Statement.WriteText` | T-SQL | C | `Statement.WriteText` | Needs a binary literal for `TIMESTAMP = 0x…`. |
| `Statement.UpdateText` | T-SQL | C | `Statement.UpdateText` | As WriteText. |
| `Statement.Send` | T-SQL | C | `Statement.Send` | Service Broker. |
| `Statement.Receive` | T-SQL | C | `Statement.Receive` | `Columns Clause[]` is a select list, so it maps to `IReadOnlyList<SelectItem>`, with `Top` (see `Clause.Top`). `Into` is a table variable, which needs a variable identifier. |
| `Statement.GetConversationGroup` | T-SQL | C | `Statement.GetConversationGroup` | `Group` is a variable held as a string. |

### Procedural and control statements

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Statement.Compound` | SQL/PSM | C | `Statement.Compound` | The new tree has no compound statement. `TriggerAction` has `Statements` and a flag, `AtomicBlock`, and `RoutineBody.Sql` holds one `Statement`, so BEGIN…END has nowhere to go. `Atomic Clause[]` is `BEGIN ATOMIC WITH (…)` for natively compiled modules. Name it after PSM's `<compound statement>` so a PSM parser can share it. |
| `Statement.If` | SQL/PSM | C | `Statement.If` | PSM has ELSEIF; T-SQL does not. |
| `Statement.While` | SQL/PSM | C | `Statement.While` | |
| `Statement.TryCatch` | T-SQL | C | `Statement.TryCatch` | |
| `Statement.Declare` | SQL/PSM | C | `Statement.DeclareVariables` | Holds `VariableDeclaration`s. `DECLARE @c CURSOR` and table variables are in the same list. The name should not collide with `Statement.DeclareCursor`. |
| `Statement.Return` | SQL/PSM | A | `Statement.Return` | Fits (`NullKeyword` is unused by T-SQL). |
| `Statement.Print` | T-SQL | C | `Statement.Print` | |
| `Statement.Throw` | T-SQL | C | `Statement.Throw` | Arguments are positional (number, message, state), or none at all. |
| `Statement.GoTo` | T-SQL | C | `Statement.GoTo` | `Name` is an `Expression`, which should be an `Identifier`. (`ConditionAction.GoTo` exists for WHENEVER, a different concept.) |
| `Statement.Break` | T-SQL | C | `Statement.Break` | PSM spells it LEAVE; the new tree has no LEAVE. |
| `Statement.Continue` | T-SQL | C | `Statement.Continue` | PSM spells it ITERATE; the new tree has no ITERATE. |
| `Statement.Label` | T-SQL | C | `Statement.Label` | `finish:`. The name is a `string` and should be an `Identifier`. |
| `Statement.RaiseError` | T-SQL | C | `Statement.RaiseError` | `Tail` (`WITH LOG, NOWAIT, SETERROR`) is text and should become a flags enum. |
| `Statement.WaitFor` | T-SQL | C | `Statement.WaitFor` | `Kind` is a string (DELAY/TIME) and should become an enum. |
| `Statement.WaitForStatement` | T-SQL | C | `Statement.WaitForReceive` | Wraps RECEIVE or GET CONVERSATION GROUP, with a TIMEOUT. |
| `Statement.Checkpoint` | T-SQL | C | `Statement.Checkpoint` | |
| `Statement.Use` | T-SQL | C | `Statement.Use` | `Name` is an `Expression`, which should be an `Identifier`. |
| `Statement.BeginDialog` | T-SQL | C | `Statement.BeginDialog` | |
| `Statement.ConversationTimer` | T-SQL | C | `Statement.BeginConversationTimer` | |
| `Statement.EndConversation` | T-SQL | C | `Statement.EndConversation` | |
| `Statement.MoveConversation` | T-SQL | C | `Statement.MoveConversation` | |

### Transactions, EXECUTE, cursors, session

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Statement.Transaction` | SQL-92 | B | `Statement.StartTransaction`, `Statement.Commit`, `Statement.Rollback`, `Statement.Savepoint` | One old record carries `Kind` as a string; the new tree has four records. T-SQL `BEGIN [DISTRIBUTED] TRAN[SACTION] name WITH MARK 'd'`, `SAVE TRAN name`, `COMMIT TRAN name WITH (DELAYED_DURABILITY = ON)` and `ROLLBACK TRAN savepoint` fit poorly: the new `Commit`/`Rollback` have `Work` (bool) and `Chain` but no transaction name, no TRAN/TRANSACTION word, and no durability. `Rollback.ToSavepoint` needs `TO SAVEPOINT` semantics (T-SQL writes just the name). There is no `SAVE`, `BEGIN TRAN` (StartTransaction is `START`), or `DISTRIBUTED`. |
| `Statement.Execute` | T-SQL | B | `Statement.Call` / `Statement.ExecuteImmediate` (uncertain) | Name clash: the new `Statement.Execute` is dynamic SQL's `EXECUTE s USING …`, a different statement. `EXEC proc args` is closest to `Call(Invocation)`, but `Argument` has no `OUTPUT`, no `DEFAULT` value, and no `@p = v` spelling (`NamedAssignment` is `=>`). `EXEC ('…' + @s) AS USER = … AT server` is closest to `ExecuteImmediate(Sql)`, but it has no context, `AT`, `AT DATA_SOURCE`, `WITH RECOMPILE / RESULT SETS`, return-code variable (`Into`), `;number`, bare call (`Bare`), or `OPENDATASOURCE` server. Decide whether T-SQL EXECUTE gets its own record (e.g. `Statement.ExecuteProcedure`) or stretches `Call`. |
| `Statement.DeclareCursor` | T-SQL | B | `Statement.DeclareCursor` | New: `CursorReference`, `CursorProperties`, `CursorSource.Query(Select, Updatability)`. The name string → `CursorReference.Name`. `CursorDefinition.Before` (INSENSITIVE, SCROLL) → `CursorProperties.Sensitivity`/`Scrollability`. No place for the T-SQL words after CURSOR (LOCAL/GLOBAL, FORWARD_ONLY, STATIC, KEYSET, DYNAMIC, FAST_FORWARD, READ_ONLY, SCROLL_LOCKS, OPTIMISTIC, TYPE_WARNING), the query's `OPTION (…)`, `WITH` before the query, or `AccessFirst`. |
| `Statement.SetCursor` | T-SQL | C | `Statement.SetCursorVariable` | `SET @c = CURSOR … FOR …`. Reuses whatever `DeclareCursor` gets. |
| `Statement.CursorAction` | T-SQL | A | `Statement.OpenCursor`, `Statement.CloseCursor` | One old record with a verb string maps to two targets. There is no `DEALLOCATE`, so it needs `Statement.DeallocateCursor`. A cursor variable `@c` goes to `CursorReference.ExtendedName`; `Global` → `CursorReference.Global`. |
| `Statement.Fetch` | T-SQL | A | `Statement.FetchCursor` | `Orientation` string → `FetchOrientation` (NEXT, PRIOR, FIRST, LAST, ABSOLUTE, RELATIVE all exist). `Offset` → `FetchOrientation.Offset`. `From` → `FromKeyword`. `Global` → `CursorReference.Global`. `Into` → `DynamicArguments.Values`. Nothing is lost. |
| `Statement.ExecuteAs` | T-SQL | C | `Statement.ExecuteAs` | `Kind` is a string (CALLER/USER/LOGIN) and should become an enum. |
| `Statement.Revert` | T-SQL | C | `Statement.Revert` | |
| `Statement.SetUser` | T-SQL | C | `Statement.SetUser` | |
| `Statement.LineNumber` | T-SQL | C | `Statement.LineNumber` | |
| `Statement.SetStatement` | T-SQL | C | `Statement.SetOptions` (holding a T-SQL `SetOption` family, see SetExpression) | Only `SET TRANSACTION ISOLATION LEVEL` has a standard target (`Statement.SetTransaction`). The rest has none. |
| `Statement.SetVariable` | T-SQL | C | `Statement.SetVariable` | PSM's `<assignment statement>` is not in the new tree. `Name` is `@v`, `@v.prop` or `@x.modify`. `Operator` (`+=`, …) needs an `AssignmentOperator` enum. `Through` (`SET @v = col = expr`). Needs a variable reference (see `ParameterKind`). |
| `Statement.Kill` | T-SQL | C | `Statement.Kill` | |
| `Statement.Reconfigure` | T-SQL | C | `Statement.Reconfigure` | |
| `Statement.Shutdown` | T-SQL | C | `Statement.Shutdown` | |
| `Statement.Dbcc` | T-SQL | C | `Statement.Dbcc` | Command as written; arguments include `NamedArgument`s; options. |
| `Statement.UpdateStatistics` | T-SQL | C | `Statement.UpdateStatistics` | Def+Tail. |
| `Statement.TriggerSwitch` | T-SQL | C | `Statement.EnableTrigger` / `Statement.DisableTrigger` | `On` is an object, `DATABASE` or `ALL SERVER`, held as a string; it needs an enum plus a name. |

### Tables, views, indexes, routines, triggers

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Statement.TableDefinition` | SQL-92 | B | `Statement.CreateTable` with `TableContents.Elements` | Old: `Elements Clause[]` mixes `ColumnDefinition`, `ConstraintDefinition` (including inline indexes) and `Connection`. New: `TableElement` (Column, TableConstraint, Like, Period, …) with no index element. No place for `Kind` (AS FILETABLE/NODE/EDGE), `Placements` (ON, TEXTIMAGE_ON, FILESTREAM_ON), `Options` (`WITH (…)`), `External` (CREATE EXTERNAL TABLE). The new `Scope` (GLOBAL/LOCAL TEMPORARY) is not how T-SQL says temp (`#t`, `##t` in the name). `SystemVersioning` (bool) is standard, while T-SQL writes `WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = …))`, and `PERIOD FOR SYSTEM_TIME (a, b)` maps to `TableElement.Period`. |
| `Statement.CreateTableAsSelect` | T-SQL | B | `Statement.CreateTable` with `TableContents.AsQuery` | The concept is the standard's `<as subquery clause>`, but `AsQuery.Data` is a required `WithDataMode`, and T-SQL (Synapse CTAS) writes no `WITH [NO] DATA`. It also needs `WITH (DISTRIBUTION = …)` options and `External`. `Body` is a `Statement` (SELECT with CTEs/OPTION), not a bare `Select`. |
| `Statement.AlterTable` | SQL-92 | B | `Statement.AlterTable` | Old: `Action` string + `Elements Clause[]` + `Options` + `Tail` (one ALTER may add several columns and constraints, drop several, `SWITCH PARTITION`, `SET (LOCK_ESCALATION)`, `WITH CHECK CHECK CONSTRAINT ALL`, `REBUILD`, `ENABLE TRIGGER`…). New: exactly one `AlterTableAction`, and `AddColumn` holds one column. Decide between a list of actions and T-SQL extension actions (`AlterTableAction.Extension` exists). |
| `Statement.ViewDefinition` | SQL-92 | B | `Statement.CreateView` | New `Query` is a required `Select`, while the old `Body` is a `Statement` (a SELECT with a CTE and OPTION). `CheckOption` bool → `CheckOption?` enum (T-SQL only writes `WITH CHECK OPTION` = `Unqualified`). No place for `Options` (`WITH SCHEMABINDING, ENCRYPTION, VIEW_METADATA`), `Materialized` (Synapse), or `Verb` (CREATE/ALTER/CREATE OR ALTER). `Columns` → `ViewSpecification.Regular`. |
| `Statement.CreateIndex` | T-SQL | C | `Statement.CreateIndex` (plus an `IndexDefinition` record shared with inline table indexes) | Kind (PRIMARY XML, XML, SELECTIVE XML, SPATIAL, COLUMNSTORE), `Using`. |
| `Statement.AlterIndex` | T-SQL | C | `Statement.AlterIndex` | `Action` string, which could become an enum plus options. |
| `Statement.StatisticsDefinition` | T-SQL | C | `Statement.CreateStatistics` | Def+Tail. |
| `Statement.CreateProcedure` | SQL/PSM | B | `Statement.CreateRoutine` with `RoutineDefinition(Kind = Procedure)` | `Parameters Clause[]` (ParameterDeclaration) → `ParameterDefinition` (see that row). `Body Statement[]` (T-SQL writes `AS stmt; stmt;` with no BEGIN) → `RoutineBody.Sql(Statement)` holds one statement, so a list or an implicit `Compound` is needed. No place for `Options` (`WITH RECOMPILE, ENCRYPTION, EXECUTE AS …, NATIVE_COMPILATION, SCHEMABINDING`), `ForReplication`, `External` (`EXTERNAL NAME a.b.c` is close to `RoutineBody.External.Name`), `Number` (`;1`), or `Verb` (CREATE OR ALTER, and ALTER PROCEDURE is not `AlterRoutine`). |
| `Statement.CreateFunction` | SQL/PSM | B | `Statement.CreateRoutine` with `RoutineDefinition(Kind = Function)` | `Returns` string → `ReturnsDefinition` (`RETURNS TABLE` fits `TableKeyword`; `RETURNS @t TABLE (…)` needs the variable name and `TableElement`s, while `TableColumns` is `FieldDefinition` only). The inline TVF body `RETURN (SELECT …)`. `Order` (CLR `ORDER (…)`). The same body-list, options and verb problems as CreateProcedure. |
| `Statement.CreateTrigger` | SQL:1999 | B | `Statement.CreateTrigger` | New: one `TriggerEvent` and `TriggerTime` (Before/After/InsteadOf); `Table` is a `QualifiedName`. T-SQL has an event list (`FOR INSERT, UPDATE`), `FOR` as its own spelling of AFTER, DDL/logon triggers `ON DATABASE` / `ON ALL SERVER` with event groups, `WITH APPEND`, `NOT FOR REPLICATION`, `WITH ENCRYPTION/EXECUTE AS`, `EXTERNAL NAME`, a `Statement[]` body, and CREATE OR ALTER. `TriggerAction.Granularity`/`When`/`Referencing` are unused by T-SQL. |
| `Statement.Classification` | T-SQL | C | `Statement.AddSensitivityClassification` | |
| `Statement.AddSignature` | T-SQL | C | `Statement.AddSignature` | `By` is text-ish expressions. |

### Principals, schemas, security

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Statement.Grant` | SQL-92 | B | `Statement.Grant` with `GrantBody.Privileges` | Old: `Privileges string[]`, `Principals string[]`, `On` string (`OBJECT::dbo.t`, `SCHEMA::s`, `DATABASE::d`), `GrantOption`, `As`. New: `Privilege.Kind` has no CONTROL, ALTER, VIEW DEFINITION, TAKE OWNERSHIP, IMPERSONATE, CREATE TABLE, or the other ~200 T-SQL permissions. `PrivilegeObjectKind` has no Object/Schema/Database/Login/… class, and there is no `::` spelling. `Grantee` has only Public/AuthorizationIdentifier. `GrantedBy` is the `Grantor` enum (CURRENT_USER/ROLE), while T-SQL `AS principal` is a name. The column list `SELECT (a, b)` fits `Privilege.Columns`. |
| `Statement.Deny` | T-SQL | C | `Statement.Deny` | Should share the `Privilege`/`PrivilegeObject` shape chosen for Grant; `Cascade`, `As`. |
| `Statement.Revoke` | SQL-92 | B | `Statement.Revoke` with `RevokeBody.Privileges` | As Grant. `Behavior` is a required `DropBehavior`, while T-SQL writes CASCADE or nothing. `From` (bool: TO against FROM) has no place. |
| `Statement.CreateRole` | SQL:1999 | A | `Statement.CreateRole` | Old is Def+Tail. `AUTHORIZATION owner` has no place (`Admin` is the `Grantor` enum, WITH ADMIN). |
| `Statement.AlterRole` | T-SQL | C | `Statement.AlterRole` | ADD/DROP MEMBER, WITH NAME. |
| `Statement.CreateServerRole` | T-SQL | C | `Statement.CreateServerRole` | |
| `Statement.AlterServerRole` | T-SQL | C | `Statement.AlterServerRole` | |
| `Statement.CreateApplicationRole` | T-SQL | C | `Statement.CreateApplicationRole` | |
| `Statement.AlterApplicationRole` | T-SQL | C | `Statement.AlterApplicationRole` | |
| `Statement.CreateLogin` | T-SQL | C | `Statement.CreateLogin` | Def+Tail. |
| `Statement.AlterLogin` | T-SQL | C | `Statement.AlterLogin` | Def+Tail. |
| `Statement.CreateUser` | T-SQL | C | `Statement.CreateUser` | Def+Tail. |
| `Statement.AlterUser` | T-SQL | C | `Statement.AlterUser` | Def+Tail. |
| `Statement.SchemaDefinition` | SQL-92 | A | `Statement.CreateSchema` | Old keeps only Name+Tail. `AUTHORIZATION o` → `Authorization`; schema elements → `Elements`. Nothing essential is missing. |
| `Statement.AlterSchema` | T-SQL | C | `Statement.AlterSchema` | `TRANSFER class::name`. |
| `Statement.AlterAuthorization` | T-SQL | C | `Statement.AlterAuthorization` | |
| `Statement.SequenceDefinition` | T-SQL | A | `Statement.CreateSequence` / `Statement.AlterSequence` | One old record (the verb is in `Verb`) maps to two targets. `SequenceOption` has no `CACHE n` / `NO CACHE`. `Restart` fits ALTER. |
| `Statement.TypeDefinition` | T-SQL | B | `Statement.CreateType` (uncertain) | T-SQL `CREATE TYPE t FROM int NOT NULL`, `AS TABLE (…)` and `EXTERNAL NAME a.[c]` do not match `UserDefinedTypeDefinition` (distinct/structured type with `Representation`, `Options`, `Methods`). The alias form is closest to `TypeRepresentation.Type`; there is no table-type representation. |
| `Statement.SynonymDefinition` | T-SQL | C | `Statement.CreateSynonym` | |
| `Statement.XmlSchemaCollectionDefinition` | T-SQL | C | `Statement.CreateXmlSchemaCollection` | |
| `Statement.AlterXmlSchemaCollection` | T-SQL | C | `Statement.AlterXmlSchemaCollection` | |
| `Statement.AssemblyDefinition` | T-SQL | C | `Statement.CreateAssembly` / `AlterAssembly` | Def+Tail, both verbs in one record. |
| `Statement.RuleDefinition` | T-SQL | C | `Statement.CreateRule` | |
| `Statement.DefaultDefinition` | T-SQL | C | `Statement.CreateDefault` | Not the standard's column default. |
| `Statement.AggregateDefinition` | T-SQL | C | `Statement.CreateAggregate` | |
| `Statement.PartitionFunctionDefinition` | T-SQL | C | `Statement.CreatePartitionFunction` | |
| `Statement.AlterPartitionFunction` | T-SQL | C | `Statement.AlterPartitionFunction` | |
| `Statement.PartitionSchemeDefinition` | T-SQL | C | `Statement.CreatePartitionScheme` | |
| `Statement.AlterPartitionScheme` | T-SQL | C | `Statement.AlterPartitionScheme` | |
| `Statement.FullTextIndexDefinition` | T-SQL | C | `Statement.CreateFullTextIndex` | |
| `Statement.AlterFullTextIndex` | T-SQL | C | `Statement.AlterFullTextIndex` | |
| `Statement.FullTextCatalogDefinition` | T-SQL | C | `Statement.CreateFullTextCatalog` | |
| `Statement.AlterFullTextCatalog` | T-SQL | C | `Statement.AlterFullTextCatalog` | |
| `Statement.FullTextStopListDefinition` | T-SQL | C | `Statement.CreateFullTextStopList` | |
| `Statement.AlterFullTextStopList` | T-SQL | C | `Statement.AlterFullTextStopList` | |
| `Statement.SearchPropertyListDefinition` | T-SQL | C | `Statement.CreateSearchPropertyList` | |
| `Statement.AlterSearchPropertyList` | T-SQL | C | `Statement.AlterSearchPropertyList` | |
| `Statement.AsymmetricKeyDefinition` | T-SQL | C | `Statement.CreateAsymmetricKey` | Def with `Options` (duplicate-option checks depend on them). |
| `Statement.AlterAsymmetricKey` | T-SQL | C | `Statement.AlterAsymmetricKey` | |
| `Statement.SymmetricKeyDefinition` | T-SQL | C | `Statement.CreateSymmetricKey` | |
| `Statement.AlterSymmetricKey` | T-SQL | C | `Statement.AlterSymmetricKey` | |
| `Statement.CertificateDefinition` | T-SQL | C | `Statement.CreateCertificate` | |
| `Statement.AlterCertificate` | T-SQL | C | `Statement.AlterCertificate` | |
| `Statement.MasterKeyDefinition` | T-SQL | C | `Statement.CreateMasterKey` | |
| `Statement.AlterMasterKey` | T-SQL | C | `Statement.AlterMasterKey` | |
| `Statement.AlterServiceMasterKey` | T-SQL | C | `Statement.AlterServiceMasterKey` | |
| `Statement.DatabaseEncryptionKeyDefinition` | T-SQL | C | `Statement.CreateDatabaseEncryptionKey` | |
| `Statement.AlterDatabaseEncryptionKey` | T-SQL | C | `Statement.AlterDatabaseEncryptionKey` | |
| `Statement.ColumnEncryptionKeyDefinition` | T-SQL | C | `Statement.CreateColumnEncryptionKey` | |
| `Statement.AlterColumnEncryptionKey` | T-SQL | C | `Statement.AlterColumnEncryptionKey` | |
| `Statement.ColumnMasterKeyDefinition` | T-SQL | C | `Statement.CreateColumnMasterKey` | |
| `Statement.CredentialDefinition` | T-SQL | C | `Statement.CreateCredential` / `AlterCredential` | Both verbs. |
| `Statement.DatabaseScopedCredentialDefinition` | T-SQL | C | `Statement.CreateDatabaseScopedCredential` / `Alter…` | Both verbs. |
| `Statement.SecurityPolicyDefinition` | T-SQL | C | `Statement.CreateSecurityPolicy` / `Alter…` | Both verbs. |
| `Statement.CryptographicProviderDefinition` | T-SQL | C | `Statement.CreateCryptographicProvider` / `Alter…` | Both verbs. |
| `Statement.OpenSymmetricKey` | T-SQL | C | `Statement.OpenSymmetricKey` | Def+Tail. |
| `Statement.OpenMasterKey` | T-SQL | C | `Statement.OpenMasterKey` | Def+Tail. |
| `Statement.CloseSymmetricKey` | T-SQL | C | `Statement.CloseSymmetricKey` | Def+Tail. |
| `Statement.CloseAllSymmetricKeys` | T-SQL | C | `Statement.CloseAllSymmetricKeys` | Def with an empty name. |
| `Statement.CloseMasterKey` | T-SQL | C | `Statement.CloseMasterKey` | Def with an empty name. |

### Server objects, database, Service Broker, backup/restore

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Statement.ExternalDataSourceDefinition` | T-SQL | C | `Statement.CreateExternalDataSource` / `Alter…` | Def+Tail, both verbs. |
| `Statement.ExternalFileFormatDefinition` | T-SQL | C | `Statement.CreateExternalFileFormat` | |
| `Statement.ExternalLibraryDefinition` | T-SQL | C | `Statement.CreateExternalLibrary` / `Alter…` | |
| `Statement.ExternalResourcePoolDefinition` | T-SQL | C | `Statement.CreateExternalResourcePool` / `Alter…` | |
| `Statement.ExternalLanguageDefinition` | T-SQL | C | `Statement.CreateExternalLanguage` / `Alter…` | |
| `Statement.ExternalModelDefinition` | T-SQL | C | `Statement.CreateExternalModel` / `Alter…` | |
| `Statement.ResourcePoolDefinition` | T-SQL | C | `Statement.CreateResourcePool` / `Alter…` | |
| `Statement.WorkloadGroupDefinition` | T-SQL | C | `Statement.CreateWorkloadGroup` / `Alter…` | |
| `Statement.AlterResourceGovernor` | T-SQL | C | `Statement.AlterResourceGovernor` | Def with an empty name. |
| `Statement.AlterServerConfiguration` | T-SQL | C | `Statement.AlterServerConfiguration` | |
| `Statement.ServerAuditDefinition` | T-SQL | C | `Statement.CreateServerAudit` / `Alter…` | |
| `Statement.AuditSpecificationDefinition` | T-SQL | C | `Statement.CreateServerAuditSpecification` / `Alter…` | |
| `Statement.DatabaseAuditSpecificationDefinition` | T-SQL | C | `Statement.CreateDatabaseAuditSpecification` / `Alter…` | |
| `Statement.EventSessionDefinition` | T-SQL | C | `Statement.CreateEventSession` / `Alter…` | `On`, `State` and `Verb` are strings and should become enums; pieces are `EventPiece`. |
| `Statement.EventNotificationDefinition` | T-SQL | C | `Statement.CreateEventNotification` | |
| `Statement.AvailabilityGroupDefinition` | T-SQL | C | `Statement.CreateAvailabilityGroup` / `Alter…` | |
| `Statement.EndpointDefinition` | T-SQL | C | `Statement.CreateEndpoint` / `Alter…` | Verb, owner, state, protocol and payload are strings and should become enums. |
| `Statement.MessageTypeDefinition` | T-SQL | C | `Statement.CreateMessageType` / `Alter…` | |
| `Statement.ContractDefinition` | T-SQL | C | `Statement.CreateContract` | |
| `Statement.QueueDefinition` | T-SQL | C | `Statement.CreateQueue` / `Alter…` | |
| `Statement.ServiceDefinition` | T-SQL | C | `Statement.CreateService` / `Alter…` | |
| `Statement.RouteDefinition` | T-SQL | C | `Statement.CreateRoute` / `Alter…` | |
| `Statement.RemoteServiceBindingDefinition` | T-SQL | C | `Statement.CreateRemoteServiceBinding` / `Alter…` | |
| `Statement.BrokerPriorityDefinition` | T-SQL | C | `Statement.CreateBrokerPriority` / `Alter…` | |
| `Statement.CreateDatabase` | T-SQL | C | `Statement.CreateDatabase` | Files, Log, Containment, Collation (string, should be `CollationName`), Tail, Options, With. |
| `Statement.AlterDatabaseSet` | T-SQL | C | `Statement.AlterDatabase` + action `Set` | Proposal: one `Statement.AlterDatabase(Name, AlterDatabaseAction)` with a T-SQL action family, mirroring `AlterTable`/`AlterTableAction`. That would absorb the 15 `AlterDatabase*` records and keep the one-level rule. |
| `Statement.AlterDatabaseScopedConfiguration` | T-SQL | C | `Statement.AlterDatabaseScopedConfiguration` | |
| `Statement.AlterDatabaseCollate` | T-SQL | C | `AlterDatabaseAction.Collate` | Def+Tail. |
| `Statement.AlterDatabaseModifyName` | T-SQL | C | `AlterDatabaseAction.ModifyName` | |
| `Statement.AlterDatabaseModifyFileGroup` | T-SQL | C | `AlterDatabaseAction.ModifyFileGroup` | |
| `Statement.AlterDatabaseModifyFile` | T-SQL | C | `AlterDatabaseAction.ModifyFile` | |
| `Statement.AlterDatabaseModify` | T-SQL | C | `AlterDatabaseAction.Modify` | |
| `Statement.AlterDatabaseAddFileGroup` | T-SQL | C | `AlterDatabaseAction.AddFileGroup` | |
| `Statement.AlterDatabaseAddLogFile` | T-SQL | C | `AlterDatabaseAction.AddLogFile` | |
| `Statement.AlterDatabaseAddFile` | T-SQL | C | `AlterDatabaseAction.AddFile` | |
| `Statement.AlterDatabaseRemoveFileGroup` | T-SQL | C | `AlterDatabaseAction.RemoveFileGroup` | |
| `Statement.AlterDatabaseRemoveFile` | T-SQL | C | `AlterDatabaseAction.RemoveFile` | |
| `Statement.AlterDatabaseRebuildLog` | T-SQL | C | `AlterDatabaseAction.RebuildLog` | |
| `Statement.AlterDatabasePerformCutover` | T-SQL | C | `AlterDatabaseAction.PerformCutover` | |
| `Statement.AlterDatabaseModifyBackupStorageRedundancy` | T-SQL | C | `AlterDatabaseAction.ModifyBackupStorageRedundancy` | |
| `Statement.BackupDatabase` | T-SQL | C | `Statement.BackupDatabase` | Def+Tail (backup options kept as text). |
| `Statement.BackupTransactionLog` | T-SQL | C | `Statement.BackupLog` | |
| `Statement.BackupServer` | T-SQL | C | `Statement.BackupServer` | |
| `Statement.BackupGroup` | T-SQL | C | `Statement.BackupGroup` | |
| `Statement.BackupCertificate` | T-SQL | C | `Statement.BackupCertificate` | |
| `Statement.BackupMasterKey` | T-SQL | C | `Statement.BackupMasterKey` | |
| `Statement.BackupServiceMasterKey` | T-SQL | C | `Statement.BackupServiceMasterKey` | |
| `Statement.BackupSymmetricKey` | T-SQL | C | `Statement.BackupSymmetricKey` | |
| `Statement.RestoreDatabase` | T-SQL | C | `Statement.RestoreDatabase` | |
| `Statement.RestoreLog` | T-SQL | C | `Statement.RestoreLog` | |
| `Statement.RestoreFileListOnly` | T-SQL | C | `Statement.RestoreFileListOnly` | |
| `Statement.RestoreHeaderOnly` | T-SQL | C | `Statement.RestoreHeaderOnly` | |
| `Statement.RestoreLabelOnly` | T-SQL | C | `Statement.RestoreLabelOnly` | |
| `Statement.RestoreRewindOnly` | T-SQL | C | `Statement.RestoreRewindOnly` | |
| `Statement.RestoreVerifyOnly` | T-SQL | C | `Statement.RestoreVerifyOnly` | |
| `Statement.RestoreMasterKey` | T-SQL | C | `Statement.RestoreMasterKey` | |
| `Statement.RestoreServiceMasterKey` | T-SQL | C | `Statement.RestoreServiceMasterKey` | |
| `Statement.RestoreSymmetricKey` | T-SQL | C | `Statement.RestoreSymmetricKey` | |

### DROP (all `Removal`: `Expression[] Names`, `Tail`, `IfExists`)

The standard's DROP records take **one** name and, except `DropTrigger`, `DropTranslation`,
`DropCharacterSet` and `DropAssertion`, a **required** `DropBehavior`. T-SQL writes a list of
names, `IF EXISTS`, and never `RESTRICT`. That shape decision is the B rows below; the C rows
follow whatever is decided.

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Statement.DropTable` | T-SQL | B | `Statement.DropTable` | `Name` → list; `Behavior` → nullable; no `IfExists`. `DROP TABLE a, b` is one statement. |
| `Statement.DropView` | T-SQL | B | `Statement.DropView` | As DropTable. |
| `Statement.DropSchema` | T-SQL | B | `Statement.DropSchema` | Single name. `Behavior` → nullable; `IfExists`. |
| `Statement.DropSequence` | T-SQL | B | `Statement.DropSequence` | List, nullable behavior, `IfExists`. |
| `Statement.DropType` | T-SQL | B | `Statement.DropType` | Nullable behavior, `IfExists`. |
| `Statement.DropRole` | T-SQL | B | `Statement.DropRole` | `Name` is an `Identifier`, while T-SQL writes a list; nullable behavior; `IfExists`. |
| `Statement.DropTrigger` | T-SQL | B | `Statement.DropTrigger` | List, `IfExists`, and `ON DATABASE` / `ON ALL SERVER` in the Tail. |
| `Statement.DropFunction` | T-SQL | B | `Statement.DropRoutine` | `RoutineDesignator` is single, with kind and parameter types; T-SQL has a list, no signature, `IfExists`; `Behavior` required. |
| `Statement.DropProcedure` | T-SQL | B | `Statement.DropRoutine` | As DropFunction; `PROC` against `PROCEDURE` spelling. |
| `Statement.DropAggregate` | T-SQL | C | `Statement.DropAggregate` | |
| `Statement.DropApplicationRole` | T-SQL | C | `Statement.DropApplicationRole` | |
| `Statement.DropAvailabilityGroup` | T-SQL | C | `Statement.DropAvailabilityGroup` | |
| `Statement.DropBrokerPriority` | T-SQL | C | `Statement.DropBrokerPriority` | |
| `Statement.DropCertificate` | T-SQL | C | `Statement.DropCertificate` | |
| `Statement.DropColumnEncryptionKey` | T-SQL | C | `Statement.DropColumnEncryptionKey` | |
| `Statement.DropColumnMasterKey` | T-SQL | C | `Statement.DropColumnMasterKey` | |
| `Statement.DropContract` | T-SQL | C | `Statement.DropContract` | |
| `Statement.DropCredential` | T-SQL | C | `Statement.DropCredential` | |
| `Statement.DropCryptographicProvider` | T-SQL | C | `Statement.DropCryptographicProvider` | |
| `Statement.DropDatabaseAuditSpecification` | T-SQL | C | `Statement.DropDatabaseAuditSpecification` | |
| `Statement.DropDatabaseScopedCredential` | T-SQL | C | `Statement.DropDatabaseScopedCredential` | |
| `Statement.DropDatabase` | T-SQL | C | `Statement.DropDatabase` | |
| `Statement.DropDefault` | T-SQL | C | `Statement.DropDefault` | Not `AlterDomainAction.DropDefault`. |
| `Statement.DropEndpoint` | T-SQL | C | `Statement.DropEndpoint` | |
| `Statement.DropExternalDataSource` | T-SQL | C | `Statement.DropExternalDataSource` | |
| `Statement.DropExternalFileFormat` | T-SQL | C | `Statement.DropExternalFileFormat` | |
| `Statement.DropExternalLanguage` | T-SQL | C | `Statement.DropExternalLanguage` | |
| `Statement.DropExternalModel` | T-SQL | C | `Statement.DropExternalModel` | |
| `Statement.DropExternalResourcePool` | T-SQL | C | `Statement.DropExternalResourcePool` | |
| `Statement.DropExternalTable` | T-SQL | C | `Statement.DropExternalTable` | |
| `Statement.DropFulltextCatalog` | T-SQL | C | `Statement.DropFullTextCatalog` | |
| `Statement.DropFulltextStoplist` | T-SQL | C | `Statement.DropFullTextStopList` | |
| `Statement.DropLogin` | T-SQL | C | `Statement.DropLogin` | |
| `Statement.DropMessageType` | T-SQL | C | `Statement.DropMessageType` | |
| `Statement.DropPartitionFunction` | T-SQL | C | `Statement.DropPartitionFunction` | |
| `Statement.DropPartitionScheme` | T-SQL | C | `Statement.DropPartitionScheme` | |
| `Statement.DropQueue` | T-SQL | C | `Statement.DropQueue` | |
| `Statement.DropRemoteServiceBinding` | T-SQL | C | `Statement.DropRemoteServiceBinding` | |
| `Statement.DropResourcePool` | T-SQL | C | `Statement.DropResourcePool` | |
| `Statement.DropRoute` | T-SQL | C | `Statement.DropRoute` | |
| `Statement.DropRule` | T-SQL | C | `Statement.DropRule` | |
| `Statement.DropSearchPropertyList` | T-SQL | C | `Statement.DropSearchPropertyList` | |
| `Statement.DropSecurityPolicy` | T-SQL | C | `Statement.DropSecurityPolicy` | |
| `Statement.DropServerAuditSpecification` | T-SQL | C | `Statement.DropServerAuditSpecification` | |
| `Statement.DropServerAudit` | T-SQL | C | `Statement.DropServerAudit` | |
| `Statement.DropServerRole` | T-SQL | C | `Statement.DropServerRole` | |
| `Statement.DropService` | T-SQL | C | `Statement.DropService` | |
| `Statement.DropStatistics` | T-SQL | C | `Statement.DropStatistics` | |
| `Statement.DropSynonym` | T-SQL | C | `Statement.DropSynonym` | |
| `Statement.DropUser` | T-SQL | C | `Statement.DropUser` | |
| `Statement.DropWorkloadClassifier` | T-SQL | C | `Statement.DropWorkloadClassifier` | |
| `Statement.DropWorkloadGroup` | T-SQL | C | `Statement.DropWorkloadGroup` | |
| `Statement.DropXmlSchemaCollection` | T-SQL | C | `Statement.DropXmlSchemaCollection` | |
| `Statement.DropAsymmetricKey` | T-SQL | C | `Statement.DropAsymmetricKey` | |
| `Statement.DropSymmetricKey` | T-SQL | C | `Statement.DropSymmetricKey` | |
| `Statement.DropAssembly` | T-SQL | C | `Statement.DropAssembly` | |
| `Statement.DropExternalLibrary` | T-SQL | C | `Statement.DropExternalLibrary` | |
| `Statement.DropEventSession` | T-SQL | C | `Statement.DropEventSession` | |
| `Statement.DropEventNotification` | T-SQL | C | `Statement.DropEventNotification` | |
| `Statement.DropFulltextIndex` | T-SQL | C | `Statement.DropFullTextIndex` | |
| `Statement.DropIndex` | T-SQL | C | `Statement.DropIndex` | `ix ON t` and `t.ix` spellings, `WITH (…)`. |
| `Statement.DropSignature` | T-SQL | C | `Statement.DropSignature` | `From`, `Counter`. |
| `Statement.DropSensitivityClassification` | T-SQL | C | `Statement.DropSensitivityClassification` | |
| `Statement.DropMasterKey` | T-SQL | C | `Statement.DropMasterKey` | |
| `Statement.DropDatabaseEncryptionKey` | T-SQL | C | `Statement.DropDatabaseEncryptionKey` | |

The abstract bases `Statement.Definition` and `Statement.Removal` are not rows. They are
one-level violations of their own (sealed records under an abstract record under `Statement`), and
the new tree's rule 6 forbids them; `Tail`/`Options`/`Verb`/`IfExists` would have to be declared on
each record, or `Statement.Extension(Dialect, Kind, Parts)` used for the text-only ones.

---

## SetExpression (20 records under 7 abstract groups)

The new tree has no SET-option family, and its two-level layout (`SetExpression.Locking.LockTimeout`)
violates requirement 6/7 as it stands.

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `SetExpression.DateAndTime.DateFirst` | T-SQL | C | `SetOption.DateFirst` | Proposal: a flat `SetOption` family (one level), with the Microsoft group as a property or enum rather than a nesting level. |
| `SetExpression.DateAndTime.DateFormat` | T-SQL | C | `SetOption.DateFormat` | |
| `SetExpression.Locking.DeadlockPriority` | T-SQL | C | `SetOption.DeadlockPriority` | |
| `SetExpression.Locking.LockTimeout` | T-SQL | C | `SetOption.LockTimeout` | |
| `SetExpression.QueryExecution.Switch` | T-SQL | C | `SetOption.Switch` | The four `Switch` records differ only by group and could become one record with `SetCategory`. `Option` is a string. |
| `SetExpression.QueryExecution.RowCount` | T-SQL | C | `SetOption.RowCount` | |
| `SetExpression.QueryExecution.TextSize` | T-SQL | C | `SetOption.TextSize` | |
| `SetExpression.QueryExecution.QueryGovernorCostLimit` | T-SQL | C | `SetOption.QueryGovernorCostLimit` | |
| `SetExpression.IsoSettings.Switch` | T-SQL | C | `SetOption.Switch` | |
| `SetExpression.Statistics.Switch` | T-SQL | C | `SetOption.Switch` | |
| `SetExpression.Statistics.Report` | T-SQL | C | `SetOption.Statistics` | `Kind` string (IO/PROFILE/TIME/XML) should become an enum. |
| `SetExpression.Transactions.Switch` | T-SQL | C | `SetOption.Switch` | |
| `SetExpression.Transactions.IsolationLevel` | SQL-92 | A | `Statement.SetTransaction` + `TransactionMode.Isolation` | `IsolationLevel` has no `Snapshot`. The `TRAN` spelling has no place. `Level` is a string today. It stands inside `SetStatement`'s list in the old tree and would be a statement of its own in the new. |
| `SetExpression.Miscellaneous.Switch` | T-SQL | C | `SetOption.Switch` | |
| `SetExpression.Miscellaneous.Language` | T-SQL | C | `SetOption.Language` | |
| `SetExpression.Miscellaneous.FipsFlagger` | T-SQL | C | `SetOption.FipsFlagger` | |
| `SetExpression.Miscellaneous.ContextInfo` | T-SQL | C | `SetOption.ContextInfo` | Binary literal. |
| `SetExpression.Miscellaneous.IdentityInsert` | T-SQL | C | `SetOption.IdentityInsert` | `Table` string → `QualifiedName`. |
| `SetExpression.Miscellaneous.Offset` | T-SQL | C | `SetOption.Offsets` | |
| `SetExpression.Miscellaneous.ErrorLevel` | T-SQL | C | `SetOption.ErrorLevel` | |

---

## Query (12 records)

The new tree has no `Query` root. A query is `Statement.Select`, and its operands are
`QueryOperand` and `SetOperation`.

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Query.Specification` | SQL-92 | B | `Statement.Select` (its own properties) | `Quantifier` string → `SetQuantifier?`. No place for `Top`. `Columns Clause[]` → `SelectItem` (T-SQL adds `VariableAssignment` and `alias = expr`). `Into Clause.Into` (a table and filegroup) has no counterpart: new `IntoClause(Targets)` is the single-row select's variables. `From TableReference[]` → `FromClause`; `GroupBy` → `GroupByClause`; `Windows` → `WindowClause`. A specification used as a set operand becomes `QueryOperand.Select`. |
| `Query.TableValueConstructor` | SQL-92 | A | `QueryOperand.Values` / `InsertSource.Values` | `Rows Expression[]` of `RowValueConstructor` → `RowValue` (`RowKeyword` false). As a whole query, it is `Statement.Select.Body`. |
| `Query.ExplicitTable` | SQL-92 | A | `QueryOperand.Table` | Name string → `QualifiedName`. The SQL-92 grammar reads it; SQL Server does not. |
| `Query.Parenthesized` | SQL-92 | B | `Statement.Select.Parentheses` (count) / `Statement.Select.Body = QueryOperand.Select` | Old nesting node against a count plus operand. A bracketed operand inside a UNION is a `QueryOperand.Select` whose `Select.Parentheses` is at least 1. |
| `Query.Ordered` | T-SQL | B | `Statement.Select.OrderBy`/`Offset`/`Fetch` | The concept is the standard's `<query expression>` with its order, but the old tree wraps and the new puts it in properties. No place for `For` (FOR XML/JSON/BROWSE). |
| `Query.Union` | SQL-92 | B | `SetOperation(Operator = Union)` in `Statement.Select.SetOperations` | Old binary tree (Left/Right) against a flat list on the first operand. Precedence (INTERSECT binds tighter) then needs brackets or nested `Select`s. `All` bool → `SetQuantifier?`. `Corresponding` is unused by T-SQL. |
| `Query.Except` | SQL-92 | B | `SetOperation(Operator = Except)` | As Union. |
| `Query.Intersect` | SQL-92 | B | `SetOperation(Operator = Intersect)` | As Union; precedence matters here. |
| `Query.DefaultValues` | T-SQL | A | `InsertSource.DefaultValues` | Standard `<from default>`; ast.md says T-SQL. |
| `Query.FromFile` | T-SQL | C | `Statement.BulkInsert` | BULK INSERT is built today as `Insert` with `Rows = FromFile`; in the new tree a statement of its own is cleaner. |
| `Query.FromStream` | T-SQL | C | `Statement.InsertBulk` | Columns carry types, so they need a `ColumnDefinition` list. |
| `Query.FromExecute` | T-SQL | C | `InsertSource.Execute` (T-SQL member of `InsertSource`) | Holds the T-SQL EXECUTE statement. |

---

## Expression (49 records)

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Expression.Or` | SQL-92 | A | `Expression.Binary(BinaryOperator.Or)` | |
| `Expression.And` | SQL-92 | A | `Expression.Binary(BinaryOperator.And)` | |
| `Expression.Not` | SQL-92 | A | `Expression.Unary(UnaryOperator.Not)` | |
| `Expression.IsTruth` | SQL-92 | A | `Expression.IsTruth` | `SqlTruth` → `BooleanLiteral`. |
| `Expression.Add` | SQL-92 | A | `Expression.Binary(Add)` | The T-SQL grammar builds `+` as `Add` whatever the operand types (`Expression.Additive`), so no typing decision is needed. |
| `Expression.Subtract` | SQL-92 | A | `Expression.Binary(Subtract)` | |
| `Expression.Concatenate` | SQL-92 | A | `Expression.Binary(Concatenate)` | T-SQL builds it only for `\|\|`. |
| `Expression.Multiply` | SQL-92 | A | `Expression.Binary(Multiply)` | |
| `Expression.Divide` | SQL-92 | A | `Expression.Binary(Divide)` | |
| `Expression.Negate` | SQL-92 | A | `Expression.Unary(Minus)` | |
| `Expression.Plus` | SQL-92 | A | `Expression.Unary(Plus)` | |
| `Expression.Modulo` | T-SQL | C | `Expression.Binary` + new `BinaryOperator.Modulo` | `Modulo` exists only in `JsonPathBinaryOperator` (verified). The precedence of `%` equals `*`. |
| `Expression.BitwiseAnd` | T-SQL | C | `Expression.Binary` + `BinaryOperator.BitwiseAnd` | Precedence equals `+`. |
| `Expression.BitwiseOr` | T-SQL | C | `Expression.Binary` + `BinaryOperator.BitwiseOr` | |
| `Expression.BitwiseXor` | T-SQL | C | `Expression.Binary` + `BinaryOperator.BitwiseXor` | |
| `Expression.BitwiseNot` | T-SQL | C | `Expression.Unary` + `UnaryOperator.BitwiseNot` | |
| `Expression.ShiftLeft` | T-SQL | C | `Expression.Binary` + `BinaryOperator.ShiftLeft` | |
| `Expression.ShiftRight` | T-SQL | C | `Expression.Binary` + `BinaryOperator.ShiftRight` | |
| `Expression.Comparison` | SQL-92 | A | `Expression.Comparison` | `ComparisonOperator` has no `NotEqualBang` (`!=`), `NotLess` (`!<`), or `NotGreater` (`!>`). `NotEqualBang` exists only in `JsonPathComparisonOperator`. |
| `Expression.Quantified` | SQL-92 | A | `Expression.QuantifiedComparison` | Quantifier string → `Quantifier` enum (All/Some/Any); `Query` → `Statement.Select`; operator as above. |
| `Expression.Between` | SQL-92 | A | `Expression.Between` | `Low`/`High` → `Lower`/`Upper`; `Symmetry` null. |
| `Expression.In` | SQL-92 | A | `Expression.In` | `Source Expression` (row or subquery) → `InSource.Values` / `InSource.Query`. |
| `Expression.Like` | SQL-92 | A | `Expression.Like(Kind = Like)` | |
| `Expression.IsNull` | SQL-92 | A | `Expression.IsNull` | |
| `Expression.Exists` | SQL-92 | A | `Expression.Exists` | |
| `Expression.Unique` | SQL-92 | A | `Expression.Unique` | |
| `Expression.Match` | SQL-92 | A | `Expression.Match` | `Qualifier` string → `UniqueKeyword` + `MatchType?`. |
| `Expression.GraphMatch` | T-SQL | C | `Expression.GraphMatch` | The pattern is kept as text. Requirement 19 would allow a `GraphPattern` family later. |
| `Expression.Overlaps` | SQL-92 | A | `Expression.Overlaps` | |
| `Expression.IsDistinctFrom` | SQL:1999 | A | `Expression.IsDistinct` | |
| `Expression.RoutineInvocation` | SQL-92 | B | `Expression.Invocation`, `Expression.Cast`, `Expression.Trim`, `Expression.Extract`, `Expression.Substring`, `Expression.Position`, `Expression.AtTimeZone`, … | One old record (`Name` string, `Arguments`, one `Word`: DISTINCT, a datetime field, trim spec, cast type) is split by function in the new tree, with `Argument` wrappers and a `QualifiedName`. There is nowhere for T-SQL built-ins whose arguments are not values: `CONVERT(type, v, style)`, `TRY_CAST`, `PARSE(v AS t USING c)`, `DATEADD(day, …)` date-part keywords, `IIF`, `CHOOSE`, `NEXT VALUE FOR … OVER`, `$PARTITION.f(x)`, `STRING_AGG … WITHIN GROUP`. The grammar also builds `AT TIME ZONE` as `RoutineInvocation("AT TIME ZONE")`, and in the new tree that is `Expression.AtTimeZone`. `Cast.Format` is a string. |
| `Expression.RowsetOrder` | T-SQL | C | Part of a T-SQL rowset table source (e.g. `TableSource.OpenRowset.Order`) | Not a value; it exists only because OPENROWSET's arguments are expressions. |
| `Expression.Case` | SQL-92 | A | `Expression.Case` | `Clause.When` → `CaseWhen` (a `When` list of one). |
| `Expression.ColumnReference` | SQL-92 | B | `Expression.Reference(QualifiedName)` | Old: text "dots and all". New: parts with `IdentifierStyle` (no bracket style). Pseudo-columns (`$action`, `$node_id`, `$IDENTITY`), `::` system functions and `a..b` empty parts need a representation. Decide whether every name rule builds `Identifier`s. |
| `Expression.Literal` | SQL-92 | B | `Expression.Literal(LiteralValue.*)`, `Expression.Parameter`, `Expression.Current`, `Expression.Default` | Old `SqlLiteralKind` + text also carries Parameter, Special and Default. See the `SqlLiteralKind` row. |
| `Expression.RowValueConstructor` | SQL-92 | A | `Expression.Row(Items, RowKeyword = false)` | |
| `Expression.NamedArgument` | T-SQL | A | `Argument(Value, Name, NamedAssignment)` | `Argument` is not an `Expression`, so every list that holds a `NamedArgument` today (`Execute.Arguments`, `Dbcc.Arguments`, rowset arguments) must become `IReadOnlyList<Argument>`. `NamedAssignment` means `=>`; T-SQL's `=` needs a spelling. `Name` string (`@p`, `FORMATFILE`) → `Identifier`. |
| `Expression.Aliased` | T-SQL | A | `Expression.TableArgument` (`Alias`) (uncertain) | `PREDICT (DATA = t AS d)` is a table argument with an alias. Whether PREDICT should be read as a polymorphic table function is uncertain. |
| `Expression.Pieced` | T-SQL | C | Field of `TableSource.OpenRowset` (provider, `server;user;password`) | Not a value. |
| `Expression.Prefixed` | T-SQL | C | Dissolve into typed rowset sources: `OpenRowset.Bulk`, `ChangeTable.Changes`, `ContainsTable.Language` | |
| `Expression.WindowFunction` | SQL:2003 | B | `Expression.Invocation.Over` (+ `Nulls`, `WithinGroup`) | Old wrapper (`Function`, `Within` text, `Over` Clause) against properties on the call. `Within` text (IGNORE NULLS, WITHIN GROUP) must be parsed into `NullTreatment?` / `WithinGroupClause`. The frame is text in `Clause.Window` and would become `WindowFrame`. |
| `Expression.Member` | T-SQL | A | `Expression.Member` | `By` string (`.`, `::`) → `MemberAccessKind` (Dot/StaticMethod); `Name` → `Identifier`; `Arguments` → `Argument` list. XML methods `x.value('…', 'int')` fit. |
| `Expression.Collated` | SQL-92 | A | `Expression.Collate` | `Collation` string → `CollationName`. |
| `Expression.Measured` | T-SQL | C | `Expression.Measured` (T-SQL) or unit enums on the options that use it | `10 MINUTES`, `50 PERCENT`, `4 GB`. |
| `Expression.Hinted` | T-SQL | C | Dissolve: JSON `NULL ON NULL` / `RETURNING` → `JsonObject.Nulls`/`Output` (exist); `GROUP BY … WITH (DISTRIBUTED_AGG)` → a T-SQL property on `GroupingElement` | Text today. |
| `Expression.JsonPair` | T-SQL | A | `JsonMember(Syntax = JsonMemberSyntax.Colon)` | `JsonMember` is not an `Expression`; T-SQL `JSON_OBJECT` must build `Expression.JsonObject` with members rather than a routine invocation with pairs. |
| `Expression.OdbcEscape` | T-SQL | C | `Expression.OdbcEscape` | `Kind` string (FN, d, t, ts, guid, interval…). |
| `Expression.Parenthesized` | SQL:2023 | A | `Expression.Parenthesized` | |
| `Expression.Subquery` | SQL-92 | A | `Expression.Subquery(Statement.Select)` | T-SQL subqueries carry `TOP … ORDER BY` / `FOR XML`, which needs those on `Statement.Select`. |

---

## TableReference (9 records)

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `TableReference.Named` | SQL-92 | A | `TableSource.Named` | `Table` string → `QualifiedName` (including a table variable `@t`, which is not a name). `SystemTime` → `SystemTimeSpecification` (no `CONTAINED IN (a, b)` or `ALL`). `Name`+`Columns` → `Alias`. `Sample` → the family's `Sample` (see `Clause.TableSample`). No place for `ForPath` (graph `FOR PATH`), `Hints` (`WITH (NOLOCK)`, and the old-style `(NOLOCK)` without WITH), or `Server` (`OPENDATASOURCE(…).db.s.t`). `Only` is unused. |
| `TableReference.Derived` | SQL-92 | A | `TableSource.Subquery` | `Name`+`Columns` → `Alias`. No place for `ForPath`. `Lateral` is unused. |
| `TableReference.Changed` | T-SQL | A | `TableSource.DataChange` (uncertain) | `DataChange(ResultOption Option, Statement Change, Alias)` is the standard's `FINAL/NEW/OLD TABLE (…)`. T-SQL's `(MERGE … OUTPUT …) AS c (cols)` writes no option, so `Option` would have to be nullable, and the rows are the OUTPUT's rather than the table's. |
| `TableReference.FunctionCall` | T-SQL | A | `TableSource.Function(Invocation, Alias)` | A plain TVF fits. No place for `Schema Clause[]` (OPENJSON/OPENXML/OPENROWSET `WITH (…)`), or column aliases beyond `Alias`. Rowset functions with non-value arguments do not fit `Invocation` (see Cross-cutting gaps). |
| `TableReference.Pivot` | T-SQL | C | `TableSource.Pivot` | `Aggregate` is an invocation; `For` is a column; `In` should be an `Identifier` list (bracketed values); alias. |
| `TableReference.Unpivot` | T-SQL | C | `TableSource.Unpivot` | |
| `TableReference.Joined` | SQL-92 | A | `TableSource.Join` | `SqlJoin` → `JoinKind?` (`Unspecified` → null). No value for `CrossApply`, `OuterApply` or `Union` (UNION JOIN). `Outer` → `OuterKeyword`; `On`/`Using` → `JoinSpecification`. No place for `Hint` (HASH/LOOP/MERGE/REMOTE). The grammar builds apply chains with a null `Left` first. |
| `TableReference.Parenthesized` | SQL-92 | A | `TableSource.Parenthesized` | |
| `TableReference.OdbcJoin` | T-SQL | C | `TableSource.OdbcJoin` | `{ OJ … }`. |

---

## Clause (37 records)

The new tree has no `Clause` root. Each of these becomes a property, a small record of no family,
or a member of another family.

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `Clause.DerivedColumn` | SQL-92 | A | `SelectItem.ExpressionItem(Expression, Alias, AsKeyword)` | No place for T-SQL's `alias = expr` form or a string alias `'x'` / `AS 'x'`. The old tree cannot tell them apart either (does the writer normalize?). Needs an alias-syntax enum. |
| `Clause.QualifiedAsterisk` | SQL-92 | A | `SelectItem.All` / `SelectItem.QualifiedAll` | `Qualifier` string → `Expression`. `$ROWGUID`/`$IDENTITY` are not asterisks. |
| `Clause.SortSpecification` | SQL-92 | A | `SortItem` | `SqlOrder` → `SortDirection?`. `NullOrdering` is unused by T-SQL. |
| `Clause.OrderBy` | SQL-92 | B | `OrderByClause` + `Statement.Select.Offset` (`OffsetClause`) + `.Fetch` (`FetchClause`) | Old: OFFSET/FETCH are two bare `Expression`s inside ORDER BY. New: separate clauses with required `RowWord` (ROW/ROWS), `FetchPosition` (FIRST/NEXT) and `FetchMode`, so the old tree loses words the new one needs. In a window (`OVER (ORDER BY …)`) only `OrderByClause` applies. |
| `Clause.Top` | T-SQL | C | `TopClause(Expression Value, bool Parentheses, bool Percent, bool WithTies)` as a property on `Statement.Select`, `Insert`, `Update`, `Delete`, `Merge` | Old `With` is a string (`TIES`) and should be a bool. Brackets (`TOP 10` against `TOP (10)`) are not kept today. |
| `Clause.Window` | SQL:2003 | A | `WindowSpecification` (via `WindowReference.Specification`) | `Name` → `Existing`; `By` → `OrderByClause`. `Frame` is text today, and `WindowFrame` (unit, extent, exclusion) needs the grammar to parse it. |
| `Clause.WindowDefinition` | SQL:2003 | A | `WindowDefinition` | |
| `Clause.SystemTime` | T-SQL | A | `SystemTimeSpecification` (AsOf, Between, FromTo) | `Kind` string → the subtypes. No `ContainedIn` or `All`. Standard `Between` has `Symmetry`, and T-SQL has none. |
| `Clause.Into` | T-SQL | C | `SelectIntoTable(QualifiedName Table, Identifier? FileGroup)` as a T-SQL property on `Statement.Select` | Not the standard `IntoClause`, which is variables. |
| `Clause.TableSample` | SQL:2003 | A | `SampleClause` (`TableSource.Sample`) | `Method` is a required `SampleMethod`, but T-SQL's `SYSTEM` is optional. No place for `Unit` (PERCENT/ROWS; the standard is percent only). `Repeatable` → `Repeat`. |
| `Clause.JsonColumn` | T-SQL | C | `RowsetColumn(Identifier Name, DataType? Type, string? Path, bool AsJson, int? Ordinal)` | Near `JsonTableColumn.Regular` but not the same (OPENXML table name form, BULK ordinal). |
| `Clause.GroupBy` | SQL-92 | B | `GroupByClause(Quantifier, Items)` | `All` bool → `SetQuantifier.All` (same word, different meaning in T-SQL). `By Expression[]` → `GroupingElement` (ROLLUP/CUBE/GROUPING SETS/`()` are routine invocations or expressions today). No place for `With` (`WITH CUBE` / `WITH ROLLUP` suffix). |
| `Clause.CommonTableExpression` | SQL:1999 | A | `CommonTableExpression` | `Name`, `Columns` → `Identifier`s; `Query` → `Statement.Select`. `WITH XMLNAMESPACES` has no place in `WithClause`. |
| `Clause.For` | T-SQL | C | `ForClause` family: `ForClause.Xml(Mode, Options)`, `ForClause.Json(Mode, Options)`, `ForClause.Browse`, as a T-SQL property on `Statement.Select` | Old: kind string + option strings. |
| `Clause.Hint` | T-SQL | C | `TableHint`, `QueryHint` (records or families), and a join-hint enum | Text today. `OPTION (…)` → `Statement.*.Options`; `WITH (NOLOCK)` → `TableSource.Named.Hints`. |
| `Clause.Option` | T-SQL | C | `Option(Identifier Name, Expression? Value, IReadOnlyList<Option> Options, …)` (a T-SQL record of no family) | The generic DDL option. Used by nearly every T-SQL DDL statement, and by `WITH XMLNAMESPACES` in the old `With`. |
| `Clause.Placement` | T-SQL | C | `Placement(PlacementKind Kind, Identifier Target, IReadOnlyList<Identifier>? Columns)` | ON / TEXTIMAGE_ON / FILESTREAM_ON. |
| `Clause.VariableAssignment` | T-SQL | C | `SelectItem.VariableAssignment(Variable, AssignmentOperator, Expression)` | A T-SQL member of `SelectItem`; needs a variable node. |
| `Clause.When` | SQL-92 | A | `CaseWhen(When list, Then)` | |
| `Clause.Set` | SQL-92 | B | `Assignment(Targets, Value, Parenthesized)` + `AssignmentTarget` | `Target` string → `QualifiedName`, but `@v` is a variable. No place for `Operator` (`+=`, `-=`, …), `.WRITE (…)` (empty operator, method call), or `Through` (`@v = col = expr`). |
| `Clause.Output` | T-SQL | C | `OutputClause(IReadOnlyList<SelectItem> Items, TableSource? Into, IReadOnlyList<Identifier>? Columns, OutputClause? Next)` | `inserted.*` / `deleted.*` / `$action` items. |
| `Clause.ExecutionContext` | T-SQL | C | `ExecutionContext(ExecutionContextKind Kind, Expression Name)` | Part of the T-SQL EXECUTE record. |
| `Clause.MergeWhen` | SQL:2003 | B | `MergeClause.Matched` / `MergeClause.NotMatched` | `By` string (BY TARGET / BY SOURCE) has no place. `WHEN NOT MATCHED BY SOURCE THEN UPDATE/DELETE` needs a matched-style action on a not-matched clause, while `NotMatched` holds only `MergeInsertAction`. `Action Statement` → `MergeMatchedAction` / `MergeInsertAction`. `INSERT DEFAULT VALUES` in a merge has no place (`MergeInsertAction.Values` is a list). |
| `Clause.VariableDeclaration` | SQL/PSM | C | `VariableDeclaration(Identifier Name, DataType? Type, Expression? Value, IReadOnlyList<TableElement>? Table, bool AsKeyword, …)` | Table variables with elements; `CURSOR` type; nullability. |
| `Clause.CursorDefinition` | T-SQL | B | `CursorProperties` + `CursorSource.Query` | See `Statement.DeclareCursor`. |
| `Clause.CursorFor` | T-SQL | A | `UpdatabilityClause(ReadOnly, UpdateColumns)` | Fits. |
| `Clause.PartitionRange` | T-SQL | C | `PartitionRange(Expression From, Expression? To)` on `Statement.TruncateTable` (and ALTER INDEX / TABLE) | |
| `Clause.ParameterDeclaration` | SQL/PSM | B | `ParameterDefinition(Mode, Name, Type, Result, Default, Locator)` | `Mode` stands before the name (IN/OUT/INOUT), while T-SQL writes `OUTPUT` / `OUT` after the default. No place for `READONLY`, `VARYING`, or `NULL`/`NOT NULL` (natively compiled modules). `Type` string → `DataType`. `@p` name → `Identifier`. `Ways` string[] holds OUTPUT/READONLY today. |
| `Clause.ColumnDefinition` | SQL-92 | B | `ColumnDefinition(Name, Type, Generation, Constraints, Collation)` | The old `Options Clause[]` keeps options, constraints and the inline index in **written order**; the new tree splits them into `Generation`, `Constraints` and `Collation`, which loses the order (requirement 3). `Computed` (`AS expr [PERSISTED]`) is close to `ColumnGeneration.Generated`, but T-SQL has no `GENERATED ALWAYS AS`. `Type` is nullable in both. |
| `Clause.ColumnOption` | T-SQL | B | Partly `Constraint.NotNull`, `ColumnDefinition.Collation`, `ColumnGeneration.Identity`/`Default`/`RowStart`/`RowEnd`; the rest has no place | `IDENTITY(1, 1)` is not `GENERATED … AS IDENTITY (START WITH 1)`. `NULL` (explicitly nullable) has no `Constraint`. No place for SPARSE, FILESTREAM, ROWGUIDCOL, MASKED WITH, ENCRYPTED WITH, NOT FOR REPLICATION, PERSISTED, or `GENERATED ALWAYS AS ROW START HIDDEN`. `ConstraintName` for a named NULL/DEFAULT. |
| `Clause.ConstraintDefinition` | SQL-92 | B | `Constraint.Unique` / `Constraint.ForeignKey` / `Constraint.Check` (+ `Name`, `Characteristics`) | `Kind` string → the subtypes, and `UniqueKind` covers PRIMARY KEY. No place for `DEFAULT … FOR col` (a named default constraint), inline `INDEX`, `Clustering` (CLUSTERED/NONCLUSTERED), `Hash`, `Columnstore`, `Order`, `Include`, `Filter`, `Options` (`WITH (…)`, bracketed or not), `Placements`, `NOT FOR REPLICATION` on CHECK, `ForColumn`, or `CONNECTION` for graph edges. `Columns` are sort specifications (ASC/DESC), while `Unique.Columns` is `Identifier`s. `Enforced` fits `ConstraintCharacteristics.Enforced`. |
| `Clause.References` | SQL-92 | A | `ReferencesSpecification` | `OnDelete`/`OnUpdate` strings → `ReferentialAction?` (all four T-SQL actions exist). No place for `NotForReplication`. `Table` string → `QualifiedName`. |
| `Clause.Connection` | T-SQL | C | `EdgeConnection(QualifiedName From, QualifiedName To)` | A graph edge constraint. |
| `Clause.Dropped` | T-SQL | B | `AlterTableAction.DropColumn` / `AlterTableAction.DropConstraint` | The standard drops one thing with a required `DropBehavior`. T-SQL `DROP COLUMN a, b, CONSTRAINT c` is a list mixing kinds, with `IF EXISTS`, and `WITH (ONLINE = ON)` / `PERIOD FOR SYSTEM_TIME` in `Tail`. |
| `Clause.DatabaseFile` | T-SQL | C | `DatabaseFile(IReadOnlyList<Option> Options)` | |
| `Clause.FileGroup` | T-SQL | C | `FileGroup(Identifier Name, …)` | |
| `Clause.EventPiece` | T-SQL | C | `EventSessionPiece(…)` | The predicate is text. |

---

## Enums and the record of no family

| Old node | Source | Cat | Target / proposal | What does not fit, notes |
| --- | --- | --- | --- | --- |
| `StatementCategory` | T-SQL (tree's own) | C | No node. A computed extension method `Category(this Statement)` outside the tree, or dropped | The new tree has nothing like it. A test reads ast.md's column against it today. |
| `SetCategory` | T-SQL | C | An enum on the proposed `SetOption` family | |
| `SqlComparison` | SQL-92 | A | `ComparisonOperator` | No `NotEqualBang`, `NotLess` or `NotGreater`. |
| `SqlJoin` | SQL-92 | A | `JoinKind?` | `Unspecified` → null. No `Union`, `CrossApply` or `OuterApply`. |
| `SqlOrder` | SQL-92 | A | `SortDirection?` | `Unspecified` → null. |
| `SqlLiteralKind` | SQL-92 | B | `LiteralValue` family + `Expression.Parameter` + `Expression.Current` + `Expression.Default` | `Number` → `LiteralValue.Numeric` with `NumericLiteralKind`, which must be classified (DecimalInteger/Decimal/Approximate) and has no **Money**. `Text`/`National` → `LiteralValue.String` with `StringLiteralKind`. `Bit` (SQL-92 `B'…'`) has no target (SQL:2023 removed bit strings). `Hex` (T-SQL `0x…` and standard `X'…'`) → `LiteralValue.Binary(Text)`, with no kind to separate the two spellings. **SQL:2023's `0xFF` is `NumericLiteralKind.HexInteger`, a number, and T-SQL's is a binary string**, so "the standard wins" cannot apply to both meanings of one spelling. `Date`/`Time`/`Timestamp` → `LiteralValue.DateTime`. `Interval` → `LiteralValue.Interval`, which needs an `IntervalQualifier`. `Null` → `LiteralValue.Null`. `Default` → `Expression.Default`. `Parameter` (`@x`, `@@x`, `?`, `:x`) → `Expression.Parameter` (see Cross-cutting gaps). `Special` (USER, CURRENT_USER, SESSION_USER, SYSTEM_USER, VALUE) → `Expression.Current` with `CurrentValue` (User, CurrentUser, SessionUser, SystemUser and Value all exist). |
| `SqlTruth` | SQL-92 | A | `BooleanLiteral` | |
| `Batch` (record, not a node) | T-SQL (client) | C | `Batch(IReadOnlyList<Statement> Statements, string? Go)` in `DotGram.Sql.Ast`, or a `Script` record | Not `ISqlSpan` today and not in the new tree. |
| `Syntax.TypeWritten` (private enum in `Syntax`) | none | none | Not part of the tree | An implementation detail of the old factories. |

---

## Cross-cutting gaps

**Identifier styles.** `IdentifierStyle { Regular, Delimited, UnicodeDelimited }` has no value for
T-SQL's `[x]` (with `]]` escaping). `"x"` is `Delimited` and unquoted names are `Regular`.
`#temp` and `##global` are regular-looking identifiers whose first character carries meaning (the
grammar reads them as `TSql.TemporaryName`), so a flag or style is needed. `Identifier.Text` keeps
quotes as written, so round trip works once a style exists. Every name rule in the T-SQL grammar
currently hands back a joined string (`ColumnReference.Text`, `Named.Table`, `Removal.Names` as
`ColumnReference`s), and all of them must build `QualifiedName` parts. `a..b` (empty middle part),
`server.db.schema.obj` and `$PARTITION` need representations. **Needs:
`IdentifierStyle.Bracketed`.**

**Variables `@x` and `@@x`.** `ParameterKind { Host, Sql, Dynamic, Embedded }` has no local
variable and no server "global" variable. The T-SQL grammar builds every variable as
`Literal(SqlLiteralKind.Parameter, "@x")` and `?`/`:x` the same way. Needed:
`ParameterKind.Variable` (and `ServerVariable` for `@@ROWCOUNT`, since the grammar already tells
them apart with `Syntax.IsServerVariable`), with the name as an `Identifier` whose text includes
`@`, or not (decide). `Parameter.Indicator` does not apply. Variables are also *targets*:
`SET @x`, `SELECT @x = …`, `FETCH … INTO @x`, `EXEC @rc = p`, `OUTPUT … INTO @t`, `@t` as a table
source. `AssignmentTarget` and `TableSource.Named` take `QualifiedName`, which cannot say it is a
variable.

**Money and binary/hex literals.** `NumericLiteralKind` has no money (`$12.50`, and the other
currency signs in `TSql.MoneyLiteral`). `LiteralValue.Binary(Text)` can hold `0x…`, but SQL:2023
defines `0x…` as `NumericLiteralKind.HexInteger`: a direct conflict of meaning for one spelling
(see `SqlLiteralKind`). T-SQL `N'…'` → `StringLiteralKind.National` fits.

**String concatenation with `+`.** There is no gap. The T-SQL grammar builds `+` as `Add` regardless
of operand types (`Expression.Additive`), and `||` as `Concatenate`; both map to
`Expression.Binary`. The old doc comment on `Expression.Concatenate` ("and T-SQL's `+` over
strings") does not describe what the grammar builds.

**TOP.** Nothing in the new tree. It is needed on `Statement.Select`, `Insert`, `Update`, `Delete`,
`Merge` and T-SQL `Receive`, as a `TopClause` (value, brackets, PERCENT, WITH TIES).

**OUTPUT.** Nothing in the new tree. It is needed on all four DML statements, with `INTO target
(cols)` and a second OUTPUT. Composable DML as a table source (`TableReference.Changed`) might
reuse `TableSource.DataChange` (uncertain; `ResultOption` is required there).

**Table, query and join hints; OPTION.** Nothing in the new tree: no `Hints` on
`TableSource.Named`, no join hint on `TableSource.Join`, no `Options` on any statement. The old
tree keeps all three as text (`Clause.Hint`). Whether they stay text or become structures is a
decision.

**FOR XML / FOR JSON / FOR BROWSE.** Nothing in the new tree. `Statement.Select.Updatability`
(`FOR UPDATE`) is the only `FOR` there, and it sits in the same position, so the grammar's `FOR`
alternatives meet there.

**SELECT … INTO.** The standard `IntoClause(Targets)` is the single-row select into variables. T-SQL
`INTO new_table [ON filegroup]` creates a table, a different concept, so it needs a T-SQL property.
(T-SQL's `SELECT @a = x` is `SelectItem`-level assignment, not INTO.)

**PIVOT / UNPIVOT.** No counterpart. `TableSource.RowPatternRecognition(Source, Clause, Alias)` is the
closest shape precedent: a postfix operator on a table source.

**CROSS / OUTER APPLY.** `JoinKind` has no apply values. Options are enum values `CrossApply` and
`OuterApply` (as the old tree does), or `Lateral` + `Join` (standard `LATERAL` exists on
`TableSource.Subquery` only, and APPLY accepts functions, not just subqueries).

**OPENROWSET / OPENJSON / OPENXML and the other rowset functions.** They fit `TableSource.Function`
only while their arguments are values. `OPENROWSET(BULK 'f', FORMATFILE = 'x', ORDER (c) UNIQUE)`,
`OPENROWSET('prov', 'srv'; 'u'; 'p', 'q')`, `OPENJSON(@j, '$.a') WITH (c int '$.c' AS JSON)`,
`OPENXML(@h, '/r', 2) WITH (…)`, `OPENQUERY(srv, 'q')`, `OPENDATASOURCE(…)`,
`CHANGETABLE(CHANGES t, @v)`, `CONTAINSTABLE`/`FREETEXTTABLE` and `PREDICT` hold `Prefixed`,
`Pieced`, `RowsetOrder`, `NamedArgument` and `JsonColumn` today. They need typed T-SQL table
sources, or a generic function source with a `WITH` schema. `JSON_TABLE`'s `JsonTableDefinition`
is a standard neighbour but not the same shape.

**GO and batches.** `Batch` (statements plus the `GO` line text) has no counterpart and is not a node.
It needs a home in `DotGram.Sql.Ast`, or it stays where it is with an `IReadOnlyList` of new
statements.

**Procedural statements.** The new tree covers Foundation, not SQL/PSM. It has `Statement.Return`
and `Statement.Call`, `TriggerAction.AtomicBlock` and `RoutineBody.Sql(Statement)`, but no block
(BEGIN…END), IF, WHILE, DECLARE of variables, variable assignment (SET @x), TRY…CATCH, PRINT,
RAISERROR, THROW, GOTO/label, BREAK/CONTINUE or WAITFOR. The PSM-sourced ones (Compound, If, While,
Declare, SetVariable, Return) should take PSM's names; the rest are T-SQL extensions. A routine body
that is a statement list (T-SQL's `AS s1; s2`) needs `Compound` or a list on `RoutineBody.Sql`.

**EXECUTE.** The new `Statement.Execute` is dynamic SQL's EXECUTE of a prepared statement, so the
name is taken. T-SQL EXECUTE (procedure call with named and OUTPUT arguments, return code, `;n`,
`AT server`, `WITH RESULT SETS`, a string run `AS USER`, bare procedure call) has no fit.
`Statement.Call` is closest for the procedure form, `Statement.ExecuteImmediate` for the string
form. `INSERT … EXECUTE` also needs an `InsertSource` member.

**SetExpression.** No family. Its two-level nesting (`SetExpression.Locking.LockTimeout`) breaks
requirements 6 and 7 and must flatten. Only `SET TRANSACTION ISOLATION LEVEL` has a standard target
(`Statement.SetTransaction`), and `IsolationLevel` has no `Snapshot`. Standard `SetTimeZone`,
`SetSchema` etc. are not T-SQL.

**Data types.** The old tree has none (strings). The new `DataType` has no length `MAX`
(`Character.Length` / `Binary.Length` are `int?`), no `TINYINT`, `BIT`, `MONEY`, `SMALLMONEY`,
`DATETIME`, `DATETIME2`, `SMALLDATETIME`, `DATETIMEOFFSET`, `UNIQUEIDENTIFIER`, `XML (schema)`,
`SQL_VARIANT`, `HIERARCHYID`, `GEOGRAPHY`/`GEOMETRY`, `ROWVERSION`/`TIMESTAMP` (T-SQL meaning),
`TEXT`/`NTEXT`/`IMAGE`, `CURSOR`, `TABLE`, `VECTOR`, or bracketed type names `[int]`.
`DataType.Extension(Dialect, Name, Arguments)` and `DataType.UserDefined` exist as escape
hatches. The T-SQL grammar also accepts `int(10)` etc., which needs arguments on any type.

**The DDL option catalogue.** About 140 T-SQL statements are `Definition`/`Removal` with `Tail` text.
The new tree has `Statement.Extension(string Dialect, string Kind, IReadOnlyList<ISqlNode> Parts)`
(also on `TableSource`, `Expression`, `AlterTableAction`, `DataType`), whose `Parts` cannot hold a
string tail. Deciding between typed T-SQL records and `Extension` settles most C rows at once.

**"Marked extension".** The new tree has no marker for a dialect node other than the
`Extension(Dialect, …)` records. The plan's "T-SQL additions as marked extensions" needs a
convention: a `// T-SQL:` comment beside the BNF comment (requirement 21), an attribute, a naming
rule, or `Extension` records only.

**Location support.** `ISqlNode : ISqlSpan` is the same `DotGram.Sql.ISqlSpan` the old roots
implement, so `LocationType = typeof(ISqlSpan)` and the `Located` parser keep working. Things to
carry over: `ISqlSpan`/`SqlSpan` live in `SqlSyntax.cs`, so they must survive its removal. In the
new tree *every* small record (`Identifier`, `Alias`, `SortItem`, `Argument`, …) is located, where
the old tree located only the six roots, so more `Locate` calls happen (the old measurement was
+14% time). `SqlWalker` finds children only through properties of an `ISqlSpan` type or **arrays**
(`type.IsArray`, SqlWalker.cs:93), so it walks nothing held in `IReadOnlyList<T>`, and
`Invocation.Copartition` is a list of lists. `Batch` is not located in either tree.

**Nodes the grammar builds with holes.** The T-SQL grammar builds some nodes with a `null!` left
side and fills it later (`Expression.Predicated`, APPLY chains, `AtTimeZone` with a `Collated(null!,
…)`). New records with `required` members and positional parameters allow the same `with` pattern,
but `init`-only properties on big records (`Statement.Select`) make "build then complete" copies
larger.

---

## Summary

### Counts per category per root

| Root | Records | A | B | C |
| --- | ---: | ---: | ---: | ---: |
| Statement | 246 | 11 | 23 | 212 |
| SetExpression | 20 | 1 | 0 | 19 |
| Query | 12 | 3 | 6 | 3 |
| Expression | 49 | 31 | 4 | 14 |
| TableReference | 9 | 6 | 0 | 3 |
| Clause | 37 | 11 | 10 | 16 |
| Enums | 7 | 4 | 1 | 2 |
| Batch | 1 | 0 | 0 | 1 |
| **Total** | **381** | **67** | **44** | **270** |

Counted from the tables above by a script. The private `Syntax.TypeWritten` is not counted.
Statement's C column includes the four SQL/PSM-sourced records with no counterpart (Compound, If,
While, Declare) and the 56 T-SQL DROP records that follow the DROP decision. Nine DROP records are
B. Of the 23 Statement B rows, 9 are DROP.

### The 15 most consequential B decisions, by how much of the grammar each touches

1. **Queries: the `Query` family against `Statement.Select` with `Body`/`SetOperations`/`Parentheses`**
   (`Statement.Select`, `Query.Specification`, `Query.Parenthesized`, `Query.Ordered`,
   `Query.Union/Except/Intersect`). Every SELECT, subquery, CTE, derived table, INSERT source, view
   body, cursor query and EXISTS/IN goes through it, and so does T-SQL's `TOP`/`ORDER BY`/`FOR`/`OPTION` placement.
2. **Names: text against `Identifier`/`QualifiedName` with a style** (`Expression.ColumnReference` and
   every `string` name in ~300 records). Every name rule, plus `[x]`, `#t`, `@v` as a name, and
   `a..b`.
3. **Literals and parameters: flat `SqlLiteralKind` against the `LiteralValue` family +
   `Parameter`/`Current`/`Default`** (`Expression.Literal`, `SqlLiteralKind`). Every value primary
   and every option value, plus the money kind and the `0x…` meaning conflict.
4. **Functions: one `RoutineInvocation` against `Invocation` + a record per keyword function**
   (`Expression.RoutineInvocation`). Every function call, the built-ins with keyword arguments
   (CONVERT, DATEADD, PARSE, TRIM, …), `Argument` wrappers, and AT TIME ZONE.
5. **Data types: strings against the `DataType` family** (inside `CreateTable`, `ColumnDefinition`,
   `ParameterDeclaration`, `VariableDeclaration`, CAST/CONVERT). Every DECLARE, CAST, column and
   parameter, plus the missing `MAX` and the T-SQL type names.
6. **DML statements: target and T-SQL parts** (`Statement.Insert/Update/Delete/Merge`). A
   `TableReference` target (alias, variable, hints) against `QualifiedName`/`TableTarget`; the second
   `FROM`; `WITH`, `TOP`, `OUTPUT`, `OPTION` on each.
7. **CREATE TABLE elements: an order-preserving `Clause[]` against split `ColumnDefinition` /
   `Constraint` / `ColumnGeneration`** (`Statement.TableDefinition`, `Clause.ColumnDefinition`,
   `Clause.ColumnOption`, `Clause.ConstraintDefinition`). Lossless order against typed properties;
   inline indexes, clustering, options, placements, IDENTITY(1,1).
8. **DROP: one name with a required `DropBehavior` against a name list with `IF EXISTS`**
   (`DropTable`, `DropView`, `DropSchema`, `DropSequence`, `DropType`, `DropRole`, `DropTrigger`,
   `DropFunction`, `DropProcedure`). Its precedent governs all 65 DROP records.
9. **Routines and triggers: `RoutineDefinition`/`ParameterDefinition`/`RoutineBody.Sql(Statement)`
   against T-SQL procedure/function/trigger shapes** (`CreateProcedure`, `CreateFunction`,
   `CreateTrigger`, `Clause.ParameterDeclaration`). A statement-list body, `OUTPUT`/`READONLY`
   after the parameter, `WITH` options, CREATE OR ALTER, trigger event lists and DDL triggers.
10. **ORDER BY / OFFSET / FETCH: inside `Clause.OrderBy` against separate `OffsetClause`/`FetchClause`
    with required words** (`Clause.OrderBy`). Every ordered query and window, plus the old tree
    not keeping ROW/ROWS/FIRST/NEXT.
11. **The EXECUTE statement against `Statement.Call` / `ExecuteImmediate`, and the name clash with
    the standard's `Statement.Execute`** (`Statement.Execute`, plus `Query.FromExecute`,
    `Expression.NamedArgument`). Procedure calls, INSERT…EXEC, and named/OUTPUT arguments.
12. **ALTER TABLE: `Action` string + element list against one `AlterTableAction`**
    (`Statement.AlterTable`, `Clause.Dropped`). All ALTER TABLE forms, multi-column ADD/DROP,
    SWITCH, SET options.
13. **Window functions: the `WindowFunction` wrapper with text frame against
    `Invocation.Over`/`WindowSpecification`/`WindowFrame`** (`Expression.WindowFunction`, with the A
    row `Clause.Window`). Every OVER clause; the frame grammar must start building structure.
14. **SET clause assignments: `Clause.Set` against `Assignment`/`AssignmentTarget`**
    (`Clause.Set`). Every UPDATE and MERGE UPDATE, compound operators, `.WRITE`, and
    `@v = col = expr`.
15. **GROUP BY: `All` + expression list + `WITH CUBE/ROLLUP` against
    `GroupByClause(Quantifier, GroupingElement)`** (`Clause.GroupBy`). Every grouped query and
    ROLLUP/CUBE/GROUPING SETS.

Next in line: `Statement.DeclareCursor`/`Clause.CursorDefinition` (T-SQL cursor options),
`Statement.Grant`/`Revoke` (permission and securable catalogue), `Statement.Transaction` (BEGIN/SAVE
TRAN, names), `Clause.MergeWhen` (BY SOURCE), `Statement.ViewDefinition` (body as statement,
options), `Statement.CreateTableAsSelect` (required `WithDataMode`), `Statement.TypeDefinition`, and
the `SqlLiteralKind`-level question of `0x…`.
