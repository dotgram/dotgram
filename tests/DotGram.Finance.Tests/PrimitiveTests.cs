using System;
using System.Text.Json;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class PrimitiveTests
{
	public static IEnumerable<object[]> KnownCodes()
	{
		using var cases = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "FieldCases.json")));
		foreach (var code in cases.RootElement.GetProperty("codes").EnumerateArray())
			yield return new object[] { code.GetProperty("tag").GetInt32(), code.GetProperty("value").GetString()! };
	}

	[Theory]
	[MemberData(nameof(KnownCodes))]
	public void Every_known_code_is_accepted(int tag, string value)
	{
		Assert.True(Valid(tag, value), $"Tag {tag}: {value}");
	}

	[Theory]
	[InlineData(38, "0", true)]
	[InlineData(38, "-1.5", true)]
	[InlineData(38, "9999999999999999999999999999999999999999.000001", true)]
	[InlineData(38, ".5", true)]
	[InlineData(38, "1e2", false)]
	[InlineData(38, "+1", false)]
	[InlineData(38, "1,5", false)]
	[InlineData(38, ".", false)]
	[InlineData(34, "0001", true)]
	[InlineData(34, "0", false)]
	[InlineData(34, "-1", false)]
	[InlineData(95, "0", true)]
	[InlineData(95, "-1", false)]
	[InlineData(52, "20000229-23:59:59.999", true)]
	[InlineData(52, "19000229-23:59:59", false)]
	[InlineData(52, "20161231-23:59:60", true)]
	[InlineData(52, "20260915-24:00:00", false)]
	[InlineData(52, "20260915-12:00:60", false)]
	[InlineData(52, "20260915-12:00:00.1", false)]
	[InlineData(75, "00000229", true)]
	[InlineData(75, "20260431", false)]
	[InlineData(200, "202609", true)]
	[InlineData(200, "202609w5", true)]
	[InlineData(200, "202609w6", false)]
	[InlineData(15, "USD", true)]
	[InlineData(15, "usd", false)]
	[InlineData(100, "XNAS", true)]
	[InlineData(100, "xnas", false)]
	[InlineData(100, "!@#$", false)]
	[InlineData(18, "1 2", true)]
	[InlineData(18, "1  2", false)]
	[InlineData(18, "1 ", false)]
	[InlineData(27, "12345", true)]
	public void Primitive_boundaries(int tag, string value, bool expected)
	{
		Assert.Equal(expected, Valid(tag, value));
	}

	static bool Valid(int tag, string value)
	{
		return FixPrimitives.Valid(new FixFieldView(value, tag, 0, 0, value.Length), FixSchema.Type(tag), FixSchema.Codes(tag));
	}
}
