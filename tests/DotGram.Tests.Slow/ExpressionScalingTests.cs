using System;
using System.Diagnostics;
using System.Linq;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A long expression costs the same per term however many there are, on the tape as well as on
/// the immediate carrier.
/// </summary>
/// <remarks>
/// <para>
/// The expression language places marks (`with state`), and a guard builds the few values it
/// asks for from the log as the text is read — `NamedType`'s, at every name, which is every
/// term of `x + x + …`. The walk that builds them first rebuilt the marks standing over its
/// start by reading the log from the beginning, whether or not anything it built was handed
/// them: a walk of the whole log per term, the tape's time growing with the square of the
/// input (1.46 from a hundred terms to a thousand, 1,322 µs against the immediate carrier's
/// 116). It reads them now only where a record it may build is handed them.
/// </para>
/// <para>
/// The bound leaves room for noise and none for the square: fifteen against ten. Timed alone,
/// the best of several runs.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class ExpressionScalingTests
{
	[Fact]
	public void A_sum_reads_in_time_linear_in_its_terms()
	{
		var shorter = Best(() => ExpressionParser.TryParse(Sum(200)).IsSuccess);
		var longer  = Best(() => ExpressionParser.TryParse(Sum(2_000)).IsSuccess);

		Assert.True(longer / shorter < 15, $"Ten times the terms took {longer / shorter:F1} times as long ({shorter:F0} µs against {longer:F0} µs).");
	}

	/// <summary>The fastest of several reads, in microseconds, after one to compile it.</summary>
	static double Best(Func<bool> read)
	{
		Assert.True(read());

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

	static string Sum(int terms)
	{
		return "(int x) => x" + string.Concat(Enumerable.Repeat(" + x", terms));
	}
}
