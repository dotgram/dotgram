# Dedicated binary fields instead of general units, 2026-09-16

## Change and result

The parser directly yields FixField again. The generated ordinary-field choice
contains no binary fields or their paired length tags. Sixteen handwritten
alternatives match a length tag and the literal data tag together, yielding one
typed binary field. Vendor binary pairs have their own alternative. Guards and
mutable byte-limit state are confined to binary parsing and unknown/vendor
classification; known ordinary fields do not pass through Unit classification.
There is no FixFieldUnit, intermediate unit array, or flattening step.

Binary Position covers the pair, while ValuePosition and Length describe its
payload. A longer prefix is set only for binary fields; ordinary Locate retains
its original two assignments and field instances need no extra storage. The
explicit message API reconstructs separate wire length nodes when requested.

Compared in the same run with the Unit-based assembly at 2512de2:

- Short orders: byte time 10.753 -> 9.180 us, reader 14.807 -> 12.606 us,
  string 18.097 -> 16.611 us (approximately 15%, 15%, 8% less time).
- Group mean times fall 3-5%; allocation falls 11-14% across input forms.
- Raw 64 KiB payload: stream means rise about 4%, within considerable noise;
  string rises 49.478 -> 57.515 us (16%). Raw allocations rise by 0.4-0.7 KiB.
  This is not a uniform performance win.

This does not establish an improvement over the earlier pre-Unit grammar:
that assembly is not the baseline of this run. Large materialization methods
still produce GRAM5003; their partitioning is outside this change.

## Validation and method

3596 Finance tests pass. Both net10.0 and netstandard2.0 Release builds pass.
Tests cover binary delimiters, zero length, truncated and mismatched pairs,
stream buffer boundaries, source locations and the explicit semantic API.

The same-process harness uses compiled public API delegates, prepares input
outside timing, and fully consumes reader/stream results. Semantic message
building is not timed. All 18 benchmark cases completed. Before timing it checks
186 message fixtures and the three workloads in all input forms.

Output intentionally changes for binary pairs: the previous assembly returns
separate length and data fields, while the candidate returns one data field.
With DOTGRAM_FIX_COMBINED_BINARY=1 the correctness projection removes paired
length fields and the binary Position property, whose meaning changed to the
start of the whole pair. It still compares case names, payload values, validity,
ValuePosition and Length. Ordinary fields are compared without normalization.
The timing path does not normalize results. Thus the raw benchmark measures the
new one-result contract; ordinary order and group workloads retain their output.

Assembly SHA-256:

- Baseline: `4893E157141C428D303F6A96216AEE8BCEDD0B11403949B30A0E0ABDE1EAE819`
- Candidate: `0057862731FF6C2DE3B7BCD4BABE3308B89C8644DA0CC90A0652CDEFE20CCF4D`

ShortRun uses three warmups and three measurements. Error is the half-width of
the 99.9% confidence interval; small timing differences should not be treated as
established improvements. Previous/Simplified are the reusable harness labels.

## Reproduction

Save the Unit-based assembly separately, build the candidate and benchmark project
in Release, then run:

```powershell
$env:DOTGRAM_FIX_BASELINE = 'T:/TEMP/fix-units-baseline/DotGram.Finance.dll'
$env:DOTGRAM_FIX_FIXTURES = 'P:/dotgram.WorkTrees/finance/tests/DotGram.Finance.Tests/Fixtures.json'
$env:DOTGRAM_FIX_COMBINED_BINARY = '1'
dotnet run --project benchmarks/DotGram.Finance.Benchmarks -c Release --no-build -- --filter '*FixGrammarComparisonBenchmarks*' --job short --inProcess
```

