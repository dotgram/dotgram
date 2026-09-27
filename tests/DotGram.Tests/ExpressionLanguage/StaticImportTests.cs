using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// `using static T;` brings T's static methods into reach as bare names, and its extension
/// methods into the extension search.
/// </summary>
/// <remarks>
/// <para>
/// Every rule was asked of the C# compiler first (2026-09-26). A local wins over a name a
/// `using static` gives, and a local DELEGATE is called in preference to a static method that
/// would also fit; two `using static`s giving one method is CS0121 — an ambiguous CALL out of
/// overload resolution, not an ambiguous reference — so this language refuses it with the
/// sentence it already had for a call it cannot choose.
/// </para>
/// <para>
/// The way stands after the one that calls a declared name, and that ordering IS the rule: the
/// guard here runs only where the reading would otherwise have refused outright.
/// </para>
/// </remarks>
public sealed class StaticImportTests
{
	delegate int Counting();

	delegate int Over(int one);

	[Fact]
	public void A_bare_name_calls_a_static_method_it_brought_in()
	{
		Assert.Equal(2, Both.Compile<Counting>("using static System.Math; () => Abs(-2)")());
	}

	[Fact]
	public void Without_the_directive_the_same_name_is_nothing()
	{
		Assert.False(Both.TryParse("() => Abs(-2)").IsSuccess);
	}

	[Fact]
	public void Two_of_them_are_chosen_between_as_one_call()
	{
		// `Math.Max` and `Math.Min` are one type; two types is the case that matters, and
		// `Convert.ToInt32` against `Math.Abs` gives each a name of its own.
		Assert.Equal(
			5,
			Both.Compile<Counting>("using static System.Math; using static System.Convert; () => Abs(-3) + ToInt32(2)")());
	}

	[Fact]
	public void A_local_of_the_name_wins_over_what_it_brought_in()
	{
		// C#'s answer, asked of Roslyn: the local delegate is called even where the static
		// method would fit.
		Assert.Equal(
			7,
			Both.Compile<Over>("using static System.Math; (int one) => { System.Func<int, int> Abs = (int n) => n + 8; Abs(one) }")(-1));
	}

	[Fact]
	public void An_overload_is_chosen_among_what_it_brought_in()
	{
		Assert.Equal(3, Both.Compile<Counting>("using static System.Math; () => (int)Max(3.0, 1.0)")());
	}

	[Fact]
	public void An_extension_method_of_the_type_it_names_is_found()
	{
		Assert.Equal(
			4,
			Both.Compile<Counting>("using static DotGram.Tests.ExpressionLanguage.Extras; () => 2.Doubled()")());
	}

	[Fact]
	public void A_name_it_did_not_bring_in_is_still_nothing()
	{
		Assert.False(Both.TryParse("using static System.Math; () => Nowhere(1)").IsSuccess);
	}

	[Fact]
	public void The_same_type_named_twice_is_named_once()
	{
		Assert.Equal(2, Both.Compile<Counting>("using static System.Math; using static System.Math; () => Abs(-2)")());
	}

	[Fact]
	public void A_bare_name_reads_a_static_member_it_brought_in()
	{
		Assert.Equal(3, Both.Compile<Counting>("using static System.Math; () => (int)PI")());
	}

	[Fact]
	public void A_local_of_the_name_wins_over_a_member_it_brought_in()
	{
		Assert.Equal(7, Both.Compile<Counting>("using static System.Math; () => { int PI = 7; PI }")());
	}

	[Fact]
	public void A_nested_type_it_brought_in_is_named()
	{
		Assert.Equal(
			(int)Environment.SpecialFolder.System,
			Both.Compile<Counting>("using static System.Environment; () => (int)SpecialFolder.System")());
	}

	[Fact]
	public void A_nested_type_and_a_using_giving_one_name_is_ambiguous()
	{
		// C#'s CS0104: a nested type a `using static` gives is a peer of a type a namespace
		// `using` gives, asked of Roslyn.
		var thrown = Assert.Throws<InvalidOperationException>(
			() => Both.Parse(
				"using static DotGram.Tests.ExpressionLanguage.Nesting; " +
				"using DotGram.Tests.ExpressionLanguage.Left; () => Twin.Value"));

		Assert.Contains("ambiguous", thrown.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void A_member_and_a_type_of_one_name_read_as_a_value_is_ambiguous()
	{
		// C#'s CS0229, which is not CS0104: the two questions are asked of different things, and
		// only a value can meet this one.
		var thrown = Assert.Throws<InvalidOperationException>(
			() => Both.Parse(
				"using static DotGram.Tests.ExpressionLanguage.Holds; " +
				"using DotGram.Tests.ExpressionLanguage.Right; () => Twin"));

		Assert.Contains("ambiguous", thrown.Message, StringComparison.Ordinal);
	}
}
