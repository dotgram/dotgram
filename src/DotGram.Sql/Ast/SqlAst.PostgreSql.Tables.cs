using System;

namespace DotGram.Sql.Ast;

// PostgreSQL's parts of the table statements: what CREATE TABLE adds to the standard's records — the partitions, the
// inheritance, the storage, the exclusion constraint — and the key column an index, an exclusion constraint, a
// partition key and ON CONFLICT share. Every record and member here carries `PostgreSQL:` with the title of the page of
// the PostgreSQL manual it comes from.

public sealed partial record ParameterDefinition
{
	/// <summary>PostgreSQL: CREATE FUNCTION. <c>IN OUT</c> written in two words, with Mode InOut.</summary>
	[Spelling] public bool InOutTwoWords { get; init; }
}

/// <summary>
/// PostgreSQL: CREATE INDEX, CREATE TABLE, INSERT. One key of an index, of an exclusion constraint, of a partition key or
/// of an ON CONFLICT target: a column by its name, a call, or an expression in brackets, with its collation, its
/// operator class and that class's options, its direction and where its nulls sort.
/// </summary>
/// <remarks>
/// T-SQL's index keys are elements too, with a column for <see cref="Key"/>, a direction at most, and nothing else;
/// a key ORDER BY sorts by is a <see cref="SortItem"/>, which has no collation or operator class.
/// <see cref="OperatorClassOptions"/> is empty where none were written, and none can be without an operator class.
/// </remarks>
public sealed record IndexElement(Expression Key, CollationName? Collation = null, QualifiedName? OperatorClass = null, SqlList<Option> OperatorClassOptions = default, SortDirection? Direction = null, NullOrdering? Nulls = null) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

public abstract partial record Statement
{
	public sealed partial record CreateTable
	{
		/// <summary>PostgreSQL: CREATE TABLE. <c>UNLOGGED</c>: a table whose rows are not written to the write-ahead log.</summary>
		public bool Unlogged { get; init; }

		/// <summary>PostgreSQL: CREATE TABLE. <c>IF NOT EXISTS</c>.</summary>
		public bool IfNotExists { get; init; }

		/// <summary>PostgreSQL: CREATE TABLE. <c>TEMP</c> written for <c>TEMPORARY</c>, with or without GLOBAL or LOCAL.</summary>
		[Spelling] public bool TempWord { get; init; }

		/// <summary>
		/// PostgreSQL: CREATE TABLE. <c>PARTITION OF parent FOR VALUES …</c>: the table is a partition of another, its
		/// columns the parent's and <see cref="Contents"/> the elements that add to them, often none.
		/// </summary>
		public PartitionParent? PartitionOf { get; init; }

		/// <summary>PostgreSQL: CREATE TABLE. <c>PARTITION BY RANGE (…)</c>: the table is partitioned, by the strategy and the keys.</summary>
		public PartitionSpecification? PartitionBy { get; init; }

		/// <summary>PostgreSQL: CREATE TABLE. <c>INHERITS (a, b)</c>: the tables whose columns this one inherits.</summary>
		public SqlList<QualifiedName> Inherits { get; init; } = [];

		/// <summary>PostgreSQL: CREATE TABLE. <c>USING method</c>: the table access method.</summary>
		public Identifier? AccessMethod { get; init; }

		/// <summary>PostgreSQL: CREATE TABLE. <c>WITHOUT OIDS</c>, which says what the default says.</summary>
		[Spelling] public bool WithoutOids { get; init; }

		/// <summary>PostgreSQL: CREATE TABLE. <c>TABLESPACE name</c>.</summary>
		public Identifier? Tablespace { get; init; }
	}
}

public sealed partial record ColumnDefinition
{
	/// <summary>PostgreSQL: CREATE TABLE. <c>STORAGE PLAIN</c> and its fellows, or <c>STORAGE DEFAULT</c>, the word DEFAULT as the name.</summary>
	public Identifier? Storage { get; init; }

	/// <summary>PostgreSQL: CREATE TABLE. <c>COMPRESSION lz4</c>, or <c>COMPRESSION DEFAULT</c>, the word DEFAULT as the name.</summary>
	public Identifier? Compression { get; init; }
}

public abstract partial record ColumnGeneration
{
	/// <summary>
	/// PostgreSQL: CREATE TABLE. <c>CONSTRAINT n DEFAULT 1</c>, <c>CONSTRAINT n GENERATED …</c>: the name gram.y reads before a
	/// default or a generation, as before a constraint; null where none was written.
	/// </summary>
	public QualifiedName? Name { get; init; }

