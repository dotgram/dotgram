# Paired stand, 2026-09-20 00:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/ladder.scan | generated | control | 140.4 | 138.0 | 0.98x | 135.4 | 0.96x | -1.8% | 0 | 0 |
| el/refused-early.bool | generated | hand | 351.1 | 1083.9 | 3.09x | 546.7 | 1.56x | -49.6% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1184.0 | 1755.8 | 1.48x | 908.1 | 0.77x | -48.3% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1026.0 | 1756.9 | 1.71x | 1720.2 | 1.68x | -2.1% | 1776 | 1720 |
| el/floor | generated | hand | 321.5 | 815.9 | 2.54x | 814.8 | 2.53x | -0.1% | 1104 | 1104 |
| el/floor | immediate | hand | 321.5 | 482.3 | 1.50x | 464.5 | 1.44x | -3.7% | 1120 | 1120 |
| el/ladder | generated | hand | 1017.2 | 1766.0 | 1.74x | 1784.3 | 1.75x | +1.0% | 1776 | 1776 |
| el/ladder | immediate | hand | 1017.2 | 1202.8 | 1.18x | 1160.5 | 1.14x | -3.5% | 1784 | 1784 |
| el/nest7 | generated | hand | 670.6 | 1721.0 | 2.57x | 1733.4 | 2.58x | +0.7% | 1152 | 1152 |
| el/nest7 | immediate | hand | 670.6 | 1094.6 | 1.63x | 1064.1 | 1.59x | -2.8% | 1120 | 1120 |
| el/block | generated | hand | 1071.8 | 1855.5 | 1.73x | 1836.5 | 1.71x | -1.0% | 2344 | 2344 |
| el/block | immediate | hand | 1071.8 | 1201.7 | 1.12x | 1159.8 | 1.08x | -3.5% | 2440 | 2440 |
| el/try | generated | hand | 3233.9 | 6229.8 | 1.93x | 4791.6 | 1.48x | -23.1% | 5632 | 4456 |
| el/try | immediate | hand | 3233.9 | 4764.6 | 1.47x | 3261.5 | 1.01x | -31.5% | 5752 | 4152 |
| el/loop | generated | hand | 2252.8 | 3716.1 | 1.65x | 3743.8 | 1.66x | +0.7% | 4776 | 4776 |
| el/loop | immediate | hand | 2252.8 | 2569.2 | 1.14x | 2543.7 | 1.13x | -1.0% | 4880 | 4880 |
| el/terms100 | generated | hand | 11517.6 | 17185.1 | 1.49x | 16868.6 | 1.46x | -1.8% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11517.6 | 13425.5 | 1.17x | 13182.4 | 1.14x | -1.8% | 18720 | 18720 |
| el/terms1000 | generated | hand | 110727.8 | 160468.1 | 1.45x | 159081.8 | 1.44x | -0.9% | 169104 | 169129 |
| el/terms1000 | immediate | hand | 110727.8 | 128961.7 | 1.16x | 126162.0 | 1.14x | -2.2% | 177123 | 177120 |
| el/overloads | generated | hand | 1846.9 | 2631.7 | 1.42x | 2613.6 | 1.42x | -0.7% | 4248 | 4248 |
| el/overloads | immediate | hand | 1846.9 | 2057.9 | 1.11x | 2018.6 | 1.09x | -1.9% | 4200 | 4200 |
| el/string | generated | hand | 287.7 | 1024.7 | 3.56x | 1007.6 | 3.50x | -1.7% | 1056 | 1056 |
| el/string | immediate | hand | 287.7 | 680.6 | 2.37x | 662.2 | 2.30x | -2.7% | 1032 | 1032 |
| el/interpolation | generated | hand | 2059.6 | 4645.9 | 2.26x | 5078.3 | 2.47x | +9.3% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2059.6 | 4165.2 | 2.02x | 4065.6 | 1.97x | -2.4% | 2424 | 2424 |
| el/untyped | generated | hand | 18722.0 | 20579.0 | 1.10x | 20864.9 | 1.11x | +1.4% | 16424 | 16424 |
| el/refused-early | generated | hand | 481.1 | 1258.3 | 2.62x | 1288.3 | 2.68x | +2.4% | 1064 | 1064 |
| el/refused-early | immediate | hand | 481.1 | 1290.3 | 2.68x | 1285.4 | 2.67x | -0.4% | 1944 | 1944 |
| el/refused-late | generated | hand | 1510.0 | 2006.5 | 1.33x | 1808.5 | 1.20x | -9.9% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1510.0 | 3395.0 | 2.25x | 3181.3 | 2.11x | -6.3% | 3928 | 3928 |
