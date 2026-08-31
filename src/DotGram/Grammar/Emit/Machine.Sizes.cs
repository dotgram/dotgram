using System;
using System.Collections.Generic;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What the generated code is estimated to cost, as against what the grammar is proved to
/// mean.
/// </summary>
/// <remarks>
/// <para>
/// The line these sit on the far side of: an estimate may choose between two shapes already
/// known to mean the same thing — unrolled against looped, a dispatch written out against a
/// call — and may never be the reason something is called legal. Whether backtracking can be
/// removed, whether a reading is settled, whether a rule may stream: those are proofs, and
/// they live beside the grammar rather than here.
/// </para>
/// <para>
/// Both of these fail in the safe direction. A body estimated too heavy to unroll keeps the
/// loop and its count; a first set too wide to write out declines the dispatch and keeps the
/// general form. Neither can make something wrong, only larger.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>
	/// How many basic blocks a recognizer may be estimated to hold before it is written in
	/// more than one method.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The limit it is set under is the compiler's below: past about two thousand blocks
	/// RyuJIT compiles a method the way it compiles one on its first call and leaves it that
	/// way. That number is measured — two grammar shapes crossing within 2% of each other on
	/// blocks while differing by 57% on size — and it is measured for two shapes, not proved
	/// for all of them. A third could cross a little lower.
	/// </para>
	/// <para>
	/// So a quarter under it, and not because a quarter is precise. Aiming at the line would
	/// be fitting a recognizer to a number that moves: the grammar is the consumer's and one
	/// rule added to it shifts the count, with nothing said out loud when it goes over —
	/// only a parse that quietly runs several times slower. The margin is what keeps that
	/// from being a thing anyone has to think about.
	/// </para>
	/// </remarks>
	const int Budget = 1500;

	/// <summary>
	/// How large a part is aimed to be, once <see cref="Budget"/> has said to divide.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A separate number because it answers a separate question, and the two were one for
	/// as long as dividing meant cutting into pieces of <see cref="Budget"/>: a machine
	/// only just over it came out in two, which every measurement calls the worst
	/// arrangement — slower than leaving it whole and slower than cutting it small.
	/// </para>
	/// <para>
	/// Measured on a grammar of a fixed hot core and a ballast grown from nothing to four
	/// hundred cold rules, timing an input that touches only the core. Undivided, the hot
	/// path holds at 379 and 545 ns while the machine is small and then falls off — 2,199
	/// ns once the ballast is fifty rules, 3,423 at four hundred. Divided into parts this
	/// size it is 519, 593, 586, 591, 555, 575: **flat, whatever the grammar is**. That
	/// flatness is the property being bought. A part of this size holds in registers, and
	/// what a crossing costs does not grow with how many there are.
	/// </para>
	/// <para>
	/// And the asymmetry is why it is small rather than careful. Dividing a machine that
	/// did not need it costs about a quarter; leaving one undivided that needed it costs
	/// four times over. So the number errs towards dividing, and the threshold above errs
	/// the same way.
	/// </para>
	/// </remarks>
	const int Part = 150;

	/// <summary>
	/// The count past which the compiler below gives up, as measured — the line the budget
	/// stands a quarter under.
	/// </summary>
	const int Limit = 2000;

	/// <summary>A method was left past <see cref="Limit"/> and nothing here could divide it.</summary>
	public const string Unoptimized = "GRAM5003";

	/// <summary>
	/// How many ranges a character class may be written out as comparisons before it is
	/// read from a table instead.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A class written out costs two comparisons per range and a branch between each, and
	/// the branches are what the compiler below counts: past about two thousand basic
	/// blocks in one method RyuJIT stops optimizing it altogether. A class read from a
	/// table costs one bounds test and one load, whatever the class contains.
	/// </para>
	/// <para>
	/// Two, then, would already pay in branches — and would lose in the small: a class of
	/// two ranges is four comparisons on values already in registers, against a load from
	/// memory that may not be in cache, and the tests that matter most are the ones run per
	/// character. Three is where the branch count starts to dominate and the load is
	/// amortized over enough comparisons to be worth taking. It is an estimate, and both
	/// shapes mean the same set.
	/// </para>
	/// </remarks>
	const int Tabulated = 3;

	/// <summary>Whether a repetition is small enough to write out rather than loop.</summary>
	bool Unrolls(Node.Repeat repeat) =>
		(repeat.Max ?? repeat.Min + 1) * Weight(repeat.Body, Unrollable) <= Unrollable;

	/// <summary>
	/// How much a repetition may be written out one after another rather than looped, counted
	/// in the states the turns would come to.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Unrolling is what removes the count, and with it the last thing the arena was holding
	/// for a repetition that needs it for nothing else. Generated size is not a cost this
	/// project minimizes, but it is not unbounded either, and it does not add — it multiplies.
	/// <c>(H16 &amp; ':'){6}</c> is six copies of <c>H16</c>, each of which is
	/// <c>Hex{1,4}</c>, and the rule that holds it has nine alternatives; counting turns
	/// alone would call each of those small and arrive at hundreds of copies of one character
	/// test.
	/// </para>
	/// <para>
	/// So the budget is turns times what a turn weighs, and a turn weighs what it will
	/// actually be written as — through the calls that are compiled in place, and through the
	/// repetitions inside it, which multiply in their turn.
	/// </para>
	/// </remarks>
	const int Unrollable = 24;

	/// <summary>
	/// About how many states a node will come to, stopping once that is more than is being
	/// asked about.
	/// </summary>
	int Weight(Node node, int budget)
	{
		if (budget <= 0)
			return 1;

		switch (node)
		{
			case Node.Empty:
				return 0;

			case Node.Sequence(var parts):
				return WeightOfAll(parts, budget);

			case Node.Choice(var alternatives):
				return WeightOfAll(alternatives, budget);

			case Node.Capture(_, var captured):
				return 1 + Weight(captured, budget - 1);

			case Node.Construct(var built, _):
				return 1 + Weight(built, budget - 1);

			case Node.Atomic(var kept):
				return 1 + Weight(kept, budget - 1);

			case Node.Marked(var kept, _):
				return 1 + Weight(kept, budget - 1);

			case Node.Lookahead(_, var seen):
				return 1 + Weight(seen, budget - 1);

			// An unbounded one is written once and gone round, so what it weighs is a turn
			// and the going round; a bounded one is written out as many times as it is
			// allowed to happen.
			case Node.Repeat(var body, _, var max):
				return (max ?? 2) * Weight(body, budget);

			case Node.Call(var rule, _) when CanInline(rule) && _graph.Bodies.TryGetValue(rule, out var called):
				return Weight(called, budget);

			default:
				return 1;
		}
	}

	int WeightOfAll(IReadOnlyList<Node> nodes, int budget)
	{
		var total = 0;

		foreach (var node in nodes)
		{
			total += Weight(node, budget - total);

			if (total > budget)
				break;
		}

		return total;
	}

	/// <summary>
	/// The widest first set a rendered test may be written from.
	/// </summary>
	/// <remarks>
	/// Guards the two places an analysis result becomes source text — <see cref="Predictive"/>
	/// and <see cref="Decidable"/> — and nothing else: an analysis that only compares sets is
	/// better off exact whatever their size.
	/// </remarks>
	const int Emitted = 8;
}
