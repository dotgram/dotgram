using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using DotGram.Finance.Fix;
using DotGram.Handwritten.Web;
using DotGram.Web;

namespace DotGram.Benchmarks;

// The web's formats, and what a regular expression says of them (architect, for Igor,
// 2026-09-18: "benchmarks of all the parsers, regex included").
//
// A row here has a hand-written reading where D16 has one written (RFC 3986, RFC 3339 and
// RFC 8259, in DotGram.Handwritten/Web), and that is its base; where none exists (RFC 5322,
// RFC 9110, RFC 9651) the first reading, the base every ratio is taken against, is the
// generated one.
// The regular expressions are transcriptions of the specification, not loose look-alikes,
// and each row says in its own words how much less than the parser one of them does. Where
// no transcription exists or one would be a fiction (JSON is recursive, a structured field
// is a small language) the row has the generated reading alone, and the report says N/A
// rather than leaving the reader to wonder.
//
// A regex is constructed inside the row's first call, not when the rows are made, so the
// first call in a fresh process pays for building it the way a user's first call would.

static partial class Stand
{
	const RegexOptions Options = RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;

	static Lazy<Regex> Lazily(string pattern, RegexOptions options = Options) =>
		new(() => new Regex(pattern, options));

	/// <summary>The interpreted reading and the compiled one of the same pattern.</summary>
	static (Lazy<Regex> Interpreted, Lazy<Regex> Compiled) Both(string pattern, RegexOptions options = Options) =>
		(Lazily(pattern, options), Lazily(pattern, options | RegexOptions.Compiled));

	static Workload Web(string name, Reading[] readings, Func<string?> agreement) => new("web", name, readings, agreement);

	/// <summary>One text, read by the generated parser alone: the formats no regex can be honest about.</summary>
	static Workload WebGenerated(string name, string text, Func<string, bool> accepts, bool accepted = true) =>
		Web(name,
			[new Reading("generated", () => accepts(text) ? 1 : 0)],
			() => accepts(text) == accepted ? null : $"  the generated parser {(accepts(text) ? "accepts" : "refuses")} it, and the row says it must {(accepted ? "accept" : "refuse")}");

	/// <summary>The JSON objects and arrays of the web rows, shared with the paired stand.</summary>
	const string JsonObjectText = "{\"id\": 12345, \"name\": \"dotgram\", \"tags\": [\"parser\", \"generator\", \"analyzer\"], \"nested\": {\"x\": 1.5, \"y\": null, \"z\": true}, \"text\": \"a string with an escape \\n and a unicode one \\u00e9\", \"list\": [1, 2, 3, 4, 5]}";

	const string JsonArrayText = "[1, 2.5, -3, \"four\", true, false, null, [5, 6], {\"k\": \"v\"}, 1e3, \"a longer string to read\", 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58]";

	const string CookieFull = "sid=38afes7a8; Expires=Wed, 21 Oct 2026 07:28:00 GMT; Max-Age=2592000; Domain=example.com; Path=/docs; Secure; HttpOnly; SameSite=Lax";

	const string CookieShort = "id=a3fWa";

	// The formats that had no row (finance-24, from the critic's Q5): one accepted and one refused text each, the refused one refused early.
	// A JSON Patch has no non-throwing text form, so it has an accepted row alone.
	const string DispositionFull = "attachment; filename=\"report-2026.pdf\"; filename*=UTF-8''r%C3%A9sum%C3%A9.pdf; size=10240";

	const string DispositionRefused = "; filename=\"x\"";

	const string TemplateFull = "https://api.example.com/v2/{+path}/items{?page,per_page,sort*}{#fragment}";

	const string TemplateRefused = "https://api.example.com/{unclosed";

	const string PatchFull = "[{\"op\":\"add\",\"path\":\"/a/b\",\"value\":{\"c\":[1,2,3]}},{\"op\":\"remove\",\"path\":\"/d\"},{\"op\":\"replace\",\"path\":\"/e\",\"value\":\"x\"},{\"op\":\"move\",\"from\":\"/f\",\"path\":\"/g\"},{\"op\":\"test\",\"path\":\"/h\",\"value\":true}]";

