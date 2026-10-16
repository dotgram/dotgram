using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Monitoring family (auditing and event capture): writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutMonitoring(ISqlNode node)
		{
			switch (node)
			{
				case Statement.CreateEventNotification create:
					Word("CREATE EVENT NOTIFICATION");
					PutOneName(create.Name, "CREATE EVENT NOTIFICATION");
					Word("ON");

					switch (create.On)
					{
						case NotificationScope.Server:
							Word("SERVER");
							break;

						case NotificationScope.Database:
							Word("DATABASE");
							break;

						case NotificationScope.Queue queue:
							Word("QUEUE");
							PutNameOf(queue.Name, "CREATE EVENT NOTIFICATION");
							break;

						default:
							throw Unwritten(create.On);
					}

					if (create.FanIn)
						Word("WITH FAN_IN");

					if (create.Events.Count == 0)
						throw Empty(create, "Events");

					if (create.Services.Count == 0)
						throw Empty(create, "Services");

					Word("FOR");
					Each(create.Events, one => PutActionWords(one, "CREATE EVENT NOTIFICATION"));
					Word("TO SERVICE");
					Each(create.Services, one => PutValue(one, "CREATE EVENT NOTIFICATION"));
					return true;

				case Statement.CreateServerAudit create:
					Word("CREATE SERVER AUDIT");
					PutOneName(create.Name, "CREATE SERVER AUDIT");
					Word("TO");
					PutAuditTarget(create.Target, created: true);
					PutAuditSettings(create, create.Options, CreateAuditSettings, "CREATE SERVER AUDIT");

					if (create.Where is { } where)
					{
						Word("WHERE");
						PutCondition(create, where);
					}

					return true;

				case Statement.AlterServerAudit alter:
					Word("ALTER SERVER AUDIT");
					PutOneName(alter.Name, "ALTER SERVER AUDIT");
					PutAuditChange(alter.Change);
					return true;

				case Statement.CreateServerAuditSpecification create:
					Word("CREATE SERVER AUDIT SPECIFICATION");
					PutSpecification(create, create.Name, create.Audit, create.Actions, create.Options, "CREATE SERVER AUDIT SPECIFICATION");
					return true;

				case Statement.AlterServerAuditSpecification alter:
					Word("ALTER SERVER AUDIT SPECIFICATION");
					PutSpecification(alter, alter.Name, alter.Audit, alter.Actions, alter.Options, "ALTER SERVER AUDIT SPECIFICATION");
					return true;

				case Statement.CreateDatabaseAuditSpecification create:
					Word("CREATE DATABASE AUDIT SPECIFICATION");
					PutSpecification(create, create.Name, create.Audit, create.Actions, create.Options, "CREATE DATABASE AUDIT SPECIFICATION");
					return true;

				case Statement.AlterDatabaseAuditSpecification alter:
					Word("ALTER DATABASE AUDIT SPECIFICATION");
					PutSpecification(alter, alter.Name, alter.Audit, alter.Actions, alter.Options, "ALTER DATABASE AUDIT SPECIFICATION");
					return true;

				case Statement.CreateEventSession create:
					Word("CREATE EVENT SESSION");
					PutOneName(create.Name, "CREATE EVENT SESSION");
					PutEventScope(create.On);
					PutEventPieces(create, create.Pieces, created: true);
					PutEventSessionOptions(create, create.Options);
					return true;

				case Statement.AlterEventSession alter:
					PutAlterEventSession(alter);
					return true;

				default:
					return false;
			}
		}

		// ── Event notifications and audits ────────────────────────────────────────

		/// <summary>
		/// A run of words, <see cref="ActionWords"/>: one at least, each a run of a name's characters that does not begin with a digit —
		/// a reserved word too — and is none of the words that end the run (<c>PermissionEnd</c>).
		/// </summary>
		void PutActionWords(ActionWords run, string place)
		{
			if (run.Words.Count == 0)
				throw Empty(run, "Words");

			foreach (var word in run.Words)
			{
				if (word.Style != IdentifierStyle.Regular || word.UnicodeEscape is not null || !Regex.IsMatch(word.Text, @"^[\p{L}_@#$][\p{L}\p{Nd}_@#$]*\z"))
					throw Misplaced(word, "Text", $"that is no word in {place}: a run of a name's characters that does not begin with a digit");

				if (Plain(word, "ON", "TO", "FROM", "WITH", "CASCADE"))
					throw Misplaced(word, "Text", $"that ends a run of words in {place} (PermissionEnd)");

				// Held to a name's characters above, which include @ # $ that a key word's text does not.
				Word(word.Text);
			}
		}

		/// <summary>Where an audit writes: a file and its settings, or one of the logs and the monitor.</summary>
		void PutAuditTarget(AuditTarget target, bool created)
		{
			switch (target)
			{
				case AuditTarget.File file:
					if (file.Options.Count == 0)
						throw Empty(file, "Options");

					// A file made is told its path somewhere among the settings (AuditCreatedTarget, Msg 102).
					if (created && !file.Options.Any(static one => one.Words.Count == 0 && Plain(one.Name, "FILEPATH")))
						throw Misplaced(file, "Options", "in CREATE SERVER AUDIT without FILEPATH, which a file made is told");

					Word("FILE");
					Open();
					PutBrokerSettings(file, file.Options, AuditFileSettings, "an audit's file", required: [], apart: []);
					Close();
					break;

				case AuditTarget.ApplicationLog:
					Word("APPLICATION_LOG");
					break;

				case AuditTarget.SecurityLog:
					Word("SECURITY_LOG");
					break;

				case AuditTarget.Url:
					Word("URL");
					break;

				case AuditTarget.ExternalMonitor:
					Word("EXTERNAL_MONITOR");
					break;

				default:
					throw Unwritten(target);
			}
		}

		/// <summary><c>WITH (setting, …)</c> of an audit, or nothing for no setting.</summary>
		void PutAuditSettings(ISqlNode owner, SqlList<Option> options, Dictionary<string, SettingEntry> catalogue, string place)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Open();
			PutBrokerSettings(owner, options, catalogue, place, required: [], apart: []);
			Close();
		}

		void PutAuditChange(AuditChange change)
		{
			switch (change)
			{
				case AuditChange.RemoveWhere:
					Word("REMOVE WHERE");
					break;

				case AuditChange.ModifyName modify:
					Word("MODIFY NAME =");
					PutOneName(modify.Name, "ALTER SERVER AUDIT");
					break;

				// AlterAuditBody: a target and then the settings and the condition, or the settings and the condition, or the condition.
				case AuditChange.Settings settings:
					if (settings.Target is null && settings.Options.Count == 0 && settings.Where is null)
						throw Empty(settings, "Target", "Options", "Where");

					if (settings.Target is { } target)
					{
						Word("TO");
						PutAuditTarget(target, created: false);
					}

					PutAuditSettings(settings, settings.Options, AlterAuditSettings, "ALTER SERVER AUDIT");

					if (settings.Where is { } where)
					{
						Word("WHERE");
						PutCondition(settings, where);
					}

					break;

				default:
					throw Unwritten(change);
			}
		}

		/// <summary>
		/// What the four audit specifications share: the name, the audit, the actions and the state, in that order, each optional.
		/// </summary>
		void PutSpecification(ISqlNode owner, Identifier name, QualifiedName? audit, SqlList<AuditAction> actions, SqlList<Option> options, string place)
		{
			PutOneName(name, place);

			if (audit is { } named)
			{
				Word("FOR SERVER AUDIT");
				PutNameOf(named, place);
			}

			foreach (var action in actions)
				PutAuditAction(action, place);

			PutAuditSettings(owner, options, SpecificationSettings, place);
		}

		void PutAuditAction(AuditAction action, string place)
		{
			if (action.Actions.Count == 0)
				throw Empty(action, "Actions");

			Word(action.Action switch
			{
				AddOrDrop.Add  => "ADD",
				AddOrDrop.Drop => "DROP",
				_              => throw NoText(action.Action),
			});

			Open();
			Each(action.Actions, one => PutActionWords(one, place));

			if (action.On is { } on)
			{
				if (on.Grantees.Count == 0)
					throw Empty(on, "Grantees");

				Word("ON");
				PutClass(on.Class);
				PutNameOf(on.Name, place);
				Word("BY");

				Each(on.Grantees, grantee =>
				{
					switch (grantee)
					{
						case Grantee.Public:
							Word("PUBLIC");
							break;

						case Grantee.Null:
							Word("NULL");
							break;

						case Grantee.Identifier identifier:
							PutOneName(identifier.Value.Name, place);
							break;

						default:
							throw Unwritten(grantee);
					}
				});
			}

			Close();

			if (action.Comma)
				Tight(",");
		}

		/// <summary>CreateServerAudit's settings: the delay, what to do on a failure, the operator audit and the GUID.</summary>
		static readonly Dictionary<string, SettingEntry> CreateAuditSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["QUEUE_DELAY"]    = new(BrokerForm.Digits, Once: false),
			["ON_FAILURE"]     = new(BrokerForm.Words, ["CONTINUE", "SHUTDOWN", "FAIL_OPERATION"], Once: false),
			["OPERATOR_AUDIT"] = new(BrokerForm.OnOff, Once: false),
			["AUDIT_GUID"]     = new(BrokerForm.String, Once: false),
		};

		/// <summary>AuditAlteredOption: the same but the GUID, and the state.</summary>
		static readonly Dictionary<string, SettingEntry> AlterAuditSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["QUEUE_DELAY"]    = new(BrokerForm.Digits, Once: false),
			["ON_FAILURE"]     = new(BrokerForm.Words, ["CONTINUE", "SHUTDOWN", "FAIL_OPERATION"], Once: false),
			["OPERATOR_AUDIT"] = new(BrokerForm.OnOff, Once: false),
			["STATE"]          = new(BrokerForm.OnOff, Once: false),
		};

		/// <summary>AuditFileOption: the path, the sizes and the count of files, and whether the space is reserved, as often as wanted.</summary>
		static readonly Dictionary<string, SettingEntry> AuditFileSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["FILEPATH"]            = new(BrokerForm.String, Once: false),
			["MAXSIZE"]             = new(BrokerForm.Measured | BrokerForm.Exact | BrokerForm.Words, ["UNLIMITED"], ["MB", "GB", "TB"], Once: false),
			["MAX_ROLLOVER_FILES"]  = new(BrokerForm.Digits | BrokerForm.Words, ["UNLIMITED"], Once: false),
			["MAX_FILES"]           = new(BrokerForm.Digits, Once: false),
			["RESERVE_DISK_SPACE"]  = new(BrokerForm.OnOff, Once: false),
		};

		/// <summary>AuditStateWith: the state, as often as wanted.</summary>
		static readonly Dictionary<string, SettingEntry> SpecificationSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["STATE"] = new(BrokerForm.OnOff, Once: false),
		};

		// ── Event sessions ────────────────────────────────────────────────────────

		void PutEventScope(EventScope scope)
		{
			Word("ON");
			Word(scope switch
			{
				EventScope.Server   => "SERVER",
				EventScope.Database => "DATABASE",
				_                   => throw NoText(scope),
			});
		}

		/// <summary>EventSessionSyntax: each setting its own kind of value, as often as wanted.</summary>
		static readonly Dictionary<string, SettingEntry> EventSessionSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["MAX_MEMORY"]             = new(BrokerForm.Measured | BrokerForm.Digits, null, ["KB", "MB"], Once: false),
			["MAX_EVENT_SIZE"]         = new(BrokerForm.Measured | BrokerForm.Digits, null, ["KB", "MB"], Once: false),
			["EVENT_RETENTION_MODE"]   = new(BrokerForm.Words, ["ALLOW_SINGLE_EVENT_LOSS", "ALLOW_MULTIPLE_EVENT_LOSS", "NO_EVENT_LOSS"], Once: false),
			["MAX_DISPATCH_LATENCY"]   = new(BrokerForm.Measured | BrokerForm.Digits | BrokerForm.Words, ["INFINITE"], ["SECONDS"], Once: false),
			["MEMORY_PARTITION_MODE"]  = new(BrokerForm.Words, ["NONE", "PER_NODE", "PER_CPU"], Once: false),
			["TRACK_CAUSALITY"]        = new(BrokerForm.OnOff, Once: false),
			["STARTUP_STATE"]          = new(BrokerForm.OnOff, Once: false),
			["MAX_DURATION"]           = new(BrokerForm.Measured | BrokerForm.Digits | BrokerForm.Words, ["UNLIMITED"], ["SECONDS", "MINUTES", "HOURS", "DAYS"], Once: false),
		};

		/// <summary><c>WITH (setting, …)</c> of a session, or nothing for no setting.</summary>
		void PutEventSessionOptions(ISqlNode owner, SqlList<Option> options)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Open();
			PutBrokerSettings(owner, options, EventSessionSettings, "an event session", required: [], apart: []);
			Close();
		}

		/// <summary>
		/// The pieces of a session made: events first, one at least, and targets after them, and nothing dropped; or the pieces of a
		/// session altered: all of one kind (<c>EventAlterations</c>), the commas between them.
		/// </summary>
		void PutEventPieces(ISqlNode owner, SqlList<EventSessionPiece> pieces, bool created)
		{
			if (pieces.Count == 0)
				throw Empty(owner, "Pieces");

			if (created)
			{
				if (pieces[0] is not EventSessionPiece.AddEvent)
					throw Misplaced(owner, "Pieces", "that do not begin with an event, which a session made is told first");

				var targets = false;

				foreach (var piece in pieces)
				{
					switch (piece)
					{
						case EventSessionPiece.AddEvent when targets:
							throw Misplaced(owner, "Pieces", "with an event after a target, which a session made reads before them");

						case EventSessionPiece.AddTarget:
							targets = true;
							break;

						case EventSessionPiece.AddEvent:
							break;

						default:
							throw Misplaced(piece, "a piece", "that drops, in CREATE EVENT SESSION, which drops nothing");
					}
				}
			}
			else if (pieces.Any(one => one.GetType() != pieces[0].GetType()))
				throw Misplaced(owner, "Pieces", "of more than one kind, which ALTER EVENT SESSION reads one at a time (Msg 156)");

			// A session made writes no comma between its last event and its first target.
			for (var at = 0; at < pieces.Count; at++)
			{
				if (at > 0 && !(created && pieces[at - 1] is EventSessionPiece.AddEvent && pieces[at] is EventSessionPiece.AddTarget))
					Tight(",");

				PutEventPiece(pieces[at]);
			}
		}

		void PutEventPiece(EventSessionPiece piece)
		{
			switch (piece)
			{
				case EventSessionPiece.AddEvent add:
					Word("ADD EVENT");
					PutEventName(add.Event, "an event session");

					if (add.Settings.Count > 0 || add.Actions.Count > 0 || add.Where is not null)
					{
						Open();

						if (add.Settings.Count > 0)
						{
							Word("SET");
							PutEventSettings(add, add.Settings, "an event");
						}

						if (add.Actions.Count > 0)
						{
							Word("ACTION");
							Open();
							Each(add.Actions, one => PutEventName(one, "an event's actions"));
							Close();
						}

						if (add.Where is { } where)
						{
							Word("WHERE");
							PutEventPredicate(where);
						}

						Close();
					}

					break;

				case EventSessionPiece.DropEvent drop:
					Word("DROP EVENT");
					PutEventName(drop.Event, "an event session");
					break;

				case EventSessionPiece.AddTarget add:
					Word("ADD TARGET");
					PutEventName(add.Target, "an event session");

					if (add.Settings.Count > 0)
					{
						Open();
						Word("SET");
						PutEventSettings(add, add.Settings, "a target");
						Close();
					}

					break;

				case EventSessionPiece.DropTarget drop:
					Word("DROP TARGET");
					PutEventName(drop.Target, "an event session");
					break;

				default:
					throw Unwritten(piece);
			}
		}

		void PutAlterEventSession(Statement.AlterEventSession alter)
		{
			var pieces  = alter.Pieces.Count > 0;
			var options = alter.Options.Count > 0;

			if (alter.State is { } state)
			{
				if (pieces || options)
					throw Misplaced(alter, "State", "beside pieces or settings, which the state is said without (Msg 102)");

				Word("ALTER EVENT SESSION");
				PutOneName(alter.Name, "ALTER EVENT SESSION");
				PutEventScope(alter.On);
				Word("STATE =");
				Word(state switch
				{
					EventSessionState.Start => "START",
					EventSessionState.Stop  => "STOP",
					_                       => throw NoText(state),
				});

				return;
			}

			if (!pieces && !options)
				throw Empty(alter, "Pieces", "Options", "State");

			Word("ALTER EVENT SESSION");
			PutOneName(alter.Name, "ALTER EVENT SESSION");
			PutEventScope(alter.On);

			if (pieces)
				PutEventPieces(alter, alter.Pieces, created: false);

			PutEventSessionOptions(alter, alter.Options);
		}

		/// <summary>
		/// A name of an event, a target, an action or a field (<c>EventObject</c>): one part, two or three, each an identifier — never
		/// four, and no part left out.
		/// </summary>
		void PutEventName(QualifiedName name, string place)
		{
			if (name.Count is < 1 or > 3)
				throw Misplaced(name, "Parts", $"{name.Count} in number, in {place}, which names in one part, two or three");

			foreach (var part in name)
				CheckExternalName(part, place);

			PutName(name);
		}

		/// <summary>
		/// <c>SET name = value, …</c>: a name of one part that is neither bracketed nor reserved (<c>EventSetting</c>), set to a value
		/// the grammar's <c>EventValue</c> reads, as often as wanted.
		/// </summary>
		void PutEventSettings(ISqlNode owner, SqlList<Option> settings, string place)
		{
			for (var at = 0; at < settings.Count; at++)
			{
				var setting = settings[at];

				if (setting.Name.Style != IdentifierStyle.Regular || setting.Name.UnicodeEscape is not null || setting.Name.Text.StartsWith("#", StringComparison.Ordinal) || !IsRegularName(setting.Name.Text) || TransactSqlReserved.Contains(setting.Name.Text))
					throw Misplaced(setting, "Name", $"that is no plain name of one part in {place}: not reserved, not bracketed, not temporary");

				if (setting.Words.Count > 0 || setting.EqualsOmitted || setting.Options.Count > 0 || setting.Order is not null || setting.Partitions is not null || setting.Place is not null)
					throw Misplaced(setting, "Words, EqualsOmitted, Options, Order, Partitions or Place", $"in {place}, whose settings are a name and a value");

				if (setting.Value is not { } value)
					throw Empty(setting, "Value");

				if (at > 0)
					Tight(",");

				PutKeyWord(setting.Name);
				Word("=");
				PutEventValue(setting, value, place);
			}
		}

		/// <summary>
		/// The grammar's <c>EventValue</c>: a number with a minus at most (T-SQL's numeric literal, so one with an empty exponent
		/// too), a binary string or a string, in as many brackets as are written; never a name, NULL, a variable or money.
		/// </summary>
		void PutEventValue(ISqlNode owner, Expression value, string place)
		{
			var brackets = 0;

			while (value is Expression.Parenthesized inner)
			{
				Open();
				brackets++;
				value = inner.Value;
			}

			switch (value)
			{
				case LiteralValue.Numeric number when IsNumberOf(number with { Text = number.Text.StartsWith("-", StringComparison.Ordinal) ? number.Text.Substring(1) : number.Text }, ExternalShape.Number | ExternalShape.OpenExponent):
					PutSignedNumber(owner, number);
					break;

				case LiteralValue.Binary { ZeroX: true }:
				case LiteralValue.String string_ when IsQuoted(string_):
					PutExpression(value);
					break;

				default:
					throw Misplaced(owner, "Value", $"other than a number, a binary string or a string, in {place} (EventValue)");
			}

			for (; brackets > 0; brackets--)
				Close();
		}

		// ── Predicates ────────────────────────────────────────────────────────────

		/// <summary>
		/// The grammar's <c>EventPredicate</c>: a factor, and factors joined by AND and OR, all from the left; a factor is a term with NOT
		/// in front of it any number of times, and a term a predicate in brackets, a call or a comparison.
		/// </summary>
		void PutEventPredicate(EventPredicate predicate)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, predicate, static (into, node) => into.PutEventPredicate(node));

				return;
			}

			switch (predicate)
			{
				// The right of a join is a factor: a join there would read back as joined from the left.
				case EventPredicate.Join join:
					PutEventPredicate(join.Left);

					Word(join.Operator switch
					{
						EventJoin.And => "AND",
						EventJoin.Or  => "OR",
						_             => throw NoText(join.Operator),
					});

					PutEventFactor(join, join.Right);
					break;

				// NOT applies to the term after it: a join under it would read back as joined above it.
				case EventPredicate.Not not:
					Word("NOT");
					PutEventFactor(not, not.Operand);
					break;

				case EventPredicate.Parenthesized brackets:
					Open();
					PutEventPredicate(brackets.Predicate);
					Close();
					break;

				case EventPredicate.Call call:
					if (call.Name.Count is < 2 or > 3)
						throw Misplaced(call, "Name", "that is not a package and a name, two parts or three (EventCall)");

					PutEventName(call.Name, "an event predicate");
					Call();
					PutEventName(call.Field, "an event predicate");
					Tight(",");
					PutEventValue(call, call.Value, "an event predicate");
					Close();
					break;

				case EventPredicate.Compare compare:
					if (compare.Exclamation && compare.Operator != ComparisonOperator.NotEqual)
						throw Misplaced(compare, "Exclamation", "on a comparison that is not an inequality");

					PutEventName(compare.Field, "an event predicate");
					Word(compare.Exclamation ? "!=" : Comparison(compare.Operator));
					PutEventValue(compare, compare.Value, "an event predicate");
					break;

				case EventPredicate.Like like:
					PutEventName(like.Field, "an event predicate");
					Word(like.Negated ? "NOT LIKE" : "LIKE");
					PutEventValue(like, like.Value, "an event predicate");
					break;

				default:
					throw Unwritten(predicate);
			}
		}

		void PutEventFactor(EventPredicate owner, EventPredicate operand)
		{
			if (operand is EventPredicate.Join)
				throw Misplaced(owner, "an operand", "that is a join with no brackets, which the grammar reads joined from the left (EventFactor)");

			PutEventPredicate(operand);
		}
	}
}
