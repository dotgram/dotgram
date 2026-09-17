using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

using DotGram.Examples.Finance;
using DotGram.Finance;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixFieldGrammarTests
{
	[Theory]
	[MemberData(nameof(Fix44Tests.Messages), MemberType = typeof(Fix44Tests))]
	public void All_fields_have_the_same_ADT_in_char_byte_and_pipe_forms(string name, string wire)
	{
		var expected = FixMessages.Parse(wire);
		var log = Log(expected);
		using var bytes = new MemoryStream(Bytes(wire));
		using var logBytes = new MemoryStream(Bytes(log));
		foreach (var message in new[] { FixMessages.Parse(bytes), FixMessages.ParseLog(log), FixMessages.Parse(logBytes, new FixParseOptions('|')) })
		{
			Assert.Equal(name, message.GetType().Name);
			Assert.Equal(expected.AllFields.Select(f => f.Value.ToString()), message.AllFields.Select(f => f.Value.ToString()));
			Assert.Equal(expected.AllFields.Select(f => f.TypedValue!.GetType()), message.AllFields.Select(f => f.TypedValue!.GetType()));
			Assert.All(message.AllFields, f => Assert.True(f.TypedValue!.IsValid, $"Tag {f.Tag}"));
		}
	}

	[Theory]
	[InlineData('\u0001')]
	[InlineData('|')]
	public void Location_covers_whole_fields_and_conversion_does_not_need_coordinates(char separator)
	{
		var wire = "1=arbitrary text|10=007|607=invalid|9001=vendor|".Replace('|', separator);
		var context = new Fix44Context();
		var direct = separator == '|' ? Fix44Grammar.ParseLogFields(wire, context) : Fix44Grammar.ParseFields(wire, context);
		using var reader = new StringReader(wire);
		var chars = separator == '|' ? Fix44Grammar.ParseLogFields(reader, context, bufferSize: 1) : Fix44Grammar.ParseFields(reader, context, bufferSize: 1);
		using var stream = new MemoryStream(Bytes(wire));
		var byteContext = new Fix44Context();
		var bytes = separator == '|' ? Fix44Grammar.ParseLogFields(stream, byteContext, bufferSize: 1) : Fix44Grammar.ParseFields(stream, byteContext, bufferSize: 1);
		foreach (var fields in new[] { direct, chars, bytes })
		{
			AssertExtents(wire, fields);
			Assert.Equal("arbitrary text", Assert.IsType<FixField.Account>(fields[0]).Value);
			Assert.True(fields[0].IsValid);
			var invalid = Assert.IsType<FixField.LegProduct>(fields[2]);
			Assert.False(invalid.TryGetValue(out _));
			Assert.Throws<InvalidOperationException>(() => invalid.Value);
			Assert.Equal(Bytes("vendor"), Assert.IsType<FixField.Unknown>(fields[3]).Value.ToArray());
		}
	}

	static void AssertExtents(string wire, FixField[] fields)
	{
		var position = 0;
		foreach (var field in fields)
		{
			var prefix = field.Tag.ToString(CultureInfo.InvariantCulture) + "=";
			Assert.Equal(position, field.Position);
			Assert.Equal(prefix, wire.Substring(field.DataPosition, prefix.Length));
			if (field.IsBinary)
			{
				var lengthEnd = wire.IndexOf(wire[field.ValuePosition + field.Length], position);
				Assert.Equal(lengthEnd + 1, field.DataPosition);
			}
			else Assert.Equal(position + prefix.Length, field.ValuePosition);
			Assert.True(field.Length >= 0);
			position = field.ValuePosition + field.Length + 1;
		}
		Assert.Equal(wire.Length, position);
	}

	[Fact]
	public void Numeric_boolean_calendar_and_binary_fields_have_native_values()
	{
		var orderWire = Fix44Tests.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		using var stream = new MemoryStream(Bytes(orderWire));
		foreach (var order in new[] { FixMessages.Parse(orderWire), FixMessages.Parse(stream) })
		{
			Assert.Equal("ABC", Assert.IsType<FixField.Symbol>(order.GetField(55)!.Value.TypedValue).Value);
			Assert.Equal('1', Assert.IsType<FixField.Side>(order.GetField(54)!.Value.TypedValue).Value);
			var price = Assert.IsType<FixField.Price>(order.GetField(44)!.Value.TypedValue).Value;
			Assert.Equal(new BigInteger(1250), price.Coefficient);
			Assert.Equal(2, price.Scale);
			Assert.True(price.TryGetDecimal(out var money));
			Assert.Equal(12.50m, money);
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
		var raw = new string(Enumerable.Range(0, length).Select(i => seed[i % seed.Length]).ToArray());
		var body = "35=A\u000149=SENDER\u000156=TARGET\u000134=1\u000152=20260915-12:00:00\u000198=0\u0001108=30\u000195=" + raw.Length + "\u000196=" + raw + "\u0001";
		var prefix = "8=FIX.4.4\u00019=" + body.Length + "\u0001" + body;
		var wire = prefix + "10=" + (prefix.Sum(c => (int)c) & 255).ToString("000", CultureInfo.InvariantCulture) + "\u0001";
		var original = FixMessages.Parse(wire);
		var log = Log(original);
		foreach (var capacity in new[] { 1, 3, 17, 4096 })
		{
			using var stream = new MemoryStream(Bytes(wire));
			var result = Fix44Grammar.TryParseFields(stream, new Fix44Context(), bufferSize: capacity);
			Assert.True(result.IsSuccess, result.Error);
			AssertExtents(wire, result.Value);
			Assert.Equal(Bytes(raw), Assert.IsType<FixField.RawData>(result.Value.Single(f => f.Tag == 96)).Value.ToArray());
			using var reader = new StringReader(log);
			var textResult = Fix44Grammar.TryParseLogFields(reader, new Fix44Context(), bufferSize: capacity);
			Assert.True(textResult.IsSuccess, textResult.Error);
			AssertExtents(log, textResult.Value);
			Assert.Equal(Bytes(raw), Assert.IsType<FixField.RawData>(textResult.Value.Single(f => f.Tag == 96)).Value.ToArray());
		}
		Assert.Equal(raw, FixMessages.ParseLog(log).GetField(96)!.Value.ToString());
	}

	[Theory]
	[InlineData("-9999999999999999999999999999999999999999.000001")]
	[InlineData(".5")]
	[InlineData("-.5")]
	[InlineData("0.0000000000000000000000000000000000000001")]
	[InlineData("1.")]
	public void Decimal_conversion_is_exact_and_equal_for_both_domains(string text)
	{
		Assert.True(FixConvert.Decimal(text.AsSpan(), out var chars));
		Assert.True(FixConvert.Decimal(Bytes(text), out var bytes));
		Assert.Equal(chars.Coefficient, bytes.Coefficient);
		Assert.Equal(chars.Scale, bytes.Scale);
	}

	[Fact]
	public void Log_checksum_still_detects_damage()
	{
		var wire = Fix44Tests.Wire("0", "112=TEST|");
		var log = Log(FixMessages.Parse(wire));
		Assert.Throws<FormatException>(() => FixMessages.ParseLog(log.Replace("TEST", "FAIL")));
	}

	static string Log(FixMessage message)
	{
		var text = message.OriginalWire.ToCharArray();
		foreach (var field in message.AllFields) text[field.ValuePosition + field.Length] = '|';
		return new string(text);
	}
	static byte[] Bytes(string text) => text.Select(c => checked((byte)c)).ToArray();
}
