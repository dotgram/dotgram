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
	public sealed record Pivot(TableSource Source, Expression.Invocation Aggregate, Identifier For, SqlList<Identifier> In, Alias? Alias = null) : TableSource;

	/// <summary>
	/// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). <c>t UNPIVOT (v FOR c IN (a, b)) AS u</c>: the
	/// source, the column the values go to, the column their names go to, and the columns unpivoted.
	/// </summary>
	public sealed record Unpivot(TableSource Source, Identifier Value, Identifier For, SqlList<Identifier> In, Alias? Alias = null) : TableSource;
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
