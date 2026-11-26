using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Expression grammars whose rules call themselves past a recognizer of the host that may succeed without moving
/// (docs/syntax.md §7.2): which of several valid splits of the input such a rule answers, in every reading, and that it
/// answers on every valid input.
/// </summary>
/// <remarks>
/// <para>
/// Each grammar comes in two families with the same shape: a calculator, whose value is the number the expression
/// evaluates to, and a counter, whose value is how many numbers it holds. A rule's value shows the part of the input it
/// took: the top rule answers what the self-calling rule returned, how many tails its continuation read after it and
/// their value, and the sum of the two, which is the same however the input was split.
/// </para>
/// <para>
/// <c>V030</c> is what the rule written with <c>@Ws &amp;</c> before its call back answered in 0.3.0, compiled here as 0.3.0
/// compiled it (<see cref="Reentry.Unchecked"/>): where several splits fit, the one with the fewest tails in the rule, a
/// consequence of the order its alternatives are tried in rather than of anything the rule says, and where the parse has
/// to give up on the recursive alternative, round for ever. <c>C1</c> is what it answers now: where the recognizer stands
/// at the rule's entry, the rule is read as its twin, and where it moves, as written. <c>Twin</c> is what the same rule
/// without the recognizer answers, read as left recursion is (§4.3), as many tails in the rule as fit. A null
/// <c>V030</c> is an input the old reading goes round on and is not pinned; its recognizer counts its calls and throws past
/// a bound no ending parse comes near, so it cannot hang a run.
/// </para>
/// </remarks>
public sealed class StandingRecursionOrderTests
{
	/// <summary>The recognizer and the reports, over a span.</summary>
	const string SpanHost = Counter + Reports + """
		static bool Ws(System.ReadOnlySpan<char> input, ref int p) { Count(); while (p < input.Length && input[p] == ' ') p++; return true; }
		""";

	/// <summary>The same over the buffered input view.</summary>
	const string ViewHost = Counter + Reports + """
		static bool Ws(ParserInput<char> input, ref int p) { Count(); while (input.Peek(p, out var c) && c == ' ') p++; return true; }
		static bool Ws(ParserInput<byte> input, ref int p) { Count(); while (input.Peek(p, out var c) && c == (byte)' ') p++; return true; }
		""";

