using System;
using System.Diagnostics;
using System.Linq;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What reading a text of blocks costs, held where it stands today so that it cannot quietly get
/// worse.
/// </summary>
/// <remarks>
/// <para>
/// This is a ratchet and not a proof: the bookkeeping a reading writes about names and blocks is
/// flat in the blocks (<see cref="BlockScalingTests"/>), and what is left above linear here is the
/// generated machinery's. Measured on 2026-09-26, tiering off, 25 to 800 sibling blocks, both
/// readers of this language in one process: the hand-written one is flat at 3.45 µs a block
/// (3.47 at 25, 3.45 at 800) and the generated one rises from 4.78 to 22.56, smoothly and with no
/// cliff, while allocation stays at ~4.6 KB a block and nothing is collected at any size. A text of
/// the same length with no block in it is flat, 2.96 µs a statement at 25 against 3.01 at 800, so
/// it is the blocks and not the length.
/// </para>
/// <para>
/// So every bound here is 2.0, the square, and nothing tighter. A time on a shared runner cannot
/// tell a defect from the runner at any bound below that: the state's own gate timed shapes whose
/// count is flat and read 1.69 and 1.36 on CI (36230002599, Linux, 2026-09-26), and over two runs
/// of the whole slow suite here the shape with NO blocks, which is linear, read 0.99 and then 1.26.
/// A bound of 1.6 would have caught the runner about as often as a regression.
/// </para>
/// <para>
/// What this leaves is a ratchet against a blow-up, which is all a time can be. The gate for the
/// reader is a count of what it does per block, as <see cref="BlockScalingTests"/> counts the
/// state's places, and it belongs with the machinery: it comes with the reader's own fix. Where
/// these rows stand today, for whoever takes that on: 1.35 at these two sizes in a quiet process
/// with tiering off, 1.19 to 1.41 over runs of the whole suite, and 1.45 over 25 to 800 blocks.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class ReaderScalingTests
{
	[Theory]
	[InlineData("the same name in every block", "{{ var i = {0}; System.Math.Abs(i); }}", 2.0)]
	[InlineData("a name apiece", "{{ var v{0} = {0}; System.Math.Abs(v{0}); }}", 2.0)]
	[InlineData("a `for` loop", "for (var i = 0; i < {0}; i++) System.Math.Abs(i);", 2.0)]
	[InlineData("no block at all", "System.Math.Abs(x + {0});", 2.0)]
	public void Four_times_the_blocks_does_not_cost_sixteen(string what, string block, double bound)
	{
		var shorter = Best(50, block);
		var longer  = Best(200, block);

		var exponent = Math.Log(longer / shorter) / Math.Log(4.0);

		Assert.True(
			exponent <= bound,
			$"With {what}, four times the blocks took {longer / shorter:F1} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs), an exponent of {exponent:F2} against {bound:F2}.");
	}

	/// <summary>The fastest of several readings of a text of that many blocks, in microseconds.</summary>
	static double Best(int blocks, string block)
	{
		var text = "(int x) => { " +
			string.Concat(Enumerable.Range(0, blocks).Select(one => string.Format(block, one) + " ")) + "x }";

		Assert.True(ExpressionParser.TryParse(text, typeof(ReaderScalingTests).Assembly).IsSuccess, text);

		GC.Collect();
		GC.WaitForPendingFinalizers();

		var best = double.MaxValue;

		for (var run = 0; run < 7; run++)
		{
			var watch = Stopwatch.StartNew();

			ExpressionParser.TryParse(text, typeof(ReaderScalingTests).Assembly);

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}
}
