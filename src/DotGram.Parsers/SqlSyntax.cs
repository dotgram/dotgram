using System;

namespace DotGram.Parsers.Sql;

// ── What a SQL parser builds ───────────────────────────────────────────────────────────────
//
// Five hierarchies, one per category the standard has: `Statement`, `Query`, `Expression`,
// `TableReference` and `Clause`. They are named after the productions they come from and are
// shared by every parser of the language — `SqlStandard92`, `TransactSql`, and whatever reads
// the same grammar next. `docs/ast.md` lists each node and the specification it is named from.
//
// **Why five and not one.** A tree with a single root types nothing: a field of it accepts a
// statement where a value belongs, and the compiler cannot tell a reader they are wrong. The
// standard does not work that way — §7 puts a <query expression> where a table belongs and §6
// puts a <value expression> where a value belongs, and the two are not interchangeable. So the
// roots are the standard's own categories, and a field says which one it holds:
// `Statement.Insert.Rows` is a `Query`, `Expression.Subquery.Query` is a `Query`, and
// `Query.Specification.From` is a `TableReference[]`. Several things that used to typecheck and
// were wrong now do not compile.
//
// **Relations by aggregation, never by inheritance.** A subquery is not a kind of query; it is
// an expression that holds one. A statement that returns rows is not a kind of query; it is a
// statement that holds one. Nothing here derives from anything but its own root, and no root
// derives from another. Crossing between hierarchies is always a field.
//
// **One level under each root.** A consumer switches over the descendants of the root it holds
// and has seen all of them. A node under another node would be a node half its readers miss.
//
// **A record per production.** `DROP TABLE` and `DROP VIEW` are not one statement with a word
// in it, and a consumer reading that word to tell them apart is doing the parser's work twice.
// The same goes for the operators: the standard writes <numeric value expression> ::= … <plus
// sign> <term> as its own production, so `Expression.Add` is a record and not an enum value.
// The one place a word survives is where the standard itself has a production for the word
// — <comp op> — so a comparison is one record carrying a `SqlComparison`.
//
// **The tree knows how it is made.** The words a grammar matches and the lists it gathers
// become nodes through the statics on each root and on `Syntax`, so that any parser of this
// language — generated under any carrier, or written by hand — builds the same tree by the
// same code.
//
// **Nothing here is a position.** The tree says what was written, not where. A consumer that
// needs the text back cuts it from the input itself, which is what §7.6 of `docs/syntax.md`
// is for.

/// <summary>A statement: §13 of the standard, and most of a dialect's reference.</summary>
public abstract record Statement
{
	// ---- §14 the data statements -------------------------------------------------------------

	/// <summary>§14.1 a query, and the clauses the statement wraps it in.</summary>
	/// <remarks>
	/// The parts in the order the reference writes them: the named queries in front, the
	/// query itself, the order its rows are asked for in, what shape they come back in, and
	/// how the whole thing is to be run. Four of the five are the statement's and not the
	/// query's — a <c>UNION</c> of two selects has one <c>ORDER BY</c> between them.
	/// </remarks>
	public sealed record Select(
		Clause[] With, Query Of, Clause? OrderBy, Clause[] For, Clause[] Options) : Statement;

	/// <summary>A select with nothing around it, which is the whole of the standard's.</summary>
	public static Select Selected(Query of, Clause? by = null) =>
		new(Clause.None, of, by, Clause.None, Clause.None);

	/// <summary>§14.11 rows written into a table, from a list, a query or nothing at all.</summary>
	/// <remarks>
	/// <see cref="Rows"/> is a <see cref="Query.TableValueConstructor"/> where they were written
	/// out, a <see cref="Query.Specification"/> where they come from one, a
	/// <see cref="Query.FromExecute"/> for T-SQL's <c>INSERT … EXEC</c>, and
	/// <see cref="Query.DefaultValues"/> for <c>DEFAULT VALUES</c>.
	/// </remarks>
	public sealed record Insert(TableReference? Target, string[]? Columns, Query Rows) : Statement;

	/// <summary>§14.14 rows changed in place: what to change, to what, and which rows.</summary>
	/// <remarks>
	/// <see cref="From"/> is T-SQL's extension and not the standard's: a second <c>FROM</c>
	/// naming the tables the rows to change are found by joining.
	/// </remarks>
	public sealed record Update(
		TableReference? Target, Clause[] Set, TableReference[] From, Expression? Where) : Statement;

	/// <summary>§14.9 rows removed, and the same two ways of saying which.</summary>
	public sealed record Delete(
		TableReference? Target, TableReference[] From, Expression? Where) : Statement;

	/// <summary>
	/// §14.12 one statement that inserts, updates and deletes, according to what a join found.
	/// </summary>
	public sealed record Merge(
		TableReference? Target, TableReference Using, Expression On, Clause[] Whens) : Statement;

	// ---- the procedural level ----------------------------------------------------------------
	//
	// Structure gets a record and shapelessness does not. A block holds statements, a
	// conditional holds two, a declaration holds a list — each of those is a shape and each
	// has one.

	/// <summary><c>BEGIN … END</c>, and the body of anything that has one.</summary>
	public sealed record Compound(Statement[] Statements) : Statement;

	/// <summary><c>IF … ELSE</c>, where either arm is one statement and a block is one.</summary>
	public sealed record If(Expression Condition, Statement Then, Statement? Else) : Statement;

	/// <summary><c>WHILE</c>, and the one statement it repeats.</summary>
	public sealed record While(Expression Condition, Statement Body) : Statement;

	/// <summary><c>BEGIN TRY … END TRY BEGIN CATCH … END CATCH</c>.</summary>
	public sealed record TryCatch(Statement[] Tried, Statement[] Caught) : Statement;

	/// <summary>One <c>DECLARE</c>, which may declare several.</summary>
	public sealed record Declare(Clause[] Variables) : Statement;

	/// <summary>A transaction begun, committed, rolled back or saved, and its name.</summary>
	public sealed record Transaction(string Kind, string? Name) : Statement;

	/// <summary>
	/// <c>EXECUTE</c>: what is called, with what, and the variable the return code goes to.
	/// </summary>
	public sealed record Execute(string? Into, string Name, Expression[] Arguments) : Statement;

	// ---- the tables ---------------------------------------------------------------------------

	/// <summary>
	/// §11.1 a table declared: its name, and the columns, constraints and indexes in it.
	/// </summary>
	public sealed record TableDefinition(string Name, Clause[] Elements) : Statement;

	/// <summary>
	/// A table whose columns are whatever a query returns — <c>CREATE TABLE … AS SELECT</c>,
	/// and the external table written the same way.
	/// </summary>
	public sealed record CreateTableAsSelect(string Name, Statement Body) : Statement;

	/// <summary>§11.10 a table changed: its name, what is being done, and to what.</summary>
	public sealed record AlterTable(string Name, string Action, Clause[] Elements) : Statement;

	// ---- the routines --------------------------------------------------------------------------

	/// <summary>A procedure: its name, what it takes, and what it does.</summary>
	public sealed record CreateProcedure(
		string Name, Clause[] Parameters, Statement[] Body) : Statement;

	/// <summary>
	/// A function, and what it returns says which of the three shapes it is: a type for a
	/// scalar, <c>TABLE</c> for either of the two that return rows.
	/// </summary>
	public sealed record CreateFunction(
		string Name, Clause[] Parameters, string? Returns, Statement[] Body) : Statement;

	/// <summary>A trigger: what it is on, what fires it, and what it does then.</summary>
	public sealed record CreateTrigger(
		string Name, string On, string[] Events, Statement[] Body) : Statement;

	/// <summary>§11.32 a view, which is a name given to a query.</summary>
	public sealed record ViewDefinition(
		string Name, string[]? Columns, Statement Body) : Statement;

	// ---- indexes -------------------------------------------------------------------------------

	/// <summary>An index declared: its name, what it is on, and the columns it is over.</summary>
	public sealed record CreateIndex(string Name, string On, string[]? Columns) : Statement;

	/// <summary>An index changed: which one, on what, and what is being done to it.</summary>
	public sealed record AlterIndex(string Name, string On, string Action) : Statement;

	/// <summary><c>CREATE STATISTICS</c>.</summary>
	public sealed record StatisticsDefinition(string Name) : Statement;

	/// <summary><c>UPDATE STATISTICS</c>, which names the table rather than the statistics.</summary>
	public sealed record UpdateStatistics(string On) : Statement;

	// ---- a word and some values ----------------------------------------------------------------
	//
	// Ten statements that are a word and what follows it. They shared one record until the
	// tree was made to say what the grammar says, and a reader who wanted the `PRINT` had to
	// look at a string to find it.

	/// <summary>One value printed.</summary>
	public sealed record Print(Expression Value) : Statement;

	/// <summary>A routine left, with a code where one was given.</summary>
	public sealed record Return(Expression? Value) : Statement;

	/// <summary>An error raised, or the caught one raised again.</summary>
	public sealed record Throw(Expression[] Arguments) : Statement;

	/// <summary>A jump to a label.</summary>
	public sealed record GoTo(Expression Label) : Statement;

	/// <summary>A loop left.</summary>
	public sealed record Break : Statement;

	/// <summary>A loop begun again.</summary>
	public sealed record Continue : Statement;

