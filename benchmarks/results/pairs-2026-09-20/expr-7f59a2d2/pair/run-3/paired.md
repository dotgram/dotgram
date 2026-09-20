# Paired stand, 2026-09-20 02:01

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit 7f59a2d2, framework net10.0, no properties, emitted 5a3915f90c8fe919). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 80.5 | 82.7 | 1.03x | 82.1 | 1.02x | -0.7% | 192 | 192 |
| fix/Order.text | generated | hand | 713.1 | 651.5 | 0.91x | 657.1 | 0.92x | +0.9% | 1096 | 1096 |
| tsql/script100.bool | generated | scriptdom | 757065.6 | 124478.1 | 0.16x | 128965.6 | 0.17x | +3.6% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 731539.1 | 124328.1 | 0.17x | 124545.3 | 0.17x | +0.2% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 711040.6 | 122817.2 | 0.17x | 128020.3 | 0.18x | +4.2% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2872984.4 | 490753.1 | 0.17x | 511909.4 | 0.18x | +4.3% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2865375.0 | 490259.4 | 0.17x | 521256.2 | 0.18x | +6.3% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2942884.4 | 500640.6 | 0.17x | 505656.2 | 0.17x | +1.0% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1866618.8 | 189032.8 | 0.10x | 191631.2 | 0.10x | +1.4% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 1104268.8 | 355149.2 | 0.32x | 361050.0 | 0.33x | +1.7% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 735558.6 | 252750.8 | 0.34x | 249259.4 | 0.34x | -1.4% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 797353.1 | 1442082.8 | 1.81x | 1028615.6 | 1.29x | -28.7% | 1599240 | 1599240 |
| sql/select20.at | generated | hand | 7408.3 | 18048.3 | 2.44x | 18395.4 | 2.48x | +1.9% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7623.8 | 18395.4 | 2.41x | 18533.2 | 2.43x | +0.7% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 9419.5 | 1185.9 | 0.13x | 1240.8 | 0.13x | +4.6% | 1328 | 1328 |
| sql/select20.scan | generated | control | 382.6 | 387.5 | 1.01x | 378.3 | 0.99x | -2.4% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3606.8 | 3669.9 | 1.02x | 3598.5 | 1.00x | -1.9% | 0 | 0 |
| tsql/select20.scan | generated | control | 397.2 | 396.7 | 1.00x | 387.5 | 0.98x | -2.3% | 0 | 0 |
| el/ladder.scan | generated | control | 142.8 | 143.7 | 1.01x | 141.2 | 0.99x | -1.8% | 0 | 0 |
| el/refused-early.bool | generated | hand | 357.2 | 1118.4 | 3.13x | 557.7 | 1.56x | -50.1% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1225.5 | 1819.2 | 1.48x | 923.5 | 0.75x | -49.2% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1052.6 | 1823.0 | 1.73x | 1779.0 | 1.69x | -2.4% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2815.5 | 12786.2 | 4.54x | 5861.7 | 2.08x | -54.2% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7534.2 | 18841.4 | 2.50x | 18593.2 | 2.47x | -1.3% | 21448 | 21392 |
| web/url.full | generated | hand | 166.6 | 298.2 | 1.79x | 279.7 | 1.68x | -6.2% | 536 | 536 |
| web/json.object | generated | hand | 605.5 | 966.3 | 1.60x | 986.7 | 1.63x | +2.1% | 2584 | 2584 |
| web/media-type.plain | generated | control | 166.2 | 197.6 | 1.19x | 218.0 | 1.31x | +10.4% | 448 | 448 |
| el/floor | generated | hand | 337.4 | 837.6 | 2.48x | 817.3 | 2.42x | -2.4% | 1104 | 1104 |
| el/floor | immediate | hand | 337.4 | 484.3 | 1.44x | 528.1 | 1.57x | +9.1% | 1120 | 1120 |
| el/ladder | generated | hand | 1053.5 | 1844.1 | 1.75x | 1801.4 | 1.71x | -2.3% | 1776 | 1776 |
| el/ladder | immediate | hand | 1053.5 | 1200.3 | 1.14x | 1252.3 | 1.19x | +4.3% | 1784 | 1784 |
| el/nest7 | generated | hand | 681.5 | 1776.8 | 2.61x | 1730.3 | 2.54x | -2.6% | 1152 | 1152 |
| el/nest7 | immediate | hand | 681.5 | 1049.0 | 1.54x | 1086.2 | 1.59x | +3.6% | 1120 | 1120 |
| el/block | generated | hand | 1102.2 | 1904.1 | 1.73x | 1910.4 | 1.73x | +0.3% | 2344 | 2344 |
| el/block | immediate | hand | 1102.2 | 1196.4 | 1.09x | 1228.6 | 1.11x | +2.7% | 2440 | 2440 |
| el/try | generated | hand | 3450.4 | 5039.4 | 1.46x | 5004.7 | 1.45x | -0.7% | 4456 | 4456 |
| el/try | immediate | hand | 3450.4 | 3497.4 | 1.01x | 3632.2 | 1.05x | +3.9% | 4152 | 4152 |
| el/loop | generated | hand | 2320.1 | 3894.8 | 1.68x | 3796.5 | 1.64x | -2.5% | 4776 | 4776 |
| el/loop | immediate | hand | 2320.1 | 2681.4 | 1.16x | 2641.7 | 1.14x | -1.5% | 4880 | 4880 |
| el/terms100 | generated | hand | 11818.7 | 17034.8 | 1.44x | 16444.8 | 1.39x | -3.5% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11818.7 | 13027.6 | 1.10x | 13224.4 | 1.12x | +1.5% | 18720 | 18720 |
| el/terms1000 | generated | hand | 115291.8 | 165632.1 | 1.44x | 157871.9 | 1.37x | -4.7% | 169129 | 169129 |
| el/terms1000 | immediate | hand | 115291.8 | 126325.4 | 1.10x | 125531.8 | 1.09x | -0.6% | 177120 | 177120 |
| el/overloads | generated | hand | 1861.7 | 2644.6 | 1.42x | 2600.9 | 1.40x | -1.7% | 4248 | 4248 |
| el/overloads | immediate | hand | 1861.7 | 2027.5 | 1.09x | 2135.3 | 1.15x | +5.3% | 4200 | 4200 |
| el/string | generated | hand | 293.9 | 980.4 | 3.34x | 958.4 | 3.26x | -2.2% | 1056 | 1056 |
| el/string | immediate | hand | 293.9 | 680.3 | 2.32x | 689.0 | 2.34x | +1.3% | 1032 | 1032 |
| el/interpolation | generated | hand | 1882.4 | 5104.9 | 2.71x | 5036.5 | 2.68x | -1.3% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1882.4 | 4604.4 | 2.45x | 4505.8 | 2.39x | -2.1% | 2424 | 2424 |
| el/untyped | generated | hand | 18472.1 | 27983.8 | 1.51x | 19862.1 | 1.08x | -29.0% | 16424 | 16424 |
| el/refused-early | generated | hand | 485.8 | 1294.9 | 2.67x | 1269.7 | 2.61x | -1.9% | 1064 | 1064 |
| el/refused-early | immediate | hand | 485.8 | 1278.4 | 2.63x | 1339.4 | 2.76x | +4.8% | 1944 | 1944 |
| el/refused-late | generated | hand | 1360.3 | 1886.4 | 1.39x | 1958.1 | 1.44x | +3.8% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1360.3 | 2930.8 | 2.15x | 3071.8 | 2.26x | +4.8% | 3928 | 3928 |
| sql/literal | generated | hand | 59.6 | 137.4 | 2.31x | 139.5 | 2.34x | +1.5% | 160 | 160 |
| sql/comment | generated | hand | 2502.0 | 5046.7 | 2.02x | 4861.8 | 1.94x | -3.7% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 57303.4 | 125052.0 | 2.18x | 118740.5 | 2.07x | -5.0% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 586189.1 | 1264558.6 | 2.16x | 1210980.1 | 2.07x | -4.2% | 1616136 | 1616203 |
| tsql/comment | generated | scriptdom | 29666.9 | 1547.9 | 0.05x | 1579.0 | 0.05x | +2.0% | 1192 | 1192 |
| sql/column | generated | hand | 163.5 | 355.0 | 2.17x | 369.3 | 2.26x | +4.0% | 392 | 416 |
| sql/arithmetic | generated | hand | 1805.5 | 4421.6 | 2.45x | 4389.3 | 2.43x | -0.7% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3056.6 | 11621.8 | 3.80x | 10470.5 | 3.43x | -9.9% | 6504 | 6504 |
| sql/condition | generated | hand | 2017.1 | 4464.2 | 2.21x | 4156.4 | 2.06x | -6.9% | 4928 | 4928 |
| sql/select1 | generated | hand | 728.7 | 1552.6 | 2.13x | 1545.4 | 2.12x | -0.5% | 1688 | 1688 |
| sql/select20 | generated | hand | 8020.2 | 18845.8 | 2.35x | 18515.4 | 2.31x | -1.8% | 21448 | 21448 |
| sql/values | generated | hand | 511.5 | 1766.7 | 3.45x | 1756.9 | 3.43x | -0.6% | 1904 | 1904 |
| sql/create | generated | hand | 832.2 | 2594.8 | 3.12x | 2713.9 | 3.26x | +4.6% | 1568 | 1568 |
| sql/refused-late | generated | hand | 2955.6 | 12664.2 | 4.28x | 11999.4 | 4.06x | -5.2% | 13552 | 13552 |
