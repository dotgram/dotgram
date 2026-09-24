using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Finance.Fix44;

using DotGram.Tests;

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
		var options = new FixContext { LengthDataPairs = new Dictionary<FixTag, FixTag>
		{
			[(FixTag)5000] = (FixTag)5001,
		} };

		var custom = FixParser.ParseFields("5000=3 | 5001=a|b | ", (options ?? new FixContext()) with { Framing = FixFraming.Log });

		Assert.Equal([5000, 5001], custom.Select(field => (int)field.Tag));
	}

	[Fact]
	public void The_standard_pairs_still_hold_beside_a_declared_one()
	{
		// What the comment beside those examples now claims, which is the half a reader is most
		// likely to doubt: declaring your own does not cost you the standard's.
		var options = new FixContext { LengthDataPairs = new Dictionary<FixTag, FixTag> { [(FixTag)5000] = (FixTag)5001 } };

		var standard = FixParser.ParseFields("95=3 | 96=a|b | ", (options ?? new FixContext()) with { Framing = FixFraming.Log });

		Assert.Equal([95, 96], standard.Select(field => (int)field.Tag));
	}

	// ── The pages themselves, compiled as they are written ──────────────────────

	/// <summary>
	/// Every fenced block of both pages compiles on its own, with the usings it shows and no
	/// others (D56).
	/// </summary>
	/// <remarks>
	/// The tests above run the examples, and they run inside a project where implicit usings are
	/// on — so a page that forgets <c>using System;</c> passes them. This package ships for
	/// <c>netstandard2.0</c> as well, where there are none, and a reader aiming at the floor is
	/// the reader the floor exists for.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Pages))]
	public void Every_block_of_a_page_compiles_as_it_is_written(string page, int block)
	{
		var blocks = ShippedPages.Blocks(page);
		var code   = ShippedPages.Inherited(blocks, block) + blocks[block];

		if (Fragments.TryGetValue((Path.GetFileName(page), block), out var why))
		{
			Assert.NotNull(why);

			return;
		}

		var said = ShippedPages.Compiles(code);

		Assert.True(said is null, said + Environment.NewLine + "----" + Environment.NewLine + code);
	}

	public static TheoryData<string, int> Pages => ShippedPages.Every(
		ShippedPages.PageOf(typeof(FixField), "README.md"),
		ShippedPages.PageOf(typeof(FixField), "SKILL.md"));

	/// <summary>The blocks that are not files of their own, and why each one is not.</summary>
	static readonly Dictionary<(string Page, int Block), string> Fragments = new();

	[Fact]
	public void The_opening_example_of_the_readme_runs()
	{
		var fields = FixParser.ParseFields("55=ABC|38=100|", FixContext.WithLogFraming);

		Assert.Equal([55, 38], fields.Select(field => (int)field.Tag));
	}
}
