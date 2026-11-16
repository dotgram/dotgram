using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A feed read from a <see cref="TextReader"/> through the overloads that take no buffer
/// parameters — the window a plain <c>parse</c> or <c>find</c> of a reader reads through, and the
/// lines a grammar that names <c>eos</c> reads: what the caller sees when the reader throws half-way,
/// and when the caller stops walking after the first record.
/// </summary>
/// <remarks>
/// <para>
/// The buffered form (<c>stream</c>) is asked the same in <c>BufferedPoolTests</c>, and a sequence
/// of lines in <c>LinesDisposeTests</c>; this is the reader overload neither reaches, which is the
/// one docs/feeds.md reads its files with.
/// </para>
/// <para>
/// What it asserts is today's behaviour, and each part of it is one a caller relies on: the
/// records read before the reader failed are handed over; the reader's own exception reaches the
/// caller as it was thrown, not wrapped and not turned into a refusal; nothing is read after the
/// failure or after the caller has stopped; nothing is read before the caller starts walking a
/// lazy result; and the reader is never disposed — it is the caller's (docs/syntax.md §6.3 says so
/// of the buffered form, and the reader overload does the same).
/// </para>
/// </remarks>
public sealed class ReaderFailureTests
{
	/// <summary>Records separated by a character the window reads, so a record is a part of the window.</summary>
	const string Windowed = """
		Row  : @int   = 'R' & d: ['0'..'9'] & '\n' => @(d[0] - '0')
		Rows : @int[] = Row* recover '\n'           => @(-1)
		parse Rows
		find Row
		""";

	/// <summary>The same records as segments, a line apiece, which is how docs/feeds.md reads a file.</summary>
	const string Segmented = """
		Row  : @int   = 'R' & d: ['0'..'9'] & eos => @(d[0] - '0')
		Rows : @int[] = Row* recover eos           => @(-1)
		parse Rows
		find Row
		parse Rows as Yielded yield : @int
		""";

	static readonly Lazy<Type> WindowedHost  = new(() => Compile(Windowed));
	static readonly Lazy<Type> SegmentedHost = new(() => Compile(Segmented));

	/// <summary>How many records the feed holds: many more than any reading of it asks for at once.</summary>
	const int Records = 2_000;

	/// <summary>How much the reader hands over a read: less than a record and a half, so a record spans reads.</summary>
	const int Chunk = 4;

	/// <summary>Where the reader throws: after two whole records and part of a third.</summary>
	const int FailAt = 7;

	static readonly string Feed = string.Concat(Enumerable.Range(0, Records).Select(static one => "R" + one % 10 + "\n"));

	/// <summary>
	/// The reader throws in the middle of the third record: the two before it are handed over, then
	/// the reader's own exception, and the reader is neither read again nor disposed.
	/// </summary>
	[Theory]
	[InlineData("windowed", "ParseRows")]
	[InlineData("windowed", "FindRow")]
	[InlineData("segmented", "ParseRows")]
	[InlineData("segmented", "FindRow")]
	public void A_reader_that_throws_half_way_hands_over_what_was_read_then_its_own_exception(string grammar, string method)
	{
		var reader = new Spy(Feed, FailAt);
		var walked = (IEnumerable)Call(Host(grammar), method, reader);

		Assert.Equal(0, reader.Reads);

		var values = new List<int>();
		var thrown = Assert.Throws<IOException>(() =>
		{
			foreach (var value in walked)
				values.Add(Value(value));
		});

		Assert.Same(reader.Thrown, thrown);
		Assert.Equal([0, 1], values);

		// Two reads that handed over the seven characters, and the one that threw: none after it.
		Assert.Equal(3, reader.Reads);
		Assert.Equal(FailAt, reader.Handed);
		Assert.False(reader.Disposed);
	}

	/// <summary>
	/// A <c>yield</c> over segments reads them whole before it hands anything over (§6.3), so the
	/// reader's exception comes out of the call itself, and nothing is handed over.
	/// </summary>
	[Fact]
	public void A_yield_over_segments_throws_the_readers_exception_from_the_call()
	{
		var reader = new Spy(Feed, FailAt);
		var thrown = Assert.Throws<IOException>(() => Call(SegmentedHost.Value, "Yielded", reader));

		Assert.Same(reader.Thrown, thrown);
		Assert.Equal(3, reader.Reads);
		Assert.Equal(FailAt, reader.Handed);
		Assert.False(reader.Disposed);
	}

