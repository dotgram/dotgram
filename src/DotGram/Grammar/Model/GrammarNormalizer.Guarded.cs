using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// A rule that comes back to itself past a recognizer of the host, with something read after the call back, read as
/// the left recursion §4.3 rewrites wherever the recognizer stands still at the rule's entry.
/// </summary>
/// <remarks>
/// <para>
/// <c>Sum = Num | @Ws &amp; Sum &amp; '+' &amp; Num</c>. Where <c>Ws</c> moves, the call back is an ordinary call
/// from a later position. Where it stands, it is the same rule at the same position — left recursion, which §4.3
/// reads as a loop over the tails and which the recognizer only hid. A recognizer's answer depends only on the text
/// and the position (§7.2), so whether it stands is settled once at the entry, and the rule becomes a choice between
/// two rules it is the name of:
/// </para>
/// <code>
/// Sum       = @Ws(stands) &amp; Sum_Left | ?!@Ws(stands) &amp; Sum_Plain
/// Sum_Left  = Num | Sum_Left &amp; '+' &amp; Num            (folded as §4.3 folds it)
/// Sum_Plain = Num | @Ws(moves) &amp; Sum &amp; '+' &amp; Num  (the rule as written)
/// </code>
/// <para>
/// Spike: taken only for a direct cycle whose recursive alternatives share one prefix of a single recognizer and
/// lookaheads, with a base, no guard, context, binding, parameter, recovery or strength on the way, and a value
/// built by <c>=&gt;</c> on every alternative or by none. Everything else is left as it is written.
/// </para>
/// </remarks>
sealed partial class GrammarNormalizer
{
	/// <summary>The rules this pass split, by the rule a caller names: the loop and the rule as written.</summary>
	readonly Dictionary<RuleSymbol, (RuleSymbol Left, RuleSymbol Plain)> _guarded = [];

	void RewriteGuardedLeftRecursion()
	{
		if (Reentry.Unchecked)
			return;

		var callers = Callers();

		foreach (var rule in _rules.ToList())
			if (_bodies.TryGetValue(rule, out var body))
				Guard(rule, body, callers);
	}

	/// <summary>For each rule, the rules that call it.</summary>
	Dictionary<RuleSymbol, HashSet<RuleSymbol>> Callers()
	{
		var callers = new Dictionary<RuleSymbol, HashSet<RuleSymbol>>();

		foreach (var pair in _bodies)
			foreach (var node in NodeWalk.Descendants(pair.Value))
				if (node is Node.Call(var callee, _))
				{
					if (!callers.TryGetValue(callee, out var set))
						callers[callee] = set = [];

					set.Add(pair.Key);
				}

		return callers;
	}

	/// <summary>The rules that may reach <paramref name="rule"/> by calls, the rule itself among them.</summary>
	static HashSet<RuleSymbol> Reaching(RuleSymbol rule, Dictionary<RuleSymbol, HashSet<RuleSymbol>> callers)
	{
		var found   = new HashSet<RuleSymbol> { rule };
		var pending = new Stack<RuleSymbol>();

		pending.Push(rule);

		while (pending.Count > 0)
			if (callers.TryGetValue(pending.Pop(), out var next))
				foreach (var one in next)
					if (found.Add(one))
						pending.Push(one);

		return found;
	}

