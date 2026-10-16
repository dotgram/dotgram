using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// Auditing and event capture: server audits, audit specifications, event sessions and event notifications.
	/// </summary>
	/// <remarks>
	/// The old tree keeps what follows the name of an event notification, an audit and an audit specification as text (<c>Tail</c>),
	/// which the arms read token by token as the grammar's rules read it. An event session is nodes, but for the settings it sets and
	/// the predicate of an event: the old tree keeps a setting's value in the shape the generic option rule gives it, which is read
	/// again from the text of the setting, and a predicate as the text written.
	/// </remarks>
	internal static class Monitoring
	{
		public static readonly string[] Owns =
		[
			"Statement.EventSessionDefinition", "Statement.EventNotificationDefinition", "Statement.ServerAuditDefinition",
			"Statement.AuditSpecificationDefinition", "Statement.DatabaseAuditSpecificationDefinition", "Clause.EventPiece",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.EventNotificationDefinition notification => Notification(notification),
				Statement.ServerAuditDefinition audit              => Audit(audit),
				Statement.AuditSpecificationDefinition server      => Specification(server, database: false),
				Statement.DatabaseAuditSpecificationDefinition db  => Specification(db, database: true),
				Statement.EventSessionDefinition session           => Session(session),
				Clause.EventPiece piece                            => Piece(piece),
				_ when Owned(Owns, old)                            => throw Inventory.Unconverted(old),
				_                                                  => null,
			};

			return made is not null;
		}

		// ── Event notifications and audits ────────────────────────────────────────

		static Ast.Statement Notification(Statement.EventNotificationDefinition old)
		{
			var reader = new Broker.Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			reader.Expect("ON");

			Ast.NotificationScope scope = reader.Take("SERVER") ? new Ast.NotificationScope.Server()
				: reader.Take("DATABASE") ? new Ast.NotificationScope.Database()
				: reader.Take("QUEUE") ? new Ast.NotificationScope.Queue(reader.Qualified())
				: throw reader.Unread();

			var fanIn = reader.Take("WITH", "FAN_IN");

			reader.Expect("FOR");

			var events = new List<Ast.ActionWords> { reader.ActionWords() };

			while (reader.TakePunctuation(","))
				events.Add(reader.ActionWords());

			reader.Expect("TO", "SERVICE");

			var services = reader.ValueList();

			return new Ast.Statement.CreateEventNotification(name, scope, events.ToSqlList(), services) { FanIn = fanIn };
		}

		static Ast.Statement Audit(Statement.ServerAuditDefinition old)
		{
			var reader = new Broker.Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			if (Broker.Verb(old) == "CREATE")
			{
				reader.Expect("TO");

				var target  = AuditTarget(reader);
				var options = reader.Take("WITH") ? reader.Bracketed() : [];
				var where   = reader.Take("WHERE") ? reader.Condition() : null;

				reader.Finish();

				return new Ast.Statement.CreateServerAudit(name, target) { Options = options, Where = where };
			}

			Ast.AuditChange change;

			if (reader.Take("REMOVE", "WHERE"))
			{
				change = new Ast.AuditChange.RemoveWhere();
			}
			else if (reader.Take("MODIFY", "NAME"))
			{
				reader.ExpectPunctuation("=");
				change = new Ast.AuditChange.ModifyName(reader.Name());
			}
			else
			{
				var target  = reader.Take("TO") ? AuditTarget(reader) : null;
				var options = reader.Take("WITH") ? reader.Bracketed() : [];
				var where   = reader.Take("WHERE") ? reader.Condition() : null;

				change = new Ast.AuditChange.Settings { Target = target, Options = options, Where = where };
			}

			reader.Finish();

			return Broker.Verb(old) == "ALTER" ? new Ast.Statement.AlterServerAudit(name, change) : throw reader.Unread();
		}

		static Ast.AuditTarget AuditTarget(Broker.Tail reader)
		{
			if (reader.Take("FILE"))
				return new Ast.AuditTarget.File(reader.Bracketed());

			if (reader.Take("APPLICATION_LOG"))
				return new Ast.AuditTarget.ApplicationLog();

			if (reader.Take("SECURITY_LOG"))
				return new Ast.AuditTarget.SecurityLog();

			if (reader.Take("URL"))
				return new Ast.AuditTarget.Url();

			if (reader.Take("EXTERNAL_MONITOR"))
				return new Ast.AuditTarget.ExternalMonitor();

			throw reader.Unread();
		}

		static Ast.Statement Specification(Statement.Definition old, bool database)
		{
			var reader  = new Broker.Tail(old, old.Tail);
			var name    = Values.Part(old, old.Name);
			Ast.QualifiedName? audit = reader.Take("FOR", "SERVER", "AUDIT") ? reader.Qualified() : null;
			var actions = new List<Ast.AuditAction>();

			while (reader.Is("ADD") || reader.Is("DROP"))
				actions.Add(Action(reader));

			var options = reader.Take("WITH") ? reader.Bracketed() : [];

			reader.Finish();

			var list = actions.ToSqlList();

			return (database, Broker.Verb(old)) switch
			{
				(false, "CREATE") => new Ast.Statement.CreateServerAuditSpecification(name) { Audit = audit, Actions = list, Options = options },
				(false, "ALTER")  => new Ast.Statement.AlterServerAuditSpecification(name) { Audit = audit, Actions = list, Options = options },
				(true, "CREATE")  => new Ast.Statement.CreateDatabaseAuditSpecification(name) { Audit = audit, Actions = list, Options = options },
				(true, "ALTER")   => new Ast.Statement.AlterDatabaseAuditSpecification(name) { Audit = audit, Actions = list, Options = options },
				_                 => throw reader.Unread(),
			};
		}

		/// <summary>One <c>ADD (…)</c> or <c>DROP (…)</c> and the comma after it, where there is one.</summary>
		static Ast.AuditAction Action(Broker.Tail reader)
		{
			var action = reader.Take("ADD") ? Ast.AddOrDrop.Add : reader.Take("DROP") ? Ast.AddOrDrop.Drop : throw reader.Unread();

			reader.ExpectPunctuation("(");

			var words = new List<Ast.ActionWords> { reader.ActionWords() };

			while (reader.TakePunctuation(","))
				words.Add(reader.ActionWords());

			Ast.AuditActionOn? on = null;

			if (reader.Take("ON"))
			{
				var kind = reader.Class();
				var item = reader.Qualified();

				reader.Expect("BY");

				var grantees = new List<Ast.Grantee> { Grantee(reader) };

				while (reader.TakePunctuation(","))
					grantees.Add(Grantee(reader));

				on = new Ast.AuditActionOn(kind, item, grantees.ToSqlList());
			}

			reader.ExpectPunctuation(")");

			return new Ast.AuditAction(action, words.ToSqlList()) { On = on, Comma = reader.TakePunctuation(",") };
		}

		static Ast.Grantee Grantee(Broker.Tail reader)
		{
			if (reader.Take("PUBLIC"))
				return new Ast.Grantee.Public();

			if (reader.Take("NULL"))
				return new Ast.Grantee.Null();

			return new Ast.Grantee.Identifier(new Ast.AuthorizationIdentifier(reader.Name()));
		}

		// ── Event sessions ────────────────────────────────────────────────────────

		/// <summary>A list the old tree leaves null where nothing was written, carried item by item.</summary>
		static SqlList<TNew> Listed<TOld, TNew>(SqlList<TOld>? items, Func<TOld, TNew> convert)
		{
			return items is { } list ? Each(list, convert) : [];
		}

		static Ast.Statement Session(Statement.EventSessionDefinition old)
		{
			var name  = Values.Part(old, old.Name);
			var scope = old.On.ToUpperInvariant() switch
			{
				"SERVER"   => Ast.EventScope.Server,
				"DATABASE" => Ast.EventScope.Database,
				_          => throw BridgeGap.Open(old, $"inventory Statement.EventSessionDefinition (ON {old.On})"),
			};

			var pieces  = Listed(old.Pieces, Piece);
			var options = Listed(old.Options, one => SessionOption(old, one));

			if (old.Verb == "CREATE")
				return new Ast.Statement.CreateEventSession(name, scope, pieces) { Options = options };

			Ast.EventSessionState? state = old.State?.ToUpperInvariant() switch
			{
				null    => null,
				"START" => Ast.EventSessionState.Start,
				"STOP"  => Ast.EventSessionState.Stop,
				_       => throw BridgeGap.Open(old, $"inventory Statement.EventSessionDefinition (STATE = {old.State})"),
			};

			return new Ast.Statement.AlterEventSession(name, scope) { Pieces = pieces, Options = options, State = state };
		}

		static Ast.EventSessionPiece Piece(Clause clause)
		{
			if (clause is not Clause.EventPiece old)
				throw BridgeGap.Open(clause, $"inventory {NameOf(clause)} (in an event session)");

			var name = Values.Name(old, old.Name);

			switch (old.Action)
			{
				case "ADD EVENT":
					return new Ast.EventSessionPiece.AddEvent(name)
					{
						Settings = Listed(old.Settings, one => EventSetting(old, one)),
						Actions  = Listed(old.Actions, one => Values.Name(old, one)),
						Where    = old.Where is { Length: > 0 } where ? Predicate(old, where) : null,
					};

				case "DROP EVENT":
					return new Ast.EventSessionPiece.DropEvent(name);

				case "ADD TARGET":
					return new Ast.EventSessionPiece.AddTarget(name) { Settings = Listed(old.Settings, one => EventSetting(old, one)) };

				case "DROP TARGET":
					return new Ast.EventSessionPiece.DropTarget(name);

				default:
					throw BridgeGap.Open(old, $"inventory Clause.EventPiece ({old.Action})");
			}
		}

		/// <summary>
		/// A setting of the session, read again from the text it was written in: a name, <c>=</c> and a value the catalogue reads, the
		/// name as written.
		/// </summary>
		static Ast.Option SessionOption(Statement.EventSessionDefinition owner, Clause old)
		{
			var read   = new Broker.Tail(owner, Spelled(old) ?? throw BridgeGap.Lost(owner, "the text of a setting"));
			var option = read.Setting();

			read.Finish();

			return option;
		}

		/// <summary>
		/// A setting an event or a target sets, read again from the text it was written in: the old tree keeps its value in the shape
		/// the generic option rule gives it, <c>(2)</c> as a column and <c>((0x10))</c> as a call, and the grammar's rule for the place
		/// is another one.
		/// </summary>
		static Ast.Option EventSetting(Clause.EventPiece owner, Clause old)
		{
			var read  = new Broker.Tail(owner, Spelled(old) ?? throw BridgeGap.Lost(owner, "the text of a setting, which the old tree keeps in the shape of the generic option rule"));
			var name  = read.Name();

			read.ExpectPunctuation("=");

			var value = read.EventValue();

			read.Finish();

			return new Ast.Option(name, value, []);
		}

		/// <summary>The words of every class a securable is written with, by the words the writer prints for it.</summary>
		internal static readonly Lazy<Dictionary<string, Ast.SecurableClass>> ClassWords = new(static () =>
		{
			var words = new Dictionary<string, Ast.SecurableClass>(StringComparer.Ordinal);

			foreach (var kind in Enum.GetValues<Ast.SecurableClass>())
			{
				var granted = Ast.Sql2023Writer.Write(new Ast.Statement.Grant
				{
					Body = new Ast.GrantBody.Privileges(
						[new Ast.Privilege(Ast.PrivilegeKind.Control, [], [])],
						new Ast.PrivilegeObject.Classed(kind, new Ast.QualifiedName([new Ast.Identifier("s")])), [new Ast.Grantee.Public()], false, false, null),
				});

				words[Regex.Match(granted, " ON (.+)::s TO ").Groups[1].Value] = kind;
			}

			return words;
		});

		static Ast.EventPredicate Predicate(Clause.EventPiece old, string text)
		{
			var reader = new Broker.Tail(old, text);
			var made   = reader.Predicate();

			reader.Finish();

			return made;
		}
	}
}
