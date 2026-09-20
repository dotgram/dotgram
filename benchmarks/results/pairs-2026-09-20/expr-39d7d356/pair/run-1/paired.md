# Paired stand, 2026-09-20 02:24

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1462593.8 | 129259.4 | 0.09x | 123243.8 | 0.08x | -4.7% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 669116.4 | 123307.8 | 0.18x | 125865.6 | 0.19x | +2.1% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 673596.1 | 123681.2 | 0.18x | 128082.0 | 0.19x | +3.6% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2686643.8 | 503387.5 | 0.19x | 486693.8 | 0.18x | -3.3% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2673734.4 | 497128.1 | 0.19x | 491178.1 | 0.18x | -1.2% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2679837.5 | 495996.9 | 0.19x | 522028.1 | 0.19x | +5.2% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1741462.5 | 190393.8 | 0.11x | 190423.4 | 0.11x | 0.0% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 898355.5 | 339446.1 | 0.38x | 337791.4 | 0.38x | -0.5% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 612710.9 | 254036.7 | 0.41x | 262268.8 | 0.43x | +3.2% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7545.1 | 18065.1 | 2.39x | 17746.2 | 2.35x | -1.8% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7362.6 | 18317.8 | 2.49x | 18536.2 | 2.52x | +1.2% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8241.6 | 1193.8 | 0.14x | 1220.9 | 0.15x | +2.3% | 1328 | 1328 |
| sql/select20.scan | generated | control | 386.6 | 385.0 | 1.00x | 386.5 | 1.00x | +0.4% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3544.6 | 3551.6 | 1.00x | 3576.3 | 1.01x | +0.7% | 0 | 0 |
| tsql/select20.scan | generated | control | 381.1 | 381.8 | 1.00x | 395.8 | 1.04x | +3.7% | 0 | 0 |
| el/ladder.scan | generated | control | 143.3 | 139.9 | 0.98x | 140.7 | 0.98x | +0.6% | 0 | 0 |
| el/refused-early.bool | generated | hand | 344.5 | 1102.8 | 3.20x | 554.3 | 1.61x | -49.7% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1199.5 | 1770.6 | 1.48x | 920.5 | 0.77x | -48.0% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1020.4 | 1822.6 | 1.79x | 1731.2 | 1.70x | -5.0% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2683.1 | 12532.5 | 4.67x | 6011.5 | 2.24x | -52.0% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7266.7 | 18232.9 | 2.51x | 18190.5 | 2.50x | -0.2% | 21448 | 21392 |
| el/floor | generated | hand | 320.3 | 820.7 | 2.56x | 804.6 | 2.51x | -2.0% | 1104 | 1104 |
| el/floor | immediate | hand | 320.3 | 488.0 | 1.52x | 476.9 | 1.49x | -2.3% | 1120 | 1120 |
| el/ladder | generated | hand | 1026.5 | 1804.0 | 1.76x | 1774.5 | 1.73x | -1.6% | 1776 | 1776 |
| el/ladder | immediate | hand | 1026.5 | 1173.3 | 1.14x | 1178.6 | 1.15x | +0.4% | 1784 | 1784 |
| el/nest7 | generated | hand | 659.5 | 1783.8 | 2.70x | 1704.9 | 2.59x | -4.4% | 1152 | 1152 |
| el/nest7 | immediate | hand | 659.5 | 1064.0 | 1.61x | 1065.9 | 1.62x | +0.2% | 1120 | 1120 |
| el/block | generated | hand | 1067.6 | 1895.5 | 1.78x | 1826.3 | 1.71x | -3.7% | 2344 | 2344 |
| el/block | immediate | hand | 1067.6 | 1163.7 | 1.09x | 1167.6 | 1.09x | +0.3% | 2440 | 2440 |
| el/try | generated | hand | 3259.3 | 4744.8 | 1.46x | 4715.8 | 1.45x | -0.6% | 4456 | 4456 |
| el/try | immediate | hand | 3259.3 | 3311.5 | 1.02x | 3287.8 | 1.01x | -0.7% | 4152 | 4152 |
| el/loop | generated | hand | 2241.3 | 3801.4 | 1.70x | 3809.7 | 1.70x | +0.2% | 4801 | 4801 |
| el/loop | immediate | hand | 2241.3 | 2591.9 | 1.16x | 2528.1 | 1.13x | -2.5% | 4880 | 4880 |
| el/terms100 | generated | hand | 11422.9 | 16443.9 | 1.44x | 16338.9 | 1.43x | -0.6% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11422.9 | 12731.1 | 1.11x | 12725.0 | 1.11x | 0.0% | 18720 | 18720 |
| el/terms1000 | generated | hand | 110699.5 | 155281.1 | 1.40x | 152832.0 | 1.38x | -1.6% | 169104 | 169104 |
| el/terms1000 | immediate | hand | 110699.5 | 123837.9 | 1.12x | 123102.2 | 1.11x | -0.6% | 177120 | 177120 |
| el/overloads | generated | hand | 1860.5 | 2628.9 | 1.41x | 2611.8 | 1.40x | -0.7% | 4248 | 4248 |
| el/overloads | immediate | hand | 1860.5 | 2044.8 | 1.10x | 2083.3 | 1.12x | +1.9% | 4200 | 4200 |
| el/string | generated | hand | 288.7 | 1008.6 | 3.49x | 952.4 | 3.30x | -5.6% | 1056 | 1056 |
| el/string | immediate | hand | 288.7 | 676.7 | 2.34x | 672.6 | 2.33x | -0.6% | 1032 | 1032 |
| el/interpolation | generated | hand | 2033.2 | 4886.0 | 2.40x | 5006.5 | 2.46x | +2.5% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2033.2 | 4804.5 | 2.36x | 4343.8 | 2.14x | -9.6% | 2424 | 2424 |
| el/untyped | generated | hand | 18020.6 | 20694.3 | 1.15x | 19532.0 | 1.08x | -5.6% | 16424 | 16424 |
| el/refused-early | generated | hand | 493.3 | 1278.9 | 2.59x | 1375.7 | 2.79x | +7.6% | 1064 | 1064 |
| el/refused-early | immediate | hand | 493.3 | 1285.6 | 2.61x | 1263.6 | 2.56x | -1.7% | 1944 | 1944 |
| el/refused-late | generated | hand | 1502.5 | 2022.1 | 1.35x | 1878.9 | 1.25x | -7.1% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1502.5 | 3406.7 | 2.27x | 3211.0 | 2.14x | -5.7% | 3928 | 3928 |
| sql/literal | generated | hand | 69.7 | 157.1 | 2.25x | 157.5 | 2.26x | +0.3% | 160 | 160 |
| sql/comment | generated | hand | 2969.3 | 5308.3 | 1.79x | 5533.9 | 1.86x | +4.3% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 73282.2 | 146814.7 | 2.00x | 137242.5 | 1.87x | -6.5% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 784267.2 | 1450085.2 | 1.85x | 1418513.3 | 1.81x | -2.2% | 1616136 | 1616160 |
| tsql/comment | generated | scriptdom | 27230.8 | 1588.0 | 0.06x | 1607.8 | 0.06x | +1.2% | 1192 | 1192 |
| sql/column | generated | hand | 209.7 | 403.0 | 1.92x | 419.4 | 2.00x | +4.1% | 392 | 416 |
| sql/arithmetic | generated | hand | 2316.0 | 4806.5 | 2.08x | 4706.9 | 2.03x | -2.1% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3563.2 | 12598.7 | 3.54x | 12257.2 | 3.44x | -2.7% | 6504 | 6504 |
| sql/condition | generated | hand | 2578.5 | 4941.8 | 1.92x | 5009.0 | 1.94x | +1.4% | 4928 | 4928 |
| sql/select1 | generated | hand | 872.9 | 1718.5 | 1.97x | 1721.9 | 1.97x | +0.2% | 1688 | 1688 |
| sql/select20 | generated | hand | 10388.4 | 21629.1 | 2.08x | 21276.6 | 2.05x | -1.6% | 21448 | 21448 |
| sql/values | generated | hand | 747.5 | 2042.1 | 2.73x | 2021.8 | 2.70x | -1.0% | 1904 | 1904 |
| sql/create | generated | hand | 1018.7 | 2895.2 | 2.84x | 2891.6 | 2.84x | -0.1% | 1568 | 1568 |
| sql/refused-late | generated | hand | 4122.4 | 14946.1 | 3.63x | 14153.2 | 3.43x | -5.3% | 13552 | 13552 |
