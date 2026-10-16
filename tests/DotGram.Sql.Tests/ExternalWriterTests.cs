using System;
using System.Collections.Generic;
using System.Linq;

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
/// The External family's writer (Sql2023Writer.TransactSql.External.cs): each record written as the words of its kind and the
/// settings its place's catalogue reads, and every tree whose text would read back as another tree refused.
/// </summary>
public sealed class ExternalWriterTests
{
	static readonly Identifier X = new("x");
	static readonly Identifier Y = new("y");

	static Expression Str(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static Expression NStr(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.National);
	}

	static Expression Word(string word)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(word)]));
	}

	static Expression Named(Identifier name)
	{
		return new Expression.Reference(new QualifiedName([name]));
	}

	static Expression Digits(string digits)
	{
		return new LiteralValue.Numeric(digits, NumericLiteralKind.DecimalInteger);
	}

	static TableElement Column(string name, DataType type)
	{
		return new TableElement.Column(new ColumnDefinition(new Identifier(name), type, null, [], null));
	}

	static DotGram.Sql.Ast.Query Select(string text)
	{
		return ((Statement.Select)Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text)).Query;
	}

	static QualifiedName Dotted(params string[] parts)
	{
		return new QualifiedName([.. parts.Select(static one => new Identifier(one))]);
	}

	static Expression Call(string name, params Expression[] arguments)
	{
		return new Expression.Invocation(new QualifiedName([new Identifier(name)]), [.. arguments.Select(static one => new Argument(one))]);
	}

	static Expression Current()
	{
		return new Expression.Current(CurrentValue.Value);
	}

	static Option Opt(string name, Expression? value = null, params Option[] options)
	{
		return new Option(new Identifier(name), value, [.. options]);
	}

	/// <summary>The trees whose text is checked, with the text the grammar reads them from.</summary>
	static readonly Dictionary<string, (ISqlNode Tree, string Text)> Texts = new(StringComparer.Ordinal)
	{
		["Create data source"] = (
			new Statement.CreateExternalDataSource(X, [Opt("LOCATION", Str("abs://c@a.net/")), Opt("CREDENTIAL", Word("c")), Opt("PUSHDOWN", Word("OFF")), Opt("CONNECTION_OPTIONS", NStr("{}")), Opt("TYPE", Word("BLOB_STORAGE"))]),
			"CREATE EXTERNAL DATA SOURCE x WITH (LOCATION = 'abs://c@a.net/', CREDENTIAL = c, PUSHDOWN = OFF, CONNECTION_OPTIONS = N'{}', TYPE = BLOB_STORAGE)"),
		["Create data source with no setting"] = (
			new Statement.CreateExternalDataSource(X, []),
			"CREATE EXTERNAL DATA SOURCE x WITH ()"),
		["Create data source of elastic query"] = (
			new Statement.CreateExternalDataSource(X, [Opt("TYPE", Word("RDBMS")), Opt("LOCATION", Str("s")), Opt("DATABASE_NAME", Str("d")), Opt("SHARD_MAP_NAME", Str("m")), Opt("RESOURCE_MANAGER_LOCATION", Str("r"))]),
			"CREATE EXTERNAL DATA SOURCE x WITH (TYPE = RDBMS, LOCATION = 's', DATABASE_NAME = 'd', SHARD_MAP_NAME = 'm', RESOURCE_MANAGER_LOCATION = 'r')"),
		["Create data source in brackets"] = (
			new Statement.CreateExternalDataSource(new Identifier("[a b]", IdentifierStyle.Bracketed), [Opt("CREDENTIAL", Named(new Identifier("\"c d\"", IdentifierStyle.Delimited)))]),
			"CREATE EXTERNAL DATA SOURCE [a b] WITH (CREDENTIAL = \"c d\")"),
		["Alter data source"] = (
			new Statement.AlterExternalDataSource(X, [Opt("LOCATION", Str("l")), Opt("CREDENTIAL", Word("c")), Opt("TYPE", Word("HADOOP")), Opt("RESOURCE_MANAGER_LOCATION", Str("r")), Opt("PUSHDOWN", Word("ON")), Opt("CONNECTION_OPTIONS", Str("o"))]),
			"ALTER EXTERNAL DATA SOURCE x SET LOCATION = 'l', CREDENTIAL = c, TYPE = HADOOP, RESOURCE_MANAGER_LOCATION = 'r', PUSHDOWN = ON, CONNECTION_OPTIONS = 'o'"),
		["Create external table"] = (
			new Statement.CreateExternalTable(Dotted("dbo", "t"), new TableContents.Elements([Column("a", new DataType.Numeric(NumericTypeKind.Int)), Column("b", new DataType.Numeric(NumericTypeKind.Int))]))
			{
				Options = [Opt("LOCATION", Str("/f")), Opt("DATA_SOURCE", Word("ds")), Opt("FILE_FORMAT", Named(new Identifier("[ff]", IdentifierStyle.Bracketed))), Opt("REJECT_TYPE", Current()), Opt("REJECT_VALUE", new LiteralValue.Numeric("10.5", NumericLiteralKind.Decimal)), Opt("REJECT_SAMPLE_VALUE", Digits("1000")), Opt("REJECTED_ROW_LOCATION", NStr("/r"))],
			},
			"CREATE EXTERNAL TABLE dbo.t (a INT, b INT) WITH (LOCATION = '/f', DATA_SOURCE = ds, FILE_FORMAT = [ff], REJECT_TYPE = VALUE, REJECT_VALUE = 10.5, REJECT_SAMPLE_VALUE = 1000, REJECTED_ROW_LOCATION = N'/r')"),
		["Create external table with no setting"] = (
			new Statement.CreateExternalTable(Dotted("t"), new TableContents.Elements([Column("a", new DataType.Numeric(NumericTypeKind.Int))])),
			"CREATE EXTERNAL TABLE t (a INT) WITH ()"),
		["Create external table with a left out part"] = (
			new Statement.CreateExternalTable(new QualifiedName([new Identifier("db"), new Identifier(""), new Identifier("t")]), new TableContents.Elements([Column("a", new DataType.Numeric(NumericTypeKind.Int))])),
			"CREATE EXTERNAL TABLE db..t (a INT) WITH ()"),
		["Create external table of elastic query"] = (
			new Statement.CreateExternalTable(Dotted("t"), new TableContents.Elements([Column("a", new DataType.Numeric(NumericTypeKind.Int))]))
			{
				Options = [Opt("DATA_SOURCE", Word("ds")), Opt("SCHEMA_NAME", Str("s")), Opt("OBJECT_NAME", Str("o")), Opt("TABLE_OPTIONS", NStr("{}")), Opt("DISTRIBUTION", Call("SHARDED", Word("a")))],
			},
			"CREATE EXTERNAL TABLE t (a INT) WITH (DATA_SOURCE = ds, SCHEMA_NAME = 's', OBJECT_NAME = 'o', TABLE_OPTIONS = N'{}', DISTRIBUTION = SHARDED(a))"),
		["Create external table replicated"] = (
			new Statement.CreateExternalTable(Dotted("t"), new TableContents.Elements([Column("a", new DataType.Numeric(NumericTypeKind.Int))])) { Options = [Opt("DISTRIBUTION", Word("REPLICATED"))] },
			"CREATE EXTERNAL TABLE t (a INT) WITH (DISTRIBUTION = REPLICATED)"),
		["Create external table percentage"] = (
			new Statement.CreateExternalTable(Dotted("t"), new TableContents.Elements([Column("a", new DataType.Numeric(NumericTypeKind.Int))])) { Options = [Opt("REJECT_TYPE", Word("PERCENTAGE")), Opt("DISTRIBUTION", Word("ROUND_ROBIN"))] },
			"CREATE EXTERNAL TABLE t (a INT) WITH (REJECT_TYPE = PERCENTAGE, DISTRIBUTION = ROUND_ROBIN)"),
		// A data source called VALUE: the expression rule reads the word as the current value, and so does the tree.
		["Create external table of a data source called VALUE"] = (
			new Statement.CreateExternalTable(Dotted("t"), new TableContents.Elements([Column("a", new DataType.Numeric(NumericTypeKind.Int))])) { Options = [Opt("DATA_SOURCE", Current()), Opt("DISTRIBUTION", Call("SHARDED", Current()))] },
			"CREATE EXTERNAL TABLE t (a INT) WITH (DATA_SOURCE = VALUE, DISTRIBUTION = SHARDED(VALUE))"),
		["Create external table as select"] = (
			new Statement.CreateExternalTable(Dotted("dbo", "t"), new TableContents.AsQuery([], Select("SELECT 1 AS a"), null)) { Options = [Opt("LOCATION", Str("/f")), Opt("DATA_SOURCE", Word("ds"))] },
			"CREATE EXTERNAL TABLE dbo.t WITH (LOCATION = '/f', DATA_SOURCE = ds) AS SELECT 1 AS a"),
		["Create external table as select with no setting"] = (
			new Statement.CreateExternalTable(Dotted("t"), new TableContents.AsQuery([], Select("SELECT 1 AS a"), null)),
			"CREATE EXTERNAL TABLE t WITH () AS SELECT 1 AS a"),
		["Create file format"] = (
			new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_TYPE", Word("DELIMITEDTEXT")), Opt("DATA_COMPRESSION", Str("c")), Opt("FORMAT_OPTIONS", null, Opt("FIELD_TERMINATOR", Str("|")), Opt("STRING_DELIMITER", NStr("\"")), Opt("FIRST_ROW", Digits("2")), Opt("USE_TYPE_DEFAULT", Word("TRUE")), Opt("ENCODING", Str("UTF8")), Opt("DATE_FORMAT", Str("yyyy")), Opt("PARSER_VERSION", Str("2.0")))]),
			"CREATE EXTERNAL FILE FORMAT x WITH (FORMAT_TYPE = DELIMITEDTEXT, DATA_COMPRESSION = 'c', FORMAT_OPTIONS (FIELD_TERMINATOR = '|', STRING_DELIMITER = N'\"', FIRST_ROW = 2, USE_TYPE_DEFAULT = TRUE, ENCODING = 'UTF8', DATE_FORMAT = 'yyyy', PARSER_VERSION = '2.0'))"),
		["Create file format with no setting"] = (
			new Statement.CreateExternalFileFormat(X, []),
			"CREATE EXTERNAL FILE FORMAT x WITH ()"),
		// The type twice, an option twice, empty brackets and the brackets twice are all read.
		["Create file format repeated"] = (
			new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_TYPE", Word("ORC")), Opt("FORMAT_TYPE", Word("JSON")), Opt("SERDE_METHOD", Str("s")), Opt("FORMAT_OPTIONS"), Opt("FORMAT_OPTIONS", null, Opt("FIRST_ROW", Digits("1")), Opt("FIRST_ROW", Digits("2")))]),
			"CREATE EXTERNAL FILE FORMAT x WITH (FORMAT_TYPE = ORC, FORMAT_TYPE = JSON, SERDE_METHOD = 's', FORMAT_OPTIONS (), FORMAT_OPTIONS (FIRST_ROW = 1, FIRST_ROW = 2))"),
	};

	public static TheoryData<string> TreeNames
	{
		get { return [.. Texts.Keys]; }
	}

	/// <summary>Each tree written as the words of its kind and the settings of its place.</summary>
	[Theory]
	[MemberData(nameof(TreeNames))]
	public void Each_statement_writes_its_kind(string which)
	{
		Assert.Equal(Texts[which].Text, TransactSqlWriter.Write(Texts[which].Tree));
	}

	/// <summary>Each tree written and read back by TransactSqlParser, carried by the bridge to the same tree.</summary>
	[Theory]
	[MemberData(nameof(TreeNames))]
	public void Each_statement_reads_back_as_itself(string which)
	{
		var tree = (Statement)Texts[which].Tree;
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names, above it, the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	static readonly Dictionary<string, ISqlNode> Refusals = new(StringComparer.Ordinal)
	{
		// Identifier does not read a variable: `@x` is a TSql.Variable.
		["a data source named by a variable"] = new Statement.CreateExternalDataSource(new Identifier("@x"), []),
		// TSqlIdentifier is no reserved word, plain.
		["a data source named by a reserved word"] = new Statement.CreateExternalDataSource(new Identifier("select"), []),
		// Identifier does not read a string.
		["a data source named by a string"] = new Statement.CreateExternalDataSource(new Identifier("x", IdentifierStyle.String), []),
		// A regular name is one word: `a b` is two.
		["a data source named by two words"] = new Statement.CreateExternalDataSource(new Identifier("a b"), []),
		// BracketedIdentifier closes its bracket.
		["a data source named in an unclosed bracket"] = new Statement.CreateExternalDataSource(new Identifier("[x", IdentifierStyle.Bracketed), []),
		// TSqlIdentifier reads a word of one character at least.
		["a data source named by the empty text"] = new Statement.CreateExternalDataSource(new Identifier(""), []),
		// DataSourceSyntax reads the data source's own settings: FORMAT_TYPE is a file format's.
		["a data source setting it has not"] = new Statement.CreateExternalDataSource(X, [Opt("FORMAT_TYPE", Word("ORC"))]),
		// DataSourceItem lists a name once (`Syntax.NamedOnce`).
		["a data source setting twice"] = new Statement.CreateExternalDataSource(X, [Opt("LOCATION", Str("a")), Opt("LOCATION", Str("b"))]),
		// The generic option's `name value` form is not DataSourceSyntax's, which fixes `=`.
		["a data source setting with its = omitted"] = new Statement.CreateExternalDataSource(X, [new Option(new Identifier("LOCATION"), Str("a"), []) { EqualsOmitted = true }]),
		// LOCATION is Quoted, not a name.
		["a data source location that is a name"] = new Statement.CreateExternalDataSource(X, [Opt("LOCATION", Word("a"))]),
		// LOCATION is Quoted, plain or national: no character set and no Unicode string.
		["a data source location that is a Unicode string"] = new Statement.CreateExternalDataSource(X, [Opt("LOCATION", new LiteralValue.String("'a'", StringLiteralKind.Unicode))]),
		// A string closes before the text ends.
		["a data source location that is not closed"] = new Statement.CreateExternalDataSource(X, [Opt("LOCATION", new LiteralValue.String("'a", StringLiteralKind.Character))]),
		// CREDENTIAL is ExternalName: a name, not a string (Msg 102).
		["a data source credential that is a string"] = new Statement.CreateExternalDataSource(X, [Opt("CREDENTIAL", Str("c"))]),
		// ExternalName is `Identifier & ?!'.'`: one part.
		["a data source credential of two parts"] = new Statement.CreateExternalDataSource(X, [Opt("CREDENTIAL", new Expression.Reference(new QualifiedName([X, Y])))]),
		// ExternalName is an Identifier, no reserved word plain.
		["a data source credential named ON"] = new Statement.CreateExternalDataSource(X, [Opt("CREDENTIAL", Word("on"))]),
		// ExternalName is an Identifier, which does not read a variable.
		["a data source credential that is a variable"] = new Statement.CreateExternalDataSource(X, [Opt("CREDENTIAL", new Expression.Variable(new Identifier("@c")))]),
		// PUSHDOWN is `= ON` or `= OFF` (OnOffValue).
		["a data source pushdown that is a number"] = new Statement.CreateExternalDataSource(X, [Opt("PUSHDOWN", Digits("1"))]),
		["a data source pushdown that is another word"] = new Statement.CreateExternalDataSource(X, [Opt("PUSHDOWN", Word("TRUE"))]),
		// TYPE is one of four words, and a word is written plain.
		["a data source type outside its words"] = new Statement.CreateExternalDataSource(X, [Opt("TYPE", Word("FOO"))]),
		["a data source type in brackets"] = new Statement.CreateExternalDataSource(X, [Opt("TYPE", Named(new Identifier("[HADOOP]", IdentifierStyle.Bracketed)))]),
		// A data source made reads TYPE as a word and not as a string (Msg 102).
		["a data source type that is a string"] = new Statement.CreateExternalDataSource(X, [Opt("TYPE", Str("HADOOP"))]),
		// A name of several words is not a setting of this place.
		["a data source setting of two words"] = new Statement.CreateExternalDataSource(X, [new Option(new Identifier("LOCATION"), Str("a"), []) { Words = [new Identifier("OF")] }]),
		// A word of a name holds no blank.
		["a data source setting named with a blank"] = new Statement.CreateExternalDataSource(X, [Opt("LOCA TION", Str("a"))]),
		// A setting is a plain word.
		["a data source setting named in brackets"] = new Statement.CreateExternalDataSource(X, [new Option(new Identifier("[LOCATION]", IdentifierStyle.Bracketed), Str("a"), [])]),
		// A setting of a data source is a name and a value: no list of its own.
		["a data source setting with options of its own"] = new Statement.CreateExternalDataSource(X, [Opt("LOCATION", Str("a"), Opt("TYPE", Word("HADOOP")))]),
		// A setting is read with a value.
		["a data source setting with no value"] = new Statement.CreateExternalDataSource(X, [Opt("LOCATION")]),
		// A data source's settings have no partitions, order or place.
		["a data source setting with partitions"] = new Statement.CreateExternalDataSource(X, [new Option(new Identifier("LOCATION"), Str("a"), []) { Partitions = [new PartitionRange(Digits("1"), null)] }]),
		// DataSourceChange reads TYPE as BLOB_STORAGE or HADOOP only.
		["an altered data source type RDBMS"] = new Statement.AlterExternalDataSource(X, [Opt("TYPE", Word("RDBMS"))]),
		// DataSourceChangeSyntax has no DATABASE_NAME, which only a data source made reads.
		["an altered data source setting it has not"] = new Statement.AlterExternalDataSource(X, [Opt("DATABASE_NAME", Str("d"))]),
		// `ALTER … SET` reads one change at least.
		["an altered data source of no setting"] = new Statement.AlterExternalDataSource(X, []),
		// DataSourceBody reads each name once, altering included.
		["an altered data source setting twice"] = new Statement.AlterExternalDataSource(X, [Opt("LOCATION", Str("a")), Opt("LOCATION", Str("b"))]),
		// FileFormatOption reads six types.
		["a file format type outside its words"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_TYPE", Word("CSV"))]),
		// FileFormatOption's FORMAT_TYPE is a word, not a string.
		["a file format type that is a string"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_TYPE", Str("ORC"))]),
		// FileFormatOption does not read a data source's setting.
		["a file format setting it has not"] = new Statement.CreateExternalFileFormat(X, [Opt("LOCATION", Str("a"))]),
		// `FORMAT_OPTIONS = (…)` is Msg 102: the brackets follow the word with no =.
		["a file format option list set with ="] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", Str("a"))]),
		["a file format option list with its = omitted"] = new Statement.CreateExternalFileFormat(X, [new Option(new Identifier("FORMAT_OPTIONS"), null, []) { EqualsOmitted = true }]),
		// FormatOption reads delimited text's settings only.
		["a format option it has not"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("FORMAT_TYPE", Word("ORC")))]),
		// FormatOption holds no list of its own: FORMAT_OPTIONS does not nest.
		["a format option list in a format option list"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("FORMAT_OPTIONS"))]),
		// FIRST_ROW is Sql92.Lexical.Digits.
		["a first row that is a string"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("FIRST_ROW", Str("2")))]),
		["a first row that is signed"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("FIRST_ROW", new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger)))]),
		["a first row that is a decimal"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("FIRST_ROW", new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal)))]),
		["a first row of a kind its text is not"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("FIRST_ROW", new LiteralValue.Numeric("2", NumericLiteralKind.Decimal)))]),
		// USE_TYPE_DEFAULT is TRUE or FALSE: not ON, not 1, not 'TRUE'.
		["a use type default that is ON"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("USE_TYPE_DEFAULT", Word("ON")))]),
		["a use type default that is a string"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("USE_TYPE_DEFAULT", Str("TRUE")))]),
		// FIELD_TERMINATOR is Quoted.
		["a field terminator that is a name"] = new Statement.CreateExternalFileFormat(X, [Opt("FORMAT_OPTIONS", null, Opt("FIELD_TERMINATOR", Word("a")))]),
		// A file format is named by an Identifier.
		["a file format named by a variable"] = new Statement.CreateExternalFileFormat(new Identifier("@f"), []),
		// An external table is named by a QualifiedName, whose parts are Identifiers: `@t` is a variable.
		["an external table named by a variable"] = new Statement.CreateExternalTable(Dotted("@t"), Elements()),
		["an external table named by a reserved word"] = new Statement.CreateExternalTable(Dotted("dbo", "table"), Elements()),
		["an external table named in an unclosed bracket"] = new Statement.CreateExternalTable(new QualifiedName([new Identifier("[t", IdentifierStyle.Bracketed)]), Elements()),
		["an external table named by two words"] = new Statement.CreateExternalTable(Dotted("a b"), Elements()),
		// NamePartName leaves a part out only before a dot: the name ends in a name.
		["an external table whose name ends in a left out part"] = new Statement.CreateExternalTable(Dotted("db", ""), Elements()),
		["an external table named by a string"] = new Statement.CreateExternalTable(new QualifiedName([new Identifier("t", IdentifierStyle.String)]), Elements()),
		// TableBody reads one element at least.
		["an external table of no column"] = new Statement.CreateExternalTable(Dotted("t"), new TableContents.Elements([])),
		// The external table reads columns, or a query: no typed table.
		["an external table of a typed table"] = new Statement.CreateExternalTable(Dotted("t"), new TableContents.Typed(Dotted("ty"), null, [])),
		// The query's form has no column list before AS.
		["an external table made of a query with columns"] = new Statement.CreateExternalTable(Dotted("t"), new TableContents.AsQuery([new Identifier("a")], Select("SELECT 1 AS a"), null)),
		// Synapse's query writes no WITH [NO] DATA.
		["an external table made of a query with its data"] = new Statement.CreateExternalTable(Dotted("t"), new TableContents.AsQuery([], Select("SELECT 1 AS a"), WithDataMode.WithData)),
		// ExternalTableItem lists a name once (`Syntax.NamedOnce`, Msg 46547).
		["an external table setting twice"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("LOCATION", Str("a")), Opt("LOCATION", Str("b"))] },
		// ExternalTableSyntax has no FORMAT_TYPE.
		["an external table setting it has not"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("FORMAT_TYPE", Word("ORC"))] },
		["an external table setting with its = omitted"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [new Option(new Identifier("LOCATION"), Str("a"), []) { EqualsOmitted = true }] },
		// LOCATION is Quoted.
		["an external table location that is a name"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("LOCATION", Word("a"))] },
		// DATA_SOURCE is ExternalName: one part, not a string, not `dbo.x`.
		["an external table data source that is a string"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DATA_SOURCE", Str("ds"))] },
		["an external table data source of two parts"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DATA_SOURCE", new Expression.Reference(Dotted("dbo", "ds")))] },
		// The expression rule reads the plain name VALUE as the current value: the tree for the name would read back as the other.
		["an external table data source that is the plain name VALUE"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DATA_SOURCE", Word("value"))] },
		["an external table file format that is a variable"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("FILE_FORMAT", new Expression.Variable(new Identifier("@f")))] },
		// REJECT_TYPE is VALUE or PERCENTAGE; the word VALUE is the current value to the expression rule, so the plain name VALUE is none.
		["an external table reject type that is the plain name VALUE"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("REJECT_TYPE", Word("VALUE"))] },
		["an external table reject type outside its words"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("REJECT_TYPE", Word("ROWS"))] },
		["an external table reject type that is another current value"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("REJECT_TYPE", new Expression.Current(CurrentValue.User))] },
		// ExactNumber is an unsigned numeric literal with no exponent (`ExternalTableSyntax`).
		["an external table reject value with an exponent"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("REJECT_VALUE", new LiteralValue.Numeric("1e3", NumericLiteralKind.Approximate))] },
		["an external table reject value that is signed"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("REJECT_VALUE", new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger))] },
		["an external table reject value that is a minus over a number"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("REJECT_VALUE", new Expression.Unary(UnaryOperator.Minus, Digits("1")))] },
		["an external table reject value that is a string"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("REJECT_VALUE", Str("1"))] },
		["an external table reject sample value that is hex"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("REJECT_SAMPLE_VALUE", new LiteralValue.Numeric("0x10", NumericLiteralKind.HexInteger))] },
		// DISTRIBUTION is SHARDED (Identifier), REPLICATED or ROUND_ROBIN.
		["an external table distribution outside its words"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", Call("HASH", Word("a")))] },
		["an external table sharded by two columns"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", Call("SHARDED", Word("a"), Word("b")))] },
		["an external table sharded by nothing"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", Call("SHARDED"))] },
		["an external table sharded by a string"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", Call("SHARDED", Str("a")))] },
		["an external table sharded by a column of two parts"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", Call("SHARDED", new Expression.Reference(Dotted("t", "a"))))] },
		["an external table sharded by the plain name VALUE"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", Call("SHARDED", Word("value")))] },
		["an external table sharded by a named argument"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", new Expression.Invocation(new QualifiedName([new Identifier("SHARDED")]), [new Argument(Word("a"), new Identifier("n"), true)]))] },
		["an external table sharded without brackets"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", new Expression.Invocation(new QualifiedName([new Identifier("SHARDED")]), [new Argument(Word("a"))]) { WithoutParentheses = true })] },
		["an external table sharded by a call with a quantifier"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", new Expression.Invocation(new QualifiedName([new Identifier("SHARDED")]), [new Argument(Word("a"))]) { Quantifier = SetQuantifier.Distinct })] },
		["an external table sharded by a qualified call"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", new Expression.Invocation(Dotted("dbo", "SHARDED"), [new Argument(Word("a"))]))] },
		// A distribution is a word or a call: a number is neither.
		["an external table distribution that is a number"] = new Statement.CreateExternalTable(Dotted("t"), Elements()) { Options = [Opt("DISTRIBUTION", Digits("1"))] },
	};

	static TableContents Elements()
	{
		return new TableContents.Elements([Column("a", new DataType.Numeric(NumericTypeKind.Int))]);
	}

	public static TheoryData<string> RefusalNames
	{
		get { return [.. Refusals.Keys]; }
	}

	[Theory]
	[MemberData(nameof(RefusalNames))]
	public void A_statement_with_no_text_is_refused(string which)
	{
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(Refusals[which]));
	}
}
