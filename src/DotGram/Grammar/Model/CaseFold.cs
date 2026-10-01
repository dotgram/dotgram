using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Model;

/// <summary>
/// What an ignore-case literal (<c>'x'i</c>, <c>"text"i</c>) matches, character by character.
/// </summary>
/// <remarks>
/// <para>
/// Each character matches itself and its simple upper and lower case (<see cref="Upper"/>,
/// <see cref="Lower"/>), and a partner is taken only on the character's own side of ASCII. So <c>'s'i</c> is <c>s</c> or <c>S</c> and
/// never U+017F, whose upper case is <c>S</c>; <c>'k'i</c> is never the Kelvin sign, whose lower
/// case is <c>k</c>; <c>"привет"i</c> reads <c>ПРИВЕТ</c>; and <c>'μ'i</c> reads <c>Μ</c> but not
/// the micro sign U+00B5, which is a character of its own that only upper-cases to <c>Μ</c>.
/// The relation is the literal's, not a symmetric one: <c>'ς'i</c> reads <c>Σ</c>, and
/// <c>'Σ'i</c> reads <c>σ</c> but not <c>ς</c>.
/// </para>
/// <para>
/// Every place that decides what such a literal matches asks here — the first sets, the
/// lexical automaton, the overlap analysis and the comparisons the emitter writes — so that
/// none of them can accept what another refuses.
/// </para>
/// <para>
/// The case mappings are the generator's own, written from the Unicode Character Database at
/// <see cref="UnicodeVersion"/> into <c>CaseFold.Table.cs</c> by <c>CaseFold.generate.cs</c>,
/// and never the runtime's: <see cref="char.ToUpperInvariant"/> answers from the ICU a machine
/// has, from the runtime's own tables in invariant globalization mode, and from NLS under .NET
/// Framework, where an editor may host the generator — so the same grammar would generate a
/// different parser on another build machine. The generated code tests the characters this
/// names rather than folding at run time, so the parser does not depend on the casing tables
/// of the runtime it is run on either.
/// </para>
/// </remarks>
public static partial class CaseFold
{
	/// <summary>
	/// The characters an ignore-case <paramref name="value"/> matches, ascending and distinct;
	/// <paramref name="value"/> alone where it has no case partner.
	/// </summary>
	public static IReadOnlyList<char> Of(char value)
	{
		if (value <= 0x7F)
		{
			var lower = (char)(value | 0x20);

			return lower is >= 'a' and <= 'z' ? [(char)(lower & ~0x20), lower] : [value];
		}

		var all = new List<char>(3) { value };

		Add(all, Upper(value));
		Add(all, Lower(value));

		all.Sort();

		return all;
	}

	/// <summary>
	/// The ASCII letter, in lower case, whose two cases are what an ignore-case
	/// <paramref name="value"/> matches; null where <paramref name="value"/> is not one.
	/// </summary>
	public static char? AsciiLetter(char value)
	{
		var lower = (char)(value | 0x20);

		return value <= 0x7F && lower is >= 'a' and <= 'z' ? lower : null;
	}

	/// <summary>Whether <paramref name="input"/> is read by an ignore-case <paramref name="value"/>.</summary>
	public static bool Matches(char value, char input)
	{
		if (input == value)
			return true;

		foreach (var one in Of(value))
			if (one == input)
				return true;

		return false;
	}

	/// <summary>Whether <paramref name="input"/> is read by the ignore-case <paramref name="literal"/>.</summary>
	public static bool Matches(string literal, ReadOnlySpan<char> input)
	{
		if (input.Length != literal.Length)
			return false;

		for (var i = 0; i < literal.Length; i++)
			if (!Matches(literal[i], input[i]))
				return false;

		return true;
	}

	/// <summary>
	/// <see cref="Of"/> as ranges of one character each.
	/// </summary>
	public static IReadOnlyList<CharRange> Ranges(char value)
	{
		var all    = Of(value);
		var ranges = new CharRange[all.Count];

		for (var i = 0; i < all.Count; i++)
			ranges[i] = new CharRange(all[i], all[i]);

		return ranges;
	}

	/// <summary>
	/// Every spelling the ignore-case <paramref name="literal"/> reads, or null where there are
	/// more than <paramref name="limit"/>.
	/// </summary>
	public static HashSet<string>? Spellings(string literal, int limit)
	{
		var count = 1L;

		foreach (var one in literal)
		{
			count *= Of(one).Count;

			if (count > limit)
				return null;
		}

		var all = new HashSet<string>(StringComparer.Ordinal) { "" };

		foreach (var one in literal)
		{
			var cases = Of(one);
			var next  = new HashSet<string>(StringComparer.Ordinal);

			foreach (var head in all)
				foreach (var each in cases)
					next.Add(head + each);

			all = next;
		}

		return all;
	}

	/// <summary>
	/// The simple upper case of <paramref name="value"/> at <see cref="UnicodeVersion"/>, or
	/// <paramref name="value"/> where it has none in the Basic Multilingual Plane.
	/// </summary>
	public static char Upper(char value)
	{
		return Mapped(Uppers, value);
	}

	/// <summary>
	/// The simple lower case of <paramref name="value"/> at <see cref="UnicodeVersion"/>, or
	/// <paramref name="value"/> where it has none in the Basic Multilingual Plane.
	/// </summary>
	public static char Lower(char value)
	{
		return Mapped(Lowers, value);
	}

	/// <summary>A character through one of the tables: the run it falls in, if any, by bisection.</summary>
	static char Mapped(int[] runs, char value)
	{
		var low  = 0;
		var high = runs.Length / 4 - 1;

		while (low <= high)
		{
			var middle = (low + high) / 2;
			var at     = middle * 4;

			if (value < runs[at])
				high = middle - 1;
			else if (value > runs[at + 1])
				low = middle + 1;
			else
				return (value - runs[at]) % runs[at + 2] == 0 ? (char)(value + runs[at + 3]) : value;
		}

		return value;
	}

	/// <summary>A partner of a character beyond ASCII, unless it is an ASCII one.</summary>
	static void Add(List<char> all, char partner)
	{
		if (partner > 0x7F && !all.Contains(partner))
			all.Add(partner);
	}
}
