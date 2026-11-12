# Share factory descriptions between sibling machines

ResultTypes now lazily retains factory descriptions per (RecognitionGraph, RuleSymbol).
It is scoped to one emission host, including the host's type naming. Different graphs
are distinct keys. Factories and member descriptions are consumed read-only; buffered
byte variants still create copies with their own method names. This removes repeated
layout and member discovery in CSharpEmitter.FactoriesOf without changing machine
selection, numbering, or generated parser infrastructure.

## Measurements

Baseline: 09eb0b3b, after merging main. Full SQL standalone Release generation,
fresh processes in baseline/candidate/candidate/baseline order, no overlapping
builds or tests. Two pairs are indicative, not a confidence interval.

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Generation | 17.277 s | 16.580 s |
| Cumulative managed allocation | 9.558 GB | 9.509 GB |
| Generated characters | 59,610,816 | 59,610,816 |

Time decreases 4.03%, allocation 48,351,236 bytes (0.51%). This does not measure peak
retained memory. SQL, Web and ExpressionLanguage source hashes match byte for byte.

TinyScalar: 21 drivers per process, discard first, median 5.781 -> 5.882 ms,
985,192 -> 984,112 allocated bytes. One pair does not establish a timing regression.

## Validation

250 focused sibling, buffered input, emitter, external input and snapshot tests:
249 initially passed, one Minimal snapshot was stale from the main merge. Running
the pre-change generator reproduced exactly the same new snapshot. Reviewed the
recovery changes and refreshed the fixture; all 18 snapshot/recovery-boundary tests
then passed. Generator and genprof rebuilt; git diff --check passed.

This is one repeated preparation step removed, not elimination of the 38 candidate
machines. Raw measurements: benchmarks/results/shared-factories-2026-09-17.json.
