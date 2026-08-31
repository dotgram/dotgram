using System;
using System.Globalization;
using System.Text;

using DotGram;

namespace DotGram.Parsers;

/// <summary>The parts of a URI reference, as RFC 3986 divides it.</summary>
/// <remarks>
/// Every part is the text as it was written, undecoded. RFC 3986 §2.4 is explicit that
/// when to decode a percent-escape is the application's question and not the parser's:
/// decoding `%2F` inside a path segment would make it a separator it is not. So the parts
/// come back as they stood, and <see cref="Rfc3986.Decode"/> is beside them for a caller
/// that has decided.
/// </remarks>
/// <param name="Scheme">`http`, or null in a relative reference.</param>
/// <param name="UserInfo">What stood before the `@`, or null.</param>
/// <param name="Host">The registered name, IPv4 address or bracketed IP literal, or null.</param>
/// <param name="Port">The digits after the `:`, or null. May be empty, which RFC 3986 allows.</param>
/// <param name="Path">Always present; the empty string where the reference has none.</param>
/// <param name="Query">What stood after the `?`, or null. Null and empty are different.</param>
/// <param name="Fragment">What stood after the `#`, or null. Null and empty are different.</param>
public sealed record UriParts(
	string? Scheme,
	string? UserInfo,
	string? Host,
	string? Port,
	string Path,
	string? Query,
	string? Fragment);

// RFC 3986, whole. Not the `Url.gram` of the test corpus, which is the same shape cut down
// to what a benchmark needs — this one is the specification, and the difference is where
// the interest is:
//
//   * `dec-octet` is the real one. The corpus grammar writes an octet as `Digit{1,3}`,
//     which reads `999` as an address; here it is the RFC's five alternatives, so `256`
//     is a registered name and `255` is an address.
//   * `IPvFuture` exists. It has never been assigned, which is why the corpus leaves it
//     out and why a parser claiming the specification cannot.
//   * A reference may be relative. `//host/path`, `/path`, `path`, `../path` and the
//     empty string are all URI references, and telling them apart is most of what the
//     `hier-part` and `relative-part` alternatives are for.
//   * The four path forms are distinguished, which is the part of the grammar that reads
//     least like a path and matters most: `path-noscheme` is what keeps `a:b` from being
//     a relative path with a colon in its first segment.
//
// **What it is written in terms of is the ABNF, rule for rule.** Appendix A can be read
// beside it line by line, and that is deliberate: a parser for a specification should be
// checkable against the specification by eye, and a rule that has been cleverly merged
// with the one below it cannot be. Where this file departs from the ABNF it says so.
//
// **Two departures**, both forced by ordered choice rather than chosen:
//
//   * `IPv6address`'s nine alternatives are written longest-first. ABNF's `/` is unordered
//     and a matcher may take them in any order; ordered choice takes the first that
//     matches, and a shorter one that is a prefix of a longer would win where the longer
//     was meant. The RFC's own order happens to be right, and this says so out loud
//     because it is load-bearing here and merely stylistic there.
//   * `path-abempty` and friends are reached through the `hier-part` alternatives in the
//     RFC's order, and `path-empty` is last because everything matches it.

