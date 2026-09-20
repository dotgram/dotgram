# Paired stand, 2026-09-20 03:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1460293.8 | 175603.1 | 0.12x | 160909.4 | 0.11x | -8.4% | 113600 | 105600 | 316 % |  |
| tsql/script100.boolboth | generated | scriptdom | 654690.6 | 119292.2 | 0.18x | 124283.6 | 0.19x | +4.2% | 105600 | 105600 | 9 % |  |
| tsql/script100 | generated | scriptdom | 662533.6 | 121349.2 | 0.18x | 125249.2 | 0.19x | +3.2% | 113600 | 113600 | 8 % |  |
| tsql/script400.bool | generated | scriptdom | 2672637.5 | 538956.2 | 0.20x | 495503.1 | 0.19x | -8.1% | 454400 | 422403 | 9 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2673184.4 | 489925.0 | 0.18x | 505028.1 | 0.19x | +3.1% | 422403 | 422400 | 29 % |  |
| tsql/script400 | generated | scriptdom | 2638284.4 | 483384.4 | 0.18x | 500921.9 | 0.19x | +3.6% | 454400 | 454400 | 6 % |  |
| tsql/columns1000 | generated | scriptdom | 1791023.4 | 191592.2 | 0.11x | 188065.6 | 0.11x | -1.8% | 200464 | 200464 | 12 % |  |
| tsql/conditions1000 | generated | scriptdom | 885499.2 | 333943.8 | 0.38x | 340712.5 | 0.38x | +2.0% | 424424 | 424424 | 6 % |  |
| tsql/rows1000 | generated | scriptdom | 589949.6 | 246647.3 | 0.42x | 242235.9 | 0.41x | -1.8% | 296472 | 296472 | 8 % |  |
| sql/select20.at | generated | hand | 7364.9 | 18797.0 | 2.55x | 17505.2 | 2.38x | -6.9% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 7277.7 | 19238.9 | 2.64x | 17938.8 | 2.46x | -6.8% | 21416 | 21416 | 19 % |  |
| tsql/insert-values.at | generated | scriptdom | 8333.8 | 1189.0 | 0.14x | 1200.9 | 0.14x | +1.0% | 1328 | 1328 | 18 % |  |
| sql/select20.scan | generated | control | 390.7 | 377.4 | 0.97x | 381.8 | 0.98x | +1.2% | 0 | 0 | 11 % |  |
| sql/conditions100.scan | generated | control | 3544.0 | 3539.1 | 1.00x | 3529.5 | 1.00x | -0.3% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 384.6 | 383.7 | 1.00x | 379.1 | 0.99x | -1.2% | 0 | 0 | 41 % |  |
| el/ladder.scan | generated | control | 141.3 | 141.4 | 1.00x | 140.0 | 0.99x | -1.0% | 0 | 0 | 18 % |  |
| sql/refused-late.bool | generated | hand | 2669.4 | 12544.8 | 4.70x | 5832.4 | 2.18x | -53.5% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 7344.3 | 19599.4 | 2.67x | 18411.3 | 2.51x | -6.1% | 21448 | 21392 | 19 % |  |
| sql/literal | generated | hand | 50.8 | 131.1 | 2.58x | 130.4 | 2.57x | -0.5% | 160 | 160 | 7 % |  |
| sql/comment | generated | hand | 2120.3 | 4708.4 | 2.22x | 4508.6 | 2.13x | -4.2% | 5136 | 5161 | 42 % |  |
| sql/conditions100 | generated | hand | 48566.6 | 119995.1 | 2.47x | 116412.5 | 2.40x | -3.0% | 161736 | 161736 | 2 % |  |
| sql/conditions1000 | generated | hand | 493990.6 | 1207825.4 | 2.45x | 1182524.2 | 2.39x | -2.1% | 1616160 | 1616160 | 4 % |  |
| tsql/comment | generated | scriptdom | 19525.0 | 1550.4 | 0.08x | 1592.1 | 0.08x | +2.7% | 1192 | 1192 | 15 % |  |
| sql/column | generated | hand | 131.7 | 334.8 | 2.54x | 347.4 | 2.64x | +3.8% | 392 | 392 | 3 % |  |
| sql/arithmetic | generated | hand | 1627.7 | 4133.9 | 2.54x | 4124.8 | 2.53x | -0.2% | 3472 | 3472 | 2 % |  |
| sql/nest8 | generated | hand | 2688.9 | 10947.6 | 4.07x | 11052.0 | 4.11x | +1.0% | 6504 | 6504 | 8 % |  |
| sql/condition | generated | hand | 1745.0 | 4258.5 | 2.44x | 4183.1 | 2.40x | -1.8% | 4928 | 4928 | 4 % |  |
| sql/select1 | generated | hand | 671.8 | 1511.5 | 2.25x | 1475.7 | 2.20x | -2.4% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 7175.4 | 18950.3 | 2.64x | 17711.4 | 2.47x | -6.5% | 21448 | 21448 | 2 % |  |
| sql/values | generated | hand | 454.3 | 1629.3 | 3.59x | 1664.5 | 3.66x | +2.2% | 1904 | 1904 | 2 % |  |
| sql/create | generated | hand | 764.0 | 2602.2 | 3.41x | 2649.7 | 3.47x | +1.8% | 1568 | 1568 | 33 % |  |
| sql/refused-late | generated | hand | 2653.8 | 12449.3 | 4.69x | 12051.9 | 4.54x | -3.2% | 13552 | 13552 | 3 % |  |
