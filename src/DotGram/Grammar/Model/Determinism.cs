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
		Node node, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol? seam) =>
		Of(node, [], following, graph, seam);

	/// <summary>
	/// Whether a repetition of this body may run to its end and never be asked to give a
	/// turn back.
	/// </summary>
	public static bool Possessive(
		Node body, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol? seam) =>
		Possessive(body, [], following, graph, seam);

	static bool Possessive(
		Node body, Asked asked, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol? seam) =>
		following.Plain.IsKnown &&
		!FirstSets.Nullable(body, graph) &&
		!Decides(body, graph, seam).Overlaps(Against(body, following, seam)) &&
		Of(body, asked, following.Or(new FollowSets.Continuation(
			FirstSets.Of(body, graph), FirstSets.Of(body, graph))), graph, seam);

	/// <summary>
	/// What tells a turn from the continuation behind it: the turn's own first set, or what
	/// it reads after the trivia where it leads with the trivia.
	/// </summary>
	/// <remarks>
	/// §4.5 weaves <c>trivia</c> between every pair of operands, so a turn and the
	/// continuation behind it both open by reading the same run of it. `trivia` is an atomic
	/// group — it commits its first reading and never gives it back — so that run is the same
	/// either way, and what decides between them is what each reads next. Compared plainly
	/// the two overlap on the trivia itself and the comparison says nothing, which on a
	/// grammar written the way §4.5 recommends is nearly every loop there is.
	/// </remarks>
	static FirstSets.First Decides(Node body, RecognitionGraph graph, RuleSymbol? seam) =>
		Past(body, seam) is { } past ? FirstSets.Of(past, graph) : FirstSets.Of(body, graph);

	/// <summary>And the half of the continuation it is compared against.</summary>
	static FirstSets.First Against(Node body, FollowSets.Continuation following, RuleSymbol? seam) =>
		Past(body, seam) is null ? following.Plain : following.AfterSeam;

	/// <summary>A turn with the trivia it leads with taken off, or null where it leads with none.</summary>
	static Node? Past(Node body, RuleSymbol? seam) =>
		seam is not null &&
		body is Node.Sequence(var parts) &&
		parts.Count > 1 &&
		parts[0] is Node.Call(var called, { Count: 0 }) &&
		ReferenceEquals(called, seam)
			? parts.Count == 2 ? parts[1] : new Node.Sequence([.. parts.Skip(1)])
			: null;

	static bool Of(
		Node node, Asked asked, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol? seam) =>
		node switch
		{
			Node.Empty or Node.Guard or Node.Lookahead or Node.Behind => true,
			Node.Literal or Node.Element or Node.External => true,
			Node.Capture  (_, var body)   => Of(body, asked, following, graph, seam),
			Node.Construct(var body, _)   => Of(body, asked, following, graph, seam),
			// An atomic group commits its first reading and never gives one back, which is
			// what "at most one match" says. Looking inside asked a harder question than the
			// braces already answer, and answered it badly wherever the body was a choice or
			// a star — which is every trivia written the way §4.5 recommends, and so nearly
			// every grammar.
			Node.Atomic                   => true,
			Node.Marked   (var body, _)   => Of(body, asked, following, graph, seam),
			Node.Sequence (var parts)     => All(parts, asked, following, graph, seam),
			Node.Choice   (var options)   => Distinguishable(options, graph) &&
			                                 All(options, asked, following, graph, seam, sequence: false),
			Node.Repeat   (var body, _, _) => Possessive(body, asked, following, graph, seam),
			Node.Call     (var rule, _)   => Of(rule, asked, following, graph, seam),
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
		RuleSymbol rule, Asked asked, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol? seam)
	{
		if (asked.TryGetValue(rule, out var already))
		{
			foreach (var seen in already)
				if (FirstSets.Same(seen.Plain, following.Plain) &&
					FirstSets.Same(seen.AfterSeam, following.AfterSeam))
					return true;
		}
		else
		{
			asked[rule] = already = [];
		}

		already.Add(following);

		var settled = graph.Bodies.TryGetValue(rule, out var body) &&
			Of(body, asked, following, graph, seam);

		already.RemoveAt(already.Count - 1);

		return settled;
	}

	static bool All(
		IReadOnlyList<Node> nodes, Asked asked, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol? seam,
		bool sequence = true)
	{
		var after = following;

		for (var at = nodes.Count - 1; at >= 0; at--)
		{
			if (!Of(nodes[at], asked, after, graph, seam))
				return false;

			if (sequence)
				after = FollowSets.Precedes(nodes[at], after, graph, seam);
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
	sealed class Asked : Dictionary<RuleSymbol, List<FollowSets.Continuation>>;
}
