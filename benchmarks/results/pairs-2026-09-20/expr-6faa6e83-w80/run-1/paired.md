# Paired stand, 2026-09-20 09:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 35.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 647768.8 | 119021.9 | 0.18x | 116825.0 | 0.18x | -1.8% | 113600 | 105600 | 706 % |  |
| tsql/script100.boolboth | generated | scriptdom | 649025.8 | 121961.7 | 0.19x | 119357.8 | 0.18x | -2.1% | 105600 | 105600 | 7 % |  |
| tsql/script100 | generated | scriptdom | 660043.8 | 121222.7 | 0.18x | 121853.1 | 0.18x | +0.5% | 113600 | 113600 | 46 % |  |
| tsql/script400.bool | generated | scriptdom | 2620984.4 | 489306.2 | 0.19x | 477812.5 | 0.18x | -2.3% | 454400 | 422403 | 18 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2595850.0 | 487903.1 | 0.19x | 472643.8 | 0.18x | -3.1% | 422403 | 422400 | 13 % |  |
| tsql/script400 | generated | scriptdom | 2688115.6 | 494068.8 | 0.18x | 493796.9 | 0.18x | -0.1% | 454400 | 454400 | 41 % |  |
| tsql/columns1000 | generated | scriptdom | 1729559.4 | 187690.6 | 0.11x | 188845.3 | 0.11x | +0.6% | 200464 | 200464 | 19 % |  |
| tsql/conditions1000 | generated | scriptdom | 882332.0 | 332175.0 | 0.38x | 338343.8 | 0.38x | +1.9% | 424424 | 424424 | 7 % |  |
| tsql/rows1000 | generated | scriptdom | 595389.1 | 248503.1 | 0.42x | 243371.9 | 0.41x | -2.1% | 296472 | 296472 | 35 % |  |
| sql/select20.at | generated | hand | 7344.1 | 17783.8 | 2.42x | 17783.1 | 2.42x | 0.0% | 21416 | 21416 | 38 % |  |
| sql/select20.window | generated | hand | 7242.3 | 18107.4 | 2.50x | 18927.1 | 2.61x | +4.5% | 21416 | 21416 | 3 % |  |
| tsql/insert-values.at | generated | scriptdom | 8208.9 | 1174.9 | 0.14x | 1238.2 | 0.15x | +5.4% | 1328 | 1328 | 16 % |  |
| sql/select20.scan | generated | control | 379.4 | 381.2 | 1.00x | 381.0 | 1.00x | -0.1% | 0 | 0 | 8 % |  |
| sql/conditions100.scan | generated | control | 3619.0 | 3577.8 | 0.99x | 3535.9 | 0.98x | -1.2% | 0 | 0 | 16 % |  |
| tsql/select20.scan | generated | control | 377.2 | 377.5 | 1.00x | 383.0 | 1.02x | +1.5% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 142.0 | 137.2 | 0.97x | 137.7 | 0.97x | +0.4% | 0 | 0 | 11 % |  |
| el/refused-early.bool | generated | hand | 332.2 | 1078.7 | 3.25x | 564.5 | 1.70x | -47.7% | 1064 | 624 | 32 % |  |
| el/refused-late.bool | generated | hand | 1200.8 | 1777.3 | 1.48x | 921.1 | 0.77x | -48.2% | 1064 | 624 | 5 % |  |
| el/ladder.bool | generated | hand | 1015.0 | 1786.9 | 1.76x | 1744.9 | 1.72x | -2.4% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2708.0 | 12222.3 | 4.51x | 5898.8 | 2.18x | -51.7% | 13552 | 6616 | 33 % |  |
| sql/select20.bool | generated | hand | 7242.1 | 18367.0 | 2.54x | 18153.1 | 2.51x | -1.2% | 21448 | 21392 | 35 % |  |
| el/floor | generated | hand | 319.1 | 809.7 | 2.54x | 808.6 | 2.53x | -0.1% | 1104 | 1104 | 7 % |  |
| el/floor | immediate | hand | 319.1 | 478.3 | 1.50x | 493.0 | 1.54x | +3.1% | 1120 | 1120 | 7 % |  |
| el/ladder | generated | hand | 1013.7 | 1789.5 | 1.77x | 1748.7 | 1.73x | -2.3% | 1776 | 1776 | 13 % |  |
| el/ladder | immediate | hand | 1013.7 | 1163.2 | 1.15x | 1223.6 | 1.21x | +5.2% | 1784 | 1784 | 13 % |  |
| el/nest7 | generated | hand | 641.8 | 1757.2 | 2.74x | 1709.6 | 2.66x | -2.7% | 1152 | 1152 | 3 % |  |
| el/nest7 | immediate | hand | 641.8 | 1053.2 | 1.64x | 1077.8 | 1.68x | +2.3% | 1120 | 1120 | 3 % |  |
| el/block | generated | hand | 1041.8 | 1796.0 | 1.72x | 1802.6 | 1.73x | +0.4% | 2344 | 2344 | 7 % |  |
| el/block | immediate | hand | 1041.8 | 1165.3 | 1.12x | 1190.6 | 1.14x | +2.2% | 2440 | 2440 | 7 % |  |
| el/try | generated | hand | 3223.8 | 6165.9 | 1.91x | 6108.9 | 1.89x | -0.9% | 5632 | 5632 | 22 % |  |
| el/try | immediate | hand | 3223.8 | 4725.0 | 1.47x | 4736.3 | 1.47x | +0.2% | 5752 | 5752 | 22 % |  |
| el/loop | generated | hand | 2200.0 | 3718.8 | 1.69x | 3707.5 | 1.69x | -0.3% | 4776 | 4776 | 7 % |  |
| el/loop | immediate | hand | 2200.0 | 2528.0 | 1.15x | 2574.0 | 1.17x | +1.8% | 4880 | 4880 | 7 % |  |
| el/terms100 | generated | hand | 12099.5 | 17128.3 | 1.42x | 17707.6 | 1.46x | +3.4% | 17904 | 17904 | 66 % |  |
| el/terms100 | immediate | hand | 12099.5 | 15154.2 | 1.25x | 14589.1 | 1.21x | -3.7% | 18720 | 18720 | 66 % |  |
| el/terms1000 | generated | hand | 111953.7 | 151390.4 | 1.35x | 153742.6 | 1.37x | +1.6% | 169104 | 169131 | 2 % |  |
| el/terms1000 | immediate | hand | 111953.7 | 122563.3 | 1.09x | 131002.7 | 1.17x | +6.9% | 177120 | 177120 | 2 % |  |
| el/overloads | generated | hand | 1893.0 | 2599.1 | 1.37x | 2573.2 | 1.36x | -1.0% | 4248 | 4248 | 11 % |  |
| el/overloads | immediate | hand | 1893.0 | 2037.0 | 1.08x | 2099.1 | 1.11x | +3.0% | 4200 | 4200 | 11 % |  |
| el/string | generated | hand | 289.3 | 955.3 | 3.30x | 963.9 | 3.33x | +0.9% | 1056 | 1056 | 9 % |  |
| el/string | immediate | hand | 289.3 | 654.6 | 2.26x | 685.7 | 2.37x | +4.7% | 1032 | 1032 | 9 % |  |
| el/interpolation | generated | hand | 1891.8 | 4475.2 | 2.37x | 5068.5 | 2.68x | +13.3% | 2368 | 2368 | 66 % |  |
| el/interpolation | immediate | hand | 1891.8 | 4389.6 | 2.32x | 3944.4 | 2.08x | -10.1% | 2424 | 2424 | 66 % |  |
| el/untyped | generated | hand | 26850.0 | 29060.5 | 1.08x | 28601.6 | 1.07x | -1.6% | 16424 | 16424 | 161 % |  |
| el/refused-early | generated | hand | 477.0 | 1232.9 | 2.58x | 1282.6 | 2.69x | +4.0% | 1064 | 1064 | 26 % |  |
| el/refused-early | immediate | hand | 477.0 | 1222.1 | 2.56x | 1255.2 | 2.63x | +2.7% | 1944 | 1944 | 26 % |  |
| el/refused-late | generated | hand | 1563.8 | 1950.1 | 1.25x | 2003.6 | 1.28x | +2.7% | 1064 | 1064 | 19 % |  |
| el/refused-late | immediate | hand | 1563.8 | 3223.2 | 2.06x | 3342.9 | 2.14x | +3.7% | 3928 | 3928 | 19 % |  |
| sql/literal | generated | hand | 66.8 | 153.1 | 2.29x | 151.0 | 2.26x | -1.3% | 160 | 160 | 24 % |  |
| sql/comment | generated | hand | 2934.9 | 5353.9 | 1.82x | 5368.4 | 1.83x | +0.3% | 5136 | 5136 | 17 % |  |
| sql/conditions100 | generated | hand | 73850.6 | 144116.7 | 1.95x | 138545.5 | 1.88x | -3.9% | 161736 | 161736 | 12 % |  |
| sql/conditions1000 | generated | hand | 732731.2 | 1465861.7 | 2.00x | 1464878.9 | 2.00x | -0.1% | 1616136 | 1616136 | 23 % |  |
| tsql/comment | generated | scriptdom | 27458.9 | 1601.4 | 0.06x | 1630.8 | 0.06x | +1.8% | 1192 | 1192 | 12 % |  |
| sql/column | generated | hand | 201.3 | 411.6 | 2.05x | 466.4 | 2.32x | +13.3% | 392 | 392 | 18 % |  |
| sql/arithmetic | generated | hand | 2158.0 | 4826.3 | 2.24x | 4799.8 | 2.22x | -0.5% | 3472 | 3472 | 46 % |  |
| sql/nest8 | generated | hand | 3566.7 | 13146.3 | 3.69x | 12378.2 | 3.47x | -5.8% | 6504 | 6504 | 18 % |  |
| sql/condition | generated | hand | 2598.4 | 5125.9 | 1.97x | 5038.9 | 1.94x | -1.7% | 4928 | 4928 | 25 % |  |
| sql/select1 | generated | hand | 929.9 | 1835.4 | 1.97x | 1786.3 | 1.92x | -2.7% | 1688 | 1688 | 19 % |  |
| sql/select20 | generated | hand | 11103.6 | 21927.1 | 1.97x | 21961.3 | 1.98x | +0.2% | 21448 | 21448 | 15 % |  |
| sql/values | generated | hand | 733.4 | 2054.2 | 2.80x | 1981.3 | 2.70x | -3.5% | 1904 | 1904 | 31 % |  |
| sql/create | generated | hand | 1111.6 | 3132.3 | 2.82x | 3131.5 | 2.82x | 0.0% | 1568 | 1568 | 46 % |  |
| sql/refused-late | generated | hand | 3157.1 | 13161.2 | 4.17x | 13048.3 | 4.13x | -0.9% | 13552 | 13552 | 22 % |  |
