using DotGram.VisualStudio;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class EmbeddedGrammarBufferAnalysisTests
{
	[Theory]
	[InlineData(true,  0, 1, true)]
	[InlineData(false, 0, 1, false)]
	[InlineData(true,  1, 1, false)]
	[InlineData(true,  0, 0, false)]
	public void PreservesNavigationOnlyAcrossTransientSyntaxErrors(
		bool hasSyntaxErrors,
		int analysisCount,
		int previousSymbolCount,
		bool expected) =>
		Assert.Equal(expected, EmbeddedGrammarBufferAnalysis.ShouldPreserveDocumentSymbols(
			hasSyntaxErrors,
			analysisCount,
			previousSymbolCount));
}
