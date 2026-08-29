using System;

using DotGram;

namespace DotGram.Examples;

// A decimal number, written once, read under two different decimal points without a
// second copy of the rule.
//
// `Number` is an ordinary rule: digits, then optionally `Point` and more digits. `Point`
// is declared and used the same way any other rule is — nothing about `Number` says it
// is replaceable.
//
// The replacement happens at the publication, not in the rule:
//
//   namespace Ctx with (Point = Comma)
//   {
//       parse Number as ParseEuropeanNumber
//   }
//
// `ParseEuropeanNumber` calls the same `Number`, with every rule it transitively reaches
// specialized under `Point -> Comma` first (docs/syntax.md §5.1). `ParseNumber`, declared
// outside the namespace, still calls the original `Number` with the original `Point` — one
// grammar, two publications, and `Number`'s own `=>` never had to learn there was a
// second decimal point at all: it reads the digits on either side of whatever `Point`
// matched and never looks at the character itself.

[Gram("""
	Digit = ['0'..'9']
	Point = '.'

	Number : @decimal = whole: Digit+ & Point & frac: Digit+ => @(Whole(whole) + Fraction(frac))
	                   | whole: Digit+                        => @(Whole(whole))

	parse Number as ParseNumber

	Comma = ','

	namespace Ctx with (Point = Comma)
	{
		parse Number as ParseEuropeanNumber
	}
	""")]
public static partial class LocaleNumber
{
	// ParseNumber, TryParseNumber, ParseEuropeanNumber and TryParseEuropeanNumber are
	// generated here.

	static decimal Whole(string digits)
	{
		var value = 0m;

		foreach (var digit in digits)
			value = value * 10 + (digit - '0');

		return value;
	}

	static decimal Fraction(string digits)
	{
		var value = 0m;
		var scale = 0.1m;

		foreach (var digit in digits)
		{
			value += (digit - '0') * scale;
			scale /= 10;
		}

		return value;
	}
}
