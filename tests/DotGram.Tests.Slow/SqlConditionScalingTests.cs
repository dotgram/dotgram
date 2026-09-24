using System;
using System.Linq;

using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every predicate of a long SQL:2023 condition allocates what one predicate needs, not a
/// share of the whole store again.
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
/// The bound leaves room for noise and none for the second: eight kilobytes a predicate
/// against the one and a half they take.
/// </para>
/// <para>
/// <b>The first defect is no longer guarded here, and that is a deliberate loss.</b> Two timed
/// tests used to hold ten times the predicates to about ten times the time, one over a search
/// condition and one over a sum, and they caught the walk of the log per element. They were
/// removed on 2026-09-24: they measured the clock, they were green here and red on CI, and a
/// check that fails on the machine rather than on the code teaches people to ignore it.
/// Allocation cannot replace them — a walk allocates nothing, which is exactly why the second
/// defect is still caught below and the first is not. What is left holds only what it can hold
/// honestly. If the walk comes back, nothing in this suite will say so.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class SqlConditionScalingTests
{
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

	static string Condition(int predicates, string connective)
	{
		return string.Join(connective, Enumerable.Range(0, predicates).Select(static i => "a" + i + " = 1"));
	}

}
