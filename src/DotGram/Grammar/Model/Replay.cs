using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Which rules may be read over an input the accepted derivation does not cover — read and
/// then thrown away, or read twice over the same text.
/// </summary>
/// <remarks>
/// <para>
/// This is the question a carrier that builds where it reads has to ask. The tape defers
/// every construction until the parse has accepted, so a <c>=&gt;</c> under it runs once
/// and only on the derivation that stood; that is what lets an author write one that is
/// not safe to run speculatively (§7.3). A carrier that builds at the accepting state of
/// the rule keeps that promise exactly for the rules named here as not replayed, and
/// breaks it for the rest.
/// </para>
/// <para>
/// The answer is conservative in the direction that costs performance and not correctness:
/// where the graph does not settle it, the rule is called replayed. A rule reached from a
/// replayed one is replayed with it — everything under a reading that is thrown away is
/// thrown away too.
/// </para>
/// <para>
/// It reads the graph and nothing else, so it stands before anything is emitted and can be
/// asked by the thing that chooses how to emit.
/// </para>
/// </remarks>
public static class Replay
{
	/// <summary>Why a rule is read where the reading may not stand.</summary>
	public enum Because
	{
		/// <summary>It is not: every reading of it is on the derivation that accepted.</summary>
		Stands,

		/// <summary>
		/// Its reading is put back only where the parse as a whole fails: something after
		/// it can refuse and nothing above it has anything else to try. No value of it is
		/// ever handed to anyone, so a carrier that built it early is caught only by a
		/// construction with an effect beyond the value it answers with.
		/// </summary>
		Losing,

		/// <summary>Something after it in the same sequence can fail, and then the reading is put back.</summary>
		Follows,

		/// <summary>It is inside a lookahead, whose reading is discarded whatever it says.</summary>
		Lookahead,

		/// <summary>
		/// A repetition may give back a turn that succeeded. Not answered here: that is the
		/// tape's way back, a fact about the emitted reader rather than about the graph, so
		/// a grammar whose reader opens ways has to be asked again before this report may
		/// be read as a proof.
		/// </summary>
		Turn,

		/// <summary>It is reached from a rule that is itself read where the reading may not stand.</summary>
		Under,
	}

	/// <summary>What the graph says about every rule in it.</summary>
	public sealed record Report(IReadOnlyDictionary<RuleSymbol, Because> Rules)
	{
		/// <summary>
		/// Where each rule with a cause of its own was first given one, where they were asked for
		/// (<see cref="Of(RecognitionGraph, bool)"/>): the report a build writes on request, and
		/// nothing a parser is generated from.
		/// </summary>
		public IReadOnlyList<Site>? Sites { get; init; }

		/// <summary>Whether this rule's reading always stands, so its value may be built where it is read.</summary>
		public bool Stands(RuleSymbol rule) =>
			!Rules.TryGetValue(rule, out var because) || because == Because.Stands;

		/// <summary>
		/// Whether every reading of it that a parse keeps is on the accepted derivation:
		/// true where it stands, and where the only readings put back belong to a parse
		/// that goes on to fail and hand nothing back.
		/// </summary>
		public bool Keeps(RuleSymbol rule) =>
			!Rules.TryGetValue(rule, out var because) || because is Because.Stands or Because.Losing;

		/// <summary>How many rules stand, of those the graph holds.</summary>
		public int Standing
		{
			get
			{
				var count = 0;

				foreach (var one in Rules.Values)
					if (one is Because.Stands or Because.Losing)
						count++;

				return count;
			}
		}
	}

	/// <summary>
	/// One place a reading was found not to stand: the rule it was read in, the rule read, why,
	/// and what around it says so — <c>[choice]</c>, <c>[turn]</c> or <c>[lookahead]</c> — and the
	/// part after it that can refuse, where one does.
	/// </summary>
	public sealed record Site(RuleSymbol Owner, RuleSymbol Called, Because Because, string? Around, Node? Failing);

	/// <summary>Every rule of the graph, and whether a reading of it can fail to stand.</summary>
	public static Report Of(RecognitionGraph graph) => Of(graph, sites: false);

