using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The region walk: one region per node, following and commitment a publication can reach
/// it with, merged wherever <c>classify</c> says two of them decide the same way.
/// </summary>
/// <remarks>
/// What turns a following and a commitment into a real <see cref="DecisionClass"/> —
/// <c>Machine.Silent</c>, <c>Machine.Possessive</c>, <c>Machine.Deterministic</c> and
/// <c>Machine.Predictive</c> — is not reachable from here (<c>Machine</c> is internal to
/// the generator and carries no test seam of its own), so these tests stand in classifiers
/// of their own to exercise the walk itself. See docs/next.md, "Next: regions", steps 2
/// and 3.
/// </remarks>
public sealed class RegionTests
{
	static readonly Func<Node, FirstSets.First, bool, DecisionClass> OneClass =
		(_, _, committed) => new DecisionClass(false, false, false, committed);

	static readonly Func<IReadOnlyList<Node>, bool> NeverPredictive  = _ => false;
	static readonly Func<IReadOnlyList<Node>, bool> AlwaysPredictive = _ => true;

	[Fact]
	public void Every_node_a_publication_reaches_has_a_region()
	{
		var graph   = Normalized("Start = 'a' & Rest\nRest  = 'b'+\nparse Start");
		var regions = Regions.Of(graph, OneClass, NeverPredictive);

		foreach (var rule in new[] { "Start", "Rest" })
			foreach (var node in Descendants(graph.Bodies[Rule(graph, rule)]))
				Assert.Contains(regions, region => ReferenceEquals(region.Node, node));
	}

	[Fact]
	public void A_rule_no_publication_reaches_has_no_region()
	{
		var graph   = Normalized("Start = 'a'\nOther = 'b'\nparse Start");
		var regions = Regions.Of(graph, OneClass, NeverPredictive);

		foreach (var node in Descendants(graph.Bodies[Rule(graph, "Other")]))
			Assert.DoesNotContain(regions, region => ReferenceEquals(region.Node, node));
	}

	[Fact]
	public void A_rule_reached_only_through_a_call_still_has_a_region()
	{
		// Start does not mention Deep itself; Middle does. The walk has to cross that call
		// to reach it, the same way it crosses every other.
		var graph   = Normalized("Start  = Middle\nMiddle = Deep\nDeep   = 'x'\nparse Start");
		var regions = Regions.Of(graph, OneClass, NeverPredictive);

		Assert.Contains(regions, region => ReferenceEquals(region.Node, graph.Bodies[Rule(graph, "Deep")]));
	}

	[Fact]
	public void A_recursive_rule_is_reached_once_and_the_walk_still_ends()
	{
		var graph   = Normalized("Start = 'x' & Start | 'y'\nparse Start");
		var regions = Regions.Of(graph, OneClass, NeverPredictive);

		foreach (var node in Descendants(graph.Bodies[Rule(graph, "Start")]))
			Assert.Contains(regions, region => ReferenceEquals(region.Node, node));
	}

	[Fact]
	public void Two_structurally_equal_literals_keep_separate_regions()
	{
		// Node is a record and compares by value, so this is really a test that the walk is
		// keyed by identity and not by what a node looks like.
		var graph   = Normalized("Start = First & Second\nFirst  = 'a'\nSecond = 'a'\nparse Start");
		var regions = Regions.Of(graph, OneClass, NeverPredictive);
		var first   = graph.Bodies[Rule(graph, "First")];
		var second  = graph.Bodies[Rule(graph, "Second")];

		Assert.Equal(first, second); // same value...
		Assert.Contains(regions, region => ReferenceEquals(region.Node, first));
		Assert.Contains(regions, region => ReferenceEquals(region.Node, second));
	}

	[Fact]
	public void Two_followings_classified_the_same_way_share_one_region()
	{
		// Shared is reached with `following = End` through Start, and again with
		// `following = All` directly through its own publication. A classifier that does
		// not read `following` cannot tell those apart, so the node gets one region however
		// many ways it is reached.
		var graph   = Normalized("Start  = Shared\nShared = 'a'\nparse Start\nfind Shared");
		var regions = Regions.Of(graph, OneClass, NeverPredictive);
		var shared  = graph.Bodies[Rule(graph, "Shared")];

		Assert.Single(regions, region => ReferenceEquals(region.Node, shared));
	}

	[Fact]
	public void Two_followings_classified_differently_split_into_two_regions()
	{
		var graph  = Normalized("Start  = Shared\nShared = 'a'\nparse Start\nfind Shared");
		var shared = graph.Bodies[Rule(graph, "Shared")];

		var regions = Regions.Of(
			graph,
			(_, following, committed) => new DecisionClass(following.IsKnown, false, false, committed),
			NeverPredictive);

		var classes = regions.Where(region => ReferenceEquals(region.Node, shared))
			.Select(region => region.Class)
			.ToList();

		// End (through Start, a parse) is known; All (through Shared's own find) is not.
		Assert.Equal(2, classes.Count);
		Assert.Contains(classes, c => c.Silent);
		Assert.Contains(classes, c => !c.Silent);
	}

