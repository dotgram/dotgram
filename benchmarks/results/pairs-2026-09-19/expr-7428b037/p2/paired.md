Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.6, 31.3, 31.2, 31.3, 30.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 12:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | 2651343.8 | 528506.2 | 0.20x | 549762.5 | 0.21x | +4.0% | 200489 | 200489 |
| tsql/conditions1000 | generated | 958836.7 | 1097392.2 | 1.14x | 1087498.4 | 1.13x | -0.9% | 424449 | 424449 |
| tsql/rows1000 | generated | 650600.0 | 838190.6 | 1.29x | 832069.5 | 1.28x | -0.7% | 296497 | 296497 |
| web/json.array10000 | generated | 174035.9 | 189142.0 | 1.09x | 193788.5 | 1.11x | +2.5% | 720048 | 720048 |
| web/json.object10000 | generated | 842381.2 | 1240160.2 | 1.47x | 1204082.0 | 1.43x | -2.9% | 1599240 | 1599240 |
| web/url.path1000 | generated | 9373.4 | 10844.7 | 1.16x | 10802.1 | 1.15x | -0.4% | 8384 | 8384 |
| web/media-type.params1000 | generated | 48081.5 | 46990.8 | 0.98x | 46756.0 | 0.97x | -0.5% | 151472 | 151472 |
| web/sf.list10000 | generated | 1153904.7 | 1181673.4 | 1.02x | 1212695.3 | 1.05x | +2.6% | 2720056 | 2720056 |
| feeds/stock-count.small.text | generated | 168.6 | 222.0 | 1.32x | 216.4 | 1.28x | -2.6% | 608 | 608 |
| feeds/stock-count.small.reader | generated | 263.6 | 528.8 | 2.01x | 520.2 | 1.97x | -1.6% | 867 | 867 |
| feeds/stock-count.small.reader64 | generated | 178.1 | 533.5 | 3.00x | 525.8 | 2.95x | -1.4% | 824 | 824 |
| feeds/stock-count.good.text | generated | 37328.3 | 25852.5 | 0.69x | 26120.8 | 0.70x | +1.0% | 111403 | 111403 |
| feeds/stock-count.good.reader | generated | 41177.4 | 52353.3 | 1.27x | 45992.3 | 1.12x | -12.2% | 111571 | 111571 |
| feeds/stock-count.good.reader64 | generated | 42333.0 | 51571.1 | 1.22x | 46025.8 | 1.09x | -10.8% | 111619 | 111619 |
| feeds/stock-count.broken.text | generated | 36227.8 | 29745.0 | 0.82x | 29449.6 | 0.81x | -1.0% | 107499 | 107499 |
| feeds/stock-count.broken.reader | generated | 39443.5 | 57125.1 | 1.45x | 53494.5 | 1.36x | -6.4% | 107667 | 107667 |
| feeds/stock-count.broken.reader64 | generated | 40625.4 | 57405.8 | 1.41x | 53034.1 | 1.31x | -7.6% | 107715 | 107715 |
| web/url.plain | generated | 95.3 | 223.1 | 2.34x | 223.9 | 2.35x | +0.3% | 360 | 360 |
| web/url.full | generated | 165.0 | 293.9 | 1.78x | 296.5 | 1.80x | +0.9% | 536 | 536 |
| web/url.ipv4 | generated | 110.4 | 231.6 | 2.10x | 232.9 | 2.11x | +0.6% | 384 | 384 |
| web/url.long-path | generated | 176.1 | 419.7 | 2.38x | 419.1 | 2.38x | -0.1% | 512 | 512 |
| web/url.refused | generated | 68.3 | 600.1 | 8.78x | 609.2 | 8.92x | +1.5% | 152 | 152 |
| web/json.object | generated | 585.6 | 932.6 | 1.59x | 936.3 | 1.60x | +0.4% | 2584 | 2584 |
| web/json.array | generated | 591.9 | 722.1 | 1.22x | 734.6 | 1.24x | +1.7% | 2400 | 2400 |
| web/cookie.full | generated | 314.5 | 347.2 | 1.10x | 348.1 | 1.11x | +0.3% | 2136 | 2136 |
| web/cookie.short | generated | 56.4 | 82.5 | 1.46x | 82.6 | 1.47x | +0.1% | 336 | 336 |
| web/pointer.full | generated | 207.5 | 200.9 | 0.97x | 201.1 | 0.97x | +0.1% | 440 | 440 |
| web/pointer.short | generated | 38.3 | 58.1 | 1.52x | 57.5 | 1.50x | -1.0% | 144 | 144 |
| web/media-type.plain | generated | 171.6 | 204.4 | 1.19x | 203.6 | 1.19x | -0.4% | 448 | 448 |
| web/media-type.quoted | generated | 265.9 | 297.4 | 1.12x | 298.2 | 1.12x | +0.2% | 816 | 816 |
| web/media-type.refused | generated | 137.8 | 166.8 | 1.21x | 168.0 | 1.22x | +0.7% | 64 | 64 |
| web/addr-spec.plain | generated | 93.4 | 115.5 | 1.24x | 114.5 | 1.23x | -0.9% | 176 | 176 |
| web/addr-spec.refused | generated | 120.0 | 146.8 | 1.22x | 146.6 | 1.22x | -0.2% | 152 | 152 |
| web/date-time.utc | generated | 38.8 | 88.8 | 2.29x | 87.8 | 2.26x | -1.0% | 176 | 176 |
| web/date-time.offset | generated | 67.0 | 115.6 | 1.73x | 114.7 | 1.71x | -0.8% | 208 | 208 |
| web/language-tag.plain | generated | 119.5 | 156.0 | 1.31x | 158.2 | 1.32x | +1.4% | 360 | 360 |
| web/language-tag.full | generated | 452.5 | 497.5 | 1.10x | 500.1 | 1.11x | +0.5% | 720 | 720 |
| web/language-tag.refused | generated | 283.0 | 331.9 | 1.17x | 325.1 | 1.15x | -2.0% | 152 | 152 |
| web/sf.item | generated | 254.0 | 323.3 | 1.27x | 312.4 | 1.23x | -3.4% | 800 | 800 |
| web/sf.list | generated | 1170.2 | 1289.4 | 1.10x | 1264.4 | 1.08x | -1.9% | 3064 | 3064 |
| web/sf.dictionary | generated | 1143.2 | 1261.5 | 1.10x | 1236.6 | 1.08x | -2.0% | 3280 | 3280 |
| el/floor | generated | 327.5 | 939.2 | 2.87x | 904.8 | 2.76x | -3.7% | 1104 | 1104 |
| el/floor | immediate | 327.5 | 488.4 | 1.49x | 468.9 | 1.43x | -4.0% | 1120 | 1120 |
| el/ladder | generated | 994.7 | 1924.8 | 1.94x | 1901.6 | 1.91x | -1.2% | 1776 | 1776 |
| el/ladder | immediate | 994.7 | 1165.2 | 1.17x | 1166.2 | 1.17x | +0.1% | 1784 | 1784 |
| el/nest7 | generated | 739.9 | 1821.4 | 2.46x | 1786.1 | 2.41x | -1.9% | 1152 | 1152 |
| el/nest7 | immediate | 739.9 | 1065.9 | 1.44x | 1049.7 | 1.42x | -1.5% | 1120 | 1120 |
| el/block | generated | 1062.4 | 2042.9 | 1.92x | 2018.5 | 1.90x | -1.2% | 2344 | 2344 |
| el/block | immediate | 1062.4 | 1174.4 | 1.11x | 1187.2 | 1.12x | +1.1% | 2440 | 2440 |
| el/loop | generated | 2280.8 | 4160.9 | 1.82x | 4113.9 | 1.80x | -1.1% | 4776 | 4776 |
| el/loop | immediate | 2280.8 | 2593.5 | 1.14x | 2574.7 | 1.13x | -0.7% | 4880 | 4880 |
| el/terms100 | generated | 11573.7 | 16580.7 | 1.43x | 16684.3 | 1.44x | +0.6% | 17904 | 17904 |
| el/terms100 | immediate | 11573.7 | 12578.9 | 1.09x | 12605.3 | 1.09x | +0.2% | 18720 | 18720 |
| el/terms1000 | generated | 112698.2 | 156290.7 | 1.39x | 158129.3 | 1.40x | +1.2% | 169107 | 169128 |
| el/terms1000 | immediate | 112698.2 | 121300.6 | 1.08x | 121465.6 | 1.08x | +0.1% | 177120 | 177120 |
| el/overloads | generated | 1825.0 | 2768.1 | 1.52x | 2794.9 | 1.53x | +1.0% | 4248 | 4248 |
| el/overloads | immediate | 1825.0 | 2053.1 | 1.12x | 2034.8 | 1.11x | -0.9% | 4200 | 4200 |
| el/string | generated | 280.7 | 1070.3 | 3.81x | 1041.6 | 3.71x | -2.7% | 1056 | 1056 |
| el/string | immediate | 280.7 | 675.8 | 2.41x | 673.2 | 2.40x | -0.4% | 1032 | 1032 |
| el/interpolation | generated | 2013.6 | 4822.2 | 2.39x | 4832.5 | 2.40x | +0.2% | 2368 | 2368 |
| el/interpolation | immediate | 2013.6 | 4195.7 | 2.08x | 4176.0 | 2.07x | -0.5% | 2424 | 2424 |
| el/untyped | generated | 19047.9 | 19024.9 | 1.00x | 21135.4 | 1.11x | +11.1% | 16424 | 16424 |
| el/refused-early | generated | 509.3 | 1541.2 | 3.03x | 1500.3 | 2.95x | -2.7% | 1064 | 1064 |
| el/refused-early | immediate | 509.3 | 1279.8 | 2.51x | 1294.6 | 2.54x | +1.2% | 1944 | 1944 |
| el/refused-late | generated | 1630.1 | 2176.2 | 1.34x | 2237.3 | 1.37x | +2.8% | 1064 | 1064 |
| el/refused-late | immediate | 1630.1 | 3310.1 | 2.03x | 3296.7 | 2.02x | -0.4% | 3928 | 3928 |
| sql/conditions100 | generated | 75626.6 | 446058.0 | 5.90x | 438116.2 | 5.79x | -1.8% | 161736 | 161736 |
| sql/conditions1000 | generated | 751433.6 | 4417912.5 | 5.88x | 4421269.5 | 5.88x | +0.1% | 1616203 | 1616203 |
| tsql/comment | generated | 27184.4 | 4181.6 | 0.15x | 4094.0 | 0.15x | -2.1% | 1192 | 1192 |
| sql/column | generated | 200.2 | 1194.5 | 5.97x | 1207.2 | 6.03x | +1.1% | 392 | 392 |
| sql/arithmetic | generated | 2289.1 | 13837.1 | 6.04x | 13619.0 | 5.95x | -1.6% | 3472 | 3472 |
| sql/select20 | generated | 10914.3 | 77444.1 | 7.10x | 78252.3 | 7.17x | +1.0% | 21448 | 21448 |
| sql/refused-late | generated | 3783.5 | 46732.6 | 12.35x | 46131.2 | 12.19x | -1.3% | 13552 | 13552 |
