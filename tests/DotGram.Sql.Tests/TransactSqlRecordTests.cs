using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// Which record and member a T-SQL text reads as where another would write the same text, which a round trip cannot see;
/// the names and types its rules build; and where a text the tree does not hold is an extension.
/// </summary>
public sealed class TransactSqlRecordTests
{
	/// <summary>
	/// A name the old tree keeps as text is split into its parts, each with the style its quotes say.
	/// </summary>
	[Theory]
	[InlineData("t",           "t")]
	[InlineData("dbo.t",       "t|t")]
	[InlineData("[dbo].[t]",   "b|b")]
	[InlineData("[a.b].c",     "b|t")]
	[InlineData("[a]]b].c",    "b|t")]
	[InlineData("\"x\".y",     "d|t")]
	[InlineData("a..b",        "t|t|t")]
	[InlineData("#tmp",        "t")]
	[InlineData("s.db.dbo.t",  "t|t|t|t")]
	public void A_name_is_split_into_its_parts(string text, string styles)
	{
		var name = Bridge.Values.Name("test", text);

		Assert.Equal(text, string.Join('.', name.Parts.Select(static one => one.Text)));
		Assert.Equal(styles, string.Join('|', name.Parts.Select(static one => one.Style switch
		{
			Ast.IdentifierStyle.Bracketed => "b",
			Ast.IdentifierStyle.Delimited => "d",
			_                             => "t",
		})));
	}

	/// <summary>
	/// A block comment nests, and a word after a comment is the word after the node.
	/// </summary>
	[Fact]
	public void Comments_are_skipped_and_nest()
	{
		Assert.Equal(["[realowner]"], SqlTokens.Of("/* outer /* inner */ cache -- */ [realowner]").Select(static one => one.Text));

		const string text = "SELECT TOP (1) a /* c */ AS x FROM t";

		var made   = (Ast.Statement.Select)TransactSqlParser.Located.ParseStatement(text);
		var column = (Ast.SelectColumn.ExpressionItem)((Ast.Query.Select)made.Query).Columns[0];

		Assert.True(column.AsKeyword);
	}

	/// <summary>
	/// The owner after a nested comment is the owner, or the statement is a gap: never the word inside it.
	/// </summary>
	[Fact]
	public void A_schema_owner_after_a_nested_comment_is_kept()
	{
		const string text = "CREATE SCHEMA s AUTHORIZATION /* outer /* inner */ cache -- */ [realowner]";

		var made = TransactSqlParser.Located.ParseStatement(text);

		if (made is not Ast.Statement.Extension)
			Assert.Equal("[realowner]", ((Ast.Statement.CreateSchema)made).Authorization!.Name.Text);
	}

	/// <summary>
	/// A name part that is none is a gap, never a name in the new tree: a number as the schema collection of an
	/// <c>xml</c> and a string as the name after OPENDATASOURCE. (A type word after a delimited name, <c>[sys]."Char"
	/// varying</c>, is the standard's spelling written with that name.)
	/// </summary>
	[Theory]
	[InlineData("CREATE TABLE t1 (c1 xml(10))")]
	[InlineData("SELECT * FROM OPENDATASOURCE ('SQLOLEDB', 'Data Source=s').'Something'")]
	[InlineData("SELECT * FROM OPENDATASOURCE ('SQLOLEDB', 'Data Source=s').'Something' AS Z")]
	public void A_name_part_that_is_none_is_a_gap(string text)
	{
		Assert.IsType<DotGram.Sql.Ast.Statement.Extension>(TransactSqlParser.Located.ParseStatement(text));
	}

	/// <summary>A delimited name with a blank after it is the blank's gap, not a part that is no name.</summary>
	[Theory]
	[InlineData("SELECT [a] .b")]
	[InlineData("SELECT * FROM [dbo] .[t]")]
	[InlineData("EXEC [dbo] . [p]")]
	public void A_delimited_name_with_a_blank_is_the_blank_s_gap(string text)
	{
		var made = Assert.IsType<Ast.Statement.Extension>(TransactSqlParser.Located.ParseStatement(text));

		Assert.DoesNotContain("that is no name", made.Kind, StringComparison.Ordinal);
	}

	/// <summary>A user-defined type is the standard's record for it.</summary>
	[Fact]
	public void A_user_defined_type_is_the_standards()
	{
		const string text = "SELECT CAST (a AS MyType) FROM t";

		Assert.Equal(SqlStandardParser.ParseStatement(text), TransactSqlParser.Located.ParseStatement(text));
	}

	/// <summary>
	/// Every one of T-SQL's system types, bracketed, is the type its bare word is: a standard spelling of one word the
	/// standard's record written as a name, <c>vector</c> its own record written so, and the rest <c>DataType.Named</c>.
	/// </summary>
	[Theory]
	[InlineData("bigint")] [InlineData("binary")] [InlineData("bit")] [InlineData("char")] [InlineData("cursor")]
	[InlineData("date")] [InlineData("datetime")] [InlineData("datetime2")] [InlineData("datetimeoffset")]
	[InlineData("decimal")] [InlineData("float")] [InlineData("geography")] [InlineData("geometry")]
	[InlineData("hierarchyid")] [InlineData("image")] [InlineData("int")] [InlineData("json")] [InlineData("money")]
	[InlineData("nchar")] [InlineData("ntext")] [InlineData("numeric")] [InlineData("nvarchar")] [InlineData("real")]
	[InlineData("rowversion")] [InlineData("smalldatetime")] [InlineData("smallint")] [InlineData("smallmoney")]
	[InlineData("sql_variant")] [InlineData("sysname")] [InlineData("table")] [InlineData("text")] [InlineData("time")]
	[InlineData("timestamp")] [InlineData("tinyint")] [InlineData("uniqueidentifier")] [InlineData("varbinary")]
	[InlineData("varchar")] [InlineData("vector")] [InlineData("xml")] [InlineData("dec")] [InlineData("integer")]
	[InlineData("character")]
	public void A_system_type_is_named_however_quoted(string name)
	{
		Assert.Contains(name, Bridge.Values.SystemTypes);

		var written = new Ast.QualifiedName([new Ast.Identifier("[" + name + "]", Ast.IdentifierStyle.Bracketed)]);
		var type    = Bridge.Values.Type(new object(), "[" + name + "]");

		if (Bridge.Values.StandardWords.Contains(name) || name == "vector")
			Assert.Equal(Bridge.Values.Type(new object(), name) with { WrittenName = written }, type);
		else
			Assert.Equal(new Ast.DataType.Named(written, []), type);
	}

