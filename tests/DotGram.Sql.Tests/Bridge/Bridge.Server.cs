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
