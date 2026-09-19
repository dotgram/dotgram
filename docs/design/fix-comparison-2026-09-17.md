# FixParser versus Fix44 — 2026-09-17

Current Release net10.0 assemblies, including the pending recovery-choice scan fix. No parser changes were made for this comparison.

## Method

Two fresh processes with reversed parser order, 1.5 seconds of warmup per case and nine alternating timed batches. Values below average the two process medians. Input construction and Latin-1 encoding are outside timing; creating readers/streams and enumerating results are included. Allocations are managed bytes per operation, not retained or peak memory. This measures warmed parsing, not cold startup.

Both implementations produce matching concrete field types, values, positions and lengths on these fixtures. Invalid-field diagnostic messages differ (grammar names and reported failure positions); only Message is excluded from recovery equivalence. The recovery case contains two invalid fields. Raw64K is a length/data pair with 65,536 X bytes, returning one binary field. Wire4 is four fields; Wire4000 and Wire8000 repeat them. These are field-parser workloads, not whole-message conformance tests.

## Results

All times are microseconds per operation.

| Input | API | Fix us | Fix44 us | Fix44 / Fix | Fix allocated B | Fix44 allocated B |
|---|---|---:|---:|---:|---:|---:|
| Wire4 (28 chars) | String | 0.97 | 2.54 | 2.63 | 464 | 456 |
| Wire4 (28 chars) | TextReader | 1.18 | 2.03 | 1.73 | 648 | 632 |
| Wire4 (28 chars) | Stream | 1.08 | 1.57 | 1.45 | 616 | 600 |
| Wire4000 (28000 chars) | String | 143902.92 | 2416.29 | 0.02 | 376088 | 376080 |
| Wire4000 (28000 chars) | TextReader | 1116.00 | 1957.00 | 1.75 | 344304 | 344288 |
| Wire4000 (28000 chars) | Stream | 1006.29 | 1504.64 | 1.50 | 280336 | 280320 |
| Wire8000 (56000 chars) | String | 613635.08 | 7211.20 | 0.01 | 25641624 | 18219616 |
| Wire8000 (56000 chars) | TextReader | 2165.80 | 3864.47 | 1.78 | 688304 | 688288 |
| Wire8000 (56000 chars) | Stream | 1971.89 | 2943.27 | 1.49 | 560336 | 560320 |
| Text2K (2052 chars) | String | 1.20 | 2.01 | 1.67 | 4264 | 4256 |
| Text2K (2052 chars) | TextReader | 2.00 | 11.27 | 5.65 | 4472 | 4456 |
| Text2K (2052 chars) | Stream | 2.68 | 8.27 | 3.09 | 8624 | 8608 |
| Raw64K (65549 chars) | String | 15.45 | 42.44 | 2.75 | 65712 | 66848 |
| Raw64K (65549 chars) | TextReader | 21.02 | 146.54 | 6.97 | 65920 | 67048 |
| Raw64K (65549 chars) | Stream | 5.42 | 122.08 | 22.54 | 65952 | 67080 |
| Recovery (32 chars) | String | 1.12 | 3.03 | 2.70 | 952 | 824 |
| Recovery (32 chars) | TextReader | 1.50 | 2.39 | 1.59 | 1032 | 984 |
| Recovery (32 chars) | Stream | 1.47 | 1.96 | 1.34 | 1160 | 1112 |

## Findings

- Fix wins small-input and streaming cases. The byte-stream 64 KiB binary case is about 23 times faster.
- Fix still has quadratic whole-string behavior: doubling 4,000 fields to 8,000 roughly quadruples time. Fix44 is much faster on these whole-string cases after the preceding recovery scan fix.
- The generated Fix switch must materialize the typed Tag before calling context.Kind(tag). Machine.cs initializes guardFrom to zero when recovery plans exist. Materialization consequently revisits accumulated history for each field, including already-built entries. The recovery traversal itself also scans the history. This is a separate issue from the fixed recovery choice deactivation loop. Streaming yields one field at a time and bounds that history. This diagnosis is based on emitted-code inspection and scaling, not a new profiler capture.
- Next optimization: bound typed switch/guard materialization and recovery traversal to their dependencies while preserving owner/recovery/backtracking semantics. Merely replacing guardFrom = 0 is insufficient because the recovery scan remains.
- Ordinary field allocations are effectively equal. At 8,000 fields the whole-string allocation jumps to 24.45 MiB for Fix and 17.38 MiB for Fix44; the parser recycling policy retains arenas only through capacity 65,536. Streaming stays around 0.53–0.66 MiB of allocations for 8,000 fields.

## Generated source size

The current generated grammar files are 573,249 bytes (0.547 MiB) for Fix and 59,429,415 bytes (56.676 MiB) for Fix44: 103.7 times larger. This excludes the shared financial field model, attributes and reports. Old part files remain in obj and must not be included in this inventory. Source size is not assembly size or JIT memory.

## Reproduction

Scratch harness: `.work/fix-comparison/probe/Program.cs`. Run the Release executable with the Finance DLL and Examples DLL paths, then repeat with `reverse`. JSON evidence, assembly hashes and individual samples: [`fix-comparison-2026-09-17.json`](../../benchmarks/results/fix-comparison-2026-09-17.json).

## Follow-up

The whole-string materialization issue identified here is addressed in [Bound recovery-aware materialization](fix-materialization-2026-09-17.md), with new paired measurements. The numbers above describe the version before that fix.
