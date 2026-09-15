using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>A Set-Cookie header field as a user agent reads it (RFC 6265 §5.2): a name, a value, and cookie-av attributes.</summary>
/// <remarks>
/// <para>
/// <see cref="Attributes"/> is every cookie-av as §5.2 divides it, recognized or not, in order. The properties are
/// what §5.2's subsections and §5.3 make of them: an attribute whose value §5.2 ignores counts for nothing, and
/// where a name is written more than once the last that counts is the one taken.
/// </para>
/// <para>
/// What the cookie means for a store — its domain against the request host, its default path, whether it has
/// expired — needs the request, which is not here; <see cref="Rfc6265.DomainMatches"/>,
/// <see cref="Rfc6265.DefaultPath"/>, <see cref="Rfc6265.PathMatches"/> and <see cref="ExpiryTime"/> are §5.1
/// and §5.3's pieces of that.
/// </para>
/// </remarks>
/// <param name="Name">The cookie-name, whitespace trimmed; never empty.</param>
/// <param name="Value">The cookie-value, whitespace trimmed, quotes and all.</param>
/// <param name="Attributes">Each cookie-av's name and value, whitespace trimmed, in order.</param>
public sealed record SetCookie(string Name, string Value, IReadOnlyList<SetCookie.Attribute> Attributes)
{
	/// <summary>A cookie-av: its name and its value, empty where there was no <c>=</c> or nothing after it.</summary>
	public sealed record Attribute(string Name, string Value);

	/// <summary>§5.2.1: the last Expires whose value is a cookie-date, or null.</summary>
	public DateTimeOffset? Expires
	{
		get
		{
			DateTimeOffset? found = null;

			foreach (var attribute in Named("Expires"))
				if (Rfc6265.TryParseCookieDate(attribute.Value, out var date))
					found = date;

			return found;
		}
	}

	/// <summary>§5.2.2: the last Max-Age whose value is an optional <c>-</c> and digits, in seconds, or null.</summary>
	/// <remarks>Zero or less expires the cookie at once. A number past a long's range is the nearest end of it.</remarks>
	public long? MaxAge
	{
		get
		{
			long? found = null;

			foreach (var attribute in Named("Max-Age"))
				if (DeltaSeconds(attribute.Value) is { } seconds)
					found = seconds;

			return found;
		}
	}

	/// <summary>§5.2.3: the last Domain with a value, its leading <c>.</c> taken off and in lower case, or null.</summary>
	public string? Domain
	{
		get
		{
			string? found = null;

			foreach (var attribute in Named("Domain"))
				if (attribute.Value.Length > 0)
					found = (attribute.Value[0] == '.' ? attribute.Value.Substring(1) : attribute.Value).ToLowerInvariant();

			return found;
		}
	}

	/// <summary>§5.2.4: the value of the last Path, or null where there is none or its value is not absolute.</summary>
	/// <remarks>Null means the default-path of the request (<see cref="Rfc6265.DefaultPath"/>), as §5.3 step 7 has it.</remarks>
	public string? Path
	{
		get
		{
			string? found = null;
			var any = false;

			foreach (var attribute in Named("Path"))
			{
				any = true;
				found = attribute.Value.Length > 0 && attribute.Value[0] == '/' ? attribute.Value : null;
			}

			return any ? found : null;
		}
	}

	/// <summary>§5.2.5.</summary>
	public bool Secure => Named("Secure").GetEnumerator().MoveNext();

	/// <summary>§5.2.6.</summary>
	public bool HttpOnly => Named("HttpOnly").GetEnumerator().MoveNext();

	/// <summary>
	/// §5.3 step 3: when the cookie expires if it is received at <paramref name="now"/> — Max-Age before Expires, the
	/// earliest representable time for a Max-Age of zero or less — or null for a cookie that lasts the session.
	/// </summary>
	public DateTimeOffset? ExpiryTime(DateTimeOffset now)
	{
		if (MaxAge is { } seconds)
		{
			if (seconds <= 0)
				return DateTimeOffset.MinValue;

			return seconds >= (DateTimeOffset.MaxValue - now).TotalSeconds ? DateTimeOffset.MaxValue : now.AddSeconds(seconds);
		}

		return Expires;
	}

