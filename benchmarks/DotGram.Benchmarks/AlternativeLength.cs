using System;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

namespace DotGram.Benchmarks;

/// <summary>
/// How long an alternative may be before a method of its own stops being free.
/// </summary>
/// <remarks>
/// <para>
/// <c>AlternativeShape.cs</c> settles which shape to write an alternative in and finds
/// that a method of its own costs nothing — because the JIT compiles it into its caller,
/// and the one row where it may not costs 58-64%. That leaves the question this asks: how
/// long may the alternative be before the JIT stops? Above that length the reader is
/// paying the 58-64% and nothing says so.
/// </para>
/// <para>
/// The measurement is a pair at each length: the same alternative as an ordinary method,
/// and as one the JIT is forbidden to compile in. While the two differ, the ordinary one
/// is being compiled in. Where they meet, it is not, and the length they meet at is the
/// line the emitter has to know about — read off in tokens here, and turned into what
/// <c>Machine.Sizes.cs</c> counts by the rule that one element of a sequence is about one
/// state.
/// </para>
/// <para>
/// Two alternatives rather than three, because two is enough to make one of them fail at
/// its last token and the code doubles with every one.
/// </para>
/// </remarks>
public class AlternativeLength
{
	/// <summary>Where a reading puts what it kept.</summary>
	public sealed class Sink
	{
		public int[] Log = new int[1 << 16];
		public int   N;
	}

