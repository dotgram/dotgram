using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Realizes every `namespace (...)` header (§18, §19 of the rebinding spec)
/// by cloning the rules that can observe it into ordinary new <see cref="RuleSymbol"/>s.
/// This is the whole of the feature — everything downstream (regions, `ExecutionPlan`,
/// capture layout, materialization, emission) sees only a larger, ordinary
/// <c>graph.Rules</c> and needs no idea a namespace was ever there. Also hosts the
/// call-graph machinery <see cref="SpecializeWithSites"/> reuses for expression- and
/// publication-scoped `with`, and the §14 type-compatibility check shared by all three.
/// </summary>
public sealed partial class GrammarNormalizer
{
	public const string IncompatibleRebinding = "GRAM4014";

	IReadOnlyList<Publication> _publications = [];

	/// <summary>
	/// One pass per `namespace (...)` header, each computing its own affected set (§18
	/// step 1) against its own fully-layered <see cref="GrammarNamespace.Rebindings"/>
	/// and cloning independently — an inner header is never built on top of an outer
	/// one's clones (§11).
	/// </summary>
	void SpecializeNamespaces()
	{
		_publications = _model.Publications;

		var sites = new List<GrammarNamespace>();

		CollectSites(_model.Root, sites);

		if (sites.Count == 0)
			return;

		var forward  = BuildCallGraph();
		var calledBy = Reverse(forward);
		var remap    = new Dictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>>();

		foreach (var site in sites)
			remap[site] = SpecializeSite(site, forward, calledBy);

		RemapPublications(remap);
	}

	static void CollectSites(GrammarNamespace ns, List<GrammarNamespace> sites)
	{
		if (ns.OwnRebindings.Count > 0)
			sites.Add(ns);

		foreach (var nested in ns.Nested)
			CollectSites(nested, sites);
	}

	/// <summary>Every rule's own direct calls, read off its already-lowered body.</summary>
	Dictionary<RuleSymbol, List<RuleSymbol>> BuildCallGraph()
	{
		var forward = new Dictionary<RuleSymbol, List<RuleSymbol>>();

		foreach (var rule in _rules)
		{
			var calls = new List<RuleSymbol>();

			foreach (var node in NodeWalk.Descendants(_bodies[rule]))
				if (node is Node.Call(var called, _))
					calls.Add(called);

			forward[rule] = calls;
		}

		return forward;
	}

	static Dictionary<RuleSymbol, List<RuleSymbol>> Reverse(Dictionary<RuleSymbol, List<RuleSymbol>> forward)
	{
		var reverse = new Dictionary<RuleSymbol, List<RuleSymbol>>();

		foreach (var pair in forward)
			foreach (var called in pair.Value)
			{
				if (!reverse.TryGetValue(called, out var callers))
					reverse[called] = callers = [];

				callers.Add(pair.Key);
			}

		return reverse;
	}

	/// <summary>
	/// Every rule declared directly in <paramref name="site"/> or in a descendant that is
	/// not itself a specialization site, plus the target of every publication declared in
	/// that same span (needed when the body declares no rule of its own at all — §10's
	/// <c>parse Tree as BTree</c>).
	/// </summary>
	HashSet<RuleSymbol> Seed(GrammarNamespace site)
	{
		var span = new HashSet<GrammarNamespace>();

		CollectSpan(site, span);

		var seed = new HashSet<RuleSymbol>();

		foreach (var ns in span)
			foreach (var rule in ns.Rules.Values)
				seed.Add(rule);

		foreach (var publication in _model.Publications)
			if (span.Contains(publication.DeclaredIn))
				seed.Add(publication.Rule);

		return seed;
	}

	static void CollectSpan(GrammarNamespace ns, HashSet<GrammarNamespace> span)
	{
		span.Add(ns);

		foreach (var nested in ns.Nested)
			if (nested.OwnRebindings.Count == 0)
				CollectSpan(nested, span);
	}

	static HashSet<RuleSymbol> ReachableFromSeed(
		HashSet<RuleSymbol> seed, Dictionary<RuleSymbol, List<RuleSymbol>> forward)
	{
		var reachable = new HashSet<RuleSymbol>(seed);
		var pending   = new Queue<RuleSymbol>(seed);

		while (pending.Count > 0)
		{
			var rule = pending.Dequeue();

			if (!forward.TryGetValue(rule, out var calls))
				continue;

			foreach (var called in calls)
				if (reachable.Add(called))
					pending.Enqueue(called);
		}

		return reachable;
	}

	/// <summary>
	/// Backward BFS from the direct callers of every bound target, through
	/// <paramref name="calledBy"/>, intersected with <paramref name="reachable"/> at every
	/// step — the set that can both reach a binding and be reached from where it applies.
	/// </summary>
	static HashSet<RuleSymbol> AffectedSet(
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		Dictionary<RuleSymbol, List<RuleSymbol>>     calledBy,
		HashSet<RuleSymbol>                          reachable)
	{
		var affected = new HashSet<RuleSymbol>();
		var pending  = new Queue<RuleSymbol>();

		foreach (var target in targets.Keys)
			if (calledBy.TryGetValue(target, out var callers))
				foreach (var caller in callers)
					if (reachable.Contains(caller) && affected.Add(caller))
						pending.Enqueue(caller);

		while (pending.Count > 0)
		{
			var rule = pending.Dequeue();

			if (!calledBy.TryGetValue(rule, out var callers))
				continue;

			foreach (var caller in callers)
				if (reachable.Contains(caller) && affected.Add(caller))
					pending.Enqueue(caller);
		}

		return affected;
	}

