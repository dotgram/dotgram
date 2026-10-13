using System;

namespace DotGram.Sql.Ast;

// T-SQL's parts of the table statements (proposals 16, 17 and 20): what CREATE TABLE and ALTER TABLE add to the
// standard's records, a column's options in the order written, the index a table declares inside itself, and the
// actions only T-SQL's ALTER TABLE has.
//
// A word naming what is a record or a member of a family; a word saying how is an enum member or a flag. An option
// of a WITH (…) is an Option (proposal 20); where it is set to a key word — ON, OFF, PAGE, NONE, AUTO — the value is
// that word as a one-part Expression.Reference, as OPENROWSET BULK's options hold theirs, and never text.

public abstract partial record Statement
{
	public partial record CreateTable
	{
		/// <summary>
		/// T-SQL: CREATE TABLE (Transact-SQL), CREATE TABLE (SQL Graph). <c>AS FILETABLE</c>, written before the
		/// columns, or <c>AS NODE</c>, <c>AS EDGE</c>, written after them; null for a table of rows.
		/// </summary>
		public TableKind? Kind { get; init; }

		/// <summary>
		/// T-SQL: CREATE TABLE (Transact-SQL). <c>ON</c>, <c>TEXTIMAGE_ON</c> and <c>FILESTREAM_ON</c>, in the order
		/// written.
		/// </summary>
		public SqlList<Placement> Placements { get; init; } = [];

		/// <summary>
		/// T-SQL: CREATE TABLE (Transact-SQL), CREATE TABLE AS SELECT (Microsoft Fabric and Azure Synapse Analytics).
		/// The table's <c>WITH (…)</c>: after the placements of a table declared, before the <c>AS</c> of one made
		/// of a query. <c>SYSTEM_VERSIONING = ON (HISTORY_TABLE = h)</c> is one of them, an option with a value and
		/// options of its own; the standard's <see cref="SystemVersioning"/> stays false.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	public partial record AlterTable
	{
		/// <summary>
		/// T-SQL: ALTER TABLE (Transact-SQL). <c>WITH CHECK</c> or <c>WITH NOCHECK</c> before an <c>ADD</c> or a
		/// <c>CHECK CONSTRAINT</c>: whether the rows already there are held to what is added or switched on.
		/// </summary>
		public ConstraintCheck? Check { get; init; }
	}
}

/// <summary>T-SQL: CREATE TABLE (Transact-SQL), CREATE TABLE (SQL Graph). What kind of table a table is.</summary>
public enum TableKind { FileTable, Node, Edge }

/// <summary>
/// T-SQL: ALTER TABLE (Transact-SQL). <c>CHECK</c> or <c>NOCHECK</c>: before <c>ADD</c>, whether the rows there are
/// checked; before <c>CONSTRAINT</c>, whether the constraints are switched on or off.
/// </summary>
public enum ConstraintCheck { Check, NoCheck }

public sealed partial record ColumnDefinition
{
	/// <summary>
	/// T-SQL: column_definition (Transact-SQL), computed_column_definition (Transact-SQL). What is said about a column
	/// after its type, in the order it was said: a T-SQL column keeps everything here and leaves the standard's
	/// <see cref="Generation"/>, <see cref="Constraints"/> and <see cref="Collation"/> empty, and a standard
	/// column keeps none here.
	/// </summary>
	public SqlList<ColumnOption> Options { get; init; } = [];
}

/// <summary>
/// T-SQL: column_definition (Transact-SQL). One thing said about a column after its type, or in place of it for a
/// computed column.
/// </summary>
public abstract record ColumnOption : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: column_definition (Transact-SQL). <c>CONSTRAINT name</c> before a <see cref="Default"/> or a
	/// <see cref="Null"/>; a constraint's own name is the constraint's.
	/// </summary>
	public Identifier? Name { get; init; }

	/// <summary>T-SQL: column_definition (Transact-SQL). <c>DEFAULT value</c>.</summary>
	public sealed record Default(Expression Value) : ColumnOption;

	/// <summary>T-SQL: column_definition (Transact-SQL). <c>IDENTITY</c>, or <c>IDENTITY (seed, increment)</c>.</summary>
	public sealed record Identity(Expression? Seed, Expression? Increment) : ColumnOption;

