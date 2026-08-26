using System;
using System.Collections.Generic;
using System.Linq;

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
/// difference §10 pins between the capture that did not happen (<c>null</c>) and the
/// run of no turns (<c>""</c>): hoisted, the capture would always record, and null
/// would quietly become empty.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Rewrite <c>(t: X)+</c> as <c>t: (X+)</c> wherever the two mean the same text.
	/// </summary>
	void HoistTextCaptures()
	{
		foreach (var rule in _rules)
		{
			// A fold's loop and a climb's calls are facts keyed to the nodes they were
			// found on; a rule that owns either is compiled by the general machinery
			// regardless, and is not worth re-keying them for.
			if (_folds.ContainsKey(rule) || _climbing.ContainsKey(rule))
				continue;

			_bodies[rule] = Hoist(_bodies[rule]);
		}

		Node Hoist(Node node)
		{
			switch (node)
			{
				case Node.Repeat(Node.Capture(var name, var captured), var min, var max)
					when (min >= 1 || max is null) &&
						!_recoveries.ContainsKey(node) &&
						PureText(captured):
					return new Node.Capture(name, new Node.Repeat(Hoist(captured), min, max));

				case Node.Sequence(var parts):
				{
					var rebuilt = Rebuilt(parts);

					return rebuilt is null ? node : new Node.Sequence(rebuilt);
				}

				case Node.Choice(var alternatives):
				{
					var rebuilt = Rebuilt(alternatives);

					return rebuilt is null ? node : new Node.Choice(rebuilt);
				}

				case Node.Repeat(var body, var min, var max) when !_recoveries.ContainsKey(node):
					return Hoist(body) is var repeated && ReferenceEquals(repeated, body)
						? node
						: new Node.Repeat(repeated, min, max);

				case Node.Atomic(var kept):
					return Hoist(kept) is var atomic && ReferenceEquals(atomic, kept)
						? node
						: new Node.Atomic(atomic);

				case Node.Capture(var name, var body):
					return Hoist(body) is var inner && ReferenceEquals(inner, body)
						? node
						: new Node.Capture(name, inner);

				case Node.Construct(var body, var how):
					return Hoist(body) is var built && ReferenceEquals(built, body)
						? node
						: new Node.Construct(built, how);

				case Node.Lookahead(var positive, var seen):
					return Hoist(seen) is var looked && ReferenceEquals(looked, seen)
						? node
						: new Node.Lookahead(positive, looked);

				default:
					return node;
			}
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
	}

	/// <summary>
	/// Whether a node's value could only ever be the text it matched — no rule value, no
	/// capture of its own, nothing a turn's record would keep that the extent does not.
	/// </summary>
	bool PureText(Node node) =>
		node switch
		{
			Node.Empty or Node.Literal or Node.Element or Node.Behind => true,
			Node.Sequence(var parts)        => parts.All(PureText),
			Node.Choice(var alternatives)   => alternatives.All(PureText),
			Node.Repeat(var body, _, _)     => !_recoveries.ContainsKey(node) && PureText(body),
			Node.Atomic(var body)           => PureText(body),
			Node.Lookahead(_, var body)     => PureText(body),
			_                               => false,
		};
}
