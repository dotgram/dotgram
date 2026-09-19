Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.5, 31.0, 31.6, 31.3, 31.0).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 04:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 75.6 | 158.9 | 2.10x | 167.5 | 2.21x | +5.4% | 192 | 192 |
| fix/slope-8.text | generated | 464.0 | 871.2 | 1.88x | 891.6 | 1.92x | +2.3% | 920 | 920 |
| feeds/stock-count.good.text | generated | 36545.9 | 35799.8 | 0.98x | 36030.1 | 0.99x | +0.6% | 111312 | 111312 |
| web/url.plain | generated | 88.3 | 219.6 | 2.49x | 221.3 | 2.51x | +0.8% | 360 | 360 |
| web/url.full | generated | 165.5 | 293.9 | 1.78x | 297.2 | 1.80x | +1.1% | 536 | 536 |
| web/url.ipv4 | generated | 106.5 | 230.4 | 2.16x | 230.5 | 2.16x | +0.1% | 384 | 384 |
| web/url.long-path | generated | 179.6 | 414.8 | 2.31x | 415.4 | 2.31x | +0.1% | 512 | 512 |
| web/url.refused | generated | 59.5 | 519.7 | 8.73x | 599.1 | 10.07x | +15.3% | 64 | 152 |
| web/json.object | generated | 570.5 | 1660.0 | 2.91x | 1659.5 | 2.91x | 0.0% | 2584 | 2584 |
| web/json.array | generated | 649.9 | 1754.8 | 2.70x | 1734.2 | 2.67x | -1.2% | 2424 | 2424 |
| el/floor | generated | 321.5 | 941.0 | 2.93x | 930.2 | 2.89x | -1.2% | 1104 | 1104 |
| el/floor | immediate | 321.5 | 480.3 | 1.49x | 483.7 | 1.50x | +0.7% | 1120 | 1120 |
| el/ladder | generated | 1018.6 | 1985.6 | 1.95x | 1947.9 | 1.91x | -1.9% | 1776 | 1776 |
| el/ladder | immediate | 1018.6 | 1179.6 | 1.16x | 1188.2 | 1.17x | +0.7% | 1784 | 1784 |
| el/nest7 | generated | 649.9 | 1821.1 | 2.80x | 1817.8 | 2.80x | -0.2% | 1152 | 1152 |
| el/nest7 | immediate | 649.9 | 1069.0 | 1.64x | 1066.2 | 1.64x | -0.3% | 1120 | 1120 |
| el/block | generated | 1069.6 | 2059.0 | 1.93x | 2049.6 | 1.92x | -0.5% | 2344 | 2344 |
| el/block | immediate | 1069.6 | 1195.5 | 1.12x | 1203.6 | 1.13x | +0.7% | 2440 | 2440 |
| el/loop | generated | 2290.6 | 4368.8 | 1.91x | 4423.5 | 1.93x | +1.3% | 4776 | 4776 |
| el/loop | immediate | 2290.6 | 2644.9 | 1.15x | 2644.3 | 1.15x | 0.0% | 5200 | 5200 |
| el/overloads | generated | 1885.1 | 2905.2 | 1.54x | 2855.5 | 1.51x | -1.7% | 4248 | 4248 |
| el/overloads | immediate | 1885.1 | 2060.4 | 1.09x | 2068.4 | 1.10x | +0.4% | 4240 | 4240 |
| el/string | generated | 285.7 | 1085.2 | 3.80x | 1067.0 | 3.73x | -1.7% | 1056 | 1056 |
| el/string | immediate | 285.7 | 684.7 | 2.40x | 688.2 | 2.41x | +0.5% | 1032 | 1032 |
| el/interpolation | generated | 1852.7 | 4850.8 | 2.62x | 5031.4 | 2.72x | +3.7% | 2368 | 2368 |
| el/interpolation | immediate | 1852.7 | 4038.1 | 2.18x | 4152.0 | 2.24x | +2.8% | 2424 | 2424 |
| el/untyped | generated | 19558.4 | 22562.8 | 1.15x | 21179.4 | 1.08x | -6.1% | 16424 | 16424 |
| el/refused-early | generated | 483.0 | 1419.0 | 2.94x | 1478.8 | 3.06x | +4.2% | 976 | 1064 |
| el/refused-early | immediate | 483.0 | 1166.2 | 2.41x | 1221.0 | 2.53x | +4.7% | 1856 | 1944 |
| el/refused-late | generated | 1492.7 | 2128.3 | 1.43x | 2154.9 | 1.44x | +1.3% | 976 | 1064 |
| el/refused-late | immediate | 1492.7 | 3056.2 | 2.05x | 3126.7 | 2.09x | +2.3% | 3840 | 3928 |
| sql/column | generated | 177.2 | 1366.9 | 7.71x | 1360.9 | 7.68x | -0.4% | 392 | 392 |
| sql/select20 | generated | 9716.7 | 93984.9 | 9.67x | 93417.6 | 9.61x | -0.6% | 21448 | 21448 |
