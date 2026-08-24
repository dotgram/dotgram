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
