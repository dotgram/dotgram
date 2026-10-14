using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>One link-value of a <c>Link</c> header field, as RFC 8288 §3 serialises it.</summary>
/// <remarks>
/// <para>
/// A link-value whose <c>rel</c> names several relation types establishes that many links, sharing a
/// target, a context and attributes (§3.3). They are kept here as the one value they were written as,
/// with every type in <see cref="Relations"/>.
/// </para>
/// <para>
/// The target and the anchor are the URI-References as written. Resolving a relative one needs the URL
/// of the representation that carried the field, which is not in the field, so it is the caller's.
/// </para>
/// </remarks>
/// <param name="Target">The URI-Reference between the angle brackets.</param>
/// <param name="Parameters">Every link-param in the order written, names in lowercase, values unquoted.</param>
public sealed record WebLink(string Target, IReadOnlyList<WebLink.Parameter> Parameters)
{
	/// <summary>A link-param: its name, its value after unquoting, and its RFC 8187 value where it has one.</summary>
	/// <param name="Name">Lowercase: parameter names are case-insensitive.</param>
	/// <param name="Value">The token or the quoted-string's content; empty where the name stands alone.</param>
	/// <param name="Extended">
	/// For a name ending in <c>*</c>, the value decoded as RFC 8187 says; null where the name has no
	/// <c>*</c> or the value is not an ext-value, which a recipient may ignore (RFC 8187 §3.2.1).
	/// </param>
	public sealed record Parameter(string Name, string Value, ExtendedValue? Extended);

	/// <summary>A Link field value (RFC 8288 §3): its link-values in order, empty list elements left out.</summary>
	/// <exception cref="FormatException">The text is no Link field; the message says where.</exception>
	public static WebLink[] ParseField(string text)
	{
		return Rfc8288.ParseLinks(text ?? throw new ArgumentNullException(nameof(text)));
	}

