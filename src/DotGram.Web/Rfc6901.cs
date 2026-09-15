using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>A JSON Pointer as RFC 6901 divides it: the reference tokens, each unescaped.</summary>
/// <remarks>
/// <para>
/// What a pointer refers to is a question about a document, and the document is not here: a token
/// names an object member or an array element, and which one it is depends on what the pointer has
/// reached. <see cref="Tokens"/> is everything the pointer says, and <see cref="ArrayIndex"/> is the
/// one rule §4 adds for the case where the value reached is an array.
/// </para>
/// </remarks>
/// <param name="Tokens">The reference tokens in order, with <c>~1</c> and <c>~0</c> turned back into <c>/</c> and <c>~</c>.</param>
public sealed record JsonPointer(IReadOnlyList<string> Tokens)
{
	/// <summary>The pointer with no tokens, which refers to the whole document.</summary>
	public static JsonPointer Root { get; } = new([]);

	/// <summary>A JSON Pointer in its string form (RFC 6901 §3): nothing, or tokens each after a <c>/</c>.</summary>
	/// <exception cref="FormatException">The text is no pointer; the message says where.</exception>
	public static JsonPointer Parse(string text) =>
		Rfc6901.ParsePointer(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A JSON Pointer in its string form, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out JsonPointer? pointer)
	{
		var match = Rfc6901.TryParsePointer(text ?? throw new ArgumentNullException(nameof(text)));

		pointer = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>A JSON Pointer in a URI fragment (§6): <c>#</c>, and the string form pct-encoded as UTF-8.</summary>
	/// <exception cref="FormatException">The text is no fragment holding a pointer; the message says where.</exception>
	public static JsonPointer ParseFragment(string text) =>
		Rfc6901.ParseFragment(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A JSON Pointer in a URI fragment, or false where the text is not one.</summary>
	public static bool TryParseFragment(string text, [NotNullWhen(true)] out JsonPointer? pointer)
	{
		var match = Rfc6901.TryParseFragment(text ?? throw new ArgumentNullException(nameof(text)));

		pointer = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>Equal to another pointer with the same tokens in the same order.</summary>
	public bool Equals(JsonPointer? other) => other is not null && Structural.Same(Tokens, other.Tokens);

	public override int GetHashCode() => Structural.Hash(Tokens);

	/// <summary>The pointer as a JSON string holds it (§5): each token after a <c>/</c>, escaped.</summary>
	public override string ToString()
	{
		var output = new StringBuilder();

		foreach (var token in Tokens)
		{
			output.Append('/');

			foreach (var character in token)
			{
				if (character == '~')
					output.Append("~0");
				else if (character == '/')
					output.Append("~1");
				else
					output.Append(character);
			}
		}

		return output.ToString();
	}

	/// <summary>The pointer as a URI fragment holds it (§6): <c>#</c>, and pct-encoded UTF-8 where RFC 3986 needs it.</summary>
	public string ToUriFragment()
	{
		var output = new StringBuilder("#");

		foreach (var octet in Encoding.UTF8.GetBytes(ToString()))
		{
			if (octet < 0x80 && Allowed((char)octet))
				output.Append((char)octet);
			else
				output.Append('%').Append(Hex[octet >> 4]).Append(Hex[octet & 0xF]);
		}

		return output.ToString();
	}

	/// <summary>The element a token names in an array (§4), or null where it names none.</summary>
	/// <remarks>
	/// <c>0</c>, or digits without a leading zero. <c>-</c> names the element after the last, which never
	/// exists and so is never an index: what it means is the application's, and <see cref="IsPastTheEnd"/>
	/// says whether a token is it. A number too large for an <c>int</c> is no element of any array a
	/// .NET collection can hold.
	/// </remarks>
	public static int? ArrayIndex(string token)
	{
		if (token is null)
			throw new ArgumentNullException(nameof(token));

		if (token.Length == 0 || token.Length > 1 && token[0] == '0')
			return null;

		var index = 0L;

		foreach (var character in token)
		{
			if (character is < '0' or > '9')
				return null;

			index = index * 10 + (character - '0');

			if (index > int.MaxValue)
				return null;
		}

		return (int)index;
	}

	/// <summary>Whether a token is <c>-</c>, the element after an array's last (§4).</summary>
	public static bool IsPastTheEnd(string token) => token == "-";

	/// <summary>The value this pointer refers to in a document (§4), or null where it refers to none.</summary>
	/// <remarks>
	/// <para>
	/// Each token takes one step: to the member of an object with that name, or to the element of an array
	/// at that index. There is no value — and so null, which is not <see cref="JsonValue.Null"/> — where an
	/// object has no member of the name or has more than one (§4 calls that member undefined), where an
	/// array's token is no index or is past its end, <c>-</c> included, and where a step is taken into a value
	/// that has no members or elements.
	/// </para>
	/// <para>
	/// What an application does with <c>-</c> or with a missing value is the application's (§7): a
	/// JSON Patch adds there, for one. This answers what the document holds.
	/// </para>
	/// </remarks>
	public JsonValue? Resolve(JsonValue document)
	{
		if (document is null)
			throw new ArgumentNullException(nameof(document));

		var value = document;

		foreach (var token in Tokens)
		{
			switch (value)
			{
				case JsonValue.Object members:
				{
					JsonValue? found = null;

					foreach (var member in members.Members)
					{
						if (!string.Equals(member.Key, token, StringComparison.Ordinal))
							continue;

						if (found is not null)
							return null;

						found = member.Value;
					}

					if (found is null)
						return null;

					value = found;
					break;
				}

				case JsonValue.Array items:
					if (ArrayIndex(token) is not { } index || index >= items.Items.Count)
						return null;

					value = items.Items[index];
					break;

				default:
					return null;
			}
		}

		return value;
	}

	// RFC 3986 §3.5: a fragment is pchar, `/` and `?`; a `%` is always a triplet's.
	static bool Allowed(char c) =>
		c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or
			'-' or '.' or '_' or '~' or
			'!' or '$' or '&' or '\'' or '(' or ')' or '*' or '+' or ',' or ';' or '=' or
			':' or '@' or '/' or '?';

	const string Hex = "0123456789ABCDEF";
}

// RFC 6901, JavaScript Object Notation (JSON) Pointer. The grammar is §3's ABNF for the string form,
// and §6's form in a URI fragment is RFC 3986's fragment rule, pct-decoded as UTF-8 and read again as
// a pointer. Escapes are undone in the order §4 gives — `~1` first, then `~0` — so `~01` is `~1` and
// never `/`.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	// §3.
	Pointer : @JsonPointer = tokens: ReferenceToken* => @(new JsonPointer(tokens))

	ReferenceToken : @string = '/' & text: TokenText => @(Rfc6901.Unescaped(text))

	// Anything but `/` and `~`, and a `~` only as `~0` or `~1`.
	TokenText = ([^ '/' | '~'] | '~' & ['0' | '1'])*

	// §6: RFC 3986's fragment, whose triplets are UTF-8 and whose text is a pointer.
	Fragment : @JsonPointer
		= '#' & text: FragmentText & when @(Rfc6901.FromFragment(text!) is not null)
		=> @(Rfc6901.FromFragment(text)!)

	FragmentText = (Unreserved | SubDelims | ':' | '@' | '/' | '?' | '%' & Hexdig & Hexdig)*

	Unreserved = ['a'..'z' | 'A'..'Z' | '0'..'9' | '-' | '.' | '_' | '~']
	SubDelims  = ['!' | '$' | '&' | '\'' | '(' | ')' | '*' | '+' | ',' | ';' | '=']
	Hexdig     = ['0'..'9' | 'a'..'f' | 'A'..'F']

	parse Pointer  as ParsePointer
	parse Fragment as ParseFragment
	""")]
static partial class Rfc6901
{
	// ParsePointer, TryParsePointer, ParseFragment and TryParseFragment are generated here.

	internal static string Unescaped(string text) =>
		text.IndexOf('~') < 0 ? text : text.Replace("~1", "/").Replace("~0", "~");

	/// <summary>The pointer a fragment's text holds, or null where its bytes are not UTF-8 or its text no pointer.</summary>
	internal static JsonPointer? FromFragment(string text)
	{
		var bytes = new byte[text.Length];
		var count = 0;

		for (var at = 0; at < text.Length; at++)
		{
			if (text[at] == '%')
			{
				bytes[count++] = (byte)(Hex(text[at + 1]) << 4 | Hex(text[at + 2]));
				at += 2;
			}
			else
				bytes[count++] = (byte)text[at];
		}

		string decoded;

		try
		{
			decoded = Strict.GetString(bytes, 0, count);
		}
		catch (DecoderFallbackException)
		{
			return null;
		}

		var pointer = TryParsePointer(decoded);

		return pointer.IsSuccess ? pointer.Value : null;
	}

	static int Hex(char digit) =>
		digit <= '9' ? digit - '0' : (digit | 0x20) - 'a' + 10;

	static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
}
