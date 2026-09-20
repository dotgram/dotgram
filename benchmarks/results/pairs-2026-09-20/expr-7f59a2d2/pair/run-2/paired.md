# Paired stand, 2026-09-20 01:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit 7f59a2d2, framework net10.0, no properties, emitted 5a3915f90c8fe919). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 76.4 | 79.7 | 1.04x | 80.3 | 1.05x | +0.8% | 192 | 192 |
| fix/Order.text | generated | hand | 659.5 | 618.3 | 0.94x | 614.4 | 0.93x | -0.6% | 1096 | 1096 |
| tsql/script100.bool | generated | scriptdom | 653490.6 | 124996.9 | 0.19x | 122187.5 | 0.19x | -2.2% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 666025.0 | 124759.4 | 0.19x | 124441.4 | 0.19x | -0.3% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 667227.3 | 125285.9 | 0.19x | 127629.7 | 0.19x | +1.9% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2730903.1 | 519368.8 | 0.19x | 522718.8 | 0.19x | +0.6% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2690478.1 | 504609.4 | 0.19x | 512090.6 | 0.19x | +1.5% | 422400 | 422403 |
| tsql/script400 | generated | scriptdom | 2696256.2 | 507953.1 | 0.19x | 514578.1 | 0.19x | +1.3% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1775856.2 | 210807.8 | 0.12x | 191482.8 | 0.11x | -9.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 921440.6 | 346795.3 | 0.38x | 343868.0 | 0.37x | -0.8% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 596283.6 | 245480.5 | 0.41x | 245244.5 | 0.41x | -0.1% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 758375.8 | 1300251.6 | 1.71x | 1305500.8 | 1.72x | +0.4% | 1599240 | 1599240 |
| sql/select20.at | generated | hand | 7672.7 | 17915.5 | 2.33x | 17951.2 | 2.34x | +0.2% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7364.2 | 18124.3 | 2.46x | 18062.3 | 2.45x | -0.3% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8297.6 | 1175.1 | 0.14x | 1206.0 | 0.15x | +2.6% | 1328 | 1328 |
| sql/select20.scan | generated | control | 389.2 | 397.3 | 1.02x | 379.4 | 0.97x | -4.5% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3625.8 | 3586.9 | 0.99x | 3567.7 | 0.98x | -0.5% | 0 | 0 |
| tsql/select20.scan | generated | control | 381.4 | 397.7 | 1.04x | 389.1 | 1.02x | -2.2% | 0 | 0 |
| el/ladder.scan | generated | control | 138.2 | 144.2 | 1.04x | 142.5 | 1.03x | -1.2% | 0 | 0 |
| el/refused-early.bool | generated | hand | 344.7 | 1118.8 | 3.25x | 549.2 | 1.59x | -50.9% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1213.4 | 1763.4 | 1.45x | 900.9 | 0.74x | -48.9% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1028.9 | 1758.1 | 1.71x | 1691.8 | 1.64x | -3.8% | 1801 | 1720 |
| sql/refused-late.bool | generated | hand | 2668.5 | 12189.7 | 4.57x | 5558.5 | 2.08x | -54.4% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7133.5 | 18044.9 | 2.53x | 18076.3 | 2.53x | +0.2% | 21448 | 21392 |
| web/url.full | generated | hand | 157.2 | 280.3 | 1.78x | 277.9 | 1.77x | -0.9% | 536 | 536 |
| web/json.object | generated | hand | 580.9 | 938.1 | 1.61x | 920.1 | 1.58x | -1.9% | 2584 | 2584 |
| web/media-type.plain | generated | control | 158.1 | 190.0 | 1.20x | 196.7 | 1.24x | +3.5% | 448 | 448 |
| el/floor | generated | hand | 305.3 | 800.5 | 2.62x | 805.8 | 2.64x | +0.7% | 1104 | 1104 |
| el/floor | immediate | hand | 305.3 | 460.6 | 1.51x | 467.8 | 1.53x | +1.6% | 1120 | 1120 |
| el/ladder | generated | hand | 1022.8 | 1741.9 | 1.70x | 1756.3 | 1.72x | +0.8% | 1776 | 1776 |
| el/ladder | immediate | hand | 1022.8 | 1156.3 | 1.13x | 1148.1 | 1.12x | -0.7% | 1784 | 1784 |
| el/nest7 | generated | hand | 710.0 | 1703.6 | 2.40x | 1694.8 | 2.39x | -0.5% | 1152 | 1176 |
| el/nest7 | immediate | hand | 710.0 | 1025.3 | 1.44x | 1031.4 | 1.45x | +0.6% | 1120 | 1120 |
| el/block | generated | hand | 1060.8 | 1813.7 | 1.71x | 1803.3 | 1.70x | -0.6% | 2344 | 2344 |
| el/block | immediate | hand | 1060.8 | 1145.7 | 1.08x | 1154.1 | 1.09x | +0.7% | 2440 | 2440 |
| el/try | generated | hand | 3311.9 | 4741.8 | 1.43x | 4665.0 | 1.41x | -1.6% | 4456 | 4456 |
| el/try | immediate | hand | 3311.9 | 3268.6 | 0.99x | 3257.6 | 0.98x | -0.3% | 4152 | 4152 |
| el/loop | generated | hand | 2259.6 | 3815.8 | 1.69x | 3691.2 | 1.63x | -3.3% | 4776 | 4776 |
| el/loop | immediate | hand | 2259.6 | 2543.6 | 1.13x | 2514.6 | 1.11x | -1.1% | 4880 | 4880 |
| el/terms100 | generated | hand | 11378.7 | 16424.6 | 1.44x | 16171.7 | 1.42x | -1.5% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11378.7 | 12744.5 | 1.12x | 12979.8 | 1.14x | +1.8% | 18720 | 18720 |
| el/terms1000 | generated | hand | 110453.3 | 155298.3 | 1.41x | 153662.2 | 1.39x | -1.1% | 169128 | 169128 |
| el/terms1000 | immediate | hand | 110453.3 | 123847.0 | 1.12x | 124074.6 | 1.12x | +0.2% | 177120 | 177120 |
| el/overloads | generated | hand | 1896.6 | 2675.3 | 1.41x | 2628.1 | 1.39x | -1.8% | 4248 | 4248 |
| el/overloads | immediate | hand | 1896.6 | 2055.4 | 1.08x | 2040.7 | 1.08x | -0.7% | 4200 | 4200 |
| el/string | generated | hand | 276.2 | 961.8 | 3.48x | 947.0 | 3.43x | -1.5% | 1056 | 1056 |
| el/string | immediate | hand | 276.2 | 663.5 | 2.40x | 667.8 | 2.42x | +0.6% | 1032 | 1032 |
| el/interpolation | generated | hand | 2225.0 | 4690.5 | 2.11x | 4964.2 | 2.23x | +5.8% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2225.0 | 4189.0 | 1.88x | 4685.1 | 2.11x | +11.8% | 2424 | 2424 |
| el/untyped | generated | hand | 27165.9 | 32145.8 | 1.18x | 30183.3 | 1.11x | -6.1% | 16424 | 16424 |
| el/refused-early | generated | hand | 487.7 | 1243.8 | 2.55x | 1275.5 | 2.62x | +2.5% | 1064 | 1064 |
| el/refused-early | immediate | hand | 487.7 | 1245.4 | 2.55x | 1254.8 | 2.57x | +0.8% | 1944 | 1944 |
| el/refused-late | generated | hand | 1548.7 | 1852.3 | 1.20x | 1984.4 | 1.28x | +7.1% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1548.7 | 3237.0 | 2.09x | 3384.4 | 2.19x | +4.6% | 3928 | 3928 |
| sql/literal | generated | hand | 69.6 | 156.5 | 2.25x | 157.1 | 2.26x | +0.4% | 160 | 160 |
| sql/comment | generated | hand | 2992.0 | 5452.2 | 1.82x | 5314.0 | 1.78x | -2.5% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 76728.4 | 142745.1 | 1.86x | 136839.9 | 1.78x | -4.1% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 805141.4 | 1467378.1 | 1.82x | 1413912.5 | 1.76x | -3.6% | 1616136 | 1616203 |
| tsql/comment | generated | scriptdom | 27345.9 | 1600.4 | 0.06x | 1645.3 | 0.06x | +2.8% | 1192 | 1192 |
| sql/column | generated | hand | 190.9 | 407.8 | 2.14x | 410.1 | 2.15x | +0.6% | 392 | 416 |
| sql/arithmetic | generated | hand | 2265.3 | 4616.6 | 2.04x | 4412.0 | 1.95x | -4.4% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3716.2 | 12344.6 | 3.32x | 11080.1 | 2.98x | -10.2% | 6504 | 6504 |
| sql/condition | generated | hand | 2731.4 | 5239.6 | 1.92x | 4859.2 | 1.78x | -7.3% | 4928 | 4928 |
| sql/select1 | generated | hand | 1466.5 | 2379.8 | 1.62x | 2519.5 | 1.72x | +5.9% | 1688 | 1688 |
| sql/select20 | generated | hand | 11244.2 | 22547.2 | 2.01x | 22252.1 | 1.98x | -1.3% | 21448 | 21448 |
| sql/values | generated | hand | 706.1 | 2070.5 | 2.93x | 1995.9 | 2.83x | -3.6% | 1904 | 1904 |
| sql/create | generated | hand | 1048.2 | 2943.0 | 2.81x | 2897.9 | 2.76x | -1.5% | 1568 | 1568 |
| sql/refused-late | generated | hand | 4249.6 | 14737.4 | 3.47x | 14125.4 | 3.32x | -4.2% | 13552 | 13552 |
