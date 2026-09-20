# Paired stand, 2026-09-20 04:08

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 754143.8 | 123978.1 | 0.16x | 122331.2 | 0.16x | -1.3% | 113600 | 105600 | 617 % |  |
| tsql/script100.boolboth | generated | scriptdom | 662789.8 | 120510.2 | 0.18x | 124642.2 | 0.19x | +3.4% | 105600 | 105600 | 6 % |  |
| tsql/script100 | generated | scriptdom | 655821.9 | 122226.6 | 0.19x | 123360.2 | 0.19x | +0.9% | 113600 | 113600 | 16 % |  |
| tsql/script400.bool | generated | scriptdom | 2666400.0 | 505390.6 | 0.19x | 493362.5 | 0.19x | -2.4% | 454400 | 422403 | 11 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2617415.6 | 469356.2 | 0.18x | 489962.5 | 0.19x | +4.4% | 422400 | 422403 | 3 % |  |
| tsql/script400 | generated | scriptdom | 2621484.4 | 497003.1 | 0.19x | 494103.1 | 0.19x | -0.6% | 454400 | 454400 | 13 % |  |
| tsql/columns1000 | generated | scriptdom | 1720889.1 | 189839.1 | 0.11x | 190342.2 | 0.11x | +0.3% | 200464 | 200464 | 3 % |  |
| tsql/conditions1000 | generated | scriptdom | 884182.8 | 334874.2 | 0.38x | 338094.5 | 0.38x | +1.0% | 424424 | 424424 | 4 % |  |
| tsql/rows1000 | generated | scriptdom | 584132.0 | 242986.7 | 0.42x | 244222.7 | 0.42x | +0.5% | 296472 | 296472 | 5 % |  |
| sql/select20.at | generated | hand | 6979.4 | 17533.2 | 2.51x | 17916.3 | 2.57x | +2.2% | 21416 | 21416 | 33 % |  |
| sql/select20.window | generated | hand | 6968.2 | 18700.5 | 2.68x | 18454.4 | 2.65x | -1.3% | 21416 | 21416 | 38 % |  |
| tsql/insert-values.at | generated | scriptdom | 8107.2 | 1159.1 | 0.14x | 1218.1 | 0.15x | +5.1% | 1328 | 1328 | 9 % |  |
| sql/select20.scan | generated | control | 383.0 | 378.8 | 0.99x | 383.3 | 1.00x | +1.2% | 0 | 0 | 13 % |  |
| sql/conditions100.scan | generated | control | 3514.6 | 3537.5 | 1.01x | 3591.6 | 1.02x | +1.5% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 381.4 | 381.2 | 1.00x | 379.2 | 0.99x | -0.5% | 0 | 0 | 17 % |  |
| el/ladder.scan | generated | control | 138.7 | 138.3 | 1.00x | 138.1 | 1.00x | -0.2% | 0 | 0 | 9 % |  |
| el/refused-early.bool | generated | hand | 342.9 | 1106.1 | 3.23x | 564.2 | 1.65x | -49.0% | 1064 | 624 | 7 % |  |
| el/refused-late.bool | generated | hand | 1182.9 | 1766.2 | 1.49x | 911.9 | 0.77x | -48.4% | 1064 | 624 | 13 % |  |
| el/ladder.bool | generated | hand | 1054.7 | 1837.3 | 1.74x | 1800.2 | 1.71x | -2.0% | 1776 | 1720 | 9 % |  |
| sql/refused-late.bool | generated | hand | 2736.0 | 12459.7 | 4.55x | 6077.4 | 2.22x | -51.2% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 7030.9 | 18225.3 | 2.59x | 18686.5 | 2.66x | +2.5% | 21448 | 21392 | 8 % |  |
| el/floor | generated | hand | 347.2 | 849.4 | 2.45x | 903.6 | 2.60x | +6.4% | 1104 | 1104 | 16 % |  |
| el/floor | immediate | hand | 347.2 | 511.3 | 1.47x | 479.8 | 1.38x | -6.2% | 1120 | 1120 | 16 % |  |
| el/ladder | generated | hand | 1040.5 | 1834.1 | 1.76x | 1879.6 | 1.81x | +2.5% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 1040.5 | 1199.3 | 1.15x | 1199.0 | 1.15x | 0.0% | 1784 | 1784 | 4 % |  |
| el/nest7 | generated | hand | 672.9 | 1727.0 | 2.57x | 1834.3 | 2.73x | +6.2% | 1152 | 1152 | 5 % |  |
| el/nest7 | immediate | hand | 672.9 | 1056.1 | 1.57x | 1071.3 | 1.59x | +1.4% | 1120 | 1120 | 5 % |  |
| el/block | generated | hand | 1071.8 | 1853.7 | 1.73x | 1930.2 | 1.80x | +4.1% | 2344 | 2344 | 3 % |  |
| el/block | immediate | hand | 1071.8 | 1195.8 | 1.12x | 1224.1 | 1.14x | +2.4% | 2440 | 2440 | 3 % |  |
| el/try | generated | hand | 3314.7 | 4747.9 | 1.43x | 4806.7 | 1.45x | +1.2% | 4456 | 4456 | 8 % |  |
| el/try | immediate | hand | 3314.7 | 3280.8 | 0.99x | 3337.0 | 1.01x | +1.7% | 4152 | 4152 | 8 % |  |
| el/loop | generated | hand | 2236.5 | 3797.6 | 1.70x | 3842.1 | 1.72x | +1.2% | 4776 | 4776 | 2 % |  |
| el/loop | immediate | hand | 2236.5 | 2591.0 | 1.16x | 2597.5 | 1.16x | +0.3% | 4880 | 4880 | 2 % |  |
| el/terms100 | generated | hand | 11222.6 | 16587.2 | 1.48x | 16289.0 | 1.45x | -1.8% | 17904 | 17904 | 2 % |  |
| el/terms100 | immediate | hand | 11222.6 | 12872.9 | 1.15x | 12979.2 | 1.16x | +0.8% | 18720 | 18720 | 2 % |  |
| el/terms1000 | generated | hand | 111181.3 | 156647.4 | 1.41x | 155915.6 | 1.40x | -0.5% | 169107 | 169104 | 26 % |  |
| el/terms1000 | immediate | hand | 111181.3 | 125106.9 | 1.13x | 125480.2 | 1.13x | +0.3% | 177120 | 177120 | 26 % |  |
| el/overloads | generated | hand | 1930.9 | 2642.0 | 1.37x | 2768.3 | 1.43x | +4.8% | 4248 | 4248 | 16 % |  |
| el/overloads | immediate | hand | 1930.9 | 2066.7 | 1.07x | 2060.4 | 1.07x | -0.3% | 4200 | 4200 | 16 % |  |
| el/string | generated | hand | 277.2 | 991.7 | 3.58x | 1044.5 | 3.77x | +5.3% | 1056 | 1056 | 10 % |  |
| el/string | immediate | hand | 277.2 | 661.1 | 2.38x | 662.3 | 2.39x | +0.2% | 1032 | 1032 | 10 % |  |
| el/interpolation | generated | hand | 2074.8 | 5131.4 | 2.47x | 5522.4 | 2.66x | +7.6% | 2368 | 2368 | 51 % |  |
| el/interpolation | immediate | hand | 2074.8 | 4518.0 | 2.18x | 3991.1 | 1.92x | -11.7% | 2424 | 2424 | 51 % |  |
| el/untyped | generated | hand | 26786.8 | 28782.7 | 1.07x | 29067.5 | 1.09x | +1.0% | 16424 | 16424 | 2 % |  |
| el/refused-early | generated | hand | 524.5 | 1272.9 | 2.43x | 1386.9 | 2.64x | +9.0% | 1064 | 1064 | 17 % |  |
| el/refused-early | immediate | hand | 524.5 | 1291.6 | 2.46x | 1298.4 | 2.48x | +0.5% | 1944 | 1944 | 17 % |  |
| el/refused-late | generated | hand | 1455.3 | 1986.8 | 1.37x | 2096.8 | 1.44x | +5.5% | 1064 | 1064 | 20 % |  |
| el/refused-late | immediate | hand | 1455.3 | 3260.8 | 2.24x | 3414.9 | 2.35x | +4.7% | 3928 | 3928 | 20 % |  |
| sql/literal | generated | hand | 69.7 | 150.3 | 2.16x | 143.6 | 2.06x | -4.5% | 160 | 160 | 24 % |  |
| sql/comment | generated | hand | 2995.1 | 5430.5 | 1.81x | 5599.9 | 1.87x | +3.1% | 5136 | 5136 | 24 % |  |
| sql/conditions100 | generated | hand | 80165.1 | 149397.9 | 1.86x | 150364.4 | 1.88x | +0.6% | 161736 | 161736 | 41 % |  |
| sql/conditions1000 | generated | hand | 753621.9 | 1500407.0 | 1.99x | 1490347.7 | 1.98x | -0.7% | 1616136 | 1616136 | 23 % |  |
| tsql/comment | generated | scriptdom | 27593.1 | 1576.9 | 0.06x | 1589.6 | 0.06x | +0.8% | 1192 | 1192 | 16 % |  |
| sql/column | generated | hand | 213.0 | 402.3 | 1.89x | 408.5 | 1.92x | +1.5% | 392 | 392 | 16 % |  |
| sql/arithmetic | generated | hand | 2315.3 | 4795.2 | 2.07x | 4756.5 | 2.05x | -0.8% | 3472 | 3472 | 22 % |  |
| sql/nest8 | generated | hand | 3667.9 | 12059.7 | 3.29x | 12418.9 | 3.39x | +3.0% | 6504 | 6504 | 17 % |  |
| sql/condition | generated | hand | 2725.9 | 4904.3 | 1.80x | 4961.9 | 1.82x | +1.2% | 4928 | 4928 | 12 % |  |
| sql/select1 | generated | hand | 909.0 | 1709.7 | 1.88x | 1825.0 | 2.01x | +6.7% | 1688 | 1688 | 16 % |  |
| sql/select20 | generated | hand | 10761.4 | 21676.3 | 2.01x | 22261.4 | 2.07x | +2.7% | 21448 | 21448 | 12 % |  |
| sql/values | generated | hand | 708.3 | 2005.1 | 2.83x | 1995.7 | 2.82x | -0.5% | 1904 | 1904 | 19 % |  |
| sql/create | generated | hand | 1110.4 | 3128.7 | 2.82x | 2962.3 | 2.67x | -5.3% | 1568 | 1568 | 18 % |  |
| sql/refused-late | generated | hand | 3317.8 | 13360.3 | 4.03x | 13439.5 | 4.05x | +0.6% | 13552 | 13552 | 73 % |  |
