# The tree

`src/DotGram.Parsers/Sql/SqlSyntax.cs` holds one tree for every dialect this project reads —
`SqlStandard92`, `TransactSql`, and whatever comes after them. This is what is in it and
where each node comes from.

## The rule

**A node is called what the production it comes from is called.** The standard's name where
the standard has the concept, the dialect's published name where it does not. So a reader
with the specification open can find the node, a reader with the node can find the
specification, and a second dialect that reads the same production builds the same node
rather than one of its own. That is the point of having one tree at all.

Where a name repeats the hierarchy it is in, the repetition goes: the standard's
`<select statement>` is `Statement.Select` and its `<set clause>` is `Clause.Set`. Where it
does not, the published name stands whole — `Statement.TableDefinition`,
`Clause.SortSpecification`.

Five sources appear in the tables:

| Source | What it means |
| --- | --- |
| `SQL-92` | ISO/IEC 9075:1992, by section and production. The grammar cites the same numbers. |
| `SQL:1999`, `SQL:2003`, `SQL:2023` | A production a later standard has and 1992 did not — a trigger, a role, `MERGE`, `IS DISTINCT FROM`, a window. |
| `SQL/PSM` | ISO/IEC 9075-4, the procedural part: blocks, conditionals, loops, variables. |
| `T-SQL` | Microsoft's reference, by the page's own title. Everything a database engine has and a standard does not. |

## The shape

**A hierarchy per category the standard has, and no root above them.** `Statement`, `Query`,
`Expression`, `TableReference`, `Clause`. A tree with a single root types nothing: a field
of it accepts a statement where a value belongs, and the compiler cannot say otherwise. The
standard does not work that way — §7 puts a `<query expression>` where a table belongs and
§6 puts a `<value expression>` where a value belongs — so the roots are its own categories
and a field says which one it holds. `Statement.Insert.Rows` is a `Query`,
`Expression.Subquery.Query` is a `Query`, `Query.Specification.From` is a
`TableReference[]`.

**As many roots as there are sublanguages.** Five is what today's surface needs, not a
closed list. A JSON path, an XQuery inside `FOR XML`, the drawing inside `MATCH (…)` and a
full-text `CONTAINS` are each a language with a grammar of its own, and each will get a root
of its own when it is kept rather than read and dropped. Adding one breaks nothing, which is
the other half of having no universal base.

**And one record above them that is no node: `Batch`.** A script is a client's idea — the
lines that say `GO` are where a client cuts a file, and the server never sees them — so
`ParseScript` hands back a `Batch[]`, each holding the statements of one batch and the `GO`
line that ended it as it was written. It is not in the tables below because nothing in the
language produces it; `ParseSql` and `ParseStatement` never build one.

**Relations by aggregation, never by inheritance.** A subquery is not a kind of query; it is
an expression that holds one. A statement that returns rows is not a kind of query; it is a
statement that holds one. Nothing derives from anything but its own root, and no root
derives from another. Crossing between hierarchies is always a field.

**One level under each root.** A consumer switches over the descendants of the root it holds
and has seen all of them. A node under another node would be a node half its readers miss.

**A record per production.** A `DROP TABLE` and a `DROP VIEW` are not one shape with a word
in it; they are two statements spelled alike, and a consumer that reads the word to tell
them apart is doing the parser's work twice. The same goes for the operators: the standard
writes `<numeric value expression> ::= … <plus sign> <term>` as its own production, so
`Expression.Add` is a record and not an enum value, and each of the predicates of §8 is a
record with its operands named rather than numbered.

**A word only where the standard has a production for the word.** `<comp op>` is one, so a
comparison is one record carrying a `SqlComparison`; `<join type>` is another, so a join
carries a `SqlJoin`.

**And the tree never chooses a word the author did not write.** `count(*)` is not
`COUNT(*)`, `SELECT a` is not `SELECT ALL a`, and a bare `JOIN` is not an `INNER JOIN` — each
pair is two texts, and answering the second having read the first is deciding something
nobody asked the parser to decide. So a routine keeps the name it was called by, a set
quantifier is a word rather than a flag, and `SqlJoin.Unspecified` says that no word stood
there. What a missing `ALL` *means* is a question for whatever reads the tree, and
`Syntax.IsDistinct` is there for a reader who wants it answered.

The exception is a keyword that is pure syntax — `SELECT`, `CASE`, `FROM`. Those are a
formatter's to case, which is why ScriptDom's own generator has a `KeywordCasing` option.

**Fields stand in the order the text writes them.** `TableReference.Named(Table, SystemTime,
Name, Columns, Sample, Hints)` is the order of `t FOR SYSTEM_TIME … AS x (…) TABLESAMPLE …
WITH (…)`. That was a reading convenience until printing arrived; it is load-bearing now,
because a printer that walks the tree emits tokens in the order the fields stand in, and
that order has to be the source's.

**The grammar says the shape; this file says which node.** The `.gram` reads `DROP <what>`
once and hands the words to `Statement.Dropped`, which turns them into the record. So the
catalogue of names lives in C#, where a catalogue of C# names belongs, and the grammar still
says one thing. `Dropped`, `Defined`, `OfDatabase`, `Commanded`, `BackedUp`, `Restored` and
`Permitted` are those factories, and each throws where the grammar has read a word the tree
has no record for — which is the two catalogues having drifted, and a defect here rather
than in anybody's SQL.

**The tree prints back.** `SqlWriter.cs` writes any of the five roots out as SQL, which is
what makes the tree checkable rather than merely typed: `benchmarks --roundtrip` parses a
statement, prints it, and holds the result against what ScriptDom makes of the original. What
the tree does not hold cannot come back, so that measurement is also the list of what is still
missing.

**A node knows where it was written, where the grammar asked.**
`[Gram(…, LocationType = typeof(ISqlSpan))]` and the five roots implement `ISqlSpan`: the
reader offers each value the range of every rule it came out of, innermost first, and the
last offer is kept. That is a little wide where a rule hands back a value another rule made —
`WhereClause` lends the condition its `WHERE` — and never wrong, which is the safe direction.
It costs fourteen per cent of the parse and no allocation at all, and a grammar that does not
ask pays neither.

**But the tree still holds no text and no line numbers.** A span says where, in characters of
the input it was measured against; a consumer that wants the text cuts it from that input,
which is what §7.6 of `syntax.md` is for. What a span is *for* is the thing no tree can hold:
a comment falls between two spans, and the innermost node containing it is the one it belongs
to.

