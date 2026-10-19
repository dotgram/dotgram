using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Monitoring family's rules (Build.Monitoring.cs): which record and members they build for a text, where another record or a
/// flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class MonitoringRecordTests
{
	static void Converts(string text)
	{
		ReadBack.Holds(text);
	}

	[Theory]
	[InlineData("CREATE EVENT NOTIFICATION n ON SERVER FOR a TO SERVICE 's'",                    "Server")]
	[InlineData("CREATE EVENT NOTIFICATION n ON DATABASE WITH FAN_IN FOR a, b c TO SERVICE 's', 'b'", "Database fanned in")]
	[InlineData("CREATE EVENT NOTIFICATION n ON QUEUE db.dbo.q FOR a TO SERVICE 's'",            "Queue")]
	[InlineData("create event notification n on queue q with fan_in for Alter_Table to service 's'", "Lower case")]
	public void The_rules_build_the_notification_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		var holds = expected switch
		{
			"Server" => made is Ast.Statement.CreateEventNotification { On: Ast.NotificationScope.Server, FanIn: false, Events: [{ Words: [{ Text: "a" }] }], Services: [Ast.LiteralValue.String] },
			"Database fanned in" => made is Ast.Statement.CreateEventNotification { On: Ast.NotificationScope.Database, FanIn: true, Events: [{ Words.Count: 1 }, { Words.Count: 2 }], Services.Count: 2 },
			"Queue" => made is Ast.Statement.CreateEventNotification { On: Ast.NotificationScope.Queue { Name.Parts.Count: 3 } },
			"Lower case" => made is Ast.Statement.CreateEventNotification { On: Ast.NotificationScope.Queue, FanIn: true, Events: [{ Words: [{ Text: "Alter_Table" }] }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	[Theory]
	[InlineData("CREATE SERVER AUDIT a TO APPLICATION_LOG",                                      "Application log")]
	[InlineData("CREATE SERVER AUDIT a TO SECURITY_LOG WITH (QUEUE_DELAY = 1) WHERE c = 1",      "Security log with all")]
	[InlineData("CREATE SERVER AUDIT a TO URL",                                                  "Url")]
	[InlineData("CREATE SERVER AUDIT a TO EXTERNAL_MONITOR",                                     "Monitor")]
	[InlineData("CREATE SERVER AUDIT a TO FILE (FILEPATH = 'p', MAXSIZE = 15 TB, MAXSIZE = UNLIMITED, MAX_ROLLOVER_FILES = unlimited)", "File")]
	[InlineData("ALTER SERVER AUDIT a TO FILE (FILEPATH = 'p')",                                 "Alter to a file")]
	[InlineData("ALTER SERVER AUDIT a REMOVE WHERE",                                             "Remove where")]
	[InlineData("ALTER SERVER AUDIT a MODIFY NAME = b",                                          "Modify name")]
	[InlineData("ALTER SERVER AUDIT a WITH (STATE = OFF)",                                       "Alter settings")]
	[InlineData("ALTER SERVER AUDIT a WHERE c = 1",                                              "Alter condition")]
	[InlineData("alter server audit a to application_log with (on_failure = shutdown) where c = 1", "Alter lower case")]
	public void The_rules_build_the_audit_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		var holds = expected switch
		{
			"Application log" => made is Ast.Statement.CreateServerAudit { Target: Ast.AuditTarget.ApplicationLog, Options.Count: 0, Where: null },
			"Security log with all" => made is Ast.Statement.CreateServerAudit { Target: Ast.AuditTarget.SecurityLog, Options: [{ Name.Text: "QUEUE_DELAY" }], Where: Ast.Expression.Comparison },
			"Url" => made is Ast.Statement.CreateServerAudit { Target: Ast.AuditTarget.Url },
			"Monitor" => made is Ast.Statement.CreateServerAudit { Target: Ast.AuditTarget.ExternalMonitor },
			"File" => made is Ast.Statement.CreateServerAudit
			{
				Target: Ast.AuditTarget.File
				{
					Options: [{ Name.Text: "FILEPATH" }, { Value: Ast.Expression.Measured { Unit.Text: "TB" } }, { Value: Ast.Expression.Reference { Name.Parts: [{ Text: "UNLIMITED" }] } }, { Name.Text: "MAX_ROLLOVER_FILES", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "unlimited" }] } }],
				},
			},
			"Alter to a file" => made is Ast.Statement.AlterServerAudit { Change: Ast.AuditChange.Settings { Target: Ast.AuditTarget.File, Options.Count: 0, Where: null } },
			"Remove where" => made is Ast.Statement.AlterServerAudit { Change: Ast.AuditChange.RemoveWhere },
			"Modify name" => made is Ast.Statement.AlterServerAudit { Change: Ast.AuditChange.ModifyName { Name.Text: "b" } },
			"Alter settings" => made is Ast.Statement.AlterServerAudit { Change: Ast.AuditChange.Settings { Target: null, Options: [{ Name.Text: "STATE" }], Where: null } },
			"Alter condition" => made is Ast.Statement.AlterServerAudit { Change: Ast.AuditChange.Settings { Target: null, Options.Count: 0, Where: Ast.Expression.Comparison } },
			"Alter lower case" => made is Ast.Statement.AlterServerAudit { Change: Ast.AuditChange.Settings { Target: Ast.AuditTarget.ApplicationLog, Options: [{ Name.Text: "on_failure" }], Where: not null } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	/// <summary>
	/// A size written against its unit (<c>1MB</c>) is one token to the tokens a definition's text is compared by and two to the grammar,
	/// and the tree writes the unit after a blank: a gap that says so.
	/// </summary>
	[Theory]
	[InlineData("CREATE SERVER AUDIT a TO FILE (FILEPATH = 'p', MAXSIZE = 1MB)")]
	public void A_text_the_records_do_not_hold_is_a_gap(string text)
	{
		Assert.IsType<DotGram.Sql.Ast.Statement.Extension>(TransactSqlParser.Located.ParseStatement(text));
	}

	[Theory]
	[InlineData("CREATE SERVER AUDIT SPECIFICATION s",                                           "Server create")]
	[InlineData("ALTER SERVER AUDIT SPECIFICATION s",                                            "Server alter")]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s",                                         "Database create")]
	[InlineData("ALTER DATABASE AUDIT SPECIFICATION s",                                          "Database alter")]
	[InlineData("CREATE SERVER AUDIT SPECIFICATION s FOR SERVER AUDIT a.b ADD (g) DROP (h),",    "Audit, an action and a comma")]
	[InlineData("ALTER DATABASE AUDIT SPECIFICATION s ADD (SELECT, insert ON OBJECT::t BY dbo, public, null, [null]) WITH (STATE = OFF)", "Action on")]
	[InlineData("CREATE SERVER AUDIT SPECIFICATION s ADD (a b, c), DROP (d)",                     "Words and commas")]
	[InlineData("alter server audit specification s for server audit a add (g) with (state = on, state = off)", "Lower case")]
	public void The_rules_build_the_specification_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		var holds = expected switch
		{
			"Server create" => made is Ast.Statement.CreateServerAuditSpecification { Name.Text: "s", Audit: null, Actions.Count: 0, Options.Count: 0 },
			"Server alter" => made is Ast.Statement.AlterServerAuditSpecification { Name.Text: "s" },
			"Database create" => made is Ast.Statement.CreateDatabaseAuditSpecification { Name.Text: "s" },
			"Database alter" => made is Ast.Statement.AlterDatabaseAuditSpecification { Name.Text: "s" },
			"Audit, an action and a comma" => made is Ast.Statement.CreateServerAuditSpecification
			{
				Audit.Parts.Count: 2, Actions: [{ Action: Ast.AddOrDrop.Add, Comma: false }, { Action: Ast.AddOrDrop.Drop, Comma: true }],
			},
			"Action on" => made is Ast.Statement.AlterDatabaseAuditSpecification
			{
				Actions: [{ Actions: [{ Words: [{ Text: "SELECT" }] }, { Words: [{ Text: "insert" }] }], On: { Class: Ast.SecurableClass.Object, Name.Parts: [{ Text: "t" }], Grantees: [Ast.Grantee.Identifier, Ast.Grantee.Public, Ast.Grantee.Null, Ast.Grantee.Identifier { Value.Name.Style: Ast.IdentifierStyle.Bracketed }] } }],
				Options: [{ Name.Text: "STATE" }],
			},
			"Words and commas" => made is Ast.Statement.CreateServerAuditSpecification { Actions: [{ Actions: [{ Words.Count: 2 }, { Words.Count: 1 }], Comma: true }, { Action: Ast.AddOrDrop.Drop, Comma: false }] },
			"Lower case" => made is Ast.Statement.AlterServerAuditSpecification { Audit: not null, Actions: [{ Action: Ast.AddOrDrop.Add }], Options: [{ Name.Text: "state" }, { Name.Text: "state" }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	[Theory]
	[InlineData("CREATE EVENT SESSION s ON SERVER ADD EVENT e",                                  "Create")]
	[InlineData("ALTER EVENT SESSION s ON SERVER ADD EVENT e",                                   "Alter adds an event")]
	[InlineData("ALTER EVENT SESSION s ON DATABASE STATE = start",                               "State")]
	[InlineData("ALTER EVENT SESSION s ON SERVER WITH (TRACK_CAUSALITY = ON)",                   "Options alone")]
	[InlineData("ALTER EVENT SESSION s ON SERVER DROP TARGET t",                                 "Drop target")]
	[InlineData("CREATE EVENT SESSION s ON SERVER ADD EVENT e (SET a = -1, b = (2), c = ((0x10)), d = N'x', e = 1.5e3 ACTION (p.a, b) WHERE x > 1) ADD TARGET t (SET f = 'g') WITH (MAX_MEMORY = 4096 KB, MAX_DISPATCH_LATENCY = INFINITE)", "All")]
	[InlineData("create event session s on server add event e () , add event f",                  "Empty brackets")]
	[InlineData("CREATE EVENT SESSION s ON SERVER ADD EVENT e (SET a = 1, a = 2)",               "A setting twice")]
	public void The_rules_build_the_session_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		var holds = expected switch
		{
			"Create" => made is Ast.Statement.CreateEventSession { On: Ast.EventScope.Server, Pieces: [Ast.EventSessionPiece.AddEvent { Event.Parts: [{ Text: "e" }], Settings.Count: 0, Actions.Count: 0, Where: null }], Options.Count: 0 },
			"Alter adds an event" => made is Ast.Statement.AlterEventSession { Pieces: [Ast.EventSessionPiece.AddEvent], State: null },
			"State" => made is Ast.Statement.AlterEventSession { On: Ast.EventScope.Database, Pieces.Count: 0, State: Ast.EventSessionState.Start },
			"Options alone" => made is Ast.Statement.AlterEventSession { Pieces.Count: 0, Options: [{ Name.Text: "TRACK_CAUSALITY" }], State: null },
			"Drop target" => made is Ast.Statement.AlterEventSession { Pieces: [Ast.EventSessionPiece.DropTarget] },
			"All" => made is Ast.Statement.CreateEventSession
			{
				Pieces:
				[
					Ast.EventSessionPiece.AddEvent
					{
						Settings:
						[
							{ Name.Text: "a", Value: Ast.LiteralValue.Numeric { Text: "-1" } },
							{ Value: Ast.Expression.Parenthesized { Value: Ast.LiteralValue.Numeric { Text: "2" } } },
							{ Value: Ast.Expression.Parenthesized { Value: Ast.Expression.Parenthesized { Value: Ast.LiteralValue.Binary { ZeroX: true } } } },
							{ Value: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } },
							{ Value: Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Approximate } },
						],
						Actions: [{ Parts.Count: 2 }, { Parts.Count: 1 }],
						Where: Ast.EventPredicate.Compare { Field.Parts.Count: 1, Operator: Ast.ComparisonOperator.Greater },
					},
					Ast.EventSessionPiece.AddTarget { Settings: [{ Name.Text: "f" }] },
				],
				Options: [{ Value: Ast.Expression.Measured { Unit.Text: "KB" } }, { Value: Ast.Expression.Reference }],
			},
			"Empty brackets" => made is Ast.Statement.CreateEventSession { Pieces: [Ast.EventSessionPiece.AddEvent { Settings.Count: 0, Where: null }, Ast.EventSessionPiece.AddEvent] },
			"A setting twice" => made is Ast.Statement.CreateEventSession { Pieces: [Ast.EventSessionPiece.AddEvent { Settings: [{ Name.Text: "a" }, { Name.Text: "a" }] }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	/// <summary>
	/// An event's predicate: terms joined from the left, NOT before a term, brackets around any predicate; the comparisons and the
	/// spelling of an inequality, a LIKE and a NOT LIKE, and a call.
	/// </summary>
	[Theory]
	[InlineData("a = 1",                              "Compare")]
	[InlineData("a = 1 AND b = 2 OR c = 3",           "Joined from the left")]
	[InlineData("a = 1 OR (b = 2 AND c = 3)",         "Brackets on the right")]
	[InlineData("(a = 1 OR b = 2) AND c = 3",         "Brackets on the left")]
	[InlineData("NOT NOT a = 1 AND NOT b = 2",        "Negations")]
	[InlineData("NOT (a = 1 OR b = 2)",               "Negated brackets")]
	[InlineData("a <> 1",                             "Inequality")]
	[InlineData("a != 1",                             "Inequality with a bang")]
	[InlineData("a !< 1 AND a !> 1 AND a >= 1 AND a <= 1", "Not less, not greater, and the rest")]
	[InlineData("a LIKE 'x'",                         "Like")]
	[InlineData("a NOT LIKE 'x'",                     "Not like")]
	[InlineData("p.f (a, -1.5)",                      "Call")]
	[InlineData("a = ((1)) AND b = - 2 AND c = 0x10", "Values")]
	[InlineData("a.b.c = 'x' and not p.q.f (a.b, 3)", "Lower case")]
	public void The_rules_build_the_predicate_the_text_means(string predicate, string expected)
	{
		var text = $"CREATE EVENT SESSION s ON SERVER ADD EVENT e (WHERE {predicate})";
		var made = TransactSqlParser.Located.ParseStatement(text);
		var held = made is Ast.Statement.CreateEventSession { Pieces: [Ast.EventSessionPiece.AddEvent { Where: { } where }] } ? where : null;

		var holds = expected switch
		{
			"Compare" => held is Ast.EventPredicate.Compare { Operator: Ast.ComparisonOperator.Equal, Exclamation: false },
			"Joined from the left" => held is Ast.EventPredicate.Join { Left: Ast.EventPredicate.Join { Operator: Ast.EventJoin.And }, Operator: Ast.EventJoin.Or, Right: Ast.EventPredicate.Compare },
			"Brackets on the right" => held is Ast.EventPredicate.Join { Left: Ast.EventPredicate.Compare, Operator: Ast.EventJoin.Or, Right: Ast.EventPredicate.Parenthesized { Predicate: Ast.EventPredicate.Join { Operator: Ast.EventJoin.And } } },
			"Brackets on the left" => held is Ast.EventPredicate.Join { Left: Ast.EventPredicate.Parenthesized, Operator: Ast.EventJoin.And },
			"Negations" => held is Ast.EventPredicate.Join { Left: Ast.EventPredicate.Not { Operand: Ast.EventPredicate.Not { Operand: Ast.EventPredicate.Compare } }, Right: Ast.EventPredicate.Not { Operand: Ast.EventPredicate.Compare } },
			"Negated brackets" => held is Ast.EventPredicate.Not { Operand: Ast.EventPredicate.Parenthesized },
			"Inequality" => held is Ast.EventPredicate.Compare { Operator: Ast.ComparisonOperator.NotEqual, Exclamation: false },
			"Inequality with a bang" => held is Ast.EventPredicate.Compare { Operator: Ast.ComparisonOperator.NotEqual, Exclamation: true },
			"Not less, not greater, and the rest" => held is Ast.EventPredicate.Join { Left: Ast.EventPredicate.Join { Left: Ast.EventPredicate.Join { Left: Ast.EventPredicate.Compare { Operator: Ast.ComparisonOperator.NotLess }, Right: Ast.EventPredicate.Compare { Operator: Ast.ComparisonOperator.NotGreater } }, Right: Ast.EventPredicate.Compare { Operator: Ast.ComparisonOperator.GreaterOrEqual } }, Right: Ast.EventPredicate.Compare { Operator: Ast.ComparisonOperator.LessOrEqual } },
			"Like" => held is Ast.EventPredicate.Like { Negated: false },
			"Not like" => held is Ast.EventPredicate.Like { Negated: true },
			"Call" => held is Ast.EventPredicate.Call { Name.Parts.Count: 2, Field.Parts.Count: 1, Value: Ast.LiteralValue.Numeric { Text: "-1.5" } },
			"Values" => held is Ast.EventPredicate.Join { Left: Ast.EventPredicate.Join { Left: Ast.EventPredicate.Compare { Value: Ast.Expression.Parenthesized { Value: Ast.Expression.Parenthesized } }, Right: Ast.EventPredicate.Compare { Value: Ast.LiteralValue.Numeric { Text: "-2" } } }, Right: Ast.EventPredicate.Compare { Value: Ast.LiteralValue.Binary { ZeroX: true } } },
			"Lower case" => held is Ast.EventPredicate.Join { Left: Ast.EventPredicate.Compare { Field.Parts.Count: 3 }, Operator: Ast.EventJoin.And, Right: Ast.EventPredicate.Not { Operand: Ast.EventPredicate.Call { Name.Parts.Count: 3, Field.Parts.Count: 2 } } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");
		Converts(text);
	}

	/// <summary>
	/// A text the grammar reads and the tree has no place for is a gap that says so: a binary string with a minus before it, which the
	/// grammar's <c>EventValue</c> reads.
	/// </summary>
	[Theory]
	[InlineData("CREATE EVENT SESSION s ON SERVER ADD EVENT e (WHERE a = - 0x10)")]
	[InlineData("CREATE EVENT SESSION s ON SERVER ADD EVENT e (SET a = -0x10)")]
	public void A_value_the_tree_has_no_place_for_is_a_gap(string text)
	{
		Assert.IsType<DotGram.Sql.Ast.Statement.Extension>(TransactSqlParser.Located.ParseStatement(text));
	}
}
