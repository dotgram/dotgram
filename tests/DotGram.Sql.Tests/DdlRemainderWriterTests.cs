using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The writer for the remainder of the DDL statements: each tree written and read back by TransactSqlParser to the
/// same tree, and every tree whose text would read back as another refused.
/// </summary>
public sealed class DdlRemainderWriterTests
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
			"CREATE INDEX unbracketed" => new Statement.CreateIndex(Qualified("t"), Index(Set("PAD_INDEX"), Set("FILLFACTOR", Number("80"))) with { OptionsUnbracketed = true }),
			"CREATE INDEX unbracketed one word" => new Statement.CreateIndex(Qualified("t"), Index(Set("SORTED_DATA_REORG")) with { OptionsUnbracketed = true }),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	[Theory]
	[InlineData("CREATE INDEX unbracketed", "CREATE INDEX i ON t (a) WITH PAD_INDEX, FILLFACTOR = 80")]
	[InlineData("CREATE INDEX unbracketed one word", "CREATE INDEX i ON t (a) WITH SORTED_DATA_REORG")]
	public void Each_tree_writes_its_text(string which, string text)
	{
		Assert.Equal(text, TransactSqlWriter.Write(Tree(which)));
	}

	[Theory]
	[InlineData("CREATE INDEX unbracketed")]
	[InlineData("CREATE INDEX unbracketed one word")]
	public void Each_tree_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	[Theory]
	[InlineData("unbracketed with no option")]
	[InlineData("unbracketed with a value on a word")]
	[InlineData("unbracketed with a word the old list does not have")]
	[InlineData("unbracketed FILLFACTOR without its number")]
	[InlineData("unbracketed at the table")]
	[InlineData("unbracketed on a columnstore index")]
	public void A_tree_with_no_text_is_refused(string which)
	{
		Statement node = which switch
		{
			// OldIndexOption: one option at least after WITH.
			"unbracketed with no option" => new Statement.CreateIndex(Qualified("t"), Index() with { OptionsUnbracketed = true }),
			// BareIndexWord & ?!'=': `WITH pad_index = ON` is Msg 156.
			"unbracketed with a value on a word" => new Statement.CreateIndex(Qualified("t"), Index(Set("PAD_INDEX", Ref("ON"))) with { OptionsUnbracketed = true }),
			// OldIndexOption: ALLOW_ROW_LOCKS is not among the eight (Msg 153).
			"unbracketed with a word the old list does not have" => new Statement.CreateIndex(Qualified("t"), Index(Set("ALLOW_ROW_LOCKS")) with { OptionsUnbracketed = true }),
			// FillFactor: FILLFACTOR = Number.
			"unbracketed FILLFACTOR without its number" => new Statement.CreateIndex(Qualified("t"), Index(Set("FILLFACTOR")) with { OptionsUnbracketed = true }),
			// TableIndexWith: brackets only.
			"unbracketed at the table" => new Statement.CreateTable { Name = Qualified("t"), Contents = new TableContents.Elements([new TableElement.Index(Index(Set("PAD_INDEX")) with { OptionsUnbracketed = true })]) },
			// ColumnstoreWith: brackets only (Msg 18054 for the old list).
			"unbracketed on a columnstore index" => new Statement.CreateIndex(Qualified("t"), Index(Set("PAD_INDEX")) with { OptionsUnbracketed = true, Storage = IndexStorage.Columnstore }),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(node));
	}
}
