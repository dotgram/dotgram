```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-MLTFGY : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

InvocationCount=1  IterationCount=1  LaunchCount=10  
RunStrategy=ColdStart  UnrollFactor=1  WarmupCount=0  

```
| Method                                 | Mean     | Error     | StdDev    | Median   | Op/s  | Allocated |
|--------------------------------------- |---------:|----------:|----------:|---------:|------:|----------:|
| &#39;ours: LoadDictionary, cold&#39;           | 15.62 ms |  0.328 ms |  0.217 ms | 15.61 ms | 64.03 |   1.08 MB |
| &#39;QuickFIX/n: new DataDictionary, cold&#39; | 26.21 ms | 31.436 ms | 20.793 ms | 19.64 ms | 38.16 |   4.85 MB |
