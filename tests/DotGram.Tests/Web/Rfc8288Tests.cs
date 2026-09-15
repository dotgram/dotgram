using System;
using System.Linq;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 8288's Link header field, held to the examples of its §3.5 and to the rules §3 and RFC 8187 give.
/// </summary>
/// <remarks>
/// There is no shared test suite for the Link header field, so the material is the RFC: every example
/// in §3.5, with its line breaks unfolded to the single space a field value carries, and each rule of §3
/// that says what a recipient does — which parameter counts where one may appear once, what an empty list
/// element is, how `title*` decodes.
/// </remarks>
public sealed class Rfc8288Tests
{
	[Fact]
	public void A_link_with_a_relation_and_a_title()
	{
		var link = Assert.Single(Rfc8288.ParseLinks("<http://example.com/TheBook/chapter2>; rel=\"previous\"; title=\"previous chapter\""));

		Assert.Equal("http://example.com/TheBook/chapter2", link.Target);
		Assert.Equal(["previous"], link.Relations);
		Assert.Equal("previous chapter", link.Title);
	}

	[Fact]
	public void An_extension_relation_type_is_a_URI()
	{
		var link = Assert.Single(Rfc8288.ParseLinks("</>; rel=\"http://example.net/foo\""));

		Assert.Equal("/", link.Target);
		Assert.Equal(["http://example.net/foo"], link.Relations);
	}

	[Fact]
	public void An_anchor_is_the_context()
	{
		var link = Assert.Single(Rfc8288.ParseLinks("</terms>; rel=\"copyright\"; anchor=\"#foo\""));

		Assert.Equal("#foo", link.Anchor);
	}

	/// <summary>§3.5's two links with titles in RFC 8187's encoding, one of them with a character past ASCII.</summary>
	[Fact]
	public void Two_links_with_encoded_titles()
	{
		var links = Rfc8288.ParseLinks(
			"</TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel, " +
			"</TheBook/chapter4>; rel=\"next\"; title*=UTF-8'de'n%c3%a4chstes%20Kapitel");

		Assert.Equal(2, links.Length);
		Assert.Equal(("letztes Kapitel", "de"), (links[0].Title, links[0].First("title*")!.Extended!.Language));
		Assert.Equal("nächstes Kapitel", links[1].Title);
		Assert.Equal(["next"], links[1].Relations);
	}

	[Fact]
	public void One_value_may_carry_several_relation_types()
	{
		var link = Assert.Single(Rfc8288.ParseLinks("<http://example.org/>; rel=\"start http://example.net/relation/other\""));

		Assert.Equal(["start", "http://example.net/relation/other"], link.Relations);
	}

	[Fact]
	public void Several_values_are_one_field()
	{
		var links = Rfc8288.ParseLinks("<https://example.org/>; rel=\"start\", <https://example.org/index>; rel=\"index\"");

		Assert.Equal(["https://example.org/", "https://example.org/index"], links.Select(link => link.Target).ToArray());
		Assert.Equal(["start", "index"], links.Select(link => link.Relations.Single()).ToArray());
	}

	// ── §3's rules for a recipient ───────────────────────────────────────────────

	/// <summary>A token and a quoted-string are the same value (§3).</summary>
	[Fact]
	public void A_token_and_a_quoted_string_are_the_same_value()
	{
		Assert.Equal(
			Rfc8288.ParseLinks("<a>; x=y").Single().First("x")!.Value,
			Rfc8288.ParseLinks("<a>; x=\"y\"").Single().First("x")!.Value);
	}

	/// <summary>Where a parameter may appear once, the first counts and the rest are ignored (§3.3, §3.4.1).</summary>
	[Fact]
	public void The_first_of_a_once_only_parameter_counts()
	{
		// A media type holds `/`, which no token does, so it is quoted.
		var link = Rfc8288.ParseLinks("<a>; rel=first; rel=second; title=one; TITLE=two; type=\"text/html\"; type=\"text/plain\"").Single();

		Assert.Equal(["first"], link.Relations);
		Assert.Equal("one", link.Title);
		Assert.Equal("text/html", link.Type);
		Assert.Equal(["rel", "rel", "title", "title", "type", "type"], link.Parameters.Select(parameter => parameter.Name).ToArray());
	}

	/// <summary>`title*` is used where it decodes, and `title` where it does not (§3.4.1, RFC 8187 §3.2.1).</summary>
	[Fact]
	public void An_encoded_title_is_preferred_where_it_decodes()
	{
		Assert.Equal("Äpfel", Rfc8288.ParseLinks("<a>; title=Apples; title*=UTF-8''%C3%84pfel").Single().Title);
		Assert.Equal("Apples", Rfc8288.ParseLinks("<a>; title=Apples; title*=UTF-8''%C3").Single().Title);
		Assert.Equal("Apples", Rfc8288.ParseLinks("<a>; title=Apples; title*=\"UTF-8''x\"").Single().Title);
		Assert.Equal("Äpfel", Rfc8288.ParseLinks("<a>; title*=iso-8859-1'en'%C4pfel").Single().Title);
	}

	/// <summary>Several hreflang parameters say several languages are available (§3.4.1).</summary>
	[Fact]
	public void Every_hreflang_counts()
	{
		Assert.Equal(["en", "de"], Rfc8288.ParseLinks("<a>; rel=alternate; hreflang=en; hreflang=de").Single().HrefLangs);
	}

	/// <summary>A list's empty elements are accepted, and a field with none is no links (RFC 9110 §5.6.1).</summary>
	[Theory]
	[InlineData("",                         0)]
	[InlineData(" , ,",                     0)]
	[InlineData(",<a>; rel=x",              1)]
	[InlineData("<a>; rel=x,, <b>; rel=y,", 2)]
	[InlineData("<a>;rel=x\t,\t<b>",        2)]
	public void Empty_list_elements_are_accepted(string field, int count) =>
		Assert.Equal(count, Rfc8288.ParseLinks(field).Length);

	[Fact]
	public void Space_is_allowed_around_the_equals_sign()
	{
		var link = Rfc8288.ParseLinks("<a>; rel = \"next\" ; title =x").Single();

		Assert.Equal(["next"], link.Relations);
		Assert.Equal("x", link.Title);
	}

	[Fact]
	public void A_parameter_without_a_value_is_empty() =>
		Assert.Equal("", Rfc8288.ParseLinks("<a>; rel=x; crossorigin").Single().First("crossorigin")!.Value);

	[Theory]
	[InlineData("<a")]                          // the target is never closed
	[InlineData("a; rel=x")]                    // no angle brackets
	[InlineData("<a> rel=x")]                   // a parameter without its semicolon
	[InlineData("<a>; rel=x <b>; rel=y")]       // two values without a comma
	[InlineData("<a>; =x")]                     // a value without a name
	[InlineData("<a>; rel=\"x")]                // a quoted-string never closed
	[InlineData("<a b>; rel=x")]                // a space is no part of a URI-Reference
	[InlineData("<a>; rel=x;")]                 // a semicolon with nothing after it
	public void What_the_ABNF_does_not_make_is_refused(string field) =>
		Assert.False(Rfc8288.TryParseLinks(field).IsSuccess, $"'{field}' was read.");
}
