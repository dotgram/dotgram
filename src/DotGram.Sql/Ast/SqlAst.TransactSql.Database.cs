using System;

namespace DotGram.Sql.Ast;

// T-SQL's database family: a database and its files and filegroups, its settings, its backups and restores,
// partitions and full-text catalogues.

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: ALTER DATABASE (Transact-SQL). <c>ALTER DATABASE { name | CURRENT } action</c>: one action, as the published
	/// syntax has it.
	/// </summary>
	public sealed record AlterDatabase(Identifier? Name, AlterDatabaseAction Action) : Statement
	{
		/// <summary>T-SQL: ALTER DATABASE (Transact-SQL). <c>CURRENT</c> for the name: the database in use.</summary>
		public bool Current { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER DATABASE SCOPED CONFIGURATION. <c>ALTER DATABASE SCOPED CONFIGURATION { [FOR SECONDARY] SET setting |
	/// CLEAR PROCEDURE_CACHE [plan_handle] }</c>: what is done to the database in use's configuration.
	/// </summary>
	public sealed record AlterDatabaseScopedConfiguration(ScopedConfigurationAction Action) : Statement;

	/// <summary>
	/// T-SQL: CREATE DATABASE (Transact-SQL). <c>CREATE DATABASE name [CONTAINMENT = …] [ON [PRIMARY] files [, filegroups]]
	/// [LOG ON files] [COLLATE c] [FOR ATTACH … | AS SNAPSHOT OF … | AS COPY OF …] [WITH options]</c>, or Azure SQL
	/// Database's <c>CREATE DATABASE name [COLLATE c] (edition options) [WITH options]</c>.
	/// </summary>
	public sealed record CreateDatabase(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). <c>CONTAINMENT = NONE | PARTIAL</c>; null where none is said.</summary>
		public DatabaseContainment? Containment { get; init; }

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). <c>PRIMARY</c> written after <c>ON</c>, before the first file.</summary>
		[Spelling] public bool Primary { get; init; }

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). The files after <c>ON</c> in no filegroup of their own: the primary's.</summary>
		public SqlList<DatabaseFile> Files { get; init; } = [];

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). The filegroups after the primary's files, in the order written.</summary>
		public SqlList<FileGroup> FileGroups { get; init; } = [];

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). The files after <c>LOG ON</c>; empty where it is not written.</summary>
		public SqlList<DatabaseFile> LogFiles { get; init; } = [];

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). <c>COLLATE collation_name</c>.</summary>
		public CollationName? Collation { get; init; }

		/// <summary>
		/// T-SQL: CREATE DATABASE (Transact-SQL), the Azure syntax. The edition options in brackets: after the name and
		/// collation of a database created, after the source of one copied.
		/// </summary>
		public SqlList<Option> EditionOptions { get; init; } = [];

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). What the database is made from; null for a new one.</summary>
		public DatabaseSource? Source { get; init; }

		/// <summary>
		/// T-SQL: CREATE DATABASE (Transact-SQL). <c>WITH option, …</c>; a copy's is <c>WITH (BACKUP_STORAGE_REDUNDANCY =
		/// …)</c>, in brackets.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}
}

/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). Whether a database is contained.</summary>
/// <remarks><see cref="None"/> is the word <c>NONE</c>; a database that says nothing has a null containment.</remarks>
public enum DatabaseContainment { None, Partial }

/// <summary>
/// T-SQL: CREATE DATABASE (Transact-SQL). <c>FILEGROUP name [CONTAINS …] [DEFAULT] file, …</c>: a filegroup and its files,
/// one at least.
/// </summary>
public sealed record FileGroup(Identifier Name, SqlList<DatabaseFile> Files) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). <c>CONTAINS FILESTREAM</c> or <c>CONTAINS MEMORY_OPTIMIZED_DATA</c>.</summary>
	public FileGroupContents? Contains { get; init; }

	/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). <c>DEFAULT</c>: the filegroup new objects go to.</summary>
	public bool Default { get; init; }
}

/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). What a database is made from instead of nothing.</summary>
public abstract record DatabaseSource : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: CREATE DATABASE (Transact-SQL). <c>FOR ATTACH</c>, <c>FOR ATTACH_REBUILD_LOG</c> or
	/// <c>FOR ATTACH_FORCE_REBUILD_LOG</c>: files that already exist.
	/// </summary>
	public sealed record ForAttach(AttachKind Kind) : DatabaseSource;

	/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). <c>AS SNAPSHOT OF database</c>.</summary>
	public sealed record AsSnapshotOf(Identifier Database) : DatabaseSource;

	/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). <c>AS COPY OF [server.]database</c>.</summary>
	public sealed record AsCopyOf(QualifiedName Database) : DatabaseSource;
}

/// <summary>
/// T-SQL: CREATE DATABASE (Transact-SQL). Which attach: <c>ATTACH</c>, <c>ATTACH_REBUILD_LOG</c>, or
/// <c>ATTACH_FORCE_REBUILD_LOG</c>, which the grammar reads and the page does not name.
/// </summary>
public enum AttachKind { Attach, AttachRebuildLog, AttachForceRebuildLog }

/// <summary>
/// T-SQL: ALTER DATABASE SET Options (Transact-SQL). <c>termination</c>: what is done with the sessions that stand in the way
/// of a setting.
/// </summary>
public abstract record Termination : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: ALTER DATABASE SET Options (Transact-SQL). <c>ROLLBACK AFTER number [SECONDS]</c>.</summary>
	public sealed record RollbackAfter(LiteralValue.Numeric Number) : Termination
	{
		/// <summary>T-SQL: ALTER DATABASE SET Options (Transact-SQL). <c>SECONDS</c> written after the number.</summary>
		[Spelling] public bool Seconds { get; init; }
	}

	/// <summary>T-SQL: ALTER DATABASE SET Options (Transact-SQL). <c>ROLLBACK IMMEDIATE</c>.</summary>
	public sealed record RollbackImmediate : Termination;

	/// <summary>T-SQL: ALTER DATABASE SET Options (Transact-SQL). <c>NO_WAIT</c>.</summary>
	public sealed record NoWait : Termination;
}

/// <summary>T-SQL: ALTER DATABASE SCOPED CONFIGURATION. What an <c>ALTER DATABASE SCOPED CONFIGURATION</c> does.</summary>
public abstract record ScopedConfigurationAction : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: ALTER DATABASE SCOPED CONFIGURATION. <c>SET setting</c>: one setting, as the published block and the engine
	/// have it.
	/// </summary>
	public sealed record Set(Option Option) : ScopedConfigurationAction
	{
		/// <summary>T-SQL: ALTER DATABASE SCOPED CONFIGURATION. <c>FOR SECONDARY</c>: set for the secondary replicas.</summary>
		public bool Secondary { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER DATABASE SCOPED CONFIGURATION. <c>CLEAR PROCEDURE_CACHE [plan_handle]</c>: the plan handle, a binary
	/// constant; null for the whole cache.
	/// </summary>
	public sealed record ClearProcedureCache(LiteralValue.Binary? PlanHandle) : ScopedConfigurationAction;
}
