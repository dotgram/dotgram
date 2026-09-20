using System;
using System.Collections.Generic;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// Every example of the package's two shipped pages, run rather than read.
/// </summary>
/// <remarks>
/// <para>
/// The same defect that reached DotGram.Finance's pages can reach these: an example is prose, so
/// nothing holds it to the package, and a decision can change what a call does without touching
/// the page that demonstrates it. README.md makes its calls in fourteen fenced blocks and SKILL.md
/// in two, and until now nothing compiled any of them.
/// </para>
/// <para>
/// These assert the values the pages claim in their comments, not merely that the calls compile —
/// a comment saying <c>0.7</c> beside a call that returns <c>0.3</c> misleads a reader exactly as
/// far as a call that throws. Where a page shows a value it cannot claim exactly, the test asserts
/// what the page could have written.
/// </para>
/// </remarks>
public sealed class WebShippedExampleTests
{
	[Fact]
	public void Json_and_its_exact_numbers()
	{
		var value = (JsonValue.Object)JsonValue.Parse("""{ "pi": 3.14159265358979323846, "tags": ["a", "b"] }""");

		var pi = (JsonValue.Number)value.Members[0].Value;

		Assert.Equal("3.14159265358979323846", pi.Text);
		Assert.Equal(3.141592653589793, pi.ToDouble());
		Assert.Equal("""{"pi":3.14159265358979323846,"tags":["a","b"]}""", value.ToString());
	}

	[Fact]
	public void Json_pointers()
	{
		var document = JsonValue.Parse("""{ "foo": ["bar", "baz"], "a/b": 1 }""");

		Assert.Equal("baz", ((JsonValue.String)JsonPointer.Parse("/foo/1").Resolve(document)!).Value);
		Assert.Equal("1", ((JsonValue.Number)JsonPointer.Parse("/a~1b").Resolve(document)!).Text);
		Assert.Null(JsonPointer.Parse("/foo/-").Resolve(document));

		var pointer = JsonPointer.Parse("/a~1b/0");

		Assert.Equal(["a/b", "0"], pointer.Tokens);
		Assert.Equal("#/a~1b/0", pointer.ToUriFragment());
		Assert.Equal(["c%d"], JsonPointer.ParseFragment("#/c%25d").Tokens);
	}

	[Fact]
	public void Json_patch()
	{
		var patch = JsonPatch.Parse("""
			[
			  { "op": "test", "path": "/a/b/c", "value": "foo" },
			  { "op": "replace", "path": "/a/b/c", "value": 42 },
			  { "op": "copy", "from": "/a/b/c", "path": "/a/b/d" }
			]
			""");

		Assert.Equal("""{"a":{"b":{"c":42,"d":42}}}""",
			patch.Apply(JsonValue.Parse("""{ "a": { "b": { "c": "foo" } } }""")).ToString());
	}

	[Fact]
	public void Media_types_and_the_quality_of_an_accept_field()
	{
		var type = MediaType.Parse("Text/HTML; Charset=\"UTF-8\"");

		Assert.Equal("UTF-8", type.Charset);
		Assert.True(type == MediaType.Parse("text/html;charset=utf-8"));

		var accept = MediaRange.ParseAccept("text/*;q=0.3, text/plain;q=0.7, */*;q=0.5");

		Assert.Equal(0.7m, MediaRange.Quality(accept, MediaType.Parse("text/plain")));
		Assert.Equal(0.3m, MediaRange.Quality(accept, MediaType.Parse("text/html")));
		Assert.Equal(0.5m, MediaRange.Quality(accept, MediaType.Parse("image/png")));
	}

	[Fact]
	public void Structured_fields()
	{
		var list = StructuredField.ParseList("text/html;q=1.0, (\"a\" \"b\");lvl=5");

		var item = (Item)list[0];

		Assert.Equal("text/html", ((BareItem.Token)item.Value).Value);
		Assert.Equal(1.0m, ((BareItem.Decimal)item.Parameters["q"]).Value);

		var dictionary = StructuredField.ParseDictionary("u=3, i");

		Assert.True(((BareItem.Boolean)((Item)dictionary["i"]).Value).Value);
		Assert.Equal("u=3, i", StructuredField.SerializeDictionary(dictionary));
	}

	[Fact]
	public void Web_links()
	{
		var links = WebLink.ParseField(
			"</TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel, " +
			"</TheBook/chapter4>; rel=\"next\"; title=\"next chapter\"");

		Assert.Equal("/TheBook/chapter2", links[0].Target);
		Assert.Equal(["previous"], links[0].Relations);
		Assert.Equal("letztes Kapitel", links[0].Title);
		Assert.Equal("next chapter", links[1].Title);
	}

	[Fact]
	public void Content_disposition()
	{
		var field = ContentDisposition.Parse(
			"attachment; filename=\"EURO rates\"; filename*=utf-8''%e2%82%ac%20rates");

		Assert.True(field.IsAttachment);
		Assert.Equal("€ rates", field.Filename);
		Assert.Equal("EURO rates", field.Find("filename")!.Value);
	}

	[Fact]
	public void Cookies()
	{
		var cookie = SetCookie.Parse("SID=31d4d96e407aad42; Path=/; Max-Age=3600; Secure; HttpOnly");

		Assert.Equal("SID", cookie.Name);
		Assert.Equal("/", cookie.Path);
		Assert.True(cookie.Secure);

		var now = DateTimeOffset.UtcNow;

		Assert.Equal(now.AddHours(1), cookie.ExpiryTime(now)!.Value, TimeSpan.FromSeconds(1));

		Assert.Equal(new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero),
			CookieDate.Parse("Sun, 06-Nov-94 08:49:37 GMT"));

