using System;
using System.Collections.Generic;
using System.IO;
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

	/// <summary>
	/// A comment or a literal names nothing: completion opens there neither on typing nor on asking, and
	/// a name beside one is completed as anywhere else. The caret is where `|` stands.
	/// </summary>
	[Theory]
	[InlineData("// TODO fix|", false)]
	[InlineData("A : int = 'a' => 1 // fi|", false)]
	[InlineData("/* fix| */ A : int = 'a' => 1", false)]
	[InlineData("/* fix */ A| : int = 'a' => 1", true)]
	[InlineData("/* unterminated fix|", false)]
	[InlineData("A : int = 'x|' => 1", false)]
	[InlineData("A : int = \"select|\" => 1", false)]
	[InlineData("A : int = \"select\"i| => 1", false)]
	[InlineData("A : int = 'unterminated|", false)]
	[InlineData("A : int = 'a' B|", true)]
	[InlineData("A : int = 'a'| B", true)]
	[InlineData("A : int = 'a' => 1\nB : int = A|", true)]
	[InlineData("A : int = 'a' => @(\"in C# x|\")", false)]
	[InlineData("A : int = 'a' => @(x) // C# comment|", false)]
	[InlineData("A : int = 'a' => @(Foo(x|))", true)]
	[InlineData("/* a block\nthat goes on fix| */\nA : int = 'a' => 1", false)]
	[InlineData("/* a block\nthat ends */ A|", true)]
	[InlineData("A : int = '\\'' B|", true)]
	public void Completion_does_not_open_in_a_comment_or_a_literal(string marked, bool opens)
	{
		var position = marked.IndexOf('|');
		var text     = marked.Remove(position, 1);
		var provider = new StandaloneGramCompletionProvider(GramLanguageService.Analyze(text));

		Assert.Equal(opens, provider.Start(text, position) is not null);
	}

	/// <summary>The same inside a grammar embedded in C#: its comments and literals, not the C# string around it.</summary>
	[Fact]
	public async Task Completion_does_not_open_in_a_comment_or_a_literal_of_an_embedded_grammar()
	{
		var folder   = Path.Combine(PlaygroundSnapshotTests.SourceRoot(), "tests", "DotGram.VisualStudio.Tests", "Playground");
		var text     = File.ReadAllText(Path.Combine(folder, PlaygroundEmbedded.FileName));
		var view     = await PlaygroundEmbedded.ViewAsync(text, folder, TestContext.Current.CancellationToken);
		var provider = new EmbeddedGramCompletionProvider(view);

		var comment = text.IndexOf("// Hover Sum", StringComparison.Ordinal) + "// Hover Su".Length;
		var literal = text.IndexOf("['0'..'9']+", StringComparison.Ordinal) + "['0".Length;
		var name    = text.IndexOf("& right: Product", StringComparison.Ordinal) + "& right: Prod".Length;

		Assert.Null(provider.Start(text, comment));
		Assert.Null(provider.Start(text, literal));
		Assert.NotNull(provider.Start(text, name));
	}

	/// <summary>A block comment over lines of an embedded grammar keeps completion closed on every line of it.</summary>
	[Fact]
	public async Task Completion_does_not_open_on_a_later_line_of_a_block_comment_in_an_embedded_grammar()
	{
		var folder   = Path.Combine(PlaygroundSnapshotTests.SourceRoot(), "tests", "DotGram.VisualStudio.Tests", "Playground");
		var original = File.ReadAllText(Path.Combine(folder, PlaygroundEmbedded.FileName));
		var newLine  = original.Contains("\r\n") ? "\r\n" : "\n";
		var anchor   = "trivia = [' ' | '\\t']*" + newLine;
		var at       = original.IndexOf(anchor, StringComparison.Ordinal);

		Assert.True(at >= 0);

		var text     = original.Insert(at + anchor.Length, newLine + "\t/* a block" + newLine + "\tthat goes on fix */" + newLine);
		var view     = await PlaygroundEmbedded.ViewAsync(text, folder, TestContext.Current.CancellationToken);
		var provider = new EmbeddedGramCompletionProvider(view);

		Assert.Null(provider.Start(text, text.IndexOf("a block", StringComparison.Ordinal) + "a blo".Length));
		Assert.Null(provider.Start(text, text.IndexOf("goes on fix", StringComparison.Ordinal) + "goes on fi".Length));
	}

	/// <summary>
	/// Whether the caret is in a comment or a literal is read from the current text, not from what the
	/// last analysis classified: a block comment opened since it, or before any, keeps the list closed
	/// on its later lines, and one whose <c>/*</c> was deleted since no longer does.
	/// </summary>
	[Theory]
	[InlineData("", "/* a block\nthat goes on fix| */\nA : int = 'a' => 1", false)]
	[InlineData("A : int = 'a' => 1\nB : int = 'b' => 1\n", "A : int = 'a' => 1\n/* B : int = 'b'\nfix| */ => 1\n", false)]
	[InlineData("/* a block\nthat goes on fix */\nA : int = 'a' => 1", "   a block\nthat goes on fix| */\nA : int = 'a' => 1", true)]
	public void Completion_reads_the_current_text_not_the_last_analysis(string analysed, string marked, bool opens)
	{
		var position = marked.IndexOf('|');
		var text     = marked.Remove(position, 1);
		var document = analysed.Length == 0 ? GramAnalysisSession.EmptyDocument : GramLanguageService.Analyze(analysed);
		var provider = new StandaloneGramCompletionProvider(document);

		Assert.Equal(opens, provider.Start(text, position) is not null);
	}

	/// <summary>
	/// C#'s strings inside <c>@(…)</c> are read as C# reads them, verbatim and raw ones too, so that a name
	/// after one is completed; and the grammar's own literals and comments as before, at the edges of a
	/// line, of the text and of an <c>i</c> marker. The caret is where `|` stands.
	/// </summary>
	[Theory]
	[InlineData("A : int = 'a' => @(Path(@\"C:\\\", x|))", true)]
	[InlineData("A : int = 'a' => @(Path($@\"{d}\\\", x|))", true)]
	[InlineData("A : int = 'a' => @(F(\"\"\"a\"b\"\"\", x|))", true)]
	[InlineData("A : int = 'a' => @(F(\"\"\"a\"b|", false)]
	[InlineData("A : int = 'a' => @(F(@\"a\"\"b|", false)]
	[InlineData("A : int = 'a' => @(F(@\"over\nlines\", x|))", true)]
	[InlineData("A : int = 'a' => @(F('\"', x|))", true)]
	[InlineData("A : int = 'a' => @(F(\"\\\\\", x|))", true)]
	[InlineData("A : int = 'a' => @(F($\"{(a ? \"x\" : \"y\")}\", x|))", true)]
	[InlineData("A : int = 'a' => @(F(\"a\" /* \" */, x|))", true)]
	[InlineData("A : int = 'a'\r\n  B|", true)]
	[InlineData("A : int = 'a' //x\r\nB|", true)]
	[InlineData("A : int = '\\\\' B|", true)]
	[InlineData("A : int = \"a\\\"b\" B|", true)]
	[InlineData("A : int = 'a'i| B", false)]
	[InlineData("A : int = 'a'ix| B", true)]
	[InlineData("|", true)]
	[InlineData("/**/A|", true)]
	[InlineData("/*/ A|", false)]
	[InlineData("A : int = \"a\"\nB|", true)]
	[InlineData("A : int = 'a\nB|", true)]
	public void Completion_reads_strings_and_comments_as_their_language_does(string marked, bool opens)
	{
		var position = marked.IndexOf('|');
		var text     = marked.Remove(position, 1);
		var provider = new StandaloneGramCompletionProvider(GramLanguageService.Analyze(text));

		Assert.Equal(opens, provider.Start(text, position) is not null);
	}

	/// <summary>
	/// A capture is offered as a construction names it, up to the caret just after the rule's last
	/// character: `=> n` typed at the end of a rule finds `n`, which Enter then leaves as typed, rather
	/// than the first name `n` begins.
	/// </summary>
	[Fact]
	public async Task A_capture_is_offered_at_the_end_of_its_rule()
	{
		const string text = "Number : int = '1' => 1\nSum : int =\n\tleft: Sum & '+' & right: Number\t=> @(left + right)\n\t| n: Number\t=> n";
		var provider = new StandaloneGramCompletionProvider(GramLanguageService.Analyze(text));
		var start    = Assert.IsType<TextSpan>(provider.Start(text, text.Length));
		var items    = await provider.ItemsAsync(text, text.Length, start, null, TestContext.Current.CancellationToken);

		Assert.Contains(items, static item => item.Label == "n");
	}

	/// <summary>A DSL's case-insensitive literal is offered as matching what is typed in any case.</summary>
	[Fact]
	public void A_case_insensitive_DSL_literal_says_so()
	{
		Assert.True(DslLiteralCompletionParser.Parse("\"select\"i")!.Value.IgnoreCase);
		Assert.False(DslLiteralCompletionParser.Parse("\"select\"")!.Value.IgnoreCase);
		Assert.False(DslLiteralCompletionParser.Parse("'i'")!.Value.IgnoreCase);
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
