using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside the namespace, where it is asked before DotGram.Sql's own: the tree T-SQL builds.
using Expression = DotGram.Sql.Ast.Expression;

/// <summary>
/// A literal of the SQL:2023 tree is an <see cref="Expression"/> itself: where it stands as a value
/// there is no node around it, and a position typed <see cref="LiteralValue"/> takes the same node.
/// </summary>
public sealed class LiteralValueTests
{
	[Fact]
	public void A_literal_standing_as_a_value_is_the_literal()
	{
		Assert.IsType<LiteralValue.Numeric>(SqlStandardParser.ParseExpression("1"));
		Assert.IsType<LiteralValue.Null>(SqlStandardParser.ParseValue("NULL"));

		var sum = Assert.IsType<Expression.Binary>(SqlStandardParser.ParseExpression("a + 'x'"));

		Assert.Equal(new LiteralValue.String("'x'", StringLiteralKind.Character), sum.Right);
		Assert.Equal(SqlStandardParser.ParseValue("'x'"), sum.Right);
	}

	/// <summary>The walker meets a literal once, as the expression it is.</summary>
	[Fact]
	public void The_walker_meets_a_literal_once()
	{
		var visited = new List<ISqlSpan>();

		SqlWalker.Walk(SqlStandardParser.ParseExpression("a + 1"), node =>
		{
			visited.Add(node);

			return true;
		});

		Assert.Single(visited.OfType<LiteralValue>());
		Assert.Equal(new[] { "Binary", "Reference", "QualifiedName", "Identifier", "Numeric" }, visited.Select(static node => node.GetType().Name));
	}

	[Theory]
	[InlineData("1 + 2.5")]
	[InlineData("-1")]
	[InlineData("CAST(NULL AS INT)")]
	[InlineData("X'0A' || X'0B'")]
	[InlineData("DATE '2026-10-13'")]
	[InlineData("INTERVAL '1' DAY")]
	public void A_literal_is_written_where_it_stands(string text)
	{
		var read = SqlStandardParser.ParseExpression(text);

		Assert.Equal(read, SqlStandardParser.ParseExpression(SqlWriter.Write(read)));
	}
}
