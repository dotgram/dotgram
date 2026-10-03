using System;
using System.Text;
using System.Threading;

using DotGram.Sql;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every shape of nesting a parser reads to any depth is read, and its tree written, on a thread of
/// an ordinary size: a reading either ends in a tree or a refusal, never in a stack overflow.
/// </summary>
/// <remarks>
/// <para>
/// The sweep behind <c>DeepTreeTests</c> in DotGram.Sql.Tests, which holds the shapes the defects
/// were found in; this holds every shape the three parsers nest, at a cost of half a minute, which is
/// why it is here (D12). Ten thousand levels on a megabyte is past the point where every one of them
/// runs a stack out, so a level that misses its check is one that overflows — and a stack overflow
/// cannot be caught: it ends the test process, and the run is red because it never finished. Anything
/// less than a crash is held to a verdict: the tree is read, written, and the text written read and
/// written again to the same text.
/// </para>
/// <para>
/// SQL:2023 is read here only in the shapes it reads in time linear in their depth. Its nested
/// <c>CASE</c>, <c>COALESCE</c> and <c>IN (SELECT …)</c> are read in time that grows with the square of
/// the depth, which is a cost and not a crash, and ten thousand levels of them take half a minute.
/// </para>
/// </remarks>
public sealed class DeepTreeSweepTests
{
	const int Depth = 10_000;

	/// <summary>The size of thread a host gets without asking.</summary>
	const int Stack = 1024 * 1024;

	[Theory]
	[InlineData("CASE in THEN",           "SELECT ",          "CASE WHEN 1 = 1 THEN ",         "1",         " END",                      "")]
	[InlineData("CASE in ELSE",           "SELECT ",          "CASE WHEN 1 = 1 THEN 1 ELSE ",  "1",         " END",                      "")]
	[InlineData("CASE in WHEN",           "SELECT ",          "CASE WHEN 1 = ",                "1",         " THEN 1 END",               "")]
	[InlineData("CASE operand",           "SELECT ",          "CASE ",                         "1",         " WHEN 1 THEN 1 END",        "")]
	[InlineData("brackets",               "SELECT ",          "(",                             "1",         ")",                         "")]
	[InlineData("subqueries",             "SELECT ",          "(SELECT ",                      "1",         ")",                         "")]
	[InlineData("signs",                  "SELECT ",          "- ",                            "1",         "",                          "")]
	[InlineData("signs on brackets",      "SELECT ",          "-(",                            "1",         ")",                         "")]
	[InlineData("bitwise NOT",            "SELECT ",          "~",                             "1",         "",                          "")]
	[InlineData("calls",                  "SELECT ",          "UPPER(",                        "'a'",       ")",                         "")]
	[InlineData("COALESCE",               "SELECT ",          "COALESCE(1, ",                  "1",         ")",                         "")]
	[InlineData("CAST",                   "SELECT ",          "CAST(",                         "1",         " AS INT)",                  "")]
	[InlineData("+ to the right",         "SELECT ",          "1 + (",                         "1",         ")",                         "")]
	[InlineData("+",                      "SELECT ",          "1 + ",                          "1",         "",                          "")]
	[InlineData("string +",               "SELECT ",          "'a' + ",                        "'a'",       "",                          "")]
	[InlineData("NOT",                    "SELECT 1 WHERE ",  "NOT ",                          "1 = 1",     "",                          "")]
	[InlineData("NOT on brackets",        "SELECT 1 WHERE ",  "NOT (",                         "1 = 1",     ")",                         "")]
	[InlineData("AND",                    "SELECT 1 WHERE ",  "1 = 1 AND ",                    "1 = 1",     "",                          "")]
	[InlineData("OR",                     "SELECT 1 WHERE ",  "1 = 1 OR ",                     "1 = 1",     "",                          "")]
	[InlineData("conditions in brackets", "SELECT 1 WHERE ",  "(",                             "1 = 1",     ")",                         "")]
	[InlineData("IN (SELECT …)",          "SELECT 1 WHERE ",  "1 IN (SELECT 1 WHERE ",         "1 = 1",     ")",                         "")]
	[InlineData("EXISTS",                 "SELECT 1 WHERE ",  "EXISTS (SELECT 1 WHERE ",       "1 = 1",     ")",                         "")]
	[InlineData("AT TIME ZONE",           "SELECT a",         " AT TIME ZONE 'UTC'",           "",          "",                          "")]
	[InlineData("derived tables",         "SELECT 1 FROM ",   "(SELECT 1 AS a FROM ",          "t",         ") AS x",                    "")]
	[InlineData("joins",                  "SELECT 1 FROM t",  " JOIN t ON 1 = 1",              "",          "",                          "")]
	[InlineData("PIVOT",                  "SELECT 1 FROM t",  " PIVOT (MAX(a) FOR b IN (c)) p", "",         "",                          "")]
	[InlineData("UNION",                  "SELECT ",          "1 UNION SELECT ",               "1",         "",                          "")]
	[InlineData("UNION in brackets",      "SELECT ",          "(SELECT 1 UNION ",              "SELECT 1",  ")",                         "")]
	[InlineData("IF",                     "",                 "IF 1 = 1 ",                     "SELECT 1",  "",                          "")]
	[InlineData("WHILE",                  "",                 "WHILE 1 = 1 ",                  "SELECT 1",  "",                          "")]
	[InlineData("BEGIN … END",            "",                 "BEGIN ",                        "SELECT 1",  " END",                      "")]
	[InlineData("TRY … CATCH",            "",                 "BEGIN TRY ",                    "SELECT 1",  " END TRY BEGIN CATCH SELECT 2 END CATCH", "")]
	public void TransactSql_reads_and_writes_any_depth(string shape, string prefix, string open, string middle, string close, string suffix)
	{
		var text = Nested(prefix, open, middle, close, suffix, Depth);

		var (written, thrown) = OnStack(() =>
		{
			Assert.True(TransactSqlParser.Located.TryParseStatement(text).IsSuccess, $"{shape}: the located reading refused it");
			Assert.True(TransactSqlParser.TryParseScript(text).IsSuccess, $"{shape}: the script reading refused it");

			return RoundTrip(text, TransactSqlParser.ParseStatement, SqlWriter.Write);
		});

		Assert.Null(thrown);
		Assert.NotEmpty(written);
	}

