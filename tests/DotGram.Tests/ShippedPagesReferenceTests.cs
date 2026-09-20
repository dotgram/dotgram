using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a page may name does not depend on which test ran first.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ShippedPages"/> compiles a page's block against a set of references. That set was
/// once "every assembly this process has loaded", which made it depend on load order: a referenced
/// assembly is loaded lazily, at its first use, so whether the one a page names was present
/// depended on whether an earlier test in the same process had happened to touch it. The check
/// passed by luck, and would have failed on the day an unrelated change reordered the run — where
/// it would have been looked for in that change.
/// </para>
/// <para>
/// This holds the property directly rather than the mechanism: a block naming assemblies that
/// nothing in this class touches must compile. Run alone, with the old construction, it failed with
/// "The type or namespace name 'ExpressionLanguage' does not exist in the namespace 'DotGram'" —
/// which is the failure the architect saw on twenty-three page blocks of a merged tree.
/// </para>
/// <para>
/// **Why the defect was green in one place and red in another, on the same commit.** An ordinary
/// page class loads the assembly its pages name, because it reaches for it itself —
/// <c>PageOf(typeof(ExpressionParser), …)</c> — so the check passes for whoever is looking at it.
/// Only a class naming a package it never touches can be in the wrong order, and no ordinary page
/// class has that shape. So a full run could be red where a single class was green, and a suite red
/// on one machine green on another. Anyone who cannot reproduce a red run here should look at what
/// loaded first rather than for a flake.
/// </para>
/// </remarks>
public sealed class ShippedPagesReferenceTests
{
	[Fact]
	public void A_block_naming_an_assembly_this_class_never_touches_compiles()
	{
		var said = ShippedPages.Compiles(
			"using System;\nusing DotGram.Web;\nusing DotGram.ExpressionLanguage;\n" +
			"MediaType m = MediaType.Parse(\"text/plain\");\n" +
			"var e = ExpressionParser.Parse(\"() => 1\");\n" +
			"Console.WriteLine(m);\nConsole.WriteLine(e);\n");

		Assert.True(said is null, said);
	}
}
