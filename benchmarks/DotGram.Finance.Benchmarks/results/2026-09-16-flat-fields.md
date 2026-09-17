# Flat FIX field grammar comparison, 2026-09-16

Baseline: commit `910c40f`, one named rule per field. Candidate: one `KnownField`
choice with 912 direct construction alternatives; `Separator` consumed by the
handwritten common `Field` rule. Both assemblies were loaded in the same process
and invoked through equivalent compiled delegates. Reflection and input generation
are outside timing. String parsing returns an array; reader and byte parsing fully
enumerate the lazy field sequence. Stream/reader creation is included in timing.
No message assembly or semantic validation is timed.

Before timing, both assemblies produce identical runtime case names and serialized
public values/locations on all 186 dictionary fixtures and all three workloads,
for strings, character readers and byte streams. The candidate also passes all
3,588 Finance tests, including malformed input, pipe separators and binary data.

The candidate uses 17,325,568 bytes for the net10.0 DLL (embedded debug information
included), compared with 23,297,024 bytes for the baseline: 25.6% smaller.

SHA-256 of measured assemblies:

- Baseline: `9253A3E86644EB743E54FC2234C03979FC95D69672EC37C0A03058942C5479F6`
- Candidate: `6EAFDC62459D3CA301F651F7F3B015F9D12901A87A3BFE47054245595551CB38`

## Interpretation

The simpler grammar is not uniformly faster. Mean time for the group workload
fell 8-18%; a short order rose 4-23%, with the largest regression on byte input.
Allocations are effectively unchanged. Raw-data results show no reliable gain;
the candidate's string raw-data measurement is particularly noisy. This ShortRun
has only three measured iterations; its wide confidence intervals do not establish
small differences. Ratios compare variants within this run only.

The candidate triggers `GRAM5003` for large recognition/materialization methods.
Finance keeps this warning visible but excludes it from warnings-as-errors. The
simplified source and smaller assembly are retained with the measured tradeoff;
removing the size limitation requires general emitter partitioning work, not more
FIX-specific grammar rules. These measurements do not prove which methods the JIT
optimized, and do not attribute the timing differences solely to that warning.

## Reproduction

Before changing the grammar, build the baseline and copy its net10.0
`DotGram.Finance.dll` outside the build output directory. Then build the candidate
and benchmark project in Release, and run:

```powershell
$env:DOTGRAM_FIX_BASELINE = 'T:/TEMP/fix-grammar-baseline/DotGram.Finance.dll'
$env:DOTGRAM_FIX_FIXTURES = 'P:/dotgram.WorkTrees/finance/tests/DotGram.Finance.Tests/Fixtures.json'
dotnet run --project benchmarks/DotGram.Finance.Benchmarks -c Release --no-build -- --filter '*FixGrammarComparisonBenchmarks*' --job short --inProcess
```

