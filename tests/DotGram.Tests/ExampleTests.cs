using System;

using DotGram.Examples;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Runs the examples in <c>examples/DotGram.Examples</c>.
/// </summary>
/// <remarks>
/// The examples are a project of their own with no test framework in it, so that they
/// can be copied as they stand. This is the only thing that runs them, and it is where
/// the assertions live — an example that stopped working would otherwise be found by
/// whoever copied it.
/// <para>
/// The generator runs over that project during this build, so a member it stopped
/// producing fails the build rather than a test.
/// </para>
/// </remarks>
public sealed class ExampleTests
{
	// ── The URL parser ───────────────────────────────────────────────────────────

	[Theory]
	[InlineData("https://example.com",              true)]
	[InlineData("https://user@example.com:8080/a",  true)]
	[InlineData("http://192.168.0.1/x?q=1#top",     true)]
	[InlineData("example.com",                      false)]
	[InlineData("gopher://example.com",             false)]
	[InlineData("https://exa mple.com",             false)]
	public void Is_url(string text, bool expected) => Assert.Equal(expected, Links.IsUrl(text));

	[Theory]
	[InlineData("https://example.com:8080", 8080)]
	[InlineData("https://example.com",       443)]
	[InlineData("http://example.com",         80)]
	[InlineData("ftp://example.com",          80)]
	public void Port_of(string url, int expected) => Assert.Equal(expected, Links.PortOf(url));

	[Fact]
	public void Hosts_in_prose() =>
		Assert.Equal(
			["a.io", "b.io"],
			Links.HostsIn("see http://a.io and https://b.io/c and http://a.io/again"));

	[Fact]
	public void Describe_names_every_part_and_marks_the_missing_ones()
	{
		Assert.Equal(
			"""
			scheme   https
			user     bob
			host     example.com
			port     8080
			path     /a/b
			query    q=1
			fragment top

			""".Replace("\r\n", "\n"),
			Links.Describe("https://bob@example.com:8080/a/b?q=1#top"));

		Assert.Contains("port     —", Links.Describe("http://example.com"));
	}

	[Fact]
	public void Describe_throws_the_type_int_Parse_throws() =>
		Assert.Throws<FormatException>(static () => Links.Describe("not a url"));

	[Fact]
	public void Parse_demands_the_whole_input_and_says_where_it_stopped()
	{
		const string text = "https://a.io and then some prose";

		// `parse` is the rule with end-of-input on the end of it, compiled into one
		// machine — so failing to reach the end sends the match back for another way
		// through the rule, and only then gives up.
		var match = Links.TryParseUrl(text);

		Assert.False(match.IsSuccess);
		Assert.NotNull(match.Error);
		Assert.Equal(12, match.Position);         // the space, where it stopped being a URL

		// Scanning for one inside other text is what `find` is for.
		Assert.Equal(["a.io"], Links.AllUrls(text).Select(m => m.Value!.Authority.Host));
	}

	// ── The calculator ───────────────────────────────────────────────────────────

	[Theory]
	[InlineData("1+2+3",         6)]
	[InlineData("1-2-3",        -4)]     // (1-2)-3, because Sum recurses on the left
	[InlineData("2+3*4",        14)]
	[InlineData("(2+3)*4",      20)]
	[InlineData("100/5/2",      10)]
	[InlineData("-3*-4",        12)]
	[InlineData(" 1 + 2 * 3 ",   7)]     // Trivia is shadowed, so spaces do not matter
	public void The_calculator_computes(string expression, int expected) =>
		Assert.Equal(expected, Calculator.Evaluate(expression));

	[Fact]
	public void And_says_where_an_expression_stops_being_one()
	{
		Assert.Equal("2*3 = 6", Calculator.Explain("2*3"));
		Assert.StartsWith("Input does not match", Calculator.Explain("2*"));
		Assert.Throws<FormatException>(static () => Calculator.Evaluate("2*"));
	}

	// ── The feed reader ──────────────────────────────────────────────────────────

	const string Text =
		"H|2026-08-13|ACME\n" +
		"R|AAPL|100|2026-08-12\n" +
		"R|MSFT|250|2026-08-12\n" +
		"R|NVDA|75|2026-08-11\n" +
		"T|3\n";

	[Fact]
	public void A_feed_is_read_whole()
	{
		var feed = FeedReader.Read(Text);

		Assert.Equal(new DateOnly(2026, 8, 13), feed.Date);
		Assert.Equal("ACME", feed.Source);

		Assert.Equal(
			[
				new Trade("AAPL", 100, new DateOnly(2026, 8, 12)),
				new Trade("MSFT", 250, new DateOnly(2026, 8, 12)),
				new Trade("NVDA",  75, new DateOnly(2026, 8, 11)),
			],
			feed.Trades);
	}

	[Theory]
	[InlineData("H|2026-08-13|ACME\n",                              "no trailer")]
	[InlineData("R|AAPL|100|2026-08-12\nT|1\n",                     "no header")]
	[InlineData("H|2026-08-13|ACME\nT|0\nR|AAPL|1|2026-08-12\n",    "a record after the trailer")]
	[InlineData("H|2026-08-13|ACME\nR|AAPL|x|2026-08-12\nT|1\n",    "a quantity that is not a number")]
	[InlineData("H|2026-08-13|ACME\nR|AAPL|1|2026-8-12\nT|1\n",     "a date that is not four-two-two")]
	[InlineData("H|2026-08-13|ACME\nR|AAPL|1|2026-08-12\nT|9\n",    "a count that disagrees")]
	public void And_refused_when_it_is_not_whole(string text, string why) =>
		Assert.True(
			Record.Exception(() => FeedReader.Read(text)) is FormatException,
			$"A feed with {why} should have been refused.");

	// ── The feed reader that recovers ────────────────────────────────────────────

	const string Broken =
		"H|2026-08-13|ACME\n" +
		"R|AAPL|100|2026-08-12\n" +
		"R|MSFT|two hundred|2026-08-12\n" +
		"R|NVDA|75|2026-08-11\n" +
		"T|3\n";

	[Fact]
	public void A_bad_record_does_not_cost_the_feed()
	{
		var lines = RecoveringFeedReader.Read(Broken);

		Assert.Equal(
			[
				new TradeLine("AAPL", 100, new DateOnly(2026, 8, 12)),
				new RejectedLine(1, 40, "R|MSFT|two hundred|2026-08-12"),
				new TradeLine("NVDA",  75, new DateOnly(2026, 8, 11)),
			],
			lines);
	}

	[Fact]
	public void And_the_rejection_says_which_record_it_was() =>
		// The ordinal counts rejected records too, so it is the record's place in the file
		// and not the place it happened to end up in among the good ones.
		Assert.Equal([1], Array.ConvertAll([.. RecoveringFeedReader.Rejected(Broken)], line => line.Ordinal));

	[Fact]
	public void But_the_frame_around_the_records_still_has_to_be_there() =>
		Assert.Throws<FormatException>(
			static () => RecoveringFeedReader.Read("R|AAPL|100|2026-08-12\n"));

	[Fact]
	public void Records_can_be_read_out_of_a_feed_that_is_not_whole() =>
		// No header, no trailer, and a line that is not a record at all — `find all`
		// passes over what it cannot match and says nothing about it.
		Assert.Equal(
			["AAPL", "MSFT"],
			Array.ConvertAll(
				[.. FeedReader.ReadRecords(
					"R|AAPL|100|2026-08-12\n" +
					"what is this line\n" +
					"R|MSFT|250|2026-08-12\n")],
				trade => trade.Symbol));
}
