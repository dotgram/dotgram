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

/// <summary>
/// T-SQL: TOP (Transact-SQL). A <c>TOP</c> before a select list or a data change statement's target; <see cref="With"/>
/// is what its WITH says, null where none was written.
/// </summary>
public sealed record TopClause(Expression Value, bool Parentheses, bool Percent, TopWith? With) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// T-SQL: TOP (Transact-SQL), VECTOR_SEARCH (Transact-SQL). What a TOP's WITH says: <c>WITH TIES</c>, the rows tied with
/// the last; <c>WITH APPROXIMATE</c>, an approximate search's rows. The two exclude each other.
/// </summary>
public enum TopWith { Ties, Approximate }

/// <summary>
/// T-SQL: INTO Clause (Transact-SQL). <c>SELECT … INTO t</c>, which makes a table. Not the
/// standard's <see cref="IntoClause"/>, whose targets are variables.
/// </summary>
public sealed record SelectIntoTable(QualifiedName Table, Identifier? FileGroup) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: OPENJSON, OPENXML, OPENROWSET BULK (Transact-SQL). A column of a rowset function's <c>WITH (…)</c> schema.</summary>
public sealed record RowsetColumn(Identifier Name, DataType? Type, string? Path, bool AsJson, int? Ordinal) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: OPENJSON (Transact-SQL), OPENXML (Transact-SQL). The column's <c>COLLATE</c>, after its type.</summary>
	public CollationName? Collation { get; init; }
}

/// <summary>T-SQL: FOR Clause (Transact-SQL). What a query returns instead of rows.</summary>
public abstract record ForClause : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// <c>FOR XML RAW</c>, <c>AUTO</c>, <c>EXPLICIT</c> or <c>PATH</c>, the element each row is written as
	/// (<c>RAW ('row')</c>, <c>PATH ('')</c>), which the grammar reads after every mode, and the directives in the
	/// order written.
	/// </summary>
	public sealed record Xml(XmlMode Mode, Expression? ElementName, SqlList<ForDirective> Directives) : ForClause;

	/// <summary><c>FOR JSON AUTO</c> or <c>PATH</c>, and the directives in the order written.</summary>
	public sealed record Json(JsonMode Mode, SqlList<ForDirective> Directives) : ForClause;
	/// <summary><c>FOR BROWSE</c>, which takes nothing.</summary>
	public sealed record Browse : ForClause;
}

/// <summary>T-SQL: FOR Clause (Transact-SQL). How <c>FOR XML</c> shapes the rows.</summary>
public enum XmlMode { Raw, Auto, Explicit, Path }

/// <summary>T-SQL: FOR Clause (Transact-SQL). How <c>FOR JSON</c> shapes the rows.</summary>
public enum JsonMode { Auto, Path }

/// <summary>
/// T-SQL: FOR Clause (Transact-SQL). One directive after the mode: <see cref="Name"/> is the string of
/// <c>ROOT ('r')</c> and <c>XMLSCHEMA ('uri')</c>; <see cref="Absent"/> is <c>ELEMENTS ABSENT</c>, which means what
/// <c>ELEMENTS</c> alone means.
/// </summary>
public sealed record ForDirective(ForDirectiveKind Kind, Expression? Name = null, [property: Spelling] bool Absent = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// T-SQL: FOR Clause (Transact-SQL). What a directive asks for. <c>FOR XML</c> only: <c>BINARY BASE64</c>,
/// <c>TYPE</c>, <c>XMLDATA</c>, <c>XMLSCHEMA</c>, <c>ELEMENTS</c>, <c>ELEMENTS XSINIL</c>; <c>FOR JSON</c> only:
/// <c>INCLUDE_NULL_VALUES</c>, <c>WITHOUT_ARRAY_WRAPPER</c>; both: <c>ROOT</c>.
/// </summary>
public enum ForDirectiveKind { BinaryBase64, Type, Root, XmlData, XmlSchema, Elements, ElementsXsiNil, IncludeNullValues, WithoutArrayWrapper }

/// <summary>
/// T-SQL: Table Hints (Transact-SQL). A hint on a table source, <c>WITH (NOLOCK)</c> and the rest.
/// </summary>
/// <remarks>
/// A family with a named member for the long tail rather than text (proposal 9): a hint of one word, with or
/// without a value, is <see cref="Named"/>; a hint with a structure has a member of its own. Whether the commas
/// between a source's hints were written is not kept — the grammar lets them be left out — and the writer always
/// writes them.
/// </remarks>
public abstract record TableHint : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>A hint the grammar reads by name, with its values: <c>NOLOCK</c>, <c>INDEX (ix, 2)</c>, <c>INDEX = ix</c>.</summary>
	public sealed record Named(Identifier Name, SqlList<Expression> Arguments) : TableHint
	{
		/// <summary>How the values were written; the form never changes what the hint means.</summary>
		[Spelling] public HintArgumentForm Form { get; init; } = HintArgumentForm.Brackets;
	}

	/// <summary><c>FORCESEEK (ix (a, b))</c>: the index and the columns it is sought on. A bare <c>FORCESEEK</c> is <see cref="Named"/>.</summary>
	public sealed record ForceSeek(Expression Index, SqlList<Identifier> Columns) : TableHint;
}

/// <summary>T-SQL: Table Hints (Transact-SQL), Query Hints (Transact-SQL). <c>NAME (a, b)</c>, <c>NAME a</c>, <c>NAME = a</c>.</summary>
public enum HintArgumentForm { Brackets, Bare, Assigned }

