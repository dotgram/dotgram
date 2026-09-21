using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 5322's addresses, held to the example messages of its Appendix A and to is_email's test suite, in both
/// readings: a receiver's, §3 with §4, and the Strict one, §3 alone.
/// </summary>
/// <remarks>
/// <para>
/// <c>IsEmail/tests.xml</c> is dominicsayers/isemail at cfeefc3, test set 3.05, under the BSD licence beside it.
/// A test's address holds a control character as U+2400 plus its code, and DEL as U+2421, since XML cannot hold
/// them. Each test has a category: <c>ISEMAIL_ERR</c> is an address RFC 5322 does not make, <c>ISEMAIL_DEPREC</c>
/// one only its obsolete syntax makes, and the rest are addresses it makes and something else — RFC 5321, DNS —
/// has a view on.
/// </para>
/// <para>
/// is_email asks more than RFC 5322 does in three places, and the expectations here say where. A hyphen at the
/// edge of a domain label (tests 30, 31, 102) is RFC 1035's objection, not RFC 5322's, whose domain is a dot-atom
/// of atext. A comment or space beside <c>@</c> is deprecated by is_email but current syntax where it stands
/// before or after the whole local part or domain; only test 86, with space inside the domain, needs §4. And a
/// quoted-pair inside a domain literal (115 to 117) is obs-dtext, which is §4's, whatever category is_email
/// files it under.
/// </para>
/// </remarks>
public sealed class Rfc5322Tests
{
	// ── Appendix A ───────────────────────────────────────────────────────────────

	/// <summary>A.1.2: three forms of mailbox in one list, and a display name that has to be quoted.</summary>
	[Fact]
	public void Different_types_of_mailboxes()
	{
		Assert.Equal(
			new EmailAddress.Mailbox("Joe Q. Public", new AddrSpec("john.q.public", "example.com")),
			EmailAddress.Mailbox.ParseStrict("\"Joe Q. Public\" <john.q.public@example.com>"));

		Assert.Equal(
			[
				new EmailAddress.Mailbox("Mary Smith", new AddrSpec("mary", "x.test")),
				new EmailAddress.Mailbox(null, new AddrSpec("jdoe", "example.org")),
				new EmailAddress.Mailbox("Who?", new AddrSpec("one", "y.test")),
			],
			EmailAddress.ParseStrictList("Mary Smith <mary@x.test>, jdoe@example.org, Who? <one@y.test>"));

		Assert.Equal(
			[
				new EmailAddress.Mailbox(null, new AddrSpec("boss", "nil.test")),
				new EmailAddress.Mailbox("Giant; \"Big\" Box", new AddrSpec("sysservices", "example.net")),
			],
			EmailAddress.ParseStrictList("<boss@nil.test>, \"Giant; \\\"Big\\\" Box\" <sysservices@example.net>"));
	}

	/// <summary>A.1.3: a group of three, and a group of none.</summary>
	[Fact]
	public void Group_addresses()
	{
		var group = Assert.IsType<EmailAddress.Group>(Assert.Single(EmailAddress.ParseStrictList("A Group:Ed Jones <c@a.test>,joe@where.test,John <jdoe@one.test>;")));

		Assert.Equal("A Group", group.DisplayName);
		Assert.Equal(["Ed Jones <c@a.test>", "joe@where.test", "John <jdoe@one.test>"], group.Members.Select(member => member.ToString()).ToArray());

		Assert.Equal(new EmailAddress.Group("Undisclosed recipients", []), Assert.Single(EmailAddress.ParseStrictList("Undisclosed recipients:;")));
	}

