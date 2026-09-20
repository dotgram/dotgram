# Paired stand, 2026-09-20 09:59

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1045100.0 | 128184.4 | 0.12x | 124834.4 | 0.12x | -2.6% | 113600 | 105600 | 448 % |  |
| tsql/script100.boolboth | generated | scriptdom | 651550.8 | 122660.9 | 0.19x | 120934.4 | 0.19x | -1.4% | 105600 | 105600 | 10 % |  |
| tsql/script100 | generated | scriptdom | 646130.5 | 123597.7 | 0.19x | 121482.0 | 0.19x | -1.7% | 113600 | 113600 | 8 % |  |
| tsql/script400.bool | generated | scriptdom | 2599984.4 | 497853.1 | 0.19x | 485281.2 | 0.19x | -2.5% | 454400 | 422400 | 12 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2591668.8 | 490381.2 | 0.19x | 487643.8 | 0.19x | -0.6% | 422400 | 422403 | 25 % |  |
| tsql/script400 | generated | scriptdom | 2600528.1 | 499393.8 | 0.19x | 494750.0 | 0.19x | -0.9% | 454400 | 454400 | 20 % |  |
| tsql/columns1000 | generated | scriptdom | 1697679.7 | 189862.5 | 0.11x | 185885.9 | 0.11x | -2.1% | 200464 | 200464 | 4 % |  |
| tsql/conditions1000 | generated | scriptdom | 877255.5 | 333198.4 | 0.38x | 334196.9 | 0.38x | +0.3% | 424424 | 424424 | 9 % |  |
| tsql/rows1000 | generated | scriptdom | 582167.2 | 244332.0 | 0.42x | 242941.4 | 0.42x | -0.6% | 296472 | 296472 | 22 % |  |
| sql/select20.at | generated | hand | 7272.5 | 18165.4 | 2.50x | 17809.7 | 2.45x | -2.0% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 7248.9 | 18673.3 | 2.58x | 18079.3 | 2.49x | -3.2% | 21416 | 21416 | 7 % |  |
| tsql/insert-values.at | generated | scriptdom | 8058.4 | 1143.7 | 0.14x | 1193.2 | 0.15x | +4.3% | 1328 | 1328 | 5 % |  |
| sql/select20.scan | generated | control | 378.0 | 378.1 | 1.00x | 375.4 | 0.99x | -0.7% | 0 | 0 | 4 % |  |
| sql/conditions100.scan | generated | control | 3506.1 | 3506.2 | 1.00x | 3489.5 | 1.00x | -0.5% | 0 | 0 | 27 % |  |
| tsql/select20.scan | generated | control | 374.5 | 383.5 | 1.02x | 378.1 | 1.01x | -1.4% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 136.6 | 136.3 | 1.00x | 136.6 | 1.00x | +0.2% | 0 | 0 | 11 % |  |
| el/refused-early.bool | generated | hand | 338.3 | 1069.7 | 3.16x | 554.6 | 1.64x | -48.2% | 1064 | 624 | 28 % |  |
| el/refused-late.bool | generated | hand | 1162.6 | 1721.3 | 1.48x | 899.9 | 0.77x | -47.7% | 1064 | 624 | 3 % |  |
| el/ladder.bool | generated | hand | 1001.9 | 1745.3 | 1.74x | 1693.1 | 1.69x | -3.0% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2662.0 | 12324.5 | 4.63x | 5882.8 | 2.21x | -52.3% | 13552 | 6616 | 11 % |  |
| sql/select20.bool | generated | hand | 7165.5 | 18698.3 | 2.61x | 18069.5 | 2.52x | -3.4% | 21448 | 21392 | 3 % |  |
| el/floor | generated | hand | 323.0 | 792.1 | 2.45x | 803.2 | 2.49x | +1.4% | 1104 | 1104 | 4 % |  |
| el/floor | immediate | hand | 323.0 | 465.4 | 1.44x | 472.9 | 1.46x | +1.6% | 1120 | 1120 | 4 % |  |
| el/ladder | generated | hand | 999.5 | 1749.3 | 1.75x | 1718.9 | 1.72x | -1.7% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 999.5 | 1159.7 | 1.16x | 1207.5 | 1.21x | +4.1% | 1784 | 1784 | 2 % |  |
| el/nest7 | generated | hand | 636.8 | 1712.0 | 2.69x | 1677.5 | 2.63x | -2.0% | 1152 | 1152 | 26 % |  |
| el/nest7 | immediate | hand | 636.8 | 1043.6 | 1.64x | 1052.5 | 1.65x | +0.8% | 1120 | 1120 | 26 % |  |
| el/block | generated | hand | 1032.4 | 1818.6 | 1.76x | 1782.6 | 1.73x | -2.0% | 2344 | 2344 | 2 % |  |
| el/block | immediate | hand | 1032.4 | 1168.7 | 1.13x | 1148.5 | 1.11x | -1.7% | 2440 | 2440 | 2 % |  |
| el/try | generated | hand | 3189.7 | 6148.4 | 1.93x | 6197.8 | 1.94x | +0.8% | 5632 | 5632 | 1 % |  |
| el/try | immediate | hand | 3189.7 | 4784.8 | 1.50x | 4683.9 | 1.47x | -2.1% | 5752 | 5752 | 1 % |  |
| el/loop | generated | hand | 2196.3 | 3738.9 | 1.70x | 3646.0 | 1.66x | -2.5% | 4776 | 4776 | 3 % |  |
| el/loop | immediate | hand | 2196.3 | 2589.9 | 1.18x | 2561.7 | 1.17x | -1.1% | 4880 | 4880 | 3 % |  |
| el/terms100 | generated | hand | 11388.6 | 16125.9 | 1.42x | 15994.6 | 1.40x | -0.8% | 17904 | 17904 | 17 % |  |
| el/terms100 | immediate | hand | 11388.6 | 12876.5 | 1.13x | 12696.5 | 1.11x | -1.4% | 18720 | 18720 | 17 % |  |
| el/terms1000 | generated | hand | 110583.9 | 153304.8 | 1.39x | 152058.1 | 1.38x | -0.8% | 169104 | 169107 | 5 % |  |
| el/terms1000 | immediate | hand | 110583.9 | 124652.7 | 1.13x | 122761.2 | 1.11x | -1.5% | 177120 | 177120 | 5 % |  |
| el/overloads | generated | hand | 1849.9 | 2657.0 | 1.44x | 2608.3 | 1.41x | -1.8% | 4248 | 4248 | 34 % |  |
| el/overloads | immediate | hand | 1849.9 | 2071.5 | 1.12x | 2134.2 | 1.15x | +3.0% | 4200 | 4200 | 34 % |  |
| el/string | generated | hand | 291.5 | 945.3 | 3.24x | 947.5 | 3.25x | +0.2% | 1056 | 1056 | 2 % |  |
| el/string | immediate | hand | 291.5 | 662.0 | 2.27x | 666.3 | 2.29x | +0.6% | 1032 | 1032 | 2 % |  |
| el/interpolation | generated | hand | 2471.7 | 5495.2 | 2.22x | 5072.1 | 2.05x | -7.7% | 2368 | 2368 | 141 % |  |
| el/interpolation | immediate | hand | 2471.7 | 4688.2 | 1.90x | 4778.1 | 1.93x | +1.9% | 2424 | 2424 | 141 % |  |
| el/untyped | generated | hand | 26630.0 | 28406.2 | 1.07x | 28445.0 | 1.07x | +0.1% | 16424 | 16424 | 39 % |  |
| el/refused-early | generated | hand | 479.3 | 1230.2 | 2.57x | 1273.8 | 2.66x | +3.5% | 1064 | 1064 | 29 % |  |
| el/refused-early | immediate | hand | 479.3 | 1378.7 | 2.88x | 1393.5 | 2.91x | +1.1% | 1944 | 1944 | 29 % |  |
| el/refused-late | generated | hand | 1462.1 | 1906.2 | 1.30x | 1960.8 | 1.34x | +2.9% | 1064 | 1064 | 15 % |  |
| el/refused-late | immediate | hand | 1462.1 | 3190.1 | 2.18x | 3192.2 | 2.18x | +0.1% | 3928 | 3928 | 15 % |  |
| sql/literal | generated | hand | 64.5 | 154.5 | 2.40x | 141.3 | 2.19x | -8.5% | 160 | 160 | 16 % |  |
| sql/comment | generated | hand | 2843.0 | 5420.0 | 1.91x | 5310.0 | 1.87x | -2.0% | 5136 | 5136 | 17 % |  |
| sql/conditions100 | generated | hand | 72474.0 | 134491.1 | 1.86x | 137042.3 | 1.89x | +1.9% | 161736 | 161736 | 11 % |  |
| sql/conditions1000 | generated | hand | 722224.2 | 1390964.1 | 1.93x | 1401449.2 | 1.94x | +0.8% | 1616136 | 1616136 | 26 % |  |
| tsql/comment | generated | scriptdom | 25742.0 | 1577.0 | 0.06x | 1563.4 | 0.06x | -0.9% | 1192 | 1192 | 18 % |  |
| sql/column | generated | hand | 183.2 | 393.5 | 2.15x | 397.2 | 2.17x | +1.0% | 392 | 392 | 28 % |  |
| sql/arithmetic | generated | hand | 2169.0 | 4473.6 | 2.06x | 4549.9 | 2.10x | +1.7% | 3472 | 3472 | 26 % |  |
| sql/nest8 | generated | hand | 3445.5 | 11713.7 | 3.40x | 12878.6 | 3.74x | +9.9% | 6504 | 6504 | 13 % |  |
| sql/condition | generated | hand | 2513.9 | 4856.8 | 1.93x | 4923.3 | 1.96x | +1.4% | 4928 | 4928 | 32 % |  |
| sql/select1 | generated | hand | 861.8 | 1749.8 | 2.03x | 1683.8 | 1.95x | -3.8% | 1688 | 1688 | 19 % |  |
| sql/select20 | generated | hand | 10269.0 | 21667.6 | 2.11x | 21210.0 | 2.07x | -2.1% | 21448 | 21448 | 16 % |  |
| sql/values | generated | hand | 647.7 | 1975.8 | 3.05x | 1917.6 | 2.96x | -2.9% | 1904 | 1904 | 15 % |  |
| sql/create | generated | hand | 970.0 | 2735.8 | 2.82x | 2728.1 | 2.81x | -0.3% | 1568 | 1568 | 10 % |  |
| sql/refused-late | generated | hand | 3616.2 | 14143.2 | 3.91x | 13976.3 | 3.86x | -1.2% | 13552 | 13552 | 22 % |  |
