using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;
using DotGram.VisualStudio;
using DotGram.Tests;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class DslRecognitionTraceTests
{
	[Fact]
	public void ReportsRuleAndCaptureExtentsFromSuccessfulDerivation()
	{
		var (graph, publication) = Compile("""
			parse Start
			Start = first: Word & (' ' & second: Word)?
			Word = ['a'..'z']+
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "one two");

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture is null &&
			extent.Position == 0 && extent.Length == 7);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture == "first" &&
			extent.Position == 0 && extent.Length == 3);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture == "second" &&
			extent.Position == 4 && extent.Length == 3);
		Assert.Equal(2, result.Extents.Count(extent => extent.Rule.Name == "Word" && extent.Capture is null));
	}

	[Fact]
	public void BacktracksFromEarlierAlternativeWhenFollowingInputFails()
	{
		var (graph, publication) = Compile("""
			parse Start
			Start = ('a' | "ab") & 'c'
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "abc");

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
	}

	[Fact]
	public void WholeParseIncludesLeadingAndTrailingTrivia()
	{
		var (graph, publication) = Compile("""
			trivia = ' '*
			parse Start
			Start = word: (['a'..'z']+)
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "  word ");

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture == "word" &&
			extent.Position == 2 && extent.Length == 4);
	}

	[Fact]
	public void AtomicCommitsToFirstSuccessfulAlternative()
	{
		var (graph, publication) = Compile("""
			parse Start
			Start = { 'a' | "ab" } & 'c'
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "abc");

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
	}

	[Fact]
	public void ReportsExpectedElementsAtFurthestFailure()
	{
		var (graph, publication) = Compile("""
			Keyword = "select"
			parse Start
			Start = Keyword & ' ' & ("name" | "count")
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "select value");

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
		Assert.Equal(7, result.FailurePosition);
		Assert.Equal(new[] { "\"count\"", "\"name\"" }, result.Expected);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Keyword" && extent.Position == 0 && extent.Length == 6);
	}

	[Fact]
	public void ReportsOperatorAlternativesAfterTrailingTrivia()
	{
		var (graph, publication) = Compile("""
			trivia    = [' ' | '\t']*
			Identifier = ['a'..'z']+
			Operator   = '+' | '-'
			Operation  = left: Identifier & Operator & right: Identifier
			parse Operation
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "customer ");

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
		Assert.Equal("customer ".Length, result.FailurePosition);
		Assert.Contains("['+' | '-']", result.Expected);
		var successful = DslRecognitionTrace.Recognize(graph, publication, "customer + value");
		Assert.Contains(successful.Extents, extent =>
			extent.Rule.Name == "Operation" && extent.Capture == "left");
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Operation" && extent.Capture == "left" &&
			extent.Position == 0 && extent.Length == "customer".Length);
	}

	[Fact]
	public void DoesNotGuessWhenRecognitionRequiresUserCode()
	{
		var (graph, publication) = ExternalGraph();

		var result = DslRecognitionTrace.Recognize(graph, publication, "anything");

		Assert.Equal(DslRecognitionStatus.Unsupported, result.Status);
	}

	[Fact]
	public void UsesToolingContractForGuardsWithoutExecutingUserCode()
	{
		var (graph, publication) = Compile("""
			parse Start
			Start = when @(Allowed) & value: ['a'..'z']+
			""");
		var accepted = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"word",
			new Contract(guard: true));
		var rejected = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"word",
			new Contract(guard: false));

		Assert.Equal(DslRecognitionStatus.Success, accepted.Status);
		Assert.Contains(accepted.Extents, extent => extent.Capture == "value");
		Assert.Equal(DslRecognitionStatus.Failure, rejected.Status);
	}

	[Fact]
	public void UsesToolingContractForExternalRecognizerExtent()
	{
		var (graph, publication) = ExternalGraph();

		var result = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"name",
			new Contract(externalRule: graph.Rules.Single(rule => rule.Name == "External")));

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Position == 0 && extent.Length == 4);
	}

	[Fact]
	public void UsesMappedGrammarRuleForExternalRecognizer()
	{
		var (graph, publication) = ExternalGraph();

		var result = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"other",
			new Contract(externalRule: graph.Rules.Single(rule => rule.Name == "External")));

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
	}

	[Fact]
	public void UsesDescriptorContractForGuardAndExternalRecognizer()
	{
		var (graph, publication) = Compile("""
			Word = ['a'..'z']+
			Start = when @(Allowed) & @Read
			parse Start
			""");
		var guard = Nodes(graph.Bodies[publication.Rule]).OfType<Node.Guard>().Single();
		var definition = new DslRecognitionContractDefinition(
			new System.Collections.Generic.Dictionary<string, bool> { [guard.Text] = true },
			new System.Collections.Generic.Dictionary<string, string> { ["Read"] = "Word" });

		var result = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"customer",
			new DslDescriptorRecognitionContract(graph, definition));

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Word" && extent.Position == 0 && extent.Length == 8);
	}

	[Fact]
	public void DescriptorContractMatchesGeneratedParserOnBoundedCorpus()
	{
		const string grammar = """
			Word = ['a'..'z']+
			Start = when @(true) & @Read
			parse Start
			""";
		var (graph, publication) = Compile(grammar);
		var guard = Nodes(graph.Bodies[publication.Rule]).OfType<Node.Guard>().Single();
		var contract = new DslDescriptorRecognitionContract(
			graph,
			new DslRecognitionContractDefinition(
				new System.Collections.Generic.Dictionary<string, bool> { [guard.Text] = true },
				new System.Collections.Generic.Dictionary<string, string> { ["Read"] = "Word" }));
		var generated = EmittedCode.Compile(
			DotGram.Grammar.Emit.CSharpEmitter.Emit(graph, "Grammar"),
			declarationMembers: """
				static bool Read(global::System.ReadOnlySpan<char> input, ref int pos)
				{
					var start = pos;
					while (pos < input.Length && input[pos] >= 'a' && input[pos] <= 'z') pos++;
					return pos > start;
				}
				""");

		foreach (var input in new[] { "", "name", "customer", "123", "name1", "Name" })
		{
			var expected = EmittedCode.Match(generated, "Grammar", "TryParseStart", input).IsSuccess;
			var actual = DslRecognitionTrace.Recognize(graph, publication, input, contract).Status ==
				DslRecognitionStatus.Success;

			Assert.Equal(expected, actual);
		}
	}

	static (RecognitionGraph Graph, Publication Publication) ExternalGraph()
	{
		var rule = new RuleSymbol("Start", new GrammarNamespace("", null), Declaration: null);
		var external = new RuleSymbol("External", new GrammarNamespace("", null), Declaration: null);
		var graph = new RecognitionGraph(
			[rule, external],
			new System.Collections.Generic.Dictionary<RuleSymbol, Node>
			{
				[rule] = new Node.External("Read"),
				[external] = new Node.Literal("name"),
			},
			new System.Collections.Generic.Dictionary<RuleSymbol, bool> { [rule] = false, [external] = false },
			new System.Collections.Generic.Dictionary<RuleSymbol, System.Collections.Generic.IReadOnlyList<ResultMember>>(),
			new System.Collections.Generic.Dictionary<RuleSymbol, string>(),
			[],
			[],
			[]);
		var publication = new Publication(
			PublishKind.Parse,
			rule,
			"ParseStart",
			new Location(0, 0),
			rule.Namespace,
			new System.Collections.Generic.Dictionary<RuleSymbol, RuleSymbol>(),
			[]);

		return (graph, publication);
	}

	sealed class Contract(bool? guard = null, RuleSymbol? externalRule = null) : IDslRecognitionContract
	{
		public bool TryEvaluateGuard(Node.Guard guardNode, RuleSymbol owner, int position, out bool accepted)
		{
			accepted = guard.GetValueOrDefault();
			return guard.HasValue;
		}

		public bool TryResolveExternal(
			Node.External external,
			RuleSymbol owner,
			out RuleSymbol rule)
		{
			rule = externalRule!;
			return externalRule is not null;
		}
	}

	static System.Collections.Generic.IEnumerable<Node> Nodes(Node node)
	{
		yield return node;
		foreach (var child in node.Children)
		foreach (var descendant in Nodes(child))
			yield return descendant;
	}

	static (RecognitionGraph Graph, Publication Publication) Compile(string source)
	{
		var parsed = GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance));
		var model  = GrammarBinder.Bind(parsed.File);
		var graph  = GrammarNormalizer.Normalize(model);

		Assert.Empty(parsed.Diagnostics);
		Assert.Empty(model.Diagnostics);
		Assert.Empty(graph.Diagnostics);

		return (graph, Assert.Single(graph.Publications));
	}
}
