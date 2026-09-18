using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using DotGram.Finance.Fix;
using DotGram.Web;

namespace DotGram.Benchmarks;

// The web's formats, and what a regular expression says of them (architect, for Igor,
// 2026-09-18: "benchmarks of all the parsers, regex included").
//
// A row here has no hand-written reading, because there is no hand-written parser of these
// formats: its first reading, the base every ratio is taken against, is the generated one.
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

	static IEnumerable<Workload> WebWorkloads()
	{
		return
		[
			.. WebUrls(),

			// RFC 8259: a recursive language; a regex cannot read it, so N/A.
			WebGenerated("json.object",
				"{\"id\": 12345, \"name\": \"dotgram\", \"tags\": [\"parser\", \"generator\", \"analyzer\"], \"nested\": {\"x\": 1.5, \"y\": null, \"z\": true}, \"text\": \"a string with an escape \\n and a unicode one \\u00e9\", \"list\": [1, 2, 3, 4, 5]}",
				static text => JsonValue.TryParse(text, out _)),
			WebGenerated("json.array",
				"[1, 2.5, -3, \"four\", true, false, null, [5, 6], {\"k\": \"v\"}, 1e3, \"a longer string to read\", 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58]",
				static text => JsonValue.TryParse(text, out _)),

			.. WebTimestamps(),
			.. WebAddresses(),
			.. WebMediaTypes(),

			// RFC 9651: a small language of its own with nested lists and parameters; N/A.
			WebGenerated("sf.item", "42;unit=\"s\";exact=?1", static text => StructuredField.TryParseItem(text, out _)),
			WebGenerated("sf.list", "\"foo\", bar;baz=?1, (1 2 3);q=0.5, :aGVsbG8=:, 10.5", static text => StructuredField.TryParseList(text, out _)),
			WebGenerated("sf.dictionary", "a=1, b=?0, c=\"text\", d=:aGVsbG8=:, e=(1 2);f=3", static text => StructuredField.TryParseDictionary(text, out _)),
		];
	}

	// ── RFC 3986 ────────────────────────────────────────────────────────────────

	/// <summary>
	/// The five inputs of <see cref="UrlBenchmarks"/> and the pattern it transcribes from the
	/// example URL grammar, held to the RFC 3986 parser part by part. Lesser: the pattern
	/// takes three schemes and no relative references, which the RFC's parser takes.
	/// </summary>
	static IEnumerable<Workload> WebUrls()
	{
		string[] names   = ["plain", "full", "ipv4", "long-path", "refused"];
		var      inputs  = UrlBenchmarks.Inputs.ToArray();
		var      (interpreted, compiled) = Both(UrlBenchmarks.Pattern);

		for (var i = 0; i < inputs.Length; i++)
		{
			var text = inputs[i];

			yield return Web(
				"url." + names[i],
				[
					new Reading("generated",      () => UrlGenerated(text)),
					new Reading("regex",          () => UrlRegex(interpreted.Value, text)),
					new Reading("regex-compiled", () => UrlRegex(compiled.Value, text)),
				],
				() => UrlDisagreement(compiled.Value, text));
		}
	}

	static int UrlGenerated(string text)
	{
		if (!UriReference.TryParse(text, out var url))
			return 0;

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
		var match    = regex.Match(text);

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
					new Reading("generated",      () => TimestampGenerated(text)),
					new Reading("regex",          () => TimestampRegex(interpreted.Value, text, out _)),
					new Reading("regex-compiled", () => TimestampRegex(compiled.Value, text, out _)),
				],
				() => TimestampDisagreement(compiled.Value, text));
		}
	}

	static int TimestampGenerated(string text)
	{
		if (!Timestamp.TryParse(text, out var timestamp))
			return 0;

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
		var match    = regex.Match(text);

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
