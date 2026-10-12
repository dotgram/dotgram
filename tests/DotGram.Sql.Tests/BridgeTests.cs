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
	[InlineData("SELECT CAST (a AS money) FROM t WHERE b = 1", "SELECT CAST (a AS money) FROM t WHERE b = 2", BridgeOracle.TypeName, true)]
	[InlineData("SELECT DATEADD (dd, 1, a) FROM t",    "SELECT DATEADD (dd, 1, a) FROM t",    BridgeOracle.DatePart, false)]
	[InlineData("SELECT DATEADD (dd, 1, a) FROM t",    "SELECT DATEADD (dd, 2, a) FROM t",    BridgeOracle.DatePart, true)]
	[InlineData("SELECT a FROM t GROUP BY ALL a",      "SELECT a FROM t GROUP BY ALL a",      BridgeOracle.GroupAll, false)]
	[InlineData("SELECT a FROM t GROUP BY ALL a",      "SELECT a FROM t GROUP BY ALL b",      BridgeOracle.GroupAll, true)]
	[InlineData("SELECT a INTO t FROM u",              "SELECT a INTO t FROM u",              BridgeOracle.Into, false)]
	[InlineData("SELECT a INTO t FROM u",              "SELECT a INTO v FROM u",              BridgeOracle.Into, true)]
	[InlineData("SELECT a AS x FROM t OPTION (RECOMPILE)", "SELECT a AS x FROM t OPTION (RECOMPILE)", BridgeOracle.Option, false)]
	[InlineData("SELECT a AS x FROM t OPTION (RECOMPILE)", "SELECT a AS y FROM t OPTION (RECOMPILE)", BridgeOracle.Option, true)]
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
		var made = new Ast.QualifiedName([new Ast.Identifier("INT")]);

		foreach (var (bridged, standard) in new (Ast.DataType, Ast.DataType)[]
		{
			(new Ast.DataType.Named(mine, []), new Ast.DataType.UserDefined(mine)),
			(new Ast.DataType.Named(made, []), new Ast.DataType.Numeric(Ast.NumericTypeKind.Int)),
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
	[InlineData("SELECT * FROM OPENXML (@h, '/r') WITH t2 x",             "TableSource.OpenXml.SchemaTable")]
	[InlineData("SELECT * FROM OPENDATASOURCE ('p', 'i').db.dbo.t",       "TableSource.Named.DataSource")]
	[InlineData("SELECT * FROM t PIVOT (SUM (a) FOR b IN ([1], [2])) p", "TableSource.Pivot")]
	[InlineData("SELECT * FROM t UNPIVOT (v FOR c IN (a, b)) u",          "TableSource.Unpivot")]
	[InlineData("SELECT * FROM t TABLESAMPLE (10)",                       "TableSource.Named.SampleMethodOmitted")]
	[InlineData("SELECT * FROM OPENROWSET ('MSDASQL', 'srv'; 'u'; 'p', 'SELECT 1') r", "TableSource.OpenRowset.Connection")]
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
			"TableSource.OpenXml.SchemaTable"       => source is Ast.TableSource.OpenXml { Schema: null, SchemaTable.Parts: [{ Text: "t2" }] },
			"TableSource.Named.DataSource"          => source is Ast.TableSource.Named { DataSource: not null, Name.Parts.Count: 3 },
			"TableSource.Pivot"                     => source is Ast.TableSource.Pivot { For.Text: "b", In: [{ Text: "[1]" }, { Text: "[2]" }], Alias.Name.Text: "p" },
			"TableSource.Unpivot"                   => source is Ast.TableSource.Unpivot { Value.Text: "v", For.Text: "c", In: [{ Text: "a" }, { Text: "b" }], Alias.Name.Text: "u" },
			"TableSource.Named.SampleMethodOmitted" => source is Ast.TableSource.Named { SampleMethodOmitted: true, SampleUnit: null, Sample: not null },
			_                                       => source is Ast.TableSource.OpenRowset { Connection.Count: 3 }
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
