using System;
using System.Buffers.Text;
using System.Globalization;
using System.Text;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The conversions have short paths for the common case — an integer or decimal of up to
/// eighteen digits, a date or time part, a tag — and each must answer exactly what the general
/// path answers: the same validity, the same value, and for a decimal the same bits, scale
/// included. Both the character and the byte form are held to it.
/// </summary>
public sealed class FixConvertFastPathTests
{
	[Theory]
	[InlineData("0")]
	[InlineData("7")]
	[InlineData("-0")]
	[InlineData("-7")]
	[InlineData("00042")]
	[InlineData("999999999999999999")]
	[InlineData("-999999999999999999")]
	[InlineData("1000000000000000000")]
	[InlineData("9223372036854775807")]
	[InlineData("-9223372036854775808")]
	[InlineData("00000000000000000001")]
	[InlineData("0000000000000000000")]
	[InlineData("-000000009223372036854775808")]
	public void An_integer_is_the_value_long_reads(string text)
	{
		var expected = long.Parse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

		Assert.True(FixConvert.Integer(text.AsSpan(), out var characters));
		Assert.True(FixConvert.Integer(Encoding.ASCII.GetBytes(text), out var bytes));
		Assert.Equal(expected, characters);
		Assert.Equal(expected, bytes);
	}

	[Theory]
	[InlineData("")]
	[InlineData("-")]
	[InlineData("+1")]
	[InlineData(" 1")]
	[InlineData("1 ")]
	[InlineData("1a")]
	[InlineData("1.0")]
	[InlineData("--1")]
	[InlineData("12345678901234567x")]
	[InlineData("1234567890123456789x")]
	[InlineData("9223372036854775808")]
	[InlineData("-9223372036854775809")]
	[InlineData("123456789012345678901234567890")]
	public void An_integer_refuses_what_is_not_digits_or_does_not_fit_a_long(string text)
	{
		Assert.False(FixConvert.Integer(text.AsSpan(), out _));
		Assert.False(FixConvert.Integer(Encoding.ASCII.GetBytes(text), out _));
	}

	[Theory]
	[InlineData("12.50")]
	[InlineData("0012.50")]
	[InlineData("100")]
	[InlineData("5.")]
	[InlineData(".5")]
	[InlineData("1.000")]
	[InlineData("0.10")]
	[InlineData("-12.50")]
	[InlineData("-.5")]
	[InlineData("123456789012345678")]
	[InlineData("12345678901234567.8")]
	[InlineData("0.000000000000000001")]
	[InlineData("1234567890123456789")]
	[InlineData("1.234567890123456789")]
	[InlineData("0.1234567890123456789012345678")]
	[InlineData("79228162514264337593543950335")]
	[InlineData("0")]
	[InlineData("0.00")]
	[InlineData("-0")]
	[InlineData("-0.00")]
	public void A_decimal_has_the_bits_decimal_parsing_gives(string text)
	{
		var expected = decimal.GetBits(decimal.Parse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture));

		Assert.True(Utf8Parser.TryParse(Encoding.ASCII.GetBytes(text), out decimal utf8, out _));
		Assert.Equal(expected, decimal.GetBits(utf8));