## What the tree does not keep

The reason a field is left out has not changed — a field nobody reads is a field that
drifts — but the list has, and it is short now. `TOP`, `OVER`, the table and query hints,
the windows, a named query's `WITH`, `OUTPUT` and the option lists of every DDL statement
were all read and dropped while nothing held the tree against anything. Something does:
`benchmarks --roundtrip` prints the tree back out and holds the result against what
ScriptDom makes of the same input, and a decoration that never reached the tree cannot come
back. So each of them is a field on a record here, which is what having a record each was
for.

What is still read and dropped is what says how something is *matched* rather than what it
is, and there are two: `CORRESPONDING` on a `UNION`, which names columns by matching rather
than by position and is a question for whatever resolves names; and the order written
inside a named query's body, since a `WITH` defines a table and a table has no order until
something asks for one.

## Walking it

`SqlWalker.Walk(root, visit)` hands every node under `root` to `visit` — the root first, each
node before what it holds — until `visit` answers false, and says whether it went to the end.
A node is anything that is an `ISqlSpan`, which is what the five roots are; what a record
holds of them, one or an array, is found from its type once, so a record added here is walked
without the walker being told.

It is how a question the parser does not answer gets asked. The parser reads what may be
written; whether an option was written twice, or a table somebody forbids was named, is a
check, and a check is a lambda that matches the nodes it is about:

```csharp
SqlWalker.Walk(statement, node =>
{
    if (node is Clause.ConstraintDefinition { Options: { } options } &&
        options.OfType<Clause.Option>().GroupBy(o => (o.Name, o.Partitions)).Any(g => g.Count() > 1))
        problems.Add(node);

    return true;
});
```

A check sees what the tree keeps and nothing else. Where a statement keeps what followed its
name as `Tail`, the words are there and their parts are not — unless the statement keeps its
settings as `Options` beside them, which the keys and certificates do: a repeated `SUBJECT`
or `ALGORITHM` is what the engine refuses there, and a list nested in one of them, a private
key's or an Always Encrypted value's, is an option holding it.

## The nodes

### `Statement`

The last column is the group the statement says it belongs to, its `Category`, a
`StatementCategory`: a query, rows changed (`Dml`), an object defined, changed or removed
(`Ddl`), a permission or the principal it names (`Dcl`), the flow of a batch (`Control`), a
transaction, the session, an `EXECUTE`, the server looked after (`Admin`), or a variable
declared or set. It is abstract on `Statement`, so a record added without one does not
compile, and a test reads this column against every record.

