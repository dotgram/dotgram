# Paired stand, 2026-09-20 02:31

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1313693.8 | 136778.1 | 0.10x | 125862.5 | 0.10x | -8.0% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 663650.0 | 121336.7 | 0.18x | 128171.1 | 0.19x | +5.6% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 673403.1 | 123378.1 | 0.18x | 126221.9 | 0.19x | +2.3% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2637075.0 | 498662.5 | 0.19x | 482943.8 | 0.18x | -3.2% | 454400 | 422400 |
| tsql/script400.boolboth | generated | scriptdom | 2693615.6 | 480490.6 | 0.18x | 488853.1 | 0.18x | +1.7% | 422403 | 422403 |
| tsql/script400 | generated | scriptdom | 2649075.0 | 488709.4 | 0.18x | 504640.6 | 0.19x | +3.3% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1835800.0 | 194498.4 | 0.11x | 206293.8 | 0.11x | +6.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 897395.3 | 342155.5 | 0.38x | 369949.2 | 0.41x | +8.1% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 613766.4 | 256979.7 | 0.42x | 275740.6 | 0.45x | +7.3% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7331.6 | 18499.8 | 2.52x | 17480.2 | 2.38x | -5.5% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7306.9 | 19248.9 | 2.63x | 18356.2 | 2.51x | -4.6% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8319.9 | 1183.8 | 0.14x | 1225.6 | 0.15x | +3.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 386.9 | 392.2 | 1.01x | 385.1 | 1.00x | -1.8% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3544.0 | 3588.7 | 1.01x | 3592.6 | 1.01x | +0.1% | 0 | 0 |
| tsql/select20.scan | generated | control | 402.0 | 391.9 | 0.97x | 391.3 | 0.97x | -0.1% | 0 | 0 |
| el/ladder.scan | generated | control | 141.3 | 139.5 | 0.99x | 138.4 | 0.98x | -0.8% | 0 | 0 |
| el/refused-early.bool | generated | hand | 337.2 | 1108.4 | 3.29x | 579.4 | 1.72x | -47.7% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1266.7 | 1870.8 | 1.48x | 992.4 | 0.78x | -47.0% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1023.0 | 1794.1 | 1.75x | 1807.7 | 1.77x | +0.8% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2745.5 | 12977.3 | 4.73x | 5982.7 | 2.18x | -53.9% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7405.9 | 19352.7 | 2.61x | 18274.2 | 2.47x | -5.6% | 21448 | 21392 |
| el/floor | generated | hand | 333.4 | 857.0 | 2.57x | 895.3 | 2.69x | +4.5% | 1104 | 1104 |
| el/floor | immediate | hand | 333.4 | 523.2 | 1.57x | 513.7 | 1.54x | -1.8% | 1120 | 1120 |
| el/ladder | generated | hand | 1047.4 | 1814.8 | 1.73x | 1875.0 | 1.79x | +3.3% | 1776 | 1776 |
| el/ladder | immediate | hand | 1047.4 | 1220.7 | 1.17x | 1213.0 | 1.16x | -0.6% | 1784 | 1784 |
| el/nest7 | generated | hand | 754.6 | 1796.0 | 2.38x | 1802.9 | 2.39x | +0.4% | 1152 | 1152 |
| el/nest7 | immediate | hand | 754.6 | 1096.1 | 1.45x | 1084.0 | 1.44x | -1.1% | 1120 | 1120 |
| el/block | generated | hand | 1074.1 | 1887.9 | 1.76x | 1911.8 | 1.78x | +1.3% | 2344 | 2344 |
| el/block | immediate | hand | 1074.1 | 1202.3 | 1.12x | 1234.6 | 1.15x | +2.7% | 2440 | 2440 |
| el/try | generated | hand | 3248.3 | 4650.6 | 1.43x | 4732.2 | 1.46x | +1.8% | 4456 | 4456 |
| el/try | immediate | hand | 3248.3 | 3263.0 | 1.00x | 3374.6 | 1.04x | +3.4% | 4152 | 4152 |
| el/loop | generated | hand | 2291.0 | 3774.8 | 1.65x | 3855.6 | 1.68x | +2.1% | 4776 | 4776 |
| el/loop | immediate | hand | 2291.0 | 2610.9 | 1.14x | 2554.3 | 1.11x | -2.2% | 4880 | 4883 |
| el/terms100 | generated | hand | 11696.2 | 16354.0 | 1.40x | 16624.2 | 1.42x | +1.7% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11696.2 | 12848.4 | 1.10x | 13681.6 | 1.17x | +6.5% | 18720 | 18720 |
| el/terms1000 | generated | hand | 114416.5 | 155366.0 | 1.36x | 159752.6 | 1.40x | +2.8% | 169104 | 169107 |
| el/terms1000 | immediate | hand | 114416.5 | 123787.7 | 1.08x | 130445.0 | 1.14x | +5.4% | 177120 | 177120 |
| el/overloads | generated | hand | 1923.0 | 2665.4 | 1.39x | 2687.9 | 1.40x | +0.8% | 4248 | 4248 |
| el/overloads | immediate | hand | 1923.0 | 2076.2 | 1.08x | 2101.1 | 1.09x | +1.2% | 4200 | 4200 |
| el/string | generated | hand | 290.0 | 1003.8 | 3.46x | 1035.4 | 3.57x | +3.1% | 1056 | 1056 |
| el/string | immediate | hand | 290.0 | 676.0 | 2.33x | 682.6 | 2.35x | +1.0% | 1032 | 1032 |
| el/interpolation | generated | hand | 2023.7 | 4960.5 | 2.45x | 4939.9 | 2.44x | -0.4% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2023.7 | 4869.2 | 2.41x | 4066.0 | 2.01x | -16.5% | 2424 | 2424 |
| el/untyped | generated | hand | 19125.6 | 26524.2 | 1.39x | 19963.7 | 1.04x | -24.7% | 16424 | 16424 |
| el/refused-early | generated | hand | 477.4 | 1245.3 | 2.61x | 1312.4 | 2.75x | +5.4% | 1064 | 1064 |
| el/refused-early | immediate | hand | 477.4 | 1239.2 | 2.60x | 1265.7 | 2.65x | +2.1% | 1944 | 1944 |
| el/refused-late | generated | hand | 1397.8 | 1940.0 | 1.39x | 1811.2 | 1.30x | -6.6% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1397.8 | 3227.6 | 2.31x | 3297.5 | 2.36x | +2.2% | 3928 | 3928 |
| sql/literal | generated | hand | 66.1 | 145.4 | 2.20x | 162.3 | 2.46x | +11.6% | 160 | 160 |
| sql/comment | generated | hand | 2869.1 | 5602.3 | 1.95x | 5405.5 | 1.88x | -3.5% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 73579.1 | 149896.1 | 2.04x | 142431.5 | 1.94x | -5.0% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 728130.5 | 1455403.1 | 2.00x | 1416955.5 | 1.95x | -2.6% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 25972.9 | 1584.8 | 0.06x | 1616.7 | 0.06x | +2.0% | 1192 | 1192 |
| sql/column | generated | hand | 183.1 | 391.7 | 2.14x | 386.0 | 2.11x | -1.5% | 392 | 392 |
| sql/arithmetic | generated | hand | 2243.0 | 4762.0 | 2.12x | 4753.0 | 2.12x | -0.2% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3889.2 | 13052.9 | 3.36x | 13779.1 | 3.54x | +5.6% | 6504 | 6504 |
| sql/condition | generated | hand | 2845.5 | 5315.9 | 1.87x | 5231.5 | 1.84x | -1.6% | 4928 | 4928 |
| sql/select1 | generated | hand | 928.5 | 1860.6 | 2.00x | 1854.2 | 2.00x | -0.3% | 1688 | 1688 |
| sql/select20 | generated | hand | 10742.7 | 23087.1 | 2.15x | 21859.6 | 2.03x | -5.3% | 21448 | 21448 |
| sql/values | generated | hand | 779.6 | 2009.0 | 2.58x | 2027.9 | 2.60x | +0.9% | 1904 | 1904 |
| sql/create | generated | hand | 999.0 | 2815.7 | 2.82x | 2934.8 | 2.94x | +4.2% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3821.1 | 15053.1 | 3.94x | 14057.9 | 3.68x | -6.6% | 13552 | 13552 |
