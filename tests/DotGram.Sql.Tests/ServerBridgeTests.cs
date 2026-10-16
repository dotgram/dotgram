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
		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

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
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

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
			Assert.Throws<BridgeGap>(() => Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));

			return;
		}

		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

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
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

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

	// @@TESTS
}
