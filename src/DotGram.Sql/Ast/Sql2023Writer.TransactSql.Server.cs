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
			var digits = number.Text.StartsWith('-') ? number.Text.Substring(1) : number.Text;

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
			if (dbcc.Command.Style != IdentifierStyle.Regular || dbcc.Command.Text.StartsWith('@'))
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
				if (name.Text.StartsWith('@'))
					CheckVariable(name, "DBCC");
				else if (name.Style != IdentifierStyle.Regular)
					throw Misplaced(argument, "Name", "that is not a plain word, in DBCC (DbccWord)");

				if (name.Text.StartsWith('@'))
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
			return Regex.IsMatch(text, @"^[$¢-¥৲-৳฿៛₠-₱﷼﹩＄￠-￡￥-￦][+-]?[0-9]+(?:\.[0-9]*)?$");
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

					if (item.Words.Count > 0 || item.Options.Count > 0 || item.EqualsOmitted || item.Value is not { } itemValue || !seen.Add(item.Name.Text))
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
			if (word.Style != IdentifierStyle.Regular || word.Text.StartsWith('@'))
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

			var key = string.Join(' ', words.Select(static one => one.Text));
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
	}
}
