# The reader over buffered input: FIX step 5 (D13, design)

A short design for the architect, before code. It is step 5 of `fix-reader-2026-09-18.md`: the
stream and `yield` forms leave the engine, so that a grammar of the scenario (§8 there) emits no
engine at all. performance-ff builds steps 2-3 (the reader with `recover`, construction at the
turn's commit) over the string form; this step takes the same reader over `BufferedText` and
`BufferedBytes`. The two are agreed to meet at one seam (section 2).

## 1. What is there today

- **Buffered input** is `BufferedText` (a `TextReader`) and `BufferedBytes` (a `Stream`, or
  `byte[]`/`ReadOnlyMemory<byte>` borrowed in place): classes with absolute positions,
  `Ensure(p, n)`, `Get(p)`, `Peek`, `Slice(p, n)` returning a span of the buffer, and
  `ReleaseBefore(p)`, compacted on the next fill, bounded by `maxRetained`. `Ensure` blocks
  until the input has the characters or has ended, so `false` means the true end. There is no
  "not read yet" state, which makes the reader's question the same one it asks of a span.
- **Only the engine reads it.** `BufferedEmitter` builds engine machines and never asks
  `CanDirect`. The engine reads through `ReadAt`/`Short`/`Room` (`text.Get(p)` against `text[p]`,
  `!text.Ensure(p, n)` against a length test), and literals compare `text.Slice(p, n)` either
  way.
- **The reader does not.** It is a `ref struct` holding `ReadOnlySpan<char> text`, and its
  emitted access is written for a span at about 25 places: the bounds test with `c = text[p]`
  (8 sites), one- and many-character literals, lookbehind, the end-of-input test, the external
  call and the text captures (`Cut`). It is always over `char`.
- **No search over a buffer.** The engine's scanners are off for buffered input, and the
  delimiter and recovery scans run character by character through `Ensure`/`Get`.
- **Retention.** A buffered `yield` releases before each element (the driver loop). A buffered
  parse returning the whole result releases only in the one-rule silent case
  (`CanReleaseBuffered`); otherwise it retains its whole input. That is the D5 defect
  `StreamingRetentionTests.A_whole_result_grows_with_the_result_and_not_with_the_input` is
  skipped for.

## 2. Parameterizing the reader by its source (Q7.5)

**Two bodies at emission, through the machine's helpers, no runtime generic.** The reader is
written once, against helpers the machine answers per input form:

| helper | over a span (today's text, byte for byte) | over a buffer |
| --- | --- | --- |
| `ReadAt(p)` | `text[p]` | `text.Get(p)` |
| `Short(n)` / `Room(n)` | `(uint)p >= (uint)text.Length`, `text.Length - p < n` | `!text.Ensure(p, n)` |
| `AtEnd(p)` (whole-input test) | `p == text.Length` | `!text.Peek(p, out _)` |
| `Search(from, stops)` | `MemoryExtensions.IndexOf`/`IndexOfAny` over `text.Slice(from)` | `text.IndexOf(from, stops)`: new, searches what is held and refills until found or ended |
| `Slice(from, to)` | `text.Slice(from, to - from)` | the same call (`BufferedText.Slice` returns a span) |
| `Cut(from, length)` | as today | as today, over `Slice` |

performance-ff routes the loop and the turn he writes through them (agreed: the loop condition
is `Room(p, 1)`, not a length captured before it). This step routes the reader's existing
access points through them too, which for the span form must be a textual no-op — the emitted
code of every grammar that does not go buffered stays byte-identical, and the comparison says
so.

Over buffered input the reader struct holds the buffer (`BufferedText` or `BufferedBytes`, a
reference) where it holds the span now, and its methods keep their signatures otherwise.
`char` against `byte` is the specialization the buffered types already use (`_Bytes` names,
`c` as `int`), not a second design.

Why not a struct type parameter: the seam is needed only at emission, both forms are written in
one pass of the same emitter, and a generic reader would put a type argument on a `ref struct`
that holds either a span or a class — legal only as a non-`ref` type argument, costing a
wrapper per form for nothing the two bodies do not already give. Code size is the cost: one
reader per input form (fix-reader §4 estimates about 5 KB a variant against 47% of today's
588 KB for the buffered engines).

## 3. Retention and release

- **`yield`** stays as it is: the driver calls the turn once per element and releases before it.
  Over the reader the turn is performance-ff's `Read_X_Turn`, so the step is that method, not
  an engine run.
- **A whole-result parse** (`parse Fields stream bytes`) releases before each turn of the
  recovering repetition, at the loop's turn start — possible exactly when the turn's
  constructions have run at its commit (step 3), since nothing after the commit reads the text
  before it. That un-skips `A_whole_result_grows_with_the_result_and_not_with_the_input`: the
  retained input is one turn, the result grows with the result. It is the gate of this step.
- **Captures over a buffer** are spans valid until the next fill; everything a construction
  keeps is materialized at the commit point (performance-ff's step 3 does that), and a value a
  guard asks for mid-turn (`tag`) is built on the spot, before any further `Ensure`.
- **Where the reader cannot release** — the tape still building after the parse, a lookbehind,
  a location type, the constructions that read `parserInput` — it retains as the engine does
  today, and says nothing new: the same rules as the driver's `ReleaseBefore` now.
- **Deepening** (the reader's second thread for deep recursion) needs the whole input as
  memory; over a buffer it hands the buffer object to the new thread instead, which is safe
  because the thread is joined before the caller reads again. If that proves awkward, the
  buffered reader is refused where `Probes` is set and those grammars stay on the engine.

## 4. `recover` over a buffer

The same loop performance-ff writes: a turn that returns -1 is the broken element, the sync is
searched from the turn's start with `Search`, the failure value is built from `Slice(start, q)`
and the furthest refusal inside the turn, and the loop goes on after the sync. Over a buffer the
only difference is that `Search` may refill, and that the failure value is built before the
release at the next turn — both already true of the shape. §8.2's "try the whole continuation
first at each boundary" is performance-ff's (condition 1 of fix-reader §6), and holds over
either source because it is written against the helpers.

## 5. Order and gates

1. **Route the reader's existing access through the helpers.** Span form only; every grammar's
   emitted code byte-identical. Can start before performance-ff's step 2 lands, touching only the
   access sites in `Machine.Reader.cs`/`Machine.Direct*.cs`, which he is told of first.
2. **`BufferedText.IndexOf`/`BufferedBytes.IndexOf`** and the buffered branches of the helpers;
   `BufferedEmitter` builds a reader where `CanDirect` holds. Gates: the buffered forms answer
   as the string form on every input the tests compare (Finance.Tests, the Fix44 oracle,
   RefusalTests' positions), `Compatibility` at C# 8, the emitted size measured.
3. **Release per turn** for a whole-result parse, after performance-ff's step 3. Gate: the
   skipped D5 test un-skipped and green; stand's stream rows before and after.

Nothing here is a language change; `Window`/`TextReader` streaming without `stream` (the
engine's line-window path) is not touched.
