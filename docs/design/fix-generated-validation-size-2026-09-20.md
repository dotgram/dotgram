# What generated validation costs in code, against walking the tables (2026-09-20)

D72 puts the build-time road on generating **code** rather than filling the same tables earlier,
and the architect asked for one number before the generator is designed: the size of the emitted
code. The number turned out to depend on a decision the number was meant to inform, so there are
two, and one of them is the answer.

## The fact that split the question in two

In the 93 messages of the published FIX 4.4 dictionary, **3,225 members are written**. After
expanding components, there are **12,532**. `Instrument` is named by twenty messages, and unrolled
code gets twenty copies of it.

So "unroll per message type" and "share a component's code" are not two knobs but opposite ends of
one. Unrolling pays for four times the member positions; sharing gives most of the size back —
and, the worry went, most of the benefit with it, because the indirection through a component is
part of what the walk pays for.

Both forms were therefore emitted and weighed.

| | methods | member positions | `Generated.dll` | against the 811,008-byte package |
| --- | ---: | ---: | ---: | ---: |
| **A**, unrolled per message type | 823 | 12,527 | **297,984 B** | +36.7 % |
| **B**, a method per component | 196 | 2,904 | **138,240 B** | +17.0 % |

A costs **2.16×** what B costs, and 158 KB more.

## Time, because size alone would have chosen blind

A and B, and the walk as it stands, over the largest fixture the package keeps — an
`ExecutionReport` of 391 fields with nested groups — best of fifteen batches of two thousand,
three interleaved rounds in one process:

| round | A | B | the walk |
| --- | ---: | ---: | ---: |
| 0 | 4,525 ns | 4,369 ns | 10,147 ns |
| 1 | 4,218 ns | 4,197 ns | 9,956 ns |
| 2 | 4,207 ns | 4,177 ns | 10,082 ns |

**A buys nothing.** B is inside one percent of A and is the lower of the two in every round. The
difference is not merely small, it does not have a sign.

**The walk's column is not comparable and must not be read as a speedup.** The generated forms do
membership, duplicates, required members and group recursion; the walk does all of that *and* the
value and code-set checks, on all 391 fields. Its 10 µs includes work the other two columns do not
do at all. What the walk column is good for is a floor: whatever generated validation eventually
costs, it starts below this and has the per-tag checks still to pay for.

## The answer

**B.** Same time as A within noise, less than half the code. And the argument does not depend on
resolving the last one percent: a difference too small to see across three interleaved rounds
cannot pay for 158 KB, whatever a measurement taken inside a proper timing window would say about
it. If someone wants the sub-percent figure anyway, that one needs a window; this conclusion does
not.

The architect's third option — generate only for the message types a consumer names — is now
arithmetic rather than a guess: at B's shape the whole schema costs 138 KB over 93 types, so a
consumer who names five pays on the order of a few kilobytes plus what their components drag in.
It is worth having, and it is not needed to make B affordable.

## What the measurement found on its own

**Membership cannot be shared, and that is a property of the problem rather than of the emitter.**
The first form B called a component's method on the scope the caller was already walking, and the
method walked it again — so every field outside the component was reported as out of place. A
component's code can carry its *required* checks, read off the caller's mask, and the groups it
holds. It cannot carry its membership: the calling scope has to list every tag of every component
it names, which is why B's switches are as wide as A's and its saving is in the required checks,
the group bodies and the method count rather than in the case labels.

This was caught by asserting that the two forms **answer the same** before timing them: A said
nothing about a valid message and B said 1,667 things. A speed comparison between two things that
disagree measures nothing, and here the disagreement was not a slip in the harness but the design
being wrong. See also the rule recorded from Q25: a check between two implementations belongs on
the path a consumer uses.

## Expectation against outcome, since the expectation was written first

Predicted before measuring: A roughly 380–750 KB ("the assembly about doubles"), B roughly
100–190 KB, and — named explicitly — that the error was more likely to be an *under*estimate of A,
because thousands of case labels stop being linear per label.

B landed at 138 KB, inside. **A landed at 298 KB, below the floor, so both the size and the named
direction of the error were wrong.** Thousands of case labels turned out to be cheaper per label,
not dearer: the compiler's jump tables and binary searches over a dense integer range are more
compact than the switch-of-a-few-dozen intuition they were extrapolated from.

## What this does not say

The value, type and code-set checks are not in either generated form. They go by tag and not by
message, so they are shared in both and their absence moves A and B equally — which is why the
comparison between them holds — but both absolute sizes are therefore floors, and the walk's time
is not comparable to either.

`FixSchema` stays in the package whole in every case. It is what a message is **built** from, not
only what it is checked against, so whatever the generator emits is an addition with nothing to
subtract. "+17 %" means seventeen per cent more, not seventeen per cent instead of the tables.

## How to take the numbers again

The emitter was a harness in `DotGram.Finance.Tests`, removed once the numbers were taken. It read
`FixSchema` directly, wrote one `Generated.cs` per form into `.work/size/`, each with a project
referencing `DotGram.Finance`, and the two assemblies were built `Release`, deterministic, with no
debug symbols, and weighed. The timing harness referenced both generated assemblies at once so
that the two forms ran in one process, warmed 200 calls, and took the best of fifteen batches of
two thousand.
