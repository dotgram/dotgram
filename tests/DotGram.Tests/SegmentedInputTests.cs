using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <c>eos</c>, the end of a segment (docs/syntax.md §3.1.1): a grammar that names it reads a
/// sequence of segments, has no character at a boundary, and publishes forms over sequences only.
/// </summary>
public sealed class SegmentedInputTests
{
	/// <summary>Command-line arguments: an option and its value are two segments.</summary>
	const string Argv = """
		Args   : @string[] = items: Arg* => @(items)
		Arg    : @string   = ("--output" | "-o") & eos & v: Value & eos => @("out=" + v)
		                   | v: Value & eos => @("arg=" + v)
		Value  = any*
		parse Args
		""";

	[Theory]
	[InlineData(new string[0], "")]
	[InlineData(new[] { "" }, "arg=")]
	[InlineData(new[] { "a" }, "arg=a")]
	[InlineData(new[] { "a", "" }, "arg=a,arg=")]
	[InlineData(new[] { "-o", "file", "x y" }, "out=file,arg=x y")]
	[InlineData(new[] { "--output", "" }, "out=")]
	[InlineData(new[] { "--output" }, "arg=--output")]
	[InlineData(new[] { "line\nbreak", "-o", "a\r\nb" }, "arg=line\nbreak,out=a\r\nb")]
	public void Arguments_are_read_one_segment_at_a_time(string[] arguments, string expected)
	{
		foreach (var direct in new[] { true, false })
			Read(Build(Argv, direct: direct), arguments, expected);
	}

	static void Read(Assembly parser, string[] arguments, string expected)
	{
		var (success, value, _, _) = TryParse(parser, "TryParseArgs", arguments);

		Assert.True(success);
		Assert.Equal(expected, string.Join(",", (string[])value!));

		// The same through a sequence that is not a list, and through the form that throws.
		Assert.Equal(expected, string.Join(",", (string[])Invoke(parser, "ParseArgs", typeof(IEnumerable<string>), arguments.Select(one => one))!));
		Assert.Equal(expected, string.Join(",", (string[])Invoke(parser, "ParseArgs", typeof(IReadOnlyList<string>), arguments)!));
	}

	/// <summary>
	/// Every segment ends with an <c>eos</c> and the input with <c>eof</c> after the last: no segment
	/// reads as <c>eof</c> alone, one empty segment as <c>eos eof</c>.
	/// </summary>
	[Theory]
	[InlineData(new string[0], "eof")]
	[InlineData(new[] { "" }, "eos eof")]
	[InlineData(new[] { "a" }, "a eos eof")]
	[InlineData(new[] { "a", "" }, "a eos eos eof")]
	[InlineData(new[] { "a", "b" }, "a eos b eos eof")]
	public void Boundaries_are_where_the_contract_puts_them(string[] segments, string expected)
	{
		var parser = Build("""
			Input : @string = parts: Part* & eof => @(string.Join(" ", parts) + (parts.Length > 0 ? " " : "") + "eof")
			Part  : @string = eos => @("eos") | t: ['a'..'z'] => @(t)
			parse Input
			""");

		var (success, value, _, _) = TryParse(parser, "TryParseInput", segments);

		Assert.True(success);
		Assert.Equal(expected, value);
	}

	/// <summary>
	/// A position is one per character and one per boundary; a refusal at a boundary says the end of
	/// a segment, and one past the last says the end of the input.
	/// </summary>
	[Fact]
	public void A_refusal_says_where_a_segment_ended()
	{
		var parser = Build("""
			Pair : @string = k: Key & '=' & v: Key & eos => @(k + "=" + v)
			Key  = ['a'..'z']+
			parse Pair
			""");

		// Where the segment goes on past what the rule reads, the end of the segment is what it wanted.
		var (success, _, error, position) = TryParse(parser, "TryParsePair", ["a=b c"]);

		Assert.False(success);
		Assert.Equal(3, position);
		Assert.Equal("Expected end of segment.", error);

		// Where it ends before the rule does, the position is the boundary's, which nothing can read.
		(success, _, error, position) = TryParse(parser, "TryParsePair", ["ab"]);

		Assert.False(success);
		Assert.Equal(2, position);
		Assert.DoesNotContain("\\u0000", error!, StringComparison.Ordinal);

		// A boundary is a position of its own: the second segment begins one past the first's end,
		// and a whole reading wants the input to end there.
		(success, _, error, position) = TryParse(parser, "TryParsePair", ["a=b", "c"]);

		Assert.False(success);
		Assert.Equal(4, position);
		Assert.Equal("Expected end of input.", error);

		var thrown = Assert.Throws<TargetInvocationException>(() => Invoke(parser, "ParsePair", typeof(IReadOnlyList<string>), new[] { "ab" }));

		Assert.IsType<FormatException>(thrown.InnerException);
	}

