using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Examples.Feeds;
using DotGram.Handwritten.Feeds;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <see cref="HandStockCount"/> held to the grammar of <c>StockCountExample</c>: the same
/// counts, the same unreadable lines, refused at the same place, from a string and from a
/// reader through buffers small enough that every line crosses one (D1).
/// </summary>
/// <remarks>
/// The inputs are the ones the recovering repetition has to get right: a closing line that
/// begins like an item, a broken line before it, a broken last item that runs into it, and a
/// closing line written twice. Each is also taken apart one character at a time.
/// </remarks>
public sealed class StockCountHandTests
{
	public static readonly string[] Inputs =
	[
		"apples: 12\npears: 7\nEND 2\n",
		"apples: 12\na 1\npears: 7\nEND 2\n",
		"apples: 12\na 1\nEND 1",
		"apples: 12\na: 1END 3\n",
		"END 0\n",
		"apples: 12\npears: 7\n",
		"END 3\nEND 3\n",
		"",
		"END 3\nx",
		"apples: 12\r\npears: 7\rEND 1\r\n",
		"\n\nEND 0",
		"apples: 12\nEND 1\n\n",
	];

	public static TheoryData<string> Texts => [.. Inputs];

	[Theory]
	[MemberData(nameof(Texts))]
	public void Every_count_is_answered_alike(string text)
	{
		Agree(text);
	}

	[Fact]
	public void And_alike_on_every_text_one_character_short_of_one()
	{
		var told = new List<string>();

		foreach (var input in Inputs)
			foreach (var text in Mutations(input))
				try
				{
					Agree(text);
				}
				catch (Exception disagreed) when (disagreed is not OutOfMemoryException)
				{
					told.Add(disagreed.Message);
				}

		Assert.True(told.Count == 0, $"{told.Count} texts told the two apart; the first:\n\n" + string.Join("\n\n", told.Take(8)));
	}

	/// <summary>The generated parser in every form it has, and the hand one, answering alike.</summary>
	static void Agree(string text)
	{
		var expected = Describe(StockCountReader.TryParseCount(text));

		foreach (var (form, answer) in new[]
		{
			("generated, reader",            Describe(StockCountReader.TryParseCount(new StringReader(text)))),
			("generated, reader, buffer 64", Describe(StockCountReader.TryParseCount(new StringReader(text), 64))),
			("generated, reader, buffer 1",  Describe(StockCountReader.TryParseCount(new StringReader(text), 1))),
			("hand",                         Describe(HandStockCount.TryRead(text, out var one, out var at), one, at)),
			("hand, reader, buffer 64",      Describe(HandStockCount.TryRead(new StringReader(text), out var two, out var there, 64), two, there)),
			("hand, reader, buffer 1",       Describe(HandStockCount.TryRead(new StringReader(text), out var three, out var where, 1), three, where)),
		})
			if (answer != expected)
				Assert.Fail($"\"{text.Replace("\n", "\\n").Replace("\r", "\\r")}\":\ngenerated: {expected}\n{form}: {answer}");
	}

	static string Describe(StockCountReader.Match<StockCount> match)
	{
		return match.IsSuccess ? Describe(match.Value) : "refused at " + match.Position;
	}

	static string Describe(bool read, StockCount? count, long failure)
	{
		return read ? Describe(count!) : "refused at " + failure;
	}

	static string Describe(StockCount count)
	{
		return $"total {count.Total}: " + string.Join(", ", count.Lines);
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
