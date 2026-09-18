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
[Collection(typeof(GeneratorCostTests.Alone))]
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
	/// The result is an int a record, and the arena holds what a record is worth to the parse;
	/// neither is anywhere near a record's text. Today the parse holds all of its input until it
	/// completes, because construction is deferred and a capture is turned into a value from
	/// the input it points into. That is a defect under D5, not a limit of this form.
	/// </remarks>
	[Theory(Skip =
		"A buffered parse returning its whole result retains all of its input: a defect under D5 " +
		"(ruling of 2026-09-17, docs/design/architecture-decisions.md). The change that fixes it removes this skip.")]
	[InlineData("reader whole")]
	[InlineData("stream whole")]
	public void A_whole_result_grows_with_the_result_and_not_with_the_input(string form)
	{
		var (small, large) = Measure(form, buffered: true);
		var perRecord      = (large.Peak - small.Peak) / (double)(large.Records - small.Records);

		Assert.True(perRecord < Payload / 4.0,
			$"{form}: the peak grew by {perRecord:F0} bytes a record over {large.Records - small.Records} records; " +
			$"a record is {Payload} characters long, so the parse is holding input (peaks {small.Peak} and {large.Peak}).");
	}

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

	static IEnumerable Read(Type host, string form, Source source) => form switch
	{
		"lines"        => Call(host, "All", source.Lines()),
		"reader"       => Call(host, "All", source.Reader()),
		"reader yield" => Call(host, "Read", source.Reader(), 4096, int.MaxValue),
		"stream yield" => Call(host, "Read", source.Stream(), 4096, int.MaxValue),
		"reader whole" => Call(host, "All", source.Reader(), 4096, int.MaxValue),
		"stream whole" => Call(host, "All", source.Stream(), 4096, int.MaxValue),
		_              => throw new ArgumentOutOfRangeException(nameof(form)),
	};

	static IEnumerable Call(Type host, string name, object input, params object[] rest)
	{
		var domain = input switch
		{
			Stream     => typeof(Stream),
			TextReader => typeof(TextReader),
			_          => typeof(IEnumerable<string>),
		};

		var types  = new[] { domain }.Concat(rest.Select(one => one.GetType())).ToArray();
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

		public TextReader Reader() => new CharReader(this);

		public Stream Stream() => new ByteStream(this);

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
			public override int Read(char[] buffer, int index, int count) =>
				source.Fill(buffer.AsSpan(index, count));

			public override int Read(Span<char> buffer) => source.Fill(buffer);

			public override int Read()
			{
				Span<char> one = stackalloc char[1];

				return source.Fill(one) == 0 ? -1 : one[0];
			}

			public override int Peek() => throw new NotSupportedException("A streamed parse does not peek.");
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
			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		}
	}
}
