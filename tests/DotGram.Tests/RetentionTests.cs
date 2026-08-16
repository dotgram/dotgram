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

	// ── Stages: whether a published rule can be read from a window ───────────────

	static Retention.Plan PlanFor(string grammar, string rule)
	{
		var graph = GrammarNormalizer.Normalize(GrammarBinder.Bind(
			GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));

		Assert.Empty(graph.Diagnostics);

		return Retention.PlanFor(graph, graph.Rules.First(r => r.Name == rule));
	}

	const string Streamable = """
		Feed    = Header & Row* recover eol & Trailer & eof
		Header  = "H" & '|' & Text & eol
		Row     = "R" & '|' & Text & eol
		Trailer = "T" & '|' & Text & eol
		Text    = [^ '|' | '\r' | '\n']+
		""";

	[Fact]
	public void A_feed_with_a_committed_run_can_be_read_from_a_window()
	{
		var plan = PlanFor(Streamable, "Feed");

		Assert.True(plan.CanStream, plan.Reason);
		Assert.Null(plan.Reason);

		// Header, the run, Trailer, eof — and the window moves at the one that commits.
		Assert.Equal(4, plan.Stages.Count);
		Assert.Single(plan.Stages, stage => stage.Committed);
	}

	[Fact]
	public void The_committed_run_is_measured_by_one_element_and_not_by_the_run()
	{
		// The run itself takes the whole file, which is the point of streaming it. What
		// has to fit the window is one `Row`.
		var run = plan().Stages.Single(stage => stage.Committed);

		Assert.Equal(LineExtent.AtEnd, run.Extent);
		Assert.Equal(LineExtent.Beyond, ExtentOf(Streamable, "Feed"));

		Retention.Plan plan() => PlanFor(Streamable, "Feed");
	}

	[Fact]
	public void An_uncommitted_run_is_measured_whole_and_names_itself()
	{
		// Take the mark off and the run stops being a stage boundary, so it is measured as
		// what it is — every row at once.
		var plan = PlanFor(Streamable.Replace(" recover eol", "", StringComparison.Ordinal), "Feed");

		Assert.False(plan.CanStream);
		Assert.Contains("Row*", plan.Reason, StringComparison.Ordinal);
		Assert.Contains("may take more than one line", plan.Reason);
	}

	[Fact]
	public void And_stages_that_all_fit_still_need_one_to_commit()
	{
		// Two lines and no run between them: each stage fits the window on its own, and
		// there is still no point at which the first may be let go.
		var plan = PlanFor(
			"""
			Pair    = Header & Trailer & eof
			Header  = "H" & eol
			Trailer = "T" & eol
			""",
			"Pair");

		Assert.False(plan.CanStream);
		Assert.Contains("none of them commits", plan.Reason);
	}

	[Fact]
	public void A_stage_that_takes_more_than_a_line_is_named()
	{
		// The message §6.3 promises: which part is responsible, not merely that something
		// is.
		var plan = PlanFor(
			Streamable.Replace("Text    = [^ '|' | '\\r' | '\\n']+", "Text    = [^ '|']+", StringComparison.Ordinal),
			"Feed");

		Assert.False(plan.CanStream);
		Assert.Contains("may take more than one line", plan.Reason);
		Assert.Contains("Header", plan.Reason);
	}

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

	[Fact]
	public void A_repetition_of_something_other_than_a_rule_has_nothing_to_hand_over()
	{
		// The driver yields the elements of a repetition of a rule, because a rule has a
		// recognizer of its own to call one element at a time. A repetition of a choice is
		// read whole by one machine, so there is nothing to give the caller between reads —
		// and the emitter, left to it, wrote an iterator with no `yield` in it, which the
		// consumer's compiler reported about a file they did not write.
		var graph = GrammarNormalizer.Normalize(GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(
			"Doc : @object[] = (Line | Blank)* & eof\n"
			+ "Line : @object = ['a'..'z']+ & eol => @(new object())\n"
			+ "Blank = eol\n"
			+ "parse Doc", RoslynCSharpScanner.Instance)).File));

		var reported = Retention.Check(graph);

		Assert.Contains(reported, d => d.Message.Contains("nothing to give the caller"));
	}
}
