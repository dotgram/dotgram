using System;

namespace DotGram.Sql.Ast;

// T-SQL's Service Broker family: the objects a conversation is made of, and the statements that begin, move, end,
// send on and receive from conversations. A setting that is a name and a value is an Option (proposal 20); a place
// that is one of several things is a small family named after the grammar's words.

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: CREATE MESSAGE TYPE (Transact-SQL). <c>CREATE MESSAGE TYPE name [AUTHORIZATION owner] [VALIDATION = …]</c>.
	/// </summary>
	public sealed record CreateMessageType(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE MESSAGE TYPE (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }

		/// <summary>T-SQL: CREATE MESSAGE TYPE (Transact-SQL). <c>VALIDATION = …</c>; null where none is written.</summary>
		public MessageValidation? Validation { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER MESSAGE TYPE (Transact-SQL). <c>ALTER MESSAGE TYPE name VALIDATION = …</c>: the validation is always said.
	/// </summary>
	public sealed record AlterMessageType(Identifier Name, MessageValidation Validation) : Statement;

	/// <summary>
	/// T-SQL: CREATE CONTRACT (Transact-SQL). <c>CREATE CONTRACT name [AUTHORIZATION owner] (message SENT BY …, …)</c>: one
	/// message type at least.
	/// </summary>
	public sealed record CreateContract(Identifier Name, SqlList<ContractMessage> Messages) : Statement
	{
		/// <summary>T-SQL: CREATE CONTRACT (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE QUEUE (Transact-SQL). <c>CREATE QUEUE name [WITH option, …] [ON filegroup]</c>: the options in the order
	/// written, each kind once; an activation made is told its procedure, its readers and whom they run as.
	/// </summary>
	public sealed record CreateQueue(QualifiedName Name) : Statement
	{
		/// <summary>T-SQL: CREATE QUEUE (Transact-SQL). <c>WITH option, …</c>: none where the statement writes no <c>WITH</c>.</summary>
		public SqlList<QueueOption> Options { get; init; } = [];

		/// <summary>
		/// T-SQL: CREATE QUEUE (Transact-SQL). <c>ON filegroup</c>, a name or a string, after the options and not before them: a
		/// placement of kind <see cref="PlacementKind.On"/> with a filegroup and no columns; null where none is written.
		/// </summary>
		public Placement? On { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER QUEUE (Transact-SQL). <c>ALTER QUEUE name change</c>: the queue is changed one way at a time.
	/// </summary>
	public sealed record AlterQueue(QualifiedName Name, QueueChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE SERVICE (Transact-SQL). <c>CREATE SERVICE name [AUTHORIZATION owner] ON QUEUE queue [(contract, …)]</c>.
	/// </summary>
	public sealed record CreateService(Identifier Name, QualifiedName Queue) : Statement
	{
		/// <summary>T-SQL: CREATE SERVICE (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }

		/// <summary>T-SQL: CREATE SERVICE (Transact-SQL). The contracts in brackets after the queue: none where no bracket is written.</summary>
		public SqlList<Identifier> Contracts { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER SERVICE (Transact-SQL). <c>ALTER SERVICE name [ON QUEUE queue] [(ADD CONTRACT c, DROP CONTRACT d, …)]</c>:
	/// a queue, a list of changes or both, one at least.
	/// </summary>
	public sealed record AlterService(Identifier Name) : Statement
	{
		/// <summary>T-SQL: ALTER SERVICE (Transact-SQL). <c>ON QUEUE queue</c>; null where none is written.</summary>
		public QualifiedName? Queue { get; init; }

		/// <summary>T-SQL: ALTER SERVICE (Transact-SQL). The contracts added and dropped in brackets: none where no bracket is written.</summary>
		public SqlList<ServiceChange> Changes { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE ROUTE (Transact-SQL). <c>CREATE ROUTE name [AUTHORIZATION owner] WITH setting, …</c>: SERVICE_NAME,
	/// BROKER_INSTANCE, LIFETIME, ADDRESS and MIRROR_ADDRESS, each once, and the address among them.
	/// </summary>
	public sealed record CreateRoute(Identifier Name, SqlList<Option> Options) : Statement
	{
		/// <summary>T-SQL: CREATE ROUTE (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER ROUTE (Transact-SQL). <c>ALTER ROUTE name WITH setting, …</c>: the settings of the creating statement, one
	/// at least, each once.
	/// </summary>
	public sealed record AlterRoute(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: CREATE REMOTE SERVICE BINDING (Transact-SQL). <c>CREATE REMOTE SERVICE BINDING name [AUTHORIZATION owner] TO
	/// SERVICE 'service' WITH setting, …</c>: USER and ANONYMOUS, each once, and the user among them.
	/// </summary>
	public sealed record CreateRemoteServiceBinding(Identifier Name, LiteralValue.String Service, SqlList<Option> Options) : Statement
	{
		/// <summary>T-SQL: CREATE REMOTE SERVICE BINDING (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER REMOTE SERVICE BINDING (Transact-SQL). <c>ALTER REMOTE SERVICE BINDING name WITH setting, …</c>: USER and
	/// ANONYMOUS, one at least, each once.
	/// </summary>
	public sealed record AlterRemoteServiceBinding(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: CREATE BROKER PRIORITY (Transact-SQL). <c>CREATE BROKER PRIORITY name FOR CONVERSATION [SET (setting, …)]</c>.
	/// </summary>
	public sealed record CreateBrokerPriority(Identifier Name) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE BROKER PRIORITY (Transact-SQL). <c>SET (setting, …)</c>: CONTRACT_NAME, LOCAL_SERVICE_NAME,
		/// REMOTE_SERVICE_NAME and PRIORITY_LEVEL, each once; none where the statement writes no <c>SET</c>.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER BROKER PRIORITY (Transact-SQL). <c>ALTER BROKER PRIORITY name FOR CONVERSATION SET (setting, …)</c>: the
	/// settings of the creating statement, one at least, each once.
	/// </summary>
	public sealed record AlterBrokerPriority(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: BEGIN DIALOG CONVERSATION (Transact-SQL). <c>BEGIN DIALOG [CONVERSATION] @handle FROM SERVICE service TO SERVICE
	/// 'service' [, 'broker'] [ON CONTRACT contract] [WITH setting, …]</c>: the handle is a variable, which the identifier keeps
	/// with its <c>@</c>, and the service it is begun to a string or a variable.
	/// </summary>
	public sealed record BeginDialog(Identifier Handle, QualifiedName From, Expression To) : Statement
	{
		/// <summary>T-SQL: BEGIN DIALOG CONVERSATION (Transact-SQL). <c>DIALOG CONVERSATION</c> against <c>DIALOG</c>: the same statement.</summary>
		[Spelling] public bool Conversation { get; init; }

		/// <summary>T-SQL: BEGIN DIALOG CONVERSATION (Transact-SQL). <c>, 'broker'</c> after the service: which broker it is on; null where none is said.</summary>
		public Expression? Instance { get; init; }

		/// <summary>T-SQL: BEGIN DIALOG CONVERSATION (Transact-SQL). <c>ON CONTRACT contract</c>; null where none is named.</summary>
		public QualifiedName? Contract { get; init; }

		/// <summary>
		/// T-SQL: BEGIN DIALOG CONVERSATION (Transact-SQL). <c>WITH setting, …</c>: RELATED_CONVERSATION_GROUP,
		/// RELATED_CONVERSATION, LIFETIME and ENCRYPTION, each once and the first two not together; none where no <c>WITH</c> is written.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: BEGIN CONVERSATION TIMER (Transact-SQL). <c>BEGIN CONVERSATION TIMER (handle) TIMEOUT = seconds</c>.
	/// </summary>
	public sealed record BeginConversationTimer(Expression Handle, Expression Timeout) : Statement;

	/// <summary>
	/// T-SQL: END CONVERSATION (Transact-SQL). <c>END CONVERSATION handle [WITH CLEANUP | WITH ERROR = code DESCRIPTION =
	/// text]</c>: ended plainly, cleaned up, or with an error and what it was, never two of the three.
	/// </summary>
	public sealed record EndConversation(Expression Handle) : Statement
	{
		/// <summary>T-SQL: END CONVERSATION (Transact-SQL). <c>WITH ERROR = code</c>, a number or a variable; null where no error is said.</summary>
		public Expression? Error { get; init; }

		/// <summary>T-SQL: END CONVERSATION (Transact-SQL). <c>DESCRIPTION = text</c> after the error, a string or a variable; null where no error is said.</summary>
		public Expression? Description { get; init; }

		/// <summary>T-SQL: END CONVERSATION (Transact-SQL). <c>WITH CLEANUP</c>: ended without telling the other side.</summary>
		public bool Cleanup { get; init; }
	}

	/// <summary>
	/// T-SQL: MOVE CONVERSATION (Transact-SQL). <c>MOVE CONVERSATION handle TO group</c>.
	/// </summary>
	public sealed record MoveConversation(Expression Handle, Expression Group) : Statement;

	/// <summary>
	/// T-SQL: SEND (Transact-SQL). <c>SEND ON CONVERSATION handle [MESSAGE TYPE type] [(body)]</c>: one handle bare or in
	/// brackets, or several in brackets.
	/// </summary>
	public sealed record Send(SqlList<Expression> Conversations) : Statement
	{
		/// <summary>T-SQL: SEND (Transact-SQL). <c>(handle)</c> against <c>handle</c>: the same statement; several handles are always in brackets.</summary>
		[Spelling] public bool Bracketed { get; init; }

		/// <summary>T-SQL: SEND (Transact-SQL). <c>MESSAGE TYPE type</c>: a name or a local variable; null where none is said.</summary>
		public Expression? MessageType { get; init; }

		/// <summary>T-SQL: SEND (Transact-SQL). The message in brackets, one value; null where none is written.</summary>
		public Expression? Body { get; init; }
	}

	/// <summary>
	/// T-SQL: RECEIVE (Transact-SQL). <c>RECEIVE [TOP (n)] columns FROM queue [INTO @table] [WHERE …]</c>: the columns a select
	/// list says it, retrieved or assigned to variables and never both.
	/// </summary>
	public sealed record Receive(SqlList<SelectColumn> Columns, QualifiedName Queue) : Statement
	{
		/// <summary>T-SQL: RECEIVE (Transact-SQL). <c>TOP (n)</c> and nothing after it; null where none is written.</summary>
		public TopClause? Top { get; init; }

		/// <summary>T-SQL: RECEIVE (Transact-SQL). <c>INTO @table</c>: a table variable, which the identifier keeps with its <c>@</c>; null where none is written.</summary>
		public Identifier? Into { get; init; }

		/// <summary>
		/// T-SQL: RECEIVE (Transact-SQL). <c>WHERE conversation_handle = value</c> or <c>conversation_group_id = value</c>: one
		/// comparison of one of the two columns with a string or a variable; null where none is written.
		/// </summary>
		public Expression? Where { get; init; }
	}

	/// <summary>
	/// T-SQL: GET CONVERSATION GROUP (Transact-SQL). <c>GET CONVERSATION GROUP @group FROM queue</c>: the group is a variable,
	/// which the identifier keeps with its <c>@</c>.
	/// </summary>
	public sealed record GetConversationGroup(Identifier Group, QualifiedName Queue) : Statement;

	/// <summary>
	/// T-SQL: WAITFOR (Transact-SQL). <c>WAITFOR (RECEIVE …) [, TIMEOUT timeout]</c>: a RECEIVE waited on, and for how long.
	/// </summary>
	public sealed record WaitForReceive(Statement.Receive Receive, Expression? Timeout) : Statement
	{
		// Declared, with `new`: the nested type Receive, which this record inherits from Statement, would otherwise be the member the name finds.
		public new Statement.Receive Receive { get; init; } = Receive;
	}

	/// <summary>
	/// T-SQL: WAITFOR (Transact-SQL). <c>WAITFOR (GET CONVERSATION GROUP …) [, TIMEOUT timeout]</c>: a GET CONVERSATION GROUP
	/// waited on, and for how long.
	/// </summary>
	public sealed record WaitForGetConversationGroup(Statement.GetConversationGroup Get, Expression? Timeout) : Statement;
}

/// <summary>
/// T-SQL: CREATE MESSAGE TYPE (Transact-SQL), ALTER MESSAGE TYPE (Transact-SQL). What a message type is validated by.
/// </summary>
public abstract record MessageValidation : ISqlNode
{
	/// <summary>Where in the text this validation was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this validation was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// <c>VALIDATION = word</c>: <c>NONE</c>, <c>EMPTY</c>, <c>WELL_FORMED_XML</c> or any other, which the engine does not
	/// check until it runs; a word, never <c>VALID_XML</c> written plain.
	/// </summary>
	public sealed record Named(Identifier Word) : MessageValidation;

	/// <summary><c>VALIDATION = VALID_XML WITH SCHEMA COLLECTION name</c>.</summary>
	public sealed record ValidXml(QualifiedName SchemaCollection) : MessageValidation;
}

/// <summary>T-SQL: CREATE CONTRACT (Transact-SQL). One message type of a contract and who may send it.</summary>
public sealed record ContractMessage(Identifier MessageType, ContractSender SentBy) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: CREATE CONTRACT (Transact-SQL). Who sends a message type: <c>INITIATOR</c>, <c>TARGET</c> or <c>ANY</c>.</summary>
public enum ContractSender { Initiator, Target, Any }

/// <summary>
/// T-SQL: CREATE QUEUE (Transact-SQL), ALTER QUEUE (Transact-SQL). One option of a queue's <c>WITH</c>.
/// </summary>
public abstract record QueueOption : ISqlNode
{
	/// <summary>Where in the text this option was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this option was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>STATUS = ON</c> or <c>OFF</c>.</summary>
	public sealed record Status(bool On) : QueueOption;

	/// <summary><c>RETENTION = ON</c> or <c>OFF</c>.</summary>
	public sealed record Retention(bool On) : QueueOption;

	/// <summary><c>POISON_MESSAGE_HANDLING (STATUS = ON)</c> or <c>OFF</c>.</summary>
	public sealed record PoisonMessageHandling(bool On) : QueueOption;

	/// <summary><c>ACTIVATION (DROP)</c>: the activation removed, in an ALTER QUEUE.</summary>
	public sealed record DropActivation : QueueOption;

	/// <summary><c>ACTIVATION (setting, …)</c>: the settings in the order written, each kind once, one at least.</summary>
	public sealed record Activation(SqlList<ActivationSetting> Settings) : QueueOption;
}

/// <summary>T-SQL: CREATE QUEUE (Transact-SQL), ALTER QUEUE (Transact-SQL). One setting of a queue's activation.</summary>
public abstract record ActivationSetting : ISqlNode
{
	/// <summary>Where in the text this setting was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this setting was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>STATUS = ON</c> or <c>OFF</c>.</summary>
	public sealed record Status(bool On) : ActivationSetting;

	/// <summary><c>PROCEDURE_NAME = procedure</c>.</summary>
	public sealed record ProcedureName(QualifiedName Name) : ActivationSetting;

	/// <summary><c>MAX_QUEUE_READERS = n</c>: an unsigned number.</summary>
	public sealed record MaxQueueReaders(LiteralValue.Numeric Readers) : ActivationSetting;

	/// <summary>
	/// <c>EXECUTE AS SELF</c>, <c>EXECUTE AS OWNER</c> (the name null), or <c>EXECUTE AS 'user'</c> (kind
	/// <see cref="ExecutionContextKind.User"/> and the name a string).
	/// </summary>
	public sealed record ExecuteAs(ExecutionContext Context) : ActivationSetting;
}

/// <summary>T-SQL: ALTER QUEUE (Transact-SQL). What an ALTER QUEUE does to the queue.</summary>
public abstract record QueueChange : ISqlNode
{
	/// <summary>Where in the text this change was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this change was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>WITH option, …</c>: the options of the queue, each kind once, one at least.</summary>
	public sealed record With(SqlList<QueueOption> Options) : QueueChange;

	/// <summary><c>REBUILD [WITH (MAXDOP = n)]</c>: an unsigned number, or null where no <c>WITH</c> is written.</summary>
	public sealed record Rebuild(LiteralValue.Numeric? MaxDop) : QueueChange;

	/// <summary><c>REORGANIZE [WITH (LOB_COMPACTION = ON | OFF)]</c>: null where no <c>WITH</c> is written.</summary>
	public sealed record Reorganize(bool? LobCompaction) : QueueChange;

	/// <summary><c>MOVE TO filegroup</c>: a placement of kind <see cref="PlacementKind.MoveTo"/> with a filegroup name and no columns.</summary>
	public sealed record MoveTo(Placement Target) : QueueChange;
}

/// <summary>T-SQL: ALTER SERVICE (Transact-SQL). One contract added to a service or dropped from it.</summary>
public sealed record ServiceChange(AddOrDrop Action, Identifier Contract) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// T-SQL: ALTER SERVICE (Transact-SQL), ALTER SERVER AUDIT SPECIFICATION (Transact-SQL), ALTER DATABASE AUDIT SPECIFICATION
/// (Transact-SQL). <c>ADD</c> or <c>DROP</c>: how an item is changed.
/// </summary>
public enum AddOrDrop { Add, Drop }
