using System;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

namespace DotGram.Benchmarks;

/// <summary>
/// The three shapes an alternative that begins like its siblings can be written in, and
/// what each of them costs.
/// </summary>
/// <remarks>
/// <para>
/// The question this settles. Over kinds a choice is usually decided by the first token,
/// and then nothing is tried in order at all. What is left is the minority the normalizer
/// factors: alternatives that begin alike, whose shared head is therefore read - and, if
/// it is captured, captured - before the choice. Each alternative then has to be able to
/// say "not me, try the next" from halfway through, and there are three ways to write that
/// without a jump. A method of its own says it by returning a number. A local function
/// says the same thing and reaches the head by capturing it. Written in place, it is a
/// staircase of nested <c>if</c>s, one rung per token, and saying "not me" is falling off
/// the end of the staircase.
/// </para>
/// <para>
/// <b>The local function is not a third thing.</b> Roslyn compiles it to an ordinary
/// static method taking a struct closure by reference, so it is the first shape with every
/// captured local passed by reference rather than the read-only ones by value - and it
/// cannot capture the input at all, because a <c>ReadOnlySpan</c> may not go into a closure
/// (CS9108). It is measured here because it looks like a third option and is not.
/// </para>
/// <para>
/// What is measured is deliberately mechanical: three alternatives of twelve tokens each,
/// two of which run to their last token and fail there, over a head of two or ten captured
/// positions. The repetition is the point - a helper shared between the shapes would change
/// the thing being compared, which is the shape of the code itself.
/// </para>
/// <para>
/// The bounds test is written the same way in all of them. <c>(uint)p &gt;= (uint)k.Length</c>
/// proves to the JIT that the index is in range and takes the check off the read that
/// follows; <c>p &lt; k.Length</c> does not, and writing one shape each way measures the
/// check rather than the shape.
/// </para>
/// </remarks>
public class AlternativeShape
{
	/// <summary>Where a reading puts what it kept - <c>Ways</c>, with all the rest taken off.</summary>
	public sealed class Sink
	{
		public int[] Log = new int[1 << 16];
		public int   N;
	}

	// A method of its own, handed the head it did not read.
	static int Extracted2_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
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
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Extracted2_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
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
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Extracted2_Part2(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
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
		if ((uint)p >= (uint)k.Length || k[p] != 103) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Extracted2(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Extracted2_Part0(k, p, w, h0, h1);

		if (q < 0) q = Extracted2_Part1(k, p, w, h0, h1);
		if (q < 0) q = Extracted2_Part2(k, p, w, h0, h1);

		return q;
	}

	// The same, which the JIT is forbidden to compile into its caller.
	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Sealed2_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
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
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Sealed2_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
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
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Sealed2_Part2(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1)
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
		if ((uint)p >= (uint)k.Length || k[p] != 103) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Sealed2(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Sealed2_Part0(k, p, w, h0, h1);

		if (q < 0) q = Sealed2_Part1(k, p, w, h0, h1);
		if (q < 0) q = Sealed2_Part2(k, p, w, h0, h1);

		return q;
	}

	// A local function, which reaches the head by capturing it. The input it cannot
	// capture and takes as a parameter, which is CS9108 and not a choice.
	static int Local2(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		int Part0(ReadOnlySpan<int> k, int pos)
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
			if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

			var a1 = p;

			p++;

			w.Log[w.N++] = h0; w.Log[w.N++] = h1;
			w.Log[w.N++] = a1; w.Log[w.N++] = p;

			return p;
		}

		int Part1(ReadOnlySpan<int> k, int pos)
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
			if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

			var a1 = p;

			p++;

			w.Log[w.N++] = h0; w.Log[w.N++] = h1;
			w.Log[w.N++] = a1; w.Log[w.N++] = p;

			return p;
		}

		int Part2(ReadOnlySpan<int> k, int pos)
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
			if ((uint)p >= (uint)k.Length || k[p] != 103) return -1;

			var a1 = p;

			p++;

			w.Log[w.N++] = h0; w.Log[w.N++] = h1;
			w.Log[w.N++] = a1; w.Log[w.N++] = p;

