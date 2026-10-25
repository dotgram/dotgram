using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotGram.Examples.Feeds;

using Xunit;

namespace DotGram.FeedLoad.Tests;

/// <summary>
/// A feed read from a list of lines, from a sequence and from a reader says the same, wherever the
/// streamed reading's window happens to end.
/// </summary>
/// <remarks>
/// The window ends at a place that depends on how much comes before it, so the same records are read
/// with a growing number of unreadable lines in front of them: each shift moves every later record by
/// a line and by a few characters. The reader and the sequence must answer as the list does for each.
/// A fresh buffer is not asked for: the shifts are what move the edge, and a first parse that grows
/// the pooled buffer is one of the many.
/// </remarks>
public sealed partial class EdgeShiftTests
{
	const int Records = 10_000;

	public static TheoryData<FeedFormat> Formats { get; } = [FeedFormat.Pipe, FeedFormat.Csv, FeedFormat.FixedWidth];

	[Theory]
	[MemberData(nameof(Formats))]
	public void A_reader_and_a_sequence_read_what_a_list_reads_at_every_shift(FeedFormat format)
	{
		using var writer = new StringWriter();

		TradingFeedGenerator.Write(writer, format, Records, Records / 100, seed: 1);

		var lines = new List<string>();

		using (var all = new StringReader(writer.ToString()))
			while (all.ReadLine() is { } line)
				lines.Add(line);

		for (var shift = 0; shift < 130; shift++)
		{
			var shifted = new List<string>(lines.Count + shift);

			shifted.Add(lines[0]);

			for (var junk = 0; junk < shift; junk++)
				shifted.Add(new string('x', 1 + (junk * 7 + shift) % 61));

			shifted.AddRange(lines.Skip(1));

			var expected = Describe(format, shifted, null, null);

			Assert.Equal(expected, Describe(format, null, shifted.AsEnumerable(), null));
			Assert.Equal(expected, Describe(format, null, null, new StringReader(string.Join("\n", shifted) + "\n")));
		}
	}

	[GeneratedRegex(@"Message = Expected .*? at (\d+:\d+)\.")]
	private static partial Regex Expectation();

	static string Describe(FeedFormat format, IReadOnlyList<string>? list, IEnumerable<string>? sequence, TextReader? reader)
	{
		IEnumerable<object> read = (format, list, sequence, reader) switch
		{
			(FeedFormat.Pipe, { } one, _, _)  => EodTrades.Read(one),
			(FeedFormat.Pipe, _, { } one, _)  => EodTrades.Read(one),
			(FeedFormat.Pipe, _, _, { } one)  => EodTrades.Read(one),
			(FeedFormat.Csv, { } one, _, _)   => CorporateActions.Read(one),
			(FeedFormat.Csv, _, { } one, _)   => CorporateActions.Read(one),
			(FeedFormat.Csv, _, _, { } one)   => CorporateActions.Read(one),
			(_, { } one, _, _)                => Confirmations.Read(one),
			(_, _, { } one, _)                => Confirmations.Read(one),
			(_, _, _, { } one)                => Confirmations.Read(one),
			_                                 => throw new ArgumentException("No input."),
		};

		// What a refusal expected is not held to: a whole reading says what could have followed the
		// repetition as well, and a streamed one does not look past it. Where it was is.
		return string.Join("\n", read.Select(static one => Expectation().Replace(one.ToString()!, "Message = refused at $1")));
	}
}