## Full report

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.400
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Job=ShortRun  Toolchain=InProcessEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method     | Workload | Input      | Mean          | Error          | StdDev      | Op/s       | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|----------- |--------- |----------- |--------------:|---------------:|------------:|-----------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| **Previous**   | **Groups**   | **Bytes**      |  **7,333.431 μs** |    **921.1326 μs** |  **50.4904 μs** |     **136.36** |  **1.00** |    **0.01** | **15.6250** |       **-** |       **-** |  **780.2 KB** |        **1.00** |
| Simplified | Groups   | Bytes      |  7,018.168 μs |  1,179.4593 μs |  64.6501 μs |     142.49 |  0.96 |    0.01 |  7.8125 |       - |       - | 686.11 KB |        0.88 |
|            |          |            |               |                |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Groups**   | **Characters** | **15,153.610 μs** |    **987.2902 μs** |  **54.1167 μs** |      **65.99** |  **1.00** |    **0.00** | **15.6250** |       **-** |       **-** | **877.76 KB** |        **1.00** |
| Simplified | Groups   | Characters | 14,384.394 μs |    979.6926 μs |  53.7003 μs |      69.52 |  0.95 |    0.00 | 15.6250 |       - |       - | 783.64 KB |        0.89 |
|            |          |            |               |                |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Groups**   | **String**     | **25,289.926 μs** | **16,409.6387 μs** | **899.4677 μs** |      **39.54** |  **1.00** |    **0.04** |       **-** |       **-** |       **-** | **329.58 KB** |        **1.00** |
| Simplified | Groups   | String     | 24,473.528 μs |    593.5293 μs |  32.5333 μs |      40.86 |  0.97 |    0.03 |       - |       - |       - |  282.5 KB |        0.86 |
|            |          |            |               |                |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Order**    | **Bytes**      |     **10.753 μs** |      **0.3294 μs** |   **0.0181 μs** |  **92,994.80** |  **1.00** |    **0.00** |  **0.1373** |       **-** |       **-** |   **7.29 KB** |        **1.00** |
| Simplified | Order    | Bytes      |      9.180 μs |      3.8805 μs |   0.2127 μs | 108,933.74 |  0.85 |    0.02 |  0.1373 |       - |       - |   6.79 KB |        0.93 |
|            |          |            |               |                |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Order**    | **Characters** |     **14.807 μs** |      **3.0964 μs** |   **0.1697 μs** |  **67,536.35** |  **1.00** |    **0.01** |  **0.2136** |       **-** |       **-** |  **11.12 KB** |        **1.00** |
| Simplified | Order    | Characters |     12.606 μs |      2.4153 μs |   0.1324 μs |  79,327.28 |  0.85 |    0.01 |  0.2136 |       - |       - |  10.62 KB |        0.96 |
|            |          |            |               |                |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Order**    | **String**     |     **18.097 μs** |      **1.6938 μs** |   **0.0928 μs** |  **55,256.81** |  **1.00** |    **0.01** |  **0.0305** |       **-** |       **-** |    **1.7 KB** |        **1.00** |
| Simplified | Order    | String     |     16.611 μs |      1.9005 μs |   0.1042 μs |  60,202.39 |  0.92 |    0.01 |       - |       - |       - |   1.44 KB |        0.84 |
|            |          |            |               |                |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Raw**      | **Bytes**      |    **180.946 μs** |    **151.1948 μs** |   **8.2875 μs** |   **5,526.51** |  **1.00** |    **0.06** | **10.9863** |  **7.3242** |  **7.3242** | **319.08 KB** |        **1.00** |
| Simplified | Raw      | Bytes      |    187.724 μs |     80.8782 μs |   4.4332 μs |   5,326.97 |  1.04 |    0.05 | 10.9863 |  7.5684 |  7.3242 | 319.51 KB |        1.00 |
|            |          |            |               |                |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Raw**      | **Characters** |    **248.864 μs** |    **194.1143 μs** |  **10.6401 μs** |   **4,018.26** |  **1.00** |    **0.05** | **20.9961** | **17.5781** | **17.5781** | **570.94 KB** |        **1.00** |
| Simplified | Raw      | Characters |    259.407 μs |    126.5294 μs |   6.9355 μs |   3,854.94 |  1.04 |    0.05 | 20.9961 | 17.5781 | 17.5781 | 571.41 KB |        1.00 |
|            |          |            |               |                |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Raw**      | **String**     |     **49.478 μs** |      **6.1688 μs** |   **0.3381 μs** |  **20,211.12** |  **1.00** |    **0.01** |  **1.2817** |       **-** |       **-** |  **65.33 KB** |        **1.00** |
| Simplified | Raw      | String     |     57.515 μs |      3.1132 μs |   0.1706 μs |  17,386.83 |  1.16 |    0.01 |  1.3428 |       - |       - |  66.01 KB |        1.01 |
