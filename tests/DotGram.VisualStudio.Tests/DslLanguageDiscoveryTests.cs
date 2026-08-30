using System;
using System.IO;
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

	[Fact]
	public void DiscoversInheritedGrammarSourcesAndIncludedNames()
	{
		var catalog = Discover(Support + """

			[DotGram.Gram("Word = ['a'..'z']+", IncludedAs = "Lexical")]
			class BaseParser;

			[DotGram.Gram("using Lexical;\nStart = Word\nparse Start")]
			[DotGram.GramLanguage("derived")]
			class DerivedParser : BaseParser;
			""");

		var language = Assert.Single(catalog.Languages);
		var included = Assert.Single(language.IncludedGrammars);
		Assert.Equal("Lexical", included.Name);
		Assert.Equal(DslGrammarSourceKind.Embedded, included.SourceKind);
		Assert.Equal("Word = ['a'..'z']+", included.GrammarSource);
	}

	[Fact]
	public void ReadsVersionedGeneratedDescriptorWithoutTheOriginalGramAttribute()
	{
		const string grammar = "Start = 'x'\nparse Start as Read";
		var sourcePayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes(grammar));
		var entriesPayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes("Read\tParse\tStart"));
		var catalog = Discover(Support + $$"""

			[DotGram.GramLanguage("referenced")]
			[DotGram.GramLanguageDescriptor(1, "referenced", "{{Hash(grammar)}}", "{{sourcePayload}}", "{{entriesPayload}}")]
			partial class Parser;
			""");

		var language = Assert.Single(catalog.Languages);
		Assert.Equal(DslGrammarSourceKind.Embedded, language.SourceKind);
		Assert.Equal("Start = 'x'\nparse Start as Read", language.GrammarSource);
		Assert.Equal(1, language.DescriptorFormatVersion);
		Assert.Equal(Hash(grammar), language.GrammarHash);
		Assert.Equal("Start", language.Entries["Read"]);
	}

	[Fact]
	public void ReadsClassificationsFromVersionTwoDescriptor()
	{
		const string grammar = "Keyword = \"let\"\nStart = name: Keyword\nparse Start as Read";
		var sourcePayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes(grammar));
		var entriesPayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes("Read\tParse\tStart"));
		var classificationsPayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes("Keyword\tKeyword\nStart.name\tVariable"));
		var catalog = Discover(Support + $$"""

			[DotGram.GramLanguage("referenced")]
			[DotGram.GramLanguageDescriptor(2, "referenced", "{{Hash(grammar)}}", "{{sourcePayload}}", "{{entriesPayload}}", "{{classificationsPayload}}")]
			partial class Parser;
			""");

		var language = Assert.Single(catalog.Languages);
		Assert.Equal(2, language.DescriptorFormatVersion);
		Assert.Equal(
			new[] { ("Keyword", "Keyword"), ("Start.name", "Variable") },
			language.Classifications.Select(item => (item.Target, item.Role)));
		Assert.All(language.Classifications, item => Assert.Null(item.Attribute));
	}

	[Fact]
	public void DiscoversDescriptorFromReferencedAssemblyWithoutLoadingIt()
	{
		const string grammar = "Start = 'x'\nparse Start as Read";
		var sourcePayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes(grammar));
		var entriesPayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes("Read\tParse\tStart"));
		var reference = Reference(Support + $$"""

			[DotGram.GramLanguage("package.language")]
			[DotGram.GramLanguageDescriptor(1, "package.language", "{{Hash(grammar)}}", "{{sourcePayload}}", "{{entriesPayload}}")]
			public class PackagedParser;
			""");
		var cancellationToken = TestContext.Current.CancellationToken;
		var compilation = CSharpCompilation.Create(
			"Consumer",
			[CSharpSyntaxTree.ParseText("class Consumer;", cancellationToken: cancellationToken)],
			[MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location), reference]);
		var package = Assert.IsAssignableFrom<IAssemblySymbol>(compilation.GetAssemblyOrModuleSymbol(reference));
		var parser = package.GlobalNamespace.GetTypeMembers("PackagedParser").Single();
		Assert.Contains(parser.GetAttributes(), attribute =>
			attribute.AttributeClass?.Name == "GramLanguageDescriptorAttribute");

		var language = Assert.Single(DslLanguageDiscovery.Discover(compilation, cancellationToken).Languages);

		Assert.Equal("package.language", language.Id);
		Assert.Equal("PackagedParser", language.ParserType.Name);
		Assert.Equal("Start = 'x'\nparse Start as Read", language.GrammarSource);
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

	static PortableExecutableReference Reference(string source)
	{
		var tree = CSharpSyntaxTree.ParseText(
			source,
			CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
		var compilation = CSharpCompilation.Create(
			"Package",
			[tree],
			[
				MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute).Assembly.Location),
			],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		using var stream = new MemoryStream();
		var emitted = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
		Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics));

		return MetadataReference.CreateFromImage(stream.ToArray());
	}

	static string Hash(string value)
	{
		using var sha = System.Security.Cryptography.SHA256.Create();
		return string.Concat(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value))
			.Select(static item => item.ToString("x2")));
	}

	[Fact]
	public void ReusesCatalogForTheSameCompilation()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var tree = CSharpSyntaxTree.ParseText(
			Support,
			CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
			cancellationToken: cancellationToken);
		var compilation = CSharpCompilation.Create(
			"Host",
			[tree],
			[MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]);

		Assert.Same(
			DslLanguageDiscovery.Discover(compilation, cancellationToken),
			DslLanguageDiscovery.Discover(compilation, cancellationToken));
	}

	static string Support => SupportEmitter.Attributes;
}
