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
	[InlineData("SELECT a FROM t WHERE CONTAINS ((t.b, c), @s, LANGUAGE 1033)", "CONTAINS with a language")]
	[InlineData("SELECT a FROM t WHERE FREETEXT (b, N'x')", "FREETEXT of one column")]
	[InlineData("SELECT a FROM t WHERE FREETEXT ((b), N'x')", "FREETEXT of one column in brackets")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((*), 'x')", "CONTAINS of every column")]
	public void The_bridge_builds_the_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"a numbered name" => made is Ast.Statement.SaveTransaction { Name: { Name: null, Variable: null, Number.Text: "-5", Qualified.Count: 2 } },
			"a numbered name with blanks" => made is Ast.Statement.SaveTransaction { Name: { Number.Text: "-100", Qualified: [{ Text: "[a ]" }, { Text: "[b ]" }] } },
			"a name" => made is Ast.Statement.SaveTransaction { Name: { Name.Text: "t1", Number: null, Qualified: null } },
			"a numbered name marked" => made is Ast.Statement.BeginTransaction { Name.Number.Text: "5", Mark: not null },
			"CONTAINS with a language" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { Kind: Ast.FullTextPredicateKind.Contains, ColumnsBracketed: true, Columns.Count: 2, Search: Ast.Expression.Variable, Language: Ast.LiteralValue.Numeric } } },
			"FREETEXT of one column" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { Kind: Ast.FullTextPredicateKind.FreeText, ColumnsBracketed: false, Language: null } } },
			"FREETEXT of one column in brackets" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { ColumnsBracketed: true, Columns.Count: 1 } } },
			"CONTAINS of every column" => made is Ast.Statement.Select { Query: Ast.Query.Select { Where: Ast.Expression.FullTextPredicate { Columns: [Ast.Expression.Asterisk], ColumnsBracketed: true } } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
