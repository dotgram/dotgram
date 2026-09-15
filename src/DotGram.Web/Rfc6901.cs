using System;
using System.Collections.Generic;
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
public static partial class Rfc6901
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
