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
			"CREATE SELECTIVE XML INDEX" => new Statement.CreateSelectiveXmlIndex(Qualified("t"), Name("sxi"), Name("c"),
				[
					new PromotedPath(Name("p1"), Str("/a")) { Hints = [new PathHint.XQuery(Str("xs:string"), (LiteralValue.Numeric)Number("5")), new PathHint.Singleton()] },
					new PromotedPath(Name("p2"), Str("/b")) { Hints = [new PathHint.Sql(new DataType.Character(CharacterTypeKind.Nvarchar, 10))] },
				])
			{
				XmlNamespaces = [new XmlNamespace(Text("urn:a"), Name("ns"))],
				Options       = [Set("PAD_INDEX", Ref("ON"))],
			},
			"ALTER INDEX paths" => new Statement.AlterIndex(Qualified("t"), new IndexAlteration.Paths([new PathAction.Remove(Name("p2")), new PathAction.Add(new PromotedPath(Name("p1"), Str("/a")))])) { Name = Name("sxi") },
			"WITH CHANGE_TRACKING_CONTEXT" => new Statement.Select { Query = ((Query.Select)Select()) with { With = new WithClause(false, []) { ChangeTrackingContext = new LiteralValue.Binary("FF") { ZeroX = true } } } },
			"CREATE MATERIALIZED VIEW" => new Statement.CreateMaterializedView(Qualified("v"), Select()) { Options = [Set("DISTRIBUTION", Ref("ROUND_ROBIN")), Set("FOR_APPEND")] },
			"CREATE JSON INDEX" => new Statement.CreateJsonIndex(Qualified("t"), Name("ix"), Name("j")) { Paths = [Str("$.a")], Options = [Set("OPTIMIZE_FOR_ARRAY_SEARCH", Ref("ON"))] },
			"CREATE XML INDEX FOR a path" => new Statement.CreateXmlIndex(Qualified("t"), Name("i"), Name("c")) { Using = Name("sxi"), ForPath = Name("p") },
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	static LiteralValue.String Str(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
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
	[InlineData("CREATE SELECTIVE XML INDEX", "CREATE SELECTIVE XML INDEX sxi ON t (c) WITH XMLNAMESPACES ('urn:a' AS ns) FOR (p1 = '/a' AS XQUERY 'xs:string' MAXLENGTH (5) SINGLETON, p2 = '/b' AS SQL NVARCHAR(10)) WITH (PAD_INDEX = ON)")]
	[InlineData("ALTER INDEX paths", "ALTER INDEX sxi ON t FOR (REMOVE p2, ADD p1 = '/a')")]
	[InlineData("CREATE XML INDEX FOR a path", "CREATE XML INDEX i ON t (c) USING XML INDEX sxi FOR (p)")]
	[InlineData("CREATE JSON INDEX", "CREATE JSON INDEX ix ON t (j) FOR ('$.a') WITH (OPTIMIZE_FOR_ARRAY_SEARCH = ON)")]
	[InlineData("CREATE MATERIALIZED VIEW", "CREATE MATERIALIZED VIEW v WITH (DISTRIBUTION = ROUND_ROBIN, FOR_APPEND) AS SELECT a FROM u")]
	[InlineData("WITH CHANGE_TRACKING_CONTEXT", "WITH CHANGE_TRACKING_CONTEXT (0xFF) SELECT a FROM u")]
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
	[InlineData("CREATE SELECTIVE XML INDEX")]
	[InlineData("ALTER INDEX paths")]
	[InlineData("CREATE XML INDEX FOR a path")]
	[InlineData("CREATE JSON INDEX")]
	[InlineData("CREATE MATERIALIZED VIEW")]
	[InlineData("WITH CHANGE_TRACKING_CONTEXT")]
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
	[InlineData("selective with no path")]
	[InlineData("selective a national path")]
	[InlineData("selective a national XQuery type")]
	[InlineData("selective a signed length")]
	[InlineData("selective an option it does not read")]
	[InlineData("XML index FOR a kind and a path")]
	[InlineData("ALTER INDEX no path changed")]
	[InlineData("JSON an option it does not read")]
	[InlineData("spatial a grid in brackets that is none")]
	[InlineData("materialized view a distribution it does not read")]
	[InlineData("a change tracking context beside namespaces")]
	[InlineData("JSON a path with an escape")]
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
			// SelectivePaths: FOR (path, …), one at least.
			"selective with no path" => new Statement.CreateSelectiveXmlIndex(Qualified("t"), Name("sxi"), Name("c"), []),
			// PromotedPath: Lexical.CharacterStringLiteral.
			"selective a national path" => new Statement.CreateSelectiveXmlIndex(Qualified("t"), Name("sxi"), Name("c"), [new PromotedPath(Name("p"), new LiteralValue.String("'/a'", StringLiteralKind.National))]),
			// PathHint: AS XQUERY Lexical.CharacterStringLiteral.
			"selective a national XQuery type" => new Statement.CreateSelectiveXmlIndex(Qualified("t"), Name("sxi"), Name("c"), [new PromotedPath(Name("p"), Str("/a")) { Hints = [new PathHint.XQuery(new LiteralValue.String("'x'", StringLiteralKind.National), null)] }]),
			// PathLength: MAXLENGTH (Lexical.UnsignedNumericLiteral).
			"selective a signed length" => new Statement.CreateSelectiveXmlIndex(Qualified("t"), Name("sxi"), Name("c"), [new PromotedPath(Name("p"), Str("/a")) { Hints = [new PathHint.XQuery(null, new LiteralValue.Numeric("-5", NumericLiteralKind.DecimalInteger))] }]),
			// XmlIndexOption: ONLINE = ON is refused for an XML index.
			"selective an option it does not read" => new Statement.CreateSelectiveXmlIndex(Qualified("t"), Name("sxi"), Name("c"), [new PromotedPath(Name("p"), Str("/a"))]) { Options = [Set("ONLINE", Ref("ON"))] },
			// XmlIndexUsing: FOR a kind or a path, never both.
			"XML index FOR a kind and a path" => new Statement.CreateXmlIndex(Qualified("t"), Name("i"), Name("c")) { Using = Name("sxi"), For = XmlIndexFor.Path, ForPath = Name("p") },
			// SpatialIndex: USING names a grid; brackets around none are no text.
			"spatial a grid in brackets that is none" => new Statement.CreateSpatialIndex(Qualified("t"), Name("i"), Name("g")) { UsingBracketed = true },
			// TableOptionSyntax: DISTRIBUTION = HASH (…), ROUND_ROBIN or REPLICATE.
			"materialized view a distribution it does not read" => new Statement.CreateMaterializedView(Qualified("v"), Select()) { Options = [Set("DISTRIBUTION", Ref("RANDOM"))] },
			// WithClause: CHANGE_TRACKING_CONTEXT or XMLNAMESPACES first, one of them.
			"a change tracking context beside namespaces" => new Statement.Select { Query = ((Query.Select)Select()) with { With = new WithClause(false, []) { ChangeTrackingContext = Number("1"), XmlNamespaces = [new XmlNamespace(Text("u"), Name("x"))] } } },
			// JsonIndexOptionSyntax: FILLFACTOR is Msg 153.
			"JSON an option it does not read" => new Statement.CreateJsonIndex(Qualified("t"), Name("ix"), Name("j")) { Options = [Set("FILLFACTOR", Number("80"))] },
			// JsonIndexPath: a plain or national string.
			"JSON a path with an escape" => new Statement.CreateJsonIndex(Qualified("t"), Name("ix"), Name("j")) { Paths = [Str("$.a") with { UnicodeEscape = '!' }] },
			// SelectivePaths: one at least.
			"ALTER INDEX no path changed" => new Statement.AlterIndex(Qualified("t"), new IndexAlteration.Paths([])) { Name = Name("sxi") },
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(node));
	}
}