	// ── Commitment (step 3, "the fourth need") ──────────────────────────────────

	[Fact]
	public void Commitment_starts_true_at_a_publications_root()
	{
		var graph   = Normalized("Start = 'a'\nparse Start");
		var regions = Regions.Of(graph, OneClass, NeverPredictive);
		var body    = graph.Bodies[Rule(graph, "Start")];

		Assert.True(RegionFor(regions, body).Class.Committed);
	}

	[Fact]
	public void A_deterministic_part_leaves_what_follows_it_committed()
	{
		// A call survives normalization as its own node; two bare literals would be merged
		// into one before the walk ever saw two parts to test the propagation between.
		var graph = Normalized("Start = 'a' & Rest\nRest = 'b'\nparse Start");
		var parts = (Node.Sequence)graph.Bodies[Rule(graph, "Start")];

		var regions = Regions.Of(
			graph, (_, _, committed) => new DecisionClass(false, false, true, committed), NeverPredictive);

		Assert.True(RegionFor(regions, parts.Nodes[1]).Class.Committed);
	}

	[Fact]
	public void A_non_deterministic_part_leaves_what_follows_it_uncommitted()
	{
		var graph = Normalized("Start = 'a' & Rest\nRest = 'b'\nparse Start");
		var parts = (Node.Sequence)graph.Bodies[Rule(graph, "Start")];

		// Everything is deterministic except the literal "a" itself.
		var regions = Regions.Of(
			graph,
			(node, _, committed) => new DecisionClass(
				false, false, node is not Node.Literal("a"), committed),
			NeverPredictive);

		Assert.True(RegionFor(regions, parts.Nodes[0]).Class.Committed);
		Assert.False(RegionFor(regions, parts.Nodes[1]).Class.Committed);
	}

	[Fact]
	public void A_predictive_choice_hands_its_alternatives_the_commitment_it_was_given()
	{
		// Two multi-character literals stay a choice of two; single characters would be
		// folded into one element set before the walk saw two alternatives to test.
		var graph        = Normalized("Start = \"ab\" | \"cd\"\nparse Start");
		var alternatives = (Node.Choice)graph.Bodies[Rule(graph, "Start")];

		var regions = Regions.Of(graph, OneClass, AlwaysPredictive);

		foreach (var alternative in alternatives.Nodes)
			Assert.True(RegionFor(regions, alternative).Class.Committed);
	}

	[Fact]
	public void A_choice_that_is_not_predictive_uncommits_every_alternative()
	{
		var graph        = Normalized("Start = \"ab\" | \"cd\"\nparse Start");
		var alternatives = (Node.Choice)graph.Bodies[Rule(graph, "Start")];

		var regions = Regions.Of(graph, OneClass, NeverPredictive);

		foreach (var alternative in alternatives.Nodes)
			Assert.False(RegionFor(regions, alternative).Class.Committed);
	}

	[Fact]
	public void A_possessive_repetitions_turn_starts_committed()
	{
		var graph  = Normalized("Start = 'a'*\nparse Start");
		var repeat = (Node.Repeat)graph.Bodies[Rule(graph, "Start")];

		var regions = Regions.Of(
			graph, (node, _, committed) => new DecisionClass(false, node is Node.Repeat, false, committed),
			NeverPredictive);

		Assert.True(RegionFor(regions, repeat.Body).Class.Committed);
	}

	[Fact]
	public void A_non_possessive_repetitions_turn_starts_uncommitted()
	{
		var graph   = Normalized("Start = 'a'*\nparse Start");
		var repeat  = (Node.Repeat)graph.Bodies[Rule(graph, "Start")];
		var regions = Regions.Of(graph, OneClass, NeverPredictive);

		Assert.False(RegionFor(regions, repeat.Body).Class.Committed);
	}

	[Fact]
	public void An_atomic_groups_close_recommits_whatever_came_before_it()
	{
		var graph = Normalized("Start = { 'a' } & 'b'\nparse Start");
		var parts = (Node.Sequence)graph.Bodies[Rule(graph, "Start")];

		// Deterministic is false everywhere, which would otherwise carry an uncommitted
		// state all the way to the end of the sequence.
		var regions = Regions.Of(graph, OneClass, NeverPredictive);

		Assert.IsType<Node.Atomic>(parts.Nodes[0]);
		Assert.True(RegionFor(regions, parts.Nodes[1]).Class.Committed);
	}

	static Region RegionFor(IReadOnlyCollection<Region> regions, Node node) =>
		regions.Single(region => ReferenceEquals(region.Node, node));

	static RuleSymbol Rule(RecognitionGraph graph, string name)
	{
		foreach (var rule in graph.Rules)
			if (rule.Name == name)
				return rule;

		throw new InvalidOperationException($"No rule named '{name}'.");
	}

	static IEnumerable<Node> Descendants(Node node)
	{
		yield return node;

		foreach (var child in node.Children)
			foreach (var descendant in Descendants(child))
				yield return descendant;
	}

	static RecognitionGraph Normalized(string text) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
}
