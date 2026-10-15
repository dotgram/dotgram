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
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
