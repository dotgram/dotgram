# Paired stand, 2026-09-20 02:28

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1414287.5 | 149715.6 | 0.11x | 125225.0 | 0.09x | -16.4% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 660232.0 | 122725.0 | 0.19x | 125633.6 | 0.19x | +2.4% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 661638.3 | 124436.7 | 0.19x | 122911.7 | 0.19x | -1.2% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2645531.2 | 503940.6 | 0.19x | 493846.9 | 0.19x | -2.0% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2673621.9 | 487334.4 | 0.18x | 494962.5 | 0.19x | +1.6% | 422400 | 422403 |
| tsql/script400 | generated | scriptdom | 2659718.8 | 489353.1 | 0.18x | 499953.1 | 0.19x | +2.2% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1748120.3 | 191960.9 | 0.11x | 190837.5 | 0.11x | -0.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 891196.1 | 357498.4 | 0.40x | 346604.7 | 0.39x | -3.0% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 592449.2 | 255268.8 | 0.43x | 250392.2 | 0.42x | -1.9% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7281.0 | 18456.2 | 2.53x | 18051.5 | 2.48x | -2.2% | 21441 | 21441 |
| sql/select20.window | generated | hand | 7253.8 | 19187.2 | 2.65x | 18859.8 | 2.60x | -1.7% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8935.1 | 1203.1 | 0.13x | 1233.1 | 0.14x | +2.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 387.8 | 386.7 | 1.00x | 391.2 | 1.01x | +1.2% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3534.4 | 3592.0 | 1.02x | 3602.7 | 1.02x | +0.3% | 0 | 0 |
| tsql/select20.scan | generated | control | 388.3 | 396.8 | 1.02x | 394.8 | 1.02x | -0.5% | 0 | 0 |
| el/ladder.scan | generated | control | 138.2 | 136.9 | 0.99x | 138.2 | 1.00x | +1.0% | 0 | 0 |
| el/refused-early.bool | generated | hand | 333.8 | 1099.3 | 3.29x | 600.6 | 1.80x | -45.4% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 1221.8 | 1790.5 | 1.47x | 963.0 | 0.79x | -46.2% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1031.0 | 1797.0 | 1.74x | 1795.3 | 1.74x | -0.1% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 2746.5 | 12572.2 | 4.58x | 5812.0 | 2.12x | -53.8% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7180.7 | 18930.8 | 2.64x | 18087.7 | 2.52x | -4.5% | 21448 | 21392 |
| el/floor | generated | hand | 315.3 | 811.6 | 2.57x | 877.2 | 2.78x | +8.1% | 1104 | 1104 |
| el/floor | immediate | hand | 315.3 | 479.7 | 1.52x | 467.0 | 1.48x | -2.6% | 1120 | 1120 |
| el/ladder | generated | hand | 1024.9 | 1777.6 | 1.73x | 1809.4 | 1.77x | +1.8% | 1776 | 1776 |
| el/ladder | immediate | hand | 1024.9 | 1204.3 | 1.18x | 1171.2 | 1.14x | -2.7% | 1784 | 1784 |
| el/nest7 | generated | hand | 734.7 | 1793.8 | 2.44x | 1780.1 | 2.42x | -0.8% | 1152 | 1152 |
| el/nest7 | immediate | hand | 734.7 | 1086.7 | 1.48x | 1059.5 | 1.44x | -2.5% | 1120 | 1120 |
| el/block | generated | hand | 1150.9 | 1928.1 | 1.68x | 1973.2 | 1.71x | +2.3% | 2344 | 2344 |
| el/block | immediate | hand | 1150.9 | 1261.9 | 1.10x | 1247.7 | 1.08x | -1.1% | 2440 | 2440 |
| el/try | generated | hand | 3449.8 | 4971.4 | 1.44x | 4811.2 | 1.39x | -3.2% | 4456 | 4456 |
| el/try | immediate | hand | 3449.8 | 3492.3 | 1.01x | 3416.1 | 0.99x | -2.2% | 4152 | 4152 |
| el/loop | generated | hand | 2411.2 | 4020.4 | 1.67x | 3984.5 | 1.65x | -0.9% | 4776 | 4776 |
| el/loop | immediate | hand | 2411.2 | 2720.4 | 1.13x | 2673.8 | 1.11x | -1.7% | 4880 | 4880 |
| el/terms100 | generated | hand | 12074.4 | 16692.2 | 1.38x | 17264.0 | 1.43x | +3.4% | 17904 | 17904 |
| el/terms100 | immediate | hand | 12074.4 | 13270.2 | 1.10x | 13234.3 | 1.10x | -0.3% | 18720 | 18720 |
| el/terms1000 | generated | hand | 122315.6 | 164533.1 | 1.35x | 171856.0 | 1.41x | +4.5% | 169129 | 169129 |
| el/terms1000 | immediate | hand | 122315.6 | 129784.7 | 1.06x | 135627.9 | 1.11x | +4.5% | 177123 | 177120 |
| el/overloads | generated | hand | 1982.4 | 2764.8 | 1.39x | 2751.5 | 1.39x | -0.5% | 4248 | 4248 |
| el/overloads | immediate | hand | 1982.4 | 2129.4 | 1.07x | 2189.4 | 1.10x | +2.8% | 4200 | 4200 |
| el/string | generated | hand | 290.3 | 982.1 | 3.38x | 1032.6 | 3.56x | +5.1% | 1056 | 1056 |
| el/string | immediate | hand | 290.3 | 686.3 | 2.36x | 681.2 | 2.35x | -0.7% | 1032 | 1032 |
| el/interpolation | generated | hand | 1624.1 | 4697.7 | 2.89x | 4978.5 | 3.07x | +6.0% | 2368 | 2368 |
| el/interpolation | immediate | hand | 1624.1 | 4141.6 | 2.55x | 4252.7 | 2.62x | +2.7% | 2424 | 2424 |
| el/untyped | generated | hand | 18252.0 | 26328.3 | 1.44x | 19888.7 | 1.09x | -24.5% | 16424 | 16424 |
| el/refused-early | generated | hand | 466.3 | 1247.4 | 2.68x | 1359.6 | 2.92x | +9.0% | 1064 | 1064 |
| el/refused-early | immediate | hand | 466.3 | 1282.6 | 2.75x | 1211.1 | 2.60x | -5.6% | 1944 | 1944 |
| el/refused-late | generated | hand | 1448.3 | 1848.7 | 1.28x | 2063.7 | 1.42x | +11.6% | 1064 | 1064 |
| el/refused-late | immediate | hand | 1448.3 | 3177.6 | 2.19x | 3112.8 | 2.15x | -2.0% | 3928 | 3928 |
| sql/literal | generated | hand | 65.0 | 149.3 | 2.30x | 151.6 | 2.33x | +1.5% | 160 | 160 |
| sql/comment | generated | hand | 2771.0 | 5352.2 | 1.93x | 5242.4 | 1.89x | -2.1% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 70957.0 | 136590.0 | 1.92x | 138920.4 | 1.96x | +1.7% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 743785.2 | 1441711.7 | 1.94x | 1370804.7 | 1.84x | -4.9% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 25278.6 | 1577.3 | 0.06x | 1584.1 | 0.06x | +0.4% | 1192 | 1192 |
| sql/column | generated | hand | 192.8 | 383.1 | 1.99x | 388.0 | 2.01x | +1.3% | 392 | 392 |
| sql/arithmetic | generated | hand | 2099.5 | 4698.1 | 2.24x | 4499.9 | 2.14x | -4.2% | 3472 | 3472 |
| sql/nest8 | generated | hand | 3428.4 | 12874.8 | 3.76x | 12013.6 | 3.50x | -6.7% | 6504 | 6504 |
| sql/condition | generated | hand | 2487.4 | 4863.5 | 1.96x | 4863.1 | 1.96x | 0.0% | 4928 | 4928 |
| sql/select1 | generated | hand | 855.3 | 1694.9 | 1.98x | 1748.6 | 2.04x | +3.2% | 1688 | 1688 |
| sql/select20 | generated | hand | 9941.9 | 21460.9 | 2.16x | 20855.5 | 2.10x | -2.8% | 21448 | 21448 |
| sql/values | generated | hand | 714.3 | 2111.5 | 2.96x | 2035.5 | 2.85x | -3.6% | 1904 | 1904 |
| sql/create | generated | hand | 1037.5 | 2992.9 | 2.88x | 2917.0 | 2.81x | -2.5% | 1568 | 1568 |
| sql/refused-late | generated | hand | 3783.9 | 14132.2 | 3.73x | 14212.7 | 3.76x | +0.6% | 13552 | 13552 |
