---
name: dotgram
description: Write, publish and debug .Gram grammars — the notation, its seam with C#, and what the generator's diagnostics mean. Use when a project references the DotGram package, when a file carries a [Gram] attribute or has a .gram extension, or when asked to parse a format, protocol, query language or small DSL in C#.
---

# .Gram

A source generator that compiles a grammar into a C# parser during the build. The parser
is ordinary C# in the assembly being compiled: there is no engine reading a grammar at run
time, no runtime package, and nothing to deploy beside the application.

Everything below is checkable. The full specification is [`docs/syntax.md`][syntax], every
diagnostic is [`docs/diagnostics.md`][diagnostics], whole parsers to copy are under
[`examples/`][examples], and the largest grammars written in this notation — RFC 3986, an
expression language, SQL-92 and T-SQL as a dialect over it — are
[`DotGram.Parsers`][parsers].

[syntax]:      https://github.com/dotgram/dotgram/blob/main/docs/syntax.md
[diagnostics]: https://github.com/dotgram/dotgram/blob/main/docs/diagnostics.md
[examples]:    https://github.com/dotgram/dotgram/tree/main/examples/DotGram.Examples
[parsers]:     https://github.com/dotgram/dotgram/tree/main/src/DotGram.Parsers

## Where a grammar lives

Two places, and the class it is attached to must be `partial` (`GRAM0002`):

```csharp
[Gram("""
	Hex   = ['0'..'9' | 'a'..'f' | 'A'..'F']
	Color = '#' & value: Hex{6}

	parse Color
	""")]
public static partial class CssColor;
```

or in a file of its own, named after the class or given to the attribute, and listed so
the generator can see it (`GRAM0003` if it is not):

```xml
<AdditionalFiles Include="Filter.gram" />
```

An inline grammar is a C# raw string literal, so it needs C# 11. A `.gram` file needs
nothing: the generated code is C# 8.

## What comes out

A rule on its own generates no API. A directive does:

| Directive | What it says | Generated |
| --- | --- | --- |
| `parse R` | the whole input is an `R` | `ParseR` — throws `FormatException`; `TryParseR` — a `Match<R>` |
| `find R` | there are `R`s inside other text | `FindR` — a lazy sequence of `Match<R>` |

`as` names the method instead: `find Row as AllRows`.

```csharp
CssColor.ParseColor("#12aBcF").Value       // 12aBcF
CssColor.TryParseColor("#xyz").IsSuccess   // false
```

A `Match<T>` carries `IsSuccess`, `Value`, `Error` and `Position`.

## What a rule can say

```dotgram
Row = "R" & '|' & symbol: Text & '|' & quantity: Digit+ & eol
```

**`name:` is a capture, and it becomes a property of the type the rule generates.** A
rule with captures and no `=>` builds a type of its own, named after the rule.

Everything an expression can be:

```dotgram
'x'   'x'i              a character, and one whose case does not matter
"text"   "text"i        a string, likewise — the `i` attaches to the literal alone
['a'..'z' | '_']        an element set: one item, and `|` inside it is union
[\p{Lu} | \p{Nd}]       Unicode categories, .NET's spelling, over characters
[^ '|' | '\n']          the complement: one item that is none of these
[@IsDigit]              a C# predicate, `bool M(char c)`, as a set member

a & b                   sequence — a connector is always required, there is no juxtaposition
a | b                   ordered choice: the first alternative that succeeds
( a & b )               grouping
{ a & b }               atomic: once it succeeds, choices made inside it are not reopened

X?  X*  X+              zero or one, zero or more, one or more
X{3}  X{2,4}  X{2,}     exactly, between, at least — the body must consume something

?=X                     positive lookahead: X matches here, nothing is consumed, X's
                        value is produced and may be captured
?!X                     negative lookahead: X does not match here, and nothing is produced

@M                      an external recognizer, `bool M(ReadOnlySpan<char> input, ref int pos)`
name: X                 a capture
```

Seven rules exist without being declared, and any of them may be shadowed by declaring
one of that name:

```dotgram
any             one input item, whatever it is; fails only where there is none left
none            zero items — always succeeds, consumes nothing
eol             one line ending: "\r\n" | "\n" | "\r", CRLF first
eof             the end of input
trivia          empty by default (below)
wordboundary    empty by default: what continues a word, for keyword boundaries
word            one whole word, whatever `wordboundary` says a word is
```

To produce something else, say what and build it:

```dotgram
Row : @string = "R" & '|' & t: Text & eol => @(t)
```

`: @T` declares the rule's result as a C# type; `=> @(…)` is C# that builds it, with the
captures in scope by name. `: OtherRule` means "whatever that rule produces", which is
what makes substitution work. A `=>` without a declared type is `GRAM4008`.

Captures can also be matched straight to a constructor or to `required` properties, in
which case the grammar holds no construction code at all.

A guard asks C# a question in the middle of a parse — the thing a grammar cannot express:

```dotgram
Tag = '<' & open: Name & '>' & "</" & close: Name & '>' & when @(open == close)
```

**Syntactic position decides the call shape**, and the generator never inspects a
signature to guess a role:

