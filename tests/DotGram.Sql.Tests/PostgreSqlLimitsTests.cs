using System;
using System.Threading;

using DotGram.Sql.PostgreSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <see cref="PostgreSqlParser.Limits"/> is the one setting of both doors: the located reading is a compilation of its
/// own, and reads the limits of the class it is nested in.
/// </summary>
/// <remarks>
/// Run alone: the limits are the process's, and a PostgreSQL test beside these would read them.
/// </remarks>
[Collection(typeof(Alone))]
public sealed class PostgreSqlLimitsTests
{
	/// <summary>
	/// The collection xunit runs after every parallel one, with nothing beside it.
	/// </summary>
	[CollectionDefinition(DisableParallelization = true)]
	public sealed class Alone;

	/// <summary>
	/// An expression nested past what the stack it is read on holds is read where nothing limits the extra stacks,
	/// and refused with <see cref="InsufficientExecutionStackException"/> by either door where the limits allow none.
	/// </summary>
	[Fact]
	public void The_limits_of_the_parser_bound_the_located_reading()
	{
		const int depth = 20_000;

		var text = new string('(', depth) + "1" + new string(')', depth);

		Assert.Same(PostgreSqlParser.ParserLimits.Default, PostgreSqlParser.Limits);
		Assert.Null(Read(() => PostgreSqlParser.Located.ParseExpression(text)));

		try
		{
			PostgreSqlParser.Limits = PostgreSqlParser.Limits.WithMaxExtraStacks(1);

			Assert.IsType<InsufficientExecutionStackException>(Read(() => PostgreSqlParser.ParseExpression(text)));
			Assert.IsType<InsufficientExecutionStackException>(Read(() => PostgreSqlParser.Located.ParseExpression(text)));
		}
		finally
		{
			PostgreSqlParser.Limits = PostgreSqlParser.ParserLimits.Default;
		}

		Assert.Null(Read(() => PostgreSqlParser.Located.ParseExpression(text)));
	}

	/// <summary>Reads on a thread whose stack runs low early, so that the reading moves; what it threw, or null.</summary>
	static Exception? Read(Action read)
	{
		Exception? thrown = null;

		var thread = new Thread(() =>
		{
			try
			{
				read();
			}
			catch (Exception caught)
			{
				thrown = caught;
			}
		}, 256 * 1024);

		thread.Start();
		thread.Join();

		return thrown;
	}
}
