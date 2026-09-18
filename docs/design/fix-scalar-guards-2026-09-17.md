# FIX: direct scalar demand in guards

## Change

A guard whose typed captures are all independent scalar leaves now builds those
values directly at the guard. It reuses the full-span leaf proof introduced by
[rule methods](fix-rule-methods-2026-09-17.md). This removes the general dependency,
link and recovery walks from `switch` and `when` for these leaves.

This deliberately retains the existing typed cache and its rollback invalidation.
The string parser may need a value again when it constructs fields after recognition.
Replacing that cache with just the current field's locals would lose earlier values
or repeat user factories. This step removes a pass, not the entire value store.

Factory timing, once-per-derivation caching and reverse arena construction order
remain the same. Multiple pending values are selected by their actual arena index;
missing optional captures and already-built values are skipped. Wide guards (more
than four typed captures), sequences, dependent values and members that can capture
different rules with a shared result type keep the general materializer. The small
limit bounds the generated selection loop's quadratic work.

The implementation is automatic, with no new user option. The final version applies
to string/span engines, including recovery. Buffered inputs retain the previous
materialization path: the trial showed inconsistent stream performance, including
a 13.2% Order/byte-stream regression in a reversed-order run. All 54 static Buffered
method bodies in generated FixGrammar match the baseline text. No FIX grammar
change is required. Generated source grows from 572,887 to 575,143 bytes (+0.39%).

## Validation

New tests cover three scalar factories with observable construction order, an
absent optional scalar, repeated demand, and different rules captured under one
name. Existing tests cover demand on abandoned branches and after rollback.
Streaming tests use one-item buffers; emitted code is compiled at C# 8.

The full core run passed 8396 tests with the broader trial enabled. After restricting
the optimization to unbuffered inputs, all 71 switch/recovery tests passed, including
an assertion that the multi-value direct path is emitted. All 6417 Finance tests
passed against the final assembly. The C# 8 compatibility project built cleanly.
Prebuilt example/package dependencies were reused for the core run.

## Final paired measurements

The baseline already includes the preceding rule-method improvement. Release
net10.0, 1.5 s warmup, nine alternating batches, two fresh processes with reversed
order. Values and diagnostic messages are checked before timing. No builds or tests
ran concurrently. The numbers are exploratory medians, not confidence intervals.

| String workload | Forward before -> after, us | Reverse before -> after, us | Time saved |
| --- | ---: | ---: | ---: |
| One field | 0.229 -> 0.218 | 0.213 -> 0.201 | 5.1-5.4% |
| Order, 15 fields | 2.682 -> 2.532 | 2.680 -> 2.450 | 5.6-8.6% |
| 64 short binary pairs | 14.230 -> 13.300 | 14.258 -> 13.078 | 6.5-8.3% |

Managed allocations per warmed call remain 184, 1448 and 6744 bytes respectively.
A separate single scaling run measured 4000 fields at 754.5 -> 692.2 us, 8000 fields
at 3606.8 -> 3454.6 us, and recovery at 0.994 -> 0.922 us. Allocations are unchanged;
the 8000-field case still allocates 24,362,160 bytes when exceeding arena retention.
That boundary remains sensitive to GC and is not resolved by this change.

Three fresh first-call processes per assembly measured medians 7.896 -> 7.784 ms,
with 3552 managed bytes in both versions. Assembly loading and reflection lookup
precede measurement; the first reflected Parse call, enumeration and JIT are timed,
not process startup. This is not a working-set measurement.

[Raw samples, hashes and the rejected buffered trial](../../benchmarks/results/fix-scalar-guards-2026-09-17.json).
Scratch harnesses, baseline assembly and full logs are under `.work/fix-scalar-guards`.
The paired harness accepts `string-only` and `reverse` after its two assembly paths.
Set `FIX_SAME_PARSER=1`; the legacy `Fix44Us` field denotes the candidate FixParser.

## Remaining work

The typed tables and caller capture records still exist. The next larger step is
specializing a whole deterministic field reader, with an explicit proof of capture
lifetimes and deferred factory timing. Stream guard specialization remains disabled
until a revised implementation demonstrates a repeatable gain.
