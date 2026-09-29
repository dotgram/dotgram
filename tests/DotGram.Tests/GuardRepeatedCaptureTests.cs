using System;
using System.Collections.Generic;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A guard is handed the value the construction is handed. A text capture written inside a
/// repetition whose body has something else in it — <c>(y: D &amp; ','?){2}</c> — records one
/// piece per turn, and §7.3 makes its value the pieces joined: the guard sees that join, not
/// the last piece, in every carrier and with the direct reader on or off.
/// </summary>
/// <remarks>
/// The guard keeps what it was handed and the construction answers both, so each case says
/// in one string what the guard saw and what the rule built: <c>guard|built</c>.
/// </remarks>
public sealed class GuardRepeatedCaptureTests
{
	const string Members = """
		static string seen = "<none>";
		static bool Seen(string value) { seen = value; return true; }
		static string Both(string value) => (seen ?? "<null>") + "|" + (value ?? "<null>");
		static string log = "";
		static bool Log(string value) { log += (log.Length > 0 ? ";" : "") + (value ?? "<null>"); return true; }
		static bool LogA(string[] value) { log += (log.Length > 0 ? ";" : "") + string.Join("/", value); return true; }
		static string Take() { var taken = log; log = ""; return taken; }
		static int made;
		static string Made(string value) { made++; return value; }
		static int TakeMade() { var taken = made; made = 0; return taken; }
		""";

