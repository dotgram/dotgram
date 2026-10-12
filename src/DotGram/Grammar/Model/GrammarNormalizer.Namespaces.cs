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

		var declared = DeclaredClones(remap);

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

			if (TypeFrom(rule) is ({ } produced, var isSequence) &&
				declared.TryGetValue(produced, out var clone) &&
				NearestSite(produced.Namespace) != site)
			{
				_produces[rule] = (clone, isSequence);
				_retyped.Add(rule);
			}
		}
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
			if (_produces.TryGetValue(rule, out var produces))
				_produces[clone] = produces;

			// A type `RetargetCallsIntoSites` moved onto a site's clone is §4.1 case 3 resolved
			// early, and follows a later replacement the way the resolution below does: `R : Ns.E`
			// published `with (E = F)` calls `F`, and has to say it builds what `F` builds.
			if (_retyped.Contains(rule))
			{
				var replaced = targets.TryGetValue(produces.Produces, out var target) ? target : produces.Produces;

				_produces[clone] = (cloneMap.TryGetValue(replaced, out var cloned) ? cloned : replaced, produces.IsSequence);
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

		foreach (var publication in _model.Publications)
		{
			var clone =
				CloneAt(NearestSite(publication.DeclaredIn), publication.Rule, remap) ??
				CloneAt(NearestSite(publication.Rule.Namespace), publication.Rule, remap);

			var moved = publication with
			{
				Rule       = clone ?? publication.Rule,
				Rebindings = Retargeted(publication.Rebindings, declared),
			};

			remapped.Add(clone is null && ReferenceEquals(moved.Rebindings, publication.Rebindings) ? publication : moved);
		}

		_publications = remapped;
	}

	/// <summary>
	/// A directive's rebindings with every rule a site declares, on either side, replaced by
	/// the site's clone of it; the same instance where none is.
	/// </summary>
	static IReadOnlyDictionary<RuleSymbol, RuleSymbol> Retargeted(
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> rebindings,
		Dictionary<RuleSymbol, RuleSymbol>          declared)
	{
		if (!rebindings.Any(pair => declared.ContainsKey(pair.Key) || declared.ContainsKey(pair.Value)))
			return rebindings;

		var moved = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var pair in rebindings)
			moved[declared.TryGetValue(pair.Key, out var key) ? key : pair.Key] =
				declared.TryGetValue(pair.Value, out var value) ? value : pair.Value;

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
		// A parameterized rule has no lowered body of its own, so `_types` never carries
		// the base symbol — its declared type stands in where that type is a concrete C#
		// one. A parameter-dependent type (`: item`) needs no check at all: both sides
		// receive the same argument, so they produce the same rule's value by
		// construction.
		var expected = _types.TryGetValue(binding.Left, out var known)
			? known
			: DeclaredConcrete(binding.Left);

		if (expected is null)
			return;

		var actual = _types.TryGetValue(binding.Right, out var declared)
			? declared
			: DeclaredConcrete(binding.Right)
				?? (binding.Right.Declaration is { Params.Count: > 0 } ? null : "string");

		if (actual is null)
			return;

		// Nothing is expecting the old type where everything that reads this rule takes its
		// own type from it: those follow the replacement, and a rebinding that changes the
		// type is what a `with` on a publication is for — one grammar, an `int` calculator
		// and a `decimal` one, differing in the rule that says what a number is.
		if (Follows(binding.Left))
			return;

		if (!_resolver.IsAssignable(actual, expected))
			Report(
				IncompatibleRebinding,
				$"'{binding.Right}' cannot replace '{binding.Left}': expected a result " +
				$"compatible with '{expected}', found '{actual}'.",
				binding.At);
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
	bool Follows(RuleSymbol replaced)
	{
		var read = false;

		foreach (var rule in _rules)
		{
			if (!_bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
			{
				if (node is not Node.Capture(_, Node.Call(var called, _)) ||
					!ReferenceEquals(called, replaced))
					continue;

				if (!_produces.TryGetValue(rule, out var produces) ||
					!ReferenceEquals(produces.Produces, replaced))
					return false;

				read = true;
			}
		}

		return read;
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