| Node | Source | Production or page | Category |
| --- | --- | --- | --- |
| `Statement.Select` | SQL-92 | §19.6 &lt;direct select statement: multiple rows&gt; — a query and its order | Query |
| `Statement.Insert` | SQL-92 | §13.8 &lt;insert statement&gt; | Dml |
| `Statement.Update` | SQL-92 | §13.10 &lt;update statement: searched&gt;, widened by T-SQL's second `FROM` | Dml |
| `Statement.Delete` | SQL-92 | §13.7 &lt;delete statement: searched&gt;, likewise | Dml |
| `Statement.Merge` | SQL:2003 | §14.9 &lt;merge statement&gt; | Dml |
| `Statement.Compound` | SQL/PSM | &lt;compound statement&gt; — T-SQL writes it without the label | Control |
| `Statement.If` | SQL/PSM | &lt;if statement&gt; | Control |
| `Statement.While` | SQL/PSM | &lt;while statement&gt; | Control |
| `Statement.TryCatch` | T-SQL | TRY...CATCH | Control |
| `Statement.Declare` | SQL/PSM | &lt;SQL variable declaration&gt; | Declaration |
| `Statement.Transaction` | SQL-92 | §13.2 &lt;commit statement&gt;, §13.3 &lt;rollback statement&gt;, and T-SQL's `BEGIN`/`SAVE` | Transaction |
| `Statement.Execute` | T-SQL | EXECUTE, and a procedure called without the word as a batch's first statement | Execute |
| `Statement.TableDefinition` | SQL-92 | §11.3 &lt;table definition&gt; | Ddl |
| `Statement.CreateTableAsSelect` | T-SQL | CREATE TABLE AS SELECT, and the external table written the same way | Ddl |
| `Statement.AlterTable` | SQL-92 | §11.10 &lt;alter table statement&gt; | Ddl |
| `Statement.CreateProcedure` | SQL/PSM | &lt;SQL-invoked procedure&gt;; T-SQL: CREATE PROCEDURE | Ddl |
| `Statement.CreateFunction` | SQL/PSM | &lt;SQL-invoked function&gt;; T-SQL: CREATE FUNCTION | Ddl |
| `Statement.CreateTrigger` | SQL:1999 | &lt;trigger definition&gt;; T-SQL: CREATE TRIGGER | Ddl |
| `Statement.ViewDefinition` | SQL-92 | §11.19 &lt;view definition&gt; | Ddl |
| `Statement.CreateIndex` | T-SQL | CREATE INDEX — the standard has no index — and its XML, columnstore and spatial kinds | Ddl |
| `Statement.AlterIndex` | T-SQL | ALTER INDEX | Ddl |
| `Statement.StatisticsDefinition` | T-SQL | CREATE STATISTICS | Ddl |
| `Statement.UpdateStatistics` | T-SQL | UPDATE STATISTICS | Admin |
| `Statement.Print` | T-SQL | PRINT | Control |
| `Statement.Return` | SQL/PSM | &lt;return statement&gt;; T-SQL: RETURN | Control |
| `Statement.Throw` | T-SQL | THROW | Control |
| `Statement.GoTo` | T-SQL | GOTO | Control |
| `Statement.Break` | T-SQL | BREAK | Control |
| `Statement.Continue` | T-SQL | CONTINUE | Control |
| `Statement.Checkpoint` | T-SQL | CHECKPOINT | Admin |
| `Statement.Use` | T-SQL | USE | Session |
| `Statement.RaiseError` | T-SQL | RAISERROR | Control |
| `Statement.WaitFor` | T-SQL | WAITFOR | Control |
| `Statement.ExecuteAs` | T-SQL | EXECUTE AS | Session |
| `Statement.Revert` | T-SQL | REVERT | Session |
| `Statement.DeclareCursor` | T-SQL | DECLARE CURSOR | Declaration |
| `Statement.SetCursor` | T-SQL | SET @local_variable — a cursor variable set to `CURSOR … FOR …` | Declaration |
| `Statement.CursorAction` | T-SQL | OPEN, CLOSE, DEALLOCATE | Control |
| `Statement.Fetch` | T-SQL | FETCH | Control |
| `Statement.Dbcc` | T-SQL | DBCC — every command, what stands in its brackets and its options | Admin |
| `Statement.BeginDialog` | T-SQL | BEGIN DIALOG CONVERSATION | Control |
| `Statement.ConversationTimer` | T-SQL | BEGIN CONVERSATION TIMER | Control |
| `Statement.EndConversation` | T-SQL | END CONVERSATION | Control |
| `Statement.MoveConversation` | T-SQL | MOVE CONVERSATION | Control |
| `Statement.Send` | T-SQL | SEND | Dml |
| `Statement.Receive` | T-SQL | RECEIVE | Dml |
| `Statement.GetConversationGroup` | T-SQL | GET CONVERSATION GROUP | Dml |
| `Statement.WaitForStatement` | T-SQL | WAITFOR — around RECEIVE or GET CONVERSATION GROUP, and its TIMEOUT | Control |
| `Statement.Truncate` | T-SQL | TRUNCATE TABLE | Ddl |
| `Statement.TriggerSwitch` | T-SQL | ENABLE TRIGGER, DISABLE TRIGGER | Ddl |
| `Statement.Kill` | T-SQL | KILL, KILL QUERY NOTIFICATION SUBSCRIPTION, KILL STATS JOB | Admin |
| `Statement.Classification` | T-SQL | ADD SENSITIVITY CLASSIFICATION | Ddl |
| `Statement.AddSignature` | T-SQL | ADD SIGNATURE, ADD COUNTER SIGNATURE | Ddl |
| `Statement.ReadText` | T-SQL | READTEXT | Dml |
| `Statement.WriteText` | T-SQL | WRITETEXT | Dml |
| `Statement.UpdateText` | T-SQL | UPDATETEXT | Dml |
| `Statement.Reconfigure` | T-SQL | RECONFIGURE | Admin |
| `Statement.SetUser` | T-SQL | SETUSER | Session |
| `Statement.CreateLogin` | T-SQL | CREATE LOGIN | Dcl |
| `Statement.AlterLogin` | T-SQL | ALTER LOGIN | Dcl |
| `Statement.CreateUser` | T-SQL | CREATE USER | Dcl |
| `Statement.AlterUser` | T-SQL | ALTER USER | Dcl |
| `Statement.CreateRole` | SQL:1999 | &lt;role definition&gt;; T-SQL: CREATE ROLE, CREATE SERVER ROLE | Dcl |
| `Statement.AlterRole` | T-SQL | ALTER ROLE, ALTER SERVER ROLE | Dcl |
| `Statement.CreateServerRole` | T-SQL | CREATE SERVER ROLE | Dcl |
| `Statement.AlterServerRole` | T-SQL | ALTER SERVER ROLE | Dcl |
| `Statement.CreateApplicationRole` | T-SQL | CREATE APPLICATION ROLE | Dcl |
| `Statement.AlterApplicationRole` | T-SQL | ALTER APPLICATION ROLE | Dcl |
| `Statement.SchemaDefinition` | SQL-92 | §11.1 &lt;schema definition&gt; | Ddl |
| `Statement.AlterSchema` | T-SQL | ALTER SCHEMA | Ddl |
| `Statement.AlterAuthorization` | T-SQL | ALTER AUTHORIZATION | Dcl |
| `Statement.ExternalDataSourceDefinition` | T-SQL | CREATE/ALTER EXTERNAL DATA SOURCE | Ddl |
| `Statement.ExternalFileFormatDefinition` | T-SQL | CREATE EXTERNAL FILE FORMAT | Ddl |
| `Statement.ExternalLibraryDefinition` | T-SQL | CREATE/ALTER EXTERNAL LIBRARY | Ddl |
| `Statement.ExternalResourcePoolDefinition` | T-SQL | CREATE/ALTER EXTERNAL RESOURCE POOL | Ddl |
| `Statement.ResourcePoolDefinition` | T-SQL | CREATE/ALTER RESOURCE POOL | Ddl |
| `Statement.WorkloadGroupDefinition` | T-SQL | CREATE/ALTER WORKLOAD GROUP | Ddl |
| `Statement.ServerAuditDefinition` | T-SQL | CREATE/ALTER SERVER AUDIT | Ddl |
| `Statement.AuditSpecificationDefinition` | T-SQL | CREATE/ALTER SERVER AUDIT SPECIFICATION | Ddl |
| `Statement.DatabaseAuditSpecificationDefinition` | T-SQL | CREATE/ALTER DATABASE AUDIT SPECIFICATION | Ddl |
| `Statement.EventSessionDefinition` | T-SQL | CREATE/ALTER EVENT SESSION | Ddl |
| `Statement.EventNotificationDefinition` | T-SQL | CREATE EVENT NOTIFICATION | Ddl |
| `Statement.MessageTypeDefinition` | T-SQL | CREATE, ALTER MESSAGE TYPE | Ddl |
| `Statement.ContractDefinition` | T-SQL | CREATE CONTRACT | Ddl |
| `Statement.QueueDefinition` | T-SQL | CREATE, ALTER QUEUE | Ddl |
| `Statement.ServiceDefinition` | T-SQL | CREATE, ALTER SERVICE | Ddl |
| `Statement.RouteDefinition` | T-SQL | CREATE, ALTER ROUTE | Ddl |
| `Statement.RemoteServiceBindingDefinition` | T-SQL | CREATE, ALTER REMOTE SERVICE BINDING | Ddl |
| `Statement.BrokerPriorityDefinition` | T-SQL | CREATE, ALTER BROKER PRIORITY | Ddl |
| `Statement.AlterResourceGovernor` | T-SQL | ALTER RESOURCE GOVERNOR | Admin |
| `Statement.AlterServerConfiguration` | T-SQL | ALTER SERVER CONFIGURATION | Admin |
| `Statement.OpenSymmetricKey` | T-SQL | OPEN SYMMETRIC KEY | Session |
| `Statement.OpenMasterKey` | T-SQL | OPEN MASTER KEY | Session |
| `Statement.CloseSymmetricKey` | T-SQL | CLOSE SYMMETRIC KEY | Session |
| `Statement.CloseAllSymmetricKeys` | T-SQL | CLOSE ALL SYMMETRIC KEYS | Session |
| `Statement.CloseMasterKey` | T-SQL | CLOSE MASTER KEY | Session |
| `Statement.PartitionFunctionDefinition` | T-SQL | CREATE PARTITION FUNCTION | Ddl |
| `Statement.AlterPartitionFunction` | T-SQL | ALTER PARTITION FUNCTION | Ddl |
| `Statement.PartitionSchemeDefinition` | T-SQL | CREATE PARTITION SCHEME | Ddl |
| `Statement.AlterPartitionScheme` | T-SQL | ALTER PARTITION SCHEME | Ddl |
| `Statement.SequenceDefinition` | T-SQL | CREATE, ALTER SEQUENCE | Ddl |
| `Statement.TypeDefinition` | T-SQL | CREATE TYPE | Ddl |
| `Statement.XmlSchemaCollectionDefinition` | T-SQL | CREATE XML SCHEMA COLLECTION | Ddl |
| `Statement.AlterXmlSchemaCollection` | T-SQL | ALTER XML SCHEMA COLLECTION | Ddl |
| `Statement.SynonymDefinition` | T-SQL | CREATE SYNONYM | Ddl |
| `Statement.AssemblyDefinition` | T-SQL | CREATE, ALTER ASSEMBLY | Ddl |
| `Statement.CryptographicProviderDefinition` | T-SQL | CREATE, ALTER CRYPTOGRAPHIC PROVIDER | Ddl |
| `Statement.ExternalLanguageDefinition` | T-SQL | CREATE, ALTER EXTERNAL LANGUAGE | Ddl |
| `Statement.RuleDefinition` | T-SQL | CREATE RULE | Ddl |
| `Statement.DefaultDefinition` | T-SQL | CREATE DEFAULT | Ddl |
| `Statement.AggregateDefinition` | T-SQL | CREATE AGGREGATE | Ddl |
| `Statement.EndpointDefinition` | T-SQL | CREATE/ALTER ENDPOINT | Ddl |
| `Statement.CreateDatabase` | T-SQL | CREATE DATABASE — the standard has no database | Ddl |
| `Statement.AlterDatabaseSet` | T-SQL | ALTER DATABASE … SET | Ddl |
| `Statement.AlterDatabaseScopedConfiguration` | T-SQL | ALTER DATABASE … SCOPED CONFIGURATION | Ddl |
| `Statement.AlterDatabaseCollate` | T-SQL | ALTER DATABASE … COLLATE | Ddl |
| `Statement.AlterDatabaseModifyName` | T-SQL | ALTER DATABASE … MODIFY NAME | Ddl |
| `Statement.AlterDatabaseModifyFileGroup` | T-SQL | ALTER DATABASE … MODIFY FILE GROUP | Ddl |
| `Statement.AlterDatabaseModifyFile` | T-SQL | ALTER DATABASE … MODIFY FILE | Ddl |
| `Statement.AlterDatabaseModify` | T-SQL | ALTER DATABASE … MODIFY | Ddl |
| `Statement.AlterDatabaseAddFileGroup` | T-SQL | ALTER DATABASE … ADD FILE GROUP | Ddl |
| `Statement.AlterDatabaseAddLogFile` | T-SQL | ALTER DATABASE … ADD LOG FILE | Ddl |
| `Statement.AlterDatabaseAddFile` | T-SQL | ALTER DATABASE … ADD FILE | Ddl |
| `Statement.AlterDatabaseRemoveFileGroup` | T-SQL | ALTER DATABASE … REMOVE FILE GROUP | Ddl |
| `Statement.AlterDatabaseRemoveFile` | T-SQL | ALTER DATABASE … REMOVE FILE | Ddl |
| `Statement.AlterDatabaseRebuildLog` | T-SQL | ALTER DATABASE … REBUILD LOG | Ddl |
| `Statement.AlterDatabasePerformCutover` | T-SQL | ALTER DATABASE … PERFORM CUTOVER | Ddl |
| `Statement.SetStatement` | T-SQL | the SET statements: what follows SET, as `SetExpression`s | Session |
| `Statement.SetVariable` | T-SQL | SET @local_variable | Declaration |
| `Statement.Grant` | SQL-92 | §12.1 &lt;grant statement&gt; | Dcl |
| `Statement.Deny` | T-SQL | DENY | Dcl |
| `Statement.Revoke` | SQL-92 | §12.2 &lt;revoke statement&gt; | Dcl |
| `Statement.FullTextIndexDefinition` | T-SQL | CREATE FULLTEXT INDEX | Ddl |
| `Statement.AlterFullTextIndex` | T-SQL | ALTER FULLTEXT INDEX | Ddl |
| `Statement.FullTextCatalogDefinition` | T-SQL | CREATE FULLTEXT CATALOG | Ddl |
| `Statement.AlterFullTextCatalog` | T-SQL | ALTER FULLTEXT CATALOG | Ddl |
| `Statement.FullTextStopListDefinition` | T-SQL | CREATE FULLTEXT STOPLIST | Ddl |
| `Statement.AlterFullTextStopList` | T-SQL | ALTER FULLTEXT STOPLIST | Ddl |
| `Statement.SearchPropertyListDefinition` | T-SQL | CREATE SEARCH PROPERTY LIST | Ddl |
| `Statement.AlterSearchPropertyList` | T-SQL | ALTER SEARCH PROPERTY LIST | Ddl |
| `Statement.BackupDatabase` | T-SQL | BACKUP DATABASE | Admin |
| `Statement.BackupTransactionLog` | T-SQL | BACKUP LOG | Admin |
| `Statement.BackupServer` | T-SQL | BACKUP SERVER | Admin |
| `Statement.BackupGroup` | T-SQL | BACKUP GROUP | Admin |
| `Statement.BackupCertificate` | T-SQL | BACKUP CERTIFICATE | Admin |
| `Statement.BackupMasterKey` | T-SQL | BACKUP MASTER KEY | Admin |
| `Statement.BackupServiceMasterKey` | T-SQL | BACKUP SERVICE MASTER KEY | Admin |
| `Statement.BackupSymmetricKey` | T-SQL | BACKUP SYMMETRIC KEY | Admin |
| `Statement.RestoreDatabase` | T-SQL | RESTORE DATABASE | Admin |
| `Statement.RestoreLog` | T-SQL | RESTORE LOG | Admin |
| `Statement.RestoreFileListOnly` | T-SQL | RESTORE FILELISTONLY | Admin |
| `Statement.RestoreHeaderOnly` | T-SQL | RESTORE HEADERONLY | Admin |
| `Statement.RestoreLabelOnly` | T-SQL | RESTORE LABELONLY | Admin |
| `Statement.RestoreRewindOnly` | T-SQL | RESTORE REWINDONLY | Admin |
| `Statement.RestoreVerifyOnly` | T-SQL | RESTORE VERIFYONLY | Admin |
| `Statement.RestoreMasterKey` | T-SQL | RESTORE MASTER KEY | Admin |
| `Statement.RestoreServiceMasterKey` | T-SQL | RESTORE SERVICE MASTER KEY | Admin |
| `Statement.RestoreSymmetricKey` | T-SQL | RESTORE SYMMETRIC KEY | Admin |
| `Statement.AsymmetricKeyDefinition` | T-SQL | CREATE ASYMMETRIC KEY | Ddl |
| `Statement.AlterAsymmetricKey` | T-SQL | ALTER ASYMMETRIC KEY | Ddl |
| `Statement.SymmetricKeyDefinition` | T-SQL | CREATE SYMMETRIC KEY | Ddl |
| `Statement.AlterSymmetricKey` | T-SQL | ALTER SYMMETRIC KEY | Ddl |
| `Statement.CertificateDefinition` | T-SQL | CREATE CERTIFICATE | Ddl |
| `Statement.AlterCertificate` | T-SQL | ALTER CERTIFICATE | Ddl |
| `Statement.MasterKeyDefinition` | T-SQL | CREATE MASTER KEY | Ddl |
| `Statement.AlterMasterKey` | T-SQL | ALTER MASTER KEY | Ddl |
| `Statement.AlterServiceMasterKey` | T-SQL | ALTER SERVICE MASTER KEY | Ddl |
| `Statement.DatabaseEncryptionKeyDefinition` | T-SQL | CREATE DATABASE ENCRYPTION KEY | Ddl |
| `Statement.AlterDatabaseEncryptionKey` | T-SQL | ALTER DATABASE ENCRYPTION KEY | Ddl |
| `Statement.ColumnEncryptionKeyDefinition` | T-SQL | CREATE COLUMN ENCRYPTION KEY | Ddl |
| `Statement.AlterColumnEncryptionKey` | T-SQL | ALTER COLUMN ENCRYPTION KEY | Ddl |
| `Statement.ColumnMasterKeyDefinition` | T-SQL | CREATE COLUMN MASTER KEY | Ddl |
| `Statement.CredentialDefinition` | T-SQL | CREATE/ALTER CREDENTIAL | Ddl |
| `Statement.DatabaseScopedCredentialDefinition` | T-SQL | CREATE/ALTER DATABASE SCOPED CREDENTIAL | Ddl |
| `Statement.SecurityPolicyDefinition` | T-SQL | CREATE/ALTER SECURITY POLICY | Ddl |
| `Statement.DropAggregate` | T-SQL | DROP AGGREGATE | Ddl |
| `Statement.DropApplicationRole` | T-SQL | DROP APPLICATION ROLE | Dcl |
| `Statement.DropAvailabilityGroup` | T-SQL | DROP AVAILABILITY GROUP | Ddl |
| `Statement.DropBrokerPriority` | T-SQL | DROP BROKER PRIORITY | Ddl |
| `Statement.DropCertificate` | T-SQL | DROP CERTIFICATE | Ddl |
| `Statement.DropColumnEncryptionKey` | T-SQL | DROP COLUMN ENCRYPTION KEY | Ddl |
| `Statement.DropColumnMasterKey` | T-SQL | DROP COLUMN MASTER KEY | Ddl |
| `Statement.DropContract` | T-SQL | DROP CONTRACT | Ddl |
| `Statement.DropCredential` | T-SQL | DROP CREDENTIAL | Ddl |
| `Statement.DropCryptographicProvider` | T-SQL | DROP CRYPTOGRAPHIC PROVIDER | Ddl |
| `Statement.DropDatabaseAuditSpecification` | T-SQL | DROP DATABASE AUDIT SPECIFICATION | Ddl |
| `Statement.DropDatabaseScopedCredential` | T-SQL | DROP DATABASE SCOPED CREDENTIAL | Ddl |
| `Statement.DropDatabase` | T-SQL | DROP DATABASE | Ddl |
| `Statement.DropDefault` | T-SQL | DROP DEFAULT | Ddl |
| `Statement.DropEndpoint` | T-SQL | DROP ENDPOINT | Ddl |
| `Statement.DropExternalDataSource` | T-SQL | DROP EXTERNAL DATA SOURCE | Ddl |
| `Statement.DropExternalFileFormat` | T-SQL | DROP EXTERNAL FILE FORMAT | Ddl |
| `Statement.DropExternalLanguage` | T-SQL | DROP EXTERNAL LANGUAGE | Ddl |
| `Statement.DropExternalModel` | T-SQL | DROP EXTERNAL MODEL | Ddl |
| `Statement.DropExternalResourcePool` | T-SQL | DROP EXTERNAL RESOURCE POOL | Ddl |
| `Statement.DropExternalTable` | T-SQL | DROP EXTERNAL TABLE | Ddl |
| `Statement.DropFulltextCatalog` | T-SQL | DROP FULLTEXT CATALOG | Ddl |
| `Statement.DropFulltextStoplist` | T-SQL | DROP FULLTEXT STOPLIST | Ddl |
| `Statement.DropFunction` | T-SQL | DROP FUNCTION | Ddl |
| `Statement.DropLogin` | T-SQL | DROP LOGIN | Dcl |
| `Statement.DropMessageType` | T-SQL | DROP MESSAGE TYPE | Ddl |
| `Statement.DropPartitionFunction` | T-SQL | DROP PARTITION FUNCTION | Ddl |
| `Statement.DropPartitionScheme` | T-SQL | DROP PARTITION SCHEME | Ddl |
| `Statement.DropProcedure` | T-SQL | DROP PROCEDURE | Ddl |
| `Statement.DropQueue` | T-SQL | DROP QUEUE | Ddl |
| `Statement.DropRemoteServiceBinding` | T-SQL | DROP REMOTE SERVICE BINDING | Ddl |
| `Statement.DropResourcePool` | T-SQL | DROP RESOURCE POOL | Ddl |
| `Statement.DropRole` | T-SQL | DROP ROLE | Dcl |
| `Statement.DropRoute` | T-SQL | DROP ROUTE | Ddl |
| `Statement.DropRule` | T-SQL | DROP RULE | Ddl |
| `Statement.DropSchema` | T-SQL | DROP SCHEMA | Ddl |
| `Statement.DropSearchPropertyList` | T-SQL | DROP SEARCH PROPERTY LIST | Ddl |
| `Statement.DropSecurityPolicy` | T-SQL | DROP SECURITY POLICY | Ddl |
| `Statement.DropSequence` | T-SQL | DROP SEQUENCE | Ddl |
| `Statement.DropServerAuditSpecification` | T-SQL | DROP SERVER AUDIT SPECIFICATION | Ddl |
| `Statement.DropServerAudit` | T-SQL | DROP SERVER AUDIT | Ddl |
| `Statement.DropServerRole` | T-SQL | DROP SERVER ROLE | Dcl |
| `Statement.DropService` | T-SQL | DROP SERVICE | Ddl |
| `Statement.DropStatistics` | T-SQL | DROP STATISTICS | Ddl |
| `Statement.DropSynonym` | T-SQL | DROP SYNONYM | Ddl |
| `Statement.DropTable` | T-SQL | DROP TABLE | Ddl |
| `Statement.DropType` | T-SQL | DROP TYPE | Ddl |
| `Statement.DropUser` | T-SQL | DROP USER | Dcl |
| `Statement.DropView` | T-SQL | DROP VIEW | Ddl |
| `Statement.DropWorkloadClassifier` | T-SQL | DROP WORKLOAD CLASSIFIER | Ddl |
| `Statement.DropWorkloadGroup` | T-SQL | DROP WORKLOAD GROUP | Ddl |
| `Statement.DropXmlSchemaCollection` | T-SQL | DROP XML SCHEMA COLLECTION | Ddl |
| `Statement.DropAsymmetricKey` | T-SQL | DROP ASYMMETRIC KEY | Ddl |
| `Statement.DropSymmetricKey` | T-SQL | DROP SYMMETRIC KEY | Ddl |
| `Statement.DropAssembly` | T-SQL | DROP ASSEMBLY | Ddl |
| `Statement.DropExternalLibrary` | T-SQL | DROP EXTERNAL LIBRARY | Ddl |
| `Statement.DropEventSession` | T-SQL | DROP EVENT SESSION | Ddl |
| `Statement.DropEventNotification` | T-SQL | DROP EVENT NOTIFICATION | Ddl |
| `Statement.DropFulltextIndex` | T-SQL | DROP FULLTEXT INDEX | Ddl |
| `Statement.DropIndex` | T-SQL | DROP INDEX | Ddl |
| `Statement.DropSignature` | T-SQL | DROP SIGNATURE | Ddl |
| `Statement.DropSensitivityClassification` | T-SQL | DROP SENSITIVITY CLASSIFICATION | Ddl |
| `Statement.DropTrigger` | T-SQL | DROP TRIGGER | Ddl |
| `Statement.DropMasterKey` | T-SQL | DROP MASTER KEY | Ddl |
| `Statement.DropDatabaseEncryptionKey` | T-SQL | DROP DATABASE ENCRYPTION KEY | Ddl |

