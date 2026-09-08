using System;

namespace DotGram.Parsers;

/// <summary>
/// What a SQL parser builds — <see cref="SqlStandard92"/>, <see cref="TransactSql"/>, or
/// anything else that reads the same grammar.
/// </summary>
/// <remarks>
/// <para>
/// <b>One tree for every dialect, named after the grammar.</b> A node is called what the
/// production it comes from is called: the standard's name where the standard has the
/// concept — <c>QuerySpecification</c>, <c>DerivedColumn</c>, <c>TableReference</c>,
/// <c>TableDefinition</c>, <c>SortSpecification</c> — and the dialect's published name
/// where it does not, which is most of the DDL. So a reader with the specification open
/// can find the node, and a reader with the node can find the specification; and a second
/// dialect that reads the same production builds the same node rather than one of its own.
/// That is the point of it: several parsers, one tree.
/// </para>
/// <para>
/// <b>One class, with its descendants inside it, one level deep.</b> Nested rather than
/// beside, so that the tree is one name to import and one place to read. Nothing here
/// derives from anything but <see cref="SqlNode"/>: a consumer switches on the record and
/// is done, and a node added below another would be a node half its readers do not see.
/// </para>
/// <para>
/// <b>A record per production, where the productions differ.</b> This file used to argue
/// the opposite — that a kind written as a field beats a type per production, because a
/// visitor over forty types has forty methods. The argument holds for the value level and
/// was kept there: <see cref="Predicate"/> is one record with a kind, because the nine
/// predicates of §8 are one shape with different operands, and <see cref="SqlOperator"/>
/// tells an <c>OR</c> from a <c>*</c> for the same reason. It does not hold for statements.
/// A <c>DROP TABLE</c> and a <c>DROP VIEW</c> are not one shape with a word in it; they are
/// two statements that happen to be spelled alike, and a consumer that has to read the word
/// to know which is a consumer doing the parser's work twice.
/// </para>
/// <para>
/// Aggregation over inheritance, still. A predicate holds its operands in an array rather
/// than in named fields per kind, so the shape of <c>x BETWEEN a AND b</c> is "the kind is
/// Between and there are three operands" rather than a type of its own. What each operand
/// means is in <see cref="SqlPredicateKind"/>'s documentation, where a reader looks once.
/// </para>
/// <para>
/// <b>The tree knows how it is made.</b> The words a grammar matches and the lists it
/// gathers become nodes through the statics here — <see cref="Compared"/>,
/// <see cref="TruthOf"/>, <see cref="Listed"/> and the rest — so that any parser of this
/// language, generated under any carrier or written by hand, builds the same tree by the
/// same code. They used to be the generated parser's own, and a second parser of the same
/// grammar could not reach them.
/// </para>
/// <para>
/// Nothing here is a position: the tree says what was written, not where. A consumer that
/// needs the text back cuts it from the input itself, which is what §7.6 is for.
/// </para>
/// </remarks>
public abstract record SqlNode
{
	/// <summary>Two operands and the operator between them.</summary>
	public sealed record BinaryExpression(SqlOperator Operator, SqlNode Left, SqlNode Right) : SqlNode;

	/// <summary>One operand and the operator in front of it — <c>NOT</c>, and the signs.</summary>
	public sealed record UnaryExpression(SqlOperator Operator, SqlNode Operand) : SqlNode;

	/// <summary>
	/// <c>x IS NOT TRUE</c> and its fellows: what is tested, and what it is tested against.
	/// </summary>
	public sealed record BooleanTest(SqlNode Operand, bool Negated, SqlTruth Truth) : SqlNode;

	/// <summary>
	/// A predicate (§8): its kind, whether <c>NOT</c> was written in the middle of it, and
	/// its operands in the order the standard writes them.
	/// </summary>
	/// <remarks>
	/// <see cref="Operator"/> is set where a comparison operator was written, and
	/// <see cref="Word"/> carries the one word some predicates need and no two need alike —
	/// the quantifier of a quantified comparison, and <c>UNIQUE</c>, <c>PARTIAL</c> or
	/// <c>FULL</c> on a match.
	/// </remarks>
	public sealed record Predicate(
		SqlPredicateKind Kind, bool Negated, SqlNode[] Operands,
		SqlOperator? Operator = null, string? Word = null) : SqlNode;

	/// <summary>
	/// A function, a set function or a cast: the name as written, its arguments, and the one
	/// word a few of them carry — <c>DISTINCT</c>, a datetime field, a trim specification, or
	/// the type a cast names.
	/// </summary>
	public sealed record RoutineInvocation(string Name, SqlNode[] Arguments, string? Word = null) : SqlNode;

	/// <summary>A <c>CASE</c>, simple where it has an operand and searched where it does not.</summary>
	public sealed record CaseExpression(SqlNode? Operand, WhenClause[] Whens, SqlNode? Else) : SqlNode;

	/// <summary>One <c>WHEN … THEN …</c> of a <c>CASE</c>.</summary>
	public sealed record WhenClause(SqlNode Test, SqlNode Result) : SqlNode;

	/// <summary>A column reference, as written, dots and all.</summary>
	public sealed record ColumnReference(string Text) : SqlNode;

	/// <summary>
	/// A literal, a parameter, or one of the words that stand where a value does —
	/// <c>NULL</c>, <c>DEFAULT</c>, <c>CURRENT_USER</c>.
	/// </summary>
	public sealed record Literal(SqlLiteralKind Kind, string Text) : SqlNode;

	/// <summary>A row of several values, <c>(a, b)</c>.</summary>
	public sealed record RowValueConstructor(SqlNode[] Values) : SqlNode;

	/// <summary>
	/// A subquery, kept as the text between its parentheses.
	/// </summary>
	/// <remarks>
	/// The grammar does not read a <c>SELECT</c> — §6 and §8 are what it covers, and a query
	/// specification is a chapter of its own. It reads far enough to find the parenthesis
	/// that closes, and hands over what stood inside.
	/// </remarks>
	public sealed record Subquery(string Text) : SqlNode;

	// ---- §7, the query level -----------------------------------------------------------------

	/// <summary>§7.9 <c>SELECT</c>, and the clauses under it.</summary>
	/// <remarks>
	/// One record for the whole of a query specification and its table expression, because
	/// the clauses are optional rather than alternative: what tells one query from another
	/// is which of them are empty. <see cref="From"/> holds the table references the
	/// standard writes as a comma list, which is a cross join said the older way.
	/// </remarks>
	public sealed record QuerySpecification(
		bool Distinct,
		SqlNode[] Columns,
		SqlNode[] From,
		SqlNode? Where,
		SqlNode[] GroupBy,
		SqlNode? Having) : SqlNode;

	/// <summary>One entry of a select list: what it is, and what it is called.</summary>
	public sealed record DerivedColumn(SqlNode Value, string? Name) : SqlNode;

	/// <summary><c>*</c>, or <c>t.*</c> — which is no column, so it is not one.</summary>
	public sealed record QualifiedAsterisk(string? Qualifier) : SqlNode;

