# Design proposal: separate lexical and syntactic machines

A proposal, not a description of the current compiler and not part of the `.gram`
language specification. It was discussed, then measured — the numbers at the bottom are
from a probe that ran, and they are what makes the rest of this worth reading.

## The problem

The generated recognition machine does two conceptually separate jobs in one dispatcher:

1. it discovers what textual unit starts at the current character position;
2. it decides which grammatical path to take, and manages parsing control flow.

The second belongs to the parsing algorithm and stays. The first is what spreads
character-level classification through the central machine, because a choice beginning at
`text[p]` may have to inspect the first character, compare literals, test ranges,
distinguish keywords from identifiers, scan trivia, select syntax, and record
continuations and captures — all at once, at every syntactic position that can reach it.

The cost is measurable, not aesthetic:

- `SqlStandard92` — a 334-line grammar — generates **23,500 lines**, about 70:1.
- `ExecutionPlan.cs` records the mechanism: the reserved-word list is 285 nodes, reaches a
  dozen places through the identifier that names it, and was **59% of the generated file**
  (60,317 lines against 22,228 with it removed) before a size threshold was added.
- `Identifier = ?!Reserved & RegularIdentifier | DelimitedIdentifier` runs a 57-way choice
  behind a negative lookahead **at every identifier position**.
- Three optimizations aimed at exactly that returned 1.4x on refusals and 5% overall. The
  entries for them in `next.md` all end the same way: the finding is how small the win is.

It also costs correctness. See "What the probe found on the way" below.

## Target architecture

```text
UTF-16 characters
        |
        v
lexical machine: char -> integer terminal kind
        |
        v
logical sequence of integer kinds
        |
        v
syntactic machine: integer kind -> typed parse result
```

The lexical machine owns trivia, exact literals, identifiers, numbers and strings, longest
match, keyword classification, source extents and lexical failures. The syntactic machine
owns sequences, alternatives, repetition, lookahead, calls, continuations, backtracking,
captures, deferred construction, recovery and syntactic failures.

The parsing algorithm does not change. Its elementary input operation does:

```text
before: input[p] is a char
after:  input[p] is an integer terminal kind
```

| Character machine | Token-kind machine |
| --- | --- |
| `c == 'x'` | `kind == 7` |
| character-set membership | terminal-set membership |
| character first sets | integer-kind first sets |
| choice, repetition, lookahead | the same operations |
| state arena and continuations | the same machinery |

### The shortcut that makes this cheap to try

`CharRange` is `(char From, char To)`. The whole analysis stack — `FirstSets`,
`FollowSets`, `Determinism`, `Doors`, `RangesTest`, `Predictive`, `Skipped`,
`Dispatchable` — is **already a machine over a 16-bit alphabet**. Allocate kinds as `char`
values and every one of those runs over tokens with no change at all. The probe below did
exactly this: it compiles through the unmodified generator.

## Terminal numbering

Number terminal *results*: not arbitrary grammar rules, and not individual occurrences.

**Every leaf gets its own kind.** A terminal whose language is exactly one complete string
— a keyword, a bracket, an operator — is one kind. Syntax never compares their text again.
An alias of a singleton does not create another kind.

**A class of strings gets one kind.** `Identifier`, `Integer`, `Real`, `String`.
`customer` and `Id` both produce `Identifier`; their values differ by source extent, not
by kind.

**Groups are contiguous ranges.** `Keyword`, `SpecialSymbol`, `Constant` are not runtime
kinds; they are intervals over the leaves. This is what makes the sum type free:

```text
sum type       -> a contiguous range
variant tag    -> the number itself
group test     -> (uint)(kind - lo) < n
payload        -> (start, length), and only where there is one
```

A keyword needs no payload at all: the kind *is* the word.

**Why per-leaf and not one kind for all keywords.** One `Keyword` kind with the word as
payload would make the first set of every keyword-led rule identical, so
`Determinism.Distinguishable` would never fire and the arena would stay full — losing the
whole point. Contiguous ranges give both: an exact `switch (kind)` for dispatch, and one
unsigned range test for "is this a keyword".

**Non-reserved words are their own range.** SQL-92 makes this concrete. Of the 97 keywords
`SqlStandard92` uses, only 47 are in its `Reserved` list; the other 50 — `AVG`,
`SUBSTRING`, `YEAR`, `INTEGER` and the rest — may also stand as identifiers. So they are
numbered as a range of their own, and the syntactic rule becomes a union:

