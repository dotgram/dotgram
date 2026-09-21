```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```
| Method                                                | Reading      | Mean         | Error       | StdDev      | Op/s        | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated  | Alloc Ratio |
|------------------------------------------------------ |------------- |-------------:|------------:|------------:|------------:|------:|--------:|--------:|--------:|--------:|-----------:|------------:|
| **&#39;ours: message from a string&#39;**                         | **Order**        |   **1,125.0 ns** |    **17.99 ns** |    **15.95 ns** |   **888,855.5** |  **1.00** |    **0.02** |  **0.0839** |       **-** |       **-** |    **4.18 KB** |        **1.00** |
| &#39;QuickFIX/n: message from a string (their only form)&#39; | Order        |     981.2 ns |    16.03 ns |    14.21 ns | 1,019,164.8 |  0.87 |    0.02 |  0.0877 |  0.0010 |       - |     4.3 KB |        1.03 |
| &#39;ours: message from a span&#39;                           | Order        |   1,156.8 ns |    21.97 ns |    20.55 ns |   864,476.1 |  1.03 |    0.02 |  0.0896 |       - |       - |    4.47 KB |        1.07 |
| &#39;ours: message from a stream (no partner)&#39;            | Order        |   1,529.0 ns |    11.91 ns |    11.14 ns |   654,038.2 |  1.36 |    0.02 |  0.1793 |       - |       - |    8.84 KB |        2.12 |
| &#39;ours: message from a reader (no partner)&#39;            | Order        |   1,455.0 ns |    19.03 ns |    17.80 ns |   687,266.0 |  1.29 |    0.02 |  0.2575 |  0.0076 |       - |   12.64 KB |        3.02 |
| &#39;ours: message from octets (no partner)&#39;              | Order        |   1,339.8 ns |    26.08 ns |    27.90 ns |   746,353.8 |  1.19 |    0.03 |  0.0896 |       - |       - |    4.48 KB |        1.07 |
| &#39;ours: message from a span of octets (no partner)&#39;    | Order        |   1,296.9 ns |    14.65 ns |    12.99 ns |   771,063.3 |  1.15 |    0.02 |  0.0935 |       - |       - |    4.64 KB |        1.11 |
| &#39;ours: fields from a string&#39;                          | Order        |     617.1 ns |    11.10 ns |     9.84 ns | 1,620,546.9 |  0.55 |    0.01 |  0.0229 |       - |       - |    1.15 KB |        0.27 |
| &#39;ours: fields from octets (no partner)&#39;               | Order        |     790.2 ns |     8.77 ns |     8.20 ns | 1,265,466.8 |  0.70 |    0.01 |  0.0238 |       - |       - |     1.2 KB |        0.29 |
| &#39;A/A ours: message from a string&#39;                     | Order        |   1,143.5 ns |    19.47 ns |    17.26 ns |   874,518.8 |  1.02 |    0.02 |  0.0839 |       - |       - |    4.18 KB |        1.00 |
|                                                       |              |              |             |             |             |       |         |         |         |         |            |             |
| **&#39;ours: message from a string&#39;**                         | **PartiesLarge** | **472,025.1 ns** | **3,493.83 ns** | **3,268.13 ns** |     **2,118.5** |  **1.00** |    **0.01** | **37.5977** | **33.6914** | **30.2734** |  **488.77 KB** |        **1.00** |
| &#39;QuickFIX/n: message from a string (their only form)&#39; | PartiesLarge | 250,262.4 ns | 4,898.98 ns | 6,540.00 ns |     3,995.8 |  0.53 |    0.01 | 34.1797 | 14.4043 |       - | 1685.02 KB |        3.45 |
| &#39;ours: message from a span&#39;                           | PartiesLarge | 469,814.6 ns | 4,104.63 ns | 3,839.47 ns |     2,128.5 |  1.00 |    0.01 | 38.5742 | 34.6680 | 30.2734 |  537.72 KB |        1.10 |
| &#39;ours: message from a stream (no partner)&#39;            | PartiesLarge | 485,803.3 ns | 4,389.03 ns | 4,105.51 ns |     2,058.4 |  1.03 |    0.01 | 41.5039 | 37.5977 | 31.2500 |  622.44 KB |        1.27 |
| &#39;ours: message from a reader (no partner)&#39;            | PartiesLarge | 484,010.9 ns | 5,333.27 ns | 4,988.74 ns |     2,066.1 |  1.03 |    0.01 | 41.9922 | 38.5742 | 31.2500 |  657.96 KB |        1.35 |
| &#39;ours: message from octets (no partner)&#39;              | PartiesLarge | 474,882.9 ns | 2,040.48 ns | 1,908.67 ns |     2,105.8 |  1.01 |    0.01 | 38.5742 | 34.1797 | 30.2734 |  537.71 KB |        1.10 |
| &#39;ours: message from a span of octets (no partner)&#39;    | PartiesLarge | 487,037.5 ns | 3,549.15 ns | 3,319.88 ns |     2,053.2 |  1.03 |    0.01 | 40.0391 | 36.1328 | 31.2500 |  562.19 KB |        1.15 |
| &#39;ours: fields from a string&#39;                          | PartiesLarge |  97,639.9 ns |   867.07 ns |   768.64 ns |    10,241.7 |  0.21 |    0.00 |  3.7842 |  0.8545 |       - |   188.7 KB |        0.39 |
| &#39;ours: fields from octets (no partner)&#39;               | PartiesLarge | 122,968.7 ns | 1,475.84 ns | 1,308.29 ns |     8,132.2 |  0.26 |    0.00 |  3.7842 |  0.6104 |       - |  188.76 KB |        0.39 |
| &#39;A/A ours: message from a string&#39;                     | PartiesLarge | 469,601.7 ns | 4,993.17 ns | 4,670.62 ns |     2,129.5 |  0.99 |    0.01 | 37.5977 | 33.6914 | 30.2734 |  488.77 KB |        1.00 |
