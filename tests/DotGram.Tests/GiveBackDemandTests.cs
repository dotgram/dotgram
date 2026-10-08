using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The immediate carrier, once a loop has given a way back, reads unbuilt until an attempt stands
/// and then builds that attempt once (<c>Machine.DefersOnGiveBack</c>): what the tape builds, and
/// what the two carriers answer, are held equal; what the immediate carrier builds is held linear.
/// </summary>
/// <remarks>
/// <para>
/// A repetition whose turn begins with what follows the repetition also begins with — <c>Ows ','
/// Ows Item</c> before <c>Ows</c> — cannot be proven final, so every turn opens a way, and a text
/// refused after the list gives the turns back one at a time, re-reading what stands each time. A
/// carrier that builds as it reads built the standing turns again on every attempt: half a million
/// members for a list of a thousand, where the tape builds none. Now the attempts after the second
/// give-back of a loop build nothing until one stands, and that one is read once more and built.
/// Every factory here logs its name, so the count of constructions is what is held, and not only
/// the answer.
/// </para>
/// <para>
/// Auto keeps every grammar here on the tape (the <c>read again</c> gate), and each test that reads
/// through Auto says so, so that nothing passes as an immediate reading that is the tape again.
/// </para>
/// <para>
/// A buffered input (a <c>TextReader</c>) is read once, recording: there the attempt that stood is
/// read again recording too, and what that reading recorded is taken back afterwards, so its
/// refusal says what the tape's says.
/// </para>
/// </remarks>
public sealed class GiveBackDemandTests
{
	const string Members = """
		public static readonly System.Collections.Generic.List<string> Built =
			new System.Collections.Generic.List<string>();
		static string Log(string name, string value)
		{
			Built.Add(name);
			return value;
		}
		static System.Collections.Generic.List<string> Join(string first, string[] rest)
		{
			Built.Add("List");
			var list = new System.Collections.Generic.List<string>(rest.Length + 1) { first };
			list.AddRange(rest);
			return list;
		}
		static System.Collections.Generic.List<string> Mark(System.Collections.Generic.List<string> list)
		{
			Built.Add("Pair");
			list.Add("#");
			return list;
		}
		""";