```dotgram
Identifier = C_Identifier | NonReserved
```

That is the general answer to contextual keywords, and it needs nothing new: a grammar
that wants "a name, or the word `var`" writes `Identifier | "var"`, and both sides are
numbers.

### Where the lowering goes

An extension of the ladder `RangesTest` already has, with one new rung:

```text
one kind             kind == k
contiguous range     (uint)(kind - lo) < n      one subtract, one compare
union of two or three   two or three of the above
arbitrary set, <= 64    (mask >> kind) & 1      a ulong in a static field, in a register
anything wider       a byte table
```

Because groups are contiguous, almost every interesting set is an interval, and intervals
beat masks — no memory touched, any alphabet size. Masks are for arbitrary unions. Note
that 128-bit masks buy nothing: `UInt128` lowers to pairs of 64-bit operations on x64, and
a membership test needs one bit, so two `ulong`s cover any realistic alphabet on the
netstandard2.0 floor.

## The lexer is inferred, not written

This is what keeps the migration from being a rewrite of every grammar.

- **Leaves are inferred.** Every exact complete literal, wherever it stands in the grammar,
  becomes a leaf with its own kind. Nobody declares `'('`.
- **Classes are declared, and already are.** A `trivia = none` namespace supplies what
  cannot be inferred — `Word`, `Number`, `Text`, `Verbatim`. `ExpressionLanguage` and
  `SqlStandard92` both have one today, written by hand for other reasons.
- **Named non-leaf rules stay as names.** `Keyword`, `SpecialSymbol`, `Constant` become
  range declarations and ADT variants, not runtime work.

The test of whether the inference is good enough: **if a grammar changes by more than the
deletion of a `?!`, the compiler is asking the author to do the compiler's job.**

### No new notation

`?!Keyword & Word` over kinds is `kind in Word and kind not in Keyword` — a finite-set
operation over small integer sets, which `FirstSets` and `Determinism` can fold into one
range test at compile time. Over characters this was inexpressible except as a lookahead,
which is why it costs what it costs today. So the first attempt should add no syntax: keep
the spelling, teach the normalizer to fold "a lookahead that consumes nothing, in front of
an elementary test" into a set operation, and see whether it fires. An explicit difference
operator is worth discussing only if it does not.

## Lexical modes: a ladder, and most grammars stay on the bottom rung

The lexer cannot always be memoryless, but the exceptions are fewer and different from
what one expects.

**Rung 0 — no state.** One `Token` root, longest match, declaration priority. Almost
everything, including `a+++++b` (maximal munch settles it deterministically; that the
answer displeases a human is a type error one level up) and every closed string form —
`"..."`, `'...'`, `@"..."` — where the delimiters bound the rule and no other grammar runs
inside.

**Rung 1 — the mode is derived: `mode = f(previous kind)`.** A table on the previous
token's kind. `/` as division against the start of a regular expression is the classic.
Entirely inside the lexer; the syntax never knows.

**Rung 2 — the mode is pushed and popped by syntax.** String interpolation is the case
that forces it: in `$"a {x + 1} b"` the closing `}` cannot be found by the lexer, because
the expression inside carries its own braces — `$"{ new[]{1,2}.Length }"` — and it nests,
so a flag will not do, it needs a stack. This rung puts a hole in the wall and must
therefore be **visible in the notation**: a hole that is declared is an interface, a hole
that is inferred is a leak.

`.gram` already has a rung-2 boundary and already solved it: inline `@(...)` stops the
grammar reader and hands over to `ICSharpScanner`, because only a C# reader knows where
the expression ends. The mechanism exists; what is missing is expressing it as a mode.

**And a fourth thing, which is not a mode at all — splitting a token.** `List<List<int>>`
needs two `>` where longest match produced one `>>`. The lexer read correctly; syntax needs
half of what it read. That is not state:

- the lexer emits `>>` as one kind with its `start` and `length`;
- syntax asking for `>` finds a token at `s` of length 2, and `Consume()` moves not to the
  next token but to character position `s + 1`, where the lexer re-reads.

Adjacency comes free from `kind + start + length` (`a.start + a.length == b.start`), and
the grammar already writes this idea by hand as `'>' & ?!'>'`.

**This is the real argument for the lazy cursor.** Splitting is free when the tokenizer can
re-read from an arbitrary character position, and awkward when there is a materialized
token array. The logical integer sequence is not quite a sequence of integers: a position
can stand in the middle of a token, and only rescanning makes that natural. Memory and
streaming are the lesser reasons.

