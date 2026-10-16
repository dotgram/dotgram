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
		/// T-SQL's Server family (the server and what runs beside the databases): writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutServer(ISqlNode node)
		{
			switch (node)
			{
				case Statement.Kill kill:
					Word("KILL");
					PutKillTarget(kill, kill.Target);

					if (kill.With is { } with)
					{
						Word("WITH");
						Word(with switch
						{
							KillOption.StatusOnly => "STATUSONLY",
							KillOption.Commit     => "COMMIT",
							KillOption.Rollback   => "ROLLBACK",
							_                     => throw NoText(with),
						});
					}

					return true;

				case Statement.KillStatsJob job:
					Word("KILL STATS JOB");
					PutKillNumber(job, job.Job);
					return true;

				case Statement.KillQueryNotificationSubscription subscription:
					if (subscription.All && subscription.Subscription is not null)
						throw Both(subscription, "All", "Subscription");

					if (!subscription.All && subscription.Subscription is null)
						throw Empty(subscription, "All", "Subscription");

					Word("KILL QUERY NOTIFICATION SUBSCRIPTION");

					if (subscription.Subscription is { } number)
						PutKillNumber(subscription, number);
					else
						Word("ALL");

					return true;

				case Statement.Reconfigure reconfigure:
					Word("RECONFIGURE");

					if (reconfigure.Override)
						Word("WITH OVERRIDE");

					return true;

				case Statement.Shutdown shutdown:
					Word("SHUTDOWN");

					if (shutdown.NoWait)
						Word("WITH NOWAIT");

					return true;

				case Statement.Dbcc dbcc:
					PutDbcc(dbcc);
					return true;

				case Statement.AlterServerConfiguration configuration:
					Word("ALTER SERVER CONFIGURATION SET");
					PutServerSetting(configuration.Setting);
					return true;

				case Statement.AlterResourceGovernor governor:
					Word("ALTER RESOURCE GOVERNOR");
					PutGovernorAction(governor.Action);

					if (governor.ReconfigureAfter)
						Word("RECONFIGURE");

					return true;

				case Statement.CreateResourcePool create:
					Word("CREATE RESOURCE POOL");
					PutPoolName(create.Name, "CREATE RESOURCE POOL");
					PutPoolOptions(create, create.Options, PoolSettings, "CREATE RESOURCE POOL");
					return true;

				case Statement.AlterResourcePool alter:
					Word("ALTER RESOURCE POOL");
					PutPoolName(alter.Name, "ALTER RESOURCE POOL");

					if (alter.Options.Count == 0)
						throw Empty(alter, "Options");

					PutPoolOptions(alter, alter.Options, PoolSettings, "ALTER RESOURCE POOL");
					return true;

				case Statement.CreateAvailabilityGroup create:
					PutCreateAvailabilityGroup(create);
					return true;

				case Statement.AlterAvailabilityGroup alter:
					Word("ALTER AVAILABILITY GROUP");
					PutOneName(alter.Name, "ALTER AVAILABILITY GROUP");
					PutAvailabilityAction(alter.Action);
					return true;

				case Statement.CreateEndpoint create:
					Word("CREATE ENDPOINT");
					PutOneName(create.Name, "CREATE ENDPOINT");
					PutAuthorization(create.Authorization, "CREATE ENDPOINT");
					PutEndpoint(create, create.Settings, create.Transport, create.Payload, create.TransportLast, created: true);
					return true;

				case Statement.AlterEndpoint alter:
					Word("ALTER ENDPOINT");
					PutOneName(alter.Name, "ALTER ENDPOINT");
					PutEndpoint(alter, alter.Settings, alter.Transport, alter.Payload, alter.TransportLast, created: false);
					return true;

				case Statement.CreateWorkloadGroup create:
					Word("CREATE WORKLOAD GROUP");
					PutPoolName(create.Name, "CREATE WORKLOAD GROUP");
					PutPoolOptions(create, create.Options, WorkloadSettings, "CREATE WORKLOAD GROUP");
					PutWorkloadPools(create, create.Pools);
					return true;

				case Statement.AlterWorkloadGroup alter:
					Word("ALTER WORKLOAD GROUP");
					PutPoolName(alter.Name, "ALTER WORKLOAD GROUP");
					PutPoolOptions(alter, alter.Options, WorkloadSettings, "ALTER WORKLOAD GROUP");
					PutWorkloadPools(alter, alter.Pools);
					return true;

				default:
					return false;
			}
		}

		// ── KILL ──────────────────────────────────────────────────────────────────

		/// <summary>
		/// The grammar's <c>KillNumber</c>: an unsigned numeric literal, which a minus may precede, kept as one token. A plus is
		/// none, and neither is a minus twice.
		/// </summary>
		void PutKillNumber(ISqlNode owner, LiteralValue.Numeric number)
		{
			var digits = number.Text.StartsWith("-", StringComparison.Ordinal) ? number.Text.Substring(1) : number.Text;

			if (!IsNumberOf(new LiteralValue.Numeric(digits, number.Kind), ExternalShape.Number))
				throw Misplaced(owner, "Number", "that is no number the grammar's KillNumber reads: an unsigned numeric literal, a minus before it at most");

			PutSignedNumber(owner, number);
		}

		/// <summary><c>KillSession</c>: a number, or a string (the unit of work), and nothing else.</summary>
		void PutKillTarget(ISqlNode owner, Expression target)
		{
			switch (target)
			{
				case LiteralValue.Numeric number:
					PutKillNumber(owner, number);
					break;

				case LiteralValue.String text when IsQuoted(text):
					PutExpression(text);
					break;

				default:
					throw Misplaced(owner, "Target", "other than a number or a string, in KILL");
			}
		}

		// ── DBCC ──────────────────────────────────────────────────────────────────

		void PutDbcc(Statement.Dbcc dbcc)
		{
			// DbccWord: a word of one part, never in brackets or quotes, a variable or a reserved word.
			if (dbcc.Command.Style != IdentifierStyle.Regular || dbcc.Command.Text.StartsWith("@", StringComparison.Ordinal))
				throw Misplaced(dbcc, "Command", "that is no plain word: not in brackets or quotes, not a variable");

			Word("DBCC");
			PutOneName(dbcc.Command, "DBCC");

			if (dbcc.Arguments is { } arguments)
			{
				Open();
				Each(arguments, one => PutDbccArgument(one));
				Close();
			}

			if (dbcc.Options.Count > 0)
			{
				Word("WITH");

				for (var at = 0; at < dbcc.Options.Count; at++)
				{
					if (at > 0)
						Tight(",");

					PutDbccOption(dbcc.Options[at], first: at == 0);
				}
			}
		}

		void PutDbccArgument(ExecuteArgument argument)
		{
			if (argument.Output is not null)
				throw Misplaced(argument, "Output", "in DBCC, whose arguments are not passed back");

			if (argument.Name is { } name)
			{
				if (name.Text.StartsWith("@", StringComparison.Ordinal))
					CheckVariable(name, "DBCC");
				else if (name.Style != IdentifierStyle.Regular)
					throw Misplaced(argument, "Name", "that is not a plain word, in DBCC (DbccWord)");

				if (name.Text.StartsWith("@", StringComparison.Ordinal))
					Word(name.Text);
				else
					PutOneName(name, "DBCC");

				Word("=");
			}

			PutDbccValue(argument, argument.Value);
		}

		/// <summary>
		/// The grammar's <c>DbccValue</c>: a number, a minus before it allowed, as one token; money; a string, plain or national; a
		/// binary string; NULL; a variable; or a name of one part that is not reserved. Not an expression, not DEFAULT, not <c>a.b</c>.
		/// </summary>
		void PutDbccValue(ISqlNode owner, Expression value)
		{
			switch (value)
			{
				case LiteralValue.Numeric { Kind: NumericLiteralKind.Money } money when IsMoney(money.Text):
					PutExpression(money);
					return;

				case LiteralValue.Numeric { Kind: not NumericLiteralKind.Money } number:
					PutKillNumber(owner, number);
					return;

				case LiteralValue.String text when IsQuoted(text):
				case LiteralValue.Binary { ZeroX: true }:
				case LiteralValue.Null:
					PutExpression(value);
					return;

				case Expression.Variable variable:
					CheckVariable(variable.Name, "DBCC");
					PutExpression(variable);
					return;

				case Expression.Reference { Name: [{ } word] }:
					PutOneName(word, "DBCC");
					return;
			}

			throw Misplaced(owner, "Value", "of a form the grammar's DbccValue does not read: an expression, DEFAULT, a name of two parts");
		}

		/// <summary>
		/// Money as the grammar's <c>MoneyLiteral</c> reads it: one of the currency signs it lists, a sign after it at most, and digits with
		/// a point that has digits before it.
		/// </summary>
		static bool IsMoney(string text)
		{
			return Regex.IsMatch(text, @"^[$¢-¥৲-৳฿៛₠-₱﷼﹩＄￠-￡￥-￦][+-]?[0-9]+(?:\.[0-9]*)?\z");
		}

		/// <summary>
		/// A word, <c>MAXDOP = n</c> (a number with no exponent), or <c>WAIT_AT_LOW_PRIORITY (MAX_DURATION = n [MINUTES],
		/// ABORT_AFTER_WAIT = word)</c>; the first of the options may be words joined by JOIN.
		/// </summary>
		void PutDbccOption(Option option, bool first)
		{
			if (option.EqualsOmitted || option.Order is not null || option.Partitions is not null || option.Place is not null)
				throw Misplaced(option, "EqualsOmitted, Order, Partitions or Place", "in DBCC, whose options are words");

			PutDbccWord(option.Name, option);

			if (option.Words.Count > 0)
			{
				if (!first)
					throw Misplaced(option, "Words", "after the first option, which alone reads words joined by JOIN");

				if (option.Words.Count % 2 != 0 || option.Value is not null || option.Options.Count > 0)
					throw Misplaced(option, "Words", "that are not words joined by JOIN, with no value");

				for (var at = 0; at < option.Words.Count; at += 2)
				{
					if (!Plain(option.Words[at], "JOIN"))
						throw Misplaced(option, "Words", "that are not joined by JOIN");

					PutKeyWord(option.Words[at]);
					PutDbccWord(option.Words[at + 1], option);
				}

				return;
			}

			if (Plain(option.Name, "MAXDOP") && option.Value is not null)
			{
				if (option.Options.Count > 0)
					throw Misplaced(option, "Options", "after MAXDOP, which reads none");

				Word("=");

				if (option.Value is not LiteralValue.Numeric number || !IsNumberOf(number, ExternalShape.Exact))
					throw Misplaced(option, "Value", "of MAXDOP that is not a number with no sign and no exponent");

				PutExpression(number);
				return;
			}

			if (option.Options.Count > 0)
			{
				// WAIT_AT_LOW_PRIORITY, with its bracket, each item once.
				if (!Plain(option.Name, "WAIT_AT_LOW_PRIORITY") || option.Value is not null)
					throw Misplaced(option, "Options", "on a word that has none: only WAIT_AT_LOW_PRIORITY is given a bracket");

				var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

				Open();

				for (var at = 0; at < option.Options.Count; at++)
				{
					var item = option.Options[at];

					if (item.Words.Count > 0 || item.Options.Count > 0 || item.EqualsOmitted || item.Order is not null || item.Partitions is not null || item.Place is not null || item.Value is not { } itemValue || !seen.Add(item.Name.Text))
						throw Misplaced(item, "a setting", "of WAIT_AT_LOW_PRIORITY that is not a name and a value, once");

					if (at > 0)
						Tight(",");

					if (Plain(item.Name, "MAX_DURATION"))
					{
						PutKeyWord(item.Name);
						Word("=");

						var amount = itemValue is Expression.Measured { Unit: var unit, Value: var inner } && Plain(unit, "MINUTES") ? inner : itemValue;

						if (amount is not LiteralValue.Numeric duration || !IsNumberOf(duration, ExternalShape.Number))
							throw Misplaced(item, "Value", "that is not a number in minutes");

						PutExpression(duration);

						if (itemValue is Expression.Measured measured)
							PutKeyWord(measured.Unit);
					}
					else if (Plain(item.Name, "ABORT_AFTER_WAIT"))
					{
						PutKeyWord(item.Name);
						Word("=");

						if (itemValue is not Expression.Reference { Name: [{ } word] })
							throw Misplaced(item, "Value", "that is not a word");

						PutDbccWord(word, item);
					}
					else
						throw Misplaced(item, "Name", "that WAIT_AT_LOW_PRIORITY does not read");
				}

				Close();
				return;
			}

			if (option.Value is not null)
				throw Misplaced(option, "Value", "on a word, which DBCC reads with a value only after MAXDOP");
		}

		/// <summary>A word the grammar's <c>DbccWord</c> reads: a plain name of one part, never a reserved word.</summary>
		void PutDbccWord(Identifier word, ISqlNode owner)
		{
			if (word.Style != IdentifierStyle.Regular || word.Text.StartsWith("@", StringComparison.Ordinal))
				throw Misplaced(owner, "a word", "that is not a plain word: DBCC reads no name in brackets or quotes, no variable");

			PutOneName(word, "DBCC");
		}

		// ── ALTER SERVER CONFIGURATION ────────────────────────────────────────────

		/// <summary>
		/// The shapes of a server setting's value, by the grammar's <c>ServerSetting</c>: processors or <c>AUTO</c>, nodes, a switch,
		/// a path or <c>DEFAULT</c>, a size in megabytes or <c>DEFAULT</c>, a count or <c>DEFAULT</c>, a string or <c>LOCAL</c>, a
		/// string or <c>DEFAULT</c> (a cluster's), a buffer pool extension, tempdb metadata, a snapshot backup, and the way the server
		/// proves itself outside.
		/// </summary>
		enum ServerShape
		{
			Affinity,
			Nodes,
			Switch,
			Path,
			Size,
			Count,
			Context,
			ClusterString,
			ClusterNumber,
			BufferPool,
			TempdbMetadata,
			SnapshotBackup,
			Authentication,
		}

		/// <summary>One setting a place reads: the shape of its value and whether the grammar writes <c>=</c> before it (<c>Sign</c>).</summary>
		readonly record struct ServerSetting(ServerShape Shape, bool Sign);

		/// <summary>
		/// ServerSetting, as one table: the words of a setting's name, upper-cased, and the value they are read with. A cluster's
		/// property is the name after <c>FAILOVER CLUSTER PROPERTY</c> and is in <see cref="ClusterProperties"/>.
		/// </summary>
		static readonly Dictionary<string, ServerSetting> ServerSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["PROCESS AFFINITY CPU"]             = new(ServerShape.Affinity, true),
			["PROCESS AFFINITY NUMANODE"]        = new(ServerShape.Nodes, true),
			["DIAGNOSTICS LOG"]                  = new(ServerShape.Switch, false),
			["DIAGNOSTICS LOG PATH"]             = new(ServerShape.Path, true),
			["DIAGNOSTICS LOG MAX_SIZE"]         = new(ServerShape.Size, true),
			["DIAGNOSTICS LOG MAX_FILES"]        = new(ServerShape.Count, true),
			["HADR CLUSTER CONTEXT"]             = new(ServerShape.Context, true),
			["BUFFER POOL EXTENSION"]            = new(ServerShape.BufferPool, false),
			["SOFTNUMA"]                         = new(ServerShape.Switch, false),
			["MEMORY_OPTIMIZED"]                 = new(ServerShape.Switch, false),
			["MEMORY_OPTIMIZED TEMPDB_METADATA"] = new(ServerShape.TempdbMetadata, true),
			["MEMORY_OPTIMIZED HYBRID_BUFFER_POOL"] = new(ServerShape.Switch, true),
			["SUSPEND_FOR_SNAPSHOT_BACKUP"]      = new(ServerShape.SnapshotBackup, true),
			["EXTERNAL AUTHENTICATION"]          = new(ServerShape.Authentication, false),
		};

		/// <summary>ClusterProperty: the properties of a cluster and the value each is set to, a string or a number.</summary>
		static readonly Dictionary<string, ServerShape> ClusterProperties = new(StringComparer.OrdinalIgnoreCase)
		{
			["SqlDumperDumpPath"]        = ServerShape.ClusterString,
			["ClusterConnectionOptions"] = ServerShape.ClusterString,
			["VerboseLogging"]           = ServerShape.ClusterNumber,
			["SqlDumperDumpFlags"]       = ServerShape.ClusterNumber,
			["SqlDumperDumpTimeOut"]     = ServerShape.ClusterNumber,
			["FailureConditionLevel"]    = ServerShape.ClusterNumber,
			["HealthCheckTimeout"]       = ServerShape.ClusterNumber,
		};

		void PutServerSetting(Option setting)
		{
			if (setting.EqualsOmitted || setting.Order is not null || setting.Partitions is not null || setting.Place is not null)
				throw Misplaced(setting, "EqualsOmitted, Order, Partitions or Place", "in ALTER SERVER CONFIGURATION, whose settings fix their form");

			var words = new List<Identifier> { setting.Name };

			words.AddRange(setting.Words);

			if (words.Any(static one => one.Style != IdentifierStyle.Regular || one.UnicodeEscape is not null))
				throw Misplaced(setting, "Name", "that holds a word that is not plain, in ALTER SERVER CONFIGURATION");

			var key = string.Join(" ", words.Select(static one => one.Text));
			ServerSetting entry;

			if (words is [{ } failover, { } cluster, { } property, { } named] && Plain(failover, "FAILOVER") && Plain(cluster, "CLUSTER") && Plain(property, "PROPERTY") && ClusterProperties.TryGetValue(named.Text, out var shape))
				entry = new ServerSetting(shape, true);
			else if (!ServerSettings.TryGetValue(key, out entry))
				throw Misplaced(setting, "Name", $"in ALTER SERVER CONFIGURATION, which reads no setting {key}");

			foreach (var word in words)
				PutKeyWord(word);

			if (entry.Sign)
				Word("=");

			var value = setting.Value ?? throw Empty(setting, "Value");
			var items = setting.Options;

			switch (entry.Shape)
			{
				case ServerShape.Affinity or ServerShape.Nodes when value is Expression.ServerRanges ranges:
					NoItems(setting, items);
					PutServerRanges(ranges);
					break;

				case ServerShape.Affinity when IsServerWord(value, "AUTO"):
					NoItems(setting, items);
					PutServerWord(value);
					break;

				case ServerShape.Switch when IsServerWord(value, "ON", "OFF"):
					NoItems(setting, items);
					PutServerWord(value);
					break;

				case ServerShape.Path or ServerShape.Context or ServerShape.ClusterString when value is LiteralValue.String path && IsQuoted(path):
					NoItems(setting, items);
					PutExpression(value);
					break;

				case ServerShape.Context when IsServerWord(value, "LOCAL"):
					NoItems(setting, items);
					PutServerWord(value);
					break;

				case ServerShape.Path or ServerShape.Size or ServerShape.Count or ServerShape.ClusterString or ServerShape.ClusterNumber when IsServerWord(value, "DEFAULT"):
					NoItems(setting, items);
					PutServerWord(value);
					break;

				case ServerShape.Size when value is Expression.Measured { Value: LiteralValue.Numeric amount, Unit: var unit } && Plain(unit, "MB") && IsNumberOf(amount, ExternalShape.Digits):
					NoItems(setting, items);
					PutExpression(amount);
					PutKeyWord(unit);
					break;

				case ServerShape.Count or ServerShape.ClusterNumber when value is LiteralValue.Numeric count && IsNumberOf(count, ExternalShape.Digits):
					NoItems(setting, items);
					PutExpression(count);
					break;

				case ServerShape.BufferPool:
					PutBufferPool(setting, value, items);
					break;

				case ServerShape.TempdbMetadata:
					PutTempdbMetadata(setting, value, items);
					break;

				case ServerShape.SnapshotBackup:
					PutSnapshotBackup(setting, value, items);
					break;

				case ServerShape.Authentication:
					PutAuthentication(setting, value, items);
					break;

				default:
					throw Misplaced(setting, "Value", $"of a form {key} is not read with in ALTER SERVER CONFIGURATION");
			}
		}

		static void NoItems(Option setting, SqlList<Option> items)
		{
			if (items.Count > 0)
				throw Misplaced(setting, "Options", "after a value that reads none, in ALTER SERVER CONFIGURATION");
		}

		static bool IsServerWord(Expression value, params string[] words)
		{
			return value is Expression.Reference { Name: [{ } word] } && Plain(word, words);
		}

		void PutServerWord(Expression value)
		{
			PutKeyWord(((Expression.Reference)value).Name[0]);
		}

		/// <summary>A value of a setting inside a bracket: the item's name and, where it has one, a value; nothing else.</summary>
		static void PlainItem(Option item, string place)
		{
			if (item.Words.Count > 0 || item.Options.Count > 0 || item.EqualsOmitted || item.Order is not null || item.Partitions is not null || item.Place is not null || item.Name.Style != IdentifierStyle.Regular)
				throw Misplaced(item, "an item", $"that is not a plain word and a value, in {place}");
		}

		/// <summary><c>ServerRanges</c>: whole numbers and ranges of them, one at least, joined by commas.</summary>
		void PutServerRanges(Expression.ServerRanges ranges)
		{
			if (ranges.Ranges.Count == 0)
				throw Empty(ranges, "Ranges");

			for (var at = 0; at < ranges.Ranges.Count; at++)
			{
				var range = ranges.Ranges[at];

				if (range.From is not LiteralValue.Numeric from || !IsNumberOf(from, ExternalShape.Digits) || range.To is not null and not LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger })
					throw Misplaced(range, "From or To", "that is no whole number, in a server's affinity (ServerRange)");

				if (range.To is LiteralValue.Numeric to && !IsNumberOf(to, ExternalShape.Digits))
					throw Misplaced(range, "To", "that is no whole number, in a server's affinity (ServerRange)");

				if (at > 0)
					Tight(",");

				PutExpression(from);

				if (range.To is { } end)
				{
					Word("TO");
					PutExpression(end);
				}
			}
		}

		/// <summary><c>BUFFER POOL EXTENSION</c>: OFF, or ON and its file and its size in brackets.</summary>
		void PutBufferPool(Option setting, Expression value, SqlList<Option> items)
		{
			if (IsServerWord(value, "OFF"))
			{
				NoItems(setting, items);
				PutServerWord(value);

				return;
			}

			if (!IsServerWord(value, "ON") || items is not [{ } file, { } size])
				throw Misplaced(setting, "Value", "of BUFFER POOL EXTENSION that is not OFF, or ON with its file and its size");

			PlainItem(file, "BUFFER POOL EXTENSION");
			PlainItem(size, "BUFFER POOL EXTENSION");

			if (!Plain(file.Name, "FILENAME") || file.Value is not LiteralValue.String path || !IsQuoted(path)
				|| !Plain(size.Name, "SIZE") || size.Value is not Expression.Measured { Value: LiteralValue.Numeric amount, Unit: var unit } || !Plain(unit, "KB", "MB", "GB") || !IsNumberOf(amount, ExternalShape.Digits))
			{
				throw Misplaced(setting, "Options", "of BUFFER POOL EXTENSION that are not FILENAME = string, SIZE = n KB, MB or GB");
			}

			PutServerWord(value);
			Open();
			PutKeyWord(file.Name);
			Word("=");
			PutExpression(path);
			Tight(",");
			PutKeyWord(size.Name);
			Word("=");
			PutExpression(amount);
			PutKeyWord(unit);
			Close();
		}

		/// <summary><c>MEMORY_OPTIMIZED TEMPDB_METADATA</c>: OFF, or ON and, in brackets, its resource pool.</summary>
		void PutTempdbMetadata(Option setting, Expression value, SqlList<Option> items)
		{
			if (IsServerWord(value, "OFF") || IsServerWord(value, "ON") && items.Count == 0)
			{
				NoItems(setting, items);
				PutServerWord(value);

				return;
			}

			if (!IsServerWord(value, "ON") || items is not [{ } pool])
				throw Misplaced(setting, "Options", "of TEMPDB_METADATA that are not a resource pool after ON");

			PlainItem(pool, "TEMPDB_METADATA");

			if (!Plain(pool.Name, "RESOURCE_POOL") || pool.Value is not LiteralValue.String name || !IsQuoted(name))
				throw Misplaced(pool, "an item", "that is not RESOURCE_POOL = string");

			PutServerWord(value);
			Open();
			PutKeyWord(pool.Name);
			Word("=");
			PutExpression(name);
			Close();
		}

		/// <summary>
		/// <c>SUSPEND_FOR_SNAPSHOT_BACKUP = ON | OFF</c> and, in brackets, the group of databases and the mode, or the mode alone
		/// (<c>SnapshotScope</c>).
		/// </summary>
		void PutSnapshotBackup(Option setting, Expression value, SqlList<Option> items)
		{
			if (!IsServerWord(value, "ON", "OFF"))
				throw Misplaced(setting, "Value", "of SUSPEND_FOR_SNAPSHOT_BACKUP that is not ON or OFF");

			PutServerWord(value);

			if (items.Count == 0)
				return;

			Option? group = null;
			Option? mode  = null;

			switch (items)
			{
				case [{ } only] when Plain(only.Name, "MODE"):
					mode = only;
					break;

				case [{ } first] when Plain(first.Name, "GROUP"):
					group = first;
					break;

				case [{ } first, { } second] when Plain(first.Name, "GROUP") && Plain(second.Name, "MODE"):
					group = first;
					mode  = second;
					break;

				default:
					throw Misplaced(setting, "Options", "of SUSPEND_FOR_SNAPSHOT_BACKUP that are not GROUP and MODE, or MODE alone");
			}

			Open();

			if (group is not null)
			{
				PlainItem(group, "SUSPEND_FOR_SNAPSHOT_BACKUP");

				if (group.Value is not Expression.Row { RowKeyword: false, Items: { Count: > 0 } databases } || databases.Any(static one => one is not Expression.Reference { Name.Count: 1 }))
					throw Misplaced(group, "Value", "of GROUP that is not a list of database names in brackets");

				PutKeyWord(group.Name);
				Word("=");
				Open();
				Each(databases, one => PutOneName(((Expression.Reference)one).Name[0], "SUSPEND_FOR_SNAPSHOT_BACKUP"));
				Close();

				if (mode is not null)
					Tight(",");
			}

			if (mode is not null)
			{
				PlainItem(mode, "SUSPEND_FOR_SNAPSHOT_BACKUP");

				if (!IsServerWord(mode.Value ?? new LiteralValue.Null(), "COPY_ONLY"))
					throw Misplaced(mode, "Value", "of MODE that is not COPY_ONLY");

				PutKeyWord(mode.Name);
				Word("=");
				PutServerWord(mode.Value!);
			}

			Close();
		}

		/// <summary><c>EXTERNAL AUTHENTICATION</c>: OFF, or ON with the one way in brackets: USE_IDENTITY, or CREDENTIAL_NAME = string.</summary>
		void PutAuthentication(Option setting, Expression value, SqlList<Option> items)
		{
			if (IsServerWord(value, "OFF"))
			{
				NoItems(setting, items);
				PutServerWord(value);

				return;
			}

			if (!IsServerWord(value, "ON") || items is not [{ } way])
				throw Misplaced(setting, "Value", "of EXTERNAL AUTHENTICATION that is not OFF, or ON with one way in brackets");

			PlainItem(way, "EXTERNAL AUTHENTICATION");

			PutServerWord(value);
			Open();

			if (Plain(way.Name, "USE_IDENTITY") && way.Value is null)
				PutKeyWord(way.Name);
			else if (Plain(way.Name, "CREDENTIAL_NAME") && way.Value is LiteralValue.String credential && IsQuoted(credential))
			{
				PutKeyWord(way.Name);
				Word("=");
				PutExpression(credential);
			}
			else
				throw Misplaced(way, "an item", "that is not USE_IDENTITY or CREDENTIAL_NAME = string");

			Close();
		}

		// ── The resource governor, resource pools and workload groups ─────────────

		void PutGovernorAction(ResourceGovernorAction action)
		{
			switch (action)
			{
				case ResourceGovernorAction.Reconfigure:
					Word("RECONFIGURE");
					break;

				case ResourceGovernorAction.Disable:
					Word("DISABLE");
					break;

				case ResourceGovernorAction.ResetStatistics:
					Word("RESET STATISTICS");
					break;

				case ResourceGovernorAction.Settings settings:
					PutGovernorSetting(settings, settings.Setting);
					break;

				default:
					throw Unwritten(action);
			}
		}

		/// <summary>
		/// <c>WITH (setting)</c>, GovernorSetting: <c>CLASSIFIER_FUNCTION</c> is NULL or a function named by two parts exactly;
		/// <c>MAX_OUTSTANDING_IO_PER_VOLUME</c> a number with no exponent, or DEFAULT.
		/// </summary>
		void PutGovernorSetting(ISqlNode owner, Option setting)
		{
			if (setting.Words.Count > 0 || setting.Options.Count > 0 || setting.EqualsOmitted || setting.Order is not null || setting.Partitions is not null || setting.Place is not null || setting.Name.Style != IdentifierStyle.Regular)
				throw Misplaced(setting, "Name", "that is not a plain name and a value, in ALTER RESOURCE GOVERNOR");

			var value = setting.Value ?? throw Empty(setting, "Value");

			Word("WITH");
			Open();

			if (Plain(setting.Name, "CLASSIFIER_FUNCTION"))
			{
				PutKeyWord(setting.Name);
				Word("=");

				switch (value)
				{
					case LiteralValue.Null:
						PutExpression(value);
						break;

					// Identifier '.' Identifier: two parts, none left out.
					case Expression.Reference { Name: [{ } schema, { } function] } when schema.Text.Length > 0 && function.Text.Length > 0:
						PutOneName(schema, "ALTER RESOURCE GOVERNOR");
						Tight(".");
						Hold();
						PutOneName(function, "ALTER RESOURCE GOVERNOR");
						break;

					default:
						throw Misplaced(setting, "Value", "that is not NULL or a function named by two parts, in CLASSIFIER_FUNCTION");
				}
			}
			else if (Plain(setting.Name, "MAX_OUTSTANDING_IO_PER_VOLUME"))
			{
				PutKeyWord(setting.Name);
				Word("=");

				if (IsServerWord(value, "DEFAULT"))
					PutServerWord(value);
				else if (value is LiteralValue.Numeric number && IsNumberOf(number, ExternalShape.Exact))
					PutExpression(number);
				else
					throw Misplaced(setting, "Value", "that is not a number with no exponent or DEFAULT, in MAX_OUTSTANDING_IO_PER_VOLUME");
			}
			else
				throw Misplaced(setting, "Name", "that ALTER RESOURCE GOVERNOR reads no setting of");

			Close();
		}

		/// <summary>
		/// A pool's or a group's name, <c>PoolName</c>: a name of one part, or a string in single quotes (an identifier of style
		/// String), plain only.
		/// </summary>
		void PutPoolName(Identifier name, string place)
		{
			if (name.Style == IdentifierStyle.String)
				Word("'" + name.Text.Replace("'", "''") + "'");
			else
				PutOneName(name, place);
		}

		/// <summary><c>WITH (setting, …)</c> of a pool or a group, or nothing for no setting.</summary>
		void PutPoolOptions(ISqlNode owner, SqlList<Option> options, Dictionary<string, SettingEntry> catalogue, string place)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Open();
			PutBrokerSettings(owner, options, catalogue, place, required: [], apart: []);
			Close();
		}

		/// <summary>PoolOption: the percentages and the I/O rates as whole numbers, the affinity to schedulers or to NUMA nodes.</summary>
		static readonly Dictionary<string, SettingEntry> PoolSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["MIN_CPU_PERCENT"]        = new(BrokerForm.Digits, Once: false),
			["MAX_CPU_PERCENT"]        = new(BrokerForm.Digits, Once: false),
			["CAP_CPU_PERCENT"]        = new(BrokerForm.Digits, Once: false),
			["MIN_MEMORY_PERCENT"]     = new(BrokerForm.Digits, Once: false),
			["MAX_MEMORY_PERCENT"]     = new(BrokerForm.Digits, Once: false),
			["MIN_IOPS_PER_VOLUME"]    = new(BrokerForm.Digits, Once: false),
			["MAX_IOPS_PER_VOLUME"]    = new(BrokerForm.Digits, Once: false),
			["TARGET_MEMORY_PERCENT"]  = new(BrokerForm.Digits, Once: false),
			["MIN_IO_PERCENT"]         = new(BrokerForm.Digits, Once: false),
			["MAX_IO_PERCENT"]         = new(BrokerForm.Digits, Once: false),
			["CAP_IO_PERCENT"]         = new(BrokerForm.Digits, Once: false),
			["AFFINITY SCHEDULER"]     = new(BrokerForm.Words | BrokerForm.Ranges, ["AUTO"], Once: false),
			["AFFINITY NUMANODE"]      = new(BrokerForm.Ranges, Once: false),
		};

		/// <summary>WorkloadOption: the importance, the grants and the limits, each its own kind of number, as often as wanted.</summary>
		static readonly Dictionary<string, SettingEntry> WorkloadSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["IMPORTANCE"]                          = new(BrokerForm.Words, ["LOW", "MEDIUM", "HIGH", "BELOW_NORMAL", "NORMAL", "ABOVE_NORMAL"], Once: false),
			["REQUEST_MAX_MEMORY_GRANT_PERCENT"]    = new(BrokerForm.Exact, Once: false),
			["REQUEST_MAX_CPU_TIME_SEC"]            = new(BrokerForm.Digits, Once: false),
			["REQUEST_MEMORY_GRANT_TIMEOUT_SEC"]    = new(BrokerForm.Digits, Once: false),
			["MAX_DOP"]                             = new(BrokerForm.Digits, Once: false),
			["GROUP_MAX_REQUESTS"]                  = new(BrokerForm.Digits, Once: false),
			["GROUP_MIN_MEMORY_PERCENT"]            = new(BrokerForm.Digits | BrokerForm.Null, Once: false),
			["GROUP_MAX_TEMPDB_DATA_MB"]            = new(BrokerForm.Exact | BrokerForm.Null, Once: false),
			["GROUP_MAX_TEMPDB_DATA_PERCENT"]       = new(BrokerForm.Exact | BrokerForm.Null, Once: false),
			["MIN_PERCENTAGE_RESOURCE"]             = new(BrokerForm.Exact, Once: false),
			["CAP_PERCENTAGE_RESOURCE"]             = new(BrokerForm.Exact, Once: false),
			["REQUEST_MIN_RESOURCE_GRANT_PERCENT"]  = new(BrokerForm.Exact, Once: false),
			["REQUEST_MAX_RESOURCE_GRANT_PERCENT"]  = new(BrokerForm.Exact, Once: false),
			["QUERY_EXECUTION_TIMEOUT_SEC"]         = new(BrokerForm.Digits, Once: false),
		};

		/// <summary><c>USING pool, EXTERNAL pool</c>: a pool is named, never quoted; none for no <c>USING</c>.</summary>
		void PutWorkloadPools(ISqlNode owner, SqlList<WorkloadPool> pools)
		{
			if (pools.Count == 0)
				return;

			Word("USING");

			for (var at = 0; at < pools.Count; at++)
			{
				if (at > 0)
					Tight(",");

				if (pools[at].External)
					Word("EXTERNAL");

				PutOneName(pools[at].Name, "a workload group's pool");
			}
		}

		// ── Availability groups ───────────────────────────────────────────────────

		/// <summary>AvailabilityWith: the settings of a group made, and the three words alone.</summary>
		static readonly Dictionary<string, SettingEntry> AvailabilityWith = new(StringComparer.OrdinalIgnoreCase)
		{
			["AUTOMATED_BACKUP_PREFERENCE"]                 = new(BrokerForm.Words, ["PRIMARY", "SECONDARY_ONLY", "SECONDARY", "NONE"], Once: false),
			["FAILURE_CONDITION_LEVEL"]                     = new(BrokerForm.Digits, Once: false),
			["HEALTH_CHECK_TIMEOUT"]                        = new(BrokerForm.Digits, Once: false),
			["DB_FAILOVER"]                                 = new(BrokerForm.OnOff, Once: false),
			["DTC_SUPPORT"]                                 = new(BrokerForm.Words, ["PER_DB", "NONE"], Once: false),
			["REQUIRED_SYNCHRONIZED_SECONDARIES_TO_COMMIT"] = new(BrokerForm.Digits, Once: false),
			["CLUSTER_TYPE"]                                = new(BrokerForm.Words, ["WSFC", "EXTERNAL", "NONE"], Once: false),
			["BASIC"]                                       = new(BrokerForm.Bare, Once: false),
			["DISTRIBUTED"]                                 = new(BrokerForm.Bare, Once: false),
			["CONTAINED"]                                   = new(BrokerForm.Bare, Once: false),
		};

		/// <summary>AvailabilitySetting: those, and the role of a replica.</summary>
		static readonly Dictionary<string, SettingEntry> AvailabilitySetting = new(AvailabilityWith, StringComparer.OrdinalIgnoreCase)
		{
			["ROLE"] = new(BrokerForm.Words, ["SECONDARY"], Once: false),
		};

		static readonly Dictionary<string, SettingEntry> SecondaryRole = new(StringComparer.OrdinalIgnoreCase)
		{
			["ALLOW_CONNECTIONS"]       = new(BrokerForm.Words, ["NO", "READ_ONLY", "ALL"], Once: false),
			["READ_ONLY_ROUTING_URL"]   = new(BrokerForm.String, Once: false),
		};

		static readonly Dictionary<string, SettingEntry> PrimaryRole = new(StringComparer.OrdinalIgnoreCase)
		{
			["ALLOW_CONNECTIONS"]       = new(BrokerForm.Words, ["READ_WRITE", "ALL"], Once: false),
			["READ_ONLY_ROUTING_LIST"]  = new(BrokerForm.Strings | BrokerForm.Words, ["NONE"], Once: false),
			["READ_WRITE_ROUTING_URL"]  = new(BrokerForm.String, Once: false),
		};

		/// <summary>AvailabilityReplicaOption: the endpoint, the modes, the numbers and the roles in brackets.</summary>
		static readonly Dictionary<string, SettingEntry> ReplicaSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["ENDPOINT_URL"]       = new(BrokerForm.String, Once: false),
			["AVAILABILITY_MODE"]  = new(BrokerForm.Words, ["SYNCHRONOUS_COMMIT", "ASYNCHRONOUS_COMMIT", "CONFIGURATION_ONLY"], Once: false),
			["FAILOVER_MODE"]      = new(BrokerForm.Words, ["AUTOMATIC", "MANUAL", "EXTERNAL"], Once: false),
			["SEEDING_MODE"]       = new(BrokerForm.Words, ["AUTOMATIC", "MANUAL"], Once: false),
			["BACKUP_PRIORITY"]    = new(BrokerForm.Digits, Once: false),
			["SESSION_TIMEOUT"]    = new(BrokerForm.Digits, Once: false),
			["SECONDARY_ROLE"]     = new(0, Once: false, Nested: SecondaryRole),
			["PRIMARY_ROLE"]       = new(0, Once: false, Nested: PrimaryRole),
		};

		/// <summary>AvailabilityGroupOption: where a group listens and how it takes part.</summary>
		static readonly Dictionary<string, SettingEntry> GroupSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["LISTENER_URL"]       = new(BrokerForm.String, Once: false),
			["LISTENER"]           = new(BrokerForm.String, Once: false),
			["AVAILABILITY_MODE"]  = new(BrokerForm.Words, ["SYNCHRONOUS_COMMIT", "ASYNCHRONOUS_COMMIT"], Once: false),
			["FAILOVER_MODE"]      = new(BrokerForm.Words, ["MANUAL"], Once: false),
			["SEEDING_MODE"]       = new(BrokerForm.Words, ["AUTOMATIC", "MANUAL"], Once: false),
		};

		void PutCreateAvailabilityGroup(Statement.CreateAvailabilityGroup create)
		{
			if ((create.ListenerName is null) != (create.Listener is null))
				throw Misplaced(create, create.ListenerName is null ? "Listener" : "ListenerName", "without the other, which are said together");

			Word("CREATE AVAILABILITY GROUP");
			PutOneName(create.Name, "CREATE AVAILABILITY GROUP");

			if (create.Options.Count > 0)
			{
				Word("WITH");
				Open();
				PutBrokerSettings(create, create.Options, AvailabilityWith, "CREATE AVAILABILITY GROUP", required: [], apart: []);
				Close();
			}

			switch (create.Source)
			{
				case AvailabilitySource.Replicas replicas:
					if (replicas.Servers.Count == 0)
						throw Empty(replicas, "Servers");

					Word("FOR");

					if (replicas.Databases.Count > 0)
					{
						Word("DATABASE");
						Each(replicas.Databases, one => PutOneName(one, "CREATE AVAILABILITY GROUP"));
					}

					Word("REPLICA ON");
					Each(replicas.Servers, PutReplica);
					break;

				case AvailabilitySource.Groups groups:
					Word("AVAILABILITY GROUP ON");
					PutGroupSpecs(groups, groups.Specs);
					break;

				default:
					throw Unwritten(create.Source);
			}

			if (create.ListenerName is { } name && create.Listener is { } listener)
			{
				Word("LISTENER");
				PutBrokerString(create, "ListenerName", name);
				Open();
				PutAvailabilityListener(listener);
				Close();
			}
		}

		void PutBrokerString(ISqlNode owner, string member, LiteralValue.String text)
		{
			if (!IsQuoted(text))
				throw Misplaced(owner, member, "that is no string the grammar's BrokerString reads");

			PutExpression(text);
		}

		/// <summary><c>'server' WITH (setting, …)</c>: the settings of a replica, one at least.</summary>
		void PutReplica(AvailabilityReplica replica)
		{
			PutBrokerString(replica, "Server", replica.Server);
			Word("WITH");
			Open();
			PutBrokerSettings(replica, replica.Options, ReplicaSettings, "an availability replica", required: [], apart: []);
			Close();
		}

		/// <summary>The groups a distributed group is made of or joined to: one, or two joined by a comma.</summary>
		void PutGroupSpecs(ISqlNode owner, SqlList<AvailabilityGroupSpec> specs)
		{
			if (specs.Count is < 1 or > 2)
				throw Misplaced(owner, "Specs", $"{specs.Count} in number, where the grammar reads one or two");

			for (var at = 0; at < specs.Count; at++)
			{
				if (at > 0)
					Tight(",");

				PutBrokerString(specs[at], "Group", specs[at].Group);
				Word("WITH");
				Open();
				PutBrokerSettings(specs[at], specs[at].Options, GroupSettings, "an availability group", required: [], apart: []);
				Close();
			}
		}

		void PutListenerAddress(ListenerAddress address, bool maskRequired)
		{
			if (maskRequired && address.Mask is null)
				throw Empty(address, "Mask");

			Open();
			PutBrokerString(address, "Address", address.Address);

			if (address.Mask is { } mask)
			{
				Tight(",");
				PutBrokerString(address, "Mask", mask);
			}

			Close();
		}

		void PutAvailabilityListener(AvailabilityListener listener)
		{
			switch (listener)
			{
				case AvailabilityListener.Dhcp dhcp:
					Word("WITH DHCP");

					if (dhcp.On is { } on)
					{
						Word("ON");
						PutListenerAddress(on, maskRequired: true);
					}

					break;

				case AvailabilityListener.Ip ip:
					if (ip.Addresses.Count == 0)
						throw Empty(ip, "Addresses");

					Word("WITH IP");
					Open();
					Each(ip.Addresses, one => PutListenerAddress(one, maskRequired: false));
					Close();

					if (ip.Port is { } port)
					{
						Tight(",");
						Word("PORT =");
						PutListenerPort(ip, port);
					}

					break;

				default:
					throw Unwritten(listener);
			}
		}

		void PutListenerPort(ISqlNode owner, LiteralValue.Numeric port)
		{
			if (!IsNumberOf(port, ExternalShape.Digits))
				throw Misplaced(owner, "Port", "that is no whole number the grammar's Digits reads");

			PutExpression(port);
		}

		void PutAvailabilityAction(AvailabilityAction action)
		{
			switch (action)
			{
				case AvailabilityAction.Set set:
					Word("SET");
					Open();

					// One setting, in brackets.
					PutBrokerSettings(set, [set.Setting], AvailabilitySetting, "ALTER AVAILABILITY GROUP", required: [], apart: []);
					Close();
					break;

				case AvailabilityAction.AddDatabase add:
					Word("ADD DATABASE");
					PutOneName(add.Database, "ALTER AVAILABILITY GROUP");
					break;

				case AvailabilityAction.RemoveDatabase remove:
					Word("REMOVE DATABASE");
					PutOneName(remove.Database, "ALTER AVAILABILITY GROUP");
					break;

				case AvailabilityAction.AddReplica add:
					Word("ADD REPLICA ON");
					PutReplica(add.Replica);
					break;

				case AvailabilityAction.ModifyReplica modify:
					Word("MODIFY REPLICA ON");
					PutReplica(modify.Replica);
					break;

				case AvailabilityAction.RemoveReplica remove:
					Word("REMOVE REPLICA ON");
					PutBrokerString(remove, "Server", remove.Server);
					break;

				case AvailabilityAction.JoinAvailabilityGroup join:
					Word("JOIN AVAILABILITY GROUP ON");
					PutGroupSpecs(join, join.Specs);
					break;

				case AvailabilityAction.ModifyAvailabilityGroup modify:
					Word("MODIFY AVAILABILITY GROUP ON");
					PutGroupSpecs(modify, modify.Specs);
					break;

				case AvailabilityAction.Join:
					Word("JOIN");
					break;

				case AvailabilityAction.CreateAnyDatabase permission:
					Word(permission.Permission switch
					{
						AvailabilityPermission.Grant => "GRANT",
						AvailabilityPermission.Deny  => "DENY",
						_                            => throw NoText(permission.Permission),
					});

					Word("CREATE ANY DATABASE");
					break;

				case AvailabilityAction.Failover:
					Word("FAILOVER");
					break;

				case AvailabilityAction.ForceFailoverAllowDataLoss:
					Word("FORCE_FAILOVER_ALLOW_DATA_LOSS");
					break;

				case AvailabilityAction.Offline:
					Word("OFFLINE");
					break;

				case AvailabilityAction.AddListener add:
					Word("ADD LISTENER");
					PutBrokerString(add, "Name", add.Name);
					Open();
					PutAvailabilityListener(add.Listener);
					Close();
					break;

				case AvailabilityAction.ModifyListener modify:
					Word("MODIFY LISTENER");
					PutBrokerString(modify, "Name", modify.Name);
					Open();

					switch (modify.Change)
					{
						case ListenerChange.AddIp add:
							Word("ADD IP");
							PutListenerAddress(add.Address, maskRequired: false);
							break;

						case ListenerChange.Port port:
							Word("PORT =");
							PutListenerPort(port, port.Number);
							break;

						default:
							throw Unwritten(modify.Change);
					}

					Close();
					break;

				case AvailabilityAction.RestartListener restart:
					Word("RESTART LISTENER");
					PutBrokerString(restart, "Name", restart.Name);
					break;

				case AvailabilityAction.RemoveListener remove:
					Word("REMOVE LISTENER");
					PutBrokerString(remove, "Name", remove.Name);
					break;

				default:
					throw Unwritten(action);
			}
		}

		// ── Endpoints ─────────────────────────────────────────────────────────────

		/// <summary>EndpointState and EndpointSetting: the state and the affinity, in any order and as often as wanted.</summary>
		static readonly Dictionary<string, SettingEntry> EndpointSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["STATE"]    = new(BrokerForm.Words, ["STARTED", "STOPPED", "DISABLED"], Once: false),
			["AFFINITY"] = new(BrokerForm.Words, ["NONE", "ADMIN"], Once: false),
		};

		/// <summary>EndpointTcpOption: the port, which a minus may precede (7810 is the engine's), and the address.</summary>
		static readonly Dictionary<string, SettingEntry> TcpSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["LISTENER_PORT"] = new(BrokerForm.SignedDigits, Once: false),
			["LISTENER_IP"]   = new(BrokerForm.Words | BrokerForm.Strings | BrokerForm.Address, ["ALL"], Once: false),
		};

		/// <summary>EndpointHttpSyntax: SQL Server 2005's HTTP endpoint.</summary>
		static readonly Dictionary<string, SettingEntry> HttpSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["PATH"]                 = new(BrokerForm.String, Once: false),
			["SITE"]                 = new(BrokerForm.String, Once: false),
			["AUTH_REALM"]           = new(BrokerForm.String | BrokerForm.Words, ["NONE"], Once: false),
			["DEFAULT_LOGON_DOMAIN"] = new(BrokerForm.String | BrokerForm.Words, ["NONE"], Once: false),
			["AUTHENTICATION"]       = new(BrokerForm.WordList, ["BASIC", "DIGEST", "NTLM", "KERBEROS", "INTEGRATED"], Once: false),
			["PORTS"]                = new(BrokerForm.WordList, ["CLEAR", "SSL"], Once: false),
			["CLEAR_PORT"]           = new(BrokerForm.SignedDigits, Once: false),
			["SSL_PORT"]             = new(BrokerForm.SignedDigits, Once: false),
			["COMPRESSION"]          = new(BrokerForm.Words, ["ENABLED", "DISABLED"], Once: false),
		};

		/// <summary>EndpointTsqlOptions: an encryption and nothing else.</summary>
		static readonly Dictionary<string, SettingEntry> TsqlSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["ENCRYPTION"] = new(BrokerForm.Encryption, Once: false),
		};

		/// <summary>EndpointBrokerOption: the encryption and the authentication, and the forwarding.</summary>
		static readonly Dictionary<string, SettingEntry> ServiceBrokerSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["ENCRYPTION"]            = new(BrokerForm.Encryption, Once: false),
			["AUTHENTICATION"]        = new(BrokerForm.Authentication, Once: false),
			["MESSAGE_FORWARDING"]    = new(BrokerForm.Words, ["ENABLED", "DISABLED"], Once: false),
			["MESSAGE_FORWARD_SIZE"]  = new(BrokerForm.SignedDigits, Once: false),
		};

		/// <summary>EndpointMirroringOption: the encryption and the authentication, and the role.</summary>
		static readonly Dictionary<string, SettingEntry> MirroringSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["ENCRYPTION"]     = new(BrokerForm.Encryption, Once: false),
			["AUTHENTICATION"] = new(BrokerForm.Authentication, Once: false),
			["ROLE"]           = new(BrokerForm.Words, ["WITNESS", "PARTNER", "ALL"], Once: false),
		};

		/// <summary>EndpointSoapSetting: the settings of a SOAP endpoint, each its own kind of value, as often as wanted.</summary>
		static readonly Dictionary<string, SettingEntry> SoapSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["WSDL"]            = new(BrokerForm.Words | BrokerForm.String, ["DEFAULT", "NONE"], Once: false),
			["BATCHES"]         = new(BrokerForm.Words, ["ENABLED", "DISABLED"], Once: false),
			["SESSIONS"]        = new(BrokerForm.Words, ["ENABLED", "DISABLED"], Once: false),
			["LOGIN_TYPE"]      = new(BrokerForm.Words, ["WINDOWS", "MIXED"], Once: false),
			["SESSION_TIMEOUT"] = new(BrokerForm.Digits | BrokerForm.Words, ["NEVER"], Once: false),
			["DATABASE"]        = new(BrokerForm.Words | BrokerForm.String, ["DEFAULT"], Once: false),
			["NAMESPACE"]       = new(BrokerForm.Words | BrokerForm.String, ["DEFAULT"], Once: false),
			["SCHEMA"]          = new(BrokerForm.Words, ["NONE", "STANDARD"], Once: false),
			["CHARACTER_SET"]   = new(BrokerForm.Words, ["SQL", "XML"], Once: false),
			["HEADER_LIMIT"]    = new(BrokerForm.Digits, Once: false),
		};

		/// <summary>WebMethodOption: the object a method is, its schema and its format.</summary>
		static readonly Dictionary<string, SettingEntry> WebMethodSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["NAME"]   = new(BrokerForm.String, Once: false),
			["SCHEMA"] = new(BrokerForm.Words, ["NONE", "STANDARD", "DEFAULT"], Once: false),
			["FORMAT"] = new(BrokerForm.Words, ["NONE", "ALL_RESULTS", "ROWSETS_ONLY"], Once: false),
		};

		void PutEndpoint(ISqlNode owner, SqlList<Option> settings, EndpointTransport? transport, EndpointPayload? payload, bool transportLast, bool created)
		{
			if (transportLast && (transport is null || payload is null))
				throw Misplaced(owner, "TransportLast", "without both a transport and a payload, which it orders");

			if (settings.Count > 0)
				PutBrokerSettings(owner, settings, EndpointSettings, "an endpoint", required: [], apart: []);

			if (!transportLast)
				PutTransport(transport);

			PutPayload(payload, created);

			if (transportLast)
				PutTransport(transport);
		}

		void PutTransport(EndpointTransport? transport)
		{
			switch (transport)
			{
				case null:
					break;

				case EndpointTransport.Tcp tcp:
					Word("AS TCP");
					Open();
					PutBrokerSettings(tcp, tcp.Options, TcpSettings, "AS TCP", required: [], apart: []);
					Close();
					break;

				case EndpointTransport.Http http:
					Word("AS HTTP");
					Open();
					PutBrokerSettings(http, http.Options, HttpSettings, "AS HTTP", required: [], apart: []);
					Close();
					break;

				default:
					throw Unwritten(transport);
			}
		}

		void PutPayload(EndpointPayload? payload, bool created)
		{
			switch (payload)
			{
				case null:
					break;

				case EndpointPayload.Tsql tsql:
					Word("FOR TSQL");
					PutPayloadSettings(tsql, tsql.Options, TsqlSettings, "FOR TSQL");
					break;

				case EndpointPayload.ServiceBroker broker:
					Word("FOR SERVICE_BROKER");
					PutPayloadSettings(broker, broker.Options, ServiceBrokerSettings, "FOR SERVICE_BROKER");
					break;

				case EndpointPayload.DatabaseMirroring mirroring:
					Word(mirroring.DataMirroring ? "FOR DATA_MIRRORING" : "FOR DATABASE_MIRRORING");
					PutPayloadSettings(mirroring, mirroring.Options, MirroringSettings, "FOR DATABASE_MIRRORING");
					break;

				case EndpointPayload.Soap soap:
					Word("FOR SOAP");
					Open();

					for (var at = 0; at < soap.Items.Count; at++)
					{
						if (at > 0)
							Tight(",");

						PutSoapItem(soap.Items[at], created);
					}

					Close();
					break;

				default:
					throw Unwritten(payload);
			}
		}

		/// <summary>The brackets of a payload, always written, and its settings if it has any.</summary>
		void PutPayloadSettings(ISqlNode owner, SqlList<Option> options, Dictionary<string, SettingEntry> catalogue, string place)
		{
			Open();

			if (options.Count > 0)
				PutBrokerSettings(owner, options, catalogue, place, required: [], apart: []);

			Close();
		}

		void PutSoapItem(SoapItem item, bool created)
		{
			switch (item)
			{
				case SoapItem.Setting setting:
					PutBrokerSettings(setting, [setting.Value], SoapSettings, "FOR SOAP", required: [], apart: []);
					break;

				case SoapItem.WebMethod method:
					if (method.Action is null && !created)
						throw Misplaced(method, "Action", "that is none, in ALTER ENDPOINT, which writes a web method added, altered or dropped (Msg 7803)");

					if (method.Action is WebMethodAction.Drop)
					{
						if (method.Options.Count > 0)
							throw Misplaced(method, "Options", "after DROP WEBMETHOD, which takes nothing further");
					}
					else if (method.Options.Count == 0)
						throw Empty(method, "Options");

					switch (method.Action)
					{
						case null:
							break;

						case WebMethodAction.Add:
							Word("ADD");
							break;

						case WebMethodAction.Alter:
							Word("ALTER");
							break;

						case WebMethodAction.Drop:
							Word("DROP");
							break;

						default:
							throw NoText(method.Action.Value);
					}

					Word("WEBMETHOD");

					if (method.Namespace is { } ns)
					{
						PutBrokerString(method, "Namespace", ns);
						Tight(".");
						Hold();
					}

					PutBrokerString(method, "Method", method.Method);

					if (method.Options.Count > 0)
					{
						Open();
						PutBrokerSettings(method, method.Options, WebMethodSettings, "WEBMETHOD", required: [], apart: []);
						Close();
					}

					break;

				default:
					throw Unwritten(item);
			}
		}

		/// <summary>
		/// EndpointEncryptionSyntax: DISABLED, NEGOTIATED or STRICT alone; SUPPORTED or REQUIRED, and then, if it is told one,
		/// ALGORITHM and AES, RC4, AES RC4 or RC4 AES. The words are written as they are, in the case they are in.
		/// </summary>
		void PutEndpointEncryption(ISqlNode owner, Expression.EndpointEncryption encryption)
		{
			var words = encryption.Words.Select(static one => one.Text.ToUpperInvariant()).ToArray();

			var read = words is ["DISABLED" or "NEGOTIATED" or "STRICT"]
				|| words is ["SUPPORTED" or "REQUIRED"]
				|| words is ["SUPPORTED" or "REQUIRED", "ALGORITHM", "AES" or "RC4"]
				|| words is ["SUPPORTED" or "REQUIRED", "ALGORITHM", "AES", "RC4"]
				|| words is ["SUPPORTED" or "REQUIRED", "ALGORITHM", "RC4", "AES"];

			if (!read || encryption.Words.Any(static one => one.Style != IdentifierStyle.Regular || one.UnicodeEscape is not null))
				throw Misplaced(encryption, "Words", "that are no encryption the grammar's EndpointEncryptionSyntax reads");

			foreach (var word in encryption.Words)
				PutKeyWord(word);
		}

		/// <summary>
		/// EndpointAuthenticationSyntax: WINDOWS, a protocol after it if it is told one, and CERTIFICATE and its name, in either order,
		/// each once, one at least. The words are written as they are; the name after CERTIFICATE is an identifier.
		/// </summary>
		void PutEndpointAuthentication(ISqlNode owner, Expression.EndpointAuthentication authentication)
		{
			var words       = authentication.Words;
			var certificate = -1;
			var windows     = false;
			var at          = 0;
			var read        = words.Count > 0;

			while (read && at < words.Count)
			{
				if (!windows && Plain(words[at], "WINDOWS"))
				{
					windows = true;
					at++;

					if (at < words.Count && Plain(words[at], "NTLM", "KERBEROS", "NEGOTIATE"))
						at++;
				}
				else if (certificate < 0 && Plain(words[at], "CERTIFICATE") && at + 1 < words.Count)
				{
					certificate = at + 1;
					at += 2;
				}
				else
					read = false;
			}

			if (!read)
				throw Misplaced(authentication, "Words", "that are no authentication the grammar's EndpointAuthenticationSyntax reads");

			for (var each = 0; each < words.Count; each++)
			{
				if (each == certificate)
					PutOneName(words[each], "an endpoint's authentication");
				else if (words[each].Style != IdentifierStyle.Regular || words[each].UnicodeEscape is not null)
					throw Misplaced(authentication, "Words", "that holds a word that is not plain");
				else
					PutKeyWord(words[each]);
			}
		}
	}
}
