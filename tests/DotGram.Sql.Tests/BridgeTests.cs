using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The bridge from the old T-SQL tree to the new one, held by its round trip over every statement T-SQL
/// reads and by a baseline per family (docs/design/sql-tsql-bridge.md).
/// </summary>
public sealed class BridgeTests(ITestOutputHelper output)
{
	/// <summary>
	/// The statements expected to fail the round trip because of a known writer defect, each with the status
	/// it fails with, as flattened text. Listed one by one, so that a statement newly broken cannot hide
	/// behind one fixed: one that fails and is not here fails the test, and one here that passes is printed.
	/// </summary>
	static readonly Dictionary<string, BridgeOracle.Status> Expected = new(StringComparer.Ordinal)
	{
	};

	/// <summary>
	/// Every concrete old record is owned by exactly one family, and every name a family owns is a record.
	/// </summary>
	[Fact]
	public void Every_old_record_belongs_to_one_family()
	{
		Type[] roots   = [typeof(Statement), typeof(Query), typeof(Expression), typeof(TableReference), typeof(Clause), typeof(SetExpression)];
		var    records = typeof(Statement).Assembly.GetTypes()
			.Where(one => one is { IsClass: true, IsAbstract: false } && roots.Any(one.IsSubclassOf))
			.Select(Bridge.NameOf)
			.ToHashSet(StringComparer.Ordinal);

		var owned  = Bridge.Ownership.SelectMany(static family => family.Owns).ToList();
		var faults = new List<string>();

		faults.AddRange(records.Where(one => !owned.Contains(one)).Select(static one => $"{one}: no family owns it"));
		faults.AddRange(owned.GroupBy(static one => one).Where(static one => one.Count() > 1).Select(static one => $"{one.Key}: owned {one.Count()} times"));
		faults.AddRange(owned.Where(one => !records.Contains(one)).Select(static one => $"{one}: owned and not an old record"));

		Assert.Equal([], faults);
	}

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
	/// Free text compares token by token: a key word without case, and nothing else — not a name in brackets,
	/// not a word that is no key word, not a string.
	/// </summary>
	[Theory]
	[InlineData("with (delayed_durability = on)", "WITH (DELAYED_DURABILITY = ON)", true)]
	[InlineData("varchar (10)",                   "VARCHAR(10)",                    true)]
	[InlineData("national character varying(5)", "NATIONAL CHARACTER VARYING(5)",  true)]
	[InlineData("AUTHORIZATION [a b]",            "AUTHORIZATION [ab]",             false)]
	[InlineData("BEGIN TRAN T1",                  "BEGIN TRAN t1",                  false)]
	[InlineData("WITH MARK 'A'",                  "WITH MARK 'a'",                  false)]
	[InlineData("AS dbo.MyInt",                   "AS dbo.myint",                   false)]
	public void Free_text_compares_key_words_without_case_and_nothing_else(string one, string two, bool same)
	{
		Assert.Equal(same, Trees.SameTokens(new object(), "Statement.Transaction.Tail", one, two));
	}

	/// <summary>
	/// A key word folds only where it is one: a routine's name of several parts, and the owner, mark, transaction
	/// or savepoint after its word, are names whose case a collation may tell apart.
	/// </summary>
	[Theory]
	[InlineData("Expression.RoutineInvocation.Name", "max",                 "MAX",                 true)]
	[InlineData("Expression.RoutineInvocation.Name", "dbo.max",             "dbo.MAX",             false)]
	[InlineData("Statement.SchemaDefinition.Tail",   "AUTHORIZATION cache", "AUTHORIZATION CACHE", false)]
	[InlineData("Statement.Transaction.Tail",        "WITH MARK cache",     "WITH MARK CACHE",     false)]
	public void A_key_word_folds_only_where_it_is_one(string member, string one, string two, bool same)
	{
		Assert.Equal(same, Trees.SameTokens(new object(), member, one, two));
	}

	/// <summary>
	/// A block comment nests, and a word after a comment is the word after the node.
	/// </summary>
	[Fact]
	public void Comments_are_skipped_and_nest()
	{
		Assert.Equal(["[realowner]"], SqlTokens.Of("/* outer /* inner */ cache -- */ [realowner]").Select(static one => one.Text));

		const string text = "SELECT TOP (1) a /* c */ AS x FROM t";

		var made   = (Ast.Statement.Select)Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);
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

