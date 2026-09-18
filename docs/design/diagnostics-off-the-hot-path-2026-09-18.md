# Diagnostics off the hot path (Q7.2)

A proposal, not a statement about the compiler. It answers
`docs/design/architecture-decisions.md` Q7.2: where recording the furthest failure stands in the
way of a faster reading, the fast reading records nothing, and a refused input is read again with
recording on, which gives the same message. It also takes Q7.5 into account (two method bodies
against a generic flag), and the expression language's `context`, whose second reading has to
begin from a clean state.

Nothing here is implemented. The generator is performance-3f's area; who carries this out is the
architect's call.

## 1. What recording is today

A parse keeps one `Failure` (`Support.cs:634-678`): the furthest position, the set expected there
(`Expected`, a reference to a `static readonly string[]`, never a copy), the sets tied with it
(`ExpectedMore`, a list made on the first tie), and `OutOfInput`, which says whether a test ran
out of input rather than met the wrong character. `Match<T>.Error` builds the message lazily, so
the text itself costs nothing until someone asks for it.

Where it is written:

| rendering | how a refusal records | in the EL output |
| --- | --- | --- |
| reader (`Machine.Reader.cs`) | a call, `Refuse_DotGram(ref failure, p, Expected{n}, ways)`, at each refusing site; `Noted` sites record on paths that succeed (an optional whose door stays shut, a fold's tail set); an `on fail` call site records only while `failure.Position <= p` | 880 sites: 580 refuse-and-return (58 with no set), 294 `on fail`, 6 notes |
| engine (`Machine.cs:1542-1562`) | one shared `Fail:` label with the max/tie logic inline, and `lookahead < 0` gating it; every backtrack step passes through it | one machine (the lexical re-read of `Value`) |
| flat (`Machine.Flat.cs`) | inline: unconditional where there are no checkpoints, the engine's shape where there are | none |
| lexer of a split grammar | a position only (`tokens.Stopped`), no set | — |

Measured (`docs/next.md`, "Built: the list of what else was expected is kept"): making
`Refuse_DotGram` return at once took the EL's deepest parenthesis from 1.59x to 1.23x the hand
parser on the immediate carrier. Recording is **a fifth to a quarter of an EL parse**, and within
one per cent of nothing on SQL.

### Where recording is more than a diagnostic

The fast reading may drop recording only where nothing but the message reads it. Four places read
it for something else:

1. **Streaming** (`StreamingEmitter.cs:66-83, 297-310`): a parse over a `TextReader` window is run
   again after `window.Extend` when `failure.Position` reached the window's end or
   `failure.Starved` is set. The position decides whether more input is fetched.
2. **Recovery**: `Reach` and the recovery messages are made while the parse goes on.
3. **`on fail` sites** read `failure.Position` — but only to decide whether to record, so they
   drop out with it.
4. **Outcome**: `Starved` against `NoMatch` is decided from `OutOfInput` and `Position` — only once
   the parse has failed, so the second reading gives it.

And two places record for nobody:

- **`find` over a string** (`tests/Snapshots/Feed.gram.g.cs:111-127`) makes a fresh `Failure` at
  every start position and drops it. A `find` needs no second reading, only no recording.
- **Lexical `Value_*` and `Measure_*` re-reads** (`CSharpEmitter.cs:1910-1920, 3184-3191`) each make
  a `Failure` and discard it.

These two are free: recording off, nothing to read again.

## 2. The contract

For every publication this applies to:

- **A success is the same success.** The fast reading's control flow may not depend on anything
  recording writes. Today it does not, outside the four places above (the ways-back replay, the
  doors and the `when` guards do not read `failure`).
- **A refusal is the same refusal.** The second reading records exactly as today's single reading
  does, so `Position`, `Outcome`, `Expected`, `ExpectedMore` and the message are unchanged. That
  holds because it *is* today's reading, run a second time over the same input and the same state.
- **Nothing observable happens twice.** Constructions and guards that run during reading run
  again in the second reading. That is acceptable only where a second run cannot be seen, which is
  §4's question.

A refused input costs one fast reading plus one recording reading. §6 covers who pays that.

## 3. How the two readings are emitted (Q7.5)

Three ways, cheapest to write first.

**A. A flag the reader holds.** One `bool Record` field on the reader struct (and a local in the
engine). Every reader site becomes `if (Record) Refuse_DotGram(...)`, the `Noted` sites the same,
and the engine's `Fail:` block is gated by it as it already is by `lookahead < 0`. One body. The
cost on the fast path is a load and a predictable branch where there is now a call with a
`ref Failure` and a compare. Code size grows by a few bytes per site. Nothing about the C# 8 floor
changes.

**B. A generic flag the JIT folds** (Q7.5). The reader becomes
`ref struct Reader<TRecord> where TRecord : struct, IRecording`, and each site tests
`default(TRecord).On` — an instance member of an empty struct, since static abstract members need
C# 11. The runtime specializes generic code over value types on every target, .NET Framework
included, and the fast instantiation has the sites removed outright. The reader being a `ref
struct` is no obstacle: it is the generic type, not the type argument. Costs: the recording
instantiation is JIT-compiled on the first refused input, which for the EL is a large method set,
so the first failing call gets slower; and whether .NET Framework's x86 JIT folds the test is for
the prototype to check, not to assume.

**C. Two bodies.** Emit every reader method twice. The fast body is clean and needs no JIT
cooperation. It roughly doubles the reader half of the output: the EL tape file is 1.64 M
characters now. Consumer compile time and first call grow with it.

**Recommendation: A first, measured; B only if A leaves more than noise on the stand's EL rows.**
The recorded cost is in the refusal path — the call, the `ref` struct copies, the compare and the
tie list — and A removes all of it but a branch. B's gain over A is that one branch per site, at
the price of a second JIT instantiation. C is last: it is Q7.5's rejected shape, and it spends
code size, which has its own cost on first call.

## 4. The second reading and `context`

A grammar with `context : @T` hands the reading an object its guards and constructions write
into. The EL's `State` is written by `Declare`, `Takes`, `Awaits`, `Scoped`, `Settles`, `Imports`,
`Rebind` and `Refuse` (`ExpressionParser.cs:2682-2850`), and the host reads it back after a
refusal (`State.Refused`, `RefusedAt`). A second reading over a used `State` would see
declarations the first one left: names that exist, scopes already recorded. It could then answer
differently, which breaks §2.

Whatever the second reading does, it has to start from the state the first one started from. The
generator cannot know how to rewind a type it did not write. So there are three choices:

1. **No second reading where a context is declared.** Recording stays on for those grammars. It
   is safe, and it leaves the EL — the one grammar this is for — exactly where it is. That defeats
   the purpose.
2. **The context says how to rewind, by shape.** Where the context type has an accessible
   `Mark()` and `Rollback(x)` taking what `Mark` returns, found through `ISymbolResolver` the way
   §7.3 finds constructors, the publication marks before the fast reading and rolls back before
   the second. The EL's `State` already has both: `Mark()` returns a `Checkpoint` of the list
   counts, the refusal and the deferred flag, and the hand parser uses exactly this to re-read
   after a refusal (`HandExpression.cs:418-446`). Where no such pair exists, choice 1 applies.
3. **The same, declared.** A grammar says it, for example as a clause on the `context`
   declaration. It is explicit, and it is a language change.

**Choices 2 and 3 are changes to what the notation means**, since a method name becomes part of
the bond with C#, so they are Igor's to decide. I recommend 2. It asks nothing of a grammar that
has no context, and a host that wants the fast reading writes two methods it may well have
already. The duck typing has precedent in §7.3.

Two things a rewind has to cover, which the EL's `Checkpoint` already covers:

- `Rebind` rewrites `_declared` entries rather than appending. The EL's rollback truncates to the
  counts, so a rebinding made after the mark is undone with its entry. A host with in-place
  writes older than the mark would have to keep them out of the reading, and the contract has to
  say so.
- `Awaits`/`Settles` are an open/close pair (Q5 left them as they are). A fast reading that fails
  between the two leaves one open; the rollback's count truncation closes it. This needs a test.

**Carriers.** Under `Immediate` every construction runs while reading, so the second reading runs
them again. For the EL that is the same as a guard running again: pure builders over a rewound
`State`. The contract has to state it: under `Immediate`, a refused input runs a construction up
to twice per derivation tried, where `CarrierKind.cs:36-44` now says once. Under the tape,
constructions run after acceptance and a refused input runs none. The only exception is the
values a guard asks for, which are materialized mid-reading and run twice like guards do.

**Exceptions.** A construction that throws, which the EL host catches as a refusal
(`ExpressionParser.cs:1363`), throws in the fast reading, and there is nothing to read again.
Unchanged.

## 5. Where it does not apply

- **Streamed input** (`TextReader`, `IEnumerable<string>`, and `Stream`): recording steers window
  extension (§1), and once a `find` or a stage has moved on, the text before it is gone. **These
  keep recording as today. Nothing is weakened, so there is nothing to bring back.** A cheaper
  variant — keep `Position` and `Starved` and drop only the sets — is possible later. It would
  still need a way to say what was expected without the text, which is exactly the weakening Q7.2
  says to ask about first.
- **Buffered text and bytes** (`BufferedText`/`BufferedBytes`) where the grammar releases the
  buffer (`Machine.CanReleaseBuffered`, `ReleaseBefore`): the prefix may be gone at the refusal.
  Recording stays. Where nothing is released, the whole input up to `maxRetained` is still there
  and a second reading is possible, but that is a second step, not the first.
- **Recovery grammars**: the messages are made during the parse. Recording stays.
- **`yield` publications**: an error surfaces during enumeration, after elements have been handed
  out. Recording stays.

In the first cut, the in-memory `Parse`/`TryParse` forms change (`string`,
`ReadOnlySpan<char>`, `byte[]`, `ReadOnlyMemory<byte>`), and `find` and the lexer's throw-away
readings drop recording outright.

## 6. Who pays for the refused input

A refused input now reads twice. Where recording costs nothing, as in SQL and most character
grammars, that is up to 2x on failure for no gain on success. A `TryParse` used to validate
input that is often bad (FIX fields, web headers) would see it. Two ways to keep that honest:

- **Measure it**: a refused-input row per family on the stand, before and after. That is the
  number this change moves in the wrong direction, and without it, it moves unseen.
- **Apply it only where it pays**: the generator can decide per grammar, for example by the share
  of reader sites that record, but not by a measurement. I'd rather apply it everywhere it is
  eligible and let the refused-input rows say whether a grammar should be exempt. That keeps it
  from becoming an option (Q3's list of options left behind by experiments).

## 7. Found on the way: the reader does not keep lookaheads quiet

`Ways.Lookahead` (`Support.cs:1431`) is read by `Refuse_DotGram` (1721) and never incremented
anywhere. The increments lived in the old direct writer (`6061b437`) and went with it in
`487362c5`, and the comment at `Machine.Reader.cs:826` still describes them. So a refusal inside
a lookahead **records** on the reader and does **not** on the engine, which gates `Fail:` by
`lookahead < 0`. The two renderings can report different positions for the same grammar and
input. The EL does not show it: its lookaheads over kinds compile to door tests, not to calls.
This is outside Q7.2, a defect in performance-3f's area, and it matters here in one way: the
second reading must use the same rendering as the first. It does, because it is the same method.

## 8. Order of work

1. Recording off in `find` and in the lexer's throw-away readings. No second reading; a stand
   row confirms.
2. Choice A on the in-memory reader and engine, for grammars without a `context`. Stand rows for
   success and for refused input, EL excluded (it has a context).
3. Igor's decision on §4. Then the EL, through `Mark`/`Rollback`, with tests: a refused input's
   `Match` is identical field by field, `State` after a refused `TryParse` equals `State` after
   today's, `Awaits` left open by a fast reading is closed, `Immediate` runs a construction at
   most twice.
4. B, only if the stand shows A leaving a measurable share on the EL.
5. The hand parsers (D1): nothing forced. They read what the generated parser reads, and what
   they record is their own business as long as the messages agree. The EL hand parser already
   keeps its furthest position with one compare per token.
