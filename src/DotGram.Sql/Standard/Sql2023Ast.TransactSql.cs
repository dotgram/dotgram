using System.Collections.Generic;

namespace DotGram.Sql.Ast;

// T-SQL's own nodes, in the standard's families and beside them (docs/design/sql-tsql-tree.md).
//
// Every record here carries `// T-SQL:` and the page of Microsoft's reference it comes from (P1),
// spelled as `TransactSql/Specification/syntax.md` spells the heading, so a reader can find it by
// searching that file. Nothing here says `Extension` and nothing holds a tail of text (P2): a
// construct T-SQL reads has a typed place or it is not read.
//
// These are the parts the statements are made of. The statements themselves follow in their own
// commits; a record whose members the inventory left open is declared with the statement that
// decides them, not before it.

/// <summary>A <c>TOP</c> before a select list or a data change statement's target.</summary>
// T-SQL: TOP (Transact-SQL)
public sealed record TopClause(Expression Value, bool Parentheses, bool Percent, bool WithTies) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>
/// <c>SELECT … INTO t</c>, which makes a table. Not the standard's <see cref="IntoClause"/>, whose
/// targets are variables.
/// </summary>
// T-SQL: INTO Clause (Transact-SQL)
public sealed record SelectIntoTable(QualifiedName Table, Identifier? FileGroup) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>A column of a rowset function's <c>WITH (…)</c> schema.</summary>
// T-SQL: OPENJSON (Transact-SQL), OPENXML (Transact-SQL), OPENROWSET BULK (Transact-SQL)
public sealed record RowsetColumn(Identifier Name, DataType? Type, string? Path, bool AsJson, int? Ordinal) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>What a query returns instead of rows.</summary>
// T-SQL: FOR Clause (Transact-SQL)
public abstract record ForClause : ISqlNode
{
	public SqlSpan Span { get; private set; }

	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}

	public sealed record Xml(Identifier Mode, IReadOnlyList<Option> Options) : ForClause;
	public sealed record Json(Identifier Mode, IReadOnlyList<Option> Options) : ForClause;
	public sealed record Browse : ForClause;
}

/// <summary>
/// A hint on a table source, <c>WITH (NOLOCK)</c> and the rest.
/// </summary>
/// <remarks>
/// A family with a named member for the long tail rather than text (proposal 9): a hint the grammar
/// reads as a shape of its own gains a member here, and the rest is <see cref="Named"/>.
/// </remarks>
// T-SQL: Table Hints (Transact-SQL)
public abstract record TableHint : ISqlNode
{
	public SqlSpan Span { get; private set; }

	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}

	public sealed record Named(Identifier Name, IReadOnlyList<Expression> Arguments) : TableHint;
}

/// <summary>A hint in a statement's <c>OPTION (…)</c>.</summary>
// T-SQL: Query Hints (Transact-SQL)
public abstract record QueryHint : ISqlNode
{
	public SqlSpan Span { get; private set; }

	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}

	public sealed record Named(Identifier Name, IReadOnlyList<Expression> Arguments) : QueryHint;
}

/// <summary>How a join is to be performed.</summary>
// T-SQL: Join hints (Transact-SQL)
public enum JoinHint { Loop, Hash, Merge, Remote }

/// <summary>
/// A named option, with a value or options of its own: the <c>WITH (…)</c> of the DDL statements.
/// </summary>
/// <remarks>
/// One record for the whole catalogue (proposal 20). It is a name and a value, not a tail of text,
/// so a round trip has something typed to print and a reader something to ask about.
/// </remarks>
// T-SQL: the WITH clause of the CREATE and ALTER statements, each on its own page
public sealed record Option(Identifier Name, Expression? Value, IReadOnlyList<Option> Options) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>Where a table, its large values or its filestream data are put.</summary>
// T-SQL: CREATE TABLE (Transact-SQL)
public sealed record Placement(PlacementKind Kind, Identifier Target, IReadOnlyList<Identifier>? Columns) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>Which placement a <see cref="Placement"/> is.</summary>
// T-SQL: CREATE TABLE (Transact-SQL)
public enum PlacementKind { On, TextImageOn, FilestreamOn }

/// <summary>
/// What a data change statement gives back, and where it puts it.
/// </summary>
/// <remarks>
/// <see cref="Next"/> because a statement may write both an <c>OUTPUT INTO</c> and a second
/// <c>OUTPUT</c> to the caller.
/// </remarks>
// T-SQL: OUTPUT clause (Transact-SQL)
public sealed record OutputClause(IReadOnlyList<SelectItem> Items, TableSource? Into, IReadOnlyList<Identifier>? Columns, OutputClause? Next) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>Whom a module or a statement runs as.</summary>
// T-SQL: EXECUTE (Transact-SQL)
public sealed record ExecutionContext(ExecutionContextKind Kind, Expression Name) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>Which execution context an <see cref="ExecutionContext"/> names.</summary>
// T-SQL: EXECUTE (Transact-SQL)
public enum ExecutionContextKind { Caller, Self, Owner, User, Login }

/// <summary>A partition, or a range of them.</summary>
// T-SQL: TRUNCATE TABLE (Transact-SQL)
public sealed record PartitionRange(Expression From, Expression? To) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>An edge constraint of a graph table: which node table connects to which.</summary>
// T-SQL: CREATE TABLE (Transact-SQL)
public sealed record EdgeConnection(QualifiedName From, QualifiedName To) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}
