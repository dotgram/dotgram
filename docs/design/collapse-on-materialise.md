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
The cost is one parallel array, or a second pass — the build loop runs backwards, so it cannot
recover the number by counting as it goes.

**This is the whole of the difficulty, and it is a contained one.** Everything else on the list
follows from numbering being preserved.

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

Two ways out, and the choice needs measuring rather than taste:

- **(a) Do not collapse over an open way.** A span with no way open inside it can never be
  re-entered. The fast path already tests `roots < 0 && root == ways.Records - 1`; whether "no way
  open within `[first, root]`" is as cheap to ask is the thing to check — `Ways.Count`/`Cursor`
  are to hand, but they are counts, not positions.
- **(b) Make the collapse undoable.** Remember the header the collapse overwrote and put it back on
  give-back. `UnwindRecords` already yields two conditional stores; a third is the same shape, and
  the memory is one entry per collapse, not per record.

(b) is more general and (a) is cheaper; (a) may also simply be true of every case that matters,
which a count would settle before either is built.

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
