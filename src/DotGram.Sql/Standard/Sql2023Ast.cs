using System;
using System.Collections.Generic;

namespace DotGram.Sql.Ast;

/// <summary>
/// What every node is. It introduces no class inheritance level: every polymorphic family is an
/// independent one-level hierarchy (Statement -> Statement.Select, Expression -> Expression.Binary).
/// </summary>
/// <remarks>
/// Where a node was written is <see cref="ISqlSpan"/>, as in the tree built today: a family's abstract
/// record keeps the span for all its members, and a record of no family keeps its own. It is a property
/// of the node, not a base class.
/// </remarks>
public interface ISqlNode : ISqlSpan;

// Text is the identifier as written, its quotes included. UnicodeEscape is `U&"a" UESCAPE '!'`'s character.
public sealed record Identifier(string Text, IdentifierStyle Style = IdentifierStyle.Regular, char? UnicodeEscape = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum IdentifierStyle { Regular, Delimited, UnicodeDelimited }

/// <summary>BNF: top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families.</summary>
public abstract record Statement : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	// BNF: <query expression>, <query specification>, <select statement: single row>, <dynamic single row select statement>
	public record Select : Statement
	{
		public WithClause? With { get; init; }
		public SetQuantifier? Quantifier { get; init; }
		public IReadOnlyList<SelectItem> Items { get; init; } = [];
		public IntoClause? Into { get; init; }
		public FromClause? From { get; init; }
		public Expression? Where { get; init; }
		public GroupByClause? GroupBy { get; init; }
		public Expression? Having { get; init; }
		public WindowClause? Window { get; init; }
		public IReadOnlyList<SetOperation> SetOperations { get; init; } = [];
		public OrderByClause? OrderBy { get; init; }
		public OffsetClause? Offset { get; init; }
		public FetchClause? Fetch { get; init; }
		// BNF: a <query expression body> that is no <query specification> — `VALUES (1)`, `TABLE t`, or a
		// query in brackets — as the first operand. The specification's own properties are then empty.
		public QueryOperand? Body { get; init; }

		// How many pairs of brackets were written around the whole query: `((SELECT 1))` is two.
		public int Parentheses { get; init; }

		// BNF: <updatability clause> of a <cursor specification>, `FOR UPDATE OF a`.
		public UpdatabilityClause? Updatability { get; init; }
	}

	// BNF: <insert statement>
	public record Insert : Statement
	{
		public required TableSource Target { get; init; }
		public IReadOnlyList<Identifier> Columns { get; init; } = [];
		public OverrideKind? Override { get; init; }
		public InsertSource SourceValue { get; init; } = new InsertSource.DefaultValues();
	}

	// BNF: <update statement: searched>, <update statement: positioned>, dynamic/preparable positioned update
	public record Update : Statement
	{
		// Null in a preparable positioned statement, which may leave its table out.
		public TableSource? Target { get; init; }
		public PeriodPortion? Portion { get; init; }
		public Alias? Alias { get; init; }
		public IReadOnlyList<Assignment> Assignments { get; init; } = [];
		public Expression? Where { get; init; }
		public CursorReference? CurrentOf { get; init; }
	}

	// BNF: <delete statement: searched>, <delete statement: positioned>, dynamic/preparable positioned delete
	public record Delete : Statement
	{
		// Null in a preparable positioned statement, which may leave its table out.
		public TableSource? Target { get; init; }
		public PeriodPortion? Portion { get; init; }
		public Alias? Alias { get; init; }
		public Expression? Where { get; init; }
		public CursorReference? CurrentOf { get; init; }
	}

	// BNF: <merge statement>
	public record Merge : Statement
	{
		public required TableSource Target { get; init; }
		public Alias? Alias { get; init; }
		public required TableSource SourceTable { get; init; }
		public required Expression On { get; init; }
		public IReadOnlyList<MergeClause> Clauses { get; init; } = [];
	}

	// BNF: <truncate table statement>
	public record TruncateTable : Statement
	{
		public required TableSource Target { get; init; }
		public IdentityRestart? Identity { get; init; }
	}

	// BNF: <schema definition>
	public record CreateSchema : Statement
	{
		public QualifiedName? Name { get; init; }
		// BNF: <schema character set or path>, written either way round.
		public bool PathFirst { get; init; }
		public AuthorizationIdentifier? Authorization { get; init; }
		public CharacterSetName? DefaultCharacterSet { get; init; }
		public PathSpecification? Path { get; init; }
		public IReadOnlyList<Statement> Elements { get; init; } = [];
	}

	// BNF: <table definition>
	public record CreateTable : Statement
	{
		public TableScope? Scope { get; init; }
		public required QualifiedName Name { get; init; }
		public required TableContents Contents { get; init; }
		public bool SystemVersioning { get; init; }
		public TableCommitAction? OnCommit { get; init; }
	}

	// BNF: <alter table statement>
	public record AlterTable : Statement
	{
		public required QualifiedName Name { get; init; }
		// T-SQL: several actions in one statement; the standard's is a list of one (design/sql-tsql-tree.md, 17).
		public IReadOnlyList<AlterTableAction> Actions { get; init; } = [];
	}

	// BNF: <drop schema statement>. T-SQL's drops name a list, say IF EXISTS, and may leave the behavior out (design/sql-tsql-tree.md).
	public record DropSchema : Statement { public IReadOnlyList<QualifiedName> Names { get; init; } = []; public bool IfExists { get; init; } public DropBehavior? Behavior { get; init; } }

	// BNF: <drop table statement>
	public record DropTable : Statement { public IReadOnlyList<QualifiedName> Names { get; init; } = []; public bool IfExists { get; init; } public DropBehavior? Behavior { get; init; } }

	// BNF: <view definition>
	public record CreateView : Statement
	{
		public bool Recursive { get; init; }
		public required QualifiedName Name { get; init; }
		public ViewSpecification? Specification { get; init; }
		public required Select Query { get; init; }
		public CheckOption? CheckOption { get; init; }
	}

	// BNF: <drop view statement>
	public record DropView : Statement { public IReadOnlyList<QualifiedName> Names { get; init; } = []; public bool IfExists { get; init; } public DropBehavior? Behavior { get; init; } }

	// BNF: <domain definition>, <alter domain statement>, <drop domain statement>
	public record CreateDomain : Statement { public required QualifiedName Name { get; init; } public bool AsKeyword { get; init; } public required DataType Type { get; init; } public Expression? Default { get; init; } public IReadOnlyList<Constraint> Constraints { get; init; } = []; public CollationName? Collation { get; init; } }
	public record AlterDomain : Statement { public required QualifiedName Name { get; init; } public required AlterDomainAction Action { get; init; } }
	public record DropDomain : Statement { public required QualifiedName Name { get; init; } public required DropBehavior Behavior { get; init; } }

	// BNF: character set/collation/translation/assertion statements
	public record CreateCharacterSet : Statement { public required CharacterSetName Name { get; init; } public bool AsKeyword { get; init; } public required CharacterSetName Source { get; init; } public CollationName? Collation { get; init; } }
	public record DropCharacterSet : Statement { public required CharacterSetName Name { get; init; } }
	public record CreateCollation : Statement { public required CollationName Name { get; init; } public required CharacterSetName CharacterSet { get; init; } public required CollationName Source { get; init; } public PadCharacteristic? Padding { get; init; } }
	public record DropCollation : Statement { public required CollationName Name { get; init; } public required DropBehavior Behavior { get; init; } }
	public record CreateTranslation : Statement { public required QualifiedName Name { get; init; } public required CharacterSetName SourceCharacterSet { get; init; } public required CharacterSetName TargetCharacterSet { get; init; } public required TranslationSource Source { get; init; } }
	public record DropTranslation : Statement { public required QualifiedName Name { get; init; } }
	public record CreateAssertion : Statement { public required QualifiedName Name { get; init; } public required Expression Condition { get; init; } public ConstraintCharacteristics? Characteristics { get; init; } }
	public record DropAssertion : Statement { public required QualifiedName Name { get; init; } public DropBehavior? Behavior { get; init; } }

	// BNF: trigger definition/drop trigger
	public record CreateTrigger : Statement
	{
		public required QualifiedName Name { get; init; }
		public required TriggerTime Time { get; init; }
		public required TriggerEvent Event { get; init; }
		public required QualifiedName Table { get; init; }
		public IReadOnlyList<TransitionReference> Referencing { get; init; } = [];
		public required TriggerAction Action { get; init; }
	}
	public record DropTrigger : Statement { public IReadOnlyList<QualifiedName> Names { get; init; } = []; public bool IfExists { get; init; } }

	// BNF: user-defined type/cast/ordering/transform statements
	public record CreateType : Statement { public required UserDefinedTypeDefinition Definition { get; init; } }
	public record AlterType : Statement { public required QualifiedName Name { get; init; } public required AlterTypeAction Action { get; init; } }
	public record DropType : Statement { public IReadOnlyList<QualifiedName> Names { get; init; } = []; public bool IfExists { get; init; } public DropBehavior? Behavior { get; init; } }
	public record CreateCast : Statement { public required DataType SourceType { get; init; } public required DataType TargetType { get; init; } public required RoutineDesignator Function { get; init; } public bool AsAssignment { get; init; } }
	public record DropCast : Statement { public required DataType SourceType { get; init; } public required DataType TargetType { get; init; } public required DropBehavior Behavior { get; init; } }
	public record CreateOrdering : Statement { public required QualifiedName TypeName { get; init; } public required OrderingDefinition Ordering { get; init; } }
	public record DropOrdering : Statement { public required QualifiedName TypeName { get; init; } public required DropBehavior Behavior { get; init; } }
	public record CreateTransform : Statement { public bool PluralKeyword { get; init; } public required QualifiedName TypeName { get; init; } public IReadOnlyList<TransformGroup> Groups { get; init; } = []; }
	public record AlterTransform : Statement { public bool PluralKeyword { get; init; } public required QualifiedName TypeName { get; init; } public IReadOnlyList<TransformAlterGroup> Groups { get; init; } = []; }
	public record DropTransform : Statement { public bool PluralKeyword { get; init; } public required TransformDropTarget Target { get; init; } public required QualifiedName TypeName { get; init; } public required DropBehavior Behavior { get; init; } }

	// BNF: schema routine, alter/drop routine
	public record CreateRoutine : Statement { public required RoutineDefinition Definition { get; init; } }
	public record AlterRoutine : Statement { public required RoutineDesignator Routine { get; init; } public IReadOnlyList<RoutineCharacteristic> Characteristics { get; init; } = []; public required AlterRoutineBehavior Behavior { get; init; } }
	public record DropRoutine : Statement { public IReadOnlyList<RoutineDesignator> Routines { get; init; } = []; public bool IfExists { get; init; } public DropBehavior? Behavior { get; init; } }

	// BNF: sequence generator definition/alter/drop
	public record CreateSequence : Statement { public required QualifiedName Name { get; init; } public IReadOnlyList<SequenceOption> Options { get; init; } = []; }
	public record AlterSequence : Statement { public required QualifiedName Name { get; init; } public IReadOnlyList<SequenceOption> Options { get; init; } = []; }
	public record DropSequence : Statement { public IReadOnlyList<QualifiedName> Names { get; init; } = []; public bool IfExists { get; init; } public DropBehavior? Behavior { get; init; } }

	// BNF: GRANT/REVOKE/ROLE
	public record Grant : Statement { public required GrantBody Body { get; init; } }
	public record Revoke : Statement { public required RevokeBody Body { get; init; } }
	public record CreateRole : Statement { public required Identifier Name { get; init; } public Grantor? Admin { get; init; } }
	public record DropRole : Statement { public IReadOnlyList<Identifier> Names { get; init; } = []; public bool IfExists { get; init; } public DropBehavior? Behavior { get; init; } }

	// BNF: cursor/data control statements
	public record DeclareCursor : Statement { public required CursorReference Cursor { get; init; } public required CursorProperties Properties { get; init; } public required CursorSource SourceValue { get; init; } }
	public record OpenCursor : Statement { public required CursorReference Cursor { get; init; } public DynamicArguments? Using { get; init; } }
	// BNF: <fetch statement>, <dynamic fetch statement>: targets, or `INTO [SQL] DESCRIPTOR d`, and whether FROM was written.
	public record FetchCursor : Statement { public FetchOrientation? Orientation { get; init; } public bool FromKeyword { get; init; } public required CursorReference Cursor { get; init; } public DynamicArguments? Into { get; init; } }
	public record CloseCursor : Statement { public required CursorReference Cursor { get; init; } }
	public record AllocateCursor : Statement { public required CursorReference Cursor { get; init; } public bool CursorKeyword { get; init; } public CursorProperties? Properties { get; init; } public required CursorAllocationSource SourceValue { get; init; } }

	// BNF: <temporary table declaration>.
	public record DeclareLocalTemporaryTable : Statement { public required QualifiedName Name { get; init; } public IReadOnlyList<TableElement> Elements { get; init; } = []; public TableCommitAction? OnCommit { get; init; } }

	// BNF: <get diagnostics statement>.
	public record GetDiagnostics : Statement { public required DiagnosticsInformation Information { get; init; } }

	public record FreeLocator : Statement { public IReadOnlyList<Expression> Locators { get; init; } = []; }
	public record HoldLocator : Statement { public IReadOnlyList<Expression> Locators { get; init; } = []; }
	public record Call : Statement { public required Expression.Invocation Invocation { get; init; } }
	public record Return : Statement { public Expression? Value { get; init; } public bool NullKeyword { get; init; } }

	// BNF: transaction statements
	public record StartTransaction : Statement { public IReadOnlyList<TransactionMode> Modes { get; init; } = []; }
	public record SetTransaction : Statement { public bool Local { get; init; } public IReadOnlyList<TransactionMode> Modes { get; init; } = []; }
	public record SetConstraints : Statement { public ConstraintTarget Target { get; init; } = new ConstraintTarget.All(); public required ConstraintTiming Timing { get; init; } }
	public record Savepoint : Statement { public required Identifier Name { get; init; } }
	public record ReleaseSavepoint : Statement { public required Identifier Name { get; init; } }
	public record Commit : Statement { public bool Work { get; init; } public ChainMode? Chain { get; init; } }
	public record Rollback : Statement { public bool Work { get; init; } public ChainMode? Chain { get; init; } public Identifier? ToSavepoint { get; init; } }

	// BNF: connection/session statements
	public record Connect : Statement { public required ConnectionTarget Target { get; init; } }
	public record SetConnection : Statement { public required ConnectionObject Connection { get; init; } }
	public record Disconnect : Statement { public required DisconnectObject Object { get; init; } }
	public record SetSessionAuthorization : Statement { public required Expression Value { get; init; } }
	public record SetRole : Statement { public Expression? Value { get; init; } public bool None { get; init; } }
	public record SetTimeZone : Statement { public Expression? Value { get; init; } public bool Local { get; init; } }
	public record SetCatalog : Statement { public required Expression Value { get; init; } }
	public record SetSchema : Statement { public required Expression Value { get; init; } }
	public record SetNames : Statement { public required Expression Value { get; init; } }
	public record SetPath : Statement { public required Expression Value { get; init; } }
	public record SetTransformGroup : Statement { public required TransformGroupCharacteristic Value { get; init; } }
	public record SetCollation : Statement { public bool NoCollation { get; init; } public Expression? Value { get; init; } public IReadOnlyList<CharacterSetName> ForCharacterSets { get; init; } = []; }
	// BNF: <session characteristic list>: one list of modes per `TRANSACTION ...` written.
	public record SetSessionCharacteristics : Statement { public IReadOnlyList<IReadOnlyList<TransactionMode>> Characteristics { get; init; } = []; }

	// BNF: diagnostics/descriptor/dynamic SQL
	public record AllocateDescriptor : Statement { public bool SqlKeyword { get; init; } public required DescriptorReference Descriptor { get; init; } public Expression? Max { get; init; } }
	public record DeallocateDescriptor : Statement { public bool SqlKeyword { get; init; } public required DescriptorReference Descriptor { get; init; } }
	public record GetDescriptor : Statement { public bool SqlKeyword { get; init; } public required DescriptorReference Descriptor { get; init; } public required DescriptorGet Body { get; init; } }
	public record SetDescriptor : Statement { public bool SqlKeyword { get; init; } public required DescriptorReference Descriptor { get; init; } public required DescriptorSet Body { get; init; } }
	public record CopyDescriptor : Statement { public required DescriptorCopy Body { get; init; } }
	public record Prepare : Statement { public required StatementReference Statement { get; init; } public Expression? Attributes { get; init; } public required Expression Sql { get; init; } }
	public record DeallocatePrepare : Statement { public required StatementReference Statement { get; init; } }
	public record Describe : Statement { public required DescribeBody Body { get; init; } }
	public record Execute : Statement { public required StatementReference Statement { get; init; } public DynamicArguments? Result { get; init; } public DynamicArguments? Parameters { get; init; } }
	public record ExecuteImmediate : Statement { public required Expression Sql { get; init; } }
	public record PipeRow : Statement { public required DescriptorReference Descriptor { get; init; } }

	// BNF: embedded WHENEVER and implementation-defined preparable statement escape hatch
	public record Whenever : Statement { public required SqlCondition Condition { get; init; } public required ConditionAction Action { get; init; } }
	public record Extension : Statement { public required string Dialect { get; init; } public required string Kind { get; init; } public IReadOnlyList<ISqlNode> Parts { get; init; } = []; }
}

