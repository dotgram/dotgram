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
/// character sits — and that is something the emitter chooses, since it writes the ranges in
/// whatever order the grammar happened to name them.
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