### `SetExpression`

What follows `SET` in a SET statement, a root of its own: the groups Microsoft's page of the
SET statements puts the settings in, each group with the settings of its own shapes. The group
is the node's `Category`, a `SetCategory`, and a statement's `SetCategory` is its settings'
together — flags, since a list may mix them. A variable assigned is not here: `SET
@local_variable` is the other construction of the word, `Statement.SetVariable`.

| Node | Source | Production or page |
| --- | --- | --- |
| `SetExpression.DateAndTime.DateFirst` | T-SQL | SET DATEFIRST |
| `SetExpression.DateAndTime.DateFormat` | T-SQL | SET DATEFORMAT |
| `SetExpression.Locking.DeadlockPriority` | T-SQL | SET DEADLOCK_PRIORITY |
| `SetExpression.Locking.LockTimeout` | T-SQL | SET LOCK_TIMEOUT |
| `SetExpression.QueryExecution.Switch` | T-SQL | SET ARITHABORT, NOCOUNT, PARSEONLY and the group's other switches |
| `SetExpression.QueryExecution.RowCount` | T-SQL | SET ROWCOUNT |
| `SetExpression.QueryExecution.TextSize` | T-SQL | SET TEXTSIZE |
| `SetExpression.QueryExecution.QueryGovernorCostLimit` | T-SQL | SET QUERY_GOVERNOR_COST_LIMIT |
| `SetExpression.IsoSettings.Switch` | T-SQL | SET ANSI_NULLS, ANSI_PADDING and the other ISO settings |
| `SetExpression.Statistics.Switch` | T-SQL | SET FORCEPLAN, SHOWPLAN_ALL, SHOWPLAN_TEXT, SHOWPLAN_XML |
| `SetExpression.Statistics.Report` | T-SQL | SET STATISTICS IO, PROFILE, TIME, XML |
| `SetExpression.Transactions.Switch` | T-SQL | SET IMPLICIT_TRANSACTIONS, REMOTE_PROC_TRANSACTIONS, XACT_ABORT |
| `SetExpression.Transactions.IsolationLevel` | SQL-92 | §13.9 &lt;set transaction statement&gt;; T-SQL spells it SET TRANSACTION ISOLATION LEVEL |
| `SetExpression.Miscellaneous.Switch` | T-SQL | SET QUOTED_IDENTIFIER and the group's other switches, NO_BROWSETABLE, which no page describes, and SET RECOMMENDATIONS |
| `SetExpression.Miscellaneous.Language` | T-SQL | SET LANGUAGE |
| `SetExpression.Miscellaneous.FipsFlagger` | T-SQL | SET FIPS_FLAGGER |
| `SetExpression.Miscellaneous.ContextInfo` | T-SQL | SET CONTEXT_INFO |
| `SetExpression.Miscellaneous.IdentityInsert` | T-SQL | SET IDENTITY_INSERT |
| `SetExpression.Miscellaneous.Offset` | T-SQL | SET OFFSETS |
| `SetExpression.Miscellaneous.ErrorLevel` | T-SQL | SET ERRLVL, which no page describes |

