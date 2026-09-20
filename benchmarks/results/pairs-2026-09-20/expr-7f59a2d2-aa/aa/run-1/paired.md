# Paired stand, 2026-09-20 02:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1284318.8 | 162571.9 | 0.13x | 160481.2 | 0.12x | -1.3% | 113600 | 105600 | 356 % |  |
| tsql/script100.boolboth | generated | scriptdom | 657568.0 | 121242.2 | 0.18x | 122362.5 | 0.19x | +0.9% | 105600 | 105600 | 2 % |  |
| tsql/script100 | generated | scriptdom | 665153.9 | 127074.2 | 0.19x | 126218.0 | 0.19x | -0.7% | 113600 | 113600 | 6 % |  |
| tsql/script400.bool | generated | scriptdom | 2669234.4 | 504528.1 | 0.19x | 486565.6 | 0.18x | -3.6% | 454400 | 422403 | 8 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2657737.5 | 493778.1 | 0.19x | 484975.0 | 0.18x | -1.8% | 422400 | 422403 | 10 % |  |
| tsql/script400 | generated | scriptdom | 2685500.0 | 507334.4 | 0.19x | 509631.2 | 0.19x | +0.5% | 454400 | 454400 | 10 % |  |
| tsql/columns1000 | generated | scriptdom | 1773245.3 | 188465.6 | 0.11x | 190595.3 | 0.11x | +1.1% | 200464 | 200464 | 3 % |  |
| tsql/conditions1000 | generated | scriptdom | 870553.9 | 336307.8 | 0.39x | 342128.9 | 0.39x | +1.7% | 424424 | 424424 | 6 % |  |
| tsql/rows1000 | generated | scriptdom | 587860.9 | 248907.0 | 0.42x | 244633.6 | 0.42x | -1.7% | 296472 | 296472 | 7 % |  |
| sql/select20.at | generated | hand | 7320.5 | 17986.2 | 2.46x | 17805.9 | 2.43x | -1.0% | 21416 | 21416 | 16 % |  |
| sql/select20.window | generated | hand | 7340.2 | 18485.8 | 2.52x | 18444.1 | 2.51x | -0.2% | 21416 | 21416 | 10 % |  |
| tsql/insert-values.at | generated | scriptdom | 8584.4 | 1200.9 | 0.14x | 1201.1 | 0.14x | 0.0% | 1328 | 1328 | 19 % |  |
| sql/select20.scan | generated | control | 399.7 | 395.9 | 0.99x | 393.4 | 0.98x | -0.6% | 0 | 0 | 31 % |  |
| sql/conditions100.scan | generated | control | 3552.2 | 3554.8 | 1.00x | 3548.6 | 1.00x | -0.2% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 378.4 | 384.6 | 1.02x | 383.6 | 1.01x | -0.3% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 139.0 | 140.3 | 1.01x | 138.8 | 1.00x | -1.1% | 0 | 0 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2878.0 | 12793.6 | 4.45x | 6084.8 | 2.11x | -52.4% | 13552 | 6616 | 21 % |  |
| sql/select20.bool | generated | hand | 7461.4 | 18992.8 | 2.55x | 19075.8 | 2.56x | +0.4% | 21448 | 21392 | 5 % |  |
| sql/literal | generated | hand | 51.9 | 126.4 | 2.43x | 130.2 | 2.51x | +3.0% | 160 | 160 | 6 % |  |
| sql/comment | generated | hand | 2141.4 | 4614.0 | 2.15x | 4544.1 | 2.12x | -1.5% | 5136 | 5136 | 3 % |  |
| sql/conditions100 | generated | hand | 48321.9 | 119288.5 | 2.47x | 117215.7 | 2.43x | -1.7% | 161761 | 161761 | 4 % |  |
| sql/conditions1000 | generated | hand | 500466.8 | 1212791.4 | 2.42x | 1205654.3 | 2.41x | -0.6% | 1616160 | 1616160 | 5 % |  |
| tsql/comment | generated | scriptdom | 19457.2 | 1569.9 | 0.08x | 1589.4 | 0.08x | +1.2% | 1192 | 1192 | 2 % |  |
| sql/column | generated | hand | 133.7 | 341.9 | 2.56x | 332.2 | 2.48x | -2.8% | 392 | 392 | 2 % |  |
| sql/arithmetic | generated | hand | 1557.8 | 4229.9 | 2.72x | 4117.7 | 2.64x | -2.7% | 3472 | 3472 | 8 % |  |
| sql/nest8 | generated | hand | 2686.8 | 11347.9 | 4.22x | 11203.2 | 4.17x | -1.3% | 6504 | 6504 | 3 % |  |
| sql/condition | generated | hand | 1808.4 | 4376.5 | 2.42x | 4303.8 | 2.38x | -1.7% | 4928 | 4928 | 18 % |  |
| sql/select1 | generated | hand | 684.6 | 1562.0 | 2.28x | 1520.7 | 2.22x | -2.6% | 1688 | 1688 | 9 % |  |
| sql/select20 | generated | hand | 7273.5 | 18681.6 | 2.57x | 18212.6 | 2.50x | -2.5% | 21448 | 21448 | 5 % |  |
| sql/values | generated | hand | 448.1 | 1748.9 | 3.90x | 1665.9 | 3.72x | -4.7% | 1904 | 1904 | 5 % |  |
| sql/create | generated | hand | 781.3 | 2646.5 | 3.39x | 2629.6 | 3.37x | -0.6% | 1568 | 1568 | 6 % |  |
| sql/refused-late | generated | hand | 2681.3 | 12500.8 | 4.66x | 12267.6 | 4.58x | -1.9% | 13552 | 13552 | 11 % |  |
