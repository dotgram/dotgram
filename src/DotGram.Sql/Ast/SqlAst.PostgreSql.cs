using System;

namespace DotGram.Sql.Ast;

// PostgreSQL's own nodes and the members it adds to the standard's: what PostgreSQL reads and the standard does not,
// in the standard's families where it has one. Every record and member here carries `// PostgreSQL:` with the title of
// the page of the PostgreSQL manual it comes from, so a reader can find it there; a member a dialect reads from a
// shared record is documented for both.

public abstract partial record Expression
{
	/// <summary>
	/// PostgreSQL: Operators. An operator the tree has no core member for — <c>@&gt;</c>, <c>-&gt;</c>, <c>~</c>,
	/// a user's own — called on one operand or two, by its symbol, or by name with a schema:
	/// <c>a OPERATOR(pg_catalog.+) b</c>.
	/// </summary>
	/// <remarks>
	/// A symbol whose meaning the tree names — <c>+</c>, <c>||</c>, <c>=</c> — is built as that member
	/// (<see cref="Binary"/>, <see cref="Comparison"/>), never as this; the <c>OPERATOR(…)</c> form is always this,
	/// since it names a schema. <see cref="Left"/> is null for a prefix operator, <c>@ x</c>.
	/// </remarks>
	public sealed record OperatorCall(QualifiedName Operator, Expression? Left, Expression Right) : Expression
	{
		/// <summary>PostgreSQL: Operators. Written <c>OPERATOR(schema.op)</c> rather than as the bare symbol.</summary>
		public bool OperatorKeyword { get; init; }
	}

	/// <summary>
	/// PostgreSQL: Row and Array Comparisons. <c>a = ANY (array)</c>, <c>a LIKE ANY (SELECT …)</c>,
	/// <c>a OPERATOR(s.op) ALL (…)</c>: an operator with a quantifier over an array expression, or over a subquery
	/// with an operator the standard's quantified comparison does not take.
	/// </summary>
	/// <remarks>
	/// The standard's shape — a comparison operator over a subquery — is <see cref="QuantifiedComparison"/> in
	/// every dialect; this is every other shape, which PostgreSQL's grammar tells apart from it by the brackets and
	/// the operator. <see cref="Right"/> is the array expression, or an <see cref="Expression.Subquery"/>.
	/// </remarks>
	public sealed record QuantifiedOperation(Expression Left, QuantifiedOperator Operator, Quantifier Quantifier, Expression Right) : Expression;

	/// <summary>PostgreSQL: XML Predicates. <c>x IS [NOT] DOCUMENT</c>.</summary>
	public sealed record IsDocument(Expression Value, bool Not) : Expression;

	public sealed partial record Cast
	{
		/// <summary>
		/// PostgreSQL: Type Casts. Which of PostgreSQL's spellings was written: <c>x::t</c>, or <c>t 'text'</c>, a typed
		/// literal; null is <c>CAST(x AS t)</c>, the standard's.
		/// </summary>
		[Spelling] public CastSpelling? Spelling { get; init; }
	}

	public sealed partial record IsNull
	{
		/// <summary>PostgreSQL: Comparison Functions and Operators. Written <c>x ISNULL</c> or <c>x NOTNULL</c>, after the value.</summary>
		[Spelling] public bool Postfix { get; init; }
	}

	public sealed partial record Element
	{
		/// <summary>
		/// PostgreSQL: Subscripts. A slice written with a colon, <c>a[1:3]</c>, where the standard writes <c>TO</c>; a
		/// bound left out, <c>a[:3]</c>, <c>a[2:]</c>, is null.
		/// </summary>
		[Spelling] public bool Colon { get; init; }
	}

	public sealed partial record Extract
	{
		/// <summary>
		/// PostgreSQL: EXTRACT, date_part. A field the standard does not name — <c>epoch</c>, <c>dow</c>, <c>'year'</c> —
		/// as the name or the string written, where <c>Field</c> is null.
		/// </summary>
		public Expression? NamedField { get; init; }
	}

	public sealed partial record Substring
	{
		/// <summary>PostgreSQL: substring. Written <c>FOR b FROM a</c>, the count before the start.</summary>
		[Spelling] public bool ForFirst { get; init; }
	}
}

/// <summary>PostgreSQL: Type Casts. The spellings of a cast besides <c>CAST(x AS t)</c>.</summary>
public enum CastSpelling
{
	/// <summary><c>x::t</c>.</summary>
	DoubleColon,
	/// <summary><c>t 'text'</c>: a type before a string, which is that type's value.</summary>
	TypedLiteral,
}

