using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Whole numbers written as the characters of one string literal, for data that is large
/// and has to cost the C# compiler one token and not one constant per number.
/// </summary>
/// <remarks>
/// <para>
/// Each number plus one is written in groups of <see cref="Bits"/> bits, the highest first, one
/// byte to a group: a byte from <see cref="More"/> says more of the number follows, one from
/// <c>0x40</c> is its last. A number is at least <c>-1</c>, so what is written is never
/// negative, and as many groups are written as it takes. <c>-1</c>, the commonest, is one byte,
/// <c>'@'</c>; a number below a thousand is two.
/// </para>
/// <para>
/// Every byte is ASCII, so the UTF-8 of the literal is the bytes themselves, and the same
/// characters are the same bytes in an ordinary string, one char each. Letters and
/// <c>'@'</c> are written as themselves and everything else as <c>\u00XX</c>, never as
/// punctuation or a space: the size estimates read the emitted text
/// (<c>Machine.Branches</c>), and a <c>"||"</c> or a <c>"case "</c> spelled by the data would
/// be counted as code of the method above it.
/// </para>
/// </remarks>
public static class SpelledNumbers
{
	/// <summary>How many bits of a number one byte of its spelling carries.</summary>
	public const int Bits = 5;

	/// <summary>
	/// The bytes that say more of the number follows: <c>0x60</c> to <c>0x7F</c>. The last byte
	/// of a number is <c>0x40</c> to <c>0x5F</c>.
	/// </summary>
	public const int More = 0x60;

	/// <summary>
	/// Appends <paramref name="numbers"/> as one string literal, quotes included, followed by
	/// <c>u8</c> where <paramref name="utf8"/> says the literal is a UTF-8 one.
	/// </summary>
	public static void Append(StringBuilder line, IEnumerable<int> numbers, bool utf8)
	{
		var groups = new Stack<int>();

		line.Append('"');

		foreach (var number in numbers)
		{
			for (var value = (uint)(number + 1); groups.Count == 0 || value != 0; value >>= Bits)
				groups.Push((int)(value & ((1 << Bits) - 1)));

			while (groups.Count > 0)
			{
				var group = groups.Pop();

				Byte(line, group | (groups.Count > 0 ? More : 0x40));
			}
		}

		line.Append('"');

		if (utf8)
			line.Append("u8");
	}

	static void Byte(StringBuilder line, int value)
	{
		if (value is '@' or >= 'A' and <= 'Z' or >= 'a' and <= 'z')
			line.Append((char)value);
		else
			line.Append("\\u").Append(value.ToString("X4", CultureInfo.InvariantCulture));
	}
}
