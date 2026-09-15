using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class PrimitiveTests
{
	public static IEnumerable<object[]> OfficialCodes()
	{
		XNamespace ns = "http://fixprotocol.io/2020/orchestra/repository";
		var repository = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "OrchestraFIX44.xml")).Root!;
		var codeSets = repository.Element(ns + "codeSets")!.Elements().ToDictionary(x => (string)x.Attribute("name")!);
		foreach (var field in repository.Element(ns + "fields")!.Elements())
		{
			if (!codeSets.TryGetValue((string)field.Attribute("type")!, out var codeSet)) continue;
			foreach (var code in codeSet.Elements(ns + "code"))
				yield return new object[] { (int)field.Attribute("id")!, (string)code.Attribute("value")! };
		}
	}

	[Theory]
	[MemberData(nameof(OfficialCodes))]
	public void Every_official_code_is_accepted(int tag, string value) => Assert.True(Valid(tag, value), $"Tag {tag}: {value}");

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
	public void Primitive_boundaries(int tag, string value, bool expected) => Assert.Equal(expected, Valid(tag, value));

	static bool Valid(int tag, string value) => FixPrimitives.Valid(new FixField(value, tag, 0, 0, value.Length), FixSchema.Type(tag), FixSchema.Codes(tag));
}
