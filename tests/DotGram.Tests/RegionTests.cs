using System;
using System.Collections.Generic;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The region walk: one region per node a publication can reach, and nothing for what no
/// publication reaches.
/// </summary>
/// <remarks>
/// Nothing is emitted from this yet — see docs/next.md, "Next: regions", step 1. Tested on
/// its own regardless, the way <see cref="RetentionTests"/> is: an analysis only ever
/// exercised through the feature it gates is one nobody can tell is wrong.
/// </remarks>
public sealed class RegionTests
{
	[Fact]
	public void Every_node_a_publication_reaches_has_a_region()
	{
		var graph = Normalized(
			"""
			Start = 'a' & Rest
			Rest  = 'b'+
			parse Start
			""");

		var regions = Regions.Of(graph);

		foreach (var rule in new[] { "Start", "Rest" })
			foreach (var node in Descendants(graph.Bodies[Rule(graph, rule)]))
				Assert.True(regions.ContainsKey(node), $"{rule}: {node}");
	}

	[Fact]
	public void A_rule_no_publication_reaches_has_no_region()
	{
		var graph = Normalized(
			"""
			Start = 'a'
			Other = 'b'
			parse Start
			""");

		var regions = Regions.Of(graph);

		foreach (var node in Descendants(graph.Bodies[Rule(graph, "Other")]))
			Assert.False(regions.ContainsKey(node), node.ToString());
	}

	[Fact]
	public void A_rule_reached_only_through_a_call_still_has_a_region()
	{
		// Start does not mention Deep itself; Middle does. The walk has to cross that call
		// to reach it, the same way it crosses every other.
		var graph = Normalized(
			"""
			Start  = Middle
			Middle = Deep
			Deep   = 'x'
			parse Start
			""");

		var regions = Regions.Of(graph);

		Assert.True(regions.ContainsKey(graph.Bodies[Rule(graph, "Deep")]));
	}

	[Fact]
	public void A_recursive_rule_is_reached_once_and_the_walk_still_ends()
	{
		var graph = Normalized(
			"""
			Start = 'x' & Start | 'y'
			parse Start
			""");

		var regions = Regions.Of(graph);

		foreach (var node in Descendants(graph.Bodies[Rule(graph, "Start")]))
			Assert.True(regions.ContainsKey(node), node.ToString());
	}

	[Fact]
	public void Two_structurally_equal_literals_keep_separate_regions()
	{
		// Node is a record and compares by value, so this is really a test that the walk is
		// keyed by identity and not by what a node looks like.
		var graph = Normalized(
			"""
			Start = First & Second
			First  = 'a'
			Second = 'a'
			parse Start
			""");

		var regions = Regions.Of(graph);
		var first   = graph.Bodies[Rule(graph, "First")];
		var second  = graph.Bodies[Rule(graph, "Second")];

		Assert.Equal(first, second); // same value...
		Assert.True(regions.ContainsKey(first));
		Assert.True(regions.ContainsKey(second));
		Assert.NotSame(regions[first], regions[second]); // ...distinct regions
	}

	[Fact]
	public void Every_region_is_in_the_one_decision_class_there_is() =>
		Assert.All(
			Regions.Of(Normalized("Start = 'a'+\nparse Start")).Values,
			region => Assert.Equal(DecisionClass.Default, region.Class));

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
