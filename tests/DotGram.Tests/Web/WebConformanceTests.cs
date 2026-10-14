using System;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// Rules of the RFCs, read from their own text, that the DotGram.Web readers once missed: each case
/// names the section it comes from.
/// </summary>
public sealed class WebConformanceTests
{
	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public void IPvFuture_version_marker_is_case_insensitive(bool absolute, bool tryParse)
	{
		// RFC 3986 §3.2.2: the IPvFuture version flag "v" is case-insensitive.
		const string Text = "https://[V1.example]/";
		UriReference? uri;

		if (tryParse)
			Assert.True(absolute ? UriReference.TryParseUri(Text, out uri) : UriReference.TryParse(Text, out uri));
		else
			uri = absolute ? UriReference.ParseUri(Text) : UriReference.Parse(Text);

		Assert.NotNull(uri);
		Assert.Equal("[V1.example]", uri.Host);
	}

	[Theory]
	[InlineData("</>; rel=next; anchor=\"/bad%ZZ\"", false)]
	[InlineData("</>; rel=next; anchor=\"/bad%ZZ\"", true)]
	public void Link_anchor_must_be_a_URI_reference(string text, bool tryParse)
	{
		// RFC 8288 §3.2: anchor has the URI-Reference syntax of RFC 3986 §4.1.
		AssertLinkRefused(text, tryParse);
	}

