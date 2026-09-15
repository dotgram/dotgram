using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 6265's Set-Cookie and Cookie header fields, held to the http-state working group's cases, to the
/// conversation of its §3.1, and to the steps §5.1 to §5.4 give.
/// </summary>
/// <remarks>
/// <para>
/// What the cases and §3.1's example show is a store: which cookies a request gets back. The <see cref="Jar"/>
/// here is §5.3 and §5.4 cut to what they ask — a public suffix list of one rule, no eviction, no non-HTTP API —
/// so that each can be replayed through what this package reads.
/// </para>
/// <para>
/// <c>HttpState/parser</c> is <c>tests/data/parser</c> of abarth/http-state at 155e45c, the IETF httpstate working
/// group's repository, copied byte for byte with its README. It carries no licence: what is in it is an IETF
/// Contribution, under the Note Well its README names. The cases the suite itself disables are left out.
/// </para>
/// </remarks>
public sealed class Rfc6265Tests
{
	// ── §3.1 ─────────────────────────────────────────────────────────────────────

	[Fact]
	public void The_conversation_of_section_3_1()
	{
		var jar = new Jar(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));
		var uri = new Uri("https://www.example.com/");

		jar.Receive(uri, "SID=31d4d96e407aad42");
		Assert.Equal("SID=31d4d96e407aad42", jar.CookieString(uri));

		jar.Receive(uri, "SID=31d4d96e407aad42; Path=/; Domain=example.com");
		Assert.Equal("SID=31d4d96e407aad42; SID=31d4d96e407aad42", jar.CookieString(uri));

		jar = new Jar(jar.Now);
		jar.Receive(uri, "SID=31d4d96e407aad42; Path=/; Secure; HttpOnly");
		jar.Receive(uri, "lang=en-US; Path=/; Domain=example.com");
		Assert.Equal("SID=31d4d96e407aad42; lang=en-US", jar.CookieString(uri));
		Assert.Equal("lang=en-US", jar.CookieString(new Uri("http://sub.example.com/")));

		jar.Receive(uri, "lang=en-US; Expires=Wed, 09 Jun 2021 10:18:14 GMT");
		Assert.Equal("SID=31d4d96e407aad42; lang=en-US; lang=en-US", jar.CookieString(uri));

		jar.Receive(uri, "lang=; Expires=Sun, 06 Nov 1994 08:49:37 GMT");
		Assert.Equal("SID=31d4d96e407aad42; lang=en-US", jar.CookieString(uri));

