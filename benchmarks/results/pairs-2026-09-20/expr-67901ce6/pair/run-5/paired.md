# Paired stand, 2026-09-20 04:39

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1011284.4 | 124681.2 | 0.12x | 124487.5 | 0.12x | -0.2% | 113600 | 105600 | 453 % |  |
| tsql/script100.boolboth | generated | scriptdom | 650697.7 | 121528.1 | 0.19x | 123939.8 | 0.19x | +2.0% | 105600 | 105600 | 14 % |  |
| tsql/script100 | generated | scriptdom | 654350.0 | 122789.8 | 0.19x | 124128.1 | 0.19x | +1.1% | 113600 | 113600 | 6 % |  |
| tsql/script400.bool | generated | scriptdom | 2630128.1 | 495500.0 | 0.19x | 493428.1 | 0.19x | -0.4% | 454400 | 422403 | 5 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2733165.6 | 489662.5 | 0.18x | 501865.6 | 0.18x | +2.5% | 422403 | 422400 | 17 % |  |
| tsql/script400 | generated | scriptdom | 2620709.4 | 493687.5 | 0.19x | 495328.1 | 0.19x | +0.3% | 454400 | 454400 | 4 % |  |
| tsql/columns1000 | generated | scriptdom | 1731717.2 | 187734.4 | 0.11x | 188400.0 | 0.11x | +0.4% | 200464 | 200464 | 5 % |  |
| tsql/conditions1000 | generated | scriptdom | 874801.6 | 334790.6 | 0.38x | 331998.4 | 0.38x | -0.8% | 424424 | 424424 | 9 % |  |
| tsql/rows1000 | generated | scriptdom | 582758.6 | 240937.5 | 0.41x | 242663.3 | 0.42x | +0.7% | 296472 | 296472 | 22 % |  |
| sql/select20.at | generated | hand | 7117.5 | 17409.9 | 2.45x | 17930.7 | 2.52x | +3.0% | 21416 | 21416 | 7 % |  |
| sql/select20.window | generated | hand | 7121.4 | 17939.3 | 2.52x | 18364.5 | 2.58x | +2.4% | 21416 | 21416 | 6 % |  |
| tsql/insert-values.at | generated | scriptdom | 8022.5 | 1154.4 | 0.14x | 1194.3 | 0.15x | +3.5% | 1328 | 1328 | 1 % |  |
| sql/select20.scan | generated | control | 384.1 | 385.3 | 1.00x | 389.1 | 1.01x | +1.0% | 0 | 0 | 6 % |  |
| sql/conditions100.scan | generated | control | 3505.0 | 3502.4 | 1.00x | 3565.7 | 1.02x | +1.8% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 398.1 | 383.0 | 0.96x | 383.0 | 0.96x | 0.0% | 0 | 0 | 28 % |  |
| el/ladder.scan | generated | control | 137.5 | 137.3 | 1.00x | 137.0 | 1.00x | -0.2% | 0 | 0 | 14 % |  |
| el/refused-early.bool | generated | hand | 333.9 | 1085.9 | 3.25x | 556.4 | 1.67x | -48.8% | 1064 | 624 | 8 % |  |
| el/refused-late.bool | generated | hand | 1172.5 | 1776.9 | 1.52x | 902.5 | 0.77x | -49.2% | 1064 | 624 | 4 % |  |
| el/ladder.bool | generated | hand | 1009.4 | 1751.6 | 1.74x | 1709.7 | 1.69x | -2.4% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2643.5 | 11978.4 | 4.53x | 5878.9 | 2.22x | -50.9% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 7100.6 | 17749.6 | 2.50x | 18594.0 | 2.62x | +4.8% | 21448 | 21392 | 6 % |  |
| el/floor | generated | hand | 323.6 | 795.2 | 2.46x | 804.3 | 2.49x | +1.1% | 1104 | 1104 | 24 % |  |
| el/floor | immediate | hand | 323.6 | 467.7 | 1.45x | 466.7 | 1.44x | -0.2% | 1120 | 1120 | 24 % |  |
| el/ladder | generated | hand | 1014.4 | 1753.1 | 1.73x | 1736.7 | 1.71x | -0.9% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 1014.4 | 1158.3 | 1.14x | 1178.2 | 1.16x | +1.7% | 1784 | 1784 | 10 % |  |
| el/nest7 | generated | hand | 634.6 | 1740.9 | 2.74x | 1705.3 | 2.69x | -2.0% | 1152 | 1152 | 2 % |  |
| el/nest7 | immediate | hand | 634.6 | 1037.0 | 1.63x | 1035.8 | 1.63x | -0.1% | 1120 | 1120 | 2 % |  |
| el/block | generated | hand | 1072.2 | 1780.7 | 1.66x | 1823.2 | 1.70x | +2.4% | 2344 | 2344 | 3 % |  |
| el/block | immediate | hand | 1072.2 | 1133.5 | 1.06x | 1158.4 | 1.08x | +2.2% | 2440 | 2440 | 3 % |  |
| el/try | generated | hand | 3212.7 | 4672.9 | 1.45x | 4661.4 | 1.45x | -0.2% | 4456 | 4456 | 22 % |  |
| el/try | immediate | hand | 3212.7 | 3231.0 | 1.01x | 3232.1 | 1.01x | 0.0% | 4152 | 4152 | 22 % |  |
| el/loop | generated | hand | 2222.2 | 3710.0 | 1.67x | 3705.8 | 1.67x | -0.1% | 4776 | 4776 | 2 % |  |
| el/loop | immediate | hand | 2222.2 | 2564.6 | 1.15x | 2527.6 | 1.14x | -1.4% | 4880 | 4880 | 2 % |  |
| el/terms100 | generated | hand | 10922.4 | 16256.8 | 1.49x | 15857.2 | 1.45x | -2.5% | 17904 | 17904 | 3 % |  |
| el/terms100 | immediate | hand | 10922.4 | 12685.3 | 1.16x | 12467.9 | 1.14x | -1.7% | 18720 | 18720 | 3 % |  |
| el/terms1000 | generated | hand | 106596.7 | 153433.0 | 1.44x | 150332.5 | 1.41x | -2.0% | 169104 | 169104 | 9 % |  |
| el/terms1000 | immediate | hand | 106596.7 | 123045.9 | 1.15x | 120032.5 | 1.13x | -2.4% | 177123 | 177120 | 9 % |  |
| el/overloads | generated | hand | 1903.7 | 2584.2 | 1.36x | 2632.2 | 1.38x | +1.9% | 4248 | 4248 | 9 % |  |
| el/overloads | immediate | hand | 1903.7 | 2086.3 | 1.10x | 2076.3 | 1.09x | -0.5% | 4200 | 4200 | 9 % |  |
| el/string | generated | hand | 294.0 | 951.3 | 3.24x | 964.3 | 3.28x | +1.4% | 1056 | 1056 | 23 % |  |
| el/string | immediate | hand | 294.0 | 662.7 | 2.25x | 667.8 | 2.27x | +0.8% | 1032 | 1032 | 23 % |  |
| el/interpolation | generated | hand | 2351.3 | 4727.7 | 2.01x | 4976.4 | 2.12x | +5.3% | 2368 | 2368 | 89 % |  |
| el/interpolation | immediate | hand | 2351.3 | 4486.1 | 1.91x | 3920.2 | 1.67x | -12.6% | 2424 | 2424 | 89 % |  |
| el/untyped | generated | hand | 18100.7 | 19722.4 | 1.09x | 19816.3 | 1.09x | +0.5% | 16424 | 16424 | 136 % |  |
| el/refused-early | generated | hand | 509.9 | 1265.8 | 2.48x | 1311.0 | 2.57x | +3.6% | 1064 | 1064 | 13 % |  |
| el/refused-early | immediate | hand | 509.9 | 1280.0 | 2.51x | 1287.0 | 2.52x | +0.5% | 1944 | 1944 | 13 % |  |
| el/refused-late | generated | hand | 1537.6 | 2004.7 | 1.30x | 2019.0 | 1.31x | +0.7% | 1064 | 1064 | 17 % |  |
| el/refused-late | immediate | hand | 1537.6 | 3376.4 | 2.20x | 3337.7 | 2.17x | -1.1% | 3928 | 3928 | 17 % |  |
| sql/literal | generated | hand | 70.7 | 153.4 | 2.17x | 165.1 | 2.33x | +7.6% | 160 | 160 | 4 % |  |
| sql/comment | generated | hand | 3061.2 | 5442.0 | 1.78x | 5511.2 | 1.80x | +1.3% | 5136 | 5136 | 17 % |  |
| sql/conditions100 | generated | hand | 78535.2 | 139412.3 | 1.78x | 142637.0 | 1.82x | +2.3% | 161736 | 161736 | 17 % |  |
| sql/conditions1000 | generated | hand | 805230.5 | 1440826.6 | 1.79x | 1466571.1 | 1.82x | +1.8% | 1616160 | 1616136 | 21 % |  |
| tsql/comment | generated | scriptdom | 27351.1 | 1580.2 | 0.06x | 1584.6 | 0.06x | +0.3% | 1192 | 1192 | 16 % |  |
| sql/column | generated | hand | 200.6 | 406.0 | 2.02x | 402.8 | 2.01x | -0.8% | 392 | 392 | 20 % |  |
| sql/arithmetic | generated | hand | 2079.2 | 4601.5 | 2.21x | 4561.4 | 2.19x | -0.9% | 3472 | 3472 | 30 % |  |
| sql/nest8 | generated | hand | 3676.8 | 12024.0 | 3.27x | 12126.5 | 3.30x | +0.9% | 6504 | 6504 | 14 % |  |
| sql/condition | generated | hand | 2688.7 | 5151.4 | 1.92x | 5132.2 | 1.91x | -0.4% | 4928 | 4928 | 20 % |  |
| sql/select1 | generated | hand | 920.5 | 1746.5 | 1.90x | 1744.1 | 1.89x | -0.1% | 1688 | 1688 | 18 % |  |
| sql/select20 | generated | hand | 10884.2 | 21633.4 | 1.99x | 22105.3 | 2.03x | +2.2% | 21448 | 21448 | 17 % |  |
| sql/values | generated | hand | 705.8 | 2083.2 | 2.95x | 1943.5 | 2.75x | -6.7% | 1904 | 1904 | 19 % |  |
| sql/create | generated | hand | 872.1 | 2693.3 | 3.09x | 2668.7 | 3.06x | -0.9% | 1568 | 1568 | 32 % |  |
| sql/refused-late | generated | hand | 3067.2 | 12749.6 | 4.16x | 12979.4 | 4.23x | +1.8% | 13552 | 13552 | 23 % |  |
