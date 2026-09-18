using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DotGram.Finance.Tests;

/// <summary>
/// The standard FIX 4.4 messages the tests read, and a way to write one more: shared by
/// DotGram.Finance.Tests and the oracle's tests, which compile this file too.
/// </summary>
public static class FixFixtures
{
	/// <summary>
	/// Every message of Fixtures.json, as its type name and its wire text.
	/// </summary>
	public static IEnumerable<object[]> Messages()
	{
		using var fixtures = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures.json")));

		foreach (var item in fixtures.RootElement.EnumerateArray())
			yield return new object[] { item.GetProperty("name").GetString()!, item.GetProperty("wire").GetString()! };
	}

	/// <summary>
	/// A FIX 4.4 message of the given type around a body written with '|' for SOH, with its
	/// standard header, body length and checksum.
	/// </summary>
	public static string Wire(string type, string body)
	{
		body = "35=" + type + "\u000149=SENDER\u000156=TARGET\u000134=1\u000152=20260915-12:00:00\u0001" + body.Replace('|', '\u0001');

		var prefix   = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var checksum = prefix.Aggregate(0, (sum, c) => (sum + c) & 255);

		return prefix + "10=" + checksum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}
}
