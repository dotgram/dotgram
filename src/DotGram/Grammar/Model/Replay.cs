using System;
using System.Collections.Generic;

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

	/// <summary>Every rule of the graph, and whether a reading of it can fail to stand.</summary>
	public static Report Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var found = new Dictionary<RuleSymbol, Because>();

		foreach (var rule in graph.Rules)
			found[rule] = Because.Stands;

		foreach (var rule in graph.Rules)
			if (graph.Bodies.TryGetValue(rule, out var body))
				Walk(body, Because.Stands, elsewhere: false, graph, found);

		Spread(graph, found);

		return new Report(found);
	}

	/// <summary>
	/// One body, with what is already known about the reading it stands in: <see
	/// cref="Because.Stands"/> while nothing can take it back, and the reason once
	/// something can.
	/// </summary>
	static void Walk(
		Node node, Because taken, bool elsewhere, RecognitionGraph graph,
		Dictionary<RuleSymbol, Because> found)
	{
		switch (node)
		{
			case Node.Call(var called, var arguments):
				if (taken != Because.Stands && found.TryGetValue(called, out var was) && Weaker(was, taken))
					found[called] = taken;

				foreach (var argument in arguments)
					Walk(argument, taken, elsewhere, graph, found);

				break;

			case Node.Sequence(var parts):
				// A part is put back when anything after it fails, because the sequence
				// fails with it and the position goes back to where the sequence began.
				for (var i = 0; i < parts.Count; i++)
				{
					var after = taken;

					for (var j = i + 1; j < parts.Count && after == Because.Stands; j++)
						if (CanFail(parts[j], graph))
						{
							after = elsewhere ? Because.Follows : Because.Losing;

							break;
						}

					Walk(parts[i], after, elsewhere, graph, found);
				}

				break;

			case Node.Choice(var alternatives):
				// A call inside an alternative stood unless something after it in that same
				// alternative failed, which the sequence above is what says. What the choice
				// adds is somewhere for such a failure to go: every alternative but the last
				// has one behind it, and a reading put back there is replaced, not lost.
				for (var i = 0; i < alternatives.Count; i++)
					Walk(alternatives[i], taken, elsewhere || i + 1 < alternatives.Count, graph, found);

				break;

			case Node.Repeat(var body, _, _):
				// A turn that fails is put back, but what stood inside it before the failure
				// is what the sequence above already answers. What a repetition adds of its
				// own is that such a failure ends the repetition and the parse goes on, so a
				// reading lost inside a turn is replaced and not merely lost. Giving back a turn
				// that succeeded is the tape's way back: see the note on Because.Turn.
				Walk(body, taken, elsewhere: true, graph, found);

				break;

			case Node.Lookahead(_, var body):
				Walk(body, Worse(taken, Because.Lookahead), elsewhere: true, graph, found);

				break;

			case Node.Atomic(var body):
				Walk(body, taken, elsewhere, graph, found);

				break;

			case Node.Capture(_, var body):
				Walk(body, taken, elsewhere, graph, found);

				break;

			case Node.Marked(var body, _):
				Walk(body, taken, elsewhere, graph, found);

				break;

			case Node.Construct(var body, _):
				Walk(body, taken, elsewhere, graph, found);

				break;
		}
	}

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
