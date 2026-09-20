# Paired stand, 2026-09-20 04:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.9 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 914462.5 | 133362.5 | 0.15x | 132268.8 | 0.14x | -0.8% | 113600 | 105600 | 529 % |  |
| tsql/script100.boolboth | generated | scriptdom | 665034.4 | 129714.8 | 0.20x | 128788.3 | 0.19x | -0.7% | 105600 | 105600 | 14 % |  |
| tsql/script100 | generated | scriptdom | 675766.4 | 131543.0 | 0.19x | 130794.5 | 0.19x | -0.6% | 113600 | 113600 | 43 % |  |
| tsql/script400.bool | generated | scriptdom | 2662100.0 | 512437.5 | 0.19x | 508540.6 | 0.19x | -0.8% | 454400 | 422400 | 11 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2687175.0 | 512103.1 | 0.19x | 512396.9 | 0.19x | +0.1% | 422403 | 422400 | 23 % |  |
| tsql/script400 | generated | scriptdom | 2674431.2 | 510159.4 | 0.19x | 513081.3 | 0.19x | +0.6% | 454400 | 454400 | 11 % |  |
| tsql/columns1000 | generated | scriptdom | 1754631.2 | 187162.5 | 0.11x | 191610.9 | 0.11x | +2.4% | 200464 | 200464 | 14 % |  |
| tsql/conditions1000 | generated | scriptdom | 896871.1 | 341432.0 | 0.38x | 341979.7 | 0.38x | +0.2% | 424424 | 424424 | 12 % |  |
| tsql/rows1000 | generated | scriptdom | 590458.6 | 242871.1 | 0.41x | 245616.4 | 0.42x | +1.1% | 296472 | 296472 | 50 % |  |
| sql/select20.at | generated | hand | 7183.8 | 17718.8 | 2.47x | 18021.2 | 2.51x | +1.7% | 21416 | 21416 | 12 % |  |
| sql/select20.window | generated | hand | 7219.9 | 18180.3 | 2.52x | 18443.9 | 2.55x | +1.5% | 21416 | 21416 | 4 % |  |
| tsql/insert-values.at | generated | scriptdom | 8324.0 | 1211.4 | 0.15x | 1259.8 | 0.15x | +4.0% | 1328 | 1328 | 17 % |  |
| sql/select20.scan | generated | control | 383.5 | 381.4 | 0.99x | 378.7 | 0.99x | -0.7% | 0 | 0 | 14 % |  |
| sql/conditions100.scan | generated | control | 3552.2 | 3517.5 | 0.99x | 3508.5 | 0.99x | -0.3% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 383.5 | 379.2 | 0.99x | 380.0 | 0.99x | +0.2% | 0 | 0 | 18 % |  |
| el/ladder.scan | generated | control | 139.2 | 140.6 | 1.01x | 139.8 | 1.00x | -0.6% | 0 | 0 | 10 % |  |
| el/refused-early.bool | generated | hand | 339.7 | 1113.2 | 3.28x | 570.0 | 1.68x | -48.8% | 1064 | 624 | 13 % |  |
| el/refused-late.bool | generated | hand | 1276.8 | 1793.0 | 1.40x | 949.4 | 0.74x | -47.0% | 1064 | 624 | 16 % |  |
| el/ladder.bool | generated | hand | 1030.1 | 1783.9 | 1.73x | 1777.3 | 1.73x | -0.4% | 1776 | 1720 | 52 % |  |
| sql/refused-late.bool | generated | hand | 2792.9 | 12581.0 | 4.50x | 6105.8 | 2.19x | -51.5% | 13552 | 6616 | 17 % |  |
| sql/select20.bool | generated | hand | 7421.8 | 18605.5 | 2.51x | 18708.3 | 2.52x | +0.6% | 21448 | 21392 | 6 % |  |
| el/floor | generated | hand | 320.6 | 805.0 | 2.51x | 831.5 | 2.59x | +3.3% | 1104 | 1104 | 39 % |  |
| el/floor | immediate | hand | 320.6 | 487.6 | 1.52x | 487.4 | 1.52x | 0.0% | 1120 | 1120 | 39 % |  |
| el/ladder | generated | hand | 1017.2 | 1791.5 | 1.76x | 1765.7 | 1.74x | -1.4% | 1776 | 1776 | 5 % |  |
| el/ladder | immediate | hand | 1017.2 | 1170.2 | 1.15x | 1179.9 | 1.16x | +0.8% | 1784 | 1784 | 5 % |  |
| el/nest7 | generated | hand | 727.0 | 1739.3 | 2.39x | 1755.0 | 2.41x | +0.9% | 1152 | 1152 | 5 % |  |
| el/nest7 | immediate | hand | 727.0 | 1050.4 | 1.44x | 1054.5 | 1.45x | +0.4% | 1120 | 1120 | 5 % |  |
| el/block | generated | hand | 1055.2 | 1848.7 | 1.75x | 1821.9 | 1.73x | -1.4% | 2344 | 2344 | 10 % |  |
| el/block | immediate | hand | 1055.2 | 1163.4 | 1.10x | 1203.3 | 1.14x | +3.4% | 2440 | 2440 | 10 % |  |
| el/try | generated | hand | 3287.2 | 4698.6 | 1.43x | 4839.2 | 1.47x | +3.0% | 4456 | 4456 | 11 % |  |
| el/try | immediate | hand | 3287.2 | 3272.5 | 1.00x | 3402.8 | 1.04x | +4.0% | 4152 | 4152 | 11 % |  |
| el/loop | generated | hand | 2252.0 | 3770.1 | 1.67x | 3784.3 | 1.68x | +0.4% | 4776 | 4801 | 9 % |  |
| el/loop | immediate | hand | 2252.0 | 2551.7 | 1.13x | 2594.2 | 1.15x | +1.7% | 4880 | 4880 | 9 % |  |
| el/terms100 | generated | hand | 11700.3 | 16246.1 | 1.39x | 16494.0 | 1.41x | +1.5% | 17904 | 17904 | 11 % |  |
| el/terms100 | immediate | hand | 11700.3 | 13135.3 | 1.12x | 13018.6 | 1.11x | -0.9% | 18720 | 18720 | 11 % |  |
| el/terms1000 | generated | hand | 112990.2 | 157441.2 | 1.39x | 162187.7 | 1.44x | +3.0% | 169104 | 169104 | 14 % |  |
| el/terms1000 | immediate | hand | 112990.2 | 126682.4 | 1.12x | 125474.4 | 1.11x | -1.0% | 177120 | 177120 | 14 % |  |
| el/overloads | generated | hand | 1914.4 | 2686.9 | 1.40x | 2609.8 | 1.36x | -2.9% | 4248 | 4248 | 11 % |  |
| el/overloads | immediate | hand | 1914.4 | 2112.5 | 1.10x | 2068.6 | 1.08x | -2.1% | 4200 | 4200 | 11 % |  |
| el/string | generated | hand | 283.9 | 964.6 | 3.40x | 987.7 | 3.48x | +2.4% | 1056 | 1056 | 23 % |  |
| el/string | immediate | hand | 283.9 | 674.8 | 2.38x | 669.4 | 2.36x | -0.8% | 1032 | 1032 | 23 % |  |
| el/interpolation | generated | hand | 2585.5 | 4743.1 | 1.83x | 5321.9 | 2.06x | +12.2% | 2368 | 2368 | 81 % |  |
| el/interpolation | immediate | hand | 2585.5 | 5033.8 | 1.95x | 4479.5 | 1.73x | -11.0% | 2424 | 2424 | 81 % |  |
| el/untyped | generated | hand | 19631.4 | 21081.1 | 1.07x | 20509.4 | 1.04x | -2.7% | 16424 | 16424 | 152 % |  |
| el/refused-early | generated | hand | 484.3 | 1247.5 | 2.58x | 1284.5 | 2.65x | +3.0% | 1064 | 1064 | 11 % |  |
| el/refused-early | immediate | hand | 484.3 | 1259.4 | 2.60x | 1280.0 | 2.64x | +1.6% | 1944 | 1944 | 11 % |  |
| el/refused-late | generated | hand | 2168.7 | 2664.8 | 1.23x | 2792.6 | 1.29x | +4.8% | 1064 | 1064 | 68 % |  |
| el/refused-late | immediate | hand | 2168.7 | 4422.9 | 2.04x | 4303.9 | 1.98x | -2.7% | 3928 | 3928 | 68 % |  |
| sql/literal | generated | hand | 125.2 | 241.4 | 1.93x | 257.2 | 2.05x | +6.6% | 160 | 160 | 50 % |  |
| sql/comment | generated | hand | 4478.7 | 8060.9 | 1.80x | 7859.6 | 1.75x | -2.5% | 5136 | 5136 | 52 % |  |
| sql/conditions100 | generated | hand | 109844.2 | 195856.2 | 1.78x | 217772.9 | 1.98x | +11.2% | 161736 | 161736 | 63 % |  |
| sql/conditions1000 | generated | hand | 1284740.6 | 2294826.6 | 1.79x | 2339373.4 | 1.82x | +1.9% | 1616136 | 1616160 | 77 % |  |
| tsql/comment | generated | scriptdom | 42821.8 | 2585.0 | 0.06x | 2550.5 | 0.06x | -1.3% | 1192 | 1192 | 35 % |  |
| sql/column | generated | hand | 296.4 | 582.9 | 1.97x | 579.6 | 1.96x | -0.6% | 392 | 392 | 133 % |  |
| sql/arithmetic | generated | hand | 3366.2 | 6779.4 | 2.01x | 6892.7 | 2.05x | +1.7% | 3472 | 3472 | 62 % |  |
| sql/nest8 | generated | hand | 4938.0 | 16241.6 | 3.29x | 16990.4 | 3.44x | +4.6% | 6504 | 6504 | 52 % |  |
| sql/condition | generated | hand | 4276.1 | 7038.4 | 1.65x | 6795.5 | 1.59x | -3.5% | 4928 | 4928 | 57 % |  |
| sql/select1 | generated | hand | 1469.4 | 2812.1 | 1.91x | 2867.7 | 1.95x | +2.0% | 1688 | 1688 | 32 % |  |
| sql/select20 | generated | hand | 16875.2 | 32332.2 | 1.92x | 32416.3 | 1.92x | +0.3% | 21448 | 21448 | 44 % |  |
| sql/values | generated | hand | 882.1 | 2783.0 | 3.16x | 2799.7 | 3.17x | +0.6% | 1904 | 1904 | 89 % |  |
| sql/create | generated | hand | 1521.7 | 4299.6 | 2.83x | 4066.6 | 2.67x | -5.4% | 1568 | 1568 | 29 % |  |
| sql/refused-late | generated | hand | 5129.2 | 17395.5 | 3.39x | 19357.0 | 3.77x | +11.3% | 13552 | 13552 | 41 % |  |
