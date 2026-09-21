using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Moving the seam out of the head of a repetition's turn, into its tail.
/// </summary>
/// <remarks>
/// <para>
/// §4.5 weaves the seam between the operands of a sequence and in front of each turn of a
/// repetition, so <c>File = (Setting &amp; eol)* &amp; eof</c> becomes
/// <c>(t &amp; Setting &amp; t &amp; eol)* &amp; t &amp; eof</c>. At the end of the input a turn
/// begins, reads its seam, fails at <c>Setting</c> and is given back seam and all — and the
/// continuation reads the same seam again. So the turn is a reading that can be replaced, and
/// every analysis that asks what a turn begins with finds the seam, which is what the
/// continuation begins with too.
/// </para>
/// <para>
/// Rewritten <c>t &amp; (R &amp; t)* &amp; C</c> from <c>(t &amp; R)* &amp; t &amp; C</c>, the
/// seam is read once before the repetition and once after each turn, and a turn begins with
/// <c>R</c>. A star gives back (§3), so the two are the same only where the seam has one
/// reading that can lead anywhere: where it only reads, and neither <c>R</c> nor <c>C</c> can
/// begin on a character a shorter reading of it would stop before — the seam's boundaries,
/// <see cref="Determinism.Contained"/>, which is the question <c>NeverGivesBack</c> asks too.
/// Then a shorter reading fails at once wherever it is tried, and what is left is the seam's
/// longest reading, which a star read twice at one place makes empty the second time. Written
/// out, with <c>p</c> where the repetition begins:
/// </para>
/// <list type="bullet">
/// <item>Before, the first turn reads <c>t</c> to <c>p1</c> and tries <c>R</c> there; after,
/// <c>t</c> reads to <c>p1</c> and the first turn tries <c>R</c> there.</item>
/// <item>Where <c>R</c> reads to <c>q</c>, before, the next turn reads <c>t</c> from <c>q</c>
/// to <c>q1</c> and tries <c>R</c> at <c>q1</c>; after, the turn ends by reading <c>t</c> from
/// <c>q</c> to <c>q1</c>, and the next tries <c>R</c> at <c>q1</c>.</item>
/// <item>Where <c>R</c> fails at <c>q1</c>, before, the turn is given back to <c>q</c> and the
/// continuation reads <c>t</c> from <c>q</c> to <c>q1</c>; after, the repetition ends at
/// <c>q1</c> and the continuation begins there without it. <c>C</c> is tried at <c>q1</c>
/// either way.</item>
/// </list>
/// <para>
/// A bound on the turns counts the <c>R</c>s either way, and a turn that reads nothing stops
/// the repetition in both. What moves is the extent of the repetition — it no longer covers
/// the seam before its first turn, and covers the one after its last — so a repetition whose
/// extent anything asks for is left as it is: one under a capture, which records it, and one
/// marked <c>recover</c>, whose turns are elements with text of their own. Nor where the seam
/// can do anything but read: a guard, a mark, a construction or a capture inside it would run
/// a different number of times.
/// </para>
/// <para>
/// Two rewrites of the same kind go with it. <c>t &amp; A | t &amp; B</c> is <c>t &amp;
/// (A | B)</c> under the same condition on <c>A</c> and <c>B</c>: the seam's one reading is
/// the one both alternatives would have begun with, so a turn that is a choice of operators
/// begins past the seam too. And <c>t &amp; t</c> is <c>t</c> for a seam that only reads:
/// every split of one run between the two ends where the one run would, and the first to be
/// tried is the longest.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Rewrite <c>(t &amp; R)* &amp; t</c> as <c>t &amp; (R &amp; t)*</c>, <c>t &amp; A | t &amp; B</c>
	/// as <c>t &amp; (A | B)</c>, and <c>t &amp; t</c> as <c>t</c>, wherever the seam <c>t</c> only
	/// reads and nothing after it can begin inside what it read.
	/// </summary>
	void HoistSeams()
	{
		var graph     = Provisional();
		var reading   = new Dictionary<RuleSymbol, bool>();
		var contained = new Dictionary<RuleSymbol, FirstSets.First>();

		foreach (var rule in _rules)
		{
			// As HoistTextCaptures: a fold's loop and a climb's alternatives are facts keyed
			// to the nodes they were found on.
			if (_folds.ContainsKey(rule) || _climbing.ContainsKey(rule))
				continue;

			_bodies[rule] = Hoist(_bodies[rule]);
		}

		Node Hoist(Node node)
		{
			switch (node)
			{
				case Node.Sequence(var parts):
				{
					var rebuilt = Rebuilt(parts);
					var moved   = Moved(rebuilt ?? parts);

					return moved is null && rebuilt is null ? node : Sequence(moved ?? [.. rebuilt!]);
				}

				case Node.Choice(var alternatives):
				{
					var rebuilt = Rebuilt(alternatives);
					var choice  = rebuilt is null ? (Node.Choice)node : ((Node.Choice)node).Rebuild(rebuilt);

					return Headed(choice) ?? choice;
				}

				case Node.Repeat(var body, var min, var max) when !_recoveries.ContainsKey(node):
					return Hoist(body) is var repeated && ReferenceEquals(repeated, body)
						? node
						: new Node.Repeat(repeated, min, max);

				case Node.Atomic(var kept):
					return Hoist(kept) is var atomic && ReferenceEquals(atomic, kept)
						? node
						: new Node.Atomic(atomic);

				case Node.Marked(var kept, var text):
					return Hoist(kept) is var marked && ReferenceEquals(marked, kept)
						? node
						: new Node.Marked(marked, text);

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

		// The parts of one sequence with each repetition whose turn begins with the seam that
		// follows it rewritten, and a seam read twice read once; null where nothing changed.
		List<Node>? Moved(IReadOnlyList<Node> parts)
		{
			List<Node>? moved = null;

			for (var i = 0; i < parts.Count; i++)
			{
				// `t & t` is `t`.
				if (i > 0 && Seam(parts[i]) is { } again && OnlyReads(again) &&
					ReferenceEquals(Seam(moved is null ? parts[i - 1] : moved.Count > 0 ? moved[^1] : null), again))
				{
					moved ??= [.. parts.Take(i)];

					continue;
				}

				if (i + 1 < parts.Count &&
					parts[i] is Node.Repeat(Node.Sequence(var turn), var min, var max) repeat &&
					!_recoveries.ContainsKey(repeat) &&
					turn.Count > 1 &&
					Seam(turn[0]) is { } seam &&
					ReferenceEquals(Seam(parts[i + 1]), seam) &&
					OnlyReads(seam) &&
					Apart(seam, Beginning(turn.Skip(1).ToList())) &&
					!Contained(seam).Overlaps(After(parts, i + 2)))
				{
					moved ??= [.. parts.Take(i)];

					// The seam before the loop, unless it stands there already.
					if (moved.Count == 0 || !ReferenceEquals(Seam(moved[^1]), seam))
						moved.Add(parts[i + 1]);

					var tail = new List<Node>(turn.Count);

					tail.AddRange(turn.Skip(1));

					// A turn that already ends with the seam keeps the one it has.
					if (!ReferenceEquals(Seam(tail[^1]), seam))
						tail.Add(turn[0]);

					moved.Add(new Node.Repeat(new Node.Sequence(tail), min, max));

					i++;

					continue;
				}

				moved?.Add(parts[i]);
			}

			return moved;
		}

		// A choice whose every alternative begins with one seam, as that seam and the choice of
		// what follows it; null where they do not, or where one could begin inside the seam.
		Node? Headed(Node.Choice choice)
		{
			RuleSymbol? seam = null;

			var rests = new List<Node>(choice.Nodes.Count);

			foreach (var alternative in choice.Nodes)
			{
				if (alternative is not Node.Sequence(var parts) || parts.Count < 2 || Seam(parts[0]) is not { } head ||
					seam is not null && !ReferenceEquals(seam, head))
					return null;

				seam = head;
				rests.Add(Beginning(parts.Skip(1).ToList()));
			}

			if (seam is null || !OnlyReads(seam) || !rests.TrueForAll(rest => Apart(seam, rest)))
				return null;

			return new Node.Sequence([((Node.Sequence)choice.Nodes[0]).Nodes[0], choice.Rebuild(rests)]);
		}

		// What the rest of the sequence can begin with, from a part on: known only where the rest
		// must read something or reaches the end, and asked of nothing further out — where it
		// could read nothing and go on to whatever follows the sequence, the answer is anything,
		// and the rewrite is not made. Asking FOLLOW of the whole grammar for the few places
		// that could use it cost more than the rewrite was worth.
		FirstSets.First After(IReadOnlyList<Node> parts, int from)
		{
			return from >= parts.Count
				? FirstSets.First.All
				: FollowSets.Plainly(new Node.Sequence([.. parts.Skip(from)]), FirstSets.First.All, graph);
		}

		// What is read after a seam, as one node.
		static Node Beginning(List<Node> parts)
		{
			return parts.Count == 1 ? parts[0] : new Node.Sequence(parts);
		}

		static Node Sequence(List<Node> parts)
		{
			return parts.Count == 1 ? parts[0] : new Node.Sequence(parts);
		}

		static RuleSymbol? Seam(Node? node)
		{
			return node is Node.Call(var called, { Count: 0 }) ? called : null;
		}

		// Whether what follows a seam must consume and cannot begin inside what the seam read:
		// then the seam has one reading that can lead anywhere, its longest.
		bool Apart(RuleSymbol seam, Node rest)
		{
			return !FirstSets.Nullable(rest, graph) && !Contained(seam).Overlaps(FirstSets.Of(rest, graph));
		}

		FirstSets.First Contained(RuleSymbol seam)
		{
			return contained.TryGetValue(seam, out var known)
				? known
				: contained[seam] = Determinism.Contained(seam, graph);
		}

		// A star that only reads — its body a repetition without a bound, or an optional of a rule
		// that is one — with nothing in it, or in what it calls, that guards, marks, builds or
		// captures.
		bool OnlyReads(RuleSymbol seam)
		{
			return reading.TryGetValue(seam, out var known)
				? known
				: reading[seam] = _bodies.TryGetValue(seam, out var body) &&
					(body is Node.Repeat(_, 0, null) ||
					 body is Node.Repeat(Node.Call(var inner, { Count: 0 }), 0, 1) &&
						_bodies.TryGetValue(inner, out var run) && run is Node.Repeat(_, _, null)) &&
					Reads(body, [seam]);
		}

		bool Reads(Node body, HashSet<RuleSymbol> seen)
		{
			foreach (var node in NodeWalk.Descendants(body))
			{
				switch (node)
				{
					case Node.Guard or Node.Marked or Node.Construct or Node.Capture:
						return false;

					case Node.Call(var called, _) when seen.Add(called):
						if (!_bodies.TryGetValue(called, out var inner) || !Reads(inner, seen))
							return false;
						break;
				}
			}

			return true;
		}
	}
}
