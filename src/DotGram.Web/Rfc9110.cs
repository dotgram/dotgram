using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>A media type as RFC 9110 §8.3.1 writes one: a type, a subtype, and parameters.</summary>
/// <remarks>
/// <para>
/// The parts are kept as written. Equality asks what the RFC says matters: a type, a subtype and a
/// parameter name are case-insensitive, and so is the value of <c>charset</c> (RFC 2046 §4.1.2), so
/// <c>text/html;charset=utf-8</c> and <c>Text/HTML;Charset="UTF-8"</c> are equal. Any other value is
/// compared as written, since whether its case matters is its parameter's to say. Parameters are
/// compared in order.
/// </para>
/// </remarks>
/// <param name="Parameters">In the order written, values unquoted, empty parameters left out.</param>
public sealed record MediaType(string Type, string Subtype, IReadOnlyList<MediaType.Parameter> Parameters)
{
	/// <summary>A parameter: its name as written and its value after unquoting.</summary>
	public sealed record Parameter(string Name, string Value);

	/// <summary>A Content-Type field value (RFC 9110 §8.3): a media type, with whitespace around it allowed.</summary>
	/// <exception cref="FormatException">The text is no media type; the message says where.</exception>
	public static MediaType Parse(string text) =>
		Rfc9110.ParseContentType(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A Content-Type field value, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out MediaType? type)
	{
		var match = Rfc9110.TryParseContentType(text ?? throw new ArgumentNullException(nameof(text)));

		type = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>The value of the first parameter of this name, whatever its case, or null.</summary>
	public string? ParameterValue(string name)
	{
		foreach (var parameter in Parameters)
			if (string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))
				return parameter.Value;

		return null;
	}

	/// <summary>The <c>charset</c> parameter, or null (§8.3.2).</summary>
	public string? Charset => ParameterValue("charset");

	/// <summary>The structured syntax suffix, what follows the subtype's last <c>+</c> (RFC 6838 §4.2.8), or null.</summary>
	public string? Suffix =>
		Subtype.LastIndexOf('+') is var plus && plus >= 0 && plus < Subtype.Length - 1 ? Subtype.Substring(plus + 1) : null;

	public bool Equals(MediaType? other)
	{
		if (other is null ||
			!string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase) ||
			!string.Equals(Subtype, other.Subtype, StringComparison.OrdinalIgnoreCase) ||
			Parameters.Count != other.Parameters.Count)
		{
			return false;
		}

		for (var index = 0; index < Parameters.Count; index++)
		{
			var mine   = Parameters[index];
			var theirs = other.Parameters[index];

			if (!string.Equals(mine.Name, theirs.Name, StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(mine.Value, theirs.Value, ValueComparison(mine.Name)))
			{
				return false;
			}
		}

		return true;
	}

	public override int GetHashCode()
	{
		var hash = Structural.Combine(
			StringComparer.OrdinalIgnoreCase.GetHashCode(Type), StringComparer.OrdinalIgnoreCase.GetHashCode(Subtype));

		foreach (var parameter in Parameters)
			hash = Structural.Combine(
				Structural.Combine(hash, StringComparer.OrdinalIgnoreCase.GetHashCode(parameter.Name)),
				ValueComparison(parameter.Name) == StringComparison.OrdinalIgnoreCase
					? StringComparer.OrdinalIgnoreCase.GetHashCode(parameter.Value)
					: StringComparer.Ordinal.GetHashCode(parameter.Value));

		return hash;
	}

	/// <summary>The media type as a field value: parameters after semicolons, a value quoted where it is no token.</summary>
	public override string ToString()
	{
		var output = new StringBuilder(Type).Append('/').Append(Subtype);

		foreach (var parameter in Parameters)
		{
			output.Append(';').Append(parameter.Name).Append('=');

			if (Rfc9110.IsToken(parameter.Value))
				output.Append(parameter.Value);
			else
			{
				output.Append('"');

				foreach (var character in parameter.Value)
				{
					if (character is '"' or '\\')
						output.Append('\\');

					output.Append(character);
				}

				output.Append('"');
			}
		}

		return output.ToString();
	}

