# What generated validation buys in time, over the whole of the work (2026-09-20)

Igor: time at start-up does not matter, the speed of **validation** does. That names the number
the size measurement could not give. There both generated forms left the value and code-set checks
out — they go by tag rather than by message and are shared however the code is arranged — so the
walk's column was recorded as a floor and explicitly not as a comparison. This is the comparison,
over the same work.

## What "the same work" is

Both sides do, on every message: membership of each field in its scope, duplicates within a scope,
the value held to its tag's type and code set, required fields, required components, every group
entry with its count and the order of its fields, and `MessageEncoding` where an `Encoded` field is
present. The encoding pass walks every field on both sides.

**Length/data adjacency is on the walk's side only**, and that is not a flaw in the comparison but
part of what is being measured: the walk asks a table twice a field whether that tag is either half
of a pair, and a generator knows statically that it is not. Naming it matters more than removing
it — a later measurement that includes it on both sides will read differently, and should say so.

**The two sides answer identically.** Asserted before anything was timed, over all 186 fixtures the
package keeps, on rule, scope, tag, group tag, entry index and position. That assertion is what
caught the design error in the size measurement, and it is the reason the numbers below compare two
things rather than two different things.

## The numbers

One process, one message — the largest fixture, an `ExecutionReport` of 391 fields with nested
groups — 300 warm-up calls, best of fifteen batches of two thousand, three interleaved rounds.

| round | the walk | generated | generated, minus the value check |
| --- | ---: | ---: | ---: |
| 0 | 9,971 ns | 8,184 ns | 5,585 ns |
| 1 | 10,046 ns | 8,023 ns | 5,261 ns |
| 2 | 10,083 ns | 8,105 ns | 5,289 ns |

**Generated validation is 1.24× the walk.** Not a multiple: about 1,930 ns of 10,000 on this
message.

## Where the rest of the time is, and what is still on the table

The third column is the same generated code with the value and code-set check left out. It costs
**about 2,720 ns**, a third of the generated form's total — and it is *the same function on both
sides*: `FixPrimitives.Valid`, with a string switch over two dozen type names and a linear scan of
a code set that runs to 94 values for `SecurityType`.

So the ledger for this message reads:

* **1,930 ns** — what the build-time road has already taken: two table lookups a field turned into
  constants, plus the adjacency question it no longer has to ask.
* **2,720 ns** — what it has not touched yet, because the check is still a shared call. This is
  D72's "code sets as switches": a switch on the value instead of a scan, and the type test inlined
  per tag rather than dispatched on a string.
* **5,380 ns** — the rest, which is walking the fields and the findings machinery, and which
  neither arrangement removes.

If the second line went to nothing, the generated form would be about 1.87× the walk. **That is a
ceiling, not a prediction**: emitting those checks means reproducing `FixPrimitives`' semantics
exactly, including `IOIQty` accepting either a quantity or a code and `MultipleValueString` taking
its codes a character at a time, and any divergence there breaks the equality the comparison rests
on.

## Correction, taken the same day by doing it

The 2,720 ns above was read as "what the road has not touched yet", and the reading was wrong. It
is the value check's **whole** cost, and most of it is not lookup overhead at all.

Fixing the shared implementation, which is what that reading argued for, was done and measured on
the same message with the same instrument. Two changes paid and one cost:

| | the walk |
| --- | ---: |
| before | 9,792–9,910 ns |
| the type as a code instead of a name | 9,451–9,568 ns |
| the code set remembered a tag instead of searched for | 9,038–9,201 ns |
| *filtering the scan on length and first character* | *9,340–9,376 ns — **reverted*** |

So the dispatch and the two lookups were worth **about 750 ns**, not 2,720. The rest of the value
check is `Numeric`, `Date`, `Time`, `Letters` and `Market` reading the characters of 391 values,
which is the work itself and does not go away by being emitted rather than called.

The filter is the more useful of the three. A ninety-four-value linear scan looks like the thing to
fix, and it is not: the sets a message actually meets are short, `SequenceEqual` is already cheap,
and the branches that skipped comparisons cost more than the comparisons. It is reverted, and the
reason is written where the loop is so that nobody tries it twice.

**What this does to the ceiling.** The earlier 1.87× rested on the 2,720 ns being removable by
emitting the checks. It is not, so the ceiling is much nearer than it looked: with the shared fixes
in, the walk is about 9,100 ns and the generated form's advantage is still roughly the same
absolute 1,900 ns, which is where it will stay until something removes character work rather than
dispatch.

## The answer to the question that was asked

The build-time road buys **speed as well as types, but a quarter and not a multiple**.

As first written this section went on to say that the larger half of the remaining cost sat
somewhere the road had not been pointed at yet. The correction above is the measured answer to
that, and it is no: three quarters of what looked available was never dispatch, it was reading the
characters of the values, and emitting the checks instead of calling them does not touch it. What
the shared fixes took — about 750 ns, or eight per cent of the walk — is most of what was there,
and both roads got it at once.

## Expectation against outcome

Written before measuring: 1.3–1.8×, with the error named as more likely an *overestimate*. The
outcome was 1.24× — just under the floor, and the named direction right. The reasoning that led to
it was also right in substance: the walk's per-field cost is two table lookups, a string switch and
a linear scan, of which generation removes the first two and leaves the last two alone.

This is the second prediction of the day and the first whose named error direction held. The one
before it — the size of the fully unrolled form — was wrong in both magnitude and direction, and
the lesson from it applies here too: the figure that is safe is the one measured at the size and
shape it will be used at.

## How to take it again

A harness in `DotGram.Finance.Tests`, removed once the numbers were taken, because the emitted code
calls `FixPrimitives`, which is internal. It read `FixSchema` and wrote two classes — the whole of
the work, and the same minus the value check — as 1.95 MB and 0.88 MB of C#; the equality test ran
first and the timing only after it passed.

Nothing emitted was committed. The generated text is derived from our own compiled tables, but the
rule stands on its own: what is generated from a dictionary does not go into this repository.
