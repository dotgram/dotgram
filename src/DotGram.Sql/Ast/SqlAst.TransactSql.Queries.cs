using System;

namespace DotGram.Sql.Ast;

// T-SQL's own parts of a query: the members its SELECT adds to the standard's records, and the table
// sources it reads that the standard has no record for (proposals 1, 5, 6 and 9).

public abstract partial record Query
{
	public partial record Select
	{
		/// <summary>
		/// T-SQL: INTO Clause (Transact-SQL). <c>SELECT … INTO t [ON fg]</c>, which makes a table: not
		/// <see cref="Into"/>, the standard's variables.
		/// </summary>
		public SelectIntoTable? IntoTable { get; init; }
	}
}

public sealed partial record WithClause
{
	/// <summary>
	/// T-SQL: WITH XMLNAMESPACES (Transact-SQL). The namespaces a query's XML is written in, before its
	/// common table expressions: <c>WITH XMLNAMESPACES ('uri' AS ns), c AS (…)</c>.
	/// </summary>
	public SqlList<XmlNamespace> XmlNamespaces { get; init; } = [];

	/// <summary>
	/// T-SQL: WITH CHANGE_TRACKING_CONTEXT (Transact-SQL) (no syntax block in syntax.md). <c>WITH CHANGE_TRACKING_CONTEXT
	/// (0xff), c AS (…)</c>: the context a statement's changes are tracked with, before the common table expressions.
	/// </summary>
	public Expression? ChangeTrackingContext { get; init; }
}

/// <summary>
/// T-SQL: WITH XMLNAMESPACES (Transact-SQL). One namespace: <c>'uri' AS ns</c>, or the default,
/// <c>DEFAULT 'uri'</c>, where <see cref="Prefix"/> is null.
/// </summary>
public sealed record XmlNamespace(Expression Uri, Identifier? Prefix) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

public sealed partial record GroupByClause
{
	/// <summary>
	/// T-SQL: GROUP BY (Transact-SQL). <c>GROUP BY ALL</c>, which keeps the groups the WHERE left empty:
	/// not the standard's <see cref="Quantifier"/>, whose <c>ALL</c> changes nothing.
	/// </summary>
	public bool AllKeyword { get; init; }

	/// <summary>T-SQL: GROUP BY (Transact-SQL). <c>WITH CUBE</c> or <c>WITH ROLLUP</c> after the list.</summary>
	public GroupByWith? With { get; init; }
}

public abstract partial record GroupingElement
{
	public partial record Ordinary
	{
		/// <summary>
		/// T-SQL: GROUP BY (Transact-SQL), Azure Synapse's. <c>GROUP BY c WITH (DISTRIBUTED_AGG)</c>: the grouping column's
		/// rows are moved to be aggregated together, said after a column and not after a list in brackets.
		/// </summary>
		public bool DistributedAggregation { get; init; }
	}
}

/// <summary>T-SQL: GROUP BY (Transact-SQL). What the list's <c>WITH</c> adds.</summary>
public enum GroupByWith { Cube, Rollup }

public abstract partial record TableSource
{
	public partial record Named
	{
		/// <summary>T-SQL: Table Hints (Transact-SQL). <c>WITH (NOLOCK, INDEX (ix))</c>.</summary>
		public SqlList<TableHint> Hints { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>t PIVOT (SUM (a) FOR b IN ([1], [2])) AS p</c>:
	/// the source, the aggregate, the column after FOR whose values become columns, which may be qualified, and
	/// those values, written as names (the IN list): each a name of one part; the parser reads a prefix, which
	/// compilation refuses (Msg 254).
	/// </summary>
	public sealed record Pivot(TableSource Source, Expression.Invocation Aggregate, QualifiedName Column, SqlList<QualifiedName> Values, Alias Alias) : TableSource;

	/// <summary>
	/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>t UNPIVOT (v FOR c IN (a, b)) AS u</c>: the
	/// source, the column the values go to and the column after FOR their names go to — each a name of one part;
	/// the parser reads a prefix, which compilation refuses (Msg 275) — and the columns folded into
	/// <see cref="Value"/> (the IN list), which may be qualified.
	/// </summary>
	public sealed record Unpivot(TableSource Source, QualifiedName Value, QualifiedName Column, SqlList<QualifiedName> Columns, Alias Alias) : TableSource;
}

public sealed partial record SampleClause
{
	/// <summary>
	/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>TABLESAMPLE (10)</c> with its <c>SYSTEM</c> left out,
	/// which T-SQL allows and the standard's <see cref="Method"/> does not.
	/// </summary>
	[Spelling] public bool MethodOmitted { get; init; }

