using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The rewrite that puts the syntactic machine over token kinds instead of characters
/// (docs/lexical-adt-design.md).
/// </summary>
/// <remarks>
/// Asserted on the model, because the model is where the claim is: the machine underneath
/// does not change at all. <c>CharRange</c> is <c>(char From, char To)</c>, so a graph over
/// kinds is a graph, and every analysis the compiler has runs over it without being told.
/// </remarks>
public sealed class LexicalSplitTests
{
	static RecognitionGraph Graph(string grammar) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!));

	const string Spaced =
		"""
		wordboundary = ['a'..'z']
		trivia = ' '*
		namespace Lexical
		{
			trivia = none
			Name = ['a'..'z'] & ['a'..'z']*
		}
		Reserved = "if" | "else"
		Id    = ?!Reserved & Lexical.Name
		Start = "if" & Id & "else" & Id
		parse Start
		""";

	/// <summary>A grammar with no trivia is not split: it is already where it belongs.</summary>
	[Fact]
	public void A_scannerless_grammar_is_not_split()
	{
		Assert.Null(LexicalSplit.Of(Graph("Start = 'a' & \"bc\"\nparse Start")));
	}

	/// <summary>What the rewrite leaves is a graph over kinds, and no trivia at all.</summary>
	/// <remarks>
	/// The seam had one job and the lexer has it now, so a place for whitespace between two
	/// kinds is a place nothing can arrive at. What is left in every body is a literal one
	/// character long, an element set over such characters, or a call to another rule.
	/// </remarks>
	[Fact]
	public void The_rewritten_graph_holds_kinds_and_no_trivia()
	{
		var split = LexicalSplit.Of(Graph(Spaced));

		Assert.NotNull(split);
		Assert.Empty(split.Blocked);
		Assert.Empty(split.Syntax.Trivia);

		foreach (var body in split.Syntax.Bodies.Values)
			foreach (var node in Descendants(body))
				if (node is Node.Literal(var text))
					Assert.Equal(1, text.Length);
	}

	/// <summary>
	/// A class stands for itself and for every word it would have matched.
	/// </summary>
	/// <remarks>
	/// This is the contextual-keyword case and it is not optional. `zone` is a word of
	/// SQL-92 and is not reserved, so `Identifier = ?!Reserved &amp; RegularIdentifier`
	/// takes it over characters; over kinds it arrives as that keyword's own kind and never
	/// reaches the class. Measured before it was believed: the probe refused
	/// <c>… / zone / …</c> until the union was written, and `zone` was the only word in the
	/// input that made it fail.
	/// </remarks>
	[Fact]
	public void A_class_stands_for_the_words_it_would_have_matched()
	{
		var split = LexicalSplit.Of(Graph(Spaced));

		Assert.NotNull(split);

		var id = split.Syntax.Rules.Single(rule => rule.Name == "Id");

		// `Name` accepts `if` and `else` as readily as anything else, so the crossing is a
		// set of three kinds — the class and the two words — and the `?!Reserved` in front
		// is what takes the two words back out again.
		var crossing = Descendants(split.Syntax.Bodies[id])
			.OfType<Node.Element>()
			.ToList();

		Assert.Contains(crossing, element => Count(element) == 3);
	}

	/// <summary>And `?!Reserved` becomes a lookahead over one range of kinds.</summary>
	/// <remarks>
	/// The whole argument in one assertion. Over characters that lookahead runs a choice of
	/// every reserved word at every identifier position; over kinds it is one element set,
	/// which the ordinary machinery then folds like any other one-item test.
	/// </remarks>
	[Fact]
	public void A_lookahead_over_a_word_list_becomes_a_lookahead_over_a_range()
	{
		var split = LexicalSplit.Of(Graph(Spaced));

		Assert.NotNull(split);

		var id = split.Syntax.Rules.Single(rule => rule.Name == "Id");

		var refused = Descendants(split.Syntax.Bodies[id])
			.OfType<Node.Lookahead>()
			.Single(one => !one.IsPositive);

		var over = Assert.IsType<Node.Element>(refused.Body);

		Assert.Equal(2, Count(over));
		Assert.Single(over.Ranges);

		// And `Reserved` is gone as a rule: it was never anything but those two kinds.
		Assert.DoesNotContain(split.Syntax.Rules, rule => rule.Name == "Reserved");
	}

	/// <summary>
	/// Two classes that accept the same string get a kind that says both.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The thing the first version got wrong. A token carries one kind, so a lexer forced to
	/// answer with one <em>pattern</em> would make every position that wanted the other stop
	/// reading — and both shipping grammars have the shape, so both were refused for having
	/// nothing wrong with them.
	/// </para>
	/// <para>
	/// A kind is a set. <c>10</c> is matched by <c>Digits</c> and by <c>Number</c> at once
	/// and its kind holds both, so <c>'(' &amp; Digits &amp; ')'</c> takes it and so does a
	/// value position; <c>1.5</c> is matched by <c>Number</c> alone and only the second takes
	/// it. Nothing is refused and nothing is widened.
	/// </para>
	/// </remarks>
	[Fact]
	public void Two_classes_that_accept_the_same_string_share_a_kind()
	{
		var split = LexicalSplit.Of(Graph(
			"""
			trivia = ' '*
			namespace Lexical
			{
				trivia = none
				Digits = ['0'..'9'] & ['0'..'9']*
				Number = Digits & ('.' & Digits)?
			}
			Start = Lexical.Digits & ',' & Lexical.Number
			parse Start
			"""));

		Assert.NotNull(split);
		Assert.Empty(split.Blocked);

		var inventory = split.Inventory;
		var digits    = Class(inventory, "Digits");
		var number    = Class(inventory, "Number");

		var shared = Assert.Single(
			inventory.Kinds,
			kind => kind.Matched.Contains(digits) && kind.Matched.Contains(number));

		// And each of them tests for that kind as well as for its own.
		Assert.Contains(Numbers(inventory, digits), one => one == shared.Number);
		Assert.Contains(Numbers(inventory, number), one => one == shared.Number);

		// The sharp part, and what an approximation cannot get right: `Digits` has no kind
		// of its own, because every string it accepts is also a `Number`. There is no
		// witness for that — it is a fact about the two languages — and only reading them
		// together finds it. `Number` does have one: `1.5`.
		Assert.DoesNotContain(inventory.Kinds, kind => kind.Matched is [var only] && only == digits);
		Assert.Contains(inventory.Kinds, kind => kind.Matched is [var only] && only == number);
	}

	/// <summary>A pattern that is not a regular language is refused, and said to be.</summary>
	/// <remarks>
	/// The patterns are read together or not at all, and reading them together is a
	/// subset construction over a Thompson machine — which has three shapes and a lookahead
	/// is none of them. Rather than approximate it the grammar keeps the character machine,
	/// which is correct and right there.
	/// </remarks>
	[Fact]
	public void A_pattern_that_is_not_regular_is_refused()
	{
		var split = LexicalSplit.Of(Graph(
			"""
			trivia = ' '*
			namespace Lexical
			{
				trivia = none
				Odd = 'a' & (?!"zz" & ['a'..'z'])* & 'b'
			}
			Start = Lexical.Odd & ',' & Lexical.Odd
			parse Start
			"""));

		Assert.Null(split);
	}

	/// <summary>A keyword's kind says it is an identifier too, which is what makes `zone` a name.</summary>
	/// <remarks>
	/// The same mechanism one level down, and the case that found it: `zone` is a word of
	/// SQL-92 and is not reserved, so `?!Reserved &amp; RegularIdentifier` takes it over
	/// characters. It reaches the syntactic machine as that keyword's kind, and only because
	/// the kind holds the class as well does the identifier position accept it.
	/// </remarks>
	[Fact]
	public void A_keyword_is_an_identifier_too()
	{
		var split = LexicalSplit.Of(Graph(Spaced));

		Assert.NotNull(split);

		var inventory = split.Inventory;
		var name      = Class(inventory, "Name");
		var word      = inventory.Patterns.OfType<TerminalInventory.Pattern.Word>()
			.Single(one => one.Text == "if");

		var of = Assert.Single(inventory.KindsOf(word));

		Assert.Equal(of.From, of.To);
		Assert.Contains(Numbers(inventory, name), one => one == of.From);
	}

	static TerminalInventory.Pattern Class(TerminalInventory inventory, string name) =>
		inventory.Patterns.OfType<TerminalInventory.Pattern.Class>().Single(one => one.Rule.Name == name);

	static IEnumerable<int> Numbers(TerminalInventory inventory, TerminalInventory.Pattern pattern) =>
		inventory.KindsOf(pattern).SelectMany(range => Enumerable.Range(range.From, range.Count));

	/// <summary>Every node of a body, the compiler's own walker being internal.</summary>
	static IEnumerable<Node> Descendants(Node node)
	{
		yield return node;

		foreach (var child in node.Children)
			foreach (var one in Descendants(child))
				yield return one;
	}

	static int Count(Node.Element element) =>
		element.Ranges.Sum(range => range.To - range.From + 1);
}
