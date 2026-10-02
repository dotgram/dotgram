using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples.Feeds;

// A logbook kept by the people who maintain the machines, an entry to a line:
//
//     2026-10-01 07:55 press-4 oil topped up
//     2026-10-01 09:10 press-2 belt replaced
//     2026-10-1 11:30 press-4 noise from the bearing     <- a day of one digit
//     2026-10-01 14:05 lathe-1 calibrated
//
// It is the repository README's third example of a feed written by people, and the same
// grammar; the tests hold the two to one text.
//
// The `recover` here has no `=>`, which is the difference from PriceListExample. A bad
// line is dropped from the sequence instead of arriving in it, so the entries come back
// as themselves — `IEnumerable<LogEntry>`, with nothing in it to filter out — and what
// was dropped goes to `OnRecovered`, the `partial void` the generated class declares
// (docs/syntax.md §8.3). Leave it unimplemented and the compiler removes every call to it
// together with its arguments: a parse then counts no lines and copies no text for a
// channel nobody listens on. LoggingFeedExample says more about why it is a `partial
// void` rather than an event.
//
// `stream yield` makes the read lazy, so the report of a bad line is made when that line
// is read: between the entry before it and the entry after it, not at the end.

/// <summary>An entry of the logbook.</summary>
public sealed record LogEntry(DateTime At, string Machine, string Note);

[Gram("""
	Log   : @LogEntry[] = Entry* recover eol

	Entry : @LogEntry = at: Stamp & ' '+ & machine: Name & ' '+ & note: Rest & eol
	                 => @(new LogEntry(ToTime(at), machine, note))

	Stamp = Digit{4} & '-' & Digit{2} & '-' & Digit{2} & ' ' & Digit{2} & ':' & Digit{2}
	Name  = [^ ' ' | '\r' | '\n']+
	Rest  = [^ '\r' | '\n']+
	Digit = ['0'..'9']

	parse Log as Read stream yield
	""")]
public static partial class Logbook
{
	/// <summary>Where the lines the logbook could not read are reported, or null to drop them.</summary>
	/// <remarks>
	/// The README writes them to <c>Console.Error</c>. An example meant to be called from
	/// anywhere, tests included, cannot own the console, so this one is told where to write.
	/// </remarks>
	[ThreadStatic]
	public static System.IO.TextWriter? Rejected;

	// Reachable from the grammar's `=>`, which becomes a method of this same class.
	static DateTime ToTime(string text)
	{
		return DateTime.ParseExact(text, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
	}

	static partial void OnRecovered(
		string rule, string text, long position, int line, int column, int ordinal, string message)
	{
		Rejected?.WriteLine($"line {line}: {message} ({text})");
	}
}
