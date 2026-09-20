# Paired stand, 2026-09-20 04:26

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 744531.2 | 125621.9 | 0.17x | 125290.6 | 0.17x | -0.3% | 113600 | 105600 | 629 % |  |
| tsql/script100.boolboth | generated | scriptdom | 660019.5 | 122194.5 | 0.19x | 122672.7 | 0.19x | +0.4% | 105600 | 105600 | 5 % |  |
| tsql/script100 | generated | scriptdom | 660235.9 | 119750.8 | 0.18x | 122598.4 | 0.19x | +2.4% | 113600 | 113600 | 7 % |  |
| tsql/script400.bool | generated | scriptdom | 2642612.5 | 478750.0 | 0.18x | 489171.9 | 0.19x | +2.2% | 454400 | 422403 | 12 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2650218.8 | 491234.4 | 0.19x | 491531.2 | 0.19x | +0.1% | 422400 | 422403 | 24 % |  |
| tsql/script400 | generated | scriptdom | 2649209.4 | 481193.8 | 0.18x | 491543.8 | 0.19x | +2.2% | 454400 | 454400 | 4 % |  |
| tsql/columns1000 | generated | scriptdom | 1762518.8 | 190843.8 | 0.11x | 186845.3 | 0.11x | -2.1% | 200464 | 200464 | 12 % |  |
| tsql/conditions1000 | generated | scriptdom | 906774.2 | 344582.0 | 0.38x | 343048.4 | 0.38x | -0.4% | 424424 | 424424 | 10 % |  |
| tsql/rows1000 | generated | scriptdom | 583405.5 | 244584.4 | 0.42x | 246420.3 | 0.42x | +0.8% | 296472 | 296472 | 5 % |  |
| sql/select20.at | generated | hand | 7235.4 | 17569.8 | 2.43x | 17726.1 | 2.45x | +0.9% | 21416 | 21416 | 18 % |  |
| sql/select20.window | generated | hand | 7218.7 | 18043.1 | 2.50x | 18159.5 | 2.52x | +0.6% | 21416 | 21416 | 4 % |  |
| tsql/insert-values.at | generated | scriptdom | 8079.7 | 1156.0 | 0.14x | 1200.0 | 0.15x | +3.8% | 1328 | 1328 | 19 % |  |
| sql/select20.scan | generated | control | 382.2 | 381.9 | 1.00x | 386.2 | 1.01x | +1.1% | 0 | 0 | 3 % |  |
| sql/conditions100.scan | generated | control | 3679.7 | 3591.9 | 0.98x | 3562.2 | 0.97x | -0.8% | 0 | 0 | 9 % |  |
| tsql/select20.scan | generated | control | 381.2 | 386.1 | 1.01x | 387.0 | 1.02x | +0.2% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 142.5 | 144.6 | 1.02x | 150.4 | 1.06x | +4.0% | 0 | 0 | 66 % |  |
| el/refused-early.bool | generated | hand | 405.5 | 1174.5 | 2.90x | 597.5 | 1.47x | -49.1% | 1064 | 624 | 58 % |  |
| el/refused-late.bool | generated | hand | 2028.1 | 2700.1 | 1.33x | 1380.0 | 0.68x | -48.9% | 1064 | 624 | 18 % |  |
| el/ladder.bool | generated | hand | 1775.7 | 2718.3 | 1.53x | 2499.5 | 1.41x | -8.1% | 1776 | 1720 | 27 % |  |
| sql/refused-late.bool | generated | hand | 4420.7 | 17305.7 | 3.91x | 8557.7 | 1.94x | -50.5% | 13552 | 6616 | 44 % |  |
| sql/select20.bool | generated | hand | 11289.4 | 27962.8 | 2.48x | 26778.6 | 2.37x | -4.2% | 21448 | 21392 | 46 % |  |
| el/floor | generated | hand | 513.6 | 1152.1 | 2.24x | 1145.6 | 2.23x | -0.6% | 1104 | 1104 | 53 % |  |
| el/floor | immediate | hand | 513.6 | 689.3 | 1.34x | 672.3 | 1.31x | -2.5% | 1120 | 1120 | 53 % |  |
| el/ladder | generated | hand | 1517.3 | 2635.2 | 1.74x | 2661.5 | 1.75x | +1.0% | 1776 | 1776 | 41 % |  |
| el/ladder | immediate | hand | 1517.3 | 1768.7 | 1.17x | 1889.3 | 1.25x | +6.8% | 1784 | 1784 | 41 % |  |
| el/nest7 | generated | hand | 1067.6 | 2584.9 | 2.42x | 2459.1 | 2.30x | -4.9% | 1152 | 1152 | 56 % |  |
| el/nest7 | immediate | hand | 1067.6 | 1557.7 | 1.46x | 1697.6 | 1.59x | +9.0% | 1120 | 1120 | 56 % |  |
| el/block | generated | hand | 1777.9 | 3011.2 | 1.69x | 2977.8 | 1.67x | -1.1% | 2344 | 2344 | 22 % |  |
| el/block | immediate | hand | 1777.9 | 1967.8 | 1.11x | 1952.8 | 1.10x | -0.8% | 2440 | 2440 | 22 % |  |
| el/try | generated | hand | 5237.4 | 7194.5 | 1.37x | 7260.6 | 1.39x | +0.9% | 4456 | 4456 | 40 % |  |
| el/try | immediate | hand | 5237.4 | 5383.6 | 1.03x | 5269.4 | 1.01x | -2.1% | 4152 | 4152 | 40 % |  |
| el/loop | generated | hand | 3765.5 | 5929.9 | 1.57x | 5424.9 | 1.44x | -8.5% | 4776 | 4776 | 40 % |  |
| el/loop | immediate | hand | 3765.5 | 4325.9 | 1.15x | 4235.4 | 1.12x | -2.1% | 4880 | 4880 | 40 % |  |
| el/terms100 | generated | hand | 11863.3 | 17046.6 | 1.44x | 17182.8 | 1.45x | +0.8% | 17904 | 17904 | 5 % |  |
| el/terms100 | immediate | hand | 11863.3 | 13392.2 | 1.13x | 13724.2 | 1.16x | +2.5% | 18720 | 18720 | 5 % |  |
| el/terms1000 | generated | hand | 117565.4 | 164670.5 | 1.40x | 163874.5 | 1.39x | -0.5% | 169104 | 169104 | 5 % |  |
| el/terms1000 | immediate | hand | 117565.4 | 127852.1 | 1.09x | 135095.7 | 1.15x | +5.7% | 177120 | 177123 | 5 % |  |
| el/overloads | generated | hand | 1888.1 | 2682.7 | 1.42x | 2617.1 | 1.39x | -2.4% | 4248 | 4248 | 6 % |  |
| el/overloads | immediate | hand | 1888.1 | 2183.9 | 1.16x | 2087.7 | 1.11x | -4.4% | 4200 | 4200 | 6 % |  |
| el/string | generated | hand | 285.0 | 981.1 | 3.44x | 987.1 | 3.46x | +0.6% | 1056 | 1056 | 18 % |  |
| el/string | immediate | hand | 285.0 | 688.8 | 2.42x | 683.6 | 2.40x | -0.8% | 1032 | 1032 | 18 % |  |
| el/interpolation | generated | hand | 2076.0 | 4784.2 | 2.30x | 4664.5 | 2.25x | -2.5% | 2368 | 2368 | 55 % |  |
| el/interpolation | immediate | hand | 2076.0 | 4006.0 | 1.93x | 4456.3 | 2.15x | +11.2% | 2424 | 2424 | 55 % |  |
| el/untyped | generated | hand | 30864.0 | 31691.9 | 1.03x | 31961.7 | 1.04x | +0.9% | 16424 | 16424 | 19 % |  |
| el/refused-early | generated | hand | 457.3 | 1238.0 | 2.71x | 1285.9 | 2.81x | +3.9% | 1064 | 1064 | 17 % |  |
| el/refused-early | immediate | hand | 457.3 | 1234.9 | 2.70x | 1208.7 | 2.64x | -2.1% | 1944 | 1944 | 17 % |  |
| el/refused-late | generated | hand | 1373.5 | 1888.5 | 1.37x | 1926.0 | 1.40x | +2.0% | 1064 | 1064 | 12 % |  |
| el/refused-late | immediate | hand | 1373.5 | 3104.3 | 2.26x | 3217.2 | 2.34x | +3.6% | 3928 | 3928 | 12 % |  |
| sql/literal | generated | hand | 63.1 | 137.2 | 2.17x | 148.3 | 2.35x | +8.1% | 160 | 160 | 22 % |  |
| sql/comment | generated | hand | 2623.7 | 4986.2 | 1.90x | 5130.7 | 1.96x | +2.9% | 5136 | 5136 | 13 % |  |
| sql/conditions100 | generated | hand | 63779.2 | 128642.8 | 2.02x | 134776.6 | 2.11x | +4.8% | 161736 | 161736 | 14 % |  |
| sql/conditions1000 | generated | hand | 634721.1 | 1327679.3 | 2.09x | 1342337.9 | 2.11x | +1.1% | 1616136 | 1616160 | 10 % |  |
| tsql/comment | generated | scriptdom | 23805.9 | 1534.3 | 0.06x | 1575.0 | 0.07x | +2.7% | 1192 | 1192 | 9 % |  |
| sql/column | generated | hand | 166.8 | 362.5 | 2.17x | 389.0 | 2.33x | +7.3% | 392 | 392 | 11 % |  |
| sql/arithmetic | generated | hand | 1916.3 | 4417.5 | 2.31x | 4481.6 | 2.34x | +1.5% | 3472 | 3472 | 7 % |  |
| sql/nest8 | generated | hand | 3218.8 | 11694.6 | 3.63x | 12838.3 | 3.99x | +9.8% | 6504 | 6504 | 58 % |  |
| sql/condition | generated | hand | 2224.6 | 4575.8 | 2.06x | 4646.1 | 2.09x | +1.5% | 4928 | 4928 | 13 % |  |
| sql/select1 | generated | hand | 790.3 | 1642.4 | 2.08x | 1625.7 | 2.06x | -1.0% | 1688 | 1688 | 22 % |  |
| sql/select20 | generated | hand | 9069.4 | 19790.2 | 2.18x | 20087.1 | 2.21x | +1.5% | 21448 | 21448 | 8 % |  |
| sql/values | generated | hand | 604.9 | 1819.6 | 3.01x | 1844.9 | 3.05x | +1.4% | 1904 | 1904 | 10 % |  |
| sql/create | generated | hand | 843.9 | 2644.6 | 3.13x | 2665.3 | 3.16x | +0.8% | 1568 | 1568 | 16 % |  |
| sql/refused-late | generated | hand | 2967.1 | 12572.5 | 4.24x | 12859.6 | 4.33x | +2.3% | 13552 | 13552 | 9 % |  |
