using System;

namespace DotGram.Sql.Ast;

// T-SQL's Monitoring family: what the server watches and who it tells — event notifications, audits and their specifications,
// and extended-event sessions. A setting that is a name and a value is an Option (proposal 20); a place that is one of several
// things is a small family named after the grammar's words.

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: CREATE EVENT NOTIFICATION (Transact-SQL). <c>CREATE EVENT NOTIFICATION name ON { SERVER | DATABASE | QUEUE queue }
	/// [WITH FAN_IN] FOR event, … TO SERVICE 'service', …</c>: the events are runs of words, and the services values.
	/// </summary>
	public sealed record CreateEventNotification(Identifier Name, NotificationScope On, SqlList<ActionWords> Events, SqlList<Expression> Services) : Statement
	{
		/// <summary>T-SQL: CREATE EVENT NOTIFICATION (Transact-SQL). <c>WITH FAN_IN</c>.</summary>
		public bool FanIn { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE SERVER AUDIT (Transact-SQL). <c>CREATE SERVER AUDIT name TO target [WITH (setting, …)] [WHERE condition]</c>:
	/// a file made is told its path.
	/// </summary>
	public sealed record CreateServerAudit(Identifier Name, AuditTarget Target) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE SERVER AUDIT (Transact-SQL). <c>WITH (setting, …)</c>: QUEUE_DELAY, ON_FAILURE, OPERATOR_AUDIT and
		/// AUDIT_GUID, in any order and as often as wanted; none where the statement writes no <c>WITH</c>.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: CREATE SERVER AUDIT (Transact-SQL). <c>WHERE condition</c>; null where none is written.</summary>
		public Expression? Where { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER SERVER AUDIT (Transact-SQL). <c>ALTER SERVER AUDIT name change</c>: an audit altered is altered by something.
	/// </summary>
	public sealed record AlterServerAudit(Identifier Name, AuditChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE SERVER AUDIT SPECIFICATION (Transact-SQL). <c>CREATE SERVER AUDIT SPECIFICATION name [FOR SERVER AUDIT audit]
	/// [ADD (action), …] [WITH (STATE = ON | OFF)]</c>: the actions the grammar reads for either scope, and its settings.
	/// </summary>
	public sealed record CreateServerAuditSpecification(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE SERVER AUDIT SPECIFICATION (Transact-SQL). <c>FOR SERVER AUDIT audit</c>; null where none is written.</summary>
		public QualifiedName? Audit { get; init; }

		/// <summary>T-SQL: CREATE SERVER AUDIT SPECIFICATION (Transact-SQL). The actions added and dropped, in the order written.</summary>
		public SqlList<AuditAction> Actions { get; init; } = [];

		/// <summary>T-SQL: CREATE SERVER AUDIT SPECIFICATION (Transact-SQL). <c>WITH (STATE = ON | OFF, …)</c>, the state as often as wanted; none where no <c>WITH</c> is written.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: ALTER SERVER AUDIT SPECIFICATION (Transact-SQL). The same parts as the creating statement, each optional.</summary>
	public sealed record AlterServerAuditSpecification(Identifier Name) : Statement
	{
		/// <summary>T-SQL: ALTER SERVER AUDIT SPECIFICATION (Transact-SQL). <c>FOR SERVER AUDIT audit</c>; null where none is written.</summary>
		public QualifiedName? Audit { get; init; }

		/// <summary>T-SQL: ALTER SERVER AUDIT SPECIFICATION (Transact-SQL). The actions added and dropped, in the order written.</summary>
		public SqlList<AuditAction> Actions { get; init; } = [];

		/// <summary>T-SQL: ALTER SERVER AUDIT SPECIFICATION (Transact-SQL). <c>WITH (STATE = ON | OFF, …)</c>; none where no <c>WITH</c> is written.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE DATABASE AUDIT SPECIFICATION. <c>CREATE DATABASE AUDIT SPECIFICATION name [FOR SERVER AUDIT audit]
	/// [ADD (action), …] [WITH (STATE = ON | OFF)]</c>.
	/// </summary>
	public sealed record CreateDatabaseAuditSpecification(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE DATABASE AUDIT SPECIFICATION. <c>FOR SERVER AUDIT audit</c>; null where none is written.</summary>
		public QualifiedName? Audit { get; init; }

		/// <summary>T-SQL: CREATE DATABASE AUDIT SPECIFICATION. The actions added and dropped, in the order written.</summary>
		public SqlList<AuditAction> Actions { get; init; } = [];

		/// <summary>T-SQL: CREATE DATABASE AUDIT SPECIFICATION. <c>WITH (STATE = ON | OFF, …)</c>; none where no <c>WITH</c> is written.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: ALTER DATABASE AUDIT SPECIFICATION (Transact-SQL). The same parts as the creating statement, each optional.</summary>
	public sealed record AlterDatabaseAuditSpecification(Identifier Name) : Statement
	{
		/// <summary>T-SQL: ALTER DATABASE AUDIT SPECIFICATION (Transact-SQL). <c>FOR SERVER AUDIT audit</c>; null where none is written.</summary>
		public QualifiedName? Audit { get; init; }

		/// <summary>T-SQL: ALTER DATABASE AUDIT SPECIFICATION (Transact-SQL). The actions added and dropped, in the order written.</summary>
		public SqlList<AuditAction> Actions { get; init; } = [];

		/// <summary>T-SQL: ALTER DATABASE AUDIT SPECIFICATION (Transact-SQL). <c>WITH (STATE = ON | OFF, …)</c>; none where no <c>WITH</c> is written.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE EVENT SESSION (Transact-SQL). <c>CREATE EVENT SESSION name ON { SERVER | DATABASE } ADD EVENT …, ADD TARGET …
	/// [WITH (setting, …)]</c>: the events first and one at least, the targets after them, and nothing dropped.
	/// </summary>
	public sealed record CreateEventSession(Identifier Name, EventScope On, SqlList<EventSessionPiece> Pieces) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE EVENT SESSION (Transact-SQL). <c>WITH (setting, …)</c>: MAX_MEMORY, EVENT_RETENTION_MODE,
		/// MAX_DISPATCH_LATENCY, MAX_EVENT_SIZE, MEMORY_PARTITION_MODE, TRACK_CAUSALITY, STARTUP_STATE and MAX_DURATION, in any order
		/// and as often as wanted; none where the statement writes no <c>WITH</c>.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER EVENT SESSION (Transact-SQL). <c>ALTER EVENT SESSION name ON { SERVER | DATABASE }</c> and one of: events added,
	/// events dropped, targets added or targets dropped, and then its options; or its options alone; or its state alone.
	/// </summary>
	public sealed record AlterEventSession(Identifier Name, EventScope On) : Statement
	{
		/// <summary>T-SQL: ALTER EVENT SESSION (Transact-SQL). The events or the targets added or dropped, all of one kind; none where the statement writes none.</summary>
		public SqlList<EventSessionPiece> Pieces { get; init; } = [];

		/// <summary>T-SQL: ALTER EVENT SESSION (Transact-SQL). <c>WITH (setting, …)</c>, alone or after the pieces; none where no <c>WITH</c> is written.</summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: ALTER EVENT SESSION (Transact-SQL). <c>STATE = START | STOP</c>, said alone; null where none is written.</summary>
		public EventSessionState? State { get; init; }
	}
}

/// <summary>
/// T-SQL: CREATE EVENT NOTIFICATION (Transact-SQL), CREATE SERVER AUDIT SPECIFICATION (Transact-SQL). A run of words the grammar's
/// <c>ActionWords</c> reads: an event, an audit action or a group of them, <c>OBJECT_CREATED</c>, <c>SELECT</c>; a word is any
/// run of a name's characters, a reserved one included, but <c>ON</c>, <c>TO</c>, <c>FROM</c>, <c>WITH</c> and <c>CASCADE</c>.
/// </summary>
public sealed record ActionWords(SqlList<Identifier> Words) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: CREATE SERVER AUDIT (Transact-SQL), ALTER SERVER AUDIT (Transact-SQL). Where an audit writes.</summary>
public abstract record AuditTarget : ISqlNode
{
	/// <summary>Where in the text this target was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this target was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// <c>FILE (setting, …)</c>: FILEPATH, MAXSIZE, MAX_ROLLOVER_FILES, MAX_FILES and RESERVE_DISK_SPACE, in any order and as often
	/// as wanted, one at least; a file made names its path among them.
	/// </summary>
	public sealed record File(SqlList<Option> Options) : AuditTarget;

	/// <summary><c>APPLICATION_LOG</c>.</summary>
	public sealed record ApplicationLog : AuditTarget;

	/// <summary><c>SECURITY_LOG</c>.</summary>
	public sealed record SecurityLog : AuditTarget;

	/// <summary><c>URL</c>, Managed Instance's, which the grammar reads by its published syntax.</summary>
	public sealed record Url : AuditTarget;

	/// <summary><c>EXTERNAL_MONITOR</c>, Azure's.</summary>
	public sealed record ExternalMonitor : AuditTarget;
}

/// <summary>T-SQL: ALTER SERVER AUDIT (Transact-SQL). What an ALTER SERVER AUDIT does.</summary>
public abstract record AuditChange : ISqlNode
{
	/// <summary>Where in the text this change was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this change was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>REMOVE WHERE</c>.</summary>
	public sealed record RemoveWhere : AuditChange;

	/// <summary><c>MODIFY NAME = name</c>.</summary>
	public sealed record ModifyName(Identifier Name) : AuditChange;

	/// <summary>
	/// <c>[TO target] [WITH (setting, …)] [WHERE condition]</c>: a target, settings or a condition, one at least, and a target
	/// before the settings.
	/// </summary>
	public sealed record Settings : AuditChange
	{
		/// <summary>T-SQL: ALTER SERVER AUDIT (Transact-SQL). <c>TO target</c>; null where none is written.</summary>
		public AuditTarget? Target { get; init; }

		/// <summary>T-SQL: ALTER SERVER AUDIT (Transact-SQL). <c>WITH (setting, …)</c>: QUEUE_DELAY, ON_FAILURE, OPERATOR_AUDIT and STATE, as often as wanted; none where no <c>WITH</c> is written.</summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: ALTER SERVER AUDIT (Transact-SQL). <c>WHERE condition</c>; null where none is written.</summary>
		public Expression? Where { get; init; }
	}
}

/// <summary>
/// T-SQL: CREATE SERVER AUDIT SPECIFICATION (Transact-SQL), CREATE DATABASE AUDIT SPECIFICATION. One <c>ADD (…)</c> or <c>DROP (…)</c>
/// of an audit specification: the actions, or groups of them, and what they are watched on.
/// </summary>
public sealed record AuditAction(AddOrDrop Action, SqlList<ActionWords> Actions) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>T-SQL: CREATE DATABASE AUDIT SPECIFICATION. <c>ON class::object BY principal, …</c>; null where none is written.</summary>
	public AuditActionOn? On { get; init; }

	/// <summary>
	/// T-SQL: CREATE SERVER AUDIT SPECIFICATION (Transact-SQL). A comma written after the action's bracket, which the grammar reads
	/// between two actions and after the last: the same specification.
	/// </summary>
	[Spelling] public bool Comma { get; init; }
}

/// <summary>T-SQL: CREATE DATABASE AUDIT SPECIFICATION. What an audit action is watched on, and for whom: <c>ON OBJECT::dbo.t BY dbo, NULL</c>.</summary>
public sealed record AuditActionOn(SecurableClass? Class, QualifiedName Name, SqlList<Grantee> Grantees) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: CREATE EVENT SESSION (Transact-SQL), ALTER EVENT SESSION (Transact-SQL). One piece of an event session.</summary>
public abstract record EventSessionPiece : ISqlNode
{
	/// <summary>Where in the text this piece was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this piece was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// <c>ADD EVENT name [(SET setting, … ACTION (name, …) WHERE predicate)]</c>: what the event collects, the actions it takes and
	/// when it fires, in that order and each once. The event is named in one part, two or three.
	/// </summary>
	public sealed record AddEvent(QualifiedName Event) : EventSessionPiece
	{
		/// <summary>T-SQL: CREATE EVENT SESSION (Transact-SQL). <c>SET setting = value, …</c>: names of one part and values the grammar's <c>EventValue</c> reads; none where no <c>SET</c> is written.</summary>
		public SqlList<Option> Settings { get; init; } = [];

		/// <summary>T-SQL: CREATE EVENT SESSION (Transact-SQL). <c>ACTION (name, …)</c>, each a name of one part, two or three; none where no <c>ACTION</c> is written.</summary>
		public SqlList<QualifiedName> Actions { get; init; } = [];

		/// <summary>T-SQL: CREATE EVENT SESSION (Transact-SQL). <c>WHERE predicate</c>; null where none is written.</summary>
		public EventPredicate? Where { get; init; }
	}

	/// <summary><c>DROP EVENT name</c>.</summary>
	public sealed record DropEvent(QualifiedName Event) : EventSessionPiece;

	/// <summary><c>ADD TARGET name [(SET setting = value, …)]</c>.</summary>
	public sealed record AddTarget(QualifiedName Target) : EventSessionPiece
	{
		/// <summary>T-SQL: CREATE EVENT SESSION (Transact-SQL). <c>(SET setting = value, …)</c>: the target's parameters; none where no <c>SET</c> is written.</summary>
		public SqlList<Option> Settings { get; init; } = [];
	}

	/// <summary><c>DROP TARGET name</c>.</summary>
	public sealed record DropTarget(QualifiedName Target) : EventSessionPiece;
}

/// <summary>
/// T-SQL: CREATE EVENT SESSION (Transact-SQL). An event's predicate, the grammar's <c>EventPredicate</c>: terms joined by AND and OR,
/// which bind alike and from the left, with NOT in front of a term and brackets around one.
/// </summary>
public abstract record EventPredicate : ISqlNode
{
	/// <summary>Where in the text this predicate was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this predicate was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>left AND right</c>, <c>left OR right</c>: the left is any predicate and the right a term, the two joins binding alike.</summary>
	public sealed record Join(EventPredicate Left, EventJoin Operator, EventPredicate Right) : EventPredicate;

	/// <summary><c>NOT term</c>: of a term, and as often as wanted.</summary>
	public sealed record Not(EventPredicate Operand) : EventPredicate;

	/// <summary><c>(predicate)</c>.</summary>
	public sealed record Parenthesized(EventPredicate Predicate) : EventPredicate;

	/// <summary>
	/// <c>package.compare (field, value)</c>: a comparison the package supplies (<c>EventCall</c>); the name is two parts or three,
	/// the field one part, two or three.
	/// </summary>
	public sealed record Call(QualifiedName Name, QualifiedName Field, Expression Value) : EventPredicate;

	/// <summary>
	/// <c>field = value</c> (<c>EventCompare</c>): the field comes first, named in one part, two or three, and the value is a number,
	/// a binary string or a string, in as many brackets as are written.
	/// </summary>
	public sealed record Compare(QualifiedName Field, ComparisonOperator Operator, Expression Value) : EventPredicate
	{
		/// <summary>T-SQL: CREATE EVENT SESSION (Transact-SQL). <c>!=</c> against <c>&lt;&gt;</c>: the same comparison, of <see cref="ComparisonOperator.NotEqual"/> alone.</summary>
		[Spelling] public bool Exclamation { get; init; }
	}

	/// <summary><c>field LIKE value</c>, <c>field NOT LIKE value</c> (<c>EventCompare</c>).</summary>
	public sealed record Like(QualifiedName Field, bool Negated, Expression Value) : EventPredicate;
}

/// <summary>T-SQL: CREATE EVENT SESSION (Transact-SQL). How an event's predicate terms are joined: <c>AND</c> or <c>OR</c> (<c>EventJoin</c>).</summary>
public enum EventJoin { And, Or }

/// <summary>T-SQL: ALTER EVENT SESSION (Transact-SQL). <c>STATE = START</c> or <c>STOP</c>.</summary>
public enum EventSessionState { Start, Stop }