	/// <summary>
	/// §7.4 one entry of a <c>FROM</c> clause: a table by name or a query standing where
	/// one does, the name it is known by there, and the names its columns are given.
	/// </summary>
	public sealed record TableReference(
		string? Table, SqlNode? Derived, string? Name, string[]? Columns) : SqlNode;

	/// <summary>§7.5 two sources and the join between them.</summary>
	/// <remarks>
	/// <see cref="On"/> where the join was qualified by a condition, <see cref="Using"/>
	/// where it named columns, and neither for a cross or a natural join.
	/// </remarks>
	public sealed record JoinedTable(
		SqlJoin Kind, bool Natural, SqlNode Left, SqlNode Right,
		SqlNode? On = null, string[]? Using = null) : SqlNode;

	/// <summary>§7.2 <c>VALUES (…), (…)</c> — a table written out.</summary>
	public sealed record TableValueConstructor(SqlNode[] Rows) : SqlNode;

	/// <summary>§13.1 a query and the order its rows are asked for in.</summary>
	public sealed record SelectStatement(SqlNode Of, SqlNode[] By) : SqlNode;

	/// <summary>One <c>ORDER BY</c> entry: what to sort by, and which way.</summary>
	public sealed record SortSpecification(SqlNode Value, bool Down) : SqlNode;

	// ---- the statements ----------------------------------------------------------------------
	//
	// One record per kind, beside the others rather than under a base of their own, which is
	// what this ADT is: one root and one level of descendants. A statement is a node like any
	// other because a statement holds queries and a query holds statements — `INSERT … SELECT`
	// one way and `SELECT … FROM (MERGE … OUTPUT …)` the other.
	//
	// What is read and dropped keeps growing, and it is worth naming here rather than only in
	// the grammar: `TOP`, `OVER`, the hints, the windows, a named query's `WITH`, and now
	// `OUTPUT`. Each is a decoration on how a statement runs or what it returns rather than on
	// what it is, and each would be a field on four records here. When one of them is wanted,
	// it is one field — but none is wanted yet, and a field nobody reads is a field that drifts.

	/// <summary>Rows written into a table, from a list, a query or nothing at all.</summary>
	/// <remarks>
	/// <see cref="Rows"/> is a <see cref="TableValueConstructor"/> where they were written out, a query
	/// where they come from one, and a <see cref="RowValueConstructor"/> of nothing for `DEFAULT VALUES`.
	/// </remarks>
	public sealed record InsertStatement(SqlNode? Target, string[]? Columns, SqlNode Rows) : SqlNode;

	/// <summary>Rows changed in place: what to change, to what, and which rows.</summary>
	/// <remarks>
	/// <see cref="From"/> is T-SQL's extension and not the standard's: a second `FROM` naming
	/// the tables the rows to change are found by joining.
	/// </remarks>
	public sealed record UpdateStatement(
		SqlNode? Target, SqlNode[] Set, SqlNode[] From, SqlNode? Where) : SqlNode;

	/// <summary>Rows removed, and the same two ways of saying which.</summary>
	public sealed record DeleteStatement(SqlNode? Target, SqlNode[] From, SqlNode? Where) : SqlNode;

	/// <summary>
	/// One statement that inserts, updates and deletes, according to what a join found.
	/// </summary>
	public sealed record MergeStatement(
		SqlNode? Target, SqlNode Using, SqlNode On, SqlNode[] Whens) : SqlNode;

	/// <summary>
	/// One arm of a merge: whether it fired on a match, which side the match was missing
	/// from, what else had to be true, and what to do.
	/// </summary>
	public sealed record MergeWhenClause(
		bool OnMatch, string? By, SqlNode? Condition, SqlNode Action) : SqlNode;

	/// <summary>
	/// One entry of a `SET`: what is assigned, the operator it was assigned with where that
	/// was not a plain `=`, and the value.
	/// </summary>
	public sealed record SetClause(string Target, string? Operator, SqlNode Value) : SqlNode;

	// ---- the procedural level ----------------------------------------------------------------
	//
	// Structure gets a record and shapelessness does not. A block holds statements, a
	// conditional holds two, a declaration holds a list — each of those is a shape and each
	// has one. `PRINT`, `RETURN`, `GOTO`, `BREAK`, `THROW`, `WAITFOR` and `USE` are a word and
	// some values and nothing else, and giving each of them a record of its own would be
	// seven names for one shape rather than seven shapes.

	/// <summary>`BEGIN … END`, and the body of anything that has one.</summary>
	public sealed record CompoundStatement(SqlNode[] Statements) : SqlNode;

	/// <summary>`IF … ELSE`, where either arm is one statement and a block is one.</summary>
	public sealed record IfStatement(SqlNode Condition, SqlNode Then, SqlNode? Else) : SqlNode;

	/// <summary>`WHILE`, and the one statement it repeats.</summary>
	public sealed record WhileStatement(SqlNode Condition, SqlNode Body) : SqlNode;

	/// <summary>`BEGIN TRY … END TRY BEGIN CATCH … END CATCH`.</summary>
	public sealed record TryCatchStatement(SqlNode[] Tried, SqlNode[] Caught) : SqlNode;

	/// <summary>One `DECLARE`, which may declare several.</summary>
	public sealed record DeclareStatement(SqlNode[] Variables) : SqlNode;

	/// <summary>
	/// One variable: its name, the type as written, and what it was given to start with.
	/// </summary>
	public sealed record VariableDeclaration(string Name, string? Type, SqlNode? Value) : SqlNode;

	/// <summary>A transaction begun, committed, rolled back or saved, and its name.</summary>
	public sealed record TransactionStatement(string Kind, string? Name) : SqlNode;

	/// <summary>
	/// `EXECUTE`: what is called, with what, and the variable the return code goes to.
	/// </summary>
	public sealed record ExecuteStatement(string? Into, string Name, SqlNode[] Arguments) : SqlNode;

	// ---- the tables ---------------------------------------------------------------------------

	/// <summary>A table declared: its name, and the columns, constraints and indexes in it.</summary>
	public sealed record TableDefinition(string Name, SqlNode[] Elements) : SqlNode;

	/// <summary>
	/// One column: its name, the type as written, the expression where it is computed rather
	/// than stored, and what is said about it after that.
	/// </summary>
	public sealed record ColumnDefinition(
		string Name, string? Type, SqlNode? Computed, SqlNode[] Constraints) : SqlNode;

	/// <summary>
	/// A constraint or an index, written on a column or on the table: its name where it was
	/// given one, which kind it is, and the columns it names.
	/// </summary>
	public sealed record ConstraintDefinition(
		string? Name, string Kind, string[]? Columns, SqlNode? Check) : SqlNode;

	/// <summary>A table changed: its name, what is being done, and to what.</summary>
	public sealed record AlterTableStatement(string Name, string Action, SqlNode[] Elements) : SqlNode;

	// ---- the routines --------------------------------------------------------------------------

