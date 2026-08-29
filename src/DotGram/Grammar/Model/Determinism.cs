using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Whether a construct has at most one match where it stands.
/// </summary>
/// <remarks>
/// <para>
/// One length, not a choice of them. Alternatives settle it when one character tells them
/// apart; a repetition settles it exactly when it is possessive, since where a repetition
/// stops is otherwise the very choice this asks about; a literal, an element and an
/// external recognizer settle it by being what they are.
/// </para>
/// <para>
/// It reads the graph and nothing else, which is why it is here rather than beside the
/// compilation that used to hold it. Two things want it and only one of them is a
/// rendering: the emitter, to know whether a repetition needs a way back written down,
/// and the normalizer, to know whether folding a shared prefix out of a choice can be
/// seen from the outside \u2014 where the prefix has one reading it cannot, and where it has
/// several it can.
/// </para>
/// <para>
/// It has to be told what follows. Possessiveness is a fact about a repetition in a place
/// and not about a repetition, and the same is true of everything built out of one.
/// </para>
/// </remarks>
public static class Determinism
{
	/// <summary>Whether this node has at most one match, given what follows it.</summary>
	/// <param name="rendered">
	/// The widest first set a decision may be taken from. A cap on what a rendering will
	/// write out, threaded in rather than assumed: an analysis that only compares sets is
	/// better off exact whatever their size, and the caller that renders is the one that
	/// has to care.
	/// </param>
	public static bool Of(
		Node node, FirstSets.First following, RecognitionGraph graph, int rendered) =>
		Of(node, [], following, graph, rendered);

	/// <summary>
	/// Whether a repetition of this body may run to its end and never be asked to give a
	/// turn back.
	/// </summary>
	public static bool Possessive(
		Node body, FirstSets.First following, RecognitionGraph graph, int rendered) =>
		Possessive(body, [], following, graph, rendered);

	static bool Possessive(
		Node body, Asked asked, FirstSets.First following, RecognitionGraph graph, int rendered) =>
		following.IsKnown &&
		!FirstSets.Nullable(body, graph) &&
		!FirstSets.Of(body, graph).Overlaps(following) &&
		Of(body, asked, FirstSets.Of(body, graph).Or(following), graph, rendered);

	static bool Of(
		Node node, Asked asked, FirstSets.First following, RecognitionGraph graph, int rendered) =>
		node switch
		{
			Node.Empty or Node.Guard or Node.Lookahead or Node.Behind => true,
			Node.Literal or Node.Element or Node.External => true,
			Node.Capture  (_, var body)   => Of(body, asked, following, graph, rendered),
			Node.Construct(var body, _)   => Of(body, asked, following, graph, rendered),
			Node.Atomic   (var body)      => Of(body, asked, following, graph, rendered),
			Node.Marked   (var body, _)   => Of(body, asked, following, graph, rendered),
			Node.Sequence (var parts)     => All(parts, asked, following, graph, rendered),
			Node.Choice   (var options)   => Distinguishable(options, graph, rendered) &&
			                                 All(options, asked, following, graph, rendered, sequence: false),
			Node.Repeat   (var body, _, _) => Possessive(body, asked, following, graph, rendered),
			Node.Call     (var rule, _)   => Of(rule, asked, following, graph, rendered),
			_                             => false,
		};

	/// <summary>Whether a call is to something determinate, guarded against calling round.</summary>
	/// <remarks>
	/// Met again on the way down, and followed by the same thing: the question is the one
	/// already being answered, so it is assumed answered yes and the walk goes on. If the
	/// rest agrees the assumption held; if anything says no this says no with it, and the
	/// assumption goes with the path that made it. Assuming yes and checking is how "never
	/// more than one" is proved of a cycle at all.
	/// </remarks>
	static bool Of(
		RuleSymbol rule, Asked asked, FirstSets.First following, RecognitionGraph graph, int rendered)
	{
		if (asked.TryGetValue(rule, out var already))
			return FirstSets.Same(already, following);

		asked[rule] = following;

		var settled = graph.Bodies.TryGetValue(rule, out var body) &&
			Of(body, asked, following, graph, rendered);

		asked.Remove(rule);

		return settled;
	}

	static bool All(
		IReadOnlyList<Node> nodes, Asked asked, FirstSets.First following, RecognitionGraph graph,
		int rendered, bool sequence = true)
	{
		var after = following;

		for (var at = nodes.Count - 1; at >= 0; at--)
		{
			if (!Of(nodes[at], asked, after, graph, rendered))
				return false;

			if (sequence)
				after = FirstSets.Precedes(nodes[at], after, graph);
		}

		return true;
	}

	/// <summary>Whether one character tells every alternative from every other.</summary>
	/// <remarks>
	/// An alternative that can match nothing matches everywhere, so it stays reachable
	/// after another has failed and cannot be told apart by a character at all. First sets
	/// say "anything" where they are unsure, and two of those overlap, so an alternative
	/// this cannot read gives up rather than claims something false.
	/// </remarks>
	public static bool Distinguishable(
		IReadOnlyList<Node> alternatives, RecognitionGraph graph, int rendered)
	{
		if (alternatives is null)
			throw new ArgumentNullException(nameof(alternatives));

		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (alternatives.Count < 2)
			return false;

		var firsts = new FirstSets.First[alternatives.Count];

		for (var at = 0; at < alternatives.Count; at++)
		{
			var first = FirstSets.Of(alternatives[at], graph);

			if (first.Anything || first.Nothing || FirstSets.Nullable(alternatives[at], graph))
				return false;

			// Knowable is not the same as worth writing down: a Unicode category is a few
			// hundred ranges, exact and useful to the analyses, and a dispatch spelled out
			// over them would be a page of comparisons where the alternative's own test is
			// one call. The set stays precise; only the rendering declines.
			if (first.Ranges.Count > rendered)
				return false;

			firsts[at] = first;
		}

		for (var one = 0; one < firsts.Length; one++)
			for (var other = one + 1; other < firsts.Length; other++)
				if (firsts[one].Overlaps(firsts[other]))
					return false;

		return true;
	}

	/// <summary>Which rules the walk is inside, and what followed each where it went in.</summary>
	/// <remarks>
	/// The path down rather than everything met on the way: a rule is taken off again on
	/// the way out, so meeting it twice in two places is not mistaken for recursion.
	/// </remarks>
	sealed class Asked : Dictionary<RuleSymbol, FirstSets.First>;
}
