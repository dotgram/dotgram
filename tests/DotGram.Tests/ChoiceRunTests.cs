using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A repetition of a choice whose turns at most characters read that one character alone — the
/// text of a quoted string, <c>([' '..'!' | '#'..'['] | '\\' &amp; Escape)*</c>, or Std's
/// <c>Escaped</c> — read through as a class inside the rule (Machine.ThroughTurns) rather than as
/// a turn per character: the same grammar with an empty literal after the character, which keeps
/// it the loop, must answer alike — what was read, how far, and what a refusal says and where.
/// </summary>
/// <remarks>
/// The rows are the shapes the class has to keep: the class first and an escape after it, the
/// class after an alternative that fails on its first item, a look before the class, alternatives
/// that overlap, a literal of two characters tried first near the end of the input, a class named
/// by a rule, a repetition of at least one, a loop that has to give characters back to what
/// follows it, an alternative that refuses before the class where nothing after the loop says
/// more, and Std's <c>Escaped</c> itself, whose stops are few enough to search for. Each is
/// read by the reader and by the engine, over both carriers, as a whole parse and as a
/// <c>find</c>, and the fast side is also held to the reference semantics.
/// </remarks>
public sealed class ChoiceRunTests
{
	/// <param name="Grammar">The grammar, with <c>{G}</c> where the slow side has its empty literal.</param>
	/// <param name="Reads">How the fast side passes over the turns: "class", "search", or null for either or neither.</param>
	/// <param name="Slow">The slow side where it is not the grammar with the empty literal in.</param>
	/// <param name="Reference">Whether the reference semantics read the grammar (they do not read Std's rules).</param>
	sealed record Row(
		string Name, string Grammar, string Alphabet, string? Reads, string[] Inputs, string? Slow = null, bool Reference = true);

