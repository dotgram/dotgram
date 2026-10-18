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
/// T-SQL: CREATE TABLE (Transact-SQL), CREATE INDEX (Transact-SQL), CREATE COLUMNSTORE INDEX (Transact-SQL). An index
/// written inside a table or on a column, <c>INDEX i [UNIQUE] [CLUSTERED | NONCLUSTERED] [COLUMNSTORE | HASH] (a ASC, …)
/// [ORDER (…)] [INCLUDE (…)] [WHERE …] [WITH (…)] [ON …] [FILESTREAM_ON …]</c>, or made by a statement of its own.
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

	/// <summary>
	/// T-SQL: CREATE COLUMNSTORE INDEX (Transact-SQL). A columnstore index's <c>ORDER (a, b)</c>: the columns its
	/// segments are sorted by.
	/// </summary>
	public SqlList<Identifier> Order { get; init; } = [];

	public SqlList<Identifier> Include { get; init; } = [];

	/// <summary>The filter, <c>WHERE …</c>.</summary>
	public Expression? Where { get; init; }

	public SqlList<Option> Options { get; init; } = [];

	/// <summary>
	/// T-SQL: CREATE INDEX (Transact-SQL). The options written without brackets, <c>WITH PAD_INDEX, FILLFACTOR = 80</c>:
	/// the old list CREATE INDEX still reads, each a word alone or FILLFACTOR with its number.
	/// </summary>
	[Spelling] public bool OptionsUnbracketed { get; init; }

	/// <summary>T-SQL: CREATE INDEX (Transact-SQL). <c>ON</c> and <c>FILESTREAM_ON</c>, in the order written.</summary>
	public SqlList<Placement> Placements { get; init; } = [];
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
	/// together, or one index alone; one column or one constraint added is the standard's <see cref="AddColumn"/>
	/// or <see cref="AddConstraint"/>.
	/// </summary>
	public sealed record Add(SqlList<TableElement> Elements) : AlterTableAction;

	/// <summary>
	/// T-SQL: ALTER TABLE (Transact-SQL). <c>ALTER INDEX i REBUILD WITH (…)</c>, an index of a memory-optimized table
	/// changed.
	/// </summary>
	public sealed record AlterIndex(Identifier Name, IndexAlteration Action) : AlterTableAction;

	/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>DROP INDEX i</c>, an index of a memory-optimized table dropped.</summary>
	public sealed record DropIndex(Identifier Name) : AlterTableAction;

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

