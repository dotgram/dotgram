# Paired stand, 2026-09-20 00:41

IGOR-DESKTOP, pinned to 0-15, high priority, control 33.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1484787.5 | 171425.0 | 0.12x | 163303.1 | 0.11x | -4.7% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 662016.4 | 123611.7 | 0.19x | 123846.9 | 0.19x | +0.2% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 659331.2 | 123713.3 | 0.19x | 127480.5 | 0.19x | +3.0% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2676009.4 | 494543.8 | 0.18x | 502196.9 | 0.19x | +1.5% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2737331.2 | 507509.4 | 0.19x | 508193.8 | 0.19x | +0.1% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2665006.2 | 499028.1 | 0.19x | 504062.5 | 0.19x | +1.0% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1756060.9 | 194945.3 | 0.11x | 189559.4 | 0.11x | -2.8% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 880620.3 | 341918.0 | 0.39x | 332366.4 | 0.38x | -2.8% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 581197.7 | 246153.9 | 0.42x | 246515.6 | 0.42x | +0.1% | 296472 | 296472 |
| web/json.object10000 | generated | hand | 664415.6 | 767848.4 | 1.16x | 1268760.2 | 1.91x | +65.2% | 1599240 | 1599240 |
| web/sf.list10000 | generated | control | 1164881.2 | 1140800.0 | 0.98x | 1124050.0 | 0.96x | -1.5% | 2720056 | 2720056 |
| sql/select20.at | generated | hand | 7466.5 | 17849.8 | 2.39x | 18218.1 | 2.44x | +2.1% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8483.8 | 21016.9 | 2.48x | 21854.6 | 2.58x | +4.0% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 13509.1 | 1801.5 | 0.13x | 1599.4 | 0.12x | -11.2% | 1328 | 1328 |
| sql/select20.scan | generated | control | 710.9 | 712.2 | 1.00x | 708.1 | 1.00x | -0.6% | 0 | 0 |
| sql/conditions100.scan | generated | control | 7214.6 | 7153.4 | 0.99x | 7135.6 | 0.99x | -0.2% | 0 | 0 |
| tsql/select20.scan | generated | control | 530.1 | 577.8 | 1.09x | 589.3 | 1.11x | +2.0% | 0 | 0 |
| el/ladder.scan | generated | control | 231.8 | 231.6 | 1.00x | 243.0 | 1.05x | +4.9% | 0 | 0 |
| el/refused-early.bool | generated | hand | 608.5 | 1639.8 | 2.69x | 804.7 | 1.32x | -50.9% | 1064 | 624 |
| el/refused-late.bool | generated | hand | 2191.4 | 2824.7 | 1.29x | 1509.6 | 0.69x | -46.6% | 1064 | 624 |
| el/ladder.bool | generated | hand | 1517.7 | 2655.7 | 1.75x | 2748.7 | 1.81x | +3.5% | 1776 | 1720 |
| sql/refused-late.bool | generated | hand | 3237.0 | 16836.6 | 5.20x | 7878.7 | 2.43x | -53.2% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 12005.7 | 28915.7 | 2.41x | 28408.5 | 2.37x | -1.8% | 21473 | 21392 |
| web/url.full | generated | hand | 314.5 | 511.3 | 1.63x | 531.1 | 1.69x | +3.9% | 536 | 536 |
| web/json.object | generated | hand | 602.2 | 956.7 | 1.59x | 966.4 | 1.60x | +1.0% | 2584 | 2584 |
| web/media-type.plain | generated | control | 167.3 | 195.4 | 1.17x | 193.3 | 1.16x | -1.1% | 448 | 448 |
| web/sf.list | generated | control | 1215.7 | 1268.3 | 1.04x | 1276.1 | 1.05x | +0.6% | 3064 | 3064 |
| el/floor | generated | hand | 346.1 | 900.1 | 2.60x | 849.9 | 2.46x | -5.6% | 1104 | 1104 |
| el/floor | immediate | hand | 346.1 | 513.3 | 1.48x | 504.8 | 1.46x | -1.7% | 1120 | 1120 |
| el/ladder | generated | hand | 1081.2 | 1916.0 | 1.77x | 1838.9 | 1.70x | -4.0% | 1776 | 1776 |
| el/ladder | immediate | hand | 1081.2 | 1250.6 | 1.16x | 1252.1 | 1.16x | +0.1% | 1784 | 1784 |
| el/nest7 | generated | hand | 701.1 | 1825.3 | 2.60x | 1768.0 | 2.52x | -3.1% | 1152 | 1152 |
| el/nest7 | immediate | hand | 701.1 | 1124.4 | 1.60x | 1093.0 | 1.56x | -2.8% | 1120 | 1120 |
| el/block | generated | hand | 1118.2 | 1938.2 | 1.73x | 1963.1 | 1.76x | +1.3% | 2344 | 2344 |
| el/block | immediate | hand | 1118.2 | 1212.6 | 1.08x | 1207.4 | 1.08x | -0.4% | 2440 | 2440 |
| el/try | generated | hand | 3346.1 | 6345.5 | 1.90x | 6351.4 | 1.90x | +0.1% | 5632 | 5632 |
| el/try | immediate | hand | 3346.1 | 4857.5 | 1.45x | 4799.6 | 1.43x | -1.2% | 5752 | 5752 |
| el/loop | generated | hand | 2323.7 | 4008.0 | 1.72x | 3863.7 | 1.66x | -3.6% | 4800 | 4776 |
| el/loop | immediate | hand | 2323.7 | 2686.2 | 1.16x | 2601.1 | 1.12x | -3.2% | 4880 | 4880 |
| el/terms100 | generated | hand | 12259.5 | 18888.8 | 1.54x | 17653.4 | 1.44x | -6.5% | 17904 | 17904 |
| el/terms100 | immediate | hand | 12259.5 | 13741.1 | 1.12x | 13709.0 | 1.12x | -0.2% | 18720 | 18720 |
| el/terms1000 | generated | hand | 117805.2 | 162950.9 | 1.38x | 161048.5 | 1.37x | -1.2% | 169104 | 169104 |
| el/terms1000 | immediate | hand | 117805.2 | 131404.6 | 1.12x | 129706.1 | 1.10x | -1.3% | 177120 | 177120 |
| el/overloads | generated | hand | 1909.3 | 2647.3 | 1.39x | 2685.9 | 1.41x | +1.5% | 4248 | 4248 |
| el/overloads | immediate | hand | 1909.3 | 2037.1 | 1.07x | 2154.1 | 1.13x | +5.7% | 4200 | 4200 |
| el/string | generated | hand | 297.3 | 1057.3 | 3.56x | 985.1 | 3.31x | -6.8% | 1056 | 1056 |
| el/string | immediate | hand | 297.3 | 698.1 | 2.35x | 683.7 | 2.30x | -2.1% | 1032 | 1032 |
| el/interpolation | generated | hand | 2887.7 | 6093.2 | 2.11x | 7025.4 | 2.43x | +15.3% | 2368 | 2368 |
| el/interpolation | immediate | hand | 2887.7 | 5507.6 | 1.91x | 5936.1 | 2.06x | +7.8% | 2424 | 2424 |
| el/untyped | generated | hand | 39051.7 | 30577.9 | 0.78x | 37528.0 | 0.96x | +22.7% | 16424 | 16424 |
| el/refused-early | generated | hand | 943.1 | 2090.0 | 2.22x | 2012.7 | 2.13x | -3.7% | 1064 | 1064 |
| el/refused-early | immediate | hand | 943.1 | 2084.2 | 2.21x | 2135.9 | 2.26x | +2.5% | 1944 | 1944 |
| el/refused-late | generated | hand | 3184.0 | 2947.5 | 0.93x | 3948.3 | 1.24x | +34.0% | 1064 | 1064 |
| el/refused-late | immediate | hand | 3184.0 | 5624.0 | 1.77x | 5993.4 | 1.88x | +6.6% | 3928 | 3928 |
| sql/literal | generated | hand | 155.3 | 307.8 | 1.98x | 296.8 | 1.91x | -3.6% | 160 | 160 |
| sql/comment | generated | hand | 5825.6 | 9766.6 | 1.68x | 9677.1 | 1.66x | -0.9% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 174507.2 | 274701.2 | 1.57x | 227221.5 | 1.30x | -17.3% | 161736 | 161736 |
| sql/conditions1000 | generated | hand | 1410784.4 | 2405129.7 | 1.70x | 2213898.4 | 1.57x | -8.0% | 1616136 | 1616136 |
| tsql/comment | generated | scriptdom | 50925.0 | 2465.4 | 0.05x | 2478.6 | 0.05x | +0.5% | 1192 | 1192 |
| sql/column | generated | hand | 403.4 | 733.8 | 1.82x | 740.3 | 1.84x | +0.9% | 392 | 392 |
| sql/arithmetic | generated | hand | 4341.5 | 7425.6 | 1.71x | 7292.2 | 1.68x | -1.8% | 3472 | 3472 |
| sql/nest8 | generated | hand | 4560.2 | 13440.2 | 2.95x | 14926.6 | 3.27x | +11.1% | 6504 | 6504 |
| sql/condition | generated | hand | 2972.6 | 5211.0 | 1.75x | 5431.6 | 1.83x | +4.2% | 4928 | 4928 |
| sql/select1 | generated | hand | 1010.9 | 1917.5 | 1.90x | 1835.4 | 1.82x | -4.3% | 1688 | 1688 |
| sql/select20 | generated | hand | 11925.6 | 23033.8 | 1.93x | 23209.2 | 1.95x | +0.8% | 21448 | 21448 |
| sql/values | generated | hand | 783.6 | 2051.3 | 2.62x | 2180.3 | 2.78x | +6.3% | 1904 | 1904 |
| sql/create | generated | hand | 1170.7 | 3038.3 | 2.60x | 3065.8 | 2.62x | +0.9% | 1568 | 1568 |
| sql/refused-late | generated | hand | 5021.0 | 15185.4 | 3.02x | 14717.7 | 2.93x | -3.1% | 13552 | 13552 |
