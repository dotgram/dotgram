Median of 4 of 5 runs, each in a process of its own; control 31.6 ns (the runs' controls: 31.2, 31.5, 31.8, 33.5, 32.7).
Dropped for a control more than 5% off the median: run 4.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 02:40

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1363990.6 | 143246.9 | 0.11x | 125543.8 | 0.09x | -12.4% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 666383.2 | 123016.4 | 0.18x | 127018.4 | 0.19x | +3.3% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 673499.6 | 124059.0 | 0.18x | 126782.4 | 0.19x | +2.2% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2666087.5 | 503664.1 | 0.19x | 490270.3 | 0.18x | -2.7% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2683675.0 | 492231.2 | 0.18x | 493070.3 | 0.18x | +0.2% | 422403 | 422403 |
| tsql/script400 | generated | scriptdom | 2669778.1 | 492675.0 | 0.18x | 502296.9 | 0.19x | +2.0% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1762969.5 | 193229.7 | 0.11x | 190630.5 | 0.11x | -1.3% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 897875.4 | 349827.0 | 0.39x | 351133.2 | 0.39x | +0.4% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 609775.0 | 256124.2 | 0.42x | 262052.3 | 0.43x | +2.3% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7312.9 | 18260.6 | 2.50x | 17898.9 | 2.45x | -2.0% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7334.7 | 19218.0 | 2.62x | 18698.0 | 2.55x | -2.7% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8327.1 | 1198.5 | 0.14x | 1223.2 | 0.15x | +2.1% | 1328 | 1328 |
| sql/select20.scan | generated | control | 387.4 | 389.5 | 1.01x | 388.8 | 1.00x | -0.2% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3544.3 | 3590.4 | 1.01x | 3597.6 | 1.02x | +0.2% | 0 | 0 |
| tsql/select20.scan | generated | control | 395.2 | 394.3 | 1.00x | 395.3 | 1.00x | +0.2% | 0 | 0 |
| el/ladder.scan | generated | control | 142.3 | 139.7 | 0.98x | 139.5 | 0.98x | -0.1% | 0 | 0 |
| el/refused-early.bool | generated | hand | 340.8 | 1105.6 | 3.24x | 590.0 | 1.73x | -46.6% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1244.2 | 1830.7 | 1.47x | 977.7 | 0.79x | -46.6% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1027.0 | 1809.8 | 1.76x | 1801.5 | 1.75x | -0.5% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2746.0 | 12774.7 | 4.65x | 5997.1 | 2.18x | -53.1% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7336.3 | 19141.8 | 2.61x | 18232.3 | 2.49x | -4.8% | 21448 | 21392 |
| el/floor | generated | hand | 326.8 | 838.9 | 2.57x | 886.2 | 2.71x | +5.6% | 1104 | 1104 |
| el/floor | immediate | hand | 326.8 | 505.6 | 1.55x | 495.3 | 1.52x | -2.0% | 1120 | 1120 |
| el/ladder | generated | hand | 1036.9 | 1809.4 | 1.74x | 1842.2 | 1.78x | +1.8% | 1776 | 1776 |
| el/ladder | immediate | hand | 1036.9 | 1212.5 | 1.17x | 1195.8 | 1.15x | -1.4% | 1784 | 1784 |
| el/nest7 | generated | hand | 744.7 | 1794.9 | 2.41x | 1791.5 | 2.41x | -0.2% | 1152 | 1152 |
| el/nest7 | immediate | hand | 744.7 | 1091.4 | 1.47x | 1074.9 | 1.44x | -1.5% | 1120 | 1120 |
| el/block | generated | hand | 1102.0 | 1896.1 | 1.72x | 1917.4 | 1.74x | +1.1% | 2344 | 2344 |
| el/block | immediate | hand | 1102.0 | 1226.5 | 1.11x | 1241.1 | 1.13x | +1.2% | 2440 | 2440 |
| el/try | generated | hand | 3354.6 | 4840.7 | 1.44x | 4771.7 | 1.42x | -1.4% | 4456 | 4456 |
| el/try | immediate | hand | 3354.6 | 3379.9 | 1.01x | 3395.4 | 1.01x | +0.5% | 4152 | 4152 |
| el/loop | generated | hand | 2351.1 | 3847.4 | 1.64x | 3920.1 | 1.67x | +1.9% | 4776 | 4776 |
| el/loop | immediate | hand | 2351.1 | 2665.7 | 1.13x | 2614.0 | 1.11x | -1.9% | 4880 | 4880 |
| el/terms100 | generated | hand | 11885.3 | 16568.0 | 1.39x | 16782.9 | 1.41x | +1.3% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11885.3 | 13059.3 | 1.10x | 13260.8 | 1.12x | +1.5% | 18720 | 18720 |
| el/terms1000 | generated | hand | 114481.9 | 157695.9 | 1.38x | 158929.5 | 1.39x | +0.8% | 169104 | 169107 |
| el/terms1000 | immediate | hand | 114481.9 | 126260.5 | 1.10x | 127174.2 | 1.11x | +0.7% | 177120 | 177120 |
| el/overloads | generated | hand | 1936.0 | 2687.7 | 1.39x | 2717.5 | 1.40x | +1.1% | 4248 | 4248 |
| el/overloads | immediate | hand | 1936.0 | 2102.8 | 1.09x | 2144.9 | 1.11x | +2.0% | 4200 | 4200 |
| el/string | generated | hand | 290.2 | 1006.2 | 3.47x | 1034.0 | 3.56x | +2.8% | 1056 | 1056 |
| el/string | immediate | hand | 290.2 | 681.5 | 2.35x | 681.9 | 2.35x | +0.1% | 1032 | 1032 |
| el/interpolation | generated | hand | 2028.4 | 4923.2 | 2.43x | 4992.5 | 2.46x | +1.4% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2028.4 | 4836.8 | 2.38x | 4298.3 | 2.12x | -11.1% | 2424 | 2424 |
| el/untyped | generated | hand | 18688.8 | 25882.6 | 1.38x | 19926.2 | 1.07x | -23.0% | 16424 | 16424 |
| el/refused-early | generated | hand | 485.4 | 1263.1 | 2.60x | 1367.7 | 2.82x | +8.3% | 1064 | 1064 |
| el/refused-early | immediate | hand | 485.4 | 1284.1 | 2.65x | 1264.7 | 2.61x | -1.5% | 1944 | 1944 |
| el/refused-late | generated | hand | 1475.4 | 1981.0 | 1.34x | 1971.3 | 1.34x | -0.5% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1475.4 | 3317.1 | 2.25x | 3254.3 | 2.21x | -1.9% | 3928 | 3928 |
| sql/literal | generated | hand | 67.9 | 153.2 | 2.26x | 159.9 | 2.35x | +4.4% | 160 | 160 |
| sql/comment | generated | hand | 2919.2 | 5477.2 | 1.88x | 5469.7 | 1.87x | -0.1% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 73430.6 | 148355.4 | 2.02x | 140675.9 | 1.92x | -5.2% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 735957.8 | 1445898.4 | 1.96x | 1393880.1 | 1.89x | -3.6% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 25625.8 | 1586.4 | 0.06x | 1612.2 | 0.06x | +1.6% | 1192 | 1192 |
| sql/column | generated | hand | 187.9 | 387.4 | 2.06x | 387.0 | 2.06x | -0.1% | 392 | 392 |
| sql/arithmetic | generated | hand | 2171.2 | 4730.1 | 2.18x | 4630.1 | 2.13x | -2.1% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3495.8 | 12736.8 | 3.64x | 12522.7 | 3.58x | -1.7% | 6504 | 6504 |
| sql/condition | generated | hand | 2533.0 | 4902.6 | 1.94x | 4936.1 | 1.95x | +0.7% | 4928 | 4928 |
| sql/select1 | generated | hand | 864.1 | 1706.7 | 1.98x | 1735.2 | 2.01x | +1.7% | 1688 | 1688 |
| sql/select20 | generated | hand | 10165.1 | 21545.0 | 2.12x | 21066.0 | 2.07x | -2.2% | 21448 | 21448 |
| sql/values | generated | hand | 730.9 | 2025.6 | 2.77x | 2024.8 | 2.77x | 0.0% | 1904 | 1904 |
| sql/create | generated | hand | 1008.8 | 2855.4 | 2.83x | 2904.3 | 2.88x | +1.7% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3971.7 | 14999.6 | 3.78x | 14183.0 | 3.57x | -5.4% | 13552 | 13552 |
