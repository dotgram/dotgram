using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Realizes every `with (...)` site (§5.1, §18/§20 of the rebinding spec) —
/// <c>namespace (...)</c>'s substitution, applied to one expression instead of a whole
/// block. Reuses <see cref="GrammarNormalizer"/>'s namespace machinery almost entirely:
/// the only genuinely new step is splicing a rewritten subtree back into the rule that
/// contains it, since a `with` declares no block of its own to specialize.
/// </summary>
public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// One pending `with` site, recorded while lowering the rule it sits in: which rule
	/// to splice back into, the identity of the operand's own already-lowered root, its
	/// resolved rebindings, and the name a clone made for it is given.
	/// </summary>
	internal sealed record WithSite(
		RuleSymbol Rule, Node Root, IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets, string Name);

	readonly List<WithSite> _pendingWith = [];
	int _withCounter;

	/// <summary>
	/// One pass per rule that contains at least one `with`, computing each site's own
	/// affected set (§18 step 1) exactly as a `namespace (...)` block does, then splicing
	/// the rewritten result back into that rule's own body — which is the one thing a
	/// `with` site needs that a namespace site does not, since it declares no block of its
	/// own to specialize.
	/// </summary>
	/// <remarks>
	/// Runs before <see cref="SpecializeNamespaces"/> (§5.1): a `with` mutates a rule's
	/// body in place, and an enclosing `namespace (...)` clone of that same rule must see
	/// the mutation already applied. The same has to hold between two `with` sites in
	/// different rules — `R2 = R1 with (C = D)` must see whatever `R1`'s own `with`
	/// already did to it, since a `with` rebinds what a rule actually resolves to, not a
	/// syntactic snapshot of it taken before this pass ran. So rule-groups are processed
	/// in dependency order (<see cref="OrderByDependency"/>, a rule after everything its
	/// own sites can reach), and the call graph is rebuilt fresh before each group rather
	/// than shared across the whole pass — sharing it once left a later group unable to
	/// see an earlier one's splice, silently dropping the later rebinding as a no-op
	/// wherever it only reached the earlier group's rule through a call the splice itself
	/// introduced.
	/// </remarks>
	void SpecializeWithSites()
	{
		if (_pendingWith.Count == 0)
			return;

		var groups = _pendingWith.GroupBy(site => site.Rule).ToList();
		var order  = OrderByDependency(groups, BuildCallGraph());

		foreach (var group in order)
		{
			var forward  = BuildCallGraph();
			var calledBy = Reverse(forward);

			var rewrites = new Dictionary<Node, (
				IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets,
				IReadOnlyDictionary<RuleSymbol, RuleSymbol> CloneMap)>(NodeIdentity.Instance);

			// Two or more sites sharing the same root only arise from direct stacking —
			// `(X with (A=B)) with (C=D)` — since `Group` is transparent at lowering and
			// both `with`s' operand lowers to the exact same node. Merged into one
			// combined rebinding set, later overriding earlier for a shared key — the
			// same child-overrides-parent layering `namespace (...)` nesting already uses
			// (`ChainResolve`) — rather than cloned in two separate passes: a second pass
			// computed against the pre-splice graph could not reach inside the clone the
			// first pass already made, since that clone is a new rule referenced only by
			// symbol, and nothing about a bare `Node.Call` carries a rule's body along
			// for a second rewrite to see.
			foreach (var atRoot in group.GroupBy(site => site.Root, NodeIdentity.Instance))
			{
				var merged = new Dictionary<RuleSymbol, RuleSymbol>();
				var name   = "";

				foreach (var site in atRoot)
				{
					foreach (var pair in site.Targets)
						merged[pair.Key] = pair.Value;

					name = site.Name;
				}

				var reachable = ReachableFromSeed(DirectCalls(atRoot.Key), forward);
				var affected  = AffectedSet(merged, calledBy, reachable);

				rewrites[atRoot.Key] = (merged,
					affected.Count == 0 ? EmptyClones : CloneAffected(affected, merged, name));
			}

			_bodies[group.Key] = SpliceWithSites(_bodies[group.Key], rewrites);
		}
	}

	/// <summary>
	/// <paramref name="groups"/>, ordered so that a rule is processed only after every
	/// other with-bearing rule its own sites can reach — the dependency <c>R2 = R1 with
	/// (C = D)</c> has on <c>R1</c>'s own <c>with</c> having already run, generalized to
	/// however deep the chain goes.
	/// </summary>
	/// <remarks>
	/// Read off <paramref name="forward"/> as it was before any group in this pass ran:
	/// which with-bearing rules a site's own operand can reach does not change once one
	/// of them is spliced, only what lies inside it does, so one snapshot answers the
	/// ordering question for the whole pass even though <see cref="SpecializeWithSites"/>
	/// rebuilds the graph again, per group, for the affected-set computation itself.
	/// A cycle between two with-bearing rules — each reachable from the other's own site —
	/// has no order that satisfies both. <c>visited</c> does not distinguish a rule still
	/// partway through recursing into its own dependencies from one already fully placed,
	/// and does not need to: either way a repeat visit is one side of the cycle simply
	/// giving up on seeing the other's splice, rather than looping forever.
	/// </remarks>
	static List<IGrouping<RuleSymbol, WithSite>> OrderByDependency(
		List<IGrouping<RuleSymbol, WithSite>> groups,
		Dictionary<RuleSymbol, List<RuleSymbol>> forward)
	{
		var byKey   = groups.ToDictionary(g => g.Key);
		var ordered = new List<IGrouping<RuleSymbol, WithSite>>(groups.Count);
		var visited = new HashSet<RuleSymbol>();

		void Visit(RuleSymbol rule)
		{
			if (!byKey.TryGetValue(rule, out var group) || !visited.Add(rule))
				return;

			foreach (var site in group)
				foreach (var called in ReachableFromSeed(DirectCalls(site.Root), forward))
					if (called != rule)
						Visit(called);

			ordered.Add(group);
		}

		foreach (var group in groups)
			Visit(group.Key);

		return ordered;
	}

	/// <summary>
	/// Every rule this node calls, at any depth — a with-site's own Seed (§18 step 1):
	/// what a `namespace` block names by declaring rules in its span, a `with` expression
	/// names by calling them directly in the one expression it wraps.
	/// </summary>
	static HashSet<RuleSymbol> DirectCalls(Node root)
	{
		var seed = new HashSet<RuleSymbol>();

		foreach (var node in NodeWalk.Descendants(root))
			if (node is Node.Call(var called, _))
				seed.Add(called);

		return seed;
	}

	/// <summary>
	/// Rebuilds <paramref name="node"/> unconditionally — same shape and the same reason
	/// as <see cref="CloneAndRewrite"/>'s own full rebuild, so an ancestor on the path
	/// to a registered root gets a new identity the same way a clone into a new rule
	/// would. At a node that is one of <paramref name="rewrites"/>' own roots, that
	/// site's already-merged rewrite is applied to the freshly rebuilt subtree.
	/// </summary>
	Node SpliceWithSites(
		Node node,
		Dictionary<Node, (
			IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets,
			IReadOnlyDictionary<RuleSymbol, RuleSymbol> CloneMap)> rewrites)
	{
		Node rebuilt = node switch
		{
			Node.Empty                                                              => new Node.Empty(),
			Node.Element  (var negated, var ranges, var categories, var references) => new Node.Element(negated, ranges, categories, references),
			Node.Literal  (var text) { IgnoreCase: var ignoreCase }                 => new Node.Literal(text) { IgnoreCase = ignoreCase },
			Node.Guard    (var text, var at)                                        => new Node.Guard(text, at),
			Node.External (var name) { HasValue: var hasValue }                     => new Node.External(name) { HasValue = hasValue },
			Node.Sequence (var nodes)                                               => new Node.Sequence([.. nodes.Select(child => SpliceWithSites(child, rewrites))]),
			Node.Choice   (var nodes)                                               => new Node.Choice([.. nodes.Select(child => SpliceWithSites(child, rewrites))]),
			Node.Atomic   (var body)                                                => new Node.Atomic(SpliceWithSites(body, rewrites)),
			Node.Repeat   (var body, var min, var max)                              => new Node.Repeat(SpliceWithSites(body, rewrites), min, max),
			Node.Lookahead(var positive, var body)                                  => new Node.Lookahead(positive, SpliceWithSites(body, rewrites)),
			Node.Capture  (var name, var body)                                      => new Node.Capture(name, SpliceWithSites(body, rewrites)),
			Node.Construct(var body, var how)                                       => new Node.Construct(SpliceWithSites(body, rewrites), how),
			// Left unrewritten here — no targets/cloneMap apply at this level. A call that
			// needs rewriting is inside some registered root's own subtree, and that is
			// what CloneAndRewrite below is for.
			Node.Call(var called, var arguments)                                    => new Node.Call(called, [.. arguments.Select(child => SpliceWithSites(child, rewrites))]),
			_ => throw new InvalidOperationException($"Unhandled node kind: {node.GetType().Name}"),
		};

		if (_bounds.TryGetValue(node, out var bound))
			_bounds[rebuilt] = bound;

		if (_recoveries.TryGetValue(node, out var recovery))
			_recoveries[rebuilt] = recovery;

		if (rewrites.TryGetValue(node, out var site))
			rebuilt = CloneAndRewrite(rebuilt, site.Targets, site.CloneMap);

		return rebuilt;
	}

	/// <summary>
	/// `parse Rule with (A = B) as Alias` — the same substitution, written directly on a
	/// publication instead of on the rule body it targets. Unlike either other extent,
	/// there is no node to splice into: the site's own seed is just the published rule
	/// itself, and specializing it either produces a clone to publish instead, or —
	/// when the rebinding cannot reach anything from there — leaves the publication
	/// exactly as it was, the same "no-op when nothing is affected" shape
	/// <see cref="SpecializeWithSites"/> and <see cref="SpecializeNamespaces"/> both have.
	/// </summary>
	/// <remarks>
	/// Runs after <see cref="SpecializeNamespaces"/>, deliberately: a publication's own
	/// `with` is the more locally written of the two, and composes on top of whatever an
	/// enclosing `namespace (...)` already did to it — the child-overrides-parent ordering
	/// nested namespace headers already use for a shared key, applied here between a block
	/// and the one directive inside it that names its own rebinding.
	/// </remarks>
	void SpecializePublicationWith()
	{
		if (_publications.All(publication => publication.Rebindings.Count == 0))
			return;

		var forward  = BuildCallGraph();
		var calledBy = Reverse(forward);
		var remapped = new List<Publication>(_publications.Count);

		foreach (var publication in _publications)
		{
			if (publication.Rebindings.Count == 0)
			{
				remapped.Add(publication);
				continue;
			}

			var reachable = ReachableFromSeed(new HashSet<RuleSymbol> { publication.Rule }, forward);
			var affected  = AffectedSet(publication.Rebindings, calledBy, reachable);

			var cloneMap = affected.Count == 0
				? EmptyClones
				: CloneAffected(affected, publication.Rebindings, "With" + (++_withCounter));

			remapped.Add(
				cloneMap.TryGetValue(publication.Rule, out var clone)
					? publication with { Rule = clone }
					: publication);
		}

		_publications = remapped;
	}
}
