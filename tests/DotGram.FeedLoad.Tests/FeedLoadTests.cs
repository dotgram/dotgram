using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Examples.Feeds;

using Xunit;

namespace DotGram.FeedLoad.Tests;

/// <summary>
/// The program of docs/feeds.md run end to end for each of its feeds: written, read by the grammar,
/// loaded into a temporary SQLite file, and every line accounted for.
/// </summary>
public sealed class FeedLoadTests
{
	/// <summary>The name of the environment variable that sets the size; the syntax of <c>--records</c>: 10k, 1M, 10M.</summary>
	const string RecordsVariable = "DOTGRAM_FEEDLOAD_RECORDS";

	/// <summary>Small enough for every run of the suite; <c>DOTGRAM_FEEDLOAD_RECORDS=10M</c> asks for the full size.</summary>
	const long DefaultRecords = 10_000;

	public static TheoryData<FeedFormat> Formats { get; } = [FeedFormat.Pipe, FeedFormat.Csv, FeedFormat.FixedWidth];

	[Theory]
	[MemberData(nameof(Formats))]
	public void EveryLineIsLoadedOrRejected(FeedFormat format)
	{
		var records = Records();

		// The command line's one bad line in 5,000, but never fewer than enough to meet every category.
		var bad = Math.Max(records / 5000, 40);

		var options = new Options(format, records, bad, 1, null, null, false);
		var summary = FeedLoader.Run(options, TextWriter.Null);

		Assert.NotNull(summary.Written);
		Assert.Equal(records, summary.Written.Records);
		Assert.Equal(bad, summary.Written.Bad);

		// Nothing lost and nothing counted twice.
		Assert.Equal(summary.Written.Records, summary.Loaded + summary.Rejected);
		Assert.Equal(summary.Written.Bad, summary.Rejected);

		// The rejects are of the kinds the generator wrote, as many of each.
		Assert.Equal(
			summary.Written.BadByCategory.OrderBy(static one => one.Key, StringComparer.Ordinal),
			summary.RejectedByCategory.OrderBy(static one => one.Key, StringComparer.Ordinal));

		// The trailer counts every detail line, the bad ones too.
		Assert.Equal(summary.Written.Records, summary.Trailer);
		Assert.True(summary.TrailerMatches);
	}

	static long Records()
	{
		var text = Environment.GetEnvironmentVariable(RecordsVariable);

		return string.IsNullOrWhiteSpace(text) ? DefaultRecords : Options.Count(text.Trim());
	}
}
