using System;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The terminal inventory: what a lexical machine would have to recognize, and what in a
/// given grammar stands in the way of asking (docs/lexical-adt-design.md).
/// </summary>
/// <remarks>
/// Asserted on the model rather than on generated code, because nothing is generated: this
/// pass emits nothing and rewrites nothing. What is worth checking is that the boundary
/// falls where §4.5 puts it, that the numbering is a partition, and that the shapes the
/// design has not decided are named rather than passed over.
/// </remarks>
public sealed class TerminalInventoryTests
{
	static TerminalInventory Of(string grammar) =>
		TerminalInventory.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));

	/// <summary>A grammar in which nothing is trivia has no boundary to find.</summary>
	/// <remarks>
	/// A URL is written over characters because a URL <em>is</em> characters, and §4.5 is
	/// what says so: no <c>trivia</c>, no seam, no lexical half. Such a grammar keeps the
	/// character machine and pays nothing.
	/// </remarks>
	[Fact]
	public void A_grammar_without_trivia_has_no_lexical_half()
	{
		var inventory = Of("Start = 'a' & \"bc\" & ['d'..'f']\nparse Start");

		Assert.False(inventory.Applies);
		Assert.Empty(inventory.Patterns);
		Assert.Empty(inventory.Kinds);
	}

	/// <summary>
	/// A literal that is all word characters is a word; anything else is a mark.
	/// </summary>
	/// <remarks>
	/// §4.6 already decided this, at the moment it wove a boundary round <c>"if"</c> and
	/// left <c>'('</c> alone, so the shape in the lowered graph is the answer and nothing
	/// has to ask the boundary rule again.
	/// </remarks>
	[Fact]
	public void A_word_literal_is_a_word_and_a_bracket_is_a_mark()
	{
		var inventory = Of(
			"""
			wordboundary = ['a'..'z']
			trivia = ' '*
			Start = "if" & '(' & "then" & ')'
			parse Start
			""");

		Assert.Equal(["if", "then"], Words(inventory));
		Assert.Equal(["(", ")"],     Marks(inventory));
	}

	/// <summary>
	/// A call that crosses into a rule with no trivia is a class, and stops the walk.
	/// </summary>
	/// <remarks>
	/// This is the whole of the boundary: not a declaration, not a file, but a reference
	/// leaving the spaced region. `Word`'s own insides are the lexer's and contribute no
	/// terminal of their own — otherwise every letter would be one.
	/// </remarks>
	[Fact]
	public void A_crossing_into_an_unspaced_rule_is_a_class()
	{
		var inventory = Of(
			"""
			trivia = ' '*
			namespace Lexical
			{
				trivia = none
				Word = ['a'..'z'] & ['a'..'z']*
			}
			Start = Lexical.Word & ',' & Lexical.Word
			parse Start
			""");

		Assert.Equal(["Word"], Classes(inventory));
		Assert.Equal([","],    Marks(inventory));
		Assert.Empty(Words(inventory));
	}

	/// <summary>What trivia and the word boundary are made of never becomes a terminal.</summary>
	/// <remarks>
	/// Both are ordinary rules declared in the same spaced namespace as the syntax, so
	/// walking them would be the natural mistake — and it was made once: whitespace,
	/// <c>"--"</c> and <c>"/*"</c> came back as terminals of SQL. The closure matters as
	/// much as the two roots, since <c>trivia</c> is usually a choice of three more rules.
	/// </remarks>
	[Fact]
	public void Trivia_and_the_boundary_contribute_no_terminals()
	{
		var inventory = Of(
			"""
			wordboundary = ['a'..'z']
			trivia = { (Space | Comment)* }
			Space   = [' ' | '\t']+
			Comment = "--" & [^ '\n']*
			Start = "if" & ';'
			parse Start
			""");

		// `;` and not `x`: a one-character literal that continues a word is a word, boundary
		// woven and all, which is §4.6 doing its job and not this pass doing something odd.
		Assert.Equal(["if"], Words(inventory));
		Assert.Equal([";"],  Marks(inventory));
		Assert.DoesNotContain("--", Marks(inventory));
	}

	/// <summary>
	/// The kinds are numbered one-based and consecutively, and every pattern is in one.
	/// </summary>
	/// <remarks>
	/// The point of the numbering is that a pattern's kinds are runs, so that a syntactic
	/// test is <c>(uint)(kind - From) &lt;= (uint)(To - From)</c> and a sum type costs
	/// nothing. A pattern in no kind at all would be a terminal the lexer can recognize and
	/// never report.
	/// </remarks>
	[Fact]
	public void Every_pattern_is_in_a_kind_and_the_kinds_are_consecutive()
	{
		var inventory = Of(
			"""
			wordboundary = ['a'..'z']
			trivia = ' '*
			namespace Lexical
			{
				trivia = none
				Word = ['a'..'z'] & ['a'..'z']*
			}
			Start = "if" & '(' & Lexical.Word & ')' & "else" & Lexical.Word
			parse Start
			""");

		Assert.Equal(
			Enumerable.Range(1, inventory.Kinds.Count),
			inventory.Kinds.Select(kind => kind.Number));

		foreach (var pattern in inventory.Patterns)
			Assert.NotEmpty(inventory.KindsOf(pattern));
	}

	/// <summary>
	/// A character class in syntactic position is its characters, while it is small enough.
	/// </summary>
	/// <remarks>
	/// <c>['+' | '-']</c> is two terminals and is written that way all over a grammar. A
	/// <c>\p{L}</c> is tens of thousands, and tens of thousands of kinds is the character
	/// machine wearing a different name — so it is named as an obstacle rather than quietly
	/// turned into an alphabet.
	/// </remarks>
	[Fact]
	public void A_small_character_class_is_its_characters_and_a_wide_one_is_named()
	{
		var small = Of("trivia = ' '*\nStart = ['+' | '-'] & 'x'\nparse Start");

		Assert.Equal(["+", "-", "x"], Marks(small));
		Assert.Empty(small.Blocked);

		var wide = Of("trivia = ' '*\nStart = 'x' & [\\p{L}]\nparse Start");

		Assert.Contains(wide.Blocked, reason => reason.Contains("characters in syntactic position"));
	}

	/// <summary>
	/// A negated class names what it excludes, and only that has to be numbered.
	/// </summary>
	/// <remarks>
	/// <c>[^ '(' | ')']</c> is "one item that is not a bracket", and over kinds it is the
	/// same sentence about a wider alphabet — which is what
	/// <c>Subquery = '(' &amp; (Balanced | [^ '(' | ')'])* &amp; ')'</c> means by "anything
	/// balanced" and could not say over characters. Counting its sixty-five thousand members
	/// was the first attempt, and it refused the split of a grammar that had no problem.
	/// </remarks>
	[Fact]
	public void A_negated_class_names_only_what_it_excludes()
	{
		var inventory = Of("trivia = ' '*\nStart = 'x' & [^ '(' | ')']\nparse Start");

		Assert.Empty(inventory.Blocked);
		Assert.Equal(["x", "(", ")"], Marks(inventory));
	}

	/// <summary>
	/// A rule that is a choice of literals is a set of terminals, and becomes a range.
	/// </summary>
	/// <remarks>
	/// <c>ExpressionLanguage</c> is where this matters: `Keyword` lists thirty-eight words
	/// in a lexical namespace, every one of which also stands in the syntax as a literal of
	/// its own, and it is reached only through <c>Name = ?!Keyword &amp; Word</c>. Given a
	/// kind of its own the lexer would have to decide whether <c>if</c> is the word or the
	/// class; given a range it decides nothing, and the lookahead becomes a subtraction and
	/// a comparison.
	/// </remarks>
	[Fact]
	public void A_rule_that_is_a_set_of_known_words_becomes_a_range()
	{
		var inventory = Of(
			"""
			wordboundary = ['a'..'z']
			trivia = ' '*
			namespace Lexical
			{
				trivia = none
				Keyword = ("if" | "else" | "while") & ?!['a'..'z']
				Word    = ['a'..'z'] & ['a'..'z']*
			}
			Name  = ?!Lexical.Keyword & Lexical.Word
			Start = "if" & Name & "else" & Name & "while" & Name
			parse Start
			""");

		var keyword = Assert.Single(inventory.Sets, set => set.Name == "Keyword");

		Assert.Equal(3, keyword.Count);
		Assert.Equal(new TerminalInventory.Group("Keyword", 1, 3), Assert.Single(keyword.Ranges));

		// And it is no longer a class: its strings are terminals, and a kind of its own on
		// top of them is the `if` with two kinds.
		Assert.DoesNotContain("Keyword", Classes(inventory));
		Assert.Empty(inventory.Blocked);
	}

	/// <summary>The same list one namespace over comes to the same range.</summary>
	/// <remarks>
	/// `SqlStandard92`'s `Reserved` sits where trivia is *not* empty, so it is syntax and its
	/// words are walked into the word group directly rather than met as a crossing. Two
	/// different roads, and what a set difference needs at the end of either is the same:
	/// those words in one run.
	/// </remarks>
	[Fact]
	public void The_same_list_in_a_spaced_namespace_is_the_same_range()
	{
		var inventory = Of(
			"""
			wordboundary = ['a'..'z']
			trivia = ' '*
			Reserved = "if" | "else" | "while"
			namespace Lexical
			{
				trivia = none
				Word = ['a'..'z'] & ['a'..'z']*
			}
			Name  = ?!Reserved & Lexical.Word
			Start = "if" & Name
			parse Start
			""");

		var reserved = Assert.Single(inventory.Sets, set => set.Name == "Reserved");

		Assert.Equal(new TerminalInventory.Group("Reserved", 1, 3), Assert.Single(reserved.Ranges));
		Assert.Empty(inventory.Blocked);
	}

	/// <summary>
	/// A set nested in another stays one range; two that cross cannot both, and say so.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The ordering is greedy and laminar: the largest set that divides what is left goes
	/// first, and the halves are ordered the same way inside. Everything nested or disjoint
	/// comes out whole, which is nearly all of it — in `SqlStandard92`, `Reserved` is
	/// fifty-six words in one run and `TruthValue`, `Quantifier` and `TrimSpecification` sit
	/// inside it in runs of their own.
	/// </para>
	/// <para>
	/// Two that cross cannot both be one run under any order, and the one that loses carries
	/// two ranges rather than a complaint: two comparisons is not a fifty-way choice.
	/// `SetQuantifier` is `DISTINCT | ALL` against `Quantifier`'s `ALL | SOME | ANY`, and
	/// that is the shape written here.
	/// </para>
	/// </remarks>
	[Fact]
	public void Nested_sets_stay_one_range_and_crossing_ones_split()
	{
		var inventory = Of(
			"""
			wordboundary = ['a'..'z']
			trivia = ' '*
			Truth    = "yes" | "no"
			Reserved = "yes" | "no" | "all" | "some"
			Quant    = "all" | "some"
			Both     = "all" | "when"
			Start = Truth & Reserved & Quant & Both & "when"
			parse Start
			""");

		// Nested in the largest, so whole.
		Assert.Single(Assert.Single(inventory.Sets, set => set.Name == "Reserved").Ranges);
		Assert.Single(Assert.Single(inventory.Sets, set => set.Name == "Truth").Ranges);
		Assert.Single(Assert.Single(inventory.Sets, set => set.Name == "Quant").Ranges);

		// Crossing `Reserved` — it holds `all` and `when`, and `when` is not reserved.
		Assert.Equal(2, Assert.Single(inventory.Sets, set => set.Name == "Both").Ranges.Count);
	}

	static string[] Words(TerminalInventory inventory) =>
		[.. inventory.Patterns.OfType<TerminalInventory.Pattern.Word>().Select(word => word.Text)];

	static string[] Marks(TerminalInventory inventory) =>
		[.. inventory.Patterns.OfType<TerminalInventory.Pattern.Mark>().Select(mark => mark.Text)];

	static string[] Classes(TerminalInventory inventory) =>
		[.. inventory.Patterns.OfType<TerminalInventory.Pattern.Class>().Select(one => one.Rule.Name)];
}
