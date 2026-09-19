Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.3, 31.7, 31.1, 31.5, 31.0).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 10:38

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/conditions1000 | generated | 1548168.8 | 1240312.5 | 0.80x | 1212887.5 | 0.78x | -2.2% | 424424 | 424424 |
| el/floor | generated | 314.7 | 963.3 | 3.06x | 916.5 | 2.91x | -4.9% | 1104 | 1129 |
| el/floor | immediate | 314.7 | 475.2 | 1.51x | 489.3 | 1.55x | +3.0% | 1120 | 1120 |
| el/ladder | generated | 1012.8 | 2137.1 | 2.11x | 1916.7 | 1.89x | -10.3% | 1776 | 1776 |
| el/ladder | immediate | 1012.8 | 1164.3 | 1.15x | 1170.7 | 1.16x | +0.5% | 1784 | 1784 |
| el/nest7 | generated | 640.6 | 1928.8 | 3.01x | 1778.4 | 2.78x | -7.8% | 1152 | 1152 |
| el/nest7 | immediate | 640.6 | 1039.4 | 1.62x | 1052.4 | 1.64x | +1.2% | 1120 | 1120 |
| el/block | generated | 1044.7 | 2108.8 | 2.02x | 2025.5 | 1.94x | -3.9% | 2344 | 2344 |
| el/block | immediate | 1044.7 | 1153.1 | 1.10x | 1160.0 | 1.11x | +0.6% | 2440 | 2440 |
| el/loop | generated | 2245.9 | 4430.4 | 1.97x | 4097.9 | 1.82x | -7.5% | 4776 | 4776 |
| el/loop | immediate | 2245.9 | 2591.2 | 1.15x | 2583.7 | 1.15x | -0.3% | 5200 | 5200 |
| el/terms100 | generated | 11510.9 | 20998.4 | 1.82x | 16841.1 | 1.46x | -19.8% | 17904 | 17904 |
| el/terms100 | immediate | 11510.9 | 12734.8 | 1.11x | 12684.1 | 1.10x | -0.4% | 18720 | 18720 |
| el/terms1000 | generated | 112153.6 | 201787.1 | 1.80x | 159386.6 | 1.42x | -21.0% | 169128 | 169128 |
| el/terms1000 | immediate | 112153.6 | 121413.6 | 1.08x | 120971.7 | 1.08x | -0.4% | 177120 | 177120 |
| el/overloads | generated | 1835.1 | 2826.5 | 1.54x | 2772.9 | 1.51x | -1.9% | 4248 | 4248 |
| el/overloads | immediate | 1835.1 | 2012.2 | 1.10x | 2025.5 | 1.10x | +0.7% | 4240 | 4240 |
| el/string | generated | 273.3 | 1049.4 | 3.84x | 1045.1 | 3.82x | -0.4% | 1056 | 1056 |
| el/string | immediate | 273.3 | 664.7 | 2.43x | 664.9 | 2.43x | 0.0% | 1032 | 1032 |
| el/interpolation | generated | 1766.1 | 4830.0 | 2.73x | 5026.2 | 2.85x | +4.1% | 2368 | 2368 |
| el/interpolation | immediate | 1766.1 | 4153.0 | 2.35x | 4130.1 | 2.34x | -0.6% | 2424 | 2424 |
| el/untyped | generated | 28440.4 | 31696.0 | 1.11x | 30805.6 | 1.08x | -2.8% | 16424 | 16424 |
| el/refused-early | generated | 473.0 | 1552.2 | 3.28x | 1475.9 | 3.12x | -4.9% | 1064 | 1064 |
| el/refused-early | immediate | 473.0 | 1222.3 | 2.58x | 1211.9 | 2.56x | -0.9% | 1944 | 1944 |
| el/refused-late | generated | 1491.5 | 2258.7 | 1.51x | 2163.8 | 1.45x | -4.2% | 1064 | 1064 |
| el/refused-late | immediate | 1491.5 | 3120.0 | 2.09x | 3153.4 | 2.11x | +1.1% | 3928 | 3928 |
| sql/conditions100 | generated | 69298.6 | 446835.2 | 6.45x | 433094.1 | 6.25x | -3.1% | 161736 | 161736 |
| sql/conditions1000 | generated | 673563.3 | 4559369.5 | 6.77x | 4346130.5 | 6.45x | -4.7% | 1616136 | 1616203 |
| sql/select20 | generated | 9541.1 | 80994.7 | 8.49x | 75542.3 | 7.92x | -6.7% | 21472 | 21472 |
