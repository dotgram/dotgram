using System;
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
	public async Task ClassifiesCustomAttributeStringByRuleAndCaptureRoles()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(Source("let total"));
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
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
	}

	[Fact]
	public async Task ReportsRecognitionFailureInsideCustomAttributeString()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var document = Document(Source("let 123"));
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Classifications);
		var diagnostic = Assert.Single(result.Diagnostics);
		Assert.Equal("GRAM5101", diagnostic.Diagnostic.Id);
		Assert.Contains("dotgram.test.filter", diagnostic.Diagnostic.Message);
	}

	[Fact]
	public async Task IgnoresOrdinaryStringAttributes()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var source = Source("let total") + """

			[System.Obsolete("let total")]
			sealed class Ordinary;
			""";
		var document = Document(source);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Equal(2, result.Classifications.Count);
	}

	static Document Document(string source)
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
			]));

		return workspace.AddDocument(project.Id, "DslHost.cs", SourceText.From(source));
	}

	static string Source(string value) => SupportEmitter.Attributes + $$""""

		[DotGram.Gram("""
			Keyword    = "let"
			Identifier = ['a'..'z']+
			Start      = Keyword & ' ' & name: (Identifier)
			parse Start
			""")]
		[DotGram.GramLanguage("dotgram.test.filter")]
		[DotGram.GramClassify("Keyword", DotGram.GramClassification.Keyword)]
		[DotGram.GramClassify("Start.name", DotGram.GramClassification.Variable)]
		static class FilterParser;

		[DotGram.GramEmbeddedLanguage(typeof(FilterParser))]
		sealed class FilterAttribute : System.Attribute
		{
			public FilterAttribute(string source) { }
		}

		[Filter("{{value}}")] 
		sealed class Example;
		"""";
}