	/// <summary>
	/// A segment that holds U+0000 is refused at it, before anything is read: the forms that answer
	/// say so there, and the one that throws throws.
	/// </summary>
	[Fact]
	public void A_segment_that_holds_the_joiner_is_refused()
	{
		var parser = Build(Argv);

		var (success, _, error, position) = TryParse(parser, "TryParseArgs", ["-o", "a\0b"]);

		Assert.False(success);
		Assert.Equal(4, position);
		Assert.Contains("U+0000", error!, StringComparison.Ordinal);

		var answered = parser.GetType("Grammar")!.GetMethod("TryParseArgs", [typeof(IReadOnlyList<string>), typeof(string[]).MakeByRefType()])!;
		var arguments = new object?[] { new[] { "\0" }, null };

		Assert.False((bool)answered.Invoke(null, arguments)!);

		var thrown = Assert.Throws<TargetInvocationException>(() => Invoke(parser, "ParseArgs", typeof(IReadOnlyList<string>), new[] { "x", "\0" }));

		Assert.Contains("U+0000", Assert.IsType<FormatException>(thrown.InnerException).Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// What a grammar that names <c>eos</c> publishes takes sequences and nothing else: no string, no
	/// reader and no position, and the forms over text it reads through are its own.
	/// </summary>
	[Fact]
	public void Only_sequences_are_published()
	{
		var type   = Build(Argv).GetType("Grammar")!;
		var published = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(one => one.DeclaringType == type).ToArray();

		Assert.NotEmpty(published);

		foreach (var method in published)
			Assert.True(
				method.GetParameters() is [var first, ..] &&
				(first.ParameterType == typeof(IReadOnlyList<string>) || first.ParameterType == typeof(IEnumerable<string>)),
				$"{method} takes no sequence");

		Assert.Equal(
			["ParseArgs", "ParseArgs", "TryParseArgs", "TryParseArgs", "TryParseArgs", "TryParseArgs"],
			published.Select(one => one.Name).Order(StringComparer.Ordinal));
	}

	/// <summary>
	/// The until idioms of the library stop at a boundary as at the end of the input: a string, a
	/// comment or a pair left open there is refused, not read on into the next segment.
	/// </summary>
	[Theory]
	[InlineData("Std.Quoted('\\'')", "'ab'", "'a", "b'")]
	[InlineData("Std.Escaped('\"', '\\\\')", "\"a\\\"b\"", "\"a\\", "\"b\"")]
	[InlineData("Std.BlockComment(\"/*\", \"*/\")", "/* a */", "/* a", " */")]
	[InlineData("Std.Nested(\"(\", \")\")", "(a(b)c)", "(a(b", "))")]
	public void The_library_stops_at_a_boundary(string rule, string whole, string open, string rest)
	{
		var parser = Build($$"""
			Items : @string[] = items: Item* => @(items)
			Item  : @string   = t: Lexeme & eos => @(t)
			Lexeme = {{rule}}
			parse Items
			""");

		var (success, value, _, _) = TryParse(parser, "TryParseItems", [whole]);

		Assert.True(success, $"{rule} over \"{whole}\"");
		Assert.Equal(whole, Assert.Single((string[])value!));

		(success, _, _, var position) = TryParse(parser, "TryParseItems", [open, rest]);

		Assert.False(success, $"{rule} read \"{open}\" on into \"{rest}\"");
		Assert.True(position <= open.Length, $"{rule}: refused at {position}, past the end of \"{open}\"");
	}

	/// <summary>
	/// A choice that reads past a boundary and gives it back answers as the same grammar over one
	/// string with a character of its own where each boundary was.
	/// </summary>
	[Theory]
	[InlineData("a|b")]
	[InlineData("a|b|c")]
	[InlineData("ab||b")]
	[InlineData("a|x")]
	[InlineData("|")]
	[InlineData("a")]
	[InlineData("a|b|c|d|c")]
	public void A_rewind_across_a_boundary_answers_as_a_placeholder_would(string written)
	{
		var segments = written.Split('|');

		const string grammar = """
			Input : @string = parts: Part* & eof => @(string.Join("|", parts))
			Part  : @string = a: Word & eos & b: Word & eos & "c" & eos => @("3:" + a + b)
			                | a: Word & eos & b: Word & eos => @("2:" + a + b)
			                | a: Word & eos => @("1:" + a)
			Word  : @string = t: (any*) => @(t)
			parse Input
			""";

		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var segmented = Build(grammar, carrier: carrier, direct: direct);
			var placed    = Build(grammar.Replace("eos", "'\\u0001'").Replace("any*", "[^ '\\u0001']*"), segmented: false, carrier: carrier, direct: direct);

			var expected = EmittedCode.Match(placed, "Grammar", "TryParseInput", string.Concat(segments.Select(one => one + "\u0001")));
			var (success, value, _, position) = TryParse(segmented, "TryParseInput", segments);

			Assert.Equal(expected.IsSuccess, success);
			Assert.Equal(expected.Value, value);
			Assert.Equal(expected.Position, position);
		}
	}

	/// <summary>Text kept across a boundary is refused: there is no text of a boundary to keep.</summary>
	[Theory]
	[InlineData("Start : @string = t: (Word & eos & Word) => @(t)\nWord = ['a'..'z']*\nparse Start")]
	[InlineData("Start : @int = $w: (Word & eos) & Word & eos => @(0)\nWord = ['a'..'z']*\nparse Start")]
	[InlineData("Start = Word & eos & Word & eos\nWord = ['a'..'z']*\nparse Start")]
	public void Text_across_a_boundary_is_refused(string grammar)
	{
		var result = Compile(grammar);

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.TextAcrossSegments);
	}

