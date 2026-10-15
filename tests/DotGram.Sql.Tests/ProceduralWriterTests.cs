using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using ColumnDefinition = DotGram.Sql.Ast.ColumnDefinition;
using ColumnOption = DotGram.Sql.Ast.ColumnOption;
using Constraint = DotGram.Sql.Ast.Constraint;
using DataType = DotGram.Sql.Ast.DataType;
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;
using TableElement = DotGram.Sql.Ast.TableElement;

/// <summary>
/// The Procedural family's writer (Sql2023Writer.TransactSql.Procedural.cs): each record written and read back by
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
	public void Each_record_writes_its_text(string which, string text)
	{
		Assert.Equal(text, Sql2023Writer.Write(Tree(which)));
	}

	/// <summary>
	/// Each record's tree, written and read back by TransactSqlParser, carried by the bridge to the same tree. A natively
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
	public void Each_record_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = Sql2023Writer.Write(tree);

		if (tree is Statement.Compound { Atomic: not null })
		{
			var source = "CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS " + text;
			var body   = ((DotGram.Sql.Statement.CreateProcedure)TransactSqlParser.Located.ParseStatement(source)).Body[0];

			Assert.Equal(tree, Bridge.ToAst(body, source));

			return;
		}

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
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
	[InlineData("an atomic setting twice")]
	[InlineData("an atomic block without a language")]
	[InlineData("an atomic block without a level")]
	[InlineData("a SET word as an atomic setting")]
	[InlineData("an atomic TEXTSIZE that is not whole")]
	[InlineData("an atomic DATEFIRST set to DEFAULT")]
	[InlineData("an atomic DELAYED_DURABILITY set to a number")]
	[InlineData("an atomic setting of several words")]
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
	[InlineData("DATEFIRST of an expression")]
	[InlineData("DATEFIRST of a reserved word")]
	[InlineData("FIPS_FLAGGER of a national string")]
	[InlineData("FIPS_FLAGGER OFF in a list")]
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
			// AtomicConstant: not DEFAULT.
			"an atomic DATEFIRST set to DEFAULT" => Atomic(Level, English, Set("DATEFIRST", Ref("DEFAULT"))),
			// AtomicSetting: DELAYED_DURABILITY = OnOff.
			"an atomic DELAYED_DURABILITY set to a number" => Atomic(Level, English, Set("DELAYED_DURABILITY", One)),
			// AtomicSetting: a name of one word.
			"an atomic setting of several words" => Atomic(Level, English, new AtomicOption.Setting(new Option(new Identifier("DATEFIRST"), One, []) { Words = [new Identifier("X")] })),
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
			// SettingValue reads the word, which the bridge keeps in capitals.
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
			// SetConstant: DATEFIRST 7 + 1 is refused.
			"DATEFIRST of an expression" => Options(Val("DATEFIRST", new Expression.Binary(One, BinaryOperator.Add, One))),
			// SetConstant: Identifier is not TSqlReserved, Msg 195.
			"DATEFIRST of a reserved word" => Options(Val("DATEFIRST", Ref("ON"))),
			// SetFipsLevel: never a national string.
			"FIPS_FLAGGER of a national string" => Options(Val("FIPS_FLAGGER", new LiteralValue.String("'FULL'", StringLiteralKind.National))),
			// SetExpressions: FIPS_FLAGGER OFF stands alone.
			"FIPS_FLAGGER OFF in a list" => Options(Val("FIPS_FLAGGER", Ref("OFF")), Val("DATEFIRST", One)),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}
}
