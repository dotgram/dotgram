using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// The server and what runs beside the databases: resource governance, server configuration, endpoints, availability groups, and the administrative commands.
	/// </summary>
	/// <remarks>
	/// KILL, RECONFIGURE, SHUTDOWN and DBCC are nodes, carried as they stand but for the options of a DBCC, which are read again from
	/// the text they were written in (the old tree upper-cases a word it keeps). The old tree keeps what follows the name of the
	/// other objects as text (<c>Tail</c>), which the arms read token by token as the grammar's rules read it; a text the arm does not
	/// read to its end is a gap.
	/// </remarks>
	internal static class Server
	{
		public static readonly string[] Owns =
		[
			"Statement.ResourcePoolDefinition", "Statement.WorkloadGroupDefinition", "Statement.AlterResourceGovernor",
			"Statement.AlterServerConfiguration", "Statement.AvailabilityGroupDefinition", "Statement.EndpointDefinition",
			"Statement.Kill", "Statement.Reconfigure", "Statement.Shutdown",
			"Statement.Dbcc",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.Kill kill                    => Kill(kill),
				Statement.Reconfigure reconfigure      => new Ast.Statement.Reconfigure(reconfigure.Override),
				Statement.Shutdown shutdown            => new Ast.Statement.Shutdown(shutdown.NoWait),
				Statement.Dbcc dbcc                    => Dbcc(dbcc),
				Statement.AlterServerConfiguration c   => Configuration(c),
				Statement.AlterResourceGovernor g      => Governor(g),
				Statement.ResourcePoolDefinition pool  => Pool(pool),
				Statement.WorkloadGroupDefinition w    => Workload(w),
				Statement.AvailabilityGroupDefinition a => Availability(a),
				_ when Owned(Owns, old)                => throw Inventory.Unconverted(old),
				_                                      => null,
			};

			return made is not null;
		}

		// ── ALTER SERVER CONFIGURATION ────────────────────────────────────────────

		static Ast.Statement Configuration(Statement.AlterServerConfiguration old)
		{
			var reader = new Broker.Tail(old, old.Tail);

			reader.Expect("SET");

			Ast.Option setting;

			if (reader.Is("PROCESS", "AFFINITY"))
			{
				var words = Name(reader, 3);

				reader.ExpectPunctuation("=");
				setting = Setting(words, reader.Take("AUTO") ? Plain(reader) : Ranges(reader));
			}
			else if (reader.Is("DIAGNOSTICS", "LOG"))
			{
				var words = Name(reader, 2);

				if (reader.Is("PATH") || reader.Is("MAX_SIZE") || reader.Is("MAX_FILES"))
				{
					words = [.. words, reader.Name()];
					reader.ExpectPunctuation("=");
					setting = Setting(words, reader.SettingValue());
				}
				else
					setting = Setting(words, Switch(reader));
			}
			else if (reader.Is("FAILOVER", "CLUSTER", "PROPERTY"))
			{
				var words = Name(reader, 3);

				words = [.. words, reader.Name()];
				reader.ExpectPunctuation("=");
				setting = Setting(words, reader.SettingValue());
			}
			else if (reader.Is("HADR", "CLUSTER", "CONTEXT"))
			{
				var words = Name(reader, 3);

				reader.ExpectPunctuation("=");
				setting = Setting(words, reader.SettingValue());
			}
			else if (reader.Is("BUFFER", "POOL", "EXTENSION"))
			{
				var words = Name(reader, 3);
				var value = Switch(reader);

				setting = Setting(words, value, ((Ast.Expression.Reference)value).Name[0].Text.Equals("ON", StringComparison.OrdinalIgnoreCase) ? reader.Bracketed() : []);
			}
			else if (reader.Is("SOFTNUMA"))
			{
				setting = Setting(Name(reader, 1), Switch(reader));
			}
			else if (reader.Is("MEMORY_OPTIMIZED"))
			{
				var words = Name(reader, 1);

				if (reader.Is("TEMPDB_METADATA") || reader.Is("HYBRID_BUFFER_POOL"))
				{
					words = [.. words, reader.Name()];
					reader.ExpectPunctuation("=");

					var value = Switch(reader);

					setting = Setting(words, value, reader.IsPunctuation("(") ? reader.Bracketed() : []);
				}
				else
					setting = Setting(words, Switch(reader));
			}
			else if (reader.Is("SUSPEND_FOR_SNAPSHOT_BACKUP"))
			{
				var words = Name(reader, 1);

				reader.ExpectPunctuation("=");

				var value = Switch(reader);

				setting = Setting(words, value, reader.IsPunctuation("(") ? Scope(old, reader) : []);
			}
			else if (reader.Is("EXTERNAL", "AUTHENTICATION"))
			{
				var words = Name(reader, 2);
				var value = Switch(reader);

				setting = Setting(words, value, reader.IsPunctuation("(") ? Way(reader) : []);
			}
			else
				throw reader.Unread();

			reader.Finish();

			return new Ast.Statement.AlterServerConfiguration(setting);
		}

		/// <summary>The words of a setting's name, as written, the first in the name and the rest as its words.</summary>
		static Ast.Identifier[] Name(Broker.Tail reader, int count)
		{
			var words = new Ast.Identifier[count];

			for (var at = 0; at < count; at++)
				words[at] = reader.Name();

			return words;
		}

		static Ast.Option Setting(Ast.Identifier[] words, Ast.Expression value, SqlList<Ast.Option>? items = null)
		{
			return new Ast.Option(words[0], value, items ?? []) { Words = [.. words.Skip(1)] };
		}

		static Ast.Expression Plain(Broker.Tail reader)
		{
			return new Ast.Expression.Reference(new Ast.QualifiedName([new Ast.Identifier(reader.Last)]));
		}

		/// <summary>ON or OFF, as written.</summary>
		static Ast.Expression Switch(Broker.Tail reader)
		{
			return reader.Take("ON") || reader.Take("OFF") ? Plain(reader) : throw reader.Unread();
		}

		/// <summary><c>ServerRanges</c>: whole numbers and ranges of them.</summary>
		static Ast.Expression Ranges(Broker.Tail reader)
		{
			var ranges = new List<Ast.PartitionRange>();

			do
			{
				var from = reader.Number();
				var to   = reader.Take("TO") ? reader.Number() : null;

				ranges.Add(new Ast.PartitionRange(from, to));
			}
			while (reader.TakePunctuation(","));

			return new Ast.Expression.ServerRanges(ranges.ToSqlList());
		}

		/// <summary><c>(GROUP = (db, …), MODE = COPY_ONLY)</c> or <c>(MODE = COPY_ONLY)</c>: the group a list of names.</summary>
		static SqlList<Ast.Option> Scope(object old, Broker.Tail reader)
		{
			reader.ExpectPunctuation("(");

			var items = new List<Ast.Option>();

			do
			{
				var name = reader.Name();

				reader.ExpectPunctuation("=");

				if (name.Text.Equals("GROUP", StringComparison.OrdinalIgnoreCase))
				{
					reader.ExpectPunctuation("(");

					var databases = new List<Ast.Expression>();

					do
						databases.Add(new Ast.Expression.Reference(new Ast.QualifiedName([reader.Name()])));
					while (reader.TakePunctuation(","));

					reader.ExpectPunctuation(")");
					items.Add(new Ast.Option(name, new Ast.Expression.Row(databases.ToSqlList()), []));
				}
				else
					items.Add(new Ast.Option(name, new Ast.Expression.Reference(new Ast.QualifiedName([reader.Name()])), []));
			}
			while (reader.TakePunctuation(","));

			reader.ExpectPunctuation(")");

			return items.ToSqlList();
		}

		/// <summary><c>(USE_IDENTITY)</c> or <c>(CREDENTIAL_NAME = 'name')</c>.</summary>
		static SqlList<Ast.Option> Way(Broker.Tail reader)
		{
			reader.ExpectPunctuation("(");

			var name = reader.Name();
			Ast.Expression? value = null;

			if (reader.TakePunctuation("="))
				value = reader.String();

			reader.ExpectPunctuation(")");

			return [new Ast.Option(name, value, [])];
		}

		// ── The resource governor, resource pools and workload groups ─────────────

		static Ast.Statement Governor(Statement.AlterResourceGovernor old)
		{
			var reader = new Broker.Tail(old, old.Tail);

			Ast.ResourceGovernorAction action;

			if (reader.Take("RECONFIGURE"))
				action = new Ast.ResourceGovernorAction.Reconfigure();
			else if (reader.Take("DISABLE"))
				action = new Ast.ResourceGovernorAction.Disable();
			else if (reader.Take("RESET", "STATISTICS"))
				action = new Ast.ResourceGovernorAction.ResetStatistics();
			else if (reader.Take("WITH"))
			{
				reader.ExpectPunctuation("(");

				var name = reader.Name();

				reader.ExpectPunctuation("=");

				Ast.Expression value;

				if (name.Text.Equals("CLASSIFIER_FUNCTION", StringComparison.OrdinalIgnoreCase))
				{
					if (reader.Take("NULL"))
						value = Values.Of(Expression.NullValue);
					else
					{
						var function = reader.Qualified();

						value = function.Count == 2 ? new Ast.Expression.Reference(function) : throw reader.Unread();
					}
				}
				else
					value = reader.SettingValue();

				reader.ExpectPunctuation(")");
				action = new Ast.ResourceGovernorAction.Settings(new Ast.Option(name, value, []));
			}
			else
				throw reader.Unread();

			var after = reader.Take("RECONFIGURE");

			reader.Finish();

			return new Ast.Statement.AlterResourceGovernor(action) { ReconfigureAfter = after };
		}

		/// <summary>A pool's or a group's name: a name, or a string in single quotes held as a string-style identifier.</summary>
		static Ast.Identifier PoolName(object old, string name)
		{
			return name.StartsWith('\'') ? new Ast.Identifier(name[1..^1].Replace("''", "'"), Ast.IdentifierStyle.String) : Values.Part(old, name);
		}

		static Ast.Statement Pool(Statement.ResourcePoolDefinition old)
		{
			var reader  = new Broker.Tail(old, old.Tail);
			var name    = PoolName(old, old.Name);
			var options = reader.Take("WITH") ? reader.Bracketed() : [];

			reader.Finish();

			return Broker.Verb(old) switch
			{
				"CREATE" => new Ast.Statement.CreateResourcePool(name) { Options = options },
				"ALTER"  => new Ast.Statement.AlterResourcePool(name, options),
				_        => throw reader.Unread(),
			};
		}

		static Ast.Statement Workload(Statement.WorkloadGroupDefinition old)
		{
			var reader  = new Broker.Tail(old, old.Tail);
			var name    = PoolName(old, old.Name);
			var options = reader.Take("WITH") ? reader.Bracketed() : [];
			var pools   = new List<Ast.WorkloadPool>();

			if (reader.Take("USING"))
			{
				do
				{
					var external = reader.Take("EXTERNAL");

					pools.Add(new Ast.WorkloadPool(reader.Name(), external));
				}
				while (reader.TakePunctuation(","));
			}

			reader.Finish();

			return Broker.Verb(old) switch
			{
				"CREATE" => new Ast.Statement.CreateWorkloadGroup(name) { Options = options, Pools = pools.ToSqlList() },
				"ALTER"  => new Ast.Statement.AlterWorkloadGroup(name) { Options = options, Pools = pools.ToSqlList() },
				_        => throw reader.Unread(),
			};
		}

		// ── Availability groups ───────────────────────────────────────────────────

		static Ast.Statement Availability(Statement.AvailabilityGroupDefinition old)
		{
			var reader = new Broker.Tail(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			if (Broker.Verb(old) == "ALTER")
			{
				var action = AvailabilityAction(old, reader);

				reader.Finish();

				return new Ast.Statement.AlterAvailabilityGroup(name, action);
			}

			SqlList<Ast.Option> options = [];

			if (reader.Take("WITH"))
			{
				reader.ExpectPunctuation("(");
				options = AvailabilityOptions(reader, closing: true);
			}

			Ast.AvailabilitySource source;

			// The FOR belongs to the replicas: a distributed group is made of `AVAILABILITY GROUP ON …` alone.
			if (reader.Take("AVAILABILITY", "GROUP", "ON"))
				source = new Ast.AvailabilitySource.Groups(GroupSpecs(reader));
			else
			{
				reader.Expect("FOR");

				var databases = new List<Ast.Identifier>();

				if (reader.Take("DATABASE"))
				{
					do
						databases.Add(reader.Name());
					while (reader.TakePunctuation(","));
				}

				reader.Expect("REPLICA", "ON");

				var servers = new List<Ast.AvailabilityReplica> { Replica(reader) };

				while (reader.TakePunctuation(","))
					servers.Add(Replica(reader));

				source = new Ast.AvailabilitySource.Replicas(databases.ToSqlList(), servers.ToSqlList());
			}

			Ast.LiteralValue.String? listenerName = null;
			Ast.AvailabilityListener? listener    = null;

			if (reader.Take("LISTENER"))
			{
				listenerName = reader.String();
				reader.ExpectPunctuation("(");
				listener = Listener(reader);
				reader.ExpectPunctuation(")");
			}

			reader.Finish();

			return new Ast.Statement.CreateAvailabilityGroup(name, source) { Options = options, ListenerName = listenerName, Listener = listener };
		}

		static Ast.AvailabilityAction AvailabilityAction(object old, Broker.Tail reader)
		{
			if (reader.Take("SET"))
			{
				reader.ExpectPunctuation("(");

				var options = AvailabilityOptions(reader, closing: true);

				return options is [{ } setting] ? new Ast.AvailabilityAction.Set(setting) : throw reader.Unread();
			}

			if (reader.Take("ADD", "DATABASE"))
				return new Ast.AvailabilityAction.AddDatabase(reader.Name());

			if (reader.Take("REMOVE", "DATABASE"))
				return new Ast.AvailabilityAction.RemoveDatabase(reader.Name());

			if (reader.Take("ADD", "REPLICA", "ON"))
				return new Ast.AvailabilityAction.AddReplica(Replica(reader));

			if (reader.Take("MODIFY", "REPLICA", "ON"))
				return new Ast.AvailabilityAction.ModifyReplica(Replica(reader));

			if (reader.Take("REMOVE", "REPLICA", "ON"))
				return new Ast.AvailabilityAction.RemoveReplica(reader.String());

			if (reader.Take("JOIN", "AVAILABILITY", "GROUP", "ON"))
				return new Ast.AvailabilityAction.JoinAvailabilityGroup(GroupSpecs(reader));

			if (reader.Take("MODIFY", "AVAILABILITY", "GROUP", "ON"))
				return new Ast.AvailabilityAction.ModifyAvailabilityGroup(GroupSpecs(reader));

			if (reader.Take("JOIN"))
				return new Ast.AvailabilityAction.Join();

			if (reader.Take("GRANT", "CREATE", "ANY", "DATABASE"))
				return new Ast.AvailabilityAction.CreateAnyDatabase(Ast.AvailabilityPermission.Grant);

			if (reader.Take("DENY", "CREATE", "ANY", "DATABASE"))
				return new Ast.AvailabilityAction.CreateAnyDatabase(Ast.AvailabilityPermission.Deny);

			if (reader.Take("FAILOVER"))
				return new Ast.AvailabilityAction.Failover();

			if (reader.Take("FORCE_FAILOVER_ALLOW_DATA_LOSS"))
				return new Ast.AvailabilityAction.ForceFailoverAllowDataLoss();

			if (reader.Take("OFFLINE"))
				return new Ast.AvailabilityAction.Offline();

			if (reader.Take("ADD", "LISTENER"))
			{
				var name = reader.String();

				reader.ExpectPunctuation("(");

				var listener = Listener(reader);

				reader.ExpectPunctuation(")");

				return new Ast.AvailabilityAction.AddListener(name, listener);
			}

			if (reader.Take("MODIFY", "LISTENER"))
			{
				var name = reader.String();

				reader.ExpectPunctuation("(");

				Ast.ListenerChange change = reader.Take("ADD", "IP") ? new Ast.ListenerChange.AddIp(Address(reader))
					: reader.Take("PORT") ? PortChange(reader)
					: throw reader.Unread();

				reader.ExpectPunctuation(")");

				return new Ast.AvailabilityAction.ModifyListener(name, change);
			}

			if (reader.Take("RESTART", "LISTENER"))
				return new Ast.AvailabilityAction.RestartListener(reader.String());

			if (reader.Take("REMOVE", "LISTENER"))
				return new Ast.AvailabilityAction.RemoveListener(reader.String());

			throw reader.Unread();
		}

		static Ast.ListenerChange PortChange(Broker.Tail reader)
		{
			reader.ExpectPunctuation("=");

			return new Ast.ListenerChange.Port(reader.Number());
		}

		/// <summary><c>('address' [, 'mask'])</c>.</summary>
		static Ast.ListenerAddress Address(Broker.Tail reader)
		{
			reader.ExpectPunctuation("(");

			var address = reader.String();
			var mask    = reader.TakePunctuation(",") ? reader.String() : null;

			reader.ExpectPunctuation(")");

			return new Ast.ListenerAddress(address, mask);
		}

		static Ast.AvailabilityListener Listener(Broker.Tail reader)
		{
			reader.Expect("WITH");

			if (reader.Take("DHCP"))
				return new Ast.AvailabilityListener.Dhcp(reader.Take("ON") ? Address(reader) : null);

			reader.Expect("IP");
			reader.ExpectPunctuation("(");

			var addresses = new List<Ast.ListenerAddress> { Address(reader) };

			while (reader.TakePunctuation(","))
				addresses.Add(Address(reader));

			reader.ExpectPunctuation(")");

			return new Ast.AvailabilityListener.Ip(addresses.ToSqlList()) { Port = reader.TakePunctuation(",") ? PortOf(reader) : null };
		}

		static Ast.LiteralValue.Numeric PortOf(Broker.Tail reader)
		{
			reader.Expect("PORT");
			reader.ExpectPunctuation("=");

			return reader.Number();
		}

		/// <summary><c>'server' WITH ( options )</c>.</summary>
		static Ast.AvailabilityReplica Replica(Broker.Tail reader)
		{
			var server = reader.String();

			reader.Expect("WITH");
			reader.ExpectPunctuation("(");

			return new Ast.AvailabilityReplica(server, AvailabilityOptions(reader, closing: true));
		}

		static SqlList<Ast.AvailabilityGroupSpec> GroupSpecs(Broker.Tail reader)
		{
			var specs = new List<Ast.AvailabilityGroupSpec>();

			do
			{
				var group = reader.String();

				reader.Expect("WITH");
				reader.ExpectPunctuation("(");
				specs.Add(new Ast.AvailabilityGroupSpec(group, AvailabilityOptions(reader, closing: true)));
			}
			while (reader.TakePunctuation(","));

			return specs.ToSqlList();
		}

		/// <summary>
		/// The options of an availability object after its opening bracket and up to the closing one: a word alone, a word and a value
		/// after <c>=</c> (a word, a string, a number, or strings in brackets), or a word and a bracket of options of its own.
		/// </summary>
		static SqlList<Ast.Option> AvailabilityOptions(Broker.Tail reader, bool closing)
		{
			var options = new List<Ast.Option>();

			do
			{
				var name = reader.Name();

				if (reader.TakePunctuation("="))
				{
					Ast.Expression value;

					if (reader.TakePunctuation("("))
					{
						var strings = new List<Ast.Expression> { reader.String() };

						while (reader.TakePunctuation(","))
							strings.Add(reader.String());

						reader.ExpectPunctuation(")");
						value = new Ast.Expression.Row(strings.ToSqlList());
					}
					else
						value = reader.SettingValue();

					options.Add(new Ast.Option(name, value, []));
				}
				else if (reader.TakePunctuation("("))
					options.Add(new Ast.Option(name, null, AvailabilityOptions(reader, closing: true)));
				else
					options.Add(new Ast.Option(name, null, []));
			}
			while (reader.TakePunctuation(","));

			if (closing)
				reader.ExpectPunctuation(")");

			return options.ToSqlList();
		}

		// ── KILL, DBCC ────────────────────────────────────────────────────────────

		static Ast.Statement Kill(Statement.Kill old)
		{
			var target = Values.Of(old.Target);

			switch (old.Kind)
			{
				case "STATS JOB":
					return new Ast.Statement.KillStatsJob(target as Ast.LiteralValue.Numeric ?? throw BridgeGap.Open(old, "inventory Statement.Kill (a statistics job that is no number)"));

				case "QUERY NOTIFICATION SUBSCRIPTION":
					return old.Target is Expression.ColumnReference { Text: var all } && all.Equals("ALL", StringComparison.OrdinalIgnoreCase)
						? new Ast.Statement.KillQueryNotificationSubscription { All = true }
						: new Ast.Statement.KillQueryNotificationSubscription { Subscription = target as Ast.LiteralValue.Numeric ?? throw BridgeGap.Open(old, "inventory Statement.Kill (a subscription that is no number)") };

				case "":
					Ast.KillOption? with = old.With?.ToUpperInvariant() switch
					{
						null         => null,
						"STATUSONLY" => Ast.KillOption.StatusOnly,
						"COMMIT"     => Ast.KillOption.Commit,
						"ROLLBACK"   => Ast.KillOption.Rollback,
						_            => throw BridgeGap.Open(old, $"inventory Statement.Kill (WITH {old.With})"),
					};

					return new Ast.Statement.Kill(target) { With = with };

				default:
					throw BridgeGap.Open(old, $"inventory Statement.Kill ({old.Kind})");
			}
		}

		static Ast.Statement Dbcc(Statement.Dbcc old)
		{
			return new Ast.Statement.Dbcc(Values.Part(old, old.Command))
			{
				Arguments = old.Arguments is { } arguments ? Each(arguments, one => DbccArgument(old, one)) : null,
				Options   = Each(old.Options, one => DbccOption(old, one)),
			};
		}

		static Ast.ExecuteArgument DbccArgument(Statement.Dbcc owner, Expression old)
		{
			return old is Expression.NamedArgument(var name, var value)
				? new Ast.ExecuteArgument(DbccValue(owner, value)) { Name = name.StartsWith('@') ? new Ast.Identifier(name) : Values.Part(owner, name) }
				: new Ast.ExecuteArgument(DbccValue(owner, old));
		}

		/// <summary>A value, money among them: the grammar reads blanks between the sign of money and its digits, which the tree keeps none of.</summary>
		static Ast.Expression DbccValue(Statement.Dbcc owner, Expression old)
		{
			return Values.Of(old) is Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Money } money && money.Text.Any(char.IsWhiteSpace)
				? throw BridgeGap.Lost(owner, $"money written with blanks (`{money.Text}`), which the tree writes without")
				: Values.Of(old);
		}

		/// <summary>
		/// An option read again from the text it was written in: the old tree upper-cases the words it keeps, and the tree keeps them as
		/// written.
		/// </summary>
		static Ast.Option DbccOption(Statement.Dbcc owner, Clause clause)
		{
			var read = new Broker.Tail(owner, Spelled(clause) ?? throw BridgeGap.Lost(owner, "the text of an option, which the old tree keeps with its words in capitals"));
			var name = read.Name();

			// Words joined by JOIN: the name, and the rest as its words.
			var words = new List<Ast.Identifier>();

			while (read.Is("JOIN"))
			{
				read.Expect("JOIN");
				words.Add(new Ast.Identifier("JOIN"));
				words.Add(read.Name());
			}

			Ast.Option option;

			if (read.TakePunctuation("="))
				option = new Ast.Option(name, read.SettingValue(), []) { Words = words.ToSqlList() };
			else if (read.IsPunctuation("("))
				option = new Ast.Option(name, null, read.Bracketed()) { Words = words.ToSqlList() };
			else
				option = new Ast.Option(name, null, []) { Words = words.ToSqlList() };

			read.Finish();

			return option;
		}
	}
}
