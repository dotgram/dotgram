# FIX parser comparison with recovery, 2026-09-16

## Method

Both parsers come from the same Release assembly at commit `dff879e`, on
Windows x64 / .NET 10.0.12 (SDK 10.0.400). Assembly SHA256:
`4F2517A906AFC1F9C8D0B556B29C281008E0FDFC2BBA29D0B5E6AF2B919F9FED`.

Three fresh processes per parser and case, reversing parser order in round two.
Short cases use 50000 warmups and 500000 measured operations. Groups uses 100
warmups and 1000 operations; Raw uses 100 warmups and 10000 operations. Each
operation includes the public Parse call, full enumeration, and reader/stream
disposal. Input strings and byte arrays are prepared outside the timed region.
No builds or tests were started alongside measurements by this task.

Results are medians of three process measurements, not confidence intervals.
These are warm microbenchmarks, excluding cold startup and network I/O.

## Results

| Input / workload | Fix44, us/op | FixDispatch, us/op | Fix44 / FixDispatch | B/op, Fix44 / FixDispatch |
| --- | ---: | ---: | ---: | ---: |
| Bytes / Groups | 1648.746 | 632.841 | 2.61x | 169352 / 169360 |
| Bytes / Order | 11.388 | 3.587 | 3.17x | 1592 / 1600 |
| Bytes / Raw | 59.329 | 104.415 | 0.57x | 67944 / 66808 |
| Bytes / Recovery | 2.809 | 1.373 | 2.05x | 1480 / 1488 |
| Bytes / Tag1 | 1.060 | 0.797 | 1.33x | 432 / 440 |
| Bytes / Tag198 | 0.947 | 0.651 | 1.46x | 432 / 440 |
| Characters / Order | 14.710 | 4.067 | 3.62x | 1416 / 1424 |
| Characters / Recovery | 3.571 | 1.448 | 2.47x | 1384 / 1392 |
| String / Order | 23.357 | 4.333 | 5.39x | 1288 / 1296 |
| String / Recovery | 4.898 | 0.895 | 5.47x | 1008 / 1016 |

Order contains 15 fields; Groups contains 3011 fields. Raw has 11 results,
including one 65536-byte binary value combined with its length field. Tag1 and
Tag198 each contain one text field. Recovery parses
`55=ABC<SOH>bad<SOH>38=2<SOH>0=X<SOH>55=END<SOH>tail`:
six results, including three Invalid fields and a malformed final field at EOF.

FixDispatch wins nine of ten measured cases. Ordinary orders are 3.17x faster
on bytes, 3.62x on characters, and 5.39x on strings. Recovery is 2.05x to 5.47x
faster. Ordinary/recovery allocation differs by only 8 bytes per operation.

The exception is large binary data: FixDispatch takes 1.76x as long as Fix44,
although it allocates 1136 fewer bytes. This is consistent across all three
processes: Fix44 57.862-60.038 us, FixDispatch 101.177-104.688 us. One-field
measurements are noisier (Tag1: FixDispatch 0.624-0.817 us).

The previous report predates recovery and has lower absolute timings. It was
not rerun in this experiment, so that historical comparison does not isolate
recovery cost from other changes or machine conditions.

## Decision

Prefer FixDispatch for ordinary field feeds on the measured workloads. Keep
Fix44 as a reference and for comparison while investigating the large-binary
regression before removing either implementation. These results do not prove
that FixDispatch is faster for every production workload.

## Reproduction and validation

Build `benchmarks/DotGram.Finance.Benchmarks` in Release, then run:

```powershell
$runner = 'benchmarks/DotGram.Finance.Benchmarks/bin/Release/net10.0/DotGram.Finance.Benchmarks.exe'
$assembly = (Resolve-Path 'src/DotGram.Finance/bin/Release/net10.0/DotGram.Finance.dll').Path
& $runner profile $assembly Bytes Order 500000 Fix44
& $runner profile $assembly Bytes Order 500000 FixDispatch
```

Repeat in three rounds, swapping parser order in round two. Use the input,
workload, and iteration counts recorded in the [raw results](2026-09-16-recovery-dispatch-raw.txt).
`Recovery` is the only added benchmark workload; parser code is unchanged.
Each measured run verifies its result count. The measured parser revision
previously passed 8218 core tests and 3807 Finance tests, including recovery
positions/raw data and parity of both implementations.
