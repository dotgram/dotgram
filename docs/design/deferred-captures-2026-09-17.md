# Rejected deferred capture-lifetime analysis experiment

Baseline includes shared factories and deferred PlanSites. Candidate deferred
Doors.ByRule, Looped and population of nested/repeated capture sets until CompileRules,
before PlanSites. It reused existing capture slots and owners to avoid new caches.

Full SQL standalone Release generation, fresh processes in baseline/candidate/
candidate/baseline order. No overlapping builds/tests during timing. SQL source
hashes match, 59,610,816 generated characters.

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Generation | 19.355 s | 20.904 s |
| Cumulative allocated bytes | 9,423,658,344 | 9,413,940,604 |

In two pairs elapsed increased 8.00% for only 9,717,740 bytes saved (0.103%). The
experiment is rejected. These runs do not isolate the cause or establish universal
regression. No peak memory claim is made. The machine source was restored to its
pre-experiment state and normal genprof rebuilt. Earlier retained optimizations
(shared factories, deferred sites) are unaffected. Descendants was not changed.

Raw results: benchmarks/results/deferred-captures-2026-09-17.json.

Validation: 337 focused emitter, reader, buffered input, sibling-publication,
snapshot and recovery tests passed with the candidate. Production and test binaries
were rebuilt after restoring the retained implementation. git diff --check passed.
