using System;
using System.Diagnostics;
using System.Linq.Expressions;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A text of blocks costs its length: four times the blocks, about four times the bookkeeping,
/// and not sixteen.
/// </summary>
/// <remarks>
/// <para>
/// What a reading writes down about names and blocks (§7.7) is read back by every use of a name
/// and by every block that is built, and each of those readings walked the whole text. Over 200
/// sibling blocks each declaring one name it was 204,230 places read where 50 read 13,580, and
/// 100,902 places walked to find one block's own declarations where 50 walked 6,477 — the text
/// squared, three times over. Counted rather than timed, because a count needs no quiet machine:
/// each of those is now one place read per use and one per declaration.
/// </para>
/// <para>
/// Written against the state directly, and not against a parse. The state is what was made to
/// cost the text's length, and it is what both readers of this language share; the reading around
/// it is not linear in the blocks yet, and a test over a parse would be asserting that instead.
/// Measured on 2026-09-26, tiering off, 25 to 800 blocks: the state's own work per block is flat,
/// the hand-written reader is flat at 3.45 µs a block, and the generated one rises from 4.78 to
/// 22.56 — which is the generated machinery's to answer for, and is reported where it belongs.
/// </para>
/// <para>
/// The bound is on the exponent and not on a time, and it is 1.3 rather than 1.1 for the reason
/// <see cref="ScriptScalingTests"/> gives: a shared runner read 1.16 of something linear. Against
/// the square's 2.0 it is a wide bound and still catches every defect this was written for.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class BlockScalingTests
{
	[Theory]
	[InlineData("the same name in every block", true)]
	[InlineData("a name apiece", false)]
	public void Four_times_the_blocks_costs_about_four_times_as_much(string what, bool alike)
	{
		var shorter = Best(200, alike);
		var longer  = Best(800, alike);

		var exponent = Math.Log(longer / shorter) / Math.Log(4.0);

		Assert.True(
			exponent <= 1.3,
			$"With {what}, four times the blocks took {longer / shorter:F1} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs), an exponent of {exponent:F2}.");
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
	static void Read(int blocks, bool alike)
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
	}

	static ExpressionParser.SourceSpan Span(int at, int length)
	{
		return new ExpressionParser.SourceSpan(at, length);
	}
}
