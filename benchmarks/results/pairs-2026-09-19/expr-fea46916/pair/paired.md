Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.2, 31.3, 31.3, 31.4, 31.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 20:25

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | 668481.2 | 1173800.0 | 1.76x | 124071.9 | 0.19x | -89.4% | 113600 | 105600 |
| tsql/script100.boolboth | generated | 660082.8 | 1158691.4 | 1.76x | 122182.0 | 0.19x | -89.5% | 105600 | 105600 |
| tsql/script100 | generated | 646936.7 | 1171857.0 | 1.81x | 124400.8 | 0.19x | -89.4% | 113600 | 113600 |
| tsql/script400.bool | generated | 2698490.6 | 17152225.0 | 6.36x | 511015.6 | 0.19x | -97.0% | 454400 | 422403 |
| tsql/script400.boolboth | generated | 2676159.4 | 16910843.8 | 6.32x | 499462.5 | 0.19x | -97.0% | 422403 | 422403 |
| tsql/script400 | generated | 2649756.2 | 17188868.8 | 6.49x | 498331.2 | 0.19x | -97.1% | 454400 | 454400 |
| tsql/columns1000 | generated | 1730131.2 | 189260.9 | 0.11x | 190042.2 | 0.11x | +0.4% | 200464 | 200464 |
| tsql/conditions1000 | generated | 873714.1 | 325598.4 | 0.37x | 334087.5 | 0.38x | +2.6% | 424424 | 424424 |
| tsql/rows1000 | generated | 591751.6 | 239332.8 | 0.40x | 240982.0 | 0.41x | +0.7% | 296472 | 296472 |
| web/json.array10000 | generated | 168342.0 | 200813.9 | 1.19x | 201058.0 | 1.19x | +0.1% | 720048 | 720048 |
| web/json.object10000 | generated | 850606.2 | 1323522.7 | 1.56x | 1297279.7 | 1.53x | -2.0% | 1599240 | 1599240 |
| web/url.path1000 | generated | 9510.7 | 11052.7 | 1.16x | 11160.9 | 1.17x | +1.0% | 8384 | 8384 |
| web/media-type.params1000 | generated | 49921.5 | 47747.3 | 0.96x | 49960.5 | 1.00x | +4.6% | 151472 | 151472 |
| web/sf.list10000 | generated | 1177559.4 | 1232034.4 | 1.05x | 1227371.9 | 1.04x | -0.4% | 2720056 | 2720056 |
| el/refused-early.bool | generated | 346.6 | 1113.7 | 3.21x | 572.4 | 1.65x | -48.6% | 1064 | 624 |
| el/refused-late.bool | generated | 1225.6 | 1792.2 | 1.46x | 938.2 | 0.77x | -47.6% | 1064 | 624 |
| el/ladder.bool | generated | 1088.2 | 1826.5 | 1.68x | 1816.6 | 1.67x | -0.5% | 1776 | 1720 |
| sql/refused-late.bool | generated | 2658.2 | 12283.3 | 4.62x | 6071.9 | 2.28x | -50.6% | 13552 | 6616 |
| sql/select20.bool | generated | 6997.9 | 18498.0 | 2.64x | 19043.2 | 2.72x | +2.9% | 21448 | 21392 |
| web/url.plain | generated | 97.0 | 226.0 | 2.33x | 223.7 | 2.31x | -1.0% | 360 | 360 |
| web/url.full | generated | 170.5 | 296.3 | 1.74x | 292.0 | 1.71x | -1.4% | 536 | 536 |
| web/url.ipv4 | generated | 116.1 | 234.0 | 2.02x | 234.0 | 2.02x | 0.0% | 384 | 384 |
| web/url.long-path | generated | 178.0 | 411.7 | 2.31x | 411.8 | 2.31x | 0.0% | 512 | 512 |
| web/url.refused | generated | 68.0 | 250.1 | 3.68x | 254.2 | 3.74x | +1.6% | 64 | 64 |
| web/json.object | generated | 581.3 | 925.4 | 1.59x | 937.3 | 1.61x | +1.3% | 2584 | 2584 |
| web/json.array | generated | 599.8 | 732.7 | 1.22x | 748.7 | 1.25x | +2.2% | 2400 | 2400 |
| web/cookie.full | generated | 303.0 | 332.0 | 1.10x | 345.1 | 1.14x | +4.0% | 2136 | 2136 |
| web/cookie.short | generated | 52.4 | 75.1 | 1.43x | 74.7 | 1.42x | -0.6% | 336 | 336 |
| web/pointer.full | generated | 203.9 | 195.8 | 0.96x | 197.1 | 0.97x | +0.7% | 440 | 440 |
| web/pointer.short | generated | 36.2 | 54.2 | 1.50x | 54.3 | 1.50x | +0.1% | 144 | 144 |
| web/media-type.plain | generated | 159.5 | 188.4 | 1.18x | 195.6 | 1.23x | +3.8% | 448 | 448 |
| web/media-type.quoted | generated | 251.4 | 291.2 | 1.16x | 291.4 | 1.16x | +0.1% | 816 | 816 |
| web/media-type.refused | generated | 63.4 | 91.0 | 1.44x | 91.3 | 1.44x | +0.4% | 64 | 64 |
| web/addr-spec.plain | generated | 89.1 | 110.9 | 1.25x | 109.8 | 1.23x | -1.0% | 176 | 176 |
| web/addr-spec.refused | generated | 46.1 | 75.7 | 1.64x | 73.9 | 1.60x | -2.4% | 64 | 64 |
| web/date-time.utc | generated | 50.3 | 61.3 | 1.22x | 63.8 | 1.27x | +4.2% | 176 | 176 |
| web/date-time.offset | generated | 77.6 | 90.4 | 1.16x | 91.6 | 1.18x | +1.3% | 208 | 208 |
| web/language-tag.plain | generated | 115.3 | 155.7 | 1.35x | 152.5 | 1.32x | -2.1% | 360 | 360 |
| web/language-tag.full | generated | 408.4 | 480.9 | 1.18x | 477.6 | 1.17x | -0.7% | 720 | 720 |
| web/language-tag.refused | generated | 111.0 | 148.7 | 1.34x | 153.5 | 1.38x | +3.2% | 64 | 64 |
| web/date-time.refused | generated | 35.8 | 47.3 | 1.32x | 50.2 | 1.40x | +6.1% | 96 | 96 |
| web/sf.item | generated | 253.4 | 296.6 | 1.17x | 297.7 | 1.17x | +0.4% | 800 | 800 |
| web/sf.list | generated | 1161.1 | 1234.9 | 1.06x | 1233.5 | 1.06x | -0.1% | 3064 | 3064 |
| web/sf.dictionary | generated | 1188.4 | 1217.2 | 1.02x | 1210.8 | 1.02x | -0.5% | 3280 | 3280 |
| el/floor | generated | 315.9 | 822.7 | 2.60x | 788.9 | 2.50x | -4.1% | 1104 | 1104 |
| el/floor | immediate | 315.9 | 472.9 | 1.50x | 474.9 | 1.50x | +0.4% | 1120 | 1120 |
| el/ladder | generated | 1022.8 | 1751.9 | 1.71x | 1753.6 | 1.71x | +0.1% | 1776 | 1776 |
| el/ladder | immediate | 1022.8 | 1157.8 | 1.13x | 1147.8 | 1.12x | -0.9% | 1784 | 1784 |
| el/nest7 | generated | 698.4 | 1736.6 | 2.49x | 1715.9 | 2.46x | -1.2% | 1152 | 1152 |
| el/nest7 | immediate | 698.4 | 1042.5 | 1.49x | 1056.9 | 1.51x | +1.4% | 1120 | 1120 |
| el/block | generated | 1063.3 | 1789.5 | 1.68x | 1795.2 | 1.69x | +0.3% | 2344 | 2344 |
| el/block | immediate | 1063.3 | 1153.3 | 1.08x | 1162.4 | 1.09x | +0.8% | 2440 | 2440 |
| el/loop | generated | 2225.0 | 3723.4 | 1.67x | 3711.1 | 1.67x | -0.3% | 4776 | 4776 |
| el/loop | immediate | 2225.0 | 2537.4 | 1.14x | 2617.6 | 1.18x | +3.2% | 4880 | 4880 |
| el/terms100 | generated | 11874.9 | 16288.7 | 1.37x | 16543.4 | 1.39x | +1.6% | 17904 | 17904 |
| el/terms100 | immediate | 11874.9 | 12828.7 | 1.08x | 12971.3 | 1.09x | +1.1% | 18720 | 18720 |
| el/terms1000 | generated | 113302.3 | 154765.7 | 1.37x | 153642.9 | 1.36x | -0.7% | 169107 | 169131 |
| el/terms1000 | immediate | 113302.3 | 123622.2 | 1.09x | 123264.3 | 1.09x | -0.3% | 177120 | 177120 |
| el/overloads | generated | 1859.2 | 2559.7 | 1.38x | 2575.6 | 1.39x | +0.6% | 4248 | 4248 |
| el/overloads | immediate | 1859.2 | 2048.4 | 1.10x | 2042.1 | 1.10x | -0.3% | 4200 | 4200 |
| el/string | generated | 278.4 | 965.2 | 3.47x | 958.4 | 3.44x | -0.7% | 1056 | 1056 |
| el/string | immediate | 278.4 | 661.4 | 2.38x | 667.1 | 2.40x | +0.9% | 1032 | 1032 |
| el/interpolation | generated | 1920.4 | 4897.4 | 2.55x | 4782.8 | 2.49x | -2.3% | 2368 | 2368 |
| el/interpolation | immediate | 1920.4 | 4391.7 | 2.29x | 4345.0 | 2.26x | -1.1% | 2424 | 2424 |
| el/untyped | generated | 28776.1 | 29847.5 | 1.04x | 31188.1 | 1.08x | +4.5% | 16424 | 16424 |
| el/refused-early | generated | 470.2 | 1232.8 | 2.62x | 1241.8 | 2.64x | +0.7% | 1064 | 1064 |
| el/refused-early | immediate | 470.2 | 1262.1 | 2.68x | 1241.4 | 2.64x | -1.6% | 1944 | 1944 |
| el/refused-late | generated | 1492.6 | 1863.8 | 1.25x | 1950.8 | 1.31x | +4.7% | 1064 | 1064 |
| el/refused-late | immediate | 1492.6 | 3238.4 | 2.17x | 3238.5 | 2.17x | 0.0% | 3928 | 3928 |
| sql/literal | generated | 64.9 | 149.0 | 2.30x | 149.3 | 2.30x | +0.2% | 160 | 160 |
| sql/comment | generated | 2729.2 | 5258.8 | 1.93x | 5347.0 | 1.96x | +1.7% | 5136 | 5136 |
| sql/conditions100 | generated | 71581.0 | 138454.6 | 1.93x | 143020.8 | 2.00x | +3.3% | 161736 | 161736 |
| sql/conditions1000 | generated | 723438.3 | 1430337.5 | 1.98x | 1448771.1 | 2.00x | +1.3% | 1616136 | 1616136 |
| tsql/comment | generated | 25890.1 | 1622.9 | 0.06x | 1558.9 | 0.06x | -3.9% | 1192 | 1192 |
| sql/column | generated | 182.2 | 389.3 | 2.14x | 389.1 | 2.14x | -0.1% | 416 | 416 |
| sql/arithmetic | generated | 2142.4 | 4586.4 | 2.14x | 4567.9 | 2.13x | -0.4% | 3472 | 3472 |
| sql/nest8 | generated | 3558.2 | 11817.3 | 3.32x | 11872.8 | 3.34x | +0.5% | 6504 | 6504 |
| sql/condition | generated | 2508.5 | 4974.1 | 1.98x | 5000.9 | 1.99x | +0.5% | 4928 | 4928 |
| sql/select1 | generated | 835.1 | 1670.4 | 2.00x | 1768.4 | 2.12x | +5.9% | 1688 | 1688 |
| sql/select20 | generated | 9950.8 | 20726.3 | 2.08x | 21149.6 | 2.13x | +2.0% | 21448 | 21448 |
| sql/values | generated | 640.0 | 1872.2 | 2.93x | 1941.6 | 3.03x | +3.7% | 1904 | 1904 |
| sql/create | generated | 955.6 | 2745.4 | 2.87x | 2786.5 | 2.92x | +1.5% | 1568 | 1568 |
| sql/refused-late | generated | 3813.7 | 13980.8 | 3.67x | 14394.5 | 3.77x | +3.0% | 13552 | 13552 |
