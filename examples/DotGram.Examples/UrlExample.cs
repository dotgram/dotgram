using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using DotGram;

namespace DotGram.Examples;

// A URL parser, after RFC 3986.
//
// The grammar is the argument to [Gram] instead of a file beside it: a single line
// ending in ".gram" is read as a path, anything else is the grammar itself.
//
// A capture sits where the value is, not where the syntax is — the '@' after a
// userinfo and the ':' before a port are punctuation, so they stay outside and the
// result holds "8080" rather than ":8080".

[Gram("""
	Url        = scheme: Scheme & "://" & authority: Authority & path: Path
	           & ('?' & query: Rest)? & ('#' & fragment: Rest)?

	Scheme     = "https" | "http" | "ftp"

	// A userinfo is followed by '@' and nothing else is, so trying the group and
	// giving it back is the whole of "is there a userinfo here".
	Authority  = (user: UserInfo & '@')? & host: Host & (':' & port: Digit+)?
	UserInfo   = (Unreserved | SubDelim | PctEncoded | ':')+
	Host       = IPv4 | RegName

	IPv4       = Octet & '.' & Octet & '.' & Octet & '.' & Octet
	Octet      = Digit{1,3}
	RegName    = (Unreserved | SubDelim | PctEncoded)+

	Path       = ('/' & Segment)*
	Segment    = (Unreserved | SubDelim | PctEncoded | ':' | '@')*
	Rest       = (Unreserved | SubDelim | PctEncoded | ':' | '@' | '/' | '?')*
	PctEncoded = '%' & Hex & Hex

	Digit      = ['0'..'9']
	Hex        = [Digit | 'a'..'f' | 'A'..'F']
	Unreserved = [Digit | 'a'..'z' | 'A'..'Z' | '-' | '.' | '_' | '~']
	SubDelim   = ['!' | '$' | '&' | '\'' | '(' | ')' | '*' | '+' | ',' | ';' | '=']

	parse Url
	find Url as AllUrls
	""")]
public static partial class Links
{
	// ParseUrl, TryParseUrl, AllUrls and the types Url and Authority are generated into
	// this class: the attribute goes on the class the parser is wanted in, and there is
	// nothing else to wire up. (Give the grammar a class of its own when the generated
	// methods should not be part of your API — see examples/README.md.)

	/// <summary>Whether the whole input is a URL.</summary>
	public static bool IsUrl(string text) => TryParseUrl(text).IsSuccess;

	/// <summary>The port, or 443 for https and 80 for the rest.</summary>
	public static int PortOf(string url)
	{
		var parsed = ParseUrl(url);

		return parsed.Authority.Port is { } port
			? int.Parse(port, CultureInfo.InvariantCulture)
			: parsed.Scheme == "https" ? 443 : 80;
	}

	/// <summary>The hosts of every URL in a piece of prose, in order, without repeats.</summary>
	public static IReadOnlyList<string> HostsIn(string prose)
	{
		var seen  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var hosts = new List<string>();

		// AllUrls is lazy — occurrences are found as they are asked for, so this reads a
		// hundred-megabyte document one match at a time rather than as an array of them.
		foreach (var found in AllUrls(prose))
			if (seen.Add(found.Value!.Authority.Host))
				hosts.Add(found.Value!.Authority.Host);

		return hosts;
	}

	/// <summary>A URL broken into its parts, one per line.</summary>
	/// <exception cref="FormatException">The input is not a URL.</exception>
	public static string Describe(string url)
	{
		var parsed = ParseUrl(url);
		var text   = new StringBuilder();

		Line("scheme",   parsed.Scheme);
		Line("user",     parsed.Authority.User);
		Line("host",     parsed.Authority.Host);
		Line("port",     parsed.Authority.Port);
		Line("path",     parsed.Path);
		Line("query",    parsed.Query);
		Line("fragment", parsed.Fragment);

		return text.ToString();

		// A part that was never there is null, which is a different answer from a part
		// that matched and was empty: "?q=" has a query, "example.com" has none.
		void Line(string name, string? value) =>
			text.Append(name.PadRight(9)).Append(value ?? "—").Append('\n');
	}
}
