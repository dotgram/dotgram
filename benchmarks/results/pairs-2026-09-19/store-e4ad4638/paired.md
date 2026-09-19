Median of 5 of 5 runs, each in a process of its own; control 31.1 ns (the runs' controls: 31.1, 31.0, 31.1, 31.1, 31.0).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 06:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/floor | generated | 326.4 | 962.4 | 2.95x | 963.0 | 2.95x | +0.1% | 1104 | 1104 |
| el/floor | immediate | 326.4 | 476.1 | 1.46x | 472.0 | 1.45x | -0.9% | 1120 | 1120 |
| el/ladder | generated | 973.0 | 2156.1 | 2.22x | 2163.9 | 2.22x | +0.4% | 1776 | 1776 |
| el/ladder | immediate | 973.0 | 1151.1 | 1.18x | 1146.0 | 1.18x | -0.4% | 1784 | 1784 |
| el/nest7 | generated | 639.4 | 1921.7 | 3.01x | 1914.1 | 2.99x | -0.4% | 1152 | 1152 |
| el/nest7 | immediate | 639.4 | 1012.0 | 1.58x | 1015.9 | 1.59x | +0.4% | 1120 | 1120 |
| el/block | generated | 1023.0 | 2142.0 | 2.09x | 2145.1 | 2.10x | +0.1% | 2344 | 2344 |
| el/block | immediate | 1023.0 | 1136.7 | 1.11x | 1136.6 | 1.11x | 0.0% | 2440 | 2440 |
| el/loop | generated | 2163.9 | 5003.9 | 2.31x | 4936.0 | 2.28x | -1.4% | 4776 | 4776 |
| el/loop | immediate | 2163.9 | 2548.1 | 1.18x | 2566.5 | 1.19x | +0.7% | 5200 | 5200 |
| el/overloads | generated | 1827.6 | 2855.4 | 1.56x | 2809.1 | 1.54x | -1.6% | 4248 | 4248 |
| el/overloads | immediate | 1827.6 | 2022.4 | 1.11x | 2012.6 | 1.10x | -0.5% | 4240 | 4240 |
| el/string | generated | 262.1 | 1023.4 | 3.90x | 1031.8 | 3.94x | +0.8% | 1056 | 1056 |
| el/string | immediate | 262.1 | 657.4 | 2.51x | 657.8 | 2.51x | +0.1% | 1032 | 1032 |
| el/interpolation | generated | 2084.7 | 4784.3 | 2.29x | 4886.6 | 2.34x | +2.1% | 2368 | 2368 |
| el/interpolation | immediate | 2084.7 | 3842.9 | 1.84x | 3897.3 | 1.87x | +1.4% | 2424 | 2424 |
| el/untyped | generated | 20116.5 | 20396.5 | 1.01x | 20174.6 | 1.00x | -1.1% | 16424 | 16424 |
| el/refused-early | generated | 471.5 | 1609.5 | 3.41x | 1573.5 | 3.34x | -2.2% | 1064 | 1064 |
| el/refused-early | immediate | 471.5 | 1181.3 | 2.51x | 1178.2 | 2.50x | -0.3% | 1944 | 1944 |
| el/refused-late | generated | 1522.8 | 2363.0 | 1.55x | 2153.0 | 1.41x | -8.9% | 1064 | 1064 |
| el/refused-late | immediate | 1522.8 | 3087.0 | 2.03x | 3068.5 | 2.02x | -0.6% | 3928 | 3928 |
| sql/literal | generated | 63.3 | 224.3 | 3.55x | 149.5 | 2.36x | -33.4% | 160 | 160 |
| sql/comment | generated | 2825.9 | 24072.1 | 8.52x | 18623.9 | 6.59x | -22.6% | 5136 | 5136 |
| sql/conditions100 | generated | 48765.3 | 2845420.3 | 58.35x | 429298.0 | 8.80x | -84.9% | 20958345 | 161760 |
| sql/conditions1000 | generated | 496291.0 | 31641437.5 | 63.76x | 4319225.0 | 8.70x | -86.3% | 167802397 | 1616160 |
| tsql/comment | generated | 19617.3 | 3952.2 | 0.20x | 3898.1 | 0.20x | -1.4% | 1192 | 1192 |
| sql/column | generated | 136.9 | 1338.1 | 9.78x | 1154.1 | 8.43x | -13.7% | 392 | 392 |
| sql/arithmetic | generated | 1582.7 | 16500.5 | 10.43x | 13584.2 | 8.58x | -17.7% | 3472 | 3472 |
| sql/nest8 | generated | 2718.4 | 44553.8 | 16.39x | 36725.5 | 13.51x | -17.6% | 6504 | 6504 |
| sql/condition | generated | 1771.6 | 17653.0 | 9.96x | 14225.5 | 8.03x | -19.4% | 4928 | 4928 |
| sql/select1 | generated | 659.8 | 7034.6 | 10.66x | 5859.0 | 8.88x | -16.7% | 1688 | 1688 |
| sql/select20 | generated | 6921.1 | 104141.7 | 15.05x | 78212.0 | 11.30x | -24.9% | 21448 | 21448 |
| sql/values | generated | 453.1 | 8808.0 | 19.44x | 6508.2 | 14.36x | -26.1% | 1904 | 1904 |
| sql/create | generated | 782.1 | 5957.5 | 7.62x | 4564.1 | 5.84x | -23.4% | 1568 | 1568 |
| sql/refused-late | generated | 2622.5 | 61473.2 | 23.44x | 46429.0 | 17.70x | -24.5% | 13832 | 13832 |
