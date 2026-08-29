using System.Threading.Tasks;

using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class DslGrammarSourceResolverTests
{
	[Fact]
	public async Task ReturnsEmbeddedGrammarWithoutAProjectDocument()
	{
		var project  = Project();
		var language = Language(DslGrammarSourceKind.Embedded, "Start = 'x'");

		var resolution = await DslGrammarSourceResolver.ResolveAsync(
			project,
			language,
			TestContext.Current.CancellationToken);

		Assert.Equal(DslGrammarResolutionKind.Resolved, resolution.Kind);
		Assert.Equal("Start = 'x'", resolution.Text);
		Assert.Null(resolution.Document);
		Assert.Empty(resolution.Candidates);
	}

	[Fact]
	public async Task ResolvesAdditionalDocumentWithGeneratorPathSemantics()
	{
		var project = Project()
			.AddAdditionalDocument("Filter.gram", SourceText.From("Start = 'x'"), filePath: @"P:\Dsl\Syntax\Filter.gram").Project
			.AddAdditionalDocument("NotFilter.gram", SourceText.From("Wrong = 'x'"), filePath: @"P:\Dsl\NotFilter.gram").Project;

		var resolution = await DslGrammarSourceResolver.ResolveAsync(
			project,
			Language(DslGrammarSourceKind.File, "Syntax/Filter.gram"),
			TestContext.Current.CancellationToken);

		Assert.Equal(DslGrammarResolutionKind.Resolved, resolution.Kind);
		Assert.Equal("Start = 'x'", resolution.Text);
		Assert.Equal(@"P:\Dsl\Syntax\Filter.gram", resolution.Document!.FilePath);
		Assert.Single(resolution.Candidates);
	}

	[Fact]
	public async Task DoesNotMatchAFileNameSuffixWithoutASeparatorBoundary()
	{
		var project = Project()
			.AddAdditionalDocument("MyFilter.gram", SourceText.From("Wrong = 'x'"), filePath: @"P:\Dsl\MyFilter.gram").Project;

		var resolution = await DslGrammarSourceResolver.ResolveAsync(
			project,
			Language(DslGrammarSourceKind.File, "Filter.gram"),
			TestContext.Current.CancellationToken);

		Assert.Equal(DslGrammarResolutionKind.Missing, resolution.Kind);
		Assert.Null(resolution.Text);
		Assert.Null(resolution.Document);
	}

	[Fact]
	public async Task ReportsEveryAmbiguousCandidateWithoutSelectingOne()
	{
		var project = Project()
			.AddAdditionalDocument("Filter.gram", SourceText.From("One = '1'"), filePath: @"P:\One\Filter.gram").Project
			.AddAdditionalDocument("Filter.gram", SourceText.From("Two = '2'"), filePath: @"P:\Two\Filter.gram").Project;

		var resolution = await DslGrammarSourceResolver.ResolveAsync(
			project,
			Language(DslGrammarSourceKind.File, "Filter.gram"),
			TestContext.Current.CancellationToken);

		Assert.Equal(DslGrammarResolutionKind.Ambiguous, resolution.Kind);
		Assert.Null(resolution.Text);
		Assert.Null(resolution.Document);
		Assert.Equal(2, resolution.Candidates.Count);
	}

	[Fact]
	public async Task JoinsInheritedGrammarUnderItsIncludedName()
	{
		var project = Project();
		var language = new DslLanguageDefinition(
			"derived",
			null!,
			DslGrammarSourceKind.Embedded,
			"using Lexical;\nStart = Word\nparse Start",
			[],
			[],
			[new DslIncludedGrammarDefinition(
				"Lexical",
				DslGrammarSourceKind.Embedded,
				"Word = ['a'..'z']+")]);

		var resolution = await DslGrammarSourceResolver.ResolveAsync(
			project,
			language,
			TestContext.Current.CancellationToken);

		Assert.Equal(DslGrammarResolutionKind.Resolved, resolution.Kind);
		Assert.Equal(
			"using Lexical;\nStart = Word\nparse Start\n\n" +
			"namespace Lexical\n{\nWord = ['a'..'z']+\n}\n",
			resolution.Text);
	}

	static Project Project() =>
		new AdhocWorkspace().AddProject("Dsl", LanguageNames.CSharp);

	static DslLanguageDefinition Language(DslGrammarSourceKind kind, string source) =>
		new(
			"test",
			null!,
			kind,
			source,
			[],
			[]);
}