	void Guard(RuleSymbol rule, Node body, Dictionary<RuleSymbol, HashSet<RuleSymbol>> callers)
	{
		if (rule.Declaration is not { } declaration || declaration.Params.Count > 0 || _guarded.ContainsKey(rule))
			return;

		var alternatives = Alternatives(body);

		if (body is Node.Choice { Selection: not null })
			return;

		var reaching = Reaching(rule, callers);

		List<Node>? prefix = null;

		var guarded = new Dictionary<int, (int Head, Node Body, Node.Construct? Built)>();

		for (var i = 0; i < alternatives.Count; i++)
		{
			var alternative = alternatives[i];
			var built       = alternative as Node.Construct;
			var inner       = built?.Body ?? alternative;

			if (_bounds.ContainsKey(inner) || _bounds.ContainsKey(alternative))
				return;

			if (inner is not Node.Sequence(var operands))
				continue;

			var head = -1;

			for (var at = 0; at < operands.Count; at++)
			{
				var operand = operands[at];

				if ((operand is Node.Capture(_, var captured) ? captured : operand) is Node.Call(var called, var arguments) &&
					ReferenceEquals(called, rule))
				{
					if (arguments.Count == 0)
						head = at;

					break;
				}

				if (operand is not (Node.External { HasValue: false, UsesContext: false } or Node.Lookahead))
					break;
			}

			if (head <= 0)
				continue;

			var leading = operands.Take(head).ToList();

			if (leading.Count(static one => one is Node.External) != 1)
				return;

			// What the lookaheads of the prefix read must not reach the rule, nor capture.
			foreach (var part in leading)
				if (NodeWalk.Descendants(part).Any(node =>
					node is Node.Capture or Node.Bind or Node.Guard or Node.Condition ||
					node is Node.Call(var called, _) && reaching.Contains(called)))
				{
					return;
				}

			if (prefix is null)
				prefix = leading;
			else if (!prefix.SequenceEqual(leading))
				return;

			guarded[i] = (head, inner, built);
		}

		if (prefix is null)
			return;

		// Something read after the call back on at least one of them: where nothing is, the cut at once already reads
		// the rule (Reentry).
		if (guarded.Values.All(one => ((Node.Sequence)one.Body).Nodes.Count == one.Head + 1))
			return;

		// A base to start from.
		if (guarded.Count == alternatives.Count)
			return;

		// What the rule reads, and what reads it, reads nothing but the text and the position.
		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Guard or Node.Condition or Node.Bind or Node.Same or Node.Counted or Node.Marked or
				Node.External { UsesContext: true } or Node.Choice { Selection: not null } ||
				node is Node.Repeat && _recoveries.ContainsKey(node) ||
				node is Node.Call { Arguments.Count: > 0 })
			{
				return;
			}

		foreach (var caller in reaching)
			if (_bodies.TryGetValue(caller, out var read) && NodeWalk.Descendants(read).Any(static node => node is Node.Guard or Node.Condition))
				return;

		if (_context is not null || _model.Context is not null)
			return;

		// A value built by every alternative, or by none and nothing captured.
		var builds = alternatives.All(static one => one is Node.Construct);

		if (!builds && NodeWalk.Descendants(body).Any(static node => node is Node.Capture or Node.Construct))
			return;

		if (builds && !_types.ContainsKey(rule))
			return;

		if (!builds && _types.ContainsKey(rule))
			return;

		// A base must not reach the rule before it has read something.
		for (var i = 0; i < alternatives.Count; i++)
			if (!guarded.ContainsKey(i) && ReachesFirst(alternatives[i], reaching))
				return;

		// Recursive on both sides: §4.3 refuses it, and so is it left.
		foreach (var (head, inner, _) in guarded.Values)
		{
			var operands = ((Node.Sequence)inner).Nodes;

			if (operands.Count > head + 1 && EndsWith(new Node.Sequence([.. operands.Skip(head + 1)]), rule))
				return;
		}

		var left  = Made(rule, "_Left");
		var plain = Made(rule, "_Plain");

		// The loop: the bases as they are, each recursive alternative as its tail after a call of the loop's own rule;
		// one with nothing after the call reads only what the entry it came back to reads, and is left out.
		var looped = new List<Node>();

		for (var i = 0; i < alternatives.Count; i++)
		{
			if (!guarded.TryGetValue(i, out var one))
			{
				looped.Add(Copied(alternatives[i]));

				continue;
			}

			var operands = ((Node.Sequence)one.Body).Nodes;

			if (operands.Count == one.Head + 1)
				continue;

			var call = operands[one.Head] is Node.Capture(var name, _)
				? (Node)new Node.Capture(name, new Node.Call(left, []))
				: new Node.Call(left, []);

			var step = new Node.Sequence([call, .. operands.Skip(one.Head + 1).Select(Copied)]);

			looped.Add(one.Built is { } built ? new Node.Construct(step, built.How) : step);
		}

		// The rule as written, its recognizer known to move: it is read only where it did not stand.
		var written = new List<Node>();

