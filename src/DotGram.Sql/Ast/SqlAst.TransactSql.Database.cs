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
}