	public sealed partial record Generated
	{
		/// <summary>PostgreSQL: CREATE TABLE. <c>STORED</c> or <c>VIRTUAL</c> after the expression; null where neither was written.</summary>
		public GenerationStorage? Storage { get; init; }
	}
}

/// <summary>PostgreSQL: CREATE TABLE. Whether a generated column's value is kept with the row or computed when it is read.</summary>
public enum GenerationStorage { Stored, Virtual }

public sealed partial record ColumnOptionList
{
	/// <summary>PostgreSQL: CREATE TABLE. A typed table's or a partition's column's <c>COLLATE c</c>.</summary>
	public CollationName? Collation { get; init; }
}

public abstract partial record TableElement
{
	public sealed partial record ColumnOptions
	{
		/// <summary>
		/// PostgreSQL: CREATE TABLE. The column's options written without the words <c>WITH OPTIONS</c>, which the standard
		/// requires and PostgreSQL leaves to the writer.
		/// </summary>
		[Spelling] public bool WithOptionsOmitted { get; init; }
	}
}

public abstract partial record Constraint
{
	/// <summary>
	/// PostgreSQL: CREATE TABLE, ALTER TABLE. <c>NO INHERIT</c>: a CHECK or NOT NULL constraint the table's children do not
	/// take.
	/// </summary>
	public bool NoInherit { get; init; }

	/// <summary>PostgreSQL: ALTER TABLE, CREATE TABLE. <c>NOT VALID</c>: the rows already there are not checked.</summary>
	public bool NotValid { get; init; }

	public sealed partial record NotNull
	{
		/// <summary>
		/// PostgreSQL: CREATE TABLE. <c>NOT NULL c</c> written at the table, naming its column; null for a column's own.
		/// </summary>
		public Identifier? Column { get; init; }
	}

	/// <summary>PostgreSQL: CREATE TABLE. An explicit <c>NULL</c> on a column: the column may hold none.</summary>
	public sealed record Null : Constraint;

	public sealed partial record Unique
	{
		/// <summary>PostgreSQL: CREATE TABLE. <c>INCLUDE (c, …)</c>: the columns the key's index holds beside the key.</summary>
		public SqlList<Identifier> Include { get; init; } = [];

		/// <summary>PostgreSQL: CREATE TABLE. <c>USING INDEX TABLESPACE name</c>: where the key's index is kept.</summary>
		public Identifier? IndexTablespace { get; init; }

		/// <summary>
		/// PostgreSQL: ALTER TABLE. <c>UNIQUE USING INDEX i</c>, <c>PRIMARY KEY USING INDEX i</c>: an index already there made
		/// the key, which then names no columns.
		/// </summary>
		public Identifier? ExistingIndex { get; init; }
	}

	/// <summary>
	/// PostgreSQL: CREATE TABLE. <c>EXCLUDE [USING method] (key WITH operator, …) [INCLUDE (…)] [WITH (…)] [USING INDEX
	/// TABLESPACE t] [WHERE (…)]</c>: no two rows whose keys the operators all answer true for.
	/// </summary>
	public sealed record Exclude(Identifier? Method, SqlList<ExclusionElement> Elements, Expression? Where = null) : Constraint
	{
		/// <summary>PostgreSQL: CREATE TABLE. <c>INCLUDE (c, …)</c>: the columns the constraint's index holds beside its keys.</summary>
		public SqlList<Identifier> Include { get; init; } = [];

		/// <summary>PostgreSQL: CREATE TABLE. The index's storage parameters, <c>WITH (fillfactor = 70)</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>PostgreSQL: CREATE TABLE. <c>USING INDEX TABLESPACE name</c>: where the constraint's index is kept.</summary>
		public Identifier? IndexTablespace { get; init; }
	}
}

public sealed partial record ReferencesSpecification
{
	/// <summary>
	/// PostgreSQL: CREATE TABLE. <c>ON DELETE SET NULL (a, b)</c> or <c>SET DEFAULT (a, b)</c>: the columns the action sets,
	/// where not all of the key's; empty where none were named. ON UPDATE names none: the server's grammar refuses them there.
	/// </summary>
	public SqlList<Identifier> OnDeleteColumns { get; init; } = [];
}

public abstract partial record SequenceOption
{
	public sealed partial record Start
	{
		/// <summary>PostgreSQL: CREATE SEQUENCE. <c>START 1</c>, the word WITH left out.</summary>
		[Spelling] public bool WithOmitted { get; init; }
	}