/// <summary>
/// PostgreSQL: Row and Array Comparisons. The operator of a <see cref="Expression.QuantifiedOperation"/>: a
/// comparison, a pattern match, or any operator by its name.
/// </summary>
public abstract record QuantifiedOperator : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected QuantifiedOperator()
	{
	}

	/// <summary>A comparison operator, <c>=</c>, <c>&lt;&gt;</c> and the rest.</summary>
	public sealed record Comparison(ComparisonOperator Operator) : QuantifiedOperator;

	/// <summary><c>[NOT] LIKE</c> or <c>[NOT] ILIKE</c>.</summary>
	public sealed record Like(LikeKind Kind, bool Not) : QuantifiedOperator;

	/// <summary>Any other operator, by its symbol or as <c>OPERATOR(schema.op)</c>.</summary>
	public sealed record Named(QualifiedName Operator, bool OperatorKeyword = false) : QuantifiedOperator;
}

public sealed partial record Argument
{
	/// <summary>PostgreSQL: Function Calls. <c>VARIADIC arr</c>, an array spread over the variadic parameters.</summary>
	public bool Variadic { get; init; }

	/// <summary>PostgreSQL: Function Calls. A named argument written <c>name := value</c>, the older spelling of <c>name =&gt; value</c>.</summary>
	[Spelling] public bool ColonEquals { get; init; }
}

public abstract partial record TableSource
{
	public sealed partial record Function
	{
		/// <summary>PostgreSQL: SELECT (FROM Clause). <c>LATERAL</c> was written before the function.</summary>
		public bool Lateral { get; init; }

		/// <summary>PostgreSQL: SELECT (FROM Clause). <c>WITH ORDINALITY</c> after the function: a column numbering its rows.</summary>
		public bool WithOrdinality { get; init; }

		/// <summary>
		/// PostgreSQL: SELECT (FROM Clause). The column definition list after the alias, <c>f(x) AS t (a int, b text)</c>, for a
		/// function returning <c>record</c>; null where none was written. With one, <see cref="Alias"/> names no columns.
		/// </summary>
		public SqlList<FieldDefinition>? ColumnDefinitions { get; init; }
	}

	public sealed partial record Unnest
	{
		/// <summary>PostgreSQL: SELECT (FROM Clause). <c>LATERAL</c> was written before <c>unnest</c>, which may stand without it there.</summary>
		public bool Lateral { get; init; }
	}

	/// <summary>
	/// PostgreSQL: SELECT (FROM Clause). <c>ROWS FROM (f(x), g(y) AS (a int))</c>: several functions side by side, each row
	/// made of one row from each, with <c>WITH ORDINALITY</c> and an alias after the bracket.
	/// </summary>
	public sealed record RowsFrom(SqlList<RowsFromItem> Items, bool WithOrdinality, Alias? Alias = null) : TableSource
	{
		/// <summary>PostgreSQL: SELECT (FROM Clause). <c>LATERAL</c> was written before <c>ROWS FROM</c>.</summary>
		public bool Lateral { get; init; }

		/// <summary>PostgreSQL: SELECT (FROM Clause). The column definition list after the alias, <c>ROWS FROM (…) AS t (a int)</c>; null where none was written. With one, <see cref="Alias"/> names no columns.</summary>
		public SqlList<FieldDefinition>? ColumnDefinitions { get; init; }
	}

	public sealed partial record Named
	{
		/// <summary>PostgreSQL: FROM Clause. <c>t *</c>: the table and its descendants, which is what a bare name means too, said in so many words.</summary>
		[Spelling] public bool Star { get; init; }
	}

	public sealed partial record Parenthesized
	{
		/// <summary>PostgreSQL: FROM Clause. <c>(a JOIN b ON …) AS j</c>: the alias of a bracketed join.</summary>
		public Alias? Alias { get; init; }
	}
}

public abstract partial record Statement
{
	public sealed partial record Insert
	{
		/// <summary>PostgreSQL: INSERT. <c>INSERT INTO t AS a …</c>: the name the target goes by in ON CONFLICT and RETURNING.</summary>
		public Alias? Alias { get; init; }

		/// <summary>PostgreSQL: INSERT. The <c>ON CONFLICT</c> clauses, in order; one in PostgreSQL, a list because SQLite chains several.</summary>
		public SqlList<ConflictClause> Conflicts { get; init; } = [];
	}

	public sealed partial record Delete
	{
		/// <summary>PostgreSQL: DELETE. <c>USING t, u</c>: the tables joined to the target for the condition, which may not name the target again.</summary>
		public FromClause? Using { get; init; }
	}

