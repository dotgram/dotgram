using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Grammar.Model;

/// <summary>
/// What a <c>when</c> asks of the grammar rather than of the input.
/// </summary>
/// <remarks>
/// A tree of its own and not more kinds of <see cref="Node"/>, because none of it is a
/// recognizer: the leaves hold recognizers and everything above them is logic. Keeping it
/// apart leaves the graph with one new node rather than four — and every pass that switches
/// over node kinds exhaustively with one arm rather than four.
/// </remarks>
public abstract record Test
{
	/// <summary>Whether two recognizers have a string in common — or, negated, have none.</summary>
	public sealed record Meets(Node Left, Node Right, bool Negated) : Test
	{
		public override string ToString() => $"{Left} is {(Negated ? "not " : "")}{Right}";
	}

	public sealed record All(Test Left, Test Right) : Test
	{
		public override string ToString() => $"{Left} and {Right}";
	}

	public sealed record Any(Test Left, Test Right) : Test
	{
		public override string ToString() => $"({Left} or {Right})";
	}

	/// <summary>Every recognizer under a test, for the walks that read nodes.</summary>
	public static IReadOnlyList<Node> Operands(Test test) => test switch
	{
		Meets(var left, var right, _) => [left, right],
		All(var left, var right)      => [.. Operands(left), .. Operands(right)],
		Any(var left, var right)      => [.. Operands(left), .. Operands(right)],
		_                             => [],
	};

	/// <summary>The same test with every recognizer under it rewritten.</summary>
	public static Test Mapped(Test test, Func<Node, Node> onto) => test switch
	{
		Meets(var left, var right, var negated) => new Meets(onto(left), onto(right), negated),
		All(var left, var right)                => new All(Mapped(left, onto), Mapped(right, onto)),
		Any(var left, var right)                => new Any(Mapped(left, onto), Mapped(right, onto)),
		_                                       => test,
	};
}
