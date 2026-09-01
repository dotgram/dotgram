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
	/// Two classes that accept the same string are refused, with the string as the witness.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A token carries one kind, so where two classes both accept it the lexer must choose
	/// and every position that wanted the other stops reading. Both shipping grammars have
	/// the shape: SQL-92's <c>Digits</c> against its <c>UnsignedNumericLiteral</c>, and
	/// <c>ExpressionLanguage</c>'s <c>TypeName</c> — which is <c>Word &amp; ('.' &amp;
	/// Word)*</c> — against its <c>Word</c>.
	/// </para>
	/// <para>
	/// Refused rather than resolved, because resolving means choosing: making them one kind
	/// widens the language and picking one narrows it, and neither is the compiler's to
	/// decide.
	/// </para>
	/// </remarks>
	[Fact]
	public void Two_classes_that_accept_the_same_string_are_refused()
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

		Assert.Contains(
			split.Blocked,
			reason => reason.Contains("Digits and Number both accept \"0\""));
	}

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
