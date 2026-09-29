using System;
using System.Collections.Generic;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A value an abandoned reading built is not the value of the reading that stands. The immediate
/// carrier builds as it reads and hands a value from callee to caller in a register of its type,
/// so a reading given back has already written that register: the value the caller is handed
/// has to be the one the reading that stands wrote, on every carrier, with the direct reader on
/// or off, parsed or found.
/// </summary>
/// <remarks>
/// <c>D</c> is a digit and <c>V</c> the digit with a <c>v</c> after it; <c>T</c> ends in
/// <c>z: D</c> and answers what it read before it, a bar, and that digit.
/// </remarks>
public sealed class AbandonedValueTests
{
	public static IEnumerable<object[]> Readings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				foreach (var find in new[] { false, true })
					yield return [carrier, direct, find];
	}

	/// <summary>
	/// A fold's value so far is its own after the turns stop, although the step tried last
	/// read a value of the same type before it was given back (§4.3: a step tried and given
	/// back never ran at all).
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_fold_step_given_back_leaves_the_value_so_far(CarrierKind carrier, bool direct, bool find)
	{
		const string Step = "F : @string = l: F & ',' & r: V & '.' => @(l + r) | y: D => @(y!)\n";
		const string Tail = "T : @string = f: F & ',' & z: D => @(f + \"|\" + z)\n";

		Check(carrier, direct, find,
		[
			// The repro: the last step read `4v` and found no dot.
			(Step + Tail,                                                                "1,2.,3.,4",  "12v3v|4"),
			// Given back at the first step: the value so far is the base.
			(Step + Tail,                                                                "1,4",        "1|4"),
			// Given back by a guard after the value was read, not by the text.
			("F : @string = l: F & ',' & r: V & when @(r != \"4v\") => @(l + r) | y: D => @(y!)\n" + Tail,
				"1,2,4",      "12v|4"),
			// Two tails, the one tried last given back after its value was read.
			("F : @string = l: F & '+' & r: V & '.' => @(l + r) | l: F & '-' & r: V & '!' => @(l + \"m\" + r) | y: D => @(y!)\n" +
				"T : @string = f: F & ['+' | '-'] & z: D => @(f + \"|\" + z)\n",     "1+2.-3!-4",  "12vm3v|4"),
			// Read through a rule that only hands it on.
			(Step + "G : @string = g: F => @(g)\nT : @string = f: G & ',' & z: D => @(f + \"|\" + z)\n",
				"1,2.,3.,4",  "12v3v|4"),
			// The base only hands a value on, and so writes nothing of its own.
			("F : @string = l: F & ',' & r: V & '.' => @(l + r) | y: V => @(y)\n" + Tail,
				"1,2.,4",     "1v2v|4"),
			// Written with binding powers: the operand to the right is the rule itself.
			("F : @string = l: F & ',' & r: F & '.' << 1 => @(l + r) | y: D => @(y!)\n" + Tail,
				"1,2.,3.,4",  "123|4"),
		]);
	}

	/// <summary>
	/// Where nothing folds, a value is handed on at the point it was built, and a reading
	/// given back after it cannot reach it: a repetition's turn, an alternative and a
	/// lookahead that each read a value of the same type and were given back.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_reading_given_back_after_a_value_leaves_that_value(CarrierKind carrier, bool direct, bool find)
	{
		const string Forward = "P : @string = p: V => @(p)\n";

		Check(carrier, direct, find,
		[
			// A turn of a repetition.
			(Forward + "T : @string = f: P & (',' & P & '.')* & ',' & z: D => @(f + \"|\" + z)\n", "1,2.,4", "1v|4"),
			// An alternative, given back for the next one.
			("T : @string = (f: V & ',' & V & '.' | f: V & ',') & z: D => @(f + \"|\" + z)\n",   "1,4",    "1v|4"),
			// A negative lookahead that read a value.
			(Forward + "T : @string = f: P & ?!(',' & P & '.') & ',' & z: D => @(f + \"|\" + z)\n", "1,4",   "1v|4"),
			// A rule's own alternative, given back after its value was read.
			("P : @string = p: V & '.' => @(p + \"!\") | p: V => @(p)\n" +
				"T : @string = f: P & ',' & z: D => @(f + \"|\" + z)\n",                         "1,4",    "1v|4"),
		]);
	}

	static void Check(CarrierKind carrier, bool direct, bool find, (string Rules, string Input, string Expected)[] cases)
	{
		var wrong = new List<string>();

		foreach (var (rules, input, expected) in cases)
		{
			var answer = Answer(rules, input, carrier, direct, find);

			if (answer != expected)
				wrong.Add(rules + "on '" + input + "': " + answer + ", not " + expected);
		}

		Assert.True(wrong.Count == 0, string.Join("\n", wrong));
	}

	static string Answer(string rules, string input, CarrierKind carrier, bool direct, bool find)
	{
		var grammar = "D = ['0'..'9']\nV : @string = d: D => @(d! + \"v\")\n" + rules;
		var result  = GramCompiler.Compile(grammar + "parse T" + (find ? "\nfind T" : ""), new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
			Direct        = direct,
		});
		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var match = EmittedCode.Match(EmittedCode.Compile(Assert.Single(result.Sources).Text), "Grammar", "TryParseT", input);

		return match.IsSuccess ? (string)match.Value! : "<refused " + match.Error + ">";
	}
}
