# Sequence, choice, groups, quantifiers and lookahead

How recognizers combine: one after another, one or the other, repeated, or tested without being
read. The specification is [`syntax.md` §3.2–§3.4 and §3.8](../../syntax.md#32-composition).

This page is about the operators of `.Gram` itself — `&`, `|`, quantifiers — and the order in which
they bind inside one expression. The precedence of the operators of the language a grammar *reads*
(`*` before `+` in arithmetic) is a property of rules, said with binding powers
(`<< n`, `>> n`, [below](#not-to-be-confused-with-binding-powers) and in
[rules](04-rules.md#binding-powers)).

## Sequence: `&`

`a & b` matches `a`, then `b` where `a` ended. Every pair of operands needs a connector: `.Gram` has
no juxtaposition, and two operands side by side are an error.

```dotgram
Word   = ['a'..'z']+
Assign = Word & '=' & Word

parse Assign
```

```text examples
ParseAssign("x=y") → "x=y"
ParseAssign("x = y") → refused
```

```dotgram GRAM2002
Assign = 'x' '=' 'y'
```

Notes:

- Nothing may stand between the operands unless the grammar says so: `"x = y"` is refused. The rule
  `trivia` is what is woven between them, and it is empty by default
  ([trivia](05-trivia-and-words.md)).
- `a ~ b` is a sequence that never lets trivia between its operands; it matters only once
  `trivia` is not empty ([`~`](05-trivia-and-words.md#adjacency-)).
- The missing connector above is reported where the next operand begins, as the start of a
  declaration that is not one (`GRAM2002`).

## Ordered choice: `|`

`a | b` tries `a` first and `b` only where `a` does not lead to a match. `&` binds tighter than `|`,
so `x & y | z` is `(x & y) | z`.

```dotgram
Scheme = "http" | "https"
Sign   = '+' & ['0'..'9'] | '-' & ['0'..'9']

parse Scheme
find  Scheme as Schemes
parse Sign
```

```text examples
ParseScheme("https") → "https"
Schemes("https") → ["http"]
ParseSign("-7") → "-7"
```

Over characters a choice **backtracks**: when something after it fails, the next alternative is
tried. `ParseScheme("https")` first takes `"http"`, finds `s` left over where `parse` wants the end,
and tries `"https"`. A `find` asks only for an occurrence, and `"http"` is one, so it is the answer.
Write the longer alternative first where it is the one wanted: `"https" | "http"`.

A rule call is transparent to this: a choice inside a called rule is resumed exactly as one written
inline.

```dotgram
Start = Name & 'y'
Name  = "xy" | "x"

parse Start
```

```text examples
ParseStart("xy") → "xy"
```

`Name` first answers `"xy"`, `'y'` then finds nothing, and the parse goes back into `Name` for
`"x"`.

Notes:

- Alternatives are never reordered, whatever their lengths: order is meaning, as in .NET regular
  expressions ([`syntax.md` §11](../../syntax.md#11-deliberately-out-of-scope)).
- In a grammar split into a lexer and a parser (`Lexical = true`), the syntactic half reads
  tokens, and there the first alternative that matches **stands**: nothing after it sends the
  parse back into it. A rule marked `Name?` gives back
  ([reading over tokens](06-lexical.md#choices-stand-and-rules-that-give-back)).
- `|` inside square brackets is set union, a different construct ([elements](02-elements.md)).

## Groups: `( … )`

Parentheses group an expression into one operand, for a quantifier, a capture or a choice inside a
sequence.

```dotgram
Key   = ('a' | 'b') & 'c'
Pairs = ('a' & 'b')+

parse Key
parse Pairs
```

```text examples
ParseKey("bc") → "bc"
ParsePairs("abab") → "abab"
ParsePairs("aba") → refused
```

Notes:

- A group never changes what is matched; it only says what belongs together.
- With a `=>` inside, a group builds a value of its own ([values](07-values.md#constructing-groups)).
- An expression nested more than 128 levels deep (groups, braces, lookaheads, captures, a call's
  arguments, …) is `GRAM2011`: name the inner part as a rule.

## Atomic groups: `{ … }`

Braces around an expression make it **atomic**: once it has matched, the choices made inside it are
final, and nothing that fails later sends the parse back into it.

```dotgram
Free   = ("xy" | "x") & 'y'
Atomic = { "xy" | "x" } & 'y'

parse Free
parse Atomic
```

```text examples
ParseFree("xy") → "xy"
ParseAtomic("xy") → refused
ParseAtomic("xyy") → "xyy"
```

`Atomic` commits to `"xy"` and cannot take it back for `'x'` alone. Braces are how a grammar says
"this reads to its end and never hands a character back", which is what a lexeme means:

```dotgram
Number = { ['0'..'9']+ }
Greedy = Number & ['0'..'9']

parse Greedy
```

```text examples
ParseGreedy("123") → refused
```

Notes:

- Braces have the same matches and value as the expression inside them; only what a later failure
  may undo changes. A failure *inside* the braces is an ordinary failure.
- Use them where the grammar has a real commit point: lexemes, trivia, a run that should not be
  cut two ways (below). Do not use them where a rule is meant to give back, as a dotted name that
  is a type only as far as it resolves.
- Rule boundaries never commit by themselves over characters: extracting part of an expression into
  a rule changes nothing.
- Braces after an operand with a number inside are a count, `X{3}`; braces where an operand begins
  are an atomic group.

## Quantifiers: `?`, `*`, `+`, `{n}`, `{n,m}`, `{n,}`

Every quantifier is postfix, as in regular expressions:

| Written | Matches |
| --- | --- |
| `X?` | zero or one `X` |
| `X*` | zero or more |
| `X+` | one or more |
| `X{3}` | exactly three |
| `X{2,4}` | two to four |
| `X{2,}` | two or more |

```dotgram
Sign    = '-'?
Digits  = ['0'..'9']+
Year    = ['0'..'9']{4}
Code    = ['A'..'Z']{2,3}
Spaces  = ' '{2,}
Number  = Sign & Digits

parse Number
parse Year
parse Code
parse Spaces
```

```text examples
ParseNumber("-42") → "-42"
ParseNumber("42") → "42"
ParseYear("2026") → "2026"
ParseYear("226") → refused
ParseCode("USD") → "USD"
ParseCode("USDT") → refused
ParseSpaces(" ") → refused
```

Quantifiers are greedy and, over characters, give back: `'a'* & 'a'` reads `aaa` by letting the
repetition return one `a`.

```dotgram
GiveBack = 'a'* & 'a'

parse GiveBack
```

```text examples
ParseGiveBack("aaa") → "aaa"
```

Notes:

- A count may also be a value parameter of the rule, `X{n}` ([parameters](04-rules.md#parameters)),
  or the length of a bound text, `X{#q}` ([bindings](12-bindings.md)).
- There are no lazy (`*?`) or possessive (`*+`) quantifiers, and one operand takes one quantifier:
  write `{ X* }` for possessive, and a group for a quantifier of a quantified operand, `(X+)*`.
- The body of `*`, `+` and `{n,m}` must consume input on every turn, which is checked when the
  grammar is built.

Refused:

```dotgram GRAM4001
Loop = ('a'?)*
```

```dotgram GRAM2005
Never = 'a'{3,2}
```

```dotgram GRAM2005
NoLower = 'a'{,3}
```

```dotgram GRAM2017
Twice = 'a'+*
```

`('a'?)*` could repeat forever without reading (`GRAM4001`). `{3,2}` can never match and `{,3}` has
no lower bound (`GRAM2005`: write `{0,3}`). A second quantifier on one operand is `GRAM2017`, which
says what to write instead: a group for a repetition of a repetition, braces for one that keeps what it
took.

## Lookahead: `?=X` and `?!X`

A lookahead tests what follows without reading it.

| Written | Succeeds where | Value |
| --- | --- | --- |
| `?=X` | `X` matches here | what `X` would give |
| `?!X` | `X` does not match here | none (`""` if captured) |

```dotgram
Keyword    = "if" | "else"
Identifier = ?!Keyword & ['a'..'z']+
Before     = ['a'..'z']+ & ?=';'

parse Identifier
find  Before as Statements
```

```text examples
ParseIdentifier("count") → "count"
ParseIdentifier("if") → refused
ParseIdentifier("iffy") → refused
Statements("a; b, c;") → ["a", "c"]
```

`?!Keyword` refuses `iffy` as well: `"if"` matches at its start. Saying "a keyword is a whole word"
is the job of `wordboundary` ([keyword boundaries](05-trivia-and-words.md#wordboundary)), or of a
lookahead after the keyword: `"if" & ?![\p{L}]`.

A positive lookahead may be captured, and then holds what the operand would have given, without
having moved:

```dotgram
Small : int = n: ?=Number & when @(n < 100) & v: Number => v
Number : int = ['0'..'9']+ => @int.Parse(parserText)

parse Small
```

```text examples
ParseSmall("42") → 42
ParseSmall("420") → refused
```

Notes:

- The spelling is .NET's (`(?=…)`, `(?!…)`), without the parentheses: `?=X` takes one operand, so a
  sequence needs a group, `?!('*' & '/')`.
- A quantifier binds tighter than a lookahead: `?!X*` is `(?!X)*`, which reads nothing and is
  refused (`GRAM4001`); write `?!(X*)`.
- A lookahead reads its own way and gives everything back: a capture inside it, or a guard there that
  reads a capture made before it, is refused (`GRAM4006`). So are a binding `$q:` and a `recover`
  inside one ([bindings](12-bindings.md), [recovery](13-recovery.md)).
- A guard reads nothing, so `?=(when @(g))` is `when @(g)`.

Refused:

```dotgram GRAM4001
NotA = ?!'a'*
```

```dotgram GRAM4006
Peek : @string = ?=(c: 'a') & any => @(c)
```

## Operator order in an expression

How the operators of `.Gram` bind inside one grammar expression, highest to lowest:

| Level | Forms |
| --- | --- |
| 1 | capture `name: X`, binding `$q: X`, lookahead `?=X`, `?!X` — prefixes, nesting either way round |
| 2 | quantifiers `X?`, `X*`, `X+`, `X{n,m}`; suffixes `recover S`, `with (…)`, `with state @(…)` |
| 3 | adjacency `a ~ b` |
| 4 | sequence `a & b` |
| 5 | construction `=> value` |
| 6 | ordered choice `a | b` |

What it means in practice:

```dotgram
Digit  : int = ['0'..'9'] => @(parserText[0] - '0')
Each   : @int[] = ds: Digit* => @(ds)
Letter = ['a'..'z']
Joined : @string = ls: Letter* => @(ls)

parse Each
parse Joined
```

```text examples
ParseEach("123") → [1, 2, 3]
ParseJoined("abc") → "abc"
```

- A quantifier applies to everything before it in its operand: `name: X*` is `(name: X)*`, a
  capture made on every turn. Turns of a rule that builds a value are collected into an array; turns
  of text are joined into one text ([values](07-values.md)). Likewise `?!X*` is `(?!X)*`.
- `=>` binds to one alternative, so every branch of a `|` builds its own value
  ([values](07-values.md)).
- `recover` binds tighter than `|`: `X* recover '|' | eol` is `(X* recover '|') | eol`; a choice of
  synchronizations needs brackets, `recover ('|' | eol)` ([recovery](13-recovery.md)).
- Inside a `when`, `is` binds tighter than `and`, and `and` than `or`
  ([guards](11-guards-and-switch.md)).

### Not to be confused with binding powers

The table above orders the operators of the grammar. The precedence of the operators of the
language being read — `*` before `+`, `^` to the right — is said on the alternatives of a rule, with
`<< n` (left-associative at level `n`) and `>> n` (right-associative). Higher binds tighter. The
whole story is in [rules](04-rules.md#binding-powers); one calculator shows the shape:

```dotgram
Expr : int = left: Expr & '+' & right: Expr   << 1  => @(left + right)
           | left: Expr & '-' & right: Expr   << 1  => @(left - right)
           | left: Expr & '*' & right: Expr   << 2  => @(left * right)
           | left: Expr & '^' & right: Expr   >> 3  => @((int)System.Math.Pow(left, right))
           | digits: ['0'..'9']+                    => @int.Parse(digits)

parse Expr
```

```text examples
ParseExpr("1+2*3") → 7
ParseExpr("8-2-1") → 5
ParseExpr("2^3^2") → 512
ParseExpr("2*3^2") → 18
```

`8-2-1` is `(8-2)-1` because `-` is left-associative, and `2^3^2` is `2^(3^2)` because `^` is
right-associative. A grammar can also say all of this with one rule per level and no markers; which
to choose is in [rules](04-rules.md#levels-as-rules).

## A run that can be cut two ways

Over characters a refused input is refused only after every reading of it has been tried. A
repetition that can read the same text in two ways makes that exponential:

```dotgram GRAM4052
Run = ('a' | 'a' & 'a')+ & 'b'

parse Run
```

```text examples
ParseRun("aab") → "aab"
```

`aa` is one turn or two, so a long run of `a` without the `b` is tried two to the n ways before it
is refused. The compiler warns (`GRAM4052`, at the rule, with the shortest text read two ways).
There are two cures:

```dotgram
OneReading = 'a'+ & 'b'
Committed  = { ('a' | 'a' & 'a')+ } & 'b'

parse OneReading
parse Committed
```

```text examples
ParseOneReading("aab") → "aab"
ParseCommitted("aab") → "aab"
```

Write the repetition so every text has one reading — this keeps the language — or brace it, which
commits the first reading and so may change the language where a later operand needed a turn given
back. The warning is exact for what it sees and blind past a call to a recursive or braced rule; a
lookahead or a guard that removes one reading is not seen, so it may warn where there is no second
reading ([`syntax.md` §3.3](../../syntax.md#33-quantifiers--postfix-as-in-regular-expressions)).
It is the one warning that can be raised to an error alone:
`<WarningsAsErrors>GRAM4052</WarningsAsErrors>`.
