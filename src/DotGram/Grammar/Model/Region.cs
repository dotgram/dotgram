using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Which bucket of caller context a node's compiled shape was decided from.
/// </summary>
/// <remarks>
/// One member for now, so every node falls into the same bucket and a <see cref="Region"/>
/// says no more than the node it names. Telling two call sites apart, or merging them
/// because the context settles a node's decisions the same way either time, is the next
/// step; see docs/next.md, "Next: regions".
/// </remarks>
public enum DecisionClass
{
	Default,
}

/// <summary>
/// A node, and the bucket of caller context its compiled shape was decided from.
/// </summary>
/// <remarks>
/// Identifies a node the way <c>Machine.Compile</c> does today — by itself alone, since
/// there is only one <see cref="DecisionClass"/> to be in. What changes in the step after
/// this one is not this shape but how many regions a rule with several call sites is
/// split into.
/// </remarks>
public sealed record Region(Node Node, DecisionClass Class);

/// <summary>
/// A region for every node a publication can reach.
/// </summary>
/// <remarks>
/// <para>
/// Walked from <see cref="RecognitionGraph.Publications"/> outward through
/// <see cref="Node.Call"/>, rather than over every rule the way <see cref="ExecutionPlan"/>
/// and <c>Machine</c>'s own compilation are. <see cref="ExecutionPlan"/>'s remarks record
/// why that walk failed there: the decision it was tried for does not depend on the
/// caller, so leaving an unpublished rule out of it just left that rule uncompiled. A
/// region is the caller's context by definition, so this is the walk that gets to start
/// from the publications instead.
/// </para>
/// <para>
/// One region per node today, because there is one decision class. Adding this walk
/// changes nothing else, which is what makes it safe to add on its own.
/// </para>
/// </remarks>
public static class Regions
{
	/// <summary>Every node reachable from a publication, each in its own region.</summary>
	public static IReadOnlyDictionary<Node, Region> Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var regions = new Dictionary<Node, Region>(NodeIdentity.Instance);
		var visited = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		foreach (var publication in graph.Publications)
			pending.Push(publication.Rule);

		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (!visited.Add(rule) || !graph.Bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
			{
				regions[node] = new Region(node, DecisionClass.Default);

				if (node is Node.Call(var called, _))
					pending.Push(called);
			}
		}

		return regions;
	}
}
