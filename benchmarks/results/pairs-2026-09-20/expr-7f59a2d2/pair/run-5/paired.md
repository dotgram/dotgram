# Paired stand, 2026-09-20 02:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit 7f59a2d2, framework net10.0, no properties, emitted 5a3915f90c8fe919). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 76.0 | 78.0 | 1.03x | 80.5 | 1.06x | +3.1% | 192 | 192 |
| fix/Order.text | generated | hand | 668.3 | 614.5 | 0.92x | 607.2 | 0.91x | -1.2% | 1096 | 1096 |
| tsql/script100.bool | generated | scriptdom | 653634.4 | 123175.0 | 0.19x | 120993.8 | 0.19x | -1.8% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 650303.9 | 124362.5 | 0.19x | 128826.6 | 0.20x | +3.6% | 105600 | 105603 |
| tsql/script100 | generated | scriptdom | 652339.1 | 123191.4 | 0.19x | 126540.6 | 0.19x | +2.7% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2607334.4 | 495068.8 | 0.19x | 508671.9 | 0.20x | +2.7% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2593403.1 | 496975.0 | 0.19x | 492078.1 | 0.19x | -1.0% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2592218.8 | 488825.0 | 0.19x | 508815.6 | 0.20x | +4.1% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1710228.1 | 186542.2 | 0.11x | 190020.3 | 0.11x | +1.9% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 872247.7 | 338072.7 | 0.39x | 334728.9 | 0.38x | -1.0% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 584200.8 | 252770.3 | 0.43x | 242511.7 | 0.42x | -4.1% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 754882.0 | 1321268.8 | 1.75x | 1495299.2 | 1.98x | +13.2% | 1599240 | 1599240 |
| sql/select20.at | generated | hand | 7862.4 | 17814.0 | 2.27x | 17573.2 | 2.24x | -1.4% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7559.9 | 18466.4 | 2.44x | 18589.4 | 2.46x | +0.7% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8496.1 | 1272.0 | 0.15x | 1278.5 | 0.15x | +0.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 389.5 | 410.1 | 1.05x | 384.1 | 0.99x | -6.3% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3617.5 | 3763.4 | 1.04x | 3628.9 | 1.00x | -3.6% | 0 | 0 |
| tsql/select20.scan | generated | control | 396.1 | 406.3 | 1.03x | 402.5 | 1.02x | -0.9% | 0 | 0 |
| el/ladder.scan | generated | control | 150.0 | 141.5 | 0.94x | 142.6 | 0.95x | +0.8% | 0 | 0 |
| el/refused-early.bool | generated | hand | 356.5 | 1158.6 | 3.25x | 565.3 | 1.59x | -51.2% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1253.8 | 1871.2 | 1.49x | 926.0 | 0.74x | -50.5% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1083.4 | 1857.5 | 1.71x | 1869.7 | 1.73x | +0.7% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2787.9 | 12451.7 | 4.47x | 5794.4 | 2.08x | -53.5% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7407.5 | 18393.0 | 2.48x | 18359.2 | 2.48x | -0.2% | 21448 | 21392 |
| web/url.full | generated | hand | 168.0 | 327.9 | 1.95x | 304.3 | 1.81x | -7.2% | 536 | 536 |
| web/json.object | generated | hand | 611.5 | 971.7 | 1.59x | 974.7 | 1.59x | +0.3% | 2584 | 2584 |
| web/media-type.plain | generated | control | 174.0 | 215.8 | 1.24x | 211.0 | 1.21x | -2.3% | 448 | 448 |
| el/floor | generated | hand | 341.4 | 850.2 | 2.49x | 841.8 | 2.47x | -1.0% | 1104 | 1104 |
| el/floor | immediate | hand | 341.4 | 497.3 | 1.46x | 497.2 | 1.46x | 0.0% | 1120 | 1120 |
| el/ladder | generated | hand | 1059.5 | 1836.0 | 1.73x | 1820.8 | 1.72x | -0.8% | 1776 | 1776 |
| el/ladder | immediate | hand | 1059.5 | 1217.7 | 1.15x | 1204.8 | 1.14x | -1.1% | 1784 | 1784 |
| el/nest7 | generated | hand | 662.5 | 1748.0 | 2.64x | 1798.8 | 2.72x | +2.9% | 1152 | 1152 |
| el/nest7 | immediate | hand | 662.5 | 1064.4 | 1.61x | 1088.1 | 1.64x | +2.2% | 1120 | 1120 |
| el/block | generated | hand | 1095.2 | 1884.7 | 1.72x | 1868.0 | 1.71x | -0.9% | 2344 | 2344 |
| el/block | immediate | hand | 1095.2 | 1199.9 | 1.10x | 1209.7 | 1.10x | +0.8% | 2440 | 2440 |
| el/try | generated | hand | 3431.2 | 5011.1 | 1.46x | 4900.8 | 1.43x | -2.2% | 4456 | 4456 |
| el/try | immediate | hand | 3431.2 | 3564.6 | 1.04x | 3464.9 | 1.01x | -2.8% | 4152 | 4152 |
| el/loop | generated | hand | 2334.9 | 3873.7 | 1.66x | 3892.7 | 1.67x | +0.5% | 4776 | 4776 |
| el/loop | immediate | hand | 2334.9 | 2722.7 | 1.17x | 2694.6 | 1.15x | -1.0% | 4880 | 4880 |
| el/terms100 | generated | hand | 11781.9 | 17326.0 | 1.47x | 16571.7 | 1.41x | -4.4% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11781.9 | 13508.7 | 1.15x | 13102.7 | 1.11x | -3.0% | 18720 | 18720 |
| el/terms1000 | generated | hand | 113906.3 | 163934.7 | 1.44x | 155493.2 | 1.37x | -5.1% | 169104 | 169104 |
| el/terms1000 | immediate | hand | 113906.3 | 130895.2 | 1.15x | 126010.3 | 1.11x | -3.7% | 177120 | 177123 |
| el/overloads | generated | hand | 1937.9 | 2656.7 | 1.37x | 2638.4 | 1.36x | -0.7% | 4248 | 4248 |
| el/overloads | immediate | hand | 1937.9 | 2116.8 | 1.09x | 2184.9 | 1.13x | +3.2% | 4200 | 4200 |
| el/string | generated | hand | 295.0 | 997.1 | 3.38x | 981.5 | 3.33x | -1.6% | 1056 | 1056 |
| el/string | immediate | hand | 295.0 | 695.8 | 2.36x | 681.3 | 2.31x | -2.1% | 1032 | 1032 |
| el/interpolation | generated | hand | 1892.2 | 5248.2 | 2.77x | 4917.6 | 2.60x | -6.3% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1892.2 | 4259.5 | 2.25x | 3896.1 | 2.06x | -8.5% | 2424 | 2424 |
| el/untyped | generated | hand | 20992.6 | 19870.6 | 0.95x | 23084.5 | 1.10x | +16.2% | 16424 | 16571 |
| el/refused-early | generated | hand | 486.0 | 1284.1 | 2.64x | 1255.5 | 2.58x | -2.2% | 1064 | 1064 |
| el/refused-early | immediate | hand | 486.0 | 1258.9 | 2.59x | 1258.8 | 2.59x | 0.0% | 1944 | 1944 |
| el/refused-late | generated | hand | 1453.0 | 2031.1 | 1.40x | 1764.6 | 1.21x | -13.1% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1453.0 | 3501.9 | 2.41x | 3275.5 | 2.25x | -6.5% | 3928 | 3928 |
| sql/literal | generated | hand | 66.4 | 148.5 | 2.24x | 153.2 | 2.31x | +3.2% | 160 | 160 |
| sql/comment | generated | hand | 2973.7 | 5406.5 | 1.82x | 5225.2 | 1.76x | -3.4% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 74964.0 | 140916.7 | 1.88x | 134421.1 | 1.79x | -4.6% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 711227.3 | 1441572.7 | 2.03x | 1421514.1 | 2.00x | -1.4% | 1616136 | 1616203 |
| tsql/comment | generated | scriptdom | 26885.9 | 1625.2 | 0.06x | 1652.9 | 0.06x | +1.7% | 1192 | 1192 |
| sql/column | generated | hand | 189.0 | 413.2 | 2.19x | 411.8 | 2.18x | -0.3% | 392 | 416 |
| sql/arithmetic | generated | hand | 2133.1 | 4551.4 | 2.13x | 4535.5 | 2.13x | -0.3% | 3496 | 3496 |
| sql/nest8 | generated | hand | 3616.0 | 12018.9 | 3.32x | 11027.8 | 3.05x | -8.2% | 6504 | 6504 |
| sql/condition | generated | hand | 2545.5 | 4963.8 | 1.95x | 4798.7 | 1.89x | -3.3% | 4928 | 4928 |
| sql/select1 | generated | hand | 923.8 | 1722.5 | 1.86x | 1747.1 | 1.89x | +1.4% | 1688 | 1688 |
| sql/select20 | generated | hand | 10558.1 | 21824.4 | 2.07x | 21501.6 | 2.04x | -1.5% | 21448 | 21448 |
| sql/values | generated | hand | 679.6 | 1978.9 | 2.91x | 1951.6 | 2.87x | -1.4% | 1904 | 1904 |
| sql/create | generated | hand | 1045.9 | 2760.1 | 2.64x | 2807.8 | 2.68x | +1.7% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3964.7 | 14530.5 | 3.66x | 13640.1 | 3.44x | -6.1% | 13552 | 13552 |