	public sealed partial record Increment
	{
		/// <summary>PostgreSQL: CREATE SEQUENCE. <c>INCREMENT 1</c>, the word BY left out.</summary>
		[Spelling] public bool ByOmitted { get; init; }
	}

	public sealed partial record Restart
	{
		/// <summary>PostgreSQL: ALTER SEQUENCE. <c>RESTART 1</c>, the word WITH left out before the value.</summary>
		[Spelling] public bool WithOmitted { get; init; }
	}

	/// <summary>PostgreSQL: CREATE SEQUENCE. <c>OWNED BY t.c</c>, the column the sequence goes with; <c>OWNED BY NONE</c> is the name none.</summary>
	public sealed record OwnedBy(QualifiedName Column) : SequenceOption;

	/// <summary>PostgreSQL: CREATE SEQUENCE. <c>LOGGED</c>, or <c>UNLOGGED</c> where the value is false.</summary>
	public sealed record Logged(bool Value) : SequenceOption;

	/// <summary>PostgreSQL: CREATE TABLE. <c>SEQUENCE NAME s</c> in an identity column's options: the name of the sequence it makes.</summary>
	public sealed record SequenceName(QualifiedName Name) : SequenceOption;
}

/// <summary>PostgreSQL: SELECT INTO. <c>SELECT … INTO [TEMP | UNLOGGED] [TABLE] t</c>, which makes a table as CREATE TABLE AS does.</summary>
public sealed partial record SelectIntoTable
{
	/// <summary>PostgreSQL: SELECT INTO. <c>TEMPORARY</c> or <c>TEMP</c>, alone or after GLOBAL or LOCAL; null for a persistent table.</summary>
	public TableScope? Scope { get; init; }

	/// <summary>PostgreSQL: SELECT INTO. <c>TEMP</c> written for <c>TEMPORARY</c>.</summary>
	[Spelling] public bool TempWord { get; init; }

	/// <summary>PostgreSQL: SELECT INTO. <c>UNLOGGED</c>.</summary>
	public bool Unlogged { get; init; }

	/// <summary>PostgreSQL: SELECT INTO. The word <c>TABLE</c> written before the name.</summary>
	[Spelling] public bool TableKeyword { get; init; }
}

/// <summary>PostgreSQL: CREATE TABLE. <c>PARTITION OF parent FOR VALUES …</c>: the parent, and the rows the partition holds.</summary>
public sealed record PartitionParent(QualifiedName Parent, PartitionBound Bound) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>PostgreSQL: CREATE TABLE. A partition's bound: the values it holds, a range of them, a remainder of a hash, or the rest.</summary>
public abstract record PartitionBound : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected PartitionBound()
	{
	}

	/// <summary>PostgreSQL: CREATE TABLE. <c>FOR VALUES IN (1, 2)</c>.</summary>
	public sealed record In(SqlList<Expression> Values) : PartitionBound;

	/// <summary>
	/// PostgreSQL: CREATE TABLE. <c>FOR VALUES FROM (1, MINVALUE) TO (10, MAXVALUE)</c>; MINVALUE and MAXVALUE are the names
	/// they are written as.
	/// </summary>
	public sealed record Range(SqlList<Expression> From, SqlList<Expression> To) : PartitionBound;

	/// <summary>PostgreSQL: CREATE TABLE. <c>FOR VALUES WITH (MODULUS 4, REMAINDER 0)</c>, each a name and its number.</summary>
	public sealed record Hash(SqlList<Option> Options) : PartitionBound;

	/// <summary>PostgreSQL: CREATE TABLE. <c>DEFAULT</c>: the rows no other partition holds.</summary>
	public sealed record Default : PartitionBound;
}

/// <summary>
/// PostgreSQL: CREATE TABLE. <c>PARTITION BY RANGE (a, (b + 1), lower(c) COLLATE "C" text_ops)</c>: the strategy, as the
/// name written, and the keys, which have no direction.
/// </summary>
public sealed record PartitionSpecification(Identifier Strategy, SqlList<IndexElement> Keys) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// PostgreSQL: CREATE TABLE. One key of an EXCLUDE constraint and its operator, <c>c WITH &amp;&amp;</c>, or
/// <c>c WITH OPERATOR(s.=)</c> where <see cref="OperatorKeyword"/> says so.
/// </summary>
public sealed record ExclusionElement(IndexElement Element, QualifiedName Operator, bool OperatorKeyword = false) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
