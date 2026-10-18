using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The remainder of the DDL statements the bridge carries: which record and members it builds for a text, where another
/// record or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class DdlRemainderBridgeTests
{
	[Theory]
	[InlineData("CREATE INDEX i ON t (a) WITH PAD_INDEX, FILLFACTOR = 80", "unbracketed")]
	[InlineData("CREATE INDEX i ON t (a) WITH (PAD_INDEX = ON, FILLFACTOR = 80)", "bracketed")]
	[InlineData("CREATE INDEX i ON t (a) WITH SORTED_DATA", "a word alone")]
	[InlineData("CREATE INDEX i ON t (a) WITH PAD_INDEX ON [PRIMARY]", "a word and a placement")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k WITH CHANGE_TRACKING AUTO", "tracking with no =")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k WITH CHANGE_TRACKING = AUTO", "tracking with =")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k WITH (SEARCH PROPERTY LIST = p)", "a property list")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k WITH (CHANGE_TRACKING OFF, NO POPULATION)", "no population")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k WITH STOPLIST system", "the system stoplist")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k WITH STOPLIST [system]", "a stoplist called system")]
	[InlineData("ALTER FULLTEXT INDEX ON t ADD (c) WITH NO POPULATION", "columns added with no population")]
	[InlineData("ALTER FULLTEXT INDEX ON t ADD (c)", "columns added")]
	[InlineData("CREATE TABLE t WITH (CLUSTERED COLUMNSTORE INDEX, DISTRIBUTION = ROUND_ROBIN) AS SELECT a FROM u", "a name of three words")]
	[InlineData("CREATE TABLE t WITH (CLUSTERED INDEX (a), DISTRIBUTION = HASH (a)) AS SELECT a FROM u", "a name of two words and its list")]
	[InlineData("CREATE SPATIAL INDEX i ON t (g) WITH (BOUNDING_BOX = (-1.5, 0, 5e2, 200))", "a bracket by position")]
	[InlineData("CREATE SPATIAL INDEX i ON t (g) WITH (BOUNDING_BOX = (xmin = -1, ymin = 0, xmax = 3, ymax = 4))", "a bracket by name")]
	[InlineData("CREATE SPATIAL INDEX i ON t (g) WITH (GRIDS = (LOW))", "a bracket of one")]
	[InlineData("CREATE SPATIAL INDEX i ON t (g) WITH (CELLS_PER_OBJECT = 16)", "a value alone")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k ON (c, FILEGROUP fg)", "the catalog first")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k ON (FILEGROUP fg, c)", "the filegroup first")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k ON (c)", "the catalog in brackets")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k ON c", "the catalog alone")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX k ON (FILEGROUP fg)", "the filegroup alone")]
	[InlineData("CREATE TRIGGER tr ON DATABASE FOR INSERT AS PRINT 1", "statements on a scope")]
	[InlineData("CREATE TRIGGER tr ON DATABASE FOR CREATE_TABLE AS PRINT 1", "event names on a scope")]
	[InlineData("CREATE TRIGGER tr ON dbo.t FOR CREATE_TABLE AS PRINT 1", "event names on a table")]
	[InlineData("CREATE TRIGGER tr ON dbo.t FOR INSERT AS PRINT 1", "statements on a table")]
	public void The_bridge_builds_the_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"unbracketed" => made is Ast.Statement.CreateIndex { Index: { OptionsUnbracketed: true, Options: [{ Name.Text: "PAD_INDEX", Value: null }, { Name.Text: "FILLFACTOR", Value: Ast.LiteralValue.Numeric }] } },
			"bracketed" => made is Ast.Statement.CreateIndex { Index: { OptionsUnbracketed: false, Options.Count: 2 } },
			"a word alone" => made is Ast.Statement.CreateIndex { Index: { OptionsUnbracketed: true, Options: [{ Name.Text: "SORTED_DATA", Value: null }] } },
			"a word and a placement" => made is Ast.Statement.CreateIndex { Index: { OptionsUnbracketed: true, Options: [{ Name.Text: "PAD_INDEX", Value: null }], Placements: [{ Kind: Ast.PlacementKind.On }] } },
			"tracking with no =" => made is Ast.Statement.CreateFullTextIndex { OptionsUnbracketed: true, Options: [{ Name.Text: "CHANGE_TRACKING", Words.Count: 0, EqualsOmitted: true, Value: Ast.Expression.Reference { Name: [{ Text: "AUTO" }] } }] },
			"tracking with =" => made is Ast.Statement.CreateFullTextIndex { Options: [{ Name.Text: "CHANGE_TRACKING", EqualsOmitted: false, Value: Ast.Expression.Reference }] },
			"a property list" => made is Ast.Statement.CreateFullTextIndex { OptionsUnbracketed: false, Options: [{ Name.Text: "SEARCH", Words: [{ Text: "PROPERTY" }, { Text: "LIST" }], EqualsOmitted: false, Value: Ast.Expression.Reference { Name: [{ Text: "p" }] } }] },
			"no population" => made is Ast.Statement.CreateFullTextIndex { Options: [{ Name.Text: "CHANGE_TRACKING", EqualsOmitted: true }, { Name.Text: "NO", Words: [{ Text: "POPULATION" }], Value: null, EqualsOmitted: false }] },
			"the system stoplist" => made is Ast.Statement.CreateFullTextIndex { Options: [{ Name.Text: "STOPLIST", EqualsOmitted: true, Value: Ast.Expression.Reference { Name: [{ Text: "system", Style: Ast.IdentifierStyle.Regular }] } }] },
			"a stoplist called system" => made is Ast.Statement.CreateFullTextIndex { Options: [{ Name.Text: "STOPLIST", EqualsOmitted: true, Value: Ast.Expression.Reference { Name: [{ Text: "[system]", Style: Ast.IdentifierStyle.Bracketed }] } }] },
			"columns added with no population" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.AddColumns { Options: [{ Name.Text: "NO", Words: [{ Text: "POPULATION" }] }] } },
			"columns added" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.AddColumns { Options.Count: 0 } },
			"a name of three words" => made is Ast.Statement.CreateTable { Options: [{ Name.Text: "CLUSTERED", Words: [{ Text: "COLUMNSTORE" }, { Text: "INDEX" }], Value: null }, { Name.Text: "DISTRIBUTION" }] },
			"a name of two words and its list" => made is Ast.Statement.CreateTable { Options: [{ Name.Text: "CLUSTERED", Words: [{ Text: "INDEX" }], Value: null, Options: [{ Name.Text: "a" }] }, _] },
			"a bracket by position" => made is Ast.Statement.CreateSpatialIndex { Options: [{ Value: Ast.Expression.Row { RowKeyword: false, Items: [Ast.LiteralValue.Numeric { Text: "-1.5" }, _, _, _] }, Options.Count: 0 }] },
			"a bracket by name" => made is Ast.Statement.CreateSpatialIndex { Options: [{ Value: null, EqualsOmitted: false, Options: [{ Name.Text: "xmin", Value: Ast.LiteralValue.Numeric { Text: "-1" } }, _, _, _] }] },
			"a bracket of one" => made is Ast.Statement.CreateSpatialIndex { Options: [{ Value: Ast.Expression.Row { Items: [Ast.Expression.Reference] } }] },
			"a value alone" => made is Ast.Statement.CreateSpatialIndex { Options: [{ Value: Ast.LiteralValue.Numeric, Options.Count: 0 }] },
			"the catalog first" => made is Ast.Statement.CreateFullTextIndex { Catalog.Text: "c", FileGroup.Text: "fg", FileGroupFirst: false, CatalogBracketed: false },
			"the filegroup first" => made is Ast.Statement.CreateFullTextIndex { Catalog.Text: "c", FileGroup.Text: "fg", FileGroupFirst: true, CatalogBracketed: false },
			"the catalog in brackets" => made is Ast.Statement.CreateFullTextIndex { Catalog.Text: "c", FileGroup: null, FileGroupFirst: false, CatalogBracketed: true },
			"the catalog alone" => made is Ast.Statement.CreateFullTextIndex { Catalog.Text: "c", FileGroup: null, FileGroupFirst: false, CatalogBracketed: false },
			"the filegroup alone" => made is Ast.Statement.CreateFullTextIndex { Catalog: null, FileGroup.Text: "fg", FileGroupFirst: false, CatalogBracketed: false },
			"statements on a scope" => made is Ast.Statement.CreateTrigger { Scope: Ast.TriggerScope.Database, Table: null, Events: [{ Kind: Ast.TriggerEventKind.Insert }], EventNames.Count: 0 },
			"event names on a scope" => made is Ast.Statement.CreateTrigger { Scope: Ast.TriggerScope.Database, Table: null, Events.Count: 0, EventNames: [{ Text: "CREATE_TABLE" }] },
			"event names on a table" => made is Ast.Statement.CreateTrigger { Scope: null, Table: not null, Events.Count: 0, EventNames: [{ Text: "CREATE_TABLE" }] },
			"statements on a table" => made is Ast.Statement.CreateTrigger { Scope: null, Table: not null, Events: [{ Kind: Ast.TriggerEventKind.Insert }], EventNames.Count: 0 },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