| C# | Role | Written as |
| --- | --- | --- |
| `bool M(char c)` | element predicate | `[@M]`, inside an element set |
| `bool M(ReadOnlySpan<char> input, ref int pos)` | external recognizer | bare `@M`, as an operand |
| `bool M(ReadOnlySpan<char> input, ref int pos, out T value)` | recognizer with a value | bare `@M` |
| any C# value | construction | `=> @M(a, b)`, `=> @(expr)` |
| any C# `bool` | guard | `when @M(a)`, `when @(expr)` |

Everything under an `@` is the consumer's own C# and is carried across as text: a name
inside it needs no `@` of its own. Outside an `@`, a bare name is the grammar's — a rule,
a capture, a parameter.

Beside the captures, a `=>` or a `when` can name what the parse worked out: `parserText`
(the matched text), `parserSpan`, `parserInput`, and in a recovery `parserOrdinal`,
`parserLine`, `parserColumn`, `parserPosition` and `parserMessage`.

## Rules that take arguments

A parameter is written like a capture, and what may be passed follows from whether it
names a rule or a C# type:

```dotgram
Lex(item)               : item   = trivia & item
List(item, sep)         : item[] = item & (sep & item)*
Padded(item, pad: char) : item   = pad* & value: item & pad* => value
Digits(n: int)          : @int   = ['0'..'9']{n} => @(int.Parse(parserText))
```

`item` is a recognizer and its result type comes from the call site; `item: Row` is a
recognizer obliged to produce `Row`; `n: int` and `t: @Tag` are values. There are no type
parameters: `: item` means "of whatever type `item` produces", and `: item[]` a sequence
of those.

**A rule whose body is a call does not inherit what the call produces.** With no declared
type, no `=>` and no captures, a rule is its matched extent — a `string` — whatever it
called (§4.1 case 4). Say the type to get the value:

```dotgram
Bare    = List(Number, ',')                     // string: the text it matched
Written : Number[] = Number & (',' & Number)*   // int[], if Number is `: @int`
```

## Namespaces

The top of a file is an implicit global namespace. A block declares another, an inner one
sees the outer, and `using` brings a namespace's names in unqualified:

```dotgram
namespace Lexical
{
	Token = ...
}

namespace Syntax
{
	using Lexical;

	Unit = Token*               // instead of Lexical.Token
}
```

Declaring a rule whose name already resolves outside is refused rather than taken as
shadowing (`GRAM3012`) — replacing a rule is what a rebinding is for:
`namespace N with (A = B) { ... }` rebinds `A` to `B` through everything the block
declares reaches.

## Whitespace

`trivia` is an ordinary rule the grammar shadows, and it is woven between operands:

```dotgram
trivia = [' ' | '\t']*
```

**It must accept empty input** (`GRAM4003`) — it stands between every pair of operands, so
a required match would demand whitespace everywhere. `Std.Spacing?`, not `Std.Spacing`.

## Precedence, two ways

One rule per level, where precedence is which rule calls which:

```dotgram
Sum     : @int = left: Sum     & op: ['+' | '-'] & right: Product => @(Join(op, left, right))
                | value: Product                                  => @(value)
Product : @int = left: Product & op: ['*' | '/'] & right: Value   => @(Join(op, left, right))
                | value: Value                                    => @(value)
```

Or one rule whose alternatives say their own strength (§4.3.1):

```dotgram
Expr : @int = left: Expr & '+' & right: Expr  << 1 => @(left + right)
            | left: Expr & '^' & right: Expr  >> 3 => @(Raise(left, right))
            | '-' & operand: Expr             >> 3 => @(-operand)
            | value: Value                         => @(value)
```

`<< n` reads the operand on the right one strength tighter, so the operator groups to the
left; `>> n` reads it at `n`, so it groups to the right. Higher binds tighter. A rule uses
one convention or the other, never both (`GRAM4009`).

## The standard library

`Std` travels inside the generator; name it in full or open it with `using Std;`.

| | |
| --- | --- |
| one character | `Digit` `HexDigit` `Letter` `Space` `Whitespace` |
| runs of them | `Digits` `HexDigits` `Blank` `Spacing` `Identifier` |
| numbers, as values | `Integer` `Long` `Decimal` `Double` |
| comments | `LineComment(start)` `BlockComment(open, close)` |
| quoted text | `Quoted(quote)` — doubled, as SQL and CSV do; `Escaped(quote, escape)` |

The numbers are unsigned: whether `-` belongs to a number or is an operator in front of
one is a question about the calling grammar. Whatever is not called is not compiled.

## One grammar, several parsers

`with` substitutes a rule through everything a publication reaches, and the result type
follows the substitution:

```dotgram
parse Expr with (Value = IntNumber)     as EvaluateInt
parse Expr with (Value = DecimalNumber) as EvaluateDecimal
```

The `=>` bodies do not change: `left + right` is C#, so what it means is whichever type
arrived. Give the result type its own operators and the same arithmetic builds a tree.

## Grammars as libraries

A grammar can be written on top of another, across a project reference. What crosses is
the grammar, not a parser: the including assembly generates its own.