	/// <summary>
	/// The list whose turns are given back. <c>Pair</c> reads a list and then one member back as
	/// its own tail; <c>Mark</c> mutates the list it is handed, so a value handed to two
	/// constructions would show.
	/// </summary>
	const string Grammar = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Pair : @List<string> = l: List & ',' & Ows & 'z' & '!' => @(Mark(l))
		parse List
		parse Pair
		""";

	static string Members_(int count)
	{
		return string.Join(", ", Enumerable.Repeat("ab", count));
	}

	/// <summary>
	/// A list refused after its last member: the tape builds nothing; the immediate carrier builds
	/// its first two attempts and no more — linear in the members, where it was their square. Two,
	/// because the first way given back is read building as it always was, and only the second
	/// raises the count: one give-back is how a choice reads its next alternative.
	/// </summary>
	[Fact]
	public void A_refused_list_builds_its_members_once()
	{
		var counts = new Dictionary<int, int>();

		foreach (var n in new[] { 50, 100 })
		{
			var input     = Members_(n) + ", <";
			var tape      = Run(CarrierKind.Tape, Grammar, "TryParseList", input);
			var immediate = Run(CarrierKind.Immediate, Grammar, "TryParseList", input);

			Assert.StartsWith("<refused", tape.Answer, StringComparison.Ordinal);
			Assert.Equal(tape.Answer, immediate.Answer);
			Assert.Empty(tape.Built);

			var items = immediate.Built.Count(static one => one == "Item");

			Assert.InRange(items, n, 2 * n);
			Assert.InRange(immediate.Built.Count(static one => one == "List"), 1, 2);

			counts[n] = items;
		}

		Assert.True(counts[100] <= 2.1 * counts[50], $"Item built {counts[50]} times at 50 members and {counts[100]} at 100.");
	}

	/// <summary>
	/// A pair accepted with one turn given back: the values agree on every carrier, the mutation
	/// included, and the immediate carrier builds the turns that stand no more than twice — the
	/// first attempt and the attempt that stood, read once more.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Immediate)]
	public void An_accepted_pair_builds_the_attempt_that_stood_once(CarrierKind carrier)
	{
		const int n = 40;

		var outcome = Run(carrier, Grammar, "TryParsePair", Members_(n) + ", z!");

		Assert.Equal("[" + string.Join("|", Enumerable.Repeat("ab", n)) + "|#]", outcome.Answer);

		if (carrier == CarrierKind.Auto)
			Assert.Equal("Immediate", outcome.Chosen);

		if (carrier == CarrierKind.Tape)
		{
			Assert.Equal(n, outcome.Built.Count(static one => one == "Item"));
			Assert.Equal(1, outcome.Built.Count(static one => one == "List"));

			return;
		}

		// The first attempt read n + 1 items (the `z` as a member), the attempt that stood n.
		Assert.InRange(outcome.Built.Count(static one => one == "Item"), n, 2 * n + 1);
		Assert.InRange(outcome.Built.Count(static one => one == "List"), 1, 2);
		Assert.Equal(1, outcome.Built.Count(static one => one == "Pair"));
	}

	/// <summary>
	/// The same list over a buffered input, whose one reading records as it goes: the members are
	/// built a bounded number of times, the values agree, and the refusal says what the tape's says.
	/// </summary>
	[Theory]
	[InlineData("TryParseList", 60, ", <", null)]
	[InlineData("TryParsePair", 60, ", z!", "#")]
	[InlineData("TryParsePair", 60, ", z?", null)]
	public void A_buffered_input_given_back_builds_a_bounded_number_of_times_and_refuses_as_the_tape_does(string entry, int n, string tail, string? accepted)
	{
		var input     = Members_(n) + tail;
		var tape      = Run(CarrierKind.Tape, Grammar, entry, input, buffered: true);
		var immediate = Run(CarrierKind.Immediate, Grammar, entry, input, buffered: true);

		Assert.Equal(tape.Answer, immediate.Answer);

		if (accepted is null)
			Assert.StartsWith("<refused", tape.Answer, StringComparison.Ordinal);
		else
			Assert.EndsWith("|" + accepted + "]", tape.Answer, StringComparison.Ordinal);

		// Two attempts per loop that gives back (the entry's, and Pair's own) build; the rest do not.
		Assert.InRange(immediate.Built.Count(static one => one == "Item"), n, 4 * (n + 1));
	}

	/// <summary>
	/// A recording attempt opens a way at a shut door of a repetition where a quiet one breaks
	/// before it: the reading that builds the attempt that stood has to be of the same kind as the
	/// attempt, or it takes the recorded ways in other places. The two grammars of the reviews that
	/// found it, on every carrier, in the answering and the throwing forms: values and messages
	/// equal to the tape's.
	/// </summary>
	const string ShutDoor = """
		trivia = none
		Inner : @string = x: ['a']+ & "aa" & ('b' & 'c')* & ['b' | '!']? & '!' => @(Log("Inner", x.ToString()))
		Start : @string = i: Inner & when @(i.Length > 0) & '?' => @(Log("Start", i))
		parse Start
		""";

	const string TwoLists = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Inner : @string = a: List & ';' & b: List & ',' & Ows & 'z' & ',' & Ows & 'z' => @(Log("Inner", string.Join("|", a) + ";" + string.Join("|", b)))
		Outer : @string = i: Inner & when @(i.Length > 0) & '!' => @(Log("Outer", i))
		parse Outer
		""";

