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

		Agree(text, "reference", Describe(generated), Describe(hand, value, failure));

		return generated;
	}

	/// <summary>The two answering the same about a text read as a URI.</summary>
	public static Rfc3986.Match<UriReference> AgreeOnUri(string text)
	{
		var generated = Rfc3986.TryParseUri(text);
		var hand      = HandUrl.TryParseUri(text, out var value, out var failure);

		Agree(text, "URI", Describe(generated), Describe(hand, value, failure));

		return generated;
	}

	static string Describe(Rfc3986.Match<UriReference> match)
	{
		return match.IsSuccess ? match.Value.ToString() : "refused at " + match.Position;
	}

	static string Describe(bool read, UriReference? value, int failure)
	{
		return read ? value!.ToString() : "refused at " + failure;
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
