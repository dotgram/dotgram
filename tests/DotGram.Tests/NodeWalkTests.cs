using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

public sealed class NodeWalkTests
{
	[Fact]
	public void Unary_nodes_keep_their_body_and_left_to_right_order()
	{
		Node left = new Node.Literal("x");
		Node right = new Node.Literal("x");
		var sequence = new Node.Sequence([left, right]);
		Node[] wrappers =
		[
			new Node.Atomic(sequence),
			new Node.Marked(sequence, "state"),
			new Node.Repeat(sequence, 0, null),
			new Node.Lookahead(true, sequence),
			new Node.Capture("text", sequence),
			new Node.Construct(sequence, new Construction.Expression("text")),
		];

		foreach (var wrapper in wrappers)
		{
			var found = Walk(wrapper).ToArray();

			Assert.Equal(4, found.Length);
			Assert.Same(wrapper, found[0]);
			Assert.Same(sequence, found[1]);
			Assert.Same(left, found[2]);
			Assert.Same(right, found[3]);
		}
	}

	[Fact]
	public void Conditions_and_custom_children_are_still_walked()
	{
		Node left = new Node.Literal("a");
		Node right = new Node.Literal("b");
		var condition = new Node.Condition(new Test.Meets(left, right, false));
		var custom = new CustomNode(condition);
		var root = new Node.Choice([custom, left]);
		var found = Walk(root).ToArray();
		Node[] expected = [root, custom, condition, left, right, left];

		Assert.Equal(expected.Length, found.Length);

		for (var i = 0; i < expected.Length; i++)
			Assert.Same(expected[i], found[i]);
	}

	[Fact]
	public void Deep_unary_trees_do_not_require_a_recursive_walk()
	{
		Node node = new Node.Literal("x");

		for (var i = 0; i < 100_000; i++)
			node = new Node.Atomic(node);

		Assert.Equal(100_001, Walk(node).Count());
	}

	static readonly Func<Node, IEnumerable<Node>> Walk =
		(Func<Node, IEnumerable<Node>>)typeof(Node).Assembly
			.GetType("DotGram.Grammar.Model.NodeWalk")!
			.GetMethod("Descendants", [typeof(Node)])!
			.CreateDelegate(typeof(Func<Node, IEnumerable<Node>>));

	sealed record CustomNode(Node Body) : Node
	{
		public override IEnumerable<Node> Children
		{
			get { yield return Body; }
		}
	}
}
