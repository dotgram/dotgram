# Paired stand, 2026-09-20 04:21

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 982881.2 | 131793.8 | 0.13x | 157018.8 | 0.16x | +19.1% | 113600 | 105600 | 466 % |  |
| tsql/script100.boolboth | generated | scriptdom | 665809.4 | 122821.1 | 0.18x | 123314.8 | 0.19x | +0.4% | 105600 | 105600 | 19 % |  |
| tsql/script100 | generated | scriptdom | 664188.3 | 125963.3 | 0.19x | 124364.8 | 0.19x | -1.3% | 113600 | 113600 | 5 % |  |
| tsql/script400.bool | generated | scriptdom | 2656646.9 | 508021.9 | 0.19x | 495962.5 | 0.19x | -2.4% | 454400 | 422403 | 26 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2709650.0 | 492150.0 | 0.18x | 502628.1 | 0.19x | +2.1% | 422400 | 422403 | 21 % |  |
| tsql/script400 | generated | scriptdom | 2689615.6 | 515350.0 | 0.19x | 506443.8 | 0.19x | -1.7% | 454400 | 454400 | 6 % |  |
| tsql/columns1000 | generated | scriptdom | 1727826.6 | 189648.4 | 0.11x | 187679.7 | 0.11x | -1.0% | 200464 | 200464 | 6 % |  |
| tsql/conditions1000 | generated | scriptdom | 904678.9 | 342447.7 | 0.38x | 345312.5 | 0.38x | +0.8% | 424424 | 424424 | 10 % |  |
| tsql/rows1000 | generated | scriptdom | 590906.2 | 244362.5 | 0.41x | 253711.7 | 0.43x | +3.8% | 296472 | 296472 | 9 % |  |
| sql/select20.at | generated | hand | 7588.8 | 18143.6 | 2.39x | 18318.4 | 2.41x | +1.0% | 21416 | 21416 | 19 % |  |
| sql/select20.window | generated | hand | 7666.0 | 18879.8 | 2.46x | 19045.1 | 2.48x | +0.9% | 21416 | 21416 | 35 % |  |
| tsql/insert-values.at | generated | scriptdom | 8884.7 | 1210.6 | 0.14x | 1237.8 | 0.14x | +2.2% | 1328 | 1328 | 29 % |  |
| sql/select20.scan | generated | control | 386.7 | 395.4 | 1.02x | 393.6 | 1.02x | -0.4% | 0 | 0 | 48 % |  |
| sql/conditions100.scan | generated | control | 3569.8 | 3599.3 | 1.01x | 3655.6 | 1.02x | +1.6% | 0 | 0 | 21 % |  |
| tsql/select20.scan | generated | control | 385.4 | 385.8 | 1.00x | 388.3 | 1.01x | +0.6% | 0 | 0 | 20 % |  |
| el/ladder.scan | generated | control | 143.2 | 139.1 | 0.97x | 143.0 | 1.00x | +2.8% | 0 | 0 | 11 % |  |
| el/refused-early.bool | generated | hand | 594.2 | 1637.7 | 2.76x | 827.9 | 1.39x | -49.4% | 1064 | 624 | 46 % |  |
| el/refused-late.bool | generated | hand | 1227.6 | 1837.6 | 1.50x | 925.4 | 0.75x | -49.6% | 1064 | 624 | 22 % |  |
| el/ladder.bool | generated | hand | 1047.3 | 1831.2 | 1.75x | 1748.3 | 1.67x | -4.5% | 1776 | 1720 | 34 % |  |
| sql/refused-late.bool | generated | hand | 2728.5 | 12534.6 | 4.59x | 6121.1 | 2.24x | -51.2% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 7330.8 | 18465.1 | 2.52x | 18512.3 | 2.53x | +0.3% | 21448 | 21392 | 6 % |  |
| el/floor | generated | hand | 336.3 | 835.8 | 2.48x | 841.0 | 2.50x | +0.6% | 1104 | 1104 | 20 % |  |
| el/floor | immediate | hand | 336.3 | 510.3 | 1.52x | 493.6 | 1.47x | -3.3% | 1120 | 1120 | 20 % |  |
| el/ladder | generated | hand | 1039.2 | 1786.5 | 1.72x | 1767.9 | 1.70x | -1.0% | 1776 | 1776 | 23 % |  |
| el/ladder | immediate | hand | 1039.2 | 1181.5 | 1.14x | 1194.5 | 1.15x | +1.1% | 1784 | 1784 | 23 % |  |
| el/nest7 | generated | hand | 661.6 | 1790.4 | 2.71x | 1762.7 | 2.66x | -1.5% | 1152 | 1152 | 10 % |  |
| el/nest7 | immediate | hand | 661.6 | 1075.4 | 1.63x | 1086.9 | 1.64x | +1.1% | 1120 | 1120 | 10 % |  |
| el/block | generated | hand | 1102.9 | 1922.2 | 1.74x | 1884.6 | 1.71x | -2.0% | 2344 | 2344 | 8 % |  |
| el/block | immediate | hand | 1102.9 | 1192.1 | 1.08x | 1206.6 | 1.09x | +1.2% | 2440 | 2440 | 8 % |  |
| el/try | generated | hand | 3356.7 | 4842.3 | 1.44x | 4766.5 | 1.42x | -1.6% | 4456 | 4456 | 9 % |  |
| el/try | immediate | hand | 3356.7 | 3397.0 | 1.01x | 3450.8 | 1.03x | +1.6% | 4152 | 4152 | 9 % |  |
| el/loop | generated | hand | 2327.9 | 3891.6 | 1.67x | 3913.3 | 1.68x | +0.6% | 4776 | 4776 | 8 % |  |
| el/loop | immediate | hand | 2327.9 | 2634.6 | 1.13x | 2753.3 | 1.18x | +4.5% | 4880 | 4880 | 8 % |  |
| el/terms100 | generated | hand | 12088.8 | 17077.0 | 1.41x | 16640.5 | 1.38x | -2.6% | 17904 | 17904 | 16 % |  |
| el/terms100 | immediate | hand | 12088.8 | 13371.8 | 1.11x | 13176.9 | 1.09x | -1.5% | 18720 | 18720 | 16 % |  |
| el/terms1000 | generated | hand | 114056.2 | 158403.6 | 1.39x | 155898.4 | 1.37x | -1.6% | 169104 | 169129 | 17 % |  |
| el/terms1000 | immediate | hand | 114056.2 | 126838.8 | 1.11x | 126237.7 | 1.11x | -0.5% | 177123 | 177120 | 17 % |  |
| el/overloads | generated | hand | 1938.4 | 2706.6 | 1.40x | 2667.7 | 1.38x | -1.4% | 4248 | 4248 | 7 % |  |
| el/overloads | immediate | hand | 1938.4 | 2133.3 | 1.10x | 2109.7 | 1.09x | -1.1% | 4200 | 4200 | 7 % |  |
| el/string | generated | hand | 309.0 | 996.6 | 3.23x | 983.8 | 3.18x | -1.3% | 1056 | 1056 | 15 % |  |
| el/string | immediate | hand | 309.0 | 681.4 | 2.20x | 696.5 | 2.25x | +2.2% | 1032 | 1032 | 15 % |  |
| el/interpolation | generated | hand | 2289.5 | 4639.3 | 2.03x | 5347.1 | 2.34x | +15.3% | 2368 | 2368 | 56 % |  |
| el/interpolation | immediate | hand | 2289.5 | 4375.1 | 1.91x | 4183.1 | 1.83x | -4.4% | 2424 | 2424 | 56 % |  |
| el/untyped | generated | hand | 18620.5 | 19984.0 | 1.07x | 19926.6 | 1.07x | -0.3% | 16424 | 16424 | 193 % |  |
| el/refused-early | generated | hand | 483.4 | 1252.7 | 2.59x | 1289.5 | 2.67x | +2.9% | 1064 | 1064 | 15 % |  |
| el/refused-early | immediate | hand | 483.4 | 1272.9 | 2.63x | 1271.8 | 2.63x | -0.1% | 1944 | 1944 | 15 % |  |
| el/refused-late | generated | hand | 1393.9 | 1950.4 | 1.40x | 1978.2 | 1.42x | +1.4% | 1064 | 1064 | 32 % |  |
| el/refused-late | immediate | hand | 1393.9 | 3355.9 | 2.41x | 3262.2 | 2.34x | -2.8% | 3928 | 3928 | 32 % |  |
| sql/literal | generated | hand | 63.3 | 154.7 | 2.44x | 147.6 | 2.33x | -4.6% | 160 | 160 | 19 % |  |
| sql/comment | generated | hand | 2909.4 | 5338.0 | 1.83x | 5575.7 | 1.92x | +4.5% | 5136 | 5136 | 14 % |  |
| sql/conditions100 | generated | hand | 72180.9 | 138374.1 | 1.92x | 141762.7 | 1.96x | +2.4% | 161736 | 161736 | 23 % |  |
| sql/conditions1000 | generated | hand | 741152.3 | 1417428.9 | 1.91x | 1448953.9 | 1.96x | +2.2% | 1616136 | 1616136 | 15 % |  |
| tsql/comment | generated | scriptdom | 26600.3 | 1607.2 | 0.06x | 1598.0 | 0.06x | -0.6% | 1192 | 1192 | 11 % |  |
| sql/column | generated | hand | 183.6 | 408.7 | 2.23x | 406.7 | 2.21x | -0.5% | 392 | 392 | 16 % |  |
| sql/arithmetic | generated | hand | 2157.9 | 4535.3 | 2.10x | 4755.5 | 2.20x | +4.9% | 3472 | 3472 | 8 % |  |
| sql/nest8 | generated | hand | 3595.2 | 12342.7 | 3.43x | 12874.5 | 3.58x | +4.3% | 6504 | 6504 | 27 % |  |
| sql/condition | generated | hand | 2528.6 | 4893.7 | 1.94x | 5038.8 | 1.99x | +3.0% | 4928 | 4928 | 14 % |  |
| sql/select1 | generated | hand | 865.7 | 1720.0 | 1.99x | 1781.5 | 2.06x | +3.6% | 1688 | 1688 | 13 % |  |
| sql/select20 | generated | hand | 10173.3 | 21035.5 | 2.07x | 21709.0 | 2.13x | +3.2% | 21448 | 21448 | 12 % |  |
| sql/values | generated | hand | 642.3 | 1899.3 | 2.96x | 1970.9 | 3.07x | +3.8% | 1904 | 1904 | 20 % |  |
| sql/create | generated | hand | 965.1 | 2810.0 | 2.91x | 2784.2 | 2.88x | -0.9% | 1568 | 1568 | 14 % |  |
| sql/refused-late | generated | hand | 3799.4 | 13983.0 | 3.68x | 14102.9 | 3.71x | +0.9% | 13552 | 13552 | 12 % |  |
