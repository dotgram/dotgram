using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Database family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutDatabase(ISqlNode node)
		{
			switch (node)
			{
				// T-SQL: ALTER DATABASE (Transact-SQL). A name or CURRENT, exactly one (DatabaseTarget), and one action.
				case Statement.AlterDatabase alter:
					Word("ALTER DATABASE");
					PutDatabaseTarget(alter, alter.Name, alter.Current);
					PutDatabaseAction(alter.Action);
					return true;

				// T-SQL: ALTER DATABASE SCOPED CONFIGURATION. FOR SECONDARY only before SET (AlterDatabaseStatement).
				case Statement.AlterDatabaseScopedConfiguration scoped:
					Word("ALTER DATABASE SCOPED CONFIGURATION");
					PutScopedAction(scoped.Action);
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// T-SQL: ALTER DATABASE (Transact-SQL). DatabaseTarget: <c>CURRENT</c>, or a name, never both; a plain name CURRENT
		/// reads back as the word.
		/// </summary>
		void PutDatabaseTarget(ISqlNode owner, Identifier? name, bool current)
		{
			if (current == name is not null)
				throw current ? Both(owner, "Name", "Current") : Empty(owner, "Name", "Current");

			if (name is { } named)
			{
				if (Plain(named, "CURRENT"))
					throw Misplaced(owner, "Name", "CURRENT unbracketed, which reads as the word (DatabaseTarget)");

				PutIdentifier(named);
			}
			else
				Word("CURRENT");
		}

		/// <summary>T-SQL: ALTER DATABASE (Transact-SQL), ALTER DATABASE File and Filegroups. AlterDatabaseAction.</summary>
		void PutDatabaseAction(AlterDatabaseAction action)
		{
			switch (action)
			{
				case AlterDatabaseAction.Collate collate:
					Word("COLLATE");
					PutCollation(collate.Collation);
					break;

				case AlterDatabaseAction.ModifyName rename:
					Word("MODIFY NAME =");
					PutIdentifier(rename.Name);
					break;

				// T-SQL: ALTER DATABASE (Transact-SQL), the Azure syntax. EditionOptions in brackets, one at least.
				case AlterDatabaseAction.Modify modify:
					if (modify.Options.Count == 0)
						throw Empty(modify, "Options");

					Word("MODIFY");
					Open();
					Each(modify.Options, option =>
					{
						// SERVICE_OBJECTIVE = ELASTIC_POOL (NAME = pool): the one value of the list with options of its own.
						if (option is { Name: var named, Value: Expression.Reference { Name: [{ } pool] }, Options: [{ Name: var inner, Value: var value, Options.Count: 0, Words.Count: 0, EqualsOmitted: false }] }
							&& Plain(named, "SERVICE_OBJECTIVE") && Plain(pool, "ELASTIC_POOL") && Plain(inner, "NAME") && NameValue(value)
							&& option is { Words.Count: 0, EqualsOmitted: false, Partitions: null, Order: null })
							PutOption(option);
						else
							PutCatalogued(option, EditionOptions, "EditionOption");
					});
					Close();

					if (modify.ManualCutover)
						Word("WITH MANUAL_CUTOVER");

					break;

				case AlterDatabaseAction.ModifyBackupStorageRedundancy redundancy:
					if (!StringValue(redundancy.Value))
						throw Misplaced(redundancy, "Value", "other than a string (Quoted)");

					Word("MODIFY BACKUP_STORAGE_REDUNDANCY =");
					PutExpression(redundancy.Value);
					break;

				case AlterDatabaseAction.PerformCutover:
					Word("PERFORM_CUTOVER");
					break;

				case AlterDatabaseAction.RebuildLog:
					Word("REBUILD LOG");
					break;

				case AlterDatabaseAction.AddFile add:
					Word("ADD FILE");
					PutDatabaseFiles(add, add.Files);

					if (add.ToFileGroup is { } group)
					{
						Word("TO FILEGROUP");
						PutIdentifier(group);
					}

					break;

				case AlterDatabaseAction.AddLogFile add:
					Word("ADD LOG FILE");
					PutDatabaseFiles(add, add.Files);
					break;

				case AlterDatabaseAction.ModifyFile modify:
					Word("MODIFY FILE");
					PutDatabaseFile(modify.File);
					break;

				case AlterDatabaseAction.RemoveFile remove:
					Word("REMOVE FILE");
					PutIdentifier(remove.Name);
					break;

				case AlterDatabaseAction.AddFileGroup add:
					Word("ADD FILEGROUP");
					PutIdentifier(add.Name);

					if (add.Contains is { } contains)
						Word(contains switch
						{
							FileGroupContents.Filestream          => "CONTAINS FILESTREAM",
							FileGroupContents.MemoryOptimizedData => "CONTAINS MEMORY_OPTIMIZED_DATA",
							_                                     => throw NoText(contains),
						});

					break;

				case AlterDatabaseAction.ModifyFileGroup modify:
					Word("MODIFY FILEGROUP");
					PutIdentifier(modify.Name);

					switch (modify.Change)
					{
						case FileGroupChange.Updatability updatability:
							Word((updatability.Kind, updatability.Underscored) switch
							{
								(FileGroupUpdatability.ReadOnly, false)  => "READONLY",
								(FileGroupUpdatability.ReadOnly, true)   => "READ_ONLY",
								(FileGroupUpdatability.ReadWrite, false) => "READWRITE",
								(FileGroupUpdatability.ReadWrite, true)  => "READ_WRITE",
								_                                        => throw NoText(updatability.Kind),
							});
							break;

						case FileGroupChange.Default:
							Word("DEFAULT");
							break;

						case FileGroupChange.Rename rename:
							Word("NAME =");
							PutIdentifier(rename.Name);
							break;

						case FileGroupChange.AutoGrow grow:
							Word(grow.Kind switch
							{
								FileGroupAutoGrow.SingleFile => "AUTOGROW_SINGLE_FILE",
								FileGroupAutoGrow.AllFiles   => "AUTOGROW_ALL_FILES",
								_                            => throw NoText(grow.Kind),
							});
							break;

						default:
							throw Unwritten(modify.Change);
					}

					break;

				case AlterDatabaseAction.RemoveFileGroup remove:
					Word("REMOVE FILEGROUP");
					PutIdentifier(remove.Name);
					break;

				// T-SQL: ALTER DATABASE SET Options (Transact-SQL). One setting at least (DatabaseSetting), then the
				// termination.
				case AlterDatabaseAction.Set set:
					if (set.Options.Count == 0)
						throw Empty(set, "Options");

					Word("SET");
					Each(set.Options, option => PutSetting(option, DatabaseSettings, "DatabaseSetting"));
					PutTermination(set.Termination);
					break;

				default:
					throw Unwritten(action);
			}
		}

		/// <summary>
		/// T-SQL: ALTER DATABASE SET Options (Transact-SQL). DatabaseTermination: whole seconds, <c>SECONDS</c> written or
		/// not, <c>IMMEDIATE</c>, or <c>NO_WAIT</c>.
		/// </summary>
		void PutTermination(Termination? termination)
		{
			switch (termination)
			{
				case null:
					return;

				case Termination.RollbackAfter after:
					if (after.Number is not { Kind: NumericLiteralKind.DecimalInteger, Text: var digits } || !Unsigned(digits, NumericLiteralKind.DecimalInteger))
						throw Misplaced(after, "Number", "other than digits (DatabaseTermination: Sql92.Lexical.Digits)");

					Word("WITH ROLLBACK AFTER");
					Word(digits);

					if (after.Seconds)
						Word("SECONDS");

					return;

				case Termination.RollbackImmediate:
					Word("WITH ROLLBACK IMMEDIATE");
					return;

				case Termination.NoWait:
					Word("WITH NO_WAIT");
					return;

				default:
					throw Unwritten(termination);
			}
		}

		/// <summary>T-SQL: ALTER DATABASE SCOPED CONFIGURATION. ScopedConfiguration and ScopedSet.</summary>
		void PutScopedAction(ScopedConfigurationAction action)
		{
			switch (action)
			{
				case ScopedConfigurationAction.Set set:
					if (set.Secondary)
						Word("FOR SECONDARY");

					Word("SET");
					PutSetting(set.Option, ScopedSettings, "ScopedSetting");
					break;

				// PlanHandle: written as binary and as nothing else.
				case ScopedConfigurationAction.ClearProcedureCache clear:
					Word("CLEAR PROCEDURE_CACHE");

					if (clear.PlanHandle is { } handle)
					{
						if (!handle.ZeroX)
							throw Misplaced(clear, "PlanHandle", "written X'…' (PlanHandle: TSql.BinaryLiteral)");

						PutExpression(handle);
					}

					break;

				default:
					throw Unwritten(action);
			}
		}

		void PutDatabaseFiles(ISqlNode owner, SqlList<DatabaseFile> files)
		{
			if (files.Count == 0)
				throw Empty(owner, "Files");

			Each(files, PutDatabaseFile);
		}

		/// <summary>T-SQL: ALTER DATABASE File and Filegroups. FileSpec: <c>(option, …)</c>, one at least, from FileOptions.</summary>
		void PutDatabaseFile(DatabaseFile file)
		{
			if (file.Options.Count == 0)
				throw Empty(file, "Options");

			Open();
			Each(file.Options, option => PutCatalogued(option, FileOptions, "FileOptionSyntax"));
			Close();
		}

		/// <summary>
		/// One option of a place whose catalogue fixes its names and the form of each value: a name of one word, with
		/// <c>=</c> and a value of the form it takes, or alone where it takes none; nothing nested.
		/// </summary>
		void PutCatalogued(Option option, Dictionary<string, Func<Expression?, bool>> catalogue, string rule)
		{
			if (option.Name.Style != IdentifierStyle.Regular || !catalogue.TryGetValue(option.Name.Text, out var takes)
				|| option.Words.Count > 0 || option.EqualsOmitted || option.Options.Count > 0 || option.Partitions is not null || option.Order is not null
				|| !takes(option.Value))
				throw Misplaced(option, "Name or Value", "of a name or a value form " + rule + " does not read");

			PutIdentifier(option.Name);

			if (option.Value is { } value)
			{
				Word("=");
				PutExpression(value);
			}
		}

		/// <summary>T-SQL: ALTER DATABASE File and Filegroups. FileOptionSyntax: each name and the values it takes.</summary>
		static readonly Dictionary<string, Func<Expression?, bool>> FileOptions = new(StringComparer.OrdinalIgnoreCase)
		{
			["NAME"]       = static value => StringValue(value) || NameValue(value),
			["NEWNAME"]    = static value => StringValue(value) || NameValue(value),
			["FILENAME"]   = static value => StringValue(value) || NameValue(value),
			["SIZE"]       = static value => Sized(value, "KB", "MB", "GB", "TB"),
			["MAXSIZE"]    = static value => Sized(value, "KB", "MB", "GB", "TB") || value is Expression.Reference { Name: [{ } word] } && Plain(word, "UNLIMITED"),
			["FILEGROWTH"] = static value => Sized(value, "KB", "MB", "GB", "TB", "%"),
			["OFFLINE"]    = static value => value is null,
		};

		/// <summary>
		/// T-SQL: ALTER DATABASE (Transact-SQL), the Azure syntax. EditionOption: a size in megabytes or gigabytes, an
		/// edition and a service objective as strings.
		/// </summary>
		static readonly Dictionary<string, Func<Expression?, bool>> EditionOptions = new(StringComparer.OrdinalIgnoreCase)
		{
			["MAXSIZE"]           = static value => value is Expression.Measured { Value: LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.Decimal } number, Unit: var unit }
				&& Unsigned(number.Text, number.Kind) && Plain(unit, "MB", "GB"),
			["EDITION"]           = static value => StringValue(value),
			["SERVICE_OBJECTIVE"] = static value => StringValue(value),
		};

		/// <summary>
		/// One way a setting of a closed catalogue is written: with <c>=</c> after its name or not, the values it takes, and
		/// the list in brackets after it, if any.
		/// </summary>
		sealed record Setting(bool Assigned, Func<Expression?, bool> Takes, Nest? Nest = null);

		/// <summary>The list in brackets after a setting: its own catalogue, whether it must be written, whether it holds one.</summary>
		sealed record Nest(Dictionary<string, Setting[]> Catalogue, bool Required = false, bool Single = false);

		/// <summary>
		/// A setting of a closed catalogue, keyed by its name and further words: the first of the ways it is written whose
		/// value and list fit, <c>=</c> or none as that way says. Where a name is written both ways with the same value,
		/// <see cref="Option.EqualsOmitted"/> says which; elsewhere it is refused.
		/// </summary>
		void PutSetting(Option option, Dictionary<string, Setting[]> catalogue, string rule)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, option, (catalogue, rule), static (into, node, place) => into.PutSetting(node, place.catalogue, place.rule));

				return;
			}

			if (option.Partitions is not null || option.Order is not null)
				throw Misplaced(option, "Partitions or Order", "in " + rule);

			if (option.Name.Style != IdentifierStyle.Regular || option.Words.Any(static word => word.Style != IdentifierStyle.Regular))
				throw Misplaced(option, "Name or Words", "quoted (" + rule + " reads its names as words)");

			var key   = string.Join(" ", new[] { option.Name.Text }.Concat(option.Words.Select(static word => word.Text)));
			var forms = catalogue.TryGetValue(key, out var all) ? all.Where(form => form.Takes(option.Value) && Fits(option, form.Nest)).ToList() : [];

			if (forms.Count == 0)
				throw Misplaced(option, "Name, Value or Options", "of a name, a value form or a list " + rule + " does not read");

			var both = forms.Any(static form => form.Assigned) && forms.Any(static form => !form.Assigned);

			if (option.EqualsOmitted && !both)
				throw Misplaced(option, "EqualsOmitted", "where " + rule + " fixes whether " + key + " takes =");

			var form = both ? forms.First(form => form.Assigned != option.EqualsOmitted) : forms[0];

			PutIdentifier(option.Name);

			foreach (var further in option.Words)
				PutIdentifier(further);

			if (form.Assigned)
				Word("=");

			if (option.Value is Expression.Reference { Name: [var word] })
				PutIdentifier(word);
			else if (option.Value is { } value)
				PutExpression(value);

			if (option.Options.Count > 0)
			{
				Open();
				Each(option.Options, one => PutSetting(one, form.Nest!.Catalogue, rule));
				Close();
			}
		}

		static bool Fits(Option option, Nest? nest)
		{
			return option.Options.Count == 0 ? nest is not { Required: true } : nest is not null && (!nest.Single || option.Options.Count == 1);
		}

		/// <summary>
		/// T-SQL: ALTER DATABASE SET Options (Transact-SQL), ALTER DATABASE SET HADR (Transact-SQL), ALTER DATABASE Database
		/// Mirroring (Transact-SQL). DatabaseSetting: each setting, by its name and further words, and the ways it is written.
		/// A word after a name it takes with no <c>=</c> is its value; <c>QUERY_STORE CLEAR [ALL]</c>, <c>PARTNER SAFETY</c>,
		/// <c>PARTNER TIMEOUT</c> and <c>HADR AVAILABILITY GROUP</c> are names of several words.
		/// </summary>
		static readonly Dictionary<string, Setting[]> DatabaseSettings = DatabaseCatalogue();

		static Dictionary<string, Setting[]> DatabaseCatalogue()
		{
			var catalogue = new Dictionary<string, Setting[]>(StringComparer.OrdinalIgnoreCase)
			{
				// DatabaseSetting: AUTO_CREATE_STATISTICS ON (INCREMENTAL = …), before the switches.
				["AUTO_CREATE_STATISTICS"]        = [new(false, Words("ON"), new(Settings(("INCREMENTAL", true, Words("ON", "OFF"))), Single: true)), new(false, Words("OFF"))],
				["MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT"] = [new(false, Words("ON", "OFF")), new(true, Words("ON", "OFF"))],
				["COMPATIBILITY_LEVEL"]           = [new(true, Digits)],
				["TWO_DIGIT_YEAR_CUTOFF"]         = [new(true, Digits)],
				["DEFAULT_LANGUAGE"]              = [new(true, static value => Digits(value) || NameValue(value))],
				["DEFAULT_FULLTEXT_LANGUAGE"]     = [new(true, static value => Digits(value) || NameValue(value))],
				["TARGET_RECOVERY_TIME"]          = [new(true, static value => value is Expression.Measured { Unit: var unit } measured && Plain(unit, "SECONDS", "MINUTES") && Decimal(measured.Value))],
				["CONTAINMENT"]                   = [new(true, Words("NONE", "PARTIAL"))],
				["DELAYED_DURABILITY"]            = [new(true, Words("DISABLED", "ALLOWED", "FORCED"))],
				["AUTOMATIC_TUNING"]              = [new(true, Words("AUTO", "CUSTOM")), new(false, None, new(Settings(("FORCE_LAST_GOOD_PLAN", true, Words("DEFAULT", "ON", "OFF"))), Required: true))],
				["CHANGE_TRACKING"]               = [new(true, Words("OFF")), new(true, Words("ON"), Tracking()), new(false, None, Tracking() with { Required = true })],
				["QUERY_STORE"]                   = [new(true, Words("OFF"), new(Settings(("FORCED", false, None)))), new(true, Words("ON"), Store()), new(false, None, Store() with { Required = true })],
				["QUERY_STORE CLEAR"]             = [new(false, None)],
				["QUERY_STORE CLEAR ALL"]         = [new(false, None)],
				["FILESTREAM"]                    = [new(false, None, new(Settings(
					("NON_TRANSACTED_ACCESS", true, Words("OFF", "READ_ONLY", "FULL")),
					("DIRECTORY_NAME",        true, static value => StringValue(value) || value is LiteralValue.Null)), Required: true))],
				["REMOTE_DATA_ARCHIVE"]           = [new(true, Words("ON"), Archive()), new(true, Words("OFF")), new(false, None, Archive())],
				["PERSISTENT_LOG_BUFFER"]         = [new(true, Words("ON"), new(Settings(("DIRECTORY_NAME", true, StringValue)), Required: true, Single: true)), new(true, Words("OFF"))],
				["SUSPEND_FOR_SNAPSHOT_BACKUP"]   = [new(true, Words("ON"), new(Settings(("MODE", true, Words("COPY_ONLY"))), Single: true)), new(true, Words("OFF"))],
				["ACCELERATED_DATABASE_RECOVERY"] = [new(true, Words("ON", "OFF"), new(Settings(("PERSISTENT_VERSION_STORE_FILEGROUP", true, NameValue)), Single: true))],

				// Availability groups and mirroring: only OFF is a switch.
				["HADR"]                          = [new(false, Words("SUSPEND", "RESUME", "OFF"))],
				["HADR AVAILABILITY GROUP"]       = [new(true, NameValue)],
				["PARTNER"]                       = [new(false, Words("FAILOVER", "FORCE_SERVICE_ALLOW_DATA_LOSS", "RESUME", "SUSPEND", "OFF")), new(true, StringValue)],
				["PARTNER SAFETY"]                = [new(false, Words("FULL", "OFF"))],
				["PARTNER TIMEOUT"]               = [new(false, Digits)],
				["WITNESS"]                       = [new(false, Words("OFF")), new(true, StringValue)],

				// DatabaseWords: the settings that take a word with no =.
				["CURSOR_DEFAULT"]                = [new(false, Words("LOCAL", "GLOBAL"))],
				["RECOVERY"]                      = [new(false, Words("FULL", "BULK_LOGGED", "SIMPLE"))],
				["PAGE_VERIFY"]                   = [new(false, Words("CHECKSUM", "TORN_PAGE_DETECTION", "NONE"))],
				["PARAMETERIZATION"]              = [new(false, Words("SIMPLE", "FORCED"))],
				["ENCRYPTION"]                    = [new(false, Words("SUSPEND", "RESUME", "ON", "OFF"))],
			};

			// DatabaseWords: the settings that are a word alone.
			foreach (var word in new[] { "ONLINE", "OFFLINE", "EMERGENCY", "READ_ONLY", "READ_WRITE", "SINGLE_USER", "RESTRICTED_USER", "MULTI_USER", "ENABLE_BROKER", "DISABLE_BROKER", "NEW_BROKER", "ERROR_BROKER_CONVERSATIONS" })
				catalogue.Add(word, [new(false, None)]);

			// DatabaseSwitch: ON or OFF with no =.
			foreach (var name in new[] { "AUTO_CLOSE", "AUTO_SHRINK", "AUTO_UPDATE_STATISTICS", "AUTO_UPDATE_STATISTICS_ASYNC", "CURSOR_CLOSE_ON_COMMIT", "DATE_CORRELATION_OPTIMIZATION", "DB_CHAINING", "TRUSTWORTHY", "MIXED_PAGE_ALLOCATION", "HONOR_BROKER_PRIORITY", "ALLOW_SNAPSHOT_ISOLATION", "READ_COMMITTED_SNAPSHOT", "ANSI_NULL_DEFAULT", "ANSI_NULLS", "ANSI_PADDING", "ANSI_WARNINGS", "ARITHABORT", "CONCAT_NULL_YIELDS_NULL", "NUMERIC_ROUNDABORT", "QUOTED_IDENTIFIER", "RECURSIVE_TRIGGERS", "TORN_PAGE_DETECTION", "TEMPORAL_HISTORY_RETENTION", "SUPPLEMENTAL_LOGGING", "VARDECIMAL_STORAGE_FORMAT", "RESULT_SET_CACHING" })
				catalogue.Add(name, [new(false, Words("ON", "OFF"))]);

			// DatabaseEqualsSwitch: ON or OFF after =.
			foreach (var name in new[] { "NESTED_TRIGGERS", "TRANSFORM_NOISE_WORDS", "OPTIMIZED_LOCKING" })
				catalogue.Add(name, [new(true, Words("ON", "OFF"))]);

			return catalogue;
		}

		/// <summary>T-SQL: ALTER DATABASE SET Options (Transact-SQL). TrackingNest: AUTO_CLEANUP, and a retention in days, hours or minutes.</summary>
		static Nest Tracking()
		{
			return new(Settings(
			("AUTO_CLEANUP",     true, Words("ON", "OFF")),
			("CHANGE_RETENTION", true, static value => value is Expression.Measured { Unit: var unit } measured && Plain(unit, "DAYS", "HOURS", "MINUTES") && Digits(measured.Value))));
		}

		/// <summary>T-SQL: ALTER DATABASE SET Options (Transact-SQL). StoreNest, its CLEANUP_POLICY and its QUERY_CAPTURE_POLICY.</summary>
		static Nest Store()
		{
			return new(StoreCatalogue());
		}

		static Dictionary<string, Setting[]> StoreCatalogue()
		{
			var catalogue = Settings(
				("OPERATION_MODE",          true, Words("READ_WRITE", "READ_ONLY")),
				("DESIRED_STATE",           true, Words("READ_WRITE", "READ_ONLY")),
				("SIZE_BASED_CLEANUP_MODE", true, Words("AUTO", "OFF")),
				("QUERY_CAPTURE_MODE",      true, Words("ALL", "AUTO", "CUSTOM", "NONE")),
				("WAIT_STATS_CAPTURE_MODE", true, Words("ON", "OFF")));

			foreach (var name in new[] { "DATA_FLUSH_INTERVAL_SECONDS", "FLUSH_INTERVAL_SECONDS", "INTERVAL_LENGTH_MINUTES", "MAX_STORAGE_SIZE_MB", "MAX_PLANS_PER_QUERY" })
				catalogue.Add(name, [new(true, Digits)]);

			catalogue.Add("CLEANUP_POLICY", [new(true, None, new(Settings(("STALE_QUERY_THRESHOLD_DAYS", true, Digits)), Required: true))]);
			catalogue.Add("QUERY_CAPTURE_POLICY", [new(true, None, new(Settings(
				("STALE_CAPTURE_POLICY_THRESHOLD", true, static value => value is Expression.Measured { Unit: var unit } measured && Plain(unit, "DAYS", "DAY", "HOURS", "HOUR") && Digits(measured.Value)),
				("EXECUTION_COUNT",                true, Digits),
				("TOTAL_COMPILE_CPU_TIME_MS",      true, Digits),
				("TOTAL_EXECUTION_CPU_TIME_MS",    true, Digits)), Required: true))]);

			return catalogue;
		}

		/// <summary>T-SQL: ALTER DATABASE SET Options (Transact-SQL). ArchiveNest.</summary>
		static Nest Archive()
		{
			return new(Settings(
			("SERVER",                    true, StringValue),
			("CREDENTIAL",                true, NameValue),
			("FEDERATED_SERVICE_ACCOUNT", true, Words("ON", "OFF"))), Required: true);
		}

		/// <summary>
		/// T-SQL: ALTER DATABASE SCOPED CONFIGURATION. ScopedSetting: the switches, which may also be set to PRIMARY, and the
		/// settings given a number, a word or an address.
		/// </summary>
		static readonly Dictionary<string, Setting[]> ScopedSettings = ScopedCatalogue();

		static Dictionary<string, Setting[]> ScopedCatalogue()
		{
			var catalogue = new Dictionary<string, Setting[]>(StringComparer.OrdinalIgnoreCase)
			{
				["MAXDOP"]                         = [new(true, static value => Signed(value) || Words("PRIMARY")(value))],
				["ELEVATE_ONLINE"]                 = [new(true, Words("OFF", "WHEN_SUPPORTED", "FAIL_UNSUPPORTED"))],
				["ELEVATE_RESUMABLE"]              = [new(true, Words("OFF", "WHEN_SUPPORTED", "FAIL_UNSUPPORTED"))],
				["LEDGER_DIGEST_STORAGE_ENDPOINT"] = [new(true, static value => StringValue(value) || Words("OFF")(value))],
				["PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES"] = [new(true, Signed)],
				["FULLTEXT_INDEX_VERSION"]         = [new(true, Signed)],
				["DW_COMPATIBILITY_LEVEL"]         = [new(true, static value => Signed(value) || Words("AUTO")(value))],
			};

			// ScopedSwitch.
			foreach (var name in new[] { "ACCELERATED_PLAN_FORCING", "ASYNC_STATS_UPDATE_WAIT_AT_LOW_PRIORITY", "BATCH_MODE_ADAPTIVE_JOINS", "BATCH_MODE_MEMORY_GRANT_FEEDBACK", "BATCH_MODE_ON_ROWSTORE", "CE_FEEDBACK", "DEFERRED_COMPILATION_TV", "DOP_FEEDBACK", "EXEC_QUERY_STATS_FOR_SCALAR_FUNCTIONS", "FORCE_SHOWPLAN_RUNTIME_PARAMETER_COLLECTION", "IDENTITY_CACHE", "INTERLEAVED_EXECUTION_TVF", "ISOLATE_SECURITY_POLICY_CARDINALITY", "GLOBAL_TEMPORARY_TABLE_AUTO_DROP", "LAST_QUERY_PLAN_STATS", "LEGACY_CARDINALITY_ESTIMATION", "LIGHTWEIGHT_QUERY_PROFILING", "MEMORY_GRANT_FEEDBACK_PERCENTILE_GRANT", "MEMORY_GRANT_FEEDBACK_PERSISTENCE", "OPTIMIZE_FOR_AD_HOC_WORKLOADS", "OPTIMIZED_PLAN_FORCING", "OPTIMIZED_SP_EXECUTESQL", "OPTIONAL_PARAMETER_OPTIMIZATION", "PARAMETER_SENSITIVE_PLAN_OPTIMIZATION", "PARAMETER_SNIFFING", "PREVIEW_FEATURES", "QUERY_OPTIMIZER_HOTFIXES", "READABLE_SECONDARY_TEMPORARY_STATS_AUTO_CREATE", "READABLE_SECONDARY_TEMPORARY_STATS_AUTO_UPDATE", "ROW_MODE_MEMORY_GRANT_FEEDBACK", "TSQL_SCALAR_UDF_INLINING", "VERBOSE_TRUNCATION_WARNINGS", "XTP_PROCEDURE_EXECUTION_STATISTICS", "XTP_QUERY_EXECUTION_STATISTICS" })
				catalogue.Add(name, [new(true, Words("ON", "OFF", "PRIMARY"))]);

			return catalogue;
		}

		/// <summary>A catalogue of settings each written one way.</summary>
		static Dictionary<string, Setting[]> Settings(params (string Name, bool Assigned, Func<Expression?, bool> Takes)[] settings)
		{
			return settings.ToDictionary(static one => one.Name, static one => new Setting[] { new(one.Assigned, one.Takes) }, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>No value.</summary>
		static bool None(Expression? value)
		{
			return value is null;
		}

		/// <summary>One of the words given, written plain.</summary>
		static Func<Expression?, bool> Words(params string[] words)
		{
			return value => value is Expression.Reference { Name: [var word] } && Plain(word, words);
		}

		/// <summary>Digits (<c>DatabaseInteger</c>).</summary>
		static bool Digits(Expression? value)
		{
			return value is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger, Text: var digits } && Unsigned(digits, NumericLiteralKind.DecimalInteger);
		}

		/// <summary>Digits with a point or without (<c>DatabaseDecimal</c>), no exponent.</summary>
		static bool Decimal(Expression? value)
		{
			return value is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.Decimal } number && Unsigned(number.Text, number.Kind);
		}

		/// <summary>A decimal with a minus in front or none (<c>ScopedNumber</c>).</summary>
		static bool Signed(Expression? value)
		{
			return value is Expression.Unary { Operator: UnaryOperator.Minus, Operand: var operand } ? Decimal(operand) : Decimal(value);
		}

		/// <summary>The grammar's <c>Quoted</c>: a string, national or not.</summary>
		static bool StringValue(Expression? value)
		{
			return value is LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null };
		}

		/// <summary>A name of one part, as a value: <c>NAME = a</c>.</summary>
		static bool NameValue(Expression? value)
		{
			return value is Expression.Reference { Name: [{ Style: IdentifierStyle.Regular or IdentifierStyle.Bracketed or IdentifierStyle.Delimited, Text.Length: > 0 }] };
		}

		/// <summary>Digits, alone or with one of the units given after them (<c>Sql92.Lexical.Digits &amp; SizeUnit?</c>).</summary>
		static bool Sized(Expression? value, params string[] units)
		{
			var number = value is Expression.Measured { Unit: var unit } measured && units.Any(one => Plain(unit, one)) ? measured.Value : value;

			return number is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger, Text: var digits } && Unsigned(digits, NumericLiteralKind.DecimalInteger);
		}
	}
}
