using System;

namespace DotGram.Sql.Ast;

// T-SQL's procedural statements (proposal 25): SQL/PSM's names where SQL/PSM has the statement — a compound statement,
// IF, WHILE — and T-SQL's own for the rest. A word that names what is a record; a word that says how is a member.

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: BEGIN...END (Transact-SQL), CREATE PROCEDURE (Transact-SQL). <c>BEGIN s; … END</c>, a block of statements;
	/// with <see cref="Atomic"/> set, <c>BEGIN ATOMIC WITH (…) s; … END</c>, a natively compiled module's body.
	/// </summary>
	public sealed record Compound(SqlList<Statement> Statements) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). <c>ATOMIC WITH (…)</c>, a natively compiled module's settings; null for a
		/// plain block.
		/// </summary>
		public SqlList<AtomicOption>? Atomic { get; init; }
	}

	/// <summary>T-SQL: IF...ELSE (Transact-SQL). <c>IF condition s [ELSE s]</c>.</summary>
	public sealed record If(Expression Condition, Statement Then, Statement? Else) : Statement;

	/// <summary>T-SQL: WHILE (Transact-SQL). <c>WHILE condition s</c>.</summary>
	public sealed record While(Expression Condition, Statement Body) : Statement;

	/// <summary>
	/// T-SQL: TRY...CATCH (Transact-SQL). <c>BEGIN TRY s; … END TRY BEGIN CATCH [s; …] END CATCH</c>: the statements
	/// tried, one at least, and those that handle an error, which may be none.
	/// </summary>
	public sealed record TryCatch(SqlList<Statement> Try, SqlList<Statement> Catch) : Statement;

	/// <summary>T-SQL: BREAK (no syntax block in syntax.md). <c>BREAK</c>, out of the innermost WHILE.</summary>
	public sealed record Break : Statement;

	/// <summary>T-SQL: CONTINUE (no syntax block in syntax.md). <c>CONTINUE</c>, to the next turn of the innermost WHILE.</summary>
	public sealed record Continue : Statement;

	/// <summary>T-SQL: GOTO (Transact-SQL). <c>GOTO label</c>.</summary>
	public sealed record GoTo(Identifier Name) : Statement;

	/// <summary>T-SQL: GOTO (Transact-SQL). <c>label:</c>, a place a GOTO goes to.</summary>
	public sealed record Label(Identifier Name) : Statement;

	/// <summary>
	/// T-SQL: DECLARE @local_variable (Transact-SQL). <c>DECLARE @a INT = 1, @t TABLE (…)</c>: variables declared, one at
	/// least, in the order written.
	/// </summary>
	public sealed record DeclareVariables(SqlList<VariableDeclaration> Variables) : Statement;

	/// <summary>
	/// T-SQL: SET @local_variable (Transact-SQL). <c>SET @v = value</c>, <c>SET @v += value</c>, a property of the
	/// variable assigned, <c>SET @g.a.b = value</c>, or a method of it called, <c>SET @x.modify('…')</c>: then
	/// <see cref="Operator"/> is null, <see cref="Members"/> empty and <see cref="Value"/> the call on the variable.
	/// </summary>
	public sealed record SetVariable(Expression.Variable Variable, Expression Value) : Statement
	{
		/// <summary>T-SQL: SET @local_variable (Transact-SQL). <c>=</c>, <c>+=</c>, …; null where a method is called.</summary>
		public AssignmentOperator? Operator { get; init; } = AssignmentOperator.Assign;

		/// <summary>T-SQL: SET @local_variable (Transact-SQL). The property assigned, <c>@g.a.b</c>: <c>a</c>, <c>b</c>.</summary>
		public SqlList<Identifier> Members { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: SET Statements (Transact-SQL). <c>SET option …</c>: the options of one form, switches sharing an ON or OFF,
	/// STATISTICS or OFFSETS words, IDENTITY_INSERT, or settings given values. <c>SET TRANSACTION ISOLATION LEVEL</c>
	/// is the standard's <see cref="SetTransaction"/>.
	/// </summary>
	public sealed record SetOptions(SqlList<SetOption> Options) : Statement;

	/// <summary>T-SQL: PRINT (Transact-SQL). <c>PRINT value</c>.</summary>
	public sealed record Print(Expression Value) : Statement;

	/// <summary>
	/// T-SQL: RAISERROR (Transact-SQL). <c>RAISERROR (message, severity, state [, argument …]) [WITH option, …]</c>: the
	/// arguments as the grammar reads them, one at least, and the options in the order written.
	/// </summary>
	public sealed record RaiseError(SqlList<Expression> Arguments) : Statement
	{
		/// <summary>T-SQL: RAISERROR (Transact-SQL). <c>WITH LOG, NOWAIT, SETERROR</c>, in the order written.</summary>
		public SqlList<RaiseErrorOption> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: THROW (Transact-SQL). <c>THROW [number, message, state]</c>: all three, or none to throw again the error a
	/// CATCH caught.
	/// </summary>
	public sealed record Throw(Expression? Number, Expression? Message, Expression? State) : Statement;

	/// <summary>T-SQL: WAITFOR (Transact-SQL). <c>WAITFOR DELAY value</c> or <c>WAITFOR TIME value</c>.</summary>
	public sealed record WaitFor(WaitForKind Kind, Expression Value) : Statement;

	/// <summary>
	/// T-SQL: EXECUTE AS (Transact-SQL). <c>EXECUTE AS { CALLER | USER = name | LOGIN = name } [WITH NO REVERT | WITH
	/// COOKIE INTO @c]</c>: whom the session runs as from here.
	/// </summary>
	public sealed record ExecuteAs(ExecutionContext Context) : Statement
	{
		/// <summary>T-SQL: EXECUTE AS (Transact-SQL). <c>WITH NO REVERT</c>.</summary>
		public bool NoRevert { get; init; }

		/// <summary>T-SQL: EXECUTE AS (Transact-SQL). <c>WITH COOKIE INTO @c</c>: the variable the cookie goes into.</summary>
		public Expression.Variable? Cookie { get; init; }

		/// <summary>T-SQL: EXECUTE AS (Transact-SQL). <c>EXEC</c> written for <c>EXECUTE</c> (SessionStatement's ExecWord).</summary>
		[Spelling] public bool Exec { get; init; }
	}

	/// <summary>T-SQL: REVERT (Transact-SQL). <c>REVERT [WITH COOKIE = value]</c>.</summary>
	public sealed record Revert(Expression? Cookie) : Statement;

	/// <summary>T-SQL: SETUSER (Transact-SQL). <c>SETUSER [name [WITH NORESET]]</c>.</summary>
	public sealed record SetUser(Expression? Name) : Statement
	{
		/// <summary>T-SQL: SETUSER (Transact-SQL). <c>WITH NORESET</c>.</summary>
		public bool NoReset { get; init; }
	}

	/// <summary>T-SQL: USE (Transact-SQL). <c>USE database</c>.</summary>
	public sealed record Use(Identifier Database) : Statement;

	/// <summary>T-SQL: LINENO (no syntax block in syntax.md). <c>LINENO n</c>: the line the errors after it are counted from.</summary>
	public sealed record LineNumber(LiteralValue.Numeric Line) : Statement;
}

/// <summary>
/// T-SQL: DECLARE @local_variable (Transact-SQL). One variable declared: of a type, and given a value or not, or a
/// table.
/// </summary>
public abstract record VariableDeclaration : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: DECLARE @local_variable (Transact-SQL). <c>@v [AS] type [= value]</c>; <c>@c [AS] CURSOR</c> is the type
	/// <see cref="DataType.Named"/> of the one-part name <c>CURSOR</c>, with no value.
	/// </summary>
	public sealed record Typed(Identifier Name, DataType Type, Expression? Value) : VariableDeclaration
	{
		/// <summary>T-SQL: DECLARE @local_variable (Transact-SQL). <c>AS</c> written between the name and the type.</summary>
		[Spelling] public bool AsKeyword { get; init; }
	}

	/// <summary>
	/// T-SQL: DECLARE @local_variable (Transact-SQL). <c>@t [AS] TABLE (…)</c>: the columns, constraints and indexes
	/// CREATE TABLE's body holds, but none named a constraint.
	/// </summary>
	public sealed record Table(Identifier Name, SqlList<TableElement> Elements) : VariableDeclaration
	{
		/// <summary>T-SQL: DECLARE @local_variable (Transact-SQL). <c>AS</c> written between the name and <c>TABLE</c>.</summary>
		[Spelling] public bool AsKeyword { get; init; }
	}
}

/// <summary>
/// T-SQL: CREATE PROCEDURE (Transact-SQL). One setting of <c>BEGIN ATOMIC WITH (…)</c>: the isolation level, or one of the
/// others, a name and a value.
/// </summary>
public abstract record AtomicOption : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: CREATE PROCEDURE (Transact-SQL). <c>TRANSACTION ISOLATION LEVEL = SNAPSHOT</c>.</summary>
	public sealed record Isolation(IsolationLevel Level) : AtomicOption
	{
		/// <summary>T-SQL: CREATE PROCEDURE (Transact-SQL). <c>TRAN</c> written for <c>TRANSACTION</c>.</summary>
		[Spelling] public bool Tran { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE PROCEDURE (Transact-SQL). <c>DATEFIRST</c>, <c>DATEFORMAT</c>, <c>LANGUAGE</c>, <c>TEXTSIZE</c> or
	/// <c>DELAYED_DURABILITY</c>, and its value.
	/// </summary>
	public sealed record Setting(Option Option) : AtomicOption;
}

/// <summary>T-SQL: RAISERROR (Transact-SQL). An option after <c>WITH</c>.</summary>
public enum RaiseErrorOption { Log, NoWait, SetError }

/// <summary>T-SQL: WAITFOR (Transact-SQL). Whether the value is how long to wait, or until when.</summary>
public enum WaitForKind { Delay, Time }
