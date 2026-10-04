using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Numbers written as the characters of a string literal: the spelling is what a lexer's table
/// is decoded from, so it is pinned here, and it reads back to the numbers it was made from.
/// </summary>
public sealed class SpelledNumbersTests
{
	[Fact]
	public void Each_number_plus_one_is_written_in_groups_of_five_bits_the_highest_first()
	{
		// -1 is '@'; 0 and 1 are 'A' and 'B'; 31 + 1 is the groups 1 and 0, "a" then "@"; 32 + 1 is 1 and 1.
		Assert.Equal("\"@ABa@aA\"", Spelled([-1, 0, 1, 31, 32], utf8: false));
	}

	[Fact]
	public void Only_letters_and_the_at_sign_are_written_as_themselves()
	{
		// 26 + 1 ends in the byte 0x5B, which is '[', and 30 + 1 in 0x5F, '_'; neither is written as punctuation.
		Assert.Equal("\"\\u005B\\u005F\"", Spelled([26, 30], utf8: false));
	}

	[Fact]
	public void A_UTF_8_literal_takes_the_suffix_and_the_same_characters()
	{
		Assert.Equal("\"@ABa@aA\"u8", Spelled([-1, 0, 1, 31, 32], utf8: true));
		Assert.Equal("\"\"u8", Spelled([], utf8: true));
	}

	[Fact]
	public void Every_number_reads_back_as_it_was_written()
	{
		var numbers = new List<int> { -1, 0, 1, 31, 32, 1023, 1024, 32767, 32768, 566_351, 1_048_575, int.MaxValue - 1 };

		for (var n = -1; n < 5000; n += 7)
			numbers.Add(n);

		var line = new StringBuilder();

		SpelledNumbers.Append(line, numbers, utf8: false);

		Assert.Equal(numbers, Decode(Unescape(line.ToString())));
	}

	static string Spelled(IEnumerable<int> numbers, bool utf8)
	{
		var line = new StringBuilder();

		SpelledNumbers.Append(line, numbers, utf8);

		return line.ToString();
	}

	/// <summary>The characters a literal holds, its escapes read.</summary>
	static string Unescape(string literal)
	{
		var text = new StringBuilder();

		for (var at = 1; at < literal.Length - 1; at++)
		{
			if (literal[at] == '\\')
			{
				text.Append((char)Convert.ToInt32(literal.Substring(at + 2, 4), 16));

				at += 5;
			}
			else
			{
				text.Append(literal[at]);
			}
		}

		return text.ToString();
	}

	/// <summary>The decoder the lexer's emitted class carries, over characters.</summary>
	static List<int> Decode(string spelled)
	{
		var decoded = new List<int>();
		var value   = 0;

		foreach (var one in spelled)
		{
			value = value << SpelledNumbers.Bits | one & ((1 << SpelledNumbers.Bits) - 1);

			if (one < SpelledNumbers.More)
			{
				decoded.Add(value - 1);

				value = 0;
			}
		}

		return decoded;
	}
}
