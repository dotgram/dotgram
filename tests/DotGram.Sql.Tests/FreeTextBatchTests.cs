using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// The writer round's free-text batch: the old tree's texts that now compare token by token, the two column reference
/// texts that do so only under the DROP that holds them, and the one exemption, <c>EXECUTE p</c>.
/// </summary>
public sealed class FreeTextBatchTests
{
	static Statement Old(string text)
	{
		return OldTree.Located.ParseStatement(text);
	}

	/// <summary>
	/// A text of the batch written otherwise than the writer prints it, but for the case of its key words and its blanks:
	/// the bridge carries it, the writer prints it its own way, and the oracle counts the statement converted, by case.
	/// </summary>
	[Theory]
	[InlineData("DECLARE @a int",                                          "Clause.VariableDeclaration.Type")]
	[InlineData("SET STATISTICS io ON",                                    "SetExpression.Statistics.Report.Kind")]
	[InlineData("RAISERROR ('x', 1, 1) with nowait",                       "Statement.RaiseError.Tail")]
	[InlineData("EXEC p with recompile",                                   "Statement.Execute.Tail")]
	public void A_text_of_the_batch_converts_by_case(string text, string member)
	{
		var outcome = BridgeOracle.Run(Old(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
		Assert.Contains(member, outcome.ByCase!);
	}

	/// <summary>
	/// The same texts of the statements whose rules build the common tree: the words are the tree's own, written in its
	/// capitals, and the statement converts with nothing aside.
	/// </summary>
	[Theory]
	[InlineData("CREATE PROCEDURE p @a int AS SELECT 1")]
	[InlineData("CREATE FUNCTION f () RETURNS int AS BEGIN RETURN 1 END")]
	[InlineData("DROP SYMMETRIC KEY k remove provider key")]
	[InlineData("DROP ASSEMBLY a with no dependents")]
	[InlineData("DROP TRIGGER r on database")]
	[InlineData("DROP EVENT SESSION s on server")]
	[InlineData("DROP SIGNATURE FROM object::p BY CERTIFICATE c")]
	[InlineData("DROP SIGNATURE FROM p BY certificate c")]
	[InlineData("DROP INDEX i on t")]
	[InlineData("DROP INDEX i ON t with (ONLINE = ON)")]
	public void A_text_of_a_rule_that_builds_the_common_tree_converts_with_nothing_aside(string text)
	{
		var outcome = BridgeOracle.Run(Old(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
		Assert.Empty(outcome.ByCase!);
	}

	/// <summary>
	/// A column reference's text compares exactly everywhere but under DROP INDEX and DROP SIGNATURE: two old trees whose
	/// column differs by case alone differ, and one under DROP INDEX does not.
	/// </summary>
	[Fact]
	public void A_column_reference_compares_exactly_outside_the_drops()
	{
		Assert.NotNull(Trees.Difference(Old("SELECT a FROM t"), Old("SELECT A FROM t")));
		Assert.NotNull(Trees.Difference(Old("SELECT 1 FROM t WHERE x = 1"), Old("SELECT 1 FROM t WHERE X = 1")));

		// A column named as a key word is a name outside the drops, whose case is kept: `date` against `DATE`.
		Assert.NotNull(Trees.Difference(Old("SELECT date FROM t"), Old("SELECT DATE FROM t")));
		Assert.NotNull(Trees.Difference(Old("SELECT a FROM t ORDER BY date"), Old("SELECT a FROM t ORDER BY DATE")));
		Assert.NotNull(Trees.Difference(Old("DROP TABLE date"), Old("DROP TABLE DATE")));

		// DROP INDEX builds the common tree, whose words are its own: the two are one tree, nothing aside.
		var byCase = new List<string>();

		Assert.Null(Trees.Difference(Old("DROP INDEX i on t"), Old("DROP INDEX i ON t"), byCase: byCase));
		Assert.Empty(byCase);

		// A name inside the index's text is a name: its case is kept.
		Assert.NotNull(Trees.Difference(Old("DROP INDEX i ON t"), Old("DROP INDEX I ON t")));
	}

	/// <summary>
	/// <c>EXECUTE p</c> alone is T-SQL's procedure call and the standard's EXECUTE of a prepared statement: the oracle
	/// exempts the whole statement under its pattern, and the statement converts.
	/// </summary>
	[Fact]
	public void EXECUTE_p_alone_means_two_things()
	{
		const string text = "EXECUTE p";

		var outcome = BridgeOracle.Run(Old(text), text);

		Assert.Equal(BridgeOracle.Status.Converted, outcome.Status);
		Assert.True(outcome.Standard);
		Assert.False(outcome.Shape);
		Assert.Equal([BridgeOracle.Execute], outcome.Dialects!);
	}

	/// <summary>
	/// <c>EXECUTE p 1</c> and <c>EXEC p</c> are not exempted: the standard reads neither (its EXECUTE takes USING for its
	/// arguments, and it has no EXEC), so neither is asked for its shape.
	/// </summary>
	[Theory]
	[InlineData("EXECUTE p 1")]
	[InlineData("EXEC p")]
	public void EXECUTE_with_anything_else_is_not_exempted(string text)
	{
		Assert.False(SqlStandardParser.TryParseStatement(text).IsSuccess);

		var outcome = BridgeOracle.Run(Old(text), text);

		Assert.Equal(BridgeOracle.Status.Converted, outcome.Status);
		Assert.False(outcome.Standard);
		Assert.Null(outcome.Dialects);
	}

	/// <summary>
	/// The pattern takes the whole statement and nothing else: an EXECUTE whose name has two parts, or the standard's
	/// EXECUTE of another name, is a shape mismatch.
	/// </summary>
	[Theory]
	[InlineData("EXECUTE dbo.p", "EXECUTE p")]
	[InlineData("EXECUTE p",     "EXECUTE q")]
	public void The_EXECUTE_pattern_exempts_the_one_call_alone(string bridged, string standard)
	{
		var made        = Bridge.ToAst(Old(bridged), bridged);
		var differences = Trees.Differences(made, SqlStandardParser.ParseStatement(standard), old: false);
		var exempted    = BridgeOracle.Meaning(differences, out var unexplained);

		Assert.DoesNotContain(BridgeOracle.Execute, exempted);
		Assert.NotEmpty(unexplained);
	}
}
