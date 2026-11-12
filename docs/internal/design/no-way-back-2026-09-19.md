# A call that needs no way back (design, 2026-09-19)

> **Measured and declined (2026-09-20).** Written, made sound, made cheap, and then refused by what
> it moved: nothing. Across the five libraries it takes 70 of 222 causes away and 45 of 1,167
> replayed rules, and **no grammar changes carrier** — 41 on the tape before and after — while
> `points` stays 2,362/3,146 on T-SQL, so nothing downstream takes up what it frees. Generation is
> level (62 s against about 60 for `DotGram.Sql`), but the generated files grow: SQL:2023 +6.3%,
> SQL-92 +4%, T-SQL +0.6%. Fewer causes, more code, one carrier moved by nobody. The prototype is
> not committed. What is below is the design as it stood, kept because the argument in §2 and the
> watch list in §6 are cited from elsewhere, and because a later reader should see what was tried.
>
> **What would make it pay.** Something downstream that turns a freed cause into a point or a
> carrier: the fourth edge of `carrier-per-construction`, or a gate that reads `Replay.Keeps` per
> machine rather than per grammar. Until one of those exists, this buys a number that nobody spends.

For the architect, before any code. It answers the cause the critic reopened: `JoinedRight`'s 29
rules are replayed for a reading that is never in fact put back, and no form the grammar can write
says so (an atomic group around the tails and around the call were both built and left the report
at `replayed: 321`). The cure belongs to the analysis.

## 1. What is asked today

`Replay.Walk`'s sequence case gives a part the cause `Follows` when anything after it in the same
sequence can fail: the sequence fails with it, the position goes back, and the reading is put back.
That is sound and it is blunt. It does not ask whether any *other* reading of that part could make
the failing continuation succeed. Where none can, the reading that was taken is the only one there
is, and it is never put back for this reason.

`Determinism.NeverGivesBack` already asks exactly this question of a repetition's turn: a turn need
not be given back where what follows the repetition cannot begin where a turn begins. This proposes
the same question one level up, of a call's whole reading.

## 2. The claim

Let `R` be a part of a sequence and `after` what follows it there. Let **S(R)** be the set of
tokens at which a *different* reading of `R` could leave the position — that is, for every way
inside `R`, the first set of what that way would read again.

> Where `S(R)` and the first set of `after` are disjoint, and nothing in `after` is a guard, the
> reading of `R` is never put back for `after`'s failure.

The argument. The reader takes the first derivation of `R` that succeeds. Any other reading it could
be asked for is one of the ways inside `R` retried, and a retried way resumes the reading at the
place that way was opened — so the position afterwards either is the same as before, or its next
token is in `S(R)`. Where the token is in `S(R)` and `S(R)` misses `First(after)`, `after` refuses
there exactly as it refused before, so no other reading rescues the parse. Where the position is the
same, `after` reads the same tokens and refuses the same way — unless something in `after` reads what
`R` built or what it changed, which is what the watch list of §6 names.

## 3. What S(R) is, and how it is computed

Over the body of `R`, and through every call it makes, taking the first set of what a way re-reads:

| Construct | What a way re-reads | S contribution |
| --- | --- | --- |
| an optional `x?` | `x`, where the reading without it is tried | `First(x)` |
| a repetition `x*` | one more turn, or one fewer | `First(x)` |
| a choice | the alternatives after the one taken | `First` of those alternatives |
| a literal, an element | nothing: its end follows from its start | nothing |
| a call | the callee's own S, cached per rule | `S(callee)` |
| a lookahead | nothing: it consumes nothing | nothing |
| anything else | not reasoned about | everything, which refuses the test |

A rule whose body holds no optional, repetition or choice has `S = ∅` and needs no way back for any
continuation. `JoinedRight` has `S = First(JoinedTail) ∪ S(TablePrimary)` — the join types, the
hints, `JOIN`, `CROSS`, `OUTER`, and whatever `TablePrimary`'s own optionals begin with — and the
continuation there is `"ON"`, which is in none of them.

The sets are the ones `FirstSets` already computes, so the analysis is one walk of each body with a
cache per rule, in the same shape as `Replay`'s own walk.

## 4. Where it plugs in

One test in `Replay.Walk`, in the sequence case, where `here = elsewhere ? Because.Follows :
Because.Losing` is decided: keep the cause the part already had where `NoWayBack(parts[i],
Next(parts, i + 1, …))` holds. Nothing else moves: `Turn`, `Lookahead` and `Under` are untouched,
and a part that fails the test is answered exactly as today.

`Commit` reads `Replay`'s report, so points follow from it without a change of their own.

## 5. What it is expected to buy, and what it may cost

- **T-SQL:** `JoinedRight`'s cause, and with it 29 rules under it, of 321 replayed. How many other
  causes the test removes is not known and is the first thing to measure — a prototype behind a
  flag, counting causes before and after, on all five libraries.
- **Generation time.** Adding an analysis is what made T-SQL's generation 4 → 86 s once (D11 B), so
  the count comes with the generator's own time beside it, and the gate is three rounds.
- **What it does not buy.** The rules stay on the tape where another cause holds them: `InlineReturn`
  is one, whose 27 rules are held by the subquery's cause as well. Fewer causes is not fewer taped
  rules until the last cause on a rule goes.

## 6. The watch list

Three places need the same list and must not keep three copies of it: this analysis, the hoist of a
seam out of a rule, and expr's "a value to the guard at the close". It is written here once, and
the others cite it as **the watch list**.

**What is on it.** A node whose answer depends on more than the position it stands at:

| On the list | Why it is there |
| --- | --- |
| `when` | it reads what the rule built, so two readings that end alike are not alike to it |
| a lookahead, `?=` and `?!` | it succeeds or fails on what stands at a position without moving it |
| a look-behind | the same, backwards |
| an external or host-measured terminal that may match empty | the host decides it, and the host may read more than the text |
| `with state` | the host's state is what the reading changed on the way, so a reading cannot be moved away from what steers it |

**One list, and one of the three is stricter than it needs to be.** For the hoist and for the
close, every row earns its place: they move a reading to another position, and each of these can
tell one position from another. For §2's question two rows are there out of caution, not necessity.
Where a reading ends at the position the greedy one ended at, the continuation reads the same text
from the same place, so a lookahead — either way, and backwards — answers exactly as it did; only
what depends on the *reading* rather than the *position* can differ, which is `when`, `with state`,
and a host that may read state behind a terminal. The two are kept anyway: a shared list that
refuses a little too often costs a few causes, and two lists that drift cost correctness. Where one
of the three ever needs a row the others must not have, it gets a name of its own and says why,
rather than a second copy of this table.

## 7. The risk, and how it is held

Soundness rests on §2's argument, and the way it can be wrong is a way inside `R` that resumes the
reading somewhere `S(R)` does not name. The table above is written to refuse rather than guess: an
unrecognized construct contributes everything, which fails the disjointness test. The gates are the
usual ones — the corpora, the refusal record, the snapshots, `--carriers` — and a grammar whose
reading changes shows there, since a rule wrongly proved settled builds a value the parse then
throws away.
