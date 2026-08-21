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
/// grammar. What separates them is kept to the least that denies the proof: the first sets
/// have to meet, which takes one character in common, so the open grammar reads three
/// characters more than the settled one and is otherwise the same work.
/// </para>
/// <para>
/// It was first written with a tail that read the whole run again, on the assumption that
/// denying possession took that much, and the number came out the same to a tenth — 122
/// against 167 there, 126 against 171 here. Which says the difference is the resume points
/// and not the reading: fifty turns, forty-five nanoseconds, nine tenths of a nanosecond a
/// turn, and an arena operation costs about eight tenths.
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

	/// <summary>
	/// Followed by something that can begin the way a word does: where each run ends is a
	/// question.
	/// </summary>
	/// <remarks>
	/// Beginning is all it takes. The proof of possession is that the first sets do not meet,
	/// so denying it needs one character in common and nothing more — the tail below reads
	/// three characters where the repetition reads four hundred, and the repetition is
	/// undecided all the same.
	/// </remarks>
	[Gram("""
		Doc  = (Word & ',')* & Tail
		Word = ['a'..'z']+
		Tail = ['a'..'z']+ & ';'
		parse Doc
		""")]
	public sealed partial class Open
	{
	}

	// One input for both, so the difference is not in what they are given. The settled
	// grammar ends on the semicolon; the open one wants a word before it, and `yyy` is that
	// word — three characters against the four hundred the repetition covers.
	static readonly string Settled_input = string.Concat(new string('x', 8), ",").Repeat(50) + ";";
	static readonly string Open_input    = string.Concat(new string('x', 8), ",").Repeat(50) + "yyy;";

	[Benchmark(Baseline = true)]
	public bool Nothing_to_give_back() => Settled.TryParseDoc(Settled_input).IsSuccess;

	[Benchmark]
	public bool A_resume_point_every_turn() => Open.TryParseDoc(Open_input).IsSuccess;
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
