# Paired stand, 2026-09-20 01:10

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1112809.4 | 141575.0 | 0.13x | 131350.0 | 0.12x | -7.2% | 113600 | 105603 |
| tsql/script100.boolboth | generated | scriptdom | 711402.3 | 126518.0 | 0.18x | 125286.7 | 0.18x | -1.0% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 722246.9 | 128257.8 | 0.18x | 131050.0 | 0.18x | +2.2% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2833034.4 | 514009.4 | 0.18x | 511021.9 | 0.18x | -0.6% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2789837.5 | 506300.0 | 0.18x | 495150.0 | 0.18x | -2.2% | 422400 | 422403 |
| tsql/script400 | generated | scriptdom | 2870637.5 | 514340.6 | 0.18x | 525446.9 | 0.18x | +2.2% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1870240.6 | 200525.0 | 0.11x | 196146.9 | 0.10x | -2.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 937543.0 | 341593.8 | 0.36x | 338603.9 | 0.36x | -0.9% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 599456.2 | 247063.3 | 0.41x | 249934.4 | 0.42x | +1.2% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7439.4 | 17768.3 | 2.39x | 18275.7 | 2.46x | +2.9% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7599.4 | 19347.2 | 2.55x | 19492.1 | 2.56x | +0.7% | 21441 | 21441 |
| tsql/insert-values.at | generated | scriptdom | 8801.9 | 1241.1 | 0.14x | 1233.9 | 0.14x | -0.6% | 1328 | 1328 |
| sql/select20.scan | generated | control | 383.5 | 385.9 | 1.01x | 384.8 | 1.00x | -0.3% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3607.2 | 3607.8 | 1.00x | 3620.5 | 1.00x | +0.4% | 0 | 0 |
| tsql/select20.scan | generated | control | 397.1 | 390.4 | 0.98x | 388.9 | 0.98x | -0.4% | 0 | 0 |
| el/ladder.scan | generated | control | 141.0 | 141.1 | 1.00x | 138.6 | 0.98x | -1.8% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2700.5 | 12398.4 | 4.59x | 6038.3 | 2.24x | -51.3% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7368.0 | 18374.4 | 2.49x | 18776.5 | 2.55x | +2.2% | 21448 | 21392 |
| sql/literal | generated | hand | 51.8 | 135.1 | 2.61x | 135.7 | 2.62x | +0.5% | 160 | 160 |
| sql/comment | generated | hand | 2236.5 | 4757.5 | 2.13x | 4977.9 | 2.23x | +4.6% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 52854.3 | 127265.2 | 2.41x | 127320.4 | 2.41x | 0.0% | 161760 | 161760 |
| sql/conditions1000 | generated | hand | 521300.8 | 1259030.1 | 2.42x | 1299710.2 | 2.49x | +3.2% | 1616160 | 1616160 |
| tsql/comment | generated | scriptdom | 22251.7 | 1635.8 | 0.07x | 1621.2 | 0.07x | -0.9% | 1192 | 1192 |
| sql/column | generated | hand | 143.3 | 348.8 | 2.43x | 360.5 | 2.52x | +3.3% | 392 | 392 |
| sql/arithmetic | generated | hand | 1688.6 | 4299.8 | 2.55x | 4465.6 | 2.64x | +3.9% | 3472 | 3472 |
| sql/nest8 | generated | hand | 2883.3 | 11603.5 | 4.02x | 12689.1 | 4.40x | +9.4% | 6504 | 6504 |
| sql/condition | generated | hand | 1821.2 | 4437.9 | 2.44x | 4571.8 | 2.51x | +3.0% | 4928 | 4928 |
| sql/select1 | generated | hand | 705.7 | 1550.7 | 2.20x | 1625.8 | 2.30x | +4.8% | 1688 | 1688 |
| sql/select20 | generated | hand | 7547.9 | 18660.5 | 2.47x | 19336.9 | 2.56x | +3.6% | 21448 | 21448 |
| sql/values | generated | hand | 470.8 | 1762.3 | 3.74x | 1839.9 | 3.91x | +4.4% | 1904 | 1904 |
| sql/create | generated | hand | 838.9 | 2800.3 | 3.34x | 2831.1 | 3.37x | +1.1% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3085.0 | 13117.9 | 4.25x | 13546.8 | 4.39x | +3.3% | 13552 | 13552 |
