# Paired stand, 2026-09-20 03:01

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1071650.0 | 122056.2 | 0.11x | 119878.1 | 0.11x | -1.8% | 113600 | 105600 | 429 % |  |
| tsql/script100.boolboth | generated | scriptdom | 653997.7 | 121849.2 | 0.19x | 120239.1 | 0.18x | -1.3% | 105600 | 105600 | 17 % |  |
| tsql/script100 | generated | scriptdom | 667173.4 | 124052.3 | 0.19x | 124390.6 | 0.19x | +0.3% | 113600 | 113600 | 14 % |  |
| tsql/script400.bool | generated | scriptdom | 2706559.4 | 505921.9 | 0.19x | 493143.8 | 0.18x | -2.5% | 454400 | 422403 | 8 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2656712.5 | 484287.5 | 0.18x | 488218.8 | 0.18x | +0.8% | 422400 | 422403 | 11 % |  |
| tsql/script400 | generated | scriptdom | 2685693.8 | 492100.0 | 0.18x | 493162.5 | 0.18x | +0.2% | 454400 | 454400 | 12 % |  |
| tsql/columns1000 | generated | scriptdom | 1726079.7 | 189201.6 | 0.11x | 187750.0 | 0.11x | -0.8% | 200464 | 200464 | 3 % |  |
| tsql/conditions1000 | generated | scriptdom | 877489.8 | 333264.1 | 0.38x | 338235.2 | 0.39x | +1.5% | 424424 | 424424 | 10 % |  |
| tsql/rows1000 | generated | scriptdom | 587230.5 | 241532.0 | 0.41x | 243961.7 | 0.42x | +1.0% | 296472 | 296472 | 7 % |  |
| sql/select20.at | generated | hand | 7664.6 | 19319.7 | 2.52x | 17999.1 | 2.35x | -6.8% | 21416 | 21416 | 28 % |  |
| sql/select20.window | generated | hand | 7285.3 | 18917.2 | 2.60x | 17995.5 | 2.47x | -4.9% | 21416 | 21416 | 4 % |  |
| tsql/insert-values.at | generated | scriptdom | 8171.4 | 1185.0 | 0.15x | 1175.6 | 0.14x | -0.8% | 1328 | 1328 | 3 % |  |
| sql/select20.scan | generated | control | 390.7 | 383.4 | 0.98x | 379.5 | 0.97x | -1.0% | 0 | 0 | 3 % |  |
| sql/conditions100.scan | generated | control | 3544.5 | 3532.4 | 1.00x | 3529.5 | 1.00x | -0.1% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 374.6 | 381.0 | 1.02x | 378.3 | 1.01x | -0.7% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 140.7 | 136.6 | 0.97x | 137.2 | 0.98x | +0.4% | 0 | 0 | 20 % |  |
| sql/refused-late.bool | generated | hand | 2672.8 | 12412.1 | 4.64x | 5850.2 | 2.19x | -52.9% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 7141.5 | 18633.4 | 2.61x | 17663.5 | 2.47x | -5.2% | 21448 | 21392 | 1 % |  |
| sql/literal | generated | hand | 50.6 | 124.7 | 2.46x | 125.8 | 2.48x | +0.9% | 160 | 160 | 4 % |  |
| sql/comment | generated | hand | 2143.9 | 4745.9 | 2.21x | 4708.2 | 2.20x | -0.8% | 5136 | 5136 | 12 % |  |
| sql/conditions100 | generated | hand | 49507.7 | 122640.9 | 2.48x | 119449.6 | 2.41x | -2.6% | 161761 | 161761 | 8 % |  |
| sql/conditions1000 | generated | hand | 497693.8 | 1216760.5 | 2.44x | 1208639.5 | 2.43x | -0.7% | 1616160 | 1616160 | 4 % |  |
| tsql/comment | generated | scriptdom | 19493.9 | 1563.1 | 0.08x | 1583.8 | 0.08x | +1.3% | 1192 | 1192 | 2 % |  |
| sql/column | generated | hand | 133.2 | 352.0 | 2.64x | 351.8 | 2.64x | -0.1% | 392 | 392 | 2 % |  |
| sql/arithmetic | generated | hand | 1603.7 | 4208.8 | 2.62x | 4172.1 | 2.60x | -0.9% | 3472 | 3472 | 11 % |  |
| sql/nest8 | generated | hand | 2711.4 | 12076.6 | 4.45x | 12151.6 | 4.48x | +0.6% | 6504 | 6504 | 3 % |  |
| sql/condition | generated | hand | 1766.4 | 4343.7 | 2.46x | 4218.9 | 2.39x | -2.9% | 4928 | 4928 | 3 % |  |
| sql/select1 | generated | hand | 678.5 | 1518.5 | 2.24x | 1498.1 | 2.21x | -1.3% | 1688 | 1688 | 10 % |  |
| sql/select20 | generated | hand | 7275.3 | 18784.9 | 2.58x | 17974.1 | 2.47x | -4.3% | 21448 | 21448 | 12 % |  |
| sql/values | generated | hand | 454.3 | 1768.4 | 3.89x | 1730.2 | 3.81x | -2.2% | 1904 | 1904 | 9 % |  |
| sql/create | generated | hand | 785.8 | 2587.4 | 3.29x | 2628.4 | 3.34x | +1.6% | 1568 | 1568 | 3 % |  |
| sql/refused-late | generated | hand | 2675.5 | 12512.3 | 4.68x | 12191.1 | 4.56x | -2.6% | 13552 | 13552 | 5 % |  |
