using System;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// FIX 5.0's MultipleStringValue: values separated by single spaces, each a word of one character or
/// more, where MultipleCharValue's are one character each. Every case through characters and octets,
/// and through both forms, the pair and the <c>out</c>.
/// </summary>
public sealed class FixMultipleStringTests
{
	[Theory]
	[InlineData("A", new[] { "A" })]
	[InlineData("AA AB", new[] { "AA", "AB" })]
	[InlineData("E.W FOK IO", new[] { "E.W", "FOK", "IO" })]
	public void Words_separated_by_single_spaces_are_read(string text, string[] expected)
	{
		foreach (var (valid, value) in Both(text))
		{
			Assert.True(valid, text);
			Assert.Equal(expected, value);
		}
	}

	[Theory]
	[InlineData("")]         // no value at all
	[InlineData("AA  AB")]   // two spaces, an empty word between them
	[InlineData(" AA")]
	[InlineData("AA ")]
	public void An_empty_word_is_not_valid(string text)
	{
		foreach (var (valid, _) in Both(text))
			Assert.False(valid, text);
	}

	[Fact]
	public void A_word_is_what_MultipleCharValue_refuses()
	{
		// The same text, read as each type: the character form holds each value to one character.
		Assert.False("AA AB".AsSpan().ToMultiple().Valid);
		Assert.True("AA AB".AsSpan().ToMultipleString().Valid);
	}

	static (bool Valid, string[] Value)[] Both(string text)
	{
		var octets = Encoding.Latin1.GetBytes(text);
		var fromChars = text.AsSpan().ToMultipleString(out var chars);
		var fromBytes = ((ReadOnlySpan<byte>)octets).ToMultipleString(out var bytes);

		return
		[
			text.AsSpan().ToMultipleString(),
			((ReadOnlySpan<byte>)octets).ToMultipleString(),
			(fromChars, chars),
			(fromBytes, bytes),
		];
	}
}
