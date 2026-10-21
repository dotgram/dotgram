using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A way back taken after something was read and built: what the parse answers then is what it
/// would answer had it read the alternative it came back for in the first place, on every
/// carrier and with the direct reader on or off.
/// </summary>
public sealed class BacktrackedValueTests
{
	public static IEnumerable<object[]> Readings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				yield return [carrier, direct];
	}

	/// <summary>
	/// Literal alternatives read as one run keep no way back between them, which holds only
	/// where nothing after the run can begin where one of them did: <c>"ccc" | 'a'</c> is such a
	/// run, but <c>"cc"</c> after it is read only by coming back from <c>"ccc"</c>. And where a
	/// member continues another, <c>'c' | "ca"</c>, coming back for the longer one is reading it
	/// from past the shorter, where nothing after the run may be read.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_literal_run_keeps_its_way_back_to_a_later_alternative_that_begins_alike(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = ("ccc" | 'a' | "cc") & 'c' => @(parserText)
			S : @string = ("ab" | 'x' | ['a'..'b']) & "bc" => @(parserText)
			T : @string = ('c' | "ca" | ['b']) => @(parserText)
			U : @string = ('b' | "bc" | "bcc") => @(parserText)
			""", carrier, direct);

		Assert.Equal((true, "ccc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ccc")));
		Assert.Equal((true, "ac"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ac")));
		Assert.Equal((true, "cccc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "cccc")));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "cc").IsSuccess);
		Assert.Equal((true, "abc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "abc")));
		Assert.Equal((true, "abbc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "abbc")));

		// A continuation of a member, "ca" after 'c', that does not fit leaves nothing after the
		// run to be read from where it stands.
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseT", "cb").IsSuccess);
		Assert.Equal((true, "ca"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseT", "ca")));
		Assert.Equal((true, "b"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseT", "b")));

		// Two continuations of one member, the second continuing the first: the second is read
		// by coming back from the first.
		Assert.Equal((true, "bcc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseU", "bcc")));
		Assert.Equal((true, "bc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseU", "bc")));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseU", "bccc").IsSuccess);
	}

	/// <summary>
	/// A guard that names a rule's value builds it while reading; where the parse comes back into
	/// that rule for another of its alternatives, what was built for the first is not the value
	/// any more — neither for the guard asked again nor for the result.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_value_a_guard_built_is_built_again_once_its_rule_reads_another_way(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = v: Two & when @(v == "2") => @(v)
			Two : @string = 'b' => @("1") | 'b' => @("2")
			S : @string = v: Longer & when @(v != "") => @(v)
			Longer : @string = 'b' => @("1") | "bc" => @("2")
			""", carrier, direct);

		Assert.Equal((true, "2"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "b")));
		Assert.Equal((true, "1"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "b")));
		Assert.Equal((true, "2"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "bc")));
	}

	/// <summary>
	/// An alternative that can read nothing in front of a literal run, where what comes after it
	/// may begin anything — a look, or a literal of no characters — has nothing after it to turn
	/// it away by, and the engine wrote that as a test of no ranges, which did not compile.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_choice_whose_later_alternatives_may_begin_anything_compiles(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = ('b'? | "b" | "cb" | ?='c') => @(parserText)
			S : @string = ('b'? | "b" | "cb" | "") => @(parserText)
			""", carrier, direct);

		foreach (var rule in new[] { "TryParseR", "TryParseS" })
		{
			Assert.Equal((true, "b"), Answer(EmittedCode.Match(assembly, "Grammar", rule, "b")));
			Assert.Equal((true, "cb"), Answer(EmittedCode.Match(assembly, "Grammar", rule, "cb")));
			Assert.Equal((true, ""), Answer(EmittedCode.Match(assembly, "Grammar", rule, "")));
			Assert.False(EmittedCode.Match(assembly, "Grammar", rule, "c").IsSuccess);
		}
	}

	/// <summary>
	/// An atomic group commits the first reading of what it holds. Where that reading came about by
	/// a run giving back for a guard — <c>'a' &amp; c: 'b'* &amp; when @(c.Length == 0)</c> reads
	/// <c>a</c> of <c>ab</c> — what follows the group cannot reach the alternative after it.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void An_atomic_group_keeps_a_first_reading_a_guard_made(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = { Y } => @(parserText)
			Y : @string = 'a' & c: 'b'* & when @(c.Length == 0) => @("1") | "ab" => @("2")
			S : @string = { 'a' & c: 'b'* & when @(c.Length == 0) | "ab" } => @(parserText)
			""", carrier, direct);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseS", "ab").IsSuccess);
		Assert.Equal((true, "a"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "a")));
		Assert.Equal((true, "a"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "a")));
	}

	/// <summary>
	/// The same commit around a run that must take a turn, <c>'b'+</c> giving back to one for
	/// the guard, and with something read after the group; and a rule called both inside braces
	/// and outside them, whose readings differ only where the braces are.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void An_atomic_group_keeps_a_first_reading_a_guard_made_whatever_follows_it(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = { Y } & 'b' => @(parserText)
			Y : @string = 'a' & c: 'b'+ & when @(c.Length == 1) => @("1") | "abb" => @("2")
			S : @string = { Y } => @(parserText) | Y => @(parserText + "!")
			T : @string = { 'a' & c: 'b'+ & when @(c.Length == 1) | "abb" } & 'b' => @(parserText)
			""", carrier, direct);

		Assert.Equal((true, "abb"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "abb")));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ab").IsSuccess);
		Assert.Equal((true, "abb!"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "abb")));
		Assert.Equal((true, "ab"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "ab")));
		Assert.Equal((true, "abb"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseT", "abb")));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseT", "ab").IsSuccess);
	}

	/// <summary>
	/// The same commit in a rule marked <c>?</c> of a grammar cut in two: the rule is read over
	/// kinds, but inside it a repetition gives back, so the braces mean what they mean over
	/// characters — and what follows the group has no say in its first reading.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void An_atomic_group_in_a_rule_that_gives_back_over_kinds_keeps_a_first_reading_a_guard_made(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			trivia = { ' '* }
			R? : @string = { 'a' & c: 'b'* & when @(c.Length == 0) | 'a' & 'b' } => @(parserText)
			S? : @string = { Y } => @(parserText)
			Y? : @string = 'a' & c: 'b'* & when @(c.Length == 0) => @("1") | 'a' & 'b' => @("2")
			""", carrier, direct, lexical: true);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseS", "ab").IsSuccess);
		Assert.Equal((true, "a"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "a")));
		Assert.Equal((true, "a"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "a")));
	}

	/// <summary>
	/// The group in a rule that does not give back over kinds, calling a rule marked <c>?</c>:
	/// directly, and through a rule that does not give back. The rule marked <c>?</c> gives back
	/// inside itself against the group's stop, however it was reached.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void An_atomic_group_over_kinds_commits_the_first_reading_of_a_called_rule_that_gives_back(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			trivia = { ' '* }
			R : @string = { y: Y } => @(y)
			S : @string = { z: Z } => @(z)
			Z : @string = y: Y => @(y)
			Y? : @string = 'a' & c: 'b'* & when @(c.Length == 0) => @("1") | 'a' & 'b' => @("2")
			T : @string = { P } => @(parserText)
			U : @string = { Q } => @(parserText)
			Q = P
			P? = 'a' & 'b'* & when @(parserText.Length == 1) | 'a' & 'b'
			V : @string = { p: M | 'a' & 'b' } => @(p ?? "fallback")
			W : @string = { n: N } => @(n)
			N : @string = p: M => @(p)
			M? : @string = s: 'a' & 'b'* & ?=(when @(parserText.Length == 1)) => @(s)
			X : @string = { P & 'c' } => @(parserText)
			""", carrier, direct, lexical: true);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseS", "ab").IsSuccess);
		Assert.Equal((true, "1"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "a")));
		Assert.Equal((true, "1"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "a")));

		// Without a capture the rule marked `?` is compiled in place, under its caller's continuation.
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseT", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseU", "ab").IsSuccess);
		Assert.Equal((true, "a"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseT", "a")));
		Assert.Equal((true, "a"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseU", "a")));

		// A captured call is a site: the body compiled where the call stood, in the arena.
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseV", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseW", "ab").IsSuccess);
		Assert.Equal((true, "a"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseV", "a")));
		Assert.Equal((true, "a"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseW", "a")));

		// A sibling after the call inside the braces: what follows the rule is still the stop
		// at its boundary, not the sibling, and the rule's first reading is `a`.
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseX", "abc").IsSuccess);
		Assert.Equal((true, "ac"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseX", "ac")));
	}

	/// <summary>
	/// A rule marked <c>?</c> expanded in place keeps the boundary a call has: what follows it
	/// inside the braces does not reach back into it for a shorter reading. And the arena-free
	/// optional of <c>(?!Q &amp; 'a')?</c> inside such a rule, entered on the <c>a</c>, still has
	/// its way back to the skip.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_rule_that_gives_back_expanded_in_a_group_over_kinds_keeps_its_boundary(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			trivia = { ' '* }
			R : @string = { y: Y & 'b' } => @(y)
			Y? : @string = t: ('a' & 'b'*) => @(t)
			S : @string = { I & 'b' } => @(parserText)
			I? = 'a' & 'b'*
			T : @string = { P } => @(parserText)
			P? = (?!Q & 'a')? & Q
			Q = 'a' & 'c'
			""", carrier, direct, lexical: true);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "abb").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseS", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseS", "abb").IsSuccess);
		Assert.Equal((true, "ac"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseT", "ac")));
	}

	/// <summary>
	/// A negative look at a rule, in a group that may be skipped: <c>(?!Q &amp; 'a'){0,1}</c> before
	/// <c>Q</c>, on <c>ac</c>, where <c>Q</c> reads <c>ac</c>. The look holds, the group is skipped,
	/// and <c>Q</c> is read.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_skipped_group_with_a_negative_look_at_a_rule_lets_the_rule_be_read(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = (?!Q & 'a'){0,1} & c: Q => @(c)
			Q : @string = c: W => @(c)
			W : @string = "ac" => @(parserText)
			""", carrier, direct);

		Assert.Equal((true, "ac"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ac")));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "aa").IsSuccess);
	}

	/// <summary>
	/// The same skip spelled <c>?</c>, over a look at a literal, and with a positive look that
	/// holds before the body fails: in each the body is entered on its first character and
	/// fails for another reason, and what follows is read where the body began.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_skipped_group_entered_on_its_first_character_lets_what_follows_be_read(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = (?!Q & 'a')? & c: Q => @(c)
			S : @string = (?!"ac" & 'a')? & "ac" => @(parserText)
			T : @string = (?="ac" & 'b')? & "ac" => @(parserText)
			Q : @string = "ac" => @(parserText)
			""", carrier, direct);

		Assert.Equal((true, "ac"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ac")));
		Assert.Equal((true, "ac"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "ac")));
		Assert.Equal((true, "ac"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseT", "ac")));
	}

	static (bool, object?) Answer((bool IsSuccess, object? Value, string? Error, long Position) match)
	{
		return (match.IsSuccess, match.Value);
	}

	static System.Reflection.Assembly Compile(string rules, CarrierKind carrier, bool direct, bool lexical = false)
	{
		var publications = string.Join("\n", rules.Split('\n')
			.Select(static line => line.Trim())
			.Where(static line => line.Length > 0 && char.IsUpper(line[0]) && line.Split(' ')[0].TrimEnd('?') is "R" or "S" or "T" or "U" or "V" or "W" or "X")
			.Select(static line => "parse " + line.Split(' ')[0].TrimEnd('?')));

		var result = GramCompiler.Compile(rules + "\n" + publications + "\n", new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = direct,
			Lexical       = lexical,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}
}
