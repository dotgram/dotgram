# The standard library, `Std`

The lexemes every grammar writes and none should have to — digits, an identifier, a number as a value, a
comment, a quoted string — are a grammar of their own, carried inside the generator. Every grammar has it
under the name `Std`. This page shows every rule of it; a short table is in
[the grammar and its host](01-grammar-and-host.md#the-standard-library-at-a-glance), and the specification
is [`syntax.md` §5.2](../../syntax.md#52-the-standard-library-std). `Std` is a namespace like any other, and
how names are opened and what wins between them is told with [namespaces](10-namespaces.md).

## Using the library

`Std.Integer` names a rule in full, with nothing declared. `using Std;` opens the namespace as `using` opens
any other, after which `Integer` is enough.

```dotgram
using Std;

trivia = { Spacing* }

Pair : @int = a: Integer & ',' & b: Std.Integer => @(a * b)

parse Pair
```

```text examples
ParsePair("6, 7") → 42
```

Notes:

- The library has no `trivia` of its own: a token is its characters with nothing between them, and the
  calling grammar spaces around the call as around anything else.
- Whatever the grammar does not call is not compiled: the library costs the rules used and nothing else.
- At the top of a file a rule of the grammar's own wins over an opened name; a namespace that opens the
  library and declares one of its names is refused (`GRAM3012`). A grammar with lexemes of its own names
  the library in full and keeps its names.

```dotgram GRAM3012
namespace Mine
{
	using Std;

	Digit = ['0'..'7']
}

parse Mine.Digit
```

## One character

| Rule | Reads |
| --- | --- |
| `Std.Digit` | one of `'0'..'9'` |
| `Std.HexDigit` | one of `'0'..'9'`, `'a'..'f'`, `'A'..'F'` |
| `Std.Letter` | one letter, `\p{L}` |
| `Std.Space` | one space or tab |
| `Std.Whitespace` | one space, tab, `\r` or `\n` |

```dotgram
parse Std.Digit      as Digit
parse Std.HexDigit   as Hex
parse Std.Letter     as Letter
parse Std.Space      as Space
parse Std.Whitespace as Whitespace
```

```text examples
Digit("7") → "7"
Digit("x") → refused
Hex("F") → "F"
Hex("g") → refused
Letter("ж") → "ж"
Letter("_") → refused
Space("\t") → "\t"
Space("\n") → refused
Whitespace("\n") → "\n"
```

Each is one item and may stand in an element set: `[Std.Digit | '_']`.

## Runs, and an identifier

| Rule | Reads |
| --- | --- |
| `Std.Digits` | one or more digits |
| `Std.HexDigits` | one or more hexadecimal digits |
| `Std.Blank` | one or more spaces and tabs |
| `Std.Spacing` | one or more whitespace characters, line endings included |
| `Std.Identifier` | a letter or `_`, then letters, digits and `_`, in the Unicode reading |

```dotgram
parse Std.Digits     as Digits
parse Std.HexDigits  as HexDigits
parse Std.Blank      as Blank
parse Std.Spacing    as Spacing
parse Std.Identifier as Identifier
```

```text examples
Digits("0042") → "0042"
Digits("") → refused
HexDigits("1fA0") → "1fA0"
Blank(" \t ") → " \t "
Blank(" \n") → refused
Spacing(" \r\n\t") → " \r\n\t"
Identifier("_tmp1") → "_tmp1"
Identifier("имя2") → "имя2"
Identifier("2x") → refused
```

Notes:

- These are text: the value of each is what it read.
- The runs are not braced in the library; a grammar that needs them atomic writes `{ Std.Digits }`
  ([composition](03-composition.md#atomic-groups---)).

## Numbers as values

| Rule | Reads | Value |
| --- | --- | --- |
| `Std.Integer` | digits | `int`, by `int.Parse` |
| `Std.Long` | digits | `long` |
| `Std.Decimal` | digits, then optionally `.` and digits | `decimal` |
| `Std.Double` | digits, an optional fraction, an optional exponent `e`/`E` with an optional sign | `double` |

```dotgram
parse Std.Integer as Integer
parse Std.Long    as Long
parse Std.Decimal as Decimal
parse Std.Double  as Double
```

```text examples
Integer("042") → 42
Long("9000000000") → 9000000000
Decimal("3.25") → 3.25
Decimal("3") → 3
Decimal(".5") → refused
Double("1.5e3") → 1500
Double("2E-2") → 0.02
Integer("-1") → refused
```

Notes:

- The numbers are read in the invariant culture, so a grammar means the same thing on every machine.
- They are unsigned: whether `-` is part of a number or an operator in front of one is the calling
  grammar's question. Write `'-'? & Std.Integer` and build the sign in a `=>`.
- What the .NET parser refuses it refuses as it always does: an `Integer` too wide for `int` throws
  `OverflowException` out of the parse, not a refusal. A grammar that wants a message, a location or a
  range of its own declares the rule itself — what a lexeme *means* takes code, and the code is the
  grammar's.

## Comments

| Rule | Reads |
| --- | --- |
| `Std.LineComment(start)` | `start` to the end of the line, the line ending left unread |
| `Std.BlockComment(open, close)` | `open` to the first `close` |
| `Std.Nested(open, close)` | `open` to the `close` that balances it, holding other pairs to any depth |

```dotgram
SqlLine  = Std.LineComment("--") & eol
CBlock   = Std.BlockComment("/*", "*/")
Nested   = Std.Nested("/*", "*/")

parse SqlLine
parse CBlock
parse Nested
find  CBlock as Blocks
```

```text examples
ParseSqlLine("-- note\n") → "-- note\n"
ParseCBlock("/* a */") → "/* a */"
ParseCBlock("/* a /* b */ c */") → refused
Blocks("/* a /* b */ c */") → ["/* a /* b */"]
ParseNested("/* a /* b */ c */") → "/* a /* b */ c */"
ParseNested("/* a /* b */") → refused
```

`LineComment` stops short of the line ending, so a grammar that counts lines still sees every one.
`BlockComment` ends at the first `close`; `Nested` ends where its opens and closes balance, and a pair that
never balances is refused where it opened.

`Nested` is the one rule of the library that reaches itself. Braced `trivia` and the lexer of a split grammar
read it with a counter, which has to know which delimiter stands where it stopped: so its arguments are two
literals, neither empty, that cannot begin at one place (`GRAM4037` otherwise).

```dotgram
trivia = { (Std.Spacing | Std.LineComment("--") | Std.Nested("/*", "*/"))* }

Word = { ['a'..'z']+ }
Pair = Word & '=' & Word

parse Pair
```

```text examples
ParsePair("a /* x /* y */ */ = b -- end") → "a /* x /* y */ */ = b"
```

```dotgram GRAM4037
trivia = { (Std.Spacing | Std.Nested("(*", "(*)"))* }

Word = { ['a'..'z']+ }

parse Word
```

## Quoted text

| Rule | Reads | Value |
| --- | --- | --- |
| `Std.Quoted(quote)` | `quote` to `quote`, with `quote` doubled inside | the text as written |
| `Std.Escaped(quote, escape)` | `quote` to `quote`, with `escape` in front of a quote or anything else | the text as written |

```dotgram
SqlString  = Std.Quoted('\'')
SqlName    = Std.Quoted('"')
CString    = Std.Escaped('"', '\\')

parse SqlString
parse SqlName
parse CString
```

```text examples
ParseSqlString("'it''s'") → "'it''s'"
ParseSqlString("'it's'") → refused
ParseSqlName("\"a\"\"b\"") → "\"a\"\"b\""
ParseCString("\"a\\\"b\"") → "\"a\\\"b\""
ParseCString("\"a\\\"") → refused
ParseCString("\"tab\\t\"") → "\"tab\\t\""
```

Both give back the text between and including the quotes as written, escapes and all: what an escape stands
for differs between languages that agree on how it is spelled, so turning `\t` into a tab is the calling
grammar's `=>`. They are written with the until idiom rather than an element set, which is the one
lookahead a lexer reads as a pattern: in a grammar read over tokens they are tokens.

## The built-in rules

`any`, `none`, `eol`, `eof` and `eos` are built into every grammar rather than written in `Std`, and so are
`trivia`, `wordboundary` and `word`. Each is shown with runs where it is told in full:

| Rule | Runs in |
| --- | --- |
| `any`, `none`, `eol`, `eof` | [elements](02-elements.md#any-none-eol-eof), [the grammar and its host](01-grammar-and-host.md#built-in-rules-and-supplied-names-at-a-glance) |
| `eos` | [elements](02-elements.md#eos-and-input-in-segments) (compiled only: such a grammar reads lists of strings) |
| `trivia` | [trivia](05-trivia-and-words.md#trivia-what-may-stand-between-operands) |
| `wordboundary` | [words](05-trivia-and-words.md#wordboundary) |
| `word` | [words](05-trivia-and-words.md#word) |
