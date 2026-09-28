using System;
using System.Collections.Generic;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

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
		static bool Log(string value) { log += (log.Length > 0 ? ";" : "") + value; return true; }
		static string Take() { var taken = log; log = ""; return taken; }
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
		})
		{
			var grammar = "D = ['0'..'9']\n" + (body.Contains("Sep") ? "Sep = '-'+\n" : "") +
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
				parse T
				"""
			: """
				D = ['0'..'9']
				T : @string = (y: D & ','?){2} & when @(y.ToString() == "12") => @(y.ToString())
				parse T
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
		{
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", input);
			Assert.True(match.IsSuccess, input + ": " + match.Error);
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
