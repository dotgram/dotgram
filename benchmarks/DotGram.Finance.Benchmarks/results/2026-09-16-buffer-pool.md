# Reuse buffered parser storage, 2026-09-16

## Allocation profile

Baseline: `dea6d7b`, after prefix-table dispatch and construction partitioning.
A runtime GC allocation trace of 1000000 one-field byte inputs attributes 91.4%
of sampled allocation volume to `System.Byte[]`. The buffer constructor and its
inlined caller dominate those samples. The trace includes process startup and
50000 warmups; sampled percentages are approximate, not exact object counts.
The existing unprofiled harness measures exact thread allocation after warmup.

## Implementation

The generated buffered character and byte readers rent storage from
`ArrayPool<T>.Shared`. Ordinary parse, find and yield publications own the lease
with `using`, including iterator completion, exceptions and early disposal.
Growth rents another buffer, copies the retained input and returns the old one.
All returned arrays are cleared. The source Stream/TextReader stays caller-owned.

The logical capacity remains separate from the pool bucket length, so a larger
rental does not increase block-read size or bypass `maxRetained`. Existing input
positions, capture copying, failure behavior and public signatures are unchanged.
There is no FIX-specific fast path and no extra pool for parser objects.

Pooling shifts buffer storage from per-enumeration garbage to reusable shared
storage. Warm allocation savings do not mean the process retains no buffers:
the shared pool manages retention and trimming. Cold rentals can allocate, and
clearing returned arrays still costs CPU. Dispose an iterator when stopping early.

## Validation

Tests inject an oversized reusable tracking pool into generated C# 8 code and
check full parsing, find/yield completion, early and repeated disposal, an iterator
that is never started, overlapping live enumerators, growth, exact retention
limits, malformed input and throwing readers. They verify balanced leases,
clearing requests and that the input remains open. Existing Finance tests cover
binary payloads, every standard field and semantic APIs.

All checks passed: core tests 8195/8195, Finance tests 3596/3596; Release C# 8
compatibility builds for net472, netstandard2.0 and net8.0; Finance builds for
netstandard2.0 and net10.0. Builds report zero warnings/errors.

## Before/after measurements

Release .NET 10.0.12, Windows x64. The existing `profile` harness was run in three
fresh processes per assembly/workload, alternating order between rounds. Every
operation fully enumerates the fields. Assembly loading and binding are excluded;
input-wrapper construction is included. No builds, tests or profilers ran alongside
timings. Order/tag workloads use 500000 operations after 50000 warmups; Groups uses
500 after 100. These are diagnostic loops, not confidence intervals.

Time is microseconds per input, median (min-max). Order contains 15 fields;
Groups contains 3011 fields.

| Input | Workload | Before, us | After, us | B/op before -> after | Allocation reduction |
| --- | --- | ---: | ---: | ---: | ---: |
| Bytes | Order | 3.284 (3.250-3.297) | 3.176 (3.170-3.191) | 5712 -> 1592 | 72.1% |
| Bytes | Groups | 609.965 (609.866-616.714) | 626.522 (626.413-628.857) | 173472 -> 169352 | 2.4% |
| Bytes | Tag1 | 0.685 (0.677-0.694) | 0.657 (0.642-0.663) | 4552 -> 432 | 90.5% |
| Bytes | Tag198 | 0.690 (0.675-0.697) | 0.650 (0.632-0.671) | 4552 -> 432 | 90.5% |
| Characters | Order | 4.773 (4.661-4.920) | 4.594 (4.385-4.624) | 9632 -> 1416 | 85.3% |
| String | Order | 5.580 (5.562-6.304) | 5.686 (5.543-6.853) | 1376 -> 1376 | 0.0% |

The byte paths save exactly 4120 B/op, a 4096-byte array plus its header. The
character path saves 8216 B/op, a 4096-character array plus its header. A separate
warm `GC.GetAllocatedBytesForCurrentThread` probe over 100000 input constructors
measures 64 B/op for MemoryStream and 32 B/op for StringReader (preallocated data,
`GC.KeepAlive` to prevent elimination). Thus input wrappers are a small part of
the baseline; their allocations remain included in the table.

Short byte orders improve about 3.3%, Tag1 about 4.1%, Tag198 about 5.9%, and
character orders about 3.7% by medians. Group time regresses 2.7%, while its
allocation falls only 2.4%: one buffer is amortized over thousands of fields.
This is a memory optimization with a measured long-input throughput tradeoff.
The string control does not use the changed buffer; its time varies across runs
and its allocation is unchanged. Do not treat its small median delta as a robust
parser effect.

Gen0 collections per 500000 byte orders fall from 56 to 15; per 500000 Tag1 inputs,
from 45 to 4; per 500000 character orders, from 95 to 14.

The second allocation trace samples 450 MiB versus 4579 MiB before. Byte arrays
fall from 91.4% to 3.4% of sampled volume; remaining samples mostly come from
iterators, input wrappers, buffer-owner objects and field values. Both traces
include startup, so remaining byte-array volume is not steady per-input buffer
allocation. Pool retention is not included in B/op.

Raw artifacts:

- [All timing and allocation runs](2026-09-16-buffer-pool-runs.txt)
- [Before allocation samples](2026-09-16-buffer-pool-before.txt)
- [After allocation samples](2026-09-16-buffer-pool-after.txt)

## Reproduction

```powershell
$app = './benchmarks/DotGram.Finance.Benchmarks/bin/Release/net10.0/DotGram.Finance.Benchmarks.exe'
& $app profile <saved-finance.dll> Bytes Tag1 500000
& $app profile <saved-finance.dll> Bytes Tag198 500000
& $app profile <saved-finance.dll> Bytes Order 500000
& $app profile <saved-finance.dll> Bytes Groups 500
& $app profile <saved-finance.dll> Characters Order 500000
& $app profile <saved-finance.dll> String Order 500000

dotnet-trace collect --profile gc-verbose -o allocations.nettrace -- $app profile <saved-finance.dll> Bytes Tag1 1000000
```

Allocation traces use dotnet-trace 10.0.745401; the GCAllocationTick summarizer is
documented in `.claude/rules/profiling.md`. Exact per-operation allocation comes
from the unprofiled harness, not from adding sampled object counts.

SHA-256 of measured Finance assemblies:

- Before: `A8C279BBAD3210247EAA9633E9D7A91797DD619D2E42FFC51CE7051CE80A33F0`
- After: `AE48FB0CE24F98623C0858D6C706417EEDEDD27C8443B3E5BE1E14EFC790390B`
