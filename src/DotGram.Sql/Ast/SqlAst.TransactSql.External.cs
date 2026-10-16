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
}
