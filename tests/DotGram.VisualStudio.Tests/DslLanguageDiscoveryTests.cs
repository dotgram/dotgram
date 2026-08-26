using System;
using System.Linq;

using DotGram.Grammar.Emit;
using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class DslLanguageDiscoveryTests
{
	[Fact]
	public void DiscoversLanguageClassificationsAndAttributeCarrierByShape()
	{
		var catalog = Discover(Support + """

			namespace Host
			{
				using DG = DotGram;

				[DG.Gram("Start = Keyword & name: Identifier")]
				[DG.GramLanguage("com.example.filter", Extensions = new[] { ".filter", ".query" })]
				[DG.GramClassify("Keyword", DG.GramClassification.Keyword)]
				[DG.GramClassify("Start.name", DG.GramClassification.Variable)]
				[DG.GramLanguageMarker(typeof(FilterAttribute))]
				public static class FilterParser;

				public sealed class FilterAttribute(string source) : System.Attribute;
			}
			""");

		var language = Assert.Single(catalog.Languages);
		Assert.Equal("com.example.filter", language.Id);
		Assert.Equal("FilterParser", language.ParserType.Name);
		Assert.Equal(DslGrammarSourceKind.Embedded, language.SourceKind);
		Assert.Equal("Start = Keyword & name: Identifier", language.GrammarSource);
		Assert.Equal(new[] { ".filter", ".query" }, language.Extensions);
		Assert.Equal(
			new[] { ("Keyword", "Keyword"), ("Start.name", "Variable") },
			language.Classifications.Select(item => (item.Target, item.Role)));

		var carrier = Assert.Single(catalog.AttributeCarriers);
		Assert.Equal("FilterAttribute", carrier.AttributeType.Name);
		Assert.Same(language, carrier.Language);
	}

	[Fact]
	public void ResolvesDefaultAndExplicitGrammarFiles()
	{
		var catalog = Discover(Support + """

			[DotGram.Gram]
			[DotGram.GramLanguage("default")]
			class DefaultParser;

			[DotGram.Gram("Syntax/Explicit.gram")]
			[DotGram.GramLanguage("explicit")]
			class ExplicitParser;
			""");

		Assert.Collection(
			catalog.Languages.OrderBy(language => language.Id),
			language =>
			{
				Assert.Equal("default", language.Id);
				Assert.Equal(DslGrammarSourceKind.File, language.SourceKind);
				Assert.Equal("DefaultParser.gram", language.GrammarSource);
			},
			language =>
			{
				Assert.Equal("explicit", language.Id);
				Assert.Equal(DslGrammarSourceKind.File, language.SourceKind);
				Assert.Equal("Syntax/Explicit.gram", language.GrammarSource);
			});
	}

	[Fact]
	public void IgnoresSameNamedAttributesWithWrongShapes()
	{
		var catalog = Discover("""
			using System;

			namespace DotGram
			{
				sealed class GramAttribute : Attribute
				{
					public string Source => "";
				}

				sealed class GramLanguageAttribute(string id) : Attribute
				{
					public int Id => id.Length;
					public string Extensions { get; set; } = "";
				}
			}

			[DotGram.Gram]
			[DotGram.GramLanguage("not-a-language")]
			class Parser;
			""");

		Assert.Empty(catalog.Languages);
		Assert.Empty(catalog.AttributeCarriers);
	}

	[Fact]
	public void IgnoresMarkerTypeThatIsNotAnAttribute()
	{
		var catalog = Discover(Support + """

			class NotAnAttribute;

			[DotGram.Gram("Start = 'x'")]
			[DotGram.GramLanguage("filter")]
			[DotGram.GramLanguageMarker(typeof(NotAnAttribute))]
			sealed class FilterParser;
			""");

		Assert.Single(catalog.Languages);
		Assert.Empty(catalog.AttributeCarriers);
	}

	[Fact]
	public void FindsNestedParserAndCarrierTypes()
	{
		var catalog = Discover(Support + """

			class Container
			{
				[DotGram.Gram("Start = 'x'")]
				[DotGram.GramLanguage("nested")]
				[DotGram.GramLanguageMarker(typeof(SyntaxAttribute))]
				public class Parser;

				public sealed class SyntaxAttribute(string source) : System.Attribute;
			}
			""");

		Assert.Equal("Parser", Assert.Single(catalog.Languages).ParserType.Name);
		Assert.Equal("SyntaxAttribute", Assert.Single(catalog.AttributeCarriers).AttributeType.Name);
	}

	static DslLanguageCatalog Discover(string source)
	{
		var tree = CSharpSyntaxTree.ParseText(
			source,
			CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
		var compilation = CSharpCompilation.Create(
			"Host",
			[tree],
			[MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]);

		return DslLanguageDiscovery.Discover(compilation);
	}

	static string Support => SupportEmitter.Attributes;
}
