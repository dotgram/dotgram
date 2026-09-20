# Paired stand, 2026-09-20 01:53

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit 7f59a2d2, framework net10.0, no properties, emitted 5a3915f90c8fe919). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 77.4 | 79.1 | 1.02x | 83.7 | 1.08x | +5.8% | 192 | 192 |
| fix/Order.text | generated | hand | 1376.9 | 1111.7 | 0.81x | 1131.6 | 0.82x | +1.8% | 1096 | 1096 |
| tsql/script100.bool | generated | scriptdom | 1068881.2 | 199325.0 | 0.19x | 199212.5 | 0.19x | -0.1% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 1042081.2 | 187889.1 | 0.18x | 189358.6 | 0.18x | +0.8% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 1030266.4 | 186354.7 | 0.18x | 185533.6 | 0.18x | -0.4% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 4082696.9 | 749237.5 | 0.18x | 750668.8 | 0.18x | +0.2% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 4124043.8 | 742771.9 | 0.18x | 745940.6 | 0.18x | +0.4% | 422403 | 422403 |
| tsql/script400 | generated | scriptdom | 2643615.6 | 520925.0 | 0.20x | 500615.6 | 0.19x | -3.9% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1747587.5 | 190112.5 | 0.11x | 188065.6 | 0.11x | -1.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 896674.2 | 331979.7 | 0.37x | 342889.8 | 0.38x | +3.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 604503.9 | 248438.3 | 0.41x | 251061.7 | 0.42x | +1.1% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 683128.1 | 1151978.1 | 1.69x | 1383857.8 | 2.03x | +20.1% | 1599240 | 1599240 |
| sql/select20.at | generated | hand | 7442.8 | 17851.5 | 2.40x | 17482.1 | 2.35x | -2.1% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7254.4 | 18346.2 | 2.53x | 18596.4 | 2.56x | +1.4% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8452.5 | 1286.5 | 0.15x | 1283.6 | 0.15x | -0.2% | 1328 | 1328 |
| sql/select20.scan | generated | control | 381.7 | 383.8 | 1.01x | 383.1 | 1.00x | -0.2% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3549.8 | 3650.4 | 1.03x | 3581.4 | 1.01x | -1.9% | 0 | 0 |
| tsql/select20.scan | generated | control | 383.0 | 415.4 | 1.08x | 388.6 | 1.01x | -6.4% | 0 | 0 |
| el/ladder.scan | generated | control | 139.3 | 138.9 | 1.00x | 145.3 | 1.04x | +4.6% | 0 | 0 |
| el/refused-early.bool | generated | hand | 363.3 | 1126.4 | 3.10x | 570.2 | 1.57x | -49.4% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1217.8 | 1802.6 | 1.48x | 944.1 | 0.78x | -47.6% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1040.1 | 1795.1 | 1.73x | 1759.5 | 1.69x | -2.0% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2747.9 | 12550.4 | 4.57x | 5825.0 | 2.12x | -53.6% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7580.4 | 18720.5 | 2.47x | 18257.5 | 2.41x | -2.5% | 21448 | 21392 |
| web/url.full | generated | hand | 162.4 | 293.6 | 1.81x | 294.9 | 1.82x | +0.5% | 536 | 536 |
| web/json.object | generated | hand | 616.7 | 1005.3 | 1.63x | 944.5 | 1.53x | -6.0% | 2584 | 2584 |
| web/media-type.plain | generated | control | 160.8 | 194.1 | 1.21x | 198.0 | 1.23x | +2.0% | 448 | 448 |
| el/floor | generated | hand | 323.4 | 799.1 | 2.47x | 811.7 | 2.51x | +1.6% | 1104 | 1104 |
| el/floor | immediate | hand | 323.4 | 463.2 | 1.43x | 465.5 | 1.44x | +0.5% | 1120 | 1120 |
| el/ladder | generated | hand | 1038.5 | 1822.4 | 1.75x | 1800.9 | 1.73x | -1.2% | 1776 | 1776 |
| el/ladder | immediate | hand | 1038.5 | 1172.0 | 1.13x | 1200.4 | 1.16x | +2.4% | 1784 | 1784 |
| el/nest7 | generated | hand | 870.9 | 1970.4 | 2.26x | 2086.6 | 2.40x | +5.9% | 1152 | 1152 |
| el/nest7 | immediate | hand | 870.9 | 1082.6 | 1.24x | 1159.3 | 1.33x | +7.1% | 1120 | 1120 |
| el/block | generated | hand | 1673.5 | 2871.0 | 1.72x | 2887.0 | 1.73x | +0.6% | 2344 | 2344 |
| el/block | immediate | hand | 1673.5 | 1836.7 | 1.10x | 1849.6 | 1.11x | +0.7% | 2440 | 2440 |
| el/try | generated | hand | 5422.2 | 7500.0 | 1.38x | 7320.3 | 1.35x | -2.4% | 4464 | 4456 |
| el/try | immediate | hand | 5422.2 | 5397.6 | 1.00x | 5415.1 | 1.00x | +0.3% | 4152 | 4152 |
| el/loop | generated | hand | 3296.5 | 5932.1 | 1.80x | 5181.9 | 1.57x | -12.6% | 4776 | 4776 |
| el/loop | immediate | hand | 3296.5 | 3777.0 | 1.15x | 4009.2 | 1.22x | +6.1% | 4880 | 4880 |
| el/terms100 | generated | hand | 19898.2 | 20433.1 | 1.03x | 19382.9 | 0.97x | -5.1% | 17904 | 17904 |
| el/terms100 | immediate | hand | 19898.2 | 16363.4 | 0.82x | 17471.4 | 0.88x | +6.8% | 18720 | 18720 |
| el/terms1000 | generated | hand | 125967.3 | 166811.3 | 1.32x | 184235.2 | 1.46x | +10.4% | 169131 | 169129 |
| el/terms1000 | immediate | hand | 125967.3 | 144428.9 | 1.15x | 129126.5 | 1.03x | -10.6% | 177120 | 177120 |
| el/overloads | generated | hand | 1881.6 | 2600.5 | 1.38x | 2610.2 | 1.39x | +0.4% | 4248 | 4248 |
| el/overloads | immediate | hand | 1881.6 | 2055.4 | 1.09x | 2040.5 | 1.08x | -0.7% | 4200 | 4200 |
| el/string | generated | hand | 317.7 | 985.6 | 3.10x | 981.0 | 3.09x | -0.5% | 1056 | 1056 |
| el/string | immediate | hand | 317.7 | 672.8 | 2.12x | 688.0 | 2.17x | +2.3% | 1032 | 1032 |
| el/interpolation | generated | hand | 2812.1 | 5404.9 | 1.92x | 5240.1 | 1.86x | -3.0% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2812.1 | 5676.7 | 2.02x | 5694.8 | 2.03x | +0.3% | 2424 | 2424 |
| el/untyped | generated | hand | 19997.5 | 23645.1 | 1.18x | 22818.4 | 1.14x | -3.5% | 16424 | 16424 |
| el/refused-early | generated | hand | 540.1 | 1297.3 | 2.40x | 1329.2 | 2.46x | +2.5% | 1064 | 1064 |
| el/refused-early | immediate | hand | 540.1 | 1356.6 | 2.51x | 1320.9 | 2.45x | -2.6% | 1944 | 1944 |
| el/refused-late | generated | hand | 1747.2 | 1896.8 | 1.09x | 2034.2 | 1.16x | +7.2% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1747.2 | 3513.0 | 2.01x | 3511.8 | 2.01x | 0.0% | 3928 | 3928 |
| sql/literal | generated | hand | 70.1 | 155.9 | 2.22x | 155.7 | 2.22x | -0.1% | 160 | 160 |
| sql/comment | generated | hand | 3273.0 | 5902.5 | 1.80x | 5734.2 | 1.75x | -2.9% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 83144.8 | 157244.9 | 1.89x | 148216.3 | 1.78x | -5.7% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 767214.8 | 1557582.8 | 2.03x | 1454607.8 | 1.90x | -6.6% | 1616203 | 1616136 |
| tsql/comment | generated | scriptdom | 28267.5 | 1645.0 | 0.06x | 1617.1 | 0.06x | -1.7% | 1192 | 1192 |
| sql/column | generated | hand | 235.8 | 415.8 | 1.76x | 412.9 | 1.75x | -0.7% | 416 | 392 |
| sql/arithmetic | generated | hand | 2423.6 | 4719.7 | 1.95x | 4750.3 | 1.96x | +0.6% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3924.5 | 12503.6 | 3.19x | 11217.1 | 2.86x | -10.3% | 6504 | 6504 |
| sql/condition | generated | hand | 2840.9 | 5104.1 | 1.80x | 5049.9 | 1.78x | -1.1% | 4928 | 4928 |
| sql/select1 | generated | hand | 937.5 | 1826.0 | 1.95x | 1771.5 | 1.89x | -3.0% | 1688 | 1688 |
| sql/select20 | generated | hand | 11474.9 | 21798.1 | 1.90x | 22267.2 | 1.94x | +2.2% | 21448 | 21448 |
| sql/values | generated | hand | 737.5 | 2069.9 | 2.81x | 1960.3 | 2.66x | -5.3% | 1904 | 1904 |
| sql/create | generated | hand | 1058.8 | 2868.2 | 2.71x | 2886.1 | 2.73x | +0.6% | 1568 | 1568 |
| sql/refused-late | generated | hand | 4282.6 | 14765.7 | 3.45x | 14051.3 | 3.28x | -4.8% | 13552 | 13552 |
