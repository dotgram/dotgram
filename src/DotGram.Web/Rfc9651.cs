using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>What a List or a Dictionary holds: an Item or an Inner List, each with its parameters.</summary>
/// <remarks>RFC 9651 §3.1. The two are told apart by type: <c>member is Item</c>.</remarks>
public abstract record Member(OrderedMap<BareItem> Parameters);

/// <summary>A bare item and its parameters (RFC 9651 §3.3).</summary>
public sealed record Item(BareItem Value, OrderedMap<BareItem> Parameters) : Member(Parameters);

/// <summary>Items in parentheses, and the parameters of the whole (RFC 9651 §3.1.1).</summary>
/// <remarks>Equal to another with equal items in the same order and equal parameters.</remarks>
public sealed record InnerList(IReadOnlyList<Item> Items, OrderedMap<BareItem> Parameters) : Member(Parameters)
{
	/// <summary>Whether the other list has the same items in the same order and the same parameters.</summary>
	public bool Equals(InnerList? other) => base.Equals(other) && Structural.Same(Items, other!.Items);

	/// <summary>A hash over the items and the parameters.</summary>
	public override int GetHashCode() => Structural.Combine(base.GetHashCode(), Structural.Hash(Items));
}

/// <summary>Structured Field Values for HTTP (RFC 9651): the three types a field is, read and written.</summary>
/// <remarks>
/// A field says which of the three it is — an Item, a List or a Dictionary — in the specification that defines
/// it, not in its value, so the reading to use is the caller's to choose.
/// </remarks>
public static class StructuredField
{
	/// <summary>An Item field value (§4.2.3).</summary>
	/// <exception cref="FormatException">The text is no Item; the message says where.</exception>
	public static Item ParseItem(string text) =>
		Rfc9651.ParseItem(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>An Item field value, or false where the text is not one.</summary>
	public static bool TryParseItem(string text, [NotNullWhen(true)] out Item? item)
	{
		var read = Rfc9651.TryParseItem(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		item = read ? parsed : null;

		return read;
	}

	/// <summary>A List field value (§4.2.1).</summary>
	/// <exception cref="FormatException">The text is no List; the message says where.</exception>
	public static IReadOnlyList<Member> ParseList(string text) =>
		Rfc9651.ParseList(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A List field value, or false where the text is not one.</summary>
	public static bool TryParseList(string text, [NotNullWhen(true)] out IReadOnlyList<Member>? list)
	{
		var read = Rfc9651.TryParseList(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		list = read ? parsed : null;

		return read;
	}

	/// <summary>A Dictionary field value (§4.2.2).</summary>
	/// <exception cref="FormatException">The text is no Dictionary; the message says where.</exception>
	public static OrderedMap<Member> ParseDictionary(string text) =>
		Rfc9651.ParseDictionary(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A Dictionary field value, or false where the text is not one.</summary>
	public static bool TryParseDictionary(string text, [NotNullWhen(true)] out OrderedMap<Member>? dictionary)
	{
		var read = Rfc9651.TryParseDictionary(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		dictionary = read ? parsed : null;

		return read;
	}

	/// <summary>Several lines of one field as the one value a field is read from.</summary>
	/// <remarks>
	/// §4.2: a parser "MUST combine all field lines … into one comma-separated field-value". Members of a List or
	/// a Dictionary survive that; a String split across two lines does not, and gains the comma, as the RFC warns.
	/// </remarks>
	public static string Combine(IEnumerable<string> lines) => Rfc9651.Combine(lines);

	/// <summary>An Item as §4.1.3 serializes it.</summary>
	/// <exception cref="ArgumentException">The Item holds what has no serialization, as §4.1 lists.</exception>
	public static string SerializeItem(Item item) => Rfc9651.SerializeItem(item);

	/// <summary>A List as §4.1.1 serializes it.</summary>
	/// <exception cref="ArgumentException">The List holds what has no serialization, as §4.1 lists.</exception>
	public static string SerializeList(IReadOnlyList<Member> list) => Rfc9651.SerializeList(list);

	/// <summary>A Dictionary as §4.1.2 serializes it.</summary>
	/// <exception cref="ArgumentException">The Dictionary holds what has no serialization, as §4.1 lists.</exception>
	public static string SerializeDictionary(OrderedMap<Member> dictionary) => Rfc9651.SerializeDictionary(dictionary);
}

/// <summary>One of the eight values RFC 9651 §3.3 defines.</summary>
/// <remarks>
/// A closed set: the eight are nested here and nothing outside can add a ninth, so a
/// <c>switch</c> over them is the whole of what a value can be.
/// </remarks>
public abstract record BareItem
{
	BareItem()
	{
	}

	/// <summary>§3.3.1: fifteen digits at most, signed.</summary>
	public sealed record Integer(long Value) : BareItem;

	/// <summary>§3.3.2: twelve digits before the point at most, and three after.</summary>
	public sealed record Decimal(decimal Value) : BareItem;

	/// <summary>§3.3.3: printable ASCII, with its escapes taken off.</summary>
	public sealed record String(string Value) : BareItem;

	/// <summary>§3.3.4: a word that begins with a letter or <c>*</c>.</summary>
	public sealed record Token(string Value) : BareItem;

	/// <summary>§3.3.5: the bytes the base64 between the colons stands for.</summary>
	/// <remarks>Equal to another holding the same bytes.</remarks>
	public sealed record ByteSequence(byte[] Value) : BareItem
	{
		/// <summary>Whether the other sequence holds the same bytes.</summary>
		public bool Equals(ByteSequence? other) => other is not null && Structural.Same(Value, other.Value);

		/// <summary>A hash over the bytes.</summary>
		public override int GetHashCode() => Structural.Hash(Value);
	}

	/// <summary>§3.3.6.</summary>
	public sealed record Boolean(bool Value) : BareItem
	{
		/// <summary>The true value, which §3.3.6 writes <c>?1</c>.</summary>
		public static Boolean True { get; } = new(true);

		/// <summary>The false value, which §3.3.6 writes <c>?0</c>.</summary>
		public static Boolean False { get; } = new(false);
	}

	/// <summary>§3.3.7: seconds from 1970-01-01T00:00:00Z, leap seconds excluded.</summary>
	public sealed record Date(long Value) : BareItem;

	/// <summary>§3.3.8: Unicode text, with its percent-escapes decoded as UTF-8.</summary>
	public sealed record DisplayString(string Value) : BareItem;
}

/// <summary>Keys in the order they were written, each once, reachable by position and by name.</summary>
/// <remarks>
/// What RFC 9651 makes of Parameters (§3.1.2) and of a Dictionary (§3.2): implementations
/// "MUST provide access … both by index and by key". A key written twice keeps the place it
/// was first written in and the value it was last given (§4.2.2, §4.2.3.2).
/// </remarks>
public sealed class OrderedMap<T> : IReadOnlyList<KeyValuePair<string, T>>, IEquatable<OrderedMap<T>>
{
	/// <summary>Whether the other map has the same keys with equal values, in the same order.</summary>
	public bool Equals(OrderedMap<T>? other)
	{
		if (ReferenceEquals(this, other))
			return true;

		if (other is null || other._entries.Count != _entries.Count)
			return false;

		for (var index = 0; index < _entries.Count; index++)
			if (!string.Equals(_entries[index].Key, other._entries[index].Key, StringComparison.Ordinal) ||
				!EqualityComparer<T>.Default.Equals(_entries[index].Value, other._entries[index].Value))
			{
				return false;
			}

		return true;
	}

	/// <summary>
	/// Whether the other is a map of the same kind with the same keys and values in the same
	/// order.
	/// </summary>
	public override bool Equals(object? other) => Equals(other as OrderedMap<T>);

	/// <summary>A hash over the keys and the values, in order.</summary>
	public override int GetHashCode()
	{
		var hash = _entries.Count;

		foreach (var entry in _entries)
			hash = Structural.Combine(
				Structural.Combine(hash, StringComparer.Ordinal.GetHashCode(entry.Key)),
				entry.Value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(entry.Value));

		return hash;
	}

	readonly List<KeyValuePair<string, T>> _entries = [];
	readonly Dictionary<string, int>       _places  = new(StringComparer.Ordinal);

	internal OrderedMap()
	{
	}

	/// <summary>A map of these entries in their order, for a value to be serialized.</summary>
	/// <remarks>A key given twice keeps the place it was first given in and the value it was last given.</remarks>
	/// <exception cref="ArgumentException">A key is null.</exception>
	public OrderedMap(IEnumerable<KeyValuePair<string, T>> entries)
	{
		if (entries is null)
			throw new ArgumentNullException(nameof(entries));

		foreach (var entry in entries)
			Set(entry.Key ?? throw new ArgumentException("A key is null.", nameof(entries)), entry.Value);
	}

	/// <summary>How many entries the map holds.</summary>
	public int Count => _entries.Count;

	/// <summary>The entry at a place, counting from zero in the order the entries were written.</summary>
	public KeyValuePair<string, T> this[int index] => _entries[index];

	/// <exception cref="KeyNotFoundException">The key is not in the map.</exception>
	public T this[string key] =>
		TryGetValue(key, out var value) ? value : throw new KeyNotFoundException($"'{key}' is not in the map.");

	/// <summary>Whether a key is one of the map's, compared as it was written.</summary>
	public bool ContainsKey(string key) => _places.ContainsKey(key);

	/// <summary>The value a key names, or false where the map has no such key.</summary>
	public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
	{
		if (_places.TryGetValue(key, out var place))
		{
			value = _entries[place].Value;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>The entries in the order they were written.</summary>
	public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => _entries.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	internal void Set(string key, T value)
	{
		if (_places.TryGetValue(key, out var place))
			_entries[place] = new KeyValuePair<string, T>(key, value);
		else
		{
			_places.Add(key, _entries.Count);
			_entries.Add(new KeyValuePair<string, T>(key, value));
		}
	}
}

// RFC 9651, Structured Field Values for HTTP. The grammar is Appendix C's ABNF, rule for
// rule, and where it and the parsing algorithms of §4.2 differ the algorithms win, as the
// RFC says they do. Where they differ is said beside the rule it touches:
//
//   * Whitespace. A field may begin with spaces and end with them (§4.2 steps 2 and 6);
//     between the members of a List or a Dictionary a tab is as good as a space, since
//     field lines are joined with commas and some joiners add one; inside an Inner List and
//     after a `;` only spaces are.
//   * What the ABNF lets through and the algorithm refuses: a base64 body that does not
//     decode, and a Display String whose bytes are not UTF-8. Both are a `when`.
//   * What the ABNF refuses and the algorithm reads: base64 without its `=` padding, which
//     parsers "SHOULD NOT fail" on (§4.2.7). The padding is put back before decoding.
//
// Numbers are the one place ordered choice has to be told something. A Decimal is tried
// before an Integer, so `4.5` is not an Integer `4` followed by something unreadable; and a
// digit too many is not refused by the number itself but by whatever must follow it — a
// sixteenth digit is neither a `;`, a `,`, a space nor the end, so the field fails there,
// which is what §4.2.4 says it must.

[Gram("""
	@using System;
	@using System.Collections.Generic;
	@using DotGram.Web;

	trivia = none

	// ── Characters ───────────────────────────────────────────────────────────────

	Digit    = ['0'..'9']
	Alpha    = ['a'..'z' | 'A'..'Z']
	Lcalpha  = ['a'..'z']
	LcHexdig = ['0'..'9' | 'a'..'f']

	// RFC 9110 §5.6.2, the token characters, and OWS beside them (§5.6.3).
	Tchar = ['!' | '#' | '$' | '%' | '&' | '\'' | '*' | '+' | '-' | '.' | '^' | '_' | '`' | '|' | '~' | '0'..'9' | 'a'..'z' | 'A'..'Z']
	Ows   = [' ' | '\t']*

	// ── The three fields (§4.2) ──────────────────────────────────────────────────

	ItemField : @Item = ' '* & item: SfItem & ' '* => @(item)

	ListField : @IReadOnlyList<Member> = ' '* & (first: ListMember & rest: ListRest* & Ows)?
		=> @(Rfc9651.Members(first, rest))

	DictionaryField : @OrderedMap<Member> = ' '* & (first: DictMember & rest: DictRest* & Ows)?
		=> @(Rfc9651.Map(first, rest))

	// ── Lists (§3.1, §4.2.1) ─────────────────────────────────────────────────────

	ListRest : @Member = Ows & ',' & Ows & member: ListMember => @(member)

	ListMember : @Member = inner: SfInnerList => @(inner) | item: SfItem => @(item)

	// §4.2.1.2. An item is followed by spaces or by the closing bracket, and nothing else.
	SfInnerList : @InnerList
		= '(' & ' '* & items: InnerMember* & ')' & parameters: SfParameters
		=> @(new InnerList(items, parameters))

	InnerMember : @Item = item: SfItem & (' '+ | ?=')') => @(item)

	// ── Parameters (§3.1.2, §4.2.3.2) ────────────────────────────────────────────

	SfParameters : @OrderedMap<BareItem> = parameters: Parameter* => @(Rfc9651.Map(parameters))

	// A key alone is a true Boolean.
	Parameter : @KeyValuePair<string, BareItem> = ';' & ' '* & key: Key & ('=' & value: SfBareItem)?
		=> @(new KeyValuePair<string, BareItem>(key, value ?? BareItem.Boolean.True))

	Key = [Lcalpha | '*'] & [Lcalpha | Digit | '_' | '-' | '.' | '*']*

	// ── Dictionaries (§3.2, §4.2.2) ──────────────────────────────────────────────

	DictRest : @KeyValuePair<string, Member> = Ows & ',' & Ows & member: DictMember => @(member)

	// A key with no `=` is a true Boolean with whatever parameters follow it.
	DictMember : @KeyValuePair<string, Member> = key: Key & ('=' & value: ListMember | parameters: SfParameters)
		=> @(new KeyValuePair<string, Member>(key, value ?? new Item(BareItem.Boolean.True, parameters!)))

	// ── Items (§3.3, §4.2.3) ─────────────────────────────────────────────────────

	SfItem : @Item = value: SfBareItem & parameters: SfParameters => @(new Item(value, parameters))

	// §4.2.3.1. Each alternative begins with a character no other one does, so which one is
	// read is decided by the first character, as the algorithm decides it.
	SfBareItem : @BareItem
		= text: SfDecimal        => @(new BareItem.Decimal(Rfc9651.Decimal(text)))
		| text: SfInteger        => @(new BareItem.Integer(Rfc9651.Integer(text)))
		| text: SfString         => @(new BareItem.String(Rfc9651.Unquoted(text)))
		| text: SfToken          => @(new BareItem.Token(text))
		| bytes: SfBinary        => @(new BareItem.ByteSequence(bytes))
		| "?1"                   => @(BareItem.Boolean.True)
		| "?0"                   => @(BareItem.Boolean.False)
		| '@' & text: SfInteger  => @(new BareItem.Date(Rfc9651.Integer(text)))
		| shown: SfDisplayString => @(new BareItem.DisplayString(shown))

	SfInteger = '-'? & Digit{1,15}
	SfDecimal = '-'? & Digit{1,12} & '.' & Digit{1,3}

	// §4.2.5. Only a quote and a backslash may be escaped.
	SfString  = '"' & ([' '..'!' | '#'..'[' | ']'..'~'] | '\\' & ['"' | '\\'])* & '"'

	SfToken   = [Alpha | '*'] & [Tchar | ':' | '/']*

	// §4.2.7. Padding only at the end, and a body that decodes.
	SfBinary : @byte[]
		= ':' & text: Base64 & ':' & when @(Rfc9651.Bytes(text!) is not null)
		=> @(Rfc9651.Bytes(text)!)

	Base64 = [Alpha | Digit | '+' | '/']* & '='*

	// §4.2.10. Lowercase escapes only, and bytes that are UTF-8.
	SfDisplayString : @string
		= '%' & '"' & text: DisplayText & '"' & when @(Rfc9651.Displayed(text!) is not null)
		=> @(Rfc9651.Displayed(text)!)

	DisplayText = ([' '..'!' | '#'..'$' | '&'..'~'] | '%' & LcHexdig & LcHexdig)*

	parse ItemField       as ParseItem
	parse ListField       as ParseList
	parse DictionaryField as ParseDictionary
	""")]
static partial class Rfc9651
{
	// ParseItem, ParseList and ParseDictionary are generated here, each with its Try form.

	/// <summary>Several lines of one field as the one value RFC 9651 parses.</summary>
	/// <remarks>
	/// §4.2: a parser "MUST combine all field lines … into one comma-separated field-value".
	/// Members of a List or a Dictionary survive that; a String split across two lines does
	/// not, and gains the comma, as the RFC warns.
	/// </remarks>
	public static string Combine(IEnumerable<string> lines)
	{
		if (lines is null)
			throw new ArgumentNullException(nameof(lines));

		return string.Join(", ", lines);
	}

	internal static IReadOnlyList<Member> Members(Member? first, Member[]? rest)
	{
		var members = new List<Member>(1 + (rest?.Length ?? 0));

		if (first is not null)
			members.Add(first);

		if (rest is not null)
			members.AddRange(rest);

		return members;
	}

	internal static OrderedMap<T> Map<T>(KeyValuePair<string, T>[] entries)
	{
		var map = new OrderedMap<T>();

		foreach (var entry in entries)
			map.Set(entry.Key, entry.Value);

		return map;
	}

	internal static OrderedMap<T> Map<T>(KeyValuePair<string, T>? first, KeyValuePair<string, T>[]? rest)
	{
		var map = new OrderedMap<T>();

		if (first is { } one)
			map.Set(one.Key, one.Value);

		if (rest is not null)
			foreach (var entry in rest)
				map.Set(entry.Key, entry.Value);

		return map;
	}

	internal static long Integer(string text) =>
		long.Parse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

	internal static decimal Decimal(string text) =>
		decimal.Parse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

	/// <summary>A String's text, its quotes and its escapes taken off.</summary>
	internal static string Unquoted(string quoted)
	{
		var built = new StringBuilder(quoted.Length - 2);

		for (var at = 1; at < quoted.Length - 1; at++)
		{
			if (quoted[at] == '\\')
				at++;

			built.Append(quoted[at]);
		}

		return built.ToString();
	}

	/// <summary>The bytes a base64 body stands for, or null where it stands for none.</summary>
	/// <remarks>
	/// Padding is put back where it was left off, which §4.2.7 asks for; a body whose length
	/// no padding can make whole, or with more padding than base64 has, does not decode.
	/// Pad bits that are not zero are let through, which the RFC also asks.
	/// </remarks>
	internal static byte[]? Bytes(string text)
	{
		var padded = (text.Length % 4) switch
		{
			0 => text,
			2 => text + "==",
			3 => text + "=",
			_ => null,
		};

		if (padded is null)
			return null;

		try
		{
			return Convert.FromBase64String(padded);
		}
		catch (FormatException)
		{
			return null;
		}
	}

	/// <summary>A Display String's text decoded, or null where its bytes are not UTF-8.</summary>
	internal static string? Displayed(string text)
	{
		// The grammar let through only printable ASCII and `%` with two lowercase hex digits,
		// so each character is one byte or three characters are.
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

		try
		{
			return Strict.GetString(bytes, 0, count);
		}
		catch (DecoderFallbackException)
		{
			return null;
		}
	}

	static int Hex(char digit) => digit <= '9' ? digit - '0' : digit - 'a' + 10;

	// ── Serializing (§4.1) ───────────────────────────────────────────────────────

	/// <summary>An Item as the value of a field (§4.1.3).</summary>
	/// <exception cref="ArgumentException">Something in it has no serialization: §4.1 fails there.</exception>
	public static string SerializeItem(Item item)
	{
		if (item is null)
			throw new ArgumentNullException(nameof(item));

		var output = new StringBuilder();

		Serialize(output, item);

		return output.ToString();
	}

	/// <summary>A List as the value of a field (§4.1.1).</summary>
	/// <remarks>An empty List is the empty string, and §4.1 says not to send the field at all.</remarks>
	/// <exception cref="ArgumentException">Something in it has no serialization: §4.1 fails there.</exception>
	public static string SerializeList(IReadOnlyList<Member> list)
	{
		if (list is null)
			throw new ArgumentNullException(nameof(list));

		var output = new StringBuilder();

		for (var index = 0; index < list.Count; index++)
		{
			if (index > 0)
				output.Append(", ");

			Serialize(output, list[index] ?? throw Refused("a member of a List is null"));
		}

		return output.ToString();
	}

	/// <summary>A Dictionary as the value of a field (§4.1.2).</summary>
	/// <remarks>An empty Dictionary is the empty string, and §4.1 says not to send the field at all.</remarks>
	/// <exception cref="ArgumentException">Something in it has no serialization: §4.1 fails there.</exception>
	public static string SerializeDictionary(OrderedMap<Member> dictionary)
	{
		if (dictionary is null)
			throw new ArgumentNullException(nameof(dictionary));

		var output = new StringBuilder();

		for (var index = 0; index < dictionary.Count; index++)
		{
			// Not deconstructed: KeyValuePair has no Deconstruct on netstandard2.0.
			var key    = dictionary[index].Key;
			var member = dictionary[index].Value;

			if (index > 0)
				output.Append(", ");

			Key(output, key);

			// A true Boolean is written as the key alone, with its parameters.
			if (member is Item { Value: BareItem.Boolean { Value: true } } truth)
				Parameters(output, truth.Parameters);
			else
			{
				output.Append('=');
				Serialize(output, member ?? throw Refused($"the member '{key}' is null"));
			}
		}

		return output.ToString();
	}

	static void Serialize(StringBuilder output, Member member)
	{
		if (member is InnerList inner)
		{
			output.Append('(');

			for (var index = 0; index < inner.Items.Count; index++)
			{
				if (index > 0)
					output.Append(' ');

				Serialize(output, inner.Items[index] ?? throw Refused("an item of an Inner List is null"));
			}

			output.Append(')');
			Parameters(output, inner.Parameters);

			return;
		}

		var item = (Item)member;

		Bare(output, item.Value);
		Parameters(output, item.Parameters);
	}

	// §4.1.1.2. A true Boolean is written as the key alone.
	static void Parameters(StringBuilder output, OrderedMap<BareItem> parameters)
	{
		foreach (var parameter in parameters ?? throw Refused("parameters are null"))
		{
			output.Append(';');
			Key(output, parameter.Key);

			if (parameter.Value is not BareItem.Boolean { Value: true })
			{
				output.Append('=');
				Bare(output, parameter.Value);
			}
		}
	}

	// §4.1.1.3.
	static void Key(StringBuilder output, string key)
	{
		if (key.Length == 0 || !(key[0] is >= 'a' and <= 'z' or '*'))
			throw Refused($"the key '{key}' does not begin with a lowercase letter or '*'");

		foreach (var character in key)
			if (!(character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' or '.' or '*'))
				throw Refused($"the key '{key}' holds a character a key may not");

		output.Append(key);
	}

	// §4.1.3.1.
	static void Bare(StringBuilder output, BareItem value)
	{
		switch (value)
		{
			case BareItem.Integer integer:
				Integer(output, integer.Value);
				break;

			case BareItem.Decimal fraction:
				Decimal(output, fraction.Value);
				break;

			case BareItem.String text:
				output.Append('"');

				foreach (var character in text.Value)
				{
					if (character is < ' ' or > '~')
						throw Refused("a String holds a character that is not printable ASCII");

					if (character is '"' or '\\')
						output.Append('\\');

					output.Append(character);
				}

				output.Append('"');
				break;

			case BareItem.Token token:
				Token(output, token.Value);
				break;

			case BareItem.ByteSequence bytes:
				output.Append(':').Append(Convert.ToBase64String(bytes.Value ?? throw Refused("a Byte Sequence is null"))).Append(':');
				break;

			case BareItem.Boolean truth:
				output.Append(truth.Value ? "?1" : "?0");
				break;

			case BareItem.Date date:
				output.Append('@');
				Integer(output, date.Value);
				break;

			case BareItem.DisplayString display:
				Displayed(output, display.Value);
				break;

			default:
				throw Refused("a bare item is null");
		}
	}

	// §4.1.4.
	static void Integer(StringBuilder output, long value)
	{
		if (value is < -999_999_999_999_999 or > 999_999_999_999_999)
			throw Refused($"the Integer {value} has more than fifteen digits");

		output.Append(value.ToString(CultureInfo.InvariantCulture));
	}

	// §4.1.5. Three places after the point, rounded half to even, and twelve digits before it.
	static void Decimal(StringBuilder output, decimal value)
	{
		var rounded = Math.Round(value, 3, MidpointRounding.ToEven);
		var size    = Math.Abs(rounded);

		if (decimal.Truncate(size) > 999_999_999_999m)
			throw Refused($"the Decimal {value} has more than twelve digits before the point");

		if (rounded < 0)
			output.Append('-');

		output.Append(size.ToString("0.0##", CultureInfo.InvariantCulture));
	}

	// §4.1.7.
	static void Token(StringBuilder output, string token)
	{
		if (token.Length == 0 || !(token[0] is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '*'))
			throw Refused($"the Token '{token}' does not begin with a letter or '*'");

		foreach (var character in token)
			if (!(character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or
				'!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~' or
				':' or '/'))
			{
				throw Refused($"the Token '{token}' holds a character a token may not");
			}

		output.Append(token);
	}

	// §4.1.11. A byte that is `%`, `"` or not printable ASCII is escaped, in lowercase hex.
	static void Displayed(StringBuilder output, string text)
	{
		byte[] bytes;

		try
		{
			bytes = Strict.GetBytes(text);
		}
		catch (EncoderFallbackException)
		{
			throw Refused("a Display String holds a surrogate that is not half of a pair");
		}

		output.Append("%\"");

		foreach (var octet in bytes)
		{
			if (octet is (byte)'%' or (byte)'"' or < 0x20 or > 0x7E)
				output.Append('%').Append(LowerHex[octet >> 4]).Append(LowerHex[octet & 0xF]);
			else
				output.Append((char)octet);
		}

		output.Append('"');
	}

	const string LowerHex = "0123456789abcdef";

	static ArgumentException Refused(string why) =>
		new($"This value has no serialization in RFC 9651 §4.1: {why}.");

	static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
}