	const string ForwardedFull = "for=192.0.2.60;proto=http;by=203.0.113.43, for=\"[2001:db8:cafe::17]:4711\";host=example.com";

	const string ForwardedRefused = "for==1";

	const string LinkFull = "<https://example.com/page/2>; rel=\"next\"; title=\"Next page\", <https://example.com/page/1>; rel=\"prev\", <https://example.com/>; rel=\"index\"; type=\"text/html\"";

	const string LinkRefused = "https://example.com/>; rel=next";

	const string CookieRefused = "=novalue";

	const string PointerFull = "/definitions/item/properties/0/a~1b/c~0d";

	const string PointerShort = "/a";

	static readonly string[] UrlNames = ["plain", "full", "ipv4", "long-path", "refused"];

	/// <summary>
	/// The URLs and the JSON of the web rows, of two builds against each other with this tree's hand
	/// parsers as the control: the RFC 3986 and RFC 8259 parsers loaded from each side by reflection.
	/// </summary>
	static IEnumerable<Workload> PairedWeb(PairedSide before, PairedSide after)
	{
		var inputs = UrlBenchmarks.Inputs.ToArray();

		for (var i = 0; i < inputs.Length; i++)
		{
			var text = inputs[i];

			yield return new Workload("web", "url." + UrlNames[i],
				[
					new Reading("hand",   () => HandUrl.TryParseReference(text, out _, out _) ? 1 : 0),
					new Reading("before", before.WebUrl(text)),
					new Reading("after",  after.WebUrl(text)),
				],
				() => null);
		}

		foreach (var (name, text) in new[] { ("json.object", JsonObjectText), ("json.array", JsonArrayText) })
		{
			yield return new Workload("web", name,
				[
					new Reading("hand",   () => HandJson.TryParse(text, out _, out _) ? 1 : 0),
					new Reading("before", before.WebJson(text)),
					new Reading("after",  after.WebJson(text)),
				],
				() => null);
		}
	}

	static IEnumerable<Workload> WebWorkloads()
	{
		return
		[
			.. WebUrls(),

			// RFC 8259: a recursive language; a regex cannot read it, so N/A. System.Text.Json is
			// the outside reading, the way ScriptDom is T-SQL's.
			.. WebJson("json.object", JsonObjectText),
			.. WebJson("json.array", JsonArrayText),

			.. WebTimestamps(),
			.. WebAddresses(),
			.. WebMediaTypes(),

			// RFC 6265 and RFC 6901: read by the generated parser alone, as the structured fields are; the cookie also by the
			// framework's CookieContainer, as a reference and no yardstick.
			.. WebCookies(),

			// RFC 6266, 6570, 6902, 7239 and 8288, which had no row: one accepted and one refused text each.
			WebGenerated("content-disposition.full", DispositionFull, static text => ContentDisposition.TryParse(text, out _)),
			WebGenerated("content-disposition.refused", DispositionRefused, static text => ContentDisposition.TryParse(text, out _), accepted: false),
			WebGenerated("uri-template.full", TemplateFull, static text => UriTemplate.TryParse(text, out _)),
			WebGenerated("uri-template.refused", TemplateRefused, static text => UriTemplate.TryParse(text, out _), accepted: false),
			WebGenerated("json-patch.full", PatchFull, static text => JsonPatchParses(text)),
			WebGenerated("forwarded.full", ForwardedFull, static text => ForwardedElement.TryParseField(text, out _)),
			WebGenerated("forwarded.refused", ForwardedRefused, static text => ForwardedElement.TryParseField(text, out _), accepted: false),
			WebGenerated("link.full", LinkFull, static text => WebLink.TryParseField(text, out _)),
			WebGenerated("link.refused", LinkRefused, static text => WebLink.TryParseField(text, out _), accepted: false),
			WebGenerated("pointer.full", PointerFull, static text => JsonPointer.TryParse(text, out _)),
			WebGenerated("pointer.short", PointerShort, static text => JsonPointer.TryParse(text, out _)),

			// RFC 9651: a small language of its own with nested lists and parameters; N/A.
			WebGenerated("sf.item", "42;unit=\"s\";exact=?1", static text => StructuredField.TryParseItem(text, out _)),
			WebGenerated("sf.list", "\"foo\", bar;baz=?1, (1 2 3);q=0.5, :aGVsbG8=:, 10.5", static text => StructuredField.TryParseList(text, out _)),
			WebGenerated("sf.dictionary", "a=1, b=?0, c=\"text\", d=:aGVsbG8=:, e=(1 2);f=3", static text => StructuredField.TryParseDictionary(text, out _)),
		];
	}

