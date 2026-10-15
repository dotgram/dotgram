using System;

namespace DotGram.Sql.Ast;

// T-SQL's routines family: EXECUTE of a module or a string, and (proposal 19) the T-SQL parts of procedures,
// functions and triggers. The standard's Statement.Execute is dynamic SQL's EXECUTE of a prepared statement and
// stays as it is.

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: EXECUTE (Transact-SQL). <c>EXEC [@rc =] module[;n] [argument, …] [AT server] [WITH option, …]</c>: a
	/// procedure called by name or through a variable.
	/// </summary>
	public sealed record ExecuteModule(ModuleReference Module, SqlList<ExecuteArgument> Arguments) : Statement
	{
		/// <summary>
		/// T-SQL: EXECUTE (Transact-SQL). <c>EXEC</c> or <c>EXECUTE</c>; null where the call is written without the
		/// word, which the grammar reads only as a batch's first statement.
		/// </summary>
		[Spelling] public ExecuteKeyword? Keyword { get; init; }

		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>@rc =</c>, the variable the return status goes to, with its @.</summary>
		public Identifier? ReturnStatus { get; init; }

		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>;n</c>, one of a group of procedures sharing a name.</summary>
		public LiteralValue.Numeric? Number { get; init; }

		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>AT linked_server</c>.</summary>
		public Identifier? At { get; init; }

		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>WITH RECOMPILE</c>, <c>WITH RESULT SETS …</c>.</summary>
		public SqlList<ExecuteOption> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: EXECUTE (Transact-SQL). <c>EXEC (string [, value, …]) [AS LOGIN | USER = 'name'] [AT server | AT
	/// DATA_SOURCE source] [WITH option, …]</c>: a string run as a batch.
	/// </summary>
	public sealed record ExecuteString(Expression Sql, SqlList<Expression> Values) : Statement
	{
		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>EXEC</c> written for <c>EXECUTE</c>.</summary>
		[Spelling] public bool Exec { get; init; }

		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>AS LOGIN = 'x'</c> or <c>AS USER = N'u'</c>, a string only.</summary>
		public ExecutionContext? Context { get; init; }

		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>AT linked_server</c>.</summary>
		public Identifier? At { get; init; }

		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>AT DATA_SOURCE data_source</c>.</summary>
		public Identifier? DataSource { get; init; }

		/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>WITH RECOMPILE</c>, <c>WITH RESULT SETS …</c>.</summary>
		public SqlList<ExecuteOption> Options { get; init; } = [];
	}
}

/// <summary>T-SQL: EXECUTE (Transact-SQL). The word a call begins with.</summary>
public enum ExecuteKeyword { Exec, Execute }

/// <summary>T-SQL: EXECUTE (Transact-SQL). The module EXECUTE calls: by its name, or through a variable.</summary>
public abstract record ModuleReference : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>[server.][database.][schema.]name</c>.</summary>
	public sealed record Named(QualifiedName Name) : ModuleReference
	{
		/// <summary>
		/// T-SQL: EXECUTE (Transact-SQL). <c>OPENDATASOURCE (…).db.s.p</c>: the server the procedure is on, with
		/// <see cref="Name"/> the rest of the name after it.
		/// </summary>
		public OpenDataSource? DataSource { get; init; }
	}

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>EXEC @name</c>: a variable holding the module's name, with its @.</summary>
	public sealed record Variable(Identifier Name) : ModuleReference;
}

/// <summary>T-SQL: EXECUTE (Transact-SQL). One value passed: by position or by name, and passed back or not.</summary>
public sealed record ExecuteArgument(Expression Value) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>@parameter =</c>, the parameter named, with its @.</summary>
	public Identifier? Name { get; init; }

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>OUTPUT</c> or <c>OUT</c>; null where neither was written.</summary>
	public OutputKeyword? Output { get; init; }
}

/// <summary>T-SQL: EXECUTE (Transact-SQL). The word that passes a variable back.</summary>
public enum OutputKeyword { Output, Out }

/// <summary>T-SQL: EXECUTE (Transact-SQL). One option after <c>WITH</c>.</summary>
public abstract record ExecuteOption : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>RECOMPILE</c>, or any word the grammar reads there.</summary>
	public sealed record Named(Identifier Name) : ExecuteOption;

	/// <summary>
	/// T-SQL: EXECUTE (Transact-SQL). <c>RESULT SETS UNDEFINED</c>, <c>RESULT SETS NONE</c>, or <c>RESULT SETS (…)</c>
	/// with the sets described, which <see cref="Sets"/> holds exactly then.
	/// </summary>
	public sealed record ResultSets(ResultSetsKind Kind, SqlList<ResultSet> Sets) : ExecuteOption;
}

/// <summary>T-SQL: EXECUTE (Transact-SQL). What <c>RESULT SETS</c> says comes back; <c>None</c> is the word NONE.</summary>
public enum ResultSetsKind { Undefined, None, Described }

/// <summary>T-SQL: EXECUTE (Transact-SQL). One result set described.</summary>
public abstract record ResultSet : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>(column type, …)</c>: the columns, each with its type.</summary>
	public sealed record Columns(SqlList<TableElement> Elements) : ResultSet;

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>AS OBJECT name</c>.</summary>
	public sealed record AsObject(QualifiedName Name) : ResultSet;

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>AS TYPE name</c>.</summary>
	public sealed record AsType(QualifiedName Name) : ResultSet;

	/// <summary>T-SQL: EXECUTE (Transact-SQL). <c>AS FOR XML</c>.</summary>
	public sealed record AsForXml : ResultSet;
}