/// <summary>T-SQL: Query Hints (Transact-SQL). A hint in a statement's <c>OPTION (…)</c>.</summary>
public abstract record QueryHint : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// A hint of one word, with or without a value: <c>RECOMPILE</c>, <c>MAXDOP 2</c>, <c>MAX_GRANT_PERCENT = 50</c>,
	/// <c>LABEL = 'x'</c>.
	/// </summary>
	public sealed record Named(Identifier Name, SqlList<Expression> Arguments) : QueryHint
	{
		/// <summary>How the value was written; the form never changes what the hint means.</summary>
		[Spelling] public HintArgumentForm Form { get; init; } = HintArgumentForm.Brackets;
	}

	/// <summary>A hint of several fixed words: <c>HASH GROUP</c>, <c>FORCE ORDER</c>, <c>KEEP PLAN</c>.</summary>
	public sealed record Phrase(QueryHintPhrase Words) : QueryHint;

	/// <summary><c>USE HINT ('a', N'b')</c>.</summary>
	public sealed record UseHint(SqlList<Expression> Hints) : QueryHint;

	/// <summary><c>USE PLAN N'…'</c>.</summary>
	public sealed record UsePlan(Expression Plan) : QueryHint;

	/// <summary><c>OPTIMIZE FOR (@a = 1, @b UNKNOWN, @c)</c>.</summary>
	public sealed record OptimizeFor(SqlList<OptimizeForVariable> Variables) : QueryHint;

	/// <summary><c>OPTIMIZE FOR UNKNOWN</c>.</summary>
	public sealed record OptimizeForUnknown : QueryHint;

	/// <summary><c>TABLE HINT (t, NOLOCK, INDEX (i))</c>: the table and the hints it is given.</summary>
	public sealed record ForTable(QualifiedName Table, SqlList<TableHint> Hints) : QueryHint;
}

/// <summary>T-SQL: Query Hints (Transact-SQL). The query hints of several fixed words.</summary>
public enum QueryHintPhrase
{
	HashGroup, OrderGroup, ConcatUnion, HashUnion, MergeUnion, KeepUnion, LoopJoin, MergeJoin, HashJoin, ExpandViews,
	ForceOrder, ForceScaleOutExecution, DisableScaleOutExecution, KeepPlan, KeepFixedPlan, RobustPlan,
}

/// <summary>
/// T-SQL: Query Hints (Transact-SQL). One variable of <c>OPTIMIZE FOR (…)</c>: the variable, keeping its <c>@</c>,
/// and the value it is optimised for, or <c>UNKNOWN</c>, or neither.
/// </summary>
public sealed record OptimizeForVariable(Identifier Variable, Expression? Value, bool Unknown) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
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
public sealed record Option(Identifier Name, Expression? Value, SqlList<Option> Options) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: OPENROWSET BULK (Transact-SQL). An <c>ORDER (…) [UNIQUE]</c> option, whose name is <c>ORDER</c>.</summary>
	public RowOrder? Order { get; init; }

	/// <summary>
	/// T-SQL: ALTER TABLE index_option (Transact-SQL), UPDATE STATISTICS (Transact-SQL). <c>DATA_COMPRESSION = PAGE ON
	/// PARTITIONS (1, 3 TO 5)</c>: the partitions the option is set for, after its value; null where none were said.
	/// </summary>
	public SqlList<PartitionRange>? Partitions { get; init; }

	/// <summary>
	/// T-SQL: the options of the CREATE and ALTER statements, each on its own page. The further words of a name written in
	/// several words: <c>MOVE TO fg</c> is <see cref="Name"/> <c>MOVE</c> with <c>TO</c> here, and <c>HADR AVAILABILITY
	/// GROUP = ag</c> is <c>HADR</c> with <c>AVAILABILITY</c> and <c>GROUP</c> here. Empty for a name of one word.
	/// </summary>
	public SqlList<Identifier> Words { get; init; } = [];

	/// <summary>
	/// T-SQL: the options of the CREATE and ALTER statements, each on its own page. A value or a bracketed list written
	/// after the name with no <c>=</c>, as in <c>WITH INLINE ON</c>. Set only where one place reads both forms for the same
	/// name, which is the generic option rule (<c>= v</c> or <c>ON</c> | <c>OFF</c> against <c>= (…)</c> or <c>(…)</c>) read
	/// without a lookahead that narrows it. Where a place's own rule fixes the form for a name (<c>RECOVERY FULL</c>,
	/// <c>PAD_INDEX = ON</c>), the flag stays false and that place's writer prints the fixed form.
	/// </summary>
	[Spelling] public bool EqualsOmitted { get; init; }
}

/// <summary>
/// T-SQL: BULK INSERT (Transact-SQL), OPENROWSET BULK (Transact-SQL). <c>ORDER (c1 ASC, …) [UNIQUE]</c>: the data is
/// already sorted so.
/// </summary>
public sealed record RowOrder(SqlList<SortItem> Columns, bool Unique) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: CREATE TABLE (Transact-SQL). Where a table, its large values or its filestream data are put.</summary>
public sealed record Placement(PlacementKind Kind, Identifier Target, SqlList<Identifier>? Columns) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
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
public sealed record OutputClause(SqlList<SelectColumn> Items, TableSource? Into, SqlList<Identifier>? Columns, OutputClause? Next) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: EXECUTE (Transact-SQL). Whom a module or a statement runs as.</summary>
public sealed record ExecutionContext(ExecutionContextKind Kind, Expression Name) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: EXECUTE (Transact-SQL). Which execution context an <see cref="ExecutionContext"/> names.</summary>
public enum ExecutionContextKind { Caller, Self, Owner, User, Login }

