# FIX runtime profiling across parser versions, 2026-09-16

## Findings

There are two distinct bottlenecks:

1. Literal alternatives sharing the first digit still backtrack sequentially. This
   makes later tags much more expensive, in both the old and current parser.
2. Flattening field rules enlarged the materialization helper. On a short order,
   materialization became about twice as expensive, outweighing cheaper recognition.
   This explains why the old parser was faster on that workload.

The Unit-based intermediate version added another regression. Removing Unit
recovered part of it, but did not fix either underlying generator issue.
No parser or compiler optimization was made in this profiling change.

## Versions and workload

| Label | Parser source | Description | Assembly SHA-256 |
| --- | --- | --- | --- |
| named | 910c40f | Separate named field rules | 9253A3E86644EB743E54FC2234C03979FC95D69672EC37C0A03058942C5479F6 |
| flat | b18b78b | One KnownField alternative list | 6EAFDC62459D3CA301F651F7F3B015F9D12901A87A3BFE47054245595551CB38 |
| units | 2512de2 | General Unit wrapper and paired fields | 4893E157141C428D303F6A96216AEE8BCEDD0B11403949B30A0E0ABDE1EAE819 |
| current | dd9c582 | Dedicated binary pairs, no Unit or vendor registration | 9EE8AF9433D63C0F3B10277665E8E52C52DDD75CFC4A64460E8E24DA3D6F87C3 |

Every version loads in a fresh process through the same AssemblyLoadContext path
and compiled delegate. The input is a fixed 15-field NewOrderSingle, read through
a MemoryStream and fully enumerated. Inputs and binding are outside timing;
stream construction is inside timing. No message semantic API is called.
All four versions produce 15 fields per operation; this input has no binary pairs,
so the changed binary-result contract does not affect this comparison.

## Unprofiled timing

.NET 10.0.12, Release, Windows 11, Ryzen 9 9950X3D. Three fresh-process runs per
version, 500,000 messages per run, after 50,000 warmup messages. Version order
alternates between rounds. No builds, other benchmark jobs, or profiler sessions
ran concurrently with these measurements. These are diagnostic loop measurements,
not a new BenchmarkDotNet confidence interval.

| Version | Median us/message | Range us/message | Allocated B/message |
| --- | ---: | ---: | ---: |
| named | 6.528 | 6.301-6.662 | 6968 |
| flat | 7.929 | 7.888-8.163 | 6968 |
| units | 10.072 | 10.023-10.506 | 7464 |
| current | 8.456 | 8.438-8.839 | 6944 |

Current is about 30% slower than named on this short-message workload. Allocation
is nearly identical (24 bytes less per current operation); both had 69 gen0 and
zero gen1/gen2 collections in each measured unprofiled run. Extra allocation/GC
therefore does not explain this regression. Units allocates another 496 bytes
relative to named and is the slowest version.

## dotTrace sampling: the same 2 million short messages

JetBrains dotTrace 2026.2.0.1, Sampling, ThreadTime. Collection starts through
MeasureProfiler.StartCollectingData after loading, warmup and an explicit GC;
it stops before printing results. Each profile covers 30 million returned fields.
Default runtime tiering is retained. Startup, assembly loading and initial JIT
compilation are excluded from the measured region; later runtime work can still
occur. Sampling times and inlining attribution are approximate.

| Version | Measured loop CPU, s | Recognition own, s | Materialization own, s | Materialization including children, s |
| --- | ---: | ---: | ---: | ---: |
| named | 13.219 | 5.015 | 3.047 | 4.313 |
| flat | 15.828 | 4.171 | 6.641 | 8.281 |
| units | 20.453 | 6.470 | 7.969 | 9.656 |
| current | 16.953 | 4.347 | 7.375 | 8.750 |

Own columns sum non-overlapping self times of methods whose names contain
Recognize_DotGram or Materialize_DotGram. The inclusive column is the total time
of the materialization entry point, including its children; do not add it to the
own columns. The recognition entry point itself calls materialization, so its
inclusive time must not be interpreted as recognition-only time.

The named -> flat transition saves 0.844 s of recognition self time but adds
3.594 s of materialization self time. Current recognition is still cheaper than
named; the materialization subtree grows from about 33% to 52% of measured CPU.

Current hot functions include:

- Materialization Part1: 6.031 s own, 7.219 s inclusive.
- Materialization entry point: 1.063 s own, 8.750 s inclusive.
- Recognition entry point: 1.438 s own (its inclusive time includes materialization).
- ParserArena.Add: 1.063 s own.

The old hot materialization Part10 takes 1.953 s own and 2.906 s inclusive.
This supports the earlier materialization diagnosis with a real CPU profile.

## Native code evidence

A separate JIT-disassembly run used TieredCompilation=0 to obtain stable FullOpts
code. This setting was not used for timings or profiles. Both methods explicitly
report optimized code; GRAM5003 is not proof of disabled JIT optimization.

