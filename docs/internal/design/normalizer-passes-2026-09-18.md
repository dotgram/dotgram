# The normalizer's passes: what builds and what optimizes (D14, inventory)

An inventory, not a proposal: every pass `GrammarNormalizer.Normalize` runs, in order, sorted by
what D14 asks — does it **build** (establish what the grammar means: lower it, resolve it, work
out values, check it) or **optimize** (change the shape of the graph and neither the language nor
the values built) — and, for each, what it reads and writes, whether running it twice is
harmless today, and whether it would mean something different over kinds than over characters.
The design of `Optimize(graph) → graph` comes after it, separately.

Paths are under `src/DotGram/Grammar/Model/`; `GN.x.cs` is `GrammarNormalizer.x.cs`, and `L:n`
is the line in `GrammarNormalizer.cs` that calls the pass.

## The state passes share

- `_bodies`, `_rules`, `_nullable`, `_results`, `_types`, `_diagnostics`, `_whenSound`
  (GN.cs:87-93).
- By node: `_bounds`, `_powers`, `_recoveries`. By rule: `_climbing`, `_says`, `_externals`,
  `_folds`, `_trivia`. Publications, readings, context and state.
- `Carry(from, to)` (GN.Recursion.cs:168) moves the node-keyed facts to a node that was rebuilt;
  its list (`Annotations`, :194) is `_bounds`, `_powers`, `_recoveries`, `_folds`, `_climbing`.
  Two passes re-key `_recoveries` by hand instead (Lists, `Results.Repeated`).
- The graph built at the end also keeps memos (FIRST, FOLLOW, calls, recursion, reachability),
  and `Factor` builds a provisional one to ask them of (GN.Factoring.cs:377).

## The passes, in order

| # | Pass | Kind | Writes | Twice harmless today? | Over kinds |
| --- | --- | --- | --- | --- | --- |
| 1 | `Collect` (L:132) | builds | `_rules` | no — appends again | neutral |
| 2 | `LowerAll` (L:133) | **mixed** | bodies, rules, trivia, bounds, recoveries, externals; GRAM4005/4009/4010/4013/4015/4025 | no — reads the syntax tree | character-bound (literals into ranges, word boundaries, trivia seams) |
| 3 | `CheckUnused` (L:141) | builds (check) | GRAM4018 | no — reports again | neutral |
| 4 | `SpecializeWithSites` (L:150) | builds | bodies, rules; GRAM4017 | no — clones again | neutral |
| 5 | `SpecializeNamespaces` (L:151) | builds | bodies, rules, publications | no | neutral |
| 6 | `SpecializePublicationWith` (L:152) | builds | readings; GRAM4023 | no | neutral |
| 7 | `DecideIntersections` (L:156) | builds | bodies; GRAM4020-4024 | yes — only bodies still holding a condition, reports de-duplicated | character-bound (lists strings from literals and ranges) |
| 8 | `ComputeTypes` (L:160) | builds | types; GRAM4011 | values yes, GRAM4011 again | neutral |
| 9 | `LowerGroupValues` (L:161) | builds | rules, bodies, types | yes before `Factor`; after it, a fold's construction could be lifted into a rule | neutral |
| 10 | `RewriteLeftRecursion` (L:162) | builds | bodies, folds, climbing, powers; GRAM4002/4009 | rewritten rules yes; refused ones report again | neutral |
| 11 | `ComputeNullability` (L:163) | builds (analysis) | `_nullable` | yes — recomputed from scratch, fixpoint | neutral |
| 12 | `ProduceFromExternals` (L:168) | builds | bodies | yes | neutral |
| 13 | `CollectSequences` (L:172) | builds (§4.1 case 2) | bodies, recoveries; GRAM4008 | rewritten yes; failed report again | neutral |
| 14 | `ExtentValues` (L:176) | builds | bodies, types | yes | neutral |
| 15 | `PassThrough` (L:177) | builds (§4.1 case 3) | bodies | yes | neutral |
| 16 | `SpaceLists` (L:182) | builds — inserts a seam, which changes the language | bodies, recoveries | yes | trivia-bound |
| 17 | `UnweaveTrivia` (L:187) | **optimizes** | bodies | yes | trivia-bound: meaningful over characters only |
| 18 | `HoistTextCaptures` (L:192) | **optimizes** | bodies | yes | character-bound: "turns are contiguous" is not true over tokens with trivia between them |
| 19 | `CollapseTransparent` (L:196) | **optimizes**, but also moves `on fail` to sources (`_says`) | bodies, says | yes | neutral |
| 20 | `ComputeResults` (L:198, and inside `Factor` and `LowerYieldPublications`) | builds | results; GRAM4007 | values yes — GRAM4007 is raised on every run (it runs up to three times) and the compiler keeps one error per position, so it is said once | neutral |
| 21 | `CheckRebindingReplacements` (L:202) | builds (check) | GRAM4014 | reports again | neutral |
| 22 | `BuildByConstructor` (L:206) | builds (§7.3) | bodies | yes | neutral |
| 23 | `Check` (L:208) | builds (checks) | GRAM4001/4002/4003/4006/4008/4010/4012 | reports again | neutral |
| 24 | `Factor` (L:213) | **mixed**: folds shared prefixes, and says GRAM4016 where it cannot | bodies, results, diagnostics | **no, in two ways**: a fresh instance says GRAM4016 again; and a second run may fold further (a prefix stopped at a non-leading capture or at `take > room`), with no fixpoint around it | through FIRST/FOLLOW/`Determinism`, which carry no alphabet — but the seam `Committed` reads is `graph.Trivia`, which a split graph does not have |
| 25 | `ReconcileContexts` (L:217) | builds | context; GRAM4025, GRAM3019 | reports again | neutral |
| 26 | `ReconcileState` (L:218) | builds | state; GRAM3020 | reports again | neutral |
| 27 | `DecideRewind` (L:219, Q7.2 step 3, not yet on main) | builds | rewinds; GRAM5013 | reports again | neutral |
| 28 | `CheckPublicationTypes` (L:222) | builds (check) | GRAM4028 | reports again | neutral |
| 29 | `LowerYieldPublications` (L:223) | builds | publications, rules, bodies, recoveries | **no** — `Publication.Yield` stays true when `Kind` becomes `Yield`, and a second run takes the other branch and reports a spurious GRAM4027 | neutral |
| 30 | `Prune` (L:225) | neither: cleanup | rules, bodies, types, results, nullable, folds, climbing | yes — returns at once when nothing is unreached | neutral |
| 31 | `CheckMarksHaveState` (L:229) | builds (check) | GRAM4029 | reports again | neutral |

