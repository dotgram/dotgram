using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A streamed parse holds what the grammar cannot yet let go of, and not what it has read
/// (docs/design/architecture-decisions.md, D5).
/// </summary>
/// <remarks>
/// <para>
/// Every form here is fed from a source that makes its text as it is asked for and keeps
/// none of it, so nothing but the parse can be holding the input. The source is also where
/// the memory is looked at: it is called from inside the parse, between one buffer and the
/// next, which is the only place a parse returning a whole array can be seen mid-flight.
/// </para>
/// <para>
/// Each form is run at two sizes, eight times apart, and what is live at the peak is
/// compared. A lazy form must hold the same at both. A form returning the whole result must
/// hold its result and nothing like the input: it may grow by what a record is worth to the
/// parse, never by what a record is long.
/// </para>
/// <para>
/// Every hundredth record is broken, and the recovery drops it and answers -1 in its place,
/// so the bound is held through recovery and not only along the clean path.
/// </para>
/// <para>
/// <b>Run alone.</b> What is live is the whole process's heap, not this test's: a neighbour
/// running beside it would be measured with it, and would pass or fail it at random.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class StreamingRetentionTests
{
	const string Grammar = """
		Row : @int = 'R' & text: ['a'..'z']+ & '\n' => @(text.Length)
		Rows : @int[] = Row* recover '\n' => @(-1)
		parse Rows as All
		parse Rows as Read yield : @int
		""";

	/// <summary>How long a record's text is. Large, so that holding input is plainly unlike holding records.</summary>
	const int Payload = 1000;

	/// <summary>Every this many records, one broken one the recovery has to take out.</summary>
	const int BrokenEvery = 100;

	const int Small = 2_000;

	const int Large = Small * 8;

	/// <summary>
	/// What may be live at the peak of the large run beyond the small one, for a form that hands
	/// its elements over as it reads them.
	/// </summary>
	/// <remarks>
	/// Measured after a forced, blocking, compacting collection, what is live is the parse's own
	/// state and nothing else: the buffer, the arena, the value tables, and the pools, which
	/// reach their high-water mark within the first records and stay there. A heap-limited run
	/// of a gigabyte through FIX's grammar held 9 to 29 KB above its floor, the same as at 16 MB.
	/// This allows ten times that, and it is still about a hundredth of the fourteen megabytes
	/// the two runs differ by in input. A parse holding even one per cent of what it read fails.
	/// </remarks>
	const long Slack = 256 * 1024;

	[Theory]
	[InlineData("lines", false)]
	[InlineData("reader", false)]
	[InlineData("reader yield", true)]
	[InlineData("stream yield", true)]
	public void A_lazy_form_holds_the_same_however_much_it_has_read(string form, bool buffered)
	{
		var (small, large) = Measure(form, buffered);

		Assert.True(large.Peak <= small.Peak + Slack,
			$"{form}: {small.Peak} bytes live at the peak of {small.Records} records, {large.Peak} at {large.Records}; " +
			$"a lazy form may not grow with what it has read.");
	}

	/// <remarks>
	/// <para>
	/// <b>A whole form over a stream holds the tape.</b> This said the opposite until
	/// 2026-09-24 — "the rows are handed over one at a time and each is let go, so it holds a
	/// record and not the stream" — and the parse has never done that. The walk that builds the
	/// result runs after the parse is accepted, so every decision has to survive until then,
	/// whatever the result weighs. Here the result is an <c>int</c> a record, four bytes, and
	/// the parse holds about four hundred and fifty.
	/// </para>
	/// <para>
	/// Measured on 2026-09-24 by two heap dumps, one size a process, taken with the parse held
	/// mid-flight. Peaks of 13,907,896 at 2,000 records and 20,353,760 at 16,000 — 457 bytes a
	/// record — and of the 363 the dumps account for:
	/// </para>
	/// <list type="bullet">
	/// <item><description>268 B, the arena (<c>ParserEntry[]</c>) — three quarters of it;</description></item>
	/// <item><description>38 B, an <c>object[]</c>;</description></item>
	/// <item><description>37 B, the int values awaiting the walk (<c>int[][] _values0</c>);</description></item>
	/// <item><description>21 B, the ways on the tape (<c>Ways.Items</c>, <c>Ways.Log</c>).</description></item>
	/// </list>
	/// <para>
	/// No field object, no string, no list, and nothing of the pool: the result is one per cent
	/// of what is held and the rest is deferral. So the bound below is not about the result. It
	/// is that a parse may hold its tape and may not approach holding the TEXT: a record is
	/// <see cref="Payload"/> characters, two bytes each, and the tape costs about a quarter of
	/// that. The bound is one byte a character — half the text, twice what is held — so it
	/// leaves room for the tape and none for the input.
	/// </para>
	/// <para>
	/// <b>Why the old assertion ever passed</b>, since that is the part worth not repeating: its
	/// bound was a quarter of a record and the growth was already 455 bytes at 4,000 and 8,000
	/// records on both sides of every commit in range. It passed only at its own chosen size,
	/// because at 16,000 records the pool of the day threw the oversized arena away and the peak
	/// FELL. 02e44143 stopped throwing it away, and the assertion had nothing left to stand on.
	/// A test whose passing condition is a discard two commits away is not measuring its subject.
	/// </para>
	/// <para>
	/// What holds constant is the lazy forms, and they have their own test
	/// (<see cref="A_lazy_form_holds_the_same_however_much_it_has_read"/>), which passes on all
	/// four. So this pair is what deferral costs, not what the grammar costs. A whole stream the
	/// recovering reader reads — FIX’s, which is not a streamed parse — is in
	/// DotGram.Finance.Tests (FixRetentionTests.A_whole_stream_holds_a_field_and_not_the_stream).
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("reader whole")]
	[InlineData("stream whole")]
	public void A_whole_result_over_a_stream_holds_its_tape_and_not_the_text(string form)
	{
		var (small, large) = Measure(form, buffered: true);
		var perRecord      = (large.Peak - small.Peak) / (double)(large.Records - small.Records);

		Assert.True(
			perRecord < Tape,
			$"{form}: the peak grew by {perRecord:F0} bytes a record over " +
			$"{large.Records - small.Records} records, against a bound of {Tape}. A record is " +
			$"{Payload} characters, so at this rate the parse is holding the text and not just the " +
			$"tape it defers (peaks {small.Peak} and {large.Peak}).");
	}

	/// <summary>What a whole form may hold a record, holding its tape but not the text.</summary>
	/// <remarks>
	/// One byte a character of the record. The text itself is two bytes a character, and the tape
	/// measured about a half of one — so this is twice what is held and half of what would mean
	/// the input was being kept. It is a bound on a known quantity and not a target: if the tape
	/// is ever made to cost less, this comes down with it rather than being left as slack.
	/// </remarks>
	const double Tape = Payload;

	/// <remarks>
	/// <para>
	/// This asserted that a finished parse leaves NOTHING behind until 2026-09-24, and it was
	/// right to until <c>02e44143</c> reversed the decision: an oversized store used to be
	/// dropped, which was a cliff rather than a bound -- one entry over and the next parse of a
	/// document that size grew everything again from nothing -- so such a store is now held while
	/// the work keeps wanting it. That commit rewrote <c>PoolRetentionTests</c> to the policy it
	/// chose and did not reach this one, which has been failing since. It is rewritten rather
	/// than deleted for the reason given there: a test contradicting a deliberate change of
	/// design looks exactly like a test that caught a regression, and only knowing which way the
	/// decision went tells them apart.
	/// </para>
	/// <para>
	/// So both halves of the chosen policy are asserted here, because either alone is a policy
	/// nobody chose. <b>Kept</b>: the store the large parse grew is still live after it, which is
	/// what makes a loop of large parses reuse it deterministically rather than when a collection
	/// happens not to have intervened. <b>Let go</b>: it is a bound and not hoarding, so once
	/// <see cref="LetGoAfter"/> parses in a row have not wanted it, it is handed to the collector
	/// and a full collection reclaims it. A test that checked only the first would pass over a
	/// parser that never let go of anything.
	/// </para>
	/// <para>
	/// What is NOT relaxed is the buffer: it is rented from the shared pool, which keeps what it
	/// is given for the life of the process, so a buffer grown to hold a long input once stayed
	/// live after the parse -- 47 MB of bytes and 112 MB of characters -- before the generated
	/// input class stopped returning a buffer longer than its kept length. <see cref="Kept"/> is
	/// still what the pool may hold of it, and the second half is held to that same figure.
	/// </para>
	/// <para>
	/// Run over a whole-result form because that is the one whose buffer grows with the input;
	/// how much it holds while it runs is the other test's business, and not this one's.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("reader whole")]
	[InlineData("stream whole")]
	public void A_finished_parse_keeps_what_it_grew_until_the_work_stops_wanting_it(string form)
	{
		var host = Compile(buffered: true);

		// The small run settles the JIT and fills the pool's buckets up to the kept length,
		// which is what the pool may legitimately go on keeping.
		Run(host, form, Small);

		var before = GC.GetTotalMemory(forceFullCollection: true);

		Run(host, form, Large);

		var kept = GC.GetTotalMemory(forceFullCollection: true);

		Assert.True(kept > before,
			$"{form}: {before} bytes live before a parse of {Large} records and {kept} after it. " +
			"The store it grew is supposed to be HELD, so that a loop of parses this size reuses " +
			"it; nothing being left is the cliff 02e44143 removed, not the bound it put there.");

		// Parses that do not want the room it grew. Each is a rental, and the slot counts rentals.
		for (var idle = 0; idle < LetGoAfter; idle++)
			Run(host, form, Small);

		var freed = GC.GetTotalMemory(forceFullCollection: true);

		Assert.True(freed <= before + Kept,
			$"{form}: {freed} bytes live after {LetGoAfter} parses of {Small} records that did not " +
			$"want the room, against {before} before the large one. A store nobody has wanted for " +
			"that many parses is handed to the collector, so what is left may be only what the " +
			"pool keeps below the kept length.");
	}

	/// <summary>Parses in a row that do not want the room before an oversized store is let go of.</summary>
	/// <remarks>
	/// <c>LargeIdle</c> and <c>LargeParserIdle</c> in <c>Support.cs</c>, both 8. It is written
	/// here as the generator's number rather than derived, because there is nothing in the
	/// emitted parser a test can ask. If the generator's number grows and this one does not,
	/// the second assertion fails and says so -- which is the direction a stale copy should fail
	/// in, and why the count is not padded with a margin that would hide it.
	/// </remarks>
	const int LetGoAfter = 8;

	/// <summary>What the pool may keep of a buffer's growth once a parse is over.</summary>
	/// <remarks>
	/// The generated input class returns a buffer to the shared pool only up to its kept length,
	/// 1,048,576 elements (BufferedEmitter). Doubling from 4,096 up to that passes through buckets
	/// that together hold under two kept lengths, at two bytes an element for characters. The
	/// small run has already filled those buckets, so the large one should add nothing to them;
	/// this is the most it could add if the pool kept one more array a bucket, plus the slack the
	/// lazy forms are allowed.
	/// </remarks>
	const long Kept = 2L * 1048576 * sizeof(char) + Slack;

	static (Outcome Small, Outcome Large) Measure(string form, bool buffered)
	{
		var host = Compile(buffered);

		// The first run settles the pools and the JIT, so neither is charged to a size.
		Run(host, form, Small);

		return (Run(host, form, Small), Run(host, form, Large));
	}

	/// <summary>What one run found: how many values came back, and the most that was ever live.</summary>
	readonly record struct Outcome(int Records, long Peak);

	static Outcome Run(Type host, string form, int records)
	{
		var source = new Source(records);
		var values = Read(host, form, source);
		var count  = 0;
		var broken = 0;

		foreach (int value in values)
		{
			count++;

			if (value == -1)
				broken++;
			else
				Assert.Equal(Payload, value);

			source.Look();
		}

		var expected = records / BrokenEvery;

		Assert.Equal(records + expected, count);
		Assert.Equal(expected, broken);

		return new Outcome(count, source.Peak);
	}

	static IEnumerable Read(Type host, string form, Source source)
	{
		return form switch
		{
			"lines" => Call(host, "All", source.Lines()),
			"reader" => Call(host, "All", source.Reader()),
			"reader yield" => Call(host, "Read", source.Reader(), 4096, int.MaxValue),
			"stream yield" => Call(host, "Read", source.Stream(), 4096, int.MaxValue),
			"reader whole" => Call(host, "All", source.Reader(), 4096, int.MaxValue),
			"stream whole" => Call(host, "All", source.Stream(), 4096, int.MaxValue),
			_ => throw new ArgumentOutOfRangeException(nameof(form)),
		};
	}

	static IEnumerable Call(Type host, string name, object input, params object[] rest)
	{
		var domain = input switch
		{
			Stream     => typeof(Stream),
			TextReader => typeof(TextReader),
			_          => typeof(IEnumerable<string>),
		};

		var types  = new[] { domain }.Concat(rest.Select(_ => typeof(int?))).ToArray();
		var method = host.GetMethod(name, types)
			?? throw new InvalidOperationException($"{host.Name} has no {name}({string.Join(", ", types.Select(one => one.Name))}).");

		return (IEnumerable)method.Invoke(null, new[] { input }.Concat(rest).ToArray())!;
	}

	static Type Compile(bool buffered)
	{
		var result = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			BufferedInput = buffered,
			BufferedBytes = buffered,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text).GetType("Grammar")!;
	}

	/// <summary>
	/// The text of the records, made as it is asked for, in whichever form a parse takes it,
	/// and the place the memory is looked at while the parse is inside a call.
	/// </summary>
	sealed class Source(int records)
	{
		static readonly string Row = "R" + new string('a', Payload);

		int _next;

		int _calls;

		public long Peak { get; private set; }

		/// <summary>The next record's line, without its terminator; null when there are none left.</summary>
		string? Next()
		{
			if (_next == records + records / BrokenEvery)
				return null;

			// The record after every hundredth good one is the broken one.
			var line = (_next + 1) % (BrokenEvery + 1) == 0 ? "X" : Row;

			_next++;

			return line;
		}

		/// <summary>What is live now, after everything that can be collected has been.</summary>
		/// <remarks>Every so many calls, not every one: a full collection is not free.</remarks>
		public void Look()
		{
			if (++_calls % 64 != 0)
				return;

			var live = GC.GetTotalMemory(forceFullCollection: true);

			if (live > Peak)
				Peak = live;
		}

		public IEnumerable<string> Lines()
		{
			for (var line = Next(); line is not null; line = Next())
			{
				Look();

				yield return line;
			}
		}

		public TextReader Reader()
		{
			return new CharReader(this);
		}

		public Stream Stream()
		{
			return new ByteStream(this);
		}

		/// <summary>The lines with their terminators, a slice at a time.</summary>
		string _pending = "";

		int _at;

		int Fill(Span<char> buffer)
		{
			Look();

			var written = 0;

			while (written < buffer.Length)
			{
				if (_at == _pending.Length)
				{
					var line = Next();

					if (line is null)
						break;

					_pending = line + "\n";
					_at      = 0;
				}

				var take = Math.Min(buffer.Length - written, _pending.Length - _at);

				_pending.AsSpan(_at, take).CopyTo(buffer.Slice(written));

				written += take;
				_at     += take;
			}

			return written;
		}

		sealed class CharReader(Source source) : TextReader
		{
			public override int Read(char[] buffer, int index, int count)
			{
				return source.Fill(buffer.AsSpan(index, count));
			}

			public override int Read(Span<char> buffer)
			{
				return source.Fill(buffer);
			}

			public override int Read()
			{
				Span<char> one = stackalloc char[1];

				return source.Fill(one) == 0 ? -1 : one[0];
			}

			public override int Peek()
			{
				throw new NotSupportedException("A streamed parse does not peek.");
			}
		}

		sealed class ByteStream(Source source) : Stream
		{
			char[] _chars = [];

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (_chars.Length < count)
					_chars = new char[count];

				var read = source.Fill(_chars.AsSpan(0, count));

				for (var i = 0; i < read; i++)
					buffer[offset + i] = (byte)_chars[i];

				return read;
			}

			public override bool CanRead  => true;
			public override bool CanSeek  => false;
			public override bool CanWrite => false;
			public override long Length   => throw new NotSupportedException();
			public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
			public override void Flush() { }
			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}
		}
	}
}
