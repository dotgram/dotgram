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

		return bit < Span && ((bit < 64 ? Word0 : Word1) >> (int)(bit & 63) & 1UL) != 0;
	}

	// A realistic mixture: mostly members, with the separators a URL actually contains, so
	// the branches are as predictable as they are in a parse and no more.
	static readonly string Input =
		string.Concat(new string('x', 4), "://", new string('y', 11), "/", new string('z', 8),
			"?a=1&b=2#top", new string('w', 12), " \t\n<>\"{}|^`");

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
