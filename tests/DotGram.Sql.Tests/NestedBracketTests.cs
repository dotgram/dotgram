using System;
using System.Threading;

using DotGram.Sql;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// Brackets in a condition, as the grammar reads them today: a bracket that holds a condition is a
/// condition, one that holds a value is the predicate's operand, and a predicate's row is read once
/// for every tail it may have.
/// </summary>
/// <remarks>
/// <para>
/// <c>BooleanPrimary</c> (SqlStandard92.gram) asks for a predicate before a bracketed condition, so
/// under <c>((( … a = 1 … )))</c> every level first reads everything below it as a value, and every
/// value bracket reads everything below it again as a subquery: the cube of the depth in rule
/// entries. What that costs is counted in <c>BracketNestingCountTests</c> (DotGram.Tests.Slow); these
/// rows hold the trees and the readings, so that a change to how a bracket is read — reordering
/// <c>BooleanPrimary</c> was tried, and made other nests exponential — has to keep them or say why.
/// </para>
/// </remarks>
public sealed class NestedBracketTests
{
	[Fact]
	public void A_bracket_that_holds_a_condition_is_a_condition()
	{
		var outer = Assert.IsType<Expression.Parenthesized>(TransactSqlParser.TryParseSearchCondition("((a = 1))").Value);
		var inner = Assert.IsType<Expression.Parenthesized>(outer.Value);
		var equal = Assert.IsType<Expression.Comparison>(inner.Value);

		Assert.Equal(SqlComparison.Equal, equal.Operator);
		Assert.IsType<Expression.ColumnReference>(equal.Left);
	}

	[Fact]
	public void A_bracket_that_holds_a_value_is_the_predicate_s_operand()
	{
		var equal = Assert.IsType<Expression.Comparison>(TransactSqlParser.TryParseSearchCondition("((a)) = 1").Value);
		var outer = Assert.IsType<Expression.Parenthesized>(equal.Left);
		var inner = Assert.IsType<Expression.Parenthesized>(outer.Value);

		Assert.IsType<Expression.ColumnReference>(inner.Value);
	}

	[Fact]
	public void A_value_bracket_inside_a_condition_bracket_is_each_what_it_holds()
	{
		var condition = Assert.IsType<Expression.Parenthesized>(TransactSqlParser.TryParseSearchCondition("((a) + 1 = 2)").Value);
		var equal     = Assert.IsType<Expression.Comparison>(condition.Value);
		var sum       = Assert.IsType<Expression.Add>(equal.Left);

		Assert.IsType<Expression.Parenthesized>(sum.Left);
	}

	[Fact]
	public void A_bracketed_subquery_compared_is_a_value()
	{
		var equal = Assert.IsType<Expression.Comparison>(TransactSqlParser.TryParseSearchCondition("((SELECT 1)) = 1").Value);
		var outer = Assert.IsType<Expression.Parenthesized>(equal.Left);

		Assert.IsType<Expression.Subquery>(outer.Value);
	}

	[Theory]
	[InlineData("(a) IS DISTINCT FROM (b)",     false)]
	[InlineData("(a) IS NOT DISTINCT FROM (b)", true)]
	[InlineData("a IS NOT DISTINCT FROM b",     true)]
	public void A_row_read_once_still_takes_IS_DISTINCT_FROM(string input, bool negated)
	{
		var distinct = Assert.IsType<Expression.IsDistinctFrom>(TransactSqlParser.TryParseSearchCondition(input).Value);

		Assert.Equal(negated, distinct.Negated);
	}

	[Fact]
	public void A_row_read_once_still_takes_the_standard_s_tails()
	{
		Assert.IsType<Expression.IsNull>(TransactSqlParser.TryParseSearchCondition("(a) IS NOT NULL").Value);
		Assert.IsType<Expression.In>(TransactSqlParser.TryParseSearchCondition("(a) IN (1, 2)").Value);
		Assert.IsType<Expression.Between>(TransactSqlParser.TryParseSearchCondition("(a) BETWEEN (1) AND (2)").Value);
	}