/// <summary>T-SQL: TRUNCATE TABLE (Transact-SQL). A partition, or a range of them.</summary>
public sealed record PartitionRange(Expression From, Expression? To) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: CREATE TABLE (Transact-SQL). An edge constraint of a graph table: which node table connects to which.</summary>
public sealed record EdgeConnection(QualifiedName From, QualifiedName To) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
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
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this option was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
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

	/// <summary>
	/// T-SQL: DECLARE @local_variable (Transact-SQL). A variable standing where a value does: a local <c>@x</c>
	/// or a system <c>@@x</c>.
	/// </summary>
	/// <remarks>
	/// The identifier's text holds the <c>@</c> or the <c>@@</c>, as <c>#t</c>'s text holds its <c>#</c>
	/// (proposal 10): in T-SQL the sign is part of the name's token, so it is part of the name, and one record
	/// serves both. Not <see cref="Parameter"/>, the standard's <c>:x</c> and <c>?</c>, whose name leaves its
	/// colon out.
	/// </remarks>
	public sealed record Variable(Identifier Name) : Expression;

	/// <summary>
	/// T-SQL: CAST and CONVERT (Transact-SQL), TRY_CONVERT (Transact-SQL).
	/// <c>[TRY_]CONVERT (type, value [, style])</c>.
	/// </summary>
	public sealed record Convert(DataType Type, Expression Value, Expression? Style = null, bool Try = false) : Expression;

	/// <summary>
	/// T-SQL: PARSE (Transact-SQL), TRY_PARSE (Transact-SQL). <c>[TRY_]PARSE (value AS type [USING culture])</c>.
	/// </summary>
	public sealed record Parse(Expression Value, DataType Type, Expression? Culture = null, bool Try = false) : Expression;

	/// <summary>
	/// T-SQL: DATEADD (Transact-SQL), and DATEDIFF, DATEDIFF_BIG, DATENAME, DATEPART, DATETRUNC, DATE_BUCKET.
	/// The date part these take as their first argument, which is neither a column nor a value: its kind and
	/// which of its words was written (<c>day</c>, <c>dd</c>, <c>d</c>).
	/// </summary>
	public sealed record DatePart(DatePartKind Kind, [property: Spelling] DatePartSpelling Spelling = DatePartSpelling.Name) : Expression;

	public partial record Like
	{
		/// <summary>
		/// T-SQL: LIKE (Transact-SQL). The escape clause written in ODBC's braces, <c>{ESCAPE '%'}</c>.
		/// </summary>
		[Spelling] public bool BracedEscape { get; init; }
	}

	/// <summary>
	/// T-SQL: TRY_CAST (Transact-SQL). <c>Try</c> is <c>TRY_CAST</c>, which gives null where <c>CAST</c> fails.
	/// </summary>
	public partial record Cast
	{
		public bool Try { get; init; }
	}

	/// <summary>
	/// T-SQL: NEXT VALUE FOR (Transact-SQL). <c>OVER (ORDER BY …)</c>, which numbers the rows of one statement in
	/// that order.
	/// </summary>
	public partial record NextValue
	{
		public WindowSpecification? Over { get; init; }
	}
}

/// <summary>T-SQL's own data types, in the standard's family (proposal 14).</summary>
public abstract partial record DataType
{
	/// <summary>
	/// T-SQL: the type's name when it was written as an identifier — bracketed, quoted or <c>sys.</c>-qualified —
	/// rather than as the key word; the same type.
	/// </summary>
	[Spelling] public QualifiedName? WrittenName { get; init; }

	/// <summary>
	/// T-SQL: Data types (Transact-SQL), a page syntax.md does not transcribe. A type T-SQL names that the standard
	/// does not spell: a name and the numbers written in its brackets — <c>tinyint</c>, <c>money</c>,
	/// <c>datetime2 (7)</c>, <c>[money]</c>, <c>sys.sysname</c>.
	/// </summary>
	/// <remarks>
	/// On SQL Server a system type's name bracketed, quoted or qualified by <c>sys</c> is the same type as the bare
	/// word, and the tree says so. In order: a one-word spelling whose meaning is the standard type's (int, integer,
	/// smallint, bigint, dec, decimal, numeric, float, real, char, character, varchar, nchar, nvarchar, binary,
	/// varbinary, date, time, json), bare, bracketed, quoted or qualified by <c>sys</c>, is the standard's record,
	/// with <see cref="WrittenName"/> set unless it was bare — never <c>timestamp</c>, which in T-SQL is rowversion;
	/// a standard spelling of several words, unbracketed, is the standard's record; <c>xml</c> is <see cref="Xml"/>;
	/// a name of one part, or <c>sys</c> and a name, however quoted, that is one of T-SQL's system types is
	/// <see cref="Named"/>, its arguments numbers; and every other name is the standard's <see cref="UserDefined"/>.
	/// </remarks>
	public sealed record Named(QualifiedName Name, SqlList<Expression> Arguments) : DataType;

	/// <summary>
	/// T-SQL: xml (Transact-SQL). <c>xml [([CONTENT | DOCUMENT] schema_collection)]</c>. <c>Modifier</c> only with
	/// <c>Schema</c>; the writer refuses <c>Modifier</c> alone.
	/// </summary>
	public sealed record Xml(XmlTypeModifier? Modifier = null, QualifiedName? Schema = null) : DataType;
}

/// <summary>T-SQL: xml (Transact-SQL). The XML type's modifier, <c>DOCUMENT</c> or <c>CONTENT</c>, as SQL/XML names it.</summary>
public enum XmlTypeModifier { Content, Document }

