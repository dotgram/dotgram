using System;

namespace DotGram.Sql.Ast;

/// <summary>T-SQL's text of the common tree.</summary>
/// <remarks>
/// The same text <see cref="Sql2023Writer.Write(ISqlNode)"/> prints, and in addition a refusal of the trees T-SQL would read as
/// another tree in three ways: a reserved word written plain where a name stands, a value where a condition stands and
/// a condition where a value stands, and a signed number where T-SQL reads an expression and the reverse. It promises no
/// more than that: a tree it writes is one the T-SQL grammar's own checks, beyond those three, may still refuse.
/// </remarks>
public static class TransactSqlWriter
{
	/// <summary>The tree's text, as T-SQL reads it.</summary>
	public static string Write(ISqlNode node)
	{
		return Sql2023Writer.Write(node, transactSql: true);
	}
}
