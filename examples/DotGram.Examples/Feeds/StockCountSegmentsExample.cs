using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using DotGram;

namespace DotGram.Examples.Feeds;

// The stock count of StockCountExample, read as a feed is read: one line is one segment, and
// `eos` ends it. It is the same grammar with its line ends turned into segment ends, and the
// same records:
//
//     apples: 12
//     pears: 7
//     plums seven          <- broken
//     END 3
//
// What changes is what a line end can be. Every segment ends with `eos`, the last one too, so
// the closing line needs no allowance for a missing last line end, and there is no string form
// and no buffer size: the input is a list of lines, a sequence of them, or a reader that is
// read as `File.ReadLines` reads a file.

[Gram("""
	@using DotGram.Examples.Feeds;

	Count : @StockCount =
		lines: Line* recover eos => @(new Unreadable(parserLine, parserText))
		& total: Total & eof
		=> @(new StockCount(lines, total))

	Line : @StockLine = item: Name & ':' & ' ' & count: Digit+ & eos => @(new Stocked(item, Number(count)))

	Total : @int = "END" & ' ' & count: Digit+ & eos => @(Number(count))

	Name  = ['a'..'z' | 'A'..'Z']+
	Digit = ['0'..'9']

	parse Count
	""")]
public static partial class StockCountSegments
{
	/// <summary>A count read from a reader, one line to a segment.</summary>
	/// <exception cref="FormatException">No closing line, or anything after it.</exception>
	public static StockCount Read(TextReader input)
	{
		return ParseCount(input);
	}

	/// <summary>A count read from lines.</summary>
	/// <exception cref="FormatException">No closing line, or anything after it.</exception>
	public static StockCount Read(IReadOnlyList<string> lines)
	{
		return ParseCount(lines);
	}

	// Reachable from the grammar's `=>`, which becomes a method of this same class.
	static int Number(string digits)
	{
		return int.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);
	}
}
