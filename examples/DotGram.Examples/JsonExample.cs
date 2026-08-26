using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram;

namespace DotGram.Examples;

// JSON, because it is what everybody writes first and because it asks for one thing the
// other examples here do not: a value that is any of six things, nested inside itself.
//
//     JsonParser.Read("""{"a": [1, true, null]}""")
//
// The grammar is RFC 8259 with nothing left out except numbers in exponent form, which
// would add a line and say nothing new. What it produces is ordinary C# data — a record
// per kind of value — so a caller reads it with patterns and nothing here has to invent
// a visitor.
//
// Two things in it are worth looking at rather than copying:
//
//   * The comma-separated list is written once, as `List(item, sep)` — §4.2's own
//     example — and specialized per call site. `List(Member, ',')` and `List(Value,
//     ',')` become two separate recognizers with no delegate, no interface and nothing
//     virtual between them, which is what "specialization" buys over a generic list.
//
//   * `trivia` is whitespace and is declared once, at the top, so no rule below it
//     mentions spacing at all (§4.5). The lexical namespace turns it off for the runs
//     where a space would be a lie: inside a string and between the digits of a number.

[Gram("""
	@using DotGram.Examples;
	@using System.Globalization;

	using Lexical;

	namespace Lexical
	{
		// No trivia in here: "a b" is a string with a space in it, and 1 2 is two
		// numbers rather than one.
		trivia = none

		Digits   = ['0'..'9']+
		Fraction = '.' & Digits
		Escape   = '\\' & ['"' | '\\' | '/' | 'b' | 'f' | 'n' | 'r' | 't']
		Plain    = [^ '"' | '\\']
		Body     = (Plain | Escape)*
	}

	trivia = [' ' | '\t' | '\r' | '\n']*

	// §4.2: written once, specialized per call site. Spaced on every turn without
	// saying so, because what it repeats is a sequence (§4.5).
	List(item, sep) : item[] = item & (sep & item)*

	Json : @JsonValue = value: Value & eof => @(value)

	Value : @JsonValue = value: Object  => @(value)
	                   | value: Array   => @(value)
	                   | text:  Text    => @(text)
	                   | value: Number  => @(value)
	                   | "true"         => @(new JsonBool(true))
	                   | "false"        => @(new JsonBool(false))
	                   | "null"         => @(new JsonNull())

	Object : @JsonValue = '{' & members: List(Member, ',') & '}' => @(new JsonObject(members))
	                    | '{' & '}'                              => @(new JsonObject([]))

	Array  : @JsonValue = '[' & items: List(Value, ',') & ']'    => @(new JsonArray(items))
	                    | '[' & ']'                              => @(new JsonArray([]))

	Member : @JsonMember = name: Text & ':' & value: Value => @(new JsonMember(name.Text, value))

	Text   : @JsonText = '"' & body: Lexical.Body & '"' => @(new JsonText(body))

	Number : @JsonValue = digits: Lexical.Digits & fraction: Lexical.Fraction
	                        => @(new JsonNumber(double.Parse(digits + fraction, CultureInfo.InvariantCulture)))
	                    | digits: Lexical.Digits
	                        => @(new JsonNumber(double.Parse(digits, CultureInfo.InvariantCulture)))

	parse Json
	""")]
public sealed partial class JsonParser
{
	/// <summary>Reads one JSON document, or throws where it is not one.</summary>
	public static JsonValue Read(string text) => ParseJson(text);
}

/// <summary>One JSON value, which is one of six things.</summary>
public abstract record JsonValue;

public sealed record JsonNull                                  : JsonValue;
public sealed record JsonBool  (bool Value)                    : JsonValue;
public sealed record JsonNumber(double Value)                  : JsonValue;
public sealed record JsonText  (string Text)                   : JsonValue;
public sealed record JsonArray (IReadOnlyList<JsonValue> Items) : JsonValue;

public sealed record JsonObject(IReadOnlyList<JsonMember> Members) : JsonValue
{
	/// <summary>The member of this name, or null. Objects are small; a scan is enough.</summary>
	public JsonValue? this[string name] =>
		Members.FirstOrDefault(member => member.Name == name)?.Value;
}

public sealed record JsonMember(string Name, JsonValue Value);
