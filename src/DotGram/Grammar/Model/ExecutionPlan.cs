using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

public enum RuleExecutionKind
{
	Direct,
	Recursive,
}

/// <summary>A strongly connected group of rule recognizers in the runtime call graph.</summary>
public sealed class ExecutionComponent(
	IReadOnlyList<RuleSymbol> rules,
	bool recursive)
{
	public IReadOnlyList<RuleSymbol> Rules { get; } = rules;

	/// <summary>
	/// Whether entering this component can return to a recognizer already active in it.
	/// A one-rule component is recursive only when that rule calls itself.
	/// </summary>
	public bool IsRecursive { get; } = recursive;
}

/// <summary>
/// The runtime rule-call structure of a normalized grammar, before a code-generation
/// strategy is chosen for it.
/// </summary>
public sealed class ExecutionPlan(
	IReadOnlyDictionary<RuleSymbol, IReadOnlyList<RuleSymbol>> calls,
	IReadOnlyList<ExecutionComponent> components,
	IReadOnlyDictionary<RuleSymbol, ExecutionComponent> componentOf,
	IReadOnlyDictionary<RuleSymbol, RuleExecutionKind> kinds,
	IReadOnlyDictionary<RuleSymbol, int?> maximumCallDepth)
{
	/// <summary>The distinct rules each rule may call, in grammar order.</summary>
	public IReadOnlyDictionary<RuleSymbol, IReadOnlyList<RuleSymbol>> Calls { get; } = calls;

	/// <summary>Strongly connected components, ordered by their first rule.</summary>
	public IReadOnlyList<ExecutionComponent> Components { get; } = components;

	/// <summary>The component containing each rule.</summary>
	public IReadOnlyDictionary<RuleSymbol, ExecutionComponent> ComponentOf { get; } = componentOf;

	/// <summary>The code-generation strategy required by each rule.</summary>
	public IReadOnlyDictionary<RuleSymbol, RuleExecutionKind> Kinds { get; } = kinds;

	/// <summary>
	/// Maximum recognizer depth, including the rule itself, or null when some reachable
	/// execution is recursive and therefore has no finite static bound.
	/// </summary>
	public IReadOnlyDictionary<RuleSymbol, int?> MaximumCallDepth { get; } = maximumCallDepth;

	public bool IsRecursive(RuleSymbol rule) => Kinds[rule] == RuleExecutionKind.Recursive;

	public bool ReachesRecursion(RuleSymbol rule) => MaximumCallDepth[rule] is null;
}

/// <summary>Builds the execution call graph and its recursion classification.</summary>
public static class ExecutionPlanner
{
	public static ExecutionPlan Analyze(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var count = graph.Rules.Count;
		var indices = new Dictionary<RuleSymbol, int>();

		for (var i = 0; i < count; i++)
			indices[graph.Rules[i]] = i;

		var edges = new List<int>[count];

		for (var i = 0; i < count; i++)
		{
			edges[i] = [];

			if (!graph.Bodies.TryGetValue(graph.Rules[i], out var body))
				continue;

			var seen = new bool[count];
			Collect(body, graph, indices, edges[i], seen);
		}

		var componentIndices = ComponentsOf(edges);
		var componentByRule = new int[count];

		for (var i = 0; i < componentIndices.Count; i++)
			foreach (var rule in componentIndices[i])
				componentByRule[rule] = i;

		var recursive = new bool[componentIndices.Count];

		for (var i = 0; i < componentIndices.Count; i++)
		{
			recursive[i] = componentIndices[i].Count > 1;

			if (!recursive[i])
			{
				var only = componentIndices[i][0];
				recursive[i] = edges[only].Contains(only);
			}
		}

		var componentEdges = new List<int>[componentIndices.Count];
		var componentEdgeSets = new HashSet<int>[componentIndices.Count];

		for (var i = 0; i < componentEdges.Length; i++)
		{
			componentEdges[i] = [];
			componentEdgeSets[i] = [];
		}

		for (var from = 0; from < count; from++)
			foreach (var to in edges[from])
			{
				var source = componentByRule[from];
				var target = componentByRule[to];

				if (source != target && componentEdgeSets[source].Add(target))
					componentEdges[source].Add(target);
			}

		var componentDepth = Depths(componentEdges, recursive);
		var components = new ExecutionComponent[componentIndices.Count];
		var componentOf = new Dictionary<RuleSymbol, ExecutionComponent>();

		for (var i = 0; i < components.Length; i++)
		{
			var rules = new RuleSymbol[componentIndices[i].Count];

			for (var j = 0; j < rules.Length; j++)
				rules[j] = graph.Rules[componentIndices[i][j]];

			components[i] = new ExecutionComponent(rules, recursive[i]);
		}

		var calls = new Dictionary<RuleSymbol, IReadOnlyList<RuleSymbol>>();
		var depths = new Dictionary<RuleSymbol, int?>();
		var kinds = new Dictionary<RuleSymbol, RuleExecutionKind>();

		for (var i = 0; i < count; i++)
		{
			var called = new RuleSymbol[edges[i].Count];

			for (var j = 0; j < called.Length; j++)
				called[j] = graph.Rules[edges[i][j]];

			calls[graph.Rules[i]] = called;
			componentOf[graph.Rules[i]] = components[componentByRule[i]];
			kinds[graph.Rules[i]] = recursive[componentByRule[i]]
				? RuleExecutionKind.Recursive
				: RuleExecutionKind.Direct;
			depths[graph.Rules[i]] = componentDepth[componentByRule[i]];
		}

		return new ExecutionPlan(calls, components, componentOf, kinds, depths);
	}

