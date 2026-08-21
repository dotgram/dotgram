using System;

using BenchmarkDotNet.Attributes;

namespace DotGram.Benchmarks;

/// <summary>
/// Three ways to ask whether a character belongs to a set.
/// </summary>
/// <remarks>
/// <para>
/// The set is <c>Unreserved</c> from the URL grammar — digits, both cases of the alphabet,
/// and four marks — which is what a generated parser asks about most often and what the
/// emitter writes today as a chain of range comparisons. The other two are what it could
/// write: each range as one unsigned comparison after shifting the character down, and the
/// whole set as bits in a window, again after shifting down.
/// </para>
/// <para>
/// Measured before anything is generated differently, because the chain may already be
/// free: <c>c >= 'a' &amp;&amp; c &lt;= 'z'</c> is a pattern RyuJIT recognizes and folds
/// into the same single unsigned comparison the second form writes by hand. If it does,
/// the second form is worth nothing and only the third is worth anything.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class Membership
{
	// The set spans '-' (45) to '~' (126): eighty-two places, so two words of bits, and the
	// character is shifted down to the first of them before either is looked at.
	const int Low = '-', Span = '~' - '-' + 1;

	static readonly ulong Word0, Word1;

	static Membership()
	{
		for (var c = 'a'; c <= 'z'; c++)
			One |= 1UL << (c - 'a');

		for (var c = '0'; c <= 'Z'; c++)
			if (c <= '9' || c >= 'A')
				Narrow |= 1UL << (c - '0');

		for (var c = '0'; c <= 'z'; c++)
		{
			var bit = c - '0';

			if (Two_chained(c))
			{
				if (bit < 64) Two0 |= 1UL << bit; else Two1 |= 1UL << (bit - 64);
			}

			if (Three_chained(c))
			{
				if (bit < 64) Three0 |= 1UL << bit; else Three1 |= 1UL << (bit - 64);
			}
		}

		for (var c = Low; c <= '~'; c++)
		{
			if (!Chained((char)c))
				continue;

			var bit = c - Low;

			if (bit < 64)
				Word0 |= 1UL << bit;
			else
				Word1 |= 1UL << (bit - 64);
		}
	}

	/// <summary>What the emitter writes today.</summary>
	static bool Chained(char c) =>
		(c >= '-' && c <= '.') || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') ||
		c == '_' || (c >= 'a' && c <= 'z') || c == '~';

	/// <summary>Each range as one unsigned comparison.</summary>
	static bool Shifted(char c) =>
		(uint)(c - '-') <= 1 || (uint)(c - '0') <= 9 || (uint)(c - 'A') <= 25 ||
		c == '_' || (uint)(c - 'a') <= 25 || c == '~';

	/// <summary>The whole set as bits in a window.</summary>
	static bool Masked(char c)
	{
		var bit = (uint)(c - Low);

		return bit < Span && ((bit < 64 ? Word0 : Word1) >> (int)bit & 1UL) != 0;
	}

	// A realistic mixture: mostly members, with the separators a URL actually contains, so
	// the branches are as predictable as they are in a parse and no more.
	static readonly string Input =
		string.Concat(new string('x', 4), "://", new string('y', 11), "/", new string('z', 8),
			"?a=1&b=2#top", new string('w', 12), " \t\n<>\"{}|^`");

	// ── Where it stops paying ───────────────────────────────────────────────────
	//
	// The same two shapes over smaller sets. One range is a single unsigned comparison once
	// the character is shifted down, and no window can beat that; the question is where
	// between one and six the two cross.

	static readonly ulong One, Two0, Two1, Three0, Three1, Narrow;

	// A set narrow enough for one word — digits and capitals span forty-three places — so
	// that the two ways of asking whether the character is inside it can be compared. The
	// bit form is only available because sixty-four is a power of two: it asks whether any
	// bit above the window is set, which is the same question as being under it.
	static bool Narrow_by_compare(char c)
	{
		var bit = (uint)(c - '0');

		return bit < 64 && (Narrow >> (int)bit & 1UL) != 0;
	}

	static bool Narrow_by_bits(char c)
	{
		var bit = (uint)(c - '0');

		return (bit & ~63u) == 0 && (Narrow >> (int)bit & 1UL) != 0;
	}

	static bool One_chained(char c)   => c >= 'a' && c <= 'z';
	static bool Two_chained(char c)   => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z');
	static bool Three_chained(char c) =>
		(c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');

	static bool One_masked(char c)
	{
		var bit = (uint)(c - 'a');

		return bit < 26 && (One >> (int)bit & 1UL) != 0;
	}

	static bool Two_masked(char c)
	{
		var bit = (uint)(c - '0');

		return bit < 75 && ((bit < 64 ? Two0 : Two1) >> (int)bit & 1UL) != 0;
	}

	static bool Three_masked(char c)
	{
		var bit = (uint)(c - '0');

		return bit < 75 && ((bit < 64 ? Three0 : Three1) >> (int)bit & 1UL) != 0;
	}

	// Written out one by one rather than handed to a counter as delegates. Measured that
	// way first, and the indirection cost more than the difference being looked for — every
	// one of them came out at twice the six-term pair beside them, which is the harness
	// speaking rather than the shapes.

	[Benchmark]
	public int One_range_chained()
	{
		var found = 0;

		foreach (var c in Input)
			if (One_chained(c))
				found++;

		return found;
	}

	[Benchmark]
	public int One_range_masked()
	{
		var found = 0;

		foreach (var c in Input)
			if (One_masked(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Two_ranges_chained()
	{
		var found = 0;

		foreach (var c in Input)
			if (Two_chained(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Two_ranges_masked()
	{
		var found = 0;

		foreach (var c in Input)
			if (Two_masked(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Three_ranges_chained()
	{
		var found = 0;

		foreach (var c in Input)
			if (Three_chained(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Three_ranges_masked()
	{
		var found = 0;

		foreach (var c in Input)
			if (Three_masked(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Window_bounded_by_compare()
	{
		var found = 0;

		foreach (var c in Input)
			if (Narrow_by_compare(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Window_bounded_by_bits()
	{
		var found = 0;

		foreach (var c in Input)
			if (Narrow_by_bits(c))
				found++;

		return found;
	}

	[Benchmark(Baseline = true)]
	public int Ranges_chained()
	{
		var found = 0;

		foreach (var c in Input)
			if (Chained(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Ranges_shifted_down()
	{
		var found = 0;

		foreach (var c in Input)
			if (Shifted(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Bits_in_a_window()
	{
		var found = 0;

		foreach (var c in Input)
			if (Masked(c))
				found++;

		return found;
	}
}
