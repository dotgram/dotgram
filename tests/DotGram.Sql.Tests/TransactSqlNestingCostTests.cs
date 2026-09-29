using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// A nest costs what its depth does, never a power of two of it: four depths a doubling apart, each
/// allocating at most a bounded multiple of the one before and each finished long before a doubling
/// reading could finish.
/// </summary>
/// <remarks>
/// <para>
/// <b>The shapes are the two where one level was read twice</b>, so a nest of n was read 2^n times:
/// 24 levels took seconds, and every level more doubled it. A bare <c>CAST(</c> that <c>TSqlCast</c>
/// refused was read again as a call to a function named <c>CAST</c> (<c>TRY_CAST</c>, <c>PARSE</c> and
/// <c>TRY_PARSE</c> alike), and a predicate read its row once for its tails and again for
/// <c>IS DISTINCT FROM</c>, which a <c>CASE</c> nested in a condition reaches at every level — refused
/// when unclosed, and ACCEPTED where each level is itself an <c>IS DISTINCT FROM</c>.
/// </para>
/// <para>
/// <b>Two witnesses, because one of the blow-ups allocates and the other does not.</b> Each depth is
/// read on a thread of its own, so the parser's stores start empty and the bytes it allocates are the
/// stores it had to grow: the second reading of a <c>CAST</c> operand left records behind, and 16
/// levels allocated 6.3 MB where 8 allocated 36 KB. A doubling of the depth may cost a constant or a
/// multiple of it, and 8 is twice the worst a store growing by doubling can show. The unclosed
/// <c>CASE</c> nest allocated 11 KB at 8 levels and at 16 while its time doubled a level, so for it
/// the witness is the watchdog: 32 levels of it were hours of reading, and read once they take well
/// under a millisecond, so ten seconds cannot fail on a loaded machine and cannot pass on the defect.
/// The depths go up in order and the first one out of bounds ends the test, so a defect fails it
/// rather than hanging it.
/// </para>
/// <para>
/// No count of rules entered is available to a test: the emitted counters are per machine, never per
/// rule (D144). The counts the fixes were measured by — rule entries and loop turns of an instrumented
/// copy, x2.00 a level before and an increment exponent of 1.00 after — are in the commit messages.
/// </para>
/// </remarks>
public sealed class TransactSqlNestingCostTests
{
	/// <summary>How much more a nest twice as deep may allocate.</summary>
	const double Slack = 8;

	/// <summary>How long a reading of any depth here may take before it is taken for a blow-up.</summary>
	static readonly TimeSpan Watchdog = TimeSpan.FromSeconds(10);

	static readonly int[] Depths = [8, 16, 32, 64];

	[Theory]
	[InlineData("CAST, unclosed",      "SELECT ", "CAST(",      "a", "",         false)]
	[InlineData("CAST, a bad type",    "SELECT ", "CAST(",      "a", " AS )",    false)]
	[InlineData("TRY_CAST, unclosed",  "SELECT ", "TRY_CAST(",  "a", "",         false)]
	[InlineData("PARSE, unclosed",     "SELECT ", "PARSE(",     "a", "",         false)]
	[InlineData("TRY_PARSE, unclosed", "SELECT ", "TRY_PARSE(", "a", "",         false)]
	[InlineData("CAST, closed",        "SELECT ", "CAST(",      "a", " AS INT)", true)]
	[InlineData("CASE in a condition, unclosed", "SELECT 1 WHERE ", "CASE WHEN ", "a = 1", "", false)]
	[InlineData("CASE in a condition, closed", "SELECT 1 WHERE ", "CASE WHEN ", "a = 1", " THEN 1 END = 1", true)]
	[InlineData(
		"CASE in a condition, IS DISTINCT FROM at every level",
		"SELECT 1 WHERE ", "CASE WHEN ", "a IS DISTINCT FROM b", " THEN 1 END IS DISTINCT FROM 1", true)]
	[InlineData(
		"CASE in a condition, IS NOT DISTINCT FROM at every level",
		"SELECT 1 WHERE ", "CASE WHEN ", "a IS NOT DISTINCT FROM b", " THEN 1 END IS NOT DISTINCT FROM 1", true)]
	public void A_nest_costs_what_its_depth_does(string what, string head, string opener, string core, string closer, bool accepted)
	{
		// The shape once at a small depth and on this thread, so that the first depth measured is not
		// the one that pays for compiling the reader.
		TransactSqlParser.TryParseStatement(Text(head, opener, core, closer, 2));

		var bytes = new List<long>();

		foreach (var depth in Depths)
		{
			var text = Text(head, opener, core, closer, depth);
			var (finished, read, allocated) = Read(text);

			Assert.True(
				finished,
				$"{what}: {depth} levels were not read in {Watchdog.TotalSeconds:F0} s, where the depths before " +
				$"allocated {string.Join(", ", bytes.Select(static one => one.ToString("N0")))} bytes. Read once a level, " +
				"they take under a millisecond.");
			Assert.Equal(accepted, read);

			if (bytes.Count > 0)
			{
				Assert.True(
					allocated <= bytes[^1] * Slack,
					$"{what}: {depth / 2} levels allocated {bytes[^1]:N0} bytes and {depth} allocated {allocated:N0}, " +
					$"{(double)allocated / bytes[^1]:F1} times as much. Doubling the depth may cost a constant or a " +
					"multiple of it, never a power of it.");
			}

			bytes.Add(allocated);
		}
	}

	/// <summary>
	/// Reads the text on a thread of its own, so that the parser's stores start empty; whether it
	/// finished within the watchdog, whether it was accepted, and the bytes the thread allocated.
	/// </summary>
	static (bool Finished, bool Read, long Allocated) Read(string text)
	{
		var read      = false;
		var allocated = 0L;
		var thread    = new Thread(
			() =>
			{
				var before = GC.GetAllocatedBytesForCurrentThread();

				read      = TransactSqlParser.TryParseStatement(text).IsSuccess;
				allocated = GC.GetAllocatedBytesForCurrentThread() - before;
			},
			16 << 20)
		{
			// A reading the watchdog gave up on runs on until the process ends; it must not keep it alive.
			IsBackground = true,
		};

		thread.Start();

		return thread.Join(Watchdog) ? (true, read, allocated) : (false, false, 0);
	}

	static string Text(string head, string opener, string core, string closer, int depth)
	{
		return head + string.Concat(Enumerable.Repeat(opener, depth)) + core + string.Concat(Enumerable.Repeat(closer, depth));
	}
}