	public bool Equals(SetCookie? other)
	{
		if (other is null ||
			!string.Equals(Name, other.Name, StringComparison.Ordinal) ||
			!string.Equals(Value, other.Value, StringComparison.Ordinal) ||
			Attributes.Count != other.Attributes.Count)
		{
			return false;
		}

		for (var index = 0; index < Attributes.Count; index++)
			if (!string.Equals(Attributes[index].Name, other.Attributes[index].Name, StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(Attributes[index].Value, other.Attributes[index].Value, StringComparison.Ordinal))
			{
				return false;
			}

		return true;
	}

	public override int GetHashCode()
	{
		var hash = Structural.Combine(StringComparer.Ordinal.GetHashCode(Name), StringComparer.Ordinal.GetHashCode(Value));

		foreach (var attribute in Attributes)
			hash = Structural.Combine(
				Structural.Combine(hash, StringComparer.OrdinalIgnoreCase.GetHashCode(attribute.Name)),
				StringComparer.Ordinal.GetHashCode(attribute.Value));

		return hash;
	}

	/// <summary>The field value: the pair, then each attribute after <c>; </c>, with <c>=</c> where it has a value.</summary>
	public override string ToString()
	{
		var output = new StringBuilder(Name).Append('=').Append(Value);

		foreach (var attribute in Attributes)
		{
			output.Append("; ").Append(attribute.Name);

			if (attribute.Value.Length > 0)
				output.Append('=').Append(attribute.Value);
		}

		return output.ToString();
	}

	IEnumerable<Attribute> Named(string name)
	{
		foreach (var attribute in Attributes)
			if (string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))
				yield return attribute;
	}

	// §5.2.2: a DIGIT or `-` first, and DIGITs after it.
	static long? DeltaSeconds(string value)
	{
		if (value.Length == 0 || value[0] is not ('-' or >= '0' and <= '9') || value == "-")
			return null;

		for (var at = 1; at < value.Length; at++)
			if (value[at] is < '0' or > '9')
				return null;

		var negative = value[0] == '-';
		var seconds  = 0L;

		for (var at = negative ? 1 : 0; at < value.Length; at++)
		{
			if (seconds > (long.MaxValue - 9) / 10)
				return negative ? long.MinValue : long.MaxValue;

			seconds = seconds * 10 + (value[at] - '0');
		}

		return negative ? -seconds : seconds;
	}
}

/// <summary>A cookie-pair of a Cookie header field (RFC 6265 §4.2.1): a name and its value as written.</summary>
/// <param name="Value">With its double quotes where it had them: §4.1.1 makes them part of the cookie-value.</param>
public sealed record CookiePair(string Name, string Value);

