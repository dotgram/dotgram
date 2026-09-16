using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixDispatchTests
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
		Equal(expected, FixDispatch.Parse(wire));
		using var reader = new StringReader(wire);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(wire));
		Equal(expected, FixDispatch.Parse(reader, bufferSize: 3));
		Equal(expected, FixDispatch.Parse(stream, bufferSize: 3));
		Equal(expected, FixDispatch.Parse(Encoding.Latin1.GetBytes(wire)));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(17)]
	[InlineData(4097)]
	public void Configured_binary_pairs_preserve_raw_bytes_and_global_locations(int size)
	{
		var pairs = new Dictionary<int, int> { [5000] = 5001 };
		var options = new FixDispatchOptions('|', pairs);
		pairs.Clear(); // Configuration owns a snapshot.
		var payload = new string(Enumerable.Range(0, size).Select(i => "|=\0ÿ"[i % 4]).ToArray());
		var input = "55=ABC|5000=" + size + "|5001=" + payload + "|55=END|";
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));
		var fields = FixDispatch.Parse(stream, options, bufferSize: 3).ToArray();
		Assert.Equal(new[] { 55, 5001, 55 }, fields.Select(field => field.Tag));
		var binary = Assert.IsType<FixField.Unknown>(fields[1]);
		Assert.Equal(Encoding.Latin1.GetBytes(payload), binary.Value.ToArray());
		Assert.Equal(7, binary.Position);
		Assert.Equal(input.IndexOf("5001=", StringComparison.Ordinal) + 5, binary.ValuePosition);
		Assert.Equal(size, binary.Length);
		Assert.True(stream.CanRead);
		Equal(fields, FixDispatch.Parse(input, options));
		Assert.Throws<FormatException>(() => FixDispatch.Parse("5000=1|5002=X|", options));
		Assert.Throws<FormatException>(() => FixDispatch.Parse("5001=X|", options));
	}

	[Theory]
	[InlineData("95=3|55=X|96=abc|")]
	[InlineData("95=5|96=abc|")]
	[InlineData("96=abc|")]
	[InlineData("0=x|")]
	[InlineData("01=x|")]
	[InlineData("2147483648=x|")]
	[InlineData("95=2147483648|96=x|")]
	public void Malformed_inputs_are_rejected_by_both_parsers(string wire)
	{
		Assert.Throws<FormatException>(() => Fix44.ParseLog(wire));
		Assert.Throws<FormatException>(() => FixDispatch.ParseLog(wire));
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(wire));
		Assert.Throws<FormatException>(() => FixDispatch.Parse(stream, new FixDispatchOptions('|'), bufferSize: 1).ToArray());
	}

	[Fact]
	public void Independent_enumerations_own_their_binary_state_and_leave_input_open()
	{
		using var input = new ShortStream(Encoding.Latin1.GetBytes("95=3|96=a|b|55=END|"));
		using var reader = new StringReader("95=1|96=X|55=NEXT|");
		using var first = FixDispatch.Parse(input, new FixDispatchOptions('|'), bufferSize: 1).GetEnumerator();
		using var second = FixDispatch.Parse(reader, new FixDispatchOptions('|'), bufferSize: 1).GetEnumerator();
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
			var original = new FixFieldOptions(separator);
			var dispatch = new FixDispatchOptions(separator);
			var expected = Fix44.Parse(input + separator, original);
			Equal(expected, Fix44.Parse(input, original));
			Equal(expected, FixDispatch.Parse(input, dispatch));
			using var oldReader = new StringReader(input);
			using var newReader = new StringReader(input);
			using var oldStream = new ShortStream(Encoding.Latin1.GetBytes(input));
			using var newStream = new ShortStream(Encoding.Latin1.GetBytes(input));
			Equal(expected, Fix44.Parse(oldReader, original, bufferSize: 1));
			Equal(expected, FixDispatch.Parse(newReader, dispatch, bufferSize: 1));
			Equal(expected, Fix44.Parse(oldStream, original, bufferSize: 1));
			Equal(expected, FixDispatch.Parse(newStream, dispatch, bufferSize: 1));
		}
	}

	[Theory]
	[InlineData("95=4|96=abc")]
	[InlineData("95=0|96=X")]
	[InlineData("95=1|96=ab")]
	[InlineData("95=1|96=a55=ABC")]
	public void Eof_does_not_relax_binary_length_or_intermediate_separators(string input)
	{
		Assert.False(Fix44.TryParse(input, out _, out _, new FixFieldOptions('|')));
		Assert.False(FixDispatch.TryParse(input, out _, out _, new FixDispatchOptions('|')));
	}

	sealed class ShortStream(byte[] input) : MemoryStream(input)
	{
		public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, Math.Min(count, 1));
	}
}
