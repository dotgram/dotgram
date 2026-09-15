using System;
using System.Linq;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 7239's Forwarded header field, held to the examples of its §4, §6 and §7 and to the rules §4 to §6 give.
/// </summary>
/// <remarks>
/// There is no shared test suite for the Forwarded header field, so the material is the RFC: every example,
/// with its line breaks unfolded to the single space a field value carries, and each rule that says what a
/// value is.
/// </remarks>
public sealed class Rfc7239Tests
{
	// ── §4 ───────────────────────────────────────────────────────────────────────

	[Fact]
	public void An_obfuscated_client()
	{
		var element = Assert.Single(ForwardedElement.ParseField("for=\"_gazonk\""));

		Assert.Equal(new ForwardedNode(ForwardedNode.Kinds.Obfuscated, "_gazonk", null), element.For);
	}

	[Fact]
	public void An_IPv6_client_with_a_port()
	{
		var element = Assert.Single(ForwardedElement.ParseField("For=\"[2001:db8:cafe::17]:4711\""));

		Assert.Equal(new ForwardedNode(ForwardedNode.Kinds.IPv6, "2001:db8:cafe::17", "4711"), element.For);
		Assert.Equal(4711, element.For!.PortNumber);
		Assert.Equal("[2001:db8:cafe::17]:4711", element.For.ToString());
	}

	[Fact]
	public void Three_parameters_in_one_element()
	{
		var element = Assert.Single(ForwardedElement.ParseField("for=192.0.2.60;proto=http;by=203.0.113.43"));

		Assert.Equal(new ForwardedNode(ForwardedNode.Kinds.IPv4, "192.0.2.60", null), element.For);
		Assert.Equal("http", element.Proto);
		Assert.Equal(new ForwardedNode(ForwardedNode.Kinds.IPv4, "203.0.113.43", null), element.By);
		Assert.Null(element.Host);
	}

	[Fact]
	public void Two_elements()
	{
		var elements = ForwardedElement.ParseField("for=192.0.2.43, for=198.51.100.17");

		Assert.Equal(["192.0.2.43", "198.51.100.17"], elements.Select(element => element.For!.Name).ToArray());
	}

	// ── §6 ───────────────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("192.0.2.43:47011", ForwardedNode.Kinds.IPv4, "192.0.2.43", "47011")]
	[InlineData("[2001:db8:cafe::17]:47011", ForwardedNode.Kinds.IPv6, "2001:db8:cafe::17", "47011")]
	[InlineData("unknown", ForwardedNode.Kinds.Unknown, "unknown", null)]
	[InlineData("UNKNOWN:_port", ForwardedNode.Kinds.Unknown, "unknown", "_port")]
	[InlineData("_hidden", ForwardedNode.Kinds.Obfuscated, "_hidden", null)]
	[InlineData("_SEVKISEK:_a.b-c", ForwardedNode.Kinds.Obfuscated, "_SEVKISEK", "_a.b-c")]
	[InlineData("[::ffff:192.0.2.1]", ForwardedNode.Kinds.IPv6, "::ffff:192.0.2.1", null)]
	[InlineData("255.255.255.255:0", ForwardedNode.Kinds.IPv4, "255.255.255.255", "0")]
	[InlineData("10.0.0.1:99999", ForwardedNode.Kinds.IPv4, "10.0.0.1", "99999")]
	public void A_node(string text, ForwardedNode.Kinds kind, string name, string? port)
	{
		Assert.Equal(new ForwardedNode(kind, name, port), ForwardedNode.Parse(text));
	}

	[Theory]
	[InlineData("hidden")]
	[InlineData("_")]
	[InlineData("256.0.0.1")]
	[InlineData("192.0.2.1:")]
	[InlineData("192.0.2.1:123456")]
	[InlineData("192.0.2.1:port")]
	[InlineData("2001:db8::1")]
	[InlineData("[2001:db8::1")]
	[InlineData("[v1.fe]")]
	[InlineData("_a b")]
	[InlineData("")]
	public void What_is_no_node(string text)
	{
		Assert.False(ForwardedNode.TryParse(text, out _), $"'{text}' was read.");
	}

	[Fact]
	public void Obfuscated_identifiers()
	{
		var elements = ForwardedElement.ParseField("for=_hidden, for=_SEVKISEK");

		Assert.All(elements, element => Assert.Equal(ForwardedNode.Kinds.Obfuscated, element.For!.Kind));
		Assert.All(elements, element => Assert.Null(element.For!.PortNumber));
	}

	// ── §7 ───────────────────────────────────────────────────────────────────────

