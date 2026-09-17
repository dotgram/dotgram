# Effect of main's delimiter optimizations on shared parsers

## Scope

Compare the performance branch at `0459ed97` with merged `a47cf61c`. Both contain
publication sharing and buffered entry partitioning. Main adds guarded character
runs, linear padded-delimiter scanning, linear recovery synchronization, a nested
construction correction and updated FIX APIs/grammar.

Two comparisons answer different questions:

1. **Actual pre-merge parser versus merged parser:** load the previously compiled
   `fix-roots.dll` and the merged parser in separate assembly contexts. Both use the
   same current Release Finance model. Call their generated ParseFields/ReadFields
   and log counterparts with freshly created contexts, because their outer public
   FIX APIs differ. Stream forwarding wrappers are excluded from both versions.
   Log input is compact and supported by both grammars. This measures parser cores,
   not historical binaries with different Finance models or public wrapper overhead.
2. **Generator effect on identical current grammar:** run both generator assemblies
   over the current sources and compile both outputs with the same Release Roslyn
   configuration and current Finance reference. This also measures padded logs with
   the older generator, a useful counterfactual rather than our pre-merge grammar.
   Public FIX wrappers and full stream enumeration are included in this comparison.

.NET 10.0.12, default tiered compilation. Two fresh processes per comparison,
opposite assembly load orders, 1.5 seconds of warmup per case/implementation and
nine rotating measurement rounds. Inputs and encoded bytes are prepared outside
measurement; reader construction is included. Allocations are measured separately
over three warmed operations. No probe builds/tests overlap runtime measurements.
Absolute times vary substantially between processes; use within-process ratios.
These are diagnostic medians, not BenchmarkDotNet confidence intervals. Cold
startup was not measured.

All 27 cases in each of four runtime passes compare serialized field values,
locations and error messages before timing and validate result counts during timing.
All comparisons succeeded. The merged source had already passed 121 focused
parser tests and 3,835 Finance tests during synchronization; no production code was
changed for this investigation.

## Actual pre-merge parser comparison

The ranges below are speedup ratios from the two processes. Greater than 1 is faster.

| Workload | String | Character stream | Byte stream |
| --- | ---: | ---: | ---: |
| Four ordinary SOH fields | 1.48-1.49x | 1.60-1.62x | 1.25-1.38x |
| 4,000 ordinary SOH fields | 1.39x | 1.60-1.74x | 1.32-1.37x |
| One 2 KiB SOH text field | 62.6-74.1x | 12.7-13.3x | 18.1-18.3x |
| Four compact log fields | 1.69-1.71x | 1.67-1.83x | 1.33-1.59x |
| 2.2 KiB log text | 53.3-68.0x | 11.2-12.4x | 15.1-17.6x |
| Log text with 1,024 internal spaces | 54.5-69.5x | 11.5-12.1x | 15.0-16.7x |
| Recovery across 4,096 spaces | 55.3-56.4x | 3.10-3.54x | 3.96-4.24x |

First process absolute examples:

| Input / form | Before | After |
| --- | ---: | ---: |
| Four fields, characters | 3.18 us | 1.98 us |
| Four fields, bytes | 2.17 us | 1.57 us |
| 4,000 fields, string | 458.71 ms | 330.32 ms |
| 4,000 fields, characters | 2.995 ms | 1.875 ms |
| 4,000 fields, bytes | 2.037 ms | 1.484 ms |

The string path still takes hundreds of milliseconds for 4,000 fields, while the
streaming paths take a few milliseconds. Delimiter scanning does not remove that
remaining scaling problem; it is a separate optimization target.

Allocation per parse:

- 4,000 SOH fields through the string entry: approximately 15,364,373 -> 376,080
  bytes, about 97.6% less. Allocation reduction is much larger than time reduction.
- The corresponding character/byte stream entries remain at 344,208 / 280,240 bytes.
- Four fields remain at 456 / 552 / 520 bytes for string/character/byte entries.
- Recovery across 4,096 spaces falls modestly: 8,672 -> 8,584 bytes for string,
  8,872 -> 8,696 for characters, 4,832 -> 4,656 for bytes.

