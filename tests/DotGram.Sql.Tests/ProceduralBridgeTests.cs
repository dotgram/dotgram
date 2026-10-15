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
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
