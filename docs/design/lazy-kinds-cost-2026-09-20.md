# What a resumable lexer costs: the number Igor's fork is missing (2026-09-20)

**Decided on 2026-09-20: the kept cutting stays and there is no reading a host holds.** Igor
took the fork this was written for, and took neither half of what it priced: lazy kinds are not
built — the number below is why — and `Over(text)` is refused, so nothing new appears in a
generated parser's public API. The cutting lives as it is: two slots a thread, the text weakly and
the kinds strongly, a slot taken by the next text. The defect that sat in it is fixed and held by
a test, and nothing else on that line is to be touched without its own reason.

The one reason to reopen it: a living consumer needing a reading they hold themselves. Not a
square somebody notices in our own tests — every caller of a positional form in this tree is ours.

What the document is kept for is the number, which closed a direction rather than opened one.

For the architect, and through him for Igor's decision between changing what a positional form
refuses (and lexing on demand) and keeping the refusal (and paying for a public type). The cost of
the second half of that fork is mine to say, because it is a cost in the emitter. Names are cited
rather than line numbers, which move between sessions: the emitter's `Machine.InputType`,
`Machine.ReadAt`, `BufferedEmitter`, `CSharpEmitter.Answering`.

## 1. Why it is a second rendering and not a helper

A split grammar's kinds are a `char[]`, and the reader reads them as a `ReadOnlySpan<char>` — the
same reader as over characters, with the kinds standing where the characters would. That is why
there is one rendering today and why it costs nothing: `InputType` is `ReadOnlySpan<char>`, and
`ReadAt(p)` is `text[p]`.

Lexing on demand means the end of the input is a question and not a length: `!text.Ensure(p, 1)`
instead of `p >= text.Length`. That is a property of the *machine*, not of a publication —
`Machine.InputType` and `Machine.ReadAt` are chosen once per machine — so a grammar that offers
positional forms over lazily cut kinds is rendered **twice**: once over the span, for the whole
form and the window form, and once over `BufferedKinds`, for the positional forms.

There is no shortcut in between, and I looked for one:

- **Refreshing the span after lexing more** does not work: the reader hands `text` down by value
  through recursive methods, so a span replaced deep in a call does not reach the callers holding
  their own copy.
- **A kinds array pre-sized to the input's length, filled lazily**, keeps the span still but gives
  the reader no way to tell "not lexed yet" from "no match": every comparison is against a
  particular kind, an unfilled entry equals none of them, and the reading refuses where it should
  have lexed on. That is a wrong answer, not a slower one.

## 2. What it costs in emitted code, measured

The reader and the walk are what a second machine renders again; both take the input type. Measured
on the generated files of this tree (a `#line` carries the grammar's absolute path, so these are
comparable only here; figures include the BOM):

| Grammar | generated | reader | walk | a second rendering |
| --- | --- | --- | --- | --- |
| T-SQL | 14.17 MB | 4.66 MB (33%) | 1.63 MB (12%) | **+44%, about 6.3 MB** |
| SQL:2023 | 8.56 MB | 2.68 MB (31%) | 2.38 MB (28%) | **+59%, about 5.0 MB** |
| the expression language | 1.87 MB | 0.48 MB (26%) | 0.62 MB (33%) | **+59%, about 1.1 MB** |

The ratio is not an estimate: `DotGram.Examples.Feeds.StockCountReader` already holds both
renderings of one grammar, and its buffered reader is within a percent of the size of its span
reader.

**And the time that comes with the size.** The generator is the slowest thing this solution
builds — T-SQL is tens of seconds — and a second rendering of the reader and the walk is close to
a second pass of what dominates it. The consumer pays it too, in their own compile of a file half
as big again. That is the part I would not discover by reading, and the part that would be felt on
every build by everyone, not only by a host that reads from a position.

## 3. What it costs to build, honestly

The seam already exists, so this is not a new machinery — it is a third implementation of one that
is running:

- **`BufferedKinds` itself**: small. `BufferedText` and `BufferedBytes` already answer `Get`,
  `Ensure` and `Slice`, and the third holds the text, the kinds, the starts and the lengths, and
  lexes a block further when asked past its count.
- **Resuming the lexer**: already nearly there. `Tokenize_DotGram(input, from, to)` starts where it
  is told, and a token boundary carries no state — even a nested comment is read inside one token —
  so "lex on from where the last token ended" is the call that already exists, appending instead of
  starting fresh.
- **The machine**: a third value of `InputType` and of the helpers around it, which are already the
  fork between the span and the buffer.
- **The arrays that grow while being read**: the starts and the lengths are handed to the walk and
  to a capture as arrays today. Lexing more replaces them, so what is handed down has to be the
  source and not the arrays. This is the one piece that touches code which is not already forked,
  and it touches the materializer's signature, which every arm goes through.
- **What loses its meaning**: `parserInput` over the whole kinds (there is no whole), and a
  look-behind before the position the reading began at — which has nothing to look at today either.

My estimate of the work, which is a judgement and not a measurement: **two to four days**, of which
the `BufferedKinds` class is the smallest part and the growing arrays the largest, plus the tests
that hold the lazy and eager readings to the same answers on the corpora.

## 4. What it buys, against what the other half of the fork buys

Lazy kinds make the loop linear without keeping anything between calls: no slots, no eviction, no
weak reference, no cutting handed to the reading that pushed it out (`TokenizationCacheTests`, a
defect that was real), and no search for the token at `at`, because a cutting begun at `at` begins
with it. A reading of one short value out of a huge text stops costing the whole text, which
neither the cache nor a held reading fixes.

A held reading (`Over(text)`) buys the same linearity for a loop, in a few hundred lines and no
size at all, and costs a type in the public API of every generated parser, forever, and does not
help the single reading out of a huge text.

**Both are ours to weigh; neither is free.** What I would say if asked to choose: the size and the
build time are paid by everyone on every build, and the public type is paid by everyone who reads
the API. But the lazy reading is the only one of the two that makes a promise the specification
already implies — that a reading which need not reach the end need not read to the end.

## 5. Independent of the fork, and a defect either way

The refusal critic found is not part of the choice: the non-windowed positional form cuts the whole
input and then refuses on `tokens.Stopped >= 0`, which is where the scan met a character no token
begins with — **anywhere in the text**. A form that is not required to reach the end is refused by
what it does not read: one bad character in the last line of a script makes the first statement
unreadable. The window form already has the other meaning, and the comment there says it: inside a
window, a character no token begins with is where the tokens end.

That should be fixed whichever way the fork goes, and it is small: the positional form ends its
tokens at `Stopped` instead of refusing. It is mine and I will do it next unless told otherwise.
