# Paired stand, 2026-09-20 01:08

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 988140.6 | 130743.8 | 0.13x | 123821.9 | 0.13x | -5.3% | 113600 | 105603 |
| tsql/script100.boolboth | generated | scriptdom | 656668.0 | 127139.1 | 0.19x | 121403.9 | 0.18x | -4.5% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 655720.3 | 128760.9 | 0.20x | 123032.8 | 0.19x | -4.4% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2644246.9 | 512453.1 | 0.19x | 486153.1 | 0.18x | -5.1% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2636356.2 | 512912.5 | 0.19x | 483018.8 | 0.18x | -5.8% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2608681.2 | 513959.4 | 0.20x | 489800.0 | 0.19x | -4.7% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1723426.6 | 188467.2 | 0.11x | 189298.4 | 0.11x | +0.4% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 875526.6 | 339668.0 | 0.39x | 338724.2 | 0.39x | -0.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 577424.2 | 241889.1 | 0.42x | 246148.4 | 0.43x | +1.8% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7212.0 | 17457.3 | 2.42x | 17610.7 | 2.44x | +0.9% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7210.0 | 18208.4 | 2.53x | 18026.4 | 2.50x | -1.0% | 21416 | 21441 |
| tsql/insert-values.at | generated | scriptdom | 8051.5 | 1253.7 | 0.16x | 1152.8 | 0.14x | -8.1% | 1328 | 1328 |
| sql/select20.scan | generated | control | 374.5 | 385.3 | 1.03x | 397.5 | 1.06x | +3.2% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3499.6 | 3512.5 | 1.00x | 3755.1 | 1.07x | +6.9% | 0 | 0 |
| tsql/select20.scan | generated | control | 372.7 | 381.4 | 1.02x | 380.1 | 1.02x | -0.3% | 0 | 0 |
| el/ladder.scan | generated | control | 137.3 | 140.4 | 1.02x | 138.4 | 1.01x | -1.5% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2662.1 | 12481.6 | 4.69x | 5880.2 | 2.21x | -52.9% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7153.4 | 18207.5 | 2.55x | 18200.0 | 2.54x | 0.0% | 21473 | 21392 |
| sql/literal | generated | hand | 54.5 | 132.0 | 2.42x | 132.1 | 2.42x | 0.0% | 160 | 160 |
| sql/comment | generated | hand | 2174.7 | 4611.9 | 2.12x | 4584.6 | 2.11x | -0.6% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 48562.4 | 119941.9 | 2.47x | 117868.4 | 2.43x | -1.7% | 161760 | 161760 |
| sql/conditions1000 | generated | hand | 492339.5 | 1235044.9 | 2.51x | 1229406.2 | 2.50x | -0.5% | 1616160 | 1616160 |
| tsql/comment | generated | scriptdom | 19866.8 | 1619.9 | 0.08x | 1586.5 | 0.08x | -2.1% | 1192 | 1192 |
| sql/column | generated | hand | 140.0 | 343.9 | 2.46x | 349.1 | 2.49x | +1.5% | 392 | 392 |
| sql/arithmetic | generated | hand | 1623.0 | 4168.5 | 2.57x | 4132.8 | 2.55x | -0.9% | 3472 | 3472 |
| sql/nest8 | generated | hand | 2772.3 | 12063.4 | 4.35x | 11206.2 | 4.04x | -7.1% | 6504 | 6504 |
| sql/condition | generated | hand | 1815.6 | 4282.8 | 2.36x | 4257.2 | 2.34x | -0.6% | 4928 | 4928 |
| sql/select1 | generated | hand | 691.6 | 1497.5 | 2.17x | 1506.7 | 2.18x | +0.6% | 1688 | 1688 |
| sql/select20 | generated | hand | 7334.7 | 18473.0 | 2.52x | 18437.5 | 2.51x | -0.2% | 21448 | 21448 |
| sql/values | generated | hand | 467.0 | 1796.2 | 3.85x | 1661.4 | 3.56x | -7.5% | 1904 | 1904 |
| sql/create | generated | hand | 814.3 | 2635.1 | 3.24x | 2658.4 | 3.26x | +0.9% | 1568 | 1568 |
| sql/refused-late | generated | hand | 2713.5 | 12631.6 | 4.66x | 12570.1 | 4.63x | -0.5% | 13552 | 13552 |
