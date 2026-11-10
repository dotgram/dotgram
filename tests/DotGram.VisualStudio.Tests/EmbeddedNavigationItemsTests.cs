using System;
using System.Linq;

using DotGram.Language;
using DotGram.VisualStudio;

using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class EmbeddedNavigationItemsTests
{
	// A document laid out as positions:
	//   Alpha  host 10..60, string 20..50, rules Word 22..30 and Start 32..48 (Start holds Inner 34..40)
	//   Beta   host 100..200, string 110..140, rule Rule 112..122; a second grammar on the same class,
	//          Beta.Tape, string 150..190, with no rules
	static readonly EmbeddedGrammarOutline[] Outlines =
	[
		Outline("Beta",      110, 30, 100, 100, Symbol("Rule", 112, 10)),
		Outline("Alpha",      20, 30,  10,  50, Symbol("Word", 22, 8), Symbol("Start", 32, 16, Symbol("Inner", 34, 6))),
		Outline("Beta.Tape", 150, 40, 100, 100),
	];

	[Fact]
	public void ListsTheGrammarsByNameEachWithItsOwnSymbols()
	{
		var grammars = EmbeddedNavigationItems.Grammars(Outlines, Locate);

		Assert.Equal(new[] { "Alpha", "Beta", "Beta.Tape" }, grammars.Select(grammar => grammar.Display));
		Assert.Equal(new[] { "Inner", "Start", "Word" }, grammars[0].Symbols.Select(symbol => symbol.Display));
		Assert.Equal(new[] { "Rule" }, grammars[1].Symbols.Select(symbol => symbol.Display));
		Assert.Empty(grammars[2].Symbols);
	}

	[Fact]
	public void TellsApartGrammarsOfOneNameAndOnesWithNone()
	{
		var outlines = new[]
		{
			Outline("Inspect(grammar)", 300, 10, 290, 50),
			Outline(null,               500, 10, 500, 10),
			Outline("Inspect(grammar)", 100, 10,  90, 50),
		};

		var grammars = EmbeddedNavigationItems.Grammars(outlines, Locate);

		Assert.Equal(
			new[] { "Grammar (line 50)", "Inspect(grammar) (line 10)", "Inspect(grammar) (line 30)" },
			grammars.Select(grammar => grammar.Display));
	}

	// Inside a string, that grammar; on its class or attributes, the class's first grammar; between
	// and after declarations, the nearest one before; before them all, the first in the document.
	[Theory]
	[InlineData(  0, "Alpha")]
	[InlineData( 12, "Alpha")]
	[InlineData( 25, "Alpha")]
	[InlineData( 55, "Alpha")]
	[InlineData( 80, "Alpha")]
	[InlineData(105, "Beta")]
	[InlineData(120, "Beta")]
	[InlineData(145, "Beta")]
	[InlineData(160, "Beta.Tape")]
	[InlineData(195, "Beta")]
	[InlineData(250, "Beta")]
	public void ChoosesTheGrammarAtThePosition(int position, string expected)
	{
		var grammars = EmbeddedNavigationItems.Grammars(Outlines, Locate);

		Assert.Equal(expected, grammars[EmbeddedNavigationItems.GrammarAt(grammars, position)].Display);
	}

	[Fact]
	public void ChoosesTheInnermostDeclarationOfNestedOnes()
	{
		var outlines = new[]
		{
			Outline("Outer",       20, 10, 10, 100),
			Outline("Outer.Inner", 60, 10, 50,  30),
		};

		var grammars = EmbeddedNavigationItems.Grammars(outlines, Locate);

		Assert.Equal("Outer",       grammars[EmbeddedNavigationItems.GrammarAt(grammars, 40)].Display);
		Assert.Equal("Outer.Inner", grammars[EmbeddedNavigationItems.GrammarAt(grammars, 55)].Display);
		Assert.Equal("Outer",       grammars[EmbeddedNavigationItems.GrammarAt(grammars, 90)].Display);
	}

	[Fact]
	public void ChoosesNoGrammarWhereThereIsNone()
	{
		Assert.Equal(-1, EmbeddedNavigationItems.GrammarAt([], 0));
	}

	// The innermost symbol of the grammar at the caret, or none between rules and outside the string.
	[Theory]
	[InlineData(20, null)]
	[InlineData(25, "Word")]
	[InlineData(31, null)]
	[InlineData(33, "Start")]
	[InlineData(35, "Inner")]
	[InlineData(45, "Start")]
	[InlineData(55, null)]
	public void ChoosesTheInnermostSymbolAtThePositionOrNone(int position, string? expected)
	{
		var symbols = EmbeddedNavigationItems.Grammars(Outlines, Locate)[0].Symbols;
		var index   = EmbeddedNavigationItems.SymbolAt(symbols, position);

		Assert.Equal(expected, index < 0 ? null : symbols[index].Display);
	}

	// Two calls on one line, `Inspect("A = 'a'"); Inspect("B = 'b'");`, share the line as well as
	// the name: the column tells them apart.
	[Fact]
	public void TellsApartGrammarsOfOneNameOnOneLine()
	{
		var outlines = new[]
		{
			Outline("Inspect(grammar)", 105, 2, 100, 50),
			Outline("Inspect(grammar)", 102, 2, 100, 50),
			Outline("Inspect(grammar)", 300, 2, 290, 50),
		};

		var grammars = EmbeddedNavigationItems.Grammars(outlines, Locate);

		Assert.Equal(
			new[] { "Inspect(grammar) (line 10, column 3)", "Inspect(grammar) (line 10, column 6)", "Inspect(grammar) (line 30)" },
			grammars.Select(grammar => grammar.Display));
	}

	// Text typed before the grammars moves every position in the outlines: what the bar chooses at
	// the caret and where it navigates to are read in the edited document, not the one analysed.
	[Fact]
	public void FollowsAnEditBeforeTheGrammars()
	{
		const int inserted = 1000;

		var moved    = EmbeddedNavigationItems.Translate(Outlines, span => new TextSpan(span.Start + inserted, span.Length));
		var grammars = EmbeddedNavigationItems.Grammars(moved, Locate);
		var alpha    = grammars[EmbeddedNavigationItems.GrammarAt(grammars, 25 + inserted)];
		var inner    = alpha.Symbols[EmbeddedNavigationItems.SymbolAt(alpha.Symbols, 35 + inserted)];

		Assert.Equal("Alpha", alpha.Display);
		Assert.Equal(20 + inserted, alpha.Span.Start);
		Assert.Equal(10 + inserted, alpha.HostSpan.Start);
		Assert.Equal("Inner", inner.Display);
		Assert.Equal(34 + inserted, inner.Position);
		Assert.Equal(34 + inserted, inner.SelectionPosition);
		Assert.Equal("Beta", grammars[EmbeddedNavigationItems.GrammarAt(grammars, 105 + inserted)].Display);
		Assert.Equal("Alpha", grammars[EmbeddedNavigationItems.GrammarAt(grammars, 105)].Display);
	}

	static (int Line, int Column) Locate(int position)
	{
		return (position / 10, position % 10 + 1);
	}

	static EmbeddedGrammarOutline Outline(
		string?                      name,
		int                          start,
		int                          length,
		int                          hostStart,
		int                          hostLength,
		params HostDocumentSymbol[] symbols)
	{
		return new EmbeddedGrammarOutline(name, new TextSpan(start, length), new TextSpan(hostStart, hostLength), symbols);
	}

	static HostDocumentSymbol Symbol(string name, int start, int length, params HostDocumentSymbol[] children)
	{
		return new HostDocumentSymbol(
			name,
			GramDocumentSymbolKind.Rule,
			new TextSpan(start, length),
			new TextSpan(start, Math.Min(length, name.Length)),
			default,
			children);
	}
}
