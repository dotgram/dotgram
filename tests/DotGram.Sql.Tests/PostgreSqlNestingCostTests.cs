using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using DotGram.Sql.PostgreSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// What grows costs what its size does, never a power of it: a dollar-quoted body twice as long and a comment
/// nested twice as deep each allocate at most a bounded multiple of the one before, and each is read long before a
/// doubling reading could finish.
/// </summary>
/// <remarks>
/// <para>
/// The shapes are PostgreSQL's own. A dollar quote is closed by the tag it opened with, matched again at every
/// <c>$</c> inside the body (docs/syntax.md §3.5.1), so a body of <c>$</c> signs is the costliest; a comment holds
/// others to any depth, where T-SQL's stop at six, and the server's lexer counts them with an integer, so a hundred
/// thousand of them are read here too, through every door. The depths and lengths go up in order and the first one
/// out of bounds ends the test, so a defect fails it rather than hanging it.
/// </para>
/// </remarks>
public sealed class PostgreSqlNestingCostTests
{
	/// <summary>How much more a reading four times as big may allocate.</summary>
	const double Slack = 8;

	/// <summary>How long a reading of any size here may take before it is taken for a blow-up.</summary>
	static readonly TimeSpan Watchdog = TimeSpan.FromSeconds(10);

	[Theory]
	[InlineData("dollar quote of dollars, closed",   "$t$", "$", "$t$",  true)]
	[InlineData("dollar quote of dollars, unclosed", "$t$", "$", "$t",   false)]
	[InlineData("dollar quote of other tags, closed", "$t$", "$x", "$t$", true)]
	[InlineData("dollar quote of text, closed",      "$t$", "a", "$t$",  true)]
	[InlineData("dollar quote of a long tag, near misses in the body", "$abcdefghij$", "$abcdefghi$", "$abcdefghij$", true)]
	[InlineData("comments nested, closed",           "",    "/*", "*/",  true)]
	[InlineData("comments nested, unclosed",         "",    "/*", "",    false)]
	public void A_run_costs_what_its_length_does(string what, string head, string unit, string tail, bool accepted)
	{
		// The shape once at a small size and on this thread, so that the first size measured is not the one that
		// pays for compiling the reader.
		PostgreSqlParser.TryParseExpression(Text(head, unit, tail, 4, accepted));

		var bytes = new List<long>();

		foreach (var size in new[] { 1 << 10, 1 << 12, 1 << 14, 1 << 16 })
		{
			var text = Text(head, unit, tail, size, accepted);
			var (finished, read, allocated) = Read(text);

			Assert.True(
				finished,
				$"{what}: {size} units were not read in {Watchdog.TotalSeconds:F0} s, where the sizes before allocated " +
				$"{string.Join(", ", bytes.Select(static one => one.ToString("N0")))} bytes.");
			Assert.Equal(accepted, read);

			if (bytes.Count > 0)
			{
				Assert.True(
					allocated <= bytes[^1] * Slack,
					$"{what}: {size / 4} units allocated {bytes[^1]:N0} bytes and {size} allocated {allocated:N0}, " +
					$"{(double)allocated / bytes[^1]:F1} times as much. Four times the size may cost a constant or a " +
					"multiple of it, never a power of it.");
			}

			bytes.Add(allocated);
		}
	}

	/// <summary>A comment a hundred thousand deep is read through every door: plain and located.</summary>
	[Fact]
	public void A_comment_nested_a_hundred_thousand_deep_is_read_through_every_door()
	{
		const int depth = 100_000;

		var text = string.Concat(Enumerable.Repeat("/*", depth)) + " 1 " + string.Concat(Enumerable.Repeat("*/", depth)) + " 1";

		Assert.True(PostgreSqlParser.TryParseExpression(text).IsSuccess);
		Assert.True(PostgreSqlParser.Located.TryParseExpression(text).IsSuccess);
		Assert.False(PostgreSqlParser.TryParseExpression(text[..^(2 * depth + 3)]).IsSuccess);
	}

	/// <summary>The text: the head, the unit as often as asked, the tail, and a value where the whole is a comment.</summary>
	static string Text(string head, string unit, string tail, int count, bool accepted)
	{
		var text = new StringBuilder(head);

		for (var i = 0; i < count; i++)
			text.Append(unit);

		if (unit == "/*")
		{
			text.Append(" 1 ");

			for (var i = 0; i < count && accepted; i++)
				text.Append(tail);

			return text.Append(" 1").ToString();
		}

		return text.Append(tail).ToString();
	}

	/// <summary>
	/// Reads the text on a thread of its own, so that the parser's stores start empty; whether it finished within the
	/// watchdog, whether it was accepted, and the bytes the thread allocated.
	/// </summary>
	static (bool Finished, bool Read, long Allocated) Read(string text)
	{
		var read      = false;
		var allocated = 0L;
		var thread    = new Thread(
			() =>
			{
				var before = GC.GetAllocatedBytesForCurrentThread();

				read      = PostgreSqlParser.TryParseExpression(text).IsSuccess;
				allocated = GC.GetAllocatedBytesForCurrentThread() - before;
			},
			64 << 20)
		{
			// A reading the watchdog gave up on runs on until the process ends; it must not keep it alive.
			IsBackground = true,
		};

		thread.Start();

		var finished = thread.Join(Watchdog);

		return (finished, read, allocated);
	}
}