| Executed materialization helper | named Part10 | current Part1 |
| --- | ---: | ---: |
| Stack frame reservation | 7456 B (0x1D20) | 26760 B (0x6888) |
| Main stack-zeroing loop | 6144 B (0x1800) | 26688 B (0x6840) |
| Native method size | 35932 B | 149297 B |

The prologue reserves/probes the frame and clears it on every invocation. The old
helper reports 122 single-block inlinees; the current helper retains calls to
ParserArena.get_Item and similar accessors. The enlarged frame, native code and
changed inlining are concrete costs consistent with the hotspot. This is not an
ablation: it does not assign exact percentages to zeroing, inlining and code size.

## Sequential tag choice is also a real bottleneck

Two unprofiled runs of 1 million one-field inputs after warmup, same byte-stream
path. Tags 1, 100 and 198 all construct string values, using identical payload X.
The table shows the mean of the two runs, in microseconds per input.

| Input | named | current |
| --- | ---: | ---: |
| 1=X | 0.552 | 0.820 |
| 100=X | 0.935 | 1.153 |
| 198=X | 4.021 | 3.698 |

Tag 198 costs about 4.5 times tag 1 in current, and about 7.3 times in named.
This problem predates the flattened grammar. The generated recognition code
selects the first digit, but then saves choice points and tests full literals
sequentially within that digit's alternatives; it does not build a complete trie.

Separate current-version dotTrace profiles, each for 5 million one-field inputs:

| Input | Total sampled own CPU, s | Recognition own, s | Arena own, s | Buffered input own, s | Materialization inclusive, s |
| --- | ---: | ---: | ---: | ---: | ---: |
| 1=X | 3.565 | 0.625 | 0.235 | 0.094 | 1.469 |
| 198=X | 19.504 | 9.954 | 2.610 | 3.094 | 1.484 |

Materialization is effectively constant here. Almost all the difference lies in
recognition, parser-entry bookkeeping and repeated buffered-input access. That
confirms the cost of sequential alternatives, independently of materialization.

## Why long groups gave a different result

Sampling profiles of 2,000 market-data inputs, each with 1,000 group entries and
3,011 fields, cover 6,022,000 fields. The group entries repeatedly use tags
269, 270 and 271, rather than the short order's distribution of tags.

| Version | Total sampled own CPU, s | Recognition own, s | Arena own, s | Materialization inclusive, s |
| --- | ---: | ---: | ---: | ---: |
| named | 16.225 | 8.704 | 3.657 | 0.844 |
| current | 14.661 | 7.671 | 2.000 | 1.500 |

Here recognition/bookkeeping dominate. Savings there exceed the added
materialization cost. This explains why prior measurements showed improved group
throughput alongside slower short orders; the old parser was not universally faster.

## Next optimizations supported by the evidence

- Generate a complete literal-prefix decision tree, preserving ordered-choice and
  failure behavior. It should avoid repeated literal checks and failed-choice
  records for distinguishable tag prefixes. This does not require Lexical mode.
- Split large construction dispatches inside a single rule, so a short field does
  not enter a helper containing every field's construction temporaries. Preserve
  the readable flat grammar instead of requiring authors to split field rules.
- Reprofile both short orders and late tags after each change. Do not infer success
  from DLL size, the absence of a warning, or one workload alone.

## Artifacts and reproduction

[Full flat function exports](2026-09-16-profile-functions.csv) retain own/inclusive
milliseconds and sample counts for all eight snapshots. CallTreeInstances is a
count of nodes in the exported call tree, not a count of runtime invocations.
The snapshots, original XML, logs and JIT listings are under T:/TEMP/fix-profile.

Build the benchmark project in Release, then run the fixed-work harness without
profiling (it prints the assembly hash, throughput, allocation and collection counts):

```powershell
DotGram.Finance.Benchmarks.exe profile <assembly-path> Bytes Order 500000
```

Other workloads: Groups, Tag1, Tag100, Tag198. Other inputs: Characters, String.

For a sampling snapshot, provide the installed profiler API path to the harness:

```powershell
$env:DOTGRAM_DOTTRACE_API = "$env:LOCALAPPDATA/JetBrains/Installations/dotTrace262/JetBrains.Profiler.Api.dll"
& "$env:LOCALAPPDATA/JetBrains/Installations/dotTrace262/dottrace.exe" start --profiling-type=Sampling --time-measurement=ThreadTime --use-api --save-to=T:/TEMP/fix-profile/current.dtp --overwrite --no-check-for-updates <absolute-apphost-path> profile <assembly-path> Bytes Order 2000000
```

Remove DOTGRAM_DOTTRACE_API for unprofiled runs. Reporter exports were generated
with a pattern file containing `<Patterns><Pattern>.*</Pattern></Patterns>` and
`--save-signature`. No external profiling package is required to build the harness;
it loads the optional local API through reflection outside the measured loop.
