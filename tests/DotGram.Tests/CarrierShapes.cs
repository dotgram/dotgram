using System;

namespace DotGram.Tests;

/// <summary>
/// The shapes the immediate carrier had to learn one at a time, with inputs each accepts and
/// refuses: held against the tape by <c>CarrierTests</c>, and read for their refusals by
/// <c>RefusalTests</c> here and in <c>DotGram.Tests.Slow</c>, which links this file.
/// </summary>
static class CarrierShapes
{
	internal static readonly (string Name, string Grammar, string[] Inputs)[] All =
	[
		("a fold",
			"trivia = ' '*\n" +
			"Start : @string = l: Start & '+' & r: Pair => @(l + \"+\" + r)\n" +
			"                | one: Pair => @(one)\n" +
			"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1 + bb = 22", "a=1", "a = 1 + bb", "a = ", ""]),

		("a cycle",
			"trivia = ' '*\n" +
			"Start : @string = l: Start & '+' & r: Pair => @(l + \"+\" + r)\n" +
			"                | one: Pair => @(one)\n" +
			"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
			"               | '(' & inner: Start & ')' => @(\"(\" + inner + \")\")\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1 + (b = 2 + c = 3)", "((a = 1))", "(a = 1", "a = 1 + (b = 2"]),

		("records gathered",
			"Start : @string = first: Name & (',' & rest: Name)* => @(first + \"|\" + string.Join(\"|\", rest))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a,b,c", "a", "a,", ",a"]),

		("text gathered",
			"Start : @string = (parts: ['a'..'z']+ & ','?)+ => @(parts)\n" +
			"parse Start\n",
			["ab,cd,e", "ab", "ab,,cd", ""]),

		// An extent has no record to be the span of and needs none: the two positions are
		// the reader's own locals. Read here beside a rule that builds and a rule that
		// collects, because what a carrier does to one it must do to all three.
		("an extent",
			"trivia = ' '*\n" +
			"Where : @SourceSpan = ['a'..'z']+\n" +
			"Name  : @string     = t: ['a'..'z']+ => @(t)\n" +
			"Pair  : @string     = n: Name & '=' & w: Where => @(n + \"@\" + w.Start + \":\" + w.Length)\n" +
			"Start : @string     = first: Pair & (',' & rest: Pair)* => @(first + string.Join(\"|\", rest))\n" +
			"parse Start\n",
			["ab=cd", "ab = cd , ef = gh", "ab=", "", "ab=cd,"]),

		("a guard over a record",
			"Start : @string = d: Digits & when @(d.Length < 3) => @(\"<\" + d + \">\")\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["12", "1234", "", "x"]),

		("marks laid over an operand",
			"state : @int\n" +
			"Start : @string = '(' & inner: Start with state @(9) & ';' => @(\"<\" + inner + \">\")\n" +
			"                | '(' & inner: Start with state @(1) & ')' => @(\"(\" + inner + \")\")\n" +
			"                | t: ['a'..'z']+ => @(string.Join(\",\", parserState.ToArray()) + \":\" + t)\n" +
			"parse Start\n",
			["a", "(a)", "((a))", "(a;", "(a"]),

		("two captures of one folding rule",
			"trivia = ' '*\n" +
			"Start : @string = l: Sum & '=' & r: Sum => @(l + \"/\" + r)\n" +
			"Sum : @string = a: Sum & '+' & b: Name => @(a + \"+\" + b)\n" +
			"              | one: Name => @(one)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a = b", "a + b = c", "a = b + c", "a + b = c + d", "a ="]),

		("a rule that reaches itself",
			"trivia = ' '*\n" +
			"Start : @string = '(' & inner: Start & ')' => @(\"(\" + inner + \")\")\n" +
			"                | t: Name => @(t)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a", "(a)", "( ( a ) )", "(a", ""]),

		("a rule that builds two ways",
			"Start : @string = a: Name & b: Digits? => @(\"n\" + a + (b ?? \"-\"))\n" +
			"                | d: Digits & c: Name? => @(\"d\" + d + (c ?? \"-\"))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["ab12", "12ab", "ab", "12", "", "1a2"]),

		("a member that may be missing",
			"Start : @string = a: Name & b: Digits? => @(a + (b ?? \"-\"))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["ab12", "ab", "12", ""]),
	];
}
