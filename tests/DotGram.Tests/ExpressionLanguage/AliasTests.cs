using System;
using System.Collections.Generic;
using System.Reflection;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// `using X = …;` gives one type or one namespace a name, and answers as C# answers.
/// </summary>
/// <remarks>
/// <para>
/// Every row here was asked of the C# compiler first and the answer written down before a line of
/// this was built (2026-09-26), which is the technique the overload rules were settled with: a
/// test written from my reading of the specification proves only that I read it the same way
/// twice. What Roslyn said:
/// </para>
/// <list type="bullet">
/// <item>an alias of a closed generic is accepted, of an open one CS7003;</item>
/// <item>an alias of a namespace is accepted, and names no type on its own;</item>
/// <item>an alias whose name is a type in the GLOBAL namespace is CS0576, reported where the
/// alias is written and not where the name is used;</item>
/// <item>an alias against a name a `using` brings in is accepted, silently, and the alias
/// wins — the same against a name a global using brings in.</item>
/// </list>
/// </remarks>
public sealed class AliasTests
{
	static readonly Assembly Caller = typeof(AliasTests).Assembly;

	delegate int Counting();

	delegate string Naming();

	[Fact]
	public void An_alias_names_a_type()
	{
		Assert.Equal(
			"hello",
			Both.Compile<Naming>("using S = System.String; () => S.Concat(\"hel\", \"lo\")")());
	}

	[Fact]
	public void An_alias_names_a_closed_generic()
	{
		Assert.Equal(
			2,
			Both.Compile<Counting>(
				"using L = System.Collections.Generic.List<int>; () => { var l = new L(); l.Add(1); l.Add(2); l.Count }")());
	}

	[Fact]
	public void An_alias_of_an_open_generic_is_refused()
	{
		Assert.False(
			Both.TryParse("using L = System.Collections.Generic.List<>; () => 1").IsSuccess,
			"an open generic was given a name; C# refuses that with CS7003.");
	}

	[Fact]
	public void An_alias_names_a_namespace()
	{
		Assert.Equal(
			1,
			Both.Compile<Counting>(
				"using C = System.Collections.Generic; () => { var l = new C.List<int>(); l.Add(1); l.Count }")());
	}

	[Fact]
	public void A_namespace_alias_is_no_type_on_its_own()
	{
		Assert.False(
			Both.TryParse("using C = System.Collections.Generic; () => C.Equals(1, 1)").IsSuccess,
			"a namespace alias answered as a type; in C# it names a namespace and nothing else.");
	}

	[Fact]
	public void An_alias_beats_a_name_a_using_brings_in()
	{
		// `Twofold` is a type in DotGram.Tests.ExpressionLanguage.Elsewhere, and the `using`
		// brings it in. C# lets the alias win with no diagnostic at all.
		Assert.Equal(
			"aliased",
			Both.Compile<Naming>(
				"using DotGram.Tests.ExpressionLanguage.Elsewhere; using Twofold = System.String; " +
				"() => Twofold.Concat(\"alias\", \"ed\")")());
	}

	[Fact]
	public void An_alias_named_after_a_type_in_the_global_namespace_is_refused()
	{
		var loaded = GeneratorDriverTests.Build(
			"""
			[DotGram.Gram("A = \"x\"")]
			public partial class Aliased
			{
				public static string Where()
				{
					return "loaded";
				}
			}
			""");

		var scope = ResolutionScope.Of(Caller, loaded);

		// The type is there, in no namespace, so it IS the global namespace's.
		Assert.True(ExpressionParser.TryParse("() => Aliased.Where()", scope).IsSuccess);

		var refused = ExpressionParser.TryParse("using Aliased = System.String; () => 1", scope);

		Assert.False(refused.IsSuccess, "an alias took a name the global namespace has; C# refuses that (CS0576).");
		Assert.Contains("Aliased", refused.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void The_same_alias_written_twice_is_the_same_alias()
	{
		Assert.Equal(
			"ab",
			Both.Compile<Naming>(
				"using S = System.String; using S = System.String; () => S.Concat(\"a\", \"b\")")());
	}

	[Fact]
	public void One_name_given_two_meanings_is_refused()
	{
		Assert.False(
			Both.TryParse("using S = System.String; using S = System.Int32; () => 1").IsSuccess,
			"one alias was given two meanings; C# refuses that (CS1537).");
	}

	[Fact]
	public void An_alias_reaches_what_is_nested_in_what_it_names()
	{
		Assert.Equal(
			(int)Environment.SpecialFolder.System,
			Both.Compile<Counting>("using E = System.Environment; () => (int)E.SpecialFolder.System")());
	}

	/// <summary>An alias is read where it stands, so a text that writes none is unchanged.</summary>
	[Fact]
	public void A_text_with_no_alias_is_unchanged()
	{
		Assert.Equal(3, Both.Compile<Counting>("() => 1 + 2")());
	}
}