		Assert.True(FixConvert.Decimal(text.AsSpan(), out var characters));
		Assert.True(FixConvert.Decimal(Encoding.ASCII.GetBytes(text), out var bytes));
		Assert.Equal(expected, decimal.GetBits(characters));
		Assert.Equal(expected, decimal.GetBits(bytes));
	}

	[Theory]
	[InlineData("")]
	[InlineData("-")]
	[InlineData(".")]
	[InlineData("1.2.3")]
	[InlineData("+1")]
	[InlineData("1e5")]
	[InlineData(" 1")]
	[InlineData("0.12345678901234567890123456789")]
	[InlineData("79228162514264337593543950336")]
	public void A_decimal_refuses_what_it_cannot_hold_or_spell(string text)
	{
		Assert.False(FixConvert.Decimal(text.AsSpan(), out var characters));
		Assert.False(FixConvert.Decimal(Encoding.ASCII.GetBytes(text), out var bytes));
		Assert.Equal(0m, characters);
		Assert.Equal(0m, bytes);
	}

	[Theory]
	[InlineData("20240229", true)]
	[InlineData("20000229", true)]
	[InlineData("19000229", false)]
	[InlineData("20230229", false)]
	[InlineData("00000101", false)]
	[InlineData("00010101", true)]
	[InlineData("20261301", false)]
	[InlineData("20260100", false)]
	[InlineData("2026O915", false)]
	[InlineData("2026-9-1", false)]
	public void A_date_is_read_from_its_digits(string text, bool valid)
	{
		Assert.Equal(valid, FixConvert.Date(text.AsSpan(), out var characters));
		Assert.Equal(valid, FixConvert.Date(Encoding.ASCII.GetBytes(text), out var bytes));
		Assert.Equal(characters, bytes);
	}

	/// <summary>
	/// The leap second the protocol admits is the second after it; a fraction is kept to the tick,
	/// and digits past the seventh are read and let go.
	/// </summary>
	[Theory]
	[InlineData("23:59:60", true, "00:00:00.0000000")]
	[InlineData("23:59:60.250", true, "00:00:00.2500000")]
	[InlineData("12:59:60", false, "")]
	[InlineData("24:00:00", false, "")]
	[InlineData("12:00:00.000", true, "12:00:00.0000000")]
	[InlineData("12:00:00.123456789", true, "12:00:00.1234567")]
	[InlineData("12:0O:00", false, "")]
	[InlineData("12:00:00.", false, "")]
	public void A_time_is_read_from_its_digits(string text, bool valid, string expected)
	{
		Assert.Equal(valid, FixConvert.Time(text.AsSpan(), out var characters));
		Assert.Equal(valid, FixConvert.Time(Encoding.ASCII.GetBytes(text), out var bytes));
		Assert.Equal(characters, bytes);

		if (valid)
			Assert.Equal(expected, characters.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture));
	}

	/// <summary>A timestamp is an instant at offset zero; the leap second at the end of a day is the next day's first.</summary>
	[Theory]
	[InlineData("19981231-23:59:59", true, "1998-12-31T23:59:59.0000000+00:00")]
	[InlineData("19981231-23:59:60", true, "1999-01-01T00:00:00.0000000+00:00")]
	[InlineData("19981231-23:59:60.500", true, "1999-01-01T00:00:00.5000000+00:00")]
	[InlineData("20260922-12:00:00.123", true, "2026-09-22T12:00:00.1230000+00:00")]
	[InlineData("00000101-00:00:00", false, "")]
	[InlineData("99991231-23:59:60", false, "")]
	[InlineData("20260922 12:00:00", false, "")]
	[InlineData("20260922-12:00", false, "")]
	public void A_timestamp_is_an_instant_at_offset_zero(string text, bool valid, string expected)
	{
		Assert.Equal(valid, FixConvert.Timestamp(text.AsSpan(), out var characters));
		Assert.Equal(valid, FixConvert.Timestamp(Encoding.ASCII.GetBytes(text), out var bytes));
		Assert.Equal(characters, bytes);

		if (valid)
		{
			Assert.Equal(TimeSpan.Zero, characters.Offset);
			Assert.Equal(expected, characters.ToString("o", CultureInfo.InvariantCulture));
		}
	}

	/// <summary>A MonthYear is its text, held to the three shapes the protocol gives it.</summary>
	[Theory]
	[InlineData("202609", true)]
	[InlineData("20260915", true)]
	[InlineData("202609w1", true)]
	[InlineData("202609w5", true)]
	[InlineData("202609w6", false)]
	[InlineData("20260931", false)]
	[InlineData("202613", false)]
	[InlineData("2026091", false)]
	public void A_month_year_is_its_text_in_one_of_three_shapes(string text, bool valid)
	{
		Assert.Equal(valid, FixConvert.MonthYear(text.AsSpan(), out var characters));
		Assert.Equal(valid, FixConvert.MonthYear(Encoding.ASCII.GetBytes(text), out var bytes));
		Assert.Equal(text, characters);
		Assert.Equal(text, bytes);
	}

	[Theory]
	[InlineData("1", 1)]
	[InlineData("00012", 12)]
	[InlineData("2147483647", int.MaxValue)]
	[InlineData("2147483648", -1)]
	[InlineData("99999999999", -1)]
	[InlineData("", -1)]
	[InlineData("-1", -1)]
	[InlineData("+1", -1)]
	[InlineData(" 1", -1)]
	[InlineData("1a", -1)]
	public void A_tag_is_digits_that_fit_an_int(string text, int expected)
	{
		Assert.Equal(expected, FixConvert.Tag(text.AsSpan()));
		Assert.Equal(expected, FixConvert.Tag(Encoding.ASCII.GetBytes(text)));
	}

	/// <summary>The data table answers the standard for every standard tag, and nothing beyond it.</summary>
	[Fact]
	public void Standard_data_tags_come_from_the_standard()
	{
		var options = new FixContext();

		for (var tag = -2; tag < 2000; tag++)
			Assert.Equal(tag > 0 && FixFieldBuilder.Value(tag, "0".AsSpan(), options.CustomFields) is FixField.Typed<ReadOnlyMemory<byte>>, options.IsData(tag));

		Assert.False(options.IsData(int.MaxValue));
		Assert.False(options.IsData(int.MinValue));
	}

	/// <summary>A custom data tag is answered by the options' own dictionary, past the schema's tags.</summary>
	[Fact]
	public void Custom_data_tags_come_from_the_options()
	{
		var options = new FixContext { LengthDataPairs = new System.Collections.Generic.Dictionary<int, int> { [5000] = 5001 } };

		Assert.IsType<FixField.Invalid>(FixFieldBuilder.Value(5001, "0".AsSpan(), options.CustomFields));
		Assert.True(options.IsData(5001));
		Assert.False(options.IsData(5000));
		Assert.True(options.IsData(96));
	}

	/// <summary>
	/// Text read from bytes is each byte as the character of the same code, the upper half
	/// included, on either side of the length where the conversion stops using the stack.
	/// </summary>
	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(256)]
	[InlineData(257)]
	[InlineData(1000)]
	public void Text_from_bytes_keeps_every_byte_as_its_character(int length)
	{
		var bytes = new byte[length];

		for (var i = 0; i < length; i++)
			bytes[i] = (byte)(i * 7 + 3);

		var text = FixConvert.Text(bytes);

		Assert.Equal(length, text.Length);

		for (var i = 0; i < length; i++)
			Assert.Equal((char)bytes[i], text[i]);
	}
}
