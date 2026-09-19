Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.4, 31.2, 31.3, 31.3, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 17:00

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 74.9 | 79.0 | 1.05x | 82.3 | 1.10x | +4.2% | 192 | 192 |
| tsql/columns1000 | generated | 2144075.0 | 470881.2 | 0.22x | 475850.0 | 0.22x | +1.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | 881868.0 | 946706.2 | 1.07x | 944008.6 | 1.07x | -0.3% | 424424 | 424424 |
| tsql/rows1000 | generated | 596985.2 | 730225.8 | 1.22x | 737482.8 | 1.24x | +1.0% | 296472 | 296472 |
| web/json.array10000 | generated | 170640.4 | 199920.3 | 1.17x | 196541.8 | 1.15x | -1.7% | 720048 | 720048 |
| web/json.object10000 | generated | 732646.1 | 1272057.8 | 1.74x | 1283752.3 | 1.75x | +0.9% | 1599240 | 1599240 |
| web/url.path1000 | generated | 9357.2 | 10986.7 | 1.17x | 10923.8 | 1.17x | -0.6% | 8384 | 8384 |
| web/media-type.params1000 | generated | 47143.8 | 47084.7 | 1.00x | 47000.1 | 1.00x | -0.2% | 151472 | 151472 |
| web/sf.list10000 | generated | 1160490.6 | 1182256.2 | 1.02x | 1164753.1 | 1.00x | -1.5% | 2720056 | 2720056 |
| web/url.plain | generated | 96.3 | 221.6 | 2.30x | 221.2 | 2.30x | -0.2% | 360 | 360 |
| web/url.full | generated | 167.1 | 306.1 | 1.83x | 299.5 | 1.79x | -2.2% | 536 | 536 |
| web/url.ipv4 | generated | 112.2 | 234.4 | 2.09x | 233.8 | 2.08x | -0.3% | 384 | 384 |
| web/url.long-path | generated | 175.5 | 415.1 | 2.37x | 419.0 | 2.39x | +0.9% | 512 | 512 |
| web/url.refused | generated | 68.3 | 610.4 | 8.94x | 607.2 | 8.89x | -0.5% | 152 | 152 |
| web/json.object | generated | 586.8 | 923.3 | 1.57x | 932.1 | 1.59x | +1.0% | 2584 | 2584 |
| web/json.array | generated | 595.1 | 719.4 | 1.21x | 736.5 | 1.24x | +2.4% | 2400 | 2400 |
| web/cookie.full | generated | 313.7 | 347.8 | 1.11x | 347.7 | 1.11x | 0.0% | 2136 | 2136 |
| web/cookie.short | generated | 56.9 | 81.8 | 1.44x | 81.5 | 1.43x | -0.4% | 336 | 336 |
| web/pointer.full | generated | 206.1 | 200.3 | 0.97x | 200.8 | 0.97x | +0.2% | 440 | 440 |
| web/pointer.short | generated | 38.1 | 56.5 | 1.48x | 56.4 | 1.48x | -0.2% | 144 | 144 |
| web/media-type.plain | generated | 166.1 | 207.3 | 1.25x | 202.8 | 1.22x | -2.2% | 448 | 448 |
| web/media-type.quoted | generated | 258.1 | 296.1 | 1.15x | 294.2 | 1.14x | -0.7% | 816 | 816 |
| web/media-type.refused | generated | 138.1 | 169.5 | 1.23x | 164.9 | 1.19x | -2.7% | 64 | 64 |
| web/addr-spec.plain | generated | 92.6 | 115.5 | 1.25x | 114.2 | 1.23x | -1.2% | 176 | 176 |
| web/addr-spec.refused | generated | 117.6 | 147.0 | 1.25x | 148.2 | 1.26x | +0.8% | 152 | 152 |
| web/date-time.utc | generated | 39.1 | 89.2 | 2.28x | 87.3 | 2.23x | -2.2% | 176 | 176 |
| web/date-time.offset | generated | 66.8 | 115.5 | 1.73x | 114.1 | 1.71x | -1.2% | 208 | 208 |
| web/language-tag.plain | generated | 116.7 | 155.4 | 1.33x | 153.9 | 1.32x | -1.0% | 360 | 360 |
| web/language-tag.full | generated | 415.0 | 498.5 | 1.20x | 488.2 | 1.18x | -2.1% | 720 | 720 |
| web/language-tag.refused | generated | 277.9 | 326.6 | 1.18x | 333.5 | 1.20x | +2.1% | 152 | 152 |
| web/sf.item | generated | 270.5 | 314.3 | 1.16x | 314.3 | 1.16x | 0.0% | 800 | 800 |
| web/sf.list | generated | 1215.4 | 1282.9 | 1.06x | 1290.0 | 1.06x | +0.6% | 3064 | 3064 |
| web/sf.dictionary | generated | 1183.8 | 1259.1 | 1.06x | 1233.7 | 1.04x | -2.0% | 3280 | 3280 |
| el/floor | generated | 349.7 | 921.7 | 2.64x | 917.1 | 2.62x | -0.5% | 1104 | 1104 |
| el/floor | immediate | 349.7 | 467.4 | 1.34x | 473.3 | 1.35x | +1.3% | 1120 | 1120 |
| el/ladder | generated | 1021.8 | 1940.1 | 1.90x | 1914.4 | 1.87x | -1.3% | 1776 | 1776 |
| el/ladder | immediate | 1021.8 | 1176.3 | 1.15x | 1158.7 | 1.13x | -1.5% | 1784 | 1784 |
| el/nest7 | generated | 756.1 | 1792.7 | 2.37x | 1803.0 | 2.38x | +0.6% | 1152 | 1152 |
| el/nest7 | immediate | 756.1 | 1058.9 | 1.40x | 1042.7 | 1.38x | -1.5% | 1120 | 1120 |
| el/block | generated | 1092.8 | 2032.4 | 1.86x | 2029.6 | 1.86x | -0.1% | 2344 | 2344 |
| el/block | immediate | 1092.8 | 1170.3 | 1.07x | 1172.5 | 1.07x | +0.2% | 2440 | 2440 |
| el/loop | generated | 2335.4 | 4171.2 | 1.79x | 4196.6 | 1.80x | +0.6% | 4776 | 4776 |
| el/loop | immediate | 2335.4 | 2608.0 | 1.12x | 2573.2 | 1.10x | -1.3% | 4880 | 4880 |
| el/terms100 | generated | 11668.4 | 16871.2 | 1.45x | 16751.8 | 1.44x | -0.7% | 17904 | 17904 |
| el/terms100 | immediate | 11668.4 | 12708.8 | 1.09x | 12652.6 | 1.08x | -0.4% | 18720 | 18720 |
| el/terms1000 | generated | 113729.6 | 159348.9 | 1.40x | 160366.9 | 1.41x | +0.6% | 169128 | 169129 |
| el/terms1000 | immediate | 113729.6 | 123871.5 | 1.09x | 122803.3 | 1.08x | -0.9% | 177120 | 177120 |
| el/overloads | generated | 1910.2 | 2845.9 | 1.49x | 2833.8 | 1.48x | -0.4% | 4248 | 4248 |
| el/overloads | immediate | 1910.2 | 2070.3 | 1.08x | 2060.2 | 1.08x | -0.5% | 4200 | 4200 |
| el/string | generated | 293.0 | 1061.1 | 3.62x | 1062.8 | 3.63x | +0.2% | 1056 | 1056 |
| el/string | immediate | 293.0 | 663.9 | 2.27x | 664.3 | 2.27x | +0.1% | 1032 | 1032 |
| el/interpolation | generated | 1975.1 | 5057.1 | 2.56x | 5033.2 | 2.55x | -0.5% | 2368 | 2368 |
| el/interpolation | immediate | 1975.1 | 4092.5 | 2.07x | 3942.5 | 2.00x | -3.7% | 2424 | 2424 |
| el/untyped | generated | 23874.3 | 31752.6 | 1.33x | 26593.4 | 1.11x | -16.2% | 16424 | 16424 |
| el/refused-early | generated | 490.4 | 1484.6 | 3.03x | 1449.2 | 2.96x | -2.4% | 1064 | 1064 |
| el/refused-early | immediate | 490.4 | 1220.2 | 2.49x | 1220.8 | 2.49x | +0.1% | 1944 | 1944 |
| el/refused-late | generated | 1518.8 | 2161.7 | 1.42x | 2180.7 | 1.44x | +0.9% | 1064 | 1064 |
| el/refused-late | immediate | 1518.8 | 3078.0 | 2.03x | 3201.0 | 2.11x | +4.0% | 3928 | 3928 |
| sql/literal | generated | 62.7 | 160.5 | 2.56x | 154.8 | 2.47x | -3.5% | 160 | 160 |
| sql/comment | generated | 2829.8 | 16775.0 | 5.93x | 17307.7 | 6.12x | +3.2% | 5136 | 5136 |
| sql/conditions100 | generated | 72489.6 | 417729.1 | 5.76x | 417177.0 | 5.75x | -0.1% | 161736 | 161736 |
| sql/conditions1000 | generated | 733759.4 | 4213382.0 | 5.74x | 4177987.5 | 5.69x | -0.8% | 1616136 | 1616179 |
| tsql/comment | generated | 25941.3 | 3676.1 | 0.14x | 3631.7 | 0.14x | -1.2% | 1216 | 1216 |
| sql/column | generated | 185.2 | 1200.7 | 6.48x | 1165.6 | 6.29x | -2.9% | 392 | 392 |
| sql/arithmetic | generated | 2123.8 | 13137.9 | 6.19x | 13017.9 | 6.13x | -0.9% | 3472 | 3472 |
| sql/nest8 | generated | 3423.7 | 34521.0 | 10.08x | 34799.6 | 10.16x | +0.8% | 6504 | 6504 |
| sql/condition | generated | 2518.9 | 13960.1 | 5.54x | 13858.2 | 5.50x | -0.7% | 4928 | 4928 |
| sql/select1 | generated | 839.4 | 5718.9 | 6.81x | 5735.4 | 6.83x | +0.3% | 1688 | 1688 |
| sql/select20 | generated | 9939.7 | 73480.7 | 7.39x | 72567.9 | 7.30x | -1.2% | 21448 | 21448 |
| sql/values | generated | 651.9 | 6331.7 | 9.71x | 6418.0 | 9.85x | +1.4% | 1904 | 1904 |
| sql/create | generated | 969.4 | 4812.7 | 4.96x | 4680.0 | 4.83x | -2.8% | 1568 | 1568 |
| sql/refused-late | generated | 3751.8 | 43361.8 | 11.56x | 43322.8 | 11.55x | -0.1% | 13552 | 13552 |