/// <summary>BNF: <select list>, <select sublist>, <derived column>, <qualified asterisk>, <all fields reference>.</summary>
public abstract record SelectItem : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record All : SelectItem;
	public record ExpressionItem(Expression Expression, Identifier? Alias = null, bool AsKeyword = false) : SelectItem;
	public record QualifiedAll(Expression Qualifier, IReadOnlyList<Identifier>? RenamedFields = null) : SelectItem;
}

public enum SetQuantifier { All, Distinct }
public enum SetOperator { Union, Except, Intersect }

/// <summary>BNF: UNION/EXCEPT/INTERSECT portions of <query expression body>/<query term>.</summary>
public sealed record SetOperation : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public required SetOperator Operator { get; init; }
	public SetQuantifier? Quantifier { get; init; }
	public CorrespondingClause? Corresponding { get; init; }
	public required QueryOperand Operand { get; init; }
}

/// <summary>BNF: <simple table>, parenthesized <query primary>. Grammar precedence nodes are intentionally erased.</summary>
public abstract record QueryOperand : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Select(Statement.Select Query) : QueryOperand;
	public record Values(IReadOnlyList<RowValue> Rows) : QueryOperand;
	public record Table(QualifiedName Name) : QueryOperand;
}

public sealed record WithClause(bool Recursive, IReadOnlyList<CommonTableExpression> Items) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record CommonTableExpression(Identifier Name, IReadOnlyList<Identifier> Columns, Statement.Select Query, SearchClause? Search = null, CycleClause? Cycle = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// BNF: <search clause>, <recursive search order>, <sequence column>.
public sealed record SearchClause(SearchOrder Order, IReadOnlyList<Identifier> Columns, Identifier SequenceColumn) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum SearchOrder { DepthFirst, BreadthFirst }

// BNF: <cycle clause>, <cycle column list>, <cycle mark column>, <cycle mark option>, <path column>.
public sealed record CycleClause(IReadOnlyList<Identifier> Columns, Identifier MarkColumn, Expression? MarkValue, Expression? NonCycleMarkValue, Identifier PathColumn) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record FromClause(IReadOnlyList<TableSource> Sources) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record IntoClause(IReadOnlyList<Expression> Targets) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record GroupByClause(SetQuantifier? Quantifier, IReadOnlyList<GroupingElement> Items) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record WindowClause(IReadOnlyList<WindowDefinition> Windows) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record OrderByClause(IReadOnlyList<SortItem> Items) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record OffsetClause(Expression Count, RowWord RowWord) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record FetchClause(FetchPosition Position, Expression? Quantity, bool Percent, RowWord RowWord, FetchMode Mode) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum RowWord { Row, Rows }
public enum FetchPosition { First, Next }
public enum FetchMode { Only, WithTies }
public sealed record CorrespondingClause(bool By, IReadOnlyList<Identifier> Columns) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record SortItem(Expression Key, SortDirection? Direction = null, NullOrdering? NullOrdering = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum SortDirection { Asc, Desc }
public enum NullOrdering { First, Last }

/// <summary>BNF: <table reference>, <table factor>, <table primary>, <joined table>, derived/collection/function/JSON tables.</summary>
public abstract record TableSource : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);

	// BNF: <table factor>'s <sample clause>, which may follow any <table primary>.
	public SampleClause? Sample { get; init; }
	public record Named(QualifiedName Name) : TableSource
	{
		public SystemTimeSpecification? SystemTime { get; init; }
		public Alias? Alias { get; init; }
		public bool Only { get; init; }
	}

	public record Subquery(Statement.Select Query) : TableSource
	{
		public bool Lateral { get; init; }
		public Alias? Alias { get; init; }
	}

	public record Join : TableSource
	{
		public required TableSource Left { get; init; }
		public required TableSource Right { get; init; }
		// BNF: <join type>; null where a bare `JOIN` was written, which is not the same text as `INNER JOIN`.
		public JoinKind? Kind { get; init; }
		public bool Natural { get; init; }
		public bool OuterKeyword { get; init; }

		// BNF: <partitioned join table>'s <join column list> on either side, `t PARTITION BY (a) LEFT JOIN u`.
		public IReadOnlyList<Expression>? LeftPartition { get; init; }
		public IReadOnlyList<Expression>? RightPartition { get; init; }
		public JoinSpecification? Specification { get; init; }
	}

	public record Unnest(IReadOnlyList<Expression> Expressions, bool WithOrdinality, Alias? Alias) : TableSource;
	public record Function(Expression.Invocation Invocation, Alias? Alias = null) : TableSource;

	// BNF: <table function derived table>, `TABLE (f (a))`.
	public record TableFunction(Expression Value, Alias? Alias = null) : TableSource;

	// BNF: <row pattern recognition clause> after a table primary, `t MATCH_RECOGNIZE (…) AS m`.
	public record RowPatternRecognition(TableSource Source, RowPatternClause Clause, Alias? Alias = null) : TableSource;
	public record JsonTable(JsonTableDefinition Definition, Alias? Alias = null) : TableSource;
	public record DataChange(ResultOption Option, Statement Change, Alias? Alias = null) : TableSource;
	public record Parenthesized(TableSource Source) : TableSource;
	public record Extension(string Dialect, string Kind, IReadOnlyList<ISqlNode> Parts) : TableSource;
}

