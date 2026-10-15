using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Database family's bridge (Bridge.Database.cs): which record and members it builds for a text, where another
/// record or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class DatabaseBridgeTests
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
	public void The_bridge_builds_the_database_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

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
			"SET WITH ROLLBACK IMMEDIATE" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Termination: Ast.Termination.RollbackImmediate } },
			"SET without termination" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Termination: null } },
			"ROLLBACK AFTER SECONDS" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Termination: Ast.Termination.RollbackAfter { Seconds: true, Number.Text: "5" } } },
			"ROLLBACK AFTER" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Termination: Ast.Termination.RollbackAfter { Seconds: false } } },
			"NO_WAIT" => made is Ast.Statement.AlterDatabase { Action: Ast.AlterDatabaseAction.Set { Termination: Ast.Termination.NoWait } },
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
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
