# Paired stand, 2026-09-20 02:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 33.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1393259.4 | 154762.5 | 0.11x | 155553.1 | 0.11x | +0.5% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 670072.7 | 121489.8 | 0.18x | 126939.8 | 0.19x | +4.5% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 676124.2 | 122240.6 | 0.18x | 127412.5 | 0.19x | +4.2% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2720956.2 | 493296.9 | 0.18x | 496503.1 | 0.18x | +0.6% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2778956.2 | 489984.4 | 0.18x | 494818.8 | 0.18x | +1.0% | 422403 | 422403 |
| tsql/script400 | generated | scriptdom | 2687818.8 | 493893.8 | 0.18x | 496321.9 | 0.18x | +0.5% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1777390.6 | 190784.4 | 0.11x | 197196.9 | 0.11x | +3.4% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 919318.0 | 338513.3 | 0.37x | 341968.0 | 0.37x | +1.0% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 654321.1 | 287052.3 | 0.44x | 360400.8 | 0.55x | +25.6% | 296472 | 296472 |
| sql/select20.at | generated | hand | 12698.5 | 29564.5 | 2.33x | 29410.5 | 2.32x | -0.5% | 21416 | 21416 |
| sql/select20.window | generated | hand | 12632.8 | 29133.0 | 2.31x | 29981.5 | 2.37x | +2.9% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 14720.5 | 2032.7 | 0.14x | 2021.5 | 0.14x | -0.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 681.2 | 696.2 | 1.02x | 688.6 | 1.01x | -1.1% | 0 | 0 |
| sql/conditions100.scan | generated | control | 6144.5 | 6755.3 | 1.10x | 6241.7 | 1.02x | -7.6% | 0 | 0 |
| tsql/select20.scan | generated | control | 633.3 | 612.5 | 0.97x | 614.6 | 0.97x | +0.3% | 0 | 0 |
| el/ladder.scan | generated | control | 243.2 | 223.6 | 0.92x | 227.2 | 0.93x | +1.6% | 0 | 0 |
| el/refused-early.bool | generated | hand | 431.3 | 1538.9 | 3.57x | 766.4 | 1.78x | -50.2% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1339.9 | 1819.0 | 1.36x | 957.0 | 0.71x | -47.4% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1778.0 | 2768.9 | 1.56x | 2847.2 | 1.60x | +2.8% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 4848.7 | 18379.1 | 3.79x | 8080.9 | 1.67x | -56.0% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 12649.8 | 28051.3 | 2.22x | 30182.1 | 2.39x | +7.6% | 21448 | 21392 |
| el/floor | generated | hand | 317.0 | 869.5 | 2.74x | 826.6 | 2.61x | -4.9% | 1104 | 1104 |
| el/floor | immediate | hand | 317.0 | 484.8 | 1.53x | 485.9 | 1.53x | +0.2% | 1120 | 1120 |
| el/ladder | generated | hand | 1021.8 | 1783.5 | 1.75x | 1787.7 | 1.75x | +0.2% | 1776 | 1776 |
| el/ladder | immediate | hand | 1021.8 | 1190.3 | 1.16x | 1165.5 | 1.14x | -2.1% | 1784 | 1784 |
| el/nest7 | generated | hand | 673.8 | 1772.6 | 2.63x | 1728.3 | 2.57x | -2.5% | 1152 | 1152 |
| el/nest7 | immediate | hand | 673.8 | 1054.4 | 1.56x | 1071.2 | 1.59x | +1.6% | 1120 | 1120 |
| el/block | generated | hand | 1107.1 | 1882.1 | 1.70x | 1878.1 | 1.70x | -0.2% | 2344 | 2344 |
| el/block | immediate | hand | 1107.1 | 1188.7 | 1.07x | 1177.9 | 1.06x | -0.9% | 2440 | 2440 |
| el/try | generated | hand | 3380.5 | 4964.9 | 1.47x | 4911.9 | 1.45x | -1.1% | 4456 | 4456 |
| el/try | immediate | hand | 3380.5 | 3361.0 | 0.99x | 3349.1 | 0.99x | -0.4% | 4152 | 4152 |
| el/loop | generated | hand | 2270.2 | 3788.9 | 1.67x | 3822.7 | 1.68x | +0.9% | 4776 | 4776 |
| el/loop | immediate | hand | 2270.2 | 2623.9 | 1.16x | 2569.1 | 1.13x | -2.1% | 4880 | 4880 |
| el/terms100 | generated | hand | 11677.0 | 16708.1 | 1.43x | 16387.4 | 1.40x | -1.9% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11677.0 | 12766.6 | 1.09x | 12806.7 | 1.10x | +0.3% | 18720 | 18720 |
| el/terms1000 | generated | hand | 112470.2 | 161092.5 | 1.43x | 156619.4 | 1.39x | -2.8% | 169107 | 169104 |
| el/terms1000 | immediate | hand | 112470.2 | 122595.6 | 1.09x | 123612.0 | 1.10x | +0.8% | 177120 | 177120 |
| el/overloads | generated | hand | 1902.0 | 2681.2 | 1.41x | 2675.7 | 1.41x | -0.2% | 4248 | 4248 |
| el/overloads | immediate | hand | 1902.0 | 2095.3 | 1.10x | 2125.5 | 1.12x | +1.4% | 4200 | 4200 |
| el/string | generated | hand | 279.8 | 1015.4 | 3.63x | 1043.6 | 3.73x | +2.8% | 1056 | 1056 |
| el/string | immediate | hand | 279.8 | 687.0 | 2.46x | 712.8 | 2.55x | +3.8% | 1032 | 1032 |
| el/interpolation | generated | hand | 1911.8 | 4756.6 | 2.49x | 4918.2 | 2.57x | +3.4% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1911.8 | 3811.4 | 1.99x | 4063.8 | 2.13x | +6.6% | 2424 | 2424 |
| el/untyped | generated | hand | 25392.6 | 20440.0 | 0.80x | 20155.8 | 0.79x | -1.4% | 16424 | 16424 |
| el/refused-early | generated | hand | 466.3 | 1218.8 | 2.61x | 1265.4 | 2.71x | +3.8% | 1064 | 1064 |
| el/refused-early | immediate | hand | 466.3 | 1225.7 | 2.63x | 1214.8 | 2.61x | -0.9% | 1944 | 1944 |
| el/refused-late | generated | hand | 1475.3 | 1857.9 | 1.26x | 2044.3 | 1.39x | +10.0% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1475.3 | 3149.7 | 2.13x | 3230.1 | 2.19x | +2.6% | 3928 | 3928 |
| sql/literal | generated | hand | 69.5 | 158.7 | 2.29x | 167.4 | 2.41x | +5.5% | 160 | 160 |
| sql/comment | generated | hand | 4186.3 | 8081.7 | 1.93x | 7543.7 | 1.80x | -6.7% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 104063.1 | 210789.1 | 2.03x | 163670.6 | 1.57x | -22.4% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 957454.7 | 1871718.8 | 1.95x | 1874009.4 | 1.96x | +0.1% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 43563.8 | 2610.8 | 0.06x | 2495.0 | 0.06x | -4.4% | 1192 | 1192 |
| sql/column | generated | hand | 282.9 | 543.1 | 1.92x | 543.6 | 1.92x | +0.1% | 392 | 392 |
| sql/arithmetic | generated | hand | 3243.2 | 6362.1 | 1.96x | 6447.2 | 1.99x | +1.3% | 3472 | 3472 |
| sql/nest8 | generated | hand | 5613.6 | 16513.5 | 2.94x | 16780.9 | 2.99x | +1.6% | 6529 | 6529 |
| sql/condition | generated | hand | 4228.7 | 8089.8 | 1.91x | 7764.2 | 1.84x | -4.0% | 4928 | 4928 |
| sql/select1 | generated | hand | 1410.4 | 2720.5 | 1.93x | 2809.6 | 1.99x | +3.3% | 1688 | 1688 |
| sql/select20 | generated | hand | 14186.5 | 33221.5 | 2.34x | 31711.8 | 2.24x | -4.5% | 21448 | 21448 |
| sql/values | generated | hand | 1066.6 | 3020.4 | 2.83x | 3163.9 | 2.97x | +4.7% | 1904 | 1904 |
| sql/create | generated | hand | 1765.9 | 4091.4 | 2.32x | 3663.5 | 2.07x | -10.5% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3493.2 | 14231.0 | 4.07x | 14079.0 | 4.03x | -1.1% | 13552 | 13552 |