	/// <summary>
	/// The same, and where <paramref name="sites"/> is asked, where each cause was found. Kept only
	/// on request: a generated parser needs the answers, and the places are for a report.
	/// </summary>
	public static Report Of(RecognitionGraph graph, bool sites)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var found  = new Dictionary<RuleSymbol, Because>();
		var follow = FollowSets.Of(graph);
		var places = sites ? new List<Site>() : null;

		foreach (var rule in graph.Rules)
			found[rule] = Because.Stands;

		foreach (var rule in graph.Rules)
			if (graph.Bodies.TryGetValue(rule, out var body))
				Walk(
					body, Because.Stands, elsewhere: false,
					follow.TryGetValue(rule, out var after) ? after : FollowSets.Continuation.All,
					FollowSets.SeamOf(rule, graph), graph, found, new Where(rule, places, null));

		Spread(graph, found);

		return new Report(found) { Sites = places };
	}

	/// <summary>
	/// One body, with what is already known about the reading it stands in: <see
	/// cref="Because.Stands"/> while nothing can take it back, and the reason once
	/// something can. <paramref name="after"/> is what can come right after the node: the
	/// rest of the sequence around it, and past a rest that may read nothing, what comes
	/// after that, up to the rule's follow — plainly, and past the <paramref name="seam"/> the
	/// rule's namespace reads between its parts, where that is the first thing read.
	/// </summary>
	static void Walk(
		Node node, Because taken, bool elsewhere, FollowSets.Continuation after, RuleSymbol? seam,
		RecognitionGraph graph, Dictionary<RuleSymbol, Because> found, Where where)
	{
		switch (node)
		{
			case Node.Call(var called, var arguments):
				if (taken != Because.Stands && found.TryGetValue(called, out var was) && Weaker(was, taken))
				{
					found[called] = taken;
					where.Places?.Add(new Site(where.Owner, called, taken, where.Why, where.Failing));
				}

				foreach (var argument in arguments)
					Walk(argument, taken, elsewhere, FollowSets.Continuation.All, seam, graph, found, where);

				break;

			case Node.Sequence(var parts):
				// A part is put back when anything after it fails, because the sequence
				// fails with it and the position goes back to where the sequence began. What
				// that says is added to what is known from outside, not only where nothing was:
				// a reading the whole parse would lose may still be replaced here, inside a
				// turn that gives it up and lets the parse go on.
				for (var i = 0; i < parts.Count; i++)
				{
					var put     = taken;
					var failing = -1;

					for (var j = i + 1; j < parts.Count; j++)
						if (CanFail(parts[j], graph))
						{
							var here = elsewhere ? Because.Follows : Because.Losing;

							if (Weaker(put, here))
							{
								put = here;
								failing = j;
							}

							break;
						}

					Walk(
						parts[i], put, elsewhere, Next(parts, i + 1, after, seam, graph), seam, graph, found,
						failing >= 0 && where.Places is not null ? where with { Failing = parts[failing] } : where);
				}

				break;

			case Node.Choice(var alternatives):
				// A call inside an alternative stood unless something after it in that same
				// alternative failed, which the sequence above is what says. What the choice
				// adds is somewhere for such a failure to go: an alternative after this one that
				// can begin where it began. A reading put back there is replaced, not lost.
				// Where no later alternative can, the choice fails with this one, and what that
				// means is the answer of whatever holds the choice — `elsewhere` from above.
				for (var i = 0; i < alternatives.Count; i++)
				{
					var replaced = !elsewhere && Replaced(alternatives, i, after.Plain, graph);

					Walk(
						alternatives[i], taken, elsewhere || replaced, after, seam, graph, found,
						replaced && where.Places is not null ? where with { Why = "[choice]" } : where);
				}

				break;

			case Node.Repeat(var body, _, _):
			{
				// A turn that fails is put back, but what stood inside it before the failure
				// is what the sequence above already answers. What a repetition adds of its
				// own is that such a failure ends the repetition and the parse goes on with what
				// follows it, which replaces the turn's reading where it can begin where the
				// turn began. Where it cannot, the parse fails there, and that is the answer of
				// whatever holds the repetition. Giving back a turn that succeeded is the tape's
				// way back: see the note on Because.Turn.
				//
				// A turn that begins with the seam begins where the continuation does, on the same
				// trivia, which says nothing; what either reads past it is what tells them apart,
				// as FollowSets' AfterSeam has it.
				var stops = Seamed(body, seam, out var past)
					? Begins(past, after.AfterSeam, graph) is not { } beyond || !after.AfterSeam.IsKnown || beyond.Overlaps(after.AfterSeam)
					: Begins(body, after.Plain, graph) is not { } begins || !after.Plain.IsKnown || begins.Overlaps(after.Plain);

				Walk(
					body, taken, elsewhere || stops, after, seam, graph, found,
					!elsewhere && stops && where.Places is not null ? where with { Why = "[turn]" } : where);

				break;
			}

			case Node.Lookahead(_, var body):
				Walk(
					body, Worse(taken, Because.Lookahead), elsewhere: true, FollowSets.Continuation.All, seam, graph, found,
					where.Places is not null && where.Why is null ? where with { Why = "[lookahead]" } : where);

				break;

			case Node.Atomic(var body):
				Walk(body, taken, elsewhere, after, seam, graph, found, where);

				break;

			case Node.Capture(_, var body):
				Walk(body, taken, elsewhere, after, seam, graph, found, where);

				break;

			case Node.Marked(var body, _):
				Walk(body, taken, elsewhere, after, seam, graph, found, where);

				break;

			case Node.Construct(var body, _):
				Walk(body, taken, elsewhere, after, seam, graph, found, where);

				break;
		}
	}

	/// <summary>
	/// Whether an alternative that fails after it began can be replaced by one tried after
	/// it: whether some later alternative can begin with a token this one begins with.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An ordered choice tries the alternatives after the one that failed, and only those.
	/// Where none of them can begin where this one began, each refuses at the first token,
	/// nothing takes this one's place, and the choice fails with it; its reading is then
	/// replaced only if something holding the choice replaces it, which is <c>elsewhere</c>
	/// and not this. The same fact the emitter asks before it writes a choice as a switch,
	/// asked of each alternative rather than of the choice as a whole: <c>COUNT(*)</c> may
	/// share its first word with the general aggregate after it, and the general aggregate
	/// with nothing after that.
	/// </para>
	/// <para>
	/// What an alternative can begin with is its first set, and where it may read nothing,
	/// whatever can follow the choice as well; so a later alternative that may read nothing
	/// can begin with anything that follows. What is not settled counts as overlapping.
	/// </para>
	/// </remarks>
	static bool Replaced(IReadOnlyList<Node> alternatives, int at, FirstSets.First after, RecognitionGraph graph)
	{
		if (at + 1 >= alternatives.Count)
			return false;

		if (Begins(alternatives[at], after, graph) is not { } begins)
			return true;

		for (var later = at + 1; later < alternatives.Count; later++)
			if (Begins(alternatives[later], after, graph) is not { } other || begins.Overlaps(other))
				return true;

		return false;
	}

	/// <summary>
	/// What a node can begin with where <paramref name="after"/> follows it: its first set,
	/// and <paramref name="after"/> too where it may read nothing. Null where that is not
	/// settled.
	/// </summary>
	static FirstSets.First? Begins(Node node, FirstSets.First after, RecognitionGraph graph)
	{
		var first = FirstSets.Of(node, graph);

		if (FirstSets.Nullable(node, graph))
			first = first.Or(after);

		return first.IsKnown ? first : null;
	}

	/// <summary>
	/// What can come after the parts before <paramref name="from"/>: the rest of the sequence,
	/// and past a rest that may read nothing, what follows the sequence. Plainly, and past the
	/// seam where the rest begins with it; where it does not, past the seam is what the rest
	/// begins with, unless the seam could have begun there too.
	/// </summary>
	static FollowSets.Continuation Next(
		IReadOnlyList<Node> parts, int from, FollowSets.Continuation after, RuleSymbol? seam, RecognitionGraph graph)
	{
		var plain = Rest(parts, from, after.Plain, graph);

		if (from >= parts.Count)
			return after;

		if (seam is not null && parts[from] is Node.Call(var called, { Count: 0 }) && ReferenceEquals(called, seam))
			return new FollowSets.Continuation(plain, Rest(parts, from + 1, after.AfterSeam, graph));

		var seamFirst = seam is not null && graph.Bodies.TryGetValue(seam, out var seamBody)
			? FirstSets.Of(seamBody, graph)
			: FirstSets.First.None;

		return new FollowSets.Continuation(plain, plain.Overlaps(seamFirst) ? FirstSets.First.All : plain);
	}

	/// <summary>What the parts from <paramref name="from"/> begin with, and <paramref name="after"/> where they may all read nothing.</summary>
	static FirstSets.First Rest(IReadOnlyList<Node> parts, int from, FirstSets.First after, RecognitionGraph graph)
	{
		var rest = FirstSets.Following(parts, from, graph);

		for (var i = from; i < parts.Count; i++)
			if (!FirstSets.Nullable(parts[i], graph))
				return rest;

		return rest.Or(after);
	}

	/// <summary>
	/// Whether a turn begins with the seam, and what it reads past it: the rest of its
	/// sequence, as one node.
	/// </summary>
	static bool Seamed(Node body, RuleSymbol? seam, out Node past)
	{
		past = body;

		if (seam is null || body is not Node.Sequence(var parts) || parts.Count < 2 ||
			parts[0] is not Node.Call(var called, { Count: 0 }) || !ReferenceEquals(called, seam))
			return false;

		past = parts.Count == 2 ? parts[1] : new Node.Sequence(parts.Skip(1).ToList());

		return true;
	}

	/// <summary>
	/// The rule being walked, where to put what is found, and what around the walk so far says a
	/// reading may be replaced — kept only where the places were asked for.
	/// </summary>
	readonly record struct Where(RuleSymbol Owner, List<Site>? Places, string? Why, Node? Failing = null);

	/// <summary>The reason already known, or the new one where nothing was known.</summary>
	static Because Worse(Because taken, Because other) => taken == Because.Stands ? other : taken;

	/// <summary>Whether the first reason says less than the second, so the second replaces it.</summary>
	static bool Weaker(Because was, Because now) =>
		was == Because.Stands || was == Because.Losing && now != Because.Losing;

	/// <summary>
	/// Whether this node can refuse where it stands. Conservative: only what provably
	/// always matches answers no, so a reason to put a reading back is never missed.
	/// </summary>
	static bool CanFail(Node node, RecognitionGraph graph)
	{
		switch (node)
		{
			case Node.Empty:
			case Node.Glue:
				return false;

			// A repetition with no lower bound matches nothing at all rather than failing.
			case Node.Repeat(_, 0, _):
				return false;

			case Node.Capture(_, var one):
				return CanFail(one, graph);

			case Node.Marked(var one, _):
				return CanFail(one, graph);

			case Node.Construct(var one, _):
				return CanFail(one, graph);

			case Node.Atomic(var one):
				return CanFail(one, graph);

			case Node.Sequence(var parts):
			{
				foreach (var part in parts)
					if (CanFail(part, graph))
						return true;

				return false;
			}

			// A choice fails only where every way through it does.
			case Node.Choice(var alternatives):
			{
				foreach (var alternative in alternatives)
					if (!CanFail(alternative, graph))
						return false;

				return true;
			}

			// A rule that matches the empty input cannot refuse.
			case Node.Call(var called, _):
				return !(graph.Nullable.TryGetValue(called, out var nullable) && nullable);

			default:
				return true;
		}
	}

	/// <summary>
	/// What a rule reaches is read inside its reading, so a reading that does not stand
	/// takes everything under it with it.
	/// </summary>
	static void Spread(RecognitionGraph graph, Dictionary<RuleSymbol, Because> found)
	{
		var calls = new CallGraph(graph.Rules, rule => Called(graph, rule));
		var again = true;

		while (again)
		{
			again = false;

			foreach (var rule in graph.Rules)
			{
				if (found[rule] == Because.Stands)
					continue;

				foreach (var under in calls.Calls(rule))
					if (found.TryGetValue(under, out var was) && Weaker(was, Under(found[rule])))
					{
						found[under] = Under(found[rule]);
						again        = true;
					}
			}
		}
	}

	/// <summary>A reading under a lost one is lost with it, and under a replaced one replaced.</summary>
	static Because Under(Because owner) => owner == Because.Losing ? Because.Losing : Because.Under;

	/// <summary>The rules one rule's body names.</summary>
	static IEnumerable<RuleSymbol> Called(RecognitionGraph graph, RuleSymbol rule)
	{
		if (!graph.Bodies.TryGetValue(rule, out var body))
			yield break;

		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Call(var called, _))
				yield return called;
	}
}
