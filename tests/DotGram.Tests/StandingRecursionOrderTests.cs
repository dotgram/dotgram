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
/// (docs/syntax.md §7.2): which of several valid splits of the input such a rule answers, in every reading.
/// </summary>
/// <remarks>
/// <para>
/// Each grammar comes in two families with the same shape: a calculator, whose value is the number the expression
/// evaluates to, and a counter, whose value is how many numbers it holds. A rule's value shows the part of the input it
/// took: the top rule answers what the self-calling rule returned, how many tails its continuation read after it and
/// their value, and the sum of the two, which is the same however the input was split.
/// </para>
/// <para>
/// <c>Today</c> is what the rule written with <c>@Ws &amp;</c> before its call back answers now: where several splits
/// fit, the one with the fewest tails in the rule, a consequence of the order its alternatives are tried in rather
/// than of anything the rule says. <c>Twin</c> is what the same rule without the recognizer answers, read as left
/// recursion is (§4.3), as many tails in the rule as fit. Where the two differ, the answer depends on the order the
/// cycle is read in; choosing the order of left recursion for the marked rule means writing <c>Twin</c> into
/// <c>Today</c> on those rows and nothing else. A null <c>Today</c> is an input the marked rule goes round on and is not
/// pinned; its recognizer counts its calls and throws past a bound no ending parse comes near, so it cannot hang a run.
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
	/// Every input with what the marked rule answers today and what its twin answers; a null <c>Today</c> goes round.
	/// </summary>
	static readonly (string Family, string Grammar, string Input, string? Today, string Twin)[] Rows =
	[
		("calculator", "tail",   "1",         "ok@1 1 + [0: 0] = 1",   "ok@1 1 + [0: 0] = 1"),
		("calculator", "tail",   "1+1",       "ok@3 1 + [1: 1] = 2",   "ok@3 2 + [0: 0] = 2"),
		("calculator", "tail",   "1+2+3",     "ok@5 1 + [2: 5] = 6",   "ok@5 6 + [0: 0] = 6"),
		("calculator", "tail",   "12+3",      "ok@4 12 + [1: 3] = 15", "ok@4 15 + [0: 0] = 15"),
		("calculator", "tail",   " 1+1",      "ok@4 2 + [0: 0] = 2",   "no@0"),
		("calculator", "tail",   " 1",        null,                    "no@0"),
		("calculator", "tail",   "1+",        null,                    "no@2"),
		("count",      "tail",   "1",         "ok@1 1 + [0: 0] = 1",   "ok@1 1 + [0: 0] = 1"),
		("count",      "tail",   "1+1",       "ok@3 1 + [1: 1] = 2",   "ok@3 2 + [0: 0] = 2"),
		("count",      "tail",   "1+1+1",     "ok@5 1 + [2: 2] = 3",   "ok@5 3 + [0: 0] = 3"),
		("count",      "tail",   "12+3",      "ok@4 1 + [1: 1] = 2",   "ok@4 2 + [0: 0] = 2"),
		("count",      "tail",   " 1+1",      "ok@4 2 + [0: 0] = 2",   "no@0"),
		("count",      "tail",   "1+",        null,                    "no@2"),

		("calculator", "end",    "1+2+3",     "ok@5 6 + [0: 0] = 6",   "ok@5 6 + [0: 0] = 6"),
		("calculator", "end",    " 1+1",      "ok@4 2 + [0: 0] = 2",   "no@0"),
		("calculator", "end",    "1+",        null,                    "no@2"),
		("count",      "end",    "1+2+3",     "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3"),
		("count",      "end",    " 1+1",      "ok@4 2 + [0: 0] = 2",   "no@0"),
		("count",      "end",    "1+",        null,                    "no@2"),

		("calculator", "parens", "1+2",       "ok@3 1 + [1: 2] = 3",   "ok@3 3 + [0: 0] = 3"),
		("calculator", "parens", "(1+2)+3",   "ok@7 3 + [1: 3] = 6",   "ok@7 6 + [0: 0] = 6"),
		("calculator", "parens", "1+(2+3)",   "ok@7 1 + [1: 5] = 6",   "ok@7 6 + [0: 0] = 6"),
		("calculator", "parens", "(1+2+3)",   "ok@7 6 + [0: 0] = 6",   "ok@7 6 + [0: 0] = 6"),
		("calculator", "parens", "(1+2",      null,                    "no@4"),
		("count",      "parens", "1+2",       "ok@3 1 + [1: 1] = 2",   "ok@3 2 + [0: 0] = 2"),
		("count",      "parens", "(1+2)+3",   "ok@7 2 + [1: 1] = 3",   "ok@7 3 + [0: 0] = 3"),
		("count",      "parens", "1+(2+3)",   "ok@7 1 + [1: 2] = 3",   "ok@7 3 + [0: 0] = 3"),
		("count",      "parens", "(1+2+3)",   "ok@7 3 + [0: 0] = 3",   "ok@7 3 + [0: 0] = 3"),

		// The levels are not ambiguous: one split fits, and both read it where the marked rule ends. Its second recursive
		// alternative is never reached, so a binary minus goes round.
		("calculator", "levels", "1+2*3",     "ok@5 7 + [0: 0] = 7",   "ok@5 7 + [0: 0] = 7"),
		("calculator", "levels", "1*2+3",     "ok@5 5 + [0: 0] = 5",   "ok@5 5 + [0: 0] = 5"),
		("calculator", "levels", "8/4/2",     "ok@5 1 + [0: 0] = 1",   "ok@5 1 + [0: 0] = 1"),
		("calculator", "levels", "-2*3",      "ok@4 -6 + [0: 0] = -6", "ok@4 -6 + [0: 0] = -6"),
		("calculator", "levels", "(1+2)*3",   "ok@7 9 + [0: 0] = 9",   "ok@7 9 + [0: 0] = 9"),
		("calculator", "levels", "2*(3+4)",   "ok@7 14 + [0: 0] = 14", "ok@7 14 + [0: 0] = 14"),
		("calculator", "levels", "1-2",       null,                    "ok@3 -1 + [0: 0] = -1"),
		("count",      "levels", "1+2*3",     "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3"),
		("count",      "levels", "3+6*(5/9)", "ok@9 4 + [0: 0] = 4",   "ok@9 4 + [0: 0] = 4"),
		("count",      "levels", "8/4/2",     "ok@5 3 + [0: 0] = 3",   "ok@5 3 + [0: 0] = 3"),
		("count",      "levels", "1-2",       null,                    "ok@3 2 + [0: 0] = 2"),

		// The base after the recursive alternative is never reached where the recognizer stands.
		("calculator", "base",   "1+2",       "ok@3 1 + [1: 2] = 3",   "ok@3 3 + [0: 0] = 3"),
		("calculator", "base",   "5",         "ok@1 5 + [0: 0] = 5",   "ok@1 5 + [0: 0] = 5"),
		("calculator", "base",   "(1+2)+3",   null,                    "ok@7 6 + [0: 0] = 6"),
		("count",      "base",   "1+2",       "ok@3 1 + [1: 1] = 2",   "ok@3 2 + [0: 0] = 2"),
		("count",      "base",   "(1+2)+3",   null,                    "ok@7 3 + [0: 0] = 3"),

		// The list asks for one more expression at the end, and the marked rule goes round there on every input.
		("calculator", "list",   "1",         null,                    "ok@1 1"),
		("calculator", "list",   "1-2",       null,                    "ok@3 -1"),
		("calculator", "list",   "1--2",      null,                    "ok@4 3"),
		("count",      "list",   "1-2-3",     null,                    "ok@5 3"),
		("count",      "list",   "(1-2)-3",   null,                    "ok@7 3"),
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
	/// The marked rule answers today's split in every reading, over a string and, buffered, over a reader too; its twin
	/// answers its own.
	/// </summary>
	[Theory]
	[MemberData(nameof(Grammars))]
	public void A_rule_past_a_recognizer_that_stands_answers_one_split_in_every_reading(string family, string name)
	{
		var marked = Grammar(family, name);
		var twin   = marked.Replace("@Ws & ", "", StringComparison.Ordinal);
		var rows   = Rows.Where(row => row.Family == family && row.Grammar == name).ToArray();

		foreach (var (direct, tape, buffered) in Readings)
		{
			var reading = $"{(direct ? "direct" : "engine")}/{(tape ? "tape" : "immediate")}/{(buffered ? "view" : "span")}";
			var today   = Build(marked, direct, tape, buffered, lexical: false, out var told);
			var other   = Build(twin, direct, tape, buffered, lexical: false, out _);

			Assert.Contains(told, static one => one.Id == Reentry.Reenters);

			foreach (var row in rows)
				foreach (var over in buffered ? new[] { typeof(string), typeof(TextReader) } : [typeof(string)])
				{
					if (row.Today is not null)
						Assert.True(Answer(today, row.Input, over) == row.Today, $"{family} {name} {reading} {over.Name} \"{row.Input}\": {Answer(today, row.Input, over)}, today {row.Today}");

					Assert.True(Answer(other, row.Input, over) == row.Twin, $"{family} {name} twin {reading} {over.Name} \"{row.Input}\": {Answer(other, row.Input, over)}, expected {row.Twin}");
				}
		}
	}

	/// <summary>
	/// Asked for tokens, a recognizer in syntactic position keeps the grammar over characters (GRAM5004), so the marked
	/// rule answers as over characters, its trivia between the items; the twin is cut into tokens and answers its own.
	/// </summary>
	[Theory]
	[InlineData("calculator", "tail", "1+1",   "ok@3 1 + [1: 1] = 2", "ok@3 2 + [0: 0] = 2")]
	[InlineData("calculator", "tail", " 1+1",  "ok@4 1 + [1: 1] = 2", "ok@4 2 + [0: 0] = 2")]
	[InlineData("calculator", "tail", "1 +1",  "ok@4 1 + [1: 1] = 2", "ok@4 2 + [0: 0] = 2")]
	[InlineData("calculator", "tail", "1+",    null,                  "no@2")]
	[InlineData("count",      "tail", "1+1+1", "ok@5 1 + [2: 2] = 3", "ok@5 3 + [0: 0] = 3")]
	[InlineData("count",      "tail", " 1",    "ok@2 1 + [0: 0] = 1", "ok@2 1 + [0: 0] = 1")]
	[InlineData("calculator", "levels", "1 + 2 * 3", "ok@9 7 + [0: 0] = 7", "ok@9 7 + [0: 0] = 7")]
	[InlineData("calculator", "levels", "1 - 2",     null,                  "ok@5 -1 + [0: 0] = -1")]
	[InlineData("count",      "levels", "(1 + 2) * 3", "ok@11 3 + [0: 0] = 3", "ok@11 3 + [0: 0] = 3")]
	public void Over_tokens_the_marked_rule_stays_over_characters(string family, string name, string input, string? today, string twin)
	{
		var marked = Lexical(Grammar(family, name));

		foreach (var direct in new[] { true, false })
			foreach (var buffered in new[] { false, true })
			{
				var parser = Build(marked, direct, tape: false, buffered, lexical: true, out var told);
				var cut    = Build(marked.Replace("@Ws & ", "", StringComparison.Ordinal), direct, tape: false, buffered, lexical: true, out var cutTold);

				Assert.Contains(told, static one => one.Id == GramCompiler.NotCut);
				Assert.DoesNotContain(cutTold, static one => one.Id == GramCompiler.NotCut);

				if (today is not null)
					Assert.Equal(today, Answer(parser, input, typeof(string)));

				Assert.Equal(twin, Answer(cut, input, typeof(string)));
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

		// GRAM5004 is what the lexical rows are about.
		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id is not (GramCompiler.BufferedUnsupported or GramCompiler.NotCut)));

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
