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
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}
}