	/// <summary>The log written out.</summary>
	public sealed record Checkpoint(Expression? Value) : Statement;

	/// <summary>The database the rest of the batch is read against.</summary>
	public sealed record Use(Expression Name) : Statement;

	/// <summary>The older spelling of an error raised.</summary>
	public sealed record RaiseError(Expression[] Arguments) : Statement;

	/// <summary>A delay, or a time to wait until.</summary>
	public sealed record WaitFor(Expression Value) : Statement;

	// ---- who may connect, what lives outside, and what the server watches ----------------------
	//
	// One record each, and the name only: what these are set to is an option list the grammar
	// reads and drops, and a field nobody reads is a field that drifts. When one of them is
	// wanted it is one field on one record here, which is what having a record each is for.

	/// <summary><c>CREATE LOGIN</c>.</summary>
	public sealed record CreateLogin(string Name) : Statement;

	/// <summary><c>ALTER LOGIN</c>.</summary>
	public sealed record AlterLogin(string Name) : Statement;

	/// <summary><c>CREATE USER</c>.</summary>
	public sealed record CreateUser(string Name) : Statement;

	/// <summary><c>ALTER USER</c>.</summary>
	public sealed record AlterUser(string Name) : Statement;

	/// <summary><c>CREATE ROLE</c>.</summary>
	public sealed record CreateRole(string Name) : Statement;

	/// <summary><c>ALTER ROLE</c>.</summary>
	public sealed record AlterRole(string Name) : Statement;

	/// <summary><c>CREATE APPLICATION ROLE</c>.</summary>
	public sealed record CreateApplicationRole(string Name) : Statement;

	/// <summary><c>ALTER APPLICATION ROLE</c>.</summary>
	public sealed record AlterApplicationRole(string Name) : Statement;

	/// <summary>§11.1 <c>CREATE SCHEMA</c>.</summary>
	public sealed record SchemaDefinition(string Name) : Statement;

	/// <summary><c>ALTER SCHEMA</c>.</summary>
	public sealed record AlterSchema(string Name) : Statement;

	/// <summary><c>ALTER AUTHORIZATION</c>.</summary>
	public sealed record AlterAuthorization(string Name) : Statement;

	/// <summary><c>EXTERNAL DATA SOURCE</c>.</summary>
	public sealed record ExternalDataSourceDefinition(string Name) : Statement;

	/// <summary><c>EXTERNAL FILE FORMAT</c>.</summary>
	public sealed record ExternalFileFormatDefinition(string Name) : Statement;

	/// <summary><c>EXTERNAL LIBRARY</c>.</summary>
	public sealed record ExternalLibraryDefinition(string Name) : Statement;

	/// <summary><c>EXTERNAL RESOURCE POOL</c>.</summary>
	public sealed record ExternalResourcePoolDefinition(string Name) : Statement;

	/// <summary><c>RESOURCE POOL</c>.</summary>
	public sealed record ResourcePoolDefinition(string Name) : Statement;

	/// <summary><c>WORKLOAD GROUP</c>.</summary>
	public sealed record WorkloadGroupDefinition(string Name) : Statement;

	/// <summary><c>SERVER AUDIT</c>.</summary>
	public sealed record ServerAuditDefinition(string Name) : Statement;

	/// <summary><c>AUDIT SPECIFICATION</c>.</summary>
	public sealed record AuditSpecificationDefinition(string Name) : Statement;

	/// <summary><c>EVENT SESSION</c>.</summary>
	public sealed record EventSessionDefinition(string Name) : Statement;

	/// <summary><c>EVENT NOTIFICATION</c>.</summary>
	public sealed record EventNotificationDefinition(string Name) : Statement;

	/// <summary><c>ENDPOINT</c>.</summary>
	public sealed record EndpointDefinition(string Name) : Statement;

	// ---- the database --------------------------------------------------------------------------

	/// <summary><c>CREATE DATABASE</c>, and the files it is made of.</summary>
	public sealed record CreateDatabase(string Name, Clause[] Files) : Statement;

	/// <summary><c>ALTER DATABASE … SET</c>, and what it was set to.</summary>
	public sealed record AlterDatabaseSet(string Name, Clause[] Settings) : Statement;

	/// <summary><c>ALTER DATABASE … SCOPED CONFIGURATION</c>, and what it was set to.</summary>
	public sealed record AlterDatabaseScopedConfiguration(string Name, Clause[] Settings) : Statement;

