using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using ColumnDefinition = DotGram.Sql.Ast.ColumnDefinition;
using ColumnOption = DotGram.Sql.Ast.ColumnOption;
using Constraint = DotGram.Sql.Ast.Constraint;
using DataType = DotGram.Sql.Ast.DataType;
using ExecutionContext = DotGram.Sql.Ast.ExecutionContext;
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using InsertSource = DotGram.Sql.Ast.InsertSource;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;
using TableElement = DotGram.Sql.Ast.TableElement;
using UnaryOperator = DotGram.Sql.Ast.UnaryOperator;
using ComparisonOperator = DotGram.Sql.Ast.ComparisonOperator;

/// <summary>
/// The Database family's writer (Sql2023Writer.TransactSql.Database.cs): each record written and read back by
/// TransactSqlParser to the same tree, and every tree whose text would read back as another refused.
/// </summary>
public sealed class DatabaseWriterTests
{
	static readonly Identifier D = new("d");

	static Identifier Name(string text)
	{
		return new Identifier(text);
	}

	static Expression Text(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static Expression Ref(string word)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(word)]));
	}

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static Expression Sized(string number, string unit)
	{
		return new Expression.Measured(Number(number), Name(unit));
	}

	static Option Set(string name, Expression? value = null)
	{
		return new Option(Name(name), value, []);
	}

	static DatabaseFile File(params Option[] options)
	{
		return new DatabaseFile([.. options]);
	}

	static Statement.AlterDatabase Alter(AlterDatabaseAction action)
	{
		return new Statement.AlterDatabase(D, action);
	}

	static Statement.AlterDatabase SetOf(params Option[] options)
	{
		return Alter(new AlterDatabaseAction.Set([.. options]));
	}

	static Option Nested(string name, Expression? value, params Option[] options)
	{
		return new Option(Name(name), value, [.. options]);
	}

	static Option Worded(string name, string[] words, Expression? value = null)
	{
		return new Option(Name(name), value, []) { Words = [.. Array.ConvertAll(words, Name)] };
	}

	static Statement.CreateDatabase Create(string name = "d")
	{
		return new Statement.CreateDatabase(Name(name));
	}

	static readonly DatabaseFile F = File(Set("NAME", Ref("f")), Set("FILENAME", Text("f.mdf")));

	static Expression Var(string name)
	{
		return new Expression.Variable(Name(name));
	}

	static BackupDevice Disk(string path)
	{
		return new BackupDevice.Physical(DeviceKind.Disk, Text(path));
	}

	static BackupDevice Device(string name)
	{
		return new BackupDevice.Logical(new QualifiedName([Name(name)]), null);
	}

	static Expression Db(string name = "d")
	{
		return new Expression.Reference(new QualifiedName([Name(name)]));
	}

	static Statement.AlterDatabaseScopedConfiguration Scoped(Option option, bool secondary = false)
	{
		return new Statement.AlterDatabaseScopedConfiguration(new ScopedConfigurationAction.Set(option) { Secondary = secondary });
	}

	/// <summary>A tree for each record of the family, and where it reads: alone, or as a natively compiled body.</summary>
	static Statement Tree(string which)
	{
		return which switch
		{
			"COLLATE" => Alter(new AlterDatabaseAction.Collate(new CollationName(new QualifiedName([Name("Latin1_General_CI_AS")])))),
			"MODIFY NAME on CURRENT" => new Statement.AlterDatabase(null, new AlterDatabaseAction.ModifyName(Name("e"))) { Current = true },
			"MODIFY" => Alter(new AlterDatabaseAction.Modify([Set("EDITION", Text("Premium")), Set("MAXSIZE", Sized("500", "GB")), new Option(Name("SERVICE_OBJECTIVE"), Ref("ELASTIC_POOL"), [Set("NAME", Ref("p"))])], true)),
			"MODIFY BACKUP_STORAGE_REDUNDANCY" => Alter(new AlterDatabaseAction.ModifyBackupStorageRedundancy(Text("GEO"))),
			"PERFORM_CUTOVER" => Alter(new AlterDatabaseAction.PerformCutover()),
			"REBUILD LOG" => Alter(new AlterDatabaseAction.RebuildLog()),
			"ADD FILE" => Alter(new AlterDatabaseAction.AddFile([File(Set("NAME", Ref("a")), Set("FILENAME", Text("a.ndf")), Set("SIZE", Sized("10", "MB")), Set("MAXSIZE", Ref("UNLIMITED")), Set("FILEGROWTH", Sized("10", "%"))), File(Set("NAME", Text("b")), Set("SIZE", Number("5")), Set("OFFLINE"))], Name("g"))),
			"ADD LOG FILE" => Alter(new AlterDatabaseAction.AddLogFile([File(Set("NAME", Ref("l")))])),
			"MODIFY FILE" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("NAME", Ref("a")), Set("NEWNAME", Ref("b")), Set("MAXSIZE", Sized("2", "TB"))))),
			"REMOVE FILE" => Alter(new AlterDatabaseAction.RemoveFile(Name("a"))),
			"ADD FILEGROUP" => Alter(new AlterDatabaseAction.AddFileGroup(Name("g"), FileGroupContents.MemoryOptimizedData)),
			"ADD FILEGROUP FILESTREAM" => Alter(new AlterDatabaseAction.AddFileGroup(Name("g"), FileGroupContents.Filestream)),
			"MODIFY FILEGROUP READ_ONLY" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Updatability(FileGroupUpdatability.ReadOnly, true))),
			"MODIFY FILEGROUP READWRITE" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Updatability(FileGroupUpdatability.ReadWrite, false))),
			"MODIFY FILEGROUP DEFAULT" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Default())),
			"MODIFY FILEGROUP NAME" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Rename(Name("h")))),
			"MODIFY FILEGROUP AUTOGROW" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.AutoGrow(FileGroupAutoGrow.AllFiles))),
			"REMOVE FILEGROUP" => Alter(new AlterDatabaseAction.RemoveFileGroup(Name("g"))),
			"a database called CURRENT" => new Statement.AlterDatabase(new Identifier("[CURRENT]", IdentifierStyle.Bracketed), new AlterDatabaseAction.RemoveFile(Name("a"))),
			"SET words alone and values with no =" => SetOf(Set("SINGLE_USER"), Set("RECOVERY", Ref("BULK_LOGGED")), Set("AUTO_CLOSE", Ref("OFF")), Set("CURSOR_DEFAULT", Ref("LOCAL")), Set("PAGE_VERIFY", Ref("NONE")), Set("ENCRYPTION", Ref("SUSPEND"))),
			"SET switches with =" => SetOf(Set("NESTED_TRIGGERS", Ref("ON")), Set("CONTAINMENT", Ref("PARTIAL")), Set("DELAYED_DURABILITY", Ref("FORCED")), Set("AUTOMATIC_TUNING", Ref("AUTO"))),
			"SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT both ways" => SetOf(Set("MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT", Ref("ON")), Set("MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT", Ref("OFF")) with { EqualsOmitted = true }),
			"SET numbers and languages" => SetOf(Set("COMPATIBILITY_LEVEL", Number("160")), Set("TWO_DIGIT_YEAR_CUTOFF", Number("2049")), Set("DEFAULT_LANGUAGE", Ref("us_english")), Set("DEFAULT_FULLTEXT_LANGUAGE", Number("1033"))),
			"SET TARGET_RECOVERY_TIME" => SetOf(Set("TARGET_RECOVERY_TIME", new Expression.Measured(new LiteralValue.Numeric("42.5", NumericLiteralKind.Decimal), Name("SECONDS")))),
			"SET lists without =" => SetOf(Nested("AUTO_CREATE_STATISTICS", Ref("ON"), Set("INCREMENTAL", Ref("ON"))), Nested("AUTOMATIC_TUNING", null, Set("FORCE_LAST_GOOD_PLAN", Ref("DEFAULT"))), Nested("FILESTREAM", null, Set("NON_TRANSACTED_ACCESS", Ref("FULL")), Set("DIRECTORY_NAME", new LiteralValue.Null()))),
			"SET CHANGE_TRACKING" => SetOf(Nested("CHANGE_TRACKING", Ref("ON"), Set("AUTO_CLEANUP", Ref("ON")), Set("CHANGE_RETENTION", Sized("2", "DAYS"))), Nested("CHANGE_TRACKING", null, Set("AUTO_CLEANUP", Ref("OFF"))), Set("CHANGE_TRACKING", Ref("OFF"))),
			"SET QUERY_STORE" => SetOf(Nested("QUERY_STORE", Ref("ON"), Set("OPERATION_MODE", Ref("READ_WRITE")), Nested("CLEANUP_POLICY", null, Set("STALE_QUERY_THRESHOLD_DAYS", Number("30"))), Set("QUERY_CAPTURE_MODE", Ref("ALL")), Nested("QUERY_CAPTURE_POLICY", null, Set("STALE_CAPTURE_POLICY_THRESHOLD", Sized("1", "DAY")), Set("EXECUTION_COUNT", Number("30")))), Nested("QUERY_STORE", Ref("OFF"), Set("FORCED")), Worded("QUERY_STORE", ["CLEAR", "ALL"])),
			"SET with lists after =" => SetOf(Nested("REMOTE_DATA_ARCHIVE", Ref("ON"), Set("SERVER", Text("s")), Set("CREDENTIAL", Ref("c"))), Nested("PERSISTENT_LOG_BUFFER", Ref("ON"), Set("DIRECTORY_NAME", Text("x"))), Nested("SUSPEND_FOR_SNAPSHOT_BACKUP", Ref("ON"), Set("MODE", Ref("COPY_ONLY"))), Nested("ACCELERATED_DATABASE_RECOVERY", Ref("ON"), Set("PERSISTENT_VERSION_STORE_FILEGROUP", Ref("fg")))),
			"SET mirroring and HADR" => SetOf(Worded("PARTNER", ["SAFETY"], Ref("FULL")), Worded("PARTNER", ["TIMEOUT"], Number("10")), Set("PARTNER", Ref("FAILOVER")), Set("WITNESS", Text("tcp://w:5022")), Worded("HADR", ["AVAILABILITY", "GROUP"], Ref("ag")), Set("HADR", Ref("SUSPEND"))),
			"SET WITH ROLLBACK AFTER" => Alter(new AlterDatabaseAction.Set([Set("SINGLE_USER")]) { Termination = new Termination.RollbackAfter(new LiteralValue.Numeric("10", NumericLiteralKind.DecimalInteger)) { Seconds = true } }),
			"SET WITH ROLLBACK AFTER without SECONDS" => Alter(new AlterDatabaseAction.Set([Set("SINGLE_USER")]) { Termination = new Termination.RollbackAfter(new LiteralValue.Numeric("10", NumericLiteralKind.DecimalInteger)) }),
			"SET WITH ROLLBACK IMMEDIATE" => Alter(new AlterDatabaseAction.Set([Set("OFFLINE")]) { Termination = new Termination.RollbackImmediate() }),
			"SET WITH NO_WAIT on CURRENT" => new Statement.AlterDatabase(null, new AlterDatabaseAction.Set([Set("READ_ONLY")]) { Termination = new Termination.NoWait() }) { Current = true },
			"SCOPED switch FOR SECONDARY" => Scoped(Set("PARAMETER_SNIFFING", Ref("PRIMARY")), true),
			"SCOPED MAXDOP negative" => Scoped(Set("MAXDOP", new Expression.Unary(UnaryOperator.Minus, Number("1")))),
			"SCOPED words and strings" => Scoped(Set("ELEVATE_ONLINE", Ref("WHEN_SUPPORTED"))),
			"SCOPED LEDGER_DIGEST_STORAGE_ENDPOINT" => Scoped(Set("LEDGER_DIGEST_STORAGE_ENDPOINT", Text("https://a.b"))),
			"SCOPED DW_COMPATIBILITY_LEVEL" => Scoped(Set("DW_COMPATIBILITY_LEVEL", Ref("AUTO"))),
			"CLEAR PROCEDURE_CACHE" => new Statement.AlterDatabaseScopedConfiguration(new ScopedConfigurationAction.ClearProcedureCache(null)),
			"CLEAR PROCEDURE_CACHE of a plan" => new Statement.AlterDatabaseScopedConfiguration(new ScopedConfigurationAction.ClearProcedureCache(new LiteralValue.Binary("0102") { ZeroX = true })),
			"CREATE DATABASE alone" => Create(),
			"CREATE DATABASE whole" => Create() with { Containment = DatabaseContainment.Partial, Primary = true, Files = [F, File(Set("NAME", Ref("g")), Set("SIZE", Sized("10", "MB")))], FileGroups = [new FileGroup(Name("fs"), [File(Set("NAME", Ref("h")))]) { Contains = FileGroupContents.Filestream, Default = true }, new FileGroup(Name("mo"), [F]) { Contains = FileGroupContents.MemoryOptimizedData }], LogFiles = [File(Set("NAME", Ref("l")), Set("FILEGROWTH", Sized("10", "%")))], Collation = new CollationName(new QualifiedName([Name("Latin1_General_CI_AS")])), Options = [Set("TRUSTWORTHY", Ref("ON")), Set("NESTED_TRIGGERS", Ref("OFF")), Set("DEFAULT_LANGUAGE", Ref("us_english")), Set("TWO_DIGIT_YEAR_CUTOFF", Number("2049")), Set("LEDGER", Ref("ON")), Set("CATALOG_COLLATION", Ref("DATABASE_DEFAULT")), Nested("FILESTREAM", null, Set("NON_TRANSACTED_ACCESS", Ref("FULL"))), Nested("PERSISTENT_LOG_BUFFER", Ref("ON"), Set("DIRECTORY_NAME", Text("x")))] },
			"CREATE DATABASE log only" => Create() with { LogFiles = [F], Containment = DatabaseContainment.None },
			"CREATE DATABASE FOR ATTACH" => Create() with { Files = [F], Source = new DatabaseSource.ForAttach(AttachKind.Attach), Options = [Set("NEW_BROKER"), Set("RESTRICTED_USER"), Nested("FILESTREAM", null, Set("DIRECTORY_NAME", new LiteralValue.Null()))] },
			"CREATE DATABASE FOR ATTACH_REBUILD_LOG" => Create() with { Files = [F], Source = new DatabaseSource.ForAttach(AttachKind.AttachRebuildLog), Options = [Set("DB_CHAINING", Ref("ON"))] },
			"CREATE DATABASE FOR ATTACH_FORCE_REBUILD_LOG" => Create() with { Files = [F], Source = new DatabaseSource.ForAttach(AttachKind.AttachForceRebuildLog) },
			"CREATE DATABASE AS SNAPSHOT OF" => Create("s") with { Files = [F], Source = new DatabaseSource.AsSnapshotOf(D), Options = [Set("TRUSTWORTHY", Ref("OFF"))] },
			"CREATE DATABASE AS COPY OF" => Create("c") with { Source = new DatabaseSource.AsCopyOf(new QualifiedName([Name("s"), D])), EditionOptions = [new Option(Name("SERVICE_OBJECTIVE"), Ref("ELASTIC_POOL"), [Set("NAME", Ref("p"))])], Options = [Set("BACKUP_STORAGE_REDUNDANCY", Text("GEO"))] },
			"CREATE DATABASE Azure" => Create() with { Collation = new CollationName(new QualifiedName([Name("Latin1_General_CI_AS")])), EditionOptions = [Set("EDITION", Text("Premium")), Set("MAXSIZE", Sized("500", "GB"))], Options = [Set("BACKUP_STORAGE_REDUNDANCY", Text("LOCAL")), Set("LEDGER", Ref("OFF"))] },
			"BACKUP DATABASE whole" => new Statement.BackupDatabase(Db(), [Disk("a.bak"), new BackupDevice.Logical(null, (Expression.Variable)Var("@dev")), new BackupDevice.Physical(DeviceKind.Tape, Var("@t"))]) { Files = [new BackupFile.File(Text("f1")), new BackupFile.FileGroup(Db("g"))], Mirrors = [new BackupMirror([new BackupDevice.Physical(DeviceKind.Url, Text("u"))]), new BackupMirror([Device("m")])], Options = [Set("DIFFERENTIAL"), Nested("COMPRESSION", null, Set("ALGORITHM", Ref("ZSTD")), Set("LEVEL", Ref("HIGH"))), Nested("ENCRYPTION", null, Set("ALGORITHM", Ref("AES_256")), Worded("SERVER", ["ASYMMETRIC", "KEY"], Ref("k"))), Set("NAME", Var("@n")), Set("DESCRIPTION", Number("5")), Set("STATS"), Set("BLOCKSIZE", Text("x"))] },
			"BACKUP DATABASE READ_WRITE_FILEGROUPS of a variable" => new Statement.BackupDatabase(Var("@d"), [Device("dev")]) { Files = [new BackupFile.ReadWriteFileGroups()], Options = [Set("STATS", Number("10")), Set("COMPRESSION")] },
			"BACKUP LOG" => new Statement.BackupLog(Db(), [Disk("l")]) { Options = [Set("NORECOVERY"), Set("STANDBY", Var("@u")), Set("NO_TRUNCATE")] },
			"BACKUP SERVER" => new Statement.BackupServer([Disk("s")]) { Options = [Set("METADATA_ONLY")] },
			"BACKUP GROUP" => new Statement.BackupGroup([Db("a"), Db("b")], [Disk("g")]) { Options = [Set("METADATA_ONLY")] },
			"BACKUP CERTIFICATE" => new Statement.BackupCertificate(Name("c"), Text("c.cer")) { Format = Text("PFX"), PrivateKey = [Set("FILE", Text("c.pvk")), Worded("ENCRYPTION", ["BY", "PASSWORD"], Text("p")), Worded("DECRYPTION", ["BY", "PASSWORD"], Text("q")), Set("ALGORITHM", Text("aes_256"))] },
			"BACKUP CERTIFICATE alone" => new Statement.BackupCertificate(Name("c"), Text("c.cer")),
			"BACKUP MASTER KEY" => new Statement.BackupMasterKey(new KeyPlace(KeyPlaceKind.Url, Text("u")), Text("p")),
			"BACKUP SERVICE MASTER KEY" => new Statement.BackupServiceMasterKey(Text("f"), Text("p")),
			"BACKUP SYMMETRIC KEY" => new Statement.BackupSymmetricKey(Name("k"), new KeyPlace(KeyPlaceKind.File, Text("f")), Text("p")),
			"RESTORE DATABASE whole" => new Statement.RestoreDatabase(Db()) { Files = [new BackupFile.Page(Text("1:57")), new BackupFile.File(Var("@f"))], From = [Disk("a.bak")], Options = [Set("NORECOVERY"), Set("FILE", Number("2")), Set("STOPAT", Var("@t")), Nested("FILESTREAM", null, Set("DIRECTORY_NAME", new LiteralValue.Null())), Set("KEEP_TEMPORAL_RETENTION")] },
			"RESTORE DATABASE alone" => new Statement.RestoreDatabase(Db()),
			"RESTORE DATABASE from a snapshot" => new Statement.RestoreDatabase(Db()) { DatabaseSnapshot = Text("s") },
			"RESTORE LOG" => new Statement.RestoreLog(Var("@d")) { From = [Device("dev")], Options = [Set("RECOVERY"), Set("STOPATMARK", Text("m"))] },
			"RESTORE FILELISTONLY" => new Statement.RestoreFileListOnly([Disk("a")]) { Options = [Set("FILE", Number("1"))] },
			"RESTORE HEADERONLY" => new Statement.RestoreHeaderOnly([Disk("a")]),
			"RESTORE LABELONLY" => new Statement.RestoreLabelOnly([Disk("a")]) { Options = [Set("MEDIANAME", Text("m"))] },
			"RESTORE REWINDONLY" => new Statement.RestoreRewindOnly([Device("t")]) { Options = [Set("UNLOAD")] },
			"RESTORE VERIFYONLY" => new Statement.RestoreVerifyOnly([Disk("a")]) { Options = [Set("CHECKSUM")] },
			"RESTORE MASTER KEY" => new Statement.RestoreMasterKey(new KeyPlace(KeyPlaceKind.File, Text("f")), Text("d"), Text("e")) { Force = true },
			"RESTORE SERVICE MASTER KEY" => new Statement.RestoreServiceMasterKey(Text("f"), Text("d")),
			"RESTORE SYMMETRIC KEY" => new Statement.RestoreSymmetricKey(Name("k"), new KeyPlace(KeyPlaceKind.Url, Text("u")), Text("d"), Text("e")),
			"CREATE PARTITION FUNCTION" => new Statement.CreatePartitionFunction(Name("f"), new DataType.Character(CharacterTypeKind.Char, 10), [Number("1"), new LiteralValue.Null(), new Expression.Unary(UnaryOperator.Minus, Number("5")), Var("@x")]) { Collation = new CollationName(new QualifiedName([Name("Estonian_CS_AS")])), Range = RangeSide.Right },
			"CREATE PARTITION FUNCTION of no boundary" => new Statement.CreatePartitionFunction(Name("f"), new DataType.Numeric(NumericTypeKind.Int), []),
			"ALTER PARTITION FUNCTION SPLIT" => new Statement.AlterPartitionFunction(Name("f"), new PartitionFunctionAction.Split(Number("500"))),
			"ALTER PARTITION FUNCTION MERGE alone" => new Statement.AlterPartitionFunction(Name("f"), new PartitionFunctionAction.Merge(null)),
			"CREATE PARTITION SCHEME" => new Statement.CreatePartitionScheme(Name("s"), Name("f"), [Ref("fg1"), Text("fg2"), new Expression.Reference(new QualifiedName([new Identifier("[PRIMARY]", IdentifierStyle.Bracketed)]))]) { All = true },
			"ALTER PARTITION SCHEME" => new Statement.AlterPartitionScheme(Name("s"), Ref("fg4")),
			"ALTER PARTITION SCHEME of none" => new Statement.AlterPartitionScheme(Name("s"), null),
			"CREATE FULLTEXT CATALOG whole" => new Statement.CreateFullTextCatalog(Name("c")) { FileGroup = Name("fg"), Path = Text("p"), AccentSensitivity = false, Default = true, Authorization = Name("dbo") },
			"ALTER FULLTEXT CATALOG REBUILD" => new Statement.AlterFullTextCatalog(Name("c"), new FullTextCatalogAction.Rebuild(true)),
			"ALTER FULLTEXT CATALOG REORGANIZE" => new Statement.AlterFullTextCatalog(Name("c"), new FullTextCatalogAction.Reorganize()),
			"ALTER FULLTEXT CATALOG AS DEFAULT" => new Statement.AlterFullTextCatalog(Name("c"), new FullTextCatalogAction.AsDefault()),
			"CREATE FULLTEXT STOPLIST FROM SYSTEM" => new Statement.CreateFullTextStopList(Name("s")) { From = new StopListSource.SystemStopList(), Authorization = Name("dbo") },
			"CREATE FULLTEXT STOPLIST FROM a stoplist" => new Statement.CreateFullTextStopList(Name("s")) { From = new StopListSource.Named(new QualifiedName([Name("db"), Name("o")])) },
			"ALTER FULLTEXT STOPLIST ADD" => new Statement.AlterFullTextStopList(Name("s"), new StopListAction.Add(Text("sw"), Ref("THAI"))),
			"ALTER FULLTEXT STOPLIST DROP" => new Statement.AlterFullTextStopList(Name("s"), new StopListAction.Drop(Text("sw"), Number("1033"))),
			"ALTER FULLTEXT STOPLIST DROP ALL" => new Statement.AlterFullTextStopList(Name("s"), new StopListAction.DropAll(new LiteralValue.Binary("411") { ZeroX = true })),
			"CREATE SEARCH PROPERTY LIST" => new Statement.CreateSearchPropertyList(Name("l")) { From = new QualifiedName([Name("db"), Name("q")]), Authorization = Name("o") },
			"ALTER SEARCH PROPERTY LIST ADD" => new Statement.AlterSearchPropertyList(Name("l"), new SearchPropertyListAction.Add(Text("Author"), [Set("PROPERTY_SET_GUID", Text("g")), Set("PROPERTY_INT_ID", Number("4")), Set("PROPERTY_DESCRIPTION", Text("d"))])),
			"ALTER SEARCH PROPERTY LIST DROP" => new Statement.AlterSearchPropertyList(Name("l"), new SearchPropertyListAction.Drop(Text("Author"))),
			"CHECKPOINT" => new Statement.Checkpoint(Number("12")),
			"CHECKPOINT alone" => new Statement.Checkpoint(null),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>Each record written as its text.</summary>
	[Theory]
	[InlineData("COLLATE", "ALTER DATABASE d COLLATE Latin1_General_CI_AS")]
	[InlineData("MODIFY NAME on CURRENT", "ALTER DATABASE CURRENT MODIFY NAME = e")]
	[InlineData("MODIFY", "ALTER DATABASE d MODIFY (EDITION = 'Premium', MAXSIZE = 500 GB, SERVICE_OBJECTIVE = ELASTIC_POOL (NAME = p)) WITH MANUAL_CUTOVER")]
	[InlineData("MODIFY BACKUP_STORAGE_REDUNDANCY", "ALTER DATABASE d MODIFY BACKUP_STORAGE_REDUNDANCY = 'GEO'")]
	[InlineData("PERFORM_CUTOVER", "ALTER DATABASE d PERFORM_CUTOVER")]
	[InlineData("REBUILD LOG", "ALTER DATABASE d REBUILD LOG")]
	[InlineData("ADD FILE", "ALTER DATABASE d ADD FILE (NAME = a, FILENAME = 'a.ndf', SIZE = 10 MB, MAXSIZE = UNLIMITED, FILEGROWTH = 10 %), (NAME = 'b', SIZE = 5, OFFLINE) TO FILEGROUP g")]
	[InlineData("ADD LOG FILE", "ALTER DATABASE d ADD LOG FILE (NAME = l)")]
	[InlineData("MODIFY FILE", "ALTER DATABASE d MODIFY FILE (NAME = a, NEWNAME = b, MAXSIZE = 2 TB)")]
	[InlineData("REMOVE FILE", "ALTER DATABASE d REMOVE FILE a")]
	[InlineData("ADD FILEGROUP", "ALTER DATABASE d ADD FILEGROUP g CONTAINS MEMORY_OPTIMIZED_DATA")]
	[InlineData("ADD FILEGROUP FILESTREAM", "ALTER DATABASE d ADD FILEGROUP g CONTAINS FILESTREAM")]
	[InlineData("MODIFY FILEGROUP READ_ONLY", "ALTER DATABASE d MODIFY FILEGROUP g READ_ONLY")]
	[InlineData("MODIFY FILEGROUP READWRITE", "ALTER DATABASE d MODIFY FILEGROUP g READWRITE")]
	[InlineData("MODIFY FILEGROUP DEFAULT", "ALTER DATABASE d MODIFY FILEGROUP g DEFAULT")]
	[InlineData("MODIFY FILEGROUP NAME", "ALTER DATABASE d MODIFY FILEGROUP g NAME = h")]
	[InlineData("MODIFY FILEGROUP AUTOGROW", "ALTER DATABASE d MODIFY FILEGROUP g AUTOGROW_ALL_FILES")]
	[InlineData("REMOVE FILEGROUP", "ALTER DATABASE d REMOVE FILEGROUP g")]
	[InlineData("a database called CURRENT", "ALTER DATABASE [CURRENT] REMOVE FILE a")]
	[InlineData("SET words alone and values with no =", "ALTER DATABASE d SET SINGLE_USER, RECOVERY BULK_LOGGED, AUTO_CLOSE OFF, CURSOR_DEFAULT LOCAL, PAGE_VERIFY NONE, ENCRYPTION SUSPEND")]
	[InlineData("SET switches with =", "ALTER DATABASE d SET NESTED_TRIGGERS = ON, CONTAINMENT = PARTIAL, DELAYED_DURABILITY = FORCED, AUTOMATIC_TUNING = AUTO")]
	[InlineData("SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT both ways", "ALTER DATABASE d SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT = ON, MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT OFF")]
	[InlineData("SET numbers and languages", "ALTER DATABASE d SET COMPATIBILITY_LEVEL = 160, TWO_DIGIT_YEAR_CUTOFF = 2049, DEFAULT_LANGUAGE = us_english, DEFAULT_FULLTEXT_LANGUAGE = 1033")]
	[InlineData("SET TARGET_RECOVERY_TIME", "ALTER DATABASE d SET TARGET_RECOVERY_TIME = 42.5 SECONDS")]
	[InlineData("SET lists without =", "ALTER DATABASE d SET AUTO_CREATE_STATISTICS ON (INCREMENTAL = ON), AUTOMATIC_TUNING (FORCE_LAST_GOOD_PLAN = DEFAULT), FILESTREAM (NON_TRANSACTED_ACCESS = FULL, DIRECTORY_NAME = NULL)")]
	[InlineData("SET CHANGE_TRACKING", "ALTER DATABASE d SET CHANGE_TRACKING = ON (AUTO_CLEANUP = ON, CHANGE_RETENTION = 2 DAYS), CHANGE_TRACKING (AUTO_CLEANUP = OFF), CHANGE_TRACKING = OFF")]
	[InlineData("SET QUERY_STORE", "ALTER DATABASE d SET QUERY_STORE = ON (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), QUERY_CAPTURE_MODE = ALL, QUERY_CAPTURE_POLICY = (STALE_CAPTURE_POLICY_THRESHOLD = 1 DAY, EXECUTION_COUNT = 30)), QUERY_STORE = OFF (FORCED), QUERY_STORE CLEAR ALL")]
	[InlineData("SET with lists after =", "ALTER DATABASE d SET REMOTE_DATA_ARCHIVE = ON (SERVER = 's', CREDENTIAL = c), PERSISTENT_LOG_BUFFER = ON (DIRECTORY_NAME = 'x'), SUSPEND_FOR_SNAPSHOT_BACKUP = ON (MODE = COPY_ONLY), ACCELERATED_DATABASE_RECOVERY = ON (PERSISTENT_VERSION_STORE_FILEGROUP = fg)")]
	[InlineData("SET mirroring and HADR", "ALTER DATABASE d SET PARTNER SAFETY FULL, PARTNER TIMEOUT 10, PARTNER FAILOVER, WITNESS = 'tcp://w:5022', HADR AVAILABILITY GROUP = ag, HADR SUSPEND")]
	[InlineData("SET WITH ROLLBACK AFTER", "ALTER DATABASE d SET SINGLE_USER WITH ROLLBACK AFTER 10 SECONDS")]
	[InlineData("SET WITH ROLLBACK AFTER without SECONDS", "ALTER DATABASE d SET SINGLE_USER WITH ROLLBACK AFTER 10")]
	[InlineData("SET WITH ROLLBACK IMMEDIATE", "ALTER DATABASE d SET OFFLINE WITH ROLLBACK IMMEDIATE")]
	[InlineData("SET WITH NO_WAIT on CURRENT", "ALTER DATABASE CURRENT SET READ_ONLY WITH NO_WAIT")]
	[InlineData("SCOPED switch FOR SECONDARY", "ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY")]
	[InlineData("SCOPED MAXDOP negative", "ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = -1")]
	[InlineData("SCOPED words and strings", "ALTER DATABASE SCOPED CONFIGURATION SET ELEVATE_ONLINE = WHEN_SUPPORTED")]
	[InlineData("SCOPED LEDGER_DIGEST_STORAGE_ENDPOINT", "ALTER DATABASE SCOPED CONFIGURATION SET LEDGER_DIGEST_STORAGE_ENDPOINT = 'https://a.b'")]
	[InlineData("SCOPED DW_COMPATIBILITY_LEVEL", "ALTER DATABASE SCOPED CONFIGURATION SET DW_COMPATIBILITY_LEVEL = AUTO")]
	[InlineData("CLEAR PROCEDURE_CACHE", "ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE")]
	[InlineData("CLEAR PROCEDURE_CACHE of a plan", "ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE 0x0102")]
	[InlineData("CREATE DATABASE alone", "CREATE DATABASE d")]
	[InlineData("CREATE DATABASE whole", "CREATE DATABASE d CONTAINMENT = PARTIAL ON PRIMARY (NAME = f, FILENAME = 'f.mdf'), (NAME = g, SIZE = 10 MB), FILEGROUP fs CONTAINS FILESTREAM DEFAULT (NAME = h), FILEGROUP mo CONTAINS MEMORY_OPTIMIZED_DATA (NAME = f, FILENAME = 'f.mdf') LOG ON (NAME = l, FILEGROWTH = 10 %) COLLATE Latin1_General_CI_AS WITH TRUSTWORTHY ON, NESTED_TRIGGERS = OFF, DEFAULT_LANGUAGE = us_english, TWO_DIGIT_YEAR_CUTOFF = 2049, LEDGER = ON, CATALOG_COLLATION = DATABASE_DEFAULT, FILESTREAM (NON_TRANSACTED_ACCESS = FULL), PERSISTENT_LOG_BUFFER = ON (DIRECTORY_NAME = 'x')")]
	[InlineData("CREATE DATABASE log only", "CREATE DATABASE d CONTAINMENT = NONE LOG ON (NAME = f, FILENAME = 'f.mdf')")]
	[InlineData("CREATE DATABASE FOR ATTACH", "CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf') FOR ATTACH WITH NEW_BROKER, RESTRICTED_USER, FILESTREAM (DIRECTORY_NAME = NULL)")]
	[InlineData("CREATE DATABASE FOR ATTACH_REBUILD_LOG", "CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf') FOR ATTACH_REBUILD_LOG WITH DB_CHAINING ON")]
	[InlineData("CREATE DATABASE FOR ATTACH_FORCE_REBUILD_LOG", "CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf') FOR ATTACH_FORCE_REBUILD_LOG")]
	[InlineData("CREATE DATABASE AS SNAPSHOT OF", "CREATE DATABASE s ON (NAME = f, FILENAME = 'f.mdf') AS SNAPSHOT OF d WITH TRUSTWORTHY OFF")]
	[InlineData("CREATE DATABASE AS COPY OF", "CREATE DATABASE c AS COPY OF s.d (SERVICE_OBJECTIVE = ELASTIC_POOL (NAME = p)) WITH (BACKUP_STORAGE_REDUNDANCY = 'GEO')")]
	[InlineData("CREATE DATABASE Azure", "CREATE DATABASE d COLLATE Latin1_General_CI_AS (EDITION = 'Premium', MAXSIZE = 500 GB) WITH BACKUP_STORAGE_REDUNDANCY = 'LOCAL', LEDGER = OFF")]
	[InlineData("BACKUP DATABASE whole", "BACKUP DATABASE d FILE = 'f1', FILEGROUP = g TO DISK = 'a.bak', @dev, TAPE = @t MIRROR TO URL = 'u' MIRROR TO m WITH DIFFERENTIAL, COMPRESSION (ALGORITHM = ZSTD, LEVEL = HIGH), ENCRYPTION (ALGORITHM = AES_256, SERVER ASYMMETRIC KEY = k), NAME = @n, DESCRIPTION = 5, STATS, BLOCKSIZE = 'x'")]
	[InlineData("BACKUP DATABASE READ_WRITE_FILEGROUPS of a variable", "BACKUP DATABASE @d READ_WRITE_FILEGROUPS TO dev WITH STATS = 10, COMPRESSION")]
	[InlineData("BACKUP LOG", "BACKUP LOG d TO DISK = 'l' WITH NORECOVERY, STANDBY = @u, NO_TRUNCATE")]
	[InlineData("BACKUP SERVER", "BACKUP SERVER TO DISK = 's' WITH METADATA_ONLY")]
	[InlineData("BACKUP GROUP", "BACKUP GROUP a, b TO DISK = 'g' WITH METADATA_ONLY")]
	[InlineData("BACKUP CERTIFICATE", "BACKUP CERTIFICATE c TO FILE = 'c.cer' WITH FORMAT = 'PFX', PRIVATE KEY (FILE = 'c.pvk', ENCRYPTION BY PASSWORD = 'p', DECRYPTION BY PASSWORD = 'q', ALGORITHM = 'aes_256')")]
	[InlineData("BACKUP CERTIFICATE alone", "BACKUP CERTIFICATE c TO FILE = 'c.cer'")]
	[InlineData("BACKUP MASTER KEY", "BACKUP MASTER KEY TO URL = 'u' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("BACKUP SERVICE MASTER KEY", "BACKUP SERVICE MASTER KEY TO FILE = 'f' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("BACKUP SYMMETRIC KEY", "BACKUP SYMMETRIC KEY k TO FILE = 'f' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("RESTORE DATABASE whole", "RESTORE DATABASE d PAGE = '1:57', FILE = @f FROM DISK = 'a.bak' WITH NORECOVERY, FILE = 2, STOPAT = @t, FILESTREAM (DIRECTORY_NAME = NULL), KEEP_TEMPORAL_RETENTION")]
	[InlineData("RESTORE DATABASE alone", "RESTORE DATABASE d")]
	[InlineData("RESTORE DATABASE from a snapshot", "RESTORE DATABASE d FROM DATABASE_SNAPSHOT = 's'")]
	[InlineData("RESTORE LOG", "RESTORE LOG @d FROM dev WITH RECOVERY, STOPATMARK = 'm'")]
	[InlineData("RESTORE FILELISTONLY", "RESTORE FILELISTONLY FROM DISK = 'a' WITH FILE = 1")]
	[InlineData("RESTORE HEADERONLY", "RESTORE HEADERONLY FROM DISK = 'a'")]
	[InlineData("RESTORE LABELONLY", "RESTORE LABELONLY FROM DISK = 'a' WITH MEDIANAME = 'm'")]
	[InlineData("RESTORE REWINDONLY", "RESTORE REWINDONLY FROM t WITH UNLOAD")]
	[InlineData("RESTORE VERIFYONLY", "RESTORE VERIFYONLY FROM DISK = 'a' WITH CHECKSUM")]
	[InlineData("RESTORE MASTER KEY", "RESTORE MASTER KEY FROM FILE = 'f' DECRYPTION BY PASSWORD = 'd' ENCRYPTION BY PASSWORD = 'e' FORCE")]
	[InlineData("RESTORE SERVICE MASTER KEY", "RESTORE SERVICE MASTER KEY FROM FILE = 'f' DECRYPTION BY PASSWORD = 'd'")]
	[InlineData("RESTORE SYMMETRIC KEY", "RESTORE SYMMETRIC KEY k FROM URL = 'u' DECRYPTION BY PASSWORD = 'd' ENCRYPTION BY PASSWORD = 'e'")]
	[InlineData("CREATE PARTITION FUNCTION", "CREATE PARTITION FUNCTION f (CHAR(10) COLLATE Estonian_CS_AS) AS RANGE RIGHT FOR VALUES (1, NULL, -5, @x)")]
	[InlineData("CREATE PARTITION FUNCTION of no boundary", "CREATE PARTITION FUNCTION f (INT) AS RANGE FOR VALUES ()")]
	[InlineData("ALTER PARTITION FUNCTION SPLIT", "ALTER PARTITION FUNCTION f () SPLIT RANGE (500)")]
	[InlineData("ALTER PARTITION FUNCTION MERGE alone", "ALTER PARTITION FUNCTION f () MERGE")]
	[InlineData("CREATE PARTITION SCHEME", "CREATE PARTITION SCHEME s AS PARTITION f ALL TO (fg1, 'fg2', [PRIMARY])")]
	[InlineData("ALTER PARTITION SCHEME", "ALTER PARTITION SCHEME s NEXT USED fg4")]
	[InlineData("ALTER PARTITION SCHEME of none", "ALTER PARTITION SCHEME s NEXT USED")]
	[InlineData("CREATE FULLTEXT CATALOG whole", "CREATE FULLTEXT CATALOG c ON FILEGROUP fg IN PATH 'p' WITH ACCENT_SENSITIVITY = OFF AS DEFAULT AUTHORIZATION dbo")]
	[InlineData("ALTER FULLTEXT CATALOG REBUILD", "ALTER FULLTEXT CATALOG c REBUILD WITH ACCENT_SENSITIVITY = ON")]
	[InlineData("ALTER FULLTEXT CATALOG REORGANIZE", "ALTER FULLTEXT CATALOG c REORGANIZE")]
	[InlineData("ALTER FULLTEXT CATALOG AS DEFAULT", "ALTER FULLTEXT CATALOG c AS DEFAULT")]
	[InlineData("CREATE FULLTEXT STOPLIST FROM SYSTEM", "CREATE FULLTEXT STOPLIST s FROM SYSTEM STOPLIST AUTHORIZATION dbo")]
	[InlineData("CREATE FULLTEXT STOPLIST FROM a stoplist", "CREATE FULLTEXT STOPLIST s FROM db.o")]
	[InlineData("ALTER FULLTEXT STOPLIST ADD", "ALTER FULLTEXT STOPLIST s ADD 'sw' LANGUAGE THAI")]
	[InlineData("ALTER FULLTEXT STOPLIST DROP", "ALTER FULLTEXT STOPLIST s DROP 'sw' LANGUAGE 1033")]
	[InlineData("ALTER FULLTEXT STOPLIST DROP ALL", "ALTER FULLTEXT STOPLIST s DROP ALL LANGUAGE 0x411")]
	[InlineData("CREATE SEARCH PROPERTY LIST", "CREATE SEARCH PROPERTY LIST l FROM db.q AUTHORIZATION o")]
	[InlineData("ALTER SEARCH PROPERTY LIST ADD", "ALTER SEARCH PROPERTY LIST l ADD 'Author' WITH (PROPERTY_SET_GUID = 'g', PROPERTY_INT_ID = 4, PROPERTY_DESCRIPTION = 'd')")]
	[InlineData("ALTER SEARCH PROPERTY LIST DROP", "ALTER SEARCH PROPERTY LIST l DROP 'Author'")]
	[InlineData("CHECKPOINT", "CHECKPOINT 12")]
	[InlineData("CHECKPOINT alone", "CHECKPOINT")]
	public void Each_record_writes_its_text(string which, string text)
	{
		Assert.Equal(text, Sql2023Writer.Write(Tree(which)));
	}

	/// <summary>
	/// Each record's tree, written and read back by TransactSqlParser, carried by the bridge to the same tree.
	/// </summary>
	[Theory]
	[InlineData("COLLATE")]
	[InlineData("MODIFY NAME on CURRENT")]
	[InlineData("MODIFY")]
	[InlineData("MODIFY BACKUP_STORAGE_REDUNDANCY")]
	[InlineData("PERFORM_CUTOVER")]
	[InlineData("REBUILD LOG")]
	[InlineData("ADD FILE")]
	[InlineData("ADD LOG FILE")]
	[InlineData("MODIFY FILE")]
	[InlineData("REMOVE FILE")]
	[InlineData("ADD FILEGROUP")]
	[InlineData("ADD FILEGROUP FILESTREAM")]
	[InlineData("MODIFY FILEGROUP READ_ONLY")]
	[InlineData("MODIFY FILEGROUP READWRITE")]
	[InlineData("MODIFY FILEGROUP DEFAULT")]
	[InlineData("MODIFY FILEGROUP NAME")]
	[InlineData("MODIFY FILEGROUP AUTOGROW")]
	[InlineData("REMOVE FILEGROUP")]
	[InlineData("a database called CURRENT")]
	[InlineData("SET words alone and values with no =")]
	[InlineData("SET switches with =")]
	[InlineData("SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT both ways")]
	[InlineData("SET numbers and languages")]
	[InlineData("SET TARGET_RECOVERY_TIME")]
	[InlineData("SET lists without =")]
	[InlineData("SET CHANGE_TRACKING")]
	[InlineData("SET QUERY_STORE")]
	[InlineData("SET with lists after =")]
	[InlineData("SET mirroring and HADR")]
	[InlineData("SET WITH ROLLBACK AFTER")]
	[InlineData("SET WITH ROLLBACK AFTER without SECONDS")]
	[InlineData("SET WITH ROLLBACK IMMEDIATE")]
	[InlineData("SET WITH NO_WAIT on CURRENT")]
	[InlineData("SCOPED switch FOR SECONDARY")]
	[InlineData("SCOPED MAXDOP negative")]
	[InlineData("SCOPED words and strings")]
	[InlineData("SCOPED LEDGER_DIGEST_STORAGE_ENDPOINT")]
	[InlineData("SCOPED DW_COMPATIBILITY_LEVEL")]
	[InlineData("CLEAR PROCEDURE_CACHE")]
	[InlineData("CLEAR PROCEDURE_CACHE of a plan")]
	[InlineData("CREATE DATABASE alone")]
	[InlineData("CREATE DATABASE whole")]
	[InlineData("CREATE DATABASE log only")]
	[InlineData("CREATE DATABASE FOR ATTACH")]
	[InlineData("CREATE DATABASE FOR ATTACH_REBUILD_LOG")]
	[InlineData("CREATE DATABASE FOR ATTACH_FORCE_REBUILD_LOG")]
	[InlineData("CREATE DATABASE AS SNAPSHOT OF")]
	[InlineData("CREATE DATABASE AS COPY OF")]
	[InlineData("CREATE DATABASE Azure")]
	[InlineData("BACKUP DATABASE whole")]
	[InlineData("BACKUP DATABASE READ_WRITE_FILEGROUPS of a variable")]
	[InlineData("BACKUP LOG")]
	[InlineData("BACKUP SERVER")]
	[InlineData("BACKUP GROUP")]
	[InlineData("BACKUP CERTIFICATE")]
	[InlineData("BACKUP CERTIFICATE alone")]
	[InlineData("BACKUP MASTER KEY")]
	[InlineData("BACKUP SERVICE MASTER KEY")]
	[InlineData("BACKUP SYMMETRIC KEY")]
	[InlineData("RESTORE DATABASE whole")]
	[InlineData("RESTORE DATABASE alone")]
	[InlineData("RESTORE DATABASE from a snapshot")]
	[InlineData("RESTORE LOG")]
	[InlineData("RESTORE FILELISTONLY")]
	[InlineData("RESTORE HEADERONLY")]
	[InlineData("RESTORE LABELONLY")]
	[InlineData("RESTORE REWINDONLY")]
	[InlineData("RESTORE VERIFYONLY")]
	[InlineData("RESTORE MASTER KEY")]
	[InlineData("RESTORE SERVICE MASTER KEY")]
	[InlineData("RESTORE SYMMETRIC KEY")]
	[InlineData("CREATE PARTITION FUNCTION")]
	[InlineData("CREATE PARTITION FUNCTION of no boundary")]
	[InlineData("ALTER PARTITION FUNCTION SPLIT")]
	[InlineData("ALTER PARTITION FUNCTION MERGE alone")]
	[InlineData("CREATE PARTITION SCHEME")]
	[InlineData("ALTER PARTITION SCHEME")]
	[InlineData("ALTER PARTITION SCHEME of none")]
	[InlineData("CREATE FULLTEXT CATALOG whole")]
	[InlineData("ALTER FULLTEXT CATALOG REBUILD")]
	[InlineData("ALTER FULLTEXT CATALOG REORGANIZE")]
	[InlineData("ALTER FULLTEXT CATALOG AS DEFAULT")]
	[InlineData("CREATE FULLTEXT STOPLIST FROM SYSTEM")]
	[InlineData("CREATE FULLTEXT STOPLIST FROM a stoplist")]
	[InlineData("ALTER FULLTEXT STOPLIST ADD")]
	[InlineData("ALTER FULLTEXT STOPLIST DROP")]
	[InlineData("ALTER FULLTEXT STOPLIST DROP ALL")]
	[InlineData("CREATE SEARCH PROPERTY LIST")]
	[InlineData("ALTER SEARCH PROPERTY LIST ADD")]
	[InlineData("ALTER SEARCH PROPERTY LIST DROP")]
	[InlineData("CHECKPOINT")]
	[InlineData("CHECKPOINT alone")]
	public void Each_record_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(OldTree.Located.ParseStatement(text), text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's message
	/// that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("a name and CURRENT")]
	[InlineData("neither name nor CURRENT")]
	[InlineData("a database named CURRENT unbracketed")]
	[InlineData("MODIFY of nothing")]
	[InlineData("MODIFY of a name it does not read")]
	[InlineData("MODIFY MAXSIZE in KB")]
	[InlineData("MODIFY EDITION of a name")]
	[InlineData("BACKUP_STORAGE_REDUNDANCY of a word")]
	[InlineData("ADD FILE of nothing")]
	[InlineData("a file of no option")]
	[InlineData("a file option it does not read")]
	[InlineData("SIZE of a fraction")]
	[InlineData("SIZE in percent")]
	[InlineData("SIZE in PB")]
	[InlineData("OFFLINE with a value")]
	[InlineData("NAME of a number")]
	[InlineData("a file option with its = left out")]
	[InlineData("an ADD FILEGROUP with no text for what it holds")]
	[InlineData("a MODIFY FILEGROUP with no text for its updatability")]
	[InlineData("SET of nothing")]
	[InlineData("SET of a name it does not read")]
	[InlineData("SET RECOVERY ON")]
	[InlineData("SET ONLINE with a value")]
	[InlineData("SET AUTO_CLOSE with its = left out flagged")]
	[InlineData("SET NESTED_TRIGGERS with its = left out")]
	[InlineData("SET a quoted name")]
	[InlineData("SET a further word it does not read")]
	[InlineData("SET FILESTREAM with no list")]
	[InlineData("SET AUTO_CREATE_STATISTICS ON with two settings")]
	[InlineData("SET AUTO_CREATE_STATISTICS OFF with a list")]
	[InlineData("SET ONLINE with a list")]
	[InlineData("SET QUERY_STORE of a setting it does not read")]
	[InlineData("SET CHANGE_RETENTION in SECONDS")]
	[InlineData("SET TARGET_RECOVERY_TIME in HOURS")]
	[InlineData("SET COMPATIBILITY_LEVEL of a fraction")]
	[InlineData("SET DEFAULT_LANGUAGE of a string")]
	[InlineData("SET a setting with partitions")]
	[InlineData("SET ROLLBACK AFTER a fraction")]
	[InlineData("SCOPED of a name it does not read")]
	[InlineData("SCOPED ELEVATE_ONLINE ON")]
	[InlineData("SCOPED MAXDOP with a plus")]
	[InlineData("SCOPED switch without =")]
	[InlineData("CLEAR PROCEDURE_CACHE of an X literal")]
	[InlineData("CREATE DATABASE PRIMARY with no files")]
	[InlineData("CREATE DATABASE filegroups with no files")]
	[InlineData("CREATE DATABASE a filegroup of no file")]
	[InlineData("CREATE DATABASE a containment of no word")]
	[InlineData("CREATE DATABASE an option SET takes and a creation does not")]
	[InlineData("CREATE DATABASE TRUSTWORTHY with =")]
	[InlineData("CREATE DATABASE a broker word on a new database")]
	[InlineData("CREATE DATABASE FOR ATTACH two broker words")]
	[InlineData("CREATE DATABASE FOR ATTACH an option twice")]
	[InlineData("CREATE DATABASE FOR ATTACH_REBUILD_LOG a broker word")]
	[InlineData("CREATE DATABASE FOR ATTACH a FILESTREAM of NON_TRANSACTED_ACCESS")]
	[InlineData("CREATE DATABASE an attach of no word")]
	[InlineData("CREATE DATABASE AS COPY OF three parts")]
	[InlineData("CREATE DATABASE AS COPY OF two options")]
	[InlineData("CREATE DATABASE AS COPY OF a LEDGER")]
	[InlineData("CREATE DATABASE Azure with files")]
	[InlineData("CREATE DATABASE edition options on a snapshot")]
	[InlineData("CREATE DATABASE a filegroup holding no word")]
	[InlineData("BACKUP DATABASE to nothing")]
	[InlineData("BACKUP DATABASE a mirror of nothing")]
	[InlineData("BACKUP DATABASE a device both named and a variable")]
	[InlineData("BACKUP DATABASE a device neither named nor a variable")]
	[InlineData("BACKUP DATABASE a device named MIRROR")]
	[InlineData("BACKUP DATABASE a device of no kind")]
	[InlineData("BACKUP DATABASE a database named FILE")]
	[InlineData("BACKUP DATABASE a database of a national string")]
	[InlineData("BACKUP DATABASE a database of a number")]
	[InlineData("BACKUP option it does not read")]
	[InlineData("BACKUP COMPRESSION LEVEL first")]
	[InlineData("BACKUP ENCRYPTION without its encryptor")]
	[InlineData("BACKUP ENCRYPTION alone")]
	[InlineData("BACKUP DESCRIPTION of a word")]
	[InlineData("BACKUP NAME of a number")]
	[InlineData("BACKUP GROUP of nothing")]
	[InlineData("BACKUP CERTIFICATE a format without a private key")]
	[InlineData("BACKUP CERTIFICATE ALGORITHM twice")]
	[InlineData("BACKUP CERTIFICATE a private key setting it does not read")]
	[InlineData("BACKUP CERTIFICATE to a variable")]
	[InlineData("BACKUP MASTER KEY a password of a variable")]
	[InlineData("BACKUP MASTER KEY a place of no kind")]
	[InlineData("RESTORE DATABASE from devices and a snapshot")]
	[InlineData("RESTORE DATABASE from a snapshot by name")]
	[InlineData("RESTORE an option it does not read")]
	[InlineData("RESTORE MOVE as an option")]
	[InlineData("RESTORE FILE of a string")]
	[InlineData("RESTORE FILELISTONLY from nothing")]
	[InlineData("RESTORE SERVICE MASTER KEY from a variable")]
	[InlineData("CREATE PARTITION FUNCTION a boundary of DEFAULT")]
	[InlineData("CREATE PARTITION FUNCTION a side of no word")]
	[InlineData("ALTER PARTITION FUNCTION SPLIT at a comparison")]
	[InlineData("CREATE PARTITION SCHEME of no filegroup")]
	[InlineData("CREATE PARTITION SCHEME a filegroup of two parts")]
	[InlineData("ALTER PARTITION SCHEME a filegroup of a number")]
	[InlineData("CREATE FULLTEXT CATALOG a path of DEFAULT")]
	[InlineData("CREATE FULLTEXT STOPLIST from three parts")]
	[InlineData("ALTER FULLTEXT STOPLIST a word unquoted")]
	[InlineData("ALTER FULLTEXT STOPLIST a language of a variable")]
	[InlineData("ALTER SEARCH PROPERTY LIST ADD out of order")]
	[InlineData("ALTER SEARCH PROPERTY LIST ADD without its id")]
	[InlineData("CHECKPOINT of DEFAULT")]
	public void A_database_tree_with_no_text_is_refused(string which)
	{
		ISqlNode node = which switch
		{
			// DatabaseTarget: CURRENT or a name.
			"a name and CURRENT" => new Statement.AlterDatabase(D, new AlterDatabaseAction.RebuildLog()) { Current = true },
			// AlterDatabaseStatement: DatabaseTarget.
			"neither name nor CURRENT" => new Statement.AlterDatabase(null, new AlterDatabaseAction.RebuildLog()),
			// DatabaseTarget: the word CURRENT.
			"a database named CURRENT unbracketed" => new Statement.AlterDatabase(Name("current"), new AlterDatabaseAction.RebuildLog()),
			// EditionOptions: one at least.
			"MODIFY of nothing" => Alter(new AlterDatabaseAction.Modify([], false)),
			// EditionSyntax.
			"MODIFY of a name it does not read" => Alter(new AlterDatabaseAction.Modify([Set("FOO", Text("x"))], false)),
			// EditionSyntax: MB or GB.
			"MODIFY MAXSIZE in KB" => Alter(new AlterDatabaseAction.Modify([Set("MAXSIZE", Sized("5", "KB"))], false)),
			// EditionSyntax: Quoted.
			"MODIFY EDITION of a name" => Alter(new AlterDatabaseAction.Modify([Set("EDITION", Ref("Premium"))], false)),
			// DatabaseModify: = Quoted.
			"BACKUP_STORAGE_REDUNDANCY of a word" => Alter(new AlterDatabaseAction.ModifyBackupStorageRedundancy(Ref("GEO"))),
			// DatabaseAdd: FileSpecText, one at least.
			"ADD FILE of nothing" => Alter(new AlterDatabaseAction.AddFile([], null)),
			// FileSpecText: one option at least.
			"a file of no option" => Alter(new AlterDatabaseAction.ModifyFile(File())),
			// FileOptionSyntax, Msg 153.
			"a file option it does not read" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("FOO", Number("1"))))),
			// FileOptionSyntax: SIZE = Digits.
			"SIZE of a fraction" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("SIZE", new LiteralValue.Numeric("10.5", NumericLiteralKind.Decimal))))),
			// FileOptionSyntax: SizeUnit, SIZE = 10 % is refused.
			"SIZE in percent" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("SIZE", Sized("10", "%"))))),
			// SizeUnit: KB, MB, GB, TB.
			"SIZE in PB" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("SIZE", Sized("10", "PB"))))),
			// FileOptionSyntax: OFFLINE alone.
			"OFFLINE with a value" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("OFFLINE", Ref("ON"))))),
			// FileOptionSyntax: Quoted or Identifier.
			"NAME of a number" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("NAME", Number("1"))))),
			// FileOptionSyntax: name = value.
			"a file option with its = left out" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("SIZE", Number("1")) with { EqualsOmitted = true }))),
			// FileGroupHolds.
			"an ADD FILEGROUP with no text for what it holds" => Alter(new AlterDatabaseAction.AddFileGroup(Name("g"), (FileGroupContents)9)),
			// FileGroupAction.
			"a MODIFY FILEGROUP with no text for its updatability" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Updatability((FileGroupUpdatability)9, false))),
			// AlterDatabaseStatement: SET DatabaseSetting, one at least.
			"SET of nothing" => Alter(new AlterDatabaseAction.Set([])),
			// DatabaseSetting, Msg 102.
			"SET of a name it does not read" => SetOf(Set("FOO", Ref("ON"))),
			// DatabaseWords: RECOVERY FULL, BULK_LOGGED or SIMPLE.
			"SET RECOVERY ON" => SetOf(Set("RECOVERY", Ref("ON"))),
			// DatabaseWords: ONLINE alone.
			"SET ONLINE with a value" => SetOf(Set("ONLINE", Ref("ON"))),
			// DatabaseSwitch: AUTO_CLOSE has no =, the flag says nothing there.
			"SET AUTO_CLOSE with its = left out flagged" => SetOf(Set("AUTO_CLOSE", Ref("ON")) with { EqualsOmitted = true }),
			// DatabaseEqualsSwitch: NESTED_TRIGGERS ON is refused (Msg 102).
			"SET NESTED_TRIGGERS with its = left out" => SetOf(Set("NESTED_TRIGGERS", Ref("ON")) with { EqualsOmitted = true }),
			// DatabaseWords: the words as words.
			"SET a quoted name" => SetOf(new Option(new Identifier("[ONLINE]", IdentifierStyle.Bracketed), null, [])),
			// DatabaseSetting: PARTNER SAFETY, PARTNER TIMEOUT.
			"SET a further word it does not read" => SetOf(Worded("PARTNER", ["FOO"], Ref("FULL"))),
			// DatabaseSetting: FILESTREAM ( FilestreamOption, … ).
			"SET FILESTREAM with no list" => SetOf(Set("FILESTREAM")),
			// IncrementalNest: one setting.
			"SET AUTO_CREATE_STATISTICS ON with two settings" => SetOf(Nested("AUTO_CREATE_STATISTICS", Ref("ON"), Set("INCREMENTAL", Ref("ON")), Set("INCREMENTAL", Ref("OFF")))),
			// DatabaseSetting: the list only after ON.
			"SET AUTO_CREATE_STATISTICS OFF with a list" => SetOf(Nested("AUTO_CREATE_STATISTICS", Ref("OFF"), Set("INCREMENTAL", Ref("ON")))),
			// DatabaseWords: ONLINE alone.
			"SET ONLINE with a list" => SetOf(Nested("ONLINE", null, Set("FORCED"))),
			// StoreSyntax.
			"SET QUERY_STORE of a setting it does not read" => SetOf(Nested("QUERY_STORE", Ref("ON"), Set("FOO", Number("1")))),
			// TrackingSyntax: DAYS, HOURS or MINUTES.
			"SET CHANGE_RETENTION in SECONDS" => SetOf(Nested("CHANGE_TRACKING", Ref("ON"), Set("CHANGE_RETENTION", Sized("2", "SECONDS")))),
			// RecoveryUnit: SECONDS or MINUTES.
			"SET TARGET_RECOVERY_TIME in HOURS" => SetOf(Set("TARGET_RECOVERY_TIME", Sized("1", "HOURS"))),
			// DatabaseInteger: Digits.
			"SET COMPATIBILITY_LEVEL of a fraction" => SetOf(Set("COMPATIBILITY_LEVEL", new LiteralValue.Numeric("1.5", NumericLiteralKind.Decimal))),
			// LanguageValue: a string is refused.
			"SET DEFAULT_LANGUAGE of a string" => SetOf(Set("DEFAULT_LANGUAGE", Text("English"))),
			// DatabaseSetting: no partitions.
			"SET a setting with partitions" => SetOf(Set("ONLINE") with { Partitions = [] }),
			// DatabaseTermination: AFTER 5.5 is refused.
			"SET ROLLBACK AFTER a fraction" => Alter(new AlterDatabaseAction.Set([Set("SINGLE_USER")]) { Termination = new Termination.RollbackAfter(new LiteralValue.Numeric("5.5", NumericLiteralKind.Decimal)) }),
			// ScopedSetting, Msg 102.
			"SCOPED of a name it does not read" => Scoped(Set("FOO", Ref("ON"))),
			// ScopedSetting: OFF, WHEN_SUPPORTED or FAIL_UNSUPPORTED.
			"SCOPED ELEVATE_ONLINE ON" => Scoped(Set("ELEVATE_ONLINE", Ref("ON"))),
			// ScopedNumber: MAXDOP = +4 is refused.
			"SCOPED MAXDOP with a plus" => Scoped(Set("MAXDOP", new Expression.Unary(UnaryOperator.Plus, Number("4")))),
			// ScopedSetting: name = value.
			"SCOPED switch without =" => Scoped(Set("IDENTITY_CACHE", Ref("OFF")) with { EqualsOmitted = true }),
			// PlanHandle: TSql.BinaryLiteral.
			"CLEAR PROCEDURE_CACHE of an X literal" => new Statement.AlterDatabaseScopedConfiguration(new ScopedConfigurationAction.ClearProcedureCache(new LiteralValue.Binary("'0102'"))),
			// DatabaseFiles: ON PRIMARY FileSpec.
			"CREATE DATABASE PRIMARY with no files" => Create() with { Primary = true },
			// DatabaseFiles: a FileSpec before the filegroups.
			"CREATE DATABASE filegroups with no files" => Create() with { FileGroups = [new FileGroup(Name("g"), [F])] },
			// FileGroupSpec: FileSpec, one at least.
			"CREATE DATABASE a filegroup of no file" => Create() with { Files = [F], FileGroups = [new FileGroup(Name("g"), [])] },
			// CreateDatabaseStatement: NONE or PARTIAL.
			"CREATE DATABASE a containment of no word" => Create() with { Containment = (DatabaseContainment)9 },
			// CreateDatabaseNames: AUTO_CLOSE is refused.
			"CREATE DATABASE an option SET takes and a creation does not" => Create() with { Options = [Set("AUTO_CLOSE", Ref("ON"))] },
			// DatabaseSwitch: TRUSTWORTHY takes no =, the flag says nothing there.
			"CREATE DATABASE TRUSTWORTHY with =" => Create() with { Options = [Set("TRUSTWORTHY", Ref("ON")) with { EqualsOmitted = true }] },
			// CreateDatabaseOption: ENABLE_BROKER only when attached.
			"CREATE DATABASE a broker word on a new database" => Create() with { Options = [Set("ENABLE_BROKER")] },
			// Syntax.Attached, Msg 102.
			"CREATE DATABASE FOR ATTACH two broker words" => Create() with { Files = [F], Source = new DatabaseSource.ForAttach(AttachKind.Attach), Options = [Set("ENABLE_BROKER"), Set("NEW_BROKER")] },
			// Syntax.NamedOnce, Msg 102.
			"CREATE DATABASE FOR ATTACH an option twice" => Create() with { Files = [F], Source = new DatabaseSource.ForAttach(AttachKind.Attach), Options = [Set("TRUSTWORTHY", Ref("ON")), Set("trustworthy", Ref("OFF"))] },
			// RebuildLogOption: ENABLE_BROKER is 102.
			"CREATE DATABASE FOR ATTACH_REBUILD_LOG a broker word" => Create() with { Files = [F], Source = new DatabaseSource.ForAttach(AttachKind.AttachRebuildLog), Options = [Set("ENABLE_BROKER")] },
			// AttachOption: FILESTREAM (DIRECTORY_NAME = …) only.
			"CREATE DATABASE FOR ATTACH a FILESTREAM of NON_TRANSACTED_ACCESS" => Create() with { Files = [F], Source = new DatabaseSource.ForAttach(AttachKind.Attach), Options = [Nested("FILESTREAM", null, Set("NON_TRANSACTED_ACCESS", Ref("FULL")))] },
			// DatabaseTail.
			"CREATE DATABASE an attach of no word" => Create() with { Files = [F], Source = new DatabaseSource.ForAttach((AttachKind)9) },
			// DatabaseTail: Identifier [. Identifier].
			"CREATE DATABASE AS COPY OF three parts" => Create() with { Source = new DatabaseSource.AsCopyOf(new QualifiedName([Name("a"), Name("b"), Name("c")])) },
			// DatabaseTail: WITH ( BackupRedundancy ).
			"CREATE DATABASE AS COPY OF two options" => Create() with { Source = new DatabaseSource.AsCopyOf(new QualifiedName([D])), Options = [Set("BACKUP_STORAGE_REDUNDANCY", Text("GEO")), Set("BACKUP_STORAGE_REDUNDANCY", Text("LOCAL"))] },
			// BackupRedundancy.
			"CREATE DATABASE AS COPY OF a LEDGER" => Create() with { Source = new DatabaseSource.AsCopyOf(new QualifiedName([D])), Options = [Set("LEDGER", Ref("ON"))] },
			// CreateDatabaseStatement: the edition options follow the name and a collation.
			"CREATE DATABASE Azure with files" => Create() with { Files = [F], EditionOptions = [Set("EDITION", Text("Basic"))] },
			// DatabaseTail: AS SNAPSHOT OF takes no edition options.
			"CREATE DATABASE edition options on a snapshot" => Create() with { Files = [F], Source = new DatabaseSource.AsSnapshotOf(D), EditionOptions = [Set("EDITION", Text("Basic"))] },
			// FileGroupHolds.
			"CREATE DATABASE a filegroup holding no word" => Create() with { Files = [F], FileGroups = [new FileGroup(Name("g"), [F]) { Contains = (FileGroupContents)9 }] },
			// BackupStatement: TO BackupDevices.
			"BACKUP DATABASE to nothing" => new Statement.BackupDatabase(Db(), []),
			// MirrorTo: BackupDevices.
			"BACKUP DATABASE a mirror of nothing" => new Statement.BackupDatabase(Db(), [Device("a")]) { Mirrors = [new BackupMirror([])] },
			// BackupDevice: QualifiedName or TSql.Variable.
			"BACKUP DATABASE a device both named and a variable" => new Statement.BackupDatabase(Db(), [new BackupDevice.Logical(new QualifiedName([Name("a")]), (Expression.Variable)Var("@a"))]),
			// BackupDevice.
			"BACKUP DATABASE a device neither named nor a variable" => new Statement.BackupDatabase(Db(), [new BackupDevice.Logical(null, null)]),
			// MirrorTo: MIRROR reads as the next clause.
			"BACKUP DATABASE a device named MIRROR" => new Statement.BackupDatabase(Db(), [Device("MIRROR")]),
			// DeviceWord.
			"BACKUP DATABASE a device of no kind" => new Statement.BackupDatabase(Db(), [new BackupDevice.Physical((DeviceKind)9, Text("x"))]),
			// BackupTarget: BackupBoundary.
			"BACKUP DATABASE a database named FILE" => new Statement.BackupDatabase(Db("FILE"), [Device("a")]),
			// BackupName: Sql92.Lexical.CharacterStringLiteral.
			"BACKUP DATABASE a database of a national string" => new Statement.BackupDatabase(new LiteralValue.String("'d'", StringLiteralKind.National), [Device("a")]),
			// BackupName.
			"BACKUP DATABASE a database of a number" => new Statement.BackupDatabase(Number("1"), [Device("a")]),
			// BackupOption: REPLACE is a restore's.
			"BACKUP option it does not read" => new Statement.BackupDatabase(Db(), [Device("a")]) { Options = [Set("REPLACE")] },
			// BackupOption: ALGORITHM, then LEVEL.
			"BACKUP COMPRESSION LEVEL first" => new Statement.BackupDatabase(Db(), [Device("a")]) { Options = [Nested("COMPRESSION", null, Set("LEVEL", Ref("LOW")), Set("ALGORITHM", Ref("ZSTD")))] },
			// BackupOption: ENCRYPTION (ALGORITHM = …, SERVER …).
			"BACKUP ENCRYPTION without its encryptor" => new Statement.BackupDatabase(Db(), [Device("a")]) { Options = [Nested("ENCRYPTION", null, Set("ALGORITHM", Ref("AES_256")))] },
			// BackupOption: ENCRYPTION alone is refused.
			"BACKUP ENCRYPTION alone" => new Statement.BackupDatabase(Db(), [Device("a")]) { Options = [Set("ENCRYPTION")] },
			// BackupValue: Quoted, TSql.Variable or a number.
			"BACKUP DESCRIPTION of a word" => new Statement.BackupDatabase(Db(), [Device("a")]) { Options = [Set("DESCRIPTION", Ref("x"))] },
			// BackupString.
			"BACKUP NAME of a number" => new Statement.BackupDatabase(Db(), [Device("a")]) { Options = [Set("NAME", Number("1"))] },
			// BackupGroup: BackupName, one at least.
			"BACKUP GROUP of nothing" => new Statement.BackupGroup([], [Device("a")]),
			// CertificatePrivateKey: FORMAT before PRIVATE KEY.
			"BACKUP CERTIFICATE a format without a private key" => new Statement.BackupCertificate(Name("c"), Text("f")) { Format = Text("PFX") },
			// CertificateKeyItems: Syntax.Once, Msg 102.
			"BACKUP CERTIFICATE ALGORITHM twice" => new Statement.BackupCertificate(Name("c"), Text("f")) { PrivateKey = [Set("ALGORITHM", Text("a")), Set("ALGORITHM", Text("b"))] },
			// CertificateKeyItem.
			"BACKUP CERTIFICATE a private key setting it does not read" => new Statement.BackupCertificate(Name("c"), Text("f")) { PrivateKey = [Set("FORMAT", Text("a"))] },
			// BackupStatement: TO FILE = Quoted, never a variable.
			"BACKUP CERTIFICATE to a variable" => new Statement.BackupCertificate(Name("c"), Var("@f")),
			// KeyLockedBy: Quoted.
			"BACKUP MASTER KEY a password of a variable" => new Statement.BackupMasterKey(new KeyPlace(KeyPlaceKind.File, Text("f")), Var("@p")),
			// KeyPlace: FILE or URL.
			"BACKUP MASTER KEY a place of no kind" => new Statement.BackupMasterKey(new KeyPlace((KeyPlaceKind)9, Text("f")), Text("p")),
			// RestoreFrom: DATABASE_SNAPSHOT or BackupDevices.
			"RESTORE DATABASE from devices and a snapshot" => new Statement.RestoreDatabase(Db()) { From = [Device("a")], DatabaseSnapshot = Text("s") },
			// RestoreFrom: BackupString, = s is refused.
			"RESTORE DATABASE from a snapshot by name" => new Statement.RestoreDatabase(Db()) { DatabaseSnapshot = Db("s") },
			// RestoreOption: DIFFERENTIAL is refused.
			"RESTORE an option it does not read" => new Statement.RestoreDatabase(Db()) { Options = [Set("DIFFERENTIAL")] },
			// RestoreOption: MOVE BackupString TO BackupString.
			"RESTORE MOVE as an option" => new Statement.RestoreDatabase(Db()) { Options = [Set("MOVE", Text("a"))] },
			// RestoreOption: FILE = UnsignedNumericLiteral or a variable.
			"RESTORE FILE of a string" => new Statement.RestoreDatabase(Db()) { Options = [Set("FILE", Text("1"))] },
			// RESTORE FILELISTONLY: FROM backup_device.
			"RESTORE FILELISTONLY from nothing" => new Statement.RestoreFileListOnly([]),
			// RestoreStatement: FROM FILE = Quoted.
			"RESTORE SERVICE MASTER KEY from a variable" => new Statement.RestoreServiceMasterKey(Var("@f"), Text("p")),
			// PartitionBoundary: NULL or ValueExpression, DEFAULT is refused.
			"CREATE PARTITION FUNCTION a boundary of DEFAULT" => new Statement.CreatePartitionFunction(Name("f"), new DataType.Numeric(NumericTypeKind.Int), [new Expression.Default()]),
			// PartitionStatement: LEFT or RIGHT.
			"CREATE PARTITION FUNCTION a side of no word" => new Statement.CreatePartitionFunction(Name("f"), new DataType.Numeric(NumericTypeKind.Int), []) { Range = (RangeSide)9 },
			// PartitionBoundary: ValueExpression.
			"ALTER PARTITION FUNCTION SPLIT at a comparison" => new Statement.AlterPartitionFunction(Name("f"), new PartitionFunctionAction.Split(new Expression.Comparison(Number("1"), ComparisonOperator.Equal, Number("1")))),
			// PartitionStatement: PartitionFileGroup, one at least.
			"CREATE PARTITION SCHEME of no filegroup" => new Statement.CreatePartitionScheme(Name("s"), Name("f"), []),
			// PartitionFileGroup: Identifier or Quoted.
			"CREATE PARTITION SCHEME a filegroup of two parts" => new Statement.CreatePartitionScheme(Name("s"), Name("f"), [new Expression.Reference(new QualifiedName([Name("a"), Name("b")]))]),
			// PartitionFileGroup.
			"ALTER PARTITION SCHEME a filegroup of a number" => new Statement.AlterPartitionScheme(Name("s"), Number("1")),
			// FullTextStatement: IN PATH ValueExpression.
			"CREATE FULLTEXT CATALOG a path of DEFAULT" => new Statement.CreateFullTextCatalog(Name("c")) { Path = new Expression.Default() },
			// StopListFrom: Msg 102.
			"CREATE FULLTEXT STOPLIST from three parts" => new Statement.CreateFullTextStopList(Name("s")) { From = new StopListSource.Named(new QualifiedName([Name("a"), Name("b"), Name("c")])) },
			// StopListAction: Quoted, a word unquoted is Msg 102.
			"ALTER FULLTEXT STOPLIST a word unquoted" => new Statement.AlterFullTextStopList(Name("s"), new StopListAction.Add(Ref("sw"), Number("1033"))),
			// StopListLanguage: a variable is Msg 102.
			"ALTER FULLTEXT STOPLIST a language of a variable" => new Statement.AlterFullTextStopList(Name("s"), new StopListAction.Drop(Text("sw"), Var("@l"))),
			// PropertyListAction: the engine refuses any other order.
			"ALTER SEARCH PROPERTY LIST ADD out of order" => new Statement.AlterSearchPropertyList(Name("l"), new SearchPropertyListAction.Add(Text("a"), [Set("PROPERTY_INT_ID", Number("4")), Set("PROPERTY_SET_GUID", Text("g"))])),
			// PropertyListAction: either without the other.
			"ALTER SEARCH PROPERTY LIST ADD without its id" => new Statement.AlterSearchPropertyList(Name("l"), new SearchPropertyListAction.Add(Text("a"), [Set("PROPERTY_SET_GUID", Text("g"))])),
			// SimpleCommand: CHECKPOINT ValueExpression.
			"CHECKPOINT of DEFAULT" => new Statement.Checkpoint(new Expression.Default()),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}
}
