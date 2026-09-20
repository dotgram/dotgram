# Paired stand, 2026-09-20 02:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit 7f59a2d2, framework net10.0, no properties, emitted 5a3915f90c8fe919). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 73.6 | 77.6 | 1.05x | 79.8 | 1.08x | +2.9% | 192 | 192 |
| fix/Order.text | generated | hand | 656.8 | 636.2 | 0.97x | 643.1 | 0.98x | +1.1% | 1096 | 1096 |
| tsql/script100.bool | generated | scriptdom | 659612.5 | 123231.2 | 0.19x | 124084.4 | 0.19x | +0.7% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 656839.8 | 124050.8 | 0.19x | 122841.4 | 0.19x | -1.0% | 105603 | 105600 |
| tsql/script100 | generated | scriptdom | 654752.3 | 124832.8 | 0.19x | 123517.2 | 0.19x | -1.1% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2643381.2 | 500790.6 | 0.19x | 491731.2 | 0.19x | -1.8% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2670362.5 | 495918.8 | 0.19x | 492793.8 | 0.18x | -0.6% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2616668.8 | 500778.1 | 0.19x | 495243.8 | 0.19x | -1.1% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1737912.5 | 189190.6 | 0.11x | 186798.4 | 0.11x | -1.3% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 888585.9 | 333162.5 | 0.37x | 334664.8 | 0.38x | +0.5% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 583052.3 | 244456.2 | 0.42x | 243053.9 | 0.42x | -0.6% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 621075.0 | 489900.0 | 0.79x | 1245257.0 | 2.01x | +154.2% | 1599240 | 1599240 |
| sql/select20.at | generated | hand | 7268.8 | 17513.0 | 2.41x | 17651.1 | 2.43x | +0.8% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7295.8 | 17911.5 | 2.46x | 17927.0 | 2.46x | +0.1% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8188.6 | 1160.3 | 0.14x | 1232.1 | 0.15x | +6.2% | 1328 | 1328 |
| sql/select20.scan | generated | control | 370.4 | 379.5 | 1.02x | 377.6 | 1.02x | -0.5% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3509.9 | 3561.3 | 1.01x | 3557.2 | 1.01x | -0.1% | 0 | 0 |
| tsql/select20.scan | generated | control | 378.5 | 377.2 | 1.00x | 377.9 | 1.00x | +0.2% | 0 | 0 |
| el/ladder.scan | generated | control | 141.6 | 136.4 | 0.96x | 138.5 | 0.98x | +1.6% | 0 | 0 |
| el/refused-early.bool | generated | hand | 330.5 | 1096.7 | 3.32x | 550.2 | 1.66x | -49.8% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1182.5 | 1752.8 | 1.48x | 902.5 | 0.76x | -48.5% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1005.5 | 1746.7 | 1.74x | 1700.5 | 1.69x | -2.6% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2656.4 | 12343.2 | 4.65x | 5604.8 | 2.11x | -54.6% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7079.6 | 18299.7 | 2.58x | 18044.2 | 2.55x | -1.4% | 21448 | 21392 |
| web/url.full | generated | hand | 154.1 | 278.2 | 1.80x | 267.2 | 1.73x | -4.0% | 536 | 536 |
| web/json.object | generated | hand | 576.5 | 920.5 | 1.60x | 915.0 | 1.59x | -0.6% | 2584 | 2584 |
| web/media-type.plain | generated | control | 156.7 | 184.7 | 1.18x | 188.1 | 1.20x | +1.9% | 448 | 448 |
| el/floor | generated | hand | 313.1 | 809.6 | 2.59x | 785.6 | 2.51x | -3.0% | 1104 | 1104 |
| el/floor | immediate | hand | 313.1 | 457.8 | 1.46x | 470.3 | 1.50x | +2.7% | 1120 | 1120 |
| el/ladder | generated | hand | 1026.8 | 1764.3 | 1.72x | 1714.0 | 1.67x | -2.9% | 1776 | 1779 |
| el/ladder | immediate | hand | 1026.8 | 1172.9 | 1.14x | 1157.7 | 1.13x | -1.3% | 1784 | 1784 |
| el/nest7 | generated | hand | 645.3 | 1700.5 | 2.64x | 1672.4 | 2.59x | -1.7% | 1152 | 1152 |
| el/nest7 | immediate | hand | 645.3 | 1028.5 | 1.59x | 1024.4 | 1.59x | -0.4% | 1120 | 1120 |
| el/block | generated | hand | 1052.3 | 1817.9 | 1.73x | 1789.4 | 1.70x | -1.6% | 2344 | 2344 |
| el/block | immediate | hand | 1052.3 | 1144.4 | 1.09x | 1152.9 | 1.10x | +0.7% | 2440 | 2440 |
| el/try | generated | hand | 3356.2 | 4788.1 | 1.43x | 4789.6 | 1.43x | 0.0% | 4456 | 4456 |
| el/try | immediate | hand | 3356.2 | 3312.9 | 0.99x | 3297.8 | 0.98x | -0.5% | 4152 | 4152 |
| el/loop | generated | hand | 2255.3 | 3748.2 | 1.66x | 3669.1 | 1.63x | -2.1% | 4776 | 4776 |
| el/loop | immediate | hand | 2255.3 | 2551.9 | 1.13x | 2558.3 | 1.13x | +0.3% | 4880 | 4880 |
| el/terms100 | generated | hand | 11259.1 | 16569.1 | 1.47x | 15901.0 | 1.41x | -4.0% | 17907 | 17904 |
| el/terms100 | immediate | hand | 11259.1 | 12871.1 | 1.14x | 12979.7 | 1.15x | +0.8% | 18720 | 18720 |
| el/terms1000 | generated | hand | 109407.4 | 157250.3 | 1.44x | 151410.7 | 1.38x | -3.7% | 169104 | 169107 |
| el/terms1000 | immediate | hand | 109407.4 | 123859.2 | 1.13x | 123909.3 | 1.13x | 0.0% | 177120 | 177120 |
| el/overloads | generated | hand | 1843.6 | 2604.6 | 1.41x | 2591.1 | 1.41x | -0.5% | 4248 | 4248 |
| el/overloads | immediate | hand | 1843.6 | 2029.6 | 1.10x | 2042.4 | 1.11x | +0.6% | 4200 | 4200 |
| el/string | generated | hand | 272.2 | 959.5 | 3.52x | 951.7 | 3.50x | -0.8% | 1056 | 1056 |
| el/string | immediate | hand | 272.2 | 669.6 | 2.46x | 664.7 | 2.44x | -0.7% | 1032 | 1032 |
| el/interpolation | generated | hand | 1779.3 | 4696.4 | 2.64x | 4792.9 | 2.69x | +2.1% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1779.3 | 4075.3 | 2.29x | 4105.7 | 2.31x | +0.7% | 2424 | 2424 |
| el/untyped | generated | hand | 20630.7 | 19244.5 | 0.93x | 22339.4 | 1.08x | +16.1% | 16424 | 16424 |
| el/refused-early | generated | hand | 470.0 | 1254.9 | 2.67x | 1222.3 | 2.60x | -2.6% | 1064 | 1064 |
| el/refused-early | immediate | hand | 470.0 | 1205.3 | 2.56x | 1200.8 | 2.56x | -0.4% | 1944 | 1944 |
| el/refused-late | generated | hand | 1443.5 | 1843.7 | 1.28x | 1930.7 | 1.34x | +4.7% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1443.5 | 3110.6 | 2.15x | 3140.3 | 2.18x | +1.0% | 3928 | 3928 |
| sql/literal | generated | hand | 62.7 | 152.2 | 2.43x | 148.9 | 2.38x | -2.2% | 160 | 160 |
| sql/comment | generated | hand | 2724.4 | 5313.0 | 1.95x | 5098.2 | 1.87x | -4.0% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 68997.4 | 135012.9 | 1.96x | 129433.9 | 1.88x | -4.1% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 744363.3 | 1381573.4 | 1.86x | 1310764.8 | 1.76x | -5.1% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 24922.3 | 1619.3 | 0.06x | 1598.0 | 0.06x | -1.3% | 1192 | 1192 |
| sql/column | generated | hand | 178.7 | 385.5 | 2.16x | 407.8 | 2.28x | +5.8% | 392 | 392 |
| sql/arithmetic | generated | hand | 2055.7 | 4388.7 | 2.13x | 4386.9 | 2.13x | 0.0% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3353.0 | 11851.6 | 3.53x | 10739.9 | 3.20x | -9.4% | 6504 | 6504 |
| sql/condition | generated | hand | 2413.5 | 4791.1 | 1.99x | 4512.8 | 1.87x | -5.8% | 4928 | 4928 |
| sql/select1 | generated | hand | 824.3 | 1783.1 | 2.16x | 1745.4 | 2.12x | -2.1% | 1688 | 1688 |
| sql/select20 | generated | hand | 9678.3 | 20255.9 | 2.09x | 20782.3 | 2.15x | +2.6% | 21448 | 21448 |
| sql/values | generated | hand | 616.9 | 1922.7 | 3.12x | 1874.6 | 3.04x | -2.5% | 1904 | 1904 |
| sql/create | generated | hand | 938.6 | 2758.4 | 2.94x | 2719.9 | 2.90x | -1.4% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3658.8 | 13824.5 | 3.78x | 13149.1 | 3.59x | -4.9% | 13552 | 13552 |
