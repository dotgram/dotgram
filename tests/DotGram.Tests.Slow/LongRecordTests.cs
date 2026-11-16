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
/// One record of a feed megabytes long: a corrupted line a repetition marked <c>recover</c> has to
/// step over, and a line refused at its very end after a prefix two alternatives share.
/// </summary>
/// <remarks>
/// <para>
/// What a well-formed feed costs is held elsewhere (<c>StreamingRetentionTests</c>, records a
/// thousand characters long). This is the record nobody planned for: a line of several megabytes
/// with no terminator until its end, which a file of records meets the day a writer crashes
/// half-way or two files are concatenated without a line break. Each test asks the three things
/// that can go wrong with it: the answer (the records after it are still read, or the refusal is
/// told where it happened), the memory (what is held grows with that one line and with nothing
/// else), and the time (four times the line costs about four times as long).
/// </para>
/// <para>
/// The line is made as it is asked for and never exists as a string, so what is live while it is
/// read is the parse's and not the source's. Memory is read as the whole process's live heap at
/// every read the parse asks of its source, and two sizes are compared rather than one size held
/// to a figure, so what the rest of the process holds cancels out.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class LongRecordTests
{
	/// <summary>
	/// Short records, one broken one of any length the source is asked for, and three forms over a
	/// reader: the window a plain <c>parse</c> of a reader reads through, and the buffered form
	/// (<c>stream</c>), lazily and whole.
	/// </summary>
	const string Feed = """
		Row  : @int   = 'R' & d: ['0'..'9'] & '\n' => @(d[0] - '0')
		Rows : @int[] = Row* recover '\n'           => @(-1)
		parse Rows
		parse Rows as Streamed stream yield : @int
		parse Rows as Whole stream
		""";

	/// <summary>Records before the broken one, and after it.</summary>
	const int Before = 2;

	const int After = 2_000;

	static readonly Lazy<Type> FeedHost = new(() => Compile(Feed));

	// ── A corrupted line ────────────────────────────────────────────────────────────

	/// <summary>
	/// A corrupted line longer than any buffer is stepped over, and every record after it is read.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The window a reader is read through holds the element it is in the middle of, and the
	/// recovery's skip is part of that element, so the line is held whole while it is skipped: the
	/// window grows to it, and nothing bounds that growth (docs/syntax.md §6.3 — the reader form
	/// takes no <c>maxRetained</c>). What is asserted is that it is the LINE that is held and not
	/// more: the growth from a line of <see cref="ShortLine"/> characters to one of
	/// <see cref="LongLine"/> is at most <see cref="Window"/> bytes a character: the window, grown by
	/// doubling, and the buffer it grew from. A parse that kept a copy of the skipped text beside
	/// the window fails it.
	/// </para>
	/// <para>
	/// The buffered form with no limit (<c>maxRetained</c> left at <c>int.MaxValue</c>) holds the same
	/// way and is asserted the same way, lazily and whole.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("reader")]
	[InlineData("stream yield")]
	[InlineData("stream whole")]
	public void A_corrupted_line_megabytes_long_is_stepped_over_and_every_record_after_it_read(string form)
	{
		var shorter = Read(FeedHost.Value, form, ShortLine, retained: null);
		var longer  = Read(FeedHost.Value, form, LongLine,  retained: null);

		foreach (var run in new[] { shorter, longer })
		{
			Assert.Null(run.Failure);
			Assert.Equal(Expected(), run.Values);
		}

		var perCharacter = (longer.Peak - shorter.Peak) / (double)(LongLine - ShortLine);

		Assert.True(
			perCharacter <= Window,
			$"{form}: the peak grew by {perCharacter:F2} bytes a character of the broken line, from {shorter.Peak:N0} " +
			$"at {ShortLine:N0} characters to {longer.Peak:N0} at {LongLine:N0}, against a bound of {Window}. " +
			"The window holds the line being skipped; more than that is the parse holding the text a second time.");
	}

	/// <summary>
	/// The buffered form under a window smaller than the corrupted line: it says the window was too
	/// small, and holds no more than the window while it finds that out.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is today's behaviour and not a promise that the line can be skipped. The recovery's skip
	/// over a bad element is held like any other part of the element, so a line longer than
	/// <c>maxRetained</c> is not stepped over: the parse throws the <c>IOException</c> §6.3 names for
	/// a retention limit, with the limit in its message, and the records after the line are not read.
	/// What it must not do is hold more than the limit on the way: the peak at a line four times as
	/// long is the same, within <see cref="Slack"/>, because neither run holds more than the window.
	/// </para>
	/// <para>
	/// A line that fits the window is stepped over and the records after it are read, under the
	/// same limit: the bound is on what is held, not on how much the feed may contain.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("stream yield")]
	[InlineData("stream whole")]
	public void Under_a_window_shorter_than_the_line_the_parse_says_so_and_holds_no_more_than_the_window(string form)
	{
		var shorter = Read(FeedHost.Value, form, ShortLine, Retained);
		var longer  = Read(FeedHost.Value, form, LongLine,  Retained);

		foreach (var run in new[] { shorter, longer })
		{
			var failure = Assert.IsType<IOException>(run.Failure);

			Assert.Contains(Retained.ToString(), failure.Message, StringComparison.Ordinal);
			Assert.Contains("maxRetained",        failure.Message, StringComparison.Ordinal);
		}

		Assert.True(
			longer.Peak <= shorter.Peak + Slack,
			$"{form}: under a window of {Retained:N0} characters the peak was {shorter.Peak:N0} bytes with a broken " +
			$"line of {ShortLine:N0} characters and {longer.Peak:N0} with one of {LongLine:N0}. A bounded window may " +
			"not grow with the line it refuses to hold.");

		var fitting = Read(FeedHost.Value, form, Retained / 2, Retained);

		Assert.Null(fitting.Failure);
		Assert.Equal(Expected(), fitting.Values);
	}

	/// <summary>
	/// Four times the corrupted line costs about four times as long to step over, in every form.
	/// </summary>
	/// <remarks>
	/// A blow-up ratchet, like the other scaling tests here: an exponent of
	/// <see cref="Linear"/> over a factor of four, with the reading repeated before it is believed
	/// (<see cref="ScalingClock"/>). A skip that went back to the start of the line on every refill,
	/// or rescanned the window for a line number at every position, is quadratic and fails it.
	/// </remarks>
	[Theory]
	[InlineData("reader")]
	[InlineData("stream yield")]
	[InlineData("stream whole")]
	public void Stepping_over_a_corrupted_line_takes_time_linear_in_its_length(string form)
	{
		var host    = FeedHost.Value;
		var reading = ScalingClock.Settle(
			() => Read(host, form, TimedLine,     retained: null, look: false),
			() => Read(host, form, TimedLine * 4, retained: null, look: false),
			Math.Pow(4.0, Linear));

		Assert.True(
			reading.Within,
			$"{form}: four times the broken line took {reading.Ratio:F2} times as long ({reading.Shorter:F2} ms against " +
			$"{reading.Longer:F2}), an exponent of {reading.Exponent(4.0):F2} against {Linear:F2}, on every one of " +
			$"{ScalingClock.Attempts} measurements ({reading.Readings}).");
	}

	/// <summary>
	/// The plain reader form steps over a corrupted line in no more than <see cref="AgainstBuffered"/>
	/// times what the buffered form takes for the same line.
	/// </summary>
	/// <remarks>
	/// Both read the line once to find the next place the synchronization stands. The reader form
	/// asks for it with a reading of its own, which costs far more than a character does, and it
	/// asked at every character of the line: a megabyte took ten times the buffered form's time,
	/// linear and so invisible to the ratchet above. A ratio of the two forms over the same line, on
	/// one machine in alternating rounds, is what shows it.
	/// </remarks>
	[Fact]
	public void The_reader_form_steps_over_a_corrupted_line_about_as_fast_as_the_buffered_form()
	{
		var host    = FeedHost.Value;
		var reading = ScalingClock.Settle(
			() => Read(host, "stream whole", ShortLine, retained: null, look: false),
			() => Read(host, "reader",       ShortLine, retained: null, look: false),
			AgainstBuffered);

		Assert.True(
			reading.Within,
			$"the reader form took {reading.Ratio:F2} times as long as the buffered form ({reading.Longer:F2} against " +
			$"{reading.Shorter:F2}) to step over a corrupted line of {ShortLine:N0} characters, against a bound of " +
			$"{AgainstBuffered:F1}, on every one of {ScalingClock.Attempts} measurements ({reading.Readings}).");
	}

	/// <summary>How many times the buffered form's time the reader form may take over one corrupted line.</summary>
	const double AgainstBuffered = 3.0;

	// ── A refusal at the end of a long record ───────────────────────────────────────

	/// <summary>
	/// Two alternatives that share a prefix as long as the record, and differ only in its last
	/// character, which the input has neither of.
	/// </summary>
	const string Prefix = """
		Line  : @int   = 'R' & ['0'..'9']+ & 'a' & '\n' => @(1)
		              | 'R' & ['0'..'9']+ & 'b' & '\n' => @(2)
		Lines : @int[] = Line*
		parse Lines
		parse Lines as Streamed stream
		""";

	static readonly Lazy<Type> PrefixHost = new(() => Compile(Prefix));

	/// <summary>
	/// A record refused at its last character, after a prefix two alternatives both read, is
	/// refused where that character is, in time linear in the prefix and holding the record and
	/// nothing more; and a refused parse leaves nothing of the record live once it has answered.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Each alternative reads the whole prefix before it fails, so the refusal costs at least two
	/// readings of it; it may not cost one reading per alternative per character. The bound on
	/// time is the same blow-up ratchet as above.
	/// </para>
	/// <para>
	/// Memory is held to the same bound a character as the corrupted line, <see cref="Window"/>, for
	/// the buffered form while it reads. What is live after the call against before it is held to
	/// <see cref="Kept"/>: a parse that kept its arena, its failure or its buffer for a record of
	/// megabytes would leave megabytes. What the shared pool keeps of a buffer across calls is not
	/// asked here — a buffer the pool already holds is reused and leaves nothing new behind — and
	/// is <c>StreamingRetentionTests</c>' question. The string form holds the caller's string, which
	/// is not the parse's to let go, and is asked only the answer and the time.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("string")]
	[InlineData("stream")]
	public void A_refusal_after_a_long_shared_prefix_is_told_at_its_place_in_linear_time_and_memory(string form)
	{
		var host    = PrefixHost.Value;
		var shorter = Refuse(host, form, ShortLine);
		var longer  = Refuse(host, form, LongLine);

		foreach (var (refusal, length) in new[] { (shorter, ShortLine), (longer, LongLine) })
		{
			Assert.False(refusal.IsSuccess);
			Assert.Equal(PrefixLead.Length + 1 + length, refusal.Position);
		}

		if (form == "stream")
		{
			var perCharacter = (longer.Peak - shorter.Peak) / (double)(LongLine - ShortLine);

			Assert.True(
				perCharacter <= Window,
				$"{form}: the peak grew by {perCharacter:F2} bytes a character of the refused record, from {shorter.Peak:N0} " +
				$"at {ShortLine:N0} characters to {longer.Peak:N0} at {LongLine:N0}, against a bound of {Window}.");

			Assert.True(
				longer.Retained <= Kept,
				$"{form}: once a record of {LongLine:N0} characters was refused, {longer.Retained:N0} bytes more were live " +
				$"than before the call, against a bound of {Kept:N0}.");
		}

		var reading = ScalingClock.Settle(
			() => Refuse(host, form, TimedLine,     look: false),
			() => Refuse(host, form, TimedLine * 4, look: false),
			Math.Pow(4.0, Linear));

		Assert.True(
			reading.Within,
			$"{form}: a refusal after four times the prefix took {reading.Ratio:F2} times as long ({reading.Shorter:F2} ms " +
			$"against {reading.Longer:F2}), an exponent of {reading.Exponent(4.0):F2} against {Linear:F2}, on every one " +
			$"of {ScalingClock.Attempts} measurements ({reading.Readings}).");
	}

	// ── Sizes and bounds ────────────────────────────────────────────────────────────

	/// <summary>The two lengths memory is compared at, in characters.</summary>
	const int ShortLine = 1 << 20;

	const int LongLine = 4 << 20;

	/// <summary>The shorter of the two lengths time is compared at; the longer is four times it.</summary>
	const int TimedLine = 1 << 18;

	/// <summary>
	/// What the buffered form may hold in the test of a window shorter than the line, in characters.
	/// </summary>
	const int Retained = 1 << 16;

	/// <summary>
	/// What the peak may grow by a character of the long record.
	/// </summary>
	/// <remarks>
	/// A buffer that doubles holds up to twice the record, four bytes a character, and while it grows
	/// the buffer it is leaving is still live beside it, two more: six, which is what the reader form
	/// measured (6.00), with the buffered form a little over it (6.67 lazily; the whole form under
	/// five). The bound leaves one and a half for the rest. A parse that also kept the record as a
	/// string, two bytes a character, would read eight and fail.
	/// </remarks>
	const double Window = 7.5;

	/// <summary>The exponent of time over a factor of four in length that counts as a blow-up.</summary>
	const double Linear = 1.6;

	/// <summary>
	/// What may differ between two runs that both hold no more than a window of <see cref="Retained"/>
	/// characters: the window itself, twice over for a buffer that doubles, and a quarter of a megabyte
	/// for the rest of the process.
	/// </summary>
	const long Slack = 2L * Retained * sizeof(char) * 2 + 256 * 1024;

	/// <summary>
	/// What a refused parse may leave live that was not live before it: the most the shared pool
	/// may be handed of a buffer (two kept lengths of 1,048,576 characters, §6.3), and a quarter of
	/// a megabyte for the rest of the process. A record of <see cref="LongLine"/> characters is
	/// eight megabytes; keeping it, or a store sized to it, exceeds this.
	/// </summary>
	const long Kept = 2L * 1048576 * sizeof(char) + 256 * 1024;

	// ── Running them ────────────────────────────────────────────────────────────────

	/// <summary>What one run handed back, how it ended, and the most that was live while it ran.</summary>
	sealed record Run(int[] Values, Exception? Failure, long Peak);

	/// <summary>
	/// What one refusal answered, the most that was live while it ran, and how much more was live
	/// after it than before.
	/// </summary>
	sealed record Refusal(bool IsSuccess, long Position, long Peak, long Retained);

	static int[] Expected()
	{
		var values = new List<int>();

		for (var record = 0; record < Before; record++)
			values.Add(record % 10);

		values.Add(-1);

		for (var record = 0; record < After; record++)
			values.Add(record % 10);

		return [.. values];
	}

	static Run Read(Type host, string form, int line, int? retained, bool look = true)
	{
		var source = new Source([("R0\nR1\n", 1), (Chunk('x'), line / ChunkLength), ("\n", 1), (Records(), 1)], look);
		var values = new List<int>();

		Exception? failure = null;

		try
		{
			var read = form switch
			{
				"reader"       => Call(host, "ParseRows", source),
				"stream yield" => Call(host, "Streamed",  source, 4096, retained ?? int.MaxValue),
				"stream whole" => Call(host, "Whole",     source, 4096, retained ?? int.MaxValue),
				_              => throw new ArgumentOutOfRangeException(nameof(form)),
			};

			foreach (int value in (IEnumerable)read!)
				values.Add(value);
		}
		catch (Exception thrown) when (thrown is IOException or FormatException)
		{
			failure = thrown;
		}

		return new Run([.. values], failure, source.Peak);
	}

	static string Records()
	{
		var records = new System.Text.StringBuilder();

		for (var record = 0; record < After; record++)
			records.Append('R').Append(record % 10).Append('\n');

		return records.ToString();
	}

	/// <summary>A long record is written a chunk at a time; every length here is a multiple of it.</summary>
	const int ChunkLength = 1024;

	static string Chunk(char filler)
	{
		return new string(filler, ChunkLength);
	}

	/// <summary>What stands before the long record: two records each alternative reads.</summary>
	const string PrefixLead = "R1a\nR2b\n";

	static Refusal Refuse(Type host, string form, int length, bool look = true)
	{
		if (form == "string")
		{
			var text  = PrefixLead + "R" + new string('7', length) + "c\n";
			var match = Call(host, "TryParseLines", text)!;

			return new Refusal(IsSuccess(match), Position(match), 0, 0);
		}

		var source = new Source([(PrefixLead + "R", 1), (Chunk('7'), length / ChunkLength), ("c\n", 1)], look);
		var before = look ? GC.GetTotalMemory(forceFullCollection: true) : 0;
		var found  = Call(host, "TryStreamed", source, 4096, int.MaxValue)!;
		var after  = look ? GC.GetTotalMemory(forceFullCollection: true) : 0;

		return new Refusal(IsSuccess(found), Position(found), source.Peak, after - before);
	}

	static bool IsSuccess(object match)
	{
		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	static long Position(object match)
	{
		return Convert.ToInt64(match.GetType().GetProperty("Position")!.GetValue(match));
	}

	static object? Call(Type host, string name, object input, params int[] rest)
	{
		var types  = new[] { input is TextReader ? typeof(TextReader) : typeof(string) }.Concat(rest.Select(_ => typeof(int?))).ToArray();
		var method = host.GetMethod(name, types)
			?? throw new InvalidOperationException($"{host.Name} has no {name}({string.Join(", ", types.Select(one => one.Name))}).");

		try
		{
			return method.Invoke(null, [input, .. rest.Select(one => (object)one)]);
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
	/// A text made of pieces, each a string repeated a number of times, written as it is asked for
	/// and never held whole; and the place the memory is looked at, at every read.
	/// </summary>
	sealed class Source((string Text, int Times)[] pieces, bool look) : TextReader
	{
		int _piece;
		int _time;
		int _at;

		public long Peak { get; private set; }

		public override int Read(char[] buffer, int index, int count)
		{
			return Read(buffer.AsSpan(index, count));
		}

		public override int Read(Span<char> buffer)
		{
			if (look)
			{
				var live = GC.GetTotalMemory(forceFullCollection: true);

				if (live > Peak)
					Peak = live;
			}

			var written = 0;

			while (written < buffer.Length && _piece < pieces.Length)
			{
				var (text, times) = pieces[_piece];
				var take          = Math.Min(buffer.Length - written, text.Length - _at);

				text.AsSpan(_at, take).CopyTo(buffer.Slice(written));

				written += take;
				_at     += take;

				if (_at < text.Length)
					continue;

				_at = 0;

				if (++_time < times)
					continue;

				_time = 0;
				_piece++;
			}

			return written;
		}

		public override int Read()
		{
			Span<char> one = stackalloc char[1];

			return Read(one) == 0 ? -1 : one[0];
		}

		public override int Peek()
		{
			throw new NotSupportedException("A streamed parse does not peek.");
		}
	}
}
