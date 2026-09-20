Median of 5 of 5 runs, each in a process of its own; control 31.7 ns (the runs' controls: 31.9, 31.9, 31.7, 31.4, 31.0).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 04:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| tsql/script100.bool | generated | scriptdom | 914462.5 | 124681.2 | 0.14x | 130784.4 | 0.14x | +4.9% | 113600 | 105600 | 37 % | [-0.8%..+19.1%], 3 of 5 positive | -1.0% [-3.3%..-0.1%], 0 of 5 positive |
| tsql/script100.boolboth | generated | scriptdom | 663911.7 | 122821.1 | 0.18x | 124943.8 | 0.19x | +1.7% | 105600 | 105600 | 2 % | [-0.7%..+2.9%], 4 of 5 positive | +2.0% [-0.3%..+3.5%], 4 of 5 positive |
| tsql/script100 | generated | scriptdom | 664188.3 | 124288.3 | 0.19x | 124364.8 | 0.19x | +0.1% | 113600 | 113600 | 3 % | [-1.3%..+2.4%], 2 of 5 positive | +0.8% [-3.2%..+3.2%], 3 of 5 positive |
| tsql/script400.bool | generated | scriptdom | 2660881.2 | 503890.6 | 0.19x | 506006.2 | 0.19x | +0.4% | 454400 | 422403 | 2 % | [-2.4%..+1.3%], 2 of 5 positive | -0.1% [-2.4%..+3.4%], 2 of 5 positive |
| tsql/script400.boolboth | generated | scriptdom | 2687175.0 | 492150.0 | 0.18x | 501865.6 | 0.19x | +2.0% | 422403 | 422403 | 5 % | [-2.6%..+2.5%], 4 of 5 positive | +0.2% [-0.5%..+4.4%], 4 of 5 positive |
| tsql/script400 | generated | scriptdom | 2674431.2 | 502606.2 | 0.19x | 506443.8 | 0.19x | +0.8% | 454400 | 454400 | 5 % | [-1.7%..+1.3%], 4 of 5 positive | +0.9% [-0.6%..+21.2%], 4 of 5 positive |
| tsql/columns1000 | generated | scriptdom | 1731717.2 | 187734.4 | 0.11x | 188400.0 | 0.11x | +0.4% | 200464 | 200464 | 40 % | [-10.2%..+2.4%], 3 of 5 positive | 0.0% [-3.4%..+0.7%], 2 of 5 positive |
| tsql/conditions1000 | generated | scriptdom | 896871.1 | 341432.0 | 0.38x | 341979.7 | 0.38x | +0.2% | 424424 | 424424 | 43 % | [-0.8%..+2.0%], 3 of 5 positive | +1.0% [-5.0%..+1.1%], 3 of 5 positive |
| tsql/rows1000 | generated | scriptdom | 590458.6 | 242871.1 | 0.41x | 245616.4 | 0.42x | +1.1% | 296472 | 296472 | 72 % | [+0.7%..+10.6%], 5 of 5 positive | +0.5% [-1.3%..+0.8%], 3 of 5 positive |
| sql/select20.at | generated | hand | 7196.1 | 17718.8 | 2.46x | 18021.2 | 2.50x | +1.7% | 21416 | 21416 | 66 % | [-5.2%..+3.0%], 4 of 5 positive | -1.0% [-1.6%..+2.2%], 2 of 5 positive |
| sql/select20.window | generated | hand | 7353.8 | 18646.0 | 2.54x | 18443.9 | 2.51x | -1.1% | 21416 | 21416 | 22 % | [-3.1%..+2.4%], 3 of 5 positive | +1.2% [-1.4%..+2.8%], 3 of 5 positive |
| tsql/insert-values.at | generated | scriptdom | 8324.0 | 1210.6 | 0.15x | 1237.8 | 0.15x | +2.2% | 1328 | 1328 | 74 % | [+0.4%..+4.4%], 5 of 5 positive | +1.4% [-2.4%..+5.1%], 3 of 5 positive |
| sql/select20.scan | generated | control | 384.1 | 389.0 | 1.01x | 389.1 | 1.01x | 0.0% | 0 | 0 | 50 % | [-0.7%..+1.9%], 2 of 5 positive | +0.4% [-0.9%..+1.2%], 3 of 5 positive |
| sql/conditions100.scan | generated | control | 3552.2 | 3546.9 | 1.00x | 3565.7 | 1.00x | +0.5% | 0 | 0 | 72 % | [-4.0%..+1.8%], 2 of 5 positive | +0.7% [-0.8%..+1.5%], 2 of 5 positive |
| tsql/select20.scan | generated | control | 385.4 | 385.8 | 1.00x | 388.3 | 1.01x | +0.6% | 0 | 0 | 66 % | [+0.0%..+2.8%], 4 of 5 positive | 0.0% [-2.8%..+0.4%], 3 of 5 positive |
| el/ladder.scan | generated | control | 142.8 | 140.6 | 0.98x | 141.2 | 0.99x | +0.4% | 0 | 0 | 88 % | [-2.5%..+2.8%], 1 of 5 positive | +1.5% [-1.4%..+4.0%], 3 of 5 positive |
| el/refused-early.bool | generated | hand | 343.4 | 1123.4 | 3.27x | 570.0 | 1.66x | -49.3% | 1064 | 624 | 79 % | [-49.4%..-47.6%], 0 of 5 positive | -50.2% [-50.9%..-47.4%], 0 of 5 positive |
| el/refused-late.bool | generated | hand | 1227.6 | 1793.0 | 1.46x | 925.4 | 0.75x | -48.4% | 1064 | 624 | 79 % | [-49.6%..-47.0%], 0 of 5 positive | -50.0% [-50.5%..-47.9%], 0 of 5 positive |
| el/ladder.bool | generated | hand | 1032.2 | 1797.1 | 1.74x | 1771.9 | 1.72x | -1.4% | 1776 | 1720 | 21 % | [-8.7%..-0.4%], 0 of 5 positive | -3.9% [-8.1%..-0.7%], 0 of 5 positive |
| sql/refused-late.bool | generated | hand | 2792.9 | 12581.0 | 4.50x | 6105.8 | 2.19x | -51.5% | 13552 | 6616 | 12 % | [-52.1%..-50.9%], 0 of 5 positive | -52.3% [-52.5%..-50.5%], 0 of 5 positive |
| sql/select20.bool | generated | hand | 7330.8 | 18444.2 | 2.52x | 18512.3 | 2.53x | +0.4% | 21448 | 21392 | 4 % | [-0.3%..+4.8%], 4 of 5 positive | +1.2% [-4.2%..+2.5%], 2 of 5 positive |
| el/floor | generated | hand | 323.6 | 805.0 | 2.49x | 826.1 | 2.55x | +2.6% | 1104 | 1104 | 6 % | [+0.4%..+3.8%], 5 of 5 positive | +7.2% [-0.6%..+7.2%], 3 of 5 positive |
| el/floor | immediate | hand | 323.6 | 487.6 | 1.51x | 487.4 | 1.51x | 0.0% | 1120 | 1120 | 6 % | [-3.3%..+1.3%], 2 of 5 positive | -2.2% [-6.2%..+0.8%], 2 of 5 positive |
| el/ladder | generated | hand | 1039.2 | 1791.5 | 1.72x | 1767.9 | 1.70x | -1.3% | 1776 | 1776 | 5 % | [-3.4%..+2.1%], 1 of 5 positive | -1.7% [-1.7%..+2.5%], 3 of 5 positive |
| el/ladder | immediate | hand | 1039.2 | 1181.5 | 1.14x | 1194.5 | 1.15x | +1.1% | 1784 | 1784 | 5 % | [-4.3%..+1.7%], 4 of 5 positive | 0.0% [-0.4%..+6.8%], 2 of 5 positive |
| el/nest7 | generated | hand | 661.6 | 1740.9 | 2.63x | 1741.3 | 2.63x | 0.0% | 1152 | 1152 | 14 % | [-2.0%..+1.1%], 2 of 5 positive | -0.8% [-4.9%..+6.2%], 2 of 5 positive |
| el/nest7 | immediate | hand | 661.6 | 1050.4 | 1.59x | 1054.1 | 1.59x | +0.3% | 1120 | 1120 | 14 % | [-1.1%..+1.2%], 3 of 5 positive | +0.4% [+0.4%..+9.0%], 5 of 5 positive |
| el/block | generated | hand | 1086.6 | 1861.5 | 1.71x | 1865.1 | 1.72x | +0.2% | 2344 | 2344 | 51 % | [-2.0%..+2.4%], 3 of 5 positive | -0.8% [-2.4%..+4.1%], 3 of 5 positive |
| el/block | immediate | hand | 1086.6 | 1192.1 | 1.10x | 1203.3 | 1.11x | +0.9% | 2440 | 2440 | 51 % | [-1.5%..+10.9%], 4 of 5 positive | +1.4% [-0.8%..+2.4%], 4 of 5 positive |
| el/try | generated | hand | 3311.4 | 4717.8 | 1.42x | 4766.5 | 1.44x | +1.0% | 4456 | 4456 | 66 % | [-4.4%..+3.0%], 2 of 5 positive | +0.7% [+0.3%..+1.9%], 5 of 5 positive |
| el/try | immediate | hand | 3311.4 | 3272.5 | 0.99x | 3402.8 | 1.03x | +4.0% | 4152 | 4152 | 66 % | [+0.0%..+14.1%], 5 of 5 positive | +1.7% [-2.1%..+3.1%], 4 of 5 positive |
| el/loop | generated | hand | 2252.8 | 3775.1 | 1.68x | 3784.3 | 1.68x | +0.2% | 4776 | 4776 | 40 % | [-0.3%..+1.4%], 3 of 5 positive | +0.1% [-8.5%..+1.2%], 2 of 5 positive |
| el/loop | immediate | hand | 2252.8 | 2564.6 | 1.14x | 2594.2 | 1.15x | +1.2% | 4880 | 4880 | 40 % | [-1.4%..+32.1%], 3 of 5 positive | +0.3% [-2.1%..+4.5%], 3 of 5 positive |
| el/terms100 | generated | hand | 11700.3 | 16256.8 | 1.39x | 16494.0 | 1.41x | +1.5% | 17904 | 17904 | 27 % | [-5.5%..+1.5%], 2 of 5 positive | -1.7% [-1.8%..+1.8%], 4 of 5 positive |
| el/terms100 | immediate | hand | 11700.3 | 13135.3 | 1.12x | 13018.6 | 1.11x | -0.9% | 18720 | 18720 | 27 % | [-1.7%..+2.8%], 2 of 5 positive | +0.8% [+0.8%..+2.9%], 5 of 5 positive |
| el/terms1000 | generated | hand | 112990.2 | 157441.2 | 1.39x | 155898.4 | 1.38x | -1.0% | 169104 | 169104 | 42 % | [-2.0%..+6.2%], 3 of 5 positive | -1.0% [-2.0%..+2.7%], 2 of 5 positive |
| el/terms1000 | immediate | hand | 112990.2 | 126682.4 | 1.12x | 125474.4 | 1.11x | -1.0% | 177120 | 177120 | 42 % | [-13.7%..+3.1%], 1 of 5 positive | +0.3% [+0.0%..+5.7%], 4 of 5 positive |
| el/overloads | generated | hand | 1914.4 | 2686.9 | 1.40x | 2632.2 | 1.37x | -2.0% | 4248 | 4248 | 74 % | [-2.9%..+1.9%], 3 of 5 positive | 0.0% [-2.4%..+4.8%], 3 of 5 positive |
| el/overloads | immediate | hand | 1914.4 | 2112.5 | 1.10x | 2076.3 | 1.08x | -1.7% | 4200 | 4200 | 74 % | [-2.5%..+1.2%], 1 of 5 positive | -0.3% [-4.4%..+2.2%], 2 of 5 positive |
| el/string | generated | hand | 294.0 | 996.6 | 3.39x | 987.7 | 3.36x | -0.9% | 1056 | 1056 | 24 % | [-1.7%..+2.4%], 3 of 5 positive | +2.5% [+0.5%..+5.3%], 5 of 5 positive |
| el/string | immediate | hand | 294.0 | 681.4 | 2.32x | 685.1 | 2.33x | +0.5% | 1032 | 1032 | 24 % | [-0.8%..+15.3%], 4 of 5 positive | +1.4% [-0.8%..+2.4%], 3 of 5 positive |
| el/interpolation | generated | hand | 2476.2 | 4743.1 | 1.92x | 5321.9 | 2.15x | +12.2% | 2368 | 2368 | 26 % | [-5.9%..+15.3%], 3 of 5 positive | +0.9% [-2.5%..+7.6%], 2 of 5 positive |
| el/interpolation | immediate | hand | 2476.2 | 4486.1 | 1.81x | 4479.5 | 1.81x | -0.1% | 2424 | 2424 | 26 % | [-18.7%..+3.8%], 1 of 5 positive | -9.7% [-11.8%..+11.2%], 2 of 5 positive |
| el/untyped | generated | hand | 19631.4 | 21081.1 | 1.07x | 20509.4 | 1.04x | -2.7% | 16424 | 16424 | 46 % | [-2.7%..+0.5%], 1 of 5 positive | +0.1% [-0.6%..+1.0%], 3 of 5 positive |
| el/refused-early | generated | hand | 484.3 | 1252.7 | 2.59x | 1289.5 | 2.66x | +2.9% | 1064 | 1064 | 28 % | [-4.1%..+5.3%], 4 of 5 positive | +5.9% [-0.6%..+9.0%], 3 of 5 positive |
| el/refused-early | immediate | hand | 484.3 | 1259.4 | 2.60x | 1280.0 | 2.64x | +1.6% | 1944 | 1944 | 28 % | [-1.0%..+13.0%], 3 of 5 positive | -0.6% [-2.1%..+0.7%], 2 of 5 positive |
| el/refused-late | generated | hand | 1393.9 | 1950.4 | 1.40x | 1978.2 | 1.42x | +1.4% | 1064 | 1064 | 60 % | [-0.3%..+4.8%], 4 of 5 positive | +2.6% [-2.2%..+5.5%], 4 of 5 positive |
| el/refused-late | immediate | hand | 1393.9 | 3355.9 | 2.41x | 3262.2 | 2.34x | -2.8% | 3928 | 3928 | 60 % | [-2.8%..+2.3%], 1 of 5 positive | +2.0% [-0.3%..+8.2%], 4 of 5 positive |
| sql/literal | generated | hand | 63.3 | 153.4 | 2.42x | 147.6 | 2.33x | -3.8% | 160 | 160 | 103 % | [-10.2%..+7.6%], 3 of 5 positive | +3.1% [-4.5%..+8.1%], 4 of 5 positive |
| sql/comment | generated | hand | 2909.4 | 5338.0 | 1.83x | 5511.2 | 1.89x | +3.2% | 5136 | 5136 | 73 % | [-2.5%..+4.5%], 3 of 5 positive | -0.7% [-0.9%..+3.1%], 2 of 5 positive |
| sql/conditions100 | generated | hand | 72180.9 | 138374.1 | 1.92x | 141762.7 | 1.96x | +2.4% | 161736 | 161736 | 76 % | [-2.6%..+11.2%], 4 of 5 positive | -1.3% [-1.3%..+4.8%], 4 of 5 positive |
| sql/conditions1000 | generated | hand | 741152.3 | 1417428.9 | 1.91x | 1448953.9 | 1.96x | +2.2% | 1616136 | 1616136 | 97 % | [-2.2%..+2.8%], 4 of 5 positive | -0.5% [-1.3%..+1.1%], 1 of 5 positive |
| tsql/comment | generated | scriptdom | 26600.3 | 1580.2 | 0.06x | 1584.6 | 0.06x | +0.3% | 1192 | 1192 | 80 % | [-1.3%..+1.0%], 2 of 5 positive | +1.4% [-0.8%..+2.7%], 4 of 5 positive |
| sql/column | generated | hand | 200.6 | 408.7 | 2.04x | 406.7 | 2.03x | -0.5% | 392 | 392 | 68 % | [-2.5%..-0.5%], 0 of 5 positive | -0.1% [-9.0%..+7.3%], 3 of 5 positive |
| sql/arithmetic | generated | hand | 2079.2 | 4535.3 | 2.18x | 4561.4 | 2.19x | +0.6% | 3472 | 3472 | 77 % | [-1.7%..+4.9%], 3 of 5 positive | -0.8% [-0.8%..+1.5%], 2 of 5 positive |
| sql/nest8 | generated | hand | 3595.2 | 12557.2 | 3.49x | 12726.3 | 3.54x | +1.3% | 6504 | 6504 | 54 % | [-8.5%..+4.6%], 4 of 5 positive | +7.1% [-0.3%..+9.8%], 4 of 5 positive |
| sql/condition | generated | hand | 2528.6 | 4893.7 | 1.94x | 5038.8 | 1.99x | +3.0% | 4928 | 4928 | 92 % | [-3.5%..+3.0%], 2 of 5 positive | +0.9% [-1.3%..+2.5%], 3 of 5 positive |
| sql/select1 | generated | hand | 865.7 | 1720.0 | 1.99x | 1744.1 | 2.01x | +1.4% | 1688 | 1688 | 84 % | [-0.1%..+3.6%], 4 of 5 positive | -0.5% [-1.1%..+6.7%], 2 of 5 positive |
| sql/select20 | generated | hand | 10173.3 | 21035.5 | 2.07x | 21709.0 | 2.13x | +3.2% | 21448 | 21448 | 86 % | [+0.3%..+3.2%], 5 of 5 positive | -0.1% [-2.1%..+2.7%], 3 of 5 positive |
| sql/values | generated | hand | 642.3 | 1899.3 | 2.96x | 1943.5 | 3.03x | +2.3% | 1904 | 1904 | 57 % | [-6.7%..+3.8%], 3 of 5 positive | -3.3% [-3.4%..+2.8%], 2 of 5 positive |
| sql/create | generated | hand | 872.1 | 2693.3 | 3.09x | 2668.7 | 3.06x | -0.9% | 1568 | 1568 | 79 % | [-5.4%..+1.2%], 1 of 5 positive | -0.6% [-5.3%..+0.8%], 1 of 5 positive |
| sql/refused-late | generated | hand | 3067.2 | 12869.1 | 4.20x | 13049.2 | 4.25x | +1.4% | 13552 | 13552 | 69 % | [+0.5%..+11.3%], 5 of 5 positive | -0.4% [-0.4%..+2.3%], 3 of 5 positive |