/// <summary>
/// T-SQL: DATEADD (Transact-SQL), DATEPART (Transact-SQL). The part of a date a function takes, by the table of
/// those pages: the name, its abbreviation and its second abbreviation.
/// </summary>
/// <remarks>
/// <list type="table">
/// <item><term>Year</term><description>year, yy, yyyy</description></item>
/// <item><term>Quarter</term><description>quarter, qq, q</description></item>
/// <item><term>Month</term><description>month, mm, m</description></item>
/// <item><term>DayOfYear</term><description>dayofyear, dy, y</description></item>
/// <item><term>Day</term><description>day, dd, d</description></item>
/// <item><term>Week</term><description>week, wk, ww</description></item>
/// <item><term>Weekday</term><description>weekday, dw, w</description></item>
/// <item><term>Hour</term><description>hour, hh</description></item>
/// <item><term>Minute</term><description>minute, mi, n</description></item>
/// <item><term>Second</term><description>second, ss, s</description></item>
/// <item><term>Millisecond</term><description>millisecond, ms</description></item>
/// <item><term>Microsecond</term><description>microsecond, mcs</description></item>
/// <item><term>Nanosecond</term><description>nanosecond, ns</description></item>
/// <item><term>TzOffset</term><description>tzoffset, tz</description></item>
/// <item><term>IsoWeek</term><description>iso_week, isowk, isoww</description></item>
/// </list>
/// <c>y</c> is the day of the year and <c>w</c> the weekday. Which function takes which part is the grammar's
/// business, not the tree's; the word's case is not kept.
/// </remarks>
public enum DatePartKind { Year, Quarter, Month, DayOfYear, Day, Week, Weekday, Hour, Minute, Second, Millisecond, Microsecond, Nanosecond, TzOffset, IsoWeek }

/// <summary>T-SQL: DATEADD (Transact-SQL). Which of a date part's words was written (<see cref="DatePartKind"/>).</summary>
public enum DatePartSpelling { Name, Abbreviation, SecondAbbreviation }

/// <summary>T-SQL's own literals, in the standard's family (proposal 11).</summary>
public abstract partial record LiteralValue
{
	/// <summary>
	/// T-SQL: Constants (Transact-SQL), a page syntax.md does not transcribe. T-SQL writes a binary string
	/// <c>0x0A</c> where the standard writes <c>X'0A'</c>.
	/// </summary>
	public new partial record Binary
	{
		/// <summary>
		/// Written <c>0x…</c>. Then <c>Text</c> is the digits after the <c>0x</c>; without it, <c>Text</c> is the
		/// quoted digits after the standard's <c>X</c>. The introducer is not in the text, as a string's is not.
		/// </summary>
		[Spelling] public bool ZeroX { get; init; }
	}
}

/// <summary>T-SQL: ALTER DATABASE (Transact-SQL). What an <c>ALTER DATABASE</c> does.</summary>
/// <remarks>
/// One action a statement, as the published syntax has it. <c>SET</c> is not here: it carries its own
/// catalogue of options and is a family of its own.
/// </remarks>
public abstract record AlterDatabaseAction : ISqlNode
{
	/// <summary>Where in the text this action was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this action was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>COLLATE collation_name</c>.</summary>
	public sealed record Collate(CollationName Collation) : AlterDatabaseAction;

	/// <summary><c>MODIFY NAME = new_database_name</c>.</summary>
	public sealed record ModifyName(Identifier Name) : AlterDatabaseAction;

	/// <summary>T-SQL: ALTER DATABASE (Transact-SQL), the Azure syntax. <c>MODIFY (edition options) [WITH MANUAL_CUTOVER]</c>.</summary>
	public sealed record Modify(SqlList<Option> Options, bool ManualCutover) : AlterDatabaseAction;

	/// <summary><c>MODIFY BACKUP_STORAGE_REDUNDANCY =</c> one of three quoted words.</summary>
	/// <remarks>
	/// The value stays an expression rather than becoming an enum: the reference writes the three
	/// quoted, and the round trip has to give the literal back as written, case included.
	/// </remarks>
	public sealed record ModifyBackupStorageRedundancy(Expression Value) : AlterDatabaseAction;

	/// <summary><c>PERFORM_CUTOVER</c>, which takes nothing.</summary>
	public sealed record PerformCutover : AlterDatabaseAction;

	/// <summary><c>REBUILD LOG</c>, which takes nothing here.</summary>
	/// <remarks>
	/// The one action of this statement that <c>TransactSql/Specification/syntax.md</c> does not publish:
	/// the words appear nowhere in the transcribed pages, and the grammar reads them anyway. So it has
	/// the parts the current reading gives it, which is none, and none is invented for it.
	/// </remarks>
	public sealed record RebuildLog : AlterDatabaseAction;

	/// <summary>T-SQL: ALTER DATABASE File and Filegroups. <c>ADD FILE ... [TO FILEGROUP g]</c>.</summary>
	public sealed record AddFile(SqlList<DatabaseFile> Files, Identifier? ToFileGroup) : AlterDatabaseAction;

	/// <summary>T-SQL: ALTER DATABASE File and Filegroups. <c>ADD LOG FILE ...</c>.</summary>
	public sealed record AddLogFile(SqlList<DatabaseFile> Files) : AlterDatabaseAction;

	/// <summary>T-SQL: ALTER DATABASE File and Filegroups. <c>MODIFY FILE (...)</c>.</summary>
	public sealed record ModifyFile(DatabaseFile File) : AlterDatabaseAction;

	/// <summary>T-SQL: ALTER DATABASE File and Filegroups. <c>REMOVE FILE logical_file_name</c>.</summary>
	public sealed record RemoveFile(Identifier Name) : AlterDatabaseAction;

	/// <summary>T-SQL: ALTER DATABASE File and Filegroups. <c>ADD FILEGROUP g [CONTAINS ...]</c>.</summary>
	public sealed record AddFileGroup(Identifier Name, FileGroupContents? Contains) : AlterDatabaseAction;

