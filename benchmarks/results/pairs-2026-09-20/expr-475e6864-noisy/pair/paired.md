Median of 4 of 5 runs, each in a process of its own; control 31.9 ns (the runs' controls: 32.0, 36.3, 31.8, 31.7, 33.0).
Dropped for a control more than 5% off the median: run 2.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 00:41

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.9 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1376950.0 | 163085.9 | 0.12x | 160065.6 | 0.12x | -1.9% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 678598.4 | 126647.7 | 0.19x | 127999.6 | 0.19x | +1.1% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 689278.9 | 125632.4 | 0.18x | 127860.2 | 0.19x | +1.8% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2714285.9 | 496842.2 | 0.18x | 502382.8 | 0.19x | +1.1% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2727985.9 | 503746.9 | 0.18x | 506376.6 | 0.19x | +0.5% | 422403 | 422402 |
| tsql/script400 | generated | scriptdom | 2707737.5 | 503350.0 | 0.19x | 510939.1 | 0.19x | +1.5% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1761532.0 | 193818.0 | 0.11x | 189686.7 | 0.11x | -2.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 893507.8 | 340519.9 | 0.38x | 337935.9 | 0.38x | -0.8% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 590095.3 | 247339.1 | 0.42x | 244772.3 | 0.41x | -1.0% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 816624.6 | 958336.7 | 1.17x | 1485876.2 | 1.82x | +55.0% | 1599240 | 1599240 |
| web/sf.list10000 | generated | control | 1259437.5 | 1237956.2 | 0.98x | 1158757.8 | 0.92x | -6.4% | 2720056 | 2720056 |
| sql/select20.at | generated | hand | 7755.2 | 18179.4 | 2.34x | 18261.2 | 2.35x | +0.4% | 21441 | 21441 |
| sql/select20.window | generated | hand | 7815.2 | 19116.8 | 2.45x | 19437.4 | 2.49x | +1.7% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8725.6 | 1200.0 | 0.14x | 1215.2 | 0.14x | +1.3% | 1328 | 1328 |
| sql/select20.scan | generated | control | 394.5 | 396.7 | 1.01x | 385.4 | 0.98x | -2.8% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3560.0 | 3604.0 | 1.01x | 3582.7 | 1.01x | -0.6% | 0 | 0 |
| tsql/select20.scan | generated | control | 381.5 | 390.2 | 1.02x | 383.3 | 1.00x | -1.8% | 0 | 0 |
| el/ladder.scan | generated | control | 141.0 | 139.4 | 0.99x | 139.3 | 0.99x | -0.1% | 0 | 0 |
| el/refused-early.bool | generated | hand | 383.7 | 1185.9 | 3.09x | 588.2 | 1.53x | -50.4% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1258.8 | 1856.7 | 1.47x | 945.0 | 0.75x | -49.1% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1086.8 | 1860.3 | 1.71x | 1835.2 | 1.69x | -1.4% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 3002.3 | 13050.4 | 4.35x | 6467.1 | 2.15x | -50.4% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7697.1 | 18788.4 | 2.44x | 19033.8 | 2.47x | +1.3% | 21448 | 21392 |
| web/url.full | generated | hand | 158.1 | 275.5 | 1.74x | 292.3 | 1.85x | +6.1% | 536 | 536 |
| web/json.object | generated | hand | 593.4 | 946.0 | 1.59x | 957.9 | 1.61x | +1.2% | 2584 | 2584 |
| web/media-type.plain | generated | control | 159.2 | 192.3 | 1.21x | 195.8 | 1.23x | +1.8% | 448 | 448 |
| web/sf.list | generated | control | 1201.5 | 1252.2 | 1.04x | 1260.1 | 1.05x | +0.6% | 3064 | 3064 |
| el/floor | generated | hand | 328.7 | 827.6 | 2.52x | 833.0 | 2.53x | +0.6% | 1104 | 1104 |
| el/floor | immediate | hand | 328.7 | 479.4 | 1.46x | 488.8 | 1.49x | +2.0% | 1120 | 1120 |
| el/ladder | generated | hand | 1059.8 | 1837.8 | 1.73x | 1829.3 | 1.73x | -0.5% | 1776 | 1776 |
| el/ladder | immediate | hand | 1059.8 | 1228.8 | 1.16x | 1254.3 | 1.18x | +2.1% | 1784 | 1784 |
| el/nest7 | generated | hand | 722.8 | 1778.9 | 2.46x | 1762.0 | 2.44x | -0.9% | 1152 | 1152 |
| el/nest7 | immediate | hand | 722.8 | 1087.7 | 1.50x | 1087.6 | 1.50x | 0.0% | 1120 | 1120 |
| el/block | generated | hand | 1119.2 | 1938.9 | 1.73x | 1940.5 | 1.73x | +0.1% | 2344 | 2344 |
| el/block | immediate | hand | 1119.2 | 1240.0 | 1.11x | 1222.0 | 1.09x | -1.5% | 2440 | 2440 |
| el/try | generated | hand | 3381.4 | 6529.6 | 1.93x | 6361.3 | 1.88x | -2.6% | 5632 | 5632 |
| el/try | immediate | hand | 3381.4 | 4906.5 | 1.45x | 4837.3 | 1.43x | -1.4% | 5752 | 5752 |
| el/loop | generated | hand | 2317.6 | 3944.6 | 1.70x | 3897.8 | 1.68x | -1.2% | 4776 | 4776 |
| el/loop | immediate | hand | 2317.6 | 2644.7 | 1.14x | 2612.3 | 1.13x | -1.2% | 4880 | 4880 |
| el/terms100 | generated | hand | 12219.0 | 18062.5 | 1.48x | 17508.9 | 1.43x | -3.1% | 17904 | 17904 |
| el/terms100 | immediate | hand | 12219.0 | 13664.3 | 1.12x | 13662.0 | 1.12x | 0.0% | 18720 | 18720 |
| el/terms1000 | generated | hand | 116165.3 | 164741.4 | 1.42x | 162775.5 | 1.40x | -1.2% | 169116 | 169117 |
| el/terms1000 | immediate | hand | 116165.3 | 132295.6 | 1.14x | 130321.5 | 1.12x | -1.5% | 177120 | 177121 |
| el/overloads | generated | hand | 1967.3 | 2713.4 | 1.38x | 2678.8 | 1.36x | -1.3% | 4248 | 4248 |
| el/overloads | immediate | hand | 1967.3 | 2130.6 | 1.08x | 2197.7 | 1.12x | +3.1% | 4200 | 4200 |
| el/string | generated | hand | 298.1 | 1039.6 | 3.49x | 998.4 | 3.35x | -4.0% | 1056 | 1056 |
| el/string | immediate | hand | 298.1 | 712.6 | 2.39x | 701.8 | 2.35x | -1.5% | 1032 | 1032 |
| el/interpolation | generated | hand | 2608.8 | 5825.6 | 2.23x | 5801.2 | 2.22x | -0.4% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2608.8 | 5229.8 | 2.00x | 5640.5 | 2.16x | +7.9% | 2424 | 2424 |
| el/untyped | generated | hand | 35910.2 | 34226.5 | 0.95x | 35062.9 | 0.98x | +2.4% | 16424 | 16424 |
| el/refused-early | generated | hand | 523.1 | 1324.3 | 2.53x | 1338.0 | 2.56x | +1.0% | 1064 | 1064 |
| el/refused-early | immediate | hand | 523.1 | 1357.3 | 2.59x | 1352.7 | 2.59x | -0.3% | 1944 | 1944 |
| el/refused-late | generated | hand | 1750.4 | 2008.3 | 1.15x | 2122.7 | 1.21x | +5.7% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1750.4 | 3546.3 | 2.03x | 3483.8 | 1.99x | -1.8% | 3928 | 3928 |
| sql/literal | generated | hand | 68.9 | 165.1 | 2.40x | 166.7 | 2.42x | +1.0% | 160 | 160 |
| sql/comment | generated | hand | 3231.6 | 5668.8 | 1.75x | 5824.9 | 1.80x | +2.8% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 83335.4 | 159224.9 | 1.91x | 157828.9 | 1.89x | -0.9% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 911501.2 | 1635838.7 | 1.79x | 1571856.2 | 1.72x | -3.9% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 28353.1 | 1599.7 | 0.06x | 1644.0 | 0.06x | +2.8% | 1192 | 1192 |
| sql/column | generated | hand | 217.4 | 434.3 | 2.00x | 432.9 | 1.99x | -0.3% | 392 | 392 |
| sql/arithmetic | generated | hand | 2397.5 | 5043.8 | 2.10x | 4945.3 | 2.06x | -2.0% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3897.5 | 13286.0 | 3.41x | 13561.8 | 3.48x | +2.1% | 6504 | 6504 |
| sql/condition | generated | hand | 2795.4 | 5203.5 | 1.86x | 5275.1 | 1.89x | +1.4% | 4928 | 4928 |
| sql/select1 | generated | hand | 978.6 | 1815.4 | 1.86x | 1807.2 | 1.85x | -0.5% | 1688 | 1688 |
| sql/select20 | generated | hand | 11343.9 | 22152.8 | 1.95x | 22604.6 | 1.99x | +2.0% | 21448 | 21448 |
| sql/values | generated | hand | 729.7 | 2012.0 | 2.76x | 2051.7 | 2.81x | +2.0% | 1904 | 1904 |
| sql/create | generated | hand | 1070.5 | 3004.1 | 2.81x | 2951.2 | 2.76x | -1.8% | 1568 | 1568 |
| sql/refused-late | generated | hand | 4235.1 | 14829.2 | 3.50x | 14711.2 | 3.47x | -0.8% | 13552 | 13552 |
