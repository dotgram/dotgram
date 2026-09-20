# Paired stand, 2026-09-20 01:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1031731.2 | 119603.1 | 0.12x | 125484.4 | 0.12x | +4.9% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 668478.9 | 127239.8 | 0.19x | 128856.2 | 0.19x | +1.3% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 663257.0 | 123957.8 | 0.19x | 130152.3 | 0.20x | +5.0% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2650956.2 | 476775.0 | 0.18x | 507353.1 | 0.19x | +6.4% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2691665.6 | 484937.5 | 0.18x | 511228.1 | 0.19x | +5.4% | 422400 | 422403 |
| tsql/script400 | generated | scriptdom | 2664756.2 | 478150.0 | 0.18x | 507428.1 | 0.19x | +6.1% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1743714.1 | 188998.4 | 0.11x | 188285.9 | 0.11x | -0.4% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 899478.1 | 335132.0 | 0.37x | 341063.3 | 0.38x | +1.8% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 593607.0 | 244951.6 | 0.41x | 248263.3 | 0.42x | +1.4% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7203.8 | 17672.2 | 2.45x | 17833.3 | 2.48x | +0.9% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7324.5 | 18315.9 | 2.50x | 18354.6 | 2.51x | +0.2% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8159.6 | 1177.0 | 0.14x | 1220.5 | 0.15x | +3.7% | 1328 | 1328 |
| sql/select20.scan | generated | control | 390.4 | 392.8 | 1.01x | 390.3 | 1.00x | -0.7% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3578.8 | 3602.3 | 1.01x | 3675.9 | 1.03x | +2.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 387.6 | 387.9 | 1.00x | 387.4 | 1.00x | -0.1% | 0 | 0 |
| el/ladder.scan | generated | control | 141.8 | 140.7 | 0.99x | 139.9 | 0.99x | -0.6% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2691.2 | 12176.2 | 4.52x | 5874.9 | 2.18x | -51.8% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7303.3 | 18154.7 | 2.49x | 18337.0 | 2.51x | +1.0% | 21473 | 21392 |
| sql/literal | generated | hand | 53.8 | 125.5 | 2.33x | 123.8 | 2.30x | -1.3% | 160 | 160 |
| sql/comment | generated | hand | 2244.4 | 4824.7 | 2.15x | 4720.7 | 2.10x | -2.2% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 49063.8 | 119749.4 | 2.44x | 118702.3 | 2.42x | -0.9% | 161760 | 161760 |
| sql/conditions1000 | generated | hand | 505873.4 | 1228572.7 | 2.43x | 1207105.5 | 2.39x | -1.7% | 1616160 | 1616160 |
| tsql/comment | generated | scriptdom | 20928.2 | 1614.7 | 0.08x | 1638.7 | 0.08x | +1.5% | 1192 | 1192 |
| sql/column | generated | hand | 139.2 | 348.8 | 2.51x | 342.5 | 2.46x | -1.8% | 392 | 392 |
| sql/arithmetic | generated | hand | 1581.8 | 4151.7 | 2.62x | 4155.4 | 2.63x | +0.1% | 3472 | 3472 |
| sql/nest8 | generated | hand | 2859.1 | 11859.9 | 4.15x | 11281.7 | 3.95x | -4.9% | 6504 | 6504 |
| sql/condition | generated | hand | 1783.8 | 4346.6 | 2.44x | 4285.9 | 2.40x | -1.4% | 4928 | 4928 |
| sql/select1 | generated | hand | 687.6 | 1498.4 | 2.18x | 1545.5 | 2.25x | +3.1% | 1688 | 1688 |
| sql/select20 | generated | hand | 7366.2 | 18331.0 | 2.49x | 18751.7 | 2.55x | +2.3% | 21448 | 21448 |
| sql/values | generated | hand | 480.8 | 1714.9 | 3.57x | 1725.2 | 3.59x | +0.6% | 1904 | 1904 |
| sql/create | generated | hand | 806.6 | 2647.5 | 3.28x | 2623.0 | 3.25x | -0.9% | 1568 | 1568 |
| sql/refused-late | generated | hand | 2718.1 | 12281.3 | 4.52x | 12427.2 | 4.57x | +1.2% | 13552 | 13552 |
