using System;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The message layer reads a group into an array sized by its NumInGroup and checks a schema's
/// references against a mask of the tags a scope holds. Neither may trust the input further than
/// the fields that are actually there.
/// </summary>
public sealed class FixMessageLayerTests
{
	/// <summary>A hostile NumInGroup is refused before anything is sized by it.</summary>
	[Theory]
	[InlineData("2000000000")]
	[InlineData("9223372036854775807")]
	[InlineData("5")]
	public void A_group_count_beyond_the_fields_left_is_refused(string count)
	{
		var wire = FixFixtures.Wire("W", "55=ABC|262=REQ|268=" + count + "|269=0|270=1|271=1|");

		// A count sizes an array before anything is read into it, so this is recognition and
		// stays a refusal however the schema is read.
		Assert.False(FixMessages.TryParse(wire, out _, out var error));
		Assert.Equal(268, error!.Tag);
		Assert.Contains("NumInGroup", error.Reason);
	}

	/// <summary>
	/// A number answers as an integer, which is what most of the tags that answer as numbers are.
	/// </summary>
	/// <remarks>
	/// FixNumber forwards a field's members and forwarded three of four; the one it left behind
	/// was the one the sequence numbers and the lengths need. Reading RefSeqNum went through a
	/// decimal or through the string.
	/// </remarks>
	[Fact]
	public void A_number_reads_as_an_integer_and_not_only_as_a_decimal()
	{
		// A Reject: RefSeqNum and RefTagID both answer with a number, and both are integers.
		var reject = (Reject)FixFixtures.Message("35=3|49=S|56=T|34=1|52=20260920-12:00:00|45=12345|371=44|");

		Assert.True(reject.RefSeqNum!.Value.TryGetInt64(out var sequence));
		Assert.Equal(12345, sequence);
		Assert.True(reject.RefTagID!.Value.TryGetInt64(out var tag));
		Assert.Equal(44, tag);

		// And it refuses what is not a whole number rather than truncating it, which is why the
		// decimal reading stays: the two answer different questions about the same characters.
		var fractional = (Reject)FixFixtures.Message("35=3|49=S|56=T|34=1|52=20260920-12:00:00|45=1.5|");

		Assert.False(fractional.RefSeqNum!.Value.TryGetInt64(out _));
		Assert.True(fractional.RefSeqNum!.Value.TryGetDecimal(out var exact));
		Assert.Equal(1.5m, exact);
	}

	/// <summary>The mask answers for standard tags; any other tag is looked for among the fields.</summary>
	[Fact]
	public void A_tag_the_mask_does_not_cover_is_looked_for()
	{
		var soh   = (char)1;
		var scope = new FixFieldSet("55=A" + soh + "5000=x" + soh, [new FixNode(55, 0, 3, 1), new FixNode(5000, 5, 10, 1)]);

		Span<ulong> seen = stackalloc ulong[15];

		seen[55 >> 6] |= 1UL << (55 & 63);

		Assert.True(FixRules.Has(scope, seen, 55));
		Assert.False(FixRules.Has(scope, seen, 56));
		Assert.True(FixRules.Has(scope, seen, 5000));
		Assert.False(FixRules.Has(scope, seen, 5001));
		Assert.False(FixRules.Has(scope, seen, 957));
	}
}
