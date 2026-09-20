# Paired stand, 2026-09-20 00:18

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/ladder.scan | generated | control | 138.5 | 132.9 | 0.96x | 133.4 | 0.96x | +0.4% | 0 | 0 |
| el/refused-early.bool | generated | hand | 328.7 | 1088.2 | 3.31x | 568.7 | 1.73x | -47.7% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1178.5 | 1789.3 | 1.52x | 912.2 | 0.77x | -49.0% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1036.5 | 1788.7 | 1.73x | 1761.2 | 1.70x | -1.5% | 1776 | 1720 |
| el/floor | generated | hand | 345.4 | 882.3 | 2.55x | 870.0 | 2.52x | -1.4% | 1104 | 1104 |
| el/floor | immediate | hand | 345.4 | 519.2 | 1.50x | 517.4 | 1.50x | -0.4% | 1120 | 1120 |
| el/ladder | generated | hand | 1042.7 | 1820.8 | 1.75x | 1845.7 | 1.77x | +1.4% | 1776 | 1776 |
| el/ladder | immediate | hand | 1042.7 | 1239.0 | 1.19x | 1210.7 | 1.16x | -2.3% | 1784 | 1784 |
| el/nest7 | generated | hand | 780.8 | 1813.2 | 2.32x | 1817.1 | 2.33x | +0.2% | 1152 | 1152 |
| el/nest7 | immediate | hand | 780.8 | 1099.3 | 1.41x | 1111.8 | 1.42x | +1.1% | 1120 | 1120 |
| el/block | generated | hand | 1137.5 | 1932.0 | 1.70x | 1897.9 | 1.67x | -1.8% | 2344 | 2344 |
| el/block | immediate | hand | 1137.5 | 1261.3 | 1.11x | 1261.2 | 1.11x | 0.0% | 2440 | 2440 |
| el/try | generated | hand | 3438.6 | 6477.9 | 1.88x | 5041.2 | 1.47x | -22.2% | 5632 | 4456 |
| el/try | immediate | hand | 3438.6 | 4961.4 | 1.44x | 3455.9 | 1.01x | -30.3% | 5752 | 4152 |
| el/loop | generated | hand | 2352.9 | 3926.0 | 1.67x | 3922.5 | 1.67x | -0.1% | 4776 | 4776 |
| el/loop | immediate | hand | 2352.9 | 2632.3 | 1.12x | 2745.0 | 1.17x | +4.3% | 4880 | 4880 |
| el/terms100 | generated | hand | 12741.6 | 16964.8 | 1.33x | 17503.9 | 1.37x | +3.2% | 17904 | 17904 |
| el/terms100 | immediate | hand | 12741.6 | 13539.6 | 1.06x | 13398.2 | 1.05x | -1.0% | 18720 | 18720 |
| el/terms1000 | generated | hand | 122339.9 | 160944.3 | 1.32x | 161706.7 | 1.32x | +0.5% | 169104 | 169107 |
| el/terms1000 | immediate | hand | 122339.9 | 128643.7 | 1.05x | 128565.9 | 1.05x | -0.1% | 177120 | 177120 |
| el/overloads | generated | hand | 1920.0 | 2725.4 | 1.42x | 2652.7 | 1.38x | -2.7% | 4248 | 4248 |
| el/overloads | immediate | hand | 1920.0 | 2099.3 | 1.09x | 2106.8 | 1.10x | +0.4% | 4200 | 4200 |
| el/string | generated | hand | 283.0 | 997.3 | 3.52x | 987.6 | 3.49x | -1.0% | 1056 | 1056 |
| el/string | immediate | hand | 283.0 | 674.1 | 2.38x | 699.3 | 2.47x | +3.7% | 1032 | 1032 |
| el/interpolation | generated | hand | 2176.2 | 5109.2 | 2.35x | 5217.8 | 2.40x | +2.1% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2176.2 | 3991.1 | 1.83x | 4173.1 | 1.92x | +4.6% | 2424 | 2424 |
| el/untyped | generated | hand | 22457.8 | 20369.6 | 0.91x | 25780.5 | 1.15x | +26.6% | 16424 | 16424 |
| el/refused-early | generated | hand | 518.6 | 1317.3 | 2.54x | 1334.3 | 2.57x | +1.3% | 1064 | 1064 |
| el/refused-early | immediate | hand | 518.6 | 1356.3 | 2.62x | 1322.1 | 2.55x | -2.5% | 1944 | 1944 |
| el/refused-late | generated | hand | 1522.8 | 1875.0 | 1.23x | 2075.9 | 1.36x | +10.7% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1522.8 | 3306.0 | 2.17x | 3489.1 | 2.29x | +5.5% | 3928 | 3928 |