	[Theory]
	[InlineData("CASE in ELSE",   "SELECT ",          "CASE WHEN 1 = 1 THEN 1 ELSE ",    "1",     " END",           " FROM t")]
	[InlineData("CASE in THEN",   "SELECT ",          "CASE WHEN 1 = 1 THEN ",           "1",     " END",           " FROM t")]
	[InlineData("brackets",       "SELECT ",          "(",                               "1",     ")",              " FROM t")]
	[InlineData("subqueries",     "SELECT ",          "(SELECT ",                        "1",     " FROM t)",       " FROM t")]
	[InlineData("+",              "SELECT ",          "1 + ",                            "1",     "",               " FROM t")]
	[InlineData("||",             "SELECT ",          "'a' || ",                         "'a'",   "",               " FROM t")]
	[InlineData("AND",            "SELECT 1 FROM t WHERE ", "1 = 1 AND ",                "1 = 1", "",               "")]
	[InlineData("IN (SELECT …)",  "SELECT 1 FROM t WHERE ", "1 IN (SELECT 1 FROM t WHERE ", "1 = 1", ")",           "")]
	[InlineData("derived tables", "SELECT 1 FROM ",   "(SELECT a FROM ",                 "t",     ") AS x",         "")]
	[InlineData("UNION",          "SELECT ",          "1 FROM t UNION SELECT ",          "1",     "",               " FROM t")]
	[InlineData("joins",          "SELECT 1 FROM ",   "t JOIN (",                        "t",     ") ON 1 = 1",     "")]
	public void Sql92_reads_and_writes_any_depth(string shape, string prefix, string open, string middle, string close, string suffix)
	{
		var text = Nested(prefix, open, middle, close, suffix, Depth);

		var (written, thrown) = OnStack(() => RoundTrip(text, Sql92Parser.ParseSelect, SqlWriter.Write));

		Assert.True(thrown is null, $"{shape}: {thrown}");
		Assert.NotEmpty(written);
	}