		var old = TransactSqlParser.Located.ParseStatement(text);

		try
		{
			Assert.Equal("[realowner]", ((Ast.Statement.CreateSchema)Bridge.ToAst(old, text)).Authorization!.Name.Text);
		}
		catch (BridgeGap)
		{
		}
	}

	/// <summary>
	/// A word the bridge read from the text and the writer then lost is found by the round trip on the new tree.
	/// </summary>
	[Theory]
	[InlineData("SELECT a AS x FROM t", "SELECT a AS x FROM t", true)]
	[InlineData("SELECT a AS x FROM t", "SELECT a x FROM t",    false)]
	public void A_word_read_from_the_text_is_held_to_the_written_text(string text, string written, bool same)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.Equal(same, BridgeOracle.Again(made, written) is null);
	}

	/// <summary>
	/// A dialect pattern exempts its own subtree and no more: the same construct with a difference elsewhere is
	/// a shape mismatch.
	/// </summary>
	[Theory]
	[InlineData("SELECT TOP (1) a FROM t WHERE b = 1", "SELECT TOP (1) a FROM t WHERE b = 1", BridgeOracle.Top, false)]
	[InlineData("SELECT TOP (1) a FROM t WHERE b = 1", "SELECT TOP (1) a FROM t WHERE b = 2", BridgeOracle.Top, true)]
	[InlineData("SELECT TOP (1) a FROM t WHERE b = 1", "SELECT TOP (1) a, b FROM t WHERE b = 1", BridgeOracle.Top, true)]
	[InlineData("UPDATE t SET u.c = 1 WHERE b = 1",    "UPDATE t SET u.c = 1 WHERE b = 1",    BridgeOracle.SetTc, false)]
	[InlineData("UPDATE t SET u.c = 1 WHERE b = 1",    "UPDATE t SET u.c = 1 WHERE b = 2",    BridgeOracle.SetTc, true)]
	[InlineData("OPEN GLOBAL c",                        "OPEN GLOBAL c",                       BridgeOracle.Global, false)]
	[InlineData("UPDATE t SET a = 1 WHERE CURRENT OF GLOBAL c", "UPDATE t SET a = 2 WHERE CURRENT OF GLOBAL c", BridgeOracle.Global, true)]
	[InlineData("SELECT a FROM t WHERE FREETEXT ((b), 'x')", "SELECT a FROM t WHERE FREETEXT ((b), 'x')", BridgeOracle.FullText, false)]
	[InlineData("SELECT a FROM t WHERE FREETEXT ((b), 'x')", "SELECT a FROM t WHERE FREETEXT ((b), 'y')", BridgeOracle.FullText, true)]
	[InlineData("SELECT CAST (a AS money) FROM t",     "SELECT CAST (a AS money) FROM t",     BridgeOracle.TypeName, false)]
	[InlineData("SELECT CAST (a AS timestamp) FROM t", "SELECT CAST (a AS timestamp) FROM t", BridgeOracle.TypeName, false)]
	[InlineData("SELECT CAST (a AS nvarchar) FROM t",  "SELECT CAST (a AS nvarchar) FROM t",  BridgeOracle.TypeName, false)]
	[InlineData("SELECT CAST (a AS sys.sysname) FROM t", "SELECT CAST (a AS sys.sysname) FROM t", BridgeOracle.TypeName, false)]
	[InlineData("SELECT CAST (a AS \"int\") FROM t",   "SELECT CAST (a AS \"int\") FROM t",   BridgeOracle.TypeName, false)]
	[InlineData("SELECT CAST (a AS sys.\"int\") FROM t", "SELECT CAST (a AS sys.\"int\") FROM t", BridgeOracle.TypeName, false)]
	[InlineData("SELECT CAST (a AS \"int\") FROM t WHERE b = 1", "SELECT CAST (a AS \"int\") FROM t WHERE b = 2", BridgeOracle.TypeName, true)]
	[InlineData("SELECT CAST (a AS xml) FROM t",       "SELECT CAST (a AS xml) FROM t",       BridgeOracle.TypeName, false)]
	[InlineData("SELECT CAST (a AS money) FROM t WHERE b = 1", "SELECT CAST (a AS money) FROM t WHERE b = 2", BridgeOracle.TypeName, true)]
	[InlineData("SELECT DATEADD (dd, 1, a) FROM t",    "SELECT DATEADD (dd, 1, a) FROM t",    BridgeOracle.DatePart, false)]
	[InlineData("SELECT DATEADD (dd, 1, a) FROM t",    "SELECT DATEADD (dd, 2, a) FROM t",    BridgeOracle.DatePart, true)]
	[InlineData("SELECT a FROM t GROUP BY ALL a",      "SELECT a FROM t GROUP BY ALL a",      BridgeOracle.GroupAll, false)]
	[InlineData("SELECT a FROM t GROUP BY ALL a",      "SELECT a FROM t GROUP BY ALL b",      BridgeOracle.GroupAll, true)]
	[InlineData("SELECT a INTO t FROM u",              "SELECT a INTO t FROM u",              BridgeOracle.Into, false)]
	[InlineData("SELECT a INTO t FROM u",              "SELECT a INTO v FROM u",              BridgeOracle.Into, true)]
	[InlineData("SELECT a AS x FROM t OPTION (RECOMPILE)", "SELECT a AS x FROM t OPTION (RECOMPILE)", BridgeOracle.Option, false)]
	[InlineData("SELECT a AS x FROM t OPTION (RECOMPILE)", "SELECT a AS y FROM t OPTION (RECOMPILE)", BridgeOracle.Option, true)]
	[InlineData("SELECT x = b FROM t",                 "SELECT x = b FROM t",                 BridgeOracle.Named, false)]
	[InlineData("SELECT x = b FROM t WHERE c = 1",     "SELECT x = b FROM t WHERE c = 2",     BridgeOracle.Named, true)]
	public void A_dialect_pattern_exempts_its_subtree_and_no_more(string bridged, string standard, string pattern, bool shape)
	{
		var made        = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(bridged), bridged);
		var differences = Trees.Differences(made, SqlStandardParser.ParseStatement(standard), old: false);
		var exempted    = BridgeOracle.Meaning(differences, out var unexplained);

		Assert.Contains(pattern, exempted);
		Assert.Equal(shape, unexplained.Count > 0);
	}

	/// <summary>
	/// A user-defined type is the standard's, and a bridge that built another record for it, or for a type the
	/// standard spells, would be a shape mismatch the type-name pattern does not exempt.
	/// </summary>
	[Fact]
	public void The_type_name_pattern_exempts_only_a_system_type()
	{
		const string text = "SELECT CAST (a AS MyType) FROM t";

		Assert.Equal(SqlStandardParser.ParseStatement(text), Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));

		var mine = new Ast.QualifiedName([new Ast.Identifier("MyType")]);
		var made   = new Ast.QualifiedName([new Ast.Identifier("INT")]);
		var xml    = new Ast.QualifiedName([new Ast.Identifier("[xml]", Ast.IdentifierStyle.Bracketed)]);
		var quoted = new Ast.QualifiedName([new Ast.Identifier("\"int\"", Ast.IdentifierStyle.Delimited)]);

		foreach (var (bridged, standard) in new (Ast.DataType, Ast.DataType)[]
		{
			(new Ast.DataType.Named(mine, []), new Ast.DataType.UserDefined(mine)),
			(new Ast.DataType.Named(made, []), new Ast.DataType.Numeric(Ast.NumericTypeKind.Int)),
			(new Ast.DataType.Named(xml, []), new Ast.DataType.UserDefined(xml)),
			(new Ast.DataType.Numeric(Ast.NumericTypeKind.Int, 10) { WrittenName = quoted }, new Ast.DataType.UserDefined(quoted)),
		})
		{
			BridgeOracle.Meaning(Trees.Differences(bridged, standard, old: false), out var unexplained);

			Assert.NotEmpty(unexplained);
		}
	}

	/// <summary>
	/// Every one of T-SQL's system types, bracketed, is the type its bare word is: a standard spelling of one word the
	/// standard's record written as a name, and the rest <c>DataType.Named</c>.
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

		if (Bridge.Values.StandardWords.Contains(name))
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
	[InlineData("UPDATE t SET @v -= 1",                                            "Assignment variable")]
	[InlineData("UPDATE t SET c ||= 'x'",                                          "Assignment compound")]
	[InlineData("UPDATE t SET c.WRITE (@x, 0, NULL)",                              "Assignment call")]
	[InlineData("UPDATE t SET a = 1",                                              "Assignment plain")]
	[InlineData("MERGE INTO t USING u ON t.a = u.a WHEN NOT MATCHED BY SOURCE AND t.b = 1 THEN UPDATE SET b = 2 WHEN NOT MATCHED BY TARGET THEN INSERT DEFAULT VALUES;", "Merge.BySource ByTarget DefaultValues")]
	public void The_bridge_builds_the_data_change_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

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
			"Assignment variable"         => made is Ast.Statement.Update { Assignments: [{ Variable.Name.Text: "@v", Targets.Count: 0, Operator: Ast.AssignmentOperator.Subtract }] },
			"Assignment compound"         => made is Ast.Statement.Update { Assignments: [{ Variable: null, Targets.Count: 1, Operator: Ast.AssignmentOperator.Concatenate }] },
			"Assignment call"             => made is Ast.Statement.Update
			{
				Assignments: [{ Variable: null, Targets.Count: 0, Value: Ast.Expression.Member { Kind: Ast.MemberAccessKind.Dot, Name.Text: "WRITE", Arguments.Count: 3, Target: Ast.Expression.Reference } }],
			},
			"Assignment plain"            => made is Ast.Statement.Update { Assignments: [{ Variable: null, Operator: Ast.AssignmentOperator.Assign, Targets.Count: 1 }] },
			_                             => made is Ast.Statement.Merge { IntoOmitted: true, Output.Items.Count: 1 },
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	/// <summary>
	/// Statements that must convert: the case of what the tree does not keep is folded, and a system type's name
	/// however written is the type.
	/// </summary>
	[Theory]
	[InlineData("SELECT 0XFF FROM t")]
	[InlineData("SELECT CAST (a AS [xml]) FROM t")]
	[InlineData("SELECT CAST (a AS [xml] (CONTENT s)) FROM t")]
	public void A_statement_converts(string text)
	{
		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
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
	public void The_bridge_builds_the_record_the_text_means(string text, string expected)
	{
		var made  = (Ast.Statement.Select)Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);
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
				value is Ast.Expression.Convert { Style: Ast.Expression.Literal { Value: Ast.LiteralValue.Numeric { Text: "112" } }, Try: false },
			"Expression.Parse { Try, Culture = 'en-US' }" =>
				value is Ast.Expression.Parse { Try: true, Culture: Ast.Expression.Literal { Value: Ast.LiteralValue.String { Text: "'en-US'" } } },
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
				value is Ast.Expression.Literal { Value: Ast.LiteralValue.Binary { Text: "0A", ZeroX: true } },
			"Expression.NextValue { Over }" =>
				value is Ast.Expression.NextValue { Over.OrderBy.Items.Count: 1 },
			"WindowFrame { Rows, 2 Preceding }" =>
				value is Ast.Expression.Invocation { Over: Ast.WindowReference.Specification { Value.Frame: { Unit: Ast.WindowFrameUnit.Rows, Extent: Ast.WindowFrameExtent.Single { Bound: { Kind: Ast.WindowBoundKind.Preceding, Offset: not null } } } } },
			"Expression.Invocation { WithinGroup }" =>
				value is Ast.Expression.Invocation { WithinGroup.OrderBy.Items.Count: 1 },
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
		var made   = (Ast.Statement.Select)Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);
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
					{ Value: Ast.Expression.Literal { Value: Ast.LiteralValue.Binary { ZeroX: true } } },
					{ Value: Ast.Expression.Literal { Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Approximate } } },
					{ Value: Ast.Expression.Literal { Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Decimal } } },
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
			"ForClause.Xml.ElementName"             => select.For is Ast.ForClause.Xml { Mode: Ast.XmlMode.Path, ElementName: Ast.Expression.Literal, Directives: [] },
			"ForDirective"                          => select.For is Ast.ForClause.Xml
			{
				Mode: Ast.XmlMode.Auto, ElementName: not null,
				Directives: [{ Kind: Ast.ForDirectiveKind.Elements, Absent: true, Name: null }, { Kind: Ast.ForDirectiveKind.Root, Name: not null }],
			},
			_                                       => source is Ast.TableSource.OpenRowset { ProviderString: null, Login: not null }
				&& Ast.Sql2023Writer.Write(made).Contains("'srv'; 'u'; 'p'", StringComparison.Ordinal),
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
		var made = (Ast.Statement.Select)Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);
		var from = ((Ast.Query.Select)made.Query).From!.Sources[0];

		Assert.Equal(said, ((Ast.TableSource.Named)from).Alias!.AsKeyword);
	}

	/// <summary>
	/// A schema's owner is one name; what follows it is a schema element the old tree keeps as text.
	/// </summary>
	[Theory]
	[InlineData("CREATE SCHEMA s AUTHORIZATION [dbo]", false)]
	[InlineData("CREATE SCHEMA s AUTHORIZATION [dbo] CREATE TABLE t (c int)", true)]
	[InlineData("CREATE SCHEMA s AUTHORIZATION dbo CREATE TABLE t (c int)", true)]
	public void A_schema_owner_is_one_name(string text, bool gap)
	{
		var old = TransactSqlParser.Located.ParseStatement(text);

		if (gap)
			Assert.Throws<BridgeGap>(() => Bridge.ToAst(old, text));
		else
			Assert.Equal("[dbo]", ((Ast.Statement.CreateSchema)Bridge.ToAst(old, text)).Authorization!.Name.Text);
	}

	/// <summary>
	/// Every statement of the corpus and of <c>TransactSqlTests</c> carried, written and read back, and no
	/// family doing worse than its baseline: <c>converted</c> no lower, the other five no higher; every
	/// counter of <c>Baseline.Counters.txt</c> no higher; every failure of the round trip one of
	/// <see cref="Expected"/>.
	/// </summary>
	/// <remarks>
	/// <c>DOTGRAM_RECORD_BRIDGE_BASELINE=1</c> rewrites the baselines and docs/design/sql-tsql-bridge.md from
	/// this run instead of comparing; nothing is written otherwise.
	/// </remarks>
	[Fact]
	public void No_family_converts_worse_than_its_baseline()
	{
		var corpus = BridgeOracle.CorpusStatements().ToList();
		var tests  = BridgeOracle.TestStatements().ToList();

		Assert.Equal(BridgeOracle.CorpusCount(), corpus.Count);
		Assert.True(corpus.Count > 6_000, $"{corpus.Count} corpus statements");
		Assert.True(tests.Count > 1_000, $"{tests.Count} statements of TransactSqlTests");

		var fromCorpus = corpus.AsParallel().AsOrdered().Select(static one => BridgeOracle.Run(one.Statement, one.Source)).ToList();
		var fromTests  = tests.AsParallel().AsOrdered().Select(static one => BridgeOracle.Run(one.Statement, one.Source)).ToList();
		var outcomes   = fromCorpus.Concat(fromTests).ToList();

		var families = Bridge.Ownership
			.SelectMany(static family => family.Owns.Select(record => (Record: record, family.Family)))
			.ToDictionary(static one => one.Record, static one => one.Family, StringComparer.Ordinal);

		var counts   = Count(outcomes);
		var counters = Counters(outcomes);
		var gaps     = outcomes
			.SelectMany(static one => one.Gaps ?? [])
			.GroupBy(static one => one.Record)
			.ToDictionary(
				static group => group.Key,
				static group => group.GroupBy(static one => one.Reason).ToDictionary(static one => one.Key, static one => one.Count()),
				StringComparer.Ordinal);

		Summarize("corpus", fromCorpus);
		Summarize("TransactSqlTests", fromTests);
		Summarize("both", outcomes, 60);
		Faults(outcomes);

		Assert.Equal([], counts.Keys.Where(one => !families.ContainsKey(one)).Select(static one => $"{one}: counted and owned by no family"));

		if (Environment.GetEnvironmentVariable(Baselines.Variable) is { Length: > 0 })
		{
			foreach (var (family, owns) in Bridge.Ownership)
				Baselines.Write(family, owns, counts);

			Baselines.WriteCounters(counters);

			var occurrences = corpus.Concat(tests)
				.SelectMany(static one => Trees.Nodes(one.Statement))
				.GroupBy(Bridge.NameOf)
				.ToDictionary(static group => group.Key, static group => group.Count(), StringComparer.Ordinal);

			BridgeReport.Write(families, counts, gaps, occurrences, counters, Expected);

			return;
		}

		var faults = new List<string>();
		var better = new List<string>();

		foreach (var (family, owns) in Bridge.Ownership)
		{
			var baseline = Baselines.Read(family);

			foreach (var record in owns)
			{
				var now  = counts.TryGetValue(record, out var found) ? found : Baselines.None;
				var then = baseline.TryGetValue(record, out var kept) ? kept : null;

				if (then is null)
				{
					faults.Add($"{family}: {record} is not in Baseline.{family}.txt. Add `{record} = {now}`.");

					continue;
				}

				var worse = new List<string>();

				if (now.Converted < then.Converted)
					worse.Add($"converted {now.Converted} where the baseline converted {then.Converted}");

				Rise(worse, "refused", now.Refused, then.Refused);
				Rise(worse, "crashed", now.Crashed, then.Crashed);
				Rise(worse, "unread", now.Unread, then.Unread);
				Rise(worse, "different", now.Different, then.Different);
				Rise(worse, "shape", now.Shape, then.Shape);

				faults.AddRange(worse.Select(one => $"{family}: {record} {one}."));

				if (now != then && worse.Count == 0)
					better.Add($"{family}: {record} = {then} -> {now}");
			}
		}

		var recorded = Baselines.Rows("Counters").ToDictionary(static one => one.Key, static one => int.Parse(one.Value), StringComparer.Ordinal);

		foreach (var (key, now) in counters)
		{
			var then = recorded.TryGetValue(key, out var kept) ? kept : 0;

			if (now > then)
				faults.Add($"Counters: {key} = {now} against {then}.");
			else if (now < then)
				better.Add($"Counters: {key} = {then} -> {now}");
		}

		foreach (var one in outcomes.Where(static one => one.Status is not (BridgeOracle.Status.Converted or BridgeOracle.Status.Gap)))
		{
			if (!Expected.TryGetValue(Flat(one.Text), out var status) || status != one.Status)
				faults.Add($"{one.Status} and not expected: {Flat(one.Text)}");
		}

		foreach (var (text, status) in Expected)
			if (!outcomes.Any(one => one.Status == status && Flat(one.Text) == text))
				better.Add($"expected {status} and no longer: {text}");

		// Printed rather than asserted: the baselines are rewritten deliberately, not by a passing run.
		if (better.Count > 0)
			foreach (var line in better.Prepend($"changed for the better, so rewrite the baselines deliberately ({Baselines.Variable}=1):"))
				output.WriteLine(line);

		Assert.Equal([], faults);
	}

	static void Rise(List<string> worse, string what, int now, int then)
	{
		if (now > then)
			worse.Add($"{what} {now} against {then}");
	}

	static Dictionary<string, Baselines.Counts> Count(IEnumerable<BridgeOracle.Outcome> outcomes)
	{
		var counts = new Dictionary<string, Baselines.Counts>(StringComparer.Ordinal);

		foreach (var one in outcomes)
		{
			var was = counts.TryGetValue(one.Counted, out var found) ? found : Baselines.None;

			was = one.Status switch
			{
				BridgeOracle.Status.Converted => was with { Converted = was.Converted + 1 },
				BridgeOracle.Status.Gap       => was with { Gap = was.Gap + 1 },
				BridgeOracle.Status.Refused   => was with { Refused = was.Refused + 1 },
				BridgeOracle.Status.Crashed   => was with { Crashed = was.Crashed + 1 },
				BridgeOracle.Status.Unread    => was with { Unread = was.Unread + 1 },
				_                             => was with { Different = was.Different + 1 },
			};

			counts[one.Counted] = one.Shape ? was with { Shape = was.Shape + 1 } : was;
		}

		return counts;
	}

	/// <summary>
	/// The debts: each member passed only with key words' case aside, each read from the text, each dialect
	/// pattern's exempted subtrees. Every name of the closed lists has a row, zero or not.
	/// </summary>
	static Dictionary<string, int> Counters(IEnumerable<BridgeOracle.Outcome> outcomes)
	{
		var counters = new Dictionary<string, int>(StringComparer.Ordinal);

		foreach (var member in Trees.FreeText)
			counters["ByCase " + member] = 0;

		counters["ByCase " + Trees.DatePartWord] = 0;

		foreach (var member in Bridge.Recoverable)
			counters["Recovered " + member] = 0;

		foreach (var pattern in BridgeOracle.Patterns)
			counters["Meaning " + pattern] = 0;

		foreach (var one in outcomes)
		{
			foreach (var member in one.ByCase ?? [])
				counters["ByCase " + member]++;

			foreach (var member in one.Recovered ?? [])
				counters["Recovered " + member]++;

			foreach (var pattern in one.Dialects ?? [])
				counters["Meaning " + pattern]++;
		}

		return counters;
	}

	void Summarize(string name, IReadOnlyList<BridgeOracle.Outcome> outcomes, int reasons = 10)
	{
		int Of(BridgeOracle.Status status)
		{
			return outcomes.Count(one => one.Status == status);
		}

		output.WriteLine(
			$"{name}: {outcomes.Count} statements; converted {Of(BridgeOracle.Status.Converted)} " +
			$"({outcomes.Count(static one => one.ByCase is { Count: > 0 })} with the case of a key word aside), gap {Of(BridgeOracle.Status.Gap)}, " +
			$"refused {Of(BridgeOracle.Status.Refused)}, crashed {Of(BridgeOracle.Status.Crashed)}, unread {Of(BridgeOracle.Status.Unread)}, " +
			$"different {Of(BridgeOracle.Status.Different)}; read by the standard's parser {outcomes.Count(static one => one.Standard)}, " +
			$"shape {outcomes.Count(static one => one.Shape)}, read with another meaning {outcomes.Count(static one => one.Dialect)}");

		foreach (var (family, owns) in Bridge.Ownership)
		{
			var mine = outcomes.Where(one => owns.Contains(one.Counted)).ToList();

			if (mine.Count > 0)
				output.WriteLine(
					$"  {family}: converted {mine.Count(static one => one.Status == BridgeOracle.Status.Converted)}, " +
					$"gap {mine.Count(static one => one.Status == BridgeOracle.Status.Gap)}, " +
					$"refused {mine.Count(static one => one.Status == BridgeOracle.Status.Refused)}, " +
					$"crashed {mine.Count(static one => one.Status == BridgeOracle.Status.Crashed)}, " +
					$"unread {mine.Count(static one => one.Status == BridgeOracle.Status.Unread)}, " +
					$"different {mine.Count(static one => one.Status == BridgeOracle.Status.Different)}, " +
					$"shape {mine.Count(static one => one.Shape)}");
		}

		output.WriteLine($"  the {reasons} commonest first gaps:");

		foreach (var reason in outcomes.Where(static one => one.Reason is not null).GroupBy(static one => $"{one.Counted}: {one.Reason}").OrderByDescending(static one => one.Count()).Take(reasons))
			output.WriteLine($"    {reason.Count(),6}  {reason.Key}");
	}

	/// <summary>
	/// Every statement that was refused, crashed, unread, different, of another shape or read with another
	/// meaning, with what explains it.
	/// </summary>
	void Faults(IEnumerable<BridgeOracle.Outcome> outcomes)
	{
		foreach (var one in outcomes.Where(static one => one.Status is not (BridgeOracle.Status.Converted or BridgeOracle.Status.Gap) || one.Shape || one.Dialect).Take(300))
		{
			output.WriteLine($"{one.Status}{(one.Shape ? " (shape)" : "")}{(one.Dialect ? $" (another meaning: {string.Join("; ", one.Dialects!)})" : "")} {one.Root}: {Flat(one.Text)}");

			if (one.Written is not null)
				output.WriteLine($"    written: {one.Written}");

			if (one.Detail is not null)
				output.WriteLine($"    {one.Detail}");
		}
	}

	static string Flat(string text)
	{
		return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
	}
}
