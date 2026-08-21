using System;

using DotGram;

namespace DotGram.Benchmarks;

// Verifies that grammar nesting is independent of the process stack.
//
//     dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --depth 2600
//
// Calls are arena frames rather than C# calls, so this is now a stress command rather
// than a search for the process-stack limit.

[Gram("Expr = '(' & Expr & ')' | 'x'\nparse Expr")]
public sealed partial class Nesting
{
	/// <summary>Parses <paramref name="depth"/> brackets round an `x`, or dies trying.</summary>
	public static bool Reads(int depth) =>
		TryParseExpr(new string('(', depth) + "x" + new string(')', depth)).IsSuccess;
}