	/// <summary>A.5: comments and folding white space nearly everywhere, all of it current syntax.</summary>
	[Fact]
	public void White_space_comments_and_other_oddities()
	{
		Assert.Equal(
			new EmailAddress.Mailbox("Pete", new AddrSpec("pete", "silly.test")),
			EmailAddress.Mailbox.ParseStrict("Pete(A nice \\) chap) <pete(his account)@silly.test(his host)>"));

		var to = "A Group(Some people)\r\n     :Chris Jones <c@(Chris's host.)public.example>,\r\n         joe@example.org,\r\n  John <jdoe@one.test> (my dear friend); (the end of the group)";

		Assert.Equal(
			new EmailAddress.Group("A Group",
			[
				new EmailAddress.Mailbox("Chris Jones", new AddrSpec("c", "public.example")),
				new EmailAddress.Mailbox(null, new AddrSpec("joe", "example.org")),
				new EmailAddress.Mailbox("John", new AddrSpec("jdoe", "one.test")),
			]),
			Assert.Single(EmailAddress.ParseStrictList(to)));

		Assert.Equal(
			new EmailAddress.Group("Hidden recipients", []),
			Assert.Single(EmailAddress.ParseStrictList("(Empty list)(start)Hidden recipients  :(nobody(that I know))  ;")));
	}

	/// <summary>A.6.1: a display name with a period unquoted, a route, a null member and space around a dot.</summary>
	[Fact]
	public void Obsolete_addressing()
	{
		Assert.Equal(
			new EmailAddress.Mailbox("Joe Q. Public", new AddrSpec("john.q.public", "example.com")),
			EmailAddress.Mailbox.Parse("Joe Q. Public <john.q.public@example.com>"));

		Assert.Equal(
			[
				new EmailAddress.Mailbox("Mary Smith", new AddrSpec("mary", "example.net")),
				new EmailAddress.Mailbox(null, new AddrSpec("jdoe", "test.example")),
			],
			EmailAddress.ParseList("Mary Smith <@node.test:mary@example.net>, , jdoe@test  . example"));

		Assert.False(EmailAddress.Mailbox.TryParseStrict("Joe Q. Public <john.q.public@example.com>", out _));
		Assert.False(EmailAddress.TryParseStrictList("Mary Smith <@node.test:mary@example.net>, , jdoe@test  . example", out _));
	}

	/// <summary>A.6.3: a comment inside a domain, and a folded line of nothing but white space.</summary>
	[Fact]
	public void Obsolete_white_space_and_comments()
	{
		Assert.Equal(new AddrSpec("jdoe", "machine.example"), EmailAddress.Mailbox.Parse("John Doe <jdoe@machine(comment).  example>").Address);
		Assert.Equal("Mary Smith", EmailAddress.Mailbox.Parse("Mary Smith\r\n  \r\n          <mary@example.net>").DisplayName);

		Assert.False(EmailAddress.Mailbox.TryParseStrict("John Doe <jdoe@machine(comment).  example>", out _));

		// The folded line of only white space is two FWS side by side — the word's trailing one and angle-addr's
		// leading one — which §3's ABNF allows; it is §3.2.2's prose that forbids the line, and a grammar does not
		// see lines.
		Assert.True(EmailAddress.Mailbox.TryParseStrict("Mary Smith\r\n  \r\n          <mary@example.net>", out _));
	}

	// ── What an address means ────────────────────────────────────────────────────

	[Theory]
	[InlineData("\"test\\ test\"@iana.org", "test test", "iana.org")]
	[InlineData("\"\"@iana.org", "", "iana.org")]
	[InlineData("(comment)test@iana.org(comment)", "test", "iana.org")]
	[InlineData("test@[RFC-5322-\\]-domain-literal]", "test", "[RFC-5322-]-domain-literal]")]
	[InlineData("test@[ 1.2.3.4 ]", "test", "[ 1.2.3.4 ]")]
	[InlineData("\"test\".test@iana.org", "test.test", "iana.org")]
	[InlineData("\"a b\" . c@x . y", "a b.c", "x.y")]
	public void An_addr_spec(string text, string localPart, string domain)
	{
		Assert.Equal(new AddrSpec(localPart, domain), AddrSpec.Parse(text));
	}

	[Theory]
	[InlineData("test@iana.org", "test@iana.org")]
	[InlineData("\"test\\ test\"@iana.org", "\"test test\"@iana.org")]
	[InlineData("\"test\".test@iana.org", "test.test@iana.org")]
	[InlineData("\"a\\\"b\"@x", "\"a\\\"b\"@x")]
	[InlineData("\"a..b\"@x", "\"a..b\"@x")]
	[InlineData("test@[RFC-5322-\\]-domain-literal]", "test@[RFC-5322-\\]-domain-literal]")]
	public void An_addr_spec_as_section_3_writes_it(string text, string written)
	{
		var spec = AddrSpec.Parse(text);

		Assert.Equal(written, spec.ToString());
		Assert.Equal(spec, AddrSpec.Parse(written));
	}

