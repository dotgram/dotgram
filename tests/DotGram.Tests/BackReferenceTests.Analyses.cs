using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the analyses answer for a binding, a reference and a count by a length: each asked of
/// a grammar small enough that the answer can be said in advance.
/// </summary>
/// <remarks>
/// <para>
/// A reference matches what its binding matched, so it may match nothing exactly where the
/// binding's body may, and it begins as that body begins. A count by a length may take no turn,
/// since the length may be zero. A binding reads what its body reads and nothing more. Each
/// analysis walks into the bodies of the binding and the count, so that what a rule called only
/// from inside one is followed by, and whether its reading can be given back, are known.
/// </para>
/// <para>
/// What the parser does with them is the business of the tests that run one; these hold the
/// answers the emitters are built on.
/// </para>
/// </remarks>
public sealed partial class BackReferenceTests
{
	// ── Nullability ─────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("R = $q: 'a'+ & $q & 'x'", false)]
	[InlineData("R = $q: 'a'* & $q & 'x'", true)]
	[InlineData("R = $q: ('a' | 'b'?) & $q & 'x'", true)]
	[InlineData("R = ($q: 'a'+ & 'x' | $q: 'b'* & 'y') & $q", true)]
	[InlineData("R = ($q: 'a'+ & 'x' | $q: 'b' & 'y') & $q", false)]
	public void A_reference_may_match_nothing_where_its_binding_may(string grammar, bool nullable)
	{
		var graph = Normalize(grammar + "\nparse R");
		var same  = Nodes<Node.Same>(BodyOf(graph, "R")).Single();

		Assert.Equal(nullable, FirstSets.Nullable(same, graph));
	}

	[Theory]
	[InlineData("R = $q: 'a'+ & 'b'{#q}", false)]
	[InlineData("R = $q: 'a'* & 'b'{#q}", true)]
	[InlineData("R = $q: 'a'+ & 'b'{0,#q}", true)]
	[InlineData("R = $q: 'a'+ & 'b'{2,#q}", false)]
	[InlineData("R = $q: 'a'+ & 'b'{#q,}", false)]
	public void A_count_by_a_length_may_take_no_turn_where_the_length_may_be_zero(string grammar, bool nullable)
	{
		var graph   = Normalize(grammar + "\nparse R");
		var counted = Nodes<Node.Counted>(BodyOf(graph, "R")).Single();

		Assert.Equal(nullable, FirstSets.Nullable(counted, graph));
	}

	[Theory]
	[InlineData("R = $q: 'a'* & 'x'", true)]
	[InlineData("R = $q: 'a'+ & 'x'", false)]
	public void A_binding_may_match_nothing_where_its_body_may(string grammar, bool nullable)
	{
		var graph = Normalize(grammar + "\nparse R");
		var bind  = Nodes<Node.Bind>(BodyOf(graph, "R")).Single();

		Assert.Equal(nullable, FirstSets.Nullable(bind, graph));
	}