// RFC 6265, HTTP State Management Mechanism. Two readings, because the RFC has two:
//
//   * §5.2's, a user agent's: a Set-Cookie field divided at `;` and `=` with whitespace trimmed, which reads
//     nearly anything and refuses only a pair with no `=` or with no name. §4.1.1's grammar for a server is
//     narrower, and §5 says in so many words that a user agent reads by the algorithm and not by it.
//   * §4.2.1's, a server's: the Cookie field a user agent following §5.4 sends, `name=value` pairs after `; `.
//
// Dates are §5.1.1: the grammar divides a date into tokens and says which production each one matches, and
// the steps that choose among them, with their flags, are C#. The productions are publications of their own,
// internal, since a token matches one only as a whole. A month name matches whatever its case, as erratum
// 8877 (reported) and every user agent have it. Erratum 4148 (verified) makes the tail after a day-of-month
// optional, as it is written here; erratum 3444 (verified) is in §4.1.1's path-value, which is not read.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	// ── §5.2 ─────────────────────────────────────────────────────────────────────

	// set-cookie-string: the name-value-pair up to the first `;`, then each cookie-av after one.
	SetCookieString : @SetCookie
		= pair: NameValuePair & attributes: CookieAvs & when @(Rfc6265.IsNameValuePair(pair!))
		=> @(Rfc6265.Cookie(pair, attributes))

	NameValuePair = [^ ';']*

	CookieAvs : @SetCookie.Attribute[] = items: CookieAv* => @(items)

	CookieAv : @SetCookie.Attribute = ';' & text: CookieAvText => @(Rfc6265.Attribute(text))

	CookieAvText = [^ ';']*

	// ── §4.2.1 ───────────────────────────────────────────────────────────────────

	// cookie-header = "Cookie:" OWS cookie-string OWS; cookie-string = cookie-pair *( ";" SP cookie-pair ).
	CookieString : @CookiePair[] = Ows & first: CookiePairRule & rest: NextCookiePair* & Ows => @(Rfc6265.Joined(first, rest))

	NextCookiePair : @CookiePair = ';' & ' ' & pair: CookiePairRule => @(pair)

	CookiePairRule : @CookiePair = name: Token & '=' & value: CookieValue => @(new CookiePair(name, value))

	// The quoted form first, as erratum 8242 (held for document update) asks of an ordered choice.
	CookieValue = '"' & CookieOctet* & '"' | CookieOctet*

	CookieOctet = ['!' | '#'..'+' | '-'..':' | '<'..'[' | ']'..'~']

	Ows   = [' ' | '\t']*
	Tchar = ['!' | '#' | '$' | '%' | '&' | '\'' | '*' | '+' | '-' | '.' | '^' | '_' | '`' | '|' | '~' | '0'..'9' | 'a'..'z' | 'A'..'Z']
	Token = Tchar+

	// ── §5.1.1 ───────────────────────────────────────────────────────────────────

	// cookie-date = *delimiter date-token-list *delimiter.
	CookieDate : @string[] = Delimiter* & first: DateToken & rest: NextDateToken* & Delimiter* => @(Rfc6265.Joined(first, rest))

	NextDateToken : @string = Delimiter+ & token: DateToken => @(token)

	DateToken = NonDelimiter+

	Delimiter    = ['\t' | ' '..'/' | ';'..'@' | '['..'`' | '{'..'~']
	NonDelimiter = [^ '\t' | ' '..'/' | ';'..'@' | '['..'`' | '{'..'~']

	// time = hms-time [ non-digit *OCTET ]; the tail is optional for time as erratum 4148 makes it for the rest.
	TimeToken : @int[] = hour: TimeField & ':' & minute: TimeField & ':' & second: TimeField & Tail
		=> @(new[] { Rfc6265.Number(hour), Rfc6265.Number(minute), Rfc6265.Number(second) })

	DayToken : @int = day: TimeField & Tail => @(Rfc6265.Number(day))

	YearToken : @int = year: YearDigits & Tail => @(Rfc6265.Number(year))

	MonthToken : @int
		= "jan"i & any* => @(1)
		| "feb"i & any* => @(2)
		| "mar"i & any* => @(3)
		| "apr"i & any* => @(4)
		| "may"i & any* => @(5)
		| "jun"i & any* => @(6)
		| "jul"i & any* => @(7)
		| "aug"i & any* => @(8)
		| "sep"i & any* => @(9)
		| "oct"i & any* => @(10)
		| "nov"i & any* => @(11)
		| "dec"i & any* => @(12)

	TimeField  = Digit{1,2}
	YearDigits = Digit{2,4}
	Tail       = (NonDigit & any*)?
	Digit      = ['0'..'9']
	NonDigit   = [^ '0'..'9']

	parse SetCookieString as ParseSetCookie
	parse CookieString    as ParseCookies

	internal parse CookieDate as ReadDateTokens
	internal parse TimeToken  as ReadTime
	internal parse DayToken   as ReadDay
	internal parse MonthToken as ReadMonth
	internal parse YearToken  as ReadYear
	""")]
public static partial class Rfc6265
{
	// ParseSetCookie, ParseCookies and their Try forms are generated here, and the date readings beside them.

	/// <summary>A cookie-date as §5.1.1 reads one, in UTC.</summary>
	/// <exception cref="FormatException">The text is no cookie-date.</exception>
	public static DateTimeOffset ParseCookieDate(string text) =>
		TryParseCookieDate(text, out var date) ? date : throw new FormatException($"'{text}' is no cookie-date (RFC 6265 §5.1.1).");

	/// <summary>A cookie-date as §5.1.1 reads one, in UTC, or false where the steps fail.</summary>
	public static bool TryParseCookieDate(string text, out DateTimeOffset date)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		date = default;

		var tokens = TryReadDateTokens(text);

		if (!tokens.IsSuccess)
			return false;

		int[]? time = null;
		int? day = null, month = null, year = null;

		// Step 2: each token matches the first production not yet found that it can.
		foreach (var token in tokens.Value!)
		{
			if (time is null && TryReadTime(token) is { IsSuccess: true } readTime)
				time = readTime.Value;
			else if (day is null && TryReadDay(token) is { IsSuccess: true } readDay)
				day = readDay.Value;
			else if (month is null && TryReadMonth(token) is { IsSuccess: true } readMonth)
				month = readMonth.Value;
			else if (year is null && TryReadYear(token) is { IsSuccess: true } readYear)
				year = readYear.Value;
		}

		if (time is null || day is null || month is null || year is not { } value)
			return false;

		// Steps 3 and 4.
		value += value switch
		{
			>= 70 and <= 99 => 1900,
			>= 0 and <= 69  => 2000,
			_               => 0,
		};

		// Steps 5 and 6.
		if (day is < 1 or > 31 || value < 1601 || time[0] > 23 || time[1] > 59 || time[2] > 59 ||
			day > DateTime.DaysInMonth(value, month.Value))
		{
			return false;
		}

		date = new DateTimeOffset(value, month.Value, day.Value, time[0], time[1], time[2], TimeSpan.Zero);
		return true;
	}

	/// <summary>§5.1.3: whether a canonicalized host name domain-matches a domain string.</summary>
	/// <remarks>Both are compared as given; §5.1.2 canonicalizes them to lower case first, and that is the caller's.</remarks>
	public static bool DomainMatches(string host, string domain)
	{
		if (host is null)
			throw new ArgumentNullException(nameof(host));

		if (domain is null)
			throw new ArgumentNullException(nameof(domain));

		if (string.Equals(host, domain, StringComparison.Ordinal))
			return true;

		return domain.Length > 0 &&
			host.Length > domain.Length &&
			host.EndsWith(domain, StringComparison.Ordinal) &&
			host[host.Length - domain.Length - 1] == '.' &&
			Uri.CheckHostName(host.Trim('[', ']')) is not (UriHostNameType.IPv4 or UriHostNameType.IPv6);
	}

	/// <summary>§5.1.4: the default-path of a request's path — its directory, or <c>/</c>.</summary>
	public static string DefaultPath(string requestPath)
	{
		if (requestPath is null)
			throw new ArgumentNullException(nameof(requestPath));

		if (requestPath.Length == 0 || requestPath[0] != '/')
			return "/";

		var last = requestPath.LastIndexOf('/');

		return last == 0 ? "/" : requestPath.Substring(0, last);
	}

	/// <summary>§5.1.4: whether a request's path path-matches a cookie-path.</summary>
	public static bool PathMatches(string requestPath, string cookiePath)
	{
		if (requestPath is null)
			throw new ArgumentNullException(nameof(requestPath));

		if (cookiePath is null)
			throw new ArgumentNullException(nameof(cookiePath));

		if (string.Equals(requestPath, cookiePath, StringComparison.Ordinal))
			return true;

		return requestPath.StartsWith(cookiePath, StringComparison.Ordinal) &&
			(cookiePath.EndsWith("/", StringComparison.Ordinal) || requestPath[cookiePath.Length] == '/');
	}

	// ── What the grammar calls ───────────────────────────────────────────────────

	/// <summary>§5.2 steps 2 and 5: a pair has an <c>=</c>, and a name before it once whitespace is trimmed.</summary>
	internal static bool IsNameValuePair(string pair)
	{
		var equals = pair.IndexOf('=');

		return equals >= 0 && Trimmed(pair.Substring(0, equals)).Length > 0;
	}

	internal static SetCookie Cookie(string pair, SetCookie.Attribute[] attributes)
	{
		var equals = pair.IndexOf('=');

		return new SetCookie(Trimmed(pair.Substring(0, equals)), Trimmed(pair.Substring(equals + 1)), attributes);
	}

	/// <summary>§5.2 steps 4 and 5 of the attributes: a cookie-av's name and value, divided at the first <c>=</c> and trimmed.</summary>
	internal static SetCookie.Attribute Attribute(string text)
	{
		var equals = text.IndexOf('=');

		return equals < 0
			? new SetCookie.Attribute(Trimmed(text), "")
			: new SetCookie.Attribute(Trimmed(text.Substring(0, equals)), Trimmed(text.Substring(equals + 1)));
	}

	internal static T[] Joined<T>(T first, T[] rest)
	{
		var all = new T[1 + rest.Length];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	internal static int Number(string digits) => int.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);

	// WSP is SP and HTAB (RFC 5234), and nothing else is trimmed.
	static string Trimmed(string text) => text.Trim(' ', '\t');
}