	[Theory]
	[InlineData(CarrierKind.Tape, ShutDoor, "TryParseStart", "aaa!!x")]
	[InlineData(CarrierKind.Auto, ShutDoor, "TryParseStart", "aaa!!x")]
	[InlineData(CarrierKind.Immediate, ShutDoor, "TryParseStart", "aaa!!x")]
	[InlineData(CarrierKind.Immediate, ShutDoor, "ParseStart", "aaa!!x")]
	[InlineData(CarrierKind.Immediate, ShutDoor, "TryParseStart", "aaa!?")]
	[InlineData(CarrierKind.Immediate, ShutDoor, "TryParseStart", "aaabc!?")]
	[InlineData(CarrierKind.Tape, TwoLists, "TryParseOuter", "ab, ab;ab, ab, z, z?")]
	[InlineData(CarrierKind.Auto, TwoLists, "TryParseOuter", "ab, ab;ab, ab, z, z?")]
	[InlineData(CarrierKind.Immediate, TwoLists, "TryParseOuter", "ab, ab;ab, ab, z, z?")]
	[InlineData(CarrierKind.Immediate, TwoLists, "ParseOuter", "ab, ab;ab, ab, z, z?")]
	[InlineData(CarrierKind.Immediate, TwoLists, "TryParseOuter", "ab, ab;ab, ab, z, z!")]
	[InlineData(CarrierKind.Immediate, TwoLists, "TryParseOuter", "ab;ab, z, z!")]
	public void A_replay_takes_the_recorded_ways_where_a_recording_attempt_took_them(CarrierKind carrier, string grammar, string entry, string input)
	{
		var tape    = Run(CarrierKind.Tape, grammar, entry, input);
		var outcome = Run(carrier, grammar, entry, input);

		Assert.Equal(tape.Answer, outcome.Answer);

		if (carrier == CarrierKind.Auto)
			Assert.Equal("Immediate", outcome.Chosen);
	}

	/// <summary>
	/// The reading that builds the attempt that stood, made while recording: an entry whose own
	/// guard asks for its value so far reads a refused input a second time with the count at zero
	/// and recording, so a loop under it that gives back twice there defers and replays while the
	/// message is being collected. The message is the tape's.
	/// </summary>
	const string SelfAskingOverTwo = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Two : @string = l: List & ',' & Ows & 'q' & ',' & Ows & 'q' => @(Log("Two", string.Join("|", l)))
		Fold : @string = l: Fold & Ows & ';' & Ows & when @(l.Length > 0) & t: Two => @(Log("Fold", l + "+" + t)) | t: Two => @(Log("Fold", t))
		parse Fold
		""";

	[Theory]
	[InlineData("TryParseFold", "ab, q, q", "ab")]
	[InlineData("TryParseFold", "ab, q, q ; ab, ab, q, q", "ab+ab|ab")]
	[InlineData("TryParseFold", "ab, q, q ; ab, q, q, x", null)]
	[InlineData("TryParseFold", "ab, q, q ; ab, q, q ;", null)]
	[InlineData("ParseFold", "ab, q, q ; ab, q, q, x", null)]
	[InlineData("ParseFold", "ab, q, q, q", "ab|q")]
	public void A_loop_replayed_while_the_message_is_collected_leaves_the_message_the_tapes(string entry, string input, string? expected)
	{
		var tape      = Run(CarrierKind.Tape, SelfAskingOverTwo, entry, input);
		var immediate = Run(CarrierKind.Immediate, SelfAskingOverTwo, entry, input);

		if (expected is not null)
			Assert.Equal(expected, tape.Answer);
		else
			Assert.StartsWith("<", tape.Answer, StringComparison.Ordinal);

		Assert.Equal(tape.Answer, immediate.Answer);
	}

	/// <summary>
	/// A guard inside the turn asks for its item: it is built in a deferred attempt as in a built
	/// one, so both carriers refuse and accept alike, with the same message.
	/// </summary>
	const string Guarded = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item & when @(item.Length < 5) => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Pair : @List<string> = l: List & ',' & Ows & 'z' & '!' => @(Mark(l))
		parse List
		parse Pair
		""";

	[Theory]
	[InlineData("TryParseList", "ab, ab, abcdef, ab")]
	[InlineData("TryParseList", "ab, ab, ab, <")]
	[InlineData("TryParsePair", "ab, ab, abcdef, z!")]
	[InlineData("TryParsePair", "ab, ab, z!")]
	[InlineData("TryParsePair", "ab, ab, z?")]
	[InlineData("ParseList", "ab, ab, abcdef, ab")]
	[InlineData("ParsePair", "ab, ab, z?")]
	public void A_guard_in_a_turn_given_back_sees_its_value(string entry, string input)
	{
		var tape      = Run(CarrierKind.Tape, Guarded, entry, input);
		var immediate = Run(CarrierKind.Immediate, Guarded, entry, input);

		Assert.Equal(tape.Answer, immediate.Answer);
	}

