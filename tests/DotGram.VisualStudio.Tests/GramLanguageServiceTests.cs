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
				("when", GramSyntaxKind.Keyword),
				("@", GramSyntaxKind.Transition),
				("(", GramSyntaxKind.Punctuation),
				("true", GramSyntaxKind.Keyword),
				(")", GramSyntaxKind.Punctuation),
				("&", GramSyntaxKind.SpecialSymbol),
				("'a'", GramSyntaxKind.Character),
				("&", GramSyntaxKind.SpecialSymbol),
				("[", GramSyntaxKind.Punctuation),
				("\\p{Lu}", GramSyntaxKind.CharacterClass),
				("]", GramSyntaxKind.Punctuation),
				("parse", GramSyntaxKind.Keyword),
				("Start", GramSyntaxKind.Identifier),
			},
			document.Classifications
				.Select(span => (source.Substring(span.Position, span.Length), span.Kind))
				.ToArray());
	}

	[Fact]
	public void UsesRoslynTokenKindsForBothCSharpValueForms()
	{
		const string source = "Primary : @int = '(' & inner: Sum & ')' => @(inner)\n" +
			"        | digits: ['0'..'9']+ => @int.Parse(digits)";

		var classified = GramLanguageService.Analyze(source).Classifications
			.Select(span => (Text: source.Substring(span.Position, span.Length), span.Kind))
			.ToArray();

		Assert.Contains(("int", GramSyntaxKind.Keyword), classified);
		Assert.Contains(("@", GramSyntaxKind.Transition), classified);
		Assert.Contains(("Parse", GramSyntaxKind.Identifier), classified);
		Assert.Contains(("inner", GramSyntaxKind.Identifier), classified);
		Assert.Contains(("digits", GramSyntaxKind.Identifier), classified);
		Assert.DoesNotContain(classified, item => item.Kind == GramSyntaxKind.EmbeddedCode);
	}

	[Fact]
	public void ClassifiesCommentsWithoutTreatingLiteralContentsAsComments()
	{
		const string source = "// line\nStart = \"/* text */\" /* block */\nparse Start";

		var classified = GramLanguageService.Analyze(source).Classifications
			.Select(span => (Text: source.Substring(span.Position, span.Length), span.Kind))
			.ToArray();

		Assert.Contains(("// line", GramSyntaxKind.Comment), classified);
		Assert.Contains(("/* block */", GramSyntaxKind.Comment), classified);
		Assert.Contains(("\"/* text */\"", GramSyntaxKind.String), classified);
		Assert.DoesNotContain(("/* text */", GramSyntaxKind.Comment), classified);
	}

	[Fact]
	public void ClassifiesGrammarMetacharactersAsSpecialSymbols()
	{
		const string source = "Start = 'a'* | ?! 'b'";

		var classified = GramLanguageService.Analyze(source).Classifications
			.Select(span => (Text: source.Substring(span.Position, span.Length), span.Kind))
			.ToArray();

		Assert.Contains(("*", GramSyntaxKind.SpecialSymbol), classified);
		Assert.Contains(("|", GramSyntaxKind.SpecialSymbol), classified);
		Assert.Contains(("?!", GramSyntaxKind.SpecialSymbol), classified);
	}

	[Fact]
	public void AttachesCompleteRuleDefinitionToRuleReferences()
	{
		const string source = "Start = 'a'\n      | 'b'\nparse Start";
		const string definition = "Start = 'a'\n      | 'b'";

		var references = GramLanguageService.Analyze(source).Classifications
			.Where(span => source.Substring(span.Position, span.Length) == "Start")
			.ToArray();

		Assert.Equal(2, references.Length);
		Assert.All(references, span => Assert.Equal(definition, span.QuickInfo));
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