	/// <summary>
	/// Which record and member the bridge builds for a data change statement's T-SQL parts: what the round trip
	/// cannot see where another record would write the same text.
	/// </summary>
	[Theory]
	[InlineData("INSERT OVER t DEFAULT VALUES",                                    "Insert.Into Over")]
	[InlineData("INSERT t VALUES (1)",                                             "Insert.Into null")]
	[InlineData("INSERT INTO OPENDATASOURCE ('p', 'i').db.dbo.t VALUES (1)",       "Insert.Target DataSource")]
	[InlineData("DELETE t WHERE a = 1",                                            "Delete.FromOmitted")]
	[InlineData("DELETE TOP (1) FROM t",                                           "Delete.From written")]
	[InlineData("WITH c AS (SELECT 1 AS a) DELETE FROM t",                         "Delete.With")]
	[InlineData("DELETE FROM dbo.f(1)",                                            "Delete.Target Function")]
	[InlineData("DELETE t OUTPUT deleted.a INTO @log (a) OUTPUT deleted.b",        "Delete.Output")]
	[InlineData("UPDATE @t WITH (NOLOCK) SET a = 1",                               "Update.Target Variable")]
	[InlineData("UPDATE t SET a = 1 OUTPUT inserted.* FROM t JOIN u ON t.a = u.a OPTION (RECOMPILE)", "Update.Output From Hints")]
	[InlineData("UPDATE OPENQUERY (s, 'SELECT a FROM t') SET a = 1",               "Update.Target OpenQuery")]
	[InlineData("MERGE t USING u ON t.a = u.a WHEN MATCHED THEN DELETE OUTPUT $action;", "Merge.IntoOmitted Output")]
	[InlineData("UPDATE t SET @v = c += 1",                                        "Assignment variable through column")]
	[InlineData("UPDATE t SET @v = c = 1",                                         "Assignment variable through column, plainly")]
	[InlineData("UPDATE t SET @v -= 1",                                            "Assignment variable")]
	[InlineData("UPDATE t SET c ||= 'x'",                                          "Assignment compound")]
	[InlineData("UPDATE t SET c.WRITE (@x, 0, NULL)",                              "Assignment call")]
	[InlineData("UPDATE t SET a = 1",                                              "Assignment plain")]
	[InlineData("INSERT INTO t SELECT a FROM (DELETE u OUTPUT deleted.a) AS c (a) WHERE a > 1", "Composable")]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS (1, 3 TO 5))",                  "Truncate partitions")]
	[InlineData("BULK INSERT t FROM 'f.csv' WITH (ORDER (a ASC, b DESC), TABLOCK, BATCHSIZE = 10)", "BulkInsert")]
	[InlineData("INSERT BULK t (a INT NOT NULL, b VARCHAR(10) COLLATE Latin1_General_CI_AS, TIMESTAMP) WITH (TABLOCK)", "InsertBulk")]
	[InlineData("READTEXT t.c @p 0 10 HOLDLOCK",                                   "ReadText")]
	[InlineData("WRITETEXT BULK t.c @p TIMESTAMP = 0x0A WITH LOG",                 "WriteText")]
	[InlineData("UPDATETEXT t.c @p NULL -1 WITH LOG u.d @q",                       "UpdateText source")]
	[InlineData("MERGE INTO t USING u ON t.a = u.a WHEN NOT MATCHED BY SOURCE AND t.b = 1 THEN UPDATE SET b = 2 WHEN NOT MATCHED BY TARGET THEN INSERT DEFAULT VALUES;", "Merge.BySource ByTarget DefaultValues")]
	public void The_bridge_builds_the_data_change_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		var holds = expected switch
		{
			"Insert.Into Over"            => made is Ast.Statement.Insert { Into: Ast.IntoKeyword.Over, SourceValue: Ast.InsertSource.DefaultValues },
			"Insert.Into null"            => made is Ast.Statement.Insert { Into: null },
			"Insert.Target DataSource"    => made is Ast.Statement.Insert { Into: Ast.IntoKeyword.Into, Target: Ast.TableSource.Named { DataSource: not null } },
			"Delete.FromOmitted"          => made is Ast.Statement.Delete { FromOmitted: true, Where: not null },
			"Delete.From written"         => made is Ast.Statement.Delete { FromOmitted: false, Top: not null },
			"Delete.With"                 => made is Ast.Statement.Delete { With.Items.Count: 1, FromOmitted: false },
			"Delete.Target Function"      => made is Ast.Statement.Delete { Target: Ast.TableSource.Function },
			"Delete.Output"               => made is Ast.Statement.Delete
			{
				Output: { Into: Ast.TableSource.Variable { Name.Text: "@log" }, Columns: [{ Text: "a" }], Next: { Into: null, Items.Count: 1 } },
			},
			"Update.Target Variable"      => made is Ast.Statement.Update { Target: Ast.TableSource.Variable { Hints.Count: 1 } },
			"Update.Output From Hints"    => made is Ast.Statement.Update
			{
				Output.Items: [Ast.SelectColumn.QualifiedAll], From.Sources: [Ast.TableSource.Join], Hints.Count: 1,
			},
			"Update.Target OpenQuery"     => made is Ast.Statement.Update { Target: Ast.TableSource.OpenQuery },
			"Merge.BySource ByTarget DefaultValues" => made is Ast.Statement.Merge
			{
				IntoOmitted: false,
				Clauses:
				[
					Ast.MergeClause.NotMatchedBySource { Condition: not null, Action: Ast.MergeMatchedAction.Update },
					Ast.MergeClause.NotMatched { ByTarget: true, Action: { DefaultValues: true, Values.Count: 0 } },
				],
			},
			"Assignment variable through column" => made is Ast.Statement.Update
			{
				Assignments: [{ Variable.Name.Text: "@v", Targets: [{ Name.Parts: [{ Text: "c" }] }], Operator: Ast.AssignmentOperator.Add }],
			},
			"Assignment variable through column, plainly" => made is Ast.Statement.Update
			{
				Assignments:
				[
					{
						Variable.Name.Text: "@v",
						Targets: [{ Name.Parts: [{ Text: "c" }], Index: null, MutationPath: null }],
						Operator: Ast.AssignmentOperator.Assign,
						Value: Ast.LiteralValue.Numeric { Text: "1" },
					},
				],
			},
			"Assignment variable"         => made is Ast.Statement.Update { Assignments: [{ Variable.Name.Text: "@v", Targets.Count: 0, Operator: Ast.AssignmentOperator.Subtract }] },
			"Assignment compound"         => made is Ast.Statement.Update { Assignments: [{ Variable: null, Targets.Count: 1, Operator: Ast.AssignmentOperator.Concatenate }] },
			"Assignment call"             => made is Ast.Statement.Update
			{
				Assignments: [{ Variable: null, Targets.Count: 0, Value: Ast.Expression.Member { Kind: Ast.MemberAccessKind.Dot, Name.Text: "WRITE", Arguments.Count: 3, Target: Ast.Expression.Reference } }],
			},
			"Composable"                  => made is Ast.Statement.Insert
			{
				SourceValue: Ast.InsertSource.Subquery
				{
					Query: Ast.Query.Select
					{
						From.Sources: [Ast.TableSource.DataChange { Option: null, Change: Ast.Statement.Delete { Output: not null }, Alias: { AsKeyword: true, Columns.Count: 1 } }],
					},
				},
			},
			"BulkInsert"                  => made is Ast.Statement.BulkInsert
			{
				File: Ast.LiteralValue,
				Options: [{ Name.Text: "ORDER", Value: null, Order: { Unique: false, Columns.Count: 2 } }, { Name.Text: "TABLOCK", Value: null }, { Value: not null }],
			},
			"InsertBulk"                  => made is Ast.Statement.InsertBulk
			{
				Columns:
				[
					{ Type: not null, Options: [Ast.ColumnOption.Constraint { Value: Ast.Constraint.NotNull }], Constraints.Count: 0 },
					{ Options: [Ast.ColumnOption.Collation], Collation: null },
					{ Type: null, Options.Count: 0 },
				],
				Options.Count: 1,
			},
			"Truncate partitions"         => made is Ast.Statement.TruncateTable { Partitions: [{ To: null }, { To: not null }] },
			"ReadText"                    => made is Ast.Statement.ReadText { HoldLock: true, Column.Parts.Count: 2, Pointer: Ast.Expression.Variable },
			"WriteText"                   => made is Ast.Statement.WriteText { Bulk: true, Log: true, Data: null, Timestamp: Ast.LiteralValue.Binary { ZeroX: true } },
			"UpdateText source"           => made is Ast.Statement.UpdateText
			{
				Offset: Ast.LiteralValue.Null, Log: true, Data: null, Source.Parts.Count: 2, SourcePointer: Ast.Expression.Variable,
			},
			"Assignment plain"            => made is Ast.Statement.Update { Assignments: [{ Variable: null, Operator: Ast.AssignmentOperator.Assign, Targets.Count: 1 }] },
			_                             => made is Ast.Statement.Merge
			{
				IntoOmitted: true,
				Output.Items: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.Reference { Name.Parts: [{ Text: "$action" }] }, Alias: null }],
			},
		};

		Assert.True(holds, $"{text} built {made}");

		ReadBack.Holds(text);
	}

	/// <summary>
	/// Which record and member the bridge builds for the table statements' T-SQL parts, the DROP list grouped by its
	/// words both ways, and that each converts.
	/// </summary>
	[Theory]
	[InlineData("ALTER TABLE t DROP COLUMN a, b, CONSTRAINT c, d", "Drop columns then constraints")]
	[InlineData("ALTER TABLE t DROP c, d, COLUMN a", "Drop constraints first without the word")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT IF EXISTS c WITH (ONLINE = ON)", "DropConstraint options")]
	[InlineData("CREATE TABLE t (c INT GENERATED ALWAYS AS ROW START HIDDEN NOT NULL)", "Generated then hidden")]
	[InlineData("CREATE TABLE t (c INT CONSTRAINT d DEFAULT 1 NULL)", "Named default and NULL")]
	[InlineData("CREATE TABLE t (b INT, c AS b + 1 PERSISTED)", "Computed")]
	[InlineData("CREATE TABLE t (a INT, CONSTRAINT p PRIMARY KEY CLUSTERED (a DESC) WITH (FILLFACTOR = 80) ON [PRIMARY])", "Key")]
	[InlineData("CREATE TABLE t (a INT PRIMARY KEY WITH FILLFACTOR = 80)", "Key unbracketed")]
	[InlineData("CREATE TABLE t (a INT, b INT, INDEX i UNIQUE NONCLUSTERED (a) INCLUDE (b) WHERE a > 1)", "Index")]
	[InlineData("CREATE TABLE t (a INT) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.h))", "System versioning option")]
	[InlineData("CREATE TABLE t (a INT) AS NODE", "Node")]
	[InlineData("CREATE TABLE t (a, b) AS SELECT 1, 2", "Table of a query")]
	[InlineData("CREATE TABLE e (a INT, CONSTRAINT c CONNECTION (n1 TO n2) ON DELETE CASCADE) AS EDGE", "Connection")]
	[InlineData("CREATE TABLE t (a INT, CONSTRAINT d DEFAULT 1 FOR a)", "Table default")]
	[InlineData("CREATE TABLE t (a INT FOREIGN KEY REFERENCES u (b) NOT FOR REPLICATION)", "Column foreign key")]
	[InlineData("ALTER TABLE t WITH NOCHECK ADD CONSTRAINT c CHECK (a > 1)", "Nocheck add")]
	[InlineData("ALTER TABLE t ADD a INT, b INT", "Add list")]
	[InlineData("ALTER TABLE t ALTER COLUMN a VARCHAR(10) COLLATE Latin1_General_BIN NOT NULL", "Alter column type")]
	[InlineData("ALTER TABLE t ALTER COLUMN a ADD MASKED WITH (FUNCTION = 'default()')", "Alter column property")]
	[InlineData("ALTER TABLE t SWITCH PARTITION 1 TO dbo.u PARTITION 2", "Switch")]
	[InlineData("ALTER TABLE t NOCHECK CONSTRAINT ALL", "Check constraints")]
	[InlineData("ALTER TABLE t ENABLE TRIGGER a, b", "Triggers")]
	[InlineData("ALTER TABLE t SET (LOCK_ESCALATION = AUTO)", "Set options")]
	[InlineData("ALTER TABLE t REBUILD PARTITION = ALL", "Rebuild")]
	[InlineData("INSERT BULK t (a INT NULL)", "Insert bulk explicit null")]
	[InlineData("ALTER TABLE t ENABLE TRIGGER ALL", "Triggers all")]
	[InlineData("CREATE TABLE t (a INT NOT NULL)", "Lone not null")]
	[InlineData("CREATE TABLE t (a VARCHAR(10) DEFAULT 'x' COLLATE Latin1_General_BIN)", "Default collated")]
	[InlineData("CREATE TABLE t (a VARCHAR(10) COLLATE Latin1_General_BIN)", "Type collated")]
	[InlineData("CREATE TABLE t (a INT, PRIMARY KEY (a))", "Plain key")]
	[InlineData("CREATE TABLE t (a INT) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo 2].[t history]))", "Option a bracketed name with a blank")]
	public void The_bridge_builds_the_table_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		static SqlList<Ast.TableElement> Elements(Ast.Statement made)
		{
			return made is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements elements } ? elements.Items : [];
		}

		static Ast.ColumnDefinition[] Columns(Ast.Statement made)
		{
			return [.. Elements(made).OfType<Ast.TableElement.Column>().Select(static one => one.Definition)];
		}

		var holds = expected switch
		{
			"Drop columns then constraints" => made is Ast.Statement.AlterTable
			{
				Actions:
				[
					Ast.AlterTableAction.Drop
					{
						Items:
						[
							Ast.AlterTableAction.DropColumn { Column.Text: "a", ColumnKeyword: true, Behavior: null },
							Ast.AlterTableAction.DropColumn { Column.Text: "b", ColumnKeyword: false },
							Ast.AlterTableAction.DropConstraint { Name.Parts: [{ Text: "c" }], ConstraintKeywordOmitted: false },
							Ast.AlterTableAction.DropConstraint { Name.Parts: [{ Text: "d" }], ConstraintKeywordOmitted: true },
						],
					},
				],
			},
			"Drop constraints first without the word" => made is Ast.Statement.AlterTable
			{
				Actions:
				[
					Ast.AlterTableAction.Drop
					{
						Items:
						[
							Ast.AlterTableAction.DropConstraint { Name.Parts: [{ Text: "c" }], ConstraintKeywordOmitted: true },
							Ast.AlterTableAction.DropConstraint { Name.Parts: [{ Text: "d" }], ConstraintKeywordOmitted: true },
							Ast.AlterTableAction.DropColumn { Column.Text: "a", ColumnKeyword: true },
						],
					},
				],
			},
			"DropConstraint options" => made is Ast.Statement.AlterTable
			{
				Actions: [Ast.AlterTableAction.DropConstraint { IfExists: true, Behavior: null, Options: [{ Name.Text: "ONLINE", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "ON" }] } }] }],
			},
			"Generated then hidden" => Columns(made) is [{ Options: [Ast.ColumnOption.GeneratedAlways { Kind: Ast.GeneratedAlwaysKind.RowStart }, Ast.ColumnOption.Hidden, Ast.ColumnOption.Constraint { Value: Ast.Constraint.NotNull }], Generation: null, Constraints.Count: 0 }],
			"Named default and NULL" => Columns(made) is [{ Options: [Ast.ColumnOption.Default { Name.Text: "d" }, Ast.ColumnOption.Null { Name: null }] }],
			"Computed" => Columns(made) is [_, { Type: null, Options: [Ast.ColumnOption.Computed, Ast.ColumnOption.Persisted] }],
			"Key" => Elements(made) is [_, Ast.TableElement.TableConstraint
			{
				Value: Ast.Constraint.Unique
				{
					Kind: Ast.UniqueKind.PrimaryKey, Columns.Count: 0, Clustering: Ast.IndexClustering.Clustered,
					Keys: [{ Direction: Ast.SortDirection.Desc }], Options: [{ Name.Text: "FILLFACTOR" }], OptionsUnbracketed: false,
					On: { Kind: Ast.PlacementKind.On, Target.Text: "[PRIMARY]" }, Name.Parts: [{ Text: "p" }],
				},
			}],
			"Key unbracketed" => Columns(made) is [{ Options: [Ast.ColumnOption.Constraint { Value: Ast.Constraint.Unique { Keys: null, OptionsUnbracketed: true, Options.Count: 1 } }] }],
			"Index" => Elements(made) is [_, _, Ast.TableElement.Index { Definition: { Unique: true, Clustering: Ast.IndexClustering.Nonclustered, Storage: null, Columns.Count: 1, Include: [{ Text: "b" }], Where: not null } }],
			"System versioning option" => made is Ast.Statement.CreateTable
			{
				SystemVersioning: false,
				Options: [{ Name.Text: "SYSTEM_VERSIONING", Value: Ast.Expression.Reference, Options: [{ Name.Text: "HISTORY_TABLE", Value: Ast.Expression.Reference { Name.Parts.Count: 2 } }] }],
			},
			"Node" => made is Ast.Statement.CreateTable { Kind: Ast.TableKind.Node, Placements.Count: 0 },
			"Table of a query" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.AsQuery { Data: null, Columns.Count: 2 }, Options.Count: 0 },
			"Connection" => Elements(made) is [_, Ast.TableElement.TableConstraint { Value: Ast.Constraint.Connection { Connections.Count: 1, OnDeleteCascade: true } }],
			"Table default" => Elements(made) is [_, Ast.TableElement.TableConstraint { Value: Ast.Constraint.Default { Column.Text: "a", Name.Parts: [{ Text: "d" }] } }],
			"Column foreign key" => Columns(made) is [{ Options: [Ast.ColumnOption.Constraint { Value: Ast.Constraint.ForeignKey { ForeignKeyWords: true, Columns.Count: 0, References.NotForReplication: true } }] }],
			"Nocheck add" => made is Ast.Statement.AlterTable { Check: Ast.ConstraintCheck.NoCheck, Actions: [Ast.AlterTableAction.AddConstraint { Constraint: Ast.Constraint.Check }] },
			"Add list" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.Add { Elements.Count: 2 }] },
			"Alter column type" => made is Ast.Statement.AlterTable
			{
				Actions: [Ast.AlterTableAction.AlterColumn { ColumnKeyword: true, Action: Ast.AlterColumnAction.SetDataType { SetDataTypeOmitted: true, ColumnOptions: [Ast.ColumnOption.Collation], Nullability: Ast.ColumnNullability.NotNull } }],
			},
			"Alter column property" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.AlterColumn { Action: Ast.AlterColumnAction.Property { Add: true, Kind: Ast.ColumnPropertyKind.Masked, MaskFunction: Ast.LiteralValue } }] },
			"Switch" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.Switch { Partition: not null, Target.Parts.Count: 2, TargetPartition: not null, Options.Count: 0 }] },
			"Check constraints" => made is Ast.Statement.AlterTable { Check: null, Actions: [Ast.AlterTableAction.CheckConstraints { Check: Ast.ConstraintCheck.NoCheck, All: true, Names.Count: 0 }] },
			"Triggers" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.Triggers { Enable: true, All: false, Names.Count: 2 }] },
			"Set options" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.SetOptions { Options: [{ Name.Text: "LOCK_ESCALATION" }] }] },
			"Rebuild" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.Rebuild { All: true, Partition: null }] },
			"Triggers all" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.Triggers { Enable: true, All: true, Names.Count: 0 }] },
			"Lone not null" => Columns(made) is [{ Options: [Ast.ColumnOption.Constraint { Value: Ast.Constraint.NotNull }], Generation: null, Constraints.Count: 0, Collation: null }],
			"Default collated" => Columns(made) is [{ Options: [Ast.ColumnOption.Default { Value: Ast.Expression.Collate { Value: Ast.LiteralValue } }], Generation: null, Collation: null, Type: Ast.DataType.Character { Collation: null } }],
			"Type collated" => Columns(made) is [{ Options: [Ast.ColumnOption.Collation], Collation: null, Type: Ast.DataType.Character { Collation: null } }],
			"Plain key" => Elements(made) is [_, Ast.TableElement.TableConstraint { Value: Ast.Constraint.Unique { Kind: Ast.UniqueKind.PrimaryKey, Columns.Count: 0, Keys: [{ Direction: null, Key: Ast.Expression.Reference }], Clustering: null } }],
			"Option a bracketed name with a blank" => made is Ast.Statement.CreateTable
			{
				Options: [{ Options: [{ Name.Text: "HISTORY_TABLE", Value: Ast.Expression.Reference { Name.Parts: [{ Style: Ast.IdentifierStyle.Bracketed }, { Style: Ast.IdentifierStyle.Bracketed }] } }] }],
			},
			"Insert bulk explicit null" => made is Ast.Statement.InsertBulk { Columns: [{ Options: [Ast.ColumnOption.Null], Constraints.Count: 0 }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		ReadBack.Holds(text);
	}

	/// <summary>
	/// Which record the bridge builds for the indexes, full-text indexes, views, statistics, types, sequences and
	/// synonyms, where another record writes the same text, and that each converts.
	/// </summary>
	[Theory]
	[InlineData("CREATE INDEX i ON t (a)",                                                                                                                                                "Create index")]
	[InlineData("CREATE CLUSTERED COLUMNSTORE INDEX i ON t ORDER (a, b)",                                                                                                                 "Columnstore order")]
	[InlineData("CREATE INDEX i ON t (a) ON fg FILESTREAM_ON fs",                                                                                                                         "Index placements")]
	[InlineData("CREATE INDEX i ON t (a) WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1, 3 TO 5))",                                                                                       "Option partitions")]
	[InlineData("CREATE TABLE t (a INT, INDEX i CLUSTERED COLUMNSTORE ORDER (a))",                                                                                                        "Inline index order")]
	[InlineData("CREATE PRIMARY XML INDEX i ON t (c)",                                                                                                                                    "Primary XML index")]
	[InlineData("CREATE XML INDEX i ON t (c) USING XML INDEX p FOR PATH WITH (MAXDOP = 1)",                                                                                               "Secondary XML index")]
	[InlineData("CREATE SPATIAL INDEX s ON t (c) USING GEOGRAPHY_AUTO_GRID WITH (DATA_COMPRESSION = PAGE) ON fg",                                                                         "Spatial index")]
	[InlineData("ALTER INDEX ALL ON t REBUILD PARTITION = ALL",                                                                                                                           "Alter all indexes")]
	[InlineData("ALTER INDEX i ON t REORGANIZE PARTITION = 2 WITH (LOB_COMPACTION = ON)",                                                                                                 "Reorganize")]
	[InlineData("ALTER INDEX i ON t SET (ALLOW_ROW_LOCKS = ON)",                                                                                                                          "Index set")]
	[InlineData("ALTER INDEX i ON t RESUME",                                                                                                                                              "Resume")]
	[InlineData("ALTER TABLE t ADD INDEX i (a)",                                                                                                                                          "Add one index")]
	[InlineData("ALTER TABLE t ADD CONSTRAINT c UNIQUE (a)",                                                                                                                              "Add one constraint")]
	[InlineData("ALTER TABLE t ALTER INDEX i REBUILD WITH (BUCKET_COUNT = 8)",                                                                                                            "Alter table alter index")]
	[InlineData("ALTER TABLE t DROP INDEX i",                                                                                                                                             "Alter table drop index")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a TYPE COLUMN b LANGUAGE 1033 STATISTICAL_SEMANTICS) KEY INDEX ix ON (cat, FILEGROUP fg) WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)",  "Full-text index")]
	[InlineData("CREATE FULLTEXT INDEX ON t (c) KEY INDEX pk ON cat WITH CHANGE_TRACKING = AUTO",                                                                                         "Full-text options unbracketed")]
	[InlineData("ALTER FULLTEXT INDEX ON t SET STOPLIST = s",                                                                                                                             "Stoplist named")]
	[InlineData("ALTER FULLTEXT INDEX ON t SET STOPLIST SYSTEM",                                                                                                                            "Stoplist system")]
	[InlineData("ALTER FULLTEXT INDEX ON t SET STOPLIST = OFF",                                                                                                                             "Stoplist off")]
	[InlineData("ALTER FULLTEXT INDEX ON t SET SEARCH PROPERTY LIST OFF",                                                                                                                   "Search property list off")]
	[InlineData("ALTER FULLTEXT INDEX ON t SET SEARCH PROPERTY LIST = p",                                                                                                                   "Search property list named")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a LANGUAGE NULL) KEY INDEX ix",                                                                                                                "Full-text language null")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a LANGUAGE English) KEY INDEX ix",                                                                                                             "Full-text language word")]
	[InlineData("ALTER FULLTEXT INDEX ON t SET CHANGE_TRACKING MANUAL",                                                                                                                   "Change tracking without =")]
	[InlineData("ALTER FULLTEXT INDEX ON t START FULL POPULATION",                                                                                                                        "Population")]
	[InlineData("ALTER FULLTEXT INDEX ON t ALTER COLUMN c ADD STATISTICAL_SEMANTICS",                                                                                                     "Full-text column altered")]
	[InlineData("CREATE OR ALTER VIEW v WITH SCHEMABINDING, VIEW_METADATA AS SELECT a FROM t WITH CHECK OPTION",                                                                          "View verb and attributes")]
	[InlineData("CREATE VIEW v AS SELECT a FROM t",                                                                                                                                       "Plain view")]
	[InlineData("CREATE STATISTICS s ON t (a) WHERE a > 5 WITH SAMPLE 12 ROWS, NORECOMPUTE",                                                                                              "Statistics")]
	[InlineData("UPDATE STATISTICS t ix WITH FULLSCAN",                                                                                                                                   "Update statistics unbracketed")]
	[InlineData("UPDATE STATISTICS t WITH RESAMPLE ON PARTITIONS (1, 3 TO 5)",                                                                                                            "Resample partitions")]
	[InlineData("CREATE TYPE t FROM varchar(10) NOT NULL",                                                                                                                                "Alias type")]
	[InlineData("CREATE TYPE t AS TABLE (a INT PRIMARY KEY) WITH (MEMORY_OPTIMIZED = ON)",                                                                                                "Table type")]
	[InlineData("CREATE SEQUENCE s CACHE 10 NO CYCLE",                                                                                                                                    "Sequence cache")]
	[InlineData("CREATE SEQUENCE s NO CACHE",                                                                                                                                             "Sequence no cache")]
	[InlineData("CREATE SYNONYM s FOR srv.db.dbo.t",                                                                                                                                      "Synonym")]
	public void The_bridge_builds_the_definition_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		static Ast.Option Word(string name, string value)
		{
			return new Ast.Option(new Ast.Identifier(name), new Ast.Expression.Reference(new Ast.QualifiedName([new Ast.Identifier(value)])), []);
		}

		var holds = expected switch
		{
			"Create index" => made is Ast.Statement.CreateIndex { Index: { Storage: null, Columns.Count: 1, Placements.Count: 0, Order.Count: 0 } },
			"Columnstore order" => made is Ast.Statement.CreateIndex { Index: { Storage: Ast.IndexStorage.Columnstore, Clustering: Ast.IndexClustering.Clustered, Columns.Count: 0, Order: [{ Text: "a" }, { Text: "b" }] } },
			"Index placements" => made is Ast.Statement.CreateIndex { Index.Placements: [{ Kind: Ast.PlacementKind.On }, { Kind: Ast.PlacementKind.FilestreamOn }] },
			"Option partitions" => made is Ast.Statement.CreateIndex { Index.Options: [{ Name.Text: "DATA_COMPRESSION", Partitions: [{ To: null }, { To: not null }] }] },
			"Inline index order" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements { Items: [_, Ast.TableElement.Index { Definition: { Storage: Ast.IndexStorage.Columnstore, Order: [{ Text: "a" }] } }] } },
			"Primary XML index" => made is Ast.Statement.CreateXmlIndex { Primary: true, Using: null, For: null, Column.Text: "c" },
			"Secondary XML index" => made is Ast.Statement.CreateXmlIndex { Primary: false, Using.Text: "p", For: Ast.XmlIndexFor.Path, Options.Count: 1 },
			"Spatial index" => made is Ast.Statement.CreateSpatialIndex { Using: Ast.SpatialTessellation.GeographyAutoGrid, Options.Count: 1, Placements: [{ Kind: Ast.PlacementKind.On }] },
			"Alter all indexes" => made is Ast.Statement.AlterIndex { All: true, Name: null, Action: Ast.IndexAlteration.Rebuild { AllPartitions: true, Partition: null } },
			"Reorganize" => made is Ast.Statement.AlterIndex { Name.Text: "i", Action: Ast.IndexAlteration.Reorganize { Partition: Ast.LiteralValue, Options.Count: 1 } },
			"Index set" => made is Ast.Statement.AlterIndex { Action: Ast.IndexAlteration.SetOptions { Options.Count: 1 } },
			"Resume" => made is Ast.Statement.AlterIndex { Action: Ast.IndexAlteration.Resume { Options.Count: 0 } },
			"Add one index" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.Add { Elements: [Ast.TableElement.Index] }] },
			"Add one constraint" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.AddConstraint] },
			"Alter table alter index" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.AlterIndex { Name.Text: "i", Action: Ast.IndexAlteration.Rebuild { Options.Count: 1 } }] },
			"Alter table drop index" => made is Ast.Statement.AlterTable { Actions: [Ast.AlterTableAction.DropIndex { Name.Text: "i" }] },
			"Full-text index" => made is Ast.Statement.CreateFullTextIndex
			{
				Columns: [{ TypeColumn.Text: "b", Language: Ast.LiteralValue, StatisticalSemantics: true }], KeyIndex.Text: "ix",
				Catalog.Text: "cat", FileGroup.Text: "fg", Options.Count: 2, OptionsUnbracketed: false,
			},
			"Full-text options unbracketed" => made is Ast.Statement.CreateFullTextIndex { Catalog.Text: "cat", FileGroup: null, OptionsUnbracketed: true } index
				&& index.Options.SequenceEqual([Word("CHANGE_TRACKING", "AUTO")]),
			"Stoplist named" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.SetStopList { Name.Text: "s", Off: false, System: false, EqualsOmitted: false } },
			"Stoplist system" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.SetStopList { Name: null, Off: false, System: true, EqualsOmitted: true } },
			"Stoplist off" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.SetStopList { Name: null, Off: true, System: false, EqualsOmitted: false } },
			"Search property list off" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.SetSearchPropertyList { Name: null, Off: true, EqualsOmitted: true } },
			"Search property list named" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.SetSearchPropertyList { Name.Text: "p", Off: false, EqualsOmitted: false } },
			"Full-text language null" => made is Ast.Statement.CreateFullTextIndex { Columns: [{ Language: Ast.LiteralValue.Null }] },
			"Full-text language word" => made is Ast.Statement.CreateFullTextIndex { Columns: [{ Language: Ast.Expression.Reference { Name.Parts: [{ Text: "English" }] } }] },
			"Change tracking without =" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.SetChangeTracking { Mode.Text: "MANUAL", EqualsOmitted: true } },
			"Population" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.Population { Action: Ast.PopulationAction.Start, Kind: Ast.PopulationKind.Full } },
			"Full-text column altered" => made is Ast.Statement.AlterFullTextIndex { Action: Ast.FullTextAlteration.AlterColumn { Add: true, Column.Column.Text: "c" } },
			"View verb and attributes" => made is Ast.Statement.CreateView
			{
				Verb: Ast.DefinitionVerb.CreateOrAlter, Attributes: [Ast.ViewAttribute.SchemaBinding, Ast.ViewAttribute.ViewMetadata], CheckOption: Ast.CheckOption.Unqualified,
			},
			"Plain view" => made is Ast.Statement.CreateView { Verb: null, Attributes.Count: 0, Specification: Ast.ViewSpecification.Regular { Columns.Count: 0 } },
			"Statistics" => made is Ast.Statement.CreateStatistics
			{
				Columns: [{ Text: "a" }], Where: not null,
				Options: [{ Name.Text: "SAMPLE", Value: Ast.Expression.Measured { Unit.Text: "ROWS" } }, { Name.Text: "NORECOMPUTE", Value: null }],
			},
			"Update statistics unbracketed" => made is Ast.Statement.UpdateStatistics { Names: [{ Text: "ix" }], NameUnbracketed: true, Options: [{ Name.Text: "FULLSCAN" }] },
			"Resample partitions" => made is Ast.Statement.UpdateStatistics { Names.Count: 0, Options: [{ Name.Text: "RESAMPLE", Value: null, Partitions.Count: 2 }] },
			"Alias type" => made is Ast.Statement.CreateAliasType { Base: Ast.DataType.Character, Nullability: Ast.ColumnNullability.NotNull },
			"Table type" => made is Ast.Statement.CreateTableType { Elements.Count: 1, Options: [{ Name.Text: "MEMORY_OPTIMIZED" }] },
			"Sequence cache" => made is Ast.Statement.CreateSequence { Options: [Ast.SequenceOption.Cache { Size: Ast.LiteralValue }, Ast.SequenceOption.Cycle { Value: false }] },
			"Sequence no cache" => made is Ast.Statement.CreateSequence { Options: [Ast.SequenceOption.NoCache] },
			"Synonym" => made is Ast.Statement.CreateSynonym { Target.Parts.Count: 4 },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		ReadBack.Holds(text);
	}

	/// <summary>
	/// A JSON function both parsers read is the standard's node, path literal included.
	/// </summary>
	[Theory]
	[InlineData("SELECT JSON_VALUE(a, '$.x') FROM t")]
	[InlineData("SELECT JSON_QUERY(a, '$.x' WITH ARRAY WRAPPER) FROM t")]
	[InlineData("SELECT JSON_OBJECT('a' : 1, 'b' : c) FROM t")]
	[InlineData("SELECT JSON_ARRAY(1, 2) FROM t")]
	[InlineData("SELECT JSON_OBJECT('a' : 1 ABSENT ON NULL RETURNING JSON) FROM t")]
	[InlineData("SELECT JSON_ARRAYAGG(a ORDER BY b) FROM t")]
	public void A_json_function_is_the_standards_node(string text)
	{
		Assert.Equal(SqlStandardParser.ParseStatement(text), TransactSqlParser.Located.ParseStatement(text));
	}

	/// <summary>
	/// Statements that must convert: the case of what the tree does not keep is folded, and a system type's name
	/// however written is the type.
	/// </summary>
	[Theory]
	[InlineData("SELECT 0XFF FROM t")]
	[InlineData("SELECT CAST (a AS [xml]) FROM t")]
	[InlineData("SELECT CAST (a AS [xml] (CONTENT s)) FROM t")]
	[InlineData("SELECT json_value(j, '$.x' returning int) FROM t")]
	[InlineData("SELECT json_object('a':1 absent on null returning json) FROM t")]
	[InlineData("SELECT Json_Query(j) FROM t")]
	[InlineData("SELECT json_array(1, 2 null on null) FROM t")]
	[InlineData("SELECT json_arrayagg(a order by b) FROM t")]
	[InlineData("SELECT json_objectagg('a':b) FROM t")]
	[InlineData("SELECT JSON_VALUE(a, '$' RETURNING returning_int) FROM t")]
	[InlineData("SELECT JSON_VALUE(a, '$' RETURNING [returning]) FROM t")]
	[InlineData("SELECT JSON_ARRAYAGG(c.name ORDER BY c.column_id) FROM t AS c")]
	[InlineData("SELECT JSON_ARRAYAGG(a ORDER BY b << 1) FROM t")]
	[InlineData("SELECT JSON_ARRAYAGG(a ORDER BY b || c) FROM t")]
	[InlineData("SELECT DATEADD(wk, 1, d)")]
	[InlineData("DECLARE @a int")]
	[InlineData("SET STATISTICS io ON")]
	[InlineData("RAISERROR ('x', 1, 1) with nowait")]
	[InlineData("EXEC p with recompile")]
	[InlineData("CREATE PROCEDURE p @a int AS SELECT 1")]
	[InlineData("CREATE FUNCTION f () RETURNS int AS BEGIN RETURN 1 END")]
	[InlineData("DROP SYMMETRIC KEY k remove provider key")]
	[InlineData("DROP ASSEMBLY a with no dependents")]
	[InlineData("DROP TRIGGER r on database")]
	[InlineData("DROP EVENT SESSION s on server")]
	[InlineData("DROP SIGNATURE FROM object::p BY CERTIFICATE c")]
	[InlineData("DROP SIGNATURE FROM p BY certificate c")]
	[InlineData("DROP INDEX i on t")]
	[InlineData("DROP INDEX i ON t with (ONLINE = ON)")]
	public void A_statement_converts(string text)
	{
		ReadBack.Holds(text);
	}

	/// <summary>
	/// <c>xml</c> qualified by <c>sys</c> is <c>DataType.Xml</c> written as that name, as <c>DECLARE @x sys.xml</c> reads it
	/// (the statement itself waits for DECLARE's record).
	/// </summary>
	[Fact]
	public void Xml_qualified_by_sys_is_xml()
	{
		Assert.Equal(
			new Ast.DataType.Xml() { WrittenName = new Ast.QualifiedName([new Ast.Identifier("sys"), new Ast.Identifier("xml")]) },
			Bridge.Values.Type(new object(), "sys.xml"));
	}

	/// <summary>
	/// Which record the bridge builds where the written text would be the same for another: what the round trip
	/// cannot see.
	/// </summary>
	[Theory]
	[InlineData("SELECT DATEADD (dd, 1, a)",                          "Expression.DatePart { Kind = Day, Spelling = Abbreviation }")]
	[InlineData("SELECT CONVERT (int, a, 112)",                       "Expression.Convert { Style = 112 }")]
	[InlineData("SELECT TRY_PARSE (a AS date USING 'en-US')",         "Expression.Parse { Try, Culture = 'en-US' }")]
	[InlineData("SELECT TRY_CAST (a AS int)",                         "Expression.Cast { Try }")]
	[InlineData("SELECT CAST (a AS int)",                             "DataType.Numeric { Int }")]
	[InlineData("SELECT CAST (a AS nvarchar(max))",                   "DataType.Character { Nvarchar, Max }")]
	[InlineData("SELECT CAST (a AS [int])",                           "DataType.Numeric { Int, [int] }")]
	[InlineData("SELECT CAST (a AS [nvarchar] (max))",                "DataType.Character { Nvarchar, Max, [nvarchar] }")]
	[InlineData("SELECT CAST (a AS sys.sysname)",                     "DataType.Named sys.sysname")]
	[InlineData("SELECT CAST (a AS [double precision])",              "DataType.UserDefined [double precision]")]
	[InlineData("SELECT CAST (a AS dbo.T)",                           "DataType.UserDefined dbo.T")]
	[InlineData("SELECT CAST (a AS xml(CONTENT s))",                  "DataType.Xml { Content, s }")]
	[InlineData("SELECT CAST (a AS [xml])",                           "DataType.Xml { [xml] }")]
	[InlineData("SELECT CAST (a AS [xml] (CONTENT s))",               "DataType.Xml { Content, s, [xml] }")]
	[InlineData("SELECT 0x0A",                                        "LiteralValue.Binary { 0A, ZeroX }")]
	[InlineData("SELECT NEXT VALUE FOR s OVER (ORDER BY a)",          "Expression.NextValue { Over }")]
	[InlineData("SELECT SUM (a) OVER (ORDER BY a ROWS 2 PRECEDING)",  "WindowFrame { Rows, 2 Preceding }")]
	[InlineData("SELECT STRING_AGG (a, ',') WITHIN GROUP (ORDER BY a)", "Expression.Invocation { WithinGroup }")]
	[InlineData("SELECT {fn LCASE (a)}",                              "Expression.OdbcEscape { fn, Invocation }")]
	[InlineData("SELECT JSON_OBJECT ('a': 1, 'b': NULL ABSENT ON NULL RETURNING JSON)", "Expression.JsonObject")]
	[InlineData("SELECT JSON_OBJECT ()",                              "Expression.JsonObject { }")]
	[InlineData("SELECT JSON_ARRAY (1, 2 NULL ON NULL)",              "Expression.JsonArray")]
	[InlineData("SELECT JSON_OBJECTAGG ('a': b) OVER (PARTITION BY c)", "Expression.JsonObjectAggregate { Over }")]
	[InlineData("SELECT JSON_ARRAYAGG (a ORDER BY b ABSENT ON NULL RETURNING JSON)", "Expression.JsonArrayAggregate")]
	[InlineData("SELECT JSON_VALUE (a, @p RETURNING INT)",            "Expression.JsonValue")]
	[InlineData("SELECT JSON_VALUE ('{}')",                           "Expression.Invocation JSON_VALUE")]
	[InlineData("SELECT JSON_QUERY (a)",                              "Expression.JsonQuery { no path }")]
	[InlineData("SELECT JSON_QUERY (a, '$.x' WITH ARRAY WRAPPER)",    "Expression.JsonQuery { WithArray }")]
	[InlineData("SELECT ISJSON (a)",                                  "Expression.Invocation ISJSON")]
	public void The_bridge_builds_the_record_the_text_means(string text, string expected)
	{
		var made  = (Ast.Statement.Select)TransactSqlParser.Located.ParseStatement(text);
		var value = ((Ast.SelectColumn.ExpressionItem)((Ast.Query.Select)made.Query).Columns[0]).Expression;

		static Ast.DataType Target(Ast.Expression value)
		{
			return ((Ast.Expression.Cast)value).Target;
		}

		var holds = expected switch
		{
			"Expression.DatePart { Kind = Day, Spelling = Abbreviation }" =>
				value is Ast.Expression.Invocation { Arguments: [{ Value: Ast.Expression.DatePart { Kind: Ast.DatePartKind.Day, Spelling: Ast.DatePartSpelling.Abbreviation } }, ..] },
			"Expression.Convert { Style = 112 }" =>
				value is Ast.Expression.Convert { Style: Ast.LiteralValue.Numeric { Text: "112" }, Try: false },
			"Expression.Parse { Try, Culture = 'en-US' }" =>
				value is Ast.Expression.Parse { Try: true, Culture: Ast.LiteralValue.String { Text: "'en-US'" } },
			"Expression.Cast { Try }" =>
				value is Ast.Expression.Cast { Try: true },
			"DataType.Numeric { Int }" =>
				Target(value) is Ast.DataType.Numeric { Kind: Ast.NumericTypeKind.Int },
			"DataType.Character { Nvarchar, Max }" =>
				Target(value) is Ast.DataType.Character { Kind: Ast.CharacterTypeKind.Nvarchar, Max: true },
			"DataType.Numeric { Int, [int] }" =>
				Target(value) is Ast.DataType.Numeric { Kind: Ast.NumericTypeKind.Int, WrittenName.Parts: [{ Text: "[int]" }] },
			"DataType.Character { Nvarchar, Max, [nvarchar] }" =>
				Target(value) is Ast.DataType.Character { Kind: Ast.CharacterTypeKind.Nvarchar, Max: true, WrittenName.Parts: [{ Text: "[nvarchar]" }] },
			"DataType.Named sys.sysname" =>
				Target(value) is Ast.DataType.Named { Name.Parts: [{ Text: "sys" }, { Text: "sysname" }], Arguments.Count: 0 },
			"DataType.UserDefined [double precision]" =>
				Target(value) is Ast.DataType.UserDefined { Name.Parts: [{ Text: "[double precision]" }] },
			"DataType.UserDefined dbo.T" =>
				Target(value) is Ast.DataType.UserDefined { Name.Parts: [{ Text: "dbo" }, { Text: "T" }] },
			"DataType.Xml { [xml] }" =>
				Target(value) is Ast.DataType.Xml { Modifier: null, Schema: null, WrittenName.Parts: [{ Text: "[xml]" }] },
			"DataType.Xml { Content, s, [xml] }" =>
				Target(value) is Ast.DataType.Xml { Modifier: Ast.XmlTypeModifier.Content, Schema.Parts: [{ Text: "s" }], WrittenName.Parts: [{ Text: "[xml]" }] },
			"DataType.Xml { Content, s }" =>
				Target(value) is Ast.DataType.Xml { Modifier: Ast.XmlTypeModifier.Content, Schema.Parts: [{ Text: "s" }] },
			"LiteralValue.Binary { 0A, ZeroX }" =>
				value is Ast.LiteralValue.Binary { Text: "0A", ZeroX: true },
			"Expression.NextValue { Over }" =>
				value is Ast.Expression.NextValue { Over.OrderBy.Items.Count: 1 },
			"WindowFrame { Rows, 2 Preceding }" =>
				value is Ast.Expression.Invocation { Over: Ast.WindowReference.Specification { Value.Frame: { Unit: Ast.WindowFrameUnit.Rows, Extent: Ast.WindowFrameExtent.Single { Bound: { Kind: Ast.WindowBoundKind.Preceding, Offset: not null } } } } },
			"Expression.Invocation { WithinGroup }" =>
				value is Ast.Expression.Invocation { WithinGroup.OrderBy.Items.Count: 1 },
			"Expression.JsonObject" =>
				value is Ast.Expression.JsonObject
				{
					Members: [{ Syntax: Ast.JsonMemberSyntax.Colon }, { Value: Ast.LiteralValue.Null }],
					Nulls: Ast.JsonNullHandling.AbsentOnNull, Output.Type: Ast.DataType.Json,
				},
			"Expression.JsonObject { }" =>
				value is Ast.Expression.JsonObject { Members.Count: 0, Nulls: null, Output: null },
			"Expression.JsonArray" =>
				value is Ast.Expression.JsonArray { Elements.Count: 2, Nulls: Ast.JsonNullHandling.NullOnNull, Output: null },
			"Expression.JsonObjectAggregate { Over }" =>
				value is Ast.Expression.JsonObjectAggregate { Pair.Syntax: Ast.JsonMemberSyntax.Colon, Over: Ast.WindowReference.Specification { Value.PartitionBy.Count: 1 } },
			"Expression.JsonArrayAggregate" =>
				value is Ast.Expression.JsonArrayAggregate { OrderBy.Items.Count: 1, Nulls: Ast.JsonNullHandling.AbsentOnNull, Output: not null, Over: null },
			"Expression.JsonValue" =>
				value is Ast.Expression.JsonValue { Common.Path: Ast.Expression.Variable, Returning: Ast.DataType.Numeric { Kind: Ast.NumericTypeKind.Int } },
			"Expression.Invocation JSON_VALUE" =>
				value is Ast.Expression.Invocation { Name.Parts: [{ Text: "JSON_VALUE" }], Arguments.Count: 1 },
			"Expression.JsonQuery { no path }" =>
				value is Ast.Expression.JsonQuery { Common.Path: null, Wrapper: null },
			"Expression.JsonQuery { WithArray }" =>
				value is Ast.Expression.JsonQuery { Common.Path: Ast.LiteralValue, Wrapper: Ast.JsonWrapperBehavior.WithArray },
			"Expression.Invocation ISJSON" =>
				value is Ast.Expression.Invocation { Name.Parts: [{ Text: "ISJSON" }] },
			_ =>
				value is Ast.Expression.OdbcEscape { Kind.Text: "fn", Value: Ast.Expression.Invocation { Name.Parts: [{ Text: "LCASE" }] } },
		};

		Assert.True(holds, $"{text} built {value}");
	}

	/// <summary>
	/// Which record the bridge builds for a query's T-SQL parts where another record would write the same text.
	/// </summary>
	[Theory]
	[InlineData("SELECT * FROM @t AS x",                                  "TableSource.Variable")]
	[InlineData("SELECT a FROM t GROUP BY ALL a",                         "GroupByClause.AllKeyword")]
	[InlineData("SELECT a INTO t FROM u",                                 "Query.Select.IntoTable")]
	[InlineData("SELECT * FROM OPENJSON (@j)",                            "TableSource.OpenJson")]
	[InlineData("SELECT @v = a FROM t",                                   "SelectColumn.VariableAssignment")]
	[InlineData("SELECT @v += a FROM t",                                  "SelectColumn.VariableAssignment +=")]
	[InlineData("SELECT 1 'a b', 2 AS N'c', 'd' = 3, 'h''i' = 7 FROM t",  "String aliases")]
	[InlineData("SELECT x = a, [@y] = b FROM t",                          "AliasFirst")]
	[InlineData("SELECT TOP (1) WITH APPROXIMATE a FROM t",               "TopWith.Approximate")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((*), 'x') AND FREETEXT (*, 'y')", "Asterisk")]
	[InlineData("SELECT @v ||= a FROM t",                                 "SelectColumn.VariableAssignment ||=")]
	[InlineData("SELECT a FROM t OPTION (OPTIMIZE FOR (@v = 0xFF, @w = 1e2, @x = 1.5))", "QueryHint.OptimizeFor literals")]
	[InlineData("SELECT * FROM t PIVOT (SUM (a) FOR [t].[b] IN ([1])) p",   "Pivot.Column qualified")]
	[InlineData("SELECT * FROM t PIVOT (SUM (a) FOR \"t\".b IN ([1])) p",  "Pivot.Column qualified")]
	[InlineData("SELECT * FROM t UNPIVOT (v FOR c IN ([t].a, b)) u",        "Unpivot.Columns qualified")]
	[InlineData("SELECT * FROM OPENXML (@h, '/r') WITH t2 x",             "TableSource.OpenXml.SchemaTable")]
	[InlineData("SELECT * FROM OPENDATASOURCE ('p', 'i').db.dbo.t",       "TableSource.Named.DataSource")]
	[InlineData("SELECT * FROM t PIVOT (SUM (a) FOR b IN ([1], [2])) p", "TableSource.Pivot")]
	[InlineData("SELECT * FROM t UNPIVOT (v FOR c IN (a, b)) u",          "TableSource.Unpivot")]
	[InlineData("SELECT * FROM t TABLESAMPLE (10)",                       "SampleClause.MethodOmitted")]
	[InlineData("SELECT * FROM OPENROWSET ('MSDASQL', 'srv'; 'u'; 'p', 'SELECT 1') r", "TableSource.OpenRowset.Login")]
	[InlineData("SELECT * FROM OPENROWSET ('SQLNCLI', 'Server=s;', 'SELECT 1') r", "TableSource.OpenRowset.ProviderString")]
	[InlineData("SELECT a FROM t FOR XML PATH ('')",                     "ForClause.Xml.ElementName")]
	[InlineData("SELECT a FROM t FOR XML AUTO ('r'), ELEMENTS ABSENT, ROOT ('x')", "ForDirective")]
	[InlineData("SELECT 1 FROM a, b FOR PATH AS e, c FOR PATH AS f WHERE MATCH(SHORTEST_PATH((f<-(e)-){1,3}a) AND LAST_NODE(f) = LAST_NODE(a))", "GraphPattern.ShortestPath")]
	[InlineData("SELECT 1 FROM a, b FOR PATH AS e WHERE MATCH(SHORTEST_PATH(a(-(e)->a)+) AND LAST_NODE(a)-(e)->a)", "GraphPattern.Chain")]
	[InlineData("SELECT 1 FROM (SELECT 1 AS x) FOR PATH AS e",              "TableSource.Subquery.ForPath")]
	[InlineData("SELECT * FROM CHANGETABLE (CHANGES dbo.t, @v, FORCESEEK) AS c (k)", "TableSource.ChangeTableChanges")]
	[InlineData("SELECT * FROM CHANGETABLE (VERSION t, (a, b), (1, 2)) AS c", "TableSource.ChangeTableVersion")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, PROPERTY (c, 'Title'), 'x', LANGUAGE 1033, 10) AS k", "TableSource.ContainsTable")]
	[InlineData("SELECT * FROM FREETEXTTABLE (t, (*), @s, 5) AS k",       "TableSource.FreetextTable")]
	public void The_bridge_builds_the_query_record_the_text_means(string text, string expected)
	{
		var made   = (Ast.Statement.Select)TransactSqlParser.Located.ParseStatement(text);
		var select = (Ast.Query.Select)made.Query;
		var source = select.From?.Sources[0];

		var holds = expected switch
		{
			"TableSource.Variable"                  => source is Ast.TableSource.Variable { Name.Text: "@t", Alias.Name.Text: "x" },
			"GroupByClause.AllKeyword"              => select.GroupBy is { AllKeyword: true, Quantifier: null },
			"Query.Select.IntoTable"                => select is { Into: null, IntoTable.Table.Parts: [{ Text: "t" }] },
			"TableSource.OpenJson"                  => source is Ast.TableSource.OpenJson { Schema: null, Path: null },
			"SelectColumn.VariableAssignment"       => select.Columns is [Ast.SelectColumn.VariableAssignment { Variable.Text: "@v", Operator: Ast.AssignmentOperator.Assign }],
			"String aliases"                        => select.Columns is
			[
				Ast.SelectColumn.ExpressionItem { Alias: { Text: "a b", Style: Ast.IdentifierStyle.String }, AsKeyword: false, AliasFirst: false },
				Ast.SelectColumn.ExpressionItem { Alias: { Text: "c", Style: Ast.IdentifierStyle.NationalString }, AsKeyword: true },
				Ast.SelectColumn.ExpressionItem { Alias: { Text: "d", Style: Ast.IdentifierStyle.String }, AliasFirst: true },
				Ast.SelectColumn.ExpressionItem { Alias: { Text: "h'i", Style: Ast.IdentifierStyle.String }, AliasFirst: true },
			],
			"AliasFirst"                            => select.Columns is
			[
				Ast.SelectColumn.ExpressionItem { Alias.Text: "x", AliasFirst: true, AsKeyword: false },
				Ast.SelectColumn.ExpressionItem { Alias: { Text: "[@y]", Style: Ast.IdentifierStyle.Bracketed }, AliasFirst: true },
			],
			"TopWith.Approximate"                   => select.Top is { With: Ast.TopWith.Approximate, Parentheses: true },
			"Asterisk"                              => select.Where is Ast.Expression.Binary && made.ToString().Split("Asterisk").Length == 3,
			"SelectColumn.VariableAssignment +="    => select.Columns is [Ast.SelectColumn.VariableAssignment { Operator: Ast.AssignmentOperator.Add }],
			"SelectColumn.VariableAssignment ||="   => select.Columns is [Ast.SelectColumn.VariableAssignment { Operator: Ast.AssignmentOperator.Concatenate }],
			"QueryHint.OptimizeFor literals"        => made.Hints is [Ast.QueryHint.OptimizeFor
			{
				Variables:
				[
					{ Value: Ast.LiteralValue.Binary { ZeroX: true } },
					{ Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Approximate } },
					{ Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Decimal } },
				],
			}],
			"Pivot.Column qualified"                => source is Ast.TableSource.Pivot { Column.Parts.Count: 2, Values: [{ Parts.Count: 1 }] },
			"Unpivot.Columns qualified"             => source is Ast.TableSource.Unpivot { Columns: [{ Parts.Count: 2 }, { Parts.Count: 1 }] },
			"TableSource.OpenXml.SchemaTable"       => source is Ast.TableSource.OpenXml { Schema: null, SchemaTable.Parts: [{ Text: "t2" }] },
			"TableSource.Named.DataSource"          => source is Ast.TableSource.Named { DataSource: not null, Name.Parts.Count: 3 },
			"TableSource.Pivot"                     => source is Ast.TableSource.Pivot { Column.Parts: [{ Text: "b" }], Values: [{ Parts: [{ Text: "[1]" }] }, { Parts: [{ Text: "[2]" }] }], Alias.Name.Text: "p" },
			"TableSource.Unpivot"                   => source is Ast.TableSource.Unpivot { Value.Parts: [{ Text: "v" }], Column.Parts: [{ Text: "c" }], Columns: [{ Parts: [{ Text: "a" }] }, { Parts: [{ Text: "b" }] }], Alias.Name.Text: "u" },
			"GraphPattern.ShortestPath"             => select.Where is Ast.Expression.GraphMatch
			{
				Patterns:
				[
					Ast.GraphPattern.ShortestPath { AnchorLast: true, Anchor: { Alias.Text: "a", LastNode: false }, Quantifier: { Min: 1, Max: 3, Plus: false }, Steps: [{ Edge: { Alias.Text: "e", Direction: Ast.GraphDirection.Left }, Node.Alias.Text: "f" }] },
					Ast.GraphPattern.LastNodes { Left.Text: "f", Right.Text: "a" },
				],
			} && select.From!.Sources[1] is Ast.TableSource.Named { ForPath: true },
			"GraphPattern.Chain"                    => select.Where is Ast.Expression.GraphMatch
			{
				Patterns:
				[
					Ast.GraphPattern.ShortestPath { AnchorLast: false, Quantifier.Plus: true, Steps: [{ Edge.Direction: Ast.GraphDirection.Right }] },
					Ast.GraphPattern.Chain { Start: { Alias.Text: "a", LastNode: true }, Steps: [{ Edge.Direction: Ast.GraphDirection.Right, Node.LastNode: false }] },
				],
			},
			"TableSource.ChangeTableChanges"        => source is Ast.TableSource.ChangeTableChanges { ForceSeek: true, Table.Parts.Count: 2, LastSyncVersion: Ast.Expression.Variable, Alias.Columns.Count: 1 },
			"TableSource.ChangeTableVersion"        => source is Ast.TableSource.ChangeTableVersion { ForceSeek: false, Columns: [{ Text: "a" }, { Text: "b" }], Values.Count: 2 },
			"TableSource.ContainsTable"             => source is Ast.TableSource.ContainsTable { ColumnsBracketed: false, Columns: [Ast.Expression.Invocation], Language: not null, Top: not null },
			"TableSource.FreetextTable"             => source is Ast.TableSource.FreetextTable { ColumnsBracketed: true, Columns: [Ast.Expression.Asterisk], Language: null, Top: not null },
			"TableSource.Subquery.ForPath"          => source is Ast.TableSource.Subquery { ForPath: true, Alias.Name.Text: "e" },
			"SampleClause.MethodOmitted"            => source is Ast.TableSource.Named { Sample: { MethodOmitted: true, Unit: null } },
			"TableSource.OpenRowset.ProviderString" => source is Ast.TableSource.OpenRowset { ProviderString: not null, Login: null },
			"ForClause.Xml.ElementName"             => select.For is Ast.ForClause.Xml { Mode: Ast.XmlMode.Path, ElementName: Ast.LiteralValue, Directives: [] },
			"ForDirective"                          => select.For is Ast.ForClause.Xml
			{
				Mode: Ast.XmlMode.Auto, ElementName: not null,
				Directives: [{ Kind: Ast.ForDirectiveKind.Elements, Absent: true, Name: null }, { Kind: Ast.ForDirectiveKind.Root, Name: not null }],
			},
			_                                       => source is Ast.TableSource.OpenRowset { ProviderString: null, Login: not null }
				&& Ast.TransactSqlWriter.Write(made).Contains("'srv'; 'u'; 'p'", StringComparison.Ordinal),
		};

		Assert.True(holds, $"{text} built {made}");
	}

	/// <summary>
	/// The word AS of an alias is a word: a name <c>"as"</c> before the alias is not it.
	/// </summary>
	[Theory]
	[InlineData("SELECT a FROM \"as\" x",    false)]
	[InlineData("SELECT a FROM [as] x",      false)]
	[InlineData("SELECT a FROM \"as\" AS x", true)]
	public void A_quoted_name_is_not_the_word_AS(string text, bool said)
	{
		var made = (Ast.Statement.Select)TransactSqlParser.Located.ParseStatement(text);
		var from = ((Ast.Query.Select)made.Query).From!.Sources[0];

		Assert.Equal(said, ((Ast.TableSource.Named)from).Alias!.AsKeyword);
	}

	/// <summary>
	/// A schema's owner is one name; what follows it is a schema element the old tree keeps as text, and the bridge reads it
	/// again as a statement.
	/// </summary>
	[Theory]
	[InlineData("CREATE SCHEMA s AUTHORIZATION [dbo]", "[dbo]", 0)]
	[InlineData("CREATE SCHEMA s AUTHORIZATION [dbo] CREATE TABLE t (c int)", "[dbo]", 1)]
	[InlineData("CREATE SCHEMA s AUTHORIZATION dbo CREATE TABLE t (c int)", "dbo", 1)]
	public void A_schema_owner_is_one_name(string text, string owner, int elements)
	{
		var made = (Ast.Statement.CreateSchema)TransactSqlParser.Located.ParseStatement(text);

		Assert.Equal(owner, made.Authorization!.Name.Text);
		Assert.Equal(elements, made.Elements.Count);
	}
}
