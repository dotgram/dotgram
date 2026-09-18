# Handwritten versus generated FIX: remaining gap — 2026-09-17

## Scope and method

Revision 3e9709de, after both quadratic-walk fixes. The handwritten parser and Fix share the field model, options and primitive conversions. They recognize fields independently. Differential setup checks types, values and locations; error wording is deliberately independent. These are field-parser tests, not message-validation benchmarks.

Main table: two fresh-process Release net10.0 quick comparisons, 150 ms warmup per implementation, nine alternating 50 ms samples, average of process medians. Readers and streams are created and fully enumerated inside the calls. The existing harness creates both a StringReader and MemoryStream for each streaming call, consistently for both implementations. Managed allocation bytes are not retained or peak memory. No builds or tests ran concurrently; no confidence intervals are claimed.

## Current Fix versus handwritten

All times are microseconds per operation. A ratio above one means the handwritten implementation is faster.

| Case | Input | Fix us | Hand us | Ratio | Fix B/op | Hand B/op |
|---|---|---:|---:|---:|---:|---:|
| One | Text | 0.235 | 0.082 | 2.87 | 184 | 304 |
| One | Bytes | 0.312 | 0.081 | 3.85 | 520 | 336 |
| One | Reader | 0.371 | 0.362 | 1.02 | 552 | 8744 |
| One | Stream | 0.362 | 0.240 | 1.51 | 584 | 4680 |
| Order | Text | 2.967 | 1.095 | 2.71 | 1448 | 1568 |
| Order | Bytes | 3.800 | 0.833 | 4.56 | 1896 | 1712 |
| Order | Reader | 4.204 | 1.450 | 2.90 | 1704 | 9896 |
| Order | Stream | 3.883 | 1.127 | 3.45 | 1848 | 5944 |
| Binary64 | Text | 0.381 | 0.116 | 3.27 | 248 | 368 |
| Binary64 | Bytes | 0.480 | 0.104 | 4.62 | 552 | 368 |
| Binary64 | Reader | 0.546 | 0.391 | 1.40 | 616 | 8808 |
| Binary64 | Stream | 0.504 | 0.260 | 1.94 | 616 | 4712 |
| Binary4096 | Text | 1.562 | 1.196 | 1.31 | 4280 | 4400 |
| Binary4096 | Bytes | 0.906 | 0.330 | 2.75 | 4584 | 4400 |
| Binary4096 | Reader | 1.823 | 2.170 | 0.84 | 4648 | 29248 |
| Binary4096 | Stream | 0.841 | 0.887 | 0.95 | 4648 | 16960 |
| BinaryMany | Text | 16.548 | 3.292 | 5.03 | 6744 | 6864 |
| BinaryMany | Bytes | 21.962 | 3.445 | 6.38 | 7048 | 6864 |
| BinaryMany | Reader | 22.432 | 4.950 | 4.53 | 6608 | 14800 |
| BinaryMany | Stream | 22.197 | 4.476 | 4.96 | 6608 | 10704 |
| Orders128 | Text | 362.601 | 121.800 | 2.98 | 174168 | 174288 |
| Orders128 | Bytes | 458.154 | 96.707 | 4.74 | 192904 | 192720 |
| Orders128 | Reader | 501.668 | 206.873 | 2.42 | 159184 | 167376 |
| Orders128 | Stream | 458.843 | 154.821 | 2.96 | 177616 | 181712 |
| Recovery | Text | 2.951 | 5.455 | 0.54 | 8712 | 8624 |
| Recovery | Bytes | 8.555 | 5.899 | 1.45 | 4944 | 4552 |
| Recovery | Reader | 12.869 | 7.901 | 1.63 | 9072 | 33464 |
| Recovery | Stream | 7.973 | 6.293 | 1.27 | 5000 | 17104 |

The earlier 462x gap on 128 orders described the version before the quadratic-walk fixes. The current gap is approximately 3x on the same string workload. BinaryMany is 64 length/data pairs, each with three payload bytes including SOH; it stresses per-field overhead rather than payload copying.

