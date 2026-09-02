using System;
using System.Linq;

using DotGram.Language;
using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class EmbeddedGrammarAnalysisTests
{
	[Fact]
	public void MapsClassificationsIntoRawStringHostPositions()
	{
		var source = Host(""""
			"""
			Start = 'a'
			parse Start
			"""
			"""");

		var analysis = Assert.Single(Analyze(source));
		var classified = analysis.Classifications
			.Select(item => (source.Substring(item.Span.Start, item.Span.Length), item.Kind))
			.ToArray();

		Assert.Contains(("'a'", GramSyntaxKind.Character), classified);
		Assert.Contains(("parse", GramSyntaxKind.Keyword), classified);

		var references = analysis.Classifications
			.Where(item => source.Substring(item.Span.Start, item.Span.Length) == "Start")
			.ToArray();

		Assert.Equal(2, references.Length);
		Assert.All(references, item => Assert.Equal(references[0].Span, item.DefinitionSpan));

		var symbols = analysis.Symbols;
		Assert.Equal(2, symbols.Count);
		Assert.Equal(new[] { "Start", "Start" }, symbols.Select(symbol => symbol.Name).ToArray());
		Assert.Equal(symbols[0].Span, symbols[0].DefinitionSpan);
		Assert.Equal(symbols[0].DefinitionSpan, symbols[1].DefinitionSpan);
		Assert.True(symbols[0].IsDefinition);
		Assert.False(symbols[1].IsDefinition);
		Assert.Empty(analysis.Diagnostics);
	}

	[Fact]
	public void MapsDiagnosticAcrossRegularStringEscape()
	{
		var source   = Host("\"Start = \\u0060\"");
		var analysis = Assert.Single(Analyze(source));
		var told     = Assert.Single(analysis.Diagnostics);

		Assert.Equal("GRAM1005", told.Diagnostic.Id);
		Assert.True(told.IsExact);
		Assert.Equal("\\u0060", source.Substring(told.Span.Start, told.Span.Length));
	}

	[Fact]
	public void MapsGivesBackRuleMarkerQuickInfo()
	{
		var source = Host("\"Backtracking? = 'a'\"");

		var marker = Assert.Single(Assert.Single(Analyze(source)).Classifications, item =>
			source.Substring(item.Span.Start, item.Span.Length) == "?");

		Assert.Equal(GramSyntaxKind.SpecialSymbol, marker.Kind);
		Assert.Contains("rule may give back", marker.QuickInfo, StringComparison.Ordinal);
	}

	[Fact]
	public void MapsLocalSymbolsAndTheirRuleScope()
	{
		var source = Host(""""
			"""
			Item = 'a'
			Start(value) = item: value => @Make(item)
			"""
			"""");

		var analysis = Assert.Single(Analyze(source));
		var parameter = analysis.Symbols.First(symbol =>
			symbol.Name == "value" && symbol.IsDefinition);
		var capture = analysis.Symbols.First(symbol =>
			symbol.Name == "item" && symbol.IsDefinition);

		Assert.Equal(GramSymbolKind.Parameter, parameter.Kind);
		Assert.Equal(GramSymbolKind.Capture, capture.Kind);
		Assert.True(parameter.ScopeSpan.Contains(parameter.Span));
		Assert.True(capture.ScopeSpan.Contains(capture.Span));
		Assert.Equal(parameter.ScopeSpan, capture.ScopeSpan);
	}

	[Fact]
	public void MapsLocalReferencesInsideCSharpExpressions()
	{
		var source = Host("\"Start = left: any => @(Use(left, left))\"");

		var analysis = Assert.Single(Analyze(source));
		var left = analysis.Symbols.Where(symbol => symbol.Name == "left").ToArray();

		Assert.Equal(3, left.Length);
		Assert.Single(left, symbol => symbol.IsDefinition);
		Assert.All(left, symbol => Assert.Equal(left[0].DefinitionSpan, symbol.DefinitionSpan));
		Assert.All(left, symbol => Assert.True(symbol.GrammarSpan.Contains(symbol.Span)));
	}

	[Fact]
	public void MapsBracePairsAndFoldingRanges()
	{
		var source = Host(""""
			"""
			Start = (
				['a']
			) => @(Call())
			"""
			"""");

		var analysis = Assert.Single(Analyze(source));
		Assert.Equal(4, analysis.Braces.Count);
		Assert.All(analysis.Braces, pair =>
		{
			Assert.True(pair.GrammarSpan.Contains(pair.OpenSpan));
			Assert.True(pair.GrammarSpan.Contains(pair.CloseSpan));
		});
		Assert.Equal(2, analysis.FoldingRanges.Count);
		Assert.All(analysis.FoldingRanges, range => Assert.True(range.GrammarSpan.Contains(range.Span)));
		var symbol = Assert.Single(analysis.DocumentSymbols);
		Assert.Equal("Start", symbol.Name);
		Assert.Equal(GramDocumentSymbolKind.Rule, symbol.Kind);
		Assert.True(symbol.GrammarSpan.Contains(symbol.SelectionSpan));
	}

	[Fact]
	public void MapsGeneratedApiNamesIntoTheHostString()
	{
		var source = Host("\"Start = 'a'\\nparse Start as ReadStart\"");

		var publication = Assert.Single(Assert.Single(Analyze(source)).PublishedApis);

		Assert.Equal("ReadStart", publication.MethodName);
		Assert.Equal("ReadStart", source.Substring(publication.Span.Start, publication.Span.Length));
		Assert.True(publication.GrammarSpan.Contains(publication.Span));
	}

	static string Host(string literal) => $$"""
		using DG = DotGram;

		namespace DotGram
		{
			sealed class GramAttribute(string text) : System.Attribute;
		}

		[DG.Gram({{literal}})]
		class Parser;
		""";

	static EmbeddedGrammarAnalysis[] Analyze(string source)
	{
		var tree = CSharpSyntaxTree.ParseText(source);
		var compilation = CSharpCompilation.Create(
			"Host",
			[tree],
			[MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]);

		return EmbeddedGrammarService
			.Analyze(compilation.GetSemanticModel(tree), tree.GetRoot())
			.ToArray();
	}
}
