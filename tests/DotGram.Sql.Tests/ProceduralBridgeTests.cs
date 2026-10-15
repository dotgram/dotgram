using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Procedural family's bridge (Bridge.Procedural.cs): which record and members it builds for a text, where another
/// record or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class ProceduralBridgeTests
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
	public void The_bridge_builds_the_procedural_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

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
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
