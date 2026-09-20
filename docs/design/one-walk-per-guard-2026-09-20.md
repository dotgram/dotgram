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
of `n` into one walk removes `n - 1` of them. What that is worth dynamically is a count sql-39 can
take on the corpora — asks that belong to a multi-value guard, rather than sites — and it should be
taken before any code.

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
- **The corpora and the refusal record unchanged**, since nothing about what is built should move.
- **My expectation, stated before the pair**: the T-SQL rows move, because 92% of its asks walk
  slowly and nearly half its sites stand in runs; the SQL:2023 rows move less, because more than
  half of its asks take the fast path, where the preamble is all there is to save; the expression
  language moves least of all. If the T-SQL rows do not move, the merge is not worth its entry and
  is refused, as the last design was.