		Assert.Equal(2, CookiePair.ParseField("SID=31d4d96e407aad42; lang=en-US").Length);
	}

	[Fact]
	public void Forwarded_elements()
	{
		var elements = ForwardedElement.ParseField(
			"for=192.0.2.43, for=\"[2001:db8:cafe::17]:4711\";by=_hidden;proto=https;host=example.com");

		Assert.Equal("192.0.2.43", elements[0].For!.Name);
		Assert.Equal(ForwardedNode.Kinds.IPv6, elements[1].For!.Kind);
		Assert.Equal(4711, elements[1].For!.PortNumber);
		Assert.Equal(ForwardedNode.Kinds.Obfuscated, elements[1].By!.Kind);
		Assert.Equal("https", elements[1].Proto);
	}

	[Fact]
	public void Uri_references()
	{
		var uri = UriReference.ParseUri("https://user@example.com:8080/a/b?q=1#top");

		Assert.Equal("https", uri.Scheme);
		Assert.Equal("example.com", uri.Host);
		Assert.Equal("8080", uri.Port);
		Assert.Equal("/a/b", uri.Path);
		Assert.Equal("q=1", uri.Query);
		Assert.Equal("top", uri.Fragment);

		Assert.Equal("../images/logo.png", UriReference.Parse("../images/logo.png?size=2").Path);
		Assert.Equal("hello world", UriReference.Decode("hello%20world"));
	}

	[Fact]
	public void Uri_templates()
	{
		var template = UriTemplate.Parse("/users{/id}{?fields,page:3}{&tags*}");

		Assert.Equal("/users/igor?fields=name,email&page=123&tags=a&tags=b",
			template.Expand(new Dictionary<string, object?>
			{
				["id"]     = "igor",
				["fields"] = new[] { "name", "email" },
				["page"]   = "12345",
				["tags"]   = new[] { "a", "b" },
			}));
	}

	[Fact]
	public void Email_addresses()
	{
		var list = EmailAddress.ParseList(
			"\"Joe Q. Public\" <john.q.public@example.com>, jdoe@example.org, Undisclosed recipients:;");

		var joe = (EmailAddress.Mailbox)list[0];

		Assert.Equal("Joe Q. Public", joe.DisplayName);
		Assert.Equal("john.q.public", joe.Address.LocalPart);
		Assert.Equal("example.com", joe.Address.Domain);

		var group = (EmailAddress.Group)list[2];

		Assert.Empty(group.Members);

		Assert.Equal("john smith", AddrSpec.Parse("(comment)\"john smith\"@example.com").LocalPart);
		Assert.False(AddrSpec.TryParseStrict("john . smith@example.com", out _));
	}

	[Fact]
	public void Timestamps()
	{
		var timestamp = Timestamp.Parse("1996-12-19T16:39:57-08:00");

		Assert.Equal(1996, timestamp.Date.Year);
		Assert.Equal(12, timestamp.Date.Month);
		Assert.Equal(19, timestamp.Date.Day);
		Assert.Equal(TimeSpan.FromHours(-8), timestamp.Time.Offset);
		Assert.Equal(new DateTimeOffset(1996, 12, 19, 16, 39, 57, TimeSpan.FromHours(-8)),
			timestamp.ToDateTimeOffset());

		Assert.Equal(29, FullDate.Parse("2020-02-29").Day);
		Assert.False(FullDate.TryParse("2021-02-29", out _));
		Assert.Equal(60, FullTime.Parse("15:59:60-08:00").Second);
	}

	[Fact]
	public void Language_tags()
	{
		var tag = LanguageTag.Parse("zh-cmn-Hans-CN-u-ca-chinese");

		Assert.Equal("zh", tag.Language);
		Assert.Equal(["cmn"], tag.ExtendedLanguages);
		Assert.Equal("Hans", tag.Script);
		Assert.Equal("CN", tag.Region);
		Assert.Equal('u', tag.Extensions[0].Singleton);
		Assert.Equal(["ca", "chinese"], tag.Extensions[0].Subtags);

		Assert.Equal("en-Latn-US", LanguageTag.Parse("EN-latn-us").ToString());
		Assert.Equal("i-klingon", LanguageTag.Parse("i-klingon").Grandfathered);
	}

	[Fact]
	public void The_skill_page_on_exact_numbers_and_on_not_throwing()
	{
		var document = (JsonValue.Object)JsonValue.Parse("""{ "price": 19.999999999999999999, "tags": ["a"] }""");
		var price    = (JsonValue.Number)document.Members[0].Value;

		Assert.Equal("19.999999999999999999", price.Text);
		Assert.Equal(20, price.ToDouble());
		Assert.True(price.TryToDecimal(out var amount));
		Assert.Equal(19.999999999999999999m, amount);

		var untrusted = "{ not json";

		Assert.False(JsonValue.TryParse(untrusted, out _));
	}

	[Fact]
	public void The_skill_page_on_strictness_and_quality()
	{
		Assert.False(AddrSpec.TryParseStrict("john . smith@example.com", out _));

		var accept = MediaRange.ParseAccept("text/*;q=0.3, text/plain;q=0.7, */*;q=0.5");

		Assert.Equal(0.7m, MediaRange.Quality(accept, MediaType.Parse("text/plain")));
	}
}
