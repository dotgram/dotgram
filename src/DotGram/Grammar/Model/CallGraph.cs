using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Which rule calls which, and what that says about the shape of a grammar.
/// </summary>
/// <remarks>
/// <para>
/// One walk instead of several. The same graph was being built and traversed in four
/// places, each for its own question and each in its own way: which rules reach
/// themselves, which rules a publication can reach, which rules a rebinding affects, and
/// whether two <c>with</c> sites reach each other. Four answers to one question about one
/// structure is how the answers come to disagree.
/// </para>
/// <para>
/// Strongly connected components are the primitive the questions actually want. "Is this
/// rule recursive" is "is it in a component of more than one, or does it call itself";
/// "do these two reach each other" is "are they in the same component"; and the order the
/// components come out in is the order a pass over a call graph has to run in. Working any
/// of those out by reachability from every node is quadratic and — worse — gives a
/// different shape of answer each time it is asked.
/// </para>
/// <para>
/// Tarjan, iteratively. Not for the speed: a grammar of a thousand rules is a small graph
/// either way. Recursion here would put the limit on the C# stack rather than on anything
/// about grammars, which is the same reason <see cref="NodeWalk"/> is iterative.
/// </para>
/// </remarks>
sealed class CallGraph
{
	readonly Dictionary<RuleSymbol, List<RuleSymbol>> _forward = [];
	readonly Dictionary<RuleSymbol, List<RuleSymbol>> _reverse = [];
	readonly Dictionary<RuleSymbol, int>              _component = [];

	List<IReadOnlyList<RuleSymbol>>? _components;

	/// <param name="rules">Every rule, so one with no calls still has an entry.</param>
	/// <param name="calls">What each rule calls directly, in any order, duplicates allowed.</param>
	public CallGraph(IEnumerable<RuleSymbol> rules, Func<RuleSymbol, IEnumerable<RuleSymbol>> calls)
	{
		if (rules is null)
			throw new ArgumentNullException(nameof(rules));

		if (calls is null)
			throw new ArgumentNullException(nameof(calls));

		foreach (var rule in rules)
		{
			_forward[rule] = [];
			_reverse[rule] = [];
		}

		foreach (var rule in _forward.Keys)
			foreach (var called in calls(rule))
			{
				// A call to something outside the set — a built-in with no body of its own —
				// is an edge to nowhere and is left out rather than given a phantom node.
				if (!_forward.TryGetValue(called, out var _) || _forward[rule].Contains(called))
					continue;

				_forward[rule].Add(called);
				_reverse[called].Add(rule);
			}
	}

	/// <summary>What a rule calls directly.</summary>
	public IReadOnlyList<RuleSymbol> Calls(RuleSymbol rule) =>
		_forward.TryGetValue(rule, out var called) ? called : [];

	/// <summary>What calls a rule directly.</summary>
	public IReadOnlyList<RuleSymbol> CalledBy(RuleSymbol rule) =>
		_reverse.TryGetValue(rule, out var callers) ? callers : [];

	/// <summary>
	/// The strongly connected components, each in the order Tarjan settles them.
	/// </summary>
	/// <remarks>
	/// Which is reverse topological order: a component is finished only once everything it
	/// can reach is, so a pass that has to run over a callee before its caller runs over
	/// these as they come, and one that has to run the other way round reverses them.
	/// </remarks>
	public IReadOnlyList<IReadOnlyList<RuleSymbol>> Components => _components ??= Tarjan();

	/// <summary>Whether these two can each reach the other.</summary>
	/// <remarks>
	/// Which is the same as sharing a component, and is why this is not a search: mutual
	/// reachability is what a component *is*.
	/// </remarks>
	public bool Together(RuleSymbol one, RuleSymbol other)
	{
		_ = Components;

		return !ReferenceEquals(one, other) &&
			_component.TryGetValue(one, out var mine) &&
			_component.TryGetValue(other, out var theirs) &&
			mine == theirs;
	}

	/// <summary>Whether a rule can reach itself, directly or round through others.</summary>
	public bool Recurses(RuleSymbol rule)
	{
		_ = Components;

		// A component of more than one is a cycle by construction; a component of one is
		// only a cycle if the rule calls itself, which Tarjan does not distinguish.
		if (_component.TryGetValue(rule, out var which) && Components[which].Count > 1)
			return true;

		return Calls(rule).Contains(rule);
	}

	List<IReadOnlyList<RuleSymbol>> Tarjan()
	{
		var components = new List<IReadOnlyList<RuleSymbol>>();
		var index      = new Dictionary<RuleSymbol, int>();
		var low        = new Dictionary<RuleSymbol, int>();
		var onStack    = new HashSet<RuleSymbol>();
		var stack      = new Stack<RuleSymbol>();
		var next       = 0;

		foreach (var start in _forward.Keys)
		{
			if (index.ContainsKey(start))
				continue;

			// The explicit walk: each frame is a rule and how far through its calls it is.
			var work = new Stack<(RuleSymbol Rule, int At)>();

			index[start] = low[start] = next++;
			stack.Push(start);
			onStack.Add(start);
			work.Push((start, 0));

			while (work.Count > 0)
			{
				var (rule, at) = work.Pop();
				var called     = Calls(rule);

				if (at < called.Count)
				{
					work.Push((rule, at + 1));

					var target = called[at];

					if (!index.ContainsKey(target))
					{
						index[target] = low[target] = next++;
						stack.Push(target);
						onStack.Add(target);
						work.Push((target, 0));
					}
					else if (onStack.Contains(target))
					{
						low[rule] = Math.Min(low[rule], index[target]);
					}

					continue;
				}

				// Done with this rule's calls: hand its low link up to whoever pushed it,
				// and close a component off where it is its own root.
				if (work.Count > 0)
				{
					var caller = work.Peek().Rule;

					low[caller] = Math.Min(low[caller], low[rule]);
				}

				if (low[rule] != index[rule])
					continue;

				var members = new List<RuleSymbol>();

				while (true)
				{
					var member = stack.Pop();

					onStack.Remove(member);
					_component[member] = components.Count;
					members.Add(member);

					if (ReferenceEquals(member, rule))
						break;
				}

				components.Add(members);
			}
		}

		return components;
	}
}
