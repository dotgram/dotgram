# Paired stand, 2026-09-20 00:24

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1197715.6 | 156528.1 | 0.13x | 156828.1 | 0.13x | +0.2% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 676444.5 | 129683.6 | 0.19x | 128671.9 | 0.19x | -0.8% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 683675.8 | 127944.5 | 0.19x | 127327.3 | 0.19x | -0.5% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2730925.0 | 508771.9 | 0.19x | 502568.8 | 0.18x | -1.2% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2718640.6 | 507453.1 | 0.19x | 504559.4 | 0.19x | -0.6% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2711375.0 | 505246.9 | 0.19x | 487578.1 | 0.18x | -3.5% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1755628.1 | 192054.7 | 0.11x | 186912.5 | 0.11x | -2.7% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 910352.3 | 336535.9 | 0.37x | 338549.2 | 0.37x | +0.6% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 592585.2 | 245195.3 | 0.41x | 242103.9 | 0.41x | -1.3% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 975429.7 | 899715.6 | 0.92x | 1532628.9 | 1.57x | +70.3% | 1599240 | 1599240 |
| web/sf.list10000 | generated | control | 1357706.2 | 1335112.5 | 0.98x | 1193465.6 | 0.88x | -10.6% | 2720056 | 2720056 |
| sql/select20.at | generated | hand | 7747.6 | 18126.4 | 2.34x | 18304.3 | 2.36x | +1.0% | 21441 | 21441 |
| sql/select20.window | generated | hand | 7502.6 | 18942.2 | 2.52x | 19110.9 | 2.55x | +0.9% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8642.5 | 1218.9 | 0.14x | 1224.6 | 0.14x | +0.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 383.8 | 402.5 | 1.05x | 380.2 | 0.99x | -5.5% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3529.8 | 3592.3 | 1.02x | 3560.9 | 1.01x | -0.9% | 0 | 0 |
| tsql/select20.scan | generated | control | 378.3 | 395.4 | 1.05x | 386.0 | 1.02x | -2.4% | 0 | 0 |
| el/ladder.scan | generated | control | 140.3 | 138.5 | 0.99x | 139.6 | 1.00x | +0.8% | 0 | 0 |
| el/refused-early.bool | generated | hand | 370.5 | 1165.3 | 3.15x | 592.7 | 1.60x | -49.1% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1315.9 | 1884.1 | 1.43x | 949.4 | 0.72x | -49.6% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1069.9 | 1858.5 | 1.74x | 1810.2 | 1.69x | -2.6% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2721.2 | 12542.6 | 4.61x | 6134.0 | 2.25x | -51.1% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7405.2 | 18158.9 | 2.45x | 18538.9 | 2.50x | +2.1% | 21448 | 21392 |
| web/url.full | generated | hand | 156.2 | 270.3 | 1.73x | 282.3 | 1.81x | +4.4% | 536 | 536 |
| web/json.object | generated | hand | 594.6 | 934.0 | 1.57x | 969.5 | 1.63x | +3.8% | 2584 | 2584 |
| web/media-type.plain | generated | control | 155.9 | 182.4 | 1.17x | 198.2 | 1.27x | +8.7% | 448 | 448 |
| web/sf.list | generated | control | 1170.0 | 1235.5 | 1.06x | 1251.6 | 1.07x | +1.3% | 3064 | 3064 |
| el/floor | generated | hand | 324.8 | 837.7 | 2.58x | 847.5 | 2.61x | +1.2% | 1104 | 1104 |
| el/floor | immediate | hand | 324.8 | 489.8 | 1.51x | 498.3 | 1.53x | +1.8% | 1120 | 1120 |
| el/ladder | generated | hand | 1052.7 | 1816.7 | 1.73x | 1824.9 | 1.73x | +0.5% | 1776 | 1776 |
| el/ladder | immediate | hand | 1052.7 | 1242.1 | 1.18x | 1269.0 | 1.21x | +2.2% | 1784 | 1784 |
| el/nest7 | generated | hand | 758.1 | 1837.1 | 2.42x | 1797.3 | 2.37x | -2.2% | 1152 | 1152 |
| el/nest7 | immediate | hand | 758.1 | 1099.0 | 1.45x | 1106.7 | 1.46x | +0.7% | 1120 | 1120 |
| el/block | generated | hand | 1120.3 | 1939.1 | 1.73x | 1917.8 | 1.71x | -1.1% | 2344 | 2344 |
| el/block | immediate | hand | 1120.3 | 1267.3 | 1.13x | 1236.6 | 1.10x | -2.4% | 2440 | 2440 |
| el/try | generated | hand | 3416.7 | 6713.7 | 1.96x | 6323.2 | 1.85x | -5.8% | 5632 | 5632 |
| el/try | immediate | hand | 3416.7 | 4955.4 | 1.45x | 4842.1 | 1.42x | -2.3% | 5752 | 5752 |
| el/loop | generated | hand | 2305.0 | 3835.5 | 1.66x | 3808.1 | 1.65x | -0.7% | 4776 | 4776 |
| el/loop | immediate | hand | 2305.0 | 2603.1 | 1.13x | 2623.5 | 1.14x | +0.8% | 4880 | 4880 |
| el/terms100 | generated | hand | 11844.6 | 17046.7 | 1.44x | 17091.7 | 1.44x | +0.3% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11844.6 | 13587.5 | 1.15x | 13495.1 | 1.14x | -0.7% | 18720 | 18720 |
| el/terms1000 | generated | hand | 113452.8 | 166531.8 | 1.47x | 164502.5 | 1.45x | -1.2% | 169128 | 169128 |
| el/terms1000 | immediate | hand | 113452.8 | 133186.6 | 1.17x | 130937.0 | 1.15x | -1.7% | 177120 | 177123 |
| el/overloads | generated | hand | 2011.7 | 2779.5 | 1.38x | 2671.6 | 1.33x | -3.9% | 4248 | 4248 |
| el/overloads | immediate | hand | 2011.7 | 2211.1 | 1.10x | 2241.3 | 1.11x | +1.4% | 4200 | 4200 |
| el/string | generated | hand | 299.0 | 1022.0 | 3.42x | 1011.8 | 3.38x | -1.0% | 1056 | 1056 |
| el/string | immediate | hand | 299.0 | 715.5 | 2.39x | 710.7 | 2.38x | -0.7% | 1032 | 1032 |
| el/interpolation | generated | hand | 2016.8 | 4615.7 | 2.29x | 5373.6 | 2.66x | +16.4% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2016.8 | 4552.1 | 2.26x | 4516.2 | 2.24x | -0.8% | 2424 | 2424 |
| el/untyped | generated | hand | 27615.7 | 39411.8 | 1.43x | 29506.7 | 1.07x | -25.1% | 16424 | 16424 |
| el/refused-early | generated | hand | 462.3 | 1260.9 | 2.73x | 1318.1 | 2.85x | +4.5% | 1064 | 1064 |
| el/refused-early | immediate | hand | 462.3 | 1325.8 | 2.87x | 1311.6 | 2.84x | -1.1% | 1944 | 1944 |
| el/refused-late | generated | hand | 1717.6 | 2045.9 | 1.19x | 2157.7 | 1.26x | +5.5% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1717.6 | 3466.9 | 2.02x | 3518.3 | 2.05x | +1.5% | 3928 | 3928 |
| sql/literal | generated | hand | 68.5 | 160.6 | 2.35x | 174.7 | 2.55x | +8.8% | 160 | 160 |
| sql/comment | generated | hand | 3152.7 | 5631.4 | 1.79x | 5791.2 | 1.84x | +2.8% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 77133.4 | 146319.9 | 1.90x | 152454.2 | 1.98x | +4.2% | 161736 | 161760 |
| sql/conditions1000 | generated | hand | 852593.0 | 1566338.3 | 1.84x | 1540628.1 | 1.81x | -1.6% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 26776.3 | 1584.8 | 0.06x | 1593.5 | 0.06x | +0.5% | 1192 | 1192 |
| sql/column | generated | hand | 210.6 | 407.8 | 1.94x | 415.2 | 1.97x | +1.8% | 392 | 392 |
| sql/arithmetic | generated | hand | 2260.7 | 5018.7 | 2.22x | 5029.9 | 2.22x | +0.2% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3804.8 | 13482.8 | 3.54x | 13902.1 | 3.65x | +3.1% | 6504 | 6504 |
| sql/condition | generated | hand | 2621.6 | 5196.1 | 1.98x | 5118.6 | 1.95x | -1.5% | 4928 | 4928 |
| sql/select1 | generated | hand | 977.3 | 1792.3 | 1.83x | 1779.0 | 1.82x | -0.7% | 1688 | 1688 |
| sql/select20 | generated | hand | 10762.2 | 21271.7 | 1.98x | 21999.9 | 2.04x | +3.4% | 21448 | 21448 |
| sql/values | generated | hand | 695.8 | 1972.7 | 2.84x | 2072.6 | 2.98x | +5.1% | 1904 | 1904 |
| sql/create | generated | hand | 1051.9 | 3003.1 | 2.86x | 2961.8 | 2.82x | -1.4% | 1568 | 1568 |
| sql/refused-late | generated | hand | 4336.8 | 15242.5 | 3.51x | 15740.0 | 3.63x | +3.3% | 13552 | 13552 |
