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
/// The Server family's writer (Sql2023Writer.TransactSql.Server.cs): each record written as the words of its kind, read back by
/// TransactSqlParser to the same tree, and every tree whose text would read back as another tree or not at all refused.
/// </summary>
public sealed class ServerWriterTests
{
	static readonly Identifier X = new("x");

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

	static Expression Var(string name)
	{
		return new Expression.Variable(new Identifier(name));
	}

	static LiteralValue.Numeric Digits(string digits)
	{
		return new LiteralValue.Numeric(digits, NumericLiteralKind.DecimalInteger);
	}

	static LiteralValue.Numeric Number(string text, NumericLiteralKind kind)
	{
		return new LiteralValue.Numeric(text, kind);
	}

	static QualifiedName Dotted(params string[] parts)
	{
		return new QualifiedName([.. parts.Select(static one => new Identifier(one))]);
	}

	static Option Opt(string name, Expression? value, params Option[] options)
	{
		return new Option(new Identifier(name), value, [.. options]);
	}

	static Option Words(string name, string[] words, Expression? value = null, params Option[] options)
	{
		return new Option(new Identifier(name), value, [.. options]) { Words = [.. words.Select(static one => new Identifier(one))] };
	}

	static Expression Measured(string amount, string unit)
	{
		return new Expression.Measured(Digits(amount), new Identifier(unit));
	}

	static ExecuteArgument Arg(Expression value, string? name = null)
	{
		return new ExecuteArgument(value) { Name = name is null ? null : new Identifier(name) };
	}

