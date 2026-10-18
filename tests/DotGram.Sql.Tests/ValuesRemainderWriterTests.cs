using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The writer for the remainder outside the DDL: each tree written and read back by TransactSqlParser to the
/// same tree, and every tree whose text would read back as another refused.
/// </summary>
public sealed class ValuesRemainderWriterTests
{
	static Identifier Name(string text)
	{
		return new Identifier(text);
	}

	static QualifiedName Qualified(string text)
	{
		return new QualifiedName([Name(text)]);
	}

	static Expression Ref(string word)
	{
		return new Expression.Reference(Qualified(word));
	}

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static Expression Text(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static Option Set(string name, Expression? value = null)
	{
		return new Option(Name(name), value, []);
	}

	static IndexDefinition Index(params Option[] options)
	{
		return new IndexDefinition(Name("i")) { Columns = [new SortItem(Ref("a"))], Options = [.. options] };
	}

	static Statement Tree(string which)
	{
		return which switch
		{
			"SAVE TRANSACTION numbered" => new Statement.SaveTransaction(new TransactionName(null) { Number = new LiteralValue.Numeric("-5", NumericLiteralKind.DecimalInteger), Qualified = new QualifiedName([Name("a"), Name("b")]) }),
			"COMMIT numbered" => new Statement.Commit { Keyword = TransactionKeyword.Tran, Name = new TransactionName(null) { Number = (LiteralValue.Numeric)Number("1"), Qualified = new QualifiedName([Name("x"), Name("y")]) } },
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	[Theory]
	[InlineData("SAVE TRANSACTION numbered", "SAVE TRANSACTION -5 : a.b")]
	[InlineData("COMMIT numbered", "COMMIT TRAN 1 : x.y")]
	public void Each_tree_writes_its_text(string which, string text)
	{
		Assert.Equal(text, TransactSqlWriter.Write(Tree(which)));
	}

	[Theory]
	[InlineData("SAVE TRANSACTION numbered")]
	[InlineData("COMMIT numbered")]
	public void Each_tree_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	[Theory]
	[InlineData("a numbered name with a plus")]
	[InlineData("a numbered name of one part")]
	[InlineData("a numbered name beside a name")]
	[InlineData("a number with no name")]
	public void A_tree_with_no_text_is_refused(string which)
	{
		Statement node = which switch
		{
			// TranName: '-'? & Lexical.Digits; a plus sign is Msg 102.
			"a numbered name with a plus" => new Statement.SaveTransaction(new TransactionName(null) { Number = new LiteralValue.Numeric("+5", NumericLiteralKind.DecimalInteger), Qualified = new QualifiedName([Name("a"), Name("b")]) }),
			// TranName: a name of two parts and no more.
			"a numbered name of one part" => new Statement.SaveTransaction(new TransactionName(null) { Number = (LiteralValue.Numeric)Number("5"), Qualified = Qualified("a") }),
			// TranName: one of the three forms.
			"a numbered name beside a name" => new Statement.SaveTransaction(new TransactionName(Name("t")) { Number = (LiteralValue.Numeric)Number("5"), Qualified = new QualifiedName([Name("a"), Name("b")]) }),
			"a number with no name" => new Statement.SaveTransaction(new TransactionName(null) { Number = (LiteralValue.Numeric)Number("5") }),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(node));
	}
}