The public-wrapper comparison has an additional 80 bytes per stream call in both
versions; do not mix its allocation rows with the internal-entry comparison.

## Why the counterfactual gains are larger

On the current padded grammar, the older generator repeatedly retries the entire
space-prefixed separator at every input position. Long internal space runs and
recovery become quadratic. The new generator scans once and preserves the ordinary
fallback for exact failure behavior. The first paired process measured up to 454x
for a text field containing 1,024 spaces and up to 1,838x for recovery across 4,096
spaces. These figures are **not** gains against our actual pre-merge simple-pipe
parser; use the preceding table for that comparison.

Short messages improve too: the character-at-a-time lookahead machinery disappears
from common text recognition. This is not exclusively a large-input optimization.

## Generator, source size and compiler cost

Two fresh generator processes per version, forward/reverse order, identical current
sources and metadata references. Mean results:

| Project | Before generation | After generation | Generated characters |
| --- | ---: | ---: | ---: |
| FIX44 | 4.733 s | 4.932 s | 54,664,641 -> 59,441,690 |
| SQL | 15.147 s | 14.900 s | 39,682,426, identical |
| ExpressionLanguage | 2.122 s | 2.185 s | 3,467,006, identical |
| TinyScalar | 608 ms | 620 ms | 30,854, identical |
| TinyStreams | 626 ms | 621 ms | 76,417, identical |

FIX generation is about 4.2% slower and cumulative allocations rise from 3.808 GB
to 4.151 GB, about 9.0%. The other timing differences do not establish improvements
or regressions with this small sample. Tiny timings include fresh Roslyn/generator
initialization; they are not tiny-parser call timings.

SQL, ExpressionLanguage and both tiny outputs match by per-file SHA-256. Their
runtime code is unchanged; no new runtime speedup is claimed for those parsers.

For the same current FIX grammar, source grows by 8.7%. The output retains four
buffered materializers: publication sharing is still active. Run-scan sites increase
from 3,918 to 10,966, including 3,536 emitted linear delimiter scans. The fast paths
retain fallback code for minimum-length diagnostics, explaining part of the cost.

The larger log-separator grammar also contributes separately to source size. With
current host/model metadata but the original simple-pipe grammar, the old generator
emits 48,781,981 characters and the new one 52,964,391. On current padded grammar,
the corresponding counts are 54,664,641 and 59,441,690. Thus the combined grammar
and generator changes grow this controlled source inventory by about 21.9%.
The archived pre-merge measurement was 48,646,501 characters with its earlier
metadata context; do not treat that slight difference as a generator regression.

One isolated Release Roslyn compilation per version, same current grammar:

| Metric | Old generator output | Merged generator output |
| --- | ---: | ---: |
| Parse, bind and emit | 150.46 s | 190.79 s |
| Cumulative allocated bytes | 22.685 GB | 25.458 GB |
| DLL bytes | 14,364,672 | 15,725,056 |

Compiler time rose 26.8% in this pair, allocations 12.2%, and DLL size 9.5%.
Time needs repeated confirmation before extrapolation to a whole Visual Studio
build. Cumulative allocation is not peak memory. These are Release compilation
measurements, not solution-build timings.

## Conclusion

The merge improves measured FIX parsing substantially, including short inputs,
without changing emitted SQL/Expression/tiny code. It increases generator/compiler
work and FIX source size. The next focused candidates are sharing repeated cold
fallback code and investigating the still-expensive large string parsing
path. Neither is implemented by this measurement change.

[Raw runs](../../benchmarks/results/main-impact-2026-09-17.json) retain every sample,
allocation count, generator stage and the generator assembly hashes. Scratch probes
are in `.work/main-impact/`; comparison builds are `fix-main-before.dll` and
`fix-main-after.dll` under `.work/sharing-validation/`.

## Follow-up

The large-string bottleneck was subsequently localized to quadratic recovery choice
deactivation and fixed; see [the measured follow-up](recovery-choice-scan-2026-09-17.md).
