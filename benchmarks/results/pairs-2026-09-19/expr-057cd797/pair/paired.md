Median of 5 of 5 runs, each in a process of its own; control 31.2 ns (the runs' controls: 31.3, 31.0, 31.4, 31.2, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 15:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | 2865912.5 | 684550.0 | 0.24x | 299693.8 | 0.10x | -56.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | 858536.7 | 1067134.4 | 1.24x | 402964.8 | 0.47x | -62.2% | 424424 | 424424 |
| tsql/rows1000 | generated | 583850.8 | 823653.9 | 1.41x | 301382.0 | 0.52x | -63.4% | 296472 | 296472 |
| el/floor | generated | 315.3 | 907.0 | 2.88x | 902.6 | 2.86x | -0.5% | 1104 | 1104 |
| el/floor | immediate | 315.3 | 466.1 | 1.48x | 457.2 | 1.45x | -1.9% | 1120 | 1120 |
| el/ladder | generated | 989.8 | 1894.9 | 1.91x | 1909.4 | 1.93x | +0.8% | 1776 | 1776 |
| el/ladder | immediate | 989.8 | 1142.9 | 1.15x | 1143.6 | 1.16x | +0.1% | 1784 | 1784 |
| el/nest7 | generated | 648.9 | 1775.4 | 2.74x | 1785.4 | 2.75x | +0.6% | 1152 | 1152 |
| el/nest7 | immediate | 648.9 | 1043.6 | 1.61x | 1031.7 | 1.59x | -1.1% | 1120 | 1120 |
| el/block | generated | 1040.0 | 2012.3 | 1.93x | 2012.0 | 1.93x | 0.0% | 2344 | 2344 |
| el/block | immediate | 1040.0 | 1144.6 | 1.10x | 1138.8 | 1.09x | -0.5% | 2440 | 2440 |
| el/loop | generated | 2201.9 | 4088.1 | 1.86x | 4109.9 | 1.87x | +0.5% | 4776 | 4776 |
| el/loop | immediate | 2201.9 | 2477.7 | 1.13x | 2526.0 | 1.15x | +2.0% | 4880 | 4880 |
| el/terms100 | generated | 11727.4 | 16698.4 | 1.42x | 16620.9 | 1.42x | -0.5% | 17904 | 17904 |
| el/terms100 | immediate | 11727.4 | 12734.3 | 1.09x | 12494.1 | 1.07x | -1.9% | 18720 | 18720 |
| el/terms1000 | generated | 114182.6 | 158813.9 | 1.39x | 156231.7 | 1.37x | -1.6% | 169107 | 169128 |
| el/terms1000 | immediate | 114182.6 | 122251.3 | 1.07x | 120833.0 | 1.06x | -1.2% | 177120 | 177120 |
| el/overloads | generated | 1840.2 | 2779.1 | 1.51x | 2748.9 | 1.49x | -1.1% | 4248 | 4248 |
| el/overloads | immediate | 1840.2 | 2011.6 | 1.09x | 2020.1 | 1.10x | +0.4% | 4200 | 4200 |
| el/string | generated | 263.5 | 1045.8 | 3.97x | 1055.6 | 4.01x | +0.9% | 1056 | 1056 |
| el/string | immediate | 263.5 | 663.9 | 2.52x | 664.8 | 2.52x | +0.1% | 1032 | 1032 |
| el/interpolation | generated | 1889.7 | 4983.0 | 2.64x | 5008.6 | 2.65x | +0.5% | 2368 | 2368 |
| el/interpolation | immediate | 1889.7 | 4562.6 | 2.41x | 4196.1 | 2.22x | -8.0% | 2424 | 2424 |
| el/untyped | generated | 27952.1 | 29411.4 | 1.05x | 30340.6 | 1.09x | +3.2% | 16424 | 16424 |
| el/refused-early | generated | 495.1 | 1487.3 | 3.00x | 1493.0 | 3.02x | +0.4% | 1064 | 1064 |
| el/refused-early | immediate | 495.1 | 1253.1 | 2.53x | 1239.1 | 2.50x | -1.1% | 1944 | 1944 |
| el/refused-late | generated | 1577.1 | 2141.2 | 1.36x | 2212.8 | 1.40x | +3.3% | 1064 | 1064 |
| el/refused-late | immediate | 1577.1 | 3049.4 | 1.93x | 3211.0 | 2.04x | +5.3% | 3928 | 3928 |
| sql/literal | generated | 65.8 | 155.7 | 2.37x | 154.9 | 2.35x | -0.5% | 160 | 160 |
| sql/comment | generated | 3029.4 | 17132.2 | 5.66x | 9445.7 | 3.12x | -44.9% | 5136 | 5136 |
| sql/conditions100 | generated | 76138.5 | 438246.4 | 5.76x | 237277.0 | 3.12x | -45.9% | 161736 | 161736 |
| sql/conditions1000 | generated | 768612.5 | 4425891.4 | 5.76x | 2383518.0 | 3.10x | -46.1% | 1616136 | 1616136 |
| tsql/comment | generated | 26659.1 | 4058.8 | 0.15x | 2089.4 | 0.08x | -48.5% | 1192 | 1192 |
| sql/column | generated | 199.5 | 1216.1 | 6.10x | 480.0 | 2.41x | -60.5% | 392 | 392 |
| sql/arithmetic | generated | 2084.9 | 13834.1 | 6.64x | 7686.2 | 3.69x | -44.4% | 3472 | 3472 |
| sql/nest8 | generated | 3642.1 | 34001.4 | 9.34x | 22675.7 | 6.23x | -33.3% | 6504 | 6504 |
| sql/condition | generated | 2616.8 | 14406.2 | 5.51x | 7987.5 | 3.05x | -44.6% | 4928 | 4928 |
| sql/select1 | generated | 881.7 | 5835.7 | 6.62x | 2761.4 | 3.13x | -52.7% | 1688 | 1688 |
| sql/select20 | generated | 10550.3 | 76396.5 | 7.24x | 41413.6 | 3.93x | -45.8% | 21448 | 21448 |
| sql/values | generated | 700.3 | 6338.6 | 9.05x | 3380.4 | 4.83x | -46.7% | 1904 | 1904 |
| sql/create | generated | 1036.6 | 4793.9 | 4.62x | 2960.1 | 2.86x | -38.3% | 1568 | 1568 |
| sql/refused-late | generated | 3614.6 | 46234.1 | 12.79x | 25653.7 | 7.10x | -44.5% | 13552 | 13552 |
