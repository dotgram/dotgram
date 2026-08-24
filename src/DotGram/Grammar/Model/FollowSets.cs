using System;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// What can come after a rule, wherever it is called from.
/// </summary>
/// <remarks>
/// <para>
/// A rule is compiled once and called from everywhere, so what follows it is not a property
/// of the rule but of the place — and the compiler, having one rule and many places, has to
/// answer for all of them at once. Without that answer everything at the end of a rule's
/// body is compiled as though anything at all might come next, which is what makes a
/// repetition there keep its ways back: it cannot be shown that nothing would ever come for
/// them.
/// </para>
/// <para>
/// The answer is the union over the call sites, and it is circular — what follows a rule
/// depends on what follows the rules that call it, and a grammar may call round in a ring.
/// So it is computed as a fixed point: start with what the publications say, go round adding
/// what each call site contributes, and stop when a round adds nothing.
/// </para>
/// <para>
/// A published rule is where the answer comes from rather than where it is needed. A
/// <c>parse</c> reads the whole input, so after its root there is the end of the text and
/// nothing else — a fact, and the strongest one here. A <c>find</c> may stop anywhere, so
/// after its root there is anything. A rule published both ways gets both, which is
/// anything.
/// </para>
/// </remarks>
public static class FollowSets
{
	/// <summary>What may follow each rule, as far as the grammar settles it.</summary>
	public static IReadOnlyDictionary<RuleSymbol, FirstSets.First> Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var follow = new Dictionary<RuleSymbol, FirstSets.First>();

		foreach (var rule in graph.Rules)
			follow[rule] = FirstSets.First.None;

		foreach (var publication in graph.Publications)
			if (follow.ContainsKey(publication.Rule))
				follow[publication.Rule] = follow[publication.Rule].Or(
					publication.Kind == PublishKind.Parse ? FirstSets.First.End : FirstSets.First.All);

		// Round and round until nothing new is said. Each round can only add — the union of
		// what a rule was told and what this round tells it — so the sets grow towards a
		// bound and stop. The count is a guard against a mistake in that argument rather
		// than against the grammar.
		for (var round = 0; round <= graph.Rules.Count + 1; round++)
		{
			var settled = true;

			foreach (var rule in graph.Rules)
				if (graph.Bodies.TryGetValue(rule, out var body))
					Contribute(body, follow[rule]);

			if (settled)
				return follow;

			void Contribute(Node node, FirstSets.First after)
			{
				switch (node)
				{
					case Node.Call(var called, _):
					{
						if (!follow.TryGetValue(called, out var told) || told.Covers(after))
							return;

						follow[called] = told.Or(after);
						settled        = false;

						return;
					}

					// Each part is followed by the rest of the sequence, and by what follows
					// the sequence where the rest can match nothing.
					case Node.Sequence(var parts):
					{
						var next = after;

						for (var i = parts.Count - 1; i >= 0; i--)
						{
							Contribute(parts[i], next);

							next = Precedes(parts[i], next, graph);
						}

						return;
					}

					// Every alternative is followed by whatever the choice is.
					case Node.Choice(var alternatives):
					{
						foreach (var alternative in alternatives)
							Contribute(alternative, after);

						return;
					}

					// A turn is followed by another turn or by whatever the repetition is.
					case Node.Repeat(var body, _, _):
						Contribute(body, FirstSets.Of(body, graph).Or(after));

						return;

					case Node.Capture(_, var captured): Contribute(captured, after); return;
					case Node.Construct(var built, _):  Contribute(built,    after); return;
					case Node.Atomic(var kept):         Contribute(kept,     after); return;

					// What is inside is read and given back, so what follows it is read
					// again by whatever comes next — which this cannot see from here.
					case Node.Lookahead(_, var seen):
						Contribute(seen, FirstSets.First.All);

						return;
				}
			}
		}

		return follow;
	}

	/// <summary>
	/// What must begin the input where a node begins, given what must begin it where the
	/// node ends.
	/// </summary>
	/// <remarks>
	/// The same walk the compiler makes over a sequence, and it has to be the same: what a
	/// rule is told about its callers has to agree with what the compiler works out inside
	/// them, or a repetition would be held to something nobody meant.
	/// </remarks>
	public static FirstSets.First Precedes(Node node, FirstSets.First after, RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var first = FirstSets.Of(node, graph);

		return first.Nothing                 ? after :
			FirstSets.Nullable(node, graph) ? first.Or(after) :
			first;
	}
}