	/// <summary>The trees whose text is checked, with the text the grammar reads them from.</summary>
	static readonly Dictionary<string, (ISqlNode Tree, string Text)> Texts = new(StringComparer.Ordinal)
	{
		["Kill a session"] = (
			new Statement.Kill(Digits("53")),
			"KILL 53"),
		["Kill a session by a negative number"] = (
			new Statement.Kill(Number("-1", NumericLiteralKind.DecimalInteger)) { With = KillOption.StatusOnly },
			"KILL -1 WITH STATUSONLY"),
		["Kill a unit of work"] = (
			new Statement.Kill(NStr("8C8A")) { With = KillOption.Rollback },
			"KILL N'8C8A' WITH ROLLBACK"),
		["Kill a unit of work and commit it"] = (
			new Statement.Kill(Str("8C8A")) { With = KillOption.Commit },
			"KILL '8C8A' WITH COMMIT"),
		["Kill a statistics job"] = (
			new Statement.KillStatsJob(Number("-12", NumericLiteralKind.DecimalInteger)),
			"KILL STATS JOB -12"),
		["Kill every subscription"] = (
			new Statement.KillQueryNotificationSubscription { All = true },
			"KILL QUERY NOTIFICATION SUBSCRIPTION ALL"),
		["Kill a subscription"] = (
			new Statement.KillQueryNotificationSubscription { Subscription = Digits("73") },
			"KILL QUERY NOTIFICATION SUBSCRIPTION 73"),
		["Reconfigure"] = (
			new Statement.Reconfigure(false),
			"RECONFIGURE"),
		["Reconfigure with override"] = (
			new Statement.Reconfigure(true),
			"RECONFIGURE WITH OVERRIDE"),
		["Shutdown"] = (
			new Statement.Shutdown(false),
			"SHUTDOWN"),
		["Shutdown with no wait"] = (
			new Statement.Shutdown(true),
			"SHUTDOWN WITH NOWAIT"),

		["Dbcc"] = (
			new Statement.Dbcc(new Identifier("CHECKDB")),
			"DBCC CHECKDB"),
		["Dbcc with empty brackets"] = (
			new Statement.Dbcc(new Identifier("CHECKDB")) { Arguments = [] },
			"DBCC CHECKDB ()"),
		["Dbcc with its arguments"] = (
			new Statement.Dbcc(new Identifier("foo")) { Arguments = [Arg(Digits("1")), Arg(Number("-2", NumericLiteralKind.DecimalInteger)), Arg(Number("1.", NumericLiteralKind.Decimal)), Arg(Number(".5", NumericLiteralKind.Decimal)), Arg(Number("1e1", NumericLiteralKind.Approximate)), Arg(Number("$3", NumericLiteralKind.Money)), Arg(Number("£-4.5", NumericLiteralKind.Money)), Arg(Str("s")), Arg(NStr("n")), Arg(new LiteralValue.Binary("0600") { ZeroX = true }), Arg(new LiteralValue.Null()), Arg(Var("@@v")), Arg(Word("NEXT")), Arg(Word("#t")), Arg(new Expression.Reference(new QualifiedName([new Identifier("[default]", IdentifierStyle.Bracketed)])))] },
			"DBCC foo (1, -2, 1., .5, 1e1, $3, £-4.5, 's', N'n', 0x0600, NULL, @@v, NEXT, #t, [default])"),
		["Dbcc with named arguments"] = (
			new Statement.Dbcc(new Identifier("foo")) { Arguments = [Arg(Str("x"), "a"), Arg(Digits("1"), "@x"), Arg(Word("c"), "b")] },
			"DBCC foo (a = 'x', @x = 1, b = c)"),
		["Dbcc with words"] = (
			new Statement.Dbcc(new Identifier("CHECKDB")) { Arguments = [Arg(Word("db"))], Options = [Opt("NO_INFOMSGS", null), Opt("NO_INFOMSGS", null), Opt("tablock", null)] },
			"DBCC CHECKDB (db) WITH NO_INFOMSGS, NO_INFOMSGS, tablock"),
		["Dbcc with words joined"] = (
			new Statement.Dbcc(new Identifier("SHOW_STATISTICS")) { Arguments = [Arg(Word("t")), Arg(Word("i"))], Options = [Words("STAT_HEADER", ["JOIN", "DENSITY_VECTOR", "JOIN", "STAT_HEADER"]), Opt("NO_INFOMSGS", null)] },
			"DBCC SHOW_STATISTICS (t, i) WITH STAT_HEADER JOIN DENSITY_VECTOR JOIN STAT_HEADER, NO_INFOMSGS"),
		["Dbcc with maxdop"] = (
			new Statement.Dbcc(new Identifier("CHECKDB")) { Options = [Opt("MAXDOP", Digits("4")), Opt("maxdop", Number(".5", NumericLiteralKind.Decimal)), Opt("MAXDOP", null)] },
			"DBCC CHECKDB WITH MAXDOP = 4, maxdop = .5, MAXDOP"),
		["Dbcc with a low priority wait"] = (
			new Statement.Dbcc(new Identifier("FOO")) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null), Opt("WAIT_AT_LOW_PRIORITY", null, Opt("MAX_DURATION", Measured("1", "MINUTES")), Opt("ABORT_AFTER_WAIT", Word("SELF")))] },
			"DBCC FOO WITH WAIT_AT_LOW_PRIORITY, WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = SELF)"),
		["Dbcc with a low priority wait of a bare duration"] = (
			new Statement.Dbcc(new Identifier("FOO")) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("ABORT_AFTER_WAIT", Word("foo")), Opt("MAX_DURATION", Digits("5")))] },
			"DBCC FOO WITH WAIT_AT_LOW_PRIORITY (ABORT_AFTER_WAIT = foo, MAX_DURATION = 5)"),
		// @@TEXTS
	};

	public static TheoryData<string> TreeNames
	{
		get { return [.. Texts.Keys]; }
	}

	/// <summary>Each tree written as the words of its kind.</summary>
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
		// KillNumber is '-'? and an unsigned numeric literal, one token; KillSession is that or BrokerString.
		["a kill of a session that is a name"] = new Statement.Kill(Word("s")),
		["a kill of a session that is a variable"] = new Statement.Kill(Var("@s")),
		["a kill of a session that is a plus over a number"] = new Statement.Kill(Number("+1", NumericLiteralKind.DecimalInteger)),
		["a kill of a session that is signed twice"] = new Statement.Kill(Number("--1", NumericLiteralKind.DecimalInteger)),
		["a kill of a session that is a minus over a number"] = new Statement.Kill(new Expression.Unary(UnaryOperator.Minus, Digits("1"))),
		["a kill of a session that is hexadecimal"] = new Statement.Kill(Number("0x10", NumericLiteralKind.HexInteger)),
		["a kill of a session that is money"] = new Statement.Kill(Number("$1", NumericLiteralKind.Money)),
		["a kill of a session that is a sum"] = new Statement.Kill(new Expression.Binary(Digits("1"), BinaryOperator.Add, Digits("2"))),
		["a kill of a session with a character set"] = new Statement.Kill(new LiteralValue.String("'x'", StringLiteralKind.Character) { CharacterSet = new CharacterSetName(Dotted("utf8")) }),
		["a kill with an option that is none"] = new Statement.Kill(Digits("1")) { With = (KillOption)9 },
		["a statistics job that is a string"] = new Statement.KillStatsJob(new LiteralValue.Numeric("'1'", NumericLiteralKind.DecimalInteger)),
		["a statistics job that is a plus"] = new Statement.KillStatsJob(Number("+1", NumericLiteralKind.DecimalInteger)),
		["a statistics job of a kind its text is not"] = new Statement.KillStatsJob(Number("1.5", NumericLiteralKind.DecimalInteger)),
		// KillSubscription is ALL or a KillNumber, one of the two.
		["a subscription kill of every subscription and one"] = new Statement.KillQueryNotificationSubscription { All = true, Subscription = Digits("1") },
		["a subscription kill of none"] = new Statement.KillQueryNotificationSubscription(),
		["a subscription kill of a number that is a string"] = new Statement.KillQueryNotificationSubscription { Subscription = Number("'1'", NumericLiteralKind.DecimalInteger) },

		// DbccWord is a plain word: not in brackets or quotes, not a qualified name, not a variable, not reserved.
		["a dbcc command in brackets"] = new Statement.Dbcc(new Identifier("[CHECKDB]", IdentifierStyle.Bracketed)),
		["a dbcc command in quotes"] = new Statement.Dbcc(new Identifier("\"CHECKDB\"", IdentifierStyle.Delimited)),
		["a dbcc command that is a variable"] = new Statement.Dbcc(new Identifier("@c")),
		["a dbcc command that is a reserved word"] = new Statement.Dbcc(new Identifier("select")),
		["a dbcc command of two words"] = new Statement.Dbcc(new Identifier("a b")),
		["a dbcc command that is a string"] = new Statement.Dbcc(new Identifier("c", IdentifierStyle.String)),
		// DbccValue: no expression, no DEFAULT (Msg 1009), no name of two parts, no call.
		["a dbcc argument that is DEFAULT"] = new Statement.Dbcc(X) { Arguments = [Arg(new Expression.Default())] },
		["a dbcc argument that is a sum"] = new Statement.Dbcc(X) { Arguments = [Arg(new Expression.Binary(Digits("1"), BinaryOperator.Add, Digits("2")))] },
		["a dbcc argument that is a call"] = new Statement.Dbcc(X) { Arguments = [Arg(new Expression.Invocation(new QualifiedName([new Identifier("f")]), []))] },
		["a dbcc argument that is a name of two parts"] = new Statement.Dbcc(X) { Arguments = [Arg(new Expression.Reference(Dotted("a", "b")))] },
		["a dbcc argument that is a reserved word"] = new Statement.Dbcc(X) { Arguments = [Arg(Word("select"))] },
		["a dbcc argument that is a name with a blank"] = new Statement.Dbcc(X) { Arguments = [Arg(Word("a b"))] },
		["a dbcc argument that is a variable of two words"] = new Statement.Dbcc(X) { Arguments = [Arg(Var("@a b"))] },
		["a dbcc argument that is a minus over a number"] = new Statement.Dbcc(X) { Arguments = [Arg(new Expression.Unary(UnaryOperator.Minus, Digits("1")))] },
		["a dbcc argument that is a plus"] = new Statement.Dbcc(X) { Arguments = [Arg(Number("+1", NumericLiteralKind.DecimalInteger))] },
		["a dbcc argument that is signed money"] = new Statement.Dbcc(X) { Arguments = [Arg(Number("-$3", NumericLiteralKind.Money))] },
		["a dbcc argument that is money of a currency the engine refuses"] = new Statement.Dbcc(X) { Arguments = [Arg(Number("₿3", NumericLiteralKind.Money))] },
		["a dbcc argument that is money with a point first"] = new Statement.Dbcc(X) { Arguments = [Arg(Number("$.5", NumericLiteralKind.Money))] },
		["a dbcc argument in brackets"] = new Statement.Dbcc(X) { Arguments = [Arg(new Expression.Parenthesized(Digits("1")))] },
		["a dbcc argument that is a string with a character set"] = new Statement.Dbcc(X) { Arguments = [Arg(new LiteralValue.String("'x'", StringLiteralKind.Character) { CharacterSet = new CharacterSetName(Dotted("utf8")) })] },
		["a dbcc argument that is a standard binary string"] = new Statement.Dbcc(X) { Arguments = [Arg(new LiteralValue.Binary("'10'"))] },
		["a dbcc argument passed back"] = new Statement.Dbcc(X) { Arguments = [new ExecuteArgument(Var("@v")) { Output = OutputKeyword.Output }] },
		["a dbcc argument named in brackets"] = new Statement.Dbcc(X) { Arguments = [new ExecuteArgument(Digits("1")) { Name = new Identifier("[a]", IdentifierStyle.Bracketed) }] },
		["a dbcc argument named by a reserved word"] = new Statement.Dbcc(X) { Arguments = [Arg(Digits("1"), "select")] },
		["a dbcc argument named by a variable of two words"] = new Statement.Dbcc(X) { Arguments = [Arg(Digits("1"), "@a b")] },
		// DbccOption: words; MAXDOP takes a number with no sign and no exponent; the bracket is WAIT_AT_LOW_PRIORITY's alone.
		["a dbcc option that is in brackets"] = new Statement.Dbcc(X) { Options = [new Option(new Identifier("[A]", IdentifierStyle.Bracketed), null, [])] },
		["a dbcc option that is a reserved word"] = new Statement.Dbcc(X) { Options = [Opt("select", null)] },
		["a dbcc option that has a value"] = new Statement.Dbcc(X) { Options = [Opt("A", Digits("1"))] },
		["a dbcc option that omits its ="] = new Statement.Dbcc(X) { Options = [new Option(new Identifier("MAXDOP"), Digits("1"), []) { EqualsOmitted = true }] },
		["a dbcc maxdop that is signed"] = new Statement.Dbcc(X) { Options = [Opt("MAXDOP", Number("-1", NumericLiteralKind.DecimalInteger))] },
		["a dbcc maxdop with an exponent"] = new Statement.Dbcc(X) { Options = [Opt("MAXDOP", Number("1e1", NumericLiteralKind.Approximate))] },
		["a dbcc maxdop that is a name"] = new Statement.Dbcc(X) { Options = [Opt("MAXDOP", Word("a"))] },
		["a dbcc maxdop with a bracket"] = new Statement.Dbcc(X) { Options = [Opt("MAXDOP", Digits("1"), Opt("A", null))] },
		["a dbcc word with a bracket"] = new Statement.Dbcc(X) { Options = [Opt("A", null, Opt("B", null))] },
		["a dbcc wait of no setting, bracketed"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", Digits("1"))] },
		["a dbcc wait with a setting it has not"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("MAX_DURATION_X", Digits("1")))] },
		["a dbcc wait with its duration twice"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("MAX_DURATION", Digits("1")), Opt("MAX_DURATION", Digits("2")))] },
		["a dbcc wait with a duration that is a name"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("MAX_DURATION", Word("a")))] },
		["a dbcc wait with a duration in seconds"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("MAX_DURATION", Measured("1", "SECONDS")))] },
		["a dbcc wait with a duration that is signed"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("MAX_DURATION", Number("-1", NumericLiteralKind.DecimalInteger)))] },
		["a dbcc wait with an abort that is a string"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("ABORT_AFTER_WAIT", Str("a")))] },
		["a dbcc wait with an abort in brackets"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("ABORT_AFTER_WAIT", new Expression.Reference(new QualifiedName([new Identifier("[a]", IdentifierStyle.Bracketed)]))))] },
		["a dbcc wait with a setting of no value"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, Opt("MAX_DURATION", null))] },
		// Only the first option reads words joined by JOIN (`WITH NO_INFOMSGS, STAT_HEADER JOIN DENSITY_VECTOR` is Msg 156).
		["a dbcc join after the first option"] = new Statement.Dbcc(X) { Options = [Opt("A", null), Words("STAT_HEADER", ["JOIN", "DENSITY_VECTOR"])] },
		["a dbcc join of an odd number of words"] = new Statement.Dbcc(X) { Options = [Words("A", ["JOIN"])] },
		["a dbcc join by another word"] = new Statement.Dbcc(X) { Options = [Words("A", ["AND", "B"])] },
		["a dbcc join of a word that is a variable"] = new Statement.Dbcc(X) { Options = [Words("A", ["JOIN", "@b"])] },
		["a dbcc join with a value"] = new Statement.Dbcc(X) { Options = [Words("A", ["JOIN", "B"], Digits("1"))] },
		// @@REFUSALS
	};

	public static TheoryData<string> RefusalNames
	{
		get { return [.. Refusals.Keys]; }
	}

	[Theory]
	[MemberData(nameof(RefusalNames))]
	public void A_statement_with_no_text_is_refused(string which)
	{
		var thrown = Assert.ThrowsAny<Exception>(() => TransactSqlWriter.Write(Refusals[which]));

		Assert.True(thrown is ArgumentException or NotSupportedException, thrown.ToString());
	}
}
