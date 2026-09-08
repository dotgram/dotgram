using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
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

	// ── Nullability ───────────────────────────────────────────────────────────────────

	/// <summary>A repetition is nullable when its body is, not only when it may be skipped.</summary>
	/// <remarks>
	/// This used to be answered in two places, and this is the case they disagreed on: the
	/// normalizer looked inside the body, the copy here stopped at <c>min == 0</c>. A
	/// repetition of exactly one is the shape that separates them — it is not refused the way
	/// a nullable body under <c>*</c> or <c>+</c> is, since it cannot spin.
	/// </remarks>
	[Fact]
	public void A_repetition_of_exactly_one_is_nullable_when_its_body_is()
	{
		const string Grammar = "A = B{1,1}\nB = none\nStart = A";

		var graph = Graph(Grammar);
		var body  = graph.Bodies[graph.Rules.First(rule => rule.Name == "A")];

		// If normalization ever folds this away the test stops testing what it names.
		Assert.IsType<Node.Repeat>(body);
		Assert.Equal(1, ((Node.Repeat)body).Min);

		Assert.True(FirstSets.Nullable(body, graph));
	}

	/// <summary>A lookbehind consumes nothing.</summary>
	/// <remarks>
	/// The other half of the disagreement, the other way round: the normalizer's copy had no
	/// case for it and fell through to "consumes", which would have made a sequence starting
	/// with one opaque to the left-recursion walk. Nothing writes a lookbehind directly — it
	/// comes from lowering a word lexeme, where a consuming literal follows it — so this asks
	/// the node, which is what the walk sees.
	/// </remarks>
	[Fact]
	public void A_lookbehind_is_nullable() =>
		Assert.True(
			FirstSets.Nullable(
				new Node.Behind(new Node.Element(false, [new CharRange('a', 'z')], [], [])),
				_ => false));

	static RecognitionGraph Graph(string text) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));

	static Node Body(string text)
	{
		var graph = Graph(text);

		return graph.Bodies[graph.Rules.First(rule => rule.Name == "A")];
	}
	// ── An optional that takes what follows it (GRAM5009) ────────────────────────

	const string Swallowing =
		"""
		wordboundary = ['a'..'z' | '_']
		trivia = { ' '* }
		namespace Lexical
		{
			trivia = none
			Name = ['a'..'z' | '_'] & ['a'..'z' | '_']*
		}
		Alias = Lexical.Name
		Start = "select" & Lexical.Name & Alias? & "into" & Lexical.Name
		parse Start
		""";

	/// <summary>
	/// An optional that can be done after one token, where that token is the one the next
	/// clause needed.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Over characters this is settled by backtracking and the parse succeeds by the second
	/// reading, which is `GRAM5002` and not a defect. Over kinds a rule's answer stands
	/// (§4): the alias takes `into`, `into` is not there any more, and the rule fails one
	/// token past what it ate — which is the least helpful place a parse can stop.
	/// </para>
	/// <para>
	/// Written because the T-SQL grammar paid for this five times before anything said it
	/// out loud: a unit that ate an `ACTION`, a routine option that ate its own `AS`, a
	/// backup file that ate the `TO` before its device, a correlation name that ate a
	/// `PIVOT`, a database setting that ate the `WITH`. Each was found by reading a corpus
	/// and noticing the parse stopped one word past what it should have.
	/// </para>
	/// </remarks>
	[Fact]
	public void An_optional_that_can_take_what_follows_it_is_said_out_loud()
	{
		var reported = Split(Swallowing);

		Assert.Contains(reported, one => one.Id == FirstSets.Swallows);
	}

	/// <summary>And not said where the optional cannot finish on that token.</summary>
	/// <remarks>
	/// The line between a warning worth reading and sixty of them. `Class?` begins with a
	/// name and needs a `::` after it, so it never takes a bare one: it fails, gives nothing
	/// back, and the clause after it reads. The question is what the optional can match in
	/// one token, not what it can begin with.
	/// </remarks>
	[Fact]
	public void And_not_where_it_would_have_to_read_two()
	{
		const string grammar =
			"""
			wordboundary = ['a'..'z' | '_']
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Name = ['a'..'z' | '_'] & ['a'..'z' | '_']*
			}
			Class = Lexical.Name & "::"
			Start = "select" & Class? & Lexical.Name & "into" & Lexical.Name
			parse Start
			""";

		Assert.DoesNotContain(Split(grammar), one => one.Id == FirstSets.Swallows);
	}

	/// <summary>And a lookahead in front of the optional is what settles it.</summary>
	[Fact]
	public void And_a_lookahead_in_front_of_it_settles_it()
	{
		const string grammar =
			"""
			wordboundary = ['a'..'z' | '_']
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Name = ['a'..'z' | '_'] & ['a'..'z' | '_']*
			}
			Alias = ?!"into" & Lexical.Name
			Start = "select" & Lexical.Name & Alias? & "into" & Lexical.Name
			parse Start
			""";

		Assert.DoesNotContain(Split(grammar), one => one.Id == FirstSets.Swallows);
	}

	/// <summary>What a grammar cut in two is told about itself.</summary>
	static IReadOnlyList<GramDiagnostic> Split(string grammar) =>
		GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Lexical       = true,
			}).Diagnostics;

}