	/// <summary>
	/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>PERCENT</c> or <c>ROWS</c> after the sample number;
	/// null where neither was written, which is a percentage as the standard's is.
	/// </summary>
	public SampleUnit? Unit { get; init; }
}

public abstract partial record TableSource
{
	/// <summary>
	/// T-SQL: the ODBC outer join escape { oj … } (no syntax block in syntax.md).
	/// <c>{OJ t LEFT OUTER JOIN u ON …}</c>.
	/// </summary>
	public sealed record OdbcJoin(TableSource Source) : TableSource;
}

/// <summary>T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). What a sample's number counts.</summary>
public enum SampleUnit { Percent, Rows }

// T-SQL's rowset functions (proposal 7): each a table source of its own, since their arguments are not all
// values, with a WITH (…) schema as a list of RowsetColumn. A table-valued function whose arguments are
// values is TableSource.Function.
public abstract partial record TableSource
{
	public partial record Named
	{
		/// <summary>
		/// T-SQL: OPENDATASOURCE (Transact-SQL). <c>OPENDATASOURCE ('provider', 'init').db.s.t</c>: the server the
		/// name is looked up on, written before it.
		/// </summary>
		public OpenDataSource? DataSource { get; init; }
	}

	/// <summary>
	/// T-SQL: OPENJSON (Transact-SQL). <c>OPENJSON (json [, path]) [WITH (…)]</c>; <see cref="Schema"/> is null
	/// where no WITH was written.
	/// </summary>
	public sealed record OpenJson(Expression Json, Expression? Path, SqlList<RowsetColumn>? Schema, Alias? Alias = null) : TableSource;

	/// <summary>
	/// T-SQL: OPENXML (Transact-SQL). <c>OPENXML (idoc, rowpattern [, flags]) [WITH (…) | WITH table]</c>: a
	/// schema of columns, or a table whose columns are the schema, or neither.
	/// </summary>
	public sealed record OpenXml(Expression Document, Expression RowPattern, Expression? Flags, SqlList<RowsetColumn>? Schema, QualifiedName? SchemaTable, Alias? Alias = null) : TableSource;

	/// <summary>T-SQL: OPENQUERY (Transact-SQL). <c>OPENQUERY (server, 'query')</c>.</summary>
	public sealed record OpenQuery(Identifier Server, Expression Query, Alias? Alias = null) : TableSource;

	/// <summary>
	/// T-SQL: OPENROWSET (Transact-SQL). <c>OPENROWSET ('provider', 'datasource'; 'user'; 'password', object)</c>,
	/// or with a provider string in place of the three: exactly one of <see cref="ProviderString"/> and
	/// <see cref="Login"/> is set, and the writer refuses both or neither. <see cref="Object"/> is a name or a
	/// query's text.
	/// </summary>
	public sealed record OpenRowset(Expression Provider, Expression? ProviderString, RowsetLogin? Login, Expression Object, Alias? Alias = null) : TableSource;

	/// <summary>
	/// T-SQL: OPENROWSET BULK (Transact-SQL). <c>OPENROWSET (BULK 'file', SINGLE_BLOB, FORMAT = 'CSV') [WITH (…)]</c>:
	/// the file, the options in the order written, and the schema where one was written.
	/// </summary>
	public sealed record OpenRowsetBulk(Expression File, SqlList<Option> Options, SqlList<RowsetColumn>? Schema, Alias? Alias = null) : TableSource;

