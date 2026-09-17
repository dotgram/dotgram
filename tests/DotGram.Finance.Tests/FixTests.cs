using System;
using System.Text;
using System.Text.Json;

using DotGram.Examples.Finance;
using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixTests
{
	static string Describe(FixField field) => field.GetType().Name + JsonSerializer.Serialize(field, field.GetType());
	static void Equal(IEnumerable<FixField> expected, IEnumerable<FixField> actual) =>
		Assert.Equal(expected.Select(Describe), actual.Select(Describe));

	[Theory]
	[MemberData(nameof(Fix44Tests.Messages), MemberType = typeof(Fix44Tests))]
	public void All_fixtures_match_types_values_and_locations_on_all_inputs(string name, string wire)
	{
		Assert.NotEmpty(name);
		var expected = Fix44.Parse(wire);
		Equal(expected, FixParser.Parse(wire));
		using var reader = new StringReader(wire);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(wire));
		Equal(expected, FixParser.Parse(reader, bufferSize: 3));
		Equal(expected, FixParser.Parse(stream, bufferSize: 3));
		Equal(expected, FixParser.Parse(Encoding.Latin1.GetBytes(wire)));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(17)]
	[InlineData(4097)]
	public void Configured_binary_pairs_preserve_raw_bytes_and_global_locations(int size)
	{
		var pairs = new Dictionary<int, int> { [5000] = 5001 };
		var options = new FixOptions('|', pairs);
		pairs.Clear(); // Configuration owns a snapshot.
		var payload = new string(Enumerable.Range(0, size).Select(i => "|=\0ÿ"[i % 4]).ToArray());
		var input = "55=ABC|5000=" + size + "|5001=" + payload + "|55=END|";
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));
		var fields = FixParser.Parse(stream, options, bufferSize: 3).ToArray();
		Assert.Equal(new[] { 55, 5001, 55 }, fields.Select(field => field.Tag));
		var binary = Assert.IsType<FixField.Unknown>(fields[1]);
		Assert.Equal(Encoding.Latin1.GetBytes(payload), binary.Value.ToArray());
		Assert.Equal(7, binary.Position);
		Assert.Equal(input.IndexOf("5001=", StringComparison.Ordinal) + 5, binary.ValuePosition);
		Assert.Equal(size, binary.Length);
		Assert.True(stream.CanRead);
		Equal(fields, FixParser.Parse(input, options));
		Assert.Contains(FixParser.Parse("5000=1|5002=X|", options), field => field is FixField.Invalid);
		Assert.Contains(FixParser.Parse("5001=X|", options), field => field is FixField.Invalid);
	}

	[Theory]
	[InlineData("95=3|55=X|96=abc|")]
	[InlineData("95=5|96=abc|")]
	[InlineData("96=abc|")]
	[InlineData("0=x|")]
	[InlineData("01=x|")]
	[InlineData("2147483648=x|")]
	[InlineData("95=2147483648|96=x|")]
	public void Malformed_inputs_return_invalid_fields_in_both_parsers(string wire)
	{
		Assert.Contains(Fix44.ParseLog(wire), field => field is FixField.Invalid);
		Assert.Contains(FixParser.ParseLog(wire), field => field is FixField.Invalid);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(wire));
		Assert.Contains(FixParser.Parse(stream, new FixOptions('|'), bufferSize: 1), field => field is FixField.Invalid);
	}

	[Fact]
	public void Independent_enumerations_own_their_binary_state_and_leave_input_open()
	{
		using var input = new ShortStream(Encoding.Latin1.GetBytes("95=3|96=a|b|55=END|"));
		using var reader = new StringReader("95=1|96=X|55=NEXT|");
		using var first = FixParser.Parse(input, new FixOptions('|'), bufferSize: 1).GetEnumerator();
		using var second = FixParser.Parse(reader, new FixOptions('|'), bufferSize: 1).GetEnumerator();
		Assert.True(first.MoveNext());
		Assert.True(second.MoveNext());
		Assert.Equal("a|b", Encoding.Latin1.GetString(Assert.IsType<FixField.RawData>(first.Current).Value.Span));
		Assert.True(first.MoveNext());
		Assert.Equal("END", Assert.IsType<FixField.Symbol>(first.Current).Value);
		second.Dispose();
		Assert.NotEqual(-1, reader.Read());
		first.Dispose();
		Assert.True(input.CanRead);
	}

	[Theory]
	[InlineData("55=ABC")]
	[InlineData("9000=ABC")]
	[InlineData("95=0|96=")]
	[InlineData("95=3|96=a|b")]
	[InlineData("95=4|96=abc|")]
	public void Final_field_can_end_at_eof_without_losing_value_or_location(string last)
	{
		foreach (var separator in new[] { '|', '\u0001' })
		{
			var input = ("55=FIRST|" + last).Replace('|', separator);
			var original = new FixOptions(separator);
			var dispatch = new FixOptions(separator);
			var expected = Fix44.Parse(input + separator, original);
			Equal(expected, Fix44.Parse(input, original));
			Equal(expected, FixParser.Parse(input, dispatch));
			using var oldReader = new StringReader(input);
			using var newReader = new StringReader(input);
			using var oldStream = new ShortStream(Encoding.Latin1.GetBytes(input));
			using var newStream = new ShortStream(Encoding.Latin1.GetBytes(input));
			Equal(expected, Fix44.Parse(oldReader, original, bufferSize: 1));
			Equal(expected, FixParser.Parse(newReader, dispatch, bufferSize: 1));
			Equal(expected, Fix44.Parse(oldStream, original, bufferSize: 1));
			Equal(expected, FixParser.Parse(newStream, dispatch, bufferSize: 1));
		}
	}

	[Theory]
	[InlineData("95=4|96=abc")]
	[InlineData("95=0|96=X")]
	[InlineData("95=1|96=ab")]
	[InlineData("95=1|96=a55=ABC")]
	public void Eof_does_not_relax_binary_length_or_intermediate_separators(string input)
	{
		Assert.Contains(Fix44.ParseLog(input), field => field is FixField.Invalid);
		Assert.Contains(FixParser.ParseLog(input), field => field is FixField.Invalid);
	}

	[Fact]
	public void Largest_tags_keep_text_and_binary_meanings_on_all_inputs()
	{
		var text = "2147483647=X";
		Assert.Equal(int.MaxValue, Assert.Single(Fix.Parse(text)).Tag);
		var options = new FixOptions('|', new Dictionary<int, int> { [int.MaxValue - 1] = int.MaxValue });
		var wire = "2147483646=3|2147483647=a|b|55=END";
		var expected = Fix.Parse(wire, options);
		var binary = Assert.IsType<FixField.Unknown>(expected[0]);
		Assert.Equal(int.MaxValue, binary.Tag);
		Assert.Equal("a|b", Encoding.Latin1.GetString(binary.Value.Span));
		Assert.Equal(0, binary.Position);
		Assert.Equal(wire.IndexOf("a|b", StringComparison.Ordinal), binary.ValuePosition);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(wire));
		using var reader = new StringReader(wire);
		Equal(expected, Fix.Parse(stream, options, bufferSize: 1));
		Equal(expected, Fix.Parse(reader, options, bufferSize: 1));
	}

	sealed class ShortStream(byte[] input) : MemoryStream(input)
	{
		public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, Math.Min(count, 1));
	}
}