	/// <summary>
	/// The caller stops after the first record: what was read for it is all that is read, the
	/// reader stays open, and walking the same result again does not read on behind the caller's back.
	/// </summary>
	/// <remarks>
	/// One read of <see cref="Chunk"/> characters is what the first record costs, and that is the
	/// figure asserted: a reading that filled a buffer ahead of the caller, or went on to the end of
	/// the feed once the enumerator was dropped, would take more.
	/// </remarks>
	[Theory]
	[InlineData("windowed", "ParseRows")]
	[InlineData("windowed", "FindRow")]
	[InlineData("segmented", "ParseRows")]
	[InlineData("segmented", "FindRow")]
	public void Stopping_after_the_first_record_reads_no_further_and_leaves_the_reader_open(string grammar, string method)
	{
		var reader = new Spy(Feed, int.MaxValue);
		var walked = (IEnumerable)Call(Host(grammar), method, reader);

		Assert.Equal(0, reader.Reads);

		foreach (var value in walked)
		{
			Assert.Equal(0, Value(value));

			break;
		}

		Assert.Equal(1,     reader.Reads);
		Assert.Equal(Chunk, reader.Handed);
		Assert.False(reader.Disposed);

		// The reader is the caller's, and still where the parse left it: past the first record
		// and the 'R' of the second, which the parse read ahead and does not give back (§6.3).
		Assert.Equal('1', (char)reader.Read());
	}

	/// <summary>
	/// A <c>yield</c> over segments has already read the whole feed when the caller stops: what is
	/// left is handed over from what it holds, and the reader is left open at its end.
	/// </summary>
	[Fact]
	public void A_yield_over_segments_has_read_everything_before_the_caller_stops()
	{
		var reader = new Spy(Feed, int.MaxValue);
		var walked = (IEnumerable)Call(SegmentedHost.Value, "Yielded", reader);

		Assert.Equal(Feed.Length, reader.Handed);

		var reads = reader.Reads;

		foreach (var value in walked)
		{
			Assert.Equal(0, Value(value));

			break;
		}

		Assert.Equal(reads, reader.Reads);
		Assert.False(reader.Disposed);
	}

	static Type Host(string grammar)
	{
		return grammar == "windowed" ? WindowedHost.Value : SegmentedHost.Value;
	}

	static int Value(object? value)
	{
		return value is int number ? number : (int)value!.GetType().GetProperty("Value")!.GetValue(value)!;
	}

	static object Call(Type host, string method, TextReader reader)
	{
		try
		{
			return host.GetMethod(method, [typeof(TextReader)])!.Invoke(null, [reader])!;
		}
		catch (TargetInvocationException e)
		{
			throw e.InnerException!;
		}
	}

	static Type Compile(string grammar)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text).GetType("Grammar")!;
	}

	/// <summary>
	/// A reader that hands over at most <see cref="Chunk"/> characters a read, throws once it has
	/// handed over <c>failAt</c> of them, and remembers every read and whether it was disposed.
	/// </summary>
	sealed class Spy(string text, int failAt) : TextReader
	{
		public int Reads { get; private set; }

		public int Handed { get; private set; }

		public bool Disposed { get; private set; }

		public IOException Thrown { get; } = new("The disk went away.");

		public override int Read(char[] buffer, int index, int count)
		{
			return Read(buffer.AsSpan(index, count));
		}

		public override int Read(Span<char> buffer)
		{
			Reads++;

			if (Handed >= failAt)
				throw Thrown;

			var take = Math.Min(Math.Min(buffer.Length, Chunk), Math.Min(text.Length, failAt) - Handed);

			text.AsSpan(Handed, take).CopyTo(buffer);

			Handed += take;

			return take;
		}

		public override int Read()
		{
			Span<char> one = stackalloc char[1];

			return Read(one) == 0 ? -1 : one[0];
		}

		public override string? ReadLine()
		{
			throw new NotSupportedException("A parse reads blocks, not lines.");
		}

		public override int Peek()
		{
			throw new NotSupportedException("A parse does not peek.");
		}

		protected override void Dispose(bool disposing)
		{
			Disposed = true;

			base.Dispose(disposing);
		}
	}
}