The generated parser wins long-space recovery on strings (2.95 versus 5.46 us), and is competitive or faster for a 4096-byte payload through reader/stream APIs. Its pooled streaming buffers allocate much less: one order through TextReader allocates 1704 versus 9896 bytes; Stream 1848 versus 5944. The handwritten reader allocates a fresh buffer and compacts it after each field. Buffer policy improvements alone cannot explain or close the CPU gap on short fields.

Three fresh-process first calls on one string field: generated median 7.248 ms, handwritten 5.045 ms. These include initialization and JIT, but exclude process startup. Warm one-field parsing is 0.235 versus 0.082 us; cold and warm costs are different questions.

## Why Fix is slower

dotTrace Sampling with ThreadTime, 10-second Order/Text loops, separate processes: generated completed 3,287,000 calls, handwritten 9,284,000. Percentages below use the generated harness total of 9813 sampled CPU ms; inlining and sampling limit granularity.

- Recognizer exclusive time: 3172 ms (32.3%). This includes real scanning as well as maintaining calls, choices, captures, completion states and dispatch. It is not all removable overhead.
- Materializer exclusive time: 2656 ms (27.1%). Even a scalar tag reaches a typed table through recorded captures and a separate walk.
- Parser.Reset inclusive time: 563 ms (5.7%), including clearing retained arrays.
- Shared FixFactory.Value inclusive time: 2359 ms (24.0%). In the handwritten profile it takes 5172/9938 ms (52.0%); the handwritten parser spends a much larger fraction doing required conversion/construction. Inclusive times must not be added to their descendants.

The handwritten implementation accumulates tag/length integers and holds field positions in locals, selects the branch directly, and constructs the field. The generated implementation first recognizes digit spans, records calls/captures, materializes scalar values for switch/when, and later walks the accepted records to build the field. Quadratic traversal is gone, but the constant work per field remains. Binary fields need tag, length and data-tag handling, amplifying this cost.

The byte-array API has another independent cost: FixParser.Parse(byte[]) wraps the array in MemoryStream and goes through the buffered streaming iterator. HandFixParser directly wraps ReadOnlyMemory<byte>. A memory-backed generated byte input would avoid that adapter and buffer copy. This does not require changing actual stream behavior.

## Controlled grammar experiment

Only in scratch: Tag and Size retain raw span captures, and FixConvert.Tag is called at their use sites. This bypasses mid-parse typed-scalar materialization. It deliberately repeats some pure FIX conversion calls, so it is evidence about overhead, not a general transformation safe for arbitrary user factories. Production sources are unchanged.

All 6417 Finance tests passed with the experimental assembly, including differential cases, binary boundaries, custom tags and recovery. Two paired processes with reversed order, 1.5-second warmups and nine alternating samples compare current and experimental assemblies directly. Full field serialization matches on the timed valid fixtures.

| Case | API | Current us | Experiment us | Reduction |
|---|---|---:|---:|---:|
| One | String | 0.235 | 0.182 | 22.6% |
| One | TextReader | 0.323 | 0.280 | 13.3% |
| One | Stream | 0.314 | 0.276 | 12.1% |
| Order | String | 2.867 | 2.253 | 21.4% |
| Order | TextReader | 4.028 | 3.310 | 17.8% |
| Order | Stream | 3.750 | 3.050 | 18.7% |
| BinaryMany | String | 16.513 | 10.475 | 36.6% |
| BinaryMany | TextReader | 22.348 | 16.815 | 24.8% |
| BinaryMany | Stream | 20.881 | 15.268 | 26.9% |

Allocations are unchanged. The experiment establishes a 21% reduction for Order/string and 37% for BinaryMany/string. It does not close the remaining gap to hand code. The generated source shrinks from 573705 to 521646 bytes, while the handwritten reader source is 9959 bytes (all exclude shared model/conversion code).

## Fix44 has an additional negative-prefix problem

A separate three-parser comparison uses the same messages and validates field equality. Two fresh processes reverse warmup order and rotate sample order.

