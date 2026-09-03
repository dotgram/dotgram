using DotGram.VisualStudio;
using DotGram.Language;

using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class EmbeddedGrammarBufferAnalysisTests
{
	[Fact]
	public void ProvisionalAnalysisKeepsTranslatedColorsForTemporarilyUnclassifiedText()
	{
		var previous = new[]
		{
			Classification(10, 4, GramSyntaxKind.Identifier),
			Classification(20, 5, GramSyntaxKind.String),
		};
		var provisional = new[] { Classification(10, 4, GramSyntaxKind.Identifier) };

		var merged = EmbeddedGrammarBufferAnalysis.MergeProvisionalClassifications(previous, provisional);

		Assert.Collection(
			merged,
			item => Assert.Equal(new TextSpan(10, 4), item.Span),
			item => Assert.Equal(new TextSpan(20, 5), item.Span));
	}

	[Theory]
	[InlineData(true,  0, 1, 0, 0, 0, true)]
	[InlineData(true,  0, 0, 1, 0, 0, true)]
	[InlineData(true,  0, 0, 0, 1, 0, true)]
	[InlineData(true,  0, 0, 0, 0, 1, true)]
	[InlineData(false, 0, 1, 1, 1, 1, false)]
	[InlineData(true,  1, 1, 1, 1, 1, false)]
	[InlineData(true,  0, 0, 0, 0, 0, false)]
	public void PreservesEmbeddedAnalysisAcrossTransientSyntaxErrors(
		bool hasSyntaxErrors,
		int analysisCount,
		int previousClassificationCount,
		int previousDslClassificationCount,
		int previousSymbolCount,
		int previousDslSiteCount,
		bool expected) =>
		Assert.Equal(expected, EmbeddedGrammarBufferAnalysis.ShouldPreserveEmbeddedAnalysis(
			hasSyntaxErrors,
			analysisCount,
			previousClassificationCount,
			previousDslClassificationCount,
			previousSymbolCount,
			previousDslSiteCount));

	static HostClassification Classification(int start, int length, GramSyntaxKind kind) =>
		new(new TextSpan(start, length), kind, null, null, default, null, 0, null);
}
