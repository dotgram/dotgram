# FIRST fixed-point work queue

FirstSets.ByRule now initially visits every rule and then revisits only callers of
changed estimates. It reuses RecognitionGraph.Calls for reverse dependencies and
suppresses duplicate pending entries. Calls in lookahead and nullable sequences
are included through the full body walk. Estimates remain available during the
fixed point; settled node caching still starts only after the queue empties.

## Measurements

Baseline includes the root reachability and boundary FIRST caches. Full SQL Release
genprof, fresh processes, baseline/candidate/candidate/baseline, no concurrent builds
or tests. Six SQL outputs match; generated size remains 59,608,778 characters.

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Generation | 16.787 s | 16.683 s |
| Cumulative managed allocation | 9.674 GB | 9.562 GB |

Allocation decreases 112,588,192 bytes (1.16%). Elapsed difference is only -0.62%,
not strong evidence of a speedup in two pairs. No peak memory claim is made.
TinyScalar medians (21 drivers, excluding first): 6.036 -> 6.145 ms,
982,168 -> 983,272 bytes; one smoke pair cannot establish timing regression.

ExpressionLanguage and Web source hashes also match. These comparison runs overlap
with tests, so their elapsed times are not used as benchmarks.

Focused FIRST tests: 37 passed, including both rule orders, nullable prefix and
positive/negative lookahead dependencies. Full suite result recorded below.

## Supplied user profile

The attached profile places RenderReader at 4657 ms, CompileRules at 4531 ms, and
38 Machine constructions at 4333 ms. Only four machines reach CompileRules. These
instrumented times are not directly comparable to the unprofiled measurements above.
Per-machine preparation and repeated reader analyses remain promising next targets;
this change alone does not remove the cost of discarded candidate machines.

Raw measurements: benchmarks/results/first-queue-2026-09-17.json.

Full core run: 8321 tests, 8320 passed, one architecture guard failed because the
previous boundary cache was declared Dictionary<Node, First> and looked like an
annotation table. Its key is now accurately narrowed to Node.Element; values are
analysis results that must not transfer to replacement nodes. No cache algorithm
change was made. Rebuilt generator/harness/tests; all 71 Rendering and FirstSets
tests then passed. The full suite was not repeated after this key-type correction.
git diff --check passed.
