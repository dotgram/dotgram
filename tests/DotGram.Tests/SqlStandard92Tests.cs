using System;

using DotGram.Parsers;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The SQL-92 expression parser, read.
/// </summary>
/// <remarks>
/// <para>
/// It had none, which came to light the moment it was worth knowing: the grammar was about
/// to be switched to the lexical split (<c>Lexical = true</c>) and "the suite is green" said
/// only that it compiled. A parser that nothing reads with is a parser nothing knows about.
/// </para>
/// <para>
/// Verdicts and not values — the grammar builds none, and what it is for is answering
/// whether a string is a <c>&lt;search condition&gt;</c>. The inputs are the corpus the
/// split was measured against, so the two say the same thing about the same strings and
/// this one says it where CI can hear.
/// </para>
/// </remarks>
public sealed class SqlStandard92Tests
{
	[Theory]
	[InlineData("a = 1")]
	[InlineData("salary BETWEEN 1000 AND 2000")]
	[InlineData("name LIKE 'A%' ESCAPE '\\'")]
	[InlineData("x IN (1, 2, 3) AND y IS NOT NULL")]
	[InlineData("(a + b) * c - d / e > f AND NOT g < h")]
	[InlineData("CAST(x AS INTEGER) = 5 OR SUBSTRING(s FROM 1 FOR 3) = 'abc'")]
	[InlineData("amount * 1.05 + tax >= total AND status <> 'CLOSED' AND created IS NOT NULL")]
	[InlineData("warehouse.zip_code = 'X' AND vendor_key IS NOT NULL AND quota > 0")]
	[InlineData("((((a + 1) * 2) - 3) / 4) + b > 0")]
	[InlineData("EXTRACT(YEAR FROM created) = 2020")]
	[InlineData("COALESCE(a, b, c) IS NOT NULL AND NULLIF(d, 0) > 1")]
	[InlineData("AVG(x) > 1 AND COUNT(*) < 100 AND SUM(DISTINCT y) = 0")]
	[InlineData("CAST(x AS NUMERIC(10, 2)) > 0")]
	[InlineData("CAST(x AS VARCHAR(20)) = 'a'")]
	public void A_search_condition_reads(string input)
	{
		Assert.True(SqlStandard92.TryParseSearchCondition(input).IsSuccess, input);
	}

	/// <summary>
	/// A word the standard does not reserve is a name, and a word it reserves is not.
	/// </summary>
	/// <remarks>
	/// §5.2 is a list of what an identifier may <em>not</em> be, and everything outside it is
	/// fair game — <c>zone</c> is a word of SQL-92, from <c>WITH TIME ZONE</c>, and a column
	/// may be called it. This is the case that the split got wrong first: over tokens the
	/// word arrives as its own kind and never reaches the identifier rule unless the kind
	/// says it is one too.
	/// </remarks>
	[Theory]
	[InlineData("zone = 1",       true)]
	[InlineData("year > month",   true)]
	[InlineData("having = 1",     false)]
	[InlineData("select = 1",     false)]
	[InlineData("\"select\" = 1", true)]

	// And one that is reserved and reads anyway, which is not an exception to the rule but
	// a different rule: §6.3 makes `VALUE` a niladic function, so `value = 1` is a value
	// specification compared with one rather than a column called `value`.
	[InlineData("value = 1",      true)]
	public void A_non_reserved_word_may_be_a_name(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandard92.TryParseSearchCondition(input).IsSuccess);
	}

	/// <summary>
	/// A `CASE` with more than one `WHEN`, which is what a woven word boundary used to refuse.
	/// </summary>
	[Theory]
	[InlineData("CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'small' ELSE 'none' END = label")]
	[InlineData("CASE a WHEN 1 THEN 2 WHEN 3 THEN 4 END = label")]
	public void A_case_may_have_more_than_one_when(string input)
	{
		Assert.True(SqlStandard92.TryParseSearchCondition(input).IsSuccess, input);
	}

	/// <summary>What is refused, and where.</summary>
	/// <remarks>
	/// The position is a character of the input whichever machine read it — over tokens it is
	/// mapped back through the extents, and getting that wrong is invisible until something
	/// asks. So it is asked here: the refusal is at the end of what could be read, not at the
	/// beginning of what could not.
	/// </remarks>
	[Theory]
	[InlineData("a = ")]
	[InlineData("BETWEEN 1 AND 2")]
	[InlineData("a AND")]
	[InlineData("(a + b")]
	[InlineData("a = 1 OR")]
	[InlineData("x IN")]
	public void What_is_not_a_search_condition_is_refused(string input)
	{
		var match = SqlStandard92.TryParseSearchCondition(input);

		Assert.False(match.IsSuccess, input);
		Assert.InRange(match.Position, 0, input.Length);
	}

	/// <summary>A refusal is reported where the reading stopped, not where it started.</summary>
	[Fact]
	public void A_refusal_says_how_far_it_got()
	{
		var early = SqlStandard92.TryParseSearchCondition("= 1");
		var late  = SqlStandard92.TryParseSearchCondition("a = 1 AND b = 2 AND c =");

		Assert.False(early.IsSuccess);
		Assert.False(late.IsSuccess);

		Assert.True(late.Position > early.Position, $"{late.Position} should be past {early.Position}");
	}

	/// <summary>And a value expression is its own publication.</summary>
	[Theory]
	[InlineData("a + b * c")]
	[InlineData("CAST(x AS INTEGER)")]
	[InlineData("'a' || 'b'")]
	public void A_value_expression_reads(string input)
	{
		Assert.True(SqlStandard92.TryParseValueExpression(input).IsSuccess, input);
	}
}