public enum JoinKind { Cross, Inner, Left, Right, Full }
public abstract record JoinSpecification : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record On(Expression Condition) : JoinSpecification;
	public record Using(IReadOnlyList<Identifier> Columns, Identifier? Correlation = null) : JoinSpecification;
}
public sealed record Alias(Identifier Name, IReadOnlyList<Identifier>? Columns = null, bool AsKeyword = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record SampleClause(SampleMethod Method, Expression Percentage, Expression? Repeat = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum SampleMethod { Bernoulli, System }
public enum ResultOption { Final, New, Old }

/// <summary>BNF: <value expression> and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars.</summary>
public abstract record Expression : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Literal(LiteralValue Value) : Expression;
	public record Reference(QualifiedName Name) : Expression;

	// BNF: <host parameter specification>, <embedded variable specification>, <dynamic parameter specification>.
	public record Parameter(ParameterKind Kind, Identifier? Name = null) : Expression
	{
		// BNF: <indicator parameter>, <indicator variable> — `:a INDICATOR :i` or `:a :i`.
		public Identifier? Indicator { get; init; }
		public bool IndicatorKeyword { get; init; }
	}

	// BNF: <general value specification>'s key words, <datetime value function>. Precision is `CURRENT_TIME (3)`'s.
	public record Current(CurrentValue Kind, QualifiedName? TypeName = null, Expression? Precision = null) : Expression;

	public record Unary(UnaryOperator Operator, Expression Operand) : Expression;
	public record Binary(Expression Left, BinaryOperator Operator, Expression Right) : Expression;

	// BNF: <multiset value expression>, <multiset term> — `MULTISET UNION ALL`, which carries a quantifier.
	public record MultisetOperation(Expression Left, MultisetOperator Operator, SetQuantifier? Quantifier, Expression Right) : Expression;

	public record Row(IReadOnlyList<Expression> Items, bool RowKeyword = false) : Expression;
	public record Parenthesized(Expression Value) : Expression;
	public record Subquery(Statement.Select Query) : Expression;

	// BNF: <case expression>. In a simple CASE a when operand may be a predicate without its left side,
	// `WHEN < 5`, whose missing operand is written as CaseOperand.
	public record Case(Expression? Operand, IReadOnlyList<CaseWhen> Whens, Expression? Else) : Expression;

	// BNF: <case operand> standing where a <when operand> leaves it out. Nothing is written for it.
	public record CaseOperand : Expression;

	public record Cast(Expression Value, DataType Target, string? Format = null) : Expression;

	// BNF: <routine invocation>, <aggregate function>, <window function>, <row pattern navigation operation>,
	// and the functions whose arguments are a plain comma list.
	public record Invocation(QualifiedName Name, IReadOnlyList<Argument> Arguments) : Expression
	{
		// BNF: <set quantifier> in a <general set function>, `COUNT (DISTINCT a)`.
		public SetQuantifier? Quantifier { get; init; }

		// BNF: <sort specification list> inside the brackets, `ARRAY_AGG (a ORDER BY b)`.
		public OrderByClause? OrderBy { get; init; }

		// BNF: <listagg overflow clause>.
		public ListaggOverflow? Overflow { get; init; }

		// BNF: <from first or last>, <null treatment>.
		public FromFirstOrLast? From { get; init; }
		public NullTreatment? Nulls { get; init; }

		// BNF: <row pattern navigation operation>'s `RUNNING` or `FINAL`.
		public RowPatternSemantics? Semantics { get; init; }

		public FilterClause? Filter { get; init; }
		public WindowReference? Over { get; init; }
		public WithinGroupClause? WithinGroup { get; init; }

		// BNF: <co-partition clause>: `COPARTITION (a, b), (c, d)`.
		public IReadOnlyList<IReadOnlyList<QualifiedName>>? Copartition { get; init; }

		// A row pattern measure used as a window function, `m OVER w`, is written without brackets.
		public bool WithoutParentheses { get; init; }
	}

	// BNF: `COUNT (*)`'s <asterisk>, as the one argument.
	public record Asterisk : Expression;

	// BNF: <method invocation>, <attribute or method reference>, <static method invocation> (`T::m`), <dereference operation>.
	public record Member(Expression Target, MemberAccessKind Kind, Identifier Name, IReadOnlyList<Argument>? Arguments = null) : Expression;

	// BNF: <generalized expression>, `(a AS t).m ()`.
	public record Generalized(Expression Value, DataType Type) : Expression;

	// BNF: <subtype treatment>, <new specification>, <reference resolution>.
	public record Treat(Expression Value, DataType Target) : Expression;
	public record New(QualifiedName TypeName, IReadOnlyList<Argument> Arguments) : Expression;
	public record Dereference(Expression Value) : Expression;

	public record Array(IReadOnlyList<Expression> Items, bool Trigraphs = false) : Expression;
	public record Multiset(IReadOnlyList<Expression> Items, bool Trigraphs = false) : Expression;
	public record CollectionQuery(CollectionKind Kind, Statement.Select Query) : Expression;

	// BNF: <array element reference>, and a JSON simplified accessor's `[1 TO 3]`. Trigraphs are `??(` and `??)`.
	public record Element(Expression Collection, Expression Index, Expression? To = null, bool Trigraphs = false) : Expression;

	// BNF: a JSON simplified accessor's `.*` and `[*]`.
	public record Wildcard(Expression Target, WildcardKind Kind) : Expression;

	// BNF: a JSON simplified accessor's <JSON array accessor>, whose subscripts are in the path language: `a[$ to last]`.
	public record JsonAccessor(Expression Target, JsonPathAccessor Accessor) : Expression;

	public record NextValue(QualifiedName Sequence) : Expression;

	// BNF: <collate clause> on a character factor.
	public record Collate(Expression Value, CollationName Collation) : Expression;

	// BNF: <time zone>, `a AT TIME ZONE z` and `a AT LOCAL`, where Zone is null.
	public record AtTimeZone(Expression Value, Expression? Zone) : Expression;

	// BNF: <interval value expression>'s `(a - b) DAY TO SECOND`.
	public record IntervalQualified(Expression Value, IntervalQualifier Qualifier) : Expression;

	// BNF: <string value function>s whose arguments are key words and not a comma list.
	public record Substring(Expression Value, Expression From, Expression? For, CharacterLengthUnits? Using) : Expression;
	public record SubstringSimilar(Expression Value, Expression Pattern, Expression Escape) : Expression;
	public record Trim(TrimSpecification? Specification, Expression? Character, bool FromKeyword, Expression Source) : Expression;
	public record Overlay(Expression Value, Expression Placing, Expression From, Expression? For, CharacterLengthUnits? Using) : Expression;
	public record Position(Expression Value, Expression Within, CharacterLengthUnits? Using) : Expression;
	public record Length(LengthFunction Function, Expression Value, CharacterLengthUnits? Using) : Expression;
	public record Extract(ExtractField Field, Expression Source) : Expression;
	// BNF: <normalize function>; a result length may carry a multiplier and its units, `10 K CHARACTERS`.
	public record Normalize(Expression Value, NormalForm? Form, Expression? MaxLength) : Expression
	{
		public char? MaxLengthMultiplier { get; init; }
		public LengthUnit? MaxLengthUnit { get; init; }
	}
	public record TranslateUsing(TranslateFunction Function, Expression Value, QualifiedName Name) : Expression;

	// BNF: <regex occurrences function>, <regex position expression>, <regex substring function>, <regex transliteration>.
	public record Regex(RegexFunction Function, Expression Pattern, Expression Value) : Expression
	{
		public Expression? Flag { get; init; }
		public Expression? From { get; init; }
		public CharacterLengthUnits? Using { get; init; }
		public RegexPositionStartOrAfter? StartOrAfter { get; init; }
		public Expression? Replacement { get; init; }
		public Expression? Occurrence { get; init; }
		public bool AllOccurrences { get; init; }
		public Expression? Group { get; init; }
	}

	// BNF predicate families collapsed into Expression children rather than a BooleanExpression hierarchy.
	public record Comparison(Expression Left, ComparisonOperator Operator, Expression Right) : Expression;
	public record Between(Expression Value, bool Not, BetweenSymmetry? Symmetry, Expression Lower, Expression Upper) : Expression;
	public record In(Expression Value, bool Not, InSource SourceValue) : Expression;

	// BNF: <like predicate>, <similar predicate>, <regex like predicate> — whose FLAG is Flag.
	public record Like(Expression Value, bool Not, LikeKind Kind, Expression Pattern, Expression? Escape = null, Expression? Flag = null) : Expression;

	public record IsNull(Expression Value, bool Not) : Expression;

	// BNF: <boolean test>, `a IS NOT UNKNOWN`.
	public record IsTruth(Expression Value, bool Not, BooleanLiteral Truth) : Expression;

	public record QuantifiedComparison(Expression Left, ComparisonOperator Operator, Quantifier Quantifier, Statement.Select Query) : Expression;
	public record Exists(Statement.Select Query) : Expression;
	public record Unique(NullDistinctness? Nulls, Statement.Select Query) : Expression;

	// BNF: <match predicate>.
	public record Match(Expression Value, bool UniqueKeyword, MatchType? Type, Statement.Select Query) : Expression;

	// BNF: <overlaps predicate>, whose two sides are rows.
	public record Overlaps(Expression Left, Expression Right) : Expression;

	public record IsDistinct(Expression Left, bool Not, Expression Right) : Expression;
	public record IsNormalized(Expression Value, bool Not, NormalForm? Form) : Expression;
	public record MemberOf(Expression Value, bool Not, bool OfKeyword, Expression Collection) : Expression;
	public record SubmultisetOf(Expression Value, bool Not, bool OfKeyword, Expression Collection) : Expression;
	public record IsSet(Expression Value, bool Not) : Expression;
	public record IsOf(Expression Value, bool Not, IReadOnlyList<TypeTest> Types) : Expression;
	public record PeriodPredicate(PeriodValue Left, PeriodOperator Operator, PeriodRight Right) : Expression;
	public record JsonPredicate(Expression Value, JsonInputClause? Input, bool Not, JsonPredicateType? Type, JsonKeyUniqueness? Uniqueness) : Expression;
	public record JsonExists(JsonApiCommon Common, JsonExistsErrorBehavior? OnError) : Expression;

	// BNF: <JSON value function>, <JSON query>, the <JSON value constructor>s and <JSON aggregate function>s.
	public record JsonValue(JsonApiCommon Common, DataType? Returning, JsonValueBehavior? OnEmpty, JsonValueBehavior? OnError) : Expression;
	public record JsonQuery(JsonApiCommon Common, JsonOutput? Output, JsonWrapperBehavior? Wrapper, JsonQuotes? Quotes, JsonQueryBehavior? OnEmpty, JsonQueryBehavior? OnError) : Expression;
	public record JsonObject(IReadOnlyList<JsonMember> Members, JsonNullHandling? Nulls, JsonKeyUniqueness? Uniqueness, JsonOutput? Output) : Expression;
	public record JsonArray(IReadOnlyList<JsonElement> Elements, JsonNullHandling? Nulls, JsonOutput? Output) : Expression;
	public record JsonArrayQuery(Statement.Select Query, JsonInputClause? Format, JsonOutput? Output, JsonNullHandling? Nulls = null) : Expression;
	public record JsonObjectAggregate(JsonMember Pair, JsonNullHandling? Nulls, JsonKeyUniqueness? Uniqueness, JsonOutput? Output) : Expression
	{
		// BNF: <aggregate function>'s <filter clause>, a <window function>'s window, a navigation's `RUNNING` or `FINAL`.
		public FilterClause? Filter { get; init; }
		public WindowReference? Over { get; init; }
		public RowPatternSemantics? Semantics { get; init; }
	}
	public record JsonArrayAggregate(JsonElement Item, OrderByClause? OrderBy, JsonNullHandling? Nulls, JsonOutput? Output) : Expression
	{
		// BNF: <aggregate function>'s <filter clause>, a <window function>'s window, a navigation's `RUNNING` or `FINAL`.
		public FilterClause? Filter { get; init; }
		public WindowReference? Over { get; init; }
		public RowPatternSemantics? Semantics { get; init; }
	}
	public record JsonParse(Expression Value, JsonInputClause? Input, JsonKeyUniqueness? Uniqueness) : Expression;
	public record JsonScalar(Expression Value) : Expression;
	public record JsonSerialize(Expression Value, JsonOutput? Output) : Expression;

	// BNF: <table argument>: `TABLE (t)`, `TABLE (SELECT ...)` or a call, with its name, partitioning, pruning and order.
	public record TableArgument(Expression Source, bool TableKeyword) : Expression
	{
		public Alias? Alias { get; init; }
		public IReadOnlyList<Expression>? PartitionBy { get; init; }
		public bool PartitionParenthesized { get; init; }
		public TablePruning? Pruning { get; init; }
		public IReadOnlyList<SortItem>? OrderBy { get; init; }
		public bool OrderByParenthesized { get; init; }
	}

	// BNF: <descriptor value constructor>, `DESCRIPTOR (a INT, b)`.
	public record DescriptorConstructor(IReadOnlyList<DescriptorColumn> Columns) : Expression;

	// BNF: <current collation specification>, `COLLATION FOR (x)`.
	public record CollationFor(Expression Value) : Expression;

	// BNF: <default specification>, `DEFAULT` where a value stands.
	public record Default : Expression;

	// BNF: <row marker expression>, `END_FRAME - 1`, and <value_of expression at row>, `VALUE_OF (x AT BEGIN_FRAME, d)`.
	public record RowMarker(RowMarkerKind Kind, UnaryOperator? DeltaSign = null, Expression? Delta = null) : Expression;
	public record ValueOf(Expression Value, Expression At, Expression? Otherwise = null) : Expression;

	public record Extension(string Dialect, string Kind, IReadOnlyList<ISqlNode> Parts) : Expression;
}

public enum ParameterKind { Host, Sql, Dynamic, Embedded }
public enum CurrentValue { Catalog, Date, DefaultTransformGroup, Path, Role, Schema, Time, Timestamp, User, SessionUser, SystemUser, Value, LocalTime, LocalTimestamp, CurrentUser, TransformGroupForType }
public enum UnaryOperator { Plus, Minus, Not }
public enum BinaryOperator { Add, Subtract, Multiply, Divide, Concatenate, And, Or }
public enum MultisetOperator { Union, Except, Intersect }
public enum ComparisonOperator { Equal, NotEqual, Less, Greater, LessOrEqual, GreaterOrEqual }
public enum BetweenSymmetry { Asymmetric, Symmetric }
public enum Quantifier { All, Some, Any }
public enum LikeKind { Like, Similar, Regex }
public enum CollectionKind { Array, Multiset, Table }
public enum MemberAccessKind { Dot, Dereference, StaticMethod }
public enum WildcardKind { Member, Array }
public enum NormalForm { NFC, NFD, NFKC, NFKD }
public enum FromFirstOrLast { First, Last }
public enum NullTreatment { RespectNulls, IgnoreNulls }
public enum RowPatternSemantics { Running, Final }
public enum CharacterLengthUnits { Characters, Octets }
public enum TrimSpecification { Leading, Trailing, Both }
public enum LengthFunction { CharLength, CharacterLength, OctetLength }
public enum TranslateFunction { Convert, Translate }
public enum ExtractField { Year, Month, Day, Hour, Minute, Second, TimezoneHour, TimezoneMinute }
public enum RegexFunction { OccurrencesRegex, PositionRegex, SubstringRegex, TranslateRegex }
public enum RegexPositionStartOrAfter { Start, After }

// BNF: <listagg overflow clause>: `ON OVERFLOW ERROR`, or `ON OVERFLOW TRUNCATE [filler] WITH|WITHOUT COUNT`.
// BNF: <descriptor column specification>.
public sealed record DescriptorColumn(Identifier Name, DataType? Type = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// BNF: <row marker>.
public enum RowMarkerKind { BeginPartition, BeginFrame, CurrentRow, FrameRow, EndFrame, EndPartition }

public sealed record ListaggOverflow(bool Truncate, Expression? Filler = null, bool? WithCount = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

public abstract record LiteralValue : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Numeric(string Text, NumericLiteralKind Kind) : LiteralValue;
	// Text is the literal's spelling as written, from its first quote to its last: the segments of `'a' 'b'`
	// and the separators between them included, the introducer not.
	public record String(string Text, StringLiteralKind Kind, CharacterSetName? CharacterSet = null, char? UnicodeEscape = null) : LiteralValue;
	public record Binary(string Text) : LiteralValue;
	public record Boolean(BooleanLiteral Value) : LiteralValue;
	public record DateTime(DateTimeLiteralKind Kind, string Text) : LiteralValue;
	// Sign is `INTERVAL -'1' DAY`'s, null where none was written.
	public record Interval(string Text, IntervalQualifier Qualifier, UnaryOperator? Sign = null) : LiteralValue;
	public record Null : LiteralValue;
}
public enum NumericLiteralKind { DecimalInteger, HexInteger, OctalInteger, BinaryInteger, Decimal, Approximate }
public enum StringLiteralKind { Character, National, Unicode }
public enum BooleanLiteral { True, False, Unknown }
public enum DateTimeLiteralKind { Date, Time, Timestamp }

// BNF: <simple when clause>, <searched when clause>. A simple one's <when operand list> is `WHEN 1, 2`; a
// searched one has a single condition.
public sealed record CaseWhen(IReadOnlyList<Expression> When, Expression Then) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record Argument(Expression Value, Identifier? Name = null, bool NamedAssignment = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record FilterClause(Expression Condition) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record WithinGroupClause(OrderByClause OrderBy) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record WindowReference : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record NameRef(Identifier Name) : WindowReference; public record Specification(WindowSpecification Value) : WindowReference; }
public sealed record WindowSpecification(Identifier? Existing, IReadOnlyList<Expression> PartitionBy, OrderByClause? OrderBy, WindowFrame? Frame) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record WindowDefinition(Identifier Name, WindowSpecification Specification) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record WindowFrame(WindowFrameUnit Unit, WindowFrameExtent Extent, WindowFrameExclusion? Exclusion, RowPatternClause? Pattern) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum WindowFrameUnit { Rows, Range, Groups }
public abstract record WindowFrameExtent : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Single(WindowFrameBound Bound) : WindowFrameExtent; public record Between(WindowFrameBound From, WindowFrameBound To) : WindowFrameExtent; }
public sealed record WindowFrameBound(WindowBoundKind Kind, Expression? Offset = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum WindowBoundKind { UnboundedPreceding, Preceding, CurrentRow, Following, UnboundedFollowing }
public enum WindowFrameExclusion { CurrentRow, Group, Ties, NoOthers }

public abstract record InSource : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Query(Statement.Select Value) : InSource; public record Values(IReadOnlyList<Expression> Items) : InSource; }
public sealed record TypeTest(QualifiedName TypeName, bool Only) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record PeriodValue : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Reference(QualifiedName Name) : PeriodValue; public record Range(Expression Start, Expression End) : PeriodValue; }
public abstract record PeriodRight : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Period(PeriodValue Value) : PeriodRight; public record Point(Expression Value) : PeriodRight; }
public enum PeriodOperator { Overlaps, Equals, Contains, Precedes, Succeeds, ImmediatelyPrecedes, ImmediatelySucceeds }