	public static IEnumerable<object[]> Readings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				foreach (var find in new[] { false, true })
					yield return [carrier, direct, find];
	}

	/// <summary>The repro: the guard compares against the join, the only value the rule builds.</summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_guard_compares_against_the_joined_pieces(CarrierKind carrier, bool direct, bool find)
	{
		var assembly = Compile(
			"""
			D = ['0'..'9']
			T : @string = (y: D & ','?){2} & when @(y == "12") => @(y!)
			""", carrier, direct, find);

		var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", "1,2");
		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal("12", match.Value);

		var last = Compile(
			"""
			D = ['0'..'9']
			T : @string = (y: D & ','?){2} & when @(y == "2") => @(y!)
			""", carrier, direct, find);

		Assert.False(EmittedCode.Match(last, "Grammar", "TryParseT", "1,2").IsSuccess);
	}

	/// <summary>What stands between the turns is never part of the value, whatever it is.</summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void The_guard_sees_what_the_construction_builds(CarrierKind carrier, bool direct, bool find)
	{
		var wrong = new List<string>();

		foreach (var (body, input, expected) in new[]
		{
			// Separators of every shape: optional, required, a run, a rule, a word.
			("(y: D & ','?){2}",                    "1,2",     "12|12"),
			("(y: D & ','?){2}",                    "12",      "12|12"),
			("(y: D & ';'){2}",                     "1;2;",    "12|12"),
			("(y: D & ' '+){3}",                    "1 2  3 ", "123|123"),
			("(y: D & Sep){2}",                     "1--2-",   "12|12"),
			("(y: D & \"ab\")+",                    "1ab2ab",  "12|12"),
			("(y: D+ & '.')+",                      "12.34.",  "1234|1234"),
			("('<' & y: D & '>')+",                 "<1><2>",  "12|12"),
			// A run and a turn beside it: the join ends where the last piece does.
			("(y: D & ',')* & y: D",                "1,2,3",   "123|123"),
			// Given back: the turn the loop gave up to what follows is not in the join.
			("(y: D & ','?)+ & '9'",                "1,29",    "12|12"),
			// A turn's alternative abandoned after it captured: its piece is not in the join.
			("((y: D & ',' & 'x') | (y: D & ','))+", "1,2,",   "12|12"),
			// A called rule's own `y` is its own, not a piece of this one's.
			("(y: D & R){2}",                       "12,34,",  "13|13"),
			// The rule inside itself: the inner reading's pieces are the inner rule's.
			("'(' & (y: D & ','?)+ & T? & ')'",     "(1,2(3,4))", "12|12"),
			// One name in two alternatives, each repeated: a slot each, one member.
			("((y: D & ',')+ | (y: D & ';')+)",     "1;2;",    "12|12"),
			// A capture that does not repeat is a single piece, as before.
			("y: D & ',' & z: D",                   "1,2",     "1|1"),
			("y: D{2}",                             "12",      "12|12"),
			("y: D+ & '2'",                         "112",     "11|11"),
			// A capture of what a lookahead saw is repeated the way any capture is (§3.4: `?=X`
			// produces X's value): the turns joined, not the span, and not the last turn.
			("(y: ?=D & D & ','?){2}",              "1,2",     "12|12"),
			("(y: ?=(D & D) & D & D & ','?){2}",    "12,34",   "1234|1234"),
			("(y: ?=D & D & ',')* & y: D",          "1,2,3",   "123|123"),
			("(y: D & ',')* & y: ?=D & D",          "1,2,3",   "123|123"),
			("(y: ?=D & D & ','?)?",                "1,",      "1|1"),
			// Its pieces can overlap, so whether they tile is not their lengths summed: "ab",
			// "b" and "d" measure the span "abcd", and are not it.
			("(y: ?=(D & D) & D){2} & D",           "123",     "1223|1223"),
			("(y: ?=(\"ab\" | L) & L & 'c'?)+",       "abcd",    "abbd|abbd"),
			// A negative lookahead produces nothing, and repeated, nothing joined.
			("(y: ?!'x' & D & ','?){2}",            "1,2",     "|"),
			// Repeated in one alternative and not in the other: one member, one shape.
			("((y: D & ','?){2} & 'a' | y: D & 'b')", "1,2a",  "12|12"),
			("((y: D & ','?){2} & 'a' | y: D & 'b')", "1b",    "1|1"),
			("(y: D & 'b' | (y: D & ','?){2} & 'a')", "1,2a",  "12|12"),
		})
		{
			var grammar = "D = ['0'..'9']\nL = ['a'..'z']\n" + (body.Contains("Sep") ? "Sep = '-'+\n" : "") +
				(body.Contains("& R)") ? "R : @string = y: D & ',' => @(y!)\n" : "") + "T : @string = " + body +
				" & when @(Seen(y)) => @(Both(y))";
			var assembly = Compile(grammar, carrier, direct, find);
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", input);
			var answer = match.IsSuccess ? (string)match.Value! : "<refused " + match.Error + ">";
			if (answer != expected)
				wrong.Add(body + " on '" + input + "': " + answer + ", not " + expected);
		}

		Assert.True(wrong.Count == 0, string.Join("\n", wrong));
	}

	/// <summary>
	/// A guard inside the loop runs at every turn and is handed the turns so far, each time
	/// joined: <c>1</c>, then <c>12</c>, then <c>123</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_guard_inside_the_loop_sees_the_turns_so_far(CarrierKind carrier, bool direct, bool find)
	{
		foreach (var (turns, input, expected) in new[] { (2, "1,2,", "1;12"), (3, "1,2,3,", "1;12;123") })
		{
			var assembly = Compile(
				"D = ['0'..'9']\nT : @string = (y: D & ',' & when @(Log(y))){" + turns + "} => @(Take())",
				carrier, direct, find);
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", input);
			Assert.True(match.IsSuccess, match.Error);
			Assert.Equal(expected, match.Value);
		}
	}

	/// <summary>A switch's selector is handed its captures the way a guard is, and selects on the join.</summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_switch_selects_on_the_joined_pieces(CarrierKind carrier, bool direct, bool find)
	{
		var assembly = Compile(
			"""
			D = ['0'..'9']
			T : @string = (y: D & ','?){2} & switch @(y) { case "12": 'a' default: 'b' } => @(y!)
			""", carrier, direct, find);

		foreach (var (input, read) in new[] { ("1,2a", true), ("12a", true), ("1,2b", false), ("2,2b", true) })
			Assert.Equal(read, EmittedCode.Match(assembly, "Grammar", "TryParseT", input).IsSuccess);
	}

	/// <summary>A repetition that took no turn: the guard and the construction agree on absence.</summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void No_turn_is_the_same_absence_for_both(CarrierKind carrier, bool direct, bool find)
	{
		var assembly = Compile(
			"""
			D = ['0'..'9']
			T : @string = '[' & (y: D & ',')* & ']' & when @(Seen(y)) => @(Both(y))
			""", carrier, direct, find);

		foreach (var (input, expected) in new[] { ("[]", null), ("[1,]", "1"), ("[1,2,]", "12") })
		{
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", input);
			Assert.True(match.IsSuccess, match.Error);
			var built = (string)match.Value!;
			var halves = built.Split('|');
			Assert.Equal(halves[1], halves[0]);
			if (expected is not null)
				Assert.Equal(expected, halves[1]);
		}
	}

	/// <summary>
	/// The repro of a capture over a lookahead: the guard and the construction both had the
	/// last turn and the span, <c>"2"</c> and <c>"1,2"</c>, where the value is <c>"12"</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void What_a_lookahead_saw_is_joined_like_any_capture(CarrierKind carrier, bool direct, bool find)
	{
		var assembly = Compile(
			"""
			D = ['0'..'9']
			T : @string = (y: ?=D & D & ','?){2} & when @(y == "12") => @(y!)
			""", carrier, direct, find);

		var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", "1,2");
		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal("12", match.Value);
	}

	/// <summary>
	/// One member, repeated in one alternative and not in the other, is recorded in one shape:
	/// the reader once wrote the capture as pieces and the other alternative's record as plain
	/// text, whose end it never declared.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_member_repeated_in_one_alternative_only_is_built_by_each(CarrierKind carrier, bool direct, bool find)
	{
		foreach (var rule in new[]
		{
			"(y: D & ','?){2} & 'a' => @(y!) | y: D & 'b' => @(y! + \"b\")",
			"y: D & 'b' => @(y! + \"b\") | (y: D & ','?){2} & 'a' => @(y!)",
		})
		{
			var assembly = Compile("D = ['0'..'9']\nT : @string = " + rule, carrier, direct, find);

			foreach (var (input, expected) in new[] { ("1,2a", "12"), ("12a", "12"), ("1b", "1b") })
			{
				var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", input);
				Assert.True(match.IsSuccess, rule + " on " + input + ": " + match.Error);
				Assert.Equal(expected, match.Value);
			}
		}
	}

	/// <summary>
	/// A left-recursive rule is a loop over its steps (§4.3), and the loop is not a repetition
	/// of the author's: each step's guard and `=>` are handed what that step captured, and a
	/// repetition inside one step is joined within it.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_fold_step_is_handed_its_own_captures(CarrierKind carrier, bool direct, bool find)
	{
		var wrong = new List<string>();

		foreach (var (rule, input, expected) in new[]
		{
			// One piece a step: each guard sees its own, not the base's nor every step's.
			("l: F & ',' & y: D & when @(Log(y)) => @(l + y) | y: D => @(y!)",               "1,2,3",     "123#2;3"),
			// A name only the steps capture, text and a rule's value.
			("l: F & ',' & z: D & when @(Log(z)) => @(l + z) | y: D => @(y!)",               "1,2,3",     "123#2;3"),
			("l: F & ',' & r: V & when @(Log(r)) => @(l + r) | y: D => @(y!)",               "1,2,3",     "12v3v#2v;3v"),
			// Two tails, each its own step.
			("l: F & '+' & y: D & when @(Log(y)) => @(l + y) | " +
				"l: F & '-' & y: D & when @(Log(y)) => @(l + \"m\" + y) | y: D => @(y!)",    "1+2-3+4",   "12m34#2;3;4"),
			// A repetition inside a step: joined within the step, by the guard and the `=>`.
			("l: F & ',' & (y: D & ';'?){2} & when @(Log(y)) => @(l + y) | y: D => @(y!)",   "1,2;3,4;5", "12345#23;45"),
			("l: F & ',' & (y: ?=D & D & ';'?){2} & when @(Log(y)) => @(l + y) | y: D => @(y!)", "1,2;3,4;5", "12345#23;45"),
			("l: F & ',' & (y: D & ';'?){2} => @(l + y) | y: D => @(y!)",                    "1,2;3,4;5", "12345#"),
			("l: F & ',' & (y: ?=(D & D?) & D){2} & when @(Log(y)) => @(l + y) | y: D => @(y!)", "1,23",    "1233#233"),
			// A rule's values repeated inside a step are the step's sequence (§7.3), to both.
			("l: F & ',' & (r: V & ';'?){2} & when @(LogA(r)) => @(l + string.Concat(r)) | y: D => @(y!)",
				"1,2;3,4;5", "12v3v4v5v#2v/3v;4v/5v"),
			("l: F & ',' & (r: V & ';'?){2} => @(l + string.Concat(r)) | y: D => @(y!)",     "1,2;3,4;5", "12v3v4v5v#"),
			// Absent in this step is absent, whatever the base wrote.
			("l: F & ',' & (y: D)? & 'x' & when @(Log(y)) => @(l + (y ?? \"_\")) | y: D => @(y!)", "1,x,2x", "1_2#<null>;2"),
			("l: F & ',' & (r: V)? & 'x' & when @(Log(r)) => @(l + (r ?? \"_\")) | r: V => @(r!)", "1,x,2x", "1v_2v#<null>;2v"),
			// A step that does not name its rule is not handed the value so far (§4.3).
			("F & ',' & y: D => @(y!) | y: D => @(y!)",                                       "1,2,3",     "3#"),
			("F & ',' & y: D & when @(Log(y)) => @(y!) | y: D => @(y!)",                      "1,2,3",     "3#2;3"),
			("l: F & '+' & y: D => @(l + y) | F & '-' & y: D => @(\"m\" + y) | y: D => @(y!)", "1+2-3+4",   "m34#"),
			// The leading capture is the value so far (§4.3), and a guard may name it (§3.6).
			("l: F & ',' & y: D & when @(Log(l)) => @(l + y) | y: D => @(y!)",               "1,2,3",     "123#1;12"),
			("l: F & ',' & r: V & when @(Log(l + r)) => @(l + r) | y: D => @(y!)",           "1,2,3",     "12v3v#12v;12v3v"),
			("l: F & '+' & y: D & when @(Log(l)) => @(l + y) | " +
				"l: F & '-' & y: D & when @(Log(l)) => @(l + \"m\" + y) | y: D => @(y!)",    "1+2-3+4",   "12m34#1;12;12m3"),
			("l: F & ',' & y: D & when @(Log(l)) => @(l + y) | l: D => @(l! + \"b\")",       "1,2,3",     "1b23#1b;1b2"),
		})
		{
			var grammar =
				"D = ['0'..'9']\nV : @string = d: D => @(d! + \"v\")\nF : @string = " + rule +
				"\nT : @string = f: F => @(f + \"#\" + Take())";
			var match = EmittedCode.Match(Compile(grammar, carrier, direct, find), "Grammar", "TryParseT", input);
			var answer = match.IsSuccess ? (string)match.Value! : "<refused " + match.Error + ">";

			if (answer != expected)
				wrong.Add(rule + " on '" + input + "': " + answer + ", not " + expected);
		}

		Assert.True(wrong.Count == 0, string.Join("\n", wrong));
	}

	/// <summary>
	/// A guard that refuses a step on the value so far ends the fold there, and what follows
	/// reads on from where the last step it let through ended.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_fold_step_guard_can_refuse_on_the_value_so_far(CarrierKind carrier, bool direct, bool find)
	{
		var assembly = Compile(
			"""
			D = ['0'..'9']
			F : @string = l: F & ',' & y: D & when @(l.Length < 2) => @(l + y) | y: D => @(y!)
			T : @string = f: F & ',' & r: D => @(f + "|" + r)
			""", carrier, direct, find);

		var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", "1,2,3");
		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal("12|3", match.Value);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseT", "1,2,3,4").IsSuccess);
	}

	/// <summary>
	/// A value so far a guard asked for is built once (§3.6): the next step's guard and the
	/// fold at acceptance go on from it rather than folding from the base again.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_fold_step_value_a_guard_asked_for_is_built_once(CarrierKind carrier, bool direct, bool find)
	{
		var assembly = Compile(
			"""
			D = ['0'..'9']
			V : @string = d: D => @(d! + "v")
			F : @string = l: F & ',' & r: V & when @(Log(l)) => @(Made(l + r)) | y: D => @(Made(y!))
			T : @string = f: F => @(f + "#" + Take() + "#" + TakeMade())
			""", carrier, direct, find);

		var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", "1,2,3,4");
		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal("12v3v4v#1;12v;12v3v#4", match.Value);
	}

	/// <summary>
	/// The rule a lexical join keeps on every path (30bccceb): turns that tile — adjacent
	/// tokens — are cut whole, keeping what stood between them, and turns with something
	/// outside the capture between them are cut one by one. The same for the walk at the end
	/// and a guard, a guard inside the loop and a switch's selector, a capture of a token and
	/// of what a lookahead saw.
	/// </summary>
	[Theory]
	[MemberData(nameof(Carriers))]
	public void A_lexical_join_keeps_the_tiling_rule_on_every_path(CarrierKind carrier, bool direct)
	{
		const string head =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Num = ['0'..'9']+
			}

			""";
		var wrong = new List<string>();

		foreach (var (rule, input, expected) in new[]
		{
			// The walk and a guard after the loop.
			("(y: Lexical.Num & ','?){2} & when @(Seen(y)) => @(Both(y))",              "1 2",       "1 2|1 2"),
			("(y: Lexical.Num & ','?){2} & when @(Seen(y)) => @(Both(y))",              "1 , 2",     "12|12"),
			("(y: Lexical.Num & ','?){2} & when @(Seen(y)) => @(Both(y))",              "1,2",       "12|12"),
			("(y: Lexical.Num & ','?){2} => @(y)",                                      "1 2",       "1 2"),
			("(y: Lexical.Num & ','?){2} => @(y)",                                      "1 , 2",     "12"),
			("y: Lexical.Num+ & when @(Seen(y)) => @(Both(y))",                         "1 2 3",     "1 2 3|1 2 3"),
			// What a lookahead saw.
			("(y: ?=Lexical.Num & Lexical.Num & ','?){2} & when @(Seen(y)) => @(Both(y))", "1 2",    "1 2|1 2"),
			("(y: ?=Lexical.Num & Lexical.Num & ','?){2} & when @(Seen(y)) => @(Both(y))", "1 , 2",  "12|12"),
			// A guard inside the loop, at every turn.
			("(y: Lexical.Num & ','? & when @(Log(y))){3} => @(Take())",                "1 2 3",     "1;1 2;1 2 3"),
			("(y: Lexical.Num & ','? & when @(Log(y))){3} => @(Take())",                "1 , 2 , 3", "1;12;123"),
			// A switch's selector.
			("(y: Lexical.Num & ','?){2} & switch @(y) { case \"1 2\": 'a' case \"12\": 'b' } => @(y)", "1 2 a",   "1 2"),
			("(y: Lexical.Num & ','?){2} & switch @(y) { case \"1 2\": 'a' case \"12\": 'b' } => @(y)", "1 , 2 b", "12"),
		})
		{
			var result = GramCompiler.Compile(head + "T : @string = " + rule + "\nparse T", new GramCompilerOptions
			{
				ClassName     = "Grammar",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Carrier       = carrier,
				Direct        = direct,
				Lexical       = true,
			});
			Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);
			var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Members);
			var match    = EmittedCode.Match(assembly, "Grammar", "TryParseT", input);
			var answer   = match.IsSuccess ? (string)match.Value! : "<refused " + match.Error + ">";

			if (answer != expected)
				wrong.Add(rule + " on '" + input + "': " + answer + ", not " + expected);
		}

		Assert.True(wrong.Count == 0, string.Join("\n", wrong));
	}

	/// <summary>
	/// A name a fold step collects and another alternative holds as one value has two types,
	/// and is refused as any such name is, saying which is which. Before, the step silently
	/// took the last of what it collected.
	/// </summary>
	[Theory]
	[InlineData("l: F & ',' & (r: V & ';'?){2} => @(l + string.Concat(r)) | r: V => @(r!)")]
	[InlineData("l: F & '+' & (r: V & ';'?){2} => @(l + string.Concat(r)) | l: F & '-' & r: V => @(l + r) | y: D => @(y!)")]
	public void A_fold_step_collecting_what_another_alternative_holds_is_refused(string rule)
	{
		var result = GramCompiler.Compile(
			"D = ['0'..'9']\nV : @string = d: D => @(d! + \"v\")\nF : @string = " + rule + "\nparse F",
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		var refused = Assert.Single(result.Diagnostics, static one => one.Id == GrammarNormalizer.CaptureTypeMismatch);
		Assert.Contains("a sequence of 'V'", refused.Message);
		Assert.Contains("the value of 'V'", refused.Message);
	}

	public static IEnumerable<object[]> Carriers()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				yield return [carrier, direct];
	}

	public static IEnumerable<object[]> Hosts()
	{
		foreach (var host in new[] { "spans", "buffered", "bytes", "lexical" })
			foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
				foreach (var direct in new[] { false, true })
					yield return [host, carrier, direct];
	}

	/// <summary>
	/// What a host asks of the reading, and a grammar cannot: a borrowed span instead of a
	/// string, a buffered reader over characters or bytes, and the lexical split, where a
	/// piece is a token and is cut from the source rather than from the input.
	/// </summary>
	[Theory]
	[MemberData(nameof(Hosts))]
	public void The_guard_sees_the_join_whatever_the_host_asks(string host, CarrierKind carrier, bool direct)
	{
		var grammar = host == "lexical"
			? """
				trivia = { ' '* }
				namespace Lexical
				{
					trivia = none
					Num = ['0'..'9']+
				}
				T : @string = (y: Lexical.Num & ','?){2} & when @(y.ToString() == "12") => @(y.ToString())
				U : @string = (y: ?=Lexical.Num & Lexical.Num & ','?){2} & when @(y.ToString() == "12") => @(y.ToString())
				parse T
				parse U
				"""
			: """
				D = ['0'..'9']
				T : @string = (y: D & ','?){2} & when @(y.ToString() == "12") => @(y.ToString())
				U : @string = (y: ?=D & D & ','?){2} & when @(y.ToString() == "12") => @(y.ToString())
				parse T
				parse U
				""";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
			Direct        = direct,
			SpanCaptures  = host == "spans",
			BufferedInput = host is "buffered" or "bytes",
			BufferedBytes = host == "bytes",
			Lexical       = host == "lexical",
		});
		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);
		var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text);

		foreach (var input in host == "lexical" ? new[] { "1,2", "1 , 2" } : new[] { "1,2", "12" })
			foreach (var method in new[] { "TryParseT", "TryParseU" })
			{
				var match = EmittedCode.Match(assembly, "Grammar", method, input);
				Assert.True(match.IsSuccess, method + " " + input + ": " + match.Error);
				Assert.Equal("12", match.Value);
			}
	}

	static Assembly Compile(string grammar, CarrierKind carrier, bool direct, bool find)
	{
		var result = GramCompiler.Compile(grammar + "\nparse T" + (find ? "\nfind T" : ""), new GramCompilerOptions
		{
			ClassName = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier = carrier,
			Direct = direct,
		});
		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);
		return EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Members);
	}
}
