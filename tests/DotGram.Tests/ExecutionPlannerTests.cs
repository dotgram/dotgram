using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

public sealed class ExecutionPlannerTests
{
	[Theory]
	[MemberData(nameof(ExampleClassifications))]
	public void Existing_examples_have_an_explicit_recursion_classification(
		string file, string[] expected)
	{
		var graph = Normalized(GrammarIn(Path.Combine(Examples, file)));
		var plan = ExecutionPlanner.Analyze(graph);
		var actual = graph.Rules.Where(plan.IsRecursive).Select(rule => rule.Name);

		Assert.Equal(expected, actual);
	}

	public static TheoryData<string, string[]> ExampleClassifications => new()
	{
		{ "CalculatorExample.cs",        ["Sum", "Product", "Unary", "Primary"] },
		{ "DecimalCalculatorExample.cs", ["Sum", "Product", "Unary", "Power", "Primary"] },
		{ "ExpressionTreeExample.cs",    ["Sum", "Product", "Unary", "Power", "Primary"] },
		{ "FeedExample.cs",              [] },
		{ "FilterExample.cs",            ["Expr"] },
		{ "FixedWidthExample.cs",        [] },
		{ "FixExample.cs",               [] },
		{ "HttpHeadersExample.cs",       [] },
		{ "IniExample.cs",               [] },
		{ "JsonExample.cs",              ["Value", "Object", "Array", "Member", "List_Member_0", "List_Value_1"] },
		{ "LoggingFeedExample.cs",       [] },
		{ "MarkdownExample.cs",          [] },
		{ "NetstringExample.cs",         [] },
		{ "OneRuleTreeExample.cs",       ["Expr"] },
		{ "RecoveringFeedExample.cs",    [] },
		{ "SqlReadOnlyExample.cs",       [] },
		{ "StreamingFeedExample.cs",     [] },
		{ "StrengthCalculatorExample.cs", ["Expr"] },
		{ "TypedCsvExample.cs",          [] },
		{ "UrlExample.cs",               [] },
		{ "XmlExample.cs",               ["Element", "Content"] },
		{ "YamlExample.cs",              [] },
	};

	[Fact]
	public void Direct_left_recursion_is_a_fold_not_runtime_recursion()
	{
		var graph = Normalized("Value = Value & 'x' | 'y'");
		var value = Rule(graph, "Value");
		var plan  = ExecutionPlanner.Analyze(graph);

		Assert.Empty(graph.Diagnostics);
		Assert.True(graph.Folds.ContainsKey(value));
		Assert.False(plan.IsRecursive(value));
		Assert.Equal(1, plan.MaximumCallDepth[value]);
	}

	[Fact]
	public void A_self_call_outside_the_left_recursive_head_is_recursive()
	{
		var graph = Normalized("Value = '(' & Value & ')' | 'x'");
		var value = Rule(graph, "Value");
		var plan  = ExecutionPlanner.Analyze(graph);

		Assert.Empty(graph.Diagnostics);
		Assert.True(plan.IsRecursive(value));
		Assert.Null(plan.MaximumCallDepth[value]);
	}

	[Fact]
	public void Mutually_recursive_rules_form_one_component()
	{
		var graph = Normalized(
			"""
			A = 'a' & B | 'x'
			B = 'b' & A | 'y'
			""");
		var a    = Rule(graph, "A");
		var b    = Rule(graph, "B");
		var plan = ExecutionPlanner.Analyze(graph);

		Assert.Empty(graph.Diagnostics);
		Assert.Same(plan.ComponentOf[a], plan.ComponentOf[b]);
		Assert.True(plan.ComponentOf[a].IsRecursive);
		Assert.Equal(new[] { "A", "B" }, plan.ComponentOf[a].Rules.Select(rule => rule.Name));
	}

	[Fact]
	public void Acyclic_depth_includes_the_entered_rule()
	{
		var graph = Normalized(
			"""
			A = B
			B = C
			C = 'c'
			""");
		var plan = ExecutionPlanner.Analyze(graph);

		Assert.Empty(graph.Diagnostics);
		Assert.Equal(3, plan.MaximumCallDepth[Rule(graph, "A")]);
		Assert.Equal(2, plan.MaximumCallDepth[Rule(graph, "B")]);
		Assert.Equal(1, plan.MaximumCallDepth[Rule(graph, "C")]);
	}

