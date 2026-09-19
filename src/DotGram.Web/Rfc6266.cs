using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>A Content-Disposition header field value as RFC 6266 §4.1 writes one: a disposition type and its parameters.</summary>
/// <remarks>
/// <para>
/// The parts are kept as written. Equality asks what the RFC says matters: the type and a parameter name are
/// case-insensitive (§4.1, §4.3), a value is compared as written, and parameters are compared in order.
/// </para>
/// <para>
/// A filename is advisory. §4.3 asks the recipient to strip the path, to distrust the extension, and to
/// replace names like <c>..</c> and device names; <see cref="Filename"/> is what the sender wrote, and making
/// it safe for a file system is the caller's.
/// </para>
/// </remarks>
/// <param name="Type">The disposition type as written.</param>
/// <param name="Parameters">In the order written, names as written, values unquoted.</param>
public sealed record ContentDisposition(string Type, IReadOnlyList<ContentDisposition.Parameter> Parameters)
{
	/// <summary>A disposition parameter: its name as written, its value after unquoting, and its RFC 8187 value where it has one.</summary>
	/// <param name="Extended">
	/// For an ext-token, a name ending in <c>*</c>, the ext-value decoded as RFC 8187 says; null for any other name.
	/// </param>
	public sealed record Parameter(string Name, string Value, ExtendedValue? Extended);

	/// <summary>A Content-Disposition field value (RFC 6266 §4.1).</summary>
	/// <exception cref="FormatException">The text is no Content-Disposition field; the message says where.</exception>
	public static ContentDisposition Parse(string text) =>
		Rfc6266.ParseContentDisposition(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A Content-Disposition field value, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out ContentDisposition? field)
	{
		var read = Rfc6266.TryParseContentDisposition(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		field = read ? parsed : null;

		return read;
	}

	/// <summary>Whether the type is <c>inline</c>, whatever its case (§4.2).</summary>
	public bool IsInline => string.Equals(Type, "inline", StringComparison.OrdinalIgnoreCase);

	/// <summary>Whether the recipient treats the payload as an attachment: <c>attachment</c>, and any type it does not know (§4.2).</summary>
	public bool IsAttachment => !IsInline;

	/// <summary>The parameter of a name, whatever its case, or null. A valid field holds each name once.</summary>
	public Parameter? Find(string name)
	{
		if (name is null)
			throw new ArgumentNullException(nameof(name));

		foreach (var parameter in Parameters)
			if (string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))
				return parameter;

		return null;
	}

	/// <summary>
	/// The filename the sender suggests: <c>filename*</c> where it decodes, <c>filename</c> otherwise, and null where
	/// there is neither (§4.3).
	/// </summary>
	/// <remarks>
	/// A <c>filename*</c> in a character encoding this does not decode — anything but UTF-8 and ISO-8859-1 —
	/// gives way to <c>filename</c>. RFC 2231's continuations, <c>filename*0</c> and on, are not RFC 6266's and
	/// are extension parameters like any other.
	/// </remarks>
	public string? Filename => Find("filename*")?.Extended?.Value ?? Find("filename")?.Value;

	public bool Equals(ContentDisposition? other)
	{
		if (other is null ||
			!string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase) ||
			Parameters.Count != other.Parameters.Count)
		{
			return false;
		}

		for (var index = 0; index < Parameters.Count; index++)
			if (!string.Equals(Parameters[index].Name, other.Parameters[index].Name, StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(Parameters[index].Value, other.Parameters[index].Value, StringComparison.Ordinal))
			{
				return false;
			}

		return true;
	}

	public override int GetHashCode()
	{
		var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Type);

		foreach (var parameter in Parameters)
			hash = Structural.Combine(
				Structural.Combine(hash, StringComparer.OrdinalIgnoreCase.GetHashCode(parameter.Name)),
				StringComparer.Ordinal.GetHashCode(parameter.Value));