Beside them, inside `LowerAll`, four rewrites that only change shape: `MergeLiterals`,
`MergeAdjacentElements`/`Coalesce`, `Flatten`, and a choice of one becoming its alternative. They
optimize, but they are run on the tree as it is lowered and are character-bound (a literal of
one character becomes a range).

## What the split does and what it runs again

`LexicalSplit.Split` (LexicalSplit.cs:196) rewrites every body over kinds — a literal and an
element become kind tests, a word becomes its kinds, a call to a valued terminal stays, a
lexical call becomes nothing and the sequence it was in closes up — and records which new node
each old one became (`_became`). It then runs **one** normalizer step: `FactorCommittedPrefixes`
(GN.Factoring.cs:12, called at LexicalSplit.cs:232).

That step is a second, weaker `Factor`. It folds the longest shared prefix of a choice whose
alternatives are all constructions, it proves nothing with FIRST, FOLLOW or `Determinism` — its
soundness is that a token commits — it reports nothing, it does not `Carry` (it skips rules with
annotations instead), and in practice it is idempotent: after one run the tails differ at their
first part.

Everything else the syntax graph holds is taken from the character graph as it was: nullability,
types, results (plus the valued terminals and the factored slots), `on fail` messages,
diagnostics (the same list), publications, externals, context. Recoveries, powers and climbing
are re-keyed through `_became`; folds are rebuilt. Trivia is empty.

## GRAM4016 over kinds

`Factor` says it (GN.Factoring.cs:531-547) where two or more alternatives begin alike and
`Determinism.Of` cannot show that sharing the beginning keeps the language. It is said once per
rule, never where the grammar recovers, and **only over characters**: nothing asks again after
the split, and the syntax graph inherits the list with the warning in it. So a rule whose
alternatives over characters could not share a beginning, and over kinds could, keeps a warning
that is no longer true of the parser that runs — SQL:2023's `SetTarget`, per D14.

(`docs/diagnostics.md` still describes GRAM4016 as "that operand leads back to the rule"; the
code has not required that since `SharedPrefixTests.cs:57`.)

## What is already there to build on

- Fixpoints: `ComputeNullability` (a `changed` flag) and `ResolveProduced` (a `settled` flag).
  Nothing loops over passes.
- "Nothing changed" by reference: most rewrites hand back the same node when no child changed
  (`CollapseTransparent`, `HoistTextCaptures`, `SpaceLists`, `Factor`, `FactorCommittedPrefixes`,
  `Unfolded`). That is the natural test of a fixpoint — a pass that returns every body by
  reference changed nothing.
- De-duplicated reports: inside the normalizer only `DecideIntersections` (`_said`), `Factor`
  (`_declined`) and the readings threshold, and every other check reports again on a second run.
  **Outside it, `GramCompiler.OnePerPosition` (GramCompiler.cs:291) keeps one *error* per
  position** — the first raised — and leaves warnings and information alone. So an error a pass
  says twice about the same place reaches the author once; a warning said twice reaches them
  twice.

## Found on the way

1. **GRAM4007 is not reported twice, though it is raised twice.** `ComputeResults` reports without
   de-duplication and runs again inside `Factor` whenever anything in the grammar folded, so a
   mismatched capture is raised once per run. The second is an error at the same position as the
   first and `OnePerPosition` drops it: a grammar with `v: Item | v: 'y'` and a rule `Factor`
   folds compiles with one GRAM4007, before and after (the test
   `SemanticTests.And_says_so_once_where_the_grammar_is_folded` holds it there). Not a present
   defect; a reason the Optimize design says Check reports once rather than relying on the
   compiler to drop what a re-run repeats, which would not cover a warning.
2. **`LowerYieldPublications` is not idempotent, and is run once.** A second run would raise a
   spurious GRAM4027, but nothing runs it twice today, and under the Optimize design it is a
   builder, which is never re-run. Not a present defect either: a constraint on where it may stand.
3. **A dead computation in `Check`**: `Doors.ByRule` is computed and never used (GN.Checks.cs:245),
   and `Reaches(Node, RuleSymbol)` and `Leading` are unused.
