using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Handwritten.Web;
using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// <see cref="DotGram.Handwritten.Web.HandJson"/> held to the generated RFC 8259 parser: the
/// same values, the same refusals, refused at the same place (D1).
/// </summary>
/// <remarks>
/// <see cref="Rfc8259Tests"/> reads JSONTestSuite through <see cref="Both"/> already, and the
/// JSON Pointer and Patch tests read their documents through it too; this adds texts that
/// touch every production вЂ” literals, the number's parts, every escape, nesting, whitespace in
/// each place it may stand вЂ” each taken apart one character at a time.
/// </remarks>
public sealed class HandJsonTests
{
	public static readonly string[] Corpus =
	[
		"{ \"a\": [1, -2.5e+3, 0, -0.0, 1E9, true, false, null], \"b\": { \"c\": \"d\" }, \"a\": {} }",
		" [ ] ",
		"{}",
		"[[[]]]",
		"{\"\":\"\"}",
		"0",
		"-1",
		"12.34",
		"1e-7",
		"-0e0",
		"true",
		"false",
		"null",
		"\"plain\"",
		"\"esc \\\" \\\\ \\/ \\b \\f \\n \\r \\t\"",
		"\"\\u00e9\\uD83D\\uDE00\\uDEAD\\U00E9\"",
		"\"\u00E9\U0001F600 raw\"",
		"\t\r\n [1 , 2 ]\n",
		"{\"x\" : 1 , \"y\" : [ \"z\" ]}",
		"[1,2,]",
		"[01]",
		"[1.]",
		"[1e]",
		"[1e+]",
		"[.5]",
		"[+1]",
		"[-]",
		"\"\\x\"",
		"\"\\u12G4\"",
		"\"tab\there\"",
		"{\"a\" 1}",
		"{\"a\":1,}",
		"{1:2}",
		"[tru]",
		"[nul]",
		"fals",
		"[1] 2",
		"\uFEFF[]",
		"[\"unterminated",
		"{\"a\":[{\"b\":[{\"c\":null}]}]}",
	];

	public static TheoryData<string> Texts => [.. Corpus];

	[Theory]
	[MemberData(nameof(Texts))]
	public void Every_text_is_answered_alike(string text)
	{
		Both.AgreeOnJson(text);
	}

	[Fact]
	public void And_alike_on_every_text_one_character_short_of_one()
	{
		var told = new List<string>();

		foreach (var json in Corpus)
			foreach (var text in Mutations(json))
				try
				{
					Both.AgreeOnJson(text);
				}
				catch (Exception disagreed) when (disagreed is not OutOfMemoryException)
				{
					told.Add(disagreed.Message);
				}

		Assert.True(told.Count == 0, $"{told.Count} texts told the two apart; the first:\n\n" + string.Join("\n\n", told.Take(8)));
	}

	/// <summary>
	/// Nested deeper than a thread's stack goes. JsonValue.ToString recurses, and so cannot say
	/// what was read at this depth; the two values are walked instead, level by level.
	/// </summary>
	[Fact]
	public void And_alike_nested_deeper_than_any_stack()
	{
		const int depth = 100_000;

		var text      = new string('[', depth) + "1" + new string(']', depth);
		var generated = Rfc8259.TryParseJson(text);

		Assert.True(generated.IsSuccess);
		Assert.True(HandJson.TryParse(text, out var hand, out _));
		Assert.Equal(depth, Depth(generated.Value));
		Assert.Equal(depth, Depth(hand!));

		var open = Rfc8259.TryParseJson(new string('[', depth));

		Assert.False(HandJson.TryParse(new string('[', depth), out _, out var failure));
		Assert.Equal(open.Position, failure);
	}

	static int Depth(JsonValue value)
	{
		var depth = 0;

		while (value is JsonValue.Array { Items: [var inner] })
		{
			depth++;
			value = inner;
		}

		return value is JsonValue.Number { Text: "1" } ? depth : -1;
	}

	/// <summary>A text cut short at every character, and with every character taken out once.</summary>
	static IEnumerable<string> Mutations(string text)
	{
		var seen = new HashSet<string>(StringComparer.Ordinal) { text };

		for (var length = 0; length < text.Length; length++)
			if (seen.Add(text.Substring(0, length)))
				yield return text.Substring(0, length);

		for (var at = 0; at < text.Length; at++)
			if (seen.Add(text.Remove(at, 1)))
				yield return text.Remove(at, 1);
	}
}
