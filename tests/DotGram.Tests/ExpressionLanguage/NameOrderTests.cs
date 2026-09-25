using System;
using System.Collections.Immutable;
using System.Linq;

using DotGram.ExpressionLanguage;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Which of two types a bare name means, when the global namespace declares one and a
/// <c>using</c> brings in another.
/// </summary>
/// <remarks>
/// <para>
/// C#'s order (§7.8): a name is looked for in the enclosing namespace first, and the
/// <c>using</c>s are consulted only where it means nothing there. A text this language reads
/// declares no namespace of its own, so its enclosing namespace is the global one — and a type
/// declared there wins outright. Two <c>using</c>s that both supply the name, with nothing
/// global, is the real ambiguity and stays refused.
/// </para>
/// <para>
/// Until 2026-09-25 this counted every hit equally and refused two of them, whatever they were.
/// A text C# reads without hesitating was refused, and it surfaced as a test that failed only
/// when another test had run first: <c>GeneratorDriverTests</c> compiles classes into the GLOBAL
/// namespace and loads them, and seven of the names it uses are also declared somewhere in this
/// repository — <c>Both</c>, <c>Grammar</c>, <c>Mixed</c>, <c>Nested</c>, <c>Pairs</c>,
/// <c>Spans</c>, <c>Taken</c>.
/// </para>
/// <para>
/// Each case is held against Roslyn compiling the same text, because the specification is what
/// this claims to follow and a reading of it is not evidence. What the oracle answers, this must
/// answer.
/// </para>
/// </remarks>
public sealed class NameOrderTests
{
	delegate string Reading();

	[Fact]
	public void A_name_the_global_namespace_declares_beats_one_a_using_brings_in()
	{
		var made = ExpressionParser.Compile<Reading>(
			"using DotGram.Tests.ExpressionLanguage; () => Shadowed.Where()", typeof(NameOrderTests).Assembly);

		Assert.Equal("global", made());
		Assert.Equal("global", WhatCSharpSays("Shadowed.Where()", "using DotGram.Tests.ExpressionLanguage;"));
	}

	[Fact]
	public void Two_usings_that_both_supply_a_name_are_still_ambiguous()
	{
		var thrown = Assert.Throws<InvalidOperationException>(
			() => ExpressionParser.Compile<Reading>(
				"using DotGram.Tests.ExpressionLanguage; using DotGram.Tests.ExpressionLanguage.Elsewhere; " +
				"() => Twofold.Where()",
				typeof(NameOrderTests).Assembly));

		Assert.Contains("ambiguous", thrown.Message, StringComparison.Ordinal);

		// And C# refuses it too, in its own words (CS0104).
		Assert.Equal(
			"CS0104",
			WhatCSharpSays(
				"Twofold.Where()",
				"using DotGram.Tests.ExpressionLanguage;",
				"using DotGram.Tests.ExpressionLanguage.Elsewhere;"));
	}

	/// <summary>What Roslyn makes of the same expression: its value, or the first error's id.</summary>
	/// <remarks>
	/// Compiled with the references this assembly itself was built against, so the types the
	/// text names are the very ones the language sees.
	/// </remarks>
	static string WhatCSharpSays(string expression, params string[] usings)
	{
		var source = string.Join("\n", usings) + "\npublic static class Asked { public static string Value => " +
			expression + "; }";

		var compilation = CSharpCompilation.Create(
			"NameOrderOracle",
			[CSharpSyntaxTree.ParseText(source)],
			References(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var refused = compilation.GetDiagnostics()
			.FirstOrDefault(one => one.Severity == DiagnosticSeverity.Error);

		if (refused is not null)
			return refused.Id;

		using var stream = new System.IO.MemoryStream();

		Assert.True(compilation.Emit(stream).Success);

		return (string)System.Reflection.Assembly.Load(stream.ToArray())
			.GetType("Asked")!
			.GetProperty("Value")!
			.GetValue(null)!;
	}

	static ImmutableArray<MetadataReference> References()
	{
		return
		[
			.. AppDomain.CurrentDomain.GetAssemblies()
				.Where(static one => !one.IsDynamic && one.Location.Length > 0)
				.Select(static one => (MetadataReference)MetadataReference.CreateFromFile(one.Location)),
		];
	}
}

/// <summary>The same name inside a namespace, so that a `using` can bring it in.</summary>
public static class Shadowed
{
	public static string Where()
	{
		return "used";
	}
}

/// <summary>One of the two a `using` apiece supplies, which is the real ambiguity.</summary>
public static class Twofold
{
	public static string Where()
	{
		return "here";
	}
}
