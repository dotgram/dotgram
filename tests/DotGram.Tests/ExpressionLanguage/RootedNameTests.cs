using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// A qualified name whose first part is already something here is never offered to the `using`s.
/// </summary>
/// <remarks>
/// <para>
/// C#'s order for a qualified name: its leftmost identifier is resolved in the enclosing
/// namespace, and where it is found there the `using`s are not consulted for that name at all.
/// Asked of Roslyn rather than assumed (2026-09-27), in the shape where consulting them WOULD
/// find something: a namespace holding a type called `System` with a nested `Math.Max` of its
/// own, a text saying `using` that namespace, and `System.Math.Max(1, 2)` written. C# accepts it
/// and answers 2 — the global `System.Math` — rather than 7 or an ambiguity.
/// </para>
/// <para>
/// This language answered 2 as well, by luck rather than by rule: the name resolved as written
/// before the imports were reached. What it did wrong was to go on asking. Every import was then
/// offered a name it could not hold, which cost `(int x) => System.Math.Max(x, 1)` five questions
/// of the type tables and fifteen prefix walks it had no use for — invisible while nothing was
/// imported by default, and the price of every qualified name once five namespaces were.
/// </para>
/// </remarks>
public sealed class RootedNameTests
{
	delegate int Counting();

	[Fact]
	public void A_using_does_not_supplant_the_first_part_of_a_qualified_name()
	{
		// `Rooted.System.Math.Max` answers 7 and the real one answers 2.
		Assert.Equal(
			2,
			Both.Compile<Counting>(
				"using DotGram.Tests.ExpressionLanguage.Rooted; () => System.Math.Max(1, 2)")());
	}

	[Fact]
	public void And_the_name_the_using_gives_is_still_reachable_when_it_is_written_whole()
	{
		Assert.Equal(
			7,
			Both.Compile<Counting>(
				"() => DotGram.Tests.ExpressionLanguage.Rooted.System.Math.Max(1, 2)")());
	}

	[Fact]
	public void A_simple_name_is_still_offered_to_the_usings()
	{
		// The rule is about a QUALIFIED name. A simple one has nowhere else to be found.
		Assert.Equal(3, Both.Compile<Counting>("using System; () => Math.Max(3, 1)")());
	}

	[Fact]
	public void A_qualified_name_whose_head_is_nothing_here_is_still_offered_to_them()
	{
		// `Generic.List<int>` under `using System.Collections;`: the head is no namespace of its
		// own, so the imports are where it is found.
		Assert.Equal(
			1,
			Both.Compile<Counting>(
				"using System.Collections; () => { var l = new Generic.List<int>(); l.Add(1); l.Count }")());
	}
}
