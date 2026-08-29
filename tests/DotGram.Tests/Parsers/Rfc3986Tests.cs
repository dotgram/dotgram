using System;

using DotGram.Parsers;

using Xunit;

namespace DotGram.Tests.Parsers;

/// <summary>
/// RFC 3986, read against the specification's own examples.
/// </summary>
/// <remarks>
/// The material here is the RFC's: §1.1.2 for the whole references, §5.4.1 for the
/// relative ones, §3.2.2 for the host forms. A parser that claims a specification is
/// worth exactly what it does with the examples that specification chose to give.
/// </remarks>
public sealed class Rfc3986Tests
{
	static UriParts Parsed(string text) => Rfc3986.ParseReference(text);

	// ── §1.1.2, the examples the RFC leads with ─────────────────────────────────

	[Fact]
	public void A_reference_comes_apart_where_the_specification_says_it_does() =>
		Assert.Equal(
			new UriParts("ftp", null, "ftp.is.co.za", null, "/rfc/rfc1808.txt", null, null),
			Parsed("ftp://ftp.is.co.za/rfc/rfc1808.txt"));

	[Fact]
	public void And_a_bracketed_address_is_a_host_like_any_other() =>
		Assert.Equal(
			new UriParts("ldap", null, "[2001:db8::7]", null, "/c=GB", "objectClass?one", null),
			Parsed("ldap://[2001:db8::7]/c=GB?objectClass?one"));

	[Fact]
	public void And_a_reference_with_no_authority_has_none_rather_than_an_empty_one() =>
		// `mailto:` and `urn:` are the shapes that catch a parser which assumes `//`:
		// there is no authority at all, and the path is everything after the colon.
		Assert.Equal(
			[
				new UriParts("mailto", null, null, null, "John.Doe@example.com", null, null),
				new UriParts("news", null, null, null, "comp.infosystems.www.servers.unix", null, null),
				new UriParts("tel", null, null, null, "+1-816-555-1212", null, null),
				new UriParts("urn", null, null, null, "oasis:names:specification:docbook:dtd:xml:4.1.2", null, null),
			],
			new[]
			{
				"mailto:John.Doe@example.com",
				"news:comp.infosystems.www.servers.unix",
				"tel:+1-816-555-1212",
				"urn:oasis:names:specification:docbook:dtd:xml:4.1.2",
			}
				.Select(Parsed));

	[Fact]
	public void And_a_port_is_the_digits_after_the_colon() =>
		Assert.Equal(
			new UriParts("telnet", null, "192.0.2.16", "80", "/", null, null),
			Parsed("telnet://192.0.2.16:80/"));

	// ── §4.2, references that are not URIs ──────────────────────────────────────

	[Theory]
	[InlineData("g",       null, "g")]
	[InlineData("./g",     null, "./g")]
	[InlineData("g/",      null, "g/")]
	[InlineData("/g",      null, "/g")]
	[InlineData("..",      null, "..")]
	[InlineData("../../g", null, "../../g")]
	[InlineData(";x",      null, ";x")]
	[InlineData("",        null, "")]
	public void A_relative_reference_has_a_path_and_no_scheme(string text, string? scheme, string path)
	{
		var parts = Parsed(text);

		Assert.Equal(scheme, parts.Scheme);
		Assert.Equal(path, parts.Path);
		Assert.Null(parts.Host);
	}

	[Fact]
	public void And_a_leading_double_slash_is_an_authority_even_with_no_scheme() =>
		Assert.Equal(new UriParts(null, null, "g", null, "", null, null), Parsed("//g"));

	[Fact]
	public void And_a_query_or_a_fragment_may_stand_alone() =>
		Assert.Equal(
			[
				new UriParts(null, null, null, null, "", "y", null),
				new UriParts(null, null, null, null, "", null, "s"),
			],
			new[] { "?y", "#s" }.Select(Parsed));

	[Fact]
	public void And_a_first_segment_holding_a_colon_is_a_scheme_unless_something_precedes_it() =>
		// `path-noscheme` is the rule that says so: `a:b` is a URI, and the same path
		// written where a scheme cannot begin is a relative reference.
		Assert.Equal(
			[
				new UriParts("a", null, null, null, "b", null, null),
				new UriParts(null, null, null, null, "./a:b", null, null),
			],
			new[] { "a:b", "./a:b" }.Select(Parsed));

	// ── §3.2.2, what a host may be ──────────────────────────────────────────────

	[Theory]
	[InlineData("http://example.com/",        "example.com")]
	[InlineData("http://192.0.2.16/",         "192.0.2.16")]
	[InlineData("http://[2001:db8::7]/",      "[2001:db8::7]")]
	[InlineData("http://[::1]/",              "[::1]")]
	[InlineData("http://[v7.host:port]/",     "[v7.host:port]")]
	[InlineData("file:///etc/hosts",          "")]
	public void A_host_is_a_literal_an_address_or_a_name(string text, string host) =>
		Assert.Equal(host, Parsed(text).Host);

	[Fact]
	public void And_an_octet_is_the_specification_own_and_not_three_digits() =>
		// The corpus grammar writes an octet as `Digit{1,3}` and so reads `999.1.1.1` as
		// an address. Here `256` is not an octet, so the whole host falls through to a
		// registered name — which is what the RFC says it is.
		Assert.Equal(
			["192.0.2.16", "255.255.255.255", "256.1.1.1", "1.2.3.4.5"],
			new[]
			{
				"http://192.0.2.16/", "http://255.255.255.255/",
				"http://256.1.1.1/", "http://1.2.3.4.5/",
			}
				.Select(text => Parsed(text).Host));

	[Fact]
	public void And_a_port_may_be_empty_which_is_not_the_same_as_absent() =>
		Assert.Equal(
			new string?[] { "", null },
			new[] { "http://h:/", "http://h/" }.Select(text => Parsed(text).Port));

	[Fact]
	public void And_so_may_a_query_and_a_fragment() =>
		// `?` with nothing after it is a query of no characters; no `?` at all is no
		// query. RFC 3986 §3.4 keeps them apart and so does this.
		Assert.Equal(
			new string?[] { "", null, "", null },
			new string?[]
			{
				Parsed("http://h/?").Query,
				Parsed("http://h/").Query,
				Parsed("http://h/#").Fragment,
				Parsed("http://h/").Fragment,
			});

	// ── What is not a reference ─────────────────────────────────────────────────

	[Theory]
	[InlineData("http://exa mple.com/")]   // a space is not a URI character
	[InlineData("http://h/%zz")]           // a percent escape is two hex digits
	[InlineData("1http://h/")]             // a scheme begins with a letter
	public void A_text_that_is_not_a_reference_is_refused(string text) =>
		Assert.False(Rfc3986.TryParseReference(text).IsSuccess);

	// ── §2.4, decoding, which is the caller's to ask for ────────────────────────

	[Theory]
	[InlineData("a%20b",           "a b")]
	[InlineData("%D0%9F",          "П")]
	[InlineData("nothing",         "nothing")]
	[InlineData("a%2Fb",           "a/b")]
	public void An_escape_is_decoded_where_a_caller_asks_and_not_before(string part, string decoded) =>
		// Not while parsing, and §2.4 says why: `%2F` in a segment is a slash that is not
		// a separator, and a path decoded on the way in cannot be taken apart again.
		Assert.Equal(decoded, Rfc3986.Decode(part));

	[Fact]
	public void And_the_parts_come_back_as_they_were_written() =>
		Assert.Equal("/a%2Fb", Parsed("http://h/a%2Fb").Path);
}