	/// <summary>
	/// A repetition of a reference that may match nothing would not end, and is refused; one
	/// whose binding must have read something is a loop like any other.
	/// </summary>
	[Fact]
	public void A_repeated_reference_is_refused_only_where_it_may_match_nothing()
	{
		Assert.Contains(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a'* & $q* & 'b'\nparse R"));
		Assert.Contains(GrammarNormalizer.NullableRepetition, Errors("R = $q: ('a'?) & $q+ & 'b'\nparse R"));
		Assert.DoesNotContain(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a'+ & $q* & 'b'\nparse R"));
		Assert.DoesNotContain(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a' & $q* & 'b'\nparse R"));
	}

	/// <summary>
	/// Left recursion is found through a reference that may read nothing, as through any
	/// prefix that may, and not through one that must read something.
	/// </summary>
	[Fact]
	public void Left_recursion_is_found_through_a_reference_that_may_read_nothing()
	{
		Assert.Contains(GrammarNormalizer.LeftRecursion, Errors("R = $q: 'a'* & $q & S | 'y'\nS = R & 'z'\nparse R"));
		Assert.DoesNotContain(GrammarNormalizer.LeftRecursion, Errors("R = $q: 'a'+ & $q & S | 'y'\nS = R & 'z'\nparse R"));
		Assert.Contains(GrammarNormalizer.LeftRecursion, Errors("R = $q: 'a'* & 'b'{#q} & S | 'y'\nS = R & 'z'\nparse R"));
		Assert.DoesNotContain(GrammarNormalizer.LeftRecursion, Errors("R = $q: 'a'+ & 'b'{#q} & S | 'y'\nS = R & 'z'\nparse R"));
	}

	// ── First sets ──────────────────────────────────────────────────────────────

	[Fact]
	public void A_reference_begins_as_its_binding_does()
	{
		var graph = Normalize("R = $q: ['a'..'c']+ & $q & 'x'\nparse R");
		var same  = Nodes<Node.Same>(BodyOf(graph, "R")).Single();

		AssertFirst(graph, same, ('a', 'c'));
		AssertFirst(graph, new Node.Sequence([same, new Node.Literal("x")]), ('a', 'c'));
	}

	[Fact]
	public void A_reference_that_may_match_nothing_lets_what_follows_it_begin()
	{
		var graph = Normalize("R = $q: 'a'* & $q & 'x'\nparse R");
		var same  = Nodes<Node.Same>(BodyOf(graph, "R")).Single();

		AssertFirst(graph, same, ('a', 'a'));
		AssertFirst(graph, new Node.Sequence([same, new Node.Literal("x")]), ('a', 'a'), ('x', 'x'));
	}

	[Fact]
	public void A_reference_begins_as_every_body_its_binding_is_bound_to()
	{
		var graph = Normalize("R = ('<' & $q: 'a'+ | $q: 'b'+) & $q\nparse R");
		var same  = Nodes<Node.Same>(BodyOf(graph, "R")).Single();

		AssertFirst(graph, same, ('a', 'b'));
	}

	/// <summary>
	/// A binding whose body reads another binding is not followed further: what it begins with
	/// would lean on that binding's answer, and anything is never wrong.
	/// </summary>
	[Fact]
	public void A_reference_to_a_binding_of_a_reference_may_begin_with_anything()
	{
		var graph = Normalize("R = $p: 'a'+ & $q: $p & $q\nparse R");
		var same  = Nodes<Node.Same>(BodyOf(graph, "R")).Last();

		Assert.Equal("q", same.Binding.Name);
		Assert.True(FirstSets.Of(same, graph).Anything);
	}

	[Fact]
	public void A_binding_and_a_count_begin_as_their_bodies_do()
	{
		var graph   = Normalize("R = $q: 'a'+ & ['b'..'d']{#q} & 'x'\nparse R");
		var body    = BodyOf(graph, "R");
		var bind    = Nodes<Node.Bind>(body).Single();
		var counted = Nodes<Node.Counted>(body).Single();

		AssertFirst(graph, bind, ('a', 'a'));
		AssertFirst(graph, counted, ('b', 'd'));
	}

	/// <summary>
	/// Alternatives are told apart by what they begin with only where neither may match nothing,
	/// and a reference that may is not one of those.
	/// </summary>
	[Theory]
	[InlineData("R = $q: 'a'+ & ($q | 'b')", true)]
	[InlineData("R = $q: 'a'+ & ($q | 'a')", false)]
	[InlineData("R = $q: 'a'* & ($q | 'b')", false)]
	public void Alternatives_led_by_a_reference_are_told_apart_only_where_they_are_apart(string grammar, bool apart)
	{
		var graph  = Normalize(grammar + "\nparse R");
		var choice = Nodes<Node.Choice>(BodyOf(graph, "R")).Single();

		Assert.Equal(apart, Determinism.Distinguishable(choice.Nodes, graph));
	}

	// ── What follows, what is given back ───────────────────────────────────────

	/// <summary>
	/// A rule called only inside a binding or a count is followed by what follows there. Left
	/// with nothing, every proof that it is apart from what follows it would hold of nothing.
	/// </summary>
	[Theory]
	[InlineData("R = $q: S & 'x' & $q\nS = 'a'+")]
	[InlineData("R = $q: 'b'+ & S{#q} & 'x'\nS = 'a'")]
	public void A_rule_called_inside_a_binding_or_a_count_is_followed(string grammar)
	{
		var graph  = Normalize(grammar + "\nparse R");
		var follow = FollowSets.Of(graph)[Rule(graph, "S")];

		Assert.False(follow.Plain.Nothing);
		Assert.True(follow.Plain.Anything || follow.Plain.Overlaps(FirstSets.First.Chars([new CharRange('x', 'x')])));
	}

	/// <summary>
	/// What is read inside a binding can be put back where something after it fails, and a turn
	/// of a count where the turns after it do: a carrier that built its value early would keep a
	/// value of a reading that is gone.
	/// </summary>
	[Theory]
	[InlineData("R = $q: S & 'x' & $q\nS = 'a'+")]
	[InlineData("R = $q: 'b'+ & S{#q,} & 'x'\nS = 'a'")]
	public void A_reading_inside_a_binding_or_a_count_may_not_stand(string grammar)
	{
		var graph = Normalize(grammar + "\nparse R");

		Assert.False(Replay.Of(graph).Stands(Rule(graph, "S")));
	}

	[Theory]
	[InlineData("R = $q: S & 'x' & $q\nS = 'a'+ => @(1)")]
	[InlineData("R = $q: 'b'+ & S{#q,} & 'x'\nS = 'a' => @(1)")]
	public void A_call_inside_a_binding_or_a_count_is_a_site(string grammar)
	{
		var graph  = Normalize(grammar + "\nparse R");
		var s      = Rule(graph, "S");
		var report = Commit.Of(graph, Replay.Of(graph));

		Assert.Contains(report.Sites, site => site.Node is Node.Call(var called, _) && ReferenceEquals(called, s));
	}

	[Fact]
	public void A_binding_leaves_the_way_back_its_body_leaves()
	{
		var graph   = Normalize("R = $q: '\"'+ & 'x'{#q} & { $r: 'y' } & $q & $r\nparse R");
		var body    = BodyOf(graph, "R");
		var binds   = Nodes<Node.Bind>(body);
		var none    = new Dictionary<RuleSymbol, bool>();

		Assert.True(Doors.LeavesOne(binds.Single(one => one.Binding.Name == "q"), none));
		Assert.True(Doors.LeavesOne(Nodes<Node.Counted>(body).Single(), none));
		Assert.False(Doors.LeavesOne(Nodes<Node.Same>(body).First(), none));
	}

	[Fact]
	public void A_reference_may_take_as_many_lines_as_anything()
	{
		var graph = Normalize("R = $q: 'a'+ & $q\nparse R");
		var same  = Nodes<Node.Same>(BodyOf(graph, "R")).Single();

		Assert.Equal(LineExtent.Beyond, Retention.Extent(same, new Dictionary<RuleSymbol, LineExtent>(), []));
	}

	// ── The passes that rebuild a body ──────────────────────────────────────────

	/// <summary>
	/// The value a sequence rule collects is collected inside a binding and from a count as from
	/// anywhere else: a binding makes no member, and its body's operands are the rule's.
	/// </summary>
	/// <remarks>
	/// Asked of the captures the collection writes; the slots they are given are the capture
	/// layout's, which numbers what it finds inside a binding and a count.
	/// </remarks>
	[Fact]
	public void A_sequence_collects_inside_a_binding_and_from_a_count()
	{
		var graph = Normalize(
			"""
			R : @Item[] = $q: Item & Item{#q}
			Item : @Item = ['a'..'z'] => @(new Item())
			parse R
			""");

		Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var body     = BodyOf(graph, "R");
		var captures = Nodes<Node.Capture>(body);

		Assert.Equal(2, captures.Length);
		Assert.IsType<Node.Capture>(Nodes<Node.Bind>(body).Single().Body);
		Assert.IsType<Node.Capture>(Nodes<Node.Counted>(body).Single().Body);
	}

	/// <summary>
	/// Between the turns of a count, as between the turns of any repetition of a list's element,
	/// the trivia is read: <c>Item{#q}</c> reads the same spacing <c>Item*</c> does.
	/// </summary>
	[Fact]
	public void A_count_of_a_list_element_is_spaced_between_its_turns()
	{
		var graph = Normalize(
			"""
			trivia = { ' '* }
			R = $q: 'x'+ & Item{#q}
			Item : @Item = ['a'..'z'] => @(new Item())
			parse R
			""");

		var counted = Nodes<Node.Counted>(BodyOf(graph, "R")).Single();
		var turn    = Assert.IsType<Node.Sequence>(counted.Body);

		Assert.Equal("trivia", Assert.IsType<Node.Call>(turn.Nodes[0]).Rule.Name);
	}

	/// <summary>
	/// The trivia woven into a rule trivia reaches is taken out again, inside a binding too: a
	/// long bracket read from trivia does not read trivia between its own equals signs.
	/// </summary>
	[Fact]
	public void Trivia_is_taken_out_of_a_binding_in_a_rule_trivia_reaches()
	{
		var graph = Normalize(
			"""
			trivia = { (' ' | Long)* }
			Long = '[' & $q: ('=' & '='*) & '[' & $q & ']'
			R = 'a'+
			parse R
			""");

		var trivia = Rule(graph, "trivia");
		var bind   = Nodes<Node.Bind>(BodyOf(graph, "Long")).Single();

		Assert.DoesNotContain(Walk(bind), node => node is Node.Call(var called, _) && ReferenceEquals(called, trivia));
	}

	/// <summary>
	/// A text capture under a count by a length is one capture of the run, as it is under any
	/// repetition without an upper bound: the turns are contiguous, so their text joined is the
	/// run's.
	/// </summary>
	[Fact]
	public void A_text_capture_is_lifted_out_of_a_count_without_an_upper_bound()
	{
		var body = Body("R = $q: 'b'+ & t: 'a'{#q,}\nparse R");

		var capture = Nodes<Node.Capture>(body).Single();

		Assert.IsType<Node.Counted>(capture.Body);
	}

	/// <summary>
	/// And under one that may take no turn and has an end: a run all the same, whose text is
	/// <c>""</c> when it took none, which the capture around it records.
	/// </summary>
	[Fact]
	public void A_text_capture_is_lifted_out_of_a_count_that_may_take_no_turn_and_has_an_end()
	{
		var body = Body("R = $q: 'b'+ & t: 'a'{0,#q}\nparse R");

		var capture = Nodes<Node.Capture>(body).Single();

		Assert.IsType<Node.Counted>(capture.Body);
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	static RuleSymbol Rule(RecognitionGraph graph, string name)
	{
		return graph.Rules.First(rule => rule.Name == name);
	}

	static Node BodyOf(RecognitionGraph graph, string name)
	{
		Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return graph.Bodies[Rule(graph, name)];
	}

	static void AssertFirst(RecognitionGraph graph, Node node, params (char From, char To)[] ranges)
	{
		var expected = FirstSets.First.Chars(ranges.Select(static one => new CharRange(one.From, one.To)));
		var actual   = FirstSets.Of(node, graph);

		Assert.True(FirstSets.Same(expected, actual), $"{node} begins with {actual}, not {expected}");
	}
}
