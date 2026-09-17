# Defer captured-call site planning until machine selection

Move PlanSites from Machine construction to the start of CompileRules. Deferred
candidate machines no longer assign site capture slots unless they survive grouping.
Strategy selection does not read the site tables; ordinary capture numbering stays
in the constructor. Immediate machines still prepare sites before compiling rules.
No new cache or runtime parser infrastructure is introduced.

## Measurements

Baseline includes shared factories, excludes rejected shared layouts. Full SQL
Release standalone genprof, fresh processes, baseline/candidate/candidate/baseline,
no overlapping builds or tests. All six source hashes match (59,610,816 characters).

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Generation | 18.776 s | 17.199 s |
| Cumulative allocated bytes | 9,471,137,752 | 9,476,292,524 |

Elapsed decreased 8.40% in these two pairs; the percentage is provisional given
run-to-run variation. Allocation changed +0.054%, effectively unchanged for this
measurement. Peak memory was not measured.

TinyScalar: 21 drivers per process, excluding first, median 5.909 -> 6.124 ms,
984,296 -> 984,112 bytes. One smoke pair is insufficient to establish a regression;
single-machine code still executes site planning once before rule compilation.

## Validation

252 focused sibling-publication, buffered input, emitter, snapshot and recovery
boundary tests passed. SQL, ExpressionLanguage and Web hashes match. Non-SQL hash
checks overlapped tests, so their elapsed times are not benchmark results.
git diff --check passed. genprof is rebuilt with the final implementation.

Raw data: benchmarks/results/deferred-sites-2026-09-17.json.
