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
		/// T-SQL's Broker family (Service Broker's objects and conversation statements): writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutBroker(ISqlNode node)
		{
			switch (node)
			{
				case Statement.CreateMessageType create:
					Word("CREATE MESSAGE TYPE");
					PutOneName(create.Name, "CREATE MESSAGE TYPE");
					PutAuthorization(create.Authorization, "CREATE MESSAGE TYPE");

					if (create.Validation is { } validation)
						PutValidation(validation);

					return true;

				case Statement.AlterMessageType alter:
					Word("ALTER MESSAGE TYPE");
					PutOneName(alter.Name, "ALTER MESSAGE TYPE");
					PutValidation(alter.Validation);
					return true;

				case Statement.CreateContract create:
					Word("CREATE CONTRACT");
					PutOneName(create.Name, "CREATE CONTRACT");
					PutAuthorization(create.Authorization, "CREATE CONTRACT");

					if (create.Messages.Count == 0)
						throw Empty(create, "Messages");

					Open();

					for (var at = 0; at < create.Messages.Count; at++)
					{
						if (at > 0)
							Tight(",");

						PutOneName(create.Messages[at].MessageType, "CREATE CONTRACT");
						Word("SENT BY");
						Word(create.Messages[at].SentBy switch
						{
							ContractSender.Initiator => "INITIATOR",
							ContractSender.Target    => "TARGET",
							ContractSender.Any       => "ANY",
							_                        => throw NoText(create.Messages[at].SentBy),
						});
					}

					Close();
					return true;

				case Statement.CreateQueue create:
					Word("CREATE QUEUE");
					PutNameOf(create.Name, "CREATE QUEUE");

					if (create.Options.Count > 0)
					{
						Word("WITH");
						PutQueueOptions(create, create.Options, created: true);
					}

					if (create.On is { } on)
						PutQueueGroup(on, PlacementKind.On, quoted: true);

					return true;

				case Statement.AlterQueue alter:
					Word("ALTER QUEUE");
					PutNameOf(alter.Name, "ALTER QUEUE");
					PutQueueChange(alter.Change);
					return true;

				case Statement.CreateService create:
					Word("CREATE SERVICE");
					PutOneName(create.Name, "CREATE SERVICE");
					PutAuthorization(create.Authorization, "CREATE SERVICE");
					Word("ON QUEUE");
					PutNameOf(create.Queue, "CREATE SERVICE");

					// The brackets are written when there are contracts.
					if (create.Contracts.Count > 0)
					{
						Open();
						Each(create.Contracts, one => PutOneName(one, "CREATE SERVICE"));
						Close();
					}

					return true;

				case Statement.AlterService alter:
					if (alter.Queue is null && alter.Changes.Count == 0)
						throw Empty(alter, "Queue", "Changes");

					Word("ALTER SERVICE");
					PutOneName(alter.Name, "ALTER SERVICE");

					if (alter.Queue is { } queue)
					{
						Word("ON QUEUE");
						PutNameOf(queue, "ALTER SERVICE");
					}

					if (alter.Changes.Count > 0)
					{
						Open();

						Each(alter.Changes, change =>
						{
							Word(change.Action switch
							{
								AddOrDrop.Add  => "ADD CONTRACT",
								AddOrDrop.Drop => "DROP CONTRACT",
								_              => throw NoText(change.Action),
							});

							PutOneName(change.Contract, "ALTER SERVICE");
						});

						Close();
					}

					return true;

				case Statement.CreateRoute create:
					Word("CREATE ROUTE");
					PutOneName(create.Name, "CREATE ROUTE");
					PutAuthorization(create.Authorization, "CREATE ROUTE");
					Word("WITH");
					PutBrokerSettings(create, create.Options, RouteSettings, "CREATE ROUTE", required: ["ADDRESS"], apart: []);
					return true;

				case Statement.AlterRoute alter:
					Word("ALTER ROUTE");
					PutOneName(alter.Name, "ALTER ROUTE");
					Word("WITH");
					PutBrokerSettings(alter, alter.Options, RouteSettings, "ALTER ROUTE", required: [], apart: []);
					return true;

				case Statement.CreateRemoteServiceBinding create:
					Word("CREATE REMOTE SERVICE BINDING");
					PutOneName(create.Name, "CREATE REMOTE SERVICE BINDING");
					PutAuthorization(create.Authorization, "CREATE REMOTE SERVICE BINDING");
					Word("TO SERVICE");

					if (!IsQuoted(create.Service))
						throw Misplaced(create, "Service", "that is no string the grammar's BrokerString reads");

					PutExpression(create.Service);
					Word("WITH");
					PutBrokerSettings(create, create.Options, BindingSettings, "CREATE REMOTE SERVICE BINDING", required: ["USER"], apart: []);
					return true;

				case Statement.AlterRemoteServiceBinding alter:
					Word("ALTER REMOTE SERVICE BINDING");
					PutOneName(alter.Name, "ALTER REMOTE SERVICE BINDING");
					Word("WITH");
					PutBrokerSettings(alter, alter.Options, BindingSettings, "ALTER REMOTE SERVICE BINDING", required: [], apart: []);
					return true;

				case Statement.CreateBrokerPriority create:
					Word("CREATE BROKER PRIORITY");
					PutOneName(create.Name, "CREATE BROKER PRIORITY");
					Word("FOR CONVERSATION");

					if (create.Options.Count > 0)
					{
						Word("SET");
						Open();
						PutBrokerSettings(create, create.Options, PrioritySettings, "CREATE BROKER PRIORITY", required: [], apart: []);
						Close();
					}

					return true;

				case Statement.AlterBrokerPriority alter:
					Word("ALTER BROKER PRIORITY");
					PutOneName(alter.Name, "ALTER BROKER PRIORITY");
					Word("FOR CONVERSATION SET");
					Open();
					PutBrokerSettings(alter, alter.Options, PrioritySettings, "ALTER BROKER PRIORITY", required: [], apart: []);
					Close();
					return true;

				case Statement.BeginDialog dialog:
					PutBeginDialog(dialog);
					return true;

				case Statement.BeginConversationTimer timer:
					Word("BEGIN CONVERSATION TIMER");
					Open();
					PutValue(timer.Handle, "BEGIN CONVERSATION TIMER");
					Close();
					Word("TIMEOUT =");
					PutValue(timer.Timeout, "BEGIN CONVERSATION TIMER");
					return true;

				case Statement.EndConversation end:
					PutEndConversation(end);
					return true;

				case Statement.MoveConversation move:
					Word("MOVE CONVERSATION");
					PutValue(move.Handle, "MOVE CONVERSATION");
					Word("TO");
					PutValue(move.Group, "MOVE CONVERSATION");
					return true;

				case Statement.Send send:
					PutSend(send);
					return true;

				case Statement.Receive receive:
					PutReceive(receive);
					return true;

				case Statement.GetConversationGroup get:
					Word("GET CONVERSATION GROUP");
					PutVariableName(get.Group, "GET CONVERSATION GROUP");
					Word("FROM");
					PutNameOf(get.Queue, "GET CONVERSATION GROUP");
					return true;

				case Statement.WaitForReceive wait:
					Word("WAITFOR");
					Open();
					PutStatement(wait.Receive);
					Close();
					PutWaitTimeout(wait, wait.Timeout);
					return true;

				case Statement.WaitForGetConversationGroup wait:
					Word("WAITFOR");
					Open();
					PutStatement(wait.Get);
					Close();
					PutWaitTimeout(wait, wait.Timeout);
					return true;

				default:
					return false;
			}
		}

		// ── Names and values the Broker, Monitoring and Server families share ─────

		/// <summary>
		/// A name of one part, as the grammar's <c>Identifier</c> reads it: <see cref="CheckExternalName"/>, and the reserved
		/// words <see cref="PutIdentifier"/> refuses.
		/// </summary>
		void PutOneName(Identifier name, string place)
		{
			CheckExternalName(name, place);
			PutIdentifier(name);
		}

		/// <summary><c>AUTHORIZATION owner</c>, or nothing where there is no owner.</summary>
		void PutAuthorization(Identifier? owner, string place)
		{
			if (owner is null)
				return;

			Word("AUTHORIZATION");
			PutOneName(owner, place);
		}

		/// <summary>
		/// A name as the grammar's <c>QualifiedName</c> reads it: parts of the kinds <see cref="CheckExternalName"/> reads, a part
		/// between two dots left out (<c>db..t</c>) and the last always named.
		/// </summary>
		void PutNameOf(QualifiedName name, string place)
		{
			for (var at = 0; at < name.Count; at++)
			{
				var part = name[at];

				if (part.Text.Length == 0 && part.Style == IdentifierStyle.Regular && at < name.Count - 1)
					continue;

				CheckExternalName(part, place);
			}

			PutName(name);
		}

		/// <summary>
		/// A variable the grammar's <c>TSql.Variable</c> reads, kept in an identifier with its <c>@</c>: an <c>@</c> and any run of
		/// the characters a name goes on with, and no blank.
		/// </summary>
		void PutVariableName(Identifier variable, string place)
		{
			CheckVariable(variable, place);
			Word(variable.Text);
		}

		static void CheckVariable(Identifier variable, string place)
		{
			if (variable.Style != IdentifierStyle.Regular || variable.UnicodeEscape is not null || !Regex.IsMatch(variable.Text, @"^@[\p{L}\p{Nd}_@$#]*$"))
				throw Misplaced(variable, "Text", $"that is no variable in {place}: an @ and the characters a name goes on with");
		}

		/// <summary>
		/// A value where the grammar reads <c>ValueExpression</c>: checked down to what the shared writer leaves to its caller
		/// (<see cref="CheckExternalValue"/>), and then written.
		/// </summary>
		void PutValue(Expression value, string place)
		{
			CheckExternalValue(value, place);
			PutExpression(value);
		}

		/// <summary>
		/// A string or a variable and nothing else (<c>BrokerTarget</c>): a string as the grammar's <c>Quoted</c> reads it, or a
		/// variable of the kind <see cref="CheckVariable"/> reads.
		/// </summary>
		void PutStringOrVariable(ISqlNode owner, string member, Expression value, string place)
		{
			switch (value)
			{
				case LiteralValue.String text when IsQuoted(text):
					PutExpression(text);
					return;

				case Expression.Variable variable:
					CheckVariable(variable.Name, place);
					PutExpression(variable);
					return;
			}

			throw Misplaced(owner, member, $"other than a string or a variable, in {place}");
		}

		/// <summary>An unsigned number the grammar's <c>Number</c> reads: digits, a point, an exponent.</summary>
		void PutUnsignedNumber(ISqlNode owner, string member, LiteralValue.Numeric number, string place)
		{
			if (!IsNumberOf(number, ExternalShape.Number))
				throw Misplaced(owner, member, $"that is no unsigned number in {place}");

			PutExpression(number);
		}

		void PutOnOff(bool on)
		{
			Word(on ? "ON" : "OFF");
		}

		// ── Message types and queues ──────────────────────────────────────────────

		void PutValidation(MessageValidation validation)
		{
			Word("VALIDATION =");

			switch (validation)
			{
				// Any word the engine does not check until it runs, but VALID_XML, which is the other alternative's.
				case MessageValidation.Named named:
					if (Plain(named.Word, "VALID_XML"))
						throw Misplaced(named, "Word", "that is VALID_XML written plain, which reads as VALID_XML WITH SCHEMA COLLECTION");

					PutOneName(named.Word, "a message type's validation");
					break;

				case MessageValidation.ValidXml xml:
					Word("VALID_XML WITH SCHEMA COLLECTION");
					PutNameOf(xml.SchemaCollection, "a message type's validation");
					break;

				default:
					throw Unwritten(validation);
			}
		}

		/// <summary>
		/// The options of a queue, <c>option, …</c>: each kind once, a queue made told its activation in full, a queue altered
		/// any part of it or its removal.
		/// </summary>
		void PutQueueOptions(ISqlNode owner, SqlList<QueueOption> options, bool created)
		{
			var seen = new HashSet<string>(StringComparer.Ordinal);

			for (var at = 0; at < options.Count; at++)
			{
				if (at > 0)
					Tight(",");

				var option = options[at];
				var kind   = option switch
				{
					QueueOption.Status                => "STATUS",
					QueueOption.Retention             => "RETENTION",
					QueueOption.PoisonMessageHandling => "POISON_MESSAGE_HANDLING",
					QueueOption.DropActivation        => "ACTIVATION",
					QueueOption.Activation            => "ACTIVATION",
					_                                 => throw Unwritten(option),
				};

				if (!seen.Add(kind))
					throw Misplaced(owner, "Options", $"that name {kind} twice, which the grammar reads once (NamedOnce)");

				switch (option)
				{
					case QueueOption.Status status:
						Word("STATUS =");
						PutOnOff(status.On);
						break;

					case QueueOption.Retention retention:
						Word("RETENTION =");
						PutOnOff(retention.On);
						break;

					case QueueOption.PoisonMessageHandling poison:
						Word("POISON_MESSAGE_HANDLING");
						Open();
						Word("STATUS =");
						PutOnOff(poison.On);
						Close();
						break;

					case QueueOption.DropActivation when created:
						throw Misplaced(option, "a dropped activation", "in CREATE QUEUE, which wants the procedure, the readers and whom they run as (Activates)");

					case QueueOption.DropActivation:
						Word("ACTIVATION");
						Open();
						Word("DROP");
						Close();
						break;

					case QueueOption.Activation activation:
						PutActivation(activation, created);
						break;
				}
			}
		}

		void PutActivation(QueueOption.Activation activation, bool created)
		{
			if (activation.Settings.Count == 0)
				throw Empty(activation, "Settings");

			var seen = new HashSet<Type>();

			foreach (var setting in activation.Settings)
				if (!seen.Add(setting.GetType()))
					throw Misplaced(activation, "Settings", "that name one setting twice, which the grammar reads once (NamedOnce)");

			if (created && !(seen.Contains(typeof(ActivationSetting.ProcedureName)) && seen.Contains(typeof(ActivationSetting.MaxQueueReaders)) && seen.Contains(typeof(ActivationSetting.ExecuteAs))))
				throw Misplaced(activation, "Settings", "in CREATE QUEUE without the procedure, the readers and whom they run as (Activates)");

			Word("ACTIVATION");
			Open();

			for (var at = 0; at < activation.Settings.Count; at++)
			{
				if (at > 0)
					Tight(",");

				switch (activation.Settings[at])
				{
					case ActivationSetting.Status status:
						Word("STATUS =");
						PutOnOff(status.On);
						break;

					case ActivationSetting.ProcedureName procedure:
						Word("PROCEDURE_NAME =");
						PutNameOf(procedure.Name, "an activation");
						break;

					case ActivationSetting.MaxQueueReaders readers:
						Word("MAX_QUEUE_READERS =");
						PutUnsignedNumber(readers, "Readers", readers.Readers, "an activation");
						break;

					// SELF or OWNER, or a user in single quotes: the grammar reads no other, and no national string.
					case ActivationSetting.ExecuteAs { Context: { Kind: ExecutionContextKind.Self, Name: null } }:
						Word("EXECUTE AS SELF");
						break;

					case ActivationSetting.ExecuteAs { Context: { Kind: ExecutionContextKind.Owner, Name: null } }:
						Word("EXECUTE AS OWNER");
						break;

					case ActivationSetting.ExecuteAs { Context: { Kind: ExecutionContextKind.User, Name: LiteralValue.String { Kind: StringLiteralKind.Character } user } } when IsQuoted(user):
						Word("EXECUTE AS");
						PutExpression(user);
						break;

					case ActivationSetting.ExecuteAs execute:
						throw Misplaced(execute, "Context", "other than SELF, OWNER or a user in single quotes");

					default:
						throw Unwritten(activation.Settings[at]);
				}
			}

			Close();
		}

		/// <summary>
		/// <c>ON filegroup</c> or <c>MOVE TO filegroup</c>: a placement of the kind the place writes, with no columns and a name of
		/// one part; a queue made may name it by a string too.
		/// </summary>
		void PutQueueGroup(Placement placement, PlacementKind kind, bool quoted)
		{
			if (placement.Kind != kind)
				throw Misplaced(placement, "Kind", $"other than {kind}, where only {kind} is written");

			if (placement.Columns is not null)
				throw Misplaced(placement, "Columns", "on a queue's filegroup, which is a name and no partition scheme");

			Word(kind == PlacementKind.On ? "ON" : "MOVE TO");

			switch (placement.Target.Style)
			{
				case IdentifierStyle.String when quoted:
					Word("'" + placement.Target.Text.Replace("'", "''") + "'");
					break;

				case IdentifierStyle.NationalString when quoted:
					Word("N'" + placement.Target.Text.Replace("'", "''") + "'");
					break;

				default:
					PutOneName(placement.Target, "a queue's filegroup");
					break;
			}
		}

		void PutQueueChange(QueueChange change)
		{
			switch (change)
			{
				case QueueChange.With with:
					if (with.Options.Count == 0)
						throw Empty(with, "Options");

					Word("WITH");
					PutQueueOptions(with, with.Options, created: false);
					break;

				case QueueChange.Rebuild rebuild:
					Word("REBUILD");

					if (rebuild.MaxDop is { } maxdop)
					{
						Word("WITH");
						Open();
						Word("MAXDOP =");
						PutUnsignedNumber(rebuild, "MaxDop", maxdop, "ALTER QUEUE");
						Close();
					}

					break;

				case QueueChange.Reorganize reorganize:
					Word("REORGANIZE");

					if (reorganize.LobCompaction is { } compaction)
					{
						Word("WITH");
						Open();
						Word("LOB_COMPACTION =");
						PutOnOff(compaction);
						Close();
					}

					break;

				case QueueChange.MoveTo move:
					PutQueueGroup(move.Target, PlacementKind.MoveTo, quoted: false);
					break;

				default:
					throw Unwritten(change);
			}
		}

		// ── Settings of a route, a binding, a priority and a dialog ───────────────

		/// <summary>
		/// The forms of value a setting is read with: a string (<c>Quoted</c>), a name, an unsigned number, ON or OFF, the word ANY, the
		/// word DEFAULT, any value, digits alone (<c>Sql92.Lexical.Digits</c>), an exact number with no exponent (<c>ExactNumber</c>),
		/// a word out of the entry's list, a number and the unit it is written in, NULL, and the ranges of an affinity in brackets.
		/// </summary>
		[Flags]
		enum BrokerForm
		{
			String   = 1,
			Name     = 2,
			Number   = 4,
			OnOff    = 8,
			Any      = 16,
			Default  = 32,
			Value    = 64,
			Digits   = 128,
			Exact    = 256,
			Words    = 512,
			Measured = 1024,
			Null     = 2048,
			Ranges   = 4096,
		}

		/// <summary>
		/// One setting a place reads: the forms of its value, the words and the units where its value is one of a closed list, and
		/// whether the place lets it stand once only. A setting of the forms alone is read once.
		/// </summary>
		readonly record struct SettingEntry(BrokerForm Forms, string[]? Words = null, string[]? Units = null, bool Once = true)
		{
			public static implicit operator SettingEntry(BrokerForm forms)
			{
				return new SettingEntry(forms);
			}
		}

		/// <summary>RouteOption: strings but the lifetime, which is a number.</summary>
		static readonly Dictionary<string, SettingEntry> RouteSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["SERVICE_NAME"]    = BrokerForm.String,
			["BROKER_INSTANCE"] = BrokerForm.String,
			["MIRROR_ADDRESS"]  = BrokerForm.String,
			["ADDRESS"]         = BrokerForm.String,
			["LIFETIME"]        = BrokerForm.Number,
		};

		/// <summary>BindingOption: the user as a name and the switch for anonymous.</summary>
		static readonly Dictionary<string, SettingEntry> BindingSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["USER"]      = BrokerForm.Name,
			["ANONYMOUS"] = BrokerForm.OnOff,
		};

		/// <summary>PriorityOption: names, a string and a level, each of them or its default word.</summary>
		static readonly Dictionary<string, SettingEntry> PrioritySettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["CONTRACT_NAME"]       = BrokerForm.Name | BrokerForm.Any,
			["LOCAL_SERVICE_NAME"]  = BrokerForm.Name | BrokerForm.Any,
			["REMOTE_SERVICE_NAME"] = BrokerForm.String | BrokerForm.Any,
			["PRIORITY_LEVEL"]      = BrokerForm.Number | BrokerForm.Default,
		};

		/// <summary>DialogOption: the related conversation and the lifetime are any value, and the encryption a switch.</summary>
		static readonly Dictionary<string, SettingEntry> DialogSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["RELATED_CONVERSATION_GROUP"] = BrokerForm.Value,
			["RELATED_CONVERSATION"]       = BrokerForm.Value,
			["LIFETIME"]                   = BrokerForm.Value,
			["ENCRYPTION"]                 = BrokerForm.OnOff,
		};

		/// <summary>
		/// A list of settings joined by commas, one at least, each a name the catalogue reads and each once; <paramref name="required"/>
		/// are among them, and at most one of <paramref name="apart"/>.
		/// </summary>
		void PutBrokerSettings(ISqlNode owner, SqlList<Option> options, Dictionary<string, SettingEntry> catalogue, string place, string[] required, string[] apart)
		{
			if (options.Count == 0)
				throw Empty(owner, "Options");

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			for (var at = 0; at < options.Count; at++)
			{
				var option = options[at];

				// A name of several words is looked up as the words joined: `AFFINITY SCHEDULER`.
				var key = string.Join(" ", option.Words.Select(static one => one.Text).Prepend(option.Name.Text));

				if (option.Name.Style != IdentifierStyle.Regular || option.Words.Any(static one => one.Style != IdentifierStyle.Regular || one.UnicodeEscape is not null) || !catalogue.TryGetValue(key, out var entry))
					throw Misplaced(option, "Name", $"in {place}, which reads no setting {key}");

				if (!seen.Add(key) && entry.Once)
					throw Misplaced(option, "Name", $"twice in {place}, which reads {key} once (NamedOnce)");

				if (option.EqualsOmitted)
					throw Misplaced(option, "EqualsOmitted", $"in {place}, whose settings are written with =");

				if (option.Order is not null || option.Partitions is not null || option.Place is not null)
					throw Misplaced(option, "an order, partitions or a place", $"in {place}, whose settings are a name and a value");

				if (option.Options.Count > 0)
					throw Misplaced(option, "Options", $"after {option.Name.Text} in {place}, which reads none");

				if (option.Value is not { } value)
					throw Empty(option, "Value");

				if (at > 0)
					Tight(",");

				PutKeyWord(option.Name);

				foreach (var word in option.Words)
					PutKeyWord(word);

				Word("=");
				PutBrokerValue(option, value, entry, place);
			}

			foreach (var name in required)
				if (!seen.Contains(name))
					throw Misplaced(owner, "Options", $"without {name}, which {place} wants");

			if (apart.Count(seen.Contains) > 1)
				throw Misplaced(owner, "Options", $"that name {string.Join(" and ", apart)} together, of which {place} reads one");
		}

		void PutBrokerValue(Option option, Expression value, SettingEntry entry, string place)
		{
			var forms = entry.Forms;

			if (value is Expression.Reference { Name: [{ } word] })
			{
				if (forms.HasFlag(BrokerForm.OnOff) && Plain(word, "ON", "OFF") || forms.HasFlag(BrokerForm.Any) && Plain(word, "ANY") || forms.HasFlag(BrokerForm.Default) && Plain(word, "DEFAULT")
					|| forms.HasFlag(BrokerForm.Words) && entry.Words is { } words && Plain(word, words))
				{
					PutKeyWord(word);
					return;
				}

				if (forms.HasFlag(BrokerForm.Name))
				{
					PutOneName(word, place);
					return;
				}
			}

			if (value is Expression.PoolRanges ranges && forms.HasFlag(BrokerForm.Ranges))
			{
				PutPoolRanges(ranges);
				return;
			}

			if (value is LiteralValue.Null && forms.HasFlag(BrokerForm.Null))
			{
				PutExpression(value);
				return;
			}

			if (value is LiteralValue.String text && forms.HasFlag(BrokerForm.String) && IsQuoted(text))
			{
				PutExpression(text);
				return;
			}

			// A setting of a number and its unit has no bare number.
			if (value is LiteralValue.Numeric number && !forms.HasFlag(BrokerForm.Measured) && IsNumeric(number, forms))
			{
				PutExpression(number);
				return;
			}

			if (value is Expression.Measured { Value: LiteralValue.Numeric amount, Unit: { } unit } && forms.HasFlag(BrokerForm.Measured) && entry.Units is { } units && Plain(unit, units) && IsNumeric(amount, forms))
			{
				PutExpression(amount);
				PutKeyWord(unit);
				return;
			}

			if (forms.HasFlag(BrokerForm.Value))
			{
				PutValue(value, place);
				return;
			}

			throw Misplaced(option, "Value", $"of a form {option.Name.Text} is not read with in {place}");
		}

		/// <summary>Whether a number is one the forms read: digits alone, an exact number with no exponent, or any unsigned number.</summary>
		static bool IsNumeric(LiteralValue.Numeric number, BrokerForm forms)
		{
			var shape = (forms.HasFlag(BrokerForm.Number) ? ExternalShape.Number : 0) | (forms.HasFlag(BrokerForm.Exact) ? ExternalShape.Exact : 0) | (forms.HasFlag(BrokerForm.Digits) ? ExternalShape.Digits : 0);

			return shape != 0 && IsNumberOf(number, shape);
		}

		// ── Conversations ─────────────────────────────────────────────────────────

		void PutBeginDialog(Statement.BeginDialog dialog)
		{
			Word("BEGIN DIALOG");

			if (dialog.Conversation)
				Word("CONVERSATION");

			PutVariableName(dialog.Handle, "BEGIN DIALOG");
			Word("FROM SERVICE");
			PutNameOf(dialog.From, "BEGIN DIALOG");
			Word("TO SERVICE");
			PutStringOrVariable(dialog, "To", dialog.To, "BEGIN DIALOG");

			if (dialog.Instance is { } instance)
			{
				Tight(",");
				PutStringOrVariable(dialog, "Instance", instance, "BEGIN DIALOG");
			}

			if (dialog.Contract is { } contract)
			{
				Word("ON CONTRACT");
				PutNameOf(contract, "BEGIN DIALOG");
			}

			if (dialog.Options.Count > 0)
			{
				Word("WITH");
				PutBrokerSettings(dialog, dialog.Options, DialogSettings, "BEGIN DIALOG", required: [], apart: ["RELATED_CONVERSATION", "RELATED_CONVERSATION_GROUP"]);
			}
		}

		void PutEndConversation(Statement.EndConversation end)
		{
			if (end.Cleanup && (end.Error is not null || end.Description is not null))
				throw Both(end, "Cleanup", "Error");

			if ((end.Error is null) != (end.Description is null))
				throw Misplaced(end, end.Error is null ? "Description" : "Error", "without the other, which are said together");

			Word("END CONVERSATION");
			PutValue(end.Handle, "END CONVERSATION");

			if (end.Cleanup)
			{
				Word("WITH CLEANUP");
			}
			else if (end.Error is { } error && end.Description is { } description)
			{
				Word("WITH ERROR =");

				// ConversationError: an unsigned number or a variable.
				switch (error)
				{
					case LiteralValue.Numeric number:
						PutUnsignedNumber(end, "Error", number, "END CONVERSATION");
						break;

					case Expression.Variable variable:
						CheckVariable(variable.Name, "END CONVERSATION");
						PutExpression(variable);
						break;

					default:
						throw Misplaced(end, "Error", "other than a number or a variable, in END CONVERSATION");
				}

				Word("DESCRIPTION =");
				PutStringOrVariable(end, "Description", description, "END CONVERSATION");
			}
		}

		void PutSend(Statement.Send send)
		{
			if (send.Conversations.Count == 0)
				throw Empty(send, "Conversations");

			if (!send.Bracketed && send.Conversations.Count > 1)
				throw Misplaced(send, "Bracketed", "false with several conversations, which are always written in brackets");

			Word("SEND ON CONVERSATION");

			if (send.Bracketed)
			{
				Open();
				Each(send.Conversations, one => PutValue(one, "SEND"));
				Close();
			}
			else
			{
				var one = send.Conversations[0];

				// Bare, the handle is read as a value: a name or a call would take the message after it for its arguments, and a
				// handle that begins with a bracket is read as the list of handles.
				if (send.MessageType is null && send.Body is not null && one is not (Expression.Variable or LiteralValue))
					throw Misplaced(send, "Conversations", "that is no variable or literal, before a message with no MESSAGE TYPE, which the grammar reads as the handle's arguments");

				PutValue(one, "SEND");

				if (Sql2023Writer.Write(one, transactSql: true).StartsWith('('))
					throw Misplaced(send, "Conversations", "that begins with a bracket and is not bracketed, which the grammar reads as the list of handles");
			}

			if (send.MessageType is { } type)
			{
				Word("MESSAGE TYPE");

				switch (type)
				{
					case Expression.Reference { Name: var name } when name.Count > 0:
						PutNameOf(name, "SEND");
						break;

					// MessageTypeName: a local variable, never one of the server's.
					case Expression.Variable variable when !Syntax.IsServerVariable(variable.Name.Text):
						CheckVariable(variable.Name, "SEND");
						PutExpression(variable);
						break;

					default:
						throw Misplaced(send, "MessageType", "other than a name or a local variable, in SEND");
				}
			}

			if (send.Body is { } body)
			{
				Open();
				PutValue(body, "SEND");
				Close();
			}
		}

		void PutReceive(Statement.Receive receive)
		{
			if (receive.Columns.Count == 0)
				throw Empty(receive, "Columns");

			var assigns   = receive.Columns.Any(static one => one is SelectColumn.VariableAssignment);
			var retrieves = receive.Columns.Any(static one => one is not SelectColumn.VariableAssignment);

			if (assigns && retrieves)
				throw Both(receive, "variable assignments", "retrieved columns");

			Word("RECEIVE");

			if (receive.Top is { } top)
			{
				if (!top.Parentheses || top.Percent || top.With is not null)
					throw Misplaced(top, "TOP", "other than TOP (n), which RECEIVE reads");

				Word("TOP");
				Open();
				PutValue(top.Value, "RECEIVE");
				Close();
			}

			for (var at = 0; at < receive.Columns.Count; at++)
			{
				if (at > 0)
					Tight(",");

				switch (receive.Columns[at])
				{
					case SelectColumn.ExpressionItem { AliasFirst: true } item:
						throw Misplaced(item, "AliasFirst", "in RECEIVE, whose columns are not written alias first");

					case SelectColumn.QualifiedAll all:
						throw Misplaced(all, "a qualified star", "in RECEIVE, which reads the star alone");

					// The select list's values are the grammar's ValueExpression, and a variable it assigns is a TSql.Variable.
					case SelectColumn.ExpressionItem item:
						if (item.Alias is null && item.AsKeyword)
							throw Misplaced(item, "AsKeyword", "with no alias, which has no AS to follow");

						// A name as an alias is the grammar's Identifier; a string is written as the string it is.
						if (item.Alias is { Style: IdentifierStyle.Regular or IdentifierStyle.Bracketed or IdentifierStyle.Delimited } alias)
							CheckExternalName(alias, "RECEIVE");

						CheckExternalValue(item.Expression, "RECEIVE");
						PutColumn(item);
						break;

					case SelectColumn.VariableAssignment assignment:
						CheckVariable(assignment.Variable, "RECEIVE");
						CheckExternalValue(assignment.Value, "RECEIVE");
						PutColumn(assignment);
						break;

					default:
						PutColumn(receive.Columns[at]);
						break;
				}
			}

			Word("FROM");
			PutNameOf(receive.Queue, "RECEIVE");

			if (receive.Into is { } into)
			{
				Word("INTO");
				PutVariableName(into, "RECEIVE");
			}

			if (receive.Where is { } where)
			{
				// ReceiveWhere: one of two columns, written bare, equal to a string or a variable.
				if (where is not Expression.Comparison { Left: Expression.Reference { Name: [{ } column] }, Operator: ComparisonOperator.Equal, Exclamation: false } comparison || !Plain(column, "CONVERSATION_HANDLE", "CONVERSATION_GROUP_ID"))
					throw Misplaced(receive, "Where", "other than conversation_handle = value or conversation_group_id = value, which RECEIVE reads");

				Word("WHERE");
				PutKeyWord(column);
				Word("=");
				PutStringOrVariable(receive, "Where", comparison.Right, "RECEIVE");
			}
		}

		/// <summary><c>, TIMEOUT n</c> after a WAITFOR's brackets: a number, a minus allowed, or a variable.</summary>
		void PutWaitTimeout(ISqlNode owner, Expression? timeout)
		{
			if (timeout is null)
				return;

			Tight(",");
			Word("TIMEOUT");

			switch (timeout)
			{
				// WaitTimeout: '-'? & UnsignedNumericLiteral, one token to the tree.
				case LiteralValue.Numeric number:
					var digits = number.Text.StartsWith('-') ? number.Text.Substring(1) : number.Text;

					if (!IsNumberOf(new LiteralValue.Numeric(digits, number.Kind), ExternalShape.Number))
						throw Misplaced(owner, "Timeout", "that is no number WAITFOR reads");

					PutSignedNumber(owner, number);
					break;

				case Expression.Variable variable:
					CheckVariable(variable.Name, "WAITFOR");
					PutExpression(variable);
					break;

				default:
					throw Misplaced(owner, "Timeout", "other than a number or a variable, in WAITFOR");
			}
		}
	}
}
