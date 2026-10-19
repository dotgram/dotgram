using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using Expression = DotGram.Sql.Ast.Expression;
using ExecutionContext = DotGram.Sql.Ast.ExecutionContext;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The Broker family's writer (Sql2023Writer.TransactSql.Broker.cs): each record written as the words of its kind, read back by
/// TransactSqlParser to the same tree, and every tree whose text would read back as another tree or not at all refused.
/// </summary>
public sealed class BrokerWriterTests
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

	static Expression Bracketed(string name)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(name, IdentifierStyle.Bracketed)]));
	}

	static Expression Var(string name)
	{
		return new Expression.Variable(new Identifier(name));
	}

	static LiteralValue.Numeric Digits(string digits)
	{
		return new LiteralValue.Numeric(digits, NumericLiteralKind.DecimalInteger);
	}

	static QualifiedName Dotted(params string[] parts)
	{
		return new QualifiedName([.. parts.Select(static one => new Identifier(one))]);
	}

	static Option Opt(string name, Expression value)
	{
		return new Option(new Identifier(name), value, []);
	}

	static ExecutionContext Context(ExecutionContextKind kind, Expression? name = null)
	{
		return new ExecutionContext(kind, name);
	}

	static ContractMessage Message(string name, ContractSender sender)
	{
		return new ContractMessage(new Identifier(name), sender);
	}

	static Placement Group(PlacementKind kind, string name, IdentifierStyle style = IdentifierStyle.Regular)
	{
		return new Placement(kind, new Identifier(name, style), null);
	}

	static Statement.Receive Received(params SelectColumn[] columns)
	{
		return new Statement.Receive([.. columns], Dotted("q"));
	}

	static Expression Equal(string column, Expression value)
	{
		return new Expression.Comparison(Word(column), ComparisonOperator.Equal, value);
	}

	static readonly SelectColumn Star = new SelectColumn.All();

	/// <summary>The trees whose text is checked, with the text the grammar reads them from.</summary>
	static readonly Dictionary<string, (ISqlNode Tree, string Text)> Texts = new(StringComparer.Ordinal)
	{
		["Create message type"] = (
			new Statement.CreateMessageType(X),
			"CREATE MESSAGE TYPE x"),
		["Create message type with all"] = (
			new Statement.CreateMessageType(new Identifier("[//a/b]", IdentifierStyle.Bracketed)) { Authorization = Y, Validation = new MessageValidation.Named(new Identifier("WELL_FORMED_XML")) },
			"CREATE MESSAGE TYPE [//a/b] AUTHORIZATION y VALIDATION = WELL_FORMED_XML"),
		["Create message type validated by any word"] = (
			new Statement.CreateMessageType(X) { Validation = new MessageValidation.Named(new Identifier("foo")) },
			"CREATE MESSAGE TYPE x VALIDATION = foo"),
		["Create message type validated by the bracketed VALID_XML"] = (
			new Statement.CreateMessageType(X) { Validation = new MessageValidation.Named(new Identifier("[VALID_XML]", IdentifierStyle.Bracketed)) },
			"CREATE MESSAGE TYPE x VALIDATION = [VALID_XML]"),
		["Create message type validated by a schema collection"] = (
			new Statement.CreateMessageType(X) { Validation = new MessageValidation.ValidXml(Dotted("dbo", "s")) },
			"CREATE MESSAGE TYPE x VALIDATION = VALID_XML WITH SCHEMA COLLECTION dbo.s"),
		["Alter message type"] = (
			new Statement.AlterMessageType(X, new MessageValidation.Named(new Identifier("EMPTY"))),
			"ALTER MESSAGE TYPE x VALIDATION = EMPTY"),
		["Alter message type validated by a schema collection"] = (
			new Statement.AlterMessageType(X, new MessageValidation.ValidXml(Dotted("s"))),
			"ALTER MESSAGE TYPE x VALIDATION = VALID_XML WITH SCHEMA COLLECTION s"),

		["Create contract"] = (
			new Statement.CreateContract(X, [Message("a", ContractSender.Initiator), Message("b", ContractSender.Target), Message("c", ContractSender.Any)]),
			"CREATE CONTRACT x (a SENT BY INITIATOR, b SENT BY TARGET, c SENT BY ANY)"),
		["Create contract owned"] = (
			new Statement.CreateContract(X, [new ContractMessage(new Identifier("[DEFAULT]", IdentifierStyle.Bracketed), ContractSender.Target)]) { Authorization = Y },
			"CREATE CONTRACT x AUTHORIZATION y ([DEFAULT] SENT BY TARGET)"),

		["Create queue"] = (
			new Statement.CreateQueue(Dotted("q")),
			"CREATE QUEUE q"),
		["Create queue with its options"] = (
			new Statement.CreateQueue(Dotted("db", "dbo", "q")) { Options = [new QueueOption.Status(true), new QueueOption.Retention(false), new QueueOption.PoisonMessageHandling(true)] },
			"CREATE QUEUE db.dbo.q WITH STATUS = ON, RETENTION = OFF, POISON_MESSAGE_HANDLING (STATUS = ON)"),
		["Create queue with an activation in full"] = (
			new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.Activation([new ActivationSetting.MaxQueueReaders(Digits("5")), new ActivationSetting.ProcedureName(Dotted("dbo", "", "p")), new ActivationSetting.Status(false), new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.Owner))])] },
			"CREATE QUEUE q WITH ACTIVATION (MAX_QUEUE_READERS = 5, PROCEDURE_NAME = dbo..p, STATUS = OFF, EXECUTE AS OWNER)"),
		["Create queue with an activation run as a user"] = (
			new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.Activation([new ActivationSetting.ProcedureName(Dotted("p")), new ActivationSetting.MaxQueueReaders(new LiteralValue.Numeric("1.5e3", NumericLiteralKind.Approximate)), new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.User, Str("u")))])] },
			"CREATE QUEUE q WITH ACTIVATION (PROCEDURE_NAME = p, MAX_QUEUE_READERS = 1.5e3, EXECUTE AS 'u')"),
		["Create queue on a filegroup"] = (
			new Statement.CreateQueue(Dotted("q")) { On = Group(PlacementKind.On, "fg") },
			"CREATE QUEUE q ON fg"),
		["Create queue on a filegroup in brackets"] = (
			new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.Status(true)], On = Group(PlacementKind.On, "[DEFAULT]", IdentifierStyle.Bracketed) },
			"CREATE QUEUE q WITH STATUS = ON ON [DEFAULT]"),
		["Create queue on a filegroup that is a string"] = (
			new Statement.CreateQueue(Dotted("q")) { On = Group(PlacementKind.On, "f'g", IdentifierStyle.String) },
			"CREATE QUEUE q ON 'f''g'"),
		["Create queue on a filegroup that is a national string"] = (
			new Statement.CreateQueue(Dotted("q")) { On = Group(PlacementKind.On, "fg", IdentifierStyle.NationalString) },
			"CREATE QUEUE q ON N'fg'"),
		["Alter queue with options"] = (
			new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Status(false), new QueueOption.DropActivation()])),
			"ALTER QUEUE q WITH STATUS = OFF, ACTIVATION (DROP)"),
		["Alter queue with a part of an activation"] = (
			new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.Status(true)])])),
			"ALTER QUEUE q WITH ACTIVATION (STATUS = ON)"),
		["Alter queue rebuilt"] = (
			new Statement.AlterQueue(Dotted("q"), new QueueChange.Rebuild(null)),
			"ALTER QUEUE q REBUILD"),
		["Alter queue rebuilt with a maxdop"] = (
			new Statement.AlterQueue(Dotted("q"), new QueueChange.Rebuild(Digits("2"))),
			"ALTER QUEUE q REBUILD WITH (MAXDOP = 2)"),
		["Alter queue reorganized"] = (
			new Statement.AlterQueue(Dotted("q"), new QueueChange.Reorganize(null)),
			"ALTER QUEUE q REORGANIZE"),
		["Alter queue reorganized with its lobs compacted"] = (
			new Statement.AlterQueue(Dotted("q"), new QueueChange.Reorganize(true)),
			"ALTER QUEUE q REORGANIZE WITH (LOB_COMPACTION = ON)"),
		["Alter queue moved"] = (
			new Statement.AlterQueue(Dotted("q"), new QueueChange.MoveTo(Group(PlacementKind.MoveTo, "[default]", IdentifierStyle.Bracketed))),
			"ALTER QUEUE q MOVE TO [default]"),

		["Create service"] = (
			new Statement.CreateService(X, Dotted("q")),
			"CREATE SERVICE x ON QUEUE q"),
		["Create service with its contracts"] = (
			new Statement.CreateService(X, Dotted("db", "dbo", "q")) { Authorization = Y, Contracts = [new Identifier("a"), new Identifier("[DEFAULT]", IdentifierStyle.Bracketed)] },
			"CREATE SERVICE x AUTHORIZATION y ON QUEUE db.dbo.q (a, [DEFAULT])"),
		["Alter service on a queue"] = (
			new Statement.AlterService(X) { Queue = Dotted("q") },
			"ALTER SERVICE x ON QUEUE q"),
		["Alter service with changes"] = (
			new Statement.AlterService(X) { Changes = [new ServiceChange(AddOrDrop.Add, new Identifier("a")), new ServiceChange(AddOrDrop.Drop, new Identifier("b"))] },
			"ALTER SERVICE x (ADD CONTRACT a, DROP CONTRACT b)"),
		["Alter service on a queue with changes"] = (
			new Statement.AlterService(X) { Queue = Dotted("q"), Changes = [new ServiceChange(AddOrDrop.Drop, new Identifier("a"))] },
			"ALTER SERVICE x ON QUEUE q (DROP CONTRACT a)"),

		["Create route"] = (
			new Statement.CreateRoute(X, [Opt("ADDRESS", Str("LOCAL"))]),
			"CREATE ROUTE x WITH ADDRESS = 'LOCAL'"),
		["Create route with all"] = (
			new Statement.CreateRoute(X, [Opt("SERVICE_NAME", Str("s")), Opt("BROKER_INSTANCE", NStr("b")), Opt("LIFETIME", Digits("10")), Opt("ADDRESS", Str("TCP://h:4022")), Opt("MIRROR_ADDRESS", Str("m"))]) { Authorization = Y },
			"CREATE ROUTE x AUTHORIZATION y WITH SERVICE_NAME = 's', BROKER_INSTANCE = N'b', LIFETIME = 10, ADDRESS = 'TCP://h:4022', MIRROR_ADDRESS = 'm'"),
		["Alter route"] = (
			new Statement.AlterRoute(X, [Opt("LIFETIME", Digits("5"))]),
			"ALTER ROUTE x WITH LIFETIME = 5"),

		["Create remote service binding"] = (
			new Statement.CreateRemoteServiceBinding(X, new LiteralValue.String("'s'", StringLiteralKind.Character), [Opt("USER", Word("u"))]),
			"CREATE REMOTE SERVICE BINDING x TO SERVICE 's' WITH USER = u"),
		["Create remote service binding with all"] = (
			new Statement.CreateRemoteServiceBinding(X, new LiteralValue.String("'s'", StringLiteralKind.National), [Opt("ANONYMOUS", Word("OFF")), Opt("USER", Bracketed("[u]"))]) { Authorization = Y },
			"CREATE REMOTE SERVICE BINDING x AUTHORIZATION y TO SERVICE N's' WITH ANONYMOUS = OFF, USER = [u]"),
		["Alter remote service binding"] = (
			new Statement.AlterRemoteServiceBinding(X, [Opt("ANONYMOUS", Word("ON"))]),
			"ALTER REMOTE SERVICE BINDING x WITH ANONYMOUS = ON"),

		["Create broker priority"] = (
			new Statement.CreateBrokerPriority(X),
			"CREATE BROKER PRIORITY x FOR CONVERSATION"),
		["Create broker priority with all"] = (
			new Statement.CreateBrokerPriority(X) { Options = [Opt("CONTRACT_NAME", Word("ANY")), Opt("LOCAL_SERVICE_NAME", Word("s")), Opt("REMOTE_SERVICE_NAME", Str("r")), Opt("PRIORITY_LEVEL", Word("DEFAULT"))] },
			"CREATE BROKER PRIORITY x FOR CONVERSATION SET (CONTRACT_NAME = ANY, LOCAL_SERVICE_NAME = s, REMOTE_SERVICE_NAME = 'r', PRIORITY_LEVEL = DEFAULT)"),
		["Create broker priority with the word ANY for a remote service"] = (
			new Statement.CreateBrokerPriority(X) { Options = [Opt("REMOTE_SERVICE_NAME", Word("ANY")), Opt("PRIORITY_LEVEL", Digits("5"))] },
			"CREATE BROKER PRIORITY x FOR CONVERSATION SET (REMOTE_SERVICE_NAME = ANY, PRIORITY_LEVEL = 5)"),
		["Alter broker priority"] = (
			new Statement.AlterBrokerPriority(X, [Opt("CONTRACT_NAME", Bracketed("[ANY]"))]),
			"ALTER BROKER PRIORITY x FOR CONVERSATION SET (CONTRACT_NAME = [ANY])"),

		["Begin dialog"] = (
			new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Str("t")),
			"BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't'"),
		["Begin dialog conversation with all"] = (
			new Statement.BeginDialog(new Identifier("@h"), Dotted("a", "b"), Var("@t")) { Conversation = true, Instance = Str("CURRENT DATABASE"), Contract = Dotted("c"), Options = [Opt("RELATED_CONVERSATION", Var("@r")), Opt("LIFETIME", Digits("60")), Opt("ENCRYPTION", Word("OFF"))] },
			"BEGIN DIALOG CONVERSATION @h FROM SERVICE a.b TO SERVICE @t, 'CURRENT DATABASE' ON CONTRACT c WITH RELATED_CONVERSATION = @r, LIFETIME = 60, ENCRYPTION = OFF"),
		["Begin dialog with a lifetime that is an expression"] = (
			new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), NStr("t")) { Instance = Var("@i"), Options = [Opt("RELATED_CONVERSATION_GROUP", Str("g")), Opt("LIFETIME", new Expression.Binary(Digits("1"), BinaryOperator.Add, Digits("1")))] },
			"BEGIN DIALOG @h FROM SERVICE s TO SERVICE N't', @i WITH RELATED_CONVERSATION_GROUP = 'g', LIFETIME = 1 + 1"),
		["Begin conversation timer"] = (
			new Statement.BeginConversationTimer(Var("@h"), Digits("10")),
			"BEGIN CONVERSATION TIMER (@h) TIMEOUT = 10"),
		["End conversation"] = (
			new Statement.EndConversation(Var("@h")),
			"END CONVERSATION @h"),
		["End conversation cleaned up"] = (
			new Statement.EndConversation(Var("@h")) { Cleanup = true },
			"END CONVERSATION @h WITH CLEANUP"),
		["End conversation with an error"] = (
			new Statement.EndConversation(Var("@h")) { Error = Var("@e"), Description = NStr("d") },
			"END CONVERSATION @h WITH ERROR = @e DESCRIPTION = N'd'"),
		["End conversation with a numeric error"] = (
			new Statement.EndConversation(Digits("10")) { Error = new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal), Description = Var("@d") },
			"END CONVERSATION 10 WITH ERROR = 1.5 DESCRIPTION = @d"),
		["Move conversation"] = (
			new Statement.MoveConversation(Var("@h"), Var("@g")),
			"MOVE CONVERSATION @h TO @g"),

		["Send"] = (
			new Statement.Send([Var("@h")]),
			"SEND ON CONVERSATION @h"),
		["Send in brackets"] = (
			new Statement.Send([Var("@h")]) { Bracketed = true },
			"SEND ON CONVERSATION (@h)"),
		["Send on several"] = (
			new Statement.Send([Var("@h"), Var("@g")]) { Bracketed = true, MessageType = Word("m"), Body = Var("@b") },
			"SEND ON CONVERSATION (@h, @g) MESSAGE TYPE m (@b)"),
		["Send a message of a type in a variable"] = (
			new Statement.Send([Var("@h")]) { MessageType = Var("@m"), Body = NStr("x") },
			"SEND ON CONVERSATION @h MESSAGE TYPE @m (N'x')"),
		["Send a message of a qualified type"] = (
			new Statement.Send([Var("@h")]) { MessageType = new Expression.Reference(Dotted("a", "b")) },
			"SEND ON CONVERSATION @h MESSAGE TYPE a.b"),
		["Send a body with no type"] = (
			new Statement.Send([Var("@h")]) { Body = new LiteralValue.Null() },
			"SEND ON CONVERSATION @h (NULL)"),
		["Send to a bracketed handle"] = (
			new Statement.Send([new Expression.Parenthesized(Var("@h"))]) { Bracketed = true, Body = Var("@b") },
			"SEND ON CONVERSATION ((@h)) (@b)"),
		["Send to a handle that is a call"] = (
			new Statement.Send([new Expression.Invocation(new QualifiedName([new Identifier("NEWID")]), [])]) { MessageType = Word("m") },
			"SEND ON CONVERSATION NEWID() MESSAGE TYPE m"),

		["Receive"] = (
			Received(Star),
			"RECEIVE * FROM q"),
		["Receive with all"] = (
			new Statement.Receive([Star], Dotted("db", "dbo", "q")) { Top = new TopClause(Digits("1"), true, false, null), Into = new Identifier("@t"), Where = Equal("conversation_handle", Var("@h")) },
			"RECEIVE TOP (1) * FROM db.dbo.q INTO @t WHERE conversation_handle = @h"),
		["Receive columns"] = (
			Received(new SelectColumn.ExpressionItem(Word("a"), new Identifier("x"), true), new SelectColumn.ExpressionItem(Word("b"))),
			"RECEIVE a AS x, b FROM q"),
		["Receive into variables"] = (
			Received(new SelectColumn.VariableAssignment(new Identifier("@v"), AssignmentOperator.Assign, Word("a"))),
			"RECEIVE @v = a FROM q"),
		["Receive where the group is a string"] = (
			new Statement.Receive([Star], Dotted("q")) { Where = Equal("CONVERSATION_GROUP_ID", NStr("g")) },
			"RECEIVE * FROM q WHERE CONVERSATION_GROUP_ID = N'g'"),
		["Get conversation group"] = (
			new Statement.GetConversationGroup(new Identifier("@g"), Dotted("db", "dbo", "q")),
			"GET CONVERSATION GROUP @g FROM db.dbo.q"),
		["Wait for a receive"] = (
			new Statement.WaitForReceive(Received(Star), null),
			"WAITFOR (RECEIVE * FROM q)"),
		["Wait for a receive with a timeout"] = (
			new Statement.WaitForReceive(new Statement.Receive([Star], Dotted("q")) { Top = new TopClause(Digits("1"), true, false, null) }, Digits("100")),
			"WAITFOR (RECEIVE TOP (1) * FROM q), TIMEOUT 100"),
		["Wait for a group with a negative timeout"] = (
			new Statement.WaitForGetConversationGroup(new Statement.GetConversationGroup(new Identifier("@g"), Dotted("q")), new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger)),
			"WAITFOR (GET CONVERSATION GROUP @g FROM q), TIMEOUT -1"),
		["Wait for a group with a timeout in a variable"] = (
			new Statement.WaitForGetConversationGroup(new Statement.GetConversationGroup(new Identifier("@g"), Dotted("q")), Var("@n")),
			"WAITFOR (GET CONVERSATION GROUP @g FROM q), TIMEOUT @n"),
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

	/// <summary>Each tree written and read back by TransactSqlParser, as the same tree.</summary>
	[Theory]
	[MemberData(nameof(TreeNames))]
	public void Each_statement_reads_back_as_itself(string which)
	{
		var tree = (Statement)Texts[which].Tree;
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, TransactSqlParser.Located.ParseStatement(text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names, above it, the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	static readonly Dictionary<string, ISqlNode> Refusals = new(StringComparer.Ordinal)
	{
		// Identifier does not read a variable, a reserved word written plain, a string, two words or an unclosed bracket.
		["a message type named by a variable"] = new Statement.CreateMessageType(new Identifier("@x")),
		["a message type named by a reserved word"] = new Statement.CreateMessageType(new Identifier("select")),
		["a message type named by a string"] = new Statement.CreateMessageType(new Identifier("x", IdentifierStyle.String)),
		["a message type named by two words"] = new Statement.CreateMessageType(new Identifier("a b")),
		["a message type named in an unclosed bracket"] = new Statement.CreateMessageType(new Identifier("[x", IdentifierStyle.Bracketed)),
		["a message type owned by a variable"] = new Statement.CreateMessageType(X) { Authorization = new Identifier("@o") },
		// MessageValidation: `?!"VALID_XML"i & Identifier` reads VALID_XML as the other alternative's first word.
		["a validation by the plain word VALID_XML"] = new Statement.CreateMessageType(X) { Validation = new MessageValidation.Named(new Identifier("valid_xml")) },
		["a validation by a reserved word"] = new Statement.AlterMessageType(X, new MessageValidation.Named(new Identifier("default"))),
		["a validation by a string"] = new Statement.AlterMessageType(X, new MessageValidation.Named(new Identifier("none", IdentifierStyle.String))),
		["a validation by a schema collection that is a variable"] = new Statement.AlterMessageType(X, new MessageValidation.ValidXml(Dotted("@s"))),

		// ContractMessage: at least one, a name, and one of the three senders.
		["a contract of no message"] = new Statement.CreateContract(X, []),
		["a contract of a message named by a variable"] = new Statement.CreateContract(X, [Message("@m", ContractSender.Any)]),
		["a contract of a message named DEFAULT, plain"] = new Statement.CreateContract(X, [Message("default", ContractSender.Any)]),
		["a contract of a message of two parts"] = new Statement.CreateContract(X, [Message("a.b", ContractSender.Any)]),
		["a contract of a sender with no word"] = new Statement.CreateContract(X, [Message("m", (ContractSender)9)]),

		// QueueName is a QualifiedName of identifiers.
		["a queue named by a variable"] = new Statement.CreateQueue(Dotted("@q")),
		["a queue named by a reserved word"] = new Statement.CreateQueue(Dotted("dbo", "table")),
		["a queue whose name ends in a left out part"] = new Statement.CreateQueue(Dotted("db", "")),
		// QueueOption is each of its kinds once (Syntax.NamedOnce, Msg 102).
		["a queue with its status twice"] = new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.Status(true), new QueueOption.Status(false)] },
		["a queue with its retention twice"] = new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.Retention(true), new QueueOption.Retention(false)] },
		["a queue with its poison message handling twice"] = new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.PoisonMessageHandling(true), new QueueOption.PoisonMessageHandling(false)] },
		["an altered queue with its activation twice"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.DropActivation(), new QueueOption.Activation([new ActivationSetting.Status(true)])])),
		// Syntax.Activates: a queue made names the procedure, the readers and whom they run as.
		["a queue made with a dropped activation"] = new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.DropActivation()] },
		["a queue made with an activation of its status only"] = new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.Activation([new ActivationSetting.Status(true)])] },
		["a queue made with an activation without its readers"] = new Statement.CreateQueue(Dotted("q")) { Options = [new QueueOption.Activation([new ActivationSetting.ProcedureName(Dotted("p")), new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.Self))])] },
		// Activation reads one setting at least, each once.
		["an activation of no setting"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([])])),
		["an activation with its status twice"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.Status(true), new ActivationSetting.Status(false)])])),
		["an activation with its procedure twice"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.ProcedureName(Dotted("p")), new ActivationSetting.ProcedureName(Dotted("p"))])])),
		// EXECUTE AS is SELF, OWNER or a string in single quotes: no caller, no login, no national string, no name after SELF.
		["an activation run as the caller"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.Caller))])])),
		["an activation run as a login"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.Login, Str("l")))])])),
		["an activation run as a national string"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.User, NStr("u")))])])),
		["an activation run as a user with no name"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.User))])])),
		["an activation run as SELF by name"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.Self, Str("u")))])])),
		["an activation run as a name"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.ExecuteAs(Context(ExecutionContextKind.User, Word("u")))])])),
		// MAX_QUEUE_READERS is an unsigned numeric literal.
		["an activation of readers that is signed"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.MaxQueueReaders(new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger))])])),
		["an activation of readers that is hexadecimal"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.MaxQueueReaders(new LiteralValue.Numeric("0x10", NumericLiteralKind.HexInteger))])])),
		["an activation of readers of a kind its text is not"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.MaxQueueReaders(new LiteralValue.Numeric("5", NumericLiteralKind.Decimal))])])),
		["an activation whose procedure is a variable"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([new QueueOption.Activation([new ActivationSetting.ProcedureName(Dotted("@p"))])])),
		// A queue's filegroup is a name or a string, of one part, after the options; ON is the only placement a queue made reads.
		["a queue on a placement of another kind"] = new Statement.CreateQueue(Dotted("q")) { On = Group(PlacementKind.MoveTo, "fg") },
		["a queue on a scheme and its columns"] = new Statement.CreateQueue(Dotted("q")) { On = new Placement(PlacementKind.On, new Identifier("ps"), [new Identifier("c")]) },
		["a queue on DEFAULT, plain"] = new Statement.CreateQueue(Dotted("q")) { On = Group(PlacementKind.On, "default") },
		["a queue on a filegroup of two words"] = new Statement.CreateQueue(Dotted("q")) { On = Group(PlacementKind.On, "a b") },
		// ALTER QUEUE: one change at a time, and MOVE TO takes a name and not a string.
		["an altered queue with no option"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.With([])),
		["an altered queue moved to a string"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.MoveTo(Group(PlacementKind.MoveTo, "fg", IdentifierStyle.String))),
		["an altered queue moved with another placement"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.MoveTo(Group(PlacementKind.On, "fg"))),
		["an altered queue moved to a scheme"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.MoveTo(new Placement(PlacementKind.MoveTo, new Identifier("ps"), [new Identifier("c")]))),
		["an altered queue rebuilt with a signed maxdop"] = new Statement.AlterQueue(Dotted("q"), new QueueChange.Rebuild(new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger))),

		// A service is named by an Identifier and keeps to a queue of a QualifiedName.
		["a service named by a variable"] = new Statement.CreateService(new Identifier("@s"), Dotted("q")),
		["a service on a queue named by a variable"] = new Statement.CreateService(X, Dotted("@q")),
		["a service with a contract named by a reserved word"] = new Statement.CreateService(X, Dotted("q")) { Contracts = [new Identifier("default")] },
		["a service owned by a string"] = new Statement.CreateService(X, Dotted("q")) { Authorization = new Identifier("o", IdentifierStyle.String) },
		// AlterService: a queue or changes, one at least.
		["an altered service that changes nothing"] = new Statement.AlterService(X),
		["an altered service with a change that is neither"] = new Statement.AlterService(X) { Changes = [new ServiceChange((AddOrDrop)9, new Identifier("c"))] },
		["an altered service with a contract named by a variable"] = new Statement.AlterService(X) { Changes = [new ServiceChange(AddOrDrop.Add, new Identifier("@c"))] },

		// RouteOption: its closed list, each once, a string but the lifetime, which is a number; a route made names its address.
		["a route made with no address"] = new Statement.CreateRoute(X, [Opt("SERVICE_NAME", Str("s"))]),
		["a route altered with no setting"] = new Statement.AlterRoute(X, []),
		["a route with a setting it has not"] = new Statement.AlterRoute(X, [Opt("USER", Word("u"))]),
		["a route with a setting twice"] = new Statement.AlterRoute(X, [Opt("ADDRESS", Str("a")), Opt("ADDRESS", Str("b"))]),
		["a route with a setting twice in two cases"] = new Statement.AlterRoute(X, [Opt("ADDRESS", Str("a")), Opt("address", Str("b"))]),
		["a route with an address that is a name"] = new Statement.AlterRoute(X, [Opt("ADDRESS", Word("a"))]),
		["a route with an address that is a variable"] = new Statement.AlterRoute(X, [Opt("ADDRESS", Var("@a"))]),
		["a route with a lifetime that is a string"] = new Statement.AlterRoute(X, [Opt("LIFETIME", Str("1"))]),
		["a route with a lifetime that is signed"] = new Statement.AlterRoute(X, [Opt("LIFETIME", new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger))]),
		["a route with a setting that omits its ="] = new Statement.AlterRoute(X, [new Option(new Identifier("ADDRESS"), Str("a"), []) { EqualsOmitted = true }]),
		["a route with a setting of two words"] = new Statement.AlterRoute(X, [new Option(new Identifier("ADDRESS"), Str("a"), []) { Words = [new Identifier("TO")] }]),
		["a route with a setting that has options of its own"] = new Statement.AlterRoute(X, [new Option(new Identifier("ADDRESS"), Str("a"), [Opt("LIFETIME", Digits("1"))])]),
		["a route with a setting that has a place"] = new Statement.AlterRoute(X, [new Option(new Identifier("ADDRESS"), Str("a"), []) { Place = new Placement(PlacementKind.MoveTo, new Identifier("fg"), null) }]),
		["a route with a setting that has no value"] = new Statement.AlterRoute(X, [new Option(new Identifier("ADDRESS"), null, [])]),
		["a route with a setting named in brackets"] = new Statement.AlterRoute(X, [new Option(new Identifier("[ADDRESS]", IdentifierStyle.Bracketed), Str("a"), [])]),

		// BindingOption: the user is an Identifier, the switch ON or OFF; a binding made names its user.
		["a binding made with no user"] = new Statement.CreateRemoteServiceBinding(X, new LiteralValue.String("'s'", StringLiteralKind.Character), [Opt("ANONYMOUS", Word("ON"))]),
		["a binding to a service that is a variable"] = new Statement.CreateRemoteServiceBinding(X, new LiteralValue.String("@s", StringLiteralKind.Character), [Opt("USER", Word("u"))]),
		["a binding to a service with a character set"] = new Statement.CreateRemoteServiceBinding(X, new LiteralValue.String("'s'", StringLiteralKind.Character) { CharacterSet = new CharacterSetName(new QualifiedName([new Identifier("utf8")])) }, [Opt("USER", Word("u"))]),
		["a binding altered with no setting"] = new Statement.AlterRemoteServiceBinding(X, []),
		["a binding with a user that is a string"] = new Statement.AlterRemoteServiceBinding(X, [Opt("USER", Str("u"))]),
		["a binding with a user that is a reserved word"] = new Statement.AlterRemoteServiceBinding(X, [Opt("USER", Word("select"))]),
		["a binding with a user of two parts"] = new Statement.AlterRemoteServiceBinding(X, [Opt("USER", new Expression.Reference(Dotted("a", "b")))]),
		["a binding with anonymous that is a name"] = new Statement.AlterRemoteServiceBinding(X, [Opt("ANONYMOUS", Word("maybe"))]),
		["a binding with anonymous that is a string"] = new Statement.AlterRemoteServiceBinding(X, [Opt("ANONYMOUS", Str("ON"))]),
		["a binding with anonymous that is a number"] = new Statement.AlterRemoteServiceBinding(X, [Opt("ANONYMOUS", Digits("1"))]),
		["a binding with its user twice"] = new Statement.AlterRemoteServiceBinding(X, [Opt("USER", Word("a")), Opt("USER", Word("b"))]),

		// PriorityOption: names or ANY, a string or ANY, a number or DEFAULT.
		["a priority with a setting it has not"] = new Statement.CreateBrokerPriority(X) { Options = [Opt("LIFETIME", Digits("1"))] },
		["a priority altered with no setting"] = new Statement.AlterBrokerPriority(X, []),
		["a priority with a setting twice"] = new Statement.AlterBrokerPriority(X, [Opt("PRIORITY_LEVEL", Digits("1")), Opt("PRIORITY_LEVEL", Digits("2"))]),
		["a priority with a contract that is a string"] = new Statement.AlterBrokerPriority(X, [Opt("CONTRACT_NAME", Str("c"))]),
		["a priority with a contract that is DEFAULT"] = new Statement.AlterBrokerPriority(X, [Opt("CONTRACT_NAME", Word("DEFAULT"))]),
		["a priority with a remote service that is a name"] = new Statement.AlterBrokerPriority(X, [Opt("REMOTE_SERVICE_NAME", Word("r"))]),
		["a priority with a level that is ANY"] = new Statement.AlterBrokerPriority(X, [Opt("PRIORITY_LEVEL", Word("ANY"))]),
		["a priority with a level that is a name"] = new Statement.AlterBrokerPriority(X, [Opt("PRIORITY_LEVEL", Word("high"))]),
		["a priority with a level that is signed"] = new Statement.AlterBrokerPriority(X, [Opt("PRIORITY_LEVEL", new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger))]),
		["a priority with a level that is a minus over a number"] = new Statement.AlterBrokerPriority(X, [Opt("PRIORITY_LEVEL", new Expression.Unary(UnaryOperator.Minus, Digits("1")))]),

		// BEGIN DIALOG: a variable to hold the handle, a string or a variable for the service, never a name.
		["a dialog whose handle is a name"] = new Statement.BeginDialog(new Identifier("h"), Dotted("s"), Str("t")),
		["a dialog whose handle is two words"] = new Statement.BeginDialog(new Identifier("@h h"), Dotted("s"), Str("t")),
		["a dialog whose handle is a delimited name"] = new Statement.BeginDialog(new Identifier("[@h]", IdentifierStyle.Bracketed), Dotted("s"), Str("t")),
		["a dialog to a service that is a name"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Word("t")),
		["a dialog to a service that is a number"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Digits("1")),
		["a dialog to a service that is a call"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), new Expression.Invocation(new QualifiedName([new Identifier("f")]), [])),
		["a dialog to a service that is a variable of two words"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Var("@a b")),
		["a dialog to a broker that is a name"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Str("t")) { Instance = Word("i") },
		["a dialog from a service that is a variable"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("@s"), Str("t")),
		["a dialog on a contract that is a variable"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Str("t")) { Contract = Dotted("@c") },
		// DialogOption: each once (Syntax.DialogAgrees, Msg 102), a conversation related one way.
		["a dialog related to a conversation and a group"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Str("t")) { Options = [Opt("RELATED_CONVERSATION", Var("@a")), Opt("RELATED_CONVERSATION_GROUP", Var("@b"))] },
		["a dialog with its lifetime twice"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Str("t")) { Options = [Opt("LIFETIME", Digits("1")), Opt("LIFETIME", Digits("2"))] },
		["a dialog with an option it has not"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Str("t")) { Options = [Opt("ADDRESS", Str("a"))] },
		["a dialog with an encryption that is a number"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Str("t")) { Options = [Opt("ENCRYPTION", Digits("1"))] },
		["a dialog with a lifetime that is a condition"] = new Statement.BeginDialog(new Identifier("@h"), Dotted("s"), Str("t")) { Options = [Opt("LIFETIME", new Expression.Comparison(Digits("1"), ComparisonOperator.Equal, Digits("1")))] },

		// A handle, a timeout and a group are values: a condition is none.
		["a timer on a condition"] = new Statement.BeginConversationTimer(new Expression.Comparison(Digits("1"), ComparisonOperator.Equal, Digits("1")), Digits("1")),
		["a timer timing out at a condition"] = new Statement.BeginConversationTimer(Var("@h"), new Expression.Comparison(Digits("1"), ComparisonOperator.Equal, Digits("1"))),
		["a move to a group that is a variable of two words"] = new Statement.MoveConversation(Var("@h"), Var("@g g")),
		// END CONVERSATION: plainly, cleaned up, or with an error and its description, in that order and with no comma.
		["an end cleaned up with an error"] = new Statement.EndConversation(Var("@h")) { Cleanup = true, Error = Digits("1"), Description = Str("d") },
		["an end with an error and no description"] = new Statement.EndConversation(Var("@h")) { Error = Digits("1") },
		["an end with a description and no error"] = new Statement.EndConversation(Var("@h")) { Description = Str("d") },
		["an end with an error that is a string"] = new Statement.EndConversation(Var("@h")) { Error = Str("1"), Description = Str("d") },
		["an end with an error that is signed"] = new Statement.EndConversation(Var("@h")) { Error = new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger), Description = Str("d") },
		["an end with an error that is a minus over a number"] = new Statement.EndConversation(Var("@h")) { Error = new Expression.Unary(UnaryOperator.Minus, Digits("1")), Description = Str("d") },
		["an end with a description that is a number"] = new Statement.EndConversation(Var("@h")) { Error = Digits("1"), Description = Digits("2") },
		["an end with a description that is a name"] = new Statement.EndConversation(Var("@h")) { Error = Digits("1"), Description = Word("d") },

		// SendTargets: one handle bare or in brackets, or several in brackets.
		["a send on no conversation"] = new Statement.Send([]),
		["a send on several conversations, bare"] = new Statement.Send([Var("@a"), Var("@b")]),
		// A bare handle is a value expression: a name before the body would take it for its arguments (`h (body)` is a call).
		["a send on a name, bare, before a body"] = new Statement.Send([Word("h")]) { Body = Var("@b") },
		["a send on a call, bare, before a body"] = new Statement.Send([new Expression.Invocation(new QualifiedName([new Identifier("f")]), [])]) { Body = Var("@b") },
		// SendTargets reads a bracket first, as the list of handles.
		["a send on a handle in brackets, not bracketed"] = new Statement.Send([new Expression.Parenthesized(Var("@h"))]) { MessageType = Word("m") },
		["a send on a handle that begins with a bracket, not bracketed"] = new Statement.Send([new Expression.Binary(new Expression.Parenthesized(Digits("1")), BinaryOperator.Add, Digits("2"))]) { MessageType = Word("m") },
		// MessageTypeName: a name, or a local variable and not one of the server's.
		["a send of a type that is a string"] = new Statement.Send([Var("@h")]) { MessageType = Str("m") },
		["a send of a type that is the server's variable"] = new Statement.Send([Var("@h")]) { MessageType = Var("@@SPID") },
		["a send of a type that is a number"] = new Statement.Send([Var("@h")]) { MessageType = Digits("1") },
		["a send of a type named by a variable"] = new Statement.Send([Var("@h")]) { MessageType = new Expression.Reference(Dotted("@m")) },
		["a send of a body that is a condition"] = new Statement.Send([Var("@h")]) { Body = new Expression.Comparison(Digits("1"), ComparisonOperator.Equal, Digits("1")) },
		["a send on a handle that is a condition"] = new Statement.Send([new Expression.Comparison(Digits("1"), ComparisonOperator.Equal, Digits("1"))]) { Bracketed = true },

		// ReceiveStatement: columns, assigned to variables or retrieved and not both; TOP in brackets; the star alone.
		["a receive of no column"] = new Statement.Receive([], Dotted("q")),
		["a receive of variables and columns"] = Received(new SelectColumn.VariableAssignment(new Identifier("@v"), AssignmentOperator.Assign, Word("a")), Star),
		["a receive of a qualified star"] = Received(new SelectColumn.QualifiedAll(Word("t"))),
		["a receive of a column given a Unicode-delimited alias"] = Received(new SelectColumn.ExpressionItem(Word("a"), new Identifier("U&\"x\"; SHUTDOWN;--", IdentifierStyle.UnicodeDelimited), true)),
		["a receive of a column given its alias first"] = Received(new SelectColumn.ExpressionItem(Word("a"), new Identifier("x")) { AliasFirst = true }),
		["a receive of a column given AS and no alias"] = Received(new SelectColumn.ExpressionItem(Word("a"), null, true)),
		["a receive of a column whose alias is two words"] = Received(new SelectColumn.ExpressionItem(Word("a"), new Identifier("x y"), true)),
		["a receive of a column whose alias is a reserved word"] = Received(new SelectColumn.ExpressionItem(Word("a"), new Identifier("from"), true)),
		["a receive of a column of a variable name"] = Received(new SelectColumn.ExpressionItem(Word("@a b"))),
		["a receive of a variable that is two words"] = Received(new SelectColumn.VariableAssignment(new Identifier("@v v"), AssignmentOperator.Assign, Word("a"))),
		["a receive from a queue named by a variable"] = new Statement.Receive([Star], Dotted("@q")),
		["a receive into a name"] = new Statement.Receive([Star], Dotted("q")) { Into = new Identifier("t") },
		["a receive into a table of two words"] = new Statement.Receive([Star], Dotted("q")) { Into = new Identifier("@t t") },
		["a receive of a top without brackets"] = new Statement.Receive([Star], Dotted("q")) { Top = new TopClause(Digits("1"), false, false, null) },
		["a receive of a top percent"] = new Statement.Receive([Star], Dotted("q")) { Top = new TopClause(Digits("1"), true, true, null) },
		["a receive of a top with ties"] = new Statement.Receive([Star], Dotted("q")) { Top = new TopClause(Digits("1"), true, false, TopWith.Ties) },
		// ReceiveWhere: one of two columns, equal to a string or a variable.
		["a receive where another column"] = new Statement.Receive([Star], Dotted("q")) { Where = Equal("message_type_name", Str("m")) },
		["a receive where a column of two parts"] = new Statement.Receive([Star], Dotted("q")) { Where = new Expression.Comparison(new Expression.Reference(Dotted("q", "conversation_handle")), ComparisonOperator.Equal, Var("@h")) },
		["a receive where a column is not equal"] = new Statement.Receive([Star], Dotted("q")) { Where = new Expression.Comparison(Word("conversation_handle"), ComparisonOperator.NotEqual, Var("@h")) },
		["a receive where a column is equal to a name"] = new Statement.Receive([Star], Dotted("q")) { Where = Equal("conversation_handle", Word("h")) },
		["a receive where a column is equal to a number"] = new Statement.Receive([Star], Dotted("q")) { Where = Equal("conversation_group_id", Digits("1")) },
		["a receive where two things hold"] = new Statement.Receive([Star], Dotted("q")) { Where = new Expression.Binary(Equal("conversation_handle", Var("@h")), BinaryOperator.And, Equal("conversation_group_id", Var("@g"))) },
		["a receive where the plain word is bracketed"] = new Statement.Receive([Star], Dotted("q")) { Where = new Expression.Comparison(Bracketed("[conversation_handle]"), ComparisonOperator.Equal, Var("@h")) },

		["a group got into a name"] = new Statement.GetConversationGroup(new Identifier("g"), Dotted("q")),
		["a group got from a queue named by a variable"] = new Statement.GetConversationGroup(new Identifier("@g"), Dotted("@q")),
		// WaitTimeout: '-'? & an unsigned numeric literal, or a variable.
		["a wait with a timeout that is a string"] = new Statement.WaitForReceive(Received(Star), Str("1")),
		["a wait with a timeout that is a name"] = new Statement.WaitForReceive(Received(Star), Word("t")),
		["a wait with a timeout that is a plus over a number"] = new Statement.WaitForReceive(Received(Star), new LiteralValue.Numeric("+1", NumericLiteralKind.DecimalInteger)),
		["a wait with a timeout that is a minus over a number"] = new Statement.WaitForReceive(Received(Star), new Expression.Unary(UnaryOperator.Minus, Digits("1"))),
		["a wait with a timeout that is hexadecimal"] = new Statement.WaitForReceive(Received(Star), new LiteralValue.Numeric("0x10", NumericLiteralKind.HexInteger)),
		["a wait with a timeout that is signed twice"] = new Statement.WaitForReceive(Received(Star), new LiteralValue.Numeric("--1", NumericLiteralKind.DecimalInteger)),
		["a wait with a timeout that is an expression"] = new Statement.WaitForGetConversationGroup(new Statement.GetConversationGroup(new Identifier("@g"), Dotted("q")), new Expression.Binary(Digits("1"), BinaryOperator.Add, Digits("1"))),
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
