using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

using DotGram.Finance.Fix;

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
		foreach (var message in new[] { FixParser.ReadMessage(bytes), FixParser.ParseMessage(log, FixFieldOptions.Log), FixParser.ReadMessage(logBytes, FixFieldOptions.Log) })
		{
			Assert.Equal(name, message.GetType().Name);
			Assert.Equal(expected.AllFields.Select(f => f.Value.ToString()), message.AllFields.Select(f => f.Value.ToString()));
			Assert.Equal(expected.AllFields.Select(f => f.TypedValue!.GetType()), message.AllFields.Select(f => f.TypedValue!.GetType()));
			Assert.All(message.AllFields, f => Assert.True(f.TypedValue!.IsValid, $"Tag {f.Tag}"));
		}
	}

	[Fact]
	public void Numeric_boolean_calendar_and_binary_fields_have_native_values()
	{
		var orderWire = FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		using var stream = new MemoryStream(Bytes(orderWire));
		foreach (var order in new[] { FixParser.ParseMessage(orderWire), FixParser.ReadMessage(stream) })
		{
			Assert.Equal("ABC", Assert.IsType<FixField.Symbol>(order.GetField(55)!.Value.TypedValue).Value);
			Assert.Equal('1', Assert.IsType<FixField.Side>(order.GetField(54)!.Value.TypedValue).Value);
			var price = Assert.IsType<FixField.Price>(order.GetField(44)!.Value.TypedValue).Value;
			Assert.Equal(12.50m, price);
			var time = Assert.IsType<FixField.TransactTime>(order.GetField(60)!.Value.TypedValue).Value;
			Assert.Equal(2026, time.Date.Year);
			Assert.Equal(12, time.Time.Hour);
			Assert.Equal(BigInteger.One, Assert.IsType<FixField.MsgSeqNum>(order.Header.GetField(34)!.Value.TypedValue).Value);
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
		Assert.Throws<FormatException>(() => FixParser.ParseMessage(log.Replace("TEST", "FAIL"), FixFieldOptions.Log));
	}

	static byte[] Bytes(string text)
	{
		return text.Select(c => checked((byte)c)).ToArray();
	}
}
