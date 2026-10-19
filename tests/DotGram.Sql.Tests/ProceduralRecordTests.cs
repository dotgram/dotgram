using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Procedural family's rules (Build.Procedural.cs): which record and members they build for a text, where another
/// record or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class ProceduralRecordTests
{
	[Theory]
	[InlineData("IF @a < 1 IF @a < 2 SELECT 1 ELSE SELECT 2", "Else of the inner IF")]
	[InlineData("IF @a < 1 BEGIN SELECT 1 END ELSE SELECT 2", "IF with a block")]
	[InlineData("WHILE @a < 1 BREAK", "WHILE and BREAK")]
	[InlineData("WHILE @a < 1 CONTINUE", "WHILE and CONTINUE")]
	[InlineData("BEGIN TRY SELECT 1 END TRY BEGIN CATCH END CATCH", "TRY")]
	[InlineData("GOTO done", "GOTO")]
	[InlineData("done:", "Label")]
	[InlineData("DECLARE @a AS INT", "AS")]
	[InlineData("DECLARE @a INT", "no AS")]
	[InlineData("DECLARE @t AS TABLE (c INT)", "TABLE with AS")]
	[InlineData("DECLARE @t TABLE (c INT)", "TABLE")]
	[InlineData("DECLARE @c cursor", "CURSOR")]
	[InlineData("DECLARE @c [cursor]", "a type named cursor")]
	[InlineData("SET @a = 1", "=")]
	[InlineData("SET @a += 1", "+=")]
	[InlineData("SET @a.m = 1", "a property")]
	[InlineData("SET @a.m(1)", "a method")]
	[InlineData("SET NOCOUNT ON", "a switch")]
	[InlineData("SET STATISTICS TIME ON", "a STATISTICS word")]
	[InlineData("SET OFFSETS SELECT ON", "an OFFSETS word")]
	[InlineData("SET IDENTITY_INSERT t OFF", "IDENTITY_INSERT")]
	[InlineData("SET FIPS_FLAGGER OFF", "FIPS_FLAGGER OFF")]
	[InlineData("SET FIPS_FLAGGER 'OFF'", "FIPS_FLAGGER a string")]
	[InlineData("SET datefirst -1", "a value")]
	[InlineData("SET TRANSACTION ISOLATION LEVEL SNAPSHOT", "the standard's SET TRANSACTION")]
	[InlineData("BEGIN TRAN", "BEGIN TRAN")]
	[InlineData("BEGIN TRANSACTION t", "BEGIN TRANSACTION")]
	[InlineData("BEGIN DISTRIBUTED TRAN", "BEGIN DISTRIBUTED")]
	[InlineData("BEGIN TRAN t WITH MARK N'm'", "a mark")]
	[InlineData("SAVE TRAN @s", "SAVE a variable")]
	[InlineData("COMMIT TRAN @t", "COMMIT a variable")]
	[InlineData("DEALLOCATE c", "DEALLOCATE")]
	[InlineData("DEALLOCATE GLOBAL c", "DEALLOCATE GLOBAL")]
	[InlineData("DECLARE c CURSOR FOR SELECT 1", "no word")]
	[InlineData("DECLARE c SCROLL CURSOR FOR SELECT 1", "SCROLL before")]
	[InlineData("DECLARE c CURSOR SCROLL FOR SELECT 1", "SCROLL after")]
	[InlineData("DECLARE c CURSOR FORWARD_ONLY FOR SELECT 1", "FORWARD_ONLY")]
	[InlineData("DECLARE c CURSOR FOR SELECT 1 FOR UPDATE OPTION (RECOMPILE)", "FOR before the hints")]
	[InlineData("DECLARE c CURSOR FOR SELECT 1 OPTION (RECOMPILE) FOR UPDATE", "FOR after the hints")]
	[InlineData("SET @c = CURSOR FOR SELECT 1", "SET a cursor")]
	[InlineData("SET @c = @d", "SET a cursor variable")]
	[InlineData("THROW", "THROW alone")]
	[InlineData("THROW 1, 'a', 1", "THROW")]
	[InlineData("RAISERROR ('a', 16, 1) WITH NOWAIT, LOG", "RAISERROR")]
	[InlineData("WAITFOR DELAY '00:00:01'", "DELAY")]
	[InlineData("WAITFOR TIME '10:00'", "TIME")]
	[InlineData("EXECUTE AS CALLER", "CALLER")]
	[InlineData("EXECUTE AS LOGIN = 'l' WITH NO REVERT", "LOGIN")]
	[InlineData("EXECUTE AS USER = 'u' WITH COOKIE INTO @c", "USER")]
	[InlineData("REVERT", "REVERT")]
	[InlineData("SETUSER 'u'", "SETUSER")]
	[InlineData("USE master", "USE")]
	[InlineData("LINENO 5", "LINENO")]
	[InlineData("PRINT 1", "PRINT")]
	public void The_rules_build_the_procedural_record_the_text_means(string text, string expected)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		var holds = expected switch
		{
			"Else of the inner IF" => made is Ast.Statement.If { Else: null, Then: Ast.Statement.If { Else: Ast.Statement.Select } },
			"IF with a block" => made is Ast.Statement.If { Then: Ast.Statement.Compound { Atomic: null, Statements.Count: 1 }, Else: Ast.Statement.Select },
			"WHILE and BREAK" => made is Ast.Statement.While { Body: Ast.Statement.Break },
			"WHILE and CONTINUE" => made is Ast.Statement.While { Body: Ast.Statement.Continue },
			"TRY" => made is Ast.Statement.TryCatch { Try.Count: 1, Catch.Count: 0 },
			"GOTO" => made is Ast.Statement.GoTo { Name.Text: "done" },
			"Label" => made is Ast.Statement.Label { Name.Text: "done" },
			"AS" => made is Ast.Statement.DeclareVariables { Variables: [Ast.VariableDeclaration.Typed { AsKeyword: true, Value: null, Type: Ast.DataType.Numeric }] },
			"no AS" => made is Ast.Statement.DeclareVariables { Variables: [Ast.VariableDeclaration.Typed { AsKeyword: false }] },
			"TABLE with AS" => made is Ast.Statement.DeclareVariables { Variables: [Ast.VariableDeclaration.Table { AsKeyword: true, Elements.Count: 1 }] },
			"TABLE" => made is Ast.Statement.DeclareVariables { Variables: [Ast.VariableDeclaration.Table { AsKeyword: false }] },
			"CURSOR" => made is Ast.Statement.DeclareVariables { Variables: [Ast.VariableDeclaration.Typed { Type: Ast.DataType.Named { Name.First: { Text: "CURSOR", Style: Ast.IdentifierStyle.Regular }, Arguments.Count: 0 }, Value: null }] },
			"a type named cursor" => made is Ast.Statement.DeclareVariables { Variables: [Ast.VariableDeclaration.Typed { Type: Ast.DataType.Named { Name.First: { Text: "[cursor]", Style: Ast.IdentifierStyle.Bracketed } } }] },
			"=" => made is Ast.Statement.SetVariable { Operator: Ast.AssignmentOperator.Assign, Members.Count: 0 },
			"+=" => made is Ast.Statement.SetVariable { Operator: Ast.AssignmentOperator.Add },
			"a property" => made is Ast.Statement.SetVariable { Operator: Ast.AssignmentOperator.Assign, Members: [{ Text: "m" }], Value: Ast.LiteralValue.Numeric },
			"a method" => made is Ast.Statement.SetVariable { Operator: null, Members.Count: 0, Value: Ast.Expression.Member { Kind: Ast.MemberAccessKind.Dot, Name.Text: "m", Arguments.Count: 1, Target: Ast.Expression.Variable { Name.Text: "@a" } } },
			"a switch" => made is Ast.Statement.SetOptions { Options: [Ast.SetOption.Switch { Option.Text: "NOCOUNT", On: true }] },
			"a STATISTICS word" => made is Ast.Statement.SetOptions { Options: [Ast.SetOption.Statistics { Kind: Ast.StatisticsKind.Time, On: true }] },
			"an OFFSETS word" => made is Ast.Statement.SetOptions { Options: [Ast.SetOption.Offsets { Keyword.Text: "SELECT", On: true }] },
			"IDENTITY_INSERT" => made is Ast.Statement.SetOptions { Options: [Ast.SetOption.IdentityInsert { On: false }] },
			"FIPS_FLAGGER OFF" => made is Ast.Statement.SetOptions { Options: [Ast.SetOption.Value { Option.Text: "FIPS_FLAGGER", Argument: Ast.Expression.Reference }] },
			"FIPS_FLAGGER a string" => made is Ast.Statement.SetOptions { Options: [Ast.SetOption.Value { Option.Text: "FIPS_FLAGGER", Argument: Ast.LiteralValue.String }] },
			"a value" => made is Ast.Statement.SetOptions { Options: [Ast.SetOption.Value { Option.Text: "DATEFIRST", Argument: Ast.LiteralValue.Numeric { Text: "-1" } }] },
			"the standard's SET TRANSACTION" => made is Ast.Statement.SetTransaction,
			"BEGIN TRAN" => made is Ast.Statement.BeginTransaction { Tran: true, Distributed: false, Name: null, Mark: null },
			"BEGIN TRANSACTION" => made is Ast.Statement.BeginTransaction { Tran: false, Name.Name.Text: "t" },
			"BEGIN DISTRIBUTED" => made is Ast.Statement.BeginTransaction { Distributed: true },
			"a mark" => made is Ast.Statement.BeginTransaction { Mark.Description: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } },
			"SAVE a variable" => made is Ast.Statement.SaveTransaction { Tran: true, Name: { Name: null, Variable: Ast.Expression.Variable } },
			"COMMIT a variable" => made is Ast.Statement.Commit { Name.Variable: Ast.Expression.Variable },
			"DEALLOCATE" => made is Ast.Statement.DeallocateCursor { Cursor: { Global: false, ExtendedName: null } },
			"DEALLOCATE GLOBAL" => made is Ast.Statement.DeallocateCursor { Cursor.Global: true },
			"no word" => made is Ast.Statement.DeclareCursor { Extended: false, SourceValue: Ast.CursorSource.Query { Updatability: null } },
			"SCROLL before" => made is Ast.Statement.DeclareCursor { Extended: false, Properties.Scrollability: Ast.CursorScrollability.Scroll },
			"SCROLL after" => made is Ast.Statement.DeclareCursor { Extended: true, Properties.Scrollability: Ast.CursorScrollability.Scroll },
			"FORWARD_ONLY" => made is Ast.Statement.DeclareCursor { Extended: true, Properties: { Scrollability: Ast.CursorScrollability.NoScroll, ForwardOnly: true } },
			"FOR before the hints" => made is Ast.Statement.DeclareCursor { SourceValue: Ast.CursorSource.Query { UpdatabilityBeforeOptions: true, Updatability.ReadOnly: false } },
			"FOR after the hints" => made is Ast.Statement.DeclareCursor { SourceValue: Ast.CursorSource.Query { UpdatabilityBeforeOptions: false, Updatability.ReadOnly: false } },
			"SET a cursor" => made is Ast.Statement.SetCursorVariable { Variable: Ast.Expression.Variable },
			"SET a cursor variable" => made is Ast.Statement.SetVariable { Value: Ast.Expression.Variable },
			"THROW alone" => made is Ast.Statement.Throw { Number: null, Message: null, State: null },
			"THROW" => made is Ast.Statement.Throw { Number: not null, Message: Ast.LiteralValue.String, State: not null },
			"RAISERROR" => made is Ast.Statement.RaiseError { Arguments.Count: 3, Options: [Ast.RaiseErrorOption.NoWait, Ast.RaiseErrorOption.Log] },
			"DELAY" => made is Ast.Statement.WaitFor { Kind: Ast.WaitForKind.Delay },
			"TIME" => made is Ast.Statement.WaitFor { Kind: Ast.WaitForKind.Time, Value: Ast.LiteralValue.String },
			"CALLER" => made is Ast.Statement.ExecuteAs { Context: { Kind: Ast.ExecutionContextKind.Caller, Name: null }, NoRevert: false, Cookie: null },
			"LOGIN" => made is Ast.Statement.ExecuteAs { Context: { Kind: Ast.ExecutionContextKind.Login, Name: Ast.LiteralValue.String }, NoRevert: true },
			"USER" => made is Ast.Statement.ExecuteAs { Context.Kind: Ast.ExecutionContextKind.User, Cookie.Name.Text: "@c" },
			"REVERT" => made is Ast.Statement.Revert { Cookie: null },
			"SETUSER" => made is Ast.Statement.SetUser { NoReset: false, Name: Ast.LiteralValue.String },
			"USE" => made is Ast.Statement.Use { Database.Text: "master" },
			"LINENO" => made is Ast.Statement.LineNumber { Line.Text: "5" },
			"PRINT" => made is Ast.Statement.Print { Value: Ast.LiteralValue.Numeric },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		ReadBack.Holds(text);
	}
}
