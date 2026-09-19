using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>A JSON value as RFC 8259 defines one: an object, an array, a number, a string or a literal name.</summary>
/// <remarks>
/// <para>
/// A closed set: the six are nested here and nothing outside can add a seventh.
/// </para>
/// <para>
/// Nothing a text says is thrown away. A number keeps its text, since §6 lets the precision a
/// reader wants be the reader's; an object keeps its members in the order written, and keeps a
/// name written twice, since §4 leaves what that means to whoever reads it.
/// </para>
/// </remarks>
public abstract record JsonValue
{
	JsonValue()
	{
	}

	/// <summary>A JSON text (RFC 8259 §2): one value, with whitespace around it.</summary>
	/// <exception cref="FormatException">The text is not JSON; the message says where it stops being so.</exception>
	public static JsonValue Parse(string text) =>
		Rfc8259.ParseJson(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A JSON text, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out JsonValue? value)
	{
		var read = Rfc8259.TryParseJson(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		value = read ? parsed : null;

		return read;
	}

	/// <summary>§4: members in the order written, names as unescaped strings, duplicates kept.</summary>
	/// <remarks>Equal to another with the same members in the same order, a name written twice counted twice.</remarks>
	public sealed record Object(IReadOnlyList<KeyValuePair<string, JsonValue>> Members) : JsonValue
	{
		public bool Equals(Object? other)
		{
			return other is not null && Same(this, other);
		}

		public override int GetHashCode()
		{
			return Hash(this);
		}
	}

	/// <summary>§5.</summary>
	/// <remarks>Equal to another with equal elements in the same order.</remarks>
	public sealed record Array(IReadOnlyList<JsonValue> Items) : JsonValue
	{
		public bool Equals(Array? other)
		{
			return other is not null && Same(this, other);
		}

		public override int GetHashCode()
		{
			return Hash(this);
		}
	}

	/// <summary>§7: the string with its escapes undone. An escaped lone surrogate stays one (§8.2).</summary>
	public sealed record String(string Value) : JsonValue;

	/// <summary>§6: the number as written, which <see cref="ToDouble"/> and the rest read at a precision of the caller's choice.</summary>
	public sealed record Number(string Text) : JsonValue
	{
		/// <summary>The nearest double, as IEEE 754 binary64 reads it; an exponent past its range is infinity.</summary>
		public double ToDouble() => double.Parse(Text, NumberStyles.Float, CultureInfo.InvariantCulture);

		/// <summary>The number as a decimal, where one holds it exactly.</summary>
		/// <returns>
		/// False when the number is out of a decimal's range, or would lose a digit that is not a
		/// trailing zero after the point: <c>1e-30</c> and a thirty-five digit fraction are refused,
		/// <c>1.500</c> is 1.5.
		/// </returns>
		public bool TryToDecimal(out decimal value)
		{
			if (decimal.TryParse(Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
				Reduce(Text, out var negative, out var digits, out var exponent) &&
				Reduce(value.ToString(CultureInfo.InvariantCulture), out var writtenNegative, out var writtenDigits, out var writtenExponent) &&
				digits == writtenDigits &&
				(digits.Length == 0 || negative == writtenNegative && exponent == writtenExponent))
				return true;

			value = default;

			return false;
		}

		// A number as its significant digits, without leading or trailing zeros, and the power of
		// ten of the last of them; zero has no digits. Both a JSON number and a decimal written
		// back are read this way, so that equal values reduce to equal parts.
		static bool Reduce(string text, out bool negative, out string digits, out long exponent)
		{
			var position = 0;

			negative = text.Length > 0 && text[0] == '-';

			if (text.Length > 0 && (text[0] == '-' || text[0] == '+'))
				position++;

			var significand = new StringBuilder();
			var fraction    = 0;

			while (position < text.Length && text[position] is >= '0' and <= '9')
				significand.Append(text[position++]);

			if (position < text.Length && text[position] == '.')
			{
				position++;

				while (position < text.Length && text[position] is >= '0' and <= '9')
				{
					significand.Append(text[position++]);
					fraction++;
				}
			}

			exponent = 0;

			var huge = false;

			if (position < text.Length && text[position] is 'e' or 'E')
			{
				position++;

				var sign = position < text.Length && text[position] == '-' ? -1 : 1;

				if (position < text.Length && text[position] is '-' or '+')
					position++;

				var start = position;

				while (position < text.Length && text[position] is >= '0' and <= '9')
				{
					// Past eighteen digits an exponent is beyond any decimal, and only zero survives it.
					if (position - start < 18)
						exponent = exponent * 10 + (text[position] - '0');
					else
						huge = true;

					position++;
				}

				exponent *= sign;
			}

			if (position != text.Length)
			{
				digits = "";
				return false;
			}

			var first = 0;

			while (first < significand.Length && significand[first] == '0')
				first++;

			var last = significand.Length;

			while (last > first && significand[last - 1] == '0')
				last--;

			digits   = significand.ToString(first, last - first);
			exponent = digits.Length == 0 ? 0 : exponent - fraction + (significand.Length - last);

			if (huge && digits.Length != 0)
				return false;

			return true;
		}

		/// <summary>The number as a long, where it is an integer written without a fraction or exponent.</summary>
		public bool TryToInt64(out long value) =>
			long.TryParse(Text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
	}

	/// <summary>§3: <c>true</c> or <c>false</c>.</summary>
	public sealed record Boolean(bool Value) : JsonValue
	{
		public static Boolean True { get; } = new(true);

		public static Boolean False { get; } = new(false);
	}

	/// <summary>§3: <c>null</c>.</summary>
	public sealed record Null : JsonValue
	{
		public static Null Instance { get; } = new();
	}

	/// <summary>The value as JSON text with no insignificant whitespace, which reads back as the same value.</summary>
	/// <remarks>Sealed, or each case would print itself the way a record does instead.</remarks>
	public sealed override string ToString()
	{
		var output  = new StringBuilder();
		var pending = new Stack<Written>();

		pending.Push(new Written(this, null));

		// With a stack of its own rather than the thread's: a text the parser reads may nest
		// deeper than a thread's stack goes, and what it reads has to write back.
		while (pending.Count > 0)
		{
			var next = pending.Pop();

			if (next.Value is null)
			{
				output.Append(next.Text);

				continue;
			}

			switch (next.Value)
			{
				case Object members:
					output.Append('{');
					pending.Push(new Written(null, "}"));

					for (var index = members.Members.Count - 1; index >= 0; index--)
					{
						pending.Push(new Written(members.Members[index].Value, null));
						pending.Push(new Written(null, Quoted(members.Members[index].Key) + ":"));

						if (index > 0)
							pending.Push(new Written(null, ","));
					}

					break;

				case Array items:
					output.Append('[');
					pending.Push(new Written(null, "]"));

					for (var index = items.Items.Count - 1; index >= 0; index--)
					{
						pending.Push(new Written(items.Items[index], null));

						if (index > 0)
							pending.Push(new Written(null, ","));
					}

					break;

				case String text:
					Quoted(output, text.Value);
					break;

				case Number number:
					output.Append(number.Text);
					break;

				case Boolean truth:
					output.Append(truth.Value ? "true" : "false");
					break;

				default:
					output.Append("null");
					break;
			}
		}

		return output.ToString();
	}

	/// <summary>What is still to be written: a value, or text as it stands.</summary>
	readonly struct Written(JsonValue? value, string? text)
	{
		public readonly JsonValue? Value = value;
		public readonly string?    Text  = text;
	}

	/// <summary>
	/// Two objects or arrays alike, member by member and element by element, walked with a stack
	/// of its own for the reason <see cref="ToString"/> is.
	/// </summary>
	static bool Same(JsonValue left, JsonValue right)
	{
		var pending = new Stack<KeyValuePair<JsonValue, JsonValue>>();

		pending.Push(new KeyValuePair<JsonValue, JsonValue>(left, right));

		while (pending.Count > 0)
		{
			var pair = pending.Pop();

			if (ReferenceEquals(pair.Key, pair.Value))
				continue;

			switch (pair.Key)
			{
				case Array items when pair.Value is Array others:
					if (items.Items.Count != others.Items.Count)
						return false;

					for (var index = 0; index < items.Items.Count; index++)
						pending.Push(new KeyValuePair<JsonValue, JsonValue>(items.Items[index], others.Items[index]));

					break;

				case Object members when pair.Value is Object others:
					if (members.Members.Count != others.Members.Count)
						return false;

					for (var index = 0; index < members.Members.Count; index++)
					{
						if (!string.Equals(members.Members[index].Key, others.Members[index].Key, StringComparison.Ordinal))
							return false;

						pending.Push(new KeyValuePair<JsonValue, JsonValue>(members.Members[index].Value, others.Members[index].Value));
					}

					break;

				case Array or Object:
					return false;

				// A number, a string or a literal name holds nothing nested, and compares as a record.
				default:
					if (!pair.Key.Equals(pair.Value))
						return false;

					break;
			}
		}

		return true;
	}

	/// <summary>
	/// A hash of everything an object or array holds, taken in the order a walk meets it, so that
	/// values <see cref="Same"/> calls alike hash alike.
	/// </summary>
	static int Hash(JsonValue value)
	{
		var hash    = 0;
		var pending = new Stack<JsonValue>();

		pending.Push(value);

		while (pending.Count > 0)
		{
			switch (pending.Pop())
			{
				case Array items:
					hash = Structural.Combine(hash, items.Items.Count * 2);

					for (var index = items.Items.Count - 1; index >= 0; index--)
						pending.Push(items.Items[index]);

					break;

				case Object members:
					hash = Structural.Combine(hash, members.Members.Count * 2 + 1);

					foreach (var member in members.Members)
						hash = Structural.Combine(hash, StringComparer.Ordinal.GetHashCode(member.Key));

					for (var index = members.Members.Count - 1; index >= 0; index--)
						pending.Push(members.Members[index].Value);

					break;

				case var scalar:
					hash = Structural.Combine(hash, scalar.GetHashCode());
					break;
			}
		}

		return hash;
	}

	static string Quoted(string text)
	{
		var output = new StringBuilder(text.Length + 2);

		Quoted(output, text);

		return output.ToString();
	}

	// §7: a quotation mark, a reverse solidus and the controls are the characters that must be escaped.
	static void Quoted(StringBuilder output, string text)
	{
		output.Append('"');

		foreach (var character in text)
		{
			switch (character)
			{
				case '"':  output.Append("\\\""); break;
				case '\\': output.Append("\\\\"); break;
				case '\b': output.Append("\\b");  break;
				case '\f': output.Append("\\f");  break;
				case '\n': output.Append("\\n");  break;
				case '\r': output.Append("\\r");  break;
				case '\t': output.Append("\\t");  break;

				default:
					if (character < ' ' || char.IsSurrogate(character))
						output.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
					else
						output.Append(character);

					break;
			}
		}

		output.Append('"');
	}
}

// RFC 8259, The JavaScript Object Notation (JSON) Data Interchange Format. The grammar is its ABNF,
// production for production, with `ws` written where the six structural characters carry it. A JSON
// text is a value between whitespace — any value, not only an object or an array (§2).
//
// A text is read as characters: §8.1 says JSON between systems is UTF-8, and decoding it is the
// caller's. A byte order mark is not whitespace and is refused, which §8.1 allows; a caller who wants
// it ignored takes it off first. Nesting is as deep as the input goes — the reading moves to a fresh
// stack where one runs low — and a limit, which §9 allows, is the caller's to set.

[Gram("""
	@using System;
	@using System.Collections.Generic;
	@using DotGram.Web;

	trivia = none

	// §2.
	Ws = [' ' | '\t' | '\n' | '\r']*

	JsonText : @JsonValue = Ws & value: Value & Ws => @(value)

	// §3.
	Value : @JsonValue
		= "false"             => @(JsonValue.Boolean.False)
		| "null"              => @(JsonValue.Null.Instance)
		| "true"              => @(JsonValue.Boolean.True)
		| members: ObjectBody => @(members)
		| items: ArrayBody    => @(items)
		| text: NumberText    => @(new JsonValue.Number(text))
		| text: StringText    => @(new JsonValue.String(Rfc8259.Unescaped(text)))

	// §4. begin-object, members, end-object: whitespace after each structural character and after each value.
	ObjectBody : @JsonValue = '{' & Ws & members: Members & '}' => @(new JsonValue.Object(members))

	Members : @KeyValuePair<string, JsonValue>[] = (first: Member & rest: NextMember*)? => @(Rfc8259.Joined(first, rest))

	Member : @KeyValuePair<string, JsonValue> = name: StringText & Ws & ':' & Ws & value: Value & Ws => @(new KeyValuePair<string, JsonValue>(Rfc8259.Unescaped(name), value))

	NextMember : @KeyValuePair<string, JsonValue> = ',' & Ws & member: Member => @(member)

	// §5.
	ArrayBody : @JsonValue = '[' & Ws & items: Items & ']' => @(new JsonValue.Array(items))

	Items : @JsonValue[] = (first: Element & rest: NextElement*)? => @(Rfc8259.Joined(first, rest))

	Element : @JsonValue = value: Value & Ws => @(value)

	NextElement : @JsonValue = ',' & Ws & element: Element => @(element)

	// §6. No leading zeros, a fraction of one digit at least, and an exponent likewise.
	NumberText = '-'? & ('0' | ['1'..'9'] & ['0'..'9']*) & ('.' & ['0'..'9']+)? & (['e' | 'E'] & ['+' | '-']? & ['0'..'9']+)?

	// §7. Any character but a quotation mark, a reverse solidus and a control, and the nine escapes.
	StringText = '"' & ([' '..'!' | '#'..'[' | ']'..'\uFFFF'] | '\\' & (['"' | '\\' | '/' | 'b' | 'f' | 'n' | 'r' | 't'] | 'u' & Hexdig{4}))* & '"'

	Hexdig = ['0'..'9' | 'a'..'f' | 'A'..'F']

	parse JsonText as ParseJson
	""")]
static partial class Rfc8259
{
	// ParseJson and TryParseJson are generated here; JsonValue.Parse is the way in.

	internal static JsonValue[] Joined(JsonValue? first, JsonValue[]? rest)
	{
		if (first is null)
			return [];

		var all = new JsonValue[1 + (rest?.Length ?? 0)];

		all[0] = first;
		rest?.CopyTo(all, 1);

		return all;
	}

	internal static KeyValuePair<string, JsonValue>[] Joined(KeyValuePair<string, JsonValue>? first, KeyValuePair<string, JsonValue>[]? rest)
	{
		if (first is not { } one)
			return [];

		var all = new KeyValuePair<string, JsonValue>[1 + (rest?.Length ?? 0)];

		all[0] = one;
		rest?.CopyTo(all, 1);

		return all;
	}

	/// <summary>A string token's text between its quotation marks, with §7's escapes undone.</summary>
	internal static string Unescaped(string token)
	{
		if (token.IndexOf('\\') < 0)
			return token.Substring(1, token.Length - 2);

		var output = new StringBuilder(token.Length);

		for (var at = 1; at < token.Length - 1; at++)
		{
			var character = token[at];

			if (character != '\\')
			{
				output.Append(character);
				continue;
			}

			var escaped = token[++at];

			switch (escaped)
			{
				case 'b': output.Append('\b'); break;
				case 'f': output.Append('\f'); break;
				case 'n': output.Append('\n'); break;
				case 'r': output.Append('\r'); break;
				case 't': output.Append('\t'); break;

				case 'u':
					output.Append((char)int.Parse(token.Substring(at + 1, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
					at += 4;
					break;

				// `"`, `\` and `/` stand for themselves.
				default:
					output.Append(escaped);
					break;
			}
		}

		return output.ToString();
	}
}
