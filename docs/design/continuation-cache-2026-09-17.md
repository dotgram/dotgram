# Cache contextual analysis on a settled graph

## Implementation

Precedes, the public Possessive entry point, and NeverGivesBack now memoize their
answers on the RecognitionGraph that owns the rule definitions. No entry is made
while FIRST rule estimates are still changing. The recursive Possessive overload
that carries an Asked set is deliberately not cached: its recursion context is
not part of the public-entry cache key.

The first 256 eligible requests per graph do not allocate contextual dictionaries.
This is a lazy allocation threshold, not a parser option or a promise that every
small grammar stays below it. It avoids creating the dictionaries for short-lived
graphs with little contextual analysis.

A key contains node identity, seam identity, and the IDs of the Plain and AfterSeam
FIRST sets. Node occurrences are not structurally merged: capture ownership and
other occurrence-sensitive metadata retain their existing meaning.

FIRST IDs are interned by content within the graph. Content equality includes all
three flags (Anything, Nothing and Ends) and the ordered ranges. Hash collisions
are resolved by exact comparison. A second dictionary remembers the ID by object
identity, so repeated access to the same FIRST object does not rehash its ranges.
Continuation keys combine the two child IDs without traversing their contents.

This applies the content-ID idea to the values used by analysis, without changing
the Node hierarchy or introducing a process-wide registry. Graph-owned references
are released with the graph; GramCompilation returns sources and diagnostics,
not the graph. The existing assumption that a graph and its node/range collections
remain unchanged during analysis still applies.

## Measurements

The baseline already includes deferred compilation of discarded machines. Two
fresh SQL generator processes per variant were run in baseline/candidate/candidate/
baseline order, without overlapping builds or tests. Preparing Roslyn inputs and
writing source hashes are outside the timed region. No generated C# was compiled
as part of these measurements.

| Metric | Baseline mean | Candidate mean | Change |
|---|---:|---:|---:|
| SQL source-generator time | 19.128 s | 17.615 s | -7.91% |
| Managed allocation | 11.602 GB | 11.333 GB | -2.32% |
| Generated C# characters | 59,608,778 | 59,608,778 | unchanged |

All six SQL output files are byte-identical. Allocation is cumulative managed
allocation, not a measurement of retained memory or process peak. Two samples
do not establish a confidence interval, and no parser runtime change is claimed.

TinyScalar was measured in two process pairs with the order reversed, using 21
fresh drivers per process and excluding the first run from warm medians. The
first pair was 6.165/6.305 ms (baseline/candidate); the reversed pair was
6.273/5.685 ms. The timing difference changed direction, so these samples do not
establish a regression or a speedup. Median allocations stayed around 982 KB
per generation, differing by less than 1 KB between variants.

Raw runs, TinyScalar samples and generator assembly hashes are saved in
`benchmarks/results/continuation-cache-2026-09-17.json`.

## Correctness checks

The focused tests exercise repeated equal continuations built from new FIRST
objects, distinct continuations, seam-sensitive results, end-of-input flags,
unknown sets, separate graphs and distinct range sets with colliding hashes.
They also cover both outcomes of the cached repetition proofs. The 33 focused
FIRST, sibling-publication and snapshot cases passed. The complete core suite
then passed all 8,308 tests. Web and ExpressionLanguage output hashes also match
the baseline exactly.

## Scope left for later

This does not cache Nullable, does not assign structural IDs to nodes, and does
not cache emitted states. It also leaves the representation of FIRST ranges
unchanged. These are separate experiments; none is required for this cache.
