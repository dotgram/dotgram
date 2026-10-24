using System;
using System.Linq;

using DotGram.Sql.Productions;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// A text of block comments that never close is refused where the first opens, and one that closes is read.
/// </summary>
/// <remarks>
/// A comment nests at any depth, so the counter that reads one reads to the end of the text before
/// it knows the comment is unclosed. The tokenizer once took the `/` and `*` of the opening for
/// tokens and asked the trivia again at the next opening inside it, which read to the end again:
/// the square of the length. Now a comment the text ends inside of ends the tokens where it opened,
/// as a character no token begins with does, and is refused there; the text is read once. That it
/// takes no longer than its length is held in <c>CommentScalingTests</c>, over a ratio of times in
/// one run; what is held here is what each reader answers, at a depth no clock is needed for.
/// </remarks>
public sealed class UnclosedCommentTests
{
	const int Openings = 2_000;

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
	public void A_TSql_statement_is_refused(string name)
	{
		var comment = Comment(name);

		Assert.False(TransactSqlParser.TryParseStatement("SELECT 1 " + comment).IsSuccess);
		Assert.False(TransactSqlParser.TryParseStatement(comment + "SELECT 1").IsSuccess);
		Assert.False(TransactSqlParser.Located.TryParseStatement("SELECT 1 " + comment).IsSuccess);
	}

	[Theory]
	[MemberData(nameof(Comments))]
	public void A_TSql_script_is_refused(string name)
	{
		var comment = Comment(name);

		Assert.False(TransactSqlParser.TryParseScript("SELECT 1\nGO\nSELECT 2 " + comment).IsSuccess);
	}

	[Theory]
	[MemberData(nameof(Comments))]
	public void A_standard_literal_and_statement_are_refused(string name)
	{
		var comment = Comment(name);

		Assert.False(SqlStandardProductions.TryParseLiteral("1 " + comment).IsSuccess);
		Assert.False(SqlStandardProductions.TryParseLiteral(comment + "1").IsSuccess);
		Assert.False(SqlStandardParser.TryParseStatement("SELECT 1 " + comment).IsSuccess);
		Assert.False(SqlStandardProductions.TryParseLiteral("'a' " + comment).IsSuccess);
	}

	[Fact]
	public void A_balanced_comment_is_read()
	{
		Assert.True(TransactSqlParser.TryParseStatement("SELECT 1 " + Balanced).IsSuccess);
		Assert.True(SqlStandardProductions.TryParseLiteral("1 " + Balanced).IsSuccess);
	}
}
