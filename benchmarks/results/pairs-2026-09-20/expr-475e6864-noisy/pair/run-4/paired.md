# Paired stand, 2026-09-20 00:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1274450.0 | 169643.8 | 0.13x | 170628.1 | 0.13x | +0.6% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 694852.3 | 130215.6 | 0.19x | 144515.6 | 0.21x | +11.0% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 709544.5 | 127103.1 | 0.18x | 134677.3 | 0.19x | +6.0% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2697646.9 | 498112.5 | 0.18x | 521643.8 | 0.19x | +4.7% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2903656.2 | 496828.1 | 0.17x | 532428.1 | 0.18x | +7.2% | 422403 | 422403 |
| tsql/script400 | generated | scriptdom | 2810709.4 | 501453.1 | 0.18x | 539340.6 | 0.19x | +7.6% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1767003.1 | 192690.6 | 0.11x | 189814.1 | 0.11x | -1.5% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 892318.8 | 339121.9 | 0.38x | 341960.2 | 0.38x | +0.8% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 605696.9 | 248524.2 | 0.41x | 243208.6 | 0.40x | -2.1% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 896060.2 | 1269398.4 | 1.42x | 1439123.4 | 1.61x | +13.4% | 1599240 | 1599240 |
| web/sf.list10000 | generated | control | 1169187.5 | 1107862.5 | 0.95x | 1104975.0 | 0.95x | -0.3% | 2720056 | 2720056 |
| sql/select20.at | generated | hand | 7762.8 | 18232.5 | 2.35x | 18131.6 | 2.34x | -0.6% | 21441 | 21441 |
| sql/select20.window | generated | hand | 7324.6 | 18821.1 | 2.57x | 18845.4 | 2.57x | +0.1% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8266.4 | 1140.0 | 0.14x | 1205.9 | 0.15x | +5.8% | 1328 | 1328 |
| sql/select20.scan | generated | control | 405.3 | 390.8 | 0.96x | 390.0 | 0.96x | -0.2% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3549.8 | 3615.7 | 1.02x | 3599.7 | 1.01x | -0.4% | 0 | 0 |
| tsql/select20.scan | generated | control | 376.3 | 385.0 | 1.02x | 380.6 | 1.01x | -1.2% | 0 | 0 |
| el/ladder.scan | generated | control | 141.7 | 140.2 | 0.99x | 138.9 | 0.98x | -0.9% | 0 | 0 |
| el/refused-early.bool | generated | hand | 346.6 | 1081.8 | 3.12x | 583.7 | 1.68x | -46.0% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1195.8 | 1773.5 | 1.48x | 940.6 | 0.79x | -47.0% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1048.7 | 1784.2 | 1.70x | 1777.6 | 1.70x | -0.4% | 1776 | 1723 |
| sql/refused-late.bool | generated | hand | 2767.7 | 12471.7 | 4.51x | 6051.3 | 2.19x | -51.5% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7471.0 | 18570.7 | 2.49x | 18780.8 | 2.51x | +1.1% | 21448 | 21392 |
| web/url.full | generated | hand | 158.1 | 270.5 | 1.71x | 278.0 | 1.76x | +2.8% | 536 | 536 |
| web/json.object | generated | hand | 580.3 | 951.8 | 1.64x | 949.3 | 1.64x | -0.3% | 2584 | 2584 |
| web/media-type.plain | generated | control | 162.4 | 194.8 | 1.20x | 199.2 | 1.23x | +2.3% | 448 | 448 |
| web/sf.list | generated | control | 1216.6 | 1280.2 | 1.05x | 1249.7 | 1.03x | -2.4% | 3064 | 3064 |
| el/floor | generated | hand | 319.4 | 805.6 | 2.52x | 802.0 | 2.51x | -0.5% | 1104 | 1104 |
| el/floor | immediate | hand | 319.4 | 467.4 | 1.46x | 479.3 | 1.50x | +2.6% | 1120 | 1120 |
| el/ladder | generated | hand | 1023.8 | 1757.4 | 1.72x | 1755.6 | 1.71x | -0.1% | 1776 | 1776 |
| el/ladder | immediate | hand | 1023.8 | 1188.3 | 1.16x | 1161.0 | 1.13x | -2.3% | 1784 | 1784 |
| el/nest7 | generated | hand | 662.5 | 1732.6 | 2.62x | 1756.1 | 2.65x | +1.4% | 1152 | 1152 |
| el/nest7 | immediate | hand | 662.5 | 1076.4 | 1.62x | 1082.1 | 1.63x | +0.5% | 1120 | 1120 |
| el/block | generated | hand | 1892.5 | 3107.3 | 1.64x | 2801.4 | 1.48x | -9.8% | 2344 | 2344 |
| el/block | immediate | hand | 1892.5 | 1850.8 | 0.98x | 1970.6 | 1.04x | +6.5% | 2440 | 2440 |
| el/try | generated | hand | 5791.5 | 10499.9 | 1.81x | 9985.7 | 1.72x | -4.9% | 5632 | 5632 |
| el/try | immediate | hand | 5791.5 | 7660.2 | 1.32x | 7680.3 | 1.33x | +0.3% | 5752 | 5752 |
| el/loop | generated | hand | 3928.3 | 6512.9 | 1.66x | 6229.0 | 1.59x | -4.4% | 4776 | 4776 |
| el/loop | immediate | hand | 3928.3 | 4521.7 | 1.15x | 4566.5 | 1.16x | +1.0% | 4880 | 4880 |
| el/terms100 | generated | hand | 20403.8 | 26858.4 | 1.32x | 27810.0 | 1.36x | +3.5% | 17904 | 17904 |
| el/terms100 | immediate | hand | 20403.8 | 22513.2 | 1.10x | 22652.2 | 1.11x | +0.6% | 18720 | 18720 |
| el/terms1000 | generated | hand | 196101.2 | 249383.2 | 1.27x | 243698.8 | 1.24x | -2.3% | 169104 | 169107 |
| el/terms1000 | immediate | hand | 196101.2 | 192333.4 | 0.98x | 213851.4 | 1.09x | +11.2% | 177120 | 177120 |
| el/overloads | generated | hand | 3348.7 | 4464.5 | 1.33x | 4394.7 | 1.31x | -1.6% | 4248 | 4248 |
| el/overloads | immediate | hand | 3348.7 | 3559.9 | 1.06x | 3640.7 | 1.09x | +2.3% | 4200 | 4200 |
| el/string | generated | hand | 445.8 | 1409.1 | 3.16x | 1493.0 | 3.35x | +6.0% | 1056 | 1056 |
| el/string | immediate | hand | 445.8 | 995.9 | 2.23x | 978.6 | 2.20x | -1.7% | 1032 | 1032 |
| el/interpolation | generated | hand | 2652.5 | 6231.2 | 2.35x | 5888.8 | 2.22x | -5.5% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2652.5 | 6153.7 | 2.32x | 6300.4 | 2.38x | +2.4% | 2424 | 2424 |
| el/untyped | generated | hand | 40460.2 | 37875.1 | 0.94x | 36528.2 | 0.90x | -3.6% | 16424 | 16424 |
| el/refused-early | generated | hand | 447.6 | 1187.8 | 2.65x | 1228.1 | 2.74x | +3.4% | 1064 | 1064 |
| el/refused-early | immediate | hand | 447.6 | 1193.0 | 2.67x | 1169.9 | 2.61x | -1.9% | 1944 | 1944 |
| el/refused-late | generated | hand | 1420.3 | 1878.9 | 1.32x | 1913.4 | 1.35x | +1.8% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1420.3 | 3111.7 | 2.19x | 3106.4 | 2.19x | -0.2% | 3928 | 3928 |
| sql/literal | generated | hand | 62.9 | 141.7 | 2.25x | 138.2 | 2.20x | -2.5% | 160 | 160 |
| sql/comment | generated | hand | 2875.3 | 5265.2 | 1.83x | 5497.6 | 1.91x | +4.4% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 65886.3 | 140018.2 | 2.13x | 137228.1 | 2.08x | -2.0% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 690637.5 | 1379415.6 | 2.00x | 1412709.4 | 2.05x | +2.4% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 24123.2 | 1537.3 | 0.06x | 1648.5 | 0.07x | +7.2% | 1192 | 1192 |
| sql/column | generated | hand | 181.3 | 400.7 | 2.21x | 364.0 | 2.01x | -9.2% | 392 | 392 |
| sql/arithmetic | generated | hand | 2002.3 | 4666.2 | 2.33x | 4463.8 | 2.23x | -4.3% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3347.1 | 12956.4 | 3.87x | 11651.2 | 3.48x | -10.1% | 6504 | 6504 |
| sql/condition | generated | hand | 2263.3 | 4747.9 | 2.10x | 4667.9 | 2.06x | -1.7% | 4928 | 4928 |
| sql/select1 | generated | hand | 830.3 | 1727.1 | 2.08x | 1645.1 | 1.98x | -4.7% | 1688 | 1688 |
| sql/select20 | generated | hand | 9401.6 | 20668.4 | 2.20x | 20774.5 | 2.21x | +0.5% | 21448 | 21448 |
| sql/values | generated | hand | 635.4 | 1905.1 | 3.00x | 1895.4 | 2.98x | -0.5% | 1904 | 1904 |
| sql/create | generated | hand | 926.9 | 2781.6 | 3.00x | 2846.3 | 3.07x | +2.3% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3409.7 | 13490.5 | 3.96x | 13621.9 | 4.00x | +1.0% | 13552 | 13552 |
