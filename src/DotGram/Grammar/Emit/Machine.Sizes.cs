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
