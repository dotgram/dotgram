using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples.Feeds;

// An order sheet as a person writes one: two header lines, then items and remarks in any
// order.
//
//     Order for: Hill Street Cafe
//     Deliver: Monday, before 8
//     12 x milk 1l
//     3 x oat milk, the barista one
//     # the croissants were stale last week, ask
//     x2 sugar                                    <- the count on the wrong side
//     4 x butter
//
// It is an example of docs/feeds.md, among the files kept by hand, and the same grammar;
// the tests hold the two to one text.
//
// The header lines are in the same sequence as the items, in the order they were read,
// because every rule here builds a `SheetLine` and `Sheet` collects them all. The plain
// `parse Sheet yield` of a sequence rule publishes an overload over a `TextReader`, whose lines
// are the segments of the grammar's `eos`, and hands them over as they are read (docs/syntax.md
// §6.3). Without `yield` the same overload would read the whole sheet and return an array.
//
// Recovery is for the lines that repeat. A sheet without its header is refused with a
// `FormatException`, thrown where the sequence is walked: nothing is read before then.
//
// `Line` gathers the two kinds of line under one name with `(v: Item | v: Remark) => @(v)`.
// The capture is what makes it a value rather than the text it matched, and the one `=>`
// after the brackets serves both alternatives.

/// <summary>A line of an order sheet.</summary>
public abstract record SheetLine;

/// <summary>Who the order is for.</summary>
public sealed record Customer(string Name) : SheetLine;

/// <summary>When it is wanted, as the person wrote it.</summary>
public sealed record Delivery(string When) : SheetLine;

/// <summary>How many of what.</summary>
public sealed record Ordered(int Quantity, string Product) : SheetLine;

/// <summary>A line for whoever reads the sheet, not for the order.</summary>
public sealed record Remark(string Text) : SheetLine;

/// <summary>A line that is neither an item nor a remark, where it is and why.</summary>
public sealed record Unclear(int Line, string Text, string Message) : SheetLine;

[Gram("""
	Sheet : @SheetLine[] = Customer & Delivery
	                     & Line* recover eos => @(new Unclear(parserLine, parserText, parserMessage))
	                     & eof

	Customer : @SheetLine = "Order for:" & ' '* & name: Rest & eos => @(new Customer(name))
	Delivery : @SheetLine = "Deliver:"   & ' '* & time: Rest & eos => @(new Delivery(time))

	Line   : @SheetLine = (v: Item | v: Remark) => @(v)
	Item   : @SheetLine = quantity: Digit{1,9} & ' '* & 'x' & ' '+ & product: Rest & eos
	                   => @(new Ordered(Number(quantity), product))
	Remark : @SheetLine = '#' & ' '* & text: Rest & eos => @(new Remark(text))

	Rest  = any+
	Digit = ['0'..'9']

	parse Sheet
	""", Carrier = GramCarrier.Tape)]
public static partial class OrderSheet
{
	// Reachable from the grammar's `=>`, which becomes a method of this same class.
	static int Number(string digits)
	{
		return int.Parse(digits, CultureInfo.InvariantCulture);
	}
}
