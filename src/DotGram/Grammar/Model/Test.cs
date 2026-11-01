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
		public override string ToString()
		{
			return $"{Left} is {(Negated ? "not " : "")}{Right}";
		}
	}

	/// <summary>
	/// A C# guard standing among the conditions, asked while the parser runs.
	/// </summary>
	/// <remarks>
	/// The one leaf that has no answer at generation, and it does not need one: what the fold
	/// produces is a residue rather than a verdict. `false and @(g)` deletes the alternative
	/// and `@(g)` is never compiled into anything; `true and @(g)` leaves `@(g)` behind as an
	/// ordinary guard; `true or @(g)` leaves nothing at all.
	/// </remarks>
	public sealed record Runs(string Text, int At) : Test
	{
		public override string ToString()
		{
			return $"@({Text})";
		}
	}

	/// <summary>Tests joined by <c>and</c>, in the order written: a list, so a long chain is not a deep tree.</summary>
	public sealed record All(IReadOnlyList<Test> Items) : Test
	{
		/// <summary>Equal where the tests are, in order: a list compares by reference, and a test by what it says.</summary>
		public bool Equals(All? other)
		{
			return other is not null && Items.SequenceEqual(other.Items);
		}

		public override int GetHashCode()
		{
			var hash = 17;

			foreach (var item in Items)
				hash = unchecked(hash * 31 + item.GetHashCode());

			return hash;
		}

		public override string ToString()
		{
			return string.Join(" and ", Items);
		}
	}

	/// <summary>Tests joined by <c>or</c>, in the order written, as <see cref="All"/>.</summary>
	public sealed record Any(IReadOnlyList<Test> Items) : Test
	{
		/// <summary>Equal where the tests are, in order: a list compares by reference, and a test by what it says.</summary>
		public bool Equals(Any? other)
		{
			return other is not null && Items.SequenceEqual(other.Items);
		}

		public override int GetHashCode()
		{
			var hash = 17;

			foreach (var item in Items)
				hash = unchecked(hash * 31 + item.GetHashCode());

			return hash;
		}

		/// <remarks>Bracketed as each pair was when they were pairs, the first two innermost.</remarks>
		public override string ToString()
		{
			var text = Items[0].ToString();

			for (var at = 1; at < Items.Count; at++)
				text = $"({text} or {Items[at]})";

			return text;
		}
	}

	/// <summary>Every recognizer under a test, for the walks that read nodes.</summary>
	public static IReadOnlyList<Node> Operands(Test test)
	{
		return test switch
		{
			Meets(var left, var right, _) => [left, right],
			Runs => [],
			All(var items) => [.. items.SelectMany(Operands)],
			Any(var items) => [.. items.SelectMany(Operands)],
			_ => [],
		};
	}

	/// <summary>The same test with every recognizer under it rewritten.</summary>
	public static Test Mapped(Test test, Func<Node, Node> onto)
	{
		return test switch
		{
			Meets(var left, var right, var negated) => new Meets(onto(left), onto(right), negated),
			Runs => test,
			All(var items) => new All([.. items.Select(one => Mapped(one, onto))]),
			Any(var items) => new Any([.. items.Select(one => Mapped(one, onto))]),
			_ => test,
		};
	}
}
