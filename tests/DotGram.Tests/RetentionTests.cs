using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What each rule can take, measured in lines — half of deciding whether a grammar can
/// stream.
/// </summary>
/// <remarks>
/// The other half, the commit points that let the window move, is not built, and nothing
/// is emitted from any of this yet. Tested on its own regardless: an analysis only ever
/// exercised through the feature it gates is one nobody can tell is wrong.
/// </remarks>
public sealed class RetentionTests
{
	static LineExtent ExtentOf(string grammar, string rule)
	{
		var graph = GrammarNormalizer.Normalize(GrammarBinder.Bind(
			GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));

		// Before the analysis, because a grammar that did not compile answers "nothing
		// takes a line" indistinguishably from one where nothing does.
		Assert.Empty(graph.Diagnostics);

		var extents = Retention.ExtentOf(graph);

		return extents.First(entry => entry.Key.Name == rule).Value;
	}

	[Theory]
	[InlineData("Word  = ['a'..'z']+",              LineExtent.None)]
	[InlineData("Word  = \"abc\"",                  LineExtent.None)]
	[InlineData("Word  = ['a'..'z']* & '\\n'",      LineExtent.AtEnd)]
	[InlineData("Word  = \"\\r\\n\"",               LineExtent.AtEnd)]
	[InlineData("Word  = '\\n' & ['a'..'z']",       LineExtent.Beyond)]
	public void What_a_rule_takes(string grammar, LineExtent expected) =>
		Assert.Equal(expected, ExtentOf(grammar, "Word"));

	[Fact]
	public void A_record_is_a_line_and_fits_one() =>
		// The distinction the three values exist for. `Row` consumes a terminator and is
		// still one line, because nothing follows it.
		Assert.Equal(
			LineExtent.AtEnd,
			ExtentOf("Row = \"R\" & Text & eol\nText = [^ '|' | '\\r' | '\\n']+", "Row"));

	[Fact]
	public void A_field_that_forgets_to_exclude_a_terminator_goes_beyond() =>
		// The case the analysis exists for. `[^ '|']` is how a field is written when the
		// author is thinking about separators and not about lines.
		Assert.Equal(
			LineExtent.Beyond,
			ExtentOf("Row = \"R\" & Text & eol\nText = [^ '|']+", "Row"));

	[Fact]
	public void And_the_field_itself_already_goes_beyond() =>
		// Not merely to the end of a line: `+` repeats, so a set that admits a terminator
		// admits any number of them, and the field alone can swallow the file.
		Assert.Equal(LineExtent.Beyond, ExtentOf("Text = [^ '|']+", "Text"));

	[Fact]
	public void A_repetition_of_lines_goes_beyond() =>
		Assert.Equal(
			LineExtent.Beyond,
			ExtentOf("Rows = Row*\nRow = \"R\" & eol", "Rows"));

	[Fact]
	public void An_optional_line_is_still_one_line() =>
		// At most once round, so at most one terminator and nothing after it.
		Assert.Equal(
			LineExtent.AtEnd,
			ExtentOf("Maybe = Row?\nRow = \"R\" & eol", "Maybe"));

	[Fact]
	public void A_repetition_of_none_takes_nothing() =>
		Assert.Equal(LineExtent.None, ExtentOf("Never = [^ '|']{0}", "Never"));

	[Fact]
	public void A_lookahead_reads_a_terminator_without_taking_one() =>
		Assert.Equal(LineExtent.None, ExtentOf("AtEnd = ?='\\n' & ['a'..'z']*", "AtEnd"));

	[Fact]
	public void A_line_followed_by_end_of_input_is_still_one_line() =>
		// `eof` consumes nothing, so it does not put the parse on the next line.
		Assert.Equal(LineExtent.AtEnd, ExtentOf("One = ['a'..'z']* & eol & eof", "One"));

	[Fact]
	public void Recursion_settles_rather_than_spinning() =>
		Assert.Equal(
			LineExtent.Beyond,
			ExtentOf("Nested = '(' & Nested & ')' | '\\n' & ['a'..'z']", "Nested"));

	[Fact]
	public void The_worst_alternative_decides() =>
		Assert.Equal(
			LineExtent.Beyond,
			ExtentOf("Either = ['a'..'z']+ | '\\n' & ['a'..'z']", "Either"));

	[Fact]
	public void A_category_is_assumed_to_admit_a_terminator() =>
		// Not looked into, and wrong in the safe direction: a rule wrongly said to take a
		// terminator loses an overload it could have had; wrongly said not to, it would
		// lose data.
		Assert.Equal(LineExtent.Beyond, ExtentOf(@"Any = [\p{L}]+", "Any"));

	[Fact]
	public void The_feed_of_the_examples_measures_as_it_reads()
	{
		const string Feed = """
			Feed    = header: Header & rows: Row* & trailer: Trailer & eof
			Header  = "H" & '|' & source: Text & eol
			Row     = "R" & '|' & symbol: Text & eol
			Trailer = "T" & '|' & count: Digit+ & eol
			Text    = [^ '|' | '\r' | '\n']+
			Digit   = ['0'..'9']
			""";

		// Fields fit inside a line; records are a line each.
		Assert.Equal(LineExtent.None,  ExtentOf(Feed, "Text"));
		Assert.Equal(LineExtent.None,  ExtentOf(Feed, "Digit"));
		Assert.Equal(LineExtent.AtEnd, ExtentOf(Feed, "Header"));
		Assert.Equal(LineExtent.AtEnd, ExtentOf(Feed, "Row"));

		// And the whole feed is many lines, which is exactly why it needs a commit point
		// before it can stream: without one, the window could never move.
		Assert.Equal(LineExtent.Beyond, ExtentOf(Feed, "Feed"));
	}
}