	/// <summary>§7.1: whitespace between the elements makes no difference, and nor does splitting them over fields.</summary>
	[Fact]
	public void Lists_that_are_equivalent()
	{
		var tight  = ForwardedElement.ParseField("for=192.0.2.43,for=\"[2001:db8:cafe::17]\",for=unknown");
		var spaced = ForwardedElement.ParseField("for=192.0.2.43, for=\"[2001:db8:cafe::17]\", for=unknown");
		var split  = ForwardedElement.ParseField("for=192.0.2.43").Concat(ForwardedElement.ParseField("for=\"[2001:db8:cafe::17]\", for=unknown")).ToArray();

		Assert.Equal(tight, spaced);
		Assert.Equal(tight, split);
	}

	/// <summary>§7.4: X-Forwarded-For converted.</summary>
	[Fact]
	public void From_X_Forwarded_For()
	{
		var elements = ForwardedElement.ParseField("for=192.0.2.43, for=\"[2001:db8:cafe::17]\"");

		Assert.Equal([ForwardedNode.Kinds.IPv4, ForwardedNode.Kinds.IPv6], elements.Select(element => element.For!.Kind).ToArray());
	}

	/// <summary>§7.5: what the second proxy sends on.</summary>
	[Fact]
	public void The_example_usage()
	{
		var elements = ForwardedElement.ParseField("for=192.0.2.43, for=198.51.100.17;by=203.0.113.60;proto=http;host=example.com");

		Assert.Equal(2, elements.Length);
		Assert.Equal("198.51.100.17", elements[1].For!.Name);
		Assert.Equal("203.0.113.60", elements[1].By!.Name);
		Assert.Equal("http", elements[1].Proto);
		Assert.Equal("example.com", elements[1].Host);
	}

	// ── What a field is ──────────────────────────────────────────────────────────

	[Fact]
	public void Empty_slots_and_elements_are_left_out()
	{
		var elements = ForwardedElement.ParseField(" , ;for=_a;; ,, ;;, by=_b; ");

		Assert.Equal(2, elements.Length);
		Assert.Equal([new ForwardedElement.Pair("for", "_a")], elements[0].Pairs);
		Assert.Equal([new ForwardedElement.Pair("by", "_b")], elements[1].Pairs);
	}

	[Fact]
	public void Extension_parameters_and_hosts()
	{
		var element = Assert.Single(ForwardedElement.ParseField("secret=\"a b\";host=\"example.com:8080\";proto=coap+tcp"));

		Assert.Equal("a b", element.Find("SECRET"));
		Assert.Equal("example.com:8080", element.Host);
		Assert.Equal("coap+tcp", element.Proto);
		Assert.Null(element.For);
	}

	[Fact]
	public void Equal_whatever_the_case_of_names()
	{
		var one   = Assert.Single(ForwardedElement.ParseField("for=_a;proto=https"));
		var other = Assert.Single(ForwardedElement.ParseField("FOR=\"_a\";Proto=https"));

		Assert.Equal(one, other);
		Assert.Equal(one.GetHashCode(), other.GetHashCode());
		Assert.NotEqual(one, Assert.Single(ForwardedElement.ParseField("for=_A;proto=https")));
	}

	[Theory]
	[InlineData("for=\"[2001:db8:cafe::17]:4711\";by=_hidden;host=\"example.com:80\";ext=\"a\\\\b\\\"c\"")]
	[InlineData("for=unknown, for=\"192.0.2.1:_p\"")]
	public void Written_back_and_read_again(string field)
	{
		var elements = ForwardedElement.ParseField(field);

		Assert.Equal(elements, ForwardedElement.ParseField(string.Join(", ", elements.Select(element => element.ToString()))));
	}

	[Theory]
	[InlineData("")]
	[InlineData(" , ; ,")]
	[InlineData("for=_a;For=_b")]
	[InlineData("for=192.0.2.43:80")]
	[InlineData("for=[2001:db8::1]")]
	[InlineData("for=hidden")]
	[InlineData("for=\"256.1.1.1\"")]
	[InlineData("by=\"_\"")]
	[InlineData("host=\"user@example.com\"")]
	[InlineData("host=\"example.com/path\"")]
	[InlineData("proto=1http")]
	[InlineData("proto=\"\"")]
	[InlineData("for=_a ;by=_b")]
	[InlineData("for=_a; by=_b")]
	[InlineData("for = _a")]
	[InlineData("for")]
	[InlineData("for=_a by=_b")]
	[InlineData("for=\"_a")]
	public void What_is_no_field(string field)
	{
		Assert.False(ForwardedElement.TryParseField(field, out _), $"'{field}' was read.");
	}
}
