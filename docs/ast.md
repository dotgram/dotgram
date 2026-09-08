# The tree

`SqlNode` in `src/DotGram.Parsers/SqlSyntax.cs` is one tree for every dialect this project
reads — `SqlStandard92`, `TransactSql`, and whatever comes after them. This is what is in
it and where each node comes from.

## The rule

**A node is called what the production it comes from is called.** The standard's name where
the standard has the concept, the dialect's published name where it does not. So a reader
with the specification open can find the node, a reader with the node can find the
specification, and a second dialect that reads the same production builds the same node
rather than one of its own. That is the point of having one tree at all.

Four sources appear in the table:

| Source | What it means |
| --- | --- |
| `SQL-92` | ISO/IEC 9075:1992, by section and production. The grammar cites the same numbers. |
| `SQL:1999`, `SQL:2003` | A production the standard has and 1992 did not — a trigger, a role, `MERGE`. |
| `SQL/PSM` | ISO/IEC 9075-4, the procedural part: blocks, conditionals, loops, variables. |
| `T-SQL` | Microsoft's reference, by the page's own title. Everything a database engine has and a standard does not. |
| `—` | Four nodes no specification names. Three are the value tower written as a tree, and the standard has a production per strength rather than a name for the node. |

## The shape

**One root, one level.** Every record derives from `SqlNode` and from nothing else, so a
consumer switches on the record and is done. Nothing derives from a record, so nothing is
invisible to a reader who has enumerated them.

**A record per production, where the productions differ.** A `DROP TABLE` and a `DROP VIEW`
are not one shape with a word in it; they are two statements spelled alike, and a consumer
that reads the word to tell them apart is doing the parser's work twice. That is why there
are sixty-four `Drop…Statement` records rather than one.

**A kind per field, where they do not.** `Predicate` is one record with a
`SqlPredicateKind`, because the nine predicates of §8 are one shape with different operands;
`SqlOperator` tells an `OR` from a `*` for the same reason. The line between the two is
whether the productions are the same shape, not how many of them there are.

**The grammar says the shape; this file says which node.** The `.gram` reads `DROP <what>`
once and hands the words to `SqlNode.Dropped`, which turns them into the record. So the
catalogue of names lives here, in C#, where a catalogue of C# names belongs, and the
grammar still says one thing. `Dropped`, `Defined`, `OfDatabase`, `Commanded` and
`Permitted` are those factories, and each throws where the grammar has read a word this
file has no record for — which is the two catalogues having drifted, and a defect here
rather than in anybody's SQL.

**Nothing here is a position.** The tree says what was written, not where. A consumer that
needs the text back cuts it from the input itself, which is what §7.6 is for.

## What is not in the tree

Read and dropped, all of it, and each for the same reason: it is a decoration on how a
statement runs or what it hands back rather than on what it is, and a field nobody reads is
a field that drifts. `TOP`, `OVER`, the table and query hints, the windows, a named query's
`WITH`, `OUTPUT`, and the option lists of every DDL statement — an index's `WITH (…)`, a
database's settings, an endpoint's protocol arguments. When one of them is wanted it is one
field on one record here, and having a record each is what makes that a small change.

## The nodes

