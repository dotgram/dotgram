# Paired stand, 2026-09-20 00:28

IGOR-DESKTOP, pinned to 0-15, high priority, control 36.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1445096.9 | 147265.6 | 0.10x | 164603.1 | 0.11x | +11.8% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 704448.4 | 133668.0 | 0.19x | 128273.4 | 0.18x | -4.0% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 689490.6 | 127948.4 | 0.19x | 129096.9 | 0.19x | +0.9% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2817806.2 | 511887.5 | 0.18x | 519390.6 | 0.18x | +1.5% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2813443.8 | 519796.9 | 0.18x | 527037.5 | 0.19x | +1.4% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2841212.5 | 521150.0 | 0.18x | 529778.1 | 0.19x | +1.7% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1882859.4 | 202900.0 | 0.11x | 196650.0 | 0.10x | -3.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 931758.6 | 356773.4 | 0.38x | 351238.3 | 0.38x | -1.6% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 595468.0 | 254035.2 | 0.43x | 252248.4 | 0.42x | -0.7% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 734738.3 | 1679884.4 | 2.29x | 1339266.4 | 1.82x | -20.3% | 1599240 | 1599240 |
| web/sf.list10000 | generated | control | 1202275.0 | 1218168.8 | 1.01x | 1251512.5 | 1.04x | +2.7% | 2720056 | 2720056 |
| sql/select20.at | generated | hand | 7935.8 | 18439.8 | 2.32x | 18640.0 | 2.35x | +1.1% | 21441 | 21441 |
| sql/select20.window | generated | hand | 7746.8 | 19185.4 | 2.48x | 19798.4 | 2.56x | +3.2% | 21440 | 21440 |
| tsql/insert-values.at | generated | scriptdom | 8547.8 | 1224.6 | 0.14x | 1255.7 | 0.15x | +2.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 382.3 | 390.5 | 1.02x | 391.4 | 1.02x | +0.2% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3566.5 | 3599.4 | 1.01x | 3648.4 | 1.02x | +1.4% | 0 | 0 |
| tsql/select20.scan | generated | control | 390.9 | 388.1 | 0.99x | 393.6 | 1.01x | +1.4% | 0 | 0 |
| el/ladder.scan | generated | control | 142.3 | 151.2 | 1.06x | 141.2 | 0.99x | -6.6% | 0 | 0 |
| el/refused-early.bool | generated | hand | 373.5 | 1189.8 | 3.19x | 654.6 | 1.75x | -45.0% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1197.1 | 1776.6 | 1.48x | 973.9 | 0.81x | -45.2% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1019.1 | 1798.0 | 1.76x | 1781.4 | 1.75x | -0.9% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2677.8 | 12341.9 | 4.61x | 5989.2 | 2.24x | -51.5% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7315.9 | 18087.1 | 2.47x | 18375.6 | 2.51x | +1.6% | 21448 | 21392 |
| web/url.full | generated | hand | 155.6 | 292.0 | 1.88x | 301.5 | 1.94x | +3.3% | 536 | 536 |
| web/json.object | generated | hand | 576.6 | 956.1 | 1.66x | 956.1 | 1.66x | 0.0% | 2584 | 2584 |
| web/media-type.plain | generated | control | 160.9 | 196.4 | 1.22x | 208.4 | 1.30x | +6.1% | 448 | 448 |
| web/sf.list | generated | control | 1213.8 | 1307.9 | 1.08x | 1305.6 | 1.08x | -0.2% | 3064 | 3064 |
| el/floor | generated | hand | 349.5 | 844.4 | 2.42x | 884.4 | 2.53x | +4.7% | 1104 | 1104 |
| el/floor | immediate | hand | 349.5 | 532.1 | 1.52x | 523.0 | 1.50x | -1.7% | 1120 | 1120 |
| el/ladder | generated | hand | 1065.0 | 1823.0 | 1.71x | 1873.9 | 1.76x | +2.8% | 1776 | 1776 |
| el/ladder | immediate | hand | 1065.0 | 1243.9 | 1.17x | 1241.8 | 1.17x | -0.2% | 1784 | 1784 |
| el/nest7 | generated | hand | 661.6 | 1784.4 | 2.70x | 1785.1 | 2.70x | 0.0% | 1152 | 1176 |
| el/nest7 | immediate | hand | 661.6 | 1109.3 | 1.68x | 1079.4 | 1.63x | -2.7% | 1120 | 1120 |
| el/block | generated | hand | 1093.4 | 1864.6 | 1.71x | 1882.0 | 1.72x | +0.9% | 2344 | 2344 |
| el/block | immediate | hand | 1093.4 | 1204.2 | 1.10x | 1174.9 | 1.07x | -2.4% | 2440 | 2440 |
| el/try | generated | hand | 3264.7 | 6263.0 | 1.92x | 6289.5 | 1.93x | +0.4% | 5632 | 5632 |
| el/try | immediate | hand | 3264.7 | 4792.3 | 1.47x | 4752.4 | 1.46x | -0.8% | 5752 | 5752 |
| el/loop | generated | hand | 2210.1 | 3791.4 | 1.72x | 3780.9 | 1.71x | -0.3% | 4776 | 4776 |
| el/loop | immediate | hand | 2210.1 | 2545.3 | 1.15x | 2560.0 | 1.16x | +0.6% | 4880 | 4880 |
| el/terms100 | generated | hand | 11305.6 | 16107.3 | 1.42x | 16328.6 | 1.44x | +1.4% | 17904 | 17904 |
| el/terms100 | immediate | hand | 11305.6 | 12682.9 | 1.12x | 12718.4 | 1.12x | +0.3% | 18720 | 18720 |
| el/terms1000 | generated | hand | 111885.7 | 153395.7 | 1.37x | 153805.5 | 1.37x | +0.3% | 169104 | 169131 |
| el/terms1000 | immediate | hand | 111885.7 | 121568.3 | 1.09x | 125314.9 | 1.12x | +3.1% | 177120 | 177120 |
| el/overloads | generated | hand | 1886.7 | 2601.4 | 1.38x | 2654.7 | 1.41x | +2.0% | 4248 | 4248 |
| el/overloads | immediate | hand | 1886.7 | 2104.2 | 1.12x | 2106.5 | 1.12x | +0.1% | 4200 | 4200 |
| el/string | generated | hand | 278.3 | 962.4 | 3.46x | 973.2 | 3.50x | +1.1% | 1056 | 1056 |
| el/string | immediate | hand | 278.3 | 677.5 | 2.43x | 673.0 | 2.42x | -0.7% | 1032 | 1032 |
| el/interpolation | generated | hand | 1552.2 | 4620.0 | 2.98x | 4675.6 | 3.01x | +1.2% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1552.2 | 4126.0 | 2.66x | 3944.7 | 2.54x | -4.4% | 2424 | 2424 |
| el/untyped | generated | hand | 27319.0 | 34620.6 | 1.27x | 28560.5 | 1.05x | -17.5% | 16424 | 16424 |
| el/refused-early | generated | hand | 448.8 | 1256.5 | 2.80x | 1354.0 | 3.02x | +7.8% | 1064 | 1064 |
| el/refused-early | immediate | hand | 448.8 | 1215.1 | 2.71x | 1253.6 | 2.79x | +3.2% | 1944 | 1944 |
| el/refused-late | generated | hand | 1471.9 | 1867.8 | 1.27x | 2111.8 | 1.43x | +13.1% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1471.9 | 3141.6 | 2.13x | 3178.8 | 2.16x | +1.2% | 3928 | 3928 |
| sql/literal | generated | hand | 65.1 | 149.4 | 2.29x | 160.0 | 2.46x | +7.1% | 160 | 160 |
| sql/comment | generated | hand | 2725.4 | 5153.0 | 1.89x | 5222.7 | 1.92x | +1.4% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 68143.8 | 138955.4 | 2.04x | 138773.1 | 2.04x | -0.1% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 728043.8 | 1409543.8 | 1.94x | 1403193.8 | 1.93x | -0.5% | 1616136 | 1616203 |
| tsql/comment | generated | scriptdom | 24841.9 | 1620.9 | 0.07x | 1669.5 | 0.07x | +3.0% | 1192 | 1192 |
| sql/column | generated | hand | 186.0 | 388.4 | 2.09x | 390.3 | 2.10x | +0.5% | 392 | 416 |
| sql/arithmetic | generated | hand | 2084.6 | 4636.2 | 2.22x | 4829.8 | 2.32x | +4.2% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3411.6 | 12594.7 | 3.69x | 13070.9 | 3.83x | +3.8% | 6504 | 6504 |
| sql/condition | generated | hand | 2414.7 | 4794.0 | 1.99x | 4894.6 | 2.03x | +2.1% | 4928 | 4928 |
| sql/select1 | generated | hand | 827.3 | 1698.6 | 2.05x | 1749.9 | 2.12x | +3.0% | 1688 | 1688 |
| sql/select20 | generated | hand | 9865.0 | 21731.2 | 2.20x | 20765.0 | 2.10x | -4.4% | 21448 | 21448 |
| sql/values | generated | hand | 636.5 | 2001.3 | 3.14x | 2029.9 | 3.19x | +1.4% | 1904 | 1904 |
| sql/create | generated | hand | 956.1 | 2911.0 | 3.04x | 2778.2 | 2.91x | -4.6% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3610.2 | 14002.3 | 3.88x | 14029.3 | 3.89x | +0.2% | 13552 | 13552 |