/// <summary>BNF: <data type> and all predefined/row/reference/collection/user-defined type rules.</summary>
public abstract record DataType : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Character(CharacterTypeKind Kind, int? Length = null, LengthUnit? Unit = null, CharacterSetName? CharacterSet = null, CollationName? Collation = null, LargeObjectSize? LargeObject = null) : DataType;
	public record Binary(BinaryTypeKind Kind, int? Length = null, LargeObjectSize? LargeObject = null) : DataType;
	public record Numeric(NumericTypeKind Kind, int? Precision = null, int? Scale = null) : DataType;
	public record Boolean : DataType;
	public record DateTime(DateTimeTypeKind Kind, int? Precision = null, TimeZoneMode? TimeZone = null) : DataType;
	public record Interval(IntervalQualifier Qualifier) : DataType;
	public record Row(IReadOnlyList<FieldDefinition> Fields) : DataType;
	public record Reference(QualifiedName ReferencedType, QualifiedName? Scope = null) : DataType;
	public record Array(DataType ElementType, int? MaximumCardinality = null, bool Trigraphs = false) : DataType;
	public record Multiset(DataType ElementType) : DataType;
	public record UserDefined(QualifiedName Name) : DataType;
	public record Json : DataType;
	public record Domain(QualifiedName Name) : DataType;
	public record Descriptor : DataType;
	public record GenericTable(PassThroughMode? PassThrough, TableSemantics? Semantics, TablePruning? Pruning) : DataType;
	public record Extension(string Dialect, string Name, IReadOnlyList<ISqlNode> Arguments) : DataType;
}