	/// <summary>T-SQL: ALTER DATABASE File and Filegroups. <c>MODIFY FILEGROUP g ...</c>.</summary>
	public sealed record ModifyFileGroup(Identifier Name, FileGroupChange Change) : AlterDatabaseAction;

	/// <summary>T-SQL: ALTER DATABASE File and Filegroups. <c>REMOVE FILEGROUP g</c>.</summary>
	public sealed record RemoveFileGroup(Identifier Name) : AlterDatabaseAction;
}

/// <summary>T-SQL: ALTER DATABASE File and Filegroups. A file, as its <c>(NAME = ..., SIZE = ...)</c> says it.</summary>
/// <remarks>
/// The filespec is a list of named options and nothing else: <c>OFFLINE</c> is a name with no value,
/// <c>MAXSIZE = UNLIMITED</c> a name as one, and <c>SIZE = 10 MB</c> an
/// <see cref="Expression.Measured"/>. So no part of it is a tail of text (P2).
/// </remarks>
public sealed record DatabaseFile(SqlList<Option> Options) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	/// <summary>Records where this file was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: ALTER DATABASE File and Filegroups. What a filegroup is declared to contain.</summary>
public enum FileGroupContents { Filestream, MemoryOptimizedData }

/// <summary>T-SQL: ALTER DATABASE File and Filegroups. What <c>MODIFY FILEGROUP</c> changes.</summary>
public abstract record FileGroupChange : ISqlNode
{
	/// <summary>Where in the text this change was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this change was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>Whether the filegroup may be written to.</summary>
	/// <remarks>
	/// <see cref="Underscored"/> because the reference publishes four words for two meanings:
	/// <c>READONLY</c> and <c>READ_ONLY</c>, <c>READWRITE</c> and <c>READ_WRITE</c>. The underscore is a
	/// spelling, so it is kept as a flag, as <c>Exclamation</c> is for <c>!=</c>.
	/// </remarks>
	public sealed record Updatability(FileGroupUpdatability Kind, [property: Spelling] bool Underscored) : FileGroupChange;

	/// <summary><c>DEFAULT</c>, which takes nothing.</summary>
	public sealed record Default : FileGroupChange;

	/// <summary><c>NAME = new_filegroup_name</c>.</summary>
	public sealed record Rename(Identifier Name) : FileGroupChange;

	/// <summary><c>AUTOGROW_SINGLE_FILE</c> or <c>AUTOGROW_ALL_FILES</c>.</summary>
	public sealed record AutoGrow(FileGroupAutoGrow Kind) : FileGroupChange;
}

/// <summary>T-SQL: ALTER DATABASE File and Filegroups. Whether a filegroup may be written to.</summary>
public enum FileGroupUpdatability { ReadOnly, ReadWrite }

/// <summary>T-SQL: ALTER DATABASE File and Filegroups. Which files a filegroup grows.</summary>
public enum FileGroupAutoGrow { SingleFile, AllFiles }

