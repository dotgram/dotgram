using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>One element of a Forwarded header field (RFC 7239 §4): what one proxy disclosed, as parameter-identifier pairs.</summary>
/// <remarks>
/// <para>
/// The pairs are kept as written, empty slots left out. Equality asks what the RFC says matters: a parameter
/// name is case-insensitive (§4), a value is compared as written, and pairs are compared in order.
/// </para>
/// <para>
/// Nothing here can be trusted (§8.1): any node on the way may have written or changed it.
/// </para>
/// </remarks>
/// <param name="Pairs">In the order written, values unquoted.</param>
public sealed record ForwardedElement(IReadOnlyList<ForwardedElement.Pair> Pairs)
{
	/// <summary>A parameter-identifier pair: the name as written and the value after unquoting.</summary>
	public sealed record Pair(string Name, string Value);

	/// <summary>A Forwarded field value (RFC 7239 §4): an element per proxy, empty ones left out.</summary>
	/// <exception cref="FormatException">The text is no Forwarded field; the message says where.</exception>
	public static ForwardedElement[] ParseField(string text) =>
		Rfc7239.ParseForwarded(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A Forwarded field value, or false where the text is not one.</summary>
	public static bool TryParseField(string text, [NotNullWhen(true)] out ForwardedElement[]? elements)
	{
		var read = Rfc7239.TryParseForwarded(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		elements = read ? parsed : null;

		return read;
	}

	/// <summary>The value of the pair of a name, whatever its case, or null. An element holds each name once.</summary>
	public string? Find(string name)
	{
		if (name is null)
			throw new ArgumentNullException(nameof(name));

		foreach (var pair in Pairs)
			if (string.Equals(pair.Name, name, StringComparison.OrdinalIgnoreCase))
				return pair.Value;

		return null;
	}

	/// <summary>§5.1: the user-agent facing interface of the proxy, or null.</summary>
	public ForwardedNode? By => Find("by") is { } value ? Rfc7239.ParseNode(value) : null;

	/// <summary>§5.2: the node that made the request to the proxy, or null.</summary>
	public ForwardedNode? For => Find("for") is { } value ? Rfc7239.ParseNode(value) : null;

	/// <summary>§5.3: the Host header field the proxy received, or null.</summary>
	public string? Host => Find("host");

	/// <summary>§5.4: the URI scheme of the protocol the request was made with, or null.</summary>
	public string? Proto => Find("proto");

	public bool Equals(ForwardedElement? other)
	{
		if (other is null || Pairs.Count != other.Pairs.Count)
			return false;

		for (var index = 0; index < Pairs.Count; index++)
			if (!string.Equals(Pairs[index].Name, other.Pairs[index].Name, StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(Pairs[index].Value, other.Pairs[index].Value, StringComparison.Ordinal))
			{
				return false;
			}

		return true;
	}

	public override int GetHashCode()
	{
		var hash = Pairs.Count;

		foreach (var pair in Pairs)
			hash = Structural.Combine(
				Structural.Combine(hash, StringComparer.OrdinalIgnoreCase.GetHashCode(pair.Name)),
				StringComparer.Ordinal.GetHashCode(pair.Value));

		return hash;
	}

	/// <summary>The element as a field writes it: pairs after semicolons, a value quoted where it is no token.</summary>
	public override string ToString()
	{
		var output = new StringBuilder();

		foreach (var pair in Pairs)
		{
			if (output.Length > 0)
				output.Append(';');

			output.Append(pair.Name).Append('=');

			if (Rfc9110.IsToken(pair.Value))
				output.Append(pair.Value);
			else
			{
				output.Append('"');

				foreach (var character in pair.Value)
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

/// <summary>A node identifier (RFC 7239 §6): an address, <c>unknown</c>, or an obfuscated identifier, and a port.</summary>
/// <param name="Kind">Which of §6's four a name is.</param>
/// <param name="Name">The address without brackets, <c>unknown</c>, or the obfuscated identifier with its underscore.</param>
/// <param name="Port">The digits or the obfuscated port after the <c>:</c>, or null.</param>
public sealed record ForwardedNode(ForwardedNode.Kinds Kind, string Name, string? Port)
{
	/// <summary>A node identifier (§6), after its value's quoted-string unescaping.</summary>
	/// <exception cref="FormatException">The text is no node identifier; the message says where.</exception>
	public static ForwardedNode Parse(string text) =>
		Rfc7239.ParseNode(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A node identifier, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out ForwardedNode? node)
	{
		var read = Rfc7239.TryParseNode(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		node = read ? parsed : null;

		return read;
	}

	/// <summary>The node identifiers of §6.</summary>
	public enum Kinds
	{
		/// <summary>An IPv4 address (§6.1).</summary>
		IPv4,

		/// <summary>An IPv6 address, written in brackets (§6.1).</summary>
		IPv6,

		/// <summary><c>unknown</c>: the proxy does not know the node (§6.2).</summary>
		Unknown,

		/// <summary>A generated identifier, beginning with an underscore (§6.3).</summary>
		Obfuscated,
	}

	/// <summary>The port as a number, or null where there is none or it is obfuscated. Up to five digits, as §6 allows.</summary>
	public int? PortNumber =>
		Port is { Length: > 0 } port && port[0] != '_' ? int.Parse(port, NumberStyles.None, CultureInfo.InvariantCulture) : null;

	/// <summary>The node as a value writes it.</summary>
	public override string ToString() =>
		(Kind == Kinds.IPv6 ? "[" + Name + "]" : Name) + (Port is null ? "" : ":" + Port);
}

// RFC 7239, Forwarded HTTP Extension. The field is §4's ABNF — a list whose empty elements a recipient
// accepts (RFC 9110 §5.6.1.2), each element semicolon-separated pairs with slots that may be empty and no
// whitespace around `;` or `=` — and a node is §6's. What §5 says a value is, after unquoting, is asked of
// each pair in a `when`, and a pair that is not what its parameter says makes the field invalid:
//
//   * `by` and `for` are nodes, which this grammar reads too: the address rules are RFC 3986's, written
//     again here since a grammar includes no other.
//   * `host` is RFC 7230's Host, a URI host and a port, which Rfc3986 reads as an authority with nothing else.
//   * `proto` is a URI scheme (RFC 3986 §3.1).
//   * A name written twice in one element, whatever its case, is invalid (§4).
//
// An element of empty slots is left out, and a field left with no element is refused: `1#` asks for one.
// Erratum 5275, reported and not verified, spells the list rule out and changes nothing read.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	// ── RFC 9110 §5.6 ────────────────────────────────────────────────────────────

	Ows   = [' ' | '\t']*
	Tchar = ['!' | '#' | '$' | '%' | '&' | '\'' | '*' | '+' | '-' | '.' | '^' | '_' | '`' | '|' | '~' | '0'..'9' | 'a'..'z' | 'A'..'Z']
	Token = Tchar+

	// qdtext and quoted-pair; obs-text is any octet from 0x80.
	QuotedString = '"' & ([' ' | '\t' | '!' | '#'..'[' | ']'..'~' | '\u0080'..'\u00FF'] | '\\' & [' ' | '\t' | '!'..'~' | '\u0080'..'\u00FF'])* & '"'

	// ── §4 ───────────────────────────────────────────────────────────────────────

	// Forwarded = 1#forwarded-element.
	ForwardedField : @ForwardedElement[] = Ows & elements: ElementList & Ows & when @(elements!.Length > 0) => @(elements)

	ElementList : @ForwardedElement[] = first: Element & rest: NextElement* => @(Rfc7239.Present(first, rest))

	NextElement : @ForwardedElement = Ows & ',' & Ows & element: Element => @(element)

	// forwarded-element = [ forwarded-pair ] *( ";" [ forwarded-pair ] ). Each slot is its text, taken
	// apart beside the rule, and what the pairs mean is one check.
	Element : @ForwardedElement = pairs: PairList & when @(Rfc7239.IsElement(pairs!)) => @(new ForwardedElement(pairs))

	PairList : @ForwardedElement.Pair[] = first: PairSlot & rest: NextPairSlot* => @(Rfc7239.Pairs(first, rest))

	NextPairSlot : @string = ';' & text: PairSlot => @(text)

	PairSlot = (Token & '=' & (Token | QuotedString))?

	// ── §6 ───────────────────────────────────────────────────────────────────────

	Node : @ForwardedNode
		= address: IPv4Address & port: NodePort             => @(new ForwardedNode(ForwardedNode.Kinds.IPv4, address, Rfc7239.Port(port)))
		| '[' & address: IPv6Address & ']' & port: NodePort => @(new ForwardedNode(ForwardedNode.Kinds.IPv6, address, Rfc7239.Port(port)))
		| "unknown"i & port: NodePort                       => @(new ForwardedNode(ForwardedNode.Kinds.Unknown, "unknown", Rfc7239.Port(port)))
		| address: ObfuscatedName & port: NodePort          => @(new ForwardedNode(ForwardedNode.Kinds.Obfuscated, address, Rfc7239.Port(port)))

	// node-port = port / obfport, after its colon.
	NodePort = (':' & (Digit{1,5} | ObfuscatedName))?

	// obfnode and obfport alike.
	ObfuscatedName = '_' & ['a'..'z' | 'A'..'Z' | '0'..'9' | '.' | '_' | '-']+

	// ── RFC 3986 §3.2.2, as Rfc3986 writes it ────────────────────────────────────

	Digit  = ['0'..'9']
	Hexdig = ['0'..'9' | 'a'..'f' | 'A'..'F']

	DecOctet = "25" & ['0'..'5']
	         |  '2' & ['0'..'4'] & Digit
	         |  '1' & Digit & Digit
	         | ['1'..'9'] & Digit
	         | Digit

	IPv4Address = DecOctet & '.' & DecOctet & '.' & DecOctet & '.' & DecOctet

	H16  = Hexdig{1,4}
	Ls32 = H16 & ':' & H16 | IPv4Address

	IPv6Address =                                    (H16 & ':'){6} & Ls32
	            |                             "::" & (H16 & ':'){5} & Ls32
	            | H16?                      & "::" & (H16 & ':'){4} & Ls32
	            | ((H16 & ':'){0,1} & H16)? & "::" & (H16 & ':'){3} & Ls32
	            | ((H16 & ':'){0,2} & H16)? & "::" & (H16 & ':'){2} & Ls32
	            | ((H16 & ':'){0,3} & H16)? & "::" & (H16 & ':')    & Ls32
	            | ((H16 & ':'){0,4} & H16)? & "::" & Ls32
	            | ((H16 & ':'){0,5} & H16)? & "::" & H16
	            | ((H16 & ':'){0,6} & H16)? & "::"

	parse ForwardedField as ParseForwarded
	parse Node           as ParseNode
	""")]
static partial class Rfc7239
{
	// ParseForwarded, ParseNode and their Try forms are generated here.

	/// <summary>The elements that hold a pair, in order.</summary>
	internal static ForwardedElement[] Present(ForwardedElement first, ForwardedElement[] rest)
	{
		var elements = new List<ForwardedElement>(1 + rest.Length);

		if (first.Pairs.Count > 0)
			elements.Add(first);

		foreach (var element in rest)
			if (element.Pairs.Count > 0)
				elements.Add(element);

		return [.. elements];
	}

	/// <summary>The pairs a run of slots holds: the empty ones left out, each value unquoted.</summary>
	internal static ForwardedElement.Pair[] Pairs(string first, string[] rest)
	{
		var pairs = new List<ForwardedElement.Pair>(1 + rest.Length);

		Add(first);

		foreach (var slot in rest)
			Add(slot);

		return [.. pairs];

		void Add(string slot)
		{
			if (slot.Length == 0)
				return;

			var equals = slot.IndexOf('=');
			var value  = slot.Substring(equals + 1);

			if (value[0] == '"')
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

			pairs.Add(new ForwardedElement.Pair(slot.Substring(0, equals), value));
		}
	}

	/// <summary>Whether pairs make an element: each name once, and each value of §5's parameters what §5 says.</summary>
	internal static bool IsElement(ForwardedElement.Pair[] pairs)
	{
		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var pair in pairs)
		{
			if (!seen.Add(pair.Name))
				return false;

			var valid = pair.Name.ToLowerInvariant() switch
			{
				"by" or "for" => TryParseNode(pair.Value, out _),
				"host"        => IsHost(pair.Value),
				"proto"       => IsScheme(pair.Value),
				_             => true,
			};

			if (!valid)
				return false;
		}

		return true;
	}

	/// <summary>A node-port from what followed a name: nothing, or a colon and the port.</summary>
	internal static string? Port(string text) => text.Length == 0 ? null : text.Substring(1);

	// RFC 7230 §5.4: Host = uri-host [ ":" port ], an authority with no userinfo.
	static bool IsHost(string value)
	{
		return Rfc3986.TryParseReference("//" + value, out var reference) &&
			reference is { UserInfo: null, Path: "", Query: null, Fragment: null };
	}

	// RFC 3986 §3.1: scheme = ALPHA *( ALPHA / DIGIT / "+" / "-" / "." ).
	static bool IsScheme(string value)
	{
		if (value.Length == 0 || value[0] is not (>= 'a' and <= 'z' or >= 'A' and <= 'Z'))
			return false;

		foreach (var character in value)
			if (character is not (>= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '+' or '-' or '.'))
				return false;

		return true;
	}
}
