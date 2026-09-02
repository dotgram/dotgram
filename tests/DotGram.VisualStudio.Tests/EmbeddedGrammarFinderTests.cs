using System;
using System.Linq;

using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class EmbeddedGrammarFinderTests
{
	[Fact]
	public void FindsQualifiedAndAliasedGramAttributesBySymbol()
	{
		var source = """
			using DG = DotGram;

			namespace DotGram
			{
				[System.AttributeUsage(System.AttributeTargets.Class)]
				sealed class GramAttribute(string text) : System.Attribute;
			}

			class GramAttribute(string text) : System.Attribute;

			[DotGram.Gram("A = 'a'")]
			class Qualified;

			[DG.Gram("B = 'b'")]
			class Aliased;

			[Gram("Not = 'it'")]
			class Unrelated;
			""";

		var grammars = Find(source);

		Assert.Equal(new[] { "A = 'a'", "B = 'b'" }, grammars.Select(grammar => grammar.Text));
	}

	[Fact]
	public void ReturnsMapWithHostDocumentPositions()
	{
		var source = """
			namespace DotGram
			{
				sealed class GramAttribute(string text) : System.Attribute;
			}

			[DotGram.Gram("Start = \"a\"")]
			class Parser;
			""";

		var grammar = Assert.Single(Find(source));
		var at      = grammar.Text.IndexOf("\"a\"", StringComparison.Ordinal);

		Assert.True(grammar.SourceMap.TryMap(at, 3, out var span));
		Assert.Equal("\\\"a\\\"", source.Substring(span.Start, span.Length));
	}

	[Fact]
	public void FindsSourceSpelledGramAttributeWithoutSemanticModel()
	{
		var source = """"
			[DotGram.Gram("""
				Start = "select"i
				""")]
			class Parser;
			"""";
		var cancellationToken = TestContext.Current.CancellationToken;
		var root = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken)
			.GetRoot(cancellationToken);

		var grammar = Assert.Single(EmbeddedGrammarFinder.FindSyntactic(root, cancellationToken));

		Assert.Contains("Start = \"select\"i", grammar.Text, StringComparison.Ordinal);
	}

	[Fact]
	public void AcceptsNamedArgumentsAndSplicesEmbeddedBaseGrammars()
	{
		var source = """
			namespace DotGram
			{
				sealed class GramAttribute(string text) : System.Attribute
				{
					public string IncludedAs { get; set; } = "";
				}
			}

			[DotGram.Gram("Word = ['a'..'z']+", IncludedAs = "Lexical")]
			class Base;

			[DotGram.Gram("using Lexical;\nStart = Word\nparse Start")]
			class Parser : Base;
			""";

		var grammars = Find(source);
		var derived = Assert.Single(
			grammars,
			grammar => grammar.Text.StartsWith("using", StringComparison.Ordinal));

		Assert.StartsWith(derived.Text, derived.AnalysisText, StringComparison.Ordinal);
		Assert.Contains("namespace Lexical\n{\nWord = ['a'..'z']+", derived.AnalysisText, StringComparison.Ordinal);
	}

	static EmbeddedGrammar[] Find(string source)
	{
		var tree = CSharpSyntaxTree.ParseText(source);
		var compilation = CSharpCompilation.Create(
			"Host",
			[tree],
			[MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]);
		var model = compilation.GetSemanticModel(tree);

		return EmbeddedGrammarFinder.Find(model, tree.GetRoot()).ToArray();
	}
}