/// <summary>T-SQL's own statements, in the standard's family.</summary>
public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: the DROP pages of the reference. Dropping something that is named and nothing more.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One record for the forty-six things whose published syntax is exactly
	/// <c>DROP &lt;words&gt; [IF EXISTS] name [,…n]</c> (proposal 18). A kind that publishes no
	/// <c>IF EXISTS</c> leaves the flag false, and one that publishes no list has a single name; a kind
	/// with anything else on its page is not here but has a record of its own, because the round trip
	/// has to give back what the page allows and no more.
	/// </para>
	/// <para>
	/// <see cref="RemoveProviderKey"/> is the one clause shared by enough kinds to be a field rather
	/// than a record: <c>SYMMETRIC KEY</c> and <c>ASYMMETRIC KEY</c>, both optional, the same words.
	/// </para>
	/// </remarks>
	public sealed record DropObject(
		DropObjectKind Kind,
		SqlList<QualifiedName> Names,
		bool IfExists = false,
		bool RemoveProviderKey = false) : Statement;

	/// <summary>T-SQL: DROP ASSEMBLY (Transact-SQL). <c>DROP ASSEMBLY [IF EXISTS] a [,…n] [WITH NO DEPENDENTS]</c>.</summary>
	public sealed record DropAssembly(SqlList<QualifiedName> Names, bool IfExists, bool NoDependents) : Statement;

	/// <summary>T-SQL: DROP EVENT SESSION (Transact-SQL). <c>DROP EVENT SESSION s ON { SERVER | DATABASE }</c>.</summary>
	/// <remarks>The scope is required on the page, so it is a property and not an option.</remarks>
	public sealed record DropEventSession(Identifier Name, EventScope On) : Statement;

	/// <summary>T-SQL: DROP EVENT NOTIFICATION (Transact-SQL). <c>DROP EVENT NOTIFICATION n [,…n] ON …</c>.</summary>
	public sealed record DropEventNotification(SqlList<Identifier> Names, NotificationScope On) : Statement;

	/// <summary>T-SQL: DROP EXTERNAL LIBRARY (Transact-SQL). <c>DROP EXTERNAL LIBRARY l [AUTHORIZATION owner]</c>.</summary>
	public sealed record DropExternalLibrary(Identifier Name, Identifier? Authorization) : Statement;

	/// <summary>T-SQL: DROP FULLTEXT INDEX (Transact-SQL). <c>DROP FULLTEXT INDEX ON table_name</c>.</summary>
	/// <remarks>It names no index: a table has one, and the target follows <c>ON</c>.</remarks>
	public sealed record DropFulltextIndex(QualifiedName Table) : Statement;

	/// <summary>T-SQL: DROP SENSITIVITY CLASSIFICATION (Transact-SQL). <c>DROP SENSITIVITY CLASSIFICATION FROM c [,…n]</c>.</summary>
	public sealed record DropSensitivityClassification(SqlList<QualifiedName> Objects) : Statement;

	/// <summary>T-SQL: DROP MASTER KEY (Transact-SQL). <c>DROP MASTER KEY</c>, which names nothing.</summary>
	public sealed record DropMasterKey : Statement;

	/// <summary>T-SQL: DROP DATABASE ENCRYPTION KEY (Transact-SQL). <c>DROP DATABASE ENCRYPTION KEY</c>, which names nothing.</summary>
	public sealed record DropDatabaseEncryptionKey : Statement;

	/// <summary>T-SQL: DROP INDEX (Transact-SQL), and DROP INDEX (Selective XML Indexes).</summary>
	/// <remarks>
	/// The page publishes a list of one form OR a list of the other, and this holds a list of either.
	/// That lets the tree hold a mixture the grammar will not read, which is the ordinary price of a
	/// family in a list; the alternative is two statements for one statement.
	/// </remarks>
	public sealed record DropIndex(SqlList<DroppedIndex> Indexes, bool IfExists) : Statement;

	/// <summary>T-SQL: DROP SIGNATURE (Transact-SQL). <c>DROP [COUNTER] SIGNATURE FROM m BY …</c>.</summary>
	public sealed record DropSignature(bool Counter, QualifiedName Module, SqlList<SignedBy> By) : Statement;

	// ---- cursors and transactions ---------------------------------------------------------------
	//
	// The standard has DECLARE, OPEN, FETCH and CLOSE, and the T-SQL words that go with them are
	// properties on CursorProperties and CursorSource.Query (SqlAst.cs). What is here is what the
	// standard has no statement for at all.

	/// <summary>T-SQL: DEALLOCATE (Transact-SQL). <c>DEALLOCATE { [GLOBAL] name | @variable }</c>.</summary>
	/// <remarks>
	/// The standard has no DEALLOCATE: a cursor there lives as long as the scope that declared it. Which
	/// of the two forms was written is <see cref="CursorReference"/>'s business, as it is for OPEN and
	/// CLOSE.
	/// </remarks>
	public sealed record DeallocateCursor(CursorReference Cursor) : Statement;

	/// <summary>
	/// T-SQL: SET @local_variable (Transact-SQL). <c>SET @c = CURSOR … FOR select</c>, a cursor defined
	/// where it stands and given to a variable.
	/// </summary>
	/// <remarks>
	/// The page publishes the same word list after CURSOR as the extended DECLARE, so the properties and
	/// the source are the same nodes; only the variable in front is this statement's own.
	/// <c>SET @c = @other</c> and <c>SET @c = name</c> are an ordinary assignment and not this.
	/// </remarks>
	public sealed record SetCursorVariable(
		Expression Variable, CursorProperties Properties, CursorSource SourceValue) : Statement;

	/// <summary>
	/// T-SQL: BEGIN TRANSACTION, BEGIN DISTRIBUTED TRANSACTION (Transact-SQL).
	/// <c>BEGIN [DISTRIBUTED] { TRAN | TRANSACTION } [ name [WITH MARK ['d']] ]</c>.
	/// </summary>
	/// <remarks>
	/// Not the standard's <see cref="StartTransaction"/>, which is the word START and a list of modes and
	/// names nothing: these two have a name, a mark and no modes at all (proposal 21). The distributed
	/// form takes no mark, which is the page's rule and not this record's.
	/// </remarks>
	public sealed record BeginTransaction(
		bool Distributed = false,
		[property: Spelling] bool Tran = false,
		TransactionName? Name = null,
		TransactionMark? Mark = null) : Statement;

	/// <summary>
	/// T-SQL: DENY (Transact-SQL). <c>DENY permission … [ON [class ::] securable] TO principal …
	/// [CASCADE] [AS principal]</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The third of the three permission statements, and the one the standard has no word for. It shares
	/// GRANT's parts — the permissions, the securable, the principals — so those are the standard's
	/// <see cref="Privilege"/>, <see cref="PrivilegeObject"/> and <see cref="Grantee"/> and not records of
	/// its own (proposal 23).
	/// </para>
	/// <para>
	/// It is a statement rather than a flag on <c>GrantBody.Privileges</c> because the page gives it a
	/// different tail: there is no <c>WITH GRANT OPTION</c> to leave false, and there is a
	/// <c>CASCADE</c> that GRANT does not have.
	/// </para>
	/// </remarks>
	public sealed record Deny(
		SqlList<Privilege> Items,
		SqlList<Grantee> Grantees,
		PrivilegeObject? Object = null,
		bool Cascade = false,
		Identifier? AsPrincipal = null) : Statement;

	/// <summary>
	/// T-SQL: SAVE TRANSACTION (Transact-SQL). <c>SAVE { TRAN | TRANSACTION } { name | @variable }</c>.
	/// </summary>
	/// <remarks>
	/// The standard's <see cref="Savepoint"/> is the word SAVEPOINT and takes an identifier; this is the
	/// word SAVE and takes a name or a variable, so it is a statement of its own (proposal 21).
	/// </remarks>
	public sealed record SaveTransaction(
		TransactionName Name, [property: Spelling] bool Tran = false) : Statement;
}

