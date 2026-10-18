using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The remainder of the statements and values the bridge carries outside the DDL: transactions, typed rowset sources,
/// T-SQL's functions of their own. Which record and members it builds for a text, where another record or a flag set
/// the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class ValuesRemainderBridgeTests
{
	[Theory]
	[InlineData("SAVE TRANSACTION -5:a.b", "a numbered name")]
	[InlineData("SAVE TRAN - 100 : [a ] . [b ]", "a numbered name with blanks")]
	[InlineData("SAVE TRANSACTION t1", "a name")]
	[InlineData("BEGIN TRAN 5:a.b WITH MARK 'x'", "a numbered name marked")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((t.b, c), @s, LANGUAGE 1033)", "CONTAINS with a language")]
	[InlineData("SELECT a FROM t WHERE FREETEXT (b, N'x')", "FREETEXT of one column")]
	[InlineData("SELECT a FROM t WHERE FREETEXT ((b), N'x')", "FREETEXT of one column in brackets")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((*), 'x')", "CONTAINS of every column")]
	[InlineData("SELECT a FROM t WHERE a IS NOT DISTINCT FROM ANY (SELECT b FROM u)", "IS NOT DISTINCT FROM ANY")]
	[InlineData("SELECT a FROM t WHERE a IS DISTINCT FROM (SELECT b FROM u)", "IS DISTINCT FROM a subquery")]
	[InlineData("SELECT AI_GENERATE_EMBEDDINGS ('x' USE MODEL [dbo.m] PARAMETERS (TRY_CONVERT (JSON, N'{}')))", "embeddings with parameters in brackets")]
	[InlineData("SELECT AI_GENERATE_EMBEDDINGS ('x' USE MODEL dbo.m PARAMETERS TRY_CONVERT (JSON, N'{}'))", "embeddings with parameters")]
	[InlineData("SELECT AI_GENERATE_EMBEDDINGS (@t USE MODEL m)", "embeddings")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1 WITH (DISTRIBUTED_AGG), c2", "a grouping column moved to be aggregated")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1, c2", "a grouping column")]
	[InlineData("SELECT * FROM AI_GENERATE_CHUNKS (SOURCE = 'x', CHUNK_TYPE = fixed, CHUNK_SIZE = 5) AS c", "chunks")]
	[InlineData("SELECT * FROM AI_GENERATE_CHUNKS (SOURCE = 'x', CHUNK_TYPE = FIXED, CHUNK_SIZE = 5, OVERLAP = 50, ENABLE_CHUNK_SET_ID = 1)", "chunks with every argument")]
	[InlineData("SELECT IDENTITY (INT, 1, 1) AS id INTO #t FROM t", "an identity")]
	[InlineData("SELECT id = IDENTITY (DECIMAL (10, 0), 1.5, +1) INTO t2 FROM t", "an identity named first with signed numbers")]
	[InlineData("SELECT IDENTITY (INT) c INTO t2 FROM t", "an identity of a type alone")]
	[InlineData("SELECT c1 FROM @v.f (1) AS t (c)", "a variable's method")]
	[InlineData("SELECT c1 FROM @v.[f.g] () t", "a variable's method of a bracketed name")]
	[InlineData("SELECT c1 FROM @v.f (CAST (1 AS INT)) t", "a variable's method with an AS in its arguments")]
	[InlineData("SELECT c1 FROM dbo.f (1) AS t", "a function")]
	[InlineData("SELECT DATEPART ([yy], x) FROM t", "a date part bracketed")]
	[InlineData("SELECT DATEPART ((year), x) FROM t", "a date part in brackets")]
	[InlineData("SELECT DATEADD (@p, 1, x) FROM t", "a variable for a date part")]
	[InlineData("SELECT DATEPART (yy, x) FROM t", "a date part")]
	[InlineData("SELECT * FROM @v.DATEPART (yy, x) AS t", "a variable's method named as a built-in")]
	[InlineData("SELECT AI_GENERATE_EMBEDDINGS ('x' USE MODEL PARAMETERS)", "embeddings of a model named PARAMETERS")]
	[InlineData("SELECT AI_GENERATE_EMBEDDINGS ('x' USE MODEL PARAMETERS PARAMETERS p)", "embeddings of a model named PARAMETERS with parameters")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (PROPERTY (c, 'Title'), 'x')", "CONTAINS of a property")]
	[InlineData("SELECT AI_GENERATE_EMBEDDINGS ('x' USE MODEL PARAMETERS.PARAMETERS PARAMETERS (1))", "embeddings of a model PARAMETERS.PARAMETERS with parameters")]
	[InlineData("SELECT a FROM t WHERE FREETEXT (PROPERTY (db..t.c, 'Title'), 'x')", "FREETEXT of a property with a part left out")]
	public void The_bridge_builds_the_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		var holds = expected switch
		{
			"a numbered name" => made is Ast.Statement.SaveTransaction { Name: { Name: null, Variable: null, Number.Text: "-5", Qualified.Count: 2 } },
			"a numbered name with blanks" => made is Ast.Statement.SaveTransaction { Name: { Number.Text: "-100", Qualified: [{ Text: "[a ]" }, { Text: "[b ]" }] } },
			"a name" => made is Ast.Statement.SaveTransaction { Name: { Name.Text: "t1", Number: null, Qualified: null } },
			"a numbered name marked" => made is Ast.Statement.BeginTransaction { Name.Number.Text: "5", Mark: not null },
			"CONTAINS with a language" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { Kind: Ast.FullTextPredicateKind.Contains, ColumnsBracketed: true, Columns.Count: 2, Search: Ast.Expression.Variable, Language: Ast.LiteralValue.Numeric } } },
			"FREETEXT of one column" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { Kind: Ast.FullTextPredicateKind.FreeText, ColumnsBracketed: false, Language: null } } },
			"FREETEXT of one column in brackets" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { ColumnsBracketed: true, Columns.Count: 1 } } },
			"CONTAINS of every column" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { Columns: [Ast.Expression.Asterisk], ColumnsBracketed: true } } },
			"IS NOT DISTINCT FROM ANY" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.IsDistinct { Not: true, Quantifier: Ast.Quantifier.Any, Right: Ast.Expression.Subquery } } },
			"IS DISTINCT FROM a subquery" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.IsDistinct { Quantifier: null } } },
			"embeddings with parameters in brackets" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.AiGenerateEmbeddings { Model: [{ Style: Ast.IdentifierStyle.Bracketed }], Parameters: Ast.Expression.Parenthesized } }] } },
			"embeddings with parameters" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.AiGenerateEmbeddings { Model.Count: 2, Parameters: Ast.Expression.Convert } }] } },
			"embeddings" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.AiGenerateEmbeddings { Source: Ast.Expression.Variable, Parameters: null } }] } },
			"a grouping column moved to be aggregated" => made is Ast.Statement.Select { Query: Ast.Query.Select { GroupBy.Items: [Ast.GroupingElement.Ordinary { DistributedAggregation: true }, Ast.GroupingElement.Ordinary { DistributedAggregation: false }] } },
			"a grouping column" => made is Ast.Statement.Select { Query: Ast.Query.Select { GroupBy.Items: [Ast.GroupingElement.Ordinary { DistributedAggregation: false }, _] } },
			"chunks" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.AiGenerateChunks { ChunkType.Text: "fixed", Overlap: null, EnableChunkSetId: null, Alias.Name.Text: "c" }] } },
			"chunks with every argument" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.AiGenerateChunks { Overlap: not null, EnableChunkSetId: not null, Alias: null }] } },
			"an identity" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.Identity { Type: Ast.DataType.Numeric { Kind: Ast.NumericTypeKind.Int }, Seed.Text: "1", Increment.Text: "1" } }], IntoTable: not null } },
			"an identity named first with signed numbers" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { AliasFirst: true, Expression: Ast.Expression.Identity { Type: Ast.DataType.Numeric { Precision: 10, Scale: 0 }, Seed: { Text: "1.5", Kind: Ast.NumericLiteralKind.Decimal }, Increment.Text: "+1" } }] } },
			"an identity of a type alone" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.Identity { Seed: null, Increment: null } }] } },
			"a variable's method" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.Function { Variable.Text: "@v", Invocation: { Name: [{ Text: "f" }], Arguments.Count: 1 }, Alias: { AsKeyword: true, Columns.Count: 1 } }] } },
			"a variable's method with an AS in its arguments" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.Function { Variable.Text: "@v", Alias.AsKeyword: false }] } },
			"a variable's method of a bracketed name" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.Function { Variable.Text: "@v", Invocation: { Name: [{ Text: "[f.g]", Style: Ast.IdentifierStyle.Bracketed }], Arguments.Count: 0 } }] } },
			"a function" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.Function { Variable: null, Invocation.Name.Count: 2 }] } },
			"a date part bracketed" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.Invocation { Arguments: [{ Value: Ast.Expression.Reference { Name: [{ Text: "[yy]", Style: Ast.IdentifierStyle.Bracketed }] } }, _] } }] } },
			"a date part in brackets" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.Invocation { Arguments: [{ Value: Ast.Expression.Parenthesized { Value: Ast.Expression.Reference } }, _] } }] } },
			"a variable for a date part" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.Invocation { Arguments: [{ Value: Ast.Expression.Variable }, _, _] } }] } },
			"a date part" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.Invocation { Arguments: [{ Value: Ast.Expression.DatePart { Kind: Ast.DatePartKind.Year, Spelling: Ast.DatePartSpelling.Abbreviation } }, _] } }] } },
			"a variable's method named as a built-in" => made is Ast.Statement.Select { Query: Ast.Query.Select { From.Sources: [Ast.TableSource.Function { Variable.Text: "@v", Invocation: { Name: [{ Text: "DATEPART" }], Arguments: [{ Value: Ast.Expression.Reference { Name: [{ Text: "yy" }] } }, _] } }] } },
			"embeddings of a model named PARAMETERS" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.AiGenerateEmbeddings { Model: [{ Text: "PARAMETERS" }], Parameters: null } }] } },
			"embeddings of a model named PARAMETERS with parameters" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.AiGenerateEmbeddings { Model: [{ Text: "PARAMETERS" }], Parameters: Ast.Expression.Reference } }] } },
			"CONTAINS of a property" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { Columns: [Ast.Expression.Invocation { Name: [{ Text: "PROPERTY" }], Arguments.Count: 2 }], ColumnsBracketed: false } } },
			"embeddings of a model PARAMETERS.PARAMETERS with parameters" => made is Ast.Statement.Select { Query: Ast.Query.Select { Columns: [Ast.SelectColumn.ExpressionItem { Expression: Ast.Expression.AiGenerateEmbeddings { Model: [{ Text: "PARAMETERS" }, { Text: "PARAMETERS" }], Parameters: Ast.Expression.Parenthesized { Value: Ast.LiteralValue.Numeric } } }] } },
			"FREETEXT of a property with a part left out" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { Kind: Ast.FullTextPredicateKind.FreeText, Columns: [Ast.Expression.Invocation { Arguments: [{ Value: Ast.Expression.Reference { Name: [{ Text: "db" }, { Text: "" }, { Text: "t" }, { Text: "c" }] } }, _] }] } } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		ReadBack.Holds(text);
	}
}
