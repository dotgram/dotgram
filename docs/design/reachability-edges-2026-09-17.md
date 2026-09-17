# Rejected direct-edge cache experiment

Baseline: the lazy root reachability cache from `reachability-cache-2026-09-17.md`.
Candidate: graph-local direct-call arrays, including trivia, enabled after the first
root closure. Subsequent roots reuse arrays instead of walking rule bodies again.
Order and duplicates were preserved. No candidate changes remain in production.

Full SQL standalone Release generation, fresh processes, order baseline/candidate/
candidate/baseline. No concurrent builds or tests. Generated source hashes match.

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Elapsed | 17.911 s | 19.046 s |
| Cumulative allocated bytes | 10,308,953,520 | 10,300,335,268 |

In these two pairs elapsed time increased 6.34%, while allocation decreased only
8.62 MB (0.084%). This does not justify the extra retained arrays and dictionary.
The direct-edge cache was removed; the previous root-result cache remains.
Measurements do not establish a universal regression, but give no reason to retain
this implementation. The standalone profiler was rebuilt after restoring baseline.

Raw measurements: `benchmarks/results/reachability-edges-2026-09-17.json`.
