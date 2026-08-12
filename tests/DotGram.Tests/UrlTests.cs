using System;
using System.IO;
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
		var found = (string[])Invoke("AllUrls", "see http://a.io and https://b.io/c okay").Value;

		Assert.Equal(["http://a.io", "https://b.io/c"], found);
	}

	[Fact]
	public void An_address_with_five_parts_is_where_rule_boundaries_show()
	{
		// Host tries IPv4 first, matches "1.2.3.4", and Url then cannot consume ".5".
		// Backtracking would go back and take RegName instead — and does not, because it
		// does not cross a rule boundary. Recorded here rather than left to be found:
		// docs/status.md says the same thing in prose.
		Assert.False(Match("https://1.2.3.4.5"));

		// The same address is fine when nothing tempts IPv4 first.
		Assert.True(Match("https://1.2.3.4"));
	}

	static bool Match(string url) => (bool)Invoke("ParseUrl", url).Matched;

	static (bool Matched, object Value) Invoke(string method, string input)
	{
		var type      = Compiled.Value.GetType("Url")!;
		var arguments = new object?[] { input, null, null, null };
		var matched   = (bool)type.GetMethod("Try" + method)!.Invoke(null, arguments)!;

		return (matched, arguments[1]!);
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