	/// <summary>T-SQL: column_definition (Transact-SQL). <c>NOT FOR REPLICATION</c> after an identity.</summary>
	public sealed record NotForReplication : ColumnOption;

	/// <summary>T-SQL: column_definition (Transact-SQL). <c>COLLATE name</c>.</summary>
	public sealed record Collation(CollationName Value) : ColumnOption;

	/// <summary>
	/// T-SQL: column_constraint (Transact-SQL). <c>NOT NULL</c>, <c>PRIMARY KEY</c>, <c>UNIQUE</c>, <c>CHECK</c>,
	/// <c>REFERENCES</c>, a column's <c>FOREIGN KEY</c>.
	/// </summary>
	public sealed record Constraint(global::DotGram.Sql.Ast.Constraint Value) : ColumnOption;

	/// <summary>T-SQL: column_definition (Transact-SQL). An explicit <c>NULL</c>: the column may hold none.</summary>
	public sealed record Null : ColumnOption;

	/// <summary>
	/// T-SQL: computed_column_definition (Transact-SQL). <c>AS value</c>, written where the type would be, which
	/// the column then has none of.
	/// </summary>
	public sealed record Computed(Expression Value) : ColumnOption;

	/// <summary>T-SQL: computed_column_definition (Transact-SQL). <c>PERSISTED</c>.</summary>
	public sealed record Persisted : ColumnOption;

	/// <summary>T-SQL: column_definition (Transact-SQL). <c>SPARSE</c>.</summary>
	public sealed record Sparse : ColumnOption;

	/// <summary>T-SQL: column_definition (Transact-SQL). <c>FILESTREAM</c>.</summary>
	public sealed record Filestream : ColumnOption;

	/// <summary>T-SQL: column_definition (Transact-SQL). <c>ROWGUIDCOL</c>.</summary>
	public sealed record RowGuidCol : ColumnOption;

	/// <summary>
	/// T-SQL: column_definition (Transact-SQL). <c>HIDDEN</c>, wherever it was written: after
	/// <c>GENERATED ALWAYS AS ROW START</c> or apart from it.
	/// </summary>
	public sealed record Hidden : ColumnOption;

	/// <summary>
	/// T-SQL: column_definition (Transact-SQL). <c>MASKED WITH (FUNCTION = 'f()')</c>: the function, the string it
	/// is named in.
	/// </summary>
	public sealed record Masked(Expression Function) : ColumnOption;

	/// <summary>
	/// T-SQL: column_definition (Transact-SQL). <c>ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = k, ENCRYPTION_TYPE = …,
	/// ALGORITHM = '…')</c>.
	/// </summary>
	public sealed record Encrypted(SqlList<Option> Options) : ColumnOption;

	/// <summary>T-SQL: column_definition (Transact-SQL). <c>GENERATED ALWAYS AS ROW START</c> and its fellows.</summary>
	public sealed record GeneratedAlways(GeneratedAlwaysKind Kind) : ColumnOption;

	/// <summary>T-SQL: CREATE TABLE (Transact-SQL). <c>INDEX i …</c> written on the column.</summary>
	public sealed record Index(IndexDefinition Definition) : ColumnOption;
}

/// <summary>T-SQL: column_definition (Transact-SQL). What a <c>GENERATED ALWAYS AS</c> column holds.</summary>
public enum GeneratedAlwaysKind { RowStart, RowEnd, TransactionIdStart, TransactionIdEnd, SequenceNumberStart, SequenceNumberEnd }

public abstract partial record Constraint
{
	public partial record Unique
	{
		/// <summary>T-SQL: table_constraint (Transact-SQL). <c>CLUSTERED</c> or <c>NONCLUSTERED</c>.</summary>
		public IndexClustering? Clustering { get; init; }

		/// <summary>
		/// T-SQL: table_constraint (Transact-SQL). The key's columns, each with the direction written: T-SQL keeps
		/// them here and leaves <see cref="Columns"/> empty, whether a direction was written or not; null where no
		/// columns were written, a column's own key.
		/// </summary>
		public SqlList<SortItem>? Keys { get; init; }

