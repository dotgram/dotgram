using System;
using System.Diagnostics;
using System.Linq;

using DotGram.Sql.Productions;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// A text of block comments that never close is refused in time linear in its length.
/// </summary>
/// <remarks>
/// A comment nests at any depth, so the counter that reads one reads to the end of the text before
/// it knows the comment is unclosed. The tokenizer once took the `/` and `*` of the opening for
/// tokens and asked the trivia again at the next opening inside it, which read to the end again:
/// the square of the length, five seconds for forty thousand openings. Now a comment the text ends
/// inside of ends the tokens where it opened, as a character no token begins with does, and is
/// refused there; the text is read once. A hundred thousand openings are read in milliseconds; the bound below
/// leaves room for a busy machine and none for the square.
/// </remarks>
public sealed class UnclosedCommentTests
{
	const int Openings = 100_000;

	static readonly string Unclosed = string.Concat(Enumerable.Repeat("/* ", Openings));

	static readonly string HalfClosed = string.Concat(Enumerable.Repeat("/* /* */ ", Openings));

	static readonly string Balanced = string.Concat(Enumerable.Repeat("/* ", Openings)) + string.Concat(Enumerable.Repeat("*/ ", Openings));

	public static TheoryData<string> Comments => new() { "unclosed", "half-closed" };

	static string Comment(string name)
	{
		return name == "unclosed" ? Unclosed : HalfClosed;
	}

	[Theory]
	[MemberData(nameof(Comments))]
	public void A_TSql_statement_is_refused_in_linear_time(string name)
	{
		var comment = Comment(name);

		Quick(() => Assert.False(TransactSqlParser.TryParseStatement("SELECT 1 " + comment).IsSuccess));
		Quick(() => Assert.False(TransactSqlParser.TryParseStatement(comment + "SELECT 1").IsSuccess));
		Quick(() => Assert.False(TransactSqlParser.Located.TryParseStatement("SELECT 1 " + comment).IsSuccess));
	}

	[Theory]
	[MemberData(nameof(Comments))]
	public void A_TSql_script_is_refused_in_linear_time(string name)
	{
		var comment = Comment(name);

		Quick(() => Assert.False(TransactSqlParser.TryParseScript("SELECT 1\nGO\nSELECT 2 " + comment).IsSuccess));
	}

	[Theory]
	[MemberData(nameof(Comments))]
	public void A_standard_literal_and_statement_are_refused_in_linear_time(string name)
	{
		var comment = Comment(name);

		Quick(() => Assert.False(SqlStandardProductions.TryParseLiteral("1 " + comment).IsSuccess));
		Quick(() => Assert.False(SqlStandardProductions.TryParseLiteral(comment + "1").IsSuccess));
		Quick(() => Assert.False(SqlStandardParser.TryParseStatement("SELECT 1 " + comment).IsSuccess));
		Quick(() => Assert.False(SqlStandardProductions.TryParseLiteral("'a' " + comment).IsSuccess));
	}

	[Fact]
	public void A_balanced_comment_a_hundred_thousand_deep_reads_in_linear_time()
	{
		Read(Balanced);

		// Four times the depth takes about four times as long when the read is linear and sixteen
		// times as long when it is quadratic. The ratio of the best times of both, measured in the
		// same run, does not move with the load of the machine as an absolute bound does; ten
		// separates the two with room on both sides.
		var shorter = string.Concat(Enumerable.Repeat("/* ", Openings / 4)) + string.Concat(Enumerable.Repeat("*/ ", Openings / 4));

		Read(shorter);

		var few  = double.MaxValue;
		var many = double.MaxValue;

		for (var attempt = 0; attempt < 7; attempt++)
		{
			few  = Math.Min(few, Time(shorter));
			many = Math.Min(many, Time(Balanced));
		}

		var ratio = many / Math.Max(few, 0.001);

		Assert.True(ratio < 10, $"four times the depth took {ratio:F1} times as long ({few:F1} ms, {many:F1} ms)");
	}

	static void Read(string comment)
	{
		Assert.True(TransactSqlParser.TryParseStatement("SELECT 1 " + comment).IsSuccess);
		Assert.True(SqlStandardProductions.TryParseLiteral("1 " + comment).IsSuccess);
	}

	static double Time(string comment)
	{
		var watch = Stopwatch.StartNew();

		Read(comment);

		return watch.Elapsed.TotalMilliseconds;
	}

	static void Quick(Action read)
	{
		var watch = Stopwatch.StartNew();

		read();

		Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1), $"took {watch.Elapsed.TotalMilliseconds:F0} ms");
	}
}
