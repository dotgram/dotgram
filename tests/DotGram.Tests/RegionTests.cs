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
/// The region walk: one region per node and following a publication can reach it with,
/// merged wherever <c>classify</c> says two followings decide the same way.
/// </summary>
/// <remarks>
/// What turns a following into a real <see cref="DecisionClass"/> — <c>Machine.Silent</c>
/// and <c>Machine.Possessive</c> — is not reachable from here (<c>Machine</c> is internal
/// to the generator and carries no test seam of its own), so these tests stand in a
/// classifier of their own to exercise the walk itself: that it reaches what a publication
/// reaches and nothing else, that it crosses a call, that it ends on a recursive rule, that
/// node identity rather than node shape is what a region is keyed on, and that two
/// followings merge or split a node's regions exactly when <c>classify</c> says they
/// should. See docs/next.md, "Next: regions", step 2.
/// </remarks>
public sealed class RegionTests
{
	static readonly Func<Node, FirstSets.First, DecisionClass> OneClass = (_, _) => default;

	[Fact]
	public void Every_node_a_publication_reaches_has_a_region()
	{
		var graph   = Normalized("Start = 'a' & Rest\nRest  = 'b'+\nparse Start");
		var regions = Regions.Of(graph, OneClass);

		foreach (var rule in new[] { "Start", "Rest" })
			foreach (var node in Descendants(graph.Bodies[Rule(graph, rule)]))
				Assert.Contains(regions, region => ReferenceEquals(region.Node, node));
	}

	[Fact]
	public void A_rule_no_publication_reaches_has_no_region()
	{
		var graph   = Normalized("Start = 'a'\nOther = 'b'\nparse Start");
		var regions = Regions.Of(graph, OneClass);

		foreach (var node in Descendants(graph.Bodies[Rule(graph, "Other")]))
			Assert.DoesNotContain(regions, region => ReferenceEquals(region.Node, node));
	}

	[Fact]
	public void A_rule_reached_only_through_a_call_still_has_a_region()
	{
		// Start does not mention Deep itself; Middle does. The walk has to cross that call
		// to reach it, the same way it crosses every other.
		var graph   = Normalized("Start  = Middle\nMiddle = Deep\nDeep   = 'x'\nparse Start");
		var regions = Regions.Of(graph, OneClass);

		Assert.Contains(regions, region => ReferenceEquals(region.Node, graph.Bodies[Rule(graph, "Deep")]));
	}

	[Fact]
	public void A_recursive_rule_is_reached_once_and_the_walk_still_ends()
	{
		var graph   = Normalized("Start = 'x' & Start | 'y'\nparse Start");
		var regions = Regions.Of(graph, OneClass);

		foreach (var node in Descendants(graph.Bodies[Rule(graph, "Start")]))
			Assert.Contains(regions, region => ReferenceEquals(region.Node, node));
	}

	[Fact]
	public void Two_structurally_equal_literals_keep_separate_regions()
	{
		// Node is a record and compares by value, so this is really a test that the walk is
		// keyed by identity and not by what a node looks like.
		var graph   = Normalized("Start = First & Second\nFirst  = 'a'\nSecond = 'a'\nparse Start");
		var regions = Regions.Of(graph, OneClass);
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
		var regions = Regions.Of(graph, OneClass);
		var shared  = graph.Bodies[Rule(graph, "Shared")];

		Assert.Single(regions, region => ReferenceEquals(region.Node, shared));
	}

	[Fact]
	public void Two_followings_classified_differently_split_into_two_regions()
	{
		var graph   = Normalized("Start  = Shared\nShared = 'a'\nparse Start\nfind Shared");
		var shared  = graph.Bodies[Rule(graph, "Shared")];

		var regions = Regions.Of(graph, (_, following) => new DecisionClass(following.IsKnown, false));
		var classes = regions.Where(region => ReferenceEquals(region.Node, shared))
			.Select(region => region.Class)
			.ToList();

		// End (through Start, a parse) is known; All (through Shared's own find) is not.
		Assert.Equal(2, classes.Count);
		Assert.Contains(new DecisionClass(true,  false), classes);
		Assert.Contains(new DecisionClass(false, false), classes);
	}

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
