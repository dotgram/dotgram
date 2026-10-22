using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The scans that search for where <c>(?!X &amp; any)*</c> stops, taken where the turn's character is
/// <c>[^ S]</c>, anything but a few characters: a run before a one-character stop
/// (Machine.StopCharacters), an until scan (Machine.UntilStops) and a scanned rule's delimiter
/// search (Machine.ScansUntil). Each row is held against the same grammar compiled with those turns
/// read one character at a time (Machine.ReadsFewOneByOne), which is the loop the search replaces:
/// the two must answer alike — what was read, how far, and what a refusal says and where.
/// </summary>
/// <remarks>
/// The rows hold what each search has to keep: an <c>S</c> of one character and of several, a
/// character of <c>S</c> right after the delimiter's first, a delimiter that begins with a character
/// of <c>S</c> (matched and not), the delimiter at the end of the input, a run that must give
/// characters back, a repetition that must turn at least once, a buffered reading, and a set too
/// large to search for, which is left to the loop.
/// </remarks>
public sealed class AnyButFewScanTests
{
	/// <param name="Searches">What the fast parser's source must hold, or null where it must not search.</param>
	sealed record Row(string Name, string Grammar, string Alphabet, string? Searches, string[] Inputs);

	static readonly Row[] Rows =
	[
		new("run, one stop",
			"""
			Body  = (?!',' & [^ ';'])*
			Start : @int = b: Body & (',' | ';') & 'z' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			",;az()", "IndexOfAny(text.Slice(p), ',', ';')",
			[",z", "ab,z", "ab;z", "ab", "a;b,z", "(a,z)", ";z", "aaa"]),

		new("run, several",
			"""
			Body  = (?![',' | '|'] & [^ ';' | '\n'])*
			Start : @int = b: Body & [',' | '|' | ';' | '\n'] & 'z' => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			",|;\naz()", "MemoryExtensions.IndexOfAny(",
			["ab|z", "ab\nz", "a\n", "ab;z", "(a,z)", "z"]),

		new("run, plus",
			"""
			Body  = (?!',' & [^ ';'])+
			Start : @int = b: Body & (',' | ';') => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			",;a()", "IndexOfAny(text.Slice(p), ',', ';')",
			["a,", ",", ";", "ab;", "(a,)", "a"]),

		new("until",
			"""
			Body  = (?!"*/" & [^ '\0'])*
			Start : @int = "/*" & b: Body & "*/" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"/*a\0 ()", "var untilFound",
			["/**/", "/* a */", "/* \0 */", "/* *\0/ */", "/* ***/", "/*", "/* a *", "/* \0", "(/* x */)", "/* */"]),

		new("until, several",
			"""
			Body  = (?!"*/" & [^ '\0' | '\n'])*
			Start : @int = "/*" & b: Body & ("*/" | ['\0' | '\n']) => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"/*a\0\n ()", "var untilFound",
			["/* a */", "/* a \n", "/* a \0", "/* *\n", "/*\n*/", "(/* x */)"]),

		new("until, delimiter in S",
			"""
			Body  = (?!"\0x" & [^ '\0'])*
			Start : @int = b: Body & "\0x" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"\0xa()", "var untilFound",
			["\0x", "a\0x", "a\0", "a\0a\0x", "a", "(a\0x)", "\0\0x"]),

		new("until, given back",
			"""
			Body  = (?!"--" & [^ '\0'])*
			Start : @int = b: Body & 'c' & "--" => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"abc-\0()", "var untilFound",
			["abc--", "c--", "--", "ab-c--", "ab\0c--", "abc\0--", "abc", "(abc--)"]),

		new("scan",
			"""
			trivia = { (' ' | "/*" & (?!"*/" & [^ '\0'])* & "*/")* }
			Start : @int = n: ['a'..'z']+ => @(n.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			" /*a\0()", "text.Slice(p, found",
			["a", "/* x */ ab", "( /* x */ a )", "/* \0 */ a", "/* a", "a /* */", "/**/a/**/", "/* *\0/ */a", "a /* \0"]),

		new("scan, delimiter in S",
			"""
			trivia = { (' ' | '<' & (?!"\0>" & [^ '\0'])* & "\0>")* }
			Start : @int = n: ['a'..'z']+ => @(n.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			" <>a\0()", "text.Slice(p, found",
			["a", "<x\0> a", "<\0\0> a", "<x\0x\0> a", "<x> a", "< a", "a <\0>", "a <\0"]),

		new("scan, one character",
			"""
			trivia = { (' ' | '<' & (?!'\0' & [^ '\0' | '>'])* & '\0')* }
			Start : @int = n: ['a'..'z']+ => @(n.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			" <>a\0()", "text.Slice(p, found",
			["a", "<x\0 a", "<> a", "<x>\0 a", "< a", "a <\0", "a <"]),

		new("too many",
			"""
			Body  = (?!"*/" & [^ 'a' | 'b' | 'c' | 'd' | 'e'])*
			Start : @int = "/*" & b: Body & ("*/" | ['a'..'e']) => @(b.Length)
			      | '(' & inner: Start & ')' => @(inner)
			""",
			"/*abex ()", null,
			["/* x */", "/* xa", "/* */", "/*"]),
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
		var seen   = false;

		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var (fast, searched) = Build(row.Grammar + "\nparse Start\n", carrier, direct, row.Searches);
			var (slow, _)        = Build(row.Grammar + "\nparse Start\n", carrier, direct, null, oneByOne: true);

			seen |= searched;

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

		Assert.True(
			seen == (row.Searches is not null),
			$"{row.Name}: {(row.Searches is null ? "a search where none may be" : $"no '{row.Searches}' in any rendering")}");

		// A `find` beside the `parse`.
		{
			var (fast, _) = Build(row.Grammar + "\nparse Start\nfind Start\n", CarrierKind.Auto, direct: false, row.Searches);
			var (slow, _) = Build(row.Grammar + "\nparse Start\nfind Start\n", CarrierKind.Auto, direct: false, null, oneByOne: true);

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
	/// Over a reader the run's and the until scan's searches ask the buffer: the same answers as the
	/// loop, through a reader handing the text over a few characters at a time.
	/// </summary>
	[Theory]
	[InlineData("run, one stop")]
	[InlineData("run, several")]
	[InlineData("until")]
	[InlineData("until, delimiter in S")]
	[InlineData("until, given back")]
	public void A_buffered_search_answers_as_the_loop(string name)
	{
		var row  = Rows.Single(one => one.Name == name);
		var fast = BuildBuffered(row.Grammar);
		var slow = BuildBuffered(row.Grammar, oneByOne: true);

		foreach (var input in Inputs(row))
		{
			var expected = Read(slow, input);
			var actual   = Read(fast, input);

			Assert.True(
				expected.Equals(actual),
				$"{row.Name}, buffered, \"{Escaped(input)}\": the loop says {expected}, the search {actual}");
		}
	}

	/// <summary>
	/// A long body with characters of <c>S</c> and near misses of the delimiter at any distance:
	/// one search finds what the loop finds.
	/// </summary>
	[Fact]
	public void A_long_body_is_searched_to_where_it_stops()
	{
		var row        = Rows.Single(one => one.Name == "until");
		var (fast, _)  = Build(row.Grammar + "\nparse Start\n", CarrierKind.Auto, false, row.Searches);
		var (slow, _)  = Build(row.Grammar + "\nparse Start\n", CarrierKind.Auto, false, null, oneByOne: true);
		var body       = new StringBuilder();

		for (var i = 0; i < 2000; i++)
			body.Append(i % 7 == 0 ? "* /" : i % 5 == 0 ? "**" : "text ");

		foreach (var input in new[] { "/*" + body + "*/", "/*" + body + "\0*/", "/*" + body })
			Assert.Equal(
				EmittedCode.Match(slow, "Grammar", "TryParseStart", input),
				EmittedCode.Match(fast, "Grammar", "TryParseStart", input));
	}

	/// <param name="searches">What the source must hold where it searches; null where it must not search.</param>
	/// <param name="oneByOne">Whether the turns of <c>[^ S]</c> are read one character at a time.</param>
	static (Assembly Assembly, bool Searched) Build(string grammar, CarrierKind carrier, bool direct, string? searches, bool oneByOne = false)
	{
		var source   = Compiled(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
		}, oneByOne);
		var searched = searches is not null && source.Contains(searches, StringComparison.Ordinal);

		return (EmittedCode.Compile(source), searched);
	}

	static Assembly BuildBuffered(string grammar, bool oneByOne = false)
	{
		return EmittedCode.Compile(Compiled(grammar + "\nparse Start\n", new GramCompilerOptions
		{
			ClassName = "Grammar", BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance,
		}, oneByOne));
	}

	static string Compiled(string grammar, GramCompilerOptions options, bool oneByOne)
	{
		var flag = (AsyncLocal<bool>)typeof(CSharpEmitter).Assembly.GetType("DotGram.Grammar.Emit.Machine", throwOnError: true)!
			.GetField("ReadsFewOneByOne", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

		flag.Value = oneByOne;

		try
		{
			var result = GramCompiler.Compile(grammar, options);

			EmittedCode.Quiet(result.Diagnostics);

			return Assert.Single(result.Sources).Text;
		}
		finally
		{
			flag.Value = false;
		}
	}

	static (bool Success, object? Value, long Position, string? Error) Read(Assembly assembly, string input)
	{
		var method = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(TextReader), typeof(int?), typeof(int?)])!;
		var match  = method.Invoke(null, [new ShortReader(input), 4, null])!;

		object? Get(string property)
		{
			return match.GetType().GetProperty(property)!.GetValue(match);
		}

		return ((bool)Get("IsSuccess")!, Get("Value"), (long)Get("Position")!, (string?)Get("Error"));
	}

	/// <summary>A reader that hands the text over three characters at a time.</summary>
	sealed class ShortReader(string text) : TextReader
	{
		int _position;

		public override int Read(char[] buffer, int index, int count)
		{
			var length = Math.Min(Math.Min(3, count), text.Length - _position);

			text.CopyTo(_position, buffer, index, length);
			_position += length;

			return length;
		}
	}

	/// <summary>The row's own inputs, and short random ones over its alphabet.</summary>
	static IEnumerable<string> Inputs(Row row)
	{
		var random = new Random(row.Name.Length * 7919 + row.Grammar.Length);
		var made   = new List<string>(row.Inputs) { "" };

		for (var i = 0; i < 150; i++)
		{
			var built = new StringBuilder();

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