	static StringComparison ValueComparison(string name) =>
		string.Equals(name, "charset", StringComparison.OrdinalIgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}

/// <summary>A media range of an <c>Accept</c> field (§12.5.1) and the weight it was given (§12.4.2).</summary>
/// <param name="Media">
/// The range: <c>*/*</c>, <c>type/*</c>, or a media type, with its parameters. The weight is not among them.
/// </param>
/// <param name="Weight">From 0, not acceptable, to 1, the default.</param>
public sealed record MediaRange(MediaType Media, decimal Weight)
{
	/// <summary>An Accept field value (RFC 9110 §12.5.1): media ranges and their weights, empty elements left out.</summary>
	/// <exception cref="FormatException">The text is no Accept field; the message says where.</exception>
	public static MediaRange[] ParseAccept(string text) =>
		Rfc9110.ParseAccept(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>An Accept field value, or false where the text is not one.</summary>
	public static bool TryParseAccept(string text, [NotNullWhen(true)] out MediaRange[]? ranges)
	{
		var match = Rfc9110.TryParseAccept(text ?? throw new ArgumentNullException(nameof(text)));

		ranges = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>
	/// The quality value an Accept field gives a media type: the weight of the most specific range that matches
	/// it, or 0 where none does (§12.5.1, §12.4.3).
	/// </summary>
	/// <remarks>
	/// More specific is a media type with more parameters, then a media type, then <c>type/*</c>, then
	/// <c>*/*</c>. Where two ranges are as specific, the first written counts. An absent field is not an empty
	/// one — without it any media type is acceptable — and that is the caller's to tell apart.
	/// </remarks>
	public static decimal Quality(IReadOnlyList<MediaRange> accept, MediaType type) => Rfc9110.Quality(accept, type);

	/// <summary>Whether this range covers a media type: its type and subtype, and every parameter it names.</summary>
	public bool Matches(MediaType type)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		if (Media.Type != "*" && !string.Equals(Media.Type, type.Type, StringComparison.OrdinalIgnoreCase))
			return false;

		if (Media.Subtype != "*" && !string.Equals(Media.Subtype, type.Subtype, StringComparison.OrdinalIgnoreCase))
			return false;

		foreach (var parameter in Media.Parameters)
			if (type.ParameterValue(parameter.Name) is not { } value ||
				!string.Equals(value, parameter.Value,
					string.Equals(parameter.Name, "charset", StringComparison.OrdinalIgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return false;
			}

		return true;
	}

	/// <summary>How specific the range is: a media type with more parameters, a media type, <c>type/*</c>, <c>*/*</c>.</summary>
	internal (int Level, int Parameters) Precedence =>
		(Media.Type == "*" ? 0 : Media.Subtype == "*" ? 1 : 2, Media.Parameters.Count);
}

// RFC 9110, HTTP Semantics: the Content-Type field (§8.3), the media type it holds (§8.3.1), and the Accept
// field (§12.5.1) with its quality values (§12.4.2). The grammar is those sections' ABNF over §5.6's
// common rules — token, quoted-string, parameters whose empty slots a recipient accepts, and a list whose
// empty elements it accepts too. Two things beside the ABNF:
//
//   * A range whose type is `*` has `*` for a subtype: `*/html` is no range. And a parameter named `q`,
//     wherever it stands, is the weight (§12.5.1 asks recipients to take it so), which has to be a qvalue
//     — 0 to 1, three decimals at most. Both are one `when`, kept in a rule that holds no group.
//   * A field value's leading and trailing whitespace is not part of it (RFC 9112 §5), so it is allowed.
//
// A type and a subtype are tokens, as HTTP reads them; RFC 6838 §4.2's narrower names for registration are
// a registration's business, and every name the IANA registry holds is one.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	// ── §5.6 ─────────────────────────────────────────────────────────────────────

	Ows   = [' ' | '\t']*
	Tchar = ['!' | '#' | '$' | '%' | '&' | '\'' | '*' | '+' | '-' | '.' | '^' | '_' | '`' | '|' | '~' | '0'..'9' | 'a'..'z' | 'A'..'Z']
	Token = Tchar+

	QuotedString = '"' & ([' ' | '\t' | '!' | '#'..'[' | ']'..'~' | '\u0080'..'\u00FF'] | '\\' & [' ' | '\t' | '!'..'~' | '\u0080'..'\u00FF'])* & '"'

	// parameters = *( OWS ";" OWS [ parameter ] ), with no whitespace around the `=`. Each slot is its text,
	// taken apart beside the rule.
	Parameters : @MediaType.Parameter[] = slots: ParameterSlot* => @(Rfc9110.Present(slots))

	ParameterSlot : @string = Ows & ';' & Ows & text: ParameterText => @(text)

	ParameterText = (Token & '=' & (Token | QuotedString))?

	// ── Content-Type ─────────────────────────────────────────────────────────────

	ContentTypeField : @MediaType = Ows & media: Media & Ows => @(media)

	Media : @MediaType = type: Token & '/' & subtype: Token & parameters: Parameters => @(new MediaType(type, subtype, parameters))

	// ── Accept ───────────────────────────────────────────────────────────────────

	AcceptField : @MediaRange[] = Ows & (',' & Ows)* & ranges: AcceptElement* => @(ranges)

	AcceptElement : @MediaRange = range: Range & Ows & ((',' & Ows)+ | ?!any) => @(range)

	Range : @MediaRange
		= type: Token & '/' & subtype: Token & parameters: Parameters & when @(Rfc9110.IsRange(type!, subtype!, parameters!))
		=> @(Rfc9110.Weighted(type, subtype, parameters))

	parse ContentTypeField as ParseContentType
	parse AcceptField      as ParseAccept
	""")]
static partial class Rfc9110
{
	// ParseContentType, ParseAccept and their Try forms are generated here.

	/// <summary>
	/// The quality value an <c>Accept</c> field gives a media type: the weight of the most specific range that
	/// matches it, or 0 where none does (§12.5.1, §12.4.3).
	/// </summary>
	/// <remarks>
	/// More specific is a media type with more parameters, then a media type, then <c>type/*</c>, then
	/// <c>*/*</c>. Where two ranges are as specific, the first written counts. An absent field is not an empty
	/// one — without it any media type is acceptable — and that is the caller's to tell apart.
	/// </remarks>
	public static decimal Quality(IReadOnlyList<MediaRange> accept, MediaType type)
	{
		if (accept is null)
			throw new ArgumentNullException(nameof(accept));

		MediaRange? best = null;

		foreach (var range in accept)
			if (range.Matches(type) && (best is null || range.Precedence.CompareTo(best.Precedence) > 0))
				best = range;

		return best?.Weight ?? 0m;
	}

	internal static bool IsToken(string text)
	{
		if (text.Length == 0)
			return false;

		foreach (var c in text)
			if (!(c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or
				'!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~'))
			{
				return false;
			}

		return true;
	}

	/// <summary>The parameters a run of slots holds: the empty ones left out, each value unquoted.</summary>
	internal static MediaType.Parameter[] Present(string[] slots)
	{
		var parameters = new List<MediaType.Parameter>(slots.Length);

		foreach (var slot in slots)
		{
			if (slot.Length == 0)
				continue;

			var equals = slot.IndexOf('=');
			var name   = slot.Substring(0, equals);
			var value  = slot.Substring(equals + 1);

			if (value.Length > 0 && value[0] == '"')
			{
				var built = new StringBuilder(value.Length);

				for (var at = 1; at < value.Length - 1; at++)
				{
					if (value[at] == '\\')
						at++;

					built.Append(value[at]);
				}

				value = built.ToString();
			}

			parameters.Add(new MediaType.Parameter(name, value));
		}

		return [.. parameters];
	}

	/// <summary>Whether a type, subtype and parameters make a range: <c>*</c> only as <c>*/*</c>, and every <c>q</c> a qvalue.</summary>
	internal static bool IsRange(string type, string subtype, MediaType.Parameter[] parameters)
	{
		if (type == "*" && subtype != "*")
			return false;

		foreach (var parameter in parameters)
			if (string.Equals(parameter.Name, "q", StringComparison.OrdinalIgnoreCase) && QualityValue(parameter.Value) is null)
				return false;

		return true;
	}

	/// <summary>A range and its weight: the first <c>q</c>, taken out of the parameters, or 1.</summary>
	internal static MediaRange Weighted(string type, string subtype, MediaType.Parameter[] parameters)
	{
		var weight = 1m;
		var found  = false;
		var kept   = new List<MediaType.Parameter>(parameters.Length);

		foreach (var parameter in parameters)
		{
			if (string.Equals(parameter.Name, "q", StringComparison.OrdinalIgnoreCase))
			{
				if (!found)
					weight = QualityValue(parameter.Value)!.Value;

				found = true;
				continue;
			}

			kept.Add(parameter);
		}

		return new MediaRange(new MediaType(type, subtype, [.. kept]), weight);
	}

	/// <summary>§12.4.2's qvalue: <c>0</c> with up to three decimals, or <c>1</c> with up to three zeros; null otherwise.</summary>
	internal static decimal? QualityValue(string text)
	{
		if (text.Length is 0 or > 5 || text[0] is not ('0' or '1') || text.Length > 1 && text[1] != '.')
			return null;

		for (var at = 2; at < text.Length; at++)
			if (text[at] is < '0' or > '9' || text[0] == '1' && text[at] != '0')
				return null;

		return decimal.Parse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
	}
}
