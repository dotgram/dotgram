using System;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// What a refusal says where a check of the grammar's own C# refused: words about the input, never
/// the C# that asked.
/// </summary>
public sealed class GuardWordsTests
{
	[Theory]
	[InlineData("bulk insert t1 from 'f1' with (keepnulls, keepnulls)",           "Expected each option once.")]
	[InlineData("GRANT SELECT ON ::s (a) TO u",                                     "Expected permissions the securable has.")]
	public void A_TSql_check_says_what_it_wants(string text, string error)
	{
		var read = TransactSqlParser.TryParseStatement(text);

		Assert.False(read.IsSuccess);
		Assert.Equal(error, read.Error);
	}

	[Theory]
	[InlineData("CREATE TABLE t (c varchar(99999999999999999999999))", "Expected a number that fits in an int.")]
	[InlineData("(a - CURRENT_DATE, CURRENT_DATE) DAY",                "Expected a datetime difference in brackets.")]
	public void A_standard_check_says_what_it_wants(string text, string error)
	{
		var read = text.StartsWith("CREATE", StringComparison.Ordinal)
			? SqlStandardParser.TryParseStatement(text).Error
			: SqlStandardParser.TryParseExpression(text).Error;

		Assert.Equal(error, read);
	}
}
