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
	/// the source, the aggregate, the column whose values become columns, and those values as names.
	/// </summary>
	public sealed record Pivot(TableSource Source, Expression.Invocation Aggregate, Identifier For, SqlList<Identifier> In, Alias Alias) : TableSource;

	/// <summary>
	/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>t UNPIVOT (v FOR c IN (a, b)) AS u</c>: the
	/// source, the column the values go to, the column their names go to, and the columns unpivoted.
	/// </summary>
	public sealed record Unpivot(TableSource Source, Identifier Value, Identifier For, SqlList<Identifier> In, Alias Alias) : TableSource;
}

public abstract partial record TableSource
{
	public partial record Named
	{
		/// <summary>
		/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>TABLESAMPLE (10)</c> with its <c>SYSTEM</c>
		/// left out, which T-SQL allows and the standard's <see cref="SampleClause.Method"/> does not.
		/// </summary>
		[Spelling] public bool SampleMethodOmitted { get; init; }

		/// <summary>
		/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>PERCENT</c> or <c>ROWS</c> after the sample
		/// number; null where neither was written, which is a percentage as the standard's is.
		/// </summary>
		public SampleUnit? SampleUnit { get; init; }
	}

	/// <summary>T-SQL: ODBC Scalar Functions (Transact-SQL). An ODBC outer join escape, <c>{OJ t LEFT OUTER JOIN u ON …}</c>.</summary>
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
	/// or with a provider string in place of the three. <see cref="Connection"/> holds one item, the provider string,
	/// or three, the data source, the user and the password; the writer refuses any other count. <see cref="Object"/>
	/// is a name or a query's text.
	/// </summary>
	public sealed record OpenRowset(Expression Provider, SqlList<Expression> Connection, Expression Object, Alias? Alias = null) : TableSource;

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
	public sealed record Predict(Expression Model, TableSource Data, Identifier? Runtime, SqlList<ColumnDefinition> Schema, Alias? Alias = null) : TableSource;
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
}

public abstract partial record SelectColumn
{
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
