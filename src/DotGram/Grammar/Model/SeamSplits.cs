using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Whether anything in a grammar can tell apart two ways of splitting one run of the seam
/// between two applications of it.
/// </summary>
/// <remarks>
/// <para>
/// §4.5 reads the seam at every gap, so a grammar is full of <c>t N t</c> where <c>N</c> can
/// read nothing: an optional part, the end of a rule and the caller's seam after it. A star
/// gives back, so the first seam could hand some of its run to the second — and every such
/// split ends where the one run would, and the first tried is the first seam's longest reading.
/// So a split other than the longest is ever the answer only where something between the two
/// seams succeeds or fails by where it stands without reading: a <c>when</c>, a look (other
/// than the end's own <c>?!any</c>, which fails anywhere inside a run), a look behind, glue, or
/// a recognizer the host measures. Where nothing can stand there, the seam's own run need never
/// give back to a seam after it, and <see cref="Determinism.NeverGivesBack"/> may compare it with
/// what follows past that seam.
/// </para>
/// <para>
/// Asked once per grammar and answered conservatively: each rule is read as a function from
/// what may have just happened — a seam read with nothing since, or a seam and then such a node
/// — to what may have happened by its end, and those functions are found together, as the
/// least answer every rule agrees with.
/// </para>
/// </remarks>
static class SeamSplits
{
	static readonly ConditionalWeakTable<RecognitionGraph, object> _answers = new();

	/// <summary>Whether a node that stands still can be met between two seams.</summary>
	public static bool Matter(RecognitionGraph graph)
	{
		return (bool)_answers.GetValue(graph, static one => Compute(one));
	}

	/// <summary>What may have just been read: a seam, with nothing since; a seam and then a node that stands still.</summary>
	readonly record struct State(bool Seam, bool Still)
	{
		public State Or(State other)
		{
			return new(Seam || other.Seam, Still || other.Still);
		}
	}

	static bool Compute(RecognitionGraph graph)
	{
		var seams = new HashSet<RuleSymbol>(graph.Trivia.Values.OfType<Node.Call>().Select(static call => call.Rule));

		if (seams.Count == 0)
			return false;

		// Each rule's answer for each of the four states it can be entered in, grown until
		// no rule's answer changes.
		var answers = new Dictionary<RuleSymbol, (State After, bool Hazard)[]>();
		var changed = true;

		while (changed)
		{
			changed = false;

			foreach (var rule in graph.Rules)
			{
				if (seams.Contains(rule) || !graph.Bodies.TryGetValue(rule, out var body))
					continue;

				if (!answers.TryGetValue(rule, out var known))
					answers[rule] = known = new (State, bool)[4];

				for (var entered = 0; entered < 4; entered++)
				{
					var state   = new State((entered & 1) != 0, (entered & 2) != 0);
					var hazard  = false;
					var after   = Walk(body, state, ref hazard);
					var widened = (after.Or(known[entered].After), hazard || known[entered].Hazard);

					if (widened != known[entered])
					{
						known[entered] = widened;
						changed        = true;
					}
				}
			}
		}

		var matters = false;

		foreach (var publication in graph.Publications)
		{
			var entry = graph.Trivia.TryGetValue(publication.Rule, out var around)
				? new Node.Sequence([around, new Node.Call(publication.Rule, []), around])
				: (Node)new Node.Call(publication.Rule, []);

			Walk(entry, default, ref matters);
		}

		return matters;

		State Walk(Node node, State state, ref bool hazard)
		{
			switch (node)
			{
				case Node.Call(var called, _) when seams.Contains(called):
					hazard |= state.Still;
					return new State(true, false);

				case Node.Call(var called, _):
				{
					if (!answers.TryGetValue(called, out var known))
						return graph.Bodies.ContainsKey(called) ? default : Still(state);

					var index = (state.Seam ? 1 : 0) | (state.Still ? 2 : 0);

					hazard |= known[index].Hazard;

					return known[index].After;
				}

				case Node.Sequence(var parts):
					foreach (var part in parts)
						state = Walk(part, state, ref hazard);
					return state;

				case Node.Choice(var alternatives):
				{
					var merged = default(State);

					foreach (var alternative in alternatives)
						merged = merged.Or(Walk(alternative, state, ref hazard));

					return merged;
				}

				case Node.Repeat(var body, var min, _):
				{
					// Taken any number of times, until what a turn may leave stops growing.
					var all  = min == 0 ? state : default;
					var turn = state;

					for (var i = 0; i < 3; i++)
					{
						turn = Walk(body, turn, ref hazard);
						all  = all.Or(turn);
						turn = turn.Or(state);
					}

					return all;
				}

				case Node.Capture(_, var body):   return Walk(body, state, ref hazard);
				case Node.Construct(var body, _): return Walk(body, state, ref hazard);
				case Node.Atomic(var body):       return Walk(body, state, ref hazard);
				case Node.Marked(var body, _):    return Walk(body, state, ref hazard);

				// The end, which fails wherever a run of the seam still stands.
				case var end when FollowSets.AtEnd(end, graph):
					return state;

				case Node.Guard or Node.Lookahead or Node.Behind or Node.Glue or Node.External:
					return Still(state);

				case Node.Element:
					return default;

				case Node.Literal(var text):
					return text.Length == 0 ? state : default;

				default:
					return state;
			}
		}

		// A node that stands still, met where a seam may just have been read.
		static State Still(State state)
		{
			return new(state.Seam, state.Still || state.Seam);
		}
	}
}
