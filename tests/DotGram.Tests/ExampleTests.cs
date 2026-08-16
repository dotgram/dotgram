using System;
using System.Globalization;
using System.IO;
using System.Linq;

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

	// ── The calculator that recurses both ways ───────────────────────────────────

	static string Decimal(string expression) =>
		DecimalCalculator.Evaluate(expression).ToString(CultureInfo.InvariantCulture);

	[Theory]
	[InlineData("1-2-3",         "-4")]     // (1-2)-3 — Sum takes its left operand at its own level
	[InlineData("100/5/2",       "10")]     // (100/5)/2, the same reason
	[InlineData("2^3^2",        "512")]     // 2^(3^2) — Power takes its right one there instead
	[InlineData("(2^3)^2",       "64")]     // and this is what the other grouping means
	public void Each_operator_groups_by_which_side_is_parsed_at_its_own_level(
		string expression, string expected) =>
		Assert.Equal(expected, Decimal(expression));

	[Theory]
	[InlineData("1/8",        "0.125")]     // `: @decimal`, so not the 0 the int one gives
	[InlineData("1.5*2",        "3.0")]
	[InlineData("2^-2",        "0.25")]
	[InlineData("-2^2",          "-4")]     // -(2^2): `^` binds tighter than unary minus
	[InlineData("2*3^2",         "18")]     // and looser than `*`
	public void And_reckons_in_decimal(string expression, string expected) =>
		Assert.Equal(expected, Decimal(expression));

	[Fact]
	public void And_says_where_a_decimal_expression_stops_being_one()
	{
		Assert.Equal("1/8 = 0.125", DecimalCalculator.Explain("1/8"));
		Assert.StartsWith("Input does not match", DecimalCalculator.Explain("1/"));
	}

	[Fact]
	public void And_a_number_is_where_a_space_still_means_something()
	{
		// `Number` is declared in a scope that shadows Trivia with `none`, so the spaces
		// this grammar ignores everywhere else are not ignored between digits.
		Assert.Equal("1.5", Decimal(" 1.5 "));
		Assert.Throws<FormatException>(static () => DecimalCalculator.Evaluate("1 . 5"));
	}

	// ── The calculator of one rule ───────────────────────────────────────────────

	static string Strength(string expression) =>
		StrengthCalculator.Evaluate(expression).ToString(CultureInfo.InvariantCulture);

	[Theory]
	[InlineData("1-2-3",         "-4")]     // `<< 1` reads its right operand at 2 → groups left
	[InlineData("2^3^2",        "512")]     // `>> 3` reads it at 3 → groups right
	[InlineData("-1-2",          "-3")]
	[InlineData("1/8",        "0.125")]
	[InlineData(" 1 + 2 * 3 ",    "7")]
	public void One_rule_holds_a_whole_expression_language(string expression, string expected) =>
		Assert.Equal(expected, Strength(expression));

	[Fact]
	public void And_says_where_a_one_rule_expression_stops_being_one()
	{
		Assert.Equal("-1-2 = -3", StrengthCalculator.Explain("-1-2"));

		// A strength refuses nothing by itself: what fails here is that `+` needs a right
		// operand, and the position named is where one stopped being available.
		Assert.StartsWith("Input does not match", StrengthCalculator.Explain("1+"));
	}

	/// <summary>
	/// One corpus, used by every test that says two of these examples agree.
	/// </summary>
	/// <remarks>
	/// Written once because the claim is only worth as much as the expressions it is made
	/// over, and a claim made over three of them is not worth much.
	/// </remarks>
	public static TheoryData<string> Expressions =>
	[
		"1+2+3",
		"1-2-3",        // (1-2)-3 — `<< 1` reads its right operand at 2
		"2+3*4",
		"2*3+4",
		"(2+3)*4",
		"100/5/2",
		"2^3^2",        // 2^(3^2) — `>> 3` reads it at 3
		"(2^3)^2",
		"1/8",
		"1.5*2",
		" 1 + 2 * 3 ",
		"-1-2",         // -(1) - 2: unary minus is stronger than binary
		"-1*2",
		"-(1-2)",
		"--1",
		"-2*3",
		"2*3^2",
		"(2+3)*-4",
		"-2^2",         // -(2^2): `^` is stronger than unary minus on its left
		"2^-2",         // 2^(-2): and weaker on its right
	];

	[Theory]
	[MemberData(nameof(Expressions))]
	public void One_rule_of_strengths_means_what_the_five_rules_of_levels_mean(string expression) =>
		// The two calculators, expression by expression. The last pair is the one that
		// looked as though it could not translate: levels say the asymmetry by naming two
		// different rules either side of `^`, and a strength is one number.
		//
		// One number says both, because a strength is not symmetric to begin with — it is
		// the strength the operand to the *right* is read at, and nothing else. A prefix
		// has no left operand, so it is a base, and a base is entered whatever strength was
		// asked for: there is nothing to its left for anything to bind more tightly than.
		// So `^` and unary minus at the same 3 gives -(2^2) on one side and 2^(-2) on the
		// other.
		Assert.Equal(Decimal(expression), Strength(expression));

	// ── The calculator that builds a tree instead ────────────────────────────────

	[Fact]
	public void An_expression_comes_back_as_a_tree_C_sharp_can_read()
	{
		// The point of the example: what comes back is data, and a pattern is how it is
		// asked what it is. Nothing here knows anything about the parser.
		var tree = ExpressionParser.Read("1 + 2 * 3");

		// The node type is the operation, so this is `is Add` and not a string compared
		// against "+".
		var (left, right) = Assert.IsType<Add>(tree);

		Assert.Equal(new Number(1m), left);

		// `*` binds tighter, so it is the one nested inside — precedence read off the
		// shape rather than trusted.
		var (two, three) = Assert.IsType<Mul>(right);

		Assert.Equal(new Number(2m), two);
		Assert.Equal(new Number(3m), three);
	}

	[Fact]
	public void And_records_make_a_whole_tree_one_assertion() =>
		// Value equality all the way down, which is the other half of "it is ordinary C#
		// data": no walker, no visitor, one `==`.
		Assert.Equal(
			new Sub(
				new Add(new Number(1m), new Number(2m)),
				new Negate(new Number(3m))),
			ExpressionParser.Read("1 + 2 - -3"));

	[Fact]
	public void And_every_operator_has_a_node_of_its_own() =>
		Assert.Equal(
			new Add(
				new Number(1m),
				new Div(
					new Mul(new Number(2m), new Pow(new Number(3m), new Number(4m))),
					new Negate(new Number(5m)))),
			ExpressionParser.Read("1 + 2 * 3 ^ 4 / -5"));

	[Theory]
	[InlineData("1-2-3",   "((1 - 2) - 3)")]      // Sum's left operand is at Sum's level
	[InlineData("2^3^2",   "(2 ^ (3 ^ 2))")]      // Power's right operand is at Power's
	[InlineData("-2^2",    "-(2 ^ 2)")]           // `^` binds tighter than unary minus
	[InlineData("2*3+4",   "((2 * 3) + 4)")]
	[InlineData("2*(3+4)", "(2 * (3 + 4))")]
	public void And_the_shape_is_the_grouping(string expression, string expected) =>
		Assert.Equal(expected, ExpressionParser.Read(expression).Print());

	[Theory]
	[MemberData(nameof(Expressions))]
	public void And_a_walk_over_it_gets_the_same_answers_as_the_calculator(string expression) =>
		// The tree is built by a grammar of levels, like DecimalCalculatorExample and
		// unlike StrengthCalculatorExample. All three answer the same, which is the point:
		// the two conventions are two ways of saying one language, and what a `=>` builds
		// — a number or a node — is a separate question from how the grammar is written.
		Assert.Equal(
			Decimal(expression),
			ExpressionParser.Read(expression).Evaluate().ToString(CultureInfo.InvariantCulture));

	[Theory]
	[MemberData(nameof(Expressions))]
	public void And_one_rule_of_strengths_builds_the_very_same_tree(string expression) =>
		// Not "the same answer" — the same tree, node for node, by record equality. Five
		// rules of levels and one rule of strengths are two ways of writing one language,
		// and this is as close as a test can get to saying so.
		Assert.Equal(ExpressionParser.Read(expression), OneRuleParser.Read(expression));

	[Fact]
	public void And_the_walks_do_not_know_which_grammar_built_it() =>
		// `Evaluate` and `Print` are on the tree and mention no parser, which is why one
		// set of them serves both grammars — and why this line says nothing about which
		// one built the node it is calling.
		Assert.Equal("((1 - 2) - -3)", OneRuleParser.Read("1-2--3").Print());

	[Fact]
	public void And_a_tree_can_be_rewritten_before_it_is_walked()
	{
		// What the tree is for, and what stays with patterns: `Evaluate` and `Print` are on
		// the nodes because every tree needs them, and everything else — this — is written
		// where it is wanted, over records the tree knows nothing about.
		Assert.Equal(6m, Double(ExpressionParser.Read("1 + 2")).Evaluate());

		static Expression Double(Expression node) => node switch
		{
			Number(var value)        => new Number(value * 2),
			Negate(var operand)      => new Negate(Double(operand)),

			Add(var left, var right) => new Add(Double(left), Double(right)),
			Sub(var left, var right) => new Sub(Double(left), Double(right)),
			Mul(var left, var right) => new Mul(Double(left), Double(right)),
			Div(var left, var right) => new Div(Double(left), Double(right)),
			Pow(var left, var right) => new Pow(Double(left), Double(right)),

			_ => node,
		};
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
				new RejectedLine(1, 3, "R|MSFT|two hundred|2026-08-12", "Input does not match 'Row' at 47."),
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

	// ── The feed that logs its rejections instead ────────────────────────────────

	[Fact]
	public void A_rejection_can_leave_by_another_door()
	{
		// §8.3's fourth row: only the good records come back, and the bad ones went to the
		// hook. The array holds `Row` and nothing else — no caller has to filter.
		var (rows, rejected) = LoggingFeedReader.Read(Broken);

		Assert.Equal(["AAPL", "NVDA"], Array.ConvertAll([.. rows], row => row.Symbol));

		var one = Assert.Single(rejected);

		Assert.Equal("Row", one.Rule);
		Assert.Equal(3,     one.Line);
		Assert.Equal("R|MSFT|two hundred|2026-08-12", one.Text);
		Assert.StartsWith("Input does not match 'Row'", one.Message);
	}

	[Fact]
	public void And_a_whole_feed_reports_nothing() =>
		Assert.Empty(LoggingFeedReader.Read(Text).Rejected);

	// ── The feed read from a reader ──────────────────────────────────────────────

	[Fact]
	public void A_feed_can_be_read_from_a_reader_a_part_at_a_time() =>
		// The envelope and the records in one sequence, in the order they were read —
		// which is what makes the trailer checkable at all without holding the file.
		Assert.Equal(
			["FeedOpening", "FeedTrade", "FeedTrade", "FeedTrade", "FeedClosing"],
			[.. StreamingFeedReader.Read(new StringReader(Text))
				.Select(part => part.GetType().Name)]);

	[Fact]
	public void And_adds_up_without_ever_holding_it() =>
		Assert.Equal((3, 425L), StreamingFeedReader.Total(new StringReader(Text)));

	[Fact]
	public void And_the_trailer_is_checked_against_what_came_before_it()
	{
		Assert.True(StreamingFeedReader.Balances(new StringReader(Text)));

		Assert.False(StreamingFeedReader.Balances(new StringReader(
			Text.Replace("T|3", "T|9", StringComparison.Ordinal))));
	}

	[Fact]
	public void And_records_of_differing_lengths_are_not_lost_at_the_edge()
	{
		// Records all one length keep every window boundary at the same offset inside a
		// record; varying them walks the boundary through every offset there is. That is
		// what a real feed does, and what every fixed-width test here could not reach — a
		// record straddling the end of the buffer was read as a broken one and stepped
		// over, silently, one per window.
		var records = string.Concat(Enumerable.Range(0, 20_000)
			.Select(i => "R|AAPL|" + (i % 1000).ToString(CultureInfo.InvariantCulture) + "|2026-08-12\n"));

		Assert.Equal(
			20_000,
			StreamingFeedReader.Total(new StringReader(
				"H|2026-08-13|ACME\n" + records + "T|20000\n")).Trades);
	}

	[Fact]
	public void And_reads_more_records_than_it_could_hold()
	{
		// The claim streaming exists to make, at a size where holding the input would be a
		// choice rather than an accident: a megabyte of records through a 4 KB window.
		var records = string.Concat(Enumerable.Repeat("R|AAPL|2|2026-08-12\n", 50_000));

		Assert.Equal(
			(50_000, 100_000L),
			StreamingFeedReader.Total(new StringReader(
				"H|2026-08-13|ACME\n" + records + "T|50000\n")));
	}

	[Fact]
	public void A_feed_can_also_be_read_from_a_sequence_of_lines()
	{
		// §6.3 lists `IEnumerable<string>` beside `TextReader`, and the difference between
		// them is one character: a reader carries its terminators and a sequence of lines
		// has had them taken off, so they are put back.
		var lines = new[] { "H|2026-08-13|ACME", "R|AAPL|100|2026-08-12", "T|1" };

		Assert.Equal(
			["FeedOpening", "FeedTrade", "FeedClosing"],
			[.. StreamingFeedReader.Read(lines).Select(part => part.GetType().Name)]);
	}

	[Fact]
	public void And_reads_the_same_things_a_reader_would()
	{
		// The same feed, given both ways. What comes back may not depend on which door it
		// came in by.
		Assert.Equal(
			[.. StreamingFeedReader.Read(new StringReader(Text)).Select(part => part.ToString())],
			[.. StreamingFeedReader.Read(Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
				.Select(part => part.ToString())]);
	}

	[Fact]
	public void And_the_reader_is_only_read_as_the_sequence_is_walked()
	{
		// Lazy, which is what "the result is walked once" in §6.3 means. Nothing is read
		// until the first `MoveNext`, so a malformed feed does not throw where it was
		// asked for.
		var sequence = StreamingFeedReader.Read(new StringReader("nonsense\n"));

		Assert.Throws<FormatException>(() => sequence.ToArray());
	}

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

	// ── The JSON parser ──────────────────────────────────────────────────────────

	[Fact]
	public void Json_reads_a_document_of_every_kind_of_value()
	{
		var read = (JsonObject)JsonParser.Read(
			"""
			{
				"name":    "a \"quoted\" name",
				"count":   42,
				"ratio":   0.25,
				"ok":      true,
				"missing": null,
				"items":   [1, "two", false, {"deep": []}]
			}
			""");

		Assert.Equal("a \\\"quoted\\\" name", ((JsonText)read["name"]!).Text);
		Assert.Equal(42,    ((JsonNumber)read["count"]!).Value);
		Assert.Equal(0.25,  ((JsonNumber)read["ratio"]!).Value);
		Assert.True(((JsonBool)read["ok"]!).Value);
		Assert.IsType<JsonNull>(read["missing"]);

		var items = (JsonArray)read["items"]!;

		Assert.Equal(4, items.Items.Count);
		Assert.Empty(((JsonArray)((JsonObject)items.Items[3])["deep"]!).Items);
	}

	[Fact]
	public void And_an_empty_object_and_array_are_documents_too()
	{
		Assert.Empty(((JsonObject)JsonParser.Read("{}")).Members);
		Assert.Empty(((JsonArray)JsonParser.Read("[ ]")).Items);
	}

	[Fact]
	public void And_what_is_not_JSON_is_refused() =>
		Assert.Throws<FormatException>(() => JsonParser.Read("{\"a\": }"));

	// ── The CSV with no `=>` in it (§7.3) ────────────────────────────────────────

	[Fact]
	public void A_row_is_built_by_the_constructor_its_captures_cover()
	{
		var trades = TypedCsv.Read(
			"""
			AAPL,100,2026-08-12
			MSFT,250,2026-08-13

			""");

		Assert.Equal(["AAPL", "MSFT"], trades.Select(trade => trade.Symbol));
		Assert.Equal([100, 250],       trades.Select(trade => trade.Size));
		Assert.Equal(new DateOnly(2026, 8, 12), trades[0].On);
	}

	[Fact]
	public void And_the_same_feed_streams_a_record_at_a_time()
	{
		// §6.3 and §7.3 in one place: the window hands back records built by a constructor
		// it knows nothing about. Nothing in the grammar asked for the overload — a rule
		// that reads a bounded amount per record gets one.
		var reader = new StringReader(
			"""
			AAPL,100,2026-08-12
			MSFT,250,2026-08-13

			""");
		var read   = 0;

		foreach (var trade in TypedCsv.Read(reader))
		{
			Assert.Equal(read == 0 ? "AAPL" : "MSFT", trade.Symbol);

			read++;
		}

		Assert.Equal(2, read);
	}

	[Fact]
	public void And_a_type_with_no_constructor_is_written_into()
	{
		var session = TypedCsv.ReadSession(
			"""
			2026-08-12/2026-08-13

			""");

		Assert.Equal(new DateOnly(2026, 8, 12), session.Opened);
		Assert.Equal(new DateOnly(2026, 8, 13), session.Closed);

		// Neither captured nor required, so the type's own default survives.
		Assert.Equal("", session.Note);
	}

	// ── The XML parser ───────────────────────────────────────────────────────────

	[Fact]
	public void Xml_reads_elements_attributes_and_text()
	{
		var root = XmlParser.Read("""<a x="1" y='2'><b>text</b><c/></a>""");

		Assert.Equal("a", root.Name);
		Assert.Equal("1", root["x"]);
		Assert.Equal("2", root["y"]);
		Assert.Equal(["b", "c"], root.Elements.Select(element => element.Name));
		Assert.Equal("text", ((XmlText)root.Elements.First().Children.Single()).Text);
		Assert.Empty(root.Elements.Last().Children);
	}

	[Fact]
	public void And_a_closing_tag_has_to_close_the_element_it_is_in()
	{
		// The `where` doing the one thing no amount of grammar can say. Saying no is an
		// ordinary non-match, so this is not an element at all rather than a wrong one.
		Assert.Throws<FormatException>(static () => XmlParser.Read("<a></b>"));

		// And the same names nested still close in order.
		Assert.Equal("a", XmlParser.Read("<a><a></a></a>").Elements.Single().Name);

		// A space before the closing bracket is XML's to allow, so it is written down.
		Assert.Equal("1", XmlParser.Read("""<a x="1" ></a >""")["x"]);
	}

	[Fact]
	public void And_whitespace_inside_an_element_is_text_because_in_XML_it_is()
	{
		// Not a decision this grammar makes lightly: content is `[^ '<']+`, so the run
		// between two tags is text whatever it holds. XML says the same — whitespace
		// between elements is a text node, and throwing it away is a reader's choice
		// rather than a parser's.
		Assert.Equal(
			["  ", "  "],
			XmlParser.Read("<a>  <b/>  </a>").Children.OfType<XmlText>().Select(text => text.Text));

		Assert.Equal(" a b ", ((XmlText)XmlParser.Read("<a> a b </a>").Children.Single()).Text);
	}

	// ── The Markdown parser ──────────────────────────────────────────────────────

	[Fact]
	public void Markdown_reads_a_document_into_blocks()
	{
		var blocks = MarkdownParser.Blocks(
			"# Title\n" +
			"\n" +
			"some text\n" +
			"- one\n" +
			"- two\n" +
			"\n" +
			"```csharp\n" +
			"var x = 1;\n" +
			"```\n").ToArray();

		var heading = Assert.IsType<MarkdownHeading>(blocks[0]);

		Assert.Equal(1, heading.Level);
		Assert.Equal("Title", heading.Text);

		Assert.Equal("some text", Assert.IsType<MarkdownParagraph>(blocks[1]).Text);
		Assert.Equal(["one", "two"], Assert.IsType<MarkdownList>(blocks[2]).Items);
		Assert.Equal(["var x = 1;"], Assert.IsType<MarkdownCode>(blocks[3]).Lines);
	}

	[Fact]
	public void And_the_level_of_a_heading_is_how_many_hashes_it_had() =>
		Assert.Equal(
			[1, 3],
			MarkdownParser.Blocks("# a\n### b\n").OfType<MarkdownHeading>().Select(h => h.Level));

	[Fact]
	public void And_a_line_that_is_none_of_the_others_is_a_paragraph() =>
		// Ordered choice is the whole definition: `#nope` has no space after the hash, so
		// it is not a heading, and nothing else claims it either.
		Assert.Equal(
			"#nope",
			Assert.IsType<MarkdownParagraph>(MarkdownParser.Blocks("#nope\n").Single()).Text);

	[Fact]
	public void And_nesting_goes_as_deep_as_the_stack_allows()
	{
		// Recursion between rules is ordinary C# recursion — `Value` calls `Array` calls
		// `Value` — so depth costs frames rather than heap. docs/status.md puts the ceiling
		// at about 2700 for a single rule; this is well inside it and passes through three
		// rules per level, which is the shape a real document has.
		const int depth = 200;

		var text = new string('[', depth) + "1" + new string(']', depth);
		var read = JsonParser.Read(text);

		for (var i = 0; i < depth; i++)
			read = Assert.IsType<JsonArray>(read).Items.Single();

		Assert.Equal(1, Assert.IsType<JsonNumber>(read).Value);
	}

	[Fact]
	public void And_an_unclosed_document_says_where_it_stopped_being_one()
	{
		// The position is what a reader needs from a format with no recovery in it: the
		// parse ran out of ways through the rule at the end of the input.
		var error = Assert.Throws<FormatException>(static () => JsonParser.Read("""{"a": [1, 2"""));

		Assert.Contains("11", error.Message, StringComparison.Ordinal);
	}

	// ── The read-only SQL guard ──────────────────────────────────────────────────

	[Theory]
	[InlineData("select * from t")]
	[InlineData("SELECT a, b FROM t WHERE x = 1")]
	[InlineData("with c as (select 1) select * from c")]
	[InlineData("select 'a''; drop table t --' from t")]     // the escaped quote
	[InlineData("select \"insert\" from t")]                  // a quoted identifier
	[InlineData("select [delete] from t")]                   // and the other spelling
	[InlineData("select 1 -- drop table t")]                 // inside a line comment
	[InlineData("select 1 /* update t set x = 1 */")]        // and a block one
	[InlineData("select into_stock from t")]                 // `into` is not a word here
	[InlineData("select * from t;")]
	public void Reads_only(string sql) => Assert.True(SqlReadOnly.IsReadOnly(sql), sql);

	[Theory]
	[InlineData("insert into t values (1)")]
	[InlineData("select * into other from t")]               // the one that hides in a select
	[InlineData("SELECT * INTO other FROM t")]
	[InlineData("select 1; drop table t")]                   // a second statement
	[InlineData("select 1;drop table t")]
	[InlineData("update t set x = 1")]
	[InlineData("delete from t")]
	[InlineData("truncate table t")]
	[InlineData("exec sp_who")]
	[InlineData("execute sp_who")]
	[InlineData("select * from t; -- and then\ndrop table t")]
	[InlineData("drop table t -- select")]
	public void Cannot_be_shown_to_read_only(string sql) =>
		Assert.False(SqlReadOnly.IsReadOnly(sql), sql);

	[Theory]
	[InlineData("select '/*' from t")]
	[InlineData("select '--' from t")]
	[InlineData("select ';drop table t' from t")]
	[InlineData("select 1 -- it's fine")]
	[InlineData("select 1 /* it's fine */")]
	[InlineData("select '''' from t")]
	[InlineData("select 1 /* unterminated")]
	public void Reads_only_across_the_awkward_ones(string sql) =>
		Assert.True(SqlReadOnly.IsReadOnly(sql), sql);

	[Theory]
	[InlineData("select/*c*/into other from t")]
	[InlineData("select 1 /* /* */ drop table t */")]
	[InlineData("select 1 /* drop table t")]
	[InlineData("select 'unterminated")]
	public void Refused_when_it_cannot_be_sure(string sql) =>
		Assert.False(SqlReadOnly.IsReadOnly(sql), sql);
}
