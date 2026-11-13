# A grammar and its host

A `.gram` grammar describes how text becomes typed values; the generator turns it into C#
methods on a partial class, the **host**. This page covers the file, its declarations and the
`@` that switches between the grammar's names and C#'s. The specification is
[`syntax.md` §1–§2](../../syntax.md#1-the-file-and-the-host-class).

## Where a grammar lives

A grammar is attached to a `partial` class with `[Gram]`, in one of three ways:

```csharp
[Gram]                                  // the file FeedGrammar.gram, found by the class's name
public partial class FeedGrammar;

[Gram("formats/feed.gram")]             // a file named explicitly
public partial class FeedGrammar;

[Gram("""                               // the grammar written right here
	Digits = ['0'..'9']+
	parse Digits
	""")]
public partial class Numbers;
```

A `.gram` file is given to the build as `<AdditionalFiles Include="…" />`. Every method the grammar
publishes becomes a static member of the host: `Numbers.ParseDigits("42")`.

Notes:

- The host must be `partial` (`GRAM0002`). A file that cannot be found is `GRAM0003`, two files
  answering to one name `GRAM0004`.
- The host may also be generated outside a build by the `dotnet dotgram` tool ([`cli.md`](../../cli.md)).

## A first grammar

```dotgram
Sum    : int = left: Sum & '+' & right: Number => @(left + right)
             | n: Number                        => n
Number : int = ['0'..'9']+ => @int.Parse(parserText)

parse Sum
```

```text examples
ParseSum("1+2+3") → 6
ParseSum("42") → 42
ParseSum("1+") → refused
TryParseSum("1+x") → refused
```

Everything a grammar is made of is in it:

- a **rule** is a name, an optional type after `:`, `=`, and a body;
- `&` joins operands into a sequence and `|` separates alternatives;
- `left:`, `right:` and `n:` **capture** what an operand read;
- `=>` builds the alternative's value, here with C# behind `@`;
- `parse Sum` **publishes** the rule: it makes `ParseSum`, which throws `FormatException` on a
  refusal, and `TryParseSum`, which answers a `Match<int>`.

`Sum` calls itself first; such a rule is read as a loop and its `=>` as a fold
([rules](04-rules.md#left-recursion)).

## Layout and comments

Whitespace and line breaks in a grammar mean nothing, and comments are C#'s: `// to the end of
the line` and `/* … */`, which does not nest.

```dotgram
/* A grammar may be laid out freely. */
Pair = Word & ',' & Word   // one rule
Word = ['a'..'z']+         // another; no separator between them

parse Pair
```

```text examples
ParsePair("ab,cd") → "ab,cd"
ParsePair("ab, cd") → refused
```

**Rules need no separator.** Every pair of operands is joined by `&`, `|` or `~`, so an expression
can never run on into the next rule: a name followed by `=`, `:` or `(` where the expression is
already complete begins a new declaration.

Notes:

- Spaces in the grammar are not spaces in the input: `"ab, cd"` is refused above. What may stand
  between operands in the input is the rule `trivia` ([briefly below](#whitespace-in-the-input-trivia-briefly),
  in full in [trivia](05-trivia-and-words.md)).
- An unterminated `/*` is `GRAM1003`; a character nothing begins is `GRAM1005`.

## Declarations

A grammar is a sequence of declarations, in any order:

| Declaration | Example | Page |
| --- | --- | --- |
| a rule | `Name = body`, `Name : Type = body`, `Name(p) = body` | [rules](04-rules.md) |
| a publication | `parse Name`, `find Name`, `internal parse X as Y` | [publication](08-publication.md) |
| a namespace | `namespace Lexical { … }`, `namespace N with (A = B) { … }` | [namespaces](10-namespaces.md) |
| an import | `using Lexical;`, `@using System.Text;` (first in the file or block) | below |
| a context type | `context : @Names` | [context and state](15-context-and-state.md) |
| a state type | `state : @Overflow` | [context and state](15-context-and-state.md) |

The words that begin a declaration — `namespace`, `parse`, `find`, `public`, `internal`,
`private`, `positional`, `context`, `state` — are words only there. A rule may be called `parse`
or `state`: what follows the name (`=`, `:`, `(`) decides.

```dotgram
parse  = "parse"
state  = "state"
Phrase = parse & ' ' & state

parse Phrase
```

```text examples
ParsePhrase("parse state") → "parse state"
```

Anything else at the top of a grammar is `GRAM2002`.

## Two vocabularies: `@` and its absence

A bare name is looked up among the grammar's rules (and, inside a rule, its captures and
parameters). A name after `@` is C#'s: a type, a method, a property. There is no fallback in
either direction.

| Written | Means |
| --- | --- |
| `Row` | the rule `Row` |
| `@Row` | the C# type or member `Row` |
| `string`, `int`, `char`, `bool`, `decimal`, … | always the C# type; these words cannot name a rule |
| `@(expression)` | a C# expression, handed to the compiler as written |

What may follow `@`: a name (`@DateOnly`), a dotted name (`@Encoding.UTF8`), a call
(`@int.Parse(text)`), a generic name (`@List<int>`), or a parenthesized C# expression
(`@(y * 10000 + m * 100)`).

```dotgram
@using System;
@using System.Globalization;

Year : int = ['0'..'9']{4} => @int.Parse(parserText, CultureInfo.InvariantCulture)
Date : @DateOnly = y: Year & '-' & m: Two & '-' & d: Two => @(new DateOnly(y, m, d))
Two  : int = ['0'..'9']{2} => @(int.Parse(parserText))

parse Date
```

```text examples
ParseDate("2026-11-10") → 11/10/2026
ParseDate("2026-1-10") → refused
```

Notes:

- The generator does not parse the C# after `@`; it finds where it ends and hands the text to
  the compiler, so C# mistakes are C# errors, reported against the grammar's line.
- An exception thrown by the C# is not a refusal: `ParseDate("2026-13-10")` matches the text and
  then `new DateOnly` throws `ArgumentOutOfRangeException` out of the parse. A value that may not
  exist is checked with a guard ([guards](11-guards-and-switch.md)).
- Inside `@(…)` and the arguments of `@M(…)` everything is C#: `=> @M(a, b)` and `=> @(M(a, b))`
  are the same construction.
- There is no block form (`@{ … }`): code belongs in the host class.
- `@(` must be written together: `@ (a + b)` is a call on a missing name. An unterminated
  `@(` is `GRAM1006`.

## Imports: `@using` and `using`

`@using X.Y;` imports a C# namespace for the C# the grammar names; `using X;` imports a grammar
namespace. Both are written as C# writes a `using` directive, semicolon included, and stand
before the declarations of a file or of a `namespace` block.

```dotgram
@using System.Text;
using Lexical;

namespace Lexical
{
	Word = ['a'..'z']+
}

Pair : string = a: Word & '=' & b: Word => @(new StringBuilder(a).Append(b).ToString())

parse Pair
```

```text examples
ParsePair("ab=cd") → "abcd"
```

An import names a namespace declared anywhere in the grammar, before or after it. A `using`
written after a declaration is refused: move it up (`GRAM2019`).

```dotgram GRAM2019
Word = ['a'..'z']+

using Lexical;
```

Notes:

- No C# namespace is imported for a grammar by default — not even `System`: `@DateOnly` above
  needs `@using System;` (`GRAM3004` otherwise).
- An unknown grammar namespace in `using` is `GRAM3003`. Two imports that supply one name are an
  error where the name is used, settled by qualifying it (`Lexical.Word`).
- The standard library is the namespace `Std`, always present: `Std.Integer`, or `using Std;`
  and then `Integer` ([standard library](09-std.md)).

## Whitespace in the input: `trivia`, briefly

Many examples in this guide read text with spaces or comments between its parts. What may stand
there is the rule `trivia`, which every grammar has: it is woven in at every `&`, and it is empty
(`none`) unless the grammar says otherwise.

```dotgram
trivia = { Std.Spacing* }

Word = { ['a'..'z']+ }
Pair = Word & '=' & Word

parse Pair
```

```text examples
ParsePair("a=b") → "a=b"
ParsePair(" a =  b ") → "a =  b"
```

Three things to know before [the full treatment](05-trivia-and-words.md):

- **Braces** around `trivia` and around lexemes such as `Word` say "what this read stays read". They
  are worth writing almost always; why is in [composition](03-composition.md#atomic-groups---) and
  [trivia](05-trivia-and-words.md#comments-and-why-trivia-is-braced).
- **A lexeme must not let trivia in.** `Word = ['a'..'z']+` is one repetition, so nothing is woven
  inside it. A rule built of several parts that is one token — a number with a fraction — is written
  in a namespace that says `trivia = none`, or with `~` between its parts, which allows no trivia.
- **Keywords** are guarded against running into the next word once the grammar declares
  `wordboundary`, the characters a word is made of ([words](05-trivia-and-words.md#wordboundary)).

## The standard library at a glance

The lexemes every grammar writes are a grammar of their own, `Std`, which every grammar has: named in
full, `Std.Integer`, or opened with `using Std;`. Each rule is spelled with no trivia inside it; the
calling grammar spaces around the call as it does around anything else.

| Rule | Reads | Gives | Example |
| --- | --- | --- | --- |
| `Std.Digit`, `Std.HexDigit`, `Std.Letter` | one digit, hex digit, letter (`\p{L}`) | text | `7`, `f`, `ж` |
| `Std.Space`, `Std.Whitespace` | one space or tab; one of those or a line ending | text | `\t` |
| `Std.Digits`, `Std.HexDigits` | one or more of them | text | `0042`, `1F` |
| `Std.Blank`, `Std.Spacing` | spaces and tabs; any whitespace, line endings included | text | `" \t"` |
| `Std.Identifier` | a letter or `_`, then letters, digits and `_` | text | `_tmp1` |
| `Std.Integer`, `Std.Long` | digits | `int`, `long` | `42` |
| `Std.Decimal` | digits, optionally `.` and digits | `decimal` | `3.25` |
| `Std.Double` | digits, an optional fraction and exponent | `double` | `1.5e3` |
| `Std.LineComment(start)` | `start` to the end of the line | text | `-- note` |
| `Std.BlockComment(open, close)` | `open` to the first `close` | text | `/* a */` |
| `Std.Nested(open, close)` | `open` to the `close` that balances it | text | `/* a /* b */ c */` |
| `Std.Quoted(quote)` | a quoted text, the quote doubled inside | text as written | `'it''s'` |
| `Std.Escaped(quote, escape)` | a quoted text, `escape` before a quote or anything | text as written | `"a\"b"` |

```dotgram
using Std;

trivia = { (Spacing | LineComment("--"))* }

Assign : @string = name: Identifier & '=' & value: Integer & ';' => @(name + "=" + (value * 2))

parse Assign
```

```text examples
ParseAssign("x = 21; -- doubled") → "x=42"
ParseAssign("_n1=0;") → "_n1=0"
```

The numbers are unsigned and read in the invariant culture; a number too large for its type throws, as
`int.Parse` does. Every rule, with its edge cases, is in [the standard library](09-std.md).

## Built-in rules and supplied names at a glance

Eight rules every grammar has without declaring them. They are ordinary rules, not keywords: a grammar
may declare its own of the same name, and `trivia` and `wordboundary` exist to be redeclared.

| Rule | Matches | Example | In full |
| --- | --- | --- | --- |
| `any` | one input item, whatever it is; fails only at the end | `'\'' & any & '\''` | [elements](02-elements.md#any-none-eol-eof) |
| `none` | nothing; always succeeds | `'a' & ('b' \| none)` | [elements](02-elements.md#any-none-eol-eof) |
| `eol` | one line ending: `\r\n`, `\n` or `\r` | `[^ '\r' \| '\n']* & eol` | [elements](02-elements.md#any-none-eol-eof) |
| `eof` | the end of the input, reading nothing | `Header & Row* & eof` | [elements](02-elements.md#any-none-eol-eof) |
| `eos` | the end of one segment, where the input is a list of strings | `Value & eos` | [elements](02-elements.md#eos-and-input-in-segments) |
| `trivia` | what may stand between operands; `none` unless redeclared | `trivia = { ' '* }` | [trivia](05-trivia-and-words.md) |
| `wordboundary` | the characters a word is made of; `none` unless redeclared | `wordboundary = ['a'..'z']` | [words](05-trivia-and-words.md#wordboundary) |
| `word` | one whole word of those characters | `"set" & word` | [words](05-trivia-and-words.md#word) |

```dotgram
wordboundary = ['a'..'z' | '0'..'9']

Quote = '\'' & any & '\''
Set   = "set" & ' '* & word & '=' & (word | none)
Lines = ([^ '\r' | '\n']* & { eol })* & eof

parse Quote
parse Set
parse Lines
```

```text examples
ParseQuote("'x'") → "'x'"
ParseSet("set x=y") → "set x=y"
ParseSet("set x=") → "set x="
ParseSet("setx=y") → refused
ParseLines("a\nb\n") → "a\nb\n"
```

`"set"` is a word literal once `wordboundary` is declared, so it does not match the start of `setx`,
although `' '*` may read nothing.

**Names the parser supplies to C#.** Inside `=>`, `when` and a `recover` factory, beside the captures:

| Name | Is | In full |
| --- | --- | --- |
| `parserText`, `parserSpan` | the text matched so far, and where it is | [values](07-values.md#names-the-parser-supplies) |
| `parserInput` | the whole input (not in a `when`) | [values](07-values.md#names-the-parser-supplies) |
| `parserOrdinal`, `parserLine`, `parserColumn`, `parserPosition` | which element of a recovering repetition, and where it begins | [recovery](13-recovery.md) |
| `parserMessage`, `parserExpected`, `parserFailureLine`, `parserFailureColumn`, `parserFailurePosition` | why a rejected element was rejected, and where reading it stopped | [recovery](13-recovery.md) |
| `parserState`, `parserMarks` | the `with state` marks standing over a construction | [state](15-context-and-state.md) |
| `context` | the object a caller hands the parse, where the grammar declares `context : @T` | [context](15-context-and-state.md) |

Every supplied name but `context` begins with `parser`, and no capture or parameter may take a name that
does (`GRAM4012`).
