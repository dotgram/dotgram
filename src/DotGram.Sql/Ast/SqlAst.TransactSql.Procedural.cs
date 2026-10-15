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
