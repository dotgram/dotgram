using System;

using DotGram;

namespace DotGram.Examples.Formats;

// A line of measurements, and the standard library doing nearly all of it:
//
//     cpu=0.75 mem=2048 host="db-1" up=36000 # sampled
//
// `using Std;` opens the library every grammar has without declaring it (docs/syntax.md
// §5.2). It travels inside the generator, so there is no package to add and no file to
// copy, and whatever a grammar does not call is not compiled.
//
// What it is worth is this line:
//
//     trivia = { (Spacing | LineComment("#")) * }
//
// Whitespace and comments, said once, by name. `LineComment` takes what opens it because
// that is the only thing that differs between one language's line comment and the next —
// `LineComment("--")` for SQL, `LineComment("//")` for C-shaped ones.
//
// And the numbers come back as numbers. `Integer`, `Long`, `Decimal` and `Double` are
// rules with values: they read the invariant culture, so a grammar means the same thing
// on every machine, and what they refuse they refuse as `int.Parse` does. A grammar that
// writes `['0'..'9']+` and parses the text afterwards has done the same work twice and
// has to remember the culture itself.
//
// The library stops where meaning begins. `Quoted('"')` gives back the text between the
// quotes **as written**, doubled quotes and all, because what an escape stands for
// differs between languages that agree on how it is spelled — so `Unquoted` below is
// this grammar's business and not the library's.
//
// The whole of the standard library, for reference:
//
//   one character      Digit HexDigit Letter Space Whitespace
//   runs of them       Digits HexDigits Blank Spacing Identifier
//   numbers, as values Integer Long Decimal Double
//   comments           LineComment(start) BlockComment(open, close)
//   quoted text        Quoted(quote) Escaped(quote, escape)

/// <summary>One measurement: what it is called, and what it read as.</summary>
public sealed record Reading(string Key, object Value);

[Gram("""
	@using DotGram.Examples.Formats;

	using Std;

	// Whitespace and `#` comments, both from the library, both skipped between operands.
	trivia = { (Spacing | LineComment("#"))* }

	Line : @Reading[] = Reading+ & eof

	Reading : @Reading = key: Identifier & '=' & value: Value => @(new Reading(key, value))

	// The lookahead settles which numeric form is in front, so neither is read and given
	// back. Without it the two would still work, on backtracking — this says it instead.
	Value : @object = ?=(Digits & '.') & d: Decimal => @(d)
	                | n: Long                       => @(n)
	                | t: Quoted('"')                => @(Unquoted(t))

	parse Line
	""")]
public static partial class MetricsLine
{
	/// <summary>What the quotes were around, with a doubled quote read as one.</summary>
	/// <remarks>
	/// The library hands back the text as written, quotes and all: it knows how a quoted
	/// string is spelled and not what this format means by one.
	/// </remarks>
	static string Unquoted(string quoted) =>
		quoted.Substring(1, quoted.Length - 2).Replace("\"\"", "\"");

	/// <summary>A line read into a lookup, for a caller that wants one.</summary>
	public static System.Collections.Generic.Dictionary<string, object> Read(string line)
	{
		var readings = new System.Collections.Generic.Dictionary<string, object>(StringComparer.Ordinal);

		foreach (var reading in ParseLine(line))
			readings[reading.Key] = reading.Value;

		return readings;
	}
}