	/// <summary>A procedure: its name, what it takes, and what it does.</summary>
	public sealed record CreateProcedureStatement(
		string Name, SqlNode[] Parameters, SqlNode[] Body) : SqlNode;

	/// <summary>
	/// A function, and what it returns says which of the three shapes it is: a type for a
	/// scalar, `TABLE` for either of the two that return rows.
	/// </summary>
	public sealed record CreateFunctionStatement(
		string Name, SqlNode[] Parameters, string? Returns, SqlNode[] Body) : SqlNode;

	/// <summary>A trigger: what it is on, what fires it, and what it does then.</summary>
	public sealed record CreateTriggerStatement(
		string Name, string On, string[] Events, SqlNode[] Body) : SqlNode;

	/// <summary>A view, which is a name given to a query.</summary>
	public sealed record ViewDefinition(string Name, string[]? Columns, SqlNode Selects) : SqlNode;

	/// <summary>One parameter of a routine: its name, its type, and its default.</summary>
	public sealed record ParameterDeclaration(string Name, string? Type, SqlNode? Value) : SqlNode;

	// ---- indexes and permissions ---------------------------------------------------------------

	/// <summary>An index declared: its name, what it is on, and the columns it is over.</summary>
	public sealed record CreateIndexStatement(string Name, string On, string[]? Columns) : SqlNode;

	/// <summary>An index changed: which one, on what, and what is being done to it.</summary>
	public sealed record AlterIndexStatement(string Name, string On, string Action) : SqlNode;


	// ---- a word and some values ----------------------------------------------------------------
	//
	// Ten statements that are a word and what follows it. They shared one record until the
	// tree was made to say what the grammar says, and a reader who wanted the `PRINT` had to
	// look at a string to find it.

	/// <summary>One value printed.</summary>
	public sealed record PrintStatement(SqlNode Value) : SqlNode;

	/// <summary>A routine left, with a code where one was given.</summary>
	public sealed record ReturnStatement(SqlNode? Value) : SqlNode;

	/// <summary>An error raised, or the caught one raised again.</summary>
	public sealed record ThrowStatement(SqlNode[] Arguments) : SqlNode;

	/// <summary>A jump to a label.</summary>
	public sealed record GoToStatement(SqlNode Label) : SqlNode;

	/// <summary>A loop left.</summary>
	public sealed record BreakStatement : SqlNode;

	/// <summary>A loop begun again.</summary>
	public sealed record ContinueStatement : SqlNode;

	/// <summary>The log written out.</summary>
	public sealed record CheckpointStatement(SqlNode? Value) : SqlNode;

	/// <summary>The database the rest of the batch is read against.</summary>
	public sealed record UseStatement(SqlNode Name) : SqlNode;

	/// <summary>The older spelling of an error raised.</summary>
	public sealed record RaiseErrorStatement(SqlNode[] Arguments) : SqlNode;

	/// <summary>A delay, or a time to wait until.</summary>
	public sealed record WaitForStatement(SqlNode Value) : SqlNode;

	// ---- who may connect, what lives outside, and what the server watches ----------------------
	//
	// One record each, and the name only: what these are set to is an option list the grammar
	// reads and drops, and a field nobody reads is a field that drifts. When one of them is
	// wanted it is one field on one record here, which is what having a record each is for.

	/// <summary><c>CREATE LOGIN</c>.</summary>
	public sealed record CreateLoginStatement(string Name) : SqlNode;

	/// <summary><c>ALTER LOGIN</c>.</summary>
	public sealed record AlterLoginStatement(string Name) : SqlNode;

	/// <summary><c>CREATE USER</c>.</summary>
	public sealed record CreateUserStatement(string Name) : SqlNode;

	/// <summary><c>ALTER USER</c>.</summary>
	public sealed record AlterUserStatement(string Name) : SqlNode;

	/// <summary><c>CREATE ROLE</c>.</summary>
	public sealed record CreateRoleStatement(string Name) : SqlNode;

	/// <summary><c>ALTER ROLE</c>.</summary>
	public sealed record AlterRoleStatement(string Name) : SqlNode;

	/// <summary><c>CREATE APPLICATION ROLE</c>.</summary>
	public sealed record CreateApplicationRoleStatement(string Name) : SqlNode;

	/// <summary><c>ALTER APPLICATION ROLE</c>.</summary>
	public sealed record AlterApplicationRoleStatement(string Name) : SqlNode;

	/// <summary><c>CREATE SCHEMA</c>.</summary>
	public sealed record SchemaDefinition(string Name) : SqlNode;

	/// <summary><c>ALTER SCHEMA</c>.</summary>
	public sealed record AlterSchemaStatement(string Name) : SqlNode;

	/// <summary><c>ALTER AUTHORIZATION</c>.</summary>
	public sealed record AlterAuthorizationStatement(string Name) : SqlNode;

	/// <summary><c>EXTERNAL DATA SOURCE</c>.</summary>
	public sealed record ExternalDataSourceDefinition(string Name) : SqlNode;

	/// <summary><c>EXTERNAL FILE FORMAT</c>.</summary>
	public sealed record ExternalFileFormatDefinition(string Name) : SqlNode;

	/// <summary><c>EXTERNAL LIBRARY</c>.</summary>
	public sealed record ExternalLibraryDefinition(string Name) : SqlNode;

	/// <summary><c>EXTERNAL RESOURCE POOL</c>.</summary>
	public sealed record ExternalResourcePoolDefinition(string Name) : SqlNode;

	/// <summary><c>RESOURCE POOL</c>.</summary>
	public sealed record ResourcePoolDefinition(string Name) : SqlNode;

	/// <summary><c>WORKLOAD GROUP</c>.</summary>
	public sealed record WorkloadGroupDefinition(string Name) : SqlNode;

	/// <summary><c>SERVER AUDIT</c>.</summary>
	public sealed record ServerAuditDefinition(string Name) : SqlNode;

	/// <summary><c>AUDIT SPECIFICATION</c>.</summary>
	public sealed record AuditSpecificationDefinition(string Name) : SqlNode;

	/// <summary><c>EVENT SESSION</c>.</summary>
	public sealed record EventSessionDefinition(string Name) : SqlNode;

	/// <summary><c>EVENT NOTIFICATION</c>.</summary>
	public sealed record EventNotificationDefinition(string Name) : SqlNode;

	/// <summary><c>ENDPOINT</c>.</summary>
	public sealed record EndpointDefinition(string Name) : SqlNode;

	// ---- the database --------------------------------------------------------------------------

	/// <summary><c>CREATE DATABASE</c>, and the files it is made of.</summary>
	public sealed record CreateDatabaseStatement(string Name, SqlNode[] Files) : SqlNode;

	/// <summary><c>ALTER DATABASE … SET</c>, and what it was set to.</summary>
	public sealed record AlterDatabaseSetStatement(string Name, SqlNode[] Settings) : SqlNode;

	/// <summary><c>ALTER DATABASE … SCOPED CONFIGURATION</c>, and what it was set to.</summary>
	public sealed record AlterDatabaseScopedConfigurationStatement(string Name, SqlNode[] Settings) : SqlNode;

