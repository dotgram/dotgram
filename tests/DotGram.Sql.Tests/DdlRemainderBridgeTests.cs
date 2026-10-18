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
	[InlineData("alter database d1 modify filegroup fg1 readonly", "READONLY in lower case")]
	[InlineData("CREATE JSON INDEX ix ON dbo.t (j) FOR ('$.a', N'$.b') WITH (MAXDOP = 2)", "a JSON index with paths")]
	[InlineData("CREATE JSON INDEX ix ON dbo.t (j)", "a JSON index")]
	[InlineData("CREATE SPATIAL INDEX i ON t (g) USING [GEOMETRY_GRID]", "a grid in brackets")]
	[InlineData("CREATE SPATIAL INDEX i ON t (g) USING GEOMETRY_GRID", "a grid")]
	[InlineData("CREATE MATERIALIZED VIEW v WITH (DISTRIBUTION = HASH (a), FOR_APPEND) AS SELECT a, COUNT_BIG (*) AS n FROM t GROUP BY a", "a materialized view")]
	[InlineData("CREATE VIEW v AS SELECT a FROM t", "a view")]
	[InlineData("WITH CHANGE_TRACKING_CONTEXT (0xff), c (a) AS (SELECT a FROM t) DELETE t FROM t", "a change tracking context and a CTE")]
	[InlineData("with change_tracking_context (@v) update t set a = 1", "a change tracking context alone")]
	[InlineData("CREATE TABLE t (c XML COLUMN_SET FOR ALL_SPARSE_COLUMNS NULL)", "a column set")]
	[InlineData("ALTER TABLE t ALTER COLUMN c VARBINARY (MAX) SPARSE FILESTREAM NOT NULL", "an altered column's front and nullability")]
	[InlineData("ALTER TABLE t ALTER COLUMN c NVARCHAR (10) COLLATE x", "an altered column's collation")]
	[InlineData("ALTER TABLE t ADD c INT DEFAULT 1 WITH VALUES", "a column's default WITH VALUES")]
	[InlineData("ALTER TABLE t ADD c INT DEFAULT 1", "a column's default")]
	[InlineData("ALTER TABLE t ADD CONSTRAINT d DEFAULT 1 FOR c WITH VALUES", "a default constraint WITH VALUES")]
	[InlineData("CREATE TABLE t (i INT NOT NULL PRIMARY KEY NONCLUSTERED HASH WITH (BUCKET_COUNT = 8)) WITH (MEMORY_OPTIMIZED = ON)", "a hash key")]
	[InlineData("CREATE TABLE t (i INT NOT NULL PRIMARY KEY NONCLUSTERED)", "a key")]
	[InlineData("ALTER TABLE t SPLIT RANGE ('2004-01-01')", "SPLIT RANGE")]
	[InlineData("ALTER TABLE t MERGE RANGE (NULL)", "MERGE RANGE")]
	[InlineData("ALTER TABLE t ENABLE FILETABLE_NAMESPACE", "ENABLE FILETABLE_NAMESPACE")]
	[InlineData("alter table t disable filetable_namespace", "DISABLE FILETABLE_NAMESPACE")]
	[InlineData("ALTER TABLE t DROP PERIOD FOR SYSTEM_TIME", "a period dropped")]
	[InlineData("ALTER TABLE t DROP PERIOD FOR SYSTEM_TIME, COLUMN a", "a period dropped in a list")]
	[InlineData("CREATE TABLE t (c DECIMAL (MAX))", "DECIMAL (MAX) in a column")]
	[InlineData("CREATE TABLE t (c DECIMAL (10))", "DECIMAL (10) in a column")]
	[InlineData("SELECT CAST (a AS int(max))", "int(max) in a cast")]
	[InlineData("CREATE XML INDEX i ON t (c) USING XML INDEX sxi FOR (PATH)", "a selective index's path called PATH")]
	[InlineData("CREATE XML INDEX i ON t (c) USING XML INDEX p FOR PATH", "a secondary index FOR PATH")]
	[InlineData("CREATE SELECTIVE XML INDEX sxi ON t (c) FOR (p1 = '/a' AS XQUERY 'xs:string' MAXLENGTH (5) SINGLETON, p2 = '/b' AS SQL NVARCHAR (10))", "promoted paths")]
	[InlineData("CREATE SELECTIVE XML INDEX sxi ON t (c) FOR (p1 = '/a' SINGLETON AS XQUERY 'xs:string')", "hints in another order")]
	[InlineData("CREATE SELECTIVE XML INDEX sxi ON t (c) WITH XMLNAMESPACES ('urn:a' AS ns) FOR (p1 = '/ns:a' AS XQUERY)", "namespaces and a bare XQUERY")]
	[InlineData("ALTER INDEX sxi ON t FOR (ADD p1 = '/a', REMOVE p2, ALTER PATH p3 = '/c' AS XQUERY 'node()')", "paths changed")]
	[InlineData("SELECT * FROM OPENJSON (a + b, N'$')", "a sum, not a word and a value")]
	[InlineData("SELECT * FROM OPENJSON (t.c)", "a qualified column, not a word and a value")]
	[InlineData("SELECT * FROM CHANGETABLE (CHANGES t, 0) AS c", "a word and its value")]
	[InlineData("restore labelonly from disk = 'd:'", "a tail in lower case")]
	[InlineData("create proc p1 @a int with execute as caller as print 'hi'", "EXECUTE AS caller")]
	[InlineData("CREATE PROCEDURE p1 @a INT WITH EXECUTE AS 'caller' AS PRINT 'hi'", "EXECUTE AS a user called caller")]
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
			"READONLY in lower case" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.ModifyFileGroup { Change: Ast.FileGroupChange.Updatability { Kind: Ast.FileGroupUpdatability.ReadOnly, Underscored: false } } },
			"a tail in lower case" => made is Ast.Statement.RestoreLabelOnly { From: [Ast.BackupDevice.Physical { Kind: Ast.DeviceKind.Disk }] },
			"EXECUTE AS caller" => made is Ast.Statement.CreateRoutine { Definition.Options: [Ast.RoutineOption.ExecuteAs { Context: { Kind: Ast.ExecutionContextKind.Caller, Name: null } }] },
			"EXECUTE AS a user called caller" => made is Ast.Statement.CreateRoutine { Definition.Options: [Ast.RoutineOption.ExecuteAs { Context: { Kind: Ast.ExecutionContextKind.User, Name: not null } }] },
			"a sum, not a word and a value" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.OpenJson { Json: Ast.Expression.Binary }] } },
			"a qualified column, not a word and a value" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.OpenJson { Json: Ast.Expression.Reference { Name.Count: 2 } }] } },
			"a word and its value" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.ChangeTableChanges] } },
			"a selective index's path called PATH" => made is Ast.Statement.CreateXmlIndex { Using.Text: "sxi", For: null, ForPath.Text: "PATH" },
			"a secondary index FOR PATH" => made is Ast.Statement.CreateXmlIndex { Using.Text: "p", For: Ast.XmlIndexFor.Path, ForPath: null },
			"promoted paths" => made is Ast.Statement.CreateSelectiveXmlIndex { Column.Text: "c", XmlNamespaces.Count: 0, Paths: [{ Name.Text: "p1", Path.Text: "'/a'", Hints: [Ast.PathHint.XQuery { Type.Text: "'xs:string'", MaxLength.Text: "5" }, Ast.PathHint.Singleton] }, { Name.Text: "p2", Hints: [Ast.PathHint.Sql { Type: Ast.DataType.Character }] }] },
			"hints in another order" => made is Ast.Statement.CreateSelectiveXmlIndex { Paths: [{ Hints: [Ast.PathHint.Singleton, Ast.PathHint.XQuery] }] },
			"namespaces and a bare XQUERY" => made is Ast.Statement.CreateSelectiveXmlIndex { XmlNamespaces: [{ Prefix.Text: "ns" }], Paths: [{ Hints: [Ast.PathHint.XQuery { Type: null, MaxLength: null }] }] },
			"paths changed" => made is Ast.Statement.AlterIndex { Name.Text: "sxi", Action: Ast.IndexAlteration.Paths { Actions: [Ast.PathAction.Add { Path.Name.Text: "p1" }, Ast.PathAction.Remove { Name.Text: "p2" }, Ast.PathAction.Alter { Path.Hints: [Ast.PathHint.XQuery] }] } },
			"a JSON index with paths" => made is Ast.Statement.CreateJsonIndex { Column.Text: "j", Paths: [{ Kind: Ast.StringLiteralKind.Character }, { Kind: Ast.StringLiteralKind.National }], Options: [{ Name.Text: "MAXDOP" }] },
			"a JSON index" => made is Ast.Statement.CreateJsonIndex { Paths.Count: 0, Options.Count: 0 },
			"a grid in brackets" => made is Ast.Statement.CreateSpatialIndex { Using: Ast.SpatialTessellation.GeometryGrid, UsingBracketed: true },
			"a grid" => made is Ast.Statement.CreateSpatialIndex { Using: Ast.SpatialTessellation.GeometryGrid, UsingBracketed: false },
			"a materialized view" => made is Ast.Statement.CreateMaterializedView { Name: [{ Text: "v" }], Options: [{ Name.Text: "DISTRIBUTION" }, { Name.Text: "FOR_APPEND", Value: null }] },
			"a view" => made is Ast.Statement.CreateView,
			"a change tracking context and a CTE" => made is Ast.Statement.Delete { With: { ChangeTrackingContext: Ast.LiteralValue.Binary, XmlNamespaces.Count: 0, Items.Count: 1 } },
			"a change tracking context alone" => made is Ast.Statement.Update { With: { ChangeTrackingContext: Ast.Expression.Variable, Items.Count: 0 } },
			"a column set" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements { Items: [Ast.TableElement.Column { Definition.Options: [Ast.ColumnOption.ColumnSet, Ast.ColumnOption.Null] }] } },
			"an altered column's front and nullability" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.AlterColumn { Action: Ast.AlterColumnAction.SetDataType { ColumnOptions: [Ast.ColumnOption.Sparse, Ast.ColumnOption.Filestream], Nullability: Ast.ColumnNullability.NotNull } }] },
			"an altered column's collation" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.AlterColumn { Action: Ast.AlterColumnAction.SetDataType { ColumnOptions: [Ast.ColumnOption.Collation], Nullability: null } }] },
			"a column's default WITH VALUES" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.AddColumn { Column.Options: [Ast.ColumnOption.Default { WithValues: true }] }] },
			"a column's default" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.AddColumn { Column.Options: [Ast.ColumnOption.Default { WithValues: false }] }] },
			"a default constraint WITH VALUES" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.AddConstraint { Constraint: Ast.Constraint.Default { WithValues: true, Column.Text: "c" } }] },
			"a hash key" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements { Items: [Ast.TableElement.Column { Definition.Options: [_, Ast.ColumnOption.Constraint { Value: Ast.Constraint.Unique { Storage: Ast.IndexStorage.Hash, Clustering: Ast.IndexClustering.Nonclustered, Options.Count: 1 } }] }] } },
			"a key" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements { Items: [Ast.TableElement.Column { Definition.Options: [_, Ast.ColumnOption.Constraint { Value: Ast.Constraint.Unique { Storage: null } }] }] } },
			"SPLIT RANGE" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.SplitRange { Boundary: Ast.LiteralValue.String }] },
			"MERGE RANGE" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.MergeRange { Boundary: Ast.LiteralValue.Null }] },
			"ENABLE FILETABLE_NAMESPACE" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.FileTableNamespace { Enable: true }] },
			"DISABLE FILETABLE_NAMESPACE" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.FileTableNamespace { Enable: false }] },
			"a period dropped" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.DropPeriod { Kind: Ast.PeriodKind.SystemTime, Behavior: null }] },
			"a period dropped in a list" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.Drop { Items: [Ast.AlterTableAction.DropPeriod, Ast.AlterTableAction.DropColumn] }] },
			"DECIMAL (MAX) in a column" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements { Items: [Ast.TableElement.Column { Definition.Type: Ast.DataType.Named { Max: true, Arguments.Count: 0, Name: [{ Text: "DECIMAL" }] } }] } },
			"DECIMAL (10) in a column" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements { Items: [Ast.TableElement.Column { Definition.Type: Ast.DataType.Numeric }] } },
			"int(max) in a cast" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.Cast { Target: Ast.DataType.Named { Max: true } } }] } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	/// <summary>
	/// A tail the old tree keeps compares token by token: a key word without its case and the blanks between tokens, and
	/// nothing else — not a string, a name in brackets, a plain name, a word or the order of the words.
	/// </summary>
	[Theory]
	[InlineData("Statement.RestoreLabelOnly.Tail", "from disk = 'd:'", "FROM DISK = 'd:'", true)]
	[InlineData("Statement.BackupDatabase.Tail", "to disk = 'd:' with format, compression", "TO DISK = 'd:' WITH  format ,compression", true)]
	[InlineData("Statement.BackupDatabase.Tail", "with encryption(algorithm = AES_128)", "WITH encryption (algorithm = AES_128)", true)]
	[InlineData("Statement.RestoreLabelOnly.Tail", "from disk = 'D:'", "FROM DISK = 'd:'", false)]
	[InlineData("Statement.AlterPartitionScheme.Tail", "next used fg1", "NEXT USED FG1", false)]
	[InlineData("Statement.AlterPartitionScheme.Tail", "next used [fg 1]", "NEXT USED [FG 1]", false)]
	[InlineData("Statement.AlterPartitionScheme.Tail", "next used [fg 1]", "NEXT USED [fg  1]", false)]
	[InlineData("Statement.BackupDatabase.Tail", "with format, compression", "WITH compression, format", false)]
	[InlineData("Statement.AlterDatabaseModifyFileGroup.Tail", "fg1 readonly", "fg1 READ_ONLY", false)]
	[InlineData("Statement.RestoreDatabase.Tail", "with stats = 10", "WITH stats = 20", false)]
	public void A_tail_compares_key_words_without_case_and_nothing_else(string member, string one, string two, bool same)
	{
		Assert.Equal(same, Trees.SameTokens(new object(), member, one, two));
	}
}
