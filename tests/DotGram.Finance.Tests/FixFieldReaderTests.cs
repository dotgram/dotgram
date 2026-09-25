using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;
using Xunit.Sdk;

namespace DotGram.Finance.Tests;

/// <summary>
/// What every FIX field parser must do, run over the parsers a concrete class names: the
/// product's generated and hand parsers in DotGram.Finance.Tests, the Fix44 oracle in
/// DotGram.Finance.Fix44.Tests, which compiles this file too. The oracle is held to the
/// product's behaviour here so that comparing the two elsewhere means something.
/// </summary>
public abstract class FixFieldReaderTests
{
	/// <summary>
	/// The parsers every test here runs over, in order.
	/// </summary>
	protected abstract FieldParser[] Parsers { get; }

	void Each(Action<FieldParser> check)
	{
		foreach (var parser in Parsers)
		{
			try
			{
				check(parser);
			}
			catch (Exception exception)
			{
				throw new XunitException(parser.Name + ": " + exception.Message, exception);
			}
		}
	}

	[Fact]
	public void Fields_do_not_require_a_message_or_semantic_validity()
	{
		Each(parser =>
		{
			var fields = parser.ParseLog("55=ABC|38=bad|54=Z|55=DEF|");

			Assert.Equal([55, 38, 54, 55], fields.Select(f => (int)f.Tag));
			Assert.Equal("ABC", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, fields[0]).Value);
			Assert.False(fields[1].IsValid);
			Assert.Equal('Z', FixFixtures.Typed<FixField.Character>(FixTag.Side, fields[2]).Value);
			Assert.Empty(parser.Parse(""));
		});
	}

	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void Explicit_semantics_reuses_the_parsed_fields(string name, string wire)
	{
		Each(parser =>
		{
			var fields  = parser.Parse(wire);
			var message = FixParser.BuildMessage(wire, fields);

			Assert.Equal(name, message.GetType().Name);

			// The message holds the objects the reader built, not copies of them.
			foreach (var field in fields)
				Assert.Contains(message.Fields, held => ReferenceEquals(held, field));

			var corrupt       = wire.Substring(0, wire.Length - 4) + "999" + wire[^1];
			var corruptFields = parser.Parse(corrupt);

			var corrupted = FixParser.BuildMessage(corrupt, corruptFields);

			Assert.False(corrupted.Validate(Fix44Context.Default));
			Assert.Contains(corrupted.InvalidFindings!, finding => finding.Rule == FixRule.CheckSumMismatch);
		});
	}

	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void Lazy_streams_match_all_standard_message_fixtures(string name, string wire)
	{
		Assert.NotEmpty(name);

		Each(parser =>
		{
			var expected = parser.Parse(wire);

			using var stream = new ShortStream(Encoding.Latin1.GetBytes(wire));
			using var reader = new StringReader(wire);

			foreach (var fields in new[] { parser.Parse(stream, bufferSize: 3), parser.Parse(reader, bufferSize: 3) })
			{
				var actual = fields.ToArray();

				Assert.Equal(expected.Select(f => (f.GetType(), f.Position, f.ValuePosition, f.Length, f.IsValid)),
					actual.Select(f => (f.GetType(), f.Position, f.ValuePosition, f.Length, f.IsValid)));
			}
		});
	}

	[Theory]
	[InlineData(1)]
	[InlineData(3)]
	[InlineData(17)]
	public void Direct_streams_return_all_fields_with_global_locations(int bufferSize)
	{
		var wire  = (string)FixFixtures.Messages().First()[1];
		var input = wire + wire;

		Each(parser =>
		{
			var expected = parser.Parse(input);

			using var reader = new StringReader(input);
			using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));

			var chars = parser.Parse(reader, bufferSize: bufferSize);
			var bytes = parser.Parse(stream, bufferSize: bufferSize);

			foreach (var actual in new[] { chars, bytes })
				Assert.Equal(expected.Select(f => (f.GetType(), f.Position, f.ValuePosition, f.Length)),
					actual.Select(f => (f.GetType(), f.Position, f.ValuePosition, f.Length)));

			Assert.True(stream.CanRead);
			Assert.Equal(wire.Length, expected[expected.Length / 2].Position);
		});
	}

	[Theory]
	[InlineData(0)]
	[InlineData(17)]
	[InlineData(4097)]
	public void Raw_data_crosses_buffers_and_preserves_delimiter_octets(int size)
	{
		var payload = new string(Enumerable.Range(0, size).Select(i => "|=\0\u00ff"[i % 4]).ToArray());
		var input   = "95=" + size + "|96=" + payload + "|55=END|";

		Each(parser =>
		{
			using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));

			var fields = parser.ParseLog(stream, bufferSize: 3).ToArray();

			Assert.Equal(size, FixFixtures.Typed<FixField.Integer>(FixTag.RawDataLength, fields[0]).Value);
			Assert.Equal(Encoding.Latin1.GetBytes(payload), FixFixtures.Typed<FixField.Data>(FixTag.RawData, fields[1]).Value.ToArray());
			Assert.Equal("END", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, fields[2]).Value);
		});
	}

	[Fact]
	public void Custom_fields_are_not_interpreted_as_binary_pairs()
	{
		Each(parser =>
		{
			var fields = parser.ParseLog("9000=3|9001=abc|");

			Assert.Equal(new[] { 9000, 9001 }, fields.Select(f => (int)f.Tag));
			Assert.All(fields, field => Assert.IsType<FixField.Invalid>(field));
			Assert.Contains(parser.ParseLog("9000=3|9001=a|b|"), field => field is FixField.Invalid);

			using var stream = new ShortStream(Encoding.Latin1.GetBytes("9000=3|9001=a|b|"));

			Assert.Contains(parser.ParseLog(stream, bufferSize: 1), field => field is FixField.Invalid);
		});
	}

	[Theory]
	[InlineData("95=3|55=X|96=abc|")]
	[InlineData("95=5|96=abc|")]
	[InlineData("96=abc|")]
	[InlineData("0=x|")]
	[InlineData("01=x|")]
	[InlineData("55abc|")]
	[InlineData("2147483648=x|")]
	[InlineData("95=2147483648|96=x|")]
	[InlineData("95=2147483647|96=x|")]
	[InlineData("95=1|2147483648=x|")]
	public void Malformed_field_syntax_returns_invalid_fields(string input)
	{
		Each(parser =>
		{
			Assert.Contains(parser.ParseLog(input), field => field is FixField.Invalid);

			using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));

			Assert.Contains(parser.ParseLog(stream, bufferSize: 1), field => field is FixField.Invalid);
		});
	}

	[Theory]
	[InlineData("96=abc|")]
	[InlineData("95=3|89=abc|")]
	[InlineData("95=1|96=a|96=b|")]
	[InlineData("95=-1|96=|")]
	[InlineData("95=+1|96=a|")]
	[InlineData("95=-0|96=|")]
	public void Incomplete_or_mismatched_pairs_return_invalid_fields_in_all_input_forms(string input)
	{
		Each(parser =>
		{
			Assert.Contains(parser.ParseLog(input), field => field is FixField.Invalid);

			using var reader = new StringReader(input);

			Assert.Contains(parser.ParseLog(reader, bufferSize: 1), field => field is FixField.Invalid);

			using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));

			Assert.Contains(parser.ParseLog(stream, bufferSize: 1), field => field is FixField.Invalid);
		});
	}

	[Theory]
	[InlineData("95=4|96=abc")]
	[InlineData("95=0|96=X")]
	[InlineData("95=1|96=ab")]
	[InlineData("95=1|96=a55=ABC")]
	public void Eof_does_not_relax_binary_length_or_intermediate_separators(string input)
	{
		Each(parser =>
		{
			Assert.Contains(parser.ParseLog(input), field => field is FixField.Invalid);
		});
	}

	[Theory]
	[InlineData("55=ABC")]
	[InlineData("9000=ABC")]
	[InlineData("95=0|96=")]
	[InlineData("95=3|96=a|b")]
	[InlineData("95=4|96=abc|")]
	public void Final_field_can_end_at_eof_without_losing_value_or_location(string last)
	{
		Each(parser =>
		{
			foreach (var separator in new[] { '|', '\u0001' })
			{
				var input    = ("55=FIRST|" + last).Replace('|', separator);
				var log      = separator == '|';
				var expected = log ? parser.ParseLog(input + separator) : parser.Parse(input + separator);

				Equal(expected, log ? parser.ParseLog(input) : parser.Parse(input));

				using var reader = new StringReader(input);
				using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));

				Equal(expected, log ? parser.ParseLog(reader, bufferSize: 1) : parser.Parse(reader, bufferSize: 1));
				Equal(expected, log ? parser.ParseLog(stream, bufferSize: 1) : parser.Parse(stream, bufferSize: 1));
			}
		});
	}

	[Fact]
	public void Streaming_pair_returns_its_data_without_reading_the_next_field()
	{
		const string pair = "95=3\u000196=a|b\u0001";

		Each(parser =>
		{
			using var stream = new ShortStream(Encoding.Latin1.GetBytes(pair + "55=X\u0001")) { ReadLimit = pair.Length };
			using var fields = parser.Parse(stream, bufferSize: 1).GetEnumerator();

			Assert.True(fields.MoveNext());
			Assert.Equal(3, FixFixtures.Typed<FixField.Integer>(FixTag.RawDataLength, fields.Current).Value);
			Assert.True(fields.MoveNext());
			Assert.Equal(new byte[] { 97, 124, 98 }, FixFixtures.Typed<FixField.Data>(FixTag.RawData, fields.Current).Value.ToArray());
			Assert.Equal(pair.Length, stream.ReadCount);
		});
	}

	[Fact]
	public void A_recovered_field_is_yielded_without_reading_the_next_field()
	{
		Each(parser =>
		{
			using var stream = new ShortStream(Encoding.ASCII.GetBytes("bad\u000155=X\u0001")) { ReadLimit = 4 };
			using var fields = parser.Parse(stream, bufferSize: 1).GetEnumerator();

			Assert.True(fields.MoveNext());
			Assert.Equal(new byte[] { 98, 97, 100 }, Assert.IsType<FixField.Invalid>(fields.Current).RawBytes.ToArray());
			Assert.Equal(4, stream.ReadCount);
		});
	}

	[Fact]
	public void Stream_enumeration_is_lazy_and_does_not_wait_for_the_next_field()
	{
		Each(parser =>
		{
			using var stream = new ShortStream(Encoding.ASCII.GetBytes("55=ABC\u000138=2\u0001")) { ReadLimit = 7 };

			var fields = parser.Parse(stream, bufferSize: 1);

			Assert.Equal(0, stream.ReadCount);

			using (var iterator = fields.GetEnumerator())
			{
				Assert.Equal(0, stream.ReadCount);
				Assert.True(iterator.MoveNext());
				Assert.Equal("ABC", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, iterator.Current).Value);
				Assert.Equal(7, stream.ReadCount);
			}

			Assert.True(stream.CanRead);
		});
	}

	[Fact]
	public void Reader_enumeration_is_lazy_and_stops_at_a_complete_field()
	{
		Each(parser =>
		{
			using var reader = new GatedReader("55=ABC\u0001");

			var fields = parser.Parse(reader, bufferSize: 1);

			Assert.Equal(0, reader.ReadCount);

			using var iterator = fields.GetEnumerator();

			Assert.True(iterator.MoveNext());
			Assert.Equal("ABC", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, iterator.Current).Value);
			Assert.Equal(7, reader.ReadCount);
		});
	}

	[Theory]
	[InlineData("55=A|broken|55=B|")]
	[InlineData("55=A|95=3|96=ab")]
	public void Errors_are_returned_during_enumeration(string wire)
	{
		Each(parser =>
		{
			using var stream = new ShortStream(Encoding.ASCII.GetBytes(wire));
			using var fields = parser.ParseLog(stream, bufferSize: 1).GetEnumerator();

			Assert.True(fields.MoveNext());
			FixFixtures.Typed<FixField.Text>(FixTag.Symbol, fields.Current);

			var invalid = false;

			while (fields.MoveNext())
				invalid |= fields.Current is FixField.Invalid;

			Assert.True(invalid);

			Assert.True(stream.CanRead);
		});
	}

	[Fact]
	public void Many_fields_release_input_and_keep_global_locations_and_owned_values()
	{
		var wire = string.Concat(Enumerable.Repeat("95=3|96=a|b|55=X|", 1000));

		Each(parser =>
		{
			using var stream = new ShortStream(Encoding.ASCII.GetBytes(wire));

			var fields = parser.ParseLog(stream, bufferSize: 3, maxRetained: 32).ToArray();

			Assert.Equal(3000, fields.Length);
			Assert.Equal(new byte[] { 97, 124, 98 }, FixFixtures.Typed<FixField.Data>(FixTag.RawData, fields[1]).Value.ToArray());
			Assert.Equal(wire.Length - 5, fields[^1].Position);
			Assert.True(stream.CanRead);
		});
	}

	[Fact]
	public void Recovery_returns_errors_in_order_with_original_input_and_absolute_positions()
	{
		const string input = "55=ABC|bad|38=2|0=X|55=END|tail";

		Each(parser =>
		{
			var text = parser.ParseLog(input);

			Assert.Equal(6, text.Length);

			using var reader = new StringReader(input);
			using var stream = new MemoryStream(Encoding.Latin1.GetBytes(input));

			Check(text, false);
			Check(parser.ParseLog(reader, 1, 16), false);
			Check(parser.ParseLog(stream, 1, 16), true);

			Assert.True(stream.CanRead);
		});

		static void Check(IEnumerable<FixField> source, bool bytes)
		{
			var fields  = source.ToArray();
			var invalid = fields.OfType<FixField.Invalid>().ToArray();

			Assert.Equal(new[] { 55, 0, 38, 0, 55, 0 }, fields.Select(field => (int)field.Tag));
			Assert.Equal("END", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, fields[4]).Value);
			Assert.Equal(new[] { 7, 16, 27 }, invalid.Select(field => field.Position));
			Assert.Equal(new[] { "bad", "0=X", "tail" }, invalid.Select(field => bytes ? Encoding.Latin1.GetString(field.RawBytes.Span) : field.RawText));
			Assert.All(invalid, field =>
			{
				Assert.False(field.IsValid);
				Assert.Equal(bytes, field.IsByteInput);
				Assert.Equal(field.Position, field.ValuePosition);
				Assert.Equal(bytes ? field.RawBytes.Length : field.RawText!.Length, field.Length);
				Assert.NotEmpty(field.Message);
			});
		}
	}

	[Fact]
	public void Raw_error_data_preserves_high_bytes_and_unicode_text()
	{
		Each(parser =>
		{
			var bytes = parser.ParseLog(new byte[] { 255, 0, 124, 53, 53, 61, 88 });

			Assert.Equal(new byte[] { 255, 0 }, Assert.IsType<FixField.Invalid>(bytes[0]).RawBytes.ToArray());
			Assert.Equal("X", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, bytes[1]).Value);

			var fields = parser.ParseLog("ошибка|55=X");

			Assert.Equal("ошибка", Assert.IsType<FixField.Invalid>(fields[0]).RawText);
		});
	}

	[Theory]
	[InlineData('\u0001')]
	[InlineData('|')]
	public void Location_covers_whole_fields_and_conversion_does_not_need_coordinates(char separator)
	{
		var wire = "1=arbitrary text|10=007|607=invalid|9001=vendor|".Replace('|', separator);
		var log  = separator == '|';

		Each(parser =>
		{
			using var reader = new StringReader(wire);
			using var stream = new MemoryStream(Latin1(wire));

			var direct = log ? parser.ParseLog(wire) : parser.Parse(wire);
			var chars  = (log ? parser.ParseLog(reader, bufferSize: 1) : parser.Parse(reader, bufferSize: 1)).ToArray();
			var bytes  = (log ? parser.ParseLog(stream, bufferSize: 1) : parser.Parse(stream, bufferSize: 1)).ToArray();

			foreach (var fields in new[] { direct, chars, bytes })
			{
				AssertExtents(wire, fields);
				Assert.Equal("arbitrary text", FixFixtures.Typed<FixField.Text>(FixTag.Account, fields[0]).Value);
				Assert.True(fields[0].IsValid);

				var invalid = FixFixtures.Typed<FixField.Integer>(FixTag.LegProduct, fields[2]);

				Assert.False(invalid.TryGetValue(out _));
				Assert.Throws<InvalidOperationException>(() => invalid.Value);
				var vendor = Assert.IsType<FixField.Invalid>(fields[3]);

				Assert.Equal(Latin1("vendor"), vendor.RawText is { } text ? Latin1(text) : vendor.RawBytes.ToArray());
			}
		});
	}

	[Theory]
	[InlineData(0)]
	[InlineData(15)]
	[InlineData(16)]
	[InlineData(17)]
	[InlineData(255)]
	[InlineData(256)]
	[InlineData(257)]
	[InlineData(4095)]
	[InlineData(4096)]
	[InlineData(4097)]
	[InlineData(65536)]
	public void Length_delimited_data_crosses_every_small_buffer_boundary(int length)
	{
		const string seed = "a|b\u0001=\0\u00ff10=000";

		var raw    = new string(Enumerable.Range(0, length).Select(i => seed[i % seed.Length]).ToArray());
		var body   = "35=A\u000149=SENDER\u000156=TARGET\u000134=1\u000152=20260915-12:00:00\u000198=0\u0001108=30\u000195=" + raw.Length + "\u000196=" + raw + "\u0001";
		var prefix = "8=FIX.4.4\u00019=" + body.Length + "\u0001" + body;
		var wire   = prefix + "10=" + (prefix.Sum(c => (int)c) & 255).ToString("000", CultureInfo.InvariantCulture) + "\u0001";
		var log    = Log(wire, FixParser.ParseMessage(wire));

		Each(parser =>
		{
			foreach (var capacity in new[] { 1, 3, 17, 4096 })
			{
				using var stream = new MemoryStream(Latin1(wire));
				using var reader = new StringReader(log);

				foreach (var (text, fields) in new[] { (wire, parser.Parse(stream, capacity).ToArray()), (log, parser.ParseLog(reader, capacity).ToArray()) })
				{
					Assert.DoesNotContain(fields, field => field is FixField.Invalid);
					AssertExtents(text, fields);
					Assert.Equal(Latin1(raw), FixFixtures.Typed<FixField.Data>(FixTag.RawData, fields.Single(f => f.Tag == FixTag.RawData)).Value.ToArray());
				}
			}
		});
	}

	/// <summary>
	/// A message's wire text with the separator after each field turned into '|'.
	/// </summary>
	internal static string Log(string wire, FixMessage message)
	{
		var text = wire.ToCharArray();

		foreach (var field in message.Fields)
			text[field.ValuePosition + field.Length] = '|';

		return new string(text);
	}

	static void AssertExtents(string wire, FixField[] fields)
	{
		var position = 0;

		foreach (var field in fields)
		{
			var prefix = ((int)field.Tag).ToString(CultureInfo.InvariantCulture) + "=";

			Assert.Equal(position, field.Position);
			Assert.Equal(prefix, wire.Substring(field.Position, prefix.Length));
			Assert.Equal(position + prefix.Length, field.ValuePosition);

			Assert.True(field.Length >= 0);

			position = field.ValuePosition + field.Length + 1;
		}

		Assert.Equal(wire.Length, position);
	}

	static void Equal(IEnumerable<FixField> expected, IEnumerable<FixField> actual)
	{
		Assert.Equal(expected.Select(Describe), actual.Select(Describe));
	}

	static string Describe(FixField field)
	{
		return field.GetType().Name + JsonSerializer.Serialize(field, field.GetType());
	}

	static byte[] Latin1(string text)
	{
		return text.Select(c => checked((byte)c)).ToArray();
	}

	sealed class GatedReader(string text) : StringReader(text)
	{
		readonly int _length = text.Length;

		public int ReadCount { get; private set; }

		public override int Read(char[] buffer, int index, int count)
		{
			if (ReadCount >= _length)
				throw new IOException("The next field has not arrived.");

			var read = base.Read(buffer, index, Math.Min(count, 1));

			ReadCount += read;
			return read;
		}
	}

	/// <summary>
	/// A stream that gives at most two bytes a read and fails past its limit, as a connection
	/// does when the next field has not arrived.
	/// </summary>
	sealed class ShortStream(byte[] data) : Stream
	{
		readonly MemoryStream _inner = new(data);

		public int ReadLimit { get; set; } = int.MaxValue;
		public int ReadCount { get; private set; }

		public override bool CanRead  => _inner.CanRead;
		public override bool CanSeek  => false;
		public override bool CanWrite => false;
		public override long Length   => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (ReadCount >= ReadLimit)
				throw new IOException("The next field has not arrived.");

			var read = _inner.Read(buffer, offset, Math.Min(count, Math.Min(2, ReadLimit - ReadCount)));

			ReadCount += read;
			return read;
		}

		public override void Flush()
		{
			throw new NotSupportedException();
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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_inner.Dispose();

			base.Dispose(disposing);
		}
	}
}
