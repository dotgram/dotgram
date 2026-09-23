using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix44;
using DotGram.Handwritten.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// A field keeps its two header lengths in narrow fields and sets a length too wide for them
/// aside. Ordinary input never reaches that table, and input that does reports the same
/// extents as any other, through every overload of both parsers.
/// </summary>
public sealed class FixFieldHeaderTests
{
	const char Soh = (char)1;

	public static TheoryData<string, bool> Ordinary()
	{
		var order  = FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		var groups = FixFixtures.Wire("D", "11=ORDER|453=2|448=P1|447=D|452=1|802=2|523=S1|803=1|523=S2|803=2|448=P2|447=D|452=3|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|");
		var binary = "95=5" + Soh + "96=a" + Soh + "b|c" + Soh + "58=x" + Soh;

		return new TheoryData<string, bool>
		{
			{ order, false },
			{ string.Concat(Enumerable.Repeat(order, 128)), false },
			{ groups, false },
			{ binary, false },
			{ order.Replace(Soh, '|'), true },
			{ "55=T 1 1/8" + "  |   " + "38=2 | 95=3 | 96=a|b | 58=hello world |", true },
		};
	}

	[Theory]
	[MemberData(nameof(Ordinary))]
	public void Ordinary_input_sets_no_length_aside(string input, bool log)
	{
		foreach (var fields in Parse(input, log))
		{
			Assert.NotEmpty(fields);
			Assert.All(fields, field => Assert.False(field.HasWideLengths));
		}
	}

	[Theory]
	[InlineData(65530)]
	[InlineData(65535)]
	[InlineData(70000)]
	public void A_data_length_with_many_leading_zeros_keeps_its_extent(int zeros)
	{
		var input = "95=" + new string('0', zeros) + "5" + Soh + "96=hello" + Soh + "58=x" + Soh;

		foreach (var fields in Parse(input, false))
		{
			var data = Assert.IsType<FixField.RawData>(fields[0]);

			Assert.Equal(0, data.Position);
			Assert.Equal(input.IndexOf("hello", StringComparison.Ordinal), data.ValuePosition);
			Assert.Equal(5, data.Length);
			Assert.Equal(input.IndexOf("96=", StringComparison.Ordinal), data.DataPosition);
			Assert.True(data.IsBinary);
			Assert.Equal(input.IndexOf("58=", StringComparison.Ordinal), fields[1].Position);
			Assert.Equal(data.ValuePosition - data.Position >= ushort.MaxValue, data.HasWideLengths);
		}
	}

	[Theory]
	[InlineData(126)]
	[InlineData(127)]
	[InlineData(300)]
	public void A_log_separator_padded_past_a_byte_keeps_its_extent(int spaces)
	{
		var padding = new string(' ', spaces);
		var input   = "8=FIX.4.4" + padding + "|" + padding + "35=D|";

		foreach (var fields in Parse(input, true))
		{
			var first = fields[0];

			Assert.Equal(0, first.Position);
			Assert.Equal(2, first.ValuePosition);
			Assert.Equal("FIX.4.4".Length, first.Length);
			Assert.Equal(input.IndexOf("35=", StringComparison.Ordinal), fields[1].Position);
			Assert.Equal(2 * spaces + 1 >= byte.MaxValue, first.HasWideLengths);

			// Locating again uses the terminator it was given, however wide.
			first.Locate(first.Position, fields[1].Position - first.Position);

			Assert.Equal("FIX.4.4".Length, first.Length);
		}
	}

	static IEnumerable<FixField[]> Parse(string input, bool log)
	{
		var bytes = Encoding.Latin1.GetBytes(input);

		if (log)
		{
			yield return FixParser.ParseFields(input, FixContext.WithLogFraming);
			yield return FixParser.ParseFields(input.AsSpan(), FixContext.WithLogFraming);
			yield return FixParser.ParseFields(bytes, FixContext.WithLogFraming);
			yield return FixParser.ReadFields(new StringReader(input), FixContext.WithLogFraming, bufferSize: 7).ToArray();
			yield return FixParser.ReadFields(new MemoryStream(bytes), FixContext.WithLogFraming, bufferSize: 7).ToArray();
			yield return HandFixParser.ParseLog(input);
			yield return HandFixParser.ParseLog(bytes);
			yield return HandFixParser.ParseLog(new StringReader(input), bufferSize: 7).ToArray();
			yield return HandFixParser.ParseLog(new MemoryStream(bytes), bufferSize: 7).ToArray();
		}
		else
		{
			yield return FixParser.ParseFields(input);
			yield return FixParser.ParseFields(input.AsSpan());
			yield return FixParser.ParseFields(bytes);
			yield return FixParser.ReadFields(new StringReader(input), bufferSize: 7).ToArray();
			yield return FixParser.ReadFields(new MemoryStream(bytes), bufferSize: 7).ToArray();
			yield return HandFixParser.Parse(input);
			yield return HandFixParser.Parse(bytes);
			yield return HandFixParser.Parse(new StringReader(input), bufferSize: 7).ToArray();
			yield return HandFixParser.Parse(new MemoryStream(bytes), bufferSize: 7).ToArray();
		}
	}
}
