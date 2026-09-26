# Collapsing a materialised span on the tape

The materialising walk is quadratic on a nest because the outer level re-lists the records whose
values the inner guards have already built. The proposal: **after a guard materialises a span,
replace that span with one record saying "built"**, so the next level's walk sees O(1) records per
level rather than all of them.

This is a reading of the emitter to price it, and a count witness for the shape it must change.
No code, and no emitter change is proposed until Igor has agreed.

It replaces the per-cycle construction design (`9223d2c5`, dropped): that needed a second carrier,
touched 182 rules of SQL:2023, and rested on a soundness condition nothing could prove. This needs
none of that — construction happens exactly where a `when` already demands it, so there is one
carrier, one stack guard, no new exception can escape, and **D138's question never arises**.

---

## 1. The witness: what the walk costs now

`.work/matcount`, an instrumented copy of `SqlStandardParser` counting the records each walk lists
(`listed`), per parse:

| input | walks | records listed | per doubling |
| --- | ---: | ---: | ---: |
| `nested200` | 1,010 | 1,325,613 | |
| `nested400` | 2,010 | 5,251,113 | **×3.96** |
| `nested800` | 4,010 | **20,902,113** | **×3.98** |
| `wide200` | 2,803 | 68,401 | |
| `wide400` | 5,603 | 136,801 | ×2.00 |
| `wide800` | 11,203 | 273,601 | ×2.00 |

**The number of walks is linear in both** — 1,010 → 2,010 → 4,010. What grows is the length of
each: 1,312 records on average at 200 levels, 5,213 at 800. Linear walks times linear length is
the square, and flat input shows the walk is not quadratic by nature: laid out wide, the same
brackets cost ×2.00.

So the target is the per-walk length, and the prediction to test is that it becomes O(1) per
level: `listed` at 800 levels falling from 20.9 million to the order of 20,000.

This is the part the `AllBuilt` watermark (`bfb36682`) did **not** touch. That removed the
triangle in the *scan* that guards the fast path — 33,451,363 elements down to 4,050. The walk's
own triangle is what is left, and it is the larger of the two.

## 2. Why the tape makes this cheap: a record carries its own length

From `Support.cs`, `Ways`: the log holds "one record per completed valued rule, written after its
children, **each starting with its own length so that a walk from the front steps from record to
record**". The scan pass is exactly that:

```
for (var at = from; at < ways.LogCount; at += log[at])
    starts[listed++] = at;
```

So a span can be skipped **without moving any memory and without rewriting the log**: give the
first record of the span a length that reaches the end of it, and one hop steps over the whole
nest. The records inside stay where they are and are never visited. A collapse is one store.

That is the cheap half, and it is cheap because of a decision already made for other reasons.

## 3. Why it is not that simple: a record's number is its position in the walk

The build pass:

```
for (var back = listed - 1; back >= 0; back--)
{
    var at   = starts[back];
    var slot = first + back;
```

**`slot = first + back`.** A record's number — the index into `built[]`, `live[]`, the values
store, and the number a `Refs` entry holds — is *its index in the walk order*. It is not stored in
the log. So a walk that skips records renumbers every record above the span, and with it:

- `built[]` and `live[]`, indexed by number;
- `Refs`, whose entries are `(slot, record, -1)` — `live[ways.Refs[at + 1]] = true` in the
  multi-root path reads a record **by number**;
- `Built` and `AllBuilt`, both watermarks over numbers;
- `Last`, the number a guard captures, which is what the fast path's `root` is.

**The fix is to make the collapsed record say how many it stands for**, and have the scan advance
the number by that count rather than by one:

```
for (var at = from; at < ways.LogCount; at += log[at])
{
    starts[listed] = at;
    slots[listed]  = slot;
    slot += log[at + 1] == Built ? log[at + 2] : 1;
    listed++;
}
```

Numbering is then preserved exactly, every table above keeps its meaning, and the walk is short.

**Corrected TWICE. The second correction is the measurement's, and it is smaller than the first.**