	IReadOnlyDictionary<RuleSymbol, RuleSymbol> SpecializeSite(
		GrammarNamespace site,
		Dictionary<RuleSymbol, List<RuleSymbol>> forward,
		Dictionary<RuleSymbol, List<RuleSymbol>> calledBy)
	{
		var targets = site.Rebindings;

		if (targets.Count == 0)
			return EmptyClones;

		var reachable = ReachableFromSeed(Seed(site), forward);
		var affected  = AffectedSet(targets, calledBy, reachable);

		return affected.Count == 0 ? EmptyClones : CloneAffected(affected, targets, site.Name);
	}

	/// <summary>
	/// Every clone's RuleSymbol is allocated before any body is cloned: a body being
	/// cloned may call a rule not yet cloned, including itself (§10) — the same
	/// two-pass shape `Machine`'s own `_entries[rule] = Reserve(out _)` uses, and for
	/// the same reason. Shared by both extents §5.1 now has: a `namespace (...)` block's
	/// own site, and a `with (...)` expression's (§18/§20).
	/// </summary>
	IReadOnlyDictionary<RuleSymbol, RuleSymbol> CloneAffected(
		HashSet<RuleSymbol> affected,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		string siteName)
	{
		var cloneMap = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var rule in affected)
			cloneMap[rule] = new RuleSymbol(NameFor(rule, siteName), rule.Namespace, rule.Declaration);

		foreach (var rule in affected)
		{
			var clone = cloneMap[rule];

			_bodies[clone] = CloneAndRewrite(_bodies[rule], targets, cloneMap);
			_rules.Add(clone);

			// The boundary trivia a whole parse of this rule wraps in (§4.5) — cloned and
			// rewritten the same way as any other node, so a `namespace (trivia = none)`
			// binding reaches it exactly like any other call.
			if (_trivia.TryGetValue(rule, out var trivia))
				_trivia[clone] = CloneAndRewrite(trivia, targets, cloneMap);

			// A rule that was itself an on-demand parameterized-rule specialization
			// (§4.2) carries its `_produces`/`_types` entry across too — the only place
			// the two specialization mechanisms interact, and it is a one-line carry, not
			// new logic. `ComputeTypes()` has not run yet, so these are the only entries
			// either dictionary can hold at this point.
			if (_produces.TryGetValue(rule, out var produces))
				_produces[clone] = produces;

			if (_types.TryGetValue(rule, out var type))
				_types[clone] = type;
		}

