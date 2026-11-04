using System;

namespace DotGram.Sql.Ast;

/// <summary>PostgreSQL's text of the common tree.</summary>
/// <remarks>
/// The same text <see cref="SqlWriter.Write(ISqlNode)"/> prints, with PostgreSQL's own spellings where the tree keeps
/// them — <c>x::t</c>, a typed literal, <c>ILIKE</c>, <c>ISNULL</c>, <c>t[]</c>, <c>$1</c> — and a refusal of the trees
/// PostgreSQL would read as others or not at all: T-SQL's operators and nodes, a word that only another dialect reads.
/// It promises no more than that: a tree it writes is one PostgreSQL's grammar's own checks may still refuse.
/// </remarks>
public static class PostgreSqlWriter
{
	/// <summary>The tree's text, as PostgreSQL reads it.</summary>
	public static string Write(ISqlNode node)
	{
		return SqlWriter.Write(node, SqlDialect.PostgreSql);
	}
}