	/// <summary>Values built on either side of a boundary may be kept; text inside one segment may.</summary>
	[Fact]
	public void Values_across_a_boundary_are_kept()
	{
		var parser = Build("""
			Rows : @string[] = (rows: Row & eos)* => @(rows)
			Row  : @string   = t: (['a'..'z']*) => @(t)
			parse Rows
			""");

		var (success, value, _, _) = TryParse(parser, "TryParseRows", ["ab", "", "c"]);

		Assert.True(success);
		Assert.Equal(["ab", "", "c"], (string[])value!);
	}

	/// <summary>U+0000 is the end of a segment, so a literal or a class that names it is refused.</summary>
	[Theory]
	[InlineData("Start = \"a\\0\" & eos\nparse Start")]
	[InlineData("Start = ['\\0'..' '] & eos\nparse Start")]
	[InlineData("Start = [\\p{Cc}] & eos\nparse Start")]
	public void The_joiner_is_no_character_of_a_segment(string grammar)
	{
		Assert.Contains(Compile(grammar).Diagnostics, one => one.Id == GrammarNormalizer.JoinerInSegmentedGrammar);
	}

	[Fact]
	public void A_lexical_grammar_that_names_eos_is_refused()
	{
		var result = Compile("trivia = { ' '* }\nStart = Word & eos\nWord = ['a'..'z']+\nparse Start", lexical: true);

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.LexicalSegments);
	}

	[Fact]
	public void A_reader_or_bytes_form_of_a_grammar_that_names_eos_is_refused()
	{
		var result = GramCompiler.Compile("Start = ['a'..'z']* & eos\nparse Start", new GramCompilerOptions
		{
			ClassName = "Grammar", BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.BufferedSegments);
	}

	/// <summary>
	/// A predicate of the host in a class is not asked about the joiner: a class that calls one stops
	/// at a boundary whatever it answers.
	/// </summary>
	[Fact]
	public void A_predicate_is_not_asked_about_the_end_of_a_segment()
	{
		var parser = Build(
			"""
			Items : @string[] = items: Item* => @(items)
			Item  : @string   = t: ([@Anything]*) & eos => @(t)
			parse Items
			""",
			members: "static bool Anything(char c) { return true; }");

		var (success, value, _, _) = TryParse(parser, "TryParseItems", ["ab", "c"]);

		Assert.True(success);
		Assert.Equal(["ab", "c"], (string[])value!);
	}

	/// <summary>
	/// Records, one a segment, with bad ones in the middle: <c>recover eos</c> steps over a bad one to
	/// the end of its segment, and what a rejected record is told counts lines as segments.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Tape, true)]
	[InlineData(CarrierKind.Immediate, true)]
	[InlineData(CarrierKind.Tape, false)]
	[InlineData(CarrierKind.Immediate, false)]
	public void A_bad_record_is_stepped_over_to_the_end_of_its_segment(CarrierKind carrier, bool direct)
	{
		var parser = Build("""
			Feed   : @string[] = records: Record* recover eos => @("bad " + parserLine + ":" + parserFailureColumn) & eof => @(records)
			Record : @string   = n: Number & '|' & t: Text & eos => @(n + "=" + t)
			Number = ['0'..'9']+
			Text   = [^ '|']+
			parse Feed
			""", carrier: carrier, direct: direct);

		var (success, value, _, _) = TryParse(parser, "TryParseFeed", ["1|a", "x|b", "2|c\nd", "3|", "4|e"]);

		Assert.True(success);
		Assert.Equal(["1=a", "bad 2:1", "2=c\nd", "bad 4:3", "4=e"], (string[])value!);
	}

	/// <summary>
	/// The same records streamed from a sequence that is read as it is asked for: the same values,
	/// and a segment that holds U+0000 throws when it is reached.
	/// </summary>
	[Fact]
	public void Records_stream_from_a_sequence()
	{
		var parser = Build("""
			Feed   : @string[] = records: Record* recover eos => @("bad " + parserLine) & eof => @(records)
			Record : @string   = n: Number & '|' & t: Text & eos => @(n + "=" + t)
			Number = ['0'..'9']+
			Text   = [^ '|']+
			parse Feed
			""");

		var streamed = ((IEnumerable<string>)Invoke(parser, "ParseFeed", typeof(IEnumerable<string>), Lazily(["1|a", "x|b", "2|c"]))!).ToArray();

		Assert.Equal(["1=a", "bad 2", "2=c"], streamed);

		var whole = Invoke(parser, "ParseFeed", typeof(IReadOnlyList<string>), new[] { "1|a", "x|b", "2|c" });

		Assert.Equal(streamed, (string[])whole!);

		var thrown = Assert.ThrowsAny<Exception>(() => ((IEnumerable<string>)Invoke(parser, "ParseFeed", typeof(IEnumerable<string>), Lazily(["1|a", "2|\0"]))!).ToArray());

		Assert.Contains("U+0000", (thrown.InnerException ?? thrown).Message, StringComparison.Ordinal);

		static IEnumerable<string> Lazily(string[] lines)
		{
			foreach (var line in lines)
				yield return line;
		}
	}

	/// <summary>
	/// A <c>find</c> and a <c>yield</c> over segments: occurrences are found in each segment, at
	/// positions of the joined reading, and elements are handed over one at a time.
	/// </summary>
	[Fact]
	public void Finds_and_yields_take_segments()
	{
		var parser = Build("""
			Item  : @int = w: Word & eos => @(w.Length)
			Items : @int[] = Item*
			Word  = ['a'..'z']+
			parse Items as Lengths yield : @int
			find Word
			""");
		var type   = parser.GetType("Grammar")!;

		var found = ((System.Collections.IEnumerable)type.GetMethod("FindWord", [typeof(IReadOnlyList<string>)])!.Invoke(null, [new[] { "ab cd", "ef" }])!)
			.Cast<object>()
			.Select(one => (long)one.GetType().GetProperty("Position")!.GetValue(one)!)
			.ToArray();

		Assert.Equal([0L, 3L, 6L], found);

		var lengths = ((IEnumerable<int>)type.GetMethod("Lengths", [typeof(IEnumerable<string>)])!.Invoke(null, [new[] { "abc", "d", "ef" }.Select(one => one)])!).ToArray();

		Assert.Equal([3, 1, 2], lengths);
	}

	/// <summary>A grammar that does not name <c>eos</c> reads U+0000 as a character, as it always did.</summary>
	[Fact]
	public void Without_eos_U0000_is_a_character()
	{
		var parser = Build("Start : @string = t: (any*) => @(t)\nparse Start", segmented: false);

		Assert.Equal("a\0b", EmittedCode.Match(parser, "Grammar", "TryParseStart", "a\0b").Value);
	}

	static GramCompilation Compile(string grammar, bool lexical = false, CarrierKind carrier = CarrierKind.Auto, bool direct = true)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Lexical = lexical, Carrier = carrier, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
		});
	}

	static Assembly Build(
		string grammar, bool segmented = true, string? members = null, CarrierKind carrier = CarrierKind.Auto, bool direct = true)
	{
		var result = Compile(grammar, carrier: carrier, direct: direct);

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		Assert.Equal(segmented, source.Contains("Joined_DotGram", StringComparison.Ordinal));

		return EmittedCode.Compile(source, declarationMembers: members);
	}

	static object? Invoke(Assembly assembly, string method, Type over, object input)
	{
		return assembly.GetType("Grammar")!.GetMethod(method, [over])!.Invoke(null, [input]);
	}

	static (bool Success, object? Value, string? Error, long Position) TryParse(Assembly assembly, string method, string[] segments)
	{
		var match = Invoke(assembly, method, typeof(IReadOnlyList<string>), segments)!;

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return ((bool)Read("IsSuccess")!, Read("Value"), (string?)Read("Error"), (long)Read("Position")!);
	}
}
