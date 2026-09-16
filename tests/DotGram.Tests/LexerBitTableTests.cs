using System;
using System.Collections.Generic;
using System.Reflection;

using DotGram.Grammar.Emit;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

public sealed class LexerBitTableTests
{
	static readonly MethodInfo Fill = typeof(LexerEmitter)
		.GetMethod("Fill", BindingFlags.NonPublic | BindingFlags.Static)!;

	[Fact]
	public void Every_boundary_alignment_matches_character_membership()
	{
		for (var from = 0; from < 32; from++)
			for (var to = from; to < 32; to++)
				Check([new CharRange((char)from, (char)to)]);
	}

	[Fact]
	public void Overlapping_unsorted_and_full_character_ranges_match_membership()
	{
		Check([new CharRange('\0', '\uffff')]);
		Check([new CharRange('\ufffd', '\uffff'), new CharRange('a', 'z'),
			new CharRange('\0', 'b'), new CharRange('x', 'c')]);
		Check([new CharRange('\u0101', '\u021f'), new CharRange('\u0107', '\u0110')]);
	}

	[Fact]
	public void Reusing_the_buffer_clears_previous_ranges()
	{
		var bits = new byte[8192];
		Fill.Invoke(null, [bits, new CharRange[] { new('\0', '\uffff') }]);
		Fill.Invoke(null, [bits, Array.Empty<CharRange>()]);
		Assert.Equal(new byte[8192], bits);
	}

	static void Check(IReadOnlyList<CharRange> ranges)
	{
		var expected = new byte[8192];
		foreach (var range in ranges)
			for (var c = (int)range.From; c <= range.To; c++)
				expected[c / 8] |= (byte)(1 << (c % 8));
		var actual = new byte[8192];
		Fill.Invoke(null, [actual, ranges]);
		Assert.Equal(expected, actual);
	}
}