	/// <summary>
	/// T-SQL: PREDICT (Transact-SQL). <c>PREDICT (MODEL = m, DATA = t AS d [, RUNTIME = ONNX]) WITH (…)</c>: the
	/// model, the source scored with its alias, the runtime, and the columns the result adds — a name, a type, a
	/// collation and <c>NOT NULL</c>, as the standard's column definition holds them, with no path.
	/// </summary>
	/// <summary>
	/// T-SQL: AI_GENERATE_CHUNKS (Transact-SQL). <c>AI_GENERATE_CHUNKS (SOURCE = v, CHUNK_TYPE = FIXED, CHUNK_SIZE = v [,
	/// OVERLAP = v [, ENABLE_CHUNK_SET_ID = v]])</c>: the five named in that order, the first three demanded, the type
	/// the one bare word.
	/// </summary>
	public sealed record AiGenerateChunks(Expression Source, Identifier ChunkType, Expression ChunkSize) : TableSource
	{
		/// <summary>T-SQL: AI_GENERATE_CHUNKS (Transact-SQL). <c>OVERLAP = v</c>.</summary>
		public Expression? Overlap { get; init; }

		/// <summary>T-SQL: AI_GENERATE_CHUNKS (Transact-SQL). <c>ENABLE_CHUNK_SET_ID = v</c>, after OVERLAP and only with it.</summary>
		public Expression? EnableChunkSetId { get; init; }

		/// <summary>T-SQL: AI_GENERATE_CHUNKS (Transact-SQL). The correlation name, without a column list.</summary>
		public Alias? Alias { get; init; }
	}

	public sealed record Predict(Expression Model, TableSource Data, Identifier? Runtime, SqlList<ColumnDefinition> Schema, Alias? Alias = null) : TableSource
	{
		/// <summary>T-SQL: PREDICT (Transact-SQL). <c>PREDICT (DATA = …, MODEL = …)</c>: the data written before the model.</summary>
		[Spelling] public bool DataFirst { get; init; }
	}
}

// The change-tracking and full-text rowset functions, one record each, what they return being what differs.
public abstract partial record TableSource
{
	/// <summary>
	/// T-SQL: CHANGETABLE (Transact-SQL) (no syntax block in syntax.md). <c>CHANGETABLE (CHANGES t, @v [, FORCESEEK])</c>:
	/// the rows of the table changed since the version given.
	/// </summary>
	public sealed record ChangeTableChanges(QualifiedName Table, Expression LastSyncVersion, bool ForceSeek, Alias? Alias = null) : TableSource;

	/// <summary>
	/// T-SQL: CHANGETABLE (Transact-SQL) (no syntax block in syntax.md). <c>CHANGETABLE (VERSION t, (a, b), (1, 2)
	/// [, FORCESEEK])</c>: the latest change to the row whose key columns hold the values given.
	/// </summary>
	public sealed record ChangeTableVersion(QualifiedName Table, SqlList<Identifier> Columns, SqlList<Expression> Values, bool ForceSeek, Alias? Alias = null) : TableSource;

	/// <summary>
	/// T-SQL: CONTAINSTABLE (Transact-SQL) (no syntax block in syntax.md). <c>CONTAINSTABLE (t, (a, b), 'x' [, LANGUAGE
	/// 1033] [, 10])</c>: the table, the columns searched, what is searched for, the language and how many rows. A
	/// column is an <see cref="Expression.Reference"/> of one part, <c>*</c> an <see cref="Expression.Asterisk"/>, and
	/// <c>PROPERTY (c, 'p')</c> an invocation, written alone and never in brackets.
	/// </summary>
	public sealed record ContainsTable(QualifiedName Table, SqlList<Expression> Columns, Expression Search, Expression? Language, Expression? Top, Alias? Alias = null) : TableSource
	{
		/// <summary>
		/// T-SQL: CONTAINSTABLE (Transact-SQL) (no syntax block in syntax.md). <c>(c)</c> and <c>(*)</c> against <c>c</c>
		/// and <c>*</c>; always set for two columns or more, which the writer refuses unbracketed.
		/// </summary>
		[Spelling] public bool ColumnsBracketed { get; init; }
	}

