using System;
using System.Linq;

using DotGram.Language;

using Xunit;

namespace DotGram.Tests.Fragments;

/// <summary>
/// Every fragment the repository ships, read into the fragment tree: C#'s parser reads each one
/// whole, the standard library's among them, and the census of what is portable is held to the one
/// checked in.
/// </summary>
public sealed class FragmentCensusTests(ITestOutputHelper output)
{
	/// <summary>
	/// What the fragment tree stands on: every fragment, delimited as a build delimits it, is one
	/// whole C# expression to C#'s parser under the fixed language version — no parse error, and
	/// no text left over.
	/// </summary>
	[Fact]
	public void Every_shipped_fragment_is_one_whole_expression()
	{
		var fragments = FragmentCensus.Fragments();
		var unread    = fragments
			.Where(static found => RoslynFragmentLowering.Lower(found.Text).Refusals.Any(static one => one.Form == "Unparsable"))
			.Select(static found => $"{found.Place} @{found.Position}: {found.Text}")
			.ToList();

		Assert.True(fragments.Count > 3000, $"Only {fragments.Count} fragments were found: the census reads the wrong place.");
		Assert.True(unread.Count == 0, string.Join("\n", unread.Take(20)));
	}

	/// <summary>
	/// The library's fragments, delimited by its own scanner on the spliced path a build takes,
	/// lower like any other.
	/// </summary>
	[Fact]
	public void The_standard_library_lowers()
	{
		var library = FragmentCensus.Fragments().Where(static found => found.Group == "Std").ToList();

		Assert.NotEmpty(library);

		foreach (var found in library)
		{
			var lowered = RoslynFragmentLowering.Lower(found.Text);

			Assert.DoesNotContain(lowered.Refusals, static one => one.Form == "Unparsable");
			Assert.Equal(found.Text.Length, lowered.Text.Length);
		}
	}

	[Fact]
	public void Every_group_is_one_the_census_knows()
	{
		var unknown = FragmentCensus.Fragments().Select(static found => found.Group).Distinct().Where(static group => group.Contains('/')).ToList();

		Assert.True(unknown.Count == 0, "Grammars no group of the census holds: " + string.Join(", ", unknown));
	}

	/// <summary>
	/// The census, held to the table checked in beside this file, and the same table DotGram.Cli.Tests
	/// holds to under the tool's Roslyn: the verdict on every shipped fragment is the one on record,
	/// under the generator's floor and under the newest alike.
	/// </summary>
	[Fact]
	public void The_census_is_the_one_checked_in()
	{
		var table = FragmentCensus.Table(FragmentCensus.Fragments());

		output.WriteLine(table);

		Assert.StartsWith("| Grammars |", table, StringComparison.Ordinal);

		var difference = FragmentCensus.Disagreement(table);

		Assert.True(difference is null, difference);
	}
}