	/// <summary><c>ALTER DATABASE … COLLATE</c>.</summary>
	public sealed record AlterDatabaseCollate(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … MODIFY NAME</c>.</summary>
	public sealed record AlterDatabaseModifyName(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … MODIFY FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseModifyFileGroup(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … MODIFY FILE</c>.</summary>
	public sealed record AlterDatabaseModifyFile(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … MODIFY</c>.</summary>
	public sealed record AlterDatabaseModify(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … ADD FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseAddFileGroup(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … ADD LOG FILE</c>.</summary>
	public sealed record AlterDatabaseAddLogFile(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … ADD FILE</c>.</summary>
	public sealed record AlterDatabaseAddFile(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … REMOVE FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseRemoveFileGroup(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … REMOVE FILE</c>.</summary>
	public sealed record AlterDatabaseRemoveFile(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … REBUILD LOG</c>.</summary>
	public sealed record AlterDatabaseRebuildLog(string Name) : Statement;

	/// <summary><c>ALTER DATABASE … PERFORM_CUTOVER</c>.</summary>
	public sealed record AlterDatabasePerformCutover(string Name) : Statement;

	// ---- the SET statements --------------------------------------------------------------------

	/// <summary>§19.4 <c>SET TRANSACTION ISOLATION LEVEL</c>.</summary>
	public sealed record SetTransactionIsolationLevel(string Level) : Statement;

	/// <summary><c>SET IDENTITY_INSERT t ON</c>.</summary>
	public sealed record SetIdentityInsert(string Table, bool On) : Statement;

	/// <summary>
	/// A setting or several turned on or off — <c>SET ANSI_NULLS, ANSI_PADDING ON</c>.
	/// </summary>
	public sealed record SetOption(string[] Options, bool On) : Statement;

	/// <summary>
	/// A setting given a value — <c>SET ROWCOUNT 10</c>, <c>SET LANGUAGE us_english</c>.
	/// </summary>
	public sealed record SetCommand(string Option, Expression? Value) : Statement;

	/// <summary>
	/// A variable assigned — <c>SET @a = 1</c>, which is a statement and not a
	/// <see cref="Clause.Set"/>: what a <c>SET</c> clause belongs to is an <c>UPDATE</c>.
	/// </summary>
	public sealed record SetVariable(string Name, Expression Value) : Statement;

	// ---- §12.1 the permissions -----------------------------------------------------------------

	/// <summary><c>GRANT</c>: what is being said about, and to whom.</summary>
	public sealed record Grant(string[] Privileges, string[] Principals) : Statement;

	/// <summary><c>DENY</c>: what is being said about, and to whom.</summary>
	public sealed record Deny(string[] Privileges, string[] Principals) : Statement;

	/// <summary><c>REVOKE</c>: what is being said about, and to whom.</summary>
	public sealed record Revoke(string[] Privileges, string[] Principals) : Statement;

	// ---- the full-text catalogue -----------------------------------------------------------------

	/// <summary><c>CREATE FULLTEXT INDEX</c>, which is named by the table it is on.</summary>
	public sealed record FullTextIndexDefinition(string On) : Statement;

	/// <summary><c>ALTER FULLTEXT INDEX</c>.</summary>
	public sealed record AlterFullTextIndex(string On) : Statement;

	/// <summary><c>CREATE FULLTEXT CATALOG</c>.</summary>
	public sealed record FullTextCatalogDefinition(string Name) : Statement;

	/// <summary><c>ALTER FULLTEXT CATALOG</c>.</summary>
	public sealed record AlterFullTextCatalog(string Name) : Statement;

	/// <summary><c>CREATE FULLTEXT STOPLIST</c>.</summary>
	public sealed record FullTextStopListDefinition(string Name) : Statement;

	/// <summary><c>ALTER FULLTEXT STOPLIST</c>.</summary>
	public sealed record AlterFullTextStopList(string Name) : Statement;

	/// <summary><c>CREATE SEARCH PROPERTY LIST</c>.</summary>
	public sealed record SearchPropertyListDefinition(string Name) : Statement;

	/// <summary><c>ALTER SEARCH PROPERTY LIST</c>.</summary>
	public sealed record AlterSearchPropertyList(string Name) : Statement;

	// ---- backup and restore ----------------------------------------------------------------------
	//
	// Two statements and one shape between them: what is being copied, the devices it goes to
	// or comes from, and a long option list. Seventeen records because there are seventeen
	// statements — a log is not a database and a header is not a file list, and the reference
	// gives each its own page.

	/// <summary><c>BACKUP DATABASE</c>.</summary>
	public sealed record BackupDatabase(string? Name) : Statement;

	/// <summary><c>BACKUP LOG</c>.</summary>
	public sealed record BackupTransactionLog(string? Name) : Statement;

	/// <summary><c>BACKUP SERVER</c>, which names nothing: there is one.</summary>
	public sealed record BackupServer(string? Name) : Statement;

	/// <summary><c>BACKUP GROUP</c>.</summary>
	public sealed record BackupGroup(string? Name) : Statement;

	/// <summary><c>BACKUP CERTIFICATE</c>.</summary>
	public sealed record BackupCertificate(string? Name) : Statement;

	/// <summary><c>BACKUP MASTER KEY</c>.</summary>
	public sealed record BackupMasterKey(string? Name) : Statement;

	/// <summary><c>BACKUP SERVICE MASTER KEY</c>.</summary>
	public sealed record BackupServiceMasterKey(string? Name) : Statement;

	/// <summary><c>BACKUP SYMMETRIC KEY</c>.</summary>
	public sealed record BackupSymmetricKey(string? Name) : Statement;

	/// <summary><c>RESTORE DATABASE</c>.</summary>
	public sealed record RestoreDatabase(string? Name) : Statement;

	/// <summary><c>RESTORE LOG</c>.</summary>
	public sealed record RestoreLog(string? Name) : Statement;

	/// <summary><c>RESTORE FILELISTONLY</c>.</summary>
	public sealed record RestoreFileListOnly(string? Name) : Statement;

	/// <summary><c>RESTORE HEADERONLY</c>.</summary>
	public sealed record RestoreHeaderOnly(string? Name) : Statement;

	/// <summary><c>RESTORE LABELONLY</c>.</summary>
	public sealed record RestoreLabelOnly(string? Name) : Statement;

	/// <summary><c>RESTORE REWINDONLY</c>.</summary>
	public sealed record RestoreRewindOnly(string? Name) : Statement;

	/// <summary><c>RESTORE VERIFYONLY</c>.</summary>
	public sealed record RestoreVerifyOnly(string? Name) : Statement;

	/// <summary><c>RESTORE MASTER KEY</c>.</summary>
	public sealed record RestoreMasterKey(string? Name) : Statement;

	/// <summary><c>RESTORE SERVICE MASTER KEY</c>.</summary>
	public sealed record RestoreServiceMasterKey(string? Name) : Statement;

	// ---- the keys, and what is locked with them --------------------------------------------------

	/// <summary><c>CREATE ASYMMETRIC KEY</c>.</summary>
	public sealed record AsymmetricKeyDefinition(string Name) : Statement;

	/// <summary><c>ALTER ASYMMETRIC KEY</c>.</summary>
	public sealed record AlterAsymmetricKey(string Name) : Statement;

	/// <summary><c>CREATE SYMMETRIC KEY</c>.</summary>
	public sealed record SymmetricKeyDefinition(string Name) : Statement;

	/// <summary><c>ALTER SYMMETRIC KEY</c>.</summary>
	public sealed record AlterSymmetricKey(string Name) : Statement;

	/// <summary><c>CREATE CERTIFICATE</c>.</summary>
	public sealed record CertificateDefinition(string Name) : Statement;

	/// <summary><c>ALTER CERTIFICATE</c>.</summary>
	public sealed record AlterCertificate(string Name) : Statement;

	/// <summary><c>CREATE MASTER KEY</c>.</summary>
	public sealed record MasterKeyDefinition(string Name) : Statement;

	/// <summary><c>ALTER MASTER KEY</c>.</summary>
	public sealed record AlterMasterKey(string Name) : Statement;

	/// <summary><c>CREATE DATABASE ENCRYPTION KEY</c>.</summary>
	public sealed record DatabaseEncryptionKeyDefinition(string Name) : Statement;

	/// <summary><c>ALTER DATABASE ENCRYPTION KEY</c>.</summary>
	public sealed record AlterDatabaseEncryptionKey(string Name) : Statement;

	/// <summary><c>CREATE COLUMN ENCRYPTION KEY</c>.</summary>
	public sealed record ColumnEncryptionKeyDefinition(string Name) : Statement;

	/// <summary><c>ALTER COLUMN ENCRYPTION KEY</c>.</summary>
	public sealed record AlterColumnEncryptionKey(string Name) : Statement;

	/// <summary><c>CREATE COLUMN MASTER KEY</c>.</summary>
	public sealed record ColumnMasterKeyDefinition(string Name) : Statement;

	/// <summary><c>CREATE/ALTER CREDENTIAL</c>.</summary>
	public sealed record CredentialDefinition(string Name) : Statement;

	/// <summary><c>CREATE/ALTER DATABASE SCOPED CREDENTIAL</c>.</summary>
	public sealed record DatabaseScopedCredentialDefinition(string Name) : Statement;

	/// <summary><c>CREATE/ALTER SECURITY POLICY</c>.</summary>
	public sealed record SecurityPolicyDefinition(string Name) : Statement;

	// ---- what a statement drops ----------------------------------------------------------------
	//
	// Sixty-six records of one shape, because sixty-six statements of one shape is what the
	// reference has: `DROP TABLE` and `DROP VIEW` are spelled alike and are not the same
	// statement. The grammar reads the shape once and `Dropped` turns the word into the record,
	// so the catalogue of names is here, in C#, and the grammar still says one thing.

	/// <summary><c>DROP AGGREGATE</c>.</summary>
	public sealed record DropAggregate(Expression[] Names) : Statement;

	/// <summary><c>DROP APPLICATION ROLE</c>.</summary>
	public sealed record DropApplicationRole(Expression[] Names) : Statement;

	/// <summary><c>DROP AVAILABILITY GROUP</c>.</summary>
	public sealed record DropAvailabilityGroup(Expression[] Names) : Statement;

	/// <summary><c>DROP BROKER PRIORITY</c>.</summary>
	public sealed record DropBrokerPriority(Expression[] Names) : Statement;

	/// <summary><c>DROP CERTIFICATE</c>.</summary>
	public sealed record DropCertificate(Expression[] Names) : Statement;

	/// <summary><c>DROP COLUMN ENCRYPTION KEY</c>.</summary>
	public sealed record DropColumnEncryptionKey(Expression[] Names) : Statement;

	/// <summary><c>DROP COLUMN MASTER KEY</c>.</summary>
	public sealed record DropColumnMasterKey(Expression[] Names) : Statement;

	/// <summary><c>DROP CONTRACT</c>.</summary>
	public sealed record DropContract(Expression[] Names) : Statement;

	/// <summary><c>DROP CREDENTIAL</c>.</summary>
	public sealed record DropCredential(Expression[] Names) : Statement;

	/// <summary><c>DROP CRYPTOGRAPHIC PROVIDER</c>.</summary>
	public sealed record DropCryptographicProvider(Expression[] Names) : Statement;

	/// <summary><c>DROP DATABASE AUDIT SPECIFICATION</c>.</summary>
	public sealed record DropDatabaseAuditSpecification(Expression[] Names) : Statement;

	/// <summary><c>DROP DATABASE SCOPED CREDENTIAL</c>.</summary>
	public sealed record DropDatabaseScopedCredential(Expression[] Names) : Statement;

	/// <summary><c>DROP DATABASE</c>.</summary>
	public sealed record DropDatabase(Expression[] Names) : Statement;

	/// <summary><c>DROP DEFAULT</c>.</summary>
	public sealed record DropDefault(Expression[] Names) : Statement;

	/// <summary><c>DROP ENDPOINT</c>.</summary>
	public sealed record DropEndpoint(Expression[] Names) : Statement;

	/// <summary><c>DROP EXTERNAL DATA SOURCE</c>.</summary>
	public sealed record DropExternalDataSource(Expression[] Names) : Statement;

	/// <summary><c>DROP EXTERNAL FILE FORMAT</c>.</summary>
	public sealed record DropExternalFileFormat(Expression[] Names) : Statement;

	/// <summary><c>DROP EXTERNAL LANGUAGE</c>.</summary>
	public sealed record DropExternalLanguage(Expression[] Names) : Statement;

	/// <summary><c>DROP EXTERNAL MODEL</c>.</summary>
	public sealed record DropExternalModel(Expression[] Names) : Statement;

	/// <summary><c>DROP EXTERNAL RESOURCE POOL</c>.</summary>
	public sealed record DropExternalResourcePool(Expression[] Names) : Statement;

	/// <summary><c>DROP EXTERNAL TABLE</c>.</summary>
	public sealed record DropExternalTable(Expression[] Names) : Statement;

	/// <summary><c>DROP FULLTEXT CATALOG</c>.</summary>
	public sealed record DropFulltextCatalog(Expression[] Names) : Statement;

	/// <summary><c>DROP FULLTEXT STOPLIST</c>.</summary>
	public sealed record DropFulltextStoplist(Expression[] Names) : Statement;

	/// <summary><c>DROP FUNCTION</c>.</summary>
	public sealed record DropFunction(Expression[] Names) : Statement;

	/// <summary><c>DROP LOGIN</c>.</summary>
	public sealed record DropLogin(Expression[] Names) : Statement;

	/// <summary><c>DROP MESSAGE TYPE</c>.</summary>
	public sealed record DropMessageType(Expression[] Names) : Statement;

	/// <summary><c>DROP PARTITION FUNCTION</c>.</summary>
	public sealed record DropPartitionFunction(Expression[] Names) : Statement;

	/// <summary><c>DROP PARTITION SCHEME</c>.</summary>
	public sealed record DropPartitionScheme(Expression[] Names) : Statement;

	/// <summary><c>DROP PROCEDURE</c>.</summary>
	public sealed record DropProcedure(Expression[] Names) : Statement;

	/// <summary><c>DROP QUEUE</c>.</summary>
	public sealed record DropQueue(Expression[] Names) : Statement;

	/// <summary><c>DROP REMOTE SERVICE BINDING</c>.</summary>
	public sealed record DropRemoteServiceBinding(Expression[] Names) : Statement;

	/// <summary><c>DROP RESOURCE POOL</c>.</summary>
	public sealed record DropResourcePool(Expression[] Names) : Statement;

	/// <summary><c>DROP ROLE</c>.</summary>
	public sealed record DropRole(Expression[] Names) : Statement;

	/// <summary><c>DROP ROUTE</c>.</summary>
	public sealed record DropRoute(Expression[] Names) : Statement;

	/// <summary><c>DROP RULE</c>.</summary>
	public sealed record DropRule(Expression[] Names) : Statement;

	/// <summary><c>DROP SCHEMA</c>.</summary>
	public sealed record DropSchema(Expression[] Names) : Statement;

	/// <summary><c>DROP SEARCH PROPERTY LIST</c>.</summary>
	public sealed record DropSearchPropertyList(Expression[] Names) : Statement;

	/// <summary><c>DROP SECURITY POLICY</c>.</summary>
	public sealed record DropSecurityPolicy(Expression[] Names) : Statement;

	/// <summary><c>DROP SEQUENCE</c>.</summary>
	public sealed record DropSequence(Expression[] Names) : Statement;

	/// <summary><c>DROP SERVER AUDIT SPECIFICATION</c>.</summary>
	public sealed record DropServerAuditSpecification(Expression[] Names) : Statement;

	/// <summary><c>DROP SERVER AUDIT</c>.</summary>
	public sealed record DropServerAudit(Expression[] Names) : Statement;

	/// <summary><c>DROP SERVER ROLE</c>.</summary>
	public sealed record DropServerRole(Expression[] Names) : Statement;

	/// <summary><c>DROP SERVICE</c>.</summary>
	public sealed record DropService(Expression[] Names) : Statement;

	/// <summary><c>DROP STATISTICS</c>.</summary>
	public sealed record DropStatistics(Expression[] Names) : Statement;

	/// <summary><c>DROP SYNONYM</c>.</summary>
	public sealed record DropSynonym(Expression[] Names) : Statement;

	/// <summary><c>DROP TABLE</c>.</summary>
	public sealed record DropTable(Expression[] Names) : Statement;

	/// <summary><c>DROP TYPE</c>.</summary>
	public sealed record DropType(Expression[] Names) : Statement;

	/// <summary><c>DROP USER</c>.</summary>
	public sealed record DropUser(Expression[] Names) : Statement;

	/// <summary><c>DROP VIEW</c>.</summary>
	public sealed record DropView(Expression[] Names) : Statement;

	/// <summary><c>DROP WORKLOAD CLASSIFIER</c>.</summary>
	public sealed record DropWorkloadClassifier(Expression[] Names) : Statement;

	/// <summary><c>DROP WORKLOAD GROUP</c>.</summary>
	public sealed record DropWorkloadGroup(Expression[] Names) : Statement;

	/// <summary><c>DROP XML SCHEMA COLLECTION</c>.</summary>
	public sealed record DropXmlSchemaCollection(Expression[] Names) : Statement;

	/// <summary><c>DROP ASYMMETRIC KEY</c>.</summary>
	public sealed record DropAsymmetricKey(Expression[] Names) : Statement;

	/// <summary><c>DROP SYMMETRIC KEY</c>.</summary>
	public sealed record DropSymmetricKey(Expression[] Names) : Statement;

	/// <summary><c>DROP ASSEMBLY</c>.</summary>
	public sealed record DropAssembly(Expression[] Names) : Statement;

	/// <summary><c>DROP EXTERNAL LIBRARY</c>.</summary>
	public sealed record DropExternalLibrary(Expression[] Names) : Statement;

	/// <summary><c>DROP EVENT SESSION</c>.</summary>
	public sealed record DropEventSession(Expression[] Names) : Statement;

	/// <summary><c>DROP EVENT NOTIFICATION</c>.</summary>
	public sealed record DropEventNotification(Expression[] Names) : Statement;

	/// <summary><c>DROP FULLTEXT INDEX</c>.</summary>
	public sealed record DropFulltextIndex(Expression[] Names) : Statement;

	/// <summary><c>DROP INDEX</c>.</summary>
	public sealed record DropIndex(Expression[] Names) : Statement;

	/// <summary><c>DROP SIGNATURE</c>.</summary>
	public sealed record DropSignature(Expression[] Names) : Statement;

	/// <summary><c>DROP SENSITIVITY CLASSIFICATION</c>.</summary>
	public sealed record DropSensitivityClassification(Expression[] Names) : Statement;

	/// <summary><c>DROP TRIGGER</c>.</summary>
	public sealed record DropTrigger(Expression[] Names) : Statement;

	/// <summary><c>DROP MASTER KEY</c>.</summary>
	public sealed record DropMasterKey(Expression[] Names) : Statement;

	/// <summary><c>DROP DATABASE ENCRYPTION KEY</c>.</summary>
	public sealed record DropDatabaseEncryptionKey(Expression[] Names) : Statement;

	// ---- how a parser makes these ------------------------------------------------------------
	//
	// The statements build through these rather than inline in their `=>`, and that is not
	// style: `GRAM5003` said the materializing method for the statement entry point was past
	// the size at which the JIT stops optimizing, and the diagnostic's own advice is to build
	// the value in a method of your own.

	/// <summary>What a call with no statements is handed, once rather than per call.</summary>
	public static readonly Statement[] None = [];

	/// <summary>The statement the word names, for the ten that are a word and a value.</summary>
	public static Statement Commanded(string word, Expression? value) =>
		word switch
		{
			"PRINT"      => new Print(value!),
			"RETURN"     => new Return(value),
			"THROW"      => new Throw(value is null ? Expression.None : [value]),
			"GOTO"       => new GoTo(value!),
			"BREAK"      => new Break(),
			"CONTINUE"   => new Continue(),
			"CHECKPOINT" => new Checkpoint(value),
			"USE"        => new Use(value!),
			"RAISERROR"  => new RaiseError(value is null ? Expression.None : [value]),
			"WAITFOR"    => new WaitFor(value!),
			_            => throw Syntax.Unknown(word),
		};

	/// <summary>The same where the values arrived as a list that may not be there.</summary>
	public static Statement Commanded(string word, Expression[]? values) =>
		word switch
		{
			"PRINT"      => new Print(values is { Length: > 0 } some ? some[0] : null!),
			"RETURN"     => new Return(values is { Length: > 0 } some ? some[0] : null),
			"THROW"      => new Throw(values ?? Expression.None),
			"GOTO"       => new GoTo(values is { Length: > 0 } some ? some[0] : null!),
			"BREAK"      => new Break(),
			"CONTINUE"   => new Continue(),
			"CHECKPOINT" => new Checkpoint(values is { Length: > 0 } some ? some[0] : null),
			"USE"        => new Use(values is { Length: > 0 } some ? some[0] : null!),
			"RAISERROR"  => new RaiseError(values ?? Expression.None),
			"WAITFOR"    => new WaitFor(values is { Length: > 0 } some ? some[0] : null!),
			_            => throw Syntax.Unknown(word),
		};

	/// <summary>What is being backed up, as the statement it is.</summary>
	public static Statement BackedUp(string what, string? name) =>
		what switch
		{
			"DATABASE"           => new BackupDatabase(name),
			"LOG"                => new BackupTransactionLog(name),
			"SERVER"             => new BackupServer(name),
			"GROUP"              => new BackupGroup(name),
			"CERTIFICATE"        => new BackupCertificate(name),
			"MASTER KEY"         => new BackupMasterKey(name),
			"SERVICE MASTER KEY" => new BackupServiceMasterKey(name),
			"SYMMETRIC KEY"      => new BackupSymmetricKey(name),
			_                    => throw Syntax.Unknown(what),
		};

	/// <summary>What is being restored, likewise.</summary>
	public static Statement Restored(string what, string? name) =>
		what switch
		{
			"DATABASE"           => new RestoreDatabase(name),
			"LOG"                => new RestoreLog(name),
			"FILELISTONLY"       => new RestoreFileListOnly(name),
			"HEADERONLY"         => new RestoreHeaderOnly(name),
			"LABELONLY"          => new RestoreLabelOnly(name),
			"REWINDONLY"         => new RestoreRewindOnly(name),
			"VERIFYONLY"         => new RestoreVerifyOnly(name),
			"MASTER KEY"         => new RestoreMasterKey(name),
			"SERVICE MASTER KEY" => new RestoreServiceMasterKey(name),
			_                    => throw Syntax.Unknown(what),
		};

	/// <summary>The definition the words name, where the tree keeps the name and no more.</summary>
	public static Statement Defined(string what, string name) =>
		what switch
		{
			"CREATE LOGIN"            => new CreateLogin(name),
			"ALTER LOGIN"             => new AlterLogin(name),
			"CREATE USER"             => new CreateUser(name),
			"ALTER USER"              => new AlterUser(name),
			"CREATE ROLE"             => new CreateRole(name),
			"ALTER ROLE"              => new AlterRole(name),
			"CREATE APPLICATION ROLE" => new CreateApplicationRole(name),
			"ALTER APPLICATION ROLE"  => new AlterApplicationRole(name),
			"CREATE SCHEMA"           => new SchemaDefinition(name),
			"ALTER SCHEMA"            => new AlterSchema(name),
			"ALTER AUTHORIZATION"     => new AlterAuthorization(name),
			"EXTERNAL DATA SOURCE"    => new ExternalDataSourceDefinition(name),
			"EXTERNAL FILE FORMAT"    => new ExternalFileFormatDefinition(name),
			"EXTERNAL LIBRARY"        => new ExternalLibraryDefinition(name),
			"EXTERNAL RESOURCE POOL"  => new ExternalResourcePoolDefinition(name),
			"RESOURCE POOL"           => new ResourcePoolDefinition(name),
			"WORKLOAD GROUP"          => new WorkloadGroupDefinition(name),
			"SERVER AUDIT"            => new ServerAuditDefinition(name),
			"AUDIT SPECIFICATION"     => new AuditSpecificationDefinition(name),
			"EVENT SESSION"           => new EventSessionDefinition(name),
			"EVENT NOTIFICATION"      => new EventNotificationDefinition(name),
			"ENDPOINT"                => new EndpointDefinition(name),

			"FULLTEXT INDEX"             => new FullTextIndexDefinition(name),
			"ALTER FULLTEXT INDEX"       => new AlterFullTextIndex(name),
			"FULLTEXT CATALOG"           => new FullTextCatalogDefinition(name),
			"ALTER FULLTEXT CATALOG"     => new AlterFullTextCatalog(name),
			"FULLTEXT STOPLIST"          => new FullTextStopListDefinition(name),
			"ALTER FULLTEXT STOPLIST"    => new AlterFullTextStopList(name),
			"SEARCH PROPERTY LIST"       => new SearchPropertyListDefinition(name),
			"ALTER SEARCH PROPERTY LIST" => new AlterSearchPropertyList(name),

			"ASYMMETRIC KEY"                => new AsymmetricKeyDefinition(name),
			"ALTER ASYMMETRIC KEY"          => new AlterAsymmetricKey(name),
			"SYMMETRIC KEY"                 => new SymmetricKeyDefinition(name),
			"ALTER SYMMETRIC KEY"           => new AlterSymmetricKey(name),
			"CERTIFICATE"                   => new CertificateDefinition(name),
			"ALTER CERTIFICATE"             => new AlterCertificate(name),
			"MASTER KEY"                    => new MasterKeyDefinition(name),
			"ALTER MASTER KEY"              => new AlterMasterKey(name),
			"DATABASE ENCRYPTION KEY"       => new DatabaseEncryptionKeyDefinition(name),
			"ALTER DATABASE ENCRYPTION KEY" => new AlterDatabaseEncryptionKey(name),
			"COLUMN ENCRYPTION KEY"         => new ColumnEncryptionKeyDefinition(name),
			"ALTER COLUMN ENCRYPTION KEY"   => new AlterColumnEncryptionKey(name),
			"COLUMN MASTER KEY"             => new ColumnMasterKeyDefinition(name),
			"CREDENTIAL"                    => new CredentialDefinition(name),
			"DATABASE SCOPED CREDENTIAL"    => new DatabaseScopedCredentialDefinition(name),
			"SECURITY POLICY"               => new SecurityPolicyDefinition(name),
			_                               => throw Syntax.Unknown(what),
		};

	/// <summary>What is being done to a database, as the statement it is.</summary>
	public static Statement OfDatabase(string name, string action, Clause[]? settings) =>
		action switch
		{
			"CREATE"               => new CreateDatabase(name, settings ?? Clause.None),
			"SET"                  => new AlterDatabaseSet(name, settings ?? Clause.None),
			"SCOPED CONFIGURATION" => new AlterDatabaseScopedConfiguration(name, settings ?? Clause.None),
			"COLLATE"              => new AlterDatabaseCollate(name),
			"MODIFY NAME"          => new AlterDatabaseModifyName(name),
			"MODIFY FILEGROUP"     => new AlterDatabaseModifyFileGroup(name),
			"MODIFY FILE"          => new AlterDatabaseModifyFile(name),
			"MODIFY"               => new AlterDatabaseModify(name),
			"ADD FILEGROUP"        => new AlterDatabaseAddFileGroup(name),
			"ADD LOG FILE"         => new AlterDatabaseAddLogFile(name),
			"ADD FILE"             => new AlterDatabaseAddFile(name),
			"REMOVE FILEGROUP"     => new AlterDatabaseRemoveFileGroup(name),
			"REMOVE FILE"          => new AlterDatabaseRemoveFile(name),
			"REBUILD LOG"          => new AlterDatabaseRebuildLog(name),
			"PERFORM_CUTOVER"      => new AlterDatabasePerformCutover(name),
			_                      => throw Syntax.Unknown(action),
		};

	/// <summary>A permission granted, denied or revoked, as the statement it is.</summary>
	public static Statement Permitted(string kind, string[] privileges, string[] principals) =>
		kind switch
		{
			"GRANT"  => new Grant(privileges, principals),
			"DENY"   => new Deny(privileges, principals),
			"REVOKE" => new Revoke(privileges, principals),
			_        => throw Syntax.Unknown(kind),
		};

	/// <summary>An alteration before the table it is applied to is known.</summary>
	public static AlterTable Altered(string action, Clause[]? elements) =>
		new("", action, elements ?? Clause.None);

	/// <summary>The <c>DROP</c> the word names, which is what a <c>DROP</c> statement is.</summary>
	/// <remarks>
	/// The word arrives as it was written — the case the author used, and whatever spacing
	/// stood between its parts — so it is squared up before it is asked about. This and the
	/// grammar's own <c>DropKind</c> are two spellings of one catalogue and have to agree; where
	/// they have drifted apart this says so rather than quietly building the wrong node, which
	/// is a defect in this file and not in anybody's SQL.
	/// </remarks>
	public static Statement Dropped(string kind, Expression[]? some)
	{
		var names = some ?? Expression.None;

		return Syntax.Squared(kind) switch
		{
			"AGGREGATE"                    => new DropAggregate(names),
			"APPLICATION ROLE"             => new DropApplicationRole(names),
			"AVAILABILITY GROUP"           => new DropAvailabilityGroup(names),
			"BROKER PRIORITY"              => new DropBrokerPriority(names),
			"CERTIFICATE"                  => new DropCertificate(names),
			"COLUMN ENCRYPTION KEY"        => new DropColumnEncryptionKey(names),
			"COLUMN MASTER KEY"            => new DropColumnMasterKey(names),
			"CONTRACT"                     => new DropContract(names),
			"CREDENTIAL"                   => new DropCredential(names),
			"CRYPTOGRAPHIC PROVIDER"       => new DropCryptographicProvider(names),
			"DATABASE AUDIT SPECIFICATION" => new DropDatabaseAuditSpecification(names),
			"DATABASE SCOPED CREDENTIAL"   => new DropDatabaseScopedCredential(names),
			"DATABASE"                     => new DropDatabase(names),
			"DEFAULT"                      => new DropDefault(names),
			"ENDPOINT"                     => new DropEndpoint(names),
			"EXTERNAL DATA SOURCE"         => new DropExternalDataSource(names),
			"EXTERNAL FILE FORMAT"         => new DropExternalFileFormat(names),
			"EXTERNAL LANGUAGE"            => new DropExternalLanguage(names),
			"EXTERNAL MODEL"               => new DropExternalModel(names),
			"EXTERNAL RESOURCE POOL"       => new DropExternalResourcePool(names),
			"EXTERNAL TABLE"               => new DropExternalTable(names),
			"FULLTEXT CATALOG"             => new DropFulltextCatalog(names),
			"FULLTEXT STOPLIST"            => new DropFulltextStoplist(names),
			"FUNCTION"                     => new DropFunction(names),
			"LOGIN"                        => new DropLogin(names),
			"MESSAGE TYPE"                 => new DropMessageType(names),
			"PARTITION FUNCTION"           => new DropPartitionFunction(names),
			"PARTITION SCHEME"             => new DropPartitionScheme(names),
			"PROCEDURE"                    => new DropProcedure(names),
			"PROC"                         => new DropProcedure(names),
			"QUEUE"                        => new DropQueue(names),
			"REMOTE SERVICE BINDING"       => new DropRemoteServiceBinding(names),
			"RESOURCE POOL"                => new DropResourcePool(names),
			"ROLE"                         => new DropRole(names),
			"ROUTE"                        => new DropRoute(names),
			"RULE"                         => new DropRule(names),
			"SCHEMA"                       => new DropSchema(names),
			"SEARCH PROPERTY LIST"         => new DropSearchPropertyList(names),
			"SECURITY POLICY"              => new DropSecurityPolicy(names),
			"SEQUENCE"                     => new DropSequence(names),
			"SERVER AUDIT SPECIFICATION"   => new DropServerAuditSpecification(names),
			"SERVER AUDIT"                 => new DropServerAudit(names),
			"SERVER ROLE"                  => new DropServerRole(names),
			"SERVICE"                      => new DropService(names),
			"STATISTICS"                   => new DropStatistics(names),
			"SYNONYM"                      => new DropSynonym(names),
			"TABLE"                        => new DropTable(names),
			"TYPE"                         => new DropType(names),
			"USER"                         => new DropUser(names),
			"VIEW"                         => new DropView(names),
			"WORKLOAD CLASSIFIER"          => new DropWorkloadClassifier(names),
			"WORKLOAD GROUP"               => new DropWorkloadGroup(names),
			"XML SCHEMA COLLECTION"        => new DropXmlSchemaCollection(names),
			"ASYMMETRIC KEY"               => new DropAsymmetricKey(names),
			"SYMMETRIC KEY"                => new DropSymmetricKey(names),
			"ASSEMBLY"                     => new DropAssembly(names),
			"EXTERNAL LIBRARY"             => new DropExternalLibrary(names),
			"EVENT SESSION"                => new DropEventSession(names),
			"EVENT NOTIFICATION"           => new DropEventNotification(names),
			"FULLTEXT INDEX"               => new DropFulltextIndex(names),
			"INDEX"                        => new DropIndex(names),
			"SIGNATURE"                    => new DropSignature(names),
			"SENSITIVITY CLASSIFICATION"   => new DropSensitivityClassification(names),
			"TRIGGER"                      => new DropTrigger(names),
			"MASTER KEY"                   => new DropMasterKey(names),
			"DATABASE ENCRYPTION KEY"      => new DropDatabaseEncryptionKey(names),
			_ => throw new ArgumentOutOfRangeException(
				nameof(kind), kind,
				"The grammar reads this after DROP and the tree has no record for it."),
		};
	}
}

/// <summary>
/// §7 the table level: what produces rows. A statement holds one, an expression holds one,
/// and a <c>FROM</c> clause holds the <see cref="TableReference"/>s it is read over.
/// </summary>
public abstract record Query
{
	/// <summary>§7.12 <c>SELECT</c>, and the clauses under it.</summary>
	/// <remarks>
	/// One record for the whole of a query specification and its table expression, because
	/// the clauses are optional rather than alternative: what tells one query from another is
	/// which of them are empty. <see cref="From"/> holds the table references the standard
	/// writes as a comma list, which is a cross join said the older way.
	/// </remarks>
	public sealed record Specification(
		bool Distinct,
		Clause? Top,
		Clause[] Columns,
		string? Into,
		TableReference[] From,
		Expression? Where,
		Clause? GroupBy,
		Expression? Having) : Query;

	/// <summary>§7.3 <c>VALUES (…), (…)</c> — a table written out.</summary>
	public sealed record TableValueConstructor(Expression[] Rows) : Query;

	/// <summary>§7.4 <c>TABLE t</c>, which is every column and every row of one table.</summary>
	public sealed record ExplicitTable(string Name) : Query;

	/// <summary>§7.13 <c>UNION</c>, and whether it keeps duplicates.</summary>
	public sealed record Union(Query Left, Query Right, bool All) : Query;

	/// <summary>§7.13 <c>EXCEPT</c>, and whether it keeps duplicates.</summary>
	public sealed record Except(Query Left, Query Right, bool All) : Query;

	/// <summary>§7.13 <c>INTERSECT</c>, and whether it keeps duplicates.</summary>
	public sealed record Intersect(Query Left, Query Right, bool All) : Query;

	/// <summary>T-SQL's <c>INSERT … DEFAULT VALUES</c>: a row of nothing but defaults.</summary>
	public sealed record DefaultValues : Query;

	/// <summary>T-SQL's <c>BULK INSERT</c>: a file standing where a query does.</summary>
	public sealed record FromFile(Expression File) : Query;

	/// <summary>
	/// T-SQL's <c>INSERT … EXEC</c>: a procedure standing where a query stands.
	/// </summary>
	/// <remarks>
	/// An aggregation and not a kind of statement — what makes the rows here is a statement,
	/// and what the <c>INSERT</c> needs is something that makes rows, so the query level holds
	/// the statement rather than the two hierarchies meeting.
	/// </remarks>
	public sealed record FromExecute(Statement.Execute Execute) : Query;

	/// <summary>Which set operator was written, and whether it keeps duplicates.</summary>
	public static Query Combined(string operatorText, string? all, Query left, Query right) =>
		(operatorText[0] | 0x20) switch
		{
			'u' => new Union(left, right, all is not null),
			'e' => new Except(left, right, all is not null),
			_   => new Intersect(left, right, all is not null),
		};
}

/// <summary>§6 the value level, and §8 the predicates: what stands where a value does.</summary>
public abstract record Expression
{
	// ---- §6.39 the boolean tower ---------------------------------------------------------------

	/// <summary><c>OR</c>.</summary>
	public sealed record Or(Expression Left, Expression Right) : Expression;

	/// <summary><c>AND</c>.</summary>
	public sealed record And(Expression Left, Expression Right) : Expression;

	/// <summary><c>NOT</c>.</summary>
	public sealed record Not(Expression Operand) : Expression;

	/// <summary>
	/// <c>x IS NOT TRUE</c> and its fellows: what is tested, and what it is tested against.
	/// </summary>
	public sealed record IsTruth(Expression Operand, bool Negated, SqlTruth Truth) : Expression;

	// ---- §6.30 the arithmetic tower --------------------------------------------------------------
	//
	// A record per operator, because the standard writes a production per operator:
	// <numeric value expression> ::= … | <numeric value expression> <plus sign> <term>.

	/// <summary><c>+</c>.</summary>
	public sealed record Add(Expression Left, Expression Right) : Expression;

	/// <summary><c>-</c>.</summary>
	public sealed record Subtract(Expression Left, Expression Right) : Expression;

	/// <summary>§6.31 <c>||</c>, and T-SQL's <c>+</c> over strings, which is the same node.</summary>
	public sealed record Concatenate(Expression Left, Expression Right) : Expression;

	/// <summary><c>*</c>.</summary>
	public sealed record Multiply(Expression Left, Expression Right) : Expression;

	/// <summary><c>/</c>.</summary>
	public sealed record Divide(Expression Left, Expression Right) : Expression;

	/// <summary>A unary <c>-</c>.</summary>
	public sealed record Negate(Expression Operand) : Expression;

	/// <summary>A unary <c>+</c>, which the standard keeps and which changes nothing.</summary>
	public sealed record Plus(Expression Operand) : Expression;

	// ---- §8 the predicates ------------------------------------------------------------------------
	//
	// A record each, with the operands named rather than numbered. The one word that survives
	// is the comparison operator, and only because <comp op> is a production of its own.

	/// <summary>§8.2 <c>a = b</c> and its five fellows.</summary>
	public sealed record Comparison(
		Expression Left, SqlComparison Operator, Expression Right) : Expression;

	/// <summary>§8.9 <c>a = ALL (…)</c> — a comparison against every row of a subquery.</summary>
	public sealed record Quantified(
		Expression Left, SqlComparison Operator, string Quantifier, Query Query) : Expression;

	/// <summary>§8.3 <c>a BETWEEN low AND high</c>.</summary>
	public sealed record Between(
		Expression Value, bool Negated, Expression Low, Expression High) : Expression;

	/// <summary>§8.4 <c>a IN (…)</c>, whose right side is a row of values or a subquery.</summary>
	public sealed record In(Expression Value, bool Negated, Expression Source) : Expression;

	/// <summary>§8.5 <c>a LIKE p ESCAPE e</c>.</summary>
	public sealed record Like(
		Expression Value, bool Negated, Expression Pattern, Expression? Escape) : Expression;

	/// <summary>§8.7 <c>a IS NULL</c>.</summary>
	public sealed record IsNull(Expression Value, bool Negated) : Expression;

	/// <summary>§8.10 <c>EXISTS (…)</c>.</summary>
	public sealed record Exists(Query Query) : Expression;

	/// <summary>§8.11 <c>UNIQUE (…)</c>.</summary>
	public sealed record Unique(Query Query) : Expression;

	/// <summary>§8.13 <c>a MATCH UNIQUE PARTIAL (…)</c>, and the words it may be qualified by.</summary>
	public sealed record Match(Expression Value, string? Qualifier, Query Query) : Expression;

	/// <summary>§8.15 <c>a OVERLAPS b</c>, whose two sides are rows.</summary>
	public sealed record Overlaps(Expression Left, Expression Right) : Expression;

	/// <summary>
	/// SQL:1999's <c>&lt;distinct predicate&gt;</c> — <c>a IS NOT DISTINCT FROM b</c>, which
	/// is a comparison that calls two nulls equal.
	/// </summary>
	public sealed record IsDistinctFrom(
		Expression Left, bool Negated, Expression Right) : Expression;

	// ---- §6 the rest of the value level ------------------------------------------------------------

	/// <summary>
	/// A function, a set function or a cast: the name as written, its arguments, and the one
	/// word a few of them carry — <c>DISTINCT</c>, a datetime field, a trim specification, or
	/// the type a cast names.
	/// </summary>
	public sealed record RoutineInvocation(
		string Name, Expression[] Arguments, string? Word = null) : Expression;

	/// <summary>
	/// §6.12 a <c>CASE</c>, simple where it has an operand and searched where it does not.
	/// </summary>
	public sealed record Case(Expression? Operand, Clause[] Whens, Expression? Else) : Expression;

	/// <summary>§6.7 a column reference, as written, dots and all.</summary>
	public sealed record ColumnReference(string Text) : Expression;

	/// <summary>
	/// §5.3 a literal, a parameter, or one of the words that stand where a value does —
	/// <c>NULL</c>, <c>DEFAULT</c>, <c>CURRENT_USER</c>.
	/// </summary>
	public sealed record Literal(SqlLiteralKind Kind, string Text) : Expression;

	/// <summary>§7.1 a row of several values, <c>(a, b)</c>.</summary>
	public sealed record RowValueConstructor(Expression[] Values) : Expression;

	/// <summary>
	/// An argument with a word in front of it, which several of T-SQL's table-valued
	/// functions write — <c>BULK 'f'</c>, <c>CHANGES t</c>, <c>LANGUAGE 1033</c>.
	/// </summary>
	public sealed record Prefixed(string Word, Expression Value) : Expression;

	/// <summary>
	/// An argument given by name rather than by position — <c>@p = 1</c> in an
	/// <c>EXECUTE</c>, <c>FORMATFILE = '…'</c> in a rowset function.
	/// </summary>
	public sealed record NamedArgument(string Name, Expression Value) : Expression;

	/// <summary>
	/// §7.15 a subquery standing where a value does — the standard's scalar and row subqueries,
	/// which differ by how many columns they return and not by how they are written.
	/// </summary>
	public sealed record Subquery(Query Query) : Expression;

	// ---- how a parser makes these -----------------------------------------------------------------

	/// <summary>What a call with no arguments is handed, once rather than per call.</summary>
	public static readonly Expression[] None = [];

	/// <summary>The two words that stand where a value does and are always the same node.</summary>
	public static readonly Expression NullValue    = new Literal(SqlLiteralKind.Null,    "NULL");

	/// <inheritdoc cref="NullValue"/>
	public static readonly Expression DefaultValue = new Literal(SqlLiteralKind.Default, "DEFAULT");

	/// <summary>An additive operator and its two operands, as the node the operator names.</summary>
	public static Expression Additive(string operatorText, Expression left, Expression right) =>
		operatorText switch
		{
			"+" => new Add(left, right),
			"-" => new Subtract(left, right),
			_   => new Concatenate(left, right),
		};

	/// <summary>Likewise for the multiplicative pair.</summary>
	public static Expression Multiplicative(string operatorText, Expression left, Expression right) =>
		operatorText == "*" ? new Multiply(left, right) : new Divide(left, right);

	/// <summary>Likewise for the sign in front of one operand.</summary>
	public static Expression Signed(string sign, Expression operand) =>
		sign == "-" ? new Negate(operand) : new Plus(operand);

	/// <summary>
	/// The left operand written into the tail the predicate was read as.
	/// </summary>
	/// <remarks>
	/// The row is read once for all the predicates that begin with one, so the tail is built
	/// without it and the slot it left is filled here. A <c>with</c> rather than a write: the
	/// tail was built with its left side null and no one has seen it, so the copy costs one
	/// allocation and the array the operands used to live in costs none.
	/// </remarks>
	public static Expression Predicated(Expression left, Expression tail) =>
		tail switch
		{
			Comparison c => c with { Left  = left },
			Quantified q => q with { Left  = left },
			Between    b => b with { Value = left },
			In         i => i with { Value = left },
			Like       l => l with { Value = left },
			IsNull     n => n with { Value = left },
			Match      m => m with { Value = left },
			Overlaps   o => o with { Left  = left },
			_            => throw new ArgumentOutOfRangeException(
				nameof(tail), tail,
				"The grammar read a predicate tail this method does not know how to complete."),
		};
}

/// <summary>
/// §7.6 what a <c>FROM</c> clause is read over: a table by name, a query standing where one
/// does, and the joins between them.
/// </summary>
public abstract record TableReference
{
	/// <summary>
	/// §7.6 a table named, the name it is known by there, and the names its columns are given.
	/// </summary>
	public sealed record Named(string Table, string? Name, string[]? Columns) : TableReference;

	/// <summary>§7.6 a query standing where a table does.</summary>
	public sealed record Derived(Query Query, string? Name, string[]? Columns) : TableReference;

	/// <summary>
	/// §7.6 a function standing where a table does — T-SQL's rowset functions, and any
	/// table-valued function called in a <c>FROM</c> clause.
	/// </summary>
	public sealed record FunctionCall(
		Expression.RoutineInvocation Function, string? Name, string[]? Columns) : TableReference;

	/// <summary>§7.7 two sources and the join between them.</summary>
	/// <remarks>
	/// <see cref="On"/> where the join was qualified by a condition, <see cref="Using"/> where
	/// it named columns, and neither for a cross or a natural join.
	/// </remarks>
	public sealed record Joined(
		SqlJoin Kind, bool Natural, TableReference Left, TableReference Right,
		Expression? On = null, string[]? Using = null) : TableReference;

	/// <summary>What a clause with no sources is handed, once rather than per call.</summary>
	public static readonly TableReference[] None = [];

	/// <summary>A join from the words around it: the kind, and which of the two tails it had.</summary>
	public static Joined Joining(
		string? kind, string? natural, TableReference left, TableReference right,
		Expression? on, string[]? columns) =>
		new(Syntax.Joined(kind), natural is not null, left, right, on, columns);
}

/// <summary>
/// The pieces a statement, a query or an expression is made of that are none of the three —
/// what the standard writes as a clause, a specification or a definition of one element.
/// </summary>
public abstract record Clause
{
	/// <summary>§7.12 one entry of a select list: what it is, and what it is called.</summary>
	public sealed record DerivedColumn(Expression Value, string? Name) : Clause;

	/// <summary>§7.12 <c>*</c>, or <c>t.*</c> — which is no column, so it is not one.</summary>
	public sealed record QualifiedAsterisk(string? Qualifier) : Clause;

	/// <summary>§10.10 one <c>ORDER BY</c> entry: what to sort by, and which way.</summary>
	/// <remarks>
	/// <see cref="Order"/> is what was written and not what it means: the standard makes
	/// ascending the default, and a statement that says so and one that does not are two
	/// different texts.
	/// </remarks>
	public sealed record SortSpecification(Expression Value, SqlOrder Order) : Clause;

	/// <summary>
	/// §10.10 the whole <c>ORDER BY</c>, with the two clauses the reference writes inside it:
	/// how many rows to step over, and how many to take.
	/// </summary>
	public sealed record OrderBy(Clause[] By, Expression? Offset, Expression? Fetch) : Clause;

	/// <summary>
	/// T-SQL's <c>TOP (n) PERCENT WITH TIES</c> — how many rows a query specification hands
	/// back, which the standard says with <see cref="OrderBy.Fetch"/> instead.
	/// </summary>
	public sealed record Top(Expression Value, bool Percent, string? With) : Clause;

	/// <summary>
	/// §7.9 <c>GROUP BY</c>: what the rows are grouped by, and the two things T-SQL writes
	/// around the list — <c>ALL</c> in front and <c>WITH CUBE</c> or <c>WITH ROLLUP</c> after.
	/// </summary>
	public sealed record GroupBy(bool All, Expression[] By, string? With) : Clause;

	/// <summary>§7.17 a named query, written in front of the statement that uses it.</summary>
	public sealed record CommonTableExpression(
		string Name, string[]? Columns, Query Query) : Clause;

	/// <summary>
	/// T-SQL's <c>FOR XML</c>, <c>FOR JSON</c> and <c>FOR BROWSE</c>: what shape the rows come
	/// back in rather than what they are.
	/// </summary>
	public sealed record For(string Kind, string[] Options) : Clause;

	/// <summary>One hint, as it was written.</summary>
	/// <remarks>
	/// A hint is a language of its own — <c>OPTIMIZE FOR (@v = 20)</c>,
	/// <c>TABLE HINT (t, FORCESEEK (ix (a, b)))</c>, <c>MAXDOP 2</c> — and every one of them
	/// says how a statement is to be run rather than what it means. The text is what the tree
	/// keeps, which loses nothing and claims nothing: a reader who wants a hint taken apart is
	/// asking the engine's question, not the language's.
	/// </remarks>
	public sealed record Hint(string Text) : Clause;

	/// <summary>
	/// What may stand in a <c>WITH</c> beside the named queries — <c>XMLNAMESPACES (…)</c> and
	/// <c>CHANGE_TRACKING_CONTEXT (…)</c>, which are not queries and are written in the same
	/// breath as one.
	/// </summary>
	public sealed record WithOption(string Text) : Clause;

	/// <summary>
	/// T-SQL's assignment written in a select list — <c>SELECT @a += 1</c>, which takes the
	/// value rather than returning it and is not a column however much it looks like one.
	/// </summary>
	public sealed record VariableAssignment(
		string Variable, string Operator, Expression Value) : Clause;

	/// <summary>§6.12 one <c>WHEN … THEN …</c> of a <c>CASE</c>.</summary>
	public sealed record When(Expression Test, Expression Result) : Clause;

	/// <summary>
	/// §14.14 one entry of a <c>SET</c>: what is assigned, the operator it was assigned with
	/// where that was not a plain <c>=</c>, and the value.
	/// </summary>
	public sealed record Set(string Target, string? Operator, Expression Value) : Clause;

	/// <summary>
	/// §14.12 one arm of a merge: whether it fired on a match, which side the match was missing
	/// from, what else had to be true, and what to do.
	/// </summary>
	public sealed record MergeWhen(
		bool OnMatch, string? By, Expression? Condition, Statement Action) : Clause;

	/// <summary>
	/// One variable: its name, the type as written, and what it was given to start with.
	/// </summary>
	public sealed record VariableDeclaration(string Name, string? Type, Expression? Value) : Clause;

	/// <summary>One parameter of a routine: its name, its type, and its default.</summary>
	public sealed record ParameterDeclaration(string Name, string? Type, Expression? Value) : Clause;

	/// <summary>
	/// §11.4 one column: its name, the type as written, the expression where it is computed
	/// rather than stored, and what is said about it after that.
	/// </summary>
	public sealed record ColumnDefinition(
		string Name, string? Type, Expression? Computed, Clause[] Constraints) : Clause;

	/// <summary>
	/// §11.6 a constraint or an index, written on a column or on the table: its name where it
	/// was given one, which kind it is, and the columns it names.
	/// </summary>
	public sealed record ConstraintDefinition(
		string? Name, string Kind, string[]? Columns, Expression? Check) : Clause;

	/// <summary>One setting of a database: its name, and what it was set to.</summary>
	/// <remarks>
	/// Not a statement and not a kind of its own. There are some two hundred of them, they
	/// differ by edition and by version, and each is a name and a value however much SQL Server
	/// means by it.
	/// </remarks>
	public sealed record DatabaseOption(string Name, Expression? Value) : Clause;

	/// <summary>What a statement with no clauses is handed, once rather than per call.</summary>
	public static readonly Clause[] None = [];

	/// <summary>A constraint written without a name, which is most of them.</summary>
	public static ConstraintDefinition Constrained(
		string kind, string[]? columns, Expression? check) => new(null, kind, columns, check);

	/// <summary>A named thing dropped or declared, where only the name and the kind matter.</summary>
	public static ConstraintDefinition Marked(string kind, string? name) =>
		new(name, kind, null, null);

	/// <summary>
	/// A <c>TOP</c> from the words written after it, which are two questions and one rule:
	/// whether it is a share rather than a count, and what it does about a tie.
	/// </summary>
	public static Top Topped(Expression value, string? suffix)
	{
		if (suffix is null)
			return new Top(value, false, null);

		var squared = Syntax.Squared(suffix);
		var percent = squared.StartsWith("PERCENT", StringComparison.Ordinal);
		var at      = squared.IndexOf("WITH ", StringComparison.Ordinal);

		return new Top(value, percent, at < 0 ? null : squared[(at + 5)..]);
	}

	/// <summary>
	/// The order clause and the two the reference writes inside it, gathered where they were
	/// read apart. Nothing where nothing was written.
	/// </summary>
	public static OrderBy? Ordered(Clause[]? by, OrderBy? window) =>
		by is null && window is null
			? null
			: new OrderBy(by ?? None, window?.Offset, window?.Fetch);
}

/// <summary>
/// What every hierarchy needs and none of them owns: the lists a grammar gathers, and the
/// words it matched read back as the constants they stand for.
/// </summary>
/// <remarks>
/// The words are told apart by length and first letter, which costs nothing and allocates
/// nothing — the alternative is the matched text, and a capture cuts a string.
/// </remarks>
public static class Syntax
{
	/// <summary>What a call with no names is handed, once rather than per call.</summary>
	public static readonly string[] NoNames = [];

	/// <summary>A head and a tail as one array, which is what a separated list comes to.</summary>
	public static T[] Listed<T>(T first, T[]? rest)
	{
		if (rest is null || rest.Length == 0)
			return [first];

		var all = new T[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	/// <summary>A head and a tail of names as one array, the way <see cref="Listed"/> does nodes.</summary>
	public static string[] Named(string first, string[]? rest) => Listed(first, rest);

	/// <summary>Whether a word that is <c>ON</c> or <c>OFF</c> was the first of the two.</summary>
	public static bool Switched(string word) => (word[0] | 0x20) == 'o' && word.Length == 2;

	/// <summary>Which way a sort specification asked for its rows (§10.10).</summary>
	public static SqlOrder Ordered(string? order) =>
		order is null   ? SqlOrder.Unspecified :
		(order[0] | 0x20) == 'a' ? SqlOrder.Ascending
		                         : SqlOrder.Descending;

	/// <summary>Which comparison operator was written (§8.2's <c>&lt;comp op&gt;</c>).</summary>
	public static SqlComparison Compared(string operatorText) => operatorText switch
	{
		"="  => SqlComparison.Equal,
		"<>" => SqlComparison.NotEqual,
		"<"  => SqlComparison.Less,
		"<=" => SqlComparison.LessOrEqual,
		">"  => SqlComparison.Greater,
		_    => SqlComparison.GreaterOrEqual,
	};

	/// <summary>Which join was written, from the words in front of <c>JOIN</c>.</summary>
	/// <remarks><c>OUTER</c> is noise beside <c>LEFT</c>, <c>RIGHT</c> and <c>FULL</c> (§7.7).</remarks>
	public static SqlJoin Joined(string? kind)
	{
		// The text is the whole of what was written, `LEFT OUTER` and not `LEFT`, so the
		// first word is what is asked about.
		if (kind is null)
			return SqlJoin.Inner;

		if (kind.StartsWith("LEFT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Left;

		if (kind.StartsWith("RIGHT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Right;

		return kind.StartsWith("FULL", StringComparison.OrdinalIgnoreCase)
			? SqlJoin.Full
			: SqlJoin.Inner;
	}

	/// <summary>A word the grammar matched, as the one constant that stands for it.</summary>
	public static SqlTruth TruthOf(string word) =>
		(word[0] | 0x20) switch
		{
			't' => SqlTruth.True,
			'f' => SqlTruth.False,
			_   => SqlTruth.Unknown,
		};

	public static string Aggregate(string word) =>
		(word[0] | 0x20) switch
		{
			'a' => "AVG",
			's' => "SUM",
			'c' => "COUNT",
			'm' => (word[1] | 0x20) == 'a' ? "MAX" : "MIN",
			_   => word,
		};

	public static string Quantified(string word) =>
		(word[0] | 0x20) switch
		{
			'a' => (word[1] | 0x20) == 'l' ? "ALL" : "ANY",
			's' => "SOME",
			_   => word,
		};

	public static string? Distinctly(string? word) =>
		word is null ? null : (word[0] | 0x20) == 'd' ? "DISTINCT" : "ALL";

	/// <summary>What a match predicate was qualified by, as one word or two.</summary>
	public static string? Matched(string? unique, string? kind)
	{
		var partial = kind is not null && (kind[0] | 0x20) == 'p';

		return unique is null
			? kind is null ? null : partial ? "PARTIAL" : "FULL"
			: kind is null ? "UNIQUE" : partial ? "UNIQUE PARTIAL" : "UNIQUE FULL";
	}

	/// <summary>A run of words as one upper-case word per space.</summary>
	public static string Squared(string words)
	{
		var made = new System.Text.StringBuilder(words.Length);

		foreach (var c in words)
			if (char.IsWhiteSpace(c))
			{
				if (made.Length > 0 && made[made.Length - 1] != ' ')
					made.Append(' ');
			}
			else
			{
				made.Append(char.ToUpperInvariant(c));
			}

		return made.ToString().TrimEnd();
	}

	/// <summary>A word the grammar read and this file has no record for.</summary>
	/// <remarks>
	/// The grammar and the switches in this file are two spellings of one catalogue and have to
	/// agree; where they have drifted apart this says so rather than quietly building the wrong
	/// node. A defect in this file, not in anybody's SQL.
	/// </remarks>
	public static ArgumentOutOfRangeException Unknown(string word) =>
		new(nameof(word), word, "The grammar reads this and the tree has no record for it.");
}

/// <summary>§8.2 <c>&lt;comp op&gt;</c>, which the standard writes as a production of its own.</summary>
public enum SqlComparison
{
	Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual,
}

/// <summary>Which join (§7.7), and the two T-SQL adds.</summary>
/// <remarks>
/// An apply reads its right side once per row of its left, which is a question about
/// evaluation rather than shape — but it is written differently and refused where a join
/// would be read, so the tree says which was written.
/// </remarks>
public enum SqlJoin
{
	Cross, Inner, Left, Right, Full, Union, CrossApply, OuterApply,
}

/// <summary>Which way a sort was asked for, and whether it was asked for at all (§10.10).</summary>
public enum SqlOrder
{
	Unspecified, Ascending, Descending,
}

/// <summary>What a literal is, where the text alone does not say.</summary>
public enum SqlLiteralKind
{
	Number, Text, National, Bit, Hex, Date, Time, Timestamp, Interval,
	Null, Default, Parameter, Special,
}

/// <summary>The three truth values a test compares against (§6.39).</summary>
public enum SqlTruth
{
	True, False, Unknown,
}
