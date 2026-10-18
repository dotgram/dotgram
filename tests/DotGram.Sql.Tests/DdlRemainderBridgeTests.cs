using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The remainder of the DDL statements the bridge carries: which record and members it builds for a text, where another
/// record or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class DdlRemainderBridgeTests
{
	[Theory]
	[InlineData("CREATE INDEX i ON t (a) WITH PAD_INDEX, FILLFACTOR = 80", "unbracketed")]
	[InlineData("CREATE INDEX i ON t (a) WITH (PAD_INDEX = ON, FILLFACTOR = 80)", "bracketed")]
	[InlineData("CREATE INDEX i ON t (a) WITH SORTED_DATA", "a word alone")]
	[InlineData("CREATE INDEX i ON t (a) WITH PAD_INDEX ON [PRIMARY]", "a word and a placement")]
	public void The_bridge_builds_the_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"unbracketed" => made is Ast.Statement.CreateIndex { Index: { OptionsUnbracketed: true, Options: [{ Name.Text: "PAD_INDEX", Value: null }, { Name.Text: "FILLFACTOR", Value: Ast.LiteralValue.Numeric }] } },
			"bracketed" => made is Ast.Statement.CreateIndex { Index: { OptionsUnbracketed: false, Options.Count: 2 } },
			"a word alone" => made is Ast.Statement.CreateIndex { Index: { OptionsUnbracketed: true, Options: [{ Name.Text: "SORTED_DATA", Value: null }] } },
			"a word and a placement" => made is Ast.Statement.CreateIndex { Index: { OptionsUnbracketed: true, Options: [{ Name.Text: "PAD_INDEX", Value: null }], Placements: [{ Kind: Ast.PlacementKind.On }] } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
