using System.Collections.Generic;
using System.Linq;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The code the package's own pages open with, run rather than read.
/// </summary>
/// <remarks>
/// Both README.md and SKILL.md opened their length/data example with a dictionary holding
/// <c>[95] = 96</c>, which D27 turned into an <see cref="System.ArgumentException"/> — so the two
/// pages a consumer reads first shipped a first call that throws. The pages were written before
/// the rule changed and nothing held them to it. These are the same calls, compiled: a change to
/// what the package accepts now breaks a test instead of a reader's first five minutes.
/// </remarks>
public sealed class ShippedExampleTests
{
	[Fact]
	public void The_length_data_example_of_both_pages_runs()
	{
		var options = new FixFieldOptions(new Dictionary<int, int>
		{
			[5000] = 5001,
		});

		var custom = FixParser.ParseLog("5000=3 | 5001=a|b | ", options);

		Assert.Equal([5001], custom.Select(field => field.Tag));
	}

	[Fact]
	public void The_standard_pairs_still_hold_beside_a_declared_one()
	{
		// What the comment beside those examples now claims, which is the half a reader is most
		// likely to doubt: declaring your own does not cost you the standard's.
		var options = new FixFieldOptions(new Dictionary<int, int> { [5000] = 5001 });

		var standard = FixParser.ParseLog("95=3 | 96=a|b | ", options);

		Assert.Equal([96], standard.Select(field => field.Tag));
	}

	[Fact]
	public void Redeclaring_a_standard_pair_is_refused_with_the_tag_named()
	{
		var refused = Assert.Throws<System.ArgumentException>(
			() => new FixFieldOptions(new Dictionary<int, int> { [95] = 96 }));

		Assert.Contains("95", refused.Message);
	}

	[Fact]
	public void The_opening_example_of_the_readme_runs()
	{
		var fields = FixParser.ParseLog("55=ABC|38=100|");

		Assert.Equal([55, 38], fields.Select(field => field.Tag));
	}
}
