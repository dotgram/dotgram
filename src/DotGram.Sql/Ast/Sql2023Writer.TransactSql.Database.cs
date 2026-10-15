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

				// T-SQL: CREATE DATABASE (Transact-SQL).
				case Statement.CreateDatabase create:
					PutCreateDatabase(create);
					return true;

				// T-SQL: BACKUP (Transact-SQL). BackupStatement.
				case Statement.BackupDatabase backup:
					Word("BACKUP DATABASE");
					PutBackupName(backup, backup.Database);
					PutBackupFiles(backup, backup.Files, restore: false);
					PutBackupTo(backup, backup.To, backup.Mirrors, backup.Options);
					return true;

				case Statement.BackupLog backup:
					Word("BACKUP LOG");
					PutBackupName(backup, backup.Database);
					PutBackupTo(backup, backup.To, backup.Mirrors, backup.Options);
					return true;

				case Statement.BackupServer backup:
					Word("BACKUP SERVER");
					PutBackupTo(backup, backup.To, backup.Mirrors, backup.Options);
					return true;

				// BackupGroup: the databases, one at least.
				case Statement.BackupGroup backup:
					if (backup.Databases.Count == 0)
						throw Empty(backup, "Databases");

					Word("BACKUP GROUP");
					Each(backup.Databases, database => PutBackupName(backup, database));
					PutBackupTo(backup, backup.To, backup.Mirrors, backup.Options);
					return true;

				// The path and the passwords are strings; the format and the private key's settings come after WITH, the
				// format only before a private key (CertificatePrivateKey).
				case Statement.BackupCertificate backup:
					Word("BACKUP CERTIFICATE");
					PutIdentifier(backup.Name);
					Word("TO FILE =");
					PutKeyString(backup, "File", backup.File);

					if (backup.PrivateKey.Count == 0)
					{
						if (backup.Format is not null)
							throw Misplaced(backup, "Format", "without a private key (CertificatePrivateKey)");

						return true;
					}

					if (backup.PrivateKey.Count(static option => Plain(option.Name, "ALGORITHM")) > 1)
						throw Misplaced(backup, "PrivateKey", "naming ALGORITHM twice (Syntax.Once, Msg 102)");

					Word("WITH");

					if (backup.Format is { } format)
					{
						Word("FORMAT =");
						PutKeyString(backup, "Format", format);
						Tight(",");
					}

					Word("PRIVATE KEY");
					Open();
					Each(backup.PrivateKey, option => PutSetting(option, PrivateKeyOptions, "CertificateKeyItem"));
					Close();
					return true;

				case Statement.BackupMasterKey backup:
					Word("BACKUP MASTER KEY TO");
					PutKeyPlace(backup.To);
					PutPassword(backup, "ENCRYPTION", backup.EncryptionPassword);
					return true;

				case Statement.BackupServiceMasterKey backup:
					Word("BACKUP SERVICE MASTER KEY TO FILE =");
					PutKeyString(backup, "File", backup.File);
					PutPassword(backup, "ENCRYPTION", backup.EncryptionPassword);
					return true;

				case Statement.BackupSymmetricKey backup:
					Word("BACKUP SYMMETRIC KEY");
					PutIdentifier(backup.Name);
					Word("TO");
					PutKeyPlace(backup.To);
					PutPassword(backup, "ENCRYPTION", backup.EncryptionPassword);
					return true;

				// T-SQL: RESTORE (Transact-SQL). RestoreStatement: the devices or a snapshot, never both.
				case Statement.RestoreDatabase restore:
					Word("RESTORE DATABASE");
					PutBackupName(restore, restore.Database);
					PutBackupFiles(restore, restore.Files, restore: true);

					if (restore.DatabaseSnapshot is { } snapshot)
					{
						if (restore.From.Count > 0)
							throw Both(restore, "From", "DatabaseSnapshot");

						if (!StringValue(snapshot) && snapshot is not Expression.Variable)
							throw Misplaced(restore, "DatabaseSnapshot", "other than a string or a variable (RestoreFrom: BackupString)");

						Word("FROM DATABASE_SNAPSHOT =");
						PutExpression(snapshot);
					}
					else
						PutRestoreFrom(restore, restore.From, required: false);

					PutBackupWith(restore.Options, RestoreOptions, "RestoreOption");
					return true;

				case Statement.RestoreLog restore:
					Word("RESTORE LOG");
					PutBackupName(restore, restore.Database);
					PutBackupFiles(restore, restore.Files, restore: true);
					PutRestoreFrom(restore, restore.From, required: false);
					PutBackupWith(restore.Options, RestoreOptions, "RestoreOption");
					return true;

				case Statement.RestoreFileListOnly restore:
					Word("RESTORE FILELISTONLY");
					PutRestoreFrom(restore, restore.From, required: true);
					PutBackupWith(restore.Options, RestoreOptions, "RestoreOption");
					return true;

				case Statement.RestoreHeaderOnly restore:
					Word("RESTORE HEADERONLY");
					PutRestoreFrom(restore, restore.From, required: true);
					PutBackupWith(restore.Options, RestoreOptions, "RestoreOption");
					return true;

				case Statement.RestoreLabelOnly restore:
					Word("RESTORE LABELONLY");
					PutRestoreFrom(restore, restore.From, required: true);
					PutBackupWith(restore.Options, RestoreOptions, "RestoreOption");
					return true;

				case Statement.RestoreRewindOnly restore:
					Word("RESTORE REWINDONLY");
					PutRestoreFrom(restore, restore.From, required: true);
					PutBackupWith(restore.Options, RestoreOptions, "RestoreOption");
					return true;

				case Statement.RestoreVerifyOnly restore:
					Word("RESTORE VERIFYONLY");
					PutRestoreFrom(restore, restore.From, required: true);
					PutBackupWith(restore.Options, RestoreOptions, "RestoreOption");
					return true;

				// The password it was locked with, the one to lock it with, and FORCE last.
				case Statement.RestoreMasterKey restore:
					Word("RESTORE MASTER KEY FROM");
					PutKeyPlace(restore.From);
					PutPassword(restore, "DECRYPTION", restore.DecryptionPassword);
					PutPassword(restore, "ENCRYPTION", restore.EncryptionPassword);

					if (restore.Force)
						Word("FORCE");

					return true;

				case Statement.RestoreServiceMasterKey restore:
					Word("RESTORE SERVICE MASTER KEY FROM FILE =");
					PutKeyString(restore, "File", restore.File);
					PutPassword(restore, "DECRYPTION", restore.DecryptionPassword);

					if (restore.Force)
						Word("FORCE");

					return true;

				case Statement.RestoreSymmetricKey restore:
					Word("RESTORE SYMMETRIC KEY");
					PutIdentifier(restore.Name);
					Word("FROM");
					PutKeyPlace(restore.From);
					PutPassword(restore, "DECRYPTION", restore.DecryptionPassword);
					PutPassword(restore, "ENCRYPTION", restore.EncryptionPassword);

					if (restore.Force)
						Word("FORCE");

					return true;

				// T-SQL: CREATE PARTITION FUNCTION (Transact-SQL). PartitionStatement: one type, a collation, the side, and the
				// boundaries, which may be none.
				case Statement.CreatePartitionFunction function:
					Word("CREATE PARTITION FUNCTION");
					PutIdentifier(function.Name);
					Open();
					PutType(function.Type);

					if (function.Collation is { } collation)
					{
						Word("COLLATE");
						PutCollation(collation);
					}

					Close();
					Word("AS RANGE");

					if (function.Range is { } side)
					{
						Word(side switch
						{
							RangeSide.Left  => "LEFT",
							RangeSide.Right => "RIGHT",
							_               => throw Misplaced(function, "Range", "of no word (LEFT or RIGHT)"),
						});
					}

					Word("FOR VALUES");
					Open();
					Each(function.Boundaries, one => PutValue(function, one));
					Close();
					return true;

				// One boundary split or merged at a time, the function named with its empty brackets.
				case Statement.AlterPartitionFunction function:
					Word("ALTER PARTITION FUNCTION");
					PutIdentifier(function.Name);
					Open();
					Close();

					var (verb, boundary) = function.Action switch
					{
						PartitionFunctionAction.Split split => ("SPLIT", split.Boundary),
						PartitionFunctionAction.Merge merge => ("MERGE", merge.Boundary),
						_                                   => throw Unwritten(function.Action),
					};

					Word(verb);

					if (boundary is not null)
					{
						Word("RANGE");
						Open();
						PutValue(function, boundary);
						Close();
					}

					return true;

				// The filegroups, one at least, each a name or a string (PartitionFileGroup).
				case Statement.CreatePartitionScheme scheme:
					if (scheme.FileGroups.Count == 0)
						throw Empty(scheme, "FileGroups");

					Word("CREATE PARTITION SCHEME");
					PutIdentifier(scheme.Name);
					Word("AS PARTITION");
					PutIdentifier(scheme.Function);

					if (scheme.All)
						Word("ALL");

					Word("TO");
					Open();
					Each(scheme.FileGroups, group => PutPartitionFileGroup(scheme, group));
					Close();
					return true;

				case Statement.AlterPartitionScheme scheme:
					Word("ALTER PARTITION SCHEME");
					PutIdentifier(scheme.Name);
					Word("NEXT USED");

					if (scheme.NextUsed is { } next)
						PutPartitionFileGroup(scheme, next);

					return true;

				// T-SQL: CREATE FULLTEXT CATALOG (Transact-SQL). Each part once, in the published order.
				case Statement.CreateFullTextCatalog catalog:
					Word("CREATE FULLTEXT CATALOG");
					PutIdentifier(catalog.Name);

					if (catalog.FileGroup is { } fileGroup)
					{
						Word("ON FILEGROUP");
						PutIdentifier(fileGroup);
					}

					if (catalog.Path is { } path)
					{
						Word("IN PATH");
						PutValue(catalog, path);
					}

					PutAccentSensitivity(catalog.AccentSensitivity);

					if (catalog.Default)
						Word("AS DEFAULT");

					if (catalog.Authorization is { } owner)
					{
						Word("AUTHORIZATION");
						PutIdentifier(owner);
					}

					return true;

				case Statement.AlterFullTextCatalog catalog:
					Word("ALTER FULLTEXT CATALOG");
					PutIdentifier(catalog.Name);

					switch (catalog.Action)
					{
						case FullTextCatalogAction.Rebuild rebuild:
							Word("REBUILD");
							PutAccentSensitivity(rebuild.AccentSensitivity);
							break;

						case FullTextCatalogAction.Reorganize:
							Word("REORGANIZE");
							break;

						case FullTextCatalogAction.AsDefault:
							Word("AS DEFAULT");
							break;

						default:
							throw Unwritten(catalog.Action);
					}

					return true;

				// T-SQL: CREATE FULLTEXT STOPLIST (Transact-SQL). StopListFrom: a name of two parts at most (Msg 102).
				case Statement.CreateFullTextStopList stopList:
					Word("CREATE FULLTEXT STOPLIST");
					PutIdentifier(stopList.Name);

					switch (stopList.From)
					{
						case null:
							break;

						case StopListSource.SystemStopList:
							Word("FROM SYSTEM STOPLIST");
							break;

						case StopListSource.Named { Name: var source }:
							if (source.Count is < 1 or > 2)
								throw Misplaced(stopList.From, "Name", "of other than one or two parts (StopListFrom, Msg 102)");

							Word("FROM");
							PutName(source);
							break;

						default:
							throw Unwritten(stopList.From);
					}

					if (stopList.Authorization is { } stopListOwner)
					{
						Word("AUTHORIZATION");
						PutIdentifier(stopListOwner);
					}

					return true;

				// StopListAction: a word quoted, and its language a string, a number, a binary string or a name.
				case Statement.AlterFullTextStopList stopList:
					Word("ALTER FULLTEXT STOPLIST");
					PutIdentifier(stopList.Name);

					switch (stopList.Action)
					{
						case StopListAction.Add add:
							Word("ADD");
							PutStopWord(add, add.Word, add.Language);
							break;

						case StopListAction.Drop drop:
							Word("DROP");
							PutStopWord(drop, drop.Word, drop.Language);
							break;

						case StopListAction.DropAll all:
							Word("DROP ALL");

							if (all.Language is { } language)
							{
								Word("LANGUAGE");
								PutStopListLanguage(all, language);
							}

							break;

						default:
							throw Unwritten(stopList.Action);
					}

					return true;

				// T-SQL: CREATE SEARCH PROPERTY LIST (Transact-SQL).
				case Statement.CreateSearchPropertyList list:
					Word("CREATE SEARCH PROPERTY LIST");
					PutIdentifier(list.Name);

					if (list.From is { } from)
					{
						Word("FROM");
						PutName(from);
					}

					if (list.Authorization is { } listOwner)
					{
						Word("AUTHORIZATION");
						PutIdentifier(listOwner);
					}

					return true;

				// PropertyListAction: the set's GUID and the integer id, in that order and in brackets, and the description
				// after them.
				case Statement.AlterSearchPropertyList list:
					Word("ALTER SEARCH PROPERTY LIST");
					PutIdentifier(list.Name);

					switch (list.Action)
					{
						case SearchPropertyListAction.Add add:
							if (add.Options.Select(static option => Key(option).ToUpperInvariant()).ToArray() is not (["PROPERTY_SET_GUID", "PROPERTY_INT_ID"] or ["PROPERTY_SET_GUID", "PROPERTY_INT_ID", "PROPERTY_DESCRIPTION"]))
								throw Misplaced(add, "Options", "other than PROPERTY_SET_GUID, PROPERTY_INT_ID and PROPERTY_DESCRIPTION in that order (PropertyListAction)");

							Word("ADD");
							PutValue(add, add.Property);
							Word("WITH");
							Open();
							Each(add.Options, option => PutSetting(option, PropertyOptions, "PropertyListAction"));
							Close();
							break;

						case SearchPropertyListAction.Drop drop:
							Word("DROP");
							PutValue(drop, drop.Property);
							break;

						default:
							throw Unwritten(list.Action);
					}

					return true;

				// T-SQL: CHECKPOINT (Transact-SQL). A value or none.
				case Statement.Checkpoint checkpoint:
					Word("CHECKPOINT");

					if (checkpoint.Duration is { } duration)
						PutValue(checkpoint, duration);

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
					Word("MODIFY");
					PutEditionOptions(modify, modify.Options);

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

		/// <summary>T-SQL: CREATE PARTITION SCHEME (Transact-SQL). PartitionFileGroup: a name of one part or a string.</summary>
		void PutPartitionFileGroup(ISqlNode owner, Expression group)
		{
			if (group is Expression.Reference { Name: [var name] })
				PutIdentifier(name);
			else if (StringValue(group))
				PutExpression(group);
			else
				throw Misplaced(owner, "FileGroups or NextUsed", "other than a name of one part or a string (PartitionFileGroup)");
		}

		/// <summary>T-SQL: CREATE FULLTEXT CATALOG (Transact-SQL). CatalogWith: <c>WITH ACCENT_SENSITIVITY = ON | OFF</c>.</summary>
		void PutAccentSensitivity(bool? sensitive)
		{
			if (sensitive is { } on)
				Word(on ? "WITH ACCENT_SENSITIVITY = ON" : "WITH ACCENT_SENSITIVITY = OFF");
		}

		/// <summary>T-SQL: ALTER FULLTEXT STOPLIST (Transact-SQL). <c>'word' LANGUAGE language</c>: the word quoted (Quoted).</summary>
		void PutStopWord(ISqlNode owner, Expression word, Expression language)
		{
			if (!StringValue(word))
				throw Misplaced(owner, "Word", "other than a string (StopListAction: Quoted)");

			PutExpression(word);
			Word("LANGUAGE");
			PutStopListLanguage(owner, language);
		}

		/// <summary>StopListLanguage: a string, an unsigned number, a binary string or a name; a variable is Msg 102.</summary>
		void PutStopListLanguage(ISqlNode owner, Expression language)
		{
			switch (language)
			{
				case Expression.Reference { Name: [var name] }:
					PutIdentifier(name);
					break;

				case LiteralValue.Binary { ZeroX: true }:
					PutExpression(language);
					break;

				case var _ when StringValue(language) || UnsignedNumber(language):
					PutExpression(language);
					break;

				default:
					throw Misplaced(owner, "Language", "other than a string, a number, a binary string or a name (StopListLanguage)");
			}
		}

		/// <summary>
		/// T-SQL: BACKUP (Transact-SQL). BackupName: a name, a variable or a string; the brackets of a list the grammar
		/// reads and keeps the first of are not written.
		/// </summary>
		void PutBackupName(ISqlNode owner, Expression name)
		{
			switch (name)
			{
				case Expression.Reference { Name: var named } when named.Count > 0:
					if (named.Count == 1 && named[0].Style == IdentifierStyle.Regular && BackupBoundary.Contains(named[0].Text))
						throw Misplaced(owner, "Database", named[0].Text + " unbracketed, which reads as the word after it (BackupBoundary)");

					PutName(named);
					break;

				case Expression.Variable:
				case LiteralValue.String { Kind: StringLiteralKind.Character, CharacterSet: null, UnicodeEscape: null }:
					PutExpression(name);
					break;

				default:
					throw Misplaced(owner, "Database", "other than a name, a variable or a string (BackupName)");
			}
		}

		static readonly HashSet<string> BackupBoundary = new(StringComparer.OrdinalIgnoreCase) { "FILE", "FILEGROUP", "READ_WRITE_FILEGROUPS", "PAGE", "TO", "FROM", "WITH" };

		/// <summary>T-SQL: BACKUP (Transact-SQL), RESTORE (Transact-SQL). BackupFile, a comma between each; PAGE a restore's.</summary>
		void PutBackupFiles(ISqlNode owner, SqlList<BackupFile> files, bool restore)
		{
			for (var at = 0; at < files.Count; at++)
			{
				if (at > 0)
					Tight(",");

				switch (files[at])
				{
					case BackupFile.File file:
						Word("FILE =");
						PutBackupName(file, file.Name);
						break;

					case BackupFile.FileGroup group:
						Word("FILEGROUP =");
						PutBackupName(group, group.Name);
						break;

					case BackupFile.ReadWriteFileGroups:
						Word("READ_WRITE_FILEGROUPS");
						break;

					case BackupFile.Page page:
						Word("PAGE =");
						PutValue(page, page.Value);
						break;

					default:
						throw Unwritten(files[at]);
				}
			}
		}

		/// <summary>T-SQL: BACKUP (Transact-SQL). <c>TO devices [MIRROR TO devices]… [WITH options]</c>.</summary>
		void PutBackupTo(ISqlNode owner, SqlList<BackupDevice> to, SqlList<BackupMirror> mirrors, SqlList<Option> options)
		{
			if (to.Count == 0)
				throw Empty(owner, "To");

			Word("TO");
			Each(to, PutBackupDevice);

			foreach (var mirror in mirrors)
			{
				if (mirror.Devices.Count == 0)
					throw Empty(mirror, "Devices");

				Word("MIRROR TO");
				Each(mirror.Devices, PutBackupDevice);
			}

			PutBackupWith(options, BackupOptions, "BackupOption");
		}

		void PutRestoreFrom(ISqlNode owner, SqlList<BackupDevice> from, bool required)
		{
			if (from.Count == 0)
			{
				if (required)
					throw Empty(owner, "From");

				return;
			}

			Word("FROM");
			Each(from, PutBackupDevice);
		}

		/// <summary>T-SQL: BACKUP (Transact-SQL). BackupDevice.</summary>
		void PutBackupDevice(BackupDevice device)
		{
			switch (device)
			{
				case BackupDevice.Logical { Name: not null, Variable: not null }:
					throw Both(device, "Name", "Variable");

				case BackupDevice.Logical { Name: { } name }:
					if (name.Count == 1 && name[0].Style == IdentifierStyle.Regular && (BackupBoundary.Contains(name[0].Text) || Plain(name[0], "MIRROR", "DATABASE_SNAPSHOT")))
						throw Misplaced(device, "Name", name[0].Text + " unbracketed, which reads as a word of the statement");

					PutName(name);
					break;

				case BackupDevice.Logical { Variable: { } variable }:
					PutExpression(variable);
					break;

				case BackupDevice.Logical:
					throw Empty(device, "Name", "Variable");

				case BackupDevice.Physical physical:
					Word(physical.Kind switch
					{
						DeviceKind.Disk => "DISK",
						DeviceKind.Tape => "TAPE",
						DeviceKind.Url  => "URL",
						_               => throw Misplaced(physical, "Kind", "of no word (DeviceWord)"),
					});
					Word("=");
					PutValue(physical, physical.Value);
					break;

				default:
					throw Unwritten(device);
			}
		}

		void PutBackupWith(SqlList<Option> options, Dictionary<string, Setting[]> catalogue, string rule)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Each(options, option => PutSetting(option, catalogue, rule));
		}

		/// <summary>KeyPlace: <c>FILE</c> or <c>URL</c>, and a string.</summary>
		void PutKeyPlace(KeyPlace place)
		{
			Word(place.Kind switch
			{
				KeyPlaceKind.File => "FILE =",
				KeyPlaceKind.Url  => "URL =",
				_                 => throw Misplaced(place, "Kind", "of no word (KeyPlace)"),
			});
			PutKeyString(place, "Path", place.Path);
		}

		/// <summary>KeyLockedBy and KeyUnlockedBy: <c>ENCRYPTION BY PASSWORD = 'p'</c>, <c>DECRYPTION BY PASSWORD = 'p'</c>.</summary>
		void PutPassword(ISqlNode owner, string which, Expression password)
		{
			Word(which + " BY PASSWORD =");
			PutKeyString(owner, which[0] + which.Substring(1).ToLowerInvariant() + "Password", password);
		}

		/// <summary>A path or a password of a key's backup: a string, never a variable (Quoted).</summary>
		void PutKeyString(ISqlNode owner, string member, Expression value)
		{
			if (!StringValue(value))
				throw Misplaced(owner, member, "other than a string (Quoted)");

			PutExpression(value);
		}

		/// <summary>
		/// T-SQL: ALTER DATABASE (Transact-SQL), CREATE DATABASE (Transact-SQL), the Azure syntax. EditionOptions in brackets,
		/// one at least.
		/// </summary>
		void PutEditionOptions(ISqlNode owner, SqlList<Option> options)
		{
			if (options.Count == 0)
				throw Empty(owner, "Options");

			Open();
			Each(options, option =>
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
		}

		/// <summary>
		/// T-SQL: CREATE DATABASE (Transact-SQL). CreateDatabaseStatement: Azure's form, a name, a collation and the edition
		/// options, when they are written with no source; else the files, the collation and the tail (DatabaseTail).
		/// </summary>
		void PutCreateDatabase(Statement.CreateDatabase create)
		{
			Word("CREATE DATABASE");
			PutIdentifier(create.Name);

			if (create.EditionOptions.Count > 0 && create.Source is null)
			{
				if (create.Containment is not null || create.Primary || create.Files.Count > 0 || create.FileGroups.Count > 0 || create.LogFiles.Count > 0)
					throw Both(create, "EditionOptions", "Containment, Files, FileGroups or LogFiles");

				if (create.Collation is { } azure)
				{
					Word("COLLATE");
					PutCollation(azure);
				}

				PutEditionOptions(create, create.EditionOptions);
				PutDatabaseWith(create, create.Options, CreateOptions, "CreateDatabaseOption");
				return;
			}

			if (create.Containment is { } containment)
			{
				Word("CONTAINMENT =");
				Word(containment switch
				{
					DatabaseContainment.None    => "NONE",
					DatabaseContainment.Partial => "PARTIAL",
					_                           => throw Misplaced(create, "Containment", "of no word (NONE or PARTIAL)"),
				});
			}

			// DatabaseFiles: the primary's files first, one at least, and only then the filegroups.
			if (create.Files.Count == 0 && (create.Primary || create.FileGroups.Count > 0))
				throw Empty(create, "Files");

			if (create.Files.Count > 0)
			{
				Word("ON");

				if (create.Primary)
					Word("PRIMARY");

				Each(create.Files, PutDatabaseFile);

				foreach (var group in create.FileGroups)
				{
					Tight(",");
					PutFileGroup(group);
				}
			}

			if (create.LogFiles.Count > 0)
			{
				Word("LOG ON");
				Each(create.LogFiles, PutDatabaseFile);
			}

			if (create.Collation is { } collation)
			{
				Word("COLLATE");
				PutCollation(collation);
			}

			switch (create.Source)
			{
				case null:
					PutDatabaseWith(create, create.Options, CreateOptions, "CreateDatabaseOption");
					break;

				// AttachOption and RebuildLogOption: each option once, and one broker option at most (Syntax.Attached).
				case DatabaseSource.ForAttach attach:
					Word(attach.Kind switch
					{
						AttachKind.Attach                => "FOR ATTACH",
						AttachKind.AttachRebuildLog      => "FOR ATTACH_REBUILD_LOG",
						AttachKind.AttachForceRebuildLog => "FOR ATTACH_FORCE_REBUILD_LOG",
						_                                => throw Misplaced(attach, "Kind", "of no word (DatabaseTail)"),
					});

					var keys = create.Options.Select(static option => Key(option)).ToList();

					if (keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() < keys.Count)
						throw Misplaced(create, "Options", "naming an option twice (Syntax.NamedOnce, Msg 102)");

					if (attach.Kind != AttachKind.AttachRebuildLog && keys.Count(static key => Brokers.Contains(key)) > 1)
						throw Misplaced(create, "Options", "naming two broker options (Syntax.Attached, Msg 102)");

					PutDatabaseWith(create, create.Options, attach.Kind == AttachKind.AttachRebuildLog ? RebuildLogOptions : AttachOptions, attach.Kind == AttachKind.AttachRebuildLog ? "RebuildLogOption" : "AttachOption");
					break;

				case DatabaseSource.AsSnapshotOf snapshot:
					Word("AS SNAPSHOT OF");
					PutIdentifier(snapshot.Database);
					PutDatabaseWith(create, create.Options, CreateOptions, "CreateDatabaseOption");
					break;

				// [server.]database, its edition options, and WITH (BACKUP_STORAGE_REDUNDANCY = …) in brackets, alone.
				case DatabaseSource.AsCopyOf copy:
					if (copy.Database.Count is < 1 or > 2)
						throw Misplaced(copy, "Database", "of other than one or two parts (DatabaseTail)");

					Word("AS COPY OF");
					PutName(copy.Database);

					if (create.EditionOptions.Count > 0)
						PutEditionOptions(create, create.EditionOptions);

					if (create.Options.Count > 1)
						throw Misplaced(create, "Options", "of more than one (DatabaseTail: WITH (BackupRedundancy))");

					if (create.Options is [var redundancy])
					{
						Word("WITH");
						Open();
						PutSetting(redundancy, CopyOptions, "BackupRedundancy");
						Close();
					}

					break;

				default:
					throw Unwritten(create.Source);
			}

			if (create.Source is not DatabaseSource.AsCopyOf && create.EditionOptions.Count > 0)
				throw Both(create, "EditionOptions", "Source");
		}

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). FileGroupSpec: a name, what it holds, DEFAULT, and its files, one at least.</summary>
		void PutFileGroup(FileGroup group)
		{
			if (group.Files.Count == 0)
				throw Empty(group, "Files");

			Word("FILEGROUP");
			PutIdentifier(group.Name);

			if (group.Contains is { } contains)
			{
				Word("CONTAINS");
				Word(contains switch
				{
					FileGroupContents.Filestream          => "FILESTREAM",
					FileGroupContents.MemoryOptimizedData => "MEMORY_OPTIMIZED_DATA",
					_                                     => throw Misplaced(group, "Contains", "of no word (FileGroupHolds)"),
				});
			}

			if (group.Default)
				Word("DEFAULT");

			Each(group.Files, PutDatabaseFile);
		}

		/// <summary><c>WITH option, …</c> from a catalogue, where any is written.</summary>
		void PutDatabaseWith(ISqlNode owner, SqlList<Option> options, Dictionary<string, Setting[]> catalogue, string rule)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Each(options, option => PutSetting(option, catalogue, rule));
		}

		/// <summary>A setting's name and further words, as its catalogue keys it.</summary>
		static string Key(Option option)
		{
			return string.Join(" ", new[] { option.Name.Text }.Concat(option.Words.Select(static word => word.Text)));
		}

		static readonly string[] Brokers = ["ENABLE_BROKER", "NEW_BROKER", "ERROR_BROKER_CONVERSATIONS"];

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
		sealed record Nest(Dictionary<string, Setting[]> Catalogue, bool Required = false, bool Single = false, Func<string[], bool>? Sequence = null);

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

			var key   = Key(option);
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
			return option.Options.Count == 0
				? nest is not { Required: true }
				: nest is not null && (!nest.Single || option.Options.Count == 1)
					&& (nest.Sequence is null || nest.Sequence([.. option.Options.Select(static one => Key(one).ToUpperInvariant())]));
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

		/// <summary>
		/// T-SQL: BACKUP (Transact-SQL). BackupOption: the words alone, the settings given a string, a variable or a
		/// number, those given only a string or a variable, STATS with a value or none, and COMPRESSION's and
		/// ENCRYPTION's lists.
		/// </summary>
		static readonly Dictionary<string, Setting[]> BackupOptions = BackupCatalogue();

		static Dictionary<string, Setting[]> BackupCatalogue()
		{
			var catalogue = new Dictionary<string, Setting[]>(StringComparer.OrdinalIgnoreCase)
			{
				["COMPRESSION"] = [new(false, None), new(false, None, new(Settings(
					("ALGORITHM", true, Words("MS_XPRESS", "ZSTD", "QAT_DEFLATE")),
					("LEVEL",     true, Words("LOW", "MEDIUM", "HIGH"))), Required: true, Sequence: static keys => keys is ["ALGORITHM"] or ["ALGORITHM", "LEVEL"]))],
				["ENCRYPTION"]  = [new(false, None, new(new Dictionary<string, Setting[]>(StringComparer.OrdinalIgnoreCase)
				{
					["ALGORITHM"]             = [new(true, Words("AES_128", "AES_192", "AES_256", "TRIPLE_DES_3KEY"))],
					["SERVER CERTIFICATE"]    = [new(true, NameValue)],
					["SERVER ASYMMETRIC KEY"] = [new(true, NameValue)],
				}, Required: true, Sequence: static keys => keys is ["ALGORITHM", "SERVER CERTIFICATE" or "SERVER ASYMMETRIC KEY"]))],
				["STATS"]       = [new(false, None), new(true, BackupValue)],
			};

			foreach (var word in new[] { "DIFFERENTIAL", "COPY_ONLY", "NO_COMPRESSION", "FILE_SNAPSHOT", "METADATA_ONLY", "SNAPSHOT", "INIT", "NOINIT", "SKIP", "NOSKIP", "FORMAT", "NOFORMAT", "CHECKSUM", "NO_CHECKSUM", "STOP_ON_ERROR", "CONTINUE_AFTER_ERROR", "RESTART", "REWIND", "NOREWIND", "UNLOAD", "NOUNLOAD", "NORECOVERY", "NO_TRUNCATE", "NO_LOG" })
				catalogue.Add(word, [new(false, None)]);

			foreach (var name in new[] { "DESCRIPTION", "MEDIADESCRIPTION", "EXPIREDATE", "RETAINDAYS", "BLOCKSIZE", "BUFFERCOUNT", "MAXTRANSFERSIZE", "BACKUP_OPTIONS" })
				catalogue.Add(name, [new(true, BackupValue)]);

			foreach (var name in new[] { "NAME", "MEDIANAME", "CREDENTIAL", "PASSWORD", "MEDIAPASSWORD", "STANDBY" })
				catalogue.Add(name, [new(true, BackupString)]);

			return catalogue;
		}

		/// <summary>
		/// T-SQL: RESTORE (Transact-SQL). RestoreOption: one list for every form of RESTORE, as the engine reads it. MOVE,
		/// with its two strings, has no place in an option and is not here.
		/// </summary>
		static readonly Dictionary<string, Setting[]> RestoreOptions = RestoreCatalogue();

		static Dictionary<string, Setting[]> RestoreCatalogue()
		{
			var catalogue = new Dictionary<string, Setting[]>(StringComparer.OrdinalIgnoreCase)
			{
				["FILE"]       = [new(true, static value => value is Expression.Variable || UnsignedNumber(value))],
				["STATS"]      = [new(false, None), new(true, BackupValue)],
				["FILESTREAM"] = [new(false, None, new(Settings(("DIRECTORY_NAME", true, static value => StringValue(value) || value is LiteralValue.Null)), Required: true, Single: true))],
			};

			foreach (var word in new[] { "RECOVERY", "NORECOVERY", "REPLACE", "RESTART", "RESTRICTED_USER", "PARTIAL", "LOADHISTORY", "METADATA_ONLY", "SNAPSHOT", "CHECKSUM", "NO_CHECKSUM", "STOP_ON_ERROR", "CONTINUE_AFTER_ERROR", "REWIND", "NOREWIND", "UNLOAD", "NOUNLOAD", "KEEP_REPLICATION", "KEEP_CDC", "KEEP_TEMPORAL_RETENTION", "ENABLE_BROKER", "ERROR_BROKER_CONVERSATIONS", "NEW_BROKER" })
				catalogue.Add(word, [new(false, None)]);

			foreach (var name in new[] { "STANDBY", "CREDENTIAL", "PASSWORD", "MEDIAPASSWORD", "MEDIANAME", "DBNAME", "RESTORE_OPTIONS" })
				catalogue.Add(name, [new(true, BackupString)]);

			foreach (var name in new[] { "BLOCKSIZE", "BUFFERCOUNT", "MAXTRANSFERSIZE", "STOPAT", "STOPATMARK", "STOPBEFOREMARK" })
				catalogue.Add(name, [new(true, BackupValue)]);

			return catalogue;
		}

		/// <summary>T-SQL: BACKUP CERTIFICATE (Transact-SQL). CertificateKeyItem: a file, the two passwords and an algorithm, each a string.</summary>
		static readonly Dictionary<string, Setting[]> PrivateKeyOptions = Settings(
			("FILE",                   true, StringValue),
			("ENCRYPTION BY PASSWORD", true, StringValue),
			("DECRYPTION BY PASSWORD", true, StringValue),
			("ALGORITHM",              true, StringValue));

		/// <summary>T-SQL: ALTER SEARCH PROPERTY LIST (Transact-SQL). PropertyListAction's settings, each a value.</summary>
		static readonly Dictionary<string, Setting[]> PropertyOptions = Settings(
			("PROPERTY_SET_GUID",    true, ValueExpression),
			("PROPERTY_INT_ID",      true, ValueExpression),
			("PROPERTY_DESCRIPTION", true, ValueExpression));

		/// <summary><c>ValueExpression</c>: a value, never DEFAULT and never a comparison.</summary>
		static bool ValueExpression(Expression? value)
		{
			return value is not (null or Expression.Default or Expression.Comparison);
		}

		/// <summary>BackupString: a string or a variable.</summary>
		static bool BackupString(Expression? value)
		{
			return StringValue(value) || value is Expression.Variable;
		}

		/// <summary>BackupValue: a string, a variable or an unsigned number.</summary>
		static bool BackupValue(Expression? value)
		{
			return BackupString(value) || UnsignedNumber(value);
		}

		/// <summary><c>Sql92.Lexical.UnsignedNumericLiteral</c>: digits, a decimal or an approximate number.</summary>
		static bool UnsignedNumber(Expression? value)
		{
			return value is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.Decimal or NumericLiteralKind.Approximate } number && Unsigned(number.Text, number.Kind);
		}

		/// <summary>
		/// T-SQL: CREATE DATABASE (Transact-SQL). CreateDatabaseOption: the settings of CreateDatabaseNames, written as SET
		/// writes them, and LEDGER, CATALOG_COLLATION and BACKUP_STORAGE_REDUNDANCY, a creation's own.
		/// </summary>
		static readonly Dictionary<string, Setting[]> CreateOptions = CreateCatalogue();

		static Dictionary<string, Setting[]> CreateCatalogue()
		{
			var catalogue = new Dictionary<string, Setting[]>(StringComparer.OrdinalIgnoreCase)
			{
				["LEDGER"]                    = [new(true, Words("ON", "OFF"))],
				["CATALOG_COLLATION"]         = [new(true, NameValue)],
				["BACKUP_STORAGE_REDUNDANCY"] = [new(true, StringValue)],
			};

			foreach (var name in new[] { "TRUSTWORTHY", "DB_CHAINING", "NESTED_TRIGGERS", "TRANSFORM_NOISE_WORDS", "DEFAULT_LANGUAGE", "DEFAULT_FULLTEXT_LANGUAGE", "TWO_DIGIT_YEAR_CUTOFF", "FILESTREAM", "PERSISTENT_LOG_BUFFER" })
				catalogue.Add(name, DatabaseSettings[name]);

			return catalogue;
		}

		/// <summary>
		/// T-SQL: CREATE DATABASE (Transact-SQL). AttachOption: a creation's settings, the broker's words and
		/// RESTRICTED_USER, and a FILESTREAM that says only where its directory is.
		/// </summary>
		static readonly Dictionary<string, Setting[]> AttachOptions = AttachCatalogue(broker: true);

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL). RebuildLogOption: AttachOption without the attaching's own words.</summary>
		static readonly Dictionary<string, Setting[]> RebuildLogOptions = AttachCatalogue(broker: false);

		static Dictionary<string, Setting[]> AttachCatalogue(bool broker)
		{
			var catalogue = new Dictionary<string, Setting[]>(CreateOptions, StringComparer.OrdinalIgnoreCase)
			{
				["FILESTREAM"] = [new(false, None, new(Settings(("DIRECTORY_NAME", true, static value => StringValue(value) || value is LiteralValue.Null)), Required: true, Single: true))],
			};

			if (broker)
			{
				foreach (var word in new[] { "RESTRICTED_USER", "ENABLE_BROKER", "NEW_BROKER", "ERROR_BROKER_CONVERSATIONS" })
					catalogue.Add(word, [new(false, None)]);
			}

			return catalogue;
		}

		/// <summary>T-SQL: CREATE DATABASE (Transact-SQL), the Azure syntax. BackupRedundancy: a copy's one option.</summary>
		static readonly Dictionary<string, Setting[]> CopyOptions = Settings(("BACKUP_STORAGE_REDUNDANCY", true, StringValue));

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