public enum CharacterTypeKind { Character, Char, CharacterVarying, CharVarying, Varchar, CharacterLargeObject, CharLargeObject, Clob, NationalCharacter, NationalChar, Nchar, NationalCharacterVarying, NationalCharVarying, NcharVarying, NationalCharacterLargeObject, NcharLargeObject, Nclob }
public enum BinaryTypeKind { Binary, BinaryVarying, Varbinary, BinaryLargeObject, Blob }
public enum NumericTypeKind { Numeric, Decimal, Dec, SmallInt, Integer, Int, BigInt, Float, Real, DoublePrecision, DecFloat }
public enum DateTimeTypeKind { Date, Time, Timestamp }
public enum TimeZoneMode { With, Without }
public enum LengthUnit { Characters, Octets }
public readonly record struct LargeObjectSize(long Value, char? Multiplier);
public sealed record FieldDefinition(Identifier Name, DataType Type) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record IntervalQualifier(DateTimeField Start, int? LeadingPrecision = null, DateTimeField? End = null, int? FractionalPrecision = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum DateTimeField { Year, Month, Day, Hour, Minute, Second }

/// <summary>BNF: table element/column/period/constraint/LIKE/typed-table/as-subquery structures.</summary>
public abstract record TableContents : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Elements(IReadOnlyList<TableElement> Items) : TableContents;
	public record Typed(QualifiedName TypeName, QualifiedName? Under, IReadOnlyList<TableElement> Items) : TableContents;
	public record AsQuery(IReadOnlyList<Identifier> Columns, Statement.Select Query, WithDataMode Data) : TableContents;
}
public enum WithDataMode { WithData, WithNoData }
public abstract record TableElement : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Column(ColumnDefinition Definition) : TableElement;
	public record Period(PeriodDefinition Definition) : TableElement;
	public record TableConstraint(Constraint Value) : TableElement;
	public record Like(QualifiedName Table, IReadOnlyList<LikeOption> Options) : TableElement;
	public record SelfReference(Identifier Name, ReferenceGeneration? Generation) : TableElement;
	public record ColumnOptions(Identifier Name, ColumnOptionList Options) : TableElement;
}
public sealed record ColumnDefinition(Identifier Name, DataType? Type, ColumnGeneration? Generation, IReadOnlyList<Constraint> Constraints, CollationName? Collation) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record ColumnGeneration : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Default(Expression Value) : ColumnGeneration;
	public record Identity(IdentityGeneration Generation, IReadOnlyList<SequenceOption> Options) : ColumnGeneration;
	public record Generated(Expression Expression) : ColumnGeneration;
	public record RowStart : ColumnGeneration;
	public record RowEnd : ColumnGeneration;
}
public enum IdentityGeneration { Always, ByDefault }
public sealed record ColumnOptionList(QualifiedName? Scope, Expression? Default, IReadOnlyList<Constraint> Constraints) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum LikeOption { IncludingIdentity, ExcludingIdentity, IncludingDefaults, ExcludingDefaults, IncludingGenerated, ExcludingGenerated }
public enum ReferenceGeneration { SystemGenerated, UserGenerated, Derived }

public abstract record Constraint : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public QualifiedName? Name { get; init; }
	public ConstraintCharacteristics? Characteristics { get; init; }
	public record NotNull : Constraint;
	public record Unique(UniqueKind Kind, NullDistinctness? Nulls, IReadOnlyList<Identifier> Columns, Identifier? WithoutOverlapsPeriod = null) : Constraint;
	public record ForeignKey(IReadOnlyList<Identifier> Columns, Identifier? ReferencingPeriod, ReferencesSpecification References) : Constraint;
	public record Check(Expression Condition) : Constraint;
}
public enum UniqueKind { Unique, PrimaryKey, UniqueValue }
public enum NullDistinctness { Distinct, NotDistinct }
public sealed record ReferencesSpecification(QualifiedName Table, IReadOnlyList<Identifier> Columns, Identifier? Period, MatchType? Match, ReferentialAction? OnUpdate, ReferentialAction? OnDelete, bool DeleteRuleFirst = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum MatchType { Full, Partial, Simple }
public enum ReferentialAction { Cascade, SetNull, SetDefault, Restrict, NoAction }
// DeferrableFirst: `DEFERRABLE INITIALLY DEFERRED` rather than `INITIALLY DEFERRED DEFERRABLE`.
public sealed record ConstraintCharacteristics(ConstraintTiming? Timing, bool? Deferrable, bool? Enforced, bool DeferrableFirst = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum ConstraintTiming { InitiallyDeferred, InitiallyImmediate, Deferred, Immediate }

public abstract record AlterTableAction : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record AddColumn(ColumnDefinition Column, bool ColumnKeyword = false) : AlterTableAction;
	public record AlterColumn(Identifier Column, AlterColumnAction Action, bool ColumnKeyword = false) : AlterTableAction;
	public record DropColumn(Identifier Column, DropBehavior Behavior, bool ColumnKeyword = false) : AlterTableAction;
	public record AddConstraint(Constraint Constraint) : AlterTableAction;
	public record AlterConstraint(QualifiedName Name, bool Enforced) : AlterTableAction;
	public record DropConstraint(QualifiedName Name, DropBehavior Behavior) : AlterTableAction;
	public record AddPeriod(PeriodDefinition Period, IReadOnlyList<AddColumn> AddedColumns) : AlterTableAction;
	public record DropPeriod(PeriodKind Kind, Identifier? ApplicationName, DropBehavior Behavior) : AlterTableAction;
	public record AddSystemVersioning : AlterTableAction;
	public record DropSystemVersioning(DropBehavior Behavior) : AlterTableAction;
	public record Extension(string Dialect, string Kind, IReadOnlyList<ISqlNode> Parts) : AlterTableAction;
}
public abstract record AlterColumnAction : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record SetDefault(Expression Value) : AlterColumnAction;
	public record DropDefault : AlterColumnAction;
	public record SetNotNull : AlterColumnAction;
	public record DropNotNull : AlterColumnAction;
	public record AddScope(QualifiedName Table) : AlterColumnAction;
	public record DropScope(DropBehavior Behavior) : AlterColumnAction;
	public record SetDataType(DataType Type) : AlterColumnAction;
	public record SetIdentityGeneration(IdentityGeneration Generation, IReadOnlyList<SequenceOption> Options) : AlterColumnAction;
	public record IdentityOptions(IReadOnlyList<SequenceOption> Options) : AlterColumnAction;
	public record DropIdentity : AlterColumnAction;
	public record DropExpression : AlterColumnAction;
}
public sealed record PeriodDefinition(PeriodKind Kind, Identifier? ApplicationName, Identifier BeginColumn, Identifier EndColumn) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum PeriodKind { SystemTime, ApplicationTime }
public enum DropBehavior { Cascade, Restrict }
public enum TableScope { GlobalTemporary, LocalTemporary }
public enum TableCommitAction { Preserve, Delete }

/// <summary>BNF: grouping element families.</summary>
public abstract record GroupingElement : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Ordinary(IReadOnlyList<Expression> Expressions, bool Parenthesized = false) : GroupingElement;
	public record Rollup(IReadOnlyList<GroupingElement> Items) : GroupingElement;
	public record Cube(IReadOnlyList<GroupingElement> Items) : GroupingElement;
	public record Sets(IReadOnlyList<GroupingElement> Items) : GroupingElement;
	public record Empty : GroupingElement;
}

