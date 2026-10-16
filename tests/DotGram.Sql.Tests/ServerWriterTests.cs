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

	static Statement.AlterServerConfiguration Configured(Option setting)
	{
		return new Statement.AlterServerConfiguration(setting);
	}

	static Expression Ranges(params (string From, string? To)[] ranges)
	{
		return new Expression.ServerRanges([.. ranges.Select(static one => new PartitionRange(Digits(one.From), one.To is null ? null : Digits(one.To)))]);
	}

	static Expression Names(params string[] names)
	{
		return new Expression.Row([.. names.Select(static one => Word(one))]);
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
		["Configure the affinity of processors"] = (
			Configured(Words("PROCESS", ["AFFINITY", "CPU"], Ranges(("1", null), ("3", "5"), ("8", null)))),
			"ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = 1, 3 TO 5, 8"),
		["Configure the affinity of processors, automatic"] = (
			Configured(Words("process", ["affinity", "cpu"], Word("auto"))),
			"ALTER SERVER CONFIGURATION SET process affinity cpu = auto"),
		["Configure the affinity of nodes"] = (
			Configured(Words("PROCESS", ["AFFINITY", "NUMANODE"], Ranges(("3", "1")))),
			"ALTER SERVER CONFIGURATION SET PROCESS AFFINITY NUMANODE = 3 TO 1"),
		["Configure the diagnostics log"] = (
			Configured(Words("DIAGNOSTICS", ["LOG"], Word("ON"))),
			"ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG ON"),
		["Configure the path of the diagnostics log"] = (
			Configured(Words("DIAGNOSTICS", ["LOG", "PATH"], NStr("C:\\logs"))),
			"ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG PATH = N'C:\\logs'"),
		["Configure the default path of the diagnostics log"] = (
			Configured(Words("DIAGNOSTICS", ["LOG", "PATH"], Word("DEFAULT"))),
			"ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG PATH = DEFAULT"),
		["Configure the size of the diagnostics log"] = (
			Configured(Words("DIAGNOSTICS", ["LOG", "MAX_SIZE"], Measured("10", "mb"))),
			"ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_SIZE = 10 mb"),
		["Configure the files of the diagnostics log"] = (
			Configured(Words("DIAGNOSTICS", ["LOG", "MAX_FILES"], Digits("10"))),
			"ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_FILES = 10"),
		["Configure the default files of the diagnostics log"] = (
			Configured(Words("DIAGNOSTICS", ["LOG", "MAX_FILES"], Word("DEFAULT"))),
			"ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_FILES = DEFAULT"),
		["Configure a cluster's path"] = (
			Configured(Words("FAILOVER", ["CLUSTER", "PROPERTY", "SqlDumperDumpPath"], Str("C:"))),
			"ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY SqlDumperDumpPath = 'C:'"),
		["Configure a cluster's number"] = (
			Configured(Words("FAILOVER", ["CLUSTER", "PROPERTY", "verboselogging"], Digits("2"))),
			"ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY verboselogging = 2"),
		["Configure a cluster's default"] = (
			Configured(Words("FAILOVER", ["CLUSTER", "PROPERTY", "HealthCheckTimeout"], Word("DEFAULT"))),
			"ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY HealthCheckTimeout = DEFAULT"),
		["Configure a cluster's context"] = (
			Configured(Words("HADR", ["CLUSTER", "CONTEXT"], Word("LOCAL"))),
			"ALTER SERVER CONFIGURATION SET HADR CLUSTER CONTEXT = LOCAL"),
		["Configure a cluster's context by a string"] = (
			Configured(Words("HADR", ["CLUSTER", "CONTEXT"], NStr("clus01"))),
			"ALTER SERVER CONFIGURATION SET HADR CLUSTER CONTEXT = N'clus01'"),
		["Configure the buffer pool extension off"] = (
			Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("OFF"))),
			"ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION OFF"),
		["Configure the buffer pool extension on"] = (
			Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("ON"), Opt("FILENAME", NStr("F:\\x.BPE")), Opt("SIZE", Measured("50", "GB")))),
			"ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = N'F:\\x.BPE', SIZE = 50 GB)"),
		["Configure soft NUMA"] = (
			Configured(Opt("SOFTNUMA", Word("off"))),
			"ALTER SERVER CONFIGURATION SET SOFTNUMA off"),
		["Configure memory-optimized"] = (
			Configured(Opt("MEMORY_OPTIMIZED", Word("ON"))),
			"ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED ON"),
		["Configure tempdb metadata"] = (
			Configured(Words("MEMORY_OPTIMIZED", ["TEMPDB_METADATA"], Word("ON"), Opt("RESOURCE_POOL", Str("p")))),
			"ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED TEMPDB_METADATA = ON (RESOURCE_POOL = 'p')"),
		["Configure tempdb metadata off"] = (
			Configured(Words("MEMORY_OPTIMIZED", ["TEMPDB_METADATA"], Word("OFF"))),
			"ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED TEMPDB_METADATA = OFF"),
		["Configure the hybrid buffer pool"] = (
			Configured(Words("MEMORY_OPTIMIZED", ["HYBRID_BUFFER_POOL"], Word("OFF"))),
			"ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED HYBRID_BUFFER_POOL = OFF"),
		["Configure a snapshot backup"] = (
			Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("OFF"))),
			"ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = OFF"),
		["Configure a snapshot backup of a group and a mode"] = (
			Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", Names("db1", "db2")), Opt("MODE", Word("COPY_ONLY")))),
			"ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (GROUP = (db1, db2), MODE = COPY_ONLY)"),
		["Configure a snapshot backup of a mode"] = (
			Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("MODE", Word("COPY_ONLY")))),
			"ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (MODE = COPY_ONLY)"),
		["Configure a snapshot backup of a group"] = (
			Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("OFF"), Opt("GROUP", new Expression.Row([new Expression.Reference(new QualifiedName([new Identifier("[db 1]", IdentifierStyle.Bracketed)]))])))),
			"ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = OFF (GROUP = ([db 1]))"),
		["Configure external authentication off"] = (
			Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("OFF"))),
			"ALTER SERVER CONFIGURATION SET EXTERNAL AUTHENTICATION OFF"),
		["Configure external authentication by the identity"] = (
			Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("ON"), Opt("USE_IDENTITY", null))),
			"ALTER SERVER CONFIGURATION SET EXTERNAL AUTHENTICATION ON (USE_IDENTITY)"),
		["Configure external authentication by a credential"] = (
			Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("ON"), Opt("CREDENTIAL_NAME", NStr("c")))),
			"ALTER SERVER CONFIGURATION SET EXTERNAL AUTHENTICATION ON (CREDENTIAL_NAME = N'c')"),
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
		// ServerSetting is a closed catalogue: a name outside it, and a form other than the setting's own.
		["a configuration of a setting it has not"] = Configured(Opt("SOFTNUMAX", Word("ON"))),
		["a configuration named in brackets"] = Configured(new Option(new Identifier("[SOFTNUMA]", IdentifierStyle.Bracketed), Word("ON"), [])),
		["a configuration that omits its ="] = Configured(new Option(new Identifier("MEMORY_OPTIMIZED"), Word("ON"), []) { EqualsOmitted = true }),
		["a configuration of HARDWARE_OFFLOAD, which the engine refuses"] = Configured(Opt("HARDWARE_OFFLOAD", Word("ON"))),
		["a configuration with a place"] = Configured(new Option(new Identifier("SOFTNUMA"), Word("ON"), []) { Place = new Placement(PlacementKind.MoveTo, new Identifier("fg"), null) }),
		["a configuration with no value"] = Configured(Opt("SOFTNUMA", null)),
		["a configuration of a property that is none"] = Configured(Words("FAILOVER", ["CLUSTER", "PROPERTY", "Colour"], Digits("1"))),
		["a configuration of a cluster with the property missing"] = Configured(Words("FAILOVER", ["CLUSTER", "PROPERTY"], Digits("1"))),
		["a configuration of a word too many"] = Configured(Words("SOFTNUMA", ["NOW"], Word("ON"))),
		["a configuration of a word in brackets"] = Configured(new Option(new Identifier("DIAGNOSTICS"), Word("ON"), []) { Words = [new Identifier("[LOG]", IdentifierStyle.Bracketed)] }),
		// A switch is ON or OFF.
		["a configuration switch that is a number"] = Configured(Opt("SOFTNUMA", Digits("1"))),
		["a configuration switch that is a string"] = Configured(Opt("SOFTNUMA", Str("ON"))),
		["a configuration switch that is in brackets"] = Configured(Opt("SOFTNUMA", new Expression.Reference(new QualifiedName([new Identifier("[ON]", IdentifierStyle.Bracketed)])))),
		["a configuration switch that has a bracket"] = Configured(Opt("SOFTNUMA", Word("ON"), Opt("MODE", Word("COPY_ONLY")))),
		["a configuration switch that is DEFAULT"] = Configured(Opt("MEMORY_OPTIMIZED", Word("DEFAULT"))),
		// ServerRanges: whole numbers, with TO between two, and AUTO for processors only.
		["an affinity of nodes that is AUTO"] = Configured(Words("PROCESS", ["AFFINITY", "NUMANODE"], Word("AUTO"))),
		["an affinity of no range"] = Configured(Words("PROCESS", ["AFFINITY", "CPU"], new Expression.ServerRanges([]))),
		["an affinity of a range that is signed"] = Configured(Words("PROCESS", ["AFFINITY", "CPU"], new Expression.ServerRanges([new PartitionRange(Number("-1", NumericLiteralKind.DecimalInteger), null)]))),
		["an affinity of a range that is a decimal"] = Configured(Words("PROCESS", ["AFFINITY", "CPU"], new Expression.ServerRanges([new PartitionRange(Number("1.5", NumericLiteralKind.Decimal), null)]))),
		["an affinity of a range ending in a name"] = Configured(Words("PROCESS", ["AFFINITY", "CPU"], new Expression.ServerRanges([new PartitionRange(Digits("1"), Word("x"))]))),
		["an affinity of a range from a variable"] = Configured(Words("PROCESS", ["AFFINITY", "CPU"], new Expression.ServerRanges([new PartitionRange(Var("@x"), null)]))),
		["an affinity of the brackets of a pool"] = Configured(Words("PROCESS", ["AFFINITY", "CPU"], new Expression.PoolRanges([new PartitionRange(Digits("1"), null)]))),
		["an affinity of a string"] = Configured(Words("PROCESS", ["AFFINITY", "CPU"], Str("AUTO"))),
		["an affinity that is a number"] = Configured(Words("PROCESS", ["AFFINITY", "CPU"], Digits("1"))),
		// The log: a path is a string or DEFAULT, a size whole megabytes, a count whole.
		["a log path that is a name"] = Configured(Words("DIAGNOSTICS", ["LOG", "PATH"], Word("p"))),
		["a log path that is a string with a character set"] = Configured(Words("DIAGNOSTICS", ["LOG", "PATH"], new LiteralValue.String("'p'", StringLiteralKind.Character) { CharacterSet = new CharacterSetName(Dotted("utf8")) })),
		["a log size in kilobytes"] = Configured(Words("DIAGNOSTICS", ["LOG", "MAX_SIZE"], Measured("10", "KB"))),
		["a log size with no unit"] = Configured(Words("DIAGNOSTICS", ["LOG", "MAX_SIZE"], Digits("10"))),
		["a log size that is a decimal"] = Configured(Words("DIAGNOSTICS", ["LOG", "MAX_SIZE"], new Expression.Measured(Number("1.5", NumericLiteralKind.Decimal), new Identifier("MB")))),
		["a log files count that is signed"] = Configured(Words("DIAGNOSTICS", ["LOG", "MAX_FILES"], Number("-1", NumericLiteralKind.DecimalInteger))),
		["a log files count that is a decimal"] = Configured(Words("DIAGNOSTICS", ["LOG", "MAX_FILES"], Number("1.5", NumericLiteralKind.Decimal))),
		["a log files count that is LOCAL"] = Configured(Words("DIAGNOSTICS", ["LOG", "MAX_FILES"], Word("LOCAL"))),
		// A cluster's property is a string for two and a number for five.
		["a cluster string property that is a number"] = Configured(Words("FAILOVER", ["CLUSTER", "PROPERTY", "SqlDumperDumpPath"], Digits("1"))),
		["a cluster number property that is a string"] = Configured(Words("FAILOVER", ["CLUSTER", "PROPERTY", "VerboseLogging"], Str("1"))),
		["a cluster number property that is signed"] = Configured(Words("FAILOVER", ["CLUSTER", "PROPERTY", "VerboseLogging"], Number("-1", NumericLiteralKind.DecimalInteger))),
		["a cluster context that is DEFAULT"] = Configured(Words("HADR", ["CLUSTER", "CONTEXT"], Word("DEFAULT"))),
		["a cluster context that is a number"] = Configured(Words("HADR", ["CLUSTER", "CONTEXT"], Digits("1"))),
		// The buffer pool extension: OFF, or ON with its file and then its size in a unit.
		["a buffer pool extension on with no bracket"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("ON"))),
		["a buffer pool extension on with its size first"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("ON"), Opt("SIZE", Measured("50", "GB")), Opt("FILENAME", Str("f")))),
		["a buffer pool extension on with its file only"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("ON"), Opt("FILENAME", Str("f")))),
		["a buffer pool extension on with a size in terabytes"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("ON"), Opt("FILENAME", Str("f")), Opt("SIZE", Measured("5", "TB")))),
		["a buffer pool extension on with a size that is a decimal"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("ON"), Opt("FILENAME", Str("f")), Opt("SIZE", new Expression.Measured(Number("1.5", NumericLiteralKind.Decimal), new Identifier("GB"))))),
		["a buffer pool extension on with a file that is a name"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("ON"), Opt("FILENAME", Word("f")), Opt("SIZE", Measured("5", "GB")))),
		["a buffer pool extension off with a bracket"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("OFF"), Opt("FILENAME", Str("f")), Opt("SIZE", Measured("5", "GB")))),
		["a buffer pool extension of a value that is none"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("AUTO"))),
		["a buffer pool extension item with a value of its own"] = Configured(Words("BUFFER", ["POOL", "EXTENSION"], Word("ON"), Opt("FILENAME", Str("f"), Opt("X", null)), Opt("SIZE", Measured("5", "GB")))),
		// Tempdb metadata: OFF, ON, or ON with a resource pool as a string.
		["tempdb metadata off with a pool"] = Configured(Words("MEMORY_OPTIMIZED", ["TEMPDB_METADATA"], Word("OFF"), Opt("RESOURCE_POOL", Str("p")))),
		["tempdb metadata with a pool that is a name"] = Configured(Words("MEMORY_OPTIMIZED", ["TEMPDB_METADATA"], Word("ON"), Opt("RESOURCE_POOL", Word("p")))),
		["tempdb metadata with two items"] = Configured(Words("MEMORY_OPTIMIZED", ["TEMPDB_METADATA"], Word("ON"), Opt("RESOURCE_POOL", Str("p")), Opt("RESOURCE_POOL", Str("q")))),
		["tempdb metadata with an item it has not"] = Configured(Words("MEMORY_OPTIMIZED", ["TEMPDB_METADATA"], Word("ON"), Opt("MODE", Str("p")))),
		["tempdb metadata that omits its ="] = Configured(new Option(new Identifier("MEMORY_OPTIMIZED"), Word("ON"), []) { Words = [new Identifier("TEMPDB_METADATA")], EqualsOmitted = true }),
		// Snapshot backup: ON or OFF, then a group and a mode, or a mode alone.
		["a snapshot backup that is a name"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("MAYBE"))),
		["a snapshot backup of a mode before a group"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("MODE", Word("COPY_ONLY")), Opt("GROUP", Names("a")))),
		["a snapshot backup of a mode that is none"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("MODE", Word("FULL")))),
		["a snapshot backup of a mode with no value"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("MODE", null))),
		["a snapshot backup of a group of no database"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", new Expression.Row([])))),
		["a snapshot backup of a group of a string"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", new Expression.Row([Str("a")])))),
		["a snapshot backup of a group of a name of two parts"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", new Expression.Row([new Expression.Reference(Dotted("a", "b"))])))),
		["a snapshot backup of a group of a name that is reserved"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", Names("select")))),
		["a snapshot backup of a group written ROW"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", new Expression.Row([Word("a")], RowKeyword: true)))),
		["a snapshot backup of a group that is a name"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", Word("a")))),
		["a snapshot backup of a group twice"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", Names("a")), Opt("GROUP", Names("b")))),
		["a snapshot backup of three items"] = Configured(Opt("SUSPEND_FOR_SNAPSHOT_BACKUP", Word("ON"), Opt("GROUP", Names("a")), Opt("MODE", Word("COPY_ONLY")), Opt("MODE", Word("COPY_ONLY")))),
		// External authentication: OFF, or ON with one way in brackets (Msg 102 for both, none, a name, or OFF with a bracket).
		["external authentication on with no way"] = Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("ON"))),
		["external authentication on with both ways"] = Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("ON"), Opt("USE_IDENTITY", null), Opt("CREDENTIAL_NAME", Str("c")))),
		["external authentication off with a way"] = Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("OFF"), Opt("USE_IDENTITY", null))),
		["external authentication by a way that is a name"] = Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("ON"), Opt("CREDENTIAL_NAME", Word("c")))),
		["external authentication by a way it has not"] = Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("ON"), Opt("CERTIFICATE", Str("c")))),
		["external authentication by the identity with a value"] = Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("ON"), Opt("USE_IDENTITY", Str("c")))),
		["external authentication by a credential with no name"] = Configured(Words("EXTERNAL", ["AUTHENTICATION"], Word("ON"), Opt("CREDENTIAL_NAME", null))),
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
