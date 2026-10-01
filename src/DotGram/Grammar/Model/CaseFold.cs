using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Model;

/// <summary>
/// What an ignore-case literal (<c>'x'i</c>, <c>"text"i</c>) matches, character by character.
/// </summary>
/// <remarks>
/// <para>
/// Each character matches itself and its simple upper and lower case
/// (<see cref="char.ToUpperInvariant"/>, <see cref="char.ToLowerInvariant"/>), and a partner is
/// taken only on the character's own side of ASCII. So <c>'s'i</c> is <c>s</c> or <c>S</c> and
/// never U+017F, whose upper case is <c>S</c>; <c>'k'i</c> is never the Kelvin sign, whose lower
/// case is <c>k</c>; <c>"привет"i</c> reads <c>ПРИВЕТ</c>; and <c>'μ'i</c> reads <c>Μ</c> but not
/// the micro sign U+00B5, which is a character of its own that only upper-cases to <c>Μ</c>.
/// The relation is the literal's, not a symmetric one: <c>'ς'i</c> reads <c>Σ</c>, and
/// <c>'Σ'i</c> reads <c>σ</c> but not <c>ς</c>.
/// </para>
/// <para>
/// Every place that decides what such a literal matches asks here — the first sets, the
/// lexical automaton, the overlap analysis and the comparisons the emitter writes — so that
/// none of them can accept what another refuses. ASCII is answered without the casing tables,
/// which is what keeps every keyword of every shipped grammar the same on any runtime that
/// hosts the generator; and the generated code tests the characters this names rather than
/// folding at run time, so the parser does not depend on the casing tables of the runtime it
/// is run on either.
/// </para>
/// </remarks>
public static class CaseFold
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

		Add(all, char.ToUpperInvariant(value));
		Add(all, char.ToLowerInvariant(value));

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

	/// <summary>A partner of a character beyond ASCII, unless it is an ASCII one.</summary>
	static void Add(List<char> all, char partner)
	{
		if (partner > 0x7F && !all.Contains(partner))
			all.Add(partner);
	}
}