/// <summary>BNF: MATCH_RECOGNIZE and row-pattern grammar. Kept separate from normal SQL expressions.</summary>
public abstract record RowPattern : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Variable(Identifier Name) : RowPattern;
	public record StartAnchor : RowPattern;
	public record EndAnchor : RowPattern;
	public record Sequence(IReadOnlyList<RowPattern> Items) : RowPattern;
	public record Alternation(IReadOnlyList<RowPattern> Items) : RowPattern;
	public record Quantified(RowPattern Pattern, RowPatternQuantifier Quantifier) : RowPattern;
	public record Parenthesized(RowPattern? Pattern) : RowPattern;
	public record Excluded(RowPattern Pattern) : RowPattern;
	public record Permute(IReadOnlyList<RowPattern> Items) : RowPattern;
}
public sealed record RowPatternQuantifier(RowPatternQuantifierKind Kind, int? Min = null, int? Max = null, bool Reluctant = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum RowPatternQuantifierKind { ZeroOrMore, OneOrMore, ZeroOrOne, Range, Exact }
public sealed record RowPatternClause(IReadOnlyList<Expression> PartitionBy, OrderByClause? OrderBy, IReadOnlyList<RowPatternMeasure> Measures, RowsPerMatch? RowsPerMatch, RowPatternSkip? AfterMatch, RowPatternInitial? Initial, RowPattern? Pattern, IReadOnlyList<RowPatternSubset> Subsets, IReadOnlyList<RowPatternDefinition> Definitions) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record RowPatternMeasure(Expression Expression, Identifier Name) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record RowPatternSubset(Identifier Name, IReadOnlyList<Identifier> Variables) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record RowPatternDefinition(Identifier Variable, Expression Condition) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum RowsPerMatch { One, All, AllShowEmpty, AllOmitEmpty, AllWithUnmatched }
public enum RowPatternInitial { Initial, Seek }
public sealed record RowPatternSkip(RowPatternSkipKind Kind, Identifier? Variable = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum RowPatternSkipKind { NextRow, PastLastRow, First, Last, Variable }

/// <summary>BNF: SQL/JSON path language. Separate expression language embedded in SQL.</summary>
public abstract record JsonPathExpression : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Literal(string Text, JsonPathLiteralKind Kind) : JsonPathExpression;
	public record Variable(JsonPathVariableKind Kind, Identifier? Name = null) : JsonPathExpression;
	public record Unary(JsonPathUnaryOperator Operator, JsonPathExpression Operand) : JsonPathExpression;
	public record Binary(JsonPathExpression Left, JsonPathBinaryOperator Operator, JsonPathExpression Right) : JsonPathExpression;
	public record Access(JsonPathExpression Target, JsonPathAccessor Accessor) : JsonPathExpression;
	public record Predicate(JsonPathPredicate Value) : JsonPathExpression;
	public record Parenthesized(JsonPathExpression Value) : JsonPathExpression;
}
public enum JsonPathLiteralKind { String, Numeric, Other }
public enum JsonPathVariableKind { Context, Named, Current, Last }
public enum JsonPathUnaryOperator { Plus, Minus }
public enum JsonPathBinaryOperator { Add, Subtract, Multiply, Divide, Modulo }
public abstract record JsonPathAccessor : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Member(string Name, bool Quoted) : JsonPathAccessor;
	public record WildcardMember : JsonPathAccessor;
	public record Array(IReadOnlyList<JsonSubscript> Subscripts) : JsonPathAccessor;
	public record WildcardArray : JsonPathAccessor;
	public record Filter(JsonPathPredicate Predicate) : JsonPathAccessor;
	public record Method(JsonMethod Value) : JsonPathAccessor;
}
public sealed record JsonSubscript(JsonPathExpression From, JsonPathExpression? To = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record JsonPathPredicate : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Exists(JsonPathExpression Expression) : JsonPathPredicate;
	public record Comparison(JsonPathExpression Left, JsonPathComparisonOperator Operator, JsonPathExpression Right) : JsonPathPredicate;
	public record LikeRegex(JsonPathExpression Value, string Pattern, string? Flags) : JsonPathPredicate;
	public record StartsWith(JsonPathExpression Value, JsonPathExpression Initial) : JsonPathPredicate;
	public record IsUnknown(JsonPathPredicate Predicate) : JsonPathPredicate;
	public record Not(JsonPathPredicate Predicate) : JsonPathPredicate;
	public record And(IReadOnlyList<JsonPathPredicate> Items) : JsonPathPredicate;
	public record Or(IReadOnlyList<JsonPathPredicate> Items) : JsonPathPredicate;
	public record Parenthesized(JsonPathPredicate Predicate) : JsonPathPredicate;
}
public enum JsonPathComparisonOperator { Equal, NotEqual, NotEqualBang, Less, Greater, LessOrEqual, GreaterOrEqual }
public sealed record JsonMethod(JsonMethodKind Kind, int? Precision = null, int? Scale = null, string? DateTimeTemplate = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum JsonMethodKind { Type, Size, Double, Ceiling, Floor, Abs, DateTime, KeyValue, BigInt, Boolean, Date, Decimal, Integer, Number, String, Time, TimeTz, Timestamp, TimestampTz }
public sealed record JsonPath(JsonPathMode Mode, JsonPathExpression Expression) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum JsonPathMode { Strict, Lax }

// --- JSON SQL structures ----------------------------------------------------
// PathSpecification is the literal as written, like LiteralValue.String.Text. ContextFormat is the context item's `FORMAT JSON`.
public sealed record JsonApiCommon(Expression Context, string PathSpecification, Identifier? PathName, IReadOnlyList<JsonArgument> Passing, JsonInputClause? ContextFormat = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record JsonArgument(Expression Value, Identifier Name, JsonInputClause? Format = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record JsonInputClause(JsonRepresentation Representation) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record JsonRepresentation(JsonEncoding? Encoding = null, string? ImplementationDefined = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum JsonEncoding { Utf8, Utf16, Utf32 }
public enum JsonPredicateType { Value, Array, Object, Scalar }
public enum JsonKeyUniqueness { WithUniqueKeys, WithUnique, WithoutUniqueKeys, WithoutUnique }
public enum JsonExistsErrorBehavior { True, False, Unknown, Error }
public sealed record JsonTableDefinition(JsonApiCommon Common, IReadOnlyList<JsonTableColumn> Columns, JsonTablePlan? Plan, JsonTableErrorBehavior? OnError, bool Primitive = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record JsonTableColumn : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Ordinality(Identifier Name) : JsonTableColumn;
	public record Regular(Identifier Name, DataType Type, string? Path, JsonValueBehavior? OnEmpty, JsonValueBehavior? OnError) : JsonTableColumn;
	public record Formatted(Identifier Name, DataType Type, JsonRepresentation? Format, string? Path, JsonWrapperBehavior? Wrapper, JsonQuotes? Quotes, JsonQueryBehavior? OnEmpty, JsonQueryBehavior? OnError) : JsonTableColumn;
	public record Nested(string Path, Identifier? Name, IReadOnlyList<JsonTableColumn> Columns, bool PathKeyword = false) : JsonTableColumn;
	public record Chaining(Identifier Name) : JsonTableColumn;
}
public abstract record JsonTablePlan : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Name(Identifier Value) : JsonTablePlan;
	public record Outer(Identifier Parent, JsonTablePlan Child) : JsonTablePlan;
	public record Inner(Identifier Parent, JsonTablePlan Child) : JsonTablePlan;
	public record Union(IReadOnlyList<JsonTablePlan> Items) : JsonTablePlan;
	public record Cross(IReadOnlyList<JsonTablePlan> Items) : JsonTablePlan;
	public record Default(JsonTableDefaultInnerOuter? InnerOuter, JsonTableDefaultUnionCross? UnionCross, bool UnionCrossFirst = false) : JsonTablePlan;
	public record Parenthesized(JsonTablePlan Value) : JsonTablePlan;
}
public enum JsonTableDefaultInnerOuter { Inner, Outer }
public enum JsonTableDefaultUnionCross { Union, Cross }
public enum JsonTableErrorBehavior { Error, Empty }
public abstract record JsonValueBehavior : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Error : JsonValueBehavior; public record Null : JsonValueBehavior; public record Default(Expression Value) : JsonValueBehavior; }
public enum JsonWrapperBehavior { Without, WithoutArray, With, WithConditional, WithUnconditional, WithArray, WithConditionalArray, WithUnconditionalArray }
public enum JsonQuotesBehavior { Keep, Omit }
public enum JsonQueryBehavior { Error, Null, EmptyArray, EmptyObject }

// BNF: <JSON output clause>: `RETURNING t FORMAT JSON ENCODING UTF8`.
public sealed record JsonOutput(DataType Type, JsonRepresentation? Format = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// BNF: <JSON query quotes behavior>: `KEEP QUOTES ON SCALAR STRING`.
public sealed record JsonQuotes(JsonQuotesBehavior Behavior, bool OnScalarString) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// BNF: <JSON name and value>: `KEY k VALUE v`, `k VALUE v` or `k : v`, with the value's format.
public sealed record JsonMember(Expression Key, Expression Value, JsonMemberSyntax Syntax, JsonInputClause? Format = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum JsonMemberSyntax { KeyValue, Value, Colon }

// BNF: <JSON value expression> in an array constructor or aggregate, with its format.
public sealed record JsonElement(Expression Value, JsonInputClause? Format = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// BNF: <JSON constructor null clause>.
public enum JsonNullHandling { NullOnNull, AbsentOnNull }

// --- DML helper families ----------------------------------------------------
public abstract record InsertSource : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Query(Statement.Select Select) : InsertSource;
	public record Values(IReadOnlyList<RowValue> Rows) : InsertSource;
	public record DefaultValues : InsertSource;
}
public sealed record RowValue(IReadOnlyList<Expression> Items, bool RowKeyword = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum OverrideKind { UserValue, SystemValue }
public abstract record MergeClause : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public Expression? Condition { get; init; }
	public record Matched(MergeMatchedAction Action) : MergeClause;
	public record NotMatched(MergeInsertAction Action) : MergeClause;
}
public abstract record MergeMatchedAction : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Update(IReadOnlyList<Assignment> Assignments) : MergeMatchedAction; public record Delete : MergeMatchedAction; }
public sealed record MergeInsertAction(IReadOnlyList<Identifier> Columns, OverrideKind? Override, IReadOnlyList<Expression> Values) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
// BNF: <set clause>, <multiple column assignment>. Parenthesized where the targets were written in brackets: `(a) = (1)` is not `a = (1)`.
public sealed record Assignment(IReadOnlyList<AssignmentTarget> Targets, Expression Value, bool Parenthesized = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// BNF: <set target>, <update target>, <mutated set clause>. Trigraphs are `??(` and `??)` around the index.
public sealed record AssignmentTarget(QualifiedName Name, Expression? Index = null, IReadOnlyList<Identifier>? MutationPath = null, bool Trigraphs = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record PeriodPortion(Identifier Name, Expression From, Expression To) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum IdentityRestart { Continue, Restart }

// --- Names and small value objects -----------------------------------------
public sealed record QualifiedName(IReadOnlyList<Identifier> Parts) : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public static QualifiedName Of(params Identifier[] parts) => new(parts);
}
public sealed record CharacterSetName(QualifiedName Name) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record CollationName(QualifiedName Name) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record AuthorizationIdentifier(Identifier Name) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record PathSpecification(IReadOnlyList<QualifiedName> Schemas) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// --- Temporal/table source --------------------------------------------------
public abstract record SystemTimeSpecification : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record AsOf(Expression Point) : SystemTimeSpecification;
	public record Between(Expression From, Expression To, BetweenSymmetry? Symmetry) : SystemTimeSpecification;
	public record FromTo(Expression From, Expression To) : SystemTimeSpecification;
}

// --- ALTER DOMAIN -----------------------------------------------------------
public abstract record AlterDomainAction : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record SetDefault(Expression Value) : AlterDomainAction;
	public record DropDefault : AlterDomainAction;
	public record AddConstraint(Constraint Constraint) : AlterDomainAction;
	public record DropConstraint(QualifiedName Name) : AlterDomainAction;
}

// --- Views -----------------------------------------------------------------
public abstract record ViewSpecification : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Regular(IReadOnlyList<Identifier> Columns) : ViewSpecification;
	public record Referenceable(QualifiedName TypeName, QualifiedName? Under, IReadOnlyList<ViewElement> Elements) : ViewSpecification;
}
public abstract record ViewElement : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record SelfReference(Identifier Name, ReferenceGeneration? Generation) : ViewElement; public record ColumnOption(Identifier Name, QualifiedName Scope) : ViewElement; }
public enum CheckOption { Cascaded, Local, Unqualified }

