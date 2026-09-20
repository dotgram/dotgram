# Paired stand, 2026-09-20 00:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/ladder.scan | generated | control | 137.4 | 138.3 | 1.01x | 139.2 | 1.01x | +0.7% | 0 | 0 |
| el/refused-early.bool | generated | hand | 334.1 | 1110.7 | 3.32x | 575.7 | 1.72x | -48.2% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1218.4 | 1813.0 | 1.49x | 935.7 | 0.77x | -48.4% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1039.7 | 1821.7 | 1.75x | 1769.3 | 1.70x | -2.9% | 1776 | 1720 |
| el/floor | generated | hand | 342.3 | 836.5 | 2.44x | 806.5 | 2.36x | -3.6% | 1104 | 1104 |
| el/floor | immediate | hand | 342.3 | 461.7 | 1.35x | 456.1 | 1.33x | -1.2% | 1120 | 1120 |
| el/ladder | generated | hand | 1047.8 | 1823.8 | 1.74x | 1786.9 | 1.71x | -2.0% | 1776 | 1776 |
| el/ladder | immediate | hand | 1047.8 | 1204.0 | 1.15x | 1203.1 | 1.15x | -0.1% | 1784 | 1784 |
| el/nest7 | generated | hand | 672.3 | 1730.4 | 2.57x | 1709.1 | 2.54x | -1.2% | 1152 | 1152 |
| el/nest7 | immediate | hand | 672.3 | 1029.7 | 1.53x | 1054.1 | 1.57x | +2.4% | 1120 | 1120 |
| el/block | generated | hand | 1057.0 | 1849.3 | 1.75x | 1866.4 | 1.77x | +0.9% | 2344 | 2344 |
| el/block | immediate | hand | 1057.0 | 1153.7 | 1.09x | 1156.5 | 1.09x | +0.2% | 2440 | 2440 |
| el/try | generated | hand | 3272.0 | 6219.1 | 1.90x | 4963.7 | 1.52x | -20.2% | 5632 | 4456 |
| el/try | immediate | hand | 3272.0 | 4775.0 | 1.46x | 3255.1 | 0.99x | -31.8% | 5752 | 4152 |
| el/loop | generated | hand | 2334.8 | 4042.7 | 1.73x | 3983.8 | 1.71x | -1.5% | 4776 | 4776 |
| el/loop | immediate | hand | 2334.8 | 2657.2 | 1.14x | 2700.1 | 1.16x | +1.6% | 4880 | 4880 |
| el/terms100 | generated | hand | 11982.6 | 16996.7 | 1.42x | 17113.8 | 1.43x | +0.7% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11982.6 | 13206.7 | 1.10x | 13303.0 | 1.11x | +0.7% | 18720 | 18720 |
| el/terms1000 | generated | hand | 113057.5 | 159873.8 | 1.41x | 155675.9 | 1.38x | -2.6% | 169104 | 169104 |
| el/terms1000 | immediate | hand | 113057.5 | 127092.5 | 1.12x | 130292.2 | 1.15x | +2.5% | 177123 | 177120 |
| el/overloads | generated | hand | 1883.3 | 2622.9 | 1.39x | 2580.1 | 1.37x | -1.6% | 4248 | 4248 |
| el/overloads | immediate | hand | 1883.3 | 2106.0 | 1.12x | 2043.5 | 1.09x | -3.0% | 4200 | 4200 |
| el/string | generated | hand | 285.2 | 976.5 | 3.42x | 972.1 | 3.41x | -0.4% | 1056 | 1056 |
| el/string | immediate | hand | 285.2 | 689.7 | 2.42x | 682.7 | 2.39x | -1.0% | 1032 | 1032 |
| el/interpolation | generated | hand | 1855.9 | 4626.5 | 2.49x | 4730.6 | 2.55x | +2.3% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1855.9 | 3949.6 | 2.13x | 3942.8 | 2.12x | -0.2% | 2424 | 2424 |
| el/untyped | generated | hand | 22919.7 | 19699.9 | 0.86x | 19752.9 | 0.86x | +0.3% | 16424 | 16424 |
| el/refused-early | generated | hand | 469.9 | 1250.8 | 2.66x | 1256.6 | 2.67x | +0.5% | 1064 | 1064 |
| el/refused-early | immediate | hand | 469.9 | 1181.3 | 2.51x | 1334.2 | 2.84x | +12.9% | 1944 | 1944 |
| el/refused-late | generated | hand | 1491.2 | 2000.4 | 1.34x | 1787.1 | 1.20x | -10.7% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1491.2 | 3410.0 | 2.29x | 3255.8 | 2.18x | -4.5% | 3928 | 3928 |
