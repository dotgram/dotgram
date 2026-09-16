using System;
using System.IO;
using System.Linq;
using System.Text;
using DotGram.Finance.Fix;
using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixFlatFieldsTests
{
	[Fact]
	public void Fields_do_not_require_a_message_or_semantic_validity()
	{
		var fields = Fix44.ParseLog("55=ABC|38=bad|54=Z|55=DEF|");
		Assert.Equal(new[] { 55, 38, 54, 55 }, fields.Select(f => f.Tag));
		Assert.Equal("ABC", Assert.IsType<FixField.Symbol>(fields[0]).Value);
		Assert.False(fields[1].IsValid);
		Assert.Equal('Z', Assert.IsType<FixField.Side>(fields[2]).Value);
		Assert.Empty(Fix44.Parse(""));
	}

	[Theory]
	[MemberData(nameof(Fix44Tests.Messages), MemberType = typeof(Fix44Tests))]
	public void Explicit_semantics_reuses_the_parsed_fields(string name, string wire)
	{
		var fields = Fix44.Parse(wire);
		var message = FixMessages.Build(wire, fields);
		Assert.Equal(name, message.GetType().Name);
		Assert.Equal(fields.Length, message.AllFields.Count());
		foreach (var pair in fields.Zip(message.AllFields)) Assert.Same(pair.First, pair.Second.TypedValue);
		var corrupt = wire.Substring(0, wire.Length - 4) + "999" + wire[^1];
		var corruptFields = Fix44.Parse(corrupt);
		Assert.False(FixMessages.TryBuild(corrupt, corruptFields, out _, out _));
	}

	[Theory]
	[MemberData(nameof(Fix44Tests.Messages), MemberType = typeof(Fix44Tests))]
	public void Lazy_streams_match_all_standard_message_fixtures(string name, string wire)
	{
		Assert.NotEmpty(name);
		var expected = Fix44.Parse(wire);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(wire));
		using var reader = new StringReader(wire);
		foreach (var fields in new[] { Fix44.Parse(stream, bufferSize: 3), Fix44.Parse(reader, bufferSize: 3) })
		{
			var actual = fields.ToArray();
			Assert.Equal(expected.Select(f => (f.GetType(), f.Position, f.ValuePosition, f.Length, f.IsValid)),
				actual.Select(f => (f.GetType(), f.Position, f.ValuePosition, f.Length, f.IsValid)));
		}
	}

	[Theory]
	[InlineData(1)]
	[InlineData(3)]
	[InlineData(17)]
	public void Direct_streams_return_all_fields_with_global_locations(int bufferSize)
	{
		var wire = (string)Fix44Tests.Messages().First()[1];
		var input = wire + wire;
		var expected = Fix44.Parse(input);
		using var reader = new StringReader(input);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));
		var chars = Fix44.Parse(reader, bufferSize: bufferSize);
		var bytes = Fix44.Parse(stream, bufferSize: bufferSize);
		Assert.True(stream.CanRead);
		foreach (var actual in new[] { chars, bytes })
			Assert.Equal(expected.Select(f => (f.GetType(), f.Position, f.ValuePosition, f.Length)),
				actual.Select(f => (f.GetType(), f.Position, f.ValuePosition, f.Length)));
		Assert.Equal(wire.Length, expected[expected.Length / 2].Position);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(17)]
	[InlineData(4097)]
	public void Raw_data_crosses_buffers_and_preserves_delimiter_octets(int size)
	{
		var payload = new string(Enumerable.Range(0, size).Select(i => "|=\0ÿ"[i % 4]).ToArray());
		var input = "95=" + size + "|96=" + payload + "|55=END|";
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));
		var fields = Fix44.Parse(stream, new FixFieldOptions('|'), bufferSize: 3).ToArray();
		Assert.Equal(Encoding.Latin1.GetBytes(payload), Assert.IsType<FixField.RawData>(fields[1]).Value.ToArray());
		Assert.Equal("END", Assert.IsType<FixField.Symbol>(fields[2]).Value);
	}

	[Fact]
	public void Vendor_raw_fields_use_syntax_options_on_the_native_stream()
	{
		using var stream = new ShortStream(Encoding.Latin1.GetBytes("9000=3|9001=a|b|55=X|"));
		var fields = Fix44.Parse(stream, new FixFieldOptions('|', new FixDataPair(9000, 9001)), bufferSize: 1).ToArray();
		Assert.Equal(new byte[] { 97, 124, 98 }, Assert.IsType<FixField.Unknown>(fields[1]).Value.ToArray());
		Assert.Equal("X", Assert.IsType<FixField.Symbol>(fields[2]).Value);
	}

	[Theory]
	[InlineData("95=3|55=X|96=abc|")]
	[InlineData("95=4|96=abc|")]
	[InlineData("55=abc")]
	[InlineData("0=x|")]
	[InlineData("55abc|")]
	public void Malformed_field_syntax_is_rejected(string input)
		=> Assert.False(Fix44.TryParse(input, out _, out _, new FixFieldOptions('|')));

	[Theory]
	[InlineData("96=abc|")]
	[InlineData("95=3|")]
	[InlineData("95=3|89=abc|")]
	[InlineData("95=1|96=a|96=b|")]
	[InlineData("95=-1|96=|")]
	[InlineData("95=+1|96=a|")]
	[InlineData("95=-0|96=|")]
	public void Incomplete_or_mismatched_pairs_fail_in_all_input_forms(string input)
	{
		var options = new FixFieldOptions('|');
		Assert.False(Fix44.TryParse(input, out _, out _, options));
		using var reader = new StringReader(input);
		Assert.False(Fix44.TryParse(reader, out _, out _, options, bufferSize: 1));
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));
		Assert.False(Fix44.TryParse(stream, out _, out _, options, bufferSize: 1));
	}

	[Fact]
	public void Streaming_pair_returns_both_fields_without_reading_the_next_field()
	{
		const string pair = "95=3|96=a|b|";
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(pair + "55=X|")) { ReadLimit = pair.Length };
		using var fields = Fix44.Parse(stream, new FixFieldOptions('|'), bufferSize: 1).GetEnumerator();
		Assert.True(fields.MoveNext());
		Assert.IsType<FixField.RawDataLength>(fields.Current);
		Assert.Equal(pair.Length, stream.ReadCount);
		Assert.True(fields.MoveNext());
		Assert.Equal(new byte[] { 97, 124, 98 }, Assert.IsType<FixField.RawData>(fields.Current).Value.ToArray());
		Assert.Equal(pair.Length, stream.ReadCount);
	}

	[Fact]
	public void Stream_enumeration_is_lazy_and_does_not_wait_for_the_next_field()
	{
		using var stream = new ShortStream(Encoding.ASCII.GetBytes("55=ABC|38=2|")) { ReadLimit = 7 };
		var fields = Fix44.Parse(stream, new FixFieldOptions('|'), bufferSize: 1);
		Assert.Equal(0, stream.ReadCount);
		using (var iterator = fields.GetEnumerator())
		{
			Assert.Equal(0, stream.ReadCount);
			Assert.True(iterator.MoveNext());
			Assert.Equal("ABC", Assert.IsType<FixField.Symbol>(iterator.Current).Value);
			Assert.Equal(7, stream.ReadCount);
		}
		Assert.True(stream.CanRead);
	}

	[Fact]
	public void Reader_enumeration_is_lazy_and_stops_at_a_complete_field()
	{
		using var reader = new GatedReader("55=ABC|");
		var fields = Fix44.Parse(reader, new FixFieldOptions('|'), bufferSize: 1);
		Assert.Equal(0, reader.ReadCount);
		using var iterator = fields.GetEnumerator();
		Assert.True(iterator.MoveNext());
		Assert.Equal("ABC", Assert.IsType<FixField.Symbol>(iterator.Current).Value);
		Assert.Equal(7, reader.ReadCount);
	}

	[Theory]
	[InlineData("55=A|broken|55=B|")]
	[InlineData("55=A|95=3|96=ab")]
	public void Errors_are_reported_during_enumeration_without_skipping_input(string wire)
	{
		using var stream = new ShortStream(Encoding.ASCII.GetBytes(wire));
		using var fields = Fix44.Parse(stream, new FixFieldOptions('|'), bufferSize: 1).GetEnumerator();
		Assert.True(fields.MoveNext());
		Assert.IsType<FixField.Symbol>(fields.Current);
		Assert.Throws<FormatException>(() => { while (fields.MoveNext()) { } });
		Assert.True(stream.CanRead);
	}

	[Fact]
	public void Many_fields_release_input_and_keep_global_locations_and_owned_values()
	{
		var wire = string.Concat(Enumerable.Repeat("95=3|96=a|b|55=X|", 1000));
		using var stream = new ShortStream(Encoding.ASCII.GetBytes(wire));
		var fields = Fix44.Parse(stream, new FixFieldOptions('|'), bufferSize: 3, maxRetained: 32).ToArray();
		Assert.Equal(3000, fields.Length);
		Assert.Equal(new byte[] { 97, 124, 98 }, Assert.IsType<FixField.RawData>(fields[1]).Value.ToArray());
		Assert.Equal(wire.Length - 5, fields[^1].Position);
		Assert.True(stream.CanRead);
	}

	sealed class GatedReader(string text) : StringReader(text)
	{
		readonly int length = text.Length;
		public int ReadCount { get; private set; }
		public override int Read(char[] buffer, int index, int count)
		{
			if (ReadCount >= length) throw new IOException("The next field has not arrived.");
			var read = base.Read(buffer, index, Math.Min(count, 1));
			ReadCount += read;
			return read;
		}
	}

	sealed class ShortStream(byte[] data) : Stream
	{
		readonly MemoryStream inner = new(data);
		public int ReadLimit { get; set; } = int.MaxValue;
		public int ReadCount { get; private set; }
		public override bool CanRead => inner.CanRead;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (ReadCount >= ReadLimit) throw new IOException("The next field has not arrived.");
			var read = inner.Read(buffer, offset, Math.Min(count, Math.Min(2, ReadLimit - ReadCount)));
			ReadCount += read;
			return read;
		}
		public override void Flush() => throw new NotSupportedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
	}
}