	/// <summary>
	/// <c>REGEXP_LIKE (…)</c> is a condition from 170 and a call's value at every level, and in
	/// brackets the predicate is asked first, so a bracketed call compared or tested for null reads as
	/// a predicate on the call's value at every level, 170 and the reading that names no level
	/// included.
	/// </summary>
	/// <remarks>
	/// <b>Not what the engine does, and pinned as it is.</b> SQL Server 2025 at level 170 refuses both:
	/// <c>Msg 102, Incorrect syntax near '='</c> and <c>Msg 156</c> near <c>IS</c>; at 160 and below it
	/// reads both, as here. Refusing them at 170 is a change of its own, apart from how a bracket's
	/// cost is fixed; this row says what is read today so that the change is seen when it comes.
	/// </remarks>
	[Fact]
	public void A_bracketed_truth_is_read_as_a_predicate_s_operand_today()
	{
		Assert.True(TransactSqlParser.TryParseStatement170("SELECT 1 WHERE (REGEXP_LIKE ('abc', '^a'))").IsSuccess);
		Assert.True(TransactSqlParser.TryParseStatement170("SELECT 1 WHERE (REGEXP_LIKE ('abc', '^a')) = 1").IsSuccess);
		Assert.True(TransactSqlParser.TryParseStatement170("SELECT 1 WHERE (REGEXP_LIKE ('abc', '^a')) IS NULL").IsSuccess);

		Assert.True(TransactSqlParser.TryParseStatement160("SELECT 1 WHERE (REGEXP_LIKE ('abc', '^a')) = 1").IsSuccess);
		Assert.True(TransactSqlParser.TryParseStatement160("SELECT 1 WHERE (REGEXP_LIKE ('abc', '^a')) IS NULL").IsSuccess);

		Assert.True(TransactSqlParser.TryParseStatement("SELECT 1 WHERE (REGEXP_LIKE ('abc', '^a'))").IsSuccess);
		Assert.True(TransactSqlParser.TryParseStatement("SELECT 1 WHERE (REGEXP_LIKE ('abc', '^a')) = 1").IsSuccess);
		Assert.True(TransactSqlParser.TryParseStatement("SELECT 1 WHERE (REGEXP_LIKE ('abc', '^a')) IS NULL").IsSuccess);
	}

	/// <summary>
	/// A nest of conditions. Shallower than the other rows: it costs the cube of its depth today.
	/// </summary>
	[Theory]
	[InlineData(100)]
	public void A_deep_nest_of_conditions_is_read(int depth)
	{
		var text  = new string('(', depth) + "a = 1" + new string(')', depth);
		var value = Deep(() => TransactSqlParser.TryParseSearchCondition(text).Value);

		Assert.IsType<Expression.Comparison>(Unwrap(value, depth));
	}

	/// <summary>Values nested in a predicate's operand, read once a level.</summary>
	[Theory]
	[InlineData(1_000)]
	public void A_deep_nest_of_values_in_a_predicate_is_read(int depth)
	{
		var text  = new string('(', depth) + "a" + new string(')', depth) + " = 1";
		var equal = Assert.IsType<Expression.Comparison>(Deep(() => TransactSqlParser.TryParseSearchCondition(text).Value));

		Assert.IsType<Expression.ColumnReference>(Unwrap(equal.Left, depth));
	}

	/// <summary>
	/// Conditions around values nested in their predicate, <c>(… (… a …) = 1 …)</c>. Shallow for the
	/// same reason as the nest of conditions.
	/// </summary>
	[Theory]
	[InlineData(50)]
	public void A_deep_nest_of_conditions_around_nested_values_is_read(int depth)
	{
		var open  = new string('(', depth);
		var close = new string(')', depth);
		var text  = open + open + "a" + close + " = 1" + close;
		var equal = Assert.IsType<Expression.Comparison>(Unwrap(Deep(() => TransactSqlParser.TryParseSearchCondition(text).Value), depth));

		Assert.IsType<Expression.ColumnReference>(Unwrap(equal.Left, depth));
	}

	[Fact]
	public void The_standard_s_own_parser_reads_the_same_trees()
	{
		var condition = Assert.IsType<Expression.Parenthesized>(Sql92Parser.TryParseSearchCondition("((a = 1))").Value);
		Assert.IsType<Expression.Parenthesized>(condition.Value);

		var equal = Assert.IsType<Expression.Comparison>(Sql92Parser.TryParseSearchCondition("((a)) = 1").Value);
		Assert.IsType<Expression.Parenthesized>(equal.Left);
	}

	/// <summary>What stands inside that many brackets, each of which must be one.</summary>
	static Expression Unwrap(Expression value, int depth)
	{
		for (var level = 0; level < depth; level++)
			value = Assert.IsType<Expression.Parenthesized>(value).Value;

		return value;
	}

	/// <summary>On a thread with room for the nest, so the row does not depend on the runner's stack.</summary>
	static Expression Deep(Func<Expression> read)
	{
		Expression? value = null;
		Exception?  error = null;

		var thread = new Thread(() =>
		{
			try { value = read(); }
			catch (Exception e) { error = e; }
		}, 64 * 1024 * 1024);

		thread.Start();
		thread.Join();

		if (error is not null)
			throw new InvalidOperationException("the deep read threw", error);

		return value!;
	}
}