		return cloneMap;
	}

	static readonly IReadOnlyDictionary<RuleSymbol, RuleSymbol> EmptyClones = new Dictionary<RuleSymbol, RuleSymbol>();

	/// <summary>
	/// A specialization's name: the rule, and the site it was cloned for — collision-
	/// avoided the same way an ordinary parameterized-rule specialization is named.
	/// </summary>
	string NameFor(RuleSymbol rule, string siteName)
	{
		// Not `@`: this name is embedded verbatim into generated C# identifiers
		// (`Recognize_<name>_Whole`), so it has to stay one itself — the same reason the
		// parameterized-rule specialization above uses `_` and not something more visibly
		// a separator.
		var name  = rule.Name + "_" + siteName;
		var taken = name;

		for (var i = 2; Named(taken); i++)
			taken = name + "_" + Text(i);

		return taken;
	}

	/// <summary>
	/// A deep copy of <paramref name="node"/> with every <see cref="Node.Call"/> site
	/// rewritten (§8, §18 step 2), and whatever <see cref="_bounds"/>/<see cref="_recoveries"/>
	/// entry the old node carried copied onto the new one — copied, not moved: the
	/// original rule keeps its own entry, unlike <see cref="Repeated"/>'s in-place rewrite.
	/// </summary>
	/// <remarks>
	/// Every node becomes a new object, even a childless <see cref="Node.Literal"/> or
	/// <see cref="Node.Element"/> — not just nodes on a path to a call — because
	/// everything keyed by <see cref="NodeIdentity"/> assumes one owning rule per node
	/// identity; sharing a subtree by reference between the original and the clone would
	/// let whichever rule's identity-keyed metadata is built second silently clobber the
	/// first's (§19).
	/// </remarks>
	Node CloneAndRewrite(
		Node node,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> cloneMap)
	{
		var clone = node switch
		{
			Node.Empty                                                              => new Node.Empty    (),
			Node.Element  (var negated, var ranges, var categories, var references) => new Node.Element  (negated, ranges, categories, references),
			Node.Literal  (var text) { IgnoreCase: var ignoreCase }                 => new Node.Literal  (text) { IgnoreCase = ignoreCase },
			Node.Guard    (var text, var at)                                        => new Node.Guard    (text, at),
			Node.External (var name) { HasValue: var hasValue }                     => new Node.External (name) { HasValue = hasValue },
			Node.Sequence (var nodes)                                               => new Node.Sequence ([.. nodes.Select(child => CloneAndRewrite(child, targets, cloneMap))]),
			Node.Choice   (var nodes)                                               => new Node.Choice   ([.. nodes.Select(child => CloneAndRewrite(child, targets, cloneMap))]),
			Node.Atomic   (var body)                                                => new Node.Atomic   (CloneAndRewrite(body, targets, cloneMap)),
			Node.Repeat   (var body, var min, var max)                              => new Node.Repeat   (CloneAndRewrite(body, targets, cloneMap), min, max),
			Node.Lookahead(var positive, var body)                                  => new Node.Lookahead(positive, CloneAndRewrite(body, targets, cloneMap)),
			Node.Capture  (var name, var body)                                      => new Node.Capture  (name, CloneAndRewrite(body, targets, cloneMap)),
			Node.Construct(var body, var how)                                       => new Node.Construct(CloneAndRewrite(body, targets, cloneMap), how),
			// CallTo, not a bare `new Node.Call`: a rebinding's right side may be a
			// built-in nothing in the grammar happened to call yet (`with (trivia =
			// none)` when `none` is otherwise unused) — built-ins are registered on
			// demand (§3.1) at the one place that ordinarily does it, and rewriting a
			// call onto one bypasses that place unless this does it too.
			Node.Call     (var called, var arguments)                               =>
				CallTo(
					RewriteTarget(called, targets, cloneMap),
					[.. arguments.Select(child => CloneAndRewrite(child, targets, cloneMap))]),
			_ => throw new InvalidOperationException($"Unhandled node kind: {node.GetType().Name}"),
		};

		if (_bounds.TryGetValue(node, out var bound))
			_bounds[clone] = bound;

		if (_recoveries.TryGetValue(node, out var recovery))
			_recoveries[clone] = recovery;

		return clone;
	}

	static RuleSymbol RewriteTarget(
		RuleSymbol called,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> cloneMap)
	{
		if (targets.TryGetValue(called, out var replacement))
			return replacement;

		return cloneMap.TryGetValue(called, out var clone) ? clone : called;
	}

	/// <summary>
	/// Remaps a `parse`/`find` directive declared inside a bound namespace to the clone it
	/// meant, once one exists — the reason <see cref="Publication.DeclaredIn"/> exists at
	/// all.
	/// </summary>
	void RemapPublications(Dictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> remap)
	{
		var remapped = new List<Publication>(_model.Publications.Count);

		foreach (var publication in _model.Publications)
		{
			var site = NearestSite(publication.DeclaredIn);

			remapped.Add(
				site is not null &&
				remap.TryGetValue(site, out var cloneMap) &&
				cloneMap.TryGetValue(publication.Rule, out var clone)
					? publication with { Rule = clone }
					: publication);
		}

		_publications = remapped;
	}

	static GrammarNamespace? NearestSite(GrammarNamespace from)
	{
		for (var at = from; at is not null; at = at.Parent)
			if (at.OwnRebindings.Count > 0)
				return at;

		return null;
	}

	/// <summary>
	/// §14: a rebinding's replacement must be valid wherever the target rule is used —
	/// checked identically for all three places §5.1 lets a `with (...)` appear: a
	/// namespace header, an expression, a publication.
	/// </summary>
	/// <remarks>
	/// Checked once <see cref="ComputeResults"/> has run, over every <c>OwnRebindings</c>
	/// entry actually written rather than any chain-resolved <c>Rebindings</c> form —
	/// checking only what a site wrote itself avoids re-reporting the same binding at
	/// every namespace-nesting depth it is visible from, or the same wrong link twice
	/// where a chain resolves through more than one hop.
	/// </remarks>
	void CheckRebindingReplacements()
	{
		CheckRebindingReplacements(_model.Root);

		foreach (var rebindings in _model.WithOwnRebindings.Values)
			foreach (var binding in rebindings)
				CheckReplacement(binding);

		foreach (var publication in _model.Publications)
			foreach (var binding in publication.OwnRebindings)
				CheckReplacement(binding);
	}

	void CheckRebindingReplacements(GrammarNamespace ns)
	{
		foreach (var binding in ns.OwnRebindings)
			CheckReplacement(binding);

		foreach (var nested in ns.Nested)
			CheckRebindingReplacements(nested);
	}

	void CheckReplacement(ResolvedRebinding binding)
	{
		if (_types.TryGetValue(binding.Left, out var expected))
		{
			var actual = _types.TryGetValue(binding.Right, out var declared) ? declared : "string";

			if (!_resolver.IsAssignable(actual, expected))
				Report(
					IncompatibleRebinding,
					$"'{binding.Right}' cannot replace '{binding.Left}': expected a result " +
					$"compatible with '{expected}', found '{actual}'.",
					binding.At);
		}
	}
}
