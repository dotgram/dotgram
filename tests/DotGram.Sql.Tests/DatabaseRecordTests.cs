using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Database family's rules (Build.Database.cs): which record and members they build for a text, where another
/// record or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class DatabaseRecordTests
{
	[Theory]
	[InlineData("ALTER DATABASE CURRENT REMOVE FILE a", "CURRENT")]
	[InlineData("ALTER DATABASE [CURRENT] REMOVE FILE a", "a database called CURRENT")]
	[InlineData("ALTER DATABASE d MODIFY FILEGROUP g READONLY", "READONLY")]
	[InlineData("ALTER DATABASE d MODIFY FILEGROUP g READ_ONLY", "READ_ONLY")]
	[InlineData("ALTER DATABASE d MODIFY FILEGROUP g NAME = h", "a filegroup renamed")]
	[InlineData("ALTER DATABASE d MODIFY NAME = e", "MODIFY NAME")]
	[InlineData("ALTER DATABASE d MODIFY FILE (NAME = a, SIZE = 10 MB)", "MODIFY FILE")]
	[InlineData("ALTER DATABASE d ADD FILE (NAME = a) TO FILEGROUP g", "ADD FILE")]
	[InlineData("ALTER DATABASE d ADD LOG FILE (NAME = a)", "ADD LOG FILE")]
	[InlineData("ALTER DATABASE d MODIFY (EDITION = 'Basic')", "MODIFY")]
	[InlineData("ALTER DATABASE d MODIFY (EDITION = 'Basic') WITH MANUAL_CUTOVER", "MODIFY WITH MANUAL_CUTOVER")]
	[InlineData("ALTER DATABASE d SET SINGLE_USER WITH ROLLBACK IMMEDIATE", "SET WITH ROLLBACK IMMEDIATE")]
	[InlineData("ALTER DATABASE d SET SINGLE_USER", "SET without termination")]
	[InlineData("ALTER DATABASE d SET SINGLE_USER WITH ROLLBACK AFTER 5 SECONDS", "ROLLBACK AFTER SECONDS")]
	[InlineData("ALTER DATABASE d SET SINGLE_USER WITH ROLLBACK AFTER 5", "ROLLBACK AFTER")]
	[InlineData("ALTER DATABASE d SET SINGLE_USER WITH NO_WAIT", "NO_WAIT")]
	[InlineData("ALTER DATABASE CURRENT SET ONLINE", "SET on CURRENT")]
	[InlineData("ALTER DATABASE d SET RECOVERY FULL", "a word value with no =")]
	[InlineData("ALTER DATABASE d SET QUERY_STORE CLEAR ALL", "a name of three words")]
	[InlineData("ALTER DATABASE d SET PARTNER SAFETY OFF", "a name of two words and a value")]
	[InlineData("ALTER DATABASE d SET PARTNER OFF", "PARTNER OFF")]
	[InlineData("ALTER DATABASE d SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT ON", "= left out")]
	[InlineData("ALTER DATABASE d SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT = ON", "= written")]
	[InlineData("ALTER DATABASE d SET AUTO_CLOSE ON", "a switch with no =")]
	[InlineData("ALTER DATABASE d SET CHANGE_TRACKING (AUTO_CLEANUP = ON)", "a list with no =")]
	[InlineData("ALTER DATABASE d SET CHANGE_TRACKING = ON (AUTO_CLEANUP = ON)", "a list after = ON")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY", "FOR SECONDARY")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 4", "not for the secondary")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE", "the whole cache")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE 0x01", "one plan")]
	[InlineData("CREATE DATABASE d ON PRIMARY (NAME = f)", "ON PRIMARY")]
	[InlineData("CREATE DATABASE d ON (NAME = f)", "ON")]
	[InlineData("CREATE DATABASE d ON (NAME = f), FILEGROUP g (NAME = h), (NAME = i)", "a filegroup's files")]
	[InlineData("CREATE DATABASE d ON (NAME = f), FILEGROUP g CONTAINS FILESTREAM DEFAULT (NAME = h)", "a filegroup's words")]
	[InlineData("CREATE DATABASE d ON (NAME = f) LOG ON (NAME = l)", "LOG ON")]
	[InlineData("CREATE DATABASE d CONTAINMENT = NONE", "CONTAINMENT = NONE")]
	[InlineData("CREATE DATABASE d", "no containment")]
	[InlineData("CREATE DATABASE d ON (NAME = f) FOR ATTACH", "FOR ATTACH")]
	[InlineData("CREATE DATABASE d ON (NAME = f) FOR ATTACH_REBUILD_LOG", "FOR ATTACH_REBUILD_LOG")]
	[InlineData("CREATE DATABASE s ON (NAME = f) AS SNAPSHOT OF d", "AS SNAPSHOT OF")]
	[InlineData("CREATE DATABASE c AS COPY OF s.d", "AS COPY OF")]
	[InlineData("CREATE DATABASE c AS COPY OF d (EDITION = 'Basic')", "a copy's edition options")]
	[InlineData("CREATE DATABASE d (EDITION = 'Basic')", "Azure's form")]
	[InlineData("CREATE DATABASE d WITH TRUSTWORTHY ON", "WITH")]
	[InlineData("BACKUP DATABASE d TO a", "BACKUP DATABASE")]
	[InlineData("BACKUP LOG d TO a", "BACKUP LOG")]
	[InlineData("BACKUP DATABASE @d TO @a", "variables")]
	[InlineData("BACKUP DATABASE d TO TAPE = 't'", "a tape")]
	[InlineData("BACKUP DATABASE d TO URL = 'u'", "a URL")]
	[InlineData("BACKUP DATABASE d FILEGROUP = 'g' TO a MIRROR TO b", "a filegroup and a mirror")]
	[InlineData("BACKUP DATABASE d TO a WITH STATS", "STATS alone")]
	[InlineData("BACKUP DATABASE d TO a WITH STATS = 5", "STATS = 5")]
	[InlineData("BACKUP DATABASE d TO a WITH ENCRYPTION (ALGORITHM = AES_128, SERVER CERTIFICATE = c)", "an encryptor of two words")]
	[InlineData("BACKUP MASTER KEY TO FILE = 'f' ENCRYPTION BY PASSWORD = 'p'", "a key to a file")]
	[InlineData("BACKUP MASTER KEY TO URL = 'f' ENCRYPTION BY PASSWORD = 'p'", "a key to a URL")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH PRIVATE KEY (FILE = 'k', ENCRYPTION BY PASSWORD = 'p')", "a private key")]
	[InlineData("RESTORE MASTER KEY FROM FILE = 'f' DECRYPTION BY PASSWORD = 'd' ENCRYPTION BY PASSWORD = 'e' FORCE", "FORCE")]
	[InlineData("RESTORE MASTER KEY FROM FILE = 'f' DECRYPTION BY PASSWORD = 'd' ENCRYPTION BY PASSWORD = 'e'", "no FORCE")]
	[InlineData("RESTORE DATABASE d FROM DATABASE_SNAPSHOT = 's'", "from a snapshot")]
	[InlineData("RESTORE DATABASE d FROM s", "from a device named s")]
	[InlineData("RESTORE LOG d FROM a", "RESTORE LOG")]
	[InlineData("RESTORE HEADERONLY FROM a", "HEADERONLY")]
	[InlineData("RESTORE LABELONLY FROM a", "LABELONLY")]
	[InlineData("RESTORE VERIFYONLY FROM a", "VERIFYONLY")]
	[InlineData("CREATE PARTITION FUNCTION f (INT) AS RANGE LEFT FOR VALUES (1)", "RANGE LEFT")]
	[InlineData("CREATE PARTITION FUNCTION f (INT) AS RANGE FOR VALUES (1)", "RANGE alone")]
	[InlineData("CREATE PARTITION FUNCTION f (CHAR(10) COLLATE Estonian_CS_AS) AS RANGE RIGHT FOR VALUES ()", "a collation")]
	[InlineData("ALTER PARTITION FUNCTION f () SPLIT RANGE (1)", "SPLIT")]
	[InlineData("ALTER PARTITION FUNCTION f () MERGE RANGE (1)", "MERGE")]
	[InlineData("ALTER PARTITION FUNCTION f () SPLIT", "SPLIT alone")]
	[InlineData("CREATE PARTITION SCHEME s AS PARTITION f ALL TO (fg)", "ALL")]
	[InlineData("CREATE PARTITION SCHEME s AS PARTITION f TO (fg)", "not ALL")]
	[InlineData("CREATE PARTITION SCHEME s AS PARTITION f TO ('fg')", "a filegroup quoted")]
	[InlineData("ALTER PARTITION SCHEME s NEXT USED", "NEXT USED alone")]
	[InlineData("CREATE FULLTEXT CATALOG c WITH ACCENT_SENSITIVITY = OFF", "insensitive")]
	[InlineData("CREATE FULLTEXT CATALOG c AS DEFAULT", "AS DEFAULT")]
	[InlineData("ALTER FULLTEXT CATALOG c REBUILD", "REBUILD")]
	[InlineData("ALTER FULLTEXT CATALOG c AS DEFAULT", "a catalog made the default")]
	[InlineData("CREATE FULLTEXT STOPLIST s FROM SYSTEM STOPLIST", "FROM SYSTEM STOPLIST")]
	[InlineData("CREATE FULLTEXT STOPLIST s FROM [SYSTEM]", "FROM a stoplist called SYSTEM")]
	[InlineData("ALTER FULLTEXT STOPLIST s DROP ALL", "DROP ALL")]
	[InlineData("ALTER FULLTEXT STOPLIST s DROP 'a' LANGUAGE 1033", "DROP a word")]
	[InlineData("ALTER FULLTEXT STOPLIST s ADD 'a' LANGUAGE [English]", "ADD a word")]
	[InlineData("ALTER SEARCH PROPERTY LIST l DROP 'a'", "DROP a property")]
	[InlineData("CHECKPOINT 5", "CHECKPOINT")]
	public void The_rules_build_the_database_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		var holds = expected switch
		{
			"CURRENT" => made is Ast.Statement.AlterDatabase { Current: true, Name: null },
			"a database called CURRENT" => made is Ast.Statement.AlterDatabase { Current: false, Name.Text: "[CURRENT]" },
			"READONLY" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.ModifyFileGroup { Change: Ast.FileGroupChange.Updatability { Kind: Ast.FileGroupUpdatability.ReadOnly, Underscored: false } } },
			"READ_ONLY" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.ModifyFileGroup { Change: Ast.FileGroupChange.Updatability { Kind: Ast.FileGroupUpdatability.ReadOnly, Underscored: true } } },
			"a filegroup renamed" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.ModifyFileGroup { Change: Ast.FileGroupChange.Rename } },
			"MODIFY NAME" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.ModifyName { Name.Text: "e" } },
			"MODIFY FILE" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.ModifyFile { File.Options: [_, { Value: Ast.Expression.Measured { Unit.Text: "MB" } }] } },
			"ADD FILE" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.AddFile { ToFileGroup.Text: "g", Files.Count: 1 } },
			"ADD LOG FILE" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.AddLogFile },
			"MODIFY" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Modify { ManualCutover: false } },
			"MODIFY WITH MANUAL_CUTOVER" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Modify { ManualCutover: true } },
			"SET WITH ROLLBACK IMMEDIATE" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set, Termination: Ast.Termination.RollbackImmediate },
			"SET without termination" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set, Termination: null },
			"ROLLBACK AFTER SECONDS" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set, Termination: Ast.Termination.RollbackAfter { Seconds: true, Number.Text: "5" } },
			"ROLLBACK AFTER" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set, Termination: Ast.Termination.RollbackAfter { Seconds: false } },
			"NO_WAIT" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set, Termination: Ast.Termination.NoWait },
			"SET on CURRENT" => made is Ast.Statement.AlterDatabase { Current: true, Name: null, Action: Ast.AlterDatabaseAction.Set },
			"a word value with no =" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ Name.Text: "RECOVERY", Words.Count: 0, Value: Ast.Expression.Reference { Name: [{ Text: "FULL" }] }, EqualsOmitted: false }] } },
			"a name of three words" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ Name.Text: "QUERY_STORE", Words: [{ Text: "CLEAR" }, { Text: "ALL" }], Value: null }] } },
			"a name of two words and a value" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ Name.Text: "PARTNER", Words: [{ Text: "SAFETY" }], Value: Ast.Expression.Reference }] } },
			"PARTNER OFF" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ Name.Text: "PARTNER", Words.Count: 0, Value: Ast.Expression.Reference }] } },
			"= left out" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ EqualsOmitted: true }] } },
			"= written" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ EqualsOmitted: false }] } },
			"a switch with no =" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ EqualsOmitted: false, Value: Ast.Expression.Reference }] } },
			"a list with no =" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ Value: null, Options.Count: 1 }] } },
			"a list after = ON" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Options: [{ Value: Ast.Expression.Reference, Options.Count: 1 }] } },
			"FOR SECONDARY" => made is Ast.Statement.AlterDatabaseScopedConfiguration { Action: Ast.ScopedConfigurationAction.Set { Secondary: true } },
			"not for the secondary" => made is Ast.Statement.AlterDatabaseScopedConfiguration { Action: Ast.ScopedConfigurationAction.Set { Secondary: false } },
			"the whole cache" => made is Ast.Statement.AlterDatabaseScopedConfiguration { Action: Ast.ScopedConfigurationAction.ClearProcedureCache { PlanHandle: null } },
			"one plan" => made is Ast.Statement.AlterDatabaseScopedConfiguration { Action: Ast.ScopedConfigurationAction.ClearProcedureCache { PlanHandle.ZeroX: true } },
			"ON PRIMARY" => made is Ast.Statement.CreateDatabase { Primary: true, Files.Count: 1 },
			"ON" => made is Ast.Statement.CreateDatabase { Primary: false, Files.Count: 1 },
			"a filegroup's files" => made is Ast.Statement.CreateDatabase { Files.Count: 1, FileGroups: [{ Name.Text: "g", Files.Count: 2, Contains: null, Default: false }] },
			"a filegroup's words" => made is Ast.Statement.CreateDatabase { FileGroups: [{ Contains: Ast.FileGroupContents.Filestream, Default: true }] },
			"LOG ON" => made is Ast.Statement.CreateDatabase { Files.Count: 1, LogFiles.Count: 1 },
			"CONTAINMENT = NONE" => made is Ast.Statement.CreateDatabase { Containment: Ast.DatabaseContainment.None },
			"no containment" => made is Ast.Statement.CreateDatabase { Containment: null, Source: null, Options.Count: 0 },
			"FOR ATTACH" => made is Ast.Statement.CreateDatabase { Source: Ast.DatabaseSource.ForAttach { Kind: Ast.AttachKind.Attach } },
			"FOR ATTACH_REBUILD_LOG" => made is Ast.Statement.CreateDatabase { Source: Ast.DatabaseSource.ForAttach { Kind: Ast.AttachKind.AttachRebuildLog } },
			"AS SNAPSHOT OF" => made is Ast.Statement.CreateDatabase { Source: Ast.DatabaseSource.AsSnapshotOf { Database.Text: "d" } },
			"AS COPY OF" => made is Ast.Statement.CreateDatabase { Source: Ast.DatabaseSource.AsCopyOf { Database.Count: 2 } },
			"a copy's edition options" => made is Ast.Statement.CreateDatabase { Source: Ast.DatabaseSource.AsCopyOf, EditionOptions.Count: 1 },
			"Azure's form" => made is Ast.Statement.CreateDatabase { Source: null, EditionOptions.Count: 1 },
			"WITH" => made is Ast.Statement.CreateDatabase { Options: [{ Name.Text: "TRUSTWORTHY", EqualsOmitted: false }] },
			"BACKUP DATABASE" => made is Ast.Statement.BackupDatabase { Database: Ast.Expression.Reference, To: [Ast.BackupDevice.Logical { Name: not null, Variable: null }] },
			"BACKUP LOG" => made is Ast.Statement.BackupLog { Database: Ast.Expression.Reference },
			"variables" => made is Ast.Statement.BackupDatabase { Database: Ast.Expression.Variable, To: [Ast.BackupDevice.Logical { Name: null, Variable: not null }] },
			"a tape" => made is Ast.Statement.BackupDatabase { To: [Ast.BackupDevice.Physical { Kind: Ast.DeviceKind.Tape }] },
			"a URL" => made is Ast.Statement.BackupDatabase { To: [Ast.BackupDevice.Physical { Kind: Ast.DeviceKind.Url }] },
			"a filegroup and a mirror" => made is Ast.Statement.BackupDatabase { Files: [Ast.BackupFile.FileGroup], Mirrors: [{ Devices.Count: 1 }] },
			"STATS alone" => made is Ast.Statement.BackupDatabase { Options: [{ Value: null }] },
			"STATS = 5" => made is Ast.Statement.BackupDatabase { Options: [{ Value: Ast.LiteralValue.Numeric }] },
			"an encryptor of two words" => made is Ast.Statement.BackupDatabase { Options: [{ Options: [_, { Name.Text: "SERVER", Words: [{ Text: "CERTIFICATE" }] }] }] },
			"a key to a file" => made is Ast.Statement.BackupMasterKey { To.Kind: Ast.KeyPlaceKind.File },
			"a key to a URL" => made is Ast.Statement.BackupMasterKey { To.Kind: Ast.KeyPlaceKind.Url },
			"a private key" => made is Ast.Statement.BackupCertificate { Format: null, PrivateKey: [_, { Name.Text: "ENCRYPTION", Words.Count: 2 }] },
			"FORCE" => made is Ast.Statement.RestoreMasterKey { Force: true, DecryptionPassword: Ast.LiteralValue.String { Text: "'d'" } },
			"no FORCE" => made is Ast.Statement.RestoreMasterKey { Force: false },
			"from a snapshot" => made is Ast.Statement.RestoreDatabase { From.Count: 0, DatabaseSnapshot: not null },
			"from a device named s" => made is Ast.Statement.RestoreDatabase { From.Count: 1, DatabaseSnapshot: null },
			"RESTORE LOG" => made is Ast.Statement.RestoreLog,
			"HEADERONLY" => made is Ast.Statement.RestoreHeaderOnly,
			"LABELONLY" => made is Ast.Statement.RestoreLabelOnly,
			"VERIFYONLY" => made is Ast.Statement.RestoreVerifyOnly,
			"RANGE LEFT" => made is Ast.Statement.CreatePartitionFunction { Range: Ast.RangeSide.Left, Boundaries.Count: 1, Collation: null },
			"RANGE alone" => made is Ast.Statement.CreatePartitionFunction { Range: null },
			"a collation" => made is Ast.Statement.CreatePartitionFunction { Range: Ast.RangeSide.Right, Boundaries.Count: 0, Collation: not null },
			"SPLIT" => made is Ast.Statement.AlterPartitionFunction { Action: Ast.PartitionFunctionAction.Split { Boundary: not null } },
			"MERGE" => made is Ast.Statement.AlterPartitionFunction { Action: Ast.PartitionFunctionAction.Merge { Boundary: not null } },
			"SPLIT alone" => made is Ast.Statement.AlterPartitionFunction { Action: Ast.PartitionFunctionAction.Split { Boundary: null } },
			"ALL" => made is Ast.Statement.CreatePartitionScheme { All: true },
			"not ALL" => made is Ast.Statement.CreatePartitionScheme { All: false, FileGroups: [Ast.Expression.Reference] },
			"a filegroup quoted" => made is Ast.Statement.CreatePartitionScheme { FileGroups: [Ast.LiteralValue.String] },
			"NEXT USED alone" => made is Ast.Statement.AlterPartitionScheme { NextUsed: null },
			"insensitive" => made is Ast.Statement.CreateFullTextCatalog { AccentSensitivity: false, Default: false },
			"AS DEFAULT" => made is Ast.Statement.CreateFullTextCatalog { AccentSensitivity: null, Default: true },
			"REBUILD" => made is Ast.Statement.AlterFullTextCatalog { Action: Ast.FullTextCatalogAction.Rebuild { AccentSensitivity: null } },
			"a catalog made the default" => made is Ast.Statement.AlterFullTextCatalog { Action: Ast.FullTextCatalogAction.AsDefault },
			"FROM SYSTEM STOPLIST" => made is Ast.Statement.CreateFullTextStopList { From: Ast.StopListSource.SystemStopList },
			"FROM a stoplist called SYSTEM" => made is Ast.Statement.CreateFullTextStopList { From: Ast.StopListSource.Named },
			"DROP ALL" => made is Ast.Statement.AlterFullTextStopList { Action: Ast.StopListAction.DropAll { Language: null } },
			"DROP a word" => made is Ast.Statement.AlterFullTextStopList { Action: Ast.StopListAction.Drop },
			"ADD a word" => made is Ast.Statement.AlterFullTextStopList { Action: Ast.StopListAction.Add { Language: Ast.Expression.Reference } },
			"DROP a property" => made is Ast.Statement.AlterSearchPropertyList { Action: Ast.SearchPropertyListAction.Drop },
			"CHECKPOINT" => made is Ast.Statement.Checkpoint { Duration: not null },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		ReadBack.Holds(text);
	}
}
