using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Realizes every `namespace (...)` header (§18, §19 of the rebinding spec)
/// by cloning the rules that can observe it into ordinary new <see cref="RuleSymbol"/>s.
/// This is the whole of the feature — everything downstream (regions, `ExecutionPlan`,
/// capture layout, materialization, emission) sees only a larger, ordinary
/// <c>graph.Rules</c> and needs no idea a namespace was ever there. Also hosts the
/// call-graph machinery <see cref="SpecializeWithGroup"/> reuses for expression- and
/// publication-scoped `with`, and the §14 type-compatibility check shared by all three.
/// </summary>
public sealed partial class GrammarNormalizer
{
	public const string IncompatibleRebinding = "GRAM4014";

	IReadOnlyList<Publication> _publications = [];

	/// <summary>
	/// Every `with` expression and every `namespace (...)` header, in one dependency order.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One pass per header, each computing its own affected set (§18 step 1) against its own
	/// fully-layered <see cref="GrammarNamespace.Rebindings"/> and cloning independently — an
	/// inner header is never built on top of an outer one's clones (§11). One pass per rule
	/// that holds a `with` (<see cref="SpecializeWithGroup"/>).
	/// </para>
	/// <para>
	/// A `with` a header's clones reach is specialized before that header, which must clone
	/// the rule with the `with` already applied; a `with` whose operand calls into a namespace
	/// with a header from outside it is specialized after that namespace, and composes on top
	/// of its header, as a publication's own `with` does (§5.1). Where each wants the other
	/// first there is no order, and the grammar is refused (GRAM4017), as two `with`s that
	/// reach each other are.
	/// </para>
	/// </remarks>
	void SpecializeWithSitesAndNamespaces()
	{
		_publications = _model.Publications;

		var sites = new List<GrammarNamespace>();

		CollectSites(_model.Root, sites);

		var groups = _pendingWith.GroupBy(site => site.Rule).ToList();

		if (sites.Count == 0 && groups.Count == 0)
			return;

		var graph = BuildCallGraph();

		// Before ordering, because a cycle is what has no order. Two rules whose sites
		// reach each other cannot both be specialized against the other already specialized,
		// and the walk below would settle it by whichever the loop met first — which is
		// document order, and not an answer to anything.
		if (groups.Count > 0 && RefuseCircularWith(groups, graph))
			groups = [];

		var order = InOneOrder(groups, sites, graph) ?? [.. sites];
		var remap = new Dictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>>();

		Dictionary<RuleSymbol, List<RuleSymbol>>? forward  = null;
		Dictionary<RuleSymbol, List<RuleSymbol>>? calledBy = null;

		foreach (var unit in order)
		{
			if (unit is GrammarNamespace site)
			{
				// Built once for every header that follows the last `with`, as it always
				// was: the headers clone from the bodies as written, never from one another's
				// clones (§11).
				if (forward is null || calledBy is null)
				{
					forward  = BuildCallGraph();
					calledBy = Reverse(forward);
				}

				remap[site] = SpecializeSite(site, forward, calledBy);

				// A `with` after this header may replace a parameterized rule whose instance the
				// header cloned, and has to know the clone for an instance of it.
				if (groups.Count > 0)
					foreach (var pair in remap[site])
						if (_origins.TryGetValue(pair.Key, out var origin) && !_origins.ContainsKey(pair.Value))
							_origins[pair.Value] = origin;

				continue;
			}

			var group = (IGrouping<RuleSymbol, WithSite>)unit;

			SpecializeWithGroup(group, remap);

			// A `with` rewrites a body in place: a header after it reads the graph afresh.
			forward  = null;
			calledBy = null;
		}

		if (sites.Count == 0)
			return;

		var declared = DeclaredClones(remap);

		_declaredClones = declared;
		_siteClones     = remap;

		RetargetCallsIntoSites(declared);
		RemapPublications(remap, declared);
	}

	/// <summary>
	/// Each site's clone of every rule the site itself declares, keyed by the original: what a
	/// call naming that rule means from anywhere.
	/// </summary>
	static Dictionary<RuleSymbol, RuleSymbol> DeclaredClones(
		Dictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> remap)
	{
		var declared = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var pair in remap)
			foreach (var clone in pair.Value)
				if (NearestSite(clone.Key.Namespace) == pair.Key)
					declared[clone.Key] = clone.Value;

