using System;
using System.Text;
using System.Threading;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// A tree of any depth a parser reads is read, written and walked on a thread of an ordinary size:
/// a reading ends in a tree or a refusal, and a walk of the tree in its answer, never in a stack
/// overflow.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why a thread of a megabyte.</b> It is what a thread is given unless somebody asks for more, and
/// a host that parses text it was sent does it on a thread like that. The generated readers carry a
/// reading onto a stack of their own when theirs runs low; whatever walks the tree afterwards by
/// recursion has to do the same.
/// </para>
/// <para>
/// <b>What a failure looks like.</b> A stack overflow cannot be caught: it ends the test process, and
/// the run is red because it never finished, not because an assertion said so. These are the two
/// shapes it was found in: ten thousand <c>CASE … ELSE CASE …</c> ended the process that read them,
/// because the reader probed its stack once in four entries and that level enters a probing rule four
/// times, so the probe fell on the same entries every level and never on the <c>ELSE</c>; and a chain
/// of ten thousand <c>+</c>, which every parser reads, ended the process that wrote it. Every
/// other shape is swept in <c>DeepTreeSweepTests</c>, and trees of a hundred thousand nodes and
/// more are written in <c>DeepTreeHandOffTests</c> (both DotGram.Tests.Slow).
/// </para>
/// </remarks>
public sealed class DeepTreeTests
{
	const int Depth = 10_000;

	/// <summary>The size of thread a host gets without asking.</summary>
	const int Stack = 1024 * 1024;

	[Fact]
	public void Ten_thousand_CASE_in_ELSE_are_read_and_written()
	{
		var text = Nested("SELECT ", "CASE WHEN 1 = 1 THEN 1 ELSE ", "1", " END", "", Depth);

		var (written, thrown) = OnStack(() =>
		{
			Assert.True(TransactSqlParser.TryParseStatement(text, 0).IsSuccess, "the positional reading refused it");
			Assert.True(TransactSqlParser.Located.TryParseStatement(text).IsSuccess, "the located reading refused it");
			Assert.True(TransactSqlParser.TryParseScript(text).IsSuccess, "the script reading refused it");
			Assert.True(Sql92Parser.TryParseSelect(text + " FROM t").IsSuccess, "SQL-92 refused it");

			return RoundTrip(text, TransactSqlParser.ParseStatement, SqlWriter.Write);
		});

		Assert.Null(thrown);
		Assert.NotEmpty(written);
	}

	/// <summary>
	/// A chain of twenty thousand <c>+</c> is written by the writer of each tree: the writers recursed a
	/// level a node and overflowed at about ten thousand. A hundred thousand is in
	/// <c>DeepTreeHandOffTests</c> (DotGram.Tests.Slow).
	/// </summary>
	[Fact]
	public void A_chain_of_twenty_thousand_is_written()
	{
		var chain = Nested("SELECT ", "1 + ", "1", "", "", 20_000);
		var from  = chain + " FROM t";

		var (written, thrown) = OnStack(() =>
		{
			var transact = SqlWriter.Write(TransactSqlParser.ParseStatement(chain));
			var standard = Ast.Sql2023Writer.Write(SqlStandardParser.ParseQueryExpression(from));
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
	/// The two cycles of the SQL:2023 writer that its checks missed, each reached through a method
	/// handed to <c>Each</c> rather than called: a grouping set inside a grouping set, and a JSON
	/// table's <c>NESTED PATH</c> inside another. <c>WriterRecursionTests</c> is what finds the next one.
	/// </summary>
	[Theory]
	[InlineData("GROUPING SETS", "SELECT a FROM t GROUP BY ", "GROUPING SETS (", "a", ")", "")]
	[InlineData("NESTED PATH",   "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (", "NESTED PATH '$' COLUMNS (", "a INTEGER PATH '$'", ")", ")) AS j")]
	public void Sql2023_writes_any_depth_of(string shape, string prefix, string open, string middle, string close, string suffix)
	{
		var text = Nested(prefix, open, middle, close, suffix, Depth);

		var (written, thrown) = OnStack(() => RoundTrip(text, SqlStandardParser.ParseQueryExpression, Ast.Sql2023Writer.Write));

		Assert.True(thrown is null, $"{shape}: {thrown}");
		Assert.NotEmpty(written);
	}

	/// <summary>
	/// What a walk throws on a stack it was handed to is thrown to the caller as itself, and the thread
	/// that asked can hand off again afterwards. Twenty thousand levels is past a megabyte and no more:
	/// an exception unwinds every frame it passes, and through a million it takes most of a minute.
	/// </summary>
	[Fact]
	public void An_exception_on_a_stack_handed_to_reaches_the_caller()
	{
		Ast.Expression made = new Unwritable();

		for (var i = 0; i < 20_000; i++)
			made = new Ast.Expression.Parenthesized(made);

		var deep = Nested("SELECT ", "1 + ", "1", "", "", 100_000);

		var (written, thrown) = OnStack(() =>
		{
			var caught = Assert.Throws<NotSupportedException>(() => Ast.Sql2023Writer.Write(made));

			Assert.Contains(nameof(Unwritable), caught.Message, StringComparison.Ordinal);

			// Thrown again by the thread that handed the walk over, which is what says it crossed.
			Assert.Contains("SqlStack", caught.StackTrace, StringComparison.Ordinal);

			return SqlWriter.Write(TransactSqlParser.ParseStatement(deep));
		});

		Assert.Null(thrown);
		Assert.Equal(deep, written);
	}

	/// <summary>A node no grammar builds, which the SQL:2023 writer refuses.</summary>
	sealed record Unwritable : Ast.Expression;

	/// <summary>
	/// A walk and a hash of a deep tree end: the walk with every node, and the hash with its value or,
	/// where its stack runs out, a catchable exception.
	/// </summary>
	[Fact]
	public void A_deep_tree_is_walked_and_hashed_without_a_crash()
	{
		var text = Nested("SELECT ", "CASE WHEN 1 = 1 THEN 1 ELSE ", "1", " END", "", Depth);

		var (nodes, thrown) = OnStack(() =>
		{
			var tree  = TransactSqlParser.ParseStatement(text);
			var count = 0;

			SqlWalker.Walk(tree, _ =>
			{
				count++;

				return true;
			});

			try
			{
				tree.GetHashCode();
			}
			catch (InsufficientExecutionStackException)
			{
				// What a hash deeper than the stack answers (TreeEqualityTests); whether this one is
				// depends on the frames the JIT gave it.
			}

			return count;
		});

		Assert.Null(thrown);
		Assert.True(nodes > 3 * Depth, $"{nodes} nodes walked");
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
