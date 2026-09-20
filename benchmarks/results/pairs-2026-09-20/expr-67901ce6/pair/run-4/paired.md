# Paired stand, 2026-09-20 04:30

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 668925.0 | 122571.9 | 0.18x | 124000.0 | 0.19x | +1.2% | 113600 | 105600 | 691 % |  |
| tsql/script100.boolboth | generated | scriptdom | 663308.6 | 123218.0 | 0.19x | 126811.7 | 0.19x | +2.9% | 105600 | 105600 | 11 % |  |
| tsql/script100 | generated | scriptdom | 653345.3 | 122824.2 | 0.19x | 125761.7 | 0.19x | +2.4% | 113600 | 113600 | 7 % |  |
| tsql/script400.bool | generated | scriptdom | 2683725.0 | 503890.6 | 0.19x | 508600.0 | 0.19x | +0.9% | 454400 | 422403 | 44 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2602693.8 | 489175.0 | 0.19x | 498262.5 | 0.19x | +1.9% | 422403 | 422403 | 6 % |  |
| tsql/script400 | generated | scriptdom | 2618100.0 | 494053.1 | 0.19x | 500468.8 | 0.19x | +1.3% | 454400 | 454400 | 12 % |  |
| tsql/columns1000 | generated | scriptdom | 1700903.1 | 187384.4 | 0.11x | 187512.5 | 0.11x | +0.1% | 200464 | 200464 | 2 % |  |
| tsql/conditions1000 | generated | scriptdom | 882846.1 | 333415.6 | 0.38x | 332076.6 | 0.38x | -0.4% | 424424 | 424424 | 9 % |  |
| tsql/rows1000 | generated | scriptdom | 586491.4 | 241309.4 | 0.41x | 244856.2 | 0.42x | +1.5% | 296472 | 296472 | 3 % |  |
| sql/select20.at | generated | hand | 7196.1 | 17580.9 | 2.44x | 17703.2 | 2.46x | +0.7% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 7353.8 | 18646.0 | 2.54x | 18390.9 | 2.50x | -1.4% | 21416 | 21416 | 27 % |  |
| tsql/insert-values.at | generated | scriptdom | 8133.5 | 1155.5 | 0.14x | 1206.4 | 0.15x | +4.4% | 1328 | 1328 | 4 % |  |
| sql/select20.scan | generated | control | 381.0 | 389.0 | 1.02x | 387.1 | 1.02x | -0.5% | 0 | 0 | 25 % |  |
| sql/conditions100.scan | generated | control | 3507.3 | 3546.9 | 1.01x | 3521.8 | 1.00x | -0.7% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 380.4 | 388.9 | 1.02x | 393.3 | 1.03x | +1.1% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 142.8 | 141.8 | 0.99x | 141.2 | 0.99x | -0.4% | 0 | 0 | 36 % |  |
| el/refused-early.bool | generated | hand | 343.4 | 1123.4 | 3.27x | 568.5 | 1.66x | -49.4% | 1064 | 624 | 6 % |  |
| el/refused-late.bool | generated | hand | 1206.7 | 1789.2 | 1.48x | 916.5 | 0.76x | -48.8% | 1064 | 624 | 5 % |  |
| el/ladder.bool | generated | hand | 1032.2 | 1797.1 | 1.74x | 1771.9 | 1.72x | -1.4% | 1776 | 1720 | 11 % |  |
| sql/refused-late.bool | generated | hand | 2837.5 | 12617.2 | 4.45x | 6047.1 | 2.13x | -52.1% | 13552 | 6616 | 39 % |  |
| sql/select20.bool | generated | hand | 7398.1 | 18444.2 | 2.49x | 18382.5 | 2.48x | -0.3% | 21448 | 21392 | 11 % |  |
| el/floor | generated | hand | 326.9 | 812.7 | 2.49x | 815.9 | 2.50x | +0.4% | 1104 | 1104 | 6 % |  |
| el/floor | immediate | hand | 326.9 | 498.3 | 1.52x | 499.4 | 1.53x | +0.2% | 1120 | 1120 | 6 % |  |
| el/ladder | generated | hand | 1063.3 | 1797.3 | 1.69x | 1835.7 | 1.73x | +2.1% | 1776 | 1776 | 26 % |  |
| el/ladder | immediate | hand | 1063.3 | 1249.4 | 1.18x | 1196.1 | 1.12x | -4.3% | 1784 | 1784 | 26 % |  |
| el/nest7 | generated | hand | 644.3 | 1722.2 | 2.67x | 1741.3 | 2.70x | +1.1% | 1152 | 1152 | 11 % |  |
| el/nest7 | immediate | hand | 644.3 | 1064.8 | 1.65x | 1053.4 | 1.64x | -1.1% | 1120 | 1120 | 11 % |  |
| el/block | generated | hand | 1608.1 | 2435.8 | 1.51x | 2493.7 | 1.55x | +2.4% | 2344 | 2369 | 37 % |  |
| el/block | immediate | hand | 1608.1 | 1683.7 | 1.05x | 1867.2 | 1.16x | +10.9% | 2440 | 2440 | 37 % |  |
| el/try | generated | hand | 5394.5 | 7499.8 | 1.39x | 7172.1 | 1.33x | -4.4% | 4456 | 4456 | 44 % |  |
| el/try | immediate | hand | 5394.5 | 4949.1 | 0.92x | 5647.0 | 1.05x | +14.1% | 4152 | 4152 | 44 % |  |
| el/loop | generated | hand | 3122.2 | 5517.6 | 1.77x | 5594.8 | 1.79x | +1.4% | 4776 | 4776 | 58 % |  |
| el/loop | immediate | hand | 3122.2 | 3347.0 | 1.07x | 4420.2 | 1.42x | +32.1% | 4880 | 4880 | 58 % |  |
| el/terms100 | generated | hand | 14026.4 | 25755.8 | 1.84x | 24333.4 | 1.73x | -5.5% | 17904 | 17904 | 55 % |  |
| el/terms100 | immediate | hand | 14026.4 | 19235.1 | 1.37x | 19778.2 | 1.41x | +2.8% | 18720 | 18720 | 55 % |  |
| el/terms1000 | generated | hand | 154120.9 | 230210.2 | 1.49x | 244477.3 | 1.59x | +6.2% | 169104 | 169104 | 78 % |  |
| el/terms1000 | immediate | hand | 154120.9 | 210404.3 | 1.37x | 181525.0 | 1.18x | -13.7% | 177120 | 177123 | 78 % |  |
| el/overloads | generated | hand | 3321.2 | 4477.9 | 1.35x | 4505.2 | 1.36x | +0.6% | 4248 | 4248 | 7 % |  |
| el/overloads | immediate | hand | 3321.2 | 3646.7 | 1.10x | 3555.4 | 1.07x | -2.5% | 4200 | 4200 | 7 % |  |
| el/string | generated | hand | 354.5 | 1477.5 | 4.17x | 1491.3 | 4.21x | +0.9% | 1056 | 1056 | 66 % |  |
| el/string | immediate | hand | 354.5 | 826.6 | 2.33x | 953.2 | 2.69x | +15.3% | 1032 | 1032 | 66 % |  |
| el/interpolation | generated | hand | 2945.1 | 6845.4 | 2.32x | 6519.1 | 2.21x | -4.8% | 2368 | 2368 | 67 % |  |
| el/interpolation | immediate | hand | 2945.1 | 6498.8 | 2.21x | 5284.2 | 1.79x | -18.7% | 2424 | 2424 | 67 % |  |
| el/untyped | generated | hand | 25484.1 | 27947.0 | 1.10x | 27847.7 | 1.09x | -0.4% | 16424 | 16424 | 89 % |  |
| el/refused-early | generated | hand | 537.4 | 1367.0 | 2.54x | 1311.5 | 2.44x | -4.1% | 1064 | 1064 | 56 % |  |
| el/refused-early | immediate | hand | 537.4 | 1170.4 | 2.18x | 1322.7 | 2.46x | +13.0% | 1944 | 1944 | 56 % |  |
| el/refused-late | generated | hand | 1341.3 | 1897.7 | 1.41x | 1891.6 | 1.41x | -0.3% | 1064 | 1064 | 14 % |  |
| el/refused-late | immediate | hand | 1341.3 | 2976.7 | 2.22x | 2964.7 | 2.21x | -0.4% | 3928 | 3928 | 14 % |  |
| sql/literal | generated | hand | 60.6 | 145.2 | 2.40x | 130.4 | 2.15x | -10.2% | 160 | 160 | 8 % |  |
| sql/comment | generated | hand | 2486.7 | 5024.3 | 2.02x | 5116.2 | 2.06x | +1.8% | 5136 | 5136 | 16 % |  |
| sql/conditions100 | generated | hand | 58580.1 | 129660.1 | 2.21x | 132835.3 | 2.27x | +2.4% | 161736 | 161760 | 7 % |  |
| sql/conditions1000 | generated | hand | 584373.0 | 1294975.8 | 2.22x | 1331439.8 | 2.28x | +2.8% | 1616160 | 1616160 | 17 % |  |
| tsql/comment | generated | scriptdom | 21983.8 | 1578.5 | 0.07x | 1574.2 | 0.07x | -0.3% | 1192 | 1192 | 16 % |  |
| sql/column | generated | hand | 160.5 | 364.2 | 2.27x | 355.2 | 2.21x | -2.5% | 392 | 392 | 12 % |  |
| sql/arithmetic | generated | hand | 1813.1 | 4404.0 | 2.43x | 4456.8 | 2.46x | +1.2% | 3472 | 3472 | 24 % |  |
| sql/nest8 | generated | hand | 3041.6 | 12616.9 | 4.15x | 12726.3 | 4.18x | +0.9% | 6504 | 6504 | 6 % |  |
| sql/condition | generated | hand | 2009.7 | 4425.1 | 2.20x | 4536.2 | 2.26x | +2.5% | 4928 | 4928 | 4 % |  |
| sql/select1 | generated | hand | 742.6 | 1560.6 | 2.10x | 1576.2 | 2.12x | +1.0% | 1688 | 1688 | 6 % |  |
| sql/select20 | generated | hand | 8108.0 | 18999.2 | 2.34x | 19066.8 | 2.35x | +0.4% | 21448 | 21448 | 2 % |  |
| sql/values | generated | hand | 523.3 | 1787.1 | 3.41x | 1800.8 | 3.44x | +0.8% | 1904 | 1904 | 12 % |  |
| sql/create | generated | hand | 840.8 | 2683.0 | 3.19x | 2638.5 | 3.14x | -1.7% | 1568 | 1568 | 6 % |  |
| sql/refused-late | generated | hand | 3014.7 | 12735.9 | 4.22x | 13049.2 | 4.33x | +2.5% | 13552 | 13552 | 11 % |  |
