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
	[InlineData("' '* & '|'i")]
	// Padding with a bound is padding: it too reads none of it, or some, and nothing else.
	[InlineData("' '{0,2} & ('|' | eof)")]
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
	[InlineData("' '* & ('|' | eof)", "Other = switch @(1) { case 1: 'a' default: 'b' }")]
	[InlineData("' '* & ('|' | eof)", "Other = @Recognize")]
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

	/// <summary>
	/// A stop read without regard to case begins with every character its matcher folds to it:
	/// <c>'s'i</c> with <c>S</c>, <c>'μ'i</c> with <c>Μ</c> — and nothing more, not U+017F or
	/// U+00B5 (docs/syntax.md). A delimiter whose stop leaves one out does not refuse it, and the
	/// run may have to give back to it.
	/// </summary>
	[Theory]
	[InlineData("['s']",                             "'s'i", false)]
	[InlineData("['s' | 'S']",                       "'s'i", true)]
	[InlineData("['s' | 'S' | '\u017F']",           "'s'i", true)]
	[InlineData("['\u03BC']",                       "'\u03BC'i", false)]
	[InlineData("['\u03BC' | '\u039C']",           "'\u03BC'i", true)]
	[InlineData("['\u03BC' | '\u039C' | '\u00B5']", "'\u03BC'i", true)]
	public void A_stop_read_without_regard_to_case_is_every_character_it_folds_to(string stop, string literal, bool settles)
	{
		Assert.Equal(settles, Settles("' '* & " + literal, separator: "' '* & " + stop));
	}

	/// <summary>
	/// What the observer rows above add is the observer and nothing else: the same rule without it
	/// lets the proof through, and the recognizer is one the host writes.
	/// </summary>
	[Fact]
	public void The_observer_rows_differ_by_the_observer_alone()
	{
		Assert.True(Settles("' '* & ('|' | eof)", "Other = 'a' | 'b'"));
		Assert.Contains(
			Normalized("' '* & ('|' | eof)", "Other = @Recognize", "' '* & '|' & ' '*").Bodies.Values.SelectMany(Walk),
			static node => node is Node.External);
		Assert.Contains(
			Normalized("' '* & ('|' | eof)", "Other = switch @(1) { case 1: 'a' default: 'b' }", "' '* & '|' & ' '*").Bodies.Values.SelectMany(Walk),
			static node => node is Node.Choice { Selection: not null });

		static IEnumerable<Node> Walk(Node node)
		{
			return node.Children.SelectMany(Walk).Prepend(node);
		}
	}

	/// <summary>
	/// What the proof gives up, pinned: a refusal's message where every way past the end of the run
	/// fails without recording anything.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The give-backs a settled run no longer makes could only fail, and they failed before the end
	/// of the run, so what they recorded is behind wherever the reading went on to. Behind, unless
	/// nothing past the run recorded anything: here the continuation reads its <c>'|'</c> and then
	/// meets a look, which the reader refuses without a word. It used to say "Expected '|'." at 4,
	/// from a give-back into "ab  x" that met the <c>x</c>; with no give-back it has only the rule
	/// to name, from where the rule began. The engine records the look itself, further on, and says
	/// the same as it did. The parse is refused either way; the reason it gives is what moved, and
	/// only for a grammar whose tail refuses silently. Accepted as the existing lead proof accepts it.
	/// </para>
	/// <para>
	/// The end of the input written in place, <c>?!any</c>, is no longer such a tail: the reader
	/// records it as it records <c>eof</c>, and both renderings name it where the engine always did.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(true,  "?!any",       "Expected end of input.",       6)]
	[InlineData(true,  "?='q' & 'q'", "Input does not match 'Item'.", 0)]
	[InlineData(false, "?!any",       "Expected end of input.",       6)]
	[InlineData(false, "?='q' & 'q'", "Expected ?='q'.",              6)]
	public void A_tail_that_refuses_silently_leaves_the_rule_to_be_named(bool direct, string tail, string error, long position)
	{
		var result = GramCompiler.Compile(
			"Separator = ' '* & '|' & ' '*\nText = (?!Separator & any)+\nItem = t: Text & ' '* & '|' & " + tail + "\nparse Item",
			new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance, Direct = direct });

		EmittedCode.Quiet(result.Diagnostics);

		var match = EmittedCode.Match(EmittedCode.Compile(Assert.Single(result.Sources).Text), "Grammar", "TryParseItem", "ab  x|z");

		Assert.Equal((false, error, position), (match.IsSuccess, match.Error, match.Position));
	}

	/// <summary>Where the padding cannot meet the end, a guard elsewhere changes nothing.</summary>
	[Fact]
	public void A_guard_elsewhere_matters_only_where_the_padding_meets_the_end()
	{
		Assert.True(Settles("' '* & '|'", "Other = 'a' & when @(true)"));
	}

	static bool Settles(string continuation, string more = "", string separator = "' '* & '|' & ' '*")
	{
		var graph  = Normalized(continuation, more, separator);
		var text   = graph.Rules.First(one => one.Name == "Text");
		var repeat = Assert.IsType<Node.Repeat>(graph.Bodies[text]);

		return Determinism.NeverGivesBack(repeat, FollowSets.Of(graph)[text], graph, null);
	}

	static RecognitionGraph Normalized(string continuation, string more, string separator)
	{
		return GrammarNormalizer.Normalize(
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
	}
}
