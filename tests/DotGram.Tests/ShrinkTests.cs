using System;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The shrinker shrinks, and converges on what the predicate actually says.
/// </summary>
public sealed class ShrinkTests
{
	[Fact]
	public void Lines_keep_only_what_the_predicate_needs()
	{
		var shrunk = Shrink.Lines(
			"noise\nkeep me\nmore noise\nalso keep\nnoise again",
			text => text.Contains("keep me", StringComparison.Ordinal) &&
				text.Contains("also keep", StringComparison.Ordinal));

		Assert.Equal("keep me\nalso keep", shrunk);
	}

	[Fact]
	public void Chars_converge_to_the_minimum()
	{
		var shrunk = Shrink.Chars(
			"xxaxxbxxcxx",
			text => text.Contains('a') && text.Contains('b') && text.Contains('c'));

		Assert.Equal("abc", shrunk);
	}

	[Fact]
	public void An_uninteresting_start_is_refused()
	{
		// A predicate that never fires would "shrink" to itself and report success;
		// refusing up front is what tells the caller their predicate is wrong.
		Assert.Throws<ArgumentException>(
			() => Shrink.Chars("abc", text => text.Contains('z')));
	}

	/// <summary>
	/// The tool used the way it exists to be used: a grammar defect, shrunk against the
	/// real compiler.
	/// </summary>
	[Fact]
	public void A_compiler_defect_shrinks_to_its_essence()
	{
		// The predicate: the diagnostic being hunted, identified by what it says and not
		// just by its id — GRAM2005 has five sites, and a predicate that names only the
		// id converges on whichever of them is cheapest to reach. The first draft of this
		// test did exactly that, which is the mistake the class doc warns about, caught
		// by its own test.
		static bool Reports(string grammar)
		{
			try
			{
				var result = Grammar.GramCompiler.Compile(
					grammar,
					new Grammar.GramCompilerOptions
					{
						ClassName = "Grammar",
						CSharpScanner = Generation.RoslynCSharpScanner.Instance,
					});

				foreach (var diagnostic in result.Diagnostics)
					if (diagnostic.Id == "GRAM2005" &&
						diagnostic.Message.Contains("can never match", StringComparison.Ordinal))
						return true;
			}
			catch
			{
				return false;
			}

			return false;
		}

		var shrunk = Shrink.Chars(
			Shrink.Lines(
				"// a comment that contributes nothing\n" +
				"Helper = 'h' & 'e' & 'l' & 'p'\n" +
				"Start = Helper? & ['a'..'z']{5,2} & Helper\n" +
				"Tail = 'x'\n" +
				"parse Start",
				Reports),
			Reports);

		// What the defect actually is: a bound written backwards. Everything else went —
		// the helper rule, the publication, even the closing brace, since the parser
		// reports the bound before missing it. Seven characters: S=e{5,2
		Assert.Contains("{5,2", shrunk, StringComparison.Ordinal);
		Assert.True(shrunk.Length <= 10, $"expected the essence, got {shrunk.Length} chars: {shrunk}");
	}
}