	const string Counter = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		public static int Calls() { return calls; }
		static void Count() { if (++calls > 20000) throw new System.InvalidOperationException("went round"); }
		""";

	/// <summary>What the top rule answers: the rule's value, then how many tails its continuation read and their value, then the whole.</summary>
	const string Reports = """
		static string Text(double value) { return value.ToString(System.Globalization.CultureInfo.InvariantCulture); }
		static double Parse(string text) { return double.Parse(text, System.Globalization.CultureInfo.InvariantCulture); }
		static string Report(double e, System.Collections.IEnumerable tail)
		{
			var k = 0;
			var v = 0.0;
			if (tail != null)
				foreach (var item in tail) { k++; v += (double)item; }
			return Text(e) + " + [" + k + ": " + Text(v) + "] = " + Text(e + v);
		}
		static string Report(int e, System.Collections.IEnumerable tail)
		{
			var k = 0;
			var v = 0;
			if (tail != null)
				foreach (var item in tail) { k++; v += (int)item; }
			return e + " + [" + k + ": " + v + "] = " + (e + v);
		}
		static string List(System.Collections.Generic.IEnumerable<double> items) { return string.Join(" ; ", System.Linq.Enumerable.Select(items, Text)); }
		static string List(System.Collections.Generic.IEnumerable<int> items) { return string.Join(" ; ", items); }
		""";

	/// <summary>The calculator: each rule's value is the number its part of the input evaluates to.</summary>
	static readonly Dictionary<string, string> Calculator = new()
	{
		// The continuation can read the same tails the rule reads.
		["tail"] = """
			T : @string = s: Sum & ('+' & t: Item)* & eof => @(Report(s, t))
			Sum : @double = n: Num => @(Parse(n))
			    | @Ws & s: Sum & '+' & n: Num => @(s + Parse(n))
			Item : @double = n: Num => @(Parse(n))
			Num : string = ['0'..'9']+
			""",

		// Only one number of tails fits what follows.
		["end"] = """
			T : @string = s: Sum & eof => @(Report(s, null))
			Sum : @double = n: Num => @(Parse(n))
			    | @Ws & s: Sum & '+' & n: Num => @(s + Parse(n))
			Num : string = ['0'..'9']+
			""",

		// Parentheses in the operand, and a continuation that reads them too.
		["parens"] = """
			T : @string = e: Sum & ('+' & t: Atom)* & eof => @(Report(e, t))
			Sum : @double = a: Atom => @(a)
			    | @Ws & l: Sum & '+' & r: Atom => @(l + r)
			Atom : @double = n: Num => @(Parse(n))
			    | '(' & e: Sum & ')' => @(e)
			Num : string = ['0'..'9']+
			""",

		// Precedence levels: the additive level calls itself past the recognizer, the multiplicative one is plain left recursion.
		["levels"] = """
			T : @string = e: Add & eof => @(Report(e, null))
			Add : @double = m: Mul => @(m)
			    | @Ws & l: Add & '+' & r: Mul => @(l + r)
			    | @Ws & l: Add & '-' & r: Mul => @(l - r)
			Mul : @double = u: Unary => @(u)
			    | l: Mul & '*' & r: Unary => @(l * r)
			    | l: Mul & '/' & r: Unary => @(l / r)
			Unary : @double = '-' & u: Unary => @(-u)
			    | a: Atom => @(a)
			Atom : @double = n: Num => @(Parse(n))
			    | '(' & e: Add & ')' => @(e)
			Num : string = ['0'..'9']+
			""",

		// A base alternative written after the recursive one.
		["base"] = """
			T : @string = s: Sum & ('+' & t: Item)* & eof => @(Report(s, t))
			Sum : @double = n: Num => @(Parse(n))
			    | @Ws & s: Sum & '+' & n: Num => @(s + Parse(n))
			    | '(' & s: Sum & ')' => @(s)
			Item : @double = n: Num => @(Parse(n))
			Num : string = ['0'..'9']+
			""",

		// A list of expressions, where a binary minus and a unary one split the input differently.
		["list"] = """
			T : @string = es: Add+ & eof => @(List(es))
			Add : @double = a: Atom => @(a)
			    | @Ws & l: Add & '-' & r: Atom => @(l - r)
			Atom : @double = n: Num => @(Parse(n))
			    | '-' & a: Atom => @(-a)
			    | '(' & e: Add & ')' => @(e)
			Num : string = ['0'..'9']+
			""",
	};

	/// <summary>The counter: each rule's value is how many numbers its part of the input holds.</summary>
	static readonly Dictionary<string, string> Count = new()
	{
		["tail"] = """
			T : @string = s: Sum & ('+' & t: Item)* & eof => @(Report(s, t))
			Sum : @int = Num => @(1)
			    | @Ws & s: Sum & '+' & Num => @(s + 1)
			Item : @int = Num => @(1)
			Num : string = ['0'..'9']+
			""",

		["end"] = """
			T : @string = s: Sum & eof => @(Report(s, null))
			Sum : @int = Num => @(1)
			    | @Ws & s: Sum & '+' & Num => @(s + 1)
			Num : string = ['0'..'9']+
			""",

		["parens"] = """
			T : @string = e: Sum & ('+' & t: Atom)* & eof => @(Report(e, t))
			Sum : @int = a: Atom => @(a)
			    | @Ws & l: Sum & '+' & r: Atom => @(l + r)
			Atom : @int = Num => @(1)
			    | '(' & e: Sum & ')' => @(e)
			Num : string = ['0'..'9']+
			""",

		["levels"] = """
			T : @string = e: Add & eof => @(Report(e, null))
			Add : @int = m: Mul => @(m)
			    | @Ws & l: Add & '+' & r: Mul => @(l + r)
			    | @Ws & l: Add & '-' & r: Mul => @(l + r)
			Mul : @int = u: Unary => @(u)
			    | l: Mul & '*' & r: Unary => @(l + r)
			    | l: Mul & '/' & r: Unary => @(l + r)
			Unary : @int = '-' & u: Unary => @(u)
			    | a: Atom => @(a)
			Atom : @int = Num => @(1)
			    | '(' & e: Add & ')' => @(e)
			Num : string = ['0'..'9']+
			""",

		["base"] = """
			T : @string = s: Sum & ('+' & t: Item)* & eof => @(Report(s, t))
			Sum : @int = Num => @(1)
			    | @Ws & s: Sum & '+' & Num => @(s + 1)
			    | '(' & s: Sum & ')' => @(s)
			Item : @int = Num => @(1)
			Num : string = ['0'..'9']+
			""",

		["list"] = """
			T : @string = es: Add+ & eof => @(List(es))
			Add : @int = a: Atom => @(a)
			    | @Ws & l: Add & '-' & r: Atom => @(l + r)
			Atom : @int = Num => @(1)
			    | '-' & a: Atom => @(a)
			    | '(' & e: Add & ')' => @(e)
			Num : string = ['0'..'9']+
			""",
	};

	/// <summary>
	/// Every input with what the marked rule answered in 0.3.0 (a null goes round), what it answers now, and what its twin
	/// answers: the two agree except where the recognizer moves at the rule's entry (<c>" 1+1"</c>, <c>" 1"</c>).
	/// </summary>
	static readonly (string Family, string Grammar, string Input, string? V030, string C1, string Twin)[] Rows =
	[
		("calculator", "tail",   "1",         "ok@1 1 + [0: 0] = 1",   "ok@1 1 + [0: 0] = 1",   "ok@1 1 + [0: 0] = 1"),
		("calculator", "tail",   "1+1",       "ok@3 1 + [1: 1] = 2",   "ok@3 2 + [0: 0] = 2",   "ok@3 2 + [0: 0] = 2"),
		("calculator", "tail",   "1+2+3",     "ok@5 1 + [2: 5] = 6",   "ok@5 6 + [0: 0] = 6",   "ok@5 6 + [0: 0] = 6"),
		("calculator", "tail",   "12+3",      "ok@4 12 + [1: 3] = 15", "ok@4 15 + [0: 0] = 15", "ok@4 15 + [0: 0] = 15"),
		("calculator", "tail",   " 1+1",      "ok@4 2 + [0: 0] = 2",   "ok@4 2 + [0: 0] = 2",   "no@0"),
		("calculator", "tail",   " 1",        null,                    "no@2",                  "no@0"),
		("calculator", "tail",   "1+",        null,                    "no@2",                  "no@2"),
		("count",      "tail",   "1",         "ok@1 1 + [0: 0] = 1",   "ok@1 1 + [0: 0] = 1",   "ok@1 1 + [0: 0] = 1"),
		("count",      "tail",   "1+1",       "ok@3 1 + [1: 1] = 2",   "ok@3 2 + [0: 0] = 2",   "ok@3 2 + [0: 0] = 2"),
		("count",      "tail",   "1+1+1",     "ok@5 1 + [2: 2] = 3",   "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3"),
		("count",      "tail",   "12+3",      "ok@4 1 + [1: 1] = 2",   "ok@4 2 + [0: 0] = 2",   "ok@4 2 + [0: 0] = 2"),
		("count",      "tail",   " 1+1",      "ok@4 2 + [0: 0] = 2",   "ok@4 2 + [0: 0] = 2",   "no@0"),
		("count",      "tail",   "1+",        null,                    "no@2",                  "no@2"),

		("calculator", "end",    "1+2+3",     "ok@5 6 + [0: 0] = 6",   "ok@5 6 + [0: 0] = 6",   "ok@5 6 + [0: 0] = 6"),
		("calculator", "end",    " 1+1",      "ok@4 2 + [0: 0] = 2",   "ok@4 2 + [0: 0] = 2",   "no@0"),
		("calculator", "end",    "1+",        null,                    "no@2",                  "no@2"),
		("count",      "end",    "1+2+3",     "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3"),
		("count",      "end",    " 1+1",      "ok@4 2 + [0: 0] = 2",   "ok@4 2 + [0: 0] = 2",   "no@0"),
		("count",      "end",    "1+",        null,                    "no@2",                  "no@2"),

		("calculator", "parens", "1+2",       "ok@3 1 + [1: 2] = 3",   "ok@3 3 + [0: 0] = 3",   "ok@3 3 + [0: 0] = 3"),
		("calculator", "parens", "(1+2)+3",   "ok@7 3 + [1: 3] = 6",   "ok@7 6 + [0: 0] = 6",   "ok@7 6 + [0: 0] = 6"),
		("calculator", "parens", "1+(2+3)",   "ok@7 1 + [1: 5] = 6",   "ok@7 6 + [0: 0] = 6",   "ok@7 6 + [0: 0] = 6"),
		("calculator", "parens", "(1+2+3)",   "ok@7 6 + [0: 0] = 6",   "ok@7 6 + [0: 0] = 6",   "ok@7 6 + [0: 0] = 6"),
		("calculator", "parens", "(1+2",      null,                    "no@4",                  "no@4"),
		("count",      "parens", "1+2",       "ok@3 1 + [1: 1] = 2",   "ok@3 2 + [0: 0] = 2",   "ok@3 2 + [0: 0] = 2"),
		("count",      "parens", "(1+2)+3",   "ok@7 2 + [1: 1] = 3",   "ok@7 3 + [0: 0] = 3",   "ok@7 3 + [0: 0] = 3"),
		("count",      "parens", "1+(2+3)",   "ok@7 1 + [1: 2] = 3",   "ok@7 3 + [0: 0] = 3",   "ok@7 3 + [0: 0] = 3"),
		("count",      "parens", "(1+2+3)",   "ok@7 3 + [0: 0] = 3",   "ok@7 3 + [0: 0] = 3",   "ok@7 3 + [0: 0] = 3"),

		// The levels are not ambiguous: one split fits, and every reading takes it where it ends. In 0.3.0 the second
		// recursive alternative is never reached, so a binary minus goes round.
		("calculator", "levels", "1+2*3",     "ok@5 7 + [0: 0] = 7",   "ok@5 7 + [0: 0] = 7",   "ok@5 7 + [0: 0] = 7"),
		("calculator", "levels", "1*2+3",     "ok@5 5 + [0: 0] = 5",   "ok@5 5 + [0: 0] = 5",   "ok@5 5 + [0: 0] = 5"),
		("calculator", "levels", "8/4/2",     "ok@5 1 + [0: 0] = 1",   "ok@5 1 + [0: 0] = 1",   "ok@5 1 + [0: 0] = 1"),
		("calculator", "levels", "-2*3",      "ok@4 -6 + [0: 0] = -6", "ok@4 -6 + [0: 0] = -6", "ok@4 -6 + [0: 0] = -6"),
		("calculator", "levels", "(1+2)*3",   "ok@7 9 + [0: 0] = 9",   "ok@7 9 + [0: 0] = 9",   "ok@7 9 + [0: 0] = 9"),
		("calculator", "levels", "2*(3+4)",   "ok@7 14 + [0: 0] = 14", "ok@7 14 + [0: 0] = 14", "ok@7 14 + [0: 0] = 14"),
		("calculator", "levels", "1-2",       null,                    "ok@3 -1 + [0: 0] = -1", "ok@3 -1 + [0: 0] = -1"),
		("count",      "levels", "1+2*3",     "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3"),
		("count",      "levels", "3+6*(5/9)", "ok@9 4 + [0: 0] = 4",   "ok@9 4 + [0: 0] = 4",   "ok@9 4 + [0: 0] = 4"),
		("count",      "levels", "8/4/2",     "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3"),
		("count",      "levels", "1-2",       null,                    "ok@3 2 + [0: 0] = 2",   "ok@3 2 + [0: 0] = 2"),

		// In 0.3.0 the base after the recursive alternative is never reached where the recognizer stands.
		("calculator", "base",   "1+2",       "ok@3 1 + [1: 2] = 3",   "ok@3 3 + [0: 0] = 3",   "ok@3 3 + [0: 0] = 3"),
		("calculator", "base",   "5",         "ok@1 5 + [0: 0] = 5",   "ok@1 5 + [0: 0] = 5",   "ok@1 5 + [0: 0] = 5"),
		("calculator", "base",   "(1+2)+3",   null,                    "ok@7 6 + [0: 0] = 6",   "ok@7 6 + [0: 0] = 6"),
		("count",      "base",   "1+2",       "ok@3 1 + [1: 1] = 2",   "ok@3 2 + [0: 0] = 2",   "ok@3 2 + [0: 0] = 2"),
		("count",      "base",   "(1+2)+3",   null,                    "ok@7 3 + [0: 0] = 3",   "ok@7 3 + [0: 0] = 3"),

		// The list asks for one more expression at the end, and in 0.3.0 the marked rule goes round there on every input.
		("calculator", "list",   "1",         null,                    "ok@1 1",                "ok@1 1"),
		("calculator", "list",   "1-2",       null,                    "ok@3 -1",               "ok@3 -1"),
		("calculator", "list",   "1--2",      null,                    "ok@4 3",                "ok@4 3"),
		("count",      "list",   "1-2-3",     null,                    "ok@5 3",                "ok@5 3"),
		("count",      "list",   "(1-2)-3",   null,                    "ok@7 3",                "ok@7 3"),
	];

	/// <summary>The readings: by methods or the engine, carried immediately or deferred, over a span or the buffered view.</summary>
	static readonly (bool Direct, bool Tape, bool Buffered)[] Readings =
	[
		.. from direct in new[] { true, false }
		   from tape in new[] { false, true }
		   from buffered in new[] { false, true }
		   select (direct, tape, buffered),
	];

	public static TheoryData<string, string> Grammars()
	{
		var grammars = new TheoryData<string, string>();

		foreach (var (family, grammar) in Rows.Select(static row => (row.Family, row.Grammar)).Distinct())
			grammars.Add(family, grammar);

		return grammars;
	}

	/// <summary>
	/// The marked rule answers its twin's split where the recognizer stands and reads as written where it moves, in
	/// every reading, over a string and, buffered, over a reader too; read as in 0.3.0 it answers the fewest tails where
	/// it ends, and its twin answers its own.
	/// </summary>
	[Theory]
	[MemberData(nameof(Grammars))]
	public void A_rule_past_a_recognizer_that_stands_answers_one_split_in_every_reading(string family, string name)
	{
		var marked = Grammar(family, name);
		var twin   = marked.Replace("@Ws & ", "", StringComparison.Ordinal);
		var rows   = Rows.Where(row => row.Family == family && row.Grammar == name).ToArray();
		var wrong  = new List<string>();

		foreach (var (direct, tape, buffered) in Readings)
		{
			var reading = $"{(direct ? "direct" : "engine")}/{(tape ? "tape" : "immediate")}/{(buffered ? "view" : "span")}";
			var read    = Build(marked, direct, tape, buffered, lexical: false, out _);
			var v030    = Unchecked(() => Build(marked, direct, tape, buffered, lexical: false, out _));
			var other   = Build(twin, direct, tape, buffered, lexical: false, out _);

			foreach (var row in rows)
				foreach (var over in buffered ? new[] { typeof(string), typeof(TextReader) } : [typeof(string)])
				{
					Check(read, row.C1, "C1");
					Check(other, row.Twin, "twin");

					if (row.V030 is not null)
						Check(v030, row.V030, "0.3.0");

					void Check(Assembly parser, string expected, string what)
					{
						var answer = Answer(parser, row.Input, over);

						if (answer != expected)
							wrong.Add($"{family} {name} {what} {reading} {over.Name} \"{row.Input}\": {answer}, expected {expected}");
					}
				}
		}

		Assert.True(wrong.Count == 0, string.Join("\n", wrong));
	}

	/// <summary>Compiled as 0.3.0 compiles it: no check at the entry, no rule read past a prefix that stands.</summary>
	static T Unchecked<T>(Func<T> build)
	{
		var was = Reentry.Unchecked;

		Reentry.Unchecked = true;

		try
		{
			return build();
		}
		finally
		{
			Reentry.Unchecked = was;
		}
	}

	/// <summary>
	/// Asked for tokens, a recognizer in syntactic position keeps the grammar over characters (GRAM5004), so the marked
	/// rule answers as over characters, its trivia between the items, read past the recognizer and the seam after it;
	/// the twin is cut into tokens and answers its own.
	/// </summary>
	[Theory]
	[InlineData("calculator", "tail",   "1+1",         "ok@3 1 + [1: 1] = 2",  "ok@3 2 + [0: 0] = 2", "ok@3 2 + [0: 0] = 2")]
	[InlineData("calculator", "tail",   " 1+1",        "ok@4 1 + [1: 1] = 2",  "ok@4 2 + [0: 0] = 2", "ok@4 2 + [0: 0] = 2")]
	[InlineData("calculator", "tail",   "1 +1",        "ok@4 1 + [1: 1] = 2",  "ok@4 2 + [0: 0] = 2", "ok@4 2 + [0: 0] = 2")]
	[InlineData("calculator", "tail",   "1+",          null,                   "no@2", "no@2")]
	[InlineData("count",      "tail",   "1+1+1",       "ok@5 1 + [2: 2] = 3",  "ok@5 3 + [0: 0] = 3", "ok@5 3 + [0: 0] = 3")]
	[InlineData("count",      "tail",   " 1",          "ok@2 1 + [0: 0] = 1",  "ok@2 1 + [0: 0] = 1", "ok@2 1 + [0: 0] = 1")]
	[InlineData("calculator", "levels", "1 + 2 * 3",   "ok@9 7 + [0: 0] = 7",  "ok@9 7 + [0: 0] = 7", "ok@9 7 + [0: 0] = 7")]
	[InlineData("calculator", "levels", "1 - 2",       null,                   "ok@5 -1 + [0: 0] = -1", "ok@5 -1 + [0: 0] = -1")]
	[InlineData("count",      "levels", "(1 + 2) * 3", "ok@11 3 + [0: 0] = 3", "ok@11 3 + [0: 0] = 3", "ok@11 3 + [0: 0] = 3")]
	public void Over_tokens_the_marked_rule_stays_over_characters(string family, string name, string input, string? v030, string c1, string twin)
	{
		var marked = Lexical(Grammar(family, name));
		var wrong  = new List<string>();

		foreach (var direct in new[] { true, false })
			foreach (var buffered in new[] { false, true })
			{
				var parser = Build(marked, direct, tape: false, buffered, lexical: true, out var told);
				var old    = Unchecked(() => Build(marked, direct, tape: false, buffered, lexical: true, out _));
				var cut    = Build(marked.Replace("@Ws & ", "", StringComparison.Ordinal), direct, tape: false, buffered, lexical: true, out var cutTold);

				Assert.Contains(told, static one => one.Id == GramCompiler.NotCut);
				Assert.DoesNotContain(cutTold, static one => one.Id == GramCompiler.NotCut);

				Check(parser, c1, "C1");
				Check(cut, twin, "twin");

				if (v030 is not null)
					Check(old, v030, "0.3.0");

				void Check(Assembly built, string expected, string what)
				{
					var answer = Answer(built, input, typeof(string));

					if (answer != expected)
						wrong.Add($"{family} {name} {what} {(direct ? "direct" : "engine")}/{(buffered ? "view" : "span")} \"{input}\": {answer}, expected {expected}");
				}
			}

		Assert.True(wrong.Count == 0, string.Join("\n", wrong));
	}

	/// <summary>
	/// What is read before the call back, in the shapes a prefix comes in, and the rules a split rule is called from
	/// and calls: each grammar with the prefix written, and the same without it, which §4.3 folds. Values show the
	/// grouping; the inputs have no space where the rule is entered, so the prefix stands and the two answer alike.
	/// </summary>
	static readonly (string Name, string Marked, string Twin, string[] Inputs)[] Shapes =
	[
		// A second operator: a later recursive alternative is read too.
		("operators", """
			T : @string = s: Sum & eof => @(s)
			Sum : @string = n: Num => @(n)
			    | @Ws & l: Sum & '+' & r: Num => @("(" + l + "+" + r + ")")
			    | @Ws & l: Sum & '-' & r: Num => @("(" + l + "-" + r + ")")
			Num : string = ['0'..'9']+
			""", "", ["1+2", "1-2", "1+2-3", "1-2+3-4", "1-", "-1"]),

		// Two levels, both past the recognizer: the base of one is the other, itself split.
		("levels", """
			T : @string = s: Add & eof => @(s)
			Add : @string = m: Mul => @(m)
			    | @Ws & l: Add & '+' & r: Mul => @("(" + l + "+" + r + ")")
			Mul : @string = n: Num => @(n)
			    | @Ws & l: Mul & '*' & r: Num => @("(" + l + "*" + r + ")")
			Num : string = ['0'..'9']+
			""", "", ["2*3", "1+2", "1*2+3", "1+2*3*4+5", "1+*2"]),

		// Under a repetition: the list asks for one more at the end, where the recognizer stands.
		("list", """
			T : @string = es: Add+ & eof => @(string.Join(" ; ", es))
			Add : @string = n: Num => @(n)
			    | @Ws & l: Add & '-' & r: Num => @("(" + l + "-" + r + ")")
			Num : string = ['0'..'9']+
			""", "", ["1", "1-2", "1-2-3", "1-"]),

		// A base written after the recursive alternative, which reaches the rule again past a bracket.
		("base", """
			T : @string = s: Sum & eof => @(s)
			Sum : @string = n: Num => @(n)
			    | @Ws & l: Sum & '+' & r: Num => @("(" + l + "+" + r + ")")
			    | '(' & s: Sum & ')' => @(s)
			Num : string = ['0'..'9']+
			""", "", ["1+2", "(1)", "(1+2)+3", "((1)+2)", "(1+2"]),

		// A choice beside the brackets that gives back into them.
		("power", """
			T : @string = s: Sum & eof => @(s)
			Sum : @string = p: P => @(p)
			    | @Ws & l: Sum & '+' & r: P => @("(" + l + "+" + r + ")")
			P : @string = a: Atom & '^' & n: Num => @("(" + a + "^" + n + ")")
			    | a: Atom => @(a)
			Atom : @string = n: Num => @(n)
			    | '(' & s: Sum & ')' => @(s)
			Num : string = ['0'..'9']+
			""", "", ["(1+2)^3", "(1+2)", "2^3+(1+2)", "1+2^3+4"]),

		// A rule that only wraps the recognizer.
		("wrapped", """
			T : @string = s: Sum & eof => @(s)
			Sum : @string = n: Num => @(n)
			    | W & l: Sum & '+' & r: Num => @("(" + l + "+" + r + ")")
			W = @Ws
			Num : string = ['0'..'9']+
			""", "W & |W = @Ws\n", ["1+2", "1+2+3"]),

		// Two recognizers, and a lookahead beside one.
		("several", """
			T : @string = s: Sum & eof => @(s)
			Sum : @string = n: Num => @(n)
			    | @Ws & @Ws & l: Sum & '+' & r: Num => @("(" + l + "+" + r + ")")
			    | ?!'#' & @Ws & l: Sum & '-' & r: Num => @("(" + l + "-" + r + ")")
			Num : string = ['0'..'9']+
			""", "@Ws & @Ws & |?!'#' & @Ws & ", ["1+2", "1-2", "1+2-3+4", "1-2+3"]),

		// Trivia woven between the recognizer and the call back.
		("trivia", """
			trivia = [' ']*
			T : @string = s: Sum & eof => @(s)
			Sum : @string = n: Num => @(n)
			    | @Ws & l: Sum & '+' & r: Num => @("(" + l + "+" + r + ")")
			Num : string = ['0'..'9']+
			""", "", ["1+2", "1 + 2", "1 +2+ 3", "1 +"]),

		// Through a rule that only forwards what it calls, which §4.3 makes direct.
		("forwarded", """
			T : @string = s: Primary & eof => @(s)
			Primary : @string = p: Call => @(p)
			    | n: Number => @(n)
			Call : @string = @Ws & t: Primary & '(' & a: Number & ')' => @(t + "(" + a + ")")
			Number : @string = d: Num => @(d)
			Num : string = ['0'..'9']+
			""", "", ["1", "1(2)", "1(2)(3)", "1(2", "(2)"]),

		// The trivia and two levels, the lower one plain left recursion.
		("trivia levels", """
			trivia = [' ']*
			T : @string = s: Add & eof => @(s)
			Add : @string = m: Mul => @(m)
			    | @Ws & l: Add & '+' & r: Mul => @("(" + l + "+" + r + ")")
			    | @Ws & l: Add & '-' & r: Mul => @("(" + l + "-" + r + ")")
			Mul : @string = a: Atom => @(a)
			    | l: Mul & '*' & r: Atom => @("(" + l + "*" + r + ")")
			Atom : @string = n: Num => @(n)
			    | '(' & s: Add & ')' => @(s)
			Num : string = ['0'..'9']+
			""", "", ["1 - 2 - 3", "(1 + 2) * 3", "1 - (2 - 3)", "3 + 6 * (5 - 9)"]),

		// Values as the twin has them. A rule typed by another rule (A : B), its value built by '=>' on every alternative.
		("typed by a rule", """
			T : @string = s: Sum & eof => @(s)
			Sum : Item = i: Item => @(i)
			    | @Ws & l: Sum & '+' & r: Item => @("(" + l + "+" + r + ")")
			Item : @string = n: Num => @("<" + n + ">")
			Num : string = ['0'..'9']+
			""", "", ["1", "1+2", "1+2+3", "1+"]),

		("typed by a record", """
			T : @string = s: Sum & eof => @(s.A + "=" + s.B)
			Sum : P = p: P => @(p)
			    | @Ws & l: Sum & '+' & n: Num => @(new P(l.A + "+" + n, l.B))
			P = a: Num & '=' & b: Num
			Num : string = ['0'..'9']+
			""", "", ["1=2", "1=2+3", "1=2+3+4", "1=2+"]),

		// The text it matched.
		("text", """
			T : @string = s: Sum & eof => @(s)
			Sum : @string = Num
			    | @Ws & Sum & '+' & Num
			Num : string = ['0'..'9']+
			""", "", ["1", "1+2", "12+3+45", "1+"]),

		// Built by no '=>', a left recursion is the record of what its bases captured, and the
		// steps only extend what it matched.
		("record", """
			T : @string = e: E & eof => @(e.A + "=" + e.B)
			E = @Ws & E & '+' & Num
			  | a: Num & '=' & b: Num
			Num : string = ['0'..'9']+
			""", "", ["1=2", "1=2+3", "1=2+3+4", "1=2+", "1+2"]),

		// A record whose base reaches it again past a bracket, and captures it.
		("record parens", """
			T : @string = e: E & eof => @(e.A + "=" + e.B + "[" + e.Inner?.A + "]")
			E = @Ws & E & '+' & Num
			  | a: Num & '=' & b: Num
			  | '(' & inner: E & ')'
			Num : string = ['0'..'9']+
			""", "", ["1=2", "1=2+3", "(1=2+3)+4", "((1=2))+3", "(1=2"]),

		// A record under a repetition, with two prefixes, past trivia, and read by a rule that builds.
		("record list", """
			T : @string = es: E+ & eof => @(string.Join(" ", System.Linq.Enumerable.Select(es, e => e.A + "=" + e.B)))
			E = @Ws & E & '+' & Num & ';'
			  | a: Num & '=' & b: Num & ';'
			Num : string = ['0'..'9']+
			""", "", ["1=2;", "1=2;+3;", "1=2;+3;4=5;", "1=2;+"]),

		("record several", """
			T : @string = e: E & eof => @(e.A + "=" + e.B)
			E = @Ws & @Ws & E & '+' & Num
			  | ?!'#' & @Ws & E & '-' & Num
			  | a: Num & '=' & b: Num
			Num : string = ['0'..'9']+
			""", "@Ws & @Ws & |?!'#' & @Ws & ", ["1=2", "1=2+3", "1=2-3+4", "1=2+3-"]),

		("record trivia", """
			trivia = [' ']*
			T : @string = s: S & eof => @(s)
			S : @string = e: E => @(e.A + "=" + e.B)
			E = @Ws & E & '+' & Num
			  | a: Num & '=' & b: Num
			Num : string = ['0'..'9']+
			""", "", ["1=2", "1 = 2 + 3", "1=2 +3+ 4", "1=2 +"]),

		// Two record levels, the base of one the other's operand.
		("record levels", """
			T : @string = e: E & eof => @(e.A + "=" + e.B + "/" + e.M?.N)
			E = @Ws & E & '+' & M
			  | a: Num & '=' & b: Num & m: M
			M = @Ws & M & '*' & Num
			  | ':' & n: Num
			Num : string = ['0'..'9']+
			""", "", ["1=2:3", "1=2:3*4", "1=2:3*4+:5*6", "1=2:3+", "1=2:*"]),
	];

	public static TheoryData<string> ShapeNames()
	{
		var names = new TheoryData<string>();

		foreach (var shape in Shapes)
			names.Add(shape.Name);

		return names;
	}

	/// <summary>Where the prefix stands, the rule answers what the rule without it answers, in every reading.</summary>
	[Theory]
	[MemberData(nameof(ShapeNames))]
	public void Where_the_prefix_stands_the_rule_answers_as_its_twin(string name)
	{
		var shape  = Shapes.Single(one => one.Name == name);
		var marked = shape.Marked.ReplaceLineEndings("\n");
		var twin   = marked;

		foreach (var prefix in (shape.Twin.Length == 0 ? "@Ws & " : shape.Twin).Split('|'))
			twin = twin.Replace(prefix, "", StringComparison.Ordinal);

		var wrong = new List<string>();

		foreach (var (direct, tape, buffered) in Readings)
		{
			var reading = $"{(direct ? "direct" : "engine")}/{(tape ? "tape" : "immediate")}/{(buffered ? "view" : "span")}";
			var read    = Build(marked, direct, tape, buffered, lexical: false, out var told);
			var other   = Build(twin, direct, tape, buffered, lexical: false, out _);

			Assert.Contains(told, static one => one.Id == "GRAM5021" && one.Message.Contains("is read as the left recursion", StringComparison.Ordinal));

			foreach (var input in shape.Inputs)
				foreach (var over in buffered ? new[] { typeof(string), typeof(TextReader) } : [typeof(string)])
				{
					var expected = Answer(other, input, typeof(string));
					var answer   = Answer(read, input, over);

					if (answer != expected)
						wrong.Add($"{name} {reading} {over.Name} \"{input}\": {answer}, twin {expected}");
				}
		}

		Assert.True(wrong.Count == 0, string.Join("\n", wrong));
	}

	/// <summary>
	/// Where the rule without the prefix is a left recursion §4.3 refuses, the rule is read as written, and the build
	/// says why in the terms of that rule.
	/// </summary>
	[Theory]
	[InlineData("A = @Ws & A & 'b'", "no alternative to start from")]
	[InlineData("A = 'a' | @Ws & A & '+' & A", "recursive on both sides")]
	[InlineData("A = 'a' | @Ws & A & 'b'?", "a step may read nothing")]
	[InlineData("A = B | @Ws & A & 'x'\nB = 'b' | @Ws & A & 'y'", "goes through 'B'")]
	[InlineData("A = 'a' | @Ws & B & 'c'\nB = A & 'b'", "goes through 'B'")]
	[InlineData("A : B = B | @Ws & A & '!'\nB : @int = 'b' => @(1)", "declares a type and no alternative builds it")]
	[InlineData("A : @SourceSpan = 'a' | @Ws & A & '+' & 'a'", "declares a type and no alternative builds it")]
	[InlineData("A = x: 'a' | @Ws & y: A & 'c'", "a step captures")]
	[InlineData("A = x: 'a' | @Ws & A & y: 'c'", "a step captures")]
	public void Where_the_twin_is_refused_the_rule_is_read_as_written(string rules, string reason)
	{
		var result = GramCompiler.Compile("T = A & eof\n" + rules + "\nparse T", Options(direct: true, tape: false, buffered: false, lexical: false));

		var refused = result.Diagnostics.Where(static one => one.Id == GrammarNormalizer.StandingRefused).ToList();

		Assert.NotEmpty(refused);
		Assert.All(refused, static one => Assert.Equal(GramSeverity.Warning, one.Severity));
		Assert.Contains(refused, one => one.Message.Contains(reason, StringComparison.Ordinal));
		Assert.DoesNotContain(result.Diagnostics, static one => one.Message.Contains("is read as the left recursion", StringComparison.Ordinal));
	}

	/// <summary>
	/// A record read past a prefix that stands is folded in place, and the type made from its captures is the one 0.3.0
	/// made: the same members, of the same types, in the same order — the alternative read as written where the prefix
	/// moves captures nothing, as it never did.
	/// </summary>
	[Theory]
	[InlineData("record")]
	[InlineData("record parens")]
	[InlineData("record several")]
	[InlineData("record levels")]
	public void A_record_keeps_the_type_it_had(string name)
	{
		var marked = Shapes.Single(one => one.Name == name).Marked.ReplaceLineEndings("\n") + "\nparse T";
		var now    = GramCompiler.Compile(marked, Options(direct: true, tape: false, buffered: false, lexical: false));
		var v030   = Unchecked(() => GramCompiler.Compile(marked, Options(direct: true, tape: false, buffered: false, lexical: false)));

		Assert.Contains(now.Diagnostics, static one => one.Id == "GRAM5021" && one.Message.Contains("is read as the left recursion", StringComparison.Ordinal));
		Assert.Equal(Types(v030.Sources[0].Text), Types(now.Sources[0].Text));
		Assert.NotEmpty(Types(now.Sources[0].Text));

		static string[] Types(string source)
		{
			return [.. Regex.Matches(source, @"public sealed class \w+\s*\{.*?\r?\n\t\}\r?\n", RegexOptions.Singleline).Select(static one => one.Value)];
		}
	}

	/// <summary>A value guard in a caller reads what the cycle builds: the rule is read as written, as in 0.3.0.</summary>
	[Fact]
	public void A_value_guard_on_the_way_keeps_the_rule_as_written()
	{
		var grammar = """
			T = d: Sum & when @(d >= 2) & eof
			Sum : @int = Num => @(1)
			    | @Ws & s: Sum & '+' & Num => @(s + 1)
			Num = ['0'..'9']+
			parse T
			""".ReplaceLineEndings("\n");

		var result = GramCompiler.Compile(grammar, Options(direct: true, tape: false, buffered: false, lexical: false));

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GrammarNormalizer.StandingRefused);
		Assert.Contains(result.Diagnostics, static one => one.Id == "GRAM5021" && one.Message.Contains("is read as it is written", StringComparison.Ordinal));
	}

	/// <summary>
	/// Where the prefix stands, the recognizer is asked once where the rule is entered, and the loop enters it no more;
	/// as written it is asked once an entry, and entered once a tail where the parse needs the recursive alternative.
	/// </summary>
	[Theory]
	[InlineData("1+1+1", 1)]
	[InlineData("1+2+3+4", 1)]
	[InlineData(" 1+1", -1)]
	[InlineData(" 1+1+1", -1)]
	public void The_recognizer_is_asked_once_where_the_prefix_stands(string input, int once)
	{
		var marked = Grammar("count", "end");
		var counts = new List<string>();

		foreach (var (direct, tape, buffered) in Readings)
		{
			var read    = Build(marked, direct, tape, buffered, lexical: false, out _);
			var written = Unchecked(() => Build(marked, direct, tape, buffered, lexical: false, out _));
			var ended   = Answer(written, input, typeof(string));
			var asked   = Calls(written);
			var answer  = Answer(read, input, typeof(string));

			Assert.StartsWith("ok@", answer, StringComparison.Ordinal);

			counts.Add($"{(direct ? "direct" : "engine")}/{(tape ? "tape" : "immediate")}/{(buffered ? "view" : "span")}: {Calls(read)} (as written {(ended == "LOOP" ? "LOOP" : asked)})");

			if (once > 0)
				Assert.Equal(once, Calls(read));
		}

		Console.WriteLine($"calls \"{input}\": " + string.Join("; ", counts));

		static int Calls(Assembly parser)
		{
			return (int)parser.GetType("Probe")!.GetMethod("Calls")!.Invoke(null, null)!;
		}
	}

	/// <summary>
	/// The prefix that stands written in the grammar rather than in the host is refused where it is generated (GRAM4002),
	/// over characters and over tokens: only a recognizer can hide it.
	/// </summary>
	[Theory]
	[InlineData("?!'#' & ", false)]
	[InlineData("?!'#' & ", true)]
	[InlineData("Pre & ", false)]
	public void A_prefix_written_in_the_grammar_is_refused(string prefix, bool lexical)
	{
		foreach (var family in new[] { "calculator", "count" })
		{
			var grammar = Grammar(family, "tail").Replace("@Ws & ", prefix, StringComparison.Ordinal) + "\nPre = '#'*";
			var result  = GramCompiler.Compile((lexical ? Lexical(grammar) : grammar) + "\nparse T", Options(direct: true, tape: false, buffered: false, lexical));

			Assert.Contains(result.Diagnostics, static one => one.Id == GrammarNormalizer.LeftRecursion);
		}
	}

	static string Grammar(string family, string name)
	{
		return (family == "calculator" ? Calculator : Count)[name].ReplaceLineEndings("\n");
	}

	/// <summary>The same grammar with its numbers lexed: <c>Num</c> in a lexical namespace, spaces as trivia between tokens.</summary>
	static string Lexical(string grammar)
	{
		var syntax = string.Join("\n", grammar.Split('\n').Where(static line => !line.StartsWith("Num ", StringComparison.Ordinal)));

		return "namespace Lexical\n{\n\ttrivia = none\n\tNum : string = ['0'..'9']+\n}\ntrivia = { [' ']* }\n" +
			Regex.Replace(syntax, @"\bNum\b", "Lexical.Num");
	}

	static GramCompilerOptions Options(bool direct, bool tape, bool buffered, bool lexical)
	{
		return new GramCompilerOptions
		{
			ClassName      = "Probe",
			Direct         = direct,
			Lexical        = lexical,
			Carrier        = tape ? CarrierKind.Deferred : CarrierKind.Auto,
			BufferedInput  = buffered,
			SymbolResolver = buffered ? StandingTurnTests.ViewResolver.Instance : PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		};
	}

	static Assembly Build(string grammar, bool direct, bool tape, bool buffered, bool lexical, out IReadOnlyList<GramDiagnostic> told)
	{
		var result = GramCompiler.Compile(grammar + "\nparse T", Options(direct, tape, buffered, lexical));

		told = result.Diagnostics;

		// GRAM5004 is what the lexical rows are about, and GRAM4016 is said of a choice beside brackets that one of the
		// shapes has on purpose.
		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id is not (GramCompiler.BufferedUnsupported or GramCompiler.NotCut or "GRAM4016")));

		// A grammar without the recognizer gets no input view over tokens, and needs none.
		var host = !grammar.Contains("@Ws", StringComparison.Ordinal) ? Counter + Reports : buffered ? ViewHost : SpanHost;

		return EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe", declarationMembers: host,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>Where the parse ended and the value, or where it stopped; <c>LOOP</c> where the recognizer went past its bound.</summary>
	static string Answer(Assembly parser, string input, Type over)
	{
		var type = parser.GetType("Probe")!;

		type.GetMethod("Reset")!.Invoke(null, null);

		object match;

		try
		{
			match = over == typeof(string)
				? type.GetMethod("TryParseT", [typeof(string)])!.Invoke(null, [input])!
				: type.GetMethod("TryParseT", [over, typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(input), 1, 1 << 16])!;
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException?.Message == "went round" || thrown.InnerException is InsufficientExecutionStackException)
		{
			return "LOOP";
		}

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"ok@{(long)Read("Position")! + (int)Read("Length")!} {Read("Value")}"
			: $"no@{Read("Position")}";
	}
}