	[Theory]
	[InlineData("</>; rel=\"../next\"", false)]
	[InlineData("</>; rel=\"../next\"", true)]
	[InlineData("</>; rel=\"next\tprev\"", false)]
	[InlineData("</>; rel=\"next\tprev\"", true)]
	public void Link_relations_require_registered_names_or_absolute_URIs_separated_by_spaces(string text, bool tryParse)
	{
		// RFC 8288 §3.3: ext-rel-type is URI, and multiple relation types use 1*SP.
		AssertLinkRefused(text, tryParse);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Link_hreflang_must_be_a_language_tag(bool tryParse)
	{
		// RFC 8288 §3.4.1: hreflang is Language-Tag; RFC 5646 §2.1 forbids underscores.
		AssertLinkRefused("</>; rel=alternate; hreflang=en_US", tryParse);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Link_type_requires_a_type_and_subtype(bool tryParse)
	{
		// RFC 8288 §3.4.1: type is type-name "/" subtype-name, not an arbitrary token.
		AssertLinkRefused("</>; rel=next; type=html", tryParse);
	}

	[Theory]
	[InlineData("</>; rel=\"next  http://example.net/rel\"")]       // 1*SP between two types
	[InlineData("</>; rel=next; rel=\"../ignored\"")]                 // a second rel is ignored, so not asked
	[InlineData("</>; type=\"Text/HTML+xml\"; type=html")]            // a second type likewise
	[InlineData("</>; rev=prev; anchor=\"#here\"; hreflang=en-US")]
	[InlineData("</>; type=\"text/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\"")] // 127
	[InlineData("</>; media=\"screen, print\"")]                      // media is CSS's syntax, not checked
	public void Link_values_with_the_RFCs_syntax_are_read(string text)
	{
		Assert.True(WebLink.TryParseField(text, out _), $"'{text}' was refused.");
	}

	[Theory]
	[InlineData("</>; rel=Next")]           // reg-rel-type is lowercase, and Next is no URI
	[InlineData("</>; rel=\"\"")]
	[InlineData("</>; rel=\" next\"")]
	[InlineData("</>; rev=\"../prev\"")]
	[InlineData("</>; hreflang=\"\"")]
	[InlineData("</>; type=\"text/\"")]
	[InlineData("</>; type=\"text/html/x\"")]
	[InlineData("</>; type=\"-text/html\"")]             // a restricted-name begins with a letter or digit
	[InlineData("</>; type=\"text/ht%ml\"")]
	[InlineData("</>; type=\"text/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\"")] // 128
	[InlineData("</>; REL=\"../next\"; rel=next")]   // the first rel, whatever its name's case, is the one asked
	[InlineData("</>; TYPE=html; type=\"text/html\"")]
	[InlineData("</>; hreflang=en; hreflang=en_US")]   // every hreflang counts, so every one is asked
	[InlineData("</>; anchor=\"#a\"; anchor=\"%ZZ\"")]
	public void Link_values_without_the_RFCs_syntax_are_refused(string text)
	{
		AssertLinkRefused(text, tryParse: true);
	}

	[Theory]
	[InlineData(0x1FFFE, false)]
	[InlineData(0x1FFFE, true)]
	[InlineData(0xE0000, false)]
	[InlineData(0xE0000, true)]
	[InlineData(0x1FFFF, true)]
	[InlineData(0xE0FFF, true)]
	[InlineData(0xEFFFE, true)]
	[InlineData(0xFFFFF, true)]
	[InlineData(0x10FFFE, true)]
	public void URI_template_literals_exclude_code_points_outside_ucschar_and_iprivate(int codePoint, bool tryParse)
	{
		// RFC 6570 §§1.5, 2.1: supplementary ucschar ranges exclude FFFE/FFFF and E0000-E0FFF;
		// iprivate does not fill either gap. A valid UTF-16 pair alone is insufficient.
		var text = "/" + char.ConvertFromUtf32(codePoint);

		if (tryParse)
			Assert.False(UriTemplate.TryParse(text, out _));
		else
			Assert.Throws<FormatException>(() => UriTemplate.Parse(text));
	}

	[Fact]
	public void URI_template_literals_read_the_ends_of_every_supplementary_plane()
	{
		// Each plane from 1 to 16 is in ucschar or iprivate from its first code point to xFFFD, and
		// its last two are not; plane 14 has U+E0000 to U+E0FFF missing as well.
		for (var plane = 1; plane <= 16; plane++)
		{
			var first = plane == 14 ? 0xE1000 : plane << 16;

			foreach (var codePoint in new[] { first, (plane << 16) | 0xFFFD })
			{
				var text = "/" + char.ConvertFromUtf32(codePoint);

				Assert.True(UriTemplate.TryParse(text, out var template), $"U+{codePoint:X} was refused.");
				Assert.Equal(text, ((UriTemplate.Literal)Assert.Single(template.Parts)).Text);
			}

			foreach (var codePoint in new[] { (plane << 16) | 0xFFFE, (plane << 16) | 0xFFFF })
				Assert.False(UriTemplate.TryParse("/" + char.ConvertFromUtf32(codePoint), out _), $"U+{codePoint:X} was read.");
		}

		Assert.False(UriTemplate.TryParse("/" + char.ConvertFromUtf32(0xE0FFF), out _));
	}

	[Theory]
	[InlineData("a=b; Max-Age=9223372036854775800", 9223372036854775800L, false)]
	[InlineData("a=b; Max-Age=9223372036854775800", 9223372036854775800L, true)]
	[InlineData("a=b; Max-Age=-9223372036854775800", -9223372036854775800L, false)]
	[InlineData("a=b; Max-Age=-9223372036854775800", -9223372036854775800L, true)]
	[InlineData("a=b; Max-Age=9223372036854775807", long.MaxValue, true)]
	[InlineData("a=b; Max-Age=-9223372036854775808", long.MinValue, true)]
	[InlineData("a=b; Max-Age=9223372036854775808", long.MaxValue, true)]      // past the end: saturates
	[InlineData("a=b; Max-Age=-9223372036854775809", long.MinValue, true)]
	[InlineData("a=b; Max-Age=0099999999999999999999", long.MaxValue, true)]
	public void Cookie_MaxAge_preserves_integers_that_fit_in_Int64(string text, long expected, bool tryParse)
	{
		// RFC 6265 §5.2.2: convert attribute-value to the integer delta-seconds.
		// A value Int64 holds is read exactly; only one past either end saturates, as documented.
		SetCookie? cookie;

		if (tryParse)
			Assert.True(SetCookie.TryParse(text, out cookie));
		else
			cookie = SetCookie.Parse(text);

		Assert.NotNull(cookie);
		Assert.Equal(expected, cookie.MaxAge);
	}

	[Theory]
	[InlineData(";", false)]
	[InlineData(";", true)]
	[InlineData(" , ; ,", false)]
	[InlineData(" , ; ,", true)]
	public void Forwarded_accepts_a_nonempty_element_containing_only_empty_pair_slots(string text, bool tryParse)
	{
		// RFC 7239 §4: forwarded-element = [ forwarded-pair ] *( ";" [ forwarded-pair ] ).
		// ";" is a nonempty element even though both optional pairs are absent; it holds no pair,
		// so nothing is returned for it.
		ForwardedElement[]? elements;

		if (tryParse)
			Assert.True(ForwardedElement.TryParseField(text, out elements));
		else
			elements = ForwardedElement.ParseField(text);

		Assert.Empty(elements);
	}

	static void AssertLinkRefused(string text, bool tryParse)
	{
		if (tryParse)
			Assert.False(WebLink.TryParseField(text, out _));
		else
			Assert.Throws<FormatException>(() => WebLink.ParseField(text));
	}
}
