using System;

using DotGram.Handwritten.Web;
using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// Both readings of a web format at once: the generated parser and the hand-written one in
/// DotGram.Handwritten, which must answer the same.
/// </summary>
/// <remarks>
/// The hand parsers are what the generated ones are measured against, and a ratio between two
/// parsers that read different languages says nothing (docs/design/architecture-decisions.md,
/// D1). So the tests of a format read through here: every text they hand it is put to both, and
/// a text that tells them apart fails where it stands. What comes back is the generated
/// parser's answer, so the assertions after the call are about the parser the tests were
/// written for.
/// </remarks>
static class Both
{
	// в”Ђв”Ђ RFC 3986 в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ

	/// <summary>A URI reference, read by both; the generated parser's answer.</summary>
	public static UriReference Reference(string text)
	{
		var match = AgreeOnReference(text);

		return match.IsSuccess ? match.Value : throw new FormatException(match.Error);
	}

	/// <summary>A URI reference or false, read by both; the generated parser's answer.</summary>
	public static bool TryReference(string text, out UriReference? reference)
	{
		var match = AgreeOnReference(text);

		reference = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>The two answering the same about a text read as a reference.</summary>
	public static Rfc3986.Match<UriReference> AgreeOnReference(string text)
	{
		var generated = Rfc3986.TryParseReference(text);
		var hand      = HandUrl.TryParseReference(text, out var value, out var failure);

		Agree(text, "reference", Describe(generated), Describe(hand, value, (long)failure));

		return generated;
	}

	/// <summary>The two answering the same about a text read as a URI.</summary>
	public static Rfc3986.Match<UriReference> AgreeOnUri(string text)
	{
		var generated = Rfc3986.TryParseUri(text);
		var hand      = HandUrl.TryParseUri(text, out var value, out var failure);

		Agree(text, "URI", Describe(generated), Describe(hand, value, (long)failure));

		return generated;
	}

	// ── RFC 3339 ────────────────────────────────────────────────────────────────

	/// <summary>A date-time or false, read by both; the generated parser's answer.</summary>
	public static bool TryTimestamp(string text, out Timestamp? timestamp)
	{
		var match = AgreeOnTimestamp(text);

		timestamp = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>A full-date or false, read by both; the generated parser's answer.</summary>
	public static bool TryFullDate(string text, out FullDate? date)
	{
		var match = AgreeOnFullDate(text);

		date = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>A full-time or false, read by both; the generated parser's answer.</summary>
	public static bool TryFullTime(string text, out FullTime? time)
	{
		var match = AgreeOnFullTime(text);

		time = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	public static Timestamp Timestamp(string text)
	{
		var match = AgreeOnTimestamp(text);

		return match.IsSuccess ? match.Value : throw new FormatException(match.Error);
	}

	public static FullTime FullTime(string text)
	{
		var match = AgreeOnFullTime(text);

		return match.IsSuccess ? match.Value : throw new FormatException(match.Error);
	}

	public static Rfc3339.Match<Timestamp> AgreeOnTimestamp(string text)
	{
		var generated = Rfc3339.TryParseTimestamp(text);
		var hand      = HandDateTime.TryParseTimestamp(text, out var value, out var failure);

		Agree(text, "date-time", Describe(generated.IsSuccess, generated.Value, generated.Position), Describe(hand, value, failure));

		return generated;
	}

	public static Rfc3339.Match<FullDate> AgreeOnFullDate(string text)
	{
		var generated = Rfc3339.TryParseFullDate(text);
		var hand      = HandDateTime.TryParseFullDate(text, out var value, out var failure);

		Agree(text, "full-date", Describe(generated.IsSuccess, generated.Value, generated.Position), Describe(hand, value, failure));

		return generated;
	}

	public static Rfc3339.Match<FullTime> AgreeOnFullTime(string text)
	{
		var generated = Rfc3339.TryParseFullTime(text);
		var hand      = HandDateTime.TryParseFullTime(text, out var value, out var failure);

		Agree(text, "full-time", Describe(generated.IsSuccess, generated.Value, generated.Position), Describe(hand, value, failure));

		return generated;
	}

	// ── Both ────────────────────────────────────────────────────────────────────

	static string Describe(Rfc3986.Match<UriReference> match)
	{
		return Describe(match.IsSuccess, match.Value, match.Position);
	}

	static string Describe<T>(bool read, T? value, long failure)
	{
		return read ? value!.ToString()! : "refused at " + failure;
	}

	static void Agree(string text, string what, string generated, string hand)
	{
		if (generated != hand)
			Assert.Fail(
				$"The generated parser and the hand-written one disagree about \"{text}\" read as a {what}.\n" +
				$"generated: {generated}\n" +
				$"by hand:   {hand}");
	}
}
