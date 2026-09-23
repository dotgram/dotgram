using System;
using System.Globalization;
using System.IO;
using System.Linq;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixFieldGrammarTests
{
	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void All_fields_have_the_same_ADT_in_char_byte_and_pipe_forms(string name, string wire)
	{
		var expected = FixParser.ParseMessage(wire);
		var log = FixFieldReaderTests.Log(wire, expected);
		using var bytes = new MemoryStream(Bytes(wire));
		using var logBytes = new MemoryStream(Bytes(log));
		foreach (var message in new[] { FixParser.ReadMessage(bytes), FixParser.ParseMessage(log, FixContext.WithLogFraming), FixParser.ReadMessage(logBytes, FixContext.WithLogFraming) })
		{
			Assert.Equal(name, message.GetType().Name);
			Assert.Equal(expected.Fields.Select(f => (f.Tag, f.GetType())), message.Fields.Select(f => (f.Tag, f.GetType())));
			Assert.All(message.Fields, f => Assert.True(f.IsValid, $"Tag {f.Tag}"));
		}
	}

	[Fact]
	public void Numeric_boolean_calendar_and_binary_fields_have_native_values()
	{
		var orderWire = FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		using var stream = new MemoryStream(Bytes(orderWire));
		foreach (var order in new[] { FixParser.ParseMessage(orderWire), FixParser.ReadMessage(stream) })
		{
			Assert.Equal("ABC", Assert.IsType<FixField.Symbol>(Field(order, 55)).Value);
			Assert.Equal('1', Assert.IsType<FixField.Side>(Field(order, 54)).Value);
			var price = Assert.IsType<FixField.Price>(Field(order, 44)).Value;
			Assert.Equal(12.50m, price);
			var time = Assert.IsType<FixField.TransactTime>(Field(order, 60)).Value;
			Assert.Equal(2026, time.Year);
			Assert.Equal(12, time.Hour);
			Assert.Equal(TimeSpan.Zero, time.Offset);
			Assert.Equal(1L, Assert.IsType<FixField.MsgSeqNum>(Field(order, 34)).Value);
		}
		Assert.True(FixConvert.Boolean("Y".AsSpan(), out var flag));
		Assert.True(flag);
		Assert.True(FixConvert.Boolean(new byte[] { (byte)'N' }, out flag));
		Assert.False(flag);
	}

	[Theory]
	[InlineData("-79228162514264337593543950335")]
	[InlineData(".5")]
	[InlineData("-.5")]
	[InlineData("0.0000000000000000000000000001")]
	[InlineData("1.")]
	public void Decimal_conversion_is_exact_and_equal_for_both_domains(string text)
	{
		Assert.True(FixConvert.Decimal(text.AsSpan(), out var chars));
		Assert.True(FixConvert.Decimal(Bytes(text), out var bytes));
		Assert.Equal(chars, bytes);
	}

	[Fact]
	public void Log_checksum_still_detects_damage()
	{
		var wire = FixFixtures.Wire("0", "112=TEST|");
		var log = FixFieldReaderTests.Log(wire, FixParser.ParseMessage(wire));
		Assert.Throws<FormatException>(() => FixParser.ParseMessage(log.Replace("TEST", "FAIL"), FixContext.WithLogFraming));
	}

	static FixField Field(FixMessage message, int tag)
	{
		return message.Fields.First(field => field.Tag == tag);
	}

	static byte[] Bytes(string text)
	{
		return text.Select(c => checked((byte)c)).ToArray();
	}
}
