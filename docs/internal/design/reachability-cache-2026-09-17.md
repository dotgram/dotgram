# Lazy publication reachability cache

The emitter repeatedly asks for the same root's reachability inside `Joined` and
publication grouping. Each request previously walked rule bodies and trivia again.
`RecognitionGraph.Reaches` now caches completed sets lazily per root. The cache uses
the existing cached `RuleSymbol` hash and equality; no global IDs or eager all-rule
index is introduced. Cached sets are internal and must be treated as read-only.
Traversal and insertion order remain unchanged to preserve generated source order.
The graph must remain unchanged after the first request, as with its other analysis caches.

## Measurement

Standalone Release genprof, full DotGram.Sql, one fresh generation per process.
Order: baseline, candidate, candidate, baseline. No concurrent builds or tests.
Baseline: dffa8b2e. Results include all generator stages, without compiling output.

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Time | 20.593 s | 20.498 s |
| Cumulative managed allocation | 11.294 GB | 10.218 GB |
| Generated characters | 59,608,778 | 59,608,778 |

Allocation decreases by 1.077 GB (9.53%). The 0.46% elapsed difference does not
establish a speedup. Allocation is cumulative, not peak retained memory; the cache
retains requested sets for the graph lifetime. All six SQL source hashes match.

TinyScalar: 21 fresh drivers per process, discard the first, compare medians.
- tiny-before: 6.438 ms, 981,952 allocated bytes.
- tiny-after: 6.624 ms, 982,592 allocated bytes.

One tiny pair is only a smoke comparison, not evidence of a speedup or regression.

Validation: 34 FirstSets, sibling-publication and snapshot tests passed, including
cycles, different roots, graph isolation, null roots, cached reuse and trivia calls.
`git diff --check` passed. The standalone profiling executable was rebuilt.

Raw measurements: `benchmarks/results/reachability-cache-2026-09-17.json`.
