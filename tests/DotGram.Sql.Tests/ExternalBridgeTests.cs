using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The External family's bridge (Bridge.External.cs): which record and members it builds for a text, where another record
/// or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class ExternalBridgeTests
{
	[Theory]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (LOCATION = 'l')",                  "Create")]
	[InlineData("ALTER EXTERNAL DATA SOURCE d SET LOCATION = 'l'",                      "Alter")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH ()",                                "Create empty")]
	[InlineData("create external data source d with (type = hadoop, pushdown = off)",   "Lower case")]
	[InlineData("CREATE EXTERNAL DATA SOURCE [d e] WITH (CREDENTIAL = [c d])",         "Names in brackets")]
	[InlineData("ALTER EXTERNAL DATA SOURCE d SET PUSHDOWN = ON, CREDENTIAL = c",       "Alter two")]
	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_TYPE = DELIMITEDTEXT, FORMAT_OPTIONS (FIRST_ROW = 2))", "File format")]
	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_OPTIONS ())",               "File format empty options")]
	public void The_bridge_builds_the_definition_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Create" => made is Ast.Statement.CreateExternalDataSource { Name.Text: "d", Options: [{ Name.Text: "LOCATION", Value: Ast.LiteralValue.String, Words.Count: 0, EqualsOmitted: false }] },
			"Alter" => made is Ast.Statement.AlterExternalDataSource { Name.Text: "d", Options: [{ Name.Text: "LOCATION", Value: Ast.LiteralValue.String }] },
			"Create empty" => made is Ast.Statement.CreateExternalDataSource { Options.Count: 0 },
			"Lower case" => made is Ast.Statement.CreateExternalDataSource { Options: [{ Name.Text: "type", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "hadoop" }] } }, { Name.Text: "pushdown" }] },
			"Names in brackets" => made is Ast.Statement.CreateExternalDataSource { Name: { Text: "[d e]", Style: Ast.IdentifierStyle.Bracketed }, Options: [{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "[c d]", Style: Ast.IdentifierStyle.Bracketed }] } }] },
			"Alter two" => made is Ast.Statement.AlterExternalDataSource { Options: [{ Name.Text: "PUSHDOWN" }, { Name.Text: "CREDENTIAL" }] },
			"File format" => made is Ast.Statement.CreateExternalFileFormat { Options: [{ Name.Text: "FORMAT_TYPE", Value: not null }, { Name.Text: "FORMAT_OPTIONS", Value: null, Options: [{ Name.Text: "FIRST_ROW", Value: Ast.LiteralValue.Numeric { Text: "2" } }], EqualsOmitted: false }] },
			"File format empty options" => made is Ast.Statement.CreateExternalFileFormat { Options: [{ Name.Text: "FORMAT_OPTIONS", Value: null, Options.Count: 0 }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	/// <summary>
	/// A text the grammar reads and the records do not hold is a gap that says so, never another record: the other verb's
	/// body, and the commas the PolyBase lists take loose, which the tree keeps nothing for.
	/// </summary>
	[Theory]
	[InlineData("CREATE EXTERNAL DATA SOURCE d SET LOCATION = 'l'")]
	[InlineData("ALTER EXTERNAL DATA SOURCE d WITH (LOCATION = 'l')")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (LOCATION = 'l',)")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (,LOCATION = 'l')")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (LOCATION = 'l',,)")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (,)")]
	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_TYPE = ORC,)")]
	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_TYPE = DELIMITEDTEXT, FORMAT_OPTIONS (FIRST_ROW = 2,))")]
	public void A_text_the_records_do_not_hold_is_a_gap(string text)
	{
		var old = TransactSqlParser.Located.ParseStatement(text);

		Assert.Throws<BridgeGap>(() => Bridge.ToAst(old, text));
	}

	[Theory]
	[InlineData("CREATE EXTERNAL TABLE t (a INT) WITH (LOCATION = 'l', DATA_SOURCE = d)",             "External table")]
	[InlineData("CREATE TABLE t (a INT) WITH (DATA_COMPRESSION = PAGE)",                              "Plain table")]
	[InlineData("CREATE EXTERNAL TABLE t WITH (LOCATION = 'l', DATA_SOURCE = d) AS SELECT 1 AS a",   "External query")]
	[InlineData("CREATE TABLE t WITH (DISTRIBUTION = ROUND_ROBIN) AS SELECT 1 AS a",                  "Plain query")]
	[InlineData("CREATE EXTERNAL TABLE t (a INT) WITH (REJECT_TYPE = VALUE)",                         "Reject type value")]
	[InlineData("CREATE EXTERNAL TABLE t (a INT) WITH (REJECT_TYPE = percentage)",                    "Reject type percentage")]
	[InlineData("CREATE EXTERNAL TABLE t (a INT) WITH (DATA_SOURCE = value)",                         "Data source called value")]
	[InlineData("CREATE EXTERNAL TABLE t (a INT) WITH (DATA_SOURCE = [value])",                       "Data source called [value]")]
	[InlineData("CREATE EXTERNAL TABLE t (a INT) WITH (DISTRIBUTION = SHARDED (c))",                  "Sharded")]
	[InlineData("CREATE EXTERNAL TABLE t (a INT,) WITH (REJECT_VALUE = 1.5,)",                        "Commas loose")]
	[InlineData("CREATE EXTERNAL TABLE t (a INT) WITH ()",                                            "No setting")]
	public void The_bridge_builds_the_table_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"External table" => made is Ast.Statement.CreateExternalTable { Contents: Ast.TableContents.Elements { Items.Count: 1 }, Options: [{ Name.Text: "LOCATION" }, { Name.Text: "DATA_SOURCE" }] },
			"Plain table" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements, Options.Count: 1 },
			"External query" => made is Ast.Statement.CreateExternalTable { Contents: Ast.TableContents.AsQuery { Columns.Count: 0, Data: null }, Options.Count: 2 },
			"Plain query" => made is Ast.Statement.CreateTable { Contents: Ast.TableContents.AsQuery { Data: null }, Options.Count: 1 },
			"Reject type value" => made is Ast.Statement.CreateExternalTable { Options: [{ Name.Text: "REJECT_TYPE", Value: Ast.Expression.Current { Kind: Ast.CurrentValue.Value } }] },
			"Reject type percentage" => made is Ast.Statement.CreateExternalTable { Options: [{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "percentage" }] } }] },
			"Data source called value" => made is Ast.Statement.CreateExternalTable { Options: [{ Name.Text: "DATA_SOURCE", Value: Ast.Expression.Current { Kind: Ast.CurrentValue.Value } }] },
			"Data source called [value]" => made is Ast.Statement.CreateExternalTable { Options: [{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "[value]", Style: Ast.IdentifierStyle.Bracketed }] } }] },
			"Sharded" => made is Ast.Statement.CreateExternalTable { Options: [{ Name.Text: "DISTRIBUTION", Value: Ast.Expression.Invocation { Name.Parts: [{ Text: "SHARDED" }], Arguments: [{ Value: Ast.Expression.Reference }] } }] },
			"Commas loose" => made is Ast.Statement.CreateExternalTable { Contents: Ast.TableContents.Elements { Items.Count: 1 }, Options: [{ Name.Text: "REJECT_VALUE", Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Decimal } }] },
			"No setting" => made is Ast.Statement.CreateExternalTable { Options.Count: 0 },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	/// <summary>
	/// The query of an external table that the common tree cannot hold, as the plain table's query cannot, is a gap.
	/// </summary>
	[Theory]
	[InlineData("CREATE EXTERNAL TABLE t WITH (LOCATION = 'l') AS SELECT 1 AS a OPTION (RECOMPILE)")]
	public void A_query_the_records_do_not_hold_is_a_gap(string text)
	{
		var old = TransactSqlParser.Located.ParseStatement(text);

		Assert.Throws<BridgeGap>(() => Bridge.ToAst(old, text));
	}

	[Theory]
	[InlineData("CREATE EXTERNAL LIBRARY l FROM (CONTENT = 'p') WITH (LANGUAGE = 'R')",                     "Library")]
	[InlineData("CREATE EXTERNAL LIBRARY l AUTHORIZATION dbo FROM (CONTENT = 'p') WITH (LANGUAGE = 'R')",   "Library owner")]
	[InlineData("ALTER EXTERNAL LIBRARY l SET (CONTENT = 'p') WITH (LANGUAGE = 'R')",                       "Library set")]
	[InlineData("ALTER EXTERNAL LIBRARY l ADD (CONTENT = 'p') WITH (LANGUAGE = 'R')",                       "Library add")]
	[InlineData("ALTER EXTERNAL LIBRARY l AUTHORIZATION dbo SET (PLATFORM = lInUx) WITH (LANGUAGE = 'R')",  "Library altered owner")]
	[InlineData("CREATE EXTERNAL LIBRARY l FROM (CONTENT = NONE) WITH (LANGUAGE = 'R')",                    "Content NONE")]
	[InlineData("CREATE EXTERNAL LIBRARY l FROM (CONTENT = [NONE]) WITH (LANGUAGE = 'R')",                  "Content [NONE]")]
	[InlineData("CREATE EXTERNAL LIBRARY l FROM (CONTENT = 1) WITH (LANGUAGE = 'R')",                       "Content number")]
	[InlineData("CREATE EXTERNAL LIBRARY l FROM (CONTENT = p) WITH (LANGUAGE = N'R')",                      "Content name, national language")]
	[InlineData("CREATE EXTERNAL LIBRARY l FROM (PLATFORM = WINDOWS, CONTENT = 'p'), (CONTENT = 0x01) WITH (LANGUAGE = 'R')", "Two files")]
	[InlineData("create external library l from (content = 'p') with (language = 'R')",                       "Library lower case")]
	[InlineData("CREATE EXTERNAL LIBRARY l FROM (CONTENT /* c /* nested */ */ = 'p') /* d */ WITH (LANGUAGE = 'R') -- e",       "Library comments")]
	public void The_bridge_builds_the_library_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Library" => made is Ast.Statement.CreateExternalLibrary { Authorization: null, Files: [{ Options: [{ Name.Text: "CONTENT" }] }], Language: { Kind: Ast.StringLiteralKind.Character } },
			"Library owner" => made is Ast.Statement.CreateExternalLibrary { Authorization.Text: "dbo" },
			"Library set" => made is Ast.Statement.AlterExternalLibrary { Authorization: null, Change: Ast.ExternalLibraryChange.Set { Files.Count: 1 } },
			"Library add" => made is Ast.Statement.AlterExternalLibrary { Change: Ast.ExternalLibraryChange.Add { Files.Count: 1 } },
			"Library altered owner" => made is Ast.Statement.AlterExternalLibrary { Authorization.Text: "dbo", Change: Ast.ExternalLibraryChange.Set { Files: [{ Options: [{ Name.Text: "PLATFORM", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "lInUx" }] } }] }] } },
			"Content NONE" => made is Ast.Statement.CreateExternalLibrary { Files: [{ Options: [{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "NONE", Style: Ast.IdentifierStyle.Regular }] } }] }] },
			"Content [NONE]" => made is Ast.Statement.CreateExternalLibrary { Files: [{ Options: [{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "[NONE]", Style: Ast.IdentifierStyle.Bracketed }] } }] }] },
			"Content number" => made is Ast.Statement.CreateExternalLibrary { Files: [{ Options: [{ Value: Ast.LiteralValue.Numeric { Text: "1", Kind: Ast.NumericLiteralKind.DecimalInteger } }] }] },
			"Content name, national language" => made is Ast.Statement.CreateExternalLibrary { Files: [{ Options: [{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "p" }] } }] }], Language: { Kind: Ast.StringLiteralKind.National } },
			"Library lower case" => made is Ast.Statement.CreateExternalLibrary { Files: [{ Options: [{ Name.Text: "content" }] }], Language.Text: "'R'" },
			"Library comments" => made is Ast.Statement.CreateExternalLibrary { Files: [{ Options: [{ Name.Text: "CONTENT", Value: Ast.LiteralValue.String }] }] },
			"Two files" => made is Ast.Statement.CreateExternalLibrary { Files: [{ Options: [{ Name.Text: "PLATFORM" }, { Name.Text: "CONTENT" }] }, { Options: [{ Value: Ast.LiteralValue.Binary { ZeroX: true } }] }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	[Theory]
	[InlineData("CREATE EXTERNAL LANGUAGE l FROM (CONTENT = 0x01, FILE_NAME = 'f')",                                     "Language")]
	[InlineData("CREATE EXTERNAL LANGUAGE l AUTHORIZATION dbo FROM (CONTENT = 'a', FILE_NAME = 'f'), (CONTENT = 'b', FILE_NAME = 'g', PLATFORM = LINUX)", "Language two files")]
	[InlineData("ALTER EXTERNAL LANGUAGE l SET (CONTENT = 'a')",                                                          "Language set")]
	[InlineData("ALTER EXTERNAL LANGUAGE l ADD (CONTENT = 'a')",                                                          "Language add")]
	[InlineData("ALTER EXTERNAL LANGUAGE l AUTHORIZATION dbo REMOVE PLATFORM WINDOWS",                                    "Language remove Windows")]
	[InlineData("alter external language l remove platform linux",                                                        "Language remove Linux")]
	[InlineData("CREATE EXTERNAL LANGUAGE l FROM (CONTENT = value, FILE_NAME = 'f')",                                      "Content VALUE")]
	[InlineData("CREATE EXTERNAL LANGUAGE l FROM (CONTENT = [value], FILE_NAME = 'f')",                                    "Content [value]")]
	[InlineData("CREATE EXTERNAL LANGUAGE l FROM (CONTENT = @c, FILE_NAME = 'f')",                                         "Content variable")]
	[InlineData("CREATE EXTERNAL LANGUAGE l FROM (CONTENT = NULL, FILE_NAME = 'f')",                                       "Content NULL")]
	[InlineData("CREATE EXTERNAL LANGUAGE l FROM (CONTENT = f(1, 2), FILE_NAME = N'f', PARAMETERS = 'p')",                 "Content call")]
	public void The_bridge_builds_the_language_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Language" => made is Ast.Statement.CreateExternalLanguage { Authorization: null, Files: [{ Options: [{ Value: Ast.LiteralValue.Binary }, { Name.Text: "FILE_NAME" }] }] },
			"Language two files" => made is Ast.Statement.CreateExternalLanguage { Authorization.Text: "dbo", Files.Count: 2 },
			"Language set" => made is Ast.Statement.AlterExternalLanguage { Change: Ast.ExternalLanguageChange.Set },
			"Language add" => made is Ast.Statement.AlterExternalLanguage { Change: Ast.ExternalLanguageChange.Add },
			"Language remove Windows" => made is Ast.Statement.AlterExternalLanguage { Authorization.Text: "dbo", Change: Ast.ExternalLanguageChange.RemovePlatform { Platform: Ast.ExternalPlatform.Windows } },
			"Language remove Linux" => made is Ast.Statement.AlterExternalLanguage { Change: Ast.ExternalLanguageChange.RemovePlatform { Platform: Ast.ExternalPlatform.Linux } },
			"Content VALUE" => made is Ast.Statement.CreateExternalLanguage { Files: [{ Options: [{ Value: Ast.Expression.Current { Kind: Ast.CurrentValue.Value } }, ..] }] },
			"Content [value]" => made is Ast.Statement.CreateExternalLanguage { Files: [{ Options: [{ Value: Ast.Expression.Reference { Name.Parts: [{ Style: Ast.IdentifierStyle.Bracketed }] } }, ..] }] },
			"Content variable" => made is Ast.Statement.CreateExternalLanguage { Files: [{ Options: [{ Value: Ast.Expression.Variable { Name.Text: "@c" } }, ..] }] },
			"Content NULL" => made is Ast.Statement.CreateExternalLanguage { Files: [{ Options: [{ Value: Ast.LiteralValue.Null }, ..] }] },
			"Content call" => made is Ast.Statement.CreateExternalLanguage { Files: [{ Options: [{ Value: Ast.Expression.Invocation { Arguments.Count: 2 } }, { Value: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } }, { Name.Text: "PARAMETERS" }] }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	[Theory]
	[InlineData("CREATE EXTERNAL MODEL m WITH (MODEL = 'a')",                                  "Model")]
	[InlineData("CREATE EXTERNAL MODEL m AUTHORIZATION dbo WITH ()",                           "Model owner, empty")]
	[InlineData("ALTER EXTERNAL MODEL m SET (MODEL = 'a')",                                    "Model altered")]
	[InlineData("ALTER EXTERNAL MODEL m SET ()",                                               "Model altered, empty")]
	[InlineData("create external model m with (model_type = embeddings, credential = [c])",    "Model lower case")]
	[InlineData("CREATE EXTERNAL RESOURCE POOL p",                                             "Pool alone")]
	[InlineData("CREATE EXTERNAL RESOURCE POOL p WITH (MAX_CPU_PERCENT = 20)",                 "Pool")]
	[InlineData("ALTER EXTERNAL RESOURCE POOL p WITH (MAX_CPU_PERCENT = 20)",                  "Pool altered")]
	[InlineData("CREATE EXTERNAL RESOURCE POOL p WITH (AFFINITY CPU = AUTO)",                  "Affinity AUTO")]
	[InlineData("CREATE EXTERNAL RESOURCE POOL p WITH (AFFINITY CPU = (0 TO 3, 5))",           "Affinity CPU ranges")]
	[InlineData("CREATE EXTERNAL RESOURCE POOL p WITH (AFFINITY NUMANODE = (1))",              "Affinity NUMA node")]
	[InlineData("create external resource pool [p] with (affinity cpu = (@a to @b))",          "Affinity variables")]
	public void The_bridge_builds_the_model_and_pool_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Model" => made is Ast.Statement.CreateExternalModel { Authorization: null, Options: [{ Name.Text: "MODEL", Value: Ast.LiteralValue.String }] },
			"Model owner, empty" => made is Ast.Statement.CreateExternalModel { Authorization.Text: "dbo", Options.Count: 0 },
			"Model altered" => made is Ast.Statement.AlterExternalModel { Options: [{ Name.Text: "MODEL" }] },
			"Model altered, empty" => made is Ast.Statement.AlterExternalModel { Options.Count: 0 },
			"Model lower case" => made is Ast.Statement.CreateExternalModel { Options: [{ Name.Text: "model_type", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "embeddings" }] } }, { Value: Ast.Expression.Reference { Name.Parts: [{ Text: "[c]", Style: Ast.IdentifierStyle.Bracketed }] } }] },
			"Pool alone" => made is Ast.Statement.CreateExternalResourcePool { Options.Count: 0 },
			"Pool" => made is Ast.Statement.CreateExternalResourcePool { Options: [{ Name.Text: "MAX_CPU_PERCENT", Value: Ast.LiteralValue.Numeric { Text: "20" } }] },
			"Pool altered" => made is Ast.Statement.AlterExternalResourcePool { Options: [{ Name.Text: "MAX_CPU_PERCENT" }] },
			"Affinity AUTO" => made is Ast.Statement.CreateExternalResourcePool { Options: [{ Name.Text: "AFFINITY", Words: [{ Text: "CPU" }], Value: Ast.Expression.Reference { Name.Parts: [{ Text: "AUTO" }] } }] },
			"Affinity CPU ranges" => made is Ast.Statement.CreateExternalResourcePool { Options: [{ Words: [{ Text: "CPU" }], Value: Ast.Expression.PoolRanges { Ranges: [{ From: Ast.LiteralValue.Numeric { Text: "0" }, To: Ast.LiteralValue.Numeric { Text: "3" } }, { From: Ast.LiteralValue.Numeric { Text: "5" }, To: null }] } }] },
			"Affinity NUMA node" => made is Ast.Statement.CreateExternalResourcePool { Options: [{ Words: [{ Text: "NUMANODE" }], Value: Ast.Expression.PoolRanges { Ranges: [{ From: Ast.LiteralValue.Numeric { Text: "1" }, To: null }] } }] },
			"Affinity variables" => made is Ast.Statement.CreateExternalResourcePool { Name.Style: Ast.IdentifierStyle.Bracketed, Options: [{ Name.Text: "affinity", Value: Ast.Expression.PoolRanges { Ranges: [{ From: Ast.Expression.Variable, To: Ast.Expression.Variable }] } }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	/// <summary>A pool the grammar lets be named by a string has no place in the tree yet.</summary>
	[Theory]
	[InlineData("CREATE EXTERNAL RESOURCE POOL 'default'")]
	[InlineData("ALTER EXTERNAL RESOURCE POOL 'default' WITH (MAX_CPU_PERCENT = 1)")]
	public void A_pool_named_by_a_string_is_a_gap(string text)
	{
		var old = TransactSqlParser.Located.ParseStatement(text);

		Assert.Throws<BridgeGap>(() => Bridge.ToAst(old, text));
	}
}
