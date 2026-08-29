using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Which rule calls which, asked once instead of four times.
/// </summary>
/// <remarks>
/// Strongly connected components are easy to write almost correctly — an iterative Tarjan
/// especially, where the low link has to travel back up an explicit stack — so the cases
/// below are the ones that separate almost from correctly: a self-call, which is a cycle
/// the component size does not show; a component reached through another; and two cycles
/// that touch without joining.
/// </remarks>
public sealed class CallGraphTests
{
	static RecognitionGraph Graph(string grammar) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				DotGram.Grammar.Parsing.GramParser.Parse(
					DotGram.Grammar.Parsing.GramLexer.Tokenize(
						grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!));

	static bool Recurses(string grammar, string rule)
	{
		var graph = Graph(grammar);

		return graph.Recursive.Any(one => one.Name == rule);
	}

	[Fact]
	public void A_rule_that_calls_itself_recurses() =>
		Assert.True(Recurses("A = 'a' & A | 'b'\nStart = A", "A"));

	[Fact]
	public void And_two_rules_that_call_each_other_both_do() =>
		Assert.All(
			new[] { "A", "B" },
			rule => Assert.True(Recurses("A = 'a' & B | 'x'\nB = 'b' & A | 'y'\nStart = A", rule)));

	[Fact]
	public void And_a_rule_that_only_reaches_a_cycle_does_not() =>
		// `Start` calls into a cycle and is not in one. Reachability alone would say it is
		// if it were asked the wrong question — "can I reach a recursive rule" rather than
		// "can I reach myself".
		Assert.False(Recurses("A = 'a' & B | 'x'\nB = 'b' & A | 'y'\nStart = A", "Start"));

	[Fact]
	public void And_a_chain_that_does_not_come_back_recurses_nowhere() =>
		Assert.All(
			new[] { "A", "B", "C" },
			rule => Assert.False(Recurses("A = 'a' & B\nB = 'b' & C\nC = 'c'\nStart = A", rule)));

	/// <summary>Two cycles sharing a rule are one component; two that only touch are two.</summary>
	/// <remarks>
	/// The case an implementation gets wrong by merging on any shared edge rather than on
	/// mutual reachability. `A ↔ B` and `C ↔ D` with `B` calling `C` is two cycles: `C`
	/// cannot get back to `B`.
	/// </remarks>
	[Fact]
	public void And_cycles_that_touch_without_joining_stay_apart()
	{
		const string Grammar =
			"""
			A = 'a' & B | 'p'
			B = 'b' & A | C
			C = 'c' & D | 'q'
			D = 'd' & C | 'r'
			Start = A
			""";

		// All four recurse, which reachability would also say.
		Assert.All(
			new[] { "A", "B", "C", "D" },
			rule => Assert.True(Recurses(Grammar, rule)));

		// But `A` and `C` are not in one cycle, and only components can tell.
		var graph = Graph(Grammar);
		var calls = new CallGraphProbe(graph);

		Assert.True(calls.Together("A", "B"));
		Assert.True(calls.Together("C", "D"));
		Assert.False(calls.Together("A", "C"));
		Assert.False(calls.Together("B", "D"));
	}

	/// <summary>Reaches the internal type the way the compiler's own callers do.</summary>
	sealed class CallGraphProbe
	{
		readonly object _graph;
		readonly RecognitionGraph _of;

		public CallGraphProbe(RecognitionGraph graph)
		{
			_of = graph;
			_graph = typeof(RecognitionGraph)
				.GetProperty("Calls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
				.GetValue(graph)!;
		}

		public bool Together(string one, string other) =>
			(bool)_graph.GetType()
				.GetMethod("Together")!
				.Invoke(_graph, [Rule(one), Rule(other)])!;

		RuleSymbol Rule(string name) => _of.Rules.First(rule => rule.Name == name);
	}
}
