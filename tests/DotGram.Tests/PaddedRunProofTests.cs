using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// When a run up to a padded delimiter need never give back to a continuation that reads padding
/// first (<see cref="Determinism.SettlesPaddedRun"/>), and every condition the proof rests on,
/// each shown refusing it where it does not hold.
/// </summary>
/// <remarks>
/// The proof: a turn of <c>(?!D &amp; any)</c> begins only where <c>D = P* &amp; S &amp; Q*</c>
/// did not match, and a continuation that reads characters of <c>P</c> and then one of <c>S</c>
/// would have been <c>D</c> matching there. Or it reads padding to the end of the input, where the
/// longest reading already stands, and only a grammar with nothing that could tell the two splits
/// apart is let through. <see cref="PaddingAfterRunCountTests"/> holds what it buys.
/// </remarks>
public sealed class PaddedRunProofTests
{
	[Theory]
	[InlineData("' '* & ('|' & ' '* | eof)")]
	[InlineData("' '* & (Separator | eof)")]
	[InlineData("' '* & '|'")]
	[InlineData("' '* & \"|x\"")]
	[InlineData("' '* & eof")]
	[InlineData("' '* & ' '* & '|'")]
	[InlineData("Space* & '|'")]
	[InlineData("' '* & Bar")]
	[InlineData("' '* & (Bar | eof)")]
	public void A_continuation_that_pads_and_then_stops_settles_the_run(string continuation)
	{
		Assert.True(Settles(continuation));
	}

	[Theory]
	// The padding may be skipped over, all of it: `' '+` must read one, which the run may have taken.
	[InlineData("' '+ & ('|' | eof)", "")]
	// Padding wider than the delimiter's: a tab the delimiter does not pad with.
	[InlineData("[' ' | '\\t']* & '|'", "")]
	// Something that consumes nothing but can refuse, between the padding and the stop.
	[InlineData("' '* & ?='|' & '|'", "")]
	[InlineData("' '* & when @(true) & '|'", "")]
	[InlineData("' '* & Guarded", "Guarded = when @(true) & '|'")]
	[InlineData("' '* & (when @(true) & '|')", "")]
	[InlineData("' '* & (?!'x' & '|' | eof)", "")]
	[InlineData("when @(true) & ' '* & '|'", "")]
	// What follows the padding may read nothing, or begins with something other than the stop.
	[InlineData("' '* & Optional & '|'", "Optional = 'x'?")]
	[InlineData("' '* & 'x'", "")]
	[InlineData("' '* & ('|' | 'x')", "")]
	// The padding meets the end, and a guard, a captured look or a recognizer is in the grammar.
	[InlineData("' '* & ('|' | eof)", "Other = 'a' & when @(true)")]
	[InlineData("' '* & eof", "Other = n: ?='a' & 'a'")]
	public void A_continuation_the_proof_does_not_hold_for_keeps_the_way_back(string continuation, string more)
	{
		Assert.False(Settles(continuation, more));
	}

	/// <summary>
	/// The stop and the padding must be apart: a delimiter whose stop is also padding is not one
	/// the run's scan recognizes, and not one the proof reasons about.
	/// </summary>
	[Fact]
	public void A_delimiter_whose_stop_pads_is_not_one()
	{
		Assert.False(Settles("' '* & '|'", separator: "' '* & [' ' | '|']"));
	}

	/// <summary>Where the padding cannot meet the end, a guard elsewhere changes nothing.</summary>
	[Fact]
	public void A_guard_elsewhere_matters_only_where_the_padding_meets_the_end()
	{
		Assert.True(Settles("' '* & '|'", "Other = 'a' & when @(true)"));
	}

	static bool Settles(string continuation, string more = "", string separator = "' '* & '|' & ' '*")
	{
		var graph = GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(
						"Separator = " + separator + "\n" +
						"Space = ' '\n" +
						"Bar = '|' & 'x'\n" +
						"Text = (?!Separator & any)+\n" +
						(more.Length > 0 ? more + "\n" : "") +
						// A rule the grammar defines is kept only where a publication reaches it.
						"Start = Text & " + continuation + (more.StartsWith("Other", System.StringComparison.Ordinal) ? " | Other" : "") + "\n" +
						"parse Start", RoslynCSharpScanner.Instance)).File));

		var text   = graph.Rules.First(one => one.Name == "Text");
		var repeat = Assert.IsType<Node.Repeat>(graph.Bodies[text]);

		return Determinism.NeverGivesBack(repeat, FollowSets.Of(graph)[text], graph, null);
	}
}
