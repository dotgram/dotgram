using System;

using DotGram;

namespace DotGram.Examples.Languages;

// One grammar, three parsers: an old dialect, a new one, and one that reads them both.
//
// SQL Server took one comparison out of the language and put another in. `a *= b` — the
// outer join written as an operator — was removed after SQL Server 2000; `a is distinct
// from b` arrived in 2022. A version chain can say the second and not the first: an
// additive chain has nowhere to write a removal once, and a subtractive one has the same
// problem the other way round.
//
// So the grammar says which alternative belongs to which reading, and the publication
// says which reading a parser is:
//
//   Version = "2000" | "2008" | "2022"
//
//   … & when Version is "2000"
//
//   parse Test with (Version = "2000") as ParseOld
//
// `Version` is an ordinary rule — the choice of every version there is — and `is` asks
// whether two recognizers have a string in common. `with` substitutes a rule at every one
// of its uses, so under `Version = "2000"` the condition that says "2022" is false, and the
// alternative it stands in is not in that parser. The question is answered when the parsers
// are generated (docs/syntax.md §3.6, §5.1). They share one machine, and all that is left for
// one to ask while it runs is which of the three it is, at an alternative not all of them have.
//
// `ParseAny` names no version, so it keeps the grammar's own `Version` — every version —
// and reads everything. The permissive parser is not a mode; it is the widest argument.

[Gram("""
	trivia       = ' '*
	wordboundary = ['a'..'z']

	// Which reading of the language a parser is for.
	Version = "2000" | "2008" | "2022"

	Name = { ['a'..'z']+ }

	Test : @string
		= l: Name & "*=" & r: Name & when Version is "2000"
			=> @($"{l} left-joined to {r}")
		| l: Name & "is" & "distinct" & "from" & r: Name & when Version is "2022"
			=> @($"{l} differs from {r}")
		| l: Name & "=" & r: Name
			=> @($"{l} equals {r}")

	parse Test with (Version = "2000") as ParseOld
	parse Test with (Version = "2022") as ParseNew
	parse Test as ParseAny
	""")]
public static partial class SqlDialect
{
	// ParseOld, ParseNew and ParseAny, with a Try… beside each, are generated here —
	// three parsers from one grammar and one machine, told apart by a number each hands it.
}
