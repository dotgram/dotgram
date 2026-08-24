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
		Assert.Contains(("parse", GramSyntaxKind.Identifier), classified);
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
