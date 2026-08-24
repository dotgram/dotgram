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
		var source   = Host("\"Start = \\u007e\"");
		var analysis = Assert.Single(Analyze(source));
		var told     = Assert.Single(analysis.Diagnostics);

		Assert.Equal("GRAM1005", told.Diagnostic.Id);
		Assert.True(told.IsExact);
		Assert.Equal("\\u007e", source.Substring(told.Span.Start, told.Span.Length));
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
