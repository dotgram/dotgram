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