		// The Cookie field the user agent sent reads back as the pairs it holds.
		Assert.Equal(
			[new CookiePair("SID", "31d4d96e407aad42"), new CookiePair("lang", "en-US")],
			CookiePair.ParseField("SID=31d4d96e407aad42; lang=en-US"));
	}

	// ── §5.2 ─────────────────────────────────────────────────────────────────────

	[Fact]
	public void A_cookie_with_its_attributes()
	{
		var cookie = SetCookie.Parse("SID=31d4d96e407aad42; Path=/; Secure; HttpOnly");

		Assert.Equal("SID", cookie.Name);
		Assert.Equal("31d4d96e407aad42", cookie.Value);
		Assert.Equal("/", cookie.Path);
		Assert.True(cookie.Secure);
		Assert.True(cookie.HttpOnly);
		Assert.Null(cookie.Domain);
		Assert.Null(cookie.ExpiryTime(DateTimeOffset.UtcNow));
	}

	[Theory]
	[InlineData("  A  = BC  ;foo;;;   bar", "A", "BC")]
	[InlineData("A=== BC  ;foo", "A", "== BC")]
	[InlineData("foo bar=baz", "foo bar", "baz")]
	[InlineData("\"a=b\"=bar", "\"a", "b\"=bar")]
	[InlineData("aBc=\"zz;pp\" ; ;", "aBc", "\"zz")]
	[InlineData("a=", "a", "")]
	[InlineData("\tfoo\t=\tbar\t \t;\tttt", "foo", "bar")]
	[InlineData("$Version=1; foo=bar", "$Version", "1")]
	public void The_pair_is_divided_and_trimmed(string field, string name, string value)
	{
		var cookie = SetCookie.Parse(field);

		Assert.Equal((name, value), (cookie.Name, cookie.Value));
	}

	/// <summary>§5.2 steps 2 and 5: no <c>=</c>, or no name, and the field is ignored.</summary>
	[Theory]
	[InlineData("")]
	[InlineData("foo")]
	[InlineData("=bar")]
	[InlineData("  =bar")]
	[InlineData("=")]
	[InlineData("foo;bar=baz")]
	[InlineData("   ")]
	public void What_a_user_agent_ignores(string field)
	{
		Assert.False(SetCookie.TryParse(field, out _), $"'{field}' was read.");
	}

	[Fact]
	public void Every_attribute_is_kept_and_divided()
	{
		var cookie = SetCookie.Parse("a=b; Secure =x; qux=\"1; 2\"; ; =v; flag");

		Assert.Equal(
			[
				new SetCookie.Attribute("Secure", "x"),
				new SetCookie.Attribute("qux", "\"1"),
				new SetCookie.Attribute("2\"", ""),
				new SetCookie.Attribute("", ""),
				new SetCookie.Attribute("", "v"),
				new SetCookie.Attribute("flag", ""),
			],
			cookie.Attributes);

		Assert.True(cookie.Secure);
	}

	[Theory]
	[InlineData("a=b; domain=.Example.COM", "example.com")]
	[InlineData("a=b; domain=  .home.example.org", "home.example.org")]
	[InlineData("a=b; domain=..home.example.org", ".home.example.org")]
	[InlineData("a=b; domain=a.com; domain=", "a.com")]
	[InlineData("a=b; domain=a.com; Domain=b.com", "b.com")]
	[InlineData("a=b; domain=", null)]
	[InlineData("a=b", null)]
	public void The_domain(string field, string? domain)
	{
		Assert.Equal(domain, SetCookie.Parse(field).Domain);
	}

	/// <summary>§5.2.4 and §5.3 step 7: the last Path counts, and one that is not absolute is the default-path.</summary>
	[Theory]
	[InlineData("a=b; path=/a", "/a")]
	[InlineData("a=b; path=/a; path=", null)]
	[InlineData("a=b; path=; path=/a", "/a")]
	[InlineData("a=b; path=a", null)]
	[InlineData("a=b; path", null)]
	[InlineData("a=b; Path= /", "/")]
	[InlineData("a=b", null)]
	public void The_path(string field, string? path)
	{
		Assert.Equal(path, SetCookie.Parse(field).Path);
	}

	[Theory]
	[InlineData("a=b; max-age=10", 10L)]
	[InlineData("a=b; max-age=-1", -1L)]
	[InlineData("a=b; max-age=0", 0L)]
	[InlineData("a=b; max-age=1.5", null)]
	[InlineData("a=b; max-age=50,399", null)]
	[InlineData("a=b; max-age=-", null)]
	[InlineData("a=b; max-age=", null)]
	[InlineData("a=b; max-age=10; max-age=x", 10L)]
	[InlineData("a=b; MAX-AGE=99999999999999999999999", long.MaxValue)]
	[InlineData("a=b; max-age=-99999999999999999999999", long.MinValue)]
	public void The_max_age(string field, long? seconds)
	{
		Assert.Equal(seconds, SetCookie.Parse(field).MaxAge);
	}

	[Fact]
	public void The_last_expires_that_is_a_date()
	{
		var cookie = SetCookie.Parse("a=b; expires=Wed, 09 Jun 2021 10:18:14 GMT; expires=never");

		Assert.Equal(new DateTimeOffset(2021, 6, 9, 10, 18, 14, TimeSpan.Zero), cookie.Expires);
	}

	/// <summary>§5.3 step 3: Max-Age before Expires, and zero or less at once.</summary>
	[Fact]
	public void The_expiry_time()
	{
		var now = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

		Assert.Equal(now.AddSeconds(60), SetCookie.Parse("a=b; expires=Wed, 09 Jun 2021 10:18:14 GMT; max-age=60").ExpiryTime(now));
		Assert.Equal(DateTimeOffset.MinValue, SetCookie.Parse("a=b; max-age=0").ExpiryTime(now));
		Assert.Equal(DateTimeOffset.MaxValue, SetCookie.Parse("a=b; max-age=99999999999999").ExpiryTime(now));
		Assert.Equal(new DateTimeOffset(2021, 6, 9, 10, 18, 14, TimeSpan.Zero), SetCookie.Parse("a=b; expires=Wed, 09 Jun 2021 10:18:14 GMT").ExpiryTime(now));
	}

	[Theory]
	[InlineData("a=b")]
	[InlineData("SID=31d4d96e407aad42; Path=/; Secure; HttpOnly")]
	[InlineData("  A  = BC  ;foo;;;   bar")]
	[InlineData("x=\"q\"; Expires=Wed, 09 Jun 2021 10:18:14 GMT; ext=1=2")]
	public void Written_back_and_read_again(string field)
	{
		var cookie = SetCookie.Parse(field);

		Assert.Equal(cookie, SetCookie.Parse(cookie.ToString()));
	}

	// ── §5.1.1 ───────────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("Wed, 09 Jun 2021 10:18:14 GMT", 2021, 6, 9, 10, 18, 14)]
	[InlineData("Sun, 06 Nov 1994 08:49:37 GMT", 1994, 11, 6, 8, 49, 37)]
	[InlineData("Sunday, 06-Nov-94 08:49:37 GMT", 1994, 11, 6, 8, 49, 37)]
	[InlineData("Sun Nov  6 08:49:37 1994", 1994, 11, 6, 8, 49, 37)]
	[InlineData("Sun, 18-Apr-2027 21:06:29 GMT", 2027, 4, 18, 21, 6, 29)]
	[InlineData("Fri 07 Aug 2019 08:04:19 GMT, baz=qux", 2019, 8, 7, 8, 4, 19)]
	[InlineData("1 jAN 69 0:0:0", 2069, 1, 1, 0, 0, 0)]
	[InlineData("1 January 70 23:59:59", 1970, 1, 1, 23, 59, 59)]
	[InlineData("  1st Feb 2000 12:00:00GMT  ", 2000, 2, 1, 12, 0, 0)]
	[InlineData("29 Feb 2000 00:00:00", 2000, 2, 29, 0, 0, 0)]
	[InlineData("01 Jan 1601 00:00:00", 1601, 1, 1, 0, 0, 0)]
	[InlineData("31 Dec 9999 23:59:59", 9999, 12, 31, 23, 59, 59)]
	public void A_cookie_date(string text, int year, int month, int day, int hour, int minute, int second)
	{
		Assert.Equal(new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero), CookieDate.Parse(text));
	}

	[Theory]
	[InlineData("")]
	[InlineData(",;")]
	[InlineData("1 Jan 2020")]
	[InlineData("Jan 2020 00:00:00")]
	[InlineData("1 2020 00:00:00")]
	[InlineData("1 Jan 00:00:00")]
	[InlineData("30 Feb 2020 00:00:00")]
	[InlineData("29 Feb 2100 00:00:00")]
	[InlineData("32 Jan 2020 00:00:00")]
	[InlineData("0 Jan 2020 00:00:00")]
	[InlineData("1 Jan 1600 00:00:00")]
	[InlineData("1 Jan 2020 24:00:00")]
	[InlineData("1 Jan 2020 00:60:00")]
	[InlineData("1 Jan 2020 00:00:60")]
	[InlineData("1 Jan 12345 00:00:00")]
	[InlineData("123 Jan 2020 00:00:00")]
	public void What_is_no_cookie_date(string text)
	{
		Assert.False(CookieDate.TryParse(text, out _), $"'{text}' was read.");
	}

	// ── §4.2.1 ───────────────────────────────────────────────────────────────────

	[Theory]
	[InlineData(" a=b ", "a", "b")]
	[InlineData("a=\"bc\"", "a", "\"bc\"")]
	[InlineData("a=", "a", "")]
	[InlineData("a=!#$%&'()*+-./:<=>?@[]^_`{|}~", "a", "!#$%&'()*+-./:<=>?@[]^_`{|}~")]
	public void A_cookie_pair(string field, string name, string value)
	{
		Assert.Equal([new CookiePair(name, value)], CookiePair.ParseField(field));
	}

	[Theory]
	[InlineData("")]
	[InlineData("a=b;c=d")]
	[InlineData("a=b;  c=d")]
	[InlineData("a=b; ")]
	[InlineData("=b")]
	[InlineData("a b=c")]
	[InlineData("a=b c")]
	[InlineData("a=\"b c\"")]
	[InlineData("a=b,")]
	[InlineData("a=b\\")]
	[InlineData("a=\"b")]
	public void What_is_no_cookie_field(string field)
	{
		Assert.False(CookiePair.TryParseField(field, out _), $"'{field}' was read.");
	}

	// ── §5.1.3, §5.1.4 ───────────────────────────────────────────────────────────

	[Theory]
	[InlineData("www.example.com", "example.com", true)]
	[InlineData("example.com", "example.com", true)]
	[InlineData("a.b.example.com", "example.com", true)]
	[InlineData("wwwexample.com", "example.com", false)]
	[InlineData("example.com", "www.example.com", false)]
	[InlineData("192.0.2.1", "0.2.1", false)]
	[InlineData("192.0.2.1", "192.0.2.1", true)]
	[InlineData("example.com", "", false)]
	public void Domain_matching(string host, string domain, bool matches)
	{
		Assert.Equal(matches, SetCookie.DomainMatches(host, domain));
	}

	[Theory]
	[InlineData("", "/")]
	[InlineData("a/b", "/")]
	[InlineData("/", "/")]
	[InlineData("/a", "/")]
	[InlineData("/a/b", "/a")]
	[InlineData("/a/b/", "/a/b")]
	public void The_default_path(string requestPath, string defaultPath)
	{
		Assert.Equal(defaultPath, SetCookie.DefaultPath(requestPath));
	}

	[Theory]
	[InlineData("/foo", "/foo", true)]
	[InlineData("/foo/bar", "/foo", true)]
	[InlineData("/foo/bar", "/foo/", true)]
	[InlineData("/foo//qux", "/foo/", true)]
	[InlineData("/fooqux", "/foo", false)]
	[InlineData("/foo", "/foo/", false)]
	[InlineData("/Foo", "/foo", false)]
	[InlineData("/", "/foo", false)]
	public void Path_matching(string requestPath, string cookiePath, bool matches)
	{
		Assert.Equal(matches, SetCookie.PathMatches(requestPath, cookiePath));
	}

	// ── The http-state working group's cases ─────────────────────────────────────

	const string Origin = "http://home.example.org:8888";

	public static TheoryData<string> HttpStateCases()
	{
		var data = new TheoryData<string>();

		foreach (var file in Directory.GetFiles(Parser, "*-test"))
		{
			var name = Path.GetFileName(file);

			if (!name.StartsWith("disabled-", StringComparison.Ordinal))
				data.Add(name.Substring(0, name.Length - "-test".Length));
		}

		return data;
	}

	/// <summary>
	/// A case as the working group's test server runs it: its Set-Cookie fields answer a request for
	/// <c>/cookie-parser?name</c> on <c>home.example.org</c>, and the Cookie field expected is what the request its
	/// Location names, <c>/cookie-parser-result?name</c> by default, carries back.
	/// </summary>
	[Theory]
	[MemberData(nameof(HttpStateCases))]
	public void The_http_state_case(string name)
	{
		// Before the earliest Expires a case means to be in the future (2019) and after the latest it means to be past.
		var jar    = new Jar(new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero));
		var result = $"{Origin}/cookie-parser-result?{name}";

		foreach (var (header, value) in Fields(Path.Combine(Parser, name + "-test")))
		{
			if (string.Equals(header, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
				jar.Receive($"{Origin}/cookie-parser?{name}", value);
			else if (string.Equals(header, "Location", StringComparison.OrdinalIgnoreCase))
				result = value.StartsWith("/", StringComparison.Ordinal) ? Origin + value : value;
		}

		// An expected file with no Cookie field expects none. One (0028) holds its test's Set-Cookie fields instead.
		var expected = Fields(Path.Combine(Parser, name + "-expected"))
			.Where(field => string.Equals(field.Header, "Cookie", StringComparison.OrdinalIgnoreCase))
			.Select(field => field.Value)
			.SingleOrDefault() ?? "";

		Assert.Equal(expected, jar.CookieString(result));
	}

	static string Parser => Path.Combine(Path.GetDirectoryName(ThisFile)!, "HttpState", "parser");

	/// <summary>A case file's header fields: ISO-8859-1 octets, one to a line, the value without its surrounding whitespace.</summary>
	static IEnumerable<(string Header, string Value)> Fields(string file)
	{
		foreach (var line in Encoding.Latin1.GetString(File.ReadAllBytes(file)).Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries))
		{
			var colon = line.IndexOf(':');

			if (colon > 0)
				yield return (line.Substring(0, colon), line.Substring(colon + 1).Trim(' ', '\t'));
		}
	}

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;

	// ── A store, §5.3 and §5.4 cut to what the tests ask ─────────────────────────

	sealed class Jar(DateTimeOffset now)
	{
		readonly List<Stored> _cookies = [];
		long _created;

		public DateTimeOffset Now { get; } = now;

		public void Receive(Uri uri, string field) => Receive(uri.OriginalString, field);

		public string CookieString(Uri uri) => CookieString(uri.OriginalString);

		public void Receive(string url, string field)
		{
			if (!SetCookie.TryParse(field, out var cookie))
				return;
			var (_, host, path) = Target(url);
			var domain = cookie.Domain ?? "";

			// §5.3 step 5, with a public suffix list of one rule: a name without a dot is a public suffix.
			if (domain.Length > 0 && domain.IndexOf('.') < 0)
			{
				if (domain != host)
					return;

				domain = "";
			}

			if (domain.Length > 0 && !SetCookie.DomainMatches(host, domain))
				return;

			var stored = new Stored(
				cookie.Name,
				cookie.Value,
				domain.Length > 0 ? domain : host,
				HostOnly: domain.Length == 0,
				cookie.Path ?? SetCookie.DefaultPath(path),
				cookie.Secure,
				cookie.ExpiryTime(Now) ?? DateTimeOffset.MaxValue,
				_created++);

			var old = _cookies.FindIndex(existing => existing.Name == stored.Name && existing.Domain == stored.Domain && existing.Path == stored.Path);

			if (old >= 0)
			{
				stored = stored with { Created = _cookies[old].Created };
				_cookies.RemoveAt(old);
			}

			_cookies.Add(stored);
			_cookies.RemoveAll(existing => existing.Expiry <= Now);
		}

		public string CookieString(string url)
		{
			var (scheme, host, path) = Target(url);

			return string.Join("; ", _cookies
				.Where(cookie => cookie.HostOnly ? cookie.Domain == host : SetCookie.DomainMatches(host, cookie.Domain))
				.Where(cookie => SetCookie.PathMatches(path, cookie.Path))
				.Where(cookie => !cookie.Secure || scheme == "https")
				.OrderByDescending(cookie => cookie.Path.Length)
				.ThenBy(cookie => cookie.Created)
				.Select(cookie => cookie.Name + "=" + cookie.Value));
		}

		// The request-uri as §5.1.2 and §5.1.4 take it: the host in lower case, the path as written and not unescaped.
		static (string Scheme, string Host, string Path) Target(string url)
		{
			var parts = UriReference.ParseUri(url);

			return (parts.Scheme!.ToLowerInvariant(), parts.Host!.ToLowerInvariant(), parts.Path.Length == 0 ? "/" : parts.Path);
		}

		sealed record Stored(string Name, string Value, string Domain, bool HostOnly, string Path, bool Secure, DateTimeOffset Expiry, long Created);
	}
}
