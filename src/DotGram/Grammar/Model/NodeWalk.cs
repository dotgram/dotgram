using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Model;

/// <summary>
/// Walking a node tree, for the passes that only need to see every node.
/// </summary>
/// <remarks>
/// <para>
/// Most passes over a grammar are not this: lowering, compilation and the analyses take
/// each node apart differently and are written as a <c>switch</c> over its shape, which
/// is what makes a missing case a compile error. This is for the other kind — the ones
/// that ask a question of every node and do not care how it is spelled.
/// </para>
/// <para>
/// Nodes are records and compare by value, so a set of them collapses two structurally
/// equal nodes into one. Everything a grammar keeps beside the tree — recoveries,
/// binding powers, fallible constructions — is keyed by <em>which</em> node, not by
/// what it looks like, so the identity comparer is not an optimization here but the
/// meaning.
/// </para>
/// </remarks>
static class NodeWalk
{
	/// <summary>
	/// The node, then everything under it, each subtree before the one to its right.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The order is the order the grammar is written in, which is the order an author
	/// reads their own rule in — so a pass that reports the first thing wrong reports the
	/// leftmost one.
	/// </para>
	/// <para>
	/// Iterative: a grammar of any depth is a tree of that depth, and recursion here would
	/// put the limit on the C# stack rather than on anything about grammars.
	/// </para>
	/// </remarks>
	public static IEnumerable<Node> Descendants(Node node)
	{
		if (node is null)
			throw new ArgumentNullException(nameof(node));

		var pending = new Stack<Node>();

		pending.Push(node);

		while (pending.Count > 0)
		{
			var current = pending.Pop();

			yield return current;

			// Backwards, because a stack hands back what went in last: pushing the operands
			// in reverse is what makes them come out in the order they were written.
			var children = current.Children as IReadOnlyList<Node> ?? [.. current.Children];

			for (var i = children.Count - 1; i >= 0; i--)
				pending.Push(children[i]);
		}
	}

	/// <summary>Every node in every one of these trees.</summary>
	public static IEnumerable<Node> Descendants(IEnumerable<Node> nodes)
	{
		foreach (var node in nodes)
			foreach (var descendant in Descendants(node))
				yield return descendant;
	}

	/// <summary>A set that holds nodes apart by identity rather than by value.</summary>
	public static HashSet<Node> ByIdentity(IEnumerable<Node> nodes)
	{
		var set = new HashSet<Node>(NodeIdentity.Instance);

		foreach (var node in nodes)
			set.Add(node);

		return set;
	}
}
