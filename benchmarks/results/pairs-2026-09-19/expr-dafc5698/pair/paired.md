Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.7, 31.5, 31.3, 31.0, 31.3).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 17:33

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | 2596418.8 | 231575.0 | 0.09x | 266875.0 | 0.10x | +15.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | 889910.9 | 416646.9 | 0.47x | 419072.7 | 0.47x | +0.6% | 424424 | 424424 |
| tsql/rows1000 | generated | 598467.2 | 307265.6 | 0.51x | 310625.0 | 0.52x | +1.1% | 296472 | 296472 |
| web/sf.list10000 | generated | 1232731.2 | 1190400.0 | 0.97x | 1225700.0 | 0.99x | +3.0% | 2720056 | 2720056 |
| el/refused-early.bool | generated | 331.4 | 1123.1 | 3.39x | 565.1 | 1.70x | -49.7% | 1064 | 624 |
| el/refused-late.bool | generated | 1196.9 | 1763.7 | 1.47x | 934.0 | 0.78x | -47.0% | 1064 | 624 |
| el/ladder.bool | generated | 1041.3 | 1782.1 | 1.71x | 1767.4 | 1.70x | -0.8% | 1776 | 1720 |
| sql/refused-late.bool | generated | 2536.8 | 16561.1 | 6.53x | 7098.1 | 2.80x | -57.1% | 13552 | 6616 |
| sql/select20.bool | generated | 6932.2 | 25417.7 | 3.67x | 22407.1 | 3.23x | -11.8% | 21448 | 21392 |
| web/addr-spec.plain | generated | 86.0 | 115.1 | 1.34x | 117.0 | 1.36x | +1.7% | 176 | 176 |
| web/addr-spec.refused | generated | 117.0 | 148.0 | 1.27x | 151.2 | 1.29x | +2.1% | 152 | 152 |
| web/sf.item | generated | 278.2 | 313.7 | 1.13x | 321.7 | 1.16x | +2.6% | 800 | 800 |
| web/sf.list | generated | 1197.4 | 1259.5 | 1.05x | 1243.2 | 1.04x | -1.3% | 3064 | 3064 |
| web/sf.dictionary | generated | 1177.4 | 1258.5 | 1.07x | 1261.3 | 1.07x | +0.2% | 3280 | 3280 |
| el/floor | generated | 316.9 | 828.4 | 2.61x | 819.6 | 2.59x | -1.1% | 1104 | 1104 |
| el/floor | immediate | 316.9 | 479.0 | 1.51x | 487.8 | 1.54x | +1.9% | 1120 | 1120 |
| el/ladder | generated | 1016.1 | 1767.3 | 1.74x | 1823.4 | 1.79x | +3.2% | 1776 | 1776 |
| el/ladder | immediate | 1016.1 | 1170.5 | 1.15x | 1191.3 | 1.17x | +1.8% | 1784 | 1784 |
| el/nest7 | generated | 663.7 | 1741.9 | 2.62x | 1732.9 | 2.61x | -0.5% | 1152 | 1152 |
| el/nest7 | immediate | 663.7 | 1056.2 | 1.59x | 1056.9 | 1.59x | +0.1% | 1120 | 1120 |
| el/block | generated | 1071.1 | 1828.0 | 1.71x | 1853.3 | 1.73x | +1.4% | 2344 | 2344 |
| el/block | immediate | 1071.1 | 1158.4 | 1.08x | 1183.5 | 1.10x | +2.2% | 2440 | 2440 |
| el/loop | generated | 2249.5 | 3756.5 | 1.67x | 3765.0 | 1.67x | +0.2% | 4776 | 4776 |
| el/loop | immediate | 2249.5 | 2570.3 | 1.14x | 2599.7 | 1.16x | +1.1% | 4880 | 4880 |
| el/terms100 | generated | 11509.1 | 16639.1 | 1.45x | 16520.2 | 1.44x | -0.7% | 17904 | 17904 |
| el/terms100 | immediate | 11509.1 | 12902.6 | 1.12x | 13017.8 | 1.13x | +0.9% | 18720 | 18720 |
| el/terms1000 | generated | 111567.4 | 157412.6 | 1.41x | 157992.7 | 1.42x | +0.4% | 169107 | 169104 |
| el/terms1000 | immediate | 111567.4 | 124934.0 | 1.12x | 125976.8 | 1.13x | +0.8% | 177120 | 177120 |
| el/overloads | generated | 1879.3 | 2603.7 | 1.39x | 2630.9 | 1.40x | +1.0% | 4248 | 4248 |
| el/overloads | immediate | 1879.3 | 2039.0 | 1.08x | 2027.8 | 1.08x | -0.5% | 4200 | 4200 |
| el/string | generated | 282.9 | 970.8 | 3.43x | 1002.0 | 3.54x | +3.2% | 1056 | 1056 |
| el/string | immediate | 282.9 | 671.8 | 2.38x | 675.1 | 2.39x | +0.5% | 1032 | 1032 |
| el/interpolation | generated | 1889.3 | 4710.9 | 2.49x | 4986.2 | 2.64x | +5.8% | 2368 | 2368 |
| el/interpolation | immediate | 1889.3 | 4335.5 | 2.29x | 4442.4 | 2.35x | +2.5% | 2424 | 2424 |
| el/untyped | generated | 17908.4 | 25268.3 | 1.41x | 19647.1 | 1.10x | -22.2% | 16424 | 16424 |
| el/refused-early | generated | 469.9 | 1268.9 | 2.70x | 1292.3 | 2.75x | +1.9% | 1064 | 1064 |
| el/refused-early | immediate | 469.9 | 1240.3 | 2.64x | 1261.6 | 2.68x | +1.7% | 1944 | 1944 |
| el/refused-late | generated | 1518.4 | 1901.0 | 1.25x | 1968.0 | 1.30x | +3.5% | 1064 | 1064 |
| el/refused-late | immediate | 1518.4 | 3223.3 | 2.12x | 3223.0 | 2.12x | 0.0% | 3928 | 3928 |
| sql/literal | generated | 64.8 | 145.5 | 2.24x | 145.0 | 2.24x | -0.4% | 160 | 160 |
| sql/comment | generated | 2586.7 | 6706.6 | 2.59x | 6018.1 | 2.33x | -10.3% | 5136 | 5136 |
| sql/conditions100 | generated | 72535.6 | 172402.6 | 2.38x | 160825.6 | 2.22x | -6.7% | 161736 | 161736 |
| sql/conditions1000 | generated | 727632.0 | 1786928.9 | 2.46x | 1607500.8 | 2.21x | -10.0% | 1616136 | 1616136 |
| tsql/comment | generated | 25169.7 | 2116.3 | 0.08x | 2085.3 | 0.08x | -1.5% | 1216 | 1216 |
| sql/column | generated | 186.5 | 405.4 | 2.17x | 430.4 | 2.31x | +6.2% | 392 | 392 |
| sql/arithmetic | generated | 2081.2 | 5919.2 | 2.84x | 5130.5 | 2.47x | -13.3% | 3472 | 3472 |
| sql/nest8 | generated | 3391.9 | 18680.9 | 5.51x | 11932.8 | 3.52x | -36.1% | 6504 | 6504 |
| sql/condition | generated | 2397.4 | 5817.9 | 2.43x | 5423.5 | 2.26x | -6.8% | 4928 | 4928 |
| sql/select1 | generated | 823.7 | 2010.2 | 2.44x | 1958.7 | 2.38x | -2.6% | 1688 | 1688 |
| sql/select20 | generated | 9676.5 | 27443.8 | 2.84x | 24695.3 | 2.55x | -10.0% | 21448 | 21448 |
| sql/values | generated | 633.3 | 2314.0 | 3.65x | 1967.7 | 3.11x | -15.0% | 1904 | 1904 |
| sql/create | generated | 935.3 | 2816.3 | 3.01x | 2817.3 | 3.01x | 0.0% | 1568 | 1568 |
| sql/refused-late | generated | 3574.5 | 18309.2 | 5.12x | 16889.6 | 4.73x | -7.8% | 13552 | 13552 |