	static int Open4_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open4_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open4(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Open4_Part0(k, p, w, h0, h1);

		if (q < 0) q = Open4_Part1(k, p, w, h0, h1);

		return q;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut4_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut4_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Shut4(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Shut4_Part0(k, p, w, h0, h1);

		if (q < 0) q = Shut4_Part1(k, p, w, h0, h1);

		return q;
	}

	static int Open8_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open8_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open8(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Open8_Part0(k, p, w, h0, h1);

		if (q < 0) q = Open8_Part1(k, p, w, h0, h1);

		return q;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut8_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut8_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Shut8(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Shut8_Part0(k, p, w, h0, h1);

		if (q < 0) q = Shut8_Part1(k, p, w, h0, h1);

		return q;
	}

	static int Open16_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open16_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open16(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Open16_Part0(k, p, w, h0, h1);

		if (q < 0) q = Open16_Part1(k, p, w, h0, h1);

		return q;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut16_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut16_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Shut16(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Shut16_Part0(k, p, w, h0, h1);

		if (q < 0) q = Shut16_Part1(k, p, w, h0, h1);

		return q;
	}

	static int Open32_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 16) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 17) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 18) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 19) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 20) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 21) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 22) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 23) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 24) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 25) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 26) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 27) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 28) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 29) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 30) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 31) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open32_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 16) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 17) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 18) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 19) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 20) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 21) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 22) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 23) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 24) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 25) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 26) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 27) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 28) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 29) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 30) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 31) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open32(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Open32_Part0(k, p, w, h0, h1);

		if (q < 0) q = Open32_Part1(k, p, w, h0, h1);

		return q;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut32_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 16) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 17) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 18) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 19) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 20) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 21) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 22) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 23) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 24) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 25) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 26) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 27) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 28) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 29) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 30) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 31) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut32_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 16) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 17) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 18) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 19) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 20) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 21) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 22) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 23) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 24) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 25) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 26) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 27) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 28) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 29) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 30) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 31) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Shut32(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Shut32_Part0(k, p, w, h0, h1);

		if (q < 0) q = Shut32_Part1(k, p, w, h0, h1);

		return q;
	}

	static int Open64_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 16) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 17) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 18) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 19) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 20) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 21) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 22) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 23) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 24) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 25) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 26) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 27) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 28) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 29) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 30) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 31) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 32) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 33) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 34) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 35) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 36) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 37) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 38) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 39) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 40) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 41) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 42) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 43) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 44) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 45) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 46) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 47) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 48) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 49) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 50) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 51) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 52) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 53) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 54) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 55) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 56) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 57) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 58) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 59) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 60) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 61) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 62) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 63) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open64_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 16) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 17) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 18) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 19) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 20) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 21) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 22) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 23) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 24) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 25) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 26) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 27) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 28) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 29) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 30) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 31) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 32) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 33) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 34) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 35) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 36) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 37) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 38) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 39) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 40) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 41) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 42) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 43) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 44) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 45) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 46) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 47) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 48) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 49) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 50) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 51) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 52) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 53) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 54) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 55) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 56) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 57) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 58) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 59) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 60) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 61) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 62) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 63) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Open64(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Open64_Part0(k, p, w, h0, h1);

		if (q < 0) q = Open64_Part1(k, p, w, h0, h1);

		return q;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut64_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 16) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 17) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 18) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 19) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 20) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 21) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 22) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 23) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 24) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 25) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 26) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 27) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 28) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 29) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 30) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 31) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 32) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 33) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 34) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 35) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 36) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 37) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 38) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 39) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 40) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 41) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 42) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 43) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 44) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 45) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 46) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 47) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 48) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 49) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 50) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 51) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 52) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 53) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 54) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 55) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 56) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 57) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 58) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 59) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 60) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 61) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 62) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 63) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Shut64_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
	{
		var p = pos;

		if ((uint)p >= (uint)k.Length || k[p] != 2) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 3) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 4) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 5) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 6) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 7) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 8) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 9) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 10) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 11) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 12) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 13) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 14) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 15) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 16) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 17) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 18) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 19) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 20) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 21) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 22) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 23) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 24) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 25) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 26) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 27) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 28) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 29) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 30) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 31) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 32) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 33) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 34) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 35) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 36) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 37) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 38) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 39) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 40) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 41) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 42) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 43) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 44) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 45) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 46) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 47) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 48) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 49) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 50) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 51) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 52) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 53) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 54) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 55) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 56) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 57) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 58) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 59) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 60) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 61) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 62) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 63) return -1;
		p++;
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Shut64(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Shut64_Part0(k, p, w, h0, h1);

		if (q < 0) q = Shut64_Part1(k, p, w, h0, h1);

		return q;
	}

	int[] _of4 = null!;
	int[] _of8 = null!;
	int[] _of16 = null!;
	int[] _of32 = null!;
	int[] _of64 = null!;

	readonly Sink _sink = new();

	[GlobalSetup]
	public void Setup()
	{
		_of4 = Input(4);
		_of8 = Input(8);
		_of16 = Input(16);
		_of32 = Input(32);
		_of64 = Input(64);
	}

	/// <summary>
	/// The head both alternatives share, the middle both want, and a last token only the
	/// second of them accepts.
	/// </summary>
	static int[] Input(int size)
	{
		var one = new int[size];

		one[0] = 1;

		for (var i = 1; i < one.Length - 1; i++)
			one[i] = i + 1;

		one[one.Length - 1] = 102;

		var all = new int[one.Length * 4096];

		for (var i = 0; i < all.Length; i++)
			all[i] = one[i % one.Length];

		return all;
	}

	[Benchmark]
	public long compiled_in_of_4()
	{
		var k = _of4.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 4 <= k.Length; at += 4)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Open4(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long not_compiled_in_of_4()
	{
		var k = _of4.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 4 <= k.Length; at += 4)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Shut4(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long compiled_in_of_8()
	{
		var k = _of8.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 8 <= k.Length; at += 8)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Open8(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long not_compiled_in_of_8()
	{
		var k = _of8.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 8 <= k.Length; at += 8)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Shut8(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long compiled_in_of_16()
	{
		var k = _of16.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 16 <= k.Length; at += 16)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Open16(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long not_compiled_in_of_16()
	{
		var k = _of16.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 16 <= k.Length; at += 16)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Shut16(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long compiled_in_of_32()
	{
		var k = _of32.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 32 <= k.Length; at += 32)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Open32(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long not_compiled_in_of_32()
	{
		var k = _of32.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 32 <= k.Length; at += 32)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Shut32(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long compiled_in_of_64()
	{
		var k = _of64.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 64 <= k.Length; at += 64)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Open64(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long not_compiled_in_of_64()
	{
		var k = _of64.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 64 <= k.Length; at += 64)
		{
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Shut64(k, at, w);
		}

		return total;
	}

}
