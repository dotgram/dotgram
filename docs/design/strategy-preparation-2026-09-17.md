# Separate strategy selection from full machine preparation (rejected)

The experiment kept factory shapes, node ownership, execution planning, context
flags and FOLLOW analysis in construction. Capture storage, recovery plans,
engine entry states and value tables moved into idempotent Prepare, called before
CompileRules. Strategy checks needed two fixes: value presence must not require
engine rule IDs, and repeated guard captures must use a local CaptureLayout.

## Results

Baseline includes shared factories and deferred sites. Full SQL Release genprof,
fresh processes, no concurrent builds or tests during timings. All six generated
SQL source hashes match; output contains 59,610,816 characters.

The first implementation prepared guard candidates eagerly. Its mean changed from
21.379 to 20.195 seconds, but the reverse pair reversed the apparent improvement;
allocations increased 20.8 MB. This was not evidence of a reliable gain.

The revised implementation removed that early preparation. Runs alternated
candidate/baseline/baseline/candidate:

| Metric | Baseline runs | Revised candidate runs |
| --- | ---: | ---: |
| Generation, seconds | 16.609 / 16.665 | 16.591 / 16.526 |
| Allocated bytes | 9,448,631,704 / 9,476,766,848 | 9,467,019,176 / 9,473,208,320 |

Mean time improves only about 0.47%; mean allocation increases about 7.4 MB.
This does not justify the additional lifecycle complexity. The implementation was
reverted; shared factories and deferred PlanSites remain.

## Validation

The initial candidate passed 8,344 of 8,345 tests. The remaining semantic test
exposed KeepsRecords requiring an engine ID during strategy selection. After
fixing that and removing early guard preparation, all 654 focused semantic,
emitter, sibling, buffered, snapshot and recovery tests passed. The revised full
suite was not rerun before rejecting the experiment. Earlier SQL, ExpressionLanguage
and Web hash comparisons also matched for the initial candidate.

After reverting, the normal profiling executable and tests were rebuilt. All 8,345 tests passed (159.984 s); git diff --check passed.
Raw data: benchmarks/results/strategy-preparation-2026-09-17.json.