### `Query`

| Node | Source | Production or page |
| --- | --- | --- |
| `Query.Specification` | SQL-92 | §7.9 &lt;query specification&gt;, with §7.4 &lt;table expression&gt; folded in |
| `Query.TableValueConstructor` | SQL-92 | §7.3 &lt;table value constructor&gt; |
| `Query.ExplicitTable` | SQL-92 | §7.10 &lt;explicit table&gt; |
| `Query.Parenthesized` | SQL-92 | §7.10 a query expression in brackets |
| `Query.Ordered` | T-SQL | SELECT — a query with its own ORDER BY and FOR clause, where no statement holds them |
| `Query.Union` | SQL-92 | §7.10 &lt;query expression&gt; with `UNION` |
| `Query.Except` | SQL-92 | §7.10 the same, with `EXCEPT` |
| `Query.Intersect` | SQL-92 | §7.10 the same, with `INTERSECT` |
| `Query.DefaultValues` | T-SQL | INSERT — `DEFAULT VALUES` |
| `Query.FromFile` | T-SQL | BULK INSERT — a file where a query stands |
| `Query.FromExecute` | T-SQL | INSERT — `INSERT … EXECUTE` |

### `Expression`

| Node | Source | Production or page |
| --- | --- | --- |
| `Expression.Or` | SQL-92 | §8.12 &lt;search condition&gt; |
| `Expression.And` | SQL-92 | §8.12 &lt;boolean term&gt; |
| `Expression.Not` | SQL-92 | §8.12 &lt;boolean factor&gt; |
| `Expression.IsTruth` | SQL-92 | §8.12 &lt;boolean test&gt; |
| `Expression.Add` | SQL-92 | §6.11 &lt;numeric value expression&gt; with &lt;plus sign&gt; |
| `Expression.Subtract` | SQL-92 | §6.11 the same, with &lt;minus sign&gt; |
| `Expression.Concatenate` | SQL-92 | §6.12 &lt;concatenation&gt;, and T-SQL's `+` over strings |
| `Expression.Multiply` | SQL-92 | §6.11 &lt;term&gt; with &lt;asterisk&gt; |
| `Expression.Divide` | SQL-92 | §6.11 the same, with &lt;solidus&gt; |
| `Expression.Negate` | SQL-92 | §6.11 &lt;factor&gt; with &lt;minus sign&gt; |
| `Expression.Plus` | SQL-92 | §6.11 the same, with &lt;plus sign&gt; |
| `Expression.Modulo` | T-SQL | Arithmetic Operators — Modulus |
| `Expression.BitwiseAnd` | T-SQL | Bitwise Operators — Bitwise AND |
| `Expression.BitwiseOr` | T-SQL | Bitwise Operators — Bitwise OR |
| `Expression.BitwiseXor` | T-SQL | Bitwise Operators — Bitwise Exclusive OR |
| `Expression.BitwiseNot` | T-SQL | Bitwise Operators — Bitwise NOT |
| `Expression.Comparison` | SQL-92 | §8.2 &lt;comparison predicate&gt; |
| `Expression.Quantified` | SQL-92 | §8.8 &lt;quantified comparison predicate&gt; |
| `Expression.Between` | SQL-92 | §8.3 &lt;between predicate&gt; |
| `Expression.In` | SQL-92 | §8.4 &lt;in predicate&gt; |
| `Expression.Like` | SQL-92 | §8.5 &lt;like predicate&gt; |
| `Expression.IsNull` | SQL-92 | §8.6 &lt;null predicate&gt; |
| `Expression.Exists` | SQL-92 | §8.9 &lt;exists predicate&gt; |
| `Expression.Unique` | SQL-92 | §8.11 &lt;unique predicate&gt; |
| `Expression.Match` | SQL-92 | §8.10 &lt;match predicate&gt; |
| `Expression.GraphMatch` | T-SQL | the graph `MATCH (…)`, its drawing kept as written |
| `Expression.Overlaps` | SQL-92 | §8.12 &lt;overlaps predicate&gt; |
| `Expression.IsDistinctFrom` | SQL:1999 | &lt;distinct predicate&gt; |
| `Expression.RoutineInvocation` | SQL-92 | §6.5 &lt;set function specification&gt;, §6.10 &lt;cast specification&gt;, and a call |
| `Expression.RowsetOrder` | T-SQL | OPENROWSET — `ORDER (c1 ASC) UNIQUE`, the order a bulk source is declared to arrive in |
| `Expression.Case` | SQL-92 | §6.9 &lt;case expression&gt; |
| `Expression.ColumnReference` | SQL-92 | §6.4 &lt;column reference&gt; |
| `Expression.Literal` | SQL-92 | §5.3 &lt;literal&gt; |
| `Expression.RowValueConstructor` | SQL-92 | §7.1 &lt;row value constructor&gt; |
| `Expression.NamedArgument` | T-SQL | an argument given by name — `EXECUTE`, and the rowset functions |
| `Expression.Aliased` | T-SQL | a value named where it stands — `DATA = t AS d` in PREDICT |
| `Expression.Pieced` | T-SQL | one argument written as several pieces with semicolons between them — `OPENROWSET`'s oldest spelling |
| `Expression.Prefixed` | T-SQL | a value with a word in front of it — `BULK` in OPENROWSET, `CHANGES` in CHANGETABLE, `LANGUAGE` in CONTAINSTABLE |
| `Expression.WindowFunction` | SQL:2003 | &lt;window function&gt; — a call and the window it is computed over |
| `Expression.Member` | T-SQL | a member of a value, or a method called on one — `a.b`, `a::b`, `a.f(1)` |
| `Expression.Collated` | SQL-92 | §6.11 &lt;collate clause&gt; on a value |
| `Expression.Measured` | T-SQL | a value with its unit after it — `10 MINUTES`, `50 PERCENT`, `4 GB` |
| `Expression.Hinted` | T-SQL | a value with the words after it that say how the engine treats it — `GROUP BY c WITH (DISTRIBUTED_AGG)`, a JSON constructor's `NULL ON NULL` and `RETURNING` |
| `Expression.JsonPair` | T-SQL | `key : value` in a JSON constructor |
| `Expression.OdbcEscape` | T-SQL | `{ FN … }`, `{ d '…' }`, `{ ts '…' }` — the word and what stands after it, kept in its braces |
| `Expression.Subquery` | SQL-92 | §7.11 &lt;subquery&gt; |
| `Expression.Parenthesized` | SQL:2023 | §6.28 &lt;parenthesized value expression&gt;, and §8.1's boolean one |

