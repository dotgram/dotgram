using System;
using System.Collections.Generic;
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

	/// <summary>§4: members in the order written, names as unescaped strings, duplicates kept.</summary>
	public sealed record Object(IReadOnlyList<KeyValuePair<string, JsonValue>> Members) : JsonValue;

	/// <summary>§5.</summary>
	public sealed record Array(IReadOnlyList<JsonValue> Items) : JsonValue;

	/// <summary>§7: the string with its escapes undone. An escaped lone surrogate stays one (§8.2).</summary>
	public sealed record String(string Value) : JsonValue;

	/// <summary>§6: the number as written, which <see cref="ToDouble"/> and the rest read at a precision of the caller's choice.</summary>
	public sealed record Number(string Text) : JsonValue
	{
		/// <summary>The nearest double, as IEEE 754 binary64 reads it; an exponent past its range is infinity.</summary>
		public double ToDouble() => double.Parse(Text, NumberStyles.Float, CultureInfo.InvariantCulture);

		/// <summary>The number as a decimal, where one holds it exactly enough and it is in range.</summary>
		public bool TryToDecimal(out decimal value) =>
			decimal.TryParse(Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

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
		var output = new StringBuilder();

		Write(output, this);

		return output.ToString();
	}

	static void Write(StringBuilder output, JsonValue value)
	{
		switch (value)
		{
			case Object members:
				output.Append('{');

				for (var index = 0; index < members.Members.Count; index++)
				{
					if (index > 0)
						output.Append(',');

					Quoted(output, members.Members[index].Key);
					output.Append(':');
					Write(output, members.Members[index].Value);
				}

				output.Append('}');
				break;

			case Array items:
				output.Append('[');

				for (var index = 0; index < items.Items.Count; index++)
				{
					if (index > 0)
						output.Append(',');

					Write(output, items.Items[index]);
				}

				output.Append(']');
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
public static partial class Rfc8259
{
	// ParseJson and TryParseJson are generated here.

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
