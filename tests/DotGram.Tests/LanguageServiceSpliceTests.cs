using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The editor's analysis of a grammar with what its host includes spliced on after it: where
/// a name goes is where the binder sends it, and the options are the generator's.
/// </summary>
public sealed class LanguageServiceSpliceTests
{
	const string Included =
		"Word = ['a'..'z']+\n" +
		"namespace Lexical\n" +
		"{\n" +
		"Digits = ['0'..'9']+\n" +
		"}\n";

	static string Joined(string own, string included)
	{
		return own + GrammarSplice.Join(
			new GrammarSplice.Part("", null, null),
			[new GrammarSplice.Part(included, "Sql92", null)]).Text;
	}

	static int Declared(string text, string declaration)
	{
		var at = text.LastIndexOf(declaration + " =", StringComparison.Ordinal);

		Assert.True(at >= 0, $"'{declaration}' is not declared in the text");

		return at;
	}

	/// <summary>A name a <c>using</c> brings in goes to the included grammar's rule.</summary>
	[Fact]
	public void An_imported_name_resolves_into_the_included_grammar()
	{
		const string own = "using Sql92;\nStart = Word\nparse Start\n";
		var text = Joined(own, Included);

		var word = Assert.Single(
			GramLanguageService.Analyze(text).Symbols,
			symbol => symbol.Name == "Word" && symbol.Position < own.Length);

		Assert.Equal(own.IndexOf("Word", StringComparison.Ordinal), word.Position);
		Assert.Equal(Declared(text, "Word"), word.DefinitionPosition);
		Assert.True(word.DefinitionPosition >= own.Length);
	}

	/// <summary>
	/// A name qualified more than one level deep is a use of its last part, and goes to the
	/// rule in the nested namespace.
	/// </summary>
	[Fact]
	public void A_deeply_qualified_name_is_a_use_of_its_last_part()
	{
		const string own = "Start = Sql92.Lexical.Digits\nparse Start\n";
		var text = Joined(own, Included);

		var digits = Assert.Single(
			GramLanguageService.Analyze(text).Symbols,
			symbol => symbol.Name == "Digits" && symbol.Position < own.Length);

		Assert.Equal(own.IndexOf("Digits", StringComparison.Ordinal), digits.Position);
		Assert.Equal("Digits".Length, digits.Length);
		Assert.Equal(Declared(text, "Digits"), digits.DefinitionPosition);
	}

	/// <summary>
	/// Two grammars may each declare a rule of one name; each use goes to the one it names,
	/// and not to whichever comes first in the joined text.
	/// </summary>
	[Fact]
	public void A_name_both_grammars_declare_goes_where_it_is_qualified_to()
	{
		const string own = "Word = 'w'\nStart = Word & Sql92.Word\nparse Start\n";
		var text = Joined(own, Included);

		var document = GramLanguageService.Analyze(text);
		var uses     = document.Symbols
			.Where(symbol => symbol.Name == "Word" && symbol.Position < own.Length && !symbol.IsDefinition)
			.OrderBy(symbol => symbol.Position)
			.ToArray();

		Assert.Equal(2, uses.Length);
		Assert.Equal(0, uses[0].DefinitionPosition);
		Assert.Equal(Declared(text, "Word"), uses[1].DefinitionPosition);

		// The quick info follows the same rule: the qualified use describes the included one.
		var described = Assert.Single(document.Classifications, span => span.Position == uses[1].Position);
		Assert.StartsWith("Word = ['a'..'z']+", described.QuickInfo, StringComparison.Ordinal);

		// And each declaration is its own definition.
		Assert.All(
			document.Symbols.Where(symbol => symbol.Name == "Word" && symbol.IsDefinition),
			symbol => Assert.Equal(symbol.Position, symbol.DefinitionPosition));
	}

	/// <summary>
	/// A rule of the standard library is spliced on past the end of the text, where nobody
	/// can be taken; a use of one is not indexed as a rule.
	/// </summary>
	[Fact]
	public void A_standard_library_rule_is_not_a_place_to_go()
	{
		const string own = "Start = Std.Integer\nparse Start\n";

		var symbols = GramLanguageService.Analyze(own).Symbols;

		Assert.All(symbols, symbol => Assert.True(symbol.DefinitionPosition < own.Length));
	}

	/// <summary><c>Lexical</c> is passed to the compile, as the generator passes it.</summary>
	[Fact]
	public void The_lexical_request_reaches_the_compile()
	{
		const string own = "Start = Name\nName = ['a'..'z']+ & @Check\nparse Start\n";

		var characters = GramLanguageService.Analyze(own, new GramAnalysisOptions());
		var tokens     = GramLanguageService.Analyze(own, new GramAnalysisOptions { Lexical = true });

		Assert.DoesNotContain(characters.Diagnostics, diagnostic => diagnostic.Id == GramCompiler.NotCut);
		Assert.Contains(tokens.Diagnostics, diagnostic => diagnostic.Id == GramCompiler.NotCut);
	}

	/// <summary>
	/// <c>Own</c> is passed to the compile: past it, a <c>parse</c> is the included grammar's,
	/// which publishes it itself, so the rule it names is not reached from here.
	/// </summary>
	[Fact]
	public void The_own_length_reaches_the_compile()
	{
		const string own = "Start = 'a'\nparse Start\n";
		var text = Joined(own, "Word = ['a'..'z']+\nparse Word\n");

		var whole = GramLanguageService.Analyze(text);
		var split = GramLanguageService.Analyze(text, new GramAnalysisOptions { Own = own.Length });

		Assert.DoesNotContain(whole.Diagnostics, diagnostic => diagnostic.Position >= own.Length);
		Assert.Contains(split.Diagnostics, diagnostic =>
			diagnostic.Position == Declared(text, "Word") && diagnostic.Message.Contains("'Word'"));
	}
}
