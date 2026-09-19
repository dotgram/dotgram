```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```
| Method  | Mean      | Error    | StdDev    | Median    | Ratio | RatioSD | Gen0      | Gen1     | Allocated | Alloc Ratio |
|-------- |----------:|---------:|----------:|----------:|------:|--------:|----------:|---------:|----------:|------------:|
| Tokens  |  66.48 ms | 1.272 ms |  2.683 ms |  65.34 ms |  0.50 |    0.10 | 6000.0000 | 500.0000 | 288.97 MB |        0.92 |
| Tree    | 137.81 ms | 8.452 ms | 24.922 ms | 148.47 ms |  1.04 |    0.27 | 6000.0000 |        - | 312.83 MB |        1.00 |
| Located |  31.25 ms | 0.498 ms |  0.573 ms |  31.04 ms |  0.23 |    0.05 |  187.5000 |        - |   9.08 MB |        0.03 |
| Grammar |  26.21 ms | 0.508 ms |  0.679 ms |  26.09 ms |  0.20 |    0.04 |  187.5000 |        - |   9.08 MB |        0.03 |
