using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Broker family's bridge (Bridge.Broker.cs): which record and members it builds for a text, where another record or a flag
/// set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class BrokerBridgeTests
{
	[Theory]
	[InlineData("CREATE MESSAGE TYPE m VALIDATION = EMPTY",                                      "Create")]
	[InlineData("ALTER MESSAGE TYPE m VALIDATION = EMPTY",                                       "Alter")]
	[InlineData("CREATE MESSAGE TYPE m AUTHORIZATION o VALIDATION = FOO",                        "Create owned, a word")]
	[InlineData("CREATE MESSAGE TYPE m VALIDATION = VALID_XML WITH SCHEMA COLLECTION dbo.s",      "Valid xml")]
	[InlineData("CREATE MESSAGE TYPE m VALIDATION = [VALID_XML]",                                "Bracketed VALID_XML is a word")]
	[InlineData("create message type m validation = valid_xml with schema collection s",          "Lower case")]
	[InlineData("CREATE MESSAGE TYPE m",                                                         "Create bare")]
	[InlineData("CREATE CONTRACT c (a SENT BY INITIATOR, [b] SENT BY target, c SENT BY ANY)",      "Contract")]
	public void The_bridge_builds_the_message_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Create" => made is Ast.Statement.CreateMessageType { Name.Text: "m", Authorization: null, Validation: Ast.MessageValidation.Named { Word.Text: "EMPTY" } },
			"Alter" => made is Ast.Statement.AlterMessageType { Name.Text: "m", Validation: Ast.MessageValidation.Named { Word.Text: "EMPTY" } },
			"Create owned, a word" => made is Ast.Statement.CreateMessageType { Authorization.Text: "o", Validation: Ast.MessageValidation.Named { Word.Text: "FOO" } },
			"Valid xml" => made is Ast.Statement.CreateMessageType { Validation: Ast.MessageValidation.ValidXml { SchemaCollection.Parts: [{ Text: "dbo" }, { Text: "s" }] } },
			"Bracketed VALID_XML is a word" => made is Ast.Statement.CreateMessageType { Validation: Ast.MessageValidation.Named { Word: { Text: "[VALID_XML]", Style: Ast.IdentifierStyle.Bracketed } } },
			"Lower case" => made is Ast.Statement.CreateMessageType { Validation: Ast.MessageValidation.ValidXml },
			"Create bare" => made is Ast.Statement.CreateMessageType { Validation: null },
			"Contract" => made is Ast.Statement.CreateContract
			{
				Messages: [{ MessageType.Text: "a", SentBy: Ast.ContractSender.Initiator }, { MessageType.Style: Ast.IdentifierStyle.Bracketed, SentBy: Ast.ContractSender.Target }, { SentBy: Ast.ContractSender.Any }],
			},
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	[Theory]
	[InlineData("CREATE QUEUE q WITH STATUS = ON, RETENTION = OFF",                              "Options in order")]
	[InlineData("ALTER QUEUE q WITH STATUS = ON, RETENTION = OFF",                               "Altered options")]
	[InlineData("ALTER QUEUE q WITH ACTIVATION (DROP)",                                          "Dropped activation")]
	[InlineData("ALTER QUEUE q WITH ACTIVATION (STATUS = OFF)",                                  "Activation of a status")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (EXECUTE AS SELF, PROCEDURE_NAME = dbo.p, MAX_QUEUE_READERS = 1, STATUS = ON), POISON_MESSAGE_HANDLING (STATUS = OFF)", "Activation and poison")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (PROCEDURE_NAME = p, MAX_QUEUE_READERS = 1, EXECUTE AS 'u')", "Activation run as a user")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (PROCEDURE_NAME = p, MAX_QUEUE_READERS = 1, EXECUTE AS owner)", "Activation run as the owner")]
	[InlineData("CREATE QUEUE q ON fg",                                                          "On a name")]
	[InlineData("CREATE QUEUE q ON 'f''g'",                                                      "On a string")]
	[InlineData("CREATE QUEUE q ON N'fg'",                                                       "On a national string")]
	[InlineData("CREATE QUEUE q ON [DEFAULT]",                                                   "On a bracketed name")]
	[InlineData("ALTER QUEUE q MOVE TO fg",                                                      "Move")]
	[InlineData("ALTER QUEUE q REBUILD WITH (MAXDOP = 2)",                                       "Rebuild")]
	[InlineData("ALTER QUEUE q REORGANIZE WITH (LOB_COMPACTION = OFF)",                          "Reorganize")]
	[InlineData("create queue db..q with status = on on fg",                                     "Lower case, a left out part")]
	public void The_bridge_builds_the_queue_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Options in order" => made is Ast.Statement.CreateQueue { Options: [Ast.QueueOption.Status { On: true }, Ast.QueueOption.Retention { On: false }], On: null },
			"Altered options" => made is Ast.Statement.AlterQueue { Change: Ast.QueueChange.With { Options: [Ast.QueueOption.Status { On: true }, Ast.QueueOption.Retention { On: false }] } },
			"Dropped activation" => made is Ast.Statement.AlterQueue { Change: Ast.QueueChange.With { Options: [Ast.QueueOption.DropActivation] } },
			"Activation of a status" => made is Ast.Statement.AlterQueue { Change: Ast.QueueChange.With { Options: [Ast.QueueOption.Activation { Settings: [Ast.ActivationSetting.Status { On: false }] }] } },
			"Activation and poison" => made is Ast.Statement.CreateQueue
			{
				Options:
				[
					Ast.QueueOption.Activation
					{
						Settings:
						[
							Ast.ActivationSetting.ExecuteAs { Context: { Kind: Ast.ExecutionContextKind.Self, Name: null } },
							Ast.ActivationSetting.ProcedureName { Name.Parts: [{ Text: "dbo" }, { Text: "p" }] },
							Ast.ActivationSetting.MaxQueueReaders { Readers.Text: "1" },
							Ast.ActivationSetting.Status { On: true },
						],
					},
					Ast.QueueOption.PoisonMessageHandling { On: false },
				],
			},
			"Activation run as a user" => made is Ast.Statement.CreateQueue { Options: [Ast.QueueOption.Activation { Settings: [_, _, Ast.ActivationSetting.ExecuteAs { Context: { Kind: Ast.ExecutionContextKind.User, Name: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.Character } } }] }] },
			"Activation run as the owner" => made is Ast.Statement.CreateQueue { Options: [Ast.QueueOption.Activation { Settings: [_, _, Ast.ActivationSetting.ExecuteAs { Context: { Kind: Ast.ExecutionContextKind.Owner, Name: null } }] }] },
			"On a name" => made is Ast.Statement.CreateQueue { On: { Kind: Ast.PlacementKind.On, Target: { Text: "fg", Style: Ast.IdentifierStyle.Regular }, Columns: null } },
			"On a string" => made is Ast.Statement.CreateQueue { On.Target: { Text: "f'g", Style: Ast.IdentifierStyle.String } },
			"On a national string" => made is Ast.Statement.CreateQueue { On.Target: { Text: "fg", Style: Ast.IdentifierStyle.NationalString } },
			"On a bracketed name" => made is Ast.Statement.CreateQueue { On.Target: { Text: "[DEFAULT]", Style: Ast.IdentifierStyle.Bracketed } },
			"Move" => made is Ast.Statement.AlterQueue { Change: Ast.QueueChange.MoveTo { Target: { Kind: Ast.PlacementKind.MoveTo, Target.Text: "fg" } } },
			"Rebuild" => made is Ast.Statement.AlterQueue { Change: Ast.QueueChange.Rebuild { MaxDop.Text: "2" } },
			"Reorganize" => made is Ast.Statement.AlterQueue { Change: Ast.QueueChange.Reorganize { LobCompaction: false } },
			"Lower case, a left out part" => made is Ast.Statement.CreateQueue { Name.Parts: [{ Text: "db" }, { Text: "" }, { Text: "q" }], On.Target.Text: "fg" },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	[Theory]
	[InlineData("CREATE SERVICE s ON QUEUE q",                                                   "Create service")]
	[InlineData("CREATE SERVICE s AUTHORIZATION o ON QUEUE q (c, [d])",                          "Create service with contracts")]
	[InlineData("ALTER SERVICE s ON QUEUE q",                                                    "Alter service on a queue")]
	[InlineData("ALTER SERVICE s (ADD CONTRACT c, DROP CONTRACT [d])",                           "Alter service with changes")]
	[InlineData("alter service s on queue q (drop contract c)",                                  "Alter service lower case")]
	[InlineData("CREATE ROUTE r WITH ADDRESS = 'a'",                                             "Create route")]
	[InlineData("ALTER ROUTE r WITH ADDRESS = 'a'",                                              "Alter route")]
	[InlineData("alter route r with lifetime = 1, mirror_address = N'm'",                        "Alter route lower case")]
	[InlineData("CREATE REMOTE SERVICE BINDING b TO SERVICE 's' WITH USER = u",                  "Create binding")]
	[InlineData("ALTER REMOTE SERVICE BINDING b WITH USER = u",                                  "Alter binding")]
	[InlineData("alter remote service binding b with anonymous = off, user = [u]",               "Alter binding lower case")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION",                                     "Create priority with no SET")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (PRIORITY_LEVEL = 5)",            "Create priority")]
	[InlineData("ALTER BROKER PRIORITY p FOR CONVERSATION SET (PRIORITY_LEVEL = DEFAULT)",       "Alter priority")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (CONTRACT_NAME = ANY, LOCAL_SERVICE_NAME = [ANY], REMOTE_SERVICE_NAME = any)", "Priority ANY and [ANY]")]
	public void The_bridge_builds_the_service_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Create service" => made is Ast.Statement.CreateService { Name.Text: "s", Queue.Parts: [{ Text: "q" }], Contracts.Count: 0, Authorization: null },
			"Create service with contracts" => made is Ast.Statement.CreateService { Authorization.Text: "o", Contracts: [{ Text: "c" }, { Style: Ast.IdentifierStyle.Bracketed }] },
			"Alter service on a queue" => made is Ast.Statement.AlterService { Queue.Parts: [{ Text: "q" }], Changes.Count: 0 },
			"Alter service with changes" => made is Ast.Statement.AlterService { Queue: null, Changes: [{ Action: Ast.AddOrDrop.Add, Contract.Text: "c" }, { Action: Ast.AddOrDrop.Drop }] },
			"Alter service lower case" => made is Ast.Statement.AlterService { Queue: not null, Changes: [{ Action: Ast.AddOrDrop.Drop }] },
			"Create route" => made is Ast.Statement.CreateRoute { Options: [{ Name.Text: "ADDRESS", Value: Ast.LiteralValue.String, EqualsOmitted: false }] },
			"Alter route" => made is Ast.Statement.AlterRoute { Options: [{ Name.Text: "ADDRESS" }] },
			"Alter route lower case" => made is Ast.Statement.AlterRoute { Options: [{ Name.Text: "lifetime", Value: Ast.LiteralValue.Numeric }, { Name.Text: "mirror_address", Value: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } }] },
			"Create binding" => made is Ast.Statement.CreateRemoteServiceBinding { Service.Kind: Ast.StringLiteralKind.Character, Options: [{ Name.Text: "USER", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "u" }] } }] },
			"Alter binding" => made is Ast.Statement.AlterRemoteServiceBinding { Options: [{ Name.Text: "USER" }] },
			"Alter binding lower case" => made is Ast.Statement.AlterRemoteServiceBinding { Options: [{ Name.Text: "anonymous", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "off" }] } }, { Value: Ast.Expression.Reference { Name.Parts: [{ Style: Ast.IdentifierStyle.Bracketed }] } }] },
			"Create priority with no SET" => made is Ast.Statement.CreateBrokerPriority { Options.Count: 0 },
			"Create priority" => made is Ast.Statement.CreateBrokerPriority { Options: [{ Name.Text: "PRIORITY_LEVEL", Value: Ast.LiteralValue.Numeric }] },
			"Alter priority" => made is Ast.Statement.AlterBrokerPriority { Options: [{ Name.Text: "PRIORITY_LEVEL", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "DEFAULT" }] } }] },
			"Priority ANY and [ANY]" => made is Ast.Statement.CreateBrokerPriority
			{
				Options:
				[
					{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "ANY", Style: Ast.IdentifierStyle.Regular }] } },
					{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "[ANY]", Style: Ast.IdentifierStyle.Bracketed }] } },
					{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "any", Style: Ast.IdentifierStyle.Regular }] } },
				],
			},
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	[Theory]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't'",                                 "Dialog")]
	[InlineData("BEGIN DIALOG CONVERSATION @h FROM SERVICE s TO SERVICE @t",                     "Dialog conversation")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't', @i ON CONTRACT c WITH ENCRYPTION = OFF, LIFETIME = 1 + 1", "Dialog with all")]
	[InlineData("BEGIN CONVERSATION TIMER (@h) TIMEOUT = 5",                                     "Timer")]
	[InlineData("END CONVERSATION @h",                                                           "End")]
	[InlineData("END CONVERSATION @h WITH CLEANUP",                                              "End cleanup")]
	[InlineData("END CONVERSATION @h WITH ERROR = 1 DESCRIPTION = 'd'",                          "End error")]
	[InlineData("MOVE CONVERSATION @h TO @g",                                                    "Move")]
	[InlineData("SEND ON CONVERSATION @h",                                                       "Send bare")]
	[InlineData("SEND ON CONVERSATION (@h)",                                                     "Send bracketed")]
	[InlineData("SEND ON CONVERSATION ((@h)) MESSAGE TYPE m (@b)",                               "Send a handle in brackets, bracketed")]
	[InlineData("SEND ON CONVERSATION (@h, @g) MESSAGE TYPE @m",                                 "Send on two, a type in a variable")]
	[InlineData("SEND ON CONVERSATION @h (@b)",                                                  "Send a body with no type")]
	[InlineData("send on conversation @h message type [a b] (N'x')",                             "Send lower case")]
	public void The_bridge_builds_the_conversation_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Dialog" => made is Ast.Statement.BeginDialog { Handle.Text: "@h", Conversation: false, To: Ast.LiteralValue.String, Instance: null, Contract: null, Options.Count: 0 },
			"Dialog conversation" => made is Ast.Statement.BeginDialog { Conversation: true, To: Ast.Expression.Variable },
			"Dialog with all" => made is Ast.Statement.BeginDialog
			{
				Instance: Ast.Expression.Variable, Contract.Parts: [{ Text: "c" }],
				Options: [{ Name.Text: "ENCRYPTION", Value: Ast.Expression.Reference }, { Name.Text: "LIFETIME", Value: Ast.Expression.Binary }],
			},
			"Timer" => made is Ast.Statement.BeginConversationTimer { Handle: Ast.Expression.Variable, Timeout: Ast.LiteralValue.Numeric },
			"End" => made is Ast.Statement.EndConversation { Error: null, Description: null, Cleanup: false },
			"End cleanup" => made is Ast.Statement.EndConversation { Error: null, Description: null, Cleanup: true },
			"End error" => made is Ast.Statement.EndConversation { Error: Ast.LiteralValue.Numeric, Description: Ast.LiteralValue.String, Cleanup: false },
			"Move" => made is Ast.Statement.MoveConversation { Handle: Ast.Expression.Variable, Group: Ast.Expression.Variable },
			"Send bare" => made is Ast.Statement.Send { Conversations: [Ast.Expression.Variable], Bracketed: false, MessageType: null, Body: null },
			"Send bracketed" => made is Ast.Statement.Send { Conversations: [Ast.Expression.Variable], Bracketed: true },
			"Send a handle in brackets, bracketed" => made is Ast.Statement.Send { Conversations: [Ast.Expression.Parenthesized], Bracketed: true, MessageType: Ast.Expression.Reference, Body: Ast.Expression.Variable },
			"Send on two, a type in a variable" => made is Ast.Statement.Send { Conversations.Count: 2, Bracketed: true, MessageType: Ast.Expression.Variable },
			"Send a body with no type" => made is Ast.Statement.Send { Conversations: [Ast.Expression.Variable], MessageType: null, Body: Ast.Expression.Variable },
			"Send lower case" => made is Ast.Statement.Send { MessageType: Ast.Expression.Reference { Name.Parts: [{ Style: Ast.IdentifierStyle.Bracketed }] }, Body: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	[Theory]
	[InlineData("RECEIVE * FROM q",                                                              "Receive")]
	[InlineData("RECEIVE TOP (2) a AS x, b FROM db.dbo.q INTO @t WHERE conversation_handle = @h", "Receive with all")]
	[InlineData("RECEIVE @v = a FROM q WHERE conversation_group_id = 'g'",                       "Receive into variables")]
	[InlineData("GET CONVERSATION GROUP @g FROM q",                                              "Get")]
	[InlineData("WAITFOR (RECEIVE * FROM q)",                                                    "Wait for receive")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM q)",                                    "Wait for get")]
	[InlineData("WAITFOR (RECEIVE * FROM q), TIMEOUT -1",                                        "Wait with a negative timeout")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM q), TIMEOUT @n",                        "Wait with a timeout in a variable")]
	[InlineData("waitfor (receive top (1) * from q), timeout 100",                               "Wait lower case")]
	public void The_bridge_builds_the_receive_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Receive" => made is Ast.Statement.Receive { Columns: [Ast.SelectColumn.All], Top: null, Into: null, Where: null },
			"Receive with all" => made is Ast.Statement.Receive
			{
				Top: { Parentheses: true, Percent: false, With: null }, Columns: [Ast.SelectColumn.ExpressionItem { AsKeyword: true }, Ast.SelectColumn.ExpressionItem],
				Queue.Parts.Count: 3, Into.Text: "@t", Where: Ast.Expression.Comparison { Left: Ast.Expression.Reference, Right: Ast.Expression.Variable },
			},
			"Receive into variables" => made is Ast.Statement.Receive { Columns: [Ast.SelectColumn.VariableAssignment { Variable.Text: "@v" }], Where: Ast.Expression.Comparison { Right: Ast.LiteralValue.String } },
			"Get" => made is Ast.Statement.GetConversationGroup { Group.Text: "@g", Queue.Parts: [{ Text: "q" }] },
			"Wait for receive" => made is Ast.Statement.WaitForReceive { Receive.Columns: [Ast.SelectColumn.All], Timeout: null },
			"Wait for get" => made is Ast.Statement.WaitForGetConversationGroup { Get.Group.Text: "@g", Timeout: null },
			"Wait with a negative timeout" => made is Ast.Statement.WaitForReceive { Timeout: Ast.LiteralValue.Numeric { Text: "-1" } },
			"Wait with a timeout in a variable" => made is Ast.Statement.WaitForGetConversationGroup { Timeout: Ast.Expression.Variable },
			"Wait lower case" => made is Ast.Statement.WaitForReceive { Receive.Top: not null, Timeout: Ast.LiteralValue.Numeric { Text: "100" } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	/// <summary>
	/// A text the grammar reads and the records do not hold is a gap that says so, never another record: a name with a blank
	/// after its dot.
	/// </summary>
	[Theory]
	[InlineData("CREATE QUEUE db. q")]
	public void A_text_the_records_do_not_hold_is_a_gap(string text)
	{
		var old = TransactSqlParser.Located.ParseStatement(text);

		Assert.Throws<BridgeGap>(() => Bridge.ToAst(old, text));
	}
}