	static void Collect(
		Node root,
		RecognitionGraph graph,
		IReadOnlyDictionary<RuleSymbol, int> indices,
		List<int> calls,
		bool[] seen)
	{
		var pending = new Stack<Node>();
		pending.Push(root);

		while (pending.Count > 0)
		{
			var node = pending.Pop();

			if (node is Node.Call(var rule, _))
			{
				// Arguments specialize a rule during normalization. CompileCall does not run
				// them, so they are deliberately not traversed as execution edges.
				if (indices.TryGetValue(rule, out var called) && !seen[called])
				{
					seen[called] = true;
					calls.Add(called);
				}

				continue;
			}

			if (graph.Recoveries.TryGetValue(node, out var recovery))
				pending.Push(recovery.Sync);

			var children = node.Children as IReadOnlyList<Node> ?? [.. node.Children];

			for (var i = children.Count - 1; i >= 0; i--)
				pending.Push(children[i]);
		}
	}

	static List<List<int>> ComponentsOf(IReadOnlyList<List<int>> edges)
	{
		var count = edges.Count;
		var visited = new bool[count];
		var finished = new List<int>(count);

		for (var start = 0; start < count; start++)
		{
			if (visited[start])
				continue;

			var pending = new Stack<(int Node, int Next)>();
			visited[start] = true;
			pending.Push((start, 0));

			while (pending.Count > 0)
			{
				var (node, next) = pending.Pop();

				if (next < edges[node].Count)
				{
					pending.Push((node, next + 1));

					var child = edges[node][next];

					if (!visited[child])
					{
						visited[child] = true;
						pending.Push((child, 0));
					}
				}
				else
				{
					finished.Add(node);
				}
			}
		}

		var reverse = new List<int>[count];

		for (var i = 0; i < count; i++)
			reverse[i] = [];

		for (var from = 0; from < count; from++)
			foreach (var to in edges[from])
				reverse[to].Add(from);

		Array.Clear(visited, 0, visited.Length);

		var components = new List<List<int>>();

		for (var i = finished.Count - 1; i >= 0; i--)
		{
			var start = finished[i];

			if (visited[start])
				continue;

			var component = new List<int>();
			var pending = new Stack<int>();
			visited[start] = true;
			pending.Push(start);

			while (pending.Count > 0)
			{
				var node = pending.Pop();
				component.Add(node);

				for (var j = reverse[node].Count - 1; j >= 0; j--)
				{
					var child = reverse[node][j];

					if (!visited[child])
					{
						visited[child] = true;
						pending.Push(child);
					}
				}
			}

			component.Sort();
			components.Add(component);
		}

		components.Sort((left, right) => left[0].CompareTo(right[0]));

		return components;
	}

	static int?[] Depths(IReadOnlyList<List<int>> edges, IReadOnlyList<bool> recursive)
	{
		var count = edges.Count;
		var remaining = new int[count];
		var parents = new List<int>[count];

		for (var i = 0; i < count; i++)
		{
			remaining[i] = edges[i].Count;
			parents[i] = [];
		}

		for (var from = 0; from < count; from++)
			foreach (var to in edges[from])
				parents[to].Add(from);

		var depths = new int?[count];
		var ready = new Queue<int>();

		for (var i = 0; i < count; i++)
			if (remaining[i] == 0)
				ready.Enqueue(i);

		while (ready.Count > 0)
		{
			var component = ready.Dequeue();
			int? depth = recursive[component] ? null : 1;

			if (depth is not null)
				foreach (var child in edges[component])
				{
					if (depths[child] is null)
					{
						depth = null;
						break;
					}

					depth = Math.Max(depth.Value, 1 + depths[child]!.Value);
				}

			depths[component] = depth;

			foreach (var parent in parents[component])
				if (--remaining[parent] == 0)
					ready.Enqueue(parent);
		}

		return depths;
	}
}
