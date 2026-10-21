using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Lifting a capture out of the repetition that repeats it.
/// </summary>
/// <remarks>
/// <para>
/// A capture binds tighter than a quantifier (§10), so <c>t: ['a'..'z']+</c> is one
/// capture repeated. Compiled as written, every turn records the character it took —
/// an arena entry per character, for a value that is defined to be the text joined.
/// Consecutive turns are contiguous — each begins where the one before it ended — so
/// the join of the turns is the extent of the repetition, and one record around the
/// loop says everything the records inside it said. That one record is what frees the
/// loop itself to compile silent: a repetition whose body writes nothing needs no
/// entry, no count and no way back.
/// </para>
/// <para>
/// Two shapes are deliberately left alone. A capture of a rule's value collects an
/// array (§4.1 case 2), where every turn's record is the point; only a capture whose
/// body is pure text moves. And an optional — <c>min == 0, max == 1</c> — keeps the
/// difference §7.3 pins between the capture that did not happen (<c>null</c>) and the
/// run of no turns (<c>""</c>): hoisted, the capture would always record, and null
/// would quietly become empty.
/// </para>
/// <para>
/// Every other bound moves, <c>{0,m}</c> included. A run that may take no turn is a run
/// all the same, and its text is <c>""</c> when it took none — which is what the record
/// around the loop says, and what the turns' records could not: none of them was written,
/// and in a rule of several alternatives the capture they leave unwritten reads as one that
/// never happened.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Rewrite <c>(t: X)+</c> as <c>t: (X+)</c> wherever the two mean the same text, and give
	/// a run whose turns cannot be one record the record of its start.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>(t: X &amp; ',')*</c> cannot move: the comma between two turns is in the extent and
	/// not in the text. A run of it that takes no turn writes nothing, and where the rule has a
	/// route that never reaches the run — another alternative, an option around it — nothing is
	/// what that route leaves too, so <c>""</c> and null would be one answer. Such a run is
	/// given an empty capture of the same name in front of it: one more piece of no text, which
	/// changes no join and says the run was reached. Only where the name is optional in the
	/// rule: where every route writes it, no record already reads as <c>""</c>.
	/// </para>
	/// <para>
	/// Every rule, a left-recursive one and one with binding powers included, and inside the
	/// element of a repetition marked <c>recover</c>: a run there is the author's as anywhere.
	/// What was worked out against a node this rebuilds — the fold's loop and accumulators, a
	/// climb's levels, a recovery — is handed to the node that replaces it.
	/// </para>
	/// </remarks>
	void HoistTextCaptures()
	{
		Dictionary<RuleSymbol, bool>? textRules = null;
		List<string> optional = [];
		Node? loop = null;
		var looking = 0;

		foreach (var rule in _rules)
		{
			// A fold's loop is the fold's and not a run of the author's: it is neither lifted
			// out of nor seeded, and what is rebuilt inside it carries the fold's facts over.
			loop = _folds.TryGetValue(rule, out var fold) ? fold.Loop : null;

			optional = OptionalText(_bodies[rule]);

			_bodies[rule] = Hoist(_bodies[rule]);
		}

		// A node rebuilt in place of another, handed what was recorded against the old one: a
		// recovery, a fold's loop and accumulators, a climb's level (Carry). The old one is in
		// no body any more, every place that held it rebuilt, so it is taken off the tables
		// the graph's integrity check reads: left there, it is a fact about nothing.
		Node Kept(Node old, Node rebuilt)
		{
			Carry(old, rebuilt);

			_recoveries.Remove(old);
			_powers.Remove(old);
			_ceilings.Remove(old);

			foreach (var climbed in _climbing.Keys.ToList())
				if (_climbing[climbed].ContainsKey(old))
				{
					var levels = new Dictionary<Node, int>(NodeIdentity.Instance);

					foreach (var pair in _climbing[climbed])
						if (!ReferenceEquals(pair.Key, old))
							levels[pair.Key] = pair.Value;

					_climbing[climbed] = levels;
				}

			return rebuilt;
		}

		Node Hoist(Node node)
		{
			switch (node)
			{
				case Node.Repeat(Node.Capture(var name, var captured), var min, var max)
					when (min > 0 || max != 1) &&
						!_recoveries.ContainsKey(node) &&
						!ReferenceEquals(node, loop) &&
						PureText(captured):
					return new Node.Capture(name, Placed(node, new Node.Repeat(Hoist(captured), min, max)));

				// A count by a length, on the same terms: its turns are as contiguous. It is never
				// the option `{0,1}`, since a length is not a number the parser is generated with,
				// so every count moves.
				case Node.Counted(Node.Capture(var name, var captured), var min, var max)
					when PureText(captured):
					return new Node.Capture(name, Placed(node, new Node.Counted(Hoist(captured), min, max)));

				case Node.Sequence(var parts):
				{
					var rebuilt = Rebuilt(parts);

					return rebuilt is null ? node : Kept(node, new Node.Sequence(rebuilt));
				}

				case Node.Choice(var alternatives):
				{
					var rebuilt = Rebuilt(alternatives);

					return rebuilt is null ? node : Kept(node, ((Node.Choice)node).Rebuild(rebuilt));
				}

				// A repetition marked `recover` is read into as well — what its element captures
				// is the rule's like anything else — and keeps its recovery; it is not seeded, as
				// its element is what it recovers.
				case Node.Repeat(var body, var min, var max):
				{
					var own = !_recoveries.ContainsKey(node) && !ReferenceEquals(node, loop);

					return Seeded(
						Hoist(body) is var repeated && ReferenceEquals(repeated, body)
							? node
							: Kept(node, new Node.Repeat(repeated, min, max)),
						own && min == 0 && max != 1);
				}

				case Node.Atomic(var kept):
					return Hoist(kept) is var atomic && ReferenceEquals(atomic, kept)
						? node
						: Kept(node, new Node.Atomic(atomic));

				case Node.Marked(var kept, var text):
					return Hoist(kept) is var marked && ReferenceEquals(marked, kept)
						? node
						: Kept(node, new Node.Marked(marked, text));

				case Node.Capture(var name, var body):
					return Hoist(body) is var inner && ReferenceEquals(inner, body)
						? node
						: Kept(node, new Node.Capture(name, inner));

				case Node.Construct(var body, var how):
					return Hoist(body) is var built && ReferenceEquals(built, body)
						? node
						: Kept(node, new Node.Construct(built, how));

				case Node.Lookahead(var positive, var seen):
				{
					looking++;

					var looked = Hoist(seen);

					looking--;

					return ReferenceEquals(looked, seen) ? node : Kept(node, new Node.Lookahead(positive, looked));
				}

				// A binding was lifted out of its own repetitions while lowering; what is left is
				// the captures inside it.
				case Node.Bind(var binding, var bound, var at):
					return Hoist(bound) is var rebound && ReferenceEquals(rebound, bound)
						? node
						: Kept(node, new Node.Bind(binding, rebound, at));

				case Node.Counted(var body, var min, var max):
					return Seeded(
						Hoist(body) is var counted && ReferenceEquals(counted, body)
							? node
							: Kept(node, new Node.Counted(counted, min, max)),
						min is not CountBound.Items { Count: >= 1 });

				default:
					return node;
			}
		}

		// A run that may take no turn, whose every turn writes a text capture the rule may also
		// leave unwritten: an empty capture of each such name in front of it.
		Node Seeded(Node run, bool mayTakeNone)
		{
			if (!mayTakeNone || looking > 0 || optional.Count == 0)
				return run;

			var body  = run is Node.Repeat(var repeated, _, _) ? repeated : ((Node.Counted)run).Body;
			var seeds = new List<Node>();

			foreach (var name in optional)
				if (Writes(body, name))
					seeds.Add(new Node.Capture(name, new Node.Empty()));

			return seeds.Count == 0 ? run : new Node.Sequence([.. seeds, run]);
		}

		// The names a rule captures as text and may leave unwritten, as its result says: those
		// whose member is optional. In the order they are first captured, so that the seeds of
		// one run stand in an order the next generation repeats.
		List<string> OptionalText(Node body)
		{
			var seen  = new HashSet<string>(StringComparer.Ordinal);
			var names = new List<string>();

			foreach (var one in Unlooked(body))
				if (one is Node.Capture(var name, var captured) &&
					CaptureLayout.ValuedCall(captured, BuildsValue) is null &&
					seen.Add(name) &&
					!Writes(body, name))
				{
					names.Add(name);
				}

			return names;
		}

		IReadOnlyList<Node>? Rebuilt(IReadOnlyList<Node> nodes)
		{
			List<Node>? rebuilt = null;

			for (var i = 0; i < nodes.Count; i++)
			{
				var hoisted = Hoist(nodes[i]);

				if (rebuilt is null && !ReferenceEquals(hoisted, nodes[i]))
					rebuilt = [.. nodes.Take(i)];

				rebuilt?.Add(hoisted);
			}

			return rebuilt;
		}

		/// <summary>
		/// Whether a node's value could only ever be the text it matched — no rule value, no
		/// capture of its own, nothing a turn's record would keep that the extent does not.
		/// </summary>
		bool PureText(Node node)
		{
			return node switch
			{
				Node.Call(var rule, var arguments) => arguments.Count == 0 && PureRule(rule),
				Node.Empty or Node.Literal or Node.Element or Node.Behind or Node.Glue => true,
				Node.Sequence(var parts) => parts.All(PureText),
				Node.Choice(var alternatives) { Selection: null } => alternatives.All(PureText),
				Node.Repeat(var body, _, _) => !_recoveries.ContainsKey(node) && PureText(body),
				Node.Atomic(var body) => PureText(body),
				Node.Marked(var body, _) => PureText(body),
				Node.Lookahead(_, var body) => PureText(body),

				// A binding adds no value to what its body matched, and a reference matches text.
				Node.Bind(_, var body, _) => PureText(body),
				Node.Counted(var body, _, _) => PureText(body),
				Node.Same => true,
				_ => false,
			};
		}

		static IEnumerable<Node> Unlooked(Node node)
		{
			yield return node;

			if (node is Node.Lookahead)
				yield break;

			foreach (var child in node.Children)
				foreach (var one in Unlooked(child))
					yield return one;
		}

		bool PureRule(RuleSymbol rule)
		{
			textRules ??= [];

			if (textRules.TryGetValue(rule, out var known))
				return known;

			// Seed before following calls: a recursive component is left to the general
			// capture machinery. A value or a state mark must keep its own lifetime too.
			textRules[rule] = false;

			var pure = !_types.ContainsKey(rule) &&
				!_folds.ContainsKey(rule) && !_climbing.ContainsKey(rule) &&
				_bodies.TryGetValue(rule, out var body) &&
				!NodeWalk.Descendants(body).Any(node => node is Node.Marked || _recoveries.ContainsKey(node)) &&
				PureText(body);

			textRules[rule] = pure;

			return pure;
		}
	}
}
