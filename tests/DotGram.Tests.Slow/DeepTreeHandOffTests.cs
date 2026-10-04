using System;
using System.Text;
using System.Threading;

using DotGram.Sql;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Trees deeper than one stack a walk is handed to, written whole on a thread of an ordinary size.
/// </summary>
/// <remarks>
/// Here rather than beside <c>DeepTreeTests</c> in DotGram.Sql.Tests, and alone: a chain of hundreds of
/// thousands of nodes alive while other tests allocate slows every collection they set off, and in that
/// project these two rows took the run from twelve seconds to more than a minute.
/// </remarks>
[Collection(typeof(Alone))]
public sealed class DeepTreeHandOffTests
{
	/// <summary>The size of thread a host gets without asking.</summary>
	const int Stack = 1024 * 1024;

	/// <summary>
	/// A chain of a hundred thousand <c>+</c>, which every parser reads, is written by the writer of
	/// its tree: the writer recursed a level a node and overflowed at about ten thousand.
	/// </summary>
	[Fact]
	public void A_chain_of_a_hundred_thousand_is_written()
	{
		var chain = Nested("SELECT ", "1 + ", "1", "", "", 100_000);
		var from  = chain + " FROM t";

		var (written, thrown) = OnStack(() =>
		{
			var transact = SqlWriter.Write(TransactSqlParser.ParseStatement(chain));
			var standard = DotGram.Sql.Ast.Sql2023Writer.Write(SqlStandardParser.ParseQueryExpression(from));
			var sql92    = SqlWriter.Write(Sql92Parser.ParseSelect(from));

			Assert.Equal(chain, transact);
			Assert.Equal(from, standard);
			Assert.Equal(from, sql92);

			return transact;
		});

		Assert.Null(thrown);
		Assert.NotEmpty(written);
	}

	/// <summary>
	/// A tree too deep for the stack a walk is handed to is handed on again from there, as many times as
	/// it takes, and written whole: half a million brackets is 32 bytes a level of a 16 MiB stack, less
	/// than the return address and the frame pointer and one argument of a call take.
	/// </summary>
	[Fact]
	public void A_walk_handed_on_is_handed_on_again_where_that_stack_runs_low()
	{
		const int depth = 500_000;

		Expression made = new Expression.Literal(SqlLiteralKind.Number, "1");

		for (var i = 0; i < depth; i++)
			made = new Expression.Parenthesized(made);

		var (written, thrown) = OnStack(() => SqlWriter.Write(made));

		Assert.Null(thrown);
		Assert.Equal(new string('(', depth) + "1" + new string(')', depth), written);
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
