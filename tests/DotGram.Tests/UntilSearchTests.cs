using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The "until" scan, <c>(?!X &amp; any)*</c>, searching for the characters <c>X</c> can begin with
/// (Machine.UntilStops) rather than asking <c>X</c> at every character: the same grammar with a
/// guard in the turn, which the search does not recognize, is the loop it replaces, and the two
/// must answer alike — what was read, how far, and what a refusal says and where.
/// </summary>
/// <remarks>
/// The rows are the shapes the search has to keep: a delimiter of several characters, two looks in
/// one turn, an ignore-case literal, a choice of delimiters, a back-reference at the start of the
/// delimiter and behind a closing bracket, a binding that may be empty, a run that has to give
/// characters back to what follows it, the scan in trivia, and delimiters the search must leave to
/// the loop: one beginning with half of a surrogate pair, and one that may begin with too many
/// characters. Each is read by the reader and by the engine, over both carriers, as a whole parse
/// and as a <c>find</c>, and the fast side is also held to the reference semantics.
/// </remarks>
public sealed class UntilSearchTests
{
	sealed record Row(string Name, string Grammar, string Alphabet, bool? Searches, string[] Inputs);

	static readonly Row[] Rows =
	[
		new("literal",
			"""
			Body  = (?!"*/" & any)*
			Start : @int = "/*" & b: Body & "*/" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"/*ab ()", true,
			["/**/", "/* a */", "/* * / */", "/* ***/", "/*", "/* a", "/* a *", "/*/", "/* */ */", "(/* x */)"]),

		new("literal, flat",
			"""
			Body  = (?!"*/" & any)*
			Start : @int = "/*" & b: Body & "*/" => @(b.Length)
			""",
			"/*ab ", true,
			["/**/", "/* a */", "/* ***/", "/*", "/* a *", "/* */ */"]),

		new("given back",
			"""
			Body  = (?!"--" & any)*
			Start : @int = b: Body & 'c' & "--" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"abc-()", true,
			["abc--", "c--", "--", "ab-c--", "abc", "abc-", "a-b-c--", "abcc--", "abc--c--", "(abc--)"]),

		new("two looks",
			"""
			Body  = (?!"*/" & ?!"/*" & any)*
			Start : @int = "/*" & b: Body & ("*/" | "/*") => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"/*a ()", true,
			["/* a */", "/* a /* b */", "/*/", "/* / * */", "/*"]),

		new("ignore case",
			"""
			Body  = (?!"end"i & any)*
			Start : @int = b: Body & "END"i => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"enEdND x()", true,
			["END", "x end", "x eNd", "x en", "x ed END", "eend", "(x End)"]),

		new("choice",
			"""
			Body  = (?!("*/" | "+/") & any)*
			Start : @int = "/*" & b: Body & ("*/" | "+/") => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"/*+a ()", true,
			["/* a +/", "/* a */", "/* + * */", "/*+", "/*"]),

		new("plus",
			"""
			Body  = (?!"--" & any)+
			Start : @int = b: Body & "--" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"ab-()", null,
			["a--", "--", "ab-a--", "a", "(a--)", "---"]),

		new("long bracket",
			"""
			Start : @int = '[' & $eq: '='* & '[' & b: ((?!(']' & $eq & ']') & any)*) & ']' & $eq & ']' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"[]=a ()", true,
			["[[]]", "[==[ a ]=] ]==]", "[=[ ]] ]=]", "[=[ ]==] ]=]", "[[ a ]", "[==[ ]=", "[=[]=]"]),

		new("raw string",
			"""
			Start : @int = { $q: '"'{3,} } & b: ({ (?!$q & any)* }) & $q & ?!'"' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\"a ()", true,
			["\"\"\" a \"\"\"", "\"\"\"\" \"\"\" \"\"\"\"", "\"\"\" \"\"\"\"", "\"\"\"\"", "\"\"\" a", "(\"\"\"x\"\"\")"]),

		new("raw string, unbraced",
			"""
			Start : @int = { $q: '"'{3,} } & b: ((?!$q & any)*) & $q & ?!'"' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\"a ()", true,
			["\"\"\" a \"\"\"", "\"\"\"\" \"\"\" \"\"\"\"", "\"\"\" \"\"\"\"", "\"\"\" a"]),

		new("dollar quoted",
			"""
			Start : @int = '$' & $tag: (Tag?) & '$' & b: ((?!('$' & $tag & '$') & any)*) & '$' & $tag & '$' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			Tag   = ['a'..'b']+
			""",
			"$ab ()", true,
			["$$ a $$", "$a$ $ $a$", "$a$ $b$ $a$", "$ab$ $a$ $ab$", "$$", "$a$ a $b$"]),

		new("trivia",
			"""
			trivia = (' ' | "/*" & (?!"*/" & any)* & "*/")*
			Start : @int = n: ['0'..'9']+ => @(n.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"/*1 ()", null,
			["1", "/* a */ 12", "( /* x */ 3 )", "/* 1", "1 /* */", "/**/1/**/"]),

		new("lengths",
			"""
			Body  = (?!("abcd" | "b") & any)*
			Start : @int = b: Body & ("abcd" | "b") & 'q' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"abcdxq()", true,
			["xbq", "xxabcdq", "xxabc", "xxab", "xxa", "xabcq", "xaq", "x"]),

		new("look before",
			"""
			Start : @int = (?!(?!('a' & 'c'* & "zzzzzzzz") & "bb") & any)* & 'q' => @(0)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"acbzxq()", false,
			["acbbx", "acbbq", "q", "accccbb", "bbq"]),

		new("surrogate",
			"""
			Body  = (?!"😀!" & any)*
			Start : @int = b: Body & "😀!" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"a!😀\uDE00()", false,
			["😀!", "a😀😀!", "\uDE00!"]),

		new("too many",
			"""
			Body  = (?!("ab" | "cd" | "ef" | "gh" | "ij" | "kl") & any)*
			Start : @int = b: Body & "kl" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"abcdklx()", false,
			["xkl", "abckl", "x", "(kl)"]),
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
	public void The_search_answers_as_the_loop_it_replaces(string name)
	{
		var row    = Rows.Single(one => one.Name == name);
		var inputs = Inputs(row);
		var graph  = Normalized(row.Grammar);
		var start  = graph.Rules.First(rule => rule.Name == "Start");

		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var fast = Build(row.Grammar + "\nparse Start\n", carrier, direct, row.Searches);
			var slow = Build(Slow(row.Grammar) + "\nparse Start\n", carrier, direct, row.Searches is null ? null : false);

			foreach (var input in inputs)
			{
				var expected = (EmittedCode.Match(slow, "Grammar", "TryParseStart", input), EmittedCode.Outcome(slow, "Grammar", "TryParseStart", input));
				var actual   = (EmittedCode.Match(fast, "Grammar", "TryParseStart", input), EmittedCode.Outcome(fast, "Grammar", "TryParseStart", input));

				Assert.True(
					expected.Equals(actual),
					$"{row.Name}, {carrier}, direct {direct}, \"{Escaped(input)}\": the loop says {expected}, the search {actual}");

				Assert.True(
					actual.Item1.IsSuccess == ReferenceInterpreter.Parses(graph, start, input),
					$"{row.Name}, {carrier}, direct {direct}, \"{Escaped(input)}\": the search says {actual.Item1.IsSuccess}");
			}
		}

		// With a `parse` beside the `find`, which a grammar with trivia needs for its entry to compile.
		{
			var fast = Build(row.Grammar + "\nparse Start\nfind Start\n", CarrierKind.Auto, direct: false, row.Searches);
			var slow = Build(Slow(row.Grammar) + "\nparse Start\nfind Start\n", CarrierKind.Auto, direct: false, row.Searches is null ? null : false);

			foreach (var input in inputs)
			{
				var expected = EmittedCode.Found(slow, "Grammar", "FindStart", input);
				var actual   = EmittedCode.Found(fast, "Grammar", "FindStart", input);

				Assert.True(
					expected.SequenceEqual(actual),
					$"{row.Name}, find, \"{Escaped(input)}\": the loop finds [{string.Join(", ", expected)}], " +
					$"the search [{string.Join(", ", actual)}]");
			}
		}
	}

	/// <summary>
	/// A delimiter that runs code of the host in a look past its first character — a guard, or a
	/// predicate in a class — is left to the loop: the loop tries it at every character, and so
	/// runs the code at every character, which a search for <c>b</c> would not.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_delimiter_that_runs_host_code_is_left_to_the_loop(bool direct)
	{
		const string guarded = """
			Stop  = ?!('a' & when @Touch()) & "bb"
			Body  = (?!Stop & any)*
			Start : @int = Body => @(Calls)
			      | '(' & inner: Start & ')' => @(inner)
			parse Start
			""";
		const string predicated = """
			Stop  = ?!('a' & [@Touched]) & "bb"
			Body  = (?!Stop & any)*
			Start : @int = Body => @(Calls)
			      | '(' & inner: Start & ')' => @(inner)
			parse Start
			""";
		const string members = """
			static int Calls;
			static bool Touch() { Calls++; return true; }
			static bool Touched(char c) { Calls++; return true; }
			""";

		foreach (var grammar in new[] { guarded, predicated })
		{
			var fast = Build(grammar, CarrierKind.Auto, direct, false, members);
			var slow = Build(Slow(grammar), CarrierKind.Auto, direct, false, members);

			foreach (var input in new[] { "", "a", "aa", "xa", "axbb", "abb", "aabba", "(a)", "aaaa" })
			{
				var expected = Counted(slow, input);
				var actual   = Counted(fast, input);

				Assert.True(expected.Equals(actual), $"\"{input}\": the loop says {expected}, the search {actual}\n{grammar}");
			}
		}

		static ((bool, object?, string?, long), int) Counted(Assembly assembly, string input)
		{
			var calls = assembly.GetType("Grammar")!.GetField("Calls", BindingFlags.NonPublic | BindingFlags.Static)!;

			calls.SetValue(null, 0);

			var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

			return (match, (int)calls.GetValue(null)!);
		}
	}

	/// <summary>
	/// A delimiter that recovers is left to the loop: a recovery matches from characters its first
	/// set does not hold, so the characters a search would pass over are ones it could match at.
	/// </summary>
	[Fact]
	public void A_delimiter_that_recovers_is_left_to_the_loop()
	{
		const string grammar = """
			Stop  = 'a'+ recover ';'
			Body  = (?!Stop & any)*
			Start : @int = b: Body & 'a' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			parse Start
			""";

		foreach (var direct in new[] { false, true })
		{
			var result = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName = "Grammar", Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
			});

			Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);
			Assert.DoesNotContain("var untilFound", Assert.Single(result.Sources).Text, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// A delimiter far into a long body is found by one search, and the body before it may hold
	/// near misses at any distance: the answer is the loop's.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_long_body_is_searched_to_its_delimiter(bool direct)
	{
		var row  = Rows.Single(one => one.Name == "long bracket");
		var fast = Build(row.Grammar + "\nparse Start\n", CarrierKind.Auto, direct, true);
		var slow = Build(Slow(row.Grammar) + "\nparse Start\n", CarrierKind.Auto, direct, false);
		var body = new StringBuilder();

		for (var i = 0; i < 2000; i++)
			body.Append(i % 7 == 0 ? "]=]" : i % 5 == 0 ? "]]" : "text ");

		foreach (var input in new[] { "[==[" + body + "]==]", "[==[" + body + "]=]", "[==[" + body })
			Assert.Equal(
				EmittedCode.Match(slow, "Grammar", "TryParseStart", input),
				EmittedCode.Match(fast, "Grammar", "TryParseStart", input));
	}

	/// <summary>
	/// The same grammar with a guard after the character, which the search does not recognize: the
	/// turn still fails where it did, at the character, so a refusal is worded alike.
	/// </summary>
	static string Slow(string grammar)
	{
		var slow = grammar.Replace("& any)", "& any & when @(true))");

		Assert.NotEqual(grammar, slow);

		return slow;
	}

	/// <param name="searches">Whether the parser must search, must not, or null where either may be right.</param>
	static Assembly Build(string grammar, CarrierKind carrier, bool direct, bool? searches, string? members = null)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;

		Assert.True(
			searches is null || searches == source.Contains("var untilFound", StringComparison.Ordinal),
			$"{(searches == true ? "No" : "A")} search for an until scan in:\n{grammar}");

		return EmittedCode.Compile(source, declarationMembers: members);
	}

	/// <summary>The row's own inputs, and short random ones over its alphabet.</summary>
	static IEnumerable<string> Inputs(Row row)
	{
		var random = new Random(row.Name.Length * 7919 + row.Grammar.Length);
		var made   = new List<string>(row.Inputs) { "" };

		for (var i = 0; i < 150; i++)
		{
			var built = new StringBuilder();

			// Most begin as the row's first input does, so the scan is reached at all.
			if (random.Next(4) != 0 && row.Inputs[random.Next(row.Inputs.Length)] is var opening)
				built.Append(opening, 0, Math.Min(4, opening.Length));

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
