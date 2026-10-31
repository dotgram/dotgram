using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Language.Tests;

public sealed class GramCompletionProviderTests
{
	/// <summary>A name Roslyn offers more than once is listed once, as Roslyn first offered it, in Roslyn's order.</summary>
	[Fact]
	public async Task A_name_Roslyn_offers_twice_is_listed_once_as_first_offered()
	{
		const string text = "A : @int = 'a' => @(Fo";
		var document = GramLanguageService.Analyze(text);
		var provider = new StandaloneGramCompletionProvider(document);
		var start    = Assert.IsType<TextSpan>(provider.Start(text, text.Length));

		var items = await provider.ItemsAsync(
			text,
			text.Length,
			start,
			(_, _) => Task.FromResult<IReadOnlyList<GramCompletionItem>>(
			[
				new GramCompletionItem("Format", "C#", "Format", "", "1", "Format", FromCSharp: true),
				new GramCompletionItem("Foo", "C#", "Foo", "", "2", "Foo", FromCSharp: true),
				new GramCompletionItem("Format", "C#", "Format", "", "3", "Format", FromCSharp: true),
			]),
			TestContext.Current.CancellationToken);

		var csharp = items.Where(static item => item.FromCSharp).ToArray();

		Assert.Equal(["Format", "Foo"], csharp.Select(static item => item.Label));
		Assert.Equal("1", csharp[0].SortText);
	}

	/// <summary>
	/// Go To Definition on a C# name asks what it binds to and nothing more: the search of the project
	/// for every declaration of the name is made only if what it binds to cannot be gone to.
	/// </summary>
	[Fact]
	public async Task The_declarations_of_a_name_are_searched_for_only_when_asked()
	{
		using var workspace = new AdhocWorkspace();
		var project = workspace.AddProject("Host", LanguageNames.CSharp)
			.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
			.AddDocument("Host.cs", "public static class Host { public static int Value(int x) { return x; } }", filePath: @"P:\Host.cs").Project;
		Assert.True(workspace.TryApplyChanges(project.Solution));

		var asked  = 0;
		var roslyn = new RoslynGramCompletion(workspace, () =>
		{
			Interlocked.Increment(ref asked);
			return @"P:\Host.cs";
		});

		var targets = Assert.IsType<RoslynDefinitionTargets>(
			await roslyn.DefinitionTargetsAsync("Host.Value(1)", 6, TestContext.Current.CancellationToken));

		Assert.Equal("Value", targets.Bound?.Name);

		// The document's path is asked for once, for the project; the search would ask again for the host.
		Assert.Equal(1, asked);

		var declarations = await targets.DeclarationsAsync!(TestContext.Current.CancellationToken);

		Assert.Equal("Value", Assert.Single(declarations).Name);
		Assert.Equal(2, asked);
	}
}
