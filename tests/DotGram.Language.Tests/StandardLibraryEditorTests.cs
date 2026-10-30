using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// The editor's half of what the library's tests in DotGram.Tests ask of the compiler: an
/// included grammar's text is read apart from the author's and from the next one's.
/// </summary>
public sealed class StandardLibraryEditorTests
{
	/// <summary>
	/// An included grammar that leaves a construct open ends it at its own end, and the next one
	/// is read whole — for the editor as for the compiler.
	/// </summary>
	[Theory]
	[InlineData("W1 = 'a' /* open\n")]
	[InlineData("W1 = ('a'\n")]
	[InlineData("W1 = \"x\\\n")]
	public void An_included_grammar_does_not_reach_the_next_one(string first)
	{
		const string own    = "A = 'a'\n";
		const string second = "W2 = ['a'..'z']+\n";

		var (text, map) = GrammarSplice.Join(
			new GrammarSplice.Part(own, null, null),
			[new GrammarSplice.Part(first, "One", null), new GrammarSplice.Part(second, "Two", null)]);

		var cuts    = GrammarSplice.Cuts(text, map.Segments.Skip(1).Select(one => (one.Start, one.Length)));
		var options = new GramAnalysisOptions { Own = own.Length, Cuts = cuts };

		Assert.Contains(GramLanguageService.Analyze(text, options).Symbols, item => item.Name == "W2");
	}

	/// <summary>In the editor an author's open comment ends with their text, and the included symbols stay.</summary>
	[Fact]
	public void The_editor_reads_the_authors_open_comment_apart_from_the_included_grammars()
	{
		const string own = "A = Base.Word /* open\n";

		var (text, map) = GrammarSplice.Join(
			new GrammarSplice.Part(own, null, null),
			[new GrammarSplice.Part("Word = ['a'..'z']+\n", "Base", null)]);

		var document = GramLanguageService.Analyze(
			text,
			new GramAnalysisOptions
			{
				Own  = own.Length,
				Cuts = GrammarSplice.Cuts(text, map.Segments.Skip(1).Select(one => (one.Start, one.Length))),
			});

		Assert.Contains(document.Symbols, item => item.Name == "Word");
		Assert.Contains(document.Diagnostics, d => d.Id == GramLexer.UnterminatedComment);
	}
}