		for (var i = 0; i < alternatives.Count; i++)
		{
			if (!guarded.TryGetValue(i, out var one))
			{
				written.Add(alternatives[i]);

				continue;
			}

			var operands = ((Node.Sequence)one.Body).Nodes;
			var moved    = new Node.Sequence([.. operands.Select((node, at) => at < one.Head && node is Node.External external ? external with { Moves = true } : node)]);

			written.Add(one.Built is { } built ? new Node.Construct(moved, built.How) : moved);
		}

		_bodies[left]  = looped.Count == 1 ? looped[0] : new Node.Choice(looped);
		_bodies[plain] = written.Count == 1 ? written[0] : new Node.Choice(written);

		// Whether the prefix stands at the entry, and whether it does not: a recognizer asked so records nothing.
		Node Stands(bool stands)
		{
			return new Node.Sequence([.. prefix.Select(node => node is Node.External external ? external with { Stands = stands } : Copied(node))]);
		}

		Node NotStands()
		{
			var looks = prefix.Where(static node => node is not Node.External).ToList();

			return looks.Count == 0
				? Stands(false)
				: new Node.Choice([Stands(false), new Node.Lookahead(false, new Node.Sequence([.. looks.Select(Copied)]))]);
		}

		Node Handed(Node lead, RuleSymbol to)
		{
			var leads = lead is Node.Sequence(var parts) ? parts : [lead];

			return builds
				? new Node.Construct(new Node.Sequence([.. leads, new Node.Capture("item0", new Node.Call(to, []))]), Construction.Operand.Instance)
				: new Node.Sequence([.. leads, new Node.Call(to, [])]);
		}

		_bodies[rule] = new Node.Choice(
		[
			Handed(Stands(true), left),
			Handed(NotStands(), plain),
		]);

		_guarded[rule] = (left, plain);
	}

	/// <summary>Whether an alternative may call into <paramref name="reaching"/> before it has read a literal or a class.</summary>
	static bool ReachesFirst(Node alternative, HashSet<RuleSymbol> reaching)
	{
		var inner    = alternative is Node.Construct(var built, _) ? built : alternative;
		var operands = inner is Node.Sequence(var nodes) ? nodes : [inner];

		foreach (var operand in operands)
		{
			if (NodeWalk.Descendants(operand).Any(node => node is Node.Call(var called, _) && reaching.Contains(called)))
				return true;

			if (operand is Node.Literal { Text.Length: > 0 } or Node.Element)
				return false;
		}

		return false;
	}

	/// <summary>A rule made beside <paramref name="rule"/>, of its type and its trivia, named after it.</summary>
	RuleSymbol Made(RuleSymbol rule, string suffix)
	{
		var name = rule.Name + suffix;

		while (_rules.Exists(one => one.Namespace == rule.Namespace && one.Name == name))
			name += "_";

		var made = new RuleSymbol(name, rule.Namespace, rule.Declaration);

		_rules.Add(made);

		if (_types.TryGetValue(rule, out var type))
			_types[made] = type;

		if (_trivia.TryGetValue(rule, out var trivia))
			_trivia[made] = trivia;

		var origin = _lifted.TryGetValue(rule, out var outer) ? outer.Origin : rule;

		_lifted[made] = new Lift(rule, origin, Transparent: false);

		return made;
	}

	/// <summary>A copy of a node no pass has recorded anything against.</summary>
	static Node Copied(Node node)
	{
		return node switch
		{
			Node.Sequence (var nodes)              => new Node.Sequence([.. nodes.Select(Copied)]),
			Node.Choice   choice                   => choice.Rebuild([.. choice.Nodes.Select(Copied)]),
			Node.Atomic   (var body)               => new Node.Atomic(Copied(body)),
			Node.Repeat   (var body, var min, var max) => new Node.Repeat(Copied(body), min, max),
			Node.Lookahead look                    => look with { Body = Copied(look.Body) },
			Node.Capture  (var name, var body)     => new Node.Capture(name, Copied(body)),
			Node.Construct(var body, var how)      => new Node.Construct(Copied(body), how),
			Node.Literal  literal                  => literal with { },
			Node.Element  element                  => element with { },
			Node.External external                 => external with { },
			Node.Call     (var rule, var arguments) => new Node.Call(rule, [.. arguments.Select(Copied)]),
			_                                      => node,
		};
	}
}
