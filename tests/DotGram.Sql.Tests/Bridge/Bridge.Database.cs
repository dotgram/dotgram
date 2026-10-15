using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

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
				Statement.Definition definition when definition.GetType().Name.StartsWith("AlterDatabase", StringComparison.Ordinal)
					=> Alter(definition),
				_ when Owned(Owns, old) => throw Inventory.Unconverted(old),
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
					=> new Ast.Termination.RollbackAfter(new Ast.LiteralValue.Numeric(tokens[3].Text, Ast.NumericLiteralKind.DecimalInteger)) { Seconds = rest.Length == 1 },
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
				text = Ast.Sql2023Writer.Write(made);
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
				text = Ast.Sql2023Writer.Write(made);
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
