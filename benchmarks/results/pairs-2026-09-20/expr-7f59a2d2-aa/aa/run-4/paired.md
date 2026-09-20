# Paired stand, 2026-09-20 03:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1125056.2 | 140843.8 | 0.13x | 133943.8 | 0.12x | -4.9% | 113600 | 105600 | 331 % |  |
| tsql/script100.boolboth | generated | scriptdom | 654875.8 | 121153.9 | 0.19x | 122093.0 | 0.19x | +0.8% | 105600 | 105600 | 11 % |  |
| tsql/script100 | generated | scriptdom | 654698.4 | 122442.2 | 0.19x | 122846.1 | 0.19x | +0.3% | 113600 | 113600 | 2 % |  |
| tsql/script400.bool | generated | scriptdom | 2607675.0 | 492053.1 | 0.19x | 487721.9 | 0.19x | -0.9% | 454400 | 422400 | 12 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2642962.5 | 479275.0 | 0.18x | 494137.5 | 0.19x | +3.1% | 422400 | 422403 | 13 % |  |
| tsql/script400 | generated | scriptdom | 2611475.0 | 494046.9 | 0.19x | 493062.5 | 0.19x | -0.2% | 454400 | 454400 | 16 % |  |
| tsql/columns1000 | generated | scriptdom | 1734645.3 | 190157.8 | 0.11x | 195376.6 | 0.11x | +2.7% | 200464 | 200464 | 3 % |  |
| tsql/conditions1000 | generated | scriptdom | 911339.1 | 337954.7 | 0.37x | 344035.2 | 0.38x | +1.8% | 424424 | 424424 | 12 % |  |
| tsql/rows1000 | generated | scriptdom | 587328.9 | 243254.7 | 0.41x | 245236.7 | 0.42x | +0.8% | 296472 | 296472 | 2 % |  |
| sql/select20.at | generated | hand | 7208.0 | 17521.5 | 2.43x | 17662.7 | 2.45x | +0.8% | 21416 | 21416 | 20 % |  |
| sql/select20.window | generated | hand | 7164.3 | 18118.3 | 2.53x | 18069.3 | 2.52x | -0.3% | 21416 | 21416 | 6 % |  |
| tsql/insert-values.at | generated | scriptdom | 8247.9 | 1176.0 | 0.14x | 1201.5 | 0.15x | +2.2% | 1328 | 1328 | 10 % |  |
| sql/select20.scan | generated | control | 386.7 | 389.0 | 1.01x | 380.4 | 0.98x | -2.2% | 0 | 0 | 11 % |  |
| sql/conditions100.scan | generated | control | 3581.4 | 3618.9 | 1.01x | 3542.7 | 0.99x | -2.1% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 374.2 | 378.7 | 1.01x | 376.6 | 1.01x | -0.6% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 138.5 | 139.6 | 1.01x | 137.8 | 0.99x | -1.3% | 0 | 0 | 44 % |  |
| sql/refused-late.bool | generated | hand | 2704.8 | 12406.3 | 4.59x | 5862.2 | 2.17x | -52.7% | 13552 | 6616 | 45 % |  |
| sql/select20.bool | generated | hand | 7398.8 | 18609.2 | 2.52x | 18995.3 | 2.57x | +2.1% | 21448 | 21392 | 9 % |  |
| sql/literal | generated | hand | 50.6 | 127.5 | 2.52x | 130.7 | 2.59x | +2.6% | 160 | 160 | 2 % |  |
| sql/comment | generated | hand | 2109.6 | 4602.0 | 2.18x | 4597.6 | 2.18x | -0.1% | 5136 | 5136 | 4 % |  |
| sql/conditions100 | generated | hand | 48586.0 | 118667.5 | 2.44x | 120472.0 | 2.48x | +1.5% | 161736 | 161736 | 3 % |  |
| sql/conditions1000 | generated | hand | 490244.1 | 1202621.5 | 2.45x | 1212937.5 | 2.47x | +0.9% | 1616160 | 1616160 | 4 % |  |
| tsql/comment | generated | scriptdom | 19705.1 | 1548.7 | 0.08x | 1538.1 | 0.08x | -0.7% | 1192 | 1192 | 4 % |  |
| sql/column | generated | hand | 132.5 | 338.7 | 2.56x | 342.9 | 2.59x | +1.2% | 392 | 392 | 1 % |  |
| sql/arithmetic | generated | hand | 1671.4 | 4172.3 | 2.50x | 4124.1 | 2.47x | -1.2% | 3472 | 3472 | 3 % |  |
| sql/nest8 | generated | hand | 2705.5 | 12186.9 | 4.50x | 11886.8 | 4.39x | -2.5% | 6504 | 6504 | 4 % |  |
| sql/condition | generated | hand | 1761.9 | 4237.1 | 2.40x | 4386.9 | 2.49x | +3.5% | 4928 | 4928 | 2 % |  |
| sql/select1 | generated | hand | 667.1 | 1487.0 | 2.23x | 1536.1 | 2.30x | +3.3% | 1688 | 1688 | 4 % |  |
| sql/select20 | generated | hand | 7160.9 | 17982.9 | 2.51x | 18063.0 | 2.52x | +0.4% | 21448 | 21448 | 6 % |  |
| sql/values | generated | hand | 452.1 | 1748.6 | 3.87x | 1718.7 | 3.80x | -1.7% | 1904 | 1904 | 18 % |  |
| sql/create | generated | hand | 780.8 | 2629.7 | 3.37x | 2648.4 | 3.39x | +0.7% | 1568 | 1568 | 9 % |  |
| sql/refused-late | generated | hand | 2736.3 | 12498.0 | 4.57x | 12537.6 | 4.58x | +0.3% | 13552 | 13552 | 12 % |  |
