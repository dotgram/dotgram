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
	};

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