// --- Triggers ---------------------------------------------------------------
public enum TriggerTime { Before, After, InsteadOf }
public sealed record TriggerEvent(TriggerEventKind Kind, IReadOnlyList<Identifier> Columns) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum TriggerEventKind { Insert, Delete, Update }
public sealed record TransitionReference(TransitionKind Kind, Identifier Name, bool RowKeyword = false, bool AsKeyword = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum TransitionKind { OldRow, NewRow, OldTable, NewTable }
public sealed record TriggerAction(TriggerGranularity? Granularity, Expression? When, IReadOnlyList<Statement> Statements, bool AtomicBlock = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum TriggerGranularity { Row, Statement }

// --- UDT/routines/transforms ------------------------------------------------
public sealed record UserDefinedTypeDefinition(QualifiedName Name, QualifiedName? Under, TypeRepresentation? Representation, IReadOnlyList<UserTypeOption> Options, IReadOnlyList<MethodSpecification> Methods) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record TypeRepresentation : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Type(DataType Value) : TypeRepresentation; public record Members(IReadOnlyList<AttributeDefinition> Attributes) : TypeRepresentation; }
public sealed record AttributeDefinition(Identifier Name, DataType Type, Expression? Default, CollationName? Collation) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record UserTypeOption : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Instantiable(bool Value) : UserTypeOption;
	public record Final(bool Value) : UserTypeOption;
	public record Reference(UserTypeReference Representation) : UserTypeOption;
	public record Cast(UserTypeCastKind Kind, Identifier FunctionName) : UserTypeOption;
}
public abstract record UserTypeReference : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Using(DataType Type) : UserTypeReference; public record FromAttributes(IReadOnlyList<Identifier> Attributes) : UserTypeReference; public record SystemGenerated : UserTypeReference; }
public enum UserTypeCastKind { ToRef, ToType, ToDistinct, ToSource }
public abstract record AlterTypeAction : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record AddAttribute(AttributeDefinition Attribute) : AlterTypeAction; public record DropAttribute(Identifier Name) : AlterTypeAction; public record AddMethod(MethodSpecification Method, bool Overriding) : AlterTypeAction; public record DropMethod(MethodDesignator Method) : AlterTypeAction; }
public sealed record MethodSpecification(MethodModifier? Modifier, Identifier Name, IReadOnlyList<ParameterDefinition> Parameters, ReturnsDefinition Returns, QualifiedName? SpecificName, bool SelfAsResult, bool SelfAsLocator, IReadOnlyList<RoutineCharacteristic> Characteristics, bool Overriding = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum MethodModifier { Instance, Static, Constructor }
public sealed record MethodDesignator(MethodModifier? Modifier, Identifier Name, IReadOnlyList<DataType> ParameterTypes) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// BNF: <schema procedure>, <schema function>, <method specification designator>. StaticDispatch is <dispatch clause>.
public sealed record RoutineDefinition(RoutineKind Kind, QualifiedName Name, IReadOnlyList<ParameterDefinition> Parameters, ReturnsDefinition? Returns, IReadOnlyList<RoutineCharacteristic> Characteristics, RoutineBody Body, MethodModifier? MethodModifier = null, QualifiedName? ForType = null, bool StaticDispatch = false, bool SpecificMethod = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum RoutineKind { Routine, Procedure, Function, Method }
// BNF: <SQL parameter declaration>. Locator is <locator indication>, `AS LOCATOR`.
public sealed record ParameterDefinition(ParameterMode? Mode, Identifier? Name, DataType Type, bool Result, Expression? Default, bool Locator = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum ParameterMode { In, Out, InOut }
// BNF: <returns clause>, <returns type>, <result cast>. `RETURNS TABLE` with no columns is TableKeyword alone.
public sealed record ReturnsDefinition(DataType? Type, IReadOnlyList<FieldDefinition>? TableColumns, bool OnlyPassThrough = false) : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public bool TableKeyword { get; init; }
	public bool Locator { get; init; }
	public DataType? CastFrom { get; init; }
	public bool CastFromLocator { get; init; }
}
public abstract record RoutineCharacteristic : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Language(string Name) : RoutineCharacteristic;
	public record ParameterStyle(ParameterStyleKind Style) : RoutineCharacteristic;
	public record Specific(QualifiedName Name) : RoutineCharacteristic;
	public record Deterministic(bool Value) : RoutineCharacteristic;
	public record DataAccess(SqlDataAccess Value) : RoutineCharacteristic;
	public record NullCall(NullCallMode Value) : RoutineCharacteristic;
	public record DynamicResultSets(int Maximum) : RoutineCharacteristic;
	public record SavepointLevel(SavepointLevelKind Value) : RoutineCharacteristic;
	// BNF: `NAME <external routine name>`. Name is written as it stands, quotes included: `NAME x` and `NAME 'x'` differ.
	public record ExternalName(string Name) : RoutineCharacteristic;
}
public enum ParameterStyleKind { Sql, General }
public enum SqlDataAccess { NoSql, ContainsSql, ReadsSqlData, ModifiesSqlData }
public enum NullCallMode { ReturnsNullOnNullInput, CalledOnNullInput }
public enum SavepointLevelKind { New, Old }
public abstract record RoutineBody : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	// BNF: <SQL routine spec>: the rights it runs with and one <SQL procedure statement>.
	public record Sql(Statement Statement, SqlSecurity? Security = null) : RoutineBody;
	// BNF: <external body reference>. Name is the <external routine name> as written, quotes included.
	public record External(string? Name, ParameterStyleKind? ParameterStyle, TransformGroupSpecification? TransformGroup, ExternalSecurity? Security) : RoutineBody;
	public record PolymorphicTableFunction(PolymorphicTableFunctionBody Body) : RoutineBody;
}
public enum SqlSecurity { Invoker, Definer }
public enum ExternalSecurity { Definer, Invoker, ImplementationDefined }
// PrivateParameters is null where no `PRIVATE` was written; PrivateDataKeyword is `PRIVATE DATA`.
public sealed record PolymorphicTableFunctionBody(IReadOnlyList<ParameterDefinition>? PrivateParameters, RoutineDesignator? Describe, RoutineDesignator? Start, RoutineDesignator Fulfill, RoutineDesignator? Finish, bool PrivateDataKeyword = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record RoutineDesignator(RoutineKind Kind, QualifiedName Name, IReadOnlyList<DataType>? ParameterTypes = null, QualifiedName? ForType = null, bool SpecificKeyword = false, MethodModifier? MethodModifier = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum AlterRoutineBehavior { Restrict }

public sealed record TranslationSource(QualifiedName? Existing, RoutineDesignator? Routine) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record OrderingDefinition(OrderingForm Form, OrderingCategory Category) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum OrderingForm { EqualsOnly, Full }
public abstract record OrderingCategory : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Relative(RoutineDesignator Function) : OrderingCategory; public record Map(RoutineDesignator Function) : OrderingCategory; public record State(QualifiedName? SpecificName) : OrderingCategory; }
public sealed record TransformGroup(Identifier Name, IReadOnlyList<TransformElement> Elements) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record TransformElement(TransformDirection Direction, RoutineDesignator Function) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum TransformDirection { ToSql, FromSql }
public sealed record TransformAlterGroup(Identifier Name, IReadOnlyList<TransformAlterAction> Actions) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record TransformAlterAction(bool Add, IReadOnlyList<TransformElement> Elements, IReadOnlyList<TransformDirection> DropKinds, DropBehavior? Behavior) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record TransformDropTarget : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record All : TransformDropTarget; public record Group(Identifier Name) : TransformDropTarget; }
public sealed record TransformGroupSpecification(Identifier? SingleGroup, IReadOnlyList<TransformGroupForType> Groups) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
// BNF: <group specification>, a transform group for a type.
public sealed record TransformGroupForType(Identifier Group, QualifiedName Type) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// --- Sequences --------------------------------------------------------------
public abstract record SequenceOption : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record DataTypeOption(DataType Type) : SequenceOption;
	public record Start(Expression Value) : SequenceOption;
	public record Increment(Expression Value) : SequenceOption;
	public record Max(Expression? Value, bool No = false) : SequenceOption;
	public record Min(Expression? Value, bool No = false) : SequenceOption;
	public record Cycle(bool Value) : SequenceOption;
	public record Restart(Expression? Value) : SequenceOption;
}

// --- Privileges/roles -------------------------------------------------------
public abstract record GrantBody : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Privileges(IReadOnlyList<Privilege> Items, PrivilegeObject Object, IReadOnlyList<Grantee> Grantees, bool HierarchyOption, bool GrantOption, Grantor? GrantedBy) : GrantBody;
	public record Roles(IReadOnlyList<Identifier> Names, IReadOnlyList<Grantee> Grantees, bool AdminOption, Grantor? GrantedBy) : GrantBody;
}
public abstract record RevokeBody : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Privileges(RevokeOption? Option, IReadOnlyList<Privilege> Items, PrivilegeObject Object, IReadOnlyList<Grantee> Grantees, Grantor? GrantedBy, DropBehavior Behavior) : RevokeBody;
	public record Roles(bool AdminOptionFor, IReadOnlyList<Identifier> Names, IReadOnlyList<Grantee> Grantees, Grantor? GrantedBy, DropBehavior Behavior) : RevokeBody;
}
public sealed record Privilege(PrivilegeKind Kind, IReadOnlyList<Identifier> Columns, IReadOnlyList<RoutineDesignator> Methods) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum PrivilegeKind { AllPrivileges, Select, Delete, Insert, Update, References, Usage, Trigger, Under, Execute }
// BNF: <object name>. TableKeyword says `ON TABLE t` rather than `ON t`.
public sealed record PrivilegeObject(PrivilegeObjectKind Kind, QualifiedName Name, RoutineDesignator? Routine = null, bool TableKeyword = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum PrivilegeObjectKind { Table, Domain, Collation, CharacterSet, Translation, Type, Sequence, Routine }
public abstract record Grantee : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Public : Grantee; public record Identifier(AuthorizationIdentifier Value) : Grantee; }
public enum Grantor { CurrentUser, CurrentRole }
public enum RevokeOption { GrantOptionFor, HierarchyOptionFor }

