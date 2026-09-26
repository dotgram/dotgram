using System;
using System.Linq;
using System.Reflection;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// The namespaces every text gets, and the switch that leaves them out.
/// </summary>
/// <remarks>
/// <para>
/// Igor, 2026-09-26: the set the older Visual Studio template gives a new project. They stand as
/// if the text had written them before its own `using`s and are PEERS of them, which is what C#
/// does with a global using — a name two of them both give is ambiguous where it is used, and it
/// does not matter which of the two the text wrote (CS0104, asked of Roslyn).
/// </para>
/// <para>
/// They are recorded without being looked for, which is two requirements at once: a namespace the
/// scope does not hold is simply absent rather than a refusal, and a text that names no type at
/// all still loads nothing (<see cref="LazyClosureTests"/>). Checking five namespaces at every
/// reading would have walked the closure for every text there is.
/// </para>
/// </remarks>
public sealed class DefaultImportTests
{
	static readonly Assembly Caller = typeof(DefaultImportTests).Assembly;

	delegate int Counting();

	delegate string Naming();

	[Fact]
	public void The_defaults_are_the_five_a_new_project_gets()
	{
		// The assertion, not a restatement of it: this is where the set is written down for a
		// reader, and the property holds the same list a text is read with.
		Assert.Equal(
			["System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks"],
			ResolutionScope.DefaultImports);
	}

	[Theory]
	[InlineData("() => System.Math.Sign(-2)")]
	[InlineData("() => Math.Sign(-2)")]
	[InlineData("() => new System.Collections.Generic.List<int>().Count")]
	[InlineData("() => new List<int>().Count")]
	[InlineData("() => new StringBuilder().Length")]
	[InlineData("() => new List<int>().Any()")]
	public void Each_default_resolves_a_short_name(string text)
	{
		Assert.True(Both.TryParse(text).IsSuccess, text);
	}

	[Fact]
	public void A_task_is_named_without_a_using()
	{
		Assert.True(Both.TryParse("() => System.Threading.Tasks.Task.CompletedTask").IsSuccess);
		Assert.True(Both.TryParse("() => Task.CompletedTask").IsSuccess);
	}

	[Fact]
	public void Without_them_a_short_name_is_not_there()
	{
		var bare = ResolutionScope.Around(Caller).WithoutDefaultImports();

		Assert.False(
			ExpressionParser.TryParse("() => new List<int>().Count", bare).IsSuccess,
			"a default namespace answered a scope that was told to leave them out.");

		// And the text may still say it itself, which is the point of leaving them out rather
		// than forbidding them.
		Assert.True(
			ExpressionParser.TryParse(
				"using System.Collections.Generic; () => new List<int>().Count", bare).IsSuccess);
	}

	[Fact]
	public void A_name_written_whole_needs_no_default()
	{
		var bare = ResolutionScope.Around(Caller).WithoutDefaultImports();

		Assert.True(ExpressionParser.TryParse("() => System.Math.Sign(-2)", bare).IsSuccess);
	}

	[Fact]
	public void The_switch_shares_the_closure_and_is_the_same_scope_either_way()
	{
		var scope = ResolutionScope.Around(Caller);
		var bare  = scope.WithoutDefaultImports();

		Assert.NotSame(scope, bare);
		Assert.False(bare.ImportsDefault);
		Assert.True(scope.ImportsDefault);

		// Made once and kept: a scope is what every cache inside the parser is keyed by.
		Assert.Same(bare, scope.WithoutDefaultImports());

		// And the two shaping forms compose to the same scope whichever order they are written.
		Assert.Same(
			scope.WithoutInternals().WithoutDefaultImports(),
			scope.WithoutDefaultImports().WithoutInternals());

		// Where it looks is unchanged, so the closure is the one thing they share.
		Assert.Equal(scope.Assemblies, bare.Assemblies);
	}

	[Fact]
	public void A_text_naming_no_type_still_loads_nothing()
	{
		// The defaults must not force the walk (42c6d5b0): they are recorded and not looked for.
		var scope = ResolutionScope.Of(Caller, typeof(ExpressionParser).Assembly);
		var held  = typeof(ResolutionScope)
			.GetField("_assemblies", BindingFlags.NonPublic | BindingFlags.Instance)!
			.GetValue(scope)!;

		Assert.True(ExpressionParser.TryParse("(int x) => (x + 1) * 2", scope).IsSuccess);

		Assert.False(
			(bool)held.GetType().GetProperty("IsValueCreated")!.GetValue(held)!,
			"the namespaces every text gets walked the closure for a text that names no type.");
	}

	[Fact]
	public void A_text_may_write_a_default_itself()
	{
		// The same namespace, recorded once. C# warns there (CS0105); this language has no
		// warnings channel, so it says nothing — a difference, and a deliberate one.
		Assert.Equal(2, Both.Compile<Counting>("using System; () => Math.Sign(2) + 1")());
	}

	[Fact]
	public void A_name_a_default_and_a_written_using_both_give_is_ambiguous()
	{
		// C#'s CS0104 counts a global using and a written one alike, which was asked of Roslyn.
		var thrown = Assert.Throws<InvalidOperationException>(
			() => Both.Parse("using DotGram.Tests.ExpressionLanguage.Against; () => Convert.Value"));

		Assert.Contains("ambiguous", thrown.Message, StringComparison.Ordinal);
	}
}