	public sealed partial record TruncateTable
	{
		/// <summary>PostgreSQL: TRUNCATE. The tables after the first, <c>TRUNCATE a, b, ONLY c *</c>; empty where one was written.</summary>
		public SqlList<TableSource> Others { get; init; } = [];

		/// <summary>PostgreSQL: TRUNCATE. <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.</summary>
		public DropBehavior? Behavior { get; init; }

		/// <summary>PostgreSQL: TRUNCATE. Written <c>TRUNCATE t</c>, without the word TABLE.</summary>
		[Spelling] public bool TableOmitted { get; init; }
	}
}

/// <summary>PostgreSQL: INSERT. <c>ON CONFLICT [target] action</c>: what is done where a row would break a unique constraint.</summary>
public sealed record ConflictClause(ConflictTarget? Target, ConflictResolution Resolution) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>PostgreSQL: INSERT. Which constraint an ON CONFLICT means: the columns or expressions of an index, with its WHERE, or a constraint by name.</summary>
public abstract record ConflictTarget : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected ConflictTarget()
	{
	}

	/// <summary>
	/// PostgreSQL: INSERT. <c>(a, (b + 1), f(c) COLLATE "C" text_pattern_ops) WHERE …</c>: the keys of a unique index,
	/// each a column by its name, an expression in brackets or a call, with what an index's key may say beside it.
	/// </summary>
	public sealed record Columns(SqlList<IndexElement> Items, Expression? Where = null) : ConflictTarget;

	/// <summary>PostgreSQL: INSERT. <c>ON CONSTRAINT name</c>.</summary>
	public sealed record Constraint(Identifier Name) : ConflictTarget;
}

/// <summary>PostgreSQL: INSERT. What ON CONFLICT does: nothing, or an update of the row with its own WHERE.</summary>
public abstract record ConflictResolution : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected ConflictResolution()
	{
	}

	/// <summary>PostgreSQL: INSERT. <c>DO NOTHING</c>.</summary>
	public sealed record Nothing : ConflictResolution;

	/// <summary>PostgreSQL: INSERT. <c>DO UPDATE SET … [WHERE …]</c>; <c>EXCLUDED.c</c> in the assignments is a reference.</summary>
	public sealed record Update(SqlList<Assignment> Assignments, Expression? Where = null) : ConflictResolution;
}

public abstract partial record MergeMatchedAction
{
	/// <summary>PostgreSQL: MERGE. <c>WHEN MATCHED THEN DO NOTHING</c>, and the same after NOT MATCHED BY SOURCE.</summary>
	public sealed record DoNothing : MergeMatchedAction;
}

public sealed partial record MergeInsertAction
{
	/// <summary>PostgreSQL: MERGE. <c>WHEN NOT MATCHED THEN DO NOTHING</c>: no insert at all, and nothing else of the record.</summary>
	public bool DoNothing { get; init; }
}

public abstract partial record Query
{
	/// <summary>
	/// PostgreSQL: WITH Queries. A data change statement where a query stands — the body of a common table expression,
	/// <c>WITH moved AS (DELETE FROM t RETURNING *) SELECT * FROM moved</c>; its rows are what its RETURNING gives.
	/// </summary>
	public sealed record DataChange(Statement Change) : Query;

	public sealed partial record Select
	{
		/// <summary>PostgreSQL: DISTINCT ON. <c>SELECT DISTINCT ON (a, b) …</c>: the values the first row of each group of is kept by; empty where none was written.</summary>
		public SqlList<Expression> DistinctOn { get; init; } = [];
	}
}

public abstract partial record DataType
{
	public sealed partial record Array
	{
		/// <summary>PostgreSQL: Arrays. Written <c>t[n]</c> or <c>t[]</c>, where the standard writes <c>t ARRAY[n]</c>.</summary>
		[Spelling] public bool Brackets { get; init; }
	}

	public sealed partial record DateTime
	{
		/// <summary>PostgreSQL: Date/Time Types. Written <c>timestamptz</c> or <c>timetz</c>, the type with time zone in one word.</summary>
		[Spelling] public bool Abbreviated { get; init; }
	}

	public sealed partial record Boolean
	{
		/// <summary>PostgreSQL: Boolean Type. Written <c>bool</c>.</summary>
		[Spelling] public bool Abbreviated { get; init; }
	}
}

/// <summary>PostgreSQL: SELECT (FROM Clause). One function of a <c>ROWS FROM (…)</c>, with its column definition list where one was written.</summary>
public sealed record RowsFromItem(Expression.Invocation Invocation, SqlList<FieldDefinition>? ColumnDefinitions = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