Reading the emitter suggested a parallel `slots[]` array, the liveness pass re-indexed, and the
`selected` build loop changed from iterating record numbers to iterating listed entries — the last
of which would have removed the `IndexOf` skip over the compact `live` map that `e4ee376e`
introduced (**that commit cites no measurement at all: a one-line message, no body**). The gate the
architect set was to measure what that skip is worth before touching it. It is worth everything:

| SQL:2023, per parse | records LISTED by the scan | records VISITED by the build |
| --- | ---: | ---: |
| `nested200` | 1,325,613 | **1,017** |
| `nested400` | 5,251,113 (×3.96) | **2,017** (×1.98) |
| `nested800` | 20,902,113 (×3.98) | **4,017** (×1.99) |

**Visiting is already linear — about five records a level, across every walk in the parse.** The
whole triangle is in the scan. So the `selected` loop's skip is not something a collapse subsumes;
it is the reason the build half is not quadratic already, and changing it could only lose.

That also makes the change much smaller than §3 first said, because the two passes can keep their
number-indexed shape:

- **the scan writes `starts[slot - first] = at` instead of `starts[listed++] = at`**, and advances
  `slot` by the collapsed record's count. The holes it leaves are never read, because a collapsed
  record is not live and both later passes test `live` before using the position — so the holes
  cost no stores, which is what made "fill them" the wrong answer.
- **the liveness pass tests `live[slot]` before it uses `at`** rather than after, which is a
  reordering of two lines.
- **the `selected` build loop does not change at all.**
- **the log-walking build loop takes `slot += count`** in place of `slot++`.

No parallel array, no iteration order changed, and the one optimisation I was about to remove
stays. **T-SQL confirms the other half of the gate**: on the same shapes its search condition
materialises once, listing 208 records at depth 200 and 808 at 800 — listed equals visited, both
linear, no guard fast path taken at all. There is nothing there for the collapse to help or to
hurt, which is what a family that is not the target should look like.

## 4. What points into a span, one at a time

- **`ways.Opened`** — a *log position*, where the record being written begins. Nothing moves, so
  it does not move. A collapse happens below it, in records already closed.
- **`Refs` / `RefsCount`** — record numbers; preserved by §3. The side stack itself is untouched:
  a collapsed span's own refs were consumed when its root was built.
- **`Built` / `AllBuilt`** — numbers; preserved. `AllBuilt` in particular becomes *more* useful: a
  collapsed record is built by construction, so a span that collapses can raise it.
- **The multi-root path** (`roots >= 0`) reads `ways.Refs` from `roots` and marks `live` by number.
  Preserved.
- **Marks** — the marks pass walks `for (var at = 0; at < from; at += log[at])` and tests
  `log[at + 1] < 0` for a mark. The collapsed kind must therefore be **non-negative**, or the pass
  must learn it. It also walks from 0, so a collapsed span in front of `from` makes that pass
  shorter too, which is a second gain and not a cost.
- **The values store** — `values.Room(ways.Records, from: first)` is sized by the number of
  records, which §3 leaves unchanged. Slightly wasteful (the dead numbers keep their slots) and
  correct; shrinking it is a separate question.

## 5. Give-back is the one that can be wrong

`UnwindRecords` restores a way: `ways.LogCount = {name}` and `ways.Records = {name}R`, and since
`bfb36682` also lowers `Built` and `AllBuilt`. Truncating the log is safe for records written
**after** a collapse. The dangerous case is a mark that lies **inside** a collapsed span: the
span's first record still claims a length reaching past the new `LogCount`, so the next walk hops
from it to beyond the end and **silently skips every record written since**. That is a wrong tree,
not a slow one.

### Measured, 2026-09-25: the case does not arise, and that is the argument for guarding it

Counted in `.work/matcount` by recording each span a guard materialises (`[from, ways.LogCount)`,
both parameters of the materializer) and asking of **every** give-back — all 1,301 assignments to
`ways.LogCount` in `SqlStandardParser`, every one an `lmN` mark — where it lands:

| input | collapses | give-backs | **inside** | at/before the start | at/after the end |
| --- | ---: | ---: | ---: | ---: | ---: |
| `nested800` | 6,409 | 4,815 | **0** | 0 | 15,392,813 |
| `wide800` | 11,202 | 16,800 | **0** | 0 | 94,024,800 |
| the SQL corpus, 1,086 files | 1,293 | 2,608 | **0** | 61 | 27,922 |

The corpus is the reading that counts — two shapes I invented cannot say what a real script does —
and it is a favourable one for finding the event: read through the SQL:2023 query-expression door,
1,085 of its 1,086 files are **refused**, and a refusal gives back more than an acceptance, not
less. All three outcomes occur, so the instrument can distinguish them: 61 give-backs land at or
before a span's start, 27,922 at or after its end, and **none inside**.

### And the accepted path, and three shapes built to produce the event

| input set | read | refused | collapses | give-backs | **inside** | at/before start | at/after end |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| the SQL corpus | 1 | 1,085 | 1,293 | 2,608 | **0** | 61 | 27,922 |
| the test rows | 406 | 1,419 | 7,196 | 11,796 | **0** | 299 | 42,847 |

The corpus alone samples the refusal path (it is ScriptDom's T-SQL through a SQL:2023 door); the
1,825 `InlineData` rows of `SqlStandardParserTests` and `SqlStandardTreeTests` are the accepted
path through the same door. Both are zero.

**Three grammars were then written on purpose to produce it**, with the check emitted into every
parser and made to report any give-back that follows a materialise at all:

- *a guard after a repetition that can give a turn back, with a failing tail* — the repetition's
  turn marks are taken after the walk's start and before the guard. **No give-back occurs while a
  span is live**: the reader abandons the whole alternative rather than giving a turn back under
  the guard.
- *the same with the repetition inside the rule the guard names* — likewise none.
- *a second alternative after a guard* — a give-back does occur, and it targets **`to = 0` against
  the span `[0, 4)`**: exactly the span's start.

That last is the mechanism, and it is worth stating because it explains all three zeros. **A
give-back that follows a materialise abandons the alternative the guard was in, and the walk's
start is that alternative's own mark, so `to == from`** — the whole span goes, which is the safe
case. To land *inside*, a way would have to be opened after the walk's start, inside the guarded
rule, and still be takeable after the guard has run; none of the three shapes produces one.

**This is not a proof.** It is three input sets and three constructed shapes agreeing, with a
mechanism that makes the agreement intelligible. A proof would have to say why no reader can open
such a way, and I cannot say that yet.

### Found while implementing: (b) is not cheap, and (a) was not what I said it was

Two things came out of writing the code that the reasoning above had not reached.

**(b) as described costs O(collapses x give-backs).** "Remember the overwritten header and restore
it in `UnwindRecords`" needs, at each give-back, a search for a live span containing the target.
At depth 800 there are 6,409 collapses and 4,815 give-backs in one parse, and the instrument that
measured the zero did exactly this scan: **94 million comparisons on `wide800`**. That is fine for
an instrument and not for a reader. A restore that is a defended no-op must also be a CHEAP
defended no-op, and nothing above says how.

**And (a) is sound after all, because I conflated two different things.** What rests on an
empirical zero is (a) as an ASSUMPTION — "no way is open inside a span, so do not worry about it".
What I dismissed with it was (a) as a RUNTIME CHECK — "collapse only when no takeable way is open
inside this span", which is a precondition the reader tests and which is correct whatever the
corpus does. The zero says such a check would almost never decline; it says nothing against making
it.

The obstacle to (a) is that **a way does not carry its log position**. `Ways.Open(at, last)` stores
two integers, the alternative in force and the last one there; the log mark lives in a C# local
(`lmN`) in the reader method. So "is a way open inside `[from, LogCount)`" cannot be asked of
`Ways` as it stands. It would need a third integer per way — a small, contained change — and then
the question is O(1) IF the newest takeable way is to hand.

That last is where it stops being obvious. Marks increase with way order, so the newest way has the
largest mark; but at the moment a guard runs, ways opened inside its own rule after the walk's
start are exactly the ones with marks above `from`, and most of them are **spent** (a way whose two
integers are equal, kept on the tape so a replay reads the same decisions in the same places). A
check against the newest way regardless of spentness would decline almost every collapse; a check
against the newest TAKEABLE way needs either a scan down the spent run or a watermark maintained as
ways are spent.

**So this is the open question, and it is a design question rather than a coding one:**

1. one integer per way for its log mark, plus a maintained "newest takeable way" watermark, and
   collapse under an O(1) precondition — sound, and it needs no restore at all; or
2. keep (b) but make the search cheap, which needs the spans in a structure a give-back can query
   in better than linear time, and they nest rather than sort; or
3. something neither of us has thought of, which is why this is going back rather than being
   chosen here.

### Settled: a stack of collapses, amortized O(1), and the premise checked

The architect's construction removes the search. Keep the collapses on a **stack in the order they
are made**, each holding `(start, end, the overwritten header)`. A collapse covers
`[from, LogCount)` at the moment it is made, so its `end` is the log's current end. On a give-back
to `to`, pop every entry whose `end > to`; if its `start < to` restore its header — the straddling
case — and if its `start >= to` the header was truncated away and there is nothing to restore.
After the pops every remaining `end <= to`, and every later collapse ends at a `LogCount >= to`, so
**the ends on the stack are non-decreasing at all times**. Each entry is pushed once and popped
once: amortized O(1) per collapse and per give-back, and a give-back that reaches no collapse costs
one comparison against the top. Nested collapses fall out — an inner span is older and its end is
no later than the outer's — and there is no per-way mark and no spentness question.

**The premise it rests on is checkable, and it holds.** Three things had to be true:

1. *Every change to `LogCount` is an append, a give-back truncation, or the reset when a `Ways` is
   rented.* It is: `LogCount` is written at `spare.LogCount = 0`, at `Log[LogCount++] = …` in
   `Begin`, `Put` and `Mark`, and in the emitted `ways.LogCount = lmN` — of which
   `SqlStandardParser` has 1,301 and **all 1,301 are `lmN` marks**.
2. *A collapse ends at the current `LogCount`.* True by construction: the span is
   `[from, ways.LogCount)` at the point the guard has finished materialising.
3. *Nothing rewrites the log in the middle.* The only writes to `Log[]` that are not appends are
   `Log[Opened] = LogCount - Opened`, in the two `End` overloads — the currently open record's own
   length header. **And no record is ever open across a collapse**, because the emitter writes
   `ways.Begin(…)`, `ways.Put(…)`, `ways.End(rb)` adjacently, at the point a rule completes and
   after its children. So `Opened` is always the newest record and never inside a completed span,
   and a later `End` cannot clobber a collapsed header.

A parse reached from inside another takes a `Ways` of its own from `_deeper`, so nothing
interleaves on one tape; and locations change `Begin(arm)` to `Begin(arm, start, end)`, which is
still an append.

**So this is taken over both earlier options**, and it makes the test stronger than an invariant
assertion: a straddle can be forced by construction, and a naive version with no restore must give
a wrong tree on it.


a corpus is evidence, not proof, and the failure it would be evidence about is a *silent wrong
tree* — the worst kind to leave resting on an empirical zero, because an event that never happens
in testing is one no test will catch on the day it starts happening. (a) would make correctness
depend on that zero holding for every grammar anyone writes. (b) is correct whatever happens, and
what the measurement buys is the knowledge that its restore path will essentially never fire: the
cost is a comparison per give-back, not work.

Two ways out, and the measurement above decides between them:

- **(a) Do not collapse over an open way.** A span with no way open inside it can never be
  re-entered. The fast path already tests `roots < 0 && root == ways.Records - 1`; whether "no way
  open within `[first, root]`" is as cheap to ask is the thing to check — `Ways.Count`/`Cursor`
  are to hand, but they are counts, not positions.
- **(b) Make the collapse undoable.** Remember the header the collapse overwrote and put it back on
  give-back. `UnwindRecords` already yields two conditional stores; a third is the same shape, and
  the memory is one entry per collapse, not per record.

(b) is more general and (a) is cheaper; (a) may also simply be true of every case that matters,
which a count would settle before either is built.

## 5a. The two callers, and why only one of them collapses

The materializer is called with two shapes of argument, and the difference is not a detail of SQL:
there are exactly **two call sites in the emitter**, and they are different jobs.

| emitter | call | in `SqlStandardParser` | what it is |
| --- | --- | ---: | --- |
| `TapeCarrier.Materialize(record, mark)` | `(gNAt, lm, lmR)` | ~148 | **a guard**, walking from its own rule's mark |
| `TapeCarrier.BuildRoot(rule, type, extent)` | `(ways.Last, 0, 0)` | 80 | **a publication's answer**, walking the whole log |

`Materialize` is emitted where a guard is handed its members (`Machine.Reader.cs:4220`, beside
`{handed}At = FirstRecord(...)`), so it runs **during** the reading, once per guard per level, each
walk starting at the mark of the rule the guard is in. **That is where the nest's quadratic lives**,
and it is where a collapse pays.

`BuildRoot` is emitted at the end of a publication (`Machine.Reader.cs:1016`), after the reading has
accepted, and the line following it is `value = …`: it builds the tree the caller asked for. It
walks from 0 because the whole log **is** the answer. There are 80 of them because SQL:2023
publishes many rules in several forms, not because anything walks the log 80 times.

**Collapsing at a `BuildRoot` site is meaningless rather than harmful.** The walk happens once, its
result is read out on the next line, and then the parse is over and `Ways` goes back to the pool —
so a collapsed record would never be read by anybody. It would be pure waste: a store, and a span
nothing walks again.

So: **collapse at `Materialize` and not at `BuildRoot`**, which in the emitter is a decision made in
one place — `TapeCarrier.Materialize` emits the collapse, `TapeCarrier.BuildRoot` does not. It needs
no test of `from` at run time, because the two are already different methods.

It also sharpens §5's mechanism. The give-backs that follow a materialise are the ones that follow a
GUARD's materialise, and `from` there is the guard's rule's mark — which is exactly the mark the
reader puts the log back to when that alternative is abandoned. `to == from` is not a coincidence of
the inputs; it is the same number reached by two routes.

## 5b. The watermark start, taken over the log collapse

We do not have to collapse the log. We only have to stop the walk looking at what is already
built, and `AllBuilt` already says where that is. Give it a companion log position and the
materializer begins:

```
if (ways.AllBuilt > first) { first = ways.AllBuilt; from = ways.AllBuiltAt; }
```

Every pass then works unchanged, because all three are expressed in `from` and `first`: the scan
runs from the raised `from`, `starts[slot - first]` stays valid against the raised `first`, the
`selected` loop is untouched, and `Room` clears `Live` only from `first` upward
(`Array.Clear(Live, from, count - from)`), so the built values below are preserved rather than
cleared. A collapsed span and a raised watermark say the same thing to the walk; one says it in the
log, the other in two integers. **Give-back needs nothing new**: `UnwindRecords` already lowers
`AllBuilt`, and the log position is lowered beside it from the same local.

### The stale `live[]` question, checked rather than argued

A raised `first` means `Room` no longer clears `Live` over `[old first, new first)`, so entries
there are whatever a previous walk left. **Every use of `live[]` the emitter writes**, exhaustively:

| | where | index |
| --- | --- | --- |
| read | `if (!live[slot]) continue;` — liveness pass | `slot = first + back`, so `>= first` |
| read | `if (!live[slot])` — the `selected` build loop | `slot` starts at `first` |
| read | `!live[slot]` — the log-walking build loop | `slot` starts at `first` |
| read | `IndexOf(new ReadOnlySpan<bool>(live, slot, ways.Records - slot), true)` | from `slot >= first` |
| write | `live[slot] = false` beside `built[slot]` | `>= first` |
| write | `live[root] = true` | the requested root |
| write | `live[ways.Refs[at + 1]] = true` — the multi-root path | **can be below `first`** |
| write | `live[log[read]] = true`, `live[log[read + 1 + item]] = true` — `Reaches` marking children | **can be below `first`** |

**Every read is at an index at or above `first`; the two writes that can go below it are never read
back.** So a stale entry below the raised `first` cannot be seen. The two writes that go below are
marking records that the raise has just declared built, which need no building — and their values
are read from the store by number, not through `live`.

### What may NOT be done, and why

The obvious companion — "the full walk builds `[first, Records)`, so let it raise the watermark
too" — is **the wrong raise again**, and it is the same wrong raise the instrument caught before.
A full walk sets `built[]` only for LIVE slots, so after one there can be dead, unbuilt slots below
`Records`; that is precisely why `Built` is not all-built and why `AllBuilt` may be raised only
from its own test. A walk may raise it only where it can prove the range wholly built — a count of
slots the build pass skipped, and a raise under `skipped == 0`.

So the first landing raises nothing new. If the triangle does not go because too many materialises
take the full walk (measured: 6,409 of 10,419 calls take the fast path at depth 800, so 39% do
not), that shows up as the count not falling, and the skipped-count raise is the next question
rather than a guess made in advance.


## 6. What it does not do

- **The refused-input square stays.** These counts are of accepted readings. A refusal re-reads,
  and whether a collapse survives what a refusal does is not analysed here — it is parked with
  Igor's other note.
- **It does not make SQL:2023 linear on its own.** `3a8d5bcd` made a parenthesis and a tuple one
  way in, read once, and that is why the expression language reads ×3.8 for four times the depth
  on the new deep rows where SQL:2023 reads ×13.4. Part of SQL's remaining exponent is the grammar
  reading a bracket more than once, which no change to the tape can remove. **The two are
  independent and the grammar one may be the larger.**

## 7. How it would be measured

1. **The count first, before any timing.** `listed` per parse at 200/400/800 nested and wide, in
   the instrumented copy, against the table in §1. Linear is `listed` rising ×2 rather than ×4. If
   it does not, nothing else matters.
2. **A second count by another road**: `walks` must stay linear, so that a shorter walk has not
   been bought with more walks.
3. **The deep rows** `sql/nested-{100,400}.match` and the EL pair (`StandNesting.cs`), whose ratio
   is the whole point: SQL:2023 ×13.4 today.
4. **Size-growing rows on every guarded family** — EL, T-SQL, FIX, Web — per the `584a7c1f`
   lesson. FIX and Web have no guard that builds, so the assertion there is that they do not move.
5. **Generation time** on `DotGram.Sql`, by `Gate-Generation.ps1`.
6. **Both carriers agreeing**, and the whole SQL corpus: a collapse that loses a record is a wrong
   tree, and §5 says how it would happen.

## 8. What I would ask before building it

1. **Is (a) or (b) the answer to give-back?** §5. A count of how often a way is open inside a span
   a guard has just materialised would decide it, and that count can be taken before any of this
   is built.
2. **Is the grammar fix available for the bracket?** §6. If `ParenthesizedJoinedTable` can read a
   bracket once the way `3a8d5bcd` made a parenthesis and a tuple read once, that is cheaper than
   anything here and should be priced first. sql-47's `ScalarSubquery` finding is the existing
   thread — though note that `ScalarSubquery` is **not** in the 182-rule component the bracket
   guard sits in, so it is a separate reading.
3. **Is the parallel array acceptable?** §3 needs the number alongside the position, and the build
   pass runs backwards. One `int[]` the length of `listed`, from the same pool as `starts`, is the
   obvious answer; if it is not acceptable, the build pass has to run forwards, which is a larger
   change than the collapse itself.