	/// <summary>
	/// The give-back inside a rule's own loop rather than the entry's: <c>Inner</c> reads the list
	/// and one member back, and <c>Outer</c> reads <c>Inner</c> one way or another.
	/// </summary>
	const string Nested = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Inner : @string = l: List & ',' & Ows & 'q' & ';' => @(Log("Inner", string.Join("|", l)))
		Outer : @string = i: Inner & '!' => @(Log("Outer", i + "!")) | i: Inner & '?' => @(Log("Outer", i + "?"))
		parse Outer
		""";

	[Theory]
	[InlineData("ab, ab, q;!", "ab|ab!")]
	[InlineData("ab, ab, q;?", "ab|ab?")]
	[InlineData("ab, ab, q;x", null)]
	[InlineData("ab, ab, q, q;!", null)]
	public void A_give_back_inside_a_nested_rule_builds_once(string input, string? expected)
	{
		var tape      = Run(CarrierKind.Tape, Nested, "TryParseOuter", input);
		var immediate = Run(CarrierKind.Immediate, Nested, "TryParseOuter", input);

		Assert.Equal(expected ?? tape.Answer, tape.Answer);
		Assert.Equal(tape.Answer, immediate.Answer);

		if (expected is null)
			return;

		// Inner is built once for the alternative that stands, and once more where the first
		// alternative read it whole and failed after it — its first attempt, as today. The items
		// are built by the first two attempts of every loop that gives back (Inner's own, the
		// alternative's, the entry's), a constant number of readings of the two that stand.
		Assert.InRange(immediate.Built.Count(static one => one == "Inner"), 1, 2);
		Assert.InRange(immediate.Built.Count(static one => one == "Item"), 2, 12);
	}

	/// <summary>
	/// Two loops, one inside the other: <c>Inner</c>'s own loop gives a turn back, defers and
	/// replays; the entry then fails after it and gives a way back into <c>Inner</c>'s region,
	/// where the shorter list is the one that stands.
	/// </summary>
	const string TwoLoops = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Inner : @string = l: List & ',' & Ows & 'q' => @(Log("Inner", string.Join("|", l)))
		Outer : @string = i: Inner & ',' & Ows & 'q' & '!' => @(Log("Outer", i + "!"))
		parse Outer
		""";

	[Theory]
	[InlineData("ab, q, q!", "ab!")]
	[InlineData("ab, ab, q, q!", "ab|ab!")]
	[InlineData("ab, q, q?", null)]
	public void An_outer_loop_retries_into_a_region_an_inner_loop_replayed(string input, string? expected)
	{
		var tape      = Run(CarrierKind.Tape, TwoLoops, "TryParseOuter", input);
		var immediate = Run(CarrierKind.Immediate, TwoLoops, "TryParseOuter", input);

		Assert.Equal(expected ?? tape.Answer, tape.Answer);
		Assert.Equal(tape.Answer, immediate.Answer);

		if (expected is not null)
			Assert.Equal(1, immediate.Built.Count(static one => one == "Outer"));
	}

	/// <summary>
	/// A left-recursive rule whose step guard reads the value so far asks for its own value while
	/// it is read: every reading of it builds, and its loop never defers — the fold is built from
	/// what the step before it built, not from a register nothing wrote. Its steps and what follows
	/// both begin with <c>Ows</c>, so a step can be given back.
	/// </summary>
	const string SelfAsking = """
		trivia = none
		Ows = [' ']*
		Start : @string = f: Fold & Ows & ';' => @(Log("Start", f))
		Fold : @string = l: Fold & Ows & ',' & Ows & when @(l.Length > 0) & d: Digit => @(Log("Fold", l + d)) | d: Digit => @(Log("Fold", d))
		Digit : @string = t: ['0'..'9'] => @(Log("Digit", t.ToString()))
		parse Start
		parse Fold
		""";

