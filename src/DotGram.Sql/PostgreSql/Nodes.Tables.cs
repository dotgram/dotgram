using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Sql.PostgreSql;

// Inside the namespace, where they are asked before DotGram.Sql's own Expression, Query and Statement.
using Expression = DotGram.Sql.Ast.Expression;

// The factories of the table statements: CREATE TABLE, its columns, constraints and partitions, and the key an index,
// an exclusion constraint, a partition key and ON CONFLICT share.
static partial class Nodes
{
	// ── Keys ───────────────────────────────────────────────────────────────────

	/// <summary>One key of an index_elem: the key, its collation, its operator class and that class's options, its order.</summary>
	public static IndexElement Indexed(Expression key, QualifiedName? collation, QualifiedName? operatorClass, Option? first, Option[]? rest, string? direction, string? nulls)
	{
		return new IndexElement(
			key,
			collation is { } named ? new CollationName(named) : null,
			operatorClass,
			List(first, rest),
			direction is null ? null : (direction[0] | 0x20) == 'a' ? SortDirection.Asc : SortDirection.Desc,
			nulls is null ? null : (nulls[0] | 0x20) == 'f' ? NullOrdering.First : NullOrdering.Last);
	}
}
