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
	public static bool Of(
		Node node, FirstSets.First following, RecognitionGraph graph) =>
		Of(node, [], following, graph);

	/// <summary>
	/// Whether a repetition of this body may run to its end and never be asked to give a
	/// turn back.
	/// </summary>
	public static bool Possessive(
		Node body, FirstSets.First following, RecognitionGraph graph) =>
		Possessive(body, [], following, graph);

	static bool Possessive(
		Node body, Asked asked, FirstSets.First following, RecognitionGraph graph) =>
		following.IsKnown &&
		!FirstSets.Nullable(body, graph) &&
		!FirstSets.Of(body, graph).Overlaps(following) &&
		Of(body, asked, FirstSets.Of(body, graph).Or(following), graph);

	static bool Of(
		Node node, Asked asked, FirstSets.First following, RecognitionGraph graph) =>
		node switch
		{
			Node.Empty or Node.Guard or Node.Lookahead or Node.Behind => true,
			Node.Literal or Node.Element or Node.External => true,
			Node.Capture  (_, var body)   => Of(body, asked, following, graph),
			Node.Construct(var body, _)   => Of(body, asked, following, graph),
			// An atomic group commits its first reading and never gives one back, which is
			// what "at most one match" says. Looking inside asked a harder question than the
			// braces already answer, and answered it badly wherever the body was a choice or
			// a star — which is every trivia written the way §4.5 recommends, and so nearly
			// every grammar.
			Node.Atomic                   => true,
			Node.Marked   (var body, _)   => Of(body, asked, following, graph),
			Node.Sequence (var parts)     => All(parts, asked, following, graph),
			Node.Choice   (var options)   => Distinguishable(options, graph) &&
			                                 All(options, asked, following, graph, sequence: false),
			Node.Repeat   (var body, _, _) => Possessive(body, asked, following, graph),
			Node.Call     (var rule, _)   => Of(rule, asked, following, graph),
			_                             => false,
		};

	/// <summary>Whether a call is to something determinate, guarded against calling round.</summary>
	/// <remarks>
	/// <para>
	/// Met again on the way down, followed by the same thing: the question is the one already
	/// being answered, so it is assumed answered yes and the walk goes on. If the rest agrees
	/// the assumption held; if anything says no this says no with it, and the assumption goes
	/// with the path that made it. Assuming yes and checking is how "never more than one" is
	/// proved of a cycle at all.
	/// </para>
	/// <para>
	/// Followed by something else, it is a different question about the same rule, and it is
	/// asked. It used to be refused instead, and that refusal is what a real grammar runs
	/// into: a reference whose type arguments are optional reaches itself through them under
	/// a continuation of their own — <c>Reference</c>, <c>TypeArgs</c>, <c>Type</c>,
	/// <c>Reference</c> — so the one question that mattered was the one being declined.
	/// </para>
	/// <para>
	/// It terminates for the reason the same-question case does: a pair is put on the path
	/// before it is walked and taken off after, so no pair is entered twice on one path, and
	/// there are finitely many — a continuation is a union of the grammar's own first sets
	/// and there are finitely many of those.
	/// </para>
	/// </remarks>
	static bool Of(
		RuleSymbol rule, Asked asked, FirstSets.First following, RecognitionGraph graph)
	{
		if (asked.TryGetValue(rule, out var already))
		{
			foreach (var seen in already)
				if (FirstSets.Same(seen, following))
					return true;
		}
		else
		{
			asked[rule] = already = [];
		}

		already.Add(following);

		var settled = graph.Bodies.TryGetValue(rule, out var body) &&
			Of(body, asked, following, graph);

		already.RemoveAt(already.Count - 1);

		return settled;
	}

	static bool All(
		IReadOnlyList<Node> nodes, Asked asked, FirstSets.First following, RecognitionGraph graph,
		bool sequence = true)
	{
		var after = following;

		for (var at = nodes.Count - 1; at >= 0; at--)
		{
			if (!Of(nodes[at], asked, after, graph))
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
	public static bool Distinguishable(IReadOnlyList<Node> alternatives, RecognitionGraph graph) =>
		Distinguishable(alternatives, graph, int.MaxValue);

	/// <param name="spelled">
	/// The widest first set the answer may be written out from. A cap on what a rendering
	/// will spell, and a caller that only compares sets passes none: a Unicode category is a
	/// few hundred ranges, exact and useful to a proof, and a page of comparisons where the
	/// alternative's own test is one call. It used to sit inside the proof, so a choice this
	/// could tell apart was called undecidable because writing the decision down would have
	/// been long — a fact about C# deciding a fact about the grammar.
	/// </param>
	public static bool Distinguishable(
		IReadOnlyList<Node> alternatives, RecognitionGraph graph, int spelled)
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
			if (first.Ranges.Count > spelled)
				return false;

			firsts[at] = first;
		}

		for (var one = 0; one < firsts.Length; one++)
			for (var other = one + 1; other < firsts.Length; other++)
				if (firsts[one].Overlaps(firsts[other]))
					return false;

		return true;
	}

	/// <summary>Which rules the walk is inside, and under what continuations.</summary>
	/// <remarks>
	/// The path down rather than everything met on the way: a rule is taken off again on the
	/// way out, so meeting it twice in two places is not mistaken for recursion. More than one
	/// continuation per rule, because a rule reached again under a different one is a
	/// different question and gets asked.
	/// </remarks>
	sealed class Asked : Dictionary<RuleSymbol, List<FirstSets.First>>;
}
