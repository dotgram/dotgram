using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using ColumnDefinition = DotGram.Sql.Ast.ColumnDefinition;
using ColumnOption = DotGram.Sql.Ast.ColumnOption;
using Constraint = DotGram.Sql.Ast.Constraint;
using DataType = DotGram.Sql.Ast.DataType;
using ExecutionContext = DotGram.Sql.Ast.ExecutionContext;
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using InsertSource = DotGram.Sql.Ast.InsertSource;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;
using TableElement = DotGram.Sql.Ast.TableElement;

/// <summary>
/// The Routines family's writer (Sql2023Writer.TransactSql.Routines.cs): each record written and read back by
/// TransactSqlParser to the same tree, and every tree whose text would read back as another refused.
/// </summary>
public sealed class RoutinesWriterTests
{
	static readonly Expression.Variable V = new(new Identifier("@a"));
	static readonly Expression One = new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger);
	static readonly QualifiedName P = new([new Identifier("p")]);

	static Identifier Name(string text)
	{
		return new Identifier(text);
	}

	static Expression Text(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static Expression Ref(string word)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(word)]));
	}

	static ExecuteArgument Arg(Expression value)
	{
		return new ExecuteArgument(value);
	}

	/// <summary><c>EXEC p</c> with the arguments given.</summary>
	static Statement.ExecuteModule Exec(params ExecuteArgument[] arguments)
	{
		return new Statement.ExecuteModule(new ModuleReference.Named(P), [.. arguments]) { Keyword = ExecuteKeyword.Exec };
	}

	static Statement.ExecuteString Run(params Expression[] values)
	{
		return new Statement.ExecuteString(Text("SELECT 1"), [.. values]);
	}

	static TableElement Column(string name, DataType? type)
	{
		return new TableElement.Column(new ColumnDefinition(Name(name), type, null, [], null));
	}

	static readonly DataType Int = new DataType.Numeric(NumericTypeKind.Int);

	static readonly Statement Selected = new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(One)] } };

	static readonly RoutineBody Body = new RoutineBody.Statements([new Statement.Return()]);

	static ParameterDefinition Parameter(string name = "@a", DataType? type = null)
	{
		return new ParameterDefinition(null, Name(name), type ?? Int, false, null);
	}

	/// <summary>T-SQL's <c>CREATE PROCEDURE p … AS RETURN</c>, or with the body given.</summary>
	static Statement.CreateRoutine Procedure(SqlList<ParameterDefinition> parameters, RoutineBody? body = null)
	{
		return new Statement.CreateRoutine { Definition = new RoutineDefinition(RoutineKind.Procedure, P, parameters, null, [], body ?? Body) };
	}

	static Statement.CreateRoutine Function(ReturnsDefinition returns, RoutineBody body, params ParameterDefinition[] parameters)
	{
		return new Statement.CreateRoutine { Definition = new RoutineDefinition(RoutineKind.Function, P, [.. parameters], returns, [], body) };
	}

	static Statement.CreateRoutine With(Statement.CreateRoutine routine, params RoutineOption[] options)
	{
		return routine with { Definition = routine.Definition with { Options = [.. options] } };
	}

	static RoutineOption Setting(string name)
	{
		return new RoutineOption.Setting(new Option(Name(name), null, []));
	}

	static readonly DataType Cursor = new DataType.Named(new QualifiedName([new Identifier("CURSOR")]), []);
	static readonly ReturnsDefinition Scalar = new(Int, null);
	static readonly ReturnsDefinition Table = new(null, null) { TableKeyword = true };
	static readonly RoutineBody Inline = new RoutineBody.Inline((Statement.Select)Selected);
	static readonly RoutineBody External = new RoutineBody.ExternalName(new QualifiedName([Name("a"), Name("c"), Name("m")]));

	static Statement.Insert Insert(InsertSource source)
	{
		return new Statement.Insert { Target = new TableSource.Named(new QualifiedName([Name("t")])), SourceValue = source, Into = IntoKeyword.Into };
	}

	/// <summary>A tree for each record of the family, and where it reads: alone, or as a natively compiled body.</summary>
	static Statement Tree(string which)
	{
		return which switch
		{
			"EXEC" => Exec(Arg(One), Arg(Ref("x")), Arg(V) with { Name = Name("@p"), Output = OutputKeyword.Out }),
			"EXECUTE everything" => new Statement.ExecuteModule(new ModuleReference.Named(new QualifiedName([Name("dbo"), Name("p")])), [Arg(new Expression.Default()), Arg(V) with { Output = OutputKeyword.Output }, Arg(new LiteralValue.Numeric("-1.5", NumericLiteralKind.Decimal)), Arg(new LiteralValue.Numeric("$5", NumericLiteralKind.Money)), Arg(new LiteralValue.Null()), Arg(new LiteralValue.Binary("01") { ZeroX = true }), Arg(new LiteralValue.String("'x'", StringLiteralKind.National))]) { Keyword = ExecuteKeyword.Execute, ReturnStatus = Name("@rc"), Number = new LiteralValue.Numeric("2", NumericLiteralKind.DecimalInteger), At = Name("srv"), Options = [new ExecuteOption.Named(Name("RECOMPILE"))] },
			"EXEC a variable" => new Statement.ExecuteModule(new ModuleReference.Variable(Name("@m")), []) { Keyword = ExecuteKeyword.Exec },
			"EXEC on OPENDATASOURCE" => new Statement.ExecuteModule(new ModuleReference.Named(new QualifiedName([Name("db"), Name("dbo"), Name("p")])) { DataSource = new OpenDataSource(Text("P"), Text("s")) }, [Arg(One)]) { Keyword = ExecuteKeyword.Exec },
			"a bare call" => new Statement.ExecuteModule(new ModuleReference.Named(P), [Arg(One)]),
			"RESULT SETS" => Exec() with { Options = [new ExecuteOption.Named(Name("RECOMPILE")), new ExecuteOption.ResultSets(ResultSetsKind.Described, [new ResultSet.Columns([Column("a", Int), Column("b", Int)]), new ResultSet.AsObject(P), new ResultSet.AsType(P), new ResultSet.AsForXml()])] },
			"RESULT SETS NONE" => Exec() with { Options = [new ExecuteOption.ResultSets(ResultSetsKind.None, [])] },
			"RESULT SETS UNDEFINED" => Exec() with { Options = [new ExecuteOption.ResultSets(ResultSetsKind.Undefined, [])] },
			"a string" => Run() with { Context = new ExecutionContext(ExecutionContextKind.User, new LiteralValue.String("'u'", StringLiteralKind.National)) },
			"a string AT" => Run(One, V) with { Exec = true, Context = new ExecutionContext(ExecutionContextKind.Login, Text("l")), At = Name("srv") },
			"a string AT DATA_SOURCE" => Run(One) with { DataSource = Name("ds"), Options = [new ExecuteOption.ResultSets(ResultSetsKind.None, [])] },
			"PROCEDURE" => With(Procedure([Parameter() with { AsKeyword = true, Nullable = false, Default = One }, Parameter("@b") with { Mode = ParameterMode.Out, ShortOutput = true }, Parameter("@c", new DataType.UserDefined(new QualifiedName([Name("tt")]))) with { ReadOnly = true }, Parameter("@d", Cursor) with { Mode = ParameterMode.Out, Varying = true }]), Setting("RECOMPILE"), new RoutineOption.ExecuteAs(new ExecutionContext(ExecutionContextKind.Owner, null))),
			"ALTER PROC" => Procedure([]) with { Verb = DefinitionVerb.Alter, Definition = Procedure([]).Definition with { Proc = true, Number = new LiteralValue.Numeric("2", NumericLiteralKind.DecimalInteger), ForReplication = true } },
			"CREATE OR ALTER PROCEDURE a block" => Procedure([], new RoutineBody.Statements([new Statement.Compound([Selected, new Statement.Return()])])) with { Verb = DefinitionVerb.CreateOrAlter },
			"PROCEDURE EXTERNAL NAME" => With(Procedure([], External), new RoutineOption.ExecuteAs(new ExecutionContext(ExecutionContextKind.User, Text("u")))),
			"PROCEDURE natively compiled" => With(Procedure([], new RoutineBody.Statements([new Statement.Compound([Selected]) { Atomic = [new AtomicOption.Isolation(IsolationLevel.Snapshot), new AtomicOption.Setting(new Option(Name("LANGUAGE"), Text("us_english"), []))] }])), Setting("NATIVE_COMPILATION"), Setting("SCHEMABINDING")),
			"FUNCTION scalar" => With(Function(Scalar, new RoutineBody.Statements([new Statement.Return { Value = One }]), Parameter()), new RoutineOption.NullCall(NullCallMode.ReturnsNullOnNullInput), new RoutineOption.Setting(new Option(Name("INLINE"), Ref("ON"), []) { EqualsOmitted = true })),
			"FUNCTION inline" => Function(Table, Inline),
			"FUNCTION inline in brackets" => Function(Table, new RoutineBody.Inline(new Statement.Select { Query = new Query.Parenthesized(((Statement.Select)Selected).Query) })),
			"FUNCTION table variable" => Function(new ReturnsDefinition(null, null) { Variable = Name("@t"), Elements = [Column("a", Int)] }, new RoutineBody.Statements([new Statement.Return()])),
			"FUNCTION CLR table" => Function(Table with { Elements = [Column("a", Int)] }, External) with { Definition = Function(Table with { Elements = [Column("a", Int)] }, External).Definition with { Order = [new SortItem(Ref("a"), SortDirection.Desc)] } },
			"FUNCTION scalar EXTERNAL NAME" => Function(Scalar, External, Parameter()),
			"INSERT EXEC" => Insert(new InsertSource.ExecuteModule(Exec(Arg(One)))),
			"INSERT EXEC a string" => Insert(new InsertSource.ExecuteString(Run())),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>Each record written as its text.</summary>
	[Theory]
	[InlineData("EXEC", "EXEC p 1, x, @p = @a OUT")]
	[InlineData("EXECUTE everything", "EXECUTE @rc = dbo.p;2 DEFAULT, @a OUTPUT, -1.5, $5, NULL, 0x01, N'x' AT srv WITH RECOMPILE")]
	[InlineData("EXEC a variable", "EXEC @m")]
	[InlineData("EXEC on OPENDATASOURCE", "EXEC OPENDATASOURCE('P', 's').db.dbo.p 1")]
	[InlineData("a bare call", "p 1")]
	[InlineData("RESULT SETS", "EXEC p WITH RECOMPILE, RESULT SETS ((a INT, b INT), AS OBJECT p, AS TYPE p, AS FOR XML)")]
	[InlineData("RESULT SETS NONE", "EXEC p WITH RESULT SETS NONE")]
	[InlineData("RESULT SETS UNDEFINED", "EXEC p WITH RESULT SETS UNDEFINED")]
	[InlineData("a string", "EXECUTE ('SELECT 1') AS USER = N'u'")]
	[InlineData("a string AT", "EXEC ('SELECT 1', 1, @a) AS LOGIN = 'l' AT srv")]
	[InlineData("a string AT DATA_SOURCE", "EXECUTE ('SELECT 1', 1) AT DATA_SOURCE ds WITH RESULT SETS NONE")]
	[InlineData("PROCEDURE", "CREATE PROCEDURE p (@a AS INT NOT NULL = 1, @b INT OUT, @c tt READONLY, @d CURSOR VARYING OUTPUT) WITH RECOMPILE, EXECUTE AS OWNER AS RETURN;")]
	[InlineData("ALTER PROC", "ALTER PROC p;2 FOR REPLICATION AS RETURN;")]
	[InlineData("CREATE OR ALTER PROCEDURE a block", "CREATE OR ALTER PROCEDURE p AS BEGIN SELECT 1; RETURN; END;")]
	[InlineData("PROCEDURE EXTERNAL NAME", "CREATE PROCEDURE p WITH EXECUTE AS 'u' AS EXTERNAL NAME a.c.m")]
	[InlineData("PROCEDURE natively compiled", "CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = 'us_english') SELECT 1; END;")]
	[InlineData("FUNCTION scalar", "CREATE FUNCTION p (@a INT) RETURNS INT WITH RETURNS NULL ON NULL INPUT, INLINE ON AS RETURN 1;")]
	[InlineData("FUNCTION inline", "CREATE FUNCTION p () RETURNS TABLE AS RETURN SELECT 1")]
	[InlineData("FUNCTION inline in brackets", "CREATE FUNCTION p () RETURNS TABLE AS RETURN (SELECT 1)")]
	[InlineData("FUNCTION table variable", "CREATE FUNCTION p () RETURNS @t TABLE (a INT) AS RETURN;")]
	[InlineData("FUNCTION CLR table", "CREATE FUNCTION p () RETURNS TABLE (a INT) ORDER (a DESC) AS EXTERNAL NAME a.c.m")]
	[InlineData("FUNCTION scalar EXTERNAL NAME", "CREATE FUNCTION p (@a INT) RETURNS INT AS EXTERNAL NAME a.c.m")]
	[InlineData("INSERT EXEC", "INSERT INTO t EXEC p 1")]
	[InlineData("INSERT EXEC a string", "INSERT INTO t EXECUTE ('SELECT 1')")]
	public void Each_record_writes_its_text(string which, string text)
	{
		Assert.Equal(text, Sql2023Writer.Write(Tree(which)));
	}

	/// <summary>
	/// Each record's tree, written and read back by TransactSqlParser, carried by the bridge to the same tree.
	/// </summary>
	[Theory]
	[InlineData("EXEC")]
	[InlineData("EXECUTE everything")]
	[InlineData("EXEC a variable")]
	[InlineData("EXEC on OPENDATASOURCE")]
	[InlineData("a bare call")]
	[InlineData("RESULT SETS")]
	[InlineData("RESULT SETS NONE")]
	[InlineData("RESULT SETS UNDEFINED")]
	[InlineData("a string")]
	[InlineData("a string AT")]
	[InlineData("a string AT DATA_SOURCE")]
	[InlineData("PROCEDURE")]
	[InlineData("ALTER PROC")]
	[InlineData("CREATE OR ALTER PROCEDURE a block")]
	[InlineData("PROCEDURE EXTERNAL NAME")]
	[InlineData("PROCEDURE natively compiled")]
	[InlineData("FUNCTION scalar")]
	[InlineData("FUNCTION inline")]
	[InlineData("FUNCTION inline in brackets")]
	[InlineData("FUNCTION table variable")]
	[InlineData("FUNCTION CLR table")]
	[InlineData("FUNCTION scalar EXTERNAL NAME")]
	[InlineData("INSERT EXEC")]
	[InlineData("INSERT EXEC a string")]
	public void Each_record_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = Sql2023Writer.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's message
	/// that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("a bare call inside a block")]
	[InlineData("a bare call in an INSERT")]
	[InlineData("a bare call AT a server")]
	[InlineData("a bare call of a variable")]
	[InlineData("a module variable with a number")]
	[InlineData("a number that is not digits")]
	[InlineData("a return status that is no variable")]
	[InlineData("an argument that is an expression")]
	[InlineData("an argument of two parts")]
	[InlineData("a hexadecimal number")]
	[InlineData("OUTPUT on a number")]
	[InlineData("AT by position")]
	[InlineData("an argument named without @")]
	[InlineData("OPENDATASOURCE of a variable")]
	[InlineData("a string AT and AT DATA_SOURCE")]
	[InlineData("a string's values with no server")]
	[InlineData("a string AS CALLER")]
	[InlineData("a string AS USER of a variable")]
	[InlineData("RESULT SETS described of nothing")]
	[InlineData("RESULT SETS NONE with sets")]
	[InlineData("a result column with no type")]
	[InlineData("a result set of no column")]
	[InlineData("T-SQL members on the standard's body")]
	[InlineData("a characteristic beside T-SQL's body")]
	[InlineData("the standard's ON NULL INPUT in WITH on its body")]
	[InlineData("the verb Create")]
	[InlineData("a procedure returning")]
	[InlineData("a procedure's inline query")]
	[InlineData("RECOMPILE beside EXTERNAL NAME")]
	[InlineData("a procedure number that is not digits")]
	[InlineData("NATIVE_COMPILATION over statements")]
	[InlineData("an atomic block without NATIVE_COMPILATION")]
	[InlineData("a body of nothing")]
	[InlineData("a parameter with no @")]
	[InlineData("a parameter IN")]
	[InlineData("OUT without Mode Out")]
	[InlineData("a cursor parameter not VARYING")]
	[InlineData("a cursor parameter not OUTPUT")]
	[InlineData("VARYING on an INT")]
	[InlineData("a cursor parameter with a default")]
	[InlineData("a function's cursor parameter")]
	[InlineData("a function FOR REPLICATION")]
	[InlineData("a function returning nothing")]
	[InlineData("a scalar function's inline query")]
	[InlineData("RETURNS TABLE over statements")]
	[InlineData("an inline function told of null input")]
	[InlineData("a CLR table over statements")]
	[InlineData("a table variable's sparse column")]
	[InlineData("a table variable's named constraint")]
	[InlineData("ORDER on a scalar function")]
	[InlineData("EXECUTE AS LOGIN")]
	[InlineData("EXECUTE AS a variable")]
	[InlineData("a setting named EXECUTE")]
	public void A_routine_tree_with_no_text_is_refused(string which)
	{
		ISqlNode node = which switch
		{
			// BareCall: only a batch's first statement (FirstPiece).
			"a bare call inside a block" => new Statement.Compound([new Statement.ExecuteModule(new ModuleReference.Named(P), [])]),
			// TSqlInsert: EXEC before the call.
			"a bare call in an INSERT" => Insert(new InsertSource.ExecuteModule(new Statement.ExecuteModule(new ModuleReference.Named(P), []))),
			// BareCall reads no AT; the engine refuses it there.
			"a bare call AT a server" => new Statement.ExecuteModule(new ModuleReference.Named(P), []) { At = Name("srv") },
			// BareCall: ?!TSql.Variable, @p 1 is Msg 102.
			"a bare call of a variable" => new Statement.ExecuteModule(new ModuleReference.Variable(Name("@m")), []),
			// ExecTarget: ExecNumber follows a name only.
			"a module variable with a number" => new Statement.ExecuteModule(new ModuleReference.Variable(Name("@m")), []) { Keyword = ExecuteKeyword.Exec, Number = new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger) },
			// ExecNumber: Digits, ;1.5 is Msg 102.
			"a number that is not digits" => Exec() with { Number = new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal) },
			// ExecReturn: TSql.Variable.
			"a return status that is no variable" => Exec() with { ReturnStatus = Name("rc") },
			// ExecValue: not an expression, 1 + 1 is refused.
			"an argument that is an expression" => Exec(Arg(new Expression.Binary(One, BinaryOperator.Add, One))),
			// ExecValue: a name of one part.
			"an argument of two parts" => Exec(Arg(new Expression.Reference(new QualifiedName([Name("a"), Name("b")])))),
			// DbccValue: UnsignedNumericLiteral or money.
			"a hexadecimal number" => Exec(Arg(new LiteralValue.Numeric("0x1F", NumericLiteralKind.HexInteger))),
			// Syntax.Passes, Msg 179.
			"OUTPUT on a number" => Exec(Arg(One) with { Output = OutputKeyword.Output }),
			// ExecArgument: ?!AT, the word reads as the AT clause.
			"AT by position" => Exec(Arg(Ref("at"))),
			// ExecArgument: TSql.Variable before =.
			"an argument named without @" => Exec(Arg(One) with { Name = Name("p") }),
			// AdHocServer: BrokerString.
			"OPENDATASOURCE of a variable" => new Statement.ExecuteModule(new ModuleReference.Named(P) { DataSource = new OpenDataSource(V, Text("s")) }, []) { Keyword = ExecuteKeyword.Exec },
			// ExecuteBody: source is null || at is null.
			"a string AT and AT DATA_SOURCE" => Run() with { At = Name("srv"), DataSource = Name("ds") },
			// ExecuteBody: values only AT a server or a data source.
			"a string's values with no server" => Run(One),
			// ExecContextKind: LOGIN or USER.
			"a string AS CALLER" => Run() with { Context = new ExecutionContext(ExecutionContextKind.Caller, null) },
			// ExecContextName: a string.
			"a string AS USER of a variable" => Run() with { Context = new ExecutionContext(ExecutionContextKind.User, V) },
			// ResultSets: ( ResultSet , ... ).
			"RESULT SETS described of nothing" => Exec() with { Options = [new ExecuteOption.ResultSets(ResultSetsKind.Described, [])] },
			// ResultSets: NONE alone.
			"RESULT SETS NONE with sets" => Exec() with { Options = [new ExecuteOption.ResultSets(ResultSetsKind.None, [new ResultSet.AsForXml()])] },
			// ResultColumns: Syntax.Typed.
			"a result column with no type" => Exec() with { Options = [new ExecuteOption.ResultSets(ResultSetsKind.Described, [new ResultSet.Columns([Column("a", null)])])] },
			// ResultColumns: TableBody, one element.
			"a result set of no column" => Exec() with { Options = [new ExecuteOption.ResultSets(ResultSetsKind.Described, [new ResultSet.Columns([])])] },
			// RoutineBody.Sql is the standard's; ALTER PROCEDURE is T-SQL's.
			"T-SQL members on the standard's body" => Procedure([]) with { Verb = DefinitionVerb.Alter, Definition = Procedure([]).Definition with { Body = new RoutineBody.Sql(new Statement.Return()) } },
			// ProcedureStatement: options in WITH, never a characteristic.
			"a characteristic beside T-SQL's body" => Procedure([]) with { Definition = Procedure([]).Definition with { Characteristics = [new RoutineCharacteristic.NullCall(NullCallMode.CalledOnNullInput)] } },
			// the standard's routine characteristic is not written in WITH.
			"the standard's ON NULL INPUT in WITH on its body" => With(Procedure([]) with { Definition = Procedure([]).Definition with { Body = new RoutineBody.Sql(new Statement.Return()) } }, new RoutineOption.NullCall(NullCallMode.CalledOnNullInput)),
			// the same text as null.
			"the verb Create" => Procedure([]) with { Verb = DefinitionVerb.Create },
			// ProcedureStatement has no RETURNS.
			"a procedure returning" => Procedure([]) with { Definition = Procedure([]).Definition with { Returns = Scalar } },
			// InlineReturn is FunctionReturns' only.
			"a procedure's inline query" => Procedure([], Inline),
			// Syntax.Externally, Msg 155.
			"RECOMPILE beside EXTERNAL NAME" => With(Procedure([], External), Setting("RECOMPILE")),
			// ProcedureStatement: ; Digits.
			"a procedure number that is not digits" => Procedure([]) with { Definition = Procedure([]).Definition with { Number = new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal) } },
			// Syntax.Natively, Msg 10783.
			"NATIVE_COMPILATION over statements" => With(Procedure([]), Setting("NATIVE_COMPILATION")),
			// Syntax.Natively, Msg 10782.
			"an atomic block without NATIVE_COMPILATION" => Procedure([], new RoutineBody.Statements([new Statement.Compound([Selected]) { Atomic = [new AtomicOption.Isolation(IsolationLevel.Snapshot), new AtomicOption.Setting(new Option(Name("LANGUAGE"), Text("us_english"), []))] }])),
			// StatementList: one statement at least.
			"a body of nothing" => Procedure([], new RoutineBody.Statements([])),
			// RoutineParameter: TSql.Variable.
			"a parameter with no @" => Procedure([Parameter("a")]),
			// ParameterWay: OUTPUT, OUT, READONLY.
			"a parameter IN" => Procedure([Parameter() with { Mode = ParameterMode.In }]),
			// ShortOutput is the spelling of Out.
			"OUT without Mode Out" => Procedure([Parameter() with { ShortOutput = true }]),
			// ParameterTail: CURSOR VARYING, Msg 1051.
			"a cursor parameter not VARYING" => Procedure([Parameter("@c", Cursor) with { Mode = ParameterMode.Out }]),
			// ParameterTail: CURSOR VARYING OUTPUT, Msg 1051.
			"a cursor parameter not OUTPUT" => Procedure([Parameter("@c", Cursor) with { Varying = true }]),
			// ParameterTail: VARYING is a cursor's.
			"VARYING on an INT" => Procedure([Parameter() with { Varying = true, Mode = ParameterMode.Out }]),
			// ParameterTail: a cursor takes no default.
			"a cursor parameter with a default" => Procedure([Parameter("@c", Cursor) with { Varying = true, Mode = ParameterMode.Out, Default = One }]),
			// Syntax.HasCursorParameter, Msg 5306.
			"a function's cursor parameter" => Function(Scalar, Body, Parameter("@c", Cursor) with { Varying = true, Mode = ParameterMode.Out }),
			// FunctionStatement has no FOR REPLICATION.
			"a function FOR REPLICATION" => Function(Scalar, Body) with { Definition = Function(Scalar, Body).Definition with { ForReplication = true } },
			// FunctionStatement: RETURNS.
			"a function returning nothing" => Function(null!, Body),
			// FunctionReturns: a type and a body.
			"a scalar function's inline query" => Function(Scalar, Inline),
			// FunctionReturns: TABLE and RETURN a query.
			"RETURNS TABLE over statements" => Function(Table, Body),
			// Syntax.Inlined, Msg 487.
			"an inline function told of null input" => With(Function(Table, Inline), new RoutineOption.NullCall(NullCallMode.CalledOnNullInput)),
			// FunctionReturns: TABLE (…) AS EXTERNAL NAME.
			"a CLR table over statements" => Function(Table with { Elements = [Column("a", Int)] }, Body),
			// Syntax.Returned, Msg 102.
			"a table variable's sparse column" => Function(new ReturnsDefinition(null, null) { Variable = Name("@t"), Elements = [new TableElement.Column(new ColumnDefinition(Name("a"), Int, null, [], null) { Options = [new ColumnOption.Sparse()] })] }, Body),
			// Syntax.Declared.
			"a table variable's named constraint" => Function(new ReturnsDefinition(null, null) { Variable = Name("@t"), Elements = [Column("a", Int), new TableElement.TableConstraint(new Constraint.Check(new Expression.Comparison(Ref("a"), ComparisonOperator.Greater, One)) { Name = new QualifiedName([Name("k")]) })] }, Body),
			// FunctionReturns: ORDER is a CLR table's.
			"ORDER on a scalar function" => Function(Scalar, External) with { Definition = Function(Scalar, External).Definition with { Order = [new SortItem(Ref("a"))] } },
			// ExecuteAsWho: CALLER, SELF, OWNER or a string.
			"EXECUTE AS LOGIN" => With(Procedure([]), new RoutineOption.ExecuteAs(new ExecutionContext(ExecutionContextKind.Login, Text("l")))),
			// ExecuteAsWho: a string.
			"EXECUTE AS a variable" => With(Procedure([]), new RoutineOption.ExecuteAs(new ExecutionContext(ExecutionContextKind.User, V))),
			// RoutineOption: EXECUTE begins EXECUTE AS.
			"a setting named EXECUTE" => With(Procedure([]), Setting("EXECUTE")),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}
}
