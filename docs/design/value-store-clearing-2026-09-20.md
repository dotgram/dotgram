# What clearing the value store costs, and the one way to not pay it (2026-09-20)

For the architect. The number is measured; the choice that follows is about retention and is not
mine to take.

## 1. The measurement

A pair of builds, the tree as it is against a tree whose `DirectValues.Return` clears nothing,
`sql-loop generated`, five runs a side, pinned, inside a slot the stand gave:

| | median | runs |
| --- | --- | --- |
| as it is | 18,106 ns a call | 17,934 – 18,480 |
| without the clearing | 16,958 ns a call | 16,709 – 17,080 |

**The clearing costs 1,148 ns of an 18,106 ns reading — 6.3%.** It agrees with what the profile
said (sql-39's anatomy: `SpanHelpers.Clear*` under `DirectValues.Return`), which is the point of
taking it two ways. My two models of it before this said 126 ns and 211 ns; both were wrong, and
wrong in the same direction sql-39's own estimates were — a model of work that touches memory
widely reads low.

**What is being cleared.** A dense store holds a table per value type — three hundred for
SQL:2023 — and `Add` hands out `N_i++` in the table for that type, so `N_i` is how many values of
that type this parse built. `Return` clears each table that was written, `[0, N_i)`, and then the
`Built` flags. For `select20` the whole parse builds about four hundred values across about
twenty-two tables: **five kilobytes, in twenty-odd calls.** At 1,148 ns that is some fifty
nanoseconds a call and about 2.5 ns a slot, which is far above what writing five kilobytes costs —
so what is paid is the calls and the cold tables, not the bytes. A cheaper clear of the same
regions is therefore not where the saving is.

## 2. Why it cannot simply be made cheaper

The work is already bounded by what the parse used: untouched tables are skipped, and a touched
one is cleared only up to its own count. The remaining levers are all small:

- **Skip the tables whose type holds no reference.** Sound — `Held<T>` is `{ T Value; }`, so for a
  value type with no references there is nothing to release and every slot is written before it is
  read. But the emitter knows type *names*, not whether a named type holds a reference, and it must
  stay free of Roslyn; a `RuntimeHelpers.IsReferenceOrContainsReferences<T>()` that the JIT folds
  away is above the `netstandard2.0` floor and needs a second spelling beneath it. It also only
  covers the few tables of built-in types — most of an AST is references.
- **A loop instead of `Array.Clear` for a small count**, or dropping the `Math.Min` that cannot
  bind. Both are shavings off fifty nanoseconds, and both cost emitted size on every one of three
  hundred tables.

## 3. The one way to not pay it, and what it costs

**Do not clear what the next parse will write over.** `Add` hands out indices from zero upward and
its caller writes the slot immediately, so every slot a parse uses is overwritten before it is
read. If a parse's own region is left as it is, the next parse of the same shape overwrites all of
it and nothing needs clearing at all. What must still be cleared is the *tail*: where this parse
used fewer slots than the one before it, `[N_i, M_i)` still holds the older parse's values. Keeping
`M_i` — the previous high-water — makes that a comparison that is usually false, and in a loop of
like-shaped parses the clearing disappears entirely.

**And the price is retention.** Between two parses the store holds the values of the last one, so a
thread that parsed a document and went quiet keeps that whole tree alive through its pooled store.
That is the leak this project has refused twice already — a thread-static holding a document
nobody else has — and it is exactly the objection that killed the weak-reference reuse in the
pools decision.

**So the shape has to be the one that decision landed on: retention measured in the thread's own
work.** The store is parked with its values; a count of consecutive parses that did not rent it
expires; when it does, the store is cleared or let go. The mechanism exists in the same file and
for the same reason, and this would be its second user rather than a new idea. What I cannot do is
decide that a parse's values may outlive it at all — that is the architect's, and Igor's if it
reaches what a consumer can observe in a memory profile.

## 4. What I would measure next, and it needs one short slot

The 1,148 ns is the value tables *and* the `Built` flags together, since the build I timed removed
both. The flags are needed whatever happens to the tables, so the number that matters for §3 is the
tables alone. One more pair — a build that clears the flags and not the tables — splits it, and it
is six minutes of the machine.

Nothing is proposed for the code until §3's question is answered.
