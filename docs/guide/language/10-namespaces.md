# Namespaces and rebinding

A grammar of more than a page groups its rules: lexical rules apart from syntax, one dialect's rules apart
from another's. Namespaces do the grouping; rebinding — `with (A = B)` — reads the same rules with one of
them replaced, which is how one grammar serves several dialects. The specification is
[`syntax.md` §5–§5.1](../../syntax.md#5-namespaces).

## Namespace blocks

`namespace Name { … }` is a block of declarations. The top of a file is the global namespace; an inner
namespace sees the outer one, and a rule inside is named `Name.Rule` from outside.

```dotgram
namespace Lexical
{
	Word   = { ['a'..'z']+ }
	Number = { ['0'..'9']+ }

	namespace Quoted
	{
		Text = '"' & [^ '"']* & '"'
	}
}

Assign = Lexical.Word & '=' & (Lexical.Number | Lexical.Quoted.Text)

parse Assign
```

```text examples
ParseAssign("x=42") → "x=42"
ParseAssign("x=\"a b\"") → "x=\"a b\""
```

Notes:

- Braces after `namespace Name` are a block, not an expression: in expression position they are an atomic
  group or a count.
- Every namespace becomes a nested static class of the generated code, and a rule's generated type is
  named with its namespaces joined by `_` (`Lexical_Word`).

## Imports

`using Name;` brings the names of a grammar namespace into the current scope unqualified. It stands at
the head of the file or of a `namespace` block, before every declaration (`GRAM2019` otherwise).

```dotgram
using Lexical;

namespace Lexical
{
	Word = { ['a'..'z']+ }
}

namespace Syntax
{
	using Lexical;

	Pair = Word & ':' & Word
}

List = Word & (',' & Word)*

parse Syntax.Pair
parse List
```

```text examples
ParsePair("a:b") → "a:b"
ParseList("a,b,c") → "a,b,c"
```

**What wins.** A rule of the scope's own wins over an imported one of the same name at the top of a
file. Inside a namespace block, declaring a rule whose name already resolves — from an enclosing scope or
through the block's own `using` — is refused (`GRAM3012`): a declaration always means a new rule, and
replacing one is what a rebinding is for (below). Two imports that supply one name are an error where the
name is used unqualified, settled by qualifying it, as in C# (`GRAM3023`); a rule the scope declares itself
wins over both:

```dotgram GRAM3023
using A;
using B;

namespace A { W = 'a' }
namespace B { W = 'b' }

P = W

parse P
```

```dotgram
using Lexical;

namespace Lexical
{
	Word = { ['a'..'z']+ }
}

Word = { ['A'..'Z']+ }

parse Word
parse Lexical.Word as ParseLower
```

```text examples
ParseWord("ABC") → "ABC"
ParseWord("abc") → refused
ParseLower("abc") → "abc"
```

Refused:

```dotgram GRAM3012
Word = { ['a'..'z']+ }

namespace Inner
{
	Word = { ['A'..'Z']+ }
}

parse Inner.Word
```

```dotgram GRAM3003
using Missing;

Word = { ['a'..'z']+ }

parse Word
```

```dotgram GRAM3001
Word = { ['a'..'z']+ }
Word = { ['A'..'Z']+ }

parse Word
```

Notes:

- Shadowing a built-in rule — `trivia`, `wordboundary`, `word`, `any`, `none`, `eol`, `eof` — is the
  language's normal mechanism, at any depth, and is never reported ([trivia](05-trivia-and-words.md)).
- The standard library is the namespace `Std`, in view in every grammar ([standard library](09-std.md)).
- A `@using` imports a C# namespace, not a grammar one ([the grammar and its host](01-grammar-and-host.md#imports-using-and-using)).

## Rebinding in a namespace header

`namespace Name with (A = B, …) { … }` rebinds `A` to `B` for the **whole call graph reached from what
the namespace declares** — not only for calls written inside the block.

```dotgram
B = 'c'
A = B
F = A

namespace Ns with (B = D)
{
	E = A
}

D = 'd'

parse F
parse Ns.E
```

```text examples
ParseF("c") → "c"
ParseE("d") → "d"
ParseE("c") → refused
```

`F`, outside, resolves `A` to the ordinary `B`. `E`, inside, resolves the same `A` through `D` — although
`A` is declared outside and never mentions `D`. `A` itself is untouched; the rules reached from inside are
read as copies with the substitution applied.

**Shadowing or rebinding?** An ordinary declaration changes what a name means only where it is lexically
visible, and a rule declared elsewhere never sees it. A header makes the *same* shared rule behave
differently depending on which call path reached it. Reach for a header when that is the point — two
decimal points over one `Number` rule; reach for an ordinary declaration when a rule's meaning is simply
different from here on.

```dotgram
Digits = ['0'..'9']+
Point  = '.'
Comma  = ','
Number = Digits & (Point & Digits)?

namespace European with (Point = Comma)
{
	Amount = Number & ' ' & "EUR"
}

Amount = Number & ' ' & "USD"

parse Amount
parse European.Amount as ParseEuro
```

```text examples
ParseAmount("1.50 USD") → "1.50 USD"
ParseEuro("1,50 EUR") → "1,50 EUR"
ParseEuro("1.50 EUR") → refused
```

Notes:

- Bindings in one header resolve together, against the scope the header is written in:
  `with (A = B, B = C)` sends a call to `A` all the way to `C`.
- A nested namespace inherits its enclosing one's bindings and may replace any of them.
- Both sides must name a visible rule (`GRAM3006`, `GRAM3007`); one rule bound twice in one header is
  `GRAM3008`; a loop, `with (A = B, B = A)`, is `GRAM3011`.
- A parameterized rule may replace, and be replaced by, one of the same signature — the same number and
  kinds of parameters (`GRAM3009` otherwise); the call's arguments are kept.
- Inside a namespace that binds a name, declaring a rule of that name is refused (`GRAM3010`): replace it
  with a nested header instead. `GRAM3012` is for a declaration that shadows a name from outside with no
  header binding it.
- The left side of a rebinding may be qualified, `with (Std.Digit = D)`; a qualifier that names no namespace
  is `GRAM3006`.
- A replacement whose result does not fit what the replaced rule's readers expect — a declared type, a
  sequence — is `GRAM4014`.
- `with` is required in a header: `namespace N (A = B)` is `GRAM2006`.

Refused:

```dotgram GRAM2006
B = 'b'
D = 'd'

namespace Ns (B = D)
{
	E = B
}

parse Ns.E
```

```dotgram GRAM3006
D = 'd'

namespace Ns with (Missing = D)
{
	E = D
}

parse Ns.E
```

```dotgram GRAM3011
A = 'a'
B = 'b'

namespace Ns with (A = B, B = A)
{
	E = A
}

parse Ns.E
```

```dotgram GRAM3009
L(x)    = x & x
M(x, y) = x & y

namespace Ns with (L = M)
{
	E = L('a')
}

parse Ns.E
```

```dotgram GRAM3010
using Std;

D = 'd'

namespace Ns with (Digit = D)
{
	Digit = 'x'
	E     = Digit
}

parse Ns.E
```

```dotgram GRAM3010
B = 'b'
D = 'd'

namespace Ns with (B = D)
{
	B = 'x'
	E = B
}

parse Ns.E
```

A qualified left side, and one whose qualifier names nothing:

```dotgram
D = ['a'..'f']

namespace Hex with (Std.Digit = D)
{
	Run = Std.Digit+
}

parse Hex.Run
```

```text examples
ParseRun("cafe") → "cafe"
ParseRun("12") → refused
```

```dotgram GRAM3006
D = 'd'

namespace Ns with (Missing.Digit = D)
{
	E = D
}

parse Ns.E
```

## Rebinding one expression: `X with (A = B)`

The same substitution on a single operand, with no block and no name:

```dotgram
Digits = ['0'..'9']+
Point  = '.'
Comma  = ','
Number = Digits & (Point & Digits)?

Row = a: Number & ';' & b: Number with (Point = Comma)

parse Row
```

```text examples
ParseRow("1.5;2,5") → Row { A = "1.5", B = "2,5" }
ParseRow("1.5;2.5") → refused
```

`with` binds as tightly as a quantifier and reaches only the operand it is written on: here `b`'s
`Number`, not `a`'s. `Number+ with (X = Y)` is `(Number+) with (X = Y)`.

**What replaces a rule may be any operand**, not only a rule's name; a choice needs brackets:

```dotgram
Item = ['a'..'z']+
Sep  = ','
List = Item & (Sep & Item)*

Loose = List with (Sep = (',' | ';'))

parse Loose
```

```text examples
ParseLoose("a,b;c") → "a,b;c"
```

The expression becomes a rule of its own, declared where the `with` is written, so it reads the
`trivia`, the imports and the bindings around it. The left side stays a name.

Notes:

- Reach for `X with (…)` when the substitution belongs to one place a rule is used, and for a header once
  more than one call needs it or it deserves a name.
- A `with` inside an argument of a parameterized rule is refused (`GRAM4041`): declare a rule with the
  rebinding and pass that.

## Rebinding a publication

A `parse` or `find` directive may carry the substitution itself, which is how one grammar publishes
several parsers:

```dotgram
Digits = ['0'..'9']+
Point  = '.'
Comma  = ','
Number = Digits & (Point & Digits)?

parse Number                        as ParseInvariant
parse Number with (Point = Comma)   as ParseEuropean
```

```text examples
ParseInvariant("3.14") → "3.14"
ParseEuropean("3,14") → "3,14"
ParseEuropean("3.14") → refused
```

The same rebinding answers conditions: `when Version is "2000"` is decided against the rules as each
publication has them, so `parse Test with (Version = "2000")` and `with (Version = "2022")` are two
parsers of one grammar ([conditions](11-guards-and-switch.md#conditions-when-a-is-b)).

**How substitutions stack.** Where several reach one call, the one written closer to where reading starts
applies on top: an outer `with` over an inner one, a caller's header over the header of a namespace it calls
into, a publication's `with` over everything. A binding of a name a lower substitution already replaced does
nothing, since nothing calls that name any more:

```dotgram
B = 'b'
D = 'd'
Y = 'y'

namespace Ns with (B = D)
{
	E = B
}

R = Ns.E with (B = Y)

parse R
```

```text examples
ParseR("d") → "d"
ParseR("y") → refused
```

Inside `Ns`, `B` is already `D`, so the outer `B = Y` has nothing to replace. A substitution that loops once
stacked is `GRAM3011`; two that each need the other applied first are `GRAM4017`.
