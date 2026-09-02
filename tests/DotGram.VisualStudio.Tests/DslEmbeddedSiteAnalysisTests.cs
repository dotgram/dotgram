using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DotGram.Grammar.Emit;
using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class DslEmbeddedSiteAnalysisTests
{
	[Fact]
	public async Task ClassifiesStringSyntaxArgumentByRuleAndCaptureRoles()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(Source("let total"));
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		var site = Assert.Single(result.Sites);
		Assert.Equal("dotgram.test.filter", site.LanguageId);
		Assert.Equal("Start", site.EntryRule);
		Assert.Equal("let total", text.ToString(site.Span));
		Assert.Collection(
			result.Classifications.OrderBy(item => item.Span.Start),
			item =>
			{
				Assert.Equal("Keyword", item.Role);
				Assert.Equal("let", text.ToString(item.Span));
			},
			item =>
			{
				Assert.Equal("Variable", item.Role);
				Assert.Equal("total", text.ToString(item.Span));
			});
		Assert.Equal(
			new[]
			{
				("Keyword", "Keyword", "let"),
				("Variable", "Start.name", "total"),
			},
			result.Symbols.OrderBy(item => item.Span.Start)
				.Select(item => (item.Role, item.Target, text.ToString(item.Span))));
	}

	[Fact]
	public async Task ExposesDefinitionLocationsForFileBackedDslSymbols()
	{
		const string grammar =
			"trivia = ' '*\n" +
			"Keyword = \"let\"\n" +
			"Identifier = ['a'..'z']+\n" +
			"Start = Keyword & ' ' & name: Identifier\n" +
			"parse Start";
		const string path = @"P:\Dsl\Filter.gram";
		var source = Support + """

			[DotGram.Gram("Filter.gram")]
			[DotGram.GramLanguage("dotgram.test.filter")]
			[DotGram.GramClassify("Keyword", DotGram.GramClassification.Keyword)]
			[DotGram.GramClassify("Start.name", DotGram.GramClassification.Variable)]
			static class FilterParser
			{
				public static string ParseStart(string input) => input;
			}

			static class Example
			{
				static void Run([System.Diagnostics.CodeAnalysis.StringSyntax("dotgram.test.filter")] string text) { }
				static void Test() => Run("let customer");
			}
			""";
		var cancellationToken = TestContext.Current.CancellationToken;
		var original = Document(source);
		var project = original.Project.AddAdditionalDocument(
			"Filter.gram",
			SourceText.From(grammar),
			filePath: path).Project;
		var document = project.GetDocument(original.Id) ?? throw new InvalidOperationException();
		var root  = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		var grammarText = SourceText.From(grammar);
		var keyword = result.Symbols.Single(item => item.Target == "Keyword");
		var capture = result.Symbols.Single(item => item.Target == "Start.name");
		Assert.Equal(path, keyword.DefinitionPath);
		Assert.Equal(grammarText.Lines.GetLinePosition(grammar.IndexOf("Keyword", StringComparison.Ordinal)).Line, keyword.DefinitionLine);
		Assert.Equal(path, capture.DefinitionPath);
		var capturePosition = grammar.IndexOf("name:", StringComparison.Ordinal);
		var expectedCapture = grammarText.Lines.GetLinePosition(capturePosition);
		Assert.Equal((expectedCapture.Line, expectedCapture.Character), (capture.DefinitionLine, capture.DefinitionColumn));
	}

	[Fact]
	public async Task ExposesLiteralCompletionsAtTheRecognitionFailurePosition()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(Source(""));
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		var site = Assert.Single(result.Sites);
		Assert.Contains("\"let\"", site.Expected);
		Assert.Equal(text.ToString().IndexOf("new Query(\"\")", StringComparison.Ordinal) + "new Query(\"".Length, site.CompletionPosition);
	}

	[Fact]
	public async Task ReportsRecognitionFailureInsideStringSyntaxArgument()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(Source("let 123"));
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		var classification = Assert.Single(result.Classifications);
		Assert.Equal("Keyword", classification.Role);
		var diagnostic = Assert.Single(result.Diagnostics);
		Assert.Equal("GRAM5101", diagnostic.Diagnostic.Id);
		Assert.Equal(
			"Expected one of ' ', ['a'..'z'] in DotGram language 'dotgram.test.filter'.",
			diagnostic.Diagnostic.Message);
	}

	[Theory]
	[InlineData("let total1", "let", "total")]
	[InlineData("let total 1", "let", "total")]
	public async Task PreservesRecognizedClassificationsBeforeInvalidSuffix(
		string value,
		string keyword,
		string variable)
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(Source(value));
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Single(result.Diagnostics);
		Assert.Equal(
			new[] { ("Keyword", keyword), ("Variable", variable) },
			result.Classifications.OrderBy(item => item.Span.Start)
				.Select(item => (item.Role, text.ToString(item.Span))));
	}

	[Fact]
	public async Task ResolvesNamedMethodArgumentThroughItsStringSyntaxParameter()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var source = Source("let total").Replace(
			"static void Test() => Run(new Query(\"let total\"));",
			"static void Test() => Execute(text: \"let total\");\n\tstatic void Execute([System.Diagnostics.CodeAnalysis.StringSyntax(\"dotgram.test.filter\")] string text) { }");
		var document = Document(source);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		Assert.Equal(
			new[] { ("Keyword", "let"), ("Variable", "total") },
			result.Classifications.OrderBy(item => item.Span.Start)
				.Select(item => (item.Role, text.ToString(item.Span))));
	}

	[Fact]
	public async Task ClassifiesLiteralReceiverOfReducedExtensionMethod()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var source = Source("let total").Replace(
			"static void Test() => Run(new Query(\"let total\"));",
			"static void Test() => \"let total\".AsFilter();\n" +
			"\tstatic string AsFilter([System.Diagnostics.CodeAnalysis.StringSyntax(\"dotgram.test.filter\")] this string text) => text;");
		var document = Document(source);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		Assert.Equal("let total", text.ToString(Assert.Single(result.Sites).Span));
		Assert.Equal(
			new[] { ("Keyword", "let"), ("Variable", "total") },
			result.Classifications.OrderBy(item => item.Span.Start)
				.Select(item => (item.Role, text.ToString(item.Span))));
	}

	[Fact]
	public async Task ClassifiesOnlyDirectFieldAndPropertyInitializers()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var source = Source("unused").Replace(
			"static void Test() => Run(new Query(\"unused\"));",
			"[System.Diagnostics.CodeAnalysis.StringSyntax(\"dotgram.test.filter\")]\n" +
			"\tstatic string Field = \"let field\";\n" +
			"\t[System.Diagnostics.CodeAnalysis.StringSyntax(\"dotgram.test.filter\")]\n" +
			"\tstatic string Property { get; } = \"let property\";\n" +
			"\tstatic void Test() => Field = \"let assignment\";");
		var document = Document(source);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		Assert.Equal(
			new[] { "let field", "let property" },
			result.Sites.OrderBy(item => item.Span.Start).Select(item => text.ToString(item.Span)));
		Assert.DoesNotContain(result.Sites, item => text.ToString(item.Span) == "let assignment");
	}

	[Theory]
	[InlineData("ParseStart")]
	[InlineData("TryParseStart")]
	public async Task RoutesGeneratedPublicationMethodInputWithoutMarkerAttribute(string method)
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var source = Source("let total").Replace(
			"static void Test() => Run(new Query(\"let total\"));",
			$"static void Test() => FilterParser.{method}(\"let total\");");
		var document = Document(source);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		Assert.Equal(
			new[] { ("Keyword", "let"), ("Variable", "total") },
			result.Classifications.OrderBy(item => item.Span.Start)
				.Select(item => (item.Role, text.ToString(item.Span))));
	}

	[Fact]
	public async Task SelectsPublicationFromGeneratedMethodWhenLanguageHasSeveralEntries()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var source = Source("let total")
			.Replace(
				"parse Start",
				"Other      = \"show\" & ' ' & name: (Identifier)\n\t\tparse Start\n\t\tparse Other")
			.Replace(
				"[DotGram.GramClassify(\"Start.name\", DotGram.GramClassification.Variable)]",
				"[DotGram.GramClassify(\"Start.name\", DotGram.GramClassification.Variable)]\n\t[DotGram.GramClassify(\"Other.name\", DotGram.GramClassification.Variable)]")
			.Replace(
				"static void Test() => Run(new Query(\"let total\"));",
				"static void Test() => FilterParser.ParseOther(\"show total\");");
		var document = Document(source);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		Assert.Equal("Other", Assert.Single(result.Sites).EntryRule);
		Assert.Contains(result.Classifications, item =>
			item.Role == "Variable" && text.ToString(item.Span) == "total");
	}

	[Fact]
	public async Task RoutesInputThroughAParserWithAnInheritedGrammar()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var source = Support + """

			[DotGram.Gram("Word = ['a'..'z']+", IncludedAs = "Lexical")]
			class LexicalParser;

			[DotGram.Gram("using Lexical;\nStart = name: Word\nparse Start")]
			[DotGram.GramLanguage("derived")]
			[DotGram.GramClassify("Start.name", DotGram.GramClassification.Variable)]
			class DerivedParser : LexicalParser
			{
				public static string ParseStart(string input) => input;
			}

			static class Example
			{
				static void Test() => DerivedParser.ParseStart("customer");
			}
			""";
		var document = Document(source);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		var classification = Assert.Single(result.Classifications);
		Assert.Equal("Variable", classification.Role);
		Assert.Equal("customer", text.ToString(classification.Span));
	}

	[Theory]
	[InlineData("ParseStart")]
	[InlineData("TryParseStart")]
	public async Task UsesDescriptorFromReferencedAssemblyForGeneratedApi(string method)
	{
		const string grammar =
			"trivia = ' '*\n" +
			"Keyword = \"let\"\n" +
			"Identifier = ['a'..'z']+\n" +
			"Start = Keyword & ' ' & name: Identifier\n" +
			"parse Start as ParseStart";
		var sourcePayload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(grammar));
		var entriesPayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes("ParseStart\tParse\tStart"));
		var classificationsPayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes("Keyword\tKeyword\nStart.name\tVariable"));
		var reference = Reference(Support + $$"""

			[DotGram.GramLanguage("package.filter")]
			[DotGram.GramLanguageDescriptor(2, "package.filter", "{{Hash(grammar)}}", "{{sourcePayload}}", "{{entriesPayload}}", "{{classificationsPayload}}")]
			public class PackagedParser
			{
				public static string ParseStart(string input) => input;
				public static string TryParseStart(string input) => input;
			}
			""");
		var source = $$"""
			class Consumer
			{
				static void Test() => PackagedParser.{{method}}("let customer");
			}
			""";
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(source, reference);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		Assert.Equal("Start", Assert.Single(result.Sites).EntryRule);
		Assert.Equal(
			new[] { ("Keyword", "let"), ("Variable", "customer") },
			result.Classifications.OrderBy(item => item.Span.Start)
				.Select(item => (item.Role, text.ToString(item.Span))));
	}

	[Theory]
	[InlineData("static void Test() => new PackagedQuery(\"let customer\");")]
	[InlineData("static void Test() => Execute(\"let customer\"); static void Execute([System.Diagnostics.CodeAnalysis.StringSyntax(\"package.filter\")] string text) { }")]
	public async Task UsesReferencedLanguageThroughStringSyntax(string consumerMember)
	{
		const string grammar =
			"trivia = ' '*\n" +
			"Keyword = \"let\"\n" +
			"Identifier = ['a'..'z']+\n" +
			"Start = Keyword & ' ' & name: Identifier\n" +
			"parse Start";
		var sourcePayload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(grammar));
		var entriesPayload = Convert.ToBase64String(
			System.Text.Encoding.UTF8.GetBytes("ParseStart\tParse\tStart"));
		var reference = Reference(Support + $$"""

			[DotGram.GramLanguage("package.filter")]
			[DotGram.GramClassify("Keyword", DotGram.GramClassification.Keyword)]
			[DotGram.GramClassify("Start.name", DotGram.GramClassification.Variable)]
			[DotGram.GramLanguageDescriptor(1, "package.filter", "{{Hash(grammar)}}", "{{sourcePayload}}", "{{entriesPayload}}")]
			public class PackagedParser;

			public sealed class PackagedQuery
			{
				public PackagedQuery([System.Diagnostics.CodeAnalysis.StringSyntax("package.filter")] string text) { }
			}
			""");
		var source = $$"""
			class Consumer
			{
				{{consumerMember}}
			}
			""";
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(source, reference);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		Assert.Equal("Start", Assert.Single(result.Sites).EntryRule);
		Assert.Equal(
			new[] { ("Keyword", "let"), ("Variable", "customer") },
			result.Classifications.OrderBy(item => item.Span.Start)
				.Select(item => (item.Role, text.ToString(item.Span))));
	}

	[Fact]
	public async Task IgnoresArgumentsForUnmarkedStringParameters()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var source = Source("let total") + """

			static class OrdinaryExample
			{
				static void Ordinary(string text) { }
				static void Test() => Ordinary("let total");
			}
			""";
		var document = Document(source);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Equal(2, result.Classifications.Count);
	}

	[Fact]
	public async Task ReusesPreparedGrammarUntilLanguageDeclarationChanges()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(Source("let total"));
		var compilation = await document.Project.GetCompilationAsync(cancellationToken) ??
			throw new InvalidOperationException();
		var language = Assert.Single(DslLanguageDiscovery.Discover(compilation, cancellationToken).Languages);
		var cache = new DslEmbeddedSiteCache();

		var first  = cache.Prepare(language, language.GrammarSource);
		var second = cache.Prepare(language, language.GrammarSource);
		var changed = cache.Prepare(language, language.GrammarSource + "\n");

		Assert.NotNull(first);
		Assert.Same(first, second);
		Assert.NotSame(first, changed);
	}

	static Document Document(string source, params MetadataReference[] references)
	{
		var workspace = new AdhocWorkspace();
		var project = workspace.AddProject(ProjectInfo.Create(
			ProjectId.CreateNewId(),
			VersionStamp.Default,
			"DslHost",
			"DslHost",
			LanguageNames.CSharp,
			parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
			compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
			metadataReferences:
			[
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				.. references,
			]));

		return workspace.AddDocument(project.Id, "DslHost.cs", SourceText.From(source));
	}

	static PortableExecutableReference Reference(string source)
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var compilation = CSharpCompilation.Create(
			"DslPackage",
			[CSharpSyntaxTree.ParseText(
				source,
				CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
				cancellationToken: cancellationToken)],
			[
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute).Assembly.Location),
			],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		using var stream = new MemoryStream();
		var emitted = compilation.Emit(stream, cancellationToken: cancellationToken);
		Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics));

		return MetadataReference.CreateFromImage(stream.ToArray());
	}

	static string Hash(string value)
	{
		using var sha = System.Security.Cryptography.SHA256.Create();
		return string.Concat(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value))
			.Select(static item => item.ToString("x2")));
	}

	static string Support => SupportEmitter.Attributes + """

		namespace System.Diagnostics.CodeAnalysis
		{
			[System.AttributeUsage(
				System.AttributeTargets.Parameter |
				System.AttributeTargets.Field |
				System.AttributeTargets.Property)]
			public sealed class StringSyntaxAttribute : System.Attribute
			{
				public StringSyntaxAttribute(string syntax) => Syntax = syntax;
				public string Syntax { get; }
			}
		}
		""";

	static string Source(string value) => Support + $$""""

		[DotGram.Gram("""
			trivia     = [' ' | '\t']*
			Keyword    = "let"
			Identifier = ['a'..'z']+
			Start      = Keyword & ' ' & name: (Identifier)
			parse Start
			""")]
		[DotGram.GramLanguage("dotgram.test.filter")]
		[DotGram.GramClassify("Keyword", DotGram.GramClassification.Keyword)]
		[DotGram.GramClassify("Start.name", DotGram.GramClassification.Variable)]
		static class FilterParser
		{
			public static string ParseStart(string input) => input;
			public static string TryParseStart(string input) => input;
			public static string ParseOther(string input) => input;
		}

		sealed class Query
		{
			public Query([System.Diagnostics.CodeAnalysis.StringSyntax("dotgram.test.filter")] string text) { }
		}

		static class Example
		{
			static void Test() => Run(new Query("{{value}}"));
			static void Run(Query query) { }
			static void Ordinary(string text) { }
		}
		"""";
}