[Gram("""
	@using System;
	@using DotGram.Parsers;

	using Lexical;

	// The whole of RFC 3986 is punctuation and character classes: nothing may be skipped
	// between any two of them, and a URI with a space in it is not a URI.
	namespace Lexical
	{
		trivia = none

		// §2.3, §2.2 — the character sets everything else is built from.
		Unreserved = ['a'..'z' | 'A'..'Z' | '0'..'9' | '-' | '.' | '_' | '~']
		SubDelims  = ['!' | '$' | '&' | '\'' | '(' | ')' | '*' | '+' | ',' | ';' | '=']
		Hexdig     = ['0'..'9' | 'a'..'f' | 'A'..'F']
		Digit      = ['0'..'9']
		Alpha      = ['a'..'z' | 'A'..'Z']

		// §2.1. Two hex digits, and the `%` is part of the text a caller gets back.
		PctEncoded = '%' & Hexdig & Hexdig

		// §3.3. A path segment's characters, and the two sets that are not quite it.
		Pchar       = Unreserved | PctEncoded | SubDelims | ':' | '@'
		Segment     = Pchar*
		SegmentNz   = Pchar+

		// The one that keeps `a:b` from being a relative path: a first segment with no
		// colon in it, because a colon there would have made the thing before it a scheme.
		SegmentNzNc = (Unreserved | PctEncoded | SubDelims | '@')+

		// §3.1. A scheme begins with a letter, which is what makes `1a:` not one.
		SchemeText = Alpha & [ 'a'..'z' | 'A'..'Z' | '0'..'9' | '+' | '-' | '.' ]*

		// §3.2.1 and §3.2.3.
		UserInfoText = (Unreserved | PctEncoded | SubDelims | ':')*
		PortText     = Digit*

		// §3.2.2. The RFC's own five alternatives, longest first — `25` before `2`, and
		// `1` before a bare digit — because ordered choice takes the first that matches
		// where ABNF's `/` may take any.
		DecOctet = "25" & ['0'..'5']
		         |  '2' & ['0'..'4'] & Digit
		         |  '1' & Digit & Digit
		         | ['1'..'9'] & Digit
		         | Digit

		IPv4Address = DecOctet & '.' & DecOctet & '.' & DecOctet & '.' & DecOctet

		H16  = Hexdig{1,4}
		Ls32 = H16 & ':' & H16 | IPv4Address

		// §3.2.2, verbatim and in the RFC's order. `::` stands for a run of zero or more
		// groups whose length is known only from how many are written either side of it,
		// and there is no way to say that but by writing out the cases.
		IPv6Address =                                    (H16 & ':'){6} & Ls32
		            |                             "::" & (H16 & ':'){5} & Ls32
		            | H16?                      & "::" & (H16 & ':'){4} & Ls32
		            | ((H16 & ':'){0,1} & H16)? & "::" & (H16 & ':'){3} & Ls32
		            | ((H16 & ':'){0,2} & H16)? & "::" & (H16 & ':'){2} & Ls32
		            | ((H16 & ':'){0,3} & H16)? & "::" & (H16 & ':')    & Ls32
		            | ((H16 & ':'){0,4} & H16)? & "::" & Ls32
		            | ((H16 & ':'){0,5} & H16)? & "::" & H16
		            | ((H16 & ':'){0,6} & H16)? & "::"

		// §3.2.2. Never assigned, and part of the specification all the same.
		IPvFuture = 'v' & Hexdig+ & '.' & (Unreserved | SubDelims | ':')+

		IPLiteral = '[' & (IPv6Address | IPvFuture) & ']'

		// §3.2.2. A registered name is what is left, and it may be empty — `file:///path`
		// has an authority whose host is the empty string.
		RegName = (Unreserved | PctEncoded | SubDelims)*

		HostText = IPLiteral | IPv4Address | RegName

		// §3.4, §3.5. The two differ in nothing but where they stand.
		QueryText    = (Pchar | '/' | '?')*
		FragmentText = (Pchar | '/' | '?')*

		// §3.3, the four forms. Which one is allowed is what the two `-part` rules below
		// decide; what each one *is* belongs here.
		PathAbEmpty  = ('/' & Segment)*
		PathAbsolute = '/' & (SegmentNz & ('/' & Segment)*)?
		PathNoScheme = SegmentNzNc & ('/' & Segment)*
		PathRootless = SegmentNz & ('/' & Segment)*
	}

	// ── The reference, and the parts it comes back as ───────────────────────────

	// §4.1. A reference is a URI or a relative one, and which it is turns on whether what
	// stands before the first `:` is a scheme. Ordered choice asks that by trying.
	UriReference : @UriParts = u: Uri => @(u) | r: RelativeRef => @(r)

	// §3.
	Uri : @UriParts = scheme: SchemeText & ':' & rest: HierPart & ('?' & query: QueryText)? & ('#' & fragment: FragmentText)?
		=> @(rest with { Scheme = scheme, Query = query, Fragment = fragment })

	// §4.2.
	RelativeRef : @UriParts = rest: RelativePart & ('?' & query: QueryText)? & ('#' & fragment: FragmentText)?
		       => @(rest with { Query = query, Fragment = fragment })

	// §3. The order is the RFC's, and `PathEmpty` is last because everything matches it.
	HierPart : @UriParts = "//" & a: Authority & path: PathAbEmpty => @(a with { Path = path })
		     | path: PathAbsolute                                  => @(Rfc3986.Only(path))
		     | path: PathRootless                                  => @(Rfc3986.Only(path))
		     | ""                                                  => @(Rfc3986.Only(""))

	// §4.2. The same, with `PathNoScheme` where `PathRootless` stood: a relative
	// reference may not begin with a segment holding a colon, or the colon would have
	// made a scheme of what came before it.
	RelativePart : @UriParts
		= "//" & a: Authority & path: PathAbEmpty => @(a with { Path = path })
		| path: PathAbsolute                      => @(Rfc3986.Only(path))
		| path: PathNoScheme                      => @(Rfc3986.Only(path))
		| ""                                      => @(Rfc3986.Only(""))

	// §3.2. A userinfo is followed by `@` and nothing else is, so trying the group and
	// giving it back is the whole of "is there a userinfo here".
	Authority : @UriParts
		= (user: UserInfoText & '@')? & host: HostText & (':' & port: PortText)?
		=> @(new UriParts(null, user, host, port, "", null, null))

	parse UriReference as ParseReference
	parse Uri           as ParseUri
	""")]
