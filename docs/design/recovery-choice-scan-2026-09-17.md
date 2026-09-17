# Bound recovery choice deactivation to the current attempt

## Problem and cause

After the main merge, FIX44 string parsing still took hundreds of milliseconds for
4,000 short fields while buffered yield readers took about two milliseconds.
Doubling the field count quadrupled string time. The cause is recognition, not
primarily materialization.

Each successful or recovered iteration called DeactivateChoices, walking backwards
through the entire recovering repetition. Earlier iterations had already turned
their Choice entries into Dead entries, but every later iteration visited them all
again. For N similarly sized fields this makes the total scan quadratic. Buffered
yield readers normally recognize one field per invocation, so they never accumulate
that long history in one recovering repetition.

A dotTrace Sampling/ThreadTime profile of the original parser attributed 8,063 ms
inclusive to Recognize_DotGram_Fields_Part59, out of 8,156 ms in the recognizer.
Own times were 3,234 ms in that part, 2,922 ms in ParserArena.get_Item and 1,891 ms
in ParserEntry.get_Kind. This localized the repeated history walk; timing results
below were collected without the profiler.

## Change and safety

Keep deactivating the current attempt's choices, then stop at its existing marker.
A normal attempt begins with the choice that resumes recovery; after commitment
that marker remains as Dead. A failed attempt unwinds that choice and appends a
PendingRecovery marker. Match both the recovery state and the current repeat index,
and require one of these marker kinds, so captures with coincidentally equal
numeric states and nested repetitions cannot be mistaken for the boundary.

The marker is part of the same arena that backtracking unwinds. This avoids a new
mutable watermark that would need its own rollback rules. Older iterations already
committed their choices. No new parser fields, arena entries, caches or buffers are
introduced. The existing boundary at the repeat itself remains a fallback limit.

Changes are in Machine.Recovery.cs. The Minimal generated-code snapshot records
the two updated loops. A new four-case test covers `*` and `+`, character-run
backtracking, repeated accepted fields, a recovered bad field, an outer alternative
that retries the recovering rule, missing terminators, character streams and
single-byte streams. Existing nested-recovery tests cover independent boundaries.

## Scaling: string input

Release generated code, .NET 10.0.12, default tiered compilation, fresh baseline and
candidate processes. One-second warmup per workload and seven samples; field count
is validated every call. The public string method materializes the entire result;
the scaling probe reads its array Length without boxing an enumerable. The fixture
repeats `55=ABC<SOH>38=100<SOH>54=1<SOH>44=12.50<SOH>`.

| Fields | Before, ms | After, ms | Speedup | Allocated bytes, unchanged |
| ---: | ---: | ---: | ---: | ---: |
| 4 | 0.00283 | 0.00250 | 1.13x | 424 |
| 1,000 | 20.679 | 0.581 | 35.6x | 94,048 |
| 2,000 | 86.439 | 1.266 | 68.3x | 188,048 |
| 4,000 | 335.869 | 2.445 | 137.4x | 376,048 |
| 8,000 | 1,365.160 | 6.398 | 213.4x | 18,219,584 |

The quadratic history walk is gone. The allocation discontinuity at 8,000 fields
is separate: inspection confirms that the 4,000-field parse retains an arena with
capacity 65,536, whereas the 8,000-field parse retains no spare parser. The existing
KeptEntries limit drops the oversized parser, so subsequent parses allocate their
working arrays again. This patch leaves that retention policy unchanged.

## Paired confirmation and stream controls

Two fresh processes, reverse assembly load order, 1.5-second warmup per
implementation/workload and nine rotating samples. Both use the same current
Release Finance model and public FIX wrappers. Inputs and encoded byte arrays are
prepared outside timing; readers are created and results fully enumerated inside.
Setup compares serialized fields including values, coordinates and error messages;
all cases match. No concurrent builds/tests run during these timings.

| Workload / form | Before, run 1 / run 2 | After, run 1 / run 2 |
| --- | ---: | ---: |
| 4 fields, string | 2.95 / 2.88 us | 2.62 / 2.62 us |
| 4 fields, character stream | 2.10 / 2.14 us | 2.04 / 2.14 us |
| 4 fields, byte stream | 1.61 / 1.63 us | 1.60 / 1.69 us |
| 4,000 fields, string | 333.63 / 336.74 ms | 2.474 / 2.444 ms |
| 4,000 fields, character stream | 1.933 / 1.971 ms | 1.940 / 1.973 ms |
| 4,000 fields, byte stream | 1.503 / 1.517 ms | 1.480 / 1.579 ms |

The large-string speedup reproduces at 135-138x. Character streams remain within
about 0.4% on the large fixture. Byte-stream differences range from 1.6% faster to
4.1% slower, with no consistent direction across load orders; do not claim a byte
speedup. The small string case improves about 9-11%. Recovery after a 4,096-space
invalid fragment also improves on string input, while streams remain close.

All paired allocation counts are unchanged, including 376,080 bytes for the
4,000-field string path and 344,288 / 280,320 bytes for character/byte streams.
The paired harness adds 32 bytes for array enumeration relative to the scaling
harness. These are diagnostic medians, not confidence intervals; cold startup was
not measured.

## Generated size and validation

FIX source grows from 59,441,690 to 59,446,130 characters: 4,440 additional characters
(less than 0.01%). No additional runtime storage is emitted. Candidate compiler
time is not compared: the full correctness suite overlapped that compilation.

All 8,384 core tests pass, including the new four cases and the updated snapshot.
The Finance/example Release build has no warnings or errors; all 3,835 Finance
tests pass against the rebuilt example. SQL, ExpressionLanguage, TinyScalar and
TinyStreams generated outputs remain byte-identical by per-file SHA-256.
git diff --check passes.

[Raw measurements](../../benchmarks/results/recovery-choice-scan-2026-09-17.json)
contain the scaling samples, both paired runs, allocation/retention checks and the
profile's top functions. Scratch programs, generated candidate and dotTrace snapshot
are under `.work/fix-string/` and `.work/fix-sharing/linear/`; the compiled candidate
is `.work/sharing-validation/fix-linear.dll`.
