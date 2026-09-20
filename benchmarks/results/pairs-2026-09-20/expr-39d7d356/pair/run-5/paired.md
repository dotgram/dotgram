# Paired stand, 2026-09-20 02:40

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1140846.9 | 178809.4 | 0.16x | 180778.1 | 0.16x | +1.1% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 697930.5 | 129460.9 | 0.19x | 128504.7 | 0.18x | -0.7% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 687847.7 | 126476.6 | 0.18x | 127343.0 | 0.19x | +0.7% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2741012.5 | 509715.6 | 0.19x | 522309.4 | 0.19x | +2.5% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2716121.9 | 514303.1 | 0.19x | 516737.5 | 0.19x | +0.5% | 422403 | 422403 |
| tsql/script400 | generated | scriptdom | 2768840.6 | 532750.0 | 0.19x | 497746.9 | 0.18x | -6.6% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1777818.8 | 200407.8 | 0.11x | 188203.1 | 0.11x | -6.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 917853.9 | 364975.8 | 0.40x | 355661.7 | 0.39x | -2.6% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 606839.1 | 264228.9 | 0.44x | 261835.9 | 0.43x | -0.9% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7294.2 | 18032.8 | 2.47x | 18165.8 | 2.49x | +0.7% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7687.6 | 19497.5 | 2.54x | 19655.2 | 2.56x | +0.8% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8334.3 | 1216.5 | 0.15x | 1159.3 | 0.14x | -4.7% | 1328 | 1328 |
| sql/select20.scan | generated | control | 408.1 | 410.2 | 1.01x | 404.4 | 0.99x | -1.4% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3832.3 | 3706.6 | 0.97x | 3733.1 | 0.97x | +0.7% | 0 | 0 |
| tsql/select20.scan | generated | control | 712.9 | 684.1 | 0.96x | 663.8 | 0.93x | -3.0% | 0 | 0 |
| el/ladder.scan | generated | control | 261.1 | 264.3 | 1.01x | 275.3 | 1.05x | +4.2% | 0 | 0 |
| el/refused-early.bool | generated | hand | 600.4 | 1734.7 | 2.89x | 884.4 | 1.47x | -49.0% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1771.8 | 2493.3 | 1.41x | 1297.3 | 0.73x | -48.0% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1786.6 | 2811.8 | 1.57x | 2692.7 | 1.51x | -4.2% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 4178.1 | 17524.5 | 4.19x | 8924.3 | 2.14x | -49.1% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 12104.3 | 28002.6 | 2.31x | 29852.4 | 2.47x | +6.6% | 21448 | 21392 |
| el/floor | generated | hand | 584.9 | 1217.6 | 2.08x | 1186.5 | 2.03x | -2.6% | 1104 | 1104 |
| el/floor | immediate | hand | 584.9 | 704.4 | 1.20x | 655.5 | 1.12x | -6.9% | 1120 | 1120 |
| el/ladder | generated | hand | 1765.7 | 2537.7 | 1.44x | 2781.6 | 1.58x | +9.6% | 1776 | 1776 |
| el/ladder | immediate | hand | 1765.7 | 1866.1 | 1.06x | 1889.7 | 1.07x | +1.3% | 1784 | 1784 |
| el/nest7 | generated | hand | 942.5 | 2408.1 | 2.56x | 2761.6 | 2.93x | +14.7% | 1152 | 1152 |
| el/nest7 | immediate | hand | 942.5 | 1599.7 | 1.70x | 1537.9 | 1.63x | -3.9% | 1120 | 1120 |
| el/block | generated | hand | 1130.0 | 1896.7 | 1.68x | 1923.0 | 1.70x | +1.4% | 2344 | 2344 |
| el/block | immediate | hand | 1130.0 | 1250.7 | 1.11x | 1258.2 | 1.11x | +0.6% | 2440 | 2440 |
| el/try | generated | hand | 3480.9 | 4936.5 | 1.42x | 5046.6 | 1.45x | +2.2% | 4456 | 4456 |
| el/try | immediate | hand | 3480.9 | 3448.2 | 0.99x | 3477.3 | 1.00x | +0.8% | 4152 | 4152 |
| el/loop | generated | hand | 2465.8 | 3893.4 | 1.58x | 4008.7 | 1.63x | +3.0% | 4776 | 4776 |
| el/loop | immediate | hand | 2465.8 | 2750.2 | 1.12x | 2730.9 | 1.11x | -0.7% | 4880 | 4880 |
| el/terms100 | generated | hand | 12306.3 | 17504.0 | 1.42x | 16941.5 | 1.38x | -3.2% | 17904 | 17904 |
| el/terms100 | immediate | hand | 12306.3 | 13777.0 | 1.12x | 13287.4 | 1.08x | -3.6% | 18720 | 18720 |
| el/terms1000 | generated | hand | 114547.3 | 160025.9 | 1.40x | 158106.4 | 1.38x | -1.2% | 169104 | 169107 |
| el/terms1000 | immediate | hand | 114547.3 | 128683.1 | 1.12x | 123903.4 | 1.08x | -3.7% | 177120 | 177120 |
| el/overloads | generated | hand | 1949.0 | 2709.9 | 1.39x | 2747.0 | 1.41x | +1.4% | 4248 | 4248 |
| el/overloads | immediate | hand | 1949.0 | 2212.1 | 1.13x | 2188.6 | 1.12x | -1.1% | 4200 | 4200 |
| el/string | generated | hand | 366.1 | 1339.9 | 3.66x | 1451.2 | 3.96x | +8.3% | 1056 | 1081 |
| el/string | immediate | hand | 366.1 | 864.9 | 2.36x | 860.2 | 2.35x | -0.5% | 1032 | 1032 |
| el/interpolation | generated | hand | 3271.6 | 7250.3 | 2.22x | 7289.0 | 2.23x | +0.5% | 2368 | 2368 |
| el/interpolation | immediate | hand | 3271.6 | 6492.3 | 1.98x | 5573.3 | 1.70x | -14.2% | 2424 | 2424 |
| el/untyped | generated | hand | 26026.4 | 25437.0 | 0.98x | 38667.8 | 1.49x | +52.0% | 16424 | 16424 |
| el/refused-early | generated | hand | 693.6 | 1723.4 | 2.48x | 1829.7 | 2.64x | +6.2% | 1064 | 1064 |
| el/refused-early | immediate | hand | 693.6 | 1831.7 | 2.64x | 2018.1 | 2.91x | +10.2% | 1944 | 1944 |
| el/refused-late | generated | hand | 2432.3 | 2870.8 | 1.18x | 2643.9 | 1.09x | -7.9% | 1064 | 1064 |
| el/refused-late | immediate | hand | 2432.3 | 4823.9 | 1.98x | 4867.4 | 2.00x | +0.9% | 3928 | 3928 |
| sql/literal | generated | hand | 105.2 | 227.4 | 2.16x | 196.0 | 1.86x | -13.8% | 160 | 160 |
| sql/comment | generated | hand | 4889.9 | 7797.1 | 1.59x | 7887.7 | 1.61x | +1.2% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 108812.7 | 205893.4 | 1.89x | 206683.9 | 1.90x | +0.4% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 636897.7 | 1328801.6 | 2.09x | 1365012.5 | 2.14x | +2.7% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 23423.0 | 1615.4 | 0.07x | 1632.3 | 0.07x | +1.0% | 1192 | 1192 |
| sql/column | generated | hand | 169.6 | 377.3 | 2.22x | 374.8 | 2.21x | -0.7% | 392 | 392 |
| sql/arithmetic | generated | hand | 1915.0 | 4494.3 | 2.35x | 4553.3 | 2.38x | +1.3% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3238.4 | 12059.8 | 3.72x | 12788.3 | 3.95x | +6.0% | 6504 | 6504 |
| sql/condition | generated | hand | 2179.7 | 4626.8 | 2.12x | 4625.4 | 2.12x | 0.0% | 4928 | 4928 |
| sql/select1 | generated | hand | 789.0 | 1631.2 | 2.07x | 1657.3 | 2.10x | +1.6% | 1688 | 1688 |
| sql/select20 | generated | hand | 9091.4 | 20004.6 | 2.20x | 19979.5 | 2.20x | -0.1% | 21448 | 21448 |
| sql/values | generated | hand | 574.4 | 1848.2 | 3.22x | 1967.4 | 3.43x | +6.4% | 1904 | 1904 |
| sql/create | generated | hand | 903.5 | 2747.5 | 3.04x | 2774.3 | 3.07x | +1.0% | 1568 | 1568 |
| sql/refused-late | generated | hand | 4357.1 | 15907.6 | 3.65x | 16125.5 | 3.70x | +1.4% | 13552 | 13552 |
