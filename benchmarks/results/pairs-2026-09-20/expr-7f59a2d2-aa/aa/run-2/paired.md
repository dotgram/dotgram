# Paired stand, 2026-09-20 02:59

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 922162.5 | 120900.0 | 0.13x | 123334.4 | 0.13x | +2.0% | 113600 | 105600 | 498 % |  |
| tsql/script100.boolboth | generated | scriptdom | 656224.2 | 124970.7 | 0.19x | 124432.0 | 0.19x | -0.4% | 105603 | 105600 | 2 % |  |
| tsql/script100 | generated | scriptdom | 663204.7 | 125464.1 | 0.19x | 127127.3 | 0.19x | +1.3% | 113600 | 113600 | 20 % |  |
| tsql/script400.bool | generated | scriptdom | 2683875.0 | 486918.8 | 0.18x | 502840.6 | 0.19x | +3.3% | 454400 | 422403 | 18 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2679496.9 | 493100.0 | 0.18x | 494634.4 | 0.18x | +0.3% | 422403 | 422403 | 11 % |  |
| tsql/script400 | generated | scriptdom | 2643196.9 | 483778.1 | 0.18x | 494859.4 | 0.19x | +2.3% | 454400 | 454400 | 6 % |  |
| tsql/columns1000 | generated | scriptdom | 1781771.9 | 195760.9 | 0.11x | 193868.8 | 0.11x | -1.0% | 200464 | 200464 | 5 % |  |
| tsql/conditions1000 | generated | scriptdom | 897404.7 | 345372.7 | 0.38x | 339814.8 | 0.38x | -1.6% | 424424 | 424424 | 10 % |  |
| tsql/rows1000 | generated | scriptdom | 596387.5 | 248972.7 | 0.42x | 245703.1 | 0.41x | -1.3% | 296472 | 296472 | 12 % |  |
| sql/select20.at | generated | hand | 7187.2 | 17775.9 | 2.47x | 17580.4 | 2.45x | -1.1% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 7175.0 | 18139.1 | 2.53x | 18193.8 | 2.54x | +0.3% | 21416 | 21416 | 18 % |  |
| tsql/insert-values.at | generated | scriptdom | 8222.5 | 1238.5 | 0.15x | 1212.6 | 0.15x | -2.1% | 1328 | 1328 | 9 % |  |
| sql/select20.scan | generated | control | 393.5 | 378.8 | 0.96x | 382.8 | 0.97x | +1.1% | 0 | 0 | 9 % |  |
| sql/conditions100.scan | generated | control | 3572.6 | 3563.0 | 1.00x | 3509.8 | 0.98x | -1.5% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 397.6 | 382.8 | 0.96x | 386.3 | 0.97x | +0.9% | 0 | 0 | 17 % |  |
| el/ladder.scan | generated | control | 140.1 | 139.0 | 0.99x | 138.5 | 0.99x | -0.3% | 0 | 0 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2696.5 | 12132.2 | 4.50x | 5807.0 | 2.15x | -52.1% | 13552 | 6616 | 13 % |  |
| sql/select20.bool | generated | hand | 7252.8 | 18124.1 | 2.50x | 17961.1 | 2.48x | -0.9% | 21448 | 21392 | 4 % |  |
| sql/literal | generated | hand | 54.1 | 121.7 | 2.25x | 120.4 | 2.22x | -1.1% | 160 | 160 | 4 % |  |
| sql/comment | generated | hand | 2127.6 | 4607.9 | 2.17x | 4612.7 | 2.17x | +0.1% | 5136 | 5136 | 9 % |  |
| sql/conditions100 | generated | hand | 49489.7 | 119607.7 | 2.42x | 116504.8 | 2.35x | -2.6% | 161736 | 161736 | 2 % |  |
| sql/conditions1000 | generated | hand | 497654.3 | 1225415.6 | 2.46x | 1177290.6 | 2.37x | -3.9% | 1616160 | 1616160 | 3 % |  |
| tsql/comment | generated | scriptdom | 19460.4 | 1603.1 | 0.08x | 1576.0 | 0.08x | -1.7% | 1192 | 1192 | 6 % |  |
| sql/column | generated | hand | 139.8 | 350.2 | 2.50x | 342.3 | 2.45x | -2.3% | 392 | 392 | 7 % |  |
| sql/arithmetic | generated | hand | 1604.3 | 4213.4 | 2.63x | 4131.4 | 2.58x | -1.9% | 3472 | 3472 | 5 % |  |
| sql/nest8 | generated | hand | 2779.2 | 11920.2 | 4.29x | 10947.8 | 3.94x | -8.2% | 6504 | 6504 | 6 % |  |
| sql/condition | generated | hand | 1775.8 | 4261.9 | 2.40x | 4183.1 | 2.36x | -1.8% | 4928 | 4928 | 35 % |  |
| sql/select1 | generated | hand | 666.5 | 1484.8 | 2.23x | 1507.1 | 2.26x | +1.5% | 1688 | 1688 | 3 % |  |
| sql/select20 | generated | hand | 7314.0 | 18342.0 | 2.51x | 18739.8 | 2.56x | +2.2% | 21448 | 21448 | 6 % |  |
| sql/values | generated | hand | 459.2 | 1703.1 | 3.71x | 1630.2 | 3.55x | -4.3% | 1904 | 1904 | 18 % |  |
| sql/create | generated | hand | 795.3 | 2635.9 | 3.31x | 2665.7 | 3.35x | +1.1% | 1568 | 1568 | 9 % |  |
| sql/refused-late | generated | hand | 2690.7 | 12170.8 | 4.52x | 12136.4 | 4.51x | -0.3% | 13552 | 13552 | 9 % |  |
