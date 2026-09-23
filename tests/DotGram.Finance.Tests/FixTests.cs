using System;
using System.Text;
using System.Text.Json;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixTests
{
	static string Describe(FixField field)
	{
		return field.GetType().Name + JsonSerializer.Serialize(field, field.GetType());
	}

	static void Equal(IEnumerable<FixField> expected, IEnumerable<FixField> actual)
	{
		Assert.Equal(expected.Select(Describe), actual.Select(Describe));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(17)]
	[InlineData(4097)]
	public void Configured_binary_pairs_preserve_raw_bytes_and_global_locations(int size)
	{
		var pairs = new Dictionary<int, int> { [5000] = 5001 };
		var options = new FixContext { LengthDataPairs = pairs };
		pairs.Clear(); // Configuration owns a snapshot.
		var payload = new string(Enumerable.Range(0, size).Select(i => "|=\0ÿ"[i % 4]).ToArray());
		var input = "55=ABC|5000=" + size + "|5001=" + payload + "|55=END|";
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));
		var fields = FixParser.ReadFields(stream, (options ?? new FixContext()) with { Framing = FixFraming.Log, BufferSize = 3 }).ToArray();
		Assert.Equal(new[] { 55, 5000, 5001, 55 }, fields.Select(field => field.Tag));
		var binary = Assert.IsType<FixField.Invalid>(fields[2]);
		Assert.Equal(Encoding.Latin1.GetBytes(payload), binary.RawBytes.ToArray());
		Assert.Equal(input.IndexOf("5001=", StringComparison.Ordinal), binary.Position);
		Assert.Equal(input.IndexOf("5001=", StringComparison.Ordinal) + 5, binary.ValuePosition);
		Assert.Equal(size, binary.Length);
		Assert.True(stream.CanRead);
		Equal(fields, FixParser.ParseFields(input, (options ?? new FixContext()) with { Framing = FixFraming.Log }));
		Assert.Contains(FixParser.ParseFields("5000=1|5002=X|", (options ?? new FixContext()) with { Framing = FixFraming.Log }), field => field is FixField.Invalid);
		Assert.Contains(FixParser.ParseFields("5001=X|", (options ?? new FixContext()) with { Framing = FixFraming.Log }), field => field is FixField.Invalid);
	}

	[Theory]
	[InlineData("2147483647=X|", int.MaxValue)]
	[InlineData("95=0003|96=a|b|", 96)]
	[InlineData("95=0000|96=", 96)]
	public void Typed_numbers_preserve_integer_boundaries_and_zero_padded_lengths(string input, int tag)
	{
		var expected = FixParser.ParseFields(input, FixContext.WithLogFraming);

		Assert.Equal(tag, expected[^1].Tag);
		Assert.DoesNotContain(expected, field => field is FixField.Invalid { Tag: 0 });

		using var reader = new StringReader(input);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));

		Equal(expected, FixParser.ReadFields(reader, FixFixtures.Reading(true, 1)));
		Equal(expected, FixParser.ReadFields(stream, FixFixtures.Reading(true, 1)));
	}

	[Fact]
	public void Independent_enumerations_own_their_binary_state_and_leave_input_open()
	{
		using var input = new ShortStream(Encoding.Latin1.GetBytes("95=3|96=a|b|55=END|"));
		using var reader = new StringReader("95=1|96=X|55=NEXT|");
		using var first = FixParser.ReadFields(input, FixFixtures.Reading(true, 1)).GetEnumerator();
		using var second = FixParser.ReadFields(reader, FixFixtures.Reading(true, 1)).GetEnumerator();
		Assert.True(first.MoveNext());
		Assert.True(second.MoveNext());
		Assert.Equal(3, Assert.IsType<FixField.RawDataLength>(first.Current).Value);
		Assert.Equal(1, Assert.IsType<FixField.RawDataLength>(second.Current).Value);
		Assert.True(first.MoveNext());
		Assert.Equal("a|b", Encoding.Latin1.GetString(Assert.IsType<FixField.RawData>(first.Current).Value.Span));
		Assert.True(first.MoveNext());
		Assert.Equal("END", Assert.IsType<FixField.Symbol>(first.Current).Value);
		second.Dispose();
		Assert.NotEqual(-1, reader.Read());
		first.Dispose();
		Assert.True(input.CanRead);
	}

	[Fact]
	public void Largest_tags_keep_text_and_binary_meanings_on_all_inputs()
	{
		var text = "2147483647=X";
		Assert.Equal(int.MaxValue, Assert.Single(FixParser.ParseFields(text)).Tag);
		var options = new FixContext { LengthDataPairs = new Dictionary<int, int> { [int.MaxValue - 1] = int.MaxValue } };
		var wire = "2147483646=3|2147483647=a|b|55=END";
		var expected = FixParser.ParseFields(wire, (options ?? new FixContext()) with { Framing = FixFraming.Log });
		Assert.Equal(int.MaxValue - 1, expected[0].Tag);
		var binary = Assert.IsType<FixField.Invalid>(expected[1]);
		Assert.Equal(int.MaxValue, binary.Tag);
		Assert.Equal("a|b", Encoding.Latin1.GetString(binary.RawBytes.Span));
		Assert.Equal(wire.IndexOf("2147483647=", StringComparison.Ordinal), binary.Position);
		Assert.Equal(wire.IndexOf("a|b", StringComparison.Ordinal), binary.ValuePosition);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(wire));
		using var reader = new StringReader(wire);
		Equal(expected, FixParser.ReadFields(stream, (options ?? new FixContext()) with { Framing = FixFraming.Log, BufferSize = 1 }));
		Equal(expected, FixParser.ReadFields(reader, (options ?? new FixContext()) with { Framing = FixFraming.Log, BufferSize = 1 }));
	}

	sealed class ShortStream(byte[] input) : MemoryStream(input)
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return base.Read(buffer, offset, Math.Min(count, 1));
		}
	}
}
