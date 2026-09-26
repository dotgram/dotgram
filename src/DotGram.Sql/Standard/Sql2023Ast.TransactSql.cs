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

/// <summary>T-SQL: SET Statements (Transact-SQL). One option of a <c>SET</c> statement.</summary>
/// <remarks>
/// One level rather than two: Microsoft groups the options by what they affect, and the old tree made
/// a family of each group, so the same <c>SET x ON</c> was five records. The group is a property here
/// (proposal 24), and the switches are one member.
/// </remarks>
public abstract record SetOption : ISqlNode
{
	/// <summary>Where in the text this option was written.</summary>
	public SqlSpan Span { get; private set; }

	/// <summary>Records where this option was written.</summary>
	public void Locate(int at, int length)
	{
		Span = new SqlSpan(at, length);
	}

	/// <summary>An option that is only turned on or off, such as <c>SET ANSI_NULLS ON</c>.</summary>
	public sealed record Switch(Identifier Option, bool On, SetCategory Category) : SetOption;

	/// <summary>T-SQL: SET DATEFIRST (Transact-SQL). Which day the week starts on.</summary>
	public sealed record DateFirst(Expression Value) : SetOption;

	/// <summary>T-SQL: SET DATEFORMAT (Transact-SQL). The order of the parts of a date.</summary>
	public sealed record DateFormat(Expression Value) : SetOption;

	/// <summary>T-SQL: SET DEADLOCK_PRIORITY (Transact-SQL). Which session gives way in a deadlock.</summary>
	public sealed record DeadlockPriority(Expression Value) : SetOption;

	/// <summary>T-SQL: SET LOCK_TIMEOUT (Transact-SQL). How long a statement waits for a lock.</summary>
	public sealed record LockTimeout(Expression Value) : SetOption;

	/// <summary>T-SQL: SET ROWCOUNT (Transact-SQL). How many rows a statement stops after.</summary>
	public sealed record RowCount(Expression Value) : SetOption;

	/// <summary>T-SQL: SET TEXTSIZE (Transact-SQL). How much of a large value is returned.</summary>
	public sealed record TextSize(Expression Value) : SetOption;

	/// <summary>T-SQL: SET QUERY_GOVERNOR_COST_LIMIT (Transact-SQL). The cost a query may not exceed.</summary>
	public sealed record QueryGovernorCostLimit(Expression Value) : SetOption;

	/// <summary>T-SQL: SET STATISTICS IO, PROFILE, TIME, XML (Transact-SQL). Which report a statement returns.</summary>
	public sealed record Statistics(StatisticsKind Kind, bool On) : SetOption;

	/// <summary>T-SQL: SET LANGUAGE (Transact-SQL). The language of the messages and the date names.</summary>
	public sealed record Language(Expression Value) : SetOption;

	/// <summary>T-SQL: SET FIPS_FLAGGER (Transact-SQL). Which standard to warn against departing from.</summary>
	public sealed record FipsFlagger(Expression Level) : SetOption;

	/// <summary>T-SQL: SET CONTEXT_INFO (Transact-SQL). A binary value the session carries.</summary>
	public sealed record ContextInfo(Expression Value) : SetOption;

	/// <summary>T-SQL: SET IDENTITY_INSERT (Transact-SQL). Whether a table takes explicit identity values.</summary>
	public sealed record IdentityInsert(QualifiedName Table, bool On) : SetOption;

	/// <summary>T-SQL: SET OFFSETS (Transact-SQL). Which key words the offsets are returned for.</summary>
	public sealed record Offsets(Identifier Keyword, bool On) : SetOption;

	/// <summary>T-SQL: SET Statements (Transact-SQL). Above which severity an error is returned.</summary>
	public sealed record ErrorLevel(Expression Value) : SetOption;
}

/// <summary>T-SQL: SET Statements (Transact-SQL). What a <see cref="SetOption.Switch"/> affects.</summary>
/// <remarks>
/// Microsoft's own grouping of the pages, kept because it is how a reader finds the option in the
/// reference. It is derivable from the option's name, so it is a convenience rather than a fact only
/// the text holds.
/// </remarks>
public enum SetCategory { DateAndTime, Locking, QueryExecution, IsoSettings, Statistics, Transactions, Miscellaneous }

/// <summary>T-SQL: SET STATISTICS IO, PROFILE, TIME, XML (Transact-SQL). Which statistics report is asked for.</summary>
public enum StatisticsKind { Io, Profile, Time, Xml }

/// <summary>T-SQL's own value expressions, in the standard's family.</summary>
public abstract partial record Expression
{
	/// <summary>T-SQL: the WITH clause of the CREATE and ALTER statements. A value and the unit it is written in, as in <c>10 MINUTES</c>, <c>50 PERCENT</c> or <c>4 GB</c>.</summary>
	public sealed record Measured(Expression Value, Identifier Unit) : Expression;

	/// <summary>T-SQL: ODBC Scalar Functions (Transact-SQL). An ODBC escape, as in <c>{fn LEFT(a, 2)}</c> or <c>{d '2026-09-26'}</c>.</summary>
	/// <remarks>
	/// <see cref="Kind"/> is an <see cref="Identifier"/> and not an enum, because the reference's list of
	/// the escapes is open-ended and an enum written now would be a guess at its end. It becomes one when
	/// the grammar has read them all and the set is known to be closed.
	/// </remarks>
	public sealed record OdbcEscape(Identifier Kind, Expression Value, bool Called) : Expression;
}
