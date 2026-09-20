# Paired stand, 2026-09-20 04:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.9 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 880968.8 | 123887.5 | 0.14x | 130784.4 | 0.15x | +5.6% | 113600 | 105600 | 544 % |  |
| tsql/script100.boolboth | generated | scriptdom | 663911.7 | 122020.3 | 0.18x | 124943.8 | 0.19x | +2.4% | 105600 | 105600 | 16 % |  |
| tsql/script100 | generated | scriptdom | 670197.7 | 124288.3 | 0.19x | 124165.6 | 0.19x | -0.1% | 113600 | 113600 | 12 % |  |
| tsql/script400.bool | generated | scriptdom | 2660881.2 | 499662.5 | 0.19x | 506006.2 | 0.19x | +1.3% | 454400 | 422400 | 13 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2660143.8 | 506700.0 | 0.19x | 493546.9 | 0.19x | -2.6% | 422400 | 422403 | 11 % |  |
| tsql/script400 | generated | scriptdom | 2746196.9 | 502606.2 | 0.18x | 508143.8 | 0.19x | +1.1% | 454400 | 454400 | 52 % |  |
| tsql/columns1000 | generated | scriptdom | 2396028.1 | 316537.5 | 0.13x | 284165.6 | 0.12x | -10.2% | 200464 | 200464 | 49 % |  |
| tsql/conditions1000 | generated | scriptdom | 1262453.9 | 478933.6 | 0.38x | 488424.2 | 0.39x | +2.0% | 424424 | 424424 | 56 % |  |
| tsql/rows1000 | generated | scriptdom | 1006579.7 | 353792.2 | 0.35x | 391445.3 | 0.39x | +10.6% | 296472 | 296472 | 48 % |  |
| sql/select20.at | generated | hand | 11892.0 | 27807.3 | 2.34x | 26368.1 | 2.22x | -5.2% | 21416 | 21416 | 53 % |  |
| sql/select20.window | generated | hand | 8747.2 | 25083.9 | 2.87x | 24309.1 | 2.78x | -3.1% | 21416 | 21416 | 70 % |  |
| tsql/insert-values.at | generated | scriptdom | 14168.2 | 1983.2 | 0.14x | 1991.2 | 0.14x | +0.4% | 1328 | 1328 | 29 % |  |
| sql/select20.scan | generated | control | 573.8 | 560.0 | 0.98x | 570.8 | 0.99x | +1.9% | 0 | 0 | 51 % |  |
| sql/conditions100.scan | generated | control | 6050.9 | 6428.3 | 1.06x | 6170.7 | 1.02x | -4.0% | 0 | 0 | 60 % |  |
| tsql/select20.scan | generated | control | 635.2 | 691.4 | 1.09x | 710.6 | 1.12x | +2.8% | 0 | 0 | 49 % |  |
| el/ladder.scan | generated | control | 262.8 | 235.7 | 0.90x | 229.9 | 0.87x | -2.5% | 0 | 0 | 52 % |  |
| el/refused-early.bool | generated | hand | 605.6 | 1681.8 | 2.78x | 881.6 | 1.46x | -47.6% | 1064 | 624 | 12 % |  |
| el/refused-late.bool | generated | hand | 2136.3 | 2806.1 | 1.31x | 1454.0 | 0.68x | -48.2% | 1064 | 624 | 27 % |  |
| el/ladder.bool | generated | hand | 1222.7 | 2353.6 | 1.92x | 2147.9 | 1.76x | -8.7% | 1776 | 1720 | 56 % |  |
| sql/refused-late.bool | generated | hand | 2982.4 | 12817.7 | 4.30x | 6166.3 | 2.07x | -51.9% | 13552 | 6616 | 57 % |  |
| sql/select20.bool | generated | hand | 7282.8 | 18129.1 | 2.49x | 18409.6 | 2.53x | +1.5% | 21448 | 21392 | 13 % |  |
| el/floor | generated | hand | 316.5 | 796.2 | 2.52x | 826.1 | 2.61x | +3.8% | 1104 | 1104 | 6 % |  |
| el/floor | immediate | hand | 316.5 | 467.4 | 1.48x | 473.7 | 1.50x | +1.3% | 1120 | 1120 | 6 % |  |
| el/ladder | generated | hand | 1044.4 | 1850.7 | 1.77x | 1788.7 | 1.71x | -3.4% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 1044.4 | 1184.2 | 1.13x | 1197.6 | 1.15x | +1.1% | 1784 | 1784 | 8 % |  |
| el/nest7 | generated | hand | 729.2 | 1749.1 | 2.40x | 1730.7 | 2.37x | -1.1% | 1152 | 1152 | 35 % |  |
| el/nest7 | immediate | hand | 729.2 | 1041.5 | 1.43x | 1054.1 | 1.45x | +1.2% | 1120 | 1120 | 35 % |  |
| el/block | generated | hand | 1086.6 | 1861.5 | 1.71x | 1865.1 | 1.72x | +0.2% | 2344 | 2344 | 20 % |  |
| el/block | immediate | hand | 1086.6 | 1193.2 | 1.10x | 1175.2 | 1.08x | -1.5% | 2440 | 2440 | 20 % |  |
| el/try | generated | hand | 3311.4 | 4717.8 | 1.42x | 4742.5 | 1.43x | +0.5% | 4456 | 4456 | 13 % |  |
| el/try | immediate | hand | 3311.4 | 3234.9 | 0.98x | 3252.1 | 0.98x | +0.5% | 4152 | 4152 | 13 % |  |
| el/loop | generated | hand | 2252.8 | 3775.1 | 1.68x | 3762.2 | 1.67x | -0.3% | 4776 | 4776 | 4 % |  |
| el/loop | immediate | hand | 2252.8 | 2559.4 | 1.14x | 2530.1 | 1.12x | -1.1% | 4880 | 4880 | 4 % |  |
| el/terms100 | generated | hand | 11194.9 | 16087.1 | 1.44x | 16303.0 | 1.46x | +1.3% | 17904 | 17904 | 4 % |  |
| el/terms100 | immediate | hand | 11194.9 | 12637.1 | 1.13x | 12891.2 | 1.15x | +2.0% | 18720 | 18720 | 4 % |  |
| el/terms1000 | generated | hand | 110082.7 | 154671.5 | 1.41x | 155734.3 | 1.41x | +0.7% | 169107 | 169104 | 4 % |  |
| el/terms1000 | immediate | hand | 110082.7 | 121560.9 | 1.10x | 125334.2 | 1.14x | +3.1% | 177120 | 177120 | 4 % |  |
| el/overloads | generated | hand | 1909.6 | 2587.3 | 1.35x | 2589.3 | 1.36x | +0.1% | 4248 | 4248 | 17 % |  |
| el/overloads | immediate | hand | 1909.6 | 2044.6 | 1.07x | 2069.3 | 1.08x | +1.2% | 4200 | 4200 | 17 % |  |
| el/string | generated | hand | 285.9 | 1010.7 | 3.54x | 993.2 | 3.47x | -1.7% | 1056 | 1056 | 13 % |  |
| el/string | immediate | hand | 285.9 | 683.5 | 2.39x | 685.1 | 2.40x | +0.2% | 1032 | 1032 | 13 % |  |
| el/interpolation | generated | hand | 2476.2 | 5274.9 | 2.13x | 4964.4 | 2.00x | -5.9% | 2368 | 2368 | 122 % |  |
| el/interpolation | immediate | hand | 2476.2 | 4454.5 | 1.80x | 4623.4 | 1.87x | +3.8% | 2424 | 2424 | 122 % |  |
| el/untyped | generated | hand | 27162.2 | 29494.0 | 1.09x | 29458.1 | 1.08x | -0.1% | 16424 | 16424 | 57 % |  |
| el/refused-early | generated | hand | 403.9 | 1184.1 | 2.93x | 1246.9 | 3.09x | +5.3% | 1064 | 1064 | 18 % |  |
| el/refused-early | immediate | hand | 403.9 | 1146.3 | 2.84x | 1134.4 | 2.81x | -1.0% | 1944 | 1944 | 18 % |  |
| el/refused-late | generated | hand | 1331.1 | 1861.7 | 1.40x | 1880.6 | 1.41x | +1.0% | 1064 | 1064 | 14 % |  |
| el/refused-late | immediate | hand | 1331.1 | 2917.0 | 2.19x | 2984.3 | 2.24x | +2.3% | 3928 | 3928 | 14 % |  |
| sql/literal | generated | hand | 60.0 | 134.6 | 2.25x | 134.7 | 2.25x | +0.1% | 160 | 160 | 6 % |  |
| sql/comment | generated | hand | 2344.1 | 4903.5 | 2.09x | 4902.8 | 2.09x | 0.0% | 5136 | 5136 | 24 % |  |
| sql/conditions100 | generated | hand | 55331.6 | 127324.7 | 2.30x | 124067.0 | 2.24x | -2.6% | 161736 | 161736 | 23 % |  |
| sql/conditions1000 | generated | hand | 563866.4 | 1282722.7 | 2.27x | 1254911.7 | 2.23x | -2.2% | 1616136 | 1616136 | 14 % |  |
| tsql/comment | generated | scriptdom | 21478.0 | 1557.9 | 0.07x | 1573.5 | 0.07x | +1.0% | 1192 | 1192 | 11 % |  |
| sql/column | generated | hand | 225.0 | 512.1 | 2.28x | 502.6 | 2.23x | -1.9% | 392 | 392 | 36 % |  |
| sql/arithmetic | generated | hand | 1754.9 | 4402.1 | 2.51x | 4329.0 | 2.47x | -1.7% | 3472 | 3472 | 71 % |  |
| sql/nest8 | generated | hand | 2984.1 | 12557.2 | 4.21x | 11485.5 | 3.85x | -8.5% | 6504 | 6504 | 40 % |  |
| sql/condition | generated | hand | 1953.1 | 4466.0 | 2.29x | 4371.2 | 2.24x | -2.1% | 4928 | 4928 | 9 % |  |
| sql/select1 | generated | hand | 749.8 | 1573.8 | 2.10x | 1578.6 | 2.11x | +0.3% | 1688 | 1688 | 16 % |  |
| sql/select20 | generated | hand | 8134.0 | 19110.8 | 2.35x | 19251.1 | 2.37x | +0.7% | 21448 | 21448 | 19 % |  |
| sql/values | generated | hand | 518.5 | 1806.2 | 3.48x | 1721.2 | 3.32x | -4.7% | 1904 | 1904 | 10 % |  |
| sql/create | generated | hand | 837.1 | 2620.1 | 3.13x | 2651.0 | 3.17x | +1.2% | 1568 | 1568 | 6 % |  |
| sql/refused-late | generated | hand | 3016.6 | 12869.1 | 4.27x | 12932.8 | 4.29x | +0.5% | 13552 | 13552 | 4 % |  |