		/// <summary>
		/// T-SQL: table_constraint (Transact-SQL), ALTER TABLE index_option (Transact-SQL). The key's <c>WITH (…)</c>,
		/// and the old <c>WITH FILLFACTOR = n</c> as an option.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>
		/// T-SQL: table_constraint (Transact-SQL). The options written without brackets, <c>WITH FILLFACTOR = 80</c>
		/// and <c>WITH SORTED_DATA</c>, which are a syntax of their own.
		/// </summary>
		[Spelling] public bool OptionsUnbracketed { get; init; }

		/// <summary>T-SQL: table_constraint (Transact-SQL). <c>ON</c> a filegroup or a partition scheme.</summary>
		public Placement? On { get; init; }
	}

	public partial record ForeignKey
	{
		/// <summary>
		/// T-SQL: column_constraint (Transact-SQL). <c>FOREIGN KEY REFERENCES …</c> on a column, which names no
		/// columns of its own and writes the words all the same.
		/// </summary>
		[Spelling] public bool ForeignKeyWords { get; init; }
	}

	public partial record Check
	{
		/// <summary>T-SQL: table_constraint (Transact-SQL). <c>CHECK NOT FOR REPLICATION (…)</c>.</summary>
		public bool NotForReplication { get; init; }
	}

	/// <summary>
	/// T-SQL: table_constraint (Transact-SQL). <c>CONSTRAINT d DEFAULT value FOR c</c>, a default written at the
	/// table for one of its columns.
	/// </summary>
	public sealed record Default(Expression Value, Identifier Column) : Constraint;

	/// <summary>
	/// T-SQL: CREATE TABLE (SQL Graph). <c>CONNECTION (a TO b, …) [ON DELETE CASCADE]</c>, which node tables an
	/// edge may join.
	/// </summary>
	public sealed record Connection(SqlList<EdgeConnection> Connections, bool OnDeleteCascade) : Constraint;
}

public sealed partial record ReferencesSpecification
{
	/// <summary>T-SQL: column_constraint (Transact-SQL), table_constraint (Transact-SQL). <c>NOT FOR REPLICATION</c>.</summary>
	public bool NotForReplication { get; init; }
}

/// <summary>T-SQL: table_constraint (Transact-SQL), CREATE INDEX (Transact-SQL). <c>CLUSTERED</c> or <c>NONCLUSTERED</c>.</summary>
public enum IndexClustering { Clustered, Nonclustered }

/// <summary>T-SQL: CREATE TABLE (Transact-SQL). How an index keeps its rows: <c>COLUMNSTORE</c>, <c>HASH</c>.</summary>
public enum IndexStorage { Columnstore, Hash }

/// <summary>
/// T-SQL: CREATE TABLE (Transact-SQL). An index written inside a table or on a column: <c>INDEX i [UNIQUE]
/// [CLUSTERED | NONCLUSTERED] [COLUMNSTORE | HASH] (a ASC, …) [INCLUDE (…)] [WHERE …] [WITH (…)] [ON …]</c>.
/// </summary>
public sealed record IndexDefinition(Identifier Name) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	public bool Unique { get; init; }
	public IndexClustering? Clustering { get; init; }
	public IndexStorage? Storage { get; init; }

	/// <summary>The key's columns, each with the direction written; none for a column's own index.</summary>
	public SqlList<SortItem> Columns { get; init; } = [];

	public SqlList<Identifier> Include { get; init; } = [];

	/// <summary>The filter, <c>WHERE …</c>.</summary>
	public Expression? Where { get; init; }

	public SqlList<Option> Options { get; init; } = [];
	public Placement? On { get; init; }
}

public abstract partial record TableElement
{
	/// <summary>T-SQL: CREATE TABLE (Transact-SQL). <c>INDEX i …</c> written at the table.</summary>
	public sealed record Index(IndexDefinition Definition) : TableElement;
}

