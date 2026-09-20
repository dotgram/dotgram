# One walk per guard, not one per value it names (2026-09-20)

For the architect. This replaces `guard-values-at-close-2026-09-19.md`, which its own numbers
refused. It asks for nothing from §3.7: the same records are built, by the same factories, at the
same moment — only in one pass instead of several.

## 1. What is paid now

A `when` that names several captured values materializes each of them with a walk of its own. In
the generated SQL:2023 that reads:

```csharp
var g0At = r6;
if (!(g0At < 0)) Materialize_…_Direct(ways, text, values, g0At, lm, lmR, …);
var g0 = …;
var g1At = r7;
if (!(g1At < 0)) Materialize_…_Direct(ways, text, values, g1At, lm, lmR, …);
…
```

Each of those calls pays the whole preamble — `values.Room`, which clears `Live` from the rule's
mark; the clearing of the `built` flags from the watermark; the `IndexOf` over them — and, where
the fast path does not take it, a listing pass over every record since the mark and a reaching pass
back over that list. sql-39 counted a slow walk at 12.3 records listed on `select20` and 14.9 on
the T-SQL corpus, with the clearing and the reaching running over the same elements. Three walks
from one guard list three times.

**How often that happens, counted statically in the generated code of this tree** (call sites that
stand within three lines of the next one, so in one guard's run):

| | call sites | runs | runs of two or more | sites inside them | longest run |
| --- | --- | --- | --- | --- | --- |
| SQL:2023 | 228 | 176 | 40 | 92 (40%) | 3 |
| T-SQL | 242 | 175 | 46 | 113 (46%) | 7 |
| the expression language | 62 | 46 | 16 | 32 (51%) | 2 |

So between a third and a half of the places that ask stand in a run with others, and merging a run
of `n` into one walk removes `n - 1` of them.

**And what it is worth dynamically, counted by sql-39 on the corpora**, with each ask marked by the
guard that made it rather than by the rule's mark — so "the same guard" means the same guard and
not a neighbour in the same rule:

| | asks | guards | second or later from the same guard | walks after merging |
| --- | --- | --- | --- | --- |
| SQL:2023, `select20` | 301 | 193 | 108 (36%) | 301 → 193 |
| T-SQL, the corpus | 17,023 | 9,287 | 7,736 (45%) | 17,023 → 9,287 |

An earlier count of sql-39's, by the rule's mark rather than by the guard, made this look three
times larger — 301 → 25 asks on `select20`. The difference is asks of *different* guards standing
in a row, and merging those is not sound: the first guard may refuse, and its neighbour's values
would then have been built for a derivation that is never accepted, which is what the tape is
chosen to prevent. Merging them would need a proof that nothing between them can refuse — the rent
that closed the last design. **So the reachable gain is the table above, and the larger number is
not ours.**

Two caveats sql-39 names and I keep: the count merges only *consecutive* asks of one guard, so a
guard whose asks are separated by another's is not counted though it might still merge — the
figures are a floor; and what a merge saves is a walk's fixed part, which on T-SQL is nearly a
whole walk (7.8% of asks take the fast path) and on SQL:2023 is mostly a preamble (56% do).

## 2. That the walk already does it

`Materialize_…_Direct` takes `roots` and `rootSlots` beside `root`: where `roots` is not negative,
it walks the side stack from there and marks live every gathered record whose slot is in the mask,
then lists, reaches and builds **once**. That is how a guard handed a sequence builds all of it in
one walk rather than one walk an element.

What it does not cover is this case, and the reason is small: a single capture's record is a local
in the reader (`r6`, `r7`), not an entry on the side stack, so a mask cannot name it. The change is
therefore to let the walk mark **several roots given as roots** — the same marking the list branch
does, over arguments instead of over the stack — and to have the reader emit one call where it
emits a run today.

Everything after the marking is unchanged: one listing, one reaching pass, one dispatch per record,
the same `built` flags and the same watermark.

## 2.1 What order things are built in, which is the one thing to hold rather than assert

A merged walk builds the union in log order. Two questions follow, and the answer is different for
each.

**Captures beside one another — the ordinary case — build in exactly the order they build in
today.** A rule reads its parts in sequence, so the records of one capture all stand before the
records of the next: their ranges of the log are disjoint and ordered. Log order over the union is
therefore the first capture's records followed by the second's, which is what two walks do one
after the other. Nothing can be observed to differ, because nothing differs.

**A capture that is an ancestor of another is the case where the order moves.** A guard may name
both a rule and something inside it. Today the inner walk builds the inner subtree whole, and the
outer walk then builds what is left of the outer one — so a record standing *before* the inner
subtree is built *after* it. Merged, everything is built in log order. The set is the same, every
child is still built before its parent, and the difference is only the order of two constructions
neither of which is the other's child.

Can that be seen? Not through the values: a factory is handed its own children and nothing else.
Only a factory with a side effect — one that counts, logs, or writes through `parserState` — could
tell, and the specification promises no order between two constructions that are not related
(§7.2 promises that guards run in written order and that `=>` runs after the match). So the merge
is allowed to take log order, and **the design says so rather than leaving it to be discovered**:
where a guard names a value inside another, the two are built in the order the input has them,
which is the order everything else is built in.

## 3. What it does not change

- **Which factories run, and when.** Exactly those the guard's values need, at the guard, as today.
  §3.7 is untouched; there is no proof to keep true, no watch list, no lookahead body, no turn of a
  repetition. This is the whole reason to prefer it to the design it replaces.
- **What is built.** The union of what the separate walks built, in the same order the walk always
  builds in — front to back over the log.
- **A guard that names one value.** Unchanged, and it is the majority of sites.

## 4. What it would cost

An entry that takes a few roots rather than one. The number of them is a choice: the runs are at
most three on SQL:2023 and seven on T-SQL, and a variant taking four with the rest falling back to
today's one-at-a-time covers everything measured. Emitted size is a signature and a marking loop,
not anything per rule.

## 5. What it must be held to

- **Both families in any pair**, because they fail the fast path for opposite reasons (sql-39):
  SQL:2023 on "the root is not the last record", T-SQL on "something below is unbuilt". A
  measurement on one is a measurement about one.
- **A size-growing row on every guarded family**, the standing rule for a change to the walk.
- **The emitted size quoted beside the time.** A walk that takes several roots where it took one is
  a wider signature on a hot path, and this is exactly the shape where the time improves, the file
  grows and nobody looks. One worktree, and say whether the BOM is in the figure.
- **The corpora and the refusal record unchanged**, since nothing about what is built should move.
- **My expectation, stated before the pair**: the T-SQL rows move most — 45% of its asks are a
  second or later from the same guard, and 92% of its asks walk slowly, so a merged ask saves most
  of a walk rather than a preamble. SQL:2023 moves less: 36% of its asks merge, and more than half
  of them take the fast path, where only the preamble is saved. The expression language moves least
  of all. This is the opposite of what sql-39's first count suggested, and it is the guard-marked
  count that decides.

  Built from the frequency of the run lengths and not from the longest one: the runs reach seven on
  T-SQL, but 1.83 asks a guard means the weight sits on pairs, so the expectation is about half the
  merged asks' walks and not six in seven.

  If the T-SQL rows do not move, the merge is not worth its entry and is refused, as the last
  design was.
