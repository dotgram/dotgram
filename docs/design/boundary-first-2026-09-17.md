# Normalize keyword boundary membership once per literal

Fresh dotTrace Sampling/ThreadTime profile of SqlStandardParser identified
GrammarNormalizer.Bounded / Continues -> FirstSets.OfElement as 734 ms of 1344 ms
inclusive Normalize time. The boundary character class was normalized again for
each character in each keyword, and membership allocated a singleton FIRST set.

Bounded now resolves the element and normalizes its FIRST set once per literal.
Membership uses binary search over the sorted disjoint ranges. No additional cache
or parser runtime infrastructure is introduced. Empty literals and non-element
boundaries preserve their previous behavior.

## Measurements

Baseline includes the root reachability cache; the rejected direct-edge cache is
absent. Release standalone genprof over full DotGram.Sql, fresh processes, order
baseline/candidate/candidate/baseline, without concurrent builds or tests.

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Generation | 18.982 s | 18.220 s |
| Cumulative managed allocation | 10.238 GB | 9.778 GB |
| Generated characters | 59,608,778 | 59,608,778 |

Time decreased 4.01%; allocation decreased 460,540,728 bytes (4.50%). These are two
pairs, not a confidence interval. Peak retained memory and parser runtime were not
measured. All six SQL outputs match by hash.

TinyScalar, 21 fresh drivers per process, excluding first: median 5.968 -> 5.973 ms,
982,528 -> 982,008 allocated bytes. This smoke comparison shows no meaningful change.

A separate post-change dotTrace profile of SqlStandardParser reports Bounded at
469 ms and Normalize at 1188 ms, versus 734 ms and 1344 ms before. Sampling CPU
figures are approximate and should not be mixed with unprofiled wall-clock figures.
The remaining per-literal normalization may be a future target, but no cross-literal
cache was added in this change.

## Validation

Validation corrected in boundary-cache-2026-09-17.md: the initial 101-test run
used an older DLL after a build failure. A successful rebuild now passes 111
FirstSets, GrammarNormalizer, LexerEmitter, sibling-publication and snapshot tests. Added boundary edge/gap, mixed-literal, empty-literal and U+FFFF cases
through a named boundary rule. git diff --check passed. genprof.exe rebuilt.

Raw measurements: benchmarks/results/boundary-first-2026-09-17.json.
