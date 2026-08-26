using System;
using System.Linq;

using DotGram.Grammar.Emit;
using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class DslClassificationBinderTests
{
	[Fact]
	public void BindsRulesAndCapturesToDefinitionPositions()
	{
		var language = Language(
			"Start = keyword: Keyword & value: Identifier\nKeyword = ^'let'\nIdentifier = ['a'..'z']+",
			("Keyword", "Keyword"),
			("Start.value", "Variable"));

		var binding = DslClassificationBinder.Bind(language, language.GrammarSource);

		Assert.Empty(binding.Diagnostics);
		Assert.Collection(
			binding.Classifications,
			item =>
			{
				Assert.Equal(DslClassificationTargetKind.Rule, item.TargetKind);
				Assert.Equal("Keyword", item.Definition.Target);
				Assert.Null(item.CaptureDefinitionPosition);
				Assert.Equal(language.GrammarSource.IndexOf("Keyword =", StringComparison.Ordinal), item.RuleDefinitionPosition);
			},
			item =>
			{
				Assert.Equal(DslClassificationTargetKind.Capture, item.TargetKind);
				Assert.Equal("Start.value", item.Definition.Target);
				Assert.Equal(language.GrammarSource.IndexOf("Start =", StringComparison.Ordinal), item.RuleDefinitionPosition);
				Assert.Equal(language.GrammarSource.IndexOf("value:", StringComparison.Ordinal), item.CaptureDefinitionPosition);
			});
	}

	[Fact]
	public void ReportsMalformedAndUnknownTargets()
	{
		var language = Language(
			"Start = name: Identifier\nIdentifier = ['a'..'z']+",
			("Start.name.extra", "Variable"),
			("Missing", "Keyword"),
			("Start.missing", "Variable"));

		var diagnostics = DslClassificationBinder.Bind(language, language.GrammarSource).Diagnostics;

		Assert.Equal(
			new[]
			{
				DslClassificationBindingDiagnosticKind.MalformedTarget,
				DslClassificationBindingDiagnosticKind.UnknownRule,
				DslClassificationBindingDiagnosticKind.UnknownCapture,
			},
			diagnostics.Select(static diagnostic => diagnostic.Kind));
	}

	[Fact]
	public void ReportsAmbiguousRulesAndDuplicateSymbolTargets()
	{
		var language = Language(
			"namespace One { Value = 'one' }\nnamespace Two { Value = 'two' }\nStart = name: Name\nName = ['a'..'z']+",
			("Value", "Keyword"),
			("Name", "Identifier"),
			("Name", "Variable"),
			("Start.name", "Variable"),
			("Start.name", "Identifier"));

		var binding = DslClassificationBinder.Bind(language, language.GrammarSource);

		Assert.Equal(2, binding.Classifications.Count);
		Assert.Equal(
			new[]
			{
				DslClassificationBindingDiagnosticKind.AmbiguousRule,
				DslClassificationBindingDiagnosticKind.DuplicateTarget,
				DslClassificationBindingDiagnosticKind.DuplicateTarget,
			},
			binding.Diagnostics.Select(static diagnostic => diagnostic.Kind));
	}

	static DslLanguageDefinition Language(string grammar, params (string Target, string Role)[] classifications)
	{
		var attributes = string.Join("\n", classifications.Select(item =>
			$"[DotGram.GramClassify(\"{item.Target}\", DotGram.GramClassification.{item.Role})]"));
		var source = SupportEmitter.Attributes + $$"""

			[DotGram.Gram({{SymbolDisplay.FormatLiteral(grammar, true)}})]
			[DotGram.GramLanguage("test")]
			{{attributes}}
			class Parser;
			""";
		var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
		var compilation = CSharpCompilation.Create(
			"Host",
			[tree],
			[MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]);

		return Assert.Single(DslLanguageDiscovery.Discover(compilation).Languages);
	}
}
