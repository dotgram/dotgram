# Paired stand, 2026-09-20 00:32

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1479450.0 | 127859.4 | 0.09x | 137750.0 | 0.09x | +7.7% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 680752.3 | 122427.3 | 0.18x | 127327.3 | 0.19x | +4.0% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 694882.0 | 124161.7 | 0.18x | 128239.8 | 0.18x | +3.3% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2777256.2 | 495571.9 | 0.18x | 499743.8 | 0.18x | +0.8% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2716665.6 | 500040.6 | 0.18x | 500865.6 | 0.18x | +0.2% | 422400 | 422403 |
| tsql/script400 | generated | scriptdom | 2704100.0 | 505709.4 | 0.19x | 517815.6 | 0.19x | +2.4% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1770521.9 | 200168.8 | 0.11x | 190512.5 | 0.11x | -4.8% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 894696.9 | 344598.4 | 0.39x | 337322.7 | 0.38x | -2.1% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 587605.5 | 250283.6 | 0.43x | 246335.9 | 0.42x | -1.6% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 737189.1 | 1016957.8 | 1.38x | 1544946.9 | 2.10x | +51.9% | 1599240 | 1599240 |
| web/sf.list10000 | generated | control | 1349687.5 | 1453150.0 | 1.08x | 1266846.9 | 0.94x | -12.8% | 2720056 | 2720056 |
| sql/select20.at | generated | hand | 7783.1 | 18513.4 | 2.38x | 18830.7 | 2.42x | +1.7% | 21441 | 21441 |
| sql/select20.window | generated | hand | 8127.7 | 19291.5 | 2.37x | 19763.8 | 2.43x | +2.4% | 21440 | 21440 |
| tsql/insert-values.at | generated | scriptdom | 8808.7 | 1181.0 | 0.13x | 1172.8 | 0.13x | -0.7% | 1328 | 1328 |
| sql/select20.scan | generated | control | 375.0 | 383.6 | 1.02x | 380.8 | 1.02x | -0.7% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3570.1 | 3563.0 | 1.00x | 3565.7 | 1.00x | +0.1% | 0 | 0 |
| tsql/select20.scan | generated | control | 384.6 | 384.4 | 1.00x | 379.8 | 0.99x | -1.2% | 0 | 0 |
| el/ladder.scan | generated | control | 138.5 | 138.7 | 1.00x | 139.0 | 1.00x | +0.3% | 0 | 0 |
| el/refused-early.bool | generated | hand | 396.8 | 1206.6 | 3.04x | 568.1 | 1.43x | -52.9% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1201.8 | 1829.4 | 1.52x | 919.0 | 0.76x | -49.8% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1103.7 | 1862.1 | 1.69x | 1860.2 | 1.69x | -0.1% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 3245.8 | 13558.1 | 4.18x | 6800.2 | 2.10x | -49.8% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7923.2 | 19006.0 | 2.40x | 19286.8 | 2.43x | +1.5% | 21448 | 21392 |
| web/url.full | generated | hand | 158.1 | 280.5 | 1.77x | 302.4 | 1.91x | +7.8% | 536 | 536 |
| web/json.object | generated | hand | 592.2 | 940.3 | 1.59x | 930.9 | 1.57x | -1.0% | 2584 | 2584 |
| web/media-type.plain | generated | control | 155.0 | 189.8 | 1.22x | 184.9 | 1.19x | -2.6% | 448 | 448 |
| web/sf.list | generated | control | 1187.4 | 1236.1 | 1.04x | 1268.6 | 1.07x | +2.6% | 3064 | 3064 |
| el/floor | generated | hand | 332.6 | 817.6 | 2.46x | 818.5 | 2.46x | +0.1% | 1104 | 1104 |
| el/floor | immediate | hand | 332.6 | 469.1 | 1.41x | 474.1 | 1.43x | +1.1% | 1120 | 1120 |
| el/ladder | generated | hand | 1066.9 | 1858.8 | 1.74x | 1833.8 | 1.72x | -1.3% | 1776 | 1776 |
| el/ladder | immediate | hand | 1066.9 | 1215.4 | 1.14x | 1256.5 | 1.18x | +3.4% | 1784 | 1784 |
| el/nest7 | generated | hand | 744.5 | 1721.0 | 2.31x | 1749.1 | 2.35x | +1.6% | 1152 | 1152 |
| el/nest7 | immediate | hand | 744.5 | 1069.3 | 1.44x | 1062.5 | 1.43x | -0.6% | 1120 | 1120 |
| el/block | generated | hand | 1095.4 | 1938.7 | 1.77x | 1895.9 | 1.73x | -2.2% | 2344 | 2344 |
| el/block | immediate | hand | 1095.4 | 1203.4 | 1.10x | 1204.1 | 1.10x | +0.1% | 2440 | 2440 |
| el/try | generated | hand | 3290.5 | 6262.1 | 1.90x | 6371.2 | 1.94x | +1.7% | 5632 | 5632 |
| el/try | immediate | hand | 3290.5 | 4728.9 | 1.44x | 4832.6 | 1.47x | +2.2% | 5752 | 5752 |
| el/loop | generated | hand | 2311.5 | 3881.2 | 1.68x | 3931.8 | 1.70x | +1.3% | 4776 | 4776 |
| el/loop | immediate | hand | 2311.5 | 2587.9 | 1.12x | 2597.4 | 1.12x | +0.4% | 4880 | 4880 |
| el/terms100 | generated | hand | 12178.4 | 17236.3 | 1.42x | 17364.5 | 1.43x | +0.7% | 17904 | 17904 |
| el/terms100 | immediate | hand | 12178.4 | 13427.6 | 1.10x | 13614.9 | 1.12x | +1.4% | 18720 | 18720 |
| el/terms1000 | generated | hand | 114525.4 | 159929.8 | 1.40x | 157089.0 | 1.37x | -1.8% | 169128 | 169128 |
| el/terms1000 | immediate | hand | 114525.4 | 127010.1 | 1.11x | 125359.0 | 1.09x | -1.3% | 177120 | 177123 |
| el/overloads | generated | hand | 1923.0 | 2628.9 | 1.37x | 2619.9 | 1.36x | -0.3% | 4248 | 4248 |
| el/overloads | immediate | hand | 1923.0 | 2050.1 | 1.07x | 2046.4 | 1.06x | -0.2% | 4200 | 4200 |
| el/string | generated | hand | 292.4 | 987.2 | 3.38x | 976.5 | 3.34x | -1.1% | 1056 | 1056 |
| el/string | immediate | hand | 292.4 | 709.7 | 2.43x | 692.9 | 2.37x | -2.4% | 1032 | 1032 |
| el/interpolation | generated | hand | 2565.0 | 5558.1 | 2.17x | 5713.6 | 2.23x | +2.8% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2565.0 | 4952.1 | 1.93x | 5345.0 | 2.08x | +7.9% | 2424 | 2424 |
| el/untyped | generated | hand | 32768.8 | 28853.6 | 0.88x | 33597.5 | 1.03x | +16.4% | 16424 | 16424 |
| el/refused-early | generated | hand | 584.0 | 1387.8 | 2.38x | 1357.9 | 2.33x | -2.2% | 1064 | 1064 |
| el/refused-early | immediate | hand | 584.0 | 1388.9 | 2.38x | 1393.8 | 2.39x | +0.4% | 1944 | 1944 |
| el/refused-late | generated | hand | 1783.2 | 1970.6 | 1.11x | 2087.7 | 1.17x | +5.9% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1783.2 | 3625.7 | 2.03x | 3449.3 | 1.93x | -4.9% | 3928 | 3928 |
| sql/literal | generated | hand | 69.3 | 169.6 | 2.45x | 158.7 | 2.29x | -6.4% | 160 | 160 |
| sql/comment | generated | hand | 3310.5 | 5706.2 | 1.72x | 5858.6 | 1.77x | +2.7% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 89537.4 | 172129.8 | 1.92x | 163203.5 | 1.82x | -5.2% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 970409.4 | 1705339.1 | 1.76x | 1603084.4 | 1.65x | -6.0% | 1616136 | 1616203 |
| tsql/comment | generated | scriptdom | 29929.8 | 1614.5 | 0.05x | 1639.5 | 0.05x | +1.5% | 1192 | 1192 |
| sql/column | generated | hand | 224.2 | 460.7 | 2.05x | 450.5 | 2.01x | -2.2% | 392 | 416 |
| sql/arithmetic | generated | hand | 2534.3 | 5068.9 | 2.00x | 4860.7 | 1.92x | -4.1% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3990.3 | 13131.9 | 3.29x | 13221.4 | 3.31x | +0.7% | 6504 | 6504 |
| sql/condition | generated | hand | 2969.2 | 5469.6 | 1.84x | 5481.3 | 1.85x | +0.2% | 4928 | 4928 |
| sql/select1 | generated | hand | 980.0 | 1838.5 | 1.88x | 1892.0 | 1.93x | +2.9% | 1688 | 1688 |
| sql/select20 | generated | hand | 13621.6 | 25682.8 | 1.89x | 23657.4 | 1.74x | -7.9% | 21448 | 21448 |
| sql/values | generated | hand | 763.6 | 2097.7 | 2.75x | 2030.9 | 2.66x | -3.2% | 1904 | 1904 |
| sql/create | generated | hand | 1089.0 | 3005.1 | 2.76x | 2940.7 | 2.70x | -2.1% | 1568 | 1568 |
| sql/refused-late | generated | hand | 4133.4 | 14473.0 | 3.50x | 14704.8 | 3.56x | +1.6% | 13552 | 13552 |
