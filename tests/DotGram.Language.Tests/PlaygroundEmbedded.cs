using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language.Tests;

/// <summary>
/// The playground's C# document as the embedded-grammar session leaves it once Roslyn has a semantic
/// model, computed the way the recording computed it: the same shared functions, over a workspace
/// that holds the playground's C# beside its .gram files, the DotGram attributes declared as source.
/// The generator does not run, so the DSL literals, which need its publications, are not recognized.
/// </summary>
static class PlaygroundEmbedded
{
	public const string FileName = "VisualStudioToolingPlayground.cs";

	static readonly string[] Grammars = ["VisualStudioToolingPlayground.gram", "ToolingQuery.gram", "RecoveryPlayground.gram"];

	public static async Task<EmbeddedGrammarView> ViewAsync(string text, string root, CancellationToken cancellationToken)
	{
		using var workspace = new AdhocWorkspace();
		var project   = workspace.AddProject(ProjectInfo.Create(
			ProjectId.CreateNewId(),
			VersionStamp.Default,
			"Playground",
			"Playground",
			LanguageNames.CSharp,
			parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
			metadataReferences: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]));
		var document  = project.AddDocument(FileName, SourceText.From(text), filePath: @"P:\Playground\" + FileName);
		project = document.Project
			.AddDocument("DotGram.Attributes.g.cs", SourceText.From(GramCompiler.EmitMarkerAttributes().Text)).Project
			.AddDocument("Declarations.cs", Declarations).Project;

		foreach (var grammar in Grammars)
			project = project.AddAdditionalDocument(
				grammar, SourceText.From(File.ReadAllText(Path.Combine(root, grammar))), filePath: @"P:\Playground\" + grammar).Project;

		document = project.GetDocument(document.Id)!;

		var rootNode     = (await document.GetSyntaxRootAsync(cancellationToken))!;
		var model        = (await document.GetSemanticModelAsync(cancellationToken))!;
		var includedFile = await StandaloneGrammarInheritance.GramFilesAsync(document.Project, cancellationToken);
		var analyses     = EmbeddedGrammarService.Analyze(model, rootNode, cancellationToken, includedFile);
		var dsl          = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, rootNode, model, cancellationToken, new DslEmbeddedSiteCache());
		var diagnostics  = await DslClassificationDiagnostics.AnalyzeAsync(document, rootNode, model.Compilation, cancellationToken);

		return new EmbeddedGrammarView(
			true,
			[.. analyses.SelectMany(static item => item.Classifications)],
			[.. analyses.SelectMany(static item => item.Diagnostics).Concat(diagnostics).Concat(dsl.Diagnostics)],
			[.. analyses.SelectMany(static item => item.Symbols)],
			dsl.Classifications,
			dsl.Symbols,
			dsl.Sites,
			[.. analyses.SelectMany(static item => item.Braces)],
			[.. analyses.SelectMany(static item => item.FoldingRanges)],
			[.. analyses.SelectMany(static item => item.DocumentSymbols)],
			[.. analyses.SelectMany(static item => item.PublishedApis)]);
	}

	/// <summary>The positions the recording asked at: every classified span's start, the DSL's too.</summary>
	public static int[] Positions(EmbeddedGrammarView view)
	{
		return view.Classifications.Select(static item => item.Span.Start)
			.Concat(view.DslClassifications.Select(static item => item.Span.Start))
			.Concat(view.DslSites.Select(static item => item.Span.Start))
			.Distinct().OrderBy(static at => at).ToArray();
	}

	/// <summary>The recording's first line: how much of the document was recognized.</summary>
	public static string Heading(EmbeddedGrammarView view)
	{
		return $"# grammars {view.Classifications.Select(static item => item.GrammarSpan).Distinct().Count()}, dsl sites {view.DslSites.Count}, dsl symbols {view.DslSymbols.Count}\n";
	}

	const string Declarations = """
		namespace DotGram
		{
			public enum GramClassification { Keyword, Identifier, Type, Variable, Function, Method, Property, Number, String, Comment, Operator, Punctuation, Namespace, Parameter, Label }

			[System.AttributeUsage(System.AttributeTargets.Class)]
			sealed class GramLanguageAttribute(string id) : System.Attribute;

			[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
			sealed class GramClassifyAttribute(string target, GramClassification classification) : System.Attribute;
		}

		namespace System.Diagnostics.CodeAnalysis
		{
			[System.AttributeUsage(System.AttributeTargets.All)]
			sealed class StringSyntaxAttribute(string syntax) : System.Attribute;
		}
		""";
}
