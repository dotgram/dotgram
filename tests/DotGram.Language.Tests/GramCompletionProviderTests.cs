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
	/// Typing opens completion on a name's characters and the dot, never on whitespace: a list opened by
	/// a line break or the tab that indents the next line would take the next Tab for its first item.
	/// </summary>
	[Theory]
	[InlineData("Sum : int =\n", '\n', false)]
	[InlineData("Sum : int =\n\t", '\t', false)]
	[InlineData("Sum : int =\n   ", ' ', false)]
	[InlineData("Sum : int =\n   left: Sum ", ' ', false)]
	[InlineData("Sum : int =\n   left: Sum &", '&', false)]
	[InlineData("Sum : int =\n   left:", ':', false)]
	[InlineData("Sum : int =\n   | n: Number                =", '=', false)]
	[InlineData("Sum : int =\n   | n: Number                =>", '>', false)]
	[InlineData("A : int = 'a' => 1", '1', false)]
	[InlineData("A : int = 'a' => 12", '2', false)]
	[InlineData("Sum : int =\n   l", 'l', true)]
	[InlineData("Sum : int =\n   left: S", 'S', true)]
	[InlineData("Sum : int =\n   _", '_', true)]
	[InlineData("A : int = Rule2", '2', true)]
	[InlineData("A : int = Std.", '.', true)]
	public void Typing_opens_completion_only_on_a_name_or_a_trigger(string text, char typed, bool opens)
	{
		text = text.Replace("\n", Environment.NewLine);

		Assert.Equal(opens, GramCompletionProvider.OpensOnTyping(text.Length, index => text[index], text.Length, typed));
	}

	/// <summary>
	/// Open completion stays open while a name is typed and closes on whatever ends it, so that it never
	/// grows over a space, a colon or `=>` and hands the next Tab its first item.
	/// </summary>
	[Theory]
	[InlineData('a', true)]
	[InlineData('Z', true)]
	[InlineData('_', true)]
	[InlineData('7', true)]
	[InlineData(' ', false)]
	[InlineData('\t', false)]
	[InlineData(':', false)]
	[InlineData('&', false)]
	[InlineData('=', false)]
	[InlineData('.', false)]
	[InlineData('(', false)]
	public void Open_completion_stays_open_only_while_a_name_is_typed(char typed, bool keeps)
	{
		Assert.Equal(keeps, GramCompletionProvider.KeepsOpenOnTyping(typed));
	}

	/// <summary>Deleting opens completion where the caret is left inside a name, and nowhere else.</summary>
	[Theory]
	[InlineData("Sum : int =\n   lef", true)]
	[InlineData("Sum : int =\n   ", false)]
	[InlineData("Sum : int =\n", false)]
	[InlineData("Sum : int = a &", false)]
	[InlineData("A : int = 'a' => 1", false)]
	public void Deleting_opens_completion_only_inside_a_name(string text, bool opens)
	{
		text = text.Replace("\n", Environment.NewLine);

		Assert.Equal(opens, GramCompletionProvider.OpensOnDeletion(text.Length, index => text[index], text.Length));
	}

	/// <summary>
	/// Asked for between words, completion still answers, with nothing to replace: where it opens is the
	/// editor's to decide by what the user did, not the provider's.
	/// </summary>
	[Fact]
	public void Asked_for_at_the_start_of_a_line_completion_replaces_nothing()
	{
		var text = "Sum : int =" + Environment.NewLine + "   ";
		var provider = new StandaloneGramCompletionProvider(GramLanguageService.Analyze(text));

		Assert.Equal(new TextSpan(text.Length, 0), provider.Start(text, text.Length));
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
