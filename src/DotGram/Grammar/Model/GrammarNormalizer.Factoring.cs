using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Reads a shared leading operand once instead of once per alternative, where doing so
	/// cannot be seen from the outside.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Ordered choice is not obliged to be spelled with a common prefix shared, and the
	/// author who does not share it writes the same language and gets a different parser:
	/// every alternative after the first reads the prefix again, and the doubling compounds
	/// through nesting. Making the author write for the machine is what a generator is for
	/// avoiding, so the fold is done here.
	/// </para>
	/// <para>
	/// <b>Only where it is invisible.</b> <c>A &amp; X | A &amp; Y</c> and <c>A &amp; (X |
	/// Y)</c> are the same grammar exactly when <c>A</c> has one reading where it stands.
	/// Where it has several the two prefer different ones: the alternatives prefer a shorter
	/// reading of <c>A</c> that lets a tail fit, and the folded form takes <c>A</c>'s own
	/// reading and then chooses. That is not a subtlety about performance, it decides which
	/// alternative matched and so which <c>=&gt;</c> runs. <see cref="Determinism"/> is the
	/// proof, and where it does not reach, nothing is folded and <c>GRAM4016</c> is left to
	/// say so to the author, whose choice it then is.
	/// </para>
	/// <para>
	/// <b>After the checks, and before nothing.</b> The grammar is checked as it was
	/// written — a <c>=&gt;</c> is refused anywhere but on an alternative of the rule, and
	/// the folded shape puts one after a shared head, which the author may not write and the
	/// compiler may. Then the results are computed again, because a fold drops a duplicate
	/// capture and the numbering of the slots is what the emitter will lay out from.
	/// </para>
	/// </remarks>
	void Factor()
	{
		// A recovery is a decision about a shape, and this moves the shape. Left out of the
		// first cut rather than reasoned about carelessly.
		if (_recoveries.Count > 0)
			return;

		var graph  = Provisional();
		var follow = FollowSets.Of(graph);
		var folded = false;

		foreach (var rule in _rules)
		{
			// A left-recursive rule was rewritten into a loop, and that loop is held by
			// identity — `Fold.Loop`, and the accumulators keyed by the steps under it. A
			// rewrite inside the body replaces those nodes and the fold stops recognizing
			// its own. Left out of the first cut rather than rebuilt carelessly.
			if (_folds.ContainsKey(rule) ||
				!_bodies.TryGetValue(rule, out var body) ||
				!follow.TryGetValue(rule, out var after))
				continue;

			var rewritten = Folded(body, after.Plain, graph);

			if (ReferenceEquals(rewritten, body))
				continue;

			_bodies[rule] = rewritten;
			folded        = true;
		}

		if (folded)
			ComputeResults();
	}

	/// <summary>The graph as it stands, for the sets this pass reasons with.</summary>
	/// <remarks>
	/// FIRST and FOLLOW are asked of the grammar before the fold and used to decide it,
	/// which is sound in the direction it matters: a fold rearranges the inside of a choice
	/// and changes nothing about what can follow that choice, so what these say about the
	/// position is what they would still say afterwards.
	/// </remarks>
	RecognitionGraph Provisional() =>
		new(_rules, _bodies, _nullable, _results, _types, [], _publications, _diagnostics)
		{
			Folds      = _folds,
			Trivia     = _trivia,
			Recoveries = _recoveries,
			Climbing   = _climbing,
			Powers     = _powers,
		};

	/// <summary>This node with every choice inside it folded where one can be.</summary>
	Node Folded(Node node, FirstSets.First following, RecognitionGraph graph)
	{
		switch (node)
		{
			case Node.Choice(var alternatives):
			{
				var inner = Each(alternatives, following, graph, sequence: false);

				return Share(inner ?? alternatives, following, graph);
			}

			case Node.Sequence(var parts):
			{
				var inner = Each(parts, following, graph);

				return inner is null ? node : new Node.Sequence(inner);
			}

			case Node.Capture(var name, var body):
			{
				var inner = Folded(body, following, graph);

				return ReferenceEquals(inner, body) ? node : new Node.Capture(name, inner);
			}

			case Node.Construct(var body, var how):
			{
				var inner = Folded(body, following, graph);

				return ReferenceEquals(inner, body) ? node : new Node.Construct(inner, how);
			}

			case Node.Atomic(var body):
			{
				var inner = Folded(body, following, graph);

				return ReferenceEquals(inner, body) ? node : new Node.Atomic(inner);
			}

			case Node.Repeat(var body, var min, var max):
			{
				// A turn is followed by another turn or by what follows the repetition.
				var inner = Folded(body, FirstSets.Of(body, graph).Or(following), graph);

				return ReferenceEquals(inner, body) ? node : new Node.Repeat(inner, min, max);
			}

			// Left alone rather than walked into. A mark stands over an extent and a
			// lookahead is rewound; both are shapes this first cut does not move.
			default: return node;
		}
	}

	/// <summary>
	/// The alternatives with every run of them that shares a determinate leading operand
	/// replaced by one alternative that reads it once.
	/// </summary>
	Node Share(IReadOnlyList<Node> alternatives, FirstSets.First following, RecognitionGraph graph)
	{
		List<Node>? folded = null;

		for (var at = 0; at < alternatives.Count; at++)
		{
			var last = Run(alternatives, at);

			if (last > at && Sharing(alternatives, at, last, following, graph) is { } one)
			{
				folded ??= [.. alternatives.Take(at)];
				folded.Add(one);
				at = last;

				continue;
			}

			folded?.Add(alternatives[at]);
		}

		if (folded is null)
			return new Node.Choice(alternatives);

		return folded.Count == 1 ? folded[0] : new Node.Choice(folded);
	}

	/// <summary>How far a run of alternatives sharing one leading operand reaches.</summary>
	int Run(IReadOnlyList<Node> alternatives, int from)
	{
		if (!Splits(alternatives[from], out _, out var head, out _) || !Movable(head))
			return from;

		var last = from;

		for (var at = from + 1; at < alternatives.Count; at++)
		{
			if (!Splits(alternatives[at], out _, out var other, out _) ||
				!Movable(other) ||
				!SameShape(head, other) ||
				!string.Equals(Named(head), Named(other), StringComparison.Ordinal))
				break;

			last = at;
		}

		return last;
	}

	/// <summary>
	/// The one alternative a run becomes, or null where the shared operand is not shown to
	/// have a single reading and the fold would therefore be visible.
	/// </summary>
	Node? Sharing(
		IReadOnlyList<Node> alternatives, int from, int last, FirstSets.First following,
		RecognitionGraph graph)
	{
		var tails = new List<Node>(last - from + 1);
		var after = FirstSets.First.None;

		for (var at = from; at <= last; at++)
		{
			Splits(alternatives[at], out var how, out _, out var tail);

			var rest = tail ?? new Node.Empty();

			// What follows the shared operand in this alternative is this tail, and past it
			// whatever follows the choice, where the tail can match nothing.
			after = after.Or(FirstSets.Precedes(rest, following, graph));
			tails.Add(how is null ? rest : new Node.Construct(rest, how));
		}

		Splits(alternatives[from], out _, out var head, out _);

		// The whole condition. Reading it once instead of once per alternative is the same
		// reading only where there was one reading to begin with.
		return Determinism.Of(head, after, graph, int.MaxValue)
			? new Node.Sequence([head, new Node.Choice(tails)])
			: null;
	}

	/// <summary>An alternative as its leading operand and what comes after it.</summary>
	static bool Splits(Node alternative, out Construction? how, out Node head, out Node? tail)
	{
		how = null;

		var inner = alternative;

		if (alternative is Node.Construct(var built, var construction))
		{
			how   = construction;
			inner = built;
		}

		if (inner is Node.Sequence(var parts))
		{
			if (parts.Count == 0)
			{
				head = inner;
				tail = null;

				return false;
			}

			head = parts[0];
			tail = parts.Count switch
			{
				1 => null,
				2 => parts[1],
				_ => new Node.Sequence([.. parts.Skip(1)]),
			};

			return true;
		}

		head = inner;
		tail = null;

		return true;
	}

	/// <summary>The name the operand is captured under, or the empty string for none.</summary>
	/// <remarks>
	/// One of a run's operands survives and the rest are dropped, so what they are called has
	/// to be the same thing: otherwise a name would lose the slot it was written to.
	/// Different names for one operand are a fold this declines rather than one it cannot
	/// have.
	/// </remarks>
	static string Named(Node head) => head is Node.Capture(var name, _) ? name : "";

	/// <summary>
	/// Whether an operand may be the one that survives, which is to say whether dropping the
	/// copies of it loses nothing.
	/// </summary>
	/// <remarks>
	/// A capture of the whole operand is kept by keeping the operand. Anything written down
	/// inside it — another capture, a construction, a mark, a guard — belongs to the copies
	/// being dropped as much as to the one being kept, and this does not move those.
	/// </remarks>
	static bool Movable(Node head)
	{
		var core = head is Node.Capture(_, var body) ? body : head;

		if (core is Node.Marked)
			return false;

		foreach (var node in NodeWalk.Descendants(core))
			if (node is Node.Capture or Node.Construct or Node.Marked or Node.Guard)
				return false;

		return true;
	}

	/// <summary>
	/// Each of them, folded, threading what follows — or null where none of them changed.
	/// </summary>
	IReadOnlyList<Node>? Each(
		IReadOnlyList<Node> nodes, FirstSets.First following, RecognitionGraph graph,
		bool sequence = true)
	{
		List<Node>? rewritten = null;
		var         after     = following;

		for (var at = nodes.Count - 1; at >= 0; at--)
		{
			var one = Folded(nodes[at], after, graph);

			if (!ReferenceEquals(one, nodes[at]))
			{
				rewritten ??= [.. nodes];
				rewritten[at] = one;
			}

			if (sequence)
				after = FirstSets.Precedes(nodes[at], after, graph);
		}

		return rewritten;
	}
}
