using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
	/// <summary>The node and everything under it, in no particular order.</summary>
	/// <remarks>
	/// Iterative: a grammar of any depth is a tree of that depth, and recursion here
	/// would put the limit on the C# stack rather than on anything about grammars.
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

			foreach (var child in current.Children)
				pending.Push(child);
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
		var set = new HashSet<Node>(Identity.Instance);

		foreach (var node in nodes)
			set.Add(node);

		return set;
	}

	sealed class Identity : IEqualityComparer<Node>
	{
		public static readonly Identity Instance = new();

		public bool Equals(Node? left, Node? right) => ReferenceEquals(left, right);
		public int  GetHashCode(Node node)          => RuntimeHelpers.GetHashCode(node);
	}
}
