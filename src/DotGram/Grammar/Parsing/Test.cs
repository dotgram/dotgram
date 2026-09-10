using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Grammar.Parsing;

/// <summary>
/// What a <c>when</c> asks of the grammar rather than of the input.
/// </summary>
/// <remarks>
/// <para>
/// A tree of its own and not more kinds of <see cref="Expr"/>, because none of it is a
/// recognizer: the leaves hold recognizers and everything above them is logic. Keeping it
/// apart is what leaves the grammar with one new expression rather than four, and one new
/// node in the graph rather than four.
/// </para>
/// <para>
/// <c>and</c> binds tighter than <c>or</c>, as in C#, and brackets say the rest. There is no
/// combinator inside a pattern — <c>Version is ("Old" | "New")</c> already says what
/// <c>Version is "Old" or "New"</c> would, and the language does not keep two ways to say
/// one thing.
/// </para>
/// </remarks>
public abstract record Test
{
	/// <summary>Whether two recognizers have a string in common — or, negated, have none.</summary>
	public sealed record Meets(Expr Left, Expr Right, bool Negated) : Test;

	/// <summary>A C# guard standing among the conditions, asked while the parser runs.</summary>
	public sealed record Runs(Expr Value) : Test;

	public sealed record All(Test Left, Test Right) : Test;
	public sealed record Any(Test Left, Test Right) : Test;

	/// <summary>Every C# guard under a test, which resolves as C# and not as a rule.</summary>
	public static IReadOnlyList<Expr> Guards(Test test) => test switch
	{
		Runs(var value)          => [value],
		All(var left, var right) => [.. Guards(left), .. Guards(right)],
		Any(var left, var right) => [.. Guards(left), .. Guards(right)],
		_                        => [],
	};

	/// <summary>Every recognizer under a test, for the walks that read expressions.</summary>
	public static IReadOnlyList<Expr> Operands(Test test) => test switch
	{
		Meets(var left, var right, _) => [left, right],
		Runs                          => [],
		All(var left, var right)      => [.. Operands(left), .. Operands(right)],
		Any(var left, var right)      => [.. Operands(left), .. Operands(right)],
		_                             => [],
	};
}