	/// <summary>A Link field value, or false where the text is not one.</summary>
	public static bool TryParseField(string text, [NotNullWhen(true)] out WebLink[]? links)
	{
		var read = Rfc8288.TryParseLinks(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		links = read ? parsed : null;

		return read;
	}

	/// <summary>
	/// A Link field value held to RFC 8288's ABNF to the letter: a registered relation type in <c>rel</c> or
	/// <c>rev</c> has to be lowercase, as <c>reg-rel-type</c> is written, where <see cref="ParseField"/> takes
	/// it in any case.
	/// </summary>
	/// <exception cref="FormatException">The text is no Link field by the ABNF; the message says where.</exception>
	public static WebLink[] ParseStrictField(string text)
	{
		return Rfc8288.ParseStrictLinks(text ?? throw new ArgumentNullException(nameof(text)));
	}

	/// <summary>A Link field value held to the ABNF to the letter, or false where the text is not one.</summary>
	public static bool TryParseStrictField(string text, [NotNullWhen(true)] out WebLink[]? links)
	{
		var read = Rfc8288.TryParseStrictLinks(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		links = read ? parsed : null;

		return read;
	}

	/// <summary>Equal to another link-value with the same target and equal parameters in the same order.</summary>
	public bool Equals(WebLink? other)
	{
		return other is not null && string.Equals(Target, other.Target, StringComparison.Ordinal) && Structural.Same(Parameters, other.Parameters);
	}

	/// <summary>A hash over the target and the parameters.</summary>
	public override int GetHashCode()
	{
		return Structural.Combine(StringComparer.Ordinal.GetHashCode(Target), Structural.Hash(Parameters));
	}

	/// <summary>The relation types of the first <c>rel</c> (§3.3), which later ones do not replace.</summary>
	/// <remarks>As written: compare them without case, as §2.1.1 does.</remarks>
	public IReadOnlyList<string> Relations =>
		First("rel") is { } rel ? rel.Value.Split([' '], StringSplitOptions.RemoveEmptyEntries) : [];

	/// <summary>The first <c>anchor</c>, the link's context where it is not the representation's URL (§3.2).</summary>
	public string? Anchor => First("anchor")?.Value;

	/// <summary>The title: the first <c>title*</c> that decoded, and otherwise the first <c>title</c> (§3.4.1).</summary>
	public string? Title => First("title*")?.Extended?.Value ?? First("title")?.Value;

	/// <summary>The first <c>type</c>, a hint at the target's media type (§3.4.1).</summary>
	public string? Type => First("type")?.Value;

	/// <summary>The first <c>media</c> (§3.4.1).</summary>
	public string? Media => First("media")?.Value;

	/// <summary>Every <c>hreflang</c>, since several say several languages are available (§3.4.1).</summary>
	public IReadOnlyList<string> HrefLangs
	{
		get
		{
			var languages = new List<string>();

			foreach (var parameter in Parameters)
				if (parameter.Name == "hreflang")
					languages.Add(parameter.Value);

			return languages;
		}
	}

	/// <summary>The first parameter of this name — the one §3 says counts where a name may appear once.</summary>
	public Parameter? First(string name)
	{
		foreach (var parameter in Parameters)
			if (string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))
				return parameter;

		return null;
	}
}

/// <summary>An RFC 8187 extended parameter value: a character encoding, a language, and the text it spells.</summary>
/// <param name="Charset">As written; <c>UTF-8</c> is the one senders may use.</param>
/// <param name="Language">A language tag, or null where the value gives none.</param>
/// <param name="Value">The text decoded; null where the charset is one this does not decode.</param>
public sealed record ExtendedValue(string Charset, string? Language, string? Value);

// RFC 8288, Web Linking, §3: the Link header field. The grammar is its ABNF over RFC 9110's rules —
// `#link-value` a list whose empty elements a recipient accepts (RFC 9110 §5.6.1), a parameter's value a
// token or a quoted-string — and what the ABNF names from elsewhere is asked of what is already here:
//
//   * The target and the anchor are URI-References, and Rfc3986 says whether one is.
//   * A value named with a trailing `*` is RFC 8187's ext-value — a charset, an optional language that
//     Rfc5646 reads, and pct-encoded octets. UTF-8 is decoded, and ISO-8859-1 as RFC 8187 encourages.
//     A value that is not an ext-value keeps its text and no decoding, which RFC 8187 §3.2.1 allows.
//
// Appendix B's algorithms are more lenient than the ABNF and say that they are advisory; the body of
// the RFC is normative, so a field that the ABNF does not make is refused whole. A link-value without a
// `rel` is well-formed and has no relations: §3.3's MUST binds the sender.
//
// The ABNF the RFC gives a parameter's value, after unquoting, is asked of it as well: `rel` and `rev`
// are relation types separated by spaces, each a registered name or an absolute URI;
// `anchor` is a URI-Reference; `hreflang` a language tag; `type` RFC 6838's type and subtype. Of `rel`
// and `type`, which may appear once, only the first is asked, since a parser ignores the rest (§3.3,
// §3.4.1). `media` is CSS's media-query-list, which is not read here, and `title*` keeps a value that
// does not decode, as RFC 8187 allows.
//
// A registered relation type is where the ABNF and practice part. `reg-rel-type` is LOALPHA and the
// rest, so `rel=Next` is not one; but §2.1.1 compares registered types without case, Appendix B lowers
// them, and HTML's link types are matched without case. So there are two
// readings, as RFC 5322's are read here twice: ParseLinks takes a registered type in any case, and
// ParseStrictLinks only in lowercase. They differ in nothing else.

// Carried immediately, by name: a link is built the moment its rule has been read, and nothing is
// recorded on a tape and replayed once the parse is accepted. Named, though left to choose the
// generator carries it so too, because the tape would hold its constructions back where: an element
// and its parameters can be asked again after they have answered, and a construction asked again
// runs again. Every construction here builds a WebLink or a Parameter from the text it was given —
// `Parameter` takes a name and what followed it apart, and `Extended` answers null rather than
// throwing where an ext-value does not decode — and reads nothing else, so a reading given up or
// asked twice leaves nothing behind and nothing can throw. Held to the tape by an agreement run:
// the same grammar compiled on the tape, over the tests' texts, each cut at every character and
// given a stray one — the same values, the same messages, nothing thrown.
[Gram("""
	@using System;
	@using System.Collections.Generic;
	@using DotGram.Web;

	trivia = none

	// ── RFC 9110 §5.6 ────────────────────────────────────────────────────────────

	Ows   = [' ' | '\t']*
	Tchar = ['!' | '#' | '$' | '%' | '&' | '\'' | '*' | '+' | '-' | '.' | '^' | '_' | '`' | '|' | '~' | '0'..'9' | 'a'..'z' | 'A'..'Z']
	// A token reads to its end and never hands a character back: nothing that may follow one begins with a
	// tchar (whitespace, `;`, `,`, `=` or the end follow a token), so a shorter reading can never lead anywhere. Without the braces a refused field tried every
	// shorter one, each try with the choice state the longer reading had left behind.
	Token = { Tchar+ }

	// qdtext and quoted-pair; obs-text is any octet from 0x80.
	QuotedString = '"' & ([' ' | '\t' | '!' | '#'..'[' | ']'..'~' | '\u0080'..'\u00FF'] | '\\' & [' ' | '\t' | '!'..'~' | '\u0080'..'\u00FF'])* & '"'

	// ── Link ─────────────────────────────────────────────────────────────────────

	// `#link-value`: empty elements before, between and after, and nothing else between two values.
	// The list is atomic for the same reason RFC 7239's is: unsealed, a refused field (an
	// unclosed `<` after n good links, say) gives a link back a retry and replays the whole
	// list from its start each time, squaring both the count and the bytes it materialises —
	// 414 MB at 1,024 links. The greedy reading is the only one a comma list ever meant.
	Field : @WebLink[] = Ows & (',' & Ows)* & { links: Element* } => @(links)

	Element : @WebLink = link: LinkValue & Ows & ((',' & Ows)+ | ?!any) => @(link)

	// The values §3.2, §3.3 and §3.4.1 give a syntax are held to it once the parameters are read, which is
	// where the first of a once-only parameter is known. Each entry has one of the two alternatives: the
	// strict reading asks a registered relation type to be lowercase, as reg-rel-type is; the other takes
	// it in any case, as relation types compare (§2.1.1) and as Appendix B and HTML read them.
	LinkValue : @WebLink
		= '<' & target: Target & '>' & parameters: Parameters & when Reading is "strict"
		  & when @(Rfc8288.AreValid(parameters!, strict: true)) => @(new WebLink(target, parameters))
		| '<' & target: Target & '>' & parameters: Parameters & when Reading is "practice"
		  & when @(Rfc8288.AreValid(parameters!, strict: false)) => @(new WebLink(target, parameters))

	Reading = "strict" | "practice"

	// A rule of its own for the same reason as Target: the guard above holds no repetition.
	Parameters : @WebLink.Parameter[] = parameters: LinkParam* => @(parameters)

	// A URI-Reference, as Rfc3986 says. The check is a rule of its own, so that no guarded rule holds a
	// repetition the reader would divide into a part.
	Target : @string = text: TargetText & when @(Rfc3986.TryParseReference(text!, out _)) => @(text)

	TargetText = [^ '>']*

	// link-param = token BWS [ "=" BWS ( token / quoted-string ) ]. The value is a rule that may read
	// nothing, and what it read is taken apart beside the name.
	LinkParam : @WebLink.Parameter = Ows & ';' & Ows & name: Token & Ows & assigned: Assignment => @(Rfc8288.Parameter(name, assigned))

	Assignment = ('=' & Ows & (Token | QuotedString))?

	parse Field with (Reading = "practice") as ParseLinks
	parse Field with (Reading = "strict")   as ParseStrictLinks
	""", Carrier = GramCarrier.Immediate)]
static partial class Rfc8288
{
	// ParseLinks, ParseStrictLinks and their Try forms are generated here.