/// <summary>
/// T-SQL: BEGIN TRANSACTION, COMMIT TRANSACTION, ROLLBACK TRANSACTION, SAVE TRANSACTION (Transact-SQL).
/// A transaction or a savepoint named, which every one of those pages publishes as
/// <c>{ name | @variable }</c>.
/// </summary>
/// <remarks>
/// Exactly one of the two is set. It follows <see cref="CursorReference"/>, which holds the same either-or
/// for a cursor's name, rather than making an identifier pretend to be an expression.
/// </remarks>
public sealed record TransactionName(Identifier? Name, Expression? Variable = null) : ISqlNode
{
	/// <inheritdoc/>
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// T-SQL: BEGIN TRANSACTION (Transact-SQL). <c>WITH MARK ['description']</c>, which marks the log.
/// </summary>
/// <remarks>The description may be left out, so the record stands for the two words alone.</remarks>
public sealed record TransactionMark(Expression? Description = null) : ISqlNode
{
	/// <inheritdoc/>
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// T-SQL: COMMIT TRANSACTION, ROLLBACK TRANSACTION (Transact-SQL). The word after COMMIT or ROLLBACK,
/// where the page makes it optional.
/// </summary>
/// <remarks>
/// One enum rather than a flag for the word and another for its spelling, because that pair has a state
/// no text stands for — <c>TRAN</c> written with no keyword — and a tree that can hold what no text says
/// will eventually be asked to write it. Every value here is a text, so the writer is a switch and its
/// default throws. Where the page makes the word mandatory, as <c>BEGIN</c> and <c>SAVE</c> do, a
/// <c>[Spelling] bool Tran</c> is enough and is what those records carry.
/// </remarks>
/// <remarks>
/// The absence of the word is a null and not a <c>None</c> member: every optional enum in this tree
/// is nullable, so a <c>None</c> beside it would be a second way to say that nothing was written, for
/// a reader and a writer to keep in step. Hiding a zero-valued enum instead is what does not work:
/// <c>JoinKind.Cross</c>, <c>SetOperator.Union</c> and <c>DropBehavior.Cascade</c> are zero and are
/// all written words. So every value here is a text, which is what makes the writer a switch whose
/// default throws.
/// </remarks>
public enum TransactionKeyword { Tran, Transaction }

/// <summary>T-SQL: DECLARE CURSOR (Transact-SQL). <c>LOCAL</c> or <c>GLOBAL</c> after the word CURSOR.</summary>
/// <remarks>
/// Not <see cref="CursorReference"/>'s Global and Local, which qualify a cursor's <em>name</em> where it is
/// used — <c>OPEN GLOBAL c</c>. This says how the cursor was declared.
/// </remarks>
public enum CursorScope { Local, Global }

/// <summary>T-SQL: DECLARE CURSOR (Transact-SQL). The kind of cursor asked for.</summary>
/// <remarks>
/// A different axis from the standard's <see cref="CursorSensitivity"/>: <c>STATIC</c> is close to
/// INSENSITIVE and the other three have no standard word at all, so the two never stand for one another.
/// <c>FAST_FORWARD</c> is a kind the page publishes as one word, not a pair of the others.
/// </remarks>
public enum CursorType { Static, Keyset, Dynamic, FastForward }

/// <summary>T-SQL: DECLARE CURSOR (Transact-SQL). What may be done through the cursor while it is open.</summary>
/// <remarks>
/// <c>READ_ONLY</c> here is the concurrency of the extended form's word list, which is not the
/// <c>FOR READ ONLY</c> of <see cref="UpdatabilityClause"/>: a page may write both.
/// </remarks>
public enum CursorConcurrency { ReadOnly, ScrollLocks, Optimistic }

/// <summary>T-SQL: DROP INDEX (Transact-SQL). One index a <see cref="Statement.DropIndex"/> drops.</summary>
public abstract record DroppedIndex : ISqlNode
{
	/// <summary>Where in the text this index was named.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this index was named.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// <c>index_name ON object [ WITH ( … ) ]</c>: the relational, XML and spatial form.
	/// </summary>
	/// <remarks>
	/// The options are <c>MAXDOP</c>, <c>ONLINE</c>, <c>MOVE TO</c> and <c>FILESTREAM_ON</c>, which are
	/// names and values, so <see cref="Option"/> holds them: <c>MOVE TO scheme (column)</c> is a name
	/// with an invocation for its value and <c>"default"</c> is a name with a literal.
	/// </remarks>
	public sealed record Named(Identifier Name, QualifiedName On, SqlList<Option> Options) : DroppedIndex;

	/// <summary><c>[ owner. ] table_or_view.index_name</c>: the backward compatible form, which writes no <c>ON</c>.</summary>
	public sealed record Qualified(QualifiedName Name) : DroppedIndex;
}

/// <summary>T-SQL: DROP SIGNATURE (Transact-SQL). What a signature was made by.</summary>
public abstract record SignedBy : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>CERTIFICATE cert_name</c>.</summary>
	public sealed record Certificate(QualifiedName Name) : SignedBy;

	/// <summary><c>ASYMMETRIC KEY key_name</c>.</summary>
	public sealed record AsymmetricKey(QualifiedName Name) : SignedBy;
}

/// <summary>T-SQL: DROP EVENT SESSION (Transact-SQL). Where an event session lives.</summary>
public enum EventScope { Server, Database }

/// <summary>T-SQL: DROP TRIGGER (Transact-SQL). What a DDL trigger was created on.</summary>
public enum TriggerScope { Database, AllServer }

/// <summary>T-SQL: DROP EVENT NOTIFICATION (Transact-SQL). What an event notification was on.</summary>
/// <remarks>
/// A family rather than an enum with a name beside it, because the queue's name belongs to the one
/// alternative that has a queue and to no other.
/// </remarks>
public abstract record NotificationScope : ISqlNode
{
	/// <summary>Where in the text this scope was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this scope was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>ON SERVER</c>.</summary>
	public sealed record Server : NotificationScope;

