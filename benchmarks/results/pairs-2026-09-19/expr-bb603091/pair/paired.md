Median of 5 of 5 runs, each in a process of its own; control 31.4 ns (the runs' controls: 31.4, 31.4, 31.4, 31.4, 31.6).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 17:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | 2148887.5 | 218068.8 | 0.10x | 219387.5 | 0.10x | +0.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | 886710.9 | 409602.3 | 0.46x | 410590.6 | 0.46x | +0.2% | 424424 | 424424 |
| tsql/rows1000 | generated | 596075.8 | 305839.1 | 0.51x | 303786.7 | 0.51x | -0.7% | 296472 | 296472 |
| web/sf.list10000 | generated | 1176896.9 | 1277906.2 | 1.09x | 1190071.9 | 1.01x | -6.9% | 2720056 | 2720056 |
| el/refused-early.bool | generated | 343.2 | 1282.3 | 3.74x | 560.5 | 1.63x | -56.3% | 1064 | 624 |
| el/refused-late.bool | generated | 1187.3 | 1947.1 | 1.64x | 911.5 | 0.77x | -53.2% | 1064 | 624 |
| el/ladder.bool | generated | 1021.0 | 1969.5 | 1.93x | 1729.5 | 1.69x | -12.2% | 1776 | 1720 |
| sql/refused-late.bool | generated | 2525.4 | 23424.1 | 9.28x | 7743.4 | 3.07x | -66.9% | 13552 | 6616 |
| sql/select20.bool | generated | 6748.1 | 37458.7 | 5.55x | 25306.8 | 3.75x | -32.4% | 21448 | 21392 |
| web/addr-spec.plain | generated | 85.8 | 115.6 | 1.35x | 116.1 | 1.35x | +0.4% | 176 | 176 |
| web/addr-spec.refused | generated | 118.7 | 146.5 | 1.23x | 148.9 | 1.25x | +1.6% | 152 | 152 |
| web/sf.item | generated | 276.0 | 323.2 | 1.17x | 325.2 | 1.18x | +0.6% | 800 | 800 |
| web/sf.list | generated | 1205.1 | 1267.0 | 1.05x | 1247.4 | 1.04x | -1.5% | 3064 | 3064 |
| web/sf.dictionary | generated | 1176.2 | 1236.8 | 1.05x | 1263.2 | 1.07x | +2.1% | 3280 | 3280 |
| el/floor | generated | 321.6 | 943.9 | 2.94x | 813.2 | 2.53x | -13.8% | 1104 | 1104 |
| el/floor | immediate | 321.6 | 472.3 | 1.47x | 478.5 | 1.49x | +1.3% | 1120 | 1120 |
| el/ladder | generated | 1019.2 | 1971.7 | 1.93x | 1777.7 | 1.74x | -9.8% | 1776 | 1776 |
| el/ladder | immediate | 1019.2 | 1179.1 | 1.16x | 1186.5 | 1.16x | +0.6% | 1784 | 1784 |
| el/nest7 | generated | 658.3 | 1837.0 | 2.79x | 1732.7 | 2.63x | -5.7% | 1152 | 1152 |
| el/nest7 | immediate | 658.3 | 1075.1 | 1.63x | 1068.4 | 1.62x | -0.6% | 1120 | 1120 |
| el/block | generated | 1079.6 | 2068.9 | 1.92x | 1849.2 | 1.71x | -10.6% | 2344 | 2344 |
| el/block | immediate | 1079.6 | 1172.5 | 1.09x | 1170.8 | 1.08x | -0.1% | 2440 | 2440 |
| el/loop | generated | 2267.6 | 4202.5 | 1.85x | 3789.5 | 1.67x | -9.8% | 4776 | 4776 |
| el/loop | immediate | 2267.6 | 2603.6 | 1.15x | 2577.6 | 1.14x | -1.0% | 4880 | 4880 |
| el/terms100 | generated | 11716.4 | 16944.4 | 1.45x | 16664.2 | 1.42x | -1.7% | 17904 | 17904 |
| el/terms100 | immediate | 11716.4 | 13090.0 | 1.12x | 12949.3 | 1.11x | -1.1% | 18720 | 18720 |
| el/terms1000 | generated | 112598.0 | 160343.8 | 1.42x | 156481.8 | 1.39x | -2.4% | 169107 | 169129 |
| el/terms1000 | immediate | 112598.0 | 126413.3 | 1.12x | 125146.4 | 1.11x | -1.0% | 177120 | 177120 |
| el/overloads | generated | 1900.3 | 2843.1 | 1.50x | 2590.6 | 1.36x | -8.9% | 4248 | 4248 |
| el/overloads | immediate | 1900.3 | 2054.0 | 1.08x | 2031.5 | 1.07x | -1.1% | 4200 | 4200 |
| el/string | generated | 280.2 | 1070.9 | 3.82x | 997.1 | 3.56x | -6.9% | 1056 | 1056 |
| el/string | immediate | 280.2 | 677.8 | 2.42x | 685.6 | 2.45x | +1.2% | 1032 | 1032 |
| el/interpolation | generated | 1846.3 | 5033.1 | 2.73x | 4817.4 | 2.61x | -4.3% | 2368 | 2368 |
| el/interpolation | immediate | 1846.3 | 4053.5 | 2.20x | 4129.7 | 2.24x | +1.9% | 2424 | 2424 |
| el/untyped | generated | 18301.0 | 25262.1 | 1.38x | 20050.5 | 1.10x | -20.6% | 16424 | 16424 |
| el/refused-early | generated | 471.6 | 1439.3 | 3.05x | 1263.4 | 2.68x | -12.2% | 1064 | 1064 |
| el/refused-early | immediate | 471.6 | 1194.2 | 2.53x | 1197.1 | 2.54x | +0.2% | 1944 | 1944 |
| el/refused-late | generated | 1404.5 | 2114.7 | 1.51x | 1949.3 | 1.39x | -7.8% | 1064 | 1064 |
| el/refused-late | immediate | 1404.5 | 3180.7 | 2.26x | 3155.5 | 2.25x | -0.8% | 3928 | 3928 |
| sql/literal | generated | 64.0 | 141.8 | 2.21x | 146.9 | 2.29x | +3.6% | 160 | 160 |
| sql/comment | generated | 2756.0 | 9054.8 | 3.29x | 6749.6 | 2.45x | -25.5% | 5136 | 5136 |
| sql/conditions100 | generated | 69761.1 | 230546.9 | 3.30x | 173357.5 | 2.49x | -24.8% | 161736 | 161736 |
| sql/conditions1000 | generated | 693176.6 | 2317000.0 | 3.34x | 1717586.7 | 2.48x | -25.9% | 1616136 | 1616203 |
| tsql/comment | generated | 25065.6 | 2193.4 | 0.09x | 2050.0 | 0.08x | -6.5% | 1216 | 1216 |
| sql/column | generated | 184.9 | 462.9 | 2.50x | 412.9 | 2.23x | -10.8% | 392 | 416 |
| sql/arithmetic | generated | 2098.2 | 7611.7 | 3.63x | 5927.9 | 2.83x | -22.1% | 3472 | 3472 |
| sql/nest8 | generated | 3417.1 | 22542.4 | 6.60x | 18346.4 | 5.37x | -18.6% | 6504 | 6504 |
| sql/condition | generated | 2469.2 | 7969.7 | 3.23x | 5896.2 | 2.39x | -26.0% | 4928 | 4928 |
| sql/select1 | generated | 819.1 | 2734.4 | 3.34x | 2064.3 | 2.52x | -24.5% | 1688 | 1688 |
| sql/select20 | generated | 9735.9 | 40607.6 | 4.17x | 28402.1 | 2.92x | -30.1% | 21448 | 21448 |
| sql/values | generated | 660.1 | 3321.5 | 5.03x | 2340.8 | 3.55x | -29.5% | 1904 | 1904 |
| sql/create | generated | 951.2 | 2843.6 | 2.99x | 2862.4 | 3.01x | +0.7% | 1568 | 1568 |
| sql/refused-late | generated | 3533.7 | 24731.7 | 7.00x | 18301.7 | 5.18x | -26.0% | 13552 | 13552 |
