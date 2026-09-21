using System;
using System.Diagnostics;
using System.Linq;

using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A long SQL:2023 condition costs the same per predicate however many there are: ten times the
/// predicates take about ten times as long, and every one of them allocates what one predicate
/// needs, not a share of the whole store again.
/// </summary>
/// <remarks>
/// <para>
/// Two defects made these grow faster than the input. The guard that asks a tower whether its
/// operands go together is handed the whole list, and the list was built an element at a time,
/// each walk reading the log from the start of the expression. And the machines whose guards
/// build while they read kept a table per value type as long as the log — three hundred of
/// them — which the thread let go once a parse had made them long, so that every parse after it
/// allocated them all again: 164 MB for a thousand predicates.
/// </para>
/// <para>
/// The bounds leave room for noise and none for either: fifteen against ten for the time, and
/// eight kilobytes a predicate against the one and a half they take. Timed alone, the best of
/// several runs.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class SqlConditionScalingTests
{
	[Theory]
	[InlineData(" AND ")]
	[InlineData(" OR ")]
	public void A_search_condition_reads_in_time_linear_in_its_predicates(string connective)
	{
		AssertLinear(
			Best(() => SqlStandardParser.TryParseSearchCondition(Condition(200, connective)).IsSuccess),
			Best(() => SqlStandardParser.TryParseSearchCondition(Condition(2_000, connective)).IsSuccess));
	}

	[Fact]
	public void And_a_sum_in_its_terms()
	{
		AssertLinear(
			Best(() => SqlStandardParser.TryParseValueExpression(Sum(200)).IsSuccess),
			Best(() => SqlStandardParser.TryParseValueExpression(Sum(2_000)).IsSuccess));
	}

	[Fact]
	public void Each_predicate_allocates_what_it_needs_and_no_more()
	{
		var text = Condition(2_000, " AND ");

		// Twice first: the thread's store is rented and grown by the first parse, and kept for
		// the next one only where it is not oversized.
		Assert.True(SqlStandardParser.TryParseSearchCondition(text).IsSuccess);
		Assert.True(SqlStandardParser.TryParseSearchCondition(text).IsSuccess);

		var before = GC.GetAllocatedBytesForCurrentThread();

		SqlStandardParser.TryParseSearchCondition(text);

		var each = (GC.GetAllocatedBytesForCurrentThread() - before) / 2_000;

		Assert.True(each < 8 * 1024, $"A predicate allocated {each:N0} bytes.");
	}

	static void AssertLinear(double shorter, double longer)
	{
		Assert.True(longer / shorter < 15, $"Ten times the input took {longer / shorter:F1} times as long ({shorter:F0} µs against {longer:F0} µs).");
	}

	/// <summary>The fastest of several reads, in microseconds, after one to compile it.</summary>
	static double Best(Func<bool> read)
	{
		Assert.True(read());

		// A heavy test before this one leaves the collector with work that is not this parse's.
		GC.Collect();
		GC.WaitForPendingFinalizers();

		var best = double.MaxValue;

		for (var run = 0; run < 11; run++)
		{
			var watch = Stopwatch.StartNew();

			read();

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}

	static string Condition(int predicates, string connective)
	{
		return string.Join(connective, Enumerable.Range(0, predicates).Select(static i => "a" + i + " = 1"));
	}

	static string Sum(int terms)
	{
		return string.Join(" + ", Enumerable.Range(0, terms).Select(static i => "a" + i));
	}
}
