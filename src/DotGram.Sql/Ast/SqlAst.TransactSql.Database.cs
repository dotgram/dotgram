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
}

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
