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
	[InlineData("CREATE PROCEDURE p AS RETURN", "PROCEDURE")]
	[InlineData("CREATE PROC p AS RETURN", "PROC")]
	[InlineData("ALTER PROC p AS RETURN", "ALTER")]
	[InlineData("CREATE OR ALTER PROCEDURE p AS RETURN", "CREATE OR ALTER")]
	[InlineData("CREATE PROCEDURE p @a AS INT AS RETURN", "a parameter's AS")]
	[InlineData("CREATE PROCEDURE p @a INT OUT AS RETURN", "a parameter OUT")]
	[InlineData("CREATE PROCEDURE p @a INT OUTPUT AS RETURN", "a parameter OUTPUT")]
	[InlineData("CREATE PROCEDURE p @c CURSOR VARYING OUTPUT AS RETURN", "a cursor")]
	[InlineData("CREATE PROCEDURE p AS BEGIN RETURN END", "a block")]
	[InlineData("CREATE PROCEDURE p AS EXTERNAL NAME a.c.m", "EXTERNAL NAME")]
	[InlineData("CREATE FUNCTION f () RETURNS INT WITH RETURNS NULL ON NULL INPUT AS BEGIN RETURN 1 END", "ON NULL INPUT in WITH")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE AS RETURN SELECT 1", "inline")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE AS RETURN (SELECT 1)", "inline in brackets")]
	[InlineData("CREATE FUNCTION f () RETURNS @t TABLE (a INT) AS BEGIN RETURN END", "a table variable")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE (a INT) AS EXTERNAL NAME a.c.m", "a CLR table")]
	[InlineData("CREATE FUNCTION f () RETURNS INT WITH EXECUTE AS CALLER AS BEGIN RETURN 1 END", "EXECUTE AS CALLER")]
	[InlineData("CREATE FUNCTION f () RETURNS INT WITH INLINE = ON AS BEGIN RETURN 1 END", "INLINE = ON")]
	[InlineData("CREATE FUNCTION f () RETURNS INT WITH INLINE ON AS BEGIN RETURN 1 END", "INLINE ON")]
	[InlineData("CREATE TRIGGER g ON t AFTER INSERT AS RETURN", "AFTER")]
	[InlineData("CREATE TRIGGER g ON t FOR INSERT AS RETURN", "FOR")]
	[InlineData("CREATE TRIGGER g ON t INSTEAD OF DELETE AS RETURN", "INSTEAD OF")]
	[InlineData("ALTER TRIGGER g ON t AFTER INSERT AS RETURN", "ALTER TRIGGER")]
	[InlineData("CREATE TRIGGER g ON DATABASE AFTER CREATE_TABLE AS RETURN", "ON DATABASE")]
	[InlineData("CREATE TRIGGER g ON ALL SERVER FOR LOGON AS RETURN", "ON ALL SERVER")]
	[InlineData("CREATE TRIGGER g ON [DATABASE] AFTER INSERT AS RETURN", "a table called DATABASE")]
	[InlineData("ENABLE TRIGGER ALL ON t", "ENABLE ALL")]
	[InlineData("DISABLE TRIGGER g ON DATABASE", "DISABLE ON DATABASE")]
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
			"PROCEDURE" => made is Ast.Statement.CreateRoutine { Verb: null, Definition: { Kind: Ast.RoutineKind.Procedure, Proc: false, Body: Ast.RoutineBody.Statements { List.Count: 1 } } },
			"PROC" => made is Ast.Statement.CreateRoutine { Definition.Proc: true },
			"ALTER" => made is Ast.Statement.CreateRoutine { Verb: Ast.DefinitionVerb.Alter, Definition.Proc: true },
			"CREATE OR ALTER" => made is Ast.Statement.CreateRoutine { Verb: Ast.DefinitionVerb.CreateOrAlter },
			"a parameter's AS" => made is Ast.Statement.CreateRoutine { Definition.Parameters: [{ AsKeyword: true, Mode: null }] },
			"a parameter OUT" => made is Ast.Statement.CreateRoutine { Definition.Parameters: [{ AsKeyword: false, Mode: Ast.ParameterMode.Out, ShortOutput: true }] },
			"a parameter OUTPUT" => made is Ast.Statement.CreateRoutine { Definition.Parameters: [{ Mode: Ast.ParameterMode.Out, ShortOutput: false }] },
			"a cursor" => made is Ast.Statement.CreateRoutine { Definition.Parameters: [{ Varying: true, Type: Ast.DataType.Named { Name.First.Text: "CURSOR" } }] },
			"a block" => made is Ast.Statement.CreateRoutine { Definition.Body: Ast.RoutineBody.Statements { List: [Ast.Statement.Compound] } },
			"EXTERNAL NAME" => made is Ast.Statement.CreateRoutine { Definition.Body: Ast.RoutineBody.ExternalName },
			"ON NULL INPUT in WITH" => made is Ast.Statement.CreateRoutine { Definition: { Characteristics.Count: 0, Options: [Ast.RoutineOption.NullCall { Mode: Ast.NullCallMode.ReturnsNullOnNullInput }] } },
			"inline" => made is Ast.Statement.CreateRoutine { Definition: { Returns: { TableKeyword: true, Elements: null, Variable: null }, Body: Ast.RoutineBody.Inline } },
			"inline in brackets" => made is Ast.Statement.CreateRoutine { Definition.Body: Ast.RoutineBody.Inline { Query.Query: Ast.Query.Parenthesized } },
			"a table variable" => made is Ast.Statement.CreateRoutine { Definition.Returns: { TableKeyword: false, Variable.Text: "@t", Elements.Count: 1 } },
			"a CLR table" => made is Ast.Statement.CreateRoutine { Definition: { Returns: { TableKeyword: true, Variable: null, Elements.Count: 1 }, Body: Ast.RoutineBody.ExternalName } },
			"EXECUTE AS CALLER" => made is Ast.Statement.CreateRoutine { Definition.Options: [Ast.RoutineOption.ExecuteAs { Context: { Kind: Ast.ExecutionContextKind.Caller, Name: null } }] },
			"INLINE = ON" => made is Ast.Statement.CreateRoutine { Definition.Options: [Ast.RoutineOption.Setting { Option: { EqualsOmitted: false, Value: not null } }] },
			"INLINE ON" => made is Ast.Statement.CreateRoutine { Definition.Options: [Ast.RoutineOption.Setting { Option.EqualsOmitted: true }] },
			"AFTER" => made is Ast.Statement.CreateTrigger { Time: Ast.TriggerTime.After, For: false, Event: null, Action: null, Scope: null, Table: not null, Events.Count: 1 },
			"FOR" => made is Ast.Statement.CreateTrigger { Time: Ast.TriggerTime.After, For: true },
			"INSTEAD OF" => made is Ast.Statement.CreateTrigger { Time: Ast.TriggerTime.InsteadOf, Events: [{ Kind: Ast.TriggerEventKind.Delete }] },
			"ALTER TRIGGER" => made is Ast.Statement.CreateTrigger { Verb: Ast.DefinitionVerb.Alter },
			"ON DATABASE" => made is Ast.Statement.CreateTrigger { Scope: Ast.TriggerScope.Database, Table: null, Events.Count: 0, EventNames: [{ Text: "CREATE_TABLE" }] },
			"ON ALL SERVER" => made is Ast.Statement.CreateTrigger { Scope: Ast.TriggerScope.AllServer, For: true },
			"a table called DATABASE" => made is Ast.Statement.CreateTrigger { Scope: null, Table: not null },
			"ENABLE ALL" => made is Ast.Statement.EnableTrigger { All: true, Names.Count: 0, Scope: null, On: not null },
			"DISABLE ON DATABASE" => made is Ast.Statement.DisableTrigger { All: false, Scope: Ast.TriggerScope.Database, On: null },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	/// <summary>
	/// The standard's null call is a characteristic its parser reads, never T-SQL's option in WITH, and the writer
	/// writes each in its own dialect's form only.
	/// </summary>
	[Fact]
	public void The_standard_reads_its_null_call_as_a_characteristic()
	{
		var read = DotGram.Sql.Standard.SqlStandardParser.TryParseStatement("CREATE FUNCTION f () RETURNS INT RETURNS NULL ON NULL INPUT RETURN 1");

		Assert.True(read.IsSuccess);
		Assert.True(read.Value is Ast.Statement.CreateRoutine
		{
			Definition: { Options.Count: 0, Characteristics: [Ast.RoutineCharacteristic.NullCall { Value: Ast.NullCallMode.ReturnsNullOnNullInput }] },
		}, $"{read.Value}");
	}
}