	/// <summary><c>ALTER DATABASE … COLLATE</c>.</summary>
	public sealed record AlterDatabaseCollateStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … MODIFY NAME</c>.</summary>
	public sealed record AlterDatabaseModifyNameStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … MODIFY FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseModifyFileGroupStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … MODIFY FILE</c>.</summary>
	public sealed record AlterDatabaseModifyFileStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … MODIFY</c>.</summary>
	public sealed record AlterDatabaseModifyStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … ADD FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseAddFileGroupStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … ADD LOG FILE</c>.</summary>
	public sealed record AlterDatabaseAddLogFileStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … ADD FILE</c>.</summary>
	public sealed record AlterDatabaseAddFileStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … REMOVE FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseRemoveFileGroupStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … REMOVE FILE</c>.</summary>
	public sealed record AlterDatabaseRemoveFileStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … REBUILD LOG</c>.</summary>
	public sealed record AlterDatabaseRebuildLogStatement(string Name) : SqlNode;

	/// <summary><c>ALTER DATABASE … PERFORM_CUTOVER</c>.</summary>
	public sealed record AlterDatabasePerformCutoverStatement(string Name) : SqlNode;

	/// <summary>One setting of a database: its name, and what it was set to.</summary>
	/// <remarks>
	/// Not a statement and not a kind of its own. There are some two hundred of them, they
	/// differ by edition and by version, and each is a name and a value however much SQL Server
	/// means by it.
	/// </remarks>
	public sealed record DatabaseOption(string Name, SqlNode? Value) : SqlNode;

	// ---- the SET statements --------------------------------------------------------------------

	/// <summary><c>SET TRANSACTION ISOLATION LEVEL</c>.</summary>
	public sealed record SetTransactionIsolationLevelStatement(string Level) : SqlNode;

	/// <summary><c>SET IDENTITY_INSERT t ON</c>.</summary>
	public sealed record SetIdentityInsertStatement(string Table, bool On) : SqlNode;

	/// <summary>A setting or several turned on or off — <c>SET ANSI_NULLS, ANSI_PADDING ON</c>.</summary>
	public sealed record SetOptionStatement(string[] Options, bool On) : SqlNode;

	/// <summary>A setting given a value — <c>SET ROWCOUNT 10</c>, <c>SET LANGUAGE us_english</c>.</summary>
	public sealed record SetCommandStatement(string Option, SqlNode? Value) : SqlNode;

	// ---- §12.1 the permissions -----------------------------------------------------------------

	/// <summary><c>GRANT</c>: what is being said about, and to whom.</summary>
	public sealed record GrantStatement(string[] Privileges, string[] Principals) : SqlNode;

	/// <summary><c>DENY</c>: what is being said about, and to whom.</summary>
	public sealed record DenyStatement(string[] Privileges, string[] Principals) : SqlNode;

	/// <summary><c>REVOKE</c>: what is being said about, and to whom.</summary>
	public sealed record RevokeStatement(string[] Privileges, string[] Principals) : SqlNode;

	// ---- the full-text catalogue -----------------------------------------------------------------

	/// <summary><c>CREATE FULLTEXT INDEX</c>, which is named by the table it is on.</summary>
	public sealed record FullTextIndexDefinition(string On) : SqlNode;

	/// <summary><c>ALTER FULLTEXT INDEX</c>.</summary>
	public sealed record AlterFullTextIndexStatement(string On) : SqlNode;

	/// <summary><c>CREATE FULLTEXT CATALOG</c>.</summary>
	public sealed record FullTextCatalogDefinition(string Name) : SqlNode;

	/// <summary><c>ALTER FULLTEXT CATALOG</c>.</summary>
	public sealed record AlterFullTextCatalogStatement(string Name) : SqlNode;

	/// <summary><c>CREATE FULLTEXT STOPLIST</c>.</summary>
	public sealed record FullTextStopListDefinition(string Name) : SqlNode;

	/// <summary><c>ALTER FULLTEXT STOPLIST</c>.</summary>
	public sealed record AlterFullTextStopListStatement(string Name) : SqlNode;

	/// <summary><c>CREATE SEARCH PROPERTY LIST</c>.</summary>
	public sealed record SearchPropertyListDefinition(string Name) : SqlNode;

	/// <summary><c>ALTER SEARCH PROPERTY LIST</c>.</summary>
	public sealed record AlterSearchPropertyListStatement(string Name) : SqlNode;

	// ---- backup and restore ----------------------------------------------------------------------
	//
	// Two statements and one shape between them: what is being copied, the devices it goes to
	// or comes from, and a long option list. Nine records because there are nine statements —
	// a log is not a database and a header is not a file list, and the reference gives each
	// its own page.

	/// <summary><c>BACKUP DATABASE</c>.</summary>
	public sealed record BackupDatabaseStatement(string? Name) : SqlNode;

	/// <summary><c>BACKUP LOG</c>.</summary>
	public sealed record BackupTransactionLogStatement(string? Name) : SqlNode;

	/// <summary><c>BACKUP SERVER</c>, which names nothing: there is one.</summary>
	public sealed record BackupServerStatement(string? Name) : SqlNode;

	/// <summary><c>BACKUP GROUP</c>.</summary>
	public sealed record BackupGroupStatement(string? Name) : SqlNode;

	/// <summary><c>BACKUP CERTIFICATE</c>.</summary>
	public sealed record BackupCertificateStatement(string? Name) : SqlNode;

	/// <summary><c>BACKUP MASTER KEY</c>.</summary>
	public sealed record BackupMasterKeyStatement(string? Name) : SqlNode;

	/// <summary><c>BACKUP SERVICE MASTER KEY</c>.</summary>
	public sealed record BackupServiceMasterKeyStatement(string? Name) : SqlNode;

	/// <summary><c>BACKUP SYMMETRIC KEY</c>.</summary>
	public sealed record BackupSymmetricKeyStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE DATABASE</c>.</summary>
	public sealed record RestoreDatabaseStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE LOG</c>.</summary>
	public sealed record RestoreLogStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE FILELISTONLY</c>.</summary>
	public sealed record RestoreFileListOnlyStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE HEADERONLY</c>.</summary>
	public sealed record RestoreHeaderOnlyStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE LABELONLY</c>.</summary>
	public sealed record RestoreLabelOnlyStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE REWINDONLY</c>.</summary>
	public sealed record RestoreRewindOnlyStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE VERIFYONLY</c>.</summary>
	public sealed record RestoreVerifyOnlyStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE MASTER KEY</c>.</summary>
	public sealed record RestoreMasterKeyStatement(string? Name) : SqlNode;

	/// <summary><c>RESTORE SERVICE MASTER KEY</c>.</summary>
	public sealed record RestoreServiceMasterKeyStatement(string? Name) : SqlNode;