	// ── RFC 8259 ────────────────────────────────────────────────────────────────

	/// <summary>
	/// One JSON text, read by hand, by the generated parser and by System.Text.Json, which only
	/// checks and builds no tree of these values' shape: the outside reading, not an equal. The
	/// hand and generated trees are held equal; System.Text.Json has only to accept.
	/// </summary>
	static IEnumerable<Workload> WebJson(string name, string text)
	{
		yield return Web(
			name,
			[
				new Reading("hand",                () => HandJson.TryParse(text, out _, out _) ? 1 : 0),
				new Reading("generated",           () => JsonValue.TryParse(text, out _) ? 1 : 0),
				new Reading("system-text-json",    () => SystemTextJson(text) ? 1 : 0),
			],
			() =>
			{
				var byHand      = HandJson.TryParse(text, out var hand, out _);
				var byGenerated = JsonValue.TryParse(text, out var generated);

				if (!byHand || !byGenerated || !SystemTextJson(text))
					return $"  every reading must accept it: hand {(byHand ? "accepts" : "refuses")}, generated {(byGenerated ? "accepts" : "refuses")}, System.Text.Json {(SystemTextJson(text) ? "accepts" : "refuses")}";

				return Equals(hand, generated) ? null : "  the hand tree and the generated tree differ";
			});
	}