			return p;
		}

		var q = Part0(k, p);

		if (q < 0) q = Part1(k, p);
		if (q < 0) q = Part2(k, p);

		return q;
	}

	// Written in place: one rung of the staircase per token, and failing is falling
	// off the end of it.
	static int Inline2(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = -1;

		{
			var p2 = p;

			if ((uint)p2 < (uint)k.Length && k[p2] == 2)
			{
				p2++;
				if ((uint)p2 < (uint)k.Length && k[p2] == 3)
				{
					p2++;
					if ((uint)p2 < (uint)k.Length && k[p2] == 4)
					{
						p2++;
						if ((uint)p2 < (uint)k.Length && k[p2] == 5)
						{
							p2++;
							if ((uint)p2 < (uint)k.Length && k[p2] == 6)
							{
								p2++;
								if ((uint)p2 < (uint)k.Length && k[p2] == 7)
								{
									p2++;
									if ((uint)p2 < (uint)k.Length && k[p2] == 8)
									{
										p2++;
										if ((uint)p2 < (uint)k.Length && k[p2] == 9)
										{
											p2++;
											if ((uint)p2 < (uint)k.Length && k[p2] == 10)
											{
												p2++;
												if ((uint)p2 < (uint)k.Length && k[p2] == 11)
												{
													p2++;
													if ((uint)p2 < (uint)k.Length && k[p2] == 101)
													{
														var a1 = p2;

														p2++;

														w.Log[w.N++] = h0; w.Log[w.N++] = h1;
														w.Log[w.N++] = a1; w.Log[w.N++] = p2;

														q = p2;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		if (q < 0)
		{
			var p2 = p;

			if ((uint)p2 < (uint)k.Length && k[p2] == 2)
			{
				p2++;
				if ((uint)p2 < (uint)k.Length && k[p2] == 3)
				{
					p2++;
					if ((uint)p2 < (uint)k.Length && k[p2] == 4)
					{
						p2++;
						if ((uint)p2 < (uint)k.Length && k[p2] == 5)
						{
							p2++;
							if ((uint)p2 < (uint)k.Length && k[p2] == 6)
							{
								p2++;
								if ((uint)p2 < (uint)k.Length && k[p2] == 7)
								{
									p2++;
									if ((uint)p2 < (uint)k.Length && k[p2] == 8)
									{
										p2++;
										if ((uint)p2 < (uint)k.Length && k[p2] == 9)
										{
											p2++;
											if ((uint)p2 < (uint)k.Length && k[p2] == 10)
											{
												p2++;
												if ((uint)p2 < (uint)k.Length && k[p2] == 11)
												{
													p2++;
													if ((uint)p2 < (uint)k.Length && k[p2] == 102)
													{
														var a1 = p2;

														p2++;

														w.Log[w.N++] = h0; w.Log[w.N++] = h1;
														w.Log[w.N++] = a1; w.Log[w.N++] = p2;

														q = p2;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		if (q < 0)
		{
			var p2 = p;

			if ((uint)p2 < (uint)k.Length && k[p2] == 2)
			{
				p2++;
				if ((uint)p2 < (uint)k.Length && k[p2] == 3)
				{
					p2++;
					if ((uint)p2 < (uint)k.Length && k[p2] == 4)
					{
						p2++;
						if ((uint)p2 < (uint)k.Length && k[p2] == 5)
						{
							p2++;
							if ((uint)p2 < (uint)k.Length && k[p2] == 6)
							{
								p2++;
								if ((uint)p2 < (uint)k.Length && k[p2] == 7)
								{
									p2++;
									if ((uint)p2 < (uint)k.Length && k[p2] == 8)
									{
										p2++;
										if ((uint)p2 < (uint)k.Length && k[p2] == 9)
										{
											p2++;
											if ((uint)p2 < (uint)k.Length && k[p2] == 10)
											{
												p2++;
												if ((uint)p2 < (uint)k.Length && k[p2] == 11)
												{
													p2++;
													if ((uint)p2 < (uint)k.Length && k[p2] == 103)
													{
														var a1 = p2;

														p2++;

														w.Log[w.N++] = h0; w.Log[w.N++] = h1;
														w.Log[w.N++] = a1; w.Log[w.N++] = p2;

														q = p2;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		return q;
	}

	// A method of its own, handed the head it did not read.
	static int Extracted10_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8, int h9)
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
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Extracted10_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8, int h9)
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
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Extracted10_Part2(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8, int h9)
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
		if ((uint)p >= (uint)k.Length || k[p] != 103) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Extracted10(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;
		var h2 = p + 2;
		var h3 = p + 3;
		var h4 = p + 4;
		var h5 = p + 5;
		var h6 = p + 6;
		var h7 = p + 7;
		var h8 = p + 8;
		var h9 = p + 9;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Extracted10_Part0(k, p, w, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);

		if (q < 0) q = Extracted10_Part1(k, p, w, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);
		if (q < 0) q = Extracted10_Part2(k, p, w, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);

		return q;
	}

	// The same, which the JIT is forbidden to compile into its caller.
	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Sealed10_Part0(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8, int h9)
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
		if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Sealed10_Part1(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8, int h9)
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
		if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static int Sealed10_Part2(ReadOnlySpan<int> k, int pos, Sink w, int h0, int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8, int h9)
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
		if ((uint)p >= (uint)k.Length || k[p] != 103) return -1;

		var a1 = p;

		p++;

		w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
		w.Log[w.N++] = a1; w.Log[w.N++] = p;

		return p;
	}

	static int Sealed10(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;
		var h2 = p + 2;
		var h3 = p + 3;
		var h4 = p + 4;
		var h5 = p + 5;
		var h6 = p + 6;
		var h7 = p + 7;
		var h8 = p + 8;
		var h9 = p + 9;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = Sealed10_Part0(k, p, w, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);

		if (q < 0) q = Sealed10_Part1(k, p, w, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);
		if (q < 0) q = Sealed10_Part2(k, p, w, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);

		return q;
	}

	// A local function, which reaches the head by capturing it. The input it cannot
	// capture and takes as a parameter, which is CS9108 and not a choice.
	static int Local10(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;
		var h2 = p + 2;
		var h3 = p + 3;
		var h4 = p + 4;
		var h5 = p + 5;
		var h6 = p + 6;
		var h7 = p + 7;
		var h8 = p + 8;
		var h9 = p + 9;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		int Part0(ReadOnlySpan<int> k, int pos)
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
			if ((uint)p >= (uint)k.Length || k[p] != 101) return -1;

			var a1 = p;

			p++;

			w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
			w.Log[w.N++] = a1; w.Log[w.N++] = p;

			return p;
		}

		int Part1(ReadOnlySpan<int> k, int pos)
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
			if ((uint)p >= (uint)k.Length || k[p] != 102) return -1;

			var a1 = p;

			p++;

			w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
			w.Log[w.N++] = a1; w.Log[w.N++] = p;

			return p;
		}

		int Part2(ReadOnlySpan<int> k, int pos)
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
			if ((uint)p >= (uint)k.Length || k[p] != 103) return -1;

			var a1 = p;

			p++;

			w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
			w.Log[w.N++] = a1; w.Log[w.N++] = p;

			return p;
		}

		var q = Part0(k, p);

		if (q < 0) q = Part1(k, p);
		if (q < 0) q = Part2(k, p);

		return q;
	}

	// Written in place: one rung of the staircase per token, and failing is falling
	// off the end of it.
	static int Inline10(ReadOnlySpan<int> k, int pos, Sink w)
	{
		var p = pos;

		var h0 = p + 0;
		var h1 = p + 1;
		var h2 = p + 2;
		var h3 = p + 3;
		var h4 = p + 4;
		var h5 = p + 5;
		var h6 = p + 6;
		var h7 = p + 7;
		var h8 = p + 8;
		var h9 = p + 9;

		if ((uint)p >= (uint)k.Length || k[p] != 1) return -1;

		p++;

		var q = -1;

		{
			var p2 = p;

			if ((uint)p2 < (uint)k.Length && k[p2] == 2)
			{
				p2++;
				if ((uint)p2 < (uint)k.Length && k[p2] == 3)
				{
					p2++;
					if ((uint)p2 < (uint)k.Length && k[p2] == 4)
					{
						p2++;
						if ((uint)p2 < (uint)k.Length && k[p2] == 5)
						{
							p2++;
							if ((uint)p2 < (uint)k.Length && k[p2] == 6)
							{
								p2++;
								if ((uint)p2 < (uint)k.Length && k[p2] == 7)
								{
									p2++;
									if ((uint)p2 < (uint)k.Length && k[p2] == 8)
									{
										p2++;
										if ((uint)p2 < (uint)k.Length && k[p2] == 9)
										{
											p2++;
											if ((uint)p2 < (uint)k.Length && k[p2] == 10)
											{
												p2++;
												if ((uint)p2 < (uint)k.Length && k[p2] == 11)
												{
													p2++;
													if ((uint)p2 < (uint)k.Length && k[p2] == 101)
													{
														var a1 = p2;

														p2++;

														w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
														w.Log[w.N++] = a1; w.Log[w.N++] = p2;

														q = p2;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		if (q < 0)
		{
			var p2 = p;

			if ((uint)p2 < (uint)k.Length && k[p2] == 2)
			{
				p2++;
				if ((uint)p2 < (uint)k.Length && k[p2] == 3)
				{
					p2++;
					if ((uint)p2 < (uint)k.Length && k[p2] == 4)
					{
						p2++;
						if ((uint)p2 < (uint)k.Length && k[p2] == 5)
						{
							p2++;
							if ((uint)p2 < (uint)k.Length && k[p2] == 6)
							{
								p2++;
								if ((uint)p2 < (uint)k.Length && k[p2] == 7)
								{
									p2++;
									if ((uint)p2 < (uint)k.Length && k[p2] == 8)
									{
										p2++;
										if ((uint)p2 < (uint)k.Length && k[p2] == 9)
										{
											p2++;
											if ((uint)p2 < (uint)k.Length && k[p2] == 10)
											{
												p2++;
												if ((uint)p2 < (uint)k.Length && k[p2] == 11)
												{
													p2++;
													if ((uint)p2 < (uint)k.Length && k[p2] == 102)
													{
														var a1 = p2;

														p2++;

														w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
														w.Log[w.N++] = a1; w.Log[w.N++] = p2;

														q = p2;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		if (q < 0)
		{
			var p2 = p;

			if ((uint)p2 < (uint)k.Length && k[p2] == 2)
			{
				p2++;
				if ((uint)p2 < (uint)k.Length && k[p2] == 3)
				{
					p2++;
					if ((uint)p2 < (uint)k.Length && k[p2] == 4)
					{
						p2++;
						if ((uint)p2 < (uint)k.Length && k[p2] == 5)
						{
							p2++;
							if ((uint)p2 < (uint)k.Length && k[p2] == 6)
							{
								p2++;
								if ((uint)p2 < (uint)k.Length && k[p2] == 7)
								{
									p2++;
									if ((uint)p2 < (uint)k.Length && k[p2] == 8)
									{
										p2++;
										if ((uint)p2 < (uint)k.Length && k[p2] == 9)
										{
											p2++;
											if ((uint)p2 < (uint)k.Length && k[p2] == 10)
											{
												p2++;
												if ((uint)p2 < (uint)k.Length && k[p2] == 11)
												{
													p2++;
													if ((uint)p2 < (uint)k.Length && k[p2] == 103)
													{
														var a1 = p2;

														p2++;

														w.Log[w.N++] = h0; w.Log[w.N++] = h1; w.Log[w.N++] = h2; w.Log[w.N++] = h3; w.Log[w.N++] = h4; w.Log[w.N++] = h5; w.Log[w.N++] = h6; w.Log[w.N++] = h7; w.Log[w.N++] = h8; w.Log[w.N++] = h9;
														w.Log[w.N++] = a1; w.Log[w.N++] = p2;

														q = p2;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		return q;
	}

	int[] _input = null!;

	readonly Sink _sink = new();

	[GlobalSetup]
	public void Setup() => _input = Input();

	/// <summary>
	/// The head every alternative shares, the middle every alternative wants, and a last
	/// token only the third of them accepts.
	/// </summary>
	static int[] Input()
	{
		var one = new int[12];

		one[0] = 1;

		for (var i = 1; i < one.Length - 1; i++)
			one[i] = i + 1;

		one[one.Length - 1] = 103;

		var all = new int[one.Length * 4096];

		for (var i = 0; i < all.Length; i++)
			all[i] = one[i % one.Length];

		return all;
	}

	[Benchmark]
	public long Extracted_head_of_2()
	{
		var k = _input.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 12 <= k.Length; at += 12)
		{
			// The log is a ring here. What is timed is the reading, and a log that grew
			// would time an allocation instead.
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Extracted2(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long Sealed_head_of_2()
	{
		var k = _input.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 12 <= k.Length; at += 12)
		{
			// The log is a ring here. What is timed is the reading, and a log that grew
			// would time an allocation instead.
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Sealed2(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long Local_head_of_2()
	{
		var k = _input.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 12 <= k.Length; at += 12)
		{
			// The log is a ring here. What is timed is the reading, and a log that grew
			// would time an allocation instead.
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Local2(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long Inline_head_of_2()
	{
		var k = _input.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 12 <= k.Length; at += 12)
		{
			// The log is a ring here. What is timed is the reading, and a log that grew
			// would time an allocation instead.
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Inline2(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long Extracted_head_of_10()
	{
		var k = _input.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 12 <= k.Length; at += 12)
		{
			// The log is a ring here. What is timed is the reading, and a log that grew
			// would time an allocation instead.
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Extracted10(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long Sealed_head_of_10()
	{
		var k = _input.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 12 <= k.Length; at += 12)
		{
			// The log is a ring here. What is timed is the reading, and a log that grew
			// would time an allocation instead.
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Sealed10(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long Local_head_of_10()
	{
		var k = _input.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 12 <= k.Length; at += 12)
		{
			// The log is a ring here. What is timed is the reading, and a log that grew
			// would time an allocation instead.
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Local10(k, at, w);
		}

		return total;
	}

	[Benchmark]
	public long Inline_head_of_10()
	{
		var k = _input.AsSpan();
		var w = _sink;

		w.N = 0;

		long total = 0;

		for (var at = 0; at + 12 <= k.Length; at += 12)
		{
			// The log is a ring here. What is timed is the reading, and a log that grew
			// would time an allocation instead.
			if (w.N > w.Log.Length - 32)
				w.N = 0;

			total += Inline10(k, at, w);
		}

		return total;
	}

}
