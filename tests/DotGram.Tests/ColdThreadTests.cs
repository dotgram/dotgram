using System;
using System.Threading;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A parser's first reading on a thread answers what it answers on a warmed one.
/// </summary>
/// <remarks>
/// <para>
/// <b>Every store a parser keeps is thread-static, and none of them shrinks.</b> So a thread that has
/// read something large carries arrays long enough to absorb an index no reading should produce, and a
/// thread that has read nothing does not. That is not a property anyone designed; it is what made a
/// crash in the materialising walk invisible to a whole test suite and to CI, for at least the eleven
/// days from v0.1.0, because every suite warms its thread before the input that reaches the defect.
/// </para>
/// <para>
/// The defect: a reference to a record is <c>ways.Last</c> at the moment it is pushed, and a give-back
/// restored the log, the record count, and the built watermarks — but not <c>Last</c>. So an abandoned
/// reading left <c>Last</c> naming a record that had been rolled back, the next push stored that number
/// as a reference, and the walk followed it. `ways.Records` at 14 while `Last` held 18. On a cold
/// thread that threw <c>IndexOutOfRangeException</c>; on a warm one it marked an unrelated record as
/// reached and said nothing.
/// </para>
/// <para>
/// <b>These run on a new thread on purpose, and each one is its own thread.</b> A test that shares the
/// suite's thread is warmed by whatever ran before it, which is the condition that hid this. One
/// reading per family, chosen to be the shape a consumer's first call would be rather than anything
/// contrived: whatever the family's own tests read most.
/// </para>
/// </remarks>
public sealed class ColdThreadTests
{
	/// <summary>The input that found it: a condition whose second operand is a value, not a condition.</summary>
	/// <remarks>
	/// It is REFUSED, and the refusal is the right answer. What was wrong was the route to it. The
	/// assertion is therefore that the parser answers at all and answers no — a cold thread used to
	/// throw here, and a warm one reached the same no through a reference to a record that did not
	/// exist.
	/// </remarks>
	[Fact]
	public void A_first_reading_of_a_condition_that_is_a_value_refuses_rather_than_throwing()
	{
		Assert.False(OnANewThread(static () =>
			DotGram.Sql.TransactSql.TransactSqlParser.TryParseSelect(
				"SELECT * FROM t WHERE c = 1 AND dbo.f(1)").IsSuccess));
	}

	/// <summary>And the same input on a thread that has already read a larger one.</summary>
	/// <remarks>
	/// The pair is the point: before the fix these two disagreed about whether an exception came out,
	/// while agreeing about the answer where one did come out. A test holding only the warm side is the
	/// suite that missed this.
	/// </remarks>
	[Fact]
	public void A_warmed_thread_answers_the_same_as_a_cold_one()
	{
		var cold = OnANewThread(static () =>
			DotGram.Sql.TransactSql.TransactSqlParser.TryParseSelect(
				"SELECT * FROM t WHERE c = 1 AND dbo.f(1)").IsSuccess);

		var warm = OnANewThread(static () =>
		{
			DotGram.Sql.TransactSql.TransactSqlParser.TryParseSelect(
				"SELECT * FROM t WHERE c = 1 AND d = 2");

			return DotGram.Sql.TransactSql.TransactSqlParser.TryParseSelect(
				"SELECT * FROM t WHERE c = 1 AND dbo.f(1)").IsSuccess;
		});

		Assert.Equal(cold, warm);
	}

	[Fact]
	public void A_first_reading_of_the_standard_answers()
	{
		Assert.True(OnANewThread(static () =>
			DotGram.Sql.Standard.SqlStandardParser.TryParseStatement("SELECT a FROM t WHERE a = 1").IsSuccess));
	}

	/// <summary>A value in brackets and an operator with nothing after it: refused, on a first reading.</summary>
	/// <remarks>
	/// The operator's record is opened and given back when no operand follows, and the guard on the
	/// value before it then built the operator into that value's slot, because the place it built
	/// from was where the last record OPENED began, which a give-back does not put back. The brackets
	/// only make the record's number large enough to be past a fresh thread's tables; without them
	/// (<c>SELECT a +</c>) the same wrong value was read out of a table long enough to hold it, and
	/// nothing threw. GuardMaterializationTests holds that side, where it changes an answer.
	/// </remarks>
	[Fact]
	public void A_first_reading_of_an_operator_with_nothing_after_it_refuses_rather_than_throwing()
	{
		Assert.False(OnANewThread(static () =>
			DotGram.Sql.Standard.SqlStandardParser.TryParseStatement("SELECT (a) +").IsSuccess));
	}

	/// <summary>The same defect in the located T-SQL reader, where it threw on a warm thread as well.</summary>
	/// <remarks>
	/// There the value built into the wrong slot reached a factory, which dereferenced it, so the
	/// NullReferenceException did not depend on what the thread had read before. The unlocated
	/// reader reaches the same state on this input and happens to refuse without touching the value.
	/// </remarks>
	[Fact]
	public void A_located_reading_of_an_unfinished_option_list_refuses_on_any_thread()
	{
		const string unfinished = "CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1,";

		Assert.False(OnANewThread(static () =>
			DotGram.Sql.TransactSql.TransactSqlParser.Located.TryParseScript(unfinished).IsSuccess));

		Assert.False(OnANewThread(static () =>
		{
			DotGram.Sql.TransactSql.TransactSqlParser.Located.TryParseScript(
				"CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1, LISTENER_IP = ALL) FOR TSQL ()");

			return DotGram.Sql.TransactSql.TransactSqlParser.Located.TryParseScript(unfinished).IsSuccess;
		}));
	}

	[Fact]
	public void A_first_reading_of_an_expression_answers()
	{
		Assert.True(OnANewThread(static () =>
			DotGram.ExpressionLanguage.ExpressionParser.TryParse(
				"(int x) => { var i = x; System.Math.Abs(i); return i; }",
				typeof(ColdThreadTests).Assembly).IsSuccess));
	}

	[Fact]
	public void A_first_reading_of_a_url_answers()
	{
		Assert.True(OnANewThread(static () =>
			DotGram.Web.UriReference.TryParse("https://example.org/a/b?c=1#d", out _)));
	}

	/// <summary>Runs it on a thread of its own, so the thread-static stores start empty.</summary>
	/// <remarks>
	/// An exception is carried back rather than swallowed: the whole point is that the cold path used to
	/// throw, so a helper that turned a throw into a false would assert nothing.
	/// </remarks>
	static bool OnANewThread(Func<bool> reading)
	{
		var answer = false;
		Exception? thrown = null;

		var thread = new Thread(() =>
		{
			try
			{
				answer = reading();
			}
			catch (Exception e)
			{
				thrown = e;
			}
		});

		thread.Start();
		thread.Join();

		if (thrown is not null)
			throw new InvalidOperationException("the first reading on a fresh thread threw.", thrown);

		return answer;
	}
}
