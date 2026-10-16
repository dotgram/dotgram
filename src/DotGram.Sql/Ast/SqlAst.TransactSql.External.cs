using System;

namespace DotGram.Sql.Ast;

// T-SQL's external family: the data sources, file formats, tables, libraries, languages, models and resource
// pools that live outside the database. A setting that is a name and a value is an Option (proposal 20); a
// place that is one of several things is a small family named after the grammar's words.

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: CREATE EXTERNAL DATA SOURCE (Transact-SQL). <c>CREATE EXTERNAL DATA SOURCE name WITH (setting, …)</c>: the
	/// settings the grammar's <c>DataSourceBody</c> reads — LOCATION, CONNECTION_OPTIONS, CREDENTIAL, PUSHDOWN, TYPE,
	/// RESOURCE_MANAGER_LOCATION, DATABASE_NAME and SHARD_MAP_NAME — each once, none or more; the brackets are always written.
	/// </summary>
	public sealed record CreateExternalDataSource(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: ALTER EXTERNAL DATA SOURCE (Transact-SQL). <c>ALTER EXTERNAL DATA SOURCE name SET setting, …</c>: the settings
	/// with no brackets, one at least, each once.
	/// </summary>
	public sealed record AlterExternalDataSource(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: CREATE EXTERNAL FILE FORMAT (Transact-SQL). <c>CREATE EXTERNAL FILE FORMAT name WITH (setting, …)</c>:
	/// FORMAT_TYPE, DATA_COMPRESSION, SERDE_METHOD and <c>FORMAT_OPTIONS (option, …)</c>, in any order and as often as wanted;
	/// the brackets are always written.
	/// </summary>
	public sealed record CreateExternalFileFormat(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: CREATE EXTERNAL TABLE (Transact-SQL), CREATE EXTERNAL TABLE AS SELECT (CETAS) (Transact-SQL). <c>CREATE EXTERNAL TABLE
	/// name ( column, … ) WITH (setting, …)</c>, or <c>CREATE EXTERNAL TABLE name WITH (setting, …) AS query</c> where the query
	/// says the columns: <see cref="Contents"/> is <see cref="TableContents.Elements"/> or <see cref="TableContents.AsQuery"/>, the
	/// latter with no columns of its own and no <c>WITH [NO] DATA</c>.
	/// </summary>
	public sealed record CreateExternalTable(QualifiedName Name, TableContents Contents) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE EXTERNAL TABLE (Transact-SQL). <c>WITH (setting, …)</c>: LOCATION, REJECTED_ROW_LOCATION, DATA_SOURCE,
		/// FILE_FORMAT, REJECT_TYPE, REJECT_VALUE, REJECT_SAMPLE_VALUE, SCHEMA_NAME, OBJECT_NAME, TABLE_OPTIONS and DISTRIBUTION,
		/// each once; the brackets are always written.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE EXTERNAL LIBRARY (Transact-SQL) - SQL Server. <c>CREATE EXTERNAL LIBRARY name [AUTHORIZATION owner] FROM
	/// (file), … WITH (LANGUAGE = 'language')</c>: one file at least, each of which names its content, and the language as a string.
	/// </summary>
	public sealed record CreateExternalLibrary(Identifier Name, SqlList<ExternalFile> Files, LiteralValue.String Language) : Statement
	{
		/// <summary>T-SQL: CREATE EXTERNAL LIBRARY (Transact-SQL) - SQL Server. <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER EXTERNAL LIBRARY (Transact-SQL). <c>ALTER EXTERNAL LIBRARY name [AUTHORIZATION owner] { SET | ADD } (file), …
	/// WITH (LANGUAGE = 'language')</c>: what is done to the files, and the language as a string.
	/// </summary>
	public sealed record AlterExternalLibrary(Identifier Name, ExternalLibraryChange Change, LiteralValue.String Language) : Statement
	{
		/// <summary>T-SQL: ALTER EXTERNAL LIBRARY (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. <c>CREATE EXTERNAL LANGUAGE name [AUTHORIZATION owner] FROM
	/// (file) [, (file)]</c>: one file, or two for two platforms.
	/// </summary>
	public sealed record CreateExternalLanguage(Identifier Name, SqlList<ExternalFile> Files) : Statement
	{
		/// <summary>T-SQL: CREATE EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. <c>ALTER EXTERNAL LANGUAGE name [AUTHORIZATION owner] { SET
	/// (file) | ADD (file) | REMOVE PLATFORM platform }</c>.
	/// </summary>
	public sealed record AlterExternalLanguage(Identifier Name, ExternalLanguageChange Change) : Statement
	{
		/// <summary>T-SQL: ALTER EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE EXTERNAL MODEL (Transact-SQL). <c>CREATE EXTERNAL MODEL name [AUTHORIZATION owner] WITH (setting, …)</c>:
	/// MODEL_TYPE, CREDENTIAL, LOCATION, API_FORMAT, MODEL, PARAMETERS and LOCAL_RUNTIME_PATH, each once, none or more; the
	/// brackets are always written.
	/// </summary>
	public sealed record CreateExternalModel(Identifier Name, SqlList<Option> Options) : Statement
	{
		/// <summary>T-SQL: CREATE EXTERNAL MODEL (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER EXTERNAL MODEL (Transact-SQL). <c>ALTER EXTERNAL MODEL name SET (setting, …)</c>: the settings of the creating
	/// statement, each once, none or more; the brackets are always written, and an owner is not read.
	/// </summary>
	public sealed record AlterExternalModel(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: CREATE EXTERNAL RESOURCE POOL (Transact-SQL). <c>CREATE EXTERNAL RESOURCE POOL name [WITH (setting, …)]</c>.
	/// </summary>
	public sealed record CreateExternalResourcePool(Identifier Name) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE EXTERNAL RESOURCE POOL (Transact-SQL). <c>WITH (setting, …)</c>: MAX_CPU_PERCENT, MAX_MEMORY_PERCENT,
		/// MAX_PROCESSES, <c>AFFINITY CPU</c> and <c>AFFINITY NUMANODE</c>, in any order and as often as wanted; none where the
		/// statement writes no <c>WITH</c>.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER EXTERNAL RESOURCE POOL (Transact-SQL). <c>ALTER EXTERNAL RESOURCE POOL name WITH (setting, …)</c>: the settings
	/// of the creating statement, one at least.
	/// </summary>
	public sealed record AlterExternalResourcePool(Identifier Name, SqlList<Option> Options) : Statement;
}

public abstract partial record Expression
{
	/// <summary>
	/// T-SQL: CREATE EXTERNAL RESOURCE POOL (Transact-SQL). The ranges an <c>AFFINITY CPU</c> or <c>AFFINITY NUMANODE</c> is set
	/// to, <c>(0 TO 3, 5)</c> (the grammar's <c>PoolRanges</c>): the value of that option and of no other.
	/// </summary>
	public sealed record PoolRanges(SqlList<PartitionRange> Ranges) : Expression;
}

/// <summary>
/// T-SQL: CREATE EXTERNAL LIBRARY (Transact-SQL) - SQL Server, CREATE EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. One file of a
/// library or a language, <c>(CONTENT = 0x…, PLATFORM = LINUX)</c>: the settings in brackets, in any order and as often as the
/// grammar's <c>LibraryFileOption</c> and <c>LanguageOption</c> read them.
/// </summary>
public sealed record ExternalFile(SqlList<Option> Options) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: ALTER EXTERNAL LIBRARY (Transact-SQL), ALTER EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. A platform: <c>WINDOWS</c> or <c>LINUX</c>.</summary>
public enum ExternalPlatform { Windows, Linux }

/// <summary>T-SQL: ALTER EXTERNAL LIBRARY (Transact-SQL). What is done to the files of a library.</summary>
public abstract record ExternalLibraryChange : ISqlNode
{
	/// <summary>Where in the text this change was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this change was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>SET (file), …</c>: the files the library has, one at least.</summary>
	public sealed record Set(SqlList<ExternalFile> Files) : ExternalLibraryChange;

	/// <summary><c>ADD (file), …</c>: files added to the library, one at least.</summary>
	public sealed record Add(SqlList<ExternalFile> Files) : ExternalLibraryChange;
}

/// <summary>T-SQL: ALTER EXTERNAL LANGUAGE (Transact-SQL) - SQL Server. What is done to the files of a language.</summary>
public abstract record ExternalLanguageChange : ISqlNode
{
	/// <summary>Where in the text this change was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this change was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>SET (file)</c>.</summary>
	public sealed record Set(ExternalFile File) : ExternalLanguageChange;

	/// <summary><c>ADD (file)</c>.</summary>
	public sealed record Add(ExternalFile File) : ExternalLanguageChange;

	/// <summary><c>REMOVE PLATFORM platform</c>.</summary>
	public sealed record RemovePlatform(ExternalPlatform Platform) : ExternalLanguageChange;
}