	/// <summary>A link-param from its name and what followed it: nothing, or <c>=</c>, space and a value.</summary>
	internal static WebLink.Parameter Parameter(string name, string assigned)
	{
		var lower = name.ToLowerInvariant();

		if (assigned.Length == 0)
			return new WebLink.Parameter(lower, "", null);

		var at     = 1;
		var quoted = false;

		while (assigned[at] is ' ' or '\t')
			at++;

		string value;

		if (assigned[at] == '"')
		{
			quoted = true;

			var built = new StringBuilder(assigned.Length - at);

			for (var index = at + 1; index < assigned.Length - 1; index++)
			{
				if (assigned[index] == '\\')
					index++;

				built.Append(assigned[index]);
			}

			value = built.ToString();
		}
		else
			value = assigned.Substring(at);

		// RFC 8187 §3.2.2: an ext-value is never a quoted-string.
		var extended = lower.EndsWith("*", StringComparison.Ordinal) && !quoted ? Extended(value) : null;

		return new WebLink.Parameter(lower, value, extended);
	}

	/// <summary>RFC 8187 §3.2.1's ext-value, or null where the text is not one.</summary>
	internal static ExtendedValue? Extended(string text)
	{
		var first  = text.IndexOf('\'');
		var second = first < 0 ? -1 : text.IndexOf('\'', first + 1);

		if (first <= 0 || second < 0)
			return null;

		var charset  = text.Substring(0, first);
		var language = second > first + 1 ? text.Substring(first + 1, second - first - 1) : null;
		var chars    = text.Substring(second + 1);

		foreach (var character in charset)
			if (!(character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or
				'!' or '#' or '$' or '%' or '&' or '+' or '-' or '^' or '_' or '`' or '{' or '}' or '~'))
			{
				return null;
			}

		if (language is not null && !Rfc5646.TryParseTag(language, out _))
			return null;

		var bytes = new byte[chars.Length];
		var count = 0;

		for (var at = 0; at < chars.Length; at++)
		{
			var character = chars[at];

			if (character == '%')
			{
				if (at + 2 >= chars.Length || !Uri.IsHexDigit(chars[at + 1]) || !Uri.IsHexDigit(chars[at + 2]))
					return null;

				bytes[count++] = (byte)(Hex(chars[at + 1]) << 4 | Hex(chars[at + 2]));
				at += 2;
			}
			else if (character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or
				'!' or '#' or '$' or '&' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~')
			{
				bytes[count++] = (byte)character;
			}
			else
				return null;
		}

		string? value = null;

		if (string.Equals(charset, "UTF-8", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				value = Strict.GetString(bytes, 0, count);
			}
			catch (DecoderFallbackException)
			{
				return null;
			}
		}
		else if (string.Equals(charset, "ISO-8859-1", StringComparison.OrdinalIgnoreCase))
		{
			var latin = new char[count];

			for (var index = 0; index < count; index++)
				latin[index] = (char)bytes[index];

			value = new string(latin);
		}

		return new ExtendedValue(charset, language, value);
	}

	/// <summary>Whether each parameter whose value the RFC gives a syntax has that syntax.</summary>
	internal static bool AreValid(WebLink.Parameter[] parameters, bool strict)
	{
		var rel  = false;
		var type = false;

		foreach (var parameter in parameters)
		{
			var valid = parameter.Name switch
			{
				"rel"      => rel || IsRelationTypes(parameter.Value, strict),
				"rev"      => IsRelationTypes(parameter.Value, strict),
				"anchor"   => Rfc3986.TryParseReference(parameter.Value, out _),
				"hreflang" => Rfc5646.TryParseTag(parameter.Value, out _),
				"type"     => type || IsMediaType(parameter.Value),
				_          => true,
			};

			if (!valid)
				return false;

			rel  |= parameter.Name == "rel";
			type |= parameter.Name == "type";
		}

		return true;
	}

	// §3.3: relation-type *( 1*SP relation-type ), relation-type = reg-rel-type / ext-rel-type.
	static bool IsRelationTypes(string value, bool strict)
	{
		if (value.Length == 0 || value[0] == ' ' || value[value.Length - 1] == ' ')
			return false;

		foreach (var type in value.Split([' '], StringSplitOptions.RemoveEmptyEntries))
			if (!IsRegisteredRelationType(type, strict) && !Rfc3986.TryParseUri(type, out _))
				return false;

		return true;
	}

	// reg-rel-type = LOALPHA *( LOALPHA / DIGIT / "." / "-" ), and outside the strict reading a letter of
	// either case, since a registered type compares without case (§2.1.1).
	static bool IsRegisteredRelationType(string type, bool strict)
	{
		for (var at = 0; at < type.Length; at++)
		{
			var character = type[at];
			var letter    = character is >= 'a' and <= 'z' || !strict && character is >= 'A' and <= 'Z';

			if (!letter && (at == 0 || character is not (>= '0' and <= '9' or '.' or '-')))
				return false;
		}

		return true;
	}

	// §3.4.1: type-name "/" subtype-name, both RFC 6838 §4.2's restricted-name.
	static bool IsMediaType(string value)
	{
		var slash = value.IndexOf('/');

		return slash >= 0 && IsRestrictedName(value.Substring(0, slash)) && IsRestrictedName(value.Substring(slash + 1));
	}

	// restricted-name = restricted-name-first *126restricted-name-chars.
	static bool IsRestrictedName(string name)
	{
		if (name.Length is 0 or > 127 || name[0] is not (>= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9'))
			return false;

		foreach (var character in name)
			if (character is not (>= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '!' or '#' or '$' or '&' or '-' or '^' or '_' or '.' or '+'))
				return false;

		return true;
	}

	static int Hex(char digit)
	{
		return digit <= '9' ? digit - '0' : (digit | 0x20) - 'a' + 10;
	}

	static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
}