| Node | Source | Production or page |
| --- | --- | --- |
| `BinaryExpression` | — | the tower of §6.11 through §6.14 as a tree; the standard has a production per strength and no name for the node |
| `UnaryExpression` | — | the same, for a sign and for `NOT` |
| `BooleanTest` | SQL-92 | §8.12 &lt;boolean test&gt; |
| `Predicate` | SQL-92 | §8 &lt;predicate&gt;, all nine of them, told apart by `SqlPredicateKind` |
| `RoutineInvocation` | SQL-92 | §6.5 &lt;set function specification&gt;, §6.10 &lt;cast specification&gt;, and a call |
| `CaseExpression` | SQL-92 | §6.9 &lt;case expression&gt; |
| `WhenClause` | SQL-92 | §6.9 &lt;simple when clause&gt;, &lt;searched when clause&gt; |
| `ColumnReference` | SQL-92 | §6.4 &lt;column reference&gt; |
| `Literal` | SQL-92 | §5.3 &lt;literal&gt; |
| `RowValueConstructor` | SQL-92 | §7.1 &lt;row value constructor&gt; |
| `Subquery` | SQL-92 | §7.11 &lt;subquery&gt; |
| `QuerySpecification` | SQL-92 | §7.9 &lt;query specification&gt;, with §7.4 &lt;table expression&gt; folded in |
| `DerivedColumn` | SQL-92 | §7.9 &lt;derived column&gt; |
| `QualifiedAsterisk` | SQL-92 | §7.9 &lt;qualified asterisk&gt; |
| `TableReference` | SQL-92 | §6.3 &lt;table reference&gt; |
| `JoinedTable` | SQL-92 | §6.3 &lt;joined table&gt; |
| `TableValueConstructor` | SQL-92 | §7.3 &lt;table value constructor&gt; |
| `SelectStatement` | SQL-92 | §19.6 &lt;direct select statement: multiple rows&gt; — a query and its order |
| `SortSpecification` | SQL-92 | §13.1 &lt;sort specification&gt; |
| `InsertStatement` | SQL-92 | §13.8 &lt;insert statement&gt; |
| `UpdateStatement` | SQL-92 | §13.10 &lt;update statement: searched&gt;, widened by T-SQL's second `FROM` |
| `DeleteStatement` | SQL-92 | §13.7 &lt;delete statement: searched&gt;, likewise |
| `MergeStatement` | SQL:2003 | §14.9 &lt;merge statement&gt; |
| `MergeWhenClause` | SQL:2003 | §14.9 &lt;merge when clause&gt; |
| `SetClause` | SQL-92 | §13.10 &lt;set clause&gt; |
| `CompoundStatement` | SQL/PSM | &lt;compound statement&gt; — T-SQL writes it without the label |
| `IfStatement` | SQL/PSM | &lt;if statement&gt; |
| `WhileStatement` | SQL/PSM | &lt;while statement&gt; |
| `TryCatchStatement` | T-SQL | TRY...CATCH |
| `DeclareStatement` | SQL/PSM | &lt;SQL variable declaration&gt; |
| `VariableDeclaration` | SQL/PSM | one &lt;variable declaration&gt; of it |
| `TransactionStatement` | SQL-92 | §13.2 &lt;commit statement&gt;, §13.3 &lt;rollback statement&gt;, and T-SQL's `BEGIN`/`SAVE` |
| `ExecuteStatement` | T-SQL | EXECUTE |
| `TableDefinition` | SQL-92 | §11.3 &lt;table definition&gt; |
| `ColumnDefinition` | SQL-92 | §11.4 &lt;column definition&gt; |
| `ConstraintDefinition` | SQL-92 | §11.6 &lt;table constraint definition&gt;, and the indexes T-SQL writes beside them |
| `AlterTableStatement` | SQL-92 | §11.10 &lt;alter table statement&gt; |
| `CreateProcedureStatement` | SQL/PSM | &lt;SQL-invoked procedure&gt;; T-SQL: CREATE PROCEDURE |
| `CreateFunctionStatement` | SQL/PSM | &lt;SQL-invoked function&gt;; T-SQL: CREATE FUNCTION |
| `CreateTriggerStatement` | SQL:1999 | &lt;trigger definition&gt;; T-SQL: CREATE TRIGGER |
| `ViewDefinition` | SQL-92 | §11.19 &lt;view definition&gt; |
| `ParameterDeclaration` | SQL/PSM | &lt;SQL parameter declaration&gt; |
| `CreateIndexStatement` | T-SQL | CREATE INDEX — the standard has no index |
| `AlterIndexStatement` | T-SQL | ALTER INDEX |
| `PrintStatement` | T-SQL | PRINT |
| `ReturnStatement` | SQL/PSM | &lt;return statement&gt;; T-SQL: RETURN |
| `ThrowStatement` | T-SQL | THROW |
| `GoToStatement` | T-SQL | GOTO |
| `BreakStatement` | T-SQL | BREAK |
| `ContinueStatement` | T-SQL | CONTINUE |
| `CheckpointStatement` | T-SQL | CHECKPOINT |
| `UseStatement` | T-SQL | USE |
| `RaiseErrorStatement` | T-SQL | RAISERROR |
| `WaitForStatement` | T-SQL | WAITFOR |
| `CreateLoginStatement` | T-SQL | CREATE LOGIN |
| `AlterLoginStatement` | T-SQL | ALTER LOGIN |
| `CreateUserStatement` | T-SQL | CREATE USER |
| `AlterUserStatement` | T-SQL | ALTER USER |
| `CreateRoleStatement` | SQL:1999 | &lt;role definition&gt;; T-SQL: CREATE ROLE, CREATE SERVER ROLE |
| `AlterRoleStatement` | T-SQL | ALTER ROLE, ALTER SERVER ROLE |
| `CreateApplicationRoleStatement` | T-SQL | CREATE APPLICATION ROLE |
| `AlterApplicationRoleStatement` | T-SQL | ALTER APPLICATION ROLE |
| `SchemaDefinition` | SQL-92 | §11.1 &lt;schema definition&gt; |
| `AlterSchemaStatement` | T-SQL | ALTER SCHEMA |
| `AlterAuthorizationStatement` | T-SQL | ALTER AUTHORIZATION |
| `ExternalDataSourceDefinition` | T-SQL | CREATE/ALTER EXTERNAL DATA SOURCE |
| `ExternalFileFormatDefinition` | T-SQL | CREATE EXTERNAL FILE FORMAT |
| `ExternalLibraryDefinition` | T-SQL | CREATE/ALTER EXTERNAL LIBRARY |
| `ExternalResourcePoolDefinition` | T-SQL | CREATE/ALTER EXTERNAL RESOURCE POOL |
| `ResourcePoolDefinition` | T-SQL | CREATE/ALTER RESOURCE POOL |
| `WorkloadGroupDefinition` | T-SQL | CREATE/ALTER WORKLOAD GROUP |
| `ServerAuditDefinition` | T-SQL | CREATE/ALTER SERVER AUDIT |
| `AuditSpecificationDefinition` | T-SQL | CREATE/ALTER SERVER or DATABASE AUDIT SPECIFICATION |
| `EventSessionDefinition` | T-SQL | CREATE/ALTER EVENT SESSION |
| `EventNotificationDefinition` | T-SQL | CREATE EVENT NOTIFICATION |
| `EndpointDefinition` | T-SQL | CREATE/ALTER ENDPOINT |
| `CreateDatabaseStatement` | T-SQL | CREATE DATABASE — the standard has no database |
| `AlterDatabaseSetStatement` | T-SQL | ALTER DATABASE … SET |
| `AlterDatabaseScopedConfigurationStatement` | T-SQL | ALTER DATABASE … SCOPED CONFIGURATION |
| `AlterDatabaseCollateStatement` | T-SQL | ALTER DATABASE … COLLATE |
| `AlterDatabaseModifyNameStatement` | T-SQL | ALTER DATABASE … MODIFY NAME |
| `AlterDatabaseModifyFileGroupStatement` | T-SQL | ALTER DATABASE … MODIFY FILE GROUP |
| `AlterDatabaseModifyFileStatement` | T-SQL | ALTER DATABASE … MODIFY FILE |
| `AlterDatabaseModifyStatement` | T-SQL | ALTER DATABASE … MODIFY |
| `AlterDatabaseAddFileGroupStatement` | T-SQL | ALTER DATABASE … ADD FILE GROUP |
| `AlterDatabaseAddLogFileStatement` | T-SQL | ALTER DATABASE … ADD LOG FILE |
| `AlterDatabaseAddFileStatement` | T-SQL | ALTER DATABASE … ADD FILE |
| `AlterDatabaseRemoveFileGroupStatement` | T-SQL | ALTER DATABASE … REMOVE FILE GROUP |
| `AlterDatabaseRemoveFileStatement` | T-SQL | ALTER DATABASE … REMOVE FILE |
| `AlterDatabaseRebuildLogStatement` | T-SQL | ALTER DATABASE … REBUILD LOG |
| `AlterDatabasePerformCutoverStatement` | T-SQL | ALTER DATABASE … PERFORM CUTOVER |
| `DatabaseOption` | T-SQL | one setting of ALTER DATABASE SET; not a statement |
| `SetTransactionIsolationLevelStatement` | SQL-92 | §13.9 &lt;set transaction statement&gt;; T-SQL spells it SET TRANSACTION ISOLATION LEVEL |
| `SetIdentityInsertStatement` | T-SQL | SET IDENTITY_INSERT |
| `SetOptionStatement` | T-SQL | the SET statements that take ON or OFF |
| `SetCommandStatement` | T-SQL | the SET statements that take a value |
| `GrantStatement` | SQL-92 | §12.1 &lt;grant statement&gt; |
| `DenyStatement` | T-SQL | DENY |
| `RevokeStatement` | SQL-92 | §12.2 &lt;revoke statement&gt; |
| `FullTextIndexDefinition` | T-SQL | CREATE FULLTEXT INDEX |
| `AlterFullTextIndexStatement` | T-SQL | ALTER FULLTEXT INDEX |
| `FullTextCatalogDefinition` | T-SQL | CREATE FULLTEXT CATALOG |
| `AlterFullTextCatalogStatement` | T-SQL | ALTER FULLTEXT CATALOG |
| `FullTextStopListDefinition` | T-SQL | CREATE FULLTEXT STOPLIST |
| `AlterFullTextStopListStatement` | T-SQL | ALTER FULLTEXT STOPLIST |
| `SearchPropertyListDefinition` | T-SQL | CREATE SEARCH PROPERTY LIST |
| `AlterSearchPropertyListStatement` | T-SQL | ALTER SEARCH PROPERTY LIST |
| `BackupDatabaseStatement` | T-SQL | BACKUP DATABASE |
| `BackupTransactionLogStatement` | T-SQL | BACKUP LOG |
| `BackupServerStatement` | T-SQL | BACKUP SERVER |
| `BackupGroupStatement` | T-SQL | BACKUP GROUP |
| `BackupCertificateStatement` | T-SQL | BACKUP CERTIFICATE |
| `BackupMasterKeyStatement` | T-SQL | BACKUP MASTER KEY |
| `BackupServiceMasterKeyStatement` | T-SQL | BACKUP SERVICE MASTER KEY |
| `BackupSymmetricKeyStatement` | T-SQL | BACKUP SYMMETRIC KEY |
| `RestoreDatabaseStatement` | T-SQL | RESTORE DATABASE |
| `RestoreLogStatement` | T-SQL | RESTORE LOG |
| `RestoreFileListOnlyStatement` | T-SQL | RESTORE FILELISTONLY |
| `RestoreHeaderOnlyStatement` | T-SQL | RESTORE HEADERONLY |
| `RestoreLabelOnlyStatement` | T-SQL | RESTORE LABELONLY |
| `RestoreRewindOnlyStatement` | T-SQL | RESTORE REWINDONLY |
| `RestoreVerifyOnlyStatement` | T-SQL | RESTORE VERIFYONLY |
| `RestoreMasterKeyStatement` | T-SQL | RESTORE MASTER KEY |
| `RestoreServiceMasterKeyStatement` | T-SQL | RESTORE SERVICE MASTER KEY |
| `DropAggregateStatement` | T-SQL | DROP AGGREGATE |
| `DropApplicationRoleStatement` | T-SQL | DROP APPLICATION ROLE |
| `DropAvailabilityGroupStatement` | T-SQL | DROP AVAILABILITY GROUP |
| `DropBrokerPriorityStatement` | T-SQL | DROP BROKER PRIORITY |
| `DropCertificateStatement` | T-SQL | DROP CERTIFICATE |
| `DropColumnEncryptionKeyStatement` | T-SQL | DROP COLUMN ENCRYPTION KEY |
| `DropColumnMasterKeyStatement` | T-SQL | DROP COLUMN MASTER KEY |
| `DropContractStatement` | T-SQL | DROP CONTRACT |
| `DropCredentialStatement` | T-SQL | DROP CREDENTIAL |
| `DropCryptographicProviderStatement` | T-SQL | DROP CRYPTOGRAPHIC PROVIDER |
| `DropDatabaseAuditSpecificationStatement` | T-SQL | DROP DATABASE AUDIT SPECIFICATION |
| `DropDatabaseScopedCredentialStatement` | T-SQL | DROP DATABASE SCOPED CREDENTIAL |
| `DropDatabaseStatement` | T-SQL | DROP DATABASE |
| `DropDefaultStatement` | T-SQL | DROP DEFAULT |
| `DropEndpointStatement` | T-SQL | DROP ENDPOINT |
| `DropExternalDataSourceStatement` | T-SQL | DROP EXTERNAL DATA SOURCE |
| `DropExternalFileFormatStatement` | T-SQL | DROP EXTERNAL FILE FORMAT |
| `DropExternalLanguageStatement` | T-SQL | DROP EXTERNAL LANGUAGE |
| `DropExternalModelStatement` | T-SQL | DROP EXTERNAL MODEL |
| `DropExternalResourcePoolStatement` | T-SQL | DROP EXTERNAL RESOURCE POOL |
| `DropExternalTableStatement` | T-SQL | DROP EXTERNAL TABLE |
| `DropFulltextCatalogStatement` | T-SQL | DROP FULLTEXT CATALOG |
| `DropFulltextStoplistStatement` | T-SQL | DROP FULLTEXT STOPLIST |
| `DropFunctionStatement` | T-SQL | DROP FUNCTION |
| `DropLoginStatement` | T-SQL | DROP LOGIN |
| `DropMessageTypeStatement` | T-SQL | DROP MESSAGE TYPE |
| `DropPartitionFunctionStatement` | T-SQL | DROP PARTITION FUNCTION |
| `DropPartitionSchemeStatement` | T-SQL | DROP PARTITION SCHEME |
| `DropProcedureStatement` | T-SQL | DROP PROCEDURE |
| `DropQueueStatement` | T-SQL | DROP QUEUE |
| `DropRemoteServiceBindingStatement` | T-SQL | DROP REMOTE SERVICE BINDING |
| `DropResourcePoolStatement` | T-SQL | DROP RESOURCE POOL |
| `DropRoleStatement` | T-SQL | DROP ROLE |
| `DropRouteStatement` | T-SQL | DROP ROUTE |
| `DropRuleStatement` | T-SQL | DROP RULE |
| `DropSchemaStatement` | T-SQL | DROP SCHEMA |
| `DropSearchPropertyListStatement` | T-SQL | DROP SEARCH PROPERTY LIST |
| `DropSecurityPolicyStatement` | T-SQL | DROP SECURITY POLICY |
| `DropSequenceStatement` | T-SQL | DROP SEQUENCE |
| `DropServerAuditSpecificationStatement` | T-SQL | DROP SERVER AUDIT SPECIFICATION |
| `DropServerAuditStatement` | T-SQL | DROP SERVER AUDIT |
| `DropServerRoleStatement` | T-SQL | DROP SERVER ROLE |
| `DropServiceStatement` | T-SQL | DROP SERVICE |
| `DropStatisticsStatement` | T-SQL | DROP STATISTICS |
| `DropSynonymStatement` | T-SQL | DROP SYNONYM |
| `DropTableStatement` | T-SQL | DROP TABLE |
| `DropTypeStatement` | T-SQL | DROP TYPE |
| `DropUserStatement` | T-SQL | DROP USER |
| `DropViewStatement` | T-SQL | DROP VIEW |
| `DropWorkloadClassifierStatement` | T-SQL | DROP WORKLOAD CLASSIFIER |
| `DropWorkloadGroupStatement` | T-SQL | DROP WORKLOAD GROUP |
| `DropXmlSchemaCollectionStatement` | T-SQL | DROP XML SCHEMA COLLECTION |
| `DropAsymmetricKeyStatement` | T-SQL | DROP ASYMMETRIC KEY |
| `DropSymmetricKeyStatement` | T-SQL | DROP SYMMETRIC KEY |
| `DropAssemblyStatement` | T-SQL | DROP ASSEMBLY |
| `DropExternalLibraryStatement` | T-SQL | DROP EXTERNAL LIBRARY |
| `DropEventSessionStatement` | T-SQL | DROP EVENT SESSION |
| `DropEventNotificationStatement` | T-SQL | DROP EVENT NOTIFICATION |
| `DropFulltextIndexStatement` | T-SQL | DROP FULLTEXT INDEX |
| `DropIndexStatement` | T-SQL | DROP INDEX |
| `DropSignatureStatement` | T-SQL | DROP SIGNATURE |
| `DropSensitivityClassificationStatement` | T-SQL | DROP SENSITIVITY CLASSIFICATION |
| `DropTriggerStatement` | T-SQL | DROP TRIGGER |
| `DropMasterKeyStatement` | T-SQL | DROP MASTER KEY |
| `DropDatabaseEncryptionKeyStatement` | T-SQL | DROP DATABASE ENCRYPTION KEY |
