using System;
using System.Diagnostics;
using System.Linq.Expressions;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A text of blocks costs its length: four times the blocks, about four times the bookkeeping,
/// and not sixteen times.
/// </summary>
/// <remarks>
/// <para>
/// What a reading writes down about names and blocks (§7.7) is read back by every use of a name
/// and by every block that is built, and each of those readings walked the whole text. Over 200
/// sibling blocks each declaring one name it was 204,230 places read where 50 read 13,580, and
/// 199,400 of those were declarations standing after the use being looked up; a block read every
/// declaration in the text to find its own, 20,300 against 1,275; and recording a block threw
/// away the order of the blocks, so a text of n of them worked it out n times.
/// </para>
/// <para>
/// <b>The gate is the count and not the clock.</b> The property is that the places a reading
/// looks at grow with the text and not with its square, and that is a count: it needs no quiet
/// machine, no window and no tiering switch, and it is the same number on every machine. The
/// first version of this gate timed the same shapes and failed on CI (36230002599, Linux,
/// 2026-09-26) at 1.69 and 1.36 on shapes whose counts are flat — 1.69 being past what a square
/// would have to clear, so no bound could have told the runner from the defect. The time is still
/// read here, bounded where only a blow-up trips it, because a count cannot see a step that grows
/// more expensive without growing more numerous.
/// </para>
/// <para>
/// Written against the state directly, and not against a parse. The state is what was made to
/// cost the text's length, and it is what both readers of this language share; the reading around
/// it is not linear in the blocks yet (<see cref="ReaderScalingTests"/>), and a test over a parse
/// would be asserting that instead. Measured on 2026-09-26, tiering off, 25 to 800 blocks: the
/// state's own work per block is flat, the hand-written reader is flat at 3.45 µs a block, and the
/// generated one rises from 4.78 to 22.56, which is the generated machinery's to answer for and is
/// reported there.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class BlockScalingTests
{
	/// <summary>Four times the blocks, and the places a reading looks at grow by about four.</summary>
	/// <remarks>
	/// <para>
	/// Deterministic, and flat to the digit: 5.00 places a block at 200 blocks and 5.00 at 800, both
	/// shapes. The margin is for rounding and the ends of a text, nothing else.
	/// </para>
	/// <para>
	/// Every defect this was written against multiplies the count rather than nudging it, and each of
	/// them was put back to see this fail on it:
	/// </para>
	/// <list type="bullet">
	/// <item>a use looking back from the END of the declarations: 105.10 places a block at 200
	/// against 404.65 at 800, on the shape where every block declares the same name;</item>
	/// <item>a block reading every declaration in the text: 203.00 against 803.00, both shapes;</item>
	/// <item>recording a block throwing away the order worked out from the blocks: 407.99 against
	/// 1608.00, both shapes.</item>
	/// </list>
	/// <para>
	/// The time assertion below passed on all three of them.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("the same name in every block", true)]
	[InlineData("a name apiece", false)]
	public void Four_times_the_blocks_looks_at_about_four_times_the_places(string what, bool alike)
	{
		var shorter = Places(200, alike) / 200.0;
		var longer  = Places(800, alike) / 800.0;

		Assert.True(
			longer <= shorter * 1.05,
			$"With {what}, a reading looked at {longer:F2} places a block over 800 blocks against " +
			$"{shorter:F2} over 200, which is {longer / shorter:F2} times as many for each.");
	}

	/// <summary>And the time it takes does not blow up, which a count cannot see.</summary>
	/// <remarks>
	/// Bounded at 2.0 — the square — and no tighter, because a shared runner reads well past 1.3 on a
	/// shape that is flat by count (1.69 on CI, above). What this catches is a step that becomes
	/// more expensive as the text grows without being taken more often, which the count above is
	/// blind to; what it cannot catch is any of the defects the count catches, since those read 1.50
	/// and 1.55 here. So it is a ratchet against a blow-up and the count is the gate.
	/// </remarks>
	[Theory]
	[InlineData("the same name in every block", true)]
	[InlineData("a name apiece", false)]
	public void Four_times_the_blocks_does_not_cost_sixteen(string what, bool alike)
	{
		var shorter = Best(200, alike);
		var longer  = Best(800, alike);

		var exponent = Math.Log(longer / shorter) / Math.Log(4.0);

		Assert.True(
			exponent <= 2.0,
			$"With {what}, four times the blocks took {longer / shorter:F1} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs), an exponent of {exponent:F2}.");
	}

	/// <summary>The places a reading of that many blocks looks at.</summary>
	static long Places(int blocks, bool alike)
	{
		var state = Read(blocks, alike);

		Assert.True(state.Places > blocks, $"a reading of {blocks} blocks looked at {state.Places} places.");

		return state.Places;
	}

	/// <summary>The fastest of several readings of what a text of that many blocks writes down.</summary>
	static double Best(int blocks, bool alike)
	{
		Read(blocks, alike);

		GC.Collect();
		GC.WaitForPendingFinalizers();

		var best = double.MaxValue;

		for (var run = 0; run < 7; run++)
		{
			var watch = Stopwatch.StartNew();

			Read(blocks, alike);

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}

	/// <summary>What reading a lambda whose body is that many sibling blocks writes down.</summary>
	/// <remarks>
	/// <para>
	/// Each block declares a name and reads it, as <c>{ var i = 1; i; }</c> does.
	/// </para>
	/// <para>
	/// Read TWICE, and built afterwards, because that is what a reading does and each of those is
	/// what a cost was hiding behind. A body is read more than once — an `if` with no `else` reads
	/// its branch as each of its two forms, and a body whose lambda had no types is read before it
	/// has them and again after — so by the second reading every declaration in the text is written
	/// down already, and a use looked for from the END of them walks the whole text to reach its
	/// own. A probe that read once and used at once finds every name in a step and passes on that
	/// defect, which is where this one began; it was held against the defect until it failed. The
	/// building comes last for the same kind of reason: on the tape nothing is built until the
	/// whole text has been read, so the first block built already sees every declaration there is.
	/// </para>
	/// </remarks>
	static ExpressionParser.State Read(int blocks, bool alike)
	{
		const int Wide = 10;

		var state = new ExpressionParser.State(typeof(BlockScalingTests).Assembly);
		var value = Expression.Constant(1);

		for (var reading = 0; reading < 2; reading++)
			for (var block = 0; block < blocks; block++)
			{
				var name = alike ? "i" : "v" + block;

				Assert.True(state.Scoped(Span(block * Wide, Wide)));
				Assert.True(state.Takes(typeof(int), name, Span(block * Wide + 2, 1)));
				Assert.True(state.Knows(name, Span(block * Wide + 5, 1)), "a block's own name was not found in it.");
			}

		for (var block = 0; block < blocks; block++)
		{
			var built = state.Block([], Span(block * Wide, Wide), value);

			Assert.Single(((BlockExpression)built).Variables);
		}

		return state;
	}

	static ExpressionParser.SourceSpan Span(int at, int length)
	{
		return new ExpressionParser.SourceSpan(at, length);
	}
}
