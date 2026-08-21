using System;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// What it is worth for a repetition to know it will never be asked to give anything back.
/// </summary>
/// <remarks>
/// <para>
/// The same words, counted the same way, in two places. In the first, what follows the
/// repetition is a character none of its turns can begin with, so no turn of it can be the
/// wrong one and the whole construct compiles to a loop: no entry, no count, no way back.
/// In the second, what follows is a repetition of the same characters, so where the first
/// one ends is genuinely in question and every turn leaves a resume point behind.
/// </para>
/// <para>
/// Two grammars rather than one measured twice, because which form is written is decided by
/// what follows — that is the analysis, and there is no way to switch it off for one
/// grammar. Which puts a bound on what the number means: the second grammar has to be able
/// to consume the same characters, or its repetition would be forced too, so it is also
/// doing recognition the first does not. The difference is the shape of grammar that admits
/// the analysis against the shape that refuses it, and it is an upper bound on the analysis
/// itself rather than a measurement of it.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public partial class Possession
{
	/// <summary>Followed by something no word begins with: the run is forced.</summary>
	[Gram("""
		Doc  = (Word & ',')* & ';'
		Word = ['a'..'z']+
		parse Doc
		""")]
	public sealed partial class Settled
	{
	}

	/// <summary>Followed by more of the same: where each run ends is a question.</summary>
	[Gram("""
		Doc  = (Word & ',')* & Tail
		Word = ['a'..'z']+
		Tail = ['a'..'z' | ',']* & ';'
		parse Doc
		""")]
	public sealed partial class Open
	{
	}

	static readonly string Input = string.Concat(new string('x', 8), ",").Repeat(50) + ";";

	[Benchmark(Baseline = true)]
	public bool Nothing_to_give_back() => Settled.TryParseDoc(Input).IsSuccess;

	[Benchmark]
	public bool A_resume_point_every_turn() => Open.TryParseDoc(Input).IsSuccess;
}

static class Repeated
{
	public static string Repeat(this string text, int times)
	{
		var built = new System.Text.StringBuilder(text.Length * times);

		for (var i = 0; i < times; i++)
			built.Append(text);

		return built.ToString();
	}
}
