# Paired stand, 2026-09-20 10:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 871671.9 | 123425.0 | 0.14x | 125584.4 | 0.14x | +1.7% | 113600 | 105600 | 524 % |  |
| tsql/script100.boolboth | generated | scriptdom | 654335.2 | 122402.3 | 0.19x | 122071.1 | 0.19x | -0.3% | 105600 | 105603 | 2 % |  |
| tsql/script100 | generated | scriptdom | 654950.8 | 122433.6 | 0.19x | 124530.5 | 0.19x | +1.7% | 113600 | 113600 | 4 % |  |
| tsql/script400.bool | generated | scriptdom | 2615184.4 | 491815.6 | 0.19x | 495709.4 | 0.19x | +0.8% | 454400 | 422400 | 17 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2623409.4 | 491753.1 | 0.19x | 490237.5 | 0.19x | -0.3% | 422403 | 422400 | 4 % |  |
| tsql/script400 | generated | scriptdom | 2620528.1 | 491696.9 | 0.19x | 496912.5 | 0.19x | +1.1% | 454400 | 454400 | 3 % |  |
| tsql/columns1000 | generated | scriptdom | 1738609.4 | 189428.1 | 0.11x | 187360.9 | 0.11x | -1.1% | 200464 | 200464 | 9 % |  |
| tsql/conditions1000 | generated | scriptdom | 893382.0 | 338796.9 | 0.38x | 340096.1 | 0.38x | +0.4% | 424424 | 424424 | 6 % |  |
| tsql/rows1000 | generated | scriptdom | 578775.0 | 241758.6 | 0.42x | 243882.8 | 0.42x | +0.9% | 296472 | 296472 | 10 % |  |
| sql/select20.at | generated | hand | 7192.4 | 17148.3 | 2.38x | 17366.7 | 2.41x | +1.3% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 7198.6 | 17913.7 | 2.49x | 17996.3 | 2.50x | +0.5% | 21416 | 21416 | 7 % |  |
| tsql/insert-values.at | generated | scriptdom | 8078.7 | 1184.2 | 0.15x | 1196.5 | 0.15x | +1.0% | 1328 | 1328 | 12 % |  |
| sql/select20.scan | generated | control | 378.6 | 373.8 | 0.99x | 373.6 | 0.99x | 0.0% | 0 | 0 | 11 % |  |
| sql/conditions100.scan | generated | control | 3467.8 | 3491.8 | 1.01x | 3486.1 | 1.01x | -0.2% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 378.3 | 376.1 | 0.99x | 384.7 | 1.02x | +2.3% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 136.7 | 136.7 | 1.00x | 136.2 | 1.00x | -0.4% | 0 | 0 | 2 % |  |
| el/refused-early.bool | generated | hand | 318.2 | 1067.9 | 3.36x | 571.3 | 1.80x | -46.5% | 1064 | 624 | 30 % |  |
| el/refused-late.bool | generated | hand | 1181.8 | 1715.7 | 1.45x | 937.3 | 0.79x | -45.4% | 1064 | 624 | 2 % |  |
| el/ladder.bool | generated | hand | 994.1 | 1742.6 | 1.75x | 1681.0 | 1.69x | -3.5% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2662.7 | 11944.3 | 4.49x | 5758.4 | 2.16x | -51.8% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 7183.7 | 17901.1 | 2.49x | 17850.0 | 2.48x | -0.3% | 21448 | 21392 | 2 % |  |
| el/floor | generated | hand | 301.6 | 791.3 | 2.62x | 817.2 | 2.71x | +3.3% | 1104 | 1104 | 18 % |  |
| el/floor | immediate | hand | 301.6 | 459.2 | 1.52x | 464.9 | 1.54x | +1.2% | 1120 | 1120 | 18 % |  |
| el/ladder | generated | hand | 988.5 | 1741.1 | 1.76x | 1713.1 | 1.73x | -1.6% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 988.5 | 1141.6 | 1.15x | 1151.4 | 1.16x | +0.9% | 1784 | 1784 | 2 % |  |
| el/nest7 | generated | hand | 690.9 | 1689.0 | 2.44x | 1689.8 | 2.45x | 0.0% | 1152 | 1152 | 10 % |  |
| el/nest7 | immediate | hand | 690.9 | 1034.7 | 1.50x | 1033.1 | 1.50x | -0.2% | 1120 | 1120 | 10 % |  |
| el/block | generated | hand | 1051.7 | 1833.3 | 1.74x | 1777.2 | 1.69x | -3.1% | 2344 | 2344 | 13 % |  |
| el/block | immediate | hand | 1051.7 | 1138.5 | 1.08x | 1134.4 | 1.08x | -0.4% | 2440 | 2440 | 13 % |  |
| el/try | generated | hand | 3198.5 | 6078.7 | 1.90x | 6099.3 | 1.91x | +0.3% | 5632 | 5632 | 3 % |  |
| el/try | immediate | hand | 3198.5 | 4682.5 | 1.46x | 4602.7 | 1.44x | -1.7% | 5752 | 5752 | 3 % |  |
| el/loop | generated | hand | 2209.9 | 3661.5 | 1.66x | 3734.7 | 1.69x | +2.0% | 4776 | 4776 | 1 % |  |
| el/loop | immediate | hand | 2209.9 | 2541.0 | 1.15x | 2502.3 | 1.13x | -1.5% | 4880 | 4880 | 1 % |  |
| el/terms100 | generated | hand | 11125.3 | 16218.5 | 1.46x | 16166.0 | 1.45x | -0.3% | 17904 | 17904 | 5 % |  |
| el/terms100 | immediate | hand | 11125.3 | 12696.2 | 1.14x | 12519.0 | 1.13x | -1.4% | 18720 | 18720 | 5 % |  |
| el/terms1000 | generated | hand | 107656.7 | 154249.8 | 1.43x | 153939.5 | 1.43x | -0.2% | 169107 | 169128 | 1 % |  |
| el/terms1000 | immediate | hand | 107656.7 | 121507.3 | 1.13x | 121450.7 | 1.13x | 0.0% | 177120 | 177120 | 1 % |  |
| el/overloads | generated | hand | 1854.8 | 2557.0 | 1.38x | 2563.6 | 1.38x | +0.3% | 4248 | 4248 | 5 % |  |
| el/overloads | immediate | hand | 1854.8 | 2022.5 | 1.09x | 2030.9 | 1.09x | +0.4% | 4200 | 4200 | 5 % |  |
| el/string | generated | hand | 274.6 | 944.1 | 3.44x | 939.4 | 3.42x | -0.5% | 1056 | 1056 | 11 % |  |
| el/string | immediate | hand | 274.6 | 655.6 | 2.39x | 663.8 | 2.42x | +1.2% | 1032 | 1032 | 11 % |  |
| el/interpolation | generated | hand | 2266.3 | 4642.6 | 2.05x | 4689.4 | 2.07x | +1.0% | 2368 | 2368 | 76 % |  |
| el/interpolation | immediate | hand | 2266.3 | 4231.2 | 1.87x | 4078.2 | 1.80x | -3.6% | 2424 | 2424 | 76 % |  |
| el/untyped | generated | hand | 26719.4 | 28385.7 | 1.06x | 28536.1 | 1.07x | +0.5% | 16424 | 16424 | 10 % |  |
| el/refused-early | generated | hand | 463.5 | 1221.9 | 2.64x | 1297.2 | 2.80x | +6.2% | 1064 | 1064 | 16 % |  |
| el/refused-early | immediate | hand | 463.5 | 1231.1 | 2.66x | 1268.6 | 2.74x | +3.0% | 1944 | 1944 | 16 % |  |
| el/refused-late | generated | hand | 1505.6 | 1917.6 | 1.27x | 1994.9 | 1.32x | +4.0% | 1064 | 1064 | 15 % |  |
| el/refused-late | immediate | hand | 1505.6 | 3129.4 | 2.08x | 3308.7 | 2.20x | +5.7% | 3928 | 3928 | 15 % |  |
| sql/literal | generated | hand | 66.6 | 155.2 | 2.33x | 150.0 | 2.25x | -3.4% | 160 | 160 | 19 % |  |
| sql/comment | generated | hand | 2922.8 | 5291.0 | 1.81x | 5282.5 | 1.81x | -0.2% | 5136 | 5136 | 17 % |  |
| sql/conditions100 | generated | hand | 75097.3 | 134821.1 | 1.80x | 136259.6 | 1.81x | +1.1% | 161736 | 161736 | 21 % |  |
| sql/conditions1000 | generated | hand | 752650.8 | 1392953.1 | 1.85x | 1404393.8 | 1.87x | +0.8% | 1616136 | 1616136 | 16 % |  |
| tsql/comment | generated | scriptdom | 26178.4 | 1570.1 | 0.06x | 1583.7 | 0.06x | +0.9% | 1192 | 1192 | 24 % |  |
| sql/column | generated | hand | 191.8 | 385.0 | 2.01x | 395.4 | 2.06x | +2.7% | 392 | 392 | 15 % |  |
| sql/arithmetic | generated | hand | 2178.1 | 4589.4 | 2.11x | 4421.4 | 2.03x | -3.7% | 3472 | 3472 | 12 % |  |
| sql/nest8 | generated | hand | 3515.6 | 11786.0 | 3.35x | 11785.2 | 3.35x | 0.0% | 6504 | 6504 | 12 % |  |
| sql/condition | generated | hand | 2579.1 | 4881.5 | 1.89x | 4922.5 | 1.91x | +0.8% | 4928 | 4928 | 15 % |  |
| sql/select1 | generated | hand | 886.4 | 1677.2 | 1.89x | 1661.7 | 1.87x | -0.9% | 1688 | 1688 | 18 % |  |
| sql/select20 | generated | hand | 10483.5 | 21052.9 | 2.01x | 21179.7 | 2.02x | +0.6% | 21448 | 21448 | 24 % |  |
| sql/values | generated | hand | 655.2 | 1888.0 | 2.88x | 1917.5 | 2.93x | +1.6% | 1904 | 1904 | 17 % |  |
| sql/create | generated | hand | 980.6 | 2847.4 | 2.90x | 2739.6 | 2.79x | -3.8% | 1568 | 1568 | 12 % |  |
| sql/refused-late | generated | hand | 3918.1 | 13863.8 | 3.54x | 13850.6 | 3.54x | -0.1% | 13552 | 13552 | 26 % |  |
