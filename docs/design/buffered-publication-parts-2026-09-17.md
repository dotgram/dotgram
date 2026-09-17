# Buffered publication boundaries: FIX runtime follow-up

## Result

The initial shared-machine implementation reduced FIX source size by 32%, but
slowed character streaming. Isolating the entry chains of separate buffered
publications removes that regression on the measured FIX workload. Small machines
still return from PlanParts before this additional partitioning is considered.

The existing size-based partitioning remains. For a divided buffered machine with
multiple published rule entries, add boundaries at those entries. Existing
Departing logic handles cross-part jumps. This avoids putting the second
publication's entire recovery loop into the first publication's final method.
FIX gains one recognizer part per shared buffered machine, with no new runtime
object or change to input buffers, public methods or materialization.

## Evidence and rejected experiments

Keeping the original owner's rule order did not remove the regression and was
reverted. Disabling tiered compilation reduced the regression to about 6%, but did
not remove it. Sampling profiles pointed to the final recognizer part, which grew
from approximately 77 KB to 83 KB of C# when it acquired a second recovery loop.
Sampling runs had fixed duration and unequal operation counts; their raw own times
are localization evidence, not per-operation speed ratios.

Restricting extra boundaries to positions with zero crossing jumps changed no FIX
output. Allowing the existing departure mechanism to handle crossings created the
needed boundary. The final generated source contains 48,646,501 characters versus
48,645,051 for initial sharing and 71,561,901 before sharing: the 32% reduction is
preserved. An isolated Release compiler run took 120.21 seconds, allocated 19.804 GB
cumulatively and produced a 12,624,384-byte assembly. This one run does not establish
a compiler-speed difference from the 114.77-second initial sharing measurement.

## Runtime comparison

Two fresh processes, opposite assembly load orders, nine rotating measurement
rounds after 1.5 seconds of warmup per variant/workload. Release generated code and
Release Finance model, default tiered compilation. No concurrent builds or tests.
Numbers in each cell are process 1 / process 2 medians, in microseconds.

| Input and reader | Before sharing | Initial sharing | Separate entry parts |
| --- | ---: | ---: | ---: |
| 4 fields, characters | 3.308 / 3.556 | 4.148 / 4.124 | 3.277 / 3.251 |
| 4 fields, bytes | 2.485 / 2.627 | 2.529 / 2.615 | 2.224 / 2.202 |
| 4,000 fields, characters | 3,157.69 / 3,230.29 | 3,977.96 / 3,971.47 | 3,088.14 / 3,114.62 |
| 4,000 fields, bytes | 2,367.97 / 2,502.57 | 2,383.94 / 2,479.51 | 2,068.22 / 2,092.00 |

The character-stream regression is absent in both runs. Byte streaming improves
by approximately 11-16% against the unshared baseline. Allocation is unchanged:
640/608 bytes for four fields and 344,296/280,328 bytes for 4,000 fields, respectively
for character/byte inputs. Cross-run timing variation remains visible; these are
results for this fixture, not guarantees for all grammars.

The input repeats `55=ABC<SOH>38=100<SOH>54=1<SOH>44=12.50<SOH>` once or 1,000
times. Each operation creates its reader and enumerates all fields; input encoding
is outside measurement. Validation checks the returned field count. Semantic
recovery and backtracking are covered separately by tests. Cold startup and other
FIX field mixes were not measured in this follow-up.

Raw data: [buffered-publication-parts-2026-09-17.json](../../benchmarks/results/buffered-publication-parts-2026-09-17.json).

## Correctness validation

- Release build succeeded with no warnings or errors.
- All 59 BufferedInput, Snapshot and Oversize tests passed.
- All 3,813 Finance tests passed against the rebuilt example assembly.
- The standalone generator profiling application was rebuilt.
- git diff --check passed.
