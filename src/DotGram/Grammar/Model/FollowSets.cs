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
	/// <summary>
	/// What can follow, seen twice: as it stands, and past the seam.
	/// </summary>
	/// <param name="Plain">What the continuation can begin with, as ever.</param>
	/// <param name="AfterSeam">
	/// What the continuation can begin with once a leading application of the namespace's
	/// trivia has consumed what it consumes. §4.5 puts that application at the head of
	/// every spaced seam, so a repetition whose turns lead with the trivia and the
	/// continuation behind it both start by reading the same run of it — and the question
	/// that decides whether a turn could instead have been the continuation is asked of
	/// what each reads <em>next</em>. Compared plainly the two overlap on the trivia itself
	/// and the comparison says nothing.
	/// </param>
	public readonly record struct Continuation(FirstSets.First Plain, FirstSets.First AfterSeam)
	{
		public static readonly Continuation All  = new(FirstSets.First.All, FirstSets.First.All);
		public static readonly Continuation None = new(FirstSets.First.None, FirstSets.First.None);
		public static readonly Continuation End  = new(FirstSets.First.End, FirstSets.First.End);

		public Continuation Or(Continuation other) =>
			new(Plain.Or(other.Plain), AfterSeam.Or(other.AfterSeam));

		public bool Covers(Continuation other) =>
			Plain.Covers(other.Plain) && AfterSeam.Covers(other.AfterSeam);
	}

	/// <summary>The rule a namespace applies at its seams, for the rule being walked.</summary>
	public static RuleSymbol? SeamOf(RuleSymbol rule, RecognitionGraph graph) =>
		graph is not null && graph.Trivia.TryGetValue(rule, out var trivia) &&
		trivia is Node.Call(var seam, _)
			? seam
			: null;

	/// <summary>What may follow each rule, as far as the grammar settles it.</summary>
	public static IReadOnlyDictionary<RuleSymbol, Continuation> Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var follow = new Dictionary<RuleSymbol, Continuation>();

		foreach (var rule in graph.Rules)
			follow[rule] = Continuation.None;

		foreach (var publication in graph.Publications)
			if (follow.ContainsKey(publication.Rule))
				follow[publication.Rule] = follow[publication.Rule].Or(
					publication.Kind == PublishKind.Parse ? Continuation.End : Continuation.All);

		// Round and round until nothing new is said. Each round can only add — the union of
		// what a rule was told and what this round tells it — so the sets grow towards a
		// bound and stop. The count is a guard against a mistake in that argument rather
		// than against the grammar.
		for (var round = 0; round <= graph.Rules.Count + 1; round++)
		{
			var settled = true;

			foreach (var rule in graph.Rules)
				if (graph.Bodies.TryGetValue(rule, out var body))
					Contribute(body, follow[rule], SeamOf(rule, graph));

			if (settled)
				return follow;

			void Contribute(Node node, Continuation after, RuleSymbol? seam)
			{
				switch (node)
				{
					case Node.Call(var called, _):
					{
						// A rule lowered under another namespace peels a different seam, so
						// what this site knows past its own is no use to it. The plain half
						// travels regardless.
						var told = ReferenceEquals(SeamOf(called, graph), seam)
							? after
							: new Continuation(after.Plain, FirstSets.First.All);

						if (!follow.TryGetValue(called, out var held) || held.Covers(told))
							return;

						follow[called] = held.Or(told);
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
							Contribute(parts[i], next, seam);

							next = Precedes(parts[i], next, graph, seam);
						}

						return;
					}

					// Every alternative is followed by whatever the choice is.
					case Node.Choice(var alternatives):
					{
						foreach (var alternative in alternatives)
							Contribute(alternative, after, seam);

						return;
					}

					// A turn is followed by another turn or by whatever the repetition is
					// followed by — except that an optional has no other turn, and telling
					// it that one might follow poisons everything upstream of its own first
					// set. `(Argument & …)?` inside a call was telling `Argument` that
					// anything could follow it, and that "anything" walked back through
					// every rule a value can name.
					case Node.Repeat(var body, _, var max):
						Contribute(
							body,
							max == 1 ? after : Precedes(body, after, graph, seam).Or(after),
							seam);

						return;

					case Node.Capture(_, var captured): Contribute(captured, after, seam); return;
					case Node.Construct(var built, _):  Contribute(built,    after, seam); return;
					case Node.Atomic(var kept):         Contribute(kept,     after, seam); return;

					// What is inside is read and given back, so what follows it is read
					// again by whatever comes next — which this cannot see from here.
					case Node.Lookahead(_, var seen):
						Contribute(seen, Continuation.All, seam);

						return;
				}
			}
		}

		return follow;
	}

	/// <summary>
	/// What must begin the input where a node begins, given what must begin it where the
	/// node ends — both halves at once.
	/// </summary>
	/// <remarks>
	/// The same walk the compiler makes over a sequence, and it has to be the same: what a
	/// rule is told about its callers has to agree with what the compiler works out inside
	/// them, or a repetition would be held to something nobody meant.
	/// </remarks>
	public static Continuation Precedes(
		Node node, Continuation after, RecognitionGraph graph, RuleSymbol? seam)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		switch (node)
		{
			// The seam itself, standing first: what follows once it has been read is the
			// old continuation as it plainly was. That is the definition of the other half.
			case Node.Call(var called, _) when seam is not null && ReferenceEquals(called, seam):
				return new Continuation(Plainly(node, after.Plain, graph), after.Plain);

			// Structure is walked rather than summarized, or a sequence that merely leads
			// with the seam would be taxed for beginning with trivia characters — which is
			// precisely the shape every spaced continuation has.
			case Node.Sequence(var parts):
			{
				var next = after;

				for (var i = parts.Count - 1; i >= 0; i--)
					next = Precedes(parts[i], next, graph, seam);

				return next;
			}

			case Node.Choice(var alternatives):
			{
				var merged = Continuation.None;

				foreach (var alternative in alternatives)
					merged = merged.Or(Precedes(alternative, after, graph, seam));

				return merged;
			}

			case Node.Capture(_, var captured):  return Precedes(captured, after, graph, seam);
			case Node.Construct(var built, _):   return Precedes(built,    after, graph, seam);
			case Node.Atomic(var kept):          return Precedes(kept,     after, graph, seam);

			// A turn is either taken — and then it stands before another turn or before the
			// continuation, with anything past its own seam unknowable from here — or, for
			// a run that may be empty, not taken at all.
			case Node.Repeat(var body, var min, _):
			{
				var turn = Precedes(
					body,
					new Continuation(Plainly(node, after.Plain, graph), FirstSets.First.All),
					graph, seam);

				var plain = Plainly(node, after.Plain, graph);

				return new Continuation(
					plain,
					min == 0 ? turn.AfterSeam.Or(after.AfterSeam) : turn.AfterSeam);
			}

			default:
			{
				var plain = Plainly(node, after.Plain, graph);
				var first = FirstSets.Of(node, graph);

				if (first.Nothing)
					return after with { Plain = plain };

				// A leaf that does not lead with the seam. A turn's seam may still have
				// consumed input where this continuation would have to begin, so its first
				// set counts past the seam only where the seam could not even have begun
				// there — and whether the seam could have *carried on* over it is
				// <c>Contained</c>'s question, asked where the peel is used, not here.
				var seamFirst = seam is not null && graph.Bodies.TryGetValue(seam, out var seamBody)
					? FirstSets.Of(seamBody, graph)
					: FirstSets.First.None;

				var past = first.Overlaps(seamFirst) ? FirstSets.First.All : first;

				return new Continuation(
					plain,
					FirstSets.Nullable(node, graph) ? past.Or(after.AfterSeam) : past);
			}
		}
	}

	/// <summary>The plain half alone, as it always was.</summary>
	public static FirstSets.First Plainly(Node node, FirstSets.First after, RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var first = FirstSets.Of(node, graph);

		return first.Nothing                 ? after :
			FirstSets.Nullable(node, graph) ? first.Or(after) :
			first;
	}
}
