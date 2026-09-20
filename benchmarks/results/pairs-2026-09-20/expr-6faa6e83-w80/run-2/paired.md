# Paired stand, 2026-09-20 09:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 798240.6 | 123525.0 | 0.15x | 122831.2 | 0.15x | -0.6% | 113600 | 105600 | 582 % |  |
| tsql/script100.boolboth | generated | scriptdom | 667878.1 | 126753.9 | 0.19x | 124536.7 | 0.19x | -1.7% | 105600 | 105600 | 9 % |  |
| tsql/script100 | generated | scriptdom | 690222.7 | 125514.1 | 0.18x | 129429.7 | 0.19x | +3.1% | 113600 | 113600 | 10 % |  |
| tsql/script400.bool | generated | scriptdom | 2774600.0 | 506859.4 | 0.18x | 515356.2 | 0.19x | +1.7% | 454400 | 422400 | 11 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2854718.8 | 538737.5 | 0.19x | 509196.9 | 0.18x | -5.5% | 422403 | 422403 | 33 % |  |
| tsql/script400 | generated | scriptdom | 2703021.9 | 507906.2 | 0.19x | 509940.6 | 0.19x | +0.4% | 454400 | 454400 | 6 % |  |
| tsql/columns1000 | generated | scriptdom | 1801431.2 | 192793.8 | 0.11x | 189192.2 | 0.11x | -1.9% | 200464 | 200464 | 8 % |  |
| tsql/conditions1000 | generated | scriptdom | 882503.9 | 335251.6 | 0.38x | 340050.8 | 0.39x | +1.4% | 424424 | 424424 | 6 % |  |
| tsql/rows1000 | generated | scriptdom | 585764.8 | 241982.0 | 0.41x | 246496.9 | 0.42x | +1.9% | 296472 | 296472 | 7 % |  |
| sql/select20.at | generated | hand | 7245.7 | 17664.2 | 2.44x | 17574.6 | 2.43x | -0.5% | 21416 | 21416 | 8 % |  |
| sql/select20.window | generated | hand | 7303.6 | 18435.2 | 2.52x | 18304.4 | 2.51x | -0.7% | 21416 | 21416 | 4 % |  |
| tsql/insert-values.at | generated | scriptdom | 8116.5 | 1185.3 | 0.15x | 1206.7 | 0.15x | +1.8% | 1328 | 1328 | 6 % |  |
| sql/select20.scan | generated | control | 377.6 | 390.4 | 1.03x | 386.6 | 1.02x | -1.0% | 0 | 0 | 4 % |  |
| sql/conditions100.scan | generated | control | 3564.1 | 3675.5 | 1.03x | 3500.3 | 0.98x | -4.8% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 394.0 | 395.5 | 1.00x | 383.8 | 0.97x | -2.9% | 0 | 0 | 20 % |  |
| el/ladder.scan | generated | control | 141.3 | 139.0 | 0.98x | 138.4 | 0.98x | -0.4% | 0 | 0 | 50 % |  |
| el/refused-early.bool | generated | hand | 355.2 | 1131.6 | 3.19x | 563.4 | 1.59x | -50.2% | 1064 | 624 | 48 % |  |
| el/refused-late.bool | generated | hand | 1190.2 | 1800.8 | 1.51x | 914.5 | 0.77x | -49.2% | 1064 | 624 | 9 % |  |
| el/ladder.bool | generated | hand | 1013.9 | 1758.8 | 1.73x | 1713.4 | 1.69x | -2.6% | 1776 | 1720 | 11 % |  |
| sql/refused-late.bool | generated | hand | 2713.6 | 12265.8 | 4.52x | 5862.6 | 2.16x | -52.2% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 7359.4 | 18133.9 | 2.46x | 18091.2 | 2.46x | -0.2% | 21448 | 21392 | 6 % |  |
| el/floor | generated | hand | 312.1 | 818.7 | 2.62x | 811.0 | 2.60x | -0.9% | 1104 | 1104 | 9 % |  |
| el/floor | immediate | hand | 312.1 | 472.5 | 1.51x | 471.2 | 1.51x | -0.3% | 1120 | 1120 | 9 % |  |
| el/ladder | generated | hand | 1025.4 | 1776.0 | 1.73x | 1752.8 | 1.71x | -1.3% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 1025.4 | 1178.0 | 1.15x | 1195.0 | 1.17x | +1.4% | 1784 | 1784 | 7 % |  |
| el/nest7 | generated | hand | 644.8 | 1720.8 | 2.67x | 1697.0 | 2.63x | -1.4% | 1152 | 1152 | 5 % |  |
| el/nest7 | immediate | hand | 644.8 | 1057.7 | 1.64x | 1060.8 | 1.65x | +0.3% | 1120 | 1120 | 5 % |  |
| el/block | generated | hand | 1083.0 | 1891.4 | 1.75x | 1802.0 | 1.66x | -4.7% | 2344 | 2344 | 4 % |  |
| el/block | immediate | hand | 1083.0 | 1171.6 | 1.08x | 1187.2 | 1.10x | +1.3% | 2440 | 2440 | 4 % |  |
| el/try | generated | hand | 3311.6 | 6201.2 | 1.87x | 6284.6 | 1.90x | +1.3% | 5632 | 5632 | 6 % |  |
| el/try | immediate | hand | 3311.6 | 4842.8 | 1.46x | 4812.7 | 1.45x | -0.6% | 5752 | 5752 | 6 % |  |
| el/loop | generated | hand | 2251.7 | 3784.3 | 1.68x | 3855.6 | 1.71x | +1.9% | 4776 | 4776 | 4 % |  |
| el/loop | immediate | hand | 2251.7 | 2615.0 | 1.16x | 2597.8 | 1.15x | -0.7% | 4880 | 4880 | 4 % |  |
| el/terms100 | generated | hand | 11392.7 | 16675.3 | 1.46x | 16739.7 | 1.47x | +0.4% | 17907 | 17904 | 14 % |  |
| el/terms100 | immediate | hand | 11392.7 | 12926.5 | 1.13x | 13258.7 | 1.16x | +2.6% | 18720 | 18720 | 14 % |  |
| el/terms1000 | generated | hand | 112446.2 | 157422.0 | 1.40x | 160429.4 | 1.43x | +1.9% | 169104 | 169104 | 10 % |  |
| el/terms1000 | immediate | hand | 112446.2 | 125076.6 | 1.11x | 127300.0 | 1.13x | +1.8% | 177120 | 177120 | 10 % |  |
| el/overloads | generated | hand | 1892.9 | 2649.1 | 1.40x | 2686.8 | 1.42x | +1.4% | 4248 | 4248 | 6 % |  |
| el/overloads | immediate | hand | 1892.9 | 2069.0 | 1.09x | 2078.0 | 1.10x | +0.4% | 4200 | 4200 | 6 % |  |
| el/string | generated | hand | 290.0 | 987.0 | 3.40x | 1018.0 | 3.51x | +3.1% | 1056 | 1056 | 26 % |  |
| el/string | immediate | hand | 290.0 | 669.9 | 2.31x | 689.6 | 2.38x | +2.9% | 1032 | 1032 | 26 % |  |
| el/interpolation | generated | hand | 2313.4 | 5069.4 | 2.19x | 5068.5 | 2.19x | 0.0% | 2368 | 2368 | 52 % |  |
| el/interpolation | immediate | hand | 2313.4 | 4594.1 | 1.99x | 4591.2 | 1.98x | -0.1% | 2424 | 2424 | 52 % |  |
| el/untyped | generated | hand | 27308.5 | 28773.7 | 1.05x | 28590.9 | 1.05x | -0.6% | 16424 | 16424 | 64 % |  |
| el/refused-early | generated | hand | 511.9 | 1300.2 | 2.54x | 1309.8 | 2.56x | +0.7% | 1064 | 1064 | 17 % |  |
| el/refused-early | immediate | hand | 511.9 | 1296.3 | 2.53x | 1310.4 | 2.56x | +1.1% | 1944 | 1944 | 17 % |  |
| el/refused-late | generated | hand | 1481.3 | 2019.6 | 1.36x | 2045.4 | 1.38x | +1.3% | 1064 | 1064 | 18 % |  |
| el/refused-late | immediate | hand | 1481.3 | 3375.3 | 2.28x | 3447.2 | 2.33x | +2.1% | 3928 | 3928 | 18 % |  |
| sql/literal | generated | hand | 66.5 | 153.7 | 2.31x | 158.1 | 2.38x | +2.9% | 160 | 160 | 68 % |  |
| sql/comment | generated | hand | 3097.2 | 5581.3 | 1.80x | 5591.9 | 1.81x | +0.2% | 5136 | 5136 | 24 % |  |
| sql/conditions100 | generated | hand | 82266.5 | 147705.3 | 1.80x | 148202.7 | 1.80x | +0.3% | 161736 | 161736 | 8 % |  |
| sql/conditions1000 | generated | hand | 875190.6 | 1468371.9 | 1.68x | 1510678.1 | 1.73x | +2.9% | 1616136 | 1616136 | 22 % |  |
| tsql/comment | generated | scriptdom | 28078.8 | 1611.3 | 0.06x | 1728.2 | 0.06x | +7.3% | 1192 | 1192 | 15 % |  |
| sql/column | generated | hand | 197.8 | 430.3 | 2.18x | 438.2 | 2.22x | +1.8% | 392 | 392 | 17 % |  |
| sql/arithmetic | generated | hand | 2314.6 | 4562.8 | 1.97x | 4757.7 | 2.06x | +4.3% | 3472 | 3472 | 33 % |  |
| sql/nest8 | generated | hand | 3731.3 | 12409.2 | 3.33x | 13130.8 | 3.52x | +5.8% | 6504 | 6504 | 18 % |  |
| sql/condition | generated | hand | 2720.7 | 4852.2 | 1.78x | 5140.2 | 1.89x | +5.9% | 4928 | 4928 | 9 % |  |
| sql/select1 | generated | hand | 967.4 | 1752.3 | 1.81x | 1758.3 | 1.82x | +0.3% | 1688 | 1688 | 21 % |  |
| sql/select20 | generated | hand | 11062.8 | 21953.9 | 1.98x | 22170.3 | 2.00x | +1.0% | 21448 | 21448 | 42 % |  |
| sql/values | generated | hand | 727.1 | 1994.6 | 2.74x | 2041.6 | 2.81x | +2.4% | 1904 | 1904 | 17 % |  |
| sql/create | generated | hand | 1062.1 | 2876.2 | 2.71x | 2846.2 | 2.68x | -1.0% | 1568 | 1568 | 13 % |  |
| sql/refused-late | generated | hand | 3669.2 | 14215.8 | 3.87x | 14254.1 | 3.88x | +0.3% | 13552 | 13552 | 34 % |  |
