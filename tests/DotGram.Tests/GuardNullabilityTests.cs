using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A <c>when</c> is handed its capture as nullable exactly where a route can reach the guard without
/// having written it, and not otherwise.
/// </summary>
/// <remarks>
/// <para>
/// The parameter's type used to come from two proxies for that question, either of which was enough
/// on its own: the member's <c>IsOptional</c>, which is false only where EVERY alternative of the
/// rule writes the name, and a comparison of the slots visible at the guard against all the member's
/// slots. So a guard reading a capture of its own alternative was nullable in any rule that had a
/// second alternative at all, and an author who wrote <c>when @(a.Length > 0)</c> got CS8602 from the
/// generated code under the <c>#nullable enable</c> it writes itself — a warning, and an error in a
/// consumer's build with warnings as errors.
/// </para>
/// <para>
/// <b>The two rows that settle it are <c>Sibling</c> and <c>OnThePath</c>.</b> Both had
/// <c>IsOptional</c> true with one slot, all of it visible — indistinguishable by either proxy — and
/// they must be answered differently: the first writes the capture on every route to its guard, the
/// second has a route that skips it. No combination of the old terms could have been right, which is
/// why this is a definite-assignment question over the alternative's prefix
/// (<c>GrammarNormalizer.WritesBefore</c>) and not a repair of either of them.
/// </para>
/// <para>
/// <b>These assert the emitted signature, not merely that the code compiles.</b> A test that only
/// compiled would pass on a nullable parameter as happily as on a non-nullable one, since nothing in
/// the emitted guard dereferences it — the author's condition does, and the author is not in the
/// fixture. The signature is the thing a consumer's own code is then written against.
/// </para>
/// <para>
/// Where the answer is "definitely assigned" the reader stops hedging: it asserts the capture is
/// present (<c>Debug.Assert</c>) and reads it unconditionally, so an unsound yes here is a wrong
/// read and not a warning. That is what makes <c>WritesBefore</c> conservative where a node instance
/// could be reached by more than one route.
/// </para>
/// </remarks>
public sealed class GuardNullabilityTests
{
	/// <summary>The capture is written by one alternative of two, so the MEMBER is optional.</summary>
	const string Sibling =
		"Start : @string\n" +
		"	= '(' & inner: Start & when @(inner.Length > 0) & ')' => @(\"[\" + inner + \"]\")\n" +
		"	| leaf: Word                                          => @(leaf)\n" +
		"Word : @string = t: ['a'..'z']+ => @(t)\n" +
		"parse Start\n";

	/// <summary>Written by both, so the member has two slots and only one is visible at the guard.</summary>
	const string Every =
		"Start : @string\n" +
		"	= '(' & inner: Start & when @(inner.Length > 0) & ')' => @(\"[\" + inner + \"]\")\n" +
		"	| inner: Word                                         => @(inner)\n" +
		"Word : @string = t: ['a'..'z']+ => @(t)\n" +
		"parse Start\n";

	/// <summary>One alternative and one slot: the shape that was already non-nullable, as a control.</summary>
	const string Single =
		"Start : @string = '(' & inner: Word & when @(inner.Length > 0) & ')' => @(inner)\n" +
		"Word : @string = t: ['a'..'z']+ => @(t)\n" +
		"parse Start\n";

	/// <summary>And an option ON THE PATH, where a route really does reach the guard with nothing.</summary>
	const string OnThePath =
		"Start : @string = ('[' & inner: Word)? & when @(!string.IsNullOrEmpty(inner)) & ']' => @(inner ?? \"\")\n" +
		"Word : @string = t: ['a'..'z']+ => @(t)\n" +
		"parse Start\n";

	/// <summary>
	/// A value type, where the change is a break rather than an annotation: <c>int?</c> to
	/// <c>int</c>. It is also the only shape in which an argument that disagreed with the
	/// parameter would fail to compile, which is why these are compiled and not only read.
	/// </summary>
	const string Valued =
		"Inner : @int = d: ['0'..'9']+ => @(int.Parse(d))\n" +
		"Start : @int = '(' & n: Inner & when @(n > 0) & ')' => @(n)\n" +
		"parse Start\n";

	/// <summary>And the same with the capture optional on the path, which stays <c>int?</c>.</summary>
	const string ValuedOnThePath =
		"Inner : @int = d: ['0'..'9']+ => @(int.Parse(d))\n" +
		"Start : @int = ('[' & n: Inner)? & when @(n.GetValueOrDefault() >= 0) & ']' => @(n.GetValueOrDefault())\n" +
		"parse Start\n";

	[Theory]
	[InlineData("Sibling",   Sibling,   "string inner")]
	[InlineData("Every",     Every,     "string inner")]
	[InlineData("Single",    Single,    "string inner")]
	[InlineData("OnThePath", OnThePath, "string? inner")]
	[InlineData("Valued",          Valued,          "int n")]
	[InlineData("ValuedOnThePath", ValuedOnThePath, "int? n")]
	public void A_guard_is_handed_its_capture_nullable_only_where_a_route_can_skip_it(
		string name, string grammar, string expected)
	{
		foreach (var direct in new[] { false, true })
		{
			var result = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName     = name + (direct ? "Direct" : "Engine"),
				Namespace     = "Guarded",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Direct        = direct,
			});

			Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

			var source = Assert.Single(result.Sources).Text;

			// EVERY guard the rendering emits, not the first of them. A rendering writes one helper
			// per site, and reading only `_Guard0` passed this file while the reader-path guards were
			// still handing their captures nullable: the two paths emit the guard separately.
			var signatures = source
				.Split('\n')
				.Select(static line => line.Trim())
				.Where(static line => line.StartsWith("static bool Recognize_", StringComparison.Ordinal) &&
					line.Contains("_Guard", StringComparison.Ordinal))
				.ToList();

			Assert.NotEmpty(signatures);

			foreach (var signature in signatures)
				Assert.True(
					signature.Contains("(" + expected + ")", StringComparison.Ordinal),
					$"{name}, {(direct ? "direct" : "engine")}: every guard should take " +
					$"'{expected}', and one takes: {signature}");

			// The parameter is half of it. A guard handed an argument the parameter cannot take is
			// a signature that reads correctly and a file that does not build, and nothing above
			// would have said so — which for a value type is the whole of the change.
			EmittedCode.Compile(source, name + (direct ? "Direct" : "Engine"), "Guarded");
		}
	}
}
