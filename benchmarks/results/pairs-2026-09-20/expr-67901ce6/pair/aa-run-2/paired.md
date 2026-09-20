# Paired stand, 2026-09-20 04:17

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1082787.5 | 196803.1 | 0.18x | 193309.4 | 0.18x | -1.8% | 113600 | 105603 | 381 % |  |
| tsql/script100.boolboth | generated | scriptdom | 1066906.2 | 189189.8 | 0.18x | 193146.1 | 0.18x | +2.1% | 105600 | 105600 | 40 % |  |
| tsql/script100 | generated | scriptdom | 1075464.1 | 190050.8 | 0.18x | 184064.1 | 0.17x | -3.2% | 113600 | 113600 | 28 % |  |
| tsql/script400.bool | generated | scriptdom | 4141778.1 | 762643.8 | 0.18x | 757612.5 | 0.18x | -0.7% | 454400 | 422400 | 58 % |  |
| tsql/script400.boolboth | generated | scriptdom | 4268584.4 | 765387.5 | 0.18x | 792446.9 | 0.19x | +3.5% | 422400 | 422403 | 15 % |  |
| tsql/script400 | generated | scriptdom | 3212428.1 | 493403.1 | 0.15x | 598100.0 | 0.19x | +21.2% | 454400 | 454400 | 59 % |  |
| tsql/columns1000 | generated | scriptdom | 1997239.1 | 201246.9 | 0.10x | 194420.3 | 0.10x | -3.4% | 200464 | 200464 | 45 % |  |
| tsql/conditions1000 | generated | scriptdom | 953231.2 | 367410.9 | 0.39x | 348957.0 | 0.37x | -5.0% | 424424 | 424424 | 60 % |  |
| tsql/rows1000 | generated | scriptdom | 595701.6 | 248766.4 | 0.42x | 245591.4 | 0.41x | -1.3% | 296472 | 296472 | 6 % |  |
| sql/select20.at | generated | hand | 7260.5 | 17722.6 | 2.44x | 17532.6 | 2.41x | -1.1% | 21416 | 21416 | 8 % |  |
| sql/select20.window | generated | hand | 7327.2 | 18603.9 | 2.54x | 19126.7 | 2.61x | +2.8% | 21416 | 21416 | 28 % |  |
| tsql/insert-values.at | generated | scriptdom | 8501.7 | 1212.2 | 0.14x | 1197.1 | 0.14x | -1.2% | 1328 | 1328 | 13 % |  |
| sql/select20.scan | generated | control | 379.6 | 387.4 | 1.02x | 383.9 | 1.01x | -0.9% | 0 | 0 | 3 % |  |
| sql/conditions100.scan | generated | control | 3593.6 | 3556.8 | 0.99x | 3576.1 | 1.00x | +0.5% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 410.2 | 405.1 | 0.99x | 393.7 | 0.96x | -2.8% | 0 | 0 | 40 % |  |
| el/ladder.scan | generated | control | 139.8 | 143.8 | 1.03x | 141.8 | 1.01x | -1.4% | 0 | 0 | 10 % |  |
| el/refused-early.bool | generated | hand | 339.1 | 1131.8 | 3.34x | 555.9 | 1.64x | -50.9% | 1064 | 624 | 15 % |  |
| el/refused-late.bool | generated | hand | 1197.3 | 1839.5 | 1.54x | 923.7 | 0.77x | -49.8% | 1064 | 624 | 23 % |  |
| el/ladder.bool | generated | hand | 1018.9 | 1752.1 | 1.72x | 1717.1 | 1.69x | -2.0% | 1776 | 1720 | 15 % |  |
| sql/refused-late.bool | generated | hand | 2695.9 | 12249.0 | 4.54x | 5873.9 | 2.18x | -52.0% | 13552 | 6616 | 11 % |  |
| sql/select20.bool | generated | hand | 7252.1 | 18253.9 | 2.52x | 18472.2 | 2.55x | +1.2% | 21448 | 21392 | 11 % |  |
| el/floor | generated | hand | 312.7 | 809.4 | 2.59x | 813.9 | 2.60x | +0.6% | 1104 | 1104 | 8 % |  |
| el/floor | immediate | hand | 312.7 | 490.5 | 1.57x | 494.2 | 1.58x | +0.8% | 1120 | 1120 | 8 % |  |
| el/ladder | generated | hand | 1017.0 | 1751.0 | 1.72x | 1746.0 | 1.72x | -0.3% | 1776 | 1776 | 5 % |  |
| el/ladder | immediate | hand | 1017.0 | 1204.0 | 1.18x | 1230.4 | 1.21x | +2.2% | 1784 | 1784 | 5 % |  |
| el/nest7 | generated | hand | 722.1 | 1683.4 | 2.33x | 1702.3 | 2.36x | +1.1% | 1152 | 1152 | 11 % |  |
| el/nest7 | immediate | hand | 722.1 | 1055.8 | 1.46x | 1060.3 | 1.47x | +0.4% | 1120 | 1120 | 11 % |  |
| el/block | generated | hand | 1061.6 | 1862.8 | 1.75x | 1818.5 | 1.71x | -2.4% | 2344 | 2344 | 10 % |  |
| el/block | immediate | hand | 1061.6 | 1186.2 | 1.12x | 1202.3 | 1.13x | +1.4% | 2440 | 2440 | 10 % |  |
| el/try | generated | hand | 3281.3 | 4694.1 | 1.43x | 4708.4 | 1.43x | +0.3% | 4456 | 4456 | 2 % |  |
| el/try | immediate | hand | 3281.3 | 3311.9 | 1.01x | 3414.6 | 1.04x | +3.1% | 4152 | 4152 | 2 % |  |
| el/loop | generated | hand | 2263.9 | 3761.3 | 1.66x | 3751.0 | 1.66x | -0.3% | 4776 | 4776 | 17 % |  |
| el/loop | immediate | hand | 2263.9 | 2592.5 | 1.15x | 2709.2 | 1.20x | +4.5% | 4880 | 4880 | 17 % |  |
| el/terms100 | generated | hand | 11739.4 | 16783.8 | 1.43x | 16801.6 | 1.43x | +0.1% | 17904 | 17904 | 16 % |  |
| el/terms100 | immediate | hand | 11739.4 | 13256.6 | 1.13x | 13437.0 | 1.14x | +1.4% | 18720 | 18720 | 16 % |  |
| el/terms1000 | generated | hand | 111227.3 | 158286.1 | 1.42x | 155096.4 | 1.39x | -2.0% | 169150 | 169128 | 18 % |  |
| el/terms1000 | immediate | hand | 111227.3 | 126017.4 | 1.13x | 125955.4 | 1.13x | 0.0% | 177120 | 177120 | 18 % |  |
| el/overloads | generated | hand | 1902.5 | 2615.9 | 1.37x | 2645.6 | 1.39x | +1.1% | 4248 | 4248 | 7 % |  |
| el/overloads | immediate | hand | 1902.5 | 2136.7 | 1.12x | 2147.6 | 1.13x | +0.5% | 4200 | 4200 | 7 % |  |
| el/string | generated | hand | 294.2 | 960.4 | 3.26x | 965.0 | 3.28x | +0.5% | 1056 | 1056 | 17 % |  |
| el/string | immediate | hand | 294.2 | 677.9 | 2.30x | 689.6 | 2.34x | +1.7% | 1032 | 1032 | 17 % |  |
| el/interpolation | generated | hand | 1992.8 | 4627.8 | 2.32x | 4827.0 | 2.42x | +4.3% | 2368 | 2368 | 81 % |  |
| el/interpolation | immediate | hand | 1992.8 | 4276.1 | 2.15x | 3788.3 | 1.90x | -11.4% | 2424 | 2424 | 81 % |  |
| el/untyped | generated | hand | 28393.9 | 29038.2 | 1.02x | 29013.8 | 1.02x | -0.1% | 16424 | 16424 | 32 % |  |
| el/refused-early | generated | hand | 428.1 | 1239.7 | 2.90x | 1233.6 | 2.88x | -0.5% | 1064 | 1064 | 13 % |  |
| el/refused-early | immediate | hand | 428.1 | 1172.1 | 2.74x | 1179.9 | 2.76x | +0.7% | 1944 | 1944 | 13 % |  |
| el/refused-late | generated | hand | 1408.6 | 1936.4 | 1.37x | 1943.7 | 1.38x | +0.4% | 1064 | 1064 | 15 % |  |
| el/refused-late | immediate | hand | 1408.6 | 3055.3 | 2.17x | 3046.3 | 2.16x | -0.3% | 3928 | 3928 | 15 % |  |
| sql/literal | generated | hand | 58.9 | 154.2 | 2.62x | 155.4 | 2.64x | +0.8% | 160 | 160 | 55 % |  |
| sql/comment | generated | hand | 2546.9 | 5017.5 | 1.97x | 4981.8 | 1.96x | -0.7% | 5136 | 5136 | 17 % |  |
| sql/conditions100 | generated | hand | 61854.9 | 130921.8 | 2.12x | 133109.9 | 2.15x | +1.7% | 161736 | 161736 | 7 % |  |
| sql/conditions1000 | generated | hand | 606391.4 | 1340736.7 | 2.21x | 1323056.2 | 2.18x | -1.3% | 1616160 | 1616136 | 11 % |  |
| tsql/comment | generated | scriptdom | 23385.8 | 1558.3 | 0.07x | 1593.1 | 0.07x | +2.2% | 1192 | 1192 | 20 % |  |
| sql/column | generated | hand | 169.3 | 404.1 | 2.39x | 395.7 | 2.34x | -2.1% | 392 | 392 | 13 % |  |
| sql/arithmetic | generated | hand | 1899.5 | 4375.1 | 2.30x | 4437.3 | 2.34x | +1.4% | 3472 | 3472 | 14 % |  |
| sql/nest8 | generated | hand | 3177.8 | 11734.3 | 3.69x | 12613.2 | 3.97x | +7.5% | 6504 | 6504 | 18 % |  |
| sql/condition | generated | hand | 2176.5 | 4604.6 | 2.12x | 4718.2 | 2.17x | +2.5% | 4928 | 4928 | 22 % |  |
| sql/select1 | generated | hand | 789.0 | 1634.9 | 2.07x | 1652.6 | 2.09x | +1.1% | 1688 | 1688 | 9 % |  |
| sql/select20 | generated | hand | 9019.8 | 20307.6 | 2.25x | 19882.5 | 2.20x | -2.1% | 21448 | 21448 | 13 % |  |
| sql/values | generated | hand | 577.6 | 1844.6 | 3.19x | 1896.3 | 3.28x | +2.8% | 1904 | 1904 | 9 % |  |
| sql/create | generated | hand | 910.3 | 2819.6 | 3.10x | 2801.5 | 3.08x | -0.6% | 1568 | 1568 | 12 % |  |
| sql/refused-late | generated | hand | 3325.1 | 13321.5 | 4.01x | 13262.5 | 3.99x | -0.4% | 13552 | 13552 | 23 % |  |
