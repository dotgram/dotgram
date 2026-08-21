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
/// <b>The numbers below do not carry into a parser, and it is not known why.</b> Generating
/// the window above the threshold they suggest was tried and measured in place: the URL
/// grammar went from 605 ns to 692 on its path-heavy input, 14% the wrong way, and was
/// reverted.
/// </para>
/// <para>
/// Two explanations were offered and both are measured wrong, below, under
/// <c>Run_</c>. That a parser reads a run of members and the chain settles on its first
/// comparison every time: it does, and the window still beats it, 19.7 against 24.6. That
/// the window as generated subtracted the low character three times: it did, and doing it
/// once instead changes nothing, 20.0 against 19.7.
/// </para>
/// <para>
/// That last one was first read off equal time, which does not say it: two more of an
/// operation that issues four to the cycle cost nothing in a loop already doing a load, a
/// shift and a branch, removed or not. <c>[DisassemblyDiagnoser]</c> settles it — both come
/// to eighty-four bytes of machine code, the same number, which two different sources reach
/// only by being compiled to the same thing. The chain is a hundred and five.
/// </para>
/// <para>
/// A thirty-two bit window was tried against a sixty-four bit one over a set that fits
/// either, in case a wide constant were what a method short of registers cannot afford.
/// Sixty-six bytes both, and level in time. It is not the width.
/// </para>
/// <para>
/// So the cost is somewhere the shape of the test does not reach: the emitted method is one
/// of several thousand states, and sixty-four-bit constants have to be materialized into
/// registers in a method that is already short of them. That is a guess and has not been
/// measured, which is exactly what the last two guesses were.
/// </para>
/// <para>
/// Measured before anything is generated differently, because the chain may already be
/// free: <c>c >= 'a' &amp;&amp; c &lt;= 'z'</c> is a pattern RyuJIT recognizes and folds
/// into the same single unsigned comparison the second form writes by hand. If it does,
/// the second form is worth nothing and only the third is worth anything.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2, printSource: false)]
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

	// ── The two readings of why it lost in a parser ─────────────────────────────
	//
	// Over a run of members, which is what a parser reads: a path segment is letters one
	// after another, so the chain settles on its first comparison every time. Against that,
	// the window as it was actually generated — subtracting the low character three times,
	// once for the bound, once to pick the word and once to shift — and the window as it
	// should have been, subtracting once into a local. Whichever of those two is the cost,
	// this says so.

	static readonly string Members = new('x', 51);

	static bool Masked_thrice(char c) =>
		(uint)(c - Low) < Span && ((c - Low < 64 ? Word0 : Word1) >> (c - Low) & 1UL) != 0;

	static bool Masked_once(char c)
	{
		var n = (uint)(c - Low);

		return n < Span && ((n < 64 ? Word0 : Word1) >> (int)n & 1UL) != 0;
	}

	[Benchmark]
	public int Run_chained()
	{
		var found = 0;

		foreach (var c in Members)
			if (Chained(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Run_masked_subtracting_thrice()
	{
		var found = 0;

		foreach (var c in Members)
			if (Masked_thrice(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Run_masked_subtracting_once()
	{
		var found = 0;

		foreach (var c in Members)
			if (Masked_once(c))
				found++;

		return found;
	}

	// ── Sixty-four bits against thirty-two ──────────────────────────────────────
	//
	// SubDelim from the URL grammar: eleven marks between '!' and '=', twenty-nine places,
	// which is a window that fits either width. If a `ulong` constant is what a method short
	// of registers cannot afford, a `uint` one over the same set will say so.

	const int SubLow = '!', SubSpan = '=' - '!' + 1;

	const ulong Wide   = 0x14000FE9UL;
	const uint  Narrow32 = 0x14000FE9u;

	static readonly string SubMembers = new('&', 51);

	static bool Sub_in_64(char c)
	{
		var n = (uint)(c - SubLow);

		return n < SubSpan && (Wide >> (int)n & 1UL) != 0;
	}

	static bool Sub_in_32(char c)
	{
		var n = (uint)(c - SubLow);

		return n < SubSpan && (Narrow32 >> (int)n & 1u) != 0;
	}

	[Benchmark]
	public int Window_of_64_bits()
	{
		var found = 0;

		foreach (var c in SubMembers)
			if (Sub_in_64(c))
				found++;

		return found;
	}

	[Benchmark]
	public int Window_of_32_bits()
	{
		var found = 0;

		foreach (var c in SubMembers)
			if (Sub_in_32(c))
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