**Where each of this repository's grammars sits.** `Rfc3986` stays scannerless — a URL's
alphabet is characters. `SqlStandard92` is rung 0. `ExpressionLanguage` is rung 0 *plus
splitting*, because of `Type = TypeName & ('<' & Type & (',' & Type)* & '>')?` beside
`Shift = left & ">>" & right`. No grammar here needs rung 1 or 2 yet; interpolated strings
would be the first.

## Two machines, two shapes of code

They should not be generated the same way.

**The lexer wants a table.** Small state space, table in L1, short loop: `state =
table[state][class]`. Indirection costs almost nothing there, and the generated code
becomes tiny — which independently relieves the 60,000-byte IL ceiling and the
`Budget`/`Part`/`PartSize` machinery that exists to work around it.

**The syntax wants direct code.** A large, data-dependent state space mispredicts indirect
branches; `switch (kind)` over a small dense alphabet is already lowered by RyuJIT to a
jump table, and it keeps direct `goto`s that the block layout can order by heat.

**Not a table of delegates.** A valueless rule call is 4.8 ns and a valued one 29 ns
(`benchmarks/CallCost.cs`). Fifteen tokens through a delegate table is 70+ ns of pure
dispatch on a parse that should cost hundreds. `goto`, not call.

**A bonus that falls out.** With kinds, error-recovery synchronization sets — "skip until
one of these" — are one mask or range test per token. Over characters that was never
cheap enough to be the ordinary answer.

## Source provenance stays

Syntax decides from kinds alone, but the text is still needed for values and diagnostics,
so every lexical result carries `kind + start + length`. That serves captures,
`parserText`, `parserSpan`, delayed conversion, diagnostic positions and the spans of
larger results. Text no longer takes part in syntactic dispatch; it is consulted for
materialization and reporting.

Diagnostics divide with the machines. The lexer reports textual failure — "unterminated
string literal at character 42" — and syntax reports terminal expectations — "expected
Identifier, `if` or `(`; found Integer". A composition rule has to keep a lexical error
from being masked by a generic syntax one.

## Internal ADTs are compile-time meaning

The compiler wants an algebraic model even though runtime code holds only integers:

```text
A & B  ->  product        A?  ->  optional
A | B  ->  sum            A*  ->  sequence
```

DotGram already produces product-like result classes from captures; it does not generally
retain which branch of an alternation matched. The lexical root needs that sum —
`Token = Keyword + Identifier + SpecialSymbol + Constant` — for stable variant identities,
named groups, and the mapping from grammar patterns to kind sets.

**The ADT explains; it does not execute.** A pattern like

```text
Operator(Identifier("b"), "+", Operator(Identifier("c"), "/", ...))
```

is matching over a *built tree*, and the whole performance story here rests on not building
one: construction is deferred to `Accept`, and eager construction was removed this year
precisely because it built values on readings the parse went on to abandon. Beauty and
speed do not trade off as long as the ADT stays a compile-time model — the sum is free
because its variants are disjoint integer ranges.

## Scannerless grammars stay valid

URL, protocol, regex-shaped and binary grammars work naturally on their own input alphabet
and should keep the current path at no lexical cost. The compiler-side abstraction:

```text
a recognition machine over an input alphabet

scannerless grammar:  the alphabet is char, or the original input item
lexed grammar:        the alphabet is an integer terminal kind
```

## What the probe measured

`.work/kinds` (scratch, not in git). `SqlStandard92`'s syntactic half — the ~40 rules from
`SearchCondition` through `Subquery` — transcribed **mechanically** onto a one-character-
per-kind alphabet, compiled by the **unmodified** generator, and fed by a hand-written
SQL-92 tokenizer. 130 kinds: 56 reserved words, 50 non-reserved, 17 symbols, 7 classes.

A gate ran first and blocked: 42 inputs — nine search conditions, their refusals derived by
cutting the last token, and adversarial ones — had to get the same verdict from both
parsers before anything was timed.

**Code, and what the arena holds:**

|  | characters | kinds | |
| --- | ---: | ---: | ---: |
| generated lines | 23,500 | 6,580 | 3.6x |
| `Choice` entries written | 143 | 19 | 7.5x |
| `Call` entries written | 299 | 64 | 4.7x |
| `Run` / `Lookahead` / `Atomic` | 20 | 0 | — |
| reads of `text[p]` | 692 | 320 | 2.2x |

**Time**, nanoseconds, min of seven windows, three runs in agreement. `lex` is the
tokenizer, `kinds` the parse alone, `total` both; `!` marks a refusal.

