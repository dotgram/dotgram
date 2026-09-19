using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// <see cref="DotGram.Handwritten.Web.HandDateTime"/> held to the generated RFC 3339 parser: the
/// same values, the same refusals, refused at the same place (D1).
/// </summary>
/// <remarks>
/// <see cref="Rfc3339Tests"/> reads the JSON Schema suite and the RFC's examples through
/// <see cref="Both"/> already; this adds the texts where §5.7 decides and the shapes around a
/// fraction and an offset, each taken apart one character at a time.
/// </remarks>
public sealed class HandDateTimeTests
{
	public static readonly string[] Corpus =
	[
		"1985-04-12T23:20:50.52Z", "1996-12-19T16:39:57-08:00", "1990-12-31T23:59:60Z", "1990-12-31T15:59:60-08:00",
		"1937-01-01T12:00:27.87+00:20", "2026-09-18t12:00:00z", "2026-09-18T12:00:00-00:00", "2026-09-18T12:00:00+23:59",
		"2024-02-29T00:00:00Z", "2023-02-29T00:00:00Z", "2000-02-29T00:00:00Z", "1900-02-29T00:00:00Z", "2026-04-31T00:00:00Z",
		"2026-13-01T00:00:00Z", "2026-00-01T00:00:00Z", "2026-01-00T00:00:00Z", "2026-01-01T24:00:00Z", "2026-01-01T23:60:00Z",
		"2026-01-01T23:59:61Z", "2026-01-01T12:00:60Z", "2026-01-01T12:00:00+24:00", "2026-01-01T12:00:00+00:60",
		"2026-01-01T12:00:00.Z", "2026-01-01T12:00:00.123456789+01:00", "2026-01-01 12:00:00Z", "2026-01-01T12:00:00",
		"2026-01-01T12:00:00Zjunk", "0000-01-01T00:00:00Z", "9999-12-31T23:59:59.9Z",
		"2026-09-18", "2026-02-30", "12:00:00Z", "23:59:60Z", "12:00:00.5-05:30",
	];

	public static TheoryData<string> Texts => [.. Corpus];

	[Theory]
	[MemberData(nameof(Texts))]
	public void Every_text_is_answered_alike(string text)
	{
		Agree(text);
	}

	[Fact]
	public void And_alike_on_every_text_one_character_short_of_one()
	{
		var told = new List<string>();

		foreach (var timestamp in Corpus)
			foreach (var text in Mutations(timestamp))
				try
				{
					Agree(text);
				}
				catch (Exception disagreed) when (disagreed is not OutOfMemoryException)
				{
					told.Add(disagreed.Message);
				}

		Assert.True(told.Count == 0, $"{told.Count} texts told the two apart; the first:\n\n" + string.Join("\n\n", told.Take(8)));
	}

	/// <summary>A text read as each of the three things RFC 3339 names.</summary>
	static void Agree(string text)
	{
		Both.AgreeOnTimestamp(text);
		Both.AgreeOnFullDate(text);
		Both.AgreeOnFullTime(text);
	}

	/// <summary>A text cut short at every character, and with every character taken out once.</summary>
	static IEnumerable<string> Mutations(string text)
	{
		var seen = new HashSet<string>(StringComparer.Ordinal) { text };

		for (var length = 0; length < text.Length; length++)
			if (seen.Add(text.Substring(0, length)))
				yield return text.Substring(0, length);

		for (var at = 0; at < text.Length; at++)
			if (seen.Add(text.Remove(at, 1)))
				yield return text.Remove(at, 1);
	}
}
