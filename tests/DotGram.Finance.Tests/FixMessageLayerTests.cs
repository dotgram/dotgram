using System;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What the message layer answers about a field once the reading is done.
/// </summary>
/// <remarks>
/// A hostile NumInGroup used to be refused here, because the count sized an array before anything
/// was read into it. Nothing is sized by a count any more: the fields are one flat list and a
/// group is the entries built from them, so a count that disagrees with what follows is a finding
/// of the schema and not a refusal of the reading.
/// </remarks>
public sealed class FixMessageLayerTests
{
	/// <summary>
	/// A number answers as an integer, which is what most of the tags that answer as numbers are.
	/// </summary>
	[Fact]
	public void A_sequence_number_reads_as_a_whole_number()
	{
		// A FixMessage.Reject: RefSeqNum and RefTagID both answer with a number, and both are integers.
		var reject = (FixMessage.Reject)FixFixtures.Message("35=3|49=S|56=T|34=1|52=20260920-12:00:00|45=12345|371=44|");

		Assert.Equal(12345L, reject.RefSeqNum!.Value);
		Assert.Equal(44L,    reject.RefTagID!.Value);
		Assert.True(reject.RefSeqNum.IsValid);

		// And what is not a whole number is not read as one: the field is there, and says so.
		var fractional = (FixMessage.Reject)FixFixtures.Message("35=3|49=S|56=T|34=1|52=20260920-12:00:00|45=1.5|");

		Assert.NotNull(fractional.RefSeqNum);
		Assert.False(fractional.RefSeqNum.IsValid);
	}
}