```text
     chars        lex      kinds      total
      464n        85n       209n       294n   1.58x  a = 1
      919n       132n       282n       414n   2.22x  salary BETWEEN 1000 AND 2000
    1,049n       333n       370n       703n   1.49x  x IN (1, 2, 3) AND y IS NOT NULL
    4,533n       727n     1,007n     1,734n   2.61x  (a + b) * c - d / e > f AND NOT g < h
    5,287n       843n       994n     1,837n   2.88x  (quantity + weight) * rate - zone / …
    3,513n       592n       696n     1,287n   2.73x  amount * 1.05 + tax >= total AND …
      777n        92n       137n       229n   3.39x  ! a =
    1,628n       125n       258n       383n   4.25x  ! salary BETWEEN 1000 AND
   11,518n       637n     1,836n     2,473n   4.66x  ! (a + b) * c - d / e > f AND NOT g <
   19,248n       767n     1,800n     2,567n   7.50x  ! (quantity + weight) * rate - zone / …
    5,814n       556n       743n     1,299n   4.48x  ! amount * 1.05 + tax >= total AND …
```

Accepted: **1.49x to 2.88x**, median 2.22x. Refused: **2.08x to 7.50x**, median 4.27x.
Refusals gain most, which is what the arena numbers predict — refusal is where a
backtracking engine walks every reading left alive, and there are five times fewer of them
to walk.

**Two biases, in opposite directions, both worth stating.** The tokenizer is hand-written,
so a generated one is unlikely to beat it: on that axis the figure is optimistic. It also
builds three buffers and a string per call because the generated parser takes a `string`,
where the design's answer is a virtual stream that allocates nothing: on that axis the
figure is conservative. An attempt to measure the second directly produced numbers that
contradicted themselves across two runs and was dropped rather than explained away.

**What it does not measure.** There is no generated lexer, so the claim "the lexer needs no
arena" is asserted by construction and not yet proved by the compiler. The syntactic half's
143-to-19 comparison is against the whole shipping parser, lexical rules included; the gap
is wide enough that the accounting does not change the conclusion, but it is not a
like-for-like count.

## What the probe found on the way

Transcribing the grammar turned up a defect in the shipping compiler, unrelated to any of
the above and larger than SQL.

**A repetition whose body begins with a word literal takes exactly one iteration once a
word boundary is woven beside that literal.** The smallest form, with no SQL in it:

```dotgram
wordboundary = ['a'..'z' | '0'..'9' | '_']
trivia = { ' '* }

Item  = "when" & ['a'..'z']
Start = "case" & Item+ & "end"
```

`case when a end` reads. `case when a when b end` does not, nor does three. The same shape
with neither trivia nor a word boundary reads any number of them.

So `SqlStandard92` today refuses `CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'small' ELSE
'none' END`, which is ordinary SQL, and refuses `CASE a WHEN 1 THEN 2 WHEN 3 THEN 4 END`
for the same reason. The transcription — which has no word literals, therefore no weaving —
accepts both.

It is worth noticing *where* the bug lives: in the seam machinery, which exists only
because lexing and parsing are the same machine.

**Fixed since.** `SpaceLists` spaced a repetition whose turn is a *valued* rule, and
`Repeated` spaced one whose body is a *sequence*; a call to a valueless rule with a seam
inside fell between them. It is spaced now, for the reason `Repeated` already gave — the
seam inside a turn and the seam between turns are the same question. A callee with no seam
of its own is untouched, which is what keeps a lexeme a lexeme.

## What the split turned out to need, once it was written

Four things the plan did not know, each found by running the rewrite against
`SqlStandard92` and its own oracle rather than by reasoning about it.

**A kind is a set of patterns, not a pattern.** This is the one that matters and it took
two attempts to see. `SELECT` is matched by the keyword *and* by `RegularIdentifier`; `0`
by `Digits` *and* by `UnsignedNumericLiteral`; `'x'` by `QuotedString` *and* by
`CharacterStringLiteral`. Written as one kind per pattern, a lexer has to choose, and every
syntactic position that wanted the other stops reading — which is how the first version
came to refuse three grammars that had nothing wrong with them. So a kind is the whole set
of patterns that matched, the test for a pattern is "the kind's set holds it" — a set of
kinds, computed at compile time and lowered to a range test — and nothing is refused.

    10          {Digits, UnsignedNumericLiteral}    one kind
    1.5         {UnsignedNumericLiteral}            another
    SELECT      {"SELECT", RegularIdentifier}       another
    zone        {"ZONE", RegularIdentifier}         another

