```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```
| Method                                | From       | Reading       | Mean         | Error       | StdDev       | Median       | Op/s        | Gen0    | Gen1    | Gen2    | Allocated  |
|-------------------------------------- |----------- |-------------- |-------------:|------------:|-------------:|-------------:|------------:|--------:|--------:|--------:|-----------:|
| **&#39;ours: parse + read&#39;**                  | **Compiled**   | **Order**         |   **1,371.0 ns** |    **18.30 ns** |     **17.12 ns** |   **1,377.5 ns** |   **729,416.0** |  **0.0877** |       **-** |       **-** |     **4.3 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Compiled   | Order         |   1,159.1 ns |    22.29 ns |     22.89 ns |   1,154.6 ns |   862,758.1 |  0.0916 |       - |       - |    4.54 KB |
| &#39;ours: parse + validate + read&#39;       | Compiled   | Order         |   1,948.5 ns |    22.94 ns |     21.46 ns |   1,947.1 ns |   513,212.3 |  0.0877 |       - |       - |    4.34 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Compiled   | Order         |   2,667.3 ns |    50.60 ns |     47.33 ns |   2,670.3 ns |   374,914.6 |  0.1335 |       - |       - |    6.55 KB |
| &#39;A/A ours: parse + read&#39;              | Compiled   | Order         |   1,281.0 ns |    18.25 ns |     16.18 ns |   1,280.6 ns |   780,640.6 |  0.0877 |       - |       - |     4.3 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Compiled   | Order         |   1,228.0 ns |    53.03 ns |    150.43 ns |   1,158.9 ns |   814,300.6 |  0.0916 |       - |       - |    4.54 KB |
| **&#39;ours: parse + read&#39;**                  | **Compiled**   | **Parties**       |   **1,980.7 ns** |    **23.91 ns** |     **22.37 ns** |   **1,984.5 ns** |   **504,870.4** |  **0.1202** |       **-** |       **-** |    **5.95 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Compiled   | Parties       |   2,250.9 ns |    30.12 ns |     25.15 ns |   2,252.2 ns |   444,275.8 |  0.2136 |  0.0038 |       - |   10.63 KB |
| &#39;ours: parse + validate + read&#39;       | Compiled   | Parties       |   3,105.2 ns |    37.46 ns |     35.04 ns |   3,102.2 ns |   322,036.0 |  0.1221 |       - |       - |    6.01 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Compiled   | Parties       |   4,449.2 ns |    57.87 ns |     51.30 ns |   4,447.6 ns |   224,758.9 |  0.2899 |  0.0076 |       - |   14.21 KB |
| &#39;A/A ours: parse + read&#39;              | Compiled   | Parties       |   2,000.0 ns |    34.98 ns |     29.21 ns |   1,995.9 ns |   499,991.7 |  0.1202 |       - |       - |    5.95 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Compiled   | Parties       |   2,782.5 ns |   189.37 ns |    558.36 ns |   2,419.9 ns |   359,384.0 |  0.2136 |  0.0038 |       - |   10.63 KB |
| **&#39;ours: parse + read&#39;**                  | **Compiled**   | **PartiesMedium** |  **19,233.8 ns** |   **366.34 ns** |    **436.10 ns** |  **19,140.9 ns** |    **51,991.8** |  **1.0681** |       **-** |       **-** |   **52.95 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Compiled   | PartiesMedium |  29,414.7 ns |   526.15 ns |    562.98 ns |  29,362.8 ns |    33,996.6 |  3.9368 |  0.7629 |       - |  193.84 KB |
| &#39;ours: parse + validate + read&#39;       | Compiled   | PartiesMedium |  27,895.8 ns |   508.52 ns |    475.67 ns |  27,935.3 ns |    35,847.7 |  1.0681 |  0.0916 |       - |   53.02 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Compiled   | PartiesMedium |  48,687.6 ns |   496.65 ns |    440.27 ns |  48,662.8 ns |    20,539.1 |  4.9438 |  1.0986 |       - |  244.42 KB |
| &#39;A/A ours: parse + read&#39;              | Compiled   | PartiesMedium |  19,067.7 ns |   347.93 ns |    325.46 ns |  18,994.6 ns |    52,444.6 |  1.0681 |       - |       - |   52.95 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Compiled   | PartiesMedium |  29,241.6 ns |   475.71 ns |    444.98 ns |  29,233.3 ns |    34,197.8 |  3.9368 |  0.7629 |       - |  193.84 KB |
| **&#39;ours: parse + read&#39;**                  | **Compiled**   | **PartiesLarge**  | **473,824.1 ns** | **3,961.62 ns** |  **3,705.70 ns** | **475,537.7 ns** |     **2,110.5** | **37.5977** | **33.6914** | **30.2734** |  **488.95 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Compiled   | PartiesLarge  | 298,063.3 ns | 5,839.90 ns |  6,248.63 ns | 297,056.9 ns |     3,355.0 | 38.0859 | 16.6016 |       - | 1888.38 KB |
| &#39;ours: parse + validate + read&#39;       | Compiled   | PartiesLarge  | 426,695.9 ns | 8,368.29 ns | 16,904.36 ns | 431,151.3 ns |     2,343.6 | 37.5977 | 34.6680 | 30.2734 |  489.13 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Compiled   | PartiesLarge  | 541,154.5 ns | 9,564.77 ns |  8,478.92 ns | 541,037.1 ns |     1,847.9 | 47.8516 | 17.5781 |       - |  2374.9 KB |
| &#39;A/A ours: parse + read&#39;              | Compiled   | PartiesLarge  | 473,375.5 ns | 3,706.40 ns |  3,466.97 ns | 474,606.2 ns |     2,112.5 | 37.5977 | 33.6914 | 30.2734 |  488.91 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Compiled   | PartiesLarge  | 302,243.6 ns | 5,146.91 ns |  4,814.42 ns | 301,044.7 ns |     3,308.6 | 38.0859 | 16.6016 |       - | 1888.38 KB |
| **&#39;ours: parse + read&#39;**                  | **Compiled**   | **Binary**        |     **958.2 ns** |    **15.62 ns** |     **14.61 ns** |     **957.9 ns** | **1,043,585.1** |  **0.0572** |       **-** |       **-** |    **2.84 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Compiled   | Binary        |     690.1 ns |    12.52 ns |     11.71 ns |     687.3 ns | 1,448,995.4 |  0.0725 |       - |       - |    3.55 KB |
| &#39;ours: parse + validate + read&#39;       | Compiled   | Binary        |   1,330.4 ns |    23.23 ns |     23.85 ns |   1,319.6 ns |   751,652.9 |  0.0572 |       - |       - |    2.88 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Compiled   | Binary        |   1,747.1 ns |    18.06 ns |     16.89 ns |   1,744.5 ns |   572,379.6 |  0.1106 |       - |       - |    5.45 KB |
| &#39;A/A ours: parse + read&#39;              | Compiled   | Binary        |     962.1 ns |     8.45 ns |      7.90 ns |     961.7 ns | 1,039,407.4 |  0.0572 |       - |       - |    2.84 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Compiled   | Binary        |     695.0 ns |    12.69 ns |     11.87 ns |     697.1 ns | 1,438,899.6 |  0.0725 |       - |       - |    3.55 KB |
| **&#39;ours: parse + read&#39;**                  | **Dictionary** | **Order**         |   **1,249.1 ns** |    **20.72 ns** |     **19.38 ns** |   **1,248.6 ns** |   **800,562.4** |  **0.0877** |       **-** |       **-** |     **4.3 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Dictionary | Order         |   1,136.5 ns |    11.80 ns |     10.46 ns |   1,134.7 ns |   879,932.7 |  0.0916 |       - |       - |    4.54 KB |
| &#39;ours: parse + validate + read&#39;       | Dictionary | Order         |   1,963.1 ns |    32.42 ns |     30.33 ns |   1,957.7 ns |   509,407.1 |  0.0877 |       - |       - |    4.34 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Dictionary | Order         |   2,659.0 ns |    34.91 ns |     29.15 ns |   2,651.9 ns |   376,075.1 |  0.1335 |       - |       - |    6.55 KB |
| &#39;A/A ours: parse + read&#39;              | Dictionary | Order         |   1,252.0 ns |    22.43 ns |     20.98 ns |   1,245.3 ns |   798,734.1 |  0.0877 |       - |       - |     4.3 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Dictionary | Order         |   1,144.5 ns |    22.23 ns |     30.42 ns |   1,144.4 ns |   873,728.9 |  0.0916 |       - |       - |    4.54 KB |
| **&#39;ours: parse + read&#39;**                  | **Dictionary** | **Parties**       |   **1,966.8 ns** |    **25.85 ns** |     **24.18 ns** |   **1,966.8 ns** |   **508,429.6** |  **0.1202** |       **-** |       **-** |    **5.95 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Dictionary | Parties       |   2,245.3 ns |    36.16 ns |     32.05 ns |   2,243.9 ns |   445,379.0 |  0.2136 |  0.0038 |       - |   10.63 KB |
| &#39;ours: parse + validate + read&#39;       | Dictionary | Parties       |   2,976.9 ns |    53.81 ns |     50.33 ns |   2,975.6 ns |   335,917.6 |  0.1221 |       - |       - |    6.01 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Dictionary | Parties       |   4,464.2 ns |    79.64 ns |     74.50 ns |   4,439.8 ns |   224,002.0 |  0.2899 |  0.0076 |       - |   14.21 KB |
| &#39;A/A ours: parse + read&#39;              | Dictionary | Parties       |   1,960.6 ns |    30.04 ns |     26.63 ns |   1,966.1 ns |   510,054.8 |  0.1202 |       - |       - |    5.95 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Dictionary | Parties       |   2,244.8 ns |    42.48 ns |     47.22 ns |   2,240.1 ns |   445,476.2 |  0.2136 |  0.0038 |       - |   10.63 KB |
| **&#39;ours: parse + read&#39;**                  | **Dictionary** | **PartiesMedium** |  **19,285.9 ns** |   **225.82 ns** |    **176.31 ns** |  **19,301.2 ns** |    **51,851.2** |  **1.0681** |       **-** |       **-** |   **52.95 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Dictionary | PartiesMedium |  29,284.9 ns |   414.38 ns |    367.33 ns |  29,193.7 ns |    34,147.3 |  3.9368 |  0.7629 |       - |  193.84 KB |
| &#39;ours: parse + validate + read&#39;       | Dictionary | PartiesMedium |  27,476.4 ns |   406.80 ns |    380.52 ns |  27,322.2 ns |    36,394.8 |  1.0681 |  0.0916 |       - |   53.02 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Dictionary | PartiesMedium |  49,979.2 ns |   997.20 ns |  1,024.06 ns |  49,791.0 ns |    20,008.3 |  4.9438 |  1.0986 |       - |  244.42 KB |
| &#39;A/A ours: parse + read&#39;              | Dictionary | PartiesMedium |  19,247.2 ns |   276.94 ns |    231.26 ns |  19,216.6 ns |    51,955.6 |  1.0681 |       - |       - |   52.95 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Dictionary | PartiesMedium |  29,568.0 ns |   347.79 ns |    308.31 ns |  29,600.2 ns |    33,820.3 |  3.9368 |  0.7629 |       - |  193.84 KB |
| **&#39;ours: parse + read&#39;**                  | **Dictionary** | **PartiesLarge**  | **445,596.4 ns** | **1,337.89 ns** |  **1,186.00 ns** | **445,432.0 ns** |     **2,244.2** | **35.6445** | **33.2031** | **28.3203** |  **488.95 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Dictionary | PartiesLarge  | 286,175.7 ns | 4,376.80 ns |  3,654.83 ns | 287,152.4 ns |     3,494.4 | 38.0859 | 16.6016 |       - | 1888.38 KB |
| &#39;ours: parse + validate + read&#39;       | Dictionary | PartiesLarge  | 454,103.1 ns | 6,640.25 ns |  5,886.41 ns | 454,193.0 ns |     2,202.1 | 35.6445 | 33.2031 | 28.3203 |  489.06 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Dictionary | PartiesLarge  | 549,508.2 ns | 7,801.90 ns |  6,514.94 ns | 551,294.6 ns |     1,819.8 | 47.8516 | 17.5781 |       - |  2374.9 KB |
| &#39;A/A ours: parse + read&#39;              | Dictionary | PartiesLarge  | 446,722.5 ns | 1,453.70 ns |  1,213.90 ns | 447,169.7 ns |     2,238.5 | 35.6445 | 33.6914 | 28.3203 |  488.94 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Dictionary | PartiesLarge  | 286,218.1 ns | 4,499.38 ns |  3,988.58 ns | 285,560.5 ns |     3,493.8 | 38.0859 | 16.6016 |       - | 1888.38 KB |
| **&#39;ours: parse + read&#39;**                  | **Dictionary** | **Binary**        |     **989.7 ns** |    **10.44 ns** |      **9.76 ns** |     **985.8 ns** | **1,010,393.4** |  **0.0572** |       **-** |       **-** |    **2.84 KB** |
| &#39;QuickFIX/n: parse + read&#39;            | Dictionary | Binary        |     689.4 ns |    12.34 ns |     10.94 ns |     685.0 ns | 1,450,520.2 |  0.0725 |       - |       - |    3.55 KB |
| &#39;ours: parse + validate + read&#39;       | Dictionary | Binary        |   1,386.2 ns |    27.58 ns |     27.09 ns |   1,390.5 ns |   721,405.0 |  0.0572 |       - |       - |    2.88 KB |
| &#39;QuickFIX/n: parse + validate + read&#39; | Dictionary | Binary        |   1,739.0 ns |    20.71 ns |     18.36 ns |   1,741.2 ns |   575,050.3 |  0.1106 |       - |       - |    5.45 KB |
| &#39;A/A ours: parse + read&#39;              | Dictionary | Binary        |     965.4 ns |    14.90 ns |     12.44 ns |     963.3 ns | 1,035,881.7 |  0.0572 |       - |       - |    2.84 KB |
| &#39;A/A QuickFIX/n: parse + read&#39;        | Dictionary | Binary        |     695.3 ns |     7.60 ns |      6.74 ns |     694.1 ns | 1,438,228.5 |  0.0725 |       - |       - |    3.55 KB |
