# Paired stand, 2026-09-20 00:14

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/ladder.scan | generated | control | 139.1 | 141.3 | 1.02x | 139.0 | 1.00x | -1.7% | 0 | 0 |
| el/refused-early.bool | generated | hand | 342.7 | 1116.3 | 3.26x | 570.3 | 1.66x | -48.9% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1190.2 | 1782.2 | 1.50x | 931.6 | 0.78x | -47.7% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1012.1 | 1811.0 | 1.79x | 1832.6 | 1.81x | +1.2% | 1776 | 1720 |
| el/floor | generated | hand | 318.2 | 825.5 | 2.59x | 819.0 | 2.57x | -0.8% | 1104 | 1104 |
| el/floor | immediate | hand | 318.2 | 494.4 | 1.55x | 488.0 | 1.53x | -1.3% | 1120 | 1120 |
| el/ladder | generated | hand | 1012.7 | 1780.6 | 1.76x | 1771.5 | 1.75x | -0.5% | 1776 | 1776 |
| el/ladder | immediate | hand | 1012.7 | 1188.1 | 1.17x | 1202.2 | 1.19x | +1.2% | 1784 | 1784 |
| el/nest7 | generated | hand | 660.0 | 1758.8 | 2.66x | 1751.6 | 2.65x | -0.4% | 1152 | 1152 |
| el/nest7 | immediate | hand | 660.0 | 1093.7 | 1.66x | 1089.6 | 1.65x | -0.4% | 1120 | 1120 |
| el/block | generated | hand | 1086.3 | 1858.3 | 1.71x | 1817.1 | 1.67x | -2.2% | 2344 | 2344 |
| el/block | immediate | hand | 1086.3 | 1189.1 | 1.09x | 1186.9 | 1.09x | -0.2% | 2440 | 2440 |
| el/try | generated | hand | 3397.6 | 6267.8 | 1.84x | 4925.1 | 1.45x | -21.4% | 5632 | 4456 |
| el/try | immediate | hand | 3397.6 | 4917.7 | 1.45x | 3345.0 | 0.98x | -32.0% | 5752 | 4152 |
| el/loop | generated | hand | 2252.0 | 3849.2 | 1.71x | 3901.2 | 1.73x | +1.4% | 4776 | 4776 |
| el/loop | immediate | hand | 2252.0 | 2679.0 | 1.19x | 2659.4 | 1.18x | -0.7% | 4880 | 4880 |
| el/terms100 | generated | hand | 11794.6 | 16856.0 | 1.43x | 16886.6 | 1.43x | +0.2% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11794.6 | 13392.9 | 1.14x | 13458.7 | 1.14x | +0.5% | 18720 | 18720 |
| el/terms1000 | generated | hand | 115416.4 | 159241.3 | 1.38x | 157394.5 | 1.36x | -1.2% | 169104 | 169104 |
| el/terms1000 | immediate | hand | 115416.4 | 125442.8 | 1.09x | 126608.7 | 1.10x | +0.9% | 177120 | 177123 |
| el/overloads | generated | hand | 1902.3 | 2683.0 | 1.41x | 2665.0 | 1.40x | -0.7% | 4248 | 4248 |
| el/overloads | immediate | hand | 1902.3 | 2147.4 | 1.13x | 2152.0 | 1.13x | +0.2% | 4200 | 4200 |
| el/string | generated | hand | 287.2 | 979.7 | 3.41x | 981.2 | 3.42x | +0.1% | 1056 | 1056 |
| el/string | immediate | hand | 287.2 | 703.6 | 2.45x | 701.3 | 2.44x | -0.3% | 1032 | 1032 |
| el/interpolation | generated | hand | 1814.5 | 5005.9 | 2.76x | 4805.8 | 2.65x | -4.0% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1814.5 | 4200.7 | 2.32x | 3710.7 | 2.05x | -11.7% | 2424 | 2424 |
| el/untyped | generated | hand | 21564.9 | 20252.5 | 0.94x | 23109.9 | 1.07x | +14.1% | 16424 | 16424 |
| el/refused-early | generated | hand | 472.0 | 1260.8 | 2.67x | 1272.4 | 2.70x | +0.9% | 1064 | 1064 |
| el/refused-early | immediate | hand | 472.0 | 1295.3 | 2.74x | 1254.7 | 2.66x | -3.1% | 1944 | 1944 |
| el/refused-late | generated | hand | 1438.3 | 1933.4 | 1.34x | 1846.6 | 1.28x | -4.5% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1438.3 | 3267.8 | 2.27x | 3246.8 | 2.26x | -0.6% | 3928 | 3928 |
