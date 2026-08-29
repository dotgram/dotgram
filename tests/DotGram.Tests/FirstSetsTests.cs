using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a construct can begin with, asked directly.
/// </summary>
/// <remarks>
/// The sets feed every follow-set and every possessiveness decision, so the properties
/// here are the ones those lean on: normalization (exactness of <c>Covers</c>, and with
/// it the follow fixed point stopping), and the three leaves that used to answer
/// "anything" — a category, a negation, a case-insensitive literal — answering with
/// characters.
/// </remarks>
public sealed class FirstSetsTests
{
	// ── The algebra ──────────────────────────────────────────────────────────────

	[Fact]
	public void Ranges_are_sorted_and_merged()
	{
		var chars = FirstSets.First.Chars(
			[new CharRange('d', 'f'), new CharRange('a', 'b'), new CharRange('c', 'c')]);

		// 'a'..'b' and 'c'..'c' are adjacent, and 'c' meets 'd'..'f': one range.
		Assert.Equal([new CharRange('a', 'f')], chars.Ranges);
	}

	[Fact]
	public void A_union_of_the_same_sets_is_the_same_list()
	{
		var one = FirstSets.First.Chars([new CharRange('a', 'z')]);
		var two = one.Or(FirstSets.First.Chars([new CharRange('b', 'c')]));

		// What the follow fixed point leans on: saying it again does not grow it.
		Assert.Equal(one.Ranges, two.Ranges);
	}

	[Theory]
	[InlineData('a', 'c', 'c', 'e', true)]
	[InlineData('a', 'c', 'd', 'e', false)]
	[InlineData('a', 'a', 'a', 'a', true)]
	public void Overlap_is_exact_at_the_edges(char aFrom, char aTo, char bFrom, char bTo, bool expected) =>
		Assert.Equal(
			expected,
			FirstSets.First.Chars([new CharRange(aFrom, aTo)])
				.Overlaps(FirstSets.First.Chars([new CharRange(bFrom, bTo)])));

	[Fact]
	public void Covers_sees_through_a_split_spelling()
	{
		// 'a'..'m' and 'n'..'z' normalize to 'a'..'z', which plainly covers 'k'..'p' —
		// the answer the unnormalized walk got wrong, and with it the fixed point.
		var whole = FirstSets.First.Chars([new CharRange('a', 'm'), new CharRange('n', 'z')]);

		Assert.True(whole.Covers(FirstSets.First.Chars([new CharRange('k', 'p')])));
	}

	// ── The leaves that used to be "anything" ────────────────────────────────────

	[Fact]
	public void A_unicode_category_is_characters_rather_than_anything()
	{
		var first = FirstSets.Of(Body("A = [\\p{Lu}]"), Graph("A = [\\p{Lu}]"));

		Assert.True(first.IsKnown);
		Assert.True(first.Overlaps(FirstSets.First.Chars([new CharRange('A', 'Z')])));
		Assert.False(first.Overlaps(FirstSets.First.Chars([new CharRange('a', 'z')])));
		Assert.False(first.Overlaps(FirstSets.First.Chars([new CharRange('0', '9')])));
	}

	[Fact]
	public void A_negated_set_is_the_complement_rather_than_anything()
	{
		var grammar = "A = [^ 'b'..'y']";
		var first   = FirstSets.Of(Body(grammar), Graph(grammar));

		Assert.True(first.IsKnown);
		Assert.True(first.Overlaps(FirstSets.First.Chars([new CharRange('a', 'a')])));
		Assert.True(first.Overlaps(FirstSets.First.Chars([new CharRange('z', 'z')])));
		Assert.False(first.Overlaps(FirstSets.First.Chars([new CharRange('b', 'y')])));
	}

	[Fact]
	public void A_case_insensitive_literal_is_its_foldings_rather_than_anything()
	{
		var grammar = "A = \"http\"i";
		var first   = FirstSets.Of(Body(grammar), Graph(grammar));

		Assert.True(first.IsKnown);
		Assert.True(first.Overlaps(FirstSets.First.Chars([new CharRange('h', 'h')])));
		Assert.True(first.Overlaps(FirstSets.First.Chars([new CharRange('H', 'H')])));
		Assert.False(first.Overlaps(FirstSets.First.Chars([new CharRange('i', 'i')])));
	}

	[Fact]
	public void A_csharp_predicate_is_still_anything()
	{
		// The one honest "anything" left: what it accepts is the host's knowledge.
		var grammar = "A = [@Allowed]";
		var first   = FirstSets.Of(Body(grammar), Graph(grammar));

		Assert.True(first.Anything);
	}

	// ── What the precision reaches ───────────────────────────────────────────────

	[Fact]
	public void An_identifier_no_longer_poisons_what_follows_it()
	{
		// The cascade this was built for: a rule headed by a category made every follow
		// set behind it "anything", and every possessiveness proof failed on arrival.
		var grammar =
			"Start = Name & ';' & 'x'\n" +
			"Name = [\\p{L}]+";
		var graph  = Graph(grammar);
		var name   = graph.Rules.First(rule => rule.Name == "Name");
		var follow = FollowSets.Of(graph)[name].Plain;

		Assert.True(follow.IsKnown);
		Assert.True(follow.Overlaps(FirstSets.First.Chars([new CharRange(';', ';')])));
	}

	static RecognitionGraph Graph(string text) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));

	static Node Body(string text)
	{
		var graph = Graph(text);

		return graph.Bodies[graph.Rules.First(rule => rule.Name == "A")];
	}
}
