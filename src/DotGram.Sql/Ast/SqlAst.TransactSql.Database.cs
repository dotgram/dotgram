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

	/// <summary>
	/// T-SQL: BACKUP (Transact-SQL). <c>BACKUP DATABASE { name | @v } [files] TO device, … [MIRROR TO device, …]… [WITH
	/// option, …]</c>.
	/// </summary>
	public sealed record BackupDatabase(Expression Database, SqlList<BackupDevice> To) : Statement
	{
		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>file_or_filegroup</c>, <c>READ_WRITE_FILEGROUPS</c>: what of the database.</summary>
		public SqlList<BackupFile> Files { get; init; } = [];

		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>MIRROR TO device, …</c>, each in the order written.</summary>
		public SqlList<BackupMirror> Mirrors { get; init; } = [];

		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: BACKUP (Transact-SQL). <c>BACKUP LOG { name | @v } TO device, … [MIRROR TO …]… [WITH option, …]</c>.</summary>
	public sealed record BackupLog(Expression Database, SqlList<BackupDevice> To) : Statement
	{
		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>MIRROR TO device, …</c>, each in the order written.</summary>
		public SqlList<BackupMirror> Mirrors { get; init; } = [];

		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: BACKUP (Transact-SQL). <c>BACKUP SERVER TO device, … [MIRROR TO …]… [WITH option, …]</c>.</summary>
	public sealed record BackupServer(SqlList<BackupDevice> To) : Statement
	{
		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>MIRROR TO device, …</c>, each in the order written.</summary>
		public SqlList<BackupMirror> Mirrors { get; init; } = [];

		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: BACKUP (Transact-SQL). <c>BACKUP GROUP database, … TO device, … [MIRROR TO …]… [WITH option, …]</c>: the
	/// databases, one at least.
	/// </summary>
	public sealed record BackupGroup(SqlList<Expression> Databases, SqlList<BackupDevice> To) : Statement
	{
		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>MIRROR TO device, …</c>, each in the order written.</summary>
		public SqlList<BackupMirror> Mirrors { get; init; } = [];

		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: BACKUP CERTIFICATE (Transact-SQL). <c>BACKUP CERTIFICATE c TO FILE = 'path' [WITH [FORMAT = 'PFX',] PRIVATE
	/// KEY (…)]</c>.
	/// </summary>
	public sealed record BackupCertificate(Identifier Name, Expression File) : Statement
	{
		/// <summary>T-SQL: BACKUP CERTIFICATE (Transact-SQL). <c>FORMAT = 'PFX'</c>, before the private key.</summary>
		public Expression? Format { get; init; }

		/// <summary>
		/// T-SQL: BACKUP CERTIFICATE (Transact-SQL). <c>PRIVATE KEY (FILE = …, ENCRYPTION BY PASSWORD = …, …)</c>: its
		/// settings; empty where no private key is written.
		/// </summary>
		public SqlList<Option> PrivateKey { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: BACKUP MASTER KEY (Transact-SQL). <c>BACKUP MASTER KEY TO { FILE | URL } = 'path' ENCRYPTION BY PASSWORD =
	/// 'password'</c>.
	/// </summary>
	public sealed record BackupMasterKey(KeyPlace To, Expression EncryptionPassword) : Statement;

	/// <summary>
	/// T-SQL: BACKUP SERVICE MASTER KEY (Transact-SQL). <c>BACKUP SERVICE MASTER KEY TO FILE = 'path' ENCRYPTION BY
	/// PASSWORD = 'password'</c>.
	/// </summary>
	public sealed record BackupServiceMasterKey(Expression File, Expression EncryptionPassword) : Statement;

	/// <summary>
	/// T-SQL: BACKUP SYMMETRIC KEY (Transact-SQL). <c>BACKUP SYMMETRIC KEY k TO { FILE | URL } = 'path' ENCRYPTION BY
	/// PASSWORD = 'password'</c>.
	/// </summary>
	public sealed record BackupSymmetricKey(Identifier Name, KeyPlace To, Expression EncryptionPassword) : Statement;

	/// <summary>
	/// T-SQL: RESTORE (Transact-SQL). <c>RESTORE DATABASE { name | @v } [files] [FROM { device, … | DATABASE_SNAPSHOT = s }]
	/// [WITH option, …]</c>.
	/// </summary>
	public sealed record RestoreDatabase(Expression Database) : Statement
	{
		/// <summary>T-SQL: RESTORE (Transact-SQL). <c>files_or_filegroups</c>, <c>PAGE = …</c>: what of the database.</summary>
		public SqlList<BackupFile> Files { get; init; } = [];

		/// <summary>T-SQL: RESTORE (Transact-SQL). <c>FROM device, …</c>.</summary>
		public SqlList<BackupDevice> From { get; init; } = [];

		/// <summary>T-SQL: RESTORE (Transact-SQL). <c>FROM DATABASE_SNAPSHOT = name</c>: a string or a variable.</summary>
		public Expression? DatabaseSnapshot { get; init; }

		/// <summary>T-SQL: RESTORE (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: RESTORE (Transact-SQL). <c>RESTORE LOG { name | @v } [files] [FROM device, …] [WITH option, …]</c>.</summary>
	public sealed record RestoreLog(Expression Database) : Statement
	{
		/// <summary>T-SQL: RESTORE (Transact-SQL). <c>file_or_filegroup_or_pages</c>.</summary>
		public SqlList<BackupFile> Files { get; init; } = [];

		/// <summary>T-SQL: RESTORE (Transact-SQL). <c>FROM device, …</c>.</summary>
		public SqlList<BackupDevice> From { get; init; } = [];

		/// <summary>T-SQL: RESTORE (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: RESTORE FILELISTONLY (Transact-SQL). <c>RESTORE FILELISTONLY FROM device, … [WITH option, …]</c>.</summary>
	public sealed record RestoreFileListOnly(SqlList<BackupDevice> From) : Statement
	{
		/// <summary>T-SQL: RESTORE FILELISTONLY (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: RESTORE HEADERONLY (Transact-SQL). <c>RESTORE HEADERONLY FROM device, … [WITH option, …]</c>.</summary>
	public sealed record RestoreHeaderOnly(SqlList<BackupDevice> From) : Statement
	{
		/// <summary>T-SQL: RESTORE HEADERONLY (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: RESTORE LABELONLY (Transact-SQL). <c>RESTORE LABELONLY FROM device, … [WITH option, …]</c>.</summary>
	public sealed record RestoreLabelOnly(SqlList<BackupDevice> From) : Statement
	{
		/// <summary>T-SQL: RESTORE LABELONLY (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: RESTORE REWINDONLY (Transact-SQL). <c>RESTORE REWINDONLY FROM device, … [WITH option, …]</c>.</summary>
	public sealed record RestoreRewindOnly(SqlList<BackupDevice> From) : Statement
	{
		/// <summary>T-SQL: RESTORE REWINDONLY (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: RESTORE VERIFYONLY (Transact-SQL). <c>RESTORE VERIFYONLY FROM device, … [WITH option, …]</c>.</summary>
	public sealed record RestoreVerifyOnly(SqlList<BackupDevice> From) : Statement
	{
		/// <summary>T-SQL: RESTORE VERIFYONLY (Transact-SQL). <c>WITH option, …</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: RESTORE MASTER KEY (Transact-SQL). <c>RESTORE MASTER KEY FROM { FILE | URL } = 'path' DECRYPTION BY PASSWORD =
	/// 'p' ENCRYPTION BY PASSWORD = 'p' [FORCE]</c>.
	/// </summary>
	public sealed record RestoreMasterKey(KeyPlace From, Expression DecryptionPassword, Expression EncryptionPassword) : Statement
	{
		/// <summary>T-SQL: RESTORE MASTER KEY (Transact-SQL). <c>FORCE</c>.</summary>
		public bool Force { get; init; }
	}

	/// <summary>
	/// T-SQL: RESTORE SERVICE MASTER KEY (Transact-SQL). <c>RESTORE SERVICE MASTER KEY FROM FILE = 'path' DECRYPTION BY
	/// PASSWORD = 'p' [FORCE]</c>.
	/// </summary>
	public sealed record RestoreServiceMasterKey(Expression File, Expression DecryptionPassword) : Statement
	{
		/// <summary>T-SQL: RESTORE SERVICE MASTER KEY (Transact-SQL). <c>FORCE</c>.</summary>
		public bool Force { get; init; }
	}

	/// <summary>
	/// T-SQL: RESTORE SYMMETRIC KEY (Transact-SQL). <c>RESTORE SYMMETRIC KEY k FROM { FILE | URL } = 'path' DECRYPTION BY
	/// PASSWORD = 'p' ENCRYPTION BY PASSWORD = 'p' [FORCE]</c>.
	/// </summary>
	public sealed record RestoreSymmetricKey(Identifier Name, KeyPlace From, Expression DecryptionPassword, Expression EncryptionPassword) : Statement
	{
		/// <summary>T-SQL: RESTORE SYMMETRIC KEY (Transact-SQL). <c>FORCE</c>.</summary>
		public bool Force { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE PARTITION FUNCTION (Transact-SQL). <c>CREATE PARTITION FUNCTION f (type [COLLATE c]) AS RANGE [LEFT |
	/// RIGHT] FOR VALUES (boundary, …)</c>: the boundaries, which may be none.
	/// </summary>
	public sealed record CreatePartitionFunction(Identifier Name, DataType Type, SqlList<Expression> Boundaries) : Statement
	{
		/// <summary>T-SQL: CREATE PARTITION FUNCTION (Transact-SQL). <c>COLLATE c</c> after the type.</summary>
		public CollationName? Collation { get; init; }

		/// <summary>
		/// T-SQL: CREATE PARTITION FUNCTION (Transact-SQL). <c>LEFT</c> or <c>RIGHT</c>: which side of a boundary its value
		/// belongs to; null where neither is said.
		/// </summary>
		public RangeSide? Range { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER PARTITION FUNCTION (Transact-SQL). <c>ALTER PARTITION FUNCTION f () { SPLIT | MERGE } [RANGE
	/// (boundary)]</c>.
	/// </summary>
	public sealed record AlterPartitionFunction(Identifier Name, PartitionFunctionAction Action) : Statement;

	/// <summary>
	/// T-SQL: CREATE PARTITION SCHEME (Transact-SQL). <c>CREATE PARTITION SCHEME s AS PARTITION f [ALL] TO (filegroup,
	/// …)</c>: the filegroups, one at least, each a name or a string.
	/// </summary>
	public sealed record CreatePartitionScheme(Identifier Name, Identifier Function, SqlList<Expression> FileGroups) : Statement
	{
		/// <summary>T-SQL: CREATE PARTITION SCHEME (Transact-SQL). <c>ALL</c>: every partition to the one filegroup.</summary>
		public bool All { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER PARTITION SCHEME (Transact-SQL). <c>ALTER PARTITION SCHEME s NEXT USED [filegroup]</c>: the filegroup, a
	/// name or a string; null where none is named.
	/// </summary>
	public sealed record AlterPartitionScheme(Identifier Name, Expression? NextUsed) : Statement;

	/// <summary>
	/// T-SQL: CREATE FULLTEXT CATALOG (Transact-SQL). <c>CREATE FULLTEXT CATALOG c [ON FILEGROUP fg] [IN PATH 'p'] [WITH
	/// ACCENT_SENSITIVITY = …] [AS DEFAULT] [AUTHORIZATION owner]</c>.
	/// </summary>
	public sealed record CreateFullTextCatalog(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE FULLTEXT CATALOG (Transact-SQL). <c>ON FILEGROUP fg</c>.</summary>
		public Identifier? FileGroup { get; init; }

		/// <summary>T-SQL: CREATE FULLTEXT CATALOG (Transact-SQL). <c>IN PATH 'rootpath'</c>.</summary>
		public Expression? Path { get; init; }

		/// <summary>T-SQL: CREATE FULLTEXT CATALOG (Transact-SQL). <c>WITH ACCENT_SENSITIVITY = ON | OFF</c>; null where unsaid.</summary>
		public bool? AccentSensitivity { get; init; }

		/// <summary>T-SQL: CREATE FULLTEXT CATALOG (Transact-SQL). <c>AS DEFAULT</c>.</summary>
		public bool Default { get; init; }

		/// <summary>T-SQL: CREATE FULLTEXT CATALOG (Transact-SQL). <c>AUTHORIZATION owner</c>.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER FULLTEXT CATALOG (Transact-SQL). <c>ALTER FULLTEXT CATALOG c { REBUILD [WITH ACCENT_SENSITIVITY = …] |
	/// REORGANIZE | AS DEFAULT }</c>.
	/// </summary>
	public sealed record AlterFullTextCatalog(Identifier Name, FullTextCatalogAction Action) : Statement;

	/// <summary>
	/// T-SQL: CREATE FULLTEXT STOPLIST (Transact-SQL). <c>CREATE FULLTEXT STOPLIST s [FROM { [db.]source | SYSTEM STOPLIST
	/// }] [AUTHORIZATION owner]</c>.
	/// </summary>
	public sealed record CreateFullTextStopList(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE FULLTEXT STOPLIST (Transact-SQL). What the stoplist is copied from; null for an empty one.</summary>
		public StopListSource? From { get; init; }

		/// <summary>T-SQL: CREATE FULLTEXT STOPLIST (Transact-SQL). <c>AUTHORIZATION owner</c>.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER FULLTEXT STOPLIST (Transact-SQL). <c>ALTER FULLTEXT STOPLIST s { ADD 'word' LANGUAGE l | DROP { 'word'
	/// LANGUAGE l | ALL [LANGUAGE l] } }</c>.
	/// </summary>
	public sealed record AlterFullTextStopList(Identifier Name, StopListAction Action) : Statement;

	/// <summary>
	/// T-SQL: CREATE SEARCH PROPERTY LIST (Transact-SQL). <c>CREATE SEARCH PROPERTY LIST l [FROM [db.]source]
	/// [AUTHORIZATION owner]</c>.
	/// </summary>
	public sealed record CreateSearchPropertyList(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE SEARCH PROPERTY LIST (Transact-SQL). <c>FROM [database_name.]source_list_name</c>.</summary>
		public QualifiedName? From { get; init; }

		/// <summary>T-SQL: CREATE SEARCH PROPERTY LIST (Transact-SQL). <c>AUTHORIZATION owner</c>.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER SEARCH PROPERTY LIST (Transact-SQL). <c>ALTER SEARCH PROPERTY LIST l { ADD 'property' WITH (…) | DROP
	/// 'property' }</c>.
	/// </summary>
	public sealed record AlterSearchPropertyList(Identifier Name, SearchPropertyListAction Action) : Statement;

	/// <summary>T-SQL: CHECKPOINT (Transact-SQL). <c>CHECKPOINT [checkpoint_duration]</c>.</summary>
	public sealed record Checkpoint(Expression? Duration) : Statement;
}

/// <summary>T-SQL: CREATE PARTITION FUNCTION (Transact-SQL). Which side of a boundary its value belongs to.</summary>
public enum RangeSide { Left, Right }

/// <summary>T-SQL: ALTER PARTITION FUNCTION (Transact-SQL). What an <c>ALTER PARTITION FUNCTION</c> does.</summary>
public abstract record PartitionFunctionAction : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: ALTER PARTITION FUNCTION (Transact-SQL). <c>SPLIT [RANGE (boundary)]</c>; the boundary null where
	/// <c>RANGE</c> is left out, which the grammar reads.
	/// </summary>
	public sealed record Split(Expression? Boundary) : PartitionFunctionAction;

	/// <summary>T-SQL: ALTER PARTITION FUNCTION (Transact-SQL). <c>MERGE [RANGE (boundary)]</c>, likewise.</summary>
	public sealed record Merge(Expression? Boundary) : PartitionFunctionAction;
}

/// <summary>T-SQL: ALTER FULLTEXT CATALOG (Transact-SQL). What an <c>ALTER FULLTEXT CATALOG</c> does.</summary>
public abstract record FullTextCatalogAction : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: ALTER FULLTEXT CATALOG (Transact-SQL). <c>REBUILD [WITH ACCENT_SENSITIVITY = ON | OFF]</c>; null where the
	/// sensitivity is unsaid.
	/// </summary>
	public sealed record Rebuild(bool? AccentSensitivity) : FullTextCatalogAction;

	/// <summary>T-SQL: ALTER FULLTEXT CATALOG (Transact-SQL). <c>REORGANIZE</c>.</summary>
	public sealed record Reorganize : FullTextCatalogAction;

	/// <summary>T-SQL: ALTER FULLTEXT CATALOG (Transact-SQL). <c>AS DEFAULT</c>.</summary>
	public sealed record AsDefault : FullTextCatalogAction;
}

/// <summary>T-SQL: CREATE FULLTEXT STOPLIST (Transact-SQL). What a stoplist is copied from.</summary>
public abstract record StopListSource : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: CREATE FULLTEXT STOPLIST (Transact-SQL). <c>FROM SYSTEM STOPLIST</c>.</summary>
	public sealed record SystemStopList : StopListSource;

	/// <summary>T-SQL: CREATE FULLTEXT STOPLIST (Transact-SQL). <c>FROM [database_name.]source_stoplist_name</c>.</summary>
	public sealed record Named(QualifiedName Name) : StopListSource;
}

/// <summary>T-SQL: ALTER FULLTEXT STOPLIST (Transact-SQL). What an <c>ALTER FULLTEXT STOPLIST</c> does.</summary>
public abstract record StopListAction : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: ALTER FULLTEXT STOPLIST (Transact-SQL). <c>ADD 'stopword' LANGUAGE language_term</c>.</summary>
	public sealed record Add(Expression Word, Expression Language) : StopListAction;

	/// <summary>T-SQL: ALTER FULLTEXT STOPLIST (Transact-SQL). <c>DROP 'stopword' LANGUAGE language_term</c>.</summary>
	public sealed record Drop(Expression Word, Expression Language) : StopListAction;

	/// <summary>
	/// T-SQL: ALTER FULLTEXT STOPLIST (Transact-SQL). <c>DROP ALL [LANGUAGE language_term]</c>; the language null for every
	/// one.
	/// </summary>
	public sealed record DropAll(Expression? Language) : StopListAction;
}

/// <summary>T-SQL: ALTER SEARCH PROPERTY LIST (Transact-SQL). What an <c>ALTER SEARCH PROPERTY LIST</c> does.</summary>
public abstract record SearchPropertyListAction : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: ALTER SEARCH PROPERTY LIST (Transact-SQL). <c>ADD 'property_name' WITH (PROPERTY_SET_GUID = …, PROPERTY_INT_ID
	/// = … [, PROPERTY_DESCRIPTION = …])</c>: the settings, in that order.
	/// </summary>
	public sealed record Add(Expression Property, SqlList<Option> Options) : SearchPropertyListAction;

	/// <summary>T-SQL: ALTER SEARCH PROPERTY LIST (Transact-SQL). <c>DROP 'property_name'</c>.</summary>
	public sealed record Drop(Expression Property) : SearchPropertyListAction;
}

/// <summary>
/// T-SQL: BACKUP (Transact-SQL). <c>backup_device</c>: a device by its logical name or a variable that holds it, or a
/// physical one.
/// </summary>
public abstract record BackupDevice : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: BACKUP (Transact-SQL). <c>{ logical_device_name | @logical_device_name_var }</c>: exactly one of the two.
	/// </summary>
	public sealed record Logical(QualifiedName? Name, Expression.Variable? Variable) : BackupDevice;

	/// <summary>T-SQL: BACKUP (Transact-SQL). <c>{ DISK | TAPE | URL } = { 'physical_device_name' | @var }</c>.</summary>
	public sealed record Physical(DeviceKind Kind, Expression Value) : BackupDevice;
}

/// <summary>T-SQL: BACKUP (Transact-SQL). What a physical device is.</summary>
public enum DeviceKind { Disk, Tape, Url }

/// <summary>T-SQL: BACKUP (Transact-SQL). <c>MIRROR TO device, …</c>: the devices, one at least.</summary>
public sealed record BackupMirror(SqlList<BackupDevice> Devices) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: BACKUP (Transact-SQL), RESTORE (Transact-SQL). What part of a database is backed up or restored.</summary>
public abstract record BackupFile : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: BACKUP (Transact-SQL). <c>FILE = { logical_file_name | @var }</c>.</summary>
	public sealed record File(Expression Name) : BackupFile;

	/// <summary>T-SQL: BACKUP (Transact-SQL). <c>FILEGROUP = { logical_filegroup_name | @var }</c>.</summary>
	public sealed record FileGroup(Expression Name) : BackupFile;

	/// <summary>T-SQL: BACKUP (Transact-SQL). <c>READ_WRITE_FILEGROUPS</c>.</summary>
	public sealed record ReadWriteFileGroups : BackupFile;

	/// <summary>T-SQL: RESTORE (Transact-SQL). <c>PAGE = 'file:page, …'</c>.</summary>
	public sealed record Page(Expression Value) : BackupFile;
}

/// <summary>
/// T-SQL: BACKUP MASTER KEY (Transact-SQL), BACKUP SYMMETRIC KEY (Transact-SQL), RESTORE MASTER KEY (Transact-SQL),
/// RESTORE SYMMETRIC KEY (Transact-SQL). <c>{ FILE | URL } = 'path'</c>: where a key goes or comes from.
/// </summary>
public sealed record KeyPlace(KeyPlaceKind Kind, Expression Path) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: BACKUP MASTER KEY (Transact-SQL). Whether a key's place is a file or a URL.</summary>
public enum KeyPlaceKind { File, Url }

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
