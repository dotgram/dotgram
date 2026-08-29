using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// Making an indirect left recursion direct, where the rules between are only names
/// for the ones they forward.
/// </summary>
/// <remarks>
/// <para>
/// §4.3 rewrites a rule that calls itself leftmost and refuses one that reaches itself
/// through another rule, because indirect recursion has arbitrarily many shapes and a
/// half-working transform is worse than a diagnostic. One of those shapes is not
/// arbitrary at all, and it is the one a layered grammar is written in:
/// </para>
/// <code>
/// Primary : @Expr = p: Call =&gt; @(p) | n: Number =&gt; @(n)
/// Call    : @Expr = target: Primary &amp; '(' &amp; args: Args &amp; ')' =&gt; @Invoke(target, args)
/// </code>
/// <para>
/// <c>Call</c> reaches itself through <c>Primary</c>, and <c>Primary</c> does nothing
/// but forward — the same identity <see cref="CollapseTransparent"/> proves for the
/// same shape, and for the same reason: an alternative that is a captured call handed
/// back unchanged means nothing the call does not already mean. So the leading
/// <c>Primary</c> is the choice of what it forwards, the alternative distributes over
/// that choice, and what is left is <c>Call</c> calling itself leftmost — direct
/// recursion, which §4.3's own rewrite then folds. Nothing new happens at run time:
/// this rewrites the grammar into a shape the language already had.
/// </para>
/// <para>
/// Distributing a choice out of the head of a sequence is sound here because calls are
/// transparent to backtracking (§11): <c>(X | Y) &amp; rest</c> and
/// <c>X &amp; rest | Y &amp; rest</c> try the same readings in the same order.
/// </para>
/// <para>
/// What stays refused is every other shape, and the reason is worth stating rather than
/// discovering: an intermediary that <em>does</em> something contributes its own
/// operands and its own <c>=&gt;</c> to the unfolded alternative, so the tail of the
/// fold would have to apply two constructions in order, against an accumulator that is
/// itself the result of one. That is a staged fold, and neither the value machinery nor
/// the arena has a shape for it — see docs/next.md.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Replace a leading call to a pass-through rule that forwards back to
	/// <paramref name="rule"/>, so that what was indirect is direct.
	/// </summary>
	/// <remarks>
	/// Asked of one rule at a time, immediately before its alternatives are classified,
	/// and answering with the alternatives to classify — the rewritten body is stored
	/// only where something actually changed, so a grammar with no such shape in it is
	/// left node-for-node as it was.
	/// </remarks>
	IReadOnlyList<Node> Unfolded(RuleSymbol rule, IReadOnlyList<Node> alternatives)
	{
		var forwards = Forwarders();

		if (forwards.Count == 0)
			return alternatives;

		List<Node>? unfolded = null;

		for (var i = 0; i < alternatives.Count; i++)
		{
			var expanded = Distribute(rule, alternatives[i], forwards);

			if (unfolded is null && (expanded.Count != 1 || !ReferenceEquals(expanded[0], alternatives[i])))
				unfolded = [.. alternatives.Take(i)];

			unfolded?.AddRange(expanded);
		}

		if (unfolded is null)
			return alternatives;

		_bodies[rule] = unfolded.Count == 1 ? unfolded[0] : new Node.Choice(unfolded);

		return unfolded;
	}

	/// <summary>
	/// One alternative, as the alternatives it becomes: itself, or one per source of the
	/// pass-through rule its head calls.
	/// </summary>
	IReadOnlyList<Node> Distribute(
		RuleSymbol rule, Node alternative, IReadOnlyDictionary<RuleSymbol, IReadOnlyList<RuleSymbol>> forwards)
	{
		var built = alternative as Node.Construct;
		var body  = built?.Body ?? alternative;

		if (body is not Node.Sequence(var operands) || operands.Count < 2)
			return [alternative];

		var head    = operands[0];
		var name    = head is Node.Capture(var captured, _) ? captured : null;
		var leading = (head is Node.Capture(_, var inner) ? inner : head) as Node.Call;

		if (leading is not { Arguments.Count: 0 } ||
			!forwards.TryGetValue(leading.Rule, out var sources) ||
			!sources.Contains(rule))
		{
			return [alternative];
		}

		// A forwarder that declares no type is a name for other rules and its own value
		// is the text it matched (§4.1 case 4). Captured, that text is what the capture
		// means — and a source with a value of its own would put that value there
		// instead. Left alone rather than quietly changed.
		if (name is not null && leading.Rule.Declaration?.Type is null)
			return [alternative];

		var made = new List<Node>(sources.Count);

		foreach (var source in sources)
		{
			var replaced = new List<Node>(operands.Count)
			{
				name is null ? CallTo(source, []) : new Node.Capture(name, CallTo(source, [])),
			};

			// The rest of the alternative is copied, node for node, and not shared: one
			// alternative becomes several, and everything downstream keys facts by node
			// identity — a capture's slot, a `recover`, a binding power — so a node
			// standing in two alternatives would have whichever was laid out second
			// clobber the first. The same reason §19 gives for cloning a whole rule
			// rather than the path to a call, and the same copy: identity-keyed entries
			// travel onto the copies.
			for (var i = 1; i < operands.Count; i++)
				replaced.Add(CloneAndRewrite(operands[i], NoTargets, [], "Unfolded"));

			Node made1 = new Node.Sequence(replaced);

			if (built is not null)
				made1 = built with { Body = made1 };

			Carry(alternative, made1);
			made.Add(made1);
		}

		return made;
	}

	/// <summary>A rewrite that replaces nothing, so the copy is only a copy.</summary>
	static readonly IReadOnlyDictionary<RuleSymbol, RuleSymbol> NoTargets =
		new Dictionary<RuleSymbol, RuleSymbol>();

	/// <summary>The identity-keyed facts a node hands to whatever replaces it.</summary>
	/// <remarks>
	/// Every pass before this one records what it worked out against the node it worked it
	/// out on, by reference — a binding power, a recovery, the loop a fold runs, the tail
	/// each of that fold's accumulators belongs to. A pass that rebuilds a node has to hand
	/// those on, or the fact is left naming a node no body holds any more.
	/// <para>
	/// The fold went unhanded for a long time, and what it cost is worth the line: a
	/// forwarder collapsed inside a left-recursive tail rebuilt the loop around it, the
	/// layout stopped recognizing that loop as the fold's, and every capture in the tails
	/// came out a sequence with the fold's own operand missing from the factory — C# the
	/// *consumer* could not compile, in a file they never wrote.
	/// </para>
	/// </remarks>
	void Carry(Node from, Node to)
	{
		if (ReferenceEquals(from, to))
			return;

		foreach (var annotation in Annotations)
			annotation.Move(from, to);
	}

	/// <summary>
	/// Everything recorded against which node, and how each of them follows a rebuild.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A list rather than a run of <c>if</c>s in <see cref="Carry"/>, because the run was
	/// the defect: what it cost when the fold was left out of it is written above, and
	/// nothing about that run said a fourth table had been added and a fifth had not. Adding
	/// one here is the only place it has to be said, and a test asks whether every table
	/// keyed by a node is in this list.
	/// </para>
	/// <para>
	/// Built once, on demand, and holding the tables themselves as well as the move: what
	/// the test compares is <em>which</em> tables are registered against which fields exist,
	/// and a delegate alone cannot be asked what it closed over.
	/// </para>
	/// </remarks>
	internal IReadOnlyList<(object Table, Action<Node, Node> Move)> Annotations =>
		_annotations ??=
		[
			Moving(_bounds),
			Moving(_powers),
			Moving(_recoveries),

			// Keyed by rule rather than by node, and named here all the same: the *values*
			// name nodes, so a rebuild has to reach into them. That is why the list holds a
			// move rather than a dictionary.
			(_folds, (from, to) =>
			{
				foreach (var rule in _folds.Keys.ToList())
					_folds[rule] = Moved(_folds[rule], from, to);
			}),

			(_climbing, (from, to) =>
			{
				foreach (var rule in _climbing.Keys.ToList())
					if (_climbing[rule].TryGetValue(from, out var level))
						_climbing[rule] = Alongside(_climbing[rule], to, level);
			}),
		];

	IReadOnlyList<(object Table, Action<Node, Node> Move)>? _annotations;

	/// <summary>One table keyed by node, and the move that follows a rebuild through it.</summary>
	static (object, Action<Node, Node>) Moving<T>(Dictionary<Node, T> table) =>
		(table, (from, to) =>
		{
			if (table.TryGetValue(from, out var found))
				table[to] = found;
		});

	/// <summary>A fold naming the node that replaced one of the two kinds it names.</summary>
	static Fold Moved(Fold fold, Node from, Node to)
	{
		if (ReferenceEquals(fold.Loop, from))
			fold = fold with { Loop = to };

		if (!fold.Accumulators.TryGetValue(from, out var accumulator))
			return fold;

		return fold with { Accumulators = Alongside(fold.Accumulators, to, accumulator) };
	}

	/// <summary>The same table with one more node in it, the node comparison unchanged.</summary>
	static Dictionary<Node, T> Alongside<T>(IReadOnlyDictionary<Node, T> table, Node node, T what)
	{
		var moved = new Dictionary<Node, T>(NodeIdentity.Instance);

		foreach (var pair in table)
			moved[pair.Key] = pair.Value;

		moved[node] = what;

		return moved;
	}

	/// <summary>
	/// Every rule that is only a name for what it forwards, and the rules it forwards to,
	/// with chains through further such rules already resolved.
	/// </summary>
	/// <remarks>
	/// The structural half of <see cref="CollapseTransparent"/>'s test, which cannot be
	/// asked here: this runs before <c>ComputeTypes</c>, so a rule's worked-out type is
	/// not available and the declared one stands in. Declared types must match exactly
	/// — the same text on the forwarder and on every source, or none on any of them —
	/// because inlining the call changes what type the capture around it has, and a
	/// forwarder that widens is doing something after all.
	/// </remarks>
	Dictionary<RuleSymbol, IReadOnlyList<RuleSymbol>> Forwarders()
	{
		var forwards = new Dictionary<RuleSymbol, IReadOnlyList<RuleSymbol>>();

		foreach (var rule in _rules)
		{
			if (rule.Declaration is not { Params.Count: 0 })
				continue;

			var sources = Sources(rule);

			if (sources is { Count: > 0 })
				forwards[rule] = sources;
		}

		if (forwards.Count == 0)
			return forwards;

		// A forwarder may forward to a forwarder. Resolved up front — and a ring of them,
		// which forwarding alone cannot make terminate, is dropped whole rather than
		// walked: `A = B, B = A` says nothing either way round.
		foreach (var rule in forwards.Keys.ToList())
		{
			var resolved = new List<RuleSymbol>();
			var ring     = false;

			void Resolve(RuleSymbol at, HashSet<RuleSymbol> path)
			{
				if (!path.Add(at))
				{
					ring = true;

					return;
				}

				if (forwards.TryGetValue(at, out var onward))
					foreach (var source in onward)
						Resolve(source, path);
				else
					resolved.Add(at);

				path.Remove(at);
			}

			Resolve(rule, []);

			if (ring)
				forwards.Remove(rule);
			else
				forwards[rule] = resolved;
		}

		return forwards;
	}

	/// <summary>
	/// What a rule forwards, or null where it does anything of its own. Both spellings:
	/// a valued rule hands a captured call back through its <c>=&gt;</c>, a valueless one
	/// is the call and nothing else.
	/// </summary>
	IReadOnlyList<RuleSymbol>? Sources(RuleSymbol rule)
	{
		var declared = rule.Declaration?.Type;
		var sources  = new List<RuleSymbol>();

		foreach (var alternative in Alternatives(_bodies[rule]))
		{
			RuleSymbol? source = alternative switch
			{
				Node.Construct(
					Node.Capture(var name, Node.Call(var valued, { Count: 0 })),
					Construction.Expression(var text, _)) when Forwards(text, name) => valued,
				Node.Call(var plain, { Count: 0 }) when declared is null            => plain,
				_                                                                  => null,
			};

			if (source is null || !SameDeclaredType(declared, source.Declaration?.Type))
				return null;

			sources.Add(source);
		}

		return sources;
	}

	/// <summary>Whether two rules declare the same type, textually, or neither declares one.</summary>
	static bool SameDeclaredType(TypeRef? one, TypeRef? other) =>
		one is null
			? other is null
			: other is not null &&
				one.IsSequence == other.IsSequence &&
				string.Equals(one.Name, other.Name, StringComparison.Ordinal);
}
