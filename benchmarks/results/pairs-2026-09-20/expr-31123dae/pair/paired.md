Median of 5 of 5 runs, each in a process of its own; control 31.8 ns (the runs' controls: 31.7, 31.8, 31.7, 32.3, 32.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 00:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/ladder.scan | generated | control | 138.5 | 138.3 | 1.00x | 138.0 | 1.00x | -0.2% | 0 | 0 |
| el/refused-early.bool | generated | hand | 334.1 | 1093.0 | 3.27x | 568.7 | 1.70x | -48.0% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1190.2 | 1789.3 | 1.50x | 926.4 | 0.78x | -48.2% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1036.5 | 1811.0 | 1.75x | 1769.3 | 1.71x | -2.3% | 1776 | 1720 |
| el/floor | generated | hand | 327.8 | 836.5 | 2.55x | 819.0 | 2.50x | -2.1% | 1104 | 1104 |
| el/floor | immediate | hand | 327.8 | 486.6 | 1.48x | 488.0 | 1.49x | +0.3% | 1120 | 1120 |
| el/ladder | generated | hand | 1017.2 | 1788.6 | 1.76x | 1784.3 | 1.75x | -0.2% | 1776 | 1776 |
| el/ladder | immediate | hand | 1017.2 | 1204.0 | 1.18x | 1202.2 | 1.18x | -0.2% | 1784 | 1784 |
| el/nest7 | generated | hand | 672.3 | 1730.4 | 2.57x | 1733.4 | 2.58x | +0.2% | 1152 | 1152 |
| el/nest7 | immediate | hand | 672.3 | 1093.7 | 1.63x | 1064.1 | 1.58x | -2.7% | 1120 | 1120 |
| el/block | generated | hand | 1078.8 | 1858.3 | 1.72x | 1866.4 | 1.73x | +0.4% | 2344 | 2344 |
| el/block | immediate | hand | 1078.8 | 1201.7 | 1.11x | 1186.9 | 1.10x | -1.2% | 2440 | 2440 |
| el/try | generated | hand | 3397.6 | 6267.8 | 1.84x | 4960.9 | 1.46x | -20.9% | 5632 | 4456 |
| el/try | immediate | hand | 3397.6 | 4910.7 | 1.45x | 3345.0 | 0.98x | -31.9% | 5752 | 4152 |
| el/loop | generated | hand | 2325.0 | 3891.3 | 1.67x | 3901.2 | 1.68x | +0.3% | 4776 | 4776 |
| el/loop | immediate | hand | 2325.0 | 2657.2 | 1.14x | 2659.4 | 1.14x | +0.1% | 4880 | 4880 |
| el/terms100 | generated | hand | 11982.6 | 16964.8 | 1.42x | 16886.6 | 1.41x | -0.5% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11982.6 | 13392.9 | 1.12x | 13394.1 | 1.12x | 0.0% | 18720 | 18720 |
| el/terms1000 | generated | hand | 113057.5 | 159873.8 | 1.41x | 157394.5 | 1.39x | -1.6% | 169104 | 169107 |
| el/terms1000 | immediate | hand | 113057.5 | 127092.5 | 1.12x | 126728.0 | 1.12x | -0.3% | 177120 | 177120 |
| el/overloads | generated | hand | 1901.4 | 2683.0 | 1.41x | 2652.7 | 1.40x | -1.1% | 4248 | 4248 |
| el/overloads | immediate | hand | 1901.4 | 2106.0 | 1.11x | 2106.8 | 1.11x | 0.0% | 4200 | 4200 |
| el/string | generated | hand | 287.2 | 997.3 | 3.47x | 987.6 | 3.44x | -1.0% | 1056 | 1056 |
| el/string | immediate | hand | 287.2 | 689.7 | 2.40x | 699.3 | 2.44x | +1.4% | 1032 | 1032 |
| el/interpolation | generated | hand | 2008.9 | 4874.9 | 2.43x | 4916.1 | 2.45x | +0.8% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2008.9 | 3991.1 | 1.99x | 3942.8 | 1.96x | -1.2% | 2424 | 2424 |
| el/untyped | generated | hand | 22457.8 | 20369.6 | 0.91x | 20864.9 | 0.93x | +2.4% | 16424 | 16424 |
| el/refused-early | generated | hand | 474.4 | 1260.8 | 2.66x | 1288.3 | 2.72x | +2.2% | 1064 | 1064 |
| el/refused-early | immediate | hand | 474.4 | 1295.3 | 2.73x | 1322.1 | 2.79x | +2.1% | 1944 | 1944 |
| el/refused-late | generated | hand | 1510.0 | 1933.4 | 1.28x | 1846.6 | 1.22x | -4.5% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1510.0 | 3395.0 | 2.25x | 3246.8 | 2.15x | -4.4% | 3928 | 3928 |
