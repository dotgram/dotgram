# `Optimize(graph) → graph` (D14, design)

A proposal for Igor and the architect, not a statement about the compiler. It builds on the
inventory (`docs/design/normalizer-passes-2026-09-18.md`, "the inventory" below) and on the
architect's frame for it. Nothing here is implemented.

## 1. What changes, in one paragraph

The normalizer becomes three stages and a check. **Build** establishes what the grammar means,
once. **Optimize** changes the shape of the graph and nothing else, as often as asked — after
Build, and again after the lexical split over kinds — and runs its passes to a fixpoint.
**Analyze** recomputes what is derived from shape (nullability, result slots, the FIRST/FOLLOW
memos) after every Optimize. **Check** is where every diagnostic about the final graph is said,
GRAM4016 among them, once, over the graph that goes to the emitter.

```
Build ──► Optimize ──► Analyze ──► Check ──► (split?) ──► Optimize(kinds) ──► Analyze ──► Check(kinds) ──► Emit
```

## 2. Which passes are which

By the inventory's table, with the architect's cuts:

| Stage | Passes |
| --- | --- |
| Build | 1-16, 20-23, 25-29, 31 of the inventory, and the builder half of 19 (below) |
| Optimize, characters only | 17 `UnweaveTrivia`, 18 `HoistTextCaptures` |
| Optimize, any alphabet | 19 `CollapseTransparent` (shape only), 24 `Factor` (shape only), the four shape rewrites now inside `LowerAll` (`MergeLiterals`, `MergeAdjacentElements`/`Coalesce`, `Flatten`, a choice of one), 30 `Prune` |
| Optimize, kinds only | `FactorCommittedPrefixes` |
| Analyze | 11 `ComputeNullability`, the slot half of 20 `ComputeResults`, the graph's FIRST/FOLLOW memos |
| Check | 3, 21, 23, 28, 31 and the diagnostic halves of 8, 10, 13, 20 (GRAM4007), 24 (GRAM4016) |

Two passes are split in two:

- **`CollapseTransparent`** moves an `on fail` message from a forwarding rule to the rule it
  forwards to (`_says`). Which rule a forwarder forwards to is a fact about meaning, so a builder
  pass, `SaysThroughForwarders`, records it before Optimize, in the same map, by the same rule
  Collapse uses today. Collapse keeps only the inlining.
- **`Factor`** today says GRAM4016 where it declines to share a beginning. It stops saying it and
  records the decline instead — which rule, which run of alternatives, why — in a table Optimize
  hands back beside the graph. Check reads the table of the last Optimize.

The four rewrites inside `LowerAll` move out of it into Optimize as passes over bodies. They are
character-bound where they fold one-character literals into ranges; over kinds a literal is a kind
test, and the same merging of adjacent kind sets is the kinds form of the same rewrite. That is
the one pass whose over-kinds form has to be written rather than skipped.

## 3. The contracts

**An optimizer changes shape, not meaning.** Stated as what it must keep:

1. the language every rule accepts;
2. every rule's members — names and types — and every construction's arguments;
3. every node-keyed fact, through `Carry` when a node is rebuilt (no pass re-keys
   `_recoveries` by hand any more);
4. the order of `_rules` and of `_publications`.

What it may change: the bodies, and therefore the slot layout Analyze recomputes. Item 2 is what
lets Build compute types and members before Optimize and the builders that need them
(`BuildByConstructor`, `CollectSequences`, `PassThrough`) run there — which is today's order with
the optimizers taken out of the middle of it.

**No diagnostics.** Optimize is given no diagnostics list at all, so saying something is not an
option it has. Whatever it declines, it records.

**A fixpoint, by reference.** Each pass takes a body and returns it — the same object where
nothing changed, which the rewrites already do (the inventory, "What is already there"). One
round runs every pass of the alphabet over every body; Optimize repeats rounds until a whole round
returns every body by reference, and stops there. A bound of eight rounds guards against a pass
that breaks the contract; reaching it is an internal error, not a diagnostic.

**Idempotent, therefore.** `Optimize(Optimize(g))` returns every body of `Optimize(g)` by
reference. That is the test the contract is held to, over every grammar in the repository.

## 4. Over kinds

