using System;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;
using DotGram.Handwritten.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// <c>maxRetained</c> bounds one field, from its tag through the separator that ends it: a field of
/// exactly that length is read, one element more is an <see cref="IOException"/> that names the limit.
/// The generated and the handwritten parser draw the line in the same place.
/// </summary>
public sealed class FixRetentionTests
{
	const int Limit = 64;

	public static TheoryData<string, bool, int> Readers()
	{
		var data = new TheoryData<string, bool, int>();

		foreach (var parser in new[] { "generated", "handwritten" })
		foreach (var bytes in new[] { false, true })
		foreach (var bufferSize in new[] { 1, 4096 })
			data.Add(parser, bytes, bufferSize);

		return data;
	}

	[Theory]
	[MemberData(nameof(Readers))]
	public void A_field_at_the_limit_is_read(string parser, bool bytes, int bufferSize)
	{
		var fields = Read(parser, bytes, bufferSize, Field(Limit) + Field(10));

		Assert.Equal(2, fields.Length);
		Assert.All(fields, field => Assert.True(field.IsValid));
	}

	[Theory]
	[MemberData(nameof(Readers))]
	public void A_field_past_the_limit_names_it(string parser, bool bytes, int bufferSize)
	{
		var error = Assert.Throws<IOException>(() => Read(parser, bytes, bufferSize, Field(Limit + 1)));

		Assert.Contains(Limit.ToString(), error.Message);
		Assert.Contains("maxRetained", error.Message);
	}

	/// <summary>
	/// The whole-stream form keeps a field's window, however long the stream (D5): it builds each
	/// field where the field ends and lets the input go before the next one, so a limit of a
	/// kilobyte reads ten thousand fields and a hundred thousand alike — where holding the stream
	/// would need the whole of it, and the limit would stop it at the first kilobyte. A broken
	/// field every hundred keeps recovery in it.
	/// </summary>
	[Theory]
	[InlineData(false, 10_000)]
	[InlineData(false, 100_000)]
	[InlineData(true, 10_000)]
	[InlineData(true, 100_000)]
	public void A_whole_stream_holds_a_field_and_not_the_stream(bool bytes, int count)
	{
		var text = new StringBuilder();

		for (var i = 0; i < count; i++)
			text.Append(i % 100 == 99 ? "bad" + (char)1 : Field(10));

		var context = new FixGrammar.FixContext(FixFieldOptions.Default);
		var fields  = bytes
			? FixGrammar.ParseFields(new MemoryStream(Encoding.Latin1.GetBytes(text.ToString())), context, 256, Limit * 16)
			: FixGrammar.ParseFields(new StringReader(text.ToString()), context, 256, Limit * 16);

		Assert.Equal(count, fields.Length);
		Assert.Equal(count / 100, fields.Count(static field => !field.IsValid));
	}

	[Fact]
	public void The_default_is_sixteen_mebi()
	{
		Assert.Equal(16 * 1024 * 1024, FixParser.DefaultMaxRetained);
	}

	/// <summary>A call that gives no limit gets the default: a field that never ends is refused at it.</summary>
	[Theory]
	[MemberData(nameof(Parsers))]
	public void Without_an_argument_the_default_bounds_a_field(string parser, bool bytes)
	{
		var error = Assert.Throws<IOException>(() => Endless(parser, bytes));

		Assert.Contains(FixParser.DefaultMaxRetained.ToString(), error.Message);
	}

	/// <summary>An argument replaces the default: a field the default reads is refused under a smaller one.</summary>
	[Theory]
	[MemberData(nameof(Parsers))]
	public void An_argument_overrides_the_default(string parser, bool bytes)
	{
		var text = Field(Limit + 1);

		Assert.Single(Read(parser, bytes, 4096, text, null));
		Assert.Throws<IOException>(() => Read(parser, bytes, 4096, text, Limit));
	}

	public static TheoryData<string, bool> Parsers()
	{
		var data = new TheoryData<string, bool>();

		foreach (var parser in new[] { "generated", "handwritten" })
		foreach (var bytes in new[] { false, true })
			data.Add(parser, bytes);

		return data;
	}

	// A field that never ends: "58=" and then 'x' for as long as it is read.
	static FixField[] Endless(string parser, bool bytes)
	{
		if (bytes)
		{
			using var stream = new EndlessStream();

			return parser == "generated" ? FixParser.Parse(stream).ToArray() : HandFixParser.Parse(stream).ToArray();
		}

		using var reader = new EndlessReader();

		return parser == "generated" ? FixParser.Parse(reader).ToArray() : HandFixParser.Parse(reader).ToArray();
	}

	sealed class EndlessReader : TextReader
	{
		long _read;

		public override int Read(char[] buffer, int index, int count)
		{
			for (var i = 0; i < count; i++, _read++)
				buffer[index + i] = _read < 3 ? "58="[(int)_read] : 'x';

			return count;
		}

		public override int Read(Span<char> buffer)
		{
			for (var i = 0; i < buffer.Length; i++, _read++)
				buffer[i] = _read < 3 ? "58="[(int)_read] : 'x';

			return buffer.Length;
		}
	}

	sealed class EndlessStream : Stream
	{
		long _read;

		public override bool CanRead  => true;
		public override bool CanSeek  => false;
		public override bool CanWrite => false;
		public override long Length   => throw new NotSupportedException();

		public override long Position
		{
			get => _read;
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			for (var i = 0; i < count; i++, _read++)
				buffer[offset + i] = _read < 3 ? (byte)"58="[(int)_read] : (byte)'x';

			return count;
		}

		public override void Flush()
		{
		}

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

	// A text field, tag and separator included, of exactly the given length.
	static string Field(int length)
	{
		return "58=" + new string('x', length - 4) + (char)1;
	}

	static FixField[] Read(string parser, bool bytes, int bufferSize, string text, int? maxRetained = Limit)
	{
		if (bytes)
		{
			using var stream = new MemoryStream(Encoding.Latin1.GetBytes(text));

			return parser == "generated"
				? FixParser.Parse(stream, null, bufferSize, maxRetained).ToArray()
				: HandFixParser.Parse(stream, null, bufferSize, maxRetained).ToArray();
		}

		using var reader = new StringReader(text);

		return parser == "generated"
			? FixParser.Parse(reader, null, bufferSize, maxRetained).ToArray()
			: HandFixParser.Parse(reader, null, bufferSize, maxRetained).ToArray();
	}
}