	[Fact]
	public void Counts_call_sites_without_duplicating_shared_rule_blocks()
	{
		var graph = Normalized(
			"""
			Start = Name & Name | { Name & ':' & Name }
			Name = ['a'..'z']+
			""");
		var start = Rule(graph, "Start");
		var name  = Rule(graph, "Name");
		var plan  = ExecutionPlanner.Analyze(graph);

		Assert.Empty(graph.Diagnostics);
		Assert.Equal(0, plan.CallSites[start]);
		Assert.Equal(4, plan.CallSites[name]);
		Assert.Equal(new[] { name }, plan.Calls[start]);
	}

	[Fact]
	public void A_direct_rule_can_reach_a_recursive_component()
	{
		var graph = Normalized(
			"""
			Start = Value
			Value = '(' & Value & ')' | 'x'
			""");
		var start = Rule(graph, "Start");
		var value = Rule(graph, "Value");
		var plan  = ExecutionPlanner.Analyze(graph);

		Assert.False(plan.IsRecursive(start));
		Assert.True(plan.IsRecursive(value));
		Assert.True(plan.ReachesRecursion(start));
		Assert.Null(plan.MaximumCallDepth[start]);
	}

	[Fact]
	public void Recovery_synchronization_is_part_of_the_owner_execution()
	{
		var graph = Normalized(
			"""
			A = Row* recover Sync
			Row = 'r'
			Sync = 's' & B
			B = 'b' & A
			""");
		var a    = Rule(graph, "A");
		var sync = Rule(graph, "Sync");
		var b    = Rule(graph, "B");
		var plan = ExecutionPlanner.Analyze(graph);

		Assert.Empty(graph.Diagnostics);
		Assert.Contains(sync, plan.Calls[a]);
		Assert.Same(plan.ComponentOf[a], plan.ComponentOf[sync]);
		Assert.Same(plan.ComponentOf[a], plan.ComponentOf[b]);
	}

	[Fact]
	public void Calls_inside_lookahead_are_runtime_edges()
	{
		var graph = Normalized(
			"""
			A = ?=B & 'a'
			B = 'b' & A | 'x'
			""");
		var a    = Rule(graph, "A");
		var b    = Rule(graph, "B");
		var plan = ExecutionPlanner.Analyze(graph);

		Assert.Empty(graph.Diagnostics);
		Assert.Contains(b, plan.Calls[a]);
		Assert.Same(plan.ComponentOf[a], plan.ComponentOf[b]);
	}

	[Fact]
	public void Analysis_itself_does_not_use_the_native_stack_for_deep_acyclic_graphs()
	{
		const int count = 10_000;

		var scope = new GrammarScope("", null);
		var rules = Enumerable.Range(0, count)
			.Select(index => new RuleSymbol("R" + index, scope, null))
			.ToArray();
		var bodies = new Dictionary<RuleSymbol, Node>();
		var nullable = new Dictionary<RuleSymbol, bool>();
		var results = new Dictionary<RuleSymbol, IReadOnlyList<ResultMember>>();

		for (var i = 0; i < count; i++)
		{
			bodies[rules[i]] = i + 1 < count
				? new Node.Call(rules[i + 1], [])
				: new Node.Literal("x");
			nullable[rules[i]] = false;
			results[rules[i]] = [];
		}

		var graph = new RecognitionGraph(
			rules,
			bodies,
			nullable,
			results,
			new Dictionary<RuleSymbol, string>(),
			[],
			[],
			[]);
		var plan = ExecutionPlanner.Analyze(graph);

		Assert.Equal(count, plan.MaximumCallDepth[rules[0]]);
		Assert.Equal(RuleExecutionKind.Direct, plan.Kinds[rules[0]]);
	}

	static RecognitionGraph Normalized(string text) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));

	static RuleSymbol Rule(RecognitionGraph graph, string name) =>
		Assert.Single(graph.Rules, rule => rule.Name == name);

	static string GrammarIn(string path)
	{
		var root = CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();
		var attribute = Assert.Single(root.DescendantNodes().OfType<AttributeSyntax>(), node =>
			node.Name.ToString() is "Gram" or "GramAttribute");
		var expression = Assert.Single(attribute.ArgumentList!.Arguments).Expression;
		var literal = Assert.IsType<LiteralExpressionSyntax>(expression);

		return literal.Token.ValueText;
	}

	static string ThisFile { get; } = FilePath();
	static string Here { get; } = Path.GetDirectoryName(ThisFile)!;
	static string Examples { get; } = Path.Combine(Here, "..", "..", "examples", "DotGram.Examples");

	static string FilePath([CallerFilePath] string path = "") => path;
}
