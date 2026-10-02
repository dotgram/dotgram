using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples.Feeds;

// A price list somebody keeps by hand, a name and a price to a line:
//
//     Espresso       2.40
//     Cappuccino     3.10
//     Flat white     3,20      <- a comma for the point
//     Croissant      2.1O      <- the letter O for a zero
//     Muffin         2.80
//
// It is the repository README's first example of a feed written by people, and the same
// grammar; the tests hold the two to one text.
//
// Three things make it read the way a person's file has to be read. `recover eol` lets a
// line be wrong without the file being wrong: the parser skips to the next line end and
// reads on. The `=>` beside it turns the skipped line into a `Typo`, so it arrives in the
// sequence in its own place, with the line a person opens the file at and a message
// giving the offset in the file where reading stopped. And `stream yield` publishes the
// list as a lazy `IEnumerable<PriceLine>` over a `TextReader`: a line is read when the
// caller asks for the next one, so a price list of any size costs one line at a time.
//
// The name is everything up to the first digit, trimmed, so an item may have spaces in
// it and line its prices up however it likes — and may not have a digit in its name,
// which is the price of a grammar this short.

/// <summary>A line of the price list: a price, or a line that is not one.</summary>
public abstract record PriceLine;

/// <summary>An item and what it costs.</summary>
public sealed record Price(string Item, decimal Amount) : PriceLine;

/// <summary>A line that could not be read, where it is and why.</summary>
public sealed record Typo(int Line, string Text, string Message) : PriceLine;

[Gram("""
	PriceList : @PriceLine[] = Entry* recover eol => @(new Typo(parserLine, parserText, parserMessage))

	Entry : @PriceLine = item: Item & amount: Amount & ' '* & eol
	                  => @(new Price(item.Trim(), ToDecimal(amount)))

	Item   = [^ '0'..'9' | '\r' | '\n']+
	Amount = ['0'..'9']+ & '.' & ['0'..'9']{2}

	parse PriceList as Read stream yield
	""")]
public static partial class PriceList
{
	// Reachable from the grammar's `=>`, which becomes a method of this same class.
	static decimal ToDecimal(string text)
	{
		return decimal.Parse(text, CultureInfo.InvariantCulture);
	}
}
