using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Server family's bridge (Bridge.Server.cs): which record and members it builds for a text, where another record or a flag set
/// the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class ServerBridgeTests
{
	static void Converts(string text)
	{
		var outcome = BridgeOracle.Run(OldTree.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	[Theory]
	[InlineData("KILL 53",                                                      "Kill a session")]
	[InlineData("KILL -1 WITH STATUSONLY",                                      "Kill a negative session")]
	[InlineData("kill N'uow' with rollback",                                    "Kill a unit of work")]
	[InlineData("KILL 'uow' WITH COMMIT",                                       "Kill a unit of work, commit")]
	[InlineData("KILL STATS JOB 12",                                            "Kill a statistics job")]
	[InlineData("kill stats job -12",                                           "Kill a negative statistics job")]
	[InlineData("KILL QUERY NOTIFICATION SUBSCRIPTION ALL",                     "Kill every subscription")]
	[InlineData("kill query notification subscription 73",                      "Kill a subscription")]
	[InlineData("RECONFIGURE",                                                  "Reconfigure")]
	[InlineData("reconfigure with override",                                    "Reconfigure with override")]
	[InlineData("SHUTDOWN",                                                     "Shutdown")]
	[InlineData("shutdown with nowait",                                         "Shutdown with no wait")]
	public void The_bridge_builds_the_command_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(OldTree.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Kill a session" => made is Ast.Statement.Kill { Target: Ast.LiteralValue.Numeric { Text: "53" }, With: null },
			"Kill a negative session" => made is Ast.Statement.Kill { Target: Ast.LiteralValue.Numeric { Text: "-1" }, With: Ast.KillOption.StatusOnly },
			"Kill a unit of work" => made is Ast.Statement.Kill { Target: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National }, With: Ast.KillOption.Rollback },
			"Kill a unit of work, commit" => made is Ast.Statement.Kill { Target: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.Character }, With: Ast.KillOption.Commit },
			"Kill a statistics job" => made is Ast.Statement.KillStatsJob { Job.Text: "12" },
			"Kill a negative statistics job" => made is Ast.Statement.KillStatsJob { Job.Text: "-12" },
			"Kill every subscription" => made is Ast.Statement.KillQueryNotificationSubscription { All: true, Subscription: null },
			"Kill a subscription" => made is Ast.Statement.KillQueryNotificationSubscription { All: false, Subscription.Text: "73" },
			"Reconfigure" => made is Ast.Statement.Reconfigure { Override: false },
			"Reconfigure with override" => made is Ast.Statement.Reconfigure { Override: true },
			"Shutdown" => made is Ast.Statement.Shutdown { NoWait: false },
			"Shutdown with no wait" => made is Ast.Statement.Shutdown { NoWait: true },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	[Theory]
	[InlineData("DBCC CHECKDB",                                                 "Bare")]
	[InlineData("DBCC CHECKDB ()",                                              "Empty brackets")]
	[InlineData("DBCC foo (1, -2, $3, N'n', 0x06, NULL, @@v, NEXT, #t, [default])", "Arguments")]
	[InlineData("DBCC foo (a = 'x', @x = 1)",                                   "Named arguments")]
	[InlineData("DBCC foo WITH maxdop = 4, MAXDOP, no_infomsgs",                "Options")]
	[InlineData("DBCC foo WITH stat_header JOIN density_vector JOIN stat_header", "Join")]
	[InlineData("DBCC foo WITH WAIT_AT_LOW_PRIORITY, wait_at_low_priority (max_duration = 1 minutes, abort_after_wait = foo)", "Wait")]
	[InlineData("DBCC foo ($ - 3)",                                             "Money with blanks")]
	public void The_bridge_builds_the_dbcc_record_the_text_means(string text, string expected)
	{
		if (expected == "Money with blanks")
		{
			Assert.Throws<BridgeGap>(() => Bridge.ToAst(OldTree.Located.ParseStatement(text), text));

			return;
		}

		var made = Bridge.ToAst(OldTree.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Bare" => made is Ast.Statement.Dbcc { Command.Text: "CHECKDB", Arguments: null, Options.Count: 0 },
			"Empty brackets" => made is Ast.Statement.Dbcc { Arguments.Count: 0 },
			"Arguments" => made is Ast.Statement.Dbcc
			{
				Arguments:
				[
					{ Value: Ast.LiteralValue.Numeric { Text: "1" }, Name: null }, { Value: Ast.LiteralValue.Numeric { Text: "-2" } }, { Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Money } },
					{ Value: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } }, { Value: Ast.LiteralValue.Binary { ZeroX: true } }, { Value: Ast.LiteralValue.Null }, { Value: Ast.Expression.Variable },
					{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "NEXT" }] } }, { Value: Ast.Expression.Reference { Name.Parts: [{ Text: "#t" }] } }, { Value: Ast.Expression.Reference { Name.Parts: [{ Style: Ast.IdentifierStyle.Bracketed }] } },
				],
			},
			"Named arguments" => made is Ast.Statement.Dbcc { Arguments: [{ Name.Text: "a", Value: Ast.LiteralValue.String }, { Name.Text: "@x", Value: Ast.LiteralValue.Numeric }] },
			"Options" => made is Ast.Statement.Dbcc { Options: [{ Name.Text: "maxdop", Value: Ast.LiteralValue.Numeric }, { Name.Text: "MAXDOP", Value: null }, { Name.Text: "no_infomsgs", Value: null }] },
			"Join" => made is Ast.Statement.Dbcc { Options: [{ Name.Text: "stat_header", Words: [{ Text: "JOIN" }, { Text: "density_vector" }, { Text: "JOIN" }, { Text: "stat_header" }] }] },
			"Wait" => made is Ast.Statement.Dbcc { Options: [{ Name.Text: "WAIT_AT_LOW_PRIORITY", Options.Count: 0 }, { Name.Text: "wait_at_low_priority", Options: [{ Name.Text: "max_duration", Value: Ast.Expression.Measured { Unit.Text: "minutes" } }, { Name.Text: "abort_after_wait", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "foo" }] } }] }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	[Theory]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = 1, 3 TO 5, 8",              "Affinity of processors")]
	[InlineData("alter server configuration set process affinity cpu = auto",                      "Affinity automatic")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY NUMANODE = 0 TO 3, 5",            "Affinity of nodes")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG ON",                               "Log on")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG PATH = N'x'",                      "Log path")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_SIZE = 10 mb",                 "Log size")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_FILES = DEFAULT",              "Log files")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY verboselogging = 2",     "Cluster property")]
	[InlineData("ALTER SERVER CONFIGURATION SET HADR CLUSTER CONTEXT = LOCAL",                     "Cluster context")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION OFF",                        "Buffer pool off")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = 'f', SIZE = 50 GB)", "Buffer pool on")]
	[InlineData("ALTER SERVER CONFIGURATION SET SOFTNUMA off",                                     "Soft NUMA")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED ON",                              "Memory-optimized")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED TEMPDB_METADATA = ON (RESOURCE_POOL = 'p')", "Tempdb metadata")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED HYBRID_BUFFER_POOL = OFF",        "Hybrid buffer pool")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (GROUP = ([db1], db2), MODE = COPY_ONLY)", "Snapshot backup")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = OFF",                "Snapshot backup off")]
	[InlineData("ALTER SERVER CONFIGURATION SET EXTERNAL AUTHENTICATION ON (CREDENTIAL_NAME = N'c')", "Authentication by a credential")]
	[InlineData("ALTER SERVER CONFIGURATION SET EXTERNAL AUTHENTICATION ON (USE_IDENTITY)",        "Authentication by the identity")]
	public void The_bridge_builds_the_configuration_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(OldTree.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Affinity of processors" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "PROCESS", Words: [{ Text: "AFFINITY" }, { Text: "CPU" }], EqualsOmitted: false, Value: Ast.Expression.ServerRanges { Ranges: [{ To: null }, { To: not null }, { To: null }] } } },
			"Affinity automatic" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "process", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "auto" }] } } },
			"Affinity of nodes" => made is Ast.Statement.AlterServerConfiguration { Setting: { Words: [{ Text: "AFFINITY" }, { Text: "NUMANODE" }], Value: Ast.Expression.ServerRanges } },
			"Log on" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "DIAGNOSTICS", Words: [{ Text: "LOG" }], Value: Ast.Expression.Reference { Name.Parts: [{ Text: "ON" }] }, EqualsOmitted: false } },
			"Log path" => made is Ast.Statement.AlterServerConfiguration { Setting: { Words: [{ Text: "LOG" }, { Text: "PATH" }], Value: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } } },
			"Log size" => made is Ast.Statement.AlterServerConfiguration { Setting: { Words: [{ Text: "LOG" }, { Text: "MAX_SIZE" }], Value: Ast.Expression.Measured { Unit.Text: "mb" } } },
			"Log files" => made is Ast.Statement.AlterServerConfiguration { Setting: { Words: [{ Text: "LOG" }, { Text: "MAX_FILES" }], Value: Ast.Expression.Reference } },
			"Cluster property" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "FAILOVER", Words: [{ Text: "CLUSTER" }, { Text: "PROPERTY" }, { Text: "verboselogging" }], Value: Ast.LiteralValue.Numeric } },
			"Cluster context" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "HADR", Words: [{ Text: "CLUSTER" }, { Text: "CONTEXT" }] } },
			"Buffer pool off" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "BUFFER", Words: [{ Text: "POOL" }, { Text: "EXTENSION" }], Value: Ast.Expression.Reference { Name.Parts: [{ Text: "OFF" }] }, Options.Count: 0 } },
			"Buffer pool on" => made is Ast.Statement.AlterServerConfiguration { Setting: { Value: Ast.Expression.Reference { Name.Parts: [{ Text: "ON" }] }, Options: [{ Name.Text: "FILENAME", Value: Ast.LiteralValue.String }, { Name.Text: "SIZE", Value: Ast.Expression.Measured { Unit.Text: "GB" } }] } },
			"Soft NUMA" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "SOFTNUMA", Words.Count: 0, Value: Ast.Expression.Reference { Name.Parts: [{ Text: "off" }] } } },
			"Memory-optimized" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "MEMORY_OPTIMIZED", Words.Count: 0 } },
			"Tempdb metadata" => made is Ast.Statement.AlterServerConfiguration { Setting: { Words: [{ Text: "TEMPDB_METADATA" }], Options: [{ Name.Text: "RESOURCE_POOL", Value: Ast.LiteralValue.String }] } },
			"Hybrid buffer pool" => made is Ast.Statement.AlterServerConfiguration { Setting: { Words: [{ Text: "HYBRID_BUFFER_POOL" }], Options.Count: 0 } },
			"Snapshot backup" => made is Ast.Statement.AlterServerConfiguration
			{
				Setting: { Name.Text: "SUSPEND_FOR_SNAPSHOT_BACKUP", Options: [{ Name.Text: "GROUP", Value: Ast.Expression.Row { RowKeyword: false, Items: [Ast.Expression.Reference { Name.Parts: [{ Style: Ast.IdentifierStyle.Bracketed }] }, Ast.Expression.Reference] } }, { Name.Text: "MODE", Value: Ast.Expression.Reference }] },
			},
			"Snapshot backup off" => made is Ast.Statement.AlterServerConfiguration { Setting: { Value: Ast.Expression.Reference { Name.Parts: [{ Text: "OFF" }] }, Options.Count: 0 } },
			"Authentication by a credential" => made is Ast.Statement.AlterServerConfiguration { Setting: { Name.Text: "EXTERNAL", Words: [{ Text: "AUTHENTICATION" }], Options: [{ Name.Text: "CREDENTIAL_NAME", Value: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } }] } },
			"Authentication by the identity" => made is Ast.Statement.AlterServerConfiguration { Setting: { Options: [{ Name.Text: "USE_IDENTITY", Value: null }] } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	[Theory]
	[InlineData("ALTER RESOURCE GOVERNOR RECONFIGURE",                                   "Reconfigure")]
	[InlineData("alter resource governor reconfigure reconfigure",                      "Reconfigure twice")]
	[InlineData("ALTER RESOURCE GOVERNOR DISABLE",                                       "Disable")]
	[InlineData("ALTER RESOURCE GOVERNOR RESET STATISTICS RECONFIGURE",                  "Reset")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = \"dbo\".[f]) RECONFIGURE", "Classifier")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (classifier_function = null)",            "No classifier")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = .5)",     "I/O limit")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = DEFAULT)", "I/O limit by default")]
	[InlineData("CREATE RESOURCE POOL p",                                                "Create pool")]
	[InlineData("ALTER RESOURCE POOL p WITH (MAX_CPU_PERCENT = 20)",                     "Alter pool")]
	[InlineData("create resource pool 'p''q' with (affinity scheduler = auto, affinity numanode = (0 to 3, 5))", "Pool by a string and its affinity")]
	[InlineData("CREATE RESOURCE POOL [default] WITH (MAX_CPU_PERCENT = 10, MAX_CPU_PERCENT = 20)", "Pool setting twice")]
	[InlineData("CREATE WORKLOAD GROUP g",                                               "Create group")]
	[InlineData("ALTER WORKLOAD GROUP g",                                                "Alter group")]
	[InlineData("ALTER WORKLOAD GROUP g WITH (IMPORTANCE = high, GROUP_MIN_MEMORY_PERCENT = NULL, REQUEST_MAX_MEMORY_GRANT_PERCENT = 25.5) USING p, external [e]", "Group with all")]
	[InlineData("CREATE WORKLOAD GROUP \"default\" USING \"default\"",                "Group in quotes")]
	public void The_bridge_builds_the_governor_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(OldTree.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Reconfigure" => made is Ast.Statement.AlterResourceGovernor { Action: Ast.ResourceGovernorAction.Reconfigure, ReconfigureAfter: false },
			"Reconfigure twice" => made is Ast.Statement.AlterResourceGovernor { Action: Ast.ResourceGovernorAction.Reconfigure, ReconfigureAfter: true },
			"Disable" => made is Ast.Statement.AlterResourceGovernor { Action: Ast.ResourceGovernorAction.Disable },
			"Reset" => made is Ast.Statement.AlterResourceGovernor { Action: Ast.ResourceGovernorAction.ResetStatistics, ReconfigureAfter: true },
			"Classifier" => made is Ast.Statement.AlterResourceGovernor { Action: Ast.ResourceGovernorAction.Settings { Setting: { Name.Text: "CLASSIFIER_FUNCTION", Value: Ast.Expression.Reference { Name.Parts: [{ Style: Ast.IdentifierStyle.Delimited }, { Style: Ast.IdentifierStyle.Bracketed }] } } }, ReconfigureAfter: true },
			"No classifier" => made is Ast.Statement.AlterResourceGovernor { Action: Ast.ResourceGovernorAction.Settings { Setting: { Name.Text: "classifier_function", Value: Ast.LiteralValue.Null } } },
			"I/O limit" => made is Ast.Statement.AlterResourceGovernor { Action: Ast.ResourceGovernorAction.Settings { Setting: { Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Decimal } } } },
			"I/O limit by default" => made is Ast.Statement.AlterResourceGovernor { Action: Ast.ResourceGovernorAction.Settings { Setting: { Value: Ast.Expression.Reference { Name.Parts: [{ Text: "DEFAULT" }] } } } },
			"Create pool" => made is Ast.Statement.CreateResourcePool { Name.Text: "p", Options.Count: 0 },
			"Alter pool" => made is Ast.Statement.AlterResourcePool { Options: [{ Name.Text: "MAX_CPU_PERCENT", Value: Ast.LiteralValue.Numeric }] },
			"Pool by a string and its affinity" => made is Ast.Statement.CreateResourcePool
			{
				Name: { Text: "p'q", Style: Ast.IdentifierStyle.String },
				Options: [{ Name.Text: "affinity", Words: [{ Text: "scheduler" }], Value: Ast.Expression.Reference }, { Words: [{ Text: "numanode" }], Value: Ast.Expression.PoolRanges { Ranges: [{ To: not null }, { To: null }] } }],
			},
			"Pool setting twice" => made is Ast.Statement.CreateResourcePool { Name.Style: Ast.IdentifierStyle.Bracketed, Options: [{ Name.Text: "MAX_CPU_PERCENT" }, { Name.Text: "MAX_CPU_PERCENT" }] },
			"Create group" => made is Ast.Statement.CreateWorkloadGroup { Options.Count: 0, Pools.Count: 0 },
			"Alter group" => made is Ast.Statement.AlterWorkloadGroup { Options.Count: 0, Pools.Count: 0 },
			"Group with all" => made is Ast.Statement.AlterWorkloadGroup
			{
				Options: [{ Name.Text: "IMPORTANCE" }, { Value: Ast.LiteralValue.Null }, { Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Decimal } }],
				Pools: [{ Name.Text: "p", External: false }, { Name.Style: Ast.IdentifierStyle.Bracketed, External: true }],
			},
			"Group in quotes" => made is Ast.Statement.CreateWorkloadGroup { Name.Style: Ast.IdentifierStyle.Delimited, Pools: [{ Name.Style: Ast.IdentifierStyle.Delimited, External: false }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	[Theory]
	[InlineData("CREATE AVAILABILITY GROUP g FOR REPLICA ON 'n' WITH (ENDPOINT_URL = 'u')",                   "Create with replicas")]
	[InlineData("CREATE AVAILABILITY GROUP g WITH (BASIC, db_failover = on) FOR DATABASE a, b REPLICA ON 'n' WITH (ENDPOINT_URL = 'u', SECONDARY_ROLE (ALLOW_CONNECTIONS = READ_ONLY), PRIMARY_ROLE (READ_ONLY_ROUTING_LIST = ('x', N'y'))), 'm' WITH (SEEDING_MODE = MANUAL) LISTENER 'd' (WITH IP (('1.2.3.4', '255.255.255.0'), ('::1')), PORT = 1433)", "Create with all")]
	[InlineData("CREATE AVAILABILITY GROUP g WITH (DISTRIBUTED) AVAILABILITY GROUP ON 'a' WITH (LISTENER_URL = 'u'), 'b' WITH (LISTENER = 'l')", "Create distributed")]
	[InlineData("create availability group g for replica on 'n' with (endpoint_url = 'u') listener 'd' (with dhcp on ('1.2.3.4', '255.255.255.0'))", "Create lower case, a network")]
	[InlineData("ALTER AVAILABILITY GROUP g SET (DB_FAILOVER = ON)",                                          "Alter by a setting")]
	[InlineData("ALTER AVAILABILITY GROUP g ADD DATABASE db1",                                               "Add a database")]
	[InlineData("ALTER AVAILABILITY GROUP g REMOVE REPLICA ON N'n'",                                         "Remove a replica")]
	[InlineData("ALTER AVAILABILITY GROUP g MODIFY REPLICA ON 'n' WITH (SEEDING_MODE = AUTOMATIC)",           "Modify a replica")]
	[InlineData("ALTER AVAILABILITY GROUP g JOIN AVAILABILITY GROUP ON 'a' WITH (SEEDING_MODE = MANUAL)",    "Join a group")]
	[InlineData("ALTER AVAILABILITY GROUP g JOIN",                                                           "Join")]
	[InlineData("alter availability group g deny create any database",                                       "Deny")]
	[InlineData("ALTER AVAILABILITY GROUP g GRANT CREATE ANY DATABASE",                                      "Grant")]
	[InlineData("alter availability group g failover",                                                       "Failover")]
	[InlineData("ALTER AVAILABILITY GROUP g FORCE_FAILOVER_ALLOW_DATA_LOSS",                                 "Force")]
	[InlineData("ALTER AVAILABILITY GROUP g OFFLINE",                                                        "Offline")]
	[InlineData("ALTER AVAILABILITY GROUP g ADD LISTENER 'd' (WITH DHCP)",                                   "Add a listener")]
	[InlineData("ALTER AVAILABILITY GROUP g MODIFY LISTENER 'd' (ADD IP ('1.2.3.4', '255.255.255.0'))",      "Modify a listener by an address")]
	[InlineData("ALTER AVAILABILITY GROUP g MODIFY LISTENER 'd' (PORT = 1433)",                              "Modify a listener by its port")]
	[InlineData("ALTER AVAILABILITY GROUP g RESTART LISTENER 'd'",                                           "Restart")]
	[InlineData("ALTER AVAILABILITY GROUP g REMOVE LISTENER 'd'",                                            "Remove a listener")]
	public void The_bridge_builds_the_availability_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(OldTree.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Create with replicas" => made is Ast.Statement.CreateAvailabilityGroup { Source: Ast.AvailabilitySource.Replicas { Databases.Count: 0, Servers: [{ Options: [{ Name.Text: "ENDPOINT_URL" }] }] }, Options.Count: 0, ListenerName: null, Listener: null },
			"Create with all" => made is Ast.Statement.CreateAvailabilityGroup
			{
				Options: [{ Name.Text: "BASIC", Value: null }, { Name.Text: "db_failover", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "on" }] } }],
				Source: Ast.AvailabilitySource.Replicas
				{
					Databases.Count: 2,
					Servers:
					[
						{ Options: [_, { Name.Text: "SECONDARY_ROLE", Value: null, Options: [{ Name.Text: "ALLOW_CONNECTIONS" }] }, { Name.Text: "PRIMARY_ROLE", Options: [{ Value: Ast.Expression.Row { RowKeyword: false, Items: [Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.Character }, Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National }] } }] }] },
						{ Options: [{ Name.Text: "SEEDING_MODE" }] },
					],
				},
				ListenerName: not null, Listener: Ast.AvailabilityListener.Ip { Addresses: [{ Mask: not null }, { Mask: null }], Port.Text: "1433" },
			},
			"Create distributed" => made is Ast.Statement.CreateAvailabilityGroup { Options: [{ Name.Text: "DISTRIBUTED" }], Source: Ast.AvailabilitySource.Groups { Specs: [{ Options: [{ Name.Text: "LISTENER_URL" }] }, { Options: [{ Name.Text: "LISTENER" }] }] } },
			"Create lower case, a network" => made is Ast.Statement.CreateAvailabilityGroup { Listener: Ast.AvailabilityListener.Dhcp { On: { Mask: not null } } },
			"Alter by a setting" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.Set { Setting.Name.Text: "DB_FAILOVER" } },
			"Add a database" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.AddDatabase { Database.Text: "db1" } },
			"Remove a replica" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.RemoveReplica { Server.Kind: Ast.StringLiteralKind.National } },
			"Modify a replica" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.ModifyReplica },
			"Join a group" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.JoinAvailabilityGroup { Specs.Count: 1 } },
			"Join" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.Join },
			"Deny" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.CreateAnyDatabase { Permission: Ast.AvailabilityPermission.Deny } },
			"Grant" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.CreateAnyDatabase { Permission: Ast.AvailabilityPermission.Grant } },
			"Failover" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.Failover },
			"Force" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.ForceFailoverAllowDataLoss },
			"Offline" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.Offline },
			"Add a listener" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.AddListener { Listener: Ast.AvailabilityListener.Dhcp { On: null } } },
			"Modify a listener by an address" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.ModifyListener { Change: Ast.ListenerChange.AddIp { Address.Mask: not null } } },
			"Modify a listener by its port" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.ModifyListener { Change: Ast.ListenerChange.Port { Number.Text: "1433" } } },
			"Restart" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.RestartListener },
			"Remove a listener" => made is Ast.Statement.AlterAvailabilityGroup { Action: Ast.AvailabilityAction.RemoveListener },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	/// <summary>
	/// A text the grammar reads and the records do not hold is a gap that says so, never another record: the body of the other verb
	/// (the grammar reads an action after CREATE and a group's makings after ALTER).
	/// </summary>
	[Theory]
	[InlineData("CREATE AVAILABILITY GROUP g ADD DATABASE d")]
	[InlineData("ALTER AVAILABILITY GROUP g FOR REPLICA ON 'n' WITH (ENDPOINT_URL = 'u')")]
	[InlineData("CREATE AVAILABILITY GROUP g FAILOVER")]
	public void An_availability_body_of_the_other_verb_is_a_gap(string text)
	{
		var old = OldTree.Located.ParseStatement(text);

		Assert.Throws<BridgeGap>(() => Bridge.ToAst(old, text));
	}

	[Theory]
	[InlineData("CREATE ENDPOINT e",                                                                  "Create bare")]
	[InlineData("ALTER ENDPOINT e",                                                                   "Alter bare")]
	[InlineData("create endpoint e authorization o state = started, affinity = none as tcp (listener_port = 4022, listener_ip = all) for tsql ()", "Create with all, lower case")]
	[InlineData("ALTER ENDPOINT e AFFINITY = ADMIN",                                                  "Alter by an affinity")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_IP = (1 . 2 .3 . 4 : 255 . 0.0 .0), LISTENER_PORT = -1)", "Addresses")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_IP = ('a', N'b'))",                                "Address strings")]
	[InlineData("CREATE ENDPOINT e AS HTTP (PATH = '/p', AUTHENTICATION = (BASIC, ntlm), PORTS = (ssl), CLEAR_PORT = 1, AUTH_REALM = none, COMPRESSION = ENABLED) FOR TSQL ()", "Http")]
	[InlineData("CREATE ENDPOINT e FOR SERVICE_BROKER (ENCRYPTION = supported ALGORITHM rc4 aes, AUTHENTICATION = WINDOWS NTLM CERTIFICATE c, MESSAGE_FORWARD_SIZE = -1)", "Broker")]
	[InlineData("CREATE ENDPOINT e FOR DATABASE_MIRRORING (ROLE = ALL, AUTHENTICATION = CERTIFICATE [c d] WINDOWS KERBEROS)", "Mirroring")]
	[InlineData("CREATE ENDPOINT e FOR data_mirroring (ROLE = PARTNER)",                              "Data mirroring")]
	[InlineData("CREATE ENDPOINT e FOR TSQL () AS TCP (LISTENER_PORT = 1)",                           "Transport last")]
	[InlineData("ALTER ENDPOINT e FOR SERVICE_BROKER () AS HTTP (PATH = 'p')",                        "Alter, transport last")]
	[InlineData("CREATE ENDPOINT e FOR SOAP (WEBMETHOD 'n'.'m' (NAME = 'd.dbo.p', SCHEMA = STANDARD), WSDL = NONE, SESSION_TIMEOUT = NEVER)", "Soap")]
	[InlineData("ALTER ENDPOINT e FOR SOAP (ADD WEBMETHOD 'm' (NAME = 'p'), ALTER WEBMETHOD 'n' (FORMAT = NONE), DROP WEBMETHOD 'o')", "Soap, altered")]
	[InlineData("CREATE ENDPOINT e FOR SOAP ()",                                                      "Soap, empty")]
	public void The_bridge_builds_the_endpoint_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(OldTree.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Create bare" => made is Ast.Statement.CreateEndpoint { Name.Text: "e", Authorization: null, Settings.Count: 0, Transport: null, Payload: null, TransportLast: false },
			"Alter bare" => made is Ast.Statement.AlterEndpoint { Settings.Count: 0, Transport: null, Payload: null },
			"Create with all, lower case" => made is Ast.Statement.CreateEndpoint
			{
				Authorization.Text: "o", Settings: [{ Name.Text: "state" }, { Name.Text: "affinity" }],
				Transport: Ast.EndpointTransport.Tcp { Options: [{ Name.Text: "listener_port", Value: Ast.LiteralValue.Numeric { Text: "4022" } }, { Name.Text: "listener_ip", Value: Ast.Expression.Reference }] },
				Payload: Ast.EndpointPayload.Tsql { Options.Count: 0 }, TransportLast: false,
			},
			"Alter by an affinity" => made is Ast.Statement.AlterEndpoint { Settings: [{ Name.Text: "AFFINITY" }] },
			"Addresses" => made is Ast.Statement.CreateEndpoint { Transport: Ast.EndpointTransport.Tcp { Options: [{ Value: Ast.Expression.EndpointAddress { Address: "1.2.3.4", Mask: "255.0.0.0" } }, { Value: Ast.LiteralValue.Numeric { Text: "-1" } }] } },
			"Address strings" => made is Ast.Statement.CreateEndpoint { Transport: Ast.EndpointTransport.Tcp { Options: [{ Value: Ast.Expression.Row { Items: [Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.Character }, Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National }] } }] } },
			"Http" => made is Ast.Statement.CreateEndpoint { Transport: Ast.EndpointTransport.Http { Options: [{ Name.Text: "PATH" }, { Value: Ast.Expression.Row { Items.Count: 2 } }, { Value: Ast.Expression.Row { Items.Count: 1 } }, { Value: Ast.LiteralValue.Numeric }, { Value: Ast.Expression.Reference }, { Value: Ast.Expression.Reference }] } },
			"Broker" => made is Ast.Statement.CreateEndpoint
			{
				Payload: Ast.EndpointPayload.ServiceBroker
				{
					Options:
					[
						{ Name.Text: "ENCRYPTION", Value: Ast.Expression.EndpointEncryption { Words: [{ Text: "supported" }, { Text: "ALGORITHM" }, { Text: "rc4" }, { Text: "aes" }] } },
						{ Name.Text: "AUTHENTICATION", Value: Ast.Expression.EndpointAuthentication { Words: [{ Text: "WINDOWS" }, { Text: "NTLM" }, { Text: "CERTIFICATE" }, { Text: "c" }] } },
						{ Value: Ast.LiteralValue.Numeric { Text: "-1" } },
					],
				},
			},
			"Mirroring" => made is Ast.Statement.CreateEndpoint { Payload: Ast.EndpointPayload.DatabaseMirroring { DataMirroring: false, Options: [{ Name.Text: "ROLE" }, { Value: Ast.Expression.EndpointAuthentication { Words: [{ Text: "CERTIFICATE" }, { Style: Ast.IdentifierStyle.Bracketed }, { Text: "WINDOWS" }, { Text: "KERBEROS" }] } }] } },
			"Data mirroring" => made is Ast.Statement.CreateEndpoint { Payload: Ast.EndpointPayload.DatabaseMirroring { DataMirroring: true } },
			"Transport last" => made is Ast.Statement.CreateEndpoint { Transport: Ast.EndpointTransport.Tcp, Payload: Ast.EndpointPayload.Tsql, TransportLast: true },
			"Alter, transport last" => made is Ast.Statement.AlterEndpoint { Transport: Ast.EndpointTransport.Http, Payload: Ast.EndpointPayload.ServiceBroker, TransportLast: true },
			"Soap" => made is Ast.Statement.CreateEndpoint
			{
				Payload: Ast.EndpointPayload.Soap
				{
					Items:
					[
						Ast.SoapItem.WebMethod { Action: null, Namespace.Text: "'n'", Method.Text: "'m'", Options: [{ Name.Text: "NAME", Value: Ast.LiteralValue.String }, { Name.Text: "SCHEMA" }] },
						Ast.SoapItem.Setting { Value.Name.Text: "WSDL" },
						Ast.SoapItem.Setting { Value.Name.Text: "SESSION_TIMEOUT" },
					],
				},
			},
			"Soap, altered" => made is Ast.Statement.AlterEndpoint
			{
				Payload: Ast.EndpointPayload.Soap
				{
					Items: [Ast.SoapItem.WebMethod { Action: Ast.WebMethodAction.Add, Options.Count: 1 }, Ast.SoapItem.WebMethod { Action: Ast.WebMethodAction.Alter }, Ast.SoapItem.WebMethod { Action: Ast.WebMethodAction.Drop, Options.Count: 0 }],
				},
			},
			"Soap, empty" => made is Ast.Statement.CreateEndpoint { Payload: Ast.EndpointPayload.Soap { Items.Count: 0 } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	// @@TESTS
}
