using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;
using DotGram.VisualStudio;

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
	public void DoesNotGuessWhenRecognitionRequiresUserCode()
	{
		var rule = new RuleSymbol("Start", new GrammarNamespace("", null), Declaration: null);
		var graph = new RecognitionGraph(
			[rule],
			new System.Collections.Generic.Dictionary<RuleSymbol, Node>
			{
				[rule] = new Node.External("Read"),
			},
			new System.Collections.Generic.Dictionary<RuleSymbol, bool> { [rule] = false },
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

		var result = DslRecognitionTrace.Recognize(graph, publication, "anything");

		Assert.Equal(DslRecognitionStatus.Unsupported, result.Status);
	}

	static (RecognitionGraph Graph, Publication Publication) Compile(string source)
	{
		var parsed = GramParser.Parse(GramLexer.Tokenize(source));
		var model  = GrammarBinder.Bind(parsed.File);
		var graph  = GrammarNormalizer.Normalize(model);

		Assert.Empty(parsed.Diagnostics);
		Assert.Empty(model.Diagnostics);
		Assert.Empty(graph.Diagnostics);

		return (graph, Assert.Single(graph.Publications));
	}
}
