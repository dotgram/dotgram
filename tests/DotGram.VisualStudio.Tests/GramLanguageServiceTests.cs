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
		Assert.All(references, span => Assert.Equal(0, span.DefinitionPosition));
	}

	[Fact]
	public void ExpandsReferencedRulesAndMarksRecursionInQuickInfo()
	{
		const string source = "Expression = Primary\nPrimary = '(' & Expression & ')' | '0'";

		var expression = GramLanguageService.Analyze(source).Classifications
			.First(span => source.Substring(span.Position, span.Length) == "Expression");

		Assert.Equal(
			"Expression = Primary\n\n" +
			"Referenced rule:\nPrimary = '(' & Expression & ')' | '0'\n\n" +
			"Recursive reference: Expression",
			expression.QuickInfo);
	}

	[Fact]
	public void AttachesParameterizedRuleSignatureToReferences()
	{
		const string source = "Repeat(count: @int, value: @char) = any\nStart = Repeat(2, 'a')";

		var references = GramLanguageService.Analyze(source).Classifications
			.Where(span => source.Substring(span.Position, span.Length) == "Repeat")
			.ToArray();

		Assert.Equal(2, references.Length);
		Assert.All(references, span => Assert.Equal("Repeat(count: @int, value: @char)", span.RuleSignature));
		Assert.All(references, span => Assert.Equal(2, span.RuleParameterCount));
	}

	[Fact]
	public void RuleQuickInfoDoesNotIncludeTriviaBeforeNextDeclaration()
	{
		const string source = "Digits(n) = any{n}\n\n// The next rule.\nStart = Digits(2)";

		var digits = GramLanguageService.Analyze(source).Classifications
			.First(span => source.Substring(span.Position, span.Length) == "Digits");

		Assert.Equal("Digits(n) = any{n}", digits.QuickInfo);
	}

	[Fact]
	public void IndexesRuleDeclarationsAndReferencesAtTheirOriginalSpans()
	{
		const string source = "Item = 'a'\n" +
			"List(value) = value+\n" +
			"Start = Item & List(Item) & [Item]\n" +
			"parse Start";

		var symbols = GramLanguageService.Analyze(source).Symbols;

		Assert.Equal(
			new[] { "Item", "List", "Start", "Item", "List", "Item", "Item", "Start" },
			symbols.Select(symbol => source.Substring(symbol.Position, symbol.Length)).ToArray());
		Assert.Equal(4, symbols.Count(symbol => symbol.Name == "Item"));
		Assert.Equal(2, symbols.Count(symbol => symbol.Name == "List"));
		Assert.Equal(2, symbols.Count(symbol => symbol.Name == "Start"));

		foreach (var group in symbols.GroupBy(symbol => symbol.Name))
		{
			var definition = Assert.Single(group, symbol => symbol.IsDefinition);
			Assert.All(group, symbol => Assert.Equal(definition.Position, symbol.DefinitionPosition));
		}
	}

	[Fact]
	public void DoesNotIndexCSharpReferencesAsGrammarRules()
	{
		const string source = "Value = 'a' => @Value";

		var value = Assert.Single(GramLanguageService.Analyze(source).Symbols);

		Assert.True(value.IsDefinition);
		Assert.Equal(0, value.Position);
	}

	[Fact]
	public void ReturnsCompilerDiagnosticsWithoutEditorSpecificTypes()
	{
		const string source = "Start = Missing\nparse Start";

		var diagnostic = Assert.Single(GramLanguageService.Analyze(source).Diagnostics);

		Assert.Equal("GRAM3002", diagnostic.Id);
		Assert.Equal(source.IndexOf("Missing", StringComparison.Ordinal), diagnostic.Position);
		Assert.Equal("Missing", source.Substring(diagnostic.Position, diagnostic.Length));
	}

	[Fact]
	public void RejectsNullText()
	{
		Assert.Throws<ArgumentNullException>(() => GramLanguageService.Analyze(null!));
	}
}
