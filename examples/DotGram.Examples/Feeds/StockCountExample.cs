using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using DotGram;

namespace DotGram.Examples.Feeds;

// A stock count, one item to a line, and a closing line that says how many there were:
//
//     apples: 12
//     pears: 7
//     plums seven          <- broken
//     END 3
//
// Like RecoveringFeedExample, a broken line costs itself and not the file: `recover eol`
// skips it, and the `=>` beside it turns what was skipped into a line of its own. What is
// different is the closing line. `END 3` begins the way an item does — a name — and only
// fails at the space where an item has its colon, so a recovering repetition would take it
// for one more broken line and leave nothing for the total.
//
// It does not, because a repetition gives a turn back when what follows it needs the text:
// the reading of `Line*` stops where `Total & eof` can be read, and nowhere earlier. So the
// closing line is the total whenever the file ends with it, and a broken line only where it
// does not. A last item with no line end, `plums: 7END 3`, is the case that shows the limit:
// recovering it would take the closing line with it, and giving it back leaves a line that
// is neither, so the file is refused.

/// <summary>A line of the count: an item and how many, or a line that is not one.</summary>
public abstract record StockLine;

/// <summary>An item that was counted.</summary>
public sealed record Stocked(string Item, int Count) : StockLine;

/// <summary>A line that was not an item, where it was and what it said.</summary>
public sealed record Unreadable(int Line, string Text) : StockLine;

/// <summary>The lines of a count and the total its closing line gives.</summary>
public sealed record StockCount(IReadOnlyList<StockLine> Lines, int Total)
{
	/// <summary>Whether the closing line counts the items that were read.</summary>
	public bool Balances
	{
		get
		{
			var counted = 0;

			foreach (var line in Lines)
				if (line is Stocked)
					counted++;

			return counted == Total;
		}
	}
}

[Gram("""
	@using DotGram.Examples.Feeds;

	Count : @StockCount =
		lines: Line* recover eol => @(new Unreadable(parserLine, parserText))
		& total: Total & eof
		=> @(new StockCount(lines, total))

	Line : @StockLine = item: Name & ':' & ' ' & count: Digit+ & eol => @(new Stocked(item, Number(count)))

	// The closing line, whose last line end may be missing.
	Total : @int = "END" & ' ' & count: Digit+ & eol? => @(Number(count))

	Name  = ['a'..'z' | 'A'..'Z']+
	Digit = ['0'..'9']

	parse Count stream
	""")]
public static partial class StockCountReader
{
	/// <summary>A count held in memory.</summary>
	/// <exception cref="FormatException">No closing line, or anything after it.</exception>
	public static StockCount Read(string text)
	{
		return ParseCount(text);
	}

	/// <summary>
	/// A count read from a reader through a buffer of <paramref name="bufferSize"/> characters,
	/// which only has to hold what the reading cannot let go of yet.
	/// </summary>
	/// <exception cref="FormatException">No closing line, or anything after it.</exception>
	public static StockCount Read(TextReader input, int? bufferSize = null)
	{
		return ParseCount(input, bufferSize);
	}

	// Reachable from the grammar's `=>`, which becomes a method of this same class.
	static int Number(string digits)
	{
		return int.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);
	}
}
