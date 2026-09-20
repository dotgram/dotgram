Median of 4 of 5 runs, each in a process of its own; control 31.1 ns (the runs' controls: 35.5, 31.6, 31.1, 31.1, 31.1).
Dropped for a control more than 5% off the median: run 1.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 10:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| tsql/script100.bool | generated | scriptdom | 874556.2 | 123475.0 | 0.14x | 123832.8 | 0.14x | +0.3% | 113600 | 105600 | 28 % | [-2.6%..+1.7%], 2 of 4 positive | +1.0% [-0.9%..+15.7%], 4 of 5 positive |
| tsql/script100.boolboth | generated | scriptdom | 656246.5 | 122531.6 | 0.19x | 121502.7 | 0.19x | -0.8% | 105600 | 105600 | 2 % | [-1.7%..+1.2%], 1 of 4 positive | -1.0% [-3.0%..+0.8%], 2 of 5 positive |
| tsql/script100 | generated | scriptdom | 658039.1 | 123015.6 | 0.19x | 123006.2 | 0.19x | 0.0% | 113600 | 113600 | 7 % | [-1.7%..+3.1%], 3 of 4 positive | -0.2% [-1.3%..-0.2%], 0 of 5 positive |
| tsql/script400.bool | generated | scriptdom | 2630315.6 | 494834.4 | 0.19x | 490495.3 | 0.19x | -0.9% | 454400 | 422400 | 7 % | [-2.5%..+1.7%], 3 of 4 positive |  |
| tsql/script400.boolboth | generated | scriptdom | 2634515.6 | 491067.2 | 0.19x | 488940.6 | 0.19x | -0.4% | 422403 | 422402 | 10 % | [-5.5%..+1.7%], 1 of 4 positive |  |
| tsql/script400 | generated | scriptdom | 2645760.9 | 495545.3 | 0.19x | 495831.2 | 0.19x | +0.1% | 454400 | 454400 | 4 % | [-0.9%..+1.1%], 3 of 4 positive |  |
| tsql/columns1000 | generated | scriptdom | 1747167.2 | 190873.4 | 0.11x | 186623.4 | 0.11x | -2.2% | 200464 | 200464 | 6 % | [-3.6%..-1.1%], 0 of 4 positive | +0.3% [-0.4%..+1.7%], 3 of 5 positive |
| tsql/conditions1000 | generated | scriptdom | 879879.7 | 337024.2 | 0.38x | 337123.8 | 0.38x | 0.0% | 424424 | 424424 | 2 % | [-2.7%..+1.4%], 3 of 4 positive |  |
| tsql/rows1000 | generated | scriptdom | 580471.1 | 241870.3 | 0.42x | 243412.1 | 0.42x | +0.6% | 296472 | 296472 | 1 % | [-0.6%..+1.9%], 2 of 4 positive |  |
| sql/select20.at | generated | hand | 7236.9 | 17573.2 | 2.43x | 17610.5 | 2.43x | +0.2% | 21416 | 21416 | 1 % | [-2.0%..+1.3%], 2 of 4 positive | +1.7% [-0.5%..+1.8%], 4 of 5 positive |
| sql/select20.window | generated | hand | 7237.6 | 18242.2 | 2.52x | 18063.1 | 2.50x | -1.0% | 21416 | 21416 | 1 % | [-3.2%..+0.5%], 1 of 4 positive | +0.8% [-0.6%..+1.4%], 4 of 5 positive |
| tsql/insert-values.at | generated | scriptdom | 8078.4 | 1184.8 | 0.15x | 1194.8 | 0.15x | +0.8% | 1328 | 1328 | 1 % | [-3.1%..+4.3%], 3 of 4 positive |  |
| sql/select20.scan | generated | control | 377.8 | 375.9 | 1.00x | 381.0 | 1.01x | +1.3% | 0 | 0 | 0 % | [-1.0%..+5.1%], 1 of 4 positive | -0.4% [-20.8%..+4.8%], 2 of 5 positive |
| sql/conditions100.scan | generated | control | 3508.3 | 3505.6 | 1.00x | 3494.9 | 1.00x | -0.3% | 0 | 0 | 3 % | [-4.8%..+5.8%], 1 of 4 positive |  |
| tsql/select20.scan | generated | control | 376.4 | 387.7 | 1.03x | 381.0 | 1.01x | -1.7% | 0 | 0 | 5 % | [-3.9%..+2.3%], 1 of 4 positive | +1.0% [-3.4%..+1.2%], 3 of 5 positive |
| el/ladder.scan | generated | control | 136.8 | 136.7 | 1.00x | 136.4 | 1.00x | -0.2% | 0 | 0 | 3 % | [-0.7%..+0.2%], 1 of 4 positive | +0.2% [-1.0%..+1.4%], 4 of 5 positive |
| el/refused-early.bool | generated | hand | 334.3 | 1074.5 | 3.21x | 560.2 | 1.68x | -47.9% | 1064 | 624 | 11 % | [-50.2%..-46.5%], 0 of 4 positive |  |
| el/refused-late.bool | generated | hand | 1174.3 | 1738.5 | 1.48x | 910.6 | 0.78x | -47.6% | 1064 | 624 | 2 % | [-49.2%..-45.4%], 0 of 4 positive |  |
| el/ladder.bool | generated | hand | 1004.6 | 1752.0 | 1.74x | 1692.4 | 1.68x | -3.4% | 1776 | 1720 | 2 % | [-4.5%..-2.6%], 0 of 4 positive | -1.7% [-3.9%..+1.0%], 1 of 5 positive |
| sql/refused-late.bool | generated | hand | 2667.9 | 12249.5 | 4.59x | 5817.9 | 2.18x | -52.5% | 13552 | 6616 | 2 % | [-52.8%..-51.8%], 0 of 4 positive | -52.2% [-53.2%..-51.8%], 0 of 5 positive |
| sql/select20.bool | generated | hand | 7195.5 | 18063.6 | 2.51x | 18047.8 | 2.51x | -0.1% | 21448 | 21392 | 3 % | [-3.4%..+0.2%], 1 of 4 positive | 0.0% [-2.4%..+0.8%], 3 of 5 positive |
| el/floor | generated | hand | 317.5 | 799.1 | 2.52x | 807.1 | 2.54x | +1.0% | 1104 | 1104 | 7 % | [-0.9%..+3.3%], 2 of 4 positive |  |
| el/floor | immediate | hand | 317.5 | 469.0 | 1.48x | 472.1 | 1.49x | +0.7% | 1120 | 1120 | 7 % | [-4.4%..+1.6%], 2 of 4 positive |  |
| el/ladder | generated | hand | 1002.7 | 1760.4 | 1.76x | 1716.0 | 1.71x | -2.5% | 1776 | 1776 | 4 % | [-3.3%..-1.3%], 0 of 4 positive | -1.4% [-1.4%..+6.4%], 2 of 5 positive |
| el/ladder | immediate | hand | 1002.7 | 1168.8 | 1.17x | 1182.3 | 1.18x | +1.2% | 1784 | 1784 | 4 % | [-2.1%..+4.1%], 3 of 4 positive | -1.3% [-2.8%..+0.7%], 1 of 5 positive |
| el/nest7 | generated | hand | 642.8 | 1712.1 | 2.66x | 1685.6 | 2.62x | -1.5% | 1152 | 1152 | 8 % | [-2.0%..+0.0%], 1 of 4 positive |  |
| el/nest7 | immediate | hand | 642.8 | 1047.7 | 1.63x | 1051.4 | 1.64x | +0.4% | 1120 | 1120 | 8 % | [-0.2%..+0.8%], 2 of 4 positive |  |
| el/block | generated | hand | 1059.4 | 1838.6 | 1.74x | 1782.7 | 1.68x | -3.0% | 2344 | 2344 | 5 % | [-4.7%..-2.0%], 0 of 4 positive |  |
| el/block | immediate | hand | 1059.4 | 1170.1 | 1.10x | 1157.1 | 1.09x | -1.1% | 2440 | 2440 | 5 % | [-2.0%..+1.3%], 1 of 4 positive |  |
| el/try | generated | hand | 3215.6 | 6115.7 | 1.90x | 6153.0 | 1.91x | +0.6% | 5632 | 5632 | 4 % | [+0.3%..+1.3%], 4 of 4 positive |  |
| el/try | immediate | hand | 3215.6 | 4736.6 | 1.47x | 4693.4 | 1.46x | -0.9% | 5752 | 5752 | 4 % | [-2.1%..+0.3%], 1 of 4 positive |  |
| el/loop | generated | hand | 2216.0 | 3747.2 | 1.69x | 3714.2 | 1.68x | -0.9% | 4776 | 4776 | 3 % | [-2.5%..+2.0%], 2 of 4 positive |  |
| el/loop | immediate | hand | 2216.0 | 2567.8 | 1.16x | 2550.5 | 1.15x | -0.7% | 4880 | 4880 | 3 % | [-1.5%..-0.3%], 0 of 4 positive |  |
| el/terms100 | generated | hand | 11307.2 | 16172.2 | 1.43x | 16166.6 | 1.43x | 0.0% | 17904 | 17904 | 2 % | [-0.8%..+0.4%], 2 of 4 positive | +0.3% [-2.3%..+1.1%], 3 of 5 positive |
| el/terms100 | immediate | hand | 11307.2 | 12848.7 | 1.14x | 12694.3 | 1.12x | -1.2% | 18720 | 18720 | 2 % | [-1.4%..+2.6%], 1 of 4 positive | +0.4% [-0.4%..+2.5%], 3 of 5 positive |
| el/terms1000 | generated | hand | 109605.7 | 153777.3 | 1.40x | 153707.9 | 1.40x | 0.0% | 169105 | 169117 | 4 % | [-0.8%..+1.9%], 2 of 4 positive | +0.1% [-1.5%..+1.0%], 2 of 5 positive |
| el/terms1000 | immediate | hand | 109605.7 | 123901.4 | 1.13x | 122637.7 | 1.12x | -1.0% | 177120 | 177120 | 4 % | [-1.5%..+1.8%], 1 of 4 positive | +1.3% [-0.5%..+1.5%], 3 of 5 positive |
| el/overloads | generated | hand | 1873.9 | 2609.1 | 1.39x | 2632.6 | 1.40x | +0.9% | 4248 | 4248 | 3 % | [-1.8%..+3.4%], 3 of 4 positive |  |
| el/overloads | immediate | hand | 1873.9 | 2059.0 | 1.10x | 2073.0 | 1.11x | +0.7% | 4200 | 4200 | 3 % | [+0.4%..+3.0%], 4 of 4 positive |  |
| el/string | generated | hand | 286.8 | 946.4 | 3.30x | 946.5 | 3.30x | 0.0% | 1056 | 1056 | 6 % | [-0.5%..+3.1%], 2 of 4 positive |  |
| el/string | immediate | hand | 286.8 | 666.0 | 2.32x | 665.1 | 2.32x | -0.1% | 1032 | 1032 | 6 % | [-2.3%..+2.9%], 3 of 4 positive |  |
| el/interpolation | generated | hand | 2289.9 | 4875.4 | 2.13x | 4878.9 | 2.13x | +0.1% | 2368 | 2368 | 16 % | [-7.7%..+1.0%], 1 of 4 positive |  |
| el/interpolation | immediate | hand | 2289.9 | 4412.7 | 1.93x | 4334.7 | 1.89x | -1.8% | 2424 | 2424 | 16 % | [-9.1%..+1.9%], 1 of 4 positive |  |
| el/untyped | generated | hand | 27014.0 | 28590.0 | 1.06x | 28563.5 | 1.06x | -0.1% | 16424 | 16424 | 12 % | [-0.6%..+0.7%], 3 of 4 positive |  |
| el/refused-early | generated | hand | 475.4 | 1228.1 | 2.58x | 1285.5 | 2.70x | +4.7% | 1064 | 1064 | 10 % | [+0.7%..+6.2%], 4 of 4 positive |  |
| el/refused-early | immediate | hand | 475.4 | 1268.6 | 2.67x | 1289.5 | 2.71x | +1.6% | 1944 | 1944 | 10 % | [-1.1%..+3.0%], 3 of 4 positive |  |
| el/refused-late | generated | hand | 1471.7 | 1929.0 | 1.31x | 1977.8 | 1.34x | +2.5% | 1064 | 1064 | 3 % | [+0.7%..+4.0%], 4 of 4 positive |  |
| el/refused-late | immediate | hand | 1471.7 | 3181.3 | 2.16x | 3250.5 | 2.21x | +2.2% | 3928 | 3928 | 3 % | [-0.1%..+5.7%], 3 of 4 positive |  |
| sql/literal | generated | hand | 66.4 | 154.1 | 2.32x | 151.5 | 2.28x | -1.7% | 160 | 160 | 3 % | [-8.5%..+6.4%], 2 of 4 positive |  |
| sql/comment | generated | hand | 2917.1 | 5355.5 | 1.84x | 5305.7 | 1.82x | -0.9% | 5136 | 5136 | 9 % | [-2.0%..+0.3%], 2 of 4 positive |  |
| sql/conditions100 | generated | hand | 74118.0 | 136981.2 | 1.85x | 138153.4 | 1.86x | +0.9% | 161736 | 161736 | 13 % | [+0.1%..+1.9%], 4 of 4 positive |  |
| sql/conditions1000 | generated | hand | 737437.5 | 1411032.0 | 1.91x | 1423425.8 | 1.93x | +0.9% | 1616136 | 1616136 | 24 % | [+0.8%..+2.9%], 4 of 4 positive |  |
| tsql/comment | generated | scriptdom | 26076.0 | 1573.5 | 0.06x | 1573.5 | 0.06x | 0.0% | 1192 | 1192 | 9 % | [-0.9%..+7.3%], 2 of 4 positive |  |
| sql/column | generated | hand | 191.3 | 399.5 | 2.09x | 397.8 | 2.08x | -0.4% | 392 | 392 | 8 % | [-1.8%..+2.7%], 3 of 4 positive |  |
| sql/arithmetic | generated | hand | 2173.5 | 4576.1 | 2.11x | 4572.7 | 2.10x | -0.1% | 3472 | 3472 | 7 % | [-3.7%..+4.3%], 2 of 4 positive |  |
| sql/nest8 | generated | hand | 3513.4 | 12097.6 | 3.44x | 12760.2 | 3.63x | +5.5% | 6504 | 6504 | 8 % | [-0.9%..+9.9%], 2 of 4 positive |  |
| sql/condition | generated | hand | 2563.5 | 4869.1 | 1.90x | 4957.1 | 1.93x | +1.8% | 4928 | 4928 | 8 % | [-0.3%..+5.9%], 3 of 4 positive |  |
| sql/select1 | generated | hand | 874.1 | 1713.5 | 1.96x | 1696.7 | 1.94x | -1.0% | 1688 | 1688 | 12 % | [-3.8%..+2.5%], 2 of 4 positive |  |
| sql/select20 | generated | hand | 10409.9 | 21360.3 | 2.05x | 21194.9 | 2.04x | -0.8% | 21448 | 21448 | 8 % | [-2.1%..+1.0%], 3 of 4 positive | +0.2% [-1.7%..+1.9%], 4 of 5 positive |
| sql/values | generated | hand | 671.8 | 1965.6 | 2.93x | 1917.6 | 2.85x | -2.4% | 1904 | 1904 | 12 % | [-2.9%..+2.4%], 2 of 4 positive |  |
| sql/create | generated | hand | 975.8 | 2861.8 | 2.93x | 2792.9 | 2.86x | -2.4% | 1568 | 1568 | 9 % | [-3.8%..-0.3%], 0 of 4 positive |  |
| sql/refused-late | generated | hand | 3757.6 | 14003.5 | 3.73x | 13913.4 | 3.70x | -0.6% | 13552 | 13552 | 8 % | [-2.1%..+0.3%], 1 of 4 positive | +0.9% [-0.3%..+1.4%], 3 of 5 positive |
