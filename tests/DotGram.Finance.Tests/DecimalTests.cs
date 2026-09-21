using System;
using System.Globalization;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class DecimalTests
{
	[Theory]
	[InlineData("79228162514264337593543950335", true)]
	[InlineData("-79228162514264337593543950335", true)]
	[InlineData("79228162514264337593543950335.0", true)]
	[InlineData("79228162514264337593543950336", false)]
	[InlineData("79228162514264337593543950335.1", false)]
	[InlineData("0.0000000000000000000000000001", true)]
	[InlineData("0.00000000000000000000000000001", false)]
	[InlineData("1.00000000000000000000000000001", false)]
	[InlineData("1.00000000000000000000000000000", true)]
	[InlineData("0.00000000000000000000000000000", true)]
	[InlineData(".5", true)]
	[InlineData("-.5", true)]
	[InlineData("1.", true)]
	[InlineData("-0", true)]
	[InlineData("+1", false)]
	[InlineData("1e2", false)]
	[InlineData(" 1", false)]
	[InlineData("1 ", false)]
	[InlineData("1,5", false)]
	[InlineData("1.2.3", false)]
	[InlineData(".", false)]
	[InlineData("-", false)]
	[InlineData("", false)]
	public void Conversion_is_exact_and_identical_for_characters_and_bytes(string text, bool valid)
	{
		Assert.Equal(valid, FixConvert.Decimal(text.AsSpan(), out var chars));
		Assert.Equal(valid, FixConvert.Decimal(Encoding.ASCII.GetBytes(text), out var bytes));
		if (valid)
		{
			Assert.Equal(decimal.Parse(text, CultureInfo.InvariantCulture), chars);
			Assert.Equal(chars, bytes);
		}
	}

	[Fact]
	public void Unrepresentable_values_remain_typed_invalid_fields()
	{
		const string input = "44=79228162514264337593543950336|38=0.00000000000000000000000000001";
		foreach (var field in FixParser.ParseFields(input, FixFieldOptions.Log))
		{
			var number = Assert.IsAssignableFrom<FixField.Typed<decimal>>(field);
			Assert.False(number.TryGetValue(out _));
			Assert.Throws<InvalidOperationException>(() => number.Value);
		}
	}
}