`Length = '(' & Digits & ')'` takes the first; a value position takes the first and the
second; an identifier position takes the third and the fourth, and `?!Reserved` then takes
the third back out. Contextual keywords, overlapping literal classes and the reserved-word
lookahead are all one mechanism, seen from three sides.

**Nothing may rewrite to nothing where nothing means something else.** `?!wordboundary` has
an operand that is entirely the lexer's; rewriting it away leaves a negative lookahead over
what matches the empty string, which refuses everywhere. The first run cut half of SQL out
of the machine that way and looked like a triumph — 3,313 lines — until the gate refused
every input.

**A negated class names what it excludes.** `[^ '(' | ')']` over characters is "one item
that is not a bracket", and over kinds it is the same sentence about a wider alphabet —
which is what `Subquery` means by "anything balanced" and could not say before. Counting
its sixty-five thousand members was the first attempt, and it refused a grammar that had no
problem.

**`trivia = none` is not where tokens end.** `TypeName = Word & ('.' & Word)*` is not a
token; it is syntax living in a lexical namespace only so that `System . Text` will not
read with the spaces in it, and its captured span is why. The set model absorbs it — `A` is
`{Word, TypeName}` and `A.B` is `{TypeName}` — so nothing is refused, but the grammar is
still saying something it does not mean, and moving the rule out (at the cost of a factory
that joins the parts) is the honest fix.

## Suggested order from here

1. ~~Terminal inventory~~ — done: patterns, kinds as sets of them, and the rules that are
   sets of terminals and become ranges.
2. ~~The rewrite of the syntactic machine over kinds~~ — done, and validated against the
   shipping parser on forty-six inputs, all of which now agree.
3. ~~One automaton over all the patterns~~ — done. Thompson and a subset construction over
   an alphabet of atoms rather than characters, so a Unicode category costs one symbol per
   interval it already had. Its accepting sets *are* the kinds, exactly: `SqlStandard92`
   comes to 135 kinds where the witness approximation guessed 137, the two extra having been
   `{Digits}` and `{QuotedString}` — sets no string can produce, because each of those
   languages is contained in another pattern's. Only reading them together finds that; no
   witness can.
4. ~~Emitting the lexer from that automaton~~ — done, and it writes no arena. 528 states
   and 2,222 lines for `SqlStandard92`, smaller than the syntactic machine it feeds. Direct
   code and not a table, and that was measured rather than assumed: a dense table is 473,616
   cells over an alphabet of 897 atoms, merging neighbouring atoms leaves 186,342 tests
   because the atoms alternate, and grouping the ways out of a state by where they lead
   leaves **1,034**, forty-three at the widest state.

   It is also *faster* than the hand-written tokenizer it replaced, which the earlier
   measurements had called an optimistic bound — 412 nanoseconds against 710 on a long
   expression. The one thing that nearly sank it was writing the wide character sets inline:
   `new char[] { … }` inside the scanning loop is an allocation per character per test, and
   the first generated lexer came out seventeen times slower than the hand one. Hoisted into
   static fields it is faster than hand-written.
5. ~~One grammar end to end~~ — done. The publication takes a string, tokenizes it and
   parses the kinds; values are cut from the text the tokens came from and positions are
   characters. Every input the shipping SQL parser accepts or refuses, the split one agrees
   with, and it is faster on all eighteen of them.
6. The `Peek` / `Consume` / `Mark` / `Restore` cursor, lazy and rescanning. It carries the
   answer to `List<List<int>>` as well: `>` is a declared pattern and `>>` begins with it,
   so a cursor that can be asked for a particular kind splits without any state at all. What
   a token was read under has to travel with it in the cache — designed in from the start it
   is a field, discovered later it is a rewrite.
6. One grammar end to end behind an opt-in, the scannerless path untouched.
7. Modes, when a grammar here first needs one. Interpolation is the case that will force
   them, because the closing brace is known only to whoever parsed the expression inside.

## Central design statement

> The generated parser should no longer discover textual terminals while making syntactic
> decisions. A separate lexical machine maps characters to deterministic integer terminal
> kinds and source extents. The existing recognition algorithm then runs over the logical
> integer sequence, keeping its state arena, backtracking, captures, recovery and deferred
> materialization. Internal ADTs explain named variants and groups at compile time; runtime
> execution needs only numeric tags and provenance.
