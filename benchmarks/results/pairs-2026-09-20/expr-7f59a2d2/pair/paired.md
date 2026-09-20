Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.6, 31.1, 31.6, 31.2, 31.3).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 02:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit 7f59a2d2, framework net10.0, no properties, emitted 5a3915f90c8fe919). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 76.4 | 79.1 | 1.04x | 80.5 | 1.05x | +1.7% | 192 | 192 |
| fix/Order.text | generated | hand | 668.3 | 636.2 | 0.95x | 643.1 | 0.96x | +1.1% | 1096 | 1096 |
| tsql/script100.bool | generated | scriptdom | 659612.5 | 124478.1 | 0.19x | 124084.4 | 0.19x | -0.3% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 666025.0 | 124362.5 | 0.19x | 124545.3 | 0.19x | +0.1% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 667227.3 | 124832.8 | 0.19x | 127629.7 | 0.19x | +2.2% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2730903.1 | 500790.6 | 0.18x | 511909.4 | 0.19x | +2.2% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2690478.1 | 496975.0 | 0.18x | 512090.6 | 0.19x | +3.0% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2643615.6 | 500778.1 | 0.19x | 505656.2 | 0.19x | +1.0% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1747587.5 | 189190.6 | 0.11x | 190020.3 | 0.11x | +0.4% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 896674.2 | 338072.7 | 0.38x | 342889.8 | 0.38x | +1.4% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 596283.6 | 248438.3 | 0.42x | 245244.5 | 0.41x | -1.3% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 754882.0 | 1300251.6 | 1.72x | 1305500.8 | 1.73x | +0.4% | 1599240 | 1599240 |
| sql/select20.at | generated | hand | 7442.8 | 17851.5 | 2.40x | 17651.1 | 2.37x | -1.1% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7364.2 | 18346.2 | 2.49x | 18533.2 | 2.52x | +1.0% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8452.5 | 1185.9 | 0.14x | 1240.8 | 0.15x | +4.6% | 1328 | 1328 |
| sql/select20.scan | generated | control | 382.6 | 387.5 | 1.01x | 379.4 | 0.99x | -2.1% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3606.8 | 3650.4 | 1.01x | 3581.4 | 0.99x | -1.9% | 0 | 0 |
| tsql/select20.scan | generated | control | 383.0 | 397.7 | 1.04x | 388.6 | 1.01x | -2.3% | 0 | 0 |
| el/ladder.scan | generated | control | 141.6 | 141.5 | 1.00x | 142.5 | 1.01x | +0.7% | 0 | 0 |
| el/refused-early.bool | generated | hand | 356.5 | 1118.8 | 3.14x | 557.7 | 1.56x | -50.2% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1217.8 | 1802.6 | 1.48x | 923.5 | 0.76x | -48.8% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1040.1 | 1795.1 | 1.73x | 1759.5 | 1.69x | -2.0% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2747.9 | 12451.7 | 4.53x | 5794.4 | 2.11x | -53.5% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7407.5 | 18393.0 | 2.48x | 18257.5 | 2.46x | -0.7% | 21448 | 21392 |
| web/url.full | generated | hand | 162.4 | 293.6 | 1.81x | 279.7 | 1.72x | -4.7% | 536 | 536 |
| web/json.object | generated | hand | 605.5 | 966.3 | 1.60x | 944.5 | 1.56x | -2.3% | 2584 | 2584 |
| web/media-type.plain | generated | control | 160.8 | 194.1 | 1.21x | 198.0 | 1.23x | +2.0% | 448 | 448 |
| el/floor | generated | hand | 323.4 | 809.6 | 2.50x | 811.7 | 2.51x | +0.3% | 1104 | 1104 |
| el/floor | immediate | hand | 323.4 | 463.2 | 1.43x | 470.3 | 1.45x | +1.5% | 1120 | 1120 |
| el/ladder | generated | hand | 1038.5 | 1822.4 | 1.75x | 1800.9 | 1.73x | -1.2% | 1776 | 1776 |
| el/ladder | immediate | hand | 1038.5 | 1172.9 | 1.13x | 1200.4 | 1.16x | +2.3% | 1784 | 1784 |
| el/nest7 | generated | hand | 681.5 | 1748.0 | 2.56x | 1730.3 | 2.54x | -1.0% | 1152 | 1152 |
| el/nest7 | immediate | hand | 681.5 | 1049.0 | 1.54x | 1086.2 | 1.59x | +3.6% | 1120 | 1120 |
| el/block | generated | hand | 1095.2 | 1884.7 | 1.72x | 1868.0 | 1.71x | -0.9% | 2344 | 2344 |
| el/block | immediate | hand | 1095.2 | 1196.4 | 1.09x | 1209.7 | 1.10x | +1.1% | 2440 | 2440 |
| el/try | generated | hand | 3431.2 | 5011.1 | 1.46x | 4900.8 | 1.43x | -2.2% | 4456 | 4456 |
| el/try | immediate | hand | 3431.2 | 3497.4 | 1.02x | 3464.9 | 1.01x | -0.9% | 4152 | 4152 |
| el/loop | generated | hand | 2320.1 | 3873.7 | 1.67x | 3796.5 | 1.64x | -2.0% | 4776 | 4776 |
| el/loop | immediate | hand | 2320.1 | 2681.4 | 1.16x | 2641.7 | 1.14x | -1.5% | 4880 | 4880 |
| el/terms100 | generated | hand | 11781.9 | 17034.8 | 1.45x | 16444.8 | 1.40x | -3.5% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11781.9 | 13027.6 | 1.11x | 13102.7 | 1.11x | +0.6% | 18720 | 18720 |
| el/terms1000 | generated | hand | 113906.3 | 163934.7 | 1.44x | 155493.2 | 1.37x | -5.1% | 169128 | 169128 |
| el/terms1000 | immediate | hand | 113906.3 | 126325.4 | 1.11x | 125531.8 | 1.10x | -0.6% | 177120 | 177120 |
| el/overloads | generated | hand | 1881.6 | 2644.6 | 1.41x | 2610.2 | 1.39x | -1.3% | 4248 | 4248 |
| el/overloads | immediate | hand | 1881.6 | 2055.4 | 1.09x | 2042.4 | 1.09x | -0.6% | 4200 | 4200 |
| el/string | generated | hand | 293.9 | 980.4 | 3.34x | 958.4 | 3.26x | -2.2% | 1056 | 1056 |
| el/string | immediate | hand | 293.9 | 672.8 | 2.29x | 681.3 | 2.32x | +1.3% | 1032 | 1032 |
| el/interpolation | generated | hand | 1892.2 | 5104.9 | 2.70x | 4964.2 | 2.62x | -2.8% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1892.2 | 4259.5 | 2.25x | 4505.8 | 2.38x | +5.8% | 2424 | 2424 |
| el/untyped | generated | hand | 20630.7 | 23645.1 | 1.15x | 22818.4 | 1.11x | -3.5% | 16424 | 16424 |
| el/refused-early | generated | hand | 486.0 | 1284.1 | 2.64x | 1269.7 | 2.61x | -1.1% | 1064 | 1064 |
| el/refused-early | immediate | hand | 486.0 | 1258.9 | 2.59x | 1258.8 | 2.59x | 0.0% | 1944 | 1944 |
| el/refused-late | generated | hand | 1453.0 | 1886.4 | 1.30x | 1958.1 | 1.35x | +3.8% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1453.0 | 3237.0 | 2.23x | 3275.5 | 2.25x | +1.2% | 3928 | 3928 |
| sql/literal | generated | hand | 66.4 | 152.2 | 2.29x | 153.2 | 2.31x | +0.7% | 160 | 160 |
| sql/comment | generated | hand | 2973.7 | 5406.5 | 1.82x | 5225.2 | 1.76x | -3.4% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 74964.0 | 140916.7 | 1.88x | 134421.1 | 1.79x | -4.6% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 744363.3 | 1441572.7 | 1.94x | 1413912.5 | 1.90x | -1.9% | 1616136 | 1616203 |
| tsql/comment | generated | scriptdom | 27345.9 | 1619.3 | 0.06x | 1617.1 | 0.06x | -0.1% | 1192 | 1192 |
| sql/column | generated | hand | 189.0 | 407.8 | 2.16x | 410.1 | 2.17x | +0.6% | 392 | 416 |
| sql/arithmetic | generated | hand | 2133.1 | 4551.4 | 2.13x | 4412.0 | 2.07x | -3.1% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3616.0 | 12018.9 | 3.32x | 11027.8 | 3.05x | -8.2% | 6504 | 6504 |
| sql/condition | generated | hand | 2545.5 | 4963.8 | 1.95x | 4798.7 | 1.89x | -3.3% | 4928 | 4928 |
| sql/select1 | generated | hand | 923.8 | 1783.1 | 1.93x | 1747.1 | 1.89x | -2.0% | 1688 | 1688 |
| sql/select20 | generated | hand | 10558.1 | 21798.1 | 2.06x | 21501.6 | 2.04x | -1.4% | 21448 | 21448 |
| sql/values | generated | hand | 679.6 | 1978.9 | 2.91x | 1951.6 | 2.87x | -1.4% | 1904 | 1904 |
| sql/create | generated | hand | 1045.9 | 2760.1 | 2.64x | 2807.8 | 2.68x | +1.7% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3964.7 | 14530.5 | 3.66x | 13640.1 | 3.44x | -6.1% | 13552 | 13552 |
