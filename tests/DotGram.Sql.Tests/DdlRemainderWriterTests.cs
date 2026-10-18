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
			"CREATE FULLTEXT INDEX every option" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k"))
			{
				Options =
				[
					Set("CHANGE_TRACKING", Ref("MANUAL")) with { EqualsOmitted = true },
					new Option(Name("NO"), null, []) { Words = [Name("POPULATION")] },
					Set("STOPLIST", Ref("SYSTEM")),
					new Option(Name("SEARCH"), Ref("p"), []) { Words = [Name("PROPERTY"), Name("LIST")], EqualsOmitted = true },
				],
			},
			"ALTER FULLTEXT INDEX DROP with no population" => new Statement.AlterFullTextIndex(Qualified("t"), new FullTextAlteration.DropColumns([Name("c")]) { Options = [NoPopulation()] }),
			"CTAS a name of three words" => new Statement.CreateTable
			{
				Name     = Qualified("t"),
				Contents = new TableContents.AsQuery([], Select(), null),
				Options  = [new Option(Name("CLUSTERED"), null, []) { Words = [Name("COLUMNSTORE"), Name("INDEX")] }],
			},
			"CREATE SPATIAL INDEX brackets" => Spatial(
				Set("BOUNDING_BOX", new Expression.Row([new LiteralValue.Numeric("-1.5", NumericLiteralKind.Decimal), Number("0"), Number("500"), Number("200")])),
				new Option(Name("GRIDS"), null, [Set("LEVEL_1", Ref("HIGH")), Set("LEVEL_2", Ref("LOW"))])),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	static Statement Spatial(params Option[] options)
	{
		return new Statement.CreateSpatialIndex(Qualified("t"), Name("i"), Name("g")) { Options = [.. options] };
	}

	static Option NoPopulation()
	{
		return new Option(Name("NO"), null, []) { Words = [Name("POPULATION")] };
	}

	static Query Select()
	{
		return new Query.Select { Columns = [new SelectColumn.ExpressionItem(Ref("a"))], From = new FromClause([new TableSource.Named(Qualified("u"))]) };
	}

	[Theory]
	[InlineData("CREATE INDEX unbracketed", "CREATE INDEX i ON t (a) WITH PAD_INDEX, FILLFACTOR = 80")]
	[InlineData("CREATE INDEX unbracketed one word", "CREATE INDEX i ON t (a) WITH SORTED_DATA_REORG")]
	[InlineData("CREATE FULLTEXT INDEX every option", "CREATE FULLTEXT INDEX ON t KEY INDEX k WITH (CHANGE_TRACKING MANUAL, NO POPULATION, STOPLIST = SYSTEM, SEARCH PROPERTY LIST p)")]
	[InlineData("ALTER FULLTEXT INDEX DROP with no population", "ALTER FULLTEXT INDEX ON t DROP (c) WITH NO POPULATION")]
	[InlineData("CTAS a name of three words", "CREATE TABLE t WITH (CLUSTERED COLUMNSTORE INDEX) AS SELECT a FROM u")]
	[InlineData("CREATE SPATIAL INDEX brackets", "CREATE SPATIAL INDEX i ON t (g) WITH (BOUNDING_BOX = (-1.5, 0, 500, 200), GRIDS = (LEVEL_1 = HIGH, LEVEL_2 = LOW))")]
	public void Each_tree_writes_its_text(string which, string text)
	{
		Assert.Equal(text, TransactSqlWriter.Write(Tree(which)));
	}

	[Theory]
	[InlineData("CREATE INDEX unbracketed")]
	[InlineData("CREATE INDEX unbracketed one word")]
	[InlineData("CREATE FULLTEXT INDEX every option")]
	[InlineData("ALTER FULLTEXT INDEX DROP with no population")]
	[InlineData("CTAS a name of three words")]
	[InlineData("CREATE SPATIAL INDEX brackets")]
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
	[InlineData("NO POPULATION first")]
	[InlineData("NO POPULATION after a stoplist")]
	[InlineData("a property list of two parts")]
	[InlineData("CHANGE_TRACKING with words")]
	[InlineData("NO POPULATION with a value")]
	[InlineData("ALTER NO POPULATION after ENABLE")]
	[InlineData("ALTER an option other than NO POPULATION")]
	[InlineData("CTAS a name whose words hold ON")]
	[InlineData("spatial an empty bracket")]
	[InlineData("spatial a switch in a bracket")]
	[InlineData("spatial a bracket in a bracket")]
	[InlineData("spatial a row with ROW")]
	[InlineData("spatial a setting with words")]
	[InlineData("spatial a value and settings")]
	[InlineData("spatial FILLFACTOR as a bracket")]
	[InlineData("filegroup first without a catalog")]
	[InlineData("catalog bracketed beside a filegroup")]
	[InlineData("catalog bracketed without a catalog")]
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
			// FullTextOption: NO POPULATION comes after CHANGE_TRACKING and a comma, nowhere else.
			"NO POPULATION first" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k")) { Options = [NoPopulation()] },
			"NO POPULATION after a stoplist" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k")) { Options = [Set("STOPLIST", Ref("OFF")), NoPopulation()] },
			// FullTextPropertyList: OFF or TSqlIdentifier & ?!'.'.
			"a property list of two parts" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k")) { Options = [new Option(Name("SEARCH"), new Expression.Reference(new QualifiedName([Name("a"), Name("b")])), []) { Words = [Name("PROPERTY"), Name("LIST")] }] },
			// FullTextOption: CHANGE_TRACKING is one word.
			"CHANGE_TRACKING with words" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k")) { Options = [new Option(Name("CHANGE_TRACKING"), Ref("AUTO"), []) { Words = [Name("NOW")] }] },
			// FullTextOption: NO POPULATION takes nothing after it.
			"NO POPULATION with a value" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k")) { Options = [Set("CHANGE_TRACKING", Ref("OFF")), NoPopulation() with { Value = Ref("ON") }] },
			// FullTextAction: ENABLE takes no NoPopulation.
			"ALTER NO POPULATION after ENABLE" => new Statement.AlterFullTextIndex(Qualified("t"), new FullTextAlteration.Enable { Options = [NoPopulation()] }),
			// NoPopulation: WITH NO POPULATION is the statement's one WITH.
			"ALTER an option other than NO POPULATION" => new Statement.AlterFullTextIndex(Qualified("t"), new FullTextAlteration.DropColumns([Name("c")]) { Options = [Set("ONLINE", Ref("ON"))] }),
			// OptionName: a run of words ends at ON or OFF.
			"CTAS a name whose words hold ON" => new Statement.CreateTable { Name = Qualified("t"), Contents = new TableContents.AsQuery([], Select(), null), Options = [new Option(Name("CLUSTERED"), null, []) { Words = [Name("ON")] }] },
			// SpatialSet: one SpatialItem at least in a bracket.
			"spatial an empty bracket" => Spatial(Set("GRIDS", new Expression.Row([]))),
			// SpatialValue: `FOO = (ON)` is Msg 156.
			"spatial a switch in a bracket" => Spatial(Set("FOO", new Expression.Row([Ref("ON")]))),
			// SpatialItem: never a bracket.
			"spatial a bracket in a bracket" => Spatial(Set("FOO", new Expression.Row([new Expression.Row([Number("1")])]))),
			// SpatialSet: a bracket, never the word ROW.
			"spatial a row with ROW" => Spatial(Set("FOO", new Expression.Row([Number("1")], RowKeyword: true))),
			// SpatialSetting: a name of one word.
			"spatial a setting with words" => Spatial(new Option(Name("GRIDS"), null, [new Option(Name("LEVEL"), Ref("LOW"), []) { Words = [Name("ONE")] }])),
			// SpatialSet: a value, or a bracket, never both.
			"spatial a value and settings" => Spatial(new Option(Name("GRIDS"), Ref("LOW"), [Set("LEVEL_1", Ref("HIGH"))])),
			// Syntax.Spatial: FILLFACTOR is a number from 1 to 100.
			"spatial FILLFACTOR as a bracket" => Spatial(Set("FILLFACTOR", new Expression.Row([Number("80")]))),
			// FullTextCatalogOn: an order is said of two places only.
			"filegroup first without a catalog" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k")) { FileGroup = Name("fg"), FileGroupFirst = true },
			// FullTextCatalogOn: with a filegroup the brackets are always written.
			"catalog bracketed beside a filegroup" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k")) { Catalog = Name("c"), FileGroup = Name("fg"), CatalogBracketed = true },
			"catalog bracketed without a catalog" => new Statement.CreateFullTextIndex(Qualified("t"), [], Name("k")) { CatalogBracketed = true },
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(node));
	}
}
