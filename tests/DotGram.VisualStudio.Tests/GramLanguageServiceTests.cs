using System;
using System.Linq;

using DotGram.Language;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class GramLanguageServiceTests
{
	[Fact]
	public void ClassifiesCompilerTokensAtTheirOriginalSpans()
	{
		const string source = "Start = when @(true) & 'a' & [\\p{Lu}]\nparse Start";

		var document = GramLanguageService.Analyze(source);

		Assert.Empty(document.Diagnostics);
		Assert.Equal(
			new[]
			{
				("Start", GramSyntaxKind.Identifier),
				("=", GramSyntaxKind.Operator),
				("when", GramSyntaxKind.Identifier),
				("@(true)", GramSyntaxKind.EmbeddedCode),
				("&", GramSyntaxKind.Operator),
				("'a'", GramSyntaxKind.Character),
				("&", GramSyntaxKind.Operator),
				("[", GramSyntaxKind.Punctuation),
				("\\p{Lu}", GramSyntaxKind.CharacterClass),
				("]", GramSyntaxKind.Punctuation),
				("parse", GramSyntaxKind.Identifier),
				("Start", GramSyntaxKind.Identifier),
			},
			document.Classifications
				.Select(span => (source.Substring(span.Position, span.Length), span.Kind))
				.ToArray());
	}

	[Fact]
	public void ReturnsCompilerDiagnosticsWithoutEditorSpecificTypes()
	{
		const string source = "Start = Missing\nparse Start";

		var diagnostic = Assert.Single(GramLanguageService.Analyze(source).Diagnostics);

		Assert.Equal("GRAM3002", diagnostic.Id);
		Assert.Equal(source.IndexOf("Missing", StringComparison.Ordinal), diagnostic.Position);
		Assert.StartsWith("Missing", source.Substring(diagnostic.Position, diagnostic.Length));
	}

	[Fact]
	public void RejectsNullText()
	{
		Assert.Throws<ArgumentNullException>(() => GramLanguageService.Analyze(null!));
	}
}
