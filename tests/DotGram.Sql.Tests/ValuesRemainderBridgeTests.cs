using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The remainder of the statements and values the bridge carries outside the DDL: transactions, typed rowset sources,
/// T-SQL's functions of their own. Which record and members it builds for a text, where another record or a flag set
/// the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class ValuesRemainderBridgeTests
{
	[Theory]
	[InlineData("SAVE TRANSACTION -5:a.b", "a numbered name")]
	[InlineData("SAVE TRAN - 100 : [a ] . [b ]", "a numbered name with blanks")]
	[InlineData("SAVE TRANSACTION t1", "a name")]
	[InlineData("BEGIN TRAN 5:a.b WITH MARK 'x'", "a numbered name marked")]
	public void The_bridge_builds_the_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"a numbered name" => made is Ast.Statement.SaveTransaction { Name: { Name: null, Variable: null, Number.Text: "-5", Qualified.Count: 2 } },
			"a numbered name with blanks" => made is Ast.Statement.SaveTransaction { Name: { Number.Text: "-100", Qualified: [{ Text: "[a ]" }, { Text: "[b ]" }] } },
			"a name" => made is Ast.Statement.SaveTransaction { Name: { Name.Text: "t1", Number: null, Qualified: null } },
			"a numbered name marked" => made is Ast.Statement.BeginTransaction { Name.Number.Text: "5", Mark: not null },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
