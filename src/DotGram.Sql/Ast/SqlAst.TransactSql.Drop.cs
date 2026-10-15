using System;

namespace DotGram.Sql.Ast;

// T-SQL's DROP statements (proposal 18): a record for each kind of object, even where two kinds' slots coincide. A word
// that names what is dropped is a record; IF EXISTS, REMOVE PROVIDER KEY and the rest say how, and are members. The
// shapes are the grammar's (TransactSql.gram, DropBody and the kind groups it calls), which differs from the published
// blocks where the engine does.

public abstract partial record Statement
{
	// [IF EXISTS] and a list of names as a table's: DropListedKind.

	/// <summary>T-SQL: DROP AGGREGATE (Transact-SQL). <c>DROP AGGREGATE [IF EXISTS] name [, …]</c>.</summary>
	public sealed record DropAggregate(SqlList<QualifiedName> Names) : Statement
	{
		/// <summary>T-SQL: DROP AGGREGATE (Transact-SQL). <c>IF EXISTS</c>.</summary>
		public bool IfExists { get; init; }
	}

	/// <summary>T-SQL: DROP DEFAULT (Transact-SQL). <c>DROP DEFAULT [IF EXISTS] name [, …]</c>.</summary>
	public sealed record DropDefault(SqlList<QualifiedName> Names) : Statement
	{
		/// <summary>T-SQL: DROP DEFAULT (Transact-SQL). <c>IF EXISTS</c>.</summary>
		public bool IfExists { get; init; }
	}

	/// <summary>T-SQL: DROP RULE (Transact-SQL). <c>DROP RULE [IF EXISTS] name [, …]</c>.</summary>
	public sealed record DropRule(SqlList<QualifiedName> Names) : Statement
	{
		/// <summary>T-SQL: DROP RULE (Transact-SQL). <c>IF EXISTS</c>.</summary>
		public bool IfExists { get; init; }
	}

	/// <summary>T-SQL: DROP SECURITY POLICY (Transact-SQL). <c>DROP SECURITY POLICY [IF EXISTS] name [, …]</c>.</summary>
	public sealed record DropSecurityPolicy(SqlList<QualifiedName> Names) : Statement
	{
		/// <summary>T-SQL: DROP SECURITY POLICY (Transact-SQL). <c>IF EXISTS</c>.</summary>
		public bool IfExists { get; init; }
	}

	/// <summary>T-SQL: DROP SYNONYM (Transact-SQL). <c>DROP SYNONYM [IF EXISTS] name [, …]</c>.</summary>
	public sealed record DropSynonym(SqlList<QualifiedName> Names) : Statement
	{
		/// <summary>T-SQL: DROP SYNONYM (Transact-SQL). <c>IF EXISTS</c>.</summary>
		public bool IfExists { get; init; }
	}

	// [IF EXISTS] and one name of one part: DropSingleKind.

	/// <summary>T-SQL: DROP USER (Transact-SQL). <c>DROP USER [IF EXISTS] name</c>.</summary>
	public sealed record DropUser(Identifier Name) : Statement
	{
		/// <summary>T-SQL: DROP USER (Transact-SQL). <c>IF EXISTS</c>.</summary>
		public bool IfExists { get; init; }
	}

	// One name as a table's, and no IF EXISTS: DropQualifiedKind.

	/// <summary>T-SQL: DROP CONTRACT (Transact-SQL). <c>DROP CONTRACT name</c>.</summary>
	public sealed record DropContract(QualifiedName Name) : Statement;

	/// <summary>T-SQL: DROP EXTERNAL TABLE (Transact-SQL). <c>DROP EXTERNAL TABLE name</c>.</summary>
	public sealed record DropExternalTable(QualifiedName Name) : Statement;

	/// <summary>T-SQL: DROP MESSAGE TYPE (Transact-SQL). <c>DROP MESSAGE TYPE name</c>.</summary>
	public sealed record DropMessageType(QualifiedName Name) : Statement;

	/// <summary>T-SQL: DROP QUEUE (Transact-SQL). <c>DROP QUEUE name</c>.</summary>
	public sealed record DropQueue(QualifiedName Name) : Statement;

