# Paired stand, 2026-09-20 00:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/ladder.scan | generated | control | 138.1 | 138.8 | 1.01x | 138.0 | 1.00x | -0.6% | 0 | 0 |
| el/refused-early.bool | generated | hand | 331.1 | 1093.0 | 3.30x | 559.9 | 1.69x | -48.8% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1215.9 | 1804.5 | 1.48x | 926.4 | 0.76x | -48.7% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1052.7 | 1856.9 | 1.76x | 1781.3 | 1.69x | -4.1% | 1776 | 1720 |
| el/floor | generated | hand | 327.8 | 843.3 | 2.57x | 855.9 | 2.61x | +1.5% | 1104 | 1104 |
| el/floor | immediate | hand | 327.8 | 486.6 | 1.48x | 498.1 | 1.52x | +2.4% | 1120 | 1120 |
| el/ladder | generated | hand | 1008.7 | 1788.6 | 1.77x | 1759.8 | 1.74x | -1.6% | 1776 | 1776 |
| el/ladder | immediate | hand | 1008.7 | 1223.0 | 1.21x | 1178.7 | 1.17x | -3.6% | 1784 | 1784 |
| el/nest7 | generated | hand | 722.8 | 1726.1 | 2.39x | 1729.0 | 2.39x | +0.2% | 1152 | 1152 |
| el/nest7 | immediate | hand | 722.8 | 1052.6 | 1.46x | 1053.8 | 1.46x | +0.1% | 1120 | 1120 |
| el/block | generated | hand | 1078.8 | 1918.4 | 1.78x | 1870.6 | 1.73x | -2.5% | 2344 | 2344 |
| el/block | immediate | hand | 1078.8 | 1211.6 | 1.12x | 1198.3 | 1.11x | -1.1% | 2440 | 2440 |
| el/try | generated | hand | 3431.2 | 6458.5 | 1.88x | 4960.9 | 1.45x | -23.2% | 5632 | 4456 |
| el/try | immediate | hand | 3431.2 | 4910.7 | 1.43x | 3440.2 | 1.00x | -29.9% | 5752 | 4152 |
| el/loop | generated | hand | 2325.0 | 3891.3 | 1.67x | 3893.6 | 1.67x | +0.1% | 4776 | 4776 |
| el/loop | immediate | hand | 2325.0 | 2701.6 | 1.16x | 2652.0 | 1.14x | -1.8% | 4880 | 4880 |
| el/terms100 | generated | hand | 12200.6 | 16542.7 | 1.36x | 16571.0 | 1.36x | +0.2% | 17904 | 17904 |
| el/terms100 | immediate | hand | 12200.6 | 13059.0 | 1.07x | 13394.1 | 1.10x | +2.6% | 18720 | 18720 |
| el/terms1000 | generated | hand | 110260.6 | 156080.0 | 1.42x | 155389.9 | 1.41x | -0.4% | 169129 | 169129 |
| el/terms1000 | immediate | hand | 110260.6 | 123906.2 | 1.12x | 126728.0 | 1.15x | +2.3% | 177120 | 177123 |
| el/overloads | generated | hand | 1901.4 | 2705.5 | 1.42x | 2659.7 | 1.40x | -1.7% | 4248 | 4248 |
| el/overloads | immediate | hand | 1901.4 | 2227.8 | 1.17x | 2217.6 | 1.17x | -0.5% | 4200 | 4200 |
| el/string | generated | hand | 290.2 | 1008.6 | 3.48x | 1008.0 | 3.47x | -0.1% | 1056 | 1056 |
| el/string | immediate | hand | 290.2 | 713.1 | 2.46x | 720.0 | 2.48x | +1.0% | 1032 | 1032 |
| el/interpolation | generated | hand | 2008.9 | 4874.9 | 2.43x | 4916.1 | 2.45x | +0.8% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2008.9 | 3906.2 | 1.94x | 3772.8 | 1.88x | -3.4% | 2424 | 2424 |
| el/untyped | generated | hand | 26047.6 | 20751.2 | 0.80x | 20401.5 | 0.78x | -1.7% | 16424 | 16424 |
| el/refused-early | generated | hand | 474.4 | 1301.4 | 2.74x | 1345.4 | 2.84x | +3.4% | 1064 | 1064 |
| el/refused-early | immediate | hand | 474.4 | 1387.6 | 2.93x | 1325.9 | 2.80x | -4.4% | 1944 | 1944 |
| el/refused-late | generated | hand | 1548.3 | 1891.8 | 1.22x | 2033.4 | 1.31x | +7.5% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1548.3 | 3439.3 | 2.22x | 3203.3 | 2.07x | -6.9% | 3928 | 3928 |
