using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The sets that tie for the furthest refusal are kept up to a cap, and past it the list ends with a mark
/// that every message words as "among others".
/// </summary>
/// <remarks>
/// A repetition inside a repetition refuses at one place by as many ways as it has of splitting its input,
/// and every way says the same sets there. A list that took each way's set held two to the n of them,
/// gigabytes on forty characters. Time stays what it was; the memory is bounded by the cap. Below the cap
/// every message is what it was: the expected strings here were taken from the generator before the cap
/// existed. The two largest cases are in the slow tests (<c>RefusalTiesCapTests</c>).
/// </remarks>
public sealed class RefusalTiesTests
{
	/// <summary>
	/// A refusal reached in two to the n ways keeps at most the cap and one mark, and says so only where it
	/// reached the cap. The engine records every way; the direct readers drop a set said twice in a row,
	/// so they reach it by shapes whose ways differ.
	/// </summary>
	[Theory]
	[InlineData(TieShapes.Once, 14, "Expected 'a' or 'b'.", "Expected 'a' or 'b'.")]
	[InlineData(TieShapes.Once, 15, "Expected 'a' or 'b', among others.", "Expected 'a' or 'b'.")]
	[InlineData(TieShapes.Once, 22, "Expected 'a' or 'b', among others.", "Expected 'a' or 'b'.")]
	[InlineData(TieShapes.Twice, 8, "Expected 'a', \"aa\" or 'b'.", "Expected ['a'], 'b' or \"aa\".")]
	[InlineData(TieShapes.Twice, 14, "Expected 'a', \"aa\" or 'b', among others.", "Expected ['a'], 'b' or \"aa\", among others.")]
	public void A_refusal_reached_in_many_ways_keeps_the_cap_at_most(string grammar, int length, string engine, string direct)
	{
		foreach (var (name, isDirect, carrier) in TieShapes.Renderings)
		{
			var parser = TieShapes.Grammar(grammar, isDirect, carrier);
			var first  = TieShapes.Ask(parser, "TryParseR0", new string('a', length) + "c");
			var again  = TieShapes.Ask(parser, "TryParseR0", new string('a', length) + "c");

			Assert.Equal(TieShapes.Cap, first.Cap);
			Assert.True(first.Tied <= first.Cap + 1, $"{name}: {first.Tied} sets tied");
			Assert.Equal(isDirect ? direct : engine, first.Said);
			Assert.Equal(first, again);
		}
	}