`Optimize` takes the alphabet as a parameter, and the list of passes is chosen from it — not
passes that check a condition inside:

- **`UnweaveTrivia` and `HoistTextCaptures` are not run over kinds.** Not as a no-op that finds
  nothing: they are not in the list. Trivia seams do not exist over kinds (the lexer ate them),
  and hoisting rests on consecutive turns being contiguous text, which over tokens with trivia
  between them they are not.
- **`Factor` over kinds** proves sharing with `Determinism` as it does over characters. Where it
  reads a seam today (`Committed` reads `FollowSets.SeamOf(graph.Trivia)`), the seam is given by
  the alphabet: over kinds there is no seam between tokens, said as a value, not found by reading
  an empty `Trivia` and concluding it.
- **`FactorCommittedPrefixes`** is the kinds-only optimizer it is now, run inside the fixpoint
  after `Factor`. Its soundness is that a token commits, which `Determinism` does not know and
  over characters is not true; so it stays a separate pass, and `Factor` does not take it over.

## 5. GRAM4016

Said by Check, once, from the declines the last Optimize recorded, over the graph that goes to
the emitter:

- a grammar that is not split: from Optimize over characters, as today;
- a split grammar: from Optimize over kinds. The character graph's declines are dropped with the
  rest of what was true only of the character graph.

So SQL:2023's `SetTarget`, whose alternatives cannot share a beginning over characters and can
over kinds, is factored over kinds and is not warned about. That is the case D14 names, and the
one where emitted code is expected to change.

## 6. Output

Where Optimize finds nothing new, every grammar compiles byte for byte as today. The places where
that could fail, each to be checked with the emitted-code comparison (`docs/development.md`,
"Verifying an emitter change") before it is taken further:

- **results before and after Optimize.** Today `ComputeResults` runs after 17-19 and again inside
  `Factor`. Under the design it runs in Build (members and types) and in Analyze (slots). Contract
  item 2 makes the members the same; the slots are recomputed from the same final bodies, so they
  are the same numbers.
- **`LowerGroupValues` after a fold** (the inventory, row 9): it stays in Build, before Optimize,
  so a construction a fold leaves inside a shared tail is never lifted.
- **the four rewrites moved out of `LowerAll`**: applied to finished bodies instead of while
  lowering; a fixpoint over them reaches the same normal form, which is what the comparison has to
  show.

Where Optimize over kinds finds something new, the split grammars' code changes — SQL:2023, EL,
and any grammar compiled with `Lexical = true`. Those are measured on the stand before and after:
generation time, emitted size, and the families' rows.

## 7. Order of work

Every step is its own commit, checked with `verify.sh` against its parent.

1. **GRAM4007 said once.** `ComputeResults` reports it from a set keyed by rule and member; a
   test with a mismatched capture and a fold asserts one diagnostic. A present defect, fixed
   before anything moves.
2. **`LowerYieldPublications` idempotent.** It reads what it wrote (`Kind == Yield`) rather than
   what it was given (`Yield`), and a test runs it twice and asserts no GRAM4027. The same.
3. **Diagnostics out of the optimizers.** `Factor` records declines; Check says GRAM4016 from
   them. Collapse's `_says` half becomes `SaysThroughForwarders`. Output byte-identical.
4. **Optimize as a stage** over characters, with the fixpoint and the contract tests (idempotence
   over every grammar, no diagnostics). Output byte-identical.
5. **Analyze** as the one place derived facts are recomputed; `Factor` stops calling
   `ComputeResults`. Output byte-identical.
6. **Optimize over kinds after the split**, with `Check(kinds)` for GRAM4016. Output changes for
   split grammars; the stand measures it; SQL:2023's `SetTarget` is the named test.

Steps 1 and 2 need no decision. 3 to 6 wait for this design to be accepted.

## 8. Questions for Igor

1. **Emitted code of every split grammar may change in step 6** (factoring over kinds finds
   shares it could not prove over characters). That is the point of D14, and it moves generated
   code nobody asked to move. Acceptable as one step measured on the stand, or wanted as a flag
   first?
2. **The rewrites inside `LowerAll`** (section 2) could stay where they are — they are not wrong
   there, only unrepeatable. Moving them is what makes Optimize complete; keeping them is less
   change now. Which?
