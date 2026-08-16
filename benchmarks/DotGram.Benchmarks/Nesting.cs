using System;

using DotGram;

namespace DotGram.Benchmarks;

// How deep a grammar may nest before the process stack runs out, measured rather than
// reasoned about.
//
//     dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --depth 2600
//
// It has to be a process of its own: nesting too far is a `StackOverflowException`, which
// .NET does not let anybody catch — the process goes. So the caller walks the depth up
// until a run exits non-zero, and the last depth that printed `ok` is the answer.
//
// The number is a property of three things: the C# stack a run gets (1 MB by default),
// what a recognizer's frame costs, and the `stackalloc int[Machine.Backtracking]` inside
// it. The third is the one this project chooses, which is why it is worth measuring what
// choosing it costs.

[Gram("Expr = '(' & Expr & ')' | 'x'\nparse Expr")]
public sealed partial class Nesting
{
	/// <summary>Parses <paramref name="depth"/> brackets round an `x`, or dies trying.</summary>
	public static bool Reads(int depth) =>
		TryParseExpr(new string('(', depth) + "x" + new string(')', depth)).IsSuccess;
}
