using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Routines family's bridge (Bridge.Routines.cs): which record and members it builds for a text, where another
/// record or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class RoutinesBridgeTests
{
	[Theory]
	[InlineData("EXEC p", "EXEC")]
	[InlineData("EXECUTE p 1", "EXECUTE")]
	[InlineData("p 1", "bare")]
	[InlineData("EXEC @m", "a variable")]
	[InlineData("EXEC @rc = p", "a return status")]
	[InlineData("EXEC p @a OUT", "OUT")]
	[InlineData("EXEC p @a OUTPUT", "OUTPUT")]
	[InlineData("EXEC p @a", "no OUTPUT")]
	[InlineData("EXEC p @x = DEFAULT", "a name and DEFAULT")]
	[InlineData("EXEC p;1", "a number")]
	[InlineData("EXEC ('SELECT 1')", "a string by EXEC")]
	[InlineData("EXECUTE ('SELECT 1')", "a string by EXECUTE")]
	[InlineData("EXEC ('SELECT ?', 1) AT srv", "a string AT")]
	[InlineData("EXEC ('SELECT 1') AT DATA_SOURCE ds", "a string AT DATA_SOURCE")]
	[InlineData("EXEC p WITH RESULT SETS NONE", "NONE")]
	[InlineData("EXEC p WITH RESULT SETS UNDEFINED", "UNDEFINED")]
	[InlineData("EXEC p WITH RESULT SETS ((a INT))", "described")]
	[InlineData("INSERT t EXEC p", "INSERT EXEC")]
	[InlineData("INSERT t EXEC ('SELECT 1')", "INSERT EXEC a string")]
	public void The_bridge_builds_the_routine_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"EXEC" => made is Ast.Statement.ExecuteModule { Keyword: Ast.ExecuteKeyword.Exec, Module: Ast.ModuleReference.Named },
			"EXECUTE" => made is Ast.Statement.ExecuteModule { Keyword: Ast.ExecuteKeyword.Execute, Arguments.Count: 1 },
			"bare" => made is Ast.Statement.ExecuteModule { Keyword: null },
			"a variable" => made is Ast.Statement.ExecuteModule { Module: Ast.ModuleReference.Variable { Name.Text: "@m" } },
			"a return status" => made is Ast.Statement.ExecuteModule { ReturnStatus.Text: "@rc", Module: Ast.ModuleReference.Named },
			"OUT" => made is Ast.Statement.ExecuteModule { Arguments: [{ Name: null, Output: Ast.OutputKeyword.Out }] },
			"OUTPUT" => made is Ast.Statement.ExecuteModule { Arguments: [{ Output: Ast.OutputKeyword.Output }] },
			"no OUTPUT" => made is Ast.Statement.ExecuteModule { Arguments: [{ Output: null, Value: Ast.Expression.Variable }] },
			"a name and DEFAULT" => made is Ast.Statement.ExecuteModule { Arguments: [{ Name.Text: "@x", Value: Ast.Expression.Default }] },
			"a number" => made is Ast.Statement.ExecuteModule { Number.Text: "1" },
			"a string by EXEC" => made is Ast.Statement.ExecuteString { Exec: true, Values.Count: 0 },
			"a string by EXECUTE" => made is Ast.Statement.ExecuteString { Exec: false },
			"a string AT" => made is Ast.Statement.ExecuteString { At.Text: "srv", DataSource: null, Values.Count: 1 },
			"a string AT DATA_SOURCE" => made is Ast.Statement.ExecuteString { At: null, DataSource.Text: "ds" },
			"NONE" => made is Ast.Statement.ExecuteModule { Options: [Ast.ExecuteOption.ResultSets { Kind: Ast.ResultSetsKind.None, Sets.Count: 0 }] },
			"UNDEFINED" => made is Ast.Statement.ExecuteModule { Options: [Ast.ExecuteOption.ResultSets { Kind: Ast.ResultSetsKind.Undefined }] },
			"described" => made is Ast.Statement.ExecuteModule { Options: [Ast.ExecuteOption.ResultSets { Kind: Ast.ResultSetsKind.Described, Sets: [Ast.ResultSet.Columns { Elements.Count: 1 }] }] },
			"INSERT EXEC" => made is Ast.Statement.Insert { SourceValue: Ast.InsertSource.ExecuteModule { Statement.Keyword: Ast.ExecuteKeyword.Exec } },
			"INSERT EXEC a string" => made is Ast.Statement.Insert { SourceValue: Ast.InsertSource.ExecuteString },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
