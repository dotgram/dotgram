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

	// @@TESTS
}