### `TableReference`

| Node | Source | Production or page |
| --- | --- | --- |
| `TableReference.Named` | SQL-92 | §6.3 &lt;table reference&gt;; T-SQL adds `FOR SYSTEM_TIME` and SQL Graph's `FOR PATH` |
| `TableReference.Derived` | SQL-92 | §6.3 &lt;derived table&gt;; T-SQL adds SQL Graph's `FOR PATH` |
| `TableReference.FunctionCall` | T-SQL | a table-valued function in a `FROM` clause, and the rowset functions |
| `TableReference.Pivot` | T-SQL | FROM — the PIVOT clause |
| `TableReference.Unpivot` | T-SQL | FROM — the UNPIVOT clause |
| `TableReference.Joined` | SQL-92 | §6.3 &lt;joined table&gt; |
| `TableReference.Parenthesized` | SQL-92 | §6.3 a table reference in brackets |
| `TableReference.OdbcJoin` | T-SQL | ODBC's outer-join escape, `{ OJ t1 LEFT JOIN t2 ON … }` |

### `Clause`

| Node | Source | Production or page |
| --- | --- | --- |
| `Clause.DerivedColumn` | SQL-92 | §7.9 &lt;derived column&gt; |
| `Clause.QualifiedAsterisk` | SQL-92 | §7.9 &lt;qualified asterisk&gt; |
| `Clause.SortSpecification` | SQL-92 | §13.1 &lt;sort specification&gt; |
| `Clause.OrderBy` | SQL-92 | §13.1 &lt;order by clause&gt;, with T-SQL's OFFSET and FETCH written inside it |
| `Clause.Top` | T-SQL | SELECT — `TOP (n) PERCENT WITH TIES` |
| `Clause.Window` | SQL:2003 | &lt;window specification&gt;; T-SQL: the OVER clause |
| `Clause.WindowDefinition` | SQL:2003 | one entry of a query's `WINDOW` clause: a name and the window it stands for |
| `Clause.SystemTime` | T-SQL | Temporal Tables — `FOR SYSTEM_TIME` |
| `Clause.Into` | T-SQL | SELECT — `INTO new_table [ON filegroup]` |
| `Clause.TableSample` | SQL:2003 | &lt;table sample clause&gt;; T-SQL: TABLESAMPLE |
| `Clause.JsonColumn` | T-SQL | OPENJSON — one column of its `WITH` schema |
| `Clause.GroupBy` | SQL-92 | §7.7 &lt;group by clause&gt;, with T-SQL's `ALL` and `WITH CUBE` |
| `Clause.CommonTableExpression` | SQL:1999 | &lt;with list element&gt;; T-SQL: WITH common_table_expression |
| `Clause.For` | T-SQL | SELECT — the FOR clause: `FOR XML`, `FOR JSON`, `FOR BROWSE` |
| `Clause.Hint` | T-SQL | Query Hints, and the table hints, kept as the words they were written as |
| `Clause.Option` | T-SQL | one option of any of the lists: `WITH (…)`, `SET (…)`, `ALTER DATABASE SET`, `MASKED WITH (…)` — a name, a value, nested options, partitions; and what stands in a `WITH` beside the named queries |
| `Clause.Placement` | T-SQL | where a table or an index is put: `ON`, `TEXTIMAGE_ON`, `FILESTREAM_ON`, and the target after `MOVE TO` |
| `Clause.VariableAssignment` | T-SQL | SELECT — `@variable = expression` in a select list |
| `Clause.When` | SQL-92 | §6.9 &lt;simple when clause&gt;, &lt;searched when clause&gt; |
| `Clause.Set` | SQL-92 | §13.10 &lt;set clause&gt; |
| `Clause.Output` | T-SQL | the OUTPUT clause of a DML statement: what is written out, the table it goes into, and a second OUTPUT after it |
| `Clause.ExecutionContext` | T-SQL | EXECUTE — AS LOGIN or AS USER and a name, after a string run |
| `Clause.CursorDefinition` | T-SQL | DECLARE CURSOR — the standard's words before CURSOR or T-SQL's after it, the query, its hints and what may be done through it |
| `Clause.CursorFor` | T-SQL | DECLARE CURSOR — FOR READ ONLY, or FOR UPDATE and the columns |
| `Clause.PartitionRange` | T-SQL | TRUNCATE TABLE — a partition by its number, or a range of them |
| `Clause.MergeWhen` | SQL:2003 | §14.9 &lt;merge when clause&gt; |
| `Clause.VariableDeclaration` | SQL/PSM | one &lt;variable declaration&gt; of it |
| `Clause.ParameterDeclaration` | SQL/PSM | &lt;SQL parameter declaration&gt; |
| `Clause.ColumnDefinition` | SQL-92 | §11.4 &lt;column definition&gt; |
| `Clause.ColumnOption` | T-SQL | one thing said about a column after its type that is not a constraint: `NULL`, `SPARSE`, `COLLATE`, `IDENTITY`, `MASKED`, … |
| `Clause.ConstraintDefinition` | SQL-92 | §11.6 &lt;table constraint definition&gt;, and the indexes T-SQL writes beside them, with everything written after them |
| `Clause.References` | SQL-92 | §11.8 &lt;references specification&gt;, with the referential actions |
| `Clause.Connection` | T-SQL | one pair of node tables a graph edge's `CONNECTION` may join |
| `Clause.Dropped` | T-SQL | one thing an ALTER TABLE … DROP names: the kind and the name, in the order a drop writes them |
| `Clause.DatabaseFile` | T-SQL | one file of a CREATE DATABASE: the bracket of options that describes it |
| `Clause.FileGroup` | T-SQL | a file group of a CREATE DATABASE: its name, what it contains, whether it is the default, its files |
| `Clause.EventPiece` | T-SQL | one piece of an event session — ADD/DROP EVENT, ADD/DROP TARGET — with its settings, actions and predicate |