	static bool SystemTextJson(string text)
	{
		try
		{
			using var document = JsonDocument.Parse(text);

			return document.RootElement.ValueKind != JsonValueKind.Undefined;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	// ── RFC 3986 ────────────────────────────────────────────────────────────────

	/// <summary>
	/// The five inputs of <see cref="UrlBenchmarks"/> and the pattern it transcribes from the
	/// example URL grammar, held to the RFC 3986 parser part by part. Lesser: the pattern
	/// takes three schemes and no relative references, which the RFC's parser takes.
	/// </summary>
	static IEnumerable<Workload> WebUrls()
	{
		string[] names   = UrlNames;
		var      inputs  = UrlBenchmarks.Inputs.ToArray();
		var      (interpreted, compiled) = Both(UrlBenchmarks.Pattern);

		for (var i = 0; i < inputs.Length; i++)
		{
			var text = inputs[i];

			yield return Web(
				"url." + names[i],
				[
					new Reading("hand",           () => UrlHand(text)),
					new Reading("generated",      () => UrlGenerated(text)),
					new Reading("regex",          () => UrlRegex(interpreted.Value, text)),
					new Reading("regex-compiled", () => UrlRegex(compiled.Value, text)),
				],
				() => UrlDisagreement(compiled.Value, text));
		}
	}

	static int UrlHand(string text) =>
		HandUrl.TryParseReference(text, out var url, out _) ? UrlLength(url!) : 0;

	static int UrlGenerated(string text) =>
		UriReference.TryParse(text, out var url) ? UrlLength(url) : 0;

	static int UrlLength(UriReference url)
	{
		return (url.Scheme?.Length ?? 0) + (url.UserInfo?.Length ?? 0) + (url.Host?.Length ?? 0) +
			(url.Port?.Length ?? 0) + url.Path.Length + (url.Query?.Length ?? 0) + (url.Fragment?.Length ?? 0);
	}

	static int UrlRegex(Regex regex, string text)
	{
		var match = regex.Match(text);

		if (!match.Success)
			return 0;

		var groups = match.Groups;

		return groups["scheme"].Value.Length + groups["user"].Value.Length + groups["host"].Value.Length +
			groups["port"].Value.Length + groups["path"].Value.Length + groups["query"].Value.Length +
			groups["fragment"].Value.Length;
	}

	static string? UrlDisagreement(Regex regex, string text)
	{
		var accepted = UriReference.TryParse(text, out var url);
		var byHand   = HandUrl.TryParseReference(text, out var hand, out _);
		var match    = regex.Match(text);

		if (accepted != byHand || (accepted && !Equals(url, hand)))
			return $"  '{text}': the generated parser {(accepted ? "reads" : "refuses")} it, the hand parser {(byHand ? "reads" : "refuses")} it, and they {(accepted && byHand ? "do not read the same" : "differ")}";

		if (accepted != match.Success)
			return $"  '{text}': the RFC 3986 parser {(accepted ? "accepts" : "refuses")} it, the pattern {(match.Success ? "accepts" : "refuses")}";

		if (!accepted)
			return null;

		var groups = match.Groups;

		return Parts(text,
			("scheme",   url!.Scheme ?? "",   groups["scheme"].Value),
			("user",     url.UserInfo ?? "",  groups["user"].Value),
			("host",     url.Host ?? "",      groups["host"].Value),
			("port",     url.Port ?? "",      groups["port"].Value),
			("path",     url.Path,            groups["path"].Value),
			("query",    url.Query ?? "",     groups["query"].Value),
			("fragment", url.Fragment ?? "",  groups["fragment"].Value));
	}

	/// <summary>The first part on which the two sides differ, or null where every part is the same.</summary>
	static string? Parts(string text, params (string Part, string Parser, string Regex)[] parts)
	{
		foreach (var (part, parser, regex) in parts)
			if (parser != regex)
				return $"  '{text}', {part}: the parser read '{parser}', the pattern '{regex}'";

		return null;
	}

	// ── RFC 3339 ────────────────────────────────────────────────────────────────

	/// <summary>
	/// A date-time and the pattern the RFC's ABNF makes of it. Lesser: the pattern says yes to
	/// the thirtieth of February and to a leap second at noon, which §5.7's checks refuse.
	/// </summary>
	static IEnumerable<Workload> WebTimestamps()
	{
		var (interpreted, compiled) = Both(
			@"^(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})[Tt](?<hour>[0-9]{2}):(?<minute>[0-9]{2}):(?<second>[0-9]{2})(?:\.(?<fraction>[0-9]+))?(?<offset>[Zz]|[+-][0-9]{2}:[0-9]{2})$");

		foreach (var (name, text) in new[]
		{
			("date-time.utc",     "2026-09-18T12:34:56Z"),
			("date-time.offset",  "2026-09-18T12:34:56.789+02:00"),
			("date-time.refused", "2026-09-18T12:34:56"),
		})
		{
			yield return Web(
				name,
				[
					new Reading("hand",           () => TimestampHand(text)),
					new Reading("generated",      () => TimestampGenerated(text)),
					new Reading("regex",          () => TimestampRegex(interpreted.Value, text, out _)),
					new Reading("regex-compiled", () => TimestampRegex(compiled.Value, text, out _)),
				],
				() => TimestampDisagreement(compiled.Value, text));
		}
	}

	static int TimestampHand(string text) =>
		HandDateTime.TryParseTimestamp(text, out var timestamp, out _) ? TimestampLength(timestamp!) : 0;

	static int TimestampGenerated(string text) =>
		Timestamp.TryParse(text, out var timestamp) ? TimestampLength(timestamp) : 0;

	static int TimestampLength(Timestamp timestamp)
	{
		var (date, time) = (timestamp.Date, timestamp.Time);

		return date.Year + date.Month + date.Day + time.Hour + time.Minute + time.Second +
			(time.Fraction?.Length ?? 0) + (int)time.Offset.TotalMinutes;
	}

	static int TimestampRegex(Regex regex, string text, out string parts)
	{
		var match = regex.Match(text);

		parts = "";

		if (!match.Success)
			return 0;

		var groups   = match.Groups;
		var offset   = groups["offset"].Value;
		var minutes  = offset.Length == 1
			? 0
			: (offset[0] == '-' ? -1 : 1) * (int.Parse(offset.AsSpan(1, 2), NumberStyles.None, CultureInfo.InvariantCulture) * 60 + int.Parse(offset.AsSpan(4, 2), NumberStyles.None, CultureInfo.InvariantCulture));
		var year     = Number(groups["year"]);
		var month    = Number(groups["month"]);
		var day      = Number(groups["day"]);
		var hour     = Number(groups["hour"]);
		var minute   = Number(groups["minute"]);
		var second   = Number(groups["second"]);
		var fraction = groups["fraction"].Success ? groups["fraction"].Length : 0;

		return year + month + day + hour + minute + second + fraction + minutes;

		static int Number(Group group) => int.Parse(group.ValueSpan, NumberStyles.None, CultureInfo.InvariantCulture);
	}

	static string? TimestampDisagreement(Regex regex, string text)
	{
		var accepted = Timestamp.TryParse(text, out var timestamp);
		var byHand   = HandDateTime.TryParseTimestamp(text, out var hand, out _);
		var match    = regex.Match(text);

		if (accepted != byHand || (accepted && !Equals(timestamp, hand)))
			return $"  '{text}': the generated parser {(accepted ? "reads" : "refuses")} it, the hand parser {(byHand ? "reads" : "refuses")} it, and they {(accepted && byHand ? "do not read the same" : "differ")}";

		if (accepted != match.Success)
			return $"  '{text}': the RFC 3339 parser {(accepted ? "accepts" : "refuses")} it, the pattern {(match.Success ? "accepts" : "refuses")}";

		if (!accepted)
			return null;

		var groups = match.Groups;
		var offset = groups["offset"].Value;
		var time   = timestamp!.Time;

		return Parts(text,
			("year",     timestamp.Date.Year.ToString("D4", CultureInfo.InvariantCulture), groups["year"].Value),
			("month",    timestamp.Date.Month.ToString("D2", CultureInfo.InvariantCulture), groups["month"].Value),
			("day",      timestamp.Date.Day.ToString("D2", CultureInfo.InvariantCulture),   groups["day"].Value),
			("hour",     time.Hour.ToString("D2", CultureInfo.InvariantCulture),     groups["hour"].Value),
			("minute",   time.Minute.ToString("D2", CultureInfo.InvariantCulture),   groups["minute"].Value),
			("second",   time.Second.ToString("D2", CultureInfo.InvariantCulture),   groups["second"].Value),
			("fraction", time.Fraction ?? "",                                        groups["fraction"].Value),
			("offset",   ((int)time.Offset.TotalMinutes).ToString(CultureInfo.InvariantCulture),
				(offset.Length == 1 ? 0 : (offset[0] == '-' ? -1 : 1) * (int.Parse(offset.AsSpan(1, 2), NumberStyles.None, CultureInfo.InvariantCulture) * 60 + int.Parse(offset.AsSpan(4, 2), NumberStyles.None, CultureInfo.InvariantCulture))).ToString(CultureInfo.InvariantCulture)));
	}

	// ── RFC 5322 ────────────────────────────────────────────────────────────────

	/// <summary>
	/// An addr-spec in §3's syntax alone (<c>TryParseStrict</c>), and the dot-atom form of it as
	/// a pattern. Lesser: no comments, no folding, no quoted local part, which §3 allows.
	/// </summary>
	static IEnumerable<Workload> WebAddresses()
	{
		const string Atom = @"[A-Za-z0-9!#$%&'*+/=?^_`{|}~\-]+";

		var (interpreted, compiled) = Both(
			$@"^(?<local>{Atom}(?:\.{Atom})*)@(?<domain>{Atom}(?:\.{Atom})*|\[[\x21-\x5A\x5E-\x7E]*\])$");

		foreach (var (name, text) in new[]
		{
			("addr-spec.plain",   "user@example.com"),
			("addr-spec.tagged",  "first.last+tag@mail.example.org"),
			("addr-spec.refused", "user@@example.com"),
		})
		{
			yield return Web(
				name,
				[
					new Reading("generated",      () => AddressGenerated(text)),
					new Reading("regex",          () => AddressRegex(interpreted.Value, text)),
					new Reading("regex-compiled", () => AddressRegex(compiled.Value, text)),

					// The framework's address parser: a reference and no yardstick (it normalizes, and reads more forms than the addr-spec of section 3).
					new Reading("reference-MailAddress", () => System.Net.Mail.MailAddress.TryCreate(text, out var mail) ? mail.Address.Length : 0),
				],
				() =>
				{
					var accepted = AddrSpec.TryParseStrict(text, out var address);
					var match    = compiled.Value.Match(text);

					if (accepted != match.Success)
						return $"  '{text}': the RFC 5322 parser {(accepted ? "accepts" : "refuses")} it, the pattern {(match.Success ? "accepts" : "refuses")}";

					return accepted
						? Parts(text, ("local part", address!.LocalPart, match.Groups["local"].Value), ("domain", address.Domain, match.Groups["domain"].Value))
						: null;
				});
		}
	}

	/// <summary>A Set-Cookie value read by the generated parser and, as a reference, by the framework's <c>CookieContainer</c>.</summary>
	static IEnumerable<Workload> WebCookies()
	{
		var uri = new Uri("https://example.com/docs");

		foreach (var (name, text) in new[] { ("cookie.full", CookieFull), ("cookie.short", CookieShort), ("cookie.refused", CookieRefused) })
		{
			yield return Web(name,
				[
					new Reading("generated", () => SetCookie.TryParse(text, out _) ? 1 : 0),

					// A jar keeps the cookie it is given, which the parser does not: a reference and no yardstick.
					new Reading("reference-CookieContainer", () => JarAccepts(uri, text) ? 1 : 0),
				],
				() => SetCookie.TryParse(text, out _) == !name.Contains("refused", StringComparison.Ordinal) ? null : $"  '{text}': the parser answers the other way");
		}
	}

	/// <summary>Whether the jar took the cookie: no exception, and a cookie held.</summary>
	static bool JarAccepts(Uri uri, string text)
	{
		var jar = new System.Net.CookieContainer();

		try
		{
			jar.SetCookies(uri, text);
		}
		catch (System.Net.CookieException)
		{
			return false;
		}

		return jar.GetCookies(uri).Count > 0;
	}

	static bool JsonPatchParses(string text)
	{
		try
		{
			return JsonPatch.Parse(text) is not null;
		}
		catch (Exception)
		{
			return false;
		}
	}

	static int AddressGenerated(string text) =>
		AddrSpec.TryParseStrict(text, out var address) ? address.LocalPart.Length + address.Domain.Length : 0;

	static int AddressRegex(Regex regex, string text)
	{
		var match = regex.Match(text);

		return match.Success ? match.Groups["local"].Value.Length + match.Groups["domain"].Value.Length : 0;
	}

	// ── RFC 9110 ────────────────────────────────────────────────────────────────

	/// <summary>
	/// A Content-Type value and the pattern §8.3.1's ABNF makes of it. Lesser: no
	/// case-folding of the type or of a charset, which the parser's equality does, and no
	/// backslash-escapes are unescaped.
	/// </summary>
	static IEnumerable<Workload> WebMediaTypes()
	{
		const string Token = @"[!#$%&'*+\-.^_`|~0-9A-Za-z]+";

		var (interpreted, compiled) = Both(
			$@"^[ \t]*(?<type>{Token})/(?<subtype>{Token})(?:[ \t]*;[ \t]*(?<name>{Token})=(?<value>{Token}|""(?:[\t !#-\[\]-~]|\\[\t -~])*""))*[ \t]*$");

		foreach (var (name, text) in new[]
		{
			("media-type.plain",   "text/html; charset=utf-8"),
			("media-type.quoted",  "multipart/form-data; boundary=\"----abc123\"; charset=UTF-8"),
			("media-type.refused", "text/"),
		})
		{
			yield return Web(
				name,
				[
					new Reading("generated",      () => MediaTypeGenerated(text)),
					new Reading("regex",          () => MediaTypeRegex(interpreted.Value, text)),
					new Reading("regex-compiled", () => MediaTypeRegex(compiled.Value, text)),

					// The framework's Content-Type parser: a reference and no yardstick (it keeps the header's own model).
					new Reading("reference-MediaTypeHeaderValue", () => System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(text, out var header) ? header.MediaType!.Length : 0),
				],
				() => MediaTypeDisagreement(compiled.Value, text));
		}
	}

	static int MediaTypeGenerated(string text)
	{
		if (!MediaType.TryParse(text, out var type))
			return 0;

		var length = type.Type.Length + type.Subtype.Length;

		for (var i = 0; i < type.Parameters.Count; i++)
			length += type.Parameters[i].Name.Length + type.Parameters[i].Value.Length;

		return length;
	}

	static int MediaTypeRegex(Regex regex, string text)
	{
		var match = regex.Match(text);

		if (!match.Success)
			return 0;

		var groups = match.Groups;
		var length = groups["type"].Value.Length + groups["subtype"].Value.Length;
		var names  = groups["name"].Captures;
		var values = groups["value"].Captures;

		for (var i = 0; i < names.Count; i++)
			length += names[i].Value.Length + Unquoted(values[i].Value).Length;

		return length;
	}

	static string Unquoted(string value) => value.Length > 1 && value[0] == '"' ? value.Substring(1, value.Length - 2) : value;

	static string? MediaTypeDisagreement(Regex regex, string text)
	{
		var accepted = MediaType.TryParse(text, out var type);
		var match    = regex.Match(text);

		if (accepted != match.Success)
			return $"  '{text}': the RFC 9110 parser {(accepted ? "accepts" : "refuses")} it, the pattern {(match.Success ? "accepts" : "refuses")}";

		if (!accepted)
			return null;

		var names  = match.Groups["name"].Captures;
		var values = match.Groups["value"].Captures;

		if (names.Count != type!.Parameters.Count)
			return $"  '{text}': the parser read {type.Parameters.Count} parameters, the pattern {names.Count}";

		var difference = Parts(text, ("type", type.Type, match.Groups["type"].Value), ("subtype", type.Subtype, match.Groups["subtype"].Value));

		for (var i = 0; difference is null && i < names.Count; i++)
			difference = Parts(text, ($"parameter {i} name", type.Parameters[i].Name, names[i].Value), ($"parameter {i} value", type.Parameters[i].Value, Unquoted(values[i].Value)));

		return difference;
	}

	// ── FIX, as a pattern ───────────────────────────────────────────────────────

	/// <summary>
	/// What FIX text is to a regular expression: <c>(\d+)=([^\x01]*)\x01</c>, the split into a tag
	/// and a value, with no typing of the value, no length/data pair and no recovery. It is
	/// much less work than any parser here does, and the rows say so in the reading's name.
	/// </summary>
	static readonly (Lazy<Regex> Interpreted, Lazy<Regex> Compiled) FixSplit =
		Both(@"(\d+)=([^\x01]*)\x01", RegexOptions.CultureInvariant);

	static int FixRegex(Regex regex, string text)
	{
		var count = 0;

		foreach (Match match in regex.Matches(text))
			count += int.Parse(match.Groups[1].ValueSpan, NumberStyles.None, CultureInfo.InvariantCulture);

		return count;
	}

	/// <summary>
	/// The hand parser's fields held to the pattern's matches: as many, the same tags, and
	/// each value where the pattern found it. Only plain rows are held to it — a length/data
	/// pair or a malformed field is a row the pattern is not asked to read.
	/// </summary>
	static string? FixRegexDisagreement(string text, IEnumerable<FixField> hand, Regex regex)
	{
		var fields  = hand.ToArray();
		var matches = regex.Matches(text);

		if (matches.Count != fields.Length)
			return $"  the hand parser read {fields.Length} fields, the pattern {matches.Count}";

		for (var i = 0; i < fields.Length; i++)
		{
			var value = matches[i].Groups[2];

			if (fields[i].Tag != int.Parse(matches[i].Groups[1].ValueSpan, NumberStyles.None, CultureInfo.InvariantCulture) ||
				fields[i].ValuePosition != value.Index || fields[i].Length != value.Length)
			{
				return $"  field {i}: the hand parser has tag {fields[i].Tag} at {fields[i].ValuePosition} for {fields[i].Length}, the pattern '{matches[i].Value.TrimEnd('\u0001')}'";
			}
		}

		return null;
	}
}