	// ---- what a statement drops ----------------------------------------------------------------
	//
	// Sixty-four records of one shape, because sixty-four statements of one shape is what
	// the reference has: `DROP TABLE` and `DROP VIEW` are spelled alike and are not the same
	// statement, and a consumer that reads a word to tell them apart is doing the parser's
	// work twice. The grammar reads the shape once and `Dropped` turns the word into the
	// record, so the catalogue of names is here, in C#, and the grammar still says one thing.

	/// <summary><c>DROP AGGREGATE</c>.</summary>
	public sealed record DropAggregateStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP APPLICATION ROLE</c>.</summary>
	public sealed record DropApplicationRoleStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP AVAILABILITY GROUP</c>.</summary>
	public sealed record DropAvailabilityGroupStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP BROKER PRIORITY</c>.</summary>
	public sealed record DropBrokerPriorityStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP CERTIFICATE</c>.</summary>
	public sealed record DropCertificateStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP COLUMN ENCRYPTION KEY</c>.</summary>
	public sealed record DropColumnEncryptionKeyStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP COLUMN MASTER KEY</c>.</summary>
	public sealed record DropColumnMasterKeyStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP CONTRACT</c>.</summary>
	public sealed record DropContractStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP CREDENTIAL</c>.</summary>
	public sealed record DropCredentialStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP CRYPTOGRAPHIC PROVIDER</c>.</summary>
	public sealed record DropCryptographicProviderStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP DATABASE AUDIT SPECIFICATION</c>.</summary>
	public sealed record DropDatabaseAuditSpecificationStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP DATABASE SCOPED CREDENTIAL</c>.</summary>
	public sealed record DropDatabaseScopedCredentialStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP DATABASE</c>.</summary>
	public sealed record DropDatabaseStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP DEFAULT</c>.</summary>
	public sealed record DropDefaultStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP ENDPOINT</c>.</summary>
	public sealed record DropEndpointStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EXTERNAL DATA SOURCE</c>.</summary>
	public sealed record DropExternalDataSourceStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EXTERNAL FILE FORMAT</c>.</summary>
	public sealed record DropExternalFileFormatStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EXTERNAL LANGUAGE</c>.</summary>
	public sealed record DropExternalLanguageStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EXTERNAL MODEL</c>.</summary>
	public sealed record DropExternalModelStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EXTERNAL RESOURCE POOL</c>.</summary>
	public sealed record DropExternalResourcePoolStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EXTERNAL TABLE</c>.</summary>
	public sealed record DropExternalTableStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP FULLTEXT CATALOG</c>.</summary>
	public sealed record DropFulltextCatalogStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP FULLTEXT STOPLIST</c>.</summary>
	public sealed record DropFulltextStoplistStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP FUNCTION</c>.</summary>
	public sealed record DropFunctionStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP LOGIN</c>.</summary>
	public sealed record DropLoginStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP MESSAGE TYPE</c>.</summary>
	public sealed record DropMessageTypeStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP PARTITION FUNCTION</c>.</summary>
	public sealed record DropPartitionFunctionStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP PARTITION SCHEME</c>.</summary>
	public sealed record DropPartitionSchemeStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP PROCEDURE</c>.</summary>
	public sealed record DropProcedureStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP QUEUE</c>.</summary>
	public sealed record DropQueueStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP REMOTE SERVICE BINDING</c>.</summary>
	public sealed record DropRemoteServiceBindingStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP RESOURCE POOL</c>.</summary>
	public sealed record DropResourcePoolStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP ROLE</c>.</summary>
	public sealed record DropRoleStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP ROUTE</c>.</summary>
	public sealed record DropRouteStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP RULE</c>.</summary>
	public sealed record DropRuleStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SCHEMA</c>.</summary>
	public sealed record DropSchemaStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SEARCH PROPERTY LIST</c>.</summary>
	public sealed record DropSearchPropertyListStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SECURITY POLICY</c>.</summary>
	public sealed record DropSecurityPolicyStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SEQUENCE</c>.</summary>
	public sealed record DropSequenceStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SERVER AUDIT SPECIFICATION</c>.</summary>
	public sealed record DropServerAuditSpecificationStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SERVER AUDIT</c>.</summary>
	public sealed record DropServerAuditStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SERVER ROLE</c>.</summary>
	public sealed record DropServerRoleStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SERVICE</c>.</summary>
	public sealed record DropServiceStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP STATISTICS</c>.</summary>
	public sealed record DropStatisticsStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SYNONYM</c>.</summary>
	public sealed record DropSynonymStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP TABLE</c>.</summary>
	public sealed record DropTableStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP TYPE</c>.</summary>
	public sealed record DropTypeStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP USER</c>.</summary>
	public sealed record DropUserStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP VIEW</c>.</summary>
	public sealed record DropViewStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP WORKLOAD CLASSIFIER</c>.</summary>
	public sealed record DropWorkloadClassifierStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP WORKLOAD GROUP</c>.</summary>
	public sealed record DropWorkloadGroupStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP XML SCHEMA COLLECTION</c>.</summary>
	public sealed record DropXmlSchemaCollectionStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP ASYMMETRIC KEY</c>.</summary>
	public sealed record DropAsymmetricKeyStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SYMMETRIC KEY</c>.</summary>
	public sealed record DropSymmetricKeyStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP ASSEMBLY</c>.</summary>
	public sealed record DropAssemblyStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EXTERNAL LIBRARY</c>.</summary>
	public sealed record DropExternalLibraryStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EVENT SESSION</c>.</summary>
	public sealed record DropEventSessionStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP EVENT NOTIFICATION</c>.</summary>
	public sealed record DropEventNotificationStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP FULLTEXT INDEX</c>.</summary>
	public sealed record DropFulltextIndexStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP INDEX</c>.</summary>
	public sealed record DropIndexStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SIGNATURE</c>.</summary>
	public sealed record DropSignatureStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP SENSITIVITY CLASSIFICATION</c>.</summary>
	public sealed record DropSensitivityClassificationStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP TRIGGER</c>.</summary>
	public sealed record DropTriggerStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP MASTER KEY</c>.</summary>
	public sealed record DropMasterKeyStatement(SqlNode[] Names) : SqlNode;

	/// <summary><c>DROP DATABASE ENCRYPTION KEY</c>.</summary>
	public sealed record DropDatabaseEncryptionKeyStatement(SqlNode[] Names) : SqlNode;


	// ---- how a parser makes these ------------------------------------------------------------

	/// <summary>What a call with no arguments is handed, once rather than per call.</summary>
	public static readonly SqlNode[] None = [];

