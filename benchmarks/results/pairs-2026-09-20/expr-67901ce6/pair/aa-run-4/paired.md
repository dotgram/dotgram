# Paired stand, 2026-09-20 04:35

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 884512.5 | 126603.1 | 0.14x | 126431.2 | 0.14x | -0.1% | 113600 | 105600 | 520 % |  |
| tsql/script100.boolboth | generated | scriptdom | 658918.0 | 121921.1 | 0.19x | 126142.2 | 0.19x | +3.5% | 105600 | 105603 | 5 % |  |
| tsql/script100 | generated | scriptdom | 661268.0 | 122434.4 | 0.19x | 126339.8 | 0.19x | +3.2% | 113600 | 113600 | 6 % |  |
| tsql/script400.bool | generated | scriptdom | 2668112.5 | 492978.1 | 0.18x | 509712.5 | 0.19x | +3.4% | 454400 | 422403 | 16 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2639890.6 | 488203.1 | 0.18x | 505181.2 | 0.19x | +3.5% | 422400 | 422403 | 15 % |  |
| tsql/script400 | generated | scriptdom | 2638537.5 | 486687.5 | 0.18x | 498078.1 | 0.19x | +2.3% | 454400 | 454400 | 47 % |  |
| tsql/columns1000 | generated | scriptdom | 1736146.9 | 188501.6 | 0.11x | 189785.9 | 0.11x | +0.7% | 200464 | 200464 | 12 % |  |
| tsql/conditions1000 | generated | scriptdom | 886126.6 | 331693.0 | 0.37x | 335281.2 | 0.38x | +1.1% | 424424 | 424424 | 5 % |  |
| tsql/rows1000 | generated | scriptdom | 583999.2 | 243427.3 | 0.42x | 242993.0 | 0.42x | -0.2% | 296472 | 296472 | 8 % |  |
| sql/select20.at | generated | hand | 7387.8 | 17717.8 | 2.40x | 17429.9 | 2.36x | -1.6% | 21416 | 21416 | 24 % |  |
| sql/select20.window | generated | hand | 7329.9 | 18230.2 | 2.49x | 18619.9 | 2.54x | +2.1% | 21416 | 21416 | 6 % |  |
| tsql/insert-values.at | generated | scriptdom | 8138.8 | 1183.5 | 0.15x | 1155.6 | 0.14x | -2.4% | 1328 | 1328 | 3 % |  |
| sql/select20.scan | generated | control | 379.6 | 379.3 | 1.00x | 382.9 | 1.01x | +0.9% | 0 | 0 | 18 % |  |
| sql/conditions100.scan | generated | control | 3532.4 | 3509.4 | 0.99x | 3488.3 | 0.99x | -0.6% | 0 | 0 | 11 % |  |
| tsql/select20.scan | generated | control | 378.1 | 375.7 | 0.99x | 377.1 | 1.00x | +0.4% | 0 | 0 | 14 % |  |
| el/ladder.scan | generated | control | 138.3 | 136.7 | 0.99x | 137.5 | 0.99x | +0.6% | 0 | 0 | 13 % |  |
| el/refused-early.bool | generated | hand | 329.2 | 1062.0 | 3.23x | 558.8 | 1.70x | -47.4% | 1064 | 624 | 4 % |  |
| el/refused-late.bool | generated | hand | 1193.9 | 1752.1 | 1.47x | 912.0 | 0.76x | -47.9% | 1064 | 624 | 11 % |  |
| el/ladder.bool | generated | hand | 1007.6 | 1787.1 | 1.77x | 1709.0 | 1.70x | -4.4% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2750.7 | 12327.6 | 4.48x | 5853.9 | 2.13x | -52.5% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 7284.6 | 18160.3 | 2.49x | 17873.9 | 2.45x | -1.6% | 21448 | 21392 | 12 % |  |
| el/floor | generated | hand | 318.8 | 813.5 | 2.55x | 871.7 | 2.73x | +7.2% | 1104 | 1104 | 14 % |  |
| el/floor | immediate | hand | 318.8 | 474.7 | 1.49x | 477.5 | 1.50x | +0.6% | 1120 | 1120 | 14 % |  |
| el/ladder | generated | hand | 1007.5 | 1803.1 | 1.79x | 1772.0 | 1.76x | -1.7% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 1007.5 | 1168.5 | 1.16x | 1164.2 | 1.16x | -0.4% | 1784 | 1784 | 3 % |  |
| el/nest7 | generated | hand | 721.0 | 1713.4 | 2.38x | 1712.6 | 2.38x | 0.0% | 1152 | 1177 | 3 % |  |
| el/nest7 | immediate | hand | 721.0 | 1040.3 | 1.44x | 1050.9 | 1.46x | +1.0% | 1120 | 1120 | 3 % |  |
| el/block | generated | hand | 1042.6 | 1805.0 | 1.73x | 1838.3 | 1.76x | +1.8% | 2344 | 2344 | 2 % |  |
| el/block | immediate | hand | 1042.6 | 1145.5 | 1.10x | 1153.5 | 1.11x | +0.7% | 2440 | 2440 | 2 % |  |
| el/try | generated | hand | 3221.7 | 4715.1 | 1.46x | 4747.9 | 1.47x | +0.7% | 4456 | 4456 | 11 % |  |
| el/try | immediate | hand | 3221.7 | 3187.2 | 0.99x | 3268.8 | 1.01x | +2.6% | 4152 | 4152 | 11 % |  |
| el/loop | generated | hand | 2233.1 | 3736.5 | 1.67x | 3766.0 | 1.69x | +0.8% | 4776 | 4776 | 11 % |  |
| el/loop | immediate | hand | 2233.1 | 2537.0 | 1.14x | 2560.1 | 1.15x | +0.9% | 4880 | 4880 | 11 % |  |
| el/terms100 | generated | hand | 11495.3 | 16132.8 | 1.40x | 16228.0 | 1.41x | +0.6% | 17904 | 17904 | 3 % |  |
| el/terms100 | immediate | hand | 11495.3 | 12704.6 | 1.11x | 12874.0 | 1.12x | +1.3% | 18720 | 18720 | 3 % |  |
| el/terms1000 | generated | hand | 111661.6 | 152224.5 | 1.36x | 154115.1 | 1.38x | +1.2% | 169104 | 169128 | 3 % |  |
| el/terms1000 | immediate | hand | 111661.6 | 122491.4 | 1.10x | 124174.7 | 1.11x | +1.4% | 177123 | 177120 | 3 % |  |
| el/overloads | generated | hand | 1846.8 | 2598.3 | 1.41x | 2634.4 | 1.43x | +1.4% | 4248 | 4248 | 5 % |  |
| el/overloads | immediate | hand | 1846.8 | 2086.8 | 1.13x | 2046.8 | 1.11x | -1.9% | 4200 | 4200 | 5 % |  |
| el/string | generated | hand | 294.4 | 944.9 | 3.21x | 984.5 | 3.34x | +4.2% | 1056 | 1056 | 2 % |  |
| el/string | immediate | hand | 294.4 | 660.3 | 2.24x | 658.1 | 2.24x | -0.3% | 1032 | 1032 | 2 % |  |
| el/interpolation | generated | hand | 2116.2 | 4760.5 | 2.25x | 4676.9 | 2.21x | -1.8% | 2368 | 2368 | 109 % |  |
| el/interpolation | immediate | hand | 2116.2 | 4625.6 | 2.19x | 4081.6 | 1.93x | -11.8% | 2424 | 2424 | 109 % |  |
| el/untyped | generated | hand | 27263.8 | 28625.9 | 1.05x | 28855.8 | 1.06x | +0.8% | 16424 | 16424 | 29 % |  |
| el/refused-early | generated | hand | 491.0 | 1235.0 | 2.52x | 1313.2 | 2.67x | +6.3% | 1064 | 1064 | 17 % |  |
| el/refused-early | immediate | hand | 491.0 | 1252.5 | 2.55x | 1245.4 | 2.54x | -0.6% | 1944 | 1944 | 17 % |  |
| el/refused-late | generated | hand | 1519.5 | 1953.4 | 1.29x | 2003.6 | 1.32x | +2.6% | 1064 | 1064 | 15 % |  |
| el/refused-late | immediate | hand | 1519.5 | 3309.1 | 2.18x | 3325.8 | 2.19x | +0.5% | 3928 | 3928 | 15 % |  |
| sql/literal | generated | hand | 66.8 | 146.6 | 2.20x | 154.9 | 2.32x | +5.6% | 160 | 160 | 53 % |  |
| sql/comment | generated | hand | 2992.7 | 5430.9 | 1.81x | 5390.5 | 1.80x | -0.7% | 5136 | 5136 | 30 % |  |
| sql/conditions100 | generated | hand | 75536.9 | 142193.5 | 1.88x | 140329.2 | 1.86x | -1.3% | 161736 | 161736 | 18 % |  |
| sql/conditions1000 | generated | hand | 733827.3 | 1451099.2 | 1.98x | 1443339.8 | 1.97x | -0.5% | 1616136 | 1616136 | 20 % |  |
| tsql/comment | generated | scriptdom | 26463.1 | 1558.3 | 0.06x | 1577.6 | 0.06x | +1.2% | 1192 | 1192 | 19 % |  |
| sql/column | generated | hand | 193.5 | 389.1 | 2.01x | 402.0 | 2.08x | +3.3% | 392 | 392 | 23 % |  |
| sql/arithmetic | generated | hand | 2211.0 | 4744.0 | 2.15x | 4707.1 | 2.13x | -0.8% | 3472 | 3472 | 19 % |  |
| sql/nest8 | generated | hand | 3569.1 | 11986.0 | 3.36x | 13045.2 | 3.66x | +8.8% | 6504 | 6504 | 28 % |  |
| sql/condition | generated | hand | 2608.9 | 4984.8 | 1.91x | 4950.2 | 1.90x | -0.7% | 4928 | 4928 | 12 % |  |
| sql/select1 | generated | hand | 894.0 | 1719.7 | 1.92x | 1700.8 | 1.90x | -1.1% | 1688 | 1712 | 12 % |  |
| sql/select20 | generated | hand | 10677.4 | 21421.8 | 2.01x | 21403.0 | 2.00x | -0.1% | 21448 | 21448 | 25 % |  |
| sql/values | generated | hand | 681.9 | 2005.4 | 2.94x | 1938.1 | 2.84x | -3.4% | 1904 | 1904 | 18 % |  |
| sql/create | generated | hand | 999.6 | 2781.4 | 2.78x | 2773.4 | 2.77x | -0.3% | 1568 | 1568 | 34 % |  |
| sql/refused-late | generated | hand | 2999.2 | 12680.4 | 4.23x | 12677.6 | 4.23x | 0.0% | 13552 | 13552 | 17 % |  |