```csharp
[Gram("""
	Word   = ['a'..'z']+
	Digits = ['0'..'9']+
	""")]
public static partial class Lexemes;

[GramInclude(typeof(Lexemes), As = "Lex")]
[Gram("""
	trivia = ' '*

	Setting : @string = key: Lex.Word & '=' & value: Lex.Digits => @(key + " is " + value)

	parse Setting
	""")]
public static partial class Settings;
```

Each include lands in a namespace of its own, so `Lex.Word` and a `Word` of your own
cannot collide. Two includes under one name is `GRAM0008`.

## Somewhere to put what the reading works out

A `=>` runs after the match and a `when` that writes to a static field has made it global.
A grammar that needs a table of names, a scope, a label — anything the reading works out
and the API has nowhere to keep — declares one:

```dotgram
context : @Names
state   : @int
```

The type is a C# name this notation never resolves. The caller makes one and hands it
over, and every publication takes it: `Grammar.ParseLambda(text, names)`. `context` is
then a name a `=>` or a `when` may use — and a hook that does not name it is not passed
it, so declaring one and never using it costs nothing and adds no argument.

`state` is the other half: a value a parse carries and may set for part of itself,
`X with state @(1)`, which is how a grammar reads something differently inside a region
it entered.

## What the attribute can be told

```csharp
[Gram("…",
	Lexical      = true,                 // compile over tokens, not characters
	LocationType = typeof(ISqlSpan),     // offer every rule the range it matched
	Stacks       = 4,                    // how many stacks a deep reading may take; 0 is no limit
	PartSize     = 60_000,               // how large a generated method may grow
	Portable     = false)]               // do not carry the grammar text in the assembly
```

`[GramOptions]` may be written as many times as there are further readings wanted, each
naming the nested class it goes into:

```csharp
[Gram("Sql.gram", Lexical = true)]
[GramOptions(Carrier = GramCarrier.Immediate, Suffix = "Immediate")]
public static partial class Sql;
```

`Sql.ParseQuery` is the first and `Sql.Immediate.ParseQuery` the second — the same
grammar, compiled the other way, side by side.

## Input that does not fit in memory

Where the generator can prove input may be released as the parse goes, it emits overloads
taking a `TextReader` and an `IEnumerable<string>` beside the ordinary ones. The window is
bounded and reused, so memory does not grow with the file. Where it cannot prove it,
`GRAM5001` says which reason applied.

A repetition can be told where to pick itself up after a bad record:

```dotgram
Feed : @string[] = Row* recover eol => @(parserText)
```

The `=>` says what to make of what was rejected — here the text of the bad line, which
arrives in the sequence beside the good ones. It can as well build a record of its own
carrying `parserLine` and `parserMessage`, or go to a `partial void` hook and stay out of
the result.

`Lexical = true` on the attribute compiles the grammar over tokens instead of characters:
a lexical half makes them, and the half above decides each choice by the token in front of
it and does not revisit it. It needs `trivia` in braces and a lexical namespace whose own
`trivia` is `none`; where the grammar cannot be cut in two, `GRAM5004` says so and the
parser is the one it would have been. **A token parse reads from memory only** — there are
no reader overloads over kinds.

## Diagnostics worth knowing before you meet them

| | |
| --- | --- |
| `GRAM0002` | the host class is not `partial` |
| `GRAM0003` | no grammar file: add it to `<AdditionalFiles>` |
| `GRAM3004` | a `@Type` the grammar names is not in view — check the `@using` inside the grammar, which is separate from the file's |
| `GRAM4003` | `trivia` must accept empty input |
| `GRAM4008` | a `=>` on a rule that does not say what type it builds |
| `GRAM4018` | a rule nothing reaches, and so nothing compiled — publish it or delete it |
| `GRAM5001` | a publication got no reader overload, and why |
| `GRAM5002` | a repetition can begin with the same input as what follows it |

The message names the section of `docs/syntax.md` that settles the question. Read it
rather than guessing at the notation.

## What is deliberately not there

- **No repair of a broken document.** Recovery is scoped to one repetition, for a feed
  (`recover`). Finding the edit an author most likely meant is a different engine.
- **Alternatives are never reordered.** `|` is ordered choice, including where one
  literal is a prefix of another. Over characters backtracking makes a later alternative
  reachable; over tokens the first that matches is the one.
- **No type parameters**, and no way to say a rule is generic: a parameter's name in type
  position is what covers it.
- **No juxtaposition.** Two expressions never sit side by side; `&` or `|` between them.

## Working rules

- **Write the grammar from the specification**, not from another parser's source. A
  transcription cannot disagree with what it was transcribed from, so it proves nothing.
- **Publish something.** A rule nothing reaches is not compiled at all (`GRAM4018`).
- **Read what was generated** when a parse surprises you:

  ```xml
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)GeneratedFiles</CompilerGeneratedFilesOutputPath>
  ```

- **Let C# do what C# is better at.** A guard, a constructor, an operator on the result
  type — the grammar says what the syntax is, and what a thing means is the C#'s.
- **On `netstandard2.0` and `net472`** add a `System.Memory` reference: the generated
  methods take `ReadOnlySpan<char>`, which those frameworks do not carry.
