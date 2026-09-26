using System;
using System.Reflection;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// A scope walks its assemblies at the first question that needs them, and a text that asks none
/// never makes it walk.
/// </summary>
/// <remarks>
/// <para>
/// Making a scope used to walk the caller's reference closure and load it: 23 ms and 145
/// assemblies over the benchmark assembly, which doubled a consumer's first call (21.0 ms to 40.7
/// on the stand's `el/floor`). Counted rather than guessed, three texts of four ask the type
/// tables NOTHING — a keyword type and a member access never reach them — so for those the whole
/// of that was paid for nothing.
/// </para>
/// <para>
/// What the closure holds does not change: it is derived from the names an assembly references,
/// and when it is derived changes nothing about what it contains. So this asserts both halves —
/// that a text which asks nothing leaves the scope unwalked, and that a text which asks gets the
/// same answers either way, including the one answer a scope's BREADTH decides, which is that a
/// name two namespaces both give is ambiguous rather than the first of them.
/// </para>
/// </remarks>
public sealed class LazyClosureTests
{
	static readonly Assembly Caller = typeof(LazyClosureTests).Assembly;

	/// <summary>Whether that scope has walked its closure yet, read the way a measurement reads it.</summary>
	/// <remarks>
	/// By reflection and on purpose, as <see cref="VocabularyTests"/> reads the caches: what is
	/// asserted is WHEN the walk happens, and a member a consumer could see would be a member a
	/// consumer could come to depend on.
	/// </remarks>
	static bool Walked(ResolutionScope scope)
	{
		var held = typeof(ResolutionScope)
			.GetField("_assemblies", BindingFlags.NonPublic | BindingFlags.Instance)!
			.GetValue(scope)!;

		return (bool)held.GetType().GetProperty("IsValueCreated")!.GetValue(held)!;
	}

	[Theory]
	[InlineData("(int x) => x")]
	[InlineData("(int x) => (x + 1) * 2")]
	[InlineData("(int x) => System.Math.Abs(x) + 1")]
	[InlineData("using System; (string s) => s.Length > 0 ? s.Trim() : s")]
	public void A_scope_is_not_walked_before_it_is_asked(string text)
	{
		// A scope of its own each time: `Around` keeps one per assembly, and a scope another test
		// has already asked something of is a scope that has walked.
		var scope = ResolutionScope.Of(Caller, typeof(ExpressionParser).Assembly);

		Assert.False(Walked(scope), "a scope walked its closure before anything asked it anything.");

		// And the text still reads. Which of these DOES walk it afterwards is not the point and is
		// allowed to change; what must hold is that MAKING the scope did not, so a host that never
		// names a type never pays for a walk.
		Assert.True(ExpressionParser.TryParse(text, scope).IsSuccess, text);
	}

	[Fact]
	public void A_text_that_names_no_type_never_walks_it()
	{
		var scope = ResolutionScope.Of(Caller, typeof(ExpressionParser).Assembly);

		Assert.True(ExpressionParser.TryParse("(int x) => (x + 1) * 2", scope).IsSuccess);

		Assert.False(
			Walked(scope),
			"a text naming no type walked the closure; the 23 ms it costs was paid for nothing.");
	}

	[Fact]
	public void Reading_the_assemblies_is_what_walks_it()
	{
		var scope = ResolutionScope.Of(Caller, typeof(ExpressionParser).Assembly);

		Assert.False(Walked(scope));
		Assert.NotEmpty(scope.Assemblies);
		Assert.True(Walked(scope), "reading the assemblies did not walk the closure.");
	}

	/// <summary>The answers, with the walk already done and with it left to the reading.</summary>
	[Theory]
	[InlineData("(int x) => System.Math.Abs(x) + 1")]
	[InlineData("using System; (int x) => Convert.ToString(x)")]
	[InlineData("(int x) => System.Nowhere.At.All(x)")]
	[InlineData("(int x) => (Nowhere)x")]
	[InlineData("using DotGram.Tests.ExpressionLanguage; using DotGram.Tests.ExpressionLanguage.Elsewhere; () => Twofold.Where()")]
	[InlineData("using DotGram.Tests.ExpressionLanguage.Elsewhere; () => Twofold.Where()")]
	public void A_text_that_names_types_answers_the_same_either_way(string text)
	{
		var walked = ResolutionScope.Of(Caller, typeof(ExpressionParser).Assembly);
		var lazy   = ResolutionScope.Of(Caller, typeof(ExpressionParser).Assembly);

		// One of them is made to walk before it is asked anything; the other is left to walk
		// wherever the reading needs it.
		Assert.NotEmpty(walked.Assemblies);
		Assert.False(Walked(lazy));

		Assert.Equal(Answer(text, walked), Answer(text, lazy));
	}

	/// <summary>What that scope makes of that text: the tree it built, or how it refused it.</summary>
	static string Answer(string text, ResolutionScope scope)
	{
		try
		{
			var match = ExpressionParser.TryParse(text, scope);

			return match.IsSuccess ? match.Value!.ToString()! : $"refused at {match.Position}: {match.Error}";
		}
		catch (Exception thrown) when (thrown is FormatException or InvalidOperationException)
		{
			return thrown.GetType().Name + ": " + thrown.Message;
		}
	}
}
