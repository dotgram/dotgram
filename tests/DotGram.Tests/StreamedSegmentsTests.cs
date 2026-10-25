using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A grammar that names <c>eos</c> streams its segments where its records fit one (docs/syntax.md
/// §6.3): the retention analysis measures it in segments, so a class that admits a line ending is
/// one character of a segment, and only <c>eos</c> ends a record.
/// </summary>
/// <remarks>
/// What is held to be lazy is held by how far the source was pulled, never by the type of what
/// comes back alone: a reader is read in blocks of several kilobytes, so a short input proves
/// nothing, and a sequence read whole into an array casts to the sequence all the same.
/// </remarks>
public sealed class StreamedSegmentsTests
{
	/// <summary>A feed in three parts: a header, records that recover at the end of their segment, a trailer.</summary>
	const string Feed = """
		Feed    : @string[] = Header & Row* recover eos => @("bad " + parserLine + ":" + parserFailureColumn + ":" + parserText) & Trailer & eof
		Header  : @string   = "H|" & t: Text & eos => @("head " + t)
		Row     : @string   = n: Number & '|' & t: Text & eos => @(n + "=" + t)
		Trailer : @string   = "T|" & n: Number & eos => @("tail " + n)
		Number  = ['0'..'9']+
		Text    = [^ '|']+
		parse Feed
		""";

	/// <summary>Records and nothing else, recovering at the end of their segment.</summary>
	const string Rows = """
		Rows   : @string[] = Row* recover eos => @("bad " + parserLine + ":" + parserFailureColumn)
		Row    : @string   = n: Number & '|' & t: Text & eos => @(n + "=" + t)
		Number = ['0'..'9']+
		Text   = [^ '|']+
		parse Rows
		""";

	/// <summary>
	/// A three-part feed is read as it is asked for, from a sequence and from a reader alike: the
	/// first element is handed over having pulled what one record needs, however long the input,
	/// and what comes back is what the list reads.
	/// </summary>
	[Fact]
	public void A_feed_in_three_parts_streams_from_a_sequence_and_from_a_reader()
	{
		var parser = Build(Feed);

		AssertStreams(parser, "ParseFeed", Lines);

		foreach (var segments in new[]
		{
			new[] { "H|day", "1|a", "x|b", "2|c\nd", "3|", "T|3" },
			["H|day", "T|0"],
			["H|day", "1|a"],
			["H|day", "1|a", "T|1", "2|b"],
			["X|day", "1|a", "T|1"],
		})
			AssertSameAsList(parser, "ParseFeed", segments);

		Assert.Equal(
			["head day", "1=a", "bad 3:1:x|b", "2=c\nd", "bad 5:3:3|", "tail 3"],
			Streamed(parser, "ParseFeed", ["H|day", "1|a", "x|b", "2|c\nd", "3|", "T|3"]));

		static IEnumerable<string> Lines(int records)
		{
			yield return "H|day";

			for (var i = 0; i < records; i++)
				yield return i % 7 == 3 ? "x|bad" : i + "|record " + i;

			yield return "T|" + records;
		}
	}

	/// <summary>A bare repetition of records streams the same way.</summary>
	[Fact]
	public void A_bare_repetition_of_records_streams()
	{
		var parser = Build(Rows);

		AssertStreams(parser, "ParseRows", Lines);

		foreach (var segments in new[]
		{
			new[] { "1|a", "x|b", "2|c", "3|" },
			[],
			["x"],
			["1|a", ""],
		})
			AssertSameAsList(parser, "ParseRows", segments);

		static IEnumerable<string> Lines(int records)
		{
			for (var i = 0; i < records; i++)
				yield return i % 5 == 2 ? "|" : i + "|record " + i;
		}
	}

	/// <summary>
	/// A class that admits a line ending reads it as a character of the segment: over segments it
	/// ends no record, so the rule still streams, and the line ending is in the value.
	/// </summary>
	[Fact]
	public void A_line_ending_inside_a_segment_is_a_character_of_it()
	{
		var parser = Build(Rows);

		Assert.Equal(["1=a\nb", "2=c\r\n"], Streamed(parser, "ParseRows", ["1|a\nb", "2|c\r\n"]));
		AssertSameAsList(parser, "ParseRows", ["1|a\nb", "x\n|c", "2|c\r\n"]);
	}

	/// <summary>
	/// <c>parserLine</c> counts segments, not the line endings inside one, on the streamed reading as
	/// on the list.
	/// </summary>
	[Fact]
	public void A_line_is_a_segment_when_a_segment_holds_a_line_ending()
	{
		var parser = Build(Rows);
		var input  = new[] { "1|a\nb\nc", "x\n|y", "2|c" };

		Assert.Equal(["1=a\nb\nc", "bad 2:1", "2=c"], Streamed(parser, "ParseRows", input));
		AssertSameAsList(parser, "ParseRows", input);
	}

	/// <summary>
	/// A repetition that recovers at a delimiter other than <c>eos</c> may step over a bad element
	/// into the segments after it: the streamed reading steps as far as the list does, and hands
	/// back what it does.
	/// </summary>
	[Fact]
	public void A_recovery_whose_skip_crosses_segments_reads_as_the_list_does()
	{
		var parser = Build("""
			Items : @string[] = Item* recover ';' => @("bad " + parserLine) & eof
			Item  : @string   = t: ['a'..'z']+ & eos => @(t)
			parse Items
			""");

		foreach (var segments in new[]
		{
			new[] { "ab", "x1", "zz", ";", "cd" },
			["ab", "x1", "zz;", "cd"],
			["x1", "zz"],
			["1;", "ab"],
			["ab", "1", ";", ";", "cd"],
			["ab", "1", "2;cd", "ef"],
		})
			AssertSameAsList(parser, "ParseItems", segments);

		// The skip from the bad element on the second segment runs to the delimiter on the fourth;
		// what follows the delimiter is the end of that segment, no element, and its skip finds no
		// delimiter before the end of the input.
		Assert.Equal(["ab", "bad 2", "bad 4"], Streamed(parser, "ParseItems", ["ab", "x1", "zz", ";", "cd"]));
	}

	/// <summary>
	/// A record that reads through a predicate of the host is not proved to stay inside its segment:
	/// the class is kept off the joiner when it is read, but what the predicate admits is not the
	/// grammar's to know, and the analysis counts it as admitting the end of a segment. Told, and the
	/// forms over a sequence and over a reader read whole.
	/// </summary>
	[Fact]
	public void A_record_read_through_a_predicate_is_not_streamed()
	{
		var result = Compile("""
			Rows : @string[] = Row* recover eos
			Row  : @string   = t: ([@Letter]+) & eos => @(t)
			parse Rows
			""");

		var said = Assert.Single(result.Diagnostics, static one => one.Id == Retention.NotStreamable);

		Assert.Equal(GramSeverity.Info, said.Severity);
		Assert.Contains("reads a sequence of segments whole", said.Message, StringComparison.Ordinal);
		Assert.Contains("may take more than one segment", said.Message, StringComparison.Ordinal);
		Assert.DoesNotContain("SegmentReader_DotGram(input, true))", Assert.Single(result.Sources).Text, StringComparison.Ordinal);
	}

	/// <summary>
	/// What does not stream over segments is said in segments: a record that reads past the end of
	/// its own segment takes more than one.
	/// </summary>
	[Fact]
	public void What_takes_more_than_one_segment_is_said_so()
	{
		var result = Compile("""
			Pairs : @string[] = Pair* recover eos
			Pair  : @string   = k: Key & eos & v: Key & eos => @(k + "=" + v)
			Key   = ['a'..'z']+
			parse Pairs
			""");

		var said = Assert.Single(result.Diagnostics, static one => one.Id == Retention.NotStreamable);

		Assert.Contains("'item0: Pair*' may take more than one segment", said.Message, StringComparison.Ordinal);
		Assert.DoesNotContain("line", said.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A <c>yield</c> over segments has forms over a sequence and over a reader that read the input
	/// whole and then hand the elements over: its author is told so (information), and so is its
	/// caller, by the documentation of each form, which does not promise a lazy reading.
	/// </summary>
	[Fact]
	public void A_yield_over_segments_says_it_reads_the_input_whole()
	{
		var result = Compile("""
			Items : @int[] = Item*
			Item  : @int   = w: Word & eos => @(w.Length)
			Word  = ['a'..'z']+
			parse Items as Lengths yield : @int
			""");

		var said = Assert.Single(result.Diagnostics, static one => one.Id == Retention.NotStreamable);

		Assert.Equal(GramSeverity.Info, said.Severity);
		Assert.StartsWith("'Lengths' reads its segments whole, then hands the elements over one at a time", said.Message, StringComparison.Ordinal);
		Assert.Contains("A plain 'parse' of the sequence rule, without 'yield', is the reading that takes the segments as they are asked for", said.Message, StringComparison.Ordinal);

		var source = Assert.Single(result.Sources).Text;

		Assert.DoesNotContain("Lazily parses", source, StringComparison.Ordinal);
		Assert.Equal(3, Count(source, "/// Reads the whole input, then hands its elements over one at a time;"));

		// And it is so: walking the reader form has read the whole reader before the first element.
		var parser  = EmittedCode.Compile(source);
		var text    = string.Concat(Enumerable.Repeat("abc\n", 5_000));
		var counted = new CountingReader(text);

		using var walked = ((IEnumerable<int>)Invoke(parser, "Lengths", typeof(TextReader), counted)!).GetEnumerator();

		Assert.True(walked.MoveNext());
		Assert.Equal(text.Length, counted.Pulled);
	}

	/// <summary>
	/// The publication streams: what comes back is no array, nothing is pulled until it is walked, and
	/// two elements are handed over having pulled the same part of the source at two sizes of it — a
	/// block of the reading's own, not a share of the input.
	/// </summary>
	static void AssertStreams(Assembly parser, string method, Func<int, IEnumerable<string>> lines)
	{
		var pulled = new List<(int Segments, int Chars)>();

		foreach (var records in new[] { 5_000, 50_000 })
		{
			var counted = new CountingSequence(lines(records));
			var read    = (IEnumerable<string>)Invoke(parser, method, typeof(IEnumerable<string>), counted)!;

			Assert.IsNotType<string[]>(read);
			Assert.Equal(0, counted.Pulled);

			using (var walked = read.GetEnumerator())
			{
				Assert.True(walked.MoveNext());
				Assert.True(walked.MoveNext());

				var segments = counted.Pulled;
				var text     = string.Concat(lines(records).Select(static line => line + "\n"));
				var reader   = new CountingReader(text);

				using var overReader = ((IEnumerable<string>)Invoke(parser, method, typeof(TextReader), reader)!).GetEnumerator();

				Assert.True(overReader.MoveNext());
				Assert.True(overReader.MoveNext());
				Assert.True(reader.Pulled < text.Length / 4, $"{reader.Pulled} of {text.Length} characters pulled for two elements");

				pulled.Add((segments, reader.Pulled));
			}
		}

		Assert.Equal(pulled[0], pulled[1]);
	}

	/// <summary>
	/// The streamed readings, over a sequence and over a reader where the segments hold no line
	/// ending, give the list's values. Where the list is refused they hand over what they read
	/// before the refusal and then throw, as a stream does: the elements before it are no part of
	/// the comparison, the throw is.
	/// </summary>
	static void AssertSameAsList(Assembly parser, string method, string[] segments)
	{
		var listed   = Outcome(() => (IEnumerable<string>)Invoke(parser, method, typeof(IReadOnlyList<string>), segments)!);
		var streamed = Outcome(() => (IEnumerable<string>)Invoke(parser, method, typeof(IEnumerable<string>), new CountingSequence(segments))!);

		Assert.Equal(Refused(listed), Refused(streamed));

		// Refused at the same place, said in words of their own: a whole parse also offers what
		// could follow the repetition, the streamed reading asks the part it is in.
		if (Refused(listed))
			Assert.Equal(Where(listed), Where(streamed));
		else
			Assert.Equal(listed, streamed);

		if (segments.Any(static one => one.Contains('\n') || one.Contains('\r')))
			return;

		var read = Outcome(() => (IEnumerable<string>)Invoke(parser, method, typeof(TextReader), new StringReader(string.Concat(segments.Select(static one => one + "\n"))))!);

		Assert.Equal(streamed, read);

		static bool Refused(string outcome)
		{
			return outcome.Contains("FormatException: ", StringComparison.Ordinal);
		}

		static string Where(string outcome)
		{
			return outcome.Substring(outcome.LastIndexOf(" at line ", StringComparison.Ordinal));
		}
	}

	static string Outcome(Func<IEnumerable<string>> read)
	{
		var said = new StringBuilder();

		try
		{
			foreach (var one in read())
				said.Append(one).Append(" | ");
		}
		catch (Exception thrown)
		{
			var inner = thrown is TargetInvocationException { InnerException: { } cause } ? cause : thrown;

			said.Append(inner.GetType().Name).Append(": ").Append(inner.Message);
		}

		return said.ToString();
	}

	static string[] Streamed(Assembly parser, string method, string[] segments)
	{
		return ((IEnumerable<string>)Invoke(parser, method, typeof(IEnumerable<string>), new CountingSequence(segments))!).ToArray();
	}

	static int Count(string text, string part)
	{
		var count = 0;

		for (var at = text.IndexOf(part, StringComparison.Ordinal); at >= 0; at = text.IndexOf(part, at + part.Length, StringComparison.Ordinal))
			count++;

		return count;
	}

	/// <summary>A sequence that says how many of its items have been asked for.</summary>
	sealed class CountingSequence(IEnumerable<string> items) : IEnumerable<string>
	{
		public int Pulled { get; private set; }

		public IEnumerator<string> GetEnumerator()
		{
			foreach (var item in items)
			{
				Pulled++;

				yield return item;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>A reader that says how many characters have been taken from it.</summary>
	sealed class CountingReader(string text) : TextReader
	{
		public int Pulled { get; private set; }

		public override int Read(char[] buffer, int index, int count)
		{
			var length = Math.Min(count, text.Length - Pulled);

			text.CopyTo(Pulled, buffer, index, length);
			Pulled += length;

			return length;
		}

		public override int Read()
		{
			return Pulled < text.Length ? text[Pulled++] : -1;
		}

		public override int Peek()
		{
			return Pulled < text.Length ? text[Pulled] : -1;
		}
	}

	static GramCompilation Compile(string grammar)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance,
		});
	}

	/// <summary>The grammar compiled, holding that it streams: its reader form reads through the lines adapter as it goes.</summary>
	static Assembly Build(string grammar)
	{
		var result = Compile(grammar);

		EmittedCode.Quiet(result.Diagnostics);
		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == Retention.NotStreamable);

		var source = Assert.Single(result.Sources).Text;

		Assert.Contains("(new SegmentReader_DotGram(input, true))", source, StringComparison.Ordinal);

		return EmittedCode.Compile(source);
	}

	static object? Invoke(Assembly assembly, string method, Type over, object input)
	{
		return assembly.GetType("Grammar")!.GetMethod(method, [over])!.Invoke(null, [input]);
	}
}