	[Theory]
	[InlineData(CarrierKind.Tape, "TryParseStart", "1 , 2 , 3 ;", "123")]
	[InlineData(CarrierKind.Auto, "TryParseStart", "1 , 2 , 3 ;", "123")]
	[InlineData(CarrierKind.Immediate, "TryParseStart", "1 , 2 , 3 ;", "123")]
	[InlineData(CarrierKind.Immediate, "TryParseStart", "1 , 2 , 3 x", null)]
	[InlineData(CarrierKind.Immediate, "TryParseFold", "1 , 2 , 3 ", null)]
	[InlineData(CarrierKind.Immediate, "ParseFold", "1 , 2 , 3 ", null)]
	[InlineData(CarrierKind.Immediate, "ParseStart", "1 , 2 , 3 x", null)]
	public void A_self_asking_fold_given_back_builds_as_the_tape_does(CarrierKind carrier, string entry, string input, string? expected)
	{
		var tape    = Run(CarrierKind.Tape, SelfAsking, entry, input);
		var outcome = Run(carrier, SelfAsking, entry, input);

		if (expected is not null)
			Assert.Equal(expected, tape.Answer);
		else
			Assert.StartsWith("<", tape.Answer, StringComparison.Ordinal);

		Assert.Equal(tape.Answer, outcome.Answer);

		if (carrier == CarrierKind.Auto)
			Assert.Equal("Immediate", outcome.Chosen);
	}

	/// <summary>
	/// A mark (§7.8) laid inside a turn that is given back: the mark the abandoned turn left is taken
	/// back before the way is flipped, and laid again by the attempt that stands, so what the items
	/// see of the state agrees with the tape.
	/// </summary>
	const string Marked = """
		@using System.Collections.Generic;
		state : @int
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item with state @(7) => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", string.Join(".", parserState.ToArray()) + ":" + text.ToString()))
		Pair : @List<string> = l: List & ',' & Ows & 'z' & '!' => @(Mark(l))
		parse List
		parse Pair
		""";

	[Theory]
	[InlineData("TryParsePair", "ab, abc, z!")]
	[InlineData("TryParsePair", "ab, abc, z?")]
	[InlineData("TryParseList", "ab, abc, <")]
	[InlineData("ParsePair", "ab, abc, z?")]
	public void A_mark_inside_a_turn_given_back_is_laid_again(string entry, string input)
	{
		var tape      = Run(CarrierKind.Tape, Marked, entry, input);
		var immediate = Run(CarrierKind.Immediate, Marked, entry, input);

		Assert.Equal(tape.Answer, immediate.Answer);
	}

	/// <summary>
	/// A choice over characters whose alternatives both read the list: the loop around each
	/// alternative gives back and defers; the second alternative is the one that stands.
	/// </summary>
	const string Alternatives = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Alt : @string = l: List & '!' => @(Log("Alt", "!" + string.Join("|", l))) | l: List & ',' & Ows & 'z' & '?' => @(Log("Alt", "?" + string.Join("|", l)))
		parse Alt
		""";

	[Theory]
	[InlineData("ab, ab, z?", "?ab|ab")]
	[InlineData("ab, ab, z!", "!ab|ab|z")]
	[InlineData("ab, ab, z;", null)]
	public void A_choice_whose_alternatives_give_back_builds_the_one_that_stood(string input, string? expected)
	{
		var tape      = Run(CarrierKind.Tape, Alternatives, "TryParseAlt", input);
		var immediate = Run(CarrierKind.Immediate, Alternatives, "TryParseAlt", input);

		Assert.Equal(expected ?? tape.Answer, tape.Answer);
		Assert.Equal(tape.Answer, immediate.Answer);

		if (expected is not null)
			Assert.Equal(1, immediate.Built.Count(static one => one == "Alt"));
	}

	/// <summary>
	/// An atomic group around the list and its tail: the group is asked for a reading until it
	/// has one, which is then sealed — the replay that builds it reads before the seal.
	/// </summary>
	const string Atomic = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Sealed : @string = { l: List & ',' & Ows & 'z' } & '!' => @(Log("Sealed", string.Join("|", l)))
		parse Sealed
		""";

