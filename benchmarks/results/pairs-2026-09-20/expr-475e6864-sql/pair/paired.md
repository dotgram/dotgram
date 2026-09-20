Median of 5 of 5 runs, each in a process of its own; control 31.7 ns (the runs' controls: 31.8, 31.5, 31.7, 31.4, 31.8).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 01:10

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1031731.2 | 126850.0 | 0.12x | 125565.6 | 0.12x | -1.0% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 668478.9 | 126518.0 | 0.19x | 125286.7 | 0.19x | -1.0% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 672860.9 | 127887.5 | 0.19x | 130152.3 | 0.19x | +1.8% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2703346.9 | 504337.5 | 0.19x | 502356.2 | 0.19x | -0.4% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2750978.1 | 506300.0 | 0.18x | 504437.5 | 0.18x | -0.4% | 422403 | 422403 |
| tsql/script400 | generated | scriptdom | 2758184.4 | 507196.9 | 0.18x | 511021.9 | 0.19x | +0.8% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1749504.7 | 192950.0 | 0.11x | 192525.0 | 0.11x | -0.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 937543.0 | 341593.8 | 0.36x | 341063.3 | 0.36x | -0.2% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 599456.2 | 247063.3 | 0.41x | 249934.4 | 0.42x | +1.2% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7439.4 | 17768.3 | 2.39x | 18248.6 | 2.45x | +2.7% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7556.5 | 19285.9 | 2.55x | 18772.5 | 2.48x | -2.7% | 21416 | 21441 |
| tsql/insert-values.at | generated | scriptdom | 8202.9 | 1226.9 | 0.15x | 1233.1 | 0.15x | +0.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 390.4 | 385.9 | 0.99x | 390.3 | 1.00x | +1.1% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3590.5 | 3607.8 | 1.00x | 3675.9 | 1.02x | +1.9% | 0 | 0 |
| tsql/select20.scan | generated | control | 387.6 | 387.9 | 1.00x | 387.4 | 1.00x | -0.1% | 0 | 0 |
| el/ladder.scan | generated | control | 141.8 | 141.1 | 0.99x | 139.9 | 0.99x | -0.8% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2700.5 | 12481.6 | 4.62x | 5990.7 | 2.22x | -52.0% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7368.0 | 18374.4 | 2.49x | 18525.6 | 2.51x | +0.8% | 21448 | 21392 |
| sql/literal | generated | hand | 53.5 | 133.1 | 2.49x | 135.7 | 2.54x | +2.0% | 160 | 160 |
| sql/comment | generated | hand | 2236.5 | 4757.5 | 2.13x | 4701.5 | 2.10x | -1.2% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 49914.2 | 124144.9 | 2.49x | 123567.4 | 2.48x | -0.5% | 161760 | 161760 |
| sql/conditions1000 | generated | hand | 505873.4 | 1235044.9 | 2.44x | 1229406.2 | 2.43x | -0.5% | 1616160 | 1616160 |
| tsql/comment | generated | scriptdom | 20277.0 | 1614.7 | 0.08x | 1586.5 | 0.08x | -1.7% | 1192 | 1192 |
| sql/column | generated | hand | 139.2 | 348.8 | 2.51x | 349.1 | 2.51x | +0.1% | 392 | 392 |
| sql/arithmetic | generated | hand | 1623.0 | 4203.7 | 2.59x | 4155.4 | 2.56x | -1.1% | 3472 | 3472 |
| sql/nest8 | generated | hand | 2859.1 | 11603.5 | 4.06x | 12191.3 | 4.26x | +5.1% | 6504 | 6504 |
| sql/condition | generated | hand | 1815.6 | 4332.0 | 2.39x | 4285.9 | 2.36x | -1.1% | 4928 | 4928 |
| sql/select1 | generated | hand | 691.6 | 1498.4 | 2.17x | 1512.4 | 2.19x | +0.9% | 1688 | 1688 |
| sql/select20 | generated | hand | 7366.2 | 18473.0 | 2.51x | 18437.5 | 2.50x | -0.2% | 21448 | 21448 |
| sql/values | generated | hand | 470.8 | 1762.3 | 3.74x | 1741.4 | 3.70x | -1.2% | 1904 | 1904 |
| sql/create | generated | hand | 806.6 | 2635.1 | 3.27x | 2632.9 | 3.26x | -0.1% | 1568 | 1568 |
| sql/refused-late | generated | hand | 2718.1 | 12631.6 | 4.65x | 12427.2 | 4.57x | -1.6% | 13552 | 13552 |