	/// <summary>The statement the word names, for the ten that are a word and a value.</summary>
	public static SqlNode Commanded(string word, SqlNode? value) =>
		word switch
		{
			"PRINT" => new PrintStatement(value!),
			"RETURN" => new ReturnStatement(value),
			"THROW" => new ThrowStatement(value is null ? None : [value]),
			"GOTO" => new GoToStatement(value!),
			"BREAK" => new BreakStatement(),
			"CONTINUE" => new ContinueStatement(),
			"CHECKPOINT" => new CheckpointStatement(value),
			"USE" => new UseStatement(value!),
			"RAISERROR" => new RaiseErrorStatement(value is null ? None : [value]),
			"WAITFOR" => new WaitForStatement(value!),
			_ => throw Unknown(word),
		};

	/// <summary>The same where the values arrived as a list that may not be there.</summary>
	public static SqlNode Commanded(string word, SqlNode[]? values) =>
		word switch
		{
			"PRINT" => new PrintStatement(values is { Length: > 0 } some ? some[0] : null!),
			"RETURN" => new ReturnStatement(values is { Length: > 0 } some ? some[0] : null),
			"THROW" => new ThrowStatement(values ?? None),
			"GOTO" => new GoToStatement(values is { Length: > 0 } some ? some[0] : null!),
			"BREAK" => new BreakStatement(),
			"CONTINUE" => new ContinueStatement(),
			"CHECKPOINT" => new CheckpointStatement(values is { Length: > 0 } some ? some[0] : null),
			"USE" => new UseStatement(values is { Length: > 0 } some ? some[0] : null!),
			"RAISERROR" => new RaiseErrorStatement(values ?? None),
			"WAITFOR" => new WaitForStatement(values is { Length: > 0 } some ? some[0] : null!),
			_ => throw Unknown(word),
		};

	/// <summary>What is being backed up, as the statement it is.</summary>
	public static SqlNode BackedUp(string what, string? name) =>
		what switch
		{
			"DATABASE"           => new BackupDatabaseStatement(name),
			"LOG"                => new BackupTransactionLogStatement(name),
			"SERVER"             => new BackupServerStatement(name),
			"GROUP"              => new BackupGroupStatement(name),
			"CERTIFICATE"        => new BackupCertificateStatement(name),
			"MASTER KEY"         => new BackupMasterKeyStatement(name),
			"SERVICE MASTER KEY" => new BackupServiceMasterKeyStatement(name),
			"SYMMETRIC KEY"      => new BackupSymmetricKeyStatement(name),
			_                    => throw Unknown(what),
		};

	/// <summary>What is being restored, likewise.</summary>
	public static SqlNode Restored(string what, string? name) =>
		what switch
		{
			"DATABASE"           => new RestoreDatabaseStatement(name),
			"LOG"                => new RestoreLogStatement(name),
			"FILELISTONLY"       => new RestoreFileListOnlyStatement(name),
			"HEADERONLY"         => new RestoreHeaderOnlyStatement(name),
			"LABELONLY"          => new RestoreLabelOnlyStatement(name),
			"REWINDONLY"         => new RestoreRewindOnlyStatement(name),
			"VERIFYONLY"         => new RestoreVerifyOnlyStatement(name),
			"MASTER KEY"         => new RestoreMasterKeyStatement(name),
			"SERVICE MASTER KEY" => new RestoreServiceMasterKeyStatement(name),
			_                    => throw Unknown(what),
		};

	/// <summary>The definition the words name, where the tree keeps the name and no more.</summary>
	public static SqlNode Defined(string what, string name) =>
		what switch
		{
			"CREATE LOGIN" => new CreateLoginStatement(name),
			"ALTER LOGIN" => new AlterLoginStatement(name),
			"CREATE USER" => new CreateUserStatement(name),
			"ALTER USER" => new AlterUserStatement(name),
			"CREATE ROLE" => new CreateRoleStatement(name),
			"ALTER ROLE" => new AlterRoleStatement(name),
			"CREATE APPLICATION ROLE" => new CreateApplicationRoleStatement(name),
			"ALTER APPLICATION ROLE" => new AlterApplicationRoleStatement(name),
			"CREATE SCHEMA" => new SchemaDefinition(name),
			"ALTER SCHEMA" => new AlterSchemaStatement(name),
			"ALTER AUTHORIZATION" => new AlterAuthorizationStatement(name),
			"EXTERNAL DATA SOURCE" => new ExternalDataSourceDefinition(name),
			"EXTERNAL FILE FORMAT" => new ExternalFileFormatDefinition(name),
			"EXTERNAL LIBRARY" => new ExternalLibraryDefinition(name),
			"EXTERNAL RESOURCE POOL" => new ExternalResourcePoolDefinition(name),
			"RESOURCE POOL" => new ResourcePoolDefinition(name),
			"WORKLOAD GROUP" => new WorkloadGroupDefinition(name),
			"SERVER AUDIT" => new ServerAuditDefinition(name),
			"AUDIT SPECIFICATION" => new AuditSpecificationDefinition(name),
			"EVENT SESSION" => new EventSessionDefinition(name),
			"EVENT NOTIFICATION" => new EventNotificationDefinition(name),
			"ENDPOINT" => new EndpointDefinition(name),

			"FULLTEXT INDEX" => new FullTextIndexDefinition(name),
			"ALTER FULLTEXT INDEX" => new AlterFullTextIndexStatement(name),
			"FULLTEXT CATALOG" => new FullTextCatalogDefinition(name),
			"ALTER FULLTEXT CATALOG" => new AlterFullTextCatalogStatement(name),
			"FULLTEXT STOPLIST" => new FullTextStopListDefinition(name),
			"ALTER FULLTEXT STOPLIST" => new AlterFullTextStopListStatement(name),
			"SEARCH PROPERTY LIST" => new SearchPropertyListDefinition(name),
			"ALTER SEARCH PROPERTY LIST" => new AlterSearchPropertyListStatement(name),
			_ => throw Unknown(what),
		};

	/// <summary>What is being done to a database, as the statement it is.</summary>
	public static SqlNode OfDatabase(string name, string action, SqlNode[]? settings) =>
		action switch
		{
			"CREATE" => new CreateDatabaseStatement(name, settings ?? None),
			"SET" => new AlterDatabaseSetStatement(name, settings ?? None),
			"SCOPED CONFIGURATION" => new AlterDatabaseScopedConfigurationStatement(name, settings ?? None),
			"COLLATE" => new AlterDatabaseCollateStatement(name),
			"MODIFY NAME" => new AlterDatabaseModifyNameStatement(name),
			"MODIFY FILEGROUP" => new AlterDatabaseModifyFileGroupStatement(name),
			"MODIFY FILE" => new AlterDatabaseModifyFileStatement(name),
			"MODIFY" => new AlterDatabaseModifyStatement(name),
			"ADD FILEGROUP" => new AlterDatabaseAddFileGroupStatement(name),
			"ADD LOG FILE" => new AlterDatabaseAddLogFileStatement(name),
			"ADD FILE" => new AlterDatabaseAddFileStatement(name),
			"REMOVE FILEGROUP" => new AlterDatabaseRemoveFileGroupStatement(name),
			"REMOVE FILE" => new AlterDatabaseRemoveFileStatement(name),
			"REBUILD LOG" => new AlterDatabaseRebuildLogStatement(name),
			"PERFORM_CUTOVER" => new AlterDatabasePerformCutoverStatement(name),
			_ => throw Unknown(action),
		};

