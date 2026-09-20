# Paired stand, 2026-09-20 10:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 877440.6 | 120262.5 | 0.14x | 121256.2 | 0.14x | +0.8% | 113600 | 105600 | 527 % |  |
| tsql/script100.boolboth | generated | scriptdom | 658157.8 | 118450.0 | 0.18x | 119867.2 | 0.18x | +1.2% | 105600 | 105600 | 12 % |  |
| tsql/script100 | generated | scriptdom | 661127.3 | 119760.2 | 0.18x | 119819.5 | 0.18x | 0.0% | 113600 | 113600 | 18 % |  |
| tsql/script400.bool | generated | scriptdom | 2645446.9 | 477540.6 | 0.18x | 484662.5 | 0.18x | +1.5% | 454400 | 422403 | 11 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2645621.9 | 476387.5 | 0.18x | 484709.4 | 0.18x | +1.7% | 422403 | 422400 | 12 % |  |
| tsql/script400 | generated | scriptdom | 2670993.8 | 479459.4 | 0.18x | 483446.9 | 0.18x | +0.8% | 454400 | 454400 | 26 % |  |
| tsql/columns1000 | generated | scriptdom | 1755725.0 | 191884.4 | 0.11x | 185037.5 | 0.11x | -3.6% | 200464 | 200464 | 2 % |  |
| tsql/conditions1000 | generated | scriptdom | 873512.5 | 339644.5 | 0.39x | 330314.8 | 0.38x | -2.7% | 424424 | 424424 | 13 % |  |
| tsql/rows1000 | generated | scriptdom | 577762.5 | 241613.3 | 0.42x | 241017.2 | 0.42x | -0.2% | 296472 | 296472 | 4 % |  |
| sql/select20.at | generated | hand | 7228.0 | 17482.1 | 2.42x | 17646.4 | 2.44x | +0.9% | 21416 | 21416 | 20 % |  |
| sql/select20.window | generated | hand | 7226.3 | 18049.2 | 2.50x | 18047.0 | 2.50x | 0.0% | 21416 | 21416 | 13 % |  |
| tsql/insert-values.at | generated | scriptdom | 8078.2 | 1196.8 | 0.15x | 1159.8 | 0.14x | -3.1% | 1328 | 1328 | 2 % |  |
| sql/select20.scan | generated | control | 377.4 | 373.0 | 0.99x | 392.1 | 1.04x | +5.1% | 0 | 0 | 3 % |  |
| sql/conditions100.scan | generated | control | 3510.5 | 3504.9 | 1.00x | 3706.6 | 1.06x | +5.8% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 374.5 | 391.9 | 1.05x | 376.5 | 1.01x | -3.9% | 0 | 0 | 42 % |  |
| el/ladder.scan | generated | control | 136.8 | 136.7 | 1.00x | 135.8 | 0.99x | -0.7% | 0 | 0 | 3 % |  |
| el/refused-early.bool | generated | hand | 330.4 | 1079.3 | 3.27x | 557.0 | 1.69x | -48.4% | 1064 | 624 | 4 % |  |
| el/refused-late.bool | generated | hand | 1166.8 | 1755.8 | 1.50x | 906.7 | 0.78x | -48.4% | 1064 | 624 | 15 % |  |
| el/ladder.bool | generated | hand | 1007.3 | 1770.7 | 1.76x | 1691.8 | 1.68x | -4.5% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2673.1 | 12233.2 | 4.58x | 5773.2 | 2.16x | -52.8% | 13552 | 6616 | 9 % |  |
| sql/select20.bool | generated | hand | 7207.3 | 17993.3 | 2.50x | 18026.1 | 2.50x | +0.2% | 21448 | 21392 | 2 % |  |
| el/floor | generated | hand | 324.7 | 806.1 | 2.48x | 798.6 | 2.46x | -0.9% | 1104 | 1104 | 2 % |  |
| el/floor | immediate | hand | 324.7 | 502.4 | 1.55x | 480.5 | 1.48x | -4.4% | 1120 | 1120 | 2 % |  |
| el/ladder | generated | hand | 1005.9 | 1771.5 | 1.76x | 1712.3 | 1.70x | -3.3% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 1005.9 | 1194.8 | 1.19x | 1169.6 | 1.16x | -2.1% | 1784 | 1784 | 2 % |  |
| el/nest7 | generated | hand | 640.7 | 1712.1 | 2.67x | 1681.3 | 2.62x | -1.8% | 1152 | 1152 | 10 % |  |
| el/nest7 | immediate | hand | 640.7 | 1051.7 | 1.64x | 1050.4 | 1.64x | -0.1% | 1120 | 1120 | 10 % |  |
| el/block | generated | hand | 1067.1 | 1843.9 | 1.73x | 1782.8 | 1.67x | -3.3% | 2344 | 2344 | 5 % |  |
| el/block | immediate | hand | 1067.1 | 1189.4 | 1.11x | 1165.7 | 1.09x | -2.0% | 2440 | 2440 | 5 % |  |
| el/try | generated | hand | 3232.7 | 6083.0 | 1.88x | 6108.2 | 1.89x | +0.4% | 5632 | 5632 | 20 % |  |
| el/try | immediate | hand | 3232.7 | 4688.3 | 1.45x | 4702.9 | 1.45x | +0.3% | 5752 | 5752 | 20 % |  |
| el/loop | generated | hand | 2222.2 | 3755.4 | 1.69x | 3693.8 | 1.66x | -1.6% | 4779 | 4776 | 7 % |  |
| el/loop | immediate | hand | 2222.2 | 2545.7 | 1.15x | 2539.3 | 1.14x | -0.3% | 4880 | 4880 | 7 % |  |
| el/terms100 | generated | hand | 11225.7 | 16112.3 | 1.44x | 16167.1 | 1.44x | +0.3% | 17904 | 17904 | 1 % |  |
| el/terms100 | immediate | hand | 11225.7 | 12820.9 | 1.14x | 12692.1 | 1.13x | -1.0% | 18720 | 18720 | 1 % |  |
| el/terms1000 | generated | hand | 108627.5 | 151872.2 | 1.40x | 153476.4 | 1.41x | +1.1% | 169107 | 169128 | 2 % |  |
| el/terms1000 | immediate | hand | 108627.5 | 123150.1 | 1.13x | 122514.3 | 1.13x | -0.5% | 177120 | 177120 | 2 % |  |
| el/overloads | generated | hand | 1911.2 | 2569.2 | 1.34x | 2656.9 | 1.39x | +3.4% | 4248 | 4248 | 5 % |  |
| el/overloads | immediate | hand | 1911.2 | 2048.9 | 1.07x | 2068.0 | 1.08x | +0.9% | 4200 | 4200 | 5 % |  |
| el/string | generated | hand | 283.7 | 947.6 | 3.34x | 945.5 | 3.33x | -0.2% | 1056 | 1056 | 2 % |  |
| el/string | immediate | hand | 283.7 | 670.4 | 2.36x | 655.3 | 2.31x | -2.3% | 1032 | 1032 | 2 % |  |
| el/interpolation | generated | hand | 2101.6 | 4681.4 | 2.23x | 4600.6 | 2.19x | -1.7% | 2368 | 2368 | 88 % |  |
| el/interpolation | immediate | hand | 2101.6 | 4205.9 | 2.00x | 3822.8 | 1.82x | -9.1% | 2424 | 2424 | 88 % |  |
| el/untyped | generated | hand | 29888.9 | 31350.5 | 1.05x | 31562.2 | 1.06x | +0.7% | 16424 | 16424 | 13 % |  |
| el/refused-early | generated | hand | 471.5 | 1226.1 | 2.60x | 1265.2 | 2.68x | +3.2% | 1064 | 1064 | 12 % |  |
| el/refused-early | immediate | hand | 471.5 | 1240.9 | 2.63x | 1226.7 | 2.60x | -1.1% | 1944 | 1944 | 12 % |  |
| el/refused-late | generated | hand | 1457.4 | 1940.4 | 1.33x | 1953.8 | 1.34x | +0.7% | 1064 | 1064 | 15 % |  |
| el/refused-late | immediate | hand | 1457.4 | 3172.5 | 2.18x | 3170.5 | 2.18x | -0.1% | 3928 | 3928 | 15 % |  |
| sql/literal | generated | hand | 66.2 | 143.9 | 2.17x | 153.0 | 2.31x | +6.4% | 160 | 160 | 19 % |  |
| sql/comment | generated | hand | 2911.5 | 5287.2 | 1.82x | 5301.5 | 1.82x | +0.3% | 5136 | 5136 | 15 % |  |
| sql/conditions100 | generated | hand | 73138.7 | 139141.3 | 1.90x | 139264.4 | 1.90x | +0.1% | 161736 | 161736 | 16 % |  |
| sql/conditions1000 | generated | hand | 701475.8 | 1429110.9 | 2.04x | 1442457.8 | 2.06x | +0.9% | 1616136 | 1616136 | 17 % |  |
| tsql/comment | generated | scriptdom | 25973.6 | 1557.1 | 0.06x | 1554.4 | 0.06x | -0.2% | 1192 | 1192 | 12 % |  |
| sql/column | generated | hand | 190.8 | 405.5 | 2.13x | 398.4 | 2.09x | -1.8% | 392 | 392 | 21 % |  |
| sql/arithmetic | generated | hand | 2158.7 | 4672.6 | 2.16x | 4595.4 | 2.13x | -1.7% | 3472 | 3472 | 10 % |  |
| sql/nest8 | generated | hand | 3511.1 | 12755.7 | 3.63x | 12641.8 | 3.60x | -0.9% | 6504 | 6504 | 18 % |  |
| sql/condition | generated | hand | 2547.9 | 5004.2 | 1.96x | 4990.9 | 1.96x | -0.3% | 4928 | 4928 | 40 % |  |
| sql/select1 | generated | hand | 860.2 | 1667.8 | 1.94x | 1709.7 | 1.99x | +2.5% | 1688 | 1688 | 4 % |  |
| sql/select20 | generated | hand | 10336.3 | 20940.2 | 2.03x | 21121.1 | 2.04x | +0.9% | 21448 | 21448 | 34 % |  |
| sql/values | generated | hand | 688.4 | 1955.4 | 2.84x | 1914.3 | 2.78x | -2.1% | 1904 | 1904 | 14 % |  |
| sql/create | generated | hand | 970.9 | 3138.5 | 3.23x | 3023.0 | 3.11x | -3.7% | 1568 | 1568 | 14 % |  |
| sql/refused-late | generated | hand | 3846.0 | 13755.6 | 3.58x | 13467.8 | 3.50x | -2.1% | 13552 | 13552 | 21 % |  |