| Case | Input | Fix us | Hand us | Fix44 us |
|---|---|---:|---:|---:|
| One | Text | 0.255 | 0.092 | 0.693 |
| One | Stream | 0.366 | 0.230 | 0.497 |
| Order | Text | 2.909 | 1.058 | 9.299 |
| Order | Stream | 3.858 | 1.091 | 5.852 |
| BinaryMany | Text | 16.631 | 3.469 | 517.329 |
| BinaryMany | Stream | 21.503 | 4.668 | 238.261 |

Fix44 tries KnownField before KnownBinary. KnownField has 880 literal tags, including 62 starting with 9, but not length tag 95. Full-prefix tables already exist: Machine.PrefixTables.cs deliberately returns to the original alternative chain when the lookup misses, preserving diagnostics. Thus 95= takes the fallback digit-9 group and repeatedly rejects ordinary-field alternatives before reaching KnownBinary.

A separate Fix44 BinaryMany/Text dotTrace profile confirms this: recognizer parts 1–4, containing the ordinary digit-9 alternatives, have 6999 ms exclusive time versus 9719 ms inclusive for recognition (about 72%; 71% of the roughly ten-second run). Materialization is only 266 ms exclusive. This particular gap is not mainly the binary block loop or typed Position rule.

## Implementation priorities

1. **Fast negative prefix decisions.** Extend PrefixTables so a proven pure-prefix miss can fail without replaying all alternatives. Preserve furthest failure position, expected literals and EOF behavior; simply removing fallback would change diagnostics. This targets Fix44 and potentially other large literal alternatives. Reordering KnownBinary first is a grammar experiment, not a demonstrated universal solution: ordinary fields can then pay failed binary alternatives.
2. **Local scalar values for switch/when.** For suitable nonrecursive leaf rules, keep capture positions and typed values in locals, materializing at exactly the existing demand point and caching once per accepted derivation. Preserve factory execution timing/count, rollback invalidation and reentrancy. Fall back to arena storage when these proofs fail. Do not generalize the experimental repeated FixConvert.Tag calls to arbitrary C# factories.
3. **Direct code for deterministic subrules within a machine.** Extend the existing flat/direct eligibility analysis to let a field reader use locals while the outer loop keeps recovery/backtracking. Streaming should use the concrete buffered input implementation, without a new virtual input interface. Keep deferred factories where required; compact capture records can replace general parser history without invoking user code prematurely. Globally enabling Direct is insufficient: Fix explicitly disables it and DirectReachable currently rejects recovery. Immediate construction everywhere is not semantics-neutral.
4. **Direct byte-memory input.** Generate byte-span/memory entry points so an already resident byte array does not masquerade as a stream. This is independent of item 2, and the current byte-array gap is larger than the string gap.
5. **Reduce live history and clearing.** Keep only required compact records; clear only live reference ranges. Do not begin by enlarging all retained buffers: it addresses the previous 8000-field allocation cliff but neither the 3x CPU gap below that cliff nor tiny-parser startup.

The evidence supports these directions, not a guaranteed parity factor. Local scalar handling alone leaves roughly a twofold Order/string gap. Approaching hand code requires removing per-field record/dispatch/materialization work as well, while retaining generic semantics. Keep benchmarks for one-field cold/warm calls, string/byte memory, streams, many short binary pairs, long payloads, recovery and inputs crossing the arena recycling limit. Revalidate URL, SQL and ExpressionLanguage when changing shared lowering decisions.

## Evidence

- [Raw results, paired samples, hashes and profiler summaries](../../benchmarks/results/hand-fix-gap-2026-09-17.json).
- [Scratch-only grammar experiment patch](../../benchmarks/results/hand-fix-raw-captures-experiment.patch).
- CPU snapshots and full reports: `.work/hand-fix-analysis/{generated-order,hand-order,fix44-binary}.{dtp,xml}`.
- Existing benchmark command: `dotnet run -c Release --project benchmarks/DotGram.Finance.Benchmarks -- --hand-fix-performance`.
- All experimental builds and harnesses are under `.work/hand-fix-analysis`; no production parser or generator changes are part of this analysis.
