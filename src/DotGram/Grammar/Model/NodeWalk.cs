using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Model;

/// <summary>
/// Walking a node tree, for the passes that only need to see every node.
/// </summary>
/// <remarks>
/// <para>
/// Most passes over a grammar are not this: lowering, compilation and the analyses take
/// each node apart differently and are written as a <c>switch</c> over its shape. This is
/// for the other kind — the ones that ask a question of every node and do not care how it
/// is spelled.
/// </para>
/// <para>
/// Such a <c>switch</c> is not exhaustive to the compiler: nearly every one ends in a
/// default arm or simply falls out, so a node kind it does not name takes that arm without
/// a word. A new kind of node is added to each of them by hand, and what each default
/// answers for it has to be read, not assumed. A walk through here needs no such care as
/// long as the node lists its operands in <see cref="Node.Children"/>.
/// </para>
/// <para>
/// Nodes are records and compare by value, so a set of them collapses two structurally
/// equal nodes into one. Everything a grammar keeps beside the tree — recoveries,
/// binding powers and construction metadata — is keyed by <em>which</em> node, not by
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

			// Unary nodes need no temporary collection to put their one operand on the stack.
			var body = current switch
			{
				Node.Atomic one    => one.Body,
				Node.Marked one    => one.Body,
				Node.Repeat one    => one.Body,
				Node.Lookahead one => one.Body,
				Node.Capture one   => one.Body,
				Node.Construct one => one.Body,
				Node.Bind one      => one.Body,
				Node.Counted one   => one.Body,
				_                  => null,
			};

			if (body is not null)
			{
				pending.Push(body);

				continue;
			}

			// Backwards, because a stack hands back what went in last: pushing the operands
			// in reverse is what makes them come out in the order they were written.
			var operands = current.Children;
			var children = operands as IReadOnlyList<Node> ?? [.. operands];

			for (var i = children.Count - 1; i >= 0; i--)
				pending.Push(children[i]);
		}
	}

	/// <summary>
	/// The node and everything under it, as <see cref="Descendants(Node)"/>, and then the
	/// synchronization of every repetition among them that recovers, with everything under that.
	/// </summary>
	/// <remarks>
	/// A recovery's synchronization is read by the parser as surely as the rule's body is, but it
	/// is kept beside the tree (<see cref="RecognitionGraph.Recoveries"/>) and is none of the
	/// repetition's <see cref="Node.Children"/>. A question about what a rule reads — whether it calls
	/// the host, or which rules it reaches — asks this walk, or it misses <c>recover @M</c>.
	/// </remarks>
	public static IEnumerable<Node> Reading(Node node, RecognitionGraph graph)
	{
		if (node is null)
			throw new ArgumentNullException(nameof(node));

		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var pending = new Queue<Node>();

		pending.Enqueue(node);

		while (pending.Count > 0)
			foreach (var each in Descendants(pending.Dequeue()))
			{
				yield return each;

				if (graph.Recoveries.TryGetValue(each, out var recovery))
					pending.Enqueue(recovery.Sync);
			}
	}

	/// <summary>
	/// Whether any rule of the grammar reads through a recognizer of the host, in its body or a
	/// synchronization — any one, or one <paramref name="which"/> picks.
	/// </summary>
	public static bool CallsTheHost(RecognitionGraph graph, Func<Node.External, bool>? which = null)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		foreach (var body in graph.Bodies.Values)
			foreach (var node in Reading(body, graph))
				if (node is Node.External external && (which is null || which(external)))
					return true;

		return false;
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