	/// <summary>T-SQL: DROP REMOTE SERVICE BINDING (Transact-SQL). <c>DROP REMOTE SERVICE BINDING name</c>.</summary>
	public sealed record DropRemoteServiceBinding(QualifiedName Name) : Statement;

	/// <summary>T-SQL: DROP ROUTE (Transact-SQL). <c>DROP ROUTE name</c>.</summary>
	public sealed record DropRoute(QualifiedName Name) : Statement;

	/// <summary>T-SQL: DROP SERVICE (Transact-SQL). <c>DROP SERVICE name</c>.</summary>
	public sealed record DropService(QualifiedName Name) : Statement;

	// One name that begins with a part — no leading dot — and no IF EXISTS: DropLedName.

	/// <summary>T-SQL: DROP XML SCHEMA COLLECTION (Transact-SQL). <c>DROP XML SCHEMA COLLECTION name</c>.</summary>
	public sealed record DropXmlSchemaCollection(QualifiedName Name) : Statement;

	// A list of names as a table's, and no IF EXISTS.

	/// <summary>T-SQL: DROP STATISTICS (Transact-SQL). <c>DROP STATISTICS name [, …]</c>.</summary>
	public sealed record DropStatistics(SqlList<QualifiedName> Names) : Statement;

	// A list of names that begin with a part, and no IF EXISTS: DropLedNames.

	/// <summary>T-SQL: DROP WORKLOAD Classifier (Transact-SQL). <c>DROP WORKLOAD CLASSIFIER name [, …]</c>.</summary>
	public sealed record DropWorkloadClassifier(SqlList<QualifiedName> Names) : Statement;

	// One name of one part and nothing else: DropNamedKind.

