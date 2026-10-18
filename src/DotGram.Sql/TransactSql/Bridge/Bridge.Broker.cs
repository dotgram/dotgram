using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Bridge
{
	/// <summary>
	/// Service Broker: message types, contracts, queues, services, routes, remote service bindings, broker priorities, and the dialog and conversation statements (BEGIN DIALOG, SEND, RECEIVE, WAITFOR).
	/// </summary>
	/// <remarks>
	/// The old tree keeps what follows the name of an object as text (<c>Tail</c>), which the arms read token by token as the
	/// grammar's rules read it; a text the arm does not read to its end is a gap. The conversation statements are nodes, carried
	/// as they stand.
	/// </remarks>
	internal static class Broker
	{
		public static readonly string[] Owns =
		[
			"Statement.MessageTypeDefinition", "Statement.ContractDefinition", "Statement.QueueDefinition",
			"Statement.ServiceDefinition", "Statement.RouteDefinition", "Statement.RemoteServiceBindingDefinition",
			"Statement.BrokerPriorityDefinition", "Statement.BeginDialog", "Statement.ConversationTimer",
			"Statement.EndConversation", "Statement.MoveConversation", "Statement.Send",
			"Statement.Receive", "Statement.GetConversationGroup", "Statement.WaitForStatement",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.MessageTypeDefinition message       => MessageType(message),
				Statement.ContractDefinition contract         => Contract(contract),
				Statement.QueueDefinition queue               => Queue(queue),
				Statement.ServiceDefinition service           => Service(service),
				Statement.RouteDefinition route               => Route(route),
				Statement.RemoteServiceBindingDefinition bind => Binding(bind),
				Statement.BrokerPriorityDefinition priority   => Priority(priority),
				Statement.BeginDialog dialog                  => Dialog(dialog),
				Statement.ConversationTimer timer             => new Ast.Statement.BeginConversationTimer(Values.Of(timer.Handle), Values.Of(timer.Timeout)),
				Statement.EndConversation end                 => End(end),
				Statement.MoveConversation move               => new Ast.Statement.MoveConversation(Values.Of(move.Handle), Values.Of(move.Group)),
				Statement.Send send                           => Send(send),
				Statement.Receive receive                     => Receive(receive),
				Statement.GetConversationGroup get            => new Ast.Statement.GetConversationGroup(Variable(get, get.Group), Values.Name(get, get.Queue)),
				Statement.WaitForStatement wait               => Wait(wait),
				_ when Owned(Owns, old)                       => throw Unconverted(old),
				_                                             => null,
			};

			return made is not null;
		}

		/// <summary>The verb a definition opened with: CREATE or ALTER, whatever the case it was written in.</summary>
		internal static string Verb(Statement.Definition old)
		{
			return old.Verb?.ToUpperInvariant() ?? throw BridgeGap.Open(old, $"inventory {NameOf(old)} (no verb)");
		}

		/// <summary>A variable kept in an identifier with its <c>@</c>.</summary>
		internal static Ast.Identifier Variable(object owner, string text)
		{
			return text.StartsWith('@') ? new Ast.Identifier(text) : throw BridgeGap.Open(owner, $"inventory {NameOf(owner)} (`{text}` where a variable stands)");
		}

		// ── Message types, contracts, services ────────────────────────────────────

		static Ast.Statement MessageType(Statement.MessageTypeDefinition old)
		{
			var reader = new Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);
			var owner  = Verb(old) == "CREATE" ? reader.Authorization() : null;

			Ast.MessageValidation? validation = null;

			if (reader.Take("VALIDATION"))
			{
				reader.ExpectPunctuation("=");

				validation = reader.Is("VALID_XML")
					? ValidXml(reader)
					: new Ast.MessageValidation.Named(reader.Name());
			}

			reader.Finish();

			return Verb(old) switch
			{
				"CREATE" => new Ast.Statement.CreateMessageType(name) { Authorization = owner, Validation = validation },
				"ALTER"  => new Ast.Statement.AlterMessageType(name, validation ?? throw reader.Unread()),
				_        => throw reader.Unread(),
			};
		}

		static Ast.MessageValidation ValidXml(Tail reader)
		{
			reader.Expect("VALID_XML", "WITH", "SCHEMA", "COLLECTION");

			return new Ast.MessageValidation.ValidXml(reader.Qualified());
		}

		static Ast.Statement Contract(Statement.ContractDefinition old)
		{
			var reader = new Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);
			var owner  = reader.Authorization();
			var items  = new List<Ast.ContractMessage>();

			reader.ExpectPunctuation("(");

			do
			{
				var message = reader.Name();

				reader.Expect("SENT", "BY");

				var sender = reader.Take("INITIATOR") ? Ast.ContractSender.Initiator
					: reader.Take("TARGET") ? Ast.ContractSender.Target
					: reader.Take("ANY") ? Ast.ContractSender.Any
					: throw reader.Unread();

				items.Add(new Ast.ContractMessage(message, sender));
			}
			while (reader.TakePunctuation(","));

			reader.ExpectPunctuation(")");
			reader.Finish();

			return new Ast.Statement.CreateContract(name, items.ToSqlList()) { Authorization = owner };
		}

		static Ast.Statement Service(Statement.ServiceDefinition old)
		{
			var reader = new Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			if (Verb(old) == "CREATE")
			{
				var owner = reader.Authorization();

				reader.Expect("ON", "QUEUE");

				var queue     = reader.Qualified();
				var contracts = new List<Ast.Identifier>();

				if (reader.TakePunctuation("("))
				{
					do
						contracts.Add(reader.Name());
					while (reader.TakePunctuation(","));

					reader.ExpectPunctuation(")");
				}

				reader.Finish();

				return new Ast.Statement.CreateService(name, queue) { Authorization = owner, Contracts = contracts.ToSqlList() };
			}

			Ast.QualifiedName? on = null;

			if (reader.Take("ON", "QUEUE"))
				on = reader.Qualified();

			var changes = new List<Ast.ServiceChange>();

			if (reader.TakePunctuation("("))
			{
				do
				{
					var action = reader.Take("ADD") ? Ast.AddOrDrop.Add : reader.Take("DROP") ? Ast.AddOrDrop.Drop : throw reader.Unread();

					reader.Expect("CONTRACT");
					changes.Add(new Ast.ServiceChange(action, reader.Name()));
				}
				while (reader.TakePunctuation(","));

				reader.ExpectPunctuation(")");
			}

			reader.Finish();

			return Verb(old) == "ALTER" ? new Ast.Statement.AlterService(name) { Queue = on, Changes = changes.ToSqlList() } : throw reader.Unread();
		}

		// ── Queues ────────────────────────────────────────────────────────────────

		static Ast.Statement Queue(Statement.QueueDefinition old)
		{
			var reader = new Tail(old, old.Tail);
			var name   = Values.Name(old, old.Name);

			if (Verb(old) == "CREATE")
			{
				var options = reader.Take("WITH") ? QueueOptions(reader) : [];
				var on      = reader.Take("ON") ? reader.Group(Ast.PlacementKind.On, quoted: true) : null;

				reader.Finish();

				return new Ast.Statement.CreateQueue(name) { Options = options, On = on };
			}

			Ast.QueueChange change;

			if (reader.Take("WITH"))
				change = new Ast.QueueChange.With(QueueOptions(reader));
			else if (reader.Take("REBUILD"))
				change = new Ast.QueueChange.Rebuild(reader.Take("WITH") ? Bracketed(reader, "MAXDOP", reader.Number) : null);
			else if (reader.Take("REORGANIZE"))
				change = new Ast.QueueChange.Reorganize(reader.Take("WITH") ? Bracketed(reader, "LOB_COMPACTION", reader.OnOff) : null);
			else if (reader.Take("MOVE", "TO"))
				change = new Ast.QueueChange.MoveTo(reader.Group(Ast.PlacementKind.MoveTo, quoted: false));
			else
				throw reader.Unread();

			reader.Finish();

			return new Ast.Statement.AlterQueue(name, change);
		}

		/// <summary><c>(NAME = value)</c>: one setting in brackets, as a REBUILD and a REORGANIZE say theirs.</summary>
		static T Bracketed<T>(Tail reader, string word, Func<T> value)
		{
			reader.ExpectPunctuation("(");
			reader.Expect(word);
			reader.ExpectPunctuation("=");

			var made = value();

			reader.ExpectPunctuation(")");

			return made;
		}

		static SqlList<Ast.QueueOption> QueueOptions(Tail reader)
		{
			var options = new List<Ast.QueueOption>();

			do
			{
				if (reader.Take("STATUS"))
				{
					reader.ExpectPunctuation("=");
					options.Add(new Ast.QueueOption.Status(reader.OnOff()));
				}
				else if (reader.Take("RETENTION"))
				{
					reader.ExpectPunctuation("=");
					options.Add(new Ast.QueueOption.Retention(reader.OnOff()));
				}
				else if (reader.Take("POISON_MESSAGE_HANDLING"))
				{
					options.Add(new Ast.QueueOption.PoisonMessageHandling(Bracketed(reader, "STATUS", reader.OnOff)));
				}
				else if (reader.Take("ACTIVATION", "(", "DROP", ")"))
				{
					options.Add(new Ast.QueueOption.DropActivation());
				}
				else if (reader.Take("ACTIVATION"))
				{
					reader.ExpectPunctuation("(");

					var settings = new List<Ast.ActivationSetting>();

					do
					{
						if (reader.Take("STATUS"))
						{
							reader.ExpectPunctuation("=");
							settings.Add(new Ast.ActivationSetting.Status(reader.OnOff()));
						}
						else if (reader.Take("PROCEDURE_NAME"))
						{
							reader.ExpectPunctuation("=");
							settings.Add(new Ast.ActivationSetting.ProcedureName(reader.Qualified()));
						}
						else if (reader.Take("MAX_QUEUE_READERS"))
						{
							reader.ExpectPunctuation("=");
							settings.Add(new Ast.ActivationSetting.MaxQueueReaders(reader.Number()));
						}
						else if (reader.Take("EXECUTE", "AS"))
						{
							settings.Add(new Ast.ActivationSetting.ExecuteAs(reader.Take("SELF") ? new Ast.ExecutionContext(Ast.ExecutionContextKind.Self, null)
								: reader.Take("OWNER") ? new Ast.ExecutionContext(Ast.ExecutionContextKind.Owner, null)
								: new Ast.ExecutionContext(Ast.ExecutionContextKind.User, reader.String())));
						}
						else
							throw reader.Unread();
					}
					while (reader.TakePunctuation(","));

					reader.ExpectPunctuation(")");
					options.Add(new Ast.QueueOption.Activation(settings.ToSqlList()));
				}
				else
					throw reader.Unread();
			}
			while (reader.TakePunctuation(","));

			return options.ToSqlList();
		}

		// ── Routes, bindings, priorities ──────────────────────────────────────────

		static Ast.Statement Route(Statement.RouteDefinition old)
		{
			var reader = new Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);
			var owner  = Verb(old) == "CREATE" ? reader.Authorization() : null;

			reader.Expect("WITH");

			var options = reader.Settings();

			reader.Finish();

			return Verb(old) switch
			{
				"CREATE" => new Ast.Statement.CreateRoute(name, options) { Authorization = owner },
				"ALTER"  => new Ast.Statement.AlterRoute(name, options),
				_        => throw reader.Unread(),
			};
		}

		static Ast.Statement Binding(Statement.RemoteServiceBindingDefinition old)
		{
			var reader = new Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			if (Verb(old) == "CREATE")
			{
				var owner = reader.Authorization();

				reader.Expect("TO", "SERVICE");

				var service = reader.String();

				reader.Expect("WITH");

				var options = reader.Settings();

				reader.Finish();

				return new Ast.Statement.CreateRemoteServiceBinding(name, service, options) { Authorization = owner };
			}

			reader.Expect("WITH");

			var altered = reader.Settings();

			reader.Finish();

			return Verb(old) == "ALTER" ? new Ast.Statement.AlterRemoteServiceBinding(name, altered) : throw reader.Unread();
		}

		static Ast.Statement Priority(Statement.BrokerPriorityDefinition old)
		{
			var reader = new Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			reader.Expect("FOR", "CONVERSATION");

			SqlList<Ast.Option> options = [];

			if (reader.Take("SET"))
			{
				reader.ExpectPunctuation("(");
				options = reader.Settings();
				reader.ExpectPunctuation(")");
			}

			reader.Finish();

			return Verb(old) switch
			{
				"CREATE" => new Ast.Statement.CreateBrokerPriority(name) { Options = options },
				"ALTER"  => new Ast.Statement.AlterBrokerPriority(name, options),
				_        => throw reader.Unread(),
			};
		}

		// ── Conversations ─────────────────────────────────────────────────────────

		static Ast.Statement Dialog(Statement.BeginDialog old)
		{
			return new Ast.Statement.BeginDialog(Variable(old, old.Handle), Values.Name(old, old.From), Values.Of(old.To))
			{
				Conversation = old.Conversation,
				Instance     = old.Instance is null ? null : Values.Of(old.Instance),
				Contract     = old.Contract is null ? null : Values.Name(old, old.Contract),
				Options      = Each(old.Options, Tables.Option),
			};
		}

		static Ast.Statement End(Statement.EndConversation old)
		{
			return new Ast.Statement.EndConversation(Values.Of(old.Handle))
			{
				Error       = old.Error is null ? null : Values.Of(old.Error),
				Description = old.Description is null ? null : Values.Of(old.Description),
				Cleanup     = old.Cleanup,
			};
		}

		static Ast.Statement Send(Statement.Send old)
		{
			return new Ast.Statement.Send(Each(old.Conversations, Values.Of))
			{
				Bracketed   = old.Bracketed,
				MessageType = old.MessageType is null ? null
					: old.MessageType.StartsWith('@') ? new Ast.Expression.Variable(Variable(old, old.MessageType))
					: new Ast.Expression.Reference(Values.Name(old, old.MessageType)),
				Body        = old.Body is null ? null : Values.Of(old.Body),
			};
		}

		static Ast.Statement.Receive Receive(Statement.Receive old)
		{
			return new Ast.Statement.Receive(Each(old.Columns, static one => Convert<Ast.SelectColumn>(one)), Values.Name(old, old.Queue))
			{
				Top   = Queries.Topped(old.Top),
				Into  = old.Into is null ? null : Variable(old, old.Into),
				Where = old.Where is null ? null : Values.Of(old.Where),
			};
		}

		static Ast.Statement Wait(Statement.WaitForStatement old)
		{
			var timeout = old.Timeout is null ? null : Values.Of(old.Timeout);

			return StatementOf(old.Statement) switch
			{
				Ast.Statement.Receive receive            => new Ast.Statement.WaitForReceive(receive, timeout),
				Ast.Statement.GetConversationGroup group => new Ast.Statement.WaitForGetConversationGroup(group, timeout),
				_                                        => throw BridgeGap.Open(old, "inventory Statement.WaitForStatement (a statement other than RECEIVE and GET CONVERSATION GROUP)"),
			};
		}

		// ── What follows a name ──────────────────────────────────────────────────

		/// <summary>
		/// What follows a definition's name, read from left to right: each step is checked, and a text that is not read to its
		/// end, or not as the grammar reads it, is a gap. Shared by the Broker, Monitoring and Server arms.
		/// </summary>
		internal sealed class Tail
		{
			readonly object _old;
			readonly string? _text;
			readonly List<SqlTokens.Token> _tokens;
			readonly List<int> _starts;
			int _at;

			/// <summary>
			/// The tokens of the text. A number with a signed exponent, <c>1E-3</c>, is cut at its sign by the tokens and is one
			/// number to the grammar: its three tokens are put together.
			/// </summary>
			public Tail(object old, string? text)
			{
				_old    = old;
				_text   = text;
				_tokens = [];
				_starts = [];

				var tokens = SqlTokens.Of(text ?? "");
				var starts = Starts(text ?? "");

				for (var at = 0; at < tokens.Count; at++)
				{
					if (tokens[at].Kind == SqlTokens.Kind.Number && Regex.IsMatch(tokens[at].Text, "^(?:[0-9]+[.]?[0-9]*|[.][0-9]+)[eE]$")
						&& at + 2 < tokens.Count && tokens[at + 1] is { Kind: SqlTokens.Kind.Punctuation, Text: "-" or "+" } && tokens[at + 2].Kind == SqlTokens.Kind.Number
						&& Regex.IsMatch(tokens[at + 2].Text, "^[0-9]+$"))
					{
						_tokens.Add(new SqlTokens.Token(SqlTokens.Kind.Number, tokens[at].Text + tokens[at + 1].Text + tokens[at + 2].Text));
						_starts.Add(starts[at]);
						at += 2;
					}
					else
					{
						_tokens.Add(tokens[at]);
						_starts.Add(starts[at]);
					}
				}
			}

			/// <summary>The position each token of a text starts at, comments and blanks skipped as the tokens skip them.</summary>
			static List<int> Starts(string text)
			{
				var starts = new List<int>();
				var at     = 0;

				foreach (var token in SqlTokens.Of(text))
				{
					at = SkipBlanks(text, at);
					starts.Add(at);
					at += token.Text.Length;
				}

				return starts;
			}

			static int SkipBlanks(string text, int at)
			{
				while (at < text.Length)
				{
					if (char.IsWhiteSpace(text[at]))
						at++;
					else if (text[at] == '-' && at + 1 < text.Length && text[at + 1] == '-')
						at = text.IndexOf('\n', at) is var end && end >= 0 ? end + 1 : text.Length;
					else if (text[at] == '/' && at + 1 < text.Length && text[at + 1] == '*')
					{
						var depth = 0;

						while (at < text.Length)
						{
							if (text[at] == '/' && at + 1 < text.Length && text[at + 1] == '*')
							{
								depth++;
								at += 2;
							}
							else if (text[at] == '*' && at + 1 < text.Length && text[at + 1] == '/')
							{
								at += 2;

								if (--depth == 0)
									break;
							}
							else
								at++;
						}
					}
					else
						break;
				}

				return at;
			}

			/// <summary>The text from the next token to the end, as it was written.</summary>
			public string Rest()
			{
				if (_at >= _tokens.Count)
					throw Unread();

				var rest = _text![_starts[_at]..];

				_at = _tokens.Count;

				return rest;
			}

			public bool End
			{
				get { return _at >= _tokens.Count; }
			}

			public BridgeGap Unread()
			{
				return BridgeGap.Open(_old, $"inventory {NameOf(_old)} (`{_text}` after the name)");
			}

			public void Finish()
			{
				if (!End)
					throw Unread();
			}

			bool WordAt(int at, string word)
			{
				return at < _tokens.Count && _tokens[at].Kind == SqlTokens.Kind.Word && _tokens[at].Text.Equals(word, StringComparison.OrdinalIgnoreCase);
			}

			/// <summary>Whether the next tokens are these words, a mark of punctuation among them standing for itself.</summary>
			public bool Is(params string[] words)
			{
				for (var at = 0; at < words.Length; at++)
				{
					var one = words[at];

					if (one.Length == 1 && !char.IsLetter(one[0]))
					{
						if (_at + at >= _tokens.Count || _tokens[_at + at] is not { Kind: SqlTokens.Kind.Punctuation } mark || mark.Text != one)
							return false;
					}
					else if (!WordAt(_at + at, one))
						return false;
				}

				return true;
			}

			public bool Take(params string[] words)
			{
				if (!Is(words))
					return false;

				_at += words.Length;

				return true;
			}

			/// <summary>Whether the next token is a string.</summary>
			public bool PeekString()
			{
				return _at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.String;
			}

			/// <summary>The text of the next token, taken whatever it is.</summary>
			public string Raw()
			{
				return _at < _tokens.Count ? _tokens[_at++].Text : throw Unread();
			}

			/// <summary>A whole number a minus may precede, kept as one token (<c>'-'? &amp; Digits</c>).</summary>
			public Ast.LiteralValue.Numeric Signed()
			{
				var minus = TakePunctuation("-");

				return Values.Of(new Expression.Literal(SqlLiteralKind.Number, (minus ? "-" : "") + Number().Text)) as Ast.LiteralValue.Numeric ?? throw Unread();
			}

			/// <summary>The text of the token just taken.</summary>
			public string Last
			{
				get { return _tokens[_at - 1].Text; }
			}

			public void Expect(params string[] words)
			{
				if (!Take(words))
					throw Unread();
			}

			public bool IsPunctuation(string text)
			{
				return _at < _tokens.Count && _tokens[_at] is { Kind: SqlTokens.Kind.Punctuation } token && token.Text == text;
			}

			public bool TakePunctuation(string text)
			{
				if (!IsPunctuation(text))
					return false;

				_at++;

				return true;
			}

			public void ExpectPunctuation(string text)
			{
				if (!TakePunctuation(text))
					throw Unread();
			}

			/// <summary>One name of one part: a word or a name in brackets or quotes.</summary>
			public Ast.Identifier Name()
			{
				return _at < _tokens.Count && _tokens[_at].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited
					? Values.Part(_old, _tokens[_at++].Text)
					: throw Unread();
			}

			/// <summary>A name of several parts joined by dots, a part between two dots left out.</summary>
			public Ast.QualifiedName Qualified()
			{
				var text = new StringBuilder();
				var part = true;

				while (_at < _tokens.Count)
				{
					var token = _tokens[_at];

					if (part && token.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited)
					{
						text.Append(token.Text);
						part = false;
					}
					else if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "." })
					{
						text.Append('.');
						part = true;
					}
					else
						break;

					_at++;
				}

				if (text.Length == 0 || part)
					throw Unread();

				return Values.Name(_old, text.ToString());
			}

			/// <summary><c>AUTHORIZATION owner</c>, or null where none is written.</summary>
			public Ast.Identifier? Authorization()
			{
				return Take("AUTHORIZATION") ? Name() : null;
			}

			/// <summary>ON or OFF.</summary>
			public bool OnOff()
			{
				return Take("ON") ? true : Take("OFF") ? false : throw Unread();
			}

			/// <summary>A string, plain or national.</summary>
			public Ast.LiteralValue.String String()
			{
				return _at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.String && Values.String(_tokens[_at++].Text) is Ast.LiteralValue.String text
					? text
					: throw Unread();
			}

			/// <summary>An unsigned number.</summary>
			public Ast.LiteralValue.Numeric Number()
			{
				return _at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.Number && Values.Of(new Expression.Literal(SqlLiteralKind.Number, _tokens[_at++].Text)) is Ast.LiteralValue.Numeric number
					? number
					: throw Unread();
			}

			/// <summary>
			/// A filegroup after ON or MOVE TO: a name, or a string, plain or national, where the place reads one.
			/// </summary>
			public Ast.Placement Group(Ast.PlacementKind kind, bool quoted)
			{
				if (quoted && _at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.String)
				{
					var token = _tokens[_at++].Text;
					var text  = token[(token.IndexOf('\'') + 1)..^1].Replace("''", "'");

					return new Ast.Placement(kind, new Ast.Identifier(text, token[0] == '\'' ? Ast.IdentifierStyle.String : Ast.IdentifierStyle.NationalString), null);
				}

				return new Ast.Placement(kind, Name(), null);
			}

			/// <summary>
			/// Settings joined by commas, each a word, <c>=</c> and a value: a string, a number and the unit after it, or a name (ON, OFF,
			/// ANY and DEFAULT among them), the name as written.
			/// </summary>
			public SqlList<Ast.Option> Settings()
			{
				var made = new List<Ast.Option> { Setting() };

				while (TakePunctuation(","))
					made.Add(Setting());

				return made.ToSqlList();
			}

			/// <summary>One setting: a word, <c>=</c> and a value (<see cref="Settings"/>).</summary>
			public Ast.Option Setting()
			{
				if (_at >= _tokens.Count || _tokens[_at].Kind != SqlTokens.Kind.Word)
					throw Unread();

				var name  = Values.Part(_old, _tokens[_at++].Text);
				var words = new List<Ast.Identifier>();

				// A name of several words: `AFFINITY SCHEDULER`.
				while (_at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.Word)
					words.Add(Values.Part(_old, _tokens[_at++].Text));

				ExpectPunctuation("=");

				return new Ast.Option(name, SettingValue(), []) { Words = words.ToSqlList() };
			}

			/// <summary><c>(a TO b, c)</c>: the ranges of an affinity in brackets, whole numbers.</summary>
			public Ast.Expression.PoolRanges Ranges()
			{
				ExpectPunctuation("(");

				var ranges = new List<Ast.PartitionRange>();

				do
				{
					var from = Number();

					ranges.Add(new Ast.PartitionRange(from, Take("TO") ? Number() : null));
				}
				while (TakePunctuation(","));

				ExpectPunctuation(")");

				return new Ast.Expression.PoolRanges(ranges.ToSqlList());
			}

			/// <summary>
			/// The words of a securable's class and the <c>::</c> after them, as the class they name, or null where the next words are
			/// followed by no <c>::</c>. A class no word names is a gap.
			/// </summary>
			public Ast.SecurableClass? Class()
			{
				var end = _at;

				while (end < _tokens.Count && _tokens[end].Kind == SqlTokens.Kind.Word)
					end++;

				if (end == _at || end + 1 >= _tokens.Count || _tokens[end] is not { Kind: SqlTokens.Kind.Punctuation, Text: ":" } || _tokens[end + 1] is not { Kind: SqlTokens.Kind.Punctuation, Text: ":" })
					return null;

				var words = string.Join(' ', _tokens.Skip(_at).Take(end - _at).Select(static one => one.Text.ToUpperInvariant()));

				_at = end + 2;

				return Monitoring.ClassWords.Value.TryGetValue(words, out var kind) ? kind : throw Unread();
			}

			/// <summary>
			/// A value of one token, a number with its unit after it (<c>15 TB</c>, <c>1MB</c>) or a name; the unit is a word that is not
			/// the next clause.
			/// </summary>
			public Ast.Expression SettingValue()
			{
				if (_at >= _tokens.Count)
					throw Unread();

				if (IsPunctuation("("))
					return Ranges();

				var token = _tokens[_at++];

				switch (token.Kind)
				{
					case SqlTokens.Kind.String:
						return Values.String(token.Text);

					case SqlTokens.Kind.Number:
					{
						// `1MB` is one token to the tokens the text is compared by and two to the grammar, and the tree keeps no blank.
						if (Regex.IsMatch(token.Text, "^(?:[0-9]+[.]?[0-9]*|[.][0-9]+)[A-Za-z_]+$") && !Regex.IsMatch(token.Text, "^(?:[0-9]+[.]?[0-9]*|[.][0-9]+)[eE]$") && !token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
							throw BridgeGap.Lost(_old, "a number written with its unit against it (`1MB`): the text is compared by tokens, and the tree writes the unit after a blank");

						var number = Values.Of(new Expression.Literal(token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? SqlLiteralKind.Hex : SqlLiteralKind.Number, token.Text));

						if (_at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.Word)
							return new Ast.Expression.Measured(number, new Ast.Identifier(_tokens[_at++].Text));

						return number;
					}

					case SqlTokens.Kind.Word when token.Text.Equals("NULL", StringComparison.OrdinalIgnoreCase):
						return Values.Of(Expression.NullValue);

					case SqlTokens.Kind.Word or SqlTokens.Kind.Delimited:
						return new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(_old, token.Text)]));

					default:
						throw Unread();
				}
			}

			/// <summary><c>( settings )</c>.</summary>
			public SqlList<Ast.Option> Bracketed()
			{
				ExpectPunctuation("(");

				var made = Settings();

				ExpectPunctuation(")");

				return made;
			}

			/// <summary>The rest of the text, a search condition, read again by the grammar's rule for it.</summary>
			public Ast.Expression Condition()
			{
				var text = Rest();

				return TransactSql.TransactSqlParser.TryParseSql("SELECT 1 WHERE " + text.Trim()) is { IsSuccess: true, Value: [Statement.Select { Of: Query.Specification { Where: { } where } }] }
					? Values.Of(where)
					: throw Unread();
			}

			/// <summary>The rest of the text, values joined by commas, read again as a select list reads them.</summary>
			public SqlList<Ast.Expression> ValueList()
			{
				var text = Rest();

				return TransactSql.TransactSqlParser.TryParseSql("SELECT " + text.Trim()) is { IsSuccess: true, Value: [Statement.Select { Of: Query.Specification { Columns: var columns } }] }
					&& columns.All(static one => one is Clause.DerivedColumn { Name: null })
					? Each(columns, static one => Values.Of(((Clause.DerivedColumn)one).Value))
					: throw Unread();
			}

			/// <summary>
			/// A run of words (<c>ActionWords</c>): the words up to a comma, a bracket or one of the words that end a run, and one at least.
			/// </summary>
			public Ast.ActionWords ActionWords()
			{
				var words = new List<Ast.Identifier>();

				while (_at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.Word && !WordAt(_at, "ON") && !WordAt(_at, "TO") && !WordAt(_at, "FROM") && !WordAt(_at, "WITH") && !WordAt(_at, "CASCADE"))
					words.Add(new Ast.Identifier(_tokens[_at++].Text));

				return words.Count > 0 ? new Ast.ActionWords(words.ToSqlList()) : throw Unread();
			}

			/// <summary>A name of one part, two or three, joined by dots (<c>EventObject</c>).</summary>
			public Ast.QualifiedName EventName()
			{
				var parts = new List<string>();

				do
				{
					if (_at >= _tokens.Count || _tokens[_at].Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited))
						throw Unread();

					parts.Add(_tokens[_at++].Text);
				}
				while (TakePunctuation("."));

				return Values.Name(_old, string.Join('.', parts));
			}

			/// <summary>
			/// A value of an event (<c>EventValue</c>): a number, a minus before it, a binary string or a string, in brackets as often as
			/// written. A binary string with a minus has no place in the tree.
			/// </summary>
			public Ast.Expression EventValue()
			{
				if (TakePunctuation("("))
				{
					var inner = EventValue();

					ExpectPunctuation(")");

					return new Ast.Expression.Parenthesized(inner);
				}

				var minus = TakePunctuation("-");

				if (_at >= _tokens.Count)
					throw Unread();

				var token = _tokens[_at++];

				switch (token.Kind)
				{
					case SqlTokens.Kind.Number when token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase):
						return minus ? throw BridgeGap.Missing(_old, "a binary string with a minus before it, which the grammar's EventValue reads and the tree has no place for") : Values.Of(new Expression.Literal(SqlLiteralKind.Hex, token.Text));

					case SqlTokens.Kind.Number:
						return Values.Of(new Expression.Literal(SqlLiteralKind.Number, (minus ? "-" : "") + token.Text));

					case SqlTokens.Kind.String when !minus:
						return Values.String(token.Text);

					default:
						throw Unread();
				}
			}

			/// <summary>An event's predicate (<c>EventPredicate</c>): factors joined from the left by AND and OR.</summary>
			public Ast.EventPredicate Predicate()
			{
				var left = Factor();

				while (true)
				{
					var join = Take("AND") ? Ast.EventJoin.And : Take("OR") ? Ast.EventJoin.Or : (Ast.EventJoin?)null;

					if (join is not { } one)
						return left;

					left = new Ast.EventPredicate.Join(left, one, Factor());
				}
			}

			Ast.EventPredicate Factor()
			{
				return Take("NOT") ? new Ast.EventPredicate.Not(Factor()) : Term();
			}

			Ast.EventPredicate Term()
			{
				if (TakePunctuation("("))
				{
					var inner = Predicate();

					ExpectPunctuation(")");

					return new Ast.EventPredicate.Parenthesized(inner);
				}

				var name = EventName();

				if (TakePunctuation("("))
				{
					var field = EventName();

					ExpectPunctuation(",");

					var value = EventValue();

					ExpectPunctuation(")");

					return new Ast.EventPredicate.Call(name, field, value);
				}

				if (Take("NOT", "LIKE"))
					return new Ast.EventPredicate.Like(name, true, EventValue());

				if (Take("LIKE"))
					return new Ast.EventPredicate.Like(name, false, EventValue());

				var (op, bang) = Take("<", ">") ? (Ast.ComparisonOperator.NotEqual, false)
					: Take("!", "=") ? (Ast.ComparisonOperator.NotEqual, true)
					: Take("!", "<") ? (Ast.ComparisonOperator.NotLess, false)
					: Take("!", ">") ? (Ast.ComparisonOperator.NotGreater, false)
					: Take(">", "=") ? (Ast.ComparisonOperator.GreaterOrEqual, false)
					: Take("<", "=") ? (Ast.ComparisonOperator.LessOrEqual, false)
					: Take("=") ? (Ast.ComparisonOperator.Equal, false)
					: Take(">") ? (Ast.ComparisonOperator.Greater, false)
					: Take("<") ? (Ast.ComparisonOperator.Less, false)
					: throw Unread();

				return new Ast.EventPredicate.Compare(name, op, EventValue()) { Exclamation = bang };
			}
		}
	}
}
