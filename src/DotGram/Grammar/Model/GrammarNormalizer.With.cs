using System;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

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
	/// resolved rebindings, the name a clone made for it is given, and — for one recorded
	/// inside an instance a substitution made — what made the instance, for the refusal.
	/// </summary>
	internal sealed record WithSite(
		RuleSymbol Rule, Node Root, IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets, string Name, Location At)
	{
		public string? Made { get; init; }
	}

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
	/// Runs in one order with the namespaces (<see cref="SpecializeWithSitesAndNamespaces"/>,
	/// §5.1): before a namespace whose clones reach this rule, since a `with` mutates a rule's
	/// body in place and the clone must see the mutation already applied, and after a
	/// namespace the operand calls into from outside, whose header it composes on top of.
	/// The same has to hold between two `with` sites in
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
	void SpecializeWithGroup(
		IGrouping<RuleSymbol, WithSite> group,
		IReadOnlyDictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> sites)
	{
		var forward  = BuildCallGraph();
		var calledBy = Reverse(forward);
		var known    = _rules.Count;

		_composeOnHeaders = sites.Count > 0;

		var rewrites = new Dictionary<Node, (
			IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets,
			Dictionary<RuleSymbol, RuleSymbol> CloneMap,
			string Name)>(NodeIdentity.Instance);

		// The roots done so far, in order: a `with` inside another's operand was lowered first,
		// so an inner root comes before every root around it.
		var inner = new List<Rewrite>();

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
			var loops  = false;

			// Stacked `(X with (D = Y)) with (Y = P)`: the outer on top of the inner. It rebinds
			// what the inner put in place, `D` on to `P`, as one `with (D = Y, Y = P)` would; and a
			// name the inner replaced already is not called any more, so binding it again does
			// nothing. Where the two together send a name back to itself they loop, as one header
			// `with (D = Y, Y = D)` does, and are refused the same way.
			foreach (var site in atRoot)
			{
				var layered = new Dictionary<RuleSymbol, RuleSymbol>();

				foreach (var pair in merged)
					layered[pair.Key] = site.Targets.TryGetValue(pair.Value, out var over) ? over : pair.Value;

				foreach (var pair in site.Targets)
					if (!merged.ContainsKey(pair.Key))
						layered[pair.Key] = pair.Value;

				foreach (var looped in Looping(layered))
				{
					Report(
						GrammarBinder.CircularRebinding,
						$"'{looped}' is bound in a cycle: this 'with' sends back to it what the " +
						"'with' it is written around put in its place.",
						site.At);

					loops = true;
				}

				merged = layered;
				name   = site.Name;
			}

			if (loops)
				continue;

			_stacking = (atRoot.Last().At, $"the 'with' in '{group.Key.Name}'");

			var under  = inner.Where(done => Within(done.Root, atRoot.Key)).ToList();
			var scopes = Scopes(atRoot.Key, under);

			// Under this `with` are the `with`s inside its operand, on operands of their own, and
			// the header of a namespace the operand calls into from outside (§5.1): along each
			// call's path the lot is one header, and one that sends a name back to itself loops.
			if (LoopsStacked(group.Key, scopes, merged, forward))
				continue;

			merged = OnTopOfHeaders(merged, sites, NearestSite(group.Key.Namespace));

			// The operand's own calls are rebound too: into a namespace already specialized they
			// read its clone, and a call the `with` itself replaces reads the replacement — and what
			// either calls is what this `with` rebinds on top, as a publication's `with` does.
			var seed = DirectCalls(atRoot.Key);

			// And what the `with`s inside the operand put in place of each call, innermost first,
			// as the splice below will: this one is over them (§5.1), and rebinds what their clones
			// call. Read off the operand as written alone, the walk found `X` calling `A` under
			// `((X with (A = B)) & '!') with (B = C)`, nothing to rebind, and the outer was dropped.
			foreach (var scope in scopes)
				seed.UnionWith(scope.Calls);

			foreach (var called in seed.ToList())
				if (Rebound(called, merged) is { } replacement)
					seed.Add(replacement);

			// Rules made since the graph was built — an inner `with`'s clones, a parameterized
			// replacement's instance read through its header, a replacement's instance an inner
			// `with` rebuilt just now — are read again, with them: the walk reads a fresh rule's
			// body, but nothing above knows what calls it.
			if (_rules.Count != known)
			{
				forward  = BuildCallGraph();
				calledBy = Reverse(forward);
				known    = _rules.Count;
			}

			var reachable = ReachableFromSeed(seed, forward, merged);

			if (_rules.Count != known)
			{
				forward   = BuildCallGraph();
				calledBy  = Reverse(forward);
				known     = _rules.Count;
				reachable = ReachableFromSeed(seed, forward, merged);
			}

			var affected  = AffectedSet(BoundCalls(merged), calledBy, reachable);

			var cloneMap = affected.Count == 0 ? [] : CloneAffected(affected, merged, name);

			foreach (var site in atRoot)
			{
				_withClones[site.Targets] = cloneMap;
				_withHosts[site.Targets]  = group.Key;
			}

			rewrites[atRoot.Key] = (merged, cloneMap, name);
			inner.Add(new Rewrite(atRoot.Key, merged, cloneMap, name));
		}

		_cloning           = group.Key;
		_bodies[group.Key] = SpliceWithSites(_bodies[group.Key], rewrites);
		_cloning           = null;

		_composeOnHeaders = false;

		_specializedWith.UnionWith(group);
	}

	/// <summary>Every `with` site specialized, or refused with the reason (<see cref="RefuseLateWith"/>).</summary>
	readonly HashSet<WithSite> _specializedWith = [];

	/// <summary>
	/// One `with` root specialized: the operand, the rebindings as merged and composed on the
	/// headers, the clones made for it, and the name they carry.
	/// </summary>
	sealed record Rewrite(
		Node Root, IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets, Dictionary<RuleSymbol, RuleSymbol> CloneMap, string Name);

	/// <summary>
	/// The calls of an operand that stand under the same inner `with`s: the inner roots around
	/// them, innermost first, and each call as those put it in place.
	/// </summary>
	sealed record Scope(List<Rewrite> Chain, HashSet<RuleSymbol> Calls);

	/// <summary>
	/// The operand's calls by what stands between each and the operand: a sibling `with` is not
	/// over a call outside it, and two calls under different inner `with`s are two paths.
	/// </summary>
	/// <remarks>
	/// Each call is rewritten through its chain as the splice will rewrite it, innermost first:
	/// what the clones so made call is what the `with` over them rebinds. <see cref="RewriteCall"/>
	/// may make a replacement's instance here, into the inner's own clones, which the splice then
	/// finds made.
	/// </remarks>
	List<Scope> Scopes(Node root, List<Rewrite> under)
	{
		var scopes = new List<Scope>();

		var insides = under.Select(done => NodeWalk.ByIdentity(NodeWalk.Descendants(done.Root))).ToList();

		// Every node, not only a call: an operand that calls nothing, `('x' with (B = D)) & '!'`,
		// still stacks what is written around it on what is written on it.
		foreach (var node in NodeWalk.Descendants(root))
		{
			var chain = under.Where((done, at) => insides[at].Contains(node)).ToList();
			var scope = scopes.FirstOrDefault(one => one.Chain.Count == chain.Count && one.Chain.Zip(chain, ReferenceEquals).All(same => same));

			if (scope is null)
				scopes.Add(scope = new Scope(chain, []));

			if (node is not Node.Call(var called, _))
				continue;

			var read = called;

			foreach (var done in chain)
				read = RewriteCall(read, done.Targets, done.CloneMap, done.Name);

			scope.Calls.Add(read);
		}

		return scopes;
	}

	/// <summary>
	/// Whether <paramref name="node"/> is <paramref name="root"/> or a node inside it.
	/// </summary>
	static bool Within(Node node, Node root)
	{
		foreach (var one in NodeWalk.Descendants(root))
			if (ReferenceEquals(one, node))
				return true;

		return false;
	}

	/// <summary>
	/// Refuses every `with` site that was never specialized (GRAM4036): one recorded after the
	/// order was fixed, inside an instance of a parameterized rule made only while another
	/// substitution was specialized. It used to be dropped, and the parser read the operand as
	/// though the `with` had not been written.
	/// </summary>
	void RefuseLateWith()
	{
		foreach (var site in _pendingWith)
			if (_specializedWith.Add(site))
				Report(
					LateWith,
					$"This 'with' is inside '{Shown(site.Rule)}', an instance of a parameterized rule made " +
					(site.Made is null
						? "only while another rebinding was applied, "
						: $"only while {site.Made}, ") +
					"after every 'with' was put in order, and is not supported yet. Write the rebinding " +
					"where the instance is called, or on the rule's own declaration.",
					site.At);
	}

	/// <summary>
	/// Refuses a <c>with</c> that, stacked on what is under it — the <c>with</c>s inside its
	/// operand on operands of their own, and the header of a namespace its operand calls into from
	/// outside — sends a name back to itself (GRAM3011), as the one header
	/// <c>with (B = D, D = B)</c> is refused, and as two <c>with</c>s on one operand are.
	/// </summary>
	/// <remarks>
	/// Along each path: only the inner `with`s around a call, and only the namespaces that call
	/// reaches into from outside — the header of the namespace the <c>with</c> is written in goes
	/// on top of it, after it, and is not stacked under it. Each layer rebinds what the layers
	/// under it put in place, and a binding of a name a lower layer already replaced does nothing —
	/// so `((N.X with (D = Y)) & '!') with (D = B)` under `N`'s `B = D` is no loop: the inner
	/// `with` sent `D` on to `Y`, and the outer's `D` binds nothing; while a second `N.X` beside
	/// the inner `with`, with nothing between it and the outer, does loop.
	/// </remarks>
	bool LoopsStacked(
		RuleSymbol host,
		List<Scope> scopes,
		Dictionary<RuleSymbol, RuleSymbol> merged,
		Dictionary<RuleSymbol, List<RuleSymbol>> forward)
	{
		var loops = false;
		var told  = new HashSet<(GrammarNamespace?, RuleSymbol)>();

		foreach (var scope in scopes)
		{
			var bases = new List<(GrammarNamespace? Into, IReadOnlyDictionary<RuleSymbol, RuleSymbol> Header)> { (null, NoTargets) };

			foreach (var (_, _, into) in CallsInto(host, scope.Calls, merged, forward))
				if (bases.All(one => one.Into != into))
					bases.Add((into, into.Rebindings));

			foreach (var (into, header) in bases)
			{
				var layered = header.ToDictionary(static pair => pair.Key, static pair => pair.Value);

				foreach (var done in scope.Chain)
					layered = Layered(layered, done.Targets);

				layered = Layered(layered, merged);

				foreach (var looped in Looping(layered))
					if (told.Add((into, looped)))
					{
						Report(
							GrammarBinder.CircularRebinding,
							into is null
								? $"'{looped}' is bound in a cycle: this 'with' sends back to it what the 'with' it " +
								  "is written around put in its place."
								: $"'{looped}' is bound in a cycle: this 'with' sends back to it what the header of " +
								  $"namespace '{into.Name}', which its operand calls into, put in its place.",
							_stacking.At);

						loops = true;
					}
			}
		}

		return loops;
	}

	/// <summary>
	/// <paramref name="upper"/> stacked on <paramref name="lower"/> (§5.1): what the lower put in
	/// place is rebound by the upper, and a binding of a name the lower already replaced does
	/// nothing.
	/// </summary>
	static Dictionary<RuleSymbol, RuleSymbol> Layered(
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> lower, IReadOnlyDictionary<RuleSymbol, RuleSymbol> upper)
	{
		var layered = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var pair in lower)
			layered[pair.Key] = upper.TryGetValue(pair.Value, out var over) ? over : pair.Value;

		foreach (var pair in upper)
			if (!lower.ContainsKey(pair.Key))
				layered[pair.Key] = pair.Value;

		return layered;
	}

	/// <summary>
	/// Every name that stacked substitutions send back to itself: a key the bindings lead back
	/// to, through however many of them — as one header <c>with (B = D, D = B)</c> loops, and
	/// as <c>with (B = D, Y = Z, D = Y, Z = B)</c> does through four.
	/// </summary>
	static IEnumerable<RuleSymbol> Looping(Dictionary<RuleSymbol, RuleSymbol> layered)
	{
		foreach (var key in layered.Keys)
		{
			var path = new HashSet<RuleSymbol> { key };

			for (var at = layered[key]; layered.ContainsKey(at); at = layered[at])
				if (!path.Add(at))
				{
					if (ReferenceEquals(at, key))
						yield return key;

					break;
				}
		}
	}

	/// <summary>
	/// A <c>with</c>'s rebindings as they apply on top of the namespaces already specialized.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A call into one of those namespaces from outside it reads the namespace's clone (§5.1),
	/// so the <c>with</c> reads it that way too: each rule such a namespace declares is bound to
	/// its clone, and the <c>with</c>'s own rebindings go on top — where the namespace cloned a
	/// rule the <c>with</c> names, the clone is replaced too, and a replacement the namespace
	/// declares is its clone. <c>Ns.E with (D = Y)</c>, where <c>Ns</c> binds <c>B = D</c>, then
	/// reads <c>Ns</c>'s clone of <c>E</c>, which calls <c>D</c>, and replaces it — the same
	/// answer <c>parse Ns.E with (D = Y)</c> gives.
	/// </para>
	/// <para>
	/// As bindings and not as an edit of the rules the <c>with</c> reaches: those are shared,
	/// and a header specialized later reads them as written, whatever order the grammar
	/// declares its <c>with</c>s in. The namespace the <c>with</c> is written in is left out;
	/// its header goes on top of the <c>with</c>, after it.
	/// </para>
	/// </remarks>
	Dictionary<RuleSymbol, RuleSymbol> OnTopOfHeaders(
		Dictionary<RuleSymbol, RuleSymbol> targets,
		IReadOnlyDictionary<GrammarNamespace, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> sites,
		GrammarNamespace? own)
	{
		if (sites.Count == 0)
			return targets;

		var declared = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var pair in sites)
			if (!Encloses(pair.Key, own))
				foreach (var clone in pair.Value)
					if (NearestSite(clone.Key.Namespace) == pair.Key)
						declared[clone.Key] = clone.Value;

		var layered = new Dictionary<RuleSymbol, RuleSymbol>(declared);

		foreach (var pair in targets)
		{
			var replacement = declared.TryGetValue(pair.Value, out var cloned) ? cloned : pair.Value;

			layered[pair.Key] = replacement;

			foreach (var site in sites)
				if (!Encloses(site.Key, own) && site.Value.TryGetValue(pair.Key, out var clone))
					layered[clone] = replacement;
		}

		return layered;
	}

	/// <summary>
	/// What a <c>with</c>'s operand calls, each call also as the <c>with</c> rebinds it: a
	/// replaced operand is read as its replacement, which the <c>with</c>'s other bindings reach
	/// into, and which has to be specialized first wherever it holds a <c>with</c> of its own.
	/// </summary>
	HashSet<RuleSymbol> OperandSeed(WithSite site)
	{
		var seed = DirectCalls(site.Root);

		foreach (var called in seed.ToList())
			if (Rebound(called, site.Targets) is { } replacement)
				seed.Add(replacement);

		return seed;
	}

	/// <summary>
	/// Every call a <c>with</c>'s operand makes, at any depth, into a namespace with a header
	/// from outside it: the caller, the rule called, and the namespace.
	/// </summary>
	/// <remarks>
	/// Each call is read as the parse will make it: through the header of the namespace the
	/// walk is in, and then through the <c>with</c>'s own rebindings on top. Not through the
	/// header of the namespace the <c>with</c> itself is written in: that one is specialized
	/// after the <c>with</c>, on top of it. A header's replacement can itself be a rule of
	/// another namespace — <c>namespace Ns1 with (B = Ns2.F)</c> — and that call is one into
	/// <c>Ns2</c>, which the <c>with</c> then reads as <c>Ns2</c> makes it.
	/// </remarks>
	List<(RuleSymbol Caller, RuleSymbol Called, GrammarNamespace Into)> CallsIntoSites(
		RuleSymbol host,
		IEnumerable<WithSite> sites,
		Dictionary<RuleSymbol, List<RuleSymbol>> forward)
	{
		var into = new List<(RuleSymbol, RuleSymbol, GrammarNamespace)>();

		foreach (var site in sites)
			into.AddRange(CallsInto(host, DirectCalls(site.Root), site.Targets, forward));

		return into;
	}

	/// <summary>
	/// The same, from the calls given — a `with`'s operand's as written, or as the `with`s inside
	/// it put them in place — under the rebindings given.
	/// </summary>
	List<(RuleSymbol Caller, RuleSymbol Called, GrammarNamespace Into)> CallsInto(
		RuleSymbol host,
		IEnumerable<RuleSymbol> calls,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		Dictionary<RuleSymbol, List<RuleSymbol>> forward)
	{
		var into    = new List<(RuleSymbol, RuleSymbol, GrammarNamespace)>();
		var seen    = new HashSet<(RuleSymbol, GrammarNamespace?)>();
		var pending = new Queue<(RuleSymbol Caller, RuleSymbol Called, GrammarNamespace? In)>();
		var own     = NearestSite(host.Namespace);

		foreach (var called in calls)
		{
			pending.Enqueue((host, called, null));

			// A call the `with` replaces is read as its replacement, with the same caller.
			if (Rebound(called, targets) is { } replacement)
				pending.Enqueue((host, replacement, null));
		}

		while (pending.Count > 0)
		{
			var (caller, called, within) = pending.Dequeue();
			var headed                   = within is null ? called : Rebound(called, within.Rebindings) ?? called;
			var reached                  = Rebound(headed, targets) ?? headed;
			var namespaced               = NearestSite(reached.Namespace);

			if (namespaced is not null && namespaced != NearestSite(caller.Namespace) && !Encloses(namespaced, own))
				into.Add((caller, reached, namespaced));

			var onward = namespaced == own && namespaced is not null ? null : namespaced ?? within;

			if (!seen.Add((reached, onward)))
				continue;

			foreach (var next in CallsOf(reached, forward))
				pending.Enqueue((reached, next, onward));
		}

		return into;
	}

	/// <summary>
	/// Refuses two or more <c>with</c>-bearing rules whose sites reach each other.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A `with` rebinds what a rule actually resolves to, not a snapshot taken before this
	/// pass ran — so `R2 = R1 with (C = D)` has to be specialized after whatever `R1`'s own
	/// `with` did to it. Where two rules each want the other done first there is no such
	/// order, and until this check the walk below picked one by which the loop reached
	/// first. That is document order: moving a rule in the file changed what the parser did,
	/// silently and in a way nothing would ever have caught.
	/// </para>
	/// <para>
	/// Refused rather than settled, because settling it means choosing what the notation
	/// means and no one has. The shape that would answer it is a specialization keyed by
	/// (rule, bindings) with memoization and a placeholder symbol before the body exists,
	/// which turns this from a sequencing problem into ordinary graph construction — and
	/// which is a piece of design, not a fix. `docs/next.md` carries the reasoning.
	/// </para>
	/// <para>
	/// A rule reaching only itself is not this: there is exactly one thing to do and one
	/// order to do it in. What has no order is two.
	/// </para>
	/// </remarks>
	bool RefuseCircularWith(
		List<IGrouping<RuleSymbol, WithSite>> groups,
		Dictionary<RuleSymbol, List<RuleSymbol>> forward)
	{
		var bearing = new HashSet<RuleSymbol>(groups.Select(group => group.Key));

		// What each with-bearing rule's own sites can reach, kept to the rules that bear
		// one: everything else is passed through rather than specialized, so it can be on
		// the path and cannot be at either end of the cycle.
		var reaches = new Dictionary<RuleSymbol, HashSet<RuleSymbol>>();

		foreach (var group in groups)
		{
			var reached = new HashSet<RuleSymbol>();

			foreach (var site in group)
				foreach (var called in ReachableFromSeed(OperandSeed(site), forward, site.Targets))
					if (bearing.Contains(called) && called != group.Key)
						reached.Add(called);

			reaches[group.Key] = reached;
		}

		// Two rules reach each other exactly when they share a strongly connected component,
		// which is what a component *is* — so the question is asked of one rather than
		// worked out by closing the relation over itself. This closure was written by hand
		// two days before `CallGraph` existed and was the fourth walk over these edges,
		// which is the duplication that made the shared one worth having.
		var cycles = new CallGraph(
			reaches.Keys,
			rule => reaches.TryGetValue(rule, out var onward) ? onward : []);

		var refused = false;

		foreach (var group in groups)
			foreach (var other in reaches[group.Key])
				if (cycles.Together(group.Key, other) &&
					string.CompareOrdinal(group.Key.Name, other.Name) < 0)
				{
					refused = true;

					Report(
						CircularWith,
						$"'{group.Key.Name}' and '{other.Name}' each contain a 'with' that reaches the " +
						$"other, so neither can be specialized against the other already specialized. " +
						$"Give one of them the rebinding the other was going to apply, or write the " +
						$"substitution as a 'namespace Name with (...)' block around both.",
						group.Key.Declaration?.At ?? default);
				}

		return refused;
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
	/// ordering question for the whole pass even though <see cref="SpecializeWithGroup"/>
	/// rebuilds the graph again, per group, for the affected-set computation itself.
	/// A cycle between two with-bearing rules has no order that satisfies both, and is
	/// refused before this runs (<see cref="RefuseCircularWith"/>) — so <c>visited</c> here
	/// guards against nothing but a rule reaching itself, where there is one thing to do and
	/// one order to do it in. It used to guard the cycle too, by having one side give up on
	/// seeing the other's splice, which settled it by document order.
	/// </remarks>
	List<IGrouping<RuleSymbol, WithSite>> OrderByDependency(
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
				foreach (var called in ReachableFromSeed(OperandSeed(site), forward, site.Targets))
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
	HashSet<RuleSymbol> DirectCalls(Node root)
	{
		var seed = new HashSet<RuleSymbol>();

		foreach (var node in NodeWalk.Descendants(root))
		{
			if (node is Node.Call(var called, _)) seed.Add(called);
			if (_recoveries.TryGetValue(node, out var recovery)) seed.UnionWith(DirectCalls(recovery.Sync));
		}

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
			Dictionary<RuleSymbol, RuleSymbol> CloneMap,
			string Name)> rewrites)
	{
		Node rebuilt = node switch
		{
			Node.Empty                                                              => new Node.Empty(),
			Node.Glue                                                               => Node.Glue.Instance,
			Node.Element  (var negated, var ranges, var categories, var references) => new Node.Element(negated, ranges, categories, references),
			Node.Literal  (var text) { IgnoreCase: var ignoreCase }                 => new Node.Literal(text) { IgnoreCase = ignoreCase },
			Node.Guard    (var text, var at)                                        => new Node.Guard(text, at),
			Node.External external                                                 => external with { },
			// Flat again: an operand kept whole so that it could be found (`LowerSequence`) is
			// spliced back into the sequence around it.
			Node.Sequence (var nodes)                                               => new Node.Sequence([.. nodes.Select(child => SpliceWithSites(child, rewrites)).SelectMany(static child => child is Node.Sequence(var inner) ? inner : [child])]),
			Node.Choice   (var nodes)                                               => ((Node.Choice)node).Rebuild([.. nodes.Select(child => SpliceWithSites(child, rewrites))]) with
			{
				Selection = ((Node.Choice)node).Selection is { } selected
					? selected with { Selector = (Node.Guard)SpliceWithSites(selected.Selector, rewrites) } : null,
			},
			Node.Atomic   (var body)                                                => new Node.Atomic(SpliceWithSites(body, rewrites)),
			Node.Marked   (var body, var text)                                      => new Node.Marked(SpliceWithSites(body, rewrites), text),
			Node.Repeat   (var body, var min, var max)                              => new Node.Repeat(SpliceWithSites(body, rewrites), min, max),
			Node.Lookahead(var positive, var body)                                  => new Node.Lookahead(positive, SpliceWithSites(body, rewrites)),
			Node.Behind   (var test)                                                => new Node.Behind(Same(test)),
			Node.Reading  (var readings)                                            => new Node.Reading(readings),
			Node.Capture  (var name, var body)                                      => new Node.Capture(name, SpliceWithSites(body, rewrites)),

			// The same symbols: this rebuilds the rule in place, and a binding stays the one its
			// references read. Only a copy into another rule gets symbols of its own.
			Node.Bind     (var binding, var body, var at)                           => new Node.Bind(binding, SpliceWithSites(body, rewrites), at),
			Node.Same     (var binding, var at)                                     => new Node.Same(binding, at),
			Node.Counted  (var body, var min, var max)                              => new Node.Counted(SpliceWithSites(body, rewrites), min, max),
			Node.Construct(var body, var how)                                       => new Node.Construct(SpliceWithSites(body, rewrites), how),
			Node.Condition(var test, var at)                                        =>
				new Node.Condition(Test.Mapped(test, one => SpliceWithSites(one, rewrites)), at),
			// Left unrewritten here — no targets/cloneMap apply at this level. A call that
			// needs rewriting is inside some registered root's own subtree, and that is
			// what CloneAndRewrite below is for.
			Node.Call(var called, var arguments)                                    => new Node.Call(called, [.. arguments.Select(child => SpliceWithSites(child, rewrites))]),
			_ => throw new InvalidOperationException($"Unhandled node kind: {node.GetType().Name}"),
		};

		if (_bounds.TryGetValue(node, out var bound))
			_bounds[rebuilt] = bound;

		Placed(node, rebuilt);

		if (_recoveries.TryGetValue(node, out var recovery))
			_recoveries[rebuilt] = recovery with { Sync = SpliceWithSites(recovery.Sync, rewrites) };

		if (rewrites.TryGetValue(node, out var site))
			rebuilt = CloneAndRewrite(rebuilt, site.Targets, site.CloneMap, site.Name);

		return rebuilt;
	}

	/// <summary>
	/// `parse Rule with (A = B) as Alias` — the same substitution, written directly on a
	/// publication instead of on the rule body it targets. Unlike either other extent,
	/// there is no node to splice into: the site's own seed is just the published rule
	/// itself, and specializing it either produces a clone to publish instead, or —
	/// when the rebinding cannot reach anything from there — leaves the publication
	/// exactly as it was, the same "no-op when nothing is affected" shape
	/// <see cref="SpecializeWithGroup"/> and <see cref="SpecializeWithSitesAndNamespaces"/> both have.
	/// </summary>
	/// <remarks>
	/// Runs after <see cref="SpecializeWithSitesAndNamespaces"/>, deliberately: a publication's own
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
		var builtAt  = _headerClones;
		var written  = _publications;
		var remapped = new List<Publication>(_publications.Count);
		var pending  = new Queue<Publication>(written);

		// What each substitution was cloned into, shared by every reading made under it; and
		// which method reads a rule under a substitution, the author's first and then the
		// ones an action's name asked for (`Redirected`).
		var clonings = new List<(IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets, Dictionary<RuleSymbol, RuleSymbol> Map, string Name)>();
		var readers  = new List<(RuleSymbol Rule, IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets, string Method)>();

		foreach (var publication in written)
			if (publication.Rebindings.Count > 0)
				readers.Add((publication.Rule, publication.Rebindings, publication.MethodName));

		// Numbered afresh, in the order the publications are written: the grammar's own
		// reading is whatever number the first publication that substitutes nothing gets.
		_readings.Clear();

		var dequeued = 0;

		_composeOnHeaders = _siteClones.Count > 0;

		while (pending.Count > 0)
		{
			var publication = pending.Dequeue();
			var at          = dequeued++;

			_stacking = (publication.At, $"the 'with' of '{publication.MethodName}'");

			var (asked, cloned) = Asked(publication);
			var numbered        = publication with { Reading = Reading(asked, publication) };

			if (cloned.Count == 0)
			{
				remapped.Add(numbered);
				continue;
			}

			var reachable = ReachableFromSeed(new HashSet<RuleSymbol> { numbered.Rule }, forward, cloned);

			// As in a `with` expression: clones made reading a replacement through its header
			// are read again with the graph that has them.
			if (builtAt != _headerClones)
			{
				forward   = BuildCallGraph();
				calledBy  = Reverse(forward);
				builtAt   = _headerClones;
				reachable = ReachableFromSeed(new HashSet<RuleSymbol> { numbered.Rule }, forward, cloned);
			}

			var affected  = AffectedSet(BoundCalls(cloned), calledBy, reachable);

			if (affected.Count == 0)
			{
				remapped.Add(numbered);
				continue;
			}

			var cloning = clonings.FirstOrDefault(one => Same(one.Targets, cloned));

			if (cloning.Map is null)
			{
				cloning = (cloned, [], "With" + (++_withCounter));
				clonings.Add(cloning);
			}

			_renaming = text => Redirected(text, publication.Rebindings, written, readers, pending);

			CloneAffected(affected, cloned, cloning.Name, cloning.Map);

			_renaming = null;

			if (at < written.Count)
				_publicationWithClones[at] = cloning.Map;

			remapped.Add(
				cloning.Map.TryGetValue(numbered.Rule, out var clone)
					? numbered with { Rule = clone }
					: numbered);
		}

		_composeOnHeaders = false;
		_publications     = remapped;
	}

	/// <summary>
	/// An action's or a guard's C#, as a clone made under <paramref name="targets"/> means it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A construction that reads a piece of the input again names a publication to read it
	/// with — <c>TryParseHole</c>, <c>ParseBody</c> — and a clone carries that text across
	/// unchanged. Left so, the clone reads its own rules under the substitution and hands
	/// every piece it reads again to the rules without it: <c>parse Lambda with (Word =
	/// AsciiWord)</c> read the lambda in ASCII and the holes of its strings in Unicode. The
	/// name means that publication under the substitution the clone was made for, so it is
	/// spelled as that one: the author's own where one publishes the same rule under the
	/// same substitution, and a private one made here where none does.
	/// </para>
	/// <para>
	/// A name is redirected only where the substitution changes what the publication reads.
	/// One it leaves alone reads the same language, and its name is left alone with it, so a
	/// grammar with nothing to redirect compiles to what it did. What counts as the name is
	/// what <see cref="ICSharpScanner.FreeNames"/> says is free: <c>Owner.TryParseHole</c> is a
	/// member of something else and is not read as this grammar's publication.
	/// </para>
	/// <para>
	/// A made publication is queued behind the rest and specialized like one the author
	/// wrote, so what its own clones name is redirected in turn; <paramref name="readers"/>
	/// is what ends that, since a rule under a substitution is given one method however many
	/// actions ask for it.
	/// </para>
	/// </remarks>
	string Redirected(
		string text,
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets,
		IReadOnlyList<Publication> written,
		List<(RuleSymbol Rule, IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets, string Method)> readers,
		Queue<Publication> pending)
	{
		if (_scanner?.FreeNames(text) is not { Count: > 0 } free)
			return text;

		Dictionary<string, string>? names = null;

		foreach (var name in free)
			foreach (var publication in written)
			{
				var tried = name == "Try" + publication.MethodName;

				if (!tried && name != publication.MethodName)
					continue;

				var merged = new Dictionary<RuleSymbol, RuleSymbol>();

				foreach (var pair in publication.Rebindings)
					merged[pair.Key] = pair.Value;

				foreach (var pair in targets)
					merged[pair.Key] = pair.Value;

				if (!BoundCalls(targets).Overlaps(Read(publication.Rule, merged)))
					break;

				var reader = readers.FirstOrDefault(one => one.Rule.Equals(publication.Rule) && Same(one.Targets, merged));

				if (reader.Method is null)
				{
					reader = (publication.Rule, merged, Unpublished(publication.MethodName, written, readers));
					readers.Add(reader);

					pending.Enqueue(publication with
					{
						Rebindings    = merged,
						OwnRebindings = [],
						MethodName    = reader.Method,
						Access        = PublishAccess.Private,
					});
				}

				if (reader.Method != publication.MethodName)
					(names ??= new Dictionary<string, string>(StringComparer.Ordinal))[name] = tried ? "Try" + reader.Method : reader.Method;

				break;
			}

		return names is null ? text : _scanner.Renamed(text, names) ?? text;
	}

	/// <summary>A method name for a made publication that no other publication has.</summary>
	string Unpublished(
		string method,
		IReadOnlyList<Publication> written,
		List<(RuleSymbol Rule, IReadOnlyDictionary<RuleSymbol, RuleSymbol> Targets, string Method)> readers)
	{
		while (true)
		{
			var taken = method + "_With" + (++_withCounter);

			if (written.All(one => one.MethodName != taken) && readers.All(one => one.Method != taken))
				return taken;
		}
	}

	/// <summary>
	/// A publication's rebindings in two: those only a condition asks about, and the rest.
	/// </summary>
	/// <remarks>
	/// A rule no parse of this publication calls — `Version`, which only `when Version is …`
	/// ever names — changes nothing a clone would carry. What it changes is how the conditions
	/// are answered, and that is answered per reading (<see cref="DecideIntersections"/>)
	/// without a rule of the grammar copied for it.
	/// </remarks>
	(Dictionary<RuleSymbol, RuleSymbol> Asked, Dictionary<RuleSymbol, RuleSymbol> Cloned) Asked(
		Publication publication)
	{
		var asked  = new Dictionary<RuleSymbol, RuleSymbol>();
		var cloned = new Dictionary<RuleSymbol, RuleSymbol>();

		if (publication.Rebindings.Count == 0)
			return (asked, cloned);

		var read = Read(publication.Rule, publication.Rebindings);

		foreach (var pair in publication.Rebindings)
		{
			var bound = BoundCalls(new Dictionary<RuleSymbol, RuleSymbol> { [pair.Key] = pair.Value });

			if (bound.Overlaps(read))
				cloned[pair.Key] = pair.Value;
			else
				asked[pair.Key] = pair.Value;
		}

		return (asked, cloned);
	}

	/// <summary>
	/// Every rule a parse from here calls, conditions aside, as the rebindings rewrite the calls.
	/// </summary>
	HashSet<RuleSymbol> Read(RuleSymbol seed, IReadOnlyDictionary<RuleSymbol, RuleSymbol> targets)
	{
		var read    = new HashSet<RuleSymbol> { seed };
		var pending = new Queue<RuleSymbol>([seed]);

		while (pending.Count > 0)
		{
			if (!_bodies.TryGetValue(pending.Dequeue(), out var body))
				continue;

			foreach (var called in Called(body))
			{
				if (read.Add(called))
					pending.Enqueue(called);

				if (Rebound(called, targets) is { } reached && read.Add(reached))
					pending.Enqueue(reached);
			}
		}

		return read;
	}

	/// <summary>The calls in a body, not counting any a condition makes.</summary>
	IEnumerable<RuleSymbol> Called(Node node)
	{
		if (node is Node.Condition)
			yield break;

		if (node is Node.Call(var called, _))
			yield return called;

		if (_recoveries.TryGetValue(node, out var recovery))
			foreach (var one in Called(recovery.Sync)) yield return one;

		foreach (var child in node.Children)
			foreach (var one in Called(child))
				yield return one;
	}

	/// <summary>The number of a reading, given one the first time it is met.</summary>
	int Reading(IReadOnlyDictionary<RuleSymbol, RuleSymbol> asked, Publication publication)
	{
		for (var index = 0; index < _readings.Count; index++)
			if (Same(_readings[index], asked))
				return index;

		_readings.Add(asked);

		if (_readings.Count == Readable + 1)
			Report(
				TooManyReadings,
				$"This grammar is published in more than {Readable} readings — more assignments of " +
				"the rules its conditions ask about than a parser's number can tell apart. Publish " +
				"fewer of them, or split the grammar.",
				publication.At);

		return _readings.Count - 1;
	}

	static bool Same(IReadOnlyDictionary<RuleSymbol, RuleSymbol> one, IReadOnlyDictionary<RuleSymbol, RuleSymbol> other)
	{
		return one.Count == other.Count &&
		one.All(pair => other.TryGetValue(pair.Key, out var to) && to.Equals(pair.Value));
	}
}