## Full BenchmarkDotNet report

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.400
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Job=ShortRun  Toolchain=InProcessEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method     | Workload | Input      | Mean          | Error         | StdDev      | Op/s       | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|----------- |--------- |----------- |--------------:|--------------:|------------:|-----------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| **Previous**   | **Groups**   | **Bytes**      |  **8,227.789 μs** | **2,806.4165 μs** | **153.8292 μs** |     **121.54** |  **1.00** |    **0.02** |       **-** |       **-** |       **-** | **686.09 KB** |        **1.00** |
| Simplified | Groups   | Bytes      |  6,778.967 μs | 1,040.7528 μs |  57.0472 μs |     147.52 |  0.82 |    0.01 |  7.8125 |       - |       - | 686.12 KB |        1.00 |
|            |          |            |               |               |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Groups**   | **Characters** | **16,182.609 μs** |   **627.0697 μs** |  **34.3718 μs** |      **61.79** |  **1.00** |    **0.00** |       **-** |       **-** |       **-** | **783.62 KB** |        **1.00** |
| Simplified | Groups   | Characters | 14,647.445 μs |   945.3665 μs |  51.8187 μs |      68.27 |  0.91 |    0.00 | 15.6250 |       - |       - | 783.66 KB |        1.00 |
|            |          |            |               |               |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Groups**   | **String**     | **24,798.617 μs** | **2,422.3699 μs** | **132.7783 μs** |      **40.32** |  **1.00** |    **0.01** |       **-** |       **-** |       **-** | **282.51 KB** |        **1.00** |
| Simplified | Groups   | String     | 22,919.782 μs | 2,412.1027 μs | 132.2155 μs |      43.63 |  0.92 |    0.01 |       - |       - |       - | 282.42 KB |        1.00 |
|            |          |            |               |               |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Order**    | **Bytes**      |      **6.508 μs** |     **0.2704 μs** |   **0.0148 μs** | **153,650.64** |  **1.00** |    **0.00** |  **0.1373** |       **-** |       **-** |    **6.8 KB** |        **1.00** |
| Simplified | Order    | Bytes      |      7.982 μs |     0.7582 μs |   0.0416 μs | 125,280.30 |  1.23 |    0.01 |  0.1373 |       - |       - |    6.8 KB |        1.00 |
|            |          |            |               |               |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Order**    | **Characters** |     **11.078 μs** |     **2.6023 μs** |   **0.1426 μs** |  **90,269.53** |  **1.00** |    **0.02** |  **0.2136** |       **-** |       **-** |  **10.63 KB** |        **1.00** |
| Simplified | Order    | Characters |     12.116 μs |     0.4730 μs |   0.0259 μs |  82,533.33 |  1.09 |    0.01 |  0.2136 |       - |       - |  10.63 KB |        1.00 |
|            |          |            |               |               |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Order**    | **String**     |     **15.943 μs** |     **1.6284 μs** |   **0.0893 μs** |  **62,723.06** |  **1.00** |    **0.01** |       **-** |       **-** |       **-** |   **1.45 KB** |        **1.00** |
| Simplified | Order    | String     |     16.644 μs |     2.0716 μs |   0.1135 μs |  60,079.97 |  1.04 |    0.01 |       - |       - |       - |   1.45 KB |        1.00 |
|            |          |            |               |               |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Raw**      | **Bytes**      |    **226.436 μs** |    **11.9028 μs** |   **0.6524 μs** |   **4,416.26** |  **1.00** |    **0.00** | **10.7422** |  **7.3242** |  **7.0801** |  **318.8 KB** |        **1.00** |
| Simplified | Raw      | Bytes      |    227.096 μs |    14.1425 μs |   0.7752 μs |   4,403.42 |  1.00 |    0.00 | 10.7422 |  7.3242 |  7.0801 |  318.8 KB |        1.00 |
|            |          |            |               |               |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Raw**      | **Characters** |    **244.136 μs** |    **44.4773 μs** |   **2.4380 μs** |   **4,096.09** |  **1.00** |    **0.01** | **19.2871** | **16.1133** | **15.6250** | **570.67 KB** |        **1.00** |
| Simplified | Raw      | Characters |    236.600 μs |   138.0641 μs |   7.5678 μs |   4,226.55 |  0.97 |    0.03 | 21.9727 | 18.5547 | 18.3105 | 570.68 KB |        1.00 |
|            |          |            |               |               |             |            |       |         |         |         |         |           |             |
| **Previous**   | **Raw**      | **String**     |     **49.225 μs** |    **16.7611 μs** |   **0.9187 μs** |  **20,314.95** |  **1.00** |    **0.02** |  **1.2817** |       **-** |       **-** |  **65.14 KB** |        **1.00** |
| Simplified | Raw      | String     |     57.283 μs |   154.4060 μs |   8.4635 μs |  17,457.17 |  1.16 |    0.15 |  1.2817 |       - |       - |  65.14 KB |        1.00 |
