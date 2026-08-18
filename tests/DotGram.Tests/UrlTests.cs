using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The URL grammar of docs/syntax.md §7.3, compiled and run against real URLs.
/// </summary>
/// <remarks>
/// <para>
/// The same file the snapshot is taken of, so the two cannot drift: one says the
/// generated code has not changed, this says the generated code is right.
/// </para>
/// <para>
/// A URL parser is the case the whole no-runtime decision was taken for — it is
/// essentially a regular expression, and nobody wants a dependency for one. It is also
/// a fair test of backtracking, because almost every part of it is a greedy run that
/// has to give characters back: a host name and a userinfo are made of the same
/// characters, and only the <c>@</c> that may or may not follow tells them apart.
/// </para>
/// </remarks>
public sealed class UrlTests
{
	[Theory]
	[InlineData("http://example.com")]
	[InlineData("https://example.com")]
	[InlineData("ftp://example.com")]
	[InlineData("https://example.com/")]
	[InlineData("https://example.com/a/b/c")]
	[InlineData("https://example.com:8080")]
	[InlineData("https://example.com:8080/path")]
	[InlineData("https://user@example.com")]
	[InlineData("https://user:secret@example.com:8080/path")]
	[InlineData("https://192.168.0.1")]
	[InlineData("https://192.168.0.1:443/x")]
	[InlineData("https://example.com/path?query=value")]
	[InlineData("https://example.com/path#fragment")]
	[InlineData("https://example.com/path?a=1&b=2#top")]
	[InlineData("https://example.com/%C3%A9")]
	[InlineData("https://example.com/a~b_c-d.e")]
	[InlineData("https://example.com?just=query")]
	[InlineData("https://example.com#just-fragment")]
	public void Parses(string url) => Assert.True(Match(url), url);

	[Theory]
	[InlineData("",                          "nothing at all")]
	[InlineData("example.com",               "no scheme")]
	[InlineData("gopher://example.com",      "a scheme the grammar does not list")]
	[InlineData("https:/example.com",        "one slash")]
	[InlineData("https://",                  "no host")]
	[InlineData("https://example.com:",      "a colon and no port")]
	[InlineData("https://example.com:80a",   "a port that is not a number")]
	[InlineData("https://exa mple.com",      "a space in the host")]
	[InlineData("https://example.com/%zz",   "a percent escape that is not hex")]
	[InlineData("https://example.com/a#b#c", "two fragments")]
	public void Refuses(string url, string why) => Assert.False(Match(url), why);

	[Theory]
	[InlineData("https://[2001:db8:85a3:8d3:1319:8a2e:370:7348]",   "eight groups, none elided")]
	[InlineData("https://[2001:db8::1]",                            "a run elided in the middle")]
	[InlineData("https://[::1]",                                    "the loopback")]
	[InlineData("https://[::]",                                     "all of it elided")]
	[InlineData("https://[1::]",                                    "elided at the end")]
	[InlineData("https://[::8]",                                    "elided at the start")]
	[InlineData("https://[1:2:3:4:5:6:7:8]",                        "the plain eight")]
	[InlineData("https://[1:2:3:4:5:6::8]",                         "one group elided")]
	[InlineData("https://[1:2:3:4:5::7:8]",                         "elided with groups after")]
	[InlineData("https://[fe80::1]:8080",                           "with a port after the bracket")]
	[InlineData("https://[::ffff:192.0.2.1]",                       "an IPv4 tail")]
	[InlineData("https://[64:ff9b::192.0.2.33]",                    "an IPv4 tail after groups")]
	[InlineData("https://[2001:db8::1]/a/b?q=1#f",                  "the rest of the URL still follows")]
	public void Parses_an_address_literal(string url, string what) => Assert.True(Match(url), what);

	[Theory]
	[InlineData("https://[2001:db8:85a3:8d3:1319:8a2e:370:7348:9]", "nine groups")]
	[InlineData("https://[1:2:3:4:5:6:7]",                          "seven groups and no elision")]
	[InlineData("https://[12345::1]",                               "five hex digits in a group")]
	[InlineData("https://[1:::2]",                                  "three colons")]
	[InlineData("https://[gggg::1]",                                "not hexadecimal")]
	[InlineData("https://[::1",                                     "no closing bracket")]
	[InlineData("https://2001:db8::1",                              "an address literal without its brackets")]
	public void Refuses_a_bad_address_literal(string url, string why) => Assert.False(Match(url), why);

	[Fact]
	public void The_elision_is_where_backtracking_earns_its_keep() =>
		// `(Group{0,5} & H16)? & "::" & H16` on `1::8`: the greedy run takes `1:`, then
		// needs another group and finds a colon, gives the group back, matches `1` as the
		// H16 instead, and only then does `::` line up. Every alternative before this one
		// was tried and given back whole.
		Assert.True(Match("https://[1::8]"));

