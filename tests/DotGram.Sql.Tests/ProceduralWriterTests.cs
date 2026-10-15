using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using DataType = DotGram.Sql.Ast.DataType;
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

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
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}
}