// --- Cursors/dynamic SQL/descriptors ---------------------------------------
// BNF: <cursor name> (a <local qualified name>, `MODULE.c`), <extended cursor name>, and a dynamic cursor's <scope option>.
// Name is null where the cursor is named by an extended name alone, `GLOBAL :c`.
public sealed record CursorReference(QualifiedName? Name, Expression? ExtendedName = null, bool Ptf = false, bool Global = false, bool Local = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record CursorProperties(CursorSensitivity? Sensitivity, CursorScrollability? Scrollability, CursorHoldability? Holdability, CursorReturnability? Returnability) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum CursorSensitivity { Sensitive, Insensitive, Asensitive }
public enum CursorScrollability { Scroll, NoScroll }
public enum CursorHoldability { WithHold, WithoutHold }
public enum CursorReturnability { WithReturn, WithoutReturn }
public abstract record CursorSource : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Query(Statement.Select Select, UpdatabilityClause? Updatability) : CursorSource; public record Prepared(StatementReference Statement) : CursorSource; }
public sealed record UpdatabilityClause(bool ReadOnly, IReadOnlyList<Identifier> UpdateColumns) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record FetchOrientation(FetchOrientationKind Kind, Expression? Offset = null) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum FetchOrientationKind { Next, Prior, First, Last, Absolute, Relative }
public abstract record CursorAllocationSource : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Prepared(StatementReference Statement) : CursorAllocationSource; public record Routine(RoutineDesignator Designator) : CursorAllocationSource; }
public sealed record StatementReference(Identifier? Name, Expression? ExtendedName = null, bool Global = false, bool Local = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record DescriptorReference(Identifier? Name, Expression? ExtendedName = null, bool Ptf = false, bool Global = false, bool Local = false) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record DescriptorGet : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Header(IReadOnlyList<DescriptorRead> Items) : DescriptorGet; public record Value(Expression Index, IReadOnlyList<DescriptorRead> Items) : DescriptorGet; }
public abstract record DescriptorSet : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Header(IReadOnlyList<DescriptorWrite> Items) : DescriptorSet; public record Value(Expression Index, IReadOnlyList<DescriptorWrite> Items) : DescriptorSet; }
public sealed record DescriptorRead(Expression Target, DescriptorItem Item) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record DescriptorWrite(DescriptorItem Item, Expression Value) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum DescriptorItem { Count, KeyType, DynamicFunction, DynamicFunctionCode, TopLevelCount, Cardinality, CharacterSetCatalog, CharacterSetName, CharacterSetSchema, CollationCatalog, CollationName, CollationSchema, Data, DatetimeIntervalCode, DatetimeIntervalPrecision, Degree, Indicator, KeyMember, Length, Level, Name, Nullable, NullOrdering, OctetLength, ParameterMode, ParameterOrdinalPosition, ParameterSpecificCatalog, ParameterSpecificName, ParameterSpecificSchema, Precision, ReturnedCardinality, ReturnedLength, ReturnedOctetLength, Scale, ScopeCatalog, ScopeName, ScopeSchema, SortDirection, Type, Unnamed, UserDefinedTypeCatalog, UserDefinedTypeName, UserDefinedTypeSchema, UserDefinedTypeCode }
public abstract record DescriptorCopy : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Whole(DescriptorReference Source, DescriptorReference Target) : DescriptorCopy; public record Item(DescriptorReference Source, Expression SourceIndex, IReadOnlyList<DescriptorCopyOption> Options, DescriptorReference Target, Expression TargetIndex) : DescriptorCopy; }
public enum DescriptorCopyOption { Name, Type, Data }
public abstract record DescribeBody : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Input(StatementReference Statement, DescriptorReference Descriptor, bool? WithNesting, bool SqlKeyword = false) : DescribeBody; public record Output(DescribeObject Object, DescriptorReference Descriptor, bool? WithNesting, bool OutputKeyword, bool SqlKeyword = false) : DescribeBody; }
public abstract record DescribeObject : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Statement(StatementReference Value) : DescribeObject; public record Cursor(CursorReference Value) : DescribeObject; }
public abstract record DynamicArguments : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Values(IReadOnlyList<Expression> Items) : DynamicArguments; public record Descriptor(DescriptorReference Value, bool SqlKeyword) : DynamicArguments; }

// --- Transactions/connections ---------------------------------------------
public abstract record TransactionMode : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Isolation(IsolationLevel Level) : TransactionMode; public record Access(TransactionAccess Mode) : TransactionMode; public record DiagnosticsSize(Expression Size) : TransactionMode; }
public enum IsolationLevel { ReadUncommitted, ReadCommitted, RepeatableRead, Serializable }
public enum TransactionAccess { ReadOnly, ReadWrite }
public abstract record ConstraintTarget : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record All : ConstraintTarget; public record Names(IReadOnlyList<QualifiedName> Values) : ConstraintTarget; }
public enum ChainMode { Chain, NoChain }
public sealed record ConnectionTarget(Expression? Server, Expression? Name, Expression? User, bool Default) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public abstract record ConnectionObject : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Default : ConnectionObject; public record Named(Expression Name) : ConnectionObject; }
public abstract record DisconnectObject : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Connection(ConnectionObject Value) : DisconnectObject; public record All : DisconnectObject; public record Current : DisconnectObject; }
public sealed record TransformGroupCharacteristic(bool DefaultGroup, QualifiedName? ForType, Expression Value) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// --- Diagnostics ------------------------------------------------------------

// BNF: <SQL diagnostics information>: a statement's items, a condition's items, or all of either.
public abstract record DiagnosticsInformation : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Statement(IReadOnlyList<StatementInformationItem> Items) : DiagnosticsInformation;
	public record Condition(Expression Number, IReadOnlyList<ConditionInformationItem> Items) : DiagnosticsInformation;

	// BNF: <all information>, `:t = ALL CONDITION 1`; Qualifier null where none was written.
	public record All(Expression Target, AllInformationQualifier? Qualifier, Expression? ConditionNumber = null) : DiagnosticsInformation;
}
public sealed record StatementInformationItem(Expression Target, StatementInformationItemName Name) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public sealed record ConditionInformationItem(Expression Target, ConditionInformationItemName Name) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum AllInformationQualifier { Statement, Condition }
public enum StatementInformationItemName { Number, More, CommandFunction, CommandFunctionCode, DynamicFunction, DynamicFunctionCode, RowCount, TransactionsCommitted, TransactionsRolledBack, TransactionActive }
public enum ConditionInformationItemName { CatalogName, ClassOrigin, ColumnName, ConditionNumber, ConnectionName, ConstraintCatalog, ConstraintName, ConstraintSchema, CursorName, MessageLength, MessageOctetLength, MessageText, ParameterMode, ParameterName, ParameterOrdinalPosition, ReturnedSqlstate, RoutineCatalog, RoutineName, RoutineSchema, SchemaName, ServerName, SpecificName, SubclassOrigin, TableName, TriggerCatalog, TriggerName, TriggerSchema }

// --- SQL conditions / WHENEVER ---------------------------------------------
public abstract record SqlCondition : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Major(SqlConditionMajor Value) : SqlCondition; public record State(string ClassCode, string? SubclassCode) : SqlCondition; public record Constraint(QualifiedName Name) : SqlCondition; }
public enum SqlConditionMajor { Exception, Warning, NotFound }
public abstract record ConditionAction : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); public record Continue : ConditionAction; public record GoTo(string Target, bool TwoWordKeyword) : ConditionAction; }

// --- Embedded SQL -----------------------------------------------------------
/// <summary>BNF: embedded SQL host-program sections. Kept out of the core SQL families.</summary>
public abstract record EmbeddedSql : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Statement(EmbeddedPrefix Prefix, DotGram.Sql.Ast.Statement Value, EmbeddedTerminator? Terminator) : EmbeddedSql;
	public record DeclareSection(EmbeddedPrefix Prefix, CharacterSetName? CharacterSet, IReadOnlyList<HostDeclaration> Declarations, EmbeddedTerminator? Terminator, EmbeddedPrefix? EndPrefix = null, EmbeddedTerminator? EndTerminator = null) : EmbeddedSql;
	public record Authorization(EmbeddedAuthorization Value) : EmbeddedSql;
	public record Path(PathSpecification Value) : EmbeddedSql;
	public record Transform(TransformGroupSpecification Value) : EmbeddedSql;
	public record Collation(IReadOnlyList<ModuleCollation> Values) : EmbeddedSql;
}
public enum EmbeddedPrefix { ExecSql, AmpersandSqlParen }
public enum EmbeddedTerminator { EndExec, Semicolon, RightParen }
public abstract record HostDeclaration : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Ada(string Text) : HostDeclaration;
	public record C(string Text) : HostDeclaration;
	public record Cobol(string Text) : HostDeclaration;
	public record Fortran(string Text) : HostDeclaration;
	public record Mumps(string Text) : HostDeclaration;
	public record Pascal(string Text) : HostDeclaration;
	public record Pli(string Text) : HostDeclaration;
}
public sealed record EmbeddedAuthorization(QualifiedName? Schema, AuthorizationIdentifier? Authorization, EmbeddedStaticMode? StaticMode) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }
public enum EmbeddedStaticMode { StaticOnly, StaticAndDynamic }
public sealed record ModuleCollation(CollationName Collation, IReadOnlyList<CharacterSetName> CharacterSets) : ISqlNode { public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length); }

// --- SQL-client modules -----------------------------------------------------

// BNF: <SQL-client module definition>: its name and character set, language, authorization, path, transform groups,
// collations, temporary tables, and what it contains.
public sealed record SqlClientModule : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public Identifier? Name { get; init; }
	public CharacterSetName? NamesAre { get; init; }
	public required string Language { get; init; }
	public required EmbeddedAuthorization Authorization { get; init; }
	public PathSpecification? Path { get; init; }
	public TransformGroupSpecification? TransformGroup { get; init; }
	public IReadOnlyList<ModuleCollation> Collations { get; init; } = [];
	public IReadOnlyList<Statement.DeclareLocalTemporaryTable> TemporaryTables { get; init; } = [];
	public IReadOnlyList<ModuleContent> Contents { get; init; } = [];
}

// BNF: <module contents>: a cursor declared, statically or dynamically, or an <externally-invoked procedure>.
public abstract record ModuleContent : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Cursor(Statement.DeclareCursor Declaration) : ModuleContent;
	public record Procedure(Identifier Name, IReadOnlyList<HostParameterDeclaration> Parameters, Statement Body) : ModuleContent;
}

// BNF: <host parameter declaration>: a host parameter with its type, or the status parameter SQLSTATE.
public abstract record HostParameterDeclaration : ISqlNode
{
	public SqlSpan Span { get; private set; } public void Locate(int at, int length) => Span = new SqlSpan(at, length);
	public record Parameter(Identifier Name, DataType Type, bool Locator = false) : HostParameterDeclaration;
	public record Status : HostParameterDeclaration;
}

// --- Misc ------------------------------------------------------------------
public enum PassThroughMode { PassThrough, NoPassThrough }
public enum TableSemantics { Row, Set }
public enum TablePruning { PruneOnEmpty, KeepOnEmpty }
public enum PadCharacteristic { NoPad, PadSpace }