	/// <summary>T-SQL: DROP APPLICATION ROLE (Transact-SQL). <c>DROP APPLICATION ROLE name</c>.</summary>
	public sealed record DropApplicationRole(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP AVAILABILITY GROUP (Transact-SQL). <c>DROP AVAILABILITY GROUP name</c>.</summary>
	public sealed record DropAvailabilityGroup(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP BROKER PRIORITY (Transact-SQL). <c>DROP BROKER PRIORITY name</c>.</summary>
	public sealed record DropBrokerPriority(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP CERTIFICATE (Transact-SQL). <c>DROP CERTIFICATE name</c>.</summary>
	public sealed record DropCertificate(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP COLUMN ENCRYPTION KEY (Transact-SQL). <c>DROP COLUMN ENCRYPTION KEY name</c>.</summary>
	public sealed record DropColumnEncryptionKey(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP COLUMN MASTER KEY (Transact-SQL). <c>DROP COLUMN MASTER KEY name</c>.</summary>
	public sealed record DropColumnMasterKey(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP CREDENTIAL (Transact-SQL). <c>DROP CREDENTIAL name</c>.</summary>
	public sealed record DropCredential(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP CRYPTOGRAPHIC PROVIDER (Transact-SQL). <c>DROP CRYPTOGRAPHIC PROVIDER name</c>.</summary>
	public sealed record DropCryptographicProvider(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP DATABASE AUDIT SPECIFICATION (Transact-SQL). <c>DROP DATABASE AUDIT SPECIFICATION name</c>.</summary>
	public sealed record DropDatabaseAuditSpecification(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP DATABASE SCOPED CREDENTIAL (Transact-SQL). <c>DROP DATABASE SCOPED CREDENTIAL name</c>.</summary>
	public sealed record DropDatabaseScopedCredential(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP ENDPOINT (Transact-SQL). <c>DROP ENDPOINT name</c>.</summary>
	public sealed record DropEndpoint(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP EXTERNAL DATA SOURCE (Transact-SQL). <c>DROP EXTERNAL DATA SOURCE name</c>.</summary>
	public sealed record DropExternalDataSource(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP EXTERNAL FILE FORMAT (Transact-SQL). <c>DROP EXTERNAL FILE FORMAT name</c>.</summary>
	public sealed record DropExternalFileFormat(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP EXTERNAL MODEL (Transact-SQL). <c>DROP EXTERNAL MODEL name</c>.</summary>
	public sealed record DropExternalModel(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP EXTERNAL RESOURCE POOL (Transact-SQL). <c>DROP EXTERNAL RESOURCE POOL name</c>.</summary>
	public sealed record DropExternalResourcePool(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP FULLTEXT CATALOG (Transact-SQL). <c>DROP FULLTEXT CATALOG name</c>.</summary>
	public sealed record DropFullTextCatalog(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP FULLTEXT STOPLIST (Transact-SQL). <c>DROP FULLTEXT STOPLIST name</c>.</summary>
	public sealed record DropFullTextStopList(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP LOGIN (Transact-SQL). <c>DROP LOGIN name</c>.</summary>
	public sealed record DropLogin(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP PARTITION FUNCTION (Transact-SQL). <c>DROP PARTITION FUNCTION name</c>.</summary>
	public sealed record DropPartitionFunction(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP PARTITION SCHEME (Transact-SQL). <c>DROP PARTITION SCHEME name</c>.</summary>
	public sealed record DropPartitionScheme(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP RESOURCE POOL (Transact-SQL). <c>DROP RESOURCE POOL name</c>.</summary>
	public sealed record DropResourcePool(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP SEARCH PROPERTY LIST (Transact-SQL). <c>DROP SEARCH PROPERTY LIST name</c>.</summary>
	public sealed record DropSearchPropertyList(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP SERVER AUDIT (Transact-SQL). <c>DROP SERVER AUDIT name</c>.</summary>
	public sealed record DropServerAudit(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP SERVER AUDIT SPECIFICATION (Transact-SQL). <c>DROP SERVER AUDIT SPECIFICATION name</c>.</summary>
	public sealed record DropServerAuditSpecification(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP SERVER ROLE (Transact-SQL). <c>DROP SERVER ROLE name</c>.</summary>
	public sealed record DropServerRole(Identifier Name) : Statement;

	/// <summary>T-SQL: DROP WORKLOAD GROUP (Transact-SQL). <c>DROP WORKLOAD GROUP name</c>.</summary>
	public sealed record DropWorkloadGroup(Identifier Name) : Statement;

	// A key: one name of one part, and REMOVE PROVIDER KEY: DropKeyKind.

	/// <summary>T-SQL: DROP ASYMMETRIC KEY (Transact-SQL). <c>DROP ASYMMETRIC KEY name [REMOVE PROVIDER KEY]</c>.</summary>
	public sealed record DropAsymmetricKey(Identifier Name) : Statement
	{
		/// <summary>T-SQL: DROP ASYMMETRIC KEY (Transact-SQL). <c>REMOVE PROVIDER KEY</c>: the provider's key goes with it.</summary>
		public bool RemoveProviderKey { get; init; }
	}

	/// <summary>T-SQL: DROP SYMMETRIC KEY (Transact-SQL). <c>DROP SYMMETRIC KEY name [REMOVE PROVIDER KEY]</c>.</summary>
	public sealed record DropSymmetricKey(Identifier Name) : Statement
	{
		/// <summary>T-SQL: DROP SYMMETRIC KEY (Transact-SQL). <c>REMOVE PROVIDER KEY</c>: the provider's key goes with it.</summary>
		public bool RemoveProviderKey { get; init; }
	}

	// One name of one part, and its owner: as DropExternalLibrary.

	/// <summary>T-SQL: DROP EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. <c>DROP EXTERNAL LANGUAGE name [AUTHORIZATION owner]</c>.</summary>
	public sealed record DropExternalLanguage(Identifier Name) : Statement
	{
		/// <summary>T-SQL: DROP EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. <c>AUTHORIZATION owner</c>.</summary>
		public Identifier? Authorization { get; init; }
	}

	// [IF EXISTS] and a list of names of one part, after the kinds that begin with its word.

	/// <summary>T-SQL: DROP DATABASE (Transact-SQL). <c>DROP DATABASE [IF EXISTS] name [, …]</c>.</summary>
	public sealed record DropDatabase(SqlList<Identifier> Names) : Statement
	{
		/// <summary>T-SQL: DROP DATABASE (Transact-SQL). <c>IF EXISTS</c>.</summary>
		public bool IfExists { get; init; }
	}
}
