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

public sealed class DslClassificationDiagnosticsTests
{
	[Fact]
	public async Task MapsInvalidTargetsToTheirExactStringContents()
	{
		var source = SupportEmitter.Attributes + """

			[DotGram.Gram("Start = name: Identifier\nIdentifier = ['a'..'z']+")]
			[DotGram.GramLanguage("test")]
			[DotGram.GramClassify("Missing", DotGram.GramClassification.Keyword)]
			[DotGram.GramClassify("Start.missing", DotGram.GramClassification.Variable)]
			class Parser;
			""";

		var diagnostics = await AnalyzeAsync(source);

		Assert.Equal(new[] { "GRAM5002", "GRAM5004" },
			diagnostics.Select(static diagnostic => diagnostic.Diagnostic.Id));
		Assert.Equal(
			new[] { "Missing", "Start.missing" },
			diagnostics.Select(diagnostic => source.Substring(diagnostic.Span.Start, diagnostic.Span.Length)));
	}

	[Fact]
	public async Task ReportsFileGrammarTargetsInTheHostDocument()
	{
		var source = SupportEmitter.Attributes + """

			[DotGram.Gram("Syntax/Filter.gram")]
			[DotGram.GramLanguage("test")]
			[DotGram.GramClassify("Start.unknown", DotGram.GramClassification.Variable)]
			class Parser;
			""";
		var diagnostics = await AnalyzeAsync(
			source,
			("Filter.gram", "Start = name: Identifier\nIdentifier = ['a'..'z']+", @"P:\Dsl\Syntax\Filter.gram"));

		var diagnostic = Assert.Single(diagnostics);
		Assert.Equal("GRAM5004", diagnostic.Diagnostic.Id);
		Assert.Equal("Start.unknown", source.Substring(diagnostic.Span.Start, diagnostic.Span.Length));
	}

	[Fact]
	public async Task IgnoresValidTargetsAndAttributesInOtherDocuments()
	{
		var source = SupportEmitter.Attributes + """

			[DotGram.Gram("Start = name: Identifier\nIdentifier = ['a'..'z']+")]
			[DotGram.GramLanguage("test")]
			[DotGram.GramClassify("Identifier", DotGram.GramClassification.Identifier)]
			[DotGram.GramClassify("Start.name", DotGram.GramClassification.Variable)]
			class Parser;
			""";

		Assert.Empty(await AnalyzeAsync(source));
	}

	static async Task<System.Collections.Generic.IReadOnlyList<HostDiagnostic>> AnalyzeAsync(
		string source,
		params (string Name, string Text, string Path)[] additionalDocuments)
	{
		using var workspace = new AdhocWorkspace();
		var project = workspace.AddProject(ProjectInfo.Create(
			ProjectId.CreateNewId(),
			VersionStamp.Default,
			"Dsl",
			"Dsl",
			LanguageNames.CSharp,
			parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
			metadataReferences: [MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]));
		var document = workspace.AddDocument(project.Id, "Parser.cs", SourceText.From(source));
		var solution = document.Project.Solution;
		foreach (var additional in additionalDocuments)
			solution = solution.AddAdditionalDocument(
				DocumentId.CreateNewId(project.Id),
				additional.Name,
				SourceText.From(additional.Text),
				filePath: additional.Path);
		project = solution.GetProject(project.Id)!;
		document = project.GetDocument(document.Id)!;

		var root = (await document.GetSyntaxRootAsync())!;
		var compilation = (await document.Project.GetCompilationAsync())!;
		return await DslClassificationDiagnostics.AnalyzeAsync(document, root, compilation);
	}
}
