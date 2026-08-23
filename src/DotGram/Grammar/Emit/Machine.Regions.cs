using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>
	/// Every region a publication of this graph can reach.
	/// </summary>
	/// <remarks>
	/// Read by <see cref="ComputeEagerRules"/> for <see cref="DecisionClass.Committed"/> and
	/// <see cref="DecisionClass.Deterministic"/>; <c>Compile</c> itself still asks
	/// <see cref="Silent"/> and <see cref="Possessive"/> directly, threading <c>following</c>
	/// down the tree the way it always has, rather than through this collection.
	/// </remarks>
	IReadOnlyCollection<Region> ComputeRegions() =>
		Regions.Of(
			_graph,
			(node, following, committed) => new DecisionClass(
				Silent(node, following),
				node is Node.Repeat(var body, _, _) && Possessive(body, following),
				Deterministic(node, [], following),
				committed),
			alternatives => Predictive(alternatives) is not null);

	/// <summary>
	/// Every rule whose value may be built the moment it returns, rather than waiting for
	/// the parse to be accepted.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A rule qualifies when <em>every</em> region its own body reaches — every call site,
	/// since the same compiled body serves all of them — is both
	/// <see cref="DecisionClass.Committed"/> (nothing upstream may still discard this call
	/// for a sibling one) and <see cref="DecisionClass.Deterministic"/> (it cannot itself be
	/// retried with a different alternative). One region reached with either false is enough
	/// to disqualify the rule everywhere, because the automaton has no way to tell, at
	/// <c>Return:</c>, which call site a particular return came from.
	/// </para>
	/// <para>
	/// Kept conservative on purpose: a grammar using recovery anywhere disqualifies every
	/// rule, matching the same guard <see cref="CanLower"/> already uses — the recovery pass
	/// inside materialization has not been checked against a bounded range.
	/// </para>
	/// </remarks>
	HashSet<RuleSymbol> ComputeEagerRules()
	{
		if (_graph.Recoveries.Count > 0)
			return [];

		var eligible = new Dictionary<RuleSymbol, bool>();

		foreach (var region in _regions)
		{
			// A rule with nothing declared to build has nothing an eager trigger would do
			// early — the same test Register/RenderEngine use to decide whether a grammar
			// needs the value tables at all.
			if (!_owners.TryGetValue(region.Node, out var rule) ||
				!ReferenceEquals(region.Node, _graph.Bodies[rule]) ||
				ValueRule(rule) < 0)
				continue;

			var qualifies = region.Class is { Committed: true, Deterministic: true };

			eligible[rule] = eligible.TryGetValue(rule, out var soFar) ? soFar && qualifies : qualifies;
		}

		var rules = new HashSet<RuleSymbol>();

		foreach (var entry in eligible)
			if (entry.Value)
				rules.Add(entry.Key);

		return rules;
	}
}
