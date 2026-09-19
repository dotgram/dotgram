# Bound recovery-aware materialization — 2026-09-17

## Change

A typed switch or guard needs values before recognition completes. The previous emitter initialized guardFrom to zero whenever recovery plans were present, even if only the latest tag was needed. Repeated fields therefore repeatedly scanned the accumulated arena.

The guard now starts its bound at entries.Count and lowers it for every unbuilt requested value, including directly captured recovery entries. The recovery materialization loop respects the same bound. An indirectly needed recovery entry follows its owning call, which is itself reachable from a requested root. Acceptance still defaults to zero and materializes the entire accepted result. Existing linking, cache invalidation and rollback behavior are unchanged; no buffers, fields or allocation paths were added.

This bounds the relevant materialization walks; it does not claim to eliminate every possible history scan in grammars using parser state marks or guards over growing sequences.

## Measurement

Current Release net10.0; same workloads as the preceding Fix/Fix44 comparison. Two fresh processes with reversed parser order, 1.5 seconds of warmup and nine alternating batches. Tables average the two process medians. Input preparation is excluded; reader/stream creation and full result enumeration are included. No builds or tests ran concurrently. Cold startup is not measured.

Results match between old and new Fix, including invalid-field diagnostics. Fix/Fix44 equivalence checks concrete types, values, locations and lengths, excluding their different diagnostic Message text. Workloads exercise field parsing, not complete-message conformance.

## Fix before and after

Times are microseconds per operation.

| Case | API | Before us | After us | Speedup | Before B/op | After B/op |
|---|---|---:|---:|---:|---:|---:|
| Wire4 | String | 1.25 | 0.92 | 1.35x | 464 | 464 |
| Wire4 | TextReader | 1.34 | 1.35 | 0.99x | 648 | 648 |
| Wire4 | Stream | 1.15 | 1.17 | 0.99x | 616 | 616 |
| Wire4000 | String | 159476.52 | 808.10 | 197.35x | 376088 | 376088 |
| Wire4000 | TextReader | 1126.05 | 1109.61 | 1.01x | 344304 | 344304 |
| Wire4000 | Stream | 1018.58 | 1000.74 | 1.02x | 280336 | 280336 |
| Wire8000 | String | 747655.90 | 4386.67 | 170.44x | 25641624 | 25641625 |
| Wire8000 | TextReader | 2396.68 | 2223.32 | 1.08x | 688304 | 688304 |
| Wire8000 | Stream | 2630.88 | 2548.25 | 1.03x | 560336 | 560336 |
| Recovery | String | 1.54 | 1.33 | 1.16x | 952 | 952 |
| Recovery | TextReader | 2.09 | 2.09 | 1.00x | 1032 | 1032 |
| Recovery | Stream | 1.78 | 1.87 | 0.96x | 1160 | 1160 |

The 4,000-field whole-string speedup reproduces at 194x and 200x in the individual runs. At 8,000 fields it ranges from 136x to 208x; old timings are noisier there. The change adds no allocations: the approximately three-byte per-call discrepancy in one 8,000-field allocation sample is measurement noise. Tiny/streaming differences of a few percent are not treated as reliable changes.

## Current Fix versus Fix44

Times are microseconds per operation.

| Case | API | Fix us | Fix44 us | Fix44 / Fix | Fix B/op | Fix44 B/op |
|---|---|---:|---:|---:|---:|---:|
| Wire4 | String | 1.12 | 3.46 | 3.10x | 464 | 456 |
| Wire4 | TextReader | 1.24 | 2.20 | 1.77x | 648 | 632 |
| Wire4 | Stream | 1.06 | 1.69 | 1.59x | 616 | 600 |
| Wire4000 | String | 786.30 | 2493.78 | 3.17x | 376088 | 376080 |
| Wire4000 | TextReader | 1099.85 | 2013.28 | 1.83x | 344304 | 344288 |
| Wire4000 | Stream | 979.46 | 1619.18 | 1.65x | 280336 | 280320 |
| Wire8000 | String | 4359.35 | 7249.16 | 1.66x | 25641629 | 18219616 |
| Wire8000 | TextReader | 2256.59 | 4059.16 | 1.80x | 688304 | 688288 |
| Wire8000 | Stream | 1920.08 | 3180.07 | 1.66x | 560336 | 560320 |
| Text2K | String | 1.18 | 2.09 | 1.76x | 4264 | 4256 |
| Text2K | TextReader | 2.15 | 11.78 | 5.47x | 4472 | 4456 |
| Text2K | Stream | 2.72 | 8.31 | 3.06x | 8624 | 8608 |
| Raw64K | String | 16.19 | 46.50 | 2.87x | 65712 | 66848 |
| Raw64K | TextReader | 22.39 | 149.76 | 6.69x | 65920 | 67048 |
| Raw64K | Stream | 6.04 | 123.76 | 20.48x | 65952 | 67080 |
| Recovery | String | 1.05 | 3.19 | 3.04x | 952 | 824 |
| Recovery | TextReader | 1.60 | 2.53 | 1.58x | 1032 | 984 |
| Recovery | Stream | 1.83 | 2.85 | 1.56x | 1160 | 1112 |

## Remaining allocation limit

Whole-string Fix at 8,000 fields still allocates about 24.45 MiB per call because its arena exceeds the 65,536-entry recycling limit. This explains why the current 8,000-field measurement does not simply double the 4,000-field time. Streaming does not retain the full history and remains around 0.53–0.66 MiB allocated per 8,000 fields. Reflection confirms that Fix retains its arena at 4,000 fields (capacity 65,536) and does not retain it at 8,000 fields. Changing the retention policy is separate work.

## Validation

- 8,388 core tests passed, including four new cases covering direct recovery roots, recovery reached through an owning rule, prior iterations, guard rejection and outer alternative rollback. Inputs cover strings, one-character readers and one-byte streams.
- 3,835 Finance tests passed on the rebuilt Finance and Examples assemblies.
- Compatibility build passed, including the C# 8 emission floor.
- Generator, Finance, Examples and test builds completed without warnings or errors; git diff --check passed.

Current generated parser source: Fix 573,705 bytes; Fix44 59,431,063 bytes. Counts exclude shared model code, reports, attributes and stale part files.

## Evidence and reproduction

[`fix-materialization-2026-09-17.json`](../../benchmarks/results/fix-materialization-2026-09-17.json) contains individual samples and DLL hashes. Scratch harness: `.work/fix-comparison/probe/Program.cs`. For before/after runs set FIX_SAME_PARSER=1 and FIX_WORKLOADS=Wire4,Wire4000,Wire8000,Recovery, then supply finance-before.dll and the current Finance DLL. For current parser comparisons clear those variables and supply current Finance and Examples DLLs. Repeat each invocation with reverse.

This follows [the initial Fix/Fix44 comparison](fix-comparison-2026-09-17.md).
