# Paired stand, 2026-09-20 03:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1196796.9 | 133162.5 | 0.11x | 145068.8 | 0.12x | +8.9% | 113600 | 105600 | 442 % |  |
| tsql/script100.boolboth | generated | scriptdom | 676689.8 | 122213.3 | 0.18x | 120689.1 | 0.18x | -1.2% | 105600 | 105600 | 3 % |  |
| tsql/script100 | generated | scriptdom | 676482.0 | 122785.2 | 0.18x | 123030.5 | 0.18x | +0.2% | 113600 | 113600 | 21 % |  |
| tsql/script400.bool | generated | scriptdom | 2712528.1 | 486618.8 | 0.18x | 482431.2 | 0.18x | -0.9% | 454400 | 422403 | 10 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2718228.1 | 482234.4 | 0.18x | 478725.0 | 0.18x | -0.7% | 422403 | 422400 | 12 % |  |
| tsql/script400 | generated | scriptdom | 2753934.4 | 492834.4 | 0.18x | 506121.9 | 0.18x | +2.7% | 454400 | 454400 | 8 % |  |
| tsql/columns1000 | generated | scriptdom | 1753459.4 | 195187.5 | 0.11x | 190948.4 | 0.11x | -2.2% | 200464 | 200464 | 4 % |  |
| tsql/conditions1000 | generated | scriptdom | 966080.5 | 338439.8 | 0.35x | 340157.8 | 0.35x | +0.5% | 424424 | 424424 | 9 % |  |
| tsql/rows1000 | generated | scriptdom | 638003.9 | 249421.9 | 0.39x | 246485.9 | 0.39x | -1.2% | 296472 | 296472 | 12 % |  |
| sql/select20.at | generated | hand | 7255.2 | 17646.4 | 2.43x | 17368.3 | 2.39x | -1.6% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 7116.9 | 18067.2 | 2.54x | 17954.7 | 2.52x | -0.6% | 21416 | 21416 | 1 % |  |
| tsql/insert-values.at | generated | scriptdom | 8301.9 | 1190.7 | 0.14x | 1154.2 | 0.14x | -3.1% | 1328 | 1328 | 5 % |  |
| sql/select20.scan | generated | control | 385.2 | 382.3 | 0.99x | 382.1 | 0.99x | -0.1% | 0 | 0 | 1 % |  |
| sql/conditions100.scan | generated | control | 3565.5 | 3559.4 | 1.00x | 3502.2 | 0.98x | -1.6% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 374.8 | 376.5 | 1.00x | 377.3 | 1.01x | +0.2% | 0 | 0 | 38 % |  |
| el/ladder.scan | generated | control | 138.6 | 138.3 | 1.00x | 138.0 | 1.00x | -0.2% | 0 | 0 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2672.7 | 12200.9 | 4.57x | 5657.6 | 2.12x | -53.6% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 7115.7 | 18062.3 | 2.54x | 17702.0 | 2.49x | -2.0% | 21448 | 21392 | 5 % |  |
| sql/literal | generated | hand | 50.4 | 132.6 | 2.63x | 134.8 | 2.67x | +1.7% | 160 | 160 | 1 % |  |
| sql/comment | generated | hand | 2134.7 | 4624.3 | 2.17x | 4546.5 | 2.13x | -1.7% | 5136 | 5161 | 6 % |  |
| sql/conditions100 | generated | hand | 49499.2 | 118333.7 | 2.39x | 116630.8 | 2.36x | -1.4% | 161736 | 161736 | 8 % |  |
| sql/conditions1000 | generated | hand | 507980.1 | 1221853.1 | 2.41x | 1193923.0 | 2.35x | -2.3% | 1616160 | 1616160 | 7 % |  |
| tsql/comment | generated | scriptdom | 20255.3 | 1573.8 | 0.08x | 1583.7 | 0.08x | +0.6% | 1192 | 1192 | 5 % |  |
| sql/column | generated | hand | 145.8 | 349.7 | 2.40x | 357.1 | 2.45x | +2.1% | 392 | 392 | 3 % |  |
| sql/arithmetic | generated | hand | 1629.1 | 4123.1 | 2.53x | 4169.4 | 2.56x | +1.1% | 3472 | 3472 | 8 % |  |
| sql/nest8 | generated | hand | 2733.1 | 11709.1 | 4.28x | 11278.8 | 4.13x | -3.7% | 6504 | 6504 | 5 % |  |
| sql/condition | generated | hand | 1779.0 | 4244.8 | 2.39x | 4217.6 | 2.37x | -0.6% | 4928 | 4928 | 7 % |  |
| sql/select1 | generated | hand | 675.6 | 1479.2 | 2.19x | 1477.7 | 2.19x | -0.1% | 1688 | 1688 | 32 % |  |
| sql/select20 | generated | hand | 7183.9 | 18015.2 | 2.51x | 17923.7 | 2.49x | -0.5% | 21448 | 21448 | 6 % |  |
| sql/values | generated | hand | 453.5 | 1714.8 | 3.78x | 1699.4 | 3.75x | -0.9% | 1904 | 1904 | 12 % |  |
| sql/create | generated | hand | 794.7 | 2670.7 | 3.36x | 2694.9 | 3.39x | +0.9% | 1568 | 1568 | 10 % |  |
| sql/refused-late | generated | hand | 2650.6 | 12238.1 | 4.62x | 11876.6 | 4.48x | -3.0% | 13552 | 13552 | 4 % |  |
