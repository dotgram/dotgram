using DotGram.VisualStudio;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class EmbeddedGrammarBufferAnalysisTests
{
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
}