	/// <summary><c>ON DATABASE</c>.</summary>
	public sealed record Database : NotificationScope;

	/// <summary><c>ON QUEUE queue_name</c>.</summary>
	public sealed record Queue(QualifiedName Name) : NotificationScope;
}

/// <summary>
/// T-SQL: the DROP pages of the reference. What a <see cref="Statement.DropObject"/> drops.
/// </summary>
/// <remarks>
/// The words the writer prints come from each page's SYNTAX line rather than its heading: the two agree
/// for forty-five of the forty-six, and for the forty-sixth the heading reads "DROP WORKLOAD Classifier"
/// where the syntax reads <c>DROP WORKLOAD CLASSIFIER</c>.
/// </remarks>
public enum DropObjectKind
{
	/// <summary><c>DROP AGGREGATE</c>.</summary>
	Aggregate,
	/// <summary><c>DROP APPLICATION ROLE</c>.</summary>
	ApplicationRole,
	/// <summary><c>DROP ASYMMETRIC KEY</c>, which takes <c>REMOVE PROVIDER KEY</c>.</summary>
	AsymmetricKey,
	/// <summary><c>DROP AVAILABILITY GROUP</c>.</summary>
	AvailabilityGroup,
	/// <summary><c>DROP BROKER PRIORITY</c>.</summary>
	BrokerPriority,
	/// <summary><c>DROP CERTIFICATE</c>.</summary>
	Certificate,
	/// <summary><c>DROP COLUMN ENCRYPTION KEY</c>.</summary>
	ColumnEncryptionKey,
	/// <summary><c>DROP COLUMN MASTER KEY</c>.</summary>
	ColumnMasterKey,
	/// <summary><c>DROP CONTRACT</c>.</summary>
	Contract,
	/// <summary><c>DROP CREDENTIAL</c>.</summary>
	Credential,
	/// <summary><c>DROP CRYPTOGRAPHIC PROVIDER</c>.</summary>
	CryptographicProvider,
	/// <summary><c>DROP DATABASE</c>.</summary>
	Database,
	/// <summary><c>DROP DATABASE AUDIT SPECIFICATION</c>.</summary>
	DatabaseAuditSpecification,
	/// <summary><c>DROP DATABASE SCOPED CREDENTIAL</c>.</summary>
	DatabaseScopedCredential,
	/// <summary><c>DROP DEFAULT</c>.</summary>
	Default,
	/// <summary><c>DROP ENDPOINT</c>.</summary>
	Endpoint,
	/// <summary><c>DROP EXTERNAL DATA SOURCE</c>.</summary>
	ExternalDataSource,
	/// <summary><c>DROP EXTERNAL FILE FORMAT</c>.</summary>
	ExternalFileFormat,
	/// <summary><c>DROP EXTERNAL LANGUAGE</c>.</summary>
	ExternalLanguage,
	/// <summary><c>DROP EXTERNAL MODEL</c>.</summary>
	ExternalModel,
	/// <summary><c>DROP EXTERNAL RESOURCE POOL</c>.</summary>
	ExternalResourcePool,
	/// <summary><c>DROP EXTERNAL TABLE</c>.</summary>
	ExternalTable,
	/// <summary><c>DROP FULLTEXT CATALOG</c>.</summary>
	FulltextCatalog,
	/// <summary><c>DROP FULLTEXT STOPLIST</c>.</summary>
	FulltextStoplist,
	/// <summary><c>DROP LOGIN</c>.</summary>
	Login,
	/// <summary><c>DROP MESSAGE TYPE</c>.</summary>
	MessageType,
	/// <summary><c>DROP PARTITION FUNCTION</c>.</summary>
	PartitionFunction,
	/// <summary><c>DROP PARTITION SCHEME</c>.</summary>
	PartitionScheme,
	/// <summary><c>DROP QUEUE</c>.</summary>
	Queue,
	/// <summary><c>DROP REMOTE SERVICE BINDING</c>.</summary>
	RemoteServiceBinding,
	/// <summary><c>DROP RESOURCE POOL</c>.</summary>
	ResourcePool,
	/// <summary><c>DROP ROUTE</c>.</summary>
	Route,
	/// <summary><c>DROP RULE</c>.</summary>
	Rule,
	/// <summary><c>DROP SEARCH PROPERTY LIST</c>.</summary>
	SearchPropertyList,
	/// <summary><c>DROP SECURITY POLICY</c>.</summary>
	SecurityPolicy,
	/// <summary><c>DROP SERVER AUDIT</c>.</summary>
	ServerAudit,
	/// <summary><c>DROP SERVER AUDIT SPECIFICATION</c>.</summary>
	ServerAuditSpecification,
	/// <summary><c>DROP SERVER ROLE</c>.</summary>
	ServerRole,
	/// <summary><c>DROP SERVICE</c>.</summary>
	Service,
	/// <summary><c>DROP STATISTICS</c>.</summary>
	Statistics,
	/// <summary><c>DROP SYMMETRIC KEY</c>, which takes <c>REMOVE PROVIDER KEY</c>.</summary>
	SymmetricKey,
	/// <summary><c>DROP SYNONYM</c>.</summary>
	Synonym,
	/// <summary><c>DROP USER</c>.</summary>
	User,
	/// <summary><c>DROP WORKLOAD CLASSIFIER</c>.</summary>
	WorkloadClassifier,
	/// <summary><c>DROP WORKLOAD GROUP</c>.</summary>
	WorkloadGroup,
	/// <summary><c>DROP XML SCHEMA COLLECTION</c>.</summary>
	XmlSchemaCollection,
}
