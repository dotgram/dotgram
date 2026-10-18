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
/// The Monitoring family's writer (Sql2023Writer.TransactSql.Monitoring.cs): each record written as the words of its kind, read back
/// by TransactSqlParser to the same tree, and every tree whose text would read back as another tree or not at all refused.
/// </summary>
public sealed class MonitoringWriterTests
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

	static Option Opt(string name, Expression? value)
	{
		return new Option(new Identifier(name), value, []);
	}

	static Expression Measured(string amount, string unit)
	{
		return new Expression.Measured(Digits(amount), new Identifier(unit));
	}

	static ActionWords Run(params string[] words)
	{
		return new ActionWords([.. words.Select(static one => new Identifier(one))]);
	}

	static EventPredicate Equal(string field, Expression value)
	{
		return new EventPredicate.Compare(Dotted(field), ComparisonOperator.Equal, value);
	}

	static EventPredicate Joined(EventPredicate left, EventJoin op, EventPredicate right)
	{
		return new EventPredicate.Join(left, op, right);
	}

	static EventPredicate Chain(EventJoin op, params EventPredicate[] terms)
	{
		return terms.Skip(1).Aggregate(terms[0], (left, right) => Joined(left, op, right));
	}

	static readonly EventPredicate A = Equal("a", Digits("1"));
	static readonly EventPredicate B = Equal("b", Digits("2"));
	static readonly EventPredicate C = Equal("c", Digits("3"));

	static AuditAction Added(params string[] words)
	{
		return new AuditAction(AddOrDrop.Add, [Run(words)]);
	}

	static readonly Expression Two = Digits("2");

	static EventSessionPiece.AddEvent Event(string name)
	{
		return new EventSessionPiece.AddEvent(Dotted(name));
	}

	/// <summary>The trees whose text is checked, with the text the grammar reads them from.</summary>
	static readonly Dictionary<string, (ISqlNode Tree, string Text)> Texts = new(StringComparer.Ordinal)
	{
		["Create event notification"] = (
			new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("OBJECT_CREATED")], [Str("s"), Str("current database")]),
			"CREATE EVENT NOTIFICATION x ON SERVER FOR OBJECT_CREATED TO SERVICE 's', 'current database'"),
		["Create event notification of a database, fanned in"] = (
			new Statement.CreateEventNotification(X, new NotificationScope.Database(), [Run("a"), Run("b", "c")], [NStr("s")]) { FanIn = true },
			"CREATE EVENT NOTIFICATION x ON DATABASE WITH FAN_IN FOR a, b c TO SERVICE N's'"),
		["Create event notification of a queue"] = (
			new Statement.CreateEventNotification(X, new NotificationScope.Queue(Dotted("db", "", "q")), [Run("select"), Run("@x"), Run("$y"), Run("_z"), Run("é")], [Var("@s")]),
			"CREATE EVENT NOTIFICATION x ON QUEUE db..q FOR select, @x, $y, _z, é TO SERVICE @s"),
		["Create event notification with an event that begins like a word that ends the run"] = (
			new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("ons", "tox", "without")], [Str("s")]),
			"CREATE EVENT NOTIFICATION x ON SERVER FOR ons tox without TO SERVICE 's'"),

		["Create server audit"] = (
			new Statement.CreateServerAudit(X, new AuditTarget.ApplicationLog()),
			"CREATE SERVER AUDIT x TO APPLICATION_LOG"),
		["Create server audit to the logs"] = (
			new Statement.CreateServerAudit(X, new AuditTarget.SecurityLog()) { Options = [Opt("ON_FAILURE", Word("CONTINUE")), Opt("ON_FAILURE", Word("SHUTDOWN")), Opt("AUDIT_GUID", Str("g")), Opt("QUEUE_DELAY", Digits("5")), Opt("OPERATOR_AUDIT", Word("OFF"))] },
			"CREATE SERVER AUDIT x TO SECURITY_LOG WITH (ON_FAILURE = CONTINUE, ON_FAILURE = SHUTDOWN, AUDIT_GUID = 'g', QUEUE_DELAY = 5, OPERATOR_AUDIT = OFF)"),
		["Create server audit to a url and a monitor"] = (
			new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Where = new Expression.Comparison(Word("c1"), ComparisonOperator.Equal, Digits("1")) },
			"CREATE SERVER AUDIT x TO URL WHERE c1 = 1"),
		["Create server audit to a monitor"] = (
			new Statement.CreateServerAudit(X, new AuditTarget.ExternalMonitor()),
			"CREATE SERVER AUDIT x TO EXTERNAL_MONITOR"),
		["Create server audit to a file"] = (
			new Statement.CreateServerAudit(X, new AuditTarget.File([Opt("MAXSIZE", Measured("15", "TB")), Opt("MAXSIZE", Word("UNLIMITED")), Opt("FILEPATH", Str("p")), Opt("FILEPATH", NStr("q")), Opt("MAX_ROLLOVER_FILES", Word("UNLIMITED")), Opt("MAX_FILES", Digits("3")), Opt("RESERVE_DISK_SPACE", Word("ON"))])) { Options = [Opt("QUEUE_DELAY", Digits("0"))] },
			"CREATE SERVER AUDIT x TO FILE (MAXSIZE = 15 TB, MAXSIZE = UNLIMITED, FILEPATH = 'p', FILEPATH = N'q', MAX_ROLLOVER_FILES = UNLIMITED, MAX_FILES = 3, RESERVE_DISK_SPACE = ON) WITH (QUEUE_DELAY = 0)"),
		["Create server audit to a file of a size with a point"] = (
			new Statement.CreateServerAudit(X, new AuditTarget.File([Opt("FILEPATH", Str("p")), Opt("MAXSIZE", new Expression.Measured(Number("1.5", NumericLiteralKind.Decimal), new Identifier("mb"))), Opt("MAX_ROLLOVER_FILES", Digits("2"))])),
			"CREATE SERVER AUDIT x TO FILE (FILEPATH = 'p', MAXSIZE = 1.5 mb, MAX_ROLLOVER_FILES = 2)"),
		["Alter server audit removing its condition"] = (
			new Statement.AlterServerAudit(X, new AuditChange.RemoveWhere()),
			"ALTER SERVER AUDIT x REMOVE WHERE"),
		["Alter server audit renamed"] = (
			new Statement.AlterServerAudit(X, new AuditChange.ModifyName(new Identifier("[y]", IdentifierStyle.Bracketed))),
			"ALTER SERVER AUDIT x MODIFY NAME = [y]"),
		["Alter server audit to a file, its settings and a condition"] = (
			new Statement.AlterServerAudit(X, new AuditChange.Settings { Target = new AuditTarget.File([Opt("MAXSIZE", Measured("5", "GB"))]), Options = [Opt("STATE", Word("ON")), Opt("ON_FAILURE", Word("FAIL_OPERATION"))], Where = new Expression.Comparison(Word("c1"), ComparisonOperator.Equal, Digits("1")) }),
			"ALTER SERVER AUDIT x TO FILE (MAXSIZE = 5 GB) WITH (STATE = ON, ON_FAILURE = FAIL_OPERATION) WHERE c1 = 1"),
		["Alter server audit with its settings"] = (
			new Statement.AlterServerAudit(X, new AuditChange.Settings { Options = [Opt("STATE", Word("OFF")), Opt("STATE", Word("ON"))] }),
			"ALTER SERVER AUDIT x WITH (STATE = OFF, STATE = ON)"),
		["Alter server audit with a condition"] = (
			new Statement.AlterServerAudit(X, new AuditChange.Settings { Where = new Expression.Comparison(Word("c1"), ComparisonOperator.Equal, Digits("1")) }),
			"ALTER SERVER AUDIT x WHERE c1 = 1"),
		["Alter server audit to the logs"] = (
			new Statement.AlterServerAudit(X, new AuditChange.Settings { Target = new AuditTarget.SecurityLog() }),
			"ALTER SERVER AUDIT x TO SECURITY_LOG"),

		["Create server audit specification"] = (
			new Statement.CreateServerAuditSpecification(X),
			"CREATE SERVER AUDIT SPECIFICATION x"),
		["Create server audit specification with all"] = (
			new Statement.CreateServerAuditSpecification(X) { Audit = Dotted("a", "b"), Actions = [Added("FAILED_LOGIN_GROUP"), new AuditAction(AddOrDrop.Drop, [Run("g")])], Options = [Opt("STATE", Word("ON")), Opt("STATE", Word("OFF"))] },
			"CREATE SERVER AUDIT SPECIFICATION x FOR SERVER AUDIT a.b ADD (FAILED_LOGIN_GROUP) DROP (g) WITH (STATE = ON, STATE = OFF)"),
		["Alter server audit specification with commas"] = (
			new Statement.AlterServerAuditSpecification(X) { Actions = [Added("a") with { Comma = true }, new AuditAction(AddOrDrop.Drop, [Run("b"), Run("c", "d")]) { Comma = true }] },
			"ALTER SERVER AUDIT SPECIFICATION x ADD (a), DROP (b, c d),"),
		["Create database audit specification"] = (
			new Statement.CreateDatabaseAuditSpecification(X) { Audit = Dotted("a"), Actions = [new AuditAction(AddOrDrop.Add, [Run("SELECT"), Run("INSERT")]) { On = new AuditActionOn(null, Dotted("dbo", "t"), [new Grantee.Identifier(new AuthorizationIdentifier(new Identifier("dbo"))), new Grantee.Public(), new Grantee.Null()]) }] },
			"CREATE DATABASE AUDIT SPECIFICATION x FOR SERVER AUDIT a ADD (SELECT, INSERT ON dbo.t BY dbo, PUBLIC, NULL)"),
		["Alter database audit specification of a class"] = (
			new Statement.AlterDatabaseAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Drop, [Run("EXECUTE")]) { On = new AuditActionOn(SecurableClass.Object, Dotted("t"), [new Grantee.Identifier(new AuthorizationIdentifier(new Identifier("[NULL]", IdentifierStyle.Bracketed)))]) }] },
			"ALTER DATABASE AUDIT SPECIFICATION x DROP (EXECUTE ON OBJECT::t BY [NULL])"),
		["Create server audit specification of an object"] = (
			new Statement.CreateServerAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Add, [Run("a")]) { On = new AuditActionOn(SecurableClass.Database, Dotted("t"), [new Grantee.Null()]) }] },
			"CREATE SERVER AUDIT SPECIFICATION x ADD (a ON DATABASE::t BY NULL)"),

		["Create event session"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e"),
		["Create event session with all"] = (
			new Statement.CreateEventSession(X, EventScope.Database, [Event("e") with { Settings = [Opt("a", Digits("1")), Opt("b", NStr("x"))], Actions = [Dotted("sqlserver", "sql_text"), new QualifiedName([new Identifier("[guid]", IdentifierStyle.Bracketed), new Identifier("p"), new Identifier("n")])], Where = A }, Event("f"), new EventSessionPiece.AddTarget(Dotted("package0", "t")) { Settings = [Opt("filename", Str("f.xel"))] }, new EventSessionPiece.AddTarget(Dotted("u"))])
			{
				Options = [Opt("MAX_MEMORY", Measured("4096", "KB")), Opt("EVENT_RETENTION_MODE", Word("NO_EVENT_LOSS")), Opt("MAX_DISPATCH_LATENCY", Measured("30", "SECONDS")), Opt("MAX_DISPATCH_LATENCY", Word("INFINITE")), Opt("MEMORY_PARTITION_MODE", Word("PER_CPU")), Opt("TRACK_CAUSALITY", Word("ON")), Opt("MAX_DURATION", Measured("1", "DAYS")), Opt("MAX_DURATION", Word("UNLIMITED")), Opt("MAX_EVENT_SIZE", Measured("2", "mb")), Opt("STARTUP_STATE", Word("OFF"))],
			},
			"CREATE EVENT SESSION x ON DATABASE ADD EVENT e (SET a = 1, b = N'x' ACTION (sqlserver.sql_text, [guid].p.n) WHERE a = 1), ADD EVENT f ADD TARGET package0.t (SET filename = 'f.xel'), ADD TARGET u WITH (MAX_MEMORY = 4096 KB, EVENT_RETENTION_MODE = NO_EVENT_LOSS, MAX_DISPATCH_LATENCY = 30 SECONDS, MAX_DISPATCH_LATENCY = INFINITE, MEMORY_PARTITION_MODE = PER_CPU, TRACK_CAUSALITY = ON, MAX_DURATION = 1 DAYS, MAX_DURATION = UNLIMITED, MAX_EVENT_SIZE = 2 mb, STARTUP_STATE = OFF)"),
		["Create event session with a target and no event setting"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Actions = [Dotted("a")] }, new EventSessionPiece.AddTarget(Dotted("t"))]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (ACTION (a)) ADD TARGET t"),
		["Create event session whose event is told its predicate only"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = B }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE b = 2)"),
		["Alter event session adding events"] = (
			new Statement.AlterEventSession(X, EventScope.Server) { Pieces = [Event("e"), Event("f") with { Settings = [Opt("a", Digits("1"))] }], Options = [Opt("MAX_MEMORY", Measured("1", "MB"))] },
			"ALTER EVENT SESSION x ON SERVER ADD EVENT e, ADD EVENT f (SET a = 1) WITH (MAX_MEMORY = 1 MB)"),
		["Alter event session dropping events"] = (
			new Statement.AlterEventSession(X, EventScope.Database) { Pieces = [new EventSessionPiece.DropEvent(Dotted("e")), new EventSessionPiece.DropEvent(Dotted("p", "f"))] },
			"ALTER EVENT SESSION x ON DATABASE DROP EVENT e, DROP EVENT p.f"),
		["Alter event session adding targets"] = (
			new Statement.AlterEventSession(X, EventScope.Server) { Pieces = [new EventSessionPiece.AddTarget(Dotted("t")) { Settings = [Opt("a", Str("b"))] }] },
			"ALTER EVENT SESSION x ON SERVER ADD TARGET t (SET a = 'b')"),
		["Alter event session dropping targets"] = (
			new Statement.AlterEventSession(X, EventScope.Server) { Pieces = [new EventSessionPiece.DropTarget(Dotted("t"))] },
			"ALTER EVENT SESSION x ON SERVER DROP TARGET t"),
		["Alter event session by its settings"] = (
			new Statement.AlterEventSession(X, EventScope.Server) { Options = [Opt("TRACK_CAUSALITY", Word("OFF"))] },
			"ALTER EVENT SESSION x ON SERVER WITH (TRACK_CAUSALITY = OFF)"),
		["Alter event session started"] = (
			new Statement.AlterEventSession(X, EventScope.Server) { State = EventSessionState.Start },
			"ALTER EVENT SESSION x ON SERVER STATE = START"),
		["Alter event session stopped"] = (
			new Statement.AlterEventSession(X, EventScope.Database) { State = EventSessionState.Stop },
			"ALTER EVENT SESSION x ON DATABASE STATE = STOP"),

		// An event's predicate: terms joined from the left, NOT before a term, brackets around any predicate.
		["Predicate joined from the left"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = Joined(Joined(A, EventJoin.And, B), EventJoin.Or, C) }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE a = 1 AND b = 2 OR c = 3)"),
		["Predicate with a join in brackets on the right"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = Joined(A, EventJoin.Or, new EventPredicate.Parenthesized(Joined(B, EventJoin.And, C))) }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE a = 1 OR (b = 2 AND c = 3))"),
		["Predicate with a join in brackets on the left"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = Joined(new EventPredicate.Parenthesized(Joined(A, EventJoin.Or, B)), EventJoin.And, C) }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE (a = 1 OR b = 2) AND c = 3)"),
		["Predicate negated"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = Joined(new EventPredicate.Not(new EventPredicate.Not(A)), EventJoin.And, new EventPredicate.Not(B)) }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE NOT NOT a = 1 AND NOT b = 2)"),
		["Predicate negating a join in brackets"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = new EventPredicate.Not(new EventPredicate.Parenthesized(Joined(A, EventJoin.Or, B))) }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE NOT (a = 1 OR b = 2))"),
		["Predicate of every comparison"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = Chain(EventJoin.And,
				new EventPredicate.Compare(Dotted("a"), ComparisonOperator.NotEqual, Digits("1")),
				new EventPredicate.Compare(Dotted("a"), ComparisonOperator.NotEqual, Digits("1")) { Exclamation = true },
				new EventPredicate.Compare(Dotted("a"), ComparisonOperator.NotLess, Digits("1")),
				new EventPredicate.Compare(Dotted("a"), ComparisonOperator.NotGreater, Digits("1")),
				new EventPredicate.Compare(Dotted("a"), ComparisonOperator.GreaterOrEqual, Digits("1")),
				new EventPredicate.Compare(Dotted("a"), ComparisonOperator.LessOrEqual, Digits("1")),
				new EventPredicate.Compare(Dotted("a"), ComparisonOperator.Greater, Digits("1")),
				new EventPredicate.Compare(Dotted("a"), ComparisonOperator.Less, Digits("1"))) }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE a <> 1 AND a != 1 AND a !< 1 AND a !> 1 AND a >= 1 AND a <= 1 AND a > 1 AND a < 1)"),
		["Predicate with a like and a call"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = Joined(Joined(new EventPredicate.Like(Dotted("p", "a"), false, Str("x")), EventJoin.Or, new EventPredicate.Like(Dotted("a"), true, NStr("y"))), EventJoin.And, new EventPredicate.Call(Dotted("p", "f"), Dotted("p", "q", "a"), Number("-1.5", NumericLiteralKind.Decimal))) }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE p.a LIKE 'x' OR a NOT LIKE N'y' AND p.f(p.q.a, -1.5))"),
		["Predicate of values"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Where = Joined(Joined(Equal("a", new Expression.Parenthesized(new Expression.Parenthesized(Digits("1")))), EventJoin.And, Equal("a", new LiteralValue.Binary("10") { ZeroX = true })), EventJoin.And, Equal("a", Number("-2e3", NumericLiteralKind.Approximate))) }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (WHERE a = ((1)) AND a = 0x10 AND a = -2e3)"),
		["Settings of values"] = (
			new Statement.CreateEventSession(X, EventScope.Server, [Event("e") with { Settings = [Opt("a", Number("-1", NumericLiteralKind.DecimalInteger)), Opt("b", new Expression.Parenthesized(Digits("2"))), Opt("c", new LiteralValue.Binary("FF") { ZeroX = true }), Opt("d", Number("1.5e3", NumericLiteralKind.Approximate)), Opt("a", Str("again"))] }]),
			"CREATE EVENT SESSION x ON SERVER ADD EVENT e (SET a = -1, b = (2), c = 0xFF, d = 1.5e3, a = 'again')"),
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

	static Statement.CreateEventSession Session(EventSessionPiece piece)
	{
		return new Statement.CreateEventSession(X, EventScope.Server, [piece]);
	}

	static Statement.CreateEventSession Where(EventPredicate predicate)
	{
		return Session(Event("e") with { Where = predicate });
	}

	static Statement.CreateEventSession Setting(Expression value)
	{
		return Session(Event("e") with { Settings = [Opt("a", value)] });
	}

	static Statement.CreateServerAudit File(params Option[] options)
	{
		return new Statement.CreateServerAudit(X, new AuditTarget.File([.. options]));
	}

	static Statement.AlterServerAudit Altered(params Option[] options)
	{
		return new Statement.AlterServerAudit(X, new AuditChange.Settings { Options = [.. options] });
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names, above it, the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	static readonly Dictionary<string, ISqlNode> Refusals = new(StringComparer.Ordinal)
	{
		// ActionWords: a run of words, each a run of a name's characters that does not begin with a digit, no word of PermissionEnd.
		["an event of no word"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run()], [Str("s")]),
		["an event of a word that begins with a digit"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("1a")], [Str("s")]),
		["an event of a word of two words"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("a b")], [Str("s")]),
		["an event of a name in brackets"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [new ActionWords([new Identifier("[a]", IdentifierStyle.Bracketed)])], [Str("s")]),
		["an event of a bare #"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("#")], [Str("s")]),
		["an event of a ## with no name"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("##")], [Str("s")]),
		["an event of a $ and a digit"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("$1")], [Str("s")]),
		["an event of a # and a digit"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("#1")], [Str("s")]),
		["an event of a word with a dot"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("a.b")], [Str("s")]),
		["an event that ends the run: TO"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("to")], [Str("s")]),
		["an event that ends the run: ON"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("a", "ON")], [Str("s")]),
		["an event that ends the run: FROM"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("from")], [Str("s")]),
		["an event that ends the run: WITH"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("With")], [Str("s")]),
		["an event that ends the run: CASCADE"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("cascade")], [Str("s")]),
		["an event notification of no event"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [], [Str("s")]),
		["an event notification of no service"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("a")], []),
		["an event notification named by a variable"] = new Statement.CreateEventNotification(new Identifier("@x"), new NotificationScope.Server(), [Run("a")], [Str("s")]),
		["an event notification of a queue named by a variable"] = new Statement.CreateEventNotification(X, new NotificationScope.Queue(Dotted("@q")), [Run("a")], [Str("s")]),
		["an event notification to a service that is a condition"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("a")], [new Expression.Comparison(Digits("1"), ComparisonOperator.Equal, Digits("1"))]),
		["an event notification to a service that is a variable of two words"] = new Statement.CreateEventNotification(X, new NotificationScope.Server(), [Run("a")], [Var("@s s")]),

		// AuditCreatedTarget names the path of a file somewhere among its settings.
		["an audit made to a file with no path"] = File(Opt("MAXSIZE", Measured("1", "MB"))),
		["an audit made to a file with no setting"] = new Statement.CreateServerAudit(X, new AuditTarget.File([])),
		["an audit altered to a file with no setting"] = new Statement.AlterServerAudit(X, new AuditChange.Settings { Target = new AuditTarget.File([]) }),
		// AuditFileOption: MAXSIZE is an exact number and MB, GB or TB, or UNLIMITED; a size with no unit and a kilobyte are Msg 102.
		["an audit file size with no unit"] = File(Opt("FILEPATH", Str("p")), Opt("MAXSIZE", Digits("1"))),
		["an audit file size in kilobytes"] = File(Opt("FILEPATH", Str("p")), Opt("MAXSIZE", Measured("1", "KB"))),
		["an audit file size with an exponent"] = File(Opt("FILEPATH", Str("p")), Opt("MAXSIZE", new Expression.Measured(Number("1e3", NumericLiteralKind.Approximate), new Identifier("MB")))),
		["an audit file size that is signed"] = File(Opt("FILEPATH", Str("p")), Opt("MAXSIZE", new Expression.Measured(Number("-1", NumericLiteralKind.DecimalInteger), new Identifier("MB")))),
		["an audit file size in a unit in brackets"] = File(Opt("FILEPATH", Str("p")), Opt("MAXSIZE", new Expression.Measured(Digits("1"), new Identifier("[MB]", IdentifierStyle.Bracketed)))),
		["an audit file size of a measured word"] = File(Opt("FILEPATH", Str("p")), Opt("MAXSIZE", new Expression.Measured(Digits("1"), new Identifier("M B")))),
		["an audit file path that is a name"] = File(Opt("FILEPATH", Word("p"))),
		["an audit file path that is a variable"] = File(Opt("FILEPATH", Var("@p"))),
		["an audit file count of -1"] = File(Opt("FILEPATH", Str("p")), Opt("MAX_ROLLOVER_FILES", Number("-1", NumericLiteralKind.DecimalInteger))),
		["an audit file count that is a decimal"] = File(Opt("FILEPATH", Str("p")), Opt("MAX_FILES", Number("1.5", NumericLiteralKind.Decimal))),
		["an audit file count outside its words"] = File(Opt("FILEPATH", Str("p")), Opt("MAX_ROLLOVER_FILES", Word("many"))),
		["an audit file setting it has not"] = File(Opt("FILEPATH", Str("p")), Opt("MAX_SIZE", Digits("1"))),
		["an audit file setting that reserves a name"] = File(Opt("FILEPATH", Str("p")), Opt("RESERVE_DISK_SPACE", Word("YES"))),
		// AuditOption: the delay in whole milliseconds, ON_FAILURE from its three words.
		["an audit delay that is a decimal"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [Opt("QUEUE_DELAY", Number("1.5", NumericLiteralKind.Decimal))] },
		["an audit delay that is a string"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [Opt("QUEUE_DELAY", Str("1"))] },
		["an audit failure outside its words"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [Opt("ON_FAILURE", Word("IGNORE"))] },
		["an audit failure that is a string"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [Opt("ON_FAILURE", Str("CONTINUE"))] },
		["an audit failure in brackets"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [Opt("ON_FAILURE", new Expression.Reference(new QualifiedName([new Identifier("[CONTINUE]", IdentifierStyle.Bracketed)])))] },
		// AuditCreatedOption has AUDIT_GUID and no STATE, AuditAlteredOption the other way round.
		["an audit made with a state"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [Opt("STATE", Word("ON"))] },
		["an audit altered with a guid"] = Altered(Opt("AUDIT_GUID", Str("g"))),
		["an audit guid that is a name"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [Opt("AUDIT_GUID", Word("g"))] },
		["an audit setting with its = omitted"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [new Option(new Identifier("QUEUE_DELAY"), Digits("1"), []) { EqualsOmitted = true }] },
		["an audit setting that has no value"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [Opt("QUEUE_DELAY", null)] },
		["an audit setting of two words"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Options = [new Option(new Identifier("QUEUE_DELAY"), Digits("1"), []) { Words = [new Identifier("X")] }] },
		// AuditWhere is a search condition: a value there is Msg 4145.
		["an audit condition that is a value"] = new Statement.CreateServerAudit(X, new AuditTarget.Url()) { Where = Digits("1") },
		["an altered audit condition that is a value"] = new Statement.AlterServerAudit(X, new AuditChange.Settings { Where = Digits("1") }),
		// AlterAuditBody: an audit altered is altered by something.
		["an audit altered by nothing"] = new Statement.AlterServerAudit(X, new AuditChange.Settings()),
		["an audit renamed to a variable"] = new Statement.AlterServerAudit(X, new AuditChange.ModifyName(new Identifier("@y"))),
		["an audit named by a reserved word"] = new Statement.CreateServerAudit(new Identifier("table"), new AuditTarget.Url()),

		// AuditAction: one word at least in the brackets; ON a name BY principals, at least one, each an identifier or PUBLIC or NULL.
		["a specification action of no word"] = new Statement.CreateServerAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Add, [])] },
		["a specification action that neither adds nor drops"] = new Statement.CreateServerAuditSpecification(X) { Actions = [new AuditAction((AddOrDrop)9, [Run("a")])] },
		["a specification action on a name by no one"] = new Statement.CreateDatabaseAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Add, [Run("a")]) { On = new AuditActionOn(null, Dotted("t"), []) }] },
		["a specification action on a name that is a variable"] = new Statement.CreateDatabaseAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Add, [Run("a")]) { On = new AuditActionOn(null, Dotted("@t"), [new Grantee.Null()]) }] },
		["a specification action by a principal of two parts"] = new Statement.CreateDatabaseAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Add, [Run("a")]) { On = new AuditActionOn(null, Dotted("t"), [new Grantee.Identifier(new AuthorizationIdentifier(new Identifier("x.y")))]) }] },
		["a specification action by a principal that is NULL, plain"] = new Statement.CreateDatabaseAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Add, [Run("a")]) { On = new AuditActionOn(null, Dotted("t"), [new Grantee.Identifier(new AuthorizationIdentifier(new Identifier("null")))]) }] },
		["a specification action by a principal that is a variable"] = new Statement.CreateDatabaseAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Add, [Run("a")]) { On = new AuditActionOn(null, Dotted("t"), [new Grantee.Identifier(new AuthorizationIdentifier(new Identifier("@p")))]) }] },
		["a specification action of a class with no word"] = new Statement.CreateDatabaseAuditSpecification(X) { Actions = [new AuditAction(AddOrDrop.Add, [Run("a")]) { On = new AuditActionOn((SecurableClass)999, Dotted("t"), [new Grantee.Null()]) }] },
		["a specification for an audit that is a variable"] = new Statement.CreateServerAuditSpecification(X) { Audit = Dotted("@a") },
		["a specification named by a variable"] = new Statement.AlterServerAuditSpecification(new Identifier("@x")),
		// AuditStateWith: the state, ON or OFF.
		["a specification state that is a name"] = new Statement.CreateServerAuditSpecification(X) { Options = [Opt("STATE", Word("MAYBE"))] },
		["a specification setting it has not"] = new Statement.CreateServerAuditSpecification(X) { Options = [Opt("QUEUE_DELAY", Digits("1"))] },

		// EventAdds: events first, one at least, targets after; nothing dropped from a session made.
		["a session made of no piece"] = new Statement.CreateEventSession(X, EventScope.Server, []),
		["a session made of a target only"] = Session(new EventSessionPiece.AddTarget(Dotted("t"))),
		["a session made of an event after a target"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e"), new EventSessionPiece.AddTarget(Dotted("t")), Event("f")]),
		["a session made dropping an event"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e"), new EventSessionPiece.DropEvent(Dotted("f"))]),
		["a session made dropping a target"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e"), new EventSessionPiece.DropTarget(Dotted("t"))]),
		// EventAlterations: events added, events dropped, targets added or targets dropped, one of the four (Msg 156).
		["a session altered adding an event and a target"] = new Statement.AlterEventSession(X, EventScope.Server) { Pieces = [Event("e"), new EventSessionPiece.AddTarget(Dotted("t"))] },
		["a session altered adding and dropping events"] = new Statement.AlterEventSession(X, EventScope.Server) { Pieces = [Event("e"), new EventSessionPiece.DropEvent(Dotted("f"))] },
		["a session altered dropping an event and a target"] = new Statement.AlterEventSession(X, EventScope.Server) { Pieces = [new EventSessionPiece.DropEvent(Dotted("e")), new EventSessionPiece.DropTarget(Dotted("t"))] },
		// A state is said alone (Msg 102).
		["a session altered with a state and a piece"] = new Statement.AlterEventSession(X, EventScope.Server) { State = EventSessionState.Start, Pieces = [Event("e")] },
		["a session altered with a state and settings"] = new Statement.AlterEventSession(X, EventScope.Server) { State = EventSessionState.Start, Options = [Opt("TRACK_CAUSALITY", Word("ON"))] },
		["a session altered with a state that is no state"] = new Statement.AlterEventSession(X, EventScope.Server) { State = (EventSessionState)9 },
		["a session altered by nothing"] = new Statement.AlterEventSession(X, EventScope.Server),
		["a session of a scope that is none"] = new Statement.AlterEventSession(X, (EventScope)9) { State = EventSessionState.Stop },
		["a session named by a variable"] = new Statement.AlterEventSession(new Identifier("@s"), EventScope.Server) { State = EventSessionState.Stop },
		// EventObject: one part, two or three, and never four; no part left out; each an identifier.
		["an event of four parts"] = Session(new EventSessionPiece.AddEvent(Dotted("a", "b", "c", "d"))),
		["an event with a part left out"] = Session(new EventSessionPiece.AddEvent(Dotted("a", "", "c"))),
		["an event named by a variable"] = Session(new EventSessionPiece.AddEvent(Dotted("@e"))),
		["an event named by a reserved word"] = Session(new EventSessionPiece.AddEvent(Dotted("p", "table"))),
		["a target of four parts"] = Session(Event("e")) with { Pieces = [Event("e"), new EventSessionPiece.AddTarget(Dotted("a", "b", "c", "d"))] },
		["an action of four parts"] = Session(Event("e") with { Actions = [Dotted("a", "b", "c", "d")] }),
		// EventSetting: a plain name, one part, not reserved, not in brackets (`[a] = 1` is Msg 102), set once to an EventValue.
		["a setting named in brackets"] = Session(Event("e") with { Settings = [new Option(new Identifier("[a]", IdentifierStyle.Bracketed), Digits("1"), [])] }),
		["a setting named by a reserved word"] = Session(Event("e") with { Settings = [Opt("select", Digits("1"))] }),
		["a setting named by a temporary name"] = Session(Event("e") with { Settings = [Opt("#a", Digits("1"))] }),
		["a setting named in two words"] = Session(Event("e") with { Settings = [Opt("a b", Digits("1"))] }),
		["a setting that omits its ="] = Session(Event("e") with { Settings = [new Option(new Identifier("a"), Digits("1"), []) { EqualsOmitted = true }] }),
		["a setting that has a name of two parts"] = Session(Event("e") with { Settings = [new Option(new Identifier("a"), Digits("1"), []) { Words = [new Identifier("b")] }] }),
		["a setting with no value"] = Session(Event("e") with { Settings = [Opt("a", null)] }),
		// EventValue: a number with a minus, a binary string or a string, in brackets: not a name, NULL, a variable, money or an expression.
		["a setting value that is a name"] = Setting(Word("v")),
		["a setting value that is NULL"] = Setting(new LiteralValue.Null()),
		["a setting value that is a variable"] = Setting(Var("@v")),
		["a setting value that is money"] = Setting(Number("$1", NumericLiteralKind.Money)),
		["a setting value that is a plus"] = Setting(Number("+1", NumericLiteralKind.DecimalInteger)),
		["a setting value that is signed twice"] = Setting(Number("--1", NumericLiteralKind.DecimalInteger)),
		["a setting value that is a minus over a number"] = Setting(new Expression.Unary(UnaryOperator.Minus, Digits("1"))),
		["a setting value that is a sum"] = Setting(new Expression.Binary(Digits("1"), BinaryOperator.Add, Digits("2"))),
		["a setting value that is a standard binary string"] = Setting(new LiteralValue.Binary("'10'")),
		["a setting value that is a hexadecimal number"] = Setting(Number("0x10", NumericLiteralKind.HexInteger)),
		["a setting value that is a string with a character set"] = Setting(new LiteralValue.String("'x'", StringLiteralKind.Character) { CharacterSet = new CharacterSetName(Dotted("utf8")) }),
		["a setting value of a kind its text is not"] = Setting(Number("1", NumericLiteralKind.Decimal)),
		["a setting value that is a number and a unit"] = Setting(Measured("1", "KB")),
		["a setting value that is a call"] = Setting(new Expression.Invocation(new QualifiedName([new Identifier("f")]), [])),
		// ACTION reads a name or more in its brackets.
		["a setting value in brackets that is a name"] = Setting(new Expression.Parenthesized(Word("v"))),
		// The event session's options are the closed list, each value its own kind (Msg 25703 and 102).
		["a session option it has not"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_SIZE", Digits("1"))] },
		["a session size with no unit"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_MEMORY", Digits("1"))] },
		["a session size in seconds"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_MEMORY", Measured("1", "SECONDS"))] },
		["a session size that is signed"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_MEMORY", new Expression.Measured(Number("-1", NumericLiteralKind.DecimalInteger), new Identifier("KB")))] },
		["a session size that is a decimal"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_EVENT_SIZE", new Expression.Measured(Number("1.5", NumericLiteralKind.Decimal), new Identifier("KB")))] },
		["a session latency in minutes"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_DISPATCH_LATENCY", Measured("1", "MINUTES"))] },
		["a session latency that is UNLIMITED"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_DISPATCH_LATENCY", Word("UNLIMITED"))] },
		["a session duration that is INFINITE"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_DURATION", Word("INFINITE"))] },
		["a session duration in weeks"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MAX_DURATION", Measured("1", "WEEKS"))] },
		["a session retention outside its words"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("EVENT_RETENTION_MODE", Word("SOME_EVENT_LOSS"))] },
		["a session partition mode outside its words"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("MEMORY_PARTITION_MODE", Word("PER_SOCKET"))] },
		["a session switch that is a number"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("TRACK_CAUSALITY", Digits("1"))] },
		["a session switch that is a string"] = new Statement.CreateEventSession(X, EventScope.Server, [Event("e")]) { Options = [Opt("STARTUP_STATE", Str("ON"))] },

		// EventPredicate: the field comes first, a call names a package, the values are EventValues.
		["a predicate of a field of four parts"] = Where(new EventPredicate.Compare(Dotted("a", "b", "c", "d"), ComparisonOperator.Equal, Digits("1"))),
		["a predicate of a field that is a variable"] = Where(new EventPredicate.Compare(Dotted("@a"), ComparisonOperator.Equal, Digits("1"))),
		["a predicate of a value that is a field"] = Where(Equal("a", Word("b"))),
		["a predicate of a value that is a variable"] = Where(Equal("a", Var("@v"))),
		["a predicate of a value that is NULL"] = Where(Equal("a", new LiteralValue.Null())),
		["a predicate of a value that is a minus over a number"] = Where(Equal("a", new Expression.Unary(UnaryOperator.Minus, Digits("1")))),
		["a predicate of a value that is signed with a plus"] = Where(Equal("a", Number("+1", NumericLiteralKind.DecimalInteger))),
		["a predicate of a value that is a hexadecimal number"] = Where(Equal("a", Number("0x10", NumericLiteralKind.HexInteger))),
		["a predicate of a value that is a bracketed name"] = Where(Equal("a", new Expression.Parenthesized(Word("b")))),
		["a predicate of an exclamation on a comparison that is not an inequality"] = Where(new EventPredicate.Compare(Dotted("a"), ComparisonOperator.Equal, Digits("1")) { Exclamation = true }),
		["a predicate of an operator with no text"] = Where(new EventPredicate.Compare(Dotted("a"), (ComparisonOperator)99, Digits("1"))),
		["a like of a value that is a name"] = Where(new EventPredicate.Like(Dotted("a"), false, Word("b"))),
		["a call of a name of one part"] = Where(new EventPredicate.Call(Dotted("f"), Dotted("a"), Digits("1"))),
		["a call of a name of four parts"] = Where(new EventPredicate.Call(Dotted("a", "b", "c", "d"), Dotted("a"), Digits("1"))),
		["a call of a field of four parts"] = Where(new EventPredicate.Call(Dotted("p", "f"), Dotted("a", "b", "c", "d"), Digits("1"))),
		["a call of a value that is a name"] = Where(new EventPredicate.Call(Dotted("p", "f"), Dotted("a"), Word("b"))),
		["a call of a name that is a variable"] = Where(new EventPredicate.Call(Dotted("p", "@f"), Dotted("a"), Digits("1"))),
		// The joins bind alike and from the left: a join on the right of one, with no brackets, reads back from the left.
		["a join with a join on its right"] = Where(Joined(A, EventJoin.Or, Joined(B, EventJoin.And, C))),
		["a join with a join on its right, of the same operator"] = Where(Joined(A, EventJoin.And, Joined(B, EventJoin.And, C))),
		// NOT applies to the term after it: under a join with no brackets, the join reads from above it.
		["a negation of a join"] = Where(new EventPredicate.Not(Joined(A, EventJoin.And, B))),
		["a negation of a negation of a join"] = Where(new EventPredicate.Not(new EventPredicate.Not(Joined(A, EventJoin.Or, B)))),
		["a join that is neither"] = Where(Joined(A, (EventJoin)9, B)),
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
