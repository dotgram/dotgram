using System;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// What a fully silent, non-recursive, capture-free publication saves by compiling to a
/// plain method instead of a state in the shared automaton — no arena, no dispatch, no
/// pooled parser.
/// </summary>
/// <remarks>
/// Same recognition work either side — the same characters read, the same repetition
/// possessive both times — and what separates them is kept to the least that denies
/// lowering: one named capture, which is what a real grammar would add the moment its
/// result needs to say which word it was.
/// </remarks>
[MemoryDiagnoser]
public partial class Flat
{
	[Gram("""
		Doc  = (Word & ',')* & ';'
		Word = ['a'..'z']+
		parse Doc
		""")]
	public sealed partial class Lowered
	{
	}

	[Gram("""
		Doc  = w: (Word & ',')* & ';'
		Word = ['a'..'z']+
		parse Doc
		""")]
	public sealed partial class NotLowered
	{
	}

	static readonly string Input = string.Concat(new string('x', 8), ",").Repeat(50) + ";";

	[Benchmark(Baseline = true)]
	public bool Flat_method() => Lowered.TryParseDoc(Input).IsSuccess;

	[Benchmark]
	public bool Shared_automaton() => NotLowered.TryParseDoc(Input).IsSuccess;
}