	static readonly Row[] Rows =
	[
		new("json string",
			"""
			Text  = '"' & ([' '..'!' | '#'..'[' | ']'..'\uFFFF']{G} | '\\' & (['"' | '\\' | '/' | 'n'] | 'u' & Hex{4}))* & '"'
			Hex   = ['0'..'9' | 'a'..'f']
			Start : @int = t: Text => @(t.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\"a\\u0\u0001()\u00e9", "class",
			["\"\"", "\"abc\"", "\"a\\\"b\"", "\"a\\\"", "\"\\\\\"", "\"a\\u00e9b\"", "\"a\\u00\"", "\"abc", "\"a\u0001b\"",
				"(\"x\")", "\"a\\", "\"\\q\"", "\"\u00e9\uD83D\uDE00\""]),

		new("quoted string",
			"""
			Quoted = '"' & ([' ' | '\t' | '!' | '#'..'[' | ']'..'~' | '\u0080'..'\u00FF']{G} | '\\' & [' ' | '\t' | '!'..'~' | '\u0080'..'\u00FF'])* & '"'
			Start : @int = q: Quoted => @(q.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\"a\\ \t\u0080\u0100()", "class",
			["\"\"", "\"a b\"", "\"a\\\"b\"", "\"\u0100\"", "\"a\\\u0100\"", "\"abc", "(\"x\")"]),

		new("class second",
			"""
			Body  = ('\\' & any | [^ '"' | '\\']{G})*
			Start : @int = '"' & b: Body & '"' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\"ab\\()", "search",
			["\"\"", "\"ab\"", "\"a\\\"b\"", "\"a\\", "\"ab", "(\"a\")"]),

		new("look before",
			"""
			Body  = (?!"*/" & [^ '\\']{G} | '\\' & any)*
			Start : @int = "/*" & b: Body & "*/" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"/*a\\ ()", null,
			["/**/", "/* a */", "/* \\*/ */", "/* * / */", "/* ***/", "/*", "/* a *", "/* a\\", "(/* x */)"]),

		new("overlapping",
			"""
			Body  = (['a'..'z']{G} | 'a' & '1')*
			Start : @int = b: Body & ';' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"ab1;()", "class",
			[";", "ab;", "a1b;", "ba1;", "a;", "a", "1;", "(b;)"]),

		new("two characters first",
			"""
			Body  = ("ab" | ['c'..'z']{G} | 'a' & '!')*
			Start : @int = b: Body & ';' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"abcz!;()", "class",
			[";", "cz;", "abc;", "ccca", "ccab;", "cca!;", "cca", "ccc", "(c;)"]),

		new("named class",
			"""
			Char  = [' '..'!' | '#'..'[' | ']'..'~']
			Body  = (Char{G} | '\\' & any)*
			Start : @int = '"' & b: Body & '"' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\"ab\\ ()", null,
			["\"\"", "\"ab\"", "\"a\\\"b\"", "\"a\\", "\"ab", "(\"a\")"]),

		new("plus",
			"""
			Body  = (['a'..'z']{G} | '-' & ['0'..'9'])+
			Start : @int = b: Body & ';' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"ab-1;()", null,
			[";", "ab;", "a-1b;", "-1;", "-;", "a-", "(b;)"]),

		new("given back",
			"""
			Body  = (['a'..'z']{G} | '-' & ['0'..'9'])*
			Start : @int = b: Body & "ab" & ';' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"abc-1;()", null,
			["ab;", "cab;", "c-1ab;", "abab;", "ab", "c", "(ab;)"]),

		new("escaped",
			"""
			Start : @int = '"' & b: ((?!'"' & ('\\' & any | ?!'\\' & any{G}))*) & '"' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\"ab\\()", "search",
			["\"\"", "\"ab\"", "\"a\\\"b\"", "\"a\\\"", "\"\\\\\"", "\"a\\", "\"ab", "(\"a\")"]),

		// What the alternative tried first refuses at a character passed over is said by a reading
		// that records, and nothing after the loop says anything further here: both looks after it
		// refuse in silence.
		new("refused before",
			"""
			Start : @int = b: ((?!'q' & ('x' & any | any{G}))*) & ?!'q' & 'q' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"axq()", null,
			["aaaq", "axaq", "q", "aaa", "xq", "(aq)"]),

		new("std escaped",
			"""
			Start : @int = e: Std.Escaped('"', '\\') => @(e.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\"ab\\()", "search",
			["\"\"", "\"ab\"", "\"a\\\"b\"", "\"a\\\"", "\"\\\\\"", "\"a\\", "\"ab", "(\"a\")"],
			Slow: """
			Escaped(quote, escape) = quote & (?!quote & (escape & any | ?!escape & any & ""))* & quote
			Start : @int = e: Escaped('"', '\\') => @(e.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			Reference: false),
	];

	public static TheoryData<string> RowNames
	{
		get
		{
			var rows = new TheoryData<string>();

			foreach (var row in Rows)
				rows.Add(row.Name);

			return rows;
		}
	}

	[Theory]
	[MemberData(nameof(RowNames))]
	public void The_class_answers_as_the_loop_it_replaces(string name)
	{
		var row    = Rows.Single(one => one.Name == name);
		var inputs = Inputs(row);
		var fastly = Fast(row);
		var slowly = Slow(row);
		var graph  = Normalized(fastly);
		var start  = graph.Rules.First(rule => rule.Name == "Start");

		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var slow = Build(slowly + "\nparse Start\n", carrier, direct, null);

			// The floor's loop and net8's search (NET8_0_OR_GREATER), both held to the loop.
			foreach (var net8 in new[] { false, true })
			{
				var fast = Build(fastly + "\nparse Start\n", carrier, direct, row.Reads, net8: net8);

				foreach (var input in inputs)
				{
					var expected = (EmittedCode.Match(slow, "Grammar", "TryParseStart", input), EmittedCode.Outcome(slow, "Grammar", "TryParseStart", input));
					var actual   = (EmittedCode.Match(fast, "Grammar", "TryParseStart", input), EmittedCode.Outcome(fast, "Grammar", "TryParseStart", input));

					Assert.True(
						expected.Equals(actual),
						$"{row.Name}, {carrier}, direct {direct}, net8 {net8}, \"{Escaped(input)}\": the loop says {expected}, the class {actual}");

					Assert.True(
						!row.Reference || actual.Item1.IsSuccess == ReferenceInterpreter.Parses(graph, start, input),
						$"{row.Name}, {carrier}, direct {direct}, net8 {net8}, \"{Escaped(input)}\": the class says {actual.Item1.IsSuccess}");
				}
			}
		}

		{
			var fast = Build(fastly + "\nparse Start\nfind Start\n", CarrierKind.Auto, direct: false, null);
			var slow = Build(slowly + "\nparse Start\nfind Start\n", CarrierKind.Auto, direct: false, null);

			foreach (var input in inputs)
			{
				var expected = EmittedCode.Found(slow, "Grammar", "FindStart", input);
				var actual   = EmittedCode.Found(fast, "Grammar", "FindStart", input);

				Assert.True(
					EmittedCode.FoundAt(slow, "Grammar", "FindStart", input)
						.SequenceEqual(EmittedCode.FoundAt(fast, "Grammar", "FindStart", input)),
					$"{row.Name}, find, \"{Escaped(input)}\": found at other places");

				Assert.True(
					expected.SequenceEqual(actual),
					$"{row.Name}, find, \"{Escaped(input)}\": the loop finds [{string.Join(", ", expected)}], " +
					$"the class [{string.Join(", ", actual)}]");
			}
		}
	}

	/// <summary>
	/// A long text with escapes at any distance is read through to its end: the answer is the
	/// loop's.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_long_text_is_read_through_to_its_end(bool direct)
	{
		var row  = Rows.Single(one => one.Name == "json string");
		var fast = Build(Fast(row) + "\nparse Start\n", CarrierKind.Auto, direct, "class");
		var net8 = Build(Fast(row) + "\nparse Start\n", CarrierKind.Auto, direct, "class", net8: true);
		var slow = Build(Slow(row) + "\nparse Start\n", CarrierKind.Auto, direct, null);
		var body = new StringBuilder();

		for (var i = 0; i < 2000; i++)
			body.Append(i % 7 == 0 ? "\\\"" : i % 5 == 0 ? "\\u00e9" : i % 3 == 0 ? "\u00e9" : "text ");

		foreach (var input in new[] { "\"" + body + "\"", "\"" + body, "\"" + body + "\\u00\"", "\"" + body + "\u0001\"" })
		{
			Assert.Equal(
				EmittedCode.Match(slow, "Grammar", "TryParseStart", input),
				EmittedCode.Match(fast, "Grammar", "TryParseStart", input));
			Assert.Equal(
				EmittedCode.Match(slow, "Grammar", "TryParseStart", input),
				EmittedCode.Match(net8, "Grammar", "TryParseStart", input));
		}
	}

	/// <summary>
	/// An alternative that runs code of the host before its first item is left to the loop: the
	/// loop tries it at every character the class reads, and so runs the code there.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void An_alternative_that_runs_host_code_is_left_to_the_loop(bool direct)
	{
		const string grammar = """
			Body  = (when @Touch() & '\\' & any | [^ '"' | '\\'])*
			Start : @int = '"' & Body & '"' => @(Calls)
			parse Start
			""";
		const string members = """
			static int Calls;
			static bool Touch() { Calls++; return false; }
			""";

		Build(grammar, CarrierKind.Auto, direct, "neither", members);
	}

	static string Fast(Row row)
	{
		return row.Grammar.Replace("{G}", "");
	}

	/// <summary>
	/// The same grammar with an empty literal after the character, which the class does not
	/// recognize: nothing else about the turn changes, so a refusal is worded alike. A guard there
	/// would also make the rule more than a recognizer, which changes what its loop records.
	/// </summary>
	static string Slow(Row row)
	{
		var slow = row.Slow ?? row.Grammar.Replace("{G}", " & \"\"");

		Assert.NotEqual(Fast(row), slow);

		return slow;
	}

	/// <param name="reads">"class", "search", "neither", or null where any may be right.</param>
	/// <param name="net8">Whether to compile what is written for net8 (NET8_0_OR_GREATER) rather than the floor.</param>
	static Assembly Build(string grammar, CarrierKind carrier, bool direct, string? reads, string? members = null, bool net8 = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;

		var classes  = ReadThrough.IsMatch(source) || source.Contains("var throughFound", StringComparison.Ordinal);
		var searches = source.Contains("var untilFound", StringComparison.Ordinal);

		Assert.True(
			reads switch
			{
				"class"   => classes,
				"search"  => searches,
				"neither" => !classes && !searches,
				_         => true,
			},
			$"Expected the turns {reads ?? "-"}, the class {classes}, the search {searches}, in:\n{grammar}");

		return EmittedCode.Compile(source, declarationMembers: members, symbols: net8 ? ["NET8_0_OR_GREATER"] : null);
	}

	/// <summary>What the class read through looks like in the emitted source (Machine.EmitThrough).</summary>
	static readonly Regex ReadThrough = new(
		@"p\+\+;\s+while \((\(uint\)p < \(uint\)text\.Length|p < untilEnd)\)", RegexOptions.CultureInvariant);

	/// <summary>The row's own inputs, and short random ones over its alphabet.</summary>
	static IEnumerable<string> Inputs(Row row)
	{
		var random = new Random(row.Name.Length * 7919 + row.Grammar.Length);
		var made   = new List<string>(row.Inputs) { "" };

		for (var i = 0; i < 150; i++)
		{
			var built = new StringBuilder();

			// Most begin as the row's first inputs do, so the loop is reached at all.
			if (random.Next(4) != 0 && row.Inputs[random.Next(row.Inputs.Length)] is var opening)
				built.Append(opening, 0, Math.Min(2, opening.Length));

			for (var length = random.Next(10); length > 0; length--)
				built.Append(row.Alphabet[random.Next(row.Alphabet.Length)]);

			if (random.Next(2) == 0)
				built.Append(row.Inputs[random.Next(row.Inputs.Length)]);

			made.Add(built.ToString());
		}

		return made;
	}

	static RecognitionGraph Normalized(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
	}

	static string Escaped(string input)
	{
		return string.Concat(input.Select(c => c is < ' ' or > '~' ? $"\\u{(int)c:X4}" : c.ToString()));
	}
}
