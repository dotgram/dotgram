# Paired stand, 2026-09-20 04:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 830693.8 | 128793.8 | 0.16x | 124521.9 | 0.15x | -3.3% | 113600 | 105600 | 588 % |  |
| tsql/script100.boolboth | generated | scriptdom | 659614.1 | 122650.0 | 0.19x | 122315.6 | 0.19x | -0.3% | 105600 | 105600 | 21 % |  |
| tsql/script100 | generated | scriptdom | 658333.6 | 123882.8 | 0.19x | 123089.8 | 0.19x | -0.6% | 113600 | 113600 | 11 % |  |
| tsql/script400.bool | generated | scriptdom | 2626715.6 | 493943.8 | 0.19x | 487843.8 | 0.19x | -1.2% | 454400 | 422403 | 11 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2618368.8 | 490562.5 | 0.19x | 487946.9 | 0.19x | -0.5% | 422403 | 422403 | 18 % |  |
| tsql/script400 | generated | scriptdom | 2623968.8 | 494368.8 | 0.19x | 499390.6 | 0.19x | +1.0% | 454400 | 454400 | 11 % |  |
| tsql/columns1000 | generated | scriptdom | 1721823.4 | 188707.8 | 0.11x | 188009.4 | 0.11x | -0.4% | 200464 | 200464 | 5 % |  |
| tsql/conditions1000 | generated | scriptdom | 877371.1 | 334539.1 | 0.38x | 337514.1 | 0.38x | +0.9% | 424424 | 424424 | 4 % |  |
| tsql/rows1000 | generated | scriptdom | 577773.4 | 243920.3 | 0.42x | 245168.8 | 0.42x | +0.5% | 296472 | 296472 | 4 % |  |
| sql/select20.at | generated | hand | 7214.7 | 17717.8 | 2.46x | 17518.9 | 2.43x | -1.1% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 7175.0 | 18074.9 | 2.52x | 17814.7 | 2.48x | -1.4% | 21416 | 21416 | 2 % |  |
| tsql/insert-values.at | generated | scriptdom | 8102.8 | 1193.1 | 0.15x | 1225.8 | 0.15x | +2.7% | 1328 | 1328 | 9 % |  |
| sql/select20.scan | generated | control | 381.7 | 382.5 | 1.00x | 381.0 | 1.00x | -0.4% | 0 | 0 | 6 % |  |
| sql/conditions100.scan | generated | control | 3539.0 | 3526.0 | 1.00x | 3515.6 | 0.99x | -0.3% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 376.7 | 380.7 | 1.01x | 381.2 | 1.01x | +0.1% | 0 | 0 | 17 % |  |
| el/ladder.scan | generated | control | 140.6 | 137.6 | 0.98x | 140.4 | 1.00x | +2.0% | 0 | 0 | 14 % |  |
| el/refused-early.bool | generated | hand | 326.2 | 1121.4 | 3.44x | 551.6 | 1.69x | -50.8% | 1064 | 624 | 14 % |  |
| el/refused-late.bool | generated | hand | 1207.2 | 1822.7 | 1.51x | 901.8 | 0.75x | -50.5% | 1064 | 624 | 3 % |  |
| el/ladder.bool | generated | hand | 1030.6 | 1727.5 | 1.68x | 1714.6 | 1.66x | -0.7% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2657.3 | 12262.7 | 4.61x | 5880.8 | 2.21x | -52.0% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 7187.1 | 18333.6 | 2.55x | 18237.6 | 2.54x | -0.5% | 21448 | 21392 | 31 % |  |
| el/floor | generated | hand | 315.5 | 801.0 | 2.54x | 799.5 | 2.53x | -0.2% | 1104 | 1104 | 5 % |  |
| el/floor | immediate | hand | 315.5 | 478.8 | 1.52x | 469.8 | 1.49x | -1.9% | 1120 | 1120 | 5 % |  |
| el/ladder | generated | hand | 1020.2 | 1724.6 | 1.69x | 1726.4 | 1.69x | +0.1% | 1776 | 1776 | 6 % |  |
| el/ladder | immediate | hand | 1020.2 | 1160.2 | 1.14x | 1159.6 | 1.14x | 0.0% | 1784 | 1784 | 6 % |  |
| el/nest7 | generated | hand | 727.5 | 1731.3 | 2.38x | 1677.2 | 2.31x | -3.1% | 1152 | 1152 | 6 % |  |
| el/nest7 | immediate | hand | 727.5 | 1025.8 | 1.41x | 1036.4 | 1.42x | +1.0% | 1120 | 1120 | 6 % |  |
| el/block | generated | hand | 1114.0 | 1797.5 | 1.61x | 1800.8 | 1.62x | +0.2% | 2344 | 2344 | 4 % |  |
| el/block | immediate | hand | 1114.0 | 1146.9 | 1.03x | 1153.1 | 1.04x | +0.5% | 2440 | 2440 | 4 % |  |
| el/try | generated | hand | 3248.5 | 4646.0 | 1.43x | 4735.0 | 1.46x | +1.9% | 4456 | 4456 | 23 % |  |
| el/try | immediate | hand | 3248.5 | 3216.5 | 0.99x | 3224.2 | 0.99x | +0.2% | 4152 | 4152 | 23 % |  |
| el/loop | generated | hand | 2256.4 | 3713.6 | 1.65x | 3686.2 | 1.63x | -0.7% | 4776 | 4776 | 1 % |  |
| el/loop | immediate | hand | 2256.4 | 2567.5 | 1.14x | 2562.3 | 1.14x | -0.2% | 4880 | 4880 | 1 % |  |
| el/terms100 | generated | hand | 11373.2 | 16007.7 | 1.41x | 16299.1 | 1.43x | +1.8% | 17904 | 17904 | 3 % |  |
| el/terms100 | immediate | hand | 11373.2 | 12481.6 | 1.10x | 12846.5 | 1.13x | +2.9% | 18720 | 18720 | 3 % |  |
| el/terms1000 | generated | hand | 110559.6 | 150792.3 | 1.36x | 154905.5 | 1.40x | +2.7% | 169107 | 169128 | 16 % |  |
| el/terms1000 | immediate | hand | 110559.6 | 121543.2 | 1.10x | 123981.2 | 1.12x | +2.0% | 177120 | 177120 | 16 % |  |
| el/overloads | generated | hand | 1870.5 | 2667.7 | 1.43x | 2641.3 | 1.41x | -1.0% | 4248 | 4248 | 2 % |  |
| el/overloads | immediate | hand | 1870.5 | 2036.9 | 1.09x | 2081.3 | 1.11x | +2.2% | 4200 | 4200 | 2 % |  |
| el/string | generated | hand | 289.9 | 933.7 | 3.22x | 953.4 | 3.29x | +2.1% | 1056 | 1056 | 5 % |  |
| el/string | immediate | hand | 289.9 | 655.2 | 2.26x | 670.7 | 2.31x | +2.4% | 1032 | 1032 | 5 % |  |
| el/interpolation | generated | hand | 2713.1 | 5371.9 | 1.98x | 5317.8 | 1.96x | -1.0% | 2368 | 2368 | 71 % |  |
| el/interpolation | immediate | hand | 2713.1 | 5066.1 | 1.87x | 5426.1 | 2.00x | +7.1% | 2424 | 2424 | 71 % |  |
| el/untyped | generated | hand | 31732.1 | 33629.8 | 1.06x | 33444.8 | 1.05x | -0.6% | 16558 | 16424 | 16 % |  |
| el/refused-early | generated | hand | 557.1 | 1354.7 | 2.43x | 1346.3 | 2.42x | -0.6% | 1064 | 1064 | 17 % |  |
| el/refused-early | immediate | hand | 557.1 | 1357.2 | 2.44x | 1346.9 | 2.42x | -0.8% | 1944 | 1944 | 17 % |  |
| el/refused-late | generated | hand | 1676.6 | 2131.3 | 1.27x | 2085.0 | 1.24x | -2.2% | 1064 | 1064 | 20 % |  |
| el/refused-late | immediate | hand | 1676.6 | 3362.5 | 2.01x | 3638.9 | 2.17x | +8.2% | 3928 | 3928 | 20 % |  |
| sql/literal | generated | hand | 72.5 | 160.5 | 2.21x | 167.9 | 2.32x | +4.6% | 160 | 160 | 29 % |  |
| sql/comment | generated | hand | 3308.7 | 5779.8 | 1.75x | 5728.0 | 1.73x | -0.9% | 5136 | 5136 | 25 % |  |
| sql/conditions100 | generated | hand | 85794.0 | 157272.5 | 1.83x | 157925.6 | 1.84x | +0.4% | 161736 | 161736 | 39 % |  |
| sql/conditions1000 | generated | hand | 833220.3 | 1535470.3 | 1.84x | 1533564.8 | 1.84x | -0.1% | 1616136 | 1616136 | 52 % |  |
| tsql/comment | generated | scriptdom | 29080.4 | 1593.1 | 0.05x | 1580.4 | 0.05x | -0.8% | 1192 | 1192 | 18 % |  |
| sql/column | generated | hand | 215.3 | 445.9 | 2.07x | 405.7 | 1.88x | -9.0% | 392 | 392 | 18 % |  |
| sql/arithmetic | generated | hand | 2657.9 | 4793.7 | 1.80x | 4776.0 | 1.80x | -0.4% | 3472 | 3472 | 25 % |  |
| sql/nest8 | generated | hand | 3908.2 | 13348.0 | 3.42x | 13309.8 | 3.41x | -0.3% | 6504 | 6504 | 5 % |  |
| sql/condition | generated | hand | 2959.7 | 5484.3 | 1.85x | 5412.4 | 1.83x | -1.3% | 4928 | 4928 | 40 % |  |
| sql/select1 | generated | hand | 979.1 | 1822.9 | 1.86x | 1802.3 | 1.84x | -1.1% | 1688 | 1688 | 17 % |  |
| sql/select20 | generated | hand | 12001.1 | 22942.1 | 1.91x | 23066.2 | 1.92x | +0.5% | 21448 | 21448 | 5 % |  |
| sql/values | generated | hand | 758.2 | 2192.2 | 2.89x | 2129.8 | 2.81x | -2.8% | 1904 | 1904 | 23 % |  |
| sql/create | generated | hand | 1072.9 | 2930.4 | 2.73x | 2914.7 | 2.72x | -0.5% | 1568 | 1568 | 21 % |  |
| sql/refused-late | generated | hand | 3402.9 | 13413.9 | 3.94x | 13418.2 | 3.94x | 0.0% | 13552 | 13552 | 8 % |  |
