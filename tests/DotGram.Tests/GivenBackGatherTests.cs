using System;
using System.Collections.Generic;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule that fails gives back what it gathered. The immediate carrier gathers onto a stack
/// per type, and a rule's record takes what was pushed since the rule began — so a rule that
/// pushed some of its turns and then failed has to leave the stacks as it found them, or the
/// next rule to collect on one of them takes its leavings: an argument list abandoned at a bad
/// argument, read again as one word by the alternative after it, left its arguments to the
/// postfix list of the expression around it. The tape's references carry the slot they were
/// pushed for and never meet; the answer has to be the same on every carrier.
/// </summary>
/// <remarks>
/// <c>F</c> gathers values onto the string stack and <c>G</c> pieces of text onto the text
/// stack, each across the turns of an argument list; <c>C</c> reads the same text as one word
/// where the list is left open, and <c>E</c> around it gathers on both stacks. <c>X</c> lets
/// <c>F</c> stand in its own argument list, so that the rule to collect next is the failed
/// rule itself.
/// </remarks>
public sealed class GivenBackGatherTests
{
	const string Rules = """
		D = ['0'..'9']
		V : @string = d: D => @(d! + "v")
		W = ['a'..'z']
		E : @string = p: C & zones: Z* & (';' & tail: W)* => @(p + "[" + string.Join(",", zones) + "]" + tail)
		Z : @string = '@' & v: V => @(v)
		C : @string = '(' & v: E & ')' => @(v)
		  | f: F => @(f)
		  | g: G => @(g)
		  | 'H' & ['(' | ',' | '0'..'9']* => @("h")
		  | 'K' & ['(' | ',' | 'a'..'z']* => @("k")
		  | 'T' & (',' & F)* & ['(' | ',' | 'H' | '0'..'9']* => @("t")
		  | v: V => @(v)
		F : @string = 'I' & '(' & a: V & ',' & b: V & ')' => @(a + b)
		  | 'H' & '(' & at: V & (',' & rest: X)* & ')' => @(at + string.Join("", rest))
		  | 'J' & '(' & ')' => @("j")
		G : @string = 'K' & '(' & (',' & rest: W)* & ')' => @(rest)
		X : @string = f: F => @(f)
		  | 'H' & ['(' | ',' | '0'..'9']* => @("h")
		  | v: V => @(v)
		parse E
		""";

	[Theory]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Immediate)]
	public void A_rule_that_fails_leaves_the_stacks_as_it_found_them(CarrierKind carrier)
	{
		var host  = Host(carrier);
		var wrong = new List<string>();

		foreach (var (input, expected) in new (string Input, string Expected)[]
		{
			// Lists that close: what each gathered is its own.
			("H(1,2,3)",       "1v2v3v[]"),
			("I(1,2)",         "1v2v[]"),
			("K(,a,b)",        "ab[]"),
			// A list left open after two turns, read again as one word: the turns are not
			// the postfix list of the expression around it, nor its text.
			("H(1,2,3,",       "h[]"),
			("H(1,2,3,@4",     "h[4v]"),
			("(H(1,2,3,)",     "h[][]"),
			("K(,a,b,",        "k[]"),
			("K(,a,b,;c",      "k[]c"),
			("(K(,a,b,);c",    "k[][]c"),
			("H(1,2,3,@4;c;d", "h[4v]cd"),
			// The failed rule's own list, around the one it failed in.
			("H(1,H(2,3,,)",   "1vh[]"),
			// Failed in a turn whose rule gathers nothing, and read on past the turn.
			("T,H(1,2,,",      "t[]"),
		})
		{
			var answer = Answer(host, input);

			if (answer != expected)
				wrong.Add($"'{input}': {answer}, not {expected}");
		}

		Assert.True(wrong.Count == 0, $"{carrier}:\n" + string.Join("\n", wrong));
	}

	/// <remarks>Read by methods on every carrier: the immediate carrier refuses any other reading.</remarks>
	static Type Host(CarrierKind carrier)
	{
		var result = GramCompiler.Compile(Rules, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
			Direct        = true,
		});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		// Asked for, the immediate carrier has to be the one reading: refused, it would be the
		// tape answering for it.
		if (carrier == CarrierKind.Immediate)
			Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GramCompiler.CarrierRefused);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text).GetType("Grammar")!;
	}

	static string Answer(Type host, string input)
	{
		var match = host.GetMethod("TryParseE", [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!
			? (string)match.GetType().GetProperty("Value")!.GetValue(match)!
			: "<refused " + match.GetType().GetProperty("Error")!.GetValue(match) + ">";
	}
}
