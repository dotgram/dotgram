using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The External family's bridge (Bridge.External.cs): which record and members it builds for a text, where another record
/// or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class ExternalBridgeTests
{
	[Theory]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (LOCATION = 'l')",                  "Create")]
	[InlineData("ALTER EXTERNAL DATA SOURCE d SET LOCATION = 'l'",                      "Alter")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH ()",                                "Create empty")]
	[InlineData("create external data source d with (type = hadoop, pushdown = off)",   "Lower case")]
	[InlineData("CREATE EXTERNAL DATA SOURCE [d e] WITH (CREDENTIAL = [c d])",         "Names in brackets")]
	[InlineData("ALTER EXTERNAL DATA SOURCE d SET PUSHDOWN = ON, CREDENTIAL = c",       "Alter two")]
	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_TYPE = DELIMITEDTEXT, FORMAT_OPTIONS (FIRST_ROW = 2))", "File format")]
	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_OPTIONS ())",               "File format empty options")]
	public void The_bridge_builds_the_definition_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Create" => made is Ast.Statement.CreateExternalDataSource { Name.Text: "d", Options: [{ Name.Text: "LOCATION", Value: Ast.LiteralValue.String, Words.Count: 0, EqualsOmitted: false }] },
			"Alter" => made is Ast.Statement.AlterExternalDataSource { Name.Text: "d", Options: [{ Name.Text: "LOCATION", Value: Ast.LiteralValue.String }] },
			"Create empty" => made is Ast.Statement.CreateExternalDataSource { Options.Count: 0 },
			"Lower case" => made is Ast.Statement.CreateExternalDataSource { Options: [{ Name.Text: "type", Value: Ast.Expression.Reference { Name.Parts: [{ Text: "hadoop" }] } }, { Name.Text: "pushdown" }] },
			"Names in brackets" => made is Ast.Statement.CreateExternalDataSource { Name: { Text: "[d e]", Style: Ast.IdentifierStyle.Bracketed }, Options: [{ Value: Ast.Expression.Reference { Name.Parts: [{ Text: "[c d]", Style: Ast.IdentifierStyle.Bracketed }] } }] },
			"Alter two" => made is Ast.Statement.AlterExternalDataSource { Options: [{ Name.Text: "PUSHDOWN" }, { Name.Text: "CREDENTIAL" }] },
			"File format" => made is Ast.Statement.CreateExternalFileFormat { Options: [{ Name.Text: "FORMAT_TYPE", Value: not null }, { Name.Text: "FORMAT_OPTIONS", Value: null, Options: [{ Name.Text: "FIRST_ROW", Value: Ast.LiteralValue.Numeric { Text: "2" } }], EqualsOmitted: false }] },
			"File format empty options" => made is Ast.Statement.CreateExternalFileFormat { Options: [{ Name.Text: "FORMAT_OPTIONS", Value: null, Options.Count: 0 }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	/// <summary>
	/// A text the grammar reads and the records do not hold is a gap that says so, never another record: the other verb's
	/// body, and the commas the PolyBase lists take loose, which the tree keeps nothing for.
	/// </summary>
	[Theory]
	[InlineData("CREATE EXTERNAL DATA SOURCE d SET LOCATION = 'l'")]
	[InlineData("ALTER EXTERNAL DATA SOURCE d WITH (LOCATION = 'l')")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (LOCATION = 'l',)")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (,LOCATION = 'l')")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (LOCATION = 'l',,)")]
	[InlineData("CREATE EXTERNAL DATA SOURCE d WITH (,)")]
	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_TYPE = ORC,)")]
	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_TYPE = DELIMITEDTEXT, FORMAT_OPTIONS (FIRST_ROW = 2,))")]
	public void A_text_the_records_do_not_hold_is_a_gap(string text)
	{
		var old = TransactSqlParser.Located.ParseStatement(text);

		Assert.Throws<BridgeGap>(() => Bridge.ToAst(old, text));
	}
}