		return declared;
	}

	/// <summary>
	/// Points every call made from outside a site, to a rule that site declares, at the site's
	/// clone of it — the body-level counterpart of <see cref="RemapPublications"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A site's clones are what its own rules mean: <c>Ns.E</c> resolves the bindings of
	/// <c>Ns</c>'s header wherever it is named from (§5.1). A publication was pointed at the clone,
	/// but a call in a rule body kept the original, so <c>R = Ns.E</c> read <c>E</c> as though
	/// the header had not been written, on every back end and with no diagnostic.
	/// </para>
	/// <para>
	/// A rule within the same site is left alone: whatever it calls that the site cloned, it
	/// reaches through its own clone, and the original is what nothing reaches once every
	/// call from outside has been moved. A body is rebuilt only where it has a call to move,
	/// so a grammar with no such call keeps every node it had.
	/// </para>
	/// </remarks>
	void RetargetCallsIntoSites(Dictionary<RuleSymbol, RuleSymbol> declared)
	{
		if (declared.Count == 0)
			return;

		RetargetArguments(declared);

		// A clone of a parameterized rule's instance is still that instance, and a later `with`
		// that replaces the rule has to recognize it as one: `parse R with (E = F)`, where `R`
		// now calls `Ns`'s clone of `E('!')`, replaces it with `F('!')`.
		foreach (var pair in declared)
			if (_origins.TryGetValue(pair.Key, out var origin) && !_origins.ContainsKey(pair.Value))
				_origins[pair.Value] = origin;

		foreach (var rule in _rules.ToList())
		{
			var site = NearestSite(rule.Namespace);

			if (_bodies.TryGetValue(rule, out var body) && CallsInto(body, declared, site))
				_bodies[rule] = Retargeted(body, declared, site);

			if (_trivia.TryGetValue(rule, out var trivia) && CallsInto(trivia, declared, site))
				_trivia[rule] = Retargeted(trivia, declared, site);
		}

		RetypeIntoSites(declared);
	}

	/// <summary>
	/// Moves the type of every rule whose type is a site's rule (§4.1 case 3, <c>R : Ns.E</c>)
	/// onto the site's clone, which is what a call to that rule now reads.
	/// </summary>
	void RetypeIntoSites(Dictionary<RuleSymbol, RuleSymbol> declared)
	{
		foreach (var rule in _rules.ToList())
		{
			var site = NearestSite(rule.Namespace);

			if (TypeFrom(rule) is ({ } produced, var isSequence) &&
				declared.TryGetValue(produced, out var clone) &&
				NearestSite(produced.Namespace) != site)
			{
				_produces[rule] = (clone, isSequence);
				_retyped.Add(rule);
			}
		}
	}

	/// <summary>What <see cref="DeclaredClones"/> answered: the clone every call to a site's rule now names.</summary>
	IReadOnlyDictionary<RuleSymbol, RuleSymbol> _declaredClones = EmptyClones;

	/// <summary>
	/// What each <c>with</c> expression cloned, for the type check of its bindings: keyed by the
	/// expression's own rebindings, the one thing its site and its check share.
	/// </summary>
	readonly Dictionary<IReadOnlyDictionary<RuleSymbol, RuleSymbol>, Dictionary<RuleSymbol, RuleSymbol>> _withClones = [];

	/// <summary>The rule each <c>with</c> expression is written in, keyed as <see cref="_withClones"/> is.</summary>
	readonly Dictionary<IReadOnlyDictionary<RuleSymbol, RuleSymbol>, RuleSymbol> _withHosts = [];

	/// <summary>
	/// What each publication's own <c>with</c> cloned, by the publication's place among those
	/// written, for the type check of its bindings.
	/// </summary>
	readonly Dictionary<int, Dictionary<RuleSymbol, RuleSymbol>> _publicationWithClones = [];

	/// <summary>What each site cloned, for the type check of its header's bindings.</summary>
	Dictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> _siteClones = [];

	/// <summary>
	/// Moves the calls an instance's saved arguments make into a site onto the site's clones.
	/// </summary>
	/// <remarks>
	/// An instance's arguments are where a later <c>with</c> rebuilds it from: <c>R =
	/// L(Ns.E)</c> rebound <c>with (L = F)</c> is <c>F</c> of what <c>L</c>'s instance now
	/// calls, <c>Ns</c>'s clone, and not the rule as written.
	/// </remarks>
	void RetargetArguments(Dictionary<RuleSymbol, RuleSymbol> declared)
	{
		foreach (var pair in _origins.ToList())
		{
			var site   = NearestSite(pair.Key.Namespace);
			var passed = pair.Value.Passed;

			if (passed.Any(one => one is not null && CallsInto(one, declared, site)))
				_origins[pair.Key] = (
					pair.Value.Origin,
					[.. passed.Select(one => one is not null && CallsInto(one, declared, site) ? Retargeted(one, declared, site) : one)],
					pair.Value.Counts,
					pair.Value.Values);
		}
	}

	/// <summary>
	/// What each clone was made from: the rule, or the clone, it copies. A rule's value read
	/// through any copy of it is that rule's value read (<see cref="Follows(RuleSymbol, IReadOnlyCollection{RuleSymbol})"/>).
	/// </summary>
	readonly Dictionary<RuleSymbol, RuleSymbol> _cloneOf = [];

	/// <summary>
	/// The rule whose own declaration says what <paramref name="rule"/>'s value is: itself,
	/// or, for a rule whose type is another rule's (§4.1 case 3), that one's, followed.
	/// </summary>
	RuleSymbol TypeSource(RuleSymbol rule)
	{
		var seen = new HashSet<RuleSymbol>();

		while (_produces.TryGetValue(rule, out var produces) && seen.Add(rule))
			rule = produces.Produces;

		return rule;
	}

	/// <summary>Whether <paramref name="rule"/> is <paramref name="from"/> or a copy of it, at any depth.</summary>
	bool Derives(RuleSymbol rule, RuleSymbol from)
	{
		for (RuleSymbol? at = rule; at is not null; at = _cloneOf.TryGetValue(at, out var made) ? made : null)
			if (ReferenceEquals(at, from))
				return true;

		return false;
	}

	/// <summary>The rules whose type <see cref="RetargetCallsIntoSites"/> moved onto a clone.</summary>
	readonly HashSet<RuleSymbol> _retyped = [];

	/// <summary>
	/// The rule whose value <paramref name="rule"/> takes (§4.1 case 3), as far as it is known
	/// before <see cref="ComputeTypes"/> has run: what an earlier pass recorded, or what the
	/// declared type names.
	/// </summary>
	/// <remarks>
	/// A rule that calls the clone takes its type from the clone too: <c>R : Ns.E = Ns.E</c>
	/// whose type stayed the original's built the replacement's value and declared the old
	/// one, and the consumer's build failed on C# nobody wrote.
	/// </remarks>
	(RuleSymbol? Produces, bool IsSequence) TypeFrom(RuleSymbol rule)
	{
		if (_produces.TryGetValue(rule, out var known))
			return known;

		if (rule.Declaration?.Type is { IsCSharp: false } declared &&
			!IsCSharpKeyword(declared.Name) &&
			!rule.Declaration.Params.Any(one => one.Name == declared.Name) &&
			rule.Namespace.LookupQualified(declared.Name) is { } produced)
			return (produced, declared.IsSequence);

		return (null, false);
	}

	/// <summary>Whether a node calls a rule another site than the caller's own cloned.</summary>
	bool CallsInto(Node node, Dictionary<RuleSymbol, RuleSymbol> declared, GrammarNamespace? site)
	{
		foreach (var called in DirectCalls(node))
			if (declared.ContainsKey(called) && NearestSite(called.Namespace) != site)
				return true;

		return false;
	}

	/// <summary>
	/// <paramref name="node"/> rebuilt with every call into another site moved to that site's
	/// clone; the caller's own site, where it has one, is left out of the map.
	/// </summary>
	Node Retargeted(Node node, Dictionary<RuleSymbol, RuleSymbol> declared, GrammarNamespace? site)
	{
		var moved = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var pair in declared)
			if (NearestSite(pair.Key.Namespace) != site)
				moved[pair.Key] = pair.Value;

		return CloneAndRewrite(node, NoTargets, moved, "");
	}

	/// <summary>
	/// The `with` rule-groups and the namespace sites, each after every one whose result it
	/// reads; null where a namespace and a `with` each need the other first (reported).
	/// </summary>
	/// <remarks>
	/// Where only one kind is present the order is the one each always had: the `with`s by
	/// <see cref="OrderByDependency"/>, and the sites as written. Nothing is asked of a site's
	/// reach then, so a grammar with no `with` expression is specialized exactly as before.
	/// </remarks>
	List<object>? InOneOrder(
		List<IGrouping<RuleSymbol, WithSite>> groups,
		List<GrammarNamespace> sites,
		Dictionary<RuleSymbol, List<RuleSymbol>> graph)
	{
		var withs = groups.Count == 0 ? [] : OrderByDependency(groups, graph);

		if (groups.Count == 0 || sites.Count == 0)
			return [.. withs, .. sites];

		var byRule  = groups.ToDictionary(group => group.Key);
		var needs   = new Dictionary<object, List<object>>();

		foreach (var group in groups)
		{
			var first = new List<object>();

			foreach (var site in group)
				foreach (var called in ReachableFromSeed(DirectCalls(site.Root), graph, site.Targets))
					if (byRule.TryGetValue(called, out var other) && other != group && !first.Contains(other))
						first.Add(other);

			foreach (var (_, _, into) in CallsIntoSites(group, graph))
				if (!first.Contains(into))
					first.Add(into);

			needs[group] = first;
		}

		foreach (var site in sites)
		{
			var reached = ReachableFromSeed(Seed(site), graph, site.Rebindings);

			needs[site] = [.. groups.Where(group => reached.Contains(group.Key))];
		}

		bool Reaches(object from, object to)
		{
			var seen    = new HashSet<object>();
			var pending = new Stack<object>([from]);

			while (pending.Count > 0)
			{
				var unit = pending.Pop();

				if (unit == to)
					return true;

				if (seen.Add(unit))
					foreach (var next in needs[unit])
						pending.Push(next);
			}

			return false;
		}

		var refused = false;

		foreach (var site in sites)
			foreach (IGrouping<RuleSymbol, WithSite> group in needs[site])
				if (Reaches(group, site))
				{
					refused = true;

					Report(
						CircularWith,
						$"'{group.Key.Name}' holds a 'with' that calls into namespace '{site.Name}' from " +
						$"outside it, and the namespace's header reaches '{group.Key.Name}', so neither can " +
						$"be specialized against the other already specialized. Write the rebinding inside " +
						$"the namespace, or on a publication ('parse … with (…)').",
						group.First().At);
				}

		if (refused)
			return null;

		var ordered = new List<object>(needs.Count);
		var visited = new HashSet<object>();

		void Visit(object unit)
		{
			if (!visited.Add(unit))
				return;

			foreach (var first in needs[unit])
				Visit(first);

			ordered.Add(unit);
		}

		foreach (var group in withs)
			Visit(group);

		foreach (var site in sites)
			Visit(site);

		return ordered;
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

			calls.AddRange(DirectCalls(_bodies[rule]));

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

		// An instance of a parameterized rule declared here is a rule of this site too, wherever
		// the call that made it is written: `Ns.E('!')` from outside reads `Ns`'s specialization
		// of `E('!')`, and only an instance this site cloned can be called as one.
		foreach (var instance in _specialized.Values)
			if (span.Contains(instance.Namespace))
				seed.Add(instance);

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

	/// <summary>
	/// Forward reachability from the seed, walked as the bindings will rewrite it: a
	/// call to a bound rule reaches its replacement, not the rule as written. Without
	/// that hop the replacement itself — reached only through the binding — never
	/// entered the set, so a replacement whose own body observes a sibling binding of
	/// the same header was left uncloned, and `with (A = B, Sep = Semi)` handed out a
	/// `B` still reading the unbound `Sep`. A call to a specialization of a bound
	/// parameterized rule reaches the replacement's specialization for the same
	/// arguments, built here on demand — its body is what the parse will actually run,
	/// and what the affected set must therefore see through.
	/// </summary>
	HashSet<RuleSymbol> ReachableFromSeed(
		HashSet<RuleSymbol> seed,
		Dictionary<RuleSymbol, List<RuleSymbol>> forward,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets)
	{
		var reachable = new HashSet<RuleSymbol>(seed);
		var pending   = new Queue<RuleSymbol>(seed);

		while (pending.Count > 0)
		{
			var rule = pending.Dequeue();

			foreach (var called in CallsOf(rule, forward))
			{
				var reached = Rebound(called, targets) ?? called;

				if (reachable.Add(reached))
					pending.Enqueue(reached);
			}
		}

		return reachable;
	}

	/// <summary>
	/// A rule's direct calls — off the prebuilt graph where it has an entry, off its
	/// body where it does not, which is every specialization instantiated during this
	/// very pass.
	/// </summary>
	IEnumerable<RuleSymbol> CallsOf(RuleSymbol rule, Dictionary<RuleSymbol, List<RuleSymbol>> forward)
	{
		if (forward.TryGetValue(rule, out var calls))
			return calls;

		if (!_bodies.TryGetValue(rule, out var body))
			return [];

		var fresh = new List<RuleSymbol>();

		fresh.AddRange(DirectCalls(body));

		return fresh;
	}

	/// <summary>
	/// What a call to <paramref name="called"/> resolves to under <paramref name="targets"/>,
	/// or null where the binding does not touch it: the replacement itself, or — for a
	/// specialization of a bound parameterized rule — the replacement's specialization
	/// for the same arguments (§5.1: the binding replaces the rule a call named; the
	/// call keeps its arguments).
	/// </summary>
	RuleSymbol? Rebound(RuleSymbol called, IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets)
	{
		if (targets.TryGetValue(called, out var replacement))
			return replacement;

		if (_origins.TryGetValue(called, out var origin) &&
			targets.TryGetValue(origin.Origin, out var bound))
			return ((Node.Call)Instantiate(bound, origin.Passed, origin.Counts, origin.Values)).Rule;

		return null;
	}

	/// <summary>
	/// The rules whose calls a binding rewrites — the bound rules themselves, plus
	/// every specialization of a bound parameterized one, since that is what a call in
	/// the graph actually names once §4.2 has instantiated it.
	/// </summary>
	HashSet<RuleSymbol> BoundCalls(IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets)
	{
		var bound = new HashSet<RuleSymbol>(targets.Keys);

		foreach (var pair in _origins)
			if (targets.ContainsKey(pair.Value.Origin))
				bound.Add(pair.Key);

		return bound;
	}

	/// <summary>
	/// Backward BFS from the direct callers of every bound target, through
	/// <paramref name="calledBy"/>, intersected with <paramref name="reachable"/> at every
	/// step — the set that can both reach a binding and be reached from where it applies.
	/// </summary>
	static HashSet<RuleSymbol> AffectedSet(
		IReadOnlyCollection<RuleSymbol>          bound,
		Dictionary<RuleSymbol, List<RuleSymbol>> calledBy,
		HashSet<RuleSymbol>                      reachable)
	{
		var affected = new HashSet<RuleSymbol>();
		var pending  = new Queue<RuleSymbol>();

		foreach (var target in bound)
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

		var reachable = ReachableFromSeed(Seed(site), forward, targets);
		var affected  = AffectedSet(BoundCalls(targets), calledBy, reachable);

		return affected.Count == 0 ? EmptyClones : CloneAffected(affected, targets, site.Name);
	}

	/// <summary>
	/// Every clone's RuleSymbol is allocated before any body is cloned: a body being
	/// cloned may call a rule not yet cloned, including itself (§10) — the same
	/// two-pass shape `Machine`'s own `_entries[rule] = Reserve(out _)` uses, and for
	/// the same reason. Shared by both extents §5.1 now has: a `namespace (...)` block's
	/// own site, and a `with (...)` expression's (§18/§20).
	/// </summary>
	Dictionary<RuleSymbol, RuleSymbol> CloneAffected(
		HashSet<RuleSymbol> affected,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		string siteName)
	{
		return CloneAffected(affected, targets, siteName, []);
	}

	/// <summary>
	/// The same, into clones already made for the same substitution: a rule already in
	/// <paramref name="cloneMap"/> is the clone it has, and only the rest are made.
	/// </summary>
	/// <remarks>
	/// Two readings under one substitution reach mostly the same rules, and a clone depends
	/// on nothing but the substitution and which rules it reaches — so the second is given the
	/// first one's clones rather than a copy of them, and a machine that reads one of them can
	/// read the other's publication from inside itself.
	/// </remarks>
	Dictionary<RuleSymbol, RuleSymbol> CloneAffected(
		HashSet<RuleSymbol> affected,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		string siteName,
		Dictionary<RuleSymbol, RuleSymbol> cloneMap)
	{
		var added = new List<RuleSymbol>(affected.Count);

		foreach (var rule in affected)
			if (!cloneMap.ContainsKey(rule))
			{
				cloneMap[rule] = new RuleSymbol(NameFor(rule, siteName), rule.Namespace, rule.Declaration);
				_cloneOf[cloneMap[rule]] = rule;
				added.Add(rule);
			}

		foreach (var rule in added)
		{
			var clone = cloneMap[rule];

			_bodies[clone] = CloneBody(_bodies[rule], targets, cloneMap, siteName);
			_rules.Add(clone);

			// The boundary trivia a whole parse of this rule wraps in (§4.5) — cloned and
			// rewritten the same way as any other node, so a `namespace N with (trivia = none)`
			// binding reaches it exactly like any other call.
			if (_trivia.TryGetValue(rule, out var trivia))
				_trivia[clone] = CloneAndRewrite(trivia, targets, cloneMap, siteName);

			// A rule that was itself an on-demand parameterized-rule specialization
			// (§4.2) carries its `_produces`/`_types` entry across too — the only place
			// the two specialization mechanisms interact, and it is a one-line carry, not
			// new logic. `ComputeTypes()` has not run yet, so these are the only entries
			// either dictionary can hold at this point.
			// A type taken from another rule follows that rule into this specialization, the way
			// the resolution below does: `R : Ns.E` published `with (E = F)` calls `F`, and has to
			// say it builds what `F` builds; and `Ns`'s `E : B` under `with (D = V)`, where the
			// header put `D` in `B`'s place, builds `V`. Copied unchanged, the clone declared the
			// type of a rule it no longer calls, and the C# did not compile.
			if (_produces.TryGetValue(rule, out var produces))
			{
				var replaced = targets.TryGetValue(produces.Produces, out var target) ? target : produces.Produces;

				_produces[clone] = (cloneMap.TryGetValue(replaced, out var cloned) ? cloned : replaced, produces.IsSequence);

				if (_retyped.Contains(rule))
					_retyped.Add(clone);
			}

			if (_types.TryGetValue(rule, out var type))
				_types[clone] = type;

			// §4.1 case 3: a rule whose type is another rule's takes it from whichever rule
			// this specialization put in that one's place. Resolved here rather than left to
			// `ComputeTypes`, which looks the name up in the rule's own namespace and would
			// find the original — the very rule this clone no longer calls.
			//
			// Without it a `parse Calc with (Value = DecimalNumber)` kept `Calc`'s original
			// type while its body built the replacement's, and the two disagreed only in the
			// emitted C#: no diagnostic here, and `CS0266` in the consumer's build about code
			// they did not write. Every clone is allocated before any body is cloned, so the
			// replacement's own clone is already in the map by the time this asks for it.
			if (!_produces.ContainsKey(clone) &&
				rule.Declaration?.Type is { IsCSharp: false } declared &&
				!IsCSharpKeyword(declared.Name) &&
				!rule.Declaration.Params.Any(one => one.Name == declared.Name) &&
				rule.Namespace.LookupQualified(declared.Name) is { } produced)
			{
				var replaced = targets.TryGetValue(produced, out var target) ? target : produced;

				_produces[clone] = (
					cloneMap.TryGetValue(replaced, out var cloned) ? cloned : replaced,
					declared.IsSequence);
			}
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
	/// <summary>A fresh <see cref="Node.Element"/> saying what this one says.</summary>
	/// <remarks>
	/// An element is a leaf, so a clone of one is a copy of its four fields — and it is
	/// copied rather than shared for the reason every other node is (§19). The one inside
	/// a <see cref="Node.Behind"/> is reached through this because a look-behind holds an
	/// element rather than a node, and the switches cannot recurse into it.
	/// </remarks>
	internal static Node.Element Same(Node.Element element)
	{
		return new(element.IsNegated, element.Ranges, element.Categories, element.References);
	}

	/// <summary>
	/// <see cref="CloneAndRewrite"/> of a whole rule body into a rule of its own, with bindings of
	/// its own.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A copy into another rule — a specialization, or a callee inlined into its caller — must not
	/// share a <see cref="TextBinding"/> with what it was copied from: everything after this tells
	/// two bindings apart by symbol, and two that shared one would be read as one. So each binding
	/// of the copy is a new symbol, and every reference and length in the copy reads the new one.
	/// </para>
	/// <para>
	/// Only here, and not in every <see cref="CloneAndRewrite"/>: a copy of part of a rule within
	/// the same rule — an alternative distributed over the rules a forwarder names, the operand of
	/// a <c>with</c> — may be bound on one side of the copy and read on the other, and keeps its
	/// symbols for that reason.
	/// </para>
	/// </remarks>
	Node CloneBody(
		Node body,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		Dictionary<RuleSymbol, RuleSymbol> cloneMap,
		string siteName)
	{
		var outer = _rebound;

		_rebound = [];

		var clone = CloneAndRewrite(body, targets, cloneMap, siteName);

		_rebound = outer;

		return clone;
	}

	/// <summary>
	/// The symbols of the body <see cref="CloneBody"/> is copying, each with its copy; null where
	/// what is copied keeps its symbols.
	/// </summary>
	Dictionary<TextBinding, TextBinding>? _rebound;

	/// <summary>The symbol a copy reads for one its original read.</summary>
	TextBinding Rebound(TextBinding binding)
	{
		if (_rebound is null)
			return binding;

		if (!_rebound.TryGetValue(binding, out var copy))
			_rebound[binding] = copy = binding.Copy();

		return copy;
	}

	CountBound Rebound(CountBound bound)
	{
		return bound is CountBound.Length(var binding, var at) ? new CountBound.Length(Rebound(binding), at) : bound;
	}

	Node CloneAndRewrite(
		Node node,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		Dictionary<RuleSymbol, RuleSymbol> cloneMap,
		string siteName)
	{
		var clone = node switch
		{
			Node.Empty                                                              => new Node.Empty    (),
			Node.Glue                                                               => Node.Glue.Instance,
			Node.Element  (var negated, var ranges, var categories, var references) => new Node.Element  (negated, ranges, categories, references),
			Node.Literal  (var text) { IgnoreCase: var ignoreCase }                 => new Node.Literal  (text) { IgnoreCase = ignoreCase },
			Node.Guard    (var text, var at)                                        => new Node.Guard    (Renaming(text), at),
			Node.External external                                                 => external with { },
			Node.Sequence (var nodes)                                               => new Node.Sequence ([.. nodes.Select(child => CloneAndRewrite(child, targets, cloneMap, siteName))]),
			Node.Choice   (var nodes)                                               => ((Node.Choice)node).Rebuild([.. nodes.Select(child => CloneAndRewrite(child, targets, cloneMap, siteName))]) with
			{
				Selection = ((Node.Choice)node).Selection is { } selected
					? selected with { Selector = (Node.Guard)CloneAndRewrite(selected.Selector, targets, cloneMap, siteName) } : null,
			},
			Node.Atomic   (var body)                                                => new Node.Atomic   (CloneAndRewrite(body, targets, cloneMap, siteName)),
			Node.Marked   (var body, var text)                                      => new Node.Marked   (CloneAndRewrite(body, targets, cloneMap, siteName), Renaming(text)),
			Node.Repeat   (var body, var min, var max)                              => new Node.Repeat   (CloneAndRewrite(body, targets, cloneMap, siteName), min, max),
			Node.Lookahead(var positive, var body)                                  => new Node.Lookahead(positive, CloneAndRewrite(body, targets, cloneMap, siteName)),
			Node.Behind   (var test)                                                => new Node.Behind   (Same(test)),
			Node.Reading  (var readings)                                            => new Node.Reading  (readings),
			Node.Capture  (var name, var body)                                      => new Node.Capture  (name, CloneAndRewrite(body, targets, cloneMap, siteName)),
			Node.Bind     (var binding, var body, var at)                           => new Node.Bind     (Rebound(binding), CloneAndRewrite(body, targets, cloneMap, siteName), at),
			Node.Same     (var binding, var at)                                     => new Node.Same     (Rebound(binding), at),
			Node.Counted  (var body, var min, var max)                              =>
				new Node.Counted(CloneAndRewrite(body, targets, cloneMap, siteName), Rebound(min), max is null ? null : Rebound(max)),
			Node.Construct(var body, var how)                                       =>
				new Node.Construct(
					CloneAndRewrite(body, targets, cloneMap, siteName),
					how is Construction.Expression written ? written with { Text = Renaming(written.Text) } : how),
			// Both sides are recognizers and a rebinding is exactly what a condition is
			// waiting for: `with (Version = "Sql2008")` is what gives `when Version is …`
			// an answer, so the condition has to be rewritten like anything else.
			Node.Condition(var test, var at)                                        =>
				new Node.Condition(
					Test.Mapped(test, one => CloneAndRewrite(one, targets, cloneMap, siteName)), at),
			// CallTo, not a bare `new Node.Call`: a rebinding's right side may be a
			// built-in nothing in the grammar happened to call yet (`with (trivia =
			// none)` when `none` is otherwise unused) — built-ins are registered on
			// demand (§3.1) at the one place that ordinarily does it, and rewriting a
			// call onto one bypasses that place unless this does it too.
			Node.Call     (var called, var arguments)                               =>
				CallTo(
					RewriteCall(called, targets, cloneMap, siteName),
					[.. arguments.Select(child => CloneAndRewrite(child, targets, cloneMap, siteName))]),
			_ => throw new InvalidOperationException($"Unhandled node kind: {node.GetType().Name}"),
		};

		if (_bounds.TryGetValue(node, out var bound))
			_bounds[clone] = bound;

		if (_recoveries.TryGetValue(node, out var recovery))
			_recoveries[clone] = recovery with
			{
				Sync = CloneAndRewrite(recovery.Sync, targets, cloneMap, siteName),
				Factory = recovery.Factory is { } factory ? Renaming(factory) : null,
			};

		return clone;
	}

	/// <summary>
	/// What the C# a clone carries is rewritten by while the clone is made, when anything is.
	/// </summary>
	/// <remarks>
	/// Set only around the cloning a publication's own <c>with</c> does
	/// (<see cref="Redirected"/>), which is the one extent whose clones have a publication to
	/// name. A field and not a parameter, because every level of the copy would carry it
	/// through to the three kinds of node that hold text.
	/// </remarks>
	Func<string, string>? _renaming;

	string Renaming(string text)
	{
		return _renaming is null ? text : _renaming(text);
	}

	RuleSymbol RewriteCall(
		RuleSymbol called,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		Dictionary<RuleSymbol, RuleSymbol> cloneMap,
		string siteName)
	{
		// A call to a specialization of a bound parameterized rule becomes the
		// replacement's specialization for the same arguments — the binding replaces
		// the rule a call named, and the call keeps its arguments (§5.1/§4.2).
		if (_origins.TryGetValue(called, out var origin) && targets.ContainsKey(origin.Origin))
			return RewrittenInstantiation(called, targets, cloneMap, siteName);

		// A bound call goes to its replacement — and to the replacement's clone where
		// one was made, because a replacement whose own body observes another binding
		// of the same header is itself in the affected set, and the plain rule would
		// hand back the unbound view §5.1's simultaneity promises away.
		if (targets.TryGetValue(called, out var replacement))
			return cloneMap.TryGetValue(replacement, out var rebound) ? rebound : replacement;

		return cloneMap.TryGetValue(called, out var clone) ? clone : called;
	}

	/// <summary>
	/// The replacement's specialization for a bound instance's own arguments, cloned
	/// under this site so that its body — the replacement's and the spliced arguments'
	/// alike — observes the same bindings as everything else the site reaches. The
	/// clone is registered before its body is rewritten, so a specialization that
	/// reaches itself resolves to its own clone instead of cloning for ever.
	/// </summary>
	RuleSymbol RewrittenInstantiation(
		RuleSymbol instance,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		Dictionary<RuleSymbol, RuleSymbol> cloneMap,
		string siteName)
	{
		var origin      = _origins[instance];
		var replacement = ((Node.Call)Instantiate(
			targets[origin.Origin], origin.Passed, origin.Counts, origin.Values)).Rule;

		if (cloneMap.TryGetValue(replacement, out var existing))
			return existing;

		var clone = new RuleSymbol(
			NameFor(replacement, siteName), replacement.Namespace, replacement.Declaration);

		cloneMap[replacement] = clone;
		_cloneOf[clone]       = replacement;

		_bodies[clone] = CloneBody(_bodies[replacement], targets, cloneMap, siteName);
		_rules.Add(clone);

		if (_trivia.TryGetValue(replacement, out var trivia))
			_trivia[clone] = CloneAndRewrite(trivia, targets, cloneMap, siteName);

		if (_produces.TryGetValue(replacement, out var produces))
			_produces[clone] = produces;

		if (_types.TryGetValue(replacement, out var type))
			_types[clone] = type;

		_origins[clone] = _origins[replacement];

		return clone;
	}

	/// <summary>
	/// Remaps a `parse`/`find` directive to the clone it meant, once one exists.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Two ways a directive can mean a specialization, and both are asked. Where it is
	/// written *inside* a bound namespace, the site is its own and what it names is
	/// whatever that site cloned, the rule's own namespace notwithstanding — the reason
	/// <see cref="Publication.DeclaredIn"/> exists at all.
	/// </para>
	/// <para>
	/// Where it is written *outside* and names a rule of a bound namespace
	/// — <c>parse Dialect.Select as ParseSelect</c> — the site is the rule's. Asking only
	/// the directive's own published the unspecialized rule and said nothing: a whole
	/// header of rebindings, a parser built as though none of them were written, and the
	/// same rebindings on the directive itself working perfectly. A dialect is exactly
	/// this shape — one header, and the entry points published beside it.
	/// </para>
	/// <para>
	/// A directive's own <c>with</c> names rules the same way, and is moved with them: once a
	/// call to <c>Ns.E</c> reads <c>Ns</c>'s clone, <c>parse R with (Ns.E = F)</c> has to
	/// replace the clone, or the binding finds no call left to replace and is dropped without a
	/// word.
	/// </para>
	/// </remarks>
	void RemapPublications(
		Dictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> remap,
		Dictionary<RuleSymbol, RuleSymbol>                                        declared)
	{
		var remapped = new List<Publication>(_model.Publications.Count);

		_publicationClones.Clear();

		foreach (var publication in _model.Publications)
		{
			var site  = NearestSite(publication.DeclaredIn);
			var clone = CloneAt(site, publication.Rule, remap);

			if (clone is null)
			{
				site  = NearestSite(publication.Rule.Namespace);
				clone = CloneAt(site, publication.Rule, remap);
			}

			// What the clone it now reads calls in place of each rule: the clones its own site
			// made, where it reads one, and the clones every call from outside names.
			var clones = clone is not null && site is not null ? remap[site] : null;

			_publicationClones.Add(clones);

			var moved = publication with
			{
				Rule       = clone ?? publication.Rule,
				Rebindings = Retargeted(publication.Rebindings, clones, declared),
			};

			remapped.Add(clone is null && ReferenceEquals(moved.Rebindings, publication.Rebindings) ? publication : moved);
		}

		_publications = remapped;
	}

	/// <summary>
	/// For each publication as written, the clones of the site whose clone it was moved onto;
	/// null where it reads no site's clone.
	/// </summary>
	readonly List<IReadOnlyDictionary<RuleSymbol, RuleSymbol>?> _publicationClones = [];

	/// <summary>
	/// What a rebinding of a publication names once the publication reads a site's clone: the
	/// site's own clone of the rule, else the clone a call from outside names, else the rule.
	/// </summary>
	/// <remarks>
	/// <c>parse Ns.G with (A = Y)</c>, where <c>A</c> is declared outside <c>Ns</c> and reached
	/// from <c>G</c>, has to replace <c>Ns</c>'s clone of <c>A</c>: that is what <c>G</c>'s clone
	/// calls, and a binding of the original found no call to replace.
	/// </remarks>
	RuleSymbol Moved(RuleSymbol rule, IReadOnlyDictionary<RuleSymbol, RuleSymbol>? clones)
	{
		return clones is not null && clones.TryGetValue(rule, out var clone) ? clone : Declared(rule);
	}

	/// <summary>
	/// A directive's rebindings with every rule a site declares, on either side, replaced by
	/// the site's clone of it; the same instance where none is.
	/// </summary>
	IReadOnlyDictionary<RuleSymbol, RuleSymbol> Retargeted(
		IReadOnlyDictionary<RuleSymbol, RuleSymbol>  rebindings,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol>? clones,
		Dictionary<RuleSymbol, RuleSymbol>           declared)
	{
		bool Touched(RuleSymbol rule)
		{
			return declared.ContainsKey(rule) || clones is not null && clones.ContainsKey(rule);
		}

		if (!rebindings.Any(pair => Touched(pair.Key) || Touched(pair.Value)))
			return rebindings;

		var moved = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var pair in rebindings)
			moved[Moved(pair.Key, clones)] = Moved(pair.Value, clones);

		return moved;
	}

	/// <summary>What one site made of a rule, or null where it made nothing of it.</summary>
	static RuleSymbol? CloneAt(
		GrammarNamespace? site,
		RuleSymbol rule,
		Dictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> remap)
	{
		return site is not null && remap.TryGetValue(site, out var cloneMap) &&
		cloneMap.TryGetValue(rule, out var clone) ? clone : null;
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

		// A replacement a `with` expression cloned is checked as it was before calls were moved;
		// one it did not is called as the namespace's clone now (`RetargetCallsIntoSites`).
		foreach (var pair in _model.WithOwnRebindings)
		{
			var cloned = _model.WithBindings.TryGetValue(pair.Key, out var targets) &&
				_withClones.TryGetValue(targets, out var made) ? made : null;

			var readers = cloned is null ? [] : new HashSet<RuleSymbol>(cloned.Values);

			if (targets is not null && _withHosts.TryGetValue(targets, out var host))
				readers.Add(host);

			foreach (var binding in pair.Value)
				CheckReplacement(
					binding,
					binding.Left,
					cloned is not null && cloned.ContainsKey(binding.Right) ? binding.Right : Declared(binding.Right),
					readers);
		}

		// A publication's rebindings were moved onto the clones a call names
		// (`RemapPublications`), so it is the clones' types that have to fit.
		for (var i = 0; i < _model.Publications.Count; i++)
		{
			var clones = i < _publicationClones.Count ? _publicationClones[i] : null;

			foreach (var binding in _model.Publications[i].OwnRebindings)
				CheckReplacement(
					binding,
					Moved(binding.Left, clones),
					Moved(binding.Right, clones),
					_publicationWithClones.TryGetValue(i, out var made) ? made.Values : []);
		}
	}

	void CheckRebindingReplacements(GrammarNamespace ns)
	{
		// A replacement this site cloned is checked as it was before calls were moved; one it did
		// not is what a call from here to `Ns.E` now reads, `Ns`'s clone.
		_siteClones.TryGetValue(ns, out var cloned);

		foreach (var binding in ns.OwnRebindings)
			CheckReplacement(
				binding,
				binding.Left,
				cloned is not null && cloned.ContainsKey(binding.Right) ? binding.Right : Declared(binding.Right),
				readers: null);

		foreach (var nested in ns.Nested)
			CheckRebindingReplacements(nested);
	}

	/// <summary>The clone a call to <paramref name="rule"/> names, or the rule where no site cloned it.</summary>
	RuleSymbol Declared(RuleSymbol rule)
	{
		return _declaredClones.TryGetValue(rule, out var clone) ? clone : rule;
	}

	/// <summary>
	/// Checks one binding, with the types of <paramref name="left"/> and <paramref name="right"/>:
	/// the rules the binding actually puts in and takes out of the parse, which a site's clone
	/// can stand for.
	/// </summary>
	/// <remarks>
	/// <paramref name="readers"/>: for a <c>with</c> or a publication, the rules its own
	/// specialization made or rewrote, whose reads of a copy of the replaced rule are asked after
	/// too (<see cref="Follows(RuleSymbol, IReadOnlyCollection{RuleSymbol})"/>); null for a
	/// header, whose check asks only who reads the rule itself — the copies of its rules are its
	/// own specialization, already of the replacement's type.
	/// </remarks>
	void CheckReplacement(ResolvedRebinding binding, RuleSymbol left, RuleSymbol right, IReadOnlyCollection<RuleSymbol>? readers)
	{
		// A parameterized rule has no lowered body of its own, so `_types` never carries
		// the base symbol — its declared type stands in where that type is a concrete C#
		// one. A parameter-dependent type (`: item`) needs no check at all: both sides
		// receive the same argument, so they produce the same rule's value by
		// construction.
		var expected = TypeOf(left, binding.Left) ?? DeclaredConcrete(binding.Left);

		if (expected is null)
			return;

		var actual = TypeOf(right, binding.Right)
			?? DeclaredConcrete(binding.Right)
				?? (binding.Right.Declaration is { Params.Count: > 0 } ? null : "string");

		if (actual is null)
			return;

		// Nothing is expecting the old type where everything that reads this rule takes its
		// own type from it: those follow the replacement, and a rebinding that changes the
		// type is what a `with` on a publication is for — one grammar, an `int` calculator
		// and a `decimal` one, differing in the rule that says what a number is.
		if (Follows(binding.Left, readers))
			return;

		if (!_resolver.IsAssignable(actual, expected))
			Report(
				IncompatibleRebinding,
				$"'{binding.Right}' cannot replace '{binding.Left}': expected a result " +
				$"compatible with '{expected}', found '{actual}'.",
				binding.At);
	}

	/// <summary>
	/// The type of the rule a binding actually uses, or of the rule as written where the one
	/// used has none of its own on record.
	/// </summary>
	/// <remarks>
	/// <c>with (B = Ns.E)</c> puts <c>Ns</c>'s clone of <c>E</c> in <c>B</c>'s place, and the
	/// clone's type is what has to fit: the original's can differ, when the header replaces what
	/// <c>E</c>'s type follows.
	/// </remarks>
	string? TypeOf(RuleSymbol used, RuleSymbol written)
	{
		if (_types.TryGetValue(used, out var type))
			return type;

		return _types.TryGetValue(written, out var declared) ? declared : null;
	}

	/// <summary>
	/// Whether everything that reads a rule's value takes its own type from that rule.
	/// </summary>
	/// <remarks>
	/// A capture is where a rule's value lands, and where it lands is what a replacement has
	/// to fit. Where every one of those landings belongs to a rule declared <c>: R</c> — §4.1
	/// case 3, "my value is R's" — there is no fixed type to fit: they are all following R,
	/// and they follow it to the replacement too. A rule that captures it into a declared C#
	/// type, or into a sequence, or hands it to a constructor, is expecting something
	/// particular, and then the check stands.
	/// </remarks>
	bool Follows(RuleSymbol replaced, IReadOnlyCollection<RuleSymbol>? readers)
	{
		return readers is not null ? Follows(replaced, true, [], readers) : FollowsHere(replaced);
	}

	/// <summary>The same, asked only of the captures of the rule itself and of its site's clone.</summary>
	bool FollowsHere(RuleSymbol replaced)
	{
		var read = false;

		_declaredClones.TryGetValue(replaced, out var clone);

		foreach (var rule in _rules)
		{
			if (!_bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
			{
				if (node is not Node.Capture(_, Node.Call(var called, _)) ||
					!ReferenceEquals(called, replaced) && (clone is null || !ReferenceEquals(called, clone)))
					continue;

				if (!_produces.TryGetValue(rule, out var produces) ||
					!ReferenceEquals(produces.Produces, called))
					return false;

				read = true;
			}
		}

		return read;
	}

	/// <summary>
	/// The same, of every copy of <paramref name="replaced"/> as well, and on through the rules
	/// that follow it: a rule that takes its type from the replaced one changes type with it,
	/// so whatever reads that rule has to follow too, or be expecting the old type.
	/// </summary>
	/// <remarks>
	/// <c>Ns</c>'s <c>E : B</c>, its header putting <c>D</c> in <c>B</c>'s place, follows
	/// <c>D</c>; <c>R : @decimal = x: (Ns.E with (D = S))</c> reads a copy of <c>E</c> into a
	/// fixed type. Asking only who reads <c>D</c> itself let a string replace a decimal, and the
	/// grammar failed later, on the name of a clone the author never wrote.
	/// </remarks>
	/// <param name="replaced">The rule whose readers are asked after.</param>
	/// <param name="top">Whether this is the replaced rule itself, which has to be read at all.</param>
	/// <param name="asked">The rules asked after already.</param>
	/// <param name="readers">
	/// The rules a read of a following rule's copy is asked after in: those this rebinding's own
	/// specialization made or rewrote. An unrelated rule reading the original keeps the original's
	/// type and expects nothing of the replacement.
	/// </param>
	bool Follows(RuleSymbol replaced, bool top, HashSet<RuleSymbol> asked, IReadOnlyCollection<RuleSymbol> readers)
	{
		if (!asked.Add(replaced))
			return true;

		var read = false;

		// A call from outside a site names the site's clone of its rule, and so does the type of
		// a rule that follows it (`RetargetCallsIntoSites`): a replacement of the rule is read
		// wherever the clone is.
		_declaredClones.TryGetValue(replaced, out var clone);

		foreach (var rule in _rules)
		{
			if (!_bodies.TryGetValue(rule, out var body) || !top && !readers.Contains(rule))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
			{
				if (node is not Node.Capture(_, Node.Call(var called, _)) ||
					!Derives(called, replaced) && (clone is null || !Derives(called, clone)))
					continue;

				if (!ReferenceEquals(TypeSource(rule), TypeSource(called)) ||
					!Follows(rule, false, asked, readers))
					return false;

				read = true;
			}
		}

		return read || !top;
	}

	/// <summary>A parameterized rule's declared type, where it is concrete C#.</summary>
	string? DeclaredConcrete(RuleSymbol rule)
	{
		return rule.Declaration is { Params.Count: > 0, Type: { } declared } &&
		(declared.IsCSharp || IsCSharpKeyword(declared.Name))
			? TypeName(declared)
			: null;
	}
}
