using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Build
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
				_ when Owned(Owns, old) => throw Unconverted(old),
				_ => null,
			};

			return made is not null;
		}

		/// <summary>What is backed up, on the common tree: the words that say what, the name, the words after it.</summary>
		internal static Statement BackedUp(string what, string? name, string? tail, SqlSpan at)
		{
			var record = what switch
			{
				"DATABASE"           => "Statement.BackupDatabase",
				"LOG"                => "Statement.BackupTransactionLog",
				"SERVER"             => "Statement.BackupServer",
				"GROUP"              => "Statement.BackupGroup",
				"CERTIFICATE"        => "Statement.BackupCertificate",
				"MASTER KEY"         => "Statement.BackupMasterKey",
				"SERVICE MASTER KEY" => "Statement.BackupServiceMasterKey",
				"SYMMETRIC KEY"      => "Statement.BackupSymmetricKey",
				_                    => throw Syntax.Unknown(what),
			};
			var old = new Defining(record, name ?? "") { Tail = tail };

			return Defer(old, at, static old => Checked(old, Backup(old)));
		}

		/// <summary>What is restored, likewise.</summary>
		internal static Statement Restored(string what, string? name, string? tail, SqlSpan at)
		{
			var record = what switch
			{
				"DATABASE"           => "Statement.RestoreDatabase",
				"LOG"                => "Statement.RestoreLog",
				"FILELISTONLY"       => "Statement.RestoreFileListOnly",
				"HEADERONLY"         => "Statement.RestoreHeaderOnly",
				"LABELONLY"          => "Statement.RestoreLabelOnly",
				"REWINDONLY"         => "Statement.RestoreRewindOnly",
				"VERIFYONLY"         => "Statement.RestoreVerifyOnly",
				"MASTER KEY"         => "Statement.RestoreMasterKey",
				"SERVICE MASTER KEY" => "Statement.RestoreServiceMasterKey",
				"SYMMETRIC KEY"      => "Statement.RestoreSymmetricKey",
				_                    => throw Syntax.Unknown(what),
			};
			var old = new Defining(record, name ?? "") { Tail = tail };

			return Defer(old, at, static old => Checked(old, Restore(old)));
		}

		/// <summary>
		/// An ALTER DATABASE action other than SET and MODIFY's edition options, on the common tree: the database, the
		/// action's words and what was written after them.
		/// </summary>
		internal static Statement OfDatabase(string name, string action, string? tail)
		{
			var record = action switch
			{
				"COLLATE"                         => "Statement.AlterDatabaseCollate",
				"MODIFY NAME"                     => "Statement.AlterDatabaseModifyName",
				"MODIFY FILEGROUP"                => "Statement.AlterDatabaseModifyFileGroup",
				"MODIFY FILE"                     => "Statement.AlterDatabaseModifyFile",
				"MODIFY"                          => "Statement.AlterDatabaseModify",
				"ADD FILEGROUP"                   => "Statement.AlterDatabaseAddFileGroup",
				"ADD LOG FILE"                    => "Statement.AlterDatabaseAddLogFile",
				"ADD FILE"                        => "Statement.AlterDatabaseAddFile",
				"REMOVE FILEGROUP"                => "Statement.AlterDatabaseRemoveFileGroup",
				"REMOVE FILE"                     => "Statement.AlterDatabaseRemoveFile",
				"REBUILD LOG"                     => "Statement.AlterDatabaseRebuildLog",
				"PERFORM_CUTOVER"                 => "Statement.AlterDatabasePerformCutover",
				"MODIFY BACKUP_STORAGE_REDUNDANCY" => "Statement.AlterDatabaseModifyBackupStorageRedundancy",
				_                                 => throw Syntax.Unknown(action),
			};
			var old = new Defining(record, name) { Tail = Syntax.Tail(tail) };

			return Hold(old, static old => Alter(old));
		}

		/// <summary>ALTER DATABASE … SET, on the common tree: the database, its settings and how open transactions end.</summary>
		internal static Statement AlterSet(string name, SqlList<Clause> settings, string? termination)
		{
			var old = new Defining("Statement.AlterDatabaseSet", name);

			return Hold(old, (settings, termination), static (old, state) => Target(old, old.Name, new Ast.AlterDatabaseAction.Set(Each(state.settings, one => Setting(old, one)))) with { Termination = Termination(old, state.termination) });
		}

		/// <summary>ALTER DATABASE … MODIFY with edition options, on the common tree.</summary>
		internal static Statement Modify(string name, SqlList<Clause>? options, SqlList<Clause>? with, SqlSpan at)
		{
			var old = new Defining("Statement.AlterDatabaseModify", name);

			return Defer(old, at, old => AlterModify(old, options, with));
		}

		/// <summary>ALTER DATABASE SCOPED CONFIGURATION, on the common tree, from what its rules read.</summary>
		internal static Statement Scoped(Statement.AlterDatabaseScopedConfiguration read)
		{
			return Hold(read, static read => Scoped(read, read));
		}

		/// <summary>Azure's CREATE DATABASE, on the common tree: the name, the collation, the edition options and the WITH.</summary>
		internal static Statement Create(string name, string? collation, SqlList<Clause>? options, SqlList<Clause>? with)
		{
			return Create(new Statement.CreateDatabase(name, Clause.None, Tail: "", Options: options, With: with) { Collation = collation });
		}

		/// <summary>
		/// CREATE DATABASE, on the common tree: the name, the containment, the files and the collation, and the tail that
		/// says what the database is made from, as its rules gathered them.
		/// </summary>
		internal static Statement Create(string name, string? containment, string? collation, Statement.CreateDatabase? files, Statement.CreateDatabase? tail)
		{
			return Create((tail ?? new Statement.CreateDatabase("", Clause.None)) with
			{
				Name = name, Containment = containment, Collation = collation,
				Files = files?.Files ?? Clause.None, Primary = files?.Primary ?? false, Log = files?.Log,
			});
		}

		static Statement Create(Statement.CreateDatabase read)
		{
			return Hold(read, static read => Create(read, read));
		}

		/// <summary>
		/// Partition functions and schemes, full-text catalogues and stoplists, search property lists, on the common tree
		/// (<see cref="Build.Defined"/>).
		/// </summary>
		internal static Ast.Statement Definition(Defining old)
		{
			return Checked(old, Defined(old));
		}

		/// <summary>
		/// ALTER DATABASE's actions other than SET, which the old tree keeps as a record each and the words after the action
		/// as text: read again by tokens, and a file specification as CREATE DATABASE's. The termination the grammar reads
		/// after any of them, which the old tree keeps at the end of the text, is the statement's.
		/// </summary>
		static Ast.Statement Alter(Defining old)
		{
			var tail   = old.Tail ?? "";
			var tokens = Located(tail);

			// DatabaseTermination, the last WITH of the text, followed by ROLLBACK or NO_WAIT.
			Ast.Termination? termination = null;
			var with = tokens.FindLastIndex(static one => one.Token.Kind == SqlTokens.Kind.Word && one.Token.Text.Equals("WITH", StringComparison.OrdinalIgnoreCase));

			if (with >= 0 && (Is(tokens, with + 1, "ROLLBACK") || Is(tokens, with + 1, "NO_WAIT")))
			{
				termination = Termination(old, tail[tokens[with].At..]);
				tail        = tail[..tokens[with].At].TrimEnd();
				tokens      = Located(tail);
			}

			var words  = Words(tail);

			Ast.AlterDatabaseAction action = old.Record switch
			{
				"Statement.AlterDatabaseCollate" => new Ast.AlterDatabaseAction.Collate(new Ast.CollationName(Values.Name(old, tail))),
				"Statement.AlterDatabaseModifyName" when words is ["=", _] => new Ast.AlterDatabaseAction.ModifyName(Values.Part(old, tokens[1].Token.Text)),
				"Statement.AlterDatabaseModifyBackupStorageRedundancy" when words is ["=", _] && tokens[1].Token.Kind == SqlTokens.Kind.String
					=> new Ast.AlterDatabaseAction.ModifyBackupStorageRedundancy(Values.String(tokens[1].Token.Text)),
				"Statement.AlterDatabasePerformCutover" when tail.Length == 0 => new Ast.AlterDatabaseAction.PerformCutover(),
				"Statement.AlterDatabaseRebuildLog" when tail.Length == 0 => new Ast.AlterDatabaseAction.RebuildLog(),
				"Statement.AlterDatabaseRebuildLog" when Is(tokens, 0, "ON") && tokens.Count > 1
					=> new Ast.AlterDatabaseAction.RebuildLog { On = Files(old, tail[tokens[1].At..]) is [var log] ? log : throw BuildGap.Open(old, $"inventory {NameOf(old)} ({tail})") },
				"Statement.AlterDatabaseAddFile" => AddFile(old, tail, tokens, log: false),
				"Statement.AlterDatabaseAddLogFile" => AddFile(old, tail, tokens, log: true),
				"Statement.AlterDatabaseModifyFile" => new Ast.AlterDatabaseAction.ModifyFile(Files(old, tail) is [var file] ? file : throw BuildGap.Open(old, $"inventory {NameOf(old)} ({tail})")),
				"Statement.AlterDatabaseRemoveFile" when tokens.Count == 1 => new Ast.AlterDatabaseAction.RemoveFile(Values.Part(old, tail)),
				"Statement.AlterDatabaseRemoveFileGroup" when tokens.Count == 1 => new Ast.AlterDatabaseAction.RemoveFileGroup(Values.Part(old, tail)),
				"Statement.AlterDatabaseAddFileGroup" => words switch
				{
					[_] => new Ast.AlterDatabaseAction.AddFileGroup(Values.Part(old, tokens[0].Token.Text), null),
					[_, "CONTAINS", "FILESTREAM"] => new Ast.AlterDatabaseAction.AddFileGroup(Values.Part(old, tokens[0].Token.Text), Ast.FileGroupContents.Filestream),
					[_, "CONTAINS", "MEMORY_OPTIMIZED_DATA"] => new Ast.AlterDatabaseAction.AddFileGroup(Values.Part(old, tokens[0].Token.Text), Ast.FileGroupContents.MemoryOptimizedData),
					_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({tail})"),
				},
				"Statement.AlterDatabaseModifyFileGroup" when tokens.Count >= 2 => new Ast.AlterDatabaseAction.ModifyFileGroup(Values.Part(old, tokens[0].Token.Text), words[1..] switch
				{
					["READONLY"]             => new Ast.FileGroupChange.Updatability(Ast.FileGroupUpdatability.ReadOnly, false),
					["READ_ONLY"]            => new Ast.FileGroupChange.Updatability(Ast.FileGroupUpdatability.ReadOnly, true),
					["READWRITE"]            => new Ast.FileGroupChange.Updatability(Ast.FileGroupUpdatability.ReadWrite, false),
					["READ_WRITE"]           => new Ast.FileGroupChange.Updatability(Ast.FileGroupUpdatability.ReadWrite, true),
					["DEFAULT"]              => new Ast.FileGroupChange.Default(),
					["AUTOGROW_SINGLE_FILE"] => new Ast.FileGroupChange.AutoGrow(Ast.FileGroupAutoGrow.SingleFile),
					["AUTOGROW_ALL_FILES"]   => new Ast.FileGroupChange.AutoGrow(Ast.FileGroupAutoGrow.AllFiles),
					["NAME", "=", _]         => new Ast.FileGroupChange.Rename(Values.Part(old, tokens[3].Token.Text)),
					_                        => throw BuildGap.Open(old, $"inventory {NameOf(old)} (MODIFY FILEGROUP … {tail})"),
				}),
				_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({tail})"),
			};

			return Checked(old, Target(old, old.Name, action) with { Termination = termination });
		}

		/// <summary>Whether the token at a place is the word given, whatever its case.</summary>
		static bool Is(List<(SqlTokens.Token Token, int At)> tokens, int at, string word)
		{
			return at >= 0 && at < tokens.Count && tokens[at].Token.Kind == SqlTokens.Kind.Word && tokens[at].Token.Text.Equals(word, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>ADD FILE and ADD LOG FILE: the files, and the filegroup after TO FILEGROUP.</summary>
		static Ast.AlterDatabaseAction AddFile(ISqlSpan old, string tail, List<(SqlTokens.Token Token, int At)> tokens, bool log)
		{
			var to    = Index(tokens, "TO");
			var files = Files(old, to < 0 ? tail : Slice(tail, tokens, 0, to));

			if (to < 0)
				return log ? new Ast.AlterDatabaseAction.AddLogFile(files) : new Ast.AlterDatabaseAction.AddFile(files, null);

			if (Words(Slice(tail, tokens, to, tokens.Count)) is not ["TO", "FILEGROUP", _])
				throw BuildGap.Open(old, $"inventory {NameOf(old)} ({tail})");

			var group = Values.Part(old, tokens[^1].Token.Text);

			return log ? new Ast.AlterDatabaseAction.AddLogFile(files) { ToFileGroup = group } : new Ast.AlterDatabaseAction.AddFile(files, group);
		}

		/// <summary>
		/// The Azure MODIFY: its edition options and MANUAL_CUTOVER. The old tree drops a termination after it, which the
		/// text has; such a text has no member and stays a gap.
		/// </summary>
		static Ast.Statement AlterModify(Defining old, SqlList<Clause>? options, SqlList<Clause>? with)
		{
			if (Spelled(old) is { } text && Words(text) is var words && (words.Contains("ROLLBACK") || words.Contains("NO_WAIT")))
				throw BuildGap.Lost(old, "Statement.AlterDatabaseModify (a termination after MODIFY)");

			var action = new Ast.AlterDatabaseAction.Modify(Each(options ?? [], one => one is Clause.Option option ? Tables.Option(option) : throw BuildGap.Open(one, $"inventory {NameOf(one)}")), with is { Length: > 0 });

			return Target(old, old.Name, action);
		}

		/// <summary>
		/// One setting of ALTER DATABASE … SET or of a scoped configuration, which the old tree keeps as a name of words, a
		/// value and the list in brackets after it. A word the setting takes with no <c>=</c> and nothing after it is kept in
		/// the name: the last word is the value, but for <c>QUERY_STORE CLEAR [ALL]</c>, which is a name of several words
		/// and nothing more. The form, <c>=</c> or none, is the catalogue's but for the one setting read both ways.
		/// </summary>
		static Ast.Option Setting(ISqlSpan old, Clause clause)
		{
			if (clause is not Clause.Option(var name, var value, var options, null, var bare))
				throw BuildGap.Open(old, $"inventory {NameOf(clause)} (in {NameOf(old)})");

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
					Values.Measured(var v, var unit) => new Ast.Expression.Measured(Values.Of(v), new Ast.Identifier(unit)),
					_                                   => Values.Of(value),
				};

			return new Ast.Option(Values.Part(old, words[0]), made, Each(options, one => Setting(old, one)))
			{
				Words         = [.. words.Skip(1).Select(word => Values.Part(old, word))],
				EqualsOmitted = bare && upper is ["MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT"],
			};
		}

		/// <summary>The termination after SET, which the old tree keeps as its words.</summary>
		static Ast.Termination? Termination(ISqlSpan old, string? text)
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
				_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} (termination `{text}`)"),
			};
		}

		/// <summary>ALTER DATABASE SCOPED CONFIGURATION: SET and its one setting, or CLEAR PROCEDURE_CACHE and its plan handle.</summary>
		static Ast.Statement Scoped(ISqlSpan old, Statement.AlterDatabaseScopedConfiguration read)
		{
			Ast.ScopedConfigurationAction action = read switch
			{
				{ Action: "SET", Settings: [var setting], Argument: null } => new Ast.ScopedConfigurationAction.Set(Setting(old, setting)) { Secondary = read.Secondary },
				{ Action: "CLEAR PROCEDURE_CACHE", Settings: [], Secondary: false } => new Ast.ScopedConfigurationAction.ClearProcedureCache(
					read.Argument is null ? null : Values.Of(read.Argument) as Ast.LiteralValue.Binary ?? throw BuildGap.Open(old, $"inventory {NameOf(old)} (a plan handle that is not binary)")),
				_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({read.Action})"),
			};

			return new Ast.Statement.AlterDatabaseScopedConfiguration(action);
		}

		/// <summary>
		/// CREATE DATABASE: the files and filegroups the old tree keeps in one list, the log's, the containment and
		/// collation as their words, the edition options, and the tail that says what the database is made from: empty
		/// for Azure's form, the attach's words, or the words of a snapshot or a copy and the database they name.
		/// </summary>
		static Ast.Statement Create(ISqlSpan old, Statement.CreateDatabase read)
		{
			var files  = new List<Ast.DatabaseFile>();
			var groups = new List<Ast.FileGroup>();

			foreach (var one in read.Files)
			{
				switch (one)
				{
					case Clause.DatabaseFile file when groups.Count == 0:
						files.Add(File(old, file));
						break;

					case Clause.FileGroup(var name, var contains, var fallback, var held):
						groups.Add(new Ast.FileGroup(Values.Part(old, name), Each(held, file => file is Clause.DatabaseFile each ? File(old, each) : throw BuildGap.Open(old, $"inventory {NameOf(file)} (in a filegroup)")))
						{
							Contains = contains is null ? null : Words(contains) switch
							{
								["FILESTREAM"]            => Ast.FileGroupContents.Filestream,
								["MEMORY_OPTIMIZED_DATA"] => Ast.FileGroupContents.MemoryOptimizedData,
								_                         => throw BuildGap.Open(old, $"inventory Clause.FileGroup (CONTAINS {contains})"),
							},
							Default = fallback,
						});
						break;

					default:
						throw BuildGap.Open(old, $"inventory {NameOf(one)} (in CREATE DATABASE's files)");
				}
			}

			var tokens = SqlTokens.Of(read.Tail ?? "").ToList();

			Ast.DatabaseSource? source = Words(read.Tail ?? "") switch
			{
				[]                               => null,
				["FOR", "ATTACH"]                => new Ast.DatabaseSource.ForAttach(Ast.AttachKind.Attach),
				["FOR", "ATTACH_REBUILD_LOG"]    => new Ast.DatabaseSource.ForAttach(Ast.AttachKind.AttachRebuildLog),
				["FOR", "ATTACH_FORCE_REBUILD_LOG"] => new Ast.DatabaseSource.ForAttach(Ast.AttachKind.AttachForceRebuildLog),
				["AS", "SNAPSHOT", "OF", _]      => new Ast.DatabaseSource.AsSnapshotOf(Values.Part(old, tokens[3].Text)),
				["AS", "COPY", "OF", ..]         => new Ast.DatabaseSource.AsCopyOf(Values.Name(old, string.Concat(tokens.Skip(3).Select(static one => one.Text)))),
				_                                => throw BuildGap.Open(old, $"inventory Statement.CreateDatabase (tail `{read.Tail}`)"),
			};

			var made = new Ast.Statement.CreateDatabase(Values.Part(old, read.Name))
			{
				Containment = read.Containment is null ? null : Words(read.Containment) switch
				{
					["NONE"]    => Ast.DatabaseContainment.None,
					["PARTIAL"] => Ast.DatabaseContainment.Partial,
					_           => throw BuildGap.Open(old, $"inventory Statement.CreateDatabase (CONTAINMENT = {read.Containment})"),
				},
				Primary        = read.Primary,
				Files          = [.. files],
				FileGroups     = [.. groups],
				LogFiles       = Each(read.Log ?? [], one => one is Clause.DatabaseFile file ? File(old, file) : throw BuildGap.Open(old, $"inventory {NameOf(one)} (in LOG ON)")),
				Collation      = read.Collation is null ? null : new Ast.CollationName(Values.Name(old, read.Collation)),
				EditionOptions = Each(read.Options ?? [], one => one is Clause.Option option ? Tables.Option(option) : throw BuildGap.Open(one, $"inventory {NameOf(one)}")),
				Source         = source,
				Options        = Each(read.With ?? [], one => Setting(old, one)),
			};

			// The containment and the tail are kept as written and compared token by token: a text that differs from the
			// writer's only by the case of its key words or its blanks carries over, any other stays a gap.
			string text;

			try
			{
				text = Ast.TransactSqlWriter.Write(made);
			}
			catch (Exception refused) when (refused is ArgumentException or NotSupportedException)
			{
				return made;
			}

			return Skim(text) is [Statement.Built { Given: Statement.CreateDatabase again }]
				&& Kept(old, "Statement.CreateDatabase.Containment", again.Containment, read.Containment) && Kept(old, "Statement.CreateDatabase.Tail", again.Tail, read.Tail)
				? made
				: throw BuildGap.Open(old, "Trees.FreeText (Statement.CreateDatabase.Containment or Tail, written otherwise than the writer prints it)");
		}

		/// <summary>
		/// BACKUP: the old tree keeps what is backed up as its record, the database as its name, and the rest as text —
		/// the files, the devices after TO and after each MIRROR TO, the options after WITH; a key's or a certificate's
		/// place and passwords. Read again by tokens.
		/// </summary>
		static Ast.Statement Backup(Defining old)
		{
			var text   = old.Tail ?? "";
			var tokens = Located(text);
			var words  = Words(text);

			if (Spelled(old) is { } spelled && Words(spelled) is [_, "DATABASE" or "LOG", "(", ..])
				throw BuildGap.Lost(old, $"{NameOf(old)} (a bracketed list of names, of which the old tree keeps the first)");

			switch (old)
			{
				case { Record: "Statement.BackupCertificate" }:
					if (words is not ["TO", "FILE", "=", _, ..] || tokens[3].Token.Kind != SqlTokens.Kind.String)
						throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");

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
						: throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");

				case { Record: "Statement.BackupMasterKey" } when words is ["TO", _, "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _]:
					return new Ast.Statement.BackupMasterKey(Place(old, tokens, 1), Password(old, tokens, 8));

				case { Record: "Statement.BackupServiceMasterKey" } when words is ["TO", "FILE", "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _]:
					return new Ast.Statement.BackupServiceMasterKey(Password(old, tokens, 3), Password(old, tokens, 8));

				case { Record: "Statement.BackupSymmetricKey" } when words is ["TO", _, "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _]:
					return new Ast.Statement.BackupSymmetricKey(Values.Part(old, old.Name), Place(old, tokens, 1), Password(old, tokens, 8));

				case { Record: "Statement.BackupMasterKey" or "Statement.BackupServiceMasterKey" or "Statement.BackupSymmetricKey" }:
					throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");
			}

			var to = Index(tokens, "TO");

			if (to < 0)
				throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");

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
					throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");

				mirrors.Add(new Ast.BackupMirror(Devices(old, Slice(text, tokens, marks[one] + 2, one + 1 < marks.Count ? marks[one + 1] : end))));
			}

			var options = with < 0 ? [] : Options(old, Slice(text, tokens, with + 1, tokens.Count));

			if (files.Count > 0 && old.Record is not "Statement.BackupDatabase")
				throw BuildGap.Missing(old, $"{NameOf(old)} (files, which only BACKUP DATABASE's syntax names)");

			return old.Record switch
			{
				"Statement.BackupDatabase"       => new Ast.Statement.BackupDatabase(BackupName(old, old.Name), devices) { Files = files, Mirrors = [.. mirrors], Options = options },
				"Statement.BackupTransactionLog" => new Ast.Statement.BackupLog(BackupName(old, old.Name), devices) { Mirrors = [.. mirrors], Options = options },
				"Statement.BackupServer" when old.Name.Length == 0 => new Ast.Statement.BackupServer(devices) { Mirrors = [.. mirrors], Options = options },
				"Statement.BackupGroup"          => new Ast.Statement.BackupGroup([.. Listed(old.Name).Select(one => BackupName(old, one))], devices) { Mirrors = [.. mirrors], Options = options },
				_                              => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({old.Name})"),
			};
		}

		/// <summary>
		/// RESTORE: likewise — the files, the devices or the snapshot after FROM, the options after WITH; a key's place,
		/// its passwords and FORCE.
		/// </summary>
		static Ast.Statement Restore(Defining old)
		{
			var text   = old.Tail ?? "";
			var tokens = Located(text);
			var words  = Words(text);

			if (Spelled(old) is { } spelled && Words(spelled) is [_, "DATABASE" or "LOG", "(", ..])
				throw BuildGap.Lost(old, $"{NameOf(old)} (a bracketed list of names, of which the old tree keeps the first)");

			switch (old)
			{
				case { Record: "Statement.RestoreMasterKey" } when words is ["FROM", _, "=", _, "DECRYPTION", "BY", "PASSWORD", "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _, .. var force] && force is [] or ["FORCE"]:
					return new Ast.Statement.RestoreMasterKey(Place(old, tokens, 1), Password(old, tokens, 8), Password(old, tokens, 13)) { Force = force.Count == 1 };

				case { Record: "Statement.RestoreServiceMasterKey" } when words is ["FROM", "FILE", "=", _, "DECRYPTION", "BY", "PASSWORD", "=", _, .. var force] && force is [] or ["FORCE"]:
					return new Ast.Statement.RestoreServiceMasterKey(Password(old, tokens, 3), Password(old, tokens, 8)) { Force = force.Count == 1 };

				case { Record: "Statement.RestoreSymmetricKey" } when words is ["FROM", _, "=", _, "DECRYPTION", "BY", "PASSWORD", "=", _, "ENCRYPTION", "BY", "PASSWORD", "=", _, .. var force] && force is [] or ["FORCE"]:
					return new Ast.Statement.RestoreSymmetricKey(Values.Part(old, old.Name), Place(old, tokens, 1), Password(old, tokens, 8), Password(old, tokens, 13)) { Force = force.Count == 1 };

				case { Record: "Statement.RestoreMasterKey" or "Statement.RestoreServiceMasterKey" or "Statement.RestoreSymmetricKey" }:
					throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");
			}

			var from  = Index(tokens, "FROM");
			var with  = Index(tokens, "WITH");
			var files = BackupFiles(old, Slice(text, tokens, 0, from >= 0 ? from : with >= 0 ? with : tokens.Count));
			var end   = with < 0 ? tokens.Count : with;

			SqlList<Ast.BackupDevice> devices = [];
			Ast.Expression? snapshot = null;

			if (from >= 0 && words.Count > from + 1 && words[from + 1] == "DATABASE_SNAPSHOT")
				snapshot = end == from + 4 && words[from + 2] == "=" ? Value(old, tokens[from + 3].Token) : throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");
			else if (from >= 0)
				devices = Devices(old, Slice(text, tokens, from + 1, end));

			var options = with < 0 ? [] : Options(old, Slice(text, tokens, with + 1, tokens.Count));

			if (old.Record is not ("Statement.RestoreDatabase" or "Statement.RestoreLog") && (old.Name.Length > 0 || files.Count > 0))
				throw BuildGap.Missing(old, $"{NameOf(old)} (a database or files, which its syntax does not name)");

			if (old.Record is "Statement.RestoreDatabase" or "Statement.RestoreLog" && old.Name.Length == 0)
				throw BuildGap.Missing(old, $"{NameOf(old)} (no database, which its syntax names)");

			if (snapshot is not null && old.Record is not "Statement.RestoreDatabase")
				throw BuildGap.Missing(old, $"{NameOf(old)} (FROM DATABASE_SNAPSHOT, which only RESTORE DATABASE's syntax names)");

			return old.Record switch
			{
				"Statement.RestoreDatabase"     => new Ast.Statement.RestoreDatabase(BackupName(old, old.Name)) { Files = files, From = devices, DatabaseSnapshot = snapshot, Options = options },
				"Statement.RestoreLog"          => new Ast.Statement.RestoreLog(BackupName(old, old.Name)) { Files = files, From = devices, Options = options },
				"Statement.RestoreFileListOnly" => new Ast.Statement.RestoreFileListOnly(devices) { Options = options },
				"Statement.RestoreHeaderOnly"   => new Ast.Statement.RestoreHeaderOnly(devices) { Options = options },
				"Statement.RestoreLabelOnly"    => new Ast.Statement.RestoreLabelOnly(devices) { Options = options },
				"Statement.RestoreRewindOnly"   => new Ast.Statement.RestoreRewindOnly(devices) { Options = options },
				"Statement.RestoreVerifyOnly"   => new Ast.Statement.RestoreVerifyOnly(devices) { Options = options },
				_                             => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})"),
			};
		}

		/// <summary>
		/// Partition functions and schemes, full-text catalogues and stoplists, search property lists: the old tree keeps
		/// each as a name and the words after it, read again by tokens; a type as a declared variable's, a value as a
		/// select list's.
		/// </summary>
		static Ast.Statement Defined(Defining old)
		{
			var text   = old.Tail ?? "";
			var tokens = Located(text);
			var words  = Words(text);
			var name   = Values.Part(old, old.Name);

			switch (old)
			{
				case { Record: "Statement.PartitionFunctionDefinition" } when words is ["(", ..]:
					var close   = Closing(tokens, 0);

					// A bracket the words kept do not close: a line comment among them, which the words kept as one line run to their end.
					if (close < 0)
						throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");

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
						throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");

					var inside = Slice(text, tokens, values, tokens.Count - 1);

					return new Ast.Statement.CreatePartitionFunction(name, Values.Type(old, type), inside.Length == 0 ? [] : [.. Listed(inside).Select(one => ValueOf(old, one))])
					{
						Collation = collate > 0 && collate < close ? new Ast.CollationName(Values.Name(old, Slice(text, tokens, collate + 1, close))) : null,
						Range     = side,
					};

				case { Record: "Statement.AlterPartitionFunction" } when words is ["(", ")", "SPLIT" or "MERGE", ..]:
					Ast.Expression? boundary = words[3..] switch
					{
						[] => null,
						["RANGE", "(", .., ")"] => ValueOf(old, Slice(text, tokens, 5, tokens.Count - 1)),
						_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					};

					return new Ast.Statement.AlterPartitionFunction(name, words[2] == "SPLIT" ? new Ast.PartitionFunctionAction.Split(boundary) : new Ast.PartitionFunctionAction.Merge(boundary));

				case { Record: "Statement.PartitionSchemeDefinition" } when words is ["AS", "PARTITION", _, .. var rest] && rest is ["ALL", "TO", "(", .., ")"] or ["TO", "(", .., ")"]:
					var all = rest[0] == "ALL";

					return new Ast.Statement.CreatePartitionScheme(name, Values.Part(old, tokens[2].Token.Text), [.. Listed(Slice(text, tokens, all ? 6 : 5, tokens.Count - 1)).Select(one => FileGroupOf(old, one))]) { All = all };

				case { Record: "Statement.AlterPartitionScheme" } when words is ["NEXT", "USED", .. var used] && used.Count <= 1:
					return new Ast.Statement.AlterPartitionScheme(name, used.Count == 0 ? null : FileGroupOf(old, tokens[2].Token.Text));

				case { Record: "Statement.FullTextCatalogDefinition" }:
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

					return at == words.Count ? catalog : throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");

				case { Record: "Statement.AlterFullTextCatalog" }:
					return new Ast.Statement.AlterFullTextCatalog(name, words switch
					{
						["REBUILD"]                     => new Ast.FullTextCatalogAction.Rebuild(null),
						["REBUILD", ..] when Sensitivity(words, 1) is { } rebuilt && words.Count == 5 => new Ast.FullTextCatalogAction.Rebuild(rebuilt),
						["REORGANIZE"]                  => new Ast.FullTextCatalogAction.Reorganize(),
						["AS", "DEFAULT"]               => new Ast.FullTextCatalogAction.AsDefault(),
						_                               => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					});

				case { Record: "Statement.FullTextStopListDefinition" }:
					var owner = Index(tokens, "AUTHORIZATION");
					var from  = words[..(owner < 0 ? words.Count : owner)] switch
					{
						[]                             => null,
						["FROM", "SYSTEM", "STOPLIST"] => new Ast.StopListSource.SystemStopList(),
						["FROM", ..]                   => (Ast.StopListSource)new Ast.StopListSource.Named(Values.Name(old, Slice(text, tokens, 1, owner < 0 ? tokens.Count : owner))),
						_                              => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					};

					return owner >= 0 && owner != tokens.Count - 2
						? throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})")
						: new Ast.Statement.CreateFullTextStopList(name) { From = from, Authorization = owner < 0 ? null : Values.Part(old, tokens[owner + 1].Token.Text) };

				case { Record: "Statement.AlterFullTextStopList" }:
					return new Ast.Statement.AlterFullTextStopList(name, words switch
					{
						["ADD", _, "LANGUAGE", _]  => new Ast.StopListAction.Add(Value(old, tokens[1].Token), Language(old, tokens[3].Token)),
						["DROP", "ALL"]            => new Ast.StopListAction.DropAll(null),
						["DROP", "ALL", "LANGUAGE", _] => new Ast.StopListAction.DropAll(Language(old, tokens[3].Token)),
						["DROP", _, "LANGUAGE", _] => (Ast.StopListAction)new Ast.StopListAction.Drop(Value(old, tokens[1].Token), Language(old, tokens[3].Token)),
						_                          => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					});

				case { Record: "Statement.SearchPropertyListDefinition" }:
					var by = Index(tokens, "AUTHORIZATION");

					// The owner is the last word; a line comment among the words kept, which run as one line, can leave none.
					if (by >= 0 && by != tokens.Count - 2)
						throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");

					var list = new Ast.Statement.CreateSearchPropertyList(name) { Authorization = by < 0 ? null : Values.Part(old, tokens[by + 1].Token.Text) };

					return words[..(by < 0 ? words.Count : by)] switch
					{
						[]          => list,
						["FROM", ..] => list with { From = Values.Name(old, Slice(text, tokens, 1, by < 0 ? tokens.Count : by)) },
						_           => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					};

				case { Record: "Statement.AlterSearchPropertyList" }:
					var with = Index(tokens, "WITH");

					return new Ast.Statement.AlterSearchPropertyList(name, words switch
					{
						["DROP", ..] => new Ast.SearchPropertyListAction.Drop(ValueOf(old, Slice(text, tokens, 1, tokens.Count))),
						["ADD", ..] when with > 1 && words[with + 1] == "(" && words[^1] == ")"
							=> (Ast.SearchPropertyListAction)new Ast.SearchPropertyListAction.Add(ValueOf(old, Slice(text, tokens, 1, with)), [.. Listed(Slice(text, tokens, with + 2, tokens.Count - 1)).Select(one => Property(old, one))]),
						_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})"),
					});

				default:
					throw BuildGap.Open(old, $"inventory {NameOf(old)} ({text})");
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
		static Ast.Expression FileGroupOf(ISqlSpan old, string text)
		{
			return SqlTokens.Of(text) is [var one] && one.Kind is SqlTokens.Kind.String or SqlTokens.Kind.Word or SqlTokens.Kind.Delimited && !one.Text.StartsWith("@", StringComparison.Ordinal)
				? Value(old, one)
				: throw BuildGap.Open(old, $"inventory {NameOf(old)} (a filegroup `{text}`)");
		}

		/// <summary>StopListLanguage: a string, a number, a binary string or a name.</summary>
		static Ast.Expression Language(ISqlSpan old, SqlTokens.Token token)
		{
			return token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
				? Values.Of(Values.Hex(token.Text))
				: Value(old, token);
		}

		/// <summary>A search property's setting: a name, <c>=</c>, a value.</summary>
		static Ast.Option Property(ISqlSpan old, string text)
		{
			var tokens = Located(text);

			return tokens.Count >= 3 && tokens[0].Token.Kind == SqlTokens.Kind.Word && tokens[1].Token.Text == "="
				? new Ast.Option(Values.Part(old, tokens[0].Token.Text), ValueOf(old, Slice(text, tokens, 2, tokens.Count)), [])
				: throw BuildGap.Open(old, $"inventory {NameOf(old)} (a setting `{text}`)");
		}

		/// <summary>A value kept as text, read again as a select list's one value.</summary>
		static Ast.Expression ValueOf(ISqlSpan old, string text)
		{
			return Queries.Specified("SELECT " + text.Trim()) is { Columns: [Clause.DerivedColumn { Value: var value, Name: null }] }
				? Values.Of(value)
				: throw BuildGap.Open(old, $"inventory {NameOf(old)} (a value `{text.Trim()}`)");
		}

		/// <summary>BackupFile, each between commas; files with no comma between them are a gap.</summary>
		static SqlList<Ast.BackupFile> BackupFiles(ISqlSpan old, string text)
		{
			if (text.Length == 0)
				return [];

			return [.. Listed(text).Select(piece => Words(piece) switch
			{
				["READ_WRITE_FILEGROUPS"] => new Ast.BackupFile.ReadWriteFileGroups(),
				["FILE", "=", ..]         => (Ast.BackupFile)new Ast.BackupFile.File(BackupName(old, Slice(piece, Located(piece), 2, int.MaxValue))),
				["FILEGROUP", "=", ..]    => new Ast.BackupFile.FileGroup(BackupName(old, Slice(piece, Located(piece), 2, int.MaxValue))),
				["PAGE", "=", _]          => new Ast.BackupFile.Page(Value(old, SqlTokens.Of(piece)[2])),
				_                         => throw BuildGap.Open(old, $"inventory {NameOf(old)} (files `{text}`)"),
			})];
		}

		/// <summary>BackupName: a name of one part or several, a variable, or a string.</summary>
		static Ast.Expression BackupName(ISqlSpan old, string text)
		{
			var tokens = SqlTokens.Of(text);

			return tokens switch
			{
				[{ Kind: SqlTokens.Kind.String } one] when one.Text[0] == '\'' => Values.String(one.Text),
				[{ Kind: SqlTokens.Kind.Word } one] when one.Text.StartsWith("@", StringComparison.Ordinal) => new Ast.Expression.Variable(new Ast.Identifier(one.Text)),
				_ when tokens.Count % 2 == 1 && tokens.Select((one, at) => at % 2 == 1 ? one.Text == "." : one.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited && !one.Text.StartsWith("@", StringComparison.Ordinal)).All(static one => one)
					=> new Ast.Expression.Reference(Values.Name(old, text)),
				_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} (a name `{text}`)"),
			};
		}

		/// <summary>BackupDevice, each between commas.</summary>
		static SqlList<Ast.BackupDevice> Devices(ISqlSpan old, string text)
		{
			return [.. Listed(text).Select(piece =>
			{
				var tokens = SqlTokens.Of(piece);

				return Words(piece) switch
				{
					["DISK" or "TAPE" or "URL", "=", _] => new Ast.BackupDevice.Physical(Words(piece)[0] switch { "DISK" => Ast.DeviceKind.Disk, "TAPE" => Ast.DeviceKind.Tape, _ => Ast.DeviceKind.Url }, Value(old, tokens[2])),
					[_] when tokens[0].Text.StartsWith("@", StringComparison.Ordinal) => (Ast.BackupDevice)new Ast.BackupDevice.Logical(null, new Ast.Expression.Variable(new Ast.Identifier(tokens[0].Text))),
					_ when BackupName(old, piece) is Ast.Expression.Reference { Name: var name } => new Ast.BackupDevice.Logical(name, null),
					_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} (a device `{piece}`)"),
				};
			})];
		}

		/// <summary>
		/// The options of a WITH or of a bracket: a name of a word or several, then nothing, <c>= value</c> or a bracketed
		/// list; MOVE's second string after TO and STOPATMARK's AFTER and its value, the option's own options.
		/// </summary>
		static SqlList<Ast.Option> Options(ISqlSpan old, string text)
		{
			return [.. Listed(text).Select(piece =>
			{
				var tokens = Located(piece);
				var named  = 0;

				while (named < tokens.Count && tokens[named].Token.Kind == SqlTokens.Kind.Word && !tokens[named].Token.Text.StartsWith("@", StringComparison.Ordinal))
					named++;

				if (named == 0)
					throw BuildGap.Open(old, $"inventory {NameOf(old)} (an option `{piece}`)");

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
					[_, var to, _] when option.Name.Text.Equals("MOVE", StringComparison.OrdinalIgnoreCase) && option.Words.Count == 0 && to.Equals("TO", StringComparison.OrdinalIgnoreCase)
						=> option with { Value = Value(old, tokens[named].Token), Options = [new Ast.Option(Values.Part(old, tokens[named + 1].Token.Text), Value(old, tokens[named + 2].Token), [])] },
					["=", _, var after, _] when after.Equals("AFTER", StringComparison.OrdinalIgnoreCase)
						=> option with { Value = Value(old, tokens[named + 1].Token), Options = [new Ast.Option(Values.Part(old, tokens[named + 2].Token.Text), Value(old, tokens[named + 3].Token), [])] },
					_ => throw BuildGap.Open(old, $"inventory {NameOf(old)} (an option `{piece}`)"),
				};
			})];
		}

		/// <summary>One token as a value: a string, a number, a variable, NULL, or a word or a name in brackets.</summary>
		static Ast.Expression Value(ISqlSpan old, SqlTokens.Token token)
		{
			return token switch
			{
				{ Kind: SqlTokens.Kind.String }                                              => Values.String(token.Text),
				{ Kind: SqlTokens.Kind.Number }                                              => Values.Of(Values.Number(token.Text)),
				{ Kind: SqlTokens.Kind.Word } when token.Text.StartsWith("@", StringComparison.Ordinal) => new Ast.Expression.Variable(new Ast.Identifier(token.Text)),
				{ Kind: SqlTokens.Kind.Word } when token.Text.Equals("NULL", StringComparison.OrdinalIgnoreCase) => new Ast.LiteralValue.Null(),
				{ Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited }                     => new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(old, token.Text)])),
				_                                                                             => throw BuildGap.Open(old, $"inventory {NameOf(old)} (a value `{token.Text}`)"),
			};
		}

		/// <summary>KeyPlace at a token: FILE or URL, <c>=</c>, a string.</summary>
		static Ast.KeyPlace Place(ISqlSpan old, List<(SqlTokens.Token Token, int At)> tokens, int at)
		{
			return new Ast.KeyPlace(tokens[at].Token.Text.ToUpperInvariant() switch
			{
				"FILE" => Ast.KeyPlaceKind.File,
				"URL"  => Ast.KeyPlaceKind.Url,
				_      => throw BuildGap.Open(old, $"inventory {NameOf(old)} ({tokens[at].Token.Text})"),
			}, Password(old, tokens, at + 2));
		}

		/// <summary>A string at a token, as a path or a password is written.</summary>
		static Ast.Expression Password(ISqlSpan old, List<(SqlTokens.Token Token, int At)> tokens, int at)
		{
			return tokens[at].Token.Kind == SqlTokens.Kind.String ? Values.String(tokens[at].Token.Text) : throw BuildGap.Open(old, $"inventory {NameOf(old)} (a password or path `{tokens[at].Token.Text}`)");
		}

		/// <summary>ALTER DATABASE's target: CURRENT, the word, or a name.</summary>
		static Ast.Statement.AlterDatabase Target(ISqlSpan old, string name, Ast.AlterDatabaseAction action)
		{
			return name == "CURRENT"
				? new Ast.Statement.AlterDatabase(null, action) { Current = true }
				: new Ast.Statement.AlterDatabase(Values.Part(old, name), action);
		}

		/// <summary>File specifications kept as text, read again as CREATE DATABASE's data files.</summary>
		static SqlList<Ast.DatabaseFile> Files(ISqlSpan old, string text)
		{
			return Skim("CREATE DATABASE d ON " + text) is [Statement.Built { Given: Statement.CreateDatabase { Files: var files, Log: null } }]
				&& files.All(static one => one is Clause.DatabaseFile)
				? Each(files, one => File(old, (Clause.DatabaseFile)one))
				: throw BuildGap.Open(old, $"inventory {NameOf(old)} (files `{text}`)");
		}

		static Ast.DatabaseFile File(ISqlSpan old, Clause.DatabaseFile file)
		{
			return new Ast.DatabaseFile(Each(file.Options, one => one is Clause.Option option ? Tables.Option(option) : throw BuildGap.Open(old, $"inventory {NameOf(one)} (in a file)")));
		}

		/// <summary>
		/// A statement made of a tail the old tree keeps as written: only one whose text, written and read again, keeps the
		/// same tail, but for the case of its key words and its blanks, carries over.
		/// </summary>
		static Ast.Statement Checked(Defining old, Ast.Statement made)
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

			return Skim(text) is [Statement.Built { Given: Defining { Tail: var tail } }] && Kept(old, NameOf(old) + ".Tail", tail, old.Tail)
				? made
				: throw BuildGap.Open(old, $"Trees.FreeText ({NameOf(old)}.Tail, written otherwise than the writer prints it)");
		}

		/// <summary>
		/// Whether a text the old tree keeps, read again from what the writer printed, is the one written: the same, or,
		/// for a member of the closed list, the same token by token but for the case of key words and the blanks.
		/// </summary>
		static bool Kept(object old, string member, string? again, string? written)
		{
			return again == written || again is not null && written is not null && Same(old, member, again, written);
		}

		/// <summary>The tokens of a text and where each begins.</summary>
		static List<(SqlTokens.Token Token, int At)> Located(string text)
		{
			return SqlTokens.Located(text);
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