		return hash;
	}

	/// <summary>The field value: parameters after semicolons, a value quoted where it is no token, an ext-value as written.</summary>
	public override string ToString()
	{
		var output = new StringBuilder(Type);

		foreach (var parameter in Parameters)
		{
			output.Append("; ").Append(parameter.Name).Append('=');

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
}

// RFC 6266, Use of the Content-Disposition Header Field in HTTP, §4.1. The grammar is its ABNF with the
// implied whitespace §4.1 points out: optional space and tab between a word and a separator — around `;`
// and `=` — and, as a field value's own, before and after the whole (RFC 9112 §5). Beside the ABNF:
//
//   * A parameter whose name ends in `*` after at least one character is an ext-token, and its value is
//     RFC 8187's ext-value, which Rfc8288 reads: never a quoted-string, a charset, a language Rfc5646 reads,
//     and pct-encoded octets. A value that is not one makes the field invalid.
//   * A name written twice, whatever its case, makes the field invalid (§4.1).
//
// §3 lets a recipient recover a usable value from an invalid field, and otherwise ignore it; this recovers
// nothing, and a field that the ABNF does not make is refused whole. Both checks are a `when` in a rule
// that holds no group.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	Ows   = [' ' | '\t']*
	Tchar = ['!' | '#' | '$' | '%' | '&' | '\'' | '*' | '+' | '-' | '.' | '^' | '_' | '`' | '|' | '~' | '0'..'9' | 'a'..'z' | 'A'..'Z']
	Token = Tchar+

	// qdtext and quoted-pair; obs-text is any octet from 0x80.
	QuotedString = '"' & ([' ' | '\t' | '!' | '#'..'[' | ']'..'~' | '\u0080'..'\u00FF'] | '\\' & [' ' | '\t' | '!'..'~' | '\u0080'..'\u00FF'])* & '"'

	// content-disposition = disposition-type *( ";" disposition-parm ).
	DispositionField : @ContentDisposition
		= Ows & type: Token & parameters: DispositionParms & Ows & when @(Rfc6266.AreDistinct(parameters!))
		=> @(new ContentDisposition(type, parameters))

	DispositionParms : @ContentDisposition.Parameter[] = items: DispositionParm* => @(items)

	// filename-parm and disp-ext-parm alike: a name, `=`, and a value, taken apart beside the rule.
	DispositionParm : @ContentDisposition.Parameter
		= Ows & ';' & Ows & name: Token & Ows & '=' & Ows & value: ParmValue & when @(Rfc6266.IsParameter(name!, value!))
		=> @(Rfc6266.Parameter(name, value))

	ParmValue = Token | QuotedString

	parse DispositionField as ParseContentDisposition
	""")]
static partial class Rfc6266
{
	// ParseContentDisposition and TryParseContentDisposition are generated here.

	/// <summary>Whether no two parameters have the same name, whatever its case.</summary>
	internal static bool AreDistinct(ContentDisposition.Parameter[] parameters)
	{
		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var parameter in parameters)
			if (!seen.Add(parameter.Name))
				return false;

		return true;
	}

	/// <summary>Whether a name and a value make a parameter: an ext-token's value has to be an ext-value.</summary>
	internal static bool IsParameter(string name, string value) =>
		!IsExtToken(name) || value[0] != '"' && Rfc8288.Extended(value) is not null;

	/// <summary>A parameter from its name and its value as written.</summary>
	internal static ContentDisposition.Parameter Parameter(string name, string value)
	{
		if (IsExtToken(name))
			return new ContentDisposition.Parameter(name, value, Rfc8288.Extended(value));

		if (value[0] != '"')
			return new ContentDisposition.Parameter(name, value, null);

		var built = new StringBuilder(value.Length);

		for (var at = 1; at < value.Length - 1; at++)
		{
			if (value[at] == '\\')
				at++;

			built.Append(value[at]);
		}

		return new ContentDisposition.Parameter(name, built.ToString(), null);
	}

	// ext-token = <the characters in token, followed by "*">.
	static bool IsExtToken(string name) => name.Length > 1 && name[name.Length - 1] == '*';
}