public abstract partial record InsertSource
{
	/// <summary>T-SQL: INSERT (Transact-SQL). <c>INSERT t EXEC p …</c>: the rows a procedure returns.</summary>
	public sealed record ExecuteModule(Statement.ExecuteModule Statement) : InsertSource;

	/// <summary>T-SQL: INSERT (Transact-SQL). <c>INSERT t EXEC ('…')</c>: the rows a string run returns.</summary>
	public sealed record ExecuteString(Statement.ExecuteString Statement) : InsertSource;
}

public abstract partial record Statement
{
	public partial record CreateRoutine
	{
		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL), CREATE FUNCTION (Transact-SQL). <c>ALTER</c> or <c>CREATE OR ALTER</c>;
		/// null for the plain <c>CREATE</c>, which is the standard's.
		/// </summary>
		public DefinitionVerb? Verb { get; init; }
	}
}

public sealed partial record RoutineDefinition
{
	/// <summary>
	/// T-SQL: CREATE PROCEDURE (Transact-SQL), CREATE FUNCTION (Transact-SQL). <c>WITH option, …</c> before the body.
	/// </summary>
	public SqlList<RoutineOption> Options { get; init; } = [];

	/// <summary>T-SQL: CREATE PROCEDURE (Transact-SQL). <c>FOR REPLICATION</c>.</summary>
	public bool ForReplication { get; init; }

	/// <summary>T-SQL: CREATE PROCEDURE (Transact-SQL). <c>;n</c>, one of a group of procedures sharing a name.</summary>
	public LiteralValue.Numeric? Number { get; init; }

	/// <summary>T-SQL: CREATE PROCEDURE (Transact-SQL). <c>PROC</c> written for <c>PROCEDURE</c>.</summary>
	[Spelling] public bool Proc { get; init; }

	/// <summary>T-SQL: CREATE FUNCTION (Transact-SQL). A CLR table-valued function's <c>ORDER (column, …)</c>.</summary>
	public SqlList<SortItem> Order { get; init; } = [];
}

public abstract partial record RoutineBody
{
	/// <summary>
	/// T-SQL: CREATE PROCEDURE (Transact-SQL), CREATE FUNCTION (Transact-SQL). <c>AS s1; s2 …</c>: the statements, one
	/// at least; <c>AS BEGIN … END</c> is a list of one block.
	/// </summary>
	public sealed record Statements(SqlList<Statement> List) : RoutineBody;

	/// <summary>
	/// T-SQL: CREATE PROCEDURE (Transact-SQL), CREATE FUNCTION (Transact-SQL). <c>AS EXTERNAL NAME assembly.class.method</c>.
	/// </summary>
	public sealed record ExternalName(QualifiedName Name) : RoutineBody;

	/// <summary>
	/// T-SQL: CREATE FUNCTION (Transact-SQL). An inline table-valued function's <c>RETURN query</c>, or <c>RETURN
	/// (query)</c>, whose brackets are a <see cref="Query.Parenthesized"/>.
	/// </summary>
	public sealed record Inline(Statement.Select Query) : RoutineBody;
}

public sealed partial record ReturnsDefinition
{
	/// <summary>T-SQL: CREATE FUNCTION (Transact-SQL). <c>RETURNS @t TABLE (…)</c>: the table's variable, with its @.</summary>
	public Identifier? Variable { get; init; }

	/// <summary>
	/// T-SQL: CREATE FUNCTION (Transact-SQL). The returned table's columns, the table CREATE TABLE's body holds: a table
	/// variable's, or a CLR function's after <c>RETURNS TABLE</c>.
	/// </summary>
	public SqlList<TableElement>? Elements { get; init; }
}

public sealed partial record ParameterDefinition
{
	/// <summary>T-SQL: CREATE PROCEDURE (Transact-SQL). <c>OUT</c> written for <c>OUTPUT</c>, with Mode Out only.</summary>
	[Spelling] public bool ShortOutput { get; init; }

	/// <summary>T-SQL: CREATE PROCEDURE (Transact-SQL). <c>@a AS INT</c>: <c>AS</c> written between the name and the type.</summary>
	[Spelling] public bool AsKeyword { get; init; }
}

/// <summary>
/// T-SQL: CREATE PROCEDURE (Transact-SQL), CREATE FUNCTION (Transact-SQL), CREATE TRIGGER (Transact-SQL). One option of
/// a module's <c>WITH</c>.
/// </summary>
public abstract record RoutineOption : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// T-SQL: CREATE PROCEDURE (Transact-SQL). One word, <c>ENCRYPTION</c>, <c>RECOMPILE</c>, <c>SCHEMABINDING</c>, or one
	/// with a value, <c>INLINE = ON</c>: the grammar's open list.
	/// </summary>
	public sealed record Setting(Option Option) : RoutineOption;

	/// <summary>
	/// T-SQL: EXECUTE AS Clause (Transact-SQL). <c>EXECUTE AS CALLER | SELF | OWNER | 'user'</c>: the kind with no name, or
	/// a user's name as a string.
	/// </summary>
	public sealed record ExecuteAs(ExecutionContext Context) : RoutineOption;

	/// <summary>
	/// T-SQL: CREATE FUNCTION (Transact-SQL). <c>RETURNS NULL ON NULL INPUT</c> or <c>CALLED ON NULL INPUT</c>, written in
	/// the WITH list.
	/// </summary>
	public sealed record NullCall(NullCallMode Mode) : RoutineOption;
}
