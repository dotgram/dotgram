using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// The editor reads a chain of calls ten thousand long on a stack of one megabyte: a
/// <c>.gram</c> document, and a grammar in the C# document of its host (<see cref="DeepChains"/>).
/// </summary>
public sealed class DeepChainAnalysisTests
{
	/// <summary>A standalone document: binding, classification, symbols, diagnostics.</summary>
	[Theory]
	[MemberData(nameof(DeepChains.Names), MemberType = typeof(DeepChains))]
	public void A_long_chain_of_calls_is_analyzed_on_a_small_stack(string shape)
	{
		var grammar  = DeepChains.Shapes[shape](DeepChains.Depth);
		var document = DeepChains.OnSmallStack(() => GramLanguageService.Analyze(grammar));

		Assert.DoesNotContain(document.Diagnostics, static d => d.Severity == GramSeverity.Error);
		Assert.NotEmpty(document.Symbols);
	}

	/// <summary>The same grammar in a <c>[Gram]</c> attribute, read the way the editor reads its host.</summary>
	[Fact]
	public void A_long_chain_of_calls_in_a_host_class_is_analyzed_on_a_small_stack()
	{
		var grammar = DeepChains.Shapes["guard"](DeepChains.Depth);
		var source  =
			"namespace DotGram { sealed class GramAttribute(string text) : System.Attribute; }\n\n" +
			"[DotGram.Gram(\"\"\"\n" + grammar + "\"\"\")]\npartial class Deep;\n";

		var tree        = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
		var compilation = CSharpCompilation.Create(
			"Host", [tree], [MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]);

		var analyses = DeepChains.OnSmallStack(() =>
			EmbeddedGrammarService.Analyze(compilation.GetSemanticModel(tree), tree.GetRoot()).ToArray());

		var analysis = Assert.Single(analyses);

		Assert.DoesNotContain(analysis.Diagnostics, static d => d.Diagnostic.Severity == GramSeverity.Error);
	}
}
