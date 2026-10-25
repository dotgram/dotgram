using System;
using System.IO;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A choice of literals that share a prefix, read longest first with a room check: where the window ends
/// between the shorter literal's end and the longer one's, the shorter is taken without saying the window
/// ran out, inside a look and outside one alike.
/// </summary>
public sealed class WindowEdgeLiteralChoiceTests
{
	[Theory]
	[InlineData("Rows : @string[] = Row* recover ';' => @(\"bad \" + parserFailureColumn) & eof\nRow : @string = k: (\"STOPS\" | \"ST\") & ';' => @(k)\nparse Rows\n",
		"ParseRows", "ST;STOPS;ST;STOPS;STOPS;")]
	[InlineData("Rows : @string[] = Row* recover ';' => @(\"bad \" + parserFailureColumn) & eof\nRow : @string = k: (\"select\" | \"sel\" | \"se\") & ' ' & t: ['a'..'z']+ & ';' => @(k + \":\" + t)\nparse Rows\n",
		"ParseRows", "sel bb;select a;se c;select a;")]
	[InlineData("Item : @string = t: (\"abcd\" | \"ab\") & ';' => @(t)\nfind Item\n",
		"FindItem", "xabcd;abcd;xabcd;ab;")]
	[InlineData("Hit : @string = t: ?=({'a' & (\"bc\" | 'b')}) & 'a' => @(t)\nfind Hit\n",
		"FindHit", "xxabc")]
	public void A_choice_of_literals_reads_past_the_window_end(string grammar, string method, string text)
	{
		var parser = WindowEdge.Compile(grammar);
		var found  = new WindowEdge.Mismatches();

		WindowEdge.Edges(parser, found, "reader", text, text.Length, WindowEdge.Said(() => parser.Call(method, typeof(string), text)),
			() => parser.Call(method, typeof(TextReader), new StringReader(text)));

		found.Assert();
	}
}