	[Theory]
	[InlineData("Joe Q. Public <john.q.public@example.com>", "\"Joe Q. Public\" <john.q.public@example.com>")]
	[InlineData("Who? <one@y.test>", "Who? <one@y.test>")]
	[InlineData("\"Giant; \\\"Big\\\" Box\" <sysservices@example.net>", "\"Giant; \\\"Big\\\" Box\" <sysservices@example.net>")]
	[InlineData("<boss@nil.test>", "boss@nil.test")]
	[InlineData("A Group:Ed Jones <c@a.test>,joe@where.test;", "A Group: Ed Jones <c@a.test>, joe@where.test;")]
	public void An_address_as_section_3_writes_it(string text, string written)
	{
		var address = Assert.Single(EmailAddress.ParseList(text));

		Assert.Equal(written, address.ToString());
		Assert.Equal(address, Assert.Single(EmailAddress.ParseStrictList(written)));
	}

	[Theory]
	[InlineData("")]
	[InlineData("a@b,")]
	[InlineData(",a@b")]
	[InlineData("a@b,,c@d")]
	[InlineData("<a@b")]
	[InlineData("a <b>")]
	[InlineData("Group:a@b")]
	public void What_is_no_strict_address_list(string text)
	{
		Assert.False(EmailAddress.TryParseStrictList(text, out _), $"'{text}' was read.");
	}

	// ── is_email ─────────────────────────────────────────────────────────────────

	/// <summary>Tests whose domain RFC 1035 objects to and RFC 5322 does not.</summary>
	static readonly HashSet<int> HostNameRules = [30, 31, 102];

	/// <summary>Tests only §4 makes that is_email files other than as deprecated, or deprecated tests §3 makes.</summary>
	static readonly HashSet<int> ObsoleteBeyondCategory = [86, 115, 116, 117];

	public static TheoryData<int> IsEmailTests()
	{
		var data = new TheoryData<int>();

		foreach (var test in Suite.Keys)
			data.Add(test);

		return data;
	}

	[Theory]
	[MemberData(nameof(IsEmailTests))]
	public void An_is_email_test(int id)
	{
		var (address, category, diagnosis) = Suite[id];

		var made     = category != "ISEMAIL_ERR" || HostNameRules.Contains(id);
		var obsolete = category == "ISEMAIL_DEPREC" && diagnosis is not ("ISEMAIL_DEPREC_CFWS_NEAR_AT" or "ISEMAIL_RFC5321_IPV6DEPRECATED")
			|| ObsoleteBeyondCategory.Contains(id);

		Assert.True(made == AddrSpec.TryParse(address, out _), $"{id} {diagnosis}: '{address}' read {(made ? "not " : "")}as an addr-spec.");
		Assert.True((made && !obsolete) == AddrSpec.TryParseStrict(address, out _), $"{id} {diagnosis}: '{address}' read {(made && !obsolete ? "not " : "")}as a Strict addr-spec.");
	}

	static readonly Dictionary<int, (string Address, string Category, string Diagnosis)> Suite = Read();

	static Dictionary<int, (string, string, string)> Read()
	{
		var tests = new Dictionary<int, (string, string, string)>();

		foreach (var test in XDocument.Load(Path.Combine(Path.GetDirectoryName(FilePath())!, "IsEmail", "tests.xml")).Root!.Elements("test"))
		{
			var address = new StringBuilder();

			foreach (var character in (string?)test.Element("address") ?? "")
				address.Append(character switch
				{
					>= '\u2400' and <= '\u241F' => (char)(character - '\u2400'),
					'\u2421'                    => '\u007F',
					_                           => character,
				});

			tests.Add((int)test.Attribute("id")!, (address.ToString(), (string)test.Element("category")!, (string)test.Element("diagnosis")!));
		}

		return tests;
	}

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
