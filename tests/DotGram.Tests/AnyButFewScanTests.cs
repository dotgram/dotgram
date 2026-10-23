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
/// <c>[^ S]</c>, anything but a few characters: an until scan (Machine.UntilStops), which passes
/// over the turns that are taken and reads the one that ends the loop as the loop does, and a
/// scanned rule's delimiter search (Machine.ScansUntil). Each row is held against the same grammar
/// compiled with those turns read one character at a time (Machine.ReadsFewOneByOne), which is the
/// loop the search replaces: the two must answer alike — what was read, how far, and what a
/// refusal says and where, to the last item of what it expected. A run (Machine.StopCharacters)
/// does not take the shape: it would end without reading the turn whose refusal the loop says.
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

	/// <summary>
	/// The shapes a search may stand for, each held against its loop over every input up to a
	/// length: where a refusal is said and what it says, the value, and what a <c>find</c> finds —
	/// over both carriers, the engine and the reader, and a buffered reading refilled a character
	/// at a time.
	/// </summary>
	/// <remarks>
	/// Short inputs over a few characters are where a search and its loop part: a delimiter cut
	/// off by the end of the input (<c>/*a*</c>), a character of <c>S</c> inside the delimiter or
	/// right before it, a turn refused at the very end. Every one is read.
	/// </remarks>
	[Theory]
	[MemberData(nameof(DifferentialNames))]
	public void Every_short_input_is_refused_as_the_loop_refuses_it(string name)
	{
		var row        = Differential.Single(one => one.Name == name);
		var inputs     = Exhaustive(row.Alphabet, row.Length).Concat(row.Inputs).Distinct().ToArray();
		var compared   = 0;
		var seen       = false;
		var grammar    = row.Grammar + "\nparse Start\n";
		var publishing = grammar + (row.Finds ? "find Start\n" : "");

		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var (fast, searched) = Build(publishing, carrier, direct, row.Searches);
			var (slow, _)        = Build(publishing, carrier, direct, null, oneByOne: true);

			seen |= searched;

			foreach (var input in inputs)
			{
				var expected = Answer(slow, input);
				var actual   = Answer(fast, input);

				compared++;

				Assert.True(
					expected == actual,
					$"{row.Name}, {carrier}, direct {direct}, \"{Escaped(input)}\": the loop says {expected}, the search {actual}");

				if (!row.Finds)
					continue;

				var found  = EmittedCode.Found(slow, "Grammar", "FindStart", input);
				var search = EmittedCode.Found(fast, "Grammar", "FindStart", input);

				compared++;

				Assert.True(
					found.SequenceEqual(search),
					$"{row.Name}, find, {carrier}, direct {direct}, \"{Escaped(input)}\": the loop finds " +
					$"[{string.Join(", ", found)}], the search [{string.Join(", ", search)}]");
			}
		}

		{
			var fast = BuildBuffered(row.Grammar);
			var slow = BuildBuffered(row.Grammar, oneByOne: true);

			foreach (var input in inputs)
				foreach (var window in new int?[] { 4, null })
				{
					var expected = Read(slow, input, window);
					var actual   = Read(fast, input, window);

					compared++;

					Assert.True(
						expected.Equals(actual),
						$"{row.Name}, buffered {window}, \"{Escaped(input)}\": the loop says {expected}, the search {actual}");
				}
		}

		Assert.True(seen || row.Searches is null, $"{row.Name}: no '{row.Searches}' in any rendering");
		TestContext.Current.TestOutputHelper?.WriteLine($"{row.Name}: {compared} readings compared, none apart");
	}

	/// <summary>
	/// The same over segments, in a grammar that names <c>eos</c>, where <c>any</c> is a turn of
	/// <c>[^ S]</c> that refuses the end of a segment: every list of segments up to a length, and
	/// each of them read from a reader as its lines.
	/// </summary>
	[Theory]
	[MemberData(nameof(SegmentedNames))]
	public void Every_short_sequence_is_refused_as_the_loop_refuses_it(string name)
	{
		var row      = Segmented.Single(one => one.Name == name);
		var inputs   = SegmentLists(row.Alphabet, row.Length).Concat(row.Inputs.Select(one => one.Split('|'))).ToArray();
		var compared = 0;
		var seen     = false;

		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var (fast, searched) = Build(row.Grammar + "\nparse Start\n", carrier, direct, row.Searches);
			var (slow, _)        = Build(row.Grammar + "\nparse Start\n", carrier, direct, null, oneByOne: true);

			seen |= searched;

			foreach (var input in inputs)
			{
				foreach (var over in new[] { typeof(IReadOnlyList<string>), typeof(TextReader) })
				{
					var expected = Answer(slow, input, over);
					var actual   = Answer(fast, input, over);

					compared++;

					Assert.True(
						expected == actual,
						$"{row.Name}, {carrier}, direct {direct}, {over.Name}, [{string.Join(", ", input.Select(one => $"\"{Escaped(one)}\""))}]: " +
						$"the loop says {expected}, the search {actual}");
				}
			}
		}

		Assert.True(seen || row.Searches is null, $"{row.Name}: no '{row.Searches}' in any rendering");
		TestContext.Current.TestOutputHelper?.WriteLine($"{row.Name}: {compared} readings compared, none apart");
	}

	/// <param name="Searches">
	/// What the fast parser's source holds where a search stands for the loop; null for a shape
	/// that is left to the loop, held here so that it stays where it is told the same.
	/// </param>
	/// <param name="Length">The longest input read, every one over <paramref name="Alphabet"/> up to it.</param>
	sealed record Shape(string Name, string Grammar, string Alphabet, string? Searches, int Length, string[] Inputs, bool Finds = true);

	static readonly Shape[] Differential =
	[
		// A run before a one-character stop, refused at the end and at a character of S.
		new("run",
			"""
			Body  = (?!',' & [^ '\0' | '\n'])*
			Start : @int = t: Body & (',' | ';') => @(t.Length)
			""",
			",;\0\na", "var until", 5, []),

		// A run that must turn at least once, left to the loop.
		new("run, plus",
			"""
			Body  = (?!',' & [^ ';'])+
			Start : @int = b: Body & (',' | ';') & 'z' => @(b.Length)
			""",
			",;az", null, 6, []),

		new("run, before more",
			"""
			Start : @int = b: ((?!',' & [^ ';'])*) & ',' & 'z' => @(b.Length)
			      | "a;" => @(-1)
			""",
			",;az", "var until", 6, []),

		// An until scan whose delimiter is cut off by the end, or holds a character of S.
		new("until",
			"""
			Body  = (?!"*/" & [^ ';'])*
			Start : @int = "/*" & b: Body & "*/" => @(b.Length)
			""",
			"/*;a", "var until", 6, ["/*a*", "/*ab", "/*a;*/", "/*a*;/"]),

		new("until, S in the delimiter",
			"""
			Body  = (?!"a;b" & [^ ';'])*
			Start : @int = '<' & b: Body & "a;b" => @(b.Length)
			""",
			"<a;b", "var until", 6, ["<a;", "<aa;b", "<a;c"]),

		new("until, three",
			"""
			Body  = (?!"-->" & [^ '\n'])*
			Start : @int = '<' & b: Body & "-->" & 'z' => @(b.Length)
			""",
			"<->\nz", "var until", 6, ["<--", "<-->z", "<a-->", "<--\n>z"]),

		new("until, first",
			"""
			Start : @int = W & 'z' => @(0)
			W = (?!"*/" & [^ ';'])* & "*/" | "a;"
			""",
			"*/;az", "var until", 6, ["a;z", "a*/z", "a*;/z"]),

		// A scanned rule's search for its delimiter.
		new("scan",
			"""
			Start : @int = W => @(0)
			W = "/*" & (?!"*/" & [^ ';'])* & "*/"
			""",
			"/*;a", "text.Slice(p, found", 6, ["/*ab", "/*a*", "/*a;", "/*a*/"]),

		new("scan, S in the delimiter",
			"""
			Start : @int = W => @(0)
			W = '<' & (?!";>" & [^ ';' | '\n'])* & ";>"
			""",
			"<;>\na", "text.Slice(p, found", 6, ["<a;", "<a;b;>", "<\n;>"]),

		new("scan, trivia",
			"""
			trivia = { (' ' | "/*" & (?!"*/" & [^ ';'])* & "*/")* }
			Start : @int = n: ['a'..'z']+ => @(n.Length)
			""",
			" /*;a", "text.Slice(p, found", 6, ["a /* ;", "/* a */ a", "a /*"]),
	];

	static readonly Shape[] Segmented =
	[
		// A rule of its own, scanned, whose run is left to the loop; and the same turns in place.
		new("eos, run",
			"""
			Body  = (?!',' & any)*
			Start : @int = b: Body & (',' | eos) & eos & eof => @(b.Length)
			""",
			",a\n", null, 6, ["a,|", "a|", ",|"]),

		new("eos, run in place",
			"""
			Start : @int = b: ((?!',' & any)*) & (',' | eos) & eos & eof => @(b.Length)
			""",
			",a\n", "var until", 6, ["a,|", "a|", ",|"]),

		new("eos, until",
			"""
			Body  = (?!"*/" & any)*
			Start : @int = "/*" & b: Body & "*/" & eos => @(b.Length)
			""",
			"/*a", "var until", 6, ["/*a*", "/*ab", "/*a*/", "/*a|*/", "/*a*|/"]),

		new("eos, scan",
			"""
			Start : @int = W & eos & (W & eos)? => @(0)
			W = "/*" & (?!"*/" & any)* & "*/"
			""",
			"/*a", "text.Slice(p, found", 6, ["/*ab", "/*a*", "/*a*/|/*", "/*/|*/"]),

		new("eos, trivia",
			"""
			trivia = { (' ' | "/*" & (?!"*/" & any)* & "*/")* }
			Start : @int = n: ['a'..'z']+ & eos => @(n.Length)
			""",
			" /*a", "text.Slice(p, found", 6, ["a /* ", "/* a */ a", "a /*|*/"]),
	];

	public static TheoryData<string> DifferentialNames
	{
		get
		{
			var rows = new TheoryData<string>();

			foreach (var row in Differential)
				rows.Add(row.Name);

			return rows;
		}
	}

	public static TheoryData<string> SegmentedNames
	{
		get
		{
			var rows = new TheoryData<string>();

			foreach (var row in Segmented)
				rows.Add(row.Name);

			return rows;
		}
	}

	/// <summary>Every string over <paramref name="alphabet"/> no longer than <paramref name="length"/>.</summary>
	static IEnumerable<string> Exhaustive(string alphabet, int length)
	{
		var level = new List<string> { "" };

		for (var i = 0; i <= length; i++)
		{
			foreach (var one in level)
				yield return one;

			level = [.. level.SelectMany(one => alphabet.Select(c => one + c))];
		}
	}

	/// <summary>
	/// Every list of segments over <paramref name="alphabet"/> whose characters and boundaries
	/// together are no more than <paramref name="length"/>, the empty list among them.
	/// </summary>
	static IEnumerable<string[]> SegmentLists(string alphabet, int length)
	{
		// A boundary is one more letter: a string over the alphabet and '|' is a list once split.
		yield return [];

		foreach (var text in Exhaustive(alphabet + "|", length))
			if (text.Length > 0 && text[^1] == '|')
				yield return text.Substring(0, text.Length - 1).Split('|');
	}

	/// <summary>What a reading answers, all of it: whether, the value, where, how much and what it says.</summary>
	static string Answer(Assembly assembly, string input)
	{
		return Answered(assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!);
	}

	/// <summary>The same over segments, as a list or as the lines of a reader.</summary>
	static string Answer(Assembly assembly, string[] segments, Type over)
	{
		object input = over == typeof(TextReader) ? new StringReader(string.Concat(segments.Select(one => one + "\n"))) : segments;

		// A line ending inside a segment is two segments to a reader: only lists that a reader can say.
		if (over == typeof(TextReader) && segments.Any(one => one.Contains('\n')))
			return "";

		return Answered(assembly.GetType("Grammar")!.GetMethod("TryParseStart", [over])!.Invoke(null, [input])!);
	}

	static string Answered(object match)
	{
		object? Get(string property)
		{
			return match.GetType().GetProperty(property)!.GetValue(match);
		}

		return $"{Get("Outcome")} {Get("Value")} at {Get("Position")}+{Get("Length")}: {Get("Error")}";
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

	static (bool Success, object? Value, long Position, string? Error) Read(Assembly assembly, string input, int? window = 4)
	{
		var method = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(TextReader), typeof(int?), typeof(int?)])!;
		var match  = method.Invoke(null, [new ShortReader(input), window, null])!;

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