	[Fact]
	public void The_scheme_prefix_does_not_shadow_the_longer_one() =>
		// "https" comes first in the grammar, but ordering it the other way would still
		// work: "http" would match, "://" would fail on the s, and the choice would be
		// asked for its next answer. Ordered choice is not first-wins-for-ever.
		Assert.True(Match("https://example.com"));

	[Fact]
	public void A_host_that_looks_like_userinfo_until_the_end_is_given_back() =>
		// UserInfo greedily eats "example.com" looking for an '@' that is not there, and
		// has to hand every character of it back for Host to have anything to match.
		Assert.True(Match("https://example.com"));

	[Fact]
	public void Finding_urls_inside_other_text()
	{
		var found = EmittedCode
			.Found(Compiled.Value, "Url", "AllUrls", "see http://a.io and https://b.io/c okay")
			.Select(url => Read(url, "Authority", "Host"));

		Assert.Equal(["a.io", "b.io"], found);
	}

	/// <summary>
	/// The captures of §7.3, read off the value the parser hands back.
	/// </summary>
	/// <remarks>
	/// By reflection here, because this file compiles the grammar at run time and the
	/// types did not exist when the test itself was compiled. <c>GeneratedApiTests</c>
	/// asks the same question of the compiler instead, over the same grammar.
	/// </remarks>
	[Theory]
	[InlineData("https://user:secret@example.com:8080/a/b?q=1#top", "Scheme",         "https")]
	[InlineData("https://user:secret@example.com:8080/a/b?q=1#top", "Authority.User", "user:secret")]
	[InlineData("https://user:secret@example.com:8080/a/b?q=1#top", "Authority.Host", "example.com")]
	[InlineData("https://user:secret@example.com:8080/a/b?q=1#top", "Authority.Port", "8080")]
	[InlineData("https://user:secret@example.com:8080/a/b?q=1#top", "Path",           "/a/b")]
	[InlineData("https://user:secret@example.com:8080/a/b?q=1#top", "Query",          "q=1")]
	[InlineData("https://user:secret@example.com:8080/a/b?q=1#top", "Fragment",       "top")]
	[InlineData("ftp://example.com",                                "Authority.Host", "example.com")]
	[InlineData("ftp://example.com",                                "Path",           "")]
	[InlineData("ftp://example.com",                                "Authority.User", null)]
	[InlineData("ftp://example.com",                                "Authority.Port", null)]
	[InlineData("ftp://example.com",                                "Query",          null)]
	[InlineData("https://[2001:db8::1]:99/x",                       "Authority.Host", "[2001:db8::1]")]
	[InlineData("https://192.168.0.1:443/x",                        "Authority.Port", "443")]
	public void The_parts_of_a_url_are_members_of_the_result(string url, string member, string? expected) =>
		Assert.Equal(expected, Read(Invoke("ParseUrl", url).Value, member.Split('.')));

	[Fact]
	public void A_capture_the_parser_gave_back_is_not_in_the_result() =>
		// UserInfo eats "example.com" looking for an '@' that is not there. Giving the
		// characters back has to give the capture back with them — the state the match
		// resumes at clears every slot the abandoned attempt could have written.
		Assert.Null(Read(Invoke("ParseUrl", "https://example.com").Value, "Authority", "User"));

	static object? Read(object? value, params string[] path)
	{
		foreach (var name in path)
			value = value?.GetType().GetProperty(name)?.GetValue(value);

		return value;
	}

	[Fact]
	public void A_host_can_reconsider_ipv4_as_a_registered_name()
	{
		// Host tries IPv4 first and matches "1.2.3.4". When Url cannot consume ".5",
		// ordinary rule calls remain transparent and Host can take RegName instead.
		Assert.True(Match("https://1.2.3.4.5"));

		// The same address is fine when nothing tempts IPv4 first.
		Assert.True(Match("https://1.2.3.4"));
	}

	static bool Match(string url) => Invoke("ParseUrl", url).Matched;

	static (bool Matched, object? Value) Invoke(string method, string input)
	{
		var (isSuccess, value, _, _) = EmittedCode.Match(Compiled.Value, "Url", "Try" + method, input);

		return (isSuccess, value);
	}

	/// <summary>Compiled once: the grammar is the same for every case in the file.</summary>
	static readonly Lazy<System.Reflection.Assembly> Compiled = new(() =>
	{
		var grammar = File.ReadAllText(Path.Combine(
			Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!, "Snapshots", "Url.gram"));

		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Url", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.Empty(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, "Url");
	});

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
