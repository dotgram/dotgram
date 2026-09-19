Median of 5 of 5 runs, each in a process of its own; control 31.1 ns (the runs' controls: 31.1, 31.1, 31.1, 30.7, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 05:34

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/floor | generated | 309.1 | 982.5 | 3.18x | 1186.1 | 3.84x | +20.7% | 1104 | 1104 |
| el/floor | immediate | 309.1 | 473.3 | 1.53x | 480.5 | 1.55x | +1.5% | 1120 | 1120 |
| el/ladder | generated | 987.2 | 2244.1 | 2.27x | 2752.9 | 2.79x | +22.7% | 1776 | 1776 |
| el/ladder | immediate | 987.2 | 1166.6 | 1.18x | 1172.2 | 1.19x | +0.5% | 1784 | 1784 |
| el/nest7 | generated | 649.2 | 1954.4 | 3.01x | 2297.5 | 3.54x | +17.6% | 1152 | 1152 |
| el/nest7 | immediate | 649.2 | 1033.7 | 1.59x | 1032.7 | 1.59x | -0.1% | 1120 | 1120 |
| el/block | generated | 1030.4 | 2185.1 | 2.12x | 2619.7 | 2.54x | +19.9% | 2344 | 2344 |
| el/block | immediate | 1030.4 | 1153.0 | 1.12x | 1156.3 | 1.12x | +0.3% | 2440 | 2440 |
| el/loop | generated | 2177.6 | 5026.9 | 2.31x | 6127.8 | 2.81x | +21.9% | 4776 | 4776 |
| el/loop | immediate | 2177.6 | 2595.4 | 1.19x | 2611.0 | 1.20x | +0.6% | 5200 | 5200 |
| el/overloads | generated | 1834.3 | 2808.0 | 1.53x | 3185.0 | 1.74x | +13.4% | 4248 | 4248 |
| el/overloads | immediate | 1834.3 | 2029.0 | 1.11x | 2028.5 | 1.11x | 0.0% | 4240 | 4240 |
| el/string | generated | 261.8 | 1053.3 | 4.02x | 1152.9 | 4.40x | +9.5% | 1056 | 1056 |
| el/string | immediate | 261.8 | 668.4 | 2.55x | 663.0 | 2.53x | -0.8% | 1032 | 1032 |
| el/interpolation | generated | 1985.1 | 4723.9 | 2.38x | 5264.2 | 2.65x | +11.4% | 2368 | 2368 |
| el/interpolation | immediate | 1985.1 | 4085.4 | 2.06x | 4209.1 | 2.12x | +3.0% | 2424 | 2424 |
| el/untyped | generated | 22658.5 | 23183.6 | 1.02x | 24226.2 | 1.07x | +4.5% | 16424 | 16424 |
| el/refused-early | generated | 456.5 | 1591.5 | 3.49x | 1861.4 | 4.08x | +17.0% | 1064 | 1064 |
| el/refused-early | immediate | 456.5 | 1181.9 | 2.59x | 1176.3 | 2.58x | -0.5% | 1944 | 1944 |
| el/refused-late | generated | 1492.0 | 2265.4 | 1.52x | 2551.3 | 1.71x | +12.6% | 1064 | 1064 |
| el/refused-late | immediate | 1492.0 | 3025.3 | 2.03x | 3101.8 | 2.08x | +2.5% | 3928 | 3928 |
| sql/literal | generated | 62.4 | 225.0 | 3.60x | 229.9 | 3.68x | +2.2% | 160 | 160 |
| sql/comment | generated | 2822.8 | 23803.6 | 8.43x | 25372.5 | 8.99x | +6.6% | 5136 | 5136 |
| sql/conditions100 | generated | 49778.7 | 2455445.3 | 49.33x | 589749.6 | 11.85x | -76.0% | 20958662 | 161760 |
| sql/conditions1000 | generated | 505700.0 | 30227831.2 | 59.77x | 5937517.2 | 11.74x | -80.4% | 167803929 | 1616203 |
| tsql/comment | generated | 19793.0 | 3959.9 | 0.20x | 4933.6 | 0.25x | +24.6% | 1192 | 1192 |
| sql/column | generated | 137.6 | 1357.5 | 9.87x | 1934.9 | 14.06x | +42.5% | 392 | 392 |
| sql/arithmetic | generated | 1587.2 | 16704.6 | 10.52x | 18384.0 | 11.58x | +10.1% | 3472 | 3472 |
| sql/nest8 | generated | 2741.7 | 45140.5 | 16.46x | 43856.8 | 16.00x | -2.8% | 6504 | 6504 |
| sql/condition | generated | 1797.5 | 17678.3 | 9.83x | 19256.3 | 10.71x | +8.9% | 4928 | 4928 |
| sql/select1 | generated | 663.0 | 7071.8 | 10.67x | 8484.1 | 12.80x | +20.0% | 1688 | 1688 |
| sql/select20 | generated | 7006.7 | 104561.4 | 14.92x | 103787.6 | 14.81x | -0.7% | 21448 | 21448 |
| sql/values | generated | 463.2 | 8876.3 | 19.16x | 8798.1 | 18.99x | -0.9% | 1904 | 1904 |
| sql/create | generated | 794.6 | 5928.4 | 7.46x | 7395.0 | 9.31x | +24.7% | 1568 | 1568 |
| sql/refused-late | generated | 2655.2 | 62162.5 | 23.41x | 61745.4 | 23.25x | -0.7% | 13832 | 13832 |