	/// <summary>
	/// Below the cap a choice whose widest group is a call refused over the ties says what it said before
	/// the cap existed. The call's refusal is taken back by the length the list had before it
	/// (Refuse_DotGram_Over): the list only grows by appends, so that length stays true.
	/// </summary>
	[Theory]
	[InlineData(8)]
	public void A_call_refused_over_ties_is_told_as_it_was_below_the_cap(int levels)
	{
		var want = new Dictionary<string, string>
		{
			["engine"]    = "Expected 'b', 'c', 'd', ['0'..'9' | 'e'..'z'], 'q', 'r' or ['0'..'9' | 'e'..'z' | '!'].",
			["tape"]      = "Expected 'b', 'c', 'd', 'q', ['0'..'9' | 'e'..'z'], 'r' or ['0'..'9' | 'e'..'z' | '!'].",
			["immediate"] = "Expected 'b', 'c', 'd', 'q', ['0'..'9' | 'e'..'z'], 'r' or ['0'..'9' | 'e'..'z' | '!'].",
		};

		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var parser = TieShapes.Grammar(TieShapes.Dispatch(levels), direct, carrier);
			var first  = TieShapes.Ask(parser, "TryParseStart", "aaa?");

			Assert.True(first.Tied <= first.Cap + 1, $"{name}: {first.Tied} sets tied");
			Assert.Equal(want[name], first.Said);
			Assert.Equal(first, TieShapes.Ask(parser, "TryParseStart", "aaa?"));
		}
	}


	/// <summary>
	/// A valued reading on the immediate carrier replays what it read once more to build it, and gives back
	/// the sets the replay recorded (Replayed_DotGram) by the length the list had; the engine records every
	/// way of the same refusal and is cut where it reaches the cap.
	/// </summary>
	[Theory]
	[InlineData(60, "Expected 'a'.", "Expected 'a'.")]
	[InlineData(100, "Expected 'a', among others.", "Expected 'a'.")]
	public void A_replayed_reading_inside_the_many_ways_is_cut_as_the_others_are(int length, string engine, string direct)
	{
		foreach (var (name, isDirect, carrier) in TieShapes.Renderings)
		{
			var parser = TieShapes.Grammar(TieShapes.Replay, isDirect, carrier);

			if (name == "immediate")
				Assert.Contains("Stood_DotGram", TieShapes.Source(TieShapes.Replay, isDirect, carrier));

			var first = TieShapes.Ask(parser, "TryParseT", new string('o', length) + "b");

			Assert.True(first.Tied <= first.Cap + 1, $"{name}: {first.Tied} sets tied");
			Assert.Equal(isDirect ? direct : engine, first.Said);
		}
	}

	/// <summary>
	/// What a recovering repetition says was wanted where its element stopped is what it said before the
	/// cap: the table is the generator's own answer taken before the cap existed, in the forms that read a
	/// recovery differently. The last rows reach the cap: the turn's message says so.
	/// </summary>
	[Theory]
	[InlineData(TieShapes.Rows, "aaa?;", "[short|short|short|Expected end of input or 'x' at 1:4.]")]
	[InlineData(TieShapes.Rows, "ax?;a;", "[long|Expected end of input or 'x' at 1:3.|short|Expected end of input or 'x' at 1:6.]")]
	[InlineData(TieShapes.Rows, "xx;ax", "[x|x|Expected end of input or 'x' at 1:3.|long]")]
	[InlineData(TieShapes.Pairs, "ab?;ac;ad;", "[i|Expected end of input or \"ac\" at 1:3.|j|Expected end of input or \"ac\" at 1:7.|Expected \"ab\" or \"ac\" at 1:9.]")]
	[InlineData(TieShapes.Pairs, "abab;a", "[i|i|Expected end of input or \"ac\" at 1:5.|Expected \"ab\" or \"ac\" at 1:7.]")]
	[InlineData(TieShapes.Nested, "[ab,ad,ac];[ab,a,d];", "[L|Expected end of input or '[' at 1:11.|L|Expected end of input or '[' at 1:20.]")]
	[InlineData(TieShapes.Nested, "[ab,ax];[", "[Expected ']', \"ab\", \"ac\" or 'd' at 1:10.|Expected ']', \"ab\", \"ac\" or 'd' at 1:10.]")]
	[InlineData(TieShapes.Cross, "aaaaaaaaaaaaaaa?;aaaaaaaaaaaaaaaaaaa?;", "[Expected 'a' or 'b', among others at 1:16.|Expected 'a' or 'b', among others at 1:37.]")]
	[InlineData(TieShapes.Cross, "aaaaaaaaaaaaaaa;b;", "[Expected 'a' or 'b', among others at 1:16.|r|Expected end of input or 'b' at 1:18.]")]
	[InlineData(TieShapes.Cross, "aaaaaaaaaaaaaaa?;b;", "[Expected 'a' or 'b', among others at 1:16.|r|Expected end of input or 'b' at 1:19.]")]
	public void What_a_recovery_says_was_wanted_is_told_in_every_form(string grammar, string input, string expected)
	{
		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var said = TieShapes.Ask(TieShapes.Grammar(grammar, direct, carrier), "TryParseStart", input).Said;

			Assert.True(said == expected, $"{name}: {said}");
		}
	}

	/// <summary>
	/// A turn that begins where the list already holds the cap and the mark has nothing of its own in it:
	/// it is told the mark, so the recovery says "among others" rather than nothing (Expecting_DotGram).
	/// </summary>
	[Fact]
	public void A_turn_that_begins_past_the_cap_is_told_the_list_was_cut()
	{
		var parser = TieShapes.Grammar(TieShapes.Cross, direct: false, CarrierKind.Auto);
		var failed = parser.GetNestedType("Failure", BindingFlags.NonPublic)!;
		var method = parser.GetMethod("Expecting_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!;
		var mark   = (string[])parser.GetField("Truncated_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
		var sets   = new List<string[]>();

		for (var one = 0; one < TieShapes.Cap; one++)
			sets.Add(["'x'"]);

		sets.Add(mark);

		object? Turn(int stood, int tied)
		{
			var failure = Activator.CreateInstance(failed)!;

			failed.GetField("Position")!.SetValue(failure, 5);
			failed.GetField("ExpectedMore")!.SetValue(failure, sets);

			return method.Invoke(null, [failure, 5, stood, tied]);
		}

		// Began before the cap: the sets the turn recorded, the mark among them.
		Assert.Equal(new[] { "'x'", "\u0001" }, ((string[])Turn(5, TieShapes.Cap - 1)!).Concat(Array.Empty<string>()).ToArray());

		// Began at the mark: the mark alone.
		Assert.Same(mark, Turn(5, TieShapes.Cap));

		// Began after the mark: the list holds nothing of the turn's, and says it was cut.
		Assert.Same(mark, Turn(5, TieShapes.Cap + 1));

		// A list that was not cut and holds nothing of the turn's says nothing, as it did.
		sets.RemoveRange(TieShapes.Cap, 1);

		Assert.Null(Turn(5, TieShapes.Cap));
	}

	/// <summary>
	/// A trace build ends the same way: the match says it, the sinks are handed the mark as the last set
	/// and a constant that names it, and the built-in sinks word it as "among others".
	/// </summary>
	[Fact]
	public void A_trace_build_words_the_cut_as_the_match_does()
	{
		var parser = TieShapes.Grammar(TieShapes.Once, direct: false, CarrierKind.Auto, trace: true);

		Assert.Equal("Expected 'a' or 'b', among others.", TieShapes.Ask(parser, "TryParseR0", new string('a', 16) + "c").Said);

		var trace = parser.GetNestedType("GramTrace")!;

		Assert.Equal("\u0001", trace.GetField("TruncatedTies")!.GetRawConstantValue());
	}
}
