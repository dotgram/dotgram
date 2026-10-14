using System;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

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
	[InlineData(0x1FFFE, false)]
	[InlineData(0x1FFFE, true)]
	[InlineData(0xE0000, false)]
	[InlineData(0xE0000, true)]
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

	[Theory]
	[InlineData("a=b; Max-Age=9223372036854775800", 9223372036854775800L, false)]
	[InlineData("a=b; Max-Age=9223372036854775800", 9223372036854775800L, true)]
	[InlineData("a=b; Max-Age=-9223372036854775800", -9223372036854775800L, false)]
	[InlineData("a=b; Max-Age=-9223372036854775800", -9223372036854775800L, true)]
	public void Cookie_MaxAge_preserves_integers_that_fit_in_Int64(string text, long expected, bool tryParse)
	{
		// RFC 6265 §5.2.2: convert attribute-value to the integer delta-seconds.
		// Neither value overflows Int64, so the documented overflow saturation does not apply.
		SetCookie? cookie;

		if (tryParse)
			Assert.True(SetCookie.TryParse(text, out cookie));
		else
			cookie = SetCookie.Parse(text);

		Assert.NotNull(cookie);
		Assert.Equal(expected, cookie.MaxAge);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Forwarded_accepts_a_nonempty_element_containing_only_an_empty_pair_slot(bool tryParse)
	{
		// RFC 7239 §4: forwarded-element = [ forwarded-pair ] *( ";" [ forwarded-pair ] ).
		// ";" is a nonempty element even though both optional pairs are absent.
		if (tryParse)
			Assert.True(ForwardedElement.TryParseField(";", out _));
		else
			ForwardedElement.ParseField(";");
	}

	static void AssertLinkRefused(string text, bool tryParse)
	{
		if (tryParse)
			Assert.False(WebLink.TryParseField(text, out _));
		else
			Assert.Throws<FormatException>(() => WebLink.ParseField(text));
	}
}