	/// <summary>
	/// T-SQL: FREETEXTTABLE (Transact-SQL) (no syntax block in syntax.md). <c>FREETEXTTABLE (t, (a, b), 'x' [, LANGUAGE
	/// 1033] [, 10])</c>: as <see cref="ContainsTable"/>, its members the same.
	/// </summary>
	public sealed record FreetextTable(QualifiedName Table, SqlList<Expression> Columns, Expression Search, Expression? Language, Expression? Top, Alias? Alias = null) : TableSource
	{
		/// <summary>T-SQL: FREETEXTTABLE (Transact-SQL) (no syntax block in syntax.md). As <see cref="ContainsTable.ColumnsBracketed"/>.</summary>
		[Spelling] public bool ColumnsBracketed { get; init; }
	}
}

/// <summary>
/// T-SQL: OPENDATASOURCE (Transact-SQL). <c>OPENDATASOURCE ('provider', 'init')</c>: the provider and the string it
/// is initialised with.
/// </summary>
public sealed record OpenDataSource(Expression Provider, Expression InitString) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// T-SQL: OPENROWSET (Transact-SQL). <c>'datasource'; 'user'; 'password'</c>: where OPENROWSET connects, and as whom.
/// </summary>
public sealed record RowsetLogin(Expression DataSource, Expression User, Expression Password) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

public abstract partial record SystemTimeSpecification
{
	/// <summary>T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>FOR SYSTEM_TIME CONTAINED IN (a, b)</c>.</summary>
	public sealed record ContainedIn(Expression From, Expression To) : SystemTimeSpecification;

	/// <summary>T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>FOR SYSTEM_TIME ALL</c>.</summary>
	public sealed record All : SystemTimeSpecification;
}

// Variables where a query names a table or assigns a value (docs/design/sql-tsql-tree.md, Decided: a variable
// is never a QualifiedName, and a family gains a T-SQL member Variable for it), and the graph path an aggregate
// is taken over.
public abstract partial record TableSource
{
	/// <summary>
	/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). A table variable as a source, <c>FROM @t AS x</c>; the
	/// identifier keeps its <c>@</c>. In a FROM the engine takes neither hints nor a sample on it, and the writer
	/// refuses both there; a data change statement's target may have hints.
	/// </summary>
	public sealed record Variable(Identifier Name) : TableSource
	{
		public Alias? Alias { get; init; }

		/// <summary>T-SQL: Table Hints (Transact-SQL).</summary>
		public SqlList<TableHint> Hints { get; init; } = [];
	}

	public partial record Function
	{
		/// <summary>
		/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (Transact-SQL). <c>FROM @v.f (1) AS t (c)</c>: the variable of a
		/// user-defined type whose table-valued method the invocation calls, the identifier keeping its <c>@</c>; the
		/// invocation's name is the method's, of one part.
		/// </summary>
		// `new`: TableSource.Variable, the table variable as a source, is another node, which this name hides inside
		// Function.
		public new Identifier? Variable { get; init; }
	}
}

public abstract partial record SelectColumn
{
	public partial record ExpressionItem
	{
		/// <summary>
		/// T-SQL: SELECT Clause (Transact-SQL). <c>x = a</c> against <c>a AS x</c>: the alias written before the value,
		/// the same column.
		/// </summary>
		[Spelling] public bool AliasFirst { get; init; }
	}

