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
	/// What a refusal says is in the grammar's own words, never U+0000: <c>any</c> as <c>any</c>, a
	/// class or a look without the end of a segment its lowering left out, and nothing at all for
	/// the look put before a class that may not read the end of a segment.
	/// </summary>
	[Theory]
	[InlineData("Start : @int = 'a' & any & eos => @(0)", "a", 1, "Expected any.")]
	[InlineData("Start : @int = 'a' & [^ ','] & eos => @(0)", "a", 1, "Expected [^ ','].")]
	[InlineData("Start : @int = ?=[^ ','] & any* & eos => @(0)", ",", 0, "Expected ?=[^ ','].")]
	[InlineData("Start : @int = ?![^ ','] & any* & eos => @(0)", "a", 0, "Expected ?![^ ','].")]
	[InlineData("Start : @string = t: [\\p{Cc} | 'a'..'z']+ & 'q' & eos => @(t)", "ab", 2, "Expected 'q'.")]
	public void A_refusal_says_what_the_grammar_wrote(string grammar, string segment, long position, string said)
	{
		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var (success, _, error, at) = TryParse(Build(grammar + "\nparse Start", carrier: carrier, direct: direct), "TryParseStart", [segment]);

			Assert.False(success);
			Assert.Equal(position, at);
			Assert.Equal(said, error);
		}
	}

	/// <summary>
	/// The form that throws says where as a line and a column, which over segments are the segment
	/// and the offset in it, both from 1: a <c>\n</c> inside a segment is a character like another.
	/// </summary>
	[Fact]
	public void A_thrown_refusal_says_the_segment_and_the_offset_in_it()
	{
		var parser = Build("""
			Pair : @string = k: Key & '=' & v: Key & eos => @(k + "=" + v)
			Key  = ['a'..'z' | '\n']+
			parse Pair
			""");

		var thrown = Assert.Throws<TargetInvocationException>(() => Invoke(parser, "ParsePair", typeof(IReadOnlyList<string>), new[] { "a\nb=c", "d" }));

		Assert.Equal("Expected end of input at line 2, column 1.", Assert.IsType<FormatException>(thrown.InnerException).Message);

		thrown = Assert.Throws<TargetInvocationException>(() => Invoke(parser, "ParsePair", typeof(IReadOnlyList<string>), new[] { "a\nb=c!" }));

		Assert.Equal("Expected end of segment at line 1, column 6.", Assert.IsType<FormatException>(thrown.InnerException).Message);
	}

	/// <summary>
	/// A publication over segments that cannot stream is told so as segments: its forms over a
	/// sequence and over a reader are there, and read the whole of it.
	/// </summary>
	[Fact]
	public void What_does_not_stream_is_said_of_segments()
	{
		var result = Compile("""
			Start : @int = (any* & eos)* & Last => @(0)
			Last  = "end" & eos
			find Start
			""");

		var said = Assert.Single(result.Diagnostics, one => one.Id == Retention.NotStreamable);

		Assert.Contains("reads a sequence of segments whole", said.Message, StringComparison.Ordinal);
		Assert.DoesNotContain("overload taking a reader", said.Message, StringComparison.Ordinal);
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
	/// What a grammar that names <c>eos</c> publishes takes sequences — a list, a sequence, or a reader
	/// read as lines — and nothing else: no string and no position, and the forms over text it reads
	/// through are its own.
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
				(first.ParameterType == typeof(IReadOnlyList<string>) || first.ParameterType == typeof(IEnumerable<string>) ||
				 first.ParameterType == typeof(TextReader)),
				$"{method} takes no sequence");

		Assert.Equal(
			["ParseArgs", "ParseArgs", "ParseArgs", "TryParseArgs", "TryParseArgs", "TryParseArgs", "TryParseArgs", "TryParseArgs", "TryParseArgs"],
			published.Select(one => one.Name).Order(StringComparer.Ordinal));
	}

	/// <summary>
	/// A publication may be called what the parser's own members are called but for their
	/// <c>_DotGram</c>: <c>Read</c>, <c>Joined</c>, <c>Located</c> and the rest meet none of them, and
	/// <c>Parse</c> meets no other publication's forms.
	/// </summary>
	[Fact]
	public void A_publication_may_be_called_what_the_parser_calls_its_own()
	{
		var parser = Build("""
			Word  = ['a'..'z']+
			Start : @int = w: Word & eos => @(w.Length)
			parse Start as Read
			parse Start as Parse
			parse Start as Joined
			parse Start as Located
			find Word as Listed
			find Word as Holds
			""");

		foreach (var name in new[] { "Read", "Parse", "Joined", "Located" })
		{
			var (success, value, _, _) = TryParse(parser, "Try" + name, ["abc"]);

			Assert.True(success, name);
			Assert.Equal(3, value);
			Assert.Equal(2, Invoke(parser, name, typeof(TextReader), new StringReader("ab\n")));
		}

		foreach (var name in new[] { "Listed", "Holds" })
			Assert.Equal(
				2,
				((System.Collections.IEnumerable)Invoke(parser, name, typeof(IReadOnlyList<string>), new[] { "ab", "cd" })!).Cast<object>().Count());
	}

	/// <summary>
	/// A class that carries a grammar naming <c>eos</c>, a second reading of it, a reading told where
	/// each value was, and inside it a grammar that does not name it: each compiles in its own
	/// scope and reads what it publishes, and the one told where reads positions as a match says
	/// them — one for every character and one for every boundary.
	/// </summary>
	[Fact]
	public void Readings_of_segments_sit_beside_a_grammar_of_text()
	{
		var assembly = GeneratorDriverTests.Build(""""
			namespace Segments
			{
				public interface ILoc
				{
					void Locate(int at, int length);

					void Locate(int at, int length, int gapStart);
				}

				public sealed class Name : ILoc
				{
					public int At = -1;

					public void Locate(int at, int length)
					{
						At = at;
					}

					public void Locate(int at, int length, int gapStart)
					{
						At = at;
					}
				}

				[DotGram.Gram("""
					Names : @Name[] = (names: Name & eos)* => @(names)
					Name  : @Name   = ['a'..'z']+ => @(new Name())
					parse Names
					""")]
				[DotGram.GramOptions(Carrier = DotGram.GramCarrier.Immediate, Suffix = "Immediate")]
				[DotGram.GramOptions(LocationType = typeof(ILoc), Suffix = "Located", PerCall = true)]
				public static partial class Args
				{
					[DotGram.Gram("""
						Word : @int = w: ['a'..'z']+ => @(w.Length)
						parse Word
						""")]
					public static partial class Text
					{
					}
				}
			}
			"""");

		int[] Read(string scope)
		{
			var names = (System.Array)assembly.GetType(scope)!.GetMethod("ParseNames", [typeof(IReadOnlyList<string>)])!.Invoke(null, [new[] { "ab", "c" }])!;

			return [.. names.Cast<object>().Select(one => (int)one.GetType().GetField("At")!.GetValue(one)!)];
		}

		Assert.Equal([-1, -1], Read("Segments.Args"));
		Assert.Equal([-1, -1], Read("Segments.Args+Immediate"));
		Assert.Equal([0, 3], Read("Segments.Args+Located"));

		Assert.Equal(3, assembly.GetType("Segments.Args+Text")!.GetMethod("ParseWord", [typeof(string)])!.Invoke(null, ["abc"]));
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

	/// <summary>
	/// A recovering repetition whose element reads boundaries reads it as an ordinary reading does
	/// (docs/syntax.md §8.2): an element that matches by giving back a turn of its own is an
	/// element, and the next one, refused with input left, is stepped over to the end. As the same
	/// grammar over one string with a character of its own where each boundary was.
	/// </summary>
	[Theory]
	[InlineData("|a\\", "0:,bad")]
	[InlineData("|a", "1:")]
	[InlineData("a\\", "bad")]
	[InlineData("||a|a\\", "0:,1:,bad")]
	[InlineData("|", "0:,0:")]
	public void A_recovering_element_gives_back_a_boundary_as_a_placeholder_would(string written, string expected)
	{
		var segments = written.Split('|');

		const string grammar = """
			Start : @string = parts: Part* recover 'x' => @("bad") & eof => @(string.Join(",", parts))
			Part  : @string = n: A* & eos => @(n.Length + ":")
			A     : @string = eos & t: 'a' => @(t)
			parse Start
			""";

		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var segmented = Build(grammar, carrier: carrier, direct: direct);
			var placed    = Build(grammar.Replace("eos", "'\\u0001'"), segmented: false, carrier: carrier, direct: direct);

			var (success, value, _, _) = TryParse(segmented, "TryParseStart", segments);

			Assert.True(success);
			Assert.Equal(expected, value);
			Assert.Equal(EmittedCode.Match(placed, "Grammar", "TryParseStart", string.Concat(segments.Select(one => one + "\u0001"))).Value, value);
		}
	}

	/// <summary>Text kept across a boundary is refused: there is no text of a boundary to keep.</summary>
	[Theory]
	[InlineData("Start : @string = t: (Word & eos & Word) => @(t)\nWord = ['a'..'z']*\nparse Start")]
	[InlineData("Start : @int = $w: (Word & eos) & Word & eos => @(0)\nWord = ['a'..'z']*\nparse Start")]
	[InlineData("Start = Word & eos & Word & eos\nWord = ['a'..'z']*\nparse Start")]
	[InlineData("Start : @string = t: ?=Whole & Whole => @(t)\nWhole = any* & eos & any* & eos\nparse Start")]
	[InlineData("Start : @int = $q: ?=(Word & eos) & Word & eos & $q => @(0)\nWord = ['a'..'z']*\nparse Start")]
	[InlineData("Start : @string = any* & eos & any* & eos => @(parserText)\nparse Start")]
	[InlineData("Start : @int = any* & eos & when @(parserText.Length > 0) & any* & eos => @(0)\nparse Start")]
	[InlineData("Start : @int = (any* & eos)* => @(parserInput.Length)\nparse Start")]
	[InlineData("Start : @int[] = items: Item* recover ';' => @(parserText.Length) & eof => @(items)\nItem : @int = ['a'..'z']+ & eos => @(0)\nparse Start")]
	[InlineData("Start : @int = $q: \"xx\" & (when @(parserText.Length > 0) & any* & eos){#q} & eof => @(0)\nparse Start")]
	[InlineData("Start : @int = $q: ['x']* & (when @(parserText.Length > 0) & any* & eos){#q,} & eof => @(0)\nparse Start")]
	[InlineData("Start : @int = $q: ['x']* & (t: (any* & eos)){1,#q} & eof => @(t.Length)\nparse Start")]
	public void Text_across_a_boundary_is_refused(string grammar)
	{
		var result = Compile(grammar);

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.TextAcrossSegments);
	}

	/// <summary>
	/// Rules that reach an <c>eos</c> only through each other: whether one does is answered for the
	/// whole grammar at once, so no rule is taken for one that reaches none because it was asked about
	/// while its own answer was still being worked out — whichever publication asks first.
	/// </summary>
	[Theory]
	[InlineData("Probe : @int = A => @(0)\nStart : @string = t: B => @(t)\nparse Probe\nparse Start")]
	[InlineData("Start : @string = t: B => @(t)\nProbe : @int = A => @(0)\nparse Start\nparse Probe")]
	[InlineData("Probe : @int = A => @(0)\nStart : @string = t: B => @(t)\nparse Start\nparse Probe")]
	[InlineData("Probe : @int = A => @(0)\nStart : @int = $q: B & $q => @(0)\nparse Probe\nparse Start")]
	[InlineData("Probe : @int = A => @(0)\nStart : @int = $q: B & 'z'{#q} & eos => @(0)\nparse Probe\nparse Start")]
	[InlineData("Probe : @int = A => @(0)\nStart : @string = B => @(parserText)\nparse Probe\nparse Start")]
	[InlineData("Probe : @int = A => @(0)\nStart : @int = B & when @(parserText.Length > 0) & eos => @(0)\nparse Probe\nparse Start")]
	[InlineData("Probe : @int = A => @(0)\nparse Probe\nfind B")]
	[InlineData("Probe : @int = A => @(0)\nItems : @string[] = (B => @(parserText))*\nparse Probe\nparse Items as Bs yield : @string")]
	public void Text_across_a_boundary_through_recursion_is_refused(string publications)
	{
		var result = Compile(publications + "\nA = B | eos\nB = 'x' & A");

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.TextAcrossSegments);
	}

	/// <summary>
	/// A repetition that recovers may step over a boundary, scanning for where it synchronizes or
	/// synchronizing on one, so text kept around it is refused whatever it synchronizes on — in a
	/// grammar that names <c>eos</c> anywhere, a publication of it elsewhere included.
	/// </summary>
	[Theory]
	[InlineData("Start : @string = t: (Word* recover eos) & eof => @(t)\nparse Start")]
	[InlineData("Start : @string = (Word* recover eos) & eof => @(parserText)\nparse Start")]
	[InlineData("Start : @int = (Word* recover eos) & when @(parserText.Length > 0) & eof => @(0)\nparse Start")]
	[InlineData("Start = Word* recover eos\nparse Start")]
	[InlineData("Start : @string = t: (Word* recover ';') & eof => @(t)\nOther : @int = 'b' & eos => @(0)\nparse Start\nparse Other")]
	[InlineData("Start : @string = (Word* recover ';') & eof => @(parserText)\nOther : @int = 'b' & eos => @(0)\nparse Start\nparse Other")]
	[InlineData("Start : @int = $q: (Word* recover ';') & 'x' & $q => @(0)\nOther : @int = 'b' & eos => @(0)\nparse Start\nparse Other")]
	public void Text_around_a_recovery_is_refused(string grammar)
	{
		var result = Compile(grammar + "\nWord = 'a'");

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.TextAcrossSegments);
	}

	/// <summary>
	/// <c>parserText</c> of what holds no boundary may be read: a rule's own text inside one segment,
	/// a guard before the first <c>eos</c>, a rejected element's text up to the end of its segment.
	/// </summary>
	[Fact]
	public void Text_of_one_segment_may_be_read()
	{
		var parser = Build("""
			Rows : @string[] = rows: Row* recover eos => @("bad " + parserText) & eof => @(rows)
			Row  : @string   = w: Word & when @(parserText.Length < 4) & eos => @(w)
			Word : @string   = ['a'..'z']+ => @(parserText)
			parse Rows
			""");

		var (success, value, _, _) = TryParse(parser, "TryParseRows", ["ab", "x1", "abcde", "c"]);

		Assert.True(success);
		Assert.Equal(["ab", "bad x1", "bad abcde", "c"], (string[])value!);
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

	/// <summary>
	/// U+0000 is the end of a segment, so a literal or a class item that is U+0000, which could never
	/// match, is refused.
	/// </summary>
	[Theory]
	[InlineData("Start = \"a\\0\" & eos\nparse Start")]
	[InlineData("Start = '\\0' & eos\nparse Start")]
	[InlineData("Start = ['\\0'] & eos\nparse Start")]
	[InlineData("Start = ['a' | '\\0'] & eos\nparse Start")]
	public void The_joiner_is_no_character_of_a_segment(string grammar)
	{
		Assert.Contains(Compile(grammar).Diagnostics, one => one.Id == GrammarNormalizer.JoinerInSegmentedGrammar);
	}

	/// <summary>
	/// A range or a category that merely holds U+0000 is read without it, since no segment holds it:
	/// it reads the characters it holds, and refuses at a boundary with what a boundary is.
	/// </summary>
	[Theory]
	[InlineData("['\\0'..'\\u001F']")]
	[InlineData("[\\p{Cc}]")]
	[InlineData("[\\p{C}]")]
	public void A_class_that_holds_the_joiner_is_read_without_it(string set)
	{
		var parser = Build($"Start : @int = c: ({set}*) & eos => @(c.Length)\nparse Start");

		Assert.Equal(2, TryParse(parser, "TryParseStart", ["\u0001\u001F"]).Value);

		var (success, _, error, position) = TryParse(parser, "TryParseStart", ["\u0001", "\u0001"]);

		Assert.False(success);
		Assert.Equal(2, position);
		Assert.DoesNotContain("\\u0000", error!, StringComparison.Ordinal);

		var one = Build($"Start : @int = {set} & eos => @(0)\nparse Start");

		(success, _, error, position) = TryParse(one, "TryParseStart", [""]);

		Assert.False(success);
		Assert.Equal(0, position);
		Assert.DoesNotContain("\\u0000", error!, StringComparison.Ordinal);

		(success, _, error, position) = TryParse(one, "TryParseStart", ["\u0001\u0001"]);

		Assert.False(success);
		Assert.Equal(1, position);
		Assert.Equal("Expected end of segment.", error);
	}

	/// <summary>
	/// A class whose item is a class rule of the author's that only looks like the one a grammar
	/// naming <c>eos</c> writes before a predicate is no class: a look and a class are two things.
	/// </summary>
	[Fact]
	public void A_look_of_the_author_is_not_merged_into_a_class()
	{
		var result = Compile("Letter = ?!'a' & ['a'..'z']\nStart : @string = t: [Letter] & eos => @(t)\nparse Start");

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.UnsupportedElement);

		// And the class the generator guards is still one class to merge.
		var parser = Build(
			"Letter = [@Lower]\nStart : @string = t: ([Letter | '-']+) & eos => @(t)\nparse Start",
			members: "static bool Lower(char c) { return c >= 'a' && c <= 'z'; }");

		Assert.Equal("a-b", TryParse(parser, "TryParseStart", ["a-b"]).Value);
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
	/// A predicate of the host in a class is never asked about the end of a segment: a class that
	/// calls one, negated or not and in a look as well, stops at a boundary whatever it would answer.
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

		var asking = Build(
			"""
			Start : @int = ([@Lower]*) & eos & ([^ @Upper]*) & eos & ?=[@Lower] & ([@Lower]*) & eos => @(0)
			parse Start
			""",
			members: "public static string Asked = \"\"; " +
				"static bool Lower(char c) { Asked += c; return true; } " +
				"static bool Upper(char c) { Asked += c; return false; }");

		Assert.True(TryParse(asking, "TryParseStart", ["ab", "cd", "ef"]).Success);

		var asked = (string)asking.GetType("Grammar")!.GetField("Asked")!.GetValue(null)!;

		Assert.Contains('e', asked);
		Assert.DoesNotContain('\0', asked);
	}

	/// <summary>
	/// A recognizer of the host is handed its own segment and nothing after it: one that wants more
	/// than the segment holds refuses there, and what it covers holds no end of a segment.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Tape, true)]
	[InlineData(CarrierKind.Immediate, true)]
	[InlineData(CarrierKind.Tape, false)]
	[InlineData(CarrierKind.Immediate, false)]
	public void A_recognizer_of_the_host_reads_one_segment(CarrierKind carrier, bool direct)
	{
		var pair = Build(
			"Start : @string = t: @Pair & any* & eos => @(t)\nparse Start",
			members: "static bool Pair(System.ReadOnlySpan<char> input, ref int p) { if (input.Length - p < 2) return false; p += 2; return true; }",
			carrier: carrier, direct: direct);

		Assert.Equal("ab", TryParse(pair, "TryParseStart", ["abc"]).Value);

		var (success, _, error, position) = TryParse(pair, "TryParseStart", ["a", "b"]);

		Assert.False(success);
		Assert.Equal(0, position);
		Assert.DoesNotContain("\\u0000", error!, StringComparison.Ordinal);
	}

	/// <summary>
	/// A recognizer that reads to the end of its input stops at the end of its segment: over a span
	/// and over a view, read by methods, and with a value, read by the engine and asked again for it.
	/// </summary>
	[Theory]
	[InlineData("Tape")]
	[InlineData("Immediate")]
	public void A_recognizer_of_the_host_reads_to_the_end_of_its_segment(string carrier)
	{
		var grammars = new[]
		{
			"Items : @string[] = items: Item* & eof => @(items)\\nItem : @string = '!' & v: @Viewed & eos => @(\\\"view \\\" + v) | t: @ToEnd & eos => @(t)\\nparse Items",
			"Items : @string[] = items: Item* & eof => @(items)\\nItem : @string = '#' & n: @Rest & eos => @(\\\"rest \\\" + n) | t: @ToEnd & eos => @(t)\\nparse Items",
		};

		foreach (var grammar in grammars)
		{
			// A recognizer with a value is read by the engine whatever was asked (GRAM5007).
			var valued = grammar.Contains("@Rest", StringComparison.Ordinal);
			var parser = GeneratorDriverTests.Build($$"""
				using System;
				[DotGram.Gram("{{grammar}}", Carrier = DotGram.GramCarrier.{{carrier}})]
				public partial class Grammar
				{
					static bool ToEnd(ReadOnlySpan<char> input, ref int p) { p = input.Length; return true; }
					{{(valued
						? "static bool Rest(ReadOnlySpan<char> input, ref int p, out int value) { value = input.Length - p; p = input.Length; return true; }"
						: "static bool Viewed(ParserInput<char> input, ref int p) { while (input.Peek(p, out _)) p++; return true; }")}}
				}
				""", permittedWarning: "GRAM5007");
			var marked = valued ? "#" : "!";
			var (read, value, _, _) = TryParse(parser, "TryParseItems", ["ab", "", marked + "xyz", "e"]);

			Assert.True(read);
			Assert.Equal(["ab", "", marked == "#" ? "rest 3" : "view xyz", "e"], (string[])value!);
		}

		// Nor anything before it: the segment it is handed starts where its own does, and so does the
		// position it is handed, which is put back where it belongs when it returns. Read by the
		// engine and asked again for the value, over a span and over a view.
		var whole = GeneratorDriverTests.Build($$"""
			using System;
			[DotGram.Gram("Start : @string = 'a' & eos & v: @Rest & eos & w: @Rest & eos & x: @Seen & eos => @(v + \"|\" + w + \"|\" + x)\nparse Start", Carrier = DotGram.GramCarrier.{{carrier}})]
			public partial class Grammar
			{
				static bool Rest(ReadOnlySpan<char> input, ref int p, out string value) { value = input.ToString() + "@" + p; p = input.Length; return true; }
				static bool Seen(ParserInput<char> input, ref int p, out string value)
				{
					value = "";
					for (var at = 0; input.Peek(at, out var c); at++) value += c;
					value += "@" + p;
					p++;
					return input.Peek(p - 1, out _);
				}
			}
			""", permittedWarning: "GRAM5007");

		Assert.Equal("bc@0|@0|d@0", TryParse(whole, "TryParseStart", ["a", "bc", "", "d"]).Value);
		Assert.False(TryParse(whole, "TryParseStart", ["a", "bc", "", ""]).Success);
	}

	/// <summary>
	/// The end of a segment never reaches a predicate of the host, however the class that calls it
	/// is written and read: beside ranges, negated, in a look, through a class rule, in a search for
	/// a delimiter, under a recovery, matching or refusing — by methods and by the engine.
	/// </summary>
	[Theory]
	[InlineData("Start : @int = ([@Ask | '0'..'9']*) & eos & ([@Ask]*) & eos => @(0)")]
	[InlineData("Start : @int = ([^ @Ask]*) & eos & ([^ @Ask | ';']*) & eos => @(0)")]
	[InlineData("Start : @int = (?=[@Ask] & any)* & eos & (?![@Ask] & any)* & eos & any* & eos => @(0)")]
	[InlineData("Letter = [@Ask]\nStart : @int = ([Letter | '-']*) & eos & ([^ Letter]*) & eos => @(0)")]
	[InlineData("Start : @int = (?!';' & [@Ask])* & eos & (?!\"*/\" & [@Ask])* & eos => @(0)")]
	[InlineData("Start : @int[] = items: Item* recover eos & eof => @(items)\nItem : @int = [@Ask]+ & ';' & eos => @(0)")]
	[InlineData("Start : @int = [@Ask]+ & '!' & eos => @(0)")]
	public void The_end_of_a_segment_never_reaches_a_predicate(string grammar)
	{
		foreach (var (carrier, direct) in new[]
			{ (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, false) })
		{
			var parser = Build(
				grammar + "\nparse Start",
				members: "public static string Asked = \"\"; static bool Ask(char c) { Asked += c; return c != 'z'; }",
				carrier: carrier, direct: direct);
			var asked = parser.GetType("Grammar")!.GetField("Asked")!;

			foreach (var segments in new[] { new[] { "ab", "cd", "ef" }, ["a;", "b;"], ["z", "", "q"], [""] })
				TryParse(parser, "TryParseStart", segments);

			var said = (string)asked.GetValue(null)!;

			Assert.NotEmpty(said);
			Assert.DoesNotContain('\0', said);
		}
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
			Feed   : @string[] = records: Record* recover eos => @("bad " + parserLine + ":" + parserFailureColumn + ":" + parserText) & eof => @(records)
			Record : @string   = n: Number & '|' & t: Text & eos => @(n + "=" + t)
			Number = ['0'..'9']+
			Text   = [^ '|']+
			parse Feed
			""", carrier: carrier, direct: direct);

		var (success, value, _, _) = TryParse(parser, "TryParseFeed", ["1|a", "x|b", "2|c\nd", "3|", "4|e"]);

		Assert.True(success);
		// What a rejected record is told is its own segment, with no end of it in its text.
		Assert.Equal(["1=a", "bad 2:1:x|b", "2=c\nd", "bad 4:3:3|", "4=e"], (string[])value!);
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

	/// <summary>
	/// A reader is a sequence of lines, split as <c>File.ReadLines</c> splits a file: every line
	/// ending is the end of a segment, a last line without one is a segment all the same, and a last
	/// line ending makes no empty segment after it. The same answers as the lines as a list, read in
	/// blocks of any size, a <c>\r\n</c> split between two of them included.
	/// </summary>
	[Theory]
	[InlineData("")]
	[InlineData("\n")]
	[InlineData("a")]
	[InlineData("a\n")]
	[InlineData("a\n\n")]
	[InlineData("-o\r\nfile\r\nx y")]
	[InlineData("a\rb\r\nc\nd")]
	[InlineData("a\r\r\nb")]
	[InlineData("\r\n\r\n")]
	[InlineData("--output\r\n")]
	public void A_reader_is_read_as_its_lines(string text)
	{
		var parser = Build(Argv);
		var lines  = new List<string>();
		var split  = new StringReader(text);

		for (var line = split.ReadLine(); line is not null; line = split.ReadLine())
			lines.Add(line);

		var expected = string.Join(",", (string[])Invoke(parser, "ParseArgs", typeof(IReadOnlyList<string>), lines.ToArray())!);

		foreach (var block in new[] { 1, 2, 3, 4096 })
		{
			var read = (string[])Invoke(parser, "ParseArgs", typeof(TextReader), new BlockReader(text, block))!;

			Assert.Equal(expected, string.Join(",", read));

			var match = parser.GetType("Grammar")!.GetMethod("TryParseArgs", [typeof(TextReader)])!.Invoke(null, [new BlockReader(text, block)])!;

			Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
		}
	}

	/// <summary>
	/// A line of a reader that holds U+0000 stops the reading with an error that names the line: the
	/// forms that answer refuse at it, the one that throws throws, and a streamed reading throws when
	/// it reaches it.
	/// </summary>
	[Fact]
	public void A_line_that_holds_the_joiner_is_refused_by_its_number()
	{
		var parser = Build(Argv);
		var match  = parser.GetType("Grammar")!.GetMethod("TryParseArgs", [typeof(TextReader)])!.Invoke(null, [new StringReader("a\r\nb\0c\nd")])!;

		Assert.False((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
		Assert.Equal(3L, match.GetType().GetProperty("Position")!.GetValue(match));
		Assert.StartsWith("Line 2 holds U+0000", (string)match.GetType().GetProperty("Error")!.GetValue(match)!, StringComparison.Ordinal);

		var thrown = Assert.Throws<TargetInvocationException>(() => Invoke(parser, "ParseArgs", typeof(TextReader), new StringReader("a\nb\nc\0")));

		Assert.StartsWith("Line 3 holds U+0000", Assert.IsType<FormatException>(thrown.InnerException).Message, StringComparison.Ordinal);

		var records = Build("""
			Feed   : @string[] = records: Record* recover eos => @("bad " + parserLine) & eof => @(records)
			Record : @string   = n: Number & '|' & t: Text & eos => @(n + "=" + t)
			Number = ['0'..'9']+
			Text   = [^ '|']+
			parse Feed
			""");

		var streamed = (IEnumerable<string>)Invoke(records, "ParseFeed", typeof(TextReader), new BlockReader("1|a\r\nx|b\n2|c", 2))!;

		Assert.Equal(["1=a", "bad 2", "2=c"], streamed.ToArray());

		var broken = Assert.ThrowsAny<Exception>(() => ((IEnumerable<string>)Invoke(records, "ParseFeed", typeof(TextReader), new StringReader("1|a\n2|\0"))!).ToArray());

		Assert.StartsWith("Line 2 holds U+0000", (broken.InnerException ?? broken).Message, StringComparison.Ordinal);
	}

	/// <summary>A reader that hands its text over a few characters at a time.</summary>
	sealed class BlockReader(string text, int block) : TextReader
	{
		int _at;

		public override int Read(char[] buffer, int index, int count)
		{
			var length = Math.Min(Math.Min(block, count), text.Length - _at);

			text.CopyTo(_at, buffer, index, length);
			_at += length;

			return length;
		}
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
