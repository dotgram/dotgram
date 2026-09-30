using System;
using System.Linq;

using DotGram.Language;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class GramDiagnosticTextTests
{
	[Fact]
	public void QuickInfoSaysWhatTheSquiggleUnderItSays()
	{
		// Quick Info over a grammar shows DotGram's own content alone, so the text Visual Studio
		// keeps for a squiggle is dropped wherever DotGram has something to say — which is on
		// every word. The message has to come from the squiggle's own diagnostic.
		const string source = "using Missing;\nStart = 'a'\nparse Start";
		var document = GramLanguageService.Analyze(source);
		var squiggles = document.Diagnostics.Select(static diagnostic =>
			(diagnostic.Position, diagnostic.Length, diagnostic));

		var underUsing = GramDiagnosticText.At(
			squiggles, source.IndexOf("Missing", StringComparison.Ordinal), source.Length);
		var underRule = GramDiagnosticText.At(
			squiggles, source.IndexOf("Start", StringComparison.Ordinal), source.Length);

		Assert.Equal(["GRAM3003: No namespace named 'Missing' is in view here."], underUsing);
		Assert.Empty(underRule);
	}

	[Fact]
	public void QuickInfoFindsASquiggleWhereItIsDrawnRatherThanWhereItPoints()
	{
		// An empty diagnostic at the end of the text is drawn under the last character.
		var diagnostic = new DotGram.Grammar.GramDiagnostic(
			"GRAM2001", "Something is missing here.", 10, 0, DotGram.Grammar.GramSeverity.Error);

		Assert.Equal((9, 1), GramDiagnosticText.Span(10, 0, 10));
		Assert.Equal(
			["GRAM2001: Something is missing here."],
			GramDiagnosticText.At([(10, 0, diagnostic)], 9, 10));
	}
}