public abstract partial record AlterTableAction
{
	public partial record DropColumn
	{
		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>DROP COLUMN IF EXISTS a</c>.</summary>
		public bool IfExists { get; init; }

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). The drop's <c>WITH (ONLINE = ON)</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	public partial record DropConstraint
	{
		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>DROP CONSTRAINT IF EXISTS c</c>.</summary>
		public bool IfExists { get; init; }

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). The drop's <c>WITH (ONLINE = ON, …)</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>
		/// T-SQL: ALTER TABLE (Transact-SQL). <c>DROP c</c>: the word left out, first in a list or after another
		/// constraint, where what is dropped is a constraint all the same.
		/// </summary>
		[Spelling] public bool ConstraintKeywordOmitted { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>ADD</c> with two elements or more, columns, constraints and indexes
	/// together; one element added is the standard's <see cref="AddColumn"/> or <see cref="AddConstraint"/>.
	/// </summary>
	public sealed record Add(SqlList<TableElement> Elements) : AlterTableAction;

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>DROP</c> with two items or more, each a <see cref="DropColumn"/> or a
	/// <see cref="DropConstraint"/>. A list is grouped by its words: <c>DROP COLUMN a, b, CONSTRAINT c, d</c> drops
	/// the columns <c>a</c> and <c>b</c> and the constraints <c>c</c> and <c>d</c>, so an item whose word is left
	/// out is of the kind of the item before it, and a first item without one is a constraint.
	/// </summary>
	public sealed record Drop(SqlList<AlterTableAction> Items) : AlterTableAction;

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>SWITCH [PARTITION n] TO t [PARTITION m] [WITH (…)]</c>: the rows moved
	/// to another table.
	/// </summary>
	public sealed record Switch(Expression? Partition, QualifiedName Target, Expression? TargetPartition, SqlList<Option> Options) : AlterTableAction;

	/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>SET (LOCK_ESCALATION = AUTO, …)</c>.</summary>
	public sealed record SetOptions(SqlList<Option> Options) : AlterTableAction;

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>REBUILD [PARTITION = n | PARTITION = ALL] [WITH (…)]</c>: one partition,
	/// all of them said so, or neither written.
	/// </summary>
	public sealed record Rebuild(Expression? Partition, bool All, SqlList<Option> Options) : AlterTableAction;

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>CHECK CONSTRAINT …</c> and <c>NOCHECK CONSTRAINT …</c>: the constraints
	/// named, or <c>ALL</c>, switched on or off.
	/// </summary>
	public sealed record CheckConstraints(ConstraintCheck Check, bool All, SqlList<Identifier> Names) : AlterTableAction;

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>ENABLE TRIGGER …</c> and <c>DISABLE TRIGGER …</c>: the triggers named, or
	/// <c>ALL</c>.
	/// </summary>
	public sealed record Triggers(bool Enable, bool All, SqlList<Identifier> Names) : AlterTableAction;

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>ENABLE CHANGE_TRACKING [WITH (TRACK_COLUMNS_UPDATED = ON)]</c> and
	/// <c>DISABLE CHANGE_TRACKING</c>.
	/// </summary>
	public sealed record ChangeTracking(bool Enable, SqlList<Option> Options) : AlterTableAction;
}

public abstract partial record AlterColumnAction
{
	public partial record SetDataType
	{
		/// <summary>
		/// T-SQL: ALTER TABLE (Transact-SQL). <c>ALTER COLUMN c INT</c>: the type written alone, without the
		/// standard's <c>SET DATA TYPE</c>.
		/// </summary>
		[Spelling] public bool SetDataTypeOmitted { get; init; }

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>COLLATE name</c> after the type.</summary>
		public CollationName? Collation { get; init; }

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>NULL</c> or <c>NOT NULL</c> after the type, last.</summary>
		public ColumnNullability? Nullability { get; init; }

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>WITH (ONLINE = ON)</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>ADD</c> or <c>DROP</c> one of a column's properties: <c>ADD ROWGUIDCOL</c>,
	/// <c>DROP PERSISTED</c>, <c>ADD MASKED WITH (FUNCTION = 'f()')</c>, which alone takes a function, and
	/// <c>DROP MASKED</c>.
	/// </summary>
	public sealed record Property(bool Add, ColumnPropertyKind Kind, Expression? MaskFunction, SqlList<Option> Options) : AlterColumnAction;
}

/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>NULL</c> or <c>NOT NULL</c> written after an altered column's type.</summary>
public enum ColumnNullability { Null, NotNull }

/// <summary>T-SQL: ALTER TABLE (Transact-SQL). A column property that <c>ALTER COLUMN … ADD</c> and <c>DROP</c> change.</summary>
public enum ColumnPropertyKind { RowGuidCol, Persisted, NotForReplication, Sparse, Hidden, Masked }
