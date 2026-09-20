# Paired stand, 2026-09-20 03:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1007996.9 | 122515.6 | 0.12x | 122812.5 | 0.12x | +0.2% | 113600 | 105603 | 454 % |  |
| tsql/script100.boolboth | generated | scriptdom | 666903.9 | 122425.0 | 0.18x | 122764.1 | 0.18x | +0.3% | 105600 | 105600 | 5 % |  |
| tsql/script100 | generated | scriptdom | 669568.0 | 123221.9 | 0.18x | 122907.8 | 0.18x | -0.3% | 113600 | 113600 | 18 % |  |
| tsql/script400.bool | generated | scriptdom | 2637134.4 | 497784.4 | 0.19x | 493812.5 | 0.19x | -0.8% | 454400 | 422403 | 3 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2670762.5 | 485634.4 | 0.18x | 491340.6 | 0.18x | +1.2% | 422400 | 422403 | 10 % |  |
| tsql/script400 | generated | scriptdom | 2672012.5 | 494962.5 | 0.19x | 486478.1 | 0.18x | -1.7% | 454400 | 454400 | 8 % |  |
| tsql/columns1000 | generated | scriptdom | 1763354.7 | 189529.7 | 0.11x | 188470.3 | 0.11x | -0.6% | 200464 | 200464 | 7 % |  |
| tsql/conditions1000 | generated | scriptdom | 878364.1 | 336544.5 | 0.38x | 332023.4 | 0.38x | -1.3% | 424424 | 424424 | 8 % |  |
| tsql/rows1000 | generated | scriptdom | 584432.0 | 244604.7 | 0.42x | 244281.2 | 0.42x | -0.1% | 296472 | 296472 | 4 % |  |
| sql/select20.at | generated | hand | 7262.9 | 17634.3 | 2.43x | 17691.3 | 2.44x | +0.3% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 7273.1 | 18036.1 | 2.48x | 17906.4 | 2.46x | -0.7% | 21416 | 21416 | 2 % |  |
| tsql/insert-values.at | generated | scriptdom | 8136.3 | 1169.6 | 0.14x | 1168.8 | 0.14x | -0.1% | 1328 | 1328 | 4 % |  |
| sql/select20.scan | generated | control | 377.7 | 375.6 | 0.99x | 386.1 | 1.02x | +2.8% | 0 | 0 | 14 % |  |
| sql/conditions100.scan | generated | control | 3508.9 | 3502.4 | 1.00x | 3589.4 | 1.02x | +2.5% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 378.9 | 381.1 | 1.01x | 380.6 | 1.00x | -0.2% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 137.8 | 138.3 | 1.00x | 137.4 | 1.00x | -0.6% | 0 | 0 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2672.8 | 12245.7 | 4.58x | 5935.7 | 2.22x | -51.5% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 7250.3 | 18039.9 | 2.49x | 18180.0 | 2.51x | +0.8% | 21448 | 21392 | 6 % |  |
| sql/literal | generated | hand | 54.0 | 129.1 | 2.39x | 132.9 | 2.46x | +3.0% | 160 | 160 | 4 % |  |
| sql/comment | generated | hand | 2164.6 | 4675.8 | 2.16x | 4557.6 | 2.11x | -2.5% | 5136 | 5161 | 2 % |  |
| sql/conditions100 | generated | hand | 48840.4 | 118297.1 | 2.42x | 124050.0 | 2.54x | +4.9% | 161761 | 161761 | 4 % |  |
| sql/conditions1000 | generated | hand | 495816.4 | 1202591.0 | 2.43x | 1243854.7 | 2.51x | +3.4% | 1616160 | 1616160 | 4 % |  |
| tsql/comment | generated | scriptdom | 19517.1 | 1562.9 | 0.08x | 1571.9 | 0.08x | +0.6% | 1192 | 1192 | 5 % |  |
| sql/column | generated | hand | 137.8 | 340.7 | 2.47x | 339.7 | 2.47x | -0.3% | 392 | 392 | 10 % |  |
| sql/arithmetic | generated | hand | 1584.3 | 4139.1 | 2.61x | 4222.5 | 2.67x | +2.0% | 3472 | 3472 | 4 % |  |
| sql/nest8 | generated | hand | 2764.7 | 11455.3 | 4.14x | 11759.9 | 4.25x | +2.7% | 6504 | 6504 | 5 % |  |
| sql/condition | generated | hand | 1771.5 | 4166.0 | 2.35x | 4331.2 | 2.44x | +4.0% | 4928 | 4928 | 4 % |  |
| sql/select1 | generated | hand | 707.3 | 1506.0 | 2.13x | 1475.1 | 2.09x | -2.1% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 7274.5 | 18117.2 | 2.49x | 17886.7 | 2.46x | -1.3% | 21448 | 21448 | 2 % |  |
| sql/values | generated | hand | 494.8 | 1767.4 | 3.57x | 1698.1 | 3.43x | -3.9% | 1904 | 1904 | 9 % |  |
| sql/create | generated | hand | 786.0 | 2595.5 | 3.30x | 2594.1 | 3.30x | -0.1% | 1568 | 1568 | 3 % |  |
| sql/refused-late | generated | hand | 2714.0 | 12240.2 | 4.51x | 12310.5 | 4.54x | +0.6% | 13552 | 13552 | 2 % |  |