// ── Indexes, full-text indexes, views, statistics, types, synonyms (task 5b) ─────────────────────────────────────────

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: CREATE INDEX (Transact-SQL), CREATE COLUMNSTORE INDEX (Transact-SQL). An index made on a table: a
	/// relational one, <c>CREATE [UNIQUE] [CLUSTERED | NONCLUSTERED] INDEX i ON t (…)</c>, or a columnstore one,
	/// <c>CREATE [CLUSTERED | NONCLUSTERED] COLUMNSTORE INDEX i ON t [(…)]</c>, which <see cref="IndexDefinition.Storage"/>
	/// says. The same index a table declares inside itself.
	/// </summary>
	public sealed record CreateIndex(QualifiedName Table, IndexDefinition Index) : Statement;

	/// <summary>
	/// T-SQL: CREATE XML INDEX (Transact-SQL). <c>CREATE [PRIMARY] XML INDEX i ON t (c) [USING XML INDEX p FOR
	/// {VALUE | PATH | PROPERTY}] [WITH (…)]</c>: a primary index, or a secondary one built over a primary.
	/// </summary>
	public sealed record CreateXmlIndex(QualifiedName Table, Identifier Name, Identifier Column) : Statement
	{
		/// <summary>T-SQL: CREATE XML INDEX (Transact-SQL). <c>PRIMARY</c>.</summary>
		public bool Primary { get; init; }

		/// <summary>T-SQL: CREATE XML INDEX (Transact-SQL). The primary index a secondary one is built over.</summary>
		public Identifier? Using { get; init; }

		/// <summary>T-SQL: CREATE XML INDEX (Transact-SQL). Which secondary index it is, said after <see cref="Using"/>.</summary>
		public XmlIndexFor? For { get; init; }

		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE SPATIAL INDEX (Transact-SQL). <c>CREATE SPATIAL INDEX i ON t (c) [USING GEOMETRY_GRID | …]
	/// [WITH (…)] [ON fg]</c>.
	/// </summary>
	public sealed record CreateSpatialIndex(QualifiedName Table, Identifier Name, Identifier Column) : Statement
	{
		/// <summary>T-SQL: CREATE SPATIAL INDEX (Transact-SQL). The tessellation scheme, <c>USING …</c>.</summary>
		public SpatialTessellation? Using { get; init; }

		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: CREATE SPATIAL INDEX (Transact-SQL). <c>ON</c> a filegroup.</summary>
		public SqlList<Placement> Placements { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER INDEX (Transact-SQL). <c>ALTER INDEX {i | ALL} ON t …</c>: one index, by <see cref="Name"/>, or all
	/// of a table's, by <see cref="All"/>, and never both or neither.
	/// </summary>
	public sealed record AlterIndex(QualifiedName Table, IndexAlteration Action) : Statement
	{
		public Identifier? Name { get; init; }

		/// <summary>T-SQL: ALTER INDEX (Transact-SQL). <c>ALL</c>: every index of the table.</summary>
		public bool All { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). <c>CREATE FULLTEXT INDEX ON t [(c …, …)] KEY INDEX k [ON {catalog |
	/// (catalog, FILEGROUP fg)}] [WITH …]</c>: the one index named by its table.
	/// </summary>
	public sealed record CreateFullTextIndex(QualifiedName Table, SqlList<FullTextIndexColumn> Columns, Identifier KeyIndex) : Statement
	{
		/// <summary>T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). The full-text catalog, <c>ON c</c>.</summary>
		public Identifier? Catalog { get; init; }

		/// <summary>T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). <c>FILEGROUP fg</c>, in the brackets after <c>ON</c>.</summary>
		public Identifier? FileGroup { get; init; }

		public SqlList<Option> Options { get; init; } = [];

		/// <summary>
		/// T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). <c>WITH CHANGE_TRACKING = AUTO</c>: the options written without
		/// their brackets, which the statement reads as well.
		/// </summary>
		[Spelling] public bool OptionsUnbracketed { get; init; }
	}

	/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>ALTER FULLTEXT INDEX ON t …</c>.</summary>
	public sealed record AlterFullTextIndex(QualifiedName Table, FullTextAlteration Action) : Statement;

	public partial record CreateView
	{
		/// <summary>
		/// T-SQL: CREATE VIEW (Transact-SQL), ALTER VIEW (Transact-SQL). <c>ALTER</c> or <c>CREATE OR ALTER</c>; null for
		/// the plain <c>CREATE</c>, the standard's, which the standard's parser builds and its tree tests print as no
		/// member at all. <see cref="DefinitionVerb.Create"/> would be the same text, and is refused.
		/// </summary>
		public DefinitionVerb? Verb { get; init; }

		/// <summary>
		/// T-SQL: CREATE VIEW (Transact-SQL). <c>WITH ENCRYPTION, SCHEMABINDING, VIEW_METADATA</c> before the
		/// <c>AS</c>, in the order written.
		/// </summary>
		public SqlList<ViewAttribute> Attributes { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE STATISTICS (Transact-SQL). <c>CREATE STATISTICS s ON t (a, …) [WHERE …] [WITH …]</c>.
	/// </summary>
	public sealed record CreateStatistics(QualifiedName Table, Identifier Name, SqlList<Identifier> Columns) : Statement
	{
		/// <summary>T-SQL: CREATE STATISTICS (Transact-SQL). The filter, <c>WHERE …</c>.</summary>
		public Expression? Where { get; init; }

		/// <summary>
		/// T-SQL: CREATE STATISTICS (Transact-SQL). <c>WITH FULLSCAN, SAMPLE 50 PERCENT, …</c>, written without
		/// brackets; <c>SAMPLE</c>'s value is a <see cref="Expression.Measured"/>.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: UPDATE STATISTICS (Transact-SQL). <c>UPDATE STATISTICS t [(s, …) | s] [WITH …]</c>.
	/// </summary>
	public sealed record UpdateStatistics(QualifiedName Table) : Statement
	{
		/// <summary>T-SQL: UPDATE STATISTICS (Transact-SQL). The statistics or indexes updated; none, all of them.</summary>
		public SqlList<Identifier> Names { get; init; } = [];

		/// <summary>
		/// T-SQL: UPDATE STATISTICS (Transact-SQL). <c>UPDATE STATISTICS t s</c>: one name written without the
		/// brackets, which the statement reads as well.
		/// </summary>
		[Spelling] public bool NameUnbracketed { get; init; }

		/// <summary>
		/// T-SQL: UPDATE STATISTICS (Transact-SQL). <c>WITH FULLSCAN, RESAMPLE ON PARTITIONS (1), …</c>, written
		/// without brackets.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE TYPE (Transact-SQL). <c>CREATE TYPE t AS TABLE (…) [WITH (MEMORY_OPTIMIZED = ON)]</c>: a type
	/// that is a table, its columns, constraints and indexes as a table's.
	/// </summary>
	public sealed record CreateTableType(QualifiedName Name, SqlList<TableElement> Elements) : Statement
	{
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE TYPE (Transact-SQL). <c>CREATE TYPE t FROM int [NULL | NOT NULL]</c>: a type made of another.
	/// </summary>
	public sealed record CreateAliasType(QualifiedName Name, DataType Base) : Statement
	{
		/// <summary>T-SQL: CREATE TYPE (Transact-SQL). <c>NULL</c> or <c>NOT NULL</c> after the type.</summary>
		public ColumnNullability? Nullability { get; init; }
	}

	/// <summary>T-SQL: CREATE SYNONYM (Transact-SQL). <c>CREATE SYNONYM s FOR t</c>: another name for an object.</summary>
	public sealed record CreateSynonym(QualifiedName Name, QualifiedName Target) : Statement;
}

/// <summary>T-SQL: CREATE XML INDEX (Transact-SQL). Which secondary XML index: <c>FOR VALUE</c>, <c>PATH</c>, <c>PROPERTY</c>.</summary>
public enum XmlIndexFor { Value, Path, Property }

/// <summary>
/// T-SQL: CREATE SPATIAL INDEX (Transact-SQL). <c>GEOMETRY_GRID</c>, <c>GEOGRAPHY_GRID</c>, <c>GEOMETRY_AUTO_GRID</c>,
/// <c>GEOGRAPHY_AUTO_GRID</c>.
/// </summary>
public enum SpatialTessellation { GeometryGrid, GeographyGrid, GeometryAutoGrid, GeographyAutoGrid }

/// <summary>T-SQL: ALTER INDEX (Transact-SQL). What an <c>ALTER INDEX</c> does to the index.</summary>
public abstract record IndexAlteration : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: ALTER INDEX (Transact-SQL). <c>REBUILD [PARTITION = n | PARTITION = ALL] [WITH (…)]</c>: one partition, all
	/// of them said so, or neither written.
	/// </summary>
	public sealed record Rebuild(Expression? Partition, bool AllPartitions, SqlList<Option> Options) : IndexAlteration;

	/// <summary>T-SQL: ALTER INDEX (Transact-SQL). <c>REORGANIZE [PARTITION = n] [WITH (…)]</c>.</summary>
	public sealed record Reorganize(Expression? Partition, SqlList<Option> Options) : IndexAlteration;

	/// <summary>T-SQL: ALTER INDEX (Transact-SQL). <c>DISABLE</c>.</summary>
	public sealed record Disable : IndexAlteration;

	/// <summary>T-SQL: ALTER INDEX (Transact-SQL). <c>SET (ALLOW_ROW_LOCKS = ON, …)</c>.</summary>
	public sealed record SetOptions(SqlList<Option> Options) : IndexAlteration;

	/// <summary>T-SQL: ALTER INDEX (Transact-SQL). <c>RESUME [WITH (…)]</c>, a resumable rebuild resumed.</summary>
	public sealed record Resume(SqlList<Option> Options) : IndexAlteration;

	/// <summary>T-SQL: ALTER INDEX (Transact-SQL). <c>PAUSE</c>.</summary>
	public sealed record Pause : IndexAlteration;

	/// <summary>T-SQL: ALTER INDEX (Transact-SQL). <c>ABORT</c>.</summary>
	public sealed record Abort : IndexAlteration;
}

/// <summary>
/// T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). One column of a full-text index: <c>c [TYPE COLUMN t] [LANGUAGE l]
/// [STATISTICAL_SEMANTICS]</c>.
/// </summary>
public sealed record FullTextIndexColumn(Identifier Column) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). <c>TYPE COLUMN t</c>: the column holding the document's type.</summary>
	public Identifier? TypeColumn { get; init; }

	/// <summary>T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). <c>LANGUAGE l</c>: a name, a number or a string.</summary>
	public Expression? Language { get; init; }

	/// <summary>T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). <c>STATISTICAL_SEMANTICS</c>.</summary>
	public bool StatisticalSemantics { get; init; }
}

/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). What an <c>ALTER FULLTEXT INDEX</c> does.</summary>
public abstract record FullTextAlteration : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>ENABLE</c>.</summary>
	public sealed record Enable : FullTextAlteration;

	/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>DISABLE</c>.</summary>
	public sealed record Disable : FullTextAlteration;

	/// <summary>
	/// T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>SET CHANGE_TRACKING [=] {MANUAL | AUTO | OFF}</c>, the word as
	/// written.
	/// </summary>
	public sealed record SetChangeTracking(Identifier Mode) : FullTextAlteration
	{
		/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>SET CHANGE_TRACKING AUTO</c>: the <c>=</c> left out.</summary>
		[Spelling] public bool EqualsOmitted { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>ADD (c …, …) [WITH NO POPULATION]</c>: columns added to the index.
	/// </summary>
	public sealed record AddColumns(SqlList<FullTextIndexColumn> Columns) : FullTextAlteration;

	/// <summary>
	/// T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>ALTER COLUMN c {ADD | DROP} STATISTICAL_SEMANTICS</c>: the column
	/// by its name alone.
	/// </summary>
	public sealed record AlterColumn(FullTextIndexColumn Column, bool Add) : FullTextAlteration;

	/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>DROP (c, …)</c>: columns dropped from the index.</summary>
	public sealed record DropColumns(SqlList<Identifier> Columns) : FullTextAlteration;

	/// <summary>
	/// T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>START {FULL | INCREMENTAL | UPDATE} POPULATION</c>, and <c>STOP</c>,
	/// <c>PAUSE</c> and <c>RESUME POPULATION</c>, which say no kind.
	/// </summary>
	public sealed record Population(PopulationAction Action, PopulationKind? Kind) : FullTextAlteration;

	/// <summary>
	/// T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>SET STOPLIST [=] {OFF | SYSTEM | s}</c>: one of the three.
	/// </summary>
	public sealed record SetStopList(Identifier? Name, bool Off, bool System) : FullTextAlteration
	{
		/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>SET STOPLIST s</c>: the <c>=</c> left out.</summary>
		[Spelling] public bool EqualsOmitted { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>SET SEARCH PROPERTY LIST [=] {OFF | p}</c>: one of the two.
	/// </summary>
	public sealed record SetSearchPropertyList(Identifier? Name, bool Off) : FullTextAlteration
	{
		/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). <c>SET SEARCH PROPERTY LIST p</c>: the <c>=</c> left out.</summary>
		[Spelling] public bool EqualsOmitted { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). The <c>WITH …</c> after the action, where the grammar reads one.
	/// </summary>
	public SqlList<Option> Options { get; init; } = [];
}

/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). What is done to a population.</summary>
public enum PopulationAction { Start, Stop, Pause, Resume }

/// <summary>T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). Which population is started.</summary>
public enum PopulationKind { Full, Incremental, Update }

/// <summary>
/// T-SQL: CREATE VIEW (Transact-SQL), ALTER VIEW (Transact-SQL). The verb a definition opens with.
/// </summary>
public enum DefinitionVerb { Create, Alter, CreateOrAlter }

/// <summary>T-SQL: CREATE VIEW (Transact-SQL). A view's attribute, <c>WITH ENCRYPTION</c> and its fellows.</summary>
public enum ViewAttribute { Encryption, SchemaBinding, ViewMetadata }

public abstract partial record SequenceOption
{
	/// <summary>T-SQL: CREATE SEQUENCE (Transact-SQL). <c>CACHE</c>, or <c>CACHE n</c>.</summary>
	public sealed record Cache(Expression? Size) : SequenceOption;

	/// <summary>T-SQL: CREATE SEQUENCE (Transact-SQL). <c>NO CACHE</c>.</summary>
	public sealed record NoCache : SequenceOption;
}
