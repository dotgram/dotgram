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