	/// <summary>A permission granted, denied or revoked, as the statement it is.</summary>
	public static SqlNode Permitted(string kind, string[] privileges, string[] principals) =>
		kind switch
		{
			"GRANT"  => new GrantStatement(privileges, principals),
			"DENY"   => new DenyStatement(privileges, principals),
			"REVOKE" => new RevokeStatement(privileges, principals),
			_        => throw Unknown(kind),
		};

	/// <summary>A word the grammar read and this file has no record for.</summary>
	/// <remarks>
	/// The grammar and the switches above are two spellings of one catalogue and have to
	/// agree; where they have drifted apart this says so rather than quietly building the
	/// wrong node. A defect in this file, not in anybody's SQL.
	/// </remarks>
	static ArgumentOutOfRangeException Unknown(string word) =>
		new(nameof(word), word, "The grammar reads this and the tree has no record for it.");

	/// <summary>The `DROP` the word names, which is what a `DROP` statement is.</summary>
	/// <remarks>
	/// The word arrives as it was written — the case the author used, and whatever spacing
	/// stood between its parts — so it is squared up before it is asked about. This and the
	/// grammar's own `DropKind` are two spellings of one catalogue and have to agree; where
	/// they have drifted apart this says so rather than quietly building the wrong node,
	/// which is a defect in this file and not in anybody's SQL.
	/// </remarks>
	public static SqlNode Dropped(string kind, SqlNode[]? some)
	{
		var names = some ?? None;

		return Squared(kind) switch
		{
			"AGGREGATE" => new DropAggregateStatement(names),
			"APPLICATION ROLE" => new DropApplicationRoleStatement(names),
			"AVAILABILITY GROUP" => new DropAvailabilityGroupStatement(names),
			"BROKER PRIORITY" => new DropBrokerPriorityStatement(names),
			"CERTIFICATE" => new DropCertificateStatement(names),
			"COLUMN ENCRYPTION KEY" => new DropColumnEncryptionKeyStatement(names),
			"COLUMN MASTER KEY" => new DropColumnMasterKeyStatement(names),
			"CONTRACT" => new DropContractStatement(names),
			"CREDENTIAL" => new DropCredentialStatement(names),
			"CRYPTOGRAPHIC PROVIDER" => new DropCryptographicProviderStatement(names),
			"DATABASE AUDIT SPECIFICATION" => new DropDatabaseAuditSpecificationStatement(names),
			"DATABASE SCOPED CREDENTIAL" => new DropDatabaseScopedCredentialStatement(names),
			"DATABASE" => new DropDatabaseStatement(names),
			"DEFAULT" => new DropDefaultStatement(names),
			"ENDPOINT" => new DropEndpointStatement(names),
			"EXTERNAL DATA SOURCE" => new DropExternalDataSourceStatement(names),
			"EXTERNAL FILE FORMAT" => new DropExternalFileFormatStatement(names),
			"EXTERNAL LANGUAGE" => new DropExternalLanguageStatement(names),
			"EXTERNAL MODEL" => new DropExternalModelStatement(names),
			"EXTERNAL RESOURCE POOL" => new DropExternalResourcePoolStatement(names),
			"EXTERNAL TABLE" => new DropExternalTableStatement(names),
			"FULLTEXT CATALOG" => new DropFulltextCatalogStatement(names),
			"FULLTEXT STOPLIST" => new DropFulltextStoplistStatement(names),
			"FUNCTION" => new DropFunctionStatement(names),
			"LOGIN" => new DropLoginStatement(names),
			"MESSAGE TYPE" => new DropMessageTypeStatement(names),
			"PARTITION FUNCTION" => new DropPartitionFunctionStatement(names),
			"PARTITION SCHEME" => new DropPartitionSchemeStatement(names),
			"PROCEDURE" => new DropProcedureStatement(names),
			"PROC" => new DropProcedureStatement(names),
			"QUEUE" => new DropQueueStatement(names),
			"REMOTE SERVICE BINDING" => new DropRemoteServiceBindingStatement(names),
			"RESOURCE POOL" => new DropResourcePoolStatement(names),
			"ROLE" => new DropRoleStatement(names),
			"ROUTE" => new DropRouteStatement(names),
			"RULE" => new DropRuleStatement(names),
			"SCHEMA" => new DropSchemaStatement(names),
			"SEARCH PROPERTY LIST" => new DropSearchPropertyListStatement(names),
			"SECURITY POLICY" => new DropSecurityPolicyStatement(names),
			"SEQUENCE" => new DropSequenceStatement(names),
			"SERVER AUDIT SPECIFICATION" => new DropServerAuditSpecificationStatement(names),
			"SERVER AUDIT" => new DropServerAuditStatement(names),
			"SERVER ROLE" => new DropServerRoleStatement(names),
			"SERVICE" => new DropServiceStatement(names),
			"STATISTICS" => new DropStatisticsStatement(names),
			"SYNONYM" => new DropSynonymStatement(names),
			"TABLE" => new DropTableStatement(names),
			"TYPE" => new DropTypeStatement(names),
			"USER" => new DropUserStatement(names),
			"VIEW" => new DropViewStatement(names),
			"WORKLOAD CLASSIFIER" => new DropWorkloadClassifierStatement(names),
			"WORKLOAD GROUP" => new DropWorkloadGroupStatement(names),
			"XML SCHEMA COLLECTION" => new DropXmlSchemaCollectionStatement(names),
			"ASYMMETRIC KEY" => new DropAsymmetricKeyStatement(names),
			"SYMMETRIC KEY" => new DropSymmetricKeyStatement(names),
			"ASSEMBLY" => new DropAssemblyStatement(names),
			"EXTERNAL LIBRARY" => new DropExternalLibraryStatement(names),
			"EVENT SESSION" => new DropEventSessionStatement(names),
			"EVENT NOTIFICATION" => new DropEventNotificationStatement(names),
			"FULLTEXT INDEX" => new DropFulltextIndexStatement(names),
			"INDEX" => new DropIndexStatement(names),
			"SIGNATURE" => new DropSignatureStatement(names),
			"SENSITIVITY CLASSIFICATION" => new DropSensitivityClassificationStatement(names),
			"TRIGGER" => new DropTriggerStatement(names),
			"MASTER KEY" => new DropMasterKeyStatement(names),
			"DATABASE ENCRYPTION KEY" => new DropDatabaseEncryptionKeyStatement(names),
			_ => throw new ArgumentOutOfRangeException(
				nameof(kind),
				kind,
				"The grammar reads this after DROP and the tree has no record for it."),
		};
	}

	/// <summary>A run of words as one upper-case word per space.</summary>
	static string Squared(string words)
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

