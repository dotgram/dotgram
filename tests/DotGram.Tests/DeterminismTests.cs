using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Whether a construct has at most one match where it stands.
/// </summary>
/// <remarks>
/// The proof left-factoring rests on, and the one the emitter asks before deciding whether a
/// repetition needs a way back written down. Asked here directly, because the two callers
/// reach it through decisions of their own and a proof is worth being able to interrogate.
/// </remarks>
public sealed class DeterminismTests
{
	/// <summary>A rule that reaches itself can still have one reading.</summary>
	/// <remarks>
	/// It could not before: a rule met on the way down was answered no, so nothing recursive
	/// was ever determinate and nothing containing one could be. The assumption is the other
	/// way now, and discharged by the rest of the walk.
	/// </remarks>
	[Fact]
	public void A_rule_that_reaches_itself_can_be_determinate()
	{
		var graph = Graph("Item = '(' & Item & ')' | ['a'..'z']\nStart = Item & ';'");

		Assert.True(Determinism.Of(Body(graph, "Item"), Ends(';'), graph, null));
	}

	/// <summary>And one whose choice a character cannot settle is not.</summary>
	[Fact]
	public void And_one_whose_alternatives_can_begin_alike_is_not()
	{
		var graph = Graph("Item = 'a' & Item | 'a'\nStart = Item & ';'");

		Assert.False(Determinism.Of(Body(graph, "Item"), Ends(';'), graph, null));
	}

	/// <summary>
	/// How wide a first set is says nothing about whether it decides anything.
	/// </summary>
	/// <remarks>
	/// The cap is a fact about what a rendering will spell out — a Unicode category is a few
	/// hundred ranges, and a dispatch written over them is a page of comparisons where the
	/// alternative's own test is one call. It used to sit inside the proof, so a choice that
	/// one character plainly settles was called undecidable because writing the decision down
	/// would have been long.
	/// </remarks>
	[Fact]
	public void A_category_beside_a_range_is_told_apart_however_long_the_answer_is()
	{
		var graph = Graph(
			"Letter = [\\p{L}]\n" +
			"Digit  = ['0'..'9']\n" +
			"Item   = Letter | Digit\n" +
			"Start  = Item & ';'");
		var body  = (Node.Choice)Body(graph, "Item");

		Assert.True(Determinism.Distinguishable(body.Nodes, graph));

		// And the caller that has to write it out still declines, which is the whole reason
		// the two are separate questions.
		Assert.False(Determinism.Distinguishable(body.Nodes, graph, 8));
	}

	static FollowSets.Continuation Ends(char c)
	{
		var only = FirstSets.First.Chars([new CharRange(c, c)]);

		return new FollowSets.Continuation(only, only);
	}

	static Node Body(RecognitionGraph graph, string rule) =>
		graph.Bodies[graph.Rules.First(one => one.Name == rule)];

	static RecognitionGraph Graph(string text) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(text + "\nparse Start", RoslynCSharpScanner.Instance)).File));
}
