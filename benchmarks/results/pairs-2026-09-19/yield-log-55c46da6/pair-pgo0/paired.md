Median of 5 of 5 runs, each in a process of its own; control 30.6 ns (the runs' controls: 30.5, 31.1, 30.6, 30.8, 30.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 21:01

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 110.6 | 121.4 | 1.10x | 124.5 | 1.13x | +2.6% | 192 | 192 |
| fix/One.bytes | generated | 116.5 | 207.5 | 1.78x | 208.0 | 1.78x | +0.2% | 248 | 248 |
| fix/Order.text | generated | 1000.4 | 1059.2 | 1.06x | 1058.8 | 1.06x | 0.0% | 1096 | 1096 |
| fix/Order.bytes | generated | 1032.5 | 1998.5 | 1.94x | 1997.7 | 1.93x | 0.0% | 1152 | 1152 |
| fix/Order.stream | generated | 1141.9 | 2370.6 | 2.08x | 2380.4 | 2.08x | +0.4% | 1712 | 1712 |
| fix/BinaryMany.text | generated | 5593.0 | 5058.0 | 0.90x | 5100.7 | 0.91x | +0.8% | 7256 | 7256 |
| fix/Orders128.text | generated | 122790.2 | 126377.5 | 1.03x | 127676.4 | 1.04x | +1.0% | 129112 | 129112 |
| fix/slope-16.text | generated | 1455.0 | 1290.2 | 0.89x | 1313.6 | 0.90x | +1.8% | 1752 | 1752 |
| fix/slope-16.bytes | generated | 1559.4 | 2345.5 | 1.50x | 2340.2 | 1.50x | -0.2% | 1808 | 1808 |
| tsql/columns1000 | generated | 2688896.9 | 233575.0 | 0.09x | 227618.8 | 0.08x | -2.6% | 200464 | 200464 |
| fix/One.log-text | generated | 120.3 | 227.3 | 1.89x | 218.9 | 1.82x | -3.7% | 192 | 192 |
| fix/One.log-bytes | generated | 120.3 | 337.2 | 2.80x | 328.2 | 2.73x | -2.7% | 248 | 248 |
| fix/One.log-stream | generated | 121.1 | 651.7 | 5.38x | 647.4 | 5.35x | -0.7% | 888 | 888 |
| fix/Order.log-text | generated | 1063.2 | 2012.3 | 1.89x | 1848.6 | 1.74x | -8.1% | 1096 | 1096 |
| fix/Order.log-bytes | generated | 1055.5 | 3234.1 | 3.06x | 3116.1 | 2.95x | -3.6% | 1152 | 1152 |
| fix/Order.log-stream | generated | 1058.4 | 4186.9 | 3.96x | 3995.0 | 3.77x | -4.6% | 1712 | 1712 |
| fix/Orders128.log-text | generated | 128430.3 | 236348.3 | 1.84x | 221584.5 | 1.73x | -6.2% | 129112 | 129112 |
| fix/Orders128.log-bytes | generated | 129935.2 | 397469.1 | 3.06x | 376415.6 | 2.90x | -5.3% | 129168 | 129168 |
| fix/Orders128.log-stream | generated | 129897.0 | 489892.6 | 3.77x | 463287.4 | 3.57x | -5.4% | 118552 | 118552 |
| fix/slope-0.log-text | generated | 32.7 | 85.3 | 2.60x | 84.6 | 2.58x | -0.8% | 88 | 88 |
| fix/slope-0.log-bytes | generated | 32.9 | 100.1 | 3.05x | 98.6 | 3.00x | -1.5% | 144 | 144 |
| fix/slope-0.log-stream | generated | 32.6 | 271.4 | 8.34x | 274.7 | 8.44x | +1.2% | 792 | 792 |
| fix/slope-4.log-text | generated | 376.1 | 678.7 | 1.80x | 638.3 | 1.70x | -5.9% | 504 | 504 |
| fix/slope-4.log-bytes | generated | 376.1 | 1074.7 | 2.86x | 1030.4 | 2.74x | -4.1% | 560 | 560 |
| fix/slope-4.log-stream | generated | 373.8 | 1543.2 | 4.13x | 1473.5 | 3.94x | -4.5% | 1176 | 1176 |
| fix/slope-16.log-text | generated | 1407.5 | 2425.9 | 1.72x | 2239.2 | 1.59x | -7.7% | 1752 | 1752 |
| fix/slope-16.log-bytes | generated | 1403.0 | 3904.4 | 2.78x | 3731.8 | 2.66x | -4.4% | 1808 | 1808 |
| fix/slope-16.log-stream | generated | 1411.4 | 4938.2 | 3.50x | 4749.4 | 3.36x | -3.8% | 2328 | 2328 |
| el/ladder.bool | generated | 1425.0 | 2477.8 | 1.74x | 2485.1 | 1.74x | +0.3% | 1328 | 1272 |
| sql/select20.bool | generated | 8055.1 | 22061.4 | 2.74x | 22113.7 | 2.75x | +0.2% | 21448 | 21392 |
| web/url.full | generated | 281.7 | 348.3 | 1.24x | 353.9 | 1.26x | +1.6% | 536 | 536 |
| el/ladder | generated | 1420.0 | 2487.0 | 1.75x | 2489.4 | 1.75x | +0.1% | 1328 | 1328 |
| el/ladder | immediate | 1420.0 | 1841.4 | 1.30x | 1838.9 | 1.30x | -0.1% | 1208 | 1208 |
| el/string | generated | 372.1 | 1075.1 | 2.89x | 1081.7 | 2.91x | +0.6% | 1024 | 1024 |
| el/string | immediate | 372.1 | 751.0 | 2.02x | 757.2 | 2.04x | +0.8% | 1000 | 1000 |
| sql/column | generated | 165.6 | 435.8 | 2.63x | 429.7 | 2.59x | -1.4% | 392 | 392 |
| sql/select20 | generated | 8114.0 | 22220.0 | 2.74x | 22272.0 | 2.74x | +0.2% | 21448 | 21448 |