	[Theory]
	[InlineData("ab, ab, z!", "ab|ab")]
	[InlineData("ab, ab, z?", null)]
	[InlineData("ab, ab, z, z!", null)]
	public void An_atomic_group_that_gives_back_is_replayed_before_it_is_sealed(string input, string? expected)
	{
		var tape      = Run(CarrierKind.Tape, Atomic, "TryParseSealed", input);
		var immediate = Run(CarrierKind.Immediate, Atomic, "TryParseSealed", input);

		Assert.Equal(expected ?? tape.Answer, tape.Answer);
		Assert.Equal(tape.Answer, immediate.Answer);

		if (expected is not null)
		{
			Assert.Equal(1, immediate.Built.Count(static one => one == "Sealed"));
			Assert.InRange(immediate.Built.Count(static one => one == "List"), 1, 2);
		}
	}

	/// <summary>
	/// A reading deep enough to be handed to a stack of its own, with the give-back at the bottom:
	/// the count travels with the registers across the hand-off, and the attempt that stands is
	/// built once on whichever stack reads it.
	/// </summary>
	const string Deep = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(Log("Rest", item))
		Item : @string = text: Word => @(Log("Item", text.ToString()))
		Nest : @string = '(' & n: Nest & ')' => @(Log("Nest", "(" + n + ")")) | l: List & ',' & Ows & 'z' => @(Log("Nest", string.Join("|", l)))
		parse Nest
		""";

	[Theory]
	[InlineData("!", 8000)]
	[InlineData("", 8000)]
	public void A_give_back_below_a_hand_off_builds_once(string tail, int depth)
	{
		var input = new string('(', depth) + "ab, ab, z" + new string(')', depth) + tail;

		// On a 256 KiB stack: eight thousand levels of a reader whose frame cannot be under forty
		// bytes do not fit it, so a reading that answers was handed off on its way down — the
		// generated reader probes and hands off (Deepen_DotGram), and without that the process
		// would have died of the overflow rather than failed the test.
		Assert.Contains("Deepen_DotGram(", Compile(CarrierKind.Immediate, Deep).Source, StringComparison.Ordinal);

		var tape      = OnSmallStack(() => Run(CarrierKind.Tape, Deep, "TryParseNest", input));
		var immediate = OnSmallStack(() => Run(CarrierKind.Immediate, Deep, "TryParseNest", input));

		Assert.Equal(tape.Answer, immediate.Answer);

		if (tail.Length == 0)
		{
			Assert.StartsWith("(", tape.Answer, StringComparison.Ordinal);
			Assert.Equal(depth + 1, immediate.Built.Count(static one => one == "Nest"));
		}
		else
		{
			// The entry's first two attempts build, each down the whole nesting; the rest do not.
			Assert.StartsWith("<refused", tape.Answer, StringComparison.Ordinal);
			Assert.InRange(immediate.Built.Count(static one => one == "Nest"), 0, 2 * (depth + 1));
		}
	}

	static T OnSmallStack<T>(Func<T> read)
	{
		T result = default!;
		Exception? failed = null;

		var thread = new Thread(() =>
		{
			try { result = read(); }
			catch (Exception e) { failed = e; }
		}, 256 * 1024);

		thread.Start();
		thread.Join();

		if (failed is not null)
			throw new InvalidOperationException("The reading on the small stack failed.", failed);

		return result;
	}

	/// <summary>
	/// Random texts over the alphabet of the grammar through every entry of it, in the answering
	/// and the throwing forms: the two carriers agree on every value and every message.
	/// </summary>
	[Theory]
	[InlineData(Grammar, "TryParseList|TryParsePair|ParseList|ParsePair", "ab ,z!<")]
	[InlineData(Guarded, "TryParseList|TryParsePair|ParseList|ParsePair", "ab ,z!<")]
	[InlineData(Nested, "TryParseOuter|ParseOuter", "ab ,q;!?")]
	[InlineData(TwoLoops, "TryParseOuter|ParseOuter", "ab ,q!")]
	[InlineData(Marked, "TryParseList|TryParsePair|ParseList|ParsePair", "ab ,z!<")]
	[InlineData(Alternatives, "TryParseAlt|ParseAlt", "ab ,z!?")]
	[InlineData(Atomic, "TryParseSealed|ParseSealed", "ab ,z!?")]
	[InlineData(ShutDoor, "TryParseStart|ParseStart", "abc!?x")]
	[InlineData(TwoLists, "TryParseOuter|ParseOuter", "ab ,;z!?")]
	[InlineData(SelfAskingOverTwo, "TryParseFold|ParseFold", "ab ,;qx")]
	public void The_carriers_agree_over_random_texts(string grammar, string entries, string alphabet)
	{
		var tape      = Compile(CarrierKind.Tape, grammar);
		var immediate = Compile(CarrierKind.Immediate, grammar);
		var random    = new Random(20261007);

		for (var i = 0; i < 4000; i++)
		{
			var length = random.Next(0, 16);
			var text   = new string(Enumerable.Range(0, length).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());

			foreach (var entry in entries.Split('|'))
				Assert.Equal(Read(tape.Host, entry, text).Answer, Read(immediate.Host, entry, text).Answer);
		}
	}

	static (Type Host, string Source, string? Chosen) Compile(CarrierKind carrier, string grammar, bool buffered = false)
	{
		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance, BufferedInput = buffered,
		});

		Assert.DoesNotContain(compiled.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(compiled.Sources).Text;

		// The immediate reading is the one under test, and a grammar it could not carry would be
		// read on the tape and pass by testing nothing. Every grammar here has a loop that gives
		// back and builds, so some loop of it is written with the hooks: the call that moves where
		// the loop stands on a way given back is written nowhere else.
		if (carrier == CarrierKind.Immediate)
			Assert.Contains("Retried_DotGram(", source, StringComparison.Ordinal);

		// Which carrier a grammar left to choose was given: the tape's walk at the end is written
		// where the immediate carrier refused it, and nowhere else.
		var chosen = carrier != CarrierKind.Auto ? carrier.ToString()
			: source.Contains("Materialize_DotGram", StringComparison.Ordinal) ? "Tape" : "Immediate";

		var host = EmittedCode.Compile(source, declarationMembers: Members).GetType("Grammar")!;

		return (host, source, chosen);
	}

	static Outcome Run(CarrierKind carrier, string grammar, string entry, string input, bool buffered = false)
	{
		var (host, _, chosen) = Compile(carrier, grammar, buffered);
		var outcome           = Read(host, entry, input, buffered);

		return outcome with { Chosen = chosen };
	}

	/// <summary>One entry over one text: the value written out, or where and why it was refused, and what was built.</summary>
	/// <param name="buffered">Whether to read the text through the <c>TextReader</c> form, which reads once, recording.</param>
	static Outcome Read(Type host, string entry, string input, bool buffered = false)
	{
		var built = (IList)host.GetField("Built", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

		built.Clear();

		var method = buffered
			? host.GetMethod(entry, [typeof(TextReader), typeof(int?), typeof(int?)])!
			: host.GetMethod(entry, [typeof(string)])!;

		if (entry.StartsWith("Try", StringComparison.Ordinal))
		{
			var match = buffered ? method.Invoke(null, [new StringReader(input), null, null])! : method.Invoke(null, [input])!;
			var ok    = (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;

			var answer = ok
				? Shown(match.GetType().GetProperty("Value")!.GetValue(match))
				: $"<refused at {match.GetType().GetProperty("Position")!.GetValue(match)}: {match.GetType().GetProperty("Error")!.GetValue(match)}>";

			return new Outcome(answer, built.Cast<string>().ToArray(), null);
		}

		try
		{
			return new Outcome(Shown(method.Invoke(null, [input])), built.Cast<string>().ToArray(), null);
		}
		catch (TargetInvocationException thrown)
		{
			return new Outcome($"<{thrown.InnerException!.GetType().Name}: {thrown.InnerException.Message}>", built.Cast<string>().ToArray(), null);
		}
	}

	static string Shown(object? value)
	{
		return value switch
		{
			null => "<null>",
			string text => text,
			IEnumerable<string> items => "[" + string.Join("|", items) + "]",
			_ => value.ToString()!,
		};
	}

	sealed record Outcome(string Answer, string[] Built, string? Chosen);
}
