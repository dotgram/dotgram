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

	static Expression PoolRanges(params (string From, string? To)[] ranges)
	{
		return new Expression.PoolRanges([.. ranges.Select(static one => new PartitionRange(Digits(one.From), one.To is null ? null : Digits(one.To)))]);
	}

	static Statement.AlterResourceGovernor Governed(Option setting)
	{
		return new Statement.AlterResourceGovernor(new ResourceGovernorAction.Settings(setting));
	}

	static AvailabilityReplica Replica(string server, params Option[] options)
	{
		return new AvailabilityReplica((LiteralValue.String)Str(server), [.. options]);
	}

	static AvailabilityGroupSpec Group(string name, params Option[] options)
	{
		return new AvailabilityGroupSpec((LiteralValue.String)Str(name), [.. options]);
	}

	static ListenerAddress Address(string address, string? mask = null)
	{
		return new ListenerAddress((LiteralValue.String)Str(address), mask is null ? null : (LiteralValue.String)Str(mask));
	}

	static Statement.AlterAvailabilityGroup Altered(AvailabilityAction action)
	{
		return new Statement.AlterAvailabilityGroup(X, action);
	}

	static Statement.CreateAvailabilityGroup Made(AvailabilitySource source)
	{
		return new Statement.CreateAvailabilityGroup(X, source);
	}

	static AvailabilitySource Replicated(params AvailabilityReplica[] replicas)
	{
		return new AvailabilitySource.Replicas([], [.. replicas]);
	}

	static readonly Option Url = Opt("ENDPOINT_URL", Str("TCP://a:5022"));

	static Expression Row(params Expression[] items)
	{
		return new Expression.Row([.. items]);
	}

	static Expression Run(Func<SqlList<Identifier>, Expression> make, params string[] words)
	{
		return make([.. words.Select(static one => one.StartsWith('[') ? new Identifier(one, IdentifierStyle.Bracketed) : new Identifier(one))]);
	}

	static Expression Encrypted(params string[] words)
	{
		return Run(static one => new Expression.EndpointEncryption(one), words);
	}

	static Expression Authenticated(params string[] words)
	{
		return Run(static one => new Expression.EndpointAuthentication(one), words);
	}

	static Expression IpAddress(string address, string? mask = null)
	{
		return new Expression.EndpointAddress(address, mask);
	}

	static Statement.CreateEndpoint Endpoint(EndpointTransport? transport, EndpointPayload? payload)
	{
		return new Statement.CreateEndpoint(X) { Transport = transport, Payload = payload };
	}

	static EndpointTransport Tcp(params Option[] options)
	{
		return new EndpointTransport.Tcp([.. options]);
	}

	static EndpointTransport Http(params Option[] options)
	{
		return new EndpointTransport.Http([.. options]);
	}

	static EndpointPayload Broker(params Option[] options)
	{
		return new EndpointPayload.ServiceBroker([.. options]);
	}

	static EndpointPayload Soap(params SoapItem[] items)
	{
		return new EndpointPayload.Soap([.. items]);
	}

	static SoapItem Soaped(string name, Expression value)
	{
		return new SoapItem.Setting(Opt(name, value));
	}

	static SoapItem Method(string method, WebMethodAction? action, params Option[] options)
	{
		return new SoapItem.WebMethod((LiteralValue.String)Str(method)) { Action = action, Options = [.. options] };
	}

	static readonly Option Port = Opt("LISTENER_PORT", Digits("4022"));

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
		["Reconfigure the governor"] = (
			new Statement.AlterResourceGovernor(new ResourceGovernorAction.Reconfigure()),
			"ALTER RESOURCE GOVERNOR RECONFIGURE"),
		["Reconfigure the governor twice"] = (
			new Statement.AlterResourceGovernor(new ResourceGovernorAction.Reconfigure()) { ReconfigureAfter = true },
			"ALTER RESOURCE GOVERNOR RECONFIGURE RECONFIGURE"),
		["Disable the governor"] = (
			new Statement.AlterResourceGovernor(new ResourceGovernorAction.Disable()) { ReconfigureAfter = true },
			"ALTER RESOURCE GOVERNOR DISABLE RECONFIGURE"),
		["Reset the statistics of the governor"] = (
			new Statement.AlterResourceGovernor(new ResourceGovernorAction.ResetStatistics()),
			"ALTER RESOURCE GOVERNOR RESET STATISTICS"),
		["Reset the statistics of the governor and reconfigure it"] = (
			new Statement.AlterResourceGovernor(new ResourceGovernorAction.ResetStatistics()) { ReconfigureAfter = true },
			"ALTER RESOURCE GOVERNOR RESET STATISTICS RECONFIGURE"),
		["Name a classifier"] = (
			Governed(Opt("CLASSIFIER_FUNCTION", new Expression.Reference(Dotted("dbo", "f")))),
			"ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.f)"),
		["Name a classifier in quotes"] = (
			Governed(Opt("classifier_function", new Expression.Reference(new QualifiedName([new Identifier("\"dbo\"", IdentifierStyle.Delimited), new Identifier("[f]", IdentifierStyle.Bracketed)])))),
			"ALTER RESOURCE GOVERNOR WITH (classifier_function = \"dbo\".[f])"),
		["Name no classifier"] = (
			Governed(Opt("CLASSIFIER_FUNCTION", new LiteralValue.Null())),
			"ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = NULL)"),
		["Limit the I/O of the governor"] = (
			Governed(Opt("MAX_OUTSTANDING_IO_PER_VOLUME", Number(".5", NumericLiteralKind.Decimal))),
			"ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = .5)"),
		["Limit the I/O of the governor by default"] = (
			Governed(Opt("MAX_OUTSTANDING_IO_PER_VOLUME", Word("DEFAULT"))),
			"ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = DEFAULT)"),

		["Create a resource pool"] = (
			new Statement.CreateResourcePool(X),
			"CREATE RESOURCE POOL x"),
		["Create a resource pool named by a string"] = (
			new Statement.CreateResourcePool(new Identifier("it's", IdentifierStyle.String)),
			"CREATE RESOURCE POOL 'it''s'"),
		["Create a resource pool with its settings"] = (
			new Statement.CreateResourcePool(new Identifier("[default]", IdentifierStyle.Bracketed)) { Options = [Opt("MIN_CPU_PERCENT", Digits("1")), Opt("MAX_CPU_PERCENT", Digits("10")), Opt("MAX_CPU_PERCENT", Digits("20")), Opt("CAP_IO_PERCENT", Digits("80")), Opt("MIN_IOPS_PER_VOLUME", Digits("0"))] },
			"CREATE RESOURCE POOL [default] WITH (MIN_CPU_PERCENT = 1, MAX_CPU_PERCENT = 10, MAX_CPU_PERCENT = 20, CAP_IO_PERCENT = 80, MIN_IOPS_PER_VOLUME = 0)"),
		["Create a resource pool with an affinity"] = (
			new Statement.CreateResourcePool(X) { Options = [Words("AFFINITY", ["SCHEDULER"], Word("AUTO")), Words("affinity", ["scheduler"], PoolRanges(("0", "63"), ("5", null))), Words("AFFINITY", ["NUMANODE"], PoolRanges(("0", null)))] },
			"CREATE RESOURCE POOL x WITH (AFFINITY SCHEDULER = AUTO, affinity scheduler = (0 TO 63, 5), AFFINITY NUMANODE = (0))"),
		["Alter a resource pool"] = (
			new Statement.AlterResourcePool(X, [Opt("MAX_MEMORY_PERCENT", Digits("15"))]),
			"ALTER RESOURCE POOL x WITH (MAX_MEMORY_PERCENT = 15)"),

		["Create a workload group"] = (
			new Statement.CreateWorkloadGroup(X),
			"CREATE WORKLOAD GROUP x"),
		["Create a workload group with its settings and pools"] = (
			new Statement.CreateWorkloadGroup(X) { Options = [Opt("IMPORTANCE", Word("HIGH")), Opt("REQUEST_MAX_MEMORY_GRANT_PERCENT", Number("25.5", NumericLiteralKind.Decimal)), Opt("MAX_DOP", Digits("1")), Opt("MAX_DOP", Digits("2")), Opt("GROUP_MIN_MEMORY_PERCENT", new LiteralValue.Null()), Opt("GROUP_MAX_TEMPDB_DATA_PERCENT", Digits("42")), Opt("QUERY_EXECUTION_TIMEOUT_SEC", Digits("5")), Opt("REQUEST_MAX_RESOURCE_GRANT_PERCENT", Number("3.5", NumericLiteralKind.Decimal))], Pools = [new WorkloadPool(new Identifier("p"), false), new WorkloadPool(new Identifier("[default]", IdentifierStyle.Bracketed), true)] },
			"CREATE WORKLOAD GROUP x WITH (IMPORTANCE = HIGH, REQUEST_MAX_MEMORY_GRANT_PERCENT = 25.5, MAX_DOP = 1, MAX_DOP = 2, GROUP_MIN_MEMORY_PERCENT = NULL, GROUP_MAX_TEMPDB_DATA_PERCENT = 42, QUERY_EXECUTION_TIMEOUT_SEC = 5, REQUEST_MAX_RESOURCE_GRANT_PERCENT = 3.5) USING p, EXTERNAL [default]"),
		["Alter a workload group"] = (
			new Statement.AlterWorkloadGroup(X),
			"ALTER WORKLOAD GROUP x"),
		["Alter a workload group named by a string"] = (
			new Statement.AlterWorkloadGroup(new Identifier("g", IdentifierStyle.String)) { Pools = [new WorkloadPool(new Identifier("e"), true)] },
			"ALTER WORKLOAD GROUP 'g' USING EXTERNAL e"),
		["Alter a workload group by its settings"] = (
			new Statement.AlterWorkloadGroup(X) { Options = [Opt("importance", Word("ABOVE_NORMAL")), Opt("GROUP_MAX_TEMPDB_DATA_MB", new LiteralValue.Null())] },
			"ALTER WORKLOAD GROUP x WITH (importance = ABOVE_NORMAL, GROUP_MAX_TEMPDB_DATA_MB = NULL)"),
		["Create an availability group"] = (
			Made(Replicated(Replica("n1", Url, Opt("AVAILABILITY_MODE", Word("SYNCHRONOUS_COMMIT")), Opt("FAILOVER_MODE", Word("AUTOMATIC"))))),
			"CREATE AVAILABILITY GROUP x FOR REPLICA ON 'n1' WITH (ENDPOINT_URL = 'TCP://a:5022', AVAILABILITY_MODE = SYNCHRONOUS_COMMIT, FAILOVER_MODE = AUTOMATIC)"),
		["Create an availability group with all"] = (
			new Statement.CreateAvailabilityGroup(X, new AvailabilitySource.Replicas([new Identifier("a"), new Identifier("b")], [Replica("n1", Url, Opt("SECONDARY_ROLE", null, Opt("ALLOW_CONNECTIONS", Word("READ_ONLY")), Opt("READ_ONLY_ROUTING_URL", Str("u"))), Opt("PRIMARY_ROLE", null, Opt("READ_ONLY_ROUTING_LIST", new Expression.Row([Str("x"), NStr("y")])), Opt("ALLOW_CONNECTIONS", Word("ALL")), Opt("READ_WRITE_ROUTING_URL", Str("w")))), Replica("n2", Opt("BACKUP_PRIORITY", Digits("50")), Opt("SESSION_TIMEOUT", Digits("10")), Opt("SEEDING_MODE", Word("MANUAL")))]))
			{
				Options = [Opt("AUTOMATED_BACKUP_PREFERENCE", Word("SECONDARY")), Opt("DB_FAILOVER", Word("ON")), Opt("BASIC", null), Opt("CONTAINED", null), Opt("CLUSTER_TYPE", Word("EXTERNAL")), Opt("DTC_SUPPORT", Word("PER_DB")), Opt("FAILURE_CONDITION_LEVEL", Digits("3")), Opt("HEALTH_CHECK_TIMEOUT", Digits("30000")), Opt("REQUIRED_SYNCHRONIZED_SECONDARIES_TO_COMMIT", Digits("1"))],
				ListenerName = (LiteralValue.String)Str("dns"), Listener = new AvailabilityListener.Ip([Address("1.2.3.4", "255.255.255.0"), Address("2001:db8::1")]) { Port = Digits("60173") },
			},
			"CREATE AVAILABILITY GROUP x WITH (AUTOMATED_BACKUP_PREFERENCE = SECONDARY, DB_FAILOVER = ON, BASIC, CONTAINED, CLUSTER_TYPE = EXTERNAL, DTC_SUPPORT = PER_DB, FAILURE_CONDITION_LEVEL = 3, HEALTH_CHECK_TIMEOUT = 30000, REQUIRED_SYNCHRONIZED_SECONDARIES_TO_COMMIT = 1) FOR DATABASE a, b REPLICA ON 'n1' WITH (ENDPOINT_URL = 'TCP://a:5022', SECONDARY_ROLE (ALLOW_CONNECTIONS = READ_ONLY, READ_ONLY_ROUTING_URL = 'u'), PRIMARY_ROLE (READ_ONLY_ROUTING_LIST = ('x', N'y'), ALLOW_CONNECTIONS = ALL, READ_WRITE_ROUTING_URL = 'w')), 'n2' WITH (BACKUP_PRIORITY = 50, SESSION_TIMEOUT = 10, SEEDING_MODE = MANUAL) LISTENER 'dns' (WITH IP (('1.2.3.4', '255.255.255.0'), ('2001:db8::1')), PORT = 60173)"),
		["Create an availability group with a routing list of none"] = (
			Made(Replicated(Replica("n1", Opt("PRIMARY_ROLE", null, Opt("READ_ONLY_ROUTING_LIST", Word("NONE")))))),
			"CREATE AVAILABILITY GROUP x FOR REPLICA ON 'n1' WITH (PRIMARY_ROLE (READ_ONLY_ROUTING_LIST = NONE))"),
		["Create a distributed availability group"] = (
			new Statement.CreateAvailabilityGroup(X, new AvailabilitySource.Groups([Group("ag2", Opt("LISTENER_URL", Str("tcp://a:5022")), Opt("AVAILABILITY_MODE", Word("ASYNCHRONOUS_COMMIT")), Opt("FAILOVER_MODE", Word("MANUAL")), Opt("SEEDING_MODE", Word("AUTOMATIC"))), Group("ag3", Opt("LISTENER", Str("l")))])) { Options = [Opt("DISTRIBUTED", null)] },
			"CREATE AVAILABILITY GROUP x WITH (DISTRIBUTED) AVAILABILITY GROUP ON 'ag2' WITH (LISTENER_URL = 'tcp://a:5022', AVAILABILITY_MODE = ASYNCHRONOUS_COMMIT, FAILOVER_MODE = MANUAL, SEEDING_MODE = AUTOMATIC), 'ag3' WITH (LISTENER = 'l')"),
		["Create an availability group with a listener of the network"] = (
			new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n1", Url))) { ListenerName = (LiteralValue.String)Str("d"), Listener = new AvailabilityListener.Dhcp(null) },
			"CREATE AVAILABILITY GROUP x FOR REPLICA ON 'n1' WITH (ENDPOINT_URL = 'TCP://a:5022') LISTENER 'd' (WITH DHCP)"),
		["Create an availability group with a listener of a network"] = (
			new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n1", Url))) { ListenerName = (LiteralValue.String)Str("d"), Listener = new AvailabilityListener.Dhcp(Address("1.2.3.4", "255.255.255.0")) },
			"CREATE AVAILABILITY GROUP x FOR REPLICA ON 'n1' WITH (ENDPOINT_URL = 'TCP://a:5022') LISTENER 'd' (WITH DHCP ON ('1.2.3.4', '255.255.255.0'))"),
		["Alter an availability group by a setting"] = (
			Altered(new AvailabilityAction.Set(Opt("DB_FAILOVER", Word("ON")))),
			"ALTER AVAILABILITY GROUP x SET (DB_FAILOVER = ON)"),
		["Alter an availability group by its role"] = (
			Altered(new AvailabilityAction.Set(Opt("ROLE", Word("SECONDARY")))),
			"ALTER AVAILABILITY GROUP x SET (ROLE = SECONDARY)"),
		["Alter an availability group by a setting alone"] = (
			Altered(new AvailabilityAction.Set(Opt("BASIC", null))),
			"ALTER AVAILABILITY GROUP x SET (BASIC)"),
		["Alter an availability group adding a database"] = (
			Altered(new AvailabilityAction.AddDatabase(new Identifier("db1"))),
			"ALTER AVAILABILITY GROUP x ADD DATABASE db1"),
		["Alter an availability group removing a database"] = (
			Altered(new AvailabilityAction.RemoveDatabase(new Identifier("db1"))),
			"ALTER AVAILABILITY GROUP x REMOVE DATABASE db1"),
		["Alter an availability group adding a replica"] = (
			Altered(new AvailabilityAction.AddReplica(Replica("n", Url))),
			"ALTER AVAILABILITY GROUP x ADD REPLICA ON 'n' WITH (ENDPOINT_URL = 'TCP://a:5022')"),
		["Alter an availability group modifying a replica"] = (
			Altered(new AvailabilityAction.ModifyReplica(Replica("n", Opt("SEEDING_MODE", Word("AUTOMATIC"))))),
			"ALTER AVAILABILITY GROUP x MODIFY REPLICA ON 'n' WITH (SEEDING_MODE = AUTOMATIC)"),
		["Alter an availability group removing a replica"] = (
			Altered(new AvailabilityAction.RemoveReplica((LiteralValue.String)NStr("n"))),
			"ALTER AVAILABILITY GROUP x REMOVE REPLICA ON N'n'"),
		["Alter an availability group joining another"] = (
			Altered(new AvailabilityAction.JoinAvailabilityGroup([Group("g", Opt("LISTENER_URL", Str("u")))])),
			"ALTER AVAILABILITY GROUP x JOIN AVAILABILITY GROUP ON 'g' WITH (LISTENER_URL = 'u')"),
		["Alter an availability group modifying two"] = (
			Altered(new AvailabilityAction.ModifyAvailabilityGroup([Group("g", Opt("SEEDING_MODE", Word("MANUAL"))), Group("h", Opt("FAILOVER_MODE", Word("MANUAL")))])),
			"ALTER AVAILABILITY GROUP x MODIFY AVAILABILITY GROUP ON 'g' WITH (SEEDING_MODE = MANUAL), 'h' WITH (FAILOVER_MODE = MANUAL)"),
		["Join an availability group"] = (
			Altered(new AvailabilityAction.Join()),
			"ALTER AVAILABILITY GROUP x JOIN"),
		["Grant the creation of any database"] = (
			Altered(new AvailabilityAction.CreateAnyDatabase(AvailabilityPermission.Grant)),
			"ALTER AVAILABILITY GROUP x GRANT CREATE ANY DATABASE"),
		["Deny the creation of any database"] = (
			Altered(new AvailabilityAction.CreateAnyDatabase(AvailabilityPermission.Deny)),
			"ALTER AVAILABILITY GROUP x DENY CREATE ANY DATABASE"),
		["Fail an availability group over"] = (
			Altered(new AvailabilityAction.Failover()),
			"ALTER AVAILABILITY GROUP x FAILOVER"),
		["Force an availability group over"] = (
			Altered(new AvailabilityAction.ForceFailoverAllowDataLoss()),
			"ALTER AVAILABILITY GROUP x FORCE_FAILOVER_ALLOW_DATA_LOSS"),
		["Take an availability group offline"] = (
			Altered(new AvailabilityAction.Offline()),
			"ALTER AVAILABILITY GROUP x OFFLINE"),
		["Add a listener"] = (
			Altered(new AvailabilityAction.AddListener((LiteralValue.String)Str("d"), new AvailabilityListener.Ip([Address("1.2.3.4")]))),
			"ALTER AVAILABILITY GROUP x ADD LISTENER 'd' (WITH IP (('1.2.3.4')))"),
		["Modify a listener by an address"] = (
			Altered(new AvailabilityAction.ModifyListener((LiteralValue.String)Str("d"), new ListenerChange.AddIp(Address("1.2.3.4", "255.255.255.0")))),
			"ALTER AVAILABILITY GROUP x MODIFY LISTENER 'd' (ADD IP ('1.2.3.4', '255.255.255.0'))"),
		["Modify a listener by its port"] = (
			Altered(new AvailabilityAction.ModifyListener((LiteralValue.String)Str("d"), new ListenerChange.Port(Digits("1433")))),
			"ALTER AVAILABILITY GROUP x MODIFY LISTENER 'd' (PORT = 1433)"),
		["Restart a listener"] = (
			Altered(new AvailabilityAction.RestartListener((LiteralValue.String)Str("d"))),
			"ALTER AVAILABILITY GROUP x RESTART LISTENER 'd'"),
		["Remove a listener"] = (
			Altered(new AvailabilityAction.RemoveListener((LiteralValue.String)Str("d"))),
			"ALTER AVAILABILITY GROUP x REMOVE LISTENER 'd'"),
		["Create an endpoint"] = (
			new Statement.CreateEndpoint(X),
			"CREATE ENDPOINT x"),
		["Create an endpoint with all"] = (
			new Statement.CreateEndpoint(X) { Authorization = new Identifier("o"), Settings = [Opt("STATE", Word("STARTED")), Opt("AFFINITY", Word("NONE")), Opt("state", Word("stopped"))], Transport = Tcp(Port, Opt("LISTENER_IP", Word("ALL"))), Payload = Broker(Opt("AUTHENTICATION", Authenticated("WINDOWS", "NTLM", "CERTIFICATE", "c")), Opt("ENCRYPTION", Encrypted("SUPPORTED", "ALGORITHM", "RC4", "AES")), Opt("MESSAGE_FORWARDING", Word("ENABLED")), Opt("MESSAGE_FORWARD_SIZE", Number("-1", NumericLiteralKind.DecimalInteger))) },
			"CREATE ENDPOINT x AUTHORIZATION o STATE = STARTED, AFFINITY = NONE, state = stopped AS TCP (LISTENER_PORT = 4022, LISTENER_IP = ALL) FOR SERVICE_BROKER (AUTHENTICATION = WINDOWS NTLM CERTIFICATE c, ENCRYPTION = SUPPORTED ALGORITHM RC4 AES, MESSAGE_FORWARDING = ENABLED, MESSAGE_FORWARD_SIZE = -1)"),
		["Create an endpoint with an affinity first"] = (
			new Statement.CreateEndpoint(X) { Settings = [Opt("AFFINITY", Word("ADMIN"))] },
			"CREATE ENDPOINT x AFFINITY = ADMIN"),
		["Create an endpoint listening on addresses"] = (
			Endpoint(Tcp(Opt("LISTENER_IP", IpAddress("1.2.3.4")), Opt("LISTENER_IP", IpAddress("1.1.1.1", "10.10.20.30")), Opt("listener_ip", Row(Str("a"), NStr("b"))), Opt("LISTENER_PORT", Number("-1", NumericLiteralKind.DecimalInteger))), null),
			"CREATE ENDPOINT x AS TCP (LISTENER_IP = (1.2.3.4), LISTENER_IP = (1.1.1.1:10.10.20.30), listener_ip = ('a', N'b'), LISTENER_PORT = -1)"),
		["Create an http endpoint"] = (
			Endpoint(Http(Opt("PATH", Str("/p")), Opt("SITE", NStr("*")), Opt("AUTH_REALM", Word("NONE")), Opt("DEFAULT_LOGON_DOMAIN", Str("d")), Opt("AUTHENTICATION", Row(Word("BASIC"), Word("kerberos"))), Opt("PORTS", Row(Word("SSL"), Word("CLEAR"))), Opt("CLEAR_PORT", Digits("10")), Opt("SSL_PORT", Number("-1", NumericLiteralKind.DecimalInteger)), Opt("COMPRESSION", Word("DISABLED"))), new EndpointPayload.Tsql([])),
			"CREATE ENDPOINT x AS HTTP (PATH = '/p', SITE = N'*', AUTH_REALM = NONE, DEFAULT_LOGON_DOMAIN = 'd', AUTHENTICATION = (BASIC, kerberos), PORTS = (SSL, CLEAR), CLEAR_PORT = 10, SSL_PORT = -1, COMPRESSION = DISABLED) FOR TSQL ()"),
		["Create an endpoint speaking T-SQL"] = (
			Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("STRICT"))])),
			"CREATE ENDPOINT x FOR TSQL (ENCRYPTION = STRICT)"),
		["Create an endpoint speaking T-SQL, encrypted by all"] = (
			Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("DISABLED")), Opt("ENCRYPTION", Encrypted("NEGOTIATED")), Opt("ENCRYPTION", Encrypted("required")), Opt("ENCRYPTION", Encrypted("REQUIRED", "ALGORITHM", "AES")), Opt("ENCRYPTION", Encrypted("SUPPORTED", "ALGORITHM", "AES", "RC4")), Opt("ENCRYPTION", Encrypted("SUPPORTED", "algorithm", "rc4"))])),
			"CREATE ENDPOINT x FOR TSQL (ENCRYPTION = DISABLED, ENCRYPTION = NEGOTIATED, ENCRYPTION = required, ENCRYPTION = REQUIRED ALGORITHM AES, ENCRYPTION = SUPPORTED ALGORITHM AES RC4, ENCRYPTION = SUPPORTED algorithm rc4)"),
		["Create an endpoint with the transport last"] = (
			new Statement.CreateEndpoint(X) { Payload = new EndpointPayload.Tsql([]), Transport = Tcp(Port), TransportLast = true },
			"CREATE ENDPOINT x FOR TSQL () AS TCP (LISTENER_PORT = 4022)"),
		["Create an endpoint mirroring databases"] = (
			Endpoint(Tcp(Port), new EndpointPayload.DatabaseMirroring([Opt("ROLE", Word("WITNESS")), Opt("AUTHENTICATION", Authenticated("CERTIFICATE", "[c d]", "WINDOWS", "KERBEROS"))])),
			"CREATE ENDPOINT x AS TCP (LISTENER_PORT = 4022) FOR DATABASE_MIRRORING (ROLE = WITNESS, AUTHENTICATION = CERTIFICATE [c d] WINDOWS KERBEROS)"),
		["Create an endpoint mirroring data"] = (
			Endpoint(Tcp(Port), new EndpointPayload.DatabaseMirroring([Opt("ROLE", Word("ALL")), Opt("AUTHENTICATION", Authenticated("WINDOWS"))]) { DataMirroring = true }),
			"CREATE ENDPOINT x AS TCP (LISTENER_PORT = 4022) FOR DATA_MIRRORING (ROLE = ALL, AUTHENTICATION = WINDOWS)"),
		["Create a SOAP endpoint"] = (
			Endpoint(Tcp(Port), Soap(Method("m", null, Opt("NAME", Str("d.dbo.n")), Opt("SCHEMA", Word("STANDARD")), Opt("FORMAT", Word("ALL_RESULTS"))), Soaped("WSDL", Word("DEFAULT")), Soaped("wsdl", Str("sp")), Soaped("BATCHES", Word("ENABLED")), Soaped("SESSIONS", Word("DISABLED")), Soaped("LOGIN_TYPE", Word("MIXED")), Soaped("SESSION_TIMEOUT", Digits("100")), Soaped("SESSION_TIMEOUT", Word("NEVER")), Soaped("DATABASE", Str("db")), Soaped("NAMESPACE", Word("DEFAULT")), Soaped("SCHEMA", Word("NONE")), Soaped("CHARACTER_SET", Word("XML")), Soaped("HEADER_LIMIT", Digits("8192")))),
			"CREATE ENDPOINT x AS TCP (LISTENER_PORT = 4022) FOR SOAP (WEBMETHOD 'm' (NAME = 'd.dbo.n', SCHEMA = STANDARD, FORMAT = ALL_RESULTS), WSDL = DEFAULT, wsdl = 'sp', BATCHES = ENABLED, SESSIONS = DISABLED, LOGIN_TYPE = MIXED, SESSION_TIMEOUT = 100, SESSION_TIMEOUT = NEVER, DATABASE = 'db', NAMESPACE = DEFAULT, SCHEMA = NONE, CHARACTER_SET = XML, HEADER_LIMIT = 8192)"),
		["Create a SOAP endpoint of no item"] = (
			Endpoint(null, Soap()),
			"CREATE ENDPOINT x FOR SOAP ()"),
		["Alter an endpoint"] = (
			new Statement.AlterEndpoint(X),
			"ALTER ENDPOINT x"),
		["Alter an endpoint by its state"] = (
			new Statement.AlterEndpoint(X) { Settings = [Opt("STATE", Word("STARTED")), Opt("AFFINITY", Word("NONE"))] },
			"ALTER ENDPOINT x STATE = STARTED, AFFINITY = NONE"),
		["Alter an endpoint by its transport last"] = (
			new Statement.AlterEndpoint(X) { Settings = [Opt("STATE", Word("DISABLED"))], Payload = Broker(), Transport = Http(Opt("PATH", Str("p"))), TransportLast = true },
			"ALTER ENDPOINT x STATE = DISABLED FOR SERVICE_BROKER () AS HTTP (PATH = 'p')"),
		["Alter a SOAP endpoint"] = (
			new Statement.AlterEndpoint(X) { Payload = Soap(Method("m", WebMethodAction.Add, Opt("NAME", Str("n"))), Method("p", WebMethodAction.Alter, Opt("FORMAT", Word("NONE"))), Method("q", WebMethodAction.Drop), new SoapItem.WebMethod((LiteralValue.String)Str("r")) { Namespace = (LiteralValue.String)NStr("n"), Action = WebMethodAction.Add, Options = [Opt("NAME", Str("x"))] }, Soaped("WSDL", Word("NONE"))) },
			"ALTER ENDPOINT x FOR SOAP (ADD WEBMETHOD 'm' (NAME = 'n'), ALTER WEBMETHOD 'p' (FORMAT = NONE), DROP WEBMETHOD 'q', ADD WEBMETHOD N'n'.'r' (NAME = 'x'), WSDL = NONE)"),
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

		Assert.Equal(tree, Bridge.ToAst(OldTree.Located.ParseStatement(text), text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names, above it, the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	static readonly Dictionary<string, ISqlNode> Refusals = new(StringComparer.Ordinal)
	{
		// KillNumber is '-'? and an unsigned numeric literal, one token; KillSession is that or BrokerString.
		["a kill of a session that is a number with an exponent of no digits"] = new Statement.Kill(Number("1E", NumericLiteralKind.Approximate)),
		["a dbcc argument that is a number with an exponent of no digits"] = new Statement.Dbcc(X) { Arguments = [Arg(Number("1E", NumericLiteralKind.Approximate), null)] },
		["a dbcc maxdop with an exponent of no digits"] = new Statement.Dbcc(X) { Options = [Opt("MAXDOP", Number("1E", NumericLiteralKind.Approximate))] },
		["a dbcc wait setting that is partitioned"] = new Statement.Dbcc(X) { Options = [Opt("WAIT_AT_LOW_PRIORITY", null, new Option(new Identifier("MAX_DURATION"), Digits("5"), []) { Partitions = [] })] },
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
		// GovernorAction: one setting, and the classifier is a function of two parts exactly or NULL.
		["a governor setting it has not"] = Governed(Opt("MAX_DOP", Digits("1"))),
		["a governor setting named in brackets"] = Governed(new Option(new Identifier("[CLASSIFIER_FUNCTION]", IdentifierStyle.Bracketed), new LiteralValue.Null(), [])),
		["a governor setting of two words"] = Governed(Words("CLASSIFIER_FUNCTION", ["X"], new LiteralValue.Null())),
		["a governor setting that omits its ="] = Governed(new Option(new Identifier("CLASSIFIER_FUNCTION"), new LiteralValue.Null(), []) { EqualsOmitted = true }),
		["a governor setting with no value"] = Governed(Opt("CLASSIFIER_FUNCTION", null)),
		["a classifier of one part"] = Governed(Opt("CLASSIFIER_FUNCTION", new Expression.Reference(Dotted("f")))),
		["a classifier of three parts"] = Governed(Opt("CLASSIFIER_FUNCTION", new Expression.Reference(Dotted("a", "b", "c")))),
		["a classifier with a part left out"] = Governed(Opt("CLASSIFIER_FUNCTION", new Expression.Reference(Dotted("a", "")))),
		["a classifier that is a string"] = Governed(Opt("CLASSIFIER_FUNCTION", Str("dbo.f"))),
		["a classifier that is a variable"] = Governed(Opt("CLASSIFIER_FUNCTION", new Expression.Reference(Dotted("dbo", "@f")))),
		["a classifier that is a reserved word"] = Governed(Opt("CLASSIFIER_FUNCTION", new Expression.Reference(Dotted("dbo", "select")))),
		["a classifier that is DEFAULT"] = Governed(Opt("CLASSIFIER_FUNCTION", Word("DEFAULT"))),
		["a governor I/O limit that is signed"] = Governed(Opt("MAX_OUTSTANDING_IO_PER_VOLUME", Number("-1", NumericLiteralKind.DecimalInteger))),
		["a governor I/O limit with an exponent"] = Governed(Opt("MAX_OUTSTANDING_IO_PER_VOLUME", Number("1e1", NumericLiteralKind.Approximate))),
		["a governor I/O limit that is NULL"] = Governed(Opt("MAX_OUTSTANDING_IO_PER_VOLUME", new LiteralValue.Null())),
		["a governor I/O limit that is a string"] = Governed(Opt("MAX_OUTSTANDING_IO_PER_VOLUME", Str("1"))),

		// PoolName is an identifier or a string in single quotes, no national string.
		["a pool named by a variable"] = new Statement.CreateResourcePool(new Identifier("@p")),
		["a pool named by a reserved word"] = new Statement.CreateResourcePool(new Identifier("select")),
		["a pool named by a national string"] = new Statement.CreateResourcePool(new Identifier("p", IdentifierStyle.NationalString)),
		["a pool named by two words"] = new Statement.CreateResourcePool(new Identifier("a b")),
		// PoolOption: whole numbers, no sign, no point (Msg 102); the affinity AUTO or ranges, ranges of nodes only.
		["a pool altered with no setting"] = new Statement.AlterResourcePool(X, []),
		["a pool setting it has not"] = new Statement.CreateResourcePool(X) { Options = [Opt("MAX_PROCESSES", Digits("1"))] },
		["a pool percentage that is a string"] = new Statement.CreateResourcePool(X) { Options = [Opt("MAX_CPU_PERCENT", Str("1"))] },
		["a pool percentage that is signed"] = new Statement.CreateResourcePool(X) { Options = [Opt("MAX_CPU_PERCENT", Number("-1", NumericLiteralKind.DecimalInteger))] },
		["a pool percentage with a fraction"] = new Statement.CreateResourcePool(X) { Options = [Opt("MAX_CPU_PERCENT", Number("1.5", NumericLiteralKind.Decimal))] },
		["a pool percentage that is a name"] = new Statement.CreateResourcePool(X) { Options = [Opt("MAX_CPU_PERCENT", Word("a"))] },
		["a pool setting that omits its ="] = new Statement.CreateResourcePool(X) { Options = [new Option(new Identifier("MAX_CPU_PERCENT"), Digits("1"), []) { EqualsOmitted = true }] },
		["a pool affinity of CPUs"] = new Statement.CreateResourcePool(X) { Options = [Words("AFFINITY", ["CPU"], PoolRanges(("0", null)))] },
		["a pool affinity of nodes that is AUTO"] = new Statement.CreateResourcePool(X) { Options = [Words("AFFINITY", ["NUMANODE"], Word("AUTO"))] },
		["a pool affinity of schedulers with no brackets"] = new Statement.CreateResourcePool(X) { Options = [Words("AFFINITY", ["SCHEDULER"], new Expression.ServerRanges([new PartitionRange(Digits("1"), null)]))] },
		["a pool affinity alone"] = new Statement.CreateResourcePool(X) { Options = [Opt("AFFINITY", PoolRanges(("0", null)))] },
		["a pool affinity of no range"] = new Statement.CreateResourcePool(X) { Options = [Words("AFFINITY", ["SCHEDULER"], new Expression.PoolRanges([]))] },
		// WorkloadOption: IMPORTANCE from its six words, the grants numbers, a few of them NULL.
		["a workload setting it has not"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("IMPORTANCES", Word("LOW"))] },
		["a workload importance outside its words"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("IMPORTANCE", Word("URGENT"))] },
		["a workload importance that is a string"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("IMPORTANCE", Str("LOW"))] },
		["a workload importance that is a number"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("IMPORTANCE", Digits("1"))] },
		["a workload limit that is signed"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("MAX_DOP", Number("-1", NumericLiteralKind.DecimalInteger))] },
		["a workload limit with a fraction"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("MAX_DOP", Number("1.5", NumericLiteralKind.Decimal))] },
		["a workload limit that is NULL"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("MAX_DOP", new LiteralValue.Null())] },
		["a workload grant with an exponent"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("REQUEST_MAX_MEMORY_GRANT_PERCENT", Number("1e1", NumericLiteralKind.Approximate))] },
		["a workload grant that is NULL"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("REQUEST_MAX_MEMORY_GRANT_PERCENT", new LiteralValue.Null())] },
		["a workload minimum that is a decimal"] = new Statement.CreateWorkloadGroup(X) { Options = [Opt("GROUP_MIN_MEMORY_PERCENT", Number("1.5", NumericLiteralKind.Decimal))] },
		["a workload group setting that omits its ="] = new Statement.CreateWorkloadGroup(X) { Options = [new Option(new Identifier("MAX_DOP"), Digits("1"), []) { EqualsOmitted = true }] },
		// WorkloadPool is EXTERNAL? and a name: no string, no variable, no reserved word, and USING names one at least.
		["a workload pool that is a string"] = new Statement.CreateWorkloadGroup(X) { Pools = [new WorkloadPool(new Identifier("p", IdentifierStyle.String), false)] },
		["a workload pool that is a variable"] = new Statement.CreateWorkloadGroup(X) { Pools = [new WorkloadPool(new Identifier("@p"), false)] },
		["a workload pool that is a reserved word"] = new Statement.CreateWorkloadGroup(X) { Pools = [new WorkloadPool(new Identifier("external"), false)] },
		["a workload group named by a variable"] = new Statement.AlterWorkloadGroup(new Identifier("@g")),
		// AvailabilityBody: an ALTER does one thing; a group made says what it is made of. The group and the database are identifiers.
		["an availability group named by a variable"] = new Statement.CreateAvailabilityGroup(new Identifier("@g"), Replicated(Replica("n", Url))),
		["an availability group named by a string"] = Altered(new AvailabilityAction.Join()) with { Name = new Identifier("g", IdentifierStyle.String) },
		["an availability group of a database that is a string"] = Altered(new AvailabilityAction.AddDatabase(new Identifier("d", IdentifierStyle.String))),
		["an availability group of a database that is a variable"] = Altered(new AvailabilityAction.AddDatabase(new Identifier("@d"))),
		["an availability group of a database that is reserved"] = Altered(new AvailabilityAction.RemoveDatabase(new Identifier("select"))),
		["an availability group made of no replica"] = Made(new AvailabilitySource.Replicas([], [])),
		["an availability group made of a database that is a name of two words"] = Made(new AvailabilitySource.Replicas([new Identifier("a b")], [Replica("n", Url)])),
		["an availability group made of three groups"] = Made(new AvailabilitySource.Groups([Group("a", Opt("LISTENER", Str("l"))), Group("b", Opt("LISTENER", Str("l"))), Group("c", Opt("LISTENER", Str("l")))])),
		["an availability group made of no group"] = Made(new AvailabilitySource.Groups([])),
		["an availability group joined to no group"] = Altered(new AvailabilityAction.JoinAvailabilityGroup([])),
		["an availability group made with a listener name and no listener"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { ListenerName = (LiteralValue.String)Str("d") },
		["an availability group made with a listener and no name"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Listener = new AvailabilityListener.Dhcp(null) },
		// AvailabilityWith: a closed list, three words alone, DB_FAILOVER ON or OFF with its =.
		["an availability setting it has not"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("COLOUR", Word("RED"))] },
		["an availability setting alone that takes a value"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("DB_FAILOVER", null)] },
		["an availability word with a value"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("BASIC", Word("ON"))] },
		["an availability backup preference outside its words"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("AUTOMATED_BACKUP_PREFERENCE", Word("ANY"))] },
		["an availability backup preference that is a string"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("AUTOMATED_BACKUP_PREFERENCE", Str("PRIMARY"))] },
		["an availability level that is signed"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("FAILURE_CONDITION_LEVEL", Number("-1", NumericLiteralKind.DecimalInteger))] },
		["an availability timeout that is a decimal"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("HEALTH_CHECK_TIMEOUT", Number("1.5", NumericLiteralKind.Decimal))] },
		["an availability failover switch that is a number"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("DB_FAILOVER", Digits("1"))] },
		["an availability cluster type outside its words"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("CLUSTER_TYPE", Word("MIXED"))] },
		["an availability setting of a group made that is the role"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { Options = [Opt("ROLE", Word("SECONDARY"))] },
		["an availability role that is PRIMARY"] = Altered(new AvailabilityAction.Set(Opt("ROLE", Word("PRIMARY")))),
		["an availability setting set with a name that omits its ="] = Altered(new AvailabilityAction.Set(new Option(new Identifier("DB_FAILOVER"), Word("ON"), []) { EqualsOmitted = true })),
		// AvailabilityReplica: a string and a bracket of one setting at least.
		["a replica named by a name"] = Made(Replicated(new AvailabilityReplica((LiteralValue.String)new LiteralValue.String("n", StringLiteralKind.Character), [Url]))),
		["a replica named by a string with a character set"] = Made(Replicated(new AvailabilityReplica(new LiteralValue.String("'n'", StringLiteralKind.Character) { CharacterSet = new CharacterSetName(Dotted("utf8")) }, [Url]))),
		["a replica of no setting"] = Made(Replicated(Replica("n"))),
		["a replica setting it has not"] = Made(Replicated(Replica("n", Opt("PORT", Digits("1"))))),
		["a replica endpoint that is a name"] = Made(Replicated(Replica("n", Opt("ENDPOINT_URL", Word("u"))))),
		["a replica availability mode outside its words"] = Made(Replicated(Replica("n", Opt("AVAILABILITY_MODE", Word("LAZY"))))),
		["a replica failover mode outside its words"] = Made(Replicated(Replica("n", Opt("FAILOVER_MODE", Word("SYNCHRONOUS_COMMIT"))))),
		["a replica seeding mode that is EXTERNAL"] = Made(Replicated(Replica("n", Opt("SEEDING_MODE", Word("EXTERNAL"))))),
		["a replica priority that is signed"] = Made(Replicated(Replica("n", Opt("BACKUP_PRIORITY", Number("-1", NumericLiteralKind.DecimalInteger))))),
		["a replica secondary role with no setting"] = Made(Replicated(Replica("n", Opt("SECONDARY_ROLE", null)))),
		["a replica secondary role with a value"] = Made(Replicated(Replica("n", Opt("SECONDARY_ROLE", Word("ON"), Opt("ALLOW_CONNECTIONS", Word("ALL")))))),
		["a replica secondary role allowing writes"] = Made(Replicated(Replica("n", Opt("SECONDARY_ROLE", null, Opt("ALLOW_CONNECTIONS", Word("READ_WRITE")))))),
		["a replica primary role allowing reads only"] = Made(Replicated(Replica("n", Opt("PRIMARY_ROLE", null, Opt("ALLOW_CONNECTIONS", Word("READ_ONLY")))))),
		["a replica primary role routing to a name"] = Made(Replicated(Replica("n", Opt("PRIMARY_ROLE", null, Opt("READ_ONLY_ROUTING_LIST", new Expression.Row([Word("a")])))))),
		["a replica primary role routing to nothing"] = Made(Replicated(Replica("n", Opt("PRIMARY_ROLE", null, Opt("READ_ONLY_ROUTING_LIST", new Expression.Row([])))))),
		["a replica primary role routing to a list written ROW"] = Made(Replicated(Replica("n", Opt("PRIMARY_ROLE", null, Opt("READ_ONLY_ROUTING_LIST", new Expression.Row([Str("a")], RowKeyword: true)))))),
		["a replica primary role routing to a string alone"] = Made(Replicated(Replica("n", Opt("PRIMARY_ROLE", null, Opt("READ_ONLY_ROUTING_LIST", Str("a")))))),
		["a replica role in a role"] = Made(Replicated(Replica("n", Opt("PRIMARY_ROLE", null, Opt("PRIMARY_ROLE", null, Opt("ALLOW_CONNECTIONS", Word("ALL"))))))),
		// AvailabilityGroupOption: where a group listens, and its modes.
		["a group setting it has not"] = Altered(new AvailabilityAction.JoinAvailabilityGroup([Group("g", Opt("ENDPOINT_URL", Str("u")))])),
		["a group of no setting"] = Altered(new AvailabilityAction.JoinAvailabilityGroup([Group("g")])),
		["a group failover mode that is AUTOMATIC"] = Altered(new AvailabilityAction.ModifyAvailabilityGroup([Group("g", Opt("FAILOVER_MODE", Word("AUTOMATIC")))])),
		["a group availability mode that is CONFIGURATION_ONLY"] = Altered(new AvailabilityAction.ModifyAvailabilityGroup([Group("g", Opt("AVAILABILITY_MODE", Word("CONFIGURATION_ONLY")))])),
		["a group listener that is a name"] = Altered(new AvailabilityAction.ModifyAvailabilityGroup([Group("g", Opt("LISTENER", Word("l")))])),
		// A listener: DHCP, or IP and addresses with a port; an address is a string and, where it has one, a mask; both with DHCP.
		["a listener on the network with an address and no mask"] = new Statement.CreateAvailabilityGroup(X, Replicated(Replica("n", Url))) { ListenerName = (LiteralValue.String)Str("d"), Listener = new AvailabilityListener.Dhcp(Address("1.2.3.4")) },
		["a listener of no address"] = Altered(new AvailabilityAction.AddListener((LiteralValue.String)Str("d"), new AvailabilityListener.Ip([]))),
		["a listener of an address that is a name"] = Altered(new AvailabilityAction.AddListener((LiteralValue.String)Str("d"), new AvailabilityListener.Ip([new ListenerAddress(new LiteralValue.String("a", StringLiteralKind.Character), null)]))),
		["a listener of a port that is signed"] = Altered(new AvailabilityAction.AddListener((LiteralValue.String)Str("d"), new AvailabilityListener.Ip([Address("1.2.3.4")]) { Port = Number("-1", NumericLiteralKind.DecimalInteger) })),
		["a listener of a port that is a decimal"] = Altered(new AvailabilityAction.AddListener((LiteralValue.String)Str("d"), new AvailabilityListener.Ip([Address("1.2.3.4")]) { Port = Number("1.5", NumericLiteralKind.Decimal) })),
		["a listener modified to a port that is signed"] = Altered(new AvailabilityAction.ModifyListener((LiteralValue.String)Str("d"), new ListenerChange.Port(Number("-1", NumericLiteralKind.DecimalInteger)))),
		["a listener named by a name"] = Altered(new AvailabilityAction.RestartListener(new LiteralValue.String("d", StringLiteralKind.Character))),
		["a listener named by a variable"] = Altered(new AvailabilityAction.RemoveListener(new LiteralValue.String("@d", StringLiteralKind.Character))),
		["an availability permission that is none"] = Altered(new AvailabilityAction.CreateAnyDatabase((AvailabilityPermission)9)),
		// An endpoint is named by an Identifier, and owned by one only where it is made.
		["an endpoint named by a variable"] = new Statement.CreateEndpoint(new Identifier("@e")),
		["an endpoint named by a reserved word"] = new Statement.AlterEndpoint(new Identifier("select")),
		["an endpoint owned by a variable"] = new Statement.CreateEndpoint(X) { Authorization = new Identifier("@o") },
		["an endpoint with the transport last, with no payload"] = new Statement.CreateEndpoint(X) { Transport = Tcp(Port), TransportLast = true },
		["an endpoint with the transport last, with no transport"] = new Statement.AlterEndpoint(X) { Payload = Broker(), TransportLast = true },
		// EndpointState: STATE from three words and AFFINITY from two, a name outside them none.
		["an endpoint state outside its words"] = new Statement.CreateEndpoint(X) { Settings = [Opt("STATE", Word("RUNNING"))] },
		["an endpoint state that is a string"] = new Statement.CreateEndpoint(X) { Settings = [Opt("STATE", Str("STARTED"))] },
		["an endpoint affinity outside its words"] = new Statement.CreateEndpoint(X) { Settings = [Opt("AFFINITY", Word("ALL"))] },
		["an endpoint setting it has not"] = new Statement.CreateEndpoint(X) { Settings = [Opt("PORT", Digits("1"))] },
		["an endpoint setting that omits its ="] = new Statement.CreateEndpoint(X) { Settings = [new Option(new Identifier("STATE"), Word("STARTED"), []) { EqualsOmitted = true }] },
		// AS TCP needs a setting at least (`AS TCP ()` is refused); the port is whole digits, a minus allowed; the address is ALL, strings or four numbers.
		["a tcp transport of no setting"] = Endpoint(Tcp(), null),
		["a tcp transport with a setting it has not"] = Endpoint(Tcp(Opt("PATH", Str("p"))), null),
		["a tcp port that is a string"] = Endpoint(Tcp(Opt("LISTENER_PORT", Str("1"))), null),
		["a tcp port that is a decimal"] = Endpoint(Tcp(Opt("LISTENER_PORT", Number("1.5", NumericLiteralKind.Decimal))), null),
		["a tcp port that is a plus"] = Endpoint(Tcp(Opt("LISTENER_PORT", Number("+1", NumericLiteralKind.DecimalInteger))), null),
		["a tcp port that is a minus over a number"] = Endpoint(Tcp(Opt("LISTENER_PORT", new Expression.Unary(UnaryOperator.Minus, Digits("1")))), null),
		["a tcp address that is a name"] = Endpoint(Tcp(Opt("LISTENER_IP", Word("x"))), null),
		["a tcp address that is a string"] = Endpoint(Tcp(Opt("LISTENER_IP", Str("1.2.3.4"))), null),
		["a tcp address of three numbers"] = Endpoint(Tcp(Opt("LISTENER_IP", IpAddress("1.2.3"))), null),
		["a tcp address of five numbers"] = Endpoint(Tcp(Opt("LISTENER_IP", IpAddress("1.2.3.4.5"))), null),
		["a tcp address with a blank"] = Endpoint(Tcp(Opt("LISTENER_IP", IpAddress("1. 2.3.4"))), null),
		["a tcp address with a sign"] = Endpoint(Tcp(Opt("LISTENER_IP", IpAddress("1.2.3.-4"))), null),
		["a tcp address with a mask of three numbers"] = Endpoint(Tcp(Opt("LISTENER_IP", IpAddress("1.2.3.4", "255.0.0"))), null),
		["a tcp address of strings in no brackets"] = Endpoint(Tcp(Opt("LISTENER_IP", Row())), null),
		["a tcp address of a string and a name"] = Endpoint(Tcp(Opt("LISTENER_IP", Row(Str("a"), Word("b")))), null),
		["a tcp address of strings written ROW"] = Endpoint(Tcp(Opt("LISTENER_IP", new Expression.Row([Str("a")], RowKeyword: true))), null),
		// HTTP: its own settings, the lists from their words, a path in quotes.
		["an http transport of no setting"] = Endpoint(Http(), null),
		["an http transport with the port of tcp"] = Endpoint(Http(Port), null),
		["an http path that is a name"] = Endpoint(Http(Opt("PATH", Word("p"))), null),
		["an http path that is NONE"] = Endpoint(Http(Opt("PATH", Word("NONE"))), null),
		["an http realm that is DEFAULT"] = Endpoint(Http(Opt("AUTH_REALM", Word("DEFAULT"))), null),
		["an http authentication outside its words"] = Endpoint(Http(Opt("AUTHENTICATION", Row(Word("BASIC"), Word("PLAIN")))), null),
		["an http authentication with no brackets"] = Endpoint(Http(Opt("AUTHENTICATION", Word("BASIC"))), null),
		["an http authentication of no word"] = Endpoint(Http(Opt("AUTHENTICATION", Row())), null),
		["an http ports of a word it has not"] = Endpoint(Http(Opt("PORTS", Row(Word("BASIC")))), null),
		["an http compression that is ON"] = Endpoint(Http(Opt("COMPRESSION", Word("ON"))), null),
		["an http port that is a string"] = Endpoint(Http(Opt("SSL_PORT", Str("1"))), null),
		// FOR: T-SQL takes an encryption and nothing else; the brokers a few more.
		["a T-SQL payload with an authentication"] = Endpoint(null, new EndpointPayload.Tsql([Opt("AUTHENTICATION", Authenticated("WINDOWS"))])),
		["a T-SQL payload with a role"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ROLE", Word("ALL"))])),
		["a broker payload with a role"] = Endpoint(null, Broker(Opt("ROLE", Word("ALL")))),
		["a mirroring payload with a forwarding"] = Endpoint(null, new EndpointPayload.DatabaseMirroring([Opt("MESSAGE_FORWARDING", Word("ENABLED"))])),
		["a mirroring role outside its words"] = Endpoint(null, new EndpointPayload.DatabaseMirroring([Opt("ROLE", Word("PRIMARY"))])),
		["a forwarding that is ON"] = Endpoint(null, Broker(Opt("MESSAGE_FORWARDING", Word("ON")))),
		["a forwarding size that is a decimal"] = Endpoint(null, Broker(Opt("MESSAGE_FORWARD_SIZE", Number("1.5", NumericLiteralKind.Decimal)))),
		// EndpointEncryptionSyntax and EndpointAuthenticationSyntax: runs of words the rules read and no others.
		["an encryption of no word"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted())])),
		["an encryption of a level it has not"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("OPTIONAL"))])),
		["an encryption that is a name"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Word("STRICT"))])),
		["an encryption of a level that takes no algorithm"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("STRICT", "ALGORITHM", "AES"))])),
		["an encryption with an algorithm missing"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("REQUIRED", "ALGORITHM"))])),
		["an encryption with an algorithm it has not"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("REQUIRED", "ALGORITHM", "DES"))])),
		["an encryption with an algorithm twice"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("REQUIRED", "ALGORITHM", "AES", "AES"))])),
		["an encryption with three algorithms"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("REQUIRED", "ALGORITHM", "AES", "RC4", "AES"))])),
		["an encryption of a word with a blank"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("REQUIRED ALGORITHM AES"))])),
		["an encryption of a word in brackets"] = Endpoint(null, new EndpointPayload.Tsql([Opt("ENCRYPTION", Encrypted("[STRICT]"))])),
		["an authentication of no word"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated()))),
		["an authentication of a word it has not"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("KERBEROS")))),
		["an authentication of a protocol with no WINDOWS"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("NTLM", "CERTIFICATE", "c")))),
		["an authentication of a protocol it has not"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("WINDOWS", "BASIC")))),
		["an authentication of WINDOWS twice"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("WINDOWS", "CERTIFICATE", "c", "WINDOWS")))),
		["an authentication of a certificate twice"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("CERTIFICATE", "a", "CERTIFICATE", "b")))),
		["an authentication of a certificate with no name"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("WINDOWS", "CERTIFICATE")))),
		["an authentication of a certificate named by a reserved word"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("CERTIFICATE", "select")))),
		["an authentication of a certificate named by a variable"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("CERTIFICATE", "@c")))),
		["an authentication of a certificate named by two words"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Authenticated("CERTIFICATE", "a b")))),
		["an authentication that is a name"] = Endpoint(null, Broker(Opt("AUTHENTICATION", Word("WINDOWS")))),
		// SOAP: the settings and the web methods; a method dropped takes nothing further, one added or altered a bracket that is not empty.
		["a SOAP setting it has not"] = Endpoint(null, Soap(Soaped("BATCHES_", Word("ENABLED")))),
		["a SOAP batches that is ON"] = Endpoint(null, Soap(Soaped("BATCHES", Word("ON")))),
		["a SOAP wsdl that is a number"] = Endpoint(null, Soap(Soaped("WSDL", Digits("1")))),
		["a SOAP login type outside its words"] = Endpoint(null, Soap(Soaped("LOGIN_TYPE", Word("SQL")))),
		["a SOAP timeout that is signed"] = Endpoint(null, Soap(Soaped("SESSION_TIMEOUT", Number("-1", NumericLiteralKind.DecimalInteger)))),
		["a SOAP schema that is DEFAULT"] = Endpoint(null, Soap(Soaped("SCHEMA", Word("DEFAULT")))),
		["a SOAP character set outside its words"] = Endpoint(null, Soap(Soaped("CHARACTER_SET", Word("UTF8")))),
		["a SOAP database that is NONE"] = Endpoint(null, Soap(Soaped("DATABASE", Word("NONE")))),
		["a SOAP header limit that is a decimal"] = Endpoint(null, Soap(Soaped("HEADER_LIMIT", Number("1.5", NumericLiteralKind.Decimal)))),
		["a SOAP method written bare, in an altered endpoint"] = new Statement.AlterEndpoint(X) { Payload = Soap(Method("m", null, Opt("NAME", Str("n")))) },
		["a SOAP method dropped with a bracket"] = Endpoint(null, Soap(Method("m", WebMethodAction.Drop, Opt("NAME", Str("n"))))),
		["a SOAP method added with no bracket"] = Endpoint(null, Soap(Method("m", WebMethodAction.Add))),
		["a SOAP method written bare, with no bracket"] = Endpoint(null, Soap(Method("m", null))),
		["a SOAP method named by a name"] = Endpoint(null, Soap(new SoapItem.WebMethod(new LiteralValue.String("m", StringLiteralKind.Character)) { Action = WebMethodAction.Add, Options = [Opt("NAME", Str("n"))] })),
		["a SOAP method that is no action"] = Endpoint(null, Soap(Method("m", (WebMethodAction)9, Opt("NAME", Str("n"))))),
		["a SOAP method object that is a name"] = Endpoint(null, Soap(Method("m", WebMethodAction.Add, Opt("NAME", Word("d.dbo.n"))))),
		["a SOAP method schema outside its words"] = Endpoint(null, Soap(Method("m", WebMethodAction.Add, Opt("SCHEMA", Word("OTHER"))))),
		["a SOAP method format outside its words"] = Endpoint(null, Soap(Method("m", WebMethodAction.Add, Opt("FORMAT", Word("XML"))))),
		["a SOAP method option it has not"] = Endpoint(null, Soap(Method("m", WebMethodAction.Add, Opt("PATH", Str("p"))))),
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
