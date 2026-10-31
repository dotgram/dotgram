using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// Navigation in a C# document asks Roslyn first and the document's analysis after: an analysis
/// published while Roslyn was being asked is the one navigated by.
/// </summary>
public sealed class EmbeddedNavigationTests
{
	const string Source = """
		namespace DotGram
		{
			sealed class GramAttribute(string text) : System.Attribute;
		}

		[DotGram.Gram("Start = Word\nWord = 'a'\nparse Start")]
		class Parser;
		""";

	[Fact]
	public async Task Ctrl_click_reads_the_analysis_after_Roslyn_answered()
	{
		var (roslyn, view) = Arrange(out var asked);
		var position       = Source.IndexOf("Word", StringComparison.Ordinal);

		var navigable = await GramNavigationProvider.NavigableAsync(
			Source, () => asked() ? view : EmbeddedGrammarView.None, position, roslyn, TestContext.Current.CancellationToken);

		var target = Assert.IsType<GramLocation.InDocument>(navigable?.Target);
		Assert.Equal(Source.IndexOf("Word =", StringComparison.Ordinal), target.Position);
	}

	[Fact]
	public async Task Go_To_Definition_reads_the_analysis_after_Roslyn_answered()
	{
		var (roslyn, view) = Arrange(out var asked);
		var position       = Source.IndexOf("Word", StringComparison.Ordinal);

		var definition = await GramNavigationProvider.DefinitionAsync(
			Source, () => asked() ? view : EmbeddedGrammarView.None, position, roslyn, TestContext.Current.CancellationToken);

		var target = Assert.IsType<GramLocation.InDocument>(definition);
		Assert.Equal(Source.IndexOf("Word =", StringComparison.Ordinal), target.Position);
	}

	/// <summary>
	/// A Roslyn that knows no document, and an analysis that is published the moment Roslyn has been
	/// asked: before, the view says nothing is known yet.
	/// </summary>
	static (RoslynGramCompletion Roslyn, EmbeddedGrammarView View) Arrange(out Func<bool> asked)
	{
		var flag   = false;
		var roslyn = new RoslynGramCompletion(new AdhocWorkspace(), () =>
		{
			flag = true;
			return null;
		});

		asked = () => flag;

		return (roslyn, View(Source));
	}

	internal static EmbeddedGrammarView View(string source)
	{
		var tree        = CSharpSyntaxTree.ParseText(source);
		var compilation = CSharpCompilation.Create(
			"Host", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
		var analyses    = EmbeddedGrammarService.Analyze(compilation.GetSemanticModel(tree), tree.GetRoot());

		return new EmbeddedGrammarView(
			true,
			[.. analyses.SelectMany(static analysis => analysis.Classifications)],
			[.. analyses.SelectMany(static analysis => analysis.Diagnostics)],
			[.. analyses.SelectMany(static analysis => analysis.Symbols)],
			[],
			[],
			[],
			[.. analyses.SelectMany(static analysis => analysis.Braces)],
			[.. analyses.SelectMany(static analysis => analysis.FoldingRanges)],
			[.. analyses.SelectMany(static analysis => analysis.DocumentSymbols)],
			[.. analyses.SelectMany(static analysis => analysis.PublishedApis)]);
	}
}
