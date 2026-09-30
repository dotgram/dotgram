using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
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
	/// <para>
	/// The proof is over the sets themselves, which are exact: a Unicode category is a few
	/// hundred ranges and every one of them is known. What used to sit inside the proof was a
	/// fact about the rendering — a dispatch written over a few hundred ranges is a page of
	/// comparisons — so a choice that one character plainly settles was called undecidable
	/// because writing the decision down would have been long.
	/// </para>
	/// <para>
	/// It is not written down that way any more: a set too wide to read is held as its bounds
	/// and searched, which is one call however wide it is. The rendering no longer declines,
	/// so the proof no longer has to be asked how long its answer would be.
	/// </para>
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
	}

	/// <summary>
	/// A look that refused <c>'s'</c> did not refuse <c>'s'i</c>: a run up to it may have taken the
	/// <c>S</c> the continuation reads, and has to give it back.
	/// </summary>
	[Fact]
	public void A_look_for_a_literal_does_not_refuse_it_read_without_regard_to_case()
	{
		const string text = "Text = (?!'s' & any)+\nStart = Text & ";

		Assert.True(Settles(Graph(text + "'s'")));
		Assert.False(Settles(Graph(text + "'s'i")));
		Assert.True(Settles(Graph("Text = (?!'s'i & any)+\nStart = Text & 's'i")));

		static bool Settles(RecognitionGraph graph)
		{
			var text = graph.Rules.First(one => one.Name == "Text");

			return Determinism.NeverGivesBack((Node.Repeat)graph.Bodies[text], FollowSets.Of(graph)[text], graph, null);
		}
	}

	/// <summary>And the reader answers as the engine does, which it did not while it thought so.</summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_run_before_a_literal_read_without_regard_to_case_gives_back_to_it(bool direct)
	{
		var result = GramCompiler.Compile(
			"Text = (?!'s' & any)+\nStart : @int = value: Text & 's'i => @(value.Length)\nparse Start",
			new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance, Direct = direct });

		EmittedCode.Quiet(result.Diagnostics);

		var parser = EmittedCode.Compile(Assert.Single(result.Sources).Text);

		Assert.Equal(1, EmittedCode.Match(parser, "Grammar", "TryParseStart", "aS").Value);
		Assert.Equal(2, EmittedCode.Match(parser, "Grammar", "TryParseStart", "aSs").Value);
	}

	static FollowSets.Continuation Ends(char c)
	{
		var only = FirstSets.First.Chars([new CharRange(c, c)]);

		return new FollowSets.Continuation(only, only);
	}

	static Node Body(RecognitionGraph graph, string rule)
	{
		return graph.Bodies[graph.Rules.First(one => one.Name == rule)];
	}

	static RecognitionGraph Graph(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(text + "\nparse Start", RoslynCSharpScanner.Instance)).File));
	}
}
