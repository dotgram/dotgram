using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Which readings a sibling or a repetition can replace, and which only a failed parse puts
/// back: the question a carrier that builds where it reads has to ask (Q7.1).
/// </summary>
/// <remarks>
/// A reading replaced by something else is <see cref="Replay.Because.Follows"/>, and keeps its
/// grammar on the tape; one put back only where the whole parse fails is <see
/// cref="Replay.Because.Losing"/>, and does not. Each control holds both answers side by side,
/// so that a report calling everything replaced — which is always safe — fails here.
/// </remarks>
public sealed class ReplayTests
{
	static Replay.Report Report(string grammar)
	{
		return Replay.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));
	}

	static Replay.Because Of(Replay.Report report, string rule)
	{
		return report.Rules.Single(pair => pair.Key.Name == rule).Value;
	}

	/// <summary>
	/// A sibling replaces an alternative only where it can begin where the alternative began.
	/// <c>'a' N 'x'</c> has <c>'a' 'y'</c> after it; <c>'b' M 'z'</c> has nothing after it that
	/// begins with <c>'b'</c>, so where <c>'z'</c> does not come the choice fails and <c>M</c>
	/// goes with the parse.
	/// </summary>
	[Fact]
	public void A_sibling_replaces_only_an_alternative_it_can_begin_where()
	{
		var report = Report(
			"""
			Start : @string = 'a' & n: N & 'x' => @(n) | 'a' & 'y' => @("y") | 'b' & m: M & 'z' => @(m) | 'c' => @("c")
			N : @string = t: 'n' => @(t)
			M : @string = t: 'm' => @(t)
			parse Start
			""");

		Assert.Equal(Replay.Because.Follows, Of(report, "N"));
		Assert.Equal(Replay.Because.Losing,  Of(report, "M"));
	}

	/// <summary>
	/// A rule that reads nothing can still refuse: <c>eof</c> where the input goes on. After it,
	/// the reading before it is replaced by the sibling that begins alike; after <c>none</c>,
	/// which never refuses, it is not replaced there at all. Matching the empty input was taken
	/// for never refusing, and <c>Held</c> was built where it was read and then replaced.
	/// </summary>
	[Theory]
	[InlineData("eof", true)]
	[InlineData("none", false)]
	public void A_rule_that_reads_nothing_and_refuses_puts_back_what_came_before(string after, bool replaced)
	{
		var report = Report(
			$$"""
			using Std;
			Start : @string = h: Held & {{after}} => @(h) | g: Given & 'x' => @(g)
			Held  : @string = t: 'a' => @(t)
			Given : @string = t: 'a' => @(t)
			parse Start
			""");

		Assert.Equal(replaced, Of(report, "Held") == Replay.Because.Follows);
	}

	/// <summary>
	/// A later alternative that may read nothing can begin with whatever follows the choice:
	/// the same alternative is replaced where what follows begins like it, and not where it
	/// does not.
	/// </summary>
	[Theory]
	[InlineData("'q'", Replay.Because.Losing)]
	[InlineData("'b'", Replay.Because.Follows)]
	public void A_sibling_that_reads_nothing_begins_with_what_follows(string after, Replay.Because expected)
	{
		var report = Report(
			$$"""
			Start : @string = x: X & {{after}} => @(x)
			X : @string = 'b' & m: M & 'z' => @(m) | none => @("")
			M : @string = t: 'm' => @(t)
			parse Start
			""");

		Assert.Equal(expected, Of(report, "M"));
	}

	/// <summary>
	/// A failed turn is replaced by what follows the repetition only where that can begin
	/// where the turn began: <c>('t' V 'd')? 'u'</c> goes on to fail at <c>'u'</c> when
	/// <c>'d'</c> does not come, and <c>('t' V 'd')? 't' 'q'</c> reads the <c>'t'</c> again.
	/// </summary>
	[Theory]
	[InlineData("'u'", Replay.Because.Losing)]
	[InlineData("'t' & 'q'", Replay.Because.Follows)]
	public void A_failed_turn_is_replaced_only_by_what_can_begin_where_it_did(string after, Replay.Because expected)
	{
		var report = Report(
			$$"""
			Start : @string = ('t' & v: V & 'd')? & {{after}} => @(v ?? "")
			V : @string = t: 'v' => @(t)
			parse Start
			""");

		Assert.Equal(expected, Of(report, "V"));
	}

	/// <summary>
	/// Where the grammar reads trivia between its parts, a turn and what follows the repetition
	/// both begin with it, which tells them apart no more than it did before; what each reads
	/// past it does. <c>(S ';')* '.'</c> cannot go on where a turn failed, and <c>(S ';')* 'a'
	/// 'z'</c> can.
	/// </summary>
	[Theory]
	[InlineData("'.'", Replay.Because.Losing)]
	[InlineData("'a' & 'z'", Replay.Because.Follows)]
	public void Past_the_seam_a_turn_is_told_from_what_follows(string after, Replay.Because expected)
	{
		var report = Report(
			$$"""
			trivia = ' '*
			File : @string = (s: S & ';')* & {{after}} => @("")
			S : @string = t: 'a' => @(t)
			parse File
			""");

		Assert.Equal(expected, Of(report, "S"));
	}

	/// <summary>
	/// And what holds the choice still answers for it: the same alternative, alone at its
	/// token, is replaced where the choice itself stands in an alternative that has a sibling
	/// beginning the same way.
	/// </summary>
	[Fact]
	public void What_holds_the_choice_still_answers_for_it()
	{
		var report = Report(
			"""
			Start : @string = x: X & 'e' => @(x) | 'b' & 'w' => @("w")
			X : @string = 'b' & m: M & 'z' => @(m) | 'c' => @("c")
			M : @string = t: 'm' => @(t)
			parse Start
			""");

		Assert.Equal(Replay.Because.Follows, Of(report, "X"));
		Assert.False(report.Keeps(report.Rules.Keys.Single(rule => rule.Name == "M")));
	}
}