	/// <summary>
	/// T-SQL: SELECT @local_variable (Transact-SQL). <c>SELECT @v = e</c>, <c>SELECT @v += e</c>: a value taken into a
	/// variable rather than returned; the identifier keeps its <c>@</c>.
	/// </summary>
	public sealed record VariableAssignment(Identifier Variable, AssignmentOperator Operator, Expression Value) : SelectColumn;
}

/// <summary>
/// T-SQL: SELECT @local_variable (Transact-SQL). An assignment's operator: <c>=</c>, <c>+=</c>, <c>-=</c>, <c>*=</c>,
/// <c>/=</c>, <c>%=</c>, <c>&amp;=</c>, <c>|=</c>, <c>^=</c>, and SQL Server 2025's <c>||=</c>.
/// </summary>
public enum AssignmentOperator { Assign, Add, Subtract, Multiply, Divide, Modulo, BitwiseAnd, BitwiseOr, BitwiseXor, Concatenate }

public abstract partial record Expression
{
	public partial record Invocation
	{
		/// <summary>
		/// T-SQL: SHORTEST_PATH (Transact-SQL). <c>WITHIN GROUP (GRAPH PATH)</c>: the aggregate is taken over the
		/// nodes of an arbitrary-length MATCH pattern, in path order.
		/// </summary>
		public bool GraphPath { get; init; }
	}
}

// T-SQL's graph pattern (proposal 15: a graph pattern is a family of its own), the operand of MATCH, which is a
// drawing rather than an expression; and FOR PATH, which marks a source read as the nodes or edges of a path of
// any length.
public abstract partial record Expression
{
	/// <summary>
	/// T-SQL: MATCH (SQL Graph). <c>MATCH (a-(e)->b AND LAST_NODE (p) = LAST_NODE (q))</c>: the patterns
	/// <c>AND</c> joins, in order.
	/// </summary>
	public sealed record GraphMatch(SqlList<GraphPattern> Patterns) : Expression;
}

public abstract partial record TableSource
{
	public partial record Named
	{
		/// <summary>
		/// T-SQL: MATCH (SQL Graph). <c>FOR PATH</c>, after the table's version and before its alias: the table is
		/// read as the nodes or edges of an arbitrary-length pattern.
		/// </summary>
		public bool ForPath { get; init; }
	}

	public partial record Subquery
	{
		/// <summary>T-SQL: MATCH (SQL Graph). <c>FOR PATH</c> after the bracketed query, before its alias.</summary>
		public bool ForPath { get; init; }
	}
}

/// <summary>T-SQL: MATCH (SQL Graph). One pattern of a MATCH.</summary>
public abstract record GraphPattern : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>a-(e)->b&lt;-(f)-c</c>: a node, then each edge and the node it reaches; either end may be <c>LAST_NODE (x)</c>.</summary>
	public sealed record Chain(GraphNode Start, SqlList<GraphStep> Steps) : GraphPattern;

	/// <summary><c>LAST_NODE (a) = LAST_NODE (b)</c>: the last nodes of two paths, compared.</summary>
	public sealed record LastNodes(Identifier Left, Identifier Right) : GraphPattern;

	/// <summary>
	/// <c>SHORTEST_PATH (n(-(e)->m)+)</c>, the anchor first and each step written edge then node; or, where
	/// <see cref="AnchorLast"/>, <c>SHORTEST_PATH ((m&lt;-(e)-)+n)</c>, each step written node then edge and the anchor
	/// after the quantifier. A step's node is never <c>LAST_NODE</c>; the anchor may be.
	/// </summary>
	public sealed record ShortestPath(GraphNode Anchor, SqlList<GraphStep> Steps, GraphQuantifier Quantifier, bool AnchorLast) : GraphPattern;
}

/// <summary>T-SQL: MATCH (SQL Graph). A node: the alias or one-part name of a node table, or <c>LAST_NODE (x)</c>.</summary>
public sealed record GraphNode(Identifier Alias, bool LastNode) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: MATCH (SQL Graph). An edge and the node it reaches.</summary>
public sealed record GraphStep(GraphEdge Edge, GraphNode Node) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: MATCH (SQL Graph). An edge, <c>-(e)-></c> or <c>&lt;-(e)-</c>.</summary>
public sealed record GraphEdge(Identifier Alias, GraphDirection Direction) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: MATCH (SQL Graph). Which way an edge points: <c>-(e)-></c> is <see cref="Right"/>, <c>&lt;-(e)-</c> <see cref="Left"/>.</summary>
public enum GraphDirection { Right, Left }

/// <summary>
/// T-SQL: MATCH (SQL Graph). How many times <c>SHORTEST_PATH</c> repeats its steps: <c>{1,3}</c>, <c>{1,}</c> with
/// no most, or <c>+</c>, which is <c>{1,}</c> written short (<see cref="Plus"/>; the writer refuses it with any other
/// bounds).
/// </summary>
public sealed record GraphQuantifier(int Min, int? Max, [property: Spelling] bool Plus) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
