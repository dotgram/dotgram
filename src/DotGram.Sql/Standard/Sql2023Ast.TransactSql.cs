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

/// <summary>T-SQL: TOP (Transact-SQL). A <c>TOP</c> before a select list or a data change statement's target.</summary>
public sealed record TopClause(Expression Value, bool Parentheses, bool Percent, bool WithTies) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>
/// T-SQL: INTO Clause (Transact-SQL). <c>SELECT … INTO t</c>, which makes a table. Not the
/// standard's <see cref="IntoClause"/>, whose targets are variables.
/// </summary>
public sealed record SelectIntoTable(QualifiedName Table, Identifier? FileGroup) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>T-SQL: OPENJSON, OPENXML, OPENROWSET BULK (Transact-SQL). A column of a rowset function's <c>WITH (…)</c> schema.</summary>
public sealed record RowsetColumn(Identifier Name, DataType? Type, string? Path, bool AsJson, int? Ordinal) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>T-SQL: FOR Clause (Transact-SQL). What a query returns instead of rows.</summary>
public abstract record ForClause : ISqlNode
{
	public SqlSpan Span { get; private set; }

	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}

	/// <summary><c>FOR XML RAW</c>, <c>AUTO</c>, <c>EXPLICIT</c> or <c>PATH</c>, and its options.</summary>
	public sealed record Xml(Identifier Mode, IReadOnlyList<Option> Options) : ForClause;
	/// <summary><c>FOR JSON AUTO</c> or <c>PATH</c>, and its options.</summary>
	public sealed record Json(Identifier Mode, IReadOnlyList<Option> Options) : ForClause;
	/// <summary><c>FOR BROWSE</c>, which takes nothing.</summary>
	public sealed record Browse : ForClause;
}

/// <summary>
/// T-SQL: Table Hints (Transact-SQL). A hint on a table source, <c>WITH (NOLOCK)</c> and the rest.
/// </summary>
/// <remarks>
/// A family with a named member for the long tail rather than text (proposal 9): a hint the grammar
/// reads as a shape of its own gains a member here, and the rest is <see cref="Named"/>.
/// </remarks>
public abstract record TableHint : ISqlNode
{
	public SqlSpan Span { get; private set; }

	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}

	/// <summary>A hint the grammar reads by name, with whatever it was given in brackets.</summary>
	public sealed record Named(Identifier Name, IReadOnlyList<Expression> Arguments) : TableHint;
}

/// <summary>T-SQL: Query Hints (Transact-SQL). A hint in a statement's <c>OPTION (…)</c>.</summary>
public abstract record QueryHint : ISqlNode
{
	public SqlSpan Span { get; private set; }

	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}

	/// <summary>A hint the grammar reads by name, with whatever it was given in brackets.</summary>
	public sealed record Named(Identifier Name, IReadOnlyList<Expression> Arguments) : QueryHint;
}

/// <summary>T-SQL: Join hints (Transact-SQL). How a join is to be performed.</summary>
public enum JoinHint { Loop, Hash, Merge, Remote }

/// <summary>
/// T-SQL: the <c>WITH (…)</c> of the CREATE and ALTER statements, each on its own page. A named
/// </summary>
/// <remarks>
/// option, with a value or options of its own. One record for the whole catalogue (proposal 20):
/// it is a name and a value, not a tail of text, so a round trip has something typed to print.
/// </remarks>
public sealed record Option(Identifier Name, Expression? Value, IReadOnlyList<Option> Options) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>T-SQL: CREATE TABLE (Transact-SQL). Where a table, its large values or its filestream data are put.</summary>
public sealed record Placement(PlacementKind Kind, Identifier Target, IReadOnlyList<Identifier>? Columns) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>T-SQL: CREATE TABLE (Transact-SQL). Which placement a <see cref="Placement"/> is.</summary>
public enum PlacementKind { On, TextImageOn, FilestreamOn }

/// <summary>
/// T-SQL: OUTPUT clause (Transact-SQL). What a data change statement gives back, and where it puts it.
/// </summary>
/// <remarks>
/// <see cref="Next"/> because a statement may write both an <c>OUTPUT INTO</c> and a second
/// <c>OUTPUT</c> to the caller.
/// </remarks>
public sealed record OutputClause(IReadOnlyList<SelectItem> Items, TableSource? Into, IReadOnlyList<Identifier>? Columns, OutputClause? Next) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>T-SQL: EXECUTE (Transact-SQL). Whom a module or a statement runs as.</summary>
public sealed record ExecutionContext(ExecutionContextKind Kind, Expression Name) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>T-SQL: EXECUTE (Transact-SQL). Which execution context an <see cref="ExecutionContext"/> names.</summary>
public enum ExecutionContextKind { Caller, Self, Owner, User, Login }

/// <summary>T-SQL: TRUNCATE TABLE (Transact-SQL). A partition, or a range of them.</summary>
public sealed record PartitionRange(Expression From, Expression? To) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}

/// <summary>T-SQL: CREATE TABLE (Transact-SQL). An edge constraint of a graph table: which node table connects to which.</summary>
public sealed record EdgeConnection(QualifiedName From, QualifiedName To) : ISqlNode { public SqlSpan Span { get; private set; }
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}
}
