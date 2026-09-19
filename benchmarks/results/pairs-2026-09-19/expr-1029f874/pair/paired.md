Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.4, 30.8, 31.4, 31.3, 30.7).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 19:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | 675887.5 | 1161709.4 | 1.72x | 1129878.1 | 1.67x | -2.7% | 113600 | 105600 |
| tsql/script100 | generated | 656293.0 | 1156633.6 | 1.76x | 1165937.5 | 1.78x | +0.8% | 113600 | 113600 |
| tsql/script400.bool | generated | 2644496.9 | 17053890.6 | 6.45x | 16809984.4 | 6.36x | -1.4% | 454400 | 422403 |
| tsql/script400 | generated | 2665096.9 | 17074603.1 | 6.41x | 16782553.1 | 6.30x | -1.7% | 454400 | 454400 |
| tsql/columns1000 | generated | 1725045.3 | 189985.9 | 0.11x | 187989.1 | 0.11x | -1.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | 867970.3 | 333370.3 | 0.38x | 334195.3 | 0.39x | +0.2% | 424424 | 424424 |
| tsql/rows1000 | generated | 576586.7 | 242102.3 | 0.42x | 246231.2 | 0.43x | +1.7% | 296472 | 296472 |
| web/json.array10000 | generated | 170860.2 | 201986.1 | 1.18x | 196816.8 | 1.15x | -2.6% | 720048 | 720048 |
| web/json.object10000 | generated | 845379.7 | 1222958.6 | 1.45x | 1352159.4 | 1.60x | +10.6% | 1599240 | 1599240 |
| web/url.path1000 | generated | 9414.6 | 10819.3 | 1.15x | 10791.2 | 1.15x | -0.3% | 8384 | 8384 |
| web/media-type.params1000 | generated | 46823.8 | 46473.7 | 0.99x | 46230.5 | 0.99x | -0.5% | 151472 | 151472 |
| web/sf.list10000 | generated | 1112243.8 | 1124190.6 | 1.01x | 1145159.4 | 1.03x | +1.9% | 2720056 | 2720056 |
| el/refused-early.bool | generated | 330.2 | 1085.2 | 3.29x | 555.8 | 1.68x | -48.8% | 1064 | 624 |
| el/refused-late.bool | generated | 1179.3 | 1725.8 | 1.46x | 899.3 | 0.76x | -47.9% | 1064 | 624 |
| el/ladder.bool | generated | 1008.7 | 1739.7 | 1.72x | 1697.6 | 1.68x | -2.4% | 1776 | 1720 |
| sql/refused-late.bool | generated | 2469.8 | 12238.9 | 4.96x | 5704.2 | 2.31x | -53.4% | 13552 | 6616 |
| sql/select20.bool | generated | 6558.6 | 17625.5 | 2.69x | 17755.5 | 2.71x | +0.7% | 21448 | 21392 |
| web/url.plain | generated | 93.8 | 214.2 | 2.28x | 215.3 | 2.30x | +0.5% | 360 | 360 |
| web/url.full | generated | 163.9 | 291.2 | 1.78x | 284.2 | 1.73x | -2.4% | 536 | 536 |
| web/url.ipv4 | generated | 109.8 | 224.8 | 2.05x | 225.4 | 2.05x | +0.3% | 384 | 384 |
| web/url.long-path | generated | 173.3 | 404.6 | 2.34x | 409.2 | 2.36x | +1.2% | 512 | 512 |
| web/url.refused | generated | 67.0 | 250.9 | 3.74x | 253.2 | 3.78x | +0.9% | 64 | 64 |
| web/json.object | generated | 574.0 | 910.1 | 1.59x | 899.3 | 1.57x | -1.2% | 2584 | 2584 |
| web/json.array | generated | 573.2 | 711.5 | 1.24x | 711.8 | 1.24x | 0.0% | 2400 | 2400 |
| web/cookie.full | generated | 297.1 | 336.2 | 1.13x | 337.7 | 1.14x | +0.5% | 2136 | 2136 |
| web/cookie.short | generated | 51.8 | 77.7 | 1.50x | 78.0 | 1.51x | +0.4% | 336 | 336 |
| web/pointer.full | generated | 202.8 | 194.9 | 0.96x | 196.1 | 0.97x | +0.6% | 440 | 440 |
| web/pointer.short | generated | 35.8 | 58.0 | 1.62x | 58.2 | 1.63x | +0.4% | 144 | 144 |
| web/media-type.plain | generated | 160.6 | 189.9 | 1.18x | 199.2 | 1.24x | +4.9% | 448 | 448 |
| web/media-type.quoted | generated | 256.2 | 289.8 | 1.13x | 287.3 | 1.12x | -0.9% | 816 | 816 |
| web/media-type.refused | generated | 61.4 | 92.6 | 1.51x | 92.5 | 1.51x | 0.0% | 64 | 64 |
| web/addr-spec.plain | generated | 89.4 | 116.2 | 1.30x | 114.1 | 1.28x | -1.8% | 176 | 176 |
| web/addr-spec.refused | generated | 45.2 | 77.2 | 1.71x | 76.6 | 1.69x | -0.8% | 64 | 64 |
| web/date-time.utc | generated | 50.6 | 66.1 | 1.31x | 65.7 | 1.30x | -0.7% | 176 | 176 |
| web/date-time.offset | generated | 77.6 | 93.9 | 1.21x | 94.5 | 1.22x | +0.6% | 208 | 208 |
| web/language-tag.plain | generated | 113.8 | 152.2 | 1.34x | 155.1 | 1.36x | +1.9% | 360 | 360 |
| web/language-tag.full | generated | 398.4 | 469.3 | 1.18x | 479.2 | 1.20x | +2.1% | 720 | 720 |
| web/language-tag.refused | generated | 109.9 | 148.9 | 1.35x | 150.3 | 1.37x | +0.9% | 64 | 64 |
| web/date-time.refused | generated | 36.0 | 51.5 | 1.43x | 51.8 | 1.44x | +0.6% | 96 | 96 |
| web/sf.item | generated | 263.0 | 292.2 | 1.11x | 287.7 | 1.09x | -1.6% | 800 | 800 |
| web/sf.list | generated | 1144.5 | 1222.8 | 1.07x | 1206.2 | 1.05x | -1.4% | 3064 | 3064 |
| web/sf.dictionary | generated | 1191.0 | 1220.0 | 1.02x | 1224.6 | 1.03x | +0.4% | 3280 | 3280 |
| el/floor | generated | 301.7 | 808.1 | 2.68x | 793.9 | 2.63x | -1.8% | 1104 | 1104 |
| el/floor | immediate | 301.7 | 468.2 | 1.55x | 466.8 | 1.55x | -0.3% | 1120 | 1120 |
| el/ladder | generated | 997.3 | 1718.0 | 1.72x | 1727.9 | 1.73x | +0.6% | 1776 | 1776 |
| el/ladder | immediate | 997.3 | 1157.8 | 1.16x | 1169.2 | 1.17x | +1.0% | 1784 | 1784 |
| el/nest7 | generated | 625.5 | 1691.4 | 2.70x | 1695.6 | 2.71x | +0.3% | 1152 | 1152 |
| el/nest7 | immediate | 625.5 | 1036.2 | 1.66x | 1050.6 | 1.68x | +1.4% | 1120 | 1120 |
| el/block | generated | 1047.5 | 1790.5 | 1.71x | 1804.9 | 1.72x | +0.8% | 2344 | 2344 |
| el/block | immediate | 1047.5 | 1152.3 | 1.10x | 1150.4 | 1.10x | -0.2% | 2440 | 2440 |
| el/loop | generated | 2214.3 | 3698.0 | 1.67x | 3637.0 | 1.64x | -1.7% | 4776 | 4776 |
| el/loop | immediate | 2214.3 | 2482.5 | 1.12x | 2500.6 | 1.13x | +0.7% | 4880 | 4880 |
| el/terms100 | generated | 11148.7 | 16008.2 | 1.44x | 15905.8 | 1.43x | -0.6% | 17904 | 17904 |
| el/terms100 | immediate | 11148.7 | 12514.3 | 1.12x | 12575.8 | 1.13x | +0.5% | 18720 | 18720 |
| el/terms1000 | generated | 108277.4 | 151392.3 | 1.40x | 151785.0 | 1.40x | +0.3% | 169128 | 169128 |
| el/terms1000 | immediate | 108277.4 | 120431.8 | 1.11x | 121473.6 | 1.12x | +0.9% | 177120 | 177120 |
| el/overloads | generated | 1825.5 | 2526.8 | 1.38x | 2547.6 | 1.40x | +0.8% | 4248 | 4248 |
| el/overloads | immediate | 1825.5 | 1995.9 | 1.09x | 2015.7 | 1.10x | +1.0% | 4200 | 4200 |
| el/string | generated | 277.1 | 968.5 | 3.49x | 944.1 | 3.41x | -2.5% | 1056 | 1056 |
| el/string | immediate | 277.1 | 665.7 | 2.40x | 678.2 | 2.45x | +1.9% | 1032 | 1032 |
| el/interpolation | generated | 1917.8 | 4520.7 | 2.36x | 4806.4 | 2.51x | +6.3% | 2368 | 2368 |
| el/interpolation | immediate | 1917.8 | 4121.4 | 2.15x | 4133.3 | 2.16x | +0.3% | 2424 | 2424 |
| el/untyped | generated | 29129.5 | 32827.2 | 1.13x | 30498.9 | 1.05x | -7.1% | 16424 | 16424 |
| el/refused-early | generated | 480.5 | 1249.1 | 2.60x | 1306.4 | 2.72x | +4.6% | 1064 | 1064 |
| el/refused-early | immediate | 480.5 | 1271.6 | 2.65x | 1257.4 | 2.62x | -1.1% | 1944 | 1944 |
| el/refused-late | generated | 1581.3 | 1922.8 | 1.22x | 2042.1 | 1.29x | +6.2% | 1064 | 1064 |
| el/refused-late | immediate | 1581.3 | 3265.9 | 2.07x | 3286.2 | 2.08x | +0.6% | 3928 | 3928 |
| sql/literal | generated | 69.5 | 145.9 | 2.10x | 145.3 | 2.09x | -0.4% | 160 | 160 |
| sql/comment | generated | 2786.0 | 5326.0 | 1.91x | 5238.1 | 1.88x | -1.7% | 5136 | 5136 |
| sql/conditions100 | generated | 73635.1 | 141566.7 | 1.92x | 139444.2 | 1.89x | -1.5% | 161736 | 161736 |
| sql/conditions1000 | generated | 753905.5 | 1413792.2 | 1.88x | 1429971.9 | 1.90x | +1.1% | 1616136 | 1616203 |
| tsql/comment | generated | 25805.2 | 1563.2 | 0.06x | 1566.1 | 0.06x | +0.2% | 1192 | 1192 |
| sql/column | generated | 191.4 | 385.2 | 2.01x | 392.6 | 2.05x | +1.9% | 392 | 392 |
| sql/arithmetic | generated | 2100.5 | 4487.9 | 2.14x | 4528.6 | 2.16x | +0.9% | 3472 | 3472 |
| sql/nest8 | generated | 3503.1 | 11804.8 | 3.37x | 11589.0 | 3.31x | -1.8% | 6504 | 6504 |
| sql/condition | generated | 2538.3 | 4810.1 | 1.90x | 4898.3 | 1.93x | +1.8% | 4928 | 4928 |
| sql/select1 | generated | 829.3 | 1659.4 | 2.00x | 1748.1 | 2.11x | +5.3% | 1688 | 1688 |
| sql/select20 | generated | 9990.2 | 21204.6 | 2.12x | 21360.4 | 2.14x | +0.7% | 21448 | 21448 |
| sql/values | generated | 659.1 | 1909.4 | 2.90x | 1954.5 | 2.97x | +2.4% | 1904 | 1904 |
| sql/create | generated | 965.2 | 2778.2 | 2.88x | 2785.3 | 2.89x | +0.3% | 1568 | 1568 |
| sql/refused-late | generated | 3761.7 | 13913.9 | 3.70x | 14124.7 | 3.75x | +1.5% | 13552 | 13552 |
