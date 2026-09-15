# FIX 4.4 parsing benchmarks

Each operation parses one complete message in Strict mode and returns its typed
model. Input generation is outside the measured operation. Setup parses every
input first, so invalid workloads fail before measurement.

```powershell
dotnet run -c Release --project benchmarks/DotGram.Finance.Benchmarks -- --filter '*Fix44Benchmarks*'
```

The report includes allocations and operations per second (messages per second).
Workloads cover a heartbeat, NewOrderSingle, 64 KiB raw data and a market-data
snapshot with 1,000 repeating group entries. For comparisons, run both variants
in the same BenchmarkDotNet invocation on the same machine.

## Measurement: 2026-09-15

BenchmarkDotNet 0.15.8; Windows 11; AMD Ryzen 9 9950X3D; .NET 10.0.12;
SDK 10.0.400. ShortRun, InProcessEmitToolchain, 1 launch, 3 warmups,
3 measurement iterations. This is a local short run, not a deployment SLA or
a comparison against another FIX engine. Error is the half-width of the
99.9% confidence interval; the group workload needs a longer run for a tight bound.

| Workload | Mean | Error | Messages/sec | Allocated/message |
| --- | ---: | ---: | ---: | ---: |
| Heartbeat | 1.383 us | 0.0896 us | 723,178 | 1.61 KiB |
| NewOrderSingle | 3.088 us | 0.3628 us | 323,798 | 2.05 KiB |
| 64 KiB raw data | 36.929 us | 2.9981 us | 27,079 | 1.47 KiB |
| 1,000 group entries | 466.291 us | 53.6967 us | 2,145 | 205.06 KiB |

The measured command appended `--job short --inProcess`. Raw data is represented
by source extents, so its payload does not allocate another 64 KiB string.
Group allocations include the returned entry objects and their field arrays.