public static partial class Rfc3986
{
	// ParseReference, TryParseReference, ParseUri and TryParseUri are generated here.

	/// <summary>A reference that has only a path — no authority, and nothing decided yet.</summary>
	/// <remarks>
	/// Three of the four `hier-part` alternatives are a path and nothing else, and the
	/// fourth carries an authority. Both hand back the same shape so that the rule above
	/// can fill in the scheme, query and fragment without asking which it got.
	/// </remarks>
	public static UriParts Only(string path) =>
		new(null, null, null, null, path ?? throw new ArgumentNullException(nameof(path)), null, null);

	/// <summary>One part with its percent-escapes turned back into the bytes they stand for.</summary>
	/// <remarks>
	/// <para>
	/// Not done while parsing, and RFC 3986 §2.4 says why: `%2F` in a path segment is a
	/// slash that is *not* a separator, and a parser that decoded as it went would produce
	/// a path nobody can take apart again. So the parts come back as they were written and
	/// this is for a caller who has decided what a part means.
	/// </para>
	/// <para>
	/// The escapes are bytes rather than characters — §2.5 — so they are gathered and
	/// decoded as UTF-8 together, which is what makes a two-byte escape pair read as one
	/// character rather than two replacement marks.
	/// </para>
	/// </remarks>
	public static string Decode(string part)
	{
		if (part is null)
			throw new ArgumentNullException(nameof(part));

		if (part.IndexOf('%') < 0)
			return part;

		var built = new StringBuilder(part.Length);
		var bytes = new List<byte>();

		for (var at = 0; at < part.Length;)
		{
			if (part[at] == '%' && at + 2 < part.Length &&
				byte.TryParse(
					part.Substring(at + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
			{
				bytes.Add(value);
				at += 3;

				continue;
			}

			if (bytes.Count > 0)
			{
				built.Append(Encoding.UTF8.GetString([.. bytes]));
				bytes.Clear();
			}

			built.Append(part[at]);
			at++;
		}

		if (bytes.Count > 0)
			built.Append(Encoding.UTF8.GetString([.. bytes]));

		return built.ToString();
	}
}
