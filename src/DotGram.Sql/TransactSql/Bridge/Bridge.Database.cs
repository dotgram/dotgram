using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Bridge
{
	/// <summary>
	/// A database and what it is made of: files, filegroups, its settings, backups and restores, partitions, full-text catalogues.
	/// </summary>
	internal static class Database
	{
		public static readonly string[] Owns =
		[
			"Statement.CreateDatabase", "Statement.AlterDatabaseSet", "Statement.AlterDatabaseScopedConfiguration",
			"Statement.AlterDatabaseCollate", "Statement.AlterDatabaseModifyName", "Statement.AlterDatabaseModifyFileGroup",
			"Statement.AlterDatabaseModifyFile", "Statement.AlterDatabaseModify", "Statement.AlterDatabaseAddFileGroup",
			"Statement.AlterDatabaseAddLogFile", "Statement.AlterDatabaseAddFile", "Statement.AlterDatabaseRemoveFileGroup",
			"Statement.AlterDatabaseRemoveFile", "Statement.AlterDatabaseRebuildLog", "Statement.AlterDatabasePerformCutover",
			"Statement.AlterDatabaseModifyBackupStorageRedundancy", "Statement.BackupDatabase", "Statement.BackupTransactionLog",
			"Statement.BackupServer", "Statement.BackupGroup", "Statement.BackupCertificate",
			"Statement.BackupMasterKey", "Statement.BackupServiceMasterKey", "Statement.BackupSymmetricKey",
			"Statement.RestoreDatabase", "Statement.RestoreLog", "Statement.RestoreFileListOnly",
			"Statement.RestoreHeaderOnly", "Statement.RestoreLabelOnly", "Statement.RestoreRewindOnly",
			"Statement.RestoreVerifyOnly", "Statement.RestoreMasterKey", "Statement.RestoreServiceMasterKey",
			"Statement.RestoreSymmetricKey", "Statement.Checkpoint", "Statement.PartitionFunctionDefinition",
			"Statement.AlterPartitionFunction", "Statement.PartitionSchemeDefinition", "Statement.AlterPartitionScheme",
			"Statement.FullTextCatalogDefinition", "Statement.AlterFullTextCatalog", "Statement.FullTextStopListDefinition",
			"Statement.AlterFullTextStopList", "Statement.SearchPropertyListDefinition", "Statement.AlterSearchPropertyList",
			"Clause.DatabaseFile", "Clause.FileGroup",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.AlterDatabaseModify modify => AlterModify(modify),
				Statement.AlterDatabaseSet set => Target(set, set.Name, new Ast.AlterDatabaseAction.Set(Each(set.Settings, one => Setting(set, one))) { Termination = Termination(set, set.Termination) }),
				Statement.AlterDatabaseScopedConfiguration scoped => Scoped(scoped),
				Statement.CreateDatabase create => Create(create),
				Statement.PartitionFunctionDefinition or Statement.AlterPartitionFunction or Statement.PartitionSchemeDefinition or Statement.AlterPartitionScheme
					or Statement.FullTextCatalogDefinition or Statement.AlterFullTextCatalog or Statement.FullTextStopListDefinition or Statement.AlterFullTextStopList
					or Statement.SearchPropertyListDefinition or Statement.AlterSearchPropertyList
					=> Checked((Statement.Definition)old, Defined((Statement.Definition)old)),
				Statement.Checkpoint(var duration) => new Ast.Statement.Checkpoint(duration is null ? null : Values.Of(duration)),
				Statement.Definition definition when definition.GetType().Name.StartsWith("Backup", StringComparison.Ordinal)
					=> Checked(definition, Backup(definition)),
				Statement.Definition definition when definition.GetType().Name.StartsWith("Restore", StringComparison.Ordinal)
					=> Checked(definition, Restore(definition)),
				Statement.Definition definition when definition.GetType().Name.StartsWith("AlterDatabase", StringComparison.Ordinal)
					=> Alter(definition),
				_ when Owned(Owns, old) => throw Unconverted(old),
				_ => null,
			};

			return made is not null;
		}

		/// <summary>
		/// ALTER DATABASE's actions other than SET, which the old tree keeps as a record each and the words after the action
		/// as text: read again by tokens, and a file specification as CREATE DATABASE's. A termination after one of them
		/// has no member, and nor has REBUILD LOG's file or a log file's filegroup.
		/// </summary>
		static Ast.Statement Alter(Statement.Definition old)
		{
			var tail   = old.Tail ?? "";
			var tokens = Located(tail);
			var words  = Words(tail);

			if (words is [.., "WITH", "ROLLBACK", "IMMEDIATE"] or [.., "WITH", "NO_WAIT"] || Index(tokens, "ROLLBACK") >= 0)
				throw BridgeGap.Missing(old, "Statement.AlterDatabase (a termination after an action other than SET)");

			Ast.AlterDatabaseAction action = old switch
			{
				Statement.AlterDatabaseCollate => new Ast.AlterDatabaseAction.Collate(new Ast.CollationName(Values.Name(old, tail))),
				Statement.AlterDatabaseModifyName when words is ["=", _] => new Ast.AlterDatabaseAction.ModifyName(Values.Part(old, tokens[1].Token.Text)),
				Statement.AlterDatabaseModifyBackupStorageRedundancy when words is ["=", _] && tokens[1].Token.Kind == SqlTokens.Kind.String
					=> new Ast.AlterDatabaseAction.ModifyBackupStorageRedundancy(Values.String(tokens[1].Token.Text)),
				Statement.AlterDatabasePerformCutover when tail.Length == 0 => new Ast.AlterDatabaseAction.PerformCutover(),
				Statement.AlterDatabaseRebuildLog when tail.Length == 0 => new Ast.AlterDatabaseAction.RebuildLog(),
				Statement.AlterDatabaseRebuildLog => throw BridgeGap.Missing(old, "AlterDatabaseAction.RebuildLog (REBUILD LOG ON a file)"),
				Statement.AlterDatabaseAddFile => AddFile(old, tail, tokens, log: false),
				Statement.AlterDatabaseAddLogFile => AddFile(old, tail, tokens, log: true),
				Statement.AlterDatabaseModifyFile => new Ast.AlterDatabaseAction.ModifyFile(Files(old, tail) is [var file] ? file : throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({tail})")),
				Statement.AlterDatabaseRemoveFile when tokens.Count == 1 => new Ast.AlterDatabaseAction.RemoveFile(Values.Part(old, tail)),
				Statement.AlterDatabaseRemoveFileGroup when tokens.Count == 1 => new Ast.AlterDatabaseAction.RemoveFileGroup(Values.Part(old, tail)),
				Statement.AlterDatabaseAddFileGroup => words switch
				{
					[_] => new Ast.AlterDatabaseAction.AddFileGroup(Values.Part(old, tokens[0].Token.Text), null),
					[_, "CONTAINS", "FILESTREAM"] => new Ast.AlterDatabaseAction.AddFileGroup(Values.Part(old, tokens[0].Token.Text), Ast.FileGroupContents.Filestream),
					[_, "CONTAINS", "MEMORY_OPTIMIZED_DATA"] => new Ast.AlterDatabaseAction.AddFileGroup(Values.Part(old, tokens[0].Token.Text), Ast.FileGroupContents.MemoryOptimizedData),
					_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({tail})"),
				},
				Statement.AlterDatabaseModifyFileGroup when tokens.Count >= 2 => new Ast.AlterDatabaseAction.ModifyFileGroup(Values.Part(old, tokens[0].Token.Text), words[1..] switch
				{
					["READONLY"]             => new Ast.FileGroupChange.Updatability(Ast.FileGroupUpdatability.ReadOnly, false),
					["READ_ONLY"]            => new Ast.FileGroupChange.Updatability(Ast.FileGroupUpdatability.ReadOnly, true),
					["READWRITE"]            => new Ast.FileGroupChange.Updatability(Ast.FileGroupUpdatability.ReadWrite, false),
					["READ_WRITE"]           => new Ast.FileGroupChange.Updatability(Ast.FileGroupUpdatability.ReadWrite, true),
					["DEFAULT"]              => new Ast.FileGroupChange.Default(),
					["AUTOGROW_SINGLE_FILE"] => new Ast.FileGroupChange.AutoGrow(Ast.FileGroupAutoGrow.SingleFile),
					["AUTOGROW_ALL_FILES"]   => new Ast.FileGroupChange.AutoGrow(Ast.FileGroupAutoGrow.AllFiles),
					["NAME", "=", _]         => new Ast.FileGroupChange.Rename(Values.Part(old, tokens[3].Token.Text)),
					_                        => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (MODIFY FILEGROUP … {tail})"),
				}),
				_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({tail})"),
			};

			return Checked(old, Target(old, old.Name, action));
		}

		/// <summary>ADD FILE and ADD LOG FILE: the files, and the filegroup after TO FILEGROUP, which a log file's has no member for.</summary>
		static Ast.AlterDatabaseAction AddFile(Statement old, string tail, List<(SqlTokens.Token Token, int At)> tokens, bool log)
		{
			var to    = Index(tokens, "TO");
			var files = Files(old, to < 0 ? tail : Slice(tail, tokens, 0, to));

			if (to < 0)
				return log ? new Ast.AlterDatabaseAction.AddLogFile(files) : new Ast.AlterDatabaseAction.AddFile(files, null);

			if (log)
				throw BridgeGap.Missing(old, "AlterDatabaseAction.AddLogFile (TO FILEGROUP after a log file)");

			return Words(Slice(tail, tokens, to, tokens.Count)) is ["TO", "FILEGROUP", _]
				? new Ast.AlterDatabaseAction.AddFile(files, Values.Part(old, tokens[^1].Token.Text))
				: throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({tail})");
		}

		/// <summary>
		/// The Azure MODIFY: its edition options and MANUAL_CUTOVER. The old tree drops a termination after it, which the
		/// text has; such a text has no member and stays a gap.
		/// </summary>
		static Ast.Statement AlterModify(Statement.AlterDatabaseModify old)
		{
			if (Spelled(old) is { } text && Words(text) is var words && (words.Contains("ROLLBACK") || words.Contains("NO_WAIT")))
				throw BridgeGap.Lost(old, "Statement.AlterDatabaseModify (a termination after MODIFY)");

			var action = new Ast.AlterDatabaseAction.Modify(Each(old.Options ?? [], one => one is Clause.Option option ? Tables.Option(option) : throw BridgeGap.Open(one, $"inventory {NameOf(one)}")), old.With is { Length: > 0 });

			return Target(old, old.Name, action);
		}

		/// <summary>
		/// One setting of ALTER DATABASE … SET or of a scoped configuration, which the old tree keeps as a name of words, a
		/// value and the list in brackets after it. A word the setting takes with no <c>=</c> and nothing after it is kept in
		/// the name: the last word is the value, but for <c>QUERY_STORE CLEAR [ALL]</c>, which is a name of several words
		/// and nothing more. The form, <c>=</c> or none, is the catalogue's but for the one setting read both ways.
		/// </summary>
		static Ast.Option Setting(Statement old, Clause clause)
		{
			if (clause is not Clause.Option(var name, var value, var options, null, var bare))
				throw BridgeGap.Open(old, $"inventory {NameOf(clause)} (in {NameOf(old)})");

			var words = SqlTokens.Of(name).Select(static one => one.Text).ToList();
			var upper = Words(name);
			Ast.Expression? made;

			if (value is null && options.Length == 0 && words.Count >= 2 && upper is not ["QUERY_STORE", "CLEAR", ..])
			{
				made  = new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(old, words[^1])]));
				words = words[..^1];
			}
			else
				made = value switch
				{
					null                                => null,
					Expression.Measured(var v, var unit) => new Ast.Expression.Measured(Values.Of(v), new Ast.Identifier(unit)),
					_                                   => Values.Of(value),
				};

			return new Ast.Option(Values.Part(old, words[0]), made, Each(options, one => Setting(old, one)))
			{
				Words         = [.. words.Skip(1).Select(word => Values.Part(old, word))],
				EqualsOmitted = bare && upper is ["MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT"],
			};
		}

		/// <summary>The termination after SET, which the old tree keeps as its words.</summary>
		static Ast.Termination? Termination(Statement old, string? text)
		{
			if (text is null)
				return null;

			var tokens = SqlTokens.Of(text).ToList();

			return Words(text) switch
			{
				["WITH", "ROLLBACK", "IMMEDIATE"] => new Ast.Termination.RollbackImmediate(),
				["WITH", "NO_WAIT"]               => new Ast.Termination.NoWait(),
				["WITH", "ROLLBACK", "AFTER", _, .. var rest] when rest is [] or ["SECONDS"]
					=> new Ast.Termination.RollbackAfter(new Ast.LiteralValue.Numeric(tokens[3].Text, Ast.NumericLiteralKind.DecimalInteger)) { Seconds = rest.Count == 1 },
				_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (termination `{text}`)"),
			};
		}

		/// <summary>ALTER DATABASE SCOPED CONFIGURATION: SET and its one setting, or CLEAR PROCEDURE_CACHE and its plan handle.</summary>
		static Ast.Statement Scoped(Statement.AlterDatabaseScopedConfiguration old)
		{
			Ast.ScopedConfigurationAction action = old switch
			{
				{ Action: "SET", Settings: [var setting], Argument: null } => new Ast.ScopedConfigurationAction.Set(Setting(old, setting)) { Secondary = old.Secondary },
				{ Action: "CLEAR PROCEDURE_CACHE", Settings: [], Secondary: false } => new Ast.ScopedConfigurationAction.ClearProcedureCache(
					old.Argument is null ? null : Values.Of(old.Argument) as Ast.LiteralValue.Binary ?? throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a plan handle that is not binary)")),
				_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({old.Action})"),
			};

			return new Ast.Statement.AlterDatabaseScopedConfiguration(action);
		}

		/// <summary>
		/// CREATE DATABASE: the files and filegroups the old tree keeps in one list, the log's, the containment and
		/// collation as their words, the edition options, and the tail that says what the database is made from: empty
		/// for Azure's form, the attach's words, or the words of a snapshot or a copy and the database they name.
		/// </summary>
		static Ast.Statement Create(Statement.CreateDatabase old)
		{
			var files  = new List<Ast.DatabaseFile>();
			var groups = new List<Ast.FileGroup>();

			foreach (var one in old.Files)
			{
				switch (one)
				{
					case Clause.DatabaseFile file when groups.Count == 0:
						files.Add(File(old, file));
						break;

					case Clause.FileGroup(var name, var contains, var fallback, var held):
						groups.Add(new Ast.FileGroup(Values.Part(old, name), Each(held, file => file is Clause.DatabaseFile each ? File(old, each) : throw BridgeGap.Open(old, $"inventory {NameOf(file)} (in a filegroup)")))
						{
							Contains = contains is null ? null : Words(contains) switch
							{
								["FILESTREAM"]            => Ast.FileGroupContents.Filestream,
								["MEMORY_OPTIMIZED_DATA"] => Ast.FileGroupContents.MemoryOptimizedData,
								_                         => throw BridgeGap.Open(old, $"inventory Clause.FileGroup (CONTAINS {contains})"),
							},
							Default = fallback,
						});
						break;

					default:
						throw BridgeGap.Open(old, $"inventory {NameOf(one)} (in CREATE DATABASE's files)");
				}
			}

			var tokens = SqlTokens.Of(old.Tail ?? "").ToList();

			Ast.DatabaseSource? source = Words(old.Tail ?? "") switch
			{
				[]                               => null,
				["FOR", "ATTACH"]                => new Ast.DatabaseSource.ForAttach(Ast.AttachKind.Attach),
				["FOR", "ATTACH_REBUILD_LOG"]    => new Ast.DatabaseSource.ForAttach(Ast.AttachKind.AttachRebuildLog),
				["FOR", "ATTACH_FORCE_REBUILD_LOG"] => new Ast.DatabaseSource.ForAttach(Ast.AttachKind.AttachForceRebuildLog),
				["AS", "SNAPSHOT", "OF", _]      => new Ast.DatabaseSource.AsSnapshotOf(Values.Part(old, tokens[3].Text)),
				["AS", "COPY", "OF", ..]         => new Ast.DatabaseSource.AsCopyOf(Values.Name(old, string.Concat(tokens.Skip(3).Select(static one => one.Text)))),
				_                                => throw BridgeGap.Open(old, $"inventory Statement.CreateDatabase (tail `{old.Tail}`)"),
			};

			var made = new Ast.Statement.CreateDatabase(Values.Part(old, old.Name))
			{
				Containment = old.Containment is null ? null : Words(old.Containment) switch
				{
					["NONE"]    => Ast.DatabaseContainment.None,
					["PARTIAL"] => Ast.DatabaseContainment.Partial,
					_           => throw BridgeGap.Open(old, $"inventory Statement.CreateDatabase (CONTAINMENT = {old.Containment})"),
				},
				Primary        = old.Primary,
				Files          = [.. files],
				FileGroups     = [.. groups],
				LogFiles       = Each(old.Log ?? [], one => one is Clause.DatabaseFile file ? File(old, file) : throw BridgeGap.Open(old, $"inventory {NameOf(one)} (in LOG ON)")),
				Collation      = old.Collation is null ? null : new Ast.CollationName(Values.Name(old, old.Collation)),
				EditionOptions = Each(old.Options ?? [], one => one is Clause.Option option ? Tables.Option(option) : throw BridgeGap.Open(one, $"inventory {NameOf(one)}")),
				Source         = source,
				Options        = Each(old.With ?? [], one => Setting(old, one)),
			};

			// The containment and the tail are kept as written and compared exactly, so a text in another case than the
			// writer's capitals stays a gap.
			string text;

			try
			{
				text = Ast.TransactSqlWriter.Write(made);
			}
			catch (Exception refused) when (refused is ArgumentException or NotSupportedException)
			{
				return made;
			}

			return TransactSql.TransactSqlParser.TryParseSql(text) is { IsSuccess: true, Value: [Statement.CreateDatabase again] }
				&& again.Containment == old.Containment && again.Tail == old.Tail
				? made
				: throw BridgeGap.Open(old, "Trees.FreeText (Statement.CreateDatabase.Containment or Tail, written otherwise than the writer prints it)");
		}

		/// <summary>
		/// BACKUP: the old tree keeps what is backed up as its record, the database as its name, and the rest as text —
		/// the files, the devices after TO and after each MIRROR TO, the options after WITH; a key's or a certificate's
		/// place and passwords. Read again by tokens.
		/// </summary>
		static Ast.Statement Backup(Statement.Definition old)
		{
			var text   = old.Tail ?? "";
			var tokens = Located(text);
			var words  = Words(text);

			if (Spelled(old) is { } spelled && Words(spelled) is [_, "DATABASE" or "LOG", "(", ..])
				throw BridgeGap.Lost(old, $"{NameOf(old)} (a bracketed list of names, of which the old tree keeps the first)");

			switch (old)
			{
				case Statement.BackupCertificate:
					if (words is not ["TO", "FILE", "=", _, ..] || tokens[3].Token.Kind != SqlTokens.Kind.String)
						throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");

					var certificate = new Ast.Statement.BackupCertificate(Values.Part(old, old.Name), Values.String(tokens[3].Token.Text));

					if (tokens.Count == 4)
						return certificate;

					var at = 5;

					if (words is [_, _, _, _, "WITH", "FORMAT", "=", _, ",", ..] && tokens[7].Token.Kind == SqlTokens.Kind.String)
					{
						certificate = certificate with { Format = Values.String(tokens[7].Token.Text) };
						at = 9;
					}

					return words.Count > at + 3 && words[4] == "WITH" && words[at] == "PRIVATE" && words[at + 1] == "KEY" && words[at + 2] == "(" && words[^1] == ")"
						? certificate with { PrivateKey = Options(old, text[(tokens[at + 2].At + 1)..tokens[^1].At]) }
						: throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");

				case Statement.BackupMasterKey when words is ["TO", _, "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _]:
					return new Ast.Statement.BackupMasterKey(Place(old, tokens, 1), Password(old, tokens, 8));

				case Statement.BackupServiceMasterKey when words is ["TO", "FILE", "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _]:
					return new Ast.Statement.BackupServiceMasterKey(Password(old, tokens, 3), Password(old, tokens, 8));

				case Statement.BackupSymmetricKey when words is ["TO", _, "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _]:
					return new Ast.Statement.BackupSymmetricKey(Values.Part(old, old.Name), Place(old, tokens, 1), Password(old, tokens, 8));

				case Statement.BackupMasterKey or Statement.BackupServiceMasterKey or Statement.BackupSymmetricKey:
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");
			}

			var to = Index(tokens, "TO");

			if (to < 0)
				throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");

			var files = BackupFiles(old, Slice(text, tokens, 0, to));
			var with  = Index(tokens, "WITH", to);
			var end   = with < 0 ? tokens.Count : with;
			var marks = new List<int>();

			for (var mirror = Index(tokens, "MIRROR", to); mirror >= 0 && mirror < end; mirror = Index(tokens, "MIRROR", mirror + 1))
				marks.Add(mirror);

			var devices = Devices(old, Slice(text, tokens, to + 1, marks.Count > 0 ? marks[0] : end));
			var mirrors = new List<Ast.BackupMirror>();

			for (var one = 0; one < marks.Count; one++)
			{
				if (words.Count <= marks[one] + 1 || words[marks[one] + 1] != "TO")
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");

				mirrors.Add(new Ast.BackupMirror(Devices(old, Slice(text, tokens, marks[one] + 2, one + 1 < marks.Count ? marks[one + 1] : end))));
			}

			var options = with < 0 ? [] : Options(old, Slice(text, tokens, with + 1, tokens.Count));

			if (files.Count > 0 && old is not Statement.BackupDatabase)
				throw BridgeGap.Missing(old, $"{NameOf(old)} (files, which only BACKUP DATABASE's syntax names)");

			return old switch
			{
				Statement.BackupDatabase       => new Ast.Statement.BackupDatabase(BackupName(old, old.Name), devices) { Files = files, Mirrors = [.. mirrors], Options = options },
				Statement.BackupTransactionLog => new Ast.Statement.BackupLog(BackupName(old, old.Name), devices) { Mirrors = [.. mirrors], Options = options },
				Statement.BackupServer when old.Name.Length == 0 => new Ast.Statement.BackupServer(devices) { Mirrors = [.. mirrors], Options = options },
				Statement.BackupGroup          => new Ast.Statement.BackupGroup([.. Listed(old.Name).Select(one => BackupName(old, one))], devices) { Mirrors = [.. mirrors], Options = options },
				_                              => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({old.Name})"),
			};
		}

		/// <summary>
		/// RESTORE: likewise — the files, the devices or the snapshot after FROM, the options after WITH; a key's place,
		/// its passwords and FORCE.
		/// </summary>
		static Ast.Statement Restore(Statement.Definition old)
		{
			var text   = old.Tail ?? "";
			var tokens = Located(text);
			var words  = Words(text);

			if (Spelled(old) is { } spelled && Words(spelled) is [_, "DATABASE" or "LOG", "(", ..])
				throw BridgeGap.Lost(old, $"{NameOf(old)} (a bracketed list of names, of which the old tree keeps the first)");

			switch (old)
			{
				case Statement.RestoreMasterKey when words is ["FROM", _, "=", _, "DECRYPTION", "BY", "PASSWORD", "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _, .. var force] && force is [] or ["FORCE"]:
					return new Ast.Statement.RestoreMasterKey(Place(old, tokens, 1), Password(old, tokens, 8), Password(old, tokens, 13)) { Force = force.Count == 1 };

				case Statement.RestoreServiceMasterKey when words is ["FROM", "FILE", "=", _, "DECRYPTION", "BY", "PASSWORD", "=", _, .. var force] && force is [] or ["FORCE"]:
					return new Ast.Statement.RestoreServiceMasterKey(Password(old, tokens, 3), Password(old, tokens, 8)) { Force = force.Count == 1 };

				case Statement.RestoreSymmetricKey when words is ["FROM", _, "=", _, "DECRYPTION", "BY", "PASSWORD", "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _, .. var force] && force is [] or ["FORCE"]:
					return new Ast.Statement.RestoreSymmetricKey(Values.Part(old, old.Name), Place(old, tokens, 1), Password(old, tokens, 8), Password(old, tokens, 13)) { Force = force.Count == 1 };

				case Statement.RestoreMasterKey or Statement.RestoreServiceMasterKey or Statement.RestoreSymmetricKey:
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");
			}

			var from  = Index(tokens, "FROM");
			var with  = Index(tokens, "WITH");
			var files = BackupFiles(old, Slice(text, tokens, 0, from >= 0 ? from : with >= 0 ? with : tokens.Count));
			var end   = with < 0 ? tokens.Count : with;

			SqlList<Ast.BackupDevice> devices = [];
			Ast.Expression? snapshot = null;

			if (from >= 0 && words.Count > from + 1 && words[from + 1] == "DATABASE_SNAPSHOT")
				snapshot = end == from + 4 && words[from + 2] == "=" ? Value(old, tokens[from + 3].Token) : throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");
			else if (from >= 0)
				devices = Devices(old, Slice(text, tokens, from + 1, end));

			var options = with < 0 ? [] : Options(old, Slice(text, tokens, with + 1, tokens.Count));

			if (old is not (Statement.RestoreDatabase or Statement.RestoreLog) && (old.Name.Length > 0 || files.Count > 0))
				throw BridgeGap.Missing(old, $"{NameOf(old)} (a database or files, which its syntax does not name)");

			if (old is Statement.RestoreDatabase or Statement.RestoreLog && old.Name.Length == 0)
				throw BridgeGap.Missing(old, $"{NameOf(old)} (no database, which its syntax names)");

			if (snapshot is not null && old is not Statement.RestoreDatabase)
				throw BridgeGap.Missing(old, $"{NameOf(old)} (FROM DATABASE_SNAPSHOT, which only RESTORE DATABASE's syntax names)");

			return old switch
			{
				Statement.RestoreDatabase     => new Ast.Statement.RestoreDatabase(BackupName(old, old.Name)) { Files = files, From = devices, DatabaseSnapshot = snapshot, Options = options },
				Statement.RestoreLog          => new Ast.Statement.RestoreLog(BackupName(old, old.Name)) { Files = files, From = devices, Options = options },
				Statement.RestoreFileListOnly => new Ast.Statement.RestoreFileListOnly(devices) { Options = options },
				Statement.RestoreHeaderOnly   => new Ast.Statement.RestoreHeaderOnly(devices) { Options = options },
				Statement.RestoreLabelOnly    => new Ast.Statement.RestoreLabelOnly(devices) { Options = options },
				Statement.RestoreRewindOnly   => new Ast.Statement.RestoreRewindOnly(devices) { Options = options },
				Statement.RestoreVerifyOnly   => new Ast.Statement.RestoreVerifyOnly(devices) { Options = options },
				_                             => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})"),
			};
		}

		/// <summary>
		/// Partition functions and schemes, full-text catalogues and stoplists, search property lists: the old tree keeps
		/// each as a name and the words after it, read again by tokens; a type as a declared variable's, a value as a
		/// select list's.
		/// </summary>
		static Ast.Statement Defined(Statement.Definition old)
		{
			var text   = old.Tail ?? "";
			var tokens = Located(text);
			var words  = Words(text);
			var name   = Values.Part(old, old.Name);

			switch (old)
			{
				case Statement.PartitionFunctionDefinition when words is ["(", ..]:
					var close   = Closing(tokens, 0);
					var collate = tokens.FindIndex(static one => one.Token.Kind == SqlTokens.Kind.Word && one.Token.Text.Equals("COLLATE", StringComparison.OrdinalIgnoreCase));
					var type    = Slice(text, tokens, 1, collate > 0 && collate < close ? collate : close);
					var after   = words[(close + 1)..];
					var side    = after switch
					{
						["AS", "RANGE", "LEFT", ..]  => Ast.RangeSide.Left,
						["AS", "RANGE", "RIGHT", ..] => Ast.RangeSide.Right,
						_                            => (Ast.RangeSide?)null,
					};
					var values = close + (side is null ? 6 : 7);

					if (after is not ["AS", "RANGE", ..] || words.Count <= values || words[values - 2] != "VALUES" || words[values - 3] != "FOR" || words[values - 1] != "(" || words[^1] != ")")
						throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");

					var inside = Slice(text, tokens, values, tokens.Count - 1);

					return new Ast.Statement.CreatePartitionFunction(name, Values.Type(old, type), inside.Length == 0 ? [] : [.. Listed(inside).Select(one => ValueOf(old, one))])
					{
						Collation = collate > 0 && collate < close ? new Ast.CollationName(Values.Name(old, Slice(text, tokens, collate + 1, close))) : null,
						Range     = side,
					};

				case Statement.AlterPartitionFunction when words is ["(", ")", "SPLIT" or "MERGE", ..]:
					Ast.Expression? boundary = words[3..] switch
					{
						[] => null,
						["RANGE", "(", .., ")"] => ValueOf(old, Slice(text, tokens, 5, tokens.Count - 1)),
						_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					};

					return new Ast.Statement.AlterPartitionFunction(name, words[2] == "SPLIT" ? new Ast.PartitionFunctionAction.Split(boundary) : new Ast.PartitionFunctionAction.Merge(boundary));

				case Statement.PartitionSchemeDefinition when words is ["AS", "PARTITION", _, .. var rest] && rest is ["ALL", "TO", "(", .., ")"] or ["TO", "(", .., ")"]:
					var all = rest[0] == "ALL";

					return new Ast.Statement.CreatePartitionScheme(name, Values.Part(old, tokens[2].Token.Text), [.. Listed(Slice(text, tokens, all ? 6 : 5, tokens.Count - 1)).Select(one => FileGroupOf(old, one))]) { All = all };

				case Statement.AlterPartitionScheme when words is ["NEXT", "USED", .. var used] && used.Count <= 1:
					return new Ast.Statement.AlterPartitionScheme(name, used.Count == 0 ? null : FileGroupOf(old, tokens[2].Token.Text));

				case Statement.FullTextCatalogDefinition:
					var catalog = new Ast.Statement.CreateFullTextCatalog(name);
					var at      = 0;

					if (words is ["ON", "FILEGROUP", _, ..])
					{
						catalog = catalog with { FileGroup = Values.Part(old, tokens[2].Token.Text) };
						at = 3;
					}

					if (words.Count > at + 2 && words[at] == "IN" && words[at + 1] == "PATH")
					{
						catalog = catalog with { Path = Value(old, tokens[at + 2].Token) };
						at += 3;
					}

					if (Sensitivity(words, at) is { } sensitive)
					{
						catalog = catalog with { AccentSensitivity = sensitive };
						at += 4;
					}

					if (words.Count > at + 1 && words[at] == "AS" && words[at + 1] == "DEFAULT")
					{
						catalog = catalog with { Default = true };
						at += 2;
					}

					if (words.Count == at + 2 && words[at] == "AUTHORIZATION")
					{
						catalog = catalog with { Authorization = Values.Part(old, tokens[at + 1].Token.Text) };
						at += 2;
					}

					return at == words.Count ? catalog : throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");

				case Statement.AlterFullTextCatalog:
					return new Ast.Statement.AlterFullTextCatalog(name, words switch
					{
						["REBUILD"]                     => new Ast.FullTextCatalogAction.Rebuild(null),
						["REBUILD", ..] when Sensitivity(words, 1) is { } rebuilt && words.Count == 5 => new Ast.FullTextCatalogAction.Rebuild(rebuilt),
						["REORGANIZE"]                  => new Ast.FullTextCatalogAction.Reorganize(),
						["AS", "DEFAULT"]               => new Ast.FullTextCatalogAction.AsDefault(),
						_                               => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					});

				case Statement.FullTextStopListDefinition:
					var owner = Index(tokens, "AUTHORIZATION");
					var from  = words[..(owner < 0 ? words.Count : owner)] switch
					{
						[]                             => null,
						["FROM", "SYSTEM", "STOPLIST"] => new Ast.StopListSource.SystemStopList(),
						["FROM", ..]                   => (Ast.StopListSource)new Ast.StopListSource.Named(Values.Name(old, Slice(text, tokens, 1, owner < 0 ? tokens.Count : owner))),
						_                              => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					};

					return owner >= 0 && owner != tokens.Count - 2
						? throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})")
						: new Ast.Statement.CreateFullTextStopList(name) { From = from, Authorization = owner < 0 ? null : Values.Part(old, tokens[owner + 1].Token.Text) };

				case Statement.AlterFullTextStopList:
					return new Ast.Statement.AlterFullTextStopList(name, words switch
					{
						["ADD", _, "LANGUAGE", _]  => new Ast.StopListAction.Add(Value(old, tokens[1].Token), Language(old, tokens[3].Token)),
						["DROP", "ALL"]            => new Ast.StopListAction.DropAll(null),
						["DROP", "ALL", "LANGUAGE", _] => new Ast.StopListAction.DropAll(Language(old, tokens[3].Token)),
						["DROP", _, "LANGUAGE", _] => (Ast.StopListAction)new Ast.StopListAction.Drop(Value(old, tokens[1].Token), Language(old, tokens[3].Token)),
						_                          => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					});

				case Statement.SearchPropertyListDefinition:
					var by   = Index(tokens, "AUTHORIZATION");
					var list = new Ast.Statement.CreateSearchPropertyList(name) { Authorization = by < 0 ? null : Values.Part(old, tokens[by + 1].Token.Text) };

					if (by >= 0 && by != tokens.Count - 2)
						throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");

					return words[..(by < 0 ? words.Count : by)] switch
					{
						[]          => list,
						["FROM", ..] => list with { From = Values.Name(old, Slice(text, tokens, 1, by < 0 ? tokens.Count : by)) },
						_           => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					};

				case Statement.AlterSearchPropertyList:
					var with = Index(tokens, "WITH");

					return new Ast.Statement.AlterSearchPropertyList(name, words switch
					{
						["DROP", ..] => new Ast.SearchPropertyListAction.Drop(ValueOf(old, Slice(text, tokens, 1, tokens.Count))),
						["ADD", ..] when with > 1 && words[with + 1] == "(" && words[^1] == ")"
							=> (Ast.SearchPropertyListAction)new Ast.SearchPropertyListAction.Add(ValueOf(old, Slice(text, tokens, 1, with)), [.. Listed(Slice(text, tokens, with + 2, tokens.Count - 1)).Select(one => Property(old, one))]),
						_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					});

				default:
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({text})");
			}
		}

		/// <summary>The token that closes the bracket opened at a token.</summary>
		static int Closing(List<(SqlTokens.Token Token, int At)> tokens, int open)
		{
			var depth = 0;

			for (var at = open; at < tokens.Count; at++)
			{
				if (tokens[at].Token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (tokens[at].Token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" } && --depth == 0)
					return at;
			}

			return tokens.Count - 1;
		}

		/// <summary><c>WITH ACCENT_SENSITIVITY = ON | OFF</c> at a word, or null.</summary>
		static bool? Sensitivity(List<string> words, int at)
		{
			return words.Count > at + 3 && words[at] == "WITH" && words[at + 1] == "ACCENT_SENSITIVITY" && words[at + 2] == "=" && words[at + 3] is "ON" or "OFF"
				? words[at + 3] == "ON"
				: null;
		}

		/// <summary>PartitionFileGroup: a name of one part or a string.</summary>
		static Ast.Expression FileGroupOf(Statement old, string text)
		{
			return SqlTokens.Of(text) is [var one] && one.Kind is SqlTokens.Kind.String or SqlTokens.Kind.Word or SqlTokens.Kind.Delimited && !one.Text.StartsWith("@", StringComparison.Ordinal)
				? Value(old, one)
				: throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a filegroup `{text}`)");
		}

		/// <summary>StopListLanguage: a string, a number, a binary string or a name.</summary>
		static Ast.Expression Language(Statement old, SqlTokens.Token token)
		{
			return token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
				? Values.Of(new Expression.Literal(SqlLiteralKind.Hex, token.Text))
				: Value(old, token);
		}

		/// <summary>A search property's setting: a name, <c>=</c>, a value.</summary>
		static Ast.Option Property(Statement old, string text)
		{
			var tokens = Located(text);

			return tokens.Count >= 3 && tokens[0].Token.Kind == SqlTokens.Kind.Word && tokens[1].Token.Text == "="
				? new Ast.Option(Values.Part(old, tokens[0].Token.Text), ValueOf(old, Slice(text, tokens, 2, tokens.Count)), [])
				: throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a setting `{text}`)");
		}

		/// <summary>A value kept as text, read again as a select list's one value.</summary>
		static Ast.Expression ValueOf(Statement old, string text)
		{
			return TransactSql.TransactSqlParser.TryParseSql("SELECT " + text.Trim()) is { IsSuccess: true, Value: [Statement.Select { Of: Query.Specification { Columns: [Clause.DerivedColumn { Value: var value, Name: null }] } }] }
				? Values.Of(value)
				: throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a value `{text.Trim()}`)");
		}

		/// <summary>BackupFile, each between commas; files with no comma between them are a gap.</summary>
		static SqlList<Ast.BackupFile> BackupFiles(Statement old, string text)
		{
			if (text.Length == 0)
				return [];

			return [.. Listed(text).Select(piece => Words(piece) switch
			{
				["READ_WRITE_FILEGROUPS"] => new Ast.BackupFile.ReadWriteFileGroups(),
				["FILE", "=", ..]         => (Ast.BackupFile)new Ast.BackupFile.File(BackupName(old, Slice(piece, Located(piece), 2, int.MaxValue))),
				["FILEGROUP", "=", ..]    => new Ast.BackupFile.FileGroup(BackupName(old, Slice(piece, Located(piece), 2, int.MaxValue))),
				["PAGE", "=", _]          => new Ast.BackupFile.Page(Value(old, SqlTokens.Of(piece)[2])),
				_                         => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (files `{text}`)"),
			})];
		}

		/// <summary>BackupName: a name of one part or several, a variable, or a string.</summary>
		static Ast.Expression BackupName(Statement old, string text)
		{
			var tokens = SqlTokens.Of(text);

			return tokens switch
			{
				[{ Kind: SqlTokens.Kind.String } one] when one.Text[0] == '\'' => Values.String(one.Text),
				[{ Kind: SqlTokens.Kind.Word } one] when one.Text.StartsWith("@", StringComparison.Ordinal) => new Ast.Expression.Variable(new Ast.Identifier(one.Text)),
				_ when tokens.Count % 2 == 1 && tokens.Select((one, at) => at % 2 == 1 ? one.Text == "." : one.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited && !one.Text.StartsWith("@", StringComparison.Ordinal)).All(static one => one)
					=> new Ast.Expression.Reference(Values.Name(old, text)),
				_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a name `{text}`)"),
			};
		}

		/// <summary>BackupDevice, each between commas.</summary>
		static SqlList<Ast.BackupDevice> Devices(Statement old, string text)
		{
			return [.. Listed(text).Select(piece =>
			{
				var tokens = SqlTokens.Of(piece);

				return Words(piece) switch
				{
					["DISK" or "TAPE" or "URL", "=", _] => new Ast.BackupDevice.Physical(Words(piece)[0] switch { "DISK" => Ast.DeviceKind.Disk, "TAPE" => Ast.DeviceKind.Tape, _ => Ast.DeviceKind.Url }, Value(old, tokens[2])),
					[_] when tokens[0].Text.StartsWith("@", StringComparison.Ordinal) => (Ast.BackupDevice)new Ast.BackupDevice.Logical(null, new Ast.Expression.Variable(new Ast.Identifier(tokens[0].Text))),
					_ when BackupName(old, piece) is Ast.Expression.Reference { Name: var name } => new Ast.BackupDevice.Logical(name, null),
					_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a device `{piece}`)"),
				};
			})];
		}

		/// <summary>
		/// The options of a WITH or of a bracket: a name of a word or several, then nothing, <c>= value</c> or a bracketed
		/// list. MOVE's two strings and STOPATMARK's AFTER have no place in an option, and are a gap.
		/// </summary>
		static SqlList<Ast.Option> Options(Statement old, string text)
		{
			return [.. Listed(text).Select(piece =>
			{
				var tokens = Located(piece);
				var named  = 0;

				while (named < tokens.Count && tokens[named].Token.Kind == SqlTokens.Kind.Word && !tokens[named].Token.Text.StartsWith("@", StringComparison.Ordinal))
					named++;

				if (named == 0)
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} (an option `{piece}`)");

				var option = new Ast.Option(Values.Part(old, tokens[0].Token.Text), null, [])
				{
					Words = [.. tokens.Take(named).Skip(1).Select(one => Values.Part(old, one.Token.Text))],
				};
				var rest = tokens.Skip(named).Select(static one => one.Token.Text).ToList();

				return rest switch
				{
					[] => option,
					["=", _] => option with { Value = Value(old, tokens[named + 1].Token) },
					["(", .., ")"] => option with { Options = Options(old, piece[(tokens[named].At + 1)..tokens[^1].At]) },
					_ when option.Name.Text.Equals("MOVE", StringComparison.OrdinalIgnoreCase) => throw BridgeGap.Missing(old, "Option (RESTORE … MOVE 'file' TO 'path': two values)"),
					_ when rest.Contains("AFTER", StringComparer.OrdinalIgnoreCase) => throw BridgeGap.Missing(old, "Option (RESTORE … STOPATMARK = mark AFTER datetime: two values)"),
					_ => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (an option `{piece}`)"),
				};
			})];
		}

		/// <summary>One token as a value: a string, a number, a variable, NULL, or a word or a name in brackets.</summary>
		static Ast.Expression Value(Statement old, SqlTokens.Token token)
		{
			return token switch
			{
				{ Kind: SqlTokens.Kind.String }                                              => Values.String(token.Text),
				{ Kind: SqlTokens.Kind.Number }                                              => Values.Of(new Expression.Literal(SqlLiteralKind.Number, token.Text)),
				{ Kind: SqlTokens.Kind.Word } when token.Text.StartsWith("@", StringComparison.Ordinal) => new Ast.Expression.Variable(new Ast.Identifier(token.Text)),
				{ Kind: SqlTokens.Kind.Word } when token.Text.Equals("NULL", StringComparison.OrdinalIgnoreCase) => new Ast.LiteralValue.Null(),
				{ Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited }                     => new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(old, token.Text)])),
				_                                                                             => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a value `{token.Text}`)"),
			};
		}

		/// <summary>KeyPlace at a token: FILE or URL, <c>=</c>, a string.</summary>
		static Ast.KeyPlace Place(Statement old, List<(SqlTokens.Token Token, int At)> tokens, int at)
		{
			return new Ast.KeyPlace(tokens[at].Token.Text.ToUpperInvariant() switch
			{
				"FILE" => Ast.KeyPlaceKind.File,
				"URL"  => Ast.KeyPlaceKind.Url,
				_      => throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({tokens[at].Token.Text})"),
			}, Password(old, tokens, at + 2));
		}

		/// <summary>A string at a token, as a path or a password is written.</summary>
		static Ast.Expression Password(Statement old, List<(SqlTokens.Token Token, int At)> tokens, int at)
		{
			return tokens[at].Token.Kind == SqlTokens.Kind.String ? Values.String(tokens[at].Token.Text) : throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a password or path `{tokens[at].Token.Text}`)");
		}

		/// <summary>ALTER DATABASE's target: CURRENT, the word, or a name.</summary>
		static Ast.Statement.AlterDatabase Target(Statement old, string name, Ast.AlterDatabaseAction action)
		{
			return name == "CURRENT"
				? new Ast.Statement.AlterDatabase(null, action) { Current = true }
				: new Ast.Statement.AlterDatabase(Values.Part(old, name), action);
		}

		/// <summary>File specifications kept as text, read again as CREATE DATABASE's data files.</summary>
		static SqlList<Ast.DatabaseFile> Files(Statement old, string text)
		{
			return TransactSql.TransactSqlParser.TryParseSql("CREATE DATABASE d ON " + text) is { IsSuccess: true, Value: [Statement.CreateDatabase { Files: var files, Log: null }] }
				&& files.All(static one => one is Clause.DatabaseFile)
				? Each(files, one => File(old, (Clause.DatabaseFile)one))
				: throw BridgeGap.Open(old, $"inventory {NameOf(old)} (files `{text}`)");
		}

		static Ast.DatabaseFile File(Statement old, Clause.DatabaseFile file)
		{
			return new Ast.DatabaseFile(Each(file.Options, one => one is Clause.Option option ? Tables.Option(option) : throw BridgeGap.Open(old, $"inventory {NameOf(one)} (in a file)")));
		}

		/// <summary>
		/// A statement made of a tail the old tree keeps as written and compares exactly: only one whose text, written and
		/// read again, keeps the same tail carries over.
		/// </summary>
		static Ast.Statement Checked(Statement.Definition old, Ast.Statement made)
		{
			string text;

			try
			{
				text = Ast.TransactSqlWriter.Write(made);
			}
			catch (Exception refused) when (refused is ArgumentException or NotSupportedException)
			{
				return made;
			}

			return TransactSql.TransactSqlParser.TryParseSql(text) is { IsSuccess: true, Value: [Statement.Definition { Tail: var tail }] } && tail == old.Tail
				? made
				: throw BridgeGap.Open(old, $"Trees.FreeText ({NameOf(old)}.Tail, written otherwise than the writer prints it)");
		}

		/// <summary>The tokens of a text and where each begins.</summary>
		static List<(SqlTokens.Token Token, int At)> Located(string text)
		{
			var located = new List<(SqlTokens.Token, int)>();
			var at      = 0;

			foreach (var token in SqlTokens.Of(text))
			{
				var found = text.IndexOf(token.Text, at, StringComparison.Ordinal);

				located.Add((token, found));
				at = found + token.Text.Length;
			}

			return located;
		}

		/// <summary>The first word outside brackets that is <paramref name="word"/>, or -1.</summary>
		static int Index(List<(SqlTokens.Token Token, int At)> tokens, string word, int from = 0)
		{
			var depth = 0;

			for (var at = 0; at < tokens.Count; at++)
			{
				var token = tokens[at].Token;

				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" })
					depth--;
				else if (at >= from && depth == 0 && token.Kind == SqlTokens.Kind.Word && token.Text.Equals(word, StringComparison.OrdinalIgnoreCase))
					return at;
			}

			return -1;
		}

		/// <summary>The text from the token <paramref name="from"/> up to the token <paramref name="to"/>, trimmed.</summary>
		static string Slice(string text, List<(SqlTokens.Token Token, int At)> tokens, int from, int to)
		{
			if (from >= tokens.Count || from >= to)
				return "";

			var end = to < tokens.Count ? tokens[to].At : text.Length;

			return text[tokens[from].At..end].Trim();
		}

		/// <summary>A text cut at its commas outside brackets.</summary>
		static List<string> Listed(string text)
		{
			var pieces = new List<string>();
			var depth  = 0;
			var from   = 0;

			foreach (var (token, at) in Located(text))
			{
				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" })
					depth--;
				else if (depth == 0 && token is { Kind: SqlTokens.Kind.Punctuation, Text: "," })
				{
					pieces.Add(text[from..at].Trim());
					from = at + 1;
				}
			}

			pieces.Add(text[from..].Trim());

			return pieces;
		}
	}
}
