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
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;
using TableElement = DotGram.Sql.Ast.TableElement;

/// <summary>
/// The Procedural family's writer (SqlWriter.TransactSql.Procedural.cs): each record written and read back by
/// TransactSqlParser to the same tree, and every tree whose text would read back as another refused.
/// </summary>
public sealed class ProceduralWriterTests
{
	static readonly Expression.Variable V = new(new Identifier("@a"));
	static readonly Expression One = new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger);

	/// <summary><c>@a &lt; 1</c>.</summary>
	static readonly Expression Less = new Expression.Comparison(V, ComparisonOperator.Less, One);

	/// <summary><c>SELECT 1</c>.</summary>
	static readonly Statement Selected = new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(One)] } };

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static Expression Ref(string word)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(word)]));
	}

	static AtomicOption Set(string name, Expression value)
	{
		return new AtomicOption.Setting(new Option(new Identifier(name), value, []));
	}

	/// <summary>A natively compiled block over <c>SELECT 1</c> with the settings given.</summary>
	static Statement.Compound Atomic(params AtomicOption[] options)
	{
		return new Statement.Compound([Selected]) { Atomic = [.. options] };
	}

	static readonly DataType Int = new DataType.Numeric(NumericTypeKind.Int);
	static readonly DataType Cursor = new DataType.Named(new QualifiedName([new Identifier("CURSOR")]), []);

	static Identifier Name(string text)
	{
		return new Identifier(text);
	}

	/// <summary>A column of a type, with options.</summary>
	static TableElement Column(string name, DataType? type, params ColumnOption[] options)
	{
		return new TableElement.Column(new ColumnDefinition(Name(name), type, null, [], null) { Options = [.. options] });
	}

	/// <summary><c>@t TABLE (c INT, …)</c> with the elements given after <c>c INT</c>.</summary>
	static Statement.DeclareVariables Table(params TableElement[] more)
	{
		return new Statement.DeclareVariables([new VariableDeclaration.Table(Name("@t"), [Column("c", Int), .. more])]);
	}

	static Statement.DeclareVariables Declare(DataType type, Expression? value)
	{
		return new Statement.DeclareVariables([new VariableDeclaration.Typed(Name("@a"), type, value)]);
	}

	/// <summary><c>@x.m(1)</c>.</summary>
	static Expression Call(Expression.Variable on)
	{
		return new Expression.Member(on, MemberAccessKind.Dot, Name("m"), [new Argument(One)]);
	}

	static SetOption Val(string name, Expression argument)
	{
		return new SetOption.Value(Name(name), argument);
	}

	static Statement.SetOptions Options(params SetOption[] options)
	{
		return new Statement.SetOptions([.. options]);
	}

	static readonly Expression MinusOne = Number("-1");

	static readonly CursorReference C = new(new QualifiedName([new Identifier("c")]));

	/// <summary><c>SELECT 1</c> as a cursor's source, its FOR and its hints as given.</summary>
	static CursorSource.Query Over(UpdatabilityClause? updatability = null, bool recompiled = false, bool before = false)
	{
		var select = new Statement.Select { Query = ((Statement.Select)Selected).Query, Hints = recompiled ? [new QueryHint.Named(Name("RECOMPILE"), [])] : [] };

		return new CursorSource.Query(select, updatability, before);
	}

	static readonly UpdatabilityClause ReadOnly = new(true, []);

	static Statement.DeclareCursor Declared(CursorProperties properties, CursorSource.Query source, bool extended = true)
	{
		return new Statement.DeclareCursor { Cursor = C, Properties = properties, SourceValue = source, Extended = extended };
	}

	static CursorProperties Words(CursorScrollability? scrollability = null)
	{
		return new CursorProperties(null, scrollability, null, null);
	}

	static Expression Text(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static readonly AtomicOption Level = new AtomicOption.Isolation(IsolationLevel.Snapshot);
	static readonly AtomicOption English = Set("LANGUAGE", new LiteralValue.String("'us_english'", StringLiteralKind.National));

	/// <summary>A tree for each record of the family, and where it reads: alone, or as a natively compiled body.</summary>
	static Statement Tree(string which)
	{
		return which switch
		{
			"Block" => new Statement.Compound([Selected, new Statement.Break()]),
			"Nested IF and ELSE" => new Statement.If(Less, new Statement.If(Less, Selected, new Statement.Break()), new Statement.Continue()),
			"IF in an ELSE" => new Statement.If(Less, Selected, new Statement.If(Less, new Statement.Break(), null)),
			"WHILE over a block" => new Statement.While(Less, new Statement.Compound([Selected, new Statement.Continue()])),
			"TRY inside IF" => new Statement.If(Less, new Statement.TryCatch([Selected], [new Statement.Break()]), null),
			"TRY with an empty CATCH" => new Statement.TryCatch([Selected], []),
			"GOTO" => new Statement.GoTo(new Identifier("done")),
			"Label" => new Statement.Label(new Identifier("done")),
			"Atomic" => Atomic(new AtomicOption.Isolation(IsolationLevel.RepeatableRead) { Tran = true }, English, Set("DATEFIRST", Number("7")), Set("TEXTSIZE", new Expression.Unary(UnaryOperator.Minus, Number("1"))), Set("DELAYED_DURABILITY", Ref("ON"))),
			"DECLARE" => new Statement.DeclareVariables([new VariableDeclaration.Typed(Name("@a"), Int, One) { AsKeyword = true }, new VariableDeclaration.Typed(Name("@b"), new DataType.Character(CharacterTypeKind.Varchar, 10), null), new VariableDeclaration.Typed(Name("@c"), Cursor, null) { AsKeyword = true }, new VariableDeclaration.Typed(Name("@d"), Int, new LiteralValue.Null())]),
			"DECLARE TABLE" => Table(Column("d", null, new ColumnOption.Computed(One)), Column("e", Int, new ColumnOption.Constraint(new Constraint.NotNull()))),
			"DECLARE AS TABLE" => new Statement.DeclareVariables([new VariableDeclaration.Table(Name("@t"), [Column("c", Int)]) { AsKeyword = true }]),
			"SET" => new Statement.SetVariable(V, One),
			"SET +=" => new Statement.SetVariable(V, One) { Operator = AssignmentOperator.Add },
			"SET a property" => new Statement.SetVariable(V, One) { Members = [Name("x"), Name("y")] },
			"SET a method" => new Statement.SetVariable(V, Call(V)) { Operator = null },
			"SET switches" => Options(new SetOption.Switch(Name("ANSI_NULLS"), true), new SetOption.Switch(Name("nocount"), true)),
			"SET STATISTICS" => Options(new SetOption.Statistics(StatisticsKind.Io, false), new SetOption.Statistics(StatisticsKind.Profile, false), new SetOption.Statistics(StatisticsKind.Time, false), new SetOption.Statistics(StatisticsKind.Xml, false)),
			"SET OFFSETS" => Options(new SetOption.Offsets(Name("SELECT"), true), new SetOption.Offsets(Name("proc"), true)),
			"SET IDENTITY_INSERT" => Options(new SetOption.IdentityInsert(new QualifiedName([Name("dbo"), Name("t")]), false)),
			"SET values" => Options(Val("DATEFIRST", Number("7")), Val("LANGUAGE", Ref("us_english")), Val("DATEFIRST", MinusOne), Val("DEADLOCK_PRIORITY", V), Val("CONTEXT_INFO", new LiteralValue.Binary("01") { ZeroX = true }), Val("FIPS_FLAGGER", new LiteralValue.String("'FULL'", StringLiteralKind.Character)), Val("QUERY_GOVERNOR_COST_LIMIT", new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal)), Val("LOCK_TIMEOUT", MinusOne), Val("DATEFORMAT", new LiteralValue.String("'dmy'", StringLiteralKind.National))),
			"SET ROWCOUNT" => Options(Val("ROWCOUNT", V)),
			"SET TEXTSIZE" => Options(Val("TEXTSIZE", MinusOne)),
			"SET ERRLVL" => Options(Val("ERRLVL", Number("5"))),
			"SET FIPS_FLAGGER OFF" => Options(Val("FIPS_FLAGGER", Ref("OFF"))),
			"BEGIN TRAN" => new Statement.BeginTransaction(false, true, new TransactionName(Name("t")), new TransactionMark(new LiteralValue.String("'m'", StringLiteralKind.Character))),
			"BEGIN DISTRIBUTED" => new Statement.BeginTransaction(true, false, new TransactionName(null, V)),
			"BEGIN alone" => new Statement.BeginTransaction(),
			"BEGIN with a bare mark" => new Statement.BeginTransaction(Mark: new TransactionMark()),
			"SAVE" => new Statement.SaveTransaction(new TransactionName(Name("s")), true),
			"DEALLOCATE GLOBAL" => new Statement.DeallocateCursor(C with { Global = true }),
			"DEALLOCATE a variable" => new Statement.DeallocateCursor(new CursorReference(null, V)),
			"DECLARE CURSOR the ISO way" => Declared(new CursorProperties(CursorSensitivity.Insensitive, CursorScrollability.Scroll, null, null), Over(ReadOnly), extended: false),
			"DECLARE CURSOR with no word" => Declared(Words(), Over(), extended: false),
			"DECLARE CURSOR T-SQL's way" => Declared(Words(CursorScrollability.NoScroll) with { Scope = CursorScope.Local, ForwardOnly = true, Type = CursorType.Static, Concurrency = CursorConcurrency.ReadOnly, TypeWarning = true }, Over()),
			"DECLARE CURSOR FOR UPDATE before the hints" => Declared(Words(CursorScrollability.Scroll) with { Scope = CursorScope.Global, Type = CursorType.Keyset, Concurrency = CursorConcurrency.Optimistic }, Over(new UpdatabilityClause(false, [Name("a")]), recompiled: true, before: true)),
			"DECLARE CURSOR FOR UPDATE after the hints" => Declared(Words() with { Type = CursorType.Dynamic, Concurrency = CursorConcurrency.ScrollLocks }, Over(new UpdatabilityClause(false, []), recompiled: true)),
			"PRINT" => new Statement.Print(Text("a")),
			"RAISERROR" => new Statement.RaiseError([Text("m"), Number("16"), One, V]) { Options = [RaiseErrorOption.Log, RaiseErrorOption.NoWait, RaiseErrorOption.SetError] },
			"RAISERROR alone" => new Statement.RaiseError([V]),
			"THROW" => new Statement.Throw(Number("50000"), Text("m"), One),
			"THROW again" => new Statement.Throw(null, null, null),
			"WAITFOR DELAY" => new Statement.WaitFor(WaitForKind.Delay, Text("00:00:01")),
			"WAITFOR TIME" => new Statement.WaitFor(WaitForKind.Time, V),
			"EXECUTE AS CALLER" => new Statement.ExecuteAs(new ExecutionContext(ExecutionContextKind.Caller, null)) { NoRevert = true },
			"EXECUTE AS USER" => new Statement.ExecuteAs(new ExecutionContext(ExecutionContextKind.User, Text("u"))) { Cookie = V },
			"EXECUTE AS LOGIN" => new Statement.ExecuteAs(new ExecutionContext(ExecutionContextKind.Login, V)),
			"REVERT" => new Statement.Revert(V),
			"REVERT alone" => new Statement.Revert(null),
			"SETUSER" => new Statement.SetUser(new LiteralValue.String("'u'", StringLiteralKind.National)) { NoReset = true },
			"SETUSER alone" => new Statement.SetUser(null),
			"USE" => new Statement.Use(Name("master")),
			"LINENO" => new Statement.LineNumber(new LiteralValue.Numeric("7", NumericLiteralKind.DecimalInteger)),
			"SET a cursor" => new Statement.SetCursorVariable(V, Words(CursorScrollability.NoScroll) with { ForwardOnly = true, Type = CursorType.FastForward, Concurrency = CursorConcurrency.ReadOnly }, Over(ReadOnly)),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>Each record written as its text.</summary>
	[Theory]
	[InlineData("Block", "BEGIN SELECT 1; BREAK; END")]
	[InlineData("Nested IF and ELSE", "IF @a < 1 IF @a < 1 SELECT 1 ELSE BREAK ELSE CONTINUE")]
	[InlineData("IF in an ELSE", "IF @a < 1 SELECT 1 ELSE IF @a < 1 BREAK")]
	[InlineData("WHILE over a block", "WHILE @a < 1 BEGIN SELECT 1; CONTINUE; END")]
	[InlineData("TRY inside IF", "IF @a < 1 BEGIN TRY SELECT 1; END TRY BEGIN CATCH BREAK; END CATCH")]
	[InlineData("TRY with an empty CATCH", "BEGIN TRY SELECT 1; END TRY BEGIN CATCH END CATCH")]
	[InlineData("GOTO", "GOTO done")]
	[InlineData("Label", "done:")]
	[InlineData("Atomic", "BEGIN ATOMIC WITH (TRAN ISOLATION LEVEL = REPEATABLE READ, LANGUAGE = N'us_english', DATEFIRST = 7, TEXTSIZE = -1, DELAYED_DURABILITY = ON) SELECT 1; END")]
	[InlineData("DECLARE", "DECLARE @a AS INT = 1, @b VARCHAR(10), @c AS CURSOR, @d INT = NULL")]
	[InlineData("DECLARE TABLE", "DECLARE @t TABLE (c INT, d AS 1, e INT NOT NULL)")]
	[InlineData("DECLARE AS TABLE", "DECLARE @t AS TABLE (c INT)")]
	[InlineData("SET", "SET @a = 1")]
	[InlineData("SET +=", "SET @a += 1")]
	[InlineData("SET a property", "SET @a.x.y = 1")]
	[InlineData("SET a method", "SET @a.m(1)")]
	[InlineData("SET switches", "SET ANSI_NULLS, nocount ON")]
	[InlineData("SET STATISTICS", "SET STATISTICS IO, PROFILE, TIME, XML OFF")]
	[InlineData("SET OFFSETS", "SET OFFSETS SELECT, proc ON")]
	[InlineData("SET IDENTITY_INSERT", "SET IDENTITY_INSERT dbo.t OFF")]
	[InlineData("SET values", "SET DATEFIRST 7, LANGUAGE us_english, DATEFIRST -1, DEADLOCK_PRIORITY @a, CONTEXT_INFO 0x01, FIPS_FLAGGER 'FULL', QUERY_GOVERNOR_COST_LIMIT 1.5, LOCK_TIMEOUT -1, DATEFORMAT N'dmy'")]
	[InlineData("SET ROWCOUNT", "SET ROWCOUNT @a")]
	[InlineData("SET TEXTSIZE", "SET TEXTSIZE -1")]
	[InlineData("SET ERRLVL", "SET ERRLVL 5")]
	[InlineData("SET FIPS_FLAGGER OFF", "SET FIPS_FLAGGER OFF")]
	[InlineData("BEGIN TRAN", "BEGIN TRAN t WITH MARK 'm'")]
	[InlineData("BEGIN DISTRIBUTED", "BEGIN DISTRIBUTED TRANSACTION @a")]
	[InlineData("BEGIN alone", "BEGIN TRANSACTION")]
	[InlineData("BEGIN with a bare mark", "BEGIN TRANSACTION WITH MARK")]
	[InlineData("SAVE", "SAVE TRAN s")]
	[InlineData("DEALLOCATE GLOBAL", "DEALLOCATE GLOBAL c")]
	[InlineData("DEALLOCATE a variable", "DEALLOCATE @a")]
	[InlineData("DECLARE CURSOR the ISO way", "DECLARE c INSENSITIVE SCROLL CURSOR FOR SELECT 1 FOR READ ONLY")]
	[InlineData("DECLARE CURSOR with no word", "DECLARE c CURSOR FOR SELECT 1")]
	[InlineData("DECLARE CURSOR T-SQL's way", "DECLARE c CURSOR LOCAL FORWARD_ONLY STATIC READ_ONLY TYPE_WARNING FOR SELECT 1")]
	[InlineData("DECLARE CURSOR FOR UPDATE before the hints", "DECLARE c CURSOR GLOBAL SCROLL KEYSET OPTIMISTIC FOR SELECT 1 FOR UPDATE OF a OPTION (RECOMPILE)")]
	[InlineData("DECLARE CURSOR FOR UPDATE after the hints", "DECLARE c CURSOR DYNAMIC SCROLL_LOCKS FOR SELECT 1 OPTION (RECOMPILE) FOR UPDATE")]
	[InlineData("PRINT", "PRINT 'a'")]
	[InlineData("RAISERROR", "RAISERROR('m', 16, 1, @a) WITH LOG, NOWAIT, SETERROR")]
	[InlineData("RAISERROR alone", "RAISERROR(@a)")]
	[InlineData("THROW", "THROW 50000, 'm', 1")]
	[InlineData("THROW again", "THROW")]
	[InlineData("WAITFOR DELAY", "WAITFOR DELAY '00:00:01'")]
	[InlineData("WAITFOR TIME", "WAITFOR TIME @a")]
	[InlineData("EXECUTE AS CALLER", "EXECUTE AS CALLER WITH NO REVERT")]
	[InlineData("EXECUTE AS USER", "EXECUTE AS USER = 'u' WITH COOKIE INTO @a")]
	[InlineData("EXECUTE AS LOGIN", "EXECUTE AS LOGIN = @a")]
	[InlineData("REVERT", "REVERT WITH COOKIE = @a")]
	[InlineData("REVERT alone", "REVERT")]
	[InlineData("SETUSER", "SETUSER N'u' WITH NORESET")]
	[InlineData("SETUSER alone", "SETUSER")]
	[InlineData("USE", "USE master")]
	[InlineData("LINENO", "LINENO 7")]
	[InlineData("SET a cursor", "SET @a = CURSOR FORWARD_ONLY FAST_FORWARD READ_ONLY FOR SELECT 1 FOR READ ONLY")]
	public void Each_record_writes_its_text(string which, string text)
	{
		Assert.Equal(text, SqlWriter.Write(Tree(which)));
	}

	/// <summary>
	/// Each record's tree, written and read back by TransactSqlParser, as the same tree. A natively
	/// compiled block reads only as a procedure's body, so it is read inside one.
	/// </summary>
	[Theory]
	[InlineData("Block")]
	[InlineData("Nested IF and ELSE")]
	[InlineData("IF in an ELSE")]
	[InlineData("WHILE over a block")]
	[InlineData("TRY inside IF")]
	[InlineData("TRY with an empty CATCH")]
	[InlineData("GOTO")]
	[InlineData("Label")]
	[InlineData("Atomic")]
	[InlineData("DECLARE")]
	[InlineData("DECLARE TABLE")]
	[InlineData("DECLARE AS TABLE")]
	[InlineData("SET")]
	[InlineData("SET +=")]
	[InlineData("SET a property")]
	[InlineData("SET a method")]
	[InlineData("SET switches")]
	[InlineData("SET STATISTICS")]
	[InlineData("SET OFFSETS")]
	[InlineData("SET IDENTITY_INSERT")]
	[InlineData("SET values")]
	[InlineData("SET ROWCOUNT")]
	[InlineData("SET TEXTSIZE")]
	[InlineData("SET ERRLVL")]
	[InlineData("SET FIPS_FLAGGER OFF")]
	[InlineData("BEGIN TRAN")]
	[InlineData("BEGIN DISTRIBUTED")]
	[InlineData("BEGIN alone")]
	[InlineData("BEGIN with a bare mark")]
	[InlineData("SAVE")]
	[InlineData("DEALLOCATE GLOBAL")]
	[InlineData("DEALLOCATE a variable")]
	[InlineData("DECLARE CURSOR the ISO way")]
	[InlineData("DECLARE CURSOR with no word")]
	[InlineData("DECLARE CURSOR T-SQL's way")]
	[InlineData("DECLARE CURSOR FOR UPDATE before the hints")]
	[InlineData("DECLARE CURSOR FOR UPDATE after the hints")]
	[InlineData("PRINT")]
	[InlineData("RAISERROR")]
	[InlineData("RAISERROR alone")]
	[InlineData("THROW")]
	[InlineData("THROW again")]
	[InlineData("WAITFOR DELAY")]
	[InlineData("WAITFOR TIME")]
	[InlineData("EXECUTE AS CALLER")]
	[InlineData("EXECUTE AS USER")]
	[InlineData("EXECUTE AS LOGIN")]
	[InlineData("REVERT")]
	[InlineData("REVERT alone")]
	[InlineData("SETUSER")]
	[InlineData("SETUSER alone")]
	[InlineData("USE")]
	[InlineData("LINENO")]
	[InlineData("SET a cursor")]
	public void Each_record_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = TransactSqlWriter.Write(tree);

		if (tree is Statement.Compound { Atomic: not null })
		{
			var source = "CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS " + text;
			var made   = (Statement.CreateRoutine)TransactSqlParser.Located.ParseStatement(source);

			Assert.Equal(tree, ((RoutineBody.Statements)made.Definition.Body).List[0]);

			return;
		}

		Assert.Equal(tree, TransactSqlParser.Located.ParseStatement(text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's message
	/// that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("an empty block")]
	[InlineData("an empty TRY")]
	[InlineData("an IF without ELSE as the THEN of one with ELSE")]
	[InlineData("an IF ending in an IF without ELSE inside a WHILE")]
	[InlineData("an atomic block in a block")]
	[InlineData("an atomic block as a THEN")]
	[InlineData("an atomic block as an ELSE")]
	[InlineData("an atomic block as a WHILE's body")]
	[InlineData("an atomic block in a TRY")]
	[InlineData("an atomic block in a CATCH")]
	[InlineData("an atomic setting twice")]
	[InlineData("an atomic block without a language")]
	[InlineData("an atomic block without a level")]
	[InlineData("a SET word as an atomic setting")]
	[InlineData("an atomic TEXTSIZE that is not whole")]
	[InlineData("an atomic TEXTSIZE of a hexadecimal number")]
	[InlineData("an atomic DELAYED_DURABILITY set to a number")]
	[InlineData("an atomic setting of several words")]
	[InlineData("a label named THROW")]
	[InlineData("a DECLARE of nothing")]
	[InlineData("a CURSOR variable given a value")]
	[InlineData("a CURSOR type spelled cursor")]
	[InlineData("a user type named TABLE")]
	[InlineData("a variable declared DEFAULT")]
	[InlineData("a table variable of nothing")]
	[InlineData("a table variable with a named constraint")]
	[InlineData("a table variable with a named DEFAULT")]
	[InlineData("a table variable with FILESTREAM")]
	[InlineData("a table variable with NOT FOR REPLICATION")]
	[InlineData("a table variable with REFERENCES")]
	[InlineData("a table variable of computed columns")]
	[InlineData("a SET of DEFAULT")]
	[InlineData("a SET of a comparison")]
	[InlineData("a SET with no operator and no call")]
	[InlineData("a SET calling a property's method")]
	[InlineData("a SET calling another variable's method")]
	[InlineData("a SET of a variable with no @")]
	[InlineData("a SET of nothing")]
	[InlineData("a switch beside a value")]
	[InlineData("switches set two ways")]
	[InlineData("a switch not in the list")]
	[InlineData("a switch in brackets")]
	[InlineData("a STATISTICS word beside a switch")]
	[InlineData("a STATISTICS kind with no word")]
	[InlineData("an OFFSETS word not in the list")]
	[InlineData("IDENTITY_INSERT beside another")]
	[InlineData("a value named in small letters")]
	[InlineData("a value not in the list")]
	[InlineData("ROWCOUNT in a list")]
	[InlineData("ROWCOUNT below nought")]
	[InlineData("TEXTSIZE of a variable")]
	[InlineData("LOCK_TIMEOUT of a fraction")]
	[InlineData("DATEFIRST of a negation")]
	[InlineData("DATEFIRST of money")]
	[InlineData("LOCK_TIMEOUT of a hexadecimal number")]
	[InlineData("TEXTSIZE of a decimal integer that is not digits")]
	[InlineData("DATEFIRST of an expression")]
	[InlineData("FIPS_FLAGGER of a national string")]
	[InlineData("FIPS_FLAGGER OFF in a list")]
	[InlineData("a transaction's name and variable both")]
	[InlineData("a transaction's name starting with @")]
	[InlineData("a transaction's name an expression")]
	[InlineData("a SAVE of no name")]
	[InlineData("a mark of DEFAULT")]
	[InlineData("a DEALLOCATE of a LOCAL name")]
	[InlineData("a DEALLOCATE of GLOBAL and a variable")]
	[InlineData("a DEALLOCATE of two parts")]
	[InlineData("a DEALLOCATE of the server's variable")]
	[InlineData("a cursor named in two parts")]
	[InlineData("T-SQL's form with no word")]
	[InlineData("FORWARD_ONLY that is not NoScroll")]
	[InlineData("INSENSITIVE after CURSOR")]
	[InlineData("FAST_FORWARD and SCROLL")]
	[InlineData("FAST_FORWARD and OPTIMISTIC")]
	[InlineData("STATIC FOR UPDATE")]
	[InlineData("INSENSITIVE FOR UPDATE")]
	[InlineData("READ_ONLY FOR READ ONLY declared")]
	[InlineData("OPTIMISTIC FOR READ ONLY")]
	[InlineData("READ ONLY of columns")]
	[InlineData("a FOR before hints with no FOR")]
	[InlineData("a cursor over VALUES")]
	[InlineData("SET a cursor INSENSITIVE")]
	[InlineData("SET a cursor of a name")]
	[InlineData("SET a cursor FOR READ_ONLY")]
	[InlineData("PRINT of DEFAULT")]
	[InlineData("RAISERROR of nothing")]
	[InlineData("RAISERROR with no text for its option")]
	[InlineData("THROW of two")]
	[InlineData("WAITFOR a number")]
	[InlineData("WAITFOR with no text for its kind")]
	[InlineData("EXECUTE AS SELF")]
	[InlineData("EXECUTE AS OWNER")]
	[InlineData("EXECUTE AS CALLER of a name")]
	[InlineData("EXECUTE AS USER of no name")]
	[InlineData("EXECUTE AS with NO REVERT and a cookie")]
	[InlineData("SETUSER NORESET with no name")]
	[InlineData("SETUSER of a name")]
	[InlineData("LINENO below nought")]
	[InlineData("LINENO of a fraction")]
	public void A_procedural_statement_with_no_text_is_refused(string which)
	{
		ISqlNode node = which switch
		{
			// StatementList reads one statement at least: BEGIN END is Msg 102.
			"an empty block" => new Statement.Compound([]),
			// TryCatchStatement: a TRY holds a statement, Msg 102.
			"an empty TRY" => new Statement.TryCatch([], [Selected]),
			// ElseBranch belongs to the innermost IF: the ELSE reads as the inner one's.
			"an IF without ELSE as the THEN of one with ELSE" => new Statement.If(Less, new Statement.If(Less, Selected, null), new Statement.Break()),
			// ElseBranch belongs to the innermost IF, inside the WHILE's branch.
			"an IF ending in an IF without ELSE inside a WHILE" => new Statement.If(Less, new Statement.While(Less, new Statement.If(Less, Selected, null)), new Statement.Break()),
			// AtomicBody is a module's body alone: nested in a block it is Msg 102.
			"an atomic block in a block" => new Statement.Compound([Atomic(Level, English)]),
			// AtomicBody is a module's body alone: inside an IF it is Msg 102.
			"an atomic block as a THEN" => new Statement.If(Less, Atomic(Level, English), null),
			// AtomicBody is a module's body alone: inside an IF it is Msg 102.
			"an atomic block as an ELSE" => new Statement.If(Less, Selected, Atomic(Level, English)),
			// AtomicBody is a module's body alone: Msg 102.
			"an atomic block as a WHILE's body" => new Statement.While(Less, Atomic(Level, English)),
			// AtomicBody is a module's body alone: Msg 102.
			"an atomic block in a TRY" => new Statement.TryCatch([Atomic(Level, English)], []),
			// AtomicBody is a module's body alone: Msg 102.
			"an atomic block in a CATCH" => new Statement.TryCatch([Selected], [Atomic(Level, English)]),
			// AtomicOptions: Syntax.Repeats, Msg 1039.
			"an atomic setting twice" => Atomic(Level, English, Set("DATEFIRST", One), Set("datefirst", One)),
			// AtomicOptions: Syntax.Atomically, Msg 10784.
			"an atomic block without a language" => Atomic(Level, Set("DATEFIRST", One)),
			// AtomicOptions: Syntax.Atomically, Msg 10784.
			"an atomic block without a level" => Atomic(English),
			// AtomicSetting names five settings: Msg 102.
			"a SET word as an atomic setting" => Atomic(Level, English, Set("ANSI_NULLS", Ref("ON"))),
			// AtomicSetting: TEXTSIZE = '-'? Digits.
			"an atomic TEXTSIZE that is not whole" => Atomic(Level, English, Set("TEXTSIZE", new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal))),
			// AtomicSetting: TEXTSIZE = '-'? Digits.
			"an atomic TEXTSIZE of a hexadecimal number" => Atomic(Level, English, Set("TEXTSIZE", new LiteralValue.Numeric("0x1F", NumericLiteralKind.HexInteger))),
			// AtomicSetting: DELAYED_DURABILITY = OnOff.
			"an atomic DELAYED_DURABILITY set to a number" => Atomic(Level, English, Set("DELAYED_DURABILITY", One)),
			// AtomicSetting: a name of one word.
			"an atomic setting of several words" => Atomic(Level, English, new AtomicOption.Setting(new Option(new Identifier("DATEFIRST"), One, []) { Words = [new Identifier("X")] })),
			// SimpleCommand reads THROW as the statement before it tries a label: THROW: is Msg 102.
			"a label named THROW" => new Statement.Label(Name("throw")),
			// DeclareStatement: one Declaration at least, Msg 102.
			"a DECLARE of nothing" => new Statement.DeclareVariables([]),
			// DeclaredAs: CURSOR takes no value, Msg 102.
			"a CURSOR variable given a value" => Declare(Cursor, One),
			// DeclaredAs reads the word, which reads back as the Named CURSOR.
			"a CURSOR type spelled cursor" => Declare(new DataType.Named(new QualifiedName([Name("cursor")]), []), null),
			// DeclaredAs: TABLE is followed by its body, Msg 102.
			"a user type named TABLE" => Declare(new DataType.UserDefined(new QualifiedName([Name("TABLE")])), null),
			// SetValue: NULL or a value, never DEFAULT, Msg 156.
			"a variable declared DEFAULT" => Declare(Int, new Expression.Default()),
			// TableBody: one element at least, Msg 102.
			"a table variable of nothing" => new Statement.DeclareVariables([new VariableDeclaration.Table(Name("@t"), [])]),
			// Syntax.Declared: no constraint named, Msg 156.
			"a table variable with a named constraint" => Table(new TableElement.TableConstraint(new Constraint.Check(Less) { Name = new QualifiedName([Name("k")]) })),
			// Syntax.Declared: no constraint named, Msg 156.
			"a table variable with a named DEFAULT" => Table(Column("d", Int, new ColumnOption.Default(One) { Name = Name("k") })),
			// Syntax.Declared: no FILESTREAM.
			"a table variable with FILESTREAM" => Table(Column("d", Int, new ColumnOption.Filestream())),
			// Syntax.Declared: no NOT FOR REPLICATION on an identity.
			"a table variable with NOT FOR REPLICATION" => Table(Column("d", Int, new ColumnOption.Identity(null, null), new ColumnOption.NotForReplication())),
			// Syntax.Declared: a column's REFERENCES without FOREIGN KEY.
			"a table variable with REFERENCES" => Table(Column("d", Int, new ColumnOption.Constraint(new Constraint.ForeignKey([], null, new ReferencesSpecification(new QualifiedName([Name("t")]), [], null, null, null, null))))),
			// Syntax.Stored: a computed column needs a stored one beside it.
			"a table variable of computed columns" => new Statement.DeclareVariables([new VariableDeclaration.Table(Name("@t"), [Column("d", null, new ColumnOption.Computed(One))])]),
			// SetValue: not DEFAULT, Msg 4119.
			"a SET of DEFAULT" => new Statement.SetVariable(V, new Expression.Default()),
			// AssignTail: SET @a = c = 1 is a variable and a column, which a variable's SET refuses.
			"a SET of a comparison" => new Statement.SetVariable(V, Less),
			// MemberTail: no operator is a method called.
			"a SET with no operator and no call" => new Statement.SetVariable(V, One) { Operator = null },
			// SetStatement: a method is the variable's own, @x.a.b('a') is refused.
			"a SET calling a property's method" => new Statement.SetVariable(V, Call(V)) { Operator = null, Members = [Name("x")] },
			// MemberTail: the method is the assigned variable's.
			"a SET calling another variable's method" => new Statement.SetVariable(V, Call(new Expression.Variable(Name("@b")))) { Operator = null },
			// TSql.Variable starts with @.
			"a SET of a variable with no @" => new Statement.SetVariable(new Expression.Variable(Name("a")), One),
			// SetStatement: SET alone is Msg 102.
			"a SET of nothing" => Options(),
			// SetExpressions: a switch among values, Msg 195.
			"a switch beside a value" => Options(new SetOption.Switch(Name("NOCOUNT"), true), Val("DATEFIRST", One)),
			// SetExpressions: the switches share one ON or OFF.
			"switches set two ways" => Options(new SetOption.Switch(Name("NOCOUNT"), true), new SetOption.Switch(Name("ANSI_NULLS"), false)),
			// SetSwitchName, Msg 195.
			"a switch not in the list" => Options(new SetOption.Switch(Name("NO_SUCH"), true)),
			// SetSwitchName reads the word, not a name.
			"a switch in brackets" => Options(new SetOption.Switch(new Identifier("[NOCOUNT]", IdentifierStyle.Bracketed), true)),
			// SetExpressions: STATISTICS reads SetReport words.
			"a STATISTICS word beside a switch" => Options(new SetOption.Statistics(StatisticsKind.Io, true), new SetOption.Switch(Name("NOCOUNT"), true)),
			// SetReportName: IO, TIME, PROFILE, XML.
			"a STATISTICS kind with no word" => Options(new SetOption.Statistics((StatisticsKind)9, true)),
			// SetOffsetName, Msg 195.
			"an OFFSETS word not in the list" => Options(new SetOption.Offsets(Name("WHERE"), true)),
			// SetExpressions: IDENTITY_INSERT stands alone, Msg 102.
			"IDENTITY_INSERT beside another" => Options(new SetOption.IdentityInsert(new QualifiedName([Name("t")]), true), new SetOption.IdentityInsert(new QualifiedName([Name("u")]), true)),
			// SettingValue reads the word, which the rules keep in capitals.
			"a value named in small letters" => Options(Val("datefirst", One)),
			// SettingValue, Msg 195.
			"a value not in the list" => Options(Val("NO_SUCH", One)),
			// SetExpressions: ROWCOUNT stands alone.
			"ROWCOUNT in a list" => Options(Val("ROWCOUNT", One), Val("DATEFIRST", One)),
			// SetRowCount: ROWCOUNT -1 is refused.
			"ROWCOUNT below nought" => Options(Val("ROWCOUNT", MinusOne)),
			// SetInteger: no variable.
			"TEXTSIZE of a variable" => Options(Val("TEXTSIZE", V)),
			// SetInteger: a whole number.
			"LOCK_TIMEOUT of a fraction" => Options(Val("LOCK_TIMEOUT", new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal))),
			// SetConstant: the sign is the literal's (Syntax.Compacted), read back as one.
			"DATEFIRST of a negation" => Options(Val("DATEFIRST", new Expression.Unary(UnaryOperator.Minus, One))),
			// SetConstant: UnsignedNumericLiteral, not money.
			"DATEFIRST of money" => Options(Val("DATEFIRST", new LiteralValue.Numeric("$5", NumericLiteralKind.Money))),
			// SetInteger: Digits.
			"LOCK_TIMEOUT of a hexadecimal number" => Options(Val("LOCK_TIMEOUT", new LiteralValue.Numeric("0x1F", NumericLiteralKind.HexInteger))),
			// SetInteger: Digits.
			"TEXTSIZE of a decimal integer that is not digits" => Options(Val("TEXTSIZE", new LiteralValue.Numeric("1e1", NumericLiteralKind.DecimalInteger))),
			// SetConstant: DATEFIRST 7 + 1 is refused.
			"DATEFIRST of an expression" => Options(Val("DATEFIRST", new Expression.Binary(One, BinaryOperator.Add, One))),
			// SetFipsLevel: never a national string.
			"FIPS_FLAGGER of a national string" => Options(Val("FIPS_FLAGGER", new LiteralValue.String("'FULL'", StringLiteralKind.National))),
			// SetExpressions: FIPS_FLAGGER OFF stands alone.
			"FIPS_FLAGGER OFF in a list" => Options(Val("FIPS_FLAGGER", Ref("OFF")), Val("DATEFIRST", One)),
			// TranName: a name or a variable.
			"a transaction's name and variable both" => new Statement.BeginTransaction(Name: new TransactionName(Name("t"), V)),
			// TranName reads @t as TSql.Variable.
			"a transaction's name starting with @" => new Statement.SaveTransaction(new TransactionName(Name("@t"))),
			// TranName: Identifier or TSql.Variable.
			"a transaction's name an expression" => new Statement.SaveTransaction(new TransactionName(null, One)),
			// TranName: SAVE TRAN alone is Msg 102.
			"a SAVE of no name" => new Statement.SaveTransaction(new TransactionName(null)),
			// TranMark: ValueExpression, not DEFAULT (Msg 156).
			"a mark of DEFAULT" => new Statement.BeginTransaction(Name: new TransactionName(Name("t")), Mark: new TransactionMark(new Expression.Default())),
			// CursorName: GLOBAL or nothing.
			"a DEALLOCATE of a LOCAL name" => new Statement.DeallocateCursor(C with { Local = true }),
			// CursorName: GLOBAL never stands before a variable.
			"a DEALLOCATE of GLOBAL and a variable" => new Statement.DeallocateCursor(new CursorReference(null, V, Global: true)),
			// CursorName: one part.
			"a DEALLOCATE of two parts" => new Statement.DeallocateCursor(new CursorReference(new QualifiedName([Name("a"), Name("c")]))),
			// LocalVariable: Syntax.IsServerVariable, Msg 102.
			"a DEALLOCATE of the server's variable" => new Statement.DeallocateCursor(new CursorReference(null, new Expression.Variable(Name("@@ROWCOUNT")))),
			// DeclareStatement: name: Identifier.
			"a cursor named in two parts" => new Statement.DeclareCursor { Cursor = new CursorReference(new QualifiedName([Name("a"), Name("c")])), Properties = Words(), SourceValue = Over() },
			// CursorDefinition: no word after CURSOR reads as the ISO form.
			"T-SQL's form with no word" => Declared(Words(), Over()),
			// CursorOption: FORWARD_ONLY and SCROLL, Syntax.CursorAgrees.
			"FORWARD_ONLY that is not NoScroll" => Declared(Words(CursorScrollability.Scroll) with { ForwardOnly = true }, Over()),
			// CursorOption has no INSENSITIVE.
			"INSENSITIVE after CURSOR" => Declared(new CursorProperties(CursorSensitivity.Insensitive, null, null, null) { Scope = CursorScope.Local }, Over()),
			// Syntax.CursorAgrees: FAST_FORWARD with SCROLL, Msg 1048.
			"FAST_FORWARD and SCROLL" => Declared(Words(CursorScrollability.Scroll) with { Type = CursorType.FastForward }, Over()),
			// Syntax.CursorAgrees: FAST_FORWARD with OPTIMISTIC, Msg 1048.
			"FAST_FORWARD and OPTIMISTIC" => Declared(Words() with { Type = CursorType.FastForward, Concurrency = CursorConcurrency.Optimistic }, Over()),
			// Syntax.CursorAgrees: FOR UPDATE on a STATIC cursor, Msg 16957.
			"STATIC FOR UPDATE" => Declared(Words() with { Type = CursorType.Static }, Over(new UpdatabilityClause(false, []))),
			// Syntax.CursorAgrees: FOR UPDATE on an INSENSITIVE cursor.
			"INSENSITIVE FOR UPDATE" => Declared(new CursorProperties(CursorSensitivity.Insensitive, null, null, null), Over(new UpdatabilityClause(false, [])), extended: false),
			// Syntax.CursorAgrees: READ_ONLY and FOR READ ONLY declared, Msg 1058.
			"READ_ONLY FOR READ ONLY declared" => Declared(Words() with { Concurrency = CursorConcurrency.ReadOnly }, Over(ReadOnly)),
			// Syntax.CursorAgrees: OPTIMISTIC and FOR READ ONLY.
			"OPTIMISTIC FOR READ ONLY" => Declared(Words() with { Concurrency = CursorConcurrency.Optimistic }, Over(ReadOnly)),
			// CursorFor: READ ONLY takes no columns.
			"READ ONLY of columns" => Declared(Words(), Over(new UpdatabilityClause(true, [Name("a")])), extended: false),
			// CursorQuery: the first form has a FOR.
			"a FOR before hints with no FOR" => Declared(Words(), Over(null, recompiled: true, before: true), extended: false),
			// Syntax.Cursorable: VALUES is Msg 156.
			"a cursor over VALUES" => Declared(Words(), new CursorSource.Query(new Statement.Select { Query = new Query.Select { Values = [new RowValue([One])] } }, null), extended: false),
			// CursorInPlace: T-SQL's words only.
			"SET a cursor INSENSITIVE" => new Statement.SetCursorVariable(V, new CursorProperties(CursorSensitivity.Insensitive, null, null, null), Over()),
			// SetStatement: TSql.Variable.
			"SET a cursor of a name" => new Statement.SetCursorVariable(Ref("c"), Words(), Over()),
			// CursorFor: READ ONLY, two words.
			"SET a cursor FOR READ_ONLY" => new Statement.SetCursorVariable(V, Words(), Over(new UpdatabilityClause(true, [], Underscored: true))),
			// ValueExpression: not DEFAULT.
			"PRINT of DEFAULT" => new Statement.Print(new Expression.Default()),
			// SimpleCommand: RAISERROR takes Arguments, Msg 102.
			"RAISERROR of nothing" => new Statement.RaiseError([]),
			// RaiseWith: LOG, NOWAIT, SETERROR.
			"RAISERROR with no text for its option" => new Statement.RaiseError([V]) { Options = [(RaiseErrorOption)9] },
			// THROW: three values or none, Msg 102.
			"THROW of two" => new Statement.Throw(One, Text("m"), null),
			// WaitValue: WAITFOR DELAY 1 is refused.
			"WAITFOR a number" => new Statement.WaitFor(WaitForKind.Delay, One),
			// WaitKind: DELAY or TIME.
			"WAITFOR with no text for its kind" => new Statement.WaitFor((WaitForKind)9, V),
			// SessionWho: SELF is a module's.
			"EXECUTE AS SELF" => new Statement.ExecuteAs(new ExecutionContext(ExecutionContextKind.Self, null)),
			// SessionWho: OWNER is a module's.
			"EXECUTE AS OWNER" => new Statement.ExecuteAs(new ExecutionContext(ExecutionContextKind.Owner, null)),
			// SessionWho: CALLER alone.
			"EXECUTE AS CALLER of a name" => new Statement.ExecuteAs(new ExecutionContext(ExecutionContextKind.Caller, V)),
			// SessionWho: USER = name.
			"EXECUTE AS USER of no name" => new Statement.ExecuteAs(new ExecutionContext(ExecutionContextKind.User, null)),
			// SessionWith: one of the two.
			"EXECUTE AS with NO REVERT and a cookie" => new Statement.ExecuteAs(new ExecutionContext(ExecutionContextKind.Caller, null)) { NoRevert = true, Cookie = V },
			// SessionNoReset only after SessionUser.
			"SETUSER NORESET with no name" => new Statement.SetUser(null) { NoReset = true },
			// SessionUser: a string or a variable.
			"SETUSER of a name" => new Statement.SetUser(Ref("u")),
			// LINENO: Digits, -1 is Msg 102.
			"LINENO below nought" => new Statement.LineNumber(new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger)),
			// LINENO: Digits, 1.5 is Msg 102.
			"LINENO of a fraction" => new Statement.LineNumber(new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal)),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => SqlWriter.Write(node));
	}
}
