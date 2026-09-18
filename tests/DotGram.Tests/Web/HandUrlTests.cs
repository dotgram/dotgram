using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// <see cref="DotGram.Handwritten.Web.HandUrl"/> held to the generated RFC 3986 parser: the
/// same parts, the same refusals, refused at the same place (D1).
/// </summary>
/// <remarks>
/// <see cref="Rfc3986Tests"/> reads every one of its texts through <see cref="Both"/> already;
/// this adds references chosen for the places two readers part вЂ” the host forms, IPv6's
/// counting, escapes, the four path forms вЂ” and each of them taken apart one character at a
/// time, which is where two readers stop in different places.
/// </remarks>
public sealed class HandUrlTests
{
	public static readonly string[] Corpus =
	[
		"ftp://ftp.is.co.za/rfc/rfc1808.txt",
		"http://www.ietf.org/rfc/rfc2396.txt",
		"ldap://[2001:db8::7]/c=GB?objectClass?one",
		"mailto:John.Doe@example.com",
		"news:comp.infosystems.www.servers.unix",
		"tel:+1-816-555-1212",
		"telnet://192.0.2.16:80/",
		"urn:oasis:names:specification:docbook:dtd:xml:4.1.2",
		"http://a/b/c/d;p?q",
		"g:h", "g", "./g", "g/", "/g", "//g", "?y", "g?y", "#s", "g#s", "g?y#s", ";x", "g;x", "g;x?y#s", "", ".", "./", "..", "../", "../g", "../..", "../../g",
		"http://user:pass@host:8080/p/a/t/h?query=1&b=2#frag",
		"http://h:/", "http://h/?", "http://h/#", "http://h/a%2Fb", "http://h/%zz", "http://h/%4", "http://exa mple.com/",
		"file:///etc/hosts", "a:b", "./a:b", "a:b:c", "1http://h/", "//@h", "//u@h@x",
		"http://192.0.2.16/", "http://255.255.255.255/", "http://256.1.1.1/", "http://1.2.3.4.5/", "http://01.2.3.4/",
		"http://[::1]/", "http://[::]/", "http://[1::]/", "http://[::ffff:192.0.2.1]/", "http://[1:2:3:4:5:6:7:8]/",
		"http://[1:2:3:4:5:6:1.2.3.4]/", "http://[1:2:3:4:5:6:7::]/", "http://[::2:3:4:5:6:7:8]/", "http://[1::8]/",
		"http://[1:2:3:4:5::1.2.3.4]/", "http://[1:2:3:4:5:6:7:8:9]/", "http://[1:::2]/", "http://[1::2::3]/", "http://[12345::]/",
		"http://[::1.2.3.256]/", "http://[::1.2.3]/", "http://[1:2:3:4:5:6:7]/", "http://[:1]/", "http://[v7.host:port]/", "http://[v.x]/",
		"http://[vA1.]/", "http://[1::2]x/", "http://[::1]:8080/p", "HTTP://EXAMPLE.COM/", "s+1-.:rest",
	];

	public static TheoryData<string> Texts => [.. Corpus];

	[Theory]
	[MemberData(nameof(Texts))]
	public void Every_reference_is_answered_alike(string text)
	{
		Both.AgreeOnReference(text);
		Both.AgreeOnUri(text);
	}

	[Fact]
	public void And_alike_on_every_text_one_character_short_of_one()
	{
		var told = new List<string>();

		foreach (var reference in Corpus)
			foreach (var text in Mutations(reference))
				try
				{
					Both.AgreeOnReference(text);
					Both.AgreeOnUri(text);
				}
				catch (Exception disagreed) when (disagreed is not OutOfMemoryException)
				{
					told.Add(disagreed.Message);
				}

		Assert.True(told.Count == 0, $"{told.Count} texts told the two apart; the first:\n\n" + string.Join("\n\n", told.Take(8)));
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
