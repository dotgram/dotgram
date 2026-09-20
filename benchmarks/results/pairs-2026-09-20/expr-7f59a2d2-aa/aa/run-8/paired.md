# Paired stand, 2026-09-20 03:11

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 857571.9 | 120659.4 | 0.14x | 123146.9 | 0.14x | +2.1% | 113600 | 105600 | 539 % |  |
| tsql/script100.boolboth | generated | scriptdom | 666315.6 | 123973.4 | 0.19x | 127217.2 | 0.19x | +2.6% | 105603 | 105600 | 12 % |  |
| tsql/script100 | generated | scriptdom | 652010.2 | 124943.0 | 0.19x | 122185.9 | 0.19x | -2.2% | 113600 | 113600 | 6 % |  |
| tsql/script400.bool | generated | scriptdom | 2650446.9 | 497725.0 | 0.19x | 490028.1 | 0.18x | -1.5% | 454400 | 422403 | 8 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2654643.8 | 495653.1 | 0.19x | 492843.8 | 0.19x | -0.6% | 422400 | 422403 | 9 % |  |
| tsql/script400 | generated | scriptdom | 2620021.9 | 494496.9 | 0.19x | 495946.9 | 0.19x | +0.3% | 454400 | 454400 | 12 % |  |
| tsql/columns1000 | generated | scriptdom | 1730659.4 | 204218.8 | 0.12x | 189335.9 | 0.11x | -7.3% | 200464 | 200464 | 12 % |  |
| tsql/conditions1000 | generated | scriptdom | 884253.9 | 334425.8 | 0.38x | 341443.0 | 0.39x | +2.1% | 424424 | 424424 | 4 % |  |
| tsql/rows1000 | generated | scriptdom | 604821.1 | 264331.2 | 0.44x | 268514.8 | 0.44x | +1.6% | 296472 | 296472 | 20 % |  |
| sql/select20.at | generated | hand | 7330.2 | 17739.5 | 2.42x | 17528.8 | 2.39x | -1.2% | 21416 | 21416 | 11 % |  |
| sql/select20.window | generated | hand | 7139.2 | 18204.9 | 2.55x | 18167.5 | 2.54x | -0.2% | 21416 | 21416 | 1 % |  |
| tsql/insert-values.at | generated | scriptdom | 8231.9 | 1202.5 | 0.15x | 1280.8 | 0.16x | +6.5% | 1328 | 1328 | 3 % |  |
| sql/select20.scan | generated | control | 376.4 | 378.9 | 1.01x | 376.2 | 1.00x | -0.7% | 0 | 0 | 4 % |  |
| sql/conditions100.scan | generated | control | 3575.4 | 3563.0 | 1.00x | 3500.6 | 0.98x | -1.8% | 0 | 0 | 60 % |  |
| tsql/select20.scan | generated | control | 384.8 | 398.0 | 1.03x | 384.9 | 1.00x | -3.3% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 138.8 | 138.0 | 0.99x | 138.5 | 1.00x | +0.4% | 0 | 0 | 8 % |  |
| sql/refused-late.bool | generated | hand | 2704.9 | 12301.0 | 4.55x | 5761.9 | 2.13x | -53.2% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 7370.0 | 18163.7 | 2.46x | 18178.4 | 2.47x | +0.1% | 21448 | 21392 | 7 % |  |
| sql/literal | generated | hand | 55.1 | 133.9 | 2.43x | 134.0 | 2.43x | +0.1% | 160 | 160 | 1 % |  |
| sql/comment | generated | hand | 2136.8 | 4635.4 | 2.17x | 4582.7 | 2.14x | -1.1% | 5136 | 5136 | 18 % |  |
| sql/conditions100 | generated | hand | 49782.0 | 121396.2 | 2.44x | 118171.2 | 2.37x | -2.7% | 161761 | 161761 | 26 % |  |
| sql/conditions1000 | generated | hand | 498360.2 | 1205533.6 | 2.42x | 1200231.2 | 2.41x | -0.4% | 1616160 | 1616160 | 3 % |  |
| tsql/comment | generated | scriptdom | 21264.5 | 1623.4 | 0.08x | 1678.3 | 0.08x | +3.4% | 1192 | 1192 | 17 % |  |
| sql/column | generated | hand | 141.0 | 364.1 | 2.58x | 343.4 | 2.44x | -5.7% | 392 | 392 | 13 % |  |
| sql/arithmetic | generated | hand | 1590.1 | 4206.8 | 2.65x | 4070.2 | 2.56x | -3.2% | 3472 | 3472 | 8 % |  |
| sql/nest8 | generated | hand | 2752.9 | 12227.2 | 4.44x | 11044.6 | 4.01x | -9.7% | 6504 | 6504 | 9 % |  |
| sql/condition | generated | hand | 1836.1 | 4354.2 | 2.37x | 4297.3 | 2.34x | -1.3% | 4928 | 4928 | 6 % |  |
| sql/select1 | generated | hand | 693.6 | 1533.3 | 2.21x | 1521.0 | 2.19x | -0.8% | 1688 | 1688 | 14 % |  |
| sql/select20 | generated | hand | 7264.8 | 18609.3 | 2.56x | 18378.9 | 2.53x | -1.2% | 21448 | 21448 | 6 % |  |
| sql/values | generated | hand | 454.4 | 1711.7 | 3.77x | 1641.8 | 3.61x | -4.1% | 1904 | 1904 | 3 % |  |
| sql/create | generated | hand | 807.7 | 2661.5 | 3.30x | 2615.3 | 3.24x | -1.7% | 1568 | 1568 | 18 % |  |
| sql/refused-late | generated | hand | 2688.5 | 12402.8 | 4.61x | 12268.3 | 4.56x | -1.1% | 13552 | 13552 | 6 % |  |
