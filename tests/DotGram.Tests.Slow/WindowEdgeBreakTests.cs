using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Readings that still take the window's end for an answer: each is read with the window's end at every
/// offset of its input and held to the whole input's form.
/// </summary>
public sealed class WindowEdgeBreakTests
{
	/// <summary>A look inside a scanned rule that a window's end satisfies: eof, ?!any, ?!class, a stop character.</summary>
	[Theory]
	[InlineData("H = { ?=(\"ab\" & eof) & 'a' }", "xxabc")]
	[InlineData("H = { ?=(\"ab\" & ?!any) & 'a' }", "xxabc")]
	[InlineData("H = { ?=(\"ab\" & ?!['c'..'d']) & 'a' }", "xxabc")]
	[InlineData("H = { ?=(['a'..'b']+ & eof) & 'a' }", "xxabbc")]
	[InlineData("H = { ?=(\"ab\" & 'c'? & eof) & 'a' }", "xxabcd")]
	[InlineData("H = { ?!(?!(\"ab\" & eof)) & 'a' }", "xxabc")]
	[InlineData("H = { ?=(\"abc\"i & ?!any) & 'a' }", "xxabcd")]
	[InlineData("H = { ?!((?!\"*/\" & [^ '*'])* & \"*/\") & 'a' }", "xxa*/")]
	[InlineData("H = { ?=((?!\"*/\" & [^ '*'])* & \"*/\") & 'a' }", "xxa*/")]
	public void A_look_inside_a_scanned_rule_is_not_satisfied_by_the_window_end(string rule, string text)
	{
		var parser = WindowEdge.Compile("Hit = ?=H & 'a'\n" + rule + "\nfind Hit\n");
		var found  = new WindowEdge.Mismatches();

		WindowEdge.Edges(parser, found, "reader", text, text.Length, WindowEdge.Said(() => parser.Call("FindHit", typeof(string), text)),
			() => parser.Call("FindHit", typeof(TextReader), new StringReader(text)));

		found.Assert();
	}

	/// <summary>The same over segments: eos then eof, or eos then ?!any, inside the scanned rule's look.</summary>
	[Theory]
	[InlineData("H = { ?=(\"ab\" & eos & eof) & 'a' }")]
	[InlineData("H = { ?=(\"ab\" & eos & ?!any) & 'a' }")]
	public void A_look_inside_a_scanned_rule_over_segments_is_not_satisfied_by_the_window_end(string rule)
	{
		var parser   = WindowEdge.Compile("Hit = ?=H & 'a'\n" + rule + "\nfind Hit\n");
		var segments = new[] { "xxab", "c" };
		var found    = new WindowEdge.Mismatches();
		var whole    = WindowEdge.Said(() => parser.Call("FindHit", typeof(IReadOnlyList<string>), segments));

		WindowEdge.Edges(parser, found, "sequence", WindowEdge.Show(segments), 7, whole,
			() => parser.Call("FindHit", typeof(IEnumerable<string>), segments.Select(static one => one)));

		found.Assert();
	}

	/// <summary>
	/// An eol inside a negative look of the machine, followed by more of the look: where the window ends
	/// right before it, the reader accepts the record the string refuses.
	/// </summary>
	[Fact]
	public void An_eol_inside_a_negative_look_reads_past_the_window_end()
	{
		var parser = WindowEdge.Compile("""
			Rows  : @string[] = Row* recover eol => @("bad " + parserLine) & eof
			Row   : @string   = t: Word & eol & ?!Dup => @(t)
			                  | "--" & eol => @("--")
			Dup   = "--" & eol & "--" & eol
			Word  = ['a'..'z']+
			parse Rows
			""");

		var text  = "ab\n--\n--\n";
		var found = new WindowEdge.Mismatches();

		WindowEdge.Edges(parser, found, "reader", text, text.Length, WindowEdge.Said(() => parser.Call("ParseRows", typeof(string), text)),
			() => parser.Call("ParseRows", typeof(TextReader), new StringReader(text)));

		found.Assert();
	}

	/// <summary>
	/// The edge oracle's allowance for a source refused at a NUL: a smaller window may hand over more before
	/// that refusal, but only more, and only what the list reads of the segments before the NUL. Found where
	/// the sequence yielded a@2 before refusing the NUL of ["xxab", "c", "x\0"] and the list nothing; the
	/// generator no longer does, so the rule is held here on its own.
	/// </summary>
	[Fact]
	public void The_oracle_reports_a_value_handed_over_before_a_refusal_of_the_source()
	{
		const string Nul     = "refused: FormatException: A segment holds U+0000, which this grammar reads as the end of a segment and never as a character of one at line 3, column 2.";
		const string Other   = "refused: FormatException: Expected 'a' at 4.";
		const string A       = "{Position=2, Value=\"a\"} | ";
		const string B       = "{Position=7, Value=\"b\"} | ";

		// A value the list does not read of the segments before the NUL.
		Assert.False(WindowEdge.Same(Nul, A + Nul, beforeNul: ""));
		Assert.False(WindowEdge.Agrees(Nul, A + Nul, beforeNul: ""));

		// Fewer values than the larger window handed over, or any other refusal.
		Assert.False(WindowEdge.Same(A + B + Nul, A + Nul, beforeNul: A + B));
		Assert.False(WindowEdge.Same(Other, A + Other, beforeNul: A));

		// No list to hold them to.
		Assert.False(WindowEdge.Same(Nul, A + Nul, beforeNul: null));

		// More, and what the list reads before the NUL.
		Assert.True(WindowEdge.Same(Nul, A + Nul, beforeNul: A + B));
		Assert.True(WindowEdge.Same(A + Nul, A + B + Nul, beforeNul: A + B));
		Assert.True(WindowEdge.Agrees(Nul, A + Nul, beforeNul: A));
	}
}
