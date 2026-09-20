Median of 9 of 9 runs, each in a process of its own; control 31.2 ns (the runs' controls: 31.2, 31.4, 31.3, 31.1, 31.1, 31.2, 31.0, 31.1, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 03:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1125056.2 | 133162.5 | 0.12x | 133943.8 | 0.12x | +0.6% | 113600 | 105600 | 61 % | [-8.4%..+8.9%], 4 of 9 positive |
| tsql/script100.boolboth | generated | scriptdom | 657568.0 | 122213.3 | 0.19x | 122362.5 | 0.19x | +0.1% | 105600 | 105600 | 3 % | [-3.6%..+4.2%], 5 of 9 positive |
| tsql/script100 | generated | scriptdom | 663204.7 | 124052.3 | 0.19x | 124138.3 | 0.19x | +0.1% | 113600 | 113600 | 4 % | [-5.2%..+3.2%], 5 of 9 positive |
| tsql/script400.bool | generated | scriptdom | 2669234.4 | 497784.4 | 0.19x | 490028.1 | 0.18x | -1.6% | 454400 | 422403 | 4 % | [-8.1%..+3.3%], 1 of 9 positive |
| tsql/script400.boolboth | generated | scriptdom | 2670762.5 | 489925.0 | 0.18x | 491340.6 | 0.18x | +0.3% | 422400 | 422403 | 3 % | [-3.1%..+3.1%], 5 of 9 positive |
| tsql/script400 | generated | scriptdom | 2672012.5 | 494046.9 | 0.18x | 495946.9 | 0.19x | +0.4% | 454400 | 454400 | 5 % | [-2.0%..+3.6%], 6 of 9 positive |
| tsql/columns1000 | generated | scriptdom | 1755442.2 | 191592.2 | 0.11x | 190210.9 | 0.11x | -0.7% | 200464 | 200464 | 4 % | [-7.3%..+2.7%], 2 of 9 positive |
| tsql/conditions1000 | generated | scriptdom | 884253.9 | 336307.8 | 0.38x | 340676.6 | 0.39x | +1.3% | 424424 | 424424 | 11 % | [-1.6%..+2.1%], 7 of 9 positive |
| tsql/rows1000 | generated | scriptdom | 589949.6 | 246647.3 | 0.42x | 245236.7 | 0.42x | -0.6% | 296472 | 296472 | 9 % | [-1.8%..+1.6%], 4 of 9 positive |
| sql/select20.at | generated | hand | 7262.9 | 17775.9 | 2.45x | 17662.7 | 2.43x | -0.6% | 21416 | 21416 | 7 % | [-6.9%..+0.8%], 2 of 9 positive |
| sql/select20.window | generated | hand | 7175.0 | 18204.9 | 2.54x | 18019.8 | 2.51x | -1.0% | 21416 | 21416 | 3 % | [-6.8%..+0.3%], 1 of 9 positive |
| tsql/insert-values.at | generated | scriptdom | 8247.9 | 1190.7 | 0.14x | 1200.9 | 0.15x | +0.9% | 1328 | 1328 | 5 % | [-3.1%..+6.5%], 4 of 9 positive |
| sql/select20.scan | generated | control | 386.7 | 382.3 | 0.99x | 381.8 | 0.99x | -0.1% | 0 | 0 | 6 % | [-10.3%..+2.8%], 3 of 9 positive |
| sql/conditions100.scan | generated | control | 3552.2 | 3559.4 | 1.00x | 3529.5 | 0.99x | -0.8% | 0 | 0 | 2 % | [-5.9%..+2.5%], 1 of 9 positive |
| tsql/select20.scan | generated | control | 378.4 | 382.8 | 1.01x | 380.6 | 1.01x | -0.6% | 0 | 0 | 6 % | [-3.3%..+0.9%], 3 of 9 positive |
| el/ladder.scan | generated | control | 138.8 | 139.0 | 1.00x | 138.0 | 0.99x | -0.7% | 0 | 0 | 3 % | [-2.7%..+0.4%], 2 of 9 positive |
| sql/refused-late.bool | generated | hand | 2672.8 | 12406.3 | 4.64x | 5850.2 | 2.19x | -52.8% | 13552 | 6616 | 9 % | [-53.6%..-51.5%], 0 of 9 positive |
| sql/select20.bool | generated | hand | 7344.3 | 18609.2 | 2.53x | 18180.0 | 2.48x | -2.3% | 21448 | 21392 | 34 % | [-8.9%..+2.1%], 4 of 9 positive |
| sql/literal | generated | hand | 51.9 | 129.1 | 2.49x | 130.7 | 2.52x | +1.3% | 160 | 160 | 9 % | [-4.5%..+3.0%], 6 of 9 positive |
| sql/comment | generated | hand | 2136.8 | 4635.4 | 2.17x | 4582.7 | 2.14x | -1.1% | 5136 | 5136 | 3 % | [-4.2%..+0.1%], 1 of 9 positive |
| sql/conditions100 | generated | hand | 49489.7 | 119607.7 | 2.42x | 118171.2 | 2.39x | -1.2% | 161736 | 161736 | 7 % | [-3.0%..+4.9%], 3 of 9 positive |
| sql/conditions1000 | generated | hand | 497693.8 | 1212791.4 | 2.44x | 1205654.3 | 2.42x | -0.6% | 1616160 | 1616160 | 5 % | [-3.9%..+3.4%], 3 of 9 positive |
| tsql/comment | generated | scriptdom | 19525.0 | 1569.9 | 0.08x | 1583.8 | 0.08x | +0.9% | 1192 | 1192 | 9 % | [-3.1%..+3.4%], 6 of 9 positive |
| sql/column | generated | hand | 137.8 | 349.7 | 2.54x | 343.4 | 2.49x | -1.8% | 392 | 392 | 10 % | [-5.7%..+3.8%], 4 of 9 positive |
| sql/arithmetic | generated | hand | 1604.3 | 4206.8 | 2.62x | 4131.4 | 2.58x | -1.8% | 3472 | 3472 | 7 % | [-3.2%..+2.0%], 2 of 9 positive |
| sql/nest8 | generated | hand | 2733.1 | 11920.2 | 4.36x | 11278.8 | 4.13x | -5.4% | 6504 | 6504 | 4 % | [-9.7%..+2.7%], 3 of 9 positive |
| sql/condition | generated | hand | 1775.8 | 4261.9 | 2.40x | 4297.3 | 2.42x | +0.8% | 4928 | 4928 | 5 % | [-2.9%..+4.0%], 3 of 9 positive |
| sql/select1 | generated | hand | 678.5 | 1511.5 | 2.23x | 1507.1 | 2.22x | -0.3% | 1688 | 1688 | 6 % | [-2.6%..+3.3%], 2 of 9 positive |
| sql/select20 | generated | hand | 7273.5 | 18609.3 | 2.56x | 18063.0 | 2.48x | -2.9% | 21448 | 21448 | 2 % | [-6.5%..+2.2%], 2 of 9 positive |
| sql/values | generated | hand | 454.3 | 1729.7 | 3.81x | 1698.1 | 3.74x | -1.8% | 1904 | 1904 | 10 % | [-4.7%..+2.9%], 2 of 9 positive |
| sql/create | generated | hand | 785.8 | 2635.9 | 3.35x | 2629.6 | 3.35x | -0.2% | 1568 | 1568 | 6 % | [-2.2%..+1.8%], 5 of 9 positive |
| sql/refused-late | generated | hand | 2681.3 | 12449.3 | 4.64x | 12267.6 | 4.58x | -1.5% | 13552 | 13552 | 3 % | [-3.2%..+0.6%], 2 of 9 positive |
