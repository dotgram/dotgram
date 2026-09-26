using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// A scope answers the same whatever else the process loads, and it answers only what it was
/// given.
/// </summary>
/// <remarks>
/// <para>
/// This is the gate for <c>ResolutionScope</c> (2026-09-26). Both halves are needed: that a scope
/// is stable proves nothing on its own unless what it replaced can be shown to move, because a
/// test that passes under both behaviours measures neither. The moving half is
/// <c>LoadOrderTests</c>, which runs in a process of its own because this assembly has loaded
/// everything it references before any test here begins.
/// </para>
/// <para>
/// What moves an answer is the same thing that cost a day on 2026-09-25:
/// <c>GeneratorDriverTests</c> compiles a class into the GLOBAL namespace and loads it, and from
/// that moment a bare name of that spelling means something else. Here it is done on purpose
/// rather than met by accident.
/// </para>
/// </remarks>
public sealed class ScopeDeterminismTests
{
	delegate object Reading();

	static readonly System.Reflection.Assembly Here = typeof(ScopeDeterminismTests).Assembly;

	/// <summary>A grammar of one rule in a class of that name, declaring no namespace at all.</summary>
	static string Source(string name)
	{
		return $$"""
		[DotGram.Gram("A = \"x\"")]
		public partial class {{name}}
		{
			public static string Where()
			{
				return "loaded";
			}
		}
		""";
	}

	[Fact]
	public void A_scope_answers_the_same_after_another_assembly_is_loaded()
	{
		var scope  = ResolutionScope.Around(Here);
		var text   = "() => Shadowing.Where()";
		var before = ExpressionParser.TryParse(text, scope);

		// The very thing that made an answer move: a class of that name, in the global namespace,
		// compiled and loaded now.
		Assert.NotNull(GeneratorDriverTests.Build(Source("Shadowing")).GetType("Shadowing"));

		var after = ExpressionParser.TryParse(text, scope);

		Assert.False(before.IsSuccess, "nothing this assembly references declares a global `Shadowing`.");
		Assert.Equal(before.IsSuccess, after.IsSuccess);
		Assert.Equal(before.Error, after.Error);
	}

	[Fact]
	public void A_scope_told_about_an_assembly_finds_it_and_goes_on_finding_it()
	{
		var loaded = GeneratorDriverTests.Build(Source("Shown")).GetType("Shown")!;
		var scope  = ResolutionScope.Of(Here, loaded.Assembly);

		Assert.Equal("loaded", ExpressionParser.Compile<Reading>("() => Shown.Where()", scope)());

		// A second assembly loaded afterwards changes nothing about this scope.
		Assert.NotNull(GeneratorDriverTests.Build(Source("Beside")).GetType("Beside"));

		Assert.Equal("loaded", ExpressionParser.Compile<Reading>("() => Shown.Where()", scope)());
	}

	[Fact]
	public void Around_is_one_scope_for_one_assembly()
	{
		// Identity and not equality: a scope is what every cache inside the parser is keyed by,
		// and a fresh instance for the same assembly would empty all of them on every call.
		Assert.Same(ResolutionScope.Around(Here), ResolutionScope.Around(Here));
	}

	[Fact]
	public void A_scope_carries_the_caller_and_what_it_references()
	{
		var scope = ResolutionScope.Around(Here);

		Assert.Same(Here, scope.Caller);
		Assert.Same(Here, scope.Assemblies[0]);
		Assert.True(scope.SeesInternals);

		// What this assembly references is in it, whether or not anything has touched it.
		Assert.Contains(typeof(ExpressionParser).Assembly, scope.Assemblies);
	}

	[Fact]
	public void Without_internals_a_text_reads_as_another_assembly_would()
	{
		const string Text = "using System; using DotGram.Tests.ExpressionLanguage; " +
			"() => SpanCalls.Length(\"1.25\".AsSpan())";

		Assert.True(ExpressionParser.TryParse(Text, ResolutionScope.Around(Here)).IsSuccess);

		Assert.False(
			ExpressionParser.TryParse(Text, ResolutionScope.Around(Here).WithoutInternals()).IsSuccess,
			"`SpanCalls` is internal to this assembly, and a scope that hides its internals must not find it.");
	}
}