	// The statements build through these rather than inline in their `=>`, and that is not
	// style: `GRAM5003` said the materializing method for the statement entry point was past
	// the size at which the JIT stops optimizing, and the diagnostic's own advice is to build
	// the value in a method of your own. Eleven `new SqlNode.Command(word, v is null ? None :
	// new SqlNode[] { v })` in one switch is a great deal of emitted branching for very
	// little said; one call each is the same tree and a fraction of the code.

	/// <summary>A constraint written without a name, which is most of them.</summary>
	public static ConstraintDefinition Constrained(string kind, string[]? columns, SqlNode? check) =>
		new(null, kind, columns, check);

	/// <summary>A named thing dropped or declared, where only the name and the kind matter.</summary>
	public static ConstraintDefinition Marked(string kind, string? name) => new(name, kind, null, null);

	/// <summary>An alteration before the table it is applied to is known.</summary>
	public static AlterTableStatement Altered(string action, SqlNode[]? elements) =>
		new("", action, elements ?? None);

	/// <summary>The two words that stand where a value does and are always the same node.</summary>
	public static readonly SqlNode NullValue    = new Literal(SqlLiteralKind.Null,    "NULL");
	public static readonly SqlNode DefaultValue = new Literal(SqlLiteralKind.Default, "DEFAULT");

	/// <summary>A head and a tail as one array, which is what a separated list comes to.</summary>
	public static SqlNode[] Listed(SqlNode first, SqlNode[]? rest)
	{
		if (rest is null || rest.Length == 0)
			return [first];

		var all = new SqlNode[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	/// <summary>
	/// The left operand written into the tail the predicate was read as.
	/// </summary>
	/// <remarks>
	/// The row is read once for all nine predicates, so the tail is built without it and
	/// the slot it left is filled here. Written into the array rather than copied onto a
	/// new record: the array is this predicate's own and nothing has seen it yet.
	/// </remarks>
	public static Predicate Predicated(SqlNode left, Predicate tail)
	{
		tail.Operands[0] = left;

		return tail;
	}

	/// <summary>Which set operator was written, and whether it keeps duplicates.</summary>
	public static SqlOperator Combined(string operatorText, string? all) =>
		operatorText.ToUpperInvariant() switch
		{
			"UNION"     => all is null ? SqlOperator.Union     : SqlOperator.UnionAll,
			"EXCEPT"    => all is null ? SqlOperator.Except    : SqlOperator.ExceptAll,
			_           => all is null ? SqlOperator.Intersect : SqlOperator.IntersectAll,
		};

	/// <summary>Whether a word that is `ON` or `OFF` was the first of the two.</summary>
	public static bool Switched(string word) => (word[0] | 0x20) == 'o' && word.Length == 2;

	/// <summary>Whether a sort specification asked for descending order (§13.1).</summary>
	public static bool Descending(string? order) =>
		string.Equals(order, "DESC", StringComparison.OrdinalIgnoreCase);

	/// <summary>A join from the words around it: the kind, and which of the two tails it had.</summary>
	public static JoinedTable Joining(
		string? kind, string? natural, SqlNode left, SqlNode right, SqlNode? on, string[]? columns) =>
		new(Joined(kind), natural is not null, left, right, on, columns);

	/// <summary>Which join was written, from the words in front of <c>JOIN</c>.</summary>
	/// <remarks><c>OUTER</c> is noise beside <c>LEFT</c>, <c>RIGHT</c> and <c>FULL</c> (§7.5).</remarks>
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

	/// <summary>A head and a tail of names as one array, the way <see cref="Listed"/> does nodes.</summary>
	public static string[] Named(string first, string[]? rest)
	{
		if (rest is null || rest.Length == 0)
			return [first];

		var all = new string[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	public static SqlOperator Additive(string operatorText) => operatorText switch
	{
		"+" => SqlOperator.Add,
		"-" => SqlOperator.Subtract,
		_   => SqlOperator.Concatenate,
	};

	public static SqlOperator Multiplicative(string operatorText) =>
		operatorText == "*" ? SqlOperator.Multiply : SqlOperator.Divide;

	public static SqlOperator Signed(string sign) =>
		sign == "-" ? SqlOperator.Negate : SqlOperator.Identity;

	public static SqlOperator Compared(string operatorText) => operatorText switch
	{
		"="  => SqlOperator.Equal,
		"<>" => SqlOperator.NotEqual,
		"<"  => SqlOperator.Less,
		"<=" => SqlOperator.LessOrEqual,
		">"  => SqlOperator.Greater,
		_    => SqlOperator.GreaterOrEqual,
	};

	/// <summary>
	/// A word the grammar matched, as the one constant that stands for it.
	/// </summary>
	/// <remarks>
	/// The text is what the author wrote, in whatever case they wrote it; the tree says the
	/// word. Told apart by length and first letter, which costs nothing and allocates
	/// nothing — the alternative is the matched text, and a capture cuts a string.
	/// </remarks>
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

	public static string? Quantified(string? word) =>
		word is null
			? null
			: (word[0] | 0x20) switch
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
}

/// <summary>What stands between two operands, or in front of one.</summary>
public enum SqlOperator
{
	Or, And, Not,
	Add, Subtract, Concatenate, Multiply, Divide,
	Negate, Identity,
	Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual,

	/// <summary>§7.10, where the operands are tables rather than values.</summary>
	Union, UnionAll, Except, ExceptAll, Intersect, IntersectAll,
}

/// <summary>Which join (§7.5).</summary>
public enum SqlJoin
{
	Cross, Inner, Left, Right, Full, Union,
}

/// <summary>
/// Which predicate (§8), and what its operands are.
/// </summary>
/// <remarks>
/// The first operand is the row on the left in every kind that has one, so a consumer
/// that only wants what is being tested reads <c>Operands[0]</c> without asking which
/// predicate it is.
/// <list type="bullet">
/// <item><see cref="Comparison"/> — the two sides, and <c>Operator</c>.</item>
/// <item><see cref="Quantified"/> — the left side and the subquery, with <c>Operator</c> and the quantifier as the word.</item>
/// <item><see cref="Between"/> — the value, the low bound, the high bound.</item>
/// <item><see cref="In"/> — the value, and one operand that is a row of the listed values or a subquery.</item>
/// <item><see cref="Like"/> — the value, the pattern, and the escape where one was written.</item>
/// <item><see cref="IsNull"/> — the value alone.</item>
/// <item><see cref="Exists"/> and <see cref="Unique"/> — the subquery alone.</item>
/// <item><see cref="Match"/> — the value and the subquery, with the modifiers as the word.</item>
/// <item><see cref="Overlaps"/> — the two rows.</item>
/// </list>
/// </remarks>
public enum SqlPredicateKind
{
	Comparison, Quantified, Between, In, Like, IsNull, Exists, Unique, Match, Overlaps,
}

/// <summary>What a literal is, where the text alone does not say.</summary>
public enum SqlLiteralKind
{
	Number, Text, National, Bit, Hex, Date, Time, Timestamp, Interval,
	Null, Default, Parameter, Special,
}

/// <summary>The three truth values a test compares against (§8.13).</summary>
public enum SqlTruth
{
	True, False, Unknown,
}
