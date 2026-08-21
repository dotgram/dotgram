using System;

using BenchmarkDotNet.Attributes;

namespace DotGram.Benchmarks;

/// <summary>
/// The loop a generated parser scans with, on its own rather than inside the method it is
/// generated into.
/// </summary>
/// <remarks>
/// <para>
/// Membership.cs measures a character test in a <c>foreach</c>; this measures the shape the
/// emitter actually writes — the possessive repetition, which is a bare loop reading the
/// span, testing, and stepping. The three forms are the ones tried on the emitter: the chain
/// of ranges it writes today, the same set as bits in a sixty-four bit window, and in a
/// thirty-two bit one.
/// </para>
/// <para>
/// Standalone because the method the emitter writes them into holds several thousand states,
/// and a change to it costs time whether or not the changed code runs — a window that was
/// never reached on the input measured cost the URL grammar seven per cent. Whatever these
/// forms are worth, that number drowns it. Here there is nothing to drown in: one small
/// method, one loop, one set.
/// </para>
/// <para>
/// SubDelim from the URL grammar — eleven marks between '!' and '=', twenty-nine places —
/// because it fits either width of window, so the same set can be asked all three ways.
/// </para>
/// <para>
/// The chain wins here — 17.7 against 19.0 — and loses in Membership.cs over a run of
/// letters, 24.6 against 19.4. What differs is not the loop and not the width: it is where
/// in the chain the character is found. A run of '&amp;' is caught by SubDelim's second
/// range; a run of 'x' is caught by Unreserved's fifth. So what a chain costs is the number
/// of terms tried before the answer, and what a window costs is the same whatever the
/// answer, and the two cross somewhere around the third or fourth term.
/// </para>
/// <para>
/// Which makes the useful question not "chain or window" but where in the chain the likely
/// character sits. The same six terms in three orders over the same input: caught first,
/// 12.4; caught second, which is what is generated today, 16.4; caught last, 37.0. Three
/// times, for writing the same set in a different order — more than any of the forms tried
/// here are worth against each other, and the window's flat 18.8 sits between the second
/// term and the third.
/// </para>
/// <para>
/// Ordering by width was tried on the emitter and came out level — 357/294/272/618/511
/// against 361/294/274/598/525 on the URL grammar. It cannot tell two ranges of the same
/// width apart, and <c>Unreserved</c> has two of twenty-six: the letters, in both cases.
/// Which of them goes first is the whole question for a lowercase URL, and width does not
/// answer it. Nor does the order the author wrote them in, because the ranges reach the
/// emitter already sorted by character code — that ordering is decided in the normalizer,
/// and it is decided before anyone knows it matters.
/// </para>
/// <para>
/// So the length of a chain is not what costs; the position of the answer in it is, at four
/// or five nanoseconds a term skipped. A chain of any length whose first term answers stays
/// where it started. Ranges are written in order of character code today, which puts
/// <c>Unreserved</c>'s letters fifth of six — near the wrong end of that range.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2, printSource: false)]
public class Scanning
{
	const ulong Wide   = 0x14000FE9UL;
	const uint  Narrow = 0x14000FE9u;

	// A run of members with one outsider at the end, which is how a repetition ends: on the
	// character that is not one of its own.
	static readonly string Text = new string('&', 64) + "z";

	[Benchmark(Baseline = true)]
	public int Ranges_chained()
	{
		var text = Text.AsSpan();
		var p    = 0;

		while (true)
		{
			if (p >= text.Length)
				break;

			var c = text[p];

			if (!(c == '!' || (c >= '$' && c <= '&') || (c >= '(' && c <= '+') ||
				  c == ',' || c == ';' || c == '='))
			{
				break;
			}

			p++;
		}

		return p;
	}

	// The same six terms in three orders, over the same input. Nothing about the set changes
	// — only where the range that answers is written.

	[Benchmark]
	public int Ranges_hit_first()
	{
		var text = Text.AsSpan();
		var p    = 0;

		while (true)
		{
			if (p >= text.Length)
				break;

			var c = text[p];

			if (!((c >= '$' && c <= '&') || c == '!' || (c >= '(' && c <= '+') ||
				  c == ',' || c == ';' || c == '='))
			{
				break;
			}

			p++;
		}

		return p;
	}

	[Benchmark]
	public int Ranges_hit_last()
	{
		var text = Text.AsSpan();
		var p    = 0;

		while (true)
		{
			if (p >= text.Length)
				break;

			var c = text[p];

			if (!(c == '!' || (c >= '(' && c <= '+') || c == ',' || c == ';' || c == '=' ||
				  (c >= '$' && c <= '&')))
			{
				break;
			}

			p++;
		}

		return p;
	}

	[Benchmark]
	public int Window_of_64_bits()
	{
		var text = Text.AsSpan();
		var p    = 0;

		while (true)
		{
			if (p >= text.Length)
				break;

			var c = text[p];

			if (!((uint)(c - '!') < 29 && (Wide >> (c - '!') & 1UL) != 0))
				break;

			p++;
		}

		return p;
	}

	[Benchmark]
	public int Window_of_32_bits()
	{
		var text = Text.AsSpan();
		var p    = 0;

		while (true)
		{
			if (p >= text.Length)
				break;

			var c = text[p];

			if (!((uint)(c - '!') < 29 && (Narrow >> (c - '!') & 1u) != 0))
				break;

			p++;
		}

		return p;
	}
}