	[Theory]
	[InlineData("brackets",          "SELECT ",          "(",                     "1",     ")",          " FROM t")]
	[InlineData("subqueries",        "SELECT ",          "(SELECT ",              "1",     " FROM t)",   " FROM t")]
	[InlineData("signs on brackets", "SELECT ",          "-(",                    "1",     ")",          " FROM t")]
	[InlineData("calls",             "SELECT ",          "ABS(",                  "1",     ")",          " FROM t")]
	[InlineData("CASE operand",      "SELECT ",          "CASE ",                 "1",     " WHEN 1 THEN 1 END", " FROM t")]
	[InlineData("CAST",              "SELECT ",          "CAST(",                 "1",     " AS INTEGER)", " FROM t")]
	[InlineData("+",                 "SELECT ",          "1 + ",                  "1",     "",           " FROM t")]
	[InlineData("||",                "SELECT ",          "'a' || ",               "'a'",   "",           " FROM t")]
	[InlineData("AND",               "SELECT 1 FROM t WHERE ", "1 = 1 AND ",      "1 = 1", "",           "")]
	[InlineData("NOT on brackets",   "SELECT 1 FROM t WHERE ", "NOT (",           "1 = 1", ")",          "")]
	[InlineData("derived tables",    "SELECT 1 FROM ",   "(SELECT 1 AS a FROM ",  "t",     ") AS x",     "")]
	[InlineData("joins",             "SELECT 1 FROM t",  " JOIN t ON 1 = 1",      "",      "",           "")]
	[InlineData("UNION",             "SELECT ",          "1 FROM t UNION SELECT ", "1",    "",           " FROM t")]
	public void Sql2023_reads_and_writes_any_depth(string shape, string prefix, string open, string middle, string close, string suffix)
	{
		var text = Nested(prefix, open, middle, close, suffix, Depth);

		var (written, thrown) = OnStack(() => RoundTrip(text, SqlStandardParser.ParseQueryExpression, DotGram.Sql.Ast.Sql2023Writer.Write));

		Assert.True(thrown is null, $"{shape}: {thrown}");
		Assert.NotEmpty(written);
	}

	/// <summary>
	/// Reads <paramref name="text"/>, writes the tree, reads what was written and writes that, and holds
	/// the two texts written to each other.
	/// </summary>
	static string RoundTrip<T>(string text, Func<string, T> parse, Func<T, string> write)
	{
		var written = write(parse(text));
		var again   = write(parse(written));

		Assert.Equal(written, again);

		return written;
	}

	static string Nested(string prefix, string open, string middle, string close, string suffix, int depth)
	{
		var text = new StringBuilder(prefix.Length + suffix.Length + middle.Length + (open.Length + close.Length) * depth);

		text.Append(prefix);

		for (var i = 0; i < depth; i++)
			text.Append(open);

		text.Append(middle);

		for (var i = 0; i < depth; i++)
			text.Append(close);

		return text.Append(suffix).ToString();
	}

	/// <summary>
	/// Runs <paramref name="function"/> on a thread of <see cref="Stack"/> bytes, and says what it answered
	/// or threw.
	/// </summary>
	static (T Answer, Exception? Thrown) OnStack<T>(Func<T> function)
	{
		T          answer = default!;
		Exception? caught = null;

		var thread = new Thread(() =>
		{
			try
			{
				answer = function();
			}
			catch (Exception exception)
			{
				caught = exception;
			}
		}, Stack);

		thread.Start();
		thread.Join();

		return (answer, caught);
	}
}
