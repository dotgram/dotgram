using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Which rules can span a line, which is half of deciding whether a grammar can stream.
/// </summary>
/// <remarks>
/// The other half — the commit points that let the window move — is not built, and
/// nothing is emitted from any of this yet. It is tested on its own because an analysis
/// that is only ever exercised through the feature it gates is an analysis nobody can
/// tell is wrong.
/// </remarks>
public sealed class RetentionTests
{
	static string[] SpanningRules(string grammar)
	{
		var graph = GrammarNormalizer.Normalize(GrammarBinder.Bind(
			GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));

		// Before the analysis, because a grammar that did not compile answers "nothing
		// spans a line" and is indistinguishable from one where nothing does.
		Assert.Empty(graph.Diagnostics);

		return
		[
			.. Retention.RulesThatSpanLines(graph)
				.Select(rule => rule.Name)
				.OrderBy(name => name, StringComparer.Ordinal)
		];
	}

	[Fact]
	public void A_rule_of_ordinary_characters_stays_on_its_line() =>
		Assert.Empty(SpanningRules("Word = ['a'..'z']+"));

	[Fact]
	public void One_that_names_a_terminator_does_not() =>
		Assert.Equal(["Line"], SpanningRules("Line = ['a'..'z']* & '\\n'"));

	[Fact]
	public void And_so_does_one_that_matches_it_in_a_literal() =>
		Assert.Equal(["Break"], SpanningRules("Break = \"\\r\\n\""));

	[Fact]
	public void A_complement_that_forgets_to_exclude_it_spans() =>
		// The case the analysis exists for. `[^ '|']` is how a field is written when the
		// author is thinking about separators and not about lines, and it will happily
		// swallow the rest of the file.
		Assert.Equal(["Field"], SpanningRules("Field = [^ '|']+"));

	[Fact]
	public void And_one_that_excludes_it_does_not() =>
		Assert.Empty(SpanningRules("Field = [^ '|' | '\\r' | '\\n']+"));

	[Fact]
	public void It_reaches_through_a_call() =>
		// `Row` spans because `Text` does, not because it says so itself.
		Assert.Equal(
			["Row", "Text"],
			SpanningRules("Row = 'R' & Text\nText = [^ '|']+"));

	[Fact]
	public void And_through_a_capture_and_a_construction() =>
		Assert.Equal(
			["Row"],
			SpanningRules("Row : @string = t: [^ '|']+ => @(t)"));

	[Fact]
	public void A_repetition_of_none_spans_nothing() =>
		// `{0}` matches nothing at all, so what it repeats cannot be reached.
		Assert.Empty(SpanningRules("Never = [^ '|']{0}"));

	[Fact]
	public void A_lookahead_reads_a_terminator_without_consuming_one() =>
		// It consumes nothing, so it retains nothing. What it needs to *see* is a window
		// question rather than a retention one, and §6.3 does not answer it yet.
		Assert.Empty(SpanningRules("AtEnd = ?='\\n' & ['a'..'z']*"));

	[Fact]
	public void Recursion_settles_rather_than_spinning() =>
		// A rule that only reaches a terminator through itself reaches one never, which is
		// what starting at "no" and growing gives. Reaching one otherwise still spreads.
		Assert.Equal(
			["Nested"],
			SpanningRules("Nested = '(' & Nested & ')' | '\\n'"));

	[Fact]
	public void A_category_is_assumed_to_admit_one() =>
		// Not looked into, and wrong in the safe direction: a rule wrongly said to span a
		// line loses an overload it could have had; wrongly said not to, it would lose data.
		Assert.Equal(["Any"], SpanningRules(@"Any = [\p{L}]+"));

	[Fact]
	public void The_feed_of_the_examples_spans_only_where_it_should()
	{
		// The shape the whole analysis is for. Every rule of a line-oriented feed stays on
		// its line except the ones that deliberately end one.
		var spanning = SpanningRules("""
			Feed    = header: Header & rows: Row* & trailer: Trailer & eof
			Header  = "H" & '|' & source: Text & eol
			Row     = "R" & '|' & symbol: Text & eol
			Trailer = "T" & '|' & count: Digit+ & eol
			Text    = [^ '|' | '\r' | '\n']+
			Digit   = ['0'..'9']
			""");

		// The rules that end a line span one; the fields do not, which is what makes a
		// line a workable unit for this grammar.
		Assert.Contains("Header",  spanning);
		Assert.Contains("Row",     spanning);
		Assert.Contains("Trailer", spanning);
		Assert.Contains("Feed",    spanning);

		Assert.DoesNotContain("Text",  spanning);
		Assert.DoesNotContain("Digit", spanning);
	}
}
