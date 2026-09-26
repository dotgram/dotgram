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
/// This is a ratchet and not a proof, and what it asserts is the EXPONENT: four times the blocks
/// must not cost much more than four times as long. The bookkeeping a reading writes about names and
/// blocks is flat in the blocks (<see cref="BlockScalingTests"/>), and since b09d95aa the generated
/// machinery is flat in them too.
/// </para>
/// <para>
/// Measured on b09d95aa (2026-09-26), 25 to 800 sibling blocks, both readers of this language in one
/// process, tiering and PGO off, fastest of seven readings a size — microseconds a block at 25
/// against 800:
/// </para>
/// <para>
/// the same name in every block, generated 4.52 to 4.47 and hand-written 3.70 to 3.53; a name
/// apiece, 4.60 to 4.46 against 3.53 to 3.52; a `for` loop, 6.37 to 6.14 against 8.84 to 4.54; no
/// block at all, 3.21 to 3.12 against 2.67 to 2.57. <b>The per-block cost does not rise at all</b>,
/// where before this the generated reader went from 4.78 to 22.56 over the same range, an exponent of
/// 1.45. It is about 1.27x the hand-written parser now, flat, rather than a curve.
/// </para>
/// <para>
/// What changed was the materialising walk, which found the marks standing over its start by
/// replaying every open and close from the log's start, so a guard near the end of a long log paid
/// for the whole front of it: 512 steps a block at 25 blocks and 8,387 at 400, x4.03 a doubling,
/// against a flat forty records listed. The reader keeps the open marks now and the walk reads a
/// prefix of them.
/// </para>
/// <para>
/// <b>Every bound here is 2.0 all the same, and the reason is the runner.</b> I lowered the three
/// block shapes to 1.3 on the strength of those readings and CI went red on the next push: "a name
/// apiece" read 1.79 (2,372 µs against 28,205) and the `for` loop 1.33, on Linux, run 36250316609.
/// The per-block cost there is about 47 µs against this machine's 4.6, so the runner is an order of
/// magnitude slower and its spread is worse than the effect any tighter bound would catch. This
/// file's predecessor had already failed at 1.69 on CI (36230002599) and its own text said a time on
/// a shared runner cannot tell a defect from the runner below 2.0 — I replaced that paragraph with
/// the 1.3 argument and then met exactly what it had warned about.
/// <para>
/// So the flatness above is a real property measured on a quiet machine, and it is NOT what this file
/// asserts. What a time can assert is a blow-up, which is 2.0. The gate for flatness is a count, as
/// <see cref="BlockScalingTests"/> counts the state's places, and it is now buildable: D144 settles
/// that the counters are emitted inside <c>#if DOTGRAM_COUNTS</c> in the support text, that no
/// project defines the symbol, and that a counting gate compiles its own parser from the same grammar
/// with it set — which counts what the shipped parser would, being the same grammar and generator.
/// Until that lands, nothing here gates the exponent and the paragraph above is the record of where
/// it stood.
/// </para>
/// <para>
/// For whoever takes that on, the quiet readings: three of them held the exponents between 0.90 and
/// 1.16 while the ABSOLUTE per-block times moved by seventy per cent on the same build — one read
/// 7.92 µs a block where another read 4.63 — which is why a count and not a clock. One reading gave
/// the `for` shape 0.60, which is not a result either: its 50-block row read 522 µs where every other
/// size fits about 6.1, and two further readings gave that shape 1.01 and 0.91.
/// </para>
/// <para>
/// <b>`no block at all` stays at 2.0, because its job is to be believed.</b> It is linear by
/// construction and cannot be made superlinear by a change to the machinery, so a failure of it is a
/// statement about the run and not about the reader — and a row that fails when the machine is noisy
/// is a row nobody reads twice. It moved with the other three between runs, which is how the third
/// reading was known to be the machine.
/// </para>
/// <para>
/// <b>A count gate is still owed, and is now buildable.</b> The marks figures above were taken with
/// counters emitted onto the generated <c>Ways</c> and then removed, because a consumer should not
/// carry them; <c>BlockScalingTests</c> can read <c>State.Places</c> only because the state is
/// written by hand. D144 settles the shape: the counters are emitted inside
/// <c>#if DOTGRAM_COUNTS</c> in the support text, no project defines the symbol, and a counting gate
/// compiles its own parser from the same grammar with it set — which counts what the shipped parser
/// would, being the same grammar and the same generator.
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
