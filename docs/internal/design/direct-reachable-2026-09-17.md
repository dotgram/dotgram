# Successful direct reachability cache (rejected)

CanDirect repeats DirectReachable when joining publications. The experiment kept
successfully validated rule closures in each Machine, reusing the first traversal's
HashSet. Later checks skipped those closures. Failed traversals were not cached,
preserving the first refusal and avoiding incorrectly accepting partial cycles.
DirectGuardNeeds still recomputed publication-dependent flags.

## Measurements

Release standalone genprof, full DotGram.Sql, fresh processes, sequential runs;
no concurrent tests/builds. Baseline includes shared factories and deferred sites.
The six generated sources matched byte-for-byte by SHA256 (59,610,816 characters).

| Metric | Baseline | Candidate |
| --- | ---: | ---: |
| Run 1, seconds | 16.208 | 17.544 |
| Run 2, seconds | 17.639 | 17.986 |
| Run 3, seconds | 20.067 | 20.599 |
| Mean seconds | 17.971 | 18.710 |
| Mean cumulative allocated bytes | 9,461,887,171 | 9,246,392,240 |
| TinyScalar median milliseconds | 5.997 | 6.245 |

Order: baseline/candidate/candidate/baseline/baseline/candidate. Timings drifted
upwards, so these runs do not isolate a precise causal slowdown. Nevertheless,
there is no demonstrated speedup: mean time is 4.1% worse despite saving 215.5 MB
of allocations (2.3%). TinyScalar used 21 drivers, excluding the first for medians;
one tiny pair is only a smoke comparison.

The implementation was rejected and reverted. No candidate test suite was run
because the benchmark failed the performance gate. The retained production code
is identical to the previously validated state (8,345 passing tests). The profiling
executable was rebuilt after reverting. No Descendants implementation changes.

Raw data: benchmarks/results/direct-reachable-2026-09-17.json.
